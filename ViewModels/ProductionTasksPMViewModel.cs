using System.Linq;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using Gamma.Interfaces;
using GalaSoft.MvvmLight.Messaging;
using Gamma.Common;

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
            ProductionTasks = new ObservableCollection<ProductionTaskBatch>
                              (from pt in DB.GammaBase.GetProductionTasks((int)PlaceGroups.PM)
                               select new ProductionTaskBatch
                               {
                                   ProductionTaskBatchID = pt.ProductionTaskBatchID,
                                   DateBegin = pt.DateBegin,
                                   Nomenclature = pt.Nomenclature + " " + pt.Characteristic,
                                   Quantity = pt.Quantity,
                                   MadeQuantity = pt.MadeQuantity,
                                   Place = pt.Place
                               });
        }

        private ObservableCollection<ProductionTaskBatch> _productiontasks;
        public ObservableCollection<ProductionTaskBatch> ProductionTasks
        {
            get { return _productiontasks; }
            private set { 
                    _productiontasks = value;
                    RaisePropertyChanged("ProductionTasks");
                }
        }    
        private void EditItem()
        {
            OpenProductionTaskBatchMessage msg = new OpenProductionTaskBatchMessage { ProductionTaskBatchID = SelectedProductionTask.ProductionTaskBatchID, BatchKind = BatchKinds.SGB };
            MessageManager.OpenProductionTask(msg);
        }

        private void NewProductionTask()
        {
            var msg = new OpenProductionTaskBatchMessage {BatchKind = BatchKinds.SGB};
            Messenger.Default.Send<OpenProductionTaskBatchMessage>(msg);
        }
        private ProductionTaskBatch _selectedProductionTask;
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
        public ProductionTaskBatch SelectedProductionTask
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