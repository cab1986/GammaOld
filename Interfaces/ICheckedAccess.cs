using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Interfaces
{
    interface ICheckedAccess
    {
        bool IsReadOnly {get; set;}

        bool CheckAccess();
    }
}
