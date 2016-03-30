using System;
using System.Linq;
using Gamma.Interfaces;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Windows;
using Gamma.Attributes;
using GalaSoft.MvvmLight.Messaging;
using System.ComponentModel.DataAnnotations;
using Gamma.Models;
using Gamma.Common;
using System.Data.Entity;

namespace Gamma.ViewModels
{
    class DocProductGroupPackViewModel : SaveImplementedViewModel, ICheckedAccess, IBarImplemented
    {
        public DocProductGroupPackViewModel()
        {
            IsNewGroupPack = true;
            GetGrossWeightCommand = new RelayCommand(GetGrossWeight, () => Scales.IsReady && !IsConfirmed);
            GetWeightCommand = new RelayCommand(GetWeight, () => Scales.IsReady && !IsConfirmed);
            AddSpoolCommand = new RelayCommand(AddSpool, () => !IsReadOnly);
            DeleteSpoolCommand = new RelayCommand(DeleteSpool, () => SelectedSpool != null && !IsReadOnly);
            OpenSpoolCommand = new RelayCommand(OpenSpool, () => SelectedSpool != null);
            Spools = new AsyncObservableCollection<PaperBase>();
            Bars.Add(ReportManager.GetReportBar("GroupPacks", VMID));
        }
        public DocProductGroupPackViewModel(Guid docID) : this() 
        {
            var doc = DB.GammaBase.Docs.Include(d => d.DocProducts).Where(d => d.DocID == docID).First();
            IsConfirmed = doc.IsConfirmed;
            IsNewGroupPack = false;
            if (doc.DocProducts.Count > 0)
            {
                var productID = doc.DocProducts.Select(d => d.ProductID).First();
                var productGroupPack = DB.GammaBase.ProductGroupPacks.Where(p => p.ProductID == productID).Select(p => p).FirstOrDefault();
                Weight = Convert.ToInt32(productGroupPack.Weight);
                GrossWeight = Convert.ToInt32(productGroupPack.GrossWeight);
            }
            var groupPackSpools = DB.GammaBase.GetGroupPackSpools(docID).ToList();
            if (groupPackSpools.Count() > 0)
            {
                BaseCoreWeight = DB.GetSpoolCoreWeight(groupPackSpools[0].ProductID);
            }
            if (groupPackSpools.Count > 0)
            {
                PlaceProductionID = groupPackSpools[0].PlaceID;
                foreach (var groupPackSpool in groupPackSpools)
                {
                    Spools.Add(new PaperBase()
                    {
                        ProductID = groupPackSpool.ProductID,
                        DocID = groupPackSpool.DocID,
                        NomenclatureID = (Guid)groupPackSpool.C1CNomenclatureID,
                        CharacteristicID = (Guid)groupPackSpool.C1CCharacteristicID,
                        Number = String.Format("{0} от {1}", groupPackSpool.Number, groupPackSpool.Date),
                        Weight = Convert.ToInt32(groupPackSpool.Quantity),
                        Nomenclature = groupPackSpool.NomenclatureName,
                    });
                }
                NomenclatureID = Spools[0].NomenclatureID;
                CharacteristicID = Spools[0].CharacteristicID;
                Diameter = DB.GetSpoolDiameter(Spools[0].ProductID);
                CoreWeight = BaseCoreWeight * Spools.Count;
            }
        }
        private int Format { get; set; }
        private bool IsNewGroupPack { get; set; }
        private void GetWeight()
        {
            Weight = Convert.ToInt32(Scales.GetWeight());            
        }

        private void GetGrossWeight()
        {
            GrossWeight = Convert.ToInt32(Scales.GetWeight());
        }
        private bool IsConfirmed { get; set; }
        public bool IsReadOnly
        {
            get 
            {
                return ((!DB.HaveWriteAccess("ProductGroupPacks") || IsConfirmed) && IsValid);
            }
        }
        private Guid? _vmID = Guid.NewGuid();
        public Guid? VMID
        {
            get
            {
                return _vmID;
            }
        }

