using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Окно авторизации
    /// </summary>
    public partial class LoginView : MVVMWindow
    {
        /// <summary>
        /// Initializes a new instance of the LoginView class.
        /// </summary>
        public LoginView()
        {
            InitializeComponent();
            Messenger.Default.Register<OpenMainMessage>(this, OpenMain);
        }

        private void OpenMain(OpenMainMessage obj)
        {
            var View = new MainView();
            View.Show();
        }
    }
}