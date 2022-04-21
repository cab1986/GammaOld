// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.ComponentModel;
using DevExpress.Mvvm;
using Gamma.Common;
using System.Linq;
using System.Collections.Generic;
using Gamma.Entities;

namespace Gamma.Models
{
    public class BrokeDecisionProduct : ViewModelBase
    {
        public BrokeDecisionProduct()
                : this(SqlGuidUtil.NewSequentialid(), SqlGuidUtil.NewSequentialid(), ProductKind.ProductBale, "", 0, ProductState.NeedsDecision, "", "", null, null, SqlGuidUtil.NewSequentialid(), null, 0, false, null, null, null)
        { }

        public BrokeDecisionProduct(Guid decisionDocId, Guid productId, ProductKind productKind, string number, decimal productQuantity, ProductState state, string nomenclatureName, 
            string measureUnit, Guid? rejectionReasonID, int? brokePlaceID, Guid nomenclatureOldId , Guid? characteristicOldId, decimal quantity = 0, bool decisionApplied = false,
            List<KeyValuePair<Guid, String>> docWithdrawals = null, //Guid? docWithdrawalID = null, 
            DateTime? decisionDate = null, int? decisionPlaceId = null)
        {
            DecisionDocId = decisionDocId;
            Quantity = quantity;
            ProductId = productId;
            Number = number;
            ProductState = state;
            NomenclatureName = nomenclatureName;
            MeasureUnit = measureUnit;
            RejectionReasonID = rejectionReasonID;
            BrokePlaceID = brokePlaceID;
            NomenclatureOldId = nomenclatureOldId;
            CharacteristicOldId = characteristicOldId;
            ProductKind = productKind;
            ProductQuantity = productQuantity;
            DecisionApplied = decisionApplied;
            //DocWithdrawalID = docWithdrawalID;
            DocWithdrawals = docWithdrawals ?? new List<KeyValuePair<Guid, string>>();
            DecisionDate = decisionDate;
            DecisionPlaceId = decisionPlaceId;
        }

        public BrokeDecisionProduct(Guid decisionDocId, Guid productId, ProductKind productKind, string number, decimal productQuantity, ProductState state, 
            string nomenclatureName, string measureUnit, Guid? rejectionReasonID, int? brokePlaceID,
            Guid nomenclatureOldId, Guid? characteristicOldId, DateTime? decisionDate, int? decisionPlaceId)
                : this(decisionDocId, productId, productKind, number, productQuantity, state, nomenclatureName, 
                measureUnit, rejectionReasonID, brokePlaceID, nomenclatureOldId, characteristicOldId, 0, false, null, decisionDate, decisionPlaceId)
        { }

        public Guid ProductId { get; set; }
        public Guid DecisionDocId { get; set; }
        public decimal ProductQuantity { get; set; }
        public Guid? RejectionReasonID { get; set; }
        public int? BrokePlaceID { get; set; }

        public bool IsChanged { get; set; } = true;

        private ProductState _productState;
        public ProductState ProductState
        {
            get { return _productState; }
            set
            {
                _productState = value;
                Decision = _productState.GetAttributeOfType<DescriptionAttribute>().Description;
            }
        }

        private string _nomenclatureName;

        public string NomenclatureName
        {
            get { return _nomenclatureName; }
            set
            {
                _nomenclatureName = value;
                RaisePropertyChanged("NomenclatureName");
            }
        }

