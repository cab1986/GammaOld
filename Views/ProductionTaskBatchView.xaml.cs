using System.Windows;
using System.Windows.Controls;
using Gamma.ViewModels;
using GalaSoft.MvvmLight.Messaging;

namespace Gamma.Views
{
    /// <summary>
    /// Description for ProductionTaskView.
    /// </summary>
    public partial class ProductionTaskBatchView : MVVMWindow
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskView class.
        /// </summary>

        public ProductionTaskBatchView(OpenProductionTaskBatchMessage msg)
        {
            this.DataContext = new ProductionTaskBatchViewModel(msg);
            InitializeComponent();
        }
    }
}