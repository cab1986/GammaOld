using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Interaction logic for DocCloseShiftView.xaml
    /// </summary>
    public partial class DocCloseShiftView
    {
        public DocCloseShiftView(OpenDocCloseShiftMessage msg)
        {
            this.DataContext = new DocCloseShiftViewModel(msg);
            InitializeComponent();
        }
    }
}
