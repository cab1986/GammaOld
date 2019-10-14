// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.SqlServer;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Interfaces;
using Gamma.Models;

namespace Gamma.ViewModels
{
    class DocShipmentOrdersViewModel : RootViewModel, IItemManager
    {
        public DocShipmentOrdersViewModel(bool isOutOrders = true)
        {
            IsOutOrders = isOutOrders;
            OpenDocShipmentOrderCommand = new DelegateCommand(OpenDocShipmentOrder, () => DB.HaveReadAccess("DocShipmentOrderInfo"));
            Intervals = new List<string> { "Активные", "Последние 300", "Поиск" };
            FindCommand = new DelegateCommand(Find);
            RefreshCommand = FindCommand;
            EditItemCommand = OpenDocShipmentOrderCommand;
            Get1CDocShipmentOrdersCommand = new DelegateCommand(Get1CDocShipmentOrders);
 //           CanChangePerson = DB.HaveWriteAccess("DocShipmentOrderInfo");
/*            Persons = GammaBase.Persons.Where(p => p.PostTypeID == (int) PersonTypes.Loader).Select(p => new Person
            {
                PersonId = p.PersonID,
                Name = p.Name
            }).ToList();
*/
            IntervalId = 0;
            Find();
        }

        private void OpenDocShipmentOrder()
        {
            UIServices.SetBusyState();
            if (SelectedDocShipmentOrder == null) return;
            var docShipmentOrderId = SelectedDocShipmentOrder.DocShipmentOrderId; //(row as DocShipmentOrder)?.DocShipmentOrderId;
//            if (docShipmentOrderId == null) return;
            MessageManager.OpenDocShipmentOrder((Guid)docShipmentOrderId);
        }

        public DelegateCommand DeleteItemCommand { get; private set; }

        public DelegateCommand<object> EditItemCommand { get; private set; }

        public DelegateCommand NewItemCommand { get; private set; }

        public DelegateCommand RefreshCommand { get; private set; }

        public DelegateCommand FindCommand { get; private set; }

        public string Number { get; set; }
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public List<string> Intervals { get; private set; }
        private int _intervalId;

        /// <summary>
        /// Отгрузки или заказы, по которым надо принять
        /// </summary>
        private bool IsOutOrders { get; set; }

        public int IntervalId
        {
            get { return _intervalId; }
            set
            {
                if (_intervalId == value) return;
                _intervalId = value;
                if (_intervalId < 2) Find();
            }
        }

//        public bool CanChangePerson { get; set; }

        private ObservableCollection<DocShipmentOrder> _docShipmentOrders;

        public ObservableCollection<DocShipmentOrder> DocShipmentOrders
        {
            get { return _docShipmentOrders; }
            set
            {
                _docShipmentOrders = value;
                RaisePropertyChanged("DocShipmentOrders");
            }
        }

        public DocShipmentOrder SelectedDocShipmentOrder { get; set; }

        public DelegateCommand OpenDocShipmentOrderCommand { get; private set; }

        public DelegateCommand Get1CDocShipmentOrdersCommand { get; private set; }


        /// <summary>
        /// Принудительная выгрузка приказов из 1С
        /// </summary>
        private void Get1CDocShipmentOrders()
        {
            UIServices.SetBusyState();
            using (var gammaBase = DB.GammaDb)
            {
                gammaBase.Get1CDocShipmentOrders();
            }
            Find();
        }

