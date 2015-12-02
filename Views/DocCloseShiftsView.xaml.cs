using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Description for DocCloseShiftsView.
    /// </summary>
    public partial class DocCloseShiftsView : Window
    {
        /// <summary>
        /// Initializes a new instance of the DocCloseShiftsView class.
        /// </summary>
        public DocCloseShiftsView()
        {
            this.DataContext = new DocCloseShiftsViewModel();
            InitializeComponent();
        }
        public DocCloseShiftsView(PlaceGroups placeGroup)
        {
            this.DataContext = new DocCloseShiftsViewModel(placeGroup);
            InitializeComponent();
        }
    }
}