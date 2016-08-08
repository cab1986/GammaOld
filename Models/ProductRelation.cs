using System;

namespace Gamma.Models
{
    public class ProductRelation
    {
        public string Description { get; set;}
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public Guid DocID { get; set; }
        public Guid ProductID { get; set; }
        public Gamma.DocTypes DocType { get; set; }
        public int? ProductKindID { get; set; }
    }
}
