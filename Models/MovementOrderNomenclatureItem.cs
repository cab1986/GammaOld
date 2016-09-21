using System;
using Gamma.ViewModels;

namespace Gamma.Models
{
    public class DocMovementOrderNomenclatureItem : DbEditItemWithNomenclatureViewModel
    {
        public Guid DocId { get; set; }
        public decimal Quantity { get; set; }
        public decimal CollectedQuantity { get; set; }
    }
}
