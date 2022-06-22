using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Controllers;
using Gamma.DialogViewModels;
using Gamma.Models;
using Gamma.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Gamma.ViewModels
{
    public class DocBrokeDecisionViewModel : SaveImplementedViewModel
    {
        public DocBrokeDecisionViewModel(Guid docBrokeID)// DocBrokeViewModel parentViewModel)
        {
            DB.AddLogMessageInformation("Открытие Решений по акту о браке DocID", "Open DocBrokeDecisionViewModel (docBrokeID = '" + docBrokeID + "')",docBrokeID);
            OpenProductCommand = new DelegateCommand(OpenProduct);
            //ParentViewModel = parentViewModel;
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
            BrokePlaces = WorkSession.Places.Where(p => ((p.IsProductionPlace ?? false) || (p.IsWarehouse ?? false)) && WorkSession.BranchIds.Contains(p.BranchID))
               .Select(p => new Place
               {
                   PlaceGuid = p.PlaceGuid,
                   PlaceID = p.PlaceID,
                   PlaceName = p.Name
               })
               .ToList();
            DocBrokeID = DocBrokeID;// parentViewModel.DocId;
            var IsEditableCollection = new ObservableCollection<bool?>
                (
                    from pt in GammaBase.GetDocBrokeDecisionEditable(WorkSession.UserID, (int?)WorkSession.ShiftID, DocBrokeID, null)
                    select pt
                );
#if (DEBUG)
            IsReadOnly = false;
#else
            IsReadOnly = !((IsEditableCollection.Count > 0) ? (bool)IsEditableCollection[0] : false)
                            || !DB.HaveWriteAccess("DocBrokeDecisionProducts");
#endif
            //IsReadOnly = (doc?.IsConfirmed ?? false) || !DB.HaveWriteAccess("DocBroke");

        }

        //private DocBrokeViewModel ParentViewModel { get; set; }

        public bool IsVisibilityExtendedField =>
#if (DEBUG)
    true;
#else
            WorkSession.DBAdmin;
#endif

        public DelegateCommand OpenProductCommand { get; private set; }

        public List<Place> BrokePlaces { get; set; }

        public ItemsChangeObservableCollection<BrokeDecisionProduct> BrokeDecisionProducts { get; set; } = new ItemsChangeObservableCollection<BrokeDecisionProduct>();

        public Dictionary<ProductState, EditBrokeDecisionItem> EditBrokeDecisionItems { get; set; } = new Dictionary<ProductState, EditBrokeDecisionItem>();

        public void AddBrokeDecisionProduct(Guid docBrokeId, Guid productId, DateTime date, decimal productQuantity, Guid? rejectionReasonID, int? brokePlaceID, bool isChanged = true)
        {
            using (var gammaBase = DB.GammaDbWithNoCheckConnection)
            {
                var product = gammaBase.vProductsBaseInfo
                    .FirstOrDefault(p => p.ProductID == productId);
                if (product == null) return;

                if (DocBrokeID == null)
                    DocBrokeID = docBrokeId;

                var docBrokeDecisionProducts = gammaBase.DocBrokeDecisionProducts.Where(d => d.DocBrokeDecision.DocBrokeID == docBrokeId &&
                        d.ProductID == productId && d.DocBrokeDecision.IsActual).ToList();
                if (docBrokeDecisionProducts.Count == 0)
                {
                    DB.AddLogMessageInformation("Добавление 'Требует решения' по продукту ProductID в Решения по Акту о браке DocID", "NeedsDecision AddBrokeDecisionProduct (docBrokeId = '" + docBrokeId + "', productId='" + productId + "')", docBrokeId, productId);
                    var curDate = DB.CurrentDateTime;
                    BrokeDecisionProducts.Add(new BrokeDecisionProduct(
                        SqlGuidUtil.NewSequentialid(),
                        product.ProductID,
                        (ProductKind)product.ProductKindID,
                        product.Number,
                        productQuantity,
                        ProductState.NeedsDecision,
                        DB.GetProductNomenclatureNameBeforeDate(product.ProductID, date),
                        product.BaseMeasureUnitName,
                        rejectionReasonID,
                        brokePlaceID,
                        product.C1CNomenclatureID,
                        product.C1CCharacteristicID,
                        product.BaseMeasureUnitQuantity ?? 0
                        )
                    {
                        DecisionDate = new DateTime(curDate.Year, curDate.Month, curDate.Day, curDate.Hour, curDate.Minute, curDate.Second),
                        DecisionPlaceId = brokePlaceID,
                        IsChanged = true//isChanged
                    }
                    );
                }
                else
                {
                    DB.AddLogMessageInformation("Загрузка решения по продукту ProductID в Решения по Акту о браке DocID", "AddBrokeDecisionProduct (docBrokeId = '" + docBrokeId + "', productId='" + productId + "')", docBrokeId, productId);
                    foreach (var decisionProduct in docBrokeDecisionProducts//.OrderBy(d => d.DocID).OrderBy(d => d.ProductID)
                        .OrderByDescending(d => d.StateID == (byte)ProductState.ForConversion || d.StateID == (byte)ProductState.Repack))
                    {
                        List<KeyValuePair<Guid, String>> docWithdrawals = new List<KeyValuePair<Guid, string>>();
                        decimal docWithdrawalSum = 0;
                        foreach (var decisionProduct1 in gammaBase.DocBrokeDecisionProducts.Where(d => d.DocBrokeDecision.DocBrokeID == docBrokeId &&
                        d.ProductID == productId && d.StateID == decisionProduct.StateID).ToList())
                        {
                            foreach (var item in decisionProduct1.DocBrokeDecisionProductWithdrawalProducts)
                            {
                                var productionProduct = item.DocWithdrawal.DocProduction.FirstOrDefault()?.DocProductionProducts.FirstOrDefault();
                                Guid? itemID = null;
                                string itemName = "";
                                decimal? itemSum = null;
                                if (decisionProduct.StateID == (byte)ProductState.Broke)
                                {
                                    itemID = item.DocWithdrawalID;
                                    itemSum = item.DocWithdrawal.DocWithdrawalProducts.Where(d => d.ProductID == decisionProduct.ProductID).Sum(d => d.Quantity);
                                    itemName = "Списание " + item.DocWithdrawal.Docs.Number + " от " + item.DocWithdrawal.Docs.Date.ToString("dd.MM.yyyy HH.mm") + " на " + itemSum;
                                }
                                else if (productionProduct != null && (decisionProduct.StateID == (byte)ProductState.ForConversion || decisionProduct.StateID == (byte)ProductState.Repack))
                                {
                                    itemID = productionProduct.ProductID;
                                    itemSum = productionProduct.Quantity;
                                    itemName = "Продукт " + productionProduct.Products.Number + " на " + itemSum;
                                }
                                if (itemID != null && itemID != Guid.Empty)
                                {
                                    docWithdrawals.Add(new KeyValuePair<Guid, string>((Guid)itemID, itemName));
                                    docWithdrawalSum += (decimal)itemSum;
                                }
                            }
                        }
                        var addItem = new BrokeDecisionProduct(
                                decisionProduct.DocID,
                                decisionProduct.ProductID,
                                (ProductKind)product.ProductKindID,
                                product.Number,
                                productQuantity,
                                (ProductState)decisionProduct.StateID,
                                DB.GetProductNomenclatureNameBeforeDate(product.ProductID, date),
                                product.BaseMeasureUnitName,
                                rejectionReasonID,
                                brokePlaceID,
                                product.C1CNomenclatureID,
                                product.C1CCharacteristicID,
                                decisionProduct.Quantity ?? 0,
                                decisionProduct.DecisionApplied,
                                //(docWithdrawalID?.DocWithdrawalID == Guid.Empty ? null : docWithdrawalID?.DocWithdrawalID),
                                docWithdrawals,
                                decisionProduct.DocBrokeDecision.DecisionDate,
                                decisionProduct.DocBrokeDecision.DecisionPlaceID
                            )
                        {
                            //DocId = decisionProduct.DocID,
                            Comment = decisionProduct.Comment,
                            NomenclatureId = decisionProduct.C1CNomenclatureID,
                            CharacteristicId = decisionProduct.C1CCharacteristicID,
                            DocWithdrawalSum = docWithdrawalSum,
                            IsVisibleRow = !(decisionProduct.DecisionApplied && NeedsProductStates.Contains((ProductState)decisionProduct.StateID)),
                            IsChanged = isChanged
                        };
                        bool isAddGodItem = false;
                        BrokeDecisionProduct addGoodItem = null;
                        if (addItem.ProductState == ProductState.Good)
                        {
                            var brokeRepackOrConv = BrokeDecisionProducts.FirstOrDefault(p => p.DecisionDocId == addItem.DecisionDocId && p.ProductId == addItem.ProductId && (p.ProductState == ProductState.ForConversion || p.ProductState == ProductState.Repack));
                            if (brokeRepackOrConv != null && brokeRepackOrConv?.DocWithdrawalSum == addItem.Quantity)
                            {
                                if (brokeRepackOrConv.ProductState == ProductState.ForConversion)
                                    addItem.Decision = "Переделано";
                                else if (brokeRepackOrConv.ProductState == ProductState.Repack)
                                    addItem.Decision = "Переупаковано";
                            }
                        }
                        else if ((addItem.ProductState == ProductState.ForConversion || addItem.ProductState == ProductState.Repack) && addItem.DocWithdrawalSum > 0 && addItem.DocWithdrawalSum < addItem.Quantity)
                        {
                            var brokeGood = BrokeDecisionProducts.FirstOrDefault(p => p.DecisionDocId == addItem.DecisionDocId && p.ProductId == addItem.ProductId && (p.ProductState == ProductState.Good));
                            if (brokeGood == null)
                            {
                                addGoodItem = new BrokeDecisionProduct(
                                decisionProduct.DocID,
                                decisionProduct.ProductID,
                                (ProductKind)product.ProductKindID,
                                product.Number,
                                productQuantity,
                                ProductState.Good,
                                DB.GetProductNomenclatureNameBeforeDate(product.ProductID, date),
                                product.BaseMeasureUnitName,
                                rejectionReasonID,
                                brokePlaceID,
                                product.C1CNomenclatureID,
                                product.C1CCharacteristicID,
                                addItem.DocWithdrawalSum,
                                false,
                                //(docWithdrawalID?.DocWithdrawalID == Guid.Empty ? null : docWithdrawalID?.DocWithdrawalID),
                                null,
                                decisionProduct.DocBrokeDecision.DecisionDate,
                                decisionProduct.DocBrokeDecision.DecisionPlaceID
                            )
                                {
                                    //Comment = decisionProduct.Comment,
                                    //NomenclatureId = decisionProduct.C1CNomenclatureID,
                                    //CharacteristicId = decisionProduct.C1CCharacteristicID,
                                    //ProductKind = (ProductKind)product.ProductKindID,
                                    //DocWithdrawalSum = docWithdrawalSum
                                    Decision = (addItem.ProductState == ProductState.ForConversion) ? "Уже переделано" :
                                                (addItem.ProductState == ProductState.Repack) ? "Уже переупаковано" : "Годная",
                                    IsNotNeedToSave = true,
                                    IsVisibleRow = !(false && NeedsProductStates.Contains(ProductState.Good)),
                                    IsChanged = isChanged
                                };
                                isAddGodItem = true;

                            }
                        }
                        BrokeDecisionProducts.Add(addItem);
                        if (isAddGodItem && addGoodItem != null)
                            BrokeDecisionProducts.Add(addGoodItem);
                    }
                }
            }
        }

        private List<BrokeDecisionProduct> prevBrokeDecisionProduct { get; set; }
        private BrokeDecisionProduct _selectedBrokeDecisionProduct;
        public BrokeDecisionProduct SelectedBrokeDecisionProduct
        {
            get { return _selectedBrokeDecisionProduct; }
            set
            {
                if (Equals(_selectedBrokeDecisionProduct, value)) return;
                IsEnabledEditBrokeDecision = !IsReadOnly && value != null;
                if (_selectedBrokeDecisionProduct != null && value != null && _selectedBrokeDecisionProduct.ProductId == value.ProductId)
                {
                    _selectedBrokeDecisionProduct = value;
                    RaisePropertyChanged("SelectedBrokeDecisionProduct");
                    return;
                }
                DB.AddLogMessageInformation("Выделено решение по продукту ProductID в Решениях по акту о браке DocID", "SET SelectedBrokeDecisionProduct (DecisionDocId = '" + value?.DecisionDocId + "', ProductState='" + value?.ProductState + "', DecisionDate='" + value?.DecisionDate + "', DecisionPlaceId='" + value?.DecisionPlaceId + "')", DocBrokeID, value?.ProductId);
                var prevProductID = _selectedBrokeDecisionProduct?.ProductId;
                _selectedBrokeDecisionProduct = value;
                if (prevProductID != null && IsEditableDecision == true && !SaveBrokeDecisionProductsToModel((Guid)prevProductID))
                    Functions.ShowMessageError("Ошибка при сохранении решения в Акте о браке", "ERROR SaveBrokeDecisionProductsToModel (prevProductID = '" + prevProductID + "', DecisionDocID = '" + DecisionDocID + "')", DocBrokeID, prevProductID);
                prevBrokeDecisionProduct = BrokeDecisionProducts.Where(p => p.ProductId == value?.ProductId).ToList();
                SetExternalRefreshInEditBrokeDecisionItems(true);
                IsChangedDecisionDateRefreshEditBrokeDecisionItem = false;
                foreach (var editBrokeDecisionItem in EditBrokeDecisionItems)
                {
                    editBrokeDecisionItem.Value.Init();
                }
                if (_selectedBrokeDecisionProduct == null)
                {
                    foreach (var editBrokeDecisionItem in EditBrokeDecisionItems)
                    {
                        editBrokeDecisionItem.Value.Update(null, false, 0, String.Empty, true, false, 0, new List<KeyValuePair<Guid, string>>());
                        //editBrokeDecisionItem.Value.BrokeDecisionProduct = null;
                        //editBrokeDecisionItem.Value.IsChecked = false;
                        //editBrokeDecisionItem.Value.Quantity = 0;
                        //editBrokeDecisionItem.Value.Comment = String.Empty;
                        //editBrokeDecisionItem.Value.IsReadOnly = true;
                        //editBrokeDecisionItem.Value.DecisionApplied = false;
                        //editBrokeDecisionItem.Value.DocWithdrawalSum = 0;
                        //editBrokeDecisionItem.Value.DocWithdrawals = new List<KeyValuePair<Guid, string>>();
                    }
                }
                else
                {
                    var products = BrokeDecisionProducts.Where(p => p.ProductId == _selectedBrokeDecisionProduct.ProductId).OrderByDescending(p => p.ProductState == ProductState.NeedsDecision || p.ProductState == ProductState.ForConversion || p.ProductState == ProductState.Repack).ToList();
                    foreach (var product in products)
                    {
                        var decisionProduct = EditBrokeDecisionItems[product.ProductState];
                        decisionProduct.Update(product, true, product.Quantity, product.Comment, GetReadOnlyForEditBrokeDecisionProduct(product.ProductState), product.DecisionApplied, product.DocWithdrawalSum, product.DocWithdrawals, product.IsNotNeedToSave, product.IsVisibleRow);
                        //decisionProduct.BrokeDecisionProduct = null;
                        //decisionProduct.Quantity = product.Quantity;
                        //decisionProduct.IsChecked = true;
                        //decisionProduct.Comment = product.Comment;
                        //decisionProduct.DecisionApplied = product.DecisionApplied;
                        //decisionProduct.DocWithdrawalSum = product.DocWithdrawalSum;
                        //decisionProduct.DocWithdrawals = product.DocWithdrawals;
                        //decisionProduct.IsReadOnly = GetReadOnlyForEditBrokeDecisionProduct(product.ProductState);
                        //decisionProduct.IsNotNeedToSave = product.IsNotNeedToSave;
                        //decisionProduct.IsVisibleRow = product.IsVisibleRow;
                        //decisionProduct.BrokeDecisionProduct = product;
                    }
                    var productStatesInProducts = products.Select(p => p.ProductState).ToList();
                    foreach (var editBrokeDecisionItem in EditBrokeDecisionItems.Where(d => !productStatesInProducts.Contains(d.Key)))
                    {
                        editBrokeDecisionItem.Value.Update(CreateNewBrokeDecisionProductWithState(_selectedBrokeDecisionProduct, editBrokeDecisionItem.Key), 
                            false, 0, String.Empty, GetReadOnlyForEditBrokeDecisionProduct(editBrokeDecisionItem.Value.ProductState), false, 0, new List<KeyValuePair<Guid, string>>());
                        //editBrokeDecisionItem.Value.BrokeDecisionProduct = null;
                        //editBrokeDecisionItem.Value.IsChecked = false;
                        //editBrokeDecisionItem.Value.Quantity = 0;
                        //editBrokeDecisionItem.Value.Comment = String.Empty;
                        //editBrokeDecisionItem.Value.IsReadOnly = GetReadOnlyForEditBrokeDecisionProduct(editBrokeDecisionItem.Value.ProductState);
                        //editBrokeDecisionItem.Value.DecisionApplied = false;
                        //editBrokeDecisionItem.Value.DocWithdrawalSum = 0;
                        //editBrokeDecisionItem.Value.DocWithdrawals = new List<KeyValuePair<Guid, string>>();
                        //editBrokeDecisionItem.Value.BrokeDecisionProduct =
                            //CreateNewBrokeDecisionProductWithState(_selectedBrokeDecisionProduct, editBrokeDecisionItem.Key);
                    }
                }
                IsUpdatedBrokeDecisionProductItems = false;
                ProductID = _selectedBrokeDecisionProduct?.ProductId;
                DecisionDocID = _selectedBrokeDecisionProduct?.DecisionDocId;
                ProductKind = _selectedBrokeDecisionProduct?.ProductKind;
                IsEditableDecision = null;// _selectedBrokeDecisionProduct?.IsEditableDecision ?? false;
                DecisionDate = _selectedBrokeDecisionProduct?.DecisionDate;
                DecisionPlaceId = _selectedBrokeDecisionProduct?.DecisionPlaceId;
                ProductQuantity = _selectedBrokeDecisionProduct?.ProductQuantity ?? 0;
                RejectionReasonID = _selectedBrokeDecisionProduct?.RejectionReasonID;
                BrokePlaceID = _selectedBrokeDecisionProduct?.BrokePlaceID;
                IsUpdatedBrokeDecisionProductItems = true;
                if (ProductID != null)
                {
                    //var sumQuantityDecisionItem = EditBrokeDecisionItems.Where(d => !NeedsProductStates.Contains(d.Key) && d.Value.Name != "Уже переделано" && d.Value.Name != "Уже переупаковано")
                    //    .Sum(d => d.Value.Quantity);
                    var sumQuantityNeedsDecisionItem = EditBrokeDecisionItems.Where(d => NeedsProductStates.Contains(d.Key))
                        .Sum(d => d.Value.Quantity);
                    //var sumWithdrawalSum = EditBrokeDecisionItems.Where(d => !NeedsProductStates.Contains(d.Key))
                    //    .Sum(d => d.Value.DocWithdrawalSum);
                    var sumWithdrawalSumNeedsDecisionItem = EditBrokeDecisionItems.Where(d => NeedsProductStates.Contains(d.Key))
                        .Sum(d => d.Value.DocWithdrawalSum);
                    //var isExistMoreTwoCheckedItem = EditBrokeDecisionItems.Count(d => d.Value.IsChecked && !NeedsProductStates.Contains(d.Key)) >= 2;
                    var countCheckedDecisionItem = EditBrokeDecisionItems.Count(d => d.Value.IsChecked && !NeedsProductStates.Contains(d.Key));
                    var isExistGoodCheckedItem = EditBrokeDecisionItems.Count(d => d.Value.IsChecked && (d.Key == ProductState.Good || d.Key == ProductState.InternalUsage || d.Key == ProductState.Limited)) > 0;
                    foreach (var editItem in EditBrokeDecisionItems)
                    {
                        editItem.Value.MinQuantity = 0;// editItem.Value.IsChecked && NeedsProductStates.Contains(editItem.Key) ? (decimal)0.001 : 0;
                        editItem.Value.MaxQuantity = (NeedsProductStates.Contains(editItem.Key) ? sumQuantityNeedsDecisionItem
                            : editItem.Value.Quantity + sumQuantityNeedsDecisionItem - sumWithdrawalSumNeedsDecisionItem);
                        //? 0//(editItem.Value.DocWithdrawalSum > 0 ? editItem.Value.DocWithdrawalSum : 0) :
                        // editItem.Value.Quantity + editItem.Value.DocWithdrawalSum + 
                        //(ProductQuantity - sumQuantityDecisionItem - sumWithdrawalSum);
                        //editItem.Value.IsExistMoreTwoCheckedItem = isExistMoreTwoCheckedItem;
                        //editItem.Value.IsExistMoreTwoCheckedItem = editItem.Value.IsChecked ? false : isExistMoreTwoCheckedItem;
                        editItem.Value.CountCheckedDecisionItem = editItem.Value.IsChecked ? null : (byte?)countCheckedDecisionItem;
                        if ((editItem.Key == ProductState.ForConversion || editItem.Key == ProductState.Repack))
                        {
                            editItem.Value.IsExistGoodItem = editItem.Value.IsChecked ? false : isExistGoodCheckedItem;
                        }
                        if (editItem.Key == ProductState.Broke)
                        {
                            editItem.Value.IsExistNeedDecisionMore0 = EditBrokeDecisionItems[ProductState.NeedsDecision]?.Quantity > 0;
                        }
                    }

                    List<KeyValuePair<Guid, String>> docWithdrawals = new List<KeyValuePair<Guid, string>>();
                    var decisionProducts = BrokeDecisionProducts.Where(p => p.ProductId == _selectedBrokeDecisionProduct.ProductId).OrderByDescending(p => NeedsProductStates.Contains(p.ProductState)).ToList();
                    foreach (var product in decisionProducts)
                    {
                        foreach (var docWithdrawalItem in product.DocWithdrawals)
                        {
                            if (!(docWithdrawals.IndexOf(docWithdrawalItem) >= 0))
                                docWithdrawals.Add(docWithdrawalItem);
                            if (product.DocWithdrawalSum > EditBrokeDecisionItems[product.ProductState].MinQuantity)
                            {
                                //if (product.DocWithdrawalSum < EditBrokeDecisionItems[product.ProductState].MaxQuantity)
                                EditBrokeDecisionItems[product.ProductState].MinQuantity = product.DocWithdrawalSum;
                            }
                        }
                        /*foreach (var editItem in EditBrokeDecisionItems.Where(d => d.Value.DocWithdrawals?.Count > 0))
                        {
                            foreach(var docWithdrawalItem in editItem.Value.DocWithdrawals)
                                if (!(docWithdrawals.IndexOf(docWithdrawalItem) >= 0))
                                        docWithdrawals.Add(docWithdrawalItem);
                        }
                        */
                    }
                    var forConversionOrRepackItem = EditBrokeDecisionItems.FirstOrDefault(d => (d.Key == ProductState.ForConversion || d.Key == ProductState.Repack) && d.Value.IsChecked);
                    if (forConversionOrRepackItem.Value != null)
                    {
                        if (forConversionOrRepackItem.Value.MinQuantity > 0)
                        {
                            EditBrokeDecisionItems[ProductState.Good].Update(forConversionOrRepackItem.Key == ProductState.ForConversion ? "Уже переделано" : forConversionOrRepackItem.Key == ProductState.Repack ? "Уже переупаковано" : "Годная",
                                true, forConversionOrRepackItem.Value.MinQuantity, forConversionOrRepackItem.Value.MinQuantity, forConversionOrRepackItem.Value.Quantity > forConversionOrRepackItem.Value.DocWithdrawalSum ? true : false);
                            //EditBrokeDecisionItems[ProductState.Good].Name = forConversionOrRepackItem.Key == ProductState.ForConversion ? "Уже переделано" : forConversionOrRepackItem.Key == ProductState.Repack ? "Уже переупаковано" : "Годная";
                            //EditBrokeDecisionItems[ProductState.Good].Quantity = forConversionOrRepackItem.Value.MinQuantity;
                            //EditBrokeDecisionItems[ProductState.Good].MaxQuantity = forConversionOrRepackItem.Value.MinQuantity;
                            //EditBrokeDecisionItems[ProductState.Good].IsNotNeedToSave = forConversionOrRepackItem.Value.Quantity > forConversionOrRepackItem.Value.DocWithdrawalSum ? true : false;
                            //EditBrokeDecisionItems[ProductState.Good].IsChecked = true;
                            EditBrokeDecisionItems[ProductState.NeedsDecision].IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0 = true;
                            EditBrokeDecisionItems[ProductState.ForConversion].IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0 = true;
                            EditBrokeDecisionItems[ProductState.Repack].IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0 = true;
                        }
                        EditBrokeDecisionItems[ProductState.Good].IsExistForConversionOrRepackItem = true;
                        EditBrokeDecisionItems[ProductState.InternalUsage].IsExistForConversionOrRepackItem = true;
                        EditBrokeDecisionItems[ProductState.Limited].IsExistForConversionOrRepackItem = true;
                    }
                    DocWithdrawals = docWithdrawals;
                    DocWithdrawal = null;
                }
                SetExternalRefreshInEditBrokeDecisionItems(false);
                RaisePropertyChanged("SelectedBrokeDecisionProduct");
            }
        }

        private Guid? ProductID { get; set; }
        private Guid? DecisionDocID { get; set; }
        private Guid? DocBrokeID { get; set; }
        public ProductKind? ProductKind { get; private set; }
        public decimal ProductQuantity { get; private set; }
        public Guid? RejectionReasonID { get; private set; }
        public int? BrokePlaceID { get; private set; }

        private bool IsUpdatedBrokeDecisionProductItems { get; set; } = false;

        /// <summary>
        /// Дата решения уже изменена (чтобы повторно не обновлять при обновлении каждого состояния в решении)
        /// </summary>
        private bool IsChangedDecisionDateRefreshEditBrokeDecisionItem { get; set; } = false;
        public bool IsEnabledEditableDecision => IsEditableDecision == true;

        private bool? _isEditableDecision { get; set; }
        public bool? IsEditableDecision
        {
            get { return _isEditableDecision; }
            set
            {
                if (_isEditableDecision == null && value != null
                    && (RejectionReasonID == null || BrokePlaceID == null)
                    )
                {
                    //MessageBox.Show("Внимание! По продукции не указана причина брака или место актирования. Изменение решения запрещено.", "Вопрос", MessageBoxButton.OK, MessageBoxImage.Information);
                    Functions.ShowMessageError("Изменение решения в Акте о браке: " + Environment.NewLine + "По продукции не указана причина брака или место актирования. Изменение решения запрещено", "ERROR SET IsEditableDecision(value = '" + value + "', DecisionDocID = '" + DecisionDocID + "')", DocBrokeID, ProductID);
                    return;
                }
                else if (_isEditableDecision == true && value == false
                    && //MessageBox.Show("Внимание! При снятии галочки изменения в решении не будут сохранены!"+Environment.NewLine + "Вы уверены?", "Вопрос", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    Functions.ShowMessageQuestion("Изменение решения в Акте о браке: " + Environment.NewLine + "При снятии галочки изменения в решении не будут сохранены! Вы уверены?", "QUEST SET IsEditableDecision(value = '" + value + "', DecisionDocID = '" + DecisionDocID + "')", DocBrokeID, ProductID) != MessageBoxResult.Yes)
                {
                    return;
                }
                else if ((_isEditableDecision == null || _isEditableDecision == false) && value == true
                    && (SelectedBrokeDecisionProduct?.ProductQuantity == EditBrokeDecisionItems.Where(p => !NeedsProductStates.Contains(p.Key)).Sum(p => (p.Key == ProductState.Broke ? p.Value.DocWithdrawalSum : p.Value.Quantity)))
                    )
                {
                    //MessageBox.Show("Внимание! По продукции не указана причина брака или место актирования. Изменение решения запрещено.", "Вопрос", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (WorkSession.RoleName == "QualityInspector" || WorkSession.DBAdmin)
                    {
                        if (MessageBoxResult.Yes != Functions.ShowMessageQuestion("Изменение решения в Акте о браке: " + Environment.NewLine + "Вся продукция в решении уже распределена. Вы уверены, что хотите изменить решение?", "ERROR SET IsEditableDecision(value = '" + value + "', DecisionDocID = '" + DecisionDocID + "')", DocBrokeID, ProductID))
                            return;
                    }
                    else
                    {
                        Functions.ShowMessageError("Изменение решения в Акте о браке: " + Environment.NewLine + "Вся продукция в решении уже распределена. Изменение решения запрещено", "ERROR SET IsEditableDecision(value = '" + value + "', DecisionDocID = '" + DecisionDocID + "')", DocBrokeID, ProductID);
                        return;
                    }
                }

                DB.AddLogMessageInformation((value == true ? "Начато изменение решения" : "Отменено изменение решения") +" по продукту ProductID в Решениях по акту о браке DocID", "SET IsEditableDecision (value = '" + value + "', DecisionDocID='" + DecisionDocID + "')", DocBrokeID, ProductID);
                _isEditableDecision = value;
                RaisePropertyChanged("IsEditableDecision");
                RaisePropertyChanged("IsEnabledEditableDecision");

                if (SelectedBrokeDecisionProduct != null && value != null)
                {
                    foreach( var item in BrokeDecisionProducts.Where(p => p.ProductId == SelectedBrokeDecisionProduct.ProductId))
                        item.IsChanged = (bool)value;
                }

                if (value == true)
                {
                    if (DecisionDate == null)
                        DecisionDate = DateTime.Now;
                    if (DecisionPlaceId == null && SelectedBrokeDecisionProduct != null)
                    {
                        using (var gammaBase = DB.GammaDbWithNoCheckConnection)
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
                DB.AddLogMessageInformation("Изменена дата решения " + value?.ToString("dd.MM.yyyy HH:mm:ss") + " по продукту ProductID в Решениях по акту о браке DocID", "SET DecisionDocDate (value = '" + value?.ToString("dd.MM.yyyy HH:mm:ss") + "', DecisionDocID='" + DecisionDocID + "')", DocBrokeID, ProductID);
                if (IsUpdatedBrokeDecisionProductItems)
                {
                    var newId = SqlGuidUtil.NewSequentialid();
                    DecisionDocID = newId;
                    foreach (var editItem in BrokeDecisionProducts.Where(p => p.ProductId == ProductID))
                    {
                        editItem.DecisionDocId = newId;
                        editItem.DecisionDate = value;
                    }
                    var item = new BrokeDecisionProduct();
                    BrokeDecisionProducts.Add(item);
                    BrokeDecisionProducts.Remove(item);
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
                DB.AddLogMessageInformation("Изменено место решения " + value + " по продукту ProductID в Решениях по акту о браке DocID", "SET DecisionPlaceId (value = '" + value + "', DecisionDocID='" + DecisionDocID + "')", DocBrokeID, ProductID);
                string decisionPlaceName = "";
                if (IsUpdatedBrokeDecisionProductItems)
                {
                    decisionPlaceName = WorkSession.Places.FirstOrDefault(r => r.PlaceID == value)?.Name;
                    foreach (var editItem in BrokeDecisionProducts.Where(p => p.ProductId == ProductID))
                    {
                        editItem.DecisionPlaceId = value;
                        editItem.DecisionPlaceName = decisionPlaceName;
                    }
                    var item = new BrokeDecisionProduct();
                    BrokeDecisionProducts.Add(item);
                    BrokeDecisionProducts.Remove(item);
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
                        case "Списани":
                            MessageManager.OpenDocWithdrawal((Guid)value);
                            break;
                        case "Продукт":
                            MessageManager.OpenDocProduct((ProductKind)ProductKind, (Guid)value);
                            break;
                    }
            }
        }

        private readonly DocumentController documentController = new DocumentController();
        private readonly ProductController productController = new ProductController();

        private void DebugFunc()
        {
            Debug.Print("Кол-во задано");
        }

        public CreateWithdrawalResult CreateWithdrawal(Guid productID, ProductKind productKind, byte stateID, decimal quantity, decimal? productionQuantity, Guid? nomenclatureId = null, Guid? characteristicId = null, int? diameter = null, byte? breakNumber = null, decimal? length = null, int? realFormat = null)
        {
            int koeff = productKind != Gamma.ProductKind.ProductSpool && productKind != Gamma.ProductKind.ProductGroupPack ? 1 : 1000;
            UICommand result = null;
            UICommand okCommand = null;
            decimal newQuantity = 0;
            string messageInvariant = (stateID == 2 ? " для утилизации " : " для нового продукта ");
            if (productKind == Gamma.ProductKind.ProductGroupPack)
            {
                if (Functions.ShowMessageQuestion($"Групповая упаковка будет списана полностью ({(quantity * koeff).ToString()} кг.)! Вы уверены?"
                    , "QUEST in CreateWithdrawal : GroupPack will be disposed of completely. docBrokeID = '" + DocBrokeID + "', productID = '" + productID + "'", DocBrokeID, productID)
                    == MessageBoxResult.Yes)
                {
                    result = okCommand;
                    newQuantity = quantity;
                }
            }
            else
            {
                int quantityMax = (int)(quantity * koeff);
                string message = "Укажите количество " + messageInvariant + Environment.NewLine + "максимальное кол-во - " + quantityMax.ToString();
                var model = new SetQuantityDialogModel(message, "Кол-во, кг/шт/пачка", 1, quantityMax);
                okCommand = new UICommand()
                {
                    Caption = "OK",
                    IsCancel = false,
                    IsDefault = true,
                    Command = new DelegateCommand<CancelEventArgs>(
                x => DebugFunc(),
                x => model.Quantity >= 1 && model.Quantity <= quantityMax),
                };
                var cancelCommand = new UICommand()
                {
                    Id = MessageBoxResult.Cancel,
                    Caption = "Отмена",
                    IsCancel = true,
                    IsDefault = false,
                };
                model.Quantity = quantityMax;
                var dialogService = GetService<IDialogService>("SetQuantityDialog");
                result = dialogService.ShowDialog(
                        dialogCommands: new List<UICommand>() { okCommand, cancelCommand },
                        title: "Кол-во " + messageInvariant,
                        viewModel: model);
                //quantity = model.Quantity * (int)(item.NewPalletCoefficient / item.NewGroupPacksInPallet);
                newQuantity = ((decimal)model.Quantity / koeff);
            }
            CreateWithdrawalResult withdrawalResult = null;
            if (result == okCommand)
            {
                DB.AddLogMessageInformation("Ввод пользователем кол-ва " + (newQuantity.ToString() ?? "NULL") + messageInvariant + " продукта ProductID в Акт о браке DocID", "SetCreateWithdrawalQuantity (Quantity='" + (newQuantity.ToString() ?? "NULL") + "', DecisionDocID='" + DecisionDocID + "', stateId='" + stateID + "', quantity='" + quantity + "', productionQuantity='" + productionQuantity + "', nomenclatureId='" + nomenclatureId + "', characteristicId='" + characteristicId + "', diameter='" + diameter + "', breakNumber='" + breakNumber + "')", DocBrokeID, productID);
                //var docBrokeDecision = documentController.ConstructDoc((Guid)DecisionDocID, DocTypes.DocBrokeDecision, true);
                //if (docBrokeDecision != null && documentController.AddDocBrokeDecision(docBrokeDecision, DocBrokeID))
                {
                    Guid newId = SqlGuidUtil.NewSequentialid();
                    int placeID = 0;
                    if (WorkSession.ShiftID > 0)
                    {
                        placeID = WorkSession.PlaceID;
                    }
                    else
                    {
                        var currentPlaceID = GammaBase.Rests.FirstOrDefault(r => r.ProductID == ProductID)?.PlaceID;
                        if (currentPlaceID == null)
                            if (Functions.ShowMessageQuestion(messageInvariant+": " + Environment.NewLine + "Продукт отсутствует на остатках! Нажмите Да, чтобы продолжить, или Нет, чтобы выйти?", "QUEST SetCreateWithdrawalQuantity docBrokeID = '" + DocBrokeID + "', productID = '" + productID + "'", DocBrokeID, productID)
                            != MessageBoxResult.Yes)
                                return null;
                        var getPlaceID = GetPlace(currentPlaceID, messageInvariant);
                        if (getPlaceID == null)
                        {
                            DB.AddLogMessageInformation("Не выбран передел списания " + messageInvariant + " продукта ProductID в Акт о браке DocID", "NOT SELECTED Place IN SetCreateWithdrawalQuantity docBrokeID = '" + DocBrokeID + "', productID = '" + productID + "'", DocBrokeID, productID);
                            return null;
                        }
                        placeID = (int)getPlaceID;
                    }
                    if (stateID == (byte)ProductState.Broke)
                    {
                        withdrawalResult = documentController.WithdrawProductQuantityFromDocBroke((Guid)DecisionDocID, productID, stateID, newId, true, newQuantity, placeID);
                    }
                    else
                    {
                        var docProduction = documentController.ConstructDoc(newId, DocTypes.DocProduction, true, placeID);
                        if (docProduction != null)
                        {
                            Guid docWithdrawalId = SqlGuidUtil.NewSequentialid();
                            withdrawalResult = documentController.WithdrawProductQuantityFromDocBroke((Guid)DecisionDocID, productID, stateID, docWithdrawalId, true, newQuantity, placeID);
                            if (withdrawalResult != null)
                            {
                                Guid productId = SqlGuidUtil.NewSequentialid();
                                int newDiameter = koeff == 1 ? 0 : (int)Math.Sqrt((double)((diameter * diameter) * newQuantity / productionQuantity));
                                int newLength = koeff == 1 ? 0 : (int)((length ?? 0) * (newQuantity / productionQuantity));
                                var product = productController.AddNewProductToDocProduction(docProduction, docWithdrawalId, productKind, (Guid)nomenclatureId, (Guid)characteristicId, newQuantity, newDiameter, breakNumber, newLength, realFormat);
                                withdrawalResult = product == null ? null : new CreateWithdrawalResult(product.ProductID, product.Number, docProduction.Date, product.ProductKind, product.Quantity, docWithdrawalId, placeID) ;
                            }
                        }
                    }
                }
                if (withdrawalResult == null)
                {
                    Functions.ShowMessageError("Списание в Акт о браке: " + Environment.NewLine + "Ошибка! Операция не проведена!", "ERROR CreateWithdrawal (Quantity='" + (newQuantity.ToString() ?? "NULL") + "', DecisionDocID='" + DecisionDocID + "', stateId='" + stateID + "', quantity='" + quantity + "', productionQuantity='" + productionQuantity + "', nomenclatureId='" + nomenclatureId + "', characteristicId='" + characteristicId + "', diameter='" + diameter + "', breakNumber='" + breakNumber + "')", DocBrokeID, productID);
                    return null;
                }
            }
            else
            {
                DB.AddLogMessageInformation("Отмена ввода пользователем кол-ва " + (newQuantity.ToString() ?? "NULL") + messageInvariant + " продукта ProductID в Акт о браке DocID", "SetCreateWithdrawalQuantity (Quantity='" + (newQuantity.ToString() ?? "NULL") + "', DecisionDocID='" + DecisionDocID + "', stateId='" + stateID + "', quantity='" + quantity + "', productionQuantity='" + productionQuantity + "', nomenclatureId='" + nomenclatureId + "', characteristicId='" + characteristicId + "', diameter='" + diameter + "', breakNumber='" + breakNumber + "')", DocBrokeID, productID);
                return null;
            }
            return withdrawalResult;
        }

        public CreateWithdrawalResult CreateWithdrawal(byte stateID, decimal quantity, decimal? productionQuantity, Guid? nomenclatureId = null, Guid? characteristicId = null, int? diameter = null, byte? breakNumber = null, decimal? length = null, int? realFormat = null)
        {
            var docWithdrawalResult =
                stateID == (byte)ProductState.Broke ? CreateWithdrawal((Guid)ProductID, (ProductKind)ProductKind, stateID, quantity, productionQuantity)
                : stateID == (byte)ProductState.ForConversion || stateID == (byte)ProductState.Repack ? CreateWithdrawal((Guid)ProductID, (ProductKind)ProductKind, stateID, quantity, productionQuantity, (Guid)nomenclatureId, (Guid)characteristicId, diameter, breakNumber, length, realFormat)
                : null;
            if (docWithdrawalResult != null)
            {
                if (docWithdrawalResult.ProductID != Guid.Empty)
                    MessageManager.OpenDocProduct(docWithdrawalResult.ProductKind, (Guid)docWithdrawalResult.ProductID);
                using (var gammaBase = DB.GammaDbWithNoCheckConnection)
                {
                    //var number = gammaBase.Docs.FirstOrDefault(d => d.DocID == docWithdrawalResult.DocID)?.Number;
                    List<KeyValuePair<Guid, String>> itemDocWithdrawals =
                        new List<KeyValuePair<Guid, string>>()
                        { new KeyValuePair<Guid, string>
                            (
                            stateID == (byte)ProductState.Broke ? docWithdrawalResult.DocID : docWithdrawalResult.ProductID,
                            (stateID == (byte)ProductState.Broke ? "Списание " : "Продукт " ) + docWithdrawalResult.Number + " от " + docWithdrawalResult.Date.ToString("dd.MM.yyyy HH.mm") + " на " + docWithdrawalResult.Quantity
                            )
                        };
                    /*var brokeDecisionItem = BrokeDecisionProducts.FirstOrDefault(d => d.ProductId == ProductID && d.ProductState == (ProductState)stateID);
                    if (brokeDecisionItem != null)
                    {
                        foreach (var docItem in brokeDecisionItem.DocWithdrawals)
                            itemDocWithdrawals.Add(docItem);
                        brokeDecisionItem.DocWithdrawals = itemDocWithdrawals;
                        brokeDecisionItem.DocWithdrawalSum += docWithdrawalResult.Quantity;
                    }*/
                    if (EditBrokeDecisionItems[(ProductState)stateID] != null)
                    {
                        foreach (var docItem in EditBrokeDecisionItems[(ProductState)stateID].DocWithdrawals)
                            itemDocWithdrawals.Add(docItem);
                        EditBrokeDecisionItems[(ProductState)stateID].DocWithdrawals = itemDocWithdrawals;
                        EditBrokeDecisionItems[(ProductState)stateID].DocWithdrawalSum += docWithdrawalResult.Quantity;
                    }
                    List<KeyValuePair<Guid, String>> docWithdrawals =
                        new List<KeyValuePair<Guid, string>>()
                        { new KeyValuePair<Guid, string>
                            (
                            stateID == (byte)ProductState.Broke ? docWithdrawalResult.DocID : docWithdrawalResult.ProductID,
                            (stateID == (byte)ProductState.Broke ? "Списание " : "Продукт " ) + docWithdrawalResult.Number + " от " + docWithdrawalResult.Date.ToString("dd.MM.yyyy HH.mm") + " на " + docWithdrawalResult.Quantity
                            )
                        };
                    foreach (var docItem in DocWithdrawals)
                        if (!(docWithdrawals.IndexOf(docItem) >= 0))
                            docWithdrawals.Add(docItem);
                    DocWithdrawals = docWithdrawals;

                    if ((ProductID ?? Guid.Empty) != Guid.Empty && docWithdrawalResult.DocID != Guid.Empty
                        && (stateID == (byte)ProductState.ForConversion || stateID == (byte)ProductState.Broke)
                        && (EditBrokeDecisionItems[ProductState.ForConversion].Quantity > 0 && EditBrokeDecisionItems[ProductState.ForConversion].Quantity == EditBrokeDecisionItems[ProductState.ForConversion].DocWithdrawalSum)
                        && (EditBrokeDecisionItems[ProductState.Broke].Quantity == 0 || (EditBrokeDecisionItems[ProductState.Broke].Quantity > 0 && EditBrokeDecisionItems[ProductState.Broke].Quantity == EditBrokeDecisionItems[ProductState.Broke].DocWithdrawalSum)))
                    {
                        var docRepackProduct = gammaBase.DocRepackProducts.FirstOrDefault(r => r.ProductID == ProductID && r.DocBrokeID == DocBrokeID);
                        if (docRepackProduct == null)
                        {
                            Guid docRepackId = SqlGuidUtil.NewSequentialid();
                            //Guid? docBrokeId;
                            var docRepack = documentController.ConstructDoc(docRepackId, DocTypes.DocRepack, true, docWithdrawalResult.PlaceID);
                            if (docRepack != null && DocBrokeID != null)// (docBrokeId = GammaBase.DocBrokeDecision.FirstOrDefault(b => b.DocID == docBrokeDecisionId)?.DocBrokeID) != null)
                            {
                                docRepack.DocRepack = new DocRepack
                                {
                                    DocID = docRepackId,
                                    DocRepackProducts = new List<DocRepackProducts>
                                                {
                                                    new DocRepackProducts
                                                    {
                                                        DocRepackProductID = SqlGuidUtil.NewSequentialid(),
                                                        DocID = docRepackId,
                                                        ProductID = (Guid)ProductID,
                                                        DocBrokeID = DocBrokeID,
                                                        //IsConfirmed = true,
                                                        Date = docRepack.Date,
                                                        //StateID = stateId,
                                                        QuantityGood = EditBrokeDecisionItems[ProductState.ForConversion].Quantity,
                                                        QuantityBroke = EditBrokeDecisionItems[ProductState.Broke]?.Quantity,
                                                        Quantity = EditBrokeDecisionItems[ProductState.ForConversion].Quantity + (EditBrokeDecisionItems[ProductState.Broke]?.Quantity ?? 0),
                                                        DocWithdrawal = new List<DocWithdrawal> { gammaBase.Docs.FirstOrDefault(d => d.DocID == docWithdrawalResult.DocID)?.DocWithdrawal}
                                                    }
                                                }
                                };
                                gammaBase.Docs.Add(docRepack);
                            }
                        }
                        else
                        {
                            docRepackProduct.QuantityGood = EditBrokeDecisionItems[ProductState.ForConversion].Quantity;
                            docRepackProduct.QuantityBroke = EditBrokeDecisionItems[ProductState.Broke]?.Quantity;
                            docRepackProduct.Quantity = EditBrokeDecisionItems[ProductState.ForConversion].Quantity + (EditBrokeDecisionItems[ProductState.Broke]?.Quantity ?? 0);
                            docRepackProduct.DocWithdrawal.Add(gammaBase.Docs.FirstOrDefault(d => d.DocID == docWithdrawalResult.DocID)?.DocWithdrawal);
                        }
                    }
                    gammaBase.SaveChanges();
                }
            };
            return docWithdrawalResult;
        }

        private bool GetReadOnlyForEditBrokeDecisionProduct(ProductState productState)
        {
            switch (productState)
            {
                case ProductState.InternalUsage:
                    return //WorkSession.PlaceGroup != PlaceGroup.Other || 
                        IsReadOnly;
                case ProductState.Good:
                    return IsReadOnly;
                case ProductState.Limited:
                    return //WorkSession.PlaceGroup != PlaceGroup.Other || 
                        IsReadOnly;
                case ProductState.Broke:
                    return IsReadOnly;
                case ProductState.ForConversion:
                    return //WorkSession.PlaceGroup != PlaceGroup.Other || 
                        IsReadOnly;
                case ProductState.Repack:
                    return //WorkSession.PlaceGroup != PlaceGroup.Other || 
                        IsReadOnly;
                case ProductState.NeedsDecision:
                    return //WorkSession.PlaceGroup != PlaceGroup.Other || 
                        IsReadOnly;
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
                    product.DecisionDocId,
                    product.ProductId,
                    product.ProductKind,
                    product.Number,
                    product.ProductQuantity,
                    productState,
                    product.NomenclatureName,
                    product.MeasureUnit,
                    product.RejectionReasonID,
                    product.BrokePlaceID,
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
                DB.AddLogMessageInformation("Изменено для всех решение '" + (ProductState)forAllProductsProkeDecision.ProductStateID.Key + "', " + forAllProductsProkeDecision.NomenclatureName + "', комментарий '" + forAllProductsProkeDecision.Comment + "' в Акте о браке DocID", "SetDecisionForAllProduct (ProductState='" + (ProductState)forAllProductsProkeDecision.ProductStateID.Key + "', NomenclatureId='" + forAllProductsProkeDecision.NomenclatureID + "', CharacteristicID='" + forAllProductsProkeDecision.CharacteristicID + "', Comment='" + forAllProductsProkeDecision.Comment + "')", DocBrokeID);
                var selectedBrokeDecisionProduct = SelectedBrokeDecisionProduct;
                foreach (var brokeDecisionProduct in BrokeDecisionProducts)
                {

                    brokeDecisionProduct.ProductState = (ProductState)forAllProductsProkeDecision.ProductStateID.Key;
                    brokeDecisionProduct.Comment = forAllProductsProkeDecision.Comment;
                    brokeDecisionProduct.Quantity = brokeDecisionProduct.ProductQuantity;
                    brokeDecisionProduct.IsChanged = true;
                    //brokeDecisionProduct.Quantity = ParentViewModel.BrokeProducts.Where(p => p.ProductId == brokeDecisionProduct.ProductId).Select(p => p.Quantity).First();
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

        public void RefreshEditBrokeDecisionItem(ProductState productState, bool updatedMainFields)
        {
            SetExternalRefreshInEditBrokeDecisionItems(true);
            var prevEditNeedsBrokeDecisionItem = EditBrokeDecisionItems.FirstOrDefault(d => d.Value.IsChecked && d.Key != productState && NeedsProductStates.Contains(d.Key));
            var currentEditBrokeDecisionItem = EditBrokeDecisionItems[productState];
            DB.AddLogMessageInformation("Начато обновление решения (при изменении) по продукту ProductID в Решениях по акту о браке DocID", "RefreshEditBrokeDecisionItem (DecisionDocID = '" + DecisionDocID + "', ProductState='" + productState + "', IsChecked='" + currentEditBrokeDecisionItem?.IsChecked + "', Quantity='" + currentEditBrokeDecisionItem?.Quantity + "', MinQuantity='" + currentEditBrokeDecisionItem?.MinQuantity + "', MaxQuantity='" + currentEditBrokeDecisionItem?.MaxQuantity + "', Name='" + currentEditBrokeDecisionItem?.Name + "', DecisionAppliedLabel='" + currentEditBrokeDecisionItem?.DecisionAppliedLabel + "', DecisionApplied='" + currentEditBrokeDecisionItem?.DecisionApplied + "', IsReadOnly='" + currentEditBrokeDecisionItem?.IsReadOnly + "', IsReadOnlyDecisionApplied='" + currentEditBrokeDecisionItem?.IsReadOnlyDecisionApplied + "', IsReadOnlyFields='" + currentEditBrokeDecisionItem?.IsReadOnlyFields + "', IsReadOnlyIsChecked='" + currentEditBrokeDecisionItem?.IsReadOnlyIsChecked + "', IsReadOnlyQuantity='" + currentEditBrokeDecisionItem?.IsReadOnlyQuantity + "', IsValid='" + currentEditBrokeDecisionItem?.IsValid + "', NomenclatureID='" + currentEditBrokeDecisionItem?.NomenclatureID + "', CharacteristicID='" + currentEditBrokeDecisionItem?.CharacteristicID + "', Comment='" + currentEditBrokeDecisionItem?.Comment + "')", DocBrokeID, ProductID);
            foreach (var item in EditBrokeDecisionItems.Where( i => i.Key != productState))
            {
                DB.AddLogMessageInformation("Начато обновление остальных решений (при изменении) по продукту ProductID в Решениях по акту о браке DocID", "RefreshEditBrokeDecisionItem (DecisionDocID = '" + DecisionDocID + "', ProductState='" + productState + "', IsChecked='" + item.Value.IsChecked + "', Quantity='" + item.Value.Quantity + "', MinQuantity='" + item.Value.MinQuantity + "', MaxQuantity='" + item.Value.MaxQuantity + "', Name='" + item.Value.Name + "', DecisionAppliedLabel='" + item.Value.DecisionAppliedLabel + "', DecisionApplied='" + item.Value.DecisionApplied + "', IsReadOnly='" + item.Value.IsReadOnly + "', IsReadOnlyDecisionApplied='" + item.Value.IsReadOnlyDecisionApplied + "', IsReadOnlyFields='" + item.Value.IsReadOnlyFields + "', IsReadOnlyIsChecked='" + item.Value.IsReadOnlyIsChecked + "', IsReadOnlyQuantity='" + item.Value.IsReadOnlyQuantity + "', IsValid='" + item.Value.IsValid + "', NomenclatureID='" + item.Value.NomenclatureID + "', CharacteristicID='" + item.Value.CharacteristicID + "', Comment='" + item.Value.Comment + "')", DocBrokeID, ProductID);
            }
            if (updatedMainFields && !IsChangedDecisionDateRefreshEditBrokeDecisionItem)
            {
               
                //IsUpdatedBrokeDecisionProductItems = false;
                DecisionDate = DateTime.Now;
                //DecisionDocID = SqlGuidUtil.NewSequentialid();
                var currentPlaceID = GammaBase.Rests.FirstOrDefault(r => r.ProductID == ProductID)?.PlaceID;
                if (currentPlaceID != null)
                {
                    DecisionPlaceId = currentPlaceID;
                    DB.AddLogMessageInformation("Изменены дата и передел решения ProductID в Решениях по акту о браке DocID", "Edit DecisionDate in RefreshEditBrokeDecisionItem (DecisionDocID = '" + DecisionDocID + "', ProductState='" + productState + "', IsChecked='" + currentEditBrokeDecisionItem?.IsChecked + "', Quantity='" + currentEditBrokeDecisionItem?.Quantity + "', MinQuantity='" + currentEditBrokeDecisionItem?.MinQuantity + "', MaxQuantity='" + currentEditBrokeDecisionItem?.MaxQuantity + "', Name='" + currentEditBrokeDecisionItem?.Name + "', DecisionAppliedLabel='" + currentEditBrokeDecisionItem?.DecisionAppliedLabel + "', DecisionApplied='" + currentEditBrokeDecisionItem?.DecisionApplied + "', IsReadOnly='" + currentEditBrokeDecisionItem?.IsReadOnly + "', IsReadOnlyDecisionApplied='" + currentEditBrokeDecisionItem?.IsReadOnlyDecisionApplied + "', IsReadOnlyFields='" + currentEditBrokeDecisionItem?.IsReadOnlyFields + "', IsReadOnlyIsChecked='" + currentEditBrokeDecisionItem?.IsReadOnlyIsChecked + "', IsReadOnlyQuantity='" + currentEditBrokeDecisionItem?.IsReadOnlyQuantity + "', IsValid='" + currentEditBrokeDecisionItem?.IsValid + "', NomenclatureID='" + currentEditBrokeDecisionItem?.NomenclatureID + "', CharacteristicID='" + currentEditBrokeDecisionItem?.CharacteristicID + "', Comment='" + currentEditBrokeDecisionItem?.Comment + "')", DocBrokeID, ProductID);
                }
                else
                {
                    DB.AddLogMessageInformation("Изменена дата решения ProductID в Решениях по акту о браке DocID", "Edit DecisionDate in RefreshEditBrokeDecisionItem (DecisionDocID = '" + DecisionDocID + "', ProductState='" + productState + "', IsChecked='" + currentEditBrokeDecisionItem?.IsChecked + "', Quantity='" + currentEditBrokeDecisionItem?.Quantity + "', MinQuantity='" + currentEditBrokeDecisionItem?.MinQuantity + "', MaxQuantity='" + currentEditBrokeDecisionItem?.MaxQuantity + "', Name='" + currentEditBrokeDecisionItem?.Name + "', DecisionAppliedLabel='" + currentEditBrokeDecisionItem?.DecisionAppliedLabel + "', DecisionApplied='" + currentEditBrokeDecisionItem?.DecisionApplied + "', IsReadOnly='" + currentEditBrokeDecisionItem?.IsReadOnly + "', IsReadOnlyDecisionApplied='" + currentEditBrokeDecisionItem?.IsReadOnlyDecisionApplied + "', IsReadOnlyFields='" + currentEditBrokeDecisionItem?.IsReadOnlyFields + "', IsReadOnlyIsChecked='" + currentEditBrokeDecisionItem?.IsReadOnlyIsChecked + "', IsReadOnlyQuantity='" + currentEditBrokeDecisionItem?.IsReadOnlyQuantity + "', IsValid='" + currentEditBrokeDecisionItem?.IsValid + "', NomenclatureID='" + currentEditBrokeDecisionItem?.NomenclatureID + "', CharacteristicID='" + currentEditBrokeDecisionItem?.CharacteristicID + "', Comment='" + currentEditBrokeDecisionItem?.Comment + "')", DocBrokeID, ProductID);
                }
                IsChangedDecisionDateRefreshEditBrokeDecisionItem = true;
                //IsUpdatedBrokeDecisionProductItems = true;
            }
            bool IsChecked = currentEditBrokeDecisionItem?.IsChecked ?? false;
            //сумма принятых решений
            var sumQuantityDecisionItem = EditBrokeDecisionItems.Where(d => !NeedsProductStates.Contains(d.Key) && d.Value.Name != "Уже переделано" && d.Value.Name != "Уже переупаковано")
                .Sum(d => d.Value.Quantity);
            //var sumWithdrawalSum = EditBrokeDecisionItems.Sum(d => d.Value.DocWithdrawalSum);

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
                /*if (!IsChecked && currentEditBrokeDecisionItem.Quantity != 0 && currentEditBrokeDecisionItem.ProductState != ProductState.Broke)
                {
                    sumQuantityDecisionItem = sumQuantityDecisionItem - currentEditBrokeDecisionItem.Quantity;
                    currentEditBrokeDecisionItem.Quantity = 0;
                }*/
                if ((ProductQuantity - sumQuantityDecisionItem) > 0)
                {
                    if (prevEditNeedsBrokeDecisionItem.Value != null)
                    {
                        prevEditNeedsBrokeDecisionItem.Value.Quantity = ProductQuantity - sumQuantityDecisionItem;
                    }
                    else
                    {
                        EditBrokeDecisionItems[ProductState.NeedsDecision].Update(true,ProductQuantity - sumQuantityDecisionItem);
                        //EditBrokeDecisionItems[ProductState.NeedsDecision].Quantity = ProductQuantity - sumQuantityDecisionItem;
                        //EditBrokeDecisionItems[ProductState.NeedsDecision].IsChecked = true;
                    }
                }
                else if ((ProductQuantity - sumQuantityDecisionItem) == 0)
                {
                    if (prevEditNeedsBrokeDecisionItem.Value != null)
                        prevEditNeedsBrokeDecisionItem.Value.Quantity = 0;
                    //prevEditNeedsBrokeDecisionItem.Value.IsChecked = false;
                }
                else
                { //этого не должно быть, так как здесь кол-во отмеченное в решениях больше, чем общее кол-во продукта
                }
            }

            var sumQuantityNeedsDecisionItem = EditBrokeDecisionItems.Where(d => NeedsProductStates.Contains(d.Key))
                        .Sum(d => d.Value.Quantity);
            var sumWithdrawalSumNeedsDecisionItem = EditBrokeDecisionItems.Where(d => NeedsProductStates.Contains(d.Key))
                .Sum(d => d.Value.DocWithdrawalSum);

            //var isExistMoreTwoCheckedItem = EditBrokeDecisionItems.Count(d => d.Value.IsChecked && !NeedsProductStates.Contains(d.Key)) > 2;
            foreach (var editItem in EditBrokeDecisionItems.OrderByDescending(d => d.Value.IsChecked))
            {
                editItem.Value.MinQuantity = 0;// editItem.Value.IsChecked && NeedsProductStates.Contains(editItem.Key) ? (decimal)0.001 : 0;
                editItem.Value.MaxQuantity = (NeedsProductStates.Contains(editItem.Key) ? sumQuantityNeedsDecisionItem
                            : editItem.Value.Quantity + sumQuantityNeedsDecisionItem - sumWithdrawalSumNeedsDecisionItem);
                //? 0//(editItem.Value.DocWithdrawalSum> 0 ? editItem.Value.DocWithdrawalSum : 0) /
                // editItem.Value.Quantity + editItem.Value.DocWithdrawalSum +
                //(ProductQuantity - sumQuantityDecisionItem - sumWithdrawalSum);
                //editItem.Value.IsExistMoreTwoCheckedItem = isExistMoreTwoCheckedItem;
                //                var brokeDecisionProduct = BrokeDecisionProducts.FirstOrDefault(p => p.ProductId == ProductID && p.ProductState == editItem.Key);
                /*if (brokeDecisionProduct?.DocWithdrawalSum > editItem.Value.MinQuantity)
                {
                    if (brokeDecisionProduct.DocWithdrawalSum < editItem.Value.MaxQuantity)
                        editItem.Value.MinQuantity = brokeDecisionProduct.DocWithdrawalSum;
                }*/
                if (editItem.Value.DocWithdrawalSum > editItem.Value.MinQuantity)
                {
                    //if (editItem.Value.DocWithdrawalSum < editItem.Value.MaxQuantity)
                    editItem.Value.MinQuantity = editItem.Value.DocWithdrawalSum;
                }
                var brokeDecisionProduct = BrokeDecisionProducts.FirstOrDefault(p => p.ProductId == ProductID && p.ProductState == editItem.Key);
                if (editItem.Value.IsChecked)
                {
                    if (brokeDecisionProduct != null)
                    {
                        brokeDecisionProduct.Quantity = editItem.Value.Quantity;
                        brokeDecisionProduct.Comment = editItem.Value.Comment;
                        brokeDecisionProduct.NomenclatureId = editItem.Value.NomenclatureID;
                        brokeDecisionProduct.CharacteristicId = editItem.Value.CharacteristicID;
                        brokeDecisionProduct.DecisionApplied = editItem.Value.DecisionApplied;
                        brokeDecisionProduct.DocWithdrawals = editItem.Value.DocWithdrawals;
                        brokeDecisionProduct.DocWithdrawalSum = editItem.Value.DocWithdrawalSum;
                        brokeDecisionProduct.DecisionDate = DecisionDate;
                        brokeDecisionProduct.DecisionPlaceId = DecisionPlaceId;
                        brokeDecisionProduct.IsNotNeedToSave = editItem.Value.IsNotNeedToSave;
                        brokeDecisionProduct.IsVisibleRow = editItem.Value.IsVisibleRow;
                        brokeDecisionProduct.Decision = editItem.Value.Name;
                    }
                    else
                    {
                        if (1 == 1)// && SelectedBrokeDecisionProduct == null)
                        {
                            var product = BrokeDecisionProducts.FirstOrDefault(p => p.ProductId == ProductID);
                            if (product != null)
                                brokeDecisionProduct = new BrokeDecisionProduct
                                    (product.DecisionDocId,
                                        product.ProductId,
                                        product.ProductKind,
                                        product.Number,
                                        product.ProductQuantity,
                                        editItem.Value.ProductState,
                                        product.NomenclatureName,
                                        product.MeasureUnit,
                                        product.RejectionReasonID,
                                        product.BrokePlaceID,
                                        product.NomenclatureOldId,
                                        product.CharacteristicOldId,
                                        editItem.Value.Quantity
                                    )
                                {
                                    Comment = editItem.Value.Comment,
                                    NomenclatureId = editItem.Value.NomenclatureID,
                                    CharacteristicId = editItem.Value.CharacteristicID,
                                    DecisionApplied = editItem.Value.DecisionApplied,
                                    DocWithdrawals = editItem.Value.DocWithdrawals,
                                    DocWithdrawalSum = editItem.Value.DocWithdrawalSum,
                                    DecisionDate = DecisionDate,
                                    DecisionPlaceId = DecisionPlaceId,
                                    IsNotNeedToSave = editItem.Value.IsNotNeedToSave,
                                    IsVisibleRow = editItem.Value.IsVisibleRow,
                                    Decision = editItem.Value.Name
                                };
                            else
                            {
                                DB.AddLogMessageError("Ошибка при сохранение решения по продукту ProductID в Акт о браке DocID", "ERROR DocBrokeDecision.SaveBrokeDecisionProductsToModel: not find product in DocBrokeDecisionProducts", DocBrokeID, ProductID);
                            }
                        }
                        else
                        {
                            brokeDecisionProduct = new BrokeDecisionProduct
                                (SelectedBrokeDecisionProduct.DecisionDocId,
                                    SelectedBrokeDecisionProduct.ProductId,
                                    SelectedBrokeDecisionProduct.ProductKind,
                                    SelectedBrokeDecisionProduct.Number,
                                    SelectedBrokeDecisionProduct.ProductQuantity,
                                    editItem.Value.ProductState,
                                    SelectedBrokeDecisionProduct.NomenclatureName,
                                    SelectedBrokeDecisionProduct.MeasureUnit,
                                    SelectedBrokeDecisionProduct.RejectionReasonID,
                                    SelectedBrokeDecisionProduct.BrokePlaceID,
                                    SelectedBrokeDecisionProduct.NomenclatureOldId,
                                    SelectedBrokeDecisionProduct.CharacteristicOldId,
                                    editItem.Value.Quantity
                                )
                            {
                                Comment = editItem.Value.Comment,
                                NomenclatureId = editItem.Value.NomenclatureID,
                                CharacteristicId = editItem.Value.CharacteristicID,
                                DecisionApplied = editItem.Value.DecisionApplied,
                                DocWithdrawals = editItem.Value.DocWithdrawals,
                                DocWithdrawalSum = editItem.Value.DocWithdrawalSum,
                                DecisionDate = DecisionDate,
                                DecisionPlaceId = DecisionPlaceId,
                                IsNotNeedToSave = editItem.Value.IsNotNeedToSave,
                                IsVisibleRow = editItem.Value.IsVisibleRow,
                                Decision = editItem.Value.Name
                            };
                        }
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
                /*{
                                    if (brokeDecisionProduct != null)
                                    {
                                        brokeDecisionProduct.Quantity = editItem.Value.Quantity;
                                        brokeDecisionProduct.Comment = editItem.Value.Comment;
                                        brokeDecisionProduct.NomenclatureId = editItem.Value.NomenclatureID;
                                        brokeDecisionProduct.CharacteristicId = editItem.Value.CharacteristicID;
                                        brokeDecisionProduct.DecisionApplied = editItem.Value.DecisionApplied;
                                        brokeDecisionProduct.DocWithdrawals = editItem.Value.DocWithdrawals;
                                        brokeDecisionProduct.DocWithdrawalSum = editItem.Value.DocWithdrawalSum;
                                    }
                                    else
                                    {
                                        brokeDecisionProduct = new BrokeDecisionProduct
                                            (SelectedBrokeDecisionProduct.DecisionDocId, 
                                                SelectedBrokeDecisionProduct.ProductId,
                                                SelectedBrokeDecisionProduct.ProductKind,
                                                SelectedBrokeDecisionProduct.Number,
                                                SelectedBrokeDecisionProduct.ProductQuantity,
                                                editItem.Value.ProductState,
                                                SelectedBrokeDecisionProduct.NomenclatureName,
                                                SelectedBrokeDecisionProduct.MeasureUnit,
                                                SelectedBrokeDecisionProduct.RejectionReasonID,
                                                SelectedBrokeDecisionProduct.BrokePlaceID,
                                                SelectedBrokeDecisionProduct.NomenclatureOldId,
                                                SelectedBrokeDecisionProduct.CharacteristicOldId,
                                                editItem.Value.Quantity
                                            )
                                        {
                                            Comment = editItem.Value.Comment,
                                            NomenclatureId = editItem.Value.NomenclatureID,
                                            CharacteristicId = editItem.Value.CharacteristicID,
                                            DecisionApplied = editItem.Value.DecisionApplied,
                                            DocWithdrawals = editItem.Value.DocWithdrawals,
                                            DocWithdrawalSum = editItem.Value.DocWithdrawalSum,
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
                                }*/
            }
            //var isExistMoreTwoCheckedItem = EditBrokeDecisionItems.Count(d => d.Value.IsChecked && !NeedsProductStates.Contains(d.Key)) >= 2;
            var countCheckedDecisionItem = EditBrokeDecisionItems.Count(d => d.Value.IsChecked && !NeedsProductStates.Contains(d.Key));
            foreach (var editItem in EditBrokeDecisionItems)
            {
                //editItem.Value.IsExistMoreTwoCheckedItem = editItem.Value.IsChecked ? false : isExistMoreTwoCheckedItem;
                editItem.Value.CountCheckedDecisionItem = editItem.Value.IsChecked ? null : (byte?)countCheckedDecisionItem;
                if ((productState == ProductState.Good || productState == ProductState.InternalUsage || productState == ProductState.Limited)
                    && (editItem.Key == ProductState.ForConversion || editItem.Key == ProductState.Repack))
                {
                    editItem.Value.IsExistGoodItem = IsChecked;
                }
                if (editItem.Key == ProductState.Broke && !NeedsProductStates.Contains(productState))
                {
                    editItem.Value.IsExistNeedDecisionMore0 = EditBrokeDecisionItems[ProductState.NeedsDecision]?.Quantity > 0;
                }
            }
            if (IsChecked)
            {
                if (productState == ProductState.ForConversion || productState == ProductState.Repack)
                {
                    if (EditBrokeDecisionItems[productState].MinQuantity > 0)
                    {
                        EditBrokeDecisionItems[ProductState.Good].Update(productState == ProductState.ForConversion ? "Уже переделано" : productState == ProductState.Repack ? "Уже переупаковано" : "Годная",
                                true, EditBrokeDecisionItems[productState].MinQuantity, EditBrokeDecisionItems[productState].MinQuantity, EditBrokeDecisionItems[productState].Quantity > EditBrokeDecisionItems[productState].DocWithdrawalSum ? true : false);
                        //EditBrokeDecisionItems[ProductState.Good].Name = productState == ProductState.ForConversion ? "Уже переделано" : productState == ProductState.Repack ? "Уже переупаковано" : "Годная";
                        //EditBrokeDecisionItems[ProductState.Good].Quantity = EditBrokeDecisionItems[productState].MinQuantity;
                        //EditBrokeDecisionItems[ProductState.Good].MaxQuantity = EditBrokeDecisionItems[productState].MinQuantity;
                        //EditBrokeDecisionItems[ProductState.Good].IsNotNeedToSave = EditBrokeDecisionItems[productState].Quantity > EditBrokeDecisionItems[productState].DocWithdrawalSum ? true : false;
                        //EditBrokeDecisionItems[ProductState.Good].IsChecked = true;
                        EditBrokeDecisionItems[ProductState.NeedsDecision].IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0 = true;
                        EditBrokeDecisionItems[ProductState.ForConversion].IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0 = true;
                        EditBrokeDecisionItems[ProductState.Repack].IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0 = true;
                    }
                    else
                    {
                        EditBrokeDecisionItems[ProductState.NeedsDecision].IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0 = false;
                        EditBrokeDecisionItems[ProductState.ForConversion].IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0 = false;
                        EditBrokeDecisionItems[ProductState.Repack].IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0 = false;
                    }
                    EditBrokeDecisionItems[ProductState.Good].IsExistForConversionOrRepackItem = true;
                    EditBrokeDecisionItems[ProductState.InternalUsage].IsExistForConversionOrRepackItem = true;
                    EditBrokeDecisionItems[ProductState.Limited].IsExistForConversionOrRepackItem = true;
                }
                else if (productState == ProductState.NeedsDecision)
                {
                    foreach (var editItem in EditBrokeDecisionItems)
                    {
                        editItem.Value.IsExistForConversionOrRepackItemWithDecisionAppliedSumMore0 = false;
                        editItem.Value.IsExistForConversionOrRepackItem = false;
                    }
                }
            }
            SetExternalRefreshInEditBrokeDecisionItems(false);
            DB.AddLogMessageInformation("Закончено обновление решения (при изменении) по продукту ProductID в Решениях по акту о браке DocID", "RefreshEditBrokeDecisionItem (DecisionDocID = '" + DecisionDocID + "', ProductState='" + productState + "', IsChecked='" + currentEditBrokeDecisionItem?.IsChecked + "', Quantity='" + currentEditBrokeDecisionItem?.Quantity + "', MinQuantity='" + currentEditBrokeDecisionItem?.MinQuantity + "', MaxQuantity='" + currentEditBrokeDecisionItem?.MaxQuantity + "', Name='" + currentEditBrokeDecisionItem?.Name + "', DecisionAppliedLabel='" + currentEditBrokeDecisionItem?.DecisionAppliedLabel + "', DecisionApplied='" + currentEditBrokeDecisionItem?.DecisionApplied + "', IsReadOnly='" + currentEditBrokeDecisionItem?.IsReadOnly + "', IsReadOnlyDecisionApplied='" + currentEditBrokeDecisionItem?.IsReadOnlyDecisionApplied + "', IsReadOnlyFields='" + currentEditBrokeDecisionItem?.IsReadOnlyFields + "', IsReadOnlyIsChecked='" + currentEditBrokeDecisionItem?.IsReadOnlyIsChecked + "', IsReadOnlyQuantity='" + currentEditBrokeDecisionItem?.IsReadOnlyQuantity + "', IsValid='" + currentEditBrokeDecisionItem?.IsValid + "', NomenclatureID='" + currentEditBrokeDecisionItem?.NomenclatureID + "', CharacteristicID='" + currentEditBrokeDecisionItem?.CharacteristicID + "', Comment='" + currentEditBrokeDecisionItem?.Comment + "')", DocBrokeID, ProductID);
            foreach (var item in EditBrokeDecisionItems.Where(i => i.Key != productState))
            {
                DB.AddLogMessageInformation("Закончено обновление остальных решений (при изменении) по продукту ProductID в Решениях по акту о браке DocID", "RefreshEditBrokeDecisionItem (DecisionDocID = '" + DecisionDocID + "', ProductState='" + productState + "', IsChecked='" + item.Value.IsChecked + "', Quantity='" + item.Value.Quantity + "', MinQuantity='" + item.Value.MinQuantity + "', MaxQuantity='" + item.Value.MaxQuantity + "', Name='" + item.Value.Name + "', DecisionAppliedLabel='" + item.Value.DecisionAppliedLabel + "', DecisionApplied='" + item.Value.DecisionApplied + "', IsReadOnly='" + item.Value.IsReadOnly + "', IsReadOnlyDecisionApplied='" + item.Value.IsReadOnlyDecisionApplied + "', IsReadOnlyFields='" + item.Value.IsReadOnlyFields + "', IsReadOnlyIsChecked='" + item.Value.IsReadOnlyIsChecked + "', IsReadOnlyQuantity='" + item.Value.IsReadOnlyQuantity + "', IsValid='" + item.Value.IsValid + "', NomenclatureID='" + item.Value.NomenclatureID + "', CharacteristicID='" + item.Value.CharacteristicID + "', Comment='" + item.Value.Comment + "')", DocBrokeID, ProductID);
            }
        }

        public bool SaveBrokeDecisionProductsToModel(Guid productID, bool onlySaveToModel = true)
        {
            DB.AddLogMessageInformation("Сохранение решения по продукту ProductID в Акт о браке DocID", "DocBrokeDecision.SaveBrokeDecisionProductsToModel (onlySaveToModel='"+ onlySaveToModel + "'", DocBrokeID, productID);
            using (var gammaBase = DB.GammaDbWithNoCheckConnection)
            {
                if (!onlySaveToModel)
                {
                    foreach (var editItem in EditBrokeDecisionItems.OrderByDescending(d => d.Value.IsChecked))
                    {
                        var brokeDecisionProduct = BrokeDecisionProducts.FirstOrDefault(p => p.ProductId == productID && p.ProductState == editItem.Key);
                        if (editItem.Value.IsChecked)
                        {
                            if (brokeDecisionProduct != null)
                            {
                                brokeDecisionProduct.Quantity = editItem.Value.Quantity;
                                brokeDecisionProduct.Comment = editItem.Value.Comment;
                                brokeDecisionProduct.NomenclatureId = editItem.Value.NomenclatureID;
                                brokeDecisionProduct.CharacteristicId = editItem.Value.CharacteristicID;
                                brokeDecisionProduct.DecisionApplied = editItem.Value.DecisionApplied;
                                brokeDecisionProduct.DocWithdrawals = editItem.Value.DocWithdrawals;
                                brokeDecisionProduct.DocWithdrawalSum = editItem.Value.DocWithdrawalSum;
                                brokeDecisionProduct.DecisionDate = DecisionDate;
                                brokeDecisionProduct.DecisionPlaceId = DecisionPlaceId;
                                brokeDecisionProduct.IsNotNeedToSave = editItem.Value.IsNotNeedToSave;
                                brokeDecisionProduct.IsVisibleRow = editItem.Value.IsVisibleRow;
                                brokeDecisionProduct.Decision = editItem.Value.Name;
                            }
                            else
                            {
                                if (1 == 1)// && SelectedBrokeDecisionProduct == null)
                                {
                                    var product = BrokeDecisionProducts.FirstOrDefault(p => p.ProductId == productID);
                                    if (product != null)
                                        brokeDecisionProduct = new BrokeDecisionProduct
                                            (product.DecisionDocId,
                                                product.ProductId,
                                                product.ProductKind,
                                                product.Number,
                                                product.ProductQuantity,
                                                editItem.Value.ProductState,
                                                product.NomenclatureName,
                                                product.MeasureUnit,
                                                product.RejectionReasonID,
                                                product.BrokePlaceID,
                                                product.NomenclatureOldId,
                                                product.CharacteristicOldId,
                                                editItem.Value.Quantity
                                            )
                                        {
                                            Comment = editItem.Value.Comment,
                                            NomenclatureId = editItem.Value.NomenclatureID,
                                            CharacteristicId = editItem.Value.CharacteristicID,
                                            DecisionApplied = editItem.Value.DecisionApplied,
                                            DocWithdrawals = editItem.Value.DocWithdrawals,
                                            DocWithdrawalSum = editItem.Value.DocWithdrawalSum,
                                            DecisionDate = DecisionDate,
                                            DecisionPlaceId = DecisionPlaceId,
                                            IsNotNeedToSave = editItem.Value.IsNotNeedToSave,
                                            IsVisibleRow = editItem.Value.IsVisibleRow,
                                            Decision = editItem.Value.Name
                                        };
                                    else
                                    {
                                        DB.AddLogMessageError("Ошибка при сохранение решения по продукту ProductID в Акт о браке DocID", "ERROR DocBrokeDecision.SaveBrokeDecisionProductsToModel: not find product in DocBrokeDecisionProducts", DocBrokeID, productID);
                                    }
                                }
                                else
                                {
                                    brokeDecisionProduct = new BrokeDecisionProduct
                                        (SelectedBrokeDecisionProduct.DecisionDocId,
                                            SelectedBrokeDecisionProduct.ProductId,
                                            SelectedBrokeDecisionProduct.ProductKind,
                                            SelectedBrokeDecisionProduct.Number,
                                            SelectedBrokeDecisionProduct.ProductQuantity,
                                            editItem.Value.ProductState,
                                            SelectedBrokeDecisionProduct.NomenclatureName,
                                            SelectedBrokeDecisionProduct.MeasureUnit,
                                            SelectedBrokeDecisionProduct.RejectionReasonID,
                                            SelectedBrokeDecisionProduct.BrokePlaceID,
                                            SelectedBrokeDecisionProduct.NomenclatureOldId,
                                            SelectedBrokeDecisionProduct.CharacteristicOldId,
                                            editItem.Value.Quantity
                                        )
                                    {
                                        Comment = editItem.Value.Comment,
                                        NomenclatureId = editItem.Value.NomenclatureID,
                                        CharacteristicId = editItem.Value.CharacteristicID,
                                        DecisionApplied = editItem.Value.DecisionApplied,
                                        DocWithdrawals = editItem.Value.DocWithdrawals,
                                        DocWithdrawalSum = editItem.Value.DocWithdrawalSum,
                                        DecisionDate = DecisionDate,
                                        DecisionPlaceId = DecisionPlaceId,
                                        IsNotNeedToSave = editItem.Value.IsNotNeedToSave,
                                        IsVisibleRow = editItem.Value.IsVisibleRow,
                                        Decision = editItem.Value.Name
                                    };
                                }
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
                var decisionDoc = BrokeDecisionProducts
                        .FirstOrDefault(p => (productID == null || p.ProductId == productID)
                            && !p.IsNotNeedToSave);
                if (decisionDoc != null)
                {
                    var docBrokeDecision = gammaBase.DocBrokeDecision.FirstOrDefault(p => p.DocID == decisionDoc.DecisionDocId);
                    if (docBrokeDecision == null)
                    {
                        var doc = documentController.ConstructDoc(decisionDoc.DecisionDocId, DocTypes.DocBrokeDecision, true);
                        docBrokeDecision = new DocBrokeDecision
                        {
                            DocID = decisionDoc.DecisionDocId,
                            DocBrokeID = DocBrokeID,
                            DecisionDate = decisionDoc.DecisionDate,
                            DecisionPlaceID = decisionDoc.DecisionPlaceId,
                            IsActual = false,
                            ProductID = decisionDoc.ProductId
                        };
                        doc.DocBrokeDecision = docBrokeDecision;
                        gammaBase.Docs.Add(doc);
                    }
                    else
                    {
                        docBrokeDecision.IsActual = false;
                        if (docBrokeDecision.DecisionPlaceID != DecisionPlaceId)
                        {
                            docBrokeDecision.DecisionDate = decisionDoc.DecisionDate;
                            docBrokeDecision.DecisionPlaceID = decisionDoc.DecisionPlaceId;
                        }
                    }
                    var decisionProducts = BrokeDecisionProducts
                        .Where(p => (productID == null || p.ProductId == productID) && p.DecisionDocId == decisionDoc.DecisionDocId
                            && !p.IsNotNeedToSave)
                        .OrderBy(p => NeedsProductStates.Contains(p.ProductState))
                        .ToList();
                    //foreach (var decisionProduct in BrokeDecisionProducts
                    //    .Where(p => (productID == null || p.ProductId == productID) && DecisionDocID == decisionDoc.DecisionDocId
                    //        && !p.IsNotNeedToSave)
                    //    .OrderBy(p => NeedsProductStates.Contains(p.ProductState))) 
                    foreach (var decisionProduct in decisionProducts)
                    {
                        var docBrokeDecisionProducts = gammaBase.DocBrokeDecisionProducts.Where(p => p.DocID == decisionProduct.DecisionDocId && p.ProductID == decisionProduct.ProductId);
                        //.FirstOtDefault();
                        foreach (var docBrokeDecisionProductsItem in docBrokeDecisionProducts)
                        {
                            if (!BrokeDecisionProducts.Any(d => d.ProductId == docBrokeDecisionProductsItem.ProductID && d.ProductState == (ProductState)docBrokeDecisionProductsItem.StateID))
                                gammaBase.DocBrokeDecisionProducts.Remove(docBrokeDecisionProductsItem);
                        }

                        var decision = docBrokeDecisionProducts.FirstOrDefault(d => d.ProductID == decisionProduct.ProductId && d.StateID == (byte)decisionProduct.ProductState);
                        if (decision == null)
                        {
                            gammaBase.DocBrokeDecisionProducts.Add(new DocBrokeDecisionProducts
                            {
                                C1CCharacteristicID = decisionProduct.CharacteristicId,
                                C1CNomenclatureID = decisionProduct.NomenclatureId,
                                Quantity = decisionProduct.Quantity,
                                ProductID = decisionProduct.ProductId,
                                DocID = decisionProduct.DecisionDocId,
                                StateID = (byte)decisionProduct.ProductState,
                                Comment = decisionProduct.Comment,
                                DecisionApplied = decisionProduct.DecisionApplied,
                                DecisionDate = decisionProduct.DecisionDate,
                                DecisionPlaceID = decisionProduct.DecisionPlaceId
                            });
                        }
                        else
                        {
                            decision.C1CCharacteristicID = decisionProduct.CharacteristicId;
                            decision.C1CNomenclatureID = decisionProduct.NomenclatureId;
                            decision.Quantity = decisionProduct.Quantity;
                            decision.Comment = decisionProduct.Comment;
                            decision.DecisionApplied = decisionProduct.DecisionApplied;
                            decision.DecisionDate = decisionProduct.DecisionDate;
                            decision.DecisionPlaceID = decisionProduct.DecisionPlaceId;
                        }
                    }
                    gammaBase.SaveChanges();
                    docBrokeDecision.IsActual = true;
                    gammaBase.SaveChanges();
                }
                foreach (var decisionProduct in BrokeDecisionProducts
                        .Where(p => p.ProductId == productID))
                {
                    decisionProduct.IsChanged = false;
                }
                DB.AddLogMessageInformation("Закончено сохранение решения по продукту ProductID в Акт о браке DocID", "DocBrokeDecision.SaveBrokeDecisionProductsToModel", DocBrokeID, productID);
                return true;
            }
        }
        public bool IsChanged => BrokeDecisionProducts?.Any(d => d.IsChanged) ?? false;

        public override bool SaveToModel()
        {
            DB.AddLogMessageInformation("Сохранение решения в Акт о браке DocID", "DocBrokeDecision.SaveToModel", DocBrokeID);
            foreach (var productID in BrokeDecisionProducts?.Where(d => d.IsChanged).Select(d => d.ProductId).Distinct())
            {
                SaveBrokeDecisionProductsToModel(productID, true);
            }
            /*if (SelectedBrokeDecisionProduct != null && SelectedBrokeDecisionProduct.ProductId != Guid.Empty)
                if (IsEditableDecision == true)
                    return SaveBrokeDecisionProductsToModel(SelectedBrokeDecisionProduct.ProductId);
                else
                {
                    DB.AddLogMessageInformation("Решения сохранять не требуется (нет галочки изменен) в Акт о браке DocID", "DocBrokeDecision.SaveToModel not save", DocBrokeID);
                    return true;
                }
            else
            {
                DB.AddLogMessageInformation("Множественное сохранение Требует решений в Акт о браке DocID", "DocBrokeDecision.SaveToModel - save NeedsDecision", DocBrokeID);
                foreach (var productID in BrokeDecisionProducts.Where(d => d.ProductState == ProductState.NeedsDecision && d.ProductQuantity == d.Quantity).Select(d => d.ProductId).Distinct())
                {
                    SaveBrokeDecisionProductsToModel(productID, true);
                }
            }*/
            return true;
        }

        public IDialogService GetService_SetBrokePlaceDialog { get; set; }

        private int? GetPlace(int? placeId = null, string message = "")
        {
            //if (SelectedBrokeProduct?.RejectionReasons == null) return;
            //MessageManager.EditRejectionReasons(SelectedBrokeProduct);
            var model = new SetBrokePlaceDialogModel(placeId);
            var okCommand = new UICommand()
            {
                Caption = "Сохранить",
                IsCancel = false,
                IsDefault = true,
                Command = new DelegateCommand<CancelEventArgs>(
             x => DebugFunc(),
             x => model.IsSaveEnabled),
            };
            var cancelCommand = new UICommand()
            {
                Id = MessageBoxResult.Cancel,
                Caption = "Отмена",
                IsCancel = true,
                IsDefault = false,
            };
            var dialogService = GetService_SetBrokePlaceDialog;// GetService<IDialogService>("SetBrokePlaceDialog");
            var result = dialogService.ShowDialog(
                dialogCommands: new List<UICommand>() { okCommand, cancelCommand },
                title: "Указание передела " + message,
                viewModel: model);
            return (result != okCommand) ? (int?)null : model.PlaceID;

            //DB.AddLogMessageInformation("Изменение места актирования ProductID в Акт о браке DocID" + Environment.NewLine + "Передел='" + model.Places.FirstOrDefault(p => p.PlaceID == model.PlaceID)?.PlaceName + "'", "SetPlace (PlaceID='" + model.PlaceID + "')", DocId, SelectedBrokeProduct?.ProductId);
        }

        private void OpenProduct()
        {
            if (SelectedBrokeDecisionProduct == null) return;
            MessageManager.OpenDocProduct(SelectedBrokeDecisionProduct.ProductKind, SelectedBrokeDecisionProduct.ProductId);
        }

    }
}
