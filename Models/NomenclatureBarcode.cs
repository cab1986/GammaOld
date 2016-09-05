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
