// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using DevExpress.Mvvm;
using System;
using System.Data.Entity;
using System.Linq;
using Gamma.Dialogs;
using System.Windows;
using Gamma.Common;
using Gamma.Entities;

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
        public SourceSpoolsViewModel()
        {
            if (!DB.HaveWriteAccess("SourceSpools"))
            {
                SourceSpoolsVisible = Visibility.Collapsed;
                return;
            }
            GammaBase = DB.GammaDb;
            
                if (WorkSession.PlaceGroup == PlaceGroup.Rw || WorkSession.PlaceGroup == PlaceGroup.Convertings) SourceSpoolsVisible = Visibility.Visible;
                else SourceSpoolsVisible = Visibility.Collapsed;
                ChangeUnwinderActiveCommand = new DelegateCommand<byte>(ChangeUnwinderActive);
                DeleteSpoolCommand = new DelegateCommand<byte>(x => DeleteSpool(x));
                ChooseSpoolCommand = new DelegateCommand<byte>(ChooseSpool);
                OpenSpoolInfoCommand = new DelegateCommand<byte>(OpenSpoolInfo);
                SourceSpools = GammaBase.SourceSpools.FirstOrDefault(s => s.PlaceID == WorkSession.PlaceID);
                if (SourceSpools == null)
                {
                    SourceSpools = new SourceSpools() { PlaceID = WorkSession.PlaceID };
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

        private void OpenSpoolInfo(byte unwinderId)
        {
            switch (unwinderId)
            {
                case 1:
                    if (Unwinder1ProductID == null) return;
                    MessageManager.OpenDocProduct(ProductKind.ProductSpool, (Guid)Unwinder1ProductID);
                    break;
                case 2:
                    if (Unwinder2ProductID == null) return;
                    MessageManager.OpenDocProduct(ProductKind.ProductSpool, (Guid)Unwinder2ProductID);
                    break;
                case 3:
                    if (Unwinder3ProductID == null) return;
                    MessageManager.OpenDocProduct(ProductKind.ProductSpool, (Guid)Unwinder3ProductID);
                    break;
            }
        }

        public DelegateCommand<byte> ChooseSpoolCommand {get; set;}
        public DelegateCommand<byte> DeleteSpoolCommand {get; set;}
        public DelegateCommand<byte> ChangeUnwinderActiveCommand { get; set; }
        public DelegateCommand<byte> OpenSpoolInfoCommand { get; private set; }

        private void ChooseSpool(byte unum)
        {
            CurrentUnwinder = unum;
            Messenger.Default.Register<ChoosenProductMessage>(this, SourceSpoolChanged);
            MessageManager.OpenFindProduct(ProductKind.ProductSpool,true);
        }

        private void CreateRemainderSpool(Guid productId, decimal weight)
        {
            UIServices.SetBusyState();
            
                var docWithdrawalProduct =
                GammaBase.DocWithdrawalProducts.OrderByDescending(d => d.DocWithdrawal.Docs.Date).Include(d => d.DocWithdrawal.Docs)
                    .FirstOrDefault(d => d.ProductID == productId);
                if (docWithdrawalProduct == null || docWithdrawalProduct.Quantity != null || docWithdrawalProduct.CompleteWithdrawal == true)
                {
                    var docId = SqlGuidUtil.NewSequentialid();
                    docWithdrawalProduct = new DocWithdrawalProducts
                    {
                        DocID = docId,
                        ProductID = productId,
                        DocWithdrawal = new DocWithdrawal
                        {
                            DocID = docId,
                            OutPlaceID = WorkSession.PlaceID,
                            Docs = new Docs
                            {
                                DocID = docId,
                                IsConfirmed = true,
                                Date = DB.CurrentDateTime,
                                DocTypeID = (int)DocTypes.DocWithdrawal,
                                PlaceID = WorkSession.PlaceID,
                                PrintName = WorkSession.PrintName,
                                ShiftID = WorkSession.ShiftID,
                                UserID = WorkSession.UserID
                            }
                        }
                    };
                    GammaBase.DocWithdrawalProducts.Add(docWithdrawalProduct);
                };
                var product = GammaBase.vProductsInfo.First(p => p.ProductID == productId);
                docWithdrawalProduct.Quantity = product.BaseMeasureUnitQuantity - weight / 1000;
                docWithdrawalProduct.CompleteWithdrawal = false;
                docWithdrawalProduct.DocWithdrawal.Docs.IsConfirmed = true;
                GammaBase.SaveChanges();
                ReportManager.PrintReport("Амбалаж", "Spool", docWithdrawalProduct.ProductID);
        }

        private byte CurrentUnwinder { get; set; }

        private void SourceSpoolChanged(ChoosenProductMessage msg)
        {
            Messenger.Default.Unregister<ChoosenProductMessage>(this);
            using (var gammaBase = DB.GammaDb)
            {
                var isWrittenOff = gammaBase.vProductsInfo.Where(p => p.ProductID == msg.ProductID).Select(p => p.IsWrittenOff).FirstOrDefault();
                if (isWrittenOff ?? false)
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
                            if (!DeleteSpool(1)) return;
                        Unwinder1ProductID = msg.ProductID;
                        SourceSpools.Unwinder1Spool = msg.ProductID;
                        SourceSpools.Unwinder1Active = Unwinder1Active;
                        break;
                    case 2:
                        if (Unwinder2ProductID != null)
                            if (!DeleteSpool(2)) return;
                        Unwinder2ProductID = msg.ProductID;
                        SourceSpools.Unwinder2Spool = msg.ProductID;
                        SourceSpools.Unwinder2Active = Unwinder2Active;
                        break;
                    case 3:
                        if (Unwinder3ProductID != null)
                            if (!DeleteSpool(3)) return;
                        Unwinder3ProductID = msg.ProductID;
                        SourceSpools.Unwinder3Spool = msg.ProductID;
                        SourceSpools.Unwinder3Active = Unwinder3Active;
                        break;
                }
                gammaBase.WriteSpoolInstallLog(msg.ProductID, WorkSession.PlaceID, WorkSession.ShiftID, CurrentUnwinder);
                gammaBase.SaveChanges();
            }
        }

        private bool DeleteSpool(byte unum)
        {
            switch (unum)
            {
                case 1:
                   if (Unwinder1ProductID == null) return true;
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
                                    CompleteWithdraw((Guid) Unwinder1ProductID);
                                    break;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    Unwinder1ProductID = null;
                    Unwinder1Active = false;
                    break;
                case 2:
                    if (Unwinder2ProductID == null) return true;
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
                                    CompleteWithdraw((Guid)Unwinder2ProductID);
                                    break;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    Unwinder2ProductID = null;
                    Unwinder2Active = false;
                    break;
                case 3:
                    if (Unwinder3ProductID == null) return true;
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
                                    CompleteWithdraw((Guid)Unwinder3ProductID);
                                    break;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    Unwinder3ProductID = null;
                    Unwinder3Active = false;
                    break;
            }
            GammaBase.SaveChanges();
            return true;
        }

        private void CompleteWithdraw(Guid productId)
        {
            UIServices.SetBusyState();
            using (var gammaBase = DB.GammaDb)
            {
                var docWithdrawalProduct =
                gammaBase.DocWithdrawalProducts.OrderByDescending(d => d.DocWithdrawal.Docs.Date).Include(d => d.DocWithdrawal.Docs)
                    .FirstOrDefault(d => d.ProductID == productId);
                if (docWithdrawalProduct?.CompleteWithdrawal == true) return;
                if (docWithdrawalProduct == null || docWithdrawalProduct.Quantity != null)
                {
                    var docId = SqlGuidUtil.NewSequentialid();
                    docWithdrawalProduct = new DocWithdrawalProducts
                    {
                        DocID = docId,
                        ProductID = productId,
                        DocWithdrawal = new DocWithdrawal
                        {
                            DocID = docId,
                            OutPlaceID = WorkSession.PlaceID,
                            Docs = new Docs
                            {
                                DocID = docId,
                                IsConfirmed = true,
                                Date = DB.CurrentDateTime,
                                DocTypeID = (int)DocTypes.DocWithdrawal,
                                PlaceID = WorkSession.PlaceID,
                                PrintName = WorkSession.PrintName,
                                ShiftID = WorkSession.ShiftID,
                                UserID = WorkSession.UserID
                            }
                        }
                    };
                    gammaBase.DocWithdrawalProducts.Add(docWithdrawalProduct);
                };
                var product = gammaBase.vProductsInfo.First(p => p.ProductID == productId);
                docWithdrawalProduct.Quantity = product.BaseMeasureUnitQuantity;
                docWithdrawalProduct.CompleteWithdrawal = true;
                docWithdrawalProduct.DocWithdrawal.Docs.IsConfirmed = true;
                gammaBase.SaveChanges();
            }
        }

        private void BrokeProduct(Guid productId, decimal weight, Guid? rejectionReasonId)
        {
            UIServices.SetBusyState();
            var docID = SqlGuidUtil.NewSequentialid();
            using (var gammaBase = DB.GammaDb)
            {
                gammaBase.CreateDocBrokeWithBrokeDecision(docID, productId, weight / 1000, rejectionReasonId,
                WorkSession.PrintName, WorkSession.PlaceID);
            }
            ReportManager.PrintReport("Амбалаж", "Spool", productId);
        }

        private void ChangeUnwinderActive(byte unum)
        {
            switch (unum)
            {
                case 1:
                    if (Unwinder1ProductID == null)
                    {
                        Unwinder1Active = false;
                        return;
                    }
                    Unwinder1Active = !Unwinder1Active;
                    GammaBase.SaveChanges();
                    if (Unwinder1Active && WorkSession.PlaceGroup == PlaceGroup.Convertings) CheckSourceSpools();
                    break;
                case 2:
                    if (Unwinder2ProductID == null)
                    {
                        Unwinder2Active = false;
                        return;
                    }
                    Unwinder2Active = !Unwinder2Active;
                    GammaBase.SaveChanges();
                    if (Unwinder2Active && WorkSession.PlaceGroup == PlaceGroup.Convertings) CheckSourceSpools();
                    break;
                case 3:
                    if (Unwinder3ProductID == null)
                    {
                        Unwinder3Active = false;
                        return;
                    }
                    Unwinder3Active = !Unwinder3Active;
                    GammaBase.SaveChanges();
                    if (Unwinder3Active && WorkSession.PlaceGroup == PlaceGroup.Convertings) CheckSourceSpools();
                    break;
            }
        }

        private void CheckSourceSpools()
        {
            using (var gammaBase = DB.GammaDb)
            {
                var productionTaskID =
                    gammaBase.ActiveProductionTasks.FirstOrDefault(pt => pt.PlaceID == WorkSession.PlaceID)?
                        .ProductionTaskID;
                if (productionTaskID == null) return;
                var checkResult = gammaBase.CheckProductionTaskSourceSpools(WorkSession.PlaceID, productionTaskID).FirstOrDefault();
                if (!string.IsNullOrEmpty(checkResult?.ResultMessage))
                {
                    MessageBox.Show(checkResult.ResultMessage, "Предупреждение", MessageBoxButton.OK,
                        MessageBoxImage.Asterisk);
                }
            }
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

        private bool CheckSpoolIsUsed(Guid? productId)
        {
            var isUsed = GammaBase.DocWithdrawalProducts.Any(
                    dw =>
                        dw.ProductID == productId && dw.Quantity == null &&
                        (dw.CompleteWithdrawal == null || dw.CompleteWithdrawal == false));
            if (isUsed) return true;
            var number = GammaBase.Products.First(p => p.ProductID == productId).Number;
            return 
                MessageBox.Show($"Из тамбура № {number} изготовлена продукция?", "Переботка тамбура", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes;
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