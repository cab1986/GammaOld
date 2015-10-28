using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Gamma.ViewModels;
namespace Gamma.Views
{
    /// <summary>
    /// Interaction logic for RoleEditView.xaml
    /// </summary>
    public partial class RoleEditView : Window
    {
        public RoleEditView(RoleEditMessage msg)
        {
            RoleEditViewModel viewModel;
            if (msg.RoleID == null) viewModel = new RoleEditViewModel();
            else viewModel = new RoleEditViewModel((Guid)msg.RoleID);
            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}
