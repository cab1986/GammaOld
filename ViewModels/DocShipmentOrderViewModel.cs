// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Attributes;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Interfaces;
using Gamma.Models;
using DevExpress.XtraEditors;

namespace Gamma.ViewModels
{
    public class DocShipmentOrderViewModel : SaveImplementedViewModel, ICheckedAccess
    {
        private bool _isShipped;
        private bool _isReturned;
        private DateTime? _dateOut;
        private bool _isConfirmed;
        private DateTime? _dateIn;
        private string _errorMessage;

        public DocShipmentOrderViewModel(Guid docShipmentOrderId)
        {
            Bars.Add(ReportManager.GetReportBar("DocShipmentOrder",VMId));
            DocShipmentOrderID = docShipmentOrderId;
            var docShipmentOrderInfo =
                GammaBase.v1COrders
                    .FirstOrDefault(d => d.C1COrderID == docShipmentOrderId);
            if (docShipmentOrderInfo == null)
            {
                MessageBox.Show("Не удалось найти приказ в базе", "Ошибка получения приказа", MessageBoxButton.OK,
                    MessageBoxImage.Hand);
                CloseWindow();
                return;
            }
            //PersonsOut = GammaBase.Persons.Where(p => p.BranchID == GammaBase.Branches.FirstOrDefault(b => b.C1CSubdivisionID == docShipmentOrderInfo.C1COutSubdivisionID).BranchID).ToList();
            //PersonsIn = GammaBase.Persons.Where(p => p.BranchID == GammaBase.Branches.FirstOrDefault(b => b.C1CSubdivisionID == docShipmentOrderInfo.C1CInSubdivisionID).BranchID).ToList();
            PlacesIn =
                GammaBase.Places.Where(
                    p => GammaBase.Places1CWarehouses
                .Where(w => w.C1CWarehouseID == docShipmentOrderInfo.C1CWarehouseLoadID)
                .Select(w => w.PlaceID).ToList().Contains(p.PlaceID)
                )
                .Select(p => new Place
                {
                    PlaceID = p.PlaceID,
                    PlaceGuid = p.PlaceGuid,
                    PlaceName = p.Name
                }).ToList();
            /*GammaBase.Places.Where(
                p =>
                    p.BranchID ==
                    GammaBase.Branches.FirstOrDefault(
                        b => b.C1CSubdivisionID == docShipmentOrderInfo.C1CInSubdivisionID).BranchID
                    && ((p.IsWarehouse??false) || (p.IsProductionPlace ?? false)))
                .Select(p => new Place
                {
                    PlaceID = p.PlaceID,
                    PlaceGuid = p.PlaceGuid,
                    PlaceName = p.Name
                }).ToList();
        PlacesOut = GammaBase.Places.Where(
                p =>
                    p.BranchID ==
                    GammaBase.Branches.FirstOrDefault(
                        b => b.C1CSubdivisionID == docShipmentOrderInfo.C1COutSubdivisionID).BranchID 
                    && ((p.IsWarehouse ?? false) || (p.IsProductionPlace ?? false))) //&& (p.IsShipmentWarehouse??false))
                .Select(p => new Place
                {
                    PlaceID = p.PlaceID,
                    PlaceGuid = p.PlaceGuid,
                    PlaceName = p.Name
                }).ToList();*/
            PlacesOut =
            GammaBase.Places.Where(
                p => GammaBase.Places1CWarehouses
            .Where(w => w.C1CWarehouseID == docShipmentOrderInfo.C1CWarehouseUnloadID)
            .Select(w => w.PlaceID).ToList().Contains(p.PlaceID)
            )
            .Select(p => new Place
            {
                PlaceID = p.PlaceID,
                PlaceGuid = p.PlaceGuid,
                PlaceName = p.Name
            }).ToList();

            OrderKindId = (byte) docShipmentOrderInfo.OrderKindID;
            Driver = docShipmentOrderInfo.Driver;
            DriverDocument = docShipmentOrderInfo.DriverDocument;
            Date = docShipmentOrderInfo.Date;
            DateOut = docShipmentOrderInfo.OutDate;
            DateIn = docShipmentOrderInfo.InDate;
            Number = docShipmentOrderInfo.Number;
            Shipper = docShipmentOrderInfo.Shipper ?? docShipmentOrderInfo.Warehouse;
            Consignee = docShipmentOrderInfo.Consignee;
            Buyer = docShipmentOrderInfo.Buyer;
            Title = $"{docShipmentOrderInfo.OrderType} № {Number}";
            VehicleNumber = docShipmentOrderInfo.VehicleNumber;
            ActivePersonOutId = docShipmentOrderInfo.OutActivePersonID;
            ActivePersonsOut = new List<object>();
            foreach (var personsItem in GammaBase.Persons.Where(p => p.DocShipmentOrderPersons.Any(d => !d.IsInActive && d.DocShipmentOrders.DocOrderID == DocShipmentOrderID)))
                ActivePersonsOut.Add(personsItem);
            ActivePersonsIn = new List<object>();
            foreach (var personsItem in GammaBase.Persons.Where(p => p.DocShipmentOrderPersons.Any(d => d.IsInActive && d.DocShipmentOrders.DocOrderID == DocShipmentOrderID)))
                ActivePersonsIn.Add(personsItem);
            ActivePersonInId = docShipmentOrderInfo.InActivePersonID;
            ShiftOutId = docShipmentOrderInfo.OutShiftID;
            ShiftInId = docShipmentOrderInfo.InShiftId;
            IsShipped = docShipmentOrderInfo.IsShipped;
            IsReturned = docShipmentOrderInfo.IsReturned;
            AbilityChange = DB.GetAbilityChangeDocShipmentOrder(DocShipmentOrderID); //может изменить значение IsReturned, влияет на расчет DenyEditOut, поэтому обязательно после IsReturned и до DenyEditOut/DenyEditIn
            if (AbilityChange == 1 && IsReturned) IsReturned = false;
            IsConfirmed = docShipmentOrderInfo.IsConfirmed??false;
            OutVisibible = docShipmentOrderInfo.OrderKindID == 0 || docShipmentOrderInfo.OrderKindID == 1; // 0 - приказ на отгрузку, 1 - внутренний заказ, 2 - заказ на перемещение
            InVisibible = false;// docShipmentOrderInfo.OrderKindID == 1; // 0 - приказ на отгрузку, 1 - внутренний заказ, 2 - заказ на перемещение
            //обязательно раньше OutPlaceId и InPlaceId
            MovementVisibible = docShipmentOrderInfo.OrderKindID == 2; // 0 - приказ на отгрузку, 1 - внутренний заказ, 2 - заказ на перемещение
            //обязательно раньше OutPlaceId и InPlaceId
            OutPlaceId = docShipmentOrderInfo.OutPlaceID ?? (PlacesOut?.Count == 1 ? PlacesOut.FirstOrDefault()?.PlaceID : null);
            InPlaceId = docShipmentOrderInfo.InPlaceID ?? (PlacesIn?.Count == 1 ? PlacesIn.FirstOrDefault()?.PlaceID : null);
            FillDocShipmentOrderGoods(docShipmentOrderId);
            DenyEditIn = /*!WorkSession.DBAdmin &&*/ ((DateOut != null && InPlaceId != null && !WorkSession.PlaceIds.Contains((int)InPlaceId)) || docShipmentOrderInfo.InBranchID != WorkSession.BranchID || IsReturned);
            DenyEditOut = /*!WorkSession.DBAdmin &&*/ ((DateIn != null && OutPlaceId != null && !WorkSession.PlaceIds.Contains((int) OutPlaceId)) || docShipmentOrderInfo.OutBranchID != WorkSession.BranchID || IsReturned);
            DenyEditInPlace = DenyEditIn || PlacesIn?.Count == 1;
            DenyEditOutPlace = DenyEditOut || PlacesOut?.Count == 1;
#if (!DEBUG)
            IsReadOnly = !DB.HaveWriteAccess("DocMovement") || !((AbilityChange ?? 0) == 1);
#else
            IsReadOnly = !((AbilityChange ?? 0) == 1);
#endif
            Movements = GammaBase.DocMovement.Include(m => m.Docs).Include(m => m.OutPlaces).Include(m => m.InPlaces)
                .Where(m => m.DocOrderID == DocShipmentOrderID)
                .Select(m => new MovementItem
                {
                    DocId = m.DocID,
                    Date = m.Docs.Date,
                    PlaceFrom = m.OutPlaces.Name,
                    PlaceTo = m.InPlaces.Name,
                    Number = m.Docs.Number,
                    IsConfirmed = m.Docs.IsConfirmed
                }).ToList();
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
            OpenMovementCommand = new DelegateCommand(() => MessageManager.OpenDocMovement(SelectedMovementItem.DocId), () => SelectedMovementItem != null);
            DocShipmentOrderGoods.CollectionChanged +=DocShipmentOrderGoodsOnCollectionChanged;
            DeleteProductCommand = new DelegateCommand(DeleteProduct, () => !DenyEditOut && SelectedProduct != null);
            //var canUploadTo1CCommand = DocOrderId == null;
            UploadTo1CCommand = new DelegateCommand(UploadTo1C, () => CanUploadTo1CCommand);
            ReturnTo1CCommand = new DelegateCommand(ReturnTo1C, () => CanReturnTo1CCommand);
        }

