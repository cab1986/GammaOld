using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Common
{
    public class ProductionTaskBatch
    {
        public DateTime? Date { get; set; }
        public Guid ProductionTaskBatchID { get; set; }
        public DateTime? DateBegin { get; set; }
        public string Nomenclature { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? MadeQuantity { get; set; }
        public string Number { get; set; }
        public string State { get; set; }
        public byte BatchKindID { get; set; }
        public int PlaceID { get; set; }
        public string Place { get; set; }
    }
}
