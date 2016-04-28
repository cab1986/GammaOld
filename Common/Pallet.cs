using System;

namespace Gamma.Common
{
    public class Pallet
    {
        public string NomenclatureName { get; set; }
        public int Quantity { get; set; }
        public Guid ProductId { get; set; }
        /// <summary>
        /// ID документа выработки
        /// </summary>
        public Guid DocId { get; set; }
        public Guid NomenclatureID { get; set; }
        public Guid CharacteristicID { get; set; }
        public string Number { get; set; }
    }
}
