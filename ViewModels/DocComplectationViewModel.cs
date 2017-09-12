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

namespace Gamma.ViewModels
{
	public class DocComplectationViewModel : SaveImplementedViewModel, IBarImplemented
	{
		#region Fields

		private readonly DocumentController documentController = new DocumentController();
		private readonly ProductController productController = new ProductController();

		private string barcode;
		private string _lastCreatedPalletNumber;

		#endregion

		#region Constructor

		public DocComplectationViewModel(Guid docId)
		{
			DocId = docId;
			using (var context = DB.GammaDb)
			{
				var docComplectation = context.DocComplectation.Include(d => d.C1CDocComplectation).FirstOrDefault(d => d.DocComplectationID == docId);
				if (docComplectation == null)
				{
					MessageBox.Show("Произошла ошибка, документа нет в базе", "Ошибка документа", MessageBoxButton.OK,
						MessageBoxImage.Error);
					CloseWindow();
					return;
				}
				DocProductionId = docComplectation.DocProductionID;
				DocWithdrawalId = docComplectation.DocWithdrawalID;
				if (DocWithdrawalId == null)
				{
					DocWithdrawalId = SqlGuidUtil.NewSequentialid();
					var docWithdrawal = documentController.ConstructDoc((Guid)DocWithdrawalId, DocTypes.DocWithdrawal);
					context.Docs.Add(docWithdrawal);
					docComplectation.DocWithdrawalID = DocWithdrawalId;
					context.SaveChanges();
				}
				if (DocProductionId == null)
				{
					DocProductionId = SqlGuidUtil.NewSequentialid();
					var docProduction = documentController.ConstructDoc((Guid)DocProductionId, DocTypes.DocProduction);
					docProduction.DocProduction = new DocProduction
					{
						DocID = (Guid)DocProductionId,
						InPlaceID = WorkSession.PlaceID,
					};
					context.Docs.Add(docProduction);
					docComplectation.DocProductionID = DocProductionId;
					context.SaveChanges();
				}
				DocDate = (DateTime)docComplectation.C1CDocComplectation.Date;
				if (DocProductionId == null)
				{
					
				}
				Number = docComplectation.C1CDocComplectation.C1CCode;
				foreach (var nomenclaturePosition in context.C1CDocComplectationNomenclature
					.Where(d => d.C1CDocComplectationID == docComplectation.C1CDocComplectationID))
				{
					var item = new ComplectationItem(nomenclaturePosition.C1CNomenclatureID,
						nomenclaturePosition.C1COldCharacteristicID, nomenclaturePosition.C1CNewCharacteristicID,
						nomenclaturePosition.C1CQualityID, nomenclaturePosition.Quantity ?? 0);
					foreach (var product in context.DocProductionProducts.Include(dp => dp.Products)
							.Where(dp => dp.DocID == DocProductionId 
							&& dp.C1CNomenclatureID == nomenclaturePosition.C1CNomenclatureID
							&& dp.C1CCharacteristicID == nomenclaturePosition.C1CNewCharacteristicID))
						{
							item.PackedPallets.Add(new Product
							{
								ProductId = product.ProductID,
								Quantity = product.Quantity ?? 0,
								Number = product.Products.Number,
								Barcode = product.Products.BarCode
							});
						}
					foreach (var product in context.DocWithdrawalProducts.Include(dp => dp.Products)
							.Where(dp => dp.DocID == DocWithdrawalId
							             && dp.Products.ProductPallets.ProductPalletItems.FirstOrDefault().C1CNomenclatureID == nomenclaturePosition.C1CNomenclatureID
							             && dp.Products.ProductPallets.ProductPalletItems.FirstOrDefault().C1CCharacteristicID == nomenclaturePosition.C1COldCharacteristicID))
						{
							item.UnpackedPallets.Add(new Product
							{
								ProductId = product.ProductID,
								Quantity = product.Quantity ?? 0,
								Number = product.Products.Number,
								Barcode = product.Products.BarCode
							});
						}
					ComplectationItems.Add(item);
				}
			}
			Bars.Add(ReportManager.GetReportBar("DocComplectation", VMID));
			Messenger.Default.Register<BarcodeMessage>(this, BarcodeReceived);
			UnpackCommand = new DelegateCommand(Unpack, () => !string.IsNullOrEmpty(Barcode));
			CreatePalletCommand = new DelegateCommand<ComplectationItem>(CreateNewPallet);
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
					pallet = context.vProductsInfo.SingleOrDefault(p => p.BarCode.Contains(Barcode) || p.Number.Contains(Barcode) && p.ProductKindID == (int)ProductKind.ProductPallet);
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
				if (!documentController.WithdrawProduct(pallet.ProductID, (Guid)DocWithdrawalId))
				{
					MessageBox.Show("Не удалось списать паллету. Ошибка в базе.");
					return;
				}
				item.UnpackedPallets.Add(new Product
				{
					ProductId = pallet.ProductID,
					Number = pallet.Number,
					Barcode = pallet.BarCode,
					Quantity = pallet.Quantity ?? 0
				});
			}
		}

		/// <summary>
		/// Cancel unpack operation of product
		/// </summary>
		/// <param name="product"></param>
		private void DeleteFromUnpack(Product product)
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
						context.DocWithdrawalProducts.First(wp => wp.DocID == DocWithdrawalId &&
																			wp.ProductID == product.ProductId));
					context.SaveChanges();
					var item = ComplectationItems.First(i => i.UnpackedPallets.Contains(product));
					item.UnpackedPallets.Remove(product);
				}
			}
		}

		private void CreateNewPallet(ComplectationItem item)
		{
			UIServices.SetBusyState();
			var product = productController.AddNewPalletToDocProduction((Guid) DocProductionId, item.NomenclatureID,
				item.NewCharacteristicId);
			item.PackedPallets.Add(product);
			LastCreatedPalletNumber = product.Number;
			ReportManager.PrintReport("Амбалаж", "Pallet", product.ProductId, false, 2);
		}

		#endregion

		#region Properties

		public DelegateCommand UnpackCommand { get; private set; }

		public DelegateCommand<ComplectationItem> CreatePalletCommand { get; private set; }

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

		public List<ComplectationItem> ComplectationItems { get; set; } = new List<ComplectationItem>();

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

		private Guid? DocWithdrawalId { get; set; }

		private Guid? DocProductionId { get; set; }

		private List<Tuple<Guid, Guid>> OldNomenclature
		{
			get
			{
				return ComplectationItems.Select(c => new Tuple<Guid, Guid>(c.NomenclatureID, c.OldCharacteristicId)).ToList();
			}
		}

		#endregion

		#region IBarImplemented

		public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();

		public Guid? VMID { get; } = Guid.NewGuid();

		#endregion
	}
}
