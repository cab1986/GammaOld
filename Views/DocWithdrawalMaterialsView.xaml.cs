using Gamma.Models;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для DocWithdrawalMaterialsView.xaml
    /// </summary>
    public partial class DocWithdrawalMaterialsView
    {
        private DocWithdrawalMaterialsView()
        {
            InitializeComponent();
        }

        public DocWithdrawalMaterialsView(BrokeProduct brokeProduct)
        {
            DataContext = new DocWithdrawalMaterialsViewModel(brokeProduct);
            InitializeComponent();
        }
    }
}
