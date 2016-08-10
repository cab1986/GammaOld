using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Common;
using Gamma.Models;
using System.Data.Entity;
using DevExpress.Mvvm;
using Gamma.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using Gamma.Attributes;

namespace Gamma.ViewModels
{
    public class DocBrokeViewModel: SaveImplementedViewModel, IBarImplemented, ICheckedAccess
    {
        public DocBrokeViewModel(Guid docBrokeId, Guid? productId = null, GammaEntities gammaBase = null) : base(gammaBase)
        {
            Bars.Add(ReportManager.GetReportBar("DocBroke", VMID));
            DocId = docBrokeId;
            DiscoverPlaces = GammaBase.Places.Where(p => (p.IsProductionPlace ?? false) || (p.IsWarehouse ?? false))
                .Select(p => new Place
                {
                    PlaceGuid = p.PlaceGuid,
                    PlaceID = p.PlaceID,
                    PlaceName = p.Name
                }).ToList();
            StorePlaces = DiscoverPlaces;
            var doc = GammaBase.Docs.Include(d => d.DocBroke)
                .Include(d => d.DocBroke.DocBrokeProducts)
                .Include(d => d.DocBroke.DocBrokeProducts.Select(dp => dp.DocBrokeProductRejectionReasons))
                .FirstOrDefault(d => d.DocID == DocId);
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
                IsConfirmed = doc.IsConfirmed;
                foreach (var brokeProduct in doc.DocBroke.DocBrokeProducts)
                {
                    AddProduct(brokeProduct.ProductID, DocId, BrokeProducts, BrokeDecisionProducts);
                }
            }
            else
            {
                Date = DB.CurrentDateTime;
            }
            if (productId != null)
            {
                AddProduct((Guid)productId, DocId, BrokeProducts, BrokeDecisionProducts);
            }
            AddProductCommand = new DelegateCommand(ChooseProductToAdd, () => !IsReadOnly);
            DeleteProductCommand = new DelegateCommand(DeleteBrokeProduct, () => !IsReadOnly);
            EditRejectionReasonsCommand = new DelegateCommand(EditRejectionReasons, () => !IsReadOnly);
            OpenProductCommand = new DelegateCommand(OpenProduct);
            IsReadOnly = (doc?.IsConfirmed ?? false) || !DB.HaveWriteAccess("DocBroke");
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
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
        /// <param name="gammaBase">Контекст БД</param>
        private void AddProduct(Guid productId, Guid docId, ICollection<BrokeProduct> brokeProducts, ICollection<BrokeDecisionProduct> brokeDecisionProducts,
            GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            if (BrokeProducts.Select(bp => bp.ProductId).Contains(productId)) return;
            var product = gammaBase.vProductsInfo
                .FirstOrDefault(p => p.ProductID == productId);
            if (product == null) return;
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
                })))
            {
                Date = product.Date,
                NomenclatureName = product.NomenclatureName,
                Number = product.Number,
                Place = product.Place,
                ShiftId = product.ShiftID,
                BaseMeasureUnit = product.BaseMeasureUnit,
                PrintName = product.PrintName,
                ProductId = product.ProductID, 
                ProductKind = (ProductKinds)product.ProductKindID,
                Quantity = docBrokeProductInfo == null ? product.BaseMeasureUnitQuantity??0 : docBrokeProductInfo.Quantity??0
            };
            brokeProducts.Add(brokeProduct);
#endregion AddBrokeProduct
#region AddBrokeDecisionProduct
            var docBrokeDecisionProducts = gammaBase.DocBrokeDecisionProducts.Where(d => d.DocID == docId && d.ProductID == productId).ToList();
            if (docBrokeDecisionProducts.Count == 0)
            {
                brokeDecisionProducts.Add(new BrokeDecisionProduct
                {
                    Quantity = product.BaseMeasureUnitQuantity ?? 0,
                    MaxQuantity = product.BaseMeasureUnitQuantity ?? 1000000,
                    ProductId = product.ProductID,
                    Number = product.Number,
                    NomenclatureName = product.NomenclatureName,
                    ProductState = ProductState.NeedsDecision,
                    MeasureUnit = product.BaseMeasureUnit
                });
            }
            else
            {
                foreach (var decisionProduct in docBrokeDecisionProducts)
                {
                    brokeDecisionProducts.Add(new BrokeDecisionProduct
                    {
                        Quantity = decisionProduct.Quantity,
                        MaxQuantity = product.BaseMeasureUnitQuantity ?? 1000000,
                        MeasureUnit = product.BaseMeasureUnit,
                        ProductState = (ProductState)decisionProduct.StateID,
                        ProductId = decisionProduct.ProductID,
                        Number = product.Number,
                        NomenclatureName = product.NomenclatureName,
                        Comment = decisionProduct.Comment,
                        NomenclatureId = decisionProduct.C1CNomenclatureID,
                        CharacteristicId = decisionProduct.C1CCharacteristicID
                    });
                }
            }
