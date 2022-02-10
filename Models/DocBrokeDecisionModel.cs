using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Gamma.Models
{
    public class DocBrokeDecisionModel : ViewModelBase
    {
        public DocBrokeDecisionModel(DocBrokeViewModel parentViewModel)
        {
            ParentViewModel = parentViewModel;
            //NeedsDecisionProduct =
            EditBrokeDecisionItems.Add(ProductState.NeedsDecision, new EditBrokeDecisionItem("Требует решения", ProductState.NeedsDecision, this));
            //RepackProduct =
            EditBrokeDecisionItems.Add(ProductState.Repack, new EditBrokeDecisionItem("На переупаковку", ProductState.Repack, this));
            //ForConversionProduct =
            EditBrokeDecisionItems.Add(ProductState.ForConversion, new EditBrokeDecisionItem("На переделку", ProductState.ForConversion, this, true));
            //GoodProduct = 
            EditBrokeDecisionItems.Add(ProductState.Good, new EditBrokeDecisionItem("Годная", ProductState.Good, this));
            //InternalUsageProduct = 
            EditBrokeDecisionItems.Add(ProductState.InternalUsage, new EditBrokeDecisionItem("Хоз. нужды", ProductState.InternalUsage, this));
            //LimitedProduct = 
            EditBrokeDecisionItems.Add(ProductState.Limited, new EditBrokeDecisionItem("Ограниченная партия", ProductState.Limited, this));
            //BrokeProduct = 
            EditBrokeDecisionItems.Add(ProductState.Broke, new EditBrokeDecisionItem("На утилизацию", ProductState.Broke, this));
            BrokeDecisionProducts.CollectionChanged += DecisionProductsChanged;
        }

        private DocBrokeViewModel ParentViewModel { get; set; }

        public ItemsChangeObservableCollection<BrokeDecisionProduct> BrokeDecisionProducts { get; set; } = new ItemsChangeObservableCollection<BrokeDecisionProduct>();

        public Dictionary<ProductState, EditBrokeDecisionItem> EditBrokeDecisionItems { get; set; } = new Dictionary<ProductState, EditBrokeDecisionItem>();

        private BrokeDecisionProduct _selectedBrokeDecisionProduct;
        public BrokeDecisionProduct SelectedBrokeDecisionProduct
        {
            get { return _selectedBrokeDecisionProduct; }
            set
            {
                if (Equals(_selectedBrokeDecisionProduct, value)) return;
                IsEnabledEditBrokeDecision = !ParentViewModel.IsReadOnly && value != null;
                if (_selectedBrokeDecisionProduct != null && value != null && _selectedBrokeDecisionProduct.ProductId == value.ProductId)
                {
                    _selectedBrokeDecisionProduct = value;
                    RaisePropertyChanged("SelectedBrokeDecisionProduct");
                    return;
                }
                _selectedBrokeDecisionProduct = value;
                SetExternalRefreshInEditBrokeDecisionItems(true);
                if (value == null)
                {
                    foreach (var editBrokeDecisionItem in EditBrokeDecisionItems)
                    {
                        editBrokeDecisionItem.Value.BrokeDecisionProduct = null;
                        editBrokeDecisionItem.Value.IsChecked = false;
                        editBrokeDecisionItem.Value.Quantity = 0;
                        editBrokeDecisionItem.Value.Comment = String.Empty;
                        editBrokeDecisionItem.Value.IsReadOnly = true;
                        editBrokeDecisionItem.Value.DecisionApplied = false;
                        editBrokeDecisionItem.Value.DocWithdrawalID = null;
                    }
                }
                else
                {
                    var products = BrokeDecisionProducts.Where(p => p.ProductId == value.ProductId).OrderByDescending(p => p.ProductState == ProductState.NeedsDecision || p.ProductState == ProductState.ForConversion || p.ProductState == ProductState.Repack).ToList();
                    foreach (var product in products)
                    {
                        var decisionProduct = EditBrokeDecisionItems[product.ProductState];
                        decisionProduct.BrokeDecisionProduct = null;
                        decisionProduct.Quantity = product.Quantity;
                        decisionProduct.IsChecked = true;
                        decisionProduct.Comment = product.Comment;
                        decisionProduct.DecisionApplied = product.DecisionApplied;
                        decisionProduct.DocWithdrawalID = product.DocWithdrawalID;
                        decisionProduct.IsReadOnly = GetReadOnlyForEditBrokeDecisionProduct(product.ProductState);

                        decisionProduct.BrokeDecisionProduct = product;                        
                    }
                    var productStatesInProducts = products.Select(p => p.ProductState).ToList();
                    foreach (var editBrokeDecisionItem in EditBrokeDecisionItems.Where(d => !productStatesInProducts.Contains(d.Key)))
                    {
                        editBrokeDecisionItem.Value.BrokeDecisionProduct = null;
                        editBrokeDecisionItem.Value.IsChecked = false;
                        editBrokeDecisionItem.Value.Quantity = 0;
                        editBrokeDecisionItem.Value.Comment = String.Empty;
                        editBrokeDecisionItem.Value.IsReadOnly = GetReadOnlyForEditBrokeDecisionProduct(editBrokeDecisionItem.Value.ProductState);
                        editBrokeDecisionItem.Value.DecisionApplied = false;
                        editBrokeDecisionItem.Value.DocWithdrawalID = null;

                        editBrokeDecisionItem.Value.BrokeDecisionProduct =
                            CreateNewBrokeDecisionProductWithState(value, editBrokeDecisionItem.Key);
                    }
                }
                SetExternalRefreshInEditBrokeDecisionItems(false);
                IsUpdatedBrokeDecisionProductItems = false;
                ProductID = value?.ProductId;
                IsEditableDecision = false;// value?.IsEditableDecision ?? false;
                DecisionDate = value?.DecisionDate;
                DecisionPlaceId = value?.DecisionPlaceId;
                IsUpdatedBrokeDecisionProductItems = true;
                if (ProductID != null)
                {
                    var productQuantity = ParentViewModel.BrokeProducts.FirstOrDefault(p => p.ProductId == ProductID).Quantity;
                    var sumQuantityDecisionItem = EditBrokeDecisionItems.Where(d => !NeedsProductStates.Contains(d.Key)).Sum(d => d.Value.Quantity);
                    foreach (var editItem in EditBrokeDecisionItems)
                    {
                        editItem.Value.MinQuantity = editItem.Value.IsChecked && NeedsProductStates.Contains(editItem.Key) ? (decimal)0.001 : 0;
                        editItem.Value.MaxQuantity = editItem.Value.Quantity + (productQuantity - sumQuantityDecisionItem);
                    }
                }
                RaisePropertyChanged("SelectedBrokeDecisionProduct");
            }
        }

        private Guid? ProductID { get; set; }

        private bool IsUpdatedBrokeDecisionProductItems { get; set; } = false;

        private bool _isEditableDecision { get; set; } = false;
        public bool IsEditableDecision
        {
            get { return _isEditableDecision; }
            set
            {
                _isEditableDecision = value;
                RaisePropertyChanged("IsEditableDecision");
                if (value == true)
                    {
                        if (DecisionDate == null)
                            DecisionDate = DateTime.Now;
                        if (DecisionPlaceId == null && SelectedBrokeDecisionProduct != null)
                        {
                            using (var gammaBase = DB.GammaDb)
                            {
                                DecisionPlaceId = gammaBase.Rests.FirstOrDefault(r => r.ProductID == ProductID)?.PlaceID;
                            }
                        }
                    }
                
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
                if (IsUpdatedBrokeDecisionProductItems)
                {
                    foreach (var editItem in BrokeDecisionProducts.Where(p => p.ProductId == ProductID))
                        editItem.DecisionDate = value;
                }
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
                string decisionPlaceName = "";
                if (IsUpdatedBrokeDecisionProductItems)
                {
                    using (var gammaBase = DB.GammaDb)
                    {
                        decisionPlaceName = gammaBase.Places.FirstOrDefault(r => r.PlaceID == value)?.Name;
                    }
                    foreach (var editItem in BrokeDecisionProducts.Where(p => p.ProductId == ProductID))
                    {
                        editItem.DecisionPlaceId = value;
                        editItem.DecisionPlaceName = decisionPlaceName;
                    }
                }
            }
        }

        private bool _isEnabledEditBrokeDecision { get; set; } = false;
        public bool IsEnabledEditBrokeDecision
        {
            get { return _isEnabledEditBrokeDecision; }
            set
            {
                _isEnabledEditBrokeDecision = value;
                RaisePropertyChanged("IsEnabledEditBrokeDecision");
            }
        }

        private bool GetReadOnlyForEditBrokeDecisionProduct(ProductState productState)
        {
            switch (productState)
            {
                case ProductState.InternalUsage:
                    return WorkSession.PlaceGroup != PlaceGroup.Other || ParentViewModel.IsReadOnly;
                case ProductState.Good:
                    return ParentViewModel.IsReadOnly;
                case ProductState.Limited:
                    return WorkSession.PlaceGroup != PlaceGroup.Other || ParentViewModel.IsReadOnly;
                case ProductState.Broke:
                    return ParentViewModel.IsReadOnly;
                case ProductState.ForConversion:
                    return WorkSession.PlaceGroup != PlaceGroup.Other || ParentViewModel.IsReadOnly;
                case ProductState.Repack:
                    return WorkSession.PlaceGroup != PlaceGroup.Other || ParentViewModel.IsReadOnly;
                case ProductState.NeedsDecision:
                    return WorkSession.PlaceGroup != PlaceGroup.Other || ParentViewModel.IsReadOnly;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Создает новое решение по продукту на базе другого с отличием состояния и количества
        /// </summary>
        /// <param name="product">исходное решение</param>
        /// <param name="productState">Новое состояние</param>
        /// <returns></returns>
        private BrokeDecisionProduct CreateNewBrokeDecisionProductWithState(BrokeDecisionProduct product,
            ProductState productState)
        {
            var decisionProduct = new BrokeDecisionProduct(
                    product.ProductId,
                    product.ProductKind,
                    product.Number,
                    productState,
                    product.NomenclatureName,
                    product.MeasureUnit,
                    product.NomenclatureOldId,
                    product.CharacteristicOldId,
                    product.DecisionDate,
                    product.DecisionPlaceId
                )
            {
                NomenclatureId = product.NomenclatureId,
                CharacteristicId = product.CharacteristicId
            };
            return decisionProduct;
        }

        private void DecisionProductsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null && e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Contains(SelectedBrokeDecisionProduct))
            {
                SelectedBrokeDecisionProduct =
                    BrokeDecisionProducts.FirstOrDefault(p => p.ProductId == ProductID);
            }
        }

        public void SetDecisionForAllProduct(BrokeDecisionForAllProducts forAllProductsProkeDecision)
        {
            if (BrokeDecisionProducts != null)
            {
                var selectedBrokeDecisionProduct = SelectedBrokeDecisionProduct;
                foreach (var brokeDecisionProduct in BrokeDecisionProducts)
                {

                    brokeDecisionProduct.ProductState = (ProductState)forAllProductsProkeDecision.ProductStateID.Key;
                    brokeDecisionProduct.Comment = forAllProductsProkeDecision.Comment;
                    brokeDecisionProduct.Quantity = ParentViewModel.BrokeProducts.Where(p => p.ProductId == brokeDecisionProduct.ProductId).Select(p => p.Quantity).First();
                    if (forAllProductsProkeDecision.ProductStateID.Key == (int)ProductState.ForConversion)
                    {
                        brokeDecisionProduct.NomenclatureId = forAllProductsProkeDecision.NomenclatureID;
                        brokeDecisionProduct.CharacteristicId = forAllProductsProkeDecision.CharacteristicID;
                    }
                }
                SelectedBrokeDecisionProduct = null;
                SelectedBrokeDecisionProduct = BrokeDecisionProducts.Where(p => p.ProductId == ProductID).FirstOrDefault();
            }
        }

        public List<ProductState> NeedsProductStates { get; private set; } = new List<ProductState>() { ProductState.NeedsDecision, ProductState.ForConversion, ProductState.Repack };

        private void SetExternalRefreshInEditBrokeDecisionItems(bool externalRefresh)
        {
            foreach (var editItem in EditBrokeDecisionItems)
                editItem.Value.ExternalRefresh = externalRefresh;
        }

        public void RefreshEditBrokeDecisionItem(ProductState productState)
        {
            SetExternalRefreshInEditBrokeDecisionItems(true);
            var prevEditNeedsBrokeDecisionItem = EditBrokeDecisionItems.FirstOrDefault(d => d.Value.IsChecked && d.Key != productState && NeedsProductStates.Contains(d.Key));
            var currentEditBrokeDecisionItem = EditBrokeDecisionItems[productState];
            bool IsChecked = currentEditBrokeDecisionItem?.IsChecked ?? false;
            Guid? ProductId = ProductID;
            var productQuantity = ParentViewModel.BrokeProducts.FirstOrDefault(p => p.ProductId == ProductId).Quantity;
            //сумма принятых решений
            var sumQuantityDecisionItem = EditBrokeDecisionItems.Where(d => !NeedsProductStates.Contains(d.Key)).Sum(d => d.Value.Quantity);

            if (NeedsProductStates.Contains(productState))
            {
                if (IsChecked)
                {
                    if (prevEditNeedsBrokeDecisionItem.Value != null)
                    {
                        currentEditBrokeDecisionItem.Quantity = prevEditNeedsBrokeDecisionItem.Value.Quantity;
                        prevEditNeedsBrokeDecisionItem.Value.IsChecked = false;
                        prevEditNeedsBrokeDecisionItem.Value.Quantity = 0;
                    }
                    else
                    {
                        //этого не должно быть - галочка ставиться на требует решения, а предыдущей галочки тртребует решения нету
                        //currentEditBrokeDecisionItem.Quantity = currentEditBrokeDecisionItem.MaxQuantity;
                    }
                }
                else
                {
                    currentEditBrokeDecisionItem.Quantity = 0;
                }
            }
            else
            {
                if (!IsChecked && currentEditBrokeDecisionItem.Quantity != 0)
                {
                    sumQuantityDecisionItem = sumQuantityDecisionItem - currentEditBrokeDecisionItem.Quantity;
                    currentEditBrokeDecisionItem.Quantity = 0;
                }
                if ((productQuantity - sumQuantityDecisionItem) > 0)
                {
                    if (prevEditNeedsBrokeDecisionItem.Value != null)
                    {
                        prevEditNeedsBrokeDecisionItem.Value.Quantity = productQuantity - sumQuantityDecisionItem;
                    }
                    else
                    {
                        EditBrokeDecisionItems[ProductState.NeedsDecision].Quantity = productQuantity - sumQuantityDecisionItem;
                        EditBrokeDecisionItems[ProductState.NeedsDecision].IsChecked = true;
                    }
                }
                else if ((productQuantity - sumQuantityDecisionItem) == 0)
                {
                    prevEditNeedsBrokeDecisionItem.Value.Quantity = 0;
                    //prevEditNeedsBrokeDecisionItem.Value.IsChecked = false;
                }
                else
                { //этого не должно быть, так как здесь кол-во отмеченное в решениях больше, чем общее кол-во продукта
                }
            }
            SetExternalRefreshInEditBrokeDecisionItems(false);

            foreach (var editItem in EditBrokeDecisionItems.OrderByDescending(d => d.Value.IsChecked))
            {
                editItem.Value.MinQuantity = editItem.Value.IsChecked && NeedsProductStates.Contains(editItem.Key) ? (decimal)0.001 : 0;
                editItem.Value.MaxQuantity = editItem.Value.Quantity + ( productQuantity - sumQuantityDecisionItem);
                var brokeDecisionProduct = BrokeDecisionProducts.FirstOrDefault(p => p.ProductId == ProductId && p.ProductState == editItem.Key);
                if (editItem.Value.IsChecked)
                {
                    if (brokeDecisionProduct != null)
                    {
                        brokeDecisionProduct.Quantity = editItem.Value.Quantity;
                        brokeDecisionProduct.Comment = editItem.Value.Comment;
                        brokeDecisionProduct.NomenclatureId = editItem.Value.NomenclatureID;
                        brokeDecisionProduct.CharacteristicId = editItem.Value.CharacteristicID;
                        brokeDecisionProduct.DecisionApplied = editItem.Value.DecisionApplied;
                        brokeDecisionProduct.DocWithdrawalID = editItem.Value.DocWithdrawalID;
                    }
                    else
                    {
                        brokeDecisionProduct = new BrokeDecisionProduct
                            (SelectedBrokeDecisionProduct.ProductId,
                                SelectedBrokeDecisionProduct.ProductKind,
                                SelectedBrokeDecisionProduct.Number,
                                editItem.Value.ProductState,
                                SelectedBrokeDecisionProduct.NomenclatureName,
                                SelectedBrokeDecisionProduct.MeasureUnit,
                                SelectedBrokeDecisionProduct.NomenclatureOldId,
                                SelectedBrokeDecisionProduct.CharacteristicOldId,
                                editItem.Value.Quantity
                            )
                        {
                            Comment = editItem.Value.Comment,
                            NomenclatureId = editItem.Value.NomenclatureID,
                            CharacteristicId = editItem.Value.CharacteristicID,
                            DecisionApplied = editItem.Value.DecisionApplied,
                            DocWithdrawalID = editItem.Value.DocWithdrawalID,
                            DecisionDate = SelectedBrokeDecisionProduct.DecisionDate,
                            DecisionPlaceId = SelectedBrokeDecisionProduct.DecisionPlaceId,
                            DecisionPlaceName = SelectedBrokeDecisionProduct.DecisionPlaceName
                        };
                        BrokeDecisionProducts.Add(brokeDecisionProduct);
                    }
                }
                else
                {
                    if (brokeDecisionProduct != null)
                    {
                        BrokeDecisionProducts.Remove(brokeDecisionProduct);
                    }
                }
            }
        }
    }
}
