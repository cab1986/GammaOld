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
            
            MaterialTabActivateCommand = new DelegateCommand<byte>(MaterialTabActivate);
            MaterialTabActivate(0);
        }
        public DocMaterialProductionGridViewModel(int placeID, int shiftID, DateTime closeDate, GammaEntities gammaDb = null):this()
        {
            GammaBase = gammaDb ?? DB.GammaDb;
            PlaceID = placeID;
            ShiftID = shiftID;
            CloseDate = closeDate;

            TankGroupContainer = new DocMaterialTankGroupContainer(PlaceID);
            CurrentTankRemaindersView = new DocMaterialTankRemaindersViewModel(PlaceID, TankGroupContainer);
            DocMaterialProductions = new DocMaterialProduction(PlaceID, ShiftID, CloseDate, TankGroupContainer);
            PlaceWithdrawalMaterialTypeID = GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID).First();
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
            DocMaterialProductions = new DocMaterialProduction(PlaceID, ShiftID, CloseDate, TankGroupContainer);
            DocMaterialProductions.LoadProductionMaterials(docID, productionProductCharacteristicIDs);
            PlaceWithdrawalMaterialTypeID = GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID).First();
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

        public DocMaterialProduction DocMaterialProductions { get; set; }
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

        public DelegateCommand AddDocMaterialProductionCommand { get; private set; }
        public DelegateCommand DeleteDocMaterialProductionCommand { get; private set; }
        public DelegateCommand BackMaterialTabCommand { get; private set; }

        public DelegateCommand<byte> MaterialTabActivateCommand { get; private set; }

        public ICommand<CellValue> MaterialRowUpdatedCommand { get; private set; }
        private void OnMaterialRowUpdated(CellValue value)
        {
            //var selectedMaterial = SelectedMaterialTabIndex == 0 ? SelectedDocMaterialProduction : SelectedMaterialTabIndex == 1 ? SelectedWithdrawalMaterial : null;
            DocMaterialProductions.MaterialChanged(SelectedMaterialTabIndex, SelectedDocMaterialProduction);
            
        }

        private void MaterialTabActivate (byte activateTabIndex)
        {
            SelectedMaterialTabIndex = activateTabIndex;
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
               /* if (value == 1)
                {
                    var currentWithdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>(DocCloseShiftWithdrawalMaterials.WithdrawalMaterials);
                    DocCloseShiftWithdrawalMaterials?.WithdrawalMaterials?.Clear();
                    foreach (var item in currentWithdrawalMaterials)
                    {
                        DocCloseShiftWithdrawalMaterials?.WithdrawalMaterials?.Add(item);
                    }
                }*/
                RaisePropertiesChanged("SelectedMaterialTabIndex");
            }
        }

        public WithdrawalMaterial SelectedWithdrawalMaterial { get; set; }

        public DocMaterialProductionItem SelectedDocMaterialProduction { get; set; }

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
            //var removeItems = DocCloseShiftWithdrawalMaterials.WithdrawalMaterials.Where(d => d.NomenclatureID == SelectedDocMaterialProduction.NomenclatureID && (d.CharacteristicID == SelectedDocMaterialProduction.CharacteristicID || (d.CharacteristicID == null && SelectedDocMaterialProduction.CharacteristicID == null))).ToArray();
            //foreach (var item in removeItems)
            //    DocCloseShiftWithdrawalMaterials.WithdrawalMaterials.Remove(item);
            DocMaterialProductions.DocMaterialProductions.Remove(SelectedDocMaterialProduction);
            TankGroupContainer.DeleteComposition(SelectedDocMaterialProduction.NomenclatureID);
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
                DocMaterialProductions.MaterialNomenclatureChanged(nomenclatureInfo);
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
            DocMaterialProductions.FillProductionMaterials(IsFillEnd);
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
            DocMaterialProductions?.ClearFromButton();
            //WithdrawalMaterialsIn?.Clear();
            //WithdrawalMaterialsOut?.Clear();
            //WithdrawalMaterialsRemainder?.Clear();
        }

        public override bool SaveToModel(Guid docId)
        {
            if (IsReadOnly) return true;
            using (var gammaBase = DB.GammaDb)
            {
                /*if (gammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceGroupID).First() == 0 && PlaceWithdrawalMaterialTypeID == 2 && DocCloseShiftWithdrawalMaterials.WithdrawalMaterials?.Where(x => x.ProductionProductCharacteristicID == null || x.ProductionProductCharacteristicID == Guid.Empty).Count() > 0)
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
                    if (!DocMaterialProductions.DocMaterialProductions.Any(d => d.NomenclatureID == materialRemove.C1CNomenclatureID && (d.CharacteristicID == materialRemove.C1CCharacteristicID || (d.CharacteristicID == null && materialRemove.C1CCharacteristicID == null))))
                    {
                        gammaBase.DocMaterialProductions.Remove(materialRemove);
                    }
                };

                foreach (var material in DocMaterialProductions.DocMaterialProductions)
                {
                    var materialInDB = materialsInDB.Where(d => d.C1CNomenclatureID == material.NomenclatureID && (d.C1CCharacteristicID == material.CharacteristicID || (d.C1CCharacteristicID == null && material.CharacteristicID == null)));
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
                            var item = materialInDB.Where(d => d.DocMaterialProductionTypeID == (int)docMaterialProductionType).FirstOrDefault();
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
                                    WithdrawByFact = material.WithdrawByFact
                                });
                            else
                            {
                                item.Quantity = quantity ?? 0;
                            }
                        }
                        else
                        {
                            if (materialInDB.Any(d => d.DocMaterialProductionTypeID == (int)docMaterialProductionType))
                                gammaBase.DocMaterialProductions.RemoveRange(materialInDB.Where(d => d.DocMaterialProductionTypeID == (int)docMaterialProductionType));
                        }
                    }
                }

                foreach (var docDelete in doc.DocMaterialProductDocs)
                {
                    if (DocMaterialProductions.Docs == null || !DocMaterialProductions.Docs.Any(d => d.DocID == docDelete.DocID))
                        doc.DocMaterialProductDocs.Remove(gammaBase.Docs.First(d => d.DocID == docDelete.DocID));
                }

                if (DocMaterialProductions.Docs != null)
                    foreach (var docAdd in DocMaterialProductions.Docs)
                    {
                        doc.DocMaterialProductDocs.Add(gammaBase.Docs.First(d => d.DocID == docAdd.DocID));
                    }

                if (CurrentTankRemaindersView != null)
                {
                    CurrentTankRemaindersView.SaveToModel(docId);
                }
                    /*
                    foreach (var material in DocMaterialProductions.DocMaterialProductions)
                        {
                        var docMaterialProductionTypeID = 4;
                        var materialInDB = materialsInDB.Where(d => d.C1CNomenclatureID == material.NomenclatureID && (d.C1CCharacteristicID == material.CharacteristicID || (d.C1CCharacteristicID == null && material.CharacteristicID == null)));
                        if (material.QuantityIn != null)
                        {
                            var item = materialInDB.Where(d => d.DocMaterialProductionTypeID == docMaterialProductionTypeID).FirstOrDefault();
                            if (item == null)
                                doc.DocMaterialProductions.Add(new DocMaterialProductions
                                {
                                    DocID = docId,
                                    C1CNomenclatureID = material.NomenclatureID,
                                    C1CCharacteristicID = material.CharacteristicID,
                                    DocMaterialProductionID = SqlGuidUtil.NewSequentialid(),
                                    DocMaterialProductionTypeID = docMaterialProductionTypeID,
                                    Quantity = material.QuantityIn ?? 0,
                                    C1CMeasureUnitID = material.MeasureUnitID,
                                    WithdrawByFact = material.WithdrawByFact
                                });
                            else
                            {
                                item.Quantity = material.QuantityIn ?? 0;
                            }
                        }
                        else
                        {
                            if (materialInDB.Any(d => d.DocMaterialProductionTypeID == docMaterialProductionTypeID))
                                gammaBase.DocMaterialProductions.RemoveRange(materialInDB.Where(d => d.DocMaterialProductionTypeID == docMaterialProductionTypeID));
                        }
                        docMaterialProductionTypeID = 5;
                        if (material.QuantitySend != null)
                        {
                            var item = materialInDB.Where(d => d.DocMaterialProductionTypeID == docMaterialProductionTypeID).FirstOrDefault();
                            if (item == null)
                                docCloseShift.DocMaterialProductions.Add(new DocMaterialProductions
                                {
                                    DocID = docId,
                                    C1CNomenclatureID = material.NomenclatureID,
                                    C1CCharacteristicID = material.CharacteristicID,
                                    DocMaterialProductionID = SqlGuidUtil.NewSequentialid(),
                                    DocMaterialProductionTypeID = docMaterialProductionTypeID,
                                    Quantity = material.QuantityOut ?? 0,
                                    C1CMeasureUnitID = material.MeasureUnitID
                                    //,WithdrawByFact = material.WithdrawByFact
                                });
                            else
                            {
                                item.Quantity = material.QuantityOut ?? 0;
                            }
                        }
                        else
                        {
                            if (materialInDB.Any(d => d.DocMaterialProductionTypeID == docMaterialProductionTypeID))
                                gammaBase.DocMaterialProductions.RemoveRange(materialInDB.Where(d => d.DocMaterialProductionTypeID == docMaterialProductionTypeID));
                        }
                        //if (material.QuantityRemainderAtBegin != null || !(material.WithdrawByFact ?? false))
                        {
                            var item = materialInDB.Where(d => d.DocMaterialProductionTypeID == 4).FirstOrDefault();
                            if (item == null)
                                docCloseShift.DocMaterialProductions.Add(new DocMaterialProductions
                                {
                                    DocID = docId,
                                    C1CNomenclatureID = material.NomenclatureID,
                                    C1CCharacteristicID = material.CharacteristicID,
                                    DocMaterialProductionID = SqlGuidUtil.NewSequentialid(),
                                    DocMaterialProductionTypeID = 4,
                                    Quantity = material.QuantityRemainderAtBegin ?? 0,
                                    C1CMeasureUnitID = material.MeasureUnitID
                                    //,WithdrawByFact = material.WithdrawByFact
                                });
                            else
                            {
                                item.Quantity = material.QuantityRemainderAtBegin ?? 0;
                            }
                        }
                        if (material.QuantityRemainderAtEnd != null)
                        {
                            var item = materialInDB.Where(d => d.DocMaterialProductionTypeID == 3).FirstOrDefault();
                            if (item == null)
                                docCloseShift.DocMaterialProductions.Add(new DocMaterialProductions
                                {
                                    DocID = docId,
                                    C1CNomenclatureID = material.NomenclatureID,
                                    C1CCharacteristicID = material.CharacteristicID,
                                    DocMaterialProductionID = SqlGuidUtil.NewSequentialid(),
                                    DocMaterialProductionTypeID = 3,
                                    Quantity = material.QuantityRemainderAtEnd ?? 0,
                                    C1CMeasureUnitID = material.MeasureUnitID
                                    //,WithdrawByFact = material.WithdrawByFact
                                });
                            else
                            {
                                item.Quantity = material.QuantityRemainderAtEnd ?? 0;
                            }
                        }
                        else
                        {
                            if (materialInDB.Any(d => d.DocMaterialProductionTypeID == 3))
                                gammaBase.DocMaterialProductions.RemoveRange(materialInDB.Where(d => d.DocMaterialProductionTypeID == 3));
                        }
                        if (material.QuantityUtil != null)
                        {
                            var item = materialInDB.Where(d => d.DocMaterialProductionTypeID == 5).FirstOrDefault();
                            if (item == null)
                                docCloseShift.DocMaterialProductions.Add(new DocMaterialProductions
                                {
                                    DocID = docId,
                                    C1CNomenclatureID = material.NomenclatureID,
                                    C1CCharacteristicID = material.CharacteristicID,
                                    DocMaterialProductionID = SqlGuidUtil.NewSequentialid(),
                                    DocMaterialProductionTypeID = 5,
                                    Quantity = material.QuantityUtil ?? 0,
                                    C1CMeasureUnitID = material.MeasureUnitID
                                    //,WithdrawByFact = material.WithdrawByFact
                                });
                            else
                            {
                                item.Quantity = material.QuantityUtil ?? 0;
                            }
                        }
                        else
                        {
                            if (materialInDB.Any(d => d.DocMaterialProductionTypeID == 5))
                                gammaBase.DocMaterialProductions.RemoveRange(materialInDB.Where(d => d.DocMaterialProductionTypeID == 5));
                        }
                        if (material.QuantityExperimental != null)
                        {
                            var item = materialInDB.Where(d => d.DocMaterialProductionTypeID == 6).FirstOrDefault();
                            if (item == null)
                                docCloseShift.DocMaterialProductions.Add(new DocMaterialProductions
                                {
                                    DocID = docId,
                                    C1CNomenclatureID = material.NomenclatureID,
                                    C1CCharacteristicID = material.CharacteristicID,
                                    DocMaterialProductionID = SqlGuidUtil.NewSequentialid(),
                                    DocMaterialProductionTypeID = 6,
                                    Quantity = material.QuantityExperimental ?? 0,
                                    C1CMeasureUnitID = material.MeasureUnitID
                                    //,WithdrawByFact = material.WithdrawByFact
                                });
                            else
                            {
                                item.Quantity = material.QuantityExperimental ?? 0;
                            }
                        }
                        else
                        {
                            if (materialInDB.Any(d => d.DocMaterialProductionTypeID == 6))
                                gammaBase.DocMaterialProductions.RemoveRange(materialInDB.Where(d => d.DocMaterialProductionTypeID == 6));
                        }
                        if (material.QuantityRePack != null)
                        {
                            var item = materialInDB.Where(d => d.DocMaterialProductionTypeID == 7).FirstOrDefault();
                            if (item == null)
                                docCloseShift.DocMaterialProductions.Add(new DocMaterialProductions
                                {
                                    DocID = docId,
                                    C1CNomenclatureID = material.NomenclatureID,
                                    C1CCharacteristicID = material.CharacteristicID,
                                    DocMaterialProductionID = SqlGuidUtil.NewSequentialid(),
                                    DocMaterialProductionTypeID = 7,
                                    Quantity = material.QuantityRePack ?? 0,
                                    C1CMeasureUnitID = material.MeasureUnitID
                                    //,WithdrawByFact = material.WithdrawByFact
                                });
                            else
                            {
                                item.Quantity = material.QuantityRePack ?? 0;
                            }
                        }
                        else
                        {
                            if (materialInDB.Any(d => d.DocMaterialProductionTypeID == 7))
                                gammaBase.DocMaterialProductions.RemoveRange(materialInDB.Where(d => d.DocMaterialProductionTypeID == 7));
                        }
                        if (material.StandardQuantity != null)
                        {
                            var item = materialInDB.Where(d => d.DocMaterialProductionTypeID == 8).FirstOrDefault();
                            if (item == null)
                                docCloseShift.DocMaterialProductions.Add(new DocMaterialProductions
                                {
                                    DocID = docId,
                                    C1CNomenclatureID = material.NomenclatureID,
                                    C1CCharacteristicID = material.CharacteristicID,
                                    DocMaterialProductionID = SqlGuidUtil.NewSequentialid(),
                                    DocMaterialProductionTypeID = 8,
                                    Quantity = material.StandardQuantity ?? 0,
                                    C1CMeasureUnitID = material.MeasureUnitID
                                    //,WithdrawByFact = material.WithdrawByFact
                                });
                            else
                            {
                                item.Quantity = material.StandardQuantity ?? 0;
                            }
                        }
                        else
                        {
                            if (materialInDB.Any(d => d.DocMaterialProductionTypeID == 8))
                                gammaBase.DocMaterialProductions.RemoveRange(materialInDB.Where(d => d.DocMaterialProductionTypeID == 8));
                        }
                        if (material.QuantityWithdrawalMaterial != null)
                        {
                            var item = materialInDB.Where(d => d.DocMaterialProductionTypeID == 0).FirstOrDefault();
                            if (item == null)
                                docCloseShift.DocMaterialProductions.Add(new DocMaterialProductions
                                {
                                    DocID = docId,
                                    C1CNomenclatureID = material.NomenclatureID,
                                    C1CCharacteristicID = material.CharacteristicID,
                                    DocMaterialProductionID = SqlGuidUtil.NewSequentialid(),
                                    DocMaterialProductionTypeID = 0,
                                    Quantity = material.QuantityWithdrawalMaterial ?? 0,
                                    C1CMeasureUnitID = material.MeasureUnitID
                                    //,WithdrawByFact = material.WithdrawByFact
                                });
                            else
                            {
                                item.Quantity = material.QuantityWithdrawalMaterial ?? 0;
                            }
                        }
                        else
                        {
                            if (materialInDB.Any(d => d.DocMaterialProductionTypeID == 0))
                                gammaBase.DocMaterialProductions.RemoveRange(materialInDB.Where(d => d.DocMaterialProductionTypeID == 0));
                        }
                    }
                    var docWithdrawalIDs = docCloseShift.DocCloseShiftWithdrawals.Select(d => d.DocID).ToList();
                    gammaBase.DocWithdrawalMaterials.RemoveRange(
                        gammaBase.DocWithdrawalMaterials.Where(d => docWithdrawalIDs.Contains(d.DocID)));
                    gammaBase.DocWithdrawal.RemoveRange(
                        gammaBase.DocWithdrawal.Where(d => docWithdrawalIDs.Contains(d.DocID)));


                    docCloseShift.DocCloseShiftWithdrawals.Clear();
                    foreach (var productionProductCharacteristicID in DocCloseShiftWithdrawalMaterials.WithdrawalMaterials.Where(x => x.Quantity != 0).Select(x => x.ProductionProductCharacteristicID).Distinct())
                    {
                        Guid docWithdrawalId;
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
                                        Date = DB.CurrentDateTime, //docCloseShift.Date,
                                        DocTypeID = (byte)DocTypes.DocWithdrawal,
                                        IsConfirmed = docCloseShift.IsConfirmed,
                                        PlaceID = WorkSession.PlaceID,
                                        ShiftID = WorkSession.ShiftID,
                                        UserID = WorkSession.UserID,
                                        PrintName = WorkSession.PrintName
                                    }
                                }
                            );
                        var docWithdrawal = docCloseShift.DocCloseShiftWithdrawals.Where(x => x.DocID == docWithdrawalId).ToList();

                        foreach (var material in DocCloseShiftWithdrawalMaterials.WithdrawalMaterials.Where(x => (x.ProductionProductCharacteristicID == productionProductCharacteristicID || (productionProductCharacteristicID == null && x.ProductionProductCharacteristicID == null)) && x.Quantity != 0))
                        {
                            docCloseShift.DocCloseShiftWithdrawals.FirstOrDefault(d => d.DocID == docWithdrawalId)?.DocWithdrawalMaterials.Add(new DocWithdrawalMaterials()
                            {
                                DocID = docWithdrawalId,
                                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                C1CCharacteristicID = material.CharacteristicID,
                                C1CNomenclatureID = material.NomenclatureID,
                                Quantity = material.Quantity,
                                WithdrawByFact = material.WithdrawByFact //!material.QuantityIsReadOnly
                            });
                        }
                    }
                    */
                    gammaBase.SaveChanges();
            }
            return true;
        }
                
        public void SetProductionProductCharacteristics(List<Guid> productionProductCharacteristicIDs)
        {
            DocMaterialProductions?.SetProductionProductCharacteristics(productionProductCharacteristicIDs);
        }

    }
}