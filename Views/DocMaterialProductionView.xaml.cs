using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Interaction logic for DocMaterialProductionView.xaml
    /// </summary>
    public partial class DocMaterialProductionView
    {
        public DocMaterialProductionView(OpenDocMaterialProductionMessage msg)
        {
            this.DataContext = new DocMaterialProductionViewModel(msg);
            InitializeComponent();
        }
    }
}
