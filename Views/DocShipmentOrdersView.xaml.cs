using System.Windows.Controls;
using DevExpress.Xpf.Grid;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для DocShipmentOrdersView.xaml
    /// </summary>
    public partial class DocShipmentOrdersView : UserControl
    {
        public DocShipmentOrdersView()
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
