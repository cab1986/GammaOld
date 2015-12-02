using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
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
    public class FindProductionTaskViewModel : RootViewModel
    {
        /// <summary>
        /// Initializes a new instance of the FindProductionTaskViewModel class.
        /// </summary>
        public FindProductionTaskViewModel(PlaceGroups placeGroup)
        {
            ProductionTaskStates = new ProductionTaskStates().ToDictionary();
            PlaceGroup = placeGroup;
            switch (placeGroup)
            {
                case PlaceGroups.Convertings:
                    Title = "Задания на СГИ";
                    break;
                case PlaceGroups.PM:
                    Title = "Задания на БДМ";
                    break;
                case PlaceGroups.RW:
                    Title = "Задания на ПРС";
                    break;
            }
            FindProductionTaskCommand = new RelayCommand(FindProductionTask);
            OpenProductionTaskCommand = new RelayCommand(() => MessageManager.OpenProductionTask(new OpenProductionTaskMessage()
                {
                    ProductionTaskID = SelectedProductionTask.ProductionTaskID,
                    ProductionTaskKind = (ProductionTaskKinds)SelectedProductionTask.ProductionTaskKindID
                })
                    , () => SelectedProductionTask != null);
        }

        private void FindProductionTask()
        {
            ProductionTasks = new ObservableCollection<ProductionTask>
            (
                from pt in DB.GammaBase.ProductionTasks where 
                    pt.Places.PlaceGroupID == (short)PlaceGroup &&
                    (DateBegin == null || pt.DateBegin >= DateBegin) &&
                    (DateEnd == null || pt.DateBegin <= DateEnd) &&
                    (Number == null || Number == "" || Number == pt.Number) &&
                    (ProductionTaskStateID == null || ProductionTaskStateID == pt.ProductionTaskStateID)
                select new ProductionTask() 
                { 
                    ProductionTaskID = pt.ProductionTaskID,
                    ProductionTaskKindID = (byte)pt.ProductionTaskKindID,
                    Number = pt.Number,
                    State = pt.ProductionTaskStates.Name,
                    Date = pt.Date,
                    DateBegin = pt.DateBegin,
                    Quantity = pt.Quantity,
                    Nomenclature = pt.C1CCharacteristics.Name != null ? pt.C1CNomenclature.Name + " " + pt.C1CCharacteristics.Name : pt.C1CNomenclature.Name,
                    PlaceID = (int)pt.PlaceID,
                    Place = pt.Places.Name
                }
            );
        }
        private PlaceGroups PlaceGroup { get; set; }
        public RelayCommand FindProductionTaskCommand { get; set; }
        public string Number { get; set; }
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public Dictionary<byte, string> ProductionTaskStates { get; set; }
        public byte? ProductionTaskStateID { get; set; }
        private ObservableCollection<ProductionTask> _productionTasks;
        public ObservableCollection<ProductionTask> ProductionTasks
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
        public string Title { get; set; }
        public ProductionTask SelectedProductionTask { get; set; }
        public RelayCommand OpenProductionTaskCommand { get; private set; }
    }
}