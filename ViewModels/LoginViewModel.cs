// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using DevExpress.Mvvm;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using Gamma.Common;
using System;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// </summary>
    public class LoginViewModel : ValidationViewModelBase
    {
        /*
        public class Metadata : IMetadataProvider<LoginViewModel>
        {
            void IMetadataProvider<LoginViewModel>.BuildMetadata(MetadataBuilder<LoginViewModel> builder)
            {
                builder.CommandFromMethod(x => x.Authenticate())
                    .CanExecuteMethod(x => x.CanExecute())
                    .CommandName("LoginCommand");
            }
        }
        */


        [Required(ErrorMessage=@"Поле не может быть пустым")]
        public string Login { get; set; }

        [Required(ErrorMessage = @"Поле не может быть пустым")]
        public string Host
        {
            get;
            set;
        }

        public List<string> Hosts { get; private set; } = new List<string>();

        public DelegateCommand LoginCommand { get; private set; }

        [Required(ErrorMessage = @"Поле не может быть пустым")]
        public string DataBase { get; set; }

        [Required(ErrorMessage = @"Поле не может быть пустым")]
        public string Password { get; set; }

        /*
        public static LoginViewModel Create()
        {
            return ViewModelSource.Create(() => new LoginViewModel());
        }
        */

        
        public LoginViewModel()
        {
            var appSettings = GammaSettings.Get();
            Hosts = appSettings.Hosts;
            if (Hosts.Count > 0 && !Hosts.Contains(appSettings.HostName)) Host = Hosts.First();
            else
            Host = string.IsNullOrEmpty(appSettings.HostName) ? "gamma" : appSettings.HostName;
            DataBase = string.IsNullOrEmpty(appSettings.DbName) ? "gammanew" : appSettings.DbName;
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

            var checkResult = DB.CheckCurrentVersion();
            var resultMessage = checkResult?.ResultMessage;
            if (checkResult == null || !(string.IsNullOrWhiteSpace(resultMessage) && !checkResult.BlockCreation))
            {
                if (checkResult == null && checkResult.BlockCreation)
                {
                    MessageBox.Show(resultMessage, "Проверка версии программы", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                else
                {
                    if (checkResult == null)
                        resultMessage = "Ошибка при проверке версии программы. (" + GammaSettings.Version + ")";
                    var dialogResult = MessageBox.Show(resultMessage + Environment.NewLine + " Продолжить работу?", "Проверка версии программы", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (dialogResult == MessageBoxResult.No)
                    {
                        return;
                    }

                }
            }
            MessageManager.OpenMain();
            CloseWindow();
        }

        public bool UseScanner { get; set; }

        public override void Dispose()
        {
            base.Dispose();
            LoginCommand = null;
            
        }

    }
    
}