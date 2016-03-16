using Gamma.Attributes;
using GalaSoft.MvvmLight.Command;
using System.ComponentModel.DataAnnotations;
using System;
using Gamma.Interfaces;
using System.Linq;
using Gamma.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Collections.Generic;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTaskBatchViewModel : SaveImplementedViewModel, ICheckedAccess
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskViewModel class.
        /// </summary>
        public ProductionTaskBatchViewModel(OpenProductionTaskBatchMessage msg)
        {
            ChangeStateReadOnly = !DB.HaveWriteAccess("ProductionTasks") &&
                !WorkSession.DBAdmin;
            TaskStates = new ProductionTaskStates().ToDictionary();
            ProcessModels = new ProcessModels().ToDictionary();
            RefreshProductionCommand = new RelayCommand(RefreshProduction);
            ShowProductCommand = new RelayCommand(ShowProduct,() => SelectedProductionTaskProduct != null);
            if (msg.ProductionTaskBatchID == null)
            {
                Date = DB.CurrentDateTime;
                ProductionTaskBatchID = SQLGuidUtil.NewSequentialId();
                Title = "Новое задание на производство";
                IsActual = false;
//                if (Places.Count > 0)
//                    PlaceID = Places[0].PlaceID;
                ProductionTaskStateID = 0;
                if (msg.BatchKind == BatchKinds.SGB)
                {
                    ProcessModelID = 0;
                }
            }
            else
            {
                GetProductionTaskInfo((Guid)msg.ProductionTaskBatchID);
                RefreshProduction();
                Title = "Задание на производство № " + Number;
            }
            switch (msg.BatchKind)
            {
                case BatchKinds.SGB:
                    //                    PlaceGroupID = (int)PlaceGroups.PM;
                    ProcessModelVisible = Visibility.Visible;
                    switch (WorkSession.PlaceGroup)
                    {
                        case PlaceGroups.PM:
                            NewProductText = "Создать новый тамбур";
                            break;
                        case PlaceGroups.RW:
                            NewProductText = "Создать новый съём";
                            break;
                        case PlaceGroups.WR:
                            NewProductText = "Создать групповую упаковку";
                            break;
                    }
                    //                    PlaceGroupID = (int)PlaceGroups.RW;
                    break;
                default:
                    break;
            }
            BatchKind = msg.BatchKind;
            CreateNewProductCommand = new RelayCommand(CreateNewProduct,CanCreateNewProduct);
            
        }

        private bool CanCreateNewProduct()
        {
            switch (BatchKind)
            {
                case BatchKinds.SGB:
                    return DB.HaveWriteAccess("ProductSpools") && IsActual;
                default:
                    break;
            }
            return false;
        }
        private SaveImplementedViewModel currentview;
        public SaveImplementedViewModel CurrentView
        {
            get { return currentview; }
            private set 
            { 
                currentview = value;
                RaisePropertyChanged("CurrentView");
            }
        }

        private DateTime date;
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
        public string NewProductText { get; set; }
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
        [UIAuth(UIAuthLevel.ReadOnly)]
        public string Comment { get; set; }
        public RelayCommand CreateNewProductCommand { get; private set; } // Создание нового продукта. В конструкторе привязка к CreateNewProduct();

        public bool IsReadOnly
        {
            get
            {
                var access = DB.GammaBase.UserPermit("ProductionTasks").FirstOrDefault();

                return 
                    ((access == null ? true : access != (byte)PermissionMark.ReadAndWrite) && !WorkSession.DBAdmin) || 
                    ProductionTaskStateID != (byte)ProductionTaskStates.NeedsDecision;
            }
            
        }

        private BatchKinds _batchKind;

 /*       public ObservableCollection<Place> Places
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
  * */
        public BatchKinds BatchKind
        {
            get { return _batchKind; }
            set 
            { 
                _batchKind = value;
                ChangeCurrentView(_batchKind);
            }
        }
        private byte? _processModeID;

        private bool _partyControl;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public bool PartyControl
        {
            get
            {
                return _partyControl;
            }
            set
            {
            	_partyControl = value;
                RaisePropertyChanged("PartyControl");
            }
        }
        [UIAuth(UIAuthLevel.ReadOnly)]
        public byte? ProcessModelID
        {
            get
            {
                return _processModeID;
            }
            set
            {
                _processModeID = value;
                if (CurrentView is IProductionTaskBatch)
                {
                    (CurrentView as IProductionTaskBatch).ProcessModelID = _processModeID ?? 0;
                }
            }
        }
        private Visibility _processModelVisible = Visibility.Hidden;
        public Visibility ProcessModelVisible
        {
            get { return _processModelVisible; }
            private set
            {
                _processModelVisible = value;
            }

        }
        private Guid _productionTaskBatchID;
        public Guid ProductionTaskBatchID
        {
            get { return _productionTaskBatchID; }
            set
            {
                if (value != null)
                {
                    _productionTaskBatchID = value;
                }
            }
        }
  

        private void GetProductionTaskInfo(Guid productionTaskBatchID)
        {
            var productionTaskBatch = (from pt in DB.GammaBase.ProductionTaskBatches.Include("ProductionTaskStates")
                                  where pt.ProductionTaskBatchID == productionTaskBatchID
                                  select pt).FirstOrDefault();
            DB.GammaBase.Entry(productionTaskBatch).Reload();
            ProductionTaskBatchID = productionTaskBatchID;
            PartyControl = productionTaskBatch.PartyControl ?? false;
            Number = productionTaskBatch.Number;
            Date = productionTaskBatch.Date;         
            Comment = productionTaskBatch.Comment;
            ProductionTaskStateID = productionTaskBatch.ProductionTaskStateID;
            ProcessModelID = (byte)productionTaskBatch.ProcessModelID;
            if (productionTaskBatch.ProductionTaskStates != null)
                IsActual = productionTaskBatch.ProductionTaskStates.IsActual;
        }
        private void ChangeCurrentView(BatchKinds batchKind)
        {
            switch (batchKind)
            {
               case BatchKinds.SGB:
                    CurrentView = new ProductionTaskBatchSGBViewModel(ProductionTaskBatchID);
                    break;
                default:
                    break;
            }
        }
   
        public override void SaveToModel()
        {
            base.SaveToModel();
            var productionTaskBatch = DB.GammaBase.ProductionTaskBatches.Find(ProductionTaskBatchID);
            if (productionTaskBatch == null)
            {
                productionTaskBatch = new ProductionTaskBatches()
                {
                    ProductionTaskBatchID = ProductionTaskBatchID,
                    UserID = WorkSession.UserID
                };
                DB.GammaBase.ProductionTaskBatches.Add(productionTaskBatch);
            }
            productionTaskBatch.ProcessModelID = ProcessModelID;
            productionTaskBatch.ProductionTaskStateID = ProductionTaskStateID;
            productionTaskBatch.Date = Date;
            productionTaskBatch.Comment = Comment;
            productionTaskBatch.PartyControl = PartyControl;
            DB.GammaBase.SaveChanges();
            if (CurrentView != null)
                CurrentView.SaveToModel(ProductionTaskBatchID);
        }

        private void SetProductionTaskProperties(ProductionTaskBatches productionTaskBatch)
        {
            productionTaskBatch.Date = Date;
            productionTaskBatch.Comment = Comment;
            productionTaskBatch.UserID = WorkSession.UserID;
            productionTaskBatch.BatchKindID = (short)BatchKind;
            productionTaskBatch.ProductionTaskStateID = ProductionTaskStateID;
        }
        //Создание нового продукта
        private void CreateNewProduct()
        {
            var docProductKind = new DocProductKinds();
            var productionTaskID = DB.GammaBase.ProductionTaskBatches.Where(p => p.ProductionTaskBatchID == ProductionTaskBatchID).
                    Select(p => p.ProductionTasks.Where(pt => pt.PlaceGroupID == (byte)WorkSession.PlaceGroup).FirstOrDefault().ProductionTaskID).
                    FirstOrDefault();
            switch (BatchKind)
            {
                case BatchKinds.SGB:
                    switch (WorkSession.PlaceGroup)
                    {
                        case PlaceGroups.PM:
                            docProductKind = DocProductKinds.DocProductSpool;
                            // Проверка на наличие тамбура с предыдущей смены
                            var docProduction = DB.GammaBase.Docs.Where(d => d.PlaceID == WorkSession.PlaceID && d.ShiftID == null).FirstOrDefault();
                            //Если нашли, то устанавливаем дату, смену и открываем для редактирования
                            if (docProduction != null)
                            {
                                docProduction.Date = DB.CurrentDateTime;
                                docProduction.ShiftID = WorkSession.ShiftID;
                                docProduction.UserID = WorkSession.UserID;
                                docProduction.PrintName = WorkSession.PrintName;
                                docProduction.DocProduction.ProductionTaskID = ProductionTaskBatchID;
                                var productID = DB.GammaBase.DocProducts.Where(d => d.DocID == docProduction.DocID).Select(d => d.ProductID).First();
                                DB.GammaBase.SaveChanges();
                                DB.GammaBase.GenerateNewNumbersForDoc(docProduction.DocID); //Генерация номера документа
                                MessageManager.OpenDocProduct(docProductKind, productID);
                                return;
                            }
                            // Проверка на наличие неподтвержденного тамбура
                            var notConfirmedDoc = DB.GammaBase.Docs.Where(d => d.PlaceID == WorkSession.PlaceID &&
                                d.ShiftID == WorkSession.ShiftID && d.DocProducts.FirstOrDefault() != null).
                                OrderByDescending(d => d.Date).Take(1).FirstOrDefault();
                            if (notConfirmedDoc != null && !notConfirmedDoc.IsConfirmed)
                            {
                                MessageBox.Show("Предыдущий тамбур не подтвержден. Он будет открыт для редактирования");
                                var productID = DB.GammaBase.DocProducts.Where(d => d.DocID == notConfirmedDoc.DocID).
                                    Select(d => d.ProductID).First();
                                MessageManager.OpenDocProduct(docProductKind, productID);
                                return;
                            }                  
                            break;
                        case PlaceGroups.RW:
                            if (!SourceSpoolsCorrect()) return;
                            docProductKind = DocProductKinds.DocProductUnload;
                            // Проверка на наличие неподтвержденного съема
                            var notConfirmedDocUnload = DB.GammaBase.Docs.Where(d => d.PlaceID == WorkSession.PlaceID &&
                                d.ShiftID == WorkSession.ShiftID).OrderByDescending(d => d.Date).Take(1).FirstOrDefault();
                            if (notConfirmedDocUnload != null && !notConfirmedDocUnload.IsConfirmed)
                            {
                                MessageBox.Show("Предыдущий съем не подтвержден. Он будет открыт для редактирования");
                                MessageManager.OpenDocProduct(docProductKind, notConfirmedDocUnload.DocID);
                                return;
                            }
                            var checkResult = DB.CheckSourceSpools(WorkSession.PlaceID, productionTaskID);
                            if (checkResult != "")
                            {
                                var dlgResult = MessageBox.Show(checkResult + "Вы уверены, что хотите продолжить?", "Проверка тамбуров", 
                                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                if (dlgResult == MessageBoxResult.No) return;
                            }
                            break;
                    }    
                    break;                  
                default:
                    break;
            }
            MessageManager.CreateNewProduct(docProductKind, productionTaskID);
        }
        private bool _isActual = true;
//        private ObservableCollection<Place> _places = DB.GetPlaces(PlaceGroups.PM);
//        private int _placeID;
/*        [Required(ErrorMessage="Необходимо выбрать передел")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public int PlaceID
        {
            get
            {
                return _placeID;
            }
            set
            {
                _placeID = value;
                RaisePropertyChanged("PlaceID");
            }
        }
 * */
        public override bool CanSaveExecute()
        {
            return base.CanSaveExecute() && (CurrentView == null ? true : CurrentView.IsValid) &&
                (DB.HaveWriteAccess("ProductionTasks"));
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
            if (ProductionTaskBatchID == null) return;
            ProductionTaskProducts = new ObservableCollection<ProductInfo>(from taskProducts in 
                                                                               DB.GammaBase.GetProductionTaskBatchProducts(ProductionTaskBatchID)
                                                                           select new ProductInfo { 
                                                                               DocID = taskProducts.DocID,
                                                                               ProductKind = (ProductKinds)taskProducts.ProductKindID,
                                                                               CharacteristicID = taskProducts.CharacteristicID,
                                                                               NomenclatureID = taskProducts.NomenclatureID,
                                                                               Date = taskProducts.Date,
                                                                               NomenclatureName = taskProducts.NomenclatureName,
                                                                               Number = taskProducts.Number,
                                                                               Quantity = taskProducts.Quantity,
                                                                               ProductID = taskProducts.ProductID,
                                                                               Place = taskProducts.Place
                                                                           });
        }
        private void ShowProduct()
        {
            DocProductKinds docProductKind;
            switch (SelectedProductionTaskProduct.ProductKind)
            {
                case ProductKinds.ProductSpool:
                    docProductKind = DocProductKinds.DocProductSpool;
                    break;
                default:
                    MessageBox.Show("Ошибка программы, действие не предусмотрено");
                    return;
            }
            MessageManager.OpenDocProduct(docProductKind, SelectedProductionTaskProduct.ProductID);
            
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
/*        private Visibility _characteristicVisible = Visibility.Visible;
        public Visibility CharacteristicVisible
        {
            get
            {
                return _characteristicVisible;
            }
            set
            {
            	_characteristicVisible = value;
                RaisePropertyChanged("CharacteristicVisible");
            }
        }
 * */
//        [Required(ErrorMessage="Необходимо выбрать характеристику")]
/*        [UIAuth(UIAuthLevel.ReadOnly)]
        public Guid? CharacteristicID { get; set; }
 * */
        private byte _productionTaskStateID;
        public byte ProductionTaskStateID
        {
            get
            {
                return _productionTaskStateID;
            }
            set
            {
            	_productionTaskStateID = value;
                RaisePropertyChanged("ProductionTaskStateID");
            }
        }
        public Dictionary<byte, string> TaskStates { get; set; }
        public Dictionary<byte, string> ProcessModels { get; set; }
        public bool ChangeStateReadOnly { get; set; }
        public string Title { get; set; }
        private bool SourceSpoolsCorrect()
        {
/*
 * var ptCharProps = DB.GammaBase.GetCharPropsDescriptions(ptCharacteristicID).FirstOrDefault();
            int sourceLayerNumbers = 0;
            foreach (var spoolID in sourceSpools)
            {
                var nomenclatureID = DB.GammaBase.ProductSpools.Where(p => p.ProductID == spoolID).Select(p => p.C1CNomenclatureID).FirstOrDefault();
                if (NomenclatureID != nomenclatureID)
                {
                    MessageBox.Show("Тамбура на раскатах не подходят для данного задания", "Не те тамбура на раскатах", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                var characteristicID = DB.GammaBase.ProductSpools.Where(p => p.ProductID == spoolID).Select(p => p.C1CCharacteristicID).FirstOrDefault();
                var sSpoolCharProps = DB.GammaBase.GetCharPropsDescriptions(characteristicID).FirstOrDefault();
                if (ptCharProps.Color != sSpoolCharProps.Color)
                {
                    MessageBox.Show("Цвет тамбура на раскате не соответствует заданию");
                    return false;
                }
                sourceLayerNumbers = sourceLayerNumbers + DB.GammaBase.GetCharSpoolLayerNumber(characteristicID).FirstOrDefault() ?? 0;
            }
            var ptLayerNumber = DB.GammaBase.GetCharSpoolLayerNumber(ptCharacteristicID).FirstOrDefault();
            if (ptLayerNumber != sourceLayerNumbers)
            {
                MessageBox.Show("Несовпадение слойности");
                return false;
            }
            return true;
 * */
            var sourceSpools = DB.GammaBase.GetActiveSourceSpools(WorkSession.PlaceID).ToList();
            if (sourceSpools.Count == 0)
            {
                MessageBox.Show("Нет активных раскатов");
                return false;
            }
            var ptCharacteristicID = (from ptrw in DB.GammaBase.ProductionTaskRWCutting
                                      where
                                          ptrw.ProductionTaskID == ProductionTaskBatchID
                                      select ptrw.C1CCharacteristicID).FirstOrDefault();
            if (ptCharacteristicID == null)
            {
                MessageBox.Show("В задании не указан раскрой");
                return false;
            }
            var result = DB.CheckSourceSpools(WorkSession.PlaceID, ProductionTaskBatchID);
            if (result == "" || result == null) return true;
            var dialogResult = MessageBox.Show(result, "Проверка исходных тамбуров", MessageBoxButton.YesNo,MessageBoxImage.Asterisk);
            if (dialogResult == MessageBoxResult.Yes)
                return true;
            else
                return false;
        }
    }

}