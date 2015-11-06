using Gamma.Interfaces;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTasksRWViewModel : RootViewModel,IItemManager
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTasksRWViewModel class.
        /// </summary>
        public ProductionTasksRWViewModel()
        {
            NewItemCommand = new RelayCommand(NewItem);
            EditItemCommand = new RelayCommand(EditItem,() => SelectedProductionTask != null);
            DeleteItemCommand = new RelayCommand(DeleteItem);
            RefreshCommand = new RelayCommand(Refresh);
            GetProductionTasks();
        }

        public RelayCommand NewItemCommand {get; set;}
        public RelayCommand EditItemCommand {get; set;}
        public RelayCommand DeleteItemCommand {get; set;}
        public RelayCommand RefreshCommand { get; set; }
        private void NewItem()
        {
            MessageManager.OpenProductionTask(new OpenProductionTaskMessage 
            { 
                ProductionTaskKind = ProductionTaskKinds.ProductionTaskRW
            });
        }
        private void EditItem()
        {
            MessageManager.OpenProductionTask(new OpenProductionTaskMessage
                {
                    ProductionTaskID = SelectedProductionTask.ProductionTaskID,
                    ProductionTaskKind = ProductionTaskKinds.ProductionTaskRW
                });
        }
        private void DeleteItem()
        {

        }
        private void Refresh()
        {
            GetProductionTasks();
        }
        private ProductionTaskRW _selectedProductionTask;
        public ProductionTaskRW SelectedProductionTask
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
        private ObservableCollection<ProductionTaskRW> _productionTasks;
        public ObservableCollection<ProductionTaskRW> ProductionTasks
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
        public class ProductionTaskRW
        {
            public Guid ProductionTaskID { get; set; }
            public String Nomenclature { get; set; }
            public decimal? TaskQuantity { get; set; }
            public int MadeQuantity { get; set; }
            public string[] Format { get; set; }
            public string Place { get; set; }
            public DateTime? DateBegin { get; set; }
        }
        
        private void GetProductionTasks()
        {
            ProductionTasks = new ObservableCollection<ProductionTaskRW>();
            var productionTaskInfo = DB.GammaBase.ProductionTasks.Include("C1CNomenclature").Include("ProductionTaskRWCutting").
                Include("Places").Where(pt => pt.ProductionTaskKindID == (short)ProductionTaskKinds.ProductionTaskRW).Select(p => p);
            foreach (var info in productionTaskInfo)
            {
                var prodTaskRW = new ProductionTaskRW() 
                { 
                    DateBegin = info.DateBegin,
                    TaskQuantity = info.Quantity,
                    ProductionTaskID = info.ProductionTaskID,
                    Place = info.Places.Name,
                    Nomenclature = info.C1CNomenclature.Name,
                    Format = new string[15] 
                };
                var cutting = info.ProductionTaskRWCutting.ToArray();
                var propsDescription = DB.GammaBase.GetCharPropsDescriptions(cutting[0].C1CCharacteristicID).FirstOrDefault();
                prodTaskRW.Nomenclature = String.Format("{0} {1} {2} {3} {4}", 
                    prodTaskRW.Nomenclature, propsDescription.CoreDiameter, 
                    propsDescription.Color, propsDescription.Diameter, propsDescription.Destination);
                for (int i = 0; i < cutting.Count(); i++)
                {
                    prodTaskRW.Format[i] = DB.GammaBase.GetCharSpoolFormat(cutting[i].C1CCharacteristicID).FirstOrDefault().ToString();
                }
                ProductionTasks.Add(prodTaskRW);
            }
            
            foreach (var ptask in ProductionTasks)
            {
                
            }
        }
    }
}