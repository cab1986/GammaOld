using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Interfaces
{
    interface IValidationExceptionHandler
    {
        void ValidationExceptionsChanged(int count);
    }
}
