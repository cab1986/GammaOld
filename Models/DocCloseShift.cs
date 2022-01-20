// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace Gamma
{
    public class DocCloseShift
    {
        public Guid DocCloseShiftID { get; set; }
        public DateTime Date { get; set; }
        public byte ShiftID { get; set; }
        public string Place { get; set; }
        public string User { get; set; }
        public string Person { get; set; }
        public string Number { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime? LastUploadedTo1C { get; set; }
    }
}