        public List<Persons> TestItems { get; set; }
        public List<Object> TestItem { get; set; }


        public string Driver { get; set; }

        public string DriverDocument { get; set; }

        public MovementProduct SelectedProduct { get; set; }

        public bool CanUploadTo1CCommand => !DenyEditOut && IsShipped;

        public bool CanReturnTo1CCommand => !DenyEditOut && !IsReturned && !IsShipped;

        public DelegateCommand DeleteProductCommand { get; private set; }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;
            if (SelectedProduct.IsConfirmed == true || SelectedProduct.IsAccepted)
            {
                MessageBox.Show("Нельзя удалить продукт, который уже получен или принят");
                return;
            }
            if (MessageBox.Show("Вы уверены, что хотите удалить данный продукт из приказа?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            using (var gammaBase = DB.GammaDb)
            {
                var outProduct =
                    gammaBase.DocOutProducts.FirstOrDefault(
                        op => op.ProductID == SelectedProduct.ProductId && op.DocID == SelectedProduct.DocMovementId);
                if (outProduct != null)
                {
                    gammaBase.DocOutProducts.Remove(outProduct);
                    gammaBase.SaveChanges();
                }                
            }
        }

        private void DocShipmentOrderGoodsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            IsConfirmed = DocShipmentOrderGoods.SelectMany(g => g.Products).All(p => p.IsConfirmed == true);
        }

