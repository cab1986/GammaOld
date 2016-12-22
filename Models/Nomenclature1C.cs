// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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
