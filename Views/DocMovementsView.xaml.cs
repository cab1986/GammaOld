using System.Windows.Controls;
using DevExpress.Xpf.Grid;
using Gamma.Models;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для DocMovementsView.xaml
    /// </summary>
    public partial class DocMovementsView : UserControl
    {
        public DocMovementsView()
        {
            InitializeComponent();
        }

        private void TableView_FocusedViewChanged(object sender, FocusedViewChangedEventArgs e)
        {
            var vm = this.DataContext as DocMovementsViewModel;
            if (vm == null) return;
            int rowHandle = ((GridControl)e.NewView.DataControl).GetMasterRowHandle();
            if (DataControlBase.InvalidRowHandle == rowHandle)
                return;
            vm.SelectedDocMovement = GridControl.GetRow(rowHandle) as MovementItem;
        }
    }
}
