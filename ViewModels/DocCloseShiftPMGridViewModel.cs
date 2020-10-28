// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Interfaces;
using Gamma.Models;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows;
using System.Collections;

namespace Gamma.ViewModels
{
    public class DocCloseShiftPMGridViewModel : SaveImplementedViewModel, ICheckedAccess, IBarImplemented, IFillClearGrid
    {
        /// <summary>
        /// Создает новый экземпляр модели для грида закрытия смены БДМ
        /// </summary>
        public DocCloseShiftPMGridViewModel()
        {
            Bars.Add(ReportManager.GetReportBar("DocCloseShiftDocPM", VMID));
            PlaceID = WorkSession.PlaceID;
            ShiftID = WorkSession.ShiftID;

            AddBeginSpoolCommand = new DelegateCommand<RemainderType>(AddSpool, !IsReadOnly);
            DeleteBeginSpoolCommand = new DelegateCommand<RemainderType>(DeleteSpool, !IsReadOnly);
            AddEndSpoolCommand = new DelegateCommand<RemainderType>(AddSpool, !IsReadOnly);
            DeleteEndSpoolCommand = new DelegateCommand<RemainderType>(DeleteSpool, !IsReadOnly);
            AddUtilizationSpoolCommand = new DelegateCommand(AddUtilizationSpool, () => !IsReadOnly);
            DeleteUtilizationSpoolCommand = new DelegateCommand(DeleteUtilizationSpool, () => !IsReadOnly);

            ShowSpoolCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedSpool.ProductID),
                () => SelectedSpool != null);
            ShowBeginSpoolCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedBeginSpool.ProductID),
                () => SelectedBeginSpool != null);
            ShowEndSpoolCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedEndSpool.ProductID),
                () => SelectedEndSpool != null);
            ShowUtilizationSpoolCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedSpool.ProductID),
                () => SelectedUtilizationSpool != null);

        }

        public DocCloseShiftPMGridViewModel(DateTime closeDate):this()
        {
            PlaceID = WorkSession.PlaceID;
            ShiftID = WorkSession.ShiftID;
            CloseDate = closeDate;

            IsWithdrawalMaterial = GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID != 0).First();
            WithdrawalMaterialsGrid = new DocCloseShiftWithdrawalMaterialViewModel(PlaceID, ShiftID, CloseDate);
            WithdrawalMaterialsGrid.SelectedMaterialTabIndex = 7;
        }

        public DocCloseShiftPMGridViewModel(Guid docId) : this()
        {
            using (var gammaBase = DB.GammaDb)
            {
                Spools = new ObservableCollection<PaperBase>(gammaBase.GetDocCloseShiftPMSpools(docId).Select(sp => new PaperBase()
                {
                    CharacteristicID = (Guid)sp.CharacteristicID,
                    NomenclatureID = sp.NomenclatureID,
                    Nomenclature = sp.Nomenclature,
                    Number = sp.Number,
                    ProductID = sp.ProductID,
                    Weight = sp.Weight ?? 0
                }));
                DocId = docId;
                var doc = gammaBase.Docs.Include(d => d.DocCloseShiftDocs).First(d => d.DocID == docId);
                DocCloseDocIds = doc.DocCloseShiftDocs.Select(dc => dc.DocID).ToList();
                PlaceID = (int)doc.PlaceID;
                ShiftID = doc.ShiftID ?? 0;
                CloseDate = doc.Date;
                IsConfirmed = doc.IsConfirmed;
                //Получение списка материалов
                //productionProductCharacteristicIDs = new List<Guid>(Spools
                //    .Select(p => p.CharacteristicID).Distinct().ToList());
                IsWithdrawalMaterial = GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID != 0).First();
                var spools = new List<DocCloseShiftWithdrawalMaterial.Product>(Spools.Select(x => new DocCloseShiftWithdrawalMaterial.Product()
                {
                    ProductID = x.ProductID,
                    CharacteristicID = x.CharacteristicID
                })).ToList();
                WithdrawalMaterialsGrid = new DocCloseShiftWithdrawalMaterialViewModel(PlaceID, ShiftID, CloseDate, DocId, spools, IsConfirmed, productionProductCharacteristicIDs);
                WithdrawalMaterialsGrid.SelectedMaterialTabIndex = 7;

                BeginSpools = new ItemsChangeObservableCollection<DocCloseShiftRemainder>(gammaBase.DocCloseShiftRemainders
                    //.Include(dr => dr.DocCloseShifts)
                    .Where(d => d.DocID == docId && (d.RemainderTypes.RemainderTypeID == 1 || d.RemainderTypes.RemainderTypeID == 3))
                    .Select(d => new DocCloseShiftRemainder()
                    {
                        ProductID = (Guid)d.ProductID,
                        StateID = d.StateID,
                        Quantity = d.Quantity,
                        RemainderTypeID = d.RemainderTypeID
                    }));
                EndSpools = new ItemsChangeObservableCollection<DocCloseShiftRemainder>(gammaBase.DocCloseShiftRemainders
                    //.Include(dr => dr.DocCloseShifts)
                    .Where(d => d.DocID == docId && d.RemainderTypes.RemainderTypeID == 2)
                    .Select(d => new DocCloseShiftRemainder()
                    {
                        ProductID = (Guid)d.ProductID,
                        StateID = d.StateID,
                        Quantity = d.Quantity,
                        RemainderTypeID = d.RemainderTypeID
                    }));
                // Получение переходных остатков
                NomenclatureRests = new ObservableCollection<Sample>(gammaBase.DocCloseShiftNomenclatureRests.Where(dr => dr.DocID == docId)
                    .Select(dr => new Sample
                    {
                        NomenclatureID = dr.C1CNomenclatureID,
                        CharacteristicID = dr.C1CCharacteristicID,
                        Quantity = dr.Quantity / dr.C1CCharacteristics.C1CMeasureUnitsPackage.Coefficient ?? 1,
                        NomenclatureName = dr.C1CNomenclature.Name + " " + dr.C1CCharacteristics.Name
                    }));
                //Получение списка утилизированной продукции
                UtilizationSpools = new ObservableCollection<PaperBase>(gammaBase.DocCloseShiftUtilizationProducts.Where(d => d.DocID == docId)
                    .Join(gammaBase.vProductsInfo, d => d.ProductID, p => p.ProductID
                    , (d, p) => new PaperBase()
                    {
                        CharacteristicID = (Guid)p.C1CCharacteristicID,
                        NomenclatureID = p.C1CNomenclatureID,
                        Nomenclature = p.NomenclatureName,
                        Number = p.Number,
                        ProductID = d.ProductID,
                        Weight = d.Quantity ?? 0
                    }));
                // Получение отходов
                Wastes = new ObservableCollection<Sample>(gammaBase.DocCloseShiftWastes.Where(dw => dw.DocID == docId)
                    .Select(dw => new Sample
                    {
                        NomenclatureID = dw.C1CNomenclatureID,
                        CharacteristicID = dw.C1CCharacteristicID,
                        Quantity = dw.Quantity,
                        NomenclatureName = dw.C1CNomenclature.Name + " " + dw.C1CCharacteristics.Name ?? "",
                        MeasureUnitId = dw.C1CMeasureUnitID,
                        MeasureUnit = dw.C1CMeasureUnits.Name
                    }));
                foreach (var waste in Wastes)
                {
                    waste.MeasureUnits = GetWasteMeasureUnits(waste.NomenclatureID);
                }

                InSpools = new ObservableCollection<MovementProduct>(gammaBase.DocCloseShiftMovementProducts.Where(d => d.DocID == docId && (bool)d.IsMovementIn)
                    .Join(gammaBase.vProductsInfo, d => d.ProductID, p => p.ProductID
                    , (d, p) => new MovementProduct
                    {
                        NomenclatureName = p.NomenclatureName,
                        Number = d.Products.Number,
                        ProductId = d.ProductID,
                        Quantity = d.Quantity ?? 0,
                        ProductKindName = d.Products.ProductKinds.Name,
                        //OrderTypeName = p.OrderTypeName,
                        DocMovementId = d.DocMovementID,
                        OutPlaceName = d.MovementPlaceName,
                        OutPlaceZoneName = d.MovementPlaceZoneName,
                        OutDate = d.DateMovement
                    }));

                OutSpools = new ObservableCollection<MovementProduct>(gammaBase.DocCloseShiftMovementProducts.Where(d => d.DocID == docId && !((bool)d.IsMovementIn))
                    .Join(gammaBase.vProductsInfo, d => d.ProductID, p => p.ProductID
                    , (d, p) => new MovementProduct
                    {
                        NomenclatureName = p.NomenclatureName,
                        Number = d.Products.Number,
                        ProductId = d.ProductID,
                        Quantity = d.Quantity ?? 0,
                        ProductKindName = d.Products.ProductKinds.Name,
                        //OrderTypeName = p.OrderTypeName,
                        DocMovementId = d.DocMovementID,
                        InPlaceName = d.MovementPlaceName,
                        InPlaceZoneName = d.MovementPlaceZoneName,
                        InDate = d.DateMovement
                    }));

            }
        }

        public bool IsChanged { get; private set; }

        private bool IsConfirmed { get; set; }
        private List<Guid> DocCloseDocIds { get; set; }
        public bool IsReadOnly => !DB.HaveWriteAccess("DocCloseShiftWithdrawals") || IsConfirmed;
        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();
        public Guid? VMID { get; } = Guid.NewGuid();
        public PaperBase SelectedSpool { get; set; }
        public DocCloseShiftRemainder SelectedBeginSpool { get; set; }
        public DocCloseShiftRemainder SelectedEndSpool { get; set; }
        private RemainderType CurrentAddSpoolRemainder { get; set; }
        public PaperBase SelectedUtilizationSpool { get; set; }

        
        private DocCloseShiftWithdrawalMaterialViewModel _withdrawalMaterialsGrid;
        public DocCloseShiftWithdrawalMaterialViewModel WithdrawalMaterialsGrid
        {
            get
            {
                return _withdrawalMaterialsGrid;
            }
            set
            {
                _withdrawalMaterialsGrid = value;
                //Bars = (_currentViewModelGrid as IBarImplemented).Bars;
            }
        }

        public void FillGrid()
        {
            FillGridWithParameter(true);
        }

        public void FillGridWithNoFillEnd()
        {
            FillGridWithParameter(false);
        }

        public void FillGridWithParameter(bool IsFillEnd = true)
        {
            UIServices.SetBusyState();
            Clear(IsFillEnd);
            using (var gammaBase = DB.GammaDb)
            {
                var existNonConfirmedDocMaterialProductions = gammaBase.GetDocMaterialProductionsOnShift(PlaceID, ShiftID, CloseDate)
                    .Where(m => !m.IsConfirmed)
                    .Count();

                if (existNonConfirmedDocMaterialProductions > 0)
                {
                    GammaBase.CriticalLogs.Add(new CriticalLogs { LogID = SqlGuidUtil.NewSequentialid(), LogDate = DB.CurrentDateTime, LogUserID = WorkSession.UserName, Log = "Заполнение Рапорта закрытия смены @PlaceID " + PlaceID.ToString() + ", @ShiftID " + ShiftID.ToString() + ", @Date " + CloseDate.ToString() + " Есть Неподтвержденный документ Расхода сырья и материалов за смену.Требуется подтвердить или удалить, иначе материалы будут рассчитаны неправильно!" });
                    MessageBox.Show("Есть Неподтвержденный документ Расхода сырья и материалов за смену."+Environment.NewLine+"Требуется подтвердить или удалить, иначе материалы будут рассчитаны неправильно!",
                        "Рапорт за смену", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DocCloseDocIds = gammaBase.Docs.
                Where(d => d.PlaceID == PlaceID && d.ShiftID == ShiftID && d.IsConfirmed &&
                    d.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    d.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    (d.DocTypeID == (int)DocTypes.DocProduction || d.DocTypeID == (int)DocTypes.DocWithdrawal)).Select(d => d.DocID).ToList();
                Spools = new ObservableCollection<PaperBase>(gammaBase.FillDocCloseShiftPMSpools(PlaceID, ShiftID, CloseDate).Select(p => new PaperBase()
                {
                    CharacteristicID = p.CharacteristicID,
                    NomenclatureID = p.NomenclatureID,
                    Nomenclature = p.NomenclatureName,
                    Number = p.Number,
                    ProductID = p.ProductID,
                    Weight = (p.Quantity ?? 0) * 1000
                }));
                NomenclatureRests = new ObservableCollection<Sample>(Spools
                    .Select(p => new Sample
                    {
                        NomenclatureID = p.NomenclatureID,
                        CharacteristicID = p.CharacteristicID,
                        Quantity = 0,
                        NomenclatureName = p.Nomenclature,
                    }).Distinct());
                //productionProductCharacteristicIDs = new List<Guid>(Spools
                //    .Select(p => p.CharacteristicID).Distinct().ToList());

                //DocCloseShiftWithdrawalMaterials.FillWithdrawalMaterials(productionProductCharacteristicIDs);
                var spools = new List<DocCloseShiftWithdrawalMaterial.Product>(Spools.Select(x => new DocCloseShiftWithdrawalMaterial.Product()
                {
                    ProductID = x.ProductID,
                    CharacteristicID = x.CharacteristicID
                })).ToList();
                WithdrawalMaterialsGrid.FillWithdrawalMaterials(productionProductCharacteristicIDs, spools);
                

                var wastes = new ItemsChangeObservableCollection<Sample>(
                    gammaBase.FillDocCloseShiftPMWastes(PlaceID, ShiftID, CloseDate)
                    .Select(w => new Sample
                    {
                        NomenclatureID = (Guid)w.NomenclatureID,
                        CharacteristicID = w.CharacteristicID,
                        Quantity = w.Quantity ?? 0,
                        NomenclatureName = w.NomenclatureName,
                        MeasureUnitId = w.MeasureUnitID,
                        MeasureUnit = w.MeasureUnit
                    }));
                foreach (var waste in wastes)
                {
                    waste.MeasureUnits = GetWasteMeasureUnits(waste.NomenclatureID);
                    waste.MeasureUnitId = waste.MeasureUnits.FirstOrDefault().Key;
                }
                Wastes = wastes;

                if (BeginSpools == null || BeginSpools?.Count() == 0)
                {
                    var PreviousDocCloseShift = gammaBase.Docs
                        .Where(d => d.DocTypeID == 3 && d.PlaceID == PlaceID && d.Date < SqlFunctions.DateAdd("mi", -1, CloseDate))
                        .OrderByDescending(d => d.Date)
                        .FirstOrDefault();
                    BeginSpools = new ObservableCollection<DocCloseShiftRemainder>(gammaBase.DocCloseShiftRemainders
                        .Where(d => (d.RemainderTypeID == 2 || d.RemainderTypeID == 0 || d.RemainderTypeID == null) && d.DocCloseShifts.DocID == PreviousDocCloseShift.DocID)
                        .Select(d => new DocCloseShiftRemainder
                        {
                            ProductID = (Guid)d.ProductID,
                            StateID = d.StateID,
                            Quantity = d.Quantity,
                            RemainderTypeID = d.RemainderTypeID == 0 || d.RemainderTypeID == null ? 3 : 1
                        }));
                }

                FillEndSpools(IsFillEnd);

                InSpools = new ObservableCollection<MovementProduct>(gammaBase.FillDocCloseShiftMovementProducts(PlaceID, ShiftID, CloseDate)
                            .Where(d => (bool)d.IsMovementIn).Select(d => new MovementProduct
                            {
                                NomenclatureName = d.NomenclatureName,
                                Number = d.Number,
                                ProductId = d.ProductID,
                                Quantity = d.Quantity ?? 0,
                                ProductKindName = d.ProductKindName,
                                OrderTypeName = d.OrderTypeName,
                                DocMovementId = d.DocMovementID,
                                OutPlaceName = d.OutPlace,
                                OutPlaceZoneName = d.OutPlaceZone,
                                OutDate = d.OutDate
                            }));

                OutSpools = new ObservableCollection<MovementProduct>(gammaBase.FillDocCloseShiftMovementProducts(PlaceID, ShiftID, CloseDate)
                            .Where(d => (bool)d.IsMovementOut).Select(d => new MovementProduct
                            {
                                NomenclatureName = d.NomenclatureName,
                                Number = d.Number,
                                ProductId = d.ProductID,
                                Quantity = d.Quantity ?? 0,
                                ProductKindName = d.ProductKindName,
                                OrderTypeName = d.OrderTypeName,
                                DocMovementId = d.DocMovementID,
                                InPlaceName = d.InPlace,
                                InPlaceZoneName = d.InPlaceZone,
                                InDate = d.InDate
                            }));

                var utilizationSpools = new ObservableCollection<PaperBase>(gammaBase.FillDocCloseShiftUtilizationSpools(PlaceID, ShiftID, CloseDate).Select(p => new PaperBase()
                {
                    CharacteristicID = p.CharacteristicID,
                    NomenclatureID = p.NomenclatureID,
                    Nomenclature = p.NomenclatureName,
                    Number = p.Number,
                    ProductID = p.ProductID,
                    Weight = (p.Quantity ?? 0) * 1000
                }));

                foreach (PaperBase spool in utilizationSpools)
                {
                    if (UtilizationSpools.Count(d => d.ProductID == spool.ProductID) == 0)
                    {
                        UtilizationSpools.Add(new PaperBase()
                        {
                            NomenclatureID = spool.NomenclatureID,
                            CharacteristicID = spool.CharacteristicID,
                            Nomenclature = spool.Nomenclature,
                            Number = spool.Number,
                            ProductID = spool.ProductID,
                            Weight = spool.Weight
                        });
                    }
                    else
                    {
                        var item = UtilizationSpools.FirstOrDefault(d => d.ProductID == spool.ProductID);
                        if (item != null)
                        {
                            item.Weight = spool.Weight;
                            item.NomenclatureID = spool.NomenclatureID;
                            item.CharacteristicID = spool.CharacteristicID;
                            item.Nomenclature = spool.Nomenclature;
                        }
                    }
                }

                IsChanged = true;
            }
        }

        private void FillEndSpools(bool IsFillEnd = true)
        {
            if (IsFillEnd)
            {
                using (var gammaBase = DB.GammaDb)
                {
                    EndSpools?.Clear();

                    EndSpools = new ItemsChangeObservableCollection<DocCloseShiftRemainder>(gammaBase.Rests
                        .Where(d => d.PlaceID == PlaceID && WorkSession.BranchID != 2).Join(gammaBase.vProductsCurrentStateInfo, d => d.ProductID, p => p.ProductID
                        , (d, p) => new DocCloseShiftRemainder
                        {
                            ProductID = (Guid)d.ProductID,
                            StateID = (p.StateID == null ? 0 : p.StateID),
                            Quantity = (d.Products.ProductSpools.DecimalWeight == null ? 0 : d.Products.ProductSpools.DecimalWeight) * 1000,
                            RemainderTypeID = 2
                        }));

                    //убираем из остатков тамбур, который утилизирован в этой смене
                    if (UtilizationSpools != null)
                    {
                        foreach (var spool in UtilizationSpools)
                        {
                            var removeSpool = EndSpools.FirstOrDefault(d => d.ProductID == spool.ProductID);
                            if (removeSpool != null)
                                EndSpools.Remove(removeSpool);
                        }
                    }

                    //убираем из остатков переходящий следующей смене тамбур в процессе производства
                    var doc = gammaBase.Docs.Where(d => d.DocTypeID == 3 && d.PlaceID == PlaceID && d.ShiftID == ShiftID && d.Date == CloseDate).FirstOrDefault();
                    if (doc != null)
                    {
                        var endSpoolsProductIDs = EndSpools.Select(d => (Guid?)d.ProductID).ToList();
                        var spoolRemainders = gammaBase.DocCloseShiftRemainders.Where(r => (r.RemainderTypeID ?? 0) == 0 && r.DocID == doc.DocID && endSpoolsProductIDs.Contains(r.ProductID));
                        foreach (var spool in spoolRemainders)
                        {
                            var removeSpool = EndSpools.FirstOrDefault(d => d.ProductID == spool.ProductID);
                            if (removeSpool != null)
                                EndSpools.Remove(removeSpool);
                        }
                    }
                }
            }
        }

        private Dictionary<Guid, string> GetWasteMeasureUnits(Guid nomenclatureId)
        {
            Dictionary<Guid, string> dict;
            using (var gammaBase = DB.GammaDb)
            {
                dict = gammaBase.C1CMeasureUnits.Where(mu => mu.C1CNomenclatureID == nomenclatureId).OrderBy(mu => mu.Coefficient).ToDictionary(x => x.C1CMeasureUnitID, v => v.Name);
            }
            return dict.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        public DelegateCommand ShowSpoolCommand { get; private set; }
        public DelegateCommand ShowBeginSpoolCommand { get; private set; }
        public DelegateCommand ShowEndSpoolCommand { get; private set; }
        public DelegateCommand ShowUtilizationSpoolCommand { get; private set; }
        public DelegateCommand<RemainderType> AddBeginSpoolCommand { get; private set; }
        public DelegateCommand<RemainderType> DeleteBeginSpoolCommand { get; private set; }
        public DelegateCommand<RemainderType> AddEndSpoolCommand { get; private set; }
        public DelegateCommand<RemainderType> DeleteEndSpoolCommand { get; private set; }
        public DelegateCommand AddUtilizationSpoolCommand { get; private set; }
        public DelegateCommand DeleteUtilizationSpoolCommand { get; private set; }


        

        
        private Guid DocId { get; set; }
        private int PlaceID { get; set; }
        private int ShiftID { get; set; }
        private DateTime CloseDate { get; set; }

        public bool IsWithdrawalMaterial { get; set; }

        public void ClearGrid() => Clear(true);
        

        public void Clear(bool IsFillEnd = true)
        {
            Spools?.Clear();
            DocCloseDocIds?.Clear();

            WithdrawalMaterialsGrid?.Clear();
            BeginSpools?.Clear();
            if (IsFillEnd) EndSpools?.Clear();
            InSpools?.Clear();
            OutSpools?.Clear();
            IsChanged = true;
            Wastes?.Clear();
            NomenclatureRests?.Clear();
            //            gammaBase.SaveChanges();           
        }

        private List<Guid> productionProductCharacteristicIDs { get; set; }

        public ObservableCollection<Sample> Wastes
        {
            get { return _wastes; }
            set
            {
                _wastes = value;
                RaisePropertyChanged("Wastes");
            }
        }

        private ObservableCollection<Sample> _nomenclatureRests;

        public ObservableCollection<Sample> NomenclatureRests
        {
            get { return _nomenclatureRests; }
            set
            {
                _nomenclatureRests = value;
                RaisePropertyChanged("NomenclatureRests");
            }
        }


        private ObservableCollection<Sample> _wastes;

        private ObservableCollection<PaperBase> _spools = new ObservableCollection<PaperBase>();
        public ObservableCollection<PaperBase> Spools
        {
            get { return _spools; }
            set
            {
                _spools = value;
                productionProductCharacteristicIDs = new List<Guid>(_spools
                    .Select(p => p.CharacteristicID).Distinct().ToList());
                RaisePropertiesChanged("Spools");
            }
        }

        private ObservableCollection<PaperBase> _utilizationSpools = new ObservableCollection<PaperBase>();
        public ObservableCollection<PaperBase> UtilizationSpools
        {
            get { return _utilizationSpools; }
            set
            {
                _utilizationSpools = value;
                RaisePropertiesChanged("UtilizationSpools");
            }
        }

        private ObservableCollection<MovementProduct> _inSpools = new ObservableCollection<MovementProduct>();
        public ObservableCollection<MovementProduct> InSpools
        {
            get { return _inSpools; }
            set
            {
                _inSpools = value;
                RaisePropertiesChanged("InSpools");
            }
        }

        private ObservableCollection<MovementProduct> _outSpools = new ObservableCollection<MovementProduct>();
        public ObservableCollection<MovementProduct> OutSpools
        {
            get { return _outSpools; }
            set
            {
                _outSpools = value;
                RaisePropertiesChanged("OutSpools");
            }
        }

        private ObservableCollection<DocCloseShiftRemainder> _beginSpools = new ObservableCollection<DocCloseShiftRemainder>();
        public ObservableCollection<DocCloseShiftRemainder> BeginSpools
        {
            get { return _beginSpools; }
            set
            {
                _beginSpools = value;
                RaisePropertiesChanged("BeginSpools");
            }
        }

        private ObservableCollection<DocCloseShiftRemainder> _endSpools = new ObservableCollection<DocCloseShiftRemainder>();
        public ObservableCollection<DocCloseShiftRemainder> EndSpools
        {
            get { return _endSpools; }
            set
            {
                _endSpools = value;
                RaisePropertiesChanged("EndSpools");
            }
        }

        private void AddSpool(RemainderType unum)
        {
            CurrentAddSpoolRemainder = unum;
            Messenger.Default.Register<ChoosenProductMessage>(this, AddSpoolSelected);
            MessageManager.OpenFindProduct(ProductKind.ProductSpool, true);
        }

        private void AddSpoolSelected(ChoosenProductMessage msg)
        {
            Messenger.Default.Unregister<ChoosenProductMessage>(this);
            using (var gammaBase = DB.GammaDb)
            {
                var isWrittenOff = gammaBase.vProductsInfo.Where(p => p.ProductID == msg.ProductID).Select(p => p.IsWrittenOff).FirstOrDefault();
                if (isWrittenOff ?? false && CurrentAddSpoolRemainder == RemainderType.End)
                {
                    MessageBox.Show("Нельзя добавить в остаток на конец смены списанный тамбур", "Списанный тамбур", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }
                var quantity = gammaBase.ProductSpools.Where(p => p.ProductID == msg.ProductID).Select(p => p.DecimalWeight).FirstOrDefault();
                switch (CurrentAddSpoolRemainder)
                {
                    case RemainderType.Begin:
                        BeginSpools.Add(new DocCloseShiftRemainder()
                        {
                            ProductID = msg.ProductID,
                            Quantity = quantity * 1000
                        });
                        break;
                    case RemainderType.End:
                        EndSpools.Add(new DocCloseShiftRemainder()
                        {
                            ProductID = msg.ProductID,
                            Quantity = quantity * 1000
                        });
                        break;

                }
            }
        }

        private void DeleteSpool(RemainderType unum)
        {
            CurrentAddSpoolRemainder = unum;
            switch (CurrentAddSpoolRemainder)
            {
                case RemainderType.Begin:
                    if (SelectedBeginSpool == null) return;
                    BeginSpools.Remove(SelectedBeginSpool);
                    break;
                case RemainderType.End:
                    if (SelectedEndSpool == null) return;
                    EndSpools.Remove(SelectedEndSpool);
                    break;

            }
        }

        private void AddUtilizationSpool()
        {
            Messenger.Default.Register<ChoosenProductMessage>(this, AddUtilizationSpoolSelected);
            MessageManager.OpenFindProduct(ProductKind.ProductSpool, true);
        }

        private void AddUtilizationSpoolSelected(ChoosenProductMessage msg)
        {
            Messenger.Default.Unregister<ChoosenProductMessage>(this);
            using (var gammaBase = DB.GammaDb)
            {
                var productBroke = gammaBase.DocBrokeDecisionProducts.Where(p => p.ProductID == msg.ProductID && p.StateID == 2).OrderByDescending(p => p.DocBroke.Docs.Date).Take(1).FirstOrDefault();
                if (productBroke?.Quantity == null)
                {
                    var dlgResult = MessageBox.Show("По тамбуру не принято решение на утилизацию. Вы уверены?", "Тамбур на утилизацию", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (dlgResult != MessageBoxResult.Yes)
                        return;
                }

                var brokeQuantity = productBroke?.Quantity;
                var product = new ObservableCollection<PaperBase>(gammaBase.vProductsInfo
                    .Where(p => p.ProductID == msg.ProductID)
                    .Select(p => new PaperBase()
                {
                    CharacteristicID = (Guid)p.C1CCharacteristicID,
                    NomenclatureID = p.C1CNomenclatureID,
                    Nomenclature = p.NomenclatureName,
                    Number = p.Number,
                    ProductID = p.ProductID,
                    Weight = (brokeQuantity ?? p.ProductionQuantity) * 1000
                }));
                foreach (var item in product)
                {
                    UtilizationSpools.Add(item);
                    var remainderSpool = EndSpools.FirstOrDefault(s => s.ProductID == item.ProductID && (s.RemainderTypeID ?? 0) == 2);
                    if (remainderSpool != null)
                        EndSpools.Remove(remainderSpool);
                }
            }
        }

        private void DeleteUtilizationSpool()
        {
            if (SelectedUtilizationSpool == null) return;
            UtilizationSpools.Remove(SelectedUtilizationSpool);
            FillEndSpools();
        }

        /// <summary>
        /// Сохранение в БД
        /// </summary>
        /// <param name="docId">ID документа закрытия смены</param>
        public override bool SaveToModel(Guid docId)
        {
            if (IsReadOnly) return true;
            using (var gammaBase = DB.GammaDb)
            {
                var docCloseShift = gammaBase.Docs.Include(d => d.DocCloseShiftDocs).Include(d => d.DocCloseShiftWithdrawals)
                .Include(d => d.DocCloseShiftWastes).Include(d => d.DocCloseShiftNomenclatureRests).Include(d => d.DocCloseShiftUtilizationProducts)
                .First(d => d.DocID == docId);
                if (docCloseShift.DocCloseShiftDocs == null) docCloseShift.DocCloseShiftDocs = new List<Docs>();
                //if (docCloseShift.DocCloseShiftWithdrawals == null)
                //    docCloseShift.DocCloseShiftWithdrawals = new List<DocWithdrawal>();
                if (docCloseShift.DocCloseShiftWastes == null)
                    docCloseShift.DocCloseShiftWastes = new List<DocCloseShiftWastes>();
                if (docCloseShift.DocCloseShiftNomenclatureRests == null)
                    docCloseShift.DocCloseShiftNomenclatureRests = new List<DocCloseShiftNomenclatureRests>();
                if (docCloseShift.DocCloseShiftMaterials == null)
                    docCloseShift.DocCloseShiftMaterials = new List<DocCloseShiftMaterials>();
                if (docCloseShift.DocCloseShiftRemainders == null)
                    docCloseShift.DocCloseShiftRemainders = new List<DocCloseShiftRemainders>();
                if (docCloseShift.DocCloseShiftUtilizationProducts == null)
                    docCloseShift.DocCloseShiftUtilizationProducts = new List<DocCloseShiftUtilizationProducts>();
                
                   
                gammaBase.DocCloseShiftRemainders.RemoveRange(docCloseShift.DocCloseShiftRemainders.Where(p => (p.RemainderTypeID ?? 0) != 0));
                if (BeginSpools != null)
                    foreach (var spool in BeginSpools)
                    {
                        docCloseShift.DocCloseShiftRemainders.Add(new DocCloseShiftRemainders
                        {
                            DocID = docId,
                            ProductID = spool.ProductID,
                            DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                            RemainderTypeID = spool.RemainderTypeID,
                            Quantity = spool.Quantity,
                            StateID = spool.StateID
                        });
                    }
                if (EndSpools != null)
                    foreach (var spool in EndSpools)
                    {
                        if (docCloseShift.DocCloseShiftRemainders.Where(p => (p.RemainderTypeID ?? 0) == 0 && p.ProductID == spool.ProductID).Count() == 0)
                            docCloseShift.DocCloseShiftRemainders.Add(new DocCloseShiftRemainders
                            {
                                DocID = docId,
                                ProductID = spool.ProductID,
                                DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                                RemainderTypeID = spool.RemainderTypeID,
                                Quantity = spool.Quantity,
                                StateID = spool.StateID
                            });
                    }

                gammaBase.DocCloseShiftNomenclatureRests.RemoveRange(docCloseShift.DocCloseShiftNomenclatureRests);
                if (NomenclatureRests != null)
                    foreach (var rest in NomenclatureRests)
                    {
                        docCloseShift.DocCloseShiftNomenclatureRests.Add(new DocCloseShiftNomenclatureRests
                        {
                            DocID = docId,
                            C1CNomenclatureID = rest.NomenclatureID,
                            C1CCharacteristicID = (Guid)rest.CharacteristicID,
                            DocCloseShiftNomenclatureRestID = SqlGuidUtil.NewSequentialid(),
                            Quantity = rest.Quantity //* gammaBase.C1CCharacteristics.FirstOrDefault(c => c.C1CCharacteristicID == rest.CharacteristicID)?.C1CMeasureUnitsPackage.Coefficient ?? 1
                        });
                    }
                    
                gammaBase.DocCloseShiftWastes.RemoveRange(docCloseShift.DocCloseShiftWastes);
                if (Wastes != null)
                    foreach (var waste in Wastes)
                    {
                        docCloseShift.DocCloseShiftWastes.Add(new DocCloseShiftWastes
                        {
                            DocID = docId,
                            C1CNomenclatureID = waste.NomenclatureID,
                            C1CCharacteristicID = waste.CharacteristicID,
                            DocCloseWhiftWasteID = SqlGuidUtil.NewSequentialid(),
                            C1CMeasureUnitID = (Guid)waste.MeasureUnitId,
                            Quantity = waste.Quantity
                        });
                    }
                gammaBase.DocCloseShiftUtilizationProducts.RemoveRange(docCloseShift.DocCloseShiftUtilizationProducts);
                if (UtilizationSpools != null)
                    foreach (var utilizationSpool in UtilizationSpools)
                    {
                        docCloseShift.DocCloseShiftUtilizationProducts.Add(new DocCloseShiftUtilizationProducts
                        {
                            DocID = docId,
                            DocCloseShiftUtilizationProductID = SqlGuidUtil.NewSequentialid(),
                            ProductID = utilizationSpool.ProductID,
                            Quantity = utilizationSpool.Weight
                        });
                        var decisionProduct = gammaBase.DocBrokeDecisionProducts.Where(d => d.ProductID == utilizationSpool.ProductID && d.Quantity * 1000 == utilizationSpool.Weight).OrderByDescending(d => d.DocBroke.Docs.Date).FirstOrDefault();
                        if (decisionProduct != null && !(decisionProduct?.DecisionApplied == true))
                        {
                            decisionProduct.DecisionApplied = true;
                        }
                    }
                gammaBase.DocCloseShiftMovementProducts.RemoveRange(docCloseShift.DocCloseShiftMovementProducts);
                if (InSpools != null)
                    foreach (var spool in InSpools)
                    {
                        docCloseShift.DocCloseShiftMovementProducts.Add(new DocCloseShiftMovementProducts
                        {
                            DocID = docId,
                            DocCloseShiftMovementProductID = SqlGuidUtil.NewSequentialid(),
                            ProductID = spool.ProductId,
                            DocMovementID = spool.DocMovementId,
                            Quantity = spool.Quantity,
                            MovementPlaceName = spool.OutPlaceName,
                            MovementPlaceZoneName = spool.OutPlaceZoneName,
                            DateMovement = spool.OutDate,
                            IsMovementIn = true
                        });
                    }
                //gammaBase.DocCloseShiftMovementProducts.RemoveRange(docCloseShift.DocCloseShiftMovementProducts);
                if (OutSpools != null)
                    foreach (var spool in OutSpools)
                    {
                        docCloseShift.DocCloseShiftMovementProducts.Add(new DocCloseShiftMovementProducts
                        {
                            DocID = docId,
                            DocCloseShiftMovementProductID = SqlGuidUtil.NewSequentialid(),
                            ProductID = spool.ProductId,
                            DocMovementID = spool.DocMovementId,
                            Quantity = spool.Quantity,
                            MovementPlaceName = spool.InPlaceName,
                            MovementPlaceZoneName = spool.InPlaceZoneName,
                            DateMovement = spool.InDate,
                            IsMovementIn = false
                        });
                    }
                if (IsChanged)
                {
                    docCloseShift.DocCloseShiftDocs.Clear();
                    foreach (var id in DocCloseDocIds)
                    {
                        docCloseShift.DocCloseShiftDocs.Add(gammaBase.Docs.First(d => d.DocID == id));
                    }                

                }

                gammaBase.SaveChanges();
                WithdrawalMaterialsGrid?.SaveToModel(docId);

            }
            return true;
        }
    }
}
