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
using Gamma.Entities;

namespace Gamma.ViewModels
{
	public class DocComplectationViewModel : SaveImplementedViewModel, IBarImplemented
	{
		#region Fields

		private DocumentController documentController = new DocumentController();

		private string barcode;

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
				DocDate = (DateTime)docComplectation.C1CDocComplectation.Date;

				Number = docComplectation.C1CDocComplectation.C1CCode;
				foreach (var nomenclaturePosition in context.C1CDocComplectationNomenclature
					.Where(d => d.C1CDocComplectationID == docComplectation.C1CDocComplectationID))
				{
					var item = new ComplectationItem(nomenclaturePosition.C1CNomenclatureID,
						nomenclaturePosition.C1COldCharacteristicID, nomenclaturePosition.C1CNewCharacteristicID,
						nomenclaturePosition.Quantity ?? 0);
					if (DocProductionId != null)
					{
						foreach (var product in context.DocProductionProducts.Include(dp => dp.Products)
							.Where(dp => dp.DocID == DocProductionId 
							&& dp.C1CNomenclatureID == nomenclaturePosition.C1CNomenclatureID
							&& dp.C1CCharacteristicID == nomenclaturePosition.C1CNewCharacteristicID))
						{
							item.PackedPallets.Add(new Product
							{
								ProductId = product.ProductID,
								Quantity = product.Quantity ?? 0,
								Number = product.Products.Number
							});
						}
					}
					if (DocWithdrawalId != null)
					{
						foreach (var product in context.DocWithdrawalProducts.Include(dp => dp.Products)
							.Where(dp => dp.DocID == DocWithdrawalId
							             && dp.Products.ProductPallets.ProductPalletItems.FirstOrDefault().C1CNomenclatureID == nomenclaturePosition.C1CNomenclatureID
							             && dp.Products.ProductPallets.ProductPalletItems.FirstOrDefault().C1CCharacteristicID == nomenclaturePosition.C1COldCharacteristicID))
						{
							item.UnpackedPallets.Add(new Product
							{
								ProductId = product.ProductID,
								Quantity = product.Quantity ?? 0,
								Number = product.Products.Number
							});
						}
					}
					ComplectationItems.Add(item);
				}
			}
			Bars.Add(ReportManager.GetReportBar("DocComplectation", VMID));
			Messenger.Default.Register<BarcodeMessage>(this, BarcodeReceived);
			UnpackCommand = new DelegateCommand(Unpack, () => !string.IsNullOrEmpty(Barcode));
		}

		#endregion

		#region Private Methods

		private void BarcodeReceived(BarcodeMessage msg)
		{
			Barcode = msg.Barcode;
			Unpack();
		}

		private void Unpack()
		{
			if (string.IsNullOrEmpty(barcode))
			{
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
				if (!OldNomenclature.Any(
					n => n.Item1.Equals(pallet.C1CNomenclatureID) && n.Item2.Equals(pallet.C1CCharacteristicID)))
				{
					MessageBox.Show("Номенклатура найденной паллеты не совпадает с документом");
					return;
				}
				bool withdrawResult = false;
				if (DocWithdrawalId == null)
				{
					DocWithdrawalId = SqlGuidUtil.NewSequentialid();
					if (documentController.WithdrawProduct(pallet.ProductID, (Guid) DocWithdrawalId))
					{
						var complectation = context.DocComplectation.First(d => d.DocComplectationID == DocId);
						complectation.DocWithdrawalID = DocWithdrawalId;
						context.SaveChanges();
					}
					else
					{
						DocWithdrawalId = null;
						MessageBox.Show("Не удалось списать паллету. Ошибка в базе.");
						return;
					}
				}
				else if (!documentController.WithdrawProduct(pallet.ProductID, (Guid)DocWithdrawalId))
				{
					MessageBox.Show("Не удалось списать паллету. Ошибка в базе.");
					return;
				}
				var item = ComplectationItems.First(i => i.NomenclatureID == pallet.C1CNomenclatureID &&
				                                         i.OldCharacteristicId == pallet.C1CCharacteristicID);
				item.UnpackedPallets.Add(new Product
				{
					ProductId = pallet.ProductID,
					Number = pallet.Number,
					Barcode = pallet.BarCode,
					Quantity = pallet.Quantity ?? 0
				});
			}
		}

		#endregion

		#region Properties

		public DelegateCommand UnpackCommand { get; private set; }

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
