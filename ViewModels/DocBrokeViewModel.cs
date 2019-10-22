// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Common;
using Gamma.Models;
using System.Data.Entity;
using DevExpress.Mvvm;
using Gamma.Interfaces;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using Gamma.Attributes;
using Gamma.Entities;

namespace Gamma.ViewModels
{
    public class DocBrokeViewModel: SaveImplementedViewModel, IBarImplemented, ICheckedAccess
    {
        public DocBrokeViewModel(Guid docBrokeId, Guid? productId = null, bool isInFuturePeriod = false)
        {
            Bars.Add(ReportManager.GetReportBar("DocBroke", VMID));
            Messenger.Default.Register<BarcodeMessage>(this, BarcodeReceived);
            DocId = docBrokeId;
            using (var gammaBase = DB.GammaDb)
            {
                DiscoverPlaces = gammaBase.Places.Where(p => ((p.IsProductionPlace ?? false) || (p.IsWarehouse ?? false)) && WorkSession.BranchIds.Contains(p.BranchID))
                .Select(p => new Place
                {
                    PlaceGuid = p.PlaceGuid,
                    PlaceID = p.PlaceID,
                    PlaceName = p.Name
                }).ToList();
                BrokePlaces = DiscoverPlaces;
                StorePlaces = gammaBase.Places.Where(p => p.IsWarehouse ?? false)
                    .Select(p => new Place
                    {
                        PlaceGuid = p.PlaceGuid,
                        PlaceID = p.PlaceID,
                        PlaceName = p.Name
                    }).ToList();
                var doc = gammaBase.Docs.Include(d => d.DocBroke)
                    .Include(d => d.DocBroke.DocBrokeProducts)
                    .Include(d => d.DocBroke.DocBrokeProducts.Select(dp => dp.DocBrokeProductRejectionReasons))
                    .FirstOrDefault(d => d.DocID == DocId);
                InFuturePeriodList = new Dictionary<bool, string>();
                InFuturePeriodList.Add(false, "25к - I передел");
                InFuturePeriodList.Add(true, "10к - II передел");
                BrokeProducts = new ItemsChangeObservableCollection<BrokeProduct>();
                InternalUsageProduct = new EditBrokeDecisionItem("Хоз. нужды", ProductState.InternalUsage, BrokeDecisionProducts);
                GoodProduct = new EditBrokeDecisionItem("Годная", ProductState.Good, BrokeDecisionProducts);
                LimitedProduct = new EditBrokeDecisionItem("Ограниченная партия", ProductState.Limited, BrokeDecisionProducts);
                BrokeProduct = new EditBrokeDecisionItem("На утилизацию", ProductState.Broke, BrokeDecisionProducts);
                RepackProduct = new EditBrokeDecisionItem("На переделку", ProductState.Repack, BrokeDecisionProducts, true);

                if (doc != null)
                {
                    DocNumber = doc.Number;
                    Date = doc.Date;
                    PlaceDiscoverId = doc.DocBroke.PlaceDiscoverID;
                    PlaceStoreId = doc.DocBroke.PlaceStoreID;
                    IsInFuturePeriod = doc.DocBroke.IsInFuturePeriod ?? false;
                    //SelectedInFuturePeriod = doc.DocBroke.IsInFuturePeriod ?? false ? FuturePeriodDocBrokeType.NextPlace : FuturePeriodDocBrokeType.FirstPlace;
                    IsConfirmed = doc.IsConfirmed;
                    UserID = doc.UserID;
                    ShiftID = doc.ShiftID;
                    foreach (var brokeProduct in doc.DocBroke.DocBrokeProducts)
                    {
                        AddProduct(brokeProduct.ProductID, DocId, BrokeProducts, BrokeDecisionProducts);
                    }
                }
                else
                {
                    Date = DB.CurrentDateTime;
                    IsInFuturePeriod = isInFuturePeriod;
                    //SelectedInFuturePeriod = isInFuturePeriod ? FuturePeriodDocBrokeType.NextPlace : FuturePeriodDocBrokeType.FirstPlace;
                    UserID = WorkSession.UserID;
                    ShiftID = WorkSession.ShiftID;
                    if (DiscoverPlaces.Select(dp => dp.PlaceID).Contains(WorkSession.PlaceID))
                    {
                        PlaceDiscoverId = DiscoverPlaces.First(dp => dp.PlaceID == WorkSession.PlaceID).PlaceGuid;
                    }
                    var number =
                        gammaBase.Docs.Where(d => d.DocTypeID == (int)DocTypes.DocBroke && d.Number != null)
                            .OrderByDescending(d => d.Date)
                            .FirstOrDefault();
                    try
                    {
                        DocNumber = (Convert.ToInt32(number) + 1).ToString();
                    }
                    catch
                    {
                        DocNumber = "1";
                    }
                }
                if (productId != null)
                {
                    AddProduct((Guid)productId, DocId, BrokeProducts, BrokeDecisionProducts);
                }

                RefreshRejectionReasonsList();

                AddProductCommand = new DelegateCommand(ChooseProductToAdd, () => !IsReadOnly);
                DeleteProductCommand = new DelegateCommand(DeleteBrokeProduct, () => !IsReadOnly);
                EditRejectionReasonsCommand = new DelegateCommand(EditRejectionReasons, () => !IsReadOnly);
                OpenProductCommand = new DelegateCommand(OpenProduct);
                var IsEditableCollection = new ObservableCollection<bool?>
                (
                    from pt in GammaBase.GetDocBrokeEditable(Date, UserID, (int?)ShiftID, (bool)(doc?.IsConfirmed ?? false), WorkSession.UserID, (int)WorkSession.ShiftID, doc?.DocID)
                    select pt
                );
                IsEditable = (IsEditableCollection.Count > 0 ) ? (bool)IsEditableCollection[0] : false;
                //IsReadOnly = (doc?.IsConfirmed ?? false) || !DB.HaveWriteAccess("DocBroke");
                IsReadOnly = (!IsEditable || !DB.HaveWriteAccess("DocBroke"));
            }
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
            BrokeDecisionProducts.CollectionChanged += DecisionProductsChanged;
            SetRejectionReasonForAllProductCommand = new DelegateCommand(SetRejectionReasonForAllProduct, () => !IsReadOnly && ForAllProductRejectionReasonID?.RejectionReasonID != null && ForAllProductRejectionReasonID?.RejectionReasonID != Guid.Empty && ForAllProductRejectionReasonComment != null && ForAllProductRejectionReasonComment != String.Empty);
        }

