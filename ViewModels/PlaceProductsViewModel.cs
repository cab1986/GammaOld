using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
            PlaceID = placeID;
            PlaceGroup = (PlaceGroups)DB.GammaBase.Places.Where(p => p.PlaceID == PlaceID).Select(p => p.PlaceGroupID).FirstOrDefault();
            switch (PlaceGroup)
            {
                case PlaceGroups.PM:
                    QuantityHeader = "Вес, кг";
                    break;
                case PlaceGroups.RW:
                    QuantityHeader = "Вес, кг";
                    break;
                case PlaceGroups.Convertings:
                    QuantityHeader = "Кол-во, шт";
                    break;
            }
            Find();
        }

        private void OpenDocProduct()
        {
            switch (SelectedProduct.ProductKind)
            {
                case ProductKinds.ProductSpool:
                    MessageManager.OpenDocProduct(new OpenDocProductMessage()
                        {
                            DocProductKind = DocProductKinds.DocProductSpool,
                            ID = SelectedProduct.ProductID,
                            IsNewProduct = false
                        });
                    break;
            }
        }
        private int PlaceID { get; set; }
        private PlaceGroups PlaceGroup { get; set; }
        private void Find()
        {
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
                            DocProductID = vpi.DocID,
                            NomenclatureID = vpi.C1CNomenclatureID,
                            NomenclatureName = vpi.NomenclatureName,
                            Number = vpi.Number,
                            Place = vpi.Place,
                            ProductID = vpi.ProductID,
                            ProductKind = (ProductKinds)vpi.ProductKindID,
                            Quantity = vpi.Quantity,
                            ShiftID = vpi.ShiftID,
                            State = vpi.State
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
                            DocProductID = vpi.DocID,
                            NomenclatureID = vpi.C1CNomenclatureID,
                            NomenclatureName = vpi.NomenclatureName,
                            Number = vpi.Number,
                            Place = vpi.Place,
                            ProductID = vpi.ProductID,
                            ProductKind = (ProductKinds)vpi.ProductKindID,
                            Quantity = vpi.Quantity,
                            ShiftID = vpi.ShiftID,
                            State = vpi.State
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
                            DocProductID = vpi.DocID,
                            NomenclatureID = vpi.C1CNomenclatureID,
                            NomenclatureName = vpi.NomenclatureName,
                            Number = vpi.Number,
                            Place = vpi.Place,
                            ProductID = vpi.ProductID,
                            ProductKind = (ProductKinds)vpi.ProductKindID,
                            Quantity = vpi.Quantity,
                            ShiftID = vpi.ShiftID,
                            State = vpi.State
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
                            DocProductID = vpi.DocID,
                            NomenclatureID = vpi.C1CNomenclatureID,
                            NomenclatureName = vpi.NomenclatureName,
                            Number = vpi.Number,
                            Place = vpi.Place,
                            ProductID = vpi.ProductID,
                            ProductKind = (ProductKinds)vpi.ProductKindID,
                            Quantity = vpi.Quantity,
                            ShiftID = vpi.ShiftID,
                            State = vpi.State
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
        public RelayCommand FindCommand { get; private set; }
        public RelayCommand OpenDocProductCommand { get; private set; }
    }
}