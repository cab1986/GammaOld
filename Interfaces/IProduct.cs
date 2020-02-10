using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Interfaces
{
    interface IProduct
    {
        Guid ProductID { get; set; }
        string Number { get; set; }
        ProductKind ProductKind { get; set; }
    }
}
