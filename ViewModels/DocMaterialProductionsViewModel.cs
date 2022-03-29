using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Interfaces;
using Gamma.Models;
using System.Windows;
using System.Data.Entity.SqlServer;

namespace Gamma.ViewModels
{
    public class DocMaterialProductionsViewModel : SaveImplementedViewModel, IItemManager
    {
        public DocMaterialProductionsViewModel()
        {
            Intervals = new List<string> { "За мою смену", "Последние 500", "Поиск" };
            RefreshCommand = new DelegateCommand(Find);
            NewItemCommand = new DelegateCommand(() => OpenDocMaterialProduction());
            EditItemCommand = new DelegateCommand(() => OpenDocMaterialProduction(SelectedDocMaterialProduction.DocID), SelectedDocMaterialProduction != null);
            DeleteItemCommand = new DelegateCommand(DeleteItem);
            using (var gammaBase = DB.GammaDb)
            {
                Places = gammaBase.Places.Where(
                    p => WorkSession.BranchIds.Contains(p.BranchID) && ((p.IsMaterialProductionPlace ?? false) || (p.PlaceGroupID == (int)PlaceGroup.PM)))
                    .Select(p => new Place()
                    {
                        PlaceID = p.PlaceID,
                        PlaceGuid = p.PlaceGuid,
                        PlaceName = p.Name
                    }).ToList();
            }
            Places.Insert(0, new Place() { PlaceName = "Все" });
            PlaceId = 0;
            IntervalId = 1;
            Find();
        }

        private List<Doc> _DocMaterialProductionsList;

        public List<Doc> DocMaterialProductionsList
        {
            get { return _DocMaterialProductionsList; }
            set
            {
                _DocMaterialProductionsList = value;
                RaisePropertyChanged("DocMaterialProductionsList");
            }
        }

        public void Find()
        {
            WorkSession.CheckExistNewVersionOfProgram();
            UIServices.SetBusyState();
            SelectedDocMaterialProduction = null;
            using (var gammaBase = DB.GammaDb)
            {
                var placeIDs = Places?.Select(p => p.PlaceID).ToList();
                switch (IntervalId)
                {
                    case 0:
                        DocMaterialProductionsList = gammaBase.Docs
                        .Where( d =>  d.DocTypeID == (byte)DocTypes.DocMaterialProduction &&
                        //(d.PlaceID == WorkSession.PlaceID) &&
                        (d.ShiftID == WorkSession.ShiftID) &&
                        (d.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime(DB.CurrentDateTime))) &&
                        (d.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime(DB.CurrentDateTime))))
                        .OrderByDescending( d => d.Date)
                        .Take(120)
                        .Select( d => new Doc
                        {
                            DocID = d.DocID,
                            Number = d.Number,
                            Date = d.Date,
                            ShiftID = d.ShiftID ?? 0,
                            Place = d.Places.Name,
                            User = d.Users.Name,
                            Person = d.Persons.Name,
                            IsConfirmed = d.IsConfirmed,
                            Comment = d.Comment
                        }).ToList();
                        break;
                    case 1:
                        DocMaterialProductionsList = gammaBase.Docs
                            .Where(d => d.DocTypeID == (byte)DocTypes.DocMaterialProduction
                            // && (PlaceId == 0 ? placeIDs.Contains(d.PlaceID ?? 0) : PlaceId == d.PlaceID)
                            )
                            .OrderByDescending(d => d.Date)
                            .Take(500)
                            .Select(d => new Doc
                            {
                                DocID = d.DocID,
                                Number = d.Number,
                                Date = d.Date,
                                ShiftID = d.ShiftID ?? 0,
                                Place = d.Places.Name,
                                User = d.Users.Name,
                                Person = d.Persons.Name,
                                IsConfirmed = d.IsConfirmed,
                                Comment = d.Comment
                            }).ToList();
                        break;
                    case 2:
                        DocMaterialProductionsList = gammaBase.Docs
                        .Where(d => d.DocTypeID == (byte)DocTypes.DocMaterialProduction &&
                       (string.IsNullOrEmpty(Number) || Number == d.Number) &&
                       (PlaceId == 0 ? placeIDs.Contains(d.PlaceID ?? 0) : PlaceId == d.PlaceID) &&
                       (DateBegin == null || d.Date >= DateBegin) &&
                       (DateEnd == null || d.Date <= DateEnd))
                        .OrderByDescending(d => d.Date)
                        .Take(500)
                        .Select(d => new Doc
                        {
                            DocID = d.DocID,
                            Number = d.Number,
                            Date = d.Date,
                            ShiftID = d.ShiftID ?? 0,
                            Place = d.Places.Name,
                            User = d.Users.Name,
                            Person = d.Persons.Name,
                            IsConfirmed = d.IsConfirmed,
                            Comment = d.Comment
                        }).ToList();
                        break;
                }
            }
        }

        private int _intervalId;

        public int IntervalId
        {
            get { return _intervalId; }
            set
            {
                if (_intervalId == value) return;
                _intervalId = value;
                if (_intervalId < 2) Find();
            }
        }

        public Doc SelectedDocMaterialProduction { get; set; }

        public List<string> Intervals { get; private set; }

        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public string Number { get; set; }
        public int? PlaceId { get; set; }
        public List<Place> Places { get; set; }

        public DelegateCommand DeleteItemCommand { get; }
        public DelegateCommand<object> EditItemCommand { get; private set; }
        public DelegateCommand NewItemCommand { get; }
        public DelegateCommand RefreshCommand { get; private set; }

        private void OpenDocMaterialProduction(Guid? docID = null)
        {
            WorkSession.CheckExistNewVersionOfProgram();
            UIServices.SetBusyState();
            Messenger.Default.Register<RefreshMessage>(this, Find);
            if (docID == null)
                MessageManager.OpenDocMaterialProduction(SqlGuidUtil.NewSequentialid());
            else
            {
                MessageManager.OpenDocMaterialProduction((Guid)docID);
            }
        }

        private void Find(RefreshMessage msg)
        {
            Find();
            Messenger.Default.Unregister<RefreshMessage>(this, Find);
        }

        private void DeleteItem()
        {
            WorkSession.CheckExistNewVersionOfProgram();
            if (SelectedDocMaterialProduction == null) return;
            var deleteItem = GammaBase.Docs.FirstOrDefault(d => d.DocID == SelectedDocMaterialProduction.DocID);
            if (deleteItem != null)
            {
                var delResult = GammaBase.Docs.Remove(deleteItem);
                try
                {
                    GammaBase.SaveChanges();
                }
                catch
                {
                    MessageBox.Show("Ошибка при удалении документа "+ delResult.Number + delResult.Date.ToString() + ". откройте документ и нажмите Очистить, затем попробуйте повторно.", "Не удалось удалить", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (delResult != null)
                {
                    Find();
                }
            }
            else
            {
                MessageBox.Show("Удаление","Не выбрана запись. Удалить не удалось.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
    }
}
