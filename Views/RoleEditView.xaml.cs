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
            var viewModel = msg.RoleID == null ? new RoleEditViewModel(msg.GammaBase) : new RoleEditViewModel((Guid)msg.RoleID, msg.GammaBase);
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
