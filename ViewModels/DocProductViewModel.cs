using DevExpress.Mvvm;
using System;
using System.Collections.ObjectModel; 
using Gamma.Models;
using System.Linq;
using Gamma.Interfaces;
using Gamma.Attributes;
using Gamma.Common;
using System.Windows;
using System.Data.Entity;
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
        /// <param name="gammaBase">Контекст бд для модели(по умолчанию Null, необходим для тестирования)</param>
        /// <param name="msg">Сообщение, содержащее параметры</param>
        public DocProductViewModel(OpenDocProductMessage msg, GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb; // Если контекст не передан, то создаем новый
            //  Если новый продукт, то сразу создаем новый документ в базе, с последующей
            //  выгрузкой документа из базы для получения номера
            if (msg.IsNewProduct) 
            {
                if (msg.ID == null && (msg.DocProductKind == DocProductKinds.DocProductSpool || 
                    msg.DocProductKind == DocProductKinds.DocProductUnload ||
                    msg.DocProductKind == DocProductKinds.DocProductPallet))
                {
                    string productKind = "";
                    switch (msg.DocProductKind)
                    {
                        case DocProductKinds.DocProductSpool:
                            productKind = "Тамбур";
                            break;
                        case DocProductKinds.DocProductUnload:
                            productKind = "Съем";
                            break;
                    }
                    MessageBox.Show($"Нельзя создать {productKind} без задания!");
                    CloseWindow();
                    return;
                }
                IsNewDoc = true;
                Doc = new Docs()
                {
                    DocID = SqlGuidUtil.NewSequentialid(),
                    DocTypeID = (int)DocTypes.DocProduction,
                    IsConfirmed = false,
                    PlaceID = WorkSession.PlaceID,
                    ShiftID = WorkSession.ShiftID,
                    UserID = WorkSession.UserID,
                    Date = DB.CurrentDateTime,
                    PrintName = WorkSession.PrintName
                };
                GammaBase.Docs.Add(Doc);
                DocProduction = new DocProduction()
                {
                    DocID = Doc.DocID,
                    InPlaceID = WorkSession.PlaceID,
                    ProductionTaskID = msg.ID
                };
                GammaBase.DocProduction.Add(DocProduction);
                GammaBase.SaveChanges(); // Сохранение в бд
                GammaBase.Entry(Doc).Reload(); // Получение обновленного документа(с номером из базы)
                Number = Doc.Number;
            }
            // Создаем дочернюю viewmodel в зависимости от типа изделия
            switch (msg.DocProductKind)
            {
                case DocProductKinds.DocProductSpool:
                    Title = "Тамбур";
                    if (!msg.IsNewProduct) // Если не новый продукт, то находим Doc, DocProduction, 
                    {
                        Doc =
                            GammaBase.Docs.Include(d => d.DocProduction).First(d => d.DocProducts.Select(dp => dp.ProductID).Contains((Guid)msg.ID) &&
                                    d.DocTypeID == (byte) DocTypes.DocProduction);
//                        GammaBase.Entry<Docs>(Doc).Reload();
                        DocProduction = Doc.DocProduction;
//                        GammaBase.Entry<DocProduction>(DocProduction).Reload();
                        var product =
                            GammaBase.Products
                                .First(p => p.ProductID == msg.ID);
/*                        if (product == null)
                        {
                            var productid = SQLGuidUtil.NewSequentialid();
                            var productionTask =
                                GammaBase.ProductionTasks.First(
                                    p => p.ProductionTaskID == Doc.DocProduction.ProductionTaskID);
                            product = new Products()
                            {
                                ProductID = productid,
                                ProductKindID = (byte) ProductKinds.ProductSpool,
                                ProductSpools = new ProductSpools()
                                {
                                    ProductID = productid,
                                    C1CNomenclatureID = productionTask.C1CNomenclatureID,
                                    C1CCharacteristicID = productionTask.C1CCharacteristicID,
                                    Diameter = 0,
                                    Weight = 0
                                },
                                DocProducts = new List<DocProducts>()
                                {
                                    new DocProducts()
                                    {
                                        DocID = Doc.DocID,
                                        ProductID = productid
                                    }
                                }
                            };
                            GammaBase.Products.Add(product);
                            GammaBase.SaveChanges();
                        }*/
                        GammaBase.Entry(product).Reload();
                        Number = product.Number;
                        Title = $"{Title} № {Number}";
                        CurrentViewModel = new DocProductSpoolViewModel(product.ProductID, false);
                        ProductRelations = new ObservableCollection<ProductRelation>
                            (
                                from prel in GammaBase.ProductRelations(product.ProductID)
                                select new ProductRelation
                                {
                                    Date = prel.Date,
                                    DocID = prel.DocID,
                                    Number = prel.Number,
                                    ProductID = prel.ProductID,
                                    ProductKind = prel.ProductKind,
                                    RelationKind = prel.RelationKind
                                }
                            );
                    }
                    else CurrentViewModel = new DocProductSpoolViewModel(Doc.DocID, true);
                    break;
                case DocProductKinds.DocProductUnload:
                    Title = "Съем";
                    if (!msg.IsNewProduct)
                    {
                        Doc = GammaBase.Docs.Include(d => d.DocProduction).First(d => d.DocID == msg.ID);
                        //GammaBase.Entry<Docs>(Doc).Reload();
                        DocProduction = Doc.DocProduction;
//                        GammaBase.Entry<DocProduction>(DocProduction).Reload();
                        Number = Doc.Number;
                        Title = $"{Title} № {Number}";
                        GetProductRelations();
                    }
                    CurrentViewModel = new DocProductUnloadViewModel(Doc.DocID, IsNewDoc);
                    break;
                case DocProductKinds.DocProductGroupPack:
                    Title = "Групповая упаковка";
                    if (!msg.IsNewProduct)
                    {
                        Doc = GammaBase.Docs.Include(d => d.DocProduction).First(d => d.DocID == msg.ID);
                        DocProduction = Doc.DocProduction;
                        Number = Doc.Number;
                        Title = $"{Title} № {Number}";
                        CurrentViewModel = new DocProductGroupPackViewModel(Doc.DocID);
                        GetProductRelations();
                    }
                    else CurrentViewModel = new DocProductGroupPackViewModel();
                    break;
                case DocProductKinds.DocProductPallet:
                    Title = "Паллета";
                    if (!msg.IsNewProduct)
                    {
                        Doc = GammaBase.Docs.Include(d => d.DocProduction).First(d => d.DocID == msg.ID);
                        DocProduction = Doc.DocProduction;
                        Number = Doc.Number;
                        Title = $"{Title} № {Number}";
                        CurrentViewModel = new DocProductPalletViewModel(Doc.DocID);
                        GetProductRelations();
                    }
                    else CurrentViewModel = new DocProductPalletViewModel();
                    break;
                default:
                    MessageBox.Show("Действие не предусмотрено програмой");
                    CloseWindow();
                    return;
            }
            var productionTaskBatch = GammaBase.ProductionTasks.Where(p => p.ProductionTaskID == DocProduction.ProductionTaskID).
                Select(p => p.ProductionTaskBatches.FirstOrDefault()).FirstOrDefault();
            if (productionTaskBatch != null) ProductionTaskBatchID = productionTaskBatch.ProductionTaskBatchID;
            DocDate = Doc.Date;
            IsConfirmed = Doc.IsConfirmed;
            Bars = ((IBarImplemented) CurrentViewModel).Bars;
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
            Messenger.Default.Register<ParentSaveMessage>(this, SaveToModel);
            ActivatedCommand = new DelegateCommand(() => IsActive = true);
            DeactivatedCommand = new DelegateCommand(() => IsActive = false);
            OpenProductionTaskCommand = new DelegateCommand(OpenProductionTask, () => ProductionTaskBatchID != null);
            Messenger.Default.Register<BarcodeMessage>(this, BarcodeReceived);
        }

        private void GetProductRelations()
        {
            ProductRelations = new ObservableCollection<ProductRelation>
                (
                from prel in GammaBase.DocRelations(Doc.DocID)
                select new ProductRelation
                {
                    Date = prel.Date,
                    DocID = prel.DocID,
                    Number = prel.Number,
                    ProductID = prel.ProductID,
                    ProductKind = prel.ProductKind,
                    RelationKind = prel.RelationKind
                }
                );
        }

        private void SaveToModel(ParentSaveMessage msg)
        {
            SaveToModel();
        }
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
                if (GammaBase.DocProducts.Where(d => d.DocID == Doc.DocID && d.ProductID == productid).Select(d => d).FirstOrDefault() != null)
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
            var msg = new OpenProductionTaskBatchMessage() { ProductionTaskBatchID = ProductionTaskBatchID };
            if (CurrentViewModel is DocProductSpoolViewModel || CurrentViewModel is DocProductUnloadViewModel 
                || CurrentViewModel is DocProductGroupPackViewModel) msg.BatchKind = BatchKinds.SGB;else if (CurrentViewModel is DocProductPalletViewModel) msg.BatchKind = BatchKinds.SGI;
            MessageManager.OpenProductionTask(msg);
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
        private bool HaveChanges { get; set; }
        [UIAuth(UIAuthLevel.ReadOnly)]
        public bool IsConfirmed 
        {
            get
            {
                return _isConfirmed;
            }
            set
            {
                if (value && !IsValid)
                    _isConfirmed = false;
                else
            	    _isConfirmed = value;
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
//            if (!IsValid) return;
            SaveToModel();
            ReportManager.PrintReport(msg.ReportID, Doc.DocID);
        }

        protected override void SaveToModel(GammaEntities gammaBase = null)
        {
            if (!DB.HaveWriteAccess("Docs")) return;
            gammaBase = gammaBase ?? DB.GammaDb;
            base.SaveToModel(gammaBase);
/*            if (Doc == null)
            {
                Doc = new Docs() 
                { 
                    DocID = Guid.NewGuid(), 
                    UserID = WorkSession.UserID, 
                    IsConfirmed = IsConfirmed,
                    PlaceID = WorkSession.PlaceID, 
                    ShiftID = WorkSession.ShiftID,
                    DocTypeID = (int)DocTypes.DocProduction,
                    PrintName = WorkSession.PrintName
                };
                DocProduction = new DocProduction() { DocID = Doc.DocID, InPlaceID = GammaBase.ProductionTasks.Where(p => p.ProductionTaskID == ProductionTaskID).Select(p => p.PlaceID).FirstOrDefault(), ProductionTaskID = ProductionTaskID };
                GammaBase.Docs.Add(Doc);
                GammaBase.DocProduction.Add(DocProduction);
            }
             */
            //           Doc.Number = Number;
            Doc.Date = DocDate;
            Doc.IsConfirmed = IsConfirmed;
            GammaBase.SaveChanges();
            HaveChanges = false;
            CurrentViewModel.SaveToModel(Doc.DocID);
        }
        private Docs Doc { get; set; }
        private DocProduction DocProduction { get; set; }
        public override bool IsValid => base.IsValid && CurrentViewModel.IsValid;

        public bool IsReadOnly => IsConfirmed || (!DB.HaveWriteAccess("DocProduction") && !WorkSession.DBAdmin);

        public override bool CanSaveExecute()
        {
            return CurrentViewModel.CanSaveExecute() && DB.HaveWriteAccess("DocProduction");
        }
        public string Title { get; set; }
        private bool IsActive { get; set; }
        public DelegateCommand ActivatedCommand { get; private set; }
        public DelegateCommand DeactivatedCommand { get; private set; }
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
        public ProductRelation SelectedProduct { get; set; }
    }
}