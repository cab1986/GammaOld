using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для PlaceAuxiliaryMaterialsView.xaml
    /// </summary>
    public partial class PlaceAuxiliaryMaterialsView : Window
    {
        public PlaceAuxiliaryMaterialsView()
        {
            DataContext = new PlaceAuxiliaryMaterialsViewModel();
            InitializeComponent();
        }
    }
}
