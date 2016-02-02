using System.Linq;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using Gamma.Interfaces;
using GalaSoft.MvvmLight.Messaging;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTasksPMViewModel : RootViewModel,IItemManager
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTasksPMViewModel class.
        /// </summary>
        public ProductionTasksPMViewModel()
        {
            GetProductionTasks();
            EditItemCommand = new RelayCommand(EditItem, () => SelectedProductionTask != null);
            NewItemCommand = new RelayCommand(NewProductionTask);
            RefreshCommand = new RelayCommand(GetProductionTasks);
        }

        private void GetProductionTasks()
        {
            var haveWriteAccess = DB.HaveWriteAccess("ProductionTasks");
            ProductionTasks = new ObservableCollection<ProductionTask>
                              (from pt in DB.GammaBase.GetProductionTasks((int)PlaceGroups.PM)
                               select new ProductionTask
                               {
                                   ProductionTaskID = pt.ProductionTaskID,
                                   DateBegin = pt.DateBegin,
                                   Nomenclature = pt.Nomenclature + " " + pt.Characteristic,
                                   Quantity = pt.Quantity,
                                   MadeQuantity = pt.MadeQuantity,
                                   Place = pt.Place
                               });
        }

        private ObservableCollection<ProductionTask> _productiontasks;
        public ObservableCollection<ProductionTask> ProductionTasks
        {
            get { return _productiontasks; }
            private set { 
                    _productiontasks = value;
                    RaisePropertyChanged("ProductionTasks");
                }
        }

        

        private void EditItem()
        {
            OpenProductionTaskMessage msg = new OpenProductionTaskMessage { ProductionTaskID = SelectedProductionTask.ProductionTaskID, ProductionTaskKind = ProductionTaskKinds.ProductionTaskPM };
            MessageManager.OpenProductionTask(msg);
        }

        private void NewProductionTask()
        {
            var msg = new OpenProductionTaskMessage {ProductionTaskKind = ProductionTaskKinds.ProductionTaskPM};
            Messenger.Default.Send<OpenProductionTaskMessage>(msg);
        }
        private ProductionTask _selectedProductionTask;
        public RelayCommand NewItemCommand
        {
            get;
            set;
        }

        public RelayCommand EditItemCommand
        {
            get;
            set;
        }

        public RelayCommand DeleteItemCommand
        {
            get;
            set;
        }
        public RelayCommand RefreshCommand
        {
            get;
            set;
        }
        public ProductionTask SelectedProductionTask
        {
            get
            {
                return _selectedProductionTask;
            }
            set
            {
                _selectedProductionTask = value;
                RaisePropertyChanged("SelectedProductionTask");
            }
        }
    }
}