// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
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

// ReSharper disable MemberCanBePrivate.Global

namespace Gamma.ViewModels
{
    class DocCloseShiftViewModel : SaveImplementedViewModel
    {
        public DocCloseShiftViewModel(OpenDocCloseShiftMessage msg)
        {
            //gammaBase = gammaBase ?? DB.GammaDb;
            if (msg.DocID == null)
            {
                Date = (DateTime)msg.CloseDate;
                IsConfirmed = false;
                PlaceID = (int)msg.PlaceID;
                IsNewDoc = true;
                Title = "Закрытие смены";
                ShiftID = msg.ShiftID;
            }
            else
            {
                Doc = (from d in GammaBase.Docs.Include("Places") where d.DocID == msg.DocID select d).FirstOrDefault();
//                GammaBase.Entry(Doc).Reload();
                Date = Doc.Date;
                Number = Doc.Number;
                IsConfirmed = Doc.IsConfirmed;
                PlaceID = (int)Doc.PlaceID;
                IsNewDoc = false;
                Title = "Закрытие смены №" + Doc.Number;
                ShiftID = Doc.ShiftID;
            }
            IsVisibilityUnwinderRemainder = false;
            //IsVisibilityRemainder = false;

            var place = GammaBase.Places.Where(p => p.PlaceID == PlaceID).FirstOrDefault();
            switch (place?.PlaceGroupID)
            {
                case (short)PlaceGroup.PM:
                    //CurrentViewModelGrid = new DocCloseShiftPMGridViewModel(msg);
                    CurrentViewModelGrid = msg.DocID == null
                        ? new DocCloseShiftPMGridViewModel(Date)
                        : new DocCloseShiftPMGridViewModel((Guid)msg.DocID);
                    CurrentViewModelRemainder = msg.DocID == null ? new DocCloseShiftRemainderViewModel() : new DocCloseShiftRemainderViewModel((Guid)msg.DocID);
                    //IsVisibilityRemainder = true;
                    break;
                case (short)PlaceGroup.Wr:
                    CurrentViewModelGrid = new DocCloseShiftWrGridViewModel(msg);
                    break;
                case (short)PlaceGroup.Rw:
                    CurrentViewModelUnwinderRemainder = msg.DocID == null ? new DocCloseShiftUnwinderRemainderViewModel(PlaceID) : new DocCloseShiftUnwinderRemainderViewModel((Guid)msg.DocID);
                    IsVisibilityUnwinderRemainder = true;
                    var rwUnwinderRemainder = CurrentViewModelUnwinderRemainder as DocCloseShiftUnwinderRemainderViewModel;
                    CurrentViewModelGrid = new DocCloseShiftRwGridViewModel(msg);
                    break;
                case (short)PlaceGroup.Convertings:
                    CurrentViewModelUnwinderRemainder = msg.DocID == null ? new DocCloseShiftUnwinderRemainderViewModel(PlaceID) : new DocCloseShiftUnwinderRemainderViewModel((Guid)msg.DocID);
                    IsVisibilityUnwinderRemainder = true;
                    CurrentViewModelRemainder = msg.DocID == null ? new DocCloseShiftRemainderViewModel() : new DocCloseShiftRemainderViewModel((Guid)msg.DocID);
                    //IsVisibilityRemainder = true;
                    var convertingrUnwinderRemainder = CurrentViewModelUnwinderRemainder as DocCloseShiftUnwinderRemainderViewModel;
                    CurrentViewModelGrid = msg.DocID == null
                        ? new DocCloseShiftConvertingGridViewModel(Date)
                        : new DocCloseShiftConvertingGridViewModel((Guid) msg.DocID, convertingrUnwinderRemainder);
                    break;
                case (short)PlaceGroup.Baler:
                    CurrentViewModelGrid = new DocCloseShiftBalerViewModel(msg);
                    break;
                case (short)PlaceGroup.Warehouses:
                    CurrentViewModelGrid = new DocCloseShiftWarehouseGridViewModel(msg);
                    break;
            }
            IsVisibilityRemainder = place?.IsEnabledRemainderInDocCloseShift ?? false;
            var unwinderRemainder = CurrentViewModelUnwinderRemainder as IFillClearGrid;
            if (unwinderRemainder != null)
            {
                FillUnwinderRemainderCommand = new DelegateCommand(unwinderRemainder.FillGrid, () => !IsConfirmed && CanEditable());
                ClearUnwinderRemainderCommand = new DelegateCommand(unwinderRemainder.ClearGrid, () => !IsConfirmed && CanEditable());
            }
            var grid = CurrentViewModelGrid as IFillClearGrid;
            if (grid != null)
            {
                FillGridCommand = new DelegateCommand(grid.FillGrid, () => !IsConfirmed && CanEditable());
                ClearGridCommand = new DelegateCommand(grid.ClearGrid, () => !IsConfirmed && CanEditable());
                FillGridWithNoEndCommand = new DelegateCommand(grid.FillGridWithNoFillEnd, () => !IsConfirmed && FillGridWithNoEndCanEnable());
            }
            UploadTo1CCommand = new DelegateCommand(UploadTo1C, () => Doc != null && CurrentViewModelGrid != null && !grid.IsChanged &&
                (place?.PlaceGroupID == (int)PlaceGroup.PM || place?.PlaceGroupID == (int)PlaceGroup.Rw || place?.PlaceGroupID == (int)PlaceGroup.Convertings));
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
        }


        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != (CurrentViewModelGrid as IBarImplemented)?.VMID) return;
            //if (!IsValid) return;
            if (CanSaveExecute())
                SaveToModel();
            else if (IsNewDoc) return;
            ReportManager.PrintReport(msg.ReportID, Doc.DocID);
        }
        private Docs Doc { get; set; }
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
            }
        }

        private bool _isConfirmed { get; set; }
        public bool IsConfirmed
        {
            get { return _isConfirmed; }
            set
            {
                _isConfirmed = value;
                var grid = CurrentViewModelGrid as DocCloseShiftConvertingGridViewModel;
                if (grid != null)
                    grid?.UpdateIsConfirmed(value);
            }
        }

        public byte? ShiftID { get; set; }
        public bool IsVisibilityUnwinderRemainder { get; set; }
        public bool IsVisibilityRemainder { get; set; }
        public override bool IsValid
        {
            get
            {
                return base.IsValid && (CanEditable() || FillGridWithNoEndCanEnable());
            }
        }

        public bool IsDateReadOnly
        {
            get
            { return !(!IsNewDoc && !IsConfirmed && DB.HaveWriteAccess("DocCloseShiftDocs") && WorkSession.ShiftID == 0); }
        }


        public bool CanEditable ()
        {
#if DEBUG
            return DB.HaveWriteAccess("DocCloseShiftDocs");
#else
            return DB.HaveWriteAccess("DocCloseShiftDocs") && (WorkSession.ShiftID == 0 || (WorkSession.ShiftID == ShiftID && WorkSession.PlaceID == PlaceID)) && (DB.CurrentDateTime < ShiftEndTime.AddHours(1));
#endif
        }

        public bool FillGridWithNoEndCanEnable()
        {
#if DEBUG
            return DB.HaveWriteAccess("DocCloseShiftDocs");
#else
            return DB.HaveWriteAccess("DocCloseShiftDocs") && WorkSession.ShiftID == 0;
#endif
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
                Bars = (_currentViewModelGrid as IBarImplemented).Bars;
            }
        }

        public SaveImplementedViewModel CurrentViewModelUnwinderRemainder { get; set; }
        public SaveImplementedViewModel CurrentViewModelRemainder { get; set; }
        public DelegateCommand FillGridCommand { get; set; }
        public DelegateCommand FillGridWithNoEndCommand { get; set; }
        public DelegateCommand ClearGridCommand { get; set; }
        public DelegateCommand FillUnwinderRemainderCommand { get; set; }
        public DelegateCommand ClearUnwinderRemainderCommand { get; set; }

        public override bool SaveToModel()
        {
            if (!CanSaveExecute()) return false;
            if (IsNewDoc)
            {
                Doc = new Docs()
                {
                    DocID = SqlGuidUtil.NewSequentialid(),
                    DocTypeID = (byte)DocTypes.DocCloseShift,
                    Date = Date,
                    UserID = WorkSession.UserID,
                    PlaceID = WorkSession.PlaceID,
                    ShiftID = WorkSession.ShiftID,
                    IsConfirmed = IsConfirmed,
                    PrintName = WorkSession.PrintName,
                    PersonGuid = WorkSession.PersonID.ToString() == "00000000-0000-0000-0000-000000000000" ? null : WorkSession.PersonID
                };
                GammaBase.Docs.Add(Doc);
            }
            //var utilizationProductsBeforeSave = GammaBase.DocCloseShiftUtilizationProducts.Where(d => d.DocID == Doc.DocID && d.Docs.IsConfirmed).ToList();
            var isConfirmedPrev = Doc.IsConfirmed;
            Doc.IsConfirmed = IsConfirmed;

            if (Doc.Date != Date)
                Doc.Date = Date;

            GammaBase.SaveChanges();
            if (IsNewDoc || !isConfirmedPrev || !IsConfirmed)
            {
                CurrentViewModelRemainder?.SaveToModel(Doc.DocID);
                CurrentViewModelUnwinderRemainder?.SaveToModel(Doc.DocID);
                CurrentViewModelGrid?.SaveToModel(Doc.DocID);
            }
            IsNewDoc = false;
            var now = (DateTime)GammaBase.LocalSettings.Select(s => SqlFunctions.GetDate()).FirstOrDefault();
            if (now != null && (now.Hour == 7 || now.Hour == 8 || now.Hour == 19 || now.Hour == 20 ))
            {
                var ur = CurrentViewModelUnwinderRemainder as DocCloseShiftUnwinderRemainderViewModel;
                if (GammaBase.SourceSpools.Any(ss => ss.PlaceID == PlaceID && (ss.Unwinder1Spool != null || ss.Unwinder2Spool != null || ss.Unwinder3Spool != null || ss.Unwinder4Spool != null)) && (ur == null || ur.SpoolRemainders == null || ur.SpoolRemainders?.Count() == 0))
                {
                    MessageBox.Show("Внимание! Вы не забыли обновить остатки на раскатах (кнопка Обновить во вкладке Остатки на раскатах)?",
                        "Остатки на раскатах", MessageBoxButton.OK, MessageBoxImage.Information);

                }
            }
            /*
            // удаляем с остатков или добавляем на остатки по утилизации
            var utilizationProductsAfterSave = GammaBase.DocCloseShiftUtilizationProducts.Where(d => d.DocID == Doc.DocID && d.Docs.IsConfirmed).ToList();
            if ((utilizationProductsBeforeSave?.Count ?? 0) > 0 || (utilizationProductsAfterSave?.Count ?? 0) > 0)
            {
                if ((utilizationProductsBeforeSave?.Count ?? 0) > 0)
                {
                    foreach (var productBefore in utilizationProductsBeforeSave)
                    {
                        //if (utilizationProductsAfterSave != null)
                        {
                            if (!utilizationProductsAfterSave?.Any(d => d.ProductID == productBefore.ProductID) ?? true)
                            {
                                if (!GammaBase.Rests.Any(r => r.ProductID == productBefore.ProductID))
                                    GammaBase.Rests.Add(new Rests()
                                    {
                                        ProductID = productBefore.ProductID,
                                        Quantity = 1
                                    });
                            }
                        }
                    }
                }
                if ((utilizationProductsAfterSave?.Count ?? 0) > 0)
                {
                    foreach (var productAfter in utilizationProductsAfterSave)
                    {
                        {
                            //var res = !utilizationProductsBeforeSave?.Any(d => d.ProductID == productAfter.ProductID) ?? true;
                            if (!utilizationProductsBeforeSave?.Any(d => d.ProductID == productAfter.ProductID) ?? true)
                            {
                                if (GammaBase.Rests.Any(r => r.ProductID == productAfter.ProductID))
                                {
                                    var restsUtilization = GammaBase.Rests.Where(r => r.ProductID == productAfter.ProductID);
                                    GammaBase.Rests.RemoveRange(restsUtilization);
                                }
                            }
                        }
                    }
                }
                GammaBase.SaveChanges();
            }*/

#if !DEBUG
            var currenGridViewModel = CurrentViewModelGrid as IFillClearGrid;
            if (currenGridViewModel != null && currenGridViewModel.IsChanged)
                DB.UploadDocCloseShiftTo1C(Doc.DocID, GammaBase);
#endif
            return true;
        }

        public DelegateCommand UploadTo1CCommand { get; private set; }

        private void UploadTo1C()
        {
            UIServices.SetBusyState();
            if (CurrentViewModelGrid != null && Doc != null)
            {
                DB.UploadDocCloseShiftTo1C(Doc.DocID, GammaBase);
            }
        }

        private ObservableCollection<BarViewModel> _bars;
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
        }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Title { get; set; }
        public override bool CanSaveExecute()
        {
            return base.CanSaveExecute() && DB.HaveWriteAccess("DocCloseShiftDocs");
        }

        protected override void SaveToModelAndClose()
        {
            base.SaveToModelAndClose();
            Messenger.Default.Send(new CloseMessage());
        }
    }
}