        private void BarcodeReceived(BarcodeMessage msg)
        {
            switch (SelectedTabIndex)
            {
                case 0:
                    if (BrokeProducts?.Count > 0)
                    {
                        var selectedBrokeProduct = BrokeProducts.Where(b => GammaBase.Products.Any(p => p.BarCode == msg.Barcode && p.ProductID == b.ProductId)).FirstOrDefault();
                        if (selectedBrokeProduct == null)
                        {
                            MessageBox.Show("Продукт со штрихкодом " + msg.Barcode + " в 'Несоответствующей продукции' не найден");
                            return;
                        }
                        SelectedBrokeProduct = selectedBrokeProduct;
                    }

                    break;
                case 1:
                    if (BrokeDecisionProducts?.Count > 0)
                    {
                        var selectedBrokeDecisionProduct = BrokeDecisionProducts.Where(b => GammaBase.Products.Any(p => p.BarCode == msg.Barcode && p.ProductID == b.ProductId)).FirstOrDefault();
                        if (selectedBrokeDecisionProduct == null)
                        {
                            MessageBox.Show("Продукт со штрихкодом " + msg.Barcode + " в 'Решениях' не найден");
                            return;
                        }
                        SelectedBrokeDecisionProduct = selectedBrokeDecisionProduct;
                    }
                    break;
            }
        }

