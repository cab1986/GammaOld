// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Gamma.Models;
using System.Linq;
using Gamma.Interfaces;
using Gamma.Attributes;
using System.Windows;
using System.Data.Entity;
using Gamma.Common;
using Gamma.Entities;
using System.ComponentModel;
using System.Windows.Input;

// ReSharper disable MemberCanBePrivate.Global

namespace Gamma.ViewModels
{
    /// <summary>
    /// ViewModel для продукта
    /// </summary>
    public class DocProductViewModel : SaveImplementedViewModel, ICheckedAccess
    {
        /// <summary>
        /// Initializes a new instance of the DocProductViewModel class.
        /// </summary>
        /// <param name="msg">Сообщение, содержащее параметры</param>
        public DocProductViewModel(OpenDocProductMessage msg)
        {
            ClosingCommand = new DelegateCommand<CancelEventArgs>(Closing);
            Products product = null;
            if (msg.IsNewProduct)
            {
                if (msg.ID == null && (msg.DocProductKind == DocProductKinds.DocProductSpool ||
                                       msg.DocProductKind == DocProductKinds.DocProductUnload))
                {
                    string productKind;
                    switch (msg.DocProductKind)
                    {
                        case DocProductKinds.DocProductSpool:
                            productKind = "Тамбур";
                            break;
                        case DocProductKinds.DocProductUnload:
                            productKind = "Съем";
                            break;
                        default:
                            productKind = "Продукт";
                            break;
                    }
                    MessageBox.Show($"Нельзя создать {productKind} без задания!");
                    CloseWindow();
                    return;
                }
                IsNewDoc = true;
                var docId = SqlGuidUtil.NewSequentialid();
                Doc = new Docs
                {
                    DocID = docId,
                    DocTypeID = (int)DocTypes.DocProduction,
                    IsConfirmed = false,
                    PlaceID = WorkSession.PlaceID,
                    ShiftID = WorkSession.ShiftID,
                    UserID = WorkSession.UserID,
                    Date = DB.CurrentDateTime,
                    PrintName = WorkSession.PrintName,
                    DocProduction = new DocProduction
                    {
                        DocID = docId,
                        InPlaceID = WorkSession.PlaceID,
                        ProductionTaskID = msg.ID,
                        HasWarnings = msg.CheckResult == SourceSpoolsCheckResult.Warning
                    }
                };
                if (msg.DocProductKind != DocProductKinds.DocProductUnload)
                {
                    product = new Products
                    {
                        ProductID = SqlGuidUtil.NewSequentialid(),
                        ProductKindID =
                            msg.DocProductKind == (byte) DocProductKinds.DocProductSpool
                                ? (byte) ProductKind.ProductSpool
                                : msg.DocProductKind == DocProductKinds.DocProductGroupPack
                                    ? (byte) ProductKind.ProductGroupPack
                                    : msg.DocProductKind == DocProductKinds.DocProductPallet 
                                    ? (byte) ProductKind.ProductPallet
                                    : msg.DocProductKind == DocProductKinds.DocProductPalletR
                                    ? (byte)ProductKind.ProductPalletR
                                    : (byte) ProductKind.ProductBale
                    };
                    GammaBase.Products.Add(product);
                    Doc.DocProduction.DocProductionProducts = new List<DocProductionProducts>()
                    {
                        new DocProductionProducts()
                        {
                            ProductID = product.ProductID,
                            DocID = Doc.DocID

                        }
                    };
                }
                GammaBase.Docs.Add(Doc);
                GammaBase.SaveChanges(); // Сохранение в бд
                GammaBase.Entry(Doc).Reload(); // Получение обновленного документа(с номером из базы)
                if (product != null)
                    GammaBase.Entry(product).Reload();
            }
            else
            {
                if (msg.DocProductKind == DocProductKinds.DocProductUnload)
                {
                    Doc = GammaBase.Docs.Include(d => d.DocProduction).First(d => d.DocID == msg.ID);
                    GetDocRelations(Doc.DocID);
                }
                else
                {
                    Doc =
                            GammaBase.Docs.Include(d => d.DocProduction).First(d => d.DocProduction.DocProductionProducts.Select(dp => dp.ProductID).Contains((Guid)msg.ID) &&
                                    d.DocTypeID == (byte)DocTypes.DocProduction);
                    product =
                        GammaBase.Products.Include(p => p.ProductStates)
                            .First(p => p.ProductID == msg.ID);
                    if (product == null)
                    {
                        MessageBox.Show(@"Не удалось получить информацию о продукте");
                        CloseWindow();
                        return;
                    }
                    GammaBase.Entry(product).Reload();
                    GetProductRelations(product.ProductID);
                }
                AllowEditDoc = DB.AllowEditDoc(Doc.DocID);
            }
            DocID = Doc.DocID;
            if (product == null)
            {
                if (IsNewDoc) State = ProductState.Good;
            }
            else
            {
                ProductID = product.ProductID;

                var rest = GammaBase.Rests.FirstOrDefault(r => r.ProductID == ProductID);
                if (rest != null)
                    CurrentPlace = rest.Places.Name;
                State = rest != null || (rest == null && IsNewDoc)
                    ? (ProductState)(product.StateID ?? 0) : (ProductState?)null;
                StateName = State != null
                    ? Functions.GetEnumDescription((ProductState)State)
                    : "Списан";
                if (State == ProductState.Broke || State == ProductState.ForConversion || State == ProductState.NeedsDecision || State == ProductState.Repack)
                {
                    var docBroke = GammaBase.Docs.Join(GammaBase.DocBrokeProducts.Where(p => p.ProductID == product.ProductID)
                        , d => d.DocID, p => p.DocID, (d, p) => new
                        {
                            DocID = d.DocID,
                            Number = d.Number,
                            Date = d.Date,
                            Quantity = p.Quantity,
                            RejectionReasonID = p.C1CRejectionReasonID,
                            PlaceID = p.PlaceID
                        }).OrderBy(d => d.Date).FirstOrDefault();
                    if (docBroke != null && docBroke.DocID != Guid.Empty)
                    {
                        DocBrokeDecision = new DocBrokeDecisionViewModel(docBroke.DocID);
                        DocBrokeDecision.AddBrokeDecisionProduct(docBroke.DocID, product.ProductID, docBroke.Date, docBroke.Quantity ?? 0, docBroke.RejectionReasonID, docBroke.PlaceID, false);
                        IsVisibleBrokeTab = true;
                    }
                }
            }

            // Создаем дочернюю viewmodel в зависимости от типа изделия
            switch (msg.DocProductKind)
            {
                case DocProductKinds.DocProductSpool:
                    Title = "Тамбур";
                    Number = product?.Number;
                    Title = $"{Title} № {Number}";
                    AllowAddToBrokeAction = DB.HaveWriteAccess("DocBroke") && State == ProductState.Good ;
                    CurrentViewModel = new DocProductSpoolViewModel(ProductID);
                    break;
                case DocProductKinds.DocProductUnload:
                    Title = "Съем";
                    Number = Doc.Number;
                    Title = $"{Title} № {Number}";
                    AllowAddToBrokeAction = false;
                    CurrentViewModel = new DocProductUnloadViewModel(DocID, IsNewDoc);
                    break;
                case DocProductKinds.DocProductGroupPack:
                    Title = "Групповая упаковка";
                    Number = product?.Number;
                    Title = $"{Title} № {Number}";
                    AllowAddToBrokeAction = DB.HaveWriteAccess("DocBroke") && State == ProductState.Good;
                    CurrentViewModel = new DocProductGroupPackViewModel(DocID);                   
                    break;
                case DocProductKinds.DocProductPallet:
                    Title = "Паллета";
                    Number = product?.Number;
                    Title = $"{Title} № {Number}";
                    AllowAddToBrokeAction = DB.HaveWriteAccess("DocBroke") && State == ProductState.Good;
                    //CurrentViewModel = new DocProductPalletViewModel(DocID);
                    CurrentViewModel = new DocProductPalletViewModel(ProductID);
                    break;
                case DocProductKinds.DocProductPalletR:
                    Title = "Неполная паллета";
                    Number = product?.Number;
                    Title = $"{Title} № {Number}";
                    AllowAddToBrokeAction = DB.HaveWriteAccess("DocBroke") && State == ProductState.Good;
                    //CurrentViewModel = new DocProductPalletViewModel(DocID);
                    CurrentViewModel = new DocProductPalletViewModel(ProductID);
                    break;
                case DocProductKinds.DocProductBale:
                    Title = "Кипа";
                    Number = product?.Number;
                    Title = $"{Title} № {Number}";
                    AllowAddToBrokeAction = false;
                    CurrentViewModel = new DocProductBaleViewModel(ProductID);
                    break;
                default:
                    MessageBox.Show("Действие не предусмотрено програмой");
                    CloseWindow();
                    return;
            }
            var productionTaskBatch = GammaBase.ProductionTasks.Where(p => p.ProductionTaskID == Doc.DocProduction.ProductionTaskID).
                Select(p => p.ProductionTaskBatches.FirstOrDefault()).FirstOrDefault();
            if (productionTaskBatch != null) ProductionTaskBatchID = productionTaskBatch.ProductionTaskBatchID;
            DocDate = Doc.Date;
            IsConfirmed = Doc.IsConfirmed || !AllowEditDoc;
            PrintName = Doc.PrintName;
            ShiftID = Doc.ShiftID.ToString();
            Place = WorkSession.Places.FirstOrDefault(p => p.PlaceID == Doc.PlaceID)?.Name;
            Bars = ((IBarImplemented) CurrentViewModel).Bars;
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
//            Messenger.Default.Register<ParentSaveMessage>(this, SaveToModel);
            ActivatedCommand = new DelegateCommand(() => IsActive = true);
            DeactivatedCommand = new DelegateCommand(() => IsActive = false);
            OpenProductionTaskCommand = new DelegateCommand(OpenProductionTask, () => ProductionTaskBatchID != null);
            OpenProductRelationCommand = new DelegateCommand(OpenProductRelation, () => SelectedRelation != null);
            AddToDocBrokeCommand = new DelegateCommand(AddToDocBroke, () => IsValid);
            Messenger.Default.Register<BarcodeMessage>(this, BarcodeReceived);
            IsReadOnly = !DB.HaveWriteAccess("DocProductionProducts") || (IsConfirmed && IsValid);
        }

