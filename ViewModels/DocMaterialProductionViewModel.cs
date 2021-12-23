﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using DevExpress.Mvvm;
using System;
using System.Linq;
using Gamma.Interfaces;
using System.Collections.ObjectModel;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Models;
using System.Data.Entity.SqlServer;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Input;
using System.Drawing;

// ReSharper disable MemberCanBePrivate.Global

namespace Gamma.ViewModels
{
    class DocMaterialProductionViewModel : SaveImplementedViewModel
    {
        public DocMaterialProductionViewModel(OpenDocMaterialProductionMessage msg)
        {
            Bars.Add(ReportManager.GetReportBar("DocMaterialProduction", VMID));
            isInitialize = true;
            //gammaBase = gammaBase ?? DB.GammaDb;
            Doc = (from d in GammaBase.Docs where d.DocID == msg.DocID select d).FirstOrDefault();
                          
            if (Doc == null)
            {
                DocID = SqlGuidUtil.NewSequentialid();
                Date = DB.CurrentDateTime;
                IsConfirmed = false;
                PlaceID = WorkSession.PlaceID;
                IsNewDoc = true;
                Title = "Остатки сырья и материалов";
                ShiftID = WorkSession.ShiftID;
                if (!WorkSession.IsUsedInOneDocMaterialDirectCalcAndComposition)
                    IsDocCompositions = MessageBox.Show("Вы создаете документ по сырью?","Новый документ",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes;
                DB.AddLogMessageInformation("Create new DocMaterialProduction @DocID='"+DocID+"' @Date='"+Date+"' @PlaceID="+PlaceID+" @ShiftID="+ ShiftID + " @IsDocCompositions=" + IsDocCompositions);
            }
            else
            {
                DocID = Doc.DocID;
                Date = Doc.Date;
                Number = Doc.Number;
                IsConfirmed = Doc.IsConfirmed;
                PlaceID = (int)Doc.PlaceID;
                IsNewDoc = false;
                Title = "Остатки сырья и материалов №" + Doc.Number;
                ShiftID = Doc.ShiftID;
                var d = Doc.Comment?.ToLower().IndexOf("сырье");
                    var f = Doc.Comment?.ToLower().IndexOf("материалы");
                if (String.IsNullOrEmpty(Doc.Comment) || (Doc.Comment?.ToLower().IndexOf("сырье") >= 0 && Doc.Comment?.ToLower().IndexOf("материалы") >= 0))
                    IsDocCompositions = null;
                else
                    IsDocCompositions = !(Doc.Comment?.IndexOf("Материалы") >= 0);
                DB.AddLogMessageInformation("Open DocMaterialProduction @DocID='" + DocID + "' @Date='" + Date + "' @PlaceID=" + PlaceID + " @ShiftID=" + ShiftID + " @IsDocCompositions=" + IsDocCompositions);
            }
            
            var paperMachinePlace = DB.GetPaperMachinePlace(PlaceID) ?? 0;
            var productionProductsListFromMadeProducts = new List<Characteristic>(GammaBase.vProductsInfo
                                .Where(r => r.PlaceID == paperMachinePlace && r.ShiftID == ShiftID 
                                    && r.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime(Date)) && r.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime(Date))
                                    )
                                .Select(r => new Characteristic
                                {
                                    CharacteristicID = (Guid)r.C1CCharacteristicID,
                                    CharacteristicName = r.NomenclatureName
                                }).Distinct());

            var productionProductsListFromProductionTask = new List<Characteristic>(GammaBase.ProductionTaskBatches
                            .Where(r => r.ProductionTaskStateID == (int)ProductionTaskStates.InProduction && r.ProductionTasks.Where(p => p.PlaceID == paperMachinePlace && p.C1CCharacteristicID != null).FirstOrDefault() != null)
                            .Select(r => new Characteristic
                            {
                                CharacteristicID = (Guid)r.ProductionTasks.Where(p => p.PlaceGroupID == 0 && p.C1CCharacteristicID != null).FirstOrDefault().C1CCharacteristicID,
                                CharacteristicName = r.ProductionTasks.Where(p => p.PlaceGroupID == 0 && p.C1CCharacteristicID != null).FirstOrDefault().C1CNomenclature.Name + " " + r.ProductionTasks.Where(p => p.PlaceGroupID == 0 && p.C1CCharacteristicID != null).FirstOrDefault().C1CCharacteristics.Name
                            }).Distinct());

            ProductionProductsList = new List<Characteristic>();
            if (productionProductsListFromMadeProducts != null)
            {
                ProductionProductsList.AddRange(productionProductsListFromMadeProducts);
            }
            if (productionProductsListFromProductionTask != null)
            {
                ProductionProductsList.AddRange(productionProductsListFromProductionTask.Where(p => !ProductionProductsList.Select(r => r.CharacteristicID).Contains(p.CharacteristicID)));
            }
            


