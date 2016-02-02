using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Interfaces;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Windows;
using Gamma.Attributes;
using GalaSoft.MvvmLight.Messaging;
using System.ComponentModel.DataAnnotations;
using Gamma.Models;

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
            Spools = new ObservableCollection<PaperBase>();
        }
        public DocProductGroupPackViewModel(Guid productID) : this() 
        {
            var doc = DB.GammaBase.Docs.Where(d => d.DocProducts.Select(dp => dp.ProductID).Contains(productID) &&
                d.DocTypeID == (byte)DocTypes.DocProduction).First();
            IsConfirmed = doc.IsConfirmed;
            IsNewGroupPack = false;
            var productGroupPack = DB.GammaBase.ProductGroupPacks.Where(p => p.ProductID == productID).Select(p => p).First();
            NomenclatureID = (Guid)productGroupPack.C1CNomenclatureID;
            CharacteristicID = (Guid)productGroupPack.C1CCharacteristicID;
            Weight = Convert.ToInt32(productGroupPack.Weight);
            GrossWeight = Convert.ToInt32(productGroupPack.GrossWeight);
            BaseCoreWeight = DB.GetCoreWeight(CharacteristicID);
            var groupPackSpools = DB.GammaBase.GetGroupPackSpools(productID);
            foreach (var groupPackSpool in groupPackSpools)
            {
                Spools.Add(new PaperBase()
                    {
                        ProductID = groupPackSpool.ProductID,
                        NomenclatureID = (Guid)groupPackSpool.C1CNomenclatureID,
                        CharacteristicID = (Guid)groupPackSpool.C1CCharacteristicID,
                        Number = groupPackSpool.Number + " от " + groupPackSpool.Date,
                        Weight = Convert.ToInt32(groupPackSpool.Quantity),
                        Nomenclature = groupPackSpool.NomenclatureName,
                    });
            }
            Diameter = DB.GetSpoolDiameter(Spools[0].ProductID);
            CoreWeight = BaseCoreWeight * Spools.Count;
        }
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
                return (!DB.HaveWriteAccess("ProductGroupPacks") || IsConfirmed);
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
        private int _diameter;
        [Range(1, 1000000, ErrorMessage = "Значение должно быть больше 0")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public int Diameter
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
            var spool = (from p in DB.GammaBase.vProductsInfo
                         where p.BarCode == barcode
                         select new PaperBase
                         {
                             CharacteristicID = (Guid)p.C1CCharacteristicID,
                             NomenclatureID = (Guid)p.C1CNomenclatureID,
                             Nomenclature = p.NomenclatureName,
                             ProductID = p.ProductID,
                             Weight = p.Quantity ?? 0,
                             Number = p.Number + " от " + p.Date
                         }).FirstOrDefault();
            AddSpoolIfCorrect(spool);
        }
        private decimal BaseCoreWeight { get; set; }
        public void AddSpoolIfCorrect(PaperBase spool)
        {
            if (Spools.Count == 0)
            {
                NomenclatureID = spool.NomenclatureID;
                CharacteristicID = spool.CharacteristicID;
                BaseCoreWeight = DB.GetSpoolCoreWeight(spool.ProductID);
                Diameter = DB.GetSpoolDiameter(spool.ProductID);
                Spools.Add(spool);
                CoreWeight = BaseCoreWeight * Spools.Count;
            }
            else
            {
                if (spool.NomenclatureID != NomenclatureID || spool.CharacteristicID != CharacteristicID)
                {
                    MessageBox.Show("Номенклатура тамбура не совпадает с номенклатурой ГУ");
                    return;
                }
                var doc = (from dp in DB.GammaBase.DocProducts
                           where
                               dp.ProductID == spool.ProductID &&
                               (dp.Docs.DocTypeID == (int)DocTypes.DocProduction || dp.Docs.DocTypeID == (int)DocTypes.DocWithdrawal)
                           orderby dp.Docs.Date descending
                           select dp.Docs
                               ).Take(1).FirstOrDefault();
                if (doc.DocTypeID == (int)DocTypes.DocWithdrawal)
                {
                    MessageBox.Show("Вы пытаететесь добавить списанное изделие");
                    return;
                }
                if (!Spools.Select(s => s.ProductID).Contains(spool.ProductID))
                {
                    Spools.Add(spool);               
                    CoreWeight = BaseCoreWeight * Spools.Count;
                }
                    
            }
        }
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
        public override void SaveToModel(Guid ItemID)
        {
            var docProduction = DB.GammaBase.Docs.Where(d => d.DocID == ItemID).Select(d => d).First();
            if (IsNewGroupPack)
            {
               var productID = SQLGuidUtil.NewSequentialId();
               var product = new Products()
               {
                   ProductID = productID,
                   ProductKindID = (byte)ProductKinds.ProductGroupPack,
                   ProductGroupPacks = new ProductGroupPacks()
                   {
                       C1CNomenclatureID = NomenclatureID,
                       C1CCharacteristicID = CharacteristicID,
                       GrossWeight = GrossWeight,
                       Weight = Weight,
                       ProductID = productID
                   }
               };
               DB.GammaBase.Products.Add(product);
               DB.GammaBase.DocProducts.Add(new DocProducts()
                   {
                       DocID = ItemID,
                       ProductID = productID,
                       IsInConfirmed = docProduction.IsConfirmed,
                   });
               var withdrawalDocID = SQLGuidUtil.NewSequentialId();
               var doc = new Docs()
               {
                   DocID = withdrawalDocID,
                   Date = docProduction.Date,
                   IsConfirmed = docProduction.IsConfirmed,
                   DocTypeID = (byte)DocTypes.DocWithdrawal,
                   PlaceID = WorkSession.PlaceID,
                   ShiftID = WorkSession.ShiftID,
                   UserID = WorkSession.UserID,
                   DocWithdrawal = new DocWithdrawal()
                   {
                       DocID = withdrawalDocID,
                       OutPlaceID = WorkSession.PlaceID
                   }
               };
               doc.DocWithdrawal.DocProduction.Add(DB.GammaBase.DocProduction.Where(d => d.DocID == ItemID).Select(d => d).First());
                foreach (var spool in Spools)
                {
                    doc.DocProducts.Add(new DocProducts()
                        {
                            DocID = withdrawalDocID,
                            IsInConfirmed = doc.IsConfirmed,
                            ProductID = spool.ProductID
                        });
                }
               DB.GammaBase.Docs.Add(doc);
            }
            else
            {
                var productGroupPack = DB.GammaBase.ProductGroupPacks.
                    Where(p => DB.GammaBase.DocProducts.Where(d => d.DocID == ItemID).Select(d => d.ProductID).Contains(p.ProductID)).First();
                productGroupPack.C1CNomenclatureID = NomenclatureID;
                productGroupPack.C1CCharacteristicID = CharacteristicID;
                productGroupPack.Weight = Weight;
                productGroupPack.GrossWeight = GrossWeight;
                var docWithdrawal = DB.GammaBase.Docs.
                    Where(d => d.DocWithdrawal.DocProduction.Select(dp => dp.DocID).Contains(ItemID)).First();
                docWithdrawal.DocProducts.Clear();             
                foreach (var spool in Spools)
                {
                    docWithdrawal.DocProducts.Add(new DocProducts()
                        {
                            DocID = docWithdrawal.DocID,
                            IsOutConfirmed = docWithdrawal.IsConfirmed,
                            ProductID = spool.ProductID
                        });
                }
            }
            DB.GammaBase.SaveChanges();
        }
        public PaperBase SelectedSpool { get; set; }

        public ObservableCollection<BarViewModel> Bars
        {
            get
            {
                return null;
            }
            set
            {
                
            }
        }
        public RelayCommand OpenSpoolCommand { get; private set; }
        private void OpenSpool()
        {
            if (SelectedSpool == null) return;
            MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedSpool.ProductID);
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
