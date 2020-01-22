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

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocCloseShiftWithdrawalMaterialViewModel : /*DbEditItemWithNomenclatureViewModel*/SaveImplementedViewModel, ICheckedAccess
    {
        
        /// <summary>
        /// Initializes a new instance of the DocCloseShiftWithdrawalMaterialViewModel class.
        /// </summary>
        public DocCloseShiftWithdrawalMaterialViewModel(int placeID, int shiftID, DateTime closeDate, GammaEntities gammaDb = null)
        {
            GammaBase = gammaDb ?? DB.GammaDb;
            PlaceID = placeID;
            ShiftID = shiftID;
            CloseDate = closeDate;

            AddDocCloseShiftMaterialCommand = new DelegateCommand(AddDocCloseShiftMaterial, () => !IsReadOnly);
            DeleteDocCloseShiftMaterialCommand = new DelegateCommand(DeleteDocCloseShiftMaterial, () => !IsReadOnly);
            
            MaterialRowUpdatedCommand = new DelegateCommand<CellValue>(OnMaterialRowUpdated);
            DocCloseShiftWithdrawalMaterials = new DocCloseShiftWithdrawalMaterial(PlaceID, ShiftID, CloseDate);
            PlaceWithdrawalMaterialTypeID = GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID).First();
        }

        public DocCloseShiftWithdrawalMaterialViewModel(int placeID, int shiftID, DateTime closeDate, Guid docID, List<DocCloseShiftWithdrawalMaterial.Product> products, bool isConfirmed, List<Guid> productionProductCharacteristicIDs, GammaEntities gammaDb = null)
        {
            GammaBase = gammaDb ?? DB.GammaDb;
            PlaceID = placeID;
            ShiftID = shiftID;
            CloseDate = closeDate;

            ProductionProducts = products;
            IsConfirmed = isConfirmed;

            AddDocCloseShiftMaterialCommand = new DelegateCommand(AddDocCloseShiftMaterial, () => !IsReadOnly);
            DeleteDocCloseShiftMaterialCommand = new DelegateCommand(DeleteDocCloseShiftMaterial, () => !IsReadOnly);
            
            MaterialRowUpdatedCommand = new DelegateCommand<CellValue>(OnMaterialRowUpdated);
            
            DocCloseShiftWithdrawalMaterials = new DocCloseShiftWithdrawalMaterial(PlaceID, ShiftID, CloseDate);
            DocCloseShiftWithdrawalMaterials.LoadWithdrawalMaterials(docID, productionProductCharacteristicIDs);
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

        public DocCloseShiftWithdrawalMaterial DocCloseShiftWithdrawalMaterials { get; set; }
        //private List<Guid> ProductionProductCharacteristicIDs { get; set; }

        private bool IsConfirmed { get; }
        public bool IsReadOnly => !(DB.HaveWriteAccess("DocCloseShiftMaterials") || WorkSession.DBAdmin) || IsConfirmed;

        public DelegateCommand AddDocCloseShiftMaterialCommand { get; private set; }
        public DelegateCommand DeleteDocCloseShiftMaterialCommand { get; private set; }
        

        public ICommand<CellValue> MaterialRowUpdatedCommand { get; private set; }
        private void OnMaterialRowUpdated(CellValue value)
        {
            var selectedMaterial = SelectedMaterialTabIndex == 0 ? SelectedDocCloseShiftMaterial : SelectedMaterialTabIndex == 1 ? SelectedWithdrawalMaterial : null;
            DocCloseShiftWithdrawalMaterials.MaterialChanged(SelectedMaterialTabIndex, selectedMaterial);
            //if (value.Property == "Quantity")
            //    Console.WriteLine("OnCellValueChanged");
        }

        
        private List<DocCloseShiftWithdrawalMaterial.Product> ProductionProducts { get; set; }

        public bool IsWithdrawalMaterialIn { get; set; }
        public string NameWithdrawalMaterialIn { get; set; }
        public bool IsWithdrawalMaterial { get; set; }

        private int _placeWithdrawalMaterialTypeID;
        public int PlaceWithdrawalMaterialTypeID
        {
            get { return _placeWithdrawalMaterialTypeID; }
            set
            {
                _placeWithdrawalMaterialTypeID = value;
                if (value == 1)
                {
                    IsWithdrawalMaterialIn = true;
                    NameWithdrawalMaterialIn = "Использовано";
                    IsWithdrawalMaterial = true;
                    //if (DocCloseShiftWithdrawalMaterials.WithdrawalMaterials?.Count()>0 && DocCloseShiftWithdrawalMaterials.DocCloseShiftMaterials?.Count() == 0)
                    //{
                    //    DocCloseShiftWithdrawalMaterials.DocCloseShiftMaterials = DocCloseShiftWithdrawalMaterials.WithdrawalMaterials;
                    //}
                }
                else if (value == 2)
                {
                    IsWithdrawalMaterialIn = true;
                    NameWithdrawalMaterialIn = "Принято";
                    IsWithdrawalMaterial = true;
                }
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
                if (value == 1)
                {
                    var currentWithdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>(DocCloseShiftWithdrawalMaterials.WithdrawalMaterials);
                    DocCloseShiftWithdrawalMaterials?.WithdrawalMaterials?.Clear();
                    foreach (var item in currentWithdrawalMaterials)
                    {
                        DocCloseShiftWithdrawalMaterials?.WithdrawalMaterials?.Add(item);
                    }
                }
                RaisePropertiesChanged("SelectedMaterialTabIndex");
            }
        }

        public WithdrawalMaterial SelectedWithdrawalMaterial { get; set; }

        public DocCloseShiftMaterial SelectedDocCloseShiftMaterial { get; set; }

        private void DeleteDocCloseShiftMaterial()
        {
            if (SelectedDocCloseShiftMaterial == null) return;
            var removeItems = DocCloseShiftWithdrawalMaterials.WithdrawalMaterials.Where(d => d.NomenclatureID == SelectedDocCloseShiftMaterial.NomenclatureID && (d.CharacteristicID == SelectedDocCloseShiftMaterial.CharacteristicID || (d.CharacteristicID == null && SelectedDocCloseShiftMaterial.CharacteristicID == null))).ToArray();
            foreach (var item in removeItems)
                DocCloseShiftWithdrawalMaterials.WithdrawalMaterials.Remove(item);
            DocCloseShiftWithdrawalMaterials.DocCloseShiftMaterials.Remove(SelectedDocCloseShiftMaterial);
            
        }

        private void AddDocCloseShiftMaterial()
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
                DocCloseShiftWithdrawalMaterials.MaterialNomenclatureChanged(nomenclatureInfo);
            }
        }

        public void FillWithdrawalMaterials(List<Guid> productionProductCharacteristicIDs, List<DocCloseShiftWithdrawalMaterial.Product> products)
        {
            ProductionProducts = products;
            DocCloseShiftWithdrawalMaterials.FillWithdrawalMaterials(productionProductCharacteristicIDs);
        }

        public void Clear()
        {
            DocCloseShiftWithdrawalMaterials?.Clear();

            //WithdrawalMaterialsIn?.Clear();
            //WithdrawalMaterialsOut?.Clear();
            //WithdrawalMaterialsRemainder?.Clear();
        }

        public override bool SaveToModel(Guid docId)
        {
            if (IsReadOnly) return true;
            using (var gammaBase = DB.GammaDb)
            {
                if (gammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceGroupID).First() == 0 && PlaceWithdrawalMaterialTypeID == 2 && DocCloseShiftWithdrawalMaterials.WithdrawalMaterials?.Where(x => x.ProductionProductCharacteristicID == null || x.ProductionProductCharacteristicID == Guid.Empty).Count() > 0)
                {
                    MessageBox.Show("Внимание! Есть использованные материалы, которые не распределены на произведенную за смену продукцию!", "Ошибка сохранения",
                            MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
                var docCloseShift = gammaBase.Docs
               .First(d => d.DocID == docId);
                if (docCloseShift.DocCloseShiftMaterials == null)
                    docCloseShift.DocCloseShiftMaterials = new List<DocCloseShiftMaterials>();

                //gammaBase.DocCloseShiftMaterials.RemoveRange(docCloseShift.DocCloseShiftMaterials);
                var materialsInDB = gammaBase.DocCloseShiftMaterials.Where(d => d.DocID == docId);
                foreach (var materialRemove in materialsInDB)
                {
                    if (!DocCloseShiftWithdrawalMaterials.DocCloseShiftMaterials.Any(d => d.NomenclatureID == materialRemove.C1CNomenclatureID && (d.CharacteristicID == materialRemove.C1CCharacteristicID || (d.CharacteristicID == null && materialRemove.C1CCharacteristicID == null))))
                    {
                        gammaBase.DocCloseShiftMaterials.Remove(materialRemove);
                    }
                };
                foreach (var material in DocCloseShiftWithdrawalMaterials.DocCloseShiftMaterials)
                    {
                    var materialInDB = materialsInDB.Where(d => d.C1CNomenclatureID == material.NomenclatureID && (d.C1CCharacteristicID == material.CharacteristicID || (d.C1CCharacteristicID == null && material.CharacteristicID == null)));
                    if (material.QuantityIn != null)
                    {
                        var item = materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == 1).FirstOrDefault();
                        if (item == null)
                            docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                            {
                                DocID = docId,
                                C1CNomenclatureID = material.NomenclatureID,
                                C1CCharacteristicID = material.CharacteristicID,
                                DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                                DocCloseShiftMaterialTypeID = 1,
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
                        if (materialInDB.Any(d => d.DocCloseShiftMaterialTypeID == 1))
                            gammaBase.DocCloseShiftMaterials.RemoveRange(materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == 1));
                    }
                    if (material.QuantityOut != null)
                    {
                        var item = materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == 2).FirstOrDefault();
                        if (item == null)
                            docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                            {
                                DocID = docId,
                                C1CNomenclatureID = material.NomenclatureID,
                                C1CCharacteristicID = material.CharacteristicID,
                                DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                                DocCloseShiftMaterialTypeID = 2,
                                Quantity = material.QuantityOut ?? 0,
                                C1CMeasureUnitID = material.MeasureUnitID,
                                WithdrawByFact = material.WithdrawByFact
                            });
                        else
                        {
                            item.Quantity = material.QuantityOut ?? 0;
                        }
                    }
                    else
                    {
                        if (materialInDB.Any(d => d.DocCloseShiftMaterialTypeID == 2))
                            gammaBase.DocCloseShiftMaterials.RemoveRange(materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == 2));
                    }
                    //if (material.QuantityRemainderAtBegin != null || !(material.WithdrawByFact ?? false))
                    {
                        var item = materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == 4).FirstOrDefault();
                        if (item == null)
                            docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                            {
                                DocID = docId,
                                C1CNomenclatureID = material.NomenclatureID,
                                C1CCharacteristicID = material.CharacteristicID,
                                DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                                DocCloseShiftMaterialTypeID = 4,
                                Quantity = material.QuantityRemainderAtBegin ?? 0,
                                C1CMeasureUnitID = material.MeasureUnitID,
                                WithdrawByFact = material.WithdrawByFact
                            });
                        else
                        {
                            item.Quantity = material.QuantityRemainderAtBegin ?? 0;
                        }
                    }
                    if (material.QuantityRemainderAtEnd != null)
                    {
                        var item = materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == 3).FirstOrDefault();
                        if (item == null)
                            docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                            {
                                DocID = docId,
                                C1CNomenclatureID = material.NomenclatureID,
                                C1CCharacteristicID = material.CharacteristicID,
                                DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                                DocCloseShiftMaterialTypeID = 3,
                                Quantity = material.QuantityRemainderAtEnd ?? 0,
                                C1CMeasureUnitID = material.MeasureUnitID,
                                WithdrawByFact = material.WithdrawByFact
                            });
                        else
                        {
                            item.Quantity = material.QuantityRemainderAtEnd ?? 0;
                        }
                    }
                    else
                    {
                        if (materialInDB.Any(d => d.DocCloseShiftMaterialTypeID == 3))
                            gammaBase.DocCloseShiftMaterials.RemoveRange(materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == 3));
                    }
                    if (material.QuantityUtil != null)
                    {
                        var item = materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == 5).FirstOrDefault();
                        if (item == null)
                            docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                            {
                                DocID = docId,
                                C1CNomenclatureID = material.NomenclatureID,
                                C1CCharacteristicID = material.CharacteristicID,
                                DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                                DocCloseShiftMaterialTypeID = 5,
                                Quantity = material.QuantityUtil ?? 0,
                                C1CMeasureUnitID = material.MeasureUnitID,
                                WithdrawByFact = material.WithdrawByFact
                            });
                        else
                        {
                            item.Quantity = material.QuantityUtil ?? 0;
                        }
                    }
                    else
                    {
                        if (materialInDB.Any(d => d.DocCloseShiftMaterialTypeID == 5))
                            gammaBase.DocCloseShiftMaterials.RemoveRange(materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == 5));
                    }
                    if (material.QuantityExperimental != null)
                    {
                        var item = materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == 6).FirstOrDefault();
                        if (item == null)
                            docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                            {
                                DocID = docId,
                                C1CNomenclatureID = material.NomenclatureID,
                                C1CCharacteristicID = material.CharacteristicID,
                                DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                                DocCloseShiftMaterialTypeID = 6,
                                Quantity = material.QuantityExperimental ?? 0,
                                C1CMeasureUnitID = material.MeasureUnitID,
                                WithdrawByFact = material.WithdrawByFact
                            });
                        else
                        {
                            item.Quantity = material.QuantityExperimental ?? 0;
                        }
                    }
                    else
                    {
                        if (materialInDB.Any(d => d.DocCloseShiftMaterialTypeID == 6))
                            gammaBase.DocCloseShiftMaterials.RemoveRange(materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == 6));
                    }
                    if (material.QuantityRePack != null)
                    {
                        var item = materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == 7).FirstOrDefault();
                        if (item == null)
                            docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                            {
                                DocID = docId,
                                C1CNomenclatureID = material.NomenclatureID,
                                C1CCharacteristicID = material.CharacteristicID,
                                DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                                DocCloseShiftMaterialTypeID = 7,
                                Quantity = material.QuantityRePack ?? 0,
                                C1CMeasureUnitID = material.MeasureUnitID,
                                WithdrawByFact = material.WithdrawByFact
                            });
                        else
                        {
                            item.Quantity = material.QuantityRePack ?? 0;
                        }
                    }
                    else
                    {
                        if (materialInDB.Any(d => d.DocCloseShiftMaterialTypeID == 7))
                            gammaBase.DocCloseShiftMaterials.RemoveRange(materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == 7));
                    }
                    if (material.QuantityWithdrawalMaterial != null)
                    {
                        var item = materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == 0).FirstOrDefault();
                        if (item == null)
                            docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                            {
                                DocID = docId,
                                C1CNomenclatureID = material.NomenclatureID,
                                C1CCharacteristicID = material.CharacteristicID,
                                DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                                DocCloseShiftMaterialTypeID = 0,
                                Quantity = material.QuantityWithdrawalMaterial ?? 0,
                                C1CMeasureUnitID = material.MeasureUnitID,
                                WithdrawByFact = material.WithdrawByFact
                            });
                        else
                        {
                            item.Quantity = material.QuantityWithdrawalMaterial ?? 0;
                        }
                    }
                    else
                    {
                        if (materialInDB.Any(d => d.DocCloseShiftMaterialTypeID == 0))
                            gammaBase.DocCloseShiftMaterials.RemoveRange(materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == 0));
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
                    var spoolIDs = ProductionProducts.Where(x => x.CharacteristicID == productionProductCharacteristicID).Select(x => x.ProductID).ToList();
                    if (spoolIDs != null)
                    {
                        var docProductionIDs = gammaBase.DocProductionProducts.Where(d => spoolIDs.Contains(d.ProductID)).Select(d => d.DocID).Distinct().ToList();
                        foreach (var docProductionItem in gammaBase.DocProduction.Where(x => docProductionIDs.Contains(x.DocID)))
                        {
                            docProductionItem.DocWithdrawal = docWithdrawal;
                        }
                    }
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

                gammaBase.SaveChanges();
            }
            return true;
        }

    }
}