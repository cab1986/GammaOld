using System;
using System.Collections.Generic;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class DowntimeTemplate : ViewModelBase
    { 
        public Guid? DowntimeTemplateID { get; set; }
        public Guid DowntimeTypeID { get; set; }
        public Guid? DowntimeTypeDetailID { get; set; }
        public string DowntimeType { get; set; }
        public string DowntimeTypeDetail { get; set; }
        public string Comment { get; set; }
        public string Name { get; set; }
        public Guid EquipmentNodeID { get; set; }
        public Guid? EquipmentNodeDetailID { get; set; }
        public string EquipmentNode { get; set; }
        public string EquipmentNodeDetail { get; set; }
        public int? PlaceID { get; set; }
        public string PlaceName { get; set; }
        public int? Duration { get; set; }

    }
}