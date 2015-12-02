using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma
{
    public class Spool : ViewModelBase
    {
        public Guid ProductID { get; set; }
        public Guid CharacteristicID { get; set; }
        public Guid NomenclatureID { get; set; }
        public string Number { get; set; }
        public string Nomenclature { get; set; }
        private int _weight;
        public int Weight
        {
            get { return _weight; }
            set
            {
                _weight = value;
                RaisePropertyChanged("Weight");
            }
        }
    }
}
