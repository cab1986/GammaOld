using GalaSoft.MvvmLight.Command;
using Gamma.Interfaces;
using System.Collections.ObjectModel;
using System;
using GalaSoft.MvvmLight.Messaging;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTasksConvertingViewModel : RootViewModel,IItemManager
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTasksConvertingViewModel class.
        /// </summary>
        public ProductionTasksConvertingViewModel()
        {
            GetProductionTasks();
            EditItemCommand = new RelayCommand(EditItem);
            NewItemCommand = new RelayCommand(NewProductionTask);
            RefreshCommand = new RelayCommand(GetProductionTasks);
        }
        private void GetProductionTasks()
        {
  /*          ProductionTasks = new ObservableCollection<ProductionTask>
                              (from pt in DB.GammaBase.ProductionTasks
                               where pt.ProductionTaskKindID == (short)ProductionTaskKinds.ProductionTaskConverting
                               select new ProductionTask
                               {
                                   ProductionTaskID = pt.ProductionTaskID,
                                   DateBegin = pt.DateBegin,
                                   Nomenclature = pt.C1CNomenclature.Name + " " + pt.C1CCharacteristics.Name,
                                   Quantity = pt.Quantity
                               });
   * */
        }
        private void EditItem()
        {
            OpenProductionTaskBatchMessage msg = new OpenProductionTaskBatchMessage { ProductionTaskBatchID = SelectedProductionTask.ProductionTaskID, BatchKind = BatchKinds.SGI };
            MessageManager.OpenProductionTask(msg);
        }

        private void NewProductionTask()
        {
            var msg = new OpenProductionTaskBatchMessage { BatchKind = BatchKinds.SGI };
            Messenger.Default.Send<OpenProductionTaskBatchMessage>(msg);
        }

        private ObservableCollection<ProductionTask> _productionTasks;
        public ObservableCollection<ProductionTask> ProductionTasks
        {
            get
            {
                return _productionTasks;
            }
            set
            {
            	_productionTasks = value;
                RaisePropertyChanged("ProductionTasks");
            }
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

        public class ProductionTask
        {
            public Guid ProductionTaskID {get; set; }
            public DateTime? DateBegin { get; set; }
            public string Nomenclature { get; set; }
            public decimal? Quantity { get; set; }
        }        
    }
}