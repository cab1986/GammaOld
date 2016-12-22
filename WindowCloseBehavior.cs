// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Windows;

namespace Gamma
{
    public static class WindowCloseBehavior
    {
        public static void SetClose(DependencyObject target, bool value)
        {
            target.SetValue(CloseProperty, value);
        }
        public static readonly DependencyProperty CloseProperty =
        DependencyProperty.RegisterAttached(
        "Close",
        typeof(bool),
        typeof(WindowCloseBehavior),
        new UIPropertyMetadata(false, OnClose));
        private static void OnClose(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool && ((bool)e.NewValue))
            {
                Window window = GetWindow(sender);
                window?.Close();
            }
        }
        private static Window GetWindow(DependencyObject sender)
        {
            Window window = null;
            if (sender is Window)
                window = (Window)sender;
            return window ?? (Window.GetWindow(sender));
        }
    }
}
