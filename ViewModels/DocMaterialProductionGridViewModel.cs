﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
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
            AddDocMaterialProductionCommand = new DelegateCommand(AddDocMaterialProduction, () => !IsReadOnly);
            DeleteDocMaterialProductionCommand = new DelegateCommand(DeleteDocMaterialProduction, () => !IsReadOnly);

            MaterialRowUpdatedCommand = new DelegateCommand<CellValue>(OnMaterialRowUpdated);
            
            //MaterialTabActivateCommand = new DelegateCommand<byte>(MaterialTabActivate);
            //MaterialTabActivate(0);
        }
        public DocMaterialProductionGridViewModel(int placeID, int shiftID, DateTime closeDate, GammaEntities gammaDb = null):this()
        {
            GammaBase = gammaDb ?? DB.GammaDb;
            PlaceID = placeID;
            ShiftID = shiftID;
            CloseDate = closeDate;

            TankGroupContainer = new DocMaterialTankGroupContainer(PlaceID);
            CurrentTankRemaindersView = new DocMaterialTankRemaindersViewModel(PlaceID, TankGroupContainer);
            DocMaterialCompositionCalculations = new DocMaterialProduction(PlaceID, ShiftID, CloseDate, TankGroupContainer);
            PlaceWithdrawalMaterialTypeID = GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID).First();
            DocMaterialProductionDirectCalculationsGrid = new DocMaterialProductionDirectCalculationMaterialViewModel(PlaceID, ShiftID, CloseDate);
            DocMaterialProductionDirectCalculationsGrid.SelectedMaterialTabIndex = 0;
        }

        public DocMaterialProductionGridViewModel(int placeID, int shiftID, DateTime closeDate, Guid docID, bool isConfirmed, List<Guid> productionProductCharacteristicIDs, GammaEntities gammaDb = null):this()
        {
            GammaBase = gammaDb ?? DB.GammaDb;
            PlaceID = placeID;
            ShiftID = shiftID;
            CloseDate = closeDate;

           IsConfirmed = isConfirmed;

                
            TankGroupContainer = new DocMaterialTankGroupContainer(docID, PlaceID);
            CurrentTankRemaindersView = new DocMaterialTankRemaindersViewModel(PlaceID, TankGroupContainer);
            DocMaterialCompositionCalculations = new DocMaterialProduction(PlaceID, ShiftID, CloseDate, TankGroupContainer);
            DocMaterialCompositionCalculations.LoadProductionMaterials(docID, productionProductCharacteristicIDs);
            PlaceWithdrawalMaterialTypeID = GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID).First();
            DocMaterialProductionDirectCalculationsGrid = new DocMaterialProductionDirectCalculationMaterialViewModel(PlaceID, ShiftID, CloseDate, docID, isConfirmed, productionProductCharacteristicIDs);
            //WithdrawalMaterialsGrid.LoadProductionMaterials(docID, productionProductCharacteristicIDs);
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
        //private List<Guid> ProductionProductCharacteristicIDs { get; set; }
        public DocMaterialTankGroupContainer TankGroupContainer { get; set; }

        private bool IsConfirmed { get; }
        public bool IsReadOnly => !(DB.HaveWriteAccess("DocMaterialProductions") || WorkSession.DBAdmin) || IsConfirmed;

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
        public DelegateCommand BackMaterialTabCommand { get; private set; }

        //public DelegateCommand<byte> MaterialTabActivateCommand { get; private set; }

        public ICommand<CellValue> MaterialRowUpdatedCommand { get; private set; }
        private void OnMaterialRowUpdated(CellValue value)
        {
            //var selectedMaterial = SelectedMaterialTabIndex == 0 ? SelectedDocMaterialProduction : SelectedMaterialTabIndex == 1 ? SelectedWithdrawalMaterial : null;
            DocMaterialCompositionCalculations.MaterialChanged(SelectedMaterialTabIndex, SelectedDocMaterialProduction);
            
        }

        //private void MaterialTabActivate (byte activateTabIndex)
        //{
        //    SelectedMaterialTabIndex = activateTabIndex;
        //}

        
      
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
               /* if (value == 1)
                {
                    var currentWithdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>(DocMaterialProductionDirectCalculationMaterials.WithdrawalMaterials);
                    DocMaterialProductionDirectCalculationMaterials?.WithdrawalMaterials?.Clear();
                    foreach (var item in currentWithdrawalMaterials)
                    {
                        DocMaterialProductionDirectCalculationMaterials?.WithdrawalMaterials?.Add(item);
                    }
                }*/
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
            //var removeItems = DocMaterialProductionDirectCalculationMaterials.WithdrawalMaterials.Where(d => d.NomenclatureID == SelectedDocMaterialProduction.NomenclatureID && (d.CharacteristicID == SelectedDocMaterialProduction.CharacteristicID || (d.CharacteristicID == null && SelectedDocMaterialProduction.CharacteristicID == null))).ToArray();
            //foreach (var item in removeItems)
            //    DocMaterialProductionDirectCalculationMaterials.WithdrawalMaterials.Remove(item);
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
            //DirectCalculation
            //CompositionCalculation
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
            //WithdrawalMaterialsIn?.Clear();
            //WithdrawalMaterialsOut?.Clear();
            //WithdrawalMaterialsRemainder?.Clear();
        }

        public override bool SaveToModel(Guid docId)
        {
            if (IsReadOnly) return true;
            using (var gammaBase = DB.GammaDb)
            {
                /*if (gammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceGroupID).First() == 0 && PlaceWithdrawalMaterialTypeID == 2 && DocMaterialProductionDirectCalculationMaterials.WithdrawalMaterials?.Where(x => x.ProductionProductCharacteristicID == null || x.ProductionProductCharacteristicID == Guid.Empty).Count() > 0)
                {
                    MessageBox.Show("Внимание! Есть использованные материалы, которые не распределены на произведенную за смену продукцию!", "Ошибка сохранения",
                            MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
                */
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

                foreach (var docDelete in doc.DocMaterialProductDocs)
                {
                    if (DocMaterialCompositionCalculations.Docs == null || !DocMaterialCompositionCalculations.Docs.Any(d => d.DocID == docDelete.DocID))
                        doc.DocMaterialProductDocs.Remove(gammaBase.Docs.First(d => d.DocID == docDelete.DocID));
                }

                if (DocMaterialCompositionCalculations.Docs != null)
                    foreach (var docAdd in DocMaterialCompositionCalculations.Docs)
                    {
                        doc.DocMaterialProductDocs.Add(gammaBase.Docs.First(d => d.DocID == docAdd.DocID));
                    }

                if (CurrentTankRemaindersView != null)
                {
                    CurrentTankRemaindersView.SaveToModel(docId);
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
            return true;
        }
                
        //public void SetProductionProductCharacteristics(List<Guid> productionProductCharacteristicIDs)
        //{
        //    DocMaterialCompositionCalculations?.SetProductionProductCharacteristics(productionProductCharacteristicIDs);
        //}

    }
}