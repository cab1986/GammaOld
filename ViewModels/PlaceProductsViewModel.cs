using DevExpress.Mvvm;
using Gamma.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// <summary>
        /// Initializes a new instance of the PlaceProductsViewModel class.
        /// </summary>
        public PlaceProductsViewModel(int placeID, GammaEntities gammaBase = null): base(gammaBase)
        {
            Intervals = new List<string> {"Последние 500", "За мою смену", "За последний день", "Поиск"};
            FindCommand = new DelegateCommand(Find);
            OpenDocProductCommand = new DelegateCommand(OpenDocProduct, () => SelectedProduct != null);
            CreateNewProductCommand = new DelegateCommand(CreateNewProduct, () => PlaceGroup == PlaceGroups.Wr);
            DeleteProductCommand = new DelegateCommand(DeleteProduct, CanDeleteExecute);
            PlaceID = placeID;
            PlaceGroup = (PlaceGroups)(GammaBase.Places.Where(p => p.PlaceID == placeID).Select(p => p.PlaceGroupID).FirstOrDefault()??0);
            switch (PlaceGroup)
            {
                case PlaceGroups.PM:
                    QuantityHeader = "Вес, кг";
                    NewProductText = "Создать новый тамбур";
                    DeleteProductText = "Удалить тамбур";
                    break;
                case PlaceGroups.Rw:
                    QuantityHeader = "Вес, кг";
                    NewProductText = "Создать новый съем";
                    DeleteProductText = "Удалить съем";
                    break;
                case PlaceGroups.Convertings:
                    QuantityHeader = "Кол-во, шт";
                    NewProductText = "Создать новую паллету";
                    DeleteProductText = "Удалить паллету";
                    break;
                case PlaceGroups.Wr:
                    QuantityHeader = "Вес нетто, кг";
                    NewProductText = "Создать новую групповую упаковку";
                    DeleteProductText = "Удалить групповую упаковку";
                    break;
            }
            Find();
        }

        private bool CanDeleteExecute()
        {
            if (SelectedProduct == null) return false;
            switch (PlaceGroup)
            {
                case PlaceGroups.PM:
                case PlaceGroups.Rw:
                    return DB.HaveWriteAccess("ProductSpools") 
                        && (WorkSession.PlaceGroup == PlaceGroups.Other || WorkSession.PlaceID == SelectedProduct.PlaceID);
                case PlaceGroups.Wr:
                    return DB.HaveWriteAccess("ProductGroupPacks")
                        && (WorkSession.PlaceGroup == PlaceGroups.Other || WorkSession.PlaceID == SelectedProduct.PlaceID);
                default:
                    return false;
            }
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;
            switch (SelectedProduct.PlaceGroup)
            {
                case PlaceGroups.Rw:
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
                case PlaceGroups.PM:
                    DeleteSpool();
                    break;
                case PlaceGroups.Wr:
                    DeleteGroupPack();
                    break;
            }
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
                case PlaceGroups.Wr:
                    var lastGroupPack = GammaBase.Docs
                        .Where(d => d.PlaceID == WorkSession.PlaceID && d.ShiftID == WorkSession.ShiftID && d.DocTypeID == (byte)DocTypes.DocProduction)
                        .OrderByDescending(d => d.Date)
                        .FirstOrDefault();
                    GammaBase.Entry(lastGroupPack).Reload();
                    if (lastGroupPack != null && !lastGroupPack.IsConfirmed)
                    {
                        MessageBox.Show("Предыдущая упаковка не подтверждена. Она будет открыта для редактирования", "Предыдущая упаковка", MessageBoxButton.OK, MessageBoxImage.Information);
                        MessageManager.OpenDocProduct(DocProductKinds.DocProductGroupPack, lastGroupPack.DocID);
                    }
                    else MessageManager.CreateNewProduct(DocProductKinds.DocProductGroupPack);
                    break;
            }
        }

        private void OpenDocProduct()
        {
            switch (SelectedProduct.ProductKind)
            {
                case ProductKinds.ProductSpool:
                    var placeGroupID = GammaBase.Docs.Where(d => d.DocID == SelectedProduct.DocID).
                        Select(d => d.Places.PlaceGroupID).FirstOrDefault();
                    if (placeGroupID == (byte)PlaceGroups.PM)
                        MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedProduct.ProductID);
                    else
                        MessageManager.OpenDocProduct(DocProductKinds.DocProductUnload, SelectedProduct.DocID);
                    break;
                case ProductKinds.ProductGroupPack:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductGroupPack, SelectedProduct.DocID);
                    break;
                case ProductKinds.ProductPallet:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductPallet, SelectedProduct.DocID);
                    break;
            }
        }
        private int PlaceID { get; set; }
        private PlaceGroups PlaceGroup { get; set; }
        private void Find()
        {
            UIServices.SetBusyState();
            switch (Intervalid)
            {
                case 0:
                    Products = new ObservableCollection<ProductInfo>
                    ((
                        from vpi in GammaBase.vProductsInfo
                        where vpi.PlaceID == PlaceID
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
                            ProductKind = (ProductKinds)vpi.ProductKindID,
                            Quantity = (int)vpi.Quantity,
                            ShiftID = vpi.ShiftID,
                            State = vpi.State,
                            PlaceID = vpi.PlaceID,
                            PlaceGroup = (PlaceGroups)vpi.PlaceGroupID
                        }
                    ).Take(500));
                    break;
                case 1:
                    Products = new ObservableCollection<ProductInfo>
                    (
                        from vpi in GammaBase.vProductsInfo
                        where vpi.PlaceID == PlaceID && vpi.ShiftID == WorkSession.ShiftID &&
                        vpi.Date >= SqlFunctions.DateAdd("hh",-1,DB.GetShiftBeginTime(DB.CurrentDateTime)) && vpi.Date <= SqlFunctions.DateAdd("hh",1,DB.GetShiftEndTime(DB.CurrentDateTime))
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
                            ProductKind = (ProductKinds)vpi.ProductKindID,
                            Quantity = (int)vpi.Quantity,
                            ShiftID = vpi.ShiftID,
                            State = vpi.State,
                            PlaceID = vpi.PlaceID,
                            PlaceGroup = (PlaceGroups)vpi.PlaceGroupID
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
                            ProductKind = (ProductKinds)vpi.ProductKindID,
                            Quantity = (int)vpi.Quantity,
                            ShiftID = vpi.ShiftID,
                            State = vpi.State,
                            PlaceID = vpi.PlaceID,
                            PlaceGroup = (PlaceGroups)vpi.PlaceGroupID
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
                            ProductKind = (ProductKinds)vpi.ProductKindID,
                            Quantity = (int)vpi.Quantity,
                            ShiftID = vpi.ShiftID,
                            State = vpi.State,
                            PlaceID = vpi.PlaceID,
                            PlaceGroup = (PlaceGroups)vpi.PlaceGroupID
                        }
                    );
                    break;
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
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string DeleteProductText { get; set; }
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string NewProductText { get; set; }
    }
}