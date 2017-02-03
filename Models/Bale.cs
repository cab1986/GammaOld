using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Models
{
    public class Bale
    {
        public Guid ProductId { get; set; }
        public string Number { get; set; }
        public string NomenclatureName { get; set; }
        public decimal Weight { get; set; }
    }
}
