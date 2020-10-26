// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Gamma.Interfaces;
using Gamma.Attributes;
using System.Data.Entity;
using Gamma.Entities;
using System.Windows;
using Gamma.Models;
using DevExpress.Mvvm;
using Gamma.Common;
using System.Collections;
using System.Windows.Data;
using System.Windows.Markup;
using DevExpress.Xpf.Grid;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocMaterialProductionGridViewModel : /*DbEditItemWithNomenclatureViewModel*/SaveImplementedViewModel, ICheckedAccess, IFillClearGrid
    {
        /// <summary>
        /// Initializes a new instance of the DocMaterialProductionGridViewModel class.
        /// </summary>
        public DocMaterialProductionGridViewModel()
        {
            AddDocMaterialProductionCommand = new DelegateCommand(AddDocMaterialProduction, () => IsAllowEditingDocMaterialCompositionCalculations);
            DeleteDocMaterialProductionCommand = new DelegateCommand(DeleteDocMaterialProduction, () => IsAllowEditingDocMaterialCompositionCalculations);
            MaterialRowUpdatedCommand = new DelegateCommand<CellValue>(OnMaterialRowUpdated);
            ValidateCellCommand = new DelegateCommand<GridCellValidationEventArgs>(OnValidateCell);
            if (WorkSession.DBAdmin || (WorkSession.RoleName == "Dispetcher"))
                IsAllowEditingReadOnlyQuantityRemainderAtEnd = true;
            else
                IsAllowEditingReadOnlyQuantityRemainderAtEnd = false;
        }

        public DocMaterialProductionGridViewModel(int placeID, GammaEntities gammaDb = null) :this()
        {
            GammaBase = gammaDb ?? DB.GammaDb;
            PlaceID = placeID;
            IsVisibleTankRemainders = !GammaBase.DocMaterialTankGroups.Any(t => t.PlaceID == placeID && t.DocMaterialProductionTypeID == null && t.NextDocMaterialTankGroupID == null);
            PlaceWithdrawalMaterialTypeID = GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID).First();

        }

        public DocMaterialProductionGridViewModel(int placeID, int shiftID, DateTime closeDate, GammaEntities gammaDb = null):this(placeID, gammaDb)
        {
            ShiftID = shiftID;
            CloseDate = closeDate;

            TankGroupContainer = new DocMaterialTankGroupContainer(PlaceID);
            CurrentTankRemaindersView = new DocMaterialTankRemaindersViewModel(PlaceID, IsConfirmed, TankGroupContainer);
            DocMaterialCompositionCalculations = new DocMaterialProduction(PlaceID, ShiftID, CloseDate, TankGroupContainer);
            IsReadOnlyQuantityRemainderAtEnd = !DocMaterialCompositionCalculations.IsNotCalculatedQuantityRemainderAtEnd;
            DocMaterialProductionDirectCalculationsGrid = new DocMaterialProductionDirectCalculationMaterialViewModel(PlaceID, ShiftID, CloseDate);
            DocMaterialProductionDirectCalculationsGrid.SelectedMaterialTabIndex = 0;
        }

        public DocMaterialProductionGridViewModel(int placeID, int shiftID, DateTime closeDate, Guid docID, bool isConfirmed, List<Guid> productionProductCharacteristicIDs, GammaEntities gammaDb = null):this(placeID, gammaDb)
        {

            ShiftID = shiftID;
            CloseDate = closeDate;

           IsConfirmed = isConfirmed;

            TankGroupContainer = new DocMaterialTankGroupContainer(docID, PlaceID);
            CurrentTankRemaindersView = new DocMaterialTankRemaindersViewModel(PlaceID, IsConfirmed, TankGroupContainer);
            DocMaterialCompositionCalculations = new DocMaterialProduction(PlaceID, ShiftID, CloseDate, TankGroupContainer);
            DocMaterialCompositionCalculations.LoadProductionMaterials(docID, productionProductCharacteristicIDs);
            IsReadOnlyQuantityRemainderAtEnd = !DocMaterialCompositionCalculations.IsNotCalculatedQuantityRemainderAtEnd;
            DocMaterialProductionDirectCalculationsGrid = new DocMaterialProductionDirectCalculationMaterialViewModel(PlaceID, ShiftID, CloseDate, docID, isConfirmed, productionProductCharacteristicIDs);
            DocMaterialProductionDirectCalculationsGrid.SelectedMaterialTabIndex = 0;
        }

        private MaterialType CurrentMaterialType;

        private int _placeID;
        private int PlaceID
        {
            get { return _placeID; }
            set
            {
                _placeID = value;
                var placeGroupID =  GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID).First();
                    if (placeGroupID == 0)
                        CurrentMaterialType = MaterialType.MaterialsSGB;
                    else if (placeGroupID == 2)
                        CurrentMaterialType = MaterialType.MaterialsSGI;
                
            }
        }

        private int ShiftID;
        DateTime CloseDate;

        public DocMaterialProduction DocMaterialCompositionCalculations { get; set; }
        public DocMaterialTankGroupContainer TankGroupContainer { get; set; }

        public bool IsVisibleTankRemainders { get; set; }
        public bool IsAllowEditingDocMaterialCompositionCalculations => !IsReadOnly && IsVisibleTankRemainders;

        public bool _isReadOnlyQuantityRemainderAtEnd { get; set; } = true;
        public bool IsReadOnlyQuantityRemainderAtEnd
        {
            get
            {
                return _isReadOnlyQuantityRemainderAtEnd;
            }
            set
            {
                _isReadOnlyQuantityRemainderAtEnd = value;
                DocMaterialCompositionCalculations.IsNotCalculatedQuantityRemainderAtEnd = !value;
                RaisePropertyChanged("IsReadOnlyQuantityRemainderAtEnd");
            }
        }

        public bool IsAllowEditingReadOnlyQuantityRemainderAtEnd { get; set; }


        private bool IsConfirmed { get; set; }
        public void ChangeConfirmed ( bool isConfirmed)
        {
            IsConfirmed = isConfirmed;
            (CurrentTankRemaindersView as DocMaterialTankRemaindersViewModel)?.ChangeConfirmed(isConfirmed);
            (DocMaterialProductionDirectCalculationsGrid as DocMaterialProductionDirectCalculationMaterialViewModel)?.ChangeConfirmed(isConfirmed);
            RaisePropertyChanged("IsReadOnly");
            RaisePropertyChanged("IsAllowEditingDocMaterialCompositionCalculations");
        }

        public bool IsReadOnly => !DB.HaveWriteAccess("DocMaterialProductions") || IsConfirmed;

        private SaveImplementedViewModel _currentTankRemaindersView;
        public SaveImplementedViewModel CurrentTankRemaindersView
        {
            get
            {
                return _currentTankRemaindersView;
            }
            set
            {
                _currentTankRemaindersView = value;
                RaisePropertyChanged("CurrentTankRemaindersView");
            }
        }

        private DocMaterialProductionDirectCalculationMaterialViewModel _docMaterialProductionDirectCalculationsGrid;
        public DocMaterialProductionDirectCalculationMaterialViewModel DocMaterialProductionDirectCalculationsGrid
        {
            get
            {
                return _docMaterialProductionDirectCalculationsGrid;
            }
            set
            {
                _docMaterialProductionDirectCalculationsGrid = value;
                //Bars = (_currentViewModelGrid as IBarImplemented).Bars;
            }
        }

        public DelegateCommand AddDocMaterialProductionCommand { get; private set; }
        public DelegateCommand DeleteDocMaterialProductionCommand { get; private set; }
        public DelegateCommand ChangeIsSendIntoNextPlaceDocMaterialProductionCommand { get; private set; }
        public DelegateCommand BackMaterialTabCommand { get; private set; }

        public ICommand<GridCellValidationEventArgs> ValidateCellCommand { get; private set; }
        public ICommand<CellValue> MaterialRowUpdatedCommand { get; private set; }
        private void OnMaterialRowUpdated(CellValue value)
        {
            //if (!SelectedDocMaterialProduction.IsNotCalculatedQuantityRemainderAtEnd) если для отдельной строки, то непонятно как считать композицию.
            if (SelectedDocMaterialProduction.IsNotSendMaterialIntoNextPlace && SelectedDocMaterialProduction.IsFullSendMaterialIntoNextPlace)
                MessageBox.Show("Ошибка! Нельзя выбрать одновременно Подано полностью и Не подано!");
            else
                if (IsReadOnlyQuantityRemainderAtEnd)
                    DocMaterialCompositionCalculations.MaterialChanged(SelectedMaterialTabIndex, SelectedDocMaterialProduction);
            
        }
        
        public void OnValidateCell(GridCellValidationEventArgs e)
        {
            if ((e.Cell.Property == "IsNotSendMaterialIntoNextPlace" && (bool)e.Value && ((DocMaterialProductionCompositionCalculationItem)e.Row).IsFullSendMaterialIntoNextPlace) || (e.Cell.Property == "IsFullSendMaterialIntoNextPlace" && (bool)e.Value && ((DocMaterialProductionCompositionCalculationItem)e.Row).IsNotSendMaterialIntoNextPlace))
            {
                bool IsNotSendMaterialIntoNextPlace = ((DocMaterialProductionCompositionCalculationItem)e.Row).IsNotSendMaterialIntoNextPlace;
                bool IsFullSendMaterialIntoNextPlace = ((DocMaterialProductionCompositionCalculationItem)e.Row).IsFullSendMaterialIntoNextPlace;
                bool IsNotSendMaterialIntoNextPlaceChange = (e.Cell.Property == "IsNotSendMaterialIntoNextPlace");
                bool IsFullSendMaterialIntoNextPlaceChange = (e.Cell.Property == "IsFullSendMaterialIntoNextPlace");
                if ((IsNotSendMaterialIntoNextPlaceChange && (bool)e.Value && IsFullSendMaterialIntoNextPlace) || (IsFullSendMaterialIntoNextPlaceChange && (bool)e.Value && IsNotSendMaterialIntoNextPlace))
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                    e.ErrorContent = "Ошибка! Нельзя выбрать одновременно 'Подано полностью' и 'Не подано'!";
                }
            }
            else
            {
                if ((e.Cell.Property == "IsNotSendMaterialIntoNextPlace") && (bool)e.Value)
                {
                    decimal SumQuantityRemainderAtEnd = ((DocMaterialProductionCompositionCalculationItem)e.Row).SumQuantityRemainderAtEnd;
                    decimal SumQuantityIn = (((DocMaterialProductionCompositionCalculationItem)e.Row).QuantityRemainderAtBegin ?? 0) + (((DocMaterialProductionCompositionCalculationItem)e.Row).QuantityIn ?? 0) + (((DocMaterialProductionCompositionCalculationItem)e.Row).QuantityDismiss ?? 0);

                    if (SumQuantityIn > SumQuantityRemainderAtEnd)
                    {
                        e.IsValid = false;
                        e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                        e.ErrorContent = "Ошибка! Нельзя выбрать 'Не подано', так как остаток на конец становится больше общего остатка в бассейнах!";
                    }
                }
            }
        }

        private int _placeWithdrawalMaterialTypeID;
        public int PlaceWithdrawalMaterialTypeID
        {
            get { return _placeWithdrawalMaterialTypeID; }
            set
            {
                _placeWithdrawalMaterialTypeID = value;
               
            }
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
               RaisePropertiesChanged("SelectedMaterialTabIndex");
            }
        }

        public WithdrawalMaterial SelectedWithdrawalMaterial { get; set; }

        public DocMaterialProductionCompositionCalculationItem SelectedDocMaterialProduction { get; set; }

        public bool IsChanged
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private void DeleteDocMaterialProduction()
        {
            if (SelectedDocMaterialProduction == null) return;
            TankGroupContainer?.DeleteComposition(SelectedDocMaterialProduction.NomenclatureID);
            DocMaterialCompositionCalculations.DocMaterialProductionCompositionCalculations.Remove(SelectedDocMaterialProduction);
        }

        private void AddDocMaterialProduction()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, SetMaterialNomenclature);
            MessageManager.FindNomenclature(CurrentMaterialType);
        }

        private void SetMaterialNomenclature(Nomenclature1CMessage msg)
        {
            Messenger.Default.Unregister<Nomenclature1CMessage>(this);
            using (var gammaBase = DB.GammaDb)
            {
                var nomenclatureInfo =
                gammaBase.C1CNomenclature.Include(n => n.C1CMeasureUnitStorage)
                    .First(n => n.C1CNomenclatureID == msg.Nomenclature1CID);
                DocMaterialCompositionCalculations.MaterialNomenclatureChanged(nomenclatureInfo);
            }
        }

        public void FillGrid()
        {
            FillProductionMaterials(true);
        }

        public void FillProductionMaterials(bool IsFillEnd = true)
        {
            var tankRemaindersView = (CurrentTankRemaindersView as IFillClearGrid);
            if (tankRemaindersView != null)
                tankRemaindersView.FillGrid();
            DocMaterialCompositionCalculations.FillProductionMaterials(IsFillEnd);
            if (DocMaterialProductionDirectCalculationsGrid != null)
            {
                DocMaterialProductionDirectCalculationsGrid.DirectCalculationMaterials?.FillProductionMaterials(IsFillEnd);
            }
        }

        public void FillGridWithNoFillEnd()
        {
            FillProductionMaterials(false);
        }

        public void ClearGrid()
        {
            var tankRemaindersView = (CurrentTankRemaindersView as IFillClearGrid);
            if (tankRemaindersView != null)
                tankRemaindersView.ClearGrid();
            DocMaterialCompositionCalculations?.ClearFromButton();
            if (DocMaterialProductionDirectCalculationsGrid != null)
            {
                DocMaterialProductionDirectCalculationsGrid.DirectCalculationMaterials?.ClearFromButton();
            }
        }

        public override bool SaveToModel(Guid docId)
        {
            if (IsReadOnly) return true;
            using (var gammaBase = DB.GammaDb)
            {
                var doc = gammaBase.Docs
               .First(d => d.DocID == docId);
                if (doc.DocMaterialProductions == null)
                    doc.DocMaterialProductions = new List<DocMaterialProductions>();

                
                //gammaBase.DocMaterialProductions.RemoveRange(docCloseShift.DocMaterialProductions);
                var materialsInDB = gammaBase.DocMaterialProductions.Where(d => d.DocID == docId);
                foreach (var materialRemove in materialsInDB)
                {
                    if (DocMaterialProductionDirectCalculationsGrid?.DirectCalculationMaterials?.Materials != null)
                    {
                        if (!DocMaterialCompositionCalculations.DocMaterialProductionCompositionCalculations.Any(d => d.NomenclatureID == materialRemove.C1CNomenclatureID && (d.CharacteristicID == materialRemove.C1CCharacteristicID || (d.CharacteristicID == null && materialRemove.C1CCharacteristicID == null)))
                                && !DocMaterialProductionDirectCalculationsGrid.DirectCalculationMaterials.Materials.Any(d => d.NomenclatureID == materialRemove.C1CNomenclatureID && (d.CharacteristicID == materialRemove.C1CCharacteristicID || (d.CharacteristicID == null && materialRemove.C1CCharacteristicID == null))))
                        {
                            gammaBase.DocMaterialProductions.Remove(materialRemove);
                        }
                    }
                    else
                    {
                        if (!DocMaterialCompositionCalculations.DocMaterialProductionCompositionCalculations.Any(d => d.NomenclatureID == materialRemove.C1CNomenclatureID && (d.CharacteristicID == materialRemove.C1CCharacteristicID || (d.CharacteristicID == null && materialRemove.C1CCharacteristicID == null))))
                        {
                            gammaBase.DocMaterialProductions.Remove(materialRemove);
                        }
                    }
                };

                bool isCompositionCalculationParameter = true;
                foreach (var material in DocMaterialCompositionCalculations.DocMaterialProductionCompositionCalculations)
                {
                    var materialInDB = materialsInDB.Where(d => d.IsCompositionCalculation == isCompositionCalculationParameter && d.C1CNomenclatureID == material.NomenclatureID && (d.C1CCharacteristicID == material.CharacteristicID || (d.C1CCharacteristicID == null && material.CharacteristicID == null)));
                    foreach (DocMaterialProductionTypes docMaterialProductionType in Enum.GetValues(typeof(DocMaterialProductionTypes)))
                    {
                        decimal? quantity = null;
                        switch (docMaterialProductionType)
                        {
                            case DocMaterialProductionTypes.In:
                                quantity = material.QuantityIn;
                                break;
                            case DocMaterialProductionTypes.Send:
                                quantity = material.QuantitySend;
                                break;
                            case DocMaterialProductionTypes.RemainderAtBegin:
                                quantity = material.QuantityRemainderAtBegin;
                                break;
                            case DocMaterialProductionTypes.RemainderAtEnd:
                                quantity = material.QuantityRemainderAtEnd;
                                break;
                            case DocMaterialProductionTypes.Dismiss:
                                quantity = material.QuantityDismiss;
                                break;
                            case DocMaterialProductionTypes.RemainderInGRVAtEnd:
                                quantity = material.QuantityRemainderInGRVAtEnd;
                                break;
                            case DocMaterialProductionTypes.Standard:
                                quantity = material.StandardQuantity;
                                break;
                            case DocMaterialProductionTypes.IsNotSend:
                                quantity = material.IsNotSendMaterialIntoNextPlace ? 1 : 0;
                                break;
                            case DocMaterialProductionTypes.IsFullSend:
                                quantity = material.IsFullSendMaterialIntoNextPlace ? 1 : 0;
                                break;
                            case DocMaterialProductionTypes.IsNotCalculatedRemainderAtEnd:
                                quantity = material.IsNotCalculatedQuantityRemainderAtEnd ? 1 : 0;
                                break;
                        }
                        if (quantity != null || docMaterialProductionType == DocMaterialProductionTypes.RemainderAtBegin)
                        {
                            var item = materialInDB.Where(d => d.DocMaterialProductionTypeID == (int)docMaterialProductionType && d.IsCompositionCalculation == isCompositionCalculationParameter).FirstOrDefault();
                            if (item == null)
                                doc.DocMaterialProductions.Add(new DocMaterialProductions
                                {
                                    DocID = docId,
                                    C1CNomenclatureID = material.NomenclatureID,
                                    C1CCharacteristicID = material.CharacteristicID,
                                    DocMaterialProductionID = SqlGuidUtil.NewSequentialid(),
                                    DocMaterialProductionTypeID = (int)docMaterialProductionType,
                                    Quantity = quantity ?? 0,
                                    C1CMeasureUnitID = material.MeasureUnitID,
                                    WithdrawByFact = material.WithdrawByFact,
                                    IsCompositionCalculation = isCompositionCalculationParameter
                                });
                            else
                            {
                                item.Quantity = quantity ?? 0;
                            }
                        }
                        else
                        {
                            if (materialInDB.Any(d => d.DocMaterialProductionTypeID == (int)docMaterialProductionType))
                                gammaBase.DocMaterialProductions.RemoveRange(materialInDB.Where(d => d.DocMaterialProductionTypeID == (int)docMaterialProductionType && d.IsCompositionCalculation == isCompositionCalculationParameter));
                        }
                    }
                }

                doc.DocMaterialProductDocs.Clear();


                if (DocMaterialCompositionCalculations.Docs != null)
                    foreach (var docAdd in DocMaterialCompositionCalculations.Docs)
                    {
                        doc.DocMaterialProductDocs.Add(gammaBase.Docs.First(d => d.DocID == docAdd.DocID));
                    }

                
                isCompositionCalculationParameter = false;
                if (DocMaterialProductionDirectCalculationsGrid?.DirectCalculationMaterials?.Materials != null)
                {
                    foreach (var material in DocMaterialProductionDirectCalculationsGrid.DirectCalculationMaterials.Materials)
                    {
                        var materialInDB = materialsInDB.Where(d => d.IsCompositionCalculation == isCompositionCalculationParameter && d.C1CNomenclatureID == material.NomenclatureID && (d.C1CCharacteristicID == material.CharacteristicID || (d.C1CCharacteristicID == null && material.CharacteristicID == null)));
                        foreach (DocMaterialProductionTypes docMaterialProductionType in Enum.GetValues(typeof(DocMaterialProductionTypes)))
                        {
                            decimal? quantity = null;
                            switch (docMaterialProductionType)
                            {
                                case DocMaterialProductionTypes.In:
                                    quantity = material.QuantityIn;
                                    break;
                                case DocMaterialProductionTypes.Send:
                                    quantity = material.QuantitySend;
                                    break;
                                case DocMaterialProductionTypes.RemainderAtBegin:
                                    quantity = material.QuantityRemainderAtBegin;
                                    break;
                                case DocMaterialProductionTypes.RemainderAtEnd:
                                    quantity = material.QuantityRemainderAtEnd;
                                    break;
                                case DocMaterialProductionTypes.Dismiss:
                                    quantity = material.QuantityDismiss;
                                    break;
                                case DocMaterialProductionTypes.Standard:
                                    quantity = material.StandardQuantity;
                                    break;
                                case DocMaterialProductionTypes.Utilization:
                                    quantity = material.QuantityUtil;
                                    break;
                                case DocMaterialProductionTypes.Experimental:
                                    quantity = material.QuantityExperimental;
                                    break;
                                case DocMaterialProductionTypes.Out:
                                    quantity = material.QuantityOut;
                                    break;
                                case DocMaterialProductionTypes.SendAtBegin:
                                    quantity = material.QuantitySendAtBegin;
                                    break;
                                case DocMaterialProductionTypes.SendAtEnd:
                                    quantity = material.QuantitySendAtEnd;
                                    break;
                            }
                            if (quantity != null || docMaterialProductionType == DocMaterialProductionTypes.RemainderAtBegin)
                            {
                                var item = materialInDB.Where(d => d.DocMaterialProductionTypeID == (int)docMaterialProductionType && d.IsCompositionCalculation == isCompositionCalculationParameter).FirstOrDefault();
                                if (item == null)
                                    doc.DocMaterialProductions.Add(new DocMaterialProductions
                                    {
                                        DocID = docId,
                                        C1CNomenclatureID = material.NomenclatureID,
                                        C1CCharacteristicID = material.CharacteristicID,
                                        DocMaterialProductionID = SqlGuidUtil.NewSequentialid(),
                                        DocMaterialProductionTypeID = (int)docMaterialProductionType,
                                        Quantity = quantity ?? 0,
                                        C1CMeasureUnitID = material.MeasureUnitID,
                                        WithdrawByFact = material.WithdrawByFact,
                                        IsCompositionCalculation = isCompositionCalculationParameter
                                    });
                                else
                                {
                                    item.Quantity = quantity ?? 0;
                                }
                            }
                            else
                            {
                                if (materialInDB.Any(d => d.DocMaterialProductionTypeID == (int)docMaterialProductionType))
                                    gammaBase.DocMaterialProductions.RemoveRange(materialInDB.Where(d => d.DocMaterialProductionTypeID == (int)docMaterialProductionType && d.IsCompositionCalculation == isCompositionCalculationParameter));
                            }
                        }
                    }
                }
                gammaBase.SaveChanges();
            }
            if (CurrentTankRemaindersView != null)
            {
                CurrentTankRemaindersView.SaveToModel(docId);
            }
            return true;
        }

    }
}