            if (!IsNewDoc)
                ActiveProductionProduct = new List<Object>(GammaBase.DocMaterialProducts
                            .Where(r => r.DocID == Doc.DocID)
                            .Select(r => new Characteristic
                            {
                                CharacteristicID = (Guid)r.C1CCharacteristicID,
                                CharacteristicName = r.C1CNomenclature.Name + " " + r.C1CCharacteristics.Name
                            }));

            var productionProductCharacteristicIDs = ProductionProductsList.Count() > 0 ?
                ProductionProductsList.Select(r => r.CharacteristicID).ToList() : new List<Guid>();

            if (!IsNewDoc)
                foreach (var item in GammaBase.DocMaterialProducts
                            .Where(r => r.DocID == Doc.DocID && !productionProductCharacteristicIDs.Contains((Guid)r.C1CCharacteristicID))
                            .Select(r => new Characteristic
                            {
                                CharacteristicID = (Guid)r.C1CCharacteristicID,
                                CharacteristicName = r.C1CNomenclature.Name + " " + r.C1CCharacteristics.Name
                            }))
                ProductionProductsList.Add(item);

            if (IsNewDoc && (!IsDocCompositions ?? false))
            {
                 ActiveProductionProduct = new List<object>(ProductionProductsList
                             .Select(r => new Characteristic
                             {
                                 CharacteristicID = (Guid)r.CharacteristicID,
                                 CharacteristicName = r.CharacteristicName
                             })); 
            }
            /*
            var activeProductionProductCharacteristicIDs =  new List<Guid>();
            if (!IsNewDoc)
                foreach (object item in ((List<object>)ActiveProductionProduct))
                    activeProductionProductCharacteristicIDs.Add(((Characteristic)item).CharacteristicID);
            else
                if (!IsDocCompositions ?? false)
                foreach (object item in ProductionProductsList)
                    activeProductionProductCharacteristicIDs.Add(((Characteristic)item).CharacteristicID); 
                    */
            CurrentViewModelGrid = Doc == null
                       ? new DocMaterialProductionGridViewModel(PlaceID, (int)ShiftID, Date, IsDocCompositions)
                       : new DocMaterialProductionGridViewModel(PlaceID, (int)ShiftID, Date, Doc.DocID, IsConfirmed, productionProductCharacteristicIDs, IsDocCompositions);

