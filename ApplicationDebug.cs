using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs args)
            {
                MessageBox.Show("Exception: " + args.ExceptionObject.ToString(),
                    "Ошибка приложения", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
                Trace.WriteLine("Global exception: " + args.ExceptionObject.ToString());
            };
//            Application.ThreadException += delegate(Object sender, ThreadExceptionEventArgs args)
//           {
//                Trace.WriteLine("Global exception: " + args.Exception.ToString());
//               Environment.Exit(0);
//            };

            base.OnStartup(e);
        }
        public static bool IsInDesignMode
        {
            get
            {
                return (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
            }
        }
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
