using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Interactivity;
using DevExpress.Xpf.Grid;
using System.Windows;
using DevExpress.Xpf.Core.Commands;

namespace DxSample
{
    class DataNavigatorBehavior : Behavior<TableView>
    {
        public static readonly DependencyProperty AddNewRowCommandProperty = DependencyProperty.RegisterAttached("AddNewRowCommand", typeof(DelegateCommand<object>), typeof(DataNavigatorBehavior), null);

        public static DelegateCommand<object> GetAddNewRowCommand(TableView target)
        {
            return (DelegateCommand<object>)target.GetValue(AddNewRowCommandProperty);
        }

        public static void SetAddNewRowCommand(TableView target, DelegateCommand<object> value)
        {
            target.SetValue(AddNewRowCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            TableView view = this.AssociatedObject as TableView;
            view.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("Dictionary.xaml", UriKind.Relative) });
            view.SetValue(DataNavigatorBehavior.AddNewRowCommandProperty, new DelegateCommand<object>(AddNewRow));
        }

        private void AddNewRow(object arg)
        {
            (this.AssociatedObject as TableView).AddNewRow();
        }
    }
}