        private decimal _quantity;

        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                RaisePropertyChanged("Quantity");
            }            
        }
        public string Comment { get; set; }
        public string Number { get; set; }
        public string Decision { get; set; }
        public ProductKind ProductKind { get; set; }
        public string NumberAndNomenclature => Number.PadRight(14) + "  |  " + NomenclatureName;
        public string DecisionDateAndPlace => (DecisionDate == null ? "" : ((DateTime)DecisionDate).ToString("dd.MM.yyyy HH:mm:ss")).PadRight(19) + (DecisionPlaceName?.Length > 0 ? "  |  " + DecisionPlaceName : "");

        private string _decisionPlaceName { get; set; }
        public string DecisionPlaceName
        {
            get { return _decisionPlaceName; }
            set
            {
                _decisionPlaceName = value;
                RaisePropertyChanged("DecisionPlaceName");
                RaisePropertyChanged("DecisionDateAndPlace");
            }
        }

        private DateTime? _decisionDate { get; set; }
        public DateTime? DecisionDate
        {
            get { return _decisionDate; }
            set
            {
                _decisionDate = value;
                RaisePropertyChanged("DecisionDate");
                RaisePropertyChanged("DecisionDateAndPlace");
            }
        }

        private int? _decisionPlaceId { get; set; }
        public int? DecisionPlaceId
        {
            get { return _decisionPlaceId; }
            set
            {
                _decisionPlaceId = value;
                DecisionPlaceName = WorkSession.Places.FirstOrDefault(p => p.PlaceID == value)?.Name;
                RaisePropertyChanged("DecisionPlaceId");

            }
        }

        private Guid? _nomenclatureId;

        public Guid? NomenclatureId
        {
            get { return _nomenclatureId; }
            set
            {
                _nomenclatureId = value;
                RaisePropertyChanged("NomenclatureId");
            }
        }

        private Guid? _characteristicId;

        public Guid? CharacteristicId
        {
            get { return _characteristicId; }
            set
            {
                _characteristicId = value;
                RaisePropertyChanged("CharacteristicId");
            }
        }

        public Guid NomenclatureOldId { get; set; }
        public Guid? CharacteristicOldId { get; set; }
        

        public string MeasureUnit { get; set; }

        protected bool Equals(BrokeDecisionProduct other)
        {
            return _productState == other._productState && ProductId.Equals(other.ProductId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BrokeDecisionProduct) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) _productState*397) ^ ProductId.GetHashCode();
            }
        }

        private bool _DecisionApplied;
        public bool DecisionApplied
        {
            get { return _DecisionApplied; }
            set
            {
                _DecisionApplied = value;
                RaisePropertyChanged("DecisionApplied");
            }

        }

        //public Guid? DocWithdrawalID { get; set; }
        private List<KeyValuePair<Guid, String>> _docWithdrawals { get; set; } = new List<KeyValuePair<Guid, string>>();
        public List<KeyValuePair<Guid, String>> DocWithdrawals
        {
            get { return _docWithdrawals; }
            set
            {
                _docWithdrawals = value;
                RaisePropertyChanged("DocWithdrawals");
            }
        }
        
        private decimal _docWithdrawalSum { get; set; }
        public decimal DocWithdrawalSum
        {
            get { return _docWithdrawalSum; }
            set
            {
                _docWithdrawalSum = value;
                DecisionAppliedLabel = "Выполнено на " + value.ToString();
                DecisionApplied = value >= Quantity;
                RaisePropertyChanged("DocWithdrawalSum");
            }
        }

        private string _decisionAppliedLabel { get; set; } = "Выполнено";
        public string DecisionAppliedLabel
        {
            get { return _decisionAppliedLabel; }
            set
            {
                _decisionAppliedLabel = value;
                RaisePropertyChanged("DecisionAppliedLabel");
            }
        }

        public bool IsNotNeedToSave { get; set; } = false;
        public bool IsVisibleRow { get; set; } = true;

        /*
        private Guid? _docWithdrawal { get; set; }
        public Guid? DocWithdrawal
        {
            get { return _docWithdrawal; }
            set
            {
                //_docWithdrawal = value;
                RaisePropertyChanged("DocWithdrawal");
                if (value != null)
                    switch (DocWithdrawals?.FirstOrDefault(d => d.Key == (Guid)value).Value.Substring(0,7))
                    {
                        case "Списани":
                            MessageManager.OpenDocWithdrawal((Guid)value);
                            break;
                        case "Продукт":
                            MessageManager.OpenDocProduct(ProductKind,(Guid)value);
                            break;
                    }
                    
            }
        }*/

    }
}