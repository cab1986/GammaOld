// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Interfaces;
using Gamma.Models;
using System.Collections.Generic;
using Gamma.Controllers;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using Gamma.DialogViewModels;
using System.ComponentModel;

namespace Gamma.ViewModels
{
    public class DocRepackViewModel : SaveImplementedViewModel, IBarImplemented
    {
        public DocRepackViewModel(Guid? docRepackId)
        {
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
            //DocRepackId = docRepackId ?? DocRepackId;
            //IsInitialize = true;
            using (var gammaBase = DB.GammaDb)
            {
                var docID = gammaBase.Docs.FirstOrDefault(d => d.DocID == docRepackId)?.DocID;
                if (docID != null)
                {
                    DocRepackId = (Guid)docID;
                }
                else
                {
                    var productionTaskID = gammaBase.ProductionTaskBatches.FirstOrDefault(p => p.ProductionTaskBatchID == docRepackId)?.ProductionTasks.FirstOrDefault()?.ProductionTaskID;
                    DocRepackId = GetNewDocRepack(productionTaskID);
                }
                var docRepack =
                    gammaBase.DocRepack.Include(d => d.Docs)
                    .FirstOrDefault(d => d.DocID == DocRepackId);
                if (docRepack == null)
                {
                    MessageBox.Show("Переупаковка не найдена в базе. Видимо уже удалено");
                    CloseWindow();
                    return;
                }
                Number = docRepack.Docs.Number;
                Date = docRepack.Docs.Date;
                dateInit = docRepack.Docs.Date;
                ShiftID = docRepack.Docs.ShiftID ?? 0;
                PlaceId = docRepack.Docs.PlaceID;
                IsConfirmed = docRepack.Docs.IsConfirmed;
                ProductionTaskId = docRepack.ProductionTaskID;
                //DenyEditPlace = PlaceId != null && !WorkSession.PlaceIds.Contains((int)PlaceId);//docShipmentOrderInfo.OutBranchID != WorkSession.BranchID;
                if (ProductionTaskId != null)
                {
                    ProductionTaskInfo = "Задание № " +
                                               GammaBase.ProductionTasks.Where(p => p.ProductionTaskID == ProductionTaskId).
                                                Select(p => p.ProductionTaskBatches.FirstOrDefault()).FirstOrDefault().Number;
                }
                RepackProducts = new ItemsChangeObservableCollection<RepackProduct>(gammaBase.vDocRepackProducts.Where(dp => dp.DocRepackID == docRepackId)
                    .Select(dp => new RepackProduct
                    {
                        DocRepackID = dp.DocRepackID,
                        DocRepackNumber = dp.DocRepackNumber,
                        ProductID = dp.ProductID,
                        ProductNumber = dp.ProductNumber,
                        NomenclatureID = dp.C1CNomenclatureID,
                        CharacteristicID = dp.C1CCharacteristicID,
                        NomenclatureName = dp.NomenclatureName,
                        Date = dp.Date,
                        Quantity = dp.Quantity ?? 0,
                        QuantityGood = dp.QuantityGood,
                        QuantityBroke = dp.QuantityBroke,
                        ShiftID = dp.ShiftID ?? 0,
                        ProductionTaskID = dp.ProductionTaskID,
                        DocBrokeID = dp.DocBrokeID,
                        DocBrokeNumber = dp.DocBrokeNumber,
                        ProductKind = (ProductKind)dp.ProductKindID
                    }));
            }
            //#if DEBUG
            //            AllowEditDoc = DB.AllowEditDoc(DocRepackId) && DB.HaveWriteAccess("DocRepack");
            //#else
            var a = DB.AllowEditDoc(DocRepackId);
                var b = DB.HaveWriteAccess("Docs");
            AllowEditDoc = DB.AllowEditDoc(DocRepackId) && DB.HaveWriteAccess("Docs") && (WorkSession.ShiftID == 0 || (WorkSession.ShiftID == ShiftID && WorkSession.PlaceID == PlaceId)) && (DB.CurrentDateTime < ShiftEndTime.AddHours(1)); ;
//#endif
            OpendProductionTaskCommand = new DelegateCommand(OpenProductionTask, () => ProductionTaskId != null);
            RepackProducts.CollectionChanged += RepackProductsOnCollectionChanged;
            AddProductCommand = new DelegateCommand(AddProduct, () => !IsReadOnly);
            ShowProductCommand = new DelegateCommand(ShowProduct, SelectedProduct != null);
            DeleteProductCommand = new DelegateCommand(DeleteProduct, () => !IsReadOnly && SelectedProduct != null);
            var canUploadTo1CCommand = ProductionTaskId == null;
            UploadTo1CCommand = new DelegateCommand(UploadTo1C,() => canUploadTo1CCommand);
            Bars.Add(ReportManager.GetReportBar("DocRepack", VMID));
            
            //IsInitialize = false;
        }

        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != VMID) return;
            if (!IsValid)
            {
                MessageBox.Show("Не заполнены некоторые обязательные поля!", "Поля не заполнены", MessageBoxButton.OK,
                    MessageBoxImage.Asterisk);
                return;
            }
            if (!SaveToModel()) return;
            ReportManager.PrintReport(msg.ReportID, DocRepackId);
        }

        private readonly DocumentController documentController = new DocumentController();

        //public bool DenyEditPlace { get; private set; }

        public bool CanChangeIsConfirmed => AllowEditDoc; //WorkSession.ShiftID == 0 && (WorkSession.DBAdmin || WorkSession.RoleName == "Dispetcher");// ProductionTaskId == null;

        public override bool CanSaveExecute()
        {
            return base.CanSaveExecute() && !IsReadOnly;
        }

        public bool IsReadOnly => !(AllowEditDoc && !IsConfirmed && DB.HaveWriteAccess("Docs"));

        public bool IsDateReadOnly
        {
            get
            { return !(!IsReadOnly && WorkSession.ShiftID == 0 && (WorkSession.DBAdmin || WorkSession.RoleName == "Dispetcher")); }
        }

        public DelegateCommand UploadTo1CCommand { get; private set; }

        //public bool CanUploadTo1C => IsConfirmed && ProductionTaskId == null;

        private void UploadTo1C()
        {
            UIServices.SetBusyState();
            if (ProductionTaskId == null && MessageBox.Show("Документ будет подтвержден, сохранен и выгружен в 1С. Продолжить?", "Выгрузка в 1С",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)// || dateInit != Date)
            {
                IsConfirmed = true;
                if (SaveToModel())
                {
                    //DB.UploadFreeRepackTo1C(DocRepackId);
                }
            }
        }

        public RepackProduct SelectedProduct { get; set; }
        public DelegateCommand ShowProductCommand { get; private set; }

        private void ShowProduct()
        {
            if (SelectedProduct?.ProductKind != null)
                switch (SelectedProduct.ProductKind)
                {
                    case ProductKind.ProductSpool:
                        MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedProduct.ProductID);
                        break;
                    case ProductKind.ProductGroupPack:
                        MessageManager.OpenDocProduct(DocProductKinds.DocProductGroupPack, SelectedProduct.ProductID);
                        break;
                    case ProductKind.ProductPallet:
                        MessageManager.OpenDocProduct(DocProductKinds.DocProductPallet, SelectedProduct.ProductID);
                        break;
                    case ProductKind.ProductPalletR:
                        MessageManager.OpenDocProduct(DocProductKinds.DocProductPalletR, SelectedProduct.ProductID);
                        break;
                    default:
                        //MessageBox.Show("Ошибка программы, действие не предусмотрено");
                        return;
                }
        }

        /// <summary>
        /// Добавление продукта к списку
        /// </summary>
        /// <param name="productId">ID продукта</param>
        /// <param name="docId">ID документа переупаковки</param>
        /// <param name="repackProducts">Список продукции</param>
        private void AddProduct(Guid productId, Guid docId, ICollection<RepackProduct> repackProducts)
        {
            using (var gammaBase = DB.GammaDb)
            {
                if (RepackProducts.Select(bp => bp.ProductID).Contains(productId))
                {
                    MessageBox.Show("Данный продукт уже добавлен ранее", "Ошибка добавления",
                        MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return;
                }
                var product = gammaBase.vProductsInfo
                    .FirstOrDefault(p => p.ProductID == productId);
                if (product == null) return;
                /*if (!IsInFuturePeriod && BrokeProducts.Count > 0 &&
                    BrokeProducts.Select(bp => bp.ProductionPlaceId).First() != product.PlaceID)
                {
                    MessageBox.Show("Нельзя добавлять продукт другого передела в акт '25к - I передел'", "Ошибка добавления",
                        MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return;
                }*/
#region AddRepackProduct
                var repackProduct = new RepackProduct()
                {
                    DocRepackID = docId,
                    //DocRepackNumber = dp.DocRepackNumber,
                    ProductID = product.ProductID,
                    ProductNumber = product.Number,
                    NomenclatureID = product.C1CNomenclatureID,
                    CharacteristicID = product.C1CCharacteristicID,
                    NomenclatureName = product.NomenclatureName,
                    Date = product.Date,
                    //Quantity = product.BaseMeasureUnitQuantity ?? 0,
                    //QuantityGood = product.BaseMeasureUnitQuantity ?? 0,
                    //QuantityBroke = NULL,
                    ShiftID = ShiftID,
                    ProductionTaskID = ProductionTaskId,
                    //DocBrokeID = dp.DocBrokeID,
                    //DocBrokeNumber = dp.DocBrokeNumber,
                    ProductKind = (ProductKind)product.ProductKindID
                };

                if ((ProductState)product.StateID == ProductState.Broke || (ProductState)product.StateID == ProductState.NeedsDecision || (ProductState)product.StateID == ProductState.ForConversion || (ProductState)product.StateID == ProductState.Repack)
                {
                    var model = new SetQuantityDialogModel("Укажите кол-во бракованных рулончиков (или пачек для салфеток) в паллете №"+ product.Number, "Брак", 0, (int)product.Quantity);
                    var okCommand = new UICommand()
                    {
                        Caption = "OK",
                        IsCancel = false,
                        IsDefault = true,
                        Command = new DelegateCommand<CancelEventArgs>(
                    x => DebugFunc(),
                    x => model.Quantity >= 0 && model.Quantity <= (int)product.Quantity),
                    };
                    var cancelCommand = new UICommand()
                    {
                        Id = MessageBoxResult.Cancel,
                        Caption = "Отмена",
                        IsCancel = true,
                        IsDefault = false,
                    };
                    var dialogService = GetService<IDialogService>("");
                    var result = dialogService.ShowDialog(
                        dialogCommands: new List<UICommand>() { okCommand, cancelCommand },
                        title: "Брак",
                        viewModel: model);
                    if (result != okCommand)
                        return;
                    var res = gammaBase.UtilizationProductWithRepackInDocBroke(product.ProductID, model.Quantity, WorkSession.ShiftID).FirstOrDefault();
                    if (res != null)
                    {
                        if (res.Quantity != null) repackProduct.Quantity = (decimal)res.Quantity;
                        if (res.QuantityGood != null) repackProduct.QuantityGood = (decimal)res.QuantityGood;
                        if (res.QuantityBroke != null) repackProduct.QuantityBroke = (decimal)res.QuantityBroke;
                        repackProduct.DocBrokeID = res.DocBrokeID;
                        repackProduct.DocBrokeNumber = res.DocBrokeNumber;
                    }
                }
                else
                {
                    repackProduct.Quantity = product.BaseMeasureUnitQuantity ?? 0;
                    repackProduct.QuantityGood = product.BaseMeasureUnitQuantity ?? 0;
                };

                var docRepackProduct = gammaBase.DocRepackProducts.FirstOrDefault(p => p.ProductID == product.ProductID && p.DocID == docId);
                if (docRepackProduct == null)
                {
                    docRepackProduct = new DocRepackProducts
                    {
                        DocRepackProductID = Guid.NewGuid(),
                        DocID = repackProduct.DocRepackID,
                        Date = DB.CurrentDateTime,
                        //IsConfirmed = RepackProduct.IsConfirmed,
                        ProductID = repackProduct.ProductID,
                        Quantity = repackProduct.Quantity,
                        QuantityGood = repackProduct.QuantityGood,
                        QuantityBroke = repackProduct.QuantityBroke,
                        DocBrokeID = repackProduct.DocBrokeID
                    };
                    gammaBase.DocRepackProducts.Add(docRepackProduct);
                }

                var docWithdrawalId = SqlGuidUtil.NewSequentialid();
                if (!documentController.WithdrawProduct(repackProduct.ProductID, docWithdrawalId, true))
                {
                    MessageBox.Show("Не удалось списать паллету. Ошибка в базе.");
                    return;
                }
                docRepackProduct.DocWithdrawal.Add(gammaBase.DocWithdrawal.First(d => d.DocID == docWithdrawalId));
                gammaBase.SaveChanges();

                
                repackProducts.Add(repackProduct);

#endregion AddRepackProduct
                /*#region AddBrokeDecisionProduct
                var docBrokeDecisionProducts = gammaBase.DocBrokeDecisionProducts.Where(d => d.DocID == docId && d.ProductID == productId).ToList();
                if (docBrokeDecisionProducts.Count == 0)
                {
                    brokeDecisionProducts.Add(new BrokeDecisionProduct(
                        product.ProductID,
                        (ProductKind)product.ProductKindID,
                        product.Number,
                        ProductState.NeedsDecision,
                        product.BaseMeasureUnitQuantity ?? 1000000,
                        DB.GetProductNomenclatureNameBeforeDate(product.ProductID, Date),
                        product.BaseMeasureUnit,
                        product.C1CNomenclatureID,
                        (Guid)product.C1CCharacteristicID,
                        product.BaseMeasureUnitQuantity ?? 0
                        )
                    );
                }
                else
                {
                    foreach (var decisionProduct in docBrokeDecisionProducts)
                    {
                        var docWithdrawalID = decisionProduct.DocBrokeDecisionProductWithdrawalProducts?.FirstOrDefault();
                        brokeDecisionProducts.Add(new BrokeDecisionProduct(
                                decisionProduct.ProductID,
                                (ProductKind)product.ProductKindID,
                                product.Number,
                                (ProductState)decisionProduct.StateID,
                                docBrokeProductInfo?.Quantity ?? 1000000,
                                DB.GetProductNomenclatureNameBeforeDate(product.ProductID, Date),
                                product.BaseMeasureUnit,
                                product.C1CNomenclatureID,
                                (Guid)product.C1CCharacteristicID,
                                decisionProduct.Quantity ?? 0,
                                decisionProduct.DecisionApplied,
                                (docWithdrawalID?.DocWithdrawalID == Guid.Empty ? null : docWithdrawalID?.DocWithdrawalID)
                            )
                        {
                            Comment = decisionProduct.Comment,
                            NomenclatureId = decisionProduct.C1CNomenclatureID,
                            CharacteristicId = decisionProduct.C1CCharacteristicID,
                            ProductKind = (ProductKind)product.ProductKindID
                        });
                    }
                }
#endregion AddBrokeDecisionProduct
                RefreshRejectionReasonsList();
                RefreshProductStateList();*/
            }
        }

        private void DebugFunc()
        {
            System.Diagnostics.Debug.Print("Кол-во задано");
        }

        public DelegateCommand AddProductCommand { get; private set; }
        public DelegateCommand DeleteProductCommand { get; private set; }

        private void AddProduct()
        {
            Messenger.Default.Register<ChoosenProductMessage>(this, AddChoosenProduct);
            MessageManager.OpenFindProduct((ProductKind)(Functions.EnumDescriptionsToList(typeof(ProductKind)).Count + 1), true, null, true, false, true);
        }

        private void AddChoosenProduct(ChoosenProductMessage msg)
        {
            Messenger.Default.Unregister<ChoosenProductMessage>(this);
            if (msg.ProductIDs == null || msg.ProductIDs?.Count == 0)
                AddProduct(msg.ProductID, DocRepackId, RepackProducts);
            else
            {
                foreach (var product in msg.ProductIDs)
                {
                    AddProduct(product, DocRepackId, RepackProducts);
                }
            }
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;
            /*if (SelectedProduct.IsConfirmed == true || SelectedProduct.IsAccepted)
            {
                MessageBox.Show("Нельзя удалить продукт, который уже получен или принят");
                return;
            }*/
//            if (SelectedProduct == null || SelectedProduct.IsConfirmed == true || SelectedProduct.IsAccepted) return;
            if (MessageBox.Show("Вы уверены, что хотите удалить данный продукт из документа?", "Удаление продукта",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            using (var gammaBase = DB.GammaDb)
            {
                var product =
                    gammaBase.DocRepackProducts.FirstOrDefault(
                        op => op.ProductID == SelectedProduct.ProductID && op.DocID == DocRepackId);
                if (product != null)
                {
                    var docWithdrawalID = product.DocWithdrawal.First().DocID;
                    gammaBase.DocWithdrawalProducts.Remove(
                        gammaBase.DocWithdrawalProducts.First(wp => wp.DocID == docWithdrawalID));
                    gammaBase.DocWithdrawal.Remove(gammaBase.DocWithdrawal.First(dw => dw.DocID == docWithdrawalID));
                    gammaBase.Docs.Remove(gammaBase.Docs.First(d => d.DocID == docWithdrawalID));
                    if (SelectedProduct.DocBrokeID != null)
                    {
                        var decisionProduct = gammaBase.DocBrokeDecisionProducts.FirstOrDefault(p => p.DocID == SelectedProduct.DocBrokeID && p.ProductID == SelectedProduct.ProductID && p.StateID == (int)ProductState.Broke);
                        if (decisionProduct != null)
                            decisionProduct.DecisionApplied = false;
                    }
                    gammaBase.DocRepackProducts.Remove(product);
                    gammaBase.SaveChanges();
                }
            }
            RepackProducts.Remove(RepackProducts.First(p => p.ProductID == SelectedProduct.ProductID));
        }

        private void RepackProductsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
           // IsConfirmed = RepackProducts.All(p => p.IsConfirmed == true);
        }

        private DateTime dateInit { get; set; }

        private Guid? ProductionTaskId { get; set; }
        public string ProductionTaskInfo { get; set; }
        public DelegateCommand OpendProductionTaskCommand { get; private set; }

        private void OpenProductionTask()
        {
            if (ProductionTaskId == null || !DB.HaveReadAccess("ProductionTasks")) return;
            UIServices.SetBusyState();
            var productionTaskBatch = GammaBase.ProductionTasks.Where(p => p.ProductionTaskID == ProductionTaskId).
                                                Select(p => p.ProductionTaskBatches.FirstOrDefault()).FirstOrDefault();
            MessageManager.OpenProductionTask((BatchKinds)productionTaskBatch.BatchKindID, productionTaskBatch.ProductionTaskBatchID);
        }

        private bool? _checkAll;

        public bool? CheckAll
        {
            get { return _checkAll; }
            set
            {
                _checkAll = value;
                RaisePropertyChanged("CheckAll");
                if (_checkAll == null) return;
               /* foreach (var product in RepackProducts)
                {
                    product.IsConfirmed = _checkAll;
                    IsConfirmed = (bool)_checkAll;
                }*/
            }
        }

        private Guid _docRepackId { get; set; }
        private Guid DocRepackId
        {
            get
            { return _docRepackId; }//?? GetNewDocRepack(); }
            set
            {
                _docRepackId = value;
            }
        }
        public string Number { get; set; }
        /// <summary>
        /// Окончание смены
        /// </summary>
        private DateTime ShiftEndTime { get; set; }
        private DateTime _date;
        public DateTime Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
                ShiftEndTime = DB.GetShiftEndTimeFromDate(Date);
            }
        }
        public byte ShiftID { get; set; }

        private int? _placeId;
        public int? PlaceId
        {
            get { return _placeId; }
            set
            {
                _placeId = value;
                if (_placeId == null) return;
                using (var gammaBase = DB.GammaDb)
                {
                    Place = gammaBase.Places.FirstOrDefault(p => p.PlaceID == _placeId)?.Name;
                }
            }
        }
        public string Place { get; set; }

        private bool AllowEditDoc { get; set; } = true;

        private bool _isConfirmed;

        public bool IsConfirmed
        {
            get
            {
                return _isConfirmed;
            }
            set
            {
                if (_isConfirmed != value)
                {
                    //if (_isConfirmed)
                        if (!AllowEditDoc)
                        {
                            MessageBox.Show("Правка невозможна. Продукция уже в выработке или с ней связаны другие документы");
                            return;
                        }
                    if (value && !IsValid)
                        _isConfirmed = false;
                    else
                        _isConfirmed = value;
                    SaveToModel();
                }
                RaisePropertyChanged("IsConfirmed");
            }
        }

        /* private void saveDoc()
         {
             if (!IsInitialize)
             {
                 using (var gammaBase = DB.GammaDb)
                 {
                     var doc = gammaBase.Docs.Where(d => d.DocID == DocRepackId).FirstOrDefault();
                     if (doc != null)
                     {
                         doc.IsConfirmed = IsConfirmed;
                     }
                     gammaBase.SaveChanges();
                 }
                 UploadTo1C();
             }
         }*/

        public ItemsChangeObservableCollection<RepackProduct> RepackProducts { get; set; }
