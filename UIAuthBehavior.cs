// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Gamma.Interfaces;
using System.Windows.Data;
using DevExpress.Xpf.Editors;
using System.Reflection;
using Gamma.Attributes;
using System.ComponentModel;
using DevExpress.Mvvm.UI.Interactivity;

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

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.DataContextChanged -= AssociatedObject_DataContextChanged;
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
            var control = this.AssociatedObject;
            if (control.DataContext.GetType().GetInterfaces().Contains(typeof(ICheckedAccess)))
            {
                if (!(control.DataContext as ICheckedAccess).IsReadOnly) return;
                object context = AssociatedObject.DataContext;
                Type contextType = context.GetType();
                PropertyInfo[] properties = contextType.GetProperties();
                WalkDownLogicalTree(control, properties);
            }
        }
        private void WalkDownLogicalTree(FrameworkElement control, PropertyInfo[] properties)
        {
            foreach (var element in LogicalTreeHelper.GetChildren(control))
            {
                var frameElement = element as FrameworkElement;
                if (frameElement == null) continue;
                WalkDownLogicalTree(frameElement,properties);
                if (frameElement is TextBox || frameElement is BaseEdit)
                {
                    var binding = new Binding();
                    if (frameElement is BaseEdit)
                    {
                        binding = BindingOperations.GetBinding((DependencyObject)frameElement, BaseEdit.EditValueProperty);
                    }
                    else
                    {
                        binding = BindingOperations.GetBinding((DependencyObject)frameElement, TextBox.TextProperty);
                    }
                    if (binding != null)
                    {
                        PropertyInfo bounded;
                        var path = binding.Path.Path;
                        if (path.Contains("[") && path.Contains("]"))
                        {
                            var tempPath = path.Substring(0, path.IndexOf("["));
                            var arrayProperty =
                                properties.FirstOrDefault(p => p.PropertyType.IsArray && p.Name.StartsWith(tempPath));
                            if (arrayProperty == null) return;
                            tempPath = path.Substring(path.IndexOf(".")+1, path.Length - path.IndexOf(".")-1);
                            bounded = arrayProperty.PropertyType.GetElementType().GetProperty(tempPath);
                        }
                        else bounded = properties.FirstOrDefault(x => x.Name == path);
                        if (bounded != null)
                        {
                            foreach (var attr in bounded.GetCustomAttributes(true))
                            {
                                var uiAuthAttr = attr as UIAuthAttribute;
                                if (uiAuthAttr != null)
                                {
                                    switch (uiAuthAttr.AuthLevel)
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
