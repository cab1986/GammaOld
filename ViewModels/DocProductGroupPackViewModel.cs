using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Interfaces;
using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using System.Windows;
using Gamma.Attributes;
using System.ComponentModel.DataAnnotations;
using Gamma.Models;
using Gamma.Common;
using System.Data.Entity;

namespace Gamma.ViewModels
{
    class DocProductGroupPackViewModel : SaveImplementedViewModel, ICheckedAccess, IBarImplemented
    {
        public DocProductGroupPackViewModel(GammaEntities gammaBase = null) : base(gammaBase)
        {
            IsNewGroupPack = true;
            GetGrossWeightCommand = new DelegateCommand(GetGrossWeight, () => Scales.IsReady && !IsConfirmed);
            GetWeightCommand = new DelegateCommand(GetWeight, () => Scales.IsReady && !IsConfirmed);
            AddSpoolCommand = new DelegateCommand(AddSpool, () => !IsReadOnly);
            DeleteSpoolCommand = new DelegateCommand(DeleteSpool, () => SelectedSpool != null && !IsReadOnly);
            OpenSpoolCommand = new DelegateCommand(OpenSpool, () => SelectedSpool != null);
            UnpackCommand = new DelegateCommand(Unpack, () => !IsNewGroupPack && ProductId != null && WorkSession.PlaceGroup == PlaceGroups.Wr);
            Spools = new AsyncObservableCollection<PaperBase>();
            Bars.Add(ReportManager.GetReportBar("GroupPacks", VMID));
        }

