using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Interfaces;
using System.Collections.ObjectModel;
using Gamma.Models;
using GalaSoft.MvvmLight.Command;

namespace Gamma.ViewModels
{
    class DocCloseShiftWRGridViewModel : SaveImplementedViewModel, IFillClearGrid
    {
        public DocCloseShiftWRGridViewModel(OpenDocCloseShiftMessage msg)
        {
            if (msg.DocID == null)
            {
                PlaceID = (int)msg.PlaceID;
                CloseDate = (DateTime)msg.CloseDate;
                ShiftID = (byte)msg.ShiftID;
                DocCloseShiftDocs = new ObservableCollection<Docs>();
                GroupPacks = new ObservableCollection<PaperBase>();
            }
            else
            {
                GroupPacks = new ObservableCollection<PaperBase>(
                    from sp in DB.GammaBase.GetDocCloseShiftWRGroupPacks(msg.DocID)
                    select new PaperBase
                    {
                        CharacteristicID = (Guid)sp.CharacteristicID,
                        NomenclatureID = (Guid)sp.NomenclatureID,
                        Nomenclature = sp.Nomenclature,
                        Number = sp.Number,
                        ProductID = sp.ProductID,
                        Weight = Convert.ToInt32(sp.Weight)
                    }
                    );
                DocCloseShift = DB.GammaBase.Docs.Include("DocCloseShiftDocs").Where(d => d.DocID == msg.DocID).FirstOrDefault();
                DocCloseShiftDocs = new ObservableCollection<Docs>(DocCloseShift.DocCloseShiftDocs);
                CloseDate = DocCloseShift.Date;
                ShiftID = (byte)DocCloseShift.ShiftID;
                PlaceID = (byte)DocCloseShift.PlaceID;
            }
            ShowGroupPackCommand = new RelayCommand(() =>
                MessageManager.OpenDocProduct(DocProductKinds.DocProductGroupPack, SelectedGroupPack.ProductID),
                () => SelectedGroupPack != null);
        }
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

                    GroupPacks.Add(
                        (from d in DB.GammaBase.DocProducts
                         join ps in DB.GammaBase.ProductGroupPacks on d.ProductID equals ps.ProductID
                         where d.DocID == doc.DocID
                         select new PaperBase
                         {
                             CharacteristicID = (Guid)ps.C1CCharacteristicID,
                             NomenclatureID = (Guid)ps.C1CNomenclatureID,
                             Nomenclature = string.Concat(ps.C1CNomenclature.Name, " ", ps.C1CCharacteristics.Name),
                             Number = d.Docs.Number,
                             ProductID = d.ProductID,
                             Weight = ps.Weight ?? 0
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
                GroupPacks.Clear();
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
        private int PlaceID { get; set; }
        private byte ShiftID { get; set; }
        private ObservableCollection<Docs> DocCloseShiftDocs { get; set; }
        private Docs DocCloseShift { get; set; }
        private DateTime CloseDate { get; set; }
        public RelayCommand ShowGroupPackCommand { get; private set; }
        public PaperBase SelectedGroupPack { get; set; }
        private ObservableCollection<PaperBase> _groupPacks;
        public ObservableCollection<PaperBase> GroupPacks
        {
            get
            {
                return _groupPacks;
            }
            set
            {
            	_groupPacks = value;
                RaisePropertyChanged("GroupPacks");
            }
        }
    }
    
}
