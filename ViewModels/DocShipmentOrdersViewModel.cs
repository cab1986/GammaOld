using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Interfaces;
using Gamma.Models;

namespace Gamma.ViewModels
{
    class DocShipmentOrdersViewModel : RootViewModel, IItemManager
    {
        public DocShipmentOrdersViewModel(bool isOutOrders = true, GammaEntities gammaBase = null) : base(gammaBase)
        {
            IsOutOrders = isOutOrders;
            OpenDocShipmentOrderCommand = new DelegateCommand(OpenDocShipmentOrder, () => DB.HaveWriteAccess("DocShipmentOrderInfo"));
            Intervals = new List<string> { "Активные", "Последние 500", "Поиск" };
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

        private void Get1CDocShipmentOrders()
        {
            UIServices.SetBusyState();
            using (var db = DB.GammaDb)
            {
                db.Get1CDocShipmentOrders();
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
                            .OrderByDescending(d => d.Date).Take(500)
                            .Select(d => new DocShipmentOrder
                            {
                                DocShipmentOrderId = d.C1COrderID,
                                Number = d.Number,
                                Date = d.Date ?? DB.CurrentDateTime,
                                VehicleNumber = d.VehicleNumber,
                                Shipper = d.Shipper,
                                Consignee = d.Consignee,
                                Buyer = d.Buyer,
                                ActivePerson = IsOutOrders?d.OutActivePerson:d.InActivePerson,
                                OrderType = d.OrderType,
                                OutDate = d.OutDate
                }));
                        break;
                    case 1:
                        DocShipmentOrders = new ObservableCollection<DocShipmentOrder>(
                            gammaBase.v1COrders.Where(d =>
                            (gammaBase.Places.FirstOrDefault(p => p.PlaceID == WorkSession.PlaceID).Branches.C1CSubdivisionID == d.C1COutSubdivisionID && IsOutOrders) ||
                            (gammaBase.Places.FirstOrDefault(p => p.PlaceID == WorkSession.PlaceID).Branches.C1CSubdivisionID == d.C1CInSubdivisionID && !IsOutOrders))
                            .OrderByDescending(d => d.Date).Take(500)
                            .Select(d => new DocShipmentOrder
                            {
                                DocShipmentOrderId = d.C1COrderID,
                                Number = d.Number,
                                Date = d.Date ?? DB.CurrentDateTime,
                                VehicleNumber = d.VehicleNumber,
                                Shipper = d.Shipper,
                                Consignee = d.Consignee,
                                Buyer = d.Buyer,
                                ActivePerson = IsOutOrders ? d.OutActivePerson : d.InActivePerson,
                                OrderType = d.OrderType,
                                OutDate = d.OutDate
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
                                .OrderByDescending(d => d.Date).Take(500)
                                .Select(d => new DocShipmentOrder
                                {
                                    DocShipmentOrderId = d.C1COrderID,
                                    Number = d.Number,
                                    Date = d.Date ?? DB.CurrentDateTime,
                                    VehicleNumber = d.VehicleNumber,
                                    Shipper = d.Shipper,
                                    Consignee = d.Consignee,
                                    Buyer = d.Buyer,
                                    ActivePerson = IsOutOrders ? d.OutActivePerson : d.InActivePerson,
                                    OrderType = d.OrderType,
                                    OutDate = d.OutDate
                                }));
                        break;
                }
            }
            FillDocShipmentOrdersWithGoods(DocShipmentOrders);
        }

        private void FillDocShipmentOrdersWithGoods(IEnumerable<DocShipmentOrder> docShipmentOrders)
        {
            using (var gammaBase = DB.GammaDb)
            {
                foreach (var docShipmentOrder in docShipmentOrders)
                {
                    docShipmentOrder.DocShipmentOrderGoods = new ObservableCollection<DocNomenclatureItem>(gammaBase.v1COrderGoods
                        .Where(d => d.DocOrderID == docShipmentOrder.DocShipmentOrderId)
                        .Select(d => new DocNomenclatureItem()
                        {
                            NomenclatureName = d.NomenclatureName,
                            Quantity = IsOutOrders?d.Quantity:SqlFunctions.StringConvert(d.OutQuantity),
                            InQuantity = IsOutOrders?(d.OutQuantity??0):(d.InQuantity??0)
                        }));
                }
            }
        }

//        public List<Person> Persons { get; set; }
    }
}
