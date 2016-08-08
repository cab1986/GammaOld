using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using DevExpress.Mvvm;
using Gamma.Common;

namespace Gamma.Models
{
    public class BrokeProduct : ViewModelBase
    {
        public BrokeProduct(ItemsChangeObservableCollection<RejectionReason> rejectionReasons)
        {
            RejectionReasons = rejectionReasons;
            RejectionReasonsString = FormRejectionReasonsString(RejectionReasons);
            RejectionReasons.CollectionChanged += FormRejectionReasonString;
        }

        private void FormRejectionReasonString(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (RejectionReasons == null) return;
            RejectionReasonsString = FormRejectionReasonsString(RejectionReasons);
        }

        public Guid ProductId { get; set; }
        public Gamma.ProductKinds ProductKind { get; set; }
        public string NomenclatureName { get; set; }
        public string Number { get; set; }
        public string BaseMeasureUnit { get; set; }
        public decimal Quantity { get; set; }

        private string _rejectionReasonsString;

        public string RejectionReasonsString
        {
            get { return _rejectionReasonsString; }
            set
            {
                _rejectionReasonsString = value;
                RaisePropertyChanged("RejectionReasonsString");
            }
        }

        public DateTime? Date { get; set; }
        public string Place { get; set; }
        public int ShiftId { get; set; }
        public string PrintName { get; set; }

        public ItemsChangeObservableCollection<RejectionReason> RejectionReasons { get; }

        private string FormRejectionReasonsString(ObservableCollection<RejectionReason> list, GammaEntities gammaDb = null)
        {
            var sbuilder = new StringBuilder();
            using (var gammaBase = DB.GammaDb)
            {
                foreach (var reason in list)
                {
                    var description =
                        gammaBase.C1CRejectionReasons.FirstOrDefault(
                            r => r.C1CRejectionReasonID == reason.RejectionReasonID)?.Description;
                    if (description == null) continue;
                    sbuilder.Append(description);
                    sbuilder.Append(Environment.NewLine);
                }
            }
                
            return sbuilder.ToString();
        }
    }
}