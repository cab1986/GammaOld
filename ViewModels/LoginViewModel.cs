using DevExpress.Mvvm;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using Gamma.Common;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class LoginViewModel : ValidationViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the LoginViewModel class.
        /// </summary>
        /// 

        private string _login;
        [Required(ErrorMessage="Поле не может быть пустым")]
        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
            }
        }
        
        [Required(ErrorMessage = "Поле не может быть пустым")]
        
        public string Host
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Поле не может быть пустым")]
        public string DataBase { get; set; }

        private string _password;

        [Required(ErrorMessage = "Поле не может быть пустым")]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public DelegateCommand LoginCommand { get; private set; }
        public LoginViewModel()
        {
            var AppSettings = GammaSettings.Get();
            Host = AppSettings.HostName;
            DataBase = AppSettings.DbName;
            Login = AppSettings.User;
            UseScanner = AppSettings.UseScanner;
            LoginCommand = new DelegateCommand(Authenticate, canExecute);
        }

        private bool canExecute()
        {
            return IsValid;
        }

        private void Authenticate()
        {
            UIServices.SetBusyState();
            GammaSettings.SetConnectionString(Host, DataBase, Login, Password);
            if (!DB.Initialize())
            {
                MessageBox.Show("Неверный логин или пароль!");
                return;
            }
            if (UseScanner && !Scanner.IsReady)
            {
                MessageBox.Show("Не удалось подключить сканер, программа запустится без сканера",
                "Ошибка при подключении сканера", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                GammaSettings.Get().UseScanner = false;
            }
            else GammaSettings.Get().UseScanner = UseScanner;
            MessageManager.OpenMain();
            CloseWindow();
        }
        public bool UseScanner { get; set; }

    }
    
}