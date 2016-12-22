// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class QualityReportItem : ViewModelBase
    {
        private bool _isSpoolBroke;
        private bool _isGroupPackBroke;
        public DateTime Date { get; set; }
        public byte ShiftId { get; set; }
        public string NomenclatureName { get; set; }
        public string SpoolNumber { get; set; }
        public string GroupPackNumber { get; set; }
        public decimal GroupPackWeight { get; set; }

        public bool IsSpoolBroke
        {
            get { return _isSpoolBroke; }
            set
            {
                _isSpoolBroke = value;
                RaisePropertyChanged("IsSpoolBroke");
            }
        }

        public bool IsGroupPackBroke
        {
            get { return _isGroupPackBroke; }
            set
            {
                _isGroupPackBroke = value;
                RaisePropertyChanged("IsGroupPackBroke");
            }
        }
    }
}
