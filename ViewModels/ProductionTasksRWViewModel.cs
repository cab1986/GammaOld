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
            public decimal? MadeQuantity { get; set; }
            public string[] Format { get; set; }
            public string Place { get; set; }
            public DateTime? DateBegin { get; set; }
        }
        
        private void GetProductionTasks()
        {
            ProductionTasks = new ObservableCollection<ProductionTaskRW>();
            var tempCollection = new ObservableCollection<ProductionTaskRW>
                (
                    from pt in DB.GammaBase.GetProductionTasks((int)PlaceGroups.RW)
                    select new ProductionTaskRW
                    {
                        DateBegin = pt.DateBegin,
                        MadeQuantity = pt.MadeQuantity,
                        Nomenclature = pt.Nomenclature,
                        ProductionTaskID = pt.ProductionTaskID,
                        TaskQuantity = pt.Quantity,
                        Place = pt.Place,
                        Format = new string[16]
                    }
                ); 
            for (int i = 0; i < tempCollection.Count; i++)
            {
                var productionTaskID = tempCollection[i].ProductionTaskID;
                var cutting = DB.GammaBase.ProductionTaskRWCutting.Where(p => p.ProductionTaskID == productionTaskID).Select(p => p).ToList();
                var propsDescription = DB.GammaBase.GetCharPropsDescriptions(cutting[0].C1CCharacteristicID).FirstOrDefault();
                tempCollection[i].Nomenclature = String.Format("{0} {1} {2} {3} {4}", 
                    tempCollection[i].Nomenclature, propsDescription.CoreDiameter, 
                    propsDescription.Color, propsDescription.Diameter, propsDescription.Destination);
                for (int k = 0; k < cutting.Count(); k++)
                {
                    tempCollection[i].Format[k] = DB.GammaBase.GetCharSpoolFormat(cutting[k].C1CCharacteristicID).FirstOrDefault().ToString();
                }
                ProductionTasks.Add(tempCollection[i]);
            }
        }
    }
}