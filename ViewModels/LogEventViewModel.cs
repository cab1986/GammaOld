// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Gamma.Interfaces;
using Gamma.Models;
using System.Data.Entity;
using DevExpress.Mvvm;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Attributes;
using Gamma.Entities;
using System.Windows.Data;
using System.Globalization;
using System.Data.Entity.SqlServer;

namespace Gamma.ViewModels
{
    public class LogEventViewModel : SaveImplementedViewModel, IBarImplemented, ICheckedAccess
    {
        public LogEventViewModel(Guid eventID, Guid? parentEventID)
        {
            EventID = eventID;
            Bars.Add(ReportManager.GetReportBar("LogEvent", VMID));
            using (var gammaBase = DB.GammaDb)
            {
                Places = gammaBase.Places
                .Where(p => WorkSession.BranchIds.Contains(p.BranchID))
                .Select(p => new Place
                {
                    PlaceGuid = p.PlaceGuid,
                    PlaceID = p.PlaceID,
                    PlaceName = p.Name
                }).ToList();
                EventKinds = gammaBase.EventKinds.Where(p => p.IsVisible ?? false)
                                .Select(p => new EventKind
                                {
                                    EventKindID = p.EventKindID,
                                    EventKindName = p.Name
                                }).ToList();
                Devices = gammaBase.Devices//.Where(p => p.IsWarehouse ?? false)
                    .Select(p => p.Name
                    ).ToList();

                Departments = gammaBase.Departments//.Where(p => ((p.IsProductionPlace ?? false) || (p.IsWarehouse ?? false)) && WorkSession.BranchIds.Contains(p.BranchID))
                .Select(p => new Department
                {
                    DepartmentID = p.DepartmentID,
                    DepartmentName = p.Name
                }).ToList();

                PrintNames = gammaBase.LogEvents//.Where(p => ((p.IsProductionPlace ?? false) || (p.IsWarehouse ?? false)) && WorkSession.BranchIds.Contains(p.BranchID))
                    .Where(p => p.Date >= SqlFunctions.DateAdd("d", -45, SqlFunctions.GetDate()))
                                .Select(p => p.PrintName).ToList();

                var doc = gammaBase.LogEvents.Include(d => d.Places).Include(d => d.EventKinds).Include(d => d.Devices)
                    .FirstOrDefault(d => d.EventID == EventID);
                if (doc != null)
                {
                    Number = doc.Number;
                    Date = doc.Date;
                    Place = doc.Places.Name;
                    PlaceID = doc.PlaceID;
                    DeviceID = doc.DeviceID;
                    DeviceName = doc.Devices.Name;
                    Department = doc.Departments?.Name;
                    if (doc.EventKindID == 0)
                        EventKindID = null;
                    else
                        EventKindID = doc.EventKindID;
                    Description = doc.Description;
                    IsSolved = doc.IsSolved;
                    SolvedLabel = (IsSolved == true ? "Закрыто " + doc.DateSolved.ToString() : "Закрыто" );
                    ParentEventID = doc.ParentEventID;
                    PrintName = doc.PrintName;
                    EventStateName = doc.EventStates.Name;
                    DepartmentID = doc.DepartmentID;
                    UserDepartmentID = doc.Users.DepartmentID;
                    LogEventList = gammaBase.GetLogEventHierarchy(eventID)//.Include(d => d.Places).Include(d => d.EventKinds).Include(d => d.Devices)
                                                                          //.Where(d => !d.IsConfirmed && d.DocTypeID == (int)DocTypes.DocInventarisation)
                                                                          //.Where(d => (d.ParentEventID == EventID || d.EventID == ParentEventID))
                                .OrderByDescending(d => d.Date).Take(500)
                                .Select(d => new LogEvent
                                {
                                    EventID = d.EventID,
                                    Number = d.Number,
                                    Date = d.Date,
                                    Description = d.Description,
                                    PrintName = d.PrintName,
                                    Place = d.PlaceName,
                                    Device = d.DeviceName,
                                    EventKind = d.EventKindName,
                                    IsSolved = d.IsSolved ?? false,
                                    ParentEventID = d.ParentEventID,
                                    Department = d.DepartmentName,
                                    EventState = d.EventStateName
                                }).ToList();

                    //DepartmentTo = gammaBase.LogEvents.Where(d => (gammaBase.LogEvents.Where(p => p.ParentEventID == EventID).Select(p => p.EventID)).Contains(d.EventID))
                    //            .Select(d => (int)d.DepartmentID).ToList();



                    IsReadOnly = !(!(bool)doc.EventStates.EventIsReadOnly);
                    IsReadOnlyPlace = (IsReadOnly || ParentEventID != null);
                    IsReadOnlyDevice = (IsReadOnly || ParentEventID != null);
                    IsReadOnlySolved = !(!IsReadOnly || (IsReadOnly && doc.EventStates.EventStateID == 5 && (IsSolved ?? true)));
                    //IsReadOnlyDepartmentTo = (IsReadOnly || ParentEventID != null);
                    if (!IsReadOnly)
                        DepartmentToNullText = "";
                    else
                        DepartmentToNullText = String.Join(", ", gammaBase.LogEvents.Where(d => (gammaBase.LogEvents.Where(p => p.ParentEventID == EventID).Select(p => p.EventID)).Contains(d.EventID))
                                .Select(d => d.Departments.Name).ToArray());
                }
                else
                {
                    Number = DB.CurrentDateTime.ToString("yyMMddHHmmssf");
                    Date = DB.CurrentDateTime;
                    ParentEventID = parentEventID;
                    PrintName = WorkSession.PrintName;
                    PlaceID = WorkSession.PlaceID;
                    UserDepartmentID = (short?)WorkSession.DepartmentID;
                    //ShiftID = WorkSession.ShiftID;
                    IsReadOnly = false; // !DB.HaveWriteAccess("LogEvent");
                    DepartmentToNullText = "";
                }
                
            }

            if (String.IsNullOrEmpty(PrintName)) PrintNameNullText = WorkSession.UserID.ToString();
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
            EditItemCommand = new DelegateCommand(() => MessageManager.OpenLogEvent((Guid)SelectedLogEvent.EventID, null), SelectedLogEvent != null);
            
        }

        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != VMID) return;
            ReportManager.PrintReport(msg.ReportID, EventID);
        }

        public DelegateCommand<object> EditItemCommand { get; private set; }

        public bool IsReadOnly { get; set; }
        public bool IsReadOnlyPlace { get; set; }
        public bool IsReadOnlyDevice { get; set; }
        public bool IsReadOnlySolved { get; set; }

        public override bool CanSaveExecute()
        {
            //return IsValid && DB.HaveWriteAccess("DocBroke");
            return IsValid && (UserDepartmentID == WorkSession.DepartmentID || DepartmentID == WorkSession.DepartmentID); //&& DB.HaveWriteAccess("DocBroke")
        }

        private Guid EventID { get; set; }

        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();
        public Guid? VMID { get; } = Guid.NewGuid();

        [UIAuth(UIAuthLevel.ReadOnly)]
        public string Number { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public DateTime Date { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public string Place { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public string PrintName { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public string Department { get; set; }

        private bool? _isSolved;

        [UIAuth(UIAuthLevel.ReadOnly)]
        public bool? IsSolved 
        {
            get { return _isSolved; }
            set
            {
                _isSolved = value;
                //if (value == true)
                //    Departments = null;
                RaisePropertyChanged("IsSolved");
            }
        }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public Guid? ParentEventID { get; set; }
        //public ObservableCollection<InventarisationItem> Items { get; set; } = new ObservableCollection<InventarisationItem>();

        public List<Place> Places { get; set; }

        //public List<int> DepartmentTo { get; set; }

        private List<short> _departmentTo;

        public List<short> DepartmentTo
        {
            get { return _departmentTo; }
            set
            {
                _departmentTo = value;
                RaisePropertyChanged("DepartmentTo");
            }
        }



        public List<Department> Departments { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        [Required(ErrorMessage = @"Передел не может быть пустым")]
        public int? PlaceID { get; set; }

        public List<String> Devices { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        [Required(ErrorMessage = @"Устройство не может быть пустым")]
        public string DeviceName { get; set; }

        public int? DeviceID { get; set; }

        public List<EventKind> EventKinds { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        [Required(ErrorMessage = @"Вид события не может быть пустым")]
        public int? EventKindID { get; set; }

        public string Description { get; set; }

        public List<String> PrintNames { get; set; }

        public string EventStateName { get; set; }

        public string DepartmentToNullText { get; set; }
        public string PrintNameNullText { get; set; }

        public short? DepartmentID { get; set; }
        public short? UserDepartmentID { get; set; }

        private string _solvedLabel;

        public string SolvedLabel
        {
            get { return _solvedLabel; }
            set
            {
                _solvedLabel = value;
                RaisePropertyChanged("SolvedLabel");
            }
        }

        private List<LogEvent> _logEventList;

        public List<LogEvent> LogEventList
        {
            get { return _logEventList; }
            set
            {
                _logEventList = value;
                RaisePropertyChanged("LogEventList");
            }
        }

        public LogEvent SelectedLogEvent { get; set; }

        public override bool SaveToModel()
        {
            using (var gammaBase = DB.GammaDb)
            {
                var device = gammaBase.Devices.FirstOrDefault(p => p.Name == DeviceName);
                if (device == null)
                {
                    device = new Devices
                    {
                        Name = DeviceName
                    };
                    gammaBase.Devices.Add(device);
                    gammaBase.SaveChanges();
                    device = gammaBase.Devices.FirstOrDefault(p => p.Name == DeviceName);
                }

                var doc = gammaBase.LogEvents.FirstOrDefault(d => d.EventID == EventID);
                if (doc == null)
                {
                    doc = new LogEvents
                    {
                        EventID = EventID,
                        Date = Date,
                        PrintName = WorkSession.PrintName
                    };
                    gammaBase.LogEvents.Add(doc);
                }
                doc.Number = Number;
                doc.IsSolved = IsSolved;
                if (!IsReadOnly)
                {
                    doc.PlaceID = PlaceID;
                    doc.EventKindID = (int)EventKindID;
                    doc.Description = Description;
                    doc.ParentEventID = ParentEventID;
                    doc.UserID = WorkSession.UserID;
                    if (doc.DepartmentID == null) doc.DepartmentID = (short?)WorkSession.DepartmentID; 
                    doc.DeviceID = device.DeviceID;
                    doc.PrintName = PrintName;
                    doc.EventStateID = 1; //в работе

                    if (DepartmentTo != null && DepartmentTo.Count > 0)
                    {
                        foreach (short i in DepartmentTo)
                        {
                            gammaBase.LogEvents.Add(new LogEvents
                            {
                                EventID = SqlGuidUtil.NewSequentialid(),
                                ParentEventID = EventID,
                                Number = DB.CurrentDateTime.ToString("yyMMddHHmmssf"),
                                Date = DB.CurrentDateTime,
                                UserID = WorkSession.UserID,
                                DepartmentID = i,
                                PlaceID = doc.PlaceID,
                                DeviceID = doc.DeviceID,
                                EventKindID = 0, //принять решение
                                EventStateID = 0 //ожидает просмотра
                            });
                        }
                        doc.EventStateID = 3; //ожидает решения
                    }
                }
                gammaBase.SaveChanges();
            }
            Messenger.Default.Send(new RefreshMessage {});
            return true;
        }
    }
}
