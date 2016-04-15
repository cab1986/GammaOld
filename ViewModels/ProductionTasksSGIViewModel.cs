using DevExpress.Mvvm;
using Gamma.Interfaces;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using Gamma.Common;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTasksSGIViewModel : RootViewModel,IItemManager
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTasksConvertingViewModel class.
        /// </summary>
        public ProductionTasksSGIViewModel()
        {
            GetProductionTasks();
            EditItemCommand = new DelegateCommand(EditItem);
            NewItemCommand = new DelegateCommand(NewProductionTask);
            RefreshCommand = new DelegateCommand(GetProductionTasks);
            DeleteItemCommand = new DelegateCommand(DeleteProductionTask);
        }

        private void DeleteProductionTask()
        {
            if (SelectedProductionTaskSGI == null) return;
            DB.GammaDb.DeleteProductionTaskBatch(SelectedProductionTaskSGI.ProductionTaskBatchID);
        }

        private void GetProductionTasks()
        {
            UiServices.SetBusyState();
            ProductionTasks = new ObservableCollection<ProductionTaskSGI>(
                from pt in DB.GammaDb.GetProductionTasks((int) BatchKinds.SGI)
                select new ProductionTaskSGI()
                {
                    ProductionTaskBatchID = pt.ProductionTaskBatchID,
                    Quantity = pt.Quantity,
                    DateBegin = pt.DateBegin,
                    Nomenclature = pt.Nomenclature + "/r/n" + pt.Characteristic,
                    Place = pt.Place,
                    Number = pt.Number,
                    MadeQuantity = pt.MadeQuantity
                }
                );

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
            var msg = new OpenProductionTaskBatchMessage { ProductionTaskBatchID = SelectedProductionTaskSGI.ProductionTaskBatchID, BatchKind = BatchKinds.SGI };
            MessageManager.OpenProductionTask(msg);
        }
        private void NewProductionTask()
        {
            MessageManager.OpenProductionTask(new OpenProductionTaskBatchMessage
            {
                BatchKind = BatchKinds.SGI
            });
        }

        private ObservableCollection<ProductionTaskSGI> _productionTasks;
        public ObservableCollection<ProductionTaskSGI> ProductionTasks
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
        private ProductionTaskSGI _selectedProductionTaskSGI;
        public DelegateCommand NewItemCommand
        {
            get;
            set;
        }

        public DelegateCommand EditItemCommand
        {
            get;
            set;
        }

        public DelegateCommand DeleteItemCommand
        {
            get;
            set;
        }
        public DelegateCommand RefreshCommand
        {
            get;
            set;
        }
        public ProductionTaskSGI SelectedProductionTaskSGI
        {
            get
            {
                return _selectedProductionTaskSGI;
            }
            set
            {
                _selectedProductionTaskSGI = value;
                RaisePropertyChanged("SelectedProductionTask");
            }
        }

        public class ProductionTaskSGI
        {
            public Guid ProductionTaskBatchID {get; set; }
            public DateTime? DateBegin { get; set; }
            public string Nomenclature { get; set; }
            public decimal? Quantity { get; set; }
            public string Place { get; set; }
            public string Number { get; set; }
            public string MadeQuantity { get; set; }
        }        
    }
}