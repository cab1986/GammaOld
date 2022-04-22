using System;
using System.Collections.ObjectModel;
using System.Linq;
using Gamma.Entities;
using Gamma.Interfaces;
using Gamma.Models;
using System.Data.Entity;
using System.Windows;
using DevExpress.Mvvm;
using System.Data.Entity.SqlServer;

namespace Gamma.ViewModels
{
    public class DocUnwinderRemainderViewModel : SaveImplementedViewModel, ICheckedAccess
    {
        public DocUnwinderRemainderViewModel(OpenDocUnwinderRemainderMessage msg)
        {
        //using (var gammaBase = DB.GammaDb)
        //    {
                if (msg.DocID == null)
                {
                    Date = (DateTime)msg.CloseDate;
                    IsConfirmed = false;
                    PlaceID = (int)msg.PlaceID;
                    IsNewDoc = true;
                    Title = "Остатки на раскатах - Новый документ";
                    ShiftID = msg.ShiftID;
                    PrintName = WorkSession.PrintName;
                    UserName = WorkSession.UserName;
                    UserID = WorkSession.UserID;
                    PersonID = WorkSession.PersonID.ToString() == "00000000-0000-0000-0000-000000000000" ? null : WorkSession.PersonID;
                    Place = msg.PlaceID == null ? "" : (from d in WorkSession.Places where d.PlaceID == msg.PlaceID select d.Name).FirstOrDefault();
                }
                else
                {
                    Doc = (from d in GammaBase.Docs.Include("Places") where d.DocID == msg.DocID select d).FirstOrDefault();
                    //                GammaBase.Entry(Doc).Reload();
                    if (Doc == null)
                    {
                        MessageBox.Show("Не удалось получить информацию о документе", "Ошибка загрузки документа",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    Date = Doc.Date;
                    Number = Doc.Number;
                    IsConfirmed = Doc.IsConfirmed;
                    PlaceID = (int)Doc.PlaceID;
                    IsNewDoc = false;
                    Title = "Остатки на раскатах - Документ №" + Doc.Number;
                    ShiftID = Doc.ShiftID;
                    PrintName = Doc.PrintName;
                    Place = Doc.Places?.Name;
                    UserName = Doc.Users?.Name;
                    UserID = Doc.UserID;
                    PersonID = Doc.PersonGuid;
                }
            //}
            AllowEditDoc = msg.DocID == null ? true : DB.AllowEditDoc((Guid)msg.DocID);
            if (!AllowEditDoc) NotAllowEditingMessage = "Изменение запрещено! С данным документом связаны другие документы!";
            CurrentViewModelUnwinderRemainder = msg.DocID == null ? new DocCloseShiftUnwinderRemainderViewModel(PlaceID) : new DocCloseShiftUnwinderRemainderViewModel((Guid)msg.DocID);
            var unwinderRemainder = CurrentViewModelUnwinderRemainder as IFillClearGrid;
            if (unwinderRemainder != null)
            {
                FillUnwinderRemainderCommand = new DelegateCommand(unwinderRemainder.FillGrid, () => !IsConfirmed && !IsReadOnly);
                ClearUnwinderRemainderCommand = new DelegateCommand(unwinderRemainder.ClearGrid, () => !IsConfirmed && !IsReadOnly);
            }
            var docDate = GammaBase.DocCloseShiftRemainders.Where(d => d.ProductID != null && d.DocCloseShifts.IsConfirmed && d.DocCloseShifts.PlaceID == PlaceID && d.DocCloseShifts.ShiftID == ShiftID &&
                d.DocCloseShifts.Date >= SqlFunctions.DateAdd("hh", 1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, Date))) &&
                d.DocCloseShifts.Date <= SqlFunctions.DateAdd("hh", -1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, Date))))
                .OrderByDescending(d => d.DocCloseShifts.Date).Select(d => new { d.DocCloseShifts.DocID, d.DocCloseShifts.Date }).FirstOrDefault();
            if (docDate != null)
            {
                AllowEditDoc = false;
                NotAllowEditingMessage = "Изменение запрещено! Остатки на раскатах сохранены в подтвержденном Рапорте закрытия смены от " + docDate?.Date.ToString() + "!";
            }

        }

        public bool IsReadOnly
        {
            get
            {
                return !DB.HaveWriteAccess("DocUnwinderRemainders") || (!IsValid || !AllowEditDoc) ;
            }
        }

        private Docs Doc { get; set; }
        public string Title { get; set; }
        public string Place { get; set; }
        public DateTime Date { get; set; }
        public string PrintName { get; set; }
        public string UserName { get; set; }
        public byte? ShiftID { get; set; }
        private bool _isConfirmed;
        private bool IsNewDoc { get; set; }
        private int PlaceID { get; set; }
        public string Number { get; set; }
        private Guid? UserID { get; set; }
        private Guid? PersonID { get; set; }
        public string NotAllowEditingMessage { get; private set; }

        public bool NotAllowEditDoc { get; private set; }
        private bool _allowEditDoc { get; set; } = true;
        public bool AllowEditDoc
        {
            get
            {
                return _allowEditDoc;
            }
            private set
            {
                if (_allowEditDoc == value)
                    return;
                _allowEditDoc = value;
                NotAllowEditDoc = !value;
                RaisePropertyChanged("NotAllowEditDoc");
            }
        }

        private bool IsChanged { get; set; } = false;

        public SaveImplementedViewModel CurrentViewModelUnwinderRemainder { get; set; }
        public DelegateCommand FillUnwinderRemainderCommand { get; set; }
        public DelegateCommand ClearUnwinderRemainderCommand { get; set; }

        public bool IsConfirmed
        {
            get
            {
                return _isConfirmed;
            }
            set
            {
                if (_isConfirmed)
                    if (!AllowEditDoc)
                    {
                        MessageBox.Show("Правка невозможна. Продукция уже в выработке или с ней связаны другие документы");
                        return;
                    }
                if (_isConfirmed != value)
                {
                    if (value && !IsValid)
                        _isConfirmed = false;
                    else
                    {
                        _isConfirmed = value;
                        IsChanged = true;
                        ((DocCloseShiftUnwinderRemainderViewModel)CurrentViewModelUnwinderRemainder)?.UpdateIsConfirmed(value);
                    }
                }
                RaisePropertyChanged("IsConfirmed");
            }
        }

        public override bool CanSaveExecute()
        {
            return IsValid && DB.HaveWriteAccess("DocUnwinderRemainders") && AllowEditDoc;
        }

        public override bool SaveToModel()
        {
            if (IsReadOnly) return true;
            //using (var gammaBase = DB.GammaDb)
            //{
                if (!CanSaveExecute()) return false;
                if (IsNewDoc)
                {
                    Doc = new Docs()
                    {
                        DocID = SqlGuidUtil.NewSequentialid(),
                        DocTypeID = (byte)DocTypes.DocUnwinderRemainder,
                        Date = Date,
                        UserID = UserID,
                        PlaceID = PlaceID,
                        ShiftID = ShiftID,
                        IsConfirmed = IsConfirmed,
                        PrintName = PrintName,
                        PersonGuid = PersonID
                    };
                    GammaBase.Docs.Add(Doc);
                }
            //var isConfirmedPrev = Doc.IsConfirmed;
            Doc.IsConfirmed = true;// IsConfirmed;
            GammaBase.SaveChanges();
            if (IsNewDoc || IsChanged || !IsConfirmed) // !isConfirmedPrev || !IsConfirmed)
            {
                CurrentViewModelUnwinderRemainder?.SaveToModel(Doc.DocID);
            }
            IsNewDoc = false;
            //}
            return true;
        }
    }
}
