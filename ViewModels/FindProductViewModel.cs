﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using DevExpress.Mvvm;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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

        private FindProductViewModel()
        {
            Messenger.Default.Register<BarcodeMessage>(this, BarcodeReceived);
            ProductKindsList = Functions.EnumDescriptionsToList(typeof(ProductKind));
            ProductKindsList.Add("Бумага-основа");
            ProductKindsList.Add("Готовая продукция");
            ProductKindsList.Add("Все");
            States = Functions.EnumDescriptionsToList(typeof(ProductState));
            States.Add("Любое");
            SelectedStateIndex = States.Count - 1;
            ResetSearchCommand = new DelegateCommand(ResetSearch);
            FindCommand = new DelegateCommand(() => Find(false));
            ChooseProductCommand = new DelegateCommand(ChooseProduct, () => SelectedProduct != null);
            ChooseAllProductCommand = new DelegateCommand(ChooseAllProduct, () => ChooseAllProductEnabled && FoundProducts?.Count > 0);
            ActivatedCommand = new DelegateCommand(() => IsActive = true);
            DeactivatedCommand = new DelegateCommand(() => IsActive = false);
            OpenProductCommand = new DelegateCommand(OpenProduct, () => SelectedProduct != null);
            PlacesList = (from p in WorkSession.Places
                          select new
                          Place
                          {
                              PlaceName = p.Name,
                              PlaceID = p.PlaceID,
                              BranchID = p.BranchID,
                              PlaceGroupID = p.PlaceGroupID
                          }
                          ).OrderBy(p => p.BranchID == WorkSession.BranchID ? 0 : 1).ThenBy(p => p.PlaceGroupID)
                          .ToList();
        }

        public FindProductViewModel(FindProductMessage msg) : this()
        {
            ButtonPanelVisible = msg.ChooseProduct;
            if (msg.CurrentPlaces != null)
                msg.CurrentPlaces.ForEach(i => SelectedCurrentPlaces.Add(i));
            if (msg.ChooseProduct && msg.BatchKind == null && (int)msg.ProductKind >= 0) //для выбора по умолчанию пункта Все
                SelectedProductKindIndex = (int)msg.ProductKind;
            else
            {
                if (msg.BatchKind == null)
                    SelectedProductKindIndex = (ProductKindsList.Count - 1);
                else
                    switch (msg.BatchKind)
                    {
                        case BatchKinds.SGB:
                            SelectedProductKindIndex = (ProductKindsList.Count - 3);
                            break;
                        case BatchKinds.SGI:
                            SelectedProductKindIndex = (ProductKindsList.Count - 2);
                            break;
                    };
            }
            CurrentPlacesSelectEnabled = msg.AllowChangeCurrentPlaces;
            ProductKindSelectEnabled = msg.AllowChangeProductKind;
            ChooseAllProductEnabled = !msg.AllowChooseOneValueOnly;
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
/*            if (ButtonPanelVisible && FoundProducts.Count == 1)
            {
                SelectedProduct = FoundProducts.First();
                ChooseProduct();
            }*/
        }
        public DelegateCommand FindCommand { get; private set; }
        public DelegateCommand ResetSearchCommand { get; private set; }

        private async void Find(bool isFromScanner)
        {
            if (IsSearching) return;
            IsSearching = !IsSearching;
            Mouse.OverrideCursor = Cursors.Wait;
            await Task.Factory.StartNew(() => AsyncFind(isFromScanner)).ContinueWith((t) => Application.Current.Dispatcher.BeginInvoke(new Action(
                () =>
                {
                    Mouse.OverrideCursor = null;
                    IsSearching = false;
                })));
            if (FoundProducts.Count == 0 && ButtonPanelVisible)
            {
                if (SelectedCurrentPlaces?.Count() > 0)
                    MessageBox.Show("На выбранном переделе продукт не найден. Возможно находится на другом переделе или уже списан", "Продукт не найден");
                else
                    MessageBox.Show("Продукт уже списан или не существует в базе", "Продукт не найден");
            }
            if (FoundProducts.Count != 1 || !ButtonPanelVisible) return;
            SelectedProduct = FoundProducts.First();
            ChooseProduct();
        }

        private bool IsSearching { get; set; }


        private void AsyncFind(bool isFromScanner)
        {
            using (var gammaBase = DB.GammaDb)
            {
                if (NomenclatureID == null && SelectedCharacteristic?.CharacteristicID == null && 
                    DateBegin == null && DateEnd == null && (SelectedPlaces == null || SelectedPlaces?.Count == 0) && (SelectedCurrentPlaces == null || SelectedCurrentPlaces?.Count == 0) && SelectedStateIndex == States.Count - 1)
                {
                    FoundProducts = new ObservableCollection<ProductInfo>
                       (
                       gammaBase.Products.Where(pinfo =>
                           (Number == null || pinfo.Number.Contains(Number) || Number == "") &&
                            (Barcode == null || pinfo.BarCode == Barcode || Barcode == "") &&
                            (pinfo.ProductKindID == SelectedProductKindIndex ||
                            (SelectedProductKindIndex == ProductKindsList.Count - 3 && (pinfo.ProductKindID == (int)ProductKind.ProductSpool || pinfo.ProductKindID == (int)ProductKind.ProductGroupPack)) ||
                            (SelectedProductKindIndex == ProductKindsList.Count - 2 && (pinfo.ProductKindID == (int)ProductKind.ProductPallet || pinfo.ProductKindID == (int)ProductKind.ProductPalletR)) ||
                             SelectedProductKindIndex == ProductKindsList.Count - 1) &&
                            (!ButtonPanelVisible || (gammaBase.Rests.Any(gs => gs.ProductID == pinfo.ProductID && gs.Quantity >= 1)
                                ) ||
                            gammaBase.vGroupPackSpools.Where(gs => gs.ProductID == pinfo.ProductID)
                                .Join(gammaBase.vProductsInfo,
                                    gs => gs.ProductGroupPackID,
                                    pi => pi.ProductID,
                                    (gs, pi) => new { IsWrittenOff = pi.IsWrittenOff ?? false }
                                ).Any(j => !j.IsWrittenOff))
                           )
                           .Select(pinfo => new ProductInfo
                           {
                               //DocID = pinfo.DocID,
                               ProductID = pinfo.ProductID,
                               Number = pinfo.Number,
                               //CharacteristicID = pinfo.C1CCharacteristicID,
                               //Date = pinfo.Date,
                               //NomenclatureID = pinfo.C1CNomenclatureID,
                               //NomenclatureName = pinfo.NomenclatureName,
                               ProductKind = (ProductKind)pinfo.ProductKindID,
                               //Quantity = pinfo.Quantity,
                               // ShiftID = pinfo.ShiftID,
                               //State = pinfo.State,
                               //Place = pinfo.Place,
                               //PlaceGroup = (PlaceGroup)pinfo.PlaceGroupID,
                               IsWrittenOff = !(gammaBase.Rests.Any(gs => gs.ProductID == pinfo.ProductID && gs.Quantity >= 1)
                                ),
                               InGroupPack = gammaBase.vGroupPackSpools.Where(gs => gs.ProductID == pinfo.ProductID)
                                   .Join(gammaBase.vProductsInfo,
                                       gs => gs.ProductGroupPackID,
                                       pi => pi.ProductID,
                                       (gs, pi) => new { IsWrittenOff = pi.IsWrittenOff ?? false }
                                   ).Any(j => !j.IsWrittenOff)
                           })
                       );
                    if ((FoundProducts?.Count == 1 && !ButtonPanelVisible) || FoundProducts?.Count > 1)
                    {
                        var foundProductIDs = FoundProducts.Select(g => g.ProductID).ToList();
                        FoundProducts = new ObservableCollection<ProductInfo>
                            (
                            gammaBase.vProductsInfo.Where(pinfo =>
                                foundProductIDs.Contains(pinfo.ProductID)
                                ).Select(pinfo => new ProductInfo
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
                                    PlaceGroup = (PlaceGroup)pinfo.PlaceGroupID,
                                    IsWrittenOff = pinfo.IsWrittenOff ?? false,
                                    CurrentPlace = pinfo.CurrentPlace,
                                    InGroupPack = gammaBase.vGroupPackSpools.Where(gs => gs.ProductID == pinfo.ProductID)
                                        .Join(gammaBase.vProductsInfo,
                                            gs => gs.ProductGroupPackID,
                                            pi => pi.ProductID,
                                            (gs, pi) => new { IsWrittenOff = pi.IsWrittenOff ?? false }
                                        ).Any(j => !j.IsWrittenOff)
                                })
                            );
                    }
                }
                else
                {
                    var charId = SelectedCharacteristic?.CharacteristicID ?? new Guid();
                    var selectedPlaces = new List<int>();
                    if (SelectedPlaces != null)
                        selectedPlaces = SelectedPlaces.Cast<int>().ToList();
                    var selectedCurrentPlaces = new List<int>();
                    if (SelectedCurrentPlaces != null)
                        selectedCurrentPlaces = SelectedCurrentPlaces.Cast<int>().ToList();

                    FoundProducts = new ObservableCollection<ProductInfo>(
                        gammaBase.vProductsInfo.Where(pinfo =>
                            (Number == null || pinfo.Number.Contains(Number) || Number == "") &&
                            (Barcode == null || pinfo.BarCode == Barcode || Barcode == "") &&
                            (NomenclatureID == null || pinfo.C1CNomenclatureID == NomenclatureID) &&
                            (charId == new Guid() || pinfo.C1CCharacteristicID == charId) &&
                            (pinfo.ProductKindID == SelectedProductKindIndex ||
                            (SelectedProductKindIndex == ProductKindsList.Count - 3 && (pinfo.ProductKindID == (int)ProductKind.ProductSpool || pinfo.ProductKindID == (int)ProductKind.ProductGroupPack)) ||
                            (SelectedProductKindIndex == ProductKindsList.Count - 2 && (pinfo.ProductKindID == (int)ProductKind.ProductPallet || pinfo.ProductKindID == (int)ProductKind.ProductPalletR)) ||
                             SelectedProductKindIndex == ProductKindsList.Count - 1) &&
                            ((DateBegin == null || pinfo.Date >= DateBegin) &&
                             (DateEnd == null || pinfo.Date <= DateEnd)) &&
                            (!ButtonPanelVisible || !(pinfo.IsWrittenOff ?? false) ||
                             gammaBase.vGroupPackSpools.Where(gs => gs.ProductID == pinfo.ProductID)
                                 .Join(gammaBase.vProductsInfo,
                                     gs => gs.ProductGroupPackID,
                                     pi => pi.ProductID,
                                     (gs, pi) => new { IsWrittenOff = pi.IsWrittenOff ?? false }
                                 ).Any(j => !j.IsWrittenOff)) &&
                            (selectedPlaces.Count == 0 || selectedPlaces.Contains((int)pinfo.PlaceID)) &&
                            (selectedCurrentPlaces.Count == 0 || selectedCurrentPlaces.Contains((int)pinfo.CurrentPlaceID) ||
                             gammaBase.vGroupPackSpools.Where(gs => gs.ProductID == pinfo.ProductID)
                                 .Join(gammaBase.vProductsInfo,
                                     gs => gs.ProductGroupPackID,
                                     pi => pi.ProductID,
                                     (gs, pi) => new { CurrentPlaceID = pi.CurrentPlaceID }
                                 ).Any(j => selectedCurrentPlaces.Contains((int)j.CurrentPlaceID))) &&
                            (SelectedStateIndex == States.Count - 1 || SelectedStateIndex == pinfo.StateID)
                            ).Select(pinfo => new ProductInfo
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
                                PlaceGroup = (PlaceGroup)pinfo.PlaceGroupID,
                                IsWrittenOff = pinfo.IsWrittenOff ?? false,
                                CurrentPlace = pinfo.CurrentPlace,
                                InGroupPack = gammaBase.vGroupPackSpools.Where(gs => gs.ProductID == pinfo.ProductID)
                                    .Join(gammaBase.vProductsInfo,
                                        gs => gs.ProductGroupPackID,
                                        pi => pi.ProductID,
                                        (gs, pi) => new { IsWrittenOff = pi.IsWrittenOff ?? false }
                                    ).Any(j => !j.IsWrittenOff)
                            })
                        );
                }
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
            if (SelectedCurrentPlaces != null) SelectedCurrentPlaces = null;
            SelectedStateIndex = States.Count - 1;
        }      
        private int _selectedProductKindIndex;
        public int SelectedProductKindIndex
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
        private List<object> _selectedCurrentPlaces = new List<object>();
        public List<object> SelectedCurrentPlaces // Object требует визуальный компонент
        {
            get
            {
                return _selectedCurrentPlaces;
            }
            set
            {
                _selectedCurrentPlaces = value;
                RaisePropertyChanged("SelectedCurrentPlaces");
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

        private bool ChooseAllProductEnabled { get; set; }
        public bool CurrentPlacesSelectEnabled { get; private set; } = true;
        private DateTime? _dateBegin;
        private DateTime? _dateEnd;

        public DelegateCommand ChooseProductCommand { get; set; }

        public DelegateCommand ChooseAllProductCommand { get; set; }

        /// <summary>
        /// Распаковка упаковки, в которой находится рулон
        /// </summary>
        /// <param name="productId">ИД рулона</param>
        /// <returns></returns>
        private bool UnpackGroupPack(Guid productId)
        {
            using (var gammaBase = DB.GammaDb)
            {
                var groupPack = gammaBase.vGroupPackSpools.Where(gs => gs.ProductID == productId).Join(gammaBase.Products,
                            gs => gs.ProductGroupPackID,
                            p => p.ProductID,
                            (gs, p) => new { ProductId = p.ProductID, Number = p.Number }).Join(gammaBase.Rests,
                            p => p.ProductId,
                            r => r.ProductID,
                            (p, r) => new { ProductId = r.ProductID, Number = p.Number }).FirstOrDefault();
                if (groupPack == null)
                {
                    MessageBox.Show("Выбранный рулон в упаковке, но при поиске упаковки произошел сбой");
                    return false;
                }
                var dlgResult =
                    MessageBox.Show(
                        $"Выбранный тамбур находится в упаковке {groupPack.Number}. Данная упаковка будет распакована. Вы согласны?",
                        "Распаковка", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dlgResult != MessageBoxResult.Yes) return false;
                gammaBase.UnpackGroupPack(groupPack.ProductId, WorkSession.PrintName);
                MessageBox.Show($"Упаковка {groupPack.Number} уничтожена");
                return true;
            }
        }

        private void ChooseProduct()
        {
            if (SelectedProduct == null) return;
            if (SelectedProduct.InGroupPack)
            {
                if (!UnpackGroupPack(SelectedProduct.ProductID)) return;
            }
            Messenger.Default.Send(new ChoosenProductMessage { ProductID = SelectedProduct.ProductID });
            CloseWindow();
        }

        private void ChooseAllProduct()
        {
            if (FoundProducts == null) return;
            if (FoundProducts.Count == 0)
            {
                return;
            }
            Messenger.Default.Send(new ChoosenProductMessage { ProductIDs = FoundProducts.Select(p => p.ProductID).ToList() });
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
                case (ProductKind.ProductPalletR):
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductPalletR, SelectedProduct.ProductID);
                    break;
            }
        }
    }
}