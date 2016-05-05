using DevExpress.Mvvm;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Gamma.Common;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public class LoginViewModel : ValidationViewModelBase
    {
        [Required(ErrorMessage=@"Поле не может быть пустым")]
        public string Login { get; set; }

        [Required(ErrorMessage = @"Поле не может быть пустым")]
        public string Host
        {
            get;
            set;
        }

        [Required(ErrorMessage = @"Поле не может быть пустым")]
        // ReSharper disable once MemberCanBePrivate.Global
        public string DataBase { get; set; }

        [Required(ErrorMessage = @"Поле не может быть пустым")]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Password { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DelegateCommand LoginCommand { get; private set; }
        public LoginViewModel()
        {
            var appSettings = GammaSettings.Get();
            Host = appSettings.HostName;
            DataBase = appSettings.DbName;
            Login = appSettings.User;
            UseScanner = appSettings.UseScanner;
            LoginCommand = new DelegateCommand(Authenticate, CanExecute);
        }

        private bool CanExecute()
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