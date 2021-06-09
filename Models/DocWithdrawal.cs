using System;

namespace Gamma.Models
{
    public class DocWithdrawalsItem
    {
        public Guid DocId { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Place { get; set; }
        public byte? ShiftID { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
