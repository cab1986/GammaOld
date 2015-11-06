using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using Gamma.Interfaces;
using Gamma.Models;
using System.Linq;
using System.Data.Entity;
using GalaSoft.MvvmLight.Messaging;
using Gamma.Attributes;


namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocProductSpoolViewModel : DBEditItemWithNomenclatureViewModel, IBarImplemented, ICheckedAccess
    {
        /// <summary>
        /// Initializes a new instance of the DocProductSpoolViewModel class.
        /// </summary>
        public DocProductSpoolViewModel()
        {

        }

        public DocProductSpoolViewModel(Guid? ID, bool NewProduct)
        {
            if (NewProduct)
            {
                var ptInfo = (from pt in DB.GammaBase.ProductionTasks
                             where pt.ProductionTaskID == ID
                             select new { NomenclatureID = pt.C1CNomenclatureID, CharacteristicID = pt.C1CCharacteristicID }).FirstOrDefault();
                if (ptInfo != null)
                {
                    NomenclatureID = ptInfo.NomenclatureID;
                    CharacteristicID = ptInfo.CharacteristicID;
                }
            }
            else
            {
                DocProduct = (from dp in DB.GammaBase.DocProducts where dp.ProductID == ID 
                              join
                              dprod in DB.GammaBase.DocProduction on dp.DocID equals dprod.DocID
                                  select dp).FirstOrDefault();
                Product = DB.GammaBase.Products.Where(prod => prod.ProductID == DocProduct.ProductID).FirstOrDefault();
                ProductSpool = DB.GammaBase.ProductSpools.Where(ps => ps.ProductID == Product.ProductID).FirstOrDefault();
                NomenclatureID = ProductSpool.C1CNomenclatureID;
                CharacteristicID = ProductSpool.C1CCharacteristicID;
                RealFormat = ProductSpool.RealFormat;
                RealBasisWeight = ProductSpool.RealBasisWeight;
                Diameter = ProductSpool.Diameter;
                Length = ProductSpool.Length;
                BreakNumber = ProductSpool.BreakNumber;
                Weight = ProductSpool.Weight;
                IsConfirmed = DocProduct.IsInConfirmed ?? false;
            }
            Bars.Add(ReportManager.GetReportBar("Spool"));
        }
        private Guid? _characteristicID;
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
        [UIAuth(UIAuthLevel.ReadOnly)]
        public int? Length
        {
            get
            {
                return _length;
            }
            set
            {
                _length = value;
                RaisePropertyChanged("Length");
            }
        }
        public Guid? DocID
        {
            get
            {
                return _docID;
            }
            set
            {
                _docID = value;
            }
        }
        [UIAuth(UIAuthLevel.ReadOnly)]
        public Guid? CharacteristicID
        {
            get { return _characteristicID; }
            set
            {
                _characteristicID = value;
                RaisePropertyChanged("CharacteristicID");
            }
        }
        protected override void NomenclatureChanged(Nomenclature1CMessage msg)
        {
            base.NomenclatureChanged(msg);
            Characteristics = DB.GetCharacteristics(NomenclatureID);
        }
        private decimal? _realBasisWeight;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal? RealBasisWeight
        {
            get { return _realBasisWeight; }
            set
            {
                _realBasisWeight = value;
                RaisePropertyChanged("RealBasisWeight");
            }
        }
        private int? _realFormat;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public int? RealFormat
        {
            get
            {
                return _realFormat;
            }
            set
            {
                _realFormat = value;
                RaisePropertyChanged("RealFormat");
            }
        }
        private byte? _breakNumber;
        [UIAuth(UIAuthLevel.ReadOnly)]
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
        private bool IsConfirmed { get; set; }
        private int _weight;   
        [UIAuth(UIAuthLevel.ReadOnly)]
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
        private int? _length;
        private int _diameter;
        private Guid? _docID;
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

        private DocProducts DocProduct { get; set; }
        private Products Product { get; set; }
        private ProductSpools ProductSpool { get; set; }
        public override void SaveToModel(Guid DocID)
        {
            base.SaveToModel();
            if (Product == null)
            {
                Product = new Products() { ProductID = Guid.NewGuid(), ProductKindID = (int)ProductKinds.ProductSpool };
                ProductSpool = new ProductSpools() { ProductID = Product.ProductID };
                DocProduct = new DocProducts()
                {
                    DocID = DocID,
                    ProductID = Product.ProductID,
                    IsInConfirmed = (from doc in DB.GammaBase.Docs
                                     where doc.DocID == DocID
                                     select doc.IsConfirmed).FirstOrDefault()
                };
                DB.GammaBase.Products.Add(Product);
                DB.GammaBase.ProductSpools.Add(ProductSpool);
                DB.GammaBase.DocProducts.Add(DocProduct);
            }
            ProductSpool.C1CNomenclatureID = NomenclatureID;
            ProductSpool.C1CCharacteristicID = CharacteristicID;
            ProductSpool.RealBasisWeight = RealBasisWeight;
            ProductSpool.RealFormat = RealFormat;
            ProductSpool.BreakNumber = BreakNumber;
            ProductSpool.Diameter = Diameter;
            ProductSpool.Length = Length;
            ProductSpool.Weight = Weight;
            DocProduct.IsInConfirmed = (from doc in DB.GammaBase.Docs
                                        where doc.DocID == DocID
                                        select doc.IsConfirmed).FirstOrDefault();
            DB.GammaBase.SaveChanges();
            Messenger.Default.Send<ProductChangedMessage>(new ProductChangedMessage() {ProductID = Product.ProductID});
        }

        public bool IsReadOnly
        {
            get 
            {
                return IsConfirmed || !DB.HaveWriteAccess("ProductSpools");
            }
        }
        public override bool CanSaveExecute()
        {
            return DB.HaveWriteAccess("ProductSpools");
        }
        protected override bool CanChooseNomenclature()
        {
            return !IsConfirmed && DB.HaveWriteAccess("ProductSpools");
        }
    }
}