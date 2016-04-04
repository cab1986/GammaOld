using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Interaction logic for PlaceGroupsNomenclatureView.xaml
    /// </summary>
    public partial class PlaceGroupsNomenclatureView : Window
    {
        public PlaceGroupsNomenclatureView()
        {
            this.DataContext = new PlaceGroupsNomenclatureViewModel();
            InitializeComponent();
        }
    }
}
