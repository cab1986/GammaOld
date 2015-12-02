using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Interfaces;
using System.Collections.ObjectModel;
using Gamma.Models;
using GalaSoft.MvvmLight.Command;

namespace Gamma.ViewModels
{
    class DocCloseShiftPMGridViewModel : SaveImplementedViewModel, IFillClearGrid
    {
        public DocCloseShiftPMGridViewModel(OpenDocCloseShiftMessage msg)
        {
            if (msg.DocID == null)
            {
                PlaceID = (int)msg.PlaceID;
                CloseDate = (DateTime)msg.CloseDate;
                ShiftID = (byte)msg.ShiftID;
                DocCloseShiftDocs = new ObservableCollection<Docs>();
            }
            else
            {
                Spools = new ObservableCollection<Spool>(
                    from sp in DB.GammaBase.GetDocCloseShiftPMSpools(msg.DocID)
                    select new Spool
                    {
                        CharacteristicID = (Guid)sp.CharacteristicID,
                        NomenclatureID = sp.NomenclatureID,
                        Nomenclature = sp.Nomenclature,
                        Number = sp.Number,
                        ProductID = sp.ProductID,
                        Weight = sp.Weight
                    }
                    );
                DocCloseShift = DB.GammaBase.Docs.Include("DocCloseShiftDocs").Where(d => d.DocID == msg.DocID).FirstOrDefault();
                DocCloseShiftDocs = new ObservableCollection<Docs>(DocCloseShift.DocCloseShiftDocs);
                CloseDate = DocCloseShift.Date;
                ShiftID = (byte)DocCloseShift.ShiftID;
                PlaceID = (byte)DocCloseShift.PlaceID;
            }
            ShowSpoolCommand = new RelayCommand(() =>
                MessageManager.OpenDocProduct(new OpenDocProductMessage() 
                {ID = SelectedSpool.ProductID,DocProductKind = DocProductKinds.DocProductSpool, IsNewProduct = false}),
                () => SelectedSpool != null);
        }
        private byte ShiftID { get; set; }
        private DateTime CloseDate { get; set; }
        private int PlaceID { get; set; }
        private Docs DocCloseShift { get; set; }
        private ObservableCollection<Docs> DocCloseShiftDocs { get; set; }
        public void FillGrid()
        {
            ClearGrid();
            DocCloseShiftDocs = new ObservableCollection<Docs>(DB.GammaBase.Docs.
                Where(d => d.PlaceID == PlaceID && d.ShiftID == ShiftID && 
                    d.Date >= DB.GetShiftBeginTime(CloseDate) && d.Date <= DB.GetShiftEndTime(CloseDate) &&
                    (d.DocTypeID == (int)DocTypes.DocProduction || d.DocTypeID == (int)DocTypes.DocWithdrawal)).Select(d => d));
            foreach (var doc in DocCloseShiftDocs)
            {
                if (doc.DocTypeID == (byte)DocTypes.DocProduction)
                {

                    Spools.Add(
                        (from d in DB.GammaBase.DocProducts 
                        join ps in DB.GammaBase.ProductSpools on d.ProductID equals ps.ProductID
                        where d.DocID == doc.DocID
                        select new Spool 
                        { 
                            CharacteristicID = (Guid)ps.C1CCharacteristicID,
                            NomenclatureID = ps.C1CNomenclatureID,
                            Nomenclature = string.Concat(ps.C1CNomenclature.Name," ",ps.C1CCharacteristics.Name),
                            Number = d.Docs.Number,
                            ProductID = d.ProductID,
                            Weight = ps.Weight
                        }).FirstOrDefault()
                    );
                }
            }
        }

        public void ClearGrid()
        {
            if (DocCloseShift != null)
            {
                DocCloseShiftDocs.Clear();
                Spools.Clear();
            }
        }
        public override void SaveToModel(Guid ItemID)
        {
            base.SaveToModel(ItemID);
            if (DocCloseShift == null)
            {
                DocCloseShift = DB.GammaBase.Docs.Where(d => d.DocID == ItemID).Select(d => d).FirstOrDefault();
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
            DB.GammaBase.SaveChanges();
        }
        private ObservableCollection<Spool> _spools = new ObservableCollection<Spool>();
        public ObservableCollection<Spool> Spools
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
        public Spool SelectedSpool { get; set; }
        public RelayCommand ShowSpoolCommand {get; private set;}
    }
}
