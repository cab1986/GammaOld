using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using DevExpress.Mvvm;

namespace Gamma.Models
{
	public class ComplectationItem : ViewModelBase
	{
		#region Fields

		private readonly decimal oldGroupPackCoefficient = 1;

		private readonly decimal oldPalletCoefficient = 1;

		private readonly decimal newGroupPackCoefficient = 1;

		private readonly decimal newPalletCoefficient = 1;

		#endregion

		#region Constructor

		public ComplectationItem(Guid nomenclatureId, Guid oldCharacteristicId, Guid newCharacteristicId, Guid? qualityId, decimal quantity)
		{
			UnpackedPallets.CollectionChanged += UnpackedChanged;
			PackedPallets.CollectionChanged += PackedChanged;
			Quantity = quantity;
			NomenclatureID = nomenclatureId;
			OldCharacteristicId = oldCharacteristicId;
			NewCharacteristicId = newCharacteristicId;
			QualityId = qualityId;
			using (var context = DB.GammaDb)
			{
				var nomInfo = context.C1CCharacteristics.Where(c =>
						c.C1CCharacteristicID == OldCharacteristicId || c.C1CCharacteristicID == NewCharacteristicId
						&& c.C1CNomenclatureID == NomenclatureID)
					.Select(c => new
					{
						c.C1CCharacteristicID,
						Nomenclature = c.C1CNomenclature.Name + " " + c.Name,
						GroupPacksCoefficient = c.C1CMeasureUnitsPackage.Coefficient ?? 0,
						PalletCoefficient = c.C1CMeasureUnitsPallet.Coefficient ?? 0
					});
				foreach (var info in nomInfo)
				{
					if (info.C1CCharacteristicID == OldCharacteristicId)
					{
						OldNomenclature = info.Nomenclature;
						oldGroupPackCoefficient = info.GroupPacksCoefficient == 0 ? 1 : info.GroupPacksCoefficient;
						oldPalletCoefficient = info.PalletCoefficient == 0 ? 1 : info.PalletCoefficient;
					}
					if (info.C1CCharacteristicID == NewCharacteristicId)
					{
						NewNomenclature = info.Nomenclature;
						newGroupPackCoefficient = info.GroupPacksCoefficient == 0 ? 1 : info.GroupPacksCoefficient;
						newPalletCoefficient = info.PalletCoefficient == 0 ? 1 : info.PalletCoefficient;
					}
				}
			}
		}

		#endregion

		#region Private methods

		private void PackedChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged(() => PackedQuantity);
			RaisePropertyChanged(() => NumPackedPallets);
		}

		private void UnpackedChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged(() => UnpackedQuantity);
			RaisePropertyChanged(() => NumUnpackedPallets);
		}

		#endregion

		public Guid NomenclatureID { get; private set; }

		public Guid OldCharacteristicId { get; private set; }

		public Guid NewCharacteristicId { get; private set; }

		public Guid? QualityId { get; private set; }

		/// <summary>
		/// String representation of old nomenclature + characteristic
		/// </summary>
		public string OldNomenclature { get; set; }

		/// <summary>
		/// String representation of new nomenclature + characteristic
		/// </summary>
		public string NewNomenclature { get; set; }

		public decimal Quantity { get; set; }

		public ObservableCollection<Product> UnpackedPallets { get; set; } = new ObservableCollection<Product>();
		public ObservableCollection<Product> PackedPallets { get; set; } = new ObservableCollection<Product>();

		public decimal UnpackedQuantity
		{
			get { return UnpackedPallets.Sum(p => p.Quantity); }
		}

		public decimal PackedQuantity
		{
			get { return PackedPallets.Sum(p => p.Quantity); }
		}

		public decimal OldPalletQuantity => Quantity / oldPalletCoefficient;

		public decimal NewPalletQuantity => Quantity / newPalletCoefficient;

		public decimal OldGroupPacksInPallet => oldPalletCoefficient / oldGroupPackCoefficient;

		public decimal NewGroupPacksInPallet => newPalletCoefficient / newGroupPackCoefficient;

		public decimal NumUnpackedPallets
		{
			get { return OldPalletQuantity > 0 ? UnpackedQuantity / oldPalletCoefficient : 0; }
		}

		public decimal NumPackedPallets
		{
			get { return NewPalletQuantity > 0 ? PackedQuantity / newPalletCoefficient : 0; }
		}
	}
}
