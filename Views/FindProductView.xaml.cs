using Gamma.ViewModels;

namespace Gamma.Views

{
    /// <summary>
    /// Description for FindProductView.
    /// </summary>
    public partial class FindProductView : MvvmWindow
    {
        /// <summary>
        /// Initializes a new instance of the FindProductView class.
        /// </summary>
        public FindProductView(FindProductMessage msg)
        {
            this.DataContext = new FindProductViewModel(msg);
            InitializeComponent();
        }
    }
}