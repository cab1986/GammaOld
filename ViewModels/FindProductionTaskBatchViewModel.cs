// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Gamma.Entities;
using Gamma.Models;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class FindProductionTaskBatchViewModel : RootViewModel
    {
        /// <summary>
        /// Initializes a new instance of the FindProductionTaskViewModel class.
        /// </summary>
        public FindProductionTaskBatchViewModel(BatchKinds batchKind, GammaEntities gammaBase = null) : base(gammaBase)
        {
            ProductionTaskStates = new ProductionTaskStates().ToDictionary();
            BatchKind = batchKind;
            switch (batchKind)
            {
                case BatchKinds.SGI:
                    Title = "Задания на СГИ";
                    break;
                case BatchKinds.SGB:
                    Title = "Задания на БДМ";
                    break;
            }
            FindProductionTaskBatchCommand = new DelegateCommand(FindProductionTaskBatch);
            OpenProductionTaskBatchCommand = new DelegateCommand(() => MessageManager.OpenProductionTask(new OpenProductionTaskBatchMessage()
                {
                    ProductionTaskBatchID = SelectedProductionTaskBatch.ProductionTaskBatchID,
                    BatchKind = (BatchKinds)SelectedProductionTaskBatch.BatchKindID
                })
                    , () => SelectedProductionTaskBatch != null);
        }



        private void FindProductionTaskBatch()
        {
            ProductionTaskBatches = new ObservableCollection<ProductionTaskBatch>
            (
                from pt in GammaBase.FindProductionTasks((int)BatchKind, ProductionTaskStateID,
                    DateBegin, DateEnd, Number)
                select new ProductionTaskBatch() 
                { 
                    ProductionTaskBatchID = pt.ProductionTaskBatchID,
                    BatchKindID = (byte)pt.BatchKindID, 
                    Number = pt.Number,
                    State = pt.ProductionTaskState,
                    Date = pt.Date,
                    DateBegin = pt.DateBegin,
                    Quantity = pt.Quantity,
                    Nomenclature = pt.Nomenclature,
                    Place = pt.Place
                }
            );
        }
        private BatchKinds BatchKind { get; set; }
        public DelegateCommand FindProductionTaskBatchCommand { get; set; }
        public string Number { get; set; }
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public Dictionary<byte, string> ProductionTaskStates { get; set; }
        public byte? ProductionTaskStateID { get; set; }
        private ObservableCollection<ProductionTaskBatch> _productionTasks;
        public ObservableCollection<ProductionTaskBatch> ProductionTaskBatches
        {
            get
            {
                return _productionTasks;
            }
            set
            {
            	_productionTasks = value;
                RaisePropertyChanged("ProductionTaskBatches");
            }
        }
        public string Title { get; set; }
        public ProductionTaskBatch SelectedProductionTaskBatch { get; set; }
        public DelegateCommand OpenProductionTaskBatchCommand { get; private set; }
    }
}