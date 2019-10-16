using System;

namespace Gamma.Models
{
    public class InventarisationItem
    {
        public string Number { get; set; }
        public string NomenclatureName { get; set; }
        public decimal? Quantity { get; set; }
        public string MeasureUnit { get; set; }
        public Guid? ProductID { get; set; }
        public ProductKind? ProductKind { get; set; }
    }
}
