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
            }
            var placeGroupID = GammaBase.Places.Where(p => p.PlaceID == PlaceID).Select(p => p.PlaceGroupID).FirstOrDefault();
            switch (placeGroupID)
            {
                case (short)PlaceGroup.PM:
                    //CurrentViewModelGrid = new DocCloseShiftPMGridViewModel(msg);
                    CurrentViewModelGrid = msg.DocID == null
                        ? new DocCloseShiftPMGridViewModel()
                        : new DocCloseShiftPMGridViewModel((Guid)msg.DocID);
                    CurrentViewModelRemainder = msg.DocID == null ? new DocCloseShiftRemainderViewModel() : new DocCloseShiftRemainderViewModel((Guid)msg.DocID);
                    break;
                case (short)PlaceGroup.Wr:
                    CurrentViewModelGrid = new DocCloseShiftWrGridViewModel(msg);
                    break;
                case (short)PlaceGroup.Rw:
                    CurrentViewModelGrid = new DocCloseShiftRwGridViewModel(msg);
                    CurrentViewModelUnwinderRemainder = msg.DocID == null ? new DocCloseShiftUnwinderRemainderViewModel(PlaceID) : new DocCloseShiftUnwinderRemainderViewModel((Guid)msg.DocID);
                    break;
                case (short)PlaceGroup.Convertings:
                    CurrentViewModelUnwinderRemainder = msg.DocID == null ? new DocCloseShiftUnwinderRemainderViewModel(PlaceID) : new DocCloseShiftUnwinderRemainderViewModel((Guid)msg.DocID);
                    CurrentViewModelRemainder = msg.DocID == null ? new DocCloseShiftRemainderViewModel() : new DocCloseShiftRemainderViewModel((Guid)msg.DocID);
                    CurrentViewModelGrid = msg.DocID == null
                        ? new DocCloseShiftConvertingGridViewModel()
                        : new DocCloseShiftConvertingGridViewModel((Guid) msg.DocID);
                    break;
                case (short)PlaceGroup.Baler:
                    CurrentViewModelGrid = new DocCloseShiftBalerViewModel(msg);
                    break;
                case (short)PlaceGroup.Warehouses:
                    CurrentViewModelGrid = new DocCloseShiftWarehouseGridViewModel(msg);
                    break;
            }
            var grid = CurrentViewModelGrid as IFillClearGrid;
            if (grid != null)
            {
                FillGridCommand = new DelegateCommand(grid.FillGrid, () => !IsConfirmed);
                ClearGridCommand = new DelegateCommand(grid.ClearGrid, () => !IsConfirmed);
            }
            UploadTo1CCommand = new DelegateCommand(UploadTo1C, () => Doc != null && CurrentViewModelGrid != null && !grid.IsChanged &&
                (placeGroupID == (int)PlaceGroup.PM || placeGroupID == (int)PlaceGroup.Rw || placeGroupID == (int)PlaceGroup.Convertings));
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
        }


        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != (CurrentViewModelGrid as IBarImplemented)?.VMID) return;
            if (!IsValid) return;
            if (CanSaveExecute())
                SaveToModel();
            else if (IsNewDoc) return;
            ReportManager.PrintReport(msg.ReportID, Doc.DocID);
        }
        private Docs Doc { get; set; }
        private bool IsNewDoc { get; set; }
        private int PlaceID { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public bool IsConfirmed { get; set; }
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
        public DelegateCommand ClearGridCommand { get; set; }

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
            var utilizationProductsBeforeSave = GammaBase.DocCloseShiftUtilizationProducts.Where(d => d.DocID == Doc.DocID && d.Docs.IsConfirmed).ToList();
            Doc.IsConfirmed = IsConfirmed;
            GammaBase.SaveChanges();
            IsNewDoc = false;
            CurrentViewModelRemainder?.SaveToModel(Doc.DocID);
            CurrentViewModelGrid?.SaveToModel(Doc.DocID);
            CurrentViewModelUnwinderRemainder?.SaveToModel(Doc.DocID);
            
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
                            var res = !utilizationProductsBeforeSave?.Any(d => d.ProductID == productAfter.ProductID) ?? true;
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
            }

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
