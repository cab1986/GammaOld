// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Common;
using Gamma.ViewModels;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class EditBrokeDecisionItem: DbEditItemWithNomenclatureViewModel
    {
        public EditBrokeDecisionItem(string name, ProductState productState, 
            DocBrokeDecisionModel parentModel, bool canChooseNomenclature = false)
        {
            Name = name;
            ParentModel = parentModel;
            ProductState = productState;
            NomenclatureVisible = canChooseNomenclature;
            OpenWithdrawalCommand = new DelegateCommand(OpenWithdrawal, () => DocWithdrawalID != null);
        }

        private DocBrokeDecisionModel ParentModel { get; }

        public DelegateCommand OpenWithdrawalCommand { get; private set; }
                
        private decimal _quantity;
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                RaisePropertyChanged("Quantity");
                RefreshEditBrokeDecisionItem();
            }
        }

        private string _comment;
        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                RaisePropertyChanged("Comment");
                RefreshEditBrokeDecisionItem();
            }
        }

        public override Guid? NomenclatureID
        {
            get { return base.NomenclatureID; }
            set
            {
                base.NomenclatureID = value;
                if (BrokeDecisionProduct != null)
                {
                    if (BrokeDecisionProduct.NomenclatureId != NomenclatureID)
                        BrokeDecisionProduct.NomenclatureId = NomenclatureID;
                }
                RaisePropertyChanged("NomenclatureID");
                RefreshEditBrokeDecisionItem();
            }
        }

        protected override bool CanChooseNomenclature()
        {
            return base.CanChooseNomenclature() && !IsReadOnlyFields;
        }

        public override Guid? CharacteristicID
        {
            get { return base.CharacteristicID; }
            set
            {
                base.CharacteristicID = value;
                if (BrokeDecisionProduct != null)
                {
                    if (BrokeDecisionProduct.CharacteristicId != CharacteristicID)
                        BrokeDecisionProduct.CharacteristicId = CharacteristicID;
                }
                RaisePropertyChanged("CharacteristicID");
                RefreshEditBrokeDecisionItem();
            }
        }

        private bool _isReadOnly = true;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                RefreshReadOnlyFields(value);
                RaisePropertyChanged("IsReadOnly");
            }
        }

        private void RefreshReadOnlyFields(bool isReadOnly)
        {
            IsReadOnlyFields = !IsChecked || isReadOnly || !(Quantity > 0) || DecisionApplied ;// || (!isReadOnly && (ProductState == ProductState.NeedsDecision || ProductState == ProductState.Repack || ProductState == ProductState.ForConversion));
            IsReadOnlyQuantity =  !IsChecked || isReadOnly || ParentModel.NeedsProductStates.Contains(ProductState) || DecisionApplied;
            IsReadOnlyDecisionApplied = !IsChecked || isReadOnly || !(Quantity > 0); 
        }

        private bool _isReadOnlyFields { get; set; }
        public bool IsReadOnlyFields
        {
            get { return _isReadOnlyFields; }
            set
            {
                _isReadOnlyFields = value;
                RaisePropertyChanged("IsReadOnlyFields");
            }
        }

        private bool _isReadOnlyQuantity = true;
        public bool IsReadOnlyQuantity
        {
            get { return _isReadOnlyQuantity; }
            set
            {
                _isReadOnlyQuantity = value;
                RaisePropertyChanged("IsReadOnlyQuantity");
            }
        }
        
        private bool _isReadOnlyDecisionApplied = true;
        public bool IsReadOnlyDecisionApplied
        {
            get { return _isReadOnlyDecisionApplied; }
            set
            {
                _isReadOnlyDecisionApplied = value;
                RaisePropertyChanged("IsReadOnlyDecisionApplied");
            }
        }

        public bool NomenclatureVisible { get; private set; }

        public bool? ExternalRefresh { get; set; } = false;

        public void RefreshEditBrokeDecisionItem()
        {
            if (ExternalRefresh == false)
                ParentModel.RefreshEditBrokeDecisionItem(ProductState);
            RefreshReadOnlyFields(IsReadOnly);
        }

        private bool _isChecked { get; set; }
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked == value 
                    || (_isChecked && !value && ExternalRefresh == false 
                        && (ParentModel.NeedsProductStates.Contains(ProductState)
                            || DecisionApplied)))
                    return;
               _isChecked = value;
                RaisePropertyChanged("IsChecked");
                RefreshEditBrokeDecisionItem();
            }
        }

        public string Name { get; set; }

        private ProductState _productState { get; set; }

        public ProductState ProductState
        {
            get { return _productState; }
            private set
            {
                _productState = value;
                IsDecisionAppliedVisible = (value == ProductState.Broke) || (value == ProductState.ForConversion);
            }
        }
        
        private BrokeDecisionProduct _brokeDecisionProduct;

        public BrokeDecisionProduct BrokeDecisionProduct
        {
            get { return _brokeDecisionProduct; }
            set
            {
                _brokeDecisionProduct = value;
                if (_brokeDecisionProduct != null)
                {
                    if (_brokeDecisionProduct.ProductState == ProductState.ForConversion)
                    {
                        var characteristicID = _brokeDecisionProduct.CharacteristicId;
                        NomenclatureID = _brokeDecisionProduct.NomenclatureId;
                        CharacteristicID = characteristicID;
                    }
                }
                RaisePropertyChanged("BrokeDecisionProduct");
            }
        }
        
        private bool _isDecisionAppliedVisible;

        public bool IsDecisionAppliedVisible
        {
            get { return _isDecisionAppliedVisible; }
            set
            {
                _isDecisionAppliedVisible = value;
                RaisePropertyChanged("IsDecisionAppliedVisible");
            }
        }

        private bool _decisionApplied;

        public bool DecisionApplied
        {
            get { return _decisionApplied; }
            set
            {
                _decisionApplied = value;
                RaisePropertyChanged("DecisionApplied");
                RefreshEditBrokeDecisionItem();
            }
        }

        public Guid? DocWithdrawalID { get; set; }

        private void OpenWithdrawal()
        {
            if (DocWithdrawalID == null ) return;
            MessageManager.OpenDocWithdrawal((Guid)DocWithdrawalID);
        }

        private decimal _maxQuantity { get; set; }
        public decimal MaxQuantity
        {
            get { return _maxQuantity; }
            set
            {
                _maxQuantity = value;
                RaisePropertyChanged("MaxQuantity");
            }
        }

        private decimal _minQuantity { get; set; }
        public decimal MinQuantity
        {
            get { return _minQuantity; }
            set
            {
                _minQuantity = value;
                RaisePropertyChanged("MinQuantity");
            }
        }
    }
}