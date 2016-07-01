using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для ImportOldProductsView.xaml
    /// </summary>
    public partial class ImportOldProductsView : Window
    {
        public ImportOldProductsView()
        {
            DataContext = new ImportOldProductsViewModel();
            InitializeComponent();
        }
    }
}
