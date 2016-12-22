// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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
