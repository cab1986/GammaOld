using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Interfaces;
using Gamma.Models;

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
                default:
                    break;
            }
            FillGridCommand = new RelayCommand((CurrentViewModelGrid as IFillClearGrid).FillGrid);
            ClearGridCommand = new RelayCommand((CurrentViewModelGrid as IFillClearGrid).ClearGrid);
        }
        private Docs Doc { get; set; }
        private bool IsNewDoc { get; set; }
        private int PlaceID { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public bool IsConfirmed { get; set; }
        public SaveImplementedViewModel CurrentViewModelGrid { get; set; }
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
                };
                DB.GammaBase.Docs.Add(Doc);
            }
            DB.GammaBase.SaveChanges();
            if (CurrentViewModelGrid != null)
                CurrentViewModelGrid.SaveToModel(Doc.DocID);
            if (CurrentViewModelRemainder != null)
                CurrentViewModelRemainder.SaveToModel(Doc.DocID);
        }
        public string Title { get; set; }
    }
}