        private void Unpack()
        {
            if (ProductId == null) return;
            if (!GammaBase.Rests.Any(r => r.ProductID == @ProductId))
            {
                MessageBox.Show("Данная упаковка не числится на остатках");
                return;
            }
            if (
                MessageBox.Show("Вы уверены, что хотите распаковать данную упаковку?", "Распаковка",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                GammaBase.UnpackGroupPack(ProductId, WorkSession.PrintName);
            }
        }

        public DocProductGroupPackViewModel(Guid docID) : this()
        {
            var doc = GammaBase.Docs.Include(d => d.DocProduction.DocProductionProducts).First(d => d.DocID == docID);
            IsConfirmed = doc.IsConfirmed;
            IsNewGroupPack = false;
            if (doc.DocProduction.DocProductionProducts.Count > 0)
            {
                ProductId = doc.DocProduction.DocProductionProducts.Select(d => d.ProductID).First();
                var productGroupPack = GammaBase.ProductGroupPacks.FirstOrDefault(p => p.ProductID == ProductId);
                Weight = Convert.ToInt32(productGroupPack?.Weight ?? 0);
                GrossWeight = Convert.ToInt32(productGroupPack?.GrossWeight ?? 0);
            }
            var groupPackSpools = GammaBase.GetGroupPackSpools(docID).ToList();
            if (groupPackSpools.Count > 0)
            {
                BaseCoreWeight = DB.GetSpoolCoreWeight(groupPackSpools[0].ProductID);
                PlaceProductionid = groupPackSpools[0].PlaceID;
                foreach (var groupPackSpool in groupPackSpools)
                {
                    Spools.Add(new PaperBase()
                    {
                        ProductID = groupPackSpool.ProductID,
                        DocID = groupPackSpool.DocID,
                        NomenclatureID = (Guid)groupPackSpool.C1CNomenclatureID,
                        CharacteristicID = (Guid)groupPackSpool.C1CCharacteristicID,
                        Number = $"{groupPackSpool.Number} от {groupPackSpool.Date}",
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
/*
        private int Format { get; set; }
*/
        private bool IsNewGroupPack { get; set; }
        private void GetWeight()
        {
            Weight = Convert.ToInt32(Scales.GetWeight());            
        }

        private Guid? ProductId { get; set; }

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
        private Guid? _vmid = Guid.NewGuid();
        public Guid? VMID
        {
            get
            {
                return _vmid;
            }
        }

        private int _weight;
        private int _grossWeight;
        [UIAuth(UIAuthLevel.ReadOnly)]
        [Range(1,1000000,ErrorMessage=@"Значение должно быть больше 0")]
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
        [Range(1, 1000000, ErrorMessage = @"Значение должно быть больше 0")]
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
        public DelegateCommand GetWeightCommand { get; private set; }
        public DelegateCommand GetGrossWeightCommand { get; private set; }
        public DelegateCommand AddSpoolCommand { get; private set; }
        public DelegateCommand DeleteSpoolCommand { get; private set; }
        public DelegateCommand UnpackCommand { get; private set; }

        

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
            var spool = (from p in GammaBase.vProductsInfo where p.ProductID == msg.ProductID
                             select p).ToList().Select( p => new PaperBase()
                             {
                                 PlaceProductionid = p.PlaceID,
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

        /// <summary>
        /// Добавление рулона по штрих-коду
        /// </summary>
        /// <param name="barcode">штрих-код</param>
        public void AddSpool(string barcode)
        {
            var p = (from pinf in GammaBase.vProductsInfo
                         where pinf.BarCode == barcode
                         select pinf).FirstOrDefault();
            if (p == null) return;
            if (p.IsWrittenOff??false)
            {
                MessageBox.Show("Вы пытаететесь добавить списанный рулон");
                return;
            }
            var spool = new PaperBase()
                             {
                                 PlaceProductionid = p.PlaceID,
                                 DocID = p.DocID,
                                 CharacteristicID = (Guid)p.C1CCharacteristicID,
                                 NomenclatureID = (Guid)p.C1CNomenclatureID,
                                 Nomenclature = p.NomenclatureName,
                                 ProductID = p.ProductID,
                                 Weight = p.Quantity ?? 0,
                                 Number = $"{p.Number} от {p.Date}"
                             };
            AddSpoolIfCorrect(spool);
        }
        private decimal BaseCoreWeight { get; set; }
        public void AddSpoolIfCorrect(PaperBase spool)
        {
            if (Spools.Count == 0)
            {
                PlaceProductionid = spool.PlaceProductionid;
                NomenclatureID = spool.NomenclatureID;
                CharacteristicID = spool.CharacteristicID;
                BaseCoreWeight = DB.GetSpoolCoreWeight(spool.ProductID);
                Diameter = DB.GetSpoolDiameter(spool.ProductID);              
                Spools.Add(spool);
                CoreWeight = BaseCoreWeight * Spools.Count;
            }
            else
            {
                if (spool.PlaceProductionid != PlaceProductionid)
                {
                    MessageBox.Show("Нельзя упаковывать рулоны с разных переделов",
                        "Рулон другого передела", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
                else if (spool.NomenclatureID != NomenclatureID || spool.CharacteristicID != CharacteristicID)
                {
                    MessageBox.Show("Номенклатура рулона не совпадает с номенклатурой ГУ");
                }
                else if (Spools.Select(s => s.ProductID).Contains(spool.ProductID))
                {
                    MessageBox.Show("Данный рулон уже занесен");
                }
                else
                {
                    Spools.Add(spool);
                    CoreWeight = BaseCoreWeight * Spools.Count;
                }
            }
        }
        private int? PlaceProductionid { get; set; }
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

        /// <summary>
        /// Сохранение в базу
        /// </summary>
        /// <param name="itemID">ID документа</param>
        /// <param name="gammaBase">Контекст БД</param>
        public override void SaveToModel(Guid itemID, GammaEntities gammaBase = null)
        {
            if (!DB.HaveWriteAccess("ProductGroupPacks") || !IsValid) return;
            //gammaBase = gammaBase ?? DB.GammaDb;
            var doc = GammaBase.Docs.Include(d => d.DocProduction).Include(d => d.DocProduction.DocWithdrawal)
                .Where(d => d.DocID == itemID).Select(d => d).First();
            if (doc.DocProduction == null)
                doc.DocProduction = new DocProduction()
                {
                    DocID = doc.DocID,
                    InPlaceID = doc.PlaceID,
                    DocProductionProducts = new List<DocProductionProducts>()
                };
            var product = GammaBase.Products.Include(p => p.ProductGroupPacks).FirstOrDefault(p => p.DocProductionProducts.FirstOrDefault().DocID == itemID);
            if (product == null)
            {
                var productId = SqlGuidUtil.NewSequentialid();
                product = new Products()
                {
                    ProductID = productId,
                    ProductKindID = (byte)ProductKinds.ProductGroupPack,
                    ProductGroupPacks = new ProductGroupPacks()
                };
                doc.DocProduction.DocProductionProducts.Add(new DocProductionProducts()
                {
                    Products = product,
                    DocID = doc.DocID
                });
//                GammaBase.Products.Add(product);
            }
            if (product.ProductGroupPacks == null) product.ProductGroupPacks = new ProductGroupPacks() { ProductID = product.ProductID };
            if (product.DocProductionProducts == null)
            {
                product.DocProductionProducts = new List<DocProductionProducts>
                {
                    new DocProductionProducts
                    {
                        ProductID = product.ProductID,
                        DocID = itemID,
                        Quantity = Weight,
                        C1CNomenclatureID = NomenclatureID,
                        C1CCharacteristicID = CharacteristicID
                    }
                };
            }
            product.ProductGroupPacks.C1CNomenclatureID = NomenclatureID;
            product.ProductGroupPacks.C1CCharacteristicID = CharacteristicID;
            product.ProductGroupPacks.Weight = Weight;
            product.ProductGroupPacks.GrossWeight = GrossWeight;
            product.ProductGroupPacks.Diameter = Diameter;
            var docWithdrawal = new Docs();
            if (doc.DocProduction.DocWithdrawal.Count > 0)
            {
                var docWithdrawalId = doc.DocProduction.DocWithdrawal.FirstOrDefault().DocID;
                docWithdrawal = GammaBase.Docs.Include(d => d.DocWithdrawal)
                    .Include(d => d.DocWithdrawal.DocWithdrawalProducts).First(d => d.DocID == docWithdrawalId);
            }
            else 
            {
                docWithdrawal.DocID = SqlGuidUtil.NewSequentialid();
                docWithdrawal.PlaceID = WorkSession.PlaceID;
                docWithdrawal.Date = DB.CurrentDateTime;
                docWithdrawal.UserID = WorkSession.UserID;
                docWithdrawal.ShiftID = WorkSession.ShiftID;
                docWithdrawal.PrintName = WorkSession.PrintName;
                docWithdrawal.DocTypeID = (byte)DocTypes.DocWithdrawal;
                docWithdrawal.IsConfirmed = doc.IsConfirmed;
                var docProductions = new ObservableCollection<DocProduction> {doc.DocProduction};
                docWithdrawal.DocWithdrawal = new DocWithdrawal()
                {
                    DocID = docWithdrawal.DocID,
                    OutPlaceID = docWithdrawal.PlaceID,
                    DocProduction = docProductions
                };
                docWithdrawal.DocWithdrawal.DocWithdrawalProducts = new ObservableCollection<DocWithdrawalProducts>();
                GammaBase.Docs.Add(docWithdrawal);
            }
            docWithdrawal.DocWithdrawal.DocWithdrawalProducts.Clear();
            foreach (var spool in Spools)
                {
                    docWithdrawal.DocWithdrawal.DocWithdrawalProducts.Add(new DocWithdrawalProducts()
                    {
                        DocID = docWithdrawal.DocID,
                        ProductID = spool.ProductID
                    });
                }
            GammaBase.SaveChanges();
        }
        // ReSharper disable once MemberCanBePrivate.Global
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
        public DelegateCommand OpenSpoolCommand { get; private set; }
        private void OpenSpool()
        {
            if (SelectedSpool == null) return;
            MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedSpool.ProductID);  
        }
        public override bool IsValid => base.IsValid && Spools.Count > 0;
    }
}
