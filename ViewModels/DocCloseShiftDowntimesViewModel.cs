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
using Gamma.Dialogs;
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
    public class DocCloseShiftDowntimesViewModel : /*DbEditItemWithNomenclatureViewModel*/SaveImplementedViewModel, ICheckedAccess
    {
        /// <summary>
        /// Initializes a new instance of the DocCloseShiftDowntimesViewModel class.
        /// </summary>
        public DocCloseShiftDowntimesViewModel()
        {
            Bars.Add(ReportManager.GetReportBar("DocCloseShiftDowntimes", VMID));
            AddDowntimeCommand = new DelegateCommand(AddDowntime, () => IsAllowEditingDowntimesInDocCloseShift && !IsReadOnly);
            DeleteDowntimeCommand = new DelegateCommand(DeleteDowntime, () => IsAllowEditingDowntimesInDocCloseShift && !IsReadOnly && SelectedDowntime != null && SelectedDowntime?.ProductionTaskID == null);
            //Downtimes = new ObservableCollection<Downtime>();

        }
        public DocCloseShiftDowntimesViewModel(int placeID, int shiftID, DateTime closeDate, Guid? docID = null, bool isConfirmed = false, GammaEntities gammaDb = null):this()
        {
            using (var gammaBase = gammaDb ?? DB.GammaDb)
            {
                PlaceID = placeID;
                ShiftID = shiftID;
                CloseDate = closeDate;
                IsConfirmed = isConfirmed;
                //DocID = docID;

                var downtimes = new ObservableCollection<Downtime>(gammaBase.DocCloseShiftDowntimes.Where(dt => dt.DocID == docID)
                     .Select(dt => new Downtime
                     {
                         ProductionTaskConvertingDowntimeID = dt.DocCloseShiftDowntimeID,
                         ProductionTaskID = dt.ProductionTaskID,
                         ProductionTaskNumber = gammaBase.ProductionTasks.FirstOrDefault(pt => pt.ProductionTaskID ==  dt.ProductionTaskID).Number ?? "",
                         DowntimeTypeID = dt.C1CDowntimeTypeID,
                         DowntimeTypeDetailID = dt.C1CDowntimeTypeDetailID,
                         //Date = dt.Date,
                         //ShiftID = dt.ShiftID,
                         DowntimeType = dt.C1CDowntimeTypes.Description,
                         DowntimeTypeDetail = dt.C1CDowntimeTypeDetails.Description,
                         Duration = dt.Duration,
                         Comment = dt.Comment,
                         DateBegin = dt.DateBegin,
                         DateEnd = dt.DateEnd
                     }));
                Downtimes = downtimes ?? new ObservableCollection<Downtime>();                
            }
        }

       /* public DocCloseShiftDowntimesViewModel(int placeID, int shiftID, DateTime closeDate, Guid docID, List<DocCloseShiftWithdrawalMaterial.Product> products, bool isConfirmed, List<Guid> productionProductCharacteristicIDs, GammaEntities gammaDb = null):this()
        {
            GammaBase = gammaDb ?? DB.GammaDb;
            PlaceID = placeID;
            ShiftID = shiftID;
            CloseDate = closeDate;

            ProductionProducts = products;
            IsConfirmed = isConfirmed;

            DocCloseShiftWithdrawalMaterials = new DocCloseShiftWithdrawalMaterial(PlaceID, ShiftID, CloseDate);
            DocCloseShiftWithdrawalMaterials.LoadWithdrawalMaterials(docID, productionProductCharacteristicIDs);
            PlaceWithdrawalMaterialTypeID = GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID).First();
        }*/
        
        //private MaterialType CurrentMaterialType;

        private int _placeID;
        private int PlaceID
        {
            get { return _placeID; }
            set
            {
                _placeID = value;
                IsAllowEditingDowntimesInDocCloseShift = GammaBase.Places.FirstOrDefault(p => p.PlaceID == value)?.IsAllowEditingDowntimesInDocCloseShift ?? false;
                /*var placeGroupID =  GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceGroupID).First();
                if (placeGroupID == 0)
                {
                    CurrentMaterialType = MaterialType.MaterialsSGB;
                    IsVisibleQuantityDismiss = true;
                }
                else if (placeGroupID == 2)
                {
                    CurrentMaterialType = MaterialType.MaterialsSGI;
                    IsVisibleQuantityDismiss = false;
                }*/

            }
        }
        private int ShiftID;
        DateTime CloseDate;
               
        private bool IsConfirmed { get; set; }
        public bool IsReadOnly => !(DB.HaveWriteAccess("DocCloseShiftDowntimes") || WorkSession.DBAdmin) || IsConfirmed;
        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();
        public Guid? VMID { get; } = Guid.NewGuid();

        private bool IsAllowEditingDowntimesInDocCloseShift { get; set; }

        public Downtime SelectedDowntime { get; set; }
        public ObservableCollection<Downtime> Downtimes { get; set; }
        /*private ObservableCollection<Downtime> _downtimes { get; set; }
        public ObservableCollection<Downtime> Downtimes
        {
            get { return _downtimes; }
            set
            {
                _downtimes = value;
            }
        }*/

        public DelegateCommand AddDowntimeCommand { get; private set; }
        public DelegateCommand DeleteDowntimeCommand { get; private set; }
        
        public DelegateCommand ShowDowntimeCommand { get; private set; }
        
        public void UpdateIsConfirmed(bool isConfirmed)
        {
            IsConfirmed = isConfirmed;
        }

        private void DeleteDowntime()
        {
            if (SelectedDowntime == null) return;
            //var removeItems = DocCloseShiftDowntimes.Where(d => d.NomenclatureID == SelectedDocCloseShiftMaterial.NomenclatureID && (d.CharacteristicID == SelectedDocCloseShiftMaterial.CharacteristicID || (d.CharacteristicID == null && SelectedDocCloseShiftMaterial.CharacteristicID == null))).ToArray();
            //foreach (var item in removeItems)
            //    DocCloseShiftDowntimes.Remove(item);
            Downtimes.Remove(SelectedDowntime);
            
        }

        private void DebugFunc()
        {
            //Debug.Print("Кол-во задано");
        }

        private void AddDowntime()
        {
            var model = new AddDowntimeDialogModel();// "Укажите параметры простоя", "Простои", 1, 1000);
            var okCommand = new UICommand()
            {
                Caption = "OK",
                IsCancel = false,
                IsDefault = true,
                Command = new DelegateCommand<CancelEventArgs>(
            x => DebugFunc(),
            x => model.IsValid && (model.DateEnd - model.DateBegin).TotalMinutes > 0 && (model.DateEnd - model.DateBegin).TotalMinutes <= 14*60),
            };
            var cancelCommand = new UICommand()
            {
                Id = MessageBoxResult.Cancel,
                Caption = "Отмена",
                IsCancel = true,
                IsDefault = false,
            };
            var dialogService = GetService<IDialogService>("AddDowntimeDialog");
            var result = dialogService.ShowDialog(
                dialogCommands: new List<UICommand>() { okCommand, cancelCommand },
                title: "Добавление простоя",
                viewModel: model);
            if (result == okCommand)
            //var dialog = new AddDowntimeDialog();
            //dialog.ShowDialog();
            //if (dialog.DialogResult == true)
            {
                string addResult = "";
                if (DB.HaveWriteAccess("DocCloseShiftDowntimes"))
                {
                    Downtimes.Add(new Downtime
                    {
                        ProductionTaskConvertingDowntimeID = Guid.NewGuid(),
                        //ProductionTaskID = null,
                        DowntimeTypeID = model.TypeID,// dialog.TypeID,
                        DowntimeTypeDetailID = model.TypeDetailID,// dialog.TypeDetailID,
                        //Date = dt.Date,
                        //ShiftID = dt.ShiftID,
                        DowntimeType = GammaBase.C1CDowntimeTypes.FirstOrDefault(t => t.C1CDowntimeTypeID == model.TypeID).Description,
                        DowntimeTypeDetail = model.TypeDetailID == null ? "" : GammaBase.C1CDowntimeTypeDetails.FirstOrDefault(t => t.C1CDowntimeTypeDetailID == model.TypeDetailID).Description,
                        Duration = (int)(model.DateEnd - model.DateBegin).TotalMinutes,
                        Comment = model.Comment,
                        DateBegin = model.DateBegin,
                        DateEnd = model.DateEnd
                    });
                }
                else
                {
                    MessageBox.Show("Недостаточно прав для добавления!");
                }

                if (addResult != "")
                {
                    MessageBox.Show(addResult, "Добавить не удалось", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
                //else RefreshDowntime();
            }
        }
        public void FillGrid()
        {
            UIServices.SetBusyState();
            //при заполнении рапорта очищаем только паллеты и документы
            //ClearGrid();
            Clear();

            using (var gammaBase = DB.GammaDb)
            {
                //if (IsEnabledDowntimes)
                {
                    var downtimes = new ObservableCollection<Downtime>(gammaBase.FillDocCloseShiftDowntimes(PlaceID, ShiftID, CloseDate)
                        .Select(dt => new Downtime
                        {
                            ProductionTaskConvertingDowntimeID = Guid.NewGuid(),
                            ProductionTaskID = dt.ProductionTaskID,
                            ProductionTaskNumber = gammaBase.ProductionTasks.FirstOrDefault(pt => pt.ProductionTaskID == dt.ProductionTaskID).Number ?? "",
                            DowntimeTypeID = dt.C1CDowntimeTypeID,
                            DowntimeTypeDetailID = dt.C1CDowntimeTypeDetailID,
                            //Date = dt.Date,
                            //ShiftID = dt.ShiftID,
                            DowntimeType = dt.DowntimeType,
                            DowntimeTypeDetail = dt.DowntimeTypeDetail,
                            Duration = dt.Duration ?? 0,
                            Comment = dt.Comment,
                            DateBegin = dt.DateBegin,
                            DateEnd = dt.DateEnd,
                            EquipmentNodeID = dt.C1CEquipmentNodeID,
                            EquipmentNodeDetailID = dt.C1CEquipmentNodeDetailID,
                            EquipmentNode = dt.EquipmentNode,
                            EquipmentNodeDetail = dt.EquipmentNodeDetail
                        }));
                    foreach (var downtime in downtimes)
                    {
                        Downtimes.Add(downtime);
                    }
                }
            }
        }

        public void Clear()
        {
            //Downtimes?.Clear();
            
            if (Downtimes != null)
                foreach (var downtime in Downtimes.Where(dt => dt.ProductionTaskID != null).ToArray())
                {
                    Downtimes.Remove(downtime);
                }
        }

        public override bool SaveToModel(Guid docId)
        {
            var ir = IsReadOnly;
            if (IsReadOnly) return true;
            using (var gammaBase = DB.GammaDb)
            {
                var docCloseShift = gammaBase.Docs
               .First(d => d.DocID == docId);
                if (docCloseShift.DocCloseShiftDowntimes == null)
                    docCloseShift.DocCloseShiftDowntimes = new List<DocCloseShiftDowntimes>();

                //gammaBase.DocCloseShiftMaterials.RemoveRange(docCloseShift.DocCloseShiftMaterials);
                var downtimesInDB = gammaBase.DocCloseShiftDowntimes.Where(d => d.DocID == docId);
                foreach (var downtimeRemove in downtimesInDB)
                {
                    if (!Downtimes.Any(d => d.ProductionTaskConvertingDowntimeID == downtimeRemove.DocCloseShiftDowntimeID))
                    {
                        gammaBase.DocCloseShiftDowntimes.Remove(downtimeRemove);
                    }
                };
                foreach (var downtime in Downtimes)
                    {
                    var downtimeInDB = downtimesInDB.Where(d => d.DocCloseShiftDowntimeID == downtime.ProductionTaskConvertingDowntimeID).FirstOrDefault();

                    if (downtimeInDB == null)
                    {
                        downtimeInDB = new DocCloseShiftDowntimes();
                        docCloseShift.DocCloseShiftDowntimes.Add(downtimeInDB);
                    }
                    downtimeInDB.DocID = docId;
                    downtimeInDB.DocCloseShiftDowntimeID = (Guid)downtime.ProductionTaskConvertingDowntimeID;
                    downtimeInDB.ProductionTaskID = downtime.ProductionTaskID;
                    downtimeInDB.C1CDowntimeTypeID = downtime.DowntimeTypeID;
                    downtimeInDB.C1CDowntimeTypeDetailID = downtime.DowntimeTypeDetailID;
                    //Date = dt.Date,
                    //ShiftID = dt.ShiftID,
                    downtimeInDB.Comment = downtime.Comment;
                    downtimeInDB.DateBegin = downtime.DateBegin;
                    downtimeInDB.DateEnd = downtime.DateEnd;
                    downtimeInDB.Duration = downtime.Duration;
                    downtimeInDB.C1CEquipmentNodeID = downtime.EquipmentNodeID;
                    downtimeInDB.C1CEquipmentNodeDetailID = downtime.EquipmentNodeDetailID;
                }
                gammaBase.SaveChanges();
            }
            return true;
        }

    }
}