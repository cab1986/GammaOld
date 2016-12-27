// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Gamma.Common
{
    public class ItemsChangeObservableCollection<T> :
           ObservableCollection<T> //where T : INotifyPropertyChanged
    {
        public ItemsChangeObservableCollection(IEnumerable<T> enumerable) //: base(enumerable)
        {
            foreach (var item in enumerable)
            {
                Add(item);
            }
        }

        public ItemsChangeObservableCollection()
        { }
         
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    RegisterPropertyChanged(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    UnRegisterPropertyChanged(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    UnRegisterPropertyChanged(e.OldItems);
                    RegisterPropertyChanged(e.NewItems);
                    break;
            }

            base.OnCollectionChanged(e);
        }

        protected override void ClearItems()
        {
            UnRegisterPropertyChanged(this);
            base.ClearItems();
        }

        private void RegisterPropertyChanged(IList items)
        {
            foreach (INotifyPropertyChanged item in items)
            {
                if (item != null)
                {
                    item.PropertyChanged += item_PropertyChanged;
                }
            }
        }

        private void UnRegisterPropertyChanged(IList items)
        {
            foreach (INotifyPropertyChanged item in items)
            {
                if (item != null)
                {
                    item.PropertyChanged -= item_PropertyChanged;
                    item.PropertyChanged -= ItemChanged;
                }
            }
        }

        public event PropertyChangedEventHandler ItemChanged;

        private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            ItemChanged?.Invoke(sender, e);
        }
    }
}