        private void Find()
        {
            UIServices.SetBusyState();
            SelectedDocShipmentOrder = null;
            using (var gammaBase = DB.GammaDb)
            {                
                switch (IntervalId)
                {
                    case 0:
                        DocShipmentOrders = new ObservableCollection<DocShipmentOrder>(
                            gammaBase.v1COrders.Where(d => ((!d.IsShipped && IsOutOrders) || (!(d.IsConfirmed??false) && !IsOutOrders)) &&
                            ((gammaBase.Places.FirstOrDefault(p => p.PlaceID == WorkSession.PlaceID).Branches.C1CSubdivisionID == d.C1COutSubdivisionID && IsOutOrders) ||
                            ((gammaBase.Places.FirstOrDefault(p => p.PlaceID == WorkSession.PlaceID).Branches.C1CSubdivisionID == d.C1CInSubdivisionID && !IsOutOrders))))
                            .OrderByDescending(d => d.Date).Take(300)
                            .Select(d => new DocShipmentOrder
                            {
                                DocShipmentOrderId = d.C1COrderID,
                                Number = d.Number,
                                Date = d.Date ?? DB.CurrentDateTime,
                                VehicleNumber = d.VehicleNumber,
                                Shipper = d.Shipper,
                                Consignee = d.Consignee,
                                Buyer = d.Buyer,
                                ActivePerson = IsOutOrders?d.OutActivePersons:d.InActivePersons,
                                OrderType = d.OrderType,
                                OutDate = d.OutDate,
                                Warehouse = d.Warehouse ?? ""
                            }));
                        break;
                    case 1:
                        DocShipmentOrders = new ObservableCollection<DocShipmentOrder>(
                            gammaBase.v1COrders.Where(d =>
                            (gammaBase.Places.FirstOrDefault(p => p.PlaceID == WorkSession.PlaceID).Branches.C1CSubdivisionID == d.C1COutSubdivisionID && IsOutOrders) ||
                            (gammaBase.Places.FirstOrDefault(p => p.PlaceID == WorkSession.PlaceID).Branches.C1CSubdivisionID == d.C1CInSubdivisionID && !IsOutOrders))
                            .OrderByDescending(d => d.Date).Take(300)
                            .Select(d => new DocShipmentOrder
                            {
                                DocShipmentOrderId = d.C1COrderID,
                                Number = d.Number,
                                Date = d.Date ?? DB.CurrentDateTime,
                                VehicleNumber = d.VehicleNumber,
                                Shipper = d.Shipper,
                                Consignee = d.Consignee,
                                Buyer = d.Buyer,
                                ActivePerson = IsOutOrders ? d.OutActivePersons : d.InActivePersons,
                                OrderType = d.OrderType,
                                OutDate = d.OutDate,
                                Warehouse = d.Warehouse ?? ""
                            }));
                        break;
                    case 2:
                        DocShipmentOrders = new ObservableCollection<DocShipmentOrder>(
                            gammaBase.v1COrders.Where(d =>
                                (Number == null || d.Number.Contains(Number)) &&
                                (d.Date >= DateBegin || DateBegin == null) &&
                                (d.Date <= DateEnd || DateEnd == null) &&
                                (
                                    (gammaBase.Places.FirstOrDefault(p => p.PlaceID == WorkSession.PlaceID).Branches.C1CSubdivisionID == d.C1COutSubdivisionID && IsOutOrders)
                                    ||
                                    (gammaBase.Places.FirstOrDefault(p => p.PlaceID == WorkSession.PlaceID).Branches.C1CSubdivisionID == d.C1CInSubdivisionID && !IsOutOrders)
                                ))
                                .OrderByDescending(d => d.Date).Take(300)
                                .Select(d => new DocShipmentOrder
                                {
                                    DocShipmentOrderId = d.C1COrderID,
                                    Number = d.Number,
                                    Date = d.Date ?? DB.CurrentDateTime,
                                    VehicleNumber = d.VehicleNumber,
                                    Shipper = d.Shipper,
                                    Consignee = d.Consignee,
                                    Buyer = d.Buyer,
                                    ActivePerson = IsOutOrders ? d.OutActivePersons : d.InActivePersons,
                                    OrderType = d.OrderType,
                                    OutDate = d.OutDate,
                                    Warehouse = d.Warehouse ?? ""
                                }));
                        break;
                }
            }
            //FillDocShipmentOrdersWithGoods(DocShipmentOrders);
        }

        private void FillDocShipmentOrdersWithGoods(IEnumerable<DocShipmentOrder> docShipmentOrders)
        {
            using (var gammaBase = DB.GammaDb)
            {
                var orderIds = docShipmentOrders.Select(d => d.DocShipmentOrderId).ToList();
                var shipmentOrderGoods = gammaBase.v1COrderGoods
                    .Where(d => d.DocOrderID != null && orderIds.Contains((Guid)d.DocOrderID))
                    .Select(d => new
                    {
                        d.DocOrderID,
                        d.NomenclatureName,
                        Quantity = IsOutOrders ? d.Quantity : SqlFunctions.StringConvert(d.OutQuantity),
                        InQuantity = IsOutOrders ? (d.OutQuantity ?? 0) : (d.InQuantity ?? 0),
                        d.Quality
                    }).ToList();
                foreach (var docShipmentOrder in docShipmentOrders)
                {
                    docShipmentOrder.DocShipmentOrderGoods = new ObservableCollection<DocNomenclatureItem>(
                        shipmentOrderGoods.Where(g => g.DocOrderID == docShipmentOrder.DocShipmentOrderId)
                        .Select(g => new DocNomenclatureItem
                        {
                            NomenclatureName = g.NomenclatureName,
                            Quantity = g.Quantity,
                            InQuantity = g.InQuantity,
                            Quality = g.Quality
                        }));
                }
            }
        }

//        public List<Person> Persons { get; set; }
    }
}
