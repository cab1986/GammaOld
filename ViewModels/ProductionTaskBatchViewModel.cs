using Gamma.Attributes;
using DevExpress.Mvvm;
using System;
using Gamma.Interfaces;
using System.Linq;
using Gamma.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Collections.Generic;
using System.Data.Entity;

namespace Gamma.ViewModels
{
    /// <summary>
    /// ViewModel для пакета заданий
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
            RefreshProductionCommand = new DelegateCommand(RefreshProduction);
            ShowProductCommand = new DelegateCommand(ShowProduct,() => SelectedProductionTaskProduct != null);
            if (msg.ProductionTaskBatchID == null)
            {
                Date = DB.CurrentDateTime;
                ProductionTaskBatchID = SqlGuidUtil.NewSequentialid();
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
                        case PlaceGroups.Rw:
                            NewProductText = "Создать новый съём";
                            break;
                        case PlaceGroups.Wr:
                            NewProductText = "Создать групповую упаковку";
                            break;
                    }
                    //                    PlaceGroupID = (int)PlaceGroups.RW;
                    break;
                case BatchKinds.SGI:
                    NewProductText = "Печать этикетки";
                    break;

            }
            BatchKind = msg.BatchKind;
            CreateNewProductCommand = new DelegateCommand(CreateNewProduct,CanCreateNewProduct);
            
        }

        private bool CanCreateNewProduct()
        {
            switch (BatchKind)
            {
                case BatchKinds.SGB:
                    return DB.HaveWriteAccess("ProductSpools") && IsActual;
                case BatchKinds.SGI:
                    return DB.HaveWriteAccess("ProductPallets") && IsActual;
                default:
                    return false;
            }
        }
        private SaveImplementedViewModel _currentView;
        public SaveImplementedViewModel CurrentView
        {
            get { return _currentView; }
            private set 
            { 
                _currentView = value;
                RaisePropertyChanged("CurrentView");
            }
        }

        private DateTime? _date;
        public DateTime? Date
        {
            get { return _date; }
            set 
            { 
                _date = value;
                RaisePropertyChanged("Date");
            }
        }

        private string _number;
        public string NewProductText { get; set; }
        private bool IsActual { get; set; } = true;

        public string Number
        {
            get { return _number; }
            set 
            { 
                _number = value;
                RaisePropertyChanged("Number");
            }
        }
        [UIAuth(UIAuthLevel.ReadOnly)]
        public string Comment { get; set; }
        /// <summary>
        /// Создание нового продукта. В конструкторе привязка к CreateNewProduct();
        /// </summary>
        public DelegateCommand CreateNewProductCommand { get; private set; }
        /// <summary>
        /// Только для чтения, если нет прав или задание не в состоянии "на рассмотрении"
        /// </summary>
        public bool IsReadOnly => (!DB.HaveWriteAccess("ProductionTasks") && !WorkSession.DBAdmin) || 
                                  ProductionTaskStateID != (byte)ProductionTaskStates.NeedsDecision;

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
        private short? _processModelid;

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
        public short? ProcessModelID
        {
            get
            {
                return _processModelid;
            }
            set
            {
                _processModelid = value;
                if (CurrentView is IProductionTaskBatch)
                {
                    (CurrentView as IProductionTaskBatch).ProcessModelID = _processModelid ?? 0;
                }
            }
        }

        public Visibility ProcessModelVisible { get; private set; } = Visibility.Hidden;

        public Guid ProductionTaskBatchID { get; set; }

        private void GetProductionTaskInfo(Guid productionTaskBatchID)
        {
            var productionTaskBatch = (from pt in DB.GammaDb.ProductionTaskBatches.Include(pt => pt.ProductionTaskStates)
                                  where pt.ProductionTaskBatchID == productionTaskBatchID
                                  select pt).FirstOrDefault();
            ProductionTaskBatchID = productionTaskBatchID;
            PartyControl = productionTaskBatch?.PartyControl ?? false;
            Number = productionTaskBatch.Number;
            Date = productionTaskBatch.Date;         
            Comment = productionTaskBatch.Comment;
            ProductionTaskStateID = productionTaskBatch.ProductionTaskStateID;
            ProcessModelID = productionTaskBatch.ProcessModelID;
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
               case BatchKinds.SGI:
                    CurrentView = new ProductionTaskSGIViewModel(ProductionTaskBatchID);
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
            productionTaskBatch.ProductionTaskStateID = ProductionTaskStateID ?? 0;
            productionTaskBatch.Date = Date;
            productionTaskBatch.Comment = Comment;
            productionTaskBatch.PartyControl = PartyControl;
            productionTaskBatch.BatchKindID = (short)BatchKind;
            DB.GammaBase.SaveChanges();
            CurrentView?.SaveToModel(ProductionTaskBatchID);
        }

