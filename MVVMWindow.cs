// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Common;

namespace Gamma
{
    public class MvvmWindow : Window
    {
        public MvvmWindow()
        {
            UIServices.SetBusyState();
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
