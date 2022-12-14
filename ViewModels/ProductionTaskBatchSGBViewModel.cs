// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using Gamma.Interfaces;
using System.Linq;
using Gamma.Entities;

// ReSharper disable MemberCanBePrivate.Global

namespace Gamma.ViewModels
{
    /// <summary>
    /// Реализация пакета заданий СГБ
    /// </summary>
    public class ProductionTaskBatchSGBViewModel : SaveImplementedViewModel, IProductionTaskBatch
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskBatchSGBViewModel class.
        /// <param name="productionTaskBatchID">id пакета заданий СГБ</param>
        /// </summary>
        public ProductionTaskBatchSGBViewModel(Guid productionTaskBatchID)
        {
            ProductionTaskBatchID = productionTaskBatchID;
            var productionTaskBatch = GammaBase.ProductionTaskBatches.Where(p => p.ProductionTaskBatchID == productionTaskBatchID).
                Select(p => p).FirstOrDefault();
            if (productionTaskBatch == null)
            {
                ProcessModelID = 0;
//                ProductionTaskSGBView = new ProductionTaskSGBViewModel();
            }
            else
            {
                ProcessModelID = productionTaskBatch.ProcessModelID ?? 0;
//                ProductionTaskSGBView = new ProductionTaskSGBViewModel(ProductionTaskBatchID);
            }             
        }
        public override bool IsValid => (FirstView?.IsValid ?? true) &&
                                        (SecondView?.IsValid ?? true) &&
//                    (ProductionTaskSGBView == null ? true : ProductionTaskSGBView.IsValid) &&
                                        (ProductionTaskWrView?.IsValid ?? true);

        private Guid ProductionTaskBatchID { get; }
        private short _processModelid;
        public short ProcessModelID
        {
            get
            {
                return _processModelid;
            }
            set
            {
                _processModelid = value;
                bool isEditingComment = false;
                if (FirstView is IProductionTaskBatch)
                {
                    isEditingComment = (FirstView as IProductionTaskBatch).OnEditingStatus;
                }
                if (FirstView is IProductionTask)
                {
                    isEditingComment = (FirstView as IProductionTask).IsEditingQuantity;
                }

                switch (ProcessModelID)
                {
                    case (byte)ProcessModels.PM:
                        if (SecondView is ProductionTaskPMViewModel) 
                        {
                            FirstView = SecondView;
                            ((ProductionTaskPMViewModel) FirstView).IsForRw = false;
                        }
                        else if (!(FirstView is ProductionTaskPMViewModel))
                            FirstView = new ProductionTaskPMViewModel(ProductionTaskBatchID, false);
                        SecondView = null;
                        ProductionTaskWrView = null;
                        break;
                    case (byte)ProcessModels.PMRw:
                        if (FirstView is ProductionTaskPMViewModel)
                        {
                            SecondView = FirstView;
                            ((ProductionTaskPMViewModel) SecondView).IsForRw = true;
                        }
                        else if (!(SecondView is ProductionTaskPMViewModel))
                            SecondView = new ProductionTaskPMViewModel(ProductionTaskBatchID, true);
                        if (!(FirstView is ProductionTaskRwViewModel))
                            FirstView = new ProductionTaskRwViewModel(ProductionTaskBatchID);
                        ProductionTaskWrView = null;
                        break;
                    case (byte)ProcessModels.PMRwWr:
                        if (FirstView is ProductionTaskPMViewModel)
                        {
                            SecondView = FirstView;
                            ((ProductionTaskPMViewModel) SecondView).IsForRw = true;
                        }
                        else if (!(SecondView is ProductionTaskPMViewModel))
                            SecondView = new ProductionTaskPMViewModel(ProductionTaskBatchID, true);
                        if (!(FirstView is ProductionTaskRwViewModel))
                            FirstView = new ProductionTaskRwViewModel(ProductionTaskBatchID);
                        ProductionTaskWrView = ProductionTaskWrView ?? new ProductionTaskWrViewModel(ProductionTaskBatchID);
                        break;
                    case (byte)ProcessModels.PMWr:
                        if (SecondView is ProductionTaskPMViewModel) 
                        {
                            FirstView = SecondView;
                            ((ProductionTaskPMViewModel) FirstView).IsForRw = false;
                        }
                        else if (!(FirstView is ProductionTaskPMViewModel))
                            FirstView = new ProductionTaskPMViewModel(ProductionTaskBatchID, false);
                        SecondView = null;
                        ProductionTaskWrView = ProductionTaskWrView ?? new ProductionTaskWrViewModel(ProductionTaskBatchID);
                        break;
                    case (byte)ProcessModels.Rw:
                        if (!(FirstView is ProductionTaskRwViewModel))
                        {
                            FirstView = new ProductionTaskRwViewModel(ProductionTaskBatchID);
                        }
                        SecondView = null;
                        ProductionTaskWrView = null;
                        break;
                    case (byte)ProcessModels.RwWr:
                        if (!(FirstView is ProductionTaskRwViewModel))
                        {
                            FirstView = new ProductionTaskRwViewModel(ProductionTaskBatchID);
                        }
                        SecondView = null;
                        ProductionTaskWrView = ProductionTaskWrView ?? new ProductionTaskWrViewModel(ProductionTaskBatchID);
                        break;
                }
                if (FirstView is IProductionTaskBatch)
                {
                    (FirstView as IProductionTaskBatch).OnEditingStatus = isEditingComment;
                }
                if (FirstView is IProductionTask)
                {
                    (FirstView as IProductionTask).IsEditingQuantity = isEditingComment;
                }
                if (SecondView != null && SecondView is IProductionTaskBatch)
                {
                    (SecondView as IProductionTaskBatch).OnEditingStatus = isEditingComment;
                }
                if (SecondView != null && SecondView is IProductionTask)
                {
                    (SecondView as IProductionTask).IsEditingQuantity = isEditingComment;
                }
            }
        }

