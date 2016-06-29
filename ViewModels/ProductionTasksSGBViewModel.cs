using Gamma.Interfaces;
using DevExpress.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using Gamma.Common;
using Gamma.Models;

namespace Gamma.ViewModels
{
    /// <summary>
    /// ViewModel для грида заданий СГБ
    /// </summary>
    public class ProductionTasksSGBViewModel : RootViewModel,IItemManager
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTasksRWViewModel class.
        /// </summary>
        public ProductionTasksSGBViewModel(GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb;
            NewItemCommand = new DelegateCommand(NewItem);
            EditItemCommand = new DelegateCommand(EditItem,() => SelectedProductionTaskBatch != null);
            DeleteItemCommand = new DelegateCommand(DeleteItem);
            RefreshCommand = new DelegateCommand(Refresh);
            GetProductionTasks();
        }

        public DelegateCommand NewItemCommand {get; set;}
        public DelegateCommand<object> EditItemCommand {get; set;}
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
            if (SelectedProductionTaskBatch == null) return;
            var delResult = GammaBase.DeleteProductionTaskBatch(SelectedProductionTaskBatch.ProductionTaskBatchID).First();
            if (string.IsNullOrEmpty(delResult)) return;
            MessageBox.Show(delResult, "Не удалось удалить", MessageBoxButton.OK, MessageBoxImage.Information);
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

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
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
            public string[] NomenclatureKind { get; set; }
        }
        
        private void GetProductionTasks()
        {
            UIServices.SetBusyState();
            ProductionTaskBatchesSGB = new ObservableCollection<ProductionTaskBatchSGB>();
            var tempCollection = new ObservableCollection<ProductionTaskBatchSGB>
                (
                    from pt in GammaBase.GetProductionTasks((int)BatchKinds.SGB)
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
                        NomenclatureKind = new string[16],
                        Number = pt.Number
                    }
                ); 
            foreach (var t in tempCollection)
            {
                var productionTaskBatchID = t.ProductionTaskBatchID;
                var cuttingList = GammaBase.GetProductionTaskBatchSGBCuttings(productionTaskBatchID).ToList();
                if (cuttingList.Count == 0)
                {
                    MessageBox.Show($"Ошибка при получении информации о задании(id: {productionTaskBatchID})");
                    continue;
                }
                var cutting = cuttingList[0];
                t.Nomenclature =
                    $"{t.Nomenclature} \r\n{cutting.CoreDiameter} {cutting.LayerNumber} {cutting.Color} {cutting.Destination}";
                t.TotalFormat = 0;
                for (int k = 0; k < cuttingList.Count(); k++)
                {
                    t.Format[k] = cuttingList[k].FormatNumeric.ToString();
                    t.NomenclatureKind[k] = cuttingList[k].NomenclatureKind;
                    t.TotalFormat += cuttingList[k].FormatNumeric ?? 0;
                }
                ProductionTaskBatchesSGB.Add(t);
            }
        }
    }
}