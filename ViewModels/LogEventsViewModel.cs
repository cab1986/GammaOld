using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Interfaces;
using Gamma.Models;
using System.Windows;

namespace Gamma.ViewModels
{
    public class LogEventsViewModel : SaveImplementedViewModel, IItemManager
    {
        public LogEventsViewModel()
        {
            Intervals = new List<string> { "Активные", "Последние 500", "Поиск" };
            IntervalId = 0;
            RefreshCommand = new DelegateCommand(Find);
            NewItemCommand = new DelegateCommand(() => OpenLogEvent());
            EditItemCommand = new DelegateCommand(() => OpenLogEvent(SelectedLogEvent.EventID), SelectedLogEvent != null);
            DeleteItemCommand = new DelegateCommand(DeleteItem);
            using (var gammaBase = DB.GammaDb)
            {
                Places = gammaBase.Places.Where(
                    p => WorkSession.BranchIds.Contains(p.BranchID))
                    .Select(p => new Place()
                    {
                        PlaceID = p.PlaceID,
                        PlaceGuid = p.PlaceGuid,
                        PlaceName = p.Name
                    }).ToList();
                Departments = gammaBase.Departments
                    //.Where(p => p.BranchID == WorkSession.BranchID && (p.IsProductionPlace ?? false))
                    .Where(p => p.DepartmentID == WorkSession.DepartmentID || WorkSession.DepartmentID == 0 )
                    .Select(p => new Department()
                    {
                        DepartmentID = p.DepartmentID,
                        DepartmentName = p.Name
                    }).ToList();
                
            }
            DepartmentIDs = Departments.ConvertAll(c => c.DepartmentID);
            if (WorkSession.DepartmentID == 0)
                DepartmentId = null;
            else
                DepartmentId = (short)WorkSession.DepartmentID;
            Find();
        }

        private List<LogEvent> _logEventsList;

        public List<LogEvent> LogEventsList
        {
            get { return _logEventsList; }
            set
            {
                _logEventsList = value;
                RaisePropertyChanged("LogEventsList");
            }
        }

