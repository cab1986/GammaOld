using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Interaction logic for FindProductionTaskView.xaml
    /// </summary>
    public partial class FindProductionTaskBatchView : Window
    {
        public FindProductionTaskBatchView(BatchKinds batchKind)
        {
            this.DataContext = new FindProductionTaskBatchViewModel(batchKind);
            InitializeComponent();
        }
    }
}
