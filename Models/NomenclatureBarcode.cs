// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace Gamma.Models
{
    public class NomenclatureBarcode
    {
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public string CharacteristicName { get; set; }
        public int BarcodeTypeId { get; set; }
        public string Barcode { get; set; }
    }
}
