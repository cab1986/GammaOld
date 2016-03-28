using GalaSoft.MvvmLight.Command;
using Gamma.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class PlaceProductsViewModel : RootViewModel
    {
        /// <summary>
        /// Initializes a new instance of the PlaceProductsViewModel class.
        /// </summary>
        public PlaceProductsViewModel(int placeID)
        {
            Intervals = new List<string>();
            Intervals.Add("Последние 500");
            Intervals.Add("За мою смену");
            Intervals.Add("За последний день");
            Intervals.Add("Поиск");
            FindCommand = new RelayCommand(Find);
            OpenDocProductCommand = new RelayCommand(OpenDocProduct, () => SelectedProduct != null);
            CreateNewProductCommand = new RelayCommand(CreateNewProduct, () => PlaceGroup == PlaceGroups.WR);
            DeleteProductCommand = new RelayCommand(DeleteProduct, CanDeleteExecute);
            PlaceID = placeID;
            PlaceGroup = (PlaceGroups)DB.GammaBase.Places.Where(p => p.PlaceID == PlaceID).Select(p => p.PlaceGroupID).FirstOrDefault();
            switch (PlaceGroup)
            {
                case PlaceGroups.PM:
                    QuantityHeader = "Вес, кг";
                    NewProductText = "Создать новый тамбур";
                    DeleteProductText = "Удалить тамбур";
                    break;
                case PlaceGroups.RW:
                    QuantityHeader = "Вес, кг";
                    NewProductText = "Создать новый съем";
                    DeleteProductText = "Удалить съем";
                    break;
                case PlaceGroups.Convertings:
                    QuantityHeader = "Кол-во, шт";
                    NewProductText = "Создать новую паллету";
                    DeleteProductText = "Удалить паллету";
                    break;
                case PlaceGroups.WR:
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
                case PlaceGroups.RW:
                    return DB.HaveWriteAccess("ProductSpools") 
                        && (WorkSession.PlaceGroup == PlaceGroups.Other || WorkSession.PlaceID == SelectedProduct.PlaceID);
                case PlaceGroups.WR:
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
                case PlaceGroups.RW:
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
                case PlaceGroups.WR:
                    DeleteGroupPack();
                    break;
            };
        }
        private void DeleteGroupPack()
        {
            var dlgResult = MessageBox.Show("Вы уверены, что хотите удалить упаковку № " + SelectedProduct.Number + " ?", "Удаление тамбура",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dlgResult == MessageBoxResult.No) return;
            var delResult = DB.GammaBase.DeleteGroupPack(SelectedProduct.ProductID).FirstOrDefault();
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
            var delResult = DB.GammaBase.DeleteSpool(SelectedProduct.ProductID).FirstOrDefault();
            if (delResult != "")
            {
                MessageBox.Show(delResult, "Удалить не удалось", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
                Products.Remove(SelectedProduct);
        }
        private void DeleteUnload()
        {
            var delResult = DB.GammaBase.DeleteUnload(SelectedProduct.DocID).FirstOrDefault();
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
                case PlaceGroups.WR:
                    MessageManager.CreateNewProduct(DocProductKinds.DocProductGroupPack);
                    break;
            }
        }

        private void OpenDocProduct()
        {
            switch (SelectedProduct.ProductKind)
            {
                case ProductKinds.ProductSpool:
                    var placeGroupID = DB.GammaBase.Docs.Where(d => d.DocID == SelectedProduct.DocID).
                        Select(d => d.Places.PlaceGroupID).FirstOrDefault();
                    if (placeGroupID == (byte)PlaceGroups.PM)
                        MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedProduct.ProductID);
                    else
                        MessageManager.OpenDocProduct(DocProductKinds.DocProductUnload, SelectedProduct.DocID);
                    break;
                case ProductKinds.ProductGroupPack:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductGroupPack, SelectedProduct.ProductID);
                    break;
                case ProductKinds.ProductPallet:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductPallet, SelectedProduct.ProductID);
                    break;
            }
        }
        private int PlaceID { get; set; }
        private PlaceGroups PlaceGroup { get; set; }
        private void Find()
        {
            UIServices.SetBusyState();
            switch (IntervalID)
            {
                case 0:
                    Products = new ObservableCollection<ProductInfo>
                    ((
                        from vpi in DB.GammaBase.vProductsInfo
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
                            Quantity = vpi.Quantity,
                            ShiftID = vpi.ShiftID,
                            State = vpi.State,
                            PlaceID = vpi.PlaceID
                        }
                    ).Take(500));
                    break;
                case 1:
                    Products = new ObservableCollection<ProductInfo>
                    (
                        from vpi in DB.GammaBase.vProductsInfo
                        where vpi.PlaceID == PlaceID && vpi.ShiftID == WorkSession.ShiftID &&
                        vpi.Date >= DB.GetShiftBeginTime(DB.CurrentDateTime) && vpi.Date <= DB.GetShiftEndTime(DB.CurrentDateTime)
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
                            Quantity = vpi.Quantity,
                            ShiftID = vpi.ShiftID,
                            State = vpi.State,
                            PlaceID = vpi.PlaceID
                        }
                    );
                    break;
                case 2:
                    var endTime = DB.CurrentDateTime;
                    var beginTime = endTime.AddDays(-1);
                    Products = new ObservableCollection<ProductInfo>
                    (
                        from vpi in DB.GammaBase.vProductsInfo
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
                            Quantity = vpi.Quantity,
                            ShiftID = vpi.ShiftID,
                            State = vpi.State,
                            PlaceID = vpi.PlaceID
                        }
                    );
                    break;
                default:
                    Products = new ObservableCollection<ProductInfo>
                    (
                        from vpi in DB.GammaBase.vProductsInfo
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
                            Quantity = vpi.Quantity,
                            ShiftID = vpi.ShiftID,
                            State = vpi.State,
                            PlaceID = vpi.PlaceID
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
        public int IntervalID { get; set; }
        public RelayCommand DeleteProductCommand { get; private set; }
        public RelayCommand CreateNewProductCommand { get; private set; }
        public RelayCommand FindCommand { get; private set; }
        public RelayCommand OpenDocProductCommand { get; private set; }
        public string DeleteProductText { get; set; }
        public string NewProductText { get; set; }
    }
}