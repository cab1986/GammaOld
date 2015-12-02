using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
