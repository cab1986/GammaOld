using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Description for ManageUsersView.
    /// </summary>
    public partial class ManageUsersView : MVVMWindow
    {
        /// <summary>
        /// Initializes a new instance of the ManageUsersView class.
        /// </summary>
        public ManageUsersView()
        {
            InitializeComponent();
            Messenger.Default.Register<PermitEditMessage>(this, PermitEdit);
            Messenger.Default.Register<RoleEditMessage>(this, RoleEdit);
            Messenger.Default.Register<UserEditMessage>(this, UserEdit);
        }
        private void PermitEdit(PermitEditMessage msg)
        {
            var view = new PermitEditView(msg);
            view.Show();
        }
        private void UserEdit(UserEditMessage msg)
        {
            var view = new UserEditView(msg);
            view.Show();
        }
        private void RoleEdit(RoleEditMessage msg)
        {
            var view = new RoleEditView(msg);
            view.Show();
        }
        
    }
}