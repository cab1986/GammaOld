﻿using System;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class ProductInfo : ViewModelBase
    {
        public Guid DocID { get; set; }

        private bool _isConfirmed;

        public bool IsConfirmed
        {
            get { return _isConfirmed; }
            set
            {
                _isConfirmed = value; 
                RaisePropertyChanged("IsConfirmed");
            }   
        }
        public Guid ProductID { get; set; }
        public ProductKind ProductKind { get; set; }
        public string Number { get; set; }
        public DateTime? Date { get; set; }
        public string NomenclatureName { get; set; }
        public Guid? NomenclatureID { get; set; }
        public Guid? CharacteristicID { get; set; }
        public decimal? Quantity { get; set; }
        public byte? ShiftID { get; set; }
        public string State { get; set; }
        public string Place { get; set; }
        public int? PlaceID { get; set; }
        public PlaceGroup PlaceGroup { get; set; }
    }
}