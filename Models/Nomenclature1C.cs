using System;

namespace Gamma.Models
{
    public class Nomenclature1C
    {
        public string Name { get; set; }
        public Guid NomenclatureID { get; set; }

        protected bool Equals(Nomenclature1C other)
        {
            return NomenclatureID.Equals(other.NomenclatureID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Nomenclature1C) obj);
        }

        public override int GetHashCode()
        {
            return NomenclatureID.GetHashCode();
        }
    }
}
