using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Attributes;
using Gamma.Common;
using Gamma.Interfaces;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class DocShipmentOrderViewModel : SaveImplementedViewModel, ICheckedAccess
    {
        private bool _isShipped;
        private DateTime? _dateOut;
        private bool _isConfirmed;
        private DateTime? _dateIn;

        public DocShipmentOrderViewModel(Guid docShipmentOrderId, GammaEntities gammaBase = null) : base(gammaBase)
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
            PersonsOut = GammaBase.Persons.Where(p => p.BranchID == GammaBase.Branches.FirstOrDefault(b => b.C1CSubdivisionID == docShipmentOrderInfo.C1COutSubdivisionID).BranchID).ToList();
            PersonsIn = GammaBase.Persons.Where(p => p.BranchID == GammaBase.Branches.FirstOrDefault(b => b.C1CSubdivisionID == docShipmentOrderInfo.C1CInSubdivisionID).BranchID).ToList();
            PlacesIn =
                GammaBase.Places.Where(
                    p =>
                        p.BranchID ==
                        GammaBase.Branches.FirstOrDefault(
                            b => b.C1CSubdivisionID == docShipmentOrderInfo.C1CInSubdivisionID).BranchID
                        && (p.IsWarehouse??false))
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
                        && (p.IsWarehouse ?? false) && (p.IsShipmentWarehouse??false))
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
            Shipper = docShipmentOrderInfo.Shipper;
            Consignee = docShipmentOrderInfo.Consignee;
            Title = $"{docShipmentOrderInfo.OrderType} № {Number}";
            VehicleNumber = docShipmentOrderInfo.VehicleNumber;
            ActivePersonOutId = docShipmentOrderInfo.OutActivePersonID;
            ActivePersonInId = docShipmentOrderInfo.InActivePersonID;
            ShiftOutId = docShipmentOrderInfo.OutShiftID;
            ShiftInId = docShipmentOrderInfo.InShiftId;
            IsShipped = docShipmentOrderInfo.IsShipped;
            IsConfirmed = docShipmentOrderInfo.IsConfirmed??false;
            OutPlaceId = docShipmentOrderInfo.OutPlaceID??PlacesOut.FirstOrDefault()?.PlaceID;
            InPlaceId = docShipmentOrderInfo.InPlaceID??PlacesIn.FirstOrDefault()?.PlaceID;
            FillDocShipmentOrderGoods(docShipmentOrderId);
            DenyEditIn = InPlaceId != null && !WorkSession.PlaceIds.Contains((int)InPlaceId) || docShipmentOrderInfo.InBranchID != WorkSession.BranchID;
            DenyEditOut = OutPlaceId != null && !WorkSession.PlaceIds.Contains((int) OutPlaceId) || docShipmentOrderInfo.OutBranchID != WorkSession.BranchID;
            InVisibible = docShipmentOrderInfo.OrderKindID == 1; // 0 - приказ на отгрузку, 1 - внутренний заказ
            IsReadOnly = !DB.HaveWriteAccess("DocMovement") || IsShipped;
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
        }

        public string Driver { get; set; }

        public string DriverDocument { get; set; }

        public MovementProduct SelectedProduct { get; set; }

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

        public int? OutPlaceId { get; set; }
        public int? InPlaceId { get; set; }

        public List<Place> PlacesIn { get; set; }
        public List<Place> PlacesOut { get; set; }

        private byte OrderKindId { get; set; }

        public bool IsShipped
        {
            get { return _isShipped; }
            set
            {
                _isShipped = value;
                if (DateOut == null && value) DateOut = DB.CurrentDateTime;
                if (!InVisibible) IsConfirmed = IsShipped;
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

        private bool? _checkAll;

        public string Title { get; set; }

        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != VMId) return;           
            SaveToModel();
            ReportManager.PrintReport(msg.ReportID, DocShipmentOrderID);
        }

        public bool InVisibible { get; set; }

        public bool DenyEditOut { get; private set; }
        public bool DenyEditIn { get; private set; }

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
                        InQuantity = d.InQuantity??0
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
                        DocMovementId = dm.DocMovementID
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

        [UIAuth(UIAuthLevel.ReadOnly)]
        public string VehicleNumber { get; set; }
        [UIAuth(UIAuthLevel.ReadOnly)]
        public Guid? ActivePersonOutId { get; set; }
        public Guid? ActivePersonInId { get; set; }
        public List<Persons> PersonsOut { get; set; }
        public List<Persons> PersonsIn { get; set; }

        public List<MovementItem> Movements { get; set; }
        public MovementItem SelectedMovementItem { get; set; }

        public DelegateCommand OpenMovementCommand { get; private set; }

        private Guid DocShipmentOrderID { get; set; }

        public override bool SaveToModel(GammaEntities gammaDb = null)
        {
            if (!DB.HaveWriteAccess("DocShipmentOrderInfo")) return true;
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
                doc.InShiftID = ShiftInId;
                doc.OutShiftID = ShiftOutId;
                doc.OutPlaceID = OutPlaceId;
                doc.OutDate = DateOut;
                doc.InDate = DateIn;
                doc.InPlaceID = InPlaceId;
                doc.InActivePersonID = ActivePersonInId;
                doc.OutActivePersonID = ActivePersonOutId;
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
    }
}
