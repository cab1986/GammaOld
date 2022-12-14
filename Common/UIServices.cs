// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Gamma.Common
{
    /// <summary>
    ///   Contains helper methods for UI, so far just one for showing a waitcursor
    /// </summary>
    public static class UIServices
    {
        /// <summary>
        ///   A value indicating whether the UI is currently busy
        /// </summary>
        private static bool _isBusy;

        /// <summary>
        /// Sets the busystate as busy.
        /// </summary>
        public static void SetBusyState()
        {
            SetBusyState(true);
        }

        /// <summary>
        /// Sets the busystate to busy or not busy.
        /// </summary>
        /// <param name="busy">if set to <c>true</c> the application is now busy.</param>
        private static void SetBusyState(bool busy)
        {
            if (busy != _isBusy)
            {
                _isBusy = busy;
//                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => Mouse.OverrideCursor = busy ? Cursors.Wait : null));  
                Mouse.OverrideCursor = busy ? Cursors.Wait : null;
                if (_isBusy)
                {
                    new DispatcherTimer(TimeSpan.FromSeconds(0), DispatcherPriority.Background, dispatcherTimer_Tick, Application.Current.Dispatcher);
                }
            }
        }

        /// <summary>
        /// Handles the Tick event of the dispatcherTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private static void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var dispatcherTimer = sender as DispatcherTimer;
            if (dispatcherTimer == null) return;
            SetBusyState(false);
            dispatcherTimer.Stop();
        }
    }
}