//        public List<Place> Warehouses { get; private set; }

        public Guid GetNewDocRepack(Guid? productionTaskID)
        {
            var newDocRepackID = Guid.NewGuid();
            using (var gammaBase = DB.GammaDb)
            {
                var docRepack =
                    gammaBase.Docs.FirstOrDefault(d => d.DocID == newDocRepackID);
                if (docRepack == null)
                {
                    gammaBase.Docs.Add(new Docs()
                    {
                        DocID = newDocRepackID,
                        DocTypeID = (int)DocTypes.DocRepack,
                        Number = Number,
                        Date = DateTime.Now,
                        PlaceID = WorkSession.PlaceID,
                        ShiftID = WorkSession.ShiftID,
                        IsConfirmed = false
                    });
                    gammaBase.DocRepack.Add(new DocRepack()
                    {
                        DocID = newDocRepackID,
                        ProductionTaskID = productionTaskID
                    });
                }
                gammaBase.SaveChanges();
            }
                return newDocRepackID;
        }

        public override bool SaveToModel()
        {
            if (!DB.HaveWriteAccess("Docs")) return true;
            using (var gammaBase = DB.GammaDb)
            {
                var docRepack =
                    gammaBase.DocRepack.Include(d => d.DocRepackProducts).Include(d => d.Docs).FirstOrDefault(d => d.DocID == DocRepackId);
                if (docRepack == null)
                {
                    MessageBox.Show("Документ уже не существует в базе. Скорей всего он был кем-то удален");
                    return false;
                }
                //docRepack.Docs.Number = Number;
               // docRepack.Docs.Date = new DateTime(Date.Year, Date.Month, Date.Day, Date.Hour,Date.Minute, 0);
                //docRepack.Docs.PlaceID = PlaceId;
                docRepack.Docs.IsConfirmed = IsConfirmed;
                /*foreach (var repackProduct in RepackProducts)
                {
                    var product = docRepack.DocRepackProducts.FirstOrDefault(p => p.ProductID == repackProduct.ProductID);
                    if (product == null)
                    {
                        product = new DocRepackProducts
                        {
                            DocRepackProductID = Guid.NewGuid(),
                            DocID = repackProduct.DocRepackID,
                            Date = DB.CurrentDateTime,
                            //IsConfirmed = RepackProduct.IsConfirmed,
                            ProductID = repackProduct.ProductID,
                            Quantity = repackProduct.Quantity,
                            QuantityGood = repackProduct.QuantityGood,
                            QuantityBroke = repackProduct.QuantityBroke
                        };
                        gammaBase.DocRepackProducts.Add(product);
                    }
                }
                //foreach (var removeProduct in gammaBase.DocRepackProducts.Where(
                //        op => !(RepackProducts.Select(p => p.ProductID).ToList().Contains(op.ProductID)) && op.DocID == DocRepackId))
                //{
                var removeProductIDs = RepackProducts.Select(p => p.ProductID).ToList();
                    gammaBase.DocRepackProducts.RemoveRange(gammaBase.DocRepackProducts.Where(
                        op => !(removeProductIDs.Contains(op.ProductID)) && op.DocID == DocRepackId));
                 //}*/
                gammaBase.SaveChanges();
            }
            return true;
        }

        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();
        public Guid? VMID { get; } = Guid.NewGuid();
    }
}
