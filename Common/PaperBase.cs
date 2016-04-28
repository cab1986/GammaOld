using System;
using DevExpress.Mvvm;

namespace Gamma.Common
{
    public class PaperBase : ViewModelBase
    {
        public byte? BreakNumber { get; set; }
        public int? PlaceProductionid { get; set; }
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
