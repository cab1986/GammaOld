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

namespace Gamma.Views
{
	/// <summary>
	/// Логика взаимодействия для DocComplectationView.xaml
	/// </summary>
	public partial class DocComplectationView : UserControl
	{
		public DocComplectationView()
		{
			InitializeComponent();
		}

        private void GridControlUnpacked_CustomRowFilter(object sender, DevExpress.Xpf.Grid.RowFilterEventArgs e)
        {
            e.Visible = ((Models.ComplectationItem)GridControlUnpacked.GetRow(GridControlUnpacked.GetRowHandleByListIndex(e.ListSourceRowIndex))).OldPalletQuantity > 0;
            e.Handled = !e.Visible ? true : false;
        }

    }
}
