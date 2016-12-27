// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class QualityReportItem : ViewModelBase
    {
        private bool _isBroke;
        public Guid ProductId { get; set; }
        public Guid? ProductGroupPackId { get; set; }
        public DateTime Date { get; set; }
        public byte ShiftId { get; set; }
        public string NomenclatureName { get; set; }
        public string SpoolNumber { get; set; }
        public string GroupPackNumber { get; set; }
        public decimal Weight { get; set; }

        public bool IsBroke
        {
            get { return _isBroke; }
            set
            {
                if (_isBroke == value) return;
                _isBroke = value;
                RaisePropertyChanged("IsBroke");
            }
        }
    }
}
