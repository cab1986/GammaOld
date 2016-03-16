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
using System.ComponentModel;

namespace Gamma
{
    public class UIAuthBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            if (this.AssociatedObject.DataContext == null)
                this.AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;
            else
            {
 //               (this.AssociatedObject.DataContext as INotifyPropertyChanged).PropertyChanged += UIAuthBehavior_PropertyChanged;
                SetReadStatus();
            }
                
                
        }

        void AssociatedObject_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.AssociatedObject.DataContext != null)
            {
                SetReadStatus();
 //               (this.AssociatedObject.DataContext as INotifyPropertyChanged).PropertyChanged += UIAuthBehavior_PropertyChanged;
                this.AssociatedObject.DataContextChanged -= AssociatedObject_DataContextChanged;
            }
        }

        private void UIAuthBehavior_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsReadOnly") SetReadStatus();
        }

        private void SetReadStatus()
        {
            var Control = this.AssociatedObject;
            if (Control.DataContext.GetType().GetInterfaces().Contains(typeof(ICheckedAccess)))
            {
                if (!(Control.DataContext as ICheckedAccess).IsReadOnly) return;
                object context = AssociatedObject.DataContext;
                Type contextType = context.GetType();
                PropertyInfo[] properties = contextType.GetProperties();
                WalkDownLogicalTree(Control, properties);
            }
        }
        private void WalkDownLogicalTree(FrameworkElement Control, PropertyInfo[] properties)
        {
            foreach (var element in LogicalTreeHelper.GetChildren(Control))
            {
                var frameElement = element as FrameworkElement;
                if (frameElement == null) continue;
                WalkDownLogicalTree(frameElement,properties);
                if (frameElement is TextBox || frameElement is BaseEdit)
                {
                    var Binding = new Binding();
                    if (frameElement is BaseEdit)
                    {
                        Binding = BindingOperations.GetBinding((DependencyObject)frameElement, BaseEdit.EditValueProperty);
                    }
                    else
                    {
                        Binding = BindingOperations.GetBinding((DependencyObject)frameElement, TextBox.TextProperty);
                    }
                    if (Binding != null)
                    {
                        PropertyInfo bounded = properties.Where(x => x.Name == Binding.Path.Path).FirstOrDefault();
                        if (bounded != null)
                        {
                            foreach (var attr in bounded.GetCustomAttributes(true))
                            {
                                var UIAuthAttr = attr as UIAuthAttribute;
                                if (UIAuthAttr != null)
                                {
                                    switch (UIAuthAttr.AuthLevel)
                                    {
                                        case UIAuthLevel.Invisible:
                                            (frameElement as Control).Visibility = Visibility.Collapsed;
                                            break;
                                        case UIAuthLevel.ReadOnly:
                                            if (frameElement is BaseEdit) (frameElement as BaseEdit).IsReadOnly = true;
                                            else (frameElement as TextBox).IsReadOnly = true;
                                            break;
                                        default:
                                            (frameElement as Control).Visibility = Visibility.Visible;
                                            (frameElement as Control).IsEnabled = true;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
