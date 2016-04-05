using DevExpress.Mvvm;
using System.Linq;
using Gamma.Interfaces;
using System;
using Gamma.Common;
using System.Collections.ObjectModel;
using Gamma.Dialogs;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ViewsManager.Initialize();
            var settings = GammaSettings.Get();
            if (WorkSession.PlaceID > 0) // Если не администрация
            {
                var dialog = new ChoosePrintNameDialog();
                dialog.ShowDialog();
                if (dialog.DialogResult == true)
                {
                    WorkSession.PrintName = dialog.PrintName;
                }
            }
            StatusText = String.Format("Сервер: {0}, БД: {1}, Сканер: {4}, Пользователь: {2}, Имя для печати: {3}", settings.HostName, settings.DbName, 
                settings.User, WorkSession.PrintName, settings.UseScanner ? "вкл" : "выкл");
            if (IsInDesignMode)
            {
                ShowReportListCommand = new DelegateCommand(() => MessageManager.OpenReportList());
//                ShowProductionTasksPMCommand = new DelegateCommand(() => CurrentView = ViewModelLocator.ProductionTasksPM);
                ShowProductionTasksSGBCommand = new DelegateCommand(() => CurrentView = ViewModelLocator.ProductionTasksSGB);
                FindProductCommand = new DelegateCommand(() => MessageManager.OpenFindProduct());
                ManageUsersCommand = new DelegateCommand(() => MessageManager.OpenManageUsers());
            }
            else
            {
                ShowReportListCommand = new DelegateCommand(() => MessageManager.OpenReportList(),
                () => WorkSession.DBAdmin || DB.HaveWriteAccess("Reports"));
//                ShowProductionTasksPMCommand = new DelegateCommand(() => CurrentView = ViewModelLocator.ProductionTasksPM, () => DB.HaveReadAccess("ProductionTasks"));
                ShowProductionTasksSGBCommand = new DelegateCommand(() => CurrentView = ViewModelLocator.ProductionTasksSGB,
                    () => DB.HaveReadAccess("ProductionTasks"));
                FindProductCommand = new DelegateCommand(() => MessageManager.OpenFindProduct());
                ManageUsersCommand = new DelegateCommand(() => MessageManager.OpenManageUsers(), () => WorkSession.DBAdmin);
                CloseShiftCommand = new DelegateCommand(CloseShift);
                OpenDocCloseShiftsCommand = new DelegateCommand<PlaceGroups>((p) => MessageManager.OpenDocCloseShifts(p));
                ConfigureComPortCommand = new DelegateCommand(() => MessageManager.ConfigureComPort(), () => WorkSession.DBAdmin || WorkSession.ProgramAdmin);
                FindProductionTaskCommand = new DelegateCommand(FindProductionTask, () => DB.HaveWriteAccess("ProductionTasks"));
                OpenPlaceProductsCommand = new DelegateCommand<int>(OpenPlaceProducts);
                OpenPlaceGroupsNomenclatureCommand = new DelegateCommand(() => MessageManager.OpenPlaceGroupsNomenclature()
                    , () => DB.HaveWriteAccess("PlaceGroup1CNomenclature"));
            }
            switch (WorkSession.PlaceGroup)
            {
                case PlaceGroups.PM:
                case PlaceGroups.RW:
                    CurrentView = ViewModelLocator.ProductionTasksSGB;
                    break;
                case PlaceGroups.WR:
                    OpenPlaceProducts(WorkSession.PlaceID);
                    break;
            }
            var places = DB.GammaBase.Places.Where(p => p.IsProductionPlace == true).Select(p => p);
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
        }
        private void OpenPlaceProducts(int placeID)
        {
            UIServices.SetBusyState();
            CurrentView = new PlaceProductsViewModel(placeID);
            RefreshCommand = (CurrentView as PlaceProductsViewModel).FindCommand;
            NewItemCommand = (CurrentView as PlaceProductsViewModel).CreateNewProductCommand;
            EditItemCommand = (CurrentView as PlaceProductsViewModel).OpenDocProductCommand;
            DeleteItemCommand = (CurrentView as PlaceProductsViewModel).DeleteProductCommand;
        }
        private void FindProductionTask()
        {
//            if (CurrentView is ProductionTasksPMViewModel)
//                MessageManager.FindProductionTaskBatch(BatchKinds.SGB);
            if (CurrentView is ProductionTasksSGBViewModel)
                MessageManager.FindProductionTaskBatch(BatchKinds.SGB);
            else if (CurrentView is ProductionTasksConvertingViewModel)
                MessageManager.FindProductionTaskBatch(BatchKinds.SGI);
        }

        private void CloseShift()
        {
            if (WorkSession.ShiftID == 0) return;
            MessageManager.OpenDocCloseShift((int)WorkSession.PlaceID, DB.CurrentDateTime, WorkSession.ShiftID);
        }

        private void CurrentViewChanged()
        {
            if (CurrentView != null && CurrentView.GetType().GetInterfaces().Contains(typeof(IItemManager)))
            {
                NewItemCommand = (CurrentView as IItemManager).NewItemCommand;
                EditItemCommand = (CurrentView as IItemManager).EditItemCommand;
                DeleteItemCommand = (CurrentView as IItemManager).DeleteItemCommand;
                RefreshCommand = (CurrentView as IItemManager).RefreshCommand;
            }
            else
            {
                NewItemCommand = null;
                EditItemCommand = null;
                DeleteItemCommand = null;
                RefreshCommand = null;
            }
            if (CurrentView is ProductionTasksSGBViewModel || CurrentView is ProductionTasksConvertingViewModel)
                ProductionTaskBarVisible = true;
            else ProductionTaskBarVisible = false;
        }
        private ViewModelBase currentView;
        public ViewModelBase CurrentView
        {
            get { return currentView; }
            private set { 
                currentView = value;
                RaisePropertyChanged("CurrentView");
                CurrentViewChanged();
            }
        }
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
        private DelegateCommand _editItemCommand;
        public DelegateCommand EditItemCommand 
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
        public DelegateCommand ShowProductionTasksConvertingCommand { get; private set; }
//        public DelegateCommand ShowProductionTasksPMCommand { get; private set; }
        public DelegateCommand ShowProductionTasksSGBCommand { get; private set; }
        public DelegateCommand FindProductCommand { get; private set; }
        public DelegateCommand ManageUsersCommand { get; set; }
        public DelegateCommand ConfigureComPortCommand { get; set; }
        public DelegateCommand CloseShiftCommand { get; set; }
        public DelegateCommand<PlaceGroups> OpenDocCloseShiftsCommand { get; private set; }
        public DelegateCommand FindProductionTaskCommand { get; private set; }
        public DelegateCommand<int> OpenPlaceProductsCommand { get; private set; }
        public DelegateCommand OpenPlaceGroupsNomenclatureCommand { get; private set; }
        public ObservableCollection<PlaceProduct> PlaceProducts { get; set; }
        public class PlaceProduct
        {
            public string Place { get; set; }
            public DelegateCommand<int> Command { get; set; }
            public int PlaceID { get; set; }
        }
    }
}