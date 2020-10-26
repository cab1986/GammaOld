// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using DevExpress.Mvvm;
using System.Linq;
using Gamma.Interfaces;
using Gamma.Common;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using Gamma.Dialogs;
using System.Deployment.Application;
using System.Diagnostics;
using Gamma.Entities;
using System.Data.Entity.SqlServer;

namespace Gamma.ViewModels
{
    /// <summary>
    /// ViewModel для главного окна приложения
    /// </summary>
    public class MainViewModel : RootViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            Messenger.Default.Register<CloseMessage>(this, msg => CloseSignal = true);
            Messenger.Default.Register<OpenProductionTaskBatchMessage>(this, OpenProductionTaskBatch);
            Messenger.Default.Register<EditDocComplectationMessage>(this, OpenDocComplectation);
            ViewsManager.Initialize();
            var settings = GammaSettings.Get();
            if (WorkSession.ShiftID != 0 && (WorkSession.IsProductionPlace || WorkSession.IsMaterialProductionPlace || WorkSession.IsShipmentWarehouse || WorkSession.IsTransitWarehouse)) // Если производственный передел
            {
                var dialog = new ChoosePrintNameDialog();
                dialog.ShowDialog();
                if (dialog.DialogResult == true)
                {
                    WorkSession.PrintName = dialog.PrintName;
                    WorkSession.PersonID = dialog.PersonID;
                }
                else
                {
                    MessageBox.Show("Не было введено имя для печати, дальнейшая работа невозможна");
                    Application.Current.Shutdown();
                    return;
                }
                if (WorkSession.PlaceGroup == PlaceGroup.Convertings)
                {
                    var placeCurrentUser =
                        GammaBase.CurrentPlaceUsers.FirstOrDefault(pu => pu.PlaceID == WorkSession.PlaceID);
                    if (placeCurrentUser == null)
                    {
                        placeCurrentUser = new CurrentPlaceUsers
                        {
                            PlaceID = WorkSession.PlaceID
                        };
                        GammaBase.CurrentPlaceUsers.Add(placeCurrentUser);
                    }
                    placeCurrentUser.UserID = WorkSession.UserID;
                    placeCurrentUser.ShiftID = WorkSession.ShiftID;
                    placeCurrentUser.PrintName = WorkSession.PrintName;
                    GammaBase.SaveChanges();
                }
            }
            StatusText = string.Format("Сервер: {0}, БД: {1}, Сканер: {4}, Логин: {2}, Имя для печати: {3}, ID пользователя: {5}", settings.HostName, settings.DbName, 
                settings.User, WorkSession.PrintName, settings.UseScanner ? "вкл" : "выкл", WorkSession.PersonID);
            if (IsInDesignMode)
            {
                ShowReportListCommand = new DelegateCommand(MessageManager.OpenReportList);
//                ShowProductionTasksPMCommand = new DelegateCommand(() => CurrentView = ViewModelLocator.ProductionTasksPM);
                ShowProductionTasksSGBCommand = new DelegateCommand(() => CurrentView = new ProductionTasksSGBViewModel());
                ShowProductionTasksSGICommand = new DelegateCommand(() => CurrentView = new ProductionTasksSGIViewModel());
                FindProductCommand = new DelegateCommand(MessageManager.OpenFindProduct);
                ManageUsersCommand = new DelegateCommand(MessageManager.OpenManageUsers);
            }
            else
            {
                ShowReportListCommand = new DelegateCommand(MessageManager.OpenReportList,
                () => WorkSession.DBAdmin || DB.HaveWriteAccess("Reports"));
//                ShowProductionTasksPMCommand = new DelegateCommand(() => CurrentView = ViewModelLocator.ProductionTasksPM, () => DB.HaveReadAccess("ProductionTasks"));
                ShowProductionTasksSGBCommand = new DelegateCommand(() => CurrentView = new ProductionTasksSGBViewModel(),
                    () => DB.HaveReadAccess("ProductionTasks"));
                ShowProductionTasksBalerCommand = new DelegateCommand(() => CurrentView = new ProductionTasksBalerViewModel(), 
                    () => DB.HaveReadAccess("ProductionTasks"));
                ShowProductionTasksSGICommand = new DelegateCommand(() => CurrentView = new ProductionTasksSGIViewModel(),
                    DB.HaveReadAccess("ProductionTasks"));
                FindProductCommand = new DelegateCommand(MessageManager.OpenFindProduct);
                ManageUsersCommand = new DelegateCommand(MessageManager.OpenManageUsers, () => WorkSession.DBAdmin);
                CloseShiftCommand = new DelegateCommand(CloseShift, IsVisibleCloseShiftButton);
                BackCommand = new DelegateCommand(() =>
                {
                    CurrentView = PreviousView;
                    PreviousView = null;
                }, () => PreviousView != null);
                OpenDocCloseShiftsCommand = new DelegateCommand<PlaceGroup>(MessageManager.OpenDocCloseShifts);
                OpenDocUnwinderRemaindersCommand = new DelegateCommand<PlaceGroup>(MessageManager.OpenDocUnwinderRemainders);
                ConfigureComPortCommand = new DelegateCommand(MessageManager.ConfigureComPort, () => WorkSession.DBAdmin || WorkSession.ProgramAdmin);
                FindProductionTaskCommand = new DelegateCommand(FindProductionTask, () => DB.HaveWriteAccess("ProductionTasks"));
                OpenPlaceProductsCommand = new DelegateCommand<int>(OpenPlaceProducts);
                EditPlaceGroupNomenclatureCommand = new DelegateCommand<int>(EditPlaceGroupNomenclature);
                PrintReportCommand = new DelegateCommand<Guid>(PrintReport);
                OpenDocShipmentOrdersCommand = new DelegateCommand(OpenDocShipmentOrders, DB.HaveReadAccess("DocShipmentInfo"));
                OpenDocInOrdersCommand = new DelegateCommand(OpenDocInOrders, DB.HaveReadAccess("DocShipmentInfo"));
                OpenPlaceGroupsNomenclatureCommand = new DelegateCommand(MessageManager.OpenPlaceGroupsNomenclature
                    , () => DB.HaveWriteAccess("PlaceGroup1CNomenclature"));
                OpenMaterialTypesNomenclatureCommand = new DelegateCommand(MessageManager.OpenMaterialTypesNomenclature,
                    () => DB.HaveWriteAccess("MaterialType1CNomenclature"));
                OpenWarehousePersonsCommand = new DelegateCommand(MessageManager.OpenWarehousePersons, () => DB.HaveReadAccess("Persons"));
                OpenImportOldProductsCommand = new DelegateCommand(MessageManager.OpenImportOldProducts, () => DB.HaveWriteAccess("Products"));
                OpenDocBrokeListCommand = new DelegateCommand(OpenDocBrokeList);
                OpenInventarisationsCommand = new DelegateCommand(OpenInventarisations);
                OpenDocMovementsCommand = new DelegateCommand(OpenDocMovements);
                OpenPlaceZonesCommand = new DelegateCommand(OpenPlaceZones);
                ShowProgrammInfoCommand = new DelegateCommand(ShowProgrammInfo);
                OpenQualityReportPMCommand = new DelegateCommand(MessageManager.OpenQualityReportPM);
                OpenHelpCommand = new DelegateCommand(() => Process.Start("http://stgwiki.sgbi.local/index.php/Gamma"));
                OpenDocWithdrawalsCommand = new DelegateCommand(() => CurrentView = new DocWithdrawalsViewModel());
                OpenComplectedPalletsCommand = new DelegateCommand(() => CurrentView = new ComplectedPalletsViewModel());
				OpenDocComplectationsCommand = new DelegateCommand(() => CurrentView = new DocComplectationsListViewModel());
                OpenLogEventsCommand = new DelegateCommand(() => CurrentView = new LogEventsViewModel());
                CreateNewDocBrokeCommand = new DelegateCommand(() => MessageManager.OpenDocBroke(SqlGuidUtil.NewSequentialid()));
                UnwinderRemainderCommand = new DelegateCommand(UnwinderRemainder);
                OpenDocMaterialProductionsCommand = new DelegateCommand(() => CurrentView = new DocMaterialProductionsViewModel());
                OpenStockRemaindersCommand = new DelegateCommand(() => CurrentView = new StockRemaindersViewModel(), WorkSession.ShiftID == 0);
                //                OpenDocMovementOrdersCommand = new DelegateCommand(OpenDocMovementOrders);
            }
            switch (WorkSession.PlaceGroup)
            {
                case PlaceGroup.PM:
                    if (WorkSession.IsMaterialProductionPlace && !WorkSession.IsProductionPlace)
                        CurrentView = new DocMaterialProductionsViewModel();
                    else
                        CurrentView = new ProductionTasksSGBViewModel();
                    break;
                case PlaceGroup.Rw:
                    CurrentView = new ProductionTasksSGBViewModel();
                    break;
                case PlaceGroup.Wr:
                    OpenPlaceProducts(WorkSession.PlaceID);
                    break;
                case PlaceGroup.Convertings:
                    CurrentView = new ProductionTasksSGIViewModel();
                    break;
                case PlaceGroup.Baler:
                    CurrentView = new ProductionTasksBalerViewModel();
                    break;
                case PlaceGroup.Warehouses:
                    if (WorkSession.RoleName == "PalletRepacker")
                        CurrentView = new DocComplectationsListViewModel();
                    else
                        CurrentView = new DocShipmentOrdersViewModel(); 
                    break;
                case PlaceGroup.Services:
                    CurrentView = new LogEventsViewModel();
                    break;

            }
            var places = GammaBase.Places.Where(p => p.IsProductionPlace == true && WorkSession.BranchIds.Contains(p.BranchID));
            PlaceProducts = new ObservableCollection<PlaceProduct>();
            foreach (var place in places)
            {
                PlaceProducts.Add(
                    new PlaceProduct
                    {
                        Place = place.Name,
                        PlaceID = place.PlaceID,
                        Command = OpenPlaceProductsCommand
                    }
                    );
            }
            Reports = new List<ReportItem>(
                                           (from rep in GammaBase.Reports
                                           join parrep in GammaBase.Reports on rep.ParentID equals parrep.ReportID
                                           where parrep.Name == "Reports"
                                           orderby rep.Name
                                           select rep).ToList().Select(p => new ReportItem
                                           {
                                               ReportId = p.ReportID,
                                               ReportName = p.Name,
                                               Command = PrintReportCommand
                                           }));
        }

        public bool IsVisibleUnwinderRemainderButton => (WorkSession.UnwindersCount > 0);

        public bool IsVisibleCloseShiftButton => WorkSession.IsProductionPlace && WorkSession.ShiftID != 0;

        public bool IsVisibleStockRemaindersButton => WorkSession.PlaceGroup == PlaceGroup.Warehouses ;

        public DelegateCommand OpenComplectedPalletsCommand { get; set; }

        public DelegateCommand OpenLogEventsCommand { get; private set; }

        public DelegateCommand CreateNewDocBrokeCommand { get; private set; }

        public DelegateCommand OpenDocComplectationsCommand { get; private set; }

        private void OpenInventarisations()
        {
            UIServices.SetBusyState();
            CurrentView = new DocInventarisationsViewModel();
        }

        private void OpenProductionTaskBatch(OpenProductionTaskBatchMessage msg)
        {
            if (msg.Window) return;
            CurrentView = new ProductionTaskBatchViewModel(msg);
            ActivatedCommand = ((ProductionTaskBatchViewModel) CurrentView).ActivatedCommand;
            DeactivatedCommand = ((ProductionTaskBatchViewModel) CurrentView).DeactivatedCommand;
        }

        private DelegateCommand _activatedCommand;
        private DelegateCommand _deactivatedCommand;

        public DelegateCommand ActivatedCommand 
        {
            get { return _activatedCommand; }
            set
            {
                _activatedCommand = value;
                RaisePropertyChanged("ActivatedCommand");
            }
        }

        public DelegateCommand DeactivatedCommand
        {
            get { return _deactivatedCommand; }
            set
            {
                _deactivatedCommand = value;
                RaisePropertyChanged("DeactivatedCommand");
            }
        }

        public DelegateCommand OpenDocShipmentOrdersCommand { get; private set; }

        private void OpenDocShipmentOrders()
        {
            UIServices.SetBusyState();
            CurrentView = new DocShipmentOrdersViewModel();
        }

        public DelegateCommand OpenDocInOrdersCommand { get; private set; }

        private void OpenDocInOrders()
        {
            UIServices.SetBusyState();
            CurrentView = new DocShipmentOrdersViewModel(false);
        }

        /*
        private void OpenDocMovementOrders()
        {
            UIServices.SetBusyState();
            CurrentView = new DocMovementOrdersViewModel();
        }
        */

        private void OpenDocMovements()
        {
            UIServices.SetBusyState();
            CurrentView = new DocMovementsViewModel();
        }

        private void OpenDocBrokeList()
        {
            UIServices.SetBusyState();
            CurrentView = new DocBrokeListViewModel();
        }

        private void OpenDocComplectation(EditDocComplectationMessage msg)
        {
            CurrentView = new DocComplectationViewModel(msg.DocId);
        }

        private void OpenPlaceProducts(int placeID)
        {
            UIServices.SetBusyState();
            (CurrentView as IDisposable)?.Dispose();
            CurrentView = new PlaceProductsViewModel(placeID);
            RefreshCommand = ((PlaceProductsViewModel) CurrentView).FindCommand;
            NewItemCommand = ((PlaceProductsViewModel) CurrentView).CreateNewProductCommand;
            EditItemCommand = ((PlaceProductsViewModel) CurrentView).OpenDocProductCommand;
            DeleteItemCommand = ((PlaceProductsViewModel) CurrentView).DeleteProductCommand;
        }

        private void EditPlaceGroupNomenclature(int placeGroupId)
        {
            MessageManager.FindNomenclature(placeGroupId, true);
        }

        private void PrintReport(Guid reportId)
        {
            ReportManager.PrintReport(reportId);
        }

        private void FindProductionTask()
        {
//            if (CurrentView is ProductionTasksPMViewModel)
//                MessageManager.FindProductionTaskBatch(BatchKinds.SGB);
            if (CurrentView is ProductionTasksSGBViewModel)
                MessageManager.FindProductionTaskBatch(BatchKinds.SGB);
            else if (CurrentView is ProductionTasksSGIViewModel)
                MessageManager.FindProductionTaskBatch(BatchKinds.SGI);
            else if (CurrentView is ProductionTasksBalerViewModel) 
                MessageManager.FindProductionTaskBatch(BatchKinds.Baler);
        }

        private void CloseShift()
        {
            if (WorkSession.ShiftID == 0) return;
            using (var gammaBase = DB.GammaDb)
            {
                var existNonConfirmedDocMaterialProductions = GammaBase.GetDocMaterialProductionsOnShift(WorkSession.PlaceID, WorkSession.ShiftID, SqlFunctions.GetDate())
                    .Where(m=> !m.IsConfirmed)
                    .Count();

                if (existNonConfirmedDocMaterialProductions > 0)
                {
                    gammaBase.CriticalLogs.Add(new CriticalLogs { LogID = SqlGuidUtil.NewSequentialid(), LogDate = DB.CurrentDateTime, LogUserID = WorkSession.UserName, Log = "Создание Рапорта закрытия смены @PlaceID " + WorkSession.PlaceID.ToString() + ", @ShiftID " + WorkSession.ShiftID.ToString() + ", @Date " + SqlFunctions.GetDate().ToString() + " Есть Неподтвержденный документ Расхода сырья и материалов за смену.Требуется подтвердить или удалить, иначе материалы будут рассчитаны неправильно!" });
                    MessageBox.Show("Есть Неподтвержденный документ Расхода сырья и материалов за смену." + Environment.NewLine + "Требуется подтвердить или удалить, иначе материалы будут рассчитаны неправильно!",
                        "Рапорт за смену", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                var lastReport =
                    gammaBase.Docs.Where(
                        d => d.ShiftID == WorkSession.ShiftID && d.DocTypeID == (int) DocTypes.DocCloseShift && d.PlaceID == WorkSession.PlaceID &&
                        d.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, SqlFunctions.GetDate()))) &&
                        d.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, SqlFunctions.GetDate())))
                        && (WorkSession.PersonID == null || WorkSession.PersonID.ToString() == "00000000-0000-0000-0000-000000000000" || (WorkSession.PersonID != null && d.PersonGuid == WorkSession.PersonID) )
                        )
                        .OrderByDescending(d => d.Date)
                        .FirstOrDefault();
                if (lastReport != null)
                {
                    MessageBox.Show("Есть рапорт за смену, он будет открыт для редактирования",
                        "Рапорт за смену", MessageBoxButton.OK, MessageBoxImage.Information);
                    MessageManager.OpenDocCloseShift(lastReport.DocID);
                    return;
                }
            }
            MessageManager.OpenDocCloseShift(WorkSession.PlaceID, DB.CurrentDateTime, WorkSession.ShiftID, WorkSession.PersonID);
        }

        private void UnwinderRemainder()
        {
            if (WorkSession.ShiftID == 0) return;
            using (var gammaBase = DB.GammaDb)
            {
                var lastDoc =
                    gammaBase.Docs.Where(
                        d => d.ShiftID == WorkSession.ShiftID && d.DocTypeID == (int)DocTypes.DocUnwinderRemainder && d.PlaceID == WorkSession.PlaceID &&
                        d.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, SqlFunctions.GetDate()))) &&
                        d.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, SqlFunctions.GetDate())))
                        && (WorkSession.PersonID == null || WorkSession.PersonID.ToString() == "00000000-0000-0000-0000-000000000000" || (WorkSession.PersonID != null && d.PersonGuid == WorkSession.PersonID))
                        )
                        .OrderByDescending(d => d.Date)
                        .Select(d => d.DocID)
                        .FirstOrDefault();
                if (lastDoc != null && lastDoc != Guid.Empty)
                {
                    MessageBox.Show("Есть документ передачи остатков на раскатах за смену, он будет открыт для редактирования",
                        "Остатки на раскатах за смену", MessageBoxButton.OK, MessageBoxImage.Information);
                    MessageManager.OpenDocUnwinderRemainder(lastDoc);
                    return;
                }
            }
            MessageManager.OpenDocUnwinderRemainder(WorkSession.PlaceID, DB.CurrentDateTime, WorkSession.ShiftID, WorkSession.PersonID);
        }

        private void CurrentViewChanged()
        {
            if ((CurrentView as IItemManager)!= null)
            {
                var itemManager = (IItemManager) CurrentView;
                NewItemCommand = itemManager.NewItemCommand;
                EditItemCommand = itemManager.EditItemCommand;
                DeleteItemCommand = itemManager.DeleteItemCommand;
                RefreshCommand = itemManager.RefreshCommand;
            }
            else
            {
                NewItemCommand = null;
                EditItemCommand = null;
                DeleteItemCommand = null;
                RefreshCommand = null;
            }
            if (CurrentView is ProductionTasksSGBViewModel || CurrentView is ProductionTasksSGIViewModel || CurrentView is ProductionTasksBalerViewModel)
                ProductionTaskBarVisible = true;
            else ProductionTaskBarVisible = false;
        }
        private RootViewModel _currentView;
        public RootViewModel CurrentView
        {
            get { return _currentView; }
            private set
            {
                PreviousView = PreviousView == value ? PreviousView = null : PreviousView = _currentView;
                _currentView = value;
                RaisePropertyChanged("CurrentView");
                CurrentViewChanged();
            }
        }

        private RootViewModel PreviousView { get; set; }
        public string StatusText { get; set; }
        private DelegateCommand _newItemCommand;
        public DelegateCommand NewItemCommand 
        {
            get { return _newItemCommand; }
            private set 
            {
                _newItemCommand = value;
                RaisePropertyChanged("NewItemCommand");
            }
        }
        private DelegateCommand<object> _editItemCommand;
        public DelegateCommand<object> EditItemCommand 
        {
            get { return _editItemCommand; }
            private set
            {
                _editItemCommand = value;
                RaisePropertyChanged("EditItemCommand");
            }
        }
        private DelegateCommand _deleteItemCommand;
        public DelegateCommand DeleteItemCommand 
        {
            get { return _deleteItemCommand; }
            private set
            {
                _deleteItemCommand = value;
                RaisePropertyChanged("DeleteItemCommand");
            }
        }

        private DelegateCommand _backCommand;
        public DelegateCommand BackCommand
        {
            get { return _backCommand; }
            set
            {
                _backCommand = value;
                RaisePropertyChanged("BackCommand");
            }
        }

        public DelegateCommand ShowProgrammInfoCommand { get; private set; }

        private void ShowProgrammInfo()
        {
            string version;
            try
            {
                version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            catch
            {
                version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            MessageBox.Show(@"Текущая версия приложения " + version);
        }

        public DelegateCommand OpenPlaceZonesCommand { get; private set; }

        private void OpenPlaceZones()
        {
            MessageManager.OpenPlaceZones();
        }

        private DelegateCommand _refreshCommand;
        public DelegateCommand RefreshCommand 
        {
            get { return _refreshCommand; }
            private set
            {
                _refreshCommand = value;
                RaisePropertyChanged("RefreshCommand");
            }
        }

        private bool _productionTaskBarVisible;
        public bool ProductionTaskBarVisible
        {
            get
            {
                return _productionTaskBarVisible;
            }
            set
            {
            	_productionTaskBarVisible = value;
                RaisePropertyChanged("ProductionTaskBarVisible");
            }
        }
        public DelegateCommand ShowReportListCommand { get; private set; }
        public DelegateCommand ShowProductionTasksSGICommand { get; private set; }
        public DelegateCommand ShowProductionTasksSGBCommand { get; private set; }
        public DelegateCommand ShowProductionTasksBalerCommand { get; private set; }
        public DelegateCommand FindProductCommand { get; private set; }
        public DelegateCommand ManageUsersCommand { get; set; }
        public DelegateCommand ConfigureComPortCommand { get; set; }
        public DelegateCommand CloseShiftCommand { get; set; }
        public DelegateCommand<PlaceGroup> OpenDocCloseShiftsCommand { get; private set; }
        public DelegateCommand<PlaceGroup> OpenDocUnwinderRemaindersCommand { get; private set; }
        public DelegateCommand FindProductionTaskCommand { get; private set; }
        public DelegateCommand<int> OpenPlaceProductsCommand { get; private set; }
        public DelegateCommand OpenPlaceGroupsNomenclatureCommand { get; private set; }
        public DelegateCommand OpenMaterialTypesNomenclatureCommand { get; private set; }
        public DelegateCommand OpenWarehousePersonsCommand { get; private set; }
        public DelegateCommand OpenImportOldProductsCommand { get; private set; }
        public DelegateCommand OpenDocBrokeListCommand { get; private set; }
        public DelegateCommand<Guid> PrintReportCommand { get; private set; }
        public ObservableCollection<PlaceProduct> PlaceProducts { get; set; }
        public DelegateCommand<int> EditPlaceGroupNomenclatureCommand { get; private set; }
        public DelegateCommand OpenDocMovementOrdersCommand { get; private set; }
        public DelegateCommand OpenDocMovementsCommand { get; private set; }
        public DelegateCommand OpenQualityReportPMCommand { get; private set; }
        public DelegateCommand ComplectPalletCommand { get; private set; }
        public DelegateCommand OpenInventarisationsCommand { get; set; }
        public DelegateCommand OpenHelpCommand { get; set; }
        public DelegateCommand OpenDocWithdrawalsCommand { get; set; }
        public DelegateCommand UnwinderRemainderCommand { get; set; }
        public DelegateCommand OpenDocMaterialProductionsCommand { get; private set; }
        public DelegateCommand OpenStockRemaindersCommand { get; private set; }

        public List<ReportItem> Reports { get; set; }



        public class PlaceProduct
        {
            public string Place { get; set; }
            public DelegateCommand<int> Command { get; set; }
            public int PlaceID { get; set; }
        }

        public class ReportItem
        {
            public DelegateCommand<Guid> Command { get; set; }
            public Guid ReportId { get; set; }
            public string ReportName { get; set; }
        }

    }
}