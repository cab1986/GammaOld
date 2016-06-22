using Gamma.Attributes;
using DevExpress.Mvvm;
using System;
using Gamma.Interfaces;
using System.Linq;
using Gamma.Models;
using System.Windows;
using System.Collections.Generic;
using System.Data.Entity;
using Gamma.Common;

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
        public ProductionTaskBatchViewModel(OpenProductionTaskBatchMessage msg, GammaEntities gammaBase = null): base(gammaBase)
        {
            Contractors = GammaBase.C1CContractors.Where(c => c.IsBuyer ?? false).ToList();
            ProductionTaskBatchID = msg.ProductionTaskBatchID ?? SqlGuidUtil.NewSequentialid();
            ChangeStateReadOnly = !DB.HaveWriteAccess("ProductionTasks");
            BatchKind = msg.BatchKind;
            TaskStates = new ProductionTaskStates().ToDictionary();
            ProcessModels = new ProcessModels().ToDictionary();
            RefreshProductionCommand = new DelegateCommand(RefreshProduction);
            ShowProductCommand = new DelegateCommand(ShowProduct,() => SelectedProductionTaskProduct != null);
            if (msg.ProductionTaskBatchID == null)
            {
                Date = DB.CurrentDateTime;
//                ProductionTaskBatchID = SqlGuidUtil.NewSequentialid();
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
                GetProductionTaskInfo(ProductionTaskBatchID);
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
            CreateNewProductCommand = new DelegateCommand(CreateNewProduct,CanCreateNewProduct);
            ActivatedCommand = new DelegateCommand(() => IsActive = true);
            DeactivatedCommand = new DelegateCommand(() => IsActive = false);
            Messenger.Default.Register<BarcodeMessage>(this, BarcodeReceived);

        }

        public override bool IsValid => base.IsValid && (!PartyControl || (PartyControl && ContractorId != null));

        private bool IsActive { get; set; } = true;
        /// <summary>
        /// Действие при получении шк от сканера
        /// </summary>
        /// <param name="msg">сообщение с шк</param>
        private void BarcodeReceived(BarcodeMessage msg)
        {
            if (!IsActive) return;
            var docProducts = GammaBase.DocProducts.Include(dp => dp.Docs).Include(dp => dp.Products)
                .FirstOrDefault(
                    dp => dp.Docs.DocTypeID == (short) DocTypes.DocProduction && dp.Products.BarCode == msg.Barcode);
            if (docProducts == null) return;
            GammaBase.Entry(docProducts).Reload();
            docProducts.IsInConfirmed = true;
            docProducts.Docs.IsConfirmed = true;
            string message;
            switch (docProducts.Products.ProductKindID)
            {
                case (byte)ProductKinds.ProductSpool:
                    message = $"Тамбур №{docProducts.Products.Number} подтвержден";
                    break;
                case (byte)ProductKinds.ProductPallet:
                    message = $"Паллета №{docProducts.Products.Number} подтверждена";
                    break;
                case (byte)ProductKinds.ProductGroupPack:
                    message = $"Групповая упаковка №{docProducts.Products.Number} подтверждена";
                    break;
                default:
                    message = $"Продукт №{docProducts.Products.Number} подтвержден";
                    break;
            }
            GammaBase.SaveChanges();
            MessageBox.Show(message, "Подтверждение", MessageBoxButton.OK, MessageBoxImage.Information);
            var productionTaskProduct =
                ProductionTaskProducts.FirstOrDefault(ptp => ptp.ProductID == docProducts.ProductID);
            if (productionTaskProduct != null) productionTaskProduct.IsConfirmed = true;
            var view = CurrentView as ProductionTaskSGIViewModel;
            if (view != null)
            {
                view.MadeQuantity =
                    _productionTaskProducts.Where(p => p.IsConfirmed).Sum(p => p.Quantity ?? 0);
            }
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

        private bool IsInProduction { get; set; }
        /// <summary>
        /// Только для чтения, если нет прав или задание не в состоянии "на рассмотрении"
        /// </summary>
        public bool IsReadOnly => !DB.HaveWriteAccess("ProductionTasks") || IsInProduction;


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
        private byte? _processModelId;

        private bool _partyControl;

        /// <summary>
        /// Идентификатор контрагента
        /// </summary>
        [UIAuth(UIAuthLevel.ReadOnly)]
        public Guid? ContractorId { get; set; }

        /// <summary>
        /// Список контрагентов
        /// </summary>
        public List<C1CContractors> Contractors { get; set; }

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
                return _processModelId;
            }
            set
            {
                _processModelId = value;
                if (CurrentView is IProductionTaskBatch)
                {
                    (CurrentView as IProductionTaskBatch).ProcessModelID = _processModelId ?? 0;
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
//            ProductionTaskBatchID = productionTaskBatchID;
            PartyControl = productionTaskBatch?.PartyControl ?? false;
            Number = productionTaskBatch?.Number;
            Date = productionTaskBatch?.Date;         
            Comment = productionTaskBatch?.Comment;
            ProductionTaskStateID = productionTaskBatch?.ProductionTaskStateID;
            IsInProduction = ProductionTaskStateID != (byte) ProductionTaskStates.NeedsDecision;
            ProcessModelID = (byte?)productionTaskBatch?.ProcessModelID;
            ContractorId = productionTaskBatch?.C1CContractorID;
            if (productionTaskBatch?.ProductionTaskStates != null)
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

        public override void SaveToModel(GammaEntities gammaBase = null)
        {
            if (!DB.HaveWriteAccess("ProductionTaskSGB")) return;
            gammaBase = gammaBase ?? DB.GammaDb;
            base.SaveToModel(gammaBase);
            var productionTaskBatch = gammaBase.ProductionTaskBatches.Find(ProductionTaskBatchID);
            if (productionTaskBatch == null)
            {
                productionTaskBatch = new ProductionTaskBatches()
                {
                    ProductionTaskBatchID = ProductionTaskBatchID,
                    UserID = WorkSession.UserID
                };
                gammaBase.ProductionTaskBatches.Add(productionTaskBatch);
            }
            productionTaskBatch.ProcessModelID = ProcessModelID;
            productionTaskBatch.ProductionTaskStateID = ProductionTaskStateID ?? 0;
            productionTaskBatch.Date = Date;
            productionTaskBatch.Comment = Comment;
            productionTaskBatch.PartyControl = PartyControl;
            productionTaskBatch.BatchKindID = (short)BatchKind;
            productionTaskBatch.C1CContractorID = ContractorId;
            gammaBase.SaveChanges();
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
            using (var gammaBase = DB.GammaDb)
            {
                var docProductKind = new DocProductKinds();
                var productionTaskID = gammaBase.ProductionTaskBatches.Where(p => p.ProductionTaskBatchID == ProductionTaskBatchID).
                        Select(p => p.ProductionTasks.FirstOrDefault(pt => pt.PlaceGroupID == (byte)WorkSession.PlaceGroup).ProductionTaskID).
                        FirstOrDefault();
                Guid productId;
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
                                var docProduction = gammaBase.Docs.Include(d => d.DocProduction)
                                    .Where(d => d.PlaceID == WorkSession.PlaceID // && d.ShiftID == null
                                                                                 //    && d.Date >= SqlFunctions.DateAdd("hh",-12, DB.GetShiftBeginTime(curDate))
                                    && d.DocTypeID == (byte)DocTypes.DocProduction).OrderByDescending(d => d.Date)
                                    .FirstOrDefault();
                                if (docProduction == null) break;
                                //Если не указана смена, то переходный тамбур. Устанавливаем недостающие свойства и открываем для редактирования
                                if (docProduction.ShiftID == null)
                                {
                                    docProduction.Date = curDate;
                                        docProduction.ShiftID = WorkSession.ShiftID;
                                        docProduction.UserID = WorkSession.UserID;
                                        docProduction.PrintName = WorkSession.PrintName;
                                        docProduction.DocProduction.ProductionTaskID = productionTaskID;
                                        productId = gammaBase.DocProducts.Where(d => d.DocID == docProduction.DocID)
                                            .Select(d => d.ProductID)
                                            .First();
                                        var productSpool =
                                            gammaBase.ProductSpools.First(p => p.ProductID == productId);
                                        var productionTaskPM =
                                            gammaBase.ProductionTasks.FirstOrDefault(
                                                p => p.ProductionTaskID == productionTaskID);
                                        if (productionTaskPM != null)
                                        {
                                            productSpool.C1CNomenclatureID = (Guid)productionTaskPM.C1CNomenclatureID;
                                            productSpool.C1CCharacteristicID = productionTaskPM.C1CCharacteristicID;
                                        }
                                        gammaBase.SaveChanges();
                                        gammaBase.GenerateNewNumbersForDoc(docProduction.DocID); //Генерация номера документа
                                        MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, productId);
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
                                    var firstOrDefault = gammaBase.DocProducts.FirstOrDefault(d => d.DocID == docProduction.DocID);
                                     if (firstOrDefault !=
                                            null)
                                            productId =
                                                firstOrDefault
                                                    .ProductID;
                                     else
                                     {
                                         break;
                                     }
                                    MessageBox.Show("Предыдущий тамбур не подтвержден. Он будет открыт для редактирования");
                                    MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, productId);
                                    return;
                                }
                                break;
                            case PlaceGroups.Rw:
                                docProductKind = DocProductKinds.DocProductUnload;
                                // Проверка на наличие неподтвержденного съема
                                var notConfirmedDocUnload = gammaBase.Docs.Where(d => d.PlaceID == WorkSession.PlaceID
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
                        MessageManager.CreateNewProduct(docProductKind, productionTaskID);
                        break;
                    // Действия для конвертингов
                    case BatchKinds.SGI:
                        var activeSourceSpools = gammaBase.GetActiveSourceSpools(WorkSession.PlaceID).ToList();
                        if (activeSourceSpools.Count == 0)
                        {
                            MessageBox.Show("Нет активных раскатов", "Не установлен тамбур", MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            return;
                        }                      
                            var currentDateTime = DB.CurrentDateTime;
                            var productionTask =
                                gammaBase.GetProductionTaskByBatchID(ProductionTaskBatchID,
                                    (short)PlaceGroups.Convertings).FirstOrDefault();
                            if (productionTask == null) return;
                            var baseQuantity =
                            (int)(gammaBase.C1CCharacteristics.Where(
                                c => c.C1CCharacteristicID == productionTask.C1CCharacteristicID)
                                .Select(c => c.C1CMeasureUnitsPallet.Coefficient).First() ?? 0);
                            productId = SqlGuidUtil.NewSequentialid();
                            var product = new Products()
                            {
                                ProductID = productId,
                                ProductKindID = (byte)ProductKinds.ProductPallet,
                                ProductPallets = new ProductPallets()
                                {
                                    ProductID = productId,
                                    ProductPalletItems = new List<ProductPalletItems>
                                    {
                                        new ProductPalletItems()
                                        {
                                            ProductPalletItemID = SqlGuidUtil.NewSequentialid(),ProductID = productId,
                                            C1CNomenclatureID = (Guid)productionTask.C1CNomenclatureID,
                                            C1CCharacteristicID = (Guid)productionTask.C1CCharacteristicID,
                                            Quantity = baseQuantity}
                                    }
                                }
                            };
                            gammaBase.Products.Add(product);
                            var docID = SqlGuidUtil.NewSequentialid();
                            var doc = new Docs
                            {
                                DocID = docID,
                                Date = currentDateTime,
                                DocTypeID = (int)DocTypes.DocProduction,
                                IsConfirmed = false,
                                PlaceID = WorkSession.PlaceID,
                                ShiftID = WorkSession.ShiftID,
                                UserID = WorkSession.UserID,
                                PrintName = WorkSession.PrintName,
                                DocProduction = new DocProduction
                                {
                                    DocID = docID,
                                    InPlaceID = WorkSession.PlaceID,
                                    ProductionTaskID = productionTask.ProductionTaskID
                                },
                                DocProducts = new List<DocProducts>
                                {
                                    new DocProducts
                                    {
                                        DocID = docID,
                                        ProductID = productId
                                    }
                                }
                            };
                            gammaBase.Docs.Add(doc);
                            var sourceSpools = gammaBase.GetActiveSourceSpools(WorkSession.PlaceID).ToList();
                            foreach (var spoolId in sourceSpools)
                            {
                                var docWithdrawal =
                                    gammaBase.DocWithdrawal
                                        .FirstOrDefault(d => d.Docs.DocProducts.Select(dp => dp.ProductID).Contains((Guid)spoolId));
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
                                            DocTypeID = (int)DocTypes.DocWithdrawal,
                                            PlaceID = WorkSession.PlaceID,
                                            UserID = WorkSession.UserID,
                                            ShiftID = WorkSession.ShiftID,
                                            PrintName = WorkSession.PrintName,
                                            IsConfirmed = false,
                                            DocProducts = new List<DocProducts>
                                            {
                                                new DocProducts
                                                {
                                                    DocID = docWithdrawalid,
                                                    ProductID = (Guid)spoolId,
                                                    IsOutConfirmed = true
                                                }
                                            }
                                        }
                                    };
                                    gammaBase.DocWithdrawal.Add(docWithdrawal);
                                }
                                if (docWithdrawal.DocProduction == null) docWithdrawal.DocProduction = new List<DocProduction>();
                                docWithdrawal.DocProduction.Add(doc.DocProduction);
                            }
                            gammaBase.SaveChanges();
                            ReportManager.PrintReport("Амбалаж", "Pallet", doc.DocID, true);
                            RefreshProduction();
                        
                        break;
                }
            }
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
            return base.CanSaveExecute() && (CurrentView?.IsValid ?? true) && (CurrentView?.CanSaveExecute() ?? true) &&
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
            ProductionTaskProducts = new ItemsChangeObservableCollection<ProductInfo>(from taskProducts in 
                                                                               GammaBase.GetBatchProducts(ProductionTaskBatchID)
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
                                                                               Place = taskProducts.Place,
                                                                               IsConfirmed = taskProducts.IsConfirmed
                                                                           });
        }
        private void ShowProduct()
        {
            switch (SelectedProductionTaskProduct.ProductKind)
            {
                case ProductKinds.ProductSpool:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedProductionTaskProduct.ProductID);
                    break;
                case ProductKinds.ProductGroupPack:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductGroupPack, SelectedProductionTaskProduct.ProductID);
                    break;
                case ProductKinds.ProductPallet:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductPallet, SelectedProductionTaskProduct.ProductID);
                    break;
                default:
                    MessageBox.Show("Ошибка программы, действие не предусмотрено");
                    return;
            }
        }
        private ItemsChangeObservableCollection<ProductInfo> _productionTaskProducts;
        public ItemsChangeObservableCollection<ProductInfo> ProductionTaskProducts
        {
            get
            {
                return _productionTaskProducts;
            }
            set
            {
                _productionTaskProducts = value;
                var view = CurrentView as ProductionTaskSGIViewModel;
                if (view != null)
                {
                    view.MadeQuantity =
                        _productionTaskProducts.Where(p => p.IsConfirmed).Sum(p => p.Quantity??0);
                }
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

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DelegateCommand ActivatedCommand { get; private set; }
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DelegateCommand DeactivatedCommand { get; private set; }

        private byte? _productionTaskStateID;
        // ReSharper disable once MemberCanBePrivate.Global
        public byte? ProductionTaskStateID
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
            var dialogResult = MessageBoxResult.None;
            using (var gammaBase = DB.GammaDb)
            {
                var sourceSpools = gammaBase.GetActiveSourceSpools(WorkSession.PlaceID).ToList();
                if (sourceSpools.Count == 0)
                {
                    MessageBox.Show("Нет активных раскатов");
                    return false;
                }
                if (!gammaBase.ProductionTaskRWCutting.Any(ptrw => ptrw.ProductionTasks.ProductionTaskBatches.FirstOrDefault().ProductionTaskBatchID == ProductionTaskBatchID))
                {
                    MessageBox.Show("В задании не указан раскрой");
                    return false;
                }
                var productionTaskId =
                    gammaBase.ProductionTasks
                        .FirstOrDefault(pt => pt.PlaceGroupID == (int) WorkSession.PlaceGroup
                                                          &&
                                                          pt.ProductionTaskBatches.Select(p => p.ProductionTaskBatchID)
                                                              .Contains(ProductionTaskBatchID))?
                        .ProductionTaskID;
                var result = gammaBase.CheckProductionTaskSourceSpools(WorkSession.PlaceID, productionTaskId).First();
                var resultMessage = result.ResultMessage;
                if (!string.IsNullOrWhiteSpace(resultMessage))
                {
                    if (result.BlockCreation)
                    {
                        MessageBox.Show(resultMessage, "Проверка исходных тамбуров", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    else
                    {
                        dialogResult = MessageBox.Show(resultMessage, "Проверка исходных тамбуров", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    }
                }
                return !result.BlockCreation || dialogResult == MessageBoxResult.Yes;
            }
        }
    }

}