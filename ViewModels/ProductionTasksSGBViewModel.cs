using Gamma.Interfaces;
using GalaSoft.MvvmLight.Command;
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
            NewItemCommand = new RelayCommand(NewItem);
            EditItemCommand = new RelayCommand(EditItem,() => SelectedProductionTaskBatch != null);
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
                        Format = new string[16]
                    }
                ); 
            for (int i = 0; i < tempCollection.Count; i++)
            {
                var productionTaskBatchID = tempCollection[i].ProductionTaskBatchID;
                var cuttingList = DB.GammaBase.GetProductionTaskBatchSGBCuttings(productionTaskBatchID).ToList();
                if (cuttingList.Count == 0)
                {
                    MessageBox.Show(String.Format("Ошибка при получении информации о задании(ID: {0})", productionTaskBatchID));
                    continue;
                }
                var cutting = cuttingList[0];
                tempCollection[i].Nomenclature = String.Format("{0} \r\n{1} {2} {3} {4}", 
                    tempCollection[i].Nomenclature, cutting.CoreDiameter, cutting.LayerNumber,  
                    cutting.Color, cutting.Destination);
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