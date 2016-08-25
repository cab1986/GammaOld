using System.Globalization;
using System.Threading;
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
            var appSettings = GammaSettings.Get();
            DevExpress.Xpf.Core.ApplicationThemeHelper.UpdateApplicationThemeName();
            // Create a new object, representing the German culture. 
            CultureInfo culture = CultureInfo.CreateSpecificCulture("ru-RU");

            // The following line provides localization for the application's user interface. 
            Thread.CurrentThread.CurrentUICulture = culture;

            // The following line provides localization for data formats. 
            Thread.CurrentThread.CurrentCulture = culture;
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
