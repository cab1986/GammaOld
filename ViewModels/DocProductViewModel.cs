using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel; 
using Gamma.Models;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Gamma.Interfaces;
using Gamma.Attributes;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocProductViewModel : DataBaseEditViewModel, ICheckedAccess
    {
        /// <summary>
        /// Initializes a new instance of the DocProductViewModel class.
        /// </summary>
        public DocProductViewModel(OpenDocProductMessage msg)
        {
            switch (msg.DocProductKind)
            {
                case DocProductKinds.DocProductSpool:
                    CurrentViewModel = new DocProductSpoolViewModel(msg.ID,msg.IsNewProduct);
                    if (!msg.IsNewProduct)
                    {
                        Doc = (from d in DB.GammaBase.Docs where 
                                   d.DocID == DB.GammaBase.DocProducts.Where(dp => dp.ProductID == msg.ID).
                                   Select(dp => dp.DocID).FirstOrDefault() select d).FirstOrDefault();
                        DocProduction = DB.GammaBase.DocProduction.Find(Doc.DocID);
                    }
                    break;
                case DocProductKinds.DocProductUnload:
                    CurrentViewModel = new DocProductUnloadViewModel(msg.ID, msg.IsNewProduct);
                    if (!msg.IsNewProduct)
                    {
                        Doc = DB.GammaBase.Docs.Find(msg.ID);
                        DocProduction = DB.GammaBase.DocProduction.Find(msg.ID);
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
                IsConfirmed = Doc.IsConfirmed ?? false;
            }
            Bars = (CurrentViewModel as IBarImplemented).Bars;
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
            Messenger.Default.Register<ParentSaveMessage>(this, SaveToModel);
        }
        private void SaveToModel(ParentSaveMessage msg)
        {
            SaveToModel();
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
        public DateTime? DocDate { get; set; }
        [UIAuth(UIAuthLevel.ReadOnly)]
        public bool IsConfirmed { get; set; }
        public string Number { get; set; }
        private Guid DocProductID { get; set; }
        private Guid? _productionTaskID { get; set; }
        private Guid? ProductionTaskID
        {
            get { return _productionTaskID; }
            set
            {
                _productionTaskID = value;
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
        private DataBaseEditViewModel _currentViewModel;
        public DataBaseEditViewModel CurrentViewModel
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
                Doc = new Docs() { DocID = Guid.NewGuid(), UserID = WorkSession.UserID, IsConfirmed = IsConfirmed };
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
    }
}