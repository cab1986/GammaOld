using GalaSoft.MvvmLight;
using System;
using Gamma.Interfaces;
using System.Linq;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTaskBatchSGBViewModel : SaveImplementedViewModel, IProductionTaskBatch
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskBatchSGBViewModel class.
        /// </summary>
        public ProductionTaskBatchSGBViewModel(Guid productionTaskBatchID)
        {
            ProductionTaskBatchID = productionTaskBatchID;
            var productionTaskBatch = DB.GammaBase.ProductionTaskBatches.Where(p => p.ProductionTaskBatchID == productionTaskBatchID).
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
        public override bool IsValid
        {
            get
            {
                return
                    (FirstView == null ? true : FirstView.IsValid) &&
                    (SecondView == null ? true : SecondView.IsValid) &&
//                    (ProductionTaskSGBView == null ? true : ProductionTaskSGBView.IsValid) &&
                    (ProductionTaskWRView == null ? true : ProductionTaskWRView.IsValid);
            }
        }
        private Guid ProductionTaskBatchID { get; set; }
        private short _processModelID;
        public short ProcessModelID
        {
            get
            {
                return _processModelID;
            }
            set
            {
                _processModelID = value;
                switch (ProcessModelID)
                {
                    case (byte)ProcessModels.PM:
                        if (SecondView is ProductionTaskPMViewModel) 
                        {
                            FirstView = SecondView;
                            (FirstView as ProductionTaskPMViewModel).IsForRW = false;
                        }
                        else if (!(FirstView is ProductionTaskPMViewModel))
                            FirstView = new ProductionTaskPMViewModel(ProductionTaskBatchID, false);
                        SecondView = null;
                        ProductionTaskWRView = null;
                        break;
                    case (byte)ProcessModels.PM_RW:
                        if (FirstView is ProductionTaskPMViewModel)
                        {
                            SecondView = FirstView;
                            (SecondView as ProductionTaskPMViewModel).IsForRW = true;
                        }
                        else if (!(SecondView is ProductionTaskPMViewModel))
                            SecondView = new ProductionTaskPMViewModel(ProductionTaskBatchID, true);
                        if (!(FirstView is ProductionTaskRWViewModel))
                            FirstView = new ProductionTaskRWViewModel(ProductionTaskBatchID);
                        ProductionTaskWRView = null;
                        break;
                    case (byte)ProcessModels.PM_RW_WR:
                        if (FirstView is ProductionTaskPMViewModel)
                        {
                            SecondView = FirstView;
                            (SecondView as ProductionTaskPMViewModel).IsForRW = true;
                        }
                        else if (!(SecondView is ProductionTaskPMViewModel))
                            SecondView = new ProductionTaskPMViewModel(ProductionTaskBatchID, true);
                        if (!(FirstView is ProductionTaskRWViewModel))
                            FirstView = new ProductionTaskRWViewModel(ProductionTaskBatchID);
                        ProductionTaskWRView = ProductionTaskWRView ?? new ProductionTaskWRViewModel(ProductionTaskBatchID);
                        break;
                    case (byte)ProcessModels.PM_WR:
                        if (SecondView is ProductionTaskPMViewModel) 
                        {
                            FirstView = SecondView;
                            (FirstView as ProductionTaskPMViewModel).IsForRW = false;
                        }
                        else if (!(FirstView is ProductionTaskPMViewModel))
                            FirstView = new ProductionTaskPMViewModel(ProductionTaskBatchID, false);
                        SecondView = null;
                        ProductionTaskWRView = ProductionTaskWRView ?? new ProductionTaskWRViewModel(ProductionTaskBatchID);
                        break;
                    default:
                        break;
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
        private SaveImplementedViewModel _productionTaskWRView;
        public SaveImplementedViewModel ProductionTaskWRView
        {
            get
            {
                return _productionTaskWRView;
            }
            set
            {
            	_productionTaskWRView = value;
                RaisePropertyChanged("ProductionTaskWRView");
            }
        }
        public override void SaveToModel(Guid itemID)
        {
            base.SaveToModel(itemID);
            if (FirstView != null) FirstView.SaveToModel(itemID);
            if (SecondView != null) SecondView.SaveToModel(itemID);
            var productionTaskID = (FirstView as IProductionTask).ProductionTaskID;
//            if (ProductionTaskSGBView != null) ProductionTaskSGBView.SaveToModel(productionTaskID);
            if (ProductionTaskWRView != null) ProductionTaskWRView.SaveToModel(productionTaskID);
        }
        
    }
}