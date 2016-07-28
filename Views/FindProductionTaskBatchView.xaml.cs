using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Interaction logic for FindProductionTaskView.xaml
    /// </summary>
    public partial class FindProductionTaskBatchView
    {
        public FindProductionTaskBatchView(BatchKinds batchKind)
        {
            DataContext = new FindProductionTaskBatchViewModel(batchKind);
            InitializeComponent();
        }
    }
}
