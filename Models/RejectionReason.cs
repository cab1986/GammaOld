using System;
using System.Linq;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class RejectionReason: ViewModelBase
    {
        private Guid _rejectionReasonId;
        public Guid RejectionReasonID
        {
            get { return _rejectionReasonId; }
            set
            {
                _rejectionReasonId = value;               
                RaisePropertyChanged("RejectionReasonID");
            }
        }

        public string Description { get; set; }

        public string FullDescription { get; set; }
        public string Comment { get; set; }
    }
}
