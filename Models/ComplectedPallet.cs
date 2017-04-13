using System;
using System.Collections.Generic;

namespace Gamma.Models
{
    public class ComplectedPallet
    {
        public Guid ProductId { get; set; }
        /// <summary>
        /// ID документа выработки
        /// </summary>
        public Guid DocId { get; set; }
        public string Number { get; set; }
        public Guid DocOrderId { get; set; }
        public List<PalletItem> PalletItems { get; set; }
        public string OrderNumber { get; set; }
        public DateTime Date { get; set; }
    }

    public class PalletItem
    {
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public string NomenclatureName { get; set; }
        public int Quantity { get; set; }
    }
}
