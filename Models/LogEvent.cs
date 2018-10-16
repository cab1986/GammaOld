using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Models
{
    public class LogEvent
    {
        public Guid EventID { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string PrintName { get; set; }
        public string Place { get; set; }
        public string EventKind { get; set; }
        public string Device { get; set; }
        public Boolean IsSolved { get; set; }
        public string Shift { get; set; }
        public Guid? ParentEventID { get; set; }
        public string Department { get; set; }
        public string EventState { get; set; }
    }
}
