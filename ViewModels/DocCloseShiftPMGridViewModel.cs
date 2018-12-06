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
            CloseDate = DB.CurrentDateTime;
            AddWithdrawalMaterialInCommand = new DelegateCommand(AddWithdrawalMaterialIn, () => !IsReadOnly);
            DeleteWithdrawalMaterialInCommand = new DelegateCommand(DeleteWithdrawalMaterialIn, () => !IsReadOnly);
            AddWithdrawalMaterialOutCommand = new DelegateCommand(AddWithdrawalMaterialOut, () => !IsReadOnly);
            DeleteWithdrawalMaterialOutCommand = new DelegateCommand(DeleteWithdrawalMaterialOut, () => !IsReadOnly);
            AddWithdrawalMaterialRemainderCommand = new DelegateCommand(AddWithdrawalMaterialRemainder, () => !IsReadOnly);
            DeleteWithdrawalMaterialRemainderCommand = new DelegateCommand(DeleteWithdrawalMaterialRemainder, () => !IsReadOnly);
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
            MaterialRowUpdatedCommand = new DelegateCommand<CellValue>(OnMaterialRowUpdated);
        }
        public ICommand<CellValue> MaterialRowUpdatedCommand { get; private set; }
        private void OnMaterialRowUpdated(CellValue value)
        {
            MaterialChanged();
            //if (value.Property == "Quantity")
            //    Console.WriteLine("OnCellValueChanged");
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
                var doc = gammaBase.Docs.Include(d => d.DocCloseShiftDocs).First(d => d.DocID == docId);
                DocCloseDocIds = doc.DocCloseShiftDocs.Select(dc => dc.DocID).ToList();
                PlaceID = (int)doc.PlaceID;
                ShiftID = doc.ShiftID ?? 0;
                CloseDate = doc.Date;
                IsConfirmed = doc.IsConfirmed;
                //Получение списка материалов
                WithdrawalMaterialsIn = new ItemsChangeObservableCollection<WithdrawalMaterial>(gammaBase.DocCloseShiftMaterials
                    .Where(dm => dm.DocID == docId && dm.DocCloseShiftMaterialTypeID == 1)
                    .Select(d => new WithdrawalMaterial
                    {
                        CharacteristicID = d.C1CCharacteristicID,
                        NomenclatureID = d.C1CNomenclatureID,
                        NomenclatureName = d.C1CNomenclature.Name,
                        DocWithdrawalMaterialID = d.DocCloseShiftMaterialID,
                        Quantity = d.Quantity,
                        BaseQuantity = d.Quantity,
                        MeasureUnit = d.C1CNomenclature.C1CMeasureUnitStorage.Name,
                        MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID
                    }));
                WithdrawalMaterialsOut = new ItemsChangeObservableCollection<WithdrawalMaterial>(gammaBase.DocCloseShiftMaterials
                    .Where(dm => dm.DocID == docId && dm.DocCloseShiftMaterialTypeID == 2)
                    .Select(d => new WithdrawalMaterial
                    {
                        CharacteristicID = d.C1CCharacteristicID,
                        NomenclatureID = d.C1CNomenclatureID,
                        NomenclatureName = d.C1CNomenclature.Name,
                        DocWithdrawalMaterialID = d.DocCloseShiftMaterialID,
                        Quantity = d.Quantity,
                        BaseQuantity = d.Quantity,
                        MeasureUnit = d.C1CNomenclature.C1CMeasureUnitStorage.Name,
                        MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID
                    }));
                WithdrawalMaterialsRemainder = new ItemsChangeObservableCollection<WithdrawalMaterial>(gammaBase.DocCloseShiftMaterials
                    .Where(dm => dm.DocID == docId && dm.DocCloseShiftMaterialTypeID == 3)
                    .Select(d => new WithdrawalMaterial
                    {
                        CharacteristicID = d.C1CCharacteristicID,
                        NomenclatureID = d.C1CNomenclatureID,
                        NomenclatureName = d.C1CNomenclature.Name,
                        DocWithdrawalMaterialID = d.DocCloseShiftMaterialID,
                        Quantity = d.Quantity,
                        BaseQuantity = d.Quantity,
                        MeasureUnit = d.C1CNomenclature.C1CMeasureUnitStorage.Name,
                        MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID
                    }));
                //Получение списка списанных материалов
                WithdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>(gammaBase.DocWithdrawalMaterials
                    .Where(dm => dm.DocWithdrawal.DocCloseShift.FirstOrDefault().DocID == docId)
                    .Select(d => new WithdrawalMaterial
                    {
                        CharacteristicID = d.C1CCharacteristicID,
                        NomenclatureID = d.C1CNomenclatureID,
                        NomenclatureName = d.C1CNomenclature.Name,
                        DocWithdrawalMaterialID = d.DocWithdrawalMaterialID,
                        Quantity = d.Quantity,
                        BaseQuantity = d.Quantity,
                        MeasureUnit = d.C1CNomenclature.C1CMeasureUnitStorage.Name,
                        MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID,
                        QuantityIsReadOnly = !d.WithdrawByFact ?? true
                    }));
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
                        Weight = p.ProductionQuantity * 1000
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

        public void FillGrid()
        {
            UIServices.SetBusyState();
            ClearGrid();
            using (var gammaBase = DB.GammaDb)
            {
                DocCloseDocIds = gammaBase.Docs.
                Where(d => d.PlaceID == PlaceID && d.ShiftID == ShiftID && d.IsConfirmed &&
                    d.Date >= SqlFunctions.DateAdd("hh", -10, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    d.Date <= SqlFunctions.DateAdd("hh", 10, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
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
                var WithdrawalMaterialsLoad =
                    new ItemsChangeObservableCollection<WithdrawalMaterial>(
                        gammaBase.FillDocCloseShiftPMMaterials(PlaceID, ShiftID, CloseDate)
                        .Take(0)    
                        .Select(m => new WithdrawalMaterial()
                            {
                                NomenclatureID = (Guid)m.NomenclatureID,
                                CharacteristicID = m.CharacteristicID,
                                QuantityIsReadOnly = !m.WithdrawByFact ?? true,
                                Quantity = m.Quantity ?? 0,
                                BaseQuantity = m.Quantity ?? 0,
                                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = m.MeasureUnit,
                                MeasureUnitID = m.MeasureUnitID
                            }));
                foreach (WithdrawalMaterial addedItem in WithdrawalMaterialsLoad)
                {
                    if (WithdrawalMaterialsIn.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                        WithdrawalMaterialsIn.Add(addedItem);
                    if (WithdrawalMaterialsOut.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                        WithdrawalMaterialsOut.Add(addedItem);
                    if (WithdrawalMaterialsRemainder.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                        WithdrawalMaterialsRemainder.Add(addedItem);
                }

                foreach (WithdrawalMaterial addedItem in WithdrawalMaterialsIn)
                {
                    if (WithdrawalMaterials.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                        WithdrawalMaterials.Add(new WithdrawalMaterial()
                        {
                            NomenclatureID = addedItem.NomenclatureID,
                            CharacteristicID = addedItem.CharacteristicID,
                            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                            Quantity = addedItem.Quantity,
                            BaseQuantity = addedItem.BaseQuantity,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.MeasureUnit,
                            MeasureUnitID = addedItem.MeasureUnitID
                        });
                }
                foreach (WithdrawalMaterial addedItem in WithdrawalMaterialsOut)
                {
                    if (WithdrawalMaterials.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                    {
                        WithdrawalMaterials.Add(new WithdrawalMaterial()
                        {
                            NomenclatureID = addedItem.NomenclatureID,
                            CharacteristicID = addedItem.CharacteristicID,
                            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                            Quantity = -addedItem.Quantity,
                            BaseQuantity = -addedItem.BaseQuantity,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.MeasureUnit,
                            MeasureUnitID = addedItem.MeasureUnitID
                        });
                    }
                    else
                    {
                        var item = WithdrawalMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                        if (item != null)
                        {
                            item.Quantity = item.Quantity - addedItem.Quantity;
                            item.BaseQuantity = item.BaseQuantity - addedItem.BaseQuantity;
                        }
                    }
                }
                foreach (WithdrawalMaterial addedItem in WithdrawalMaterialsRemainder)
                {
                    if (WithdrawalMaterials.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                    {
                        WithdrawalMaterials.Add(new WithdrawalMaterial()
                        {
                            NomenclatureID = addedItem.NomenclatureID,
                            CharacteristicID = addedItem.CharacteristicID,
                            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                            Quantity = -addedItem.Quantity,
                            BaseQuantity = -addedItem.BaseQuantity,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.MeasureUnit,
                            MeasureUnitID = addedItem.MeasureUnitID
                        });
                    }
                    else
                    {
                        var item = WithdrawalMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                        if (item != null)
                        {
                            item.Quantity = item.Quantity - addedItem.Quantity;
                            item.BaseQuantity = item.BaseQuantity - addedItem.BaseQuantity;
                        }
                    }
                }

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

                EndSpools = new ItemsChangeObservableCollection<DocCloseShiftRemainder>(gammaBase.Rests
                    .Where(d => d.PlaceID == PlaceID).Join(gammaBase.vProductsCurrentStateInfo, d => d.ProductID, p => p.ProductID
                    , (d, p) => new DocCloseShiftRemainder
                    {
                        ProductID = (Guid)d.ProductID,
                        StateID = (p.StateID == null ? 0 : p.StateID),
                        Quantity = (d.Products.ProductSpools.DecimalWeight == null ? 0 : d.Products.ProductSpools.DecimalWeight) * 1000,
                        RemainderTypeID = 2
                    }));

                if (BeginSpools == null || BeginSpools?.Count() == 0)
                {
                    var PreviousDocCloseShift = gammaBase.Docs
                        .Where(d => d.DocTypeID == 3 && d.PlaceID == PlaceID && d.Date < CloseDate)
                        .OrderByDescending(d => d.Date)
                        .FirstOrDefault();
                    BeginSpools = new ObservableCollection<DocCloseShiftRemainder> (gammaBase.DocCloseShiftRemainders
                        .Where(d => (d.RemainderTypeID == 2 || d.RemainderTypeID == 0 || d.RemainderTypeID == null) && d.DocCloseShifts.DocID == PreviousDocCloseShift.DocID)
                        .Select(d => new DocCloseShiftRemainder
                        {
                            ProductID = (Guid)d.ProductID,
                            StateID = d.StateID,
                            Quantity = d.Quantity,
                            RemainderTypeID = d.RemainderTypeID == 0 || d.RemainderTypeID == null ? 3 : 1
                        }));
                    /*    BeginSpools = new ObservableCollection<DocCloseShiftRemainder>(gammaBase.DocCloseShiftRemainders
                            .Where(d => d.RemainderTypeID == 1 && d.DocCloseShifts.PlaceID == PlaceID)
                            .OrderByDescending(d => d.DocCloseShifts.Date)
                            .Take(1)
                            .Select(d => new DocCloseShiftRemainder
                            {
                                ProductID = (Guid)d.ProductID,
                                Quantity = d.Products.ProductSpools.DecimalWeight * 1000
                            }));
                    */
                    /*foreach (var spool in EndSpools)
                    {
                        if (Spools.Where(d => d.ProductID == spool.ProductID).Count() == 0 && BeginSpools.Where(d => d.ProductID == spool.ProductID).Count() == 0)
                        {
                            BeginSpools.Add(new DocCloseShiftRemainder()
                            {
                                ProductID = spool.ProductID,
                                Quantity = spool.Quantity
                            });
                        }
                    }*/
                }

                InSpools = new ObservableCollection<MovementProduct>(gammaBase.FillDocCloseShiftMovementProducts(PlaceID, ShiftID, CloseDate)
                            .Where(d => (bool)d.IsMovementIn).Select(d => new MovementProduct
                            {
                                NomenclatureName = d.NomenclatureName,
                                Number = d.Number,
                                ProductId = d.ProductID,
                                Quantity = (d.Quantity ?? 0) * 1000,
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
                                Quantity = (d.Quantity ?? 0) * 1000,
                                ProductKindName = d.ProductKindName,
                                OrderTypeName = d.OrderTypeName,
                                DocMovementId = d.DocMovementID,
                                InPlaceName = d.InPlace,
                                InPlaceZoneName = d.InPlaceZone,
                                InDate = d.InDate
                            }));
                IsChanged = true;
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
        public DelegateCommand AddWithdrawalMaterialInCommand { get; private set; }
        public DelegateCommand DeleteWithdrawalMaterialInCommand { get; private set; }
        public DelegateCommand AddWithdrawalMaterialOutCommand { get; private set; }
        public DelegateCommand DeleteWithdrawalMaterialOutCommand { get; private set; }
        public DelegateCommand AddWithdrawalMaterialRemainderCommand { get; private set; }
        public DelegateCommand DeleteWithdrawalMaterialRemainderCommand { get; private set; }
        public DelegateCommand<RemainderType> AddBeginSpoolCommand { get; private set; }
        public DelegateCommand<RemainderType> DeleteBeginSpoolCommand { get; private set; }
        public DelegateCommand<RemainderType> AddEndSpoolCommand { get; private set; }
        public DelegateCommand<RemainderType> DeleteEndSpoolCommand { get; private set; }
        public DelegateCommand AddUtilizationSpoolCommand { get; private set; }
        public DelegateCommand DeleteUtilizationSpoolCommand { get; private set; }


        private void MaterialChanged()
        {
            var selectedMaterial = SelectedWithdrawalMaterialIn ?? SelectedWithdrawalMaterialOut ?? SelectedWithdrawalMaterialRemainder;
            var materialIn = WithdrawalMaterialsIn.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID).Sum(d => d.Quantity);
            var materialOut = WithdrawalMaterialsOut.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID).Sum(d => d.Quantity);
            var materialRemainder = WithdrawalMaterialsRemainder.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID).Sum(d => d.Quantity);
            var materialBaseIn = WithdrawalMaterialsIn.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID).Sum(d => d.BaseQuantity);
            var materialBaseOut = WithdrawalMaterialsOut.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID).Sum(d => d.BaseQuantity);
            var materialBaseRemainder = WithdrawalMaterialsRemainder.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID).Sum(d => d.BaseQuantity);
            var withdrawalMaterial = WithdrawalMaterials.FirstOrDefault(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null)));// && d.CharacteristicID == selectedMaterial?.CharacteristicID);
            if (withdrawalMaterial == null)
            {
                WithdrawalMaterials.Add(new WithdrawalMaterial()
                {
                    NomenclatureID = selectedMaterial.NomenclatureID,
                    NomenclatureName = selectedMaterial.NomenclatureName,
                    CharacteristicID = selectedMaterial.CharacteristicID,
                    DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                    Quantity = materialIn - materialOut - materialRemainder,
                    BaseQuantity = materialBaseIn - materialBaseOut - materialBaseRemainder,
                    MeasureUnit = selectedMaterial.MeasureUnit,
                    MeasureUnitID = selectedMaterial.MeasureUnitID
                }
                );
            }
            else
            {
                withdrawalMaterial.Quantity = materialIn - materialOut - materialRemainder;
                withdrawalMaterial.BaseQuantity = materialBaseIn - materialBaseOut - materialBaseRemainder;
            }
            RaisePropertiesChanged("WithdrawalMaterials");
        }

        public WithdrawalMaterial SelectedWithdrawalMaterialIn { get; set; }

        private void DeleteWithdrawalMaterialIn()
        {
            if (SelectedWithdrawalMaterialIn == null) return;
            WithdrawalMaterialsIn.Remove(SelectedWithdrawalMaterialIn);
        }

        private void AddWithdrawalMaterialIn()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, SetMaterialNomenclature);
            MessageManager.FindNomenclature(MaterialType.MaterialsSGB);
        }

        public WithdrawalMaterial SelectedWithdrawalMaterialOut { get; set; }

        private void DeleteWithdrawalMaterialOut()
        {
            if (SelectedWithdrawalMaterialOut == null) return;
            WithdrawalMaterialsOut.Remove(SelectedWithdrawalMaterialOut);
        }

        private void AddWithdrawalMaterialOut()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, SetMaterialNomenclature);
            MessageManager.FindNomenclature(MaterialType.MaterialsSGB);
        }

        public WithdrawalMaterial SelectedWithdrawalMaterialRemainder { get; set; }

        private void DeleteWithdrawalMaterialRemainder()
        {
            if (SelectedWithdrawalMaterialRemainder == null) return;
            WithdrawalMaterialsRemainder.Remove(SelectedWithdrawalMaterialRemainder);
        }

        private void AddWithdrawalMaterialRemainder()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, SetMaterialNomenclature);
            MessageManager.FindNomenclature(MaterialType.MaterialsSGB);
        }

        private void SetMaterialNomenclature(Nomenclature1CMessage msg)
        {
            Messenger.Default.Unregister<Nomenclature1CMessage>(this);
            using (var gammaBase = DB.GammaDb)
            {
                var nomenclatureInfo =
                gammaBase.C1CNomenclature.Include(n => n.C1CMeasureUnitStorage)
                    .First(n => n.C1CNomenclatureID == msg.Nomenclature1CID);
                if (WithdrawalMaterialsIn.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                    WithdrawalMaterialsIn.Add(new WithdrawalMaterial
                    {
                        NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                        QuantityIsReadOnly = false,
                        Quantity = 0,
                        MeasureUnitID = nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID,
                        MeasureUnit = nomenclatureInfo.C1CMeasureUnitStorage.Name,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid()
                    });
                if (WithdrawalMaterialsOut.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                    WithdrawalMaterialsOut.Add(new WithdrawalMaterial
                    {
                        NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                        QuantityIsReadOnly = false,
                        Quantity = 0,
                        MeasureUnitID = nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID,
                        MeasureUnit = nomenclatureInfo.C1CMeasureUnitStorage.Name,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid()
                    });
                if (WithdrawalMaterialsRemainder.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                    WithdrawalMaterialsRemainder.Add(new WithdrawalMaterial
                    {
                        NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                        QuantityIsReadOnly = false,
                        Quantity = 0,
                        MeasureUnitID = nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID,
                        MeasureUnit = nomenclatureInfo.C1CMeasureUnitStorage.Name,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid()
                    });
                if (WithdrawalMaterials.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                    WithdrawalMaterials.Add(new WithdrawalMaterial
                    {
                        NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                        QuantityIsReadOnly = false,
                        Quantity = 0,
                        MeasureUnitID = nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID,
                        MeasureUnit = nomenclatureInfo.C1CMeasureUnitStorage.Name,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid()
                    });
            }
        }

        private int PlaceID { get; set; }
        private int ShiftID { get; set; }
        private DateTime CloseDate { get; set; }
        public void ClearGrid()
        {
            Spools?.Clear();
            DocCloseDocIds?.Clear();
            WithdrawalMaterials?.Clear();
            //WithdrawalMaterialsIn?.Clear();
            //WithdrawalMaterialsOut?.Clear();
            //WithdrawalMaterialsRemainder?.Clear();
            EndSpools?.Clear();
            InSpools?.Clear();
            OutSpools?.Clear();
            IsChanged = true;
            Wastes?.Clear();
            NomenclatureRests?.Clear();
            //            gammaBase.SaveChanges();           
        }

        private int _selectedMaterialTabIndex;
        public int SelectedMaterialTabIndex
        {
            get
            {
                return _selectedMaterialTabIndex;
            }
            set
            {
                _selectedMaterialTabIndex = value;
                if (value == 3)
                {
                    var currentWithdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial> (WithdrawalMaterials);
                    WithdrawalMaterials?.Clear();
                    WithdrawalMaterials = currentWithdrawalMaterials;
                }
            }
        }

        private ItemsChangeObservableCollection<WithdrawalMaterial> _withdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>();

        public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterials
        {
            get { return _withdrawalMaterials; }
            set
            {
                _withdrawalMaterials = value;
                RaisePropertiesChanged("WithdrawalMaterials");
            }
        }
        
        private ItemsChangeObservableCollection<WithdrawalMaterial> _withdrawalMaterialsIn = new ItemsChangeObservableCollection<WithdrawalMaterial>();

        public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterialsIn
        {
            get { return _withdrawalMaterialsIn; }
            set
            {
                _withdrawalMaterialsIn = value;
                RaisePropertiesChanged("WithdrawalMaterialsIn");
            }
        }

        private ItemsChangeObservableCollection<WithdrawalMaterial> _withdrawalMaterialsOut = new ItemsChangeObservableCollection<WithdrawalMaterial>();

        public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterialsOut
        {
            get { return _withdrawalMaterialsOut; }
            set
            {
                _withdrawalMaterialsOut = value;
                RaisePropertiesChanged("WithdrawalMaterialsOut");
            }
        }

        private ItemsChangeObservableCollection<WithdrawalMaterial> _withdrawalMaterialsRemainder = new ItemsChangeObservableCollection<WithdrawalMaterial>();

        public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterialsRemainder
        {
            get { return _withdrawalMaterialsRemainder; }
            set
            {
                _withdrawalMaterialsRemainder = value;
                RaisePropertiesChanged("WithdrawalMaterialsRemainder");
            }
        }

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
                /*var isWrittenOff = gammaBase.vProductsInfo.Where(p => p.ProductID == msg.ProductID).Select(p => p.IsWrittenOff).FirstOrDefault();
                if (isWrittenOff ?? false && CurrentAddSpoolRemainder == RemainderType.End)
                {
                    MessageBox.Show("Нельзя добавить в остаток на конец смены списанный тамбур", "Списанный тамбур", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }
                */
                var product = new ObservableCollection<PaperBase>(gammaBase.vProductsInfo
                    .Where(p => p.ProductID == msg.ProductID)
                    .Select(p => new PaperBase()
                {
                    CharacteristicID = (Guid)p.C1CCharacteristicID,
                    NomenclatureID = p.C1CNomenclatureID,
                    Nomenclature = p.NomenclatureName,
                    Number = p.Number,
                    ProductID = p.ProductID,
                    Weight = p.ProductionQuantity * 1000
                }));
                foreach (var item in product)
                    UtilizationSpools.Add(item);
            }
        }

        private void DeleteUtilizationSpool()
        {
            if (SelectedUtilizationSpool == null) return;
            UtilizationSpools.Remove(SelectedUtilizationSpool);
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
                Guid docWithdrawalId;
                if (docCloseShift.DocCloseShiftWithdrawals == null)
                    docCloseShift.DocCloseShiftWithdrawals = new List<DocWithdrawal>();
                if (docCloseShift.DocCloseShiftWastes == null)
                    docCloseShift.DocCloseShiftWastes = new List<DocCloseShiftWastes>();
                if (docCloseShift.DocCloseShiftNomenclatureRests == null)
                    docCloseShift.DocCloseShiftNomenclatureRests = new List<DocCloseShiftNomenclatureRests>();
                if (docCloseShift.DocCloseShiftMaterials == null)
                    docCloseShift.DocCloseShiftMaterials = new List<DocCloseShiftMaterials>();
                if (docCloseShift.DocCloseShiftRemainders == null)
                    docCloseShift.DocCloseShiftRemainders = new List<DocCloseShiftRemainders>();
                if (docCloseShift.DocCloseShiftRemainders == null)
                    docCloseShift.DocCloseShiftRemainders = new List<DocCloseShiftRemainders>();
                if (docCloseShift.DocCloseShiftUtilizationProducts == null)
                    docCloseShift.DocCloseShiftUtilizationProducts = new List<DocCloseShiftUtilizationProducts>();
                if (docCloseShift.DocCloseShiftWithdrawals.Count == 0)
                {
                    docWithdrawalId = SqlGuidUtil.NewSequentialid();
                    docCloseShift.DocCloseShiftWithdrawals.Add
                    (
                        new DocWithdrawal
                        {
                            DocID = docWithdrawalId,
                            DocWithdrawalMaterials = new List<DocWithdrawalMaterials>(),
                            Docs = new Docs()
                            {
                                DocID = docWithdrawalId,
                                Date = docCloseShift.Date,
                                DocTypeID = (byte)DocTypes.DocWithdrawal,
                                IsConfirmed = docCloseShift.IsConfirmed,
                                PlaceID = WorkSession.PlaceID,
                                ShiftID = WorkSession.ShiftID,
                                UserID = WorkSession.UserID,
                                PrintName = WorkSession.PrintName
                            }
                        }
                    );
                }
                gammaBase.DocCloseShiftMaterials.RemoveRange(docCloseShift.DocCloseShiftMaterials);
                if (WithdrawalMaterialsIn != null)
                    foreach (var material in WithdrawalMaterialsIn)
                    {
                        docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                        {
                            DocID = docId,
                            C1CNomenclatureID = material.NomenclatureID,
                            C1CCharacteristicID = material.CharacteristicID,
                            DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                            DocCloseShiftMaterialTypeID = 1,
                            Quantity = material.Quantity
                        });
                    }
                if (WithdrawalMaterialsOut != null)
                    foreach (var material in WithdrawalMaterialsOut)
                    {
                        docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                        {
                            DocID = docId,
                            C1CNomenclatureID = material.NomenclatureID,
                            C1CCharacteristicID = material.CharacteristicID,
                            DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                            DocCloseShiftMaterialTypeID = 2,
                            Quantity = material.Quantity
                        });
                    }
                if (WithdrawalMaterialsRemainder != null)
                    foreach (var material in WithdrawalMaterialsRemainder)
                    {
                        docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                        {
                            DocID = docId,
                            C1CNomenclatureID = material.NomenclatureID,
                            C1CCharacteristicID = material.CharacteristicID,
                            DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                            DocCloseShiftMaterialTypeID = 3,
                            Quantity = material.Quantity
                        });
                    }

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
                            ProductID = utilizationSpool.ProductID
                        });
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
                gammaBase.DocCloseShiftMovementProducts.RemoveRange(docCloseShift.DocCloseShiftMovementProducts);
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
                gammaBase.DocWithdrawalMaterials.RemoveRange(
                    docCloseShift.DocCloseShiftWithdrawals.FirstOrDefault()?.DocWithdrawalMaterials);
                docWithdrawalId = docCloseShift.DocCloseShiftWithdrawals.First().DocID;
                foreach (var material in WithdrawalMaterials)
                {
                    docCloseShift.DocCloseShiftWithdrawals.FirstOrDefault()?.DocWithdrawalMaterials.Add(new DocWithdrawalMaterials()
                    {
                        DocID = docWithdrawalId,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        C1CCharacteristicID = material.CharacteristicID,
                        C1CNomenclatureID = material.NomenclatureID,
                        Quantity = material.Quantity,
                        WithdrawByFact = !material.QuantityIsReadOnly
                    });
                }
                gammaBase.SaveChanges();
            }
            return true;
        }
    }
}
