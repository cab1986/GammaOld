using DevExpress.Mvvm;
using System;
using System.Linq;
using Gamma.Models;
using Gamma.Dialogs;
using System.Windows;
using Gamma.Common;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SourceSpoolsViewModel : RootViewModel
    {
        /// <summary>
        /// Initializes a new instance of the SourceSpoolsViewModel class.
        /// </summary>
        public SourceSpoolsViewModel(GammaEntities gammaBase = null): base(gammaBase)
        {
            if (!DB.HaveWriteAccess("SourceSpools"))
            {
                SourceSpoolsVisible = Visibility.Collapsed;
                return;
            } 
            if (WorkSession.PlaceGroup == PlaceGroups.Rw || WorkSession.PlaceGroup == PlaceGroups.Convertings) SourceSpoolsVisible = Visibility.Visible;
            else SourceSpoolsVisible = Visibility.Collapsed;
            ChangeUnwinderActiveCommand = new DelegateCommand<byte>(ChangeUnwinderActive);
            DeleteSpoolCommand = new DelegateCommand<byte>(DeleteSpool);
            ChooseSpoolCommand = new DelegateCommand<byte>(ChooseSpool);
            SourceSpools = GammaBase.SourceSpools.FirstOrDefault(s => s.PlaceID == WorkSession.PlaceID);
            if (SourceSpools == null)
            {
                SourceSpools = new SourceSpools() { PlaceID = (int)WorkSession.PlaceID };
                GammaBase.SourceSpools.Add(SourceSpools);
                GammaBase.SaveChanges();
            }
            else
            {
                Unwinder1ProductID = SourceSpools.Unwinder1Spool;
                Unwinder2ProductID = SourceSpools.Unwinder2Spool;
                Unwinder3ProductID = SourceSpools.Unwinder3Spool;
                Unwinder1Active = SourceSpools.Unwinder1Active ?? false;
                Unwinder2Active = SourceSpools.Unwinder2Active ?? false;
                Unwinder3Active = SourceSpools.Unwinder3Active ?? false;
            }
            var unwindersCount = DB.GetUnwindersCount(WorkSession.PlaceID, GammaBase);
            switch (unwindersCount)
            {
                case 1:
                    Unwinder2Visible = false;
                    Unwinder3Visible = false;
                    break;
                case 2:
                    Unwinder3Visible = false;
                    break;
                default:
                    Unwinder3Visible = true;
                    Unwinder2Visible = true;
                    break;
            }
        }
        public DelegateCommand<byte> ChooseSpoolCommand {get; set;}
        public DelegateCommand<byte> DeleteSpoolCommand {get; set;}
        public DelegateCommand<byte> ChangeUnwinderActiveCommand { get; set; }

        private void ChooseSpool(byte unum)
        {
            CurrentUnwinder = unum;
            Messenger.Default.Register<ChoosenProductMessage>(this, SourceSpoolChanged);
            MessageManager.OpenFindProduct(ProductKinds.ProductSpool,true);
        }
        private void CreateRemainderSpool(Guid productId, decimal weight)
        {
            UIServices.SetBusyState();
            var docWithdrawalProduct =
                GammaBase.DocWithdrawalProducts.OrderByDescending(d => d.DocWithdrawal.Docs.Date)
                    .FirstOrDefault(d => d.ProductID == productId);
            if (docWithdrawalProduct == null) return;
            docWithdrawalProduct.Quantity = weight;
            docWithdrawalProduct.CompleteWithdrawal = false;
            GammaBase.SaveChanges();
            ReportManager.PrintReport("Амбалаж", "Spool", docWithdrawalProduct.DocID);
        }

        private byte CurrentUnwinder { get; set; }
        private void SourceSpoolChanged(ChoosenProductMessage msg)
        {
            Messenger.Default.Unregister<ChoosenProductMessage>(this);
            var isWrittenOff = GammaBase.vProductsInfo.Where(p => p.ProductID == msg.ProductID).Select(p => p.IsWrittenOff).FirstOrDefault();
            if (isWrittenOff??false)
            {
                MessageBox.Show("Нельзя повторно использовать списанный тамбур", "Списанный тамбур", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            if (Unwinder1ProductID == msg.ProductID || Unwinder2ProductID == msg.ProductID || Unwinder3ProductID == msg.ProductID)
            {
                MessageBox.Show("Данный тамбур уже установлен на раскат", "На раскате", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            switch (CurrentUnwinder)
            {
                case 1:
                    if (Unwinder1ProductID != null)
                        DeleteSpool(1);
                    Unwinder1ProductID = msg.ProductID;
                    SourceSpools.Unwinder1Spool = msg.ProductID;
                    SourceSpools.Unwinder1Active = Unwinder1Active;
                    break;
                case 2:
                    if (Unwinder2ProductID != null)
                        DeleteSpool(2);
                    Unwinder2ProductID = msg.ProductID;
                    SourceSpools.Unwinder2Spool = msg.ProductID;
                    SourceSpools.Unwinder2Active = Unwinder2Active;
                    break;
                case 3:
                    if (Unwinder3ProductID != null)
                        DeleteSpool(3);
                    Unwinder3ProductID = msg.ProductID;
                    SourceSpools.Unwinder3Spool = msg.ProductID;
                    SourceSpools.Unwinder3Active = Unwinder3Active;
                    break;
                default:
                    break;
            }
            GammaBase.WriteSpoolInstallLog(msg.ProductID, WorkSession.PlaceID, WorkSession.ShiftID, CurrentUnwinder);
            GammaBase.SaveChanges();
        }
        private void DeleteSpool(byte unum)
        {
            switch (unum)
            {
                case 1:
                   if (CheckSpoolIsUsed(Unwinder1ProductID))
                    {
                        var dialog = new ChangeSpoolDialog((Guid)Unwinder1ProductID);
                        dialog.ShowDialog();
                        if (dialog.DialogResult == true)
                        {
                            switch (dialog.ChangeState)
                            {
                                case SpoolChangeState.WithBroke:
                                    BrokeProduct((Guid)Unwinder1ProductID, dialog.Weight, dialog.RejectionReasonID);
                                    break;
                                case SpoolChangeState.WithRemainder:
                                    CreateRemainderSpool((Guid)Unwinder1ProductID, dialog.Weight);
                                    break;
                                case SpoolChangeState.FullyConverted:
                                    ComleteWithdraw((Guid) Unwinder1ProductID);
                                    break;
                            }
                        }
                    }
                    Unwinder1ProductID = null;
                    Unwinder1Active = false;
                    break;
                case 2:
                    if (CheckSpoolIsUsed(Unwinder2ProductID))
                    {
                        var dialog = new ChangeSpoolDialog((Guid)Unwinder2ProductID);
                        dialog.ShowDialog();
                        if (dialog.DialogResult == true)
                        {
                            switch (dialog.ChangeState)
                            {
                                case SpoolChangeState.WithBroke:
                                    BrokeProduct((Guid)Unwinder2ProductID, dialog.Weight, dialog.RejectionReasonID);
                                    break;
                                case SpoolChangeState.WithRemainder:
                                    CreateRemainderSpool((Guid)Unwinder2ProductID, dialog.Weight);
                                    break;
                                case SpoolChangeState.FullyConverted:
                                    ComleteWithdraw((Guid)Unwinder2ProductID);
                                    break;
                            }
                        }
                    }
                    Unwinder2ProductID = null;
                    Unwinder2Active = false;
                    break;
                case 3:
                    if (CheckSpoolIsUsed(Unwinder3ProductID))
                    {
                        var dialog = new ChangeSpoolDialog((Guid)Unwinder3ProductID);
                        dialog.ShowDialog();
                        if (dialog.DialogResult == true)
                        {
                            switch (dialog.ChangeState)
                            {
                                case SpoolChangeState.WithBroke:
                                    BrokeProduct((Guid)Unwinder3ProductID, dialog.Weight, dialog.RejectionReasonID);
                                    break;
                                case SpoolChangeState.WithRemainder:
                                    CreateRemainderSpool((Guid)Unwinder3ProductID, dialog.Weight);
                                    break;
                                case SpoolChangeState.FullyConverted:
                                    ComleteWithdraw((Guid)Unwinder3ProductID);
                                    break;
                            }
                        }
                    }
                    Unwinder3ProductID = null;
                    Unwinder3Active = false;
                    break;
            }
            GammaBase.SaveChanges();
        }

        private void ComleteWithdraw(Guid productId)
        {
            UIServices.SetBusyState();
            var docWithdrawalProduct =
                GammaBase.DocWithdrawalProducts.OrderByDescending(d => d.DocWithdrawal.Docs.Date)
                    .FirstOrDefault(d => d.ProductID == productId);
            if (docWithdrawalProduct == null) return;
            docWithdrawalProduct.CompleteWithdrawal = true;
            GammaBase.SaveChanges();
        }

        private void BrokeProduct(Guid productid, decimal weight, Guid? rejectionReasonId)
        {
            var docID = SqlGuidUtil.NewSequentialid();
            GammaBase.CreateDocBrokeWithBrokeDecision(docID, productid, weight/1000, rejectionReasonId,
                WorkSession.PrintName, WorkSession.PlaceID);
//            MessageManager.OpenDocBroke(docID);
            var docProductionId = GammaBase.DocProductionProducts
                .Where(d => d.ProductID == productid && d.DocProduction.Docs.DocTypeID == (byte)DocTypes.DocProduction)
                .Select(d => d.DocID).FirstOrDefault();
            ReportManager.PrintReport("Амбалаж на утилизацию", "Spool", docProductionId);
        }
        private void ChangeUnwinderActive(byte unum)
        {
            switch (unum)
            {
                case 1:
                    Unwinder1Active = !Unwinder1Active;
                    break;
                case 2:
                    Unwinder2Active = !Unwinder2Active;
                    break;
                case 3:
                    Unwinder3Active = !Unwinder3Active;
                    break;
                default:
                    break;
            }
            GammaBase.SaveChanges();
        }
        private bool _unwinder1Active;
        public bool Unwinder1Active
        {
            get
            {
                return _unwinder1Active;
            }
            set
            {
                if (_unwinder1Active == value)
                    return;
                _unwinder1Active = value;
                SourceSpools.Unwinder1Active = value;
                RaisePropertyChanged("Unwinder1Active");
            }
        }
        private bool _unwinder2Active;
        public bool Unwinder2Active
        {
            get
            {
                return _unwinder2Active;
            }
            set
            {
                if (_unwinder2Active == value)
                    return;
                _unwinder2Active = value;
                SourceSpools.Unwinder2Active = value;
                RaisePropertyChanged("Unwinder2Active");
            }
        }
        private bool _unwinder3Active;
        public bool Unwinder3Active
        {
            get
            {
                return _unwinder3Active;
            }
            set
            {
                if (_unwinder3Active == value)
                    return;
                _unwinder3Active = value;
                SourceSpools.Unwinder3Active = value;
                RaisePropertyChanged("Unwinder3Active");
            }
        }

        public bool Unwinder2Visible { get; set; } = true;
        public bool Unwinder3Visible { get; set; } = true;
        private Guid? _unwinder1ProductID;
        private Guid? _unwinder2ProductID;
        private Guid? _unwinder3ProductID;
        private Guid? Unwinder1ProductID
        {
            get
            {
                return _unwinder1ProductID;
            }
            set
            {
                if (_unwinder1ProductID == value) return;
            	_unwinder1ProductID = value;
                SourceSpools.Unwinder1Spool = value;
                Unwinder1Nomenclature = ProductNomenclature(_unwinder1ProductID);
            }
        }
        private Guid? Unwinder2ProductID
        {
            get
            {
                return _unwinder2ProductID;
            }
            set
            {
                if (_unwinder2ProductID == value) return;
                _unwinder2ProductID = value;
                SourceSpools.Unwinder2Spool = value;
                Unwinder2Nomenclature = ProductNomenclature(_unwinder2ProductID);
            }
        }
        private Guid? Unwinder3ProductID
        {
            get
            {
                return _unwinder3ProductID;
            }
            set
            {
                if (_unwinder3ProductID == value) return;
                _unwinder3ProductID = value;
                SourceSpools.Unwinder3Spool = value;
                Unwinder3Nomenclature = ProductNomenclature(_unwinder3ProductID);
            }
        }
        private string _unwinder1Nomenclature;
        private string _unwinder2Nomenclature;
        private string _unwinder3Nomenclature;
        public string Unwinder1Nomenclature
        {
            get
            {
                return _unwinder1Nomenclature;
            }
            set
            {
                _unwinder1Nomenclature = value;
                RaisePropertyChanged("Unwinder1Nomenclature");
            }
        }
        public string Unwinder2Nomenclature
        {
            get
            {
                return _unwinder2Nomenclature;
            }
            set
            {
                _unwinder2Nomenclature = value;
                RaisePropertyChanged("Unwinder2Nomenclature");
            }
        }
        public string Unwinder3Nomenclature
        {
            get
            {
                return _unwinder3Nomenclature;
            }
            set
            {
                _unwinder3Nomenclature = value;
                RaisePropertyChanged("Unwinder3Nomenclature");
            }
        }
        private string ProductNomenclature(Guid? productid)
        {
            return (from pspool in GammaBase.ProductSpools
                    where pspool.ProductID == productid
                    select "№" + pspool.Products.Number + " " + pspool.C1CNomenclature.Name + " " + pspool.C1CCharacteristics.Name).FirstOrDefault();
        }
        private SourceSpools SourceSpools { get; set; }
        private bool CheckSpoolIsUsed(Guid? productid)
        {
            return GammaBase.DocWithdrawal.Any(dw => GammaBase.DocWithdrawalProducts.Where
                (dp => dp.ProductID == productid).Select(dp => dp.DocID).Contains(dw.DocID));
        }
        private Visibility _sourceSpoolsVisible;
        public Visibility SourceSpoolsVisible
        {
            get
            {
                return _sourceSpoolsVisible;
            }
            set
            {
                _sourceSpoolsVisible = value;
                RaisePropertyChanged("SourceSpoolsVisible");
            }
        }
    }
}