/*
        private void SetProductionTaskProperties(ProductionTaskBatches productionTaskBatch)
        {
            productionTaskBatch.Date = Date;
            productionTaskBatch.Comment = Comment;
            productionTaskBatch.UserID = WorkSession.UserID;
            productionTaskBatch.BatchKindID = (short)BatchKind;
            productionTaskBatch.ProductionTaskStateID = ProductionTaskStateID;
        }
*/
        //Создание нового продукта
        private void CreateNewProduct()
        {
            var docProductKind = new DocProductKinds();
            var productionTaskID = DB.GammaBase.ProductionTaskBatches.Where(p => p.ProductionTaskBatchID == ProductionTaskBatchID).
                    Select(p => p.ProductionTasks.FirstOrDefault(pt => pt.PlaceGroupID == (byte)WorkSession.PlaceGroup).ProductionTaskID).
                    FirstOrDefault();
            switch (BatchKind)
            {
                case BatchKinds.SGB:
                    switch (WorkSession.PlaceGroup)
                    {
                        case PlaceGroups.PM:
                            docProductKind = DocProductKinds.DocProductSpool;
                            // Проверка на наличие тамбура с предыдущей смены
                            var curDate = DB.CurrentDateTime;
                            // получаем предыдущий документ в базе
                            var docProduction = DB.GammaDb.Docs.Include(d => d.DocProduction)
                                .Where(d => d.PlaceID == WorkSession.PlaceID // && d.ShiftID == null
                            //    && d.Date >= SqlFunctions.DateAdd("hh",-12, DB.GetShiftBeginTime(curDate))
                                && d.DocTypeID == (byte)DocTypes.DocProduction).OrderByDescending(d => d.Date)
                                .FirstOrDefault();
                            if (docProduction == null) break;
                            //Если не указана смена, то переходный тамбур. Устанавливаем недостающие свойства и открываем для редактирования
                            if (docProduction.ShiftID == null)
                            {
                                using (var gammaBase = DB.GammaDb)
                                {
                                    docProduction.Date = curDate;
                                    docProduction.ShiftID = WorkSession.ShiftID;
                                    docProduction.UserID = WorkSession.UserID;
                                    docProduction.PrintName = WorkSession.PrintName;
                                    docProduction.DocProduction.ProductionTaskID = productionTaskID;
                                    var productid =
                                        gammaBase.DocProducts.Where(d => d.DocID == docProduction.DocID)
                                            .Select(d => d.ProductID)
                                            .First();
                                    var productSpool =
                                        gammaBase.ProductSpools.FirstOrDefault(p => p.ProductID == productid);
                                    var productionTaskPM =
                                        gammaBase.ProductionTasks.FirstOrDefault(
                                            p => p.ProductionTaskID == productionTaskID);
                                    if (productionTaskPM != null)
                                    {
                                        productSpool.C1CNomenclatureID = productionTaskPM.C1CNomenclatureID;
                                        productSpool.C1CCharacteristicID = productionTaskPM.C1CCharacteristicID;
                                    }
                                    gammaBase.SaveChanges();
                                    gammaBase.GenerateNewNumbersForDoc(docProduction.DocID); //Генерация номера документа
                                    MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, productid);
                                }
                                return;
                            }
                            
                            /*var notConfirmedDoc = DB.GammaBase.Docs.Where(d => d.PlaceID == WorkSession.PlaceID &&
                                d.ShiftID == WorkSession.ShiftID && d.DocProducts.FirstOrDefault() != null
                                && d.DocTypeID == (byte)DocTypes.DocProduction).
                                OrderByDescending(d => d.Date).Take(1).FirstOrDefault();
                             * */
                            //если предыдущий тамбур этой смены и не подтвержден, то открываем для редактирования
                            if (docProduction.ShiftID == WorkSession.ShiftID && !docProduction.IsConfirmed)
                            {
                                MessageBox.Show("Предыдущий тамбур не подтвержден. Он будет открыт для редактирования");
                                var productid = DB.GammaDb.DocProducts.Where(d => d.DocID == docProduction.DocID).Select(d => d.ProductID).First();
                                MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, productid);
                                return;
                            }                  
                            break;
                        case PlaceGroups.Rw:
                            docProductKind = DocProductKinds.DocProductUnload;
                            // Проверка на наличие неподтвержденного съема
                            var notConfirmedDocUnload = DB.GammaDb.Docs.Where(d => d.PlaceID == WorkSession.PlaceID 
                            && d.DocTypeID == (byte)DocTypes.DocProduction).OrderByDescending(d => d.Date).FirstOrDefault();
                            if (notConfirmedDocUnload != null && !notConfirmedDocUnload.IsConfirmed)
                            {
                                MessageBox.Show("Предыдущий съем не подтвержден. Он будет открыт для редактирования");
                                MessageManager.OpenDocProduct(DocProductKinds.DocProductUnload, notConfirmedDocUnload.DocID);
                                return;
                            }
                            if (!SourceSpoolsCorrect()) return;
//                            var checkResult = DB.CheckSourceSpools(WorkSession.PlaceID, productionTaskID);
//                            if (checkResult != "")
//                            {
//                                var dlgResult = MessageBox.Show(checkResult + "Вы уверены, что хотите продолжить?", "Проверка тамбуров", 
//                                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
//                                if (dlgResult == MessageBoxResult.No) return;
//                            }
                            break;
                    }    
                    break;          
                    case BatchKinds.SGI:
                        using (var gammaBase = DB.GammaDb)
                        {
                            var currentDateTime = DB.CurrentDateTime;
                            var productionTask =
                                gammaBase.GetProductionTaskByBatchID(ProductionTaskBatchID,
                                    (short) PlaceGroups.Convertings).FirstOrDefault();
                            if (productionTask == null) return;
                            var productid = SqlGuidUtil.NewSequentialid();
                            var product = new Products()
                            {
                                ProductID = productid,
                                ProductKindID = (byte) ProductKinds.ProductPallet,
                                ProductPallets = new ProductPallets()
                                {
                                    ProductID = productid,
                                    ProductPalletItems = new List<ProductPalletItems>
                                    {
                                        new ProductPalletItems()
                                        {
                                            ProductID = productid,
                                            C1CNomenclatureID = productionTask.C1CNomenclatureID,
                                            C1CCharacteristicID = productionTask.C1CCharacteristicID,
                                            Quantity = (int)productionTask.Quantity
                                        }
                                    }
                                }
                            };
                            gammaBase.Products.Add(product);
                            var docID = SqlGuidUtil.NewSequentialid();
                            var doc = new Docs()
                            {
                                DocID = docID,
                                Date = currentDateTime,
                                DocTypeID = (int) DocTypes.DocProduction,
                                IsConfirmed = false,
                                PlaceID = WorkSession.PlaceID,
                                ShiftID = WorkSession.ShiftID,
                                UserID = WorkSession.UserID,
                                PrintName = WorkSession.PrintName,
                                DocProduction = new DocProduction()
                                {
                                    DocID = docID,
                                    InPlaceID = WorkSession.PlaceID,
                                    ProductionTaskID = productionTask.ProductionTaskID
                                },
                                DocProducts = new List<DocProducts>
                                {
                                    new DocProducts()
                                    {
                                        DocID = docID,
                                        ProductID = productid
                                    }
                                }
                            };
                            gammaBase.Docs.Add(doc);
                            var sourceSpools = gammaBase.GetActiveSourceSpools(WorkSession.PlaceID).ToList();
                            foreach (var spoolid in sourceSpools)
                            {
                                var docWithdrawal =
                                    gammaBase.DocWithdrawal
                                        .FirstOrDefault(d => d.Docs.DocProducts.Select(dp => dp.ProductID).Contains((Guid) spoolid));
                                if (docWithdrawal == null)
                                {
                                    var docWithdrawalid = SqlGuidUtil.NewSequentialid();
                                    docWithdrawal = new DocWithdrawal()
                                    {
                                        DocID = docWithdrawalid,
                                        OutPlaceID = WorkSession.PlaceID,
                                        Docs = new Docs()
                                        {
                                            DocID = docWithdrawalid,
                                            Date = currentDateTime,
                                            DocTypeID = (int) DocTypes.DocWithdrawal,
                                            PlaceID = WorkSession.PlaceID,
                                            UserID = WorkSession.UserID,
                                            ShiftID = WorkSession.ShiftID,
                                            PrintName = WorkSession.PrintName,
                                            IsConfirmed = false,
                                            DocProducts = new List<DocProducts>
                                            {
                                                new DocProducts()
                                                {
                                                    DocID = docWithdrawalid,
                                                    ProductID = (Guid)spoolid,
                                                    IsOutConfirmed = true
                                                }
                                            }
                                        }
                                    };
                                }
                                if (docWithdrawal.DocProduction == null) docWithdrawal.DocProduction = new List<DocProduction>();
                                docWithdrawal.DocProduction.Add(doc.DocProduction);
                                gammaBase.DocWithdrawal.Add(docWithdrawal);
                                gammaBase.SaveChanges();
                                ReportManager.PrintReport("Амбалаж","Pallet",productid, false); 
                            }
                        }
                        break;        
            }
            MessageManager.CreateNewProduct(docProductKind, productionTaskID);
        }

