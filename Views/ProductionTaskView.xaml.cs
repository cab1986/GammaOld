using System.Windows;
using System.Windows.Controls;
using Gamma.ViewModels;
using GalaSoft.MvvmLight.Messaging;

namespace Gamma.Views
{
    /// <summary>
    /// Description for ProductionTaskView.
    /// </summary>
    public partial class ProductionTaskView : MVVMWindow
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskView class.
        /// </summary>

        public ProductionTaskView(OpenProductionTaskMessage msg)
        {
            this.DataContext = new ProductionTaskViewModel(msg);
            InitializeComponent();
            Messenger.Default.Register<OpenNomenclatureMessage>(this, OpenNomenclature);
            Messenger.Default.Register<OpenDocProductMessage>(this, OpenDocProduct);
        }

        private void OpenNomenclature(OpenNomenclatureMessage msg)
        {
            new NomenclatureView().Show();
        }
        private void OpenDocProduct(OpenDocProductMessage msg)
        {
            new DocProductView(msg).Show();
        }
    }
}