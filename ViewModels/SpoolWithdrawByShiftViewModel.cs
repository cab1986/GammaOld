using System;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;

namespace Gamma.ViewModels
{
	public class SpoolWithdrawByShiftViewModel : RootViewModel
	{
		#region Fields

		

		#endregion

		#region Constructor

		public SpoolWithdrawByShiftViewModel()
		{
			
		}

		#endregion

		#region Properties

		public ObservableCollection<UsedSpool> UsedSpools { get; set; } = new ObservableCollection<UsedSpool>();

		public UsedSpool SelectedSpool { get; set; }

		public DelegateCommand OpenSpoolCommand { get; private set; }

		#endregion

		#region Private Methods

		private void OpenSpool()
		{
			if (SelectedSpool == null)
			{
				return;
			}
			MessageManager.OpenDocProduct(ProductKind.ProductSpool, SelectedSpool.ProductId);
		}

		private void UpdateSpools()
		{
			UsedSpools.Clear();
			using (var context = DB.GammaDb)
			{
				
			}
		}

		#endregion

	}

	public class UsedSpool
	{
		public Guid ProductId { get; set; }
		public string Number { get; set; }
		public string Nomenclature { get; set; }
		public decimal UsedQuantity { get; set; }
		public decimal RemainderQuantity { get; set; }
	}
}
