﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Interfaces;
using Gamma.Models;
using System.Windows;
using System.Data.Entity.SqlServer;

namespace Gamma.ViewModels
{
    public class StockRemaindersViewModel : SaveImplementedViewModel, IItemManager
    {
        public StockRemaindersViewModel()
        {
            Intervals = new List<string> {"Нет", "Да"};
            ProductKindsList = Functions.EnumDescriptionsToList(typeof(ProductKind));
            ProductKindsList.Add("Все");
            SelectedProductKindIndex = ProductKindsList.Count - 1;
            States = Functions.EnumDescriptionsToList(typeof(ProductState));
            States.Add("Любое");
            SelectedStateIndex = States.Count - 1;
            RefreshCommand = new DelegateCommand(Find);
            NewItemCommand = new DelegateCommand(() => OpenStockRemainder());
            EditItemCommand = new DelegateCommand(() => OpenStockRemainder(SelectedStockRemainder.DocID), SelectedStockRemainder != null);
            DeleteItemCommand = new DelegateCommand(DeleteItem);
            using (var gammaBase = DB.GammaDb)
            {
                Places = gammaBase.Places.Where(
                    p => WorkSession.BranchIds.Contains(p.BranchID) && ( (p.IsShipmentWarehouse ?? false) || (p.IsTransitWarehouse ?? false)))
                    .Select(p => new Place()
                    {
                        PlaceID = p.PlaceID,
                        PlaceGuid = p.PlaceGuid,
                        PlaceName = p.Name
                    }).ToList();
            }
            Places.Insert(0, new Place() { PlaceName = "Все" });
            var places = Places.Select(pl => pl.PlaceID).ToList();
            using (var gammaBase = DB.GammaDb)
            {
                placeZones = gammaBase.PlaceZones.Where(
                    p => places.Contains(p.PlaceID))
                    .Select(p => new PlaceZone()
                    {
                        PlaceId = p.PlaceID,
                        PlaceZoneId = p.PlaceZoneID,
                        Name = p.Name
                    }).ToList();
            }
            placeZones.Insert(0, new PlaceZone() { Name = "Все" });

            PlaceId = 0;
            IntervalId = 1;
            //Find();
        }

        private List<ProductInfo> _StockRemaindersList;

        public List<ProductInfo> StockRemaindersList
        {
            get { return _StockRemaindersList; }
            set
            {
                _StockRemaindersList = value;
                RaisePropertyChanged("StockRemaindersList");
            }
        }

        public void Find()
        {
            UIServices.SetBusyState();
            SelectedStockRemainder = null;
            using (var gammaBase = DB.GammaDb)
            {
                var placeIDs = Places?.Select(p => p.PlaceID).ToList();
                switch (IntervalId)
                {
                    case 0:
                        StockRemaindersList = gammaBase.vProductsInfo
                            .Where(d =>
                             d.CurrentPlaceID != null && d.CurrentPlaceID != 33 &&
                             (PlaceId == 0 ? placeIDs.Contains(d.CurrentPlaceID ?? 0) : PlaceId == d.CurrentPlaceID)  &&
                             (PlaceZoneId == null || PlaceZoneId == d.CurrentPlaceZoneID) &&
                             (d.ProductKindID == SelectedProductKindIndex ||
                             SelectedProductKindIndex == ProductKindsList.Count - 1) &&
                             (SelectedStateIndex == States.Count - 1 || SelectedStateIndex == d.StateID) &&
                             ((DateBegin == null || d.Date >= DateBegin) &&
                             (DateEnd == null || d.Date <= DateEnd))
                            )
                            .OrderByDescending(d => d.Date)
                            .Select(d => new ProductInfo
                            {
                                DocID = d.DocID,
                                Number = d.Number,
                                Date = d.Date,
                                ShiftID = d.ShiftID ?? 0,
                                CurrentPlace = d.CurrentPlace,
                                State = d.State,
                                NomenclatureName = d.NomenclatureName,
                                IsConfirmed = d.IsConfirmed,
                                Count = 1,
                                Quantity = d.Quantity
                            }).ToList();
                        break;
                    case 1:
                    case 2:
                        StockRemaindersList = gammaBase.vProductsInfo
                        .Where(d =>
                       d.CurrentPlaceID != null && d.CurrentPlaceID != 33 &&
                            (PlaceId == 0 ? placeIDs.Contains(d.CurrentPlaceID ?? 0) : PlaceId == d.CurrentPlaceID) &&
                            (PlaceZoneId == null || PlaceZoneId == d.CurrentPlaceZoneID) &&
                            (d.ProductKindID == SelectedProductKindIndex ||
                            SelectedProductKindIndex == ProductKindsList.Count - 1) &&
                            (SelectedStateIndex == States.Count - 1 || SelectedStateIndex == d.StateID) &&
                            ((DateBegin == null || d.Date >= DateBegin) &&
                            (DateEnd == null || d.Date <= DateEnd))
                             )
                        .GroupBy(d => new { d.CurrentPlace, d.NomenclatureName, d.State, d.IsConfirmed })
                        .OrderByDescending(d => d.Key.NomenclatureName)
                        .Select(d => new ProductInfo
                        {
                            DocID = Guid.Empty,
                            Number = null,
                            Date = null,
                            ShiftID = null,
                            CurrentPlace = d.Key.CurrentPlace,
                            State = d.Key.State,
                            NomenclatureName = d.Key.NomenclatureName,
                            IsConfirmed = d.Key.IsConfirmed,
                            Count = d.Count(),
                            Quantity = d.Sum(x => x.Quantity)
                        }).ToList();
                        break;
                }
            }
        }

