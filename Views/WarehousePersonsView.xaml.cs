using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для WarehousePersonsView.xaml
    /// </summary>
    public partial class WarehousePersonsView : Window
    {
        public WarehousePersonsView()
        {
            DataContext = new WarehousePersonsViewModel();
            InitializeComponent();
        }
    }
}
