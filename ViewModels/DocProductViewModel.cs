using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel; 
using Gamma.Models;
using System.Linq;
using Gamma.Interfaces;
using Gamma.Attributes;
using Gamma.Common;
using System.Windows;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocProductViewModel : SaveImplementedViewModel, ICheckedAccess
    {
        /// <summary>
        /// Initializes a new instance of the DocProductViewModel class.
        /// </summary>
        public DocProductViewModel(OpenDocProductMessage msg)
        {
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
                    MessageBox.Show(String.Format("Нельзя создать {0} без задания!", productKind));
                    CloseWindow();
                    return;
                }
                IsNewDoc = true;
                Doc = new Docs()
                {
                    DocID = SQLGuidUtil.NewSequentialId(),
                    DocTypeID = (int)DocTypes.DocProduction,
                    IsConfirmed = false,
                    PlaceID = WorkSession.PlaceID,
                    ShiftID = WorkSession.ShiftID,
                    UserID = WorkSession.UserID,
                    Date = DB.CurrentDateTime,
                    PrintName = WorkSession.PrintName
                };
                DB.GammaBase.Docs.Add(Doc);
                DocProduction = new DocProduction()
                {
                    DocID = Doc.DocID,
                    InPlaceID = WorkSession.PlaceID,
                    ProductionTaskID = msg.ID
                };
                DB.GammaBase.DocProduction.Add(DocProduction);
                DB.GammaBase.SaveChanges();
                DB.GammaBase.Entry<Docs>(Doc).Reload();
                Number = Doc.Number;
            }
            
            // Создаем дочернюю viewmodel в зависимости от типа изделия
            switch (msg.DocProductKind)
            {
                case DocProductKinds.DocProductSpool:
                    Title = "Тамбур";
                    if (!msg.IsNewProduct) // Если не новый продукт, то находим Doc, DocProduction, 
                                            //Product и обновляем в кэше с помощью Reload()
                    {
                        Doc = DB.GammaBase.Docs.FirstOrDefault(d => d.DocID == msg.ID);
                        DB.GammaBase.Entry<Docs>(Doc).Reload();
                        DocProduction = DB.GammaBase.DocProduction.Find(Doc.DocID);
                        DB.GammaBase.Entry<DocProduction>(DocProduction).Reload();
                        var product = DB.GammaBase.Products.Where(p => p.DocProducts.Select(d => d.DocID).Contains(Doc.DocID)).FirstOrDefault();
                        DB.GammaBase.Entry<Products>(product).Reload();
                        Number = product.Number;
                        Title = String.Format("{0} № {1}", Title, Number);
                    }
                    CurrentViewModel = new DocProductSpoolViewModel(Doc.DocID, IsNewDoc);
                    break;
                case DocProductKinds.DocProductUnload:
                    Title = "Съем";
                    if (!msg.IsNewProduct)
                    {
                        Doc = DB.GammaBase.Docs.Find(msg.ID);
                        DB.GammaBase.Entry<Docs>(Doc).Reload();
                        DocProduction = DB.GammaBase.DocProduction.Find(msg.ID);
                        DB.GammaBase.Entry<DocProduction>(DocProduction).Reload();
                        Number = Doc.Number;
                        Title = String.Format("{0} № {1}", Title, Number);
                    }
                    CurrentViewModel = new DocProductUnloadViewModel(Doc.DocID, IsNewDoc);
                    break;
                case DocProductKinds.DocProductGroupPack:
                    Title = "Групповая упаковка";
                    if (!msg.IsNewProduct)
                    {
                        Doc = DB.GammaBase.Docs.FirstOrDefault(d => d.DocID == msg.ID);
                        DocProduction = DB.GammaBase.DocProduction.Find(Doc.DocID);
                        Number = Doc.Number;
                        Title = String.Format("{0} № {1}", Title, Number);
                        CurrentViewModel = new DocProductGroupPackViewModel(Doc.DocID);
                    }
                    else CurrentViewModel = new DocProductGroupPackViewModel();
                    break;
                default:
                    MessageBox.Show("Действие не предусмотрено програмой");
                    CloseWindow();
                    return;
            }
            var productionTaskBatch = DB.GammaBase.ProductionTasks.Where(p => p.ProductionTaskID == DocProduction.ProductionTaskID).
                Select(p => p.ProductionTaskBatches.FirstOrDefault()).FirstOrDefault();
            if (productionTaskBatch != null) ProductionTaskBatchID = productionTaskBatch.ProductionTaskBatchID;
            DocDate = Doc.Date;
            IsConfirmed = Doc.IsConfirmed;
            ProductRelations = new ObservableCollection<ProductRelation>
                (
                    from prel in DB.GammaBase.GetProductRelations(Doc.DocID)
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
            Bars = (CurrentViewModel as IBarImplemented).Bars;
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
            Messenger.Default.Register<ParentSaveMessage>(this, SaveToModel);
            ActivatedCommand = new RelayCommand(() => IsActive = true);
            DeactivatedCommand = new RelayCommand(() => IsActive = false);
            OpenProductionTaskCommand = new RelayCommand(OpenProductionTask, () => ProductionTaskBatchID != null);
            Messenger.Default.Register<BarcodeMessage>(this, BarcodeReceived);
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
                var productId = (from p in DB.GammaBase.Products
                                 where p.BarCode == msg.Barcode
                                 select p.ProductID).FirstOrDefault();
                if (productId != new Guid()) 
                    if (DB.GammaBase.DocProducts.Where(d => d.DocID == Doc.DocID && d.ProductID == productId).Select(d => d) != null)
                        IsConfirmed = false;
            }
            else if (CurrentViewModel is DocProductGroupPackViewModel)
            {
                (CurrentViewModel as DocProductGroupPackViewModel).AddSpool(msg.Barcode);
            }
        }
        private bool IsNewDoc { get; set; }
        private void OpenProductionTask()
        {
            var msg = new OpenProductionTaskBatchMessage() { ProductionTaskBatchID = ProductionTaskBatchID };
            if (CurrentViewModel is DocProductSpoolViewModel) msg.BatchKind = BatchKinds.SGB;
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
                var pinfo = (from pt in DB.GammaBase.ProductionTaskBatches
                     where pt.ProductionTaskBatchID == value
                     select new 
                     {
                         Number = pt.Number,
                         Date = pt.Date 
                     }).FirstOrDefault();
                if (pinfo != null)
                    ProductionTaskInfo = string.Format("Задание №{0} от {1}", pinfo.Number, pinfo.Date);
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
        public RelayCommand OpenProductionTaskCommand { get; private set; }
        private ObservableCollection<BarViewModel> _bars;
      
        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != (CurrentViewModel as IBarImplemented).VMID) return;
