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

        public BrokeProduct(Guid? rejectionReasonID, Guid? secondRejectionReasonID, string comment , int? placeId, int? brokePlaceId = null, byte? brokeShiftId = null
            , string brokePrintName = null) : this()
        {
            RejectionReasonID = rejectionReasonID;
            SecondRejectionReasonID = secondRejectionReasonID;
            RejectionReasonComment = comment;
            BrokePlaceId = brokePlaceId;
            BrokeShiftId = brokeShiftId;
            PrintName = brokePrintName;
            PlaceId = placeId;
        }

        /*public BrokeProduct(ItemsChangeObservableCollection<RejectionReason> rejectionReasons, int? placeId, int? brokePlaceId = null, byte? brokeShiftId = null
            , string brokePrintName = null) : this()
        {
            RejectionReasons = rejectionReasons;
            RejectionReasonsString = FormRejectionReasonsString(RejectionReasons);
            RejectionReasonCommentsString = FormRejectionReasonCommentsString(RejectionReasons);
            RejectionReasons.CollectionChanged += FormRejectionReasonString;
            BrokePlaceId = brokePlaceId;
            BrokeShiftId = brokeShiftId;
            PrintName = brokePrintName;
            PlaceId = placeId;
        }
        
        private void FormRejectionReasonString(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (RejectionReasons == null) return;
            RejectionReasonsString = FormRejectionReasonsString(RejectionReasons);
            RejectionReasonCommentsString = FormRejectionReasonCommentsString(RejectionReasons);
        }
        */
        public Guid ProductId { get; set; }
        public ProductKind ProductKind { get; set; }
        public string NomenclatureName { get; set; }
        public string Number { get; set; }
        public string BaseMeasureUnit { get; set; }
        public decimal Quantity { get; set; }

        private Guid? _rejectionReasonID;
        public Guid? RejectionReasonID
        {
            get { return _rejectionReasonID; }
            set
            {
                _rejectionReasonID = value;
                using (var gammaBase = DB.GammaDb)
                {
                   RejectionReasonName = gammaBase.C1CRejectionReasons.FirstOrDefault(
                                r => r.C1CRejectionReasonID == _rejectionReasonID)?.Description;
                }
                RaisePropertyChanged("RejectionReasonID");
            }
        }

        private Guid? _secondRejectionReasonID;
        public Guid? SecondRejectionReasonID
        {
            get { return _secondRejectionReasonID; }
            set
            {
                _secondRejectionReasonID = value;
                using (var gammaBase = DB.GammaDb)
                {
                    SecondRejectionReasonName = gammaBase.C1CRejectionReasons.FirstOrDefault(
                                 r => r.C1CRejectionReasonID == _secondRejectionReasonID)?.Description;
                }
                RaisePropertyChanged("SecondRejectionReasonID");
            }
        }

        public string SecondRejectionReasonName { get; set; }

        private string _rejectionReasonName;
        public string RejectionReasonName
        {
            get { return _rejectionReasonName; }
            set
            {
                _rejectionReasonName = value;
                RaisePropertyChanged("RejectionReasonName");
            }
        }        

        private string _rejectionReasonComment;
        public string RejectionReasonComment
        {
            get { return _rejectionReasonComment; }
            set
            {
                _rejectionReasonComment = value;
                RaisePropertyChanged("RejectionReasonComment");
            }
        }
        
        private byte? _shiftId;
        private int _productionPlaceId;
        private int? _brokePlaceId;

        public DateTime? Date { get; set; }

        private string _brokePlaceName { get; set; }
        public string BrokePlaceName
        {
            get { return _brokePlaceName; }
            set
            {
                _brokePlaceName = value;
                RaisePropertyChanged("BrokePlaceName");
            }
        }

        public int? BrokePlaceId
        {
            get { return _brokePlaceId; }
            set
            {
                _brokePlaceId = value;
                if (BrokePlaceId == ProductionPlaceId) PrintName = ProductionPrintName;
                RefreshBrokePlaceName();
                RaisePropertyChanged("BrokePlaceId");
            }
        }

        public int ProductionPlaceId
        {
            get { return _productionPlaceId; }
            set
            {
                _productionPlaceId = value;
                if (BrokePlaceId == null) BrokePlaceId = value;
            }
        }

        public string Place { get; set; }
        private byte? _brokeShiftId { get; set; }
        public byte? BrokeShiftId
        {
            get { return _brokeShiftId; }
            set
            {
                _brokeShiftId = value;
                RefreshBrokePlaceName();
                RaisePropertyChanged("BrokeShiftId");
            }
        }

        public byte? ShiftId
        {
            get { return _shiftId; }
            set
            {
                _shiftId = value;
                if (BrokeShiftId == null) BrokeShiftId = value;
            }
        }

        public string ProductionPrintName { get; set; }
        private string _printName { get; set; }
        public string PrintName
        {
            get { return _printName; }
            set
            {
                _printName = value;
                RefreshBrokePlaceName();
                RaisePropertyChanged("PrintName");
            }
        }

        private int? _placeId { get; set; }
        public int? PlaceId
        {
            get
            {
                return _placeId;
            }
            set
            {
                _placeId = value;
                using (var gammaBase = DB.GammaDb)
                {
                    PlaceName = value == null ? string.Empty : gammaBase.Places.FirstOrDefault(p => p.PlaceID == value).Name;
                }
            }
        }
        public string PlaceName { get; private set; }
        
        private void RefreshBrokePlaceName ()
        {
            using (var gammaBase = DB.GammaDb)
            {
                BrokePlaceName = (BrokePlaceId == null ? string.Empty : gammaBase.Places.FirstOrDefault(p => p.PlaceID == BrokePlaceId)?.Name)
                    + (BrokeShiftId == null ? string.Empty : ", Смена " + BrokeShiftId.ToString())
                    + (PrintName == null ? string.Empty : ",  " + PrintName);
            }
        }

        //private byte? _stateId { get; set; }
        //public byte? StateId
        //{
        //    get { return _stateId; }
        //    set
        //    {
        //        _stateId = value;
        //        RaisePropertyChanged("StateId");
        //    }
        //}
        /*
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
            foreach (var reason in list.Where(reason => reason.Comment != null).Distinct())
                {
                    sbuilderReason.Append(reason.Comment);
                    sbuilderReason.Append(Environment.NewLine);
                }
            return sbuilderReason.ToString();
        }*/
    }
}