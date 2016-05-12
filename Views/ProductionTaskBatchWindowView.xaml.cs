using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для ProductionTaskBatchWindowView.xaml
    /// </summary>
    public partial class ProductionTaskBatchWindowView : Window
    {
        public ProductionTaskBatchWindowView()
        {
            InitializeComponent();
        }

        public ProductionTaskBatchWindowView(OpenProductionTaskBatchMessage msg)
        {
            DataContext = new ProductionTaskBatchWrapperViewModel(msg);
            InitializeComponent();
        }
    }
}
