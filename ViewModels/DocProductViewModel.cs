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
            StartInitialization();
            IsReadOnly = !DB.HaveWriteAccess("DocProductionProducts") 
                //|| (IsConfirmed && IsValid) 
                || !msg.AllowEdit;
            LoadData(msg.IsNewProduct, msg.ID, msg.DocProductKind, msg.CheckResult);
            ClosingCommand = new DelegateCommand<CancelEventArgs>(Closing);
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
//            Messenger.Default.Register<ParentSaveMessage>(this, SaveToModel);
            ActivatedCommand = new DelegateCommand(() => IsActive = true);
            DeactivatedCommand = new DelegateCommand(() => IsActive = false);
            OpenProductionTaskCommand = new DelegateCommand(OpenProductionTask, () => ProductionTaskBatchID != null);
            OpenProductRelationCommand = new DelegateCommand(OpenProductRelation, () => SelectedRelation != null);
            AddToDocBrokeCommand = new DelegateCommand(AddToDocBroke, () => IsValid);
            Messenger.Default.Register<BarcodeMessage>(this, BarcodeReceived);
            EndInitialization();
        }

        public bool AllowAddToBrokeAction { get; set; }
        public DelegateCommand AddToDocBrokeCommand { get; private set; }

        private void LoadData(bool isNewProduct, Guid? iD, DocProductKinds docProductKind, SourceSpoolsCheckResult? checkResult = null)
        {
            Products product = null;
            if (isNewProduct)
            {
                DB.AddLogMessageInformation("Загрузка данных: новый продукт",
                        "LoadData in DocProductViewModel: IsNewProduct", DocID, ProductID);
                if (iD == null && (docProductKind == DocProductKinds.DocProductSpool ||
                                       docProductKind == DocProductKinds.DocProductUnload))
                {
                    string productKind;
                    switch (docProductKind)
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
                    Functions.ShowMessageInformation("Невозможно создать " + productKind + " без задания!",
                        "Error LoadData in DocProductViewModel: Cannot create " + productKind + " without a job!", DocID, ProductID);
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
                    //Date = DB.CurrentDateTime,
                    PrintName = WorkSession.PrintName,
                    DocProduction = new DocProduction
                    {
                        DocID = docId,
                        InPlaceID = WorkSession.PlaceID,
                        ProductionTaskID = iD,
                        HasWarnings = checkResult == SourceSpoolsCheckResult.Warning
                    }
                };
                if (docProductKind != DocProductKinds.DocProductUnload)
                {
                    product = new Products
                    {
                        ProductID = SqlGuidUtil.NewSequentialid(),
                        ProductKindID =
                            docProductKind == (byte)DocProductKinds.DocProductSpool
                                ? (byte)ProductKind.ProductSpool
                                : docProductKind == DocProductKinds.DocProductGroupPack
                                    ? (byte)ProductKind.ProductGroupPack
                                    : docProductKind == DocProductKinds.DocProductPallet
                                    ? (byte)ProductKind.ProductPallet
                                    : docProductKind == DocProductKinds.DocProductPalletR
                                    ? (byte)ProductKind.ProductPalletR
                                    : (byte)ProductKind.ProductBale
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
                //GammaBase.SaveChanges(); // Сохранение в бд
                //GammaBase.Entry(Doc).Reload(); // Получение обновленного документа(с номером из базы)
                //if (product != null)
                //    GammaBase.Entry(product).Reload();
                SetIsChanged(true);
            }
            else
            {
                DB.AddLogMessageInformation("Загрузка данных: существующий продукт",
                        "LoadData in DocProductViewModel: not IsNewProduct", DocID, ProductID);

                if (docProductKind == DocProductKinds.DocProductUnload)
                {
                    Doc = GammaBase.Docs.Include(d => d.DocProduction).First(d => d.DocID == iD);
                    GetDocRelations(Doc.DocID);
                }
                else
                {
                    Doc =
                            GammaBase.Docs.Include(d => d.DocProduction).First(d => d.DocProduction.DocProductionProducts.Select(dp => dp.ProductID).Contains((Guid)iD) &&
                                    d.DocTypeID == (byte)DocTypes.DocProduction);
                    product =
                        GammaBase.Products.Include(p => p.ProductStates)
                            .First(p => p.ProductID == iD);
                    if (product == null)
                    {
                        Functions.ShowMessageError(@"Не удалось получить информацию о продукте","Error LoadDate in DocProductViewModel: product is null", DocID, ProductID);
                        CloseWindow();
                        return;
                    }
                    GammaBase.Entry(product).Reload();
                    GetProductRelations(product.ProductID);
                }
                AllowEditDoc = DB.AllowEditDoc(Doc.DocID);
                DocDate = Doc.Date;
            }
            DocID = Doc.DocID;
            if (product == null)
            {
                if (IsNewDoc)
                    State = ProductState.Good;
                else if (docProductKind == DocProductKinds.DocProductUnload)
                {
                    var dp = GammaBase.DocProductionProducts.Where(p => p.DocID == DocID).Select(p => p.ProductID).ToList();
                    if (!GammaBase.Products.Any(d => dp.Contains(d.ProductID) && (d.StateID ?? 0) > 0))
                    {
                        State = ProductState.Good;
                    }
                }
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

            ProductionTaskID = Doc.DocProduction.ProductionTaskID;
            var productionTaskBatch = GammaBase.ProductionTasks.Where(p => p.ProductionTaskID == ProductionTaskID).
                Select(p => p.ProductionTaskBatches.FirstOrDefault()).FirstOrDefault();
            if (productionTaskBatch != null) ProductionTaskBatchID = productionTaskBatch.ProductionTaskBatchID;
            
            IsConfirmed = Doc.IsConfirmed || !AllowEditDoc;
            PrintName = Doc.PrintName;
            ShiftID = Doc.ShiftID.ToString();
            Place = WorkSession.Places.FirstOrDefault(p => p.PlaceID == Doc.PlaceID)?.Name;
            DocProductKind = docProductKind;
            //if (CurrentViewModel != null) Messenger.Default.Unregister(CurrentViewModel);
            // Создаем дочернюю viewmodel в зависимости от типа изделия
            switch (docProductKind)
            {
                case DocProductKinds.DocProductSpool:
                    Title = "Тамбур";
                    Number = product?.Number;
                    Title = $"{Title} № {Number}";
                    AllowAddToBrokeAction = DB.HaveWriteAccess("DocBroke") && State == ProductState.Good;
                    if (CurrentViewModel == null)
                        CurrentViewModel = new DocProductSpoolViewModel(ProductID, DocID, IsReadOnly || IsConfirmed, ProductionTaskID, Doc.PlaceID);
                    else
                        (CurrentViewModel as DocProductSpoolViewModel).UpdateViewModel(ProductID, DocID, IsReadOnly || IsConfirmed, ProductionTaskID, Doc.PlaceID);
                    break;
                case DocProductKinds.DocProductUnload:
                    Title = "Съем";
                    Number = Doc.Number;
                    Title = $"{Title} № {Number}";
                    AllowAddToBrokeAction = DB.HaveWriteAccess("DocBroke") && State == ProductState.Good;
                    if (CurrentViewModel == null)
                        CurrentViewModel = new DocProductUnloadViewModel(DocID, IsReadOnly || IsConfirmed, this, ProductionTaskID, Doc.PlaceID, IsNewDoc);
                    else
                        (CurrentViewModel as DocProductUnloadViewModel).UpdateViewModel(DocID, IsReadOnly || IsConfirmed, this, ProductionTaskID, Doc.PlaceID, IsNewDoc);
                    break;
                case DocProductKinds.DocProductGroupPack:
                    Title = "Групповая упаковка";
                    Number = product?.Number;
                    Title = $"{Title} № {Number}";
                    AllowAddToBrokeAction = DB.HaveWriteAccess("DocBroke") && State == ProductState.Good;
                    if (CurrentViewModel == null)
                        CurrentViewModel = new DocProductGroupPackViewModel(DocID, IsReadOnly || IsConfirmed);
                    else
                        (CurrentViewModel as DocProductGroupPackViewModel).UpdateViewModel(DocID, IsReadOnly || IsConfirmed);
                    break;
                case DocProductKinds.DocProductPallet:
                    Title = "Паллета";
                    Number = product?.Number;
                    Title = $"{Title} № {Number}";
                    AllowAddToBrokeAction = DB.HaveWriteAccess("DocBroke") && State == ProductState.Good;
                    //CurrentViewModel = new DocProductPalletViewModel(DocID);
                    CurrentViewModel = new DocProductPalletViewModel(ProductID, DocID, IsReadOnly || IsConfirmed);
                    break;
                case DocProductKinds.DocProductPalletR:
                    Title = "Неполная паллета";
                    Number = product?.Number;
                    Title = $"{Title} № {Number}";
                    AllowAddToBrokeAction = DB.HaveWriteAccess("DocBroke") && State == ProductState.Good;
                    //CurrentViewModel = new DocProductPalletViewModel(DocID);
                    CurrentViewModel = new DocProductPalletViewModel(ProductID, DocID, IsReadOnly || IsConfirmed);
                    break;
                case DocProductKinds.DocProductBale:
                    Title = "Кипа";
                    Number = product?.Number;
                    Title = $"{Title} № {Number}";
                    AllowAddToBrokeAction = false;
                    CurrentViewModel = new DocProductBaleViewModel(ProductID);
                    break;
                default:
                    Functions.ShowMessageError(@"Неизвестный вид продукта", "Error LoadDate in DocProductViewModel: unknown DocProductKind", DocID, ProductID);
                    CloseWindow();
                    return;
            }
            Bars = ((IBarImplemented)CurrentViewModel).Bars;
        }

        private void AddToDocBroke()
        {
            DB.AddLogMessageInformation("Выбран пункт Добавить в акт", "AddToDocBroke in DocProductViewModel", DocID, ProductID);
            UIServices.SetBusyState();
            if (!SaveToModel(true))
            {
                return;
            }
            if (Doc == null) return;
            //Guid? productId = null;
            List<Guid?> productIDs = new List<Guid?>();
            if (CurrentViewModel is DocProductSpoolViewModel)
            {
                productIDs.Add(((DocProductSpoolViewModel) CurrentViewModel).ProductId);
            }
            else if (CurrentViewModel is DocProductGroupPackViewModel && ((DocProductGroupPackViewModel)CurrentViewModel).ProductId != null)
            {
                productIDs.Add((Guid)((DocProductGroupPackViewModel)CurrentViewModel).ProductId);
            }
            else if (CurrentViewModel is DocProductPalletViewModel && ((DocProductPalletViewModel)CurrentViewModel).ProductId != null)
            {
                productIDs.Add((Guid)((DocProductPalletViewModel)CurrentViewModel).ProductId);
            }
            else if (CurrentViewModel is DocProductUnloadViewModel && ((DocProductUnloadViewModel)CurrentViewModel).UnloadSpools?.Count() > 0)
            {
                if (((DocProductUnloadViewModel)CurrentViewModel).UnloadSpools.Any(s => s.Checked && s.Weight <= 1))
                {
                    Functions.ShowMessageInformation("Внимание, акт не создан! Требуется указать правильный вес для всех рулонов!", "Error AddToDocBroke in DocProductViewModel: exist spool with weight <= 1 ", DocID);
                }
                else
                {
                    foreach (var spool in ((DocProductUnloadViewModel)CurrentViewModel).UnloadSpools.Where(s => s.Checked))
                    {
                        productIDs.Add(spool.ProductID);
                    }
                    if (GammaBase.Products.Any(p => productIDs.Contains(p.ProductID) && (p.StateID ?? 0) > 0))
                    {
                        Functions.ShowMessageInformation("Внимание, акт не создан! Не у всех рулонов качество Годный!", "Error AddToDocBroke in DocProductViewModel: exist spool with state != Good ", DocID);
                        productIDs.Clear();
                    }
                }
            }
            if (productIDs.Count() == 0) return;
            using (var gammaBase = DB.GammaDbWithNoCheckConnection)
            {
                if (!gammaBase.Rests.Any(r => productIDs.Contains(r.ProductID) && r.Quantity > 0))
                {
                    Functions.ShowMessageError(@"Нельзя актировать продукт, которого нет на остатках", "Error AddToDocBroke in DocProductViewModel: product not exist in rest - ProductIDs" + productIDs.ToString(), DocID, ProductID);
                    return;
                }
                if (
                    gammaBase.SourceSpools.Any(
                        s =>
                            (s.Unwinder1Spool != null && productIDs.Contains((Guid)s.Unwinder1Spool)) || (s.Unwinder2Spool != null && productIDs.Contains((Guid)s.Unwinder2Spool)) ||
                            (s.Unwinder3Spool != null && productIDs.Contains((Guid)s.Unwinder3Spool)) || (s.Unwinder4Spool != null && productIDs.Contains((Guid)s.Unwinder4Spool))))
                {
                    Functions.ShowMessageError(@"Нельзя актировать тамбур, который находится на раскате. Актируйте тамбур при снятии или снимите тамбур с раската и затем актируйте",
                        "Error AddToDocBroke in DocProductViewModel: product is used on place - ProductIDs" + productIDs.ToString(), DocID, ProductID);
                    return;
                }
                var docProduction =
                    gammaBase.Docs.FirstOrDefault(
                        d => d.DocProduction.DocProductionProducts.Any(dp => productIDs.Contains(dp.ProductID)));
                if (docProduction == null)
                {
                    Functions.ShowMessageError(@"Непредвиденная ошибка при добавлении в акт! Не удалось получить информацию о продукте.",
                        "Error AddToDocBroke in DocProductViewModel: Could not retrieve product information - ProductIDs" + productIDs.ToString(), DocID, ProductID);
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
                    Functions.ShowMessageInformation(@"Есть незакрытый акт. Данный продукт будет добавлен в этот акт",
                                "Information AddToDocBroke in DocProductViewModel: Exist not closed doc broke", DocID, ProductID);
                    MessageManager.OpenDocBroke((Guid)docBrokeId[0], productIDs);
                }
                else
                {
                    MessageManager.OpenDocBroke(SqlGuidUtil.NewSequentialid(), productIDs, docProduction.PlaceID != WorkSession.PlaceID);
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

        private DateTime? _docDate { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public DateTime? DocDate 
        {
            get
            {
                return _docDate;
            }
            set
            {
                _docDate = value;
                RaisePropertyChanged("DocDate");
                if (!GetInitialization) SetIsChanged(true);
            }
        }

        private bool _isConfirmed;
        private DocProductKinds DocProductKind { get; set; }
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
                if (!GetInitialization)
                    DB.AddLogMessageInformation("Начало изменения Подтвержден на " + value + " в паллете ProductID", "Start SET IsConfirmed in DocProductViewModel: value = " + value);
                var prevIsConfirmed = _isConfirmed;
                if (_isConfirmed)
                    if (!AllowEditDoc)
                    {
                        Functions.ShowMessageInformation(@"Правка невозможна. Продукция уже в выработке или с ней связаны другие документы",
                                "Information edit IsConfirmed in DocProductViewModel: not edit value - product in other docs");
                        return;
                    }
                    else
                    {
                        if (Doc != null && GammaBase.Docs.Any(p => p.DocID == Doc.DocID && p.DocCloseShift.Any(d => !d.IsConfirmed)))
                        {
                            var docCloseShift = Doc.DocCloseShift.FirstOrDefault();
                            Functions.ShowMessageInformation("Продукт № " + Number + " будет удален из рапорта закрытия смены № " + docCloseShift?.Number + " от " + docCloseShift?.Date.ToString() + " Смена " + docCloseShift?.ShiftID.ToString() + ". ОБЯЗАТЕЛЬНО откройте рапорт закрытия смены и заполните повторно!",
                                "Information edit IsConfirmed in DocProductViewModel: product deleted from doccloseshift", DocID, ProductID);
                            var delResult = GammaBase.DeleteDocFromDocCloseShiftDocs(Doc.DocID).FirstOrDefault();
                            if (delResult != "")
                            {
                                Functions.ShowMessageError("Ошибка при удалении: "+ delResult,
                                "Error DeleteDocFromDocCloseShiftDocs - IsConfirmed in DocProductViewModel: " + delResult, DocID, ProductID);
                                return;
                            }
                        }
                    };
                if (value && !IsValid)
                    _isConfirmed = false;
                else 
            	    _isConfirmed = value;
                if (prevIsConfirmed != _isConfirmed && DocID != null) MessageManager.DocChanged(DocID, IsReadOnly || IsConfirmed);
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    //if (CurrentViewModel is DocProductUnloadViewModel)
                    //    (CurrentViewModel as DocProductUnloadViewModel).SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменено Подтвержден на " + value + " в паллете ProductID", "SET IsConfirmed in DocProductViewModel: value = " + value, DocID, ProductID);
                }
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
                if (!GetInitialization) SetIsChanged(true);
            }
        }
        private Guid? ProductionTaskID { get; set; }

        private Guid? _productionTaskBatchID;
        private Guid? ProductionTaskBatchID
        {
            get { return _productionTaskBatchID; }
            set
            {
                _productionTaskBatchID = value;
                if (!GetInitialization) SetIsChanged(true);
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
                if (!GetInitialization) SetIsChanged(true);
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
                Functions.ShowMessageInformation("Не заполнены некоторые обязательные поля!",
                "Information PrintReport in DocProductViewModel: not IsValid", DocID, ProductID);
                return;
            }
            if (!SaveToModel(true))
            {
                DB.AddLogMessageInformation("Печать не произведена (сохранить продукт не удалось)","Not printed in DocProductViewModel: SaveToModel return false",DocID, ProductID);
                return;
            }
            var reportName = WorkSession.Reports.Where(r => r.ReportID == msg.ReportID).Select(r => r.Name).FirstOrDefault();
            var parentId = WorkSession.Reports.Where(r => r.ReportID == msg.ReportID).Select(r => r.ParentID).FirstOrDefault();
            var parentName = WorkSession.Reports.Where(r => r.ReportID == parentId).Select(r => r.Name).FirstOrDefault();
            if (reportName != null && parentName != null)
                if (reportName == "Амбалаж" && parentName == "GroupPacks")
                {
                    if (Doc.DocID != null && !IsConfirmed)
                    {
                        IsConfirmed = true;
                        Doc.IsConfirmed = true;
                        IsReadOnly = true;
                        var grouppackViewModel = CurrentViewModel as DocProductGroupPackViewModel;
                        grouppackViewModel.IsAllowDelete = false;
                        GammaBase.SaveChanges();
                    }
                }
            
            if (CurrentViewModel is DocProductPalletViewModel)
            {
                var palletViewModel = CurrentViewModel as DocProductPalletViewModel;
                var palletKindID = GammaBase.Products.Where(r => r.ProductID == palletViewModel.ProductId).Select(r => r.ProductKindID).FirstOrDefault();
                if (palletKindID == (byte)ProductKind.ProductPalletR  && reportName != "Неполная паллета" && parentName == "Pallet")
                {
                    Functions.ShowMessageInformation("Нельзя печатать этикетку полной паллеты на неполную!",
                        "Information PrintReport in DocProductViewModel: Do not print a full pallet label on an incomplete pallet", DocID, ProductID);
                    return;
                }
                if (palletKindID == (byte)ProductKind.ProductPallet && reportName == "Неполная паллета" && parentName == "Pallet")
                {
                    Functions.ShowMessageInformation("Нельзя печатать этикетку неполной паллеты на полную!",
                        "Information PrintReport in DocProductViewModel: Do not print an incomplete pallet label on the full pallet", DocID, ProductID);
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
                        Functions.ShowMessageInformation("Продукт ограниченно годен. Не забудьте наклеить синий амбалаж",
                            "Information PrintReport in DocProductViewModel: Product is of limited use", DocID, ProductID);
                }
            }
            var spoolViewModel = CurrentViewModel as DocProductSpoolViewModel;
            ReportManager.PrintReport(msg.ReportID, spoolViewModel?.ProductId ?? Doc.DocID);
        }

        public Docs GetDoc => Doc;        

        public override bool SaveToModel()
        {
            return SaveToModel(false);
        }

        public bool SaveToModel(bool isLoadData)
        {
            DB.AddLogMessageInformation("Начало сохранения продукта", "Start SaveToModel in DocProductViewModel", DocID, ProductID);
            if (IsReadOnly || !(IsChanged || CurrentViewModel.IsChanged))
            {
                DB.AddLogMessageInformation("Успешный выход без сохранения продукта", "Quit successed from SaveToModel in DocProductViewModel: IsReadOnly = " + IsReadOnly.ToString() + " || !(IsChanged = " + IsChanged.ToString() + " || CurrentViewModel.IsChanged = " + CurrentViewModel.IsChanged.ToString() + ")" , DocID, ProductID);
                return true;
            }
            if ((CurrentViewModel is IProductValidate) && !(CurrentViewModel as IProductValidate).IsValidProduct)
            {
                DB.AddLogMessageInformation("Ошибочный выход из сохранения продукта", "Quit failed from SaveToModel in DocProductViewModel: (CurrentViewModel as IProductValidate).IsValidProduct = " + (CurrentViewModel as IProductValidate).IsValidProduct.ToString(), DocID, ProductID);
                return false;
            }
            Doc.Date = DocDate ?? DB.CurrentDateTime;
            Doc.IsConfirmed = IsConfirmed;
            GammaBase.SaveChanges();
            GammaBase.Entry(Doc).Reload(); // Получение обновленного документа(с номером из базы)
            DocID = Doc.DocID;
            if (DocProductKind != DocProductKinds.DocProductUnload)
                ProductID = Doc.DocProduction.DocProductionProducts.FirstOrDefault().ProductID;
            IsNewDoc = false;
            if (!CurrentViewModel.SaveToModel(Doc.DocID))
            {
                DB.AddLogMessageInformation("Ошибочное окончание сохранения продукта", "Quit failed from SaveToModel in DocProductViewModel: CurrentViewModel.SaveToModel return false ", DocID, ProductID);
                return false;
            }
            if (isLoadData)
                LoadData(IsNewDoc, DocProductKind == DocProductKinds.DocProductUnload ? DocID : ProductID , DocProductKind);
            SetIsChanged(false);
            CurrentViewModel.SetIsChanged(false);
            DB.AddLogMessageInformation("Успешное окончание сохранения продукта", "End SaveToModel in DocProductViewModel: success", DocID, ProductID);
            return true;
        }

        private Docs Doc { get; set; }

        public override sealed bool IsValid => base.IsValid && (CurrentViewModel?.IsValid ?? true);

        public override bool CanSaveExecute()
        {
            return !IsReadOnly && IsValid && (CurrentViewModel?.CanSaveExecute() ?? false); //&& DB.HaveWriteAccess("DocProduction");
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

                DB.AddLogMessageInformation("Выбрана вкладка " + (value == 0 ? "Основной" : value == 1 ? "Связи" : value == 2 ? "Решения о браке" : "") + " в Карточке продукта ProductID", "SET SelectedTabIndex ='" + value + "')", DocID,ProductID);
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
                if (Functions.ShowMessageQuestion("Закрытие карточки продукта: " + Environment.NewLine + "Есть несохраненные решения по браку! Нажмите Да, чтобы сохранить, или Нет, чтобы закрыть без сохранения?", "QUEST ClosingDocProduct in DocProductViewModel ", DocID, ProductID)
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