        private bool _onEditingStatus { get; set; }
        public bool OnEditingStatus 
        {
            get
            {
                return _onEditingStatus;
            }
            set
            {
                _onEditingStatus = value;
                if (FirstView != null &&  FirstView is IProductionTask)
                {
                    (FirstView as IProductionTask).IsEditingQuantity = value;
                }
                if (SecondView != null && SecondView is IProductionTask)
                {
                    (SecondView as IProductionTask).IsEditingQuantity = value;
                }
            }
        }

        private SaveImplementedViewModel _firstView;
        public SaveImplementedViewModel FirstView
        {
            get
            {
                return _firstView;
            }
            set
            {
            	_firstView = value;
                RaisePropertyChanged("FirstView");
            }
        }
/*        private SaveImplementedViewModel _productionTaskSGBView;
        public SaveImplementedViewModel ProductionTaskSGBView
        {
            get
            {
                return _productionTaskSGBView;
            }
            set
            {
            	_productionTaskSGBView = value;
                RaisePropertyChanged("ProductionTaskSGBView");
            }
        }
 * */
        private SaveImplementedViewModel _secondView;
        public SaveImplementedViewModel SecondView
        {
            get
            {
                return _secondView;
            }
            set
            {
            	_secondView = value;
                RaisePropertyChanged("SecondView");
            }
        }
        private SaveImplementedViewModel _productionTaskWrView;
        public SaveImplementedViewModel ProductionTaskWrView
        {
            get
            {
                return _productionTaskWrView;
            }
            set
            {
            	_productionTaskWrView = value;
                RaisePropertyChanged("ProductionTaskWrView");
            }
        }
        public override bool SaveToModel(Guid itemID)
        {
            FirstView?.SaveToModel(itemID);
            SecondView?.SaveToModel(itemID);
            var productionTask = FirstView as IProductionTask;
            if (productionTask == null) return true;
            var productionTaskID = productionTask.ProductionTaskID;
//            if (ProductionTaskSGBView != null) ProductionTaskSGBView.SaveToModel(productionTaskID);
            ProductionTaskWrView?.SaveToModel(productionTaskID);
            return true;
        }

        public override bool CanSaveExecute()
        {
            return base.CanSaveExecute() && (FirstView?.CanSaveExecute() ?? true) &&
                   (SecondView?.CanSaveExecute() ?? true)
                   && (ProductionTaskWrView?.CanSaveExecute() ?? true);
        }
        
    }
}