// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using DevExpress.Mvvm;
using Gamma.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Data.Entity.SqlServer;
using Gamma.Models;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Gamma.ViewModels
{
    /// <summary>
    /// ViewModel произведенная продукция передела
    /// </summary>
    public class PlaceProductsViewModel : RootViewModel
    {
        private PlaceProductsViewModel()
        {
            FindCommand = new DelegateCommand(Find);
            OpenDocProductCommand = new DelegateCommand(OpenDocProduct, () => SelectedProduct != null);
            CreateNewProductCommand = new DelegateCommand(CreateNewProduct, () => PlaceGroup == PlaceGroup.Wr && WorkSession.PlaceGroup == PlaceGroup);
            DeleteProductCommand = new DelegateCommand(DeleteProduct, CanDeleteExecute);
        }


        /// <summary>
        /// Initializes a new instance of the PlaceProductsViewModel class.
        /// </summary>
        public PlaceProductsViewModel(int placeID) : this()
        {
            Intervals = new List<string> {"Последние 500", "За мою смену", "За последний день", "Поиск"};
            if (WorkSession.IsProductionPlace)
            {
                Intervalid = 1;
            }
            PlaceID = placeID;
            PlaceGroup = (PlaceGroup)(GammaBase.Places.Where(p => p.PlaceID == placeID).Select(p => p.PlaceGroupID).FirstOrDefault());
            switch (PlaceGroup)
            {
                case PlaceGroup.PM:
                    QuantityHeader = "Вес, кг";
                    NewProductText = "Создать новый тамбур";
                    DeleteProductText = "Удалить тамбур";
                    break;
                case PlaceGroup.Rw:
                    QuantityHeader = "Вес, кг";
                    NewProductText = "Создать новый съем";
                    DeleteProductText = "Удалить съем";
                    break;
                case PlaceGroup.Convertings:
                    QuantityHeader = "Кол-во, рул";
                    NewProductText = "Создать новую паллету";
                    DeleteProductText = "Удалить паллету";
                    break;
                case PlaceGroup.Wr:
                    QuantityHeader = "Вес нетто, кг";
                    NewProductText = "Создать новую групповую упаковку";
                    DeleteProductText = "Удалить групповую упаковку";
                    break;
                case PlaceGroup.Baler:
                    QuantityHeader = "Вес нетто, кг";
                    NewProductText = "Создать новую кипу";
                    DeleteProductText = "Удалить кипу";
                    break;
            }
            Find();
        }

        private bool CanDeleteExecute()
        {
            if (SelectedProduct == null) return false;
            switch (PlaceGroup)
            {
                case PlaceGroup.PM:
                case PlaceGroup.Rw:
                    return DB.HaveWriteAccess("ProductSpools") 
                        && (WorkSession.PlaceGroup == PlaceGroup.Other || WorkSession.PlaceID == SelectedProduct.PlaceID);
                case PlaceGroup.Wr:
                    return DB.HaveWriteAccess("ProductGroupPacks")
                        && (WorkSession.PlaceGroup == PlaceGroup.Other || WorkSession.PlaceID == SelectedProduct.PlaceID);
                case PlaceGroup.Convertings:
                    return DB.HaveWriteAccess("ProductPallets")
                        && (WorkSession.PlaceGroup == PlaceGroup.Other || WorkSession.PlaceID == SelectedProduct.PlaceID);
                case PlaceGroup.Baler:
                    return DB.HaveWriteAccess("ProductBales")
                        && (WorkSession.PlaceGroup == PlaceGroup.Other || WorkSession.PlaceID == SelectedProduct.PlaceID);
                default:
                    return false;
            }
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;
            switch (SelectedProduct.PlaceGroup)
            {
                case PlaceGroup.Rw:
                    var dlgResult = MessageBox.Show("Хотите удалить съем целиком?", "Удаление продукта",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    switch (dlgResult)
                    {
                        case MessageBoxResult.Yes:
                            DeleteUnload();
                            return;
                        case MessageBoxResult.No:
                            DeleteSpool();
                            return;
                        default:
                            return;
                    }
                case PlaceGroup.PM:
                    DeleteSpool();
                    break;
                case PlaceGroup.Wr:
                    DeleteGroupPack();
                    break;
                case PlaceGroup.Convertings:
                    DeletePallet();
                    break;
                case PlaceGroup.Baler:
                    DeleteBale();
                    break;
            }
        }

        private void DeleteBale()
        {
            
        }

        private void DeletePallet()
        {
            var dlgResult = MessageBox.Show("Вы уверены, что хотите удалить паллету № " + SelectedProduct.Number + " ?", "Удаление паллеты",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dlgResult == MessageBoxResult.No) return;
            var delResult = GammaBase.DeletePallet(SelectedProduct.ProductID).FirstOrDefault();
            if (delResult != "")
            {
                MessageBox.Show(delResult, "Удалить не удалось", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
                Products.Remove(SelectedProduct);
        }


        private void DeleteGroupPack()
        {
            var dlgResult = MessageBox.Show("Вы уверены, что хотите удалить упаковку № " + SelectedProduct.Number + " ?", "Удаление тамбура",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dlgResult == MessageBoxResult.No) return;
            var delResult = GammaBase.DeleteGroupPack(SelectedProduct.ProductID).FirstOrDefault();
            if (delResult != "")
            {
                MessageBox.Show(delResult, "Удалить не удалось", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
                Products.Remove(SelectedProduct);
        }

        private void DeleteSpool()
        {
            var dlgResult = MessageBox.Show("Вы уверены, что хотите удалить тамбур № " + SelectedProduct.Number + " ?", "Удаление тамбура",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dlgResult == MessageBoxResult.No) return;
            var delResult = GammaBase.DeleteSpool(SelectedProduct.ProductID).FirstOrDefault();
            if (delResult != "")
            {
                MessageBox.Show(delResult, "Удалить не удалось", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
                Products.Remove(SelectedProduct);
        }

        private void DeleteUnload()
        {
            var delResult = GammaBase.DeleteUnload(SelectedProduct.DocID).FirstOrDefault();
            if (delResult != "")
            {
                MessageBox.Show(delResult, "Удалить не удалось", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
            {
                var productsToRemove = Products.Where(p => p.DocID == SelectedProduct.DocID).ToList();
                foreach (var product in productsToRemove)
                {
                    Products.Remove(product);
                }
            }
        }

        private void CreateNewProduct()
        {
            switch (PlaceGroup)
            {
                case PlaceGroup.Wr:
                    var curDate = DB.CurrentDateTime;
                    var lastGroupPack = GammaBase.Docs.Include(d => d.DocProduction.DocProductionProducts)
                        .Where(d => d.PlaceID == WorkSession.PlaceID && d.ShiftID == WorkSession.ShiftID && d.DocTypeID == (byte)DocTypes.DocProduction
                            && d.Date >= SqlFunctions.DateAdd("hh",-12, curDate))
                        .OrderByDescending(d => d.Date)
                        .FirstOrDefault();
                    if (lastGroupPack != null)
                        GammaBase.Entry(lastGroupPack).Reload();
                    if (lastGroupPack != null && !lastGroupPack.IsConfirmed && lastGroupPack.DocProduction.DocProductionProducts.Count > 0)
                    {
                        var docProductionProduct = lastGroupPack.DocProduction.DocProductionProducts.FirstOrDefault();
                        if (docProductionProduct != null)
                        {
                            MessageBox.Show("Предыдущая упаковка не подтверждена. Она будет открыта для редактирования", "Предыдущая упаковка", MessageBoxButton.OK, MessageBoxImage.Information);
                            MessageManager.OpenDocProduct(DocProductKinds.DocProductGroupPack, docProductionProduct.ProductID);
                        }
                    }
                    else MessageManager.CreateNewProduct(DocProductKinds.DocProductGroupPack);
                    break;
            }
        }

        private void OpenDocProduct()
        {
            switch (SelectedProduct.ProductKind)
            {
                case ProductKind.ProductSpool:
                    var placeGroupID = GammaBase.Docs.Where(d => d.DocID == SelectedProduct.DocID).
                        Select(d => d.Places.PlaceGroupID).FirstOrDefault();
                    if (placeGroupID == (byte)PlaceGroup.PM)
                        MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedProduct.ProductID);
                    else
                        MessageManager.OpenDocProduct(DocProductKinds.DocProductUnload, SelectedProduct.DocID);
                    break;
                case ProductKind.ProductGroupPack:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductGroupPack, SelectedProduct.ProductID);
                    break;
                case ProductKind.ProductPallet:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductPallet, SelectedProduct.ProductID);
                    break;
                case ProductKind.ProductPalletR:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductPalletR, SelectedProduct.ProductID);
                    break;
                case ProductKind.ProductBale:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductBale, SelectedProduct.ProductID);
                    break;
            }
        }
        private int PlaceID { get; set; }
        private PlaceGroup PlaceGroup { get; set; }
        private bool WarehouseProducts { get; set; }

        private void Find()
        {
            UIServices.SetBusyState();
            using (var gammaBase = DB.GammaDb)
            {
                switch (Intervalid)
                {
                    case 0:
                        Products = new ObservableCollection<ProductInfo>
                        (
                            gammaBase.vProductsInfo.Where(vpi => vpi.PlaceID == PlaceID).OrderByDescending(vpi => vpi.Date).Take(500)
                            .Select(vpi => new ProductInfo
                            {
                                CharacteristicID = vpi.C1CCharacteristicID,
                                Date = vpi.Date,
                                DocID = vpi.DocID,
                                NomenclatureID = vpi.C1CNomenclatureID,
                                NomenclatureName = vpi.NomenclatureName,
                                Number = vpi.Number,
                                Place = vpi.Place,
                                ProductID = vpi.ProductID,
                                ProductKind = (ProductKind)vpi.ProductKindID,
                                Quantity = (ProductKind)vpi.ProductKindID != ProductKind.ProductPallet && (ProductKind)vpi.ProductKindID != ProductKind.ProductPalletR ? vpi.ProductionQuantity * 1000 : vpi.ProductionQuantity,
                                ShiftID = vpi.ShiftID,
                                State = vpi.State,
                                PlaceID = vpi.PlaceID,
                                PlaceGroup = (PlaceGroup)vpi.PlaceGroupID,
                                CurrentPlace = vpi.CurrentPlace,
                                IsConfirmed = vpi.IsConfirmed
                            })   
                        );
                        break;
                    case 1:
                        Products = new ObservableCollection<ProductInfo>
                        (
                            from vpi in GammaBase.vProductsInfo
                            where vpi.PlaceID == PlaceID && vpi.ShiftID == WorkSession.ShiftID &&
                            vpi.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime(DB.CurrentDateTime)) && vpi.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime(DB.CurrentDateTime))
                            orderby vpi.Date descending
                            select new ProductInfo
                            {
                                CharacteristicID = vpi.C1CCharacteristicID,
                                Date = vpi.Date,
                                DocID = vpi.DocID,
                                NomenclatureID = vpi.C1CNomenclatureID,
                                NomenclatureName = vpi.NomenclatureName,
                                Number = vpi.Number,
                                Place = vpi.Place,
                                ProductID = vpi.ProductID,
                                ProductKind = (ProductKind)vpi.ProductKindID,
                                Quantity = (ProductKind)vpi.ProductKindID != ProductKind.ProductPallet && (ProductKind)vpi.ProductKindID != ProductKind.ProductPalletR ? vpi.ProductionQuantity * 1000 : vpi.ProductionQuantity,
                                ShiftID = vpi.ShiftID,
                                State = vpi.State,
                                PlaceID = vpi.PlaceID,
                                PlaceGroup = (PlaceGroup)vpi.PlaceGroupID,
                                CurrentPlace = vpi.CurrentPlace,
                                IsConfirmed = vpi.IsConfirmed
                            }
                        );
                        break;
                    case 2:
                        var endTime = DB.CurrentDateTime;
                        var beginTime = endTime.AddDays(-1);
                        Products = new ObservableCollection<ProductInfo>
                        (
                            from vpi in GammaBase.vProductsInfo
                            where vpi.PlaceID == PlaceID &&
                            vpi.Date >= beginTime && vpi.Date <= endTime
                            orderby vpi.Date descending
                            select new ProductInfo
                            {
                                CharacteristicID = vpi.C1CCharacteristicID,
                                Date = vpi.Date,
                                DocID = vpi.DocID,
                                NomenclatureID = vpi.C1CNomenclatureID,
                                NomenclatureName = vpi.NomenclatureName,
                                Number = vpi.Number,
                                Place = vpi.Place,
                                ProductID = vpi.ProductID,
                                ProductKind = (ProductKind)vpi.ProductKindID,
                                Quantity = (ProductKind)vpi.ProductKindID != ProductKind.ProductPallet && (ProductKind)vpi.ProductKindID != ProductKind.ProductPalletR ? vpi.ProductionQuantity * 1000 : vpi.ProductionQuantity,
                                ShiftID = vpi.ShiftID,
                                State = vpi.State,
                                PlaceID = vpi.PlaceID,
                                PlaceGroup = (PlaceGroup)vpi.PlaceGroupID,
                                CurrentPlace = vpi.CurrentPlace,
                                IsConfirmed = vpi.IsConfirmed
                            }
                        );
                        break;
                    default:
                        Products = new ObservableCollection<ProductInfo>
                        (
                            from vpi in GammaBase.vProductsInfo
                            where vpi.PlaceID == PlaceID &&
                            (Number == null || Number == "" || vpi.Number == Number) &&
                            (DateBegin == null || vpi.Date >= DateBegin) &&
                            (DateEnd == null || vpi.Date <= DateEnd)
                            orderby vpi.Date descending
                            select new ProductInfo
                            {
                                CharacteristicID = vpi.C1CCharacteristicID,
                                Date = vpi.Date,
                                DocID = vpi.DocID,
                                NomenclatureID = vpi.C1CNomenclatureID,
                                NomenclatureName = vpi.NomenclatureName,
                                Number = vpi.Number,
                                Place = vpi.Place,
                                ProductID = vpi.ProductID,
                                ProductKind = (ProductKind)vpi.ProductKindID,
                                Quantity = (ProductKind)vpi.ProductKindID != ProductKind.ProductPallet && (ProductKind)vpi.ProductKindID != ProductKind.ProductPalletR ? vpi.ProductionQuantity * 1000 : vpi.ProductionQuantity,
                                ShiftID = vpi.ShiftID,
                                State = vpi.State,
                                PlaceID = vpi.PlaceID,
                                PlaceGroup = (PlaceGroup)vpi.PlaceGroupID,
                                CurrentPlace = vpi.CurrentPlace,
                                IsConfirmed = vpi.IsConfirmed
                            }
                        );
                        break;
                }
            }
        }

        public string QuantityHeader { get; set; }
        private ObservableCollection<ProductInfo> _products;
        public ObservableCollection<ProductInfo> Products
        {
            get
            {
                return _products;
            }
            set
            {
                _products = value;
                RaisePropertyChanged("Products");
            }
        }
        
        public ProductInfo SelectedProduct { get; set; }
        public string Number { get; set; }       
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public List<string> Intervals { get; set; }
        private int _intervalid;

        public int Intervalid
        {
            get { return _intervalid; }
            set
            {
                if (_intervalid == value) return;
                _intervalid = value < 0 ? 0 : value;
                if (_intervalid < 3) Find();
            }
        }
        public DelegateCommand DeleteProductCommand { get; private set; }
        public DelegateCommand CreateNewProductCommand { get; private set; }
        public DelegateCommand FindCommand { get; private set; }
        public DelegateCommand OpenDocProductCommand { get; private set; }
        
        public string DeleteProductText { get; set; }
        
        public string NewProductText { get; set; }

        public override void Dispose()
        {
            base.Dispose();
            DeleteProductCommand = null;
            CreateNewProductCommand = null;
            FindCommand = null;
            OpenDocProductCommand = null;
        }
    }
}