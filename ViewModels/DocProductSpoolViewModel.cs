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
    public class DocProductSpoolViewModel : DbEditItemWithNomenclatureViewModel, IBarImplemented, ICheckedAccess
    {
        /// <summary>
        /// Инициализация информации о тамбуре
        /// </summary>
        /// <param name="id">Для нового тамбура DocID, иначе ProductID</param>
        /// <param name="isNewProduct">Новый тамбур</param>
        /// <param name="gammaBase">Контекст БД</param>
        public DocProductSpoolViewModel(Guid id, bool isNewProduct, GammaEntities gammaBase = null): base(gammaBase) // если это новое изделие, то id - Это docID иначе productId
        {
            Guid docID;
            Guid productId;
            if (isNewProduct)
            {
                docID = id;
                productId = SqlGuidUtil.NewSequentialid();
            }
            else
            {
                productId = id;
                docID =
                    GammaBase.DocProducts.Where(
                        dp => dp.ProductID == id && dp.Docs.DocTypeID == (byte)DocTypes.DocProduction)
                        .Select(dp => dp.DocID)
                        .First();
            }
            var doc = GammaBase.Docs.First(d => d.DocID == docID);
            States = new ProductStates().ToDictionary();
            ToughnessKinds = new ToughnessKinds().ToDictionary();
            RejectionReasons = new ObservableCollection<RejectionReason>
                (from r in GammaBase.GetSpoolRejectionReasons()
                 select new RejectionReason
                 {
                     RejectionReasonID = (Guid)r.RejectionReasonID,
                     Description = r.Description,
                     FullDescription = r.FullDescription
                 });
            if (isNewProduct)
            {
                Product =
                    GammaBase.Products.Include(p => p.ProductSpools)
                        .FirstOrDefault(p => p.DocProducts.Select(d => d.DocID).Contains(id));
                if (Product == null)
                {
                    Product = new Products()
                    {
                        ProductID = productId,
                        ProductKindID = (byte) ProductKinds.ProductSpool,
                    };
                    GammaBase.Products.Add(Product);
                }
                if (Product.ProductSpools == null)
                {
                    Product.ProductSpools = new ProductSpools
                    {
                        ProductID = Product.ProductID
                    };
                }
                var ptInfo = (from d in GammaBase.DocProduction
                    where d.DocID == docID
                    select new 
                    { 
                        NomenclatureID = d.ProductionTasks.C1CNomenclatureID, 
                        CharacteristicID = d.ProductionTasks.C1CCharacteristicID 
                    }).FirstOrDefault();
                Product.ProductSpools.C1CNomenclatureID = (Guid)ptInfo.NomenclatureID;
                Product.ProductSpools.C1CCharacteristicID = ptInfo.CharacteristicID;
                Product.ProductSpools.RealFormat = DB.GetLastFormat(WorkSession.PlaceID);
                ProductSpool = Product.ProductSpools;
                GammaBase.SaveChanges();
                GammaBase.Entry(Product).Reload();
            }
            else
            {
                DocProduct = (from dp in GammaBase.DocProducts.Include(a => a.Docs) where dp.DocID == docID
                                  select dp).FirstOrDefault();
                Product = GammaBase.Products.Include(p => p.ProductSpools).First(p => p.ProductID == productId);
                ProductSpool = Product.ProductSpools ?? new ProductSpools
                {
                    ProductID = Product.ProductID
                };
                RealBasisWeight = ProductSpool.RealBasisWeight;
                Diameter = ProductSpool.Diameter;
                Length = ProductSpool.Length ?? 0;
                BreakNumber = ProductSpool.BreakNumber;
                Weight = ProductSpool.DecimalWeight??0;
                ToughnessKindID = ProductSpool.ToughnessKindID ?? 1;
                var stateInfo = (from d in GammaBase.DocChangeStateProducts
                    where d.ProductID == Product.ProductID
                    orderby
                    d.Docs.Date descending
                    select new 
                    {
                        d.StateID,
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
            Bars.Add(ReportManager.GetReportBar("SpoolLabels", VMID));
            IsConfirmed = doc.IsConfirmed && IsValid;
        }
        [UIAuth(UIAuthLevel.ReadOnly)]
        [Range(1,5000,ErrorMessage = @"Необходимо указать диаметр")]
        // ReSharper disable once MemberCanBePrivate.Global
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
        public Guid? DocID { get; set; }

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
        private decimal _weight;   
        [Range(1,10000,ErrorMessage="Укажите вес тамбура")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal Weight
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
        private Guid? _vmid = Guid.NewGuid();
        public Guid? VMID
        {
            get
            {
                return _vmid;
            }
        }

        private DocProducts DocProduct { get; set; }
        private Products Product { get; set; }
        public ProductSpools ProductSpool { get; set; }
        public override void SaveToModel(Guid itemID, GammaEntities gammaBase = null)
        {
            if (!DB.HaveWriteAccess("ProductSpools")) return;
            gammaBase = gammaBase ?? DB.GammaDb;
            base.SaveToModel();
  /*          if (Product == null)
            {
                Product = new Products() { ProductID = SQLGuidUtil.NewSequentialid(), ProductKindID = (int)ProductKinds.ProductSpool };
                ProductSpool = new ProductSpools() { ProductID = Product.ProductID };
                DocProduct = new DocProducts()
                {
                    DocID = DocID,
                    ProductID = Product.ProductID,
                    IsInConfirmed = (from doc in GammaBase.Docs
                                     where doc.DocID == DocID
                                     select doc.IsConfirmed).FirstOrDefault()
                };
                GammaBase.Products.Add(Product);
                GammaBase.ProductSpools.Add(ProductSpool);
                GammaBase.DocProducts.Add(DocProduct);
                if (StateID != (byte)ProductStates.Good)
                    GammaBase.DocProductChangeState.Add(new DocProductChangeState() { DocID = DocID, StateID = (byte)StateID });
            }
            else
 */
            var stateid = (from d in GammaBase.DocChangeStateProducts where d.ProductID == Product.ProductID
                               orderby
                               d.Docs.Date descending
                                   select d.StateID).Take(1).FirstOrDefault();
            if (stateid != StateID)
            {
                 var docChangeid = SqlGuidUtil.NewSequentialid();
                 var docChangeStateProducts = new Collection<DocChangeStateProducts>();
                 docChangeStateProducts.Add(new DocChangeStateProducts() 
                 { 
                     DocID = docChangeid, 
                     ProductID = Product.ProductID,
                     StateID = (byte)StateID,
                     C1CRejectionReasonID = RejectionReasonID,
                     Quantity = Weight
                 });
                    var doc = new Docs()
                    {
                        DocID = docChangeid,
                        Date = DB.CurrentDateTime,
                        DocTypeID = (byte)DocTypes.DocChangeState,
                        IsConfirmed = true,
                        UserID = WorkSession.UserID,
                        DocChangeStateProducts = docChangeStateProducts
                    };
                    GammaBase.Docs.Add(doc);
            }
            ProductSpool.C1CNomenclatureID = (Guid)NomenclatureID;
            ProductSpool.C1CCharacteristicID = CharacteristicID;
            ProductSpool.RealBasisWeight = RealBasisWeight;
            ProductSpool.RealFormat = RealFormat;
            ProductSpool.BreakNumber = BreakNumber;
            ProductSpool.Diameter = Diameter;
            ProductSpool.Length = Length;
            ProductSpool.DecimalWeight = Weight;
            ProductSpool.ToughnessKindID = ToughnessKindID;
            DocProduct.IsInConfirmed = (from doc in GammaBase.Docs
                                        where doc.DocID == itemID
                                        select doc.IsConfirmed).FirstOrDefault();
            GammaBase.SaveChanges();
            Messenger.Default.Send(new ProductChangedMessage {ProductID = Product.ProductID});
        }
        public bool IsReadOnly => IsConfirmed || !DB.HaveWriteAccess("ProductSpools");

        public override bool CanSaveExecute()
        {
            return base.CanSaveExecute() && DB.HaveWriteAccess("ProductSpools");
        }
        protected override bool CanChooseNomenclature()
        {
            return !IsConfirmed && DB.HaveWriteAccess("ProductSpools");
        }
        private byte? _stateid;
        [UIAuth(UIAuthLevel.ReadOnly)]
        [Required(ErrorMessage=@"Необходимо выбрать качество")]
        public byte? StateID
        {
            get
            {
                return _stateid;
            }
            set
            {
            	_stateid = value;
                RaisePropertyChanged("StateID");
            }
        }

        private byte _toughnessKindID = 1;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public byte ToughnessKindID
        {
            get { return _toughnessKindID; }
            set
            {
                _toughnessKindID = value;
                RaisePropertiesChanged("ToughnessKindID");
            }
        }
        public Dictionary<byte, string> ToughnessKinds { get; set; }
        public Dictionary<byte, string> States { get; set; }
        public ObservableCollection<RejectionReason> RejectionReasons { get; set; }
        [UIAuth(UIAuthLevel.ReadOnly)]
        public Guid? RejectionReasonID { get; set; }

    }
}