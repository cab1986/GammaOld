using System;
using System.Windows;
using DevExpress.Mvvm;

namespace Gamma
{
    public class MvvmWindow : Window
    {
        protected MvvmWindow(): base()
        {
            Closed += MVVMWindow_Closed;
        }

        void MVVMWindow_Closed(object sender, EventArgs e)
        {
            Closed -= MVVMWindow_Closed;
            Messenger.Default.Unregister(this);
            (DataContext as IDisposable)?.Dispose();
            DataContext = null;
        }
        
    }
}