//            if (!IsValid) return;
            SaveToModel();
            ReportManager.PrintReport(msg.ReportID, Doc.DocID);
        }

        public override void SaveToModel()
        {
            if (!DB.HaveWriteAccess("Docs")) return;
            base.SaveToModel();
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
                DocProduction = new DocProduction() { DocID = Doc.DocID, InPlaceID = DB.GammaBase.ProductionTasks.Where(p => p.ProductionTaskID == ProductionTaskID).Select(p => p.PlaceID).FirstOrDefault(), ProductionTaskID = ProductionTaskID };
                DB.GammaBase.Docs.Add(Doc);
                DB.GammaBase.DocProduction.Add(DocProduction);
            }
             */
            //           Doc.Number = Number;
            Doc.Date = DocDate;
            Doc.IsConfirmed = IsConfirmed;
            DB.GammaBase.SaveChanges();
            HaveChanges = false;
            CurrentViewModel.SaveToModel(Doc.DocID);
        }
        private Docs Doc { get; set; }
        private DocProduction DocProduction { get; set; }
        public override bool IsValid
        {
            get
            {
                return base.IsValid && CurrentViewModel.IsValid;
            }
        }

        public bool IsReadOnly
        {
            get 
            {
                return IsConfirmed || (!DB.HaveWriteAccess("DocProduction") && !WorkSession.DBAdmin);
            }
        }
        public override bool CanSaveExecute()
        {
            return CurrentViewModel.CanSaveExecute() && DB.HaveWriteAccess("DocProduction");
        }
        public string Title { get; set; }
        private bool IsActive { get; set; }
        public RelayCommand ActivatedCommand { get; private set; }
        public RelayCommand DeactivatedCommand { get; private set; }
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