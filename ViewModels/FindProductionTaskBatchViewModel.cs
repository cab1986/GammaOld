﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Gamma.Common;

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
        public FindProductionTaskBatchViewModel(BatchKinds batchKind)
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
            FindProductionTaskBatchCommand = new RelayCommand(FindProductionTaskBatch);
            OpenProductionTaskBatchCommand = new RelayCommand(() => MessageManager.OpenProductionTask(new OpenProductionTaskBatchMessage()
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
                from pt in DB.GammaBase.FindProductionTasks((int)BatchKind, ProductionTaskStateID,
                    DateBegin, DateEnd)
                select new ProductionTaskBatch() 
                { 
                    ProductionTaskBatchID = pt.ProductionTaskBatchID,
                    BatchKindID = (byte)pt.BatchKindID, 
                    Number = pt.Number,
                    State = pt.ProductionTaskState,
                    Date = pt.Date,
                    DateBegin = pt.DateBegin,
                    Quantity = pt.Quantity,
                    Nomenclature = pt.Nomenclature
                }
            );
        }
        private BatchKinds BatchKind { get; set; }
        public RelayCommand FindProductionTaskBatchCommand { get; set; }
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
        public RelayCommand OpenProductionTaskBatchCommand { get; private set; }
    }
}