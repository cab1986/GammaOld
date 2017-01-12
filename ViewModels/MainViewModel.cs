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
using Gamma.Entities;

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
        public MainViewModel(GammaEntities gammaBase = null): base(gammaBase)
        {
            Messenger.Default.Register<CloseMessage>(this, msg => CloseSignal = true);
            Messenger.Default.Register<OpenProductionTaskBatchMessage>(this, OpenProductionTaskBatch);
            ViewsManager.Initialize();
            var settings = GammaSettings.Get();
            if (WorkSession.IsProductionPlace) // Если производственный передел
            {
                var dialog = new ChoosePrintNameDialog();
                dialog.ShowDialog();
                if (dialog.DialogResult == true)
                {
                    WorkSession.PrintName = dialog.PrintName;
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
            StatusText = string.Format("Сервер: {0}, БД: {1}, Сканер: {4}, Пользователь: {2}, Имя для печати: {3}", settings.HostName, settings.DbName, 
                settings.User, WorkSession.PrintName, settings.UseScanner ? "вкл" : "выкл");
            if (IsInDesignMode)
            {
                ShowReportListCommand = new DelegateCommand(MessageManager.OpenReportList);
//                ShowProductionTasksPMCommand = new DelegateCommand(() => CurrentView = ViewModelLocator.ProductionTasksPM);
                ShowProductionTasksSGBCommand = new DelegateCommand(() => CurrentView = new ProductionTasksSGBViewModel(DB.GammaDb));
                ShowProductionTasksSGICommand = new DelegateCommand(() => CurrentView = new ProductionTasksSGIViewModel(DB.GammaDb));
                FindProductCommand = new DelegateCommand(MessageManager.OpenFindProduct);
                ManageUsersCommand = new DelegateCommand(MessageManager.OpenManageUsers);
            }
            else
            {
                ShowReportListCommand = new DelegateCommand(MessageManager.OpenReportList,
                () => WorkSession.DBAdmin || DB.HaveWriteAccess("Reports"));
//                ShowProductionTasksPMCommand = new DelegateCommand(() => CurrentView = ViewModelLocator.ProductionTasksPM, () => DB.HaveReadAccess("ProductionTasks"));
                ShowProductionTasksSGBCommand = new DelegateCommand(() => CurrentView = new ProductionTasksSGBViewModel(DB.GammaDb),
                    () => DB.HaveReadAccess("ProductionTasks"));
                ShowProductionTasksBalerCommand = new DelegateCommand(() => CurrentView = new ProductionTasksBalerViewModel(), 
                    () => DB.HaveReadAccess("ProductionTasks"));
                ShowProductionTasksSGICommand = new DelegateCommand(() => CurrentView = new ProductionTasksSGIViewModel(DB.GammaDb),
                    DB.HaveReadAccess("ProductionTasks"));
                FindProductCommand = new DelegateCommand(MessageManager.OpenFindProduct);
                ManageUsersCommand = new DelegateCommand(MessageManager.OpenManageUsers, () => WorkSession.DBAdmin);
                CloseShiftCommand = new DelegateCommand(CloseShift);
                BackCommand = new DelegateCommand(() =>
                {
                    CurrentView = PreviousView;
                    PreviousView = null;
                }, () => PreviousView != null);
                OpenDocCloseShiftsCommand = new DelegateCommand<PlaceGroup>(MessageManager.OpenDocCloseShifts);
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
//                OpenDocMovementOrdersCommand = new DelegateCommand(OpenDocMovementOrders);
                OpenDocMovementsCommand = new DelegateCommand(OpenDocMovements);
                OpenPlaceZonesCommand = new DelegateCommand(OpenPlaceZones);
                ShowProgrammInfoCommand = new DelegateCommand(ShowProgrammInfo);
                OpenQualityReportPMCommand = new DelegateCommand(MessageManager.OpenQualityReportPM);
            }
            switch (WorkSession.PlaceGroup)
            {
                case PlaceGroup.PM:
                case PlaceGroup.Rw:
                    CurrentView = new ProductionTasksSGBViewModel(DB.GammaDb);
                    break;
                case PlaceGroup.Wr:
                    OpenPlaceProducts(WorkSession.PlaceID);
                    break;
                case PlaceGroup.Convertings:
                    CurrentView = new ProductionTasksSGIViewModel(DB.GammaDb);
                    break;
                case PlaceGroup.Baler:
                    CurrentView = new ProductionTasksBalerViewModel();
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

        private void OpenProductionTaskBatch(OpenProductionTaskBatchMessage msg)
        {
            if (msg.Window) return;
            CurrentView = new ProductionTaskBatchViewModel(msg, DB.GammaDb);
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
        }

        private void CloseShift()
        {
            if (WorkSession.ShiftID == 0) return;
            MessageManager.OpenDocCloseShift(WorkSession.PlaceID, DB.CurrentDateTime, WorkSession.ShiftID);
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