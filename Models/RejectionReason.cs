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

        private string _comment;

        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                RaisePropertyChanged("Comment");
            }
        }
    }
}
