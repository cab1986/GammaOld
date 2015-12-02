using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma
{
    public class DocCloseShift
    {
        public Guid DocCloseShiftID { get; set; }
        public DateTime Date { get; set; }
        public byte ShiftID { get; set; }
        public string Place { get; set; }
    }
}
