using System.Windows;

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
            Gamma.Properties.Settings.Default.Save();
            GammaSettings.Serialize();
        }
        private void Application_DispatcherUnhandledException(object sender,
                       System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
                //Handling the exception within the UnhandledException handler.
            var message = "";
            if (e.Exception.InnerException != null) message = e.Exception.InnerException.ToString();
            else
            {
                message = e.Exception.ToString();
            }
                MessageBox.Show(message, "Exception Caught",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
        }
    }
}
