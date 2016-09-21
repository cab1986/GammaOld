using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Interfaces;
using Gamma.Models;

namespace Gamma.ViewModels
{
    class DocShipmentOrdersViewModel : RootViewModel, IItemManager
    {
        public DocShipmentOrdersViewModel(GammaEntities gammaBase = null) : base(gammaBase)
        {
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

        // ReSharper disable once MemberCanBePrivate.Global
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
                            gammaBase.C1CDocShipmentOrder.Where(d => (d.DocShipmentOrderInfo == null || (d.DocShipmentOrderInfo.IsShipped == null || !(bool)d.DocShipmentOrderInfo.IsShipped)) && 
                            gammaBase.Places.FirstOrDefault(p => p.PlaceID == WorkSession.PlaceID).Branches.C1CSubdivisionID == d.C1CWarehouses.C1CSubdivisionID)
                            .OrderByDescending(d => d.C1CDate).Take(500)
                            .Select(d => new DocShipmentOrder
                            {
                                DocShipmentOrderId = d.C1CDocShipmentOrderID,
                                Number = d.C1CNumber,
                                Date = d.C1CDate ?? DB.CurrentDateTime,
                                VehicleNumber = d.DocShipmentOrderInfo.VehicleNumber,
                                Consignee = d.C1CConsignees.Description,
                                ActivePerson = d.DocShipmentOrderInfo.Persons.Name,
                            }));
                        break;
                    case 1:
                        DocShipmentOrders = new ObservableCollection<DocShipmentOrder>(
                            gammaBase.C1CDocShipmentOrder
                            .Where(d => gammaBase.Places.FirstOrDefault(p => p.PlaceID == WorkSession.PlaceID).Branches.C1CSubdivisionID == d.C1CWarehouses.C1CSubdivisionID)
                            .OrderByDescending(d => d.C1CDate).Take(500)
                            .Select(d => new DocShipmentOrder
                            {
                                DocShipmentOrderId = d.C1CDocShipmentOrderID,
                                Number = d.C1CNumber,
                                Date = d.C1CDate ?? DB.CurrentDateTime,
                                Consignee = d.C1CConsignees.Description,
                                ActivePerson = d.DocShipmentOrderInfo.Persons.Name,
                            }));
                        break;
                    case 2:
                        DocShipmentOrders = new ObservableCollection<DocShipmentOrder>(
                            gammaBase.C1CDocShipmentOrder.Where(d =>
                                (Number == null || d.C1CNumber.Contains(Number)) &&
                                (d.C1CDate >= DateBegin || DateBegin == null) &&
                                (d.C1CDate <= DateEnd || DateEnd == null) &&
                                gammaBase.Places.FirstOrDefault(p => p.PlaceID == WorkSession.PlaceID).Branches.C1CSubdivisionID == d.C1CWarehouses.C1CSubdivisionID)
                                .OrderByDescending(d => d.C1CDate).Take(500)
                                .Select(d => new DocShipmentOrder
                                {
                                    DocShipmentOrderId = d.C1CDocShipmentOrderID,
                                    Number = d.C1CNumber,
                                    Date = d.C1CDate ?? DB.CurrentDateTime,
                                    Consignee = d.C1CConsignees.Description,
                                    ActivePerson = d.DocShipmentOrderInfo.Persons.Name,
                                }));
                        break;
                }
            }
            FillDocShipmentOrdersWithGoods(DocShipmentOrders);
        }

        private void FillDocShipmentOrdersWithGoods(ObservableCollection<DocShipmentOrder> docShipmentOrders)
        {
            using (var gammaBase = DB.GammaDb)
            {
                foreach (var docShipmentOrder in docShipmentOrders)
                {
                    docShipmentOrder.DocShipmentOrderGoods = new ObservableCollection<DocNomenclatureItem>(gammaBase.vDocShipmentOrders
                        .Where(d => d.C1CDocShipmentOrderID == docShipmentOrder.DocShipmentOrderId)
                        .Select(d => new DocNomenclatureItem()
                        {
                            NomenclatureName = d.NomenclatureName,
                            Quantity = d.Quantity,
                            CollectedQuantity = d.CollectedQuantity
                        }));
                }
            }
        }

//        public List<Person> Persons { get; set; }
    }
}
