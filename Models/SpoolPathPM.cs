using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    class SpoolPathPM : ViewModelBase
    {
        public DateTime Date { get; set; }
        public int PMShiftId { get; set; }
        public string NomenclatureName { get; set; }
        public string SpoolNumber { get; set; }
        public string GroupPackNumber { get; set; }
        public decimal GroupPackWeight { get; set; }

        private bool _spoolIsCheckedForBroke;

        public bool SpoolIsCheckedForBroke
        {
            get { return _spoolIsCheckedForBroke; }
            set
            {
                _spoolIsCheckedForBroke = value;
                RaisePropertyChanged("SpoolIsCheckedForBroke");
            }
        }

        public bool GroupPackIsCheckedForBroke { get; set; }
    }
}
