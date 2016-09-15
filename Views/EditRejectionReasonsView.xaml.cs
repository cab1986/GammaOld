using Gamma.Models;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для EditRejectionReasonsView.xaml
    /// </summary>
    public partial class EditRejectionReasonsView
    {
        private EditRejectionReasonsView()
        {
            InitializeComponent();
        }

        public EditRejectionReasonsView(BrokeProduct brokeProduct)
        {
            DataContext = new EditRejectionReasonsViewModel(brokeProduct);
            InitializeComponent();
        }
    }
}
