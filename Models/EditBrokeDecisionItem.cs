// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Common;
using Gamma.ViewModels;

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
        }

        private decimal _quantity;

        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                if (BrokeDecisionProduct != null)
                {
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
                            {
                                MaxQuantity = BrokeDecisionProduct.MaxQuantity,
                                ProductId = BrokeDecisionProduct.ProductId,
                                NomenclatureName = BrokeDecisionProduct.NomenclatureName,
                                Number = BrokeDecisionProduct.Number,
                                ProductState = ProductState.NeedsDecision,
                                MeasureUnit = BrokeDecisionProduct.MeasureUnit
                            };
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
                if (value && ProductState != ProductState.Broke && 
                    (
                        BrokeDecisionProducts.Any(p => p.ProductState != ProductState 
                        && p.ProductId == BrokeDecisionProduct.ProductId
                        && p.ProductState != ProductState.Broke && p.ProductState != ProductState.NeedsDecision)
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
                        BrokeDecisionProducts.Add(BrokeDecisionProduct);
                    
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
                            {
                                MaxQuantity = BrokeDecisionProduct.MaxQuantity,
                                Quantity = BrokeDecisionProduct.MaxQuantity - sumQuantity,
                                ProductId = BrokeDecisionProduct.ProductId,
                                Number = BrokeDecisionProduct.Number,
                                ProductState = ProductState.NeedsDecision,
                                NomenclatureName = BrokeDecisionProduct.NomenclatureName,
                                MeasureUnit = BrokeDecisionProduct.MeasureUnit
                            };
                            BrokeDecisionProducts.Add(productNeedsDecision);
                        }
                    }
                }
                else
                {
                    var sumQuantity = BrokeDecisionProducts.Except(new List<BrokeDecisionProduct> {BrokeDecisionProduct}).Where(p => p.ProductId == BrokeDecisionProduct.ProductId)
                        .Sum(p => p.Quantity);
                    if (productNeedsDecision == null)
                    {
                        productNeedsDecision = new BrokeDecisionProduct
                        {
                            ProductState = ProductState.NeedsDecision,
                            Quantity = BrokeDecisionProduct.MaxQuantity - sumQuantity,
                            MaxQuantity = BrokeDecisionProduct.MaxQuantity,
                            ProductId = BrokeDecisionProduct.ProductId,
                            Number = BrokeDecisionProduct.Number,
                            NomenclatureName = BrokeDecisionProduct.NomenclatureName,
                            MeasureUnit = BrokeDecisionProduct.MeasureUnit
                        };
                        BrokeDecisionProducts.Add(productNeedsDecision);
                    }
                    else
                    {
                        productNeedsDecision.Quantity += BrokeDecisionProduct.Quantity;
                        Quantity = 0;
                    }
                    BrokeDecisionProducts.Remove(BrokeDecisionProduct);
                }
            }
        }

        public string Name { get; set; }

        private ProductState ProductState { get; set; }

        private BrokeDecisionProduct _brokeDecisionProduct;

        public BrokeDecisionProduct BrokeDecisionProduct
        {
            get { return _brokeDecisionProduct; }
            set
            {
                _brokeDecisionProduct = value;
                if (_brokeDecisionProduct != null)
                {
                    NomenclatureID = _brokeDecisionProduct.NomenclatureId;
                    CharacteristicID = _brokeDecisionProduct.CharacteristicId;
                }
                RaisePropertyChanged("BrokeDecisionProduct");
            }
        }
    }
}