#endregion AddBrokeDecisionProduct
        }

        private void ChooseProductToAdd()
        {
            Messenger.Default.Register<ChoosenProductMessage>(this, AddChoosenProduct);
            MessageManager.OpenFindProduct(ProductKinds.ProductSpool, true, true);
        }

        private void AddChoosenProduct(ChoosenProductMessage msg)
        {
            AddProduct(msg.ProductID, DocId, BrokeProducts, BrokeDecisionProducts);
            Messenger.Default.Unregister<ChoosenProductMessage>(this);
        }

        public List<Place> DiscoverPlaces { get; set; }
        public List<Place> StorePlaces { get; set; }

        public bool IsConfirmed { get; set; }
        
        [UIAuth(UIAuthLevel.ReadOnly)]
        public string DocNumber { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public DateTime Date { get; set; }

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
        private ItemsChangeObservableCollection<BrokeDecisionProduct> _brokeDecisionProducts = new ItemsChangeObservableCollection<BrokeDecisionProduct>();

        public ItemsChangeObservableCollection<BrokeDecisionProduct> BrokeDecisionProducts
        {
            get { return _brokeDecisionProducts; }
            set
            {
                _brokeDecisionProducts = value;
                RaisePropertyChanged("BrokeDecisionProducts");
            }
        }

        public override bool CanSaveExecute()
        {
            return IsValid && !IsReadOnly;
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
        }
        
        public bool IsInFuturePeriod { get; set; }

        private Guid DocId { get; }

        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();

        public Guid? VMID { get; } = Guid.NewGuid();

        public bool IsReadOnly { get; }

        private BrokeDecisionProduct _selectedBrokeDecisionProduct;

        public BrokeDecisionProduct SelectedBrokeDecisionProduct
        {
            get { return _selectedBrokeDecisionProduct; }
            set
            {
                if (Equals(_selectedBrokeDecisionProduct, value)) return;
                _selectedBrokeDecisionProduct = value;
                InternalUsageProduct.BrokeDecisionProduct = null;
                InternalUsageProduct.IsChecked = false;
                InternalUsageProduct.Quantity = 0;
                InternalUsageProduct.IsReadOnly = (value == null) || WorkSession.PlaceGroup != PlaceGroups.Other || IsReadOnly;
                GoodProduct.BrokeDecisionProduct = null;
                GoodProduct.IsChecked = false;
                GoodProduct.Quantity = 0;
                GoodProduct.IsReadOnly = (value == null || IsReadOnly);
                LimitedProduct.BrokeDecisionProduct = null;
                LimitedProduct.IsChecked = false;
                LimitedProduct.Quantity = 0;
                LimitedProduct.IsReadOnly = (value == null) || WorkSession.PlaceGroup != PlaceGroups.Other || IsReadOnly;
                BrokeProduct.BrokeDecisionProduct = null;
                BrokeProduct.IsChecked = false;
                BrokeProduct.Quantity = 0;
                BrokeProduct.IsReadOnly = value == null || IsReadOnly;
                RepackProduct.BrokeDecisionProduct = null;
                RepackProduct.IsChecked = false;
                RepackProduct.Quantity = 0;
                RepackProduct.IsReadOnly = (value == null) || WorkSession.PlaceGroup != PlaceGroups.Other || IsReadOnly;
                if (value == null) return;
                var products = BrokeDecisionProducts.Where(p => p.ProductId == value.ProductId).ToList();
                foreach (var product in products)
                {
                    switch (product.ProductState)
                    {
                        case ProductState.Broke:
                            BrokeProduct.IsChecked = true;
                            BrokeProduct.Quantity = product.Quantity;
                            BrokeProduct.BrokeDecisionProduct = product;
                            break;
                        case ProductState.Good:
                            GoodProduct.IsChecked = true;
                            GoodProduct.Quantity = product.Quantity;
                            GoodProduct.BrokeDecisionProduct = product;
                            break;
                        case ProductState.InternalUsage:
                            InternalUsageProduct.IsChecked = true;
                            InternalUsageProduct.Quantity = product.Quantity;
                            InternalUsageProduct.BrokeDecisionProduct = product;
                            break;
                        case ProductState.Limited:
                            LimitedProduct.IsChecked = true;
                            LimitedProduct.Quantity = product.Quantity;
                            LimitedProduct.BrokeDecisionProduct = product;
                            break;
                        case ProductState.Repack:
                            RepackProduct.IsChecked = true;
                            RepackProduct.Quantity = product.Quantity;
                            RepackProduct.BrokeDecisionProduct = product;
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
            MessageManager.EditRejectionReasons(SelectedBrokeProduct.RejectionReasons);
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
            var decisionProduct = new BrokeDecisionProduct
            {
                CharacteristicId = product.CharacteristicId,
                NomenclatureId = product.NomenclatureId,
                Quantity = 0,
                MaxQuantity = product.MaxQuantity,
                ProductId = product.ProductId,
                Number = product.Number,
                NomenclatureName = product.NomenclatureName,
                ProductState = productState
            };
            return decisionProduct;
        }

        public BrokeProduct SelectedBrokeProduct { get; set; }

        public EditBrokeDecisionItem InternalUsageProduct { get; set; }
        public EditBrokeDecisionItem GoodProduct { get; set; }
        public EditBrokeDecisionItem LimitedProduct { get; set; } 
        public EditBrokeDecisionItem BrokeProduct { get; set; } 
        public EditBrokeDecisionItem RepackProduct { get; set; } 

        public override void SaveToModel(GammaEntities gamma = null)
        {
            if (!DB.HaveWriteAccess("DocBroke")) return;
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
                        Comment = decisionProduct.Comment
                    });
                }
#endregion
                gammaBase.SaveChanges();
            }
        }
    }
}