        private int? _outPlaceId { get; set; }
        public int? OutPlaceId 
        {
            get { return _outPlaceId; }
            set
            {
                _outPlaceId = value;

                if (value == null || (MovementVisibible && InPlaceId == null && PlacesIn?.Count > 0))
                    PersonsOut = GammaBase.Persons.Take(0).ToList(); //пустой список
                else
                {
                    var rootPlaces = GammaBase.Places.Where(p => p.PlaceID == value).Select(p => p.RootPlaceID).ToList();
                    var users = GammaBase.Users.Where(u => u.Places.Any(p => p.PlaceID == value || rootPlaces.Contains(p.PlaceID))).Select(u => u.UserID).ToList();
                    var rootUsers = GammaBase.Users.Where(u => u.RootUserID != null && u.Places.Any(p => p.PlaceID == value || rootPlaces.Contains(p.PlaceID))).Select(u => u.RootUserID).ToList();
                    PersonsOut = GammaBase.Persons.Where(p => (users.Contains(p.Users.UserID) || rootUsers.Contains(p.Users.UserID))).OrderBy(p => p.Name).ToList();
                }
                RaisePropertyChanged("OutPlaceId");
            }
        }
        

        private int? _inPlaceId { get; set; }
        public int? InPlaceId
        {
            get { return _inPlaceId; }
            set
            {
                _inPlaceId = value;

                if (value == null)
                    PersonsIn = GammaBase.Persons.Take(0).ToList(); //пустой список
                else
                {
                    var rootPlaces = GammaBase.Places.Where(p => p.PlaceID == value).Select(p => p.RootPlaceID).ToList();
                    var users = GammaBase.Users.Where(u => u.Places.Any(p => p.PlaceID == value || rootPlaces.Contains(p.PlaceID))).Select(u => u.UserID).ToList();
                    var rootUsers = GammaBase.Users.Where(u => u.RootUserID != null && u.Places.Any(p => p.PlaceID == value || rootPlaces.Contains(p.PlaceID))).Select(u => u.RootUserID).ToList();
                    PersonsIn = GammaBase.Persons.Where(p => (users.Contains(p.Users.UserID) || rootUsers.Contains(p.Users.UserID))).OrderBy(p => p.Name).ToList();
                }
                if (MovementVisibible)
                {
                    OutPlaceId = OutPlaceId;
                    /*if (InPlaceId == null)
                        PersonsOut = GammaBase.Persons.Where(p => value != null).ToList(); //пустой список;
                    else
                    {
                        var rootPlaces = GammaBase.Places.Where(p => p.PlaceID == value).Select(p => p.RootPlaceID).ToList();
                        var users = GammaBase.Users.Where(u => u.Places.Any(p => p.PlaceID == value || rootPlaces.Contains(p.PlaceID))).Select(u => u.UserID).ToList();
                        var rootUsers = GammaBase.Users.Where(u => u.RootUserID != null && u.Places.Any(p => p.PlaceID == value || rootPlaces.Contains(p.PlaceID))).Select(u => u.RootUserID).ToList();
                        PersonsOut = GammaBase.Persons.Where(p => (users.Contains(p.Users.UserID) || rootUsers.Contains(p.Users.UserID))).OrderBy(p => p.Name).ToList();
                    }*/
                }
                RaisePropertyChanged("InPlaceId");
            }
        }

