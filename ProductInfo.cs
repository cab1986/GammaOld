using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma
{
    public class ProductInfo
    {
        public Guid DocProductID { get; set; }
        public Guid ProductID { get; set; }
        public ProductKinds ProductKind { get; set; }
        public string Number { get; set; }
        public DateTime? Date { get; set; }
        public string NomenclatureName { get; set; }
        public Guid? NomenclatureID { get; set; }
        public Guid? CharacteristicID { get; set; }
        public int? Quantity { get; set; }
        public Byte? ShiftID { get; set; }
        public ProductStatesFilter State { get; set; }
        public string Place { get; set; }
    }
}