//        private ObservableCollection<Place> _places = DB.GetPlaces(PlaceGroups.PM);
//        private int _PlaceID;
/*        [Required(ErrorMessage="Необходимо выбрать передел")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public int PlaceID
        {
            get
            {
                return _PlaceID;
            }
            set
            {
                _PlaceID = value;
                RaisePropertyChanged("PlaceID");
            }
        }
 * */
        public override bool CanSaveExecute()
        {
            return base.CanSaveExecute() && (CurrentView?.IsValid ?? true) &&
                (DB.HaveWriteAccess("ProductionTasks"));
        }
        public DelegateCommand RefreshProductionCommand { get; private set; }
        public DelegateCommand ShowProductCommand { get; private set; }
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
            MessageManager.OpenDocProduct(docProductKind, SelectedProductionTaskProduct.DocID);
            
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
        private byte? _ProductionTaskStateID;
        public byte? ProductionTaskStateID
        {
            get
            {
                return _ProductionTaskStateID;
            }
            set
            {
            	_ProductionTaskStateID = value;
                RaisePropertyChanged("ProductionTaskStateID");
            }
        }
        public Dictionary<byte, string> TaskStates { get; set; }
        public Dictionary<byte, string> ProcessModels { get; set; }
        public bool ChangeStateReadOnly { get; set; }
        public string Title { get; set; }
        private bool SourceSpoolsCorrect()
        {
            var sourceSpools = DB.GammaBase.GetActiveSourceSpools(WorkSession.PlaceID).ToList();
            if (sourceSpools.Count == 0)
            {
                MessageBox.Show("Нет активных раскатов");
                return false;
            }
            if (!DB.GammaBase.ProductionTaskRWCutting.Any(ptrw => ptrw.ProductionTasks.ProductionTaskBatches.FirstOrDefault().ProductionTaskBatchID == ProductionTaskBatchID))
            {
                MessageBox.Show("В задании не указан раскрой");
                return false;
            }
            var result = DB.CheckSourceSpools(WorkSession.PlaceID, ProductionTaskBatchID);
            if (string.IsNullOrEmpty(result)) return true;
            var dialogResult = MessageBox.Show(result, "Проверка исходных тамбуров", MessageBoxButton.YesNo,MessageBoxImage.Warning);
            if (dialogResult == MessageBoxResult.Yes)
                return true;
            else
                return false;
        }
    }

}