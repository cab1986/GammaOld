using GalaSoft.MvvmLight;
using System.Windows.Documents;
using System.Linq;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using Gamma.Interfaces;
using GalaSoft.MvvmLight.Messaging;
using System.ComponentModel;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTasksPMViewModel : RootViewModel,IItemManager
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTasksPMViewModel class.
        /// </summary>
        public ProductionTasksPMViewModel()
        {
            GetProductionTasks();
            EditItemCommand = new RelayCommand(EditItem);
            NewItemCommand = new RelayCommand(NewProductionTaskPM);
            RefreshCommand = new RelayCommand(GetProductionTasks);
        }

        private void GetProductionTasks()
        {
            ProductionTasks = new ObservableCollection<ProductionTask>
                              (from pt in DB.GammaBase.ProductionTasks
                               join ptconf in DB.GammaBase.ProductionTaskConfig on pt.ProductionTaskID equals ptconf.ProductionTaskID
                               join ch in DB.GammaBase.C1CCharacteristics on ptconf.C1CCharacteristicID equals ch.C1CCharacteristicID
                               select new ProductionTask
                               {
                                   ProductionTaskID = pt.ProductionTaskID,
                                   DateBegin = pt.DateBegin,
                                   Nomenclature = ptconf.C1CNomenclature.Name,
                                   Quantity = ptconf.TaskQuantity
                               });
        }

        private ObservableCollection<ProductionTask> _productiontasks;
        public ObservableCollection<ProductionTask> ProductionTasks
        {
            get { return _productiontasks; }
            private set { 
                    _productiontasks = value;
                    RaisePropertyChanged("ProductionTasks");
                }
        }

        public class ProductionTask : ViewModelBase
        {
            private Guid _productionTaskID;
            public Guid ProductionTaskID 
            {
                get
                {
                    return _productionTaskID;
                }
                set {
                        _productionTaskID = value;
                        RaisePropertyChanged("ProductionTaskID");
                    } 
            }
            private DateTime? _dateBegin;
            public DateTime? DateBegin 
            {
                get { return _dateBegin; } 
                set
                {
                	_dateBegin = value;
                    RaisePropertyChanged("DateBegin");
                }
            }
            private string _nomenclature;
            public string Nomenclature 
            {
                get { return _nomenclature; }
                set
                {
                	_nomenclature = value;
                    RaisePropertyChanged("Nomenclature");
                }
            }
            private int? _quantity;
            public int? Quantity 
            {
                get { return _quantity; }
                set
                {
                    _quantity = value;
                    RaisePropertyChanged("Quantity");
                }
            }
        }

        private void EditItem()
        {
            OpenProductionTaskMessage msg = new OpenProductionTaskMessage { ProductionTaskID = SelectedProductionTask.ProductionTaskID, ProductionTaskKind = ProductionTaskKinds.ProductionTaskPM };
            MessageManager.OpenProductionTask(msg);
        }

        private void NewProductionTaskPM()
        {
            var msg = new OpenProductionTaskMessage {ProductionTaskKind = ProductionTaskKinds.ProductionTaskPM};
            Messenger.Default.Send<OpenProductionTaskMessage>(msg);
        }
        private ProductionTask _selectedProductionTask;
        


        public RelayCommand NewItemCommand
        {
            get;
            set;
        }

        public RelayCommand EditItemCommand
        {
            get;
            set;
        }

        public RelayCommand DeleteItemCommand
        {
            get;
            set;
        }
        public RelayCommand RefreshCommand
        {
            get;
            set;
        }
        public ProductionTask SelectedProductionTask
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
    }
}