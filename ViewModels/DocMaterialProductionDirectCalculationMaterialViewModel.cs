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
    public class DocMaterialProductionDirectCalculationMaterialViewModel : /*DbEditItemWithNomenclatureViewModel*/SaveImplementedViewModel, ICheckedAccess
    {

        /// <summary>
        /// Initializes a new instance of the DocMaterialProductionDirectCalculationMaterialViewModel class.
        /// </summary>
        public DocMaterialProductionDirectCalculationMaterialViewModel(int placeID, int shiftID, DateTime closeDate, GammaEntities gammaDb = null)
        {
            GammaBase = gammaDb ?? DB.GammaDb;
            PlaceID = placeID;
            ShiftID = shiftID;
            CloseDate = closeDate;

            DB.AddLogMessageInformation("Open DocMaterialProductionDirectCalculation @CloseDate=" + CloseDate + " @PlaceID=" + PlaceID + " @ShiftID=" + ShiftID);

            AddDirectCalculationMaterialCommand = new DelegateCommand(AddDirectCalculationMaterial, () => !IsReadOnly);
            DeleteDirectCalculationMaterialCommand = new DelegateCommand(DeleteDirectCalculationMaterial, () => !IsReadOnly);

            DirectCalculationMaterials = new DocMaterialProductionDirectCalculationMaterial(PlaceID, ShiftID, CloseDate);
        }

        public DocMaterialProductionDirectCalculationMaterialViewModel(int placeID, int shiftID, DateTime closeDate, Guid docID, bool isConfirmed, List<Guid> productionProductCharacteristicIDs, GammaEntities gammaDb = null)
        {
            GammaBase = gammaDb ?? DB.GammaDb;
            PlaceID = placeID;
            ShiftID = shiftID;
            CloseDate = closeDate;

            IsConfirmed = isConfirmed;

            DB.AddLogMessageInformation("Open DocMaterialProductionDirectCalculation @CloseDate=" + CloseDate + " @PlaceID=" + PlaceID + " @ShiftID=" + ShiftID + " @IsConfirmed=" + IsConfirmed);

            AddDirectCalculationMaterialCommand = new DelegateCommand(AddDirectCalculationMaterial, () => !IsReadOnly);
            DeleteDirectCalculationMaterialCommand = new DelegateCommand(DeleteDirectCalculationMaterial, () => !IsReadOnly);

            DirectCalculationMaterials = new DocMaterialProductionDirectCalculationMaterial(PlaceID, ShiftID, CloseDate);
            DirectCalculationMaterials.LoadProductionMaterials(docID, productionProductCharacteristicIDs);
        }

        private MaterialType CurrentMaterialType;

        private int _placeID;
        private int PlaceID
        {
            get { return _placeID; }
            set
            {
                _placeID = value;
                var placeGroupID = GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID).First();
                if (placeGroupID == 0)
                    CurrentMaterialType = MaterialType.MaterialsSGB;
                else if (placeGroupID == 2)
                    CurrentMaterialType = MaterialType.MaterialsSGI;

            }
        }
        private int ShiftID;
        DateTime CloseDate;
        public void SetCloseDate(DateTime value)
        {
            CloseDate = value;
        }

        public DocMaterialProductionDirectCalculationMaterial DirectCalculationMaterials { get; set; }
        
        private bool IsConfirmed { get; set; } = false;
        public void ChangeConfirmed(bool isConfirmed)
        {
            IsConfirmed = isConfirmed;
            RaisePropertyChanged("IsReadOnly");
        }
        public bool IsReadOnly => !(DB.HaveWriteAccess("DocMaterialProductions") || WorkSession.DBAdmin) || IsConfirmed;

        public DelegateCommand AddDirectCalculationMaterialCommand { get; private set; }
        public DelegateCommand DeleteDirectCalculationMaterialCommand { get; private set; }

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
            }
        }

        public WithdrawalMaterial SelectedWithdrawalMaterial { get; set; }

        public DocMaterialProductionDirectCalculationItem SelectedDirectCalculationMaterial { get; set; }

        private void DeleteDirectCalculationMaterial()
        {
            if (SelectedDirectCalculationMaterial == null) return;
            DirectCalculationMaterials.Materials.Remove(SelectedDirectCalculationMaterial);
            DB.AddLogMessageInformation("Delete material DocMaterialProductionDirectCalculation @CloseDate=" + CloseDate + " @PlaceID=" + PlaceID + " @ShiftID=" + ShiftID + " @SelectedDirectCalculationMaterial.NomenclatureID=" + SelectedDirectCalculationMaterial.NomenclatureID);
        }

        private void AddDirectCalculationMaterial()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, SetMaterialNomenclature);
            MessageManager.FindNomenclature(CurrentMaterialType);
        }

        private void SetMaterialNomenclature(Nomenclature1CMessage msg)
        {
            Messenger.Default.Unregister<Nomenclature1CMessage>(this);
            var isWithdrawalByFact = MessageBox.Show("Материал учитывается по факту?", "Вопрос", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
            using (var gammaBase = DB.GammaDb)
            {
                var nomenclatureInfo =
                gammaBase.C1CNomenclature.Include(n => n.C1CMeasureUnitStorage)
                    .First(n => n.C1CNomenclatureID == msg.Nomenclature1CID);
                DirectCalculationMaterials.MaterialNomenclatureChanged(nomenclatureInfo, isWithdrawalByFact);
            }
            DB.AddLogMessageInformation("Add material DocMaterialProductionDirectCalculation @CloseDate=" + CloseDate + " @PlaceID=" + PlaceID + " @ShiftID=" + ShiftID + " @msg.Nomenclature1CID=" + msg.Nomenclature1CID);
        }

        public void Clear()
        {
            DirectCalculationMaterials?.Clear();
        }

        public override bool SaveToModel(Guid docId)
        {
            if (IsReadOnly) return true;
            //сохраняется на уровень выше.
            return true;
        }

    }
}