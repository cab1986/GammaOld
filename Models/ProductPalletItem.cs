using System;

namespace Gamma.Models
{
    public class ProductPalletItem
    {
        public ProductPalletItem() { }

        public ProductPalletItem(Guid nomenclatureId, Guid characteristicId, int quantity, string nomenclatureName, Guid? productPalletItemId = null)
        {
            NomenclatureId = nomenclatureId;
            CharacteristicId = characteristicId;
            Quantity = quantity;
            NomenclatureName = nomenclatureName;
            ProductPalletItemId = productPalletItemId ?? SqlGuidUtil.NewSequentialid();
        }

        public Guid ProductPalletItemId { get; set; }
        public string NomenclatureName { get; set; }
        public int Quantity { get; set; }
        public Guid CharacteristicId { get; set; }
        public Guid NomenclatureId { get; set; }
    }
}
