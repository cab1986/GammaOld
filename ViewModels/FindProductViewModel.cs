using DevExpress.Mvvm;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Gamma.Common;
using Gamma.Models;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class FindProductViewModel : DbEditItemWithNomenclatureViewModel
    {
        /// <summary>
        /// Initializes a new instance of the FindProductViewModel class.
        /// </summary>
        
        private FindProductViewModel(GammaEntities gammaBase = null) : base(gammaBase)
        {
            Messenger.Default.Register<BarcodeMessage>(this,BarcodeReceived);
            ProductKindsList = Functions.EnumDescriptionsToList(typeof(ProductKind));
            ProductKindsList.Add("Все");
            States = Functions.EnumDescriptionsToList(typeof(ProductState));
            States.Add("Любое");
            SelectedStateIndex = States.Count - 1;
            ResetSearchCommand = new DelegateCommand(ResetSearch);
            FindCommand = new DelegateCommand(() => Find(false));
            ChooseProductCommand = new DelegateCommand(ChooseProduct, () => SelectedProduct != null);
            ActivatedCommand = new DelegateCommand(() => IsActive = true);
            DeactivatedCommand = new DelegateCommand(() => IsActive = false);
            OpenProductCommand = new DelegateCommand(OpenProduct, () => SelectedProduct != null);
            PlacesList = (from p in GammaBase.Places
                          select new
                          Place
                          {
                              PlaceName = p.Name,
                              PlaceID = p.PlaceID
                          }
                          ).ToList();
            SelectedPlaces = new List<Object>(PlacesList);
        }

        public FindProductViewModel(FindProductMessage msg) : this()
        {
            ButtonPanelVisible = msg.ChooseProduct;
            SelectedProductKindIndex = (byte)msg.ProductKind;
            ProductKindSelectEnabled = msg.AllowChangeProductKind;
        }
        private bool _buttonPanelVisible;
        public bool ButtonPanelVisible
        {
            get
            {
                return _buttonPanelVisible;
            }
            set
            {
                if (_buttonPanelVisible == value)
                    return;
                _buttonPanelVisible = value;
                RaisePropertyChanged("ButtonPanelVisible");
            }
        }
        private string _number = "";
        public DateTime? DateBegin
        {
            get
            {
                return _dateBegin;
            }
            set
            {
                _dateBegin = value;
                RaisePropertyChanged("DateBegin");
            }
        }
        public DateTime? DateEnd
        {
            get
            {
                return _dateEnd;
            }
            set
            {
                _dateEnd = value;
                RaisePropertyChanged("DateEnd");
            }
        }
        public string Number
        {
            get
            {
                return _number;
            }
            set
            {
                if (_number == value)
                    return;
                _number = value;
                RaisePropertyChanged("Number");
            }
        }
        private string _barcode = "";
        public string Barcode
        {
            get
            {
                return _barcode;
            }
            set
            {
                _barcode = value;
                RaisePropertyChanged("Barcode");
            }
        }

        private void BarcodeReceived(BarcodeMessage msg)
        {
            if (!IsActive) return;
            ResetSearch();
            Barcode = msg.Barcode;
            Find(true);
            if (ButtonPanelVisible && FoundProducts.Count == 1)
            {
                SelectedProduct = FoundProducts.First();
                ChooseProduct();
            }
        }
        public DelegateCommand FindCommand { get; private set; }
        public DelegateCommand ResetSearchCommand { get; private set; }

        private void Find(bool isFromScanner)
        {
            UIServices.SetBusyState();
            if (isFromScanner)
            {
                FoundProducts = new ObservableCollection<ProductInfo>
                (
                    (
                        from pinfo in GammaBase.vProductsInfo
                        where pinfo.BarCode == Barcode && (!ButtonPanelVisible || (ButtonPanelVisible && !(pinfo.IsWrittenOff??false)))
                        select new ProductInfo
                        {
                            DocID = pinfo.DocID,
                            ProductID = pinfo.ProductID,
                            Number = pinfo.Number,
                            CharacteristicID = pinfo.C1CCharacteristicID,
                            Date = pinfo.Date,
                            NomenclatureID = pinfo.C1CNomenclatureID,
                            NomenclatureName = pinfo.NomenclatureName,
                            ProductKind = (ProductKind)pinfo.ProductKindID,
                            Quantity = pinfo.Quantity,
                            ShiftID = pinfo.ShiftID,
                            Place = pinfo.Place,
                            State = pinfo.State,
                            PlaceGroup = (PlaceGroups)pinfo.PlaceGroupID
                        }
                    ).OrderByDescending(p => p.Date)
                );
            }
            else
            {
                var charId = SelectedCharacteristic?.CharacteristicID ?? new Guid();
                var selectedPlaces = new List<string>();
                if (SelectedPlaces != null)
                    selectedPlaces = SelectedPlaces.Cast<string>().ToList();
                FoundProducts = new ObservableCollection<ProductInfo>
                (
                from pinfo in GammaBase.vProductsInfo
                where
                (Number == null || pinfo.Number.Contains(Number) || Number == "") &&
                (Barcode == null || pinfo.BarCode == Barcode || Barcode == "") &&
                (NomenclatureID == null || pinfo.C1CNomenclatureID == NomenclatureID) &&
                (charId == new Guid() || pinfo.C1CCharacteristicID == charId) &&
                (pinfo.ProductKindID == SelectedProductKindIndex || SelectedProductKindIndex == ProductKindsList.Count - 1) &&
                ((DateBegin == null || pinfo.Date >= DateBegin) && (DateEnd == null || pinfo.Date <= DateEnd)) &&
                ((ButtonPanelVisible && !(pinfo.IsWrittenOff??false)) || !ButtonPanelVisible)
                &&
                (selectedPlaces.Count == 0 || selectedPlaces.Contains(pinfo.Place)) &&
                (SelectedStateIndex == States.Count - 1 || SelectedStateIndex == pinfo.StateID)
                select new ProductInfo
                {
                    DocID = pinfo.DocID,
                    ProductID = pinfo.ProductID,
                    Number = pinfo.Number,
                    CharacteristicID = pinfo.C1CCharacteristicID,
                    Date = pinfo.Date,
                    NomenclatureID = pinfo.C1CNomenclatureID,
                    NomenclatureName = pinfo.NomenclatureName,
                    ProductKind = (ProductKind)pinfo.ProductKindID,
                    Quantity = pinfo.Quantity,
                    ShiftID = pinfo.ShiftID,
                    State = pinfo.State,
                    Place = pinfo.Place,
                    PlaceGroup = (PlaceGroups)pinfo.PlaceGroupID
                }
                );
            }
            if (FoundProducts.Count == 0 && ButtonPanelVisible)
            {
                MessageBox.Show("Продукт уже списан или не существует в базе", "Продукт не найден");
            }
            if (FoundProducts.Count != 1 || !ButtonPanelVisible) return;
            Messenger.Default.Send(new ChoosenProductMessage { ProductID = FoundProducts.First().ProductID });
            CloseWindow();
        }

        private void ResetSearch()
        {
            Number = "";
            Barcode = "";
            NomenclatureID = null;
            DateBegin = null;
            DateEnd = null;
            if (SelectedPlaces != null) SelectedPlaces = null;
            SelectedStateIndex = States.Count - 1;
        }      
        private byte _selectedProductKindIndex;
        public byte SelectedProductKindIndex
        {
            get
            {
                return _selectedProductKindIndex;
            }
            set
            {
                _selectedProductKindIndex = value;
                RaisePropertyChanged("SelectedProductKindIndex");
            }
        }
        public List<string> ProductKindsList { get; set; }
        public List<string> States { get; set; }
        private int _selectedStateIndex;
        public int SelectedStateIndex
        {
            get
            {
                return _selectedStateIndex;
            }
            set
            {
                _selectedStateIndex = value;
                RaisePropertyChanged("SelectedStateIndex");
            }
        }
        public List<Place> PlacesList { get; set; }
        private List<object> _selectedPlaces = new List<object>();
        public List<object> SelectedPlaces // Object требует визуальный компонент
        {
            get
            {
                return _selectedPlaces;
            }
            set
            {
                _selectedPlaces= value;
                RaisePropertyChanged("SelectedPlaces");
            }
        }
        public Characteristic SelectedCharacteristic { get; set; }
        private ObservableCollection<ProductInfo> _foundProducts;
        public ObservableCollection<ProductInfo> FoundProducts
        {
            get
            {
                return _foundProducts;
            }
            set
            {
                _foundProducts = value;
                RaisePropertyChanged("FoundProducts");
            }
        }
        public ProductInfo SelectedProduct { get; set; }
        private bool _productKindSelectEnabled = true;
        public bool ProductKindSelectEnabled
        {
            get
            {
                return _productKindSelectEnabled;
            }
            set
            {
                _productKindSelectEnabled = value;
                RaisePropertyChanged("ProductKindSelectEnabled");
            }
        }
        private DateTime? _dateBegin;
        private DateTime? _dateEnd;

        public DelegateCommand ChooseProductCommand { get; set; }

        private void ChooseProduct()
        {
            if (SelectedProduct == null) return;
            Messenger.Default.Send(new ChoosenProductMessage { ProductID = SelectedProduct.ProductID });
            CloseWindow();
        }
        public DelegateCommand ActivatedCommand { get; private set; }
        public DelegateCommand DeactivatedCommand { get; private set; }
        private bool IsActive { get; set; }
        public DelegateCommand OpenProductCommand { get; private set; }
        private void OpenProduct()
        {
            switch (SelectedProduct.ProductKind)
            {
                case (ProductKind.ProductSpool):
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedProduct.ProductID);
                    break;
                case (ProductKind.ProductGroupPack):
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductGroupPack, SelectedProduct.ProductID);
                    break;
                case (ProductKind.ProductPallet):
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductPallet, SelectedProduct.ProductID);
                    break;
            }
        }
    }
}