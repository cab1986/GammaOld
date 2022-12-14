// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Diagnostics;
using System.Windows;

namespace Gamma
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.DataBindingSource.Listeners.Add(new DebugTraceListener());
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;
#endif
#if (!DEBUG)
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs args)
            {
                var exception = args.ExceptionObject as Exception;
                if (exception == null)
                {
                    MessageBox.Show("Exception: " + args.ExceptionObject.ToString(),
                        "Ошибка приложения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(exception.InnerException != null ? exception.InnerException.ToString() : exception.ToString(), 
                        "Ошибка приложения", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                }
//                Environment.Exit(0);
                Trace.WriteLine("Global exception: " + args.ExceptionObject.ToString());
                
            };
#endif
//            Application.ThreadException += delegate(Object sender, ThreadExceptionEventArgs args)
//           {
//                Trace.WriteLine("Global exception: " + args.Exception.ToString());
//               Environment.Exit(0);
//            };

            base.OnStartup(e);
        }
/*        public static bool IsInDesignMode
        {
            get
            {
                return (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
            }
        }
        */
    }

    public class DebugTraceListener : TraceListener
    {
        public override void Write(string message)
        {
        }

        public override void WriteLine(string message)
        {
#if DEBUG
            Debugger.Break();
#endif
        }
    }
}
