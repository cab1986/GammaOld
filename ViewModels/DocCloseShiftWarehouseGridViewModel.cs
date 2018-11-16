// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Interfaces;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class DocCloseShiftWarehouseGridViewModel : SaveImplementedViewModel, IFillClearGrid, IBarImplemented
    {
        public DocCloseShiftWarehouseGridViewModel(OpenDocCloseShiftMessage msg)
        {
            Bars.Add(ReportManager.GetReportBar("DocCloseShiftWarehouse", VMID));
            if (msg.DocID == null)
            {
                PlaceID = (int)msg.PlaceID;
                CloseDate = (DateTime)msg.CloseDate;
                ShiftID = (byte)msg.ShiftID;
                PersonID = (Guid)msg.PersonID;
                DocCloseShiftDocs = new ObservableCollection<Docs>();
                Movements = new ObservableCollection<MovementProduct>();
            }
            else
            {
                Movements = new ObservableCollection<MovementProduct>(GammaBase.GetDocCloseShiftWarehouseMovements(msg.DocID, null)
                    .Select(d => new MovementProduct
                    {
                        NomenclatureName = d.Nomenclature,
                        Number = d.Number,
                        ProductId = d.ProductID,
                        Quantity = d.Weight ?? 0,
                        ProductKindName = d.ProductKindName,
                        OrderTypeName = d.OrderTypeName,
                        InPlaceName = d.InPlace,
                        InPlaceZoneName = d.InPlaceZone,
                        OutPlaceName = d.OutPlace,
                        OutPlaceZoneName = d.OutPlaceZone
                    }));
                var docCloseShift = GammaBase.Docs.Include(d => d.DocCloseShiftDocs).First(d => d.DocID == msg.DocID);
                DocCloseShiftDocs = new ObservableCollection<Docs>(docCloseShift.DocCloseShiftDocs);
                CloseDate = docCloseShift.Date;
                ShiftID = (docCloseShift.ShiftID ?? 0);
                PlaceID = (docCloseShift.PlaceID ?? -1);
                PersonID = docCloseShift.PersonGuid;
            }
            ShowMovementCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(DocProductKinds.DocProductBale, SelectedMovement.ProductId),
                () => SelectedMovement != null);
        }

        public void FillGrid()
        {
            UIServices.SetBusyState();
            ClearGrid();
            /*DocCloseShiftDocs = new ObservableCollection<Docs>(GammaBase.Docs.
                Where(d => d.PersonGuid == PersonID && //d.PlaceID == PlaceID && d.ShiftID == ShiftID &&
                    d.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    d.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    (d.DocTypeID == (int)DocTypes.DocMovement || d.DocTypeID == (int)DocTypes.DocShipment)).OrderByDescending(d => d.Date));
            var docList = DocCloseShiftDocs.Select(a => a.DocID).ToList();*/
            /*var docList = GammaBase.vDocMovementProducts
                .Where(d => (d.InPersonID == PersonID && //d.PlaceID == PlaceID && d.ShiftID == ShiftID &&
                    d.InDate >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    d.InDate <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate)))) ||
                    (d.OutPersonID == PersonID && //d.PlaceID == PlaceID && d.ShiftID == ShiftID &&
                    d.OutDate >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    d.OutDate <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate)))))
                .Select(a => a.DocMovementID).ToList();*/
            var docList = GammaBase.FillDocCloseShiftWarehouseDocs(PlaceID, ShiftID, CloseDate, PersonID).ToList();
            DocCloseShiftDocs = new ObservableCollection<Docs>(GammaBase.Docs.
                Where(d => docList.Contains(d.DocID)).OrderByDescending(d => d.Date));
            /*Movements = new ObservableCollection<MovementProduct>(
                    (from d in GammaBase.vDocMovementProducts
                     from ps in GammaBase.Docs.Where(p => d.DocMovementID == p.DocID && d.InPersonID == p.PersonGuid).DefaultIfEmpty()
                     where docList.Contains(d.DocMovementID)
                     select new MovementProduct
                     {
                         NomenclatureName = d.NomenclatureName,
                         Number = d.Number,
                         ProductId = d.ProductID,
                         Quantity = d.Quantity ?? 0,
                         ProductKindName = d.ProductKindName,
                         OrderTypeName = d.OrderTypeName,
                         InPlaceName = d.InPlace,
                         InPlaceZoneName = d.InPlaceZone,
                         OutPlaceName = d.OutPlace,
                         OutPlaceZoneName = d.OutPlaceZone
                     })
                    );*/
            Movements = new ObservableCollection<MovementProduct>(GammaBase.FillDocCloseShiftWarehouseMovements(PlaceID, ShiftID, CloseDate, PersonID).Select(d => new MovementProduct
            {
                NomenclatureName = d.NomenclatureName,
                Number = d.Number,
                ProductId = d.ProductID,
                Quantity = d.Quantity ?? 0,
                ProductKindName = d.ProductKindName,
                OrderTypeName = d.OrderTypeName,
                InPlaceName = d.InPlace,
                InPlaceZoneName = d.InPlaceZone,
                OutPlaceName = d.OutPlace,
                OutPlaceZoneName = d.OutPlaceZone
            }));

            IsChanged = true;
        }

        public bool IsChanged { get; private set; }

        public void ClearGrid()
        {
            DocCloseShiftDocs.Clear();
            Movements.Clear();
            IsChanged = true;
        }
        public override bool SaveToModel(Guid itemID)
        {
            if (!DB.HaveWriteAccess("DocCloseShiftDocs")) return true;
            var doc = GammaBase.Docs.Include(d => d.DocCloseShiftDocs).First(d => d.DocID == itemID);
            if (doc.DocCloseShiftDocs == null)
            {
                doc.DocCloseShiftDocs = new ObservableCollection<Docs>();
            }
            doc.DocCloseShiftDocs.Clear();
            foreach (var docCloseShiftDoc in DocCloseShiftDocs)
            {
                doc.DocCloseShiftDocs.Add(docCloseShiftDoc);
            }
            GammaBase.SaveChanges();
            return true;
        }
        private int PlaceID { get; set; }
        private byte ShiftID { get; set; }
        private Guid? PersonID { get; set; }
        private ObservableCollection<Docs> DocCloseShiftDocs { get; set; }
        private DateTime CloseDate { get; set; }
        public DelegateCommand ShowMovementCommand { get; private set; }
        public MovementProduct SelectedMovement { get; set; }
        private ObservableCollection<MovementProduct> _movements;
        public ObservableCollection<MovementProduct> Movements
        {
            get
            {
                return _movements;
            }
            set
            {
                _movements = value;
                RaisePropertyChanged("Movements");
            }
        }
        private ObservableCollection<BarViewModel> _bars = new ObservableCollection<BarViewModel>();
        public ObservableCollection<BarViewModel> Bars
        {
            get
            {
                return _bars;
            }
            set
            {
                _bars = value;
                RaisePropertyChanged("Bars");
            }
        }

        public Guid? VMID { get; } = Guid.NewGuid();
    }
}
