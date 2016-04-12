using Gamma.Interfaces;
using DevExpress.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Gamma.Common;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTasksSGBViewModel : RootViewModel,IItemManager
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTasksRWViewModel class.
        /// </summary>
        public ProductionTasksSGBViewModel()
        {
            NewItemCommand = new DelegateCommand(NewItem);
            EditItemCommand = new DelegateCommand(EditItem,() => SelectedProductionTaskBatch != null);
            DeleteItemCommand = new DelegateCommand(DeleteItem);
            RefreshCommand = new DelegateCommand(Refresh);
            GetProductionTasks();
        }

        public DelegateCommand NewItemCommand {get; set;}
        public DelegateCommand EditItemCommand {get; set;}
        public DelegateCommand DeleteItemCommand {get; set;}
        public DelegateCommand RefreshCommand { get; set; }
        private void NewItem()
        {
            MessageManager.OpenProductionTask(new OpenProductionTaskBatchMessage 
            { 
                BatchKind = BatchKinds.SGB
            });
        }
        private void EditItem()
        {
            MessageManager.OpenProductionTask(new OpenProductionTaskBatchMessage
                {
                    ProductionTaskBatchID = SelectedProductionTaskBatch.ProductionTaskBatchID,
                    BatchKind = BatchKinds.SGB
                });
        }
        private void DeleteItem()
        {

        }
        private void Refresh()
        {
            GetProductionTasks();
        }
        private ProductionTaskBatchSGB _selectedProductionTaskBatch;
        public ProductionTaskBatchSGB SelectedProductionTaskBatch
        {
            get
            {
                return _selectedProductionTaskBatch;
            }
            set
            {
                _selectedProductionTaskBatch = value;
                RaisePropertyChanged("SelectedProductionTaskBatch");
            }
        }
        private ObservableCollection<ProductionTaskBatchSGB> _productionTaskBatchesSGB;
        public ObservableCollection<ProductionTaskBatchSGB> ProductionTaskBatchesSGB
        {
            get
            {
                return _productionTaskBatchesSGB;
            }
            set
            {
                _productionTaskBatchesSGB = value;
                RaisePropertyChanged("ProductionTaskBatchesSGB");
            }
        }
        public class ProductionTaskBatchSGB
        {
            public Guid ProductionTaskBatchID { get; set; }
            public string Nomenclature { get; set; }
            public decimal? TaskQuantity { get; set; }
            public string MadeQuantity { get; set; }
            public string[] Format { get; set; }
            public int TotalFormat { get; set; }
            public string Place { get; set; }
            public DateTime? DateBegin { get; set; }
            public byte EnumColor { get; set; }
            public string Number { get; set; }
        }
        
        private void GetProductionTasks()
        {
            UIServices.SetBusyState();
            ProductionTaskBatchesSGB = new ObservableCollection<ProductionTaskBatchSGB>();
            var tempCollection = new ObservableCollection<ProductionTaskBatchSGB>
                (
                    from pt in DB.GammaBase.GetProductionTasks((int)BatchKinds.SGB)
                    select new ProductionTaskBatchSGB
                    {
                        DateBegin = pt.DateBegin,
                        MadeQuantity = pt.MadeQuantity,
                        Nomenclature = pt.Nomenclature,
                        ProductionTaskBatchID = pt.ProductionTaskBatchID,
                        TaskQuantity = pt.Quantity,
                        Place = pt.Place,
                        EnumColor = (byte?)pt.EnumColor ?? 0,
                        Format = new string[16],
                        Number = pt.Number
                    }
                ); 
            for (int i = 0; i < tempCollection.Count; i++)
            {
                var productionTaskBatchId = tempCollection[i].ProductionTaskBatchID;
                var cuttingList = DB.GammaBase.GetProductionTaskBatchSGBCuttings(productionTaskBatchId).ToList();
                if (cuttingList.Count == 0)
                {
                    MessageBox.Show($"Ошибка при получении информации о задании(ID: {productionTaskBatchId})");
                    continue;
                }
                var cutting = cuttingList[0];
                tempCollection[i].Nomenclature =
                    $"{tempCollection[i].Nomenclature} \r\n{cutting.CoreDiameter} {cutting.LayerNumber} {cutting.Color} {cutting.Destination}";
                tempCollection[i].TotalFormat = 0;
                for (int k = 0; k < cuttingList.Count(); k++)
                {
                    tempCollection[i].Format[k] = cuttingList[k].FormatNumeric.ToString();
                    tempCollection[i].TotalFormat += cuttingList[k].FormatNumeric ?? 0;
                }
                ProductionTaskBatchesSGB.Add(tempCollection[i]);
            }
        }
    }
}