using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Interfaces;
using Gamma.Models;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Messaging;

namespace Gamma.ViewModels
{
    class DocCloseShiftViewModel : SaveImplementedViewModel
    {
        public DocCloseShiftViewModel(OpenDocCloseShiftMessage msg)
        {
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
                Doc = (from d in DB.GammaBase.Docs.Include("Places") where d.DocID == msg.DocID select d).FirstOrDefault();
                DB.GammaBase.Entry<Docs>(Doc).Reload();
                Date = Doc.Date;
                Number = Doc.Number;
                IsConfirmed = Doc.IsConfirmed;
                PlaceID = (int)Doc.PlaceID;
                IsNewDoc = false;
                Title = "Закрытие смены №" + Doc.Number;
            }
            var placeGroupID = DB.GammaBase.Places.Where(p => p.PlaceID == PlaceID).Select(p => p.PlaceGroupID).FirstOrDefault();
            switch (placeGroupID)
            {
                case (short)PlaceGroups.PM:
                    CurrentViewModelGrid = new DocCloseShiftPMGridViewModel(msg);
                    if (msg.DocID == null)
                        CurrentViewModelRemainder = new DocCloseShiftPMRemainderViewModel();
                    else
                        CurrentViewModelRemainder = new DocCloseShiftPMRemainderViewModel((Guid)msg.DocID);
                    break;
                case (short)PlaceGroups.WR:
                    CurrentViewModelGrid = new DocCloseShiftWRGridViewModel(msg);
                    break;
                default:
                    break;
            }
            FillGridCommand = new RelayCommand((CurrentViewModelGrid as IFillClearGrid).FillGrid, () => !IsConfirmed);
            ClearGridCommand = new RelayCommand((CurrentViewModelGrid as IFillClearGrid).ClearGrid, () => !IsConfirmed);
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
        }
        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != (CurrentViewModelGrid as IBarImplemented).VMID) return;
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
        public RelayCommand FillGridCommand { get; set; }
        public RelayCommand ClearGridCommand { get; set; }
        public override void SaveToModel()
        {
            base.SaveToModel();
            if (IsNewDoc)
            {
                Doc = new Docs()
                {
                    DocID = SQLGuidUtil.NewSequentialId(),
                    DocTypeID = (byte)DocTypes.DocCloseShift,
                    Date = Date,
                    UserID = WorkSession.UserID,
                    PlaceID = WorkSession.PlaceID,
                    ShiftID = WorkSession.ShiftID,
                    IsConfirmed = IsConfirmed,
                    PrintName = WorkSession.PrintName
                };
                DB.GammaBase.Docs.Add(Doc);
            }
            Doc.IsConfirmed = IsConfirmed;
            DB.GammaBase.SaveChanges();
            IsNewDoc = false;
            if (CurrentViewModelGrid != null)
                CurrentViewModelGrid.SaveToModel(Doc.DocID);
            if (CurrentViewModelRemainder != null)
                CurrentViewModelRemainder.SaveToModel(Doc.DocID);
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
        public string Title { get; set; }
        public override bool CanSaveExecute()
        {
            return base.CanSaveExecute() && DB.HaveWriteAccess("DocCloseShiftDocs");
        }
    }
}
