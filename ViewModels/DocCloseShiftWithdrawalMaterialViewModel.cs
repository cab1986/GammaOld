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
            AddWithdrawalMaterialRemainderCommand = new DelegateCommand(AddWithdrawalMaterialRemainder, () => !IsReadOnly);
            DeleteWithdrawalMaterialRemainderCommand = new DelegateCommand(DeleteWithdrawalMaterialRemainder, () => !IsReadOnly);

            MaterialRowUpdatedCommand = new DelegateCommand<CellValue>(OnMaterialRowUpdated);
            DocCloseShiftWithdrawalMaterials = new DocCloseShiftWithdrawalMaterial(PlaceID, ShiftID, CloseDate);
        }

        public DocCloseShiftWithdrawalMaterialViewModel(int placeID, int shiftID, DateTime closeDate, Guid docID, ObservableCollection<PaperBase> spools, bool isConfirmed, List<Guid> productionProductCharacteristicIDs, GammaEntities gammaDb = null)
        {
            GammaBase = gammaDb ?? DB.GammaDb;
            PlaceID = placeID;
            ShiftID = shiftID;
            CloseDate = closeDate;

            Spools = spools;
            IsConfirmed = isConfirmed;

            AddWithdrawalMaterialInCommand = new DelegateCommand(AddWithdrawalMaterialIn, () => !IsReadOnly);
            DeleteWithdrawalMaterialInCommand = new DelegateCommand(DeleteWithdrawalMaterialIn, () => !IsReadOnly);
            AddWithdrawalMaterialOutCommand = new DelegateCommand(AddWithdrawalMaterialOut, () => !IsReadOnly);
            DeleteWithdrawalMaterialOutCommand = new DelegateCommand(DeleteWithdrawalMaterialOut, () => !IsReadOnly);
            AddWithdrawalMaterialRemainderCommand = new DelegateCommand(AddWithdrawalMaterialRemainder, () => !IsReadOnly);
            DeleteWithdrawalMaterialRemainderCommand = new DelegateCommand(DeleteWithdrawalMaterialRemainder, () => !IsReadOnly);

            MaterialRowUpdatedCommand = new DelegateCommand<CellValue>(OnMaterialRowUpdated);
            
            DocCloseShiftWithdrawalMaterials = new DocCloseShiftWithdrawalMaterial(PlaceID, ShiftID, CloseDate);
            DocCloseShiftWithdrawalMaterials.LoadWithdrawalMaterials(docID, productionProductCharacteristicIDs);
           
        }

        private int PlaceID;
        private int ShiftID;
        DateTime CloseDate;

        public DocCloseShiftWithdrawalMaterial DocCloseShiftWithdrawalMaterials { get; set; }
        //private List<Guid> ProductionProductCharacteristicIDs { get; set; }

        private bool IsConfirmed { get; }
        public bool IsReadOnly => !(DB.HaveWriteAccess("DocCloseShiftWithdrawalMaterials") || WorkSession.DBAdmin) || IsConfirmed;

        public DelegateCommand AddWithdrawalMaterialInCommand { get; private set; }
        public DelegateCommand DeleteWithdrawalMaterialInCommand { get; private set; }
        public DelegateCommand AddWithdrawalMaterialOutCommand { get; private set; }
        public DelegateCommand DeleteWithdrawalMaterialOutCommand { get; private set; }
        public DelegateCommand AddWithdrawalMaterialRemainderCommand { get; private set; }
        public DelegateCommand DeleteWithdrawalMaterialRemainderCommand { get; private set; }


        public ICommand<CellValue> MaterialRowUpdatedCommand { get; private set; }
        private void OnMaterialRowUpdated(CellValue value)
        {
            var selectedMaterial = SelectedMaterialTabIndex == 0 ? SelectedWithdrawalMaterialIn : SelectedMaterialTabIndex == 1 ? SelectedWithdrawalMaterialOut : SelectedMaterialTabIndex == 2 ? SelectedWithdrawalMaterialRemainder : SelectedMaterialTabIndex == 3 ? SelectedWithdrawalMaterial : null;
            DocCloseShiftWithdrawalMaterials.MaterialChanged(SelectedMaterialTabIndex, selectedMaterial);
            //if (value.Property == "Quantity")
            //    Console.WriteLine("OnCellValueChanged");
        }

        private ObservableCollection<PaperBase> Spools { get; set; }

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
            MessageManager.FindNomenclature(MaterialType.MaterialsSGB);
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
            MessageManager.FindNomenclature(MaterialType.MaterialsSGB);
        }

        public WithdrawalMaterial SelectedWithdrawalMaterialRemainder { get; set; }

        private void DeleteWithdrawalMaterialRemainder()
        {
            if (SelectedWithdrawalMaterialRemainder == null) return;
            DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsRemainder.Remove(SelectedWithdrawalMaterialRemainder);
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
                DocCloseShiftWithdrawalMaterials.MaterialNomenclatureChanged(nomenclatureInfo);
            }
        }

        public void FillWithdrawalMaterials(List<Guid> productionProductCharacteristicIDs)
        {
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
            if (DocCloseShiftWithdrawalMaterials.WithdrawalMaterials?.Where(x => x.ProductionProductCharacteristicID == null || x.ProductionProductCharacteristicID == Guid.Empty).Count() > 0)
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
                            Quantity = material.Quantity
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
                            Quantity = material.Quantity
                        });
                    }
                if (DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsRemainder != null)
                    foreach (var material in DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsRemainder)
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
                            Quantity = material.Quantity
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
                    var spoolIDs = Spools.Where(x => x.CharacteristicID == productionProductCharacteristicID).Select(x => x.ProductID).ToList();
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
                            WithdrawByFact = !material.QuantityIsReadOnly
                        });
                    }
                }

                gammaBase.SaveChanges();
            }
            return true;
        }

    }
}