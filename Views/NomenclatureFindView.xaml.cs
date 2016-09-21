using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Description for NomenclatureFindView.
    /// </summary>
    public partial class NomenclatureFindView : Window
    {
        /// <summary>
        /// Initializes a new instance of the NomenclatureFindView class.
        /// </summary>
        public NomenclatureFindView(int? placeGroupID, bool nomenclatureEdit)
        {
            DataContext = new NomenclatureFindViewModel(placeGroupID, nomenclatureEdit);
            InitializeComponent();
        }

        public NomenclatureFindView(MaterialTypes materialType)
        {
            DataContext = new NomenclatureFindViewModel(materialType);
            InitializeComponent();
        }
    }
}