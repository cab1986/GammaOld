// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Common;
using Gamma.ViewModels;
using DevExpress.Mvvm;
using Gamma.DialogViewModels;
using System.ComponentModel;
using System.Windows;
using Gamma.Controllers;

namespace Gamma.Models
{
    public class EditBrokeDecisionItem: DbEditItemWithNomenclatureViewModel
    {
        public EditBrokeDecisionItem(string name, ProductState productState, 
            DocBrokeDecisionViewModel parentModel, bool canChooseNomenclature = false)
        {
            Name = name;
            ParentModel = parentModel;
            ProductState = productState;
            NomenclatureVisible = canChooseNomenclature;
            //OpenWithdrawalCommand = new DelegateCommand<Guid>(OpenWithdrawal); //, () => DocWithdrawals?.Count != 0);
            CreateWithdrawalCommand = new DelegateCommand(CreateWithdrawal, ()=> DB.HaveWriteAccess("DocWithdrawalProducts"));
        }

        private DocBrokeDecisionViewModel ParentModel { get; }

        private readonly DocumentController documentController = new DocumentController();
        private readonly ProductController productController = new ProductController();

        public bool IsVisibilityExtendedField =>
#if (DEBUG)
    true;
#else
            WorkSession.DBAdmin;
#endif


        //public DelegateCommand<Guid> OpenWithdrawalCommand { get; private set; }
        public DelegateCommand CreateWithdrawalCommand { get; private set; }

        private decimal _quantity;
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                RaisePropertyChanged("Quantity");
                RefreshEditBrokeDecisionItem();
                RefreshDecisionApplied();
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

        public void Init()
        {
            if (ProductState == ProductState.Good)
                Name = "������";
            IsReadOnly = false;
            IsReadOnlyDecisionApplied = false;
            IsReadOnlyFields = false;
            IsReadOnlyIsChecked = false;
            IsReadOnlyQuantity = false;
            IsExistForConversionOrRepackItem = false;
            IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0 = false;
            IsExistMoreTwoCheckedItem = false;
            IsNotNeedToSave = false;
            IsVisibleRow = true;
        }

        private bool _isReadOnly = true;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                RefreshReadOnlyFields(value);
                RefreshReadOnlyIsChecked(value);
                RaisePropertyChanged("IsReadOnly");
            }
        }

        private bool _isReadOnlyIsChecked { get; set; }
        public bool IsReadOnlyIsChecked
        {
            get { return _isReadOnlyIsChecked; }
            set
            {
                _isReadOnlyIsChecked = value;
                RaisePropertyChanged("IsReadOnlyIsChecked");
            }
        }

        private void RefreshReadOnlyIsChecked(bool isReadOnly)
        {
            IsReadOnlyIsChecked = isReadOnly //|| MaxQuantity == 0 || MinQuantity > 0 
                ;//|| IsExistMoreTwoCheckedItem
                //|| ((ProductState == ProductState.Good || ProductState == ProductState.InternalUsage || ProductState == ProductState.Limited) && IsExistForConversionOrRepackItem)
            //|| (ParentModel.NeedsProductStates.Contains(ProductState) && (IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0 || IsExistGoodItem));
            RefreshReadOnlyFields(isReadOnly);
        }

        private void RefreshReadOnlyFields(bool isReadOnly)
        {
            var isReadOnlyIsChecked = IsReadOnlyIsChecked || IsExistMoreTwoCheckedItem
                     || ((ProductState == ProductState.Good || ProductState == ProductState.InternalUsage || ProductState == ProductState.Limited) && IsExistForConversionOrRepackItem)
                     || (ParentModel.NeedsProductStates.Contains(ProductState) && (IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0 || IsExistGoodItem));
            IsReadOnlyFields = !IsChecked || isReadOnly || !(Quantity > 0) || DecisionApplied || isReadOnlyIsChecked;// || (!isReadOnly && (ProductState == ProductState.NeedsDecision || ProductState == ProductState.Repack || ProductState == ProductState.ForConversion));
            IsReadOnlyQuantity =  !IsChecked || isReadOnly || ParentModel.NeedsProductStates.Contains(ProductState) || !(!DecisionApplied || IsExistNeedDecisionMore0) || isReadOnlyIsChecked;
            IsReadOnlyDecisionApplied = !IsChecked || isReadOnly || !(Quantity > 0) || DecisionApplied;
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
                if (_isChecked == value)
                    return;
                else if (ExternalRefresh == false)
                {
                    if (_isChecked && !value
                            && ParentModel.NeedsProductStates.Contains(ProductState))
                    {
                        MessageBox.Show("����� ����� ������� � ������ ������� �������");
                        return;
                    }
                    if (_isChecked && !value
                            && //((ParentModel.NeedsProductStates.Contains(ProductState) || DecisionApplied)
                                DocWithdrawalSum > 0)
                    {
                        MessageBox.Show("����� ����� �������, ���� ��� ���� ��������� � ���������");
                        return;
                    }
                    else if (!_isChecked && value
                            && MaxQuantity == 0)
                    {
                        MessageBox.Show("����� ��������� �������, ��� ��� ���� ��� ��������� ���������� � �������."+Environment.NewLine
                            +"������� �������� ��� ��������� �������.");
                        return;
                    }
                    else if (!_isChecked && value
                            && ParentModel.ProductKind == ProductKind.ProductGroupPack
                            && (!ParentModel.NeedsProductStates.Contains(ProductState)))
                    {
                        MessageBox.Show("����� ��������� �������, ��� ��� ��� ��������� ��������. ������� ���������� ��.");
                        return;
                    }
                    else if (IsExistMoreTwoCheckedItem)
                    {
                        MessageBox.Show("����� ��������� �������, ��� ��� ��� ������� 2 �������.");
                        return;
                    }
                    else if (((ProductState == ProductState.Good || ProductState == ProductState.InternalUsage || ProductState == ProductState.Limited) && IsExistForConversionOrRepackItem))
                    {
                        MessageBox.Show("����� ��������� �������, ��� ��� ��� ������� �� ������������ ��� �� ���������.");
                        return;
                    }
                    else if (ParentModel.NeedsProductStates.Contains(ProductState) && IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0)
                    {
                        MessageBox.Show("����� ��������� �������, ��� ��� ��� ���� �� ������������ ��� �� ��������� � ��������� ������ 0.");
                        return;
                    }
                    else if (ParentModel.NeedsProductStates.Contains(ProductState) && IsExistGoodItem)
                    {
                        MessageBox.Show("����� ��������� �������, ��� ��� ��� ������ ������");
                        return;
                    }
                }
                _isChecked = value;
                RaisePropertyChanged("IsChecked");
                RefreshEditBrokeDecisionItem();
            }
        }

        private string _name { get; set; }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        private ProductState _productState { get; set; }
        public ProductState ProductState
        {
            get { return _productState; }
            private set
            {
                _productState = value;
                IsDecisionAppliedVisible = (value == ProductState.Broke) || (value == ProductState.ForConversion) || (value == ProductState.Repack);
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
                IsVisibleRow = !(value && ParentModel.NeedsProductStates.Contains(ProductState));
                RefreshEditBrokeDecisionItem();
            }
        }

