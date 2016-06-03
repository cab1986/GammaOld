using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
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
            Intervals = new List<string> { "Активные", "Последние 500", "Поиск" };
            FindCommand = new DelegateCommand(Find);
            CanChangePerson = DB.HaveWriteAccess("ActiveOrders");
            Persons = GammaBase.Persons.Where(p => p.PostTypeID == (int) PersonTypes.Loader).Select(p => new Person
            {
                PersonId = p.PersonID,
                Name = p.Name
            }).ToList();
            IntervalId = 0;
        }

        public DelegateCommand DeleteItemCommand { get; private set; }

        public DelegateCommand EditItemCommand { get; private set; }

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

        public bool CanChangePerson { get; set; }

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

        private void Find()
        {
            UIServices.SetBusyState();
            using (var gammaBase = DB.GammaDb)
            {
                switch (IntervalId)
                {
                    case 0:
                        DocShipmentOrders = new ObservableCollection<DocShipmentOrder>(
                            gammaBase.C1CDocShipmentOrder.Where(d => !d.Posted).OrderByDescending(d => d.C1CDate)
                            .Select(d => new DocShipmentOrder
                            {
                                DocShipmentOrderId = d.C1CDocShipmentOrderID,
                                Number = d.C1CNumber,
                                Date = d.C1CDate ?? DB.CurrentDateTime,
                                ActivePersonId = d.ActiveOrders.PersonID,
                            }));
                        break;
                    case 1:
                        DocShipmentOrders = new ObservableCollection<DocShipmentOrder>(
                            gammaBase.C1CDocShipmentOrder.OrderByDescending(d => d.C1CDate).Take(500)
                            .Select(d => new DocShipmentOrder
                            {
                                DocShipmentOrderId = d.C1CDocShipmentOrderID,
                                Number = d.C1CNumber,
                                Date = d.C1CDate ?? DB.CurrentDateTime,
                                ActivePersonId = d.ActiveOrders.PersonID,
                            }));
                        break;
                    case 2:
                        DocShipmentOrders = new ObservableCollection<DocShipmentOrder>(
                            gammaBase.C1CDocShipmentOrder.Where(d =>
                                (d.C1CNumber == null || d.C1CNumber.Contains(Number) || Number == "") &&
                                (d.C1CDate >= DateBegin || DateBegin == null) &&
                                (d.C1CDate <= DateEnd || DateEnd == null)).OrderByDescending(d => d.C1CDate).Take(500)
                                .Select(d => new DocShipmentOrder
                                {
                                    DocShipmentOrderId = d.C1CDocShipmentOrderID,
                                    Number = d.C1CNumber,
                                    Date = d.C1CDate ?? DB.CurrentDateTime,
                                    ActivePersonId = d.ActiveOrders.PersonID,
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
                    docShipmentOrder.DocShipmentOrderGoods = new ObservableCollection<DocShipmentGood>(gammaBase.vDocShipmentOrders
                        .Where(d => d.C1CDocShipmentOrderID == docShipmentOrder.DocShipmentOrderId)
                        .Select(d => new DocShipmentGood()
                        {
                            NomenclatureName = d.NomenclatureName,
                            Quantity = d.Quantity,
                            CollectedQuantity = d.CollectedQuantity
                        }));
                }
            }
        }

        public List<Person> Persons { get; set; }
    }



    public class Person
    {
        public int PersonId { get; set; }
        public string Name { get; set; }
    }

    public class DocShipmentOrder
    {
        public Guid DocShipmentOrderId { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        private int? _activePersonId;

        public int? ActivePersonId
        {
            get { return _activePersonId; }
            set
            {
                if (_activePersonId == value) return;
                _activePersonId = value;
                SetActivePersonOrder(DocShipmentOrderId, _activePersonId);
            }
        }

        private void SetActivePersonOrder(Guid docShipmentOrderId, int? activePersonId)
        {
            using (var gammaBase = DB.GammaDb)
            {
                var activeOrder =
                    gammaBase.ActiveOrders.FirstOrDefault(ao => ao.C1CDocShipmentOrderID == docShipmentOrderId);               
                if (activeOrder == null)
                {
                    activeOrder = new ActiveOrders
                    {
                        C1CDocShipmentOrderID = docShipmentOrderId
                    };
                    gammaBase.ActiveOrders.Add(activeOrder);
                }
                activeOrder.PersonID = activePersonId;

                // На случай, если в базе уже удален приказ с данным ID
                try
                {
                    gammaBase.SaveChanges();
                }
                catch (Exception)
                {
                    
                }
                
            }
        }
        public ObservableCollection<DocShipmentGood> DocShipmentOrderGoods { get; set; }
    }

    public class DocShipmentGood
    {
        public string NomenclatureName { get; set; }
        public string Quantity { get; set; }
        public decimal CollectedQuantity { get; set; }
    }
}
