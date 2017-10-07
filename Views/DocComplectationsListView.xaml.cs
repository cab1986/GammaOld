using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Xpf.Grid;
using Gamma.Models;
using Gamma.ViewModels;

namespace Gamma.Views
{
	/// <summary>
	/// Логика взаимодействия для DocComplectationsListView.xaml
	/// </summary>
	public partial class DocComplectationsListView : UserControl
	{
		public DocComplectationsListView()
		{
			InitializeComponent();
		}

		private void TableView_FocusedViewChanged(object sender, FocusedViewChangedEventArgs e)
		{
			var vm = this.DataContext as DocShipmentOrdersViewModel;
			if (vm == null) return;
			int rowHandle = ((GridControl)e.NewView.DataControl).GetMasterRowHandle();
			if (DataControlBase.InvalidRowHandle == rowHandle)
				return;
			vm.SelectedDocShipmentOrder = GridControl.GetRow(rowHandle) as DocShipmentOrder;
		}
	}
}
