using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Linq;
using Gamma.Models;
using Gamma.Dialogs;
using System.Windows;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SourceSpoolsViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the SourceSpoolsViewModel class.
        /// </summary>
        public SourceSpoolsViewModel()
        {
            if (WorkSession.PlaceGroup == PlaceGroups.RW || WorkSession.PlaceGroup == PlaceGroups.Convertings) SourceSpoolsVisible = Visibility.Visible;
            else SourceSpoolsVisible = Visibility.Collapsed;
            ChangeUnwinderActiveCommand = new RelayCommand<short>(ChangeUnwinderActive);
            DeleteSpoolCommand = new RelayCommand<short>(DeleteSpool);
            ChooseSpoolCommand = new RelayCommand<short>(ChooseSpool);
            SourceSpools = DB.GammaBase.SourceSpools.Where(s => s.PlaceID == WorkSession.PlaceID).FirstOrDefault();
            if (SourceSpools == null)
            {
                SourceSpools = new SourceSpools() { PlaceID = (int)WorkSession.PlaceID };
                DB.GammaBase.SourceSpools.Add(SourceSpools);
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
        }
        public RelayCommand<short> ChooseSpoolCommand {get; set;}
        public RelayCommand<short> DeleteSpoolCommand {get; set;}
        public RelayCommand<short> ChangeUnwinderActiveCommand { get; set; }
        private void ChooseSpool(short unum)
        {
            CurrentUnwinder = unum;
            Messenger.Default.Register<ChoosenSourceProductMessage>(this, SourceSpoolChanged);
            MessageManager.OpenFindProduct(new FindProductMessage { ChooseSourceProduct = true, ProductKind = ProductKinds.ProductSpool });
        }
        private short CurrentUnwinder { get; set; }
        private void SourceSpoolChanged(ChoosenSourceProductMessage msg)
        {
            Messenger.Default.Unregister<ChoosenSourceProductMessage>(this);
            switch (CurrentUnwinder)
            {
                case 1:
                    DeleteSpool(1);
                    Unwinder1ProductID = msg.ProductID;
                    break;
                case 2:
                    DeleteSpool(2);
                    Unwinder2ProductID = msg.ProductID;
                    break;
                case 3:
                    DeleteSpool(3);
                    Unwinder3ProductID = msg.ProductID;
                    break;
                default:
                    break;
            }
            DB.GammaBase.SaveChanges();
        }
        private void DeleteSpool(short unum)
        {
            switch (unum)
            {
                case 1:
                    if (!CheckSpoolIsUsed(Unwinder1ProductID))
                    {
                        Unwinder1ProductID = null;
                        Unwinder1Active = false;
                    }
                    else
                    {
                        var dialog = new ChangeSpoolDialog();
                        dialog.ShowDialog();
                        if (dialog.DialogResult == true)
                        {
                            switch (dialog.ChangeState)
                            {
                                case SpoolChangeState.FullyConverted:
                                    Unwinder1ProductID = null;
                                    Unwinder1Active = false;
                                    break;
                                case SpoolChangeState.WithBroke:
                                    break;
                                case SpoolChangeState.WithRemainder:
                                    break;
                            }
                        }
                    }
                    break;
                case 2:
                    if (!CheckSpoolIsUsed(Unwinder2ProductID))
                    {
                        Unwinder2ProductID = null;
                        Unwinder2Active = false;
                    }
                    else
                    {
                        var dialog = new ChangeSpoolDialog();
                        dialog.ShowDialog();
                        if (dialog.DialogResult == true)
                        {
                            switch (dialog.ChangeState)
                            {
                                case SpoolChangeState.FullyConverted:
                                    Unwinder2ProductID = null;
                                    Unwinder2Active = false;
                                    break;
                                case SpoolChangeState.WithBroke:
                                    break;
                                case SpoolChangeState.WithRemainder:
                                    break;
                            }
                        }
                    }
                    break;
                case 3:
                    if (!CheckSpoolIsUsed(Unwinder3ProductID))
                    {
                        Unwinder3ProductID = null;
                        Unwinder3Active = false;
                    }
                    else
                    {
                        var dialog = new ChangeSpoolDialog();
                        dialog.ShowDialog();
                        if (dialog.DialogResult == true)
                        {
                            switch (dialog.ChangeState)
                            {
                                case SpoolChangeState.FullyConverted:
                                    Unwinder3ProductID = null;
                                    Unwinder3Active = false;
                                    break;
                                case SpoolChangeState.WithBroke:
                                    break;
                                case SpoolChangeState.WithRemainder:
                                    break;
                            }
                        }
                    }
                    break;
            }
            DB.GammaBase.SaveChanges();
        }
        private void ChangeUnwinderActive(short unum)
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
            DB.GammaBase.SaveChanges();
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
        private bool _unwinder3Visible = true;
        public bool Unwinder3Visible
        {
            get
            {
                return _unwinder3Visible;
            }
            set
            {
                if (_unwinder3Visible == value)
                    return;
                _unwinder3Visible = value;
                RaisePropertyChanged("Unwinder3Visible");
            }
        }
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
        private string ProductNomenclature(Guid? productID)
        {
            return (from pspool in DB.GammaBase.ProductSpools
                    where pspool.ProductID == productID
                    select pspool.C1CNomenclature.Name + " " + pspool.C1CCharacteristics.Name).FirstOrDefault();
        }
        private SourceSpools SourceSpools { get; set; }
        private bool CheckSpoolIsUsed(Guid? productID)
        {
            return DB.GammaBase.DocWithdrawal.Any(dw => dw.DocID == DB.GammaBase.DocProducts.Where
                (dp => dp.ProductID == productID).Select(dp => dp.DocID).FirstOrDefault());
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