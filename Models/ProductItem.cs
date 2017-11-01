using System;

namespace Gamma.Models
{
    public class ProductItem
    {
        public ProductItem() { }

        public ProductItem(Guid nomenclatureId, Guid characteristicId, int quantity, string nomenclatureName, Guid? productItemId = null)
        {
            NomenclatureId = nomenclatureId;
            CharacteristicId = characteristicId;
            Quantity = quantity;
            NomenclatureName = nomenclatureName;
            ProductItemId = productItemId ?? SqlGuidUtil.NewSequentialid();
        }

        public Guid ProductItemId { get; set; }
        public string NomenclatureName { get; set; }
        public int Quantity { get; set; }
        public Guid CharacteristicId { get; set; }
        public Guid NomenclatureId { get; set; }
    }
}
