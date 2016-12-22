// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace Gamma.Models
{
    public class Nomenclature
    {
        protected bool Equals(Nomenclature other)
        {
            return NomenclatureID.Equals(other.NomenclatureID) && CharacteristicID.Equals(other.CharacteristicID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Nomenclature) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (NomenclatureID.GetHashCode()*397) ^ CharacteristicID.GetHashCode();
            }
        }

        public Guid? NomenclatureID { get; set; }
        public string NomenclatureName { get; set; }
        public Guid? CharacteristicID { get; set; }
        public string CharacteristicName { get; set; }
    }
}
