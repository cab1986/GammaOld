using System;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using DevExpress.Xpf.CodeView;

namespace Gamma.ViewModels
{
	public class SpoolWithdrawByShiftViewModel : RootViewModel
	{
		#region Fields

		

		#endregion

		#region Constructor

		public SpoolWithdrawByShiftViewModel()
		{
			OpenSpoolCommand = new DelegateCommand(OpenSpool);
			UpdateSpools();
			Messenger.Default.Register<SpoolWithdrawed>(this, s => UpdateSpools());
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
				UsedSpools.AddRange(context.DocWithdrawalProducts.Where(dw => dw.DocWithdrawal.Docs.ShiftID == WorkSession.ShiftID
				&& dw.DocWithdrawal.Docs.Date >= DB.GetShiftBeginTime(DateTime.Now) && dw.DocWithdrawal.Docs.Date <= DB.GetShiftEndTime(DateTime.Now)
				&& dw.Products.ProductKindID == (int)ProductKind.ProductSpool
                && dw.DocWithdrawal.Docs.PlaceID == WorkSession.PlaceID)
                .GroupBy(dw => dw.ProductID)
				.Select(gp => new UsedSpool
					{
						ProductId = gp.Key,
						UsedQuantity = (int)gp.Sum(dw => dw.Quantity ?? 0) * 1000,
						Number = gp.FirstOrDefault().Products.Number,
						RemainderQuantity = (int)(gp.FirstOrDefault().Products.ProductSpools.DecimalWeight*1000),
						Nomenclature = gp.FirstOrDefault().Products.ProductSpools.C1CNomenclature.Name + gp.FirstOrDefault().Products.ProductSpools.C1CCharacteristics.Name,
						InstallDate = context.SpoolInstallLog.OrderByDescending(s => s.Date).FirstOrDefault(s => s.ProductID == gp.Key).Date
				}).OrderBy(s => s.InstallDate));
			}
		}

		#endregion

	}

	public class UsedSpool
	{
		public Guid ProductId { get; set; }
		public string Number { get; set; }
		public string Nomenclature { get; set; }
		public int UsedQuantity { get; set; }
		public int RemainderQuantity { get; set; }
		public DateTime? InstallDate { get; set; }
	}
}
