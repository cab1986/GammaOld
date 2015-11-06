using System.Collections.ObjectModel;
using Gamma.Interfaces;
using System;
using GalaSoft.MvvmLight.Command;
using System.Linq;
using Gamma.Models;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocProductUnloadViewModel : DBEditItemWithNomenclatureViewModel, IBarImplemented
    {
        /// <summary>
        /// Initializes a new instance of the DocProductUnloadViewModel class.
        /// </summary>
        public DocProductUnloadViewModel(Guid ID, bool NewProduct)
        {
            CreateSpoolsCommand = new RelayCommand(CreateSpools);
            EditSpoolCommand = new RelayCommand(EditSpool);
            if (NewProduct)
            {
                
                UnloadSpools = new ObservableCollection<Spool>();
            }
            else
            {
                
                DocProducts = new ObservableCollection<DocProducts>
                (DB.GammaBase.DocProducts.Where(dp => dp.DocID == ID));
                Products = new ObservableCollection<Products>
                (from p in DB.GammaBase.Products where DocProducts.Where(dp => dp.ProductID == p.ProductID).Any() select p);
                UnloadSpools = new ObservableCollection<Spool>(from dp in DocProducts
                                                         join ps in DB.GammaBase.ProductSpools on dp.ProductID equals ps.ProductID
                                                         select new Spool
                                                         {
                                                             ProductID = ps.ProductID,
                                                             Number = dp.Products.Number,
                                                             Nomenclature = ps.C1CNomenclature.Name + " " + ps.C1CCharacteristics.Name,
                                                             CharacteristicID = (Guid)ps.C1CCharacteristicID,
                                                             NomenclatureID = ps.C1CNomenclatureID,
                                                             Weight = ps.Weight
                                                         }
                                    );
                ProductSpools = new ObservableCollection<ProductSpools>
                (from p in DB.GammaBase.ProductSpools where DocProducts.Where(dp => dp.ProductID == p.ProductID).Any() select p);
            }   
        }

        



        private ObservableCollection<BarViewModel> _bars;
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
        public RelayCommand CreateSpoolsCommand { get; private set; }
        
        private void CreateSpools()
        {
            if (UnloadSpools.Count > 0) 
            {
                UnloadSpools.Clear();
                DB.GammaBase.DocProducts.RemoveRange(DocProducts);
                DB.GammaBase.ProductSpools.RemoveRange(ProductSpools);
                DB.GammaBase.Products.RemoveRange(Products);
                DocProducts = null;
                ProductSpools = null;
                Products = null;
            }
            
            Messenger.Default.Send<ParentSaveMessage>(new ParentSaveMessage());
        }
        private ObservableCollection<Spool> _unloadSpools = new ObservableCollection<Spool>();
        public ObservableCollection<Spool> UnloadSpools
        {
            get
            {
                return _unloadSpools;
            }
            set
            {
                _unloadSpools = value;
                RaisePropertyChanged("Spools");
            }
        }
        public class Spool : ViewModelBase
        {
            public Guid ProductID { get; set; }
            public Guid CharacteristicID { get; set; }
            public Guid NomenclatureID { get; set; }
            public string Number { get; set; }
            public string Nomenclature { get; set; }
            private int _weight;
            public int Weight 
            {
                get { return _weight; }
                set
                {
                    _weight = value;
                    RaisePropertyChanged("Weight");
                }
            }
        }
        public override void SaveToModel(Guid ItemID)
        {
            base.SaveToModel(ItemID);
            if (DocProducts == null)
            {
                var docProduction = DB.GammaBase.DocProduction.Find(ItemID);
                var sourceSpools = DB.GammaBase.SourceSpools.Find(WorkSession.PlaceID);
                var doc = new Docs()
                {
                    DocID = Guid.NewGuid(),
                    UserID = WorkSession.UserID,
                    DocTypeID = (int)DocTypes.DocWithdrawal,
                    Date = DB.CurrentDateTime,
                    IsConfirmed = true
                };
                DB.GammaBase.Docs.Add(doc);
                var docWithdrawal = new DocWithdrawal()
                {
                    DocID = doc.DocID,
                    OutPlaceID = WorkSession.PlaceID
                };
                docProduction.DocWithdrawal.Add(docWithdrawal);
                Products = new ObservableCollection<Products>();
                ProductSpools = new ObservableCollection<ProductSpools>();
                DocProducts = new ObservableCollection<DocProducts>();
                foreach (var spool in UnloadSpools)
                {
                    Products.Add(new Products()
                    {
                        ProductID = spool.ProductID,
                        ProductKindID = (byte)ProductKinds.ProductSpool,
                    });
                    ProductSpools.Add(new ProductSpools()
                        {
                            ProductID = spool.ProductID,
                            Diameter = Diameter,
                            BreakNumber = BreakNumber,
                            Weight = spool.Weight,
                            C1CNomenclatureID = spool.NomenclatureID,
                            C1CCharacteristicID = spool.CharacteristicID
                        });
                    DocProducts.Add(new DocProducts()
                        {
                            DocID = ItemID,
                            ProductID = spool.ProductID,
                        });
                }
                DB.GammaBase.Products.AddRange(Products);
                DB.GammaBase.ProductSpools.AddRange(ProductSpools);
                DB.GammaBase.DocProducts.AddRange(DocProducts);
            }
            DB.GammaBase.SaveChanges();
        }
        private ObservableCollection<DocProducts> DocProducts { get; set; }
        private ObservableCollection<Products> Products { get; set; }
        private ObservableCollection<ProductSpools> ProductSpools { get; set; }
        private byte? _breakNumber;
        public byte? BreakNumber
        {
            get
            {
                return _breakNumber;
            }
            set
            {
                _breakNumber = value;
                RaisePropertyChanged("BreakNumber");
            }
        }
        private int _diameter;
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
        public RelayCommand EditSpoolCommand { get; set; }
        public Spool SelectedUnloadSpool 
        {
            get; set;
        }
        private void EditSpool()
        {
            Messenger.Default.Register<ProductChangedMessage>(this,ProductChanged);
            MessageManager.OpenDocProduct(new OpenDocProductMessage
            {
                DocProductKind = DocProductKinds.DocProductSpool,
                ID = SelectedUnloadSpool.ProductID,
                IsNewProduct = false
            });
        }
        private void ProductChanged(ProductChangedMessage msg)
        {
            Messenger.Default.Unregister(this);
            UnloadSpools.Where(u => u.ProductID == msg.ProductID).FirstOrDefault().Weight =
                (from ps in DB.GammaBase.ProductSpools where ps.ProductID == msg.ProductID select ps.Weight).FirstOrDefault();
        }
        
    }
}