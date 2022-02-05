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
        public EditBrokeDecisionItem(string name, ProductState productState, ItemsChangeObservableCollection<BrokeDecisionProduct> decisionProducts, bool canChooseNomenclature = false)
        {
            Name = name;
            ProductState = productState;
            NomenclatureVisible = canChooseNomenclature;
            BrokeDecisionProducts = decisionProducts;
            OpenWithdrawalCommand = new DelegateCommand(OpenWithdrawal);
        }

        public DelegateCommand OpenWithdrawalCommand { get; private set; }

        private decimal _quantity;

        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                if (BrokeDecisionProduct != null)
                {
                    if (IsChecked && ProductState == ProductState.Broke && BrokeDecisionProducts.Any(bp => bp.ProductState == ProductState.ForConversion && bp.ProductId == BrokeDecisionProduct.ProductId && IsChecked))
                    {
                        var productForConversionDecision = BrokeDecisionProducts.FirstOrDefault(bp => bp.ProductState == ProductState.ForConversion && bp.ProductId == BrokeDecisionProduct.ProductId && IsChecked);
                        if (productForConversionDecision != null)
                        {
                            productForConversionDecision.Quantity = productForConversionDecision.Quantity - (value - _quantity);
                        }
                    }

                    var productNeedsDecision = BrokeDecisionProducts.FirstOrDefault(bp => bp.ProductState == ProductState.NeedsDecision && bp.ProductId == BrokeDecisionProduct.ProductId);
                    var sumQuantity = 
                        BrokeDecisionProducts
                            .Where(p => p.ProductId == BrokeDecisionProduct.ProductId).ToList().Except(new List<BrokeDecisionProduct>
                        {
                            BrokeDecisionProduct,
                            productNeedsDecision
                        })
                        .Sum(p => p.Quantity);
                    if (sumQuantity + value >= BrokeDecisionProduct.MaxQuantity)
                    {
                        value = BrokeDecisionProduct.MaxQuantity - sumQuantity;
                        if (IsChecked)
                            BrokeDecisionProducts.Remove(productNeedsDecision);                          
                    }
                    else if (IsChecked)
                    {
                        if (productNeedsDecision == null)
                        {
                            productNeedsDecision = new BrokeDecisionProduct
                            (BrokeDecisionProducts,
                                BrokeDecisionProduct.ProductId,
                                BrokeDecisionProduct.ProductKind,
                                BrokeDecisionProduct.Number,
                                ProductState.NeedsDecision,
                                BrokeDecisionProduct.MaxQuantity,
                                BrokeDecisionProduct.NomenclatureName,
                                BrokeDecisionProduct.MeasureUnit,
                                BrokeDecisionProduct.NomenclatureOldId,
                                BrokeDecisionProduct.CharacteristicOldId                                
                            );
                            productNeedsDecision.DecisionDate = BrokeDecisionProduct.DecisionDate;
                            productNeedsDecision.DecisionPlaceId = BrokeDecisionProduct.DecisionPlaceId;
                            BrokeDecisionProducts.Add(productNeedsDecision);
                        }
                        productNeedsDecision.Quantity = BrokeDecisionProduct.MaxQuantity - sumQuantity - value;                        
                    }
                    BrokeDecisionProduct.Quantity = value;                   
                }
                _quantity = value;
                RaisePropertyChanged("Quantity");
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
            }
        }

        protected override bool CanChooseNomenclature()
        {
            return base.CanChooseNomenclature() && !IsReadOnly;
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
            }
        }

        private bool _isReadOnly = true;

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                RaisePropertyChanged("IsReadOnly");
            }
        }

        private ItemsChangeObservableCollection<BrokeDecisionProduct> BrokeDecisionProducts { get; set; }
        public bool NomenclatureVisible { get; private set; }

        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked == value) return;
                if (BrokeDecisionProduct == null)
                {
                    _isChecked = value;
                    RaisePropertyChanged("IsChecked");
                    return;
                }
                // ���� �� ������ ��� ������, �� �� ����������, �� ������� (2 ��������� ����������)
                if (value && (ProductState != ProductState.Broke || (ProductState == ProductState.Broke && (BrokeDecisionProduct.ProductKind != ProductKind.ProductSpool && BrokeDecisionProduct.ProductKind != ProductKind.ProductPallet && BrokeDecisionProduct.ProductKind != ProductKind.ProductPalletR))) && 
                    (
                        BrokeDecisionProducts.Any(p => p.ProductState != ProductState 
                        && p.ProductId == BrokeDecisionProduct.ProductId
                        && (p.ProductState != ProductState.Broke || (p.ProductState == ProductState.Broke && (BrokeDecisionProduct.ProductKind != ProductKind.ProductSpool && BrokeDecisionProduct.ProductKind != ProductKind.ProductPallet && BrokeDecisionProduct.ProductKind != ProductKind.ProductPalletR))) && p.ProductState != ProductState.NeedsDecision)
                    )) return;
                _isChecked = value;
                RaisePropertyChanged("IsChecked");
                var productNeedsDecision = BrokeDecisionProducts.FirstOrDefault(
                        bp =>
                            bp.ProductId == BrokeDecisionProduct.ProductId &&
                            bp.ProductState == ProductState.NeedsDecision);
                if (value)
                {

                    if (!BrokeDecisionProducts.Contains(BrokeDecisionProduct))
                    {
                        if (BrokeDecisionProducts.Count != 0)
                        {
                            BrokeDecisionProduct.IsEditableDecision = BrokeDecisionProducts.FirstOrDefault(p => p.IsEditableDecision = true)?.IsEditableDecision ?? false;
                            BrokeDecisionProduct.DecisionDate = BrokeDecisionProducts.FirstOrDefault(p => p.DecisionDate != null)?.DecisionDate;
                            BrokeDecisionProduct.DecisionPlaceId = BrokeDecisionProducts.FirstOrDefault(p => p.DecisionPlaceId != null)?.DecisionPlaceId;
                        }
                        BrokeDecisionProducts.Add(BrokeDecisionProduct);
                    }
                    if (BrokeDecisionProduct.ProductKind == ProductKind.ProductSpool || BrokeDecisionProduct.ProductKind == ProductKind.ProductPallet || BrokeDecisionProduct.ProductKind == ProductKind.ProductPalletR) // ����� ��� ��������
                    {
                        var sumQuantity = BrokeDecisionProducts.Where(p => p.ProductId == BrokeDecisionProduct.ProductId).Except(new List<BrokeDecisionProduct>
                        {
                            productNeedsDecision
                        })
                        .Sum(p => p.Quantity);
                        if (sumQuantity >= BrokeDecisionProduct.MaxQuantity)
                        {
                            BrokeDecisionProducts.Remove(productNeedsDecision);
                        }
                        else
                        {
                            if (productNeedsDecision != null)
                            {
                                productNeedsDecision.Quantity = BrokeDecisionProduct.MaxQuantity - sumQuantity;
                            }
                            else
                            {
                                productNeedsDecision = new BrokeDecisionProduct
                                    (BrokeDecisionProducts,
                                        BrokeDecisionProduct.ProductId,
                                        BrokeDecisionProduct.ProductKind,
                                        BrokeDecisionProduct.Number,
                                        ProductState.NeedsDecision,
                                        BrokeDecisionProduct.MaxQuantity,
                                        BrokeDecisionProduct.NomenclatureName,
                                        BrokeDecisionProduct.MeasureUnit,
                                        BrokeDecisionProduct.NomenclatureOldId,
                                        BrokeDecisionProduct.CharacteristicOldId,
                                        BrokeDecisionProduct.MaxQuantity - sumQuantity
                                    );
                                productNeedsDecision.DecisionDate = BrokeDecisionProduct.DecisionDate;
                                productNeedsDecision.DecisionPlaceId = BrokeDecisionProduct.DecisionPlaceId;
                                BrokeDecisionProducts.Add(productNeedsDecision);
                            }
                        }
                    }
                    else // ����� ��� �����, ����� �������� (���������� ���������� ���������)
                    {
                        if (productNeedsDecision == null) return;
                        BrokeDecisionProduct.Quantity = productNeedsDecision.Quantity;
                        Quantity = productNeedsDecision.Quantity;
                        BrokeDecisionProducts.Remove(productNeedsDecision);
                        if (BrokeDecisionProduct.ProductKind != ProductKind.ProductPallet && BrokeDecisionProduct.ProductKind != ProductKind.ProductPalletR && BrokeDecisionProduct.ProductState != ProductState.ForConversion) return;
                        NomenclatureID = productNeedsDecision.NomenclatureOldId;
                        CharacteristicID = productNeedsDecision.CharacteristicOldId;
                    }
                }
                else
                {
                    if (BrokeDecisionProduct.ProductKind == ProductKind.ProductSpool || BrokeDecisionProduct.ProductKind == ProductKind.ProductPallet || BrokeDecisionProduct.ProductKind == ProductKind.ProductPalletR)
                    {
                        var sumQuantity = BrokeDecisionProducts.Except(new List<BrokeDecisionProduct>
                        {
                            BrokeDecisionProduct
                        }).Where(p => p.ProductId == BrokeDecisionProduct.ProductId)
                            .Sum(p => p.Quantity);
                        if (productNeedsDecision == null)
                        {
                            productNeedsDecision = new BrokeDecisionProduct
                                (BrokeDecisionProducts,
                                    BrokeDecisionProduct.ProductId,
                                    BrokeDecisionProduct.ProductKind,
                                    BrokeDecisionProduct.Number,
                                    ProductState.NeedsDecision,
                                    BrokeDecisionProduct.MaxQuantity,
                                    BrokeDecisionProduct.NomenclatureName,
                                    BrokeDecisionProduct.MeasureUnit,
                                    BrokeDecisionProduct.NomenclatureOldId,
                                    BrokeDecisionProduct.CharacteristicOldId,
                                    BrokeDecisionProduct.MaxQuantity - sumQuantity
                                );
                            productNeedsDecision.DecisionDate = BrokeDecisionProduct.DecisionDate;
                            productNeedsDecision.DecisionPlaceId = BrokeDecisionProduct.DecisionPlaceId;
                            BrokeDecisionProducts.Add(productNeedsDecision);
                        }
                        else
                        {
                            productNeedsDecision.Quantity += BrokeDecisionProduct.Quantity;
                            Quantity = 0;
                            NomenclatureID = null;
                            CharacteristicID = null;
                        }
                        BrokeDecisionProducts.Remove(BrokeDecisionProduct);
                    }
                    else
                    {
                        if (productNeedsDecision == null)
                        {
                            productNeedsDecision = new BrokeDecisionProduct
                                (BrokeDecisionProducts,
                                    BrokeDecisionProduct.ProductId,
                                    BrokeDecisionProduct.ProductKind,
                                    BrokeDecisionProduct.Number,
                                    ProductState.NeedsDecision,
                                    BrokeDecisionProduct.MaxQuantity,
                                    BrokeDecisionProduct.NomenclatureName,
                                    BrokeDecisionProduct.MeasureUnit,
                                    BrokeDecisionProduct.NomenclatureOldId,
                                    BrokeDecisionProduct.CharacteristicOldId,
                                    BrokeDecisionProduct.MaxQuantity
                                );
                            productNeedsDecision.DecisionDate = BrokeDecisionProduct.DecisionDate;
                            productNeedsDecision.DecisionPlaceId = BrokeDecisionProduct.DecisionPlaceId;
                            BrokeDecisionProducts.Add(productNeedsDecision);
                        }
                        else
                        {
                            productNeedsDecision.Quantity = BrokeDecisionProduct.Quantity;
                        }
                        BrokeDecisionProducts.Remove(BrokeDecisionProduct);
                        Quantity = 0;
                        NomenclatureID = null;
                        CharacteristicID = null;
                    }
                }
            }
        }

        public string Name { get; set; }

        private ProductState _productState { get; set; }

        private ProductState ProductState
        {
            get { return _productState; }
            set
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
            }
        }

        public Guid? DocWithdrawalID { get; set; }

        private void OpenWithdrawal()
        {
            if (DocWithdrawalID == null ) return;
            MessageManager.OpenDocWithdrawal((Guid)DocWithdrawalID);
        }
    }
}