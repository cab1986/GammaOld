// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace Gamma.Models
{
    public class Place
    {
        public Guid? PlaceGuid { get; set; }
        public int PlaceID { get; set; }
        public string PlaceName { get; set; }
    }
}
