using System;
using Gamma.ViewModels;
namespace Gamma.Views
{
    /// <summary>
    /// Logic fo RoleEidtView Window
    /// </summary>
    public partial class RoleEditView
    {
        public RoleEditView(RoleEditMessage msg)
        {
            var viewModel = msg.RoleID == null ? new RoleEditViewModel() : new RoleEditViewModel((Guid)msg.RoleID);
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
