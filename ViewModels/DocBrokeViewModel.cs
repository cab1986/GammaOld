using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Common;
using Gamma.Models;
using System.Data.Entity;
using System.Text;
using DevExpress.Mvvm;
using Gamma.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
                    PlaceID = p.PlaceID,
                    PlaceName = p.Name
                }).ToList();
            StorePlaces = DiscoverPlaces;
            var doc = GammaBase.Docs.Include(d => d.DocBroke)
                .Include(d => d.DocBroke.DocBrokeProducts)
                .Include(d => d.DocBroke.DocBrokeProducts.Select(dp => dp.DocBrokeProductRejectionReasons))
                .FirstOrDefault(d => d.DocID == DocId);
            BrokeProducts = new ItemsChangeObservableCollection<BrokeProduct>();
            InternalUsageProduct = new EditBrokeDecisionItem("Хоз. нужды", ProductStates.InternalUsage, BrokeDecisionProducts);
            GoodProduct = new EditBrokeDecisionItem("Годная", ProductStates.Good, BrokeDecisionProducts);
            LimitedProduct = new EditBrokeDecisionItem("Ограниченная партия", ProductStates.Limited, BrokeDecisionProducts);
            BrokeProduct = new EditBrokeDecisionItem("На утилизацию", ProductStates.Broke, BrokeDecisionProducts);
            RepackProduct = new EditBrokeDecisionItem("На переделку", ProductStates.Repack, BrokeDecisionProducts, true);

            if (doc != null)
            {
                DocNumber = doc.Number;
                Date = doc.Date;
                PlaceDiscoverId = doc.DocBroke.PlaceDiscoverID;
                PlaceStoreId = doc.DocBroke.PlaceStoreID;
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
            AddProductCommand = new DelegateCommand(ChooseProductToAdd);
        }

        /// <summary>
        /// Добавление продукта к списку продукции акта
        /// </summary>
        /// <param name="productId">ID продукта</param>
        /// <param name="docId">ID документа акта о браке</param>
        /// <param name="brokeProducts">Список продукции</param>
        /// <param name="brokeDecisionProducts">Список решений по продукции</param>
        /// <param name="gammaBase">Контекст БД</param>
        private void AddProduct(Guid productId, Guid docId, ICollection<BrokeProduct> brokeProducts, ICollection<BrokeDecisionProduct> brokeDecisionProducts, GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            if (BrokeProducts.Select(bp => bp.ProductId).Contains(productId)) return;
            var product = gammaBase.vProductsInfo
                .FirstOrDefault(p => p.ProductID == productId);
            if (product == null) return;
#region AddBrokeProduct
            var docBrokeProductInfo =
                    gammaBase.DocBrokeProducts.Include(d => d.DocBrokeProductRejectionReasons)
                    .FirstOrDefault(d => d.DocID == docId);
            var brokeProduct = new BrokeProduct
            {
                Date = product.Date,
                NomenclatureName = product.ShortNomenclatureName,
                Number = product.Number,
                Place = product.Place,
                ShiftId = product.ShiftID,
                BaseMeasureUnit = product.BaseMeasureUnit,
                PrintName = product.PrintName,
                ProductId = product.ProductID, 
                Quantity = product.BaseMeasureUnitQuantity??0,
            };
            if (docBrokeProductInfo != null)
            {
                brokeProduct.Quantity = docBrokeProductInfo.Quantity ?? 0;
                brokeProduct.RejectionReasons = docBrokeProductInfo.DocBrokeProductRejectionReasons.ToList();
            }
            brokeProducts.Add(brokeProduct);
#endregion AddBrokeProduct
#region AddBrokeDecisionProduct
            var docBrokeDecisionProducts = gammaBase.DocBrokeDecisionProducts.Where(d => d.DocID == docId).ToList();
            if (docBrokeDecisionProducts.Count == 0)
            {
                brokeDecisionProducts.Add(new BrokeDecisionProduct
                {
                    Quantity = product.BaseMeasureUnitQuantity ?? 0,
                    MaxQuantity = product.BaseMeasureUnitQuantity ?? 1000000,
                    ProductId = product.ProductID,
                    Number = product.Number,
                    NomenclatureName = product.ShortNomenclatureName,
                    ProductState = ProductStates.NeedsDecision
                });
            }
            else
            {
                foreach (var decisionProduct in docBrokeDecisionProducts)
                {
                    brokeDecisionProducts.Add(new BrokeDecisionProduct
                    {
                        Quantity = decisionProduct.Quantity,
                        MaxQuantity = product.Quantity ?? 1000000,
                        ProductState = (ProductStates)decisionProduct.StateID,
                        ProductId = decisionProduct.ProductID,
                        Number = product.Number,
                        NomenclatureName = product.ShortNomenclatureName,
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
        public string DocNumber { get; set; }
        public DateTime Date { get; set; }
        public Guid? PlaceDiscoverId { get; set; }
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

        
        public DelegateCommand AddProductCommand { get; private set; }
        public DelegateCommand DeleteProductCommand { get; private set; }
        
        private Guid DocId { get; set; }

        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();

        public Guid? VMID { get; } = Guid.NewGuid();

        public bool IsReadOnly { get; private set; }

        private BrokeDecisionProduct _selectedBrokeDecisionProduct;

        public BrokeDecisionProduct SelectedBrokeDecisionProduct
        {
            get { return _selectedBrokeDecisionProduct; }
            set
            {
                if (_selectedBrokeDecisionProduct == value) return;
                _selectedBrokeDecisionProduct = value;
                InternalUsageProduct.BrokeDecisionProduct = null;
                InternalUsageProduct.IsChecked = false;
                GoodProduct.BrokeDecisionProduct = null;
                GoodProduct.IsChecked = false;
                LimitedProduct.BrokeDecisionProduct = null;
                LimitedProduct.IsChecked = false;
                BrokeProduct.BrokeDecisionProduct = null;
                BrokeProduct.IsChecked = false;
                RepackProduct.BrokeDecisionProduct = null;
                RepackProduct.IsChecked = false;
                if (value == null) return;
                var products = BrokeDecisionProducts.Where(p => p.ProductId == value.ProductId).ToList();
                foreach (var product in products)
                {
                    switch (product.ProductState)
                    {
                        case ProductStates.Broke:
                            BrokeProduct.IsChecked = true;
                            BrokeProduct.BrokeDecisionProduct = product;
                            break;
                        case ProductStates.Good:
                            GoodProduct.IsChecked = true;
                            GoodProduct.BrokeDecisionProduct = product;
                            break;
                        case ProductStates.InternalUsage:
                            InternalUsageProduct.IsChecked = true;
                            InternalUsageProduct.BrokeDecisionProduct = product;
                            break;
                        case ProductStates.Limited:
                            LimitedProduct.IsChecked = true;
                            LimitedProduct.BrokeDecisionProduct = product;
                            break;
                        case ProductStates.Repack:
                            RepackProduct.IsChecked = true;
                            RepackProduct.BrokeDecisionProduct = product;
                            break;
                    }
                }
                if (BrokeProduct.BrokeDecisionProduct == null)
                    BrokeProduct.BrokeDecisionProduct =
                        CreateNewBrokeDecisionProductWithState(SelectedBrokeDecisionProduct, ProductStates.Broke);
                if (GoodProduct.BrokeDecisionProduct == null)
                    GoodProduct.BrokeDecisionProduct =
                        CreateNewBrokeDecisionProductWithState(SelectedBrokeDecisionProduct, ProductStates.Good);
                if (InternalUsageProduct.BrokeDecisionProduct == null)
                    InternalUsageProduct.BrokeDecisionProduct =
                        CreateNewBrokeDecisionProductWithState(SelectedBrokeDecisionProduct, ProductStates.InternalUsage);
                if (LimitedProduct.BrokeDecisionProduct == null)
                    LimitedProduct.BrokeDecisionProduct =
                        CreateNewBrokeDecisionProductWithState(SelectedBrokeDecisionProduct, ProductStates.Limited);
                if (RepackProduct.BrokeDecisionProduct == null)
                    RepackProduct.BrokeDecisionProduct =
                        CreateNewBrokeDecisionProductWithState(SelectedBrokeDecisionProduct, ProductStates.Repack);
            }
        }

        /// <summary>
        /// Создает новое решение по продукту на базе другого с отличием состояния и количества
        /// </summary>
        /// <param name="product">исходное решение</param>
        /// <param name="productState">Новое состояние</param>
        /// <returns></returns>
        private BrokeDecisionProduct CreateNewBrokeDecisionProductWithState(BrokeDecisionProduct product,
            ProductStates productState)
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
            using (var gammaBase = DB.GammaDb)
            {
                var doc = gammaBase.Docs.Include(d => d.DocBroke).Include(d => d.DocBroke.DocBrokeProducts)
                    .Include(d => d.DocBroke.DocBrokeProducts.SelectMany(dp => dp.DocBrokeProductRejectionReasons))
                    .FirstOrDefault(d => d.DocID == DocId && d.DocTypeID == (int) DocTypes.DocBroke);
                if (doc == null)
                {
                    doc = new Docs
                    {
                        DocID = DocId,
                        DocTypeID = (int) DocTypes.DocBroke,
                        Date = Date,
                        Number = DocNumber,
                        PlaceID = WorkSession.PlaceID,
                        ShiftID = WorkSession.ShiftID,
                        UserID = WorkSession.UserID,
                        PrintName = WorkSession.PrintName,
                        DocBroke = new DocBroke
                        {
                            DocID = DocId,
                            PlaceDiscoverID = PlaceDiscoverId,
                            PlaceStoreID = PlaceStoreId,
                            DocBrokeProducts = new List<DocBrokeProducts>()
                        }
                    };
                    gammaBase.Docs.Add(doc);
                }
                foreach (var docBrokeProduct in doc.DocBroke.DocBrokeProducts)
                {
                    if (docBrokeProduct.DocBrokeProductRejectionReasons.Count > 0)
                    {
                        docBrokeProduct.DocBrokeProductRejectionReasons.Clear();
                    }
                }
                foreach (var docBrokeProduct in BrokeProducts)
                {
                    var brokeProduct = new DocBrokeProducts
                    {
                        ProductID = docBrokeProduct.ProductId,
                        DocID = doc.DocID,
                        Quantity = docBrokeProduct.Quantity,
                        DocBrokeProductRejectionReasons = docBrokeProduct.RejectionReasons
                    };
                    doc.DocBroke.DocBrokeProducts.Add(brokeProduct);
                }
                gammaBase.SaveChanges();
            }
        }
    }

    public class EditBrokeDecisionItem: DbEditItemWithNomenclatureViewModel
    {
        public EditBrokeDecisionItem(string name, ProductStates productState, ItemsChangeObservableCollection<BrokeDecisionProduct> decisionProducts, bool canChooseNomenclature = false)
        {
            Name = name;
            ProductState = productState;
            NomenclatureVisible = canChooseNomenclature;
            BrokeDecisionProducts = decisionProducts;
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
                    BrokeDecisionProducts.Remove(BrokeDecisionProducts.FirstOrDefault(
                        bp =>
                            bp.ProductId == BrokeDecisionProduct.ProductId &&
                            bp.ProductState == ProductStates.NeedsDecision));

                }
                else
                {
                    var needDecisionProduct = new BrokeDecisionProduct
                    {
                        ProductState = ProductStates.NeedsDecision,
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
                }
            }
        }
        public string Name { get; set; }
        private ProductStates ProductState { get; set; }

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

    public class BrokeDecisionProduct : ViewModelBase
    {
        public decimal MaxQuantity { get; set; }

        public Guid ProductId { get; set; }

        private ProductStates _productState;
        public ProductStates ProductState
        {
            get { return _productState; }
            set
            {
                _productState = value;
                Decision = _productState.GetAttributeOfType<DescriptionAttribute>().Description;
            }
        }
        public string NomenclatureName { get; set; }

        private decimal _quantity;

        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                RaisePropertyChanged("Quantity");
            }
            
        }
        public string Comment { get; set; }
        public string Number { get; set; }
        public string Decision { get; private set; }
        public Guid? NomenclatureId { get; set; }
        public Guid? CharacteristicId { get; set; }
    }

    public class BrokeProduct : ViewModelBase
    {
        public Guid ProductId { get; set; }
        public string NomenclatureName { get; set; }
        public string Number { get; set; }
        public string BaseMeasureUnit { get; set; }
        public decimal Quantity { get; set; }

        private string _rejectionReasonString;

        public string RejectionReasonsString
        {
            get { return _rejectionReasonString; }
            set
            {
                _rejectionReasonString = value;
                RaisePropertyChanged("RejectionReasonString");
            }
        }

        public DateTime? Date { get; set; }
        public string Place { get; set; }
        public int ShiftId { get; set; }
        public string PrintName { get; set; }
        private List<DocBrokeProductRejectionReasons> _rejectionReasons;

        public List<DocBrokeProductRejectionReasons> RejectionReasons
        {
            get { return _rejectionReasons; }
            set
            {
                _rejectionReasons = value;
                RejectionReasonsString = FormRejectionReasonsString(_rejectionReasons);
            }
        }

        private string FormRejectionReasonsString(List<DocBrokeProductRejectionReasons> list, GammaEntities gammaDb = null)
        {
            var sbuilder = new StringBuilder();
            using (var gammaBase = gammaDb ?? DB.GammaDb)
            {
                foreach (var reason in list)
                {
                    var description =
                        gammaBase.C1CRejectionReasons.FirstOrDefault(
                            r => r.C1CRejectionReasonID == reason.C1CRejectionReasonID);
                    if (description != null)
                    {
                        sbuilder.Append(description);
                        sbuilder.Append(Environment.NewLine);
                    }
                }
            }
            return sbuilder.ToString();
        }
    }
}
