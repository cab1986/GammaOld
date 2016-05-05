using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Interaction logic for SourceSpoolsView.xaml
    /// </summary>
    public partial class SourceSpoolsView
    {
        public SourceSpoolsView()
        {
            DataContext = new SourceSpoolsViewModel(DB.GammaDb);
            InitializeComponent();
        }
    }
}
