// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Entities;

namespace Gamma.Models
{
    public class BrokeProduct : ViewModelBase
    {
        private BrokeProduct()
        {
            
        }

        public BrokeProduct(ItemsChangeObservableCollection<RejectionReason> rejectionReasons, int? brokePlaceId = null, byte? brokeShiftId = null
            , string brokePrintName = null) : this()
        {
            RejectionReasons = rejectionReasons;
            RejectionReasonsString = FormRejectionReasonsString(RejectionReasons);
            RejectionReasonCommentsString = FormRejectionReasonCommentsString(RejectionReasons);
            RejectionReasons.CollectionChanged += FormRejectionReasonString;
            BrokePlaceId = brokePlaceId;
            BrokeShiftId = brokeShiftId;
            PrintName = brokePrintName;
        }

        private void FormRejectionReasonString(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (RejectionReasons == null) return;
            RejectionReasonsString = FormRejectionReasonsString(RejectionReasons);
            RejectionReasonCommentsString = FormRejectionReasonCommentsString(RejectionReasons);
        }

        public Guid ProductId { get; set; }
        public ProductKind ProductKind { get; set; }
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

        private string _rejectionReasonCommentsString;
        private byte? _shiftId;
        private int _productionPlaceId;
        private int? _brokePlaceId;

        public string RejectionReasonCommentsString
        {
            get { return _rejectionReasonCommentsString; }
            set
            {
                _rejectionReasonCommentsString = value;
                RaisePropertyChanged("RejectionReasonCommentsString");
            }
        }

        public DateTime? Date { get; set; }

        public int? BrokePlaceId
        {
            get { return _brokePlaceId; }
            set
            {
                _brokePlaceId = value;
                if (BrokePlaceId == ProductionPlaceId) PrintName = ProductionPrintName;
                RaisePropertyChanged("BrokePlaceId");
            }
        }

        public int ProductionPlaceId
        {
            get { return _productionPlaceId; }
            set
            {
                _productionPlaceId = value;
                if (BrokePlaceId == null) BrokePlaceId = ProductionPlaceId;
            }
        }

        public string Place { get; set; }
        public byte? BrokeShiftId { get; set; }

        public byte? ShiftId
        {
            get { return _shiftId; }
            set
            {
                _shiftId = value;
                if (BrokeShiftId == null) BrokeShiftId = ShiftId;
            }
        }

        public string ProductionPrintName { get; set; }
        public string PrintName { get; set; }
//        public List<Place> Places { get; set; }

        public ItemsChangeObservableCollection<RejectionReason> RejectionReasons { get; }

        private string FormRejectionReasonsString(IEnumerable<RejectionReason> list, GammaEntities gammaDb = null)
        {
            var sbuilderReason = new StringBuilder();
            using (var gammaBase = DB.GammaDb)
            {
                foreach (var reason in list)
                {
                    var description =
                        gammaBase.C1CRejectionReasons.FirstOrDefault(
                            r => r.C1CRejectionReasonID == reason.RejectionReasonID)?.Description;
                    if (description == null) continue;
                    sbuilderReason.Append(description);
                    sbuilderReason.Append(Environment.NewLine);
                }
            }
                
            return sbuilderReason.ToString();
        }

        private string FormRejectionReasonCommentsString(IEnumerable<RejectionReason> list)
        {
            var sbuilderReason = new StringBuilder();
            foreach (var reason in list.Where(reason => reason.Comment != null))
                {
                    sbuilderReason.Append(reason.Comment);
                    sbuilderReason.Append(Environment.NewLine);
                }
            return sbuilderReason.ToString();
        }
    }
}