        private void DecisionProductsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null && e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Contains(SelectedBrokeDecisionProduct))
            {
                SelectedBrokeDecisionProduct =
                    BrokeDecisionProducts.FirstOrDefault(p => p.ProductId == SelectedBrokeDecisionProduct.ProductId);
            }
        }

        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != VMID) return;
            if (!IsValid)
            {
                MessageBox.Show("Не заполнены некоторые обязательные поля!", "Поля не заполнены", MessageBoxButton.OK,
                    MessageBoxImage.Asterisk);
                return;
            }
            SaveToModel();
            ReportManager.PrintReport(msg.ReportID, DocId);
        }

        
        /// <summary>
        /// Добавление продукта к списку продукции акта
        /// </summary>
        /// <param name="productId">ID продукта</param>
        /// <param name="docId">ID документа акта о браке</param>
        /// <param name="brokeProducts">Список продукции</param>
        /// <param name="brokeDecisionProducts">Список решений по продукции</param>
        private void AddProduct(Guid productId, Guid docId, ICollection<BrokeProduct> brokeProducts, ICollection<BrokeDecisionProduct> brokeDecisionProducts)
        {
            using (var gammaBase = DB.GammaDb)
            {
                if (BrokeProducts.Select(bp => bp.ProductId).Contains(productId)) return;
                var product = gammaBase.vProductsInfo
                    .FirstOrDefault(p => p.ProductID == productId);
                if (product == null) return;
                if (!IsInFuturePeriod && BrokeProducts.Count > 0 &&
                    BrokeProducts.Select(bp => bp.ProductionPlaceId).First() != product.PlaceID)
                {
                    MessageBox.Show("Нельзя добавлять продукт другого передела в акт '25к - I передел'", "Ошибка добавления",
                        MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return;
                }
                #region AddBrokeProduct
                var docBrokeProductInfo =
                        gammaBase.DocBrokeProducts.Include(d => d.DocBrokeProductRejectionReasons)
                        .FirstOrDefault(d => d.DocID == docId && d.ProductID == productId);
                var brokeProduct = new BrokeProduct(docBrokeProductInfo == null ? new ItemsChangeObservableCollection<RejectionReason>() :
                    new ItemsChangeObservableCollection<RejectionReason>(docBrokeProductInfo.DocBrokeProductRejectionReasons
                    .Select(d => new RejectionReason()
                    {
                        RejectionReasonID = d.C1CRejectionReasonID,
                        Comment = d.Comment
                    })), docBrokeProductInfo?.BrokePlaceID, docBrokeProductInfo?.BrokeShiftID, docBrokeProductInfo?.BrokePrintName)
                {
                    Date = product.Date,
                    NomenclatureName = DB.GetProductNomenclatureNameBeforeDate(product.ProductID, Date),
                    Number = product.Number,
                    Place = product.Place,
                    ProductionPlaceId = (int)product.PlaceID,
                    ProductionPrintName = product.PrintName,
                    ShiftId = product.ShiftID,
                    BaseMeasureUnit = product.BaseMeasureUnit,
                    ProductId = product.ProductID,
                    ProductKind = (ProductKind)product.ProductKindID,
                    Quantity = docBrokeProductInfo == null ? product.BaseMeasureUnitQuantity ?? 0 : docBrokeProductInfo.Quantity ?? 0
                };
                if (brokeProduct.BrokePlaceId == brokeProduct.ProductionPlaceId && brokeProduct.PrintName == null)
                    brokeProduct.PrintName = brokeProduct.ProductionPrintName;
                brokeProducts.Add(brokeProduct);
                #endregion AddBrokeProduct
                #region AddBrokeDecisionProduct
                var docBrokeDecisionProducts = gammaBase.DocBrokeDecisionProducts.Where(d => d.DocID == docId && d.ProductID == productId).ToList();
                if (docBrokeDecisionProducts.Count == 0)
                {
                    brokeDecisionProducts.Add(new BrokeDecisionProduct(
                        product.ProductID,
                        (ProductKind)product.ProductKindID,
                        product.Number,
                        ProductState.NeedsDecision,
                        product.BaseMeasureUnitQuantity ?? 1000000,
                        DB.GetProductNomenclatureNameBeforeDate(product.ProductID, Date),
                        product.BaseMeasureUnit,
                        product.C1CNomenclatureID,
                        (Guid)product.C1CCharacteristicID,
                        product.BaseMeasureUnitQuantity ?? 0
                        )
                    );
                }
                else
                {
                    foreach (var decisionProduct in docBrokeDecisionProducts)
                    {
                        var docWithdrawalID = decisionProduct.DocBrokeDecisionProductWithdrawalProducts?.FirstOrDefault();
                        brokeDecisionProducts.Add(new BrokeDecisionProduct(
                                decisionProduct.ProductID,
                                (ProductKind)product.ProductKindID,
                                product.Number,
                                (ProductState)decisionProduct.StateID,
                                docBrokeProductInfo?.Quantity ?? 1000000,
                                DB.GetProductNomenclatureNameBeforeDate(product.ProductID, Date),
                                product.BaseMeasureUnit,
                                product.C1CNomenclatureID,
                                (Guid)product.C1CCharacteristicID,
                                decisionProduct.Quantity ?? 0,
                                decisionProduct.DecisionApplied,
                                (docWithdrawalID?.DocWithdrawalID == Guid.Empty ? null : docWithdrawalID?.DocWithdrawalID)
                            )
                        {
                            Comment = decisionProduct.Comment,
                            NomenclatureId = decisionProduct.C1CNomenclatureID,
                            CharacteristicId = decisionProduct.C1CCharacteristicID,
                            ProductKind = (ProductKind)product.ProductKindID
                        });
                    }
                }
                #endregion AddBrokeDecisionProduct
                RefreshRejectionReasonsList();
            }
        }

        private void ChooseProductToAdd()
        {
            Messenger.Default.Register<ChoosenProductMessage>(this, AddChoosenProduct);
            MessageManager.OpenFindProduct(ProductKind.ProductSpool-1, true, true, false);
        }

        private void AddChoosenProduct(ChoosenProductMessage msg)
        {
            if (msg.ProductIDs == null || msg.ProductIDs?.Count == 0)
                AddProduct(msg.ProductID, DocId, BrokeProducts, BrokeDecisionProducts);
            else
            {
                foreach (var product in msg.ProductIDs)
                {
                    AddProduct(product, DocId, BrokeProducts, BrokeDecisionProducts);
                }
            }
            Messenger.Default.Unregister<ChoosenProductMessage>(this);
        }

        public List<Place> DiscoverPlaces { get; set; }
        public List<Place> StorePlaces { get; set; }
        public List<Place> BrokePlaces { get; set; }
        public Dictionary<bool, string> InFuturePeriodList { get; set; }

        public bool IsConfirmed { get; set; }
        
        [UIAuth(UIAuthLevel.ReadOnly)]
        public string DocNumber { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public DateTime Date { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public Guid? UserID { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public byte? ShiftID { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        [Required(ErrorMessage = @"Место обнаружения не может быть пустым")]
        public Guid? PlaceDiscoverId { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        [Required(ErrorMessage = @"Место хранения не может быть пустым")]
        public Guid? PlaceStoreId { get; set; }

        private ItemsChangeObservableCollection<BrokeProduct> _brokeProducts = new ItemsChangeObservableCollection<BrokeProduct>();

        public ItemsChangeObservableCollection<BrokeProduct> BrokeProducts
        {
            get { return _brokeProducts; }
            set
            {
                _brokeProducts = value;
                RaisePropertyChanged("BrokeProducts");
            }
        }

        public ItemsChangeObservableCollection<BrokeDecisionProduct> BrokeDecisionProducts { get; set; } = new ItemsChangeObservableCollection<BrokeDecisionProduct>();

        public override bool CanSaveExecute()
        {
            //return IsValid && DB.HaveWriteAccess("DocBroke");
            return IsValid && DB.HaveWriteAccess("DocBroke") && IsEditable;
        }

        public int SelectedTabIndex { get; set; }

        public DelegateCommand SetRejectionReasonForAllProductCommand { get; private set; }

        public string ForAllProductRejectionReasonComment { get; set; }

        public RejectionReason ForAllProductRejectionReasonID { get; set; }
        private List<RejectionReason> _rejectionReasonsList { get; set; }
        public List<RejectionReason> RejectionReasonsList 
          {
            get { return _rejectionReasonsList; }
            set
            {
                _rejectionReasonsList = value;
                RaisePropertyChanged("RejectionReasonsList");
            }
        }

        private void RefreshRejectionReasonsList()
        {
            if (BrokeProducts?.Count > 0 && (RejectionReasonsList == null || RejectionReasonsList?.Count == 0))
            {
                if (BrokeProducts.Select(p => p.ProductKind).Distinct().Count() == 1 ||
                    (BrokeProducts.Select(p => p.ProductKind).Distinct().Count() == 2 && BrokeProducts.Any(p => p.ProductKind == ProductKind.ProductPallet) && BrokeProducts.Any(p => p.ProductKind == ProductKind.ProductPalletR)))
                {
                    var productKind = (int)BrokeProducts.Select(p => p.ProductKind).FirstOrDefault();
                    RejectionReasonsList = new List<RejectionReason>(GammaBase.C1CRejectionReasons
                        .Where(r => (!r.IsFolder ?? true) && (!r.IsMarked ?? true) && (r.ParentID == null || (r.ParentID != null
                        && GammaBase.ProductKinds.FirstOrDefault(pk => pk.ProductKindID == productKind).C1CRejectionReasons.Select(rr => rr.C1CRejectionReasonID).Contains((Guid)r.ParentID))))
                        .Select(r => new RejectionReason
                        {
                            RejectionReasonID = r.C1CRejectionReasonID,
                            Description = r.Description,
                            FullDescription = r.FullDescription
                        }).OrderBy(r => r.Description));
                }
                else
                {
                    RejectionReasonsList = new List<RejectionReason>();
                }
            }
            else
            {
                RejectionReasonsList = new List<RejectionReason>();
            }
        }

        private void SetRejectionReasonForAllProduct()
        {
            if (BrokeProducts != null)
            {
                foreach(var brokeProduct in BrokeProducts)
                {
                    brokeProduct.RejectionReasons.Clear();
                    brokeProduct.RejectionReasons.Add(new RejectionReason()
                    { 
                        RejectionReasonID = ForAllProductRejectionReasonID.RejectionReasonID,
                        Comment = ForAllProductRejectionReasonComment
                    });
                }
            }
        }

        public DelegateCommand AddProductCommand { get; private set; }
        public DelegateCommand DeleteProductCommand { get; private set; }

        private void DeleteBrokeProduct()
        {
            if (SelectedBrokeProduct == null) return;
            var decisionProductsToRemove =
                BrokeDecisionProducts.Where(p => p.ProductId == SelectedBrokeProduct.ProductId).ToList();
            foreach (var product in decisionProductsToRemove)
            {
                BrokeDecisionProducts.Remove(product);
            }
            BrokeProducts.Remove(SelectedBrokeProduct);
            RefreshRejectionReasonsList();
        }
        
        private bool _isInFuturePeriod { get; set; }
        public bool IsInFuturePeriod
        {
            get
            {
                return _isInFuturePeriod;
            }
            set
            {
                if (_isInFuturePeriod && !value && BrokeProducts.Count > 0 &&
                    BrokeProducts.Select(bp => bp.ProductionPlaceId).Distinct().Count() > 1)
                {
                    MessageBox.Show("Нельзя изменить акт на '25к - I передел', так как в акте продукты других переделов", "Ошибка изменения",
                        MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return;
                }
                _isInFuturePeriod = value;
                RaisePropertiesChanged("IsInFuturePeriod");
            }
        }

        private Guid DocId { get; }

        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();

        public Guid? VMID { get; } = Guid.NewGuid();

        public bool IsReadOnly { get; }

        public bool IsEditable { get; }

        private BrokeDecisionProduct _selectedBrokeDecisionProduct;

        public BrokeDecisionProduct SelectedBrokeDecisionProduct
        {
            get { return _selectedBrokeDecisionProduct; }
            set
            {
                if (Equals(_selectedBrokeDecisionProduct, value)) return;
                if (_selectedBrokeDecisionProduct != null && value != null && _selectedBrokeDecisionProduct.ProductId == value.ProductId)
                {
                    _selectedBrokeDecisionProduct = value;
                    RaisePropertyChanged("SelectedBrokeDecisionProduct");
                    return;
                }
                _selectedBrokeDecisionProduct = value;
                InternalUsageProduct.BrokeDecisionProduct = null;
                InternalUsageProduct.IsChecked = false;
                InternalUsageProduct.Quantity = 0;
                InternalUsageProduct.IsReadOnly = (value == null) || WorkSession.PlaceGroup != PlaceGroup.Other || IsReadOnly;
                InternalUsageProduct.DecisionApplied = false;
                InternalUsageProduct.DocWithdrawalID = null;
                GoodProduct.BrokeDecisionProduct = null;
                GoodProduct.IsChecked = false;
                GoodProduct.Quantity = 0;
                GoodProduct.IsReadOnly = (value == null || IsReadOnly);
                GoodProduct.DecisionApplied = false;
                GoodProduct.DocWithdrawalID = null;
                LimitedProduct.BrokeDecisionProduct = null;
                LimitedProduct.IsChecked = false;
                LimitedProduct.Quantity = 0;
                LimitedProduct.IsReadOnly = (value == null) || WorkSession.PlaceGroup != PlaceGroup.Other || IsReadOnly;
                LimitedProduct.DecisionApplied = false;
                LimitedProduct.DocWithdrawalID = null;
                BrokeProduct.BrokeDecisionProduct = null;
                BrokeProduct.IsChecked = false;
                BrokeProduct.Quantity = 0;
                BrokeProduct.IsReadOnly = value == null || IsReadOnly;
                BrokeProduct.DecisionApplied = false;
                BrokeProduct.DocWithdrawalID = null;
                RepackProduct.BrokeDecisionProduct = null;
                RepackProduct.IsChecked = false;
                RepackProduct.Quantity = 0;
                RepackProduct.IsReadOnly = (value == null) || WorkSession.PlaceGroup != PlaceGroup.Other || IsReadOnly;
                RepackProduct.DecisionApplied = false;
                RepackProduct.DocWithdrawalID = null;
                if (value == null) return;
                var products = BrokeDecisionProducts.Where(p => p.ProductId == value.ProductId).ToList();
                foreach (var product in products)
                {
                    switch (product.ProductState)
                    {
                        case ProductState.Broke:
                            BrokeProduct.Quantity = product.Quantity;
                            BrokeProduct.BrokeDecisionProduct = product;
                            BrokeProduct.IsChecked = true;
                            BrokeProduct.DecisionApplied = product.DecisionApplied;
                            BrokeProduct.DocWithdrawalID = product.DocWithdrawalID;
                            break;
                        case ProductState.Good:
                            GoodProduct.Quantity = product.Quantity;
                            GoodProduct.BrokeDecisionProduct = product;
                            GoodProduct.IsChecked = true;
                            GoodProduct.DecisionApplied = product.DecisionApplied;
                            GoodProduct.DocWithdrawalID = product.DocWithdrawalID;
                            break;
                        case ProductState.InternalUsage:
                            InternalUsageProduct.Quantity = product.Quantity;
                            InternalUsageProduct.BrokeDecisionProduct = product;
                            InternalUsageProduct.IsChecked = true;
                            InternalUsageProduct.DecisionApplied = product.DecisionApplied;
                            InternalUsageProduct.DocWithdrawalID = product.DocWithdrawalID;
                            break;
                        case ProductState.Limited:
                            LimitedProduct.Quantity = product.Quantity;
                            LimitedProduct.BrokeDecisionProduct = product;
                            LimitedProduct.IsChecked = true;
                            LimitedProduct.DecisionApplied = product.DecisionApplied;
                            LimitedProduct.DocWithdrawalID = product.DocWithdrawalID;
                            break;
                        case ProductState.Repack:
                            RepackProduct.Quantity = product.Quantity;
                            RepackProduct.BrokeDecisionProduct = product;
                            RepackProduct.IsChecked = true;
                            RepackProduct.DecisionApplied = product.DecisionApplied;
                            RepackProduct.DocWithdrawalID = product.DocWithdrawalID;
                            break;
                    }
                }
                if (BrokeProduct.BrokeDecisionProduct == null)
                    BrokeProduct.BrokeDecisionProduct =
                        CreateNewBrokeDecisionProductWithState(SelectedBrokeDecisionProduct, ProductState.Broke);
                if (GoodProduct.BrokeDecisionProduct == null)
                    GoodProduct.BrokeDecisionProduct =
                        CreateNewBrokeDecisionProductWithState(SelectedBrokeDecisionProduct, ProductState.Good);
                if (InternalUsageProduct.BrokeDecisionProduct == null)
                    InternalUsageProduct.BrokeDecisionProduct =
                        CreateNewBrokeDecisionProductWithState(SelectedBrokeDecisionProduct, ProductState.InternalUsage);
                if (LimitedProduct.BrokeDecisionProduct == null)
                    LimitedProduct.BrokeDecisionProduct =
                        CreateNewBrokeDecisionProductWithState(SelectedBrokeDecisionProduct, ProductState.Limited);
                if (RepackProduct.BrokeDecisionProduct == null)
                    RepackProduct.BrokeDecisionProduct =
                        CreateNewBrokeDecisionProductWithState(SelectedBrokeDecisionProduct, ProductState.Repack);
                RaisePropertyChanged("SelectedBrokeDecisionProduct");
            }
        }

        private void OpenProduct()
        {
            if (SelectedBrokeProduct == null) return;
            MessageManager.OpenDocProduct(SelectedBrokeProduct.ProductKind, SelectedBrokeProduct.ProductId);
        }

        public DelegateCommand OpenProductCommand { get; private set; }
        public DelegateCommand EditRejectionReasonsCommand { get; private set; }

        private void EditRejectionReasons()
        {
            if (SelectedBrokeProduct?.RejectionReasons == null) return;
            MessageManager.EditRejectionReasons(SelectedBrokeProduct);
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
                    product.MaxQuantity,
                    product.NomenclatureName,
                    product.MeasureUnit,
                    product.NomenclatureOldId,
                    product.CharacteristicOldId
                )
            {
                CharacteristicId = product.CharacteristicId,
                NomenclatureId = product.NomenclatureId,
            };
            return decisionProduct;
        }

        public BrokeProduct SelectedBrokeProduct { get; set; }

        public EditBrokeDecisionItem InternalUsageProduct { get; set; }
        public EditBrokeDecisionItem GoodProduct { get; set; }
        public EditBrokeDecisionItem LimitedProduct { get; set; } 
        public EditBrokeDecisionItem BrokeProduct { get; set; } 
        public EditBrokeDecisionItem RepackProduct { get; set; } 

        public override bool SaveToModel()
        {
            if (!DB.HaveWriteAccess("DocBroke")) return true;
            if (
                BrokeDecisionProducts.Any(
                    dp =>
                        dp.ProductState == ProductState.Repack &&
                        (dp.NomenclatureId == null || dp.CharacteristicId == null)))
            {
                MessageBox.Show("При решении \"на переделку\" необходимо указать номенклатуру и характеристику");
                return false;
            }
            if (
                BrokeProducts.Any(
                    dp =>
                        (dp.RejectionReasonsString == null || dp.RejectionReasonsString.Length == 0 
                        || dp.RejectionReasonCommentsString == null || dp.RejectionReasonCommentsString.Length == 0)))
            {
                MessageBox.Show("Обязательно требуется заполнить поле Дефекты и Причины несоответствия во всех продуктах");
                return false;
            }
            using (var gammaBase = DB.GammaDb)
            {
                var doc = gammaBase.Docs.Include(d => d.DocBroke).Include(d => d.DocBroke.DocBrokeProducts)
                    .Include(d => d.DocBroke.DocBrokeProducts.Select(dp => dp.DocBrokeProductRejectionReasons)).FirstOrDefault(d => d.DocID == DocId && d.DocTypeID == (int) DocTypes.DocBroke);
                if (doc == null)
                {
                    doc = new Docs
                    {
                        DocID = DocId,
                        DocTypeID = (int) DocTypes.DocBroke,
                        Date = Date,
                        PlaceID = WorkSession.PlaceID,
                        ShiftID = WorkSession.ShiftID,
                        UserID = WorkSession.UserID,
                        PrintName = WorkSession.PrintName
                    };
                    gammaBase.Docs.Add(doc);
                }
                doc.Number = DocNumber;
                doc.IsConfirmed = IsConfirmed;
                if (doc.DocBroke == null)
                    doc.DocBroke = new DocBroke
                    {
                        DocID = DocId,
                        PlaceDiscoverID = PlaceDiscoverId,
                        PlaceStoreID = PlaceStoreId,
                        DocBrokeProducts = new List<DocBrokeProducts>(),
                    };
                doc.DocBroke.PlaceDiscoverID = PlaceDiscoverId;
                doc.DocBroke.PlaceStoreID = PlaceStoreId;
                doc.DocBroke.IsInFuturePeriod = IsInFuturePeriod;
                foreach (var docBrokeProduct in doc.DocBroke.DocBrokeProducts)
                {
                    if (docBrokeProduct.DocBrokeProductRejectionReasons.Count > 0)
                    {
                        docBrokeProduct.DocBrokeProductRejectionReasons.Clear();
                    }
                }
                doc.DocBroke.DocBrokeProducts.Clear();
                foreach (var docBrokeProduct in BrokeProducts)
                {
                    var brokeProduct = new DocBrokeProducts
                    {
                        ProductID = docBrokeProduct.ProductId,
                        DocID = doc.DocID,
                        Quantity = docBrokeProduct.Quantity,
                        BrokePlaceID = docBrokeProduct.BrokePlaceId,
                        BrokeShiftID = docBrokeProduct.BrokeShiftId,
                        BrokePrintName = docBrokeProduct.PrintName,
                        DocBrokeProductRejectionReasons = new List<DocBrokeProductRejectionReasons>()
                    };
                    foreach (var reason in docBrokeProduct.RejectionReasons)
                    {
                        brokeProduct.DocBrokeProductRejectionReasons.Add(new DocBrokeProductRejectionReasons
                        {
                            ProductID = brokeProduct.ProductID,
                            DocID = brokeProduct.DocID,
                            C1CRejectionReasonID = reason.RejectionReasonID,
                            Comment = reason.Comment
                        });
                    }
                    doc.DocBroke.DocBrokeProducts.Add(brokeProduct);
                }
#region Сохранение решений по продукции
                if (doc.DocBroke.DocBrokeDecisionProducts == null) 
                    doc.DocBroke.DocBrokeDecisionProducts = new List<DocBrokeDecisionProducts>();
                else 
                    doc.DocBroke.DocBrokeDecisionProducts.Clear();
                foreach (var decisionProduct in BrokeDecisionProducts)
                {
                    doc.DocBroke.DocBrokeDecisionProducts.Add(new DocBrokeDecisionProducts
                    {
                        C1CCharacteristicID = decisionProduct.CharacteristicId,
                        C1CNomenclatureID = decisionProduct.NomenclatureId,
                        Quantity = decisionProduct.Quantity,
                        ProductID = decisionProduct.ProductId,
                        DocID = DocId,
                        StateID = (byte)decisionProduct.ProductState,
                        Comment = decisionProduct.Comment,
                        DecisionApplied = decisionProduct.DecisionApplied
                    });
                }
#endregion
                gammaBase.SaveChanges();
            }
            return true;
        }
    }
}
