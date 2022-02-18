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
            BrokePlaces = GammaBase.Places.Where(p => ((p.IsProductionPlace ?? false) || (p.IsWarehouse ?? false)) && WorkSession.BranchIds.Contains(p.BranchID))
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
                            || !DB.HaveWriteAccess("DocBrokeDecisionProducts"));
#endif
            //IsReadOnly = (doc?.IsConfirmed ?? false) || !DB.HaveWriteAccess("DocBroke");

        }

        //private DocBrokeViewModel ParentViewModel { get; set; }

        public bool IsReadOnly { get; } = false;

        public List<Place> BrokePlaces { get; set; }

        public ItemsChangeObservableCollection<BrokeDecisionProduct> BrokeDecisionProducts { get; set; } = new ItemsChangeObservableCollection<BrokeDecisionProduct>();

        public Dictionary<ProductState, EditBrokeDecisionItem> EditBrokeDecisionItems { get; set; } = new Dictionary<ProductState, EditBrokeDecisionItem>();

        public void AddBrokeDecisionProduct(Guid productId, Guid docId, DateTime date, decimal productQuantity)
        {
            using (var gammaBase = DB.GammaDb)
            {
                var product = gammaBase.vProductsInfo
                    .FirstOrDefault(p => p.ProductID == productId);
                if (product == null) return;

                var docBrokeDecisionProducts = gammaBase.DocBrokeDecisionProducts.Where(d => d.DocID == docId && d.ProductID == productId).ToList();
                if (docBrokeDecisionProducts.Count == 0)
                {
                    BrokeDecisionProducts.Add(new BrokeDecisionProduct(
                        product.ProductID,
                        (ProductKind)product.ProductKindID,
                        product.Number,
                        productQuantity,
                        ProductState.NeedsDecision,
                        DB.GetProductNomenclatureNameBeforeDate(product.ProductID, date),
                        product.BaseMeasureUnit,
                        product.C1CNomenclatureID,
                        product.C1CCharacteristicID,
                        product.BaseMeasureUnitQuantity ?? 0
                        )
                    { DocId = docId }
                    );
                }
                else
                {
                    foreach (var decisionProduct in docBrokeDecisionProducts.OrderBy(d => d.DocID).OrderBy(d => d.ProductID).OrderByDescending(d => d.StateID == (byte)ProductState.ForConversion || d.StateID == (byte)ProductState.Repack))
                    {
                        List<KeyValuePair<Guid, String>> docWithdrawals = new List<KeyValuePair<Guid, string>>();
                        decimal docWithdrawalSum = 0;
                        foreach (var item in decisionProduct.DocBrokeDecisionProductWithdrawalProducts)
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
                        var addItem = new BrokeDecisionProduct(
                                decisionProduct.ProductID,
                                (ProductKind)product.ProductKindID,
                                product.Number,
                                productQuantity,
                                (ProductState)decisionProduct.StateID,
                                DB.GetProductNomenclatureNameBeforeDate(product.ProductID, date),
                                product.BaseMeasureUnit,
                                product.C1CNomenclatureID,
                                product.C1CCharacteristicID,
                                decisionProduct.Quantity ?? 0,
                                decisionProduct.DecisionApplied,
                                //(docWithdrawalID?.DocWithdrawalID == Guid.Empty ? null : docWithdrawalID?.DocWithdrawalID),
                                docWithdrawals,
                                decisionProduct.DecisionDate,
                                decisionProduct.DecisionPlaceID
                            )
                        {
                            DocId = decisionProduct.DocID,
                            Comment = decisionProduct.Comment,
                            NomenclatureId = decisionProduct.C1CNomenclatureID,
                            CharacteristicId = decisionProduct.C1CCharacteristicID,
                            ProductKind = (ProductKind)product.ProductKindID,
                            DecisionPlaceName = gammaBase.Places.FirstOrDefault(p => p.PlaceID == decisionProduct.DecisionPlaceID)?.Name,
                            DocWithdrawalSum = docWithdrawalSum
                        };
                        var brokeRepackOrConv = BrokeDecisionProducts.FirstOrDefault(p => p.DocId == addItem.DocId && p.ProductId == addItem.ProductId && (p.ProductState == ProductState.ForConversion || p.ProductState == ProductState.Repack));
                        if (addItem.ProductState == ProductState.Good && brokeRepackOrConv != null && brokeRepackOrConv?.DocWithdrawalSum == addItem.Quantity)
                        {
                            if (brokeRepackOrConv.ProductState == ProductState.ForConversion)
                                addItem.Decision = "Переделано";
                            else if (brokeRepackOrConv.ProductState == ProductState.Repack)
                                addItem.Decision = "Переупаковано";
                        }
                        BrokeDecisionProducts.Add(addItem);
                    }
                }
            }
        }

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
                if (_selectedBrokeDecisionProduct != null && IsEditableDecision == true && !SaveBrokeDecisionProductsToModel(_selectedBrokeDecisionProduct.ProductId))
                    MessageBox.Show("Ошибка при сохранении решения"," Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _selectedBrokeDecisionProduct = value;
                SetExternalRefreshInEditBrokeDecisionItems(true);
                foreach (var editBrokeDecisionItem in EditBrokeDecisionItems)
                {
                    editBrokeDecisionItem.Value.Init();
                }
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
                        editBrokeDecisionItem.Value.DocWithdrawalSum = 0;
                        editBrokeDecisionItem.Value.DocWithdrawals = new List<KeyValuePair<Guid, string>>();
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
                        decisionProduct.DocWithdrawalSum = product.DocWithdrawalSum;
                        decisionProduct.DocWithdrawals = product.DocWithdrawals;
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
                        editBrokeDecisionItem.Value.DocWithdrawalSum = 0;
                        editBrokeDecisionItem.Value.DocWithdrawals = new List<KeyValuePair<Guid, string>>();
                        editBrokeDecisionItem.Value.BrokeDecisionProduct =
                            CreateNewBrokeDecisionProductWithState(value, editBrokeDecisionItem.Key);
                    }
                }
                IsUpdatedBrokeDecisionProductItems = false;
                ProductID = value?.ProductId;
                DocBrokeID = value?.DocId;
                ProductKind = value?.ProductKind;
                IsEditableDecision = null;// value?.IsEditableDecision ?? false;
                DecisionDate = value?.DecisionDate;
                DecisionPlaceId = value?.DecisionPlaceId;
                ProductQuantity = value?.ProductQuantity ?? 0;
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
                    var isExistMoreTwoCheckedItem = EditBrokeDecisionItems.Count(d => d.Value.IsChecked && !NeedsProductStates.Contains(d.Key)) >= 2;
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
                        editItem.Value.IsExistMoreTwoCheckedItem = editItem.Value.IsChecked ? false : isExistMoreTwoCheckedItem;
                        if ((editItem.Key == ProductState.ForConversion || editItem.Key == ProductState.Repack))
                        {
                            editItem.Value.IsExistGoodItem = editItem.Value.IsChecked ? false : isExistGoodCheckedItem;
                        }
                    }
                }
                List<KeyValuePair<Guid, String>> docWithdrawals = new List<KeyValuePair<Guid, string>>();
                var decisionProducts = BrokeDecisionProducts.Where(p => p.ProductId == value.ProductId).OrderByDescending(p => NeedsProductStates.Contains(p.ProductState)).ToList();
                foreach (var product in decisionProducts)
                {
                    foreach (var docWithdrawalItem in product.DocWithdrawals)
                    {
                        if (!(docWithdrawals.IndexOf(docWithdrawalItem) >= 0))
                            docWithdrawals.Add(docWithdrawalItem);
                        if (product.DocWithdrawalSum > EditBrokeDecisionItems[product.ProductState].MinQuantity)
                        {
                            if (product.DocWithdrawalSum < EditBrokeDecisionItems[product.ProductState].MaxQuantity)
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
                        EditBrokeDecisionItems[ProductState.Good].Name = forConversionOrRepackItem.Key == ProductState.ForConversion ? "Уже переделано" : forConversionOrRepackItem.Key == ProductState.Repack ? "Уже переупаковано" : "Годная";
                        EditBrokeDecisionItems[ProductState.Good].Quantity = forConversionOrRepackItem.Value.MinQuantity;
                        EditBrokeDecisionItems[ProductState.Good].MaxQuantity = forConversionOrRepackItem.Value.MinQuantity;
                        EditBrokeDecisionItems[ProductState.Good].IsChecked = true;
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
                SetExternalRefreshInEditBrokeDecisionItems(false);
                RaisePropertyChanged("SelectedBrokeDecisionProduct");
            }
        }

        private Guid? ProductID { get; set; }
        private Guid? DocBrokeID { get; set; }
        public ProductKind? ProductKind { get; private set; }
        public decimal ProductQuantity { get; private set; }

        private bool IsUpdatedBrokeDecisionProductItems { get; set; } = false;

        public bool IsEnabledEditableDecision => IsEditableDecision == true;

        private bool? _isEditableDecision { get; set; }
        public bool? IsEditableDecision
        {
            get { return _isEditableDecision; }
            set
            {
                if (_isEditableDecision == true && value == false
                    && MessageBox.Show("Внимание! При снятии галочки изменения в решении не будут сохранены!"+Environment.NewLine + "Вы уверены?", "Вопрос", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                    _isEditableDecision = value;
                    RaisePropertyChanged("IsEditableDecision");
                    RaisePropertyChanged("IsEnabledEditableDecision");
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

        public CreateWithdrawalResult CreateWithdrawal(Guid productID, ProductKind productKind, byte stateID, decimal quantity, decimal? productionQuantity, Guid? nomenclatureId = null, Guid? characteristicId = null, int? diameter = null, byte? breakNumber = null)
        {
            int koeff = productKind != Gamma.ProductKind.ProductSpool ? 1 : 1000;
            int quantityMax = (int)(quantity * koeff);
            string message = "Укажите количество для " + (stateID == 2 ? "утилизации " : "нового продукта ") + Environment.NewLine + "максимальное кол-во - " + quantityMax.ToString();
            var model = new SetQuantityDialogModel(message, "Кол-во, кг/шт/пачка", 1, quantityMax);
            var okCommand = new UICommand()
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
            UICommand result = null;
            model.Quantity = quantityMax;
            if (stateID == (byte)ProductState.Repack)
            {
                result = okCommand;
                model.Quantity = quantityMax;
            }
            else
            {
                var dialogService = GetService<IDialogService>("SetQuantityDialog");
                result = dialogService.ShowDialog(
                    dialogCommands: new List<UICommand>() { okCommand, cancelCommand },
                    title: "Кол-во на утилизацию",
                    viewModel: model);
                //quantity = model.Quantity * (int)(item.NewPalletCoefficient / item.NewGroupPacksInPallet);
            }
            CreateWithdrawalResult withdrawalResult = null;
            if (result == okCommand)
            {
                Guid newId = SqlGuidUtil.NewSequentialid();
                decimal newQuantity = ((decimal)model.Quantity / koeff);
                if (stateID == (byte)ProductState.Broke)
                {
                    withdrawalResult = documentController.WithdrawProductQuantityFromDocBroke((Guid)DocBrokeID, productID, stateID, newId, true, newQuantity);
                }
                else
                {
                    var docProduction = documentController.ConstructDoc(newId, DocTypes.DocProduction, true, WorkSession.PlaceID);
                    if (docProduction != null)
                    {
                        Guid docWithdrawalId = SqlGuidUtil.NewSequentialid();
                        withdrawalResult = documentController.WithdrawProductQuantityFromDocBroke((Guid)DocBrokeID, productID, stateID, docWithdrawalId, true, newQuantity);
                        if (withdrawalResult != null)
                        {
                            Guid productId = SqlGuidUtil.NewSequentialid();
                            int newDiameter = productKind != Gamma.ProductKind.ProductSpool ? 0 : (int)Math.Sqrt((double)((diameter * diameter) * newQuantity / productionQuantity));
                            var product = productController.AddNewProductToDocProduction(docProduction, docWithdrawalId, productKind, (Guid)nomenclatureId, (Guid)characteristicId, newQuantity, newDiameter, breakNumber);
                            withdrawalResult = product == null ? null : new CreateWithdrawalResult(product.ProductID, product.Number, docProduction.Date, product.ProductKind, product.Quantity);

                        }
                    }
                }
                if (withdrawalResult == null)
                {
                    MessageBox.Show("Ошибка! Операция не проведена!", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            else
                return null;
            return withdrawalResult;
        }

        public CreateWithdrawalResult CreateWithdrawal(byte stateID, decimal quantity, decimal? productionQuantity, Guid? nomenclatureId = null, Guid? characteristicId = null, int? diameter = null, byte? breakNumber = null)
        {
            var docWithdrawalResult =
                stateID == (byte)ProductState.Broke ? CreateWithdrawal((Guid)ProductID, (ProductKind)ProductKind, stateID, quantity, productionQuantity)
                : stateID == (byte)ProductState.ForConversion || stateID == (byte)ProductState.Repack ? CreateWithdrawal((Guid)ProductID, (ProductKind)ProductKind, stateID, quantity, productionQuantity, (Guid)nomenclatureId, (Guid)characteristicId, diameter, breakNumber)
                : null;
            if (docWithdrawalResult != null)
            {
                if (docWithdrawalResult.ProductID != Guid.Empty)
                    MessageManager.OpenDocProduct(docWithdrawalResult.ProductKind, (Guid)docWithdrawalResult.ProductID);
                using (var gammaBase = DB.GammaDb)
                {
                    //var number = gammaBase.Docs.FirstOrDefault(d => d.DocID == docWithdrawalResult.DocID)?.Number;
                    List<KeyValuePair<Guid, String>> itemDocWithdrawals =
                        new List<KeyValuePair<Guid, string>>()
                        { new KeyValuePair<Guid, string>
                            (
                            stateID == 2 ? docWithdrawalResult.DocID : docWithdrawalResult.ProductID,
                            (stateID == 2 ? "Списание " : "Продукт " ) + docWithdrawalResult.Number + " от " + docWithdrawalResult.Date.ToString("dd.MM.yyyy HH.mm") + " на " + docWithdrawalResult.Quantity
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
                            stateID == 2 ? docWithdrawalResult.DocID : docWithdrawalResult.ProductID,
                            (stateID == 2 ? "Списание " : "Продукт " ) + docWithdrawalResult.Number + " от " + docWithdrawalResult.Date.ToString("dd.MM.yyyy HH.mm") + " на " + docWithdrawalResult.Quantity
                            )
                        };
                    foreach (var docItem in DocWithdrawals)
                        if (!(docWithdrawals.IndexOf(docItem) >= 0))
                            docWithdrawals.Add(docItem);
                    DocWithdrawals = docWithdrawals;
                }
            };
            return docWithdrawalResult;
        }

        private bool GetReadOnlyForEditBrokeDecisionProduct(ProductState productState)
        {
            switch (productState)
            {
                case ProductState.InternalUsage:
                    return WorkSession.PlaceGroup != PlaceGroup.Other || IsReadOnly;
                case ProductState.Good:
                    return IsReadOnly;
                case ProductState.Limited:
                    return WorkSession.PlaceGroup != PlaceGroup.Other || IsReadOnly;
                case ProductState.Broke:
                    return IsReadOnly;
                case ProductState.ForConversion:
                    return WorkSession.PlaceGroup != PlaceGroup.Other || IsReadOnly;
                case ProductState.Repack:
                    return WorkSession.PlaceGroup != PlaceGroup.Other || IsReadOnly;
                case ProductState.NeedsDecision:
                    return WorkSession.PlaceGroup != PlaceGroup.Other || IsReadOnly;
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
                    product.ProductQuantity,
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
                CharacteristicId = product.CharacteristicId,
                DocId = product.DocId
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
                    brokeDecisionProduct.Quantity = brokeDecisionProduct.ProductQuantity;
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

        public void RefreshEditBrokeDecisionItem(ProductState productState)
        {
            SetExternalRefreshInEditBrokeDecisionItems(true);
            var prevEditNeedsBrokeDecisionItem = EditBrokeDecisionItems.FirstOrDefault(d => d.Value.IsChecked && d.Key != productState && NeedsProductStates.Contains(d.Key));
            var currentEditBrokeDecisionItem = EditBrokeDecisionItems[productState];
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
                if (!IsChecked && currentEditBrokeDecisionItem.Quantity != 0)
                {
                    sumQuantityDecisionItem = sumQuantityDecisionItem - currentEditBrokeDecisionItem.Quantity;
                    currentEditBrokeDecisionItem.Quantity = 0;
                }
                if ((ProductQuantity - sumQuantityDecisionItem) > 0)
                {
                    if (prevEditNeedsBrokeDecisionItem.Value != null)
                    {
                        prevEditNeedsBrokeDecisionItem.Value.Quantity = ProductQuantity - sumQuantityDecisionItem;
                    }
                    else
                    {
                        EditBrokeDecisionItems[ProductState.NeedsDecision].Quantity = ProductQuantity - sumQuantityDecisionItem;
                        EditBrokeDecisionItems[ProductState.NeedsDecision].IsChecked = true;
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
                var brokeDecisionProduct = BrokeDecisionProducts.FirstOrDefault(p => p.ProductId == ProductID && p.ProductState == editItem.Key);
                /*if (brokeDecisionProduct?.DocWithdrawalSum > editItem.Value.MinQuantity)
                {
                    if (brokeDecisionProduct.DocWithdrawalSum < editItem.Value.MaxQuantity)
                        editItem.Value.MinQuantity = brokeDecisionProduct.DocWithdrawalSum;
                }*/
                if (editItem.Value.DocWithdrawalSum > editItem.Value.MinQuantity)
                {
                    if (editItem.Value.DocWithdrawalSum < editItem.Value.MaxQuantity)
                        editItem.Value.MinQuantity = editItem.Value.DocWithdrawalSum;
                }

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
                    }
                    else
                    {
                        brokeDecisionProduct = new BrokeDecisionProduct
                            (SelectedBrokeDecisionProduct.ProductId,
                                SelectedBrokeDecisionProduct.ProductKind,
                                SelectedBrokeDecisionProduct.Number,
                                SelectedBrokeDecisionProduct.ProductQuantity,
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
                }
            }
            var isExistMoreTwoCheckedItem = EditBrokeDecisionItems.Count(d => d.Value.IsChecked && !NeedsProductStates.Contains(d.Key)) >= 2;
            foreach (var editItem in EditBrokeDecisionItems)
            {
                editItem.Value.IsExistMoreTwoCheckedItem = editItem.Value.IsChecked ? false : isExistMoreTwoCheckedItem;
                if ((productState == ProductState.Good || productState == ProductState.InternalUsage || productState == ProductState.Limited)
                    && (editItem.Key == ProductState.ForConversion || editItem.Key == ProductState.Repack))
                {
                    editItem.Value.IsExistGoodItem = IsChecked;
                }
            }
            if (IsChecked)
            {
                if (productState == ProductState.ForConversion || productState == ProductState.Repack)
                {
                    if (EditBrokeDecisionItems[productState].MinQuantity > 0)
                    {
                        EditBrokeDecisionItems[ProductState.Good].Name = productState == ProductState.ForConversion ? "Уже переделано" : productState == ProductState.Repack ? "Уже переупаковано" : "Годная";
                        EditBrokeDecisionItems[ProductState.Good].Quantity = EditBrokeDecisionItems[productState].MinQuantity;
                        EditBrokeDecisionItems[ProductState.Good].MaxQuantity = EditBrokeDecisionItems[productState].MinQuantity;
                        EditBrokeDecisionItems[ProductState.Good].IsChecked = true;
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
        }

        public bool SaveBrokeDecisionProductsToModel(Guid? productID)
        {
            using (var gammaBase = DB.GammaDb)
            {
                foreach (var decisionProduct in BrokeDecisionProducts.Where(p => productID == null || p.ProductId == productID))
                {
                    var docBrokeDecisionProducts = gammaBase.DocBrokeDecisionProducts.Where(p => p.DocID == DocBrokeID && p.ProductID == decisionProduct.ProductId);
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
                            DocID = (Guid)DocBrokeID,
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
                return true;
            }
        }

        public override bool SaveToModel()
        {
            if (SelectedBrokeDecisionProduct != null && SelectedBrokeDecisionProduct.ProductId != Guid.Empty && IsEditableDecision == true)
                return SaveBrokeDecisionProductsToModel(SelectedBrokeDecisionProduct.ProductId);
            else
                return true;
        }
    }
}
