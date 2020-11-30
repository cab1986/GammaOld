using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Gamma.Controllers;
using Gamma.Interfaces;
using Gamma.Models;
using System.Data.Entity;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Entities;
using DevExpress.Xpf.Grid;
using Gamma.DialogViewModels;
using System.ComponentModel;

namespace Gamma.ViewModels
{
    public class Complectation_Item
    {
        public Guid NomenclatureID { get; set; }
        public Guid OldCharacteristicId { get; set; }
        public Guid NewCharacteristicId { get; set; }
        public Guid? QualityId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class DocComplectationViewModel : SaveImplementedViewModel, IBarImplemented
	{
		#region Fields

		private readonly DocumentController documentController = new DocumentController();
		private readonly ProductController productController = new ProductController();

		private string barcode;
		private string _lastCreatedPalletNumber;

		private int placeId;

        private Guid? placeZoneID;

        #endregion

        #region Constructor

        public DocComplectationViewModel(Guid docId)
		{
			DocId = docId;
            using (var context = DB.GammaDb)
            {
                var docComplectation = context.DocComplectation.Include(d => d.C1CDocComplectation)
                    .Include(d => d.DocProduction.Select(dp => dp.DocProductionProducts))
                    .Include(d => d.DocWithdrawal.Select(dw => dw.DocWithdrawalProducts))
                    .FirstOrDefault(d => d.DocComplectationID == docId);
                if (docComplectation == null)
                {
                    MessageBox.Show("Произошла ошибка, документа нет в базе", "Ошибка документа", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    CloseWindow();
                    return;
                }
                
                placeId = (context.Places.FirstOrDefault(p => docComplectation.C1CDocComplectation.C1CWarehouseID != null && p.C1CPlaceID == docComplectation.C1CDocComplectation.C1CWarehouseID)?.PlaceID
                    ?? context.PlaceZones.FirstOrDefault(p => p.Name == "Перепаллетировка" && p.PlaceID == WorkSession.PlaceID)?.PlaceID)
                     ?? (WorkSession.BranchID == 1 ? 8 : (WorkSession.BranchID == 2 ? 28 : WorkSession.PlaceID));
                placeZoneID = context.PlaceZones.FirstOrDefault(p => p.Name == "Перепаллетировка" && p.PlaceID == WorkSession.PlaceID)?.PlaceZoneID
                     ?? context.PlaceZones.FirstOrDefault(p => p.Name == "Перепаллетировка" && ((p.Places.BranchID == 1 && p.PlaceID == 8) || (p.Places.BranchID == 2 && p.PlaceID == 28)))?.PlaceZoneID;
                DocDate = (DateTime)(docComplectation.C1CDocComplectation.Date ?? DB.CurrentDateTime);
                Number = docComplectation.C1CDocComplectation.C1CCode;
                var complectation_Items = context.C1CDocComplectationNomenclature
                    .Where(d => d.C1CDocComplectationID == docComplectation.C1CDocComplectationID)
                    .Select(d => new Complectation_Item()
                    {
                        NomenclatureID = d.C1CNomenclatureID,
                        OldCharacteristicId = d.C1COldCharacteristicID,
                        NewCharacteristicId = d.C1CNewCharacteristicID,
                        QualityId = d.C1CQualityID,
                        Quantity = d.Quantity ?? 0
                    })
                    .ToList();
                if (!(complectation_Items == null || complectation_Items.Count == 0))
                {
                    foreach (var citem in complectation_Items)
                    {
                        var item = new ComplectationItem(citem.NomenclatureID,
                                                citem.OldCharacteristicId, citem.NewCharacteristicId,
                                                citem.QualityId, citem.Quantity);
                        foreach (var product in docComplectation
                            .DocProduction
                            .SelectMany(dp => dp.DocProductionProducts)
                            .Where(dp => dp.Products.ProductItems.FirstOrDefault().C1CNomenclatureID == item.NomenclatureID
                                && dp.Products.ProductItems.FirstOrDefault().C1CCharacteristicID == item.NewCharacteristicId))
                        {
                            item.PackedPallets.Add(new ComplectationProduct
                            {
                                DocId = product.DocID,
                                ProductId = product.ProductID,
                                Quantity = product.Quantity ?? 0,
                                Number = product.Products.Number,
                                Barcode = product.Products.BarCode
                            });
                        }
                        foreach (var product in docComplectation
                            .DocWithdrawal
                            .SelectMany(dw => dw.DocWithdrawalProducts)
                            .Where(dw => dw.Products.ProductItems.FirstOrDefault().C1CNomenclatureID == item.NomenclatureID
                                        && dw.Products.ProductItems.FirstOrDefault().C1CCharacteristicID == item.OldCharacteristicId))
                        {
                            item.UnpackedPallets.Add(new ComplectationProduct
                            {
                                DocId = product.DocID,
                                ProductId = product.ProductID,
                                Quantity = product.Quantity ?? 0,
                                Number = product.Products.Number,
                                Barcode = product.Products.BarCode
                            });
                        }
                        ComplectationItems.Add(item);

                        var palletRs = docComplectation
                            .DocProduction
                            .SelectMany(dp => dp.DocProductionProducts)
                            .Where(dp => dp.Products.ProductPallets.ProductItems.FirstOrDefault().C1CNomenclatureID == item.NomenclatureID
                                && dp.Products.ProductPallets.ProductItems.FirstOrDefault().C1CCharacteristicID == item.OldCharacteristicId);
                        if (palletRs?.Count() > 0)
                        {
                            var itemPalletR = new ComplectationItem(citem.NomenclatureID,
                                                citem.OldCharacteristicId, citem.OldCharacteristicId,
                                                citem.QualityId, 0);
                            foreach (var product in palletRs)
                            {
                                itemPalletR.PackedPallets.Add(new ComplectationProduct
                                {
                                    DocId = product.DocID,
                                    ProductId = product.ProductID,
                                    Quantity = product.Quantity ?? 0,
                                    Number = product.Products.Number,
                                    Barcode = product.Products.BarCode
                                });
                            }
                            ComplectationItems.Add(itemPalletR);
                        }

                    }
                }
                else
                {
                    var docWithdrawalProduct = docComplectation.DocWithdrawal.SelectMany(d => d.DocWithdrawalProducts).FirstOrDefault();
                    if (docWithdrawalProduct != null)
                    {
                        var productItem = docWithdrawalProduct.Products.ProductItems.FirstOrDefault();
                        if (productItem != null)
                            complectation_Items.Add(new Complectation_Item() { NomenclatureID = productItem.C1CNomenclatureID, OldCharacteristicId = (Guid)productItem.C1CCharacteristicID, NewCharacteristicId = (Guid)productItem.C1CCharacteristicID, QualityId = new Guid("D05404A0-6BCE-449B-A798-41EBE5E5B977"), Quantity = 1 });

                        foreach (var citem in complectation_Items)
                        {
                            var item = new ComplectationItem(citem.NomenclatureID,
                                                    citem.OldCharacteristicId, citem.NewCharacteristicId,
                                                    citem.QualityId, citem.Quantity);
                            foreach (var product in docComplectation
                                .DocProduction
                                .SelectMany(dp => dp.DocProductionProducts)
                                .Where(dp => dp.Products.ProductItems.FirstOrDefault().C1CNomenclatureID == item.NomenclatureID
                                    && dp.Products.ProductItems.FirstOrDefault().C1CCharacteristicID == item.NewCharacteristicId))
                            {
                                item.PackedPallets.Add(new ComplectationProduct
                                {
                                    DocId = product.DocID,
                                    ProductId = product.ProductID,
                                    Quantity = product.Quantity ?? 0,
                                    Number = product.Products.Number,
                                    Barcode = product.Products.BarCode
                                });
                            }
                            foreach (var product in docComplectation
                                .DocWithdrawal
                                .SelectMany(dw => dw.DocWithdrawalProducts)
                                .Where(dw => dw.Products.ProductItems.FirstOrDefault().C1CNomenclatureID == item.NomenclatureID
                                            && dw.Products.ProductItems.FirstOrDefault().C1CCharacteristicID == item.OldCharacteristicId))
                            {
                                item.UnpackedPallets.Add(new ComplectationProduct
                                {
                                    DocId = product.DocID,
                                    ProductId = product.ProductID,
                                    Quantity = product.Quantity ?? 0,
                                    Number = product.Products.Number,
                                    Barcode = product.Products.BarCode
                                });
                            }
                            ComplectationItems.Add(item);
                        }
                    }
                }
            }
            Bars.Add(ReportManager.GetReportBar("DocComplectation", VMID));
            Messenger.Default.Register<BarcodeMessage>(this, BarcodeReceived);
            UnpackCommand = new DelegateCommand(Unpack, () => !string.IsNullOrEmpty(Barcode));
            CreatePalletCommand = new DelegateCommand<ComplectationItem>(CreateNewPallet);
            CreatePalletRCommand = new DelegateCommand<ComplectationItem>(CreateNewPalletR);
            OpenPackedPalletItemCommand = new DelegateCommand(() => OpenProduct(SelectedPackedPalletItem), () => SelectedPackedPalletItem != null);
            OpenUnpackedPalletItemCommand = new DelegateCommand(() => OpenProduct(SelectedUnpackedPalletItem), () => SelectedUnpackedPalletItem != null);
        }

        #endregion

        #region Private Methods

        private void BarcodeReceived(BarcodeMessage msg)
		{
			var bcode = msg.Barcode;
			var pallet = ComplectationItems
				.SelectMany(i => i.PackedPallets)
				.FirstOrDefault(p => p.Barcode.Equals(bcode));
			if (pallet != null)
			{
				var dlgResult = MessageBox.Show($"Вы хотите удалить c штрих-кодом {bcode}?", "Удаление паллеты", MessageBoxButton.YesNo,
					MessageBoxImage.Question);
				if (dlgResult == MessageBoxResult.Yes)
				{
					using (var context = DB.GammaDb)
					{
						var result = context.DeletePallet(pallet.ProductId).FirstOrDefault();
						if (!string.IsNullOrEmpty(result))
						{
							MessageBox.Show(result);
						}
					}
				}
				return;
			}
			Barcode = bcode;
			Unpack();
		}

		private void Unpack()
		{
			if (string.IsNullOrEmpty(Barcode))
			{
				return;
			}
			UIServices.SetBusyState();
			var products = ComplectationItems.SelectMany(c => c.UnpackedPallets)
				.Where(p => p.Number.Contains(Barcode) || p.Barcode.Contains(Barcode)).ToList();
			if (products.Count > 1)
			{
				MessageBox.Show("Уточните номер или штрихкод, найдено более одного продукта");
				return;
			}
			if (products.Any())
			{
				DeleteFromUnpack(products.First());
				return;
			}
            using (var context = DB.GammaDb)
			{
				vProductsInfo pallet;
				try
				{
					pallet = context.vProductsInfo.SingleOrDefault(p => (p.BarCode.Contains(Barcode) || p.Number.Contains(Barcode)) && (p.ProductKindID == (int)ProductKind.ProductPallet || p.ProductKindID == (int)ProductKind.ProductPalletR));
				}
				catch
				{
					MessageBox.Show("Уточните номер или штрихкод, найдено более одного продукта");
					return;
				}
				if (pallet == null)
				{
					MessageBox.Show("Паллета с таким номером или штрихкодом не найдена");
					return;
				}
				if (pallet.IsWrittenOff ?? false)
				{
					MessageBox.Show("Паллета уже списана");
					return;
				}


                if (!(ComplectationItems?.Count() > 0))
                {
                    var complectationItems = new List<ComplectationItem>(ComplectationItems);
                    var itemAdd = new ComplectationItem(pallet.C1CNomenclatureID,
                        (Guid)pallet.C1CCharacteristicID, (Guid)pallet.C1CCharacteristicID,
                        pallet.C1CQualityID, pallet.Quantity ?? 0);
                    complectationItems.Add(itemAdd);
                    ComplectationItems = complectationItems;
                }

                var item = ComplectationItems.FirstOrDefault(i => i.NomenclatureID.Equals(pallet.C1CNomenclatureID) &&
																i.OldCharacteristicId.Equals(pallet.C1CCharacteristicID));
				if (item == null)
				{
					MessageBox.Show("Номенклатура найденной паллеты не совпадает с документом");
					return;
				}
				if (item.QualityId != null && pallet.C1CQualityID != null && item.QualityId != pallet.C1CQualityID)
				{
					MessageBox.Show("Качество найденной паллеты не совпадает с документом");
					return;
				}
				var docWithdrawalId = SqlGuidUtil.NewSequentialid();
				if (!documentController.WithdrawProduct(pallet.ProductID, docWithdrawalId, true))
				{
					MessageBox.Show("Не удалось списать паллету. Ошибка в базе.");
					return;
				}
				context.DocComplectation.First(d => d.DocComplectationID == DocId).DocWithdrawal.Add(context.DocWithdrawal.First(d => d.DocID == docWithdrawalId));
				context.SaveChanges();
				item.UnpackedPallets.Add(new ComplectationProduct
				{
					DocId = docWithdrawalId,
					ProductId = pallet.ProductID,
					Number = pallet.Number,
					Barcode = pallet.BarCode,
					Quantity = pallet.Quantity ?? 0
				});
			}

            RaisePropertyChanged(() => ComplectationItems);
        }

		/// <summary>
		/// Cancel unpack operation of product
		/// </summary>
		/// <param name="product"></param>
		private void DeleteFromUnpack(ComplectationProduct product)
		{
			if (product == null)
			{
				return;
			}
			var dlgResult =
				MessageBox.Show("Паллета с таким номером или штрих-кодом уже распакована. Хотите отменить операцию?", 
				"Отмена распаковки", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (dlgResult == MessageBoxResult.Yes)
			{
				using (var context = DB.GammaDb)
				{
					context.DocWithdrawalProducts.Remove(
						context.DocWithdrawalProducts.First(wp => wp.DocID == product.DocId));
					context.DocWithdrawal.Remove(context.DocWithdrawal.First(dw => dw.DocID == product.DocId));
					context.Docs.Remove(context.Docs.First(d => d.DocID == product.DocId));
					context.DocComplectation.First(d => d.DocComplectationID == DocId)
						.DocWithdrawal.Remove(context.DocWithdrawal.First(dw => dw.DocID == product.DocId));
					context.SaveChanges();
					var item = ComplectationItems.First(i => i.UnpackedPallets.Contains(product));
					item.UnpackedPallets.Remove(product);
				}
			}
		}

		private void CreateNewPallet(ComplectationItem item)
		{
			UIServices.SetBusyState();
            var unpackedProducts = item.UnpackedPallets.Sum(p => p.Quantity);
            var packedProducts = item.PackedPallets.Sum(p => p.Quantity);

            if (unpackedProducts == 0 || unpackedProducts < packedProducts + item.NewPalletCoefficient) //(packedProducts == 0 ? 0 : packedProducts + item.PackedPallets.FirstOrDefault().Quantity))
            {
                MessageBox.Show("Ошибка! Создать новую паллету невозможно, недостаточно распакованных паллет."+ "\n" + "Сначала распакуйте старую паллету!", "Ошибка документа", MessageBoxButton.OK,
                        MessageBoxImage.Error); ;
                return;
            }

            var docProductionId = SqlGuidUtil.NewSequentialid();
			var docProduction = documentController.ConstructDoc(docProductionId, DocTypes.DocProduction, true, placeId);
			docProduction.DocProduction = new DocProduction
			{
				DocID = docProductionId,
				InPlaceID = placeId
			};
			using (var context = DB.GammaDb)
			{
				context.Docs.Add(docProduction);
				context.DocComplectation.First(d => d.DocComplectationID == DocId).DocProduction.Add(docProduction.DocProduction);
				context.SaveChanges();
			}
			var product = productController.AddNewPalletToDocProduction(docProductionId, item.NomenclatureID,
				item.NewCharacteristicId, placeZoneID);
			var complectedPallet = new ComplectationProduct
			{
				DocId = docProductionId,
				ProductId = product.ProductID,
				Quantity = product.Quantity,
				Barcode = product.Barcode,
				Number = product.Number
			};
			item.PackedPallets.Add(complectedPallet);
			LastCreatedPalletNumber = product.Number;
			ReportManager.PrintReport("Амбалаж", "Pallet", docProductionId, false, 1);

            RaisePropertyChanged(() => ComplectationItems);
        }

        private void CreateNewPalletR(ComplectationItem item)
        {
            UIServices.SetBusyState();
            var unpackedProducts = item.UnpackedPallets.Sum(p => p.Quantity);
            var itemPalletR = (item.OldCharacteristicId != item.NewCharacteristicId) ? ComplectationItems.Where(c => c.NewCharacteristicId == item.OldCharacteristicId).FirstOrDefault() : null;
            var packedProducts = item.PackedPallets.Sum(p => p.Quantity) + (itemPalletR == null ? 0 : itemPalletR.PackedPallets.Sum(p => p.Quantity));

            if (unpackedProducts == 0)
            {
                MessageBox.Show("Ошибка! Создать неполную паллету невозможно, нет распакованных паллет" + "\n" + "Сначала распакуйте старую паллету!", "Ошибка документа", MessageBoxButton.OK,
                        MessageBoxImage.Error); ;
                return;
            }
            //else 
            //    if (item.OldPalletQuantity > item.NumUnpackedPallets)
            //{
            //    MessageBox.Show("Ошибка! Создать неполную паллету невозможно, мало распакованных паллет" + "\n" + "Сначала распакуйте старую паллету!", "Ошибка документа", MessageBoxButton.OK,
            //            MessageBoxImage.Error); ;
            //    return;
            //}
            else
            if (unpackedProducts - packedProducts <= 0)
            {
                MessageBox.Show("Ошибка! Создать неполную паллету невозможно, все распакованные паллету уже запакованы в новые!" + "\n" + "Сначала распакуйте старую паллету!", "Ошибка документа", MessageBoxButton.OK,
                        MessageBoxImage.Error); ;
                return;
            }
            int quantity;
            bool dialogResult;
            if (item.OldCharacteristicId != item.NewCharacteristicId)
            {                
                if (unpackedProducts - packedProducts >= item.NewPalletCoefficient)
                {
                    MessageBox.Show("Ошибка! Создать неполную паллету невозможно, распакованных паллет достаточно для создания полной паллеты" + "\n" + "Сначала создайте полную паллету!", "Ошибка документа", MessageBoxButton.OK,
                            MessageBoxImage.Error); ;
                    return;
                }

                quantity = (int)(unpackedProducts - packedProducts);
                dialogResult = true;
            }
            else
            {
                var baseQuantity = (int)(item.NewGroupPacksInPallet);

                var model = new SetQuantityDialogModel("Укажите групповых упаковок в неполной паллете", "Кол-во, гр.уп.", 1, Math.Min(baseQuantity, (int)((unpackedProducts - packedProducts) / (item.NewPalletCoefficient / item.NewGroupPacksInPallet))));
                var okCommand = new UICommand()
                {
                    Caption = "OK",
                    IsCancel = false,
                    IsDefault = true,
                    Command = new DelegateCommand<CancelEventArgs>(
                x => DebugFunc(),
                x => model.Quantity >= 1 && model.Quantity < baseQuantity),
                };
                var cancelCommand = new UICommand()
                {
                    Id = MessageBoxResult.Cancel,
                    Caption = "Отмена",
                    IsCancel = true,
                    IsDefault = false,
                };
                var dialogService = GetService<IDialogService>("");
                var result = dialogService.ShowDialog(
                    dialogCommands: new List<UICommand>() { okCommand, cancelCommand },
                    title: "Кол-во групповых упаковок",
                    viewModel: model);
                quantity = model.Quantity * (int)(item.NewPalletCoefficient / item.NewGroupPacksInPallet);
                dialogResult = (result == okCommand);
            }
            if (dialogResult)
            {
                var docProductionId = SqlGuidUtil.NewSequentialid();
                var docProduction = documentController.ConstructDoc(docProductionId, DocTypes.DocProduction, true, placeId);
                docProduction.DocProduction = new DocProduction
                {
                    DocID = docProductionId,
                    InPlaceID = placeId
                };
                using (var context = DB.GammaDb)
                {
                    context.Docs.Add(docProduction);
                    context.DocComplectation.First(d => d.DocComplectationID == DocId).DocProduction.Add(docProduction.DocProduction);
                    context.SaveChanges();
                }
                var product = productController.AddNewPalletToDocProduction(docProductionId, item.NomenclatureID,
                    item.OldCharacteristicId, quantity, placeZoneID);
                var complectedPallet = new ComplectationProduct
                {
                    DocId = docProductionId,
                    ProductId = product.ProductID,
                    Quantity = quantity,
                    Barcode = product.Barcode,
                    Number = product.Number
                };
                if (item.OldCharacteristicId == item.NewCharacteristicId)
                    item.PackedPallets.Add(complectedPallet);
                else
                {
                    var itemAdd = new ComplectationItem(item.NomenclatureID,
                                item.OldCharacteristicId, item.OldCharacteristicId,
                                item.QualityId, 0);
                    itemAdd.PackedPallets.Add(complectedPallet);
                    ComplectationItems.Add(itemAdd);
                }
                LastCreatedPalletNumber = product.Number;
                ReportManager.PrintReport("Неполная паллета", "Pallet", docProductionId, false, 1);

                RaisePropertyChanged(() => ComplectationItems);
            }
        }

        private void DebugFunc()
        {
            System.Diagnostics.Debug.Print("Кол-во задано");
        }

        #endregion

        #region Properties


        public DelegateCommand UnpackCommand { get; private set; }

		public DelegateCommand<ComplectationItem> CreatePalletCommand { get; private set; }

        public DelegateCommand<ComplectationItem> CreatePalletRCommand { get; private set; }

        /// <summary>
        /// 1C Document Number
        /// </summary>
        public string Number { get; set; }

		/// <summary>
		/// 1C Date
		/// </summary>
		public DateTime DocDate { get; set; }

		/// <summary>
		/// Unpack barcode or number
		/// </summary>
		public string Barcode
		{
			get { return barcode; }
			set
			{
				if (barcode == value)
				{
					return;
				}
				barcode = value;
				RaisePropertyChanged(() => Barcode);
			}
		}

        private List<ComplectationItem> _complectationItems { get; set; } = new List<ComplectationItem>();

        public List<ComplectationItem> ComplectationItems
        {
            get { return _complectationItems; }
            set
            {
                if (_complectationItems == value)
                {
                    return;
                }
                _complectationItems = value;
                RaisePropertyChanged(() => ComplectationItems);
            }
        }

        public string LastCreatedPalletNumber
		{
			get { return _lastCreatedPalletNumber; }
			set
			{
				if (_lastCreatedPalletNumber == value)
				{
					return;
				}
				_lastCreatedPalletNumber = value;
				RaisePropertyChanged(() => LastCreatedPalletNumber);
			}
		}

		#endregion

		#region Private properties

		private Guid DocId { get; set; }

		private List<Tuple<Guid, Guid>> OldNomenclature
		{
			get
			{
				return ComplectationItems.Select(c => new Tuple<Guid, Guid>(c.NomenclatureID, c.OldCharacteristicId)).ToList();
			}
		}

        public ComplectationProduct SelectedPackedPalletItem { get; set; }
        public DelegateCommand OpenPackedPalletItemCommand { get; private set; }
        public ComplectationProduct SelectedUnpackedPalletItem { get; set; }
        public DelegateCommand OpenUnpackedPalletItemCommand { get; private set; }
        private void OpenProduct(ComplectationProduct selectedPalletItem)
        {
            MessageManager.OpenDocProduct(DocProductKinds.DocProductPallet, selectedPalletItem.ProductId);
        }

        #endregion

        #region IBarImplemented

        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();

		public Guid? VMID { get; } = Guid.NewGuid();

		#endregion
	}
}
