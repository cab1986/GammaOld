// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using DevExpress.Mvvm;
using Gamma.Interfaces;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using Gamma.Common;
using Gamma.Entities;

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
        /// Initializes a new instance of the ProductionTasksConvertingGridViewModel class.
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
            UIServices.SetBusyState();
            ProductionTasks = new ObservableCollection<ProductionTaskSGI>(
                from pt in DB.GammaDb.GetProductionTasks((int) BatchKinds.SGI)
                select new ProductionTaskSGI()
                {
                    ProductionTaskBatchID = pt.ProductionTaskBatchID,
                    Quantity = pt.Quantity,
                    DateBegin = pt.DateBegin,
                    Nomenclature = pt.Nomenclature + "\r\n" + pt.Characteristic,
                    Place = pt.Place,
                    Number = pt.Number,
                    MadeQuantity = pt.MadeQuantity,
                    EnumColor = (byte?)pt.EnumColor ?? 0 // Если 3, то как на СГБ розовым цветить будем(активные задания)
                }
                );
        }

        private void EditItem()
        {
            MessageManager.OpenProductionTask(BatchKinds.SGI, SelectedProductionTaskSGI.ProductionTaskBatchID, WorkSession.PlaceGroup == PlaceGroup.Other);
        }

        private void NewProductionTask()
        {
            MessageManager.NewProductionTask(BatchKinds.SGI);
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

        public DelegateCommand<object> EditItemCommand
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
            public byte EnumColor { get; set; }
        }        
    }
}