        public void Find()
        {
            UIServices.SetBusyState();
            SelectedLogEvent = null;
            using (var gammaBase = DB.GammaDb)
            {
                switch (IntervalId)
                {
                    case 0:
                        LogEventsList = gammaBase.LogEvents.Include(d => d.Places).Include(d => d.EventKinds).Include(d=>d.Devices).Include(d => d.Shifts)
                            //.Where(d => !d.IsConfirmed && d.DocTypeID == (int)DocTypes.DocInventarisation)
                            .Where
                              (d => 
                                !(d.IsSolved == true)  
                                && (PlaceId == null || d.PlaceID == PlaceId) 
                                && (DepartmentIDs.Contains((short)d.DepartmentID) || DepartmentIDs.Contains((short)d.Users.DepartmentID) || (WorkSession.PlaceID == 0 && DepartmentIDs.Contains((short)d.Places.DepartmentID)))
                                && (DepartmentId == null || (d.Users.DepartmentID == DepartmentId || d.DepartmentID == DepartmentId || (WorkSession.PlaceID == 0 && d.Places.DepartmentID == DepartmentId)))
                              )
                            .OrderByDescending(d => d.Date).Take(500)
                            .Select(d => new LogEvent
                            {
                                EventID = d.EventID,
                                Number = d.Number,
                                Date = d.Date,
                                Description = d.Description,
                                PrintName = d.Users.Name+"("+d.PrintName+")",
                                Place = d.Places.Name,
                                Device = d.Devices.Name,
                                EventKind = d.EventKinds.Name,
                                IsSolved = d.IsSolved ?? false,
                                Shift = d.Shifts.Name,
                                Department = d.Departments.Name,
                                EventState = d.EventStates.Name,
                            }).ToList();
                        break;
                    case 1:
                        LogEventsList = gammaBase.LogEvents.Include(d => d.Places).Include(d => d.EventKinds).Include(d => d.Devices).Include(d => d.Shifts)
                            //.Where(d => d.DocTypeID == (int)DocTypes.DocInventarisation)
                            .Where
                              (d =>
                                (PlaceId == null || d.PlaceID == PlaceId)
                                && (DepartmentIDs.Contains((short)d.DepartmentID) || DepartmentIDs.Contains((short)d.Users.DepartmentID) || (WorkSession.PlaceID == 0 && DepartmentIDs.Contains((short)d.Places.DepartmentID)))
                                && (DepartmentId == null || (d.Users.DepartmentID == DepartmentId || d.DepartmentID == DepartmentId || (WorkSession.PlaceID == 0 && d.Places.DepartmentID == DepartmentId)))
                              )
                            .Take(500)
                            .Select(d => new LogEvent
                            {
                                EventID = d.EventID,
                                Number = d.Number,
                                Date = d.Date,
                                Description = d.Description,
                                PrintName = d.Users.Name + "(" + d.PrintName + ")",
                                Place = d.Places.Name,
                                Device = d.Devices.Name,
                                EventKind = d.EventKinds.Name,
                                IsSolved = d.IsSolved ?? false,
                                Shift = d.Shifts.Name,
                                Department = d.Departments.Name,
                                EventState = d.EventStates.Name,
                            }).ToList();
                        break;
                    case 2:
                        LogEventsList = gammaBase.LogEvents.Include(d => d.Places).Include(d => d.EventKinds).Include(d => d.Devices).Include(d => d.Shifts)
                            .Where
                            (d => 
                                (string.IsNullOrEmpty(Number) || Number == d.Number)
                                && (DateBegin == null || d.Date >= DateBegin)
                                && (DateEnd == null || d.Date <= DateEnd)
                                && (PlaceId == null || d.PlaceID == PlaceId)
                                && (DepartmentId == null || d.DepartmentID == DepartmentId)
                                && (DepartmentIDs.Contains((short)d.DepartmentID) || DepartmentIDs.Contains((short)d.Users.DepartmentID) || (WorkSession.PlaceID == 0 && DepartmentIDs.Contains((short)d.Places.DepartmentID)))
                            )
                            .Take(500)
                            .Select(d => new LogEvent
                            {
                                EventID = d.EventID,
                                Number = d.Number,
                                Date = d.Date,
                                Description = d.Description,
                                PrintName = d.Users.Name + "(" + d.PrintName + ")",
                                Place = d.Places.Name,
                                Device = d.Devices.Name,
                                EventKind = d.EventKinds.Name,
                                IsSolved = d.IsSolved ?? false,
                                Shift = d.Shifts.Name,
                                Department = d.Departments.Name,
                                EventState = d.EventStates.Name,
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

        public LogEvent SelectedLogEvent { get; set; }

        public List<string> Intervals { get; private set; }

        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public string Number { get; set; }
        public int? PlaceId { get; set; }
        public List<Place> Places { get; set; }

        public short? DepartmentId { get; set; }
        public List<Department> Departments { get; set; }
        public List<short> DepartmentIDs { get; set; }

        public DelegateCommand DeleteItemCommand { get; }
        public DelegateCommand<object> EditItemCommand { get; private set; }
        public DelegateCommand NewItemCommand { get; }
        public DelegateCommand RefreshCommand { get; private set; }

        private void OpenLogEvent(Guid? eventID = null)
        {
            UIServices.SetBusyState();
            Messenger.Default.Register<RefreshMessage>(this, Find);
            if (eventID == null)
                MessageManager.OpenLogEvent(SqlGuidUtil.NewSequentialid(), null);
            else
            {
                MessageManager.OpenLogEvent((Guid)eventID, null);
            }
        }

        private void Find(RefreshMessage msg)
        {
            Find();
            Messenger.Default.Unregister<RefreshMessage>(this, Find);
        }

        private void DeleteItem()
        {
            if (SelectedLogEvent == null) return;
            var deleteItem = GammaBase.LogEvents.FirstOrDefault(d => d.EventID == SelectedLogEvent.EventID);
            if (deleteItem != null)
            {
                var delResult = GammaBase.LogEvents.Remove(deleteItem);
                GammaBase.SaveChanges();
                if (delResult != null)
                {
                    Find();
                    return;
                }
                MessageBox.Show(delResult.Number + delResult.Date.ToString(), "Не удалось удалить", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Удаление","Не выбрана запись. Удалить не удалось.", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
