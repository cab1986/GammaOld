﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
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
            Get1CDocShipmentOrdersCommand = new DelegateCommand(Get1CDocShipmentOrders, () => false);
 //           CanChangePerson = DB.HaveWriteAccess("DocShipmentOrderInfo");
/*            Persons = GammaBase.Persons.Where(p => p.PostTypeID == (int) PersonTypes.Loader).Select(p => new Person
            {
                PersonId = p.PersonID,
                Name = p.Name
            }).ToList();
*/
            DateBegin = DateTime.Now.AddMonths(-6);
            IntervalId = 2;
            Find();
        }

        private void OpenDocShipmentOrder()
        {
            WorkSession.CheckExistNewVersionOfProgram();
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

        private string _number;
        public string Number
        {
            get { return _number; }
            set
            {
                if (_number == value) return;
                _number = value;
                //Find();
                RaisePropertyChanged("Number");
            }
        }
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
                if (_intervalId < 2)
                {
                    Number = null;
                    Find();
                }
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
            WorkSession.CheckExistNewVersionOfProgram();
            UIServices.SetBusyState();
            SelectedDocShipmentOrder = null;
            using (var gammaBase = DB.GammaDb)
            {                
                switch (IntervalId)
                {
                    case 0:
                        DocShipmentOrders = new ObservableCollection<DocShipmentOrder>(
                            gammaBase.Get1COrders(WorkSession.PlaceID, "%" + Number + "%", DateBegin, DateEnd, IsOutOrders, 300)
                            .OrderByDescending(d => d.Date).Take(300)
                            .Where(d => (!d.IsShipped && IsOutOrders) || (!(d.IsConfirmed ?? false) && !IsOutOrders))
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
                                Warehouse = d.Warehouse ?? "",
                                IsConfirmed = d.IsShipped,
                                IsReturned = d.IsReturned,
                                LastUploadedTo1C = d.LastUploadedTo1C
                            }));
                        break;
                    case 1:
                        DocShipmentOrders = new ObservableCollection<DocShipmentOrder>(
                            gammaBase.Get1COrders(WorkSession.PlaceID, "%"+Number+"%" , DateBegin, DateEnd, IsOutOrders,300)
                            .OrderByDescending(d => d.Date)//.Take(300)
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
                                Warehouse = d.Warehouse ?? "",
                                IsConfirmed = d.IsShipped,
                                IsReturned = d.IsReturned,
                                LastUploadedTo1C = d.LastUploadedTo1C
                            }));
                        break;
                    case 2:
                        {
                            DocShipmentOrders = new ObservableCollection<DocShipmentOrder>(
                                gammaBase.Get1COrders(WorkSession.PlaceID, "%" + Number, DateBegin, DateEnd, IsOutOrders, 300)
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
                                        Warehouse = d.Warehouse ?? "",
                                        IsConfirmed = d.IsShipped,
                                        IsReturned = d.IsReturned,
                                        LastUploadedTo1C = d.LastUploadedTo1C
                                    }));
                            break;
                        }
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
    }
}
