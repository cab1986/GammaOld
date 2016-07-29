using System;
using System.ComponentModel;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class BrokeDecisionProduct : ViewModelBase
    {
        public decimal MaxQuantity { get; set; }

        public Guid ProductId { get; set; }

        private Gamma.ProductStates _productState;
        public Gamma.ProductStates ProductState
        {
            get { return _productState; }
            set
            {
                _productState = value;
                Decision = _productState.GetAttributeOfType<DescriptionAttribute>().Description;
            }
        }
        public string NomenclatureName { get; set; }

        private decimal _quantity;

        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                RaisePropertyChanged("Quantity");
            }
            
        }
        public string Comment { get; set; }
        public string Number { get; set; }
        public string Decision { get; private set; }
        public Guid? NomenclatureId { get; set; }
        public Guid? CharacteristicId { get; set; }
    }
}