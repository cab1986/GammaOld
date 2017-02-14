namespace Gamma.Models
{
    public class InventarisationItem
    {
        public string Barcode { get; set; }
        public string NomenclatureName { get; set; }
        public decimal? Quantity { get; set; }
        public string MeasureUnit { get; set; }
    }
}
