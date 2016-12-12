using System;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class PlaceZone : ViewModelBase
    {
        private string _name;
        public Guid PlaceZoneId { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                PlaceZoneChanged?.Invoke();
                RaisePropertyChanged("Name");
            }
        }

        public Guid? PlaceZoneParentId { get; set; }

        public event Action PlaceZoneChanged;
    }
}
