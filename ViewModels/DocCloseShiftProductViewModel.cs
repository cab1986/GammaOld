// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Gamma.Interfaces;
using Gamma.Attributes;
using System.Data.Entity;
using Gamma.Entities;
using System.Windows;
using Gamma.Models;
using DevExpress.Mvvm;
using Gamma.Common;
using System.Collections;
using System.Windows.Data;
using System.Data.Entity.SqlServer;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocCloseShiftProductViewModel : /*DbEditItemWithNomenclatureViewModel*/SaveImplementedViewModel, ICheckedAccess
    {

        /// <summary>
        /// Initializes a new instance of the DocCloseShiftProductViewModel class.
        /// </summary>
        public DocCloseShiftProductViewModel()
        {
            AddBeginProductCommand = new DelegateCommand<RemainderType>(AddProduct, !IsReadOnly);
            DeleteBeginProductCommand = new DelegateCommand<RemainderType>(DeleteProduct, !IsReadOnly);
            AddEndProductCommand = new DelegateCommand<RemainderType>(AddProduct, !IsReadOnly);
            DeleteEndProductCommand = new DelegateCommand<RemainderType>(DeleteProduct, !IsReadOnly);
            AddUtilizationProductCommand = new DelegateCommand(AddUtilizationProduct, () => !IsReadOnly);
            DeleteUtilizationProductCommand = new DelegateCommand(DeleteUtilizationProduct, () => !IsReadOnly);

            ShowProductCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(SelectedProduct.ProductKind, SelectedProduct.ProductID),
                () => SelectedProduct != null);
            ShowBeginProductCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(SelectedBeginProduct.ProductKind, SelectedBeginProduct.ProductID),
                () => SelectedBeginProduct != null);
            ShowEndProductCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(SelectedEndProduct.ProductKind, SelectedEndProduct.ProductID),
                () => SelectedEndProduct != null);
            ShowUtilizationProductCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(SelectedUtilizationProduct.ProductKind, SelectedUtilizationProduct.ProductID),
                () => SelectedUtilizationProduct != null);
        }

        public DocCloseShiftProductViewModel(int placeID, int shiftID, DateTime closeDate, GammaEntities gammaDb = null) : this()
        {
            GammaBase = gammaDb ?? DB.GammaDb;
            PlaceID = placeID;
            ShiftID = shiftID;
            CloseDate = closeDate;
            PlaceGroupID = WorkSession.Places.First(p => p.PlaceID == PlaceID).PlaceGroupID;
        }

        public DocCloseShiftProductViewModel(Guid docId, bool isConfirmed, GammaEntities gammaDb = null):this()
        {
            GammaBase = gammaDb ?? DB.GammaDb;

            IsConfirmed = isConfirmed;

            using (var gammaBase = GammaBase)
            {
                Products = new ObservableCollection<Product>(gammaBase.GetDocCloseShiftProductionProducts(docId).Select(sp => new Product()
                {
                    CharacteristicID = (Guid)sp.CharacteristicID,
                    NomenclatureID = sp.NomenclatureID,
                    NomenclatureName = sp.Nomenclature,
                    Number = sp.Number,
                    ProductID = sp.ProductID,
                    Quantity = sp.Quantity ?? 0,
                    Date = sp.Date,
                    ProductKind = (ProductKind)sp.ProductKindID
                }));
                DocId = docId;
                var doc = gammaBase.Docs.Include(d => d.DocCloseShiftDocs).First(d => d.DocID == docId);
                DocCloseDocIds = doc.DocCloseShiftDocs.Select(dc => dc.DocID).ToList();
                PlaceID = (int)doc.PlaceID;
                ShiftID = doc.ShiftID ?? 0;
                CloseDate = doc.Date;
                PlaceGroupID = doc.Places.PlaceGroupID;
                //IsConfirmed = doc.IsConfirmed;

                BeginProducts = new ItemsChangeObservableCollection<DocCloseShiftRemainder>(gammaBase.DocCloseShiftRemainders
                    //.Include(dr => dr.DocCloseShifts)
                    .Where(d => d.DocID == docId && !(d.IsMaterial ?? false) && (d.RemainderTypes.RemainderTypeID == 1 || d.RemainderTypes.RemainderTypeID == 3))
                    .Select(d => new DocCloseShiftRemainder()
                    {
                        ProductID = (Guid)d.ProductID,
                        StateID = d.StateID,
                        Quantity = d.Quantity,
                        RemainderTypeID = d.RemainderTypeID,
                        ProductKind = d.Products == null ? new ProductKind() : (ProductKind)d.Products.ProductKindID
                    }));
                EndProducts = new ItemsChangeObservableCollection<DocCloseShiftRemainder>(gammaBase.DocCloseShiftRemainders
                    //.Include(dr => dr.DocCloseShifts)
                    .Where(d => d.DocID == docId && !(d.IsMaterial ?? false) && d.RemainderTypes.RemainderTypeID == 2)
                    .Select(d => new DocCloseShiftRemainder()
                    {
                        ProductID = (Guid)d.ProductID,
                        StateID = d.StateID,
                        Quantity = d.Quantity,
                        RemainderTypeID = d.RemainderTypeID,
                        ProductKind = d.Products == null ? new ProductKind() : (ProductKind)d.Products.ProductKindID
                    }));
                //Получение списка утилизированной продукции
                UtilizationProducts = new ObservableCollection<Product>(gammaBase.DocCloseShiftUtilizationProducts
                    .Where(d => d.DocID == docId && !(d.IsMaterial ?? true))
                    .Join(gammaBase.vProductsInfo, d => d.ProductID, p => p.ProductID
                    , (d, p) => new Product()
                    {
                        CharacteristicID = (Guid)p.C1CCharacteristicID,
                        NomenclatureID = p.C1CNomenclatureID,
                        NomenclatureName = p.NomenclatureName,
                        Number = p.Number,
                        ProductID = d.ProductID,
                        Quantity = d.Quantity ?? 0,
                        ProductKind = (ProductKind)p.ProductKindID
                    }));
                
                InProducts = new ObservableCollection<MovementProduct>(gammaBase.DocCloseShiftMovementProducts
                    .Where(d => d.DocID == docId && (bool)d.IsMovementIn && !(bool)d.IsMaterial)
                    .Join(gammaBase.vProductsInfo, d => d.ProductID, p => p.ProductID
                    , (d, p) => new MovementProduct
                    {
                        NomenclatureName = p.NomenclatureName,
                        Number = d.Products.Number,
                        ProductId = d.ProductID,
                        Quantity = d.Quantity ?? 0,
                        ProductKindName = d.Products.ProductKinds.Name,
                        //OrderTypeName = p.OrderTypeName,
                        DocMovementId = d.DocMovementID,
                        OutPlaceName = d.MovementPlaceName,
                        OutPlaceZoneName = d.MovementPlaceZoneName,
                        OutDate = d.DateMovement,
                        ProductKind = (ProductKind)p.ProductKindID
                    }));

                OutProducts = new ObservableCollection<MovementProduct>(gammaBase.DocCloseShiftMovementProducts
                    .Where(d => d.DocID == docId && !((bool)d.IsMovementIn) && !(bool)d.IsMaterial)
                    .Join(gammaBase.vProductsInfo, d => d.ProductID, p => p.ProductID
                    , (d, p) => new MovementProduct
                    {
                        NomenclatureName = p.NomenclatureName,
                        Number = d.Products.Number,
                        ProductId = d.ProductID,
                        Quantity = d.Quantity ?? 0,
                        ProductKindName = d.Products.ProductKinds.Name,
                        //OrderTypeName = p.OrderTypeName,
                        DocMovementId = d.DocMovementID,
                        InPlaceName = d.MovementPlaceName,
                        InPlaceZoneName = d.MovementPlaceZoneName,
                        InDate = d.DateMovement,
                        ProductKind = (ProductKind)p.ProductKindID
                    }));
                RepackProducts = new ObservableCollection<Sample>(gammaBase.DocCloseShiftRepackProducts.Where(ds => ds.DocID == DocId)
                    .Select(ds => new Sample
                    {
                        NomenclatureID = ds.C1CNomenclatureID,
                        CharacteristicID = ds.C1CCharacteristicID,
                        Quantity = ds.Quantity,
                        NomenclatureName = ds.C1CNomenclature.Name + " " + ds.C1CCharacteristics.Name,
                        ProductionTaskID = ds.ProductionTaskID
                    }));
                /*
                var repackProducts = new ObservableCollection<Sample>(gammaBase.DocCloseShiftRepackProducts.Where(ds => ds.DocID == DocId)
                    .Select(ds => new Sample
                    {
                        NomenclatureID = ds.C1CNomenclatureID,
                        CharacteristicID = ds.C1CCharacteristicID,
                        Quantity = ds.Quantity,
                        NomenclatureName = ds.C1CNomenclature.Name + " " + ds.C1CCharacteristics.Name,
                        MeasureUnitId = ds.C1CMeasureUnitID
                    }));
                foreach (var product in repackProducts)
                {
                    product.MeasureUnits = GetSampleMeasureUnits(product.NomenclatureID, product.CharacteristicID);
                    if (product.MeasureUnitId == null) product.MeasureUnitId = product.MeasureUnits.FirstOrDefault().Key;
                }
                RepackProducts = repackProducts;
                */

            }
        }

        public bool IsChanged { get; private set; }

        private bool IsConfirmed { get; set; }
        private List<Guid> DocCloseDocIds { get; set; }
        public bool IsReadOnly => !DB.HaveWriteAccess("DocCloseShiftDocs") || IsConfirmed;
        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();
        public Guid? VMID { get; } = Guid.NewGuid();
        public Product SelectedProduct { get; set; }
        public DocCloseShiftRemainder SelectedBeginProduct { get; set; }
        public DocCloseShiftRemainder SelectedEndProduct { get; set; }
        private RemainderType CurrentAddProductRemainder { get; set; }
        public Product SelectedUtilizationProduct { get; set; }

        private Guid DocId { get; set; }
        private int PlaceID { get; set; }
        private int ShiftID { get; set; }
        private DateTime CloseDate { get; set; }
        private short PlaceGroupID { get; set; }

        public string HeaderQuantityField => @"Кол-во, кг/рул/пач";

        public DelegateCommand ShowProductCommand { get; private set; }
        public DelegateCommand ShowBeginProductCommand { get; private set; }
        public DelegateCommand ShowEndProductCommand { get; private set; }
        public DelegateCommand ShowUtilizationProductCommand { get; private set; }
        public DelegateCommand<RemainderType> AddBeginProductCommand { get; private set; }
        public DelegateCommand<RemainderType> DeleteBeginProductCommand { get; private set; }
        public DelegateCommand<RemainderType> AddEndProductCommand { get; private set; }
        public DelegateCommand<RemainderType> DeleteEndProductCommand { get; private set; }
        public DelegateCommand AddUtilizationProductCommand { get; private set; }
        public DelegateCommand DeleteUtilizationProductCommand { get; private set; }

        private int _selectedTabIndex = 2;//tab = Изготовлено
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                _selectedTabIndex = value;
                RaisePropertiesChanged("SelectedTabIndex");
            }
        }

        private ObservableCollection<Product> _products = new ObservableCollection<Product>();
        public ObservableCollection<Product> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                RaisePropertiesChanged("Products");
            }
        }

        private ObservableCollection<Product> _utilizationProducts = new ObservableCollection<Product>();
        public ObservableCollection<Product> UtilizationProducts
        {
            get { return _utilizationProducts; }
            set
            {
                _utilizationProducts = value;
                RaisePropertiesChanged("UtilizationProducts");
            }
        }

        private ObservableCollection<MovementProduct> _inProducts = new ObservableCollection<MovementProduct>();
        public ObservableCollection<MovementProduct> InProducts
        {
            get { return _inProducts; }
            set
            {
                _inProducts = value;
                RaisePropertiesChanged("InProducts");
            }
        }

        private ObservableCollection<MovementProduct> _outProducts = new ObservableCollection<MovementProduct>();
        public ObservableCollection<MovementProduct> OutProducts
        {
            get { return _outProducts; }
            set
            {
                _outProducts = value;
                RaisePropertiesChanged("OutProducts");
            }
        }

        private ObservableCollection<DocCloseShiftRemainder> _beginProducts = new ObservableCollection<DocCloseShiftRemainder>();
        public ObservableCollection<DocCloseShiftRemainder> BeginProducts
        {
            get { return _beginProducts; }
            set
            {
                _beginProducts = value;
                RaisePropertiesChanged("BeginProducts");
            }
        }

        private ObservableCollection<DocCloseShiftRemainder> _endProducts = new ObservableCollection<DocCloseShiftRemainder>();
        public ObservableCollection<DocCloseShiftRemainder> EndProducts
        {
            get { return _endProducts; }
            set
            {
                _endProducts = value;
                RaisePropertiesChanged("EndProducts");
            }
        }

        private ObservableCollection<Sample> _repackProducts = new ObservableCollection<Sample>();
        public ObservableCollection<Sample> RepackProducts
        {
            get { return _repackProducts; }
            set
            {
                _repackProducts = value;
                RaisePropertiesChanged("RepackProducts");
            }
        }

        public void FillGrid(bool IsFillEnd = true)
        {
            UIServices.SetBusyState();
            Clear(IsFillEnd);
            using (var gammaBase = DB.GammaDb)
            {
                DocCloseDocIds = gammaBase.Docs.
                Where(d => d.PlaceID == PlaceID && d.ShiftID == ShiftID && d.IsConfirmed &&
                    d.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    d.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    (d.DocTypeID == (int)DocTypes.DocProduction || d.DocTypeID == (int)DocTypes.DocWithdrawal)).Select(d => d.DocID).ToList();
                Products = new ObservableCollection<Product>(gammaBase.FillDocCloseShiftProductionProducts(PlaceID, ShiftID, CloseDate).Select(p => new Product()
                {
                    CharacteristicID = (Guid)p.CharacteristicID,
                    NomenclatureID = p.NomenclatureID,
                    NomenclatureName = p.NomenclatureName,
                    Number = p.Number,
                    ProductID = p.ProductID,
                    Quantity = p.Quantity ?? 0,
                    Date = p.Date,
                    ProductKind = (ProductKind)p.ProductKindID
                }));
                                
                //if (BeginProducts == null || BeginProducts?.Count() == 0)
                {
                    var PreviousDocCloseShift = gammaBase.Docs
                        .Where(d => d.DocTypeID == 3 && d.PlaceID == PlaceID && d.Date < SqlFunctions.DateAdd("mi", -1, CloseDate))
                        .OrderByDescending(d => d.Date)
                        .FirstOrDefault();
                    BeginProducts = new ObservableCollection<DocCloseShiftRemainder>(gammaBase.DocCloseShiftRemainders
                        .Where(d => (d.RemainderTypeID == 2 || d.RemainderTypeID == 0 || d.RemainderTypeID == null) && d.DocCloseShifts.DocID == PreviousDocCloseShift.DocID && !(d.IsMaterial ?? false))
                        .Select(d => new DocCloseShiftRemainder
                        {
                            ProductID = (Guid)d.ProductID,
                            StateID = d.StateID,
                            Quantity = d.Quantity,
                            RemainderTypeID = d.RemainderTypeID == 0 || d.RemainderTypeID == null ? 3 : 1,
                            ProductKind = d.Products == null ? new ProductKind() :(ProductKind)d.Products.ProductKindID
                        }));
                }

                FillEndProducts(IsFillEnd);

                InProducts = new ObservableCollection<MovementProduct>(gammaBase.FillDocCloseShiftMovementProducts(PlaceID, ShiftID, CloseDate)
                            .Where(d => (bool)d.IsMovementIn && (bool)d.IsProductionProduct).Select(d => new MovementProduct
                            {
                                NomenclatureName = d.NomenclatureName,
                                Number = d.Number,
                                ProductId = d.ProductID,
                                Quantity = d.Quantity ?? 0,
                                ProductKindName = d.ProductKindName,
                                OrderTypeName = d.OrderTypeName,
                                DocMovementId = d.DocMovementID,
                                OutPlaceName = d.OutPlace,
                                OutPlaceZoneName = d.OutPlaceZone,
                                OutDate = d.OutDate,
                                ProductKind = (ProductKind)d.ProductKindID
                            }));

                OutProducts = new ObservableCollection<MovementProduct>(gammaBase.FillDocCloseShiftMovementProducts(PlaceID, ShiftID, CloseDate)
                            .Where(d => (bool)d.IsMovementOut && (bool)d.IsProductionProduct).Select(d => new MovementProduct
                            {
                                NomenclatureName = d.NomenclatureName,
                                Number = d.Number,
                                ProductId = d.ProductID,
                                Quantity = d.Quantity ?? 0,
                                ProductKindName = d.ProductKindName,
                                OrderTypeName = d.OrderTypeName,
                                DocMovementId = d.DocMovementID,
                                InPlaceName = d.InPlace,
                                InPlaceZoneName = d.InPlaceZone,
                                InDate = d.InDate,
                                ProductKind = (ProductKind)d.ProductKindID
                            }));
                RepackProducts = new ObservableCollection<Sample>(gammaBase.FillDocCloseShiftRepackProducts(PlaceID, ShiftID, CloseDate)
                    .Select(ds => new Sample
                    {
                        NomenclatureID = (Guid)ds.NomenclatureID,
                        CharacteristicID = ds.CharacteristicID,
                        Quantity = ds.Quantity ?? 0,
                        NomenclatureName = ds.NomenclatureName,
                        ProductionTaskID = ds.ProductionTaskID
                    }));
/*
                var repackProducts = new ObservableCollection<Sample>(gammaBase.FillDocCloseShiftRepackProducts(PlaceID, ShiftID, CloseDate)
                    .Select(ds => new Sample
                    {
                        NomenclatureID = (Guid)ds.NomenclatureID,
                        CharacteristicID = ds.CharacteristicID,
                        Quantity = ds.Quantity ?? 0,
                        NomenclatureName = ds.NomenclatureName,
                        MeasureUnitId = ds.BaseMeasureUnitID
                    }));
                foreach (var product in repackProducts)
                {
                    product.MeasureUnits = GetSampleMeasureUnits(product.NomenclatureID, product.CharacteristicID);
                    if (product.MeasureUnitId == null) product.MeasureUnitId = product.MeasureUnits.FirstOrDefault().Key;
                }
                RepackProducts = repackProducts;
                */
                var utilizationProducts = new ObservableCollection<Product>(gammaBase.FillDocCloseShiftUtilizationProducts(PlaceID, ShiftID, CloseDate)
                    .Where(p => !(p.IsMaterial ?? true)).Select(p => new Product()
                {
                    CharacteristicID = p.CharacteristicID,
                    NomenclatureID = p.NomenclatureID,
                    NomenclatureName = p.NomenclatureName,
                    Number = p.Number,
                    ProductID = p.ProductID,
                    Quantity = p.Quantity ?? 0,
                    ProductKind = (ProductKind)p.ProductKindID
                }));

                foreach (Product product in utilizationProducts)
                {
                    if (UtilizationProducts.Count(d => d.ProductID == product.ProductID) == 0)
                    {
                        UtilizationProducts.Add(new Product()
                        {
                            NomenclatureID = product.NomenclatureID,
                            CharacteristicID = product.CharacteristicID,
                            NomenclatureName = product.NomenclatureName,
                            Number = product.Number,
                            ProductID = product.ProductID,
                            Quantity = product.Quantity,
                            ProductKind = product.ProductKind
                        });
                    }
                    else
                    {
                        var item = UtilizationProducts.FirstOrDefault(d => d.ProductID == product.ProductID);
                        if (item != null)
                        {
                            item.Quantity = product.Quantity;
                            item.NomenclatureID = product.NomenclatureID;
                            item.CharacteristicID = product.CharacteristicID;
                            item.NomenclatureName = product.NomenclatureName;
                        }
                    }
                }

                IsChanged = true;
            }
        }

        private void FillEndProducts(bool IsFillEnd = true)
        {
            if (IsFillEnd)
                using (var gammaBase = DB.GammaDb)
                {
                    EndProducts?.Clear();

                    EndProducts = new ItemsChangeObservableCollection<DocCloseShiftRemainder>(
                        gammaBase.vProductsInfo
                            .Where(p => p.CurrentPlaceID == PlaceID && 
                                        ((PlaceGroupID == (short)PlaceGroup.PM && (p.ProductKindID == (byte)ProductKind.ProductSpool || p.ProductKindID == (byte)ProductKind.ProductGroupPack))
                                            || (PlaceGroupID == (short)PlaceGroup.Convertings && (p.ProductKindID == (byte)ProductKind.ProductPallet || p.ProductKindID == (byte)ProductKind.ProductPalletR))))
                        .Select(p => new DocCloseShiftRemainder
                        {
                            ProductID = (Guid)p.ProductID,
                            StateID = p.StateID,
                            Quantity = (p.Quantity ?? 0) * (p.ProductKindID == (byte)ProductKind.ProductSpool || p.ProductKindID == (byte)ProductKind.ProductGroupPack ? 1000 : 1),
                            RemainderTypeID = 2,
                            ProductKind = (ProductKind)p.ProductKindID
                        }));
                    
                    //убираем из остатков тамбур, который утилизирован в этой смене
                    if (UtilizationProducts != null)
                    {
                        foreach (var product in UtilizationProducts)
                        {
                            var removeProduct = EndProducts.FirstOrDefault(d => d.ProductID == product.ProductID);
                            if (removeProduct != null)
                                EndProducts.Remove(removeProduct);
                        }
                    }

                    //убираем из остатков переходящий следующей смене тамбур в процессе производства
                    var doc = gammaBase.Docs.Where(d => d.DocTypeID == 3 && d.PlaceID == PlaceID && d.ShiftID == ShiftID && d.Date == CloseDate).FirstOrDefault();
                    if (doc != null)
                    {
                        var endProductsProductIDs = EndProducts.Select(d => (Guid?)d.ProductID).ToList();
                        var productRemainders = gammaBase.DocCloseShiftRemainders.Where(r => (r.RemainderTypeID ?? 0) == 0 && r.DocID == doc.DocID && endProductsProductIDs.Contains(r.ProductID));
                        foreach (var product in productRemainders)
                        {
                            var removeProduct = EndProducts.FirstOrDefault(d => d.ProductID == product.ProductID);
                            if (removeProduct != null)
                                EndProducts.Remove(removeProduct);
                        }
                    }
                }
        }

        private Dictionary<Guid, string> GetSampleMeasureUnits(Guid nomenclatureId, Guid? characteristicId)
        {
            var dict = new Dictionary<Guid, string>();
            using (var gammaBase = DB.GammaDb)
            {
                var unit =
                    gammaBase.C1CCharacteristics.FirstOrDefault(c => c.C1CCharacteristicID == characteristicId)?
                        .C1CMeasureUnitsPackage;
                if (unit != null && !dict.ContainsKey(unit.C1CMeasureUnitID))
                    dict.Add(unit.C1CMeasureUnitID, unit.Name);
                unit =
                    gammaBase.C1CNomenclature.FirstOrDefault(n => n.C1CNomenclatureID == nomenclatureId)?
                        .C1CMeasureUnitStorage;
                if (unit != null && !dict.ContainsKey(unit.C1CMeasureUnitID))
                    dict.Add(unit.C1CMeasureUnitID, unit.Name);
                unit =
                    gammaBase.C1CNomenclature.FirstOrDefault(n => n.C1CNomenclatureID == nomenclatureId)?
                        .C1CMeasureUnitSets;
                if (unit != null && !dict.ContainsKey(unit.C1CMeasureUnitID))
                    dict.Add(unit.C1CMeasureUnitID, unit.Name);
            }
            return dict;
        }

        private void AddProduct(RemainderType unum)
        {
            CurrentAddProductRemainder = unum;
            Messenger.Default.Register<ChoosenProductMessage>(this, AddProductSelected);
            MessageManager.OpenFindProduct(ProductKind.ProductSpool, true);
        }

        private void AddProductSelected(ChoosenProductMessage msg)
        {
            Messenger.Default.Unregister<ChoosenProductMessage>(this);
            using (var gammaBase = DB.GammaDb)
            {
                var isWrittenOff = gammaBase.vProductsInfo.Where(p => p.ProductID == msg.ProductID).Select(p => p.IsWrittenOff).FirstOrDefault();
                if (isWrittenOff ?? false && CurrentAddProductRemainder == RemainderType.End)
                {
                    MessageBox.Show("Нельзя добавить в остаток на конец смены списанный тамбур", "Списанный тамбур", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }
                var quantity = gammaBase.ProductSpools.Where(p => p.ProductID == msg.ProductID).Select(p => p.DecimalWeight).FirstOrDefault();
                switch (CurrentAddProductRemainder)
                {
                    case RemainderType.Begin:
                        BeginProducts.Add(new DocCloseShiftRemainder()
                        {
                            ProductID = msg.ProductID,
                            Quantity = quantity * 1000,
                            ProductKind = ProductKind.ProductSpool
                        });
                        break;
                    case RemainderType.End:
                        EndProducts.Add(new DocCloseShiftRemainder()
                        {
                            ProductID = msg.ProductID,
                            Quantity = quantity * 1000,
                            ProductKind = ProductKind.ProductSpool
                        });
                        break;

                }
            }
        }

        private void DeleteProduct(RemainderType unum)
        {
            CurrentAddProductRemainder = unum;
            switch (CurrentAddProductRemainder)
            {
                case RemainderType.Begin:
                    if (SelectedBeginProduct == null) return;
                    BeginProducts.Remove(SelectedBeginProduct);
                    break;
                case RemainderType.End:
                    if (SelectedEndProduct == null) return;
                    EndProducts.Remove(SelectedEndProduct);
                    break;

            }
        }

        private void AddUtilizationProduct()
        {
            Messenger.Default.Register<ChoosenProductMessage>(this, AddUtilizationProductSelected);
            MessageManager.OpenFindProduct(ProductKind.ProductSpool, true);
        }

        private void AddUtilizationProductSelected(ChoosenProductMessage msg)
        {
            Messenger.Default.Unregister<ChoosenProductMessage>(this);
            using (var gammaBase = DB.GammaDb)
            {
                var productBroke = gammaBase.DocBrokeDecisionProducts.Where(p => p.ProductID == msg.ProductID && p.StateID == 2).OrderByDescending(p => p.DocBroke.Docs.Date).Take(1).FirstOrDefault();
                if (productBroke?.Quantity == null)
                {
                    var dlgResult = MessageBox.Show("По "+ productBroke.Products.ProductKinds.Name + " не принято решение на утилизацию. Вы уверены?", productBroke.Products.ProductKinds.Name + " на утилизацию", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (dlgResult != MessageBoxResult.Yes)
                        return;
                }

                var brokeQuantity = productBroke?.Quantity;
                var product = new ObservableCollection<Product>(gammaBase.vProductsInfo
                    .Where(p => p.ProductID == msg.ProductID)
                    .Select(p => new Product()
                    {
                        CharacteristicID = (Guid)p.C1CCharacteristicID,
                        NomenclatureID = p.C1CNomenclatureID,
                        NomenclatureName = p.NomenclatureName,
                        Number = p.Number,
                        ProductID = p.ProductID,
                        Quantity = (brokeQuantity ?? p.ProductionQuantity) * (((ProductKind)p.ProductKindID == ProductKind.ProductSpool || (ProductKind)p.ProductKindID == ProductKind.ProductGroupPack) ? 1000 : 1),
                        ProductKind = (ProductKind)p.ProductKindID
                    }));
                foreach (var item in product)
                {
                    UtilizationProducts.Add(item);
                    var remainderProduct = EndProducts.FirstOrDefault(s => s.ProductID == item.ProductID && (s.RemainderTypeID ?? 0) == 2);
                    if (remainderProduct != null)
                        EndProducts.Remove(remainderProduct);
                }
            }
        }

        private void DeleteUtilizationProduct()
        {
            if (SelectedUtilizationProduct == null) return;
            UtilizationProducts.Remove(SelectedUtilizationProduct);
            FillEndProducts();
        }

        public void Clear(bool IsFillEnd = true)
        {
            Products?.Clear();
            DocCloseDocIds?.Clear();

            BeginProducts?.Clear();
            if (IsFillEnd) EndProducts?.Clear();
            InProducts?.Clear();
            OutProducts?.Clear();
            RepackProducts?.Clear();
            IsChanged = true;
            
            //            gammaBase.SaveChanges();           
        }

        /// <summary>
        /// Сохранение в БД
        /// </summary>
        /// <param name="docId">ID документа закрытия смены</param>
        public override bool SaveToModel(Guid docId)
        {
            if (IsReadOnly) return true;
            using (var gammaBase = DB.GammaDb)
            {
                var docCloseShift = gammaBase.Docs.Include(d => d.DocCloseShiftDocs).Include(d => d.DocCloseShiftWithdrawals).Include(d => d.DocCloseShiftRepackProducts)
                .First(d => d.DocID == docId);
                if (docCloseShift.DocCloseShiftDocs == null) docCloseShift.DocCloseShiftDocs = new List<Docs>();
                if (docCloseShift.DocCloseShiftRepackProducts == null)
                    docCloseShift.DocCloseShiftRepackProducts = new List<DocCloseShiftRepackProducts>();

                //if (docCloseShift.DocCloseShiftWithdrawals == null)
                //    docCloseShift.DocCloseShiftWithdrawals = new List<DocWithdrawal>();
                //if (docCloseShift.DocCloseShiftRemainders == null)
                //    docCloseShift.DocCloseShiftRemainders = new List<DocCloseShiftRemainders>();

                /*
                gammaBase.DocCloseShiftRemainders.RemoveRange(docCloseShift.DocCloseShiftRemainders.Where(p => (p.RemainderTypeID ?? 0) != 0));
                if (BeginProducts != null)
                    foreach (var spool in BeginProducts)
                    {
                        docCloseShift.DocCloseShiftRemainders.Add(new DocCloseShiftRemainders
                        {
                            DocID = docId,
                            ProductID = spool.ProductID,
                            DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                            RemainderTypeID = spool.RemainderTypeID,
                            Quantity = spool.Quantity,
                            StateID = spool.StateID
                        });
                    }
                if (EndProducts != null)
                    foreach (var spool in EndProducts)
                    {
                        if (docCloseShift.DocCloseShiftRemainders.Where(p => (p.RemainderTypeID ?? 0) == 0 && p.ProductID == spool.ProductID).Count() == 0)
                            docCloseShift.DocCloseShiftRemainders.Add(new DocCloseShiftRemainders
                            {
                                DocID = docId,
                                ProductID = spool.ProductID,
                                DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                                RemainderTypeID = spool.RemainderTypeID,
                                Quantity = spool.Quantity,
                                StateID = spool.StateID
                            });
                    }
                    */
                //gammaBase.DocCloseShiftUtilizationProducts.RemoveRange(docCloseShift.DocCloseShiftUtilizationProducts);
                if (UtilizationProducts != null)
                    foreach (var utilizationProduct in UtilizationProducts)
                    {
                        //docCloseShift.DocCloseShiftUtilizationProducts.Add(new DocCloseShiftUtilizationProducts
                        //{
                        //    DocID = docId,
                        //    DocCloseShiftUtilizationProductID = SqlGuidUtil.NewSequentialid(),
                        //    ProductID = utilizationProduct.ProductID,
                        //    Quantity = utilizationProduct.Weight
                        //});
                        var decisionProduct = gammaBase.DocBrokeDecisionProducts.Where(d => d.ProductID == utilizationProduct.ProductID && d.Quantity * 1000 == utilizationProduct.Quantity).OrderByDescending(d => d.DocBroke.Docs.Date).FirstOrDefault();
                        if (decisionProduct != null && !(decisionProduct?.DecisionApplied == true))
                        {
                            decisionProduct.DecisionApplied = true;
                        }
                    }
                /*gammaBase.DocCloseShiftMovementProducts.RemoveRange(docCloseShift.DocCloseShiftMovementProducts);
                if (InProducts != null)
                    foreach (var spool in InProducts)
                    {
                        docCloseShift.DocCloseShiftMovementProducts.Add(new DocCloseShiftMovementProducts
                        {
                            DocID = docId,
                            DocCloseShiftMovementProductID = SqlGuidUtil.NewSequentialid(),
                            ProductID = spool.ProductId,
                            DocMovementID = spool.DocMovementId,
                            Quantity = spool.Quantity,
                            MovementPlaceName = spool.OutPlaceName,
                            MovementPlaceZoneName = spool.OutPlaceZoneName,
                            DateMovement = spool.OutDate,
                            IsMovementIn = true
                        });
                    }
                //gammaBase.DocCloseShiftMovementProducts.RemoveRange(docCloseShift.DocCloseShiftMovementProducts);
                if (OutProducts != null)
                    foreach (var spool in OutProducts)
                    {
                        docCloseShift.DocCloseShiftMovementProducts.Add(new DocCloseShiftMovementProducts
                        {
                            DocID = docId,
                            DocCloseShiftMovementProductID = SqlGuidUtil.NewSequentialid(),
                            ProductID = spool.ProductId,
                            DocMovementID = spool.DocMovementId,
                            Quantity = spool.Quantity,
                            MovementPlaceName = spool.InPlaceName,
                            MovementPlaceZoneName = spool.InPlaceZoneName,
                            DateMovement = spool.InDate,
                            IsMovementIn = false
                        });
                    }*/
                gammaBase.DocCloseShiftRepackProducts.RemoveRange(docCloseShift.DocCloseShiftRepackProducts);
                if (RepackProducts != null)
                    foreach (var product in RepackProducts)
                    {
                        docCloseShift.DocCloseShiftRepackProducts.Add(new DocCloseShiftRepackProducts
                        {
                            DocID = docId,
                            C1CNomenclatureID = product.NomenclatureID,
                            C1CCharacteristicID = (Guid)product.CharacteristicID,
                            DocCloseShiftRepackProductID = SqlGuidUtil.NewSequentialid(),
                            Quantity = product.Quantity,
                            C1CMeasureUnitID = product.MeasureUnitId,
                            ProductionTaskID = product.ProductionTaskID
                        });
                    }
                if (IsChanged)
                {
                    docCloseShift.DocCloseShiftDocs.Clear();
                    foreach (var id in DocCloseDocIds)
                    {
                        docCloseShift.DocCloseShiftDocs.Add(gammaBase.Docs.First(d => d.DocID == id));
                    }
                }

                gammaBase.SaveChanges();
            }
            return true;
        }

    }
}