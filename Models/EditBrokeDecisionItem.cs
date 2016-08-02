using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Common;
using Gamma.ViewModels;

namespace Gamma.Models
{
    public class EditBrokeDecisionItem: DbEditItemWithNomenclatureViewModel
    {
        public EditBrokeDecisionItem(string name, Gamma.ProductStates productState, ItemsChangeObservableCollection<BrokeDecisionProduct> decisionProducts, bool canChooseNomenclature = false)
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
                    var sumQuantity =
                        BrokeDecisionProducts.Except(new List<BrokeDecisionProduct> {BrokeDecisionProduct})
                            .Where(p => p.ProductId == BrokeDecisionProduct.ProductId)
                            .Sum(p => p.Quantity);
                    if (sumQuantity + value > BrokeDecisionProduct.MaxQuantity)
                        value = BrokeDecisionProduct.MaxQuantity - sumQuantity;
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
                _isChecked = value;
                RaisePropertyChanged("IsChecked");
                if (BrokeDecisionProduct == null) return;
                if (value)
                {
                    if (!BrokeDecisionProducts.Contains(BrokeDecisionProduct))
                        BrokeDecisionProducts.Add(BrokeDecisionProduct);
                    var productNeedsDecision = BrokeDecisionProducts.FirstOrDefault(
                        bp =>
                            bp.ProductId == BrokeDecisionProduct.ProductId &&
                            bp.ProductState == Gamma.ProductStates.NeedsDecision);
                    var sumQuantity = BrokeDecisionProducts.Where(p => p.ProductId == BrokeDecisionProduct.ProductId)
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
                                ProductState = Gamma.ProductStates.NeedsDecision,
                                NomenclatureName = BrokeDecisionProduct.NomenclatureName
                            };
                            BrokeDecisionProducts.Add(productNeedsDecision);
                        }
                    }

                }
                else
                {
                    var needDecisionProduct = new BrokeDecisionProduct
                    {
                        ProductState = Gamma.ProductStates.NeedsDecision,
                        Quantity = BrokeDecisionProduct.MaxQuantity,
                        MaxQuantity = BrokeDecisionProduct.MaxQuantity,
                        ProductId = BrokeDecisionProduct.ProductId,
                        Number = BrokeDecisionProduct.Number,
                        NomenclatureName = BrokeDecisionProduct.Number
                    };
                    BrokeDecisionProducts.Remove(BrokeDecisionProduct);
                    if (BrokeDecisionProducts.All(bp => bp.ProductId != BrokeDecisionProduct.ProductId))
                    { 
                        BrokeDecisionProducts.Add(needDecisionProduct);
                    }
                    Quantity = 0;
                }
            }
        }

        public string Name { get; set; }

        private Gamma.ProductStates ProductState { get; set; }

        private BrokeDecisionProduct _brokeDecisionProduct;

        public BrokeDecisionProduct BrokeDecisionProduct
        {
            get { return _brokeDecisionProduct; }
            set
            {
                _brokeDecisionProduct = value;
                RaisePropertyChanged("BrokeDecisionProduct");
            }
        }
    }
}