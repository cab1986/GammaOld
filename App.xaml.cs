using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Gamma.Views;
using System.IO;

namespace Gamma
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var AppSettings = GammaSettings.Get();
            DevExpress.Xpf.Core.ApplicationThemeHelper.UpdateApplicationThemeName();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            GammaSettings.Serialize();
        }
    }
}
