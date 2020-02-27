using System;
using System.Collections.Generic;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class Sample : ViewModelBase
        {
            public Guid? ProductionTaskConvertingSampleID { get; set; }
            public Guid NomenclatureID { get; set; }
            public Guid? CharacteristicID { get; set; }
            public string NomenclatureName { get; set; }
            public Guid? MeasureUnitId { get; set; }
            public string MeasureUnit { get; set; }
            public decimal Quantity { get; set; }
            public DateTime? Date { get; set; }
            public int? ShiftID { get; set; }
            public Dictionary<Guid, string> MeasureUnits { get; set; }

            protected bool Equals(Sample other)
            {
                return NomenclatureID.Equals(other.NomenclatureID) && CharacteristicID.Equals(other.CharacteristicID);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Sample)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (NomenclatureID.GetHashCode() * 397) ^ CharacteristicID.GetHashCode();
                }
            }
        }
}