        public List<Place> PlacesIn { get; set; }
        public List<Place> PlacesOut { get; set; }

        private byte OrderKindId { get; set; }

        public bool IsShipped
        {
            get { return _isShipped; }
            set
            {
                _isShipped = value;
                if (IsReturned && value)
                    IsReturned = !value;
                if (DateOut == null && value) DateOut = DB.CurrentDateTime;
                //if (!InVisibible)
                    IsConfirmed = IsShipped;
                RaisePropertyChanged("IsShipped");
            }
        }

        public bool IsReturned
        {
            get { return _isReturned; }
            set
            {
                _isReturned = value;
                if (IsShipped && value)
                    IsShipped = !value;
                RaisePropertyChanged("IsReturned");
            }
        }

        public bool IsConfirmed
        {
            get { return _isConfirmed; }
            set
            {
                _isConfirmed = value;
                if (DateIn == null && _isConfirmed) DateIn = DB.CurrentDateTime;
                RaisePropertyChanged("IsConfirmed");
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                RaisePropertyChanged("ErrorMessage");
            }
        }

        private bool? _checkAll;

        public string Title { get; set; }

        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != VMId) return;           
            SaveToModel();
            ReportManager.PrintReport(msg.ReportID, DocShipmentOrderID);
        }

        public bool OutVisibible { get; private set; }
        public bool InVisibible { get; private set; }
        public bool MovementVisibible { get; private set; }

        public bool DenyEditOut { get; private set; }
        public bool DenyEditIn { get; private set; }
        public bool DenyEditOutPlace { get; private set; }
        public bool DenyEditInPlace { get; private set; }

        public bool DenyEditReturned => DenyEditOut || !IsReturned;

        //public bool DenyEditMovementPersonOut => DenyEditOut || (MovementVisibible && InPlaceId == null);
        private bool _denyEditMovementPersonOut { get; set; }
        public bool DenyEditMovementPersonOut
        {
            get { return _denyEditMovementPersonOut; }
            set
            {
                _denyEditMovementPersonOut = value;
                RaisePropertyChanged("DenyEditMovementPersonOut");
            }
        }

        private int? _abilityChange { get; set; }
        public int? AbilityChange
        {
            get { return _abilityChange; }
            set
            {
                _abilityChange = value;
                if (value == null)
                    ErrorMessage = "Ошибка при проверке разрешения на изменения из 1С";
                else
                {
                    if ((int)value != 1)
                    {
                        ErrorMessage = "Запрещено изменение. Задание заблокировано в 1С.";
                    }
                    else
                        ErrorMessage = "";
                }               

            }
        }

        public DelegateCommand UploadTo1CCommand { get; private set; }

        private void UploadTo1C()
        {
            UIServices.SetBusyState();
            if (MessageBox.Show("Документ будет сохранен и выгружен в 1С. Продолжить?", "Выгрузка в 1С",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)// || dateInit != Date)
            {
                //IsShipped = true;
                if (SaveToModel())
                {
//#if (!DEBUG)
                    if (DB.UploadShipmentOrderTo1C(DocShipmentOrderID))
                        CloseWindow();
//#endif
                }
            }
        }

        public DelegateCommand ReturnTo1CCommand { get; private set; }

        private void ReturnTo1C()
        {
            UIServices.SetBusyState();
            if (MessageBox.Show("Документ будет сохранен и возвращен в 1С на изменение. Продолжить?", "Выгрузка в 1С",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)// || dateInit != Date)
            {
                IsReturned = true;
                if (SaveToModel())
                { 
//#if (!DEBUG)
                    if (DB.UploadShipmentOrderTo1C(DocShipmentOrderID))
                        CloseWindow();
//#endif
                }
            }
        }

        private Guid VMId { get; } = Guid.NewGuid();

        private void FillDocShipmentOrderGoods(Guid docShipmentOrderId)
        {
            DocShipmentOrderGoods =
                new ItemsChangeObservableCollection<MovementGood>(GammaBase.v1COrderGoods.Where(
                    d => d.DocOrderID == docShipmentOrderId)
                    .Select(d => new MovementGood
                    {
                        NomenclatureID = d.C1CNomenclatureID,
                        CharacteristicID = d.C1CCharacteristicID,
                        NomenclatureName = d.NomenclatureName,
                        Amount = d.Quantity,
                        OutQuantity = d.OutQuantity??0,
                        InQuantity = d.InQuantity??0,
                        Quality = d.Quality
                    }));
            foreach (var good in DocShipmentOrderGoods)
            {
                good.Products = new ItemsChangeObservableCollection<MovementProduct>(GammaBase.vDocMovementProducts
                    .Where(dm => dm.DocOrderID == DocShipmentOrderID 
                    && dm.C1CNomenclatureID == good.NomenclatureID && dm.C1CCharacteristicID == good.CharacteristicID).Select(dm => new MovementProduct
                    {
                        ProductId = dm.ProductID,
                        Number = dm.Number,
                        Quantity = dm.Quantity??0,
                        IsShipped = dm.IsShipped??false,
                        IsAccepted = dm.IsAccepted??false,
                        IsConfirmed = dm.IsConfirmed,
                        DocMovementId = dm.DocMovementID,
                        OutPerson = dm.OutPerson
                    }));
            }
            IsConfirmed = DocShipmentOrderGoods.SelectMany(g => g.Products).Any(p => p.IsConfirmed ?? false);
            if (DocShipmentOrderGoods.Count < 1)
            {
                CheckAll = null;
                IsConfirmed = false;
                return;
            }
            if (DocShipmentOrderGoods.SelectMany(g => g.Products).Any() && DocShipmentOrderGoods.SelectMany(g => g.Products).All(p => p.IsConfirmed == true))
            {
                CheckAll = true;
            }
            else if (DocShipmentOrderGoods.SelectMany(g => g.Products).Any() &&
                DocShipmentOrderGoods.SelectMany(g => g.Products)
                    .All(p => p.IsConfirmed == null || p.IsConfirmed == false))
            {
                CheckAll = false;
            }
        }

        public byte? ShiftOutId { get; set; }
        public byte? ShiftInId { get; set; }

        public List<BarViewModel> Bars { get; set; } = new List<BarViewModel>();

        public DateTime? Date { get; set; }

        public DateTime? DateOut
        {
            get { return _dateOut; }
            set
            {
                _dateOut = value;
                RaisePropertyChanged("DateOut");
            }
        }

        public DateTime? DateIn
        {
            get { return _dateIn; }
            set
            {
                _dateIn = value;
                RaisePropertyChanged("DateIn");
            }
        }

        public string Number { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string Buyer { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public string VehicleNumber { get; set; }
        [UIAuth(UIAuthLevel.ReadOnly)]
        public Guid? ActivePersonOutId { get; set; }
        private List<Object> _activePersonsOut { get; set; }
        public List<Object> ActivePersonsOut
        {
            get { return _activePersonsOut; }
            set
            {
                _activePersonsOut = value;
                RaisePropertyChanged("ActivePersonsOut");
            }

        }

        public List<Object> ActivePersonsIn { get; set; }

        public Guid? ActivePersonInId { get; set; }
        private List<Persons> _personsOut { get; set; }
        public List<Persons> PersonsOut
        {
            get { return _personsOut; }
            set
            {
                _personsOut = value;
                RaisePropertyChanged("PersonsOut");
            }
        }

        public List<Persons> _personsIn { get; set; }
        public List<Persons> PersonsIn
        {
            get { return _personsIn; }
            set
            {
                _personsIn = value;
                RaisePropertyChanged("PersonsIn");
            }
        }

        public List<MovementItem> Movements { get; set; }
        public MovementItem SelectedMovementItem { get; set; }

        public DelegateCommand OpenMovementCommand { get; private set; }

        private Guid DocShipmentOrderID { get; set; }

        public override bool SaveToModel()
        {
            if (!DB.HaveWriteAccess("DocShipmentOrderInfo")) return true;
            AbilityChange = DB.GetAbilityChangeDocShipmentOrder(DocShipmentOrderID);
            if (AbilityChange == null)
                MessageBox.Show("Ошибка при проверке разрешения на изменения из 1С");
            else
            {
                if ((int)AbilityChange != 1)
                {
                    MessageBox.Show("Запрещено изменение. Задание заблокировано в 1С.");
                    return false;
                }
            }


            using (var gammaBase = DB.GammaDb)
            {
                var doc =
                    gammaBase.DocShipmentOrders.FirstOrDefault(d => d.DocOrderID == DocShipmentOrderID);
                if (doc == null)
                {
                    doc = new DocShipmentOrders
                    {
                        DocOrderID = DocShipmentOrderID,
                        OrderTypeID = OrderKindId
                    };
                    gammaBase.DocShipmentOrders.Add(doc);
                }
                doc.VehicleNumber = VehicleNumber;
                doc.IsShipped = IsShipped;
                doc.IsReturned = IsReturned;
                doc.InShiftID = ShiftInId;
                doc.OutShiftID = ShiftOutId;
                doc.OutPlaceID = OutPlaceId;
                doc.OutDate = DateOut;
                doc.InDate = DateIn;
                doc.InPlaceID = InPlaceId;
                doc.InActivePersonID = ActivePersonInId;
                doc.OutActivePersonID = ActivePersonOutId;

                List<Guid> outActivePersonIds = new List<Guid>();
                if (ActivePersonsOut != null)
                    foreach (object item in ((List<object>)ActivePersonsOut))
                        outActivePersonIds.Add(((Persons)item).PersonID);
                //gammaBase.DocShipmentOrderPersons.RemoveRange(gammaBase.DocShipmentOrderPersons.Where(p => !p.IsInActive && p.Persons.PlaceID != OutPlaceId && p.DocOrderID == DocShipmentOrderID && !outActivePersonIds.Contains(p.PersonID)));
                gammaBase.DocShipmentOrderPersons.RemoveRange(gammaBase.DocShipmentOrderPersons.Where(p => !p.IsInActive && p.DocOrderID == DocShipmentOrderID));
                foreach (var personsItem in gammaBase.Persons.Where(p => outActivePersonIds.Contains(p.PersonID)))// && (p.PlaceID == OutPlaceId || (OutPlaceId == 104 && p.PlaceID == 28)) && !p.DocShipmentOrderPersons.Any(d => !d.IsInActive && d.DocShipmentOrders.DocOrderID == DocShipmentOrderID)))
                {
                    var docShipmentOrderPerson = new DocShipmentOrderPersons
                    {
                        DocShipmentOrderPersonID = SqlGuidUtil.NewSequentialid(),
                        DocOrderID = DocShipmentOrderID,
                        PersonID = personsItem.PersonID,
                        IsInActive = false
                    };
                    gammaBase.DocShipmentOrderPersons.Add(docShipmentOrderPerson);

                }
                List<Guid> inActivePersonIds = new List<Guid>();
                if (ActivePersonsIn != null)
                    foreach (object item in ((List<object>)ActivePersonsIn))
                        inActivePersonIds.Add(((Persons)item).PersonID);
                //gammaBase.DocShipmentOrderPersons.RemoveRange(gammaBase.DocShipmentOrderPersons.Where(p => p.IsInActive && p.Persons.PlaceID != InPlaceId && p.DocOrderID == DocShipmentOrderID && !inActivePersonIds.Contains(p.PersonID)));
                gammaBase.DocShipmentOrderPersons.RemoveRange(gammaBase.DocShipmentOrderPersons.Where(p => p.IsInActive && p.DocOrderID == DocShipmentOrderID));
                foreach (var personsItem in gammaBase.Persons.Where(p => inActivePersonIds.Contains(p.PersonID)))// && (p.PlaceID == InPlaceId || (InPlaceId == 104 && p.PlaceID == 28)) && !p.DocShipmentOrderPersons.Any(d => d.IsInActive && d.DocShipmentOrders.DocOrderID == DocShipmentOrderID)))
                {
                    var docShipmentOrderPerson = new DocShipmentOrderPersons
                    {
                        DocShipmentOrderPersonID = SqlGuidUtil.NewSequentialid(),
                        DocOrderID = DocShipmentOrderID,
                        PersonID = personsItem.PersonID,
                        IsInActive = true
                    };
                    gammaBase.DocShipmentOrderPersons.Add(docShipmentOrderPerson);

                }

                doc.Driver = Driver;
                doc.DriverDocument = DriverDocument;
                foreach (var docMovement in Movements.Select(movement => gammaBase.Docs.FirstOrDefault(d => d.DocID == movement.DocId)).Where(docMovement => docMovement != null))
                {
                    docMovement.IsConfirmed = IsConfirmed;
                }
                foreach (var good in DocShipmentOrderGoods)
                {
                    foreach (var goodProduct in good.Products)
                    {
                        var docInProduct = gammaBase.DocInProducts.FirstOrDefault(p => p.ProductID == goodProduct.ProductId && p.DocID == goodProduct.DocMovementId);
                        if (docInProduct != null)
                        {
                            docInProduct.IsConfirmed = goodProduct.IsConfirmed;
                        }
                        else if (goodProduct.IsConfirmed == true)
                        {
                            docInProduct = new DocInProducts
                            {
                                DocID = goodProduct.DocMovementId,
                                Date = DB.CurrentDateTime,
                                IsConfirmed = goodProduct.IsConfirmed,
                                ProductID = goodProduct.ProductId
                            };
                            gammaBase.DocInProducts.Add(docInProduct);
                        }
                    }
                }
                gammaBase.SaveChanges();
            }
            return true;
        }

        public ItemsChangeObservableCollection<MovementGood> DocShipmentOrderGoods { get; set; }

        public bool IsReadOnly { get; private set; }

        public bool? CheckAll
        {
            get { return _checkAll; }
            set
            {
                _checkAll = value;
                RaisePropertyChanged("CheckAll");
                if (_checkAll == null) return;
                foreach (var product in DocShipmentOrderGoods.SelectMany(good => good.Products))
                {
                    product.IsConfirmed = _checkAll;
                    IsConfirmed = (bool)_checkAll;
                }
            }
        }

        public override bool CanSaveExecute()
        {
            return base.CanSaveExecute() && !IsReadOnly;
        }
    }
}
