using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для PlaceZonesView.xaml
    /// </summary>
    public partial class PlaceZonesView : Window
    {
        public PlaceZonesView()
        {
            DataContext = new PlaceZonesViewModel();
            InitializeComponent();
        }
    }
}
