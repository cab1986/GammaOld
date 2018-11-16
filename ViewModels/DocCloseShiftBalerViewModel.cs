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
    public class DocCloseShiftBalerViewModel : SaveImplementedViewModel, IFillClearGrid, IBarImplemented
    {
        public DocCloseShiftBalerViewModel(OpenDocCloseShiftMessage msg)
        {
            Bars.Add(ReportManager.GetReportBar("DocCloseShiftBaler", VMID));
            if (msg.DocID == null)
            {
                PlaceID = (int)msg.PlaceID;
                CloseDate = (DateTime)msg.CloseDate;
                ShiftID = (byte)msg.ShiftID;
                DocCloseShiftDocs = new ObservableCollection<Docs>();
                Bales = new ObservableCollection<Bale>();
            }
            else
            {
                Bales = new ObservableCollection<Bale>(
                    from sp in GammaBase.GetDocCloseShiftBalerBales(msg.DocID)
                    select new Bale()
                    {
                        NomenclatureName = sp.NomenclatureName,
                        Number = sp.Number,
                        ProductId = sp.ProductID,
                        Weight = sp.Weight ?? 0
                    }
                    );
                var docCloseShift = GammaBase.Docs.Include(d => d.DocCloseShiftDocs).First(d => d.DocID == msg.DocID);
                DocCloseShiftDocs = new ObservableCollection<Docs>(docCloseShift.DocCloseShiftDocs);
                CloseDate = docCloseShift.Date;
                ShiftID = (docCloseShift.ShiftID ?? 0);
                PlaceID = (docCloseShift.PlaceID ?? -1);
            }
            ShowBaleCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(DocProductKinds.DocProductBale, SelectedBale.ProductId),
                () => SelectedBale != null);
        }

        public void FillGrid()
        {
            UIServices.SetBusyState();
            ClearGrid();
            DocCloseShiftDocs = new ObservableCollection<Docs>(GammaBase.Docs.
                Where(d => d.PlaceID == PlaceID && d.ShiftID == ShiftID &&
                    d.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    d.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    (d.DocTypeID == (int)DocTypes.DocProduction || d.DocTypeID == (int)DocTypes.DocWithdrawal)).OrderByDescending(d => d.Date));
            foreach (var doc in DocCloseShiftDocs.Where(doc => doc.DocTypeID == (byte)DocTypes.DocProduction))
            {
                Bales.Add(
                    (from d in GammaBase.DocProductionProducts
                     join ps in GammaBase.ProductBales on d.ProductID equals ps.ProductID
                     where d.DocID == doc.DocID
                     select new Bale
                     {
                         NomenclatureName = string.Concat(ps.C1CNomenclature.Name, " ", ps.C1CCharacteristics.Name),
                         Number = d.DocProduction.Docs.Number,
                         ProductId = d.ProductID,
                         Weight = ps.Weight ?? 0
                     }).FirstOrDefault()
                    );
            }
            IsChanged = true;
        }

        public bool IsChanged { get; private set; }

        public void ClearGrid()
        {
            DocCloseShiftDocs.Clear();
            Bales.Clear();
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
        private ObservableCollection<Docs> DocCloseShiftDocs { get; set; }
        private DateTime CloseDate { get; set; }
        public DelegateCommand ShowBaleCommand { get; private set; }
        public Bale SelectedBale { get; set; }
        private ObservableCollection<Bale> _bales;
        public ObservableCollection<Bale> Bales
        {
            get
            {
                return _bales;
            }
            set
            {
                _bales = value;
                RaisePropertyChanged("Bales");
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
