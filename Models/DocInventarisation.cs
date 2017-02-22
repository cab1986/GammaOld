using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Models
{
    public class DocInventarisation
    {
        public Guid DocId { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Warehouse { get; set; }
    }
}
