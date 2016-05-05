using System;
using System.Windows;
using DevExpress.Mvvm;

namespace Gamma
{
    public class MvvmWindow : Window
    {
        public MvvmWindow()
        {
            Closed += MVVMWindow_Closed;
        }

        void MVVMWindow_Closed(object sender, EventArgs e)
        {
            Messenger.Default.Unregister(this);
        }
        
    }
}
