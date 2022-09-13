using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Common;
using Gamma.ViewModels;
using DevExpress.Mvvm;


namespace Gamma.Models
{
    public class BrokePlace : DbEditItemWithNomenclatureViewModel
    {
        public BrokePlace()
        {
            BrokePlaceList = WorkSession.Places.Select(p => new Place
               {
                   PlaceID = p.PlaceID,
                   PlaceName = p.Name
               })
               .ToList();

            BrokeShiftsList = new List<KeyValuePair<byte, string>>();
            BrokeShiftsList.Add(new KeyValuePair<byte, string>(0, "Не сменный"));
            foreach (var shiftItem in WorkSession.Shifts)
            {
                BrokeShiftsList.Add(new KeyValuePair<byte, string>(shiftItem.ShiftID, shiftItem.Name));
            }
            
        }




        public string ForAllProductBrokeFIO { get; set; }

        public List<Place> BrokePlaceList { get; set; }
        private int? _placeID;

        public int? PlaceID
        {
            get { return _placeID; }
            set
            {
                _placeID = value;
                RaisePropertyChanged("PlaceID");
            }
        }
        private byte? _shiftID { get; set; }
        public byte? ForAllProductBrokeShiftID
        {
            get { return _shiftID; }
            set
            {
                _shiftID = value;
                RaisePropertyChanged("ForAllProductBrokeShiftID");
            }
        }
        private KeyValuePair<int, string> _shiftStateID { get; set; }
        public KeyValuePair<int, string> ShiftStateID
        {
            get { return _shiftStateID; }
            set
            {
                _shiftStateID = value;
                RaisePropertyChanged("ShiftStateID");
            }
        }
        public List<Place> ProductPlaceList { get; private set; }
        private List<KeyValuePair<byte, string>> _brokeShiftsList { get; set; }
        public List<KeyValuePair<byte, string>> BrokeShiftsList
        {
            get { return _brokeShiftsList; }
            set
            {
                _brokeShiftsList = value;
                RaisePropertyChanged("BrokeShiftsList");
            }
        }




        
    }
}
