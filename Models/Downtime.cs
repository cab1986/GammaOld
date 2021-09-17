using System;
using System.Collections.Generic;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class Downtime : ViewModelBase
    {
        public Guid? ProductionTaskConvertingDowntimeID { get; set; }
        public Guid DowntimeTypeID { get; set; }
        public Guid? DowntimeTypeDetailID { get; set; }
        public string DowntimeType { get; set; }
        public string DowntimeTypeDetail { get; set; }
        public string Comment { get; set; }
        public int Duration { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public int? ShiftID { get; set; }
        public Guid? ProductionTaskID { get; set; }
        public string ProductionTaskNumber { get; set; }
        public string ProductionTaskGroup => ProductionTaskID == null ? "Общие" : "По заданию на производство";
        public Guid EquipmentNodeID { get; set; }
        public Guid? EquipmentNodeDetailID { get; set; }
        public string EquipmentNode { get; set; }
        public string EquipmentNodeDetail { get; set; }

        //protected bool Equals(Sample other)
        //{
        //    return NomenclatureID.Equals(other.NomenclatureID) && CharacteristicID.Equals(other.CharacteristicID);
        //}

        //public override bool Equals(object obj)
        //{
        //    if (ReferenceEquals(null, obj)) return false;
        //    if (ReferenceEquals(this, obj)) return true;
        //    if (obj.GetType() != this.GetType()) return false;
        //    return Equals((Sample)obj);
        //}

        //public override int GetHashCode()
        //{
        //    unchecked
        //    {
        //        return (NomenclatureID.GetHashCode() * 397) ^ CharacteristicID.GetHashCode();
        //    }
        //}
    }
}