            var grid = CurrentViewModelGrid as IFillClearGrid;
            if (grid != null)
            {
                FillGridCommand = new DelegateCommand(grid.FillGrid, () => !IsConfirmed && CanEditable() && ActiveProductionProduct?.Count > 0);
                ClearGridCommand = new DelegateCommand(grid.ClearGrid, () => !IsConfirmed && CanEditable() && ActiveProductionProduct?.Count > 0);
                FillGridWithNoEndCommand = new DelegateCommand(grid.FillGridWithNoFillEnd, () => !IsConfirmed && FillGridWithNoEndCanEnable() && ActiveProductionProduct?.Count > 0);
            }
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
            isInitialize = false;
        }


        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != VMID) return;
            //if (msg.VMID != (CurrentViewModelGrid as IBarImplemented)?.VMID) return;
            //if (!IsValid) return;
            if (CanSaveExecute())
                SaveToModel();
            else if (IsNewDoc) return;
            ReportManager.PrintReport(msg.ReportID, Doc.DocID);
        }
        private Docs Doc { get; set; }
        private Guid DocID { get; set; }
        private bool IsNewDoc { get; set; }
        private int PlaceID { get; set; }
        public string Number { get; set; }
        /// <summary>
        /// Окончание смены
        /// </summary>
        private DateTime ShiftEndTime { get; set; }
        private DateTime _date;
        public DateTime Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
                ShiftEndTime = DB.GetShiftEndTimeFromDate(Date.AddHours(-1));
                var grid = CurrentViewModelGrid as DocMaterialProductionGridViewModel;
                if (grid != null && value != null)
                {
                    grid.CloseDate = value;
                    DB.AddLogMessageInformation("Change Date @Date" + Date + " @PlaceID=" + PlaceID + " @ShiftID=" + ShiftID + " activeProductionProduct to " );

                }
            }
        }

        private bool? IsDocCompositions { get; set; }
        public bool IsVisibleActiveProductionProducts => IsDocCompositions ?? true;
        private bool isInitialize { get; set; }
        public bool _isConfirmed { get; set; }
        public bool IsConfirmed
        {
            get { return _isConfirmed; }
            set
            {
                if (_isConfirmed != value)
                {
                    if (_isConfirmed && !value && Doc?.DocID != null)
                    {
                        if (isDocUsedNextPlace)
                        {
                            MessageBox.Show("Внимание! Документ уже использован следующим переделом! Требуется Очистить документ следующего передела");
                            return;
                        }
                    }
                    _isConfirmed = value;
                    if (!isInitialize)
                    {
                        if (value)
                        {
                            if (SaveToModel(false))
                                MessageBox.Show("Документ сохранен.");
                            else
                            {
                                MessageBox.Show("Ошибка! Документ не сохранен.");
                                _isConfirmed = !value;
                                return;
                            }
                        }
                        else
                        {
                            using (var gammaBase = DB.GammaDb)
                            {
                                var doc = gammaBase.Docs.Where(d => d.DocID == DocID).FirstOrDefault();
                                if (doc != null)
                                    doc.IsConfirmed = IsConfirmed;
                                gammaBase.SaveChanges();
                            }
                        }
                        (CurrentViewModelGrid as DocMaterialProductionGridViewModel)?.ChangeConfirmed(value);
                    }
                    RaisePropertyChanged("IsConfirmed");
                    RaisePropertyChanged("IsDateReadOnly");
                }
            }
        }

        public bool isDocUsedNextPlace
        {
            get
            {
                using (var gammaBase = DB.GammaDb)
                {
                    return (gammaBase.Docs.Any(d => d.DocID == DocID && d.DocCloseShift.Any()) || gammaBase.Docs.Any(d => d.DocID == DocID && d.DocMaterialProduct.Any()));
                }
            }
        }

        public byte? ShiftID { get; set; }

        private List<Object> _activeProductionProduct { get; set; }
        public List<Object> ActiveProductionProduct
        {
            get { return _activeProductionProduct; }
            set
            {
                _activeProductionProduct = value;
                var grid = CurrentViewModelGrid as DocMaterialProductionGridViewModel;
                if (grid != null && value != null)
                {
                    var activeProductionProductCharacteristicIDs = new List<Guid>();
                    foreach (object item in ((List<object>)value))
                        activeProductionProductCharacteristicIDs.Add(((Characteristic)item).CharacteristicID);
                    grid.DocMaterialCompositionCalculations?.SetProductionProductCharacteristics(activeProductionProductCharacteristicIDs);
                    grid.DocMaterialProductionDirectCalculationsGrid?.DirectCalculationMaterials?.SetProductionProductCharacteristics(activeProductionProductCharacteristicIDs);
                    DB.AddLogMessageInformation("Change DocMaterialTankRemainders @Date" + Date+" @PlaceID=" + PlaceID + " @ShiftID=" + ShiftID + " activeProductionProduct to " + string.Join(",",  activeProductionProductCharacteristicIDs));

                }
                RaisePropertyChanged("ActiveProductionProduct");
            }

        }

        private List<Characteristic> _productionProductsList { get; set; }
        public List<Characteristic> ProductionProductsList
        {
            get { return _productionProductsList; }
            set
            {
                _productionProductsList = value;
                RaisePropertyChanged("ProductionProductsList");
            }
        }

        //public bool IsReadOnly => !(!IsConfirmed && DB.HaveWriteAccess("DocMaterialProductions"));

        public bool IsDateReadOnly
        {
            get
            {
#if DEBUG
                return !(!IsConfirmed && DB.HaveWriteAccess("DocMaterialProductions"));
#else
                return !(!IsNewDoc && !IsConfirmed && DB.HaveWriteAccess("DocMaterialProductions") && WorkSession.ShiftID == 0); 
#endif
            }
        }

        public bool ConfirmedIsReadOnly => !CanEditable();

        public bool CanEditable ()
        {
#if DEBUG
            return DB.HaveWriteAccess("DocMaterialProductions");
#else
            return DB.HaveWriteAccess("DocMaterialProductions") && (WorkSession.ShiftID == 0 || (WorkSession.ShiftID == ShiftID && WorkSession.PlaceID == PlaceID) && (DB.CurrentDateTime < ShiftEndTime.AddHours(1)));
#endif
        }

        public bool FillGridWithNoEndCanEnable()
        {
            return DB.HaveWriteAccess("DocMaterialProductions") && WorkSession.ShiftID == 0;
        }

        private SaveImplementedViewModel _currentViewModelGrid;
        public SaveImplementedViewModel CurrentViewModelGrid 
        {
            get
            {
                return _currentViewModelGrid;
            }
            set
            {
                _currentViewModelGrid = value;
                //Bars = (_currentViewModelGrid as IBarImplemented).Bars;
            }
        }

        public DelegateCommand FillGridCommand { get; set; }
        public DelegateCommand FillGridWithNoEndCommand { get; set; }
        public DelegateCommand ClearGridCommand { get; set; }

        public override bool SaveToModel()
        {
            return SaveToModel(true);
        }

        private System.IO.MemoryStream PrintScreen()
        {

            Bitmap printscreen = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            System.IO.MemoryStream result = new System.IO.MemoryStream();
            printscreen.Save(result, System.Drawing.Imaging.ImageFormat.Png);
            return result;

        }

        private void addLogMessageInformationWithPrintScreen(bool isCheckCanSaveExecute)
        {
            try
            {
                DB.AddLogMessageInformationWithImage("Save document DocMaterialProduction @IsNewDoc=" + IsNewDoc + " @DocID =" + DocID + " @Date=" + Date + " @PlaceID=" + PlaceID + " @ShiftID=" + ShiftID + "@isCheckCanSaveExecute=" + isCheckCanSaveExecute, PrintScreen());
            }
            catch
            {
                DB.AddLogMessageInformation("Save document DocMaterialProduction (error save image) @IsNewDoc=" + IsNewDoc + " @DocID =" + DocID + " @Date=" + Date + " @PlaceID=" + PlaceID + " @ShiftID=" + ShiftID + "@isCheckCanSaveExecute=" + isCheckCanSaveExecute);
            }
        }

        public bool SaveToModel(bool isCheckCanSaveExecute)
        {
            //PrintScreen();
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            addLogMessageInformationWithPrintScreen(isCheckCanSaveExecute);

            if ((isCheckCanSaveExecute && !CanSaveExecute()) || DocIsUsedNextPlace())
            {
                Mouse.OverrideCursor = null;
                return false;
            }
            if (IsNewDoc)
            {
                Doc = new Docs()
                {
                    DocID = DocID,
                    DocTypeID = (byte)DocTypes.DocMaterialProduction,
                    Date = Date,
                    UserID = WorkSession.UserID,
                    PlaceID = WorkSession.PlaceID,
                    ShiftID = WorkSession.ShiftID,
                    IsConfirmed = IsConfirmed,
                    PrintName = WorkSession.PrintName,
                    PersonGuid = WorkSession.PersonID.ToString() == "00000000-0000-0000-0000-000000000000" ? null : WorkSession.PersonID,
                    Comment = WorkSession.IsUsedInOneDocMaterialDirectCalcAndComposition ? "Сырье и материалы" : (bool)IsDocCompositions ? "Сырье" : "Материалы"
                };
                GammaBase.Docs.Add(Doc);
            }
            Doc.IsConfirmed = IsConfirmed;

            if (Doc.Date != Date)
                Doc.Date = Date;

            List<Guid> activeProductionProductCharacteristicIds = new List<Guid>();
            if (ActiveProductionProduct != null)
                foreach (object item in ((List<object>)ActiveProductionProduct))
                    activeProductionProductCharacteristicIds.Add(((Characteristic)item).CharacteristicID);
            GammaBase.DocMaterialProducts.RemoveRange(GammaBase.DocMaterialProducts.Where(p => p.DocID == Doc.DocID));
            foreach (var productCharacteristicItem in GammaBase.C1CCharacteristics.Where(p => activeProductionProductCharacteristicIds.Contains(p.C1CCharacteristicID)))
            {
                var docMaterialProduct = new DocMaterialProducts
                {
                    DocMaterialProductID = SqlGuidUtil.NewSequentialid(),
                    DocID = Doc.DocID,
                    C1CNomenclatureID = productCharacteristicItem.C1CNomenclatureID,
                    C1CCharacteristicID = productCharacteristicItem.C1CCharacteristicID
                };
                GammaBase.DocMaterialProducts.Add(docMaterialProduct);
            }

            GammaBase.SaveChanges();

            CurrentViewModelGrid?.SaveToModel(Doc.DocID);
            IsNewDoc = false;

            Messenger.Default.Send(new RefreshMessage { });
            Mouse.OverrideCursor = null;
            return true;
        }

        private Guid VMID { get; } = Guid.NewGuid();

        public List<BarViewModel> Bars { get; set; } = new List<BarViewModel>();
        /*private ObservableCollection<BarViewModel> _bars;
        public ObservableCollection<BarViewModel> Bars
        {
            get
            {
                return _bars;
            }
            set
            {
                _bars = value;
                RaisePropertyChanged("Bars");
            }
        }*/
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Title { get; set; }
        public override bool CanSaveExecute()
        {
#if DEBUG
            return base.CanSaveExecute() && CurrentViewModelGrid.IsValid;
#else
            return base.CanSaveExecute() && CanEditable() && !isDocUsedNextPlace && !IsConfirmed && CurrentViewModelGrid.IsValid;
#endif
        }
        private bool DocIsUsedNextPlace()
        {
            using (var gammaBase = DB.GammaDb)
            {
                if (isDocUsedNextPlace)
                {
                    MessageBox.Show("Внимание! Документ уже использован следующим переделом! Требуется Очистить документ следующего передела");
                    return true;
                }
            }
            return false;
        }

    protected override void SaveToModelAndClose()
        {
            base.SaveToModelAndClose();
            Messenger.Default.Send(new CloseMessage());
        }
    }
}
