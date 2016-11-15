using System;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class MovementProduct : ViewModelBase
    {
        public Guid DocMovementId { get; set; }
        public Guid ProductId { get; set; }
        public ProductKind ProductKind { get; set; }
        public string Number { get; set; }
        public decimal Quantity { get; set; }
        public bool IsShipped { get; set; }
        public bool IsAccepted { get; set; }

        private bool? _isConfirmed;
        
        public string NomenclatureName { get; set; }
        public Guid? NomenclatureId { get; set; }
        public Guid? CharacteristicId { get; set; }

        public bool? IsConfirmed
        {
            get { return _isConfirmed; }
            set
            {
                _isConfirmed = value;
                RaisePropertyChanged("IsConfirmed");
            }
        }
    }
}