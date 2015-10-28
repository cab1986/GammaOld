using System.Windows;
using Gamma.ViewModels;
using System;

namespace Gamma.Views
{
    /// <summary>
    /// Description for NewPermitView.
    /// </summary>
    public partial class PermitEditView : Window
    {
        /// <summary>
        /// Initializes a new instance of the NewPermitView class.
        /// </summary>
        public PermitEditView(PermitEditMessage msg)
        {
            PermitEditViewModel viewModel;
            if (msg.PermitID == null) viewModel = new PermitEditViewModel();
            else viewModel = new PermitEditViewModel((Guid)msg.PermitID);
            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}