using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamma
{
    public class PaperBase : ViewModelBase
    {
        public int? PlaceProductionID { get; set; }
        public DateTime Date { get; set; }
        public Guid DocID { get; set; }
        public Guid ProductID { get; set; }
        public Guid CharacteristicID { get; set; }
        public Guid NomenclatureID { get; set; }
        public string Number { get; set; }
        public string Nomenclature { get; set; }
        private decimal _weight;
        public decimal Weight
        {
            get { return _weight; }
            set
            {
                _weight = value;
                RaisePropertyChanged("Weight");
            }
        }
        public int Diameter { get; set; }
    }
}
