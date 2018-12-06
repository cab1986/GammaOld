// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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
        public string OutPerson { get; set; }

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

        public string ProductKindName { get; set; }
        public string OrderTypeName { get; set; }
        public string InPlaceName { get; set; }
        public string InPlaceZoneName { get; set; }
        public DateTime? InDate { get; set; }
        public string OutPlaceName { get; set; }
        public string OutPlaceZoneName { get; set; }
        public DateTime? OutDate { get; set; }

    }
}