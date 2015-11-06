using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Gamma
{
    public class MVVMWindow : Window
    {
        public MVVMWindow()
        {
            this.Closed += MVVMWindow_Closed;
        }

        void MVVMWindow_Closed(object sender, EventArgs e)
        {
            Messenger.Default.Unregister(this);
        }
        
    }
}
