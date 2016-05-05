using DevExpress.Mvvm;
using System.Linq;
using Gamma.Interfaces;
using Gamma.Common;
using System.Collections.ObjectModel;
using Gamma.Dialogs;
using Gamma.Models;

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
                ShowProductionTasksSGICommand = new DelegateCommand(() => CurrentView = new ProductionTasksSGIViewModel(DB.GammaDb),
                    DB.HaveReadAccess("ProductionTasks"));
                FindProductCommand = new DelegateCommand(MessageManager.OpenFindProduct);
                ManageUsersCommand = new DelegateCommand(MessageManager.OpenManageUsers, () => WorkSession.DBAdmin);
                CloseShiftCommand = new DelegateCommand(CloseShift);
                OpenDocCloseShiftsCommand = new DelegateCommand<PlaceGroups>(MessageManager.OpenDocCloseShifts);
                ConfigureComPortCommand = new DelegateCommand(MessageManager.ConfigureComPort, () => WorkSession.DBAdmin || WorkSession.ProgramAdmin);
                FindProductionTaskCommand = new DelegateCommand(FindProductionTask, () => DB.HaveWriteAccess("ProductionTasks"));
                OpenPlaceProductsCommand = new DelegateCommand<int>(OpenPlaceProducts);
                OpenPlaceGroupsNomenclatureCommand = new DelegateCommand(MessageManager.OpenPlaceGroupsNomenclature
                    , () => DB.HaveWriteAccess("PlaceGroup1CNomenclature"));
                OpenMaterialTypesNomenclatureCommand = new DelegateCommand(MessageManager.OpenMaterialTypesNomenclature,
                    () => DB.HaveWriteAccess("MaterialType1CNomenclature"));
            }
            switch (WorkSession.PlaceGroup)
            {
                case PlaceGroups.PM:
                case PlaceGroups.Rw:
                    CurrentView = new ProductionTasksSGBViewModel(DB.GammaDb);
                    break;
                case PlaceGroups.Wr:
                    OpenPlaceProducts(WorkSession.PlaceID);
                    break;
                case PlaceGroups.Convertings:
                    CurrentView = new ProductionTasksSGIViewModel(DB.GammaDb);
                    break;
            }
            var places = GammaBase.Places.Where(p => p.IsProductionPlace == true).Select(p => p);
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
            if (CurrentView != null && CurrentView.GetType().GetInterfaces().Contains(typeof(IItemManager)))
            {
                NewItemCommand = (CurrentView as IItemManager)?.NewItemCommand;
                EditItemCommand = (CurrentView as IItemManager)?.EditItemCommand;
                DeleteItemCommand = (CurrentView as IItemManager)?.DeleteItemCommand;
                RefreshCommand = (CurrentView as IItemManager)?.RefreshCommand;
            }
            else
            {
                NewItemCommand = null;
                EditItemCommand = null;
                DeleteItemCommand = null;
                RefreshCommand = null;
            }
            if (CurrentView is ProductionTasksSGBViewModel || CurrentView is ProductionTasksSGIViewModel)
                ProductionTaskBarVisible = true;
            else ProductionTaskBarVisible = false;
        }
        private ViewModelBase _currentView;
        public ViewModelBase CurrentView
        {
            get { return _currentView; }
            private set { 
                _currentView = value;
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
        public DelegateCommand ShowProductionTasksSGICommand { get; private set; }
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
        public DelegateCommand OpenMaterialTypesNomenclatureCommand { get; private set; }
        public ObservableCollection<PlaceProduct> PlaceProducts { get; set; }
        public class PlaceProduct
        {
            public string Place { get; set; }
            public DelegateCommand<int> Command { get; set; }
            public int PlaceID { get; set; }
        }
    }
}