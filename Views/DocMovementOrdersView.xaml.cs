using DevExpress.Xpf.Grid;
using Gamma.Models;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для DocMovementOrdersView.xaml
    /// </summary>
    public partial class DocMovementOrdersView
    {
        public DocMovementOrdersView()
        {
            InitializeComponent();
        }

        private void TableView_FocusedViewChanged(object sender, FocusedViewChangedEventArgs e)
        {
            var vm = DataContext as DocMovementOrdersViewModel;
            if (vm == null) return;
            int rowHandle = ((GridControl)e.NewView.DataControl).GetMasterRowHandle();
            if (DataControlBase.InvalidRowHandle == rowHandle)
                return;
            vm.SelectedDocMovementOrderItem = GridControl.GetRow(rowHandle) as DocMovementOrderItem;
        }
    }
}
