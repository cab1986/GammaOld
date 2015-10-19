using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.ComponentModel.DataAnnotations;
using System;
using Gamma.Interfaces;
using System.Linq;
using Gamma.Models;
using System.Collections.ObjectModel;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTaskViewModel : DataBaseEditViewModel, ICheckedAccess
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskViewModel class.
        /// </summary>
        public ProductionTaskViewModel(OpenProductionTaskMessage msg)
        {
            RefreshProductionCommand = new RelayCommand(RefreshProduction);
            ShowProductCommand = new RelayCommand(ShowProduct,() => SelectedProductionTaskProduct != null);
            switch (msg.ProductionTaskKind)
            {
                case ProductionTaskKinds.ProductionTaskPM:
                    Places = DB.GetPlaces(PlaceGroups.PM);
                    break;
                case ProductionTaskKinds.ProductionTaskRW:
                    Places = DB.GetPlaces(PlaceGroups.RW);
                    break;
                default:
                    break;
            }
            if (msg.ProductionTaskID == null)
            {
                Date = DB.CurrentDateTime;
            }
            else
            {
                GetProductionTaskInfo((Guid)msg.ProductionTaskID);
                RefreshProduction();
            }
            ProductionTaskKind = msg.ProductionTaskKind;
            CreateNewProductCommand = new RelayCommand(CreateNewProduct,
                () => IsActual);
        }
        private DataBaseEditViewModel currentview;
        public DataBaseEditViewModel CurrentView
        {
            get { return currentview; }
            private set 
            { 
                currentview = value;
                RaisePropertyChanged("CurrentView");
            }
        }

        private DateTime date;
        [Required(ErrorMessage="Поле не может быть пустым")]
        public DateTime Date
        {
            get { return date; }
            set 
            { 
                date = value;
                RaisePropertyChanged("Date");
            }
        }

        private string number;
        private bool IsActual
        {
            get
            {
                return _isActual;
            }
            set
            {
                _isActual = value;
            }
        }
        public string Number
        {
            get { return number; }
            set 
            { 
                number = value;
                RaisePropertyChanged("Number");
            }
        }
        [Required(ErrorMessage = "Поле не может быть пустым")]
        public DateTime? DateBegin { get; set; }
        [Required(ErrorMessage = "Поле не может быть пустым")]
        public DateTime? DateEnd { get; set; }
        public string Comment { get; set; }
        public RelayCommand CreateNewProductCommand { get; private set; } 

        public bool IsReadOnly
        {
            get;
            set;
        }

        public bool CheckAccess()
        {
            return true;
        }

        private ProductionTaskKinds _productionTaskKind;

        public ObservableCollection<Place> Places
        {
            get
            {
                return _places;
            }
            set
            {
                _places = value;
                RaisePropertyChanged("Places");
            }
        }
        public ProductionTaskKinds ProductionTaskKind
        {
            get { return _productionTaskKind; }
            set 
            { 
                _productionTaskKind = value;
                ChangeCurrentView(_productionTaskKind);
            }
        }
        private Guid _productionTaskID;
        public Guid ProductionTaskID
        {
            get { return _productionTaskID; }
            set
            {
                if (value != null)
                {
                    _productionTaskID = value;
                }
            }
        }
  

        private void GetProductionTaskInfo(Guid ProductionTaskID)
        {
            var ProductionTask = (from pt in DB.GammaBase.ProductionTasks
                                  where pt.ProductionTaskID == ProductionTaskID
                                  select pt).FirstOrDefault();
            this.ProductionTaskID = ProductionTaskID;
            Number = ProductionTask.Number;
            Date = ProductionTask.Date;
            DateBegin = ProductionTask.DateBegin;
            DateEnd = ProductionTask.DateEnd;
            Comment = ProductionTask.Comment;
            SelectedPlace = Places.Where(place => place.PlaceID == ProductionTask.PlaceID).FirstOrDefault();
            if (ProductionTask.ProductionTaskStates != null)
                IsActual = ProductionTask.ProductionTaskStates.IsActual;
        }
        private void ChangeCurrentView(ProductionTaskKinds ProductionTaskKind)
        {
            switch (ProductionTaskKind)
            {
                case ProductionTaskKinds.ProductionTaskPM:
                    if (ProductionTaskID != new Guid())
                    {
                        CurrentView = new ProductionTaskPMViewModel(ProductionTaskID);
                    }
                    else CurrentView = new ProductionTaskPMViewModel();
                    break;
                case ProductionTaskKinds.ProductionTaskRW:
                    if (ProductionTaskID != new Guid())
                    {
                        CurrentView = new ProductionTaskRWViewModel(ProductionTaskID);
                    }
                    else CurrentView = new ProductionTaskRWViewModel();
                    break;
                default:
                    break;
            }
        }
   
        public override void SaveToModel()
        {
            base.SaveToModel();
            if (ProductionTaskID == new Guid())
            {
                ProductionTaskID = Guid.NewGuid();
                var ProductionTask = new ProductionTasks();
                SetProductionTaskProperties(ProductionTask);
                ProductionTask.ProductionTaskID = (Guid)ProductionTaskID;
                DB.GammaBase.ProductionTasks.Add(ProductionTask);
            }
            else
            {
                var ProductionTask = DB.GammaBase.ProductionTasks.Find(ProductionTaskID);
                SetProductionTaskProperties(ProductionTask);
            }
            CurrentView.SaveToModel(ProductionTaskID);
            DB.GammaBase.SaveChanges();
        }

        private void SetProductionTaskProperties(ProductionTasks ProductionTask)
        {
                    ProductionTask.Date = Date;
                    ProductionTask.DateBegin = DateBegin;
                    ProductionTask.DateEnd = DateEnd;
                    ProductionTask.Comment = Comment;
                    ProductionTask.PlaceID = SelectedPlace.PlaceID;
                    ProductionTask.Number = "abcdef";
                    ProductionTask.ProductionTaskKindID = (short)ProductionTaskKind;
        }
        private void CreateNewProduct()
        {
            var msg = new OpenDocProductMessage();
            switch (ProductionTaskKind)
            {
                case ProductionTaskKinds.ProductionTaskPM:
                    msg = new OpenDocProductMessage { DocProductKind = DocProductKinds.DocProductSpool, ID = ProductionTaskID, IsNewProduct = true };
                    break;
                case ProductionTaskKinds.ProductionTaskRW:
                    msg = new OpenDocProductMessage
                    {
                        DocProductKind = DocProductKinds.DocProductUnload,
                        ID = ProductionTaskID,
                        IsNewProduct = true
                    };
                    break;
                default:
                    break;
            }
            MessageManager.OpenDocProduct(msg);
        }
        private bool _isActual = true;
        private ObservableCollection<Place> _places = DB.GetPlaces(PlaceGroups.PM);
        private Place _selectedPlace;
        [Required(ErrorMessage="Необходимо выбрать передел")]
        public Place SelectedPlace
        {
            get
            {
                return _selectedPlace;
            }
            set
            {
                _selectedPlace = value;
                RaisePropertyChanged("SelectedPlace");
            }
        }
        protected override bool canExecute()
        {
            return base.canExecute() && CurrentView.IsValid;
        }
        public RelayCommand RefreshProductionCommand { get; private set; }
        public RelayCommand ShowProductCommand { get; private set; }
        private ProductInfo _selectedProductionTaskProduct;
        public ProductInfo SelectedProductionTaskProduct
        {
            get
            {
            	return _selectedProductionTaskProduct;
            }
            set
            {
                _selectedProductionTaskProduct = value;
                RaisePropertyChanged("SelectedProductionTaskProduct");
            }
        }
        private void RefreshProduction()
        {
            if (ProductionTaskID == null) return;
            ProductionTaskProducts = new ObservableCollection<ProductInfo>
                (from taskProducts in
                  DB.GammaBase.GetProductionTaskProducts(ProductionTaskID)
                select new ProductInfo 
                {DocProductID = taskProducts.DocID, ProductKind = (ProductKinds)taskProducts.ProductKindID, CharacteristicID = taskProducts.CharacteristicID, NomenclatureID = taskProducts.NomenclatureID, Date = taskProducts.Date,
                NomenclatureName = taskProducts.NomenclatureName,Number = taskProducts.Number, Quantity = taskProducts.Quantity,
                ProductID = taskProducts.ProductID});
        }
        private void ShowProduct()
        {
            OpenDocProductMessage msg;
            switch (SelectedProductionTaskProduct.ProductKind)
            {
                case ProductKinds.ProductSpool:
                    msg = new OpenDocProductMessage() { ID = SelectedProductionTaskProduct.ProductID, DocProductKind = DocProductKinds.DocProductSpool };
                    break;
                default:
                    msg = new OpenDocProductMessage();
                    break;
            }
            MessageManager.OpenDocProduct(msg);
            
        }
        private ObservableCollection<ProductInfo> _productionTaskProducts;
        public ObservableCollection<ProductInfo> ProductionTaskProducts
        {
            get
            {
                return _productionTaskProducts;
            }
            set
            {
                _productionTaskProducts = value;
                RaisePropertyChanged("ProductionTaskProducts");
            }
        }
    }

}