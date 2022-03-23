using Gamma.Entities;
using Gamma.Models;
using Gamma.ViewModels;
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
    /// Interaction logic for DocBrokeDecisionView.xaml
    /// </summary>
    public partial class DocBrokeDecisionView : UserControl
    {
        public DocBrokeDecisionView()
        {
            InitializeComponent();
        }
        /*private void GridControl_CustomRowFilter(object sender, DevExpress.Xpf.Grid.RowFilterEventArgs e)
        {
            var row = ((BrokeDecisionProduct)grid.GetRow(grid.GetRowHandleByListIndex(e.ListSourceRowIndex)));
            if (row != null)
            {
                e.Visible = row.IsVisibleRow; //row == null || !(row.DecisionApplied && (row.ProductState == ProductState.NeedsDecision || row.ProductState == ProductState.ForConversion || row.ProductState == ProductState.Repack));
                e.Handled = !e.Visible ? true : false;
            }
        }*/
    }
}
