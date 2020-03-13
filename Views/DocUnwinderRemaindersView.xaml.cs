using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Description for DocUnwinderRemaindersView.
    /// </summary>
    public partial class DocUnwinderRemaindersView : Window
    {
        /// <summary>
        /// Initializes a new instance of the DocUnwinderRemaindersView class.
        /// </summary>
        public DocUnwinderRemaindersView()
        {
            this.DataContext = new DocUnwinderRemaindersViewModel();
            InitializeComponent();
        }
        public DocUnwinderRemaindersView(PlaceGroup placeGroup)
        {
            this.DataContext = new DocUnwinderRemaindersViewModel(placeGroup);
            InitializeComponent();
        }
    }
}