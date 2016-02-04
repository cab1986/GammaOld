using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Linq;
using Gamma.Interfaces;
using System.Collections.Generic;
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
                ShowReportListCommand = new RelayCommand(() => MessageManager.OpenReportList());
                ShowProductionTasksPMCommand = new RelayCommand(() => CurrentView = ViewModelLocator.ProductionTasksPM);
                ShowProductionTasksRWCommand = new RelayCommand(() => CurrentView = ViewModelLocator.ProductionTasksRW);
                FindProductCommand = new RelayCommand(() => MessageManager.OpenFindProduct());
                ManageUsersCommand = new RelayCommand(() => MessageManager.OpenManageUsers());
            }
            else
            {
                ShowReportListCommand = new RelayCommand(() => MessageManager.OpenReportList(),
                () => WorkSession.DBAdmin || DB.HaveWriteAccess("Reports"));
                ShowProductionTasksPMCommand = new RelayCommand(() => CurrentView = ViewModelLocator.ProductionTasksPM, () => DB.HaveReadAccess("ProductionTasks"));
                ShowProductionTasksRWCommand = new RelayCommand(() => CurrentView = ViewModelLocator.ProductionTasksRW,
                    () => DB.HaveReadAccess("ProductionTasks"));
                FindProductCommand = new RelayCommand(() => MessageManager.OpenFindProduct());
                ManageUsersCommand = new RelayCommand(() => MessageManager.OpenManageUsers(), () => WorkSession.DBAdmin);
                CloseShiftCommand = new RelayCommand(CloseShift);
                OpenDocCloseShiftsCommand = new RelayCommand<PlaceGroups>((p) => MessageManager.OpenDocCloseShifts(p));
                ConfigureComPortCommand = new RelayCommand(() => MessageManager.ConfigureComPort(), () => WorkSession.DBAdmin || WorkSession.ProgramAdmin);
                FindProductionTaskCommand = new RelayCommand(FindProductionTask, () => DB.HaveWriteAccess("ProductionTasks"));
                OpenPlaceProductsCommand = new RelayCommand<int>(OpenPlaceProducts);
                OpenPlaceGroupsNomenclatureCommand = new RelayCommand(() => MessageManager.OpenPlaceGroupsNomenclature()
                    , () => DB.HaveWriteAccess("PlaceGroup1CNomenclature"));
            }
            switch (WorkSession.PlaceGroup)
            {
                case PlaceGroups.PM:
                    CurrentView = ViewModelLocator.ProductionTasksPM;
                    break;
                case PlaceGroups.RW:
                    CurrentView = ViewModelLocator.ProductionTasksRW;
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
            CurrentView = new PlaceProductsViewModel(placeID);
            RefreshCommand = (CurrentView as PlaceProductsViewModel).FindCommand;
            NewItemCommand = (CurrentView as PlaceProductsViewModel).CreateNewProductCommand;
            EditItemCommand = (CurrentView as PlaceProductsViewModel).OpenDocProductCommand;
        }
        private void FindProductionTask()
        {
            if (CurrentView is ProductionTasksPMViewModel)
                MessageManager.FindProductionTask(PlaceGroups.PM);
            else if (CurrentView is ProductionTasksRWViewModel)
                MessageManager.FindProductionTask(PlaceGroups.RW);
            else if (CurrentView is ProductionTasksConvertingViewModel)
                MessageManager.FindProductionTask(PlaceGroups.Convertings);
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
            if (CurrentView is ProductionTasksPMViewModel || CurrentView is ProductionTasksRWViewModel || CurrentView is ProductionTasksConvertingViewModel)
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
        private RelayCommand _newItemCommand;
        public RelayCommand NewItemCommand 
        {
            get { return _newItemCommand; }
            private set 
            {
                _newItemCommand = value;
                RaisePropertyChanged("NewItemCommand");
            }
        }
        private RelayCommand _editItemCommand;
        public RelayCommand EditItemCommand 
        {
            get { return _editItemCommand; }
            private set
            {
                _editItemCommand = value;
                RaisePropertyChanged("EditItemCommand");
            }
        }
        private RelayCommand _deleteItemCommand;
        public RelayCommand DeleteItemCommand 
        {
            get { return _deleteItemCommand; }
            private set
            {
                _deleteItemCommand = value;
                RaisePropertyChanged("DeleteItemCommand");
            }
        }
        private RelayCommand _refreshCommand;
        public RelayCommand RefreshCommand 
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
        public RelayCommand ShowReportListCommand { get; private set; }
        public RelayCommand ShowProductionTasksConvertingCommand { get; private set; }
        public RelayCommand ShowProductionTasksPMCommand { get; private set; }
        public RelayCommand ShowProductionTasksRWCommand { get; private set; }
        public RelayCommand FindProductCommand { get; private set; }
        public RelayCommand ManageUsersCommand { get; set; }
        public RelayCommand ConfigureComPortCommand { get; set; }
        public RelayCommand CloseShiftCommand { get; set; }
        public RelayCommand<PlaceGroups> OpenDocCloseShiftsCommand { get; private set; }
        public RelayCommand FindProductionTaskCommand { get; private set; }
        public RelayCommand<int> OpenPlaceProductsCommand { get; private set; }
        public RelayCommand OpenPlaceGroupsNomenclatureCommand { get; private set; }
        public ObservableCollection<PlaceProduct> PlaceProducts { get; set; }
        public class PlaceProduct
        {
            public string Place { get; set; }
            public RelayCommand<int> Command { get; set; }
            public int PlaceID { get; set; }
        }
    }
}