// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Interfaces;
using Gamma.Models;
using System.Data.Entity;
using System.Windows;

namespace Gamma.ViewModels
{
    class DocCloseShiftRwGridViewModel: SaveImplementedViewModel, IFillClearGrid, IBarImplemented
    {
        public DocCloseShiftRwGridViewModel(OpenDocCloseShiftMessage msg)
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
                var productIds =
                    GammaBase.Docs.Where(d => d.DocID == msg.DocID)
                        .SelectMany(d => d.DocCloseShiftProducts.Select(dp => dp.ProductID))
                        .ToList();
                Spools = new ObservableCollection<PaperBase>(
                    GammaBase.vProductsInfo.Where(p => productIds.Contains(p.ProductID))
                    .Select(p => new PaperBase 
                    {
                        CharacteristicID = (Guid)p.C1CCharacteristicID,
                        NomenclatureID = p.C1CNomenclatureID,
                        Nomenclature = p.NomenclatureName,
                        Number = p.Number,
                        ProductID = p.ProductID,
                        Weight = p.ProductionQuantity
                    }
                    ));
                UnwinderSpools = new ObservableCollection<PaperBase>(
                    GammaBase.DocCloseShiftRemainders.Where(r => (r.DocID == msg.DocID) && r.IsSourceProduct == true && (r.RemainderTypeID == null || r.RemainderTypeID == 0))
                    .Join(GammaBase.vProductsInfo, r => r.ProductID, p => p.ProductID
                    , (r, p) => new PaperBase
                    {
                        CharacteristicID = (Guid)p.C1CCharacteristicID,
                        NomenclatureID = p.C1CNomenclatureID,
                        Nomenclature = p.NomenclatureName,
                        Number = r.Products.Number,
                        ProductID = (Guid)r.ProductID,
                        Weight = r.Quantity
                    }
                    ));
                DocCloseShift = GammaBase.Docs.Include(d => d.DocCloseShiftDocs).FirstOrDefault(d => d.DocID == msg.DocID);
                //DocCloseShiftDocs = new ObservableCollection<Docs>(DocCloseShift.DocCloseShiftDocs);
                DocCloseDocIds = DocCloseShift.DocCloseShiftDocs.Select(dc => dc.DocID).ToList();
                CloseDate = DocCloseShift.Date;
                ShiftID = (byte)DocCloseShift.ShiftID;
                PlaceID = (byte)DocCloseShift.PlaceID;
            }
            ShowSpoolCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedSpool.ProductID),
                () => SelectedSpool != null);
            ShowUnwinderSpoolCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedUnwinderSpool.ProductID),
                () => SelectedUnwinderSpool != null);
            Bars.Add(ReportManager.GetReportBar("DocCloseShiftDocRw", VMID));
        }

        public bool IsChanged { get; private set; }

        public Guid? VMID { get; } = Guid.NewGuid();

        private byte ShiftID { get; set; }
        private DateTime CloseDate { get; set; }
        private int PlaceID { get; set; }
        private Docs DocCloseShift { get; set; }
        private ObservableCollection<Docs> DocCloseShiftDocs { get; set; }
        private List<Guid> DocCloseDocIds { get; set; }

        public void FillGridWithNoFillEnd()
        {
            throw new NotImplementedException();
        }

        public void FillGrid()
        {
            UIServices.SetBusyState();
            ClearGrid();
            DocCloseDocIds = GammaBase.Docs.
                Where(d => d.PlaceID == PlaceID && d.ShiftID == ShiftID &&
                    d.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    d.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    (d.DocTypeID == (int)DocTypes.DocProduction || d.DocTypeID == (int)DocTypes.DocWithdrawal)
                    && !d.DocCloseShift.Any() && d.IsConfirmed)
                    .Select(d => d.DocID).ToList();
            Spools = new ObservableCollection<PaperBase>(GammaBase.FillDocCloseShiftRwSpools(PlaceID, ShiftID, CloseDate)
                .Select(s => new PaperBase
                {
                    NomenclatureID = s.C1CNomenclatureID,
                    CharacteristicID = s.C1CCharacteristicID,
                    ProductID = s.ProductID,
                    Weight = s.Weight ??0,
                    Nomenclature = s.NomenclatureName,
                    Number = s.Number,
                    DocID = s.DocID
                }));
            UnwinderSpools = new ObservableCollection<PaperBase>(GammaBase.FillDocCloseShiftRwUnwinderSpools(PlaceID, ShiftID, CloseDate)
                .Select(s => new PaperBase
                {
                    NomenclatureID = (Guid)s.C1CNomenclatureID,
                    CharacteristicID = (Guid)s.C1CCharacteristicID,
                    ProductID = (Guid)s.ProductID,
                    Weight = s.Weight,
                    Nomenclature = s.NomenclatureName,
                    Number = s.Number,
                    DocID = (Guid)s.DocID
                }));
            /*
                        foreach (var doc in DocCloseShiftDocs.Where(doc => doc.DocTypeID == (byte)DocTypes.DocProduction))
                        {
                            Spools.Add(
                                (from d in GammaBase.DocProductionProducts
                                 join ps in GammaBase.vProductsInfo on d.ProductID equals ps.ProductID
                                 where d.DocID == doc.DocID
                                 select new PaperBase
                                 {
                                     CharacteristicID = (Guid)ps.C1CCharacteristicID,
                                     NomenclatureID = (Guid)ps.C1CNomenclatureID,
                                     Nomenclature = ps.NomenclatureName,
                                     Number = d.DocProduction.Docs.Number,
                                     ProductID = d.ProductID,
                                     Weight = ps.ProductionQuantity * 1000
                                 }).FirstOrDefault()
                                );
                        }
            */
            IsChanged = true;
        }

        public void ClearGrid()
        {
            //DocCloseShiftDocs.Clear();
            DocCloseDocIds?.Clear();
            Spools?.Clear();
            UnwinderSpools?.Clear();
            SaveToModel();
            IsChanged = true;
        }
        public override bool SaveToModel(Guid itemID)
        {
            if (DocCloseShift == null)
            {
                DocCloseShift = GammaBase.Docs.Include(d => d.DocCloseShiftDocs).Include(d=>d.DocCloseShiftProducts).First(d => d.DocID == itemID);
                foreach (var doc in DocCloseShiftDocs)
                {
                    DocCloseShift.DocCloseShiftDocs.Add(doc);
                }
                //if (DocCloseShift.DocCloseShiftRemainders == null)
                //    DocCloseShift.DocCloseShiftRemainders = new List<DocCloseShiftRemainders>();
            }
            else
            {
                DocCloseShift.DocCloseShiftDocs.Clear();
                DocCloseShift.DocCloseShiftProducts.Clear();

                if (DocCloseDocIds != null)
                {
                    foreach (var doc in DocCloseDocIds)
                    {
                        DocCloseShift.DocCloseShiftDocs.Add(GammaBase.Docs.First(d => d.DocID == doc));
                    }
                }
                //if (DocCloseShift.DocCloseShiftRemainders == null)
                //    DocCloseShift.DocCloseShiftRemainders = new List<DocCloseShiftRemainders>();
                GammaBase.DocCloseShiftRemainders.RemoveRange(DocCloseShift.DocCloseShiftRemainders.Where(p => p.IsSourceProduct == true && (p.RemainderTypeID == null || p.RemainderTypeID == 0) ));//(p => (p.RemainderTypeID ?? 0) != 0));

            }

            foreach (var Product in UnwinderSpools)
            {
                DocCloseShift.DocCloseShiftRemainders.Add(new DocCloseShiftRemainders
                {
                    DocID = itemID,
                    ProductID = Product.ProductID,
                    DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                    RemainderTypeID = 0,
                    Quantity = Product.Quantity,
                    IsMaterial = true,
                    IsSourceProduct = true
                });
            }

            var productIds = Spools.Select(s => s.ProductID).ToList();
            foreach (var product in GammaBase.Products.Where(p => productIds.Contains(p.ProductID)))
            {
                DocCloseShift.DocCloseShiftProducts.Add(product);
            }
            //DocCloseShift.DocCloseShiftProducts.Add( = GammaBase.Products.Where(p => productIds.Contains(p.ProductID)).ToList();
            GammaBase.SaveChanges();
            return true;
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
        public DelegateCommand ShowSpoolCommand { get; private set; }
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

        private ObservableCollection<PaperBase> _unwinderSpools = new ObservableCollection<PaperBase>();
        public ObservableCollection<PaperBase> UnwinderSpools
        {
            get
            {
                return _unwinderSpools;
            }
            set
            {
                _unwinderSpools = value;
                RaisePropertyChanged("UnwinderSpools");
            }
        }
        public PaperBase SelectedUnwinderSpool { get; set; }
        public DelegateCommand ShowUnwinderSpoolCommand { get; private set; }

    }
}
