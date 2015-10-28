using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Linq;
using System.Reflection;
using Gamma.Interfaces;
using System.Collections.Generic;
using System;
using System.Windows;

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
            if (IsInDesignMode)
            {
                ShowReportListCommand = new RelayCommand(() => MessageManager.OpenReportList());
                ShowProductionTasksPMCommand = new RelayCommand(() => CurrentView = ViewModelLocator.ProductionTasksPM);
                ShowProductionTasksRWCommand = new RelayCommand(() => CurrentView = ViewModelLocator.ProductionTasksRW);
                FindProductCommand = new RelayCommand(() => MessageManager.OpenFindProduct(new FindProductMessage { ChooseSourceProduct = false }));
                ManageUsersCommand = new RelayCommand(() => MessageManager.OpenManageUsers());
            }
            else
            {
                ShowReportListCommand = new RelayCommand(() => MessageManager.OpenReportList(),
                () => WorkSession.DBAdmin || DB.GammaBase.UserPermit("Reports").FirstOrDefault() > 1);
                ShowProductionTasksPMCommand = new RelayCommand(() => CurrentView = ViewModelLocator.ProductionTasksPM, () => DB.HaveAccess("ProductionTaskPM"));
                ShowProductionTasksRWCommand = new RelayCommand(() => CurrentView = ViewModelLocator.ProductionTasksRW,
                    () => DB.HaveAccess("ProductionTaskRW"));
                FindProductCommand = new RelayCommand(() => MessageManager.OpenFindProduct(new FindProductMessage { ChooseSourceProduct = false }));
                ManageUsersCommand = new RelayCommand(() => MessageManager.OpenManageUsers(), () => WorkSession.DBAdmin);
            }
            if (WorkSession.PlaceGroup == PlaceGroups.PM) CurrentView = ViewModelLocator.ProductionTasksPM;
            else if (WorkSession.PlaceGroup == PlaceGroups.RW) CurrentView = ViewModelLocator.ProductionTasksRW;
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
        }

        private ViewModelBase _currentgridview;

        public ViewModelBase CurrentGridView
        {
            get {return _currentgridview;}
            set
            {
                _currentgridview = value;
                RaisePropertyChanged("CurrentGridView");
            }
        }

        private Dictionary<string, ViewModelBase> dictGridViews = new Dictionary<string, ViewModelBase>();

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
        public RelayCommand ShowReportListCommand { get; private set; }
        public RelayCommand ShowProductionTasksPMCommand { get; private set; }
        public RelayCommand ShowProductionTasksRWCommand { get; private set; }
        public RelayCommand FindProductCommand { get; private set; }
        public RelayCommand ManageUsersCommand { get; set; }
        public RelayCommand ConfigureComPortCommand { get; set; }
    }
}