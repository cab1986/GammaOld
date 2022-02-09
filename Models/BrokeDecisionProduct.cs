// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.ComponentModel;
using DevExpress.Mvvm;
using Gamma.Common;
using System.Linq;

namespace Gamma.Models
{
    public class BrokeDecisionProduct : ViewModelBase
    {
        public BrokeDecisionProduct(Guid productId, ProductKind productKind, string number, ProductState state, string nomenclatureName, 
            string measureUnit, Guid nomenclatureOldId , Guid? characteristicOldId, decimal quantity = 0, bool decisionApplied = false, Guid? docWithdrawalID = null, DateTime? decisionDate = null, int? decisionPlaceId = null)
        {
            Quantity = quantity;
            ProductId = productId;
            Number = number;
            ProductState = state;
            NomenclatureName = nomenclatureName;
            MeasureUnit = measureUnit;
            NomenclatureOldId = nomenclatureOldId;
            CharacteristicOldId = characteristicOldId;
            ProductKind = productKind;
            DecisionApplied = decisionApplied;
            DocWithdrawalID = docWithdrawalID;
            DecisionDate = decisionDate;
            DecisionPlaceId = decisionPlaceId;
        }

        public BrokeDecisionProduct(Guid productId, ProductKind productKind, string number, ProductState state, 
            string nomenclatureName,
            string measureUnit, Guid nomenclatureOldId, Guid? characteristicOldId, DateTime? decisionDate, int? decisionPlaceId)
                : this(productId, productKind, number, state, nomenclatureName, 
                measureUnit, nomenclatureOldId, characteristicOldId, 0, false, null, decisionDate, decisionPlaceId)
        { }

        public Guid ProductId { get; set; }

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
        public string Decision { get; private set; }
        public ProductKind ProductKind { get; set; }
        public string NumberAndNomenclature => Number.PadRight(14) + "  |  " + NomenclatureName;
        public string DecisionDateAndPlace => (DecisionDate == null ? "" : ((DateTime)DecisionDate).ToString("dd.MM.yyyy HH:mm:ss")).PadRight(19) + (DecisionPlaceName?.Length > 0 ? "  |  " + DecisionPlaceName : "");
        public string DecisionPlaceName { get; set; }
                
        private DateTime? _decisionDate { get; set; }
        public DateTime? DecisionDate
        {
            get { return _decisionDate; }
            set
            {
                _decisionDate = value;
                RaisePropertyChanged("DecisionDate");

            }
        }

        private int? _decisionPlaceId { get; set; }
        public int? DecisionPlaceId
        {
            get { return _decisionPlaceId; }
            set
            {
                _decisionPlaceId = value;
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

        public Guid? DocWithdrawalID { get; set; }
    }
}