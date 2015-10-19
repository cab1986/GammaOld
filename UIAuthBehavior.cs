using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Gamma.Interfaces;
using System.Windows.Data;
using DevExpress.Xpf.Editors;
using System.Reflection;
using Gamma.Attributes;
using System.Windows.Media;

namespace Gamma
{
    public class UIAuthBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            SetReadStatus();
        }

        private void SetReadStatus()
        {
            var Control = this.AssociatedObject;
            if (Control.DataContext.GetType().GetInterfaces().Contains(typeof(ICheckedAccess)))
            {
                object context = AssociatedObject.DataContext;
                Type contextType = context.GetType();
                PropertyInfo[] properties = contextType.GetProperties();
                bool ReadOnly = (Control.DataContext as ICheckedAccess).IsReadOnly;
                var ChildCount = VisualTreeHelper.GetChildrenCount(Control);
                for (int i = 0; i < ChildCount; i++)
                {
                    var Element = VisualTreeHelper.GetChild(Control, i);
                    if (Element is TextBox || Element is TextEdit)
                    {
                        var Binding = new Binding();
                        if (Element is TextEdit)
                        {
                            Binding = BindingOperations.GetBinding((DependencyObject)Element, TextEdit.EditValueProperty);
                        }
                        else
                        {
                            Binding = BindingOperations.GetBinding((DependencyObject)Element, TextBox.TextProperty);
                        }
                        PropertyInfo bounded = properties.Where(x => x.Name == Binding.Path.Path).FirstOrDefault();
                        foreach (var attr in bounded.GetCustomAttributes(true))
                        {
                            var UIAuthAttr = attr as UIAuthAttribute;
                            if (UIAuthAttr != null)
                            {
                                if (UIAuthAttr.AuthLevel == UIAuthLevel.Invisible)
                                {
                                    (Element as Control).Visibility = Visibility.Hidden;
                                }
                                else if (UIAuthAttr.AuthLevel == UIAuthLevel.ReadOnly)
                                {
                                    (Element as Control).IsEnabled = false;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
