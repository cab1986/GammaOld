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
            public String NomenclatureName { get; set; }
            public int TaskQuantity { get; set; }
            public ObservableCollection<ProductionTaskConfig> ProductionTaskConfiguration { get; set; }
        }
        public class ProductionTaskConfig
        {
            public string CharacteristicName { get; set; }
            public byte? DocProductsQuantity { get; set; }
        }
        private void GetProductionTasks()
        {
            ProductionTasks = new ObservableCollection<ProductionTaskRW>
            (
                from ptask in DB.GammaBase.ProductionTasks
                where ptask.ProductionTaskKindID == (short)ProductionTaskKinds.ProductionTaskRW
                select new ProductionTaskRW
                {
                    ProductionTaskID = ptask.ProductionTaskID
                }
            );
            foreach (var ptask in ProductionTasks)
            {
                ptask.ProductionTaskConfiguration = new ObservableCollection<ProductionTaskConfig>();
                var ptconfig = DB.GammaBase.ProductionTaskConfig
                    .Include("C1CCharacteristics")
                    .Where(ptc => ptc.ProductionTaskID == ptask.ProductionTaskID).Select(ptc => ptc);
                ptask.NomenclatureName = ptconfig.Select(p => p.C1CNomenclature.Name).FirstOrDefault();
                ptask.TaskQuantity = ptconfig.Select(p => p.TaskQuantity).FirstOrDefault();
                foreach (var ptc in ptconfig)
                {
                    ptask.ProductionTaskConfiguration.Add(new ProductionTaskConfig
                    {
                        CharacteristicName = ptc.C1CCharacteristics.Name,
                        DocProductsQuantity = ptc.DocProductsQuantity
                    });
                }
            }
        }
    }
}