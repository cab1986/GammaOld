using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Description for ManageUsersView.
    /// </summary>
    public partial class ManageUsersView
    {
        /// <summary>
        /// Initializes a new instance of the ManageUsersView class.
        /// </summary>
        public ManageUsersView()
        {
            DataContext = new ManageUsersViewModel();
            InitializeComponent();
        }        
    }
}