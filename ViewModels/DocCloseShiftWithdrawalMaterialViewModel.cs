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
using Gamma.DialogViewModels;
using System.ComponentModel;

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
        public DocCloseShiftWithdrawalMaterialViewModel()
        {
            AddDocCloseShiftMaterialCommand = new DelegateCommand(AddDocCloseShiftMaterial, () => !IsReadOnly);
            DeleteDocCloseShiftMaterialCommand = new DelegateCommand(DeleteDocCloseShiftMaterial, () => !IsReadOnly);
            BackMaterialTabCommand = new DelegateCommand(() => SelectedMaterialTabIndex = 0);

            MaterialRowUpdatedCommand = new DelegateCommand<CellValue>(OnMaterialRowUpdated);
            ProcessCellCommand = new DelegateCommand<List<object>>(ProcessCell);

            ShowProductCommand = new DelegateCommand(() =>
                    MessageManager.OpenDocProduct(SelectedProduct.ProductKind, SelectedProduct.ProductID),
                    () => SelectedProduct != null);
            ShowBeginProductCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(SelectedBeginProduct.ProductKind, SelectedBeginProduct.ProductID),
                () => SelectedBeginProduct != null);
            ShowEndProductCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(SelectedEndProduct.ProductKind, SelectedEndProduct.ProductID),
                () => SelectedEndProduct != null);
            ShowUtilizationProductCommand = new DelegateCommand(() =>
                MessageManager.OpenDocProduct(SelectedUtilizationProduct.ProductKind, SelectedUtilizationProduct.ProductID),
                () => SelectedUtilizationProduct != null);
            MaterialTabActivateCommand = new DelegateCommand<byte>(MaterialTabActivate);
        }
        public DocCloseShiftWithdrawalMaterialViewModel(int placeID, int shiftID, DateTime closeDate, GammaEntities gammaDb = null):this()
        {
            GammaBase = gammaDb ?? DB.GammaDb;
            PlaceID = placeID;
            ShiftID = shiftID;
            CloseDate = closeDate;

            DocCloseShiftWithdrawalMaterials = new DocCloseShiftWithdrawalMaterial(PlaceID, ShiftID, CloseDate);
            PlaceWithdrawalMaterialTypeID = WorkSession.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID).First();
        }

        public DocCloseShiftWithdrawalMaterialViewModel(int placeID, int shiftID, DateTime closeDate, Guid docID, List<DocCloseShiftWithdrawalMaterial.Product> products, bool isConfirmed, List<Guid> productionProductCharacteristicIDs, GammaEntities gammaDb = null):this()
        {
            GammaBase = gammaDb ?? DB.GammaDb;
            PlaceID = placeID;
            ShiftID = shiftID;
            CloseDate = closeDate;

            ProductionProducts = products;
            IsConfirmed = isConfirmed;

            DocCloseShiftWithdrawalMaterials = new DocCloseShiftWithdrawalMaterial(PlaceID, ShiftID, CloseDate);
            DocCloseShiftWithdrawalMaterials.LoadWithdrawalMaterials(docID, productionProductCharacteristicIDs);
            PlaceWithdrawalMaterialTypeID = WorkSession.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID).First();
        }

        private MaterialType CurrentMaterialType;

        private int _placeID;
        private int PlaceID
        {
            get { return _placeID; }
            set
            {
                _placeID = value;
                var placeGroupID = WorkSession.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceGroupID).First();
                if (placeGroupID == 0)
                {
                    CurrentMaterialType = MaterialType.MaterialsSGB;
                    IsVisibleQuantityDismiss = true;
                }
                else if (placeGroupID == 2)
                {
                    CurrentMaterialType = MaterialType.MaterialsSGI;
                    IsVisibleQuantityDismiss = false;
                }
                
            }
        }
        private int ShiftID;
        DateTime CloseDate;

        public DocCloseShiftWithdrawalMaterial DocCloseShiftWithdrawalMaterials { get; set; }
        //private List<Guid> ProductionProductCharacteristicIDs { get; set; }

        private bool IsConfirmed { get; set; }
        public bool IsReadOnly => !(DB.HaveWriteAccess("DocCloseShiftMaterials") || WorkSession.DBAdmin) || IsConfirmed;

        public void UpdateIsConfirmed(bool isConfirmed)
        {
            IsConfirmed = isConfirmed;
        }

        public DocCloseShiftRemainder SelectedBeginProduct { get; set; }
        public DocCloseShiftRemainder SelectedEndProduct { get; set; }
        public Product SelectedUtilizationProduct { get; set; }

        public DelegateCommand AddDocCloseShiftMaterialCommand { get; private set; }
        public DelegateCommand DeleteDocCloseShiftMaterialCommand { get; private set; }
        public DelegateCommand BackMaterialTabCommand { get; private set; }

        public DelegateCommand ShowProductCommand { get; private set; }
        public DelegateCommand ShowBeginProductCommand { get; private set; }
        public DelegateCommand ShowEndProductCommand { get; private set; }
        public DelegateCommand ShowUtilizationProductCommand { get; private set; }
        public DelegateCommand<byte> MaterialTabActivateCommand { get; private set; }

        public ICommand<CellValue> MaterialRowUpdatedCommand { get; private set; }
        private void OnMaterialRowUpdated(CellValue value)
        {
            var selectedMaterial = SelectedMaterialTabIndex == 0 ? SelectedDocCloseShiftMaterial : SelectedMaterialTabIndex == 1 ? SelectedWithdrawalMaterial : null;
            DocCloseShiftWithdrawalMaterials.MaterialChanged(SelectedMaterialTabIndex, selectedMaterial);
            //if (value.Property == "Quantity")
            //    Console.WriteLine("OnCellValueChanged");
        }

        private void MaterialTabActivate (byte activateTabIndex)
        {
            SelectedMaterialTabIndex = activateTabIndex;
        }

        private List<DocCloseShiftWithdrawalMaterial.Product> ProductionProducts { get; set; }

        public bool IsWithdrawalMaterialIn { get; set; }
        public string NameWithdrawalMaterialIn { get; set; }
        public bool IsWithdrawalMaterial { get; set; }

        /// <summary>
        /// Показывать колонку Распущено только для БДМ
        /// </summary>
        public bool IsVisibleQuantityDismiss { get; set; }

        public string HeaderQuantityField => @"Кол-во, кг/рул/пач";
        public Product SelectedProduct { get; set; }

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

        private int _selectedMaterialTabIndex = 0;
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

        public ICommand<List<object>> ProcessCellCommand { get; private set; }
        public void ProcessCell(List<object> cellInfo)
        {
            if (cellInfo != null && cellInfo.Count == 2)
                switch (cellInfo[1].ToString())
                {
                    case "QuantityRemainderAtBegin":
                        SelectedMaterialTabIndex = 2;
                        break;
                    case "QuantityIn":
                        SelectedMaterialTabIndex = 3;
                        break;
                    case "QuantityOut":
                        SelectedMaterialTabIndex = 4;
                        break;
                    case "QuantityUtil":
                        SelectedMaterialTabIndex = 5;
                        break;
                    case "QuantityRemainderAtEnd":
                        SelectedMaterialTabIndex = 6;
                        break;
                }
                
        }
        /*
        public class CellInfoMultiValueConverter : MarkupExtensionBase, IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return values.ToList();
            }
            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public abstract class MarkupExtensionBase : MarkupExtension
        {
            public override object ProvideValue(IServiceProvider serviceProvider)
            {
                return this;
            }
        }
        */
        private void DeleteDocCloseShiftMaterial()
        {
            if (SelectedDocCloseShiftMaterial == null) return;
            var removeItems = DocCloseShiftWithdrawalMaterials.WithdrawalMaterials.Where(d => d.NomenclatureID == SelectedDocCloseShiftMaterial.NomenclatureID && (d.CharacteristicID == SelectedDocCloseShiftMaterial.CharacteristicID || (d.CharacteristicID == null && SelectedDocCloseShiftMaterial.CharacteristicID == null))).ToArray();
            foreach (var item in removeItems)
                DocCloseShiftWithdrawalMaterials.WithdrawalMaterials.Remove(item);
            DocCloseShiftWithdrawalMaterials.DocCloseShiftMaterials.Remove(SelectedDocCloseShiftMaterial);
            
        }

        private void DebugFunc()
        {
            System.Diagnostics.Debug.Print("Кол-во задано");
        }

        private void AddDocCloseShiftMaterial()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, SetMaterialNomenclature);
            MessageManager.FindNomenclature(CurrentMaterialType);
        }

        private void SetMaterialNomenclature(Nomenclature1CMessage msg)
        {
            Messenger.Default.Unregister<Nomenclature1CMessage>(this);

            var model = new ChooseMeasureUnitDialogModel(msg.Nomenclature1CID);
            var okCommand = new UICommand()
            {
                Caption = "OK",
                IsCancel = false,
                IsDefault = true,
                Command = new DelegateCommand<CancelEventArgs>(
            x => DebugFunc(),
            x => model.WithdrawalTypeID != null && model.MeasureUnitID != null && model.MeasureUnitID != Guid.Empty),
            };
            var cancelCommand = new UICommand()
            {
                Id = MessageBoxResult.Cancel,
                Caption = "Отмена",
                IsCancel = true,
                IsDefault = false,
            };
            var dialogService = GetService<IDialogService>("ChooseMeasureUnitDialog");
            var result = dialogService.ShowDialog(
                dialogCommands: new List<UICommand>() { okCommand, cancelCommand },
                title: "Укажите параметры",
                viewModel: model);
            if (result != okCommand) return;
            if (!(model.WithdrawalTypeID != null && model.MeasureUnitID != null && model.MeasureUnitID != Guid.Empty))
            {
                MessageBox.Show("Не удалось определить параметры");
                return;
            }

            using (var gammaBase = DB.GammaDb)
            {
                var nomenclatureInfo =
                gammaBase.C1CNomenclature.Include(n => n.C1CMeasureUnitStorage)
                    .First(n => n.C1CNomenclatureID == msg.Nomenclature1CID);
                var m = new Dictionary<Guid, String>();
                m.Add(model.MeasureUnitID, model.MeasureUnitName);
                DocCloseShiftWithdrawalMaterials.MaterialNomenclatureChanged(nomenclatureInfo, model.WithdrawalTypeID == 1, m);
            }
        }

        public void FillWithdrawalMaterials(List<Guid> productionProductCharacteristicIDs, List<DocCloseShiftWithdrawalMaterial.Product> products, DocCloseShiftUnwinderRemainderViewModel spoolUnwinderRemainders = null, bool IsFillEnd = true)
        {
            ProductionProducts = products;
            DocCloseShiftWithdrawalMaterials.FillWithdrawalMaterials(productionProductCharacteristicIDs, spoolUnwinderRemainders, IsFillEnd);
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
            var ir = IsReadOnly;
            if (IsReadOnly) return true;
            using (var gammaBase = DB.GammaDb)
            {
                if (WorkSession.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceGroupID).First() == 0 && PlaceWithdrawalMaterialTypeID == 2 && DocCloseShiftWithdrawalMaterials.WithdrawalMaterials?.Where(x => x.ProductionProductCharacteristicID == null || x.ProductionProductCharacteristicID == Guid.Empty).Count() > 0)
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

                    foreach (DocCloseShiftMaterialTypes docCloseShiftMaterialType in Enum.GetValues(typeof(DocCloseShiftMaterialTypes)))
                    {
                        decimal? quantity = null;
                        switch (docCloseShiftMaterialType)
                        {
                            case DocCloseShiftMaterialTypes.Withdrawal:
                                quantity = material.QuantityWithdrawalMaterial;
                                break;
                            case DocCloseShiftMaterialTypes.In:
                                quantity = material.QuantityIn;
                                break;
                            case DocCloseShiftMaterialTypes.Out:
                                quantity = material.QuantityOut;
                                break;
                            case DocCloseShiftMaterialTypes.RemainderAtEnd:
                                quantity = material.QuantityRemainderAtEnd;
                                break;
                            case DocCloseShiftMaterialTypes.RemainderAtBegin:
                                quantity = material.QuantityRemainderAtBegin;
                                break;
                            case DocCloseShiftMaterialTypes.Utilization:
                                quantity = material.QuantityUtil;
                                break;
                            case DocCloseShiftMaterialTypes.Experimental:
                                quantity = material.QuantityExperimental;
                                break;
                            case DocCloseShiftMaterialTypes.RePack:
                                quantity = material.QuantityRePack;
                                break;
                            case DocCloseShiftMaterialTypes.Standard:
                                quantity = material.StandardQuantity;
                                break;
                            /*case DocCloseShiftMaterialTypes.Dismiss:
                                quantity = material.QuantityDismiss;
                                break;*/
                        }
                        if (quantity != null || docCloseShiftMaterialType == DocCloseShiftMaterialTypes.RemainderAtBegin)
                        {
                            var item = materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == (int)docCloseShiftMaterialType).FirstOrDefault();
                            if (item == null)
                                docCloseShift.DocCloseShiftMaterials.Add(new DocCloseShiftMaterials
                                {
                                    DocID = docId,
                                    C1CNomenclatureID = material.NomenclatureID,
                                    C1CCharacteristicID = material.CharacteristicID,
                                    DocCloseShiftMaterialID = SqlGuidUtil.NewSequentialid(),
                                    DocCloseShiftMaterialTypeID = (int)docCloseShiftMaterialType,
                                    Quantity = quantity == null ? 0 : (decimal)(material.MeasureUnit == "кг  " ? quantity / 1000 : quantity),
                                    C1CMeasureUnitID = material.MeasureUnitID,
                                    WithdrawByFact = material.WithdrawByFact
                                });
                            else
                            {
                                item.Quantity = quantity == null ? 0 : (decimal)(material.MeasureUnit == "кг  " ? quantity / 1000 : quantity);
                            }
                        }
                        else
                        {
                            if (materialInDB.Any(d => d.DocCloseShiftMaterialTypeID == (int)docCloseShiftMaterialType))
                                gammaBase.DocCloseShiftMaterials.RemoveRange(materialInDB.Where(d => d.DocCloseShiftMaterialTypeID == (int)docCloseShiftMaterialType));
                        }
                    }
                }
                var docWithdrawalIDs = docCloseShift.DocCloseShiftWithdrawals.Select(d => d.DocID).ToList();
                gammaBase.DocWithdrawalMaterials.RemoveRange(
                    gammaBase.DocWithdrawalMaterials.Where(d => docWithdrawalIDs.Contains(d.DocID)));
                gammaBase.DocWithdrawal.RemoveRange(
                    gammaBase.DocWithdrawal.Where(d => docWithdrawalIDs.Contains(d.DocID)));

                if (DocCloseShiftWithdrawalMaterials != null && DocCloseShiftWithdrawalMaterials.MaterialsDismiss != null)
                    foreach (var id in DocCloseShiftWithdrawalMaterials.MaterialsDismiss.Select(m => m.DocID))
                    {
                        docCloseShift.DocCloseShiftDocs.Add(gammaBase.Docs.First(d => d.DocID == id));
                    }

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