        private int _intervalId;

        public int IntervalId
        {
            get { return _intervalId; }
            set
            {
                if (_intervalId == value) return;
                _intervalId = value;
                switch (value)
                {
                    case 0:
                        NomenclatureNameGroupIndex = 0;
                        PlaceGroupIndex = 1;
                        IsVisibleWithGroup = true;
                        break;
                    case 1:
                        NomenclatureNameGroupIndex = 0;
                        PlaceGroupIndex = 1;
                        IsVisibleWithGroup = false;
                        break;
                    case 2:
                        NomenclatureNameGroupIndex = 1;
                        PlaceGroupIndex = 0;
                        IsVisibleWithGroup = false;
                        break;
                }
                RaisePropertyChanged("NomenclatureNameGroupIndex");
                RaisePropertyChanged("PlaceGroupIndex");
                //if (_intervalId < 2) Find();
            }
        }

        public int? NomenclatureNameGroupIndex { get; set; }
        public int? PlaceGroupIndex { get; set; }

        
        private bool _isVisibleWithGroup;
        public bool IsVisibleWithGroup
        {
            get
            {
                return _isVisibleWithGroup;
            }
            set
            {
                _isVisibleWithGroup = value;
                RaisePropertyChanged("IsVisibleWithGroup");
            }
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


        public ProductInfo SelectedStockRemainder { get; set; }

        public List<string> Intervals { get; private set; }

        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public string Number { get; set; }

        private int? _placeId { get; set; }

        public int? PlaceId
        {
            get
            {
                return _placeId;
            }
            set
            {
                _placeId = value;
                PlaceZoneId = null;
                RaisePropertyChanged("PlaceZones");
                RaisePropertyChanged("PlaceZoneId");
            }
        }


        public List<Place> Places { get; set; }
        private List<PlaceZone> placeZones { get; set; }
        public List<PlaceZone> PlaceZones =>
        placeZones.Where(pl => pl.PlaceId == PlaceId).ToList(); 
        
        public Guid? PlaceZoneId { get; set; }

        public DelegateCommand DeleteItemCommand { get; }
        public DelegateCommand<object> EditItemCommand { get; private set; }
        public DelegateCommand NewItemCommand { get; }
        public DelegateCommand RefreshCommand { get; private set; }

        private void OpenStockRemainder(Guid? docID = null)
        {
            UIServices.SetBusyState();
            Messenger.Default.Register<RefreshMessage>(this, Find);
            if (docID == null)
                MessageManager.OpenDocMaterialProduction(SqlGuidUtil.NewSequentialid());
            else
            {
                MessageManager.OpenDocMaterialProduction((Guid)docID);
            }
        }

        private void Find(RefreshMessage msg)
        {
            Find();
            Messenger.Default.Unregister<RefreshMessage>(this, Find);
        }

        private void DeleteItem()
        {
            if (SelectedStockRemainder == null) return;
            var deleteItem = GammaBase.Docs.FirstOrDefault(d => d.DocID == SelectedStockRemainder.DocID);
            if (deleteItem != null)
            {
                var delResult = GammaBase.Docs.Remove(deleteItem);
                GammaBase.SaveChanges();
                if (delResult != null)
                {
                    Find();
                    return;
                }
                MessageBox.Show(delResult.Number + delResult.Date.ToString(), "Не удалось удалить", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Удаление","Не выбрана запись. Удалить не удалось.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
    }
}