        public bool AllowAddToBrokeAction { get; set; }
        public DelegateCommand AddToDocBrokeCommand { get; private set; }

        private void AddToDocBroke()
        {
            UIServices.SetBusyState();
            SaveToModel();
            if (Doc == null) return;
            Guid? productId = null;
            if (CurrentViewModel is DocProductSpoolViewModel)
            {
                productId = ((DocProductSpoolViewModel) CurrentViewModel).ProductId;
            }
            else if (CurrentViewModel is DocProductGroupPackViewModel)
            {
                productId = ((DocProductGroupPackViewModel)CurrentViewModel).ProductId;
            }
            else if (CurrentViewModel is DocProductPalletViewModel)
            {
                productId = ((DocProductPalletViewModel)CurrentViewModel).ProductId;
            }
            if (productId == null) return;
            using (var gammaBase = DB.GammaDbWithNoCheckConnection)
            {
                if (!gammaBase.Rests.Any(r => r.ProductID == productId && r.Quantity > 0))
                {
                    MessageBox.Show("Нельзя актировать продукт, которого нет на остатках");
                    return;
                }
                if (
                    gammaBase.SourceSpools.Any(
                        s =>
                            s.Unwinder1Spool == productId || s.Unwinder2Spool == productId ||
                            s.Unwinder3Spool == productId || s.Unwinder4Spool == productId))
                {
                    MessageBox.Show(
                        "Нельзя актировать тамбур, который находится на раскате. Актируйте тамбур при снятии или снимите тамбур с раската и затем актируйте",
                        "Ошибка актирования", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                var docProduction =
                    gammaBase.Docs.FirstOrDefault(
                        d => d.DocProduction.DocProductionProducts.Any(dp => dp.ProductID == productId));
                if (docProduction == null)
                {
                    MessageBox.Show("Непредвиденная ошибка при добавлении в акт! Не удалось получить информацию о продукте.", "Ошибка актирования",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                /*
                var docBrokeId =
                    gammaBase.Docs.FirstOrDefault(
                        d => d.DocTypeID == (int)DocTypes.DocBroke && !d.IsConfirmed 
                            && d.PlaceID == WorkSession.PlaceID
                            && (((d.DocBroke.IsInFuturePeriod ?? false) && (docProduction.PlaceID != WorkSession.PlaceID))
                                || ((!d.DocBroke.IsInFuturePeriod ?? true) && (docProduction.PlaceID == WorkSession.PlaceID))
                                || !WorkSession.IsProductionPlace
                               )
                           )?.DocID;
                */
                var docBrokeId = new ObservableCollection<Guid>
                (
                    from pt in gammaBase.GetDocBrokeID((int)WorkSession.PlaceID, WorkSession.UserID, (int)WorkSession.ShiftID, (int)docProduction.PlaceID, (bool)WorkSession.IsProductionPlace)
                    select new Guid(pt.ToString())
                );
                if (docBrokeId.Count > 0)
                {
                    MessageBox.Show("Есть незакрытый акт. Данный продукт будет добавлен в этот акт");
                    MessageManager.OpenDocBroke((Guid)docBrokeId[0], productId);
                }
                else
                {
                    MessageManager.OpenDocBroke(SqlGuidUtil.NewSequentialid(), productId, docProduction.PlaceID != WorkSession.PlaceID);
                }
            }
        }

        public Guid DocID { get; set; }
        public Guid ProductID { get; set; }
        public ProductState? State { get; set; }
        public string StateName { get; set; }
        public string PrintName { get; set; }
        public string Place { get; set; }
        public string ShiftID { get; set; }
        public string CurrentPlace { get; set; }

        public DocBrokeDecisionViewModel DocBrokeDecision { get; set; }
        public bool IsVisibleBrokeTab { get; set; } = false;

        private void GetProductRelations(Guid productId)
        {
            ProductRelations = new ObservableCollection<ProductRelation>
                (
                from prel in GammaBase.GetProductRelations(productId)
                select new ProductRelation
                {
                    Description = prel.Description,
                    Date = prel.Date,
                    DocID = prel.DocID,
                    Number = prel.Number,
                    ProductID = prel.ProductID,
                    ProductKindID = prel.ProductKindID,
                    DocType = (DocTypes)(prel.DocTypeID??0)
                }
                );
        }
        
        private void GetDocRelations(Guid docId)
        {
            ProductRelations = new ObservableCollection<ProductRelation>
                (
                from prel in GammaBase.GetDocRelations(docId)
                select new ProductRelation
                {
                    Date = prel.Date,
                    DocID = prel.DocID,
                    Number = prel.Number,
                    ProductID = prel.ProductID,
                    Description = prel.Description,
                    ProductKindID = prel.ProductKindID,
                    DocType = (DocTypes)(prel.DocTypeID??0)
                }
                );
                
        }

/*
        private void SaveToModel(ParentSaveMessage msg)
        {
            SaveToModel();
        }
*/
        private void BarcodeReceived(BarcodeMessage msg)
        {
            if (!IsActive) return;
            if (Doc == null) return;
            if (CurrentViewModel is DocProductSpoolViewModel)
            {
                var productid = (from p in GammaBase.Products
                                 where p.BarCode == msg.Barcode
                                 select p.ProductID).FirstOrDefault();
                if (productid == new Guid()) return;
                if (GammaBase.DocProductionProducts.Where(d => d.DocID == Doc.DocID && d.ProductID == productid).Select(d => d).FirstOrDefault() != null)
                    IsConfirmed = false;
            }
            else
            {
                (CurrentViewModel as DocProductGroupPackViewModel)?.AddSpool(msg.Barcode);
            }
        }
        private bool IsNewDoc { get; set; }

        private void OpenProductionTask()
        {
            var batchKind = BatchKinds.SGB;
            if (CurrentViewModel is DocProductPalletViewModel) batchKind = BatchKinds.SGI;
            MessageManager.OpenProductionTask(batchKind, (Guid)ProductionTaskBatchID);
        }

        public ObservableCollection<BarViewModel> Bars
        {
            get
            {
                return _bars;
            }
            set
            {
                _bars = value;
                RaisePropertyChanged("Bars");
            }
        }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public DateTime DocDate { get; set; }
        private bool _isConfirmed;

        private bool AllowEditDoc { get; set; } = true;

        [UIAuth(UIAuthLevel.ReadOnly)]
        public bool IsConfirmed 
        {
            get
            {
                return _isConfirmed;
            }
            set
            {
                if (_isConfirmed)
                    if (!AllowEditDoc)
                    {
                        MessageBox.Show("Правка невозможна. Продукция уже в выработке или с ней связаны другие документы");
                        return;
                    }
                    else
                    {
                        if (Doc != null && GammaBase.Docs.Any(p => p.DocID == Doc.DocID && p.DocCloseShift.Any(d => !d.IsConfirmed)))
                        {
                            var docCloseShift = Doc.DocCloseShift.FirstOrDefault();
                            MessageBox.Show("Продукт № " + Number + " будет удален из рапорта закрытия смены № "+ docCloseShift?.Number + " от " +docCloseShift?.Date.ToString() + " Смена " +docCloseShift?.ShiftID.ToString() + ". ОБЯЗАТЕЛЬНО откройте рапорт закрытия смены и заполните повторно!");
                            var delResult = GammaBase.DeleteDocFromDocCloseShiftDocs(Doc.DocID).FirstOrDefault();
                            if (delResult != "")
                            {
                                MessageBox.Show(delResult, "Удалить не удалось", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                                return;
                            }
                        }
                    };
                if (value && !IsValid)
                    _isConfirmed = false;
                else 
            	    _isConfirmed = value;
                if (Doc != null) MessageManager.DocChanged(Doc.DocID, IsConfirmed);
                RaisePropertyChanged("IsConfirmed");
            }
        }
        private string _number;
        public string Number
        {
            get
            {
                return _number;
            }
            set
            {
            	_number = value;
                RaisePropertyChanged("Number");
            }
        }
        private Guid? _productionTaskBatchID;
        private Guid? ProductionTaskBatchID
        {
            get { return _productionTaskBatchID; }
            set
            {
                _productionTaskBatchID = value;
                if (value == null) return;
                var pinfo = (from pt in GammaBase.ProductionTaskBatches
                     where pt.ProductionTaskBatchID == value
                     select new 
                     {
                         pt.Number, pt.Date 
                     }).FirstOrDefault();
                if (pinfo != null)
                    ProductionTaskInfo = $"Задание №{pinfo.Number} от {pinfo.Date}";
            }
        }
        private string _productionTaskInfo;
        public string ProductionTaskInfo 
        {
            get { return _productionTaskInfo; }
            set
            {
                _productionTaskInfo = value;
                RaisePropertyChanged("ProductionTaskInfo");
            }
        }
        private SaveImplementedViewModel _currentViewModel;
        public SaveImplementedViewModel CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                _currentViewModel = value;
                RaisePropertyChanged("CurrentViewModel");
            }
        }
        public DelegateCommand OpenProductionTaskCommand { get; private set; }
        
        private ObservableCollection<BarViewModel> _bars;
      
        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != (CurrentViewModel as IBarImplemented)?.VMID) return;
            if (!IsValid)
            {
                MessageBox.Show("Не заполнены некоторые обязательные поля!", "Поля не заполнены", MessageBoxButton.OK,
                    MessageBoxImage.Asterisk);
                return;
            }
            if (!SaveToModel()) return;
            var reportName = WorkSession.Reports.Where(r => r.ReportID == msg.ReportID).Select(r => r.Name).FirstOrDefault();
            var parentId = WorkSession.Reports.Where(r => r.ReportID == msg.ReportID).Select(r => r.ParentID).FirstOrDefault();
            var parentName = WorkSession.Reports.Where(r => r.ReportID == parentId).Select(r => r.Name).FirstOrDefault();
            if (reportName != null && parentName != null)
                if (reportName == "Амбалаж" && parentName == "GroupPacks")
                {
                    if (Doc.DocID != null && !IsConfirmed)
                    {
                        IsConfirmed = true;
                        var grouppackViewModel = CurrentViewModel as DocProductGroupPackViewModel;
                        grouppackViewModel.IsAllowDelete = false;
                        if (!SaveToModel()) return;
                    }
                }
            
            if (CurrentViewModel is DocProductPalletViewModel)
            {
                var palletViewModel = CurrentViewModel as DocProductPalletViewModel;
                var palletKindID = GammaBase.Products.Where(r => r.ProductID == palletViewModel.ProductId).Select(r => r.ProductKindID).FirstOrDefault();
                if (palletKindID == (byte)ProductKind.ProductPalletR  && reportName != "Неполная паллета" && parentName == "Pallet")
                {
                    MessageBox.Show("Нельзя печатать этикетку полной паллеты на неполную!", "Ошибка печати неполной паллеты", MessageBoxButton.OK,
                        MessageBoxImage.Asterisk);
                    return;
                }
                if (palletKindID == (byte)ProductKind.ProductPallet && reportName == "Неполная паллета" && parentName == "Pallet")
                {
                    MessageBox.Show("Нельзя печатать этикетку неполной паллеты на полную!", "Ошибка печати неполной паллеты", MessageBoxButton.OK,
                        MessageBoxImage.Asterisk);
                    return;
                }
            }
            using (var gammaBase = DB.GammaDb)
                {
                    var state =
                        (ProductState) (gammaBase.vProductsInfo.FirstOrDefault(p => p.DocID == Doc.DocID 
                            && p.NomenclatureKindID == 1)?.StateID ?? 0);
                    if (state == ProductState.Limited)
                    {
                        MessageBox.Show(@"Продукт ограниченно годен. Не забудьте наклеить синий амбалаж",
                            "Ограниченно годен",
                            MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }
                }
            var spoolViewModel = CurrentViewModel as DocProductSpoolViewModel;
            ReportManager.PrintReport(msg.ReportID, spoolViewModel?.ProductId ?? Doc.DocID);
        }

        public override bool SaveToModel()
        {
            if (IsReadOnly && IsConfirmed) return true;
            Doc.Date = DocDate;
            Doc.IsConfirmed = IsConfirmed;
            GammaBase.SaveChanges();
            return IsReadOnly || CurrentViewModel.SaveToModel(Doc.DocID);
        }

        private Docs Doc { get; set; }

        public override sealed bool IsValid => base.IsValid && (CurrentViewModel?.IsValid ?? false);

        public bool IsReadOnly { get; set; }

        public override bool CanSaveExecute()
        {
            return IsValid && (CurrentViewModel?.CanSaveExecute() ?? false) && DB.HaveWriteAccess("DocProduction");
        }
        public string Title { get; set; }
        private bool IsActive { get; set; }
        public DelegateCommand ActivatedCommand { get; private set; }
        public DelegateCommand DeactivatedCommand { get; private set; }
        public DelegateCommand OpenProductRelationCommand { get; private set; }

        private void OpenProductRelation()
        {
            if (SelectedRelation == null) return;
            switch (SelectedRelation.DocType)
            {
                case DocTypes.DocBroke:
                    MessageManager.OpenDocBroke(SelectedRelation.DocID);
                    break;
                case DocTypes.DocShipment:
                    MessageManager.OpenDocShipmentOrder(SelectedRelation.DocID);
                    break;
                case DocTypes.DocMovement:
                    MessageManager.OpenDocMovement(SelectedRelation.DocID);
                    break;
                case DocTypes.DocWithdrawal:
                    MessageManager.OpenDocWithdrawal(SelectedRelation.DocID);
                    break;
                case DocTypes.DocUtilization:
                    MessageManager.OpenDocWithdrawal(SelectedRelation.DocID);
                    break;
                case DocTypes.DocSpecificstionQuantity:
                    MessageManager.OpenDocWithdrawal(SelectedRelation.DocID);
                    break;
                case DocTypes.DocRepack:
                    MessageManager.OpenDocRepack(SelectedRelation.DocID);
                    break;
                default:
                    switch (SelectedRelation.ProductKindID)
                    {
                        case (int)ProductKind.ProductSpool:
                            MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedRelation.ProductID);
                            break;
                        case (int)ProductKind.ProductGroupPack:
                            MessageManager.OpenDocProduct(DocProductKinds.DocProductGroupPack, SelectedRelation.ProductID);
                            break;
                        case (int)ProductKind.ProductPallet:
                            MessageManager.OpenDocProduct(DocProductKinds.DocProductPallet, SelectedRelation.ProductID);
                            break;
                        case (int)ProductKind.ProductPalletR:
                            MessageManager.OpenDocProduct(DocProductKinds.DocProductPalletR, SelectedRelation.ProductID);
                            break;
                    }
                    break;
            }
        }

        private ObservableCollection<ProductRelation> _productRelations;
        public ObservableCollection<ProductRelation> ProductRelations
        {
            get
            {
                return _productRelations;
            }
            set
            {
            	_productRelations = value;
                RaisePropertyChanged("ProductRelations");
            }
        }
        public ProductRelation SelectedRelation { get; set; }

        private int _selectedTabIndex { get; set; }
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (_selectedTabIndex != 2 && value == 2)
                {
                    if (DocBrokeDecision != null && DocBrokeDecision.GetService_SetBrokePlaceDialog == null)
                    {
                        var dialogService = GetService<IDialogService>("SetBrokePlaceDialog");
                        DocBrokeDecision.GetService_SetBrokePlaceDialog = dialogService;
                    }
                }
                if (_selectedTabIndex == 2 && value != 2)
                {
                    if (DocBrokeDecision?.SelectedBrokeDecisionProduct != null)
                    {
                        DocBrokeDecision.SelectedBrokeDecisionProduct = null;
                    }
                    GetProductRelations(ProductID);
                }
                _selectedTabIndex = value;

                DB.AddLogMessageInformation("Выбрана вкладка " + (value == 0 ? "Основной" : value == 1 ? "Связи" : value == 2 ? "Решения о браке" : "") + " в Карточке продукта DocID", "SET SelectedTabIndex ='" + value + "')", Doc.DocID);
   }
        }

        public override void Dispose()
        {
            base.Dispose();
            (CurrentViewModel as IDisposable)?.Dispose();
            CurrentViewModel = null;
        }

        public ICommand ClosingCommand { get; private set; }

        void Closing(CancelEventArgs e)
        {
            if (DocBrokeDecision?.IsChanged ?? false)
            {
                if (Functions.ShowMessageQuestion("Закрытие карточки продукта: " + Environment.NewLine + "Есть несохраненные решения по браку! Нажмите Да, чтобы сохранить, или Нет, чтобы закрыть без сохранения?", "QUEST ClosingDocProduct DocID = '" + Doc.DocID + "'", Doc.DocID)
                    == MessageBoxResult.Yes)
                {
                    DocBrokeDecision?.SaveToModel();
                }
                return;
            }


            //if (MessageBox.Show("Close?", "", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.No)
            //{
            //    e.Cancel = true;
            //}
        }
    }
}