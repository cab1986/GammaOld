using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Description for NomenclatureView.
    /// </summary>
    public partial class NomenclatureView : Window
    {
        /// <summary>
        /// Initializes a new instance of the NomenclatureView class.
        /// </summary>
        public NomenclatureView(int placeGroupID)
        {
            DataContext = new NomenclatureViewModel(placeGroupID);
            InitializeComponent();
        }

        public NomenclatureView(MaterialTypes materialType)
        {
            DataContext = new NomenclatureViewModel(materialType);
            InitializeComponent();
        }
    }
}