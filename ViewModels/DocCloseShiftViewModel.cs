using DevExpress.Mvvm;
using System;
using System.Linq;
using Gamma.Interfaces;
using Gamma.Models;
using System.Collections.ObjectModel;
using Gamma.Common;

// ReSharper disable MemberCanBePrivate.Global

namespace Gamma.ViewModels
{
    class DocCloseShiftViewModel : SaveImplementedViewModel
    {
        public DocCloseShiftViewModel(OpenDocCloseShiftMessage msg, GammaEntities gammaBase = null) : base(gammaBase)
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
                    CurrentViewModelGrid = new DocCloseShiftPMGridViewModel(msg);
                    CurrentViewModelRemainder = msg.DocID == null ? new DocCloseShiftPMRemainderViewModel() : new DocCloseShiftPMRemainderViewModel((Guid)msg.DocID);
                    break;
                case (short)PlaceGroup.Wr:
                    CurrentViewModelGrid = new DocCloseShiftWrGridViewModel(msg);
                    break;
                case (short)PlaceGroup.Rw:
                    CurrentViewModelRemainder = msg.DocID == null ? new DocCloseShiftUnwinderRemainderViewModel(PlaceID) : new DocCloseShiftUnwinderRemainderViewModel((Guid)msg.DocID);
                    break;
                case (short)PlaceGroup.Convertings:
                    CurrentViewModelRemainder = msg.DocID == null ? new DocCloseShiftUnwinderRemainderViewModel(PlaceID) : new DocCloseShiftUnwinderRemainderViewModel((Guid)msg.DocID);
                    CurrentViewModelGrid = msg.DocID == null
                        ? new DocCloseShiftConvertingGridViewModel()
                        : new DocCloseShiftConvertingGridViewModel((Guid) msg.DocID);
                    break;
            }
            var grid = CurrentViewModelGrid as IFillClearGrid;
            if (grid != null)
            {
                FillGridCommand = new DelegateCommand(grid.FillGrid, () => !IsConfirmed);
                ClearGridCommand = new DelegateCommand(grid.ClearGrid, () => !IsConfirmed);
            }
            UploadTo1CCommand = new DelegateCommand(UploadTo1C, () => Doc != null && CurrentViewModelGrid != null && placeGroupID == (int)PlaceGroup.PM);
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
        public SaveImplementedViewModel CurrentViewModelRemainder { get; set; }
        public DelegateCommand FillGridCommand { get; set; }
        public DelegateCommand ClearGridCommand { get; set; }

        public override bool SaveToModel(GammaEntities gammaBase = null)
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
                    PrintName = WorkSession.PrintName
                };
                GammaBase.Docs.Add(Doc);
            }
            Doc.IsConfirmed = IsConfirmed;
            GammaBase.SaveChanges();
            IsNewDoc = false;
            CurrentViewModelRemainder?.SaveToModel(Doc.DocID);
            CurrentViewModelGrid?.SaveToModel(Doc.DocID);
            var currenGridViewModel = CurrentViewModelGrid as IFillClearGrid;
            if (currenGridViewModel != null && currenGridViewModel.IsChanged)
                DB.UploadDocCloseShiftTo1C(Doc.DocID, GammaBase);
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
