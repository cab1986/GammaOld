using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Attributes;
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
            DenyEditIn = docShipmentOrderInfo.InBranchID != WorkSession.BranchID;
            DenyEditOut = docShipmentOrderInfo.OutBranchID != WorkSession.BranchID;
            InVisibible = docShipmentOrderInfo.OrderKindID == 1; // 0 - приказ на отгрузку, 1 - внутренний заказ
            IsReadOnly = !DB.HaveWriteAccess("DocMovement") || IsShipped;
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
        }

        public int? OutPlaceId { get; set; }
        public int? InPlaceId { get; set; }

        public List<Place> PlacesIn { get; set; }
        public List<Place> PlacesOut { get; set; }

        public bool IsShipped
        {
            get { return _isShipped; }
            set
            {
                _isShipped = value;
                if (DateOut == null) DateOut = DB.CurrentDateTime;
            }
        }

        public bool IsConfirmed
        {
            get { return _isConfirmed; }
            set
            {
                _isConfirmed = value;
                if (DateIn == null) DateIn = DB.CurrentDateTime;
            }
        }

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
                new ObservableCollection<MovementGood>(GammaBase.v1COrderGoods.Where(
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
                good.Products = new List<MovementProduct>(GammaBase.vDocMovementProducts
                    .Where(dm => dm.DocOrderID == DocShipmentOrderID 
                    && dm.C1CNomenclatureID == good.NomenclatureID && dm.C1CCharacteristicID == good.CharacteristicID).Select(dm => new MovementProduct
                    {
                        ProductId = dm.ProductID,
                        Number = dm.Number,
                        Quantity = dm.Quantity??0,
                        IsShipped = dm.IsShipped??false,
                        IsAccepted = dm.IsAccepted??false,
                        IsConfirmed = dm.IsConfirmed
                    }));
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
                        DocOrderID = DocShipmentOrderID
                    };
                    gammaBase.DocShipmentOrders.Add(doc);
                }
                doc.VehicleNumber = VehicleNumber;
                doc.IsShipped = IsShipped;
                doc.InShiftID = ShiftInId;
                doc.OutShiftID = ShiftOutId;
                doc.OutPlaceID = OutPlaceId;
                doc.InPlaceID = InPlaceId;
                doc.InActivePersonID = ActivePersonInId;
                doc.OutActivePersonID = ActivePersonOutId;
                
                gammaBase.SaveChanges();
            }
            return true;
        }

        public ObservableCollection<MovementGood> DocShipmentOrderGoods { get; set; }

        public bool IsReadOnly { get; private set; }
    }
}
