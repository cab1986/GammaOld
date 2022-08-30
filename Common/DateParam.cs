using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Common
{
    public class DateParam
    {
        public DateParam(string labelDate = null, DateTime? defaultDate = null, DateTime? minDate = null, DateTime? maxDate = null)
        {
            Label = labelDate;
            DefaultDate = defaultDate;
            MinDate = minDate;
            MaxDate = maxDate;
        }

        public string Label { get; private set; }
        public DateTime? DefaultDate { get; private set; }
        public DateTime? MinDate { get; private set; }
        public DateTime? MaxDate { get; private set; }
    }
}
