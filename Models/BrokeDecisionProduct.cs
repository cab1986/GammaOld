// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.ComponentModel;
using DevExpress.Mvvm;
using Gamma.Common;

namespace Gamma.Models
{
    public class BrokeDecisionProduct : ViewModelBase
    {
        public BrokeDecisionProduct(Guid productId, ProductKind productKind, string number, ProductState state, decimal maxQuantity,  string nomenclatureName, 
            string measureUnit, Guid nomenclatureOldId , Guid characteristicOldId, decimal quantity = 0, bool decisionApplied = false, Guid? docWithdrawalID = null)
        {
            MaxQuantity = maxQuantity;
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
        }

        public decimal MaxQuantity { get; set; }

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
        public Guid CharacteristicOldId { get; set; }
        

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