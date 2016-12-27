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
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;

namespace Gamma
{
    public class UIAuthBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Unloaded += AssociatedObjectUnloaded;
            if (this.AssociatedObject.DataContext == null)
                this.AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;
            else
            {
 //               (this.AssociatedObject.DataContext as INotifyPropertyChanged).PropertyChanged += UIAuthBehavior_PropertyChanged;
                Initialize();
            }              
        }

        private void AssociatedObjectUnloaded(object sender, RoutedEventArgs e)
        {
            CleanUp();
        }

        private void Initialize()
        {
            SetReadStatus();
            var context = this.AssociatedObject.DataContext as INotifyPropertyChanged;
            if (context == null) return;
            context.PropertyChanged += UIAuthBehavior_PropertyChanged;
        }

        private void CleanUp()
        {
            AssociatedObject.DataContextChanged -= AssociatedObject_DataContextChanged;
            var context = this.AssociatedObject.DataContext as INotifyPropertyChanged;
            if (context == null) return;
            context.PropertyChanged -= UIAuthBehavior_PropertyChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            CleanUp();
        }

        void AssociatedObject_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.AssociatedObject.DataContext != null)
            {
                Initialize();
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
            if (!control.DataContext.GetType().GetInterfaces().Contains(typeof (ICheckedAccess))) return;
//                if (!(control.DataContext as ICheckedAccess).IsReadOnly) return;
            var checkedContext = control.DataContext as ICheckedAccess;
            if (checkedContext == null) return;
            IsReadOnly = checkedContext.IsReadOnly;
            object context = AssociatedObject.DataContext;
            Type contextType = context.GetType();
            PropertyInfo[] properties = contextType.GetProperties();
            WalkDownLogicalTree(control, properties);
        }

        private bool IsReadOnly { get; set; }

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
                                            (frameElement as Control).Visibility = IsReadOnly ? Visibility.Collapsed : Visibility.Visible;
                                            break;
                                        case UIAuthLevel.ReadOnly:
                                            if (frameElement is BaseEdit) (frameElement as BaseEdit).IsReadOnly = IsReadOnly;
                                            else (frameElement as TextBox).IsReadOnly = IsReadOnly;
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
