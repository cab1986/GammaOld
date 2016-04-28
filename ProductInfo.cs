using System;
using System.Linq;
using DevExpress.Mvvm;

namespace Gamma
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
                RaisePropertiesChanged("IsConfirmed");
            }   
        }
        public Guid ProductID { get; set; }
        public ProductKinds ProductKind { get; set; }
        public string Number { get; set; }
        public DateTime? Date { get; set; }
        public string NomenclatureName { get; set; }
        public Guid? NomenclatureID { get; set; }
        public Guid? CharacteristicID { get; set; }
        public int? Quantity { get; set; }
        public byte? ShiftID { get; set; }
        public string State { get; set; }
        public string Place { get; set; }
        private int? _placeID;
        public int? PlaceID 
        {
            get { return _placeID; } 
            set
            {
            	_placeID = value;
                PlaceGroup = (PlaceGroups)DB.GammaBase.Places.Where(p => p.PlaceID == value).Select(p => p.PlaceGroupID).FirstOrDefault();
            }
        }
        public PlaceGroups PlaceGroup { get; set; }
    }
}
