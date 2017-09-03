using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using DevExpress.Mvvm;

namespace Gamma.Models
{
	public class ComplectationItem : ViewModelBase
	{
		public ComplectationItem(Guid nomenclatureId, Guid oldCharacteristicId, Guid newCharacteristicId, decimal quantity)
		{
			UnpackedPallets.CollectionChanged += UnpackedChanged;
			PackedPallets.CollectionChanged += PackedChanged;
			Quantity = quantity;
			NomenclatureID = nomenclatureId;
			OldCharacteristicId = oldCharacteristicId;
			NewCharacteristicId = newCharacteristicId;
		}

		#region Private methods

		private void PackedChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged(() => PackedQuantity);
			RaisePropertyChanged(() => NumComplectedPallets);
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

		public string OldNomenclatre { get; set; }
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

		public decimal OldPalletQuantity { get; set; }
		public decimal NewPalletQuantity { get; set; }

		public decimal NumUnpackedPallets
		{
			get { return OldPalletQuantity > 0 ? UnpackedQuantity / OldPalletQuantity : 0; }
		}

		public decimal NumComplectedPallets
		{
			get { return NewPalletQuantity > 0 ? UnpackedQuantity / NewPalletQuantity : 0; }
		}
	}
}
