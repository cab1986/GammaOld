using System.Windows;
using Gamma.ViewModels;
using System;

namespace Gamma.Views
{
    /// <summary>
    /// Description for DocProductView.
    /// </summary>
    public partial class DocProductView : MVVMWindow
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