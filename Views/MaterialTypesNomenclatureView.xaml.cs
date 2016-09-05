using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для MaterialTypeNomenclatureView.xaml
    /// </summary>
    public partial class MaterialTypesNomenclatureView : Window
    {
        public MaterialTypesNomenclatureView()
        {
            DataContext = new MaterialTypesNomenclatureViewModel();
            InitializeComponent();
        }
    }
}
