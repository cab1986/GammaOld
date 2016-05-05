using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Description for ReportListView.
    /// </summary>
    public partial class ReportListView
    {
        /// <summary>
        /// Initializes a new instance of the ReportListView class.
        /// </summary>
        public ReportListView()
        {
            DataContext = new ReportListViewModel(DB.GammaDb);
            InitializeComponent();
        }
    }
}