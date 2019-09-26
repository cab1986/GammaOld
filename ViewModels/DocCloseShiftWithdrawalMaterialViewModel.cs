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

            AddWithdrawalMaterialInCommand = new DelegateCommand(AddWithdrawalMaterialIn, () => !IsReadOnly);
            DeleteWithdrawalMaterialInCommand = new DelegateCommand(DeleteWithdrawalMaterialIn, () => !IsReadOnly);
            AddWithdrawalMaterialOutCommand = new DelegateCommand(AddWithdrawalMaterialOut, () => !IsReadOnly);
            DeleteWithdrawalMaterialOutCommand = new DelegateCommand(DeleteWithdrawalMaterialOut, () => !IsReadOnly);
            AddWithdrawalMaterialRemainderAtBeginCommand = new DelegateCommand(AddWithdrawalMaterialRemainderAtBegin, () => !IsReadOnly);
            DeleteWithdrawalMaterialRemainderAtBeginCommand = new DelegateCommand(DeleteWithdrawalMaterialRemainderAtBegin, () => !IsReadOnly);
            AddWithdrawalMaterialRemainderAtEndCommand = new DelegateCommand(AddWithdrawalMaterialRemainderAtEnd, () => !IsReadOnly);
            DeleteWithdrawalMaterialRemainderAtEndCommand = new DelegateCommand(DeleteWithdrawalMaterialRemainderAtEnd, () => !IsReadOnly);

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

            AddWithdrawalMaterialInCommand = new DelegateCommand(AddWithdrawalMaterialIn, () => !IsReadOnly);
            DeleteWithdrawalMaterialInCommand = new DelegateCommand(DeleteWithdrawalMaterialIn, () => !IsReadOnly);
            AddWithdrawalMaterialOutCommand = new DelegateCommand(AddWithdrawalMaterialOut, () => !IsReadOnly);
            DeleteWithdrawalMaterialOutCommand = new DelegateCommand(DeleteWithdrawalMaterialOut, () => !IsReadOnly);
            AddWithdrawalMaterialRemainderAtBeginCommand = new DelegateCommand(AddWithdrawalMaterialRemainderAtBegin, () => !IsReadOnly);
            DeleteWithdrawalMaterialRemainderAtBeginCommand = new DelegateCommand(DeleteWithdrawalMaterialRemainderAtBegin, () => !IsReadOnly);
            AddWithdrawalMaterialRemainderAtEndCommand = new DelegateCommand(AddWithdrawalMaterialRemainderAtEnd, () => !IsReadOnly);
            DeleteWithdrawalMaterialRemainderAtEndCommand = new DelegateCommand(DeleteWithdrawalMaterialRemainderAtEnd, () => !IsReadOnly);

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

        public DelegateCommand AddWithdrawalMaterialInCommand { get; private set; }
        public DelegateCommand DeleteWithdrawalMaterialInCommand { get; private set; }
        public DelegateCommand AddWithdrawalMaterialOutCommand { get; private set; }
        public DelegateCommand DeleteWithdrawalMaterialOutCommand { get; private set; }
        public DelegateCommand AddWithdrawalMaterialRemainderAtBeginCommand { get; private set; }
        public DelegateCommand DeleteWithdrawalMaterialRemainderAtBeginCommand { get; private set; }
        public DelegateCommand AddWithdrawalMaterialRemainderAtEndCommand { get; private set; }
        public DelegateCommand DeleteWithdrawalMaterialRemainderAtEndCommand { get; private set; }


        public ICommand<CellValue> MaterialRowUpdatedCommand { get; private set; }
        private void OnMaterialRowUpdated(CellValue value)
        {
            var selectedMaterial = SelectedMaterialTabIndex == 0 ? SelectedWithdrawalMaterialRemainderAtBegin : SelectedMaterialTabIndex == 1 ? SelectedWithdrawalMaterialIn : SelectedMaterialTabIndex == 2 ? SelectedWithdrawalMaterialOut : SelectedMaterialTabIndex == 3 ? SelectedWithdrawalMaterialRemainderAtEnd : SelectedMaterialTabIndex == 4 ? SelectedWithdrawalMaterial : null;
            DocCloseShiftWithdrawalMaterials.MaterialChanged(SelectedMaterialTabIndex, selectedMaterial);
            //if (value.Property == "Quantity")
            //    Console.WriteLine("OnCellValueChanged");
        }

        
        private List<DocCloseShiftWithdrawalMaterial.Product> ProductionProducts { get; set; }

        public bool IsWithdrawalMaterialIn { get; set; }
        public string NameWithdrawalMaterialIn { get; set; }
        public bool IsWithdrawalMaterialOut { get; set; }
        public bool IsWithdrawalMaterialRemainderAtBegin { get; set; }
        public bool IsWithdrawalMaterialRemainderAtEnd { get; set; }
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
                    IsWithdrawalMaterialOut = false;
                    IsWithdrawalMaterialRemainderAtBegin = false;
                    IsWithdrawalMaterialRemainderAtEnd = false;
                    IsWithdrawalMaterial = true;
                    if (DocCloseShiftWithdrawalMaterials.WithdrawalMaterials?.Count()>0 && DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsIn?.Count() == 0)
                    {
                        DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsIn = DocCloseShiftWithdrawalMaterials.WithdrawalMaterials;
                    }
                }
                else if (value == 2)
                {
                    IsWithdrawalMaterialIn = true;
                    NameWithdrawalMaterialIn = "Принято";
                    IsWithdrawalMaterialOut = true;
                    IsWithdrawalMaterialRemainderAtBegin = true;
                    IsWithdrawalMaterialRemainderAtEnd = true;
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
                if (value == 4)
                {
                    var currentWithdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>(DocCloseShiftWithdrawalMaterials.WithdrawalMaterials);
                    DocCloseShiftWithdrawalMaterials?.WithdrawalMaterials?.Clear();
                    foreach (var item in currentWithdrawalMaterials)
                    {
                        DocCloseShiftWithdrawalMaterials?.WithdrawalMaterials?.Add(item);
                    }
                }
            }
        }

        public WithdrawalMaterial SelectedWithdrawalMaterial { get; set; }

        public WithdrawalMaterial SelectedWithdrawalMaterialIn { get; set; }

        private void DeleteWithdrawalMaterialIn()
        {
            if (SelectedWithdrawalMaterialIn == null) return;
            DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsIn.Remove(SelectedWithdrawalMaterialIn);
        }

        private void AddWithdrawalMaterialIn()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, SetMaterialNomenclature);
            MessageManager.FindNomenclature(CurrentMaterialType);
        }

        public WithdrawalMaterial SelectedWithdrawalMaterialOut { get; set; }

        private void DeleteWithdrawalMaterialOut()
        {
            if (SelectedWithdrawalMaterialOut == null) return;
            DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsOut.Remove(SelectedWithdrawalMaterialOut);
        }

        private void AddWithdrawalMaterialOut()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, SetMaterialNomenclature);
            MessageManager.FindNomenclature(CurrentMaterialType);
        }

        public WithdrawalMaterial SelectedWithdrawalMaterialRemainderAtBegin { get; set; }

        private void DeleteWithdrawalMaterialRemainderAtBegin()
        {
            if (SelectedWithdrawalMaterialRemainderAtBegin == null) return;
            DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsRemainderAtBegin.Remove(SelectedWithdrawalMaterialRemainderAtBegin);
        }

        private void AddWithdrawalMaterialRemainderAtBegin()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, SetMaterialNomenclature);
            MessageManager.FindNomenclature(CurrentMaterialType);
        }

        public WithdrawalMaterial SelectedWithdrawalMaterialRemainderAtEnd { get; set; }

        private void DeleteWithdrawalMaterialRemainderAtEnd()
        {
            if (SelectedWithdrawalMaterialRemainderAtEnd == null) return;
            DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsRemainderAtEnd.Remove(SelectedWithdrawalMaterialRemainderAtEnd);
        }

        private void AddWithdrawalMaterialRemainderAtEnd()
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
            if (PlaceWithdrawalMaterialTypeID == 2 && DocCloseShiftWithdrawalMaterials.WithdrawalMaterials?.Where(x => x.ProductionProductCharacteristicID == null || x.ProductionProductCharacteristicID == Guid.Empty).Count() > 0)
            {
                MessageBox.Show("Внимание! Есть использованные материалы, которые не распределены на произведенную за смену продукцию!", "Ошибка сохранения",
                        MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            if (IsReadOnly) return true;
            using (var gammaBase = DB.GammaDb)
            {
                var docCloseShift = gammaBase.Docs
               .First(d => d.DocID == docId);
                if (docCloseShift.DocCloseShiftMaterials == null)
                    docCloseShift.DocCloseShiftMaterials = new List<DocCloseShiftMaterials>();

                gammaBase.DocCloseShiftMaterials.RemoveRange(docCloseShift.DocCloseShiftMaterials);
                if (DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsIn != null)
                    foreach (var material in DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsIn)
                    {
                        docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                        {
                            DocID = docId,
                            C1CNomenclatureID = material.NomenclatureID,
                            C1CCharacteristicID = material.CharacteristicID,
                            DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                            DocCloseShiftMaterialTypeID = 1,
                            Quantity = material.Quantity,
                            C1CMeasureUnitID = material.MeasureUnitID,
                            WithdrawByFact = material.WithdrawByFact
                        });
                    }
                if (DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsOut != null)
                    foreach (var material in DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsOut)
                    {
                        docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                        {
                            DocID = docId,
                            C1CNomenclatureID = material.NomenclatureID,
                            C1CCharacteristicID = material.CharacteristicID,
                            DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                            DocCloseShiftMaterialTypeID = 2,
                            Quantity = material.Quantity,
                            C1CMeasureUnitID = material.MeasureUnitID,
                            WithdrawByFact = material.WithdrawByFact
                        });
                    }
                if (DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsRemainderAtBegin != null)
                    foreach (var material in DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsRemainderAtBegin)
                    {
                        docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                        {
                            DocID = docId,
                            C1CNomenclatureID = material.NomenclatureID,
                            C1CCharacteristicID = material.CharacteristicID,
                            DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                            DocCloseShiftMaterialTypeID = 4,
                            Quantity = material.Quantity,
                            C1CMeasureUnitID = material.MeasureUnitID,
                            WithdrawByFact = material.WithdrawByFact
                        });
                    }
                if (DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsRemainderAtEnd != null)
                    foreach (var material in DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsRemainderAtEnd)
                    {
                        docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                        {
                            DocID = docId,
                            C1CNomenclatureID = material.NomenclatureID,
                            C1CCharacteristicID = material.CharacteristicID,
                            DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                            DocCloseShiftMaterialTypeID = 3,
                            Quantity = material.Quantity,
                            C1CMeasureUnitID = material.MeasureUnitID,
                            WithdrawByFact = material.WithdrawByFact
                        });
                    }
                if (DocCloseShiftWithdrawalMaterials.WithdrawalMaterials != null)
                    foreach (var material in DocCloseShiftWithdrawalMaterials.WithdrawalMaterials)
                    {
                        docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                        {
                            DocID = docId,
                            C1CNomenclatureID = material.NomenclatureID,
                            C1CCharacteristicID = material.CharacteristicID,
                            DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                            DocCloseShiftMaterialTypeID = 0,
                            Quantity = material.Quantity,
                            C1CMeasureUnitID = material.MeasureUnitID,
                            WithdrawByFact = material.WithdrawByFact
                            //ProductionProductCharacteristicID = material.ProductionProductCharacteristicID
                        });
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