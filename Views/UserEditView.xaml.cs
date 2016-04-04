using System;
using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Interaction logic for NewUserView.xaml
    /// </summary>
    public partial class UserEditView : Window
    {
        public UserEditView(UserEditMessage msg)
        {
            UserEditViewModel viewModel;
            if (msg.UserID == null) viewModel = new UserEditViewModel();
            else viewModel = new UserEditViewModel((Guid)msg.UserID);
            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}
