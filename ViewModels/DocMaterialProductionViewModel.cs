// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using DevExpress.Mvvm;
using System;
using System.Linq;
using Gamma.Interfaces;
using System.Collections.ObjectModel;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Models;
using System.Data.Entity.SqlServer;
using System.Windows;
using System.Collections.Generic;

// ReSharper disable MemberCanBePrivate.Global

namespace Gamma.ViewModels
{
    class DocMaterialProductionViewModel : SaveImplementedViewModel
    {
        public DocMaterialProductionViewModel(OpenDocMaterialProductionMessage msg)
        {
            //gammaBase = gammaBase ?? DB.GammaDb;
            Doc = (from d in GammaBase.Docs where d.DocID == msg.DocID select d).FirstOrDefault();
                          
            if (Doc == null)
            {
                Date = DB.CurrentDateTime;
                IsConfirmed = false;
                PlaceID = WorkSession.PlaceID;
                IsNewDoc = true;
                Title = "Остатки сырья и материалов";
                ShiftID = WorkSession.ShiftID;
            }
            else
            {
                Date = Doc.Date;
                Number = Doc.Number;
                IsConfirmed = Doc.IsConfirmed;
                PlaceID = (int)Doc.PlaceID;
                IsNewDoc = false;
                Title = "Остатки сырья и материалов №" + Doc.Number;
                ShiftID = Doc.ShiftID;
            }
            //IsVisibilityRemainder = false;

            var place = GammaBase.Places.Where(p => p.PlaceID == PlaceID).FirstOrDefault();

            ProductionProductsList = new List<Characteristic>(GammaBase.ProductionTaskBatches
                            .Where(r => r.ProductionTaskStateID == (int)ProductionTaskStates.InProduction && r.ProductionTasks.Where(p => p.PlaceGroupID == 0 && p.C1CCharacteristicID != null).FirstOrDefault() != null)
                            .Select(r => new Characteristic
                            {
                                CharacteristicID = (Guid)r.ProductionTasks.Where(p => p.PlaceGroupID == 0 && p.C1CCharacteristicID != null).FirstOrDefault().C1CCharacteristicID,
                                CharacteristicName = r.ProductionTasks.Where(p => p.PlaceGroupID == 0 && p.C1CCharacteristicID != null).FirstOrDefault().C1CNomenclature.Name + " " + r.ProductionTasks.Where(p => p.PlaceGroupID == 0 && p.C1CCharacteristicID != null).FirstOrDefault().C1CCharacteristics.Name
                            }));

            if (!IsNewDoc)
                ActiveProductionProduct = new List<Object>(GammaBase.DocMaterialProducts
                            .Where(r => r.DocID == Doc.DocID)
                            .Select(r => new Characteristic
                            {
                                CharacteristicID = (Guid)r.C1CCharacteristicID,
                                CharacteristicName = r.C1CNomenclature.Name + " " + r.C1CCharacteristics.Name
                            }));

            var productionProductCharacteristicIDs = ProductionProductsList.Count() > 0 ?
                ProductionProductsList.Select(r => r.CharacteristicID).ToList() : new List<Guid>();

            if (!IsNewDoc)
                foreach (var item in GammaBase.DocMaterialProducts
                            .Where(r => r.DocID == Doc.DocID && !productionProductCharacteristicIDs.Contains((Guid)r.C1CCharacteristicID))
                            .Select(r => new Characteristic
                            {
                                CharacteristicID = (Guid)r.C1CCharacteristicID,
                                CharacteristicName = r.C1CNomenclature.Name + " " + r.C1CCharacteristics.Name
                            }))
                ProductionProductsList.Add(item);
            /*ActiveProductionProduct = new List<object>();
            foreach (var item in ProductionProductsList)
                ActiveProductionProduct.Add(item);
                */

            /*var productionProductCharacteristicIDs = ProductionProductsList.Count() > 0 ?
                ProductionProductsList.Select(r => r.CharacteristicID).ToList() : new List<Guid>() ;
                */
            var activeProductionProductCharacteristicIDs =  new List<Guid>();
            if (!IsNewDoc)
                foreach (object item in ((List<object>)ActiveProductionProduct))
                    activeProductionProductCharacteristicIDs.Add(((Characteristic)item).CharacteristicID);

            CurrentViewModelGrid = Doc == null
                       ? new DocMaterialProductionGridViewModel(PlaceID, (int)ShiftID, Date)
                       : new DocMaterialProductionGridViewModel(PlaceID, (int)ShiftID, Date, Doc.DocID, IsConfirmed, productionProductCharacteristicIDs);

            var grid = CurrentViewModelGrid as IFillClearGrid;
            if (grid != null)
            {
                FillGridCommand = new DelegateCommand(grid.FillGrid, () => !IsConfirmed && CanEditable() && ActiveProductionProduct?.Count > 0);
                ClearGridCommand = new DelegateCommand(grid.ClearGrid, () => !IsConfirmed && CanEditable() && ActiveProductionProduct?.Count > 0);
                FillGridWithNoEndCommand = new DelegateCommand(grid.FillGridWithNoFillEnd, () => !IsConfirmed && FillGridWithNoEndCanEnable() && ActiveProductionProduct?.Count > 0);
            }
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
        }


        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != (CurrentViewModelGrid as IBarImplemented)?.VMID) return;
            //if (!IsValid) return;
            if (CanSaveExecute())
                SaveToModel();
            else if (IsNewDoc) return;
            ReportManager.PrintReport(msg.ReportID, Doc.DocID);
        }
        private Docs Doc { get; set; }
        private bool IsNewDoc { get; set; }
        private int PlaceID { get; set; }
        public string Number { get; set; }
        /// <summary>
        /// Окончание смены
        /// </summary>
        private DateTime ShiftEndTime { get; set; }
        private DateTime _date;
        public DateTime Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
                ShiftEndTime = DB.GetShiftEndTimeFromDate(Date.AddHours(-1));
            }
        }

        public bool IsConfirmed { get; set; }
        public byte? ShiftID { get; set; }

        private List<Object> _activeProductionProduct { get; set; }
        public List<Object> ActiveProductionProduct
        {
            get { return _activeProductionProduct; }
            set
            {
                _activeProductionProduct = value;
                var grid = CurrentViewModelGrid as DocMaterialProductionGridViewModel;
                if (grid != null && value != null)
                {
                    var activeProductionProductCharacteristicIDs = new List<Guid>();
                    foreach (object item in ((List<object>)value))
                        activeProductionProductCharacteristicIDs.Add(((Characteristic)item).CharacteristicID);
                    grid.DocMaterialCompositionCalculations.SetProductionProductCharacteristics(activeProductionProductCharacteristicIDs);
                    grid.DocMaterialProductionDirectCalculationsGrid?.DirectCalculationMaterials?.SetProductionProductCharacteristics(activeProductionProductCharacteristicIDs);
                }
                RaisePropertyChanged("ActiveProductionProduct");
            }

        }

        private List<Characteristic> _productionProductsList { get; set; }
        public List<Characteristic> ProductionProductsList
        {
            get { return _productionProductsList; }
            set
            {
                _productionProductsList = value;
                RaisePropertyChanged("ProductionProductsList");
            }
        }

        public bool IsDateReadOnly
        {
            get
            { return !(!IsNewDoc && !IsConfirmed && DB.HaveWriteAccess("DocMaterialProductions") && WorkSession.ShiftID == 0); }
        }


        public bool CanEditable ()
        {
#if DEBUG
            return DB.HaveWriteAccess("DocMaterialProductions");
#else
            return DB.HaveWriteAccess("DocMaterialProductions") && (WorkSession.ShiftID == 0 || (WorkSession.ShiftID == ShiftID && WorkSession.PlaceID == PlaceID)) && (DB.CurrentDateTime < ShiftEndTime.AddHours(1));
#endif
        }

        public bool FillGridWithNoEndCanEnable()
        {
            return DB.HaveWriteAccess("DocMaterialProductions") && WorkSession.ShiftID == 0;
        }

        private SaveImplementedViewModel _currentViewModelGrid;
        public SaveImplementedViewModel CurrentViewModelGrid 
        {
            get
            {
                return _currentViewModelGrid;
            }
            set
            {
                _currentViewModelGrid = value;
                //Bars = (_currentViewModelGrid as IBarImplemented).Bars;
            }
        }

        public DelegateCommand FillGridCommand { get; set; }
        public DelegateCommand FillGridWithNoEndCommand { get; set; }
        public DelegateCommand ClearGridCommand { get; set; }

        public override bool SaveToModel()
        {
            if (!CanSaveExecute()) return false;
            if (IsNewDoc)
            {
                Doc = new Docs()
                {
                    DocID = SqlGuidUtil.NewSequentialid(),
                    DocTypeID = (byte)DocTypes.DocMaterialProduction,
                    Date = Date,
                    UserID = WorkSession.UserID,
                    PlaceID = WorkSession.PlaceID,
                    ShiftID = WorkSession.ShiftID,
                    IsConfirmed = IsConfirmed,
                    PrintName = WorkSession.PrintName,
                    PersonGuid = WorkSession.PersonID.ToString() == "00000000-0000-0000-0000-000000000000" ? null : WorkSession.PersonID
                };
                GammaBase.Docs.Add(Doc);
            }
            //var utilizationProductsBeforeSave = GammaBase.DocMaterialProductionUtilizationProducts.Where(d => d.DocID == Doc.DocID && d.Docs.IsConfirmed).ToList();
            var isConfirmedPrev = Doc.IsConfirmed;
            Doc.IsConfirmed = IsConfirmed;

            if (Doc.Date != Date)
                Doc.Date = Date;

            List<Guid> activeProductionProductCharacteristicIds = new List<Guid>();
            if (ActiveProductionProduct != null)
                foreach (object item in ((List<object>)ActiveProductionProduct))
                    activeProductionProductCharacteristicIds.Add(((Characteristic)item).CharacteristicID);
            //gammaBase.DocShipmentOrderPersons.RemoveRange(gammaBase.DocShipmentOrderPersons.Where(p => !p.IsInActive && p.Persons.PlaceID != OutPlaceId && p.DocOrderID == DocShipmentOrderID && !outActivePersonIds.Contains(p.PersonID)));
            GammaBase.DocMaterialProducts.RemoveRange(GammaBase.DocMaterialProducts.Where(p => p.DocID == Doc.DocID));
            foreach (var productCharacteristicItem in GammaBase.C1CCharacteristics.Where(p => activeProductionProductCharacteristicIds.Contains(p.C1CCharacteristicID)))
            {
                var docMaterialProduct = new DocMaterialProducts
                {
                    DocMaterialProductID = SqlGuidUtil.NewSequentialid(),
                    DocID = Doc.DocID,
                    C1CNomenclatureID = productCharacteristicItem.C1CNomenclatureID,
                    C1CCharacteristicID = productCharacteristicItem.C1CCharacteristicID
                };
                GammaBase.DocMaterialProducts.Add(docMaterialProduct);
            }

            GammaBase.SaveChanges();

            if (IsNewDoc || !isConfirmedPrev || !IsConfirmed)
            {
                CurrentViewModelGrid?.SaveToModel(Doc.DocID);
            }
            IsNewDoc = false;

            Messenger.Default.Send(new RefreshMessage { });
            return true;
        }

        private ObservableCollection<BarViewModel> _bars;
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
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Title { get; set; }
        public override bool CanSaveExecute()
        {
            return base.CanSaveExecute() && DB.HaveWriteAccess("DocMaterialProductions");
        }

        protected override void SaveToModelAndClose()
        {
            base.SaveToModelAndClose();
            Messenger.Default.Send(new CloseMessage());
        }
    }
}
