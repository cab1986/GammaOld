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
						NewPalletCoefficient = info.PalletCoefficient == 0 ? 1 : info.PalletCoefficient;
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

        public decimal NewPalletCoefficient { get; private set; }

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

		public ObservableCollection<ComplectationProduct> UnpackedPallets { get; set; } = new ObservableCollection<ComplectationProduct>();
		public ObservableCollection<ComplectationProduct> PackedPallets { get; set; } = new ObservableCollection<ComplectationProduct>();

		public decimal UnpackedQuantity
		{
			get { return UnpackedPallets.Sum(p => p.Quantity); }
		}

		public decimal PackedQuantity
		{
			get { return PackedPallets.Sum(p => p.Quantity); }
		}

		public decimal OldPalletQuantity => Quantity / oldPalletCoefficient;

		public decimal NewPalletQuantity => Quantity / NewPalletCoefficient;

		public decimal OldGroupPacksInPallet => oldPalletCoefficient / oldGroupPackCoefficient;

		public decimal NewGroupPacksInPallet => NewPalletCoefficient / newGroupPackCoefficient;

        public decimal OldGroupPackQuantity => Quantity / oldGroupPackCoefficient;

        public decimal NewGroupPackQuantity => Quantity / oldPalletCoefficient;

        public decimal NumUnpackedPallets
		{
			get { return OldPalletQuantity > 0 ? UnpackedQuantity / oldPalletCoefficient : 0; }
		}

		public decimal NumPackedPallets
		{
			get { return NewPalletQuantity > 0 ? PackedQuantity / NewPalletCoefficient : 0; }
		}

        public string Unpacked
        {
            get { return UnpackedPallets?.Count() + Environment.NewLine+"\\" + (int)(UnpackedQuantity/oldGroupPackCoefficient); }
        }

        public string Packed
        {
            get { return PackedPallets?.Count() + Environment.NewLine+"\\" + (int)(PackedQuantity/newGroupPackCoefficient); }
        }

        public string RequiredUnpacked
        {
            get { return (int)OldPalletQuantity + Environment.NewLine + "\\" + (int)OldGroupPackQuantity; }
        }

        public string RequiredPacked
        {
            get { return (int)NewPalletQuantity + Environment.NewLine + "\\" + (int)NewGroupPackQuantity; }
        }

        public bool IsEnabledCreateNewPallet => (OldPalletQuantity > 0);
	}
}