        private int _weight;
        private int _grossWeight;
        [UIAuth(UIAuthLevel.ReadOnly)]
        [Range(1,1000000,ErrorMessage="Значение должно быть больше 0")]
        public int Weight
        {
            get
            {
                return _weight;
            }
            set
            {
            	_weight = value;
                RaisePropertyChanged("Weight");
            }
        }
        [Range(1, 1000000, ErrorMessage = "Значение должно быть больше 0")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public int GrossWeight
        {
            get
            {
                return _grossWeight;
            }
            set
            {
            	_grossWeight = value;
                RaisePropertyChanged("GrossWeight");
            }
        }
        private short _diameter;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public short Diameter
        {
            get
            {
                return _diameter;
            }
            set
            {
            	_diameter = value;
                RaisePropertyChanged("Diameter");
            }
        }
        private decimal _coreWeight;
        public decimal CoreWeight
        {
            get
            {
                return _coreWeight;
            }
            set
            {
            	_coreWeight = value;
                RaisePropertyChanged("CoreWeight");
            }
        }
        public RelayCommand GetWeightCommand { get; private set; }
        public RelayCommand GetGrossWeightCommand { get; private set; }
        public RelayCommand AddSpoolCommand { get; private set; }
        public RelayCommand DeleteSpoolCommand { get; private set; }
        private void AddSpool()
        {
            MessageManager.OpenFindProduct(ProductKinds.ProductSpool, true);
            Messenger.Default.Register<ChoosenProductMessage>(this, AddChoosenSpool);
        }
        private void DeleteSpool()
        {
            Spools.Remove(SelectedSpool);
            CoreWeight = BaseCoreWeight * Spools.Count;
        }
        private void AddChoosenSpool(ChoosenProductMessage msg)
        {
            Messenger.Default.Unregister(this);
            var spool = (from p in DB.GammaBase.vProductsInfo where p.ProductID == msg.ProductID
                             select p).ToList().Select( p => new PaperBase()
                             {
                                 PlaceProductionID = p.PlaceID,
                                 DocID = p.DocID,
                                 CharacteristicID = (Guid)p.C1CCharacteristicID,
                                 NomenclatureID = (Guid)p.C1CNomenclatureID,
                                 Nomenclature = p.NomenclatureName,
                                 ProductID = p.ProductID,
                                 Weight = p.Quantity ?? 0,
                                 Number = p.Number + " от " + p.Date
                             }).FirstOrDefault();
            AddSpoolIfCorrect(spool);
        }
        public void AddSpool(string barcode)
        {
            var p = (from pinf in DB.GammaBase.vProductsInfo
                         where pinf.BarCode == barcode
                         select pinf).FirstOrDefault();
            if (p == null) return;
            if (p.IsWrittenOff)
            {
                MessageBox.Show("Вы пытаететесь добавить списанный рулон");
                return;
            }
            var spool = new PaperBase()
                             {
                                 PlaceProductionID = p.PlaceID,
                                 DocID = p.DocID,
                                 CharacteristicID = (Guid)p.C1CCharacteristicID,
                                 NomenclatureID = (Guid)p.C1CNomenclatureID,
                                 Nomenclature = p.NomenclatureName,
                                 ProductID = p.ProductID,
                                 Weight = p.Quantity ?? 0,
                                 Number = string.Format("{0} от {1}", p.Number, p.Date)
                             };
            AddSpoolIfCorrect(spool);
        }
        private decimal BaseCoreWeight { get; set; }
        public void AddSpoolIfCorrect(PaperBase spool)
        {
            if (Spools.Count == 0)
            {
                PlaceProductionID = spool.PlaceProductionID;
                NomenclatureID = spool.NomenclatureID;
                CharacteristicID = spool.CharacteristicID;
                BaseCoreWeight = DB.GetSpoolCoreWeight(spool.ProductID);
                Diameter = DB.GetSpoolDiameter(spool.ProductID);              
                Spools.Add(spool);
                CoreWeight = BaseCoreWeight * Spools.Count;
            }
            else
            {
                if (spool.PlaceProductionID != PlaceProductionID)
                {
                    MessageBox.Show("Нельзя упаковывать рулоны с разных переделов",
                        "Рулон другого передела", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return;
                }
                if (spool.NomenclatureID != NomenclatureID || spool.CharacteristicID != CharacteristicID)
                {
                    MessageBox.Show("Номенклатура тамбура не совпадает с номенклатурой ГУ");
                    return;
                }
                if (Spools.Select(s => s.ProductID).Contains(spool.ProductID))
                {
                    MessageBox.Show("Данный рулон уже занесен");
                    return;
                }
                else
                {
                    Spools.Add(spool);
                    CoreWeight = BaseCoreWeight * Spools.Count;
                }
            }
        }
        private int? PlaceProductionID { get; set; }
        private Guid NomenclatureID {get; set; }
        private Guid CharacteristicID { get; set; }
        private ObservableCollection<PaperBase> _spools;
        public ObservableCollection<PaperBase> Spools
        {
            get
            {
                return _spools;
            }
            set
            {
            	_spools = value;
                RaisePropertyChanged("Spools");
            }
        }
        public override void SaveToModel(Guid itemID)
        {
            if (!DB.HaveWriteAccess("ProductGroupPacks") || !IsValid) return;
            var doc = DB.GammaBase.Docs.Include(d => d.DocProduction).Include(d => d.DocProduction.DocWithdrawal)
                .Where(d => d.DocID == itemID).Select(d => d).First();
            if (doc.DocProduction == null)
                doc.DocProduction = new DocProduction()
                {
                    DocID = doc.DocID,
                    InPlaceID = doc.PlaceID
                };
            var product = DB.GammaBase.Products.Include(p => p.ProductGroupPacks).Where(p => p.DocProducts.FirstOrDefault().DocID == itemID).FirstOrDefault();
            if (product == null)
            {
                var productID = SQLGuidUtil.NewSequentialId();
                product = new Products()
                {
                    ProductID = productID,
                    ProductKindID = (byte)ProductKinds.ProductGroupPack,
                    ProductGroupPacks = new ProductGroupPacks()
                };
                doc.DocProducts.Add(new DocProducts()
                {
                    Products = product,
                    DocID = doc.DocID,
                    IsInConfirmed = doc.IsConfirmed
                });
//                DB.GammaBase.Products.Add(product);
            }
            if (product.ProductGroupPacks == null) product.ProductGroupPacks = new ProductGroupPacks() { ProductID = product.ProductID };
            product.ProductGroupPacks.C1CNomenclatureID = NomenclatureID;
            product.ProductGroupPacks.C1CCharacteristicID = CharacteristicID;
            product.ProductGroupPacks.Weight = Weight;
            product.ProductGroupPacks.GrossWeight = GrossWeight;
            product.ProductGroupPacks.Diameter = Diameter;
            Docs docWithdrawal = new Docs();
            if (doc.DocProduction.DocWithdrawal.Count > 0)
            {
                var docWithdrawalID = doc.DocProduction.DocWithdrawal.FirstOrDefault().DocID;
                docWithdrawal = DB.GammaBase.Docs.Include(d => d.DocWithdrawal).Include(d => d.DocProducts)
                    .Where(d => d.DocID == docWithdrawalID).First();
            }
            else 
            {
                docWithdrawal.DocID = SQLGuidUtil.NewSequentialId();
                docWithdrawal.PlaceID = WorkSession.PlaceID;
                docWithdrawal.Date = DB.CurrentDateTime;
                docWithdrawal.UserID = WorkSession.UserID;
                docWithdrawal.ShiftID = WorkSession.ShiftID;
                docWithdrawal.PrintName = WorkSession.PrintName;
                docWithdrawal.DocTypeID = (byte)DocTypes.DocWithdrawal;
                docWithdrawal.IsConfirmed = doc.IsConfirmed;
                var docProductions = new ObservableCollection<DocProduction>();
                docProductions.Add(doc.DocProduction);
                docWithdrawal.DocWithdrawal = new DocWithdrawal()
                {
                    DocID = docWithdrawal.DocID,
                    OutPlaceID = docWithdrawal.PlaceID,
                    DocProduction = docProductions
                };
                docWithdrawal.DocProducts = new ObservableCollection<DocProducts>();
                DB.GammaBase.Docs.Add(docWithdrawal);
            }
            docWithdrawal.DocProducts.Clear();
            foreach (var spool in Spools)
                {
                    docWithdrawal.DocProducts.Add(new DocProducts()
                    {
                        DocID = docWithdrawal.DocID,
                        IsInConfirmed = doc.IsConfirmed,
                        ProductID = spool.ProductID
                    });
                }
            DB.GammaBase.SaveChanges();
        }
        public PaperBase SelectedSpool { get; set; }

        private ObservableCollection<BarViewModel> _bars = new ObservableCollection<BarViewModel>();
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
        public RelayCommand OpenSpoolCommand { get; private set; }
        private void OpenSpool()
        {
            if (SelectedSpool == null) return;
            MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedSpool.DocID);  
        }
        public override bool IsValid
        {
            get
            {
                return base.IsValid && Spools.Count > 0;
            }
        }
    }
}
