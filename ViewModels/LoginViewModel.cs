using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using Gamma.Attributes;
using Gamma.Interfaces;

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

        public RelayCommand LoginCommand { get; private set; }
        public LoginViewModel()
        {
            var AppSettings = GammaSettings.Get();
            Host = AppSettings.HostName;
            DataBase = AppSettings.DbName;
            Login = AppSettings.User;
            LoginCommand = new RelayCommand(Authenticate, canExecute);
        }

        private bool canExecute()
        {
            return this.IsValid;
        }

        private void Authenticate()
        {
            GammaSettings.SetConnectionString(Host, DataBase, Login, Password);
            if (!DB.Initialize())
            {
                MessageBox.Show("Неверный логин или пароль!");
                return;
            }
            MessageManager.OpenMain();
            CloseWindow();
        }

        }
    
}