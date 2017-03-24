using DevExpress.Mvvm;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Окно авторизации
    /// </summary>
    public partial class LoginView
    {
        /// <summary>
        /// Initializes a new instance of the LoginView class.
        /// </summary>
        public LoginView()
        {
            DataContext = new LoginViewModel();
            InitializeComponent();
            Messenger.Default.Register<OpenMainMessage>(this, OpenMain);
        }

        private void OpenMain(OpenMainMessage obj)
        {
            var view = new MainView();
            view.Show();
        }
    }
}