// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace Gamma.Models
{
    public class Characteristic
    {
        protected bool Equals(Characteristic other)
        {
            return CharacteristicID.Equals(other.CharacteristicID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Characteristic) obj);
        }

        public override int GetHashCode()
        {
            return CharacteristicID.GetHashCode();
        }

        public Guid CharacteristicID { get; set; }
        public string CharacteristicName { get; set; }
    }
}