using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel; 
using Gamma.Models;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Gamma.Interfaces;
using Gamma.Attributes;
using Gamma.Common;

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
            ActivatedCommand = new RelayCommand(() => IsActive = true);
            DeactivatedCommand = new RelayCommand(() => IsActive = false);
            Messenger.Default.Register<BarcodeMessage>(this, BarcodeReceived);
            switch (msg.DocProductKind)
            {
                case DocProductKinds.DocProductSpool:
                    Title = "Тамбур";
                    CurrentViewModel = new DocProductSpoolViewModel(msg.ID,msg.IsNewProduct);
                    if (!msg.IsNewProduct)
                    {
                        Doc = (from d in DB.GammaBase.Docs where 
                                   DB.GammaBase.DocProducts.Where(dp => dp.ProductID == msg.ID).
                                   Select(dp => dp.DocID).Contains(d.DocID) &&
                                   d.DocTypeID == (byte)DocTypes.DocProduction
                               select d).FirstOrDefault();
                        DocProduction = DB.GammaBase.DocProduction.Find(Doc.DocID);
                        var product = DB.GammaBase.Products.Find(msg.ID);
                        Number = product.Number;
                        String.Format("{0}№ {1}", Title, Number);
                    }
                    break;
                case DocProductKinds.DocProductUnload:
                    Title = "Съем";
                    CurrentViewModel = new DocProductUnloadViewModel(msg.ID, msg.IsNewProduct);
                    if (!msg.IsNewProduct)
                    {
                        Doc = DB.GammaBase.Docs.Find(msg.ID);
                        DocProduction = DB.GammaBase.DocProduction.Find(msg.ID);
                        Number = Doc.Number;
                        Title = String.Format("{0}№ {1}", Title, Number);
                    }
                    break;
                default:
                    break;      
            }
            if (msg.IsNewProduct)
            {
                ProductionTaskID = msg.ID;
                DocDate = DB.CurrentDateTime;
            }
            else
            {
                DocDate = Doc.Date;
                ProductionTaskID = DocProduction.ProductionTaskID;
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
            }
            Bars = (CurrentViewModel as IBarImplemented).Bars;
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
            Messenger.Default.Register<ParentSaveMessage>(this, SaveToModel);
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
        }
        private void OpenProductionTask()
        {
            var msg = new OpenProductionTaskMessage() { ProductionTaskID = ProductionTaskID };
            if (CurrentViewModel is DocProductSpoolViewModel) msg.ProductionTaskKind = ProductionTaskKinds.ProductionTaskPM;
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
        
        [UIAuth(UIAuthLevel.ReadOnly)]
        public bool IsConfirmed 
        {
            get
            {
                return _isConfirmed;
            }
            set
            {
            	_isConfirmed = value;
                RaisePropertyChanged("IsConfirmed");
            }
        }
        public string Number { get; set; }
        private Guid DocProductID { get; set; }
        private Guid? _productionTaskID;
        private Guid? ProductionTaskID
        {
            get { return _productionTaskID; }
            set
            {
                _productionTaskID = value;
                if (value == null) return;
                var pinfo = (from pt in DB.GammaBase.ProductionTasks
                     where pt.ProductionTaskID == value
                     select new {Number = pt.Number,Date = pt.Date}).FirstOrDefault();
                ProductionTaskInfo = string.Format("Задание №{0} от {1}", pinfo.Number, pinfo.Date.ToString());
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
            if (!IsValid) return;
            SaveToModel();
            ReportManager.PrintReport(msg.ReportID, Doc.DocID);
        }
        public override void SaveToModel()
        {
            base.SaveToModel();
            if (Doc == null)
            {
                Doc = new Docs() 
                { 
                    DocID = Guid.NewGuid(), 
                    UserID = WorkSession.UserID, 
                    IsConfirmed = IsConfirmed,
                    PlaceID = WorkSession.PlaceID, 
                    ShiftID = WorkSession.ShiftID,
                    DocTypeID = (int)DocTypes.DocProduction
                };
                DocProduction = new DocProduction() { DocID = Doc.DocID, InPlaceID = DB.GammaBase.ProductionTasks.Where(p => p.ProductionTaskID == ProductionTaskID).Select(p => p.PlaceID).FirstOrDefault(), ProductionTaskID = ProductionTaskID };
                DB.GammaBase.Docs.Add(Doc);
                DB.GammaBase.DocProduction.Add(DocProduction);
            }
            Doc.Number = Number;
            Doc.Date = DocDate;
            Doc.IsConfirmed = IsConfirmed;
            DB.GammaBase.SaveChanges();
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
                return IsConfirmed || ((DB.GammaBase.UserPermit("DocProduction").FirstOrDefault() != (byte)PermissionMark.ReadAndWrite) && 
                    !WorkSession.DBAdmin);
            }
        }
        public override bool CanSaveExecute()
        {
            return CurrentViewModel.CanSaveExecute() && (DB.GammaBase.UserPermit("DocProduction").FirstOrDefault() == (byte)PermissionMark.ReadAndWrite);
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