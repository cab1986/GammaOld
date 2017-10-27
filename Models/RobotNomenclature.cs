// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace Gamma.Models
{
    public class RobotNomenclature
    {
        public int ProdNumber { get; set; }
        public string ProdDescription { get; set; }
        public string EANFullPallet { get; set; }
        public int ProductionLine { get; set; }
        public int PlaceID { get; set; }
        public string ProdName { get; set; }
    }
}