/*        private bool _isExistWithdrawalSum;
        public bool IsExistWithdrawalSum
        {
            get { return _isExistWithdrawalSum; }
            set
            {
                _isExistWithdrawalSum = value;
                RaisePropertyChanged("IsExistWithdrawalSum");
                RefreshReadOnlyIsChecked(IsReadOnly);
            }
        }
*/
        private bool _isExistMoreTwoCheckedItem;
        public bool IsExistMoreTwoCheckedItem
        {
            get { return _isExistMoreTwoCheckedItem; }
            set
            {
                _isExistMoreTwoCheckedItem = value;
                RaisePropertyChanged("IsExistMoreTwoCheckedItem");
                RefreshReadOnlyIsChecked(IsReadOnly);
            }
        }

        private bool _isExistForConversionOrRepackItem;
        public bool IsExistForConversionOrRepackItem
        {
            get { return _isExistForConversionOrRepackItem; }
            set
            {
                _isExistForConversionOrRepackItem = value;
                RaisePropertyChanged("IsExistForConversionOrRepackItem");
                RefreshReadOnlyIsChecked(IsReadOnly);
            }
        }

        private bool _isExistForConversionOrRepackItemWithDecisionAppliedSumMore0;
        public bool IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0
        {
            get { return _isExistForConversionOrRepackItemWithDecisionAppliedSumMore0; }
            set
            {
                _isExistForConversionOrRepackItemWithDecisionAppliedSumMore0 = value;
                RaisePropertyChanged("IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0");
                RefreshReadOnlyIsChecked(IsReadOnly);
            }
        }

        private bool _isExistGoodItem = false;
        public bool IsExistGoodItem
        {
            get { return _isExistGoodItem; }
            set
            {
                _isExistGoodItem = value;
                RaisePropertyChanged("IsExistGoodItem");
                RefreshReadOnlyIsChecked(IsReadOnly);
            }
        }

        private bool _isExistNeedDecisionMore0 = false;
        public bool IsExistNeedDecisionMore0
        {
            get { return _isExistNeedDecisionMore0; }
            set
            {
                _isExistNeedDecisionMore0 = value;
                RaisePropertyChanged("IsExistNeedDecisionMore0");
                RefreshReadOnlyIsChecked(IsReadOnly);
            }
        }

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
                RefreshDecisionApplied();
                RaisePropertyChanged("DocWithdrawalSum");
            }
        }

        private void RefreshDecisionApplied()
        {
            DecisionApplied = DocWithdrawalSum > 0 && DocWithdrawalSum >= Quantity;
            DecisionAppliedLabel = "��������� �� " + (DocWithdrawalSum > 0 ? DocWithdrawalSum.ToString() : "");
        }

        private string _decisionAppliedLabel { get; set; } = "���������";
        public string DecisionAppliedLabel
        {
            get { return _decisionAppliedLabel; }
            set
            {
                _decisionAppliedLabel = value;
                RaisePropertyChanged("DecisionAppliedLabel");
            }
        }

        private Guid? _docWithdrawal { get; set; }
        public Guid? DocWithdrawal
        {
            get { return _docWithdrawal; }
            set
            {
                //_docWithdrawal = value;
                RaisePropertyChanged("DocWithdrawal");
                if (value != null)
                    switch (DocWithdrawals?.FirstOrDefault(d => d.Key == (Guid)value).Value.Substring(0, 7))
                    {
                        case "�������":
                            MessageManager.OpenDocWithdrawal((Guid)value);
                            break;
                        case "�������":
                            MessageManager.OpenDocProduct((ProductKind)ParentModel.ProductKind, (Guid)value);
                            break;
                    }

            }
        }

        private Guid? ProductId => BrokeDecisionProduct?.ProductId;
        private void CreateWithdrawal()
        {
            if ( ParentModel.ProductKind == ProductKind.ProductGroupPack)
                Functions.ShowMessageError("������� ��������� � ��� � �����: " + Environment.NewLine + "������ ������ ���������, ��� ��� ��� ��������� ��������. ������� ���������� ��.", "ERROR CreateWithdrawal (Product is GroupPack)", null, ProductId);
            else if (ProductId == null)
                Functions.ShowMessageError("������� ��������� � ��� � �����: " + Environment.NewLine + "������ ������ ���������, ��� ��� �� ��������� �������", "ERROR CreateWithdrawal (ProductId is NULL)", null, ProductId);
            else if (!ParentModel.SaveBrokeDecisionProductsToModel((Guid)ProductId))
                Functions.ShowMessageError("������� ��������� � ��� � �����: " + Environment.NewLine + "������ ��� ���������� �������", "ERROR CreateWithdrawal (error save)", null, ProductId);
            else
            {
                var productionQuantity = ParentModel.ProductQuantity;// GammaBase.DocProductionProducts.FirstOrDefault(p => p.ProductID == ProductId).Quantity;
                var productSpool = GammaBase.ProductSpools.FirstOrDefault(p => p.ProductID == ProductId);
                var docWithdrawalId =
                    ProductState == ProductState.Broke ? ParentModel.CreateWithdrawal((byte)ProductState, Quantity - DocWithdrawalSum, productionQuantity)
                    : ProductState == ProductState.ForConversion ? (NomenclatureID == null || CharacteristicID == null ? null : ParentModel.CreateWithdrawal((byte)ProductState, Quantity - DocWithdrawalSum, productionQuantity, (Guid)NomenclatureID, (Guid)CharacteristicID, productSpool?.Diameter, productSpool?.BreakNumber, productSpool?.Length, productSpool?.RealFormat))
                    : ProductState == ProductState.Repack ? ParentModel.CreateWithdrawal((byte)ProductState, Quantity - DocWithdrawalSum, productionQuantity, (Guid)productSpool?.C1CNomenclatureID, (Guid)productSpool?.C1CCharacteristicID, productSpool?.Diameter, productSpool?.BreakNumber, productSpool?.Length, productSpool?.RealFormat)
                    : null;
                if (docWithdrawalId != null)
                    if (DocWithdrawalSum > MinQuantity)
                        MinQuantity = DocWithdrawalSum > Quantity ? Quantity : DocWithdrawalSum;
                if (!ParentModel.SaveBrokeDecisionProductsToModel((Guid)ProductId))
                    Functions.ShowMessageError("������� ��������� � ��� � �����: " + Environment.NewLine + "������ ��� ���������� �������", "ERROR CreateWithdrawal (error save)", null, ProductId);
            }
        }

        private decimal _maxQuantity { get; set; }
        public decimal MaxQuantity
        {
            get { return _maxQuantity; }
            set
            {
                _maxQuantity = value;
                RefreshReadOnlyIsChecked(IsReadOnly);
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
                RefreshReadOnlyIsChecked(IsReadOnly);
                RaisePropertyChanged("MinQuantity");
            }
        }

        private bool _isNotNeedToSave { get; set; } = false;
        public bool IsNotNeedToSave
        {
            get { return _isNotNeedToSave; }
            set
            {
                _isNotNeedToSave = value;
                RaisePropertyChanged("IsNotNeedToSave");
            }
        }
        
        private bool _isVisibleRow { get; set; } = true;
        public bool IsVisibleRow
        {
            get { return _isVisibleRow; }
            set
            {
                _isVisibleRow = value;
                RaisePropertyChanged("IsVisibleRow");
            }
        }
    }
}