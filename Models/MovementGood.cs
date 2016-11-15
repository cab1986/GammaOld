using System;
using System.Collections.Specialized;
using Gamma.Common;
using Gamma.ViewModels;

namespace Gamma.Models
{
    public class MovementGood : DbEditItemWithNomenclatureViewModel
    {
        
        private void ProductsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RaisePropertyChanged("Products");
        }
        

        public string Amount { get; set; }
        public decimal OutQuantity { get; set; }
        public decimal InQuantity { get; set; }
        
        private ItemsChangeObservableCollection<MovementProduct> _products;

        public ItemsChangeObservableCollection<MovementProduct> Products
        {
            get { return _products; }
            set
            {
                if (_products != null)
                    Products.CollectionChanged -= ProductsOnCollectionChanged;
                _products = value;
                if (_products != null)
                    Products.CollectionChanged += ProductsOnCollectionChanged;
                RaisePropertyChanged("Products");
            }
        }
    }
}
