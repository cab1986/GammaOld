using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.ComponentModel;
using System.Windows.Documents;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class FindProductViewModel : DBEditItemWithNomenclatureViewModel
    {
        /// <summary>
        /// Initializes a new instance of the FindProductViewModel class.
        /// </summary>
        
        private FindProductViewModel()
        {
            Messenger.Default.Register<BarcodeMessage>(this,BarcodeReceived);
            ProductKindsList = Functions.EnumDescriptionsToList(typeof(ProductKinds));
            ProductKindsList.Add("Все");
            States = Functions.EnumDescriptionsToList(typeof(ProductStates));
            States.Add("Любое");
            SelectedStateIndex = States.Count - 1;
            ResetSearchCommand = new RelayCommand(ResetSearch);
            FindCommand = new RelayCommand(() => Find(false));
            ChooseProductCommand = new RelayCommand(ChooseProduct, () => SelectedProduct != null);
            ActivatedCommand = new RelayCommand(() => IsActive = true);
            DeactivatedCommand = new RelayCommand(() => IsActive = false);
            OpenProductCommand = new RelayCommand(OpenProduct, () => SelectedProduct != null);
            PlacesList = (from p in DB.GammaBase.Places
                          select new
                          Place
                          {
                              PlaceName = p.Name,
                              PlaceID = p.PlaceID
                          }
                          ).ToList<Place>();
            SelectedPlaces = new List<Object>(PlacesList);
        }
        public FindProductViewModel(FindProductMessage msg) : this()
        {
            ButtonPanelVisible = msg.ChooseSourceProduct;
            SelectedProductKindIndex = (byte)msg.ProductKind;
            ProductKindSelectEnabled = !msg.ChooseSourceProduct;
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
        public void BarcodeReceived(BarcodeMessage msg)
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
        public RelayCommand FindCommand { get; private set; }
        public RelayCommand ResetSearchCommand { get; private set; }

        private void Find(bool IsFromScanner)
        {
            if (IsFromScanner)
            {
                FoundProducts = new ObservableCollection<ProductInfo>
                (
                from pinfo in DB.GammaBase.vProductsInfo
                where pinfo.BarCode == Barcode
                select new ProductInfo
                {
                    DocID = pinfo.DocID,
                    ProductID = pinfo.ProductID,
                    Number = pinfo.Number,
                    CharacteristicID = pinfo.C1CCharacteristicID,
                    Date = pinfo.Date,
                    NomenclatureID = pinfo.C1CNomenclatureID,
                    NomenclatureName = pinfo.NomenclatureName,
                    ProductKind = (ProductKinds)pinfo.ProductKindID,
                    Quantity = pinfo.Quantity,
                    ShiftID = pinfo.ShiftID
                }
                );
            }
            else
            {
                Guid charID = SelectedCharacteristic == null ? new Guid() : SelectedCharacteristic.CharacteristicID;
                var selectedPlaces = new List<string>();
                if (SelectedPlaces != null)
                    selectedPlaces = SelectedPlaces.Cast<string>().ToList();
                FoundProducts = new ObservableCollection<ProductInfo>
                (
                from pinfo in DB.GammaBase.vProductsInfo
                where
                (Number == null || pinfo.Number == Number || Number == "") &&
                (Barcode == null || pinfo.BarCode == Barcode || Barcode == "") &&
                (NomenclatureID == null ? true : pinfo.C1CNomenclatureID == NomenclatureID) &&
                (charID == new Guid() ? true : pinfo.C1CCharacteristicID == charID) &&
                (pinfo.ProductKindID == SelectedProductKindIndex || SelectedProductKindIndex == ProductKindsList.Count - 1) &&
                ((DateBegin == null ? true : pinfo.Date >= DateBegin) && (DateEnd == null ? true : pinfo.Date <= DateEnd)) &&
                ((ButtonPanelVisible && !pinfo.IsWrittenOff) || !ButtonPanelVisible)
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
                    ProductKind = (ProductKinds)pinfo.ProductKindID,
                    Quantity = pinfo.Quantity,
                    ShiftID = pinfo.ShiftID,
                    State = pinfo.State
                }
                );
            }
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
        private List<Object> _selectedPlaces = new List<Object>();
        public List<Object> SelectedPlaces // Object требует визуальный компонент
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
        public RelayCommand ChooseProductCommand { get; set; }
        private void ChooseProduct()
        {
            if (SelectedProduct == null) return;
            Messenger.Default.Send<ChoosenProductMessage>(new ChoosenProductMessage { ProductID = (Guid)SelectedProduct.ProductID });
            CloseWindow();
        }
        public RelayCommand ActivatedCommand { get; private set; }
        public RelayCommand DeactivatedCommand { get; private set; }
        private bool IsActive { get; set; }
        public RelayCommand OpenProductCommand { get; private set; }
        private void OpenProduct()
        {
            MessageManager.OpenDocProduct(
                SelectedProduct.ProductKind == ProductKinds.ProductSpool ? DocProductKinds.DocProductSpool : DocProductKinds.DocProductPallet, 
                SelectedProduct.ProductID);
        }
    }
}