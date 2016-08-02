using System;
using System.Linq;
using Gamma.Interfaces;
using System.Collections.ObjectModel;
using Gamma.Models;
using DevExpress.Mvvm;
using System.Data.Entity.SqlServer;
using Gamma.Common;

namespace Gamma.ViewModels
{
    class DocCloseShiftPMGridViewModel : SaveImplementedViewModel, IFillClearGrid, IBarImplemented
    {
        public DocCloseShiftPMGridViewModel(OpenDocCloseShiftMessage msg, GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb;
            if (msg.DocID == null)
            {
                PlaceID = (int)msg.PlaceID;
                CloseDate = (DateTime)msg.CloseDate;
                ShiftID = (byte)msg.ShiftID;
                DocCloseShiftDocs = new ObservableCollection<Docs>();
            }
            else
            {
                Spools = new ObservableCollection<PaperBase>(
                    from sp in GammaBase.GetDocCloseShiftPMSpools(msg.DocID)
                    select new PaperBase
                    {
                        CharacteristicID = (Guid)sp.CharacteristicID,
                        NomenclatureID = sp.NomenclatureID,
                        Nomenclature = sp.Nomenclature,
                        Number = sp.Number,
                        ProductID = sp.ProductID,
                        Weight = sp.Weight??0
                    }
                    );
                DocCloseShift = GammaBase.Docs.Include("DocCloseShiftDocs").FirstOrDefault(d => d.DocID == msg.DocID);
                DocCloseShiftDocs = new ObservableCollection<Docs>(DocCloseShift.DocCloseShiftDocs);
                CloseDate = DocCloseShift.Date;
                ShiftID = (byte)DocCloseShift.ShiftID;
                PlaceID = (byte)DocCloseShift.PlaceID;
            }
            ShowSpoolCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedSpool.ProductID),
                () => SelectedSpool != null);
            Bars.Add(ReportManager.GetReportBar("DocCloseShiftDocPM", VMID));
        }

        public bool IsChanged { get; private set; }

        public Guid? VMID { get; } = Guid.NewGuid();

        private byte ShiftID { get; set; }
        private DateTime CloseDate { get; set; }
        private int PlaceID { get; set; }
        private Docs DocCloseShift { get; set; }
        private ObservableCollection<Docs> DocCloseShiftDocs { get; set; }
        public void FillGrid()
        {
            ClearGrid();
            DocCloseShiftDocs = new ObservableCollection<Docs>(GammaBase.Docs.
                Where(d => d.PlaceID == PlaceID && d.ShiftID == ShiftID && 
                    d.Date >= SqlFunctions.DateAdd("hh",-1,DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh",-1,CloseDate))) && 
                    d.Date <= SqlFunctions.DateAdd("hh",1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh",-1,CloseDate))) &&
                    (d.DocTypeID == (int)DocTypes.DocProduction || d.DocTypeID == (int)DocTypes.DocWithdrawal)
                    && !d.DocCloseShift.Any() && d.IsConfirmed)
                    .Select(d => d));
            foreach (var doc in DocCloseShiftDocs.Where(doc => doc.DocTypeID == (byte)DocTypes.DocProduction))
            {
                Spools.Add(
                    (from d in GammaBase.DocProductionProducts 
                        join ps in GammaBase.ProductSpools on d.ProductID equals ps.ProductID
                        where d.DocID == doc.DocID
                        select new PaperBase 
                        { 
                            CharacteristicID = (Guid)ps.C1CCharacteristicID,
                            NomenclatureID = ps.C1CNomenclatureID,
                            Nomenclature = string.Concat(ps.C1CNomenclature.Name," ",ps.C1CCharacteristics.Name),
                            Number = d.DocProduction.Docs.Number,
                            ProductID = d.ProductID,
                            Weight = ps.DecimalWeight
                        }).FirstOrDefault()
                    );
            }
            IsChanged = true;
        }

        public void ClearGrid()
        {
            DocCloseShiftDocs.Clear();
            Spools.Clear();
            IsChanged = true;
        }
        public override void SaveToModel(Guid itemID, GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            base.SaveToModel(itemID, gammaBase);
            if (DocCloseShift == null)
            {
                DocCloseShift = GammaBase.Docs.FirstOrDefault(d => d.DocID == itemID);
                foreach (var doc in DocCloseShiftDocs)
                {
                    DocCloseShift.DocCloseShiftDocs.Add(doc);
                }
            }
            else
            {
                DocCloseShift.DocCloseShiftDocs.Clear();
                foreach (var doc in DocCloseShiftDocs)
                {
                    DocCloseShift.DocCloseShiftDocs.Add(doc);
                }
            }
            GammaBase.SaveChanges();
        }
        private ObservableCollection<PaperBase> _spools = new ObservableCollection<PaperBase>();
        public ObservableCollection<PaperBase> Spools
        {
            get
            {
                return _spools;
            }
            set
            {
            	_spools = value;
                RaisePropertyChanged("Spools");
            }
        }
        public PaperBase SelectedSpool { get; set; }
        public DelegateCommand ShowSpoolCommand {get; private set;}
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
    }
}
