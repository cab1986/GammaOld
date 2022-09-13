// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using DevExpress.Mvvm;
using Gamma.Interfaces;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using Gamma.Common;
using Gamma.Entities;
using System.Collections.Generic;
using System.Windows;

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
            IsEnabledProductionTaskStates = WorkSession.ShiftID == 0;
            ProductionTaskStates = new ProductionTaskStates().ToDictionary();
            ProductionTaskStateID = 1;
            //GetProductionTasks();
            EditItemCommand = new DelegateCommand(EditItem, SelectedProductionTaskSGI != null);
            NewItemCommand = new DelegateCommand(NewProductionTask);
            RefreshCommand = new DelegateCommand(GetProductionTasks);
            DeleteItemCommand = new DelegateCommand(DeleteProductionTask, () => false);
            CopyProductionTaskCommand = new DelegateCommand(CopyProductionTask);
        }

        public Dictionary<byte, string> ProductionTaskStates { get; set; }
        private byte? _productionTaskStateID { get; set; }
        public byte? ProductionTaskStateID
        {
            get { return _productionTaskStateID; }
            set
            {
                if (_productionTaskStateID == value) return;
                _productionTaskStateID = value< 0 ? 0 : value;
                if (_productionTaskStateID < 3) GetProductionTasks();
            }
        }

        public bool IsEnabledProductionTaskStates { get; set; }

        private void DeleteProductionTask()
        {
            if (!WorkSession.CheckExistNewVersionOfProgram())
            {
                if (SelectedProductionTaskSGI == null) return;
                //DB.GammaDb.DeleteProductionTaskBatch(SelectedProductionTaskSGI.ProductionTaskBatchID);
                var delResult = GammaBase.DeleteProductionTaskBatch(SelectedProductionTaskSGI.ProductionTaskBatchID).FirstOrDefault();
                if (delResult != "")
                {
                    MessageBox.Show(delResult, "Удалить не удалось", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
                else
                    ProductionTasks.Remove(SelectedProductionTaskSGI);
            }
        }

        private void GetProductionTasks()
        {
            if (!WorkSession.CheckExistNewVersionOfProgram())
            {
                UIServices.SetBusyState();
                ProductionTasks = new ObservableCollection<ProductionTaskSGI>(
                    from pt in DB.GammaDbWithNoCheckConnection.GetProductionTasksOnState((int)BatchKinds.SGI, ProductionTaskStateID)
                    select new ProductionTaskSGI()
                    {
                        ProductionTaskBatchID = pt.ProductionTaskBatchID,
                        Quantity = pt.Quantity,
                        DateBegin = pt.DateBegin,
                        Nomenclature = pt.Nomenclature + "\r\n" + pt.Characteristic,
                        Place = pt.Place,
                        Number = pt.Number,
                        Comment = pt.Comment,
                        CommentPlus = (pt.Comment == null) ? null : "!",
                        MadeQuantity = pt.MadeQuantity,
                        EnumColor = (byte?)pt.EnumColor ?? 0 // Если 3, то как на СГБ розовым цветить будем(активные задания)
                    }
                    );

            }
        }

        private void EditItem()
        {
            WorkSession.CheckExistNewVersionOfProgram();
            MessageManager.OpenProductionTask(BatchKinds.SGI, SelectedProductionTaskSGI.ProductionTaskBatchID, WorkSession.PlaceGroup == PlaceGroup.Other);
        }

        private void NewProductionTask()
        {
            WorkSession.CheckExistNewVersionOfProgram();
            MessageManager.NewProductionTask(BatchKinds.SGI);
        }

        private void CopyProductionTask()
        {
            if (!WorkSession.CheckExistNewVersionOfProgram())
            {
                //Create new Task from selected Task
                using (var gammaBase = DB.GammaDbWithNoCheckConnection)
                {
                    Guid? newProductionTaskBatchID = gammaBase.CreateNewTaskBatchOneBased(SelectedProductionTaskSGI.ProductionTaskBatchID).FirstOrDefault();

                    if (newProductionTaskBatchID != null && newProductionTaskBatchID != Guid.Empty)
                        MessageManager.OpenProductionTask(BatchKinds.SGI, (Guid)newProductionTaskBatchID, WorkSession.PlaceGroup == PlaceGroup.Other);
                    else
                    {
                        string errorText = "Ошибка при копировании задания. Новое задание не создано.";
                        MessageBox.Show(errorText, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        DB.AddLogMessageError(errorText);
                    }
                }
            }
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

        public DelegateCommand CopyProductionTaskCommand { get; private set; }

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
            public string Comment { get; set; }
            public string CommentPlus { get; set; }
        }        
    }
}