using System;
using System.Collections.ObjectModel;
using Gamma.Interfaces;
using Gamma.Models;
using System.Linq;
using System.Data.Entity;
using Gamma.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm;


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

        public DocProductSpoolViewModel(Guid id, bool isNewProduct) // если это новое изделие, то id - Это docId иначе productId
        {
            Guid docId;
            Guid productId;
            if (isNewProduct)
            {
                docId = id;
                productId = SQLGuidUtil.NewSequentialId();
            }
            else
            {
                productId = id;
                docId =
                    DB.GammaBase.DocProducts.Where(
                        dp => dp.ProductID == id && dp.Docs.DocTypeID == (byte)DocTypes.DocProduction)
                        .Select(dp => dp.DocID)
                        .First();
            }
            States = new ProductStates().ToDictionary();
            RejectionReasons = new ObservableCollection<RejectionReason>
                (from r in DB.GammaBase.GetSpoolRejectionReasons()
                 select new RejectionReason
                 {
                     RejectionReasonID = (Guid)r.RejectionReasonID,
                     Description = r.Description,
                     FullDescription = r.FullDescription
                 });
            if (isNewProduct)
            {
                Product = new Products()
                {
                    ProductID = productId,
                    ProductKindID = (byte)ProductKinds.ProductSpool
                };
                DB.GammaBase.Products.Add(Product);
                var ptInfo = (from d in DB.GammaBase.DocProduction
                    where d.DocID == docId
                    select new 
                    { 
                        NomenclatureID = d.ProductionTasks.C1CNomenclatureID, 
                        CharacteristicID = d.ProductionTasks.C1CCharacteristicID 
                    }).FirstOrDefault();
                ProductSpool = new ProductSpools()
                {
                    ProductID = Product.ProductID,
                    C1CNomenclatureID = ptInfo.NomenclatureID,
                    C1CCharacteristicID = ptInfo.CharacteristicID,
                    RealFormat = DB.GetLastFormat(WorkSession.PlaceID)
                };
                DB.GammaBase.ProductSpools.Add(ProductSpool);
                DocProduct = new DocProducts()
                {
                    DocID = docId,
                    ProductID = Product.ProductID
                };
                DB.GammaBase.DocProducts.Add(DocProduct);
                DB.GammaBase.SaveChanges();
                DB.GammaBase.Entry<Products>(Product).Reload();
                RealFormat = DB.GetLastFormat(WorkSession.PlaceID);
            }
            else
            {
                DocProduct = (from dp in DB.GammaBase.DocProducts.Include(a => a.Docs) where dp.DocID == docId
                                  select dp).FirstOrDefault();
                Product = DB.GammaBase.Products.Include(p => p.ProductSpools).First(p => p.ProductID == productId);
//                    DB.GammaBase.Products.Where(prod => prod.DocProducts.FirstOrDefault().DocID == docID).FirstOrDefault();
//                DB.GammaBase.Entry<Products>(Product).Reload();  По наблюдениям Include заставляет EF выполнить запрос к базе, даже если Entity уже загружена
                ProductSpool = Product.ProductSpools;
//                ProductSpool = DB.GammaBase.ProductSpools.FirstOrDefault(ps => ps.ProductID == Product.ProductID);
//                DB.GammaBase.Entry<ProductSpools>(ProductSpool).Reload();
                //RealFormat = ProductSpool.RealFormat ?? 0;
                RealBasisWeight = ProductSpool.RealBasisWeight;
                Diameter = ProductSpool.Diameter;
                Length = ProductSpool.Length ?? 0;
                BreakNumber = ProductSpool.BreakNumber;
                Weight = ProductSpool.Weight;
                var stateInfo = (from d in DB.GammaBase.DocChangeStateProducts
                    where d.ProductID == Product.ProductID
                    orderby
                    d.Docs.Date descending
                    select new 
                    { 
                        StateID = d.StateID,
                        RejectionReasonID = d.C1CRejectionReasonID
                    }).Take(1).FirstOrDefault();
                if (stateInfo != null)
                {
                    StateID = stateInfo.StateID;
                    RejectionReasonID = stateInfo.RejectionReasonID;
                }
                else
                {
                    StateID = 0;
                }
            }
            NomenclatureID = ProductSpool.C1CNomenclatureID;
            CharacteristicID = ProductSpool.C1CCharacteristicID;
            RealFormat = ProductSpool.RealFormat ?? 0;
            Bars.Add(ReportManager.GetReportBar("Spool", VMID));
            IsConfirmed = DocProduct.Docs.IsConfirmed && IsValid;
        }
        private Guid? _characteristicId;
        [UIAuth(UIAuthLevel.ReadOnly)]
        [Range(1,5000,ErrorMessage = "Необходимо указать диаметр")]
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
        public decimal? Length
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
        [Required(ErrorMessage="Необходимо выбрать характеристику")]
        public Guid? CharacteristicID
        {
            get { return _characteristicId; }
            set
            {
                _characteristicId = value;
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
        [Range(100,5000,ErrorMessage="Необходимо указать реальный формат")]
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
        [Range(1,10000,ErrorMessage="Укажите вес тамбура")]
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
        private decimal? _length;
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
        private Guid? _vmID = Guid.NewGuid();
        public Guid? VMID
        {
            get
            {
                return _vmID;
            }
        }

        private DocProducts DocProduct { get; set; }
        private Products Product { get; set; }
        private ProductSpools ProductSpool { get; set; }
        public override void SaveToModel(Guid itemId)
        {
            if (!DB.HaveWriteAccess("ProductSpools")) return;
            base.SaveToModel();
  /*          if (Product == null)
            {
                Product = new Products() { ProductID = SQLGuidUtil.NewSequentialId(), ProductKindID = (int)ProductKinds.ProductSpool };
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
                if (StateID != (byte)ProductStates.Good)
                    DB.GammaBase.DocProductChangeState.Add(new DocProductChangeState() { DocID = DocID, StateID = (byte)StateID });
            }
            else
 */
            var stateID = (from d in DB.GammaBase.DocChangeStateProducts where d.ProductID == Product.ProductID
                               orderby
                               d.Docs.Date descending
                                   select d.StateID).Take(1).FirstOrDefault();
            if (stateID != StateID)
            {
                 var docChangeID = SQLGuidUtil.NewSequentialId();
                 var docChangeStateProducts = new Collection<DocChangeStateProducts>();
                 docChangeStateProducts.Add(new DocChangeStateProducts() 
                 { 
                     DocID = docChangeID, 
                     ProductID = Product.ProductID,
                     StateID = (Byte)StateID,
                     C1CRejectionReasonID = RejectionReasonID,
                     Quantity = Weight
                 });
                    var doc = new Docs()
                    {
                        DocID = docChangeID,
                        Date = DB.CurrentDateTime,
                        DocTypeID = (byte)DocTypes.DocChangeState,
                        IsConfirmed = true,
                        UserID = WorkSession.UserID,
                        DocChangeStateProducts = docChangeStateProducts
                    };
                    DB.GammaBase.Docs.Add(doc);
            }
            ProductSpool.C1CNomenclatureID = (Guid)NomenclatureID;
            ProductSpool.C1CCharacteristicID = CharacteristicID;
            ProductSpool.RealBasisWeight = RealBasisWeight;
            ProductSpool.RealFormat = RealFormat;
            ProductSpool.BreakNumber = BreakNumber;
            ProductSpool.Diameter = Diameter;
            ProductSpool.Length = Length;
            ProductSpool.Weight = Weight;
            DocProduct.IsInConfirmed = (from doc in DB.GammaBase.Docs
                                        where doc.DocID == itemId
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
            return base.CanSaveExecute() && DB.HaveWriteAccess("ProductSpools");
        }
        protected override bool CanChooseNomenclature()
        {
            return !IsConfirmed && DB.HaveWriteAccess("ProductSpools");
        }
        private byte? _stateID;
        [UIAuth(UIAuthLevel.ReadOnly)]
        [Required(ErrorMessage="Необходимо выбрать качество")]
        public byte? StateID
        {
            get
            {
                return _stateID;
            }
            set
            {
            	_stateID = value;
                RaisePropertyChanged("StateID");
            }
        }
        public Dictionary<byte, string> States { get; set; }
        public ObservableCollection<RejectionReason> RejectionReasons { get; set; }
        [UIAuth(UIAuthLevel.ReadOnly)]
        public Guid? RejectionReasonID { get; set; }

    }
}