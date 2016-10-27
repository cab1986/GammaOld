using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Interfaces;
using Gamma.Models;
using System.Globalization;

namespace Gamma.ViewModels
{
    class DocMovementOrdersViewModel : RootViewModel, IItemManager
    {
        public DocMovementOrdersViewModel()
        {
            using (var gammaBase = DB.GammaDb)
            {
                Intervals = new List<string> { "Последние 500", "Поиск" };
                IntervalId = 0;
                ResetSearchCommand = new DelegateCommand(ResetSearch);
                EditItemCommand = new DelegateCommand(() => MessageManager.OpenDocMovementOrder(SelectedDocMovementOrderItem.DocId), () => SelectedDocMovementOrderItem != null);
                NewItemCommand = new DelegateCommand(CreateNewDocMovementOrder);
                RefreshCommand = new DelegateCommand(Find);
                Warehouses = gammaBase.Places.Where(p => p.BranchID == WorkSession.BranchID && (p.IsWarehouse ?? false))
                   .Select(p => new Place
                   {
                       PlaceID = p.PlaceID,
                       PlaceGuid = p.PlaceGuid,
                       PlaceName = p.Name
                   }).ToList();
            }
            Find();
        }

        private ObservableCollection<MovementItem> _docMovementOrderItems;
        private DateTime? _filterDateBegin;
        private DateTime? _filterDateEnd;
        private string _filterNumber;
        private int? _filterPlaceTo;
        private int? _filterPlaceFrom;

        public ObservableCollection<MovementItem> DocMovementOrderItems
        {
            get { return _docMovementOrderItems; }
            set
            {
                _docMovementOrderItems = value;
                RaisePropertyChanged("DocMovementOrderItems");
            }
        }

        public int IntervalId { get; set; }


        public DateTime? FilterDateBegin
        {
            get { return _filterDateBegin; }
            set
            {
                _filterDateBegin = value;
                RaisePropertyChanged("FilterDateBegin");
            }
        }

        public DateTime? FilterDateEnd
        {
            get { return _filterDateEnd; }
            set
            {
                _filterDateEnd = value;
                RaisePropertyChanged("FilterDateEnd");
            }
        }

        public string FilterNumber
        {
            get { return _filterNumber; }
            set
            {
                _filterNumber = value;
                RaisePropertyChanged("FilterNumber");
            }
        }

        public int? FilterPlaceTo
        {
            get { return _filterPlaceTo; }
            set
            {
                _filterPlaceTo = value;
                RaisePropertyChanged("FilterPlaceTo");
            }
        }

        public int? FilterPlaceFrom
        {
            get { return _filterPlaceFrom; }
            set
            {
                _filterPlaceFrom = value;
                RaisePropertyChanged("FilterPlaceFrom");
            }
        }

        public List<Place> Warehouses { get; private set; }

        public List<string> Intervals { get; private set; }

        public MovementItem SelectedDocMovementOrderItem { get; set; }

        private void CreateNewDocMovementOrder()
        {
            MessageManager.OpenDocMovementOrder();
        }

        private void Find()
        {
            UIServices.SetBusyState();
            SelectedDocMovementOrderItem = null;
            using (var gammaBase = DB.GammaDb)
            {
                switch (IntervalId)
                {
                    case 0:
                        DocMovementOrderItems = new ObservableCollection<MovementItem>(
                            gammaBase.DocMovementOrder.OrderByDescending(d => d.Docs.Date).Take(500)
                            .Select(d => new MovementItem
                            {
                                DocId = d.Docs.DocID,
                                Date = d.Docs.Date,
                                Number = d.Docs.Number,
                                PlaceFrom = d.PlacesFrom.Name,
                                PlaceTo = d.PlacesTo.Name
                            }));
                        break;
                    case 1:
                        DocMovementOrderItems = new ObservableCollection<MovementItem>(
                            gammaBase.DocMovementOrder
                            .Where(d =>
                                (FilterPlaceTo == null || d.InPlaceID == FilterPlaceTo) &&
                                (FilterPlaceFrom == null || d.OutPlaceID == FilterPlaceFrom) &&
                                (FilterDateBegin == null || d.Docs.Date >= FilterDateBegin) &&
                                (FilterDateEnd == null || d.Docs.Date <= FilterDateEnd) &&
                                (FilterNumber == null || d.Docs.Number.Contains(FilterNumber)) 
                                ).OrderByDescending(d => d.Docs.Date).Take(500)
                                .Select(d => new MovementItem
                                {
                                    DocId = d.Docs.DocID,
                                    Date = d.Docs.Date,
                                    Number = d.Docs.Number,
                                    PlaceFrom = d.PlacesFrom.Name,
                                    PlaceTo = d.PlacesTo.Name
                                }));
                        break;
                }
                foreach (var docMovementOrderItem in DocMovementOrderItems)
                {
                    docMovementOrderItem.NomenclatureItems = new ObservableCollection<DocNomenclatureItem>(
                        gammaBase.GetDocMovementOrderNomenclature(docMovementOrderItem.DocId).Select(
                            d => new DocNomenclatureItem
                            {
                                NomenclatureName = d.NomenclatureName,
                                Quantity = (d.Quantity ?? 0).ToString(CultureInfo.InvariantCulture),
                                OutQuantity = d.OutQuantity??0,
                                InQuantity = d.InQuantity ?? 0
                            }));
                }
            }
        }



        private void ResetSearch()
        {
            FilterNumber = null;
            FilterDateEnd = null;
            FilterDateBegin = null;
            FilterPlaceFrom = null;
            FilterPlaceTo = null;
        }

        public DelegateCommand ResetSearchCommand { get; }
        public DelegateCommand NewItemCommand { get; }
        public DelegateCommand<object> EditItemCommand { get; }
        public DelegateCommand DeleteItemCommand { get; }
        public DelegateCommand RefreshCommand { get; }
    }
}
