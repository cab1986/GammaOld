using System;

namespace Gamma.Common
{
    public class ProductRelation
    {
        public string RelationKind { get; set;}
        public string ProductKind { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public Guid DocID { get; set; }
        public Guid ProductID { get; set; }
        public ProductKinds ProductKindID { get; set; }
    }
}
