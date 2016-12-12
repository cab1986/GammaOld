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
            if (!WorkSession.IsRemotePrinting && WorkSession.PlaceGroup == PlaceGroup.Convertings)
            {
                MakeProductionTaskActiveForPlace();
            }
            ChangeStateReadOnly = !DB.HaveWriteAccess("ProductionTasks");
            BatchKind = msg.BatchKind;
            TaskStates = new ProductionTaskStates().ToDictionary();
            ProcessModels = new ProcessModels().ToDictionary();
            RefreshProductionCommand = new DelegateCommand(RefreshProduction);
            ShowProductCommand = new DelegateCommand(ShowProduct,() => SelectedProductionTaskProduct != null);
            DeleteProdutCommand = new DelegateCommand(DeleteProduct, () => SelectedProductionTaskProduct != null);
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
                        case PlaceGroup.PM:
                            NewProductText = "Создать новый тамбур";
                            break;
                        case PlaceGroup.Rw:
                            NewProductText = "Создать новый съём";
                            break;
                        case PlaceGroup.Wr:
                            NewProductText = "Создать групповую упаковку";
                            break;
                    }
                    //                    PlaceGroupID = (int)PlaceGroups.RW;
                    break;
                case BatchKinds.SGI:
                    NewProductText = WorkSession.IsRemotePrinting? "Сделать задание активным" : "Печать этикетки";
                    ExpandProductionTaskProducts = WorkSession.PlaceGroup != PlaceGroup.Other;
                    break;

            }
            CreateNewProductCommand = !WorkSession.IsRemotePrinting ? new DelegateCommand(CreateNewProduct, CanCreateNewProduct) 
                : new DelegateCommand(CreateNewProduct, DB.HaveWriteAccess("ActiveProductionTasks"));
            ActivatedCommand = new DelegateCommand(() => IsActive = true);
            DeactivatedCommand = new DelegateCommand(() => IsActive = false);
            Messenger.Default.Register<BarcodeMessage>(this, BarcodeReceived);
            Messenger.Default.Register<ProductChangedMessage>(this, ProductChanged);
        }

        public DelegateCommand DeleteProdutCommand { get; private set; }

        private void DeleteProduct()
        {
            if (SelectedProductionTaskProduct == null) return;
            if (WorkSession.ShiftID != 0 && (SelectedProductionTaskProduct.PlaceID != WorkSession.PlaceID || SelectedProductionTaskProduct.ShiftID != WorkSession.ShiftID))
            {
                MessageBox.Show("Вы не можете удалить продукцию другой смены или другого передела");
                return;
            }
            if (MessageBox.Show(
                "Вы уверены, что хотите удалить продукт № " + SelectedProductionTaskProduct.Number + "?",
                "Удаление продукта", MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.Yes) != MessageBoxResult.Yes)
            {
                return;
            }
            switch (SelectedProductionTaskProduct.ProductKind)
            {
                    case ProductKind.ProductSpool:
                        if (DB.HaveWriteAccess("ProductSpools"))
                        {
                            GammaBase.DeleteSpool(SelectedProductionTaskProduct.ProductID);
                        }
                        else
                        {
                            MessageBox.Show("Не достаточно прав для удаления тамбура");
                        }
                        break;
                    case ProductKind.ProductGroupPack:
                        if (DB.HaveWriteAccess("ProductGroupPacks"))
                        {
                            GammaBase.DeleteGroupPack(SelectedProductionTaskProduct.ProductID);
                        }
                        else
                        {
                            MessageBox.Show("Не достаточно прав для удаления групповой упаковки");
                        }
                        break;
                    case ProductKind.ProductPallet:
                        if (DB.HaveWriteAccess("ProductPallets"))
                        {
                            GammaBase.DeletePallet(SelectedProductionTaskProduct.ProductID);
                        }
                        else
                        {
                            MessageBox.Show("Не достаточно прав для удаления паллеты");
                        }
                        break;
            }
        }


        public override bool IsValid => base.IsValid && (!PartyControl || (PartyControl && ContractorId != null));

        private bool IsActive { get; set; } = true;

        private void ProductChanged(ProductChangedMessage msg)
        {
            var product = ProductionTaskProducts.FirstOrDefault(p => p.ProductID == msg.ProductID);
            if (product == null) return;
            using (var gammaBase = DB.GammaDb)
            {
                var info = gammaBase.vProductsInfo.FirstOrDefault(p => p.ProductID == product.ProductID);
                if (info == null) return;
                product.NomenclatureID = info.C1CNomenclatureID;
                product.CharacteristicID = info.C1CCharacteristicID;
                product.NomenclatureName = info.NomenclatureName;
                product.Quantity = info.Quantity;
                if (product.IsConfirmed == (info.IsConfirmed ?? false)) return;
                product.IsConfirmed = info.IsConfirmed ?? false;
                var view = CurrentView as ProductionTaskSGIViewModel;
                if (view != null)
                {
                    view.MadeQuantity =
                        _productionTaskProducts.Where(p => p.IsConfirmed).Sum(p => p.Quantity ?? 0);
                }
            }

        }

        /// <summary>
        /// Действие при получении шк от сканера
        /// </summary>
        /// <param name="msg">сообщение с шк</param>
        private void BarcodeReceived(BarcodeMessage msg)
        {
            if (!IsActive) return;
            var docProductionProducts = GammaBase.DocProductionProducts.Include(dp => dp.DocProduction.Docs).Include(dp => dp.Products)
                .FirstOrDefault(
                    dp => dp.Products.BarCode == msg.Barcode);
            if (docProductionProducts == null) return;
            GammaBase.Entry(docProductionProducts.DocProduction.Docs).Reload();
            docProductionProducts.DocProduction.Docs.IsConfirmed = true;
            string message;
            switch (docProductionProducts.Products.ProductKindID)
            {
                case (byte)ProductKind.ProductSpool:
                    message = $"Тамбур №{docProductionProducts.Products.Number} подтвержден";
                    break;
                case (byte)ProductKind.ProductPallet:
                    message = $"Паллета №{docProductionProducts.Products.Number} подтверждена";
                    break;
                case (byte)ProductKind.ProductGroupPack:
                    message = $"Групповая упаковка №{docProductionProducts.Products.Number} подтверждена";
                    break;
                default:
                    message = $"Продукт №{docProductionProducts.Products.Number} подтвержден";
                    break;
            }
            GammaBase.SaveChanges();
            MessageBox.Show(message, "Подтверждение", MessageBoxButton.OK, MessageBoxImage.Information);
            var productionTaskProduct =
                ProductionTaskProducts.FirstOrDefault(ptp => ptp.ProductID == docProductionProducts.ProductID);
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
                    ((ProductionTaskSGIViewModel)CurrentView).PrintExampleEvent += SaveToModel;
                    break;
            }
        }

        

        public override bool SaveToModel(GammaEntities gammaBase = null)
        {
            if (!DB.HaveWriteAccess("ProductionTaskBatches")) return true;
            gammaBase = gammaBase ?? DB.GammaDb;
            var productionTaskBatch = gammaBase.ProductionTaskBatches.FirstOrDefault(p => p.ProductionTaskBatchID == ProductionTaskBatchID);
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
            return true;
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

        private void MakeProductionTaskActiveForPlace()
        {
            using (var gammaBase = DB.GammaDb)
            {
                gammaBase.MakeProductionTaskActiveForPlace(WorkSession.PlaceID, ProductionTaskBatchID);
            }
        }

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
                var checkResult = SourceSpoolsCheckResult.Correct;
                switch (BatchKind)
                {
                    case BatchKinds.SGB:
                        switch (WorkSession.PlaceGroup)
                        {
                            case PlaceGroup.PM:
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
                                        productId = gammaBase.DocProductionProducts.Where(d => d.DocID == docProduction.DocID)
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
                                //если предыдущий тамбур этой смены и не подтвержден, то открываем для редактирования
                                if (docProduction.ShiftID == WorkSession.ShiftID && !docProduction.IsConfirmed)
                                {
                                    var firstOrDefault = gammaBase.DocProductionProducts.FirstOrDefault(d => d.DocID == docProduction.DocID);
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
                            case PlaceGroup.Rw:
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
                                checkResult = SourceSpoolsCorrect(WorkSession.PlaceID, productionTaskID);
                                break;
                        }
                        if (checkResult == SourceSpoolsCheckResult.Block) return;
                        MessageManager.CreateNewProduct(docProductKind, productionTaskID, checkResult);
                        break;
                    // Действия для конвертингов
                    case BatchKinds.SGI:
                        checkResult = SourceSpoolsCorrect(WorkSession.PlaceID, productionTaskID);
                        if (checkResult == SourceSpoolsCheckResult.Block) return;
                        /*
                        var activeSourceSpools = gammaBase.GetActiveSourceSpools(WorkSession.PlaceID).ToList();
                        if (activeSourceSpools.Count == 0)
                        {
                            MessageBox.Show("Нет активных раскатов", "Не установлен тамбур", MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            return;
                        }
                        */
                            var currentDateTime = DB.CurrentDateTime;
                            var productionTask =
                                gammaBase.GetProductionTaskByBatchID(ProductionTaskBatchID,
                                    (short)PlaceGroup.Convertings).FirstOrDefault();
                            if (productionTask == null) return;
                            var baseQuantity =
                            (int)(gammaBase.C1CCharacteristics.Where(
                                c => c.C1CCharacteristicID == productionTask.C1CCharacteristicID)
                                .Select(c => c.C1CMeasureUnitsPallet.Coefficient).First() ?? 0);
                            productId = SqlGuidUtil.NewSequentialid();
                            var product = new Products()
                            {
                                ProductID = productId,
                                ProductKindID = (byte)ProductKind.ProductPallet,
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
                                    ProductionTaskID = productionTask.ProductionTaskID,
                                    HasWarnings = checkResult == SourceSpoolsCheckResult.Warning,
                                    DocProductionProducts = new List<DocProductionProducts>
                                    {
                                        new DocProductionProducts
                                        {
                                            DocID = docID,
                                            ProductID = productId,
                                            C1CNomenclatureID = (Guid)productionTask.C1CNomenclatureID,
                                            C1CCharacteristicID = (Guid)productionTask.C1CCharacteristicID,
                                            Quantity = baseQuantity
                                        }
                                    }
                                }
                            };
                            gammaBase.Docs.Add(doc);
                            var sourceSpools = gammaBase.GetActiveSourceSpools(WorkSession.PlaceID).ToList();
                            foreach (var spoolId in sourceSpools.Where(s => s != null))
                            {
                                var docWithdrawalProduct =
                                    gammaBase.DocWithdrawalProducts.Include(d => d.DocWithdrawal).Include(d => d.DocWithdrawal.Docs)
                                    .Include(d => d.DocWithdrawal.DocProduction)
                                    .FirstOrDefault(d => d.ProductID == spoolId
                                                                                        && d.Quantity == null &&
                                                                                        (d.CompleteWithdrawal == null ||
                                                                                         d.CompleteWithdrawal == false));
                                if (docWithdrawalProduct == null)
                                {
                                    var docWithdrawalid = SqlGuidUtil.NewSequentialid();
                                    docWithdrawalProduct = new DocWithdrawalProducts
                                    {
                                        DocID = docWithdrawalid,
                                        ProductID = (Guid)spoolId,
                                        DocWithdrawal = new DocWithdrawal
                                        {
                                            DocID =  docWithdrawalid,
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
                                                IsConfirmed = false
                                            }
                                        }
                                    };
                                    gammaBase.DocWithdrawalProducts.Add(docWithdrawalProduct);
                                }
                                if (docWithdrawalProduct.DocWithdrawal.DocProduction == null) docWithdrawalProduct.DocWithdrawal.DocProduction = new List<DocProduction>();
                                docWithdrawalProduct.DocWithdrawal.DocProduction.Add(doc.DocProduction);
                            }
                            gammaBase.SaveChanges();
                            ReportManager.PrintReport("Амбалаж", "Pallet", doc.DocID, false, 2);
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
                                                                               ProductKind = (ProductKind)taskProducts.ProductKindID,
                                                                               CharacteristicID = taskProducts.CharacteristicID,
                                                                               NomenclatureID = taskProducts.NomenclatureID,
                                                                               Date = taskProducts.Date,
                                                                               NomenclatureName = taskProducts.NomenclatureName,
                                                                               Number = taskProducts.Number,
                                                                               Quantity = taskProducts.Quantity,
                                                                               ProductID = taskProducts.ProductID,
                                                                               Place = taskProducts.Place,
                                                                               IsConfirmed = taskProducts.IsConfirmed,
                                                                               PlaceID = taskProducts.PlaceID,
                                                                               ShiftID = taskProducts.ShiftID
                                                                           });
        }
        private void ShowProduct()
        {
            switch (SelectedProductionTaskProduct.ProductKind)
            {
                case ProductKind.ProductSpool:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedProductionTaskProduct.ProductID);
                    break;
                case ProductKind.ProductGroupPack:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductGroupPack, SelectedProductionTaskProduct.ProductID);
                    break;
                case ProductKind.ProductPallet:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductPallet, SelectedProductionTaskProduct.ProductID);
                    break;
                default:
                    MessageBox.Show("Ошибка программы, действие не предусмотрено");
                    return;
            }
        }

        public bool ExpandProductionTaskProducts { get; private set; }

        private ItemsChangeObservableCollection<ProductInfo> _productionTaskProducts = new ItemsChangeObservableCollection<ProductInfo>();
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
        public string Title { get; private set; }

        private SourceSpoolsCheckResult SourceSpoolsCorrect(int placeId, Guid productionTaskId)
        {
            using (var gammaBase = DB.GammaDb)
            {
                var sourceSpools = gammaBase.GetActiveSourceSpools(placeId).ToList();
                if (sourceSpools.Count == 0)
                {
                    MessageBox.Show("Нет активных раскатов");
                    return SourceSpoolsCheckResult.Block;
                }
                /*
                if (!gammaBase.ProductionTaskRWCutting.Any(ptrw => ptrw.ProductionTasks.ProductionTaskBatches.FirstOrDefault().ProductionTaskBatchID == ProductionTaskBatchID))
                {
                    MessageBox.Show("В задании не указан раскрой");
                    return false;
                }
                */
                var result = gammaBase.CheckProductionTaskSourceSpools(placeId, productionTaskId).First();
                var resultMessage = result.ResultMessage;
                if (string.IsNullOrWhiteSpace(resultMessage) && !result.BlockCreation)
                    return SourceSpoolsCheckResult.Correct;
                if (result.BlockCreation)
                {
                    MessageBox.Show(resultMessage, "Проверка исходных тамбуров", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return SourceSpoolsCheckResult.Block;
                }
                var dialogResult = MessageBox.Show(resultMessage, "Проверка исходных тамбуров", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                return dialogResult == MessageBoxResult.Yes ? SourceSpoolsCheckResult.Warning : SourceSpoolsCheckResult.Block;
            }
        }
    }

}