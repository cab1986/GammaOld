using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Description for DocProductView.
    /// </summary>
    public partial class DocProductView : MvvmWindow
    {
        /// <summary>
        /// Initializes a new instance of the DocProductView class.
        /// </summary>
        public DocProductView(OpenDocProductMessage msg)
        {
            this.DataContext = new DocProductViewModel(msg);
            InitializeComponent();
        }
        
    }
}