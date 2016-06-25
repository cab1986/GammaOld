using System;
using System.Collections.ObjectModel;
using Gamma.Interfaces;
using Gamma.Models;
using System.Linq;
using System.Data.Entity;
using Gamma.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Windows;


namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public sealed class DocProductSpoolViewModel : DbEditItemWithNomenclatureViewModel, IBarImplemented, ICheckedAccess
    {
        /// <summary>
        /// Инициализация информации о тамбуре
        /// </summary>
        /// <param name="docId">ID документа</param>
        /// <param name="gammaBase">Контекст БД</param>
        public DocProductSpoolViewModel(Guid docId, GammaEntities gammaBase = null): base(gammaBase)
        {
            ProductId = GammaBase.DocProducts.FirstOrDefault(d => d.DocID == docId)?.ProductID ??
                            SqlGuidUtil.NewSequentialid();
            var doc = GammaBase.Docs.Include(d => d.DocProduction).First(d => d.DocID == docId);
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
            var productSpool = GammaBase.ProductSpools.FirstOrDefault(p => p.ProductID == ProductId);
            if (productSpool == null)
            {
                var productionTask =
                    GammaBase.ProductionTasks.FirstOrDefault(
                        pt => pt.ProductionTaskID == doc.DocProduction.ProductionTaskID);
                if (productionTask != null)
                {
                    NomenclatureID = productionTask.C1CNomenclatureID;
                    CharacteristicID = productionTask.C1CCharacteristicID;
                }
                else
                {
                    MessageBox.Show("Не удалось получить информацию о задании, выбирите номенклатуру вручную");
                }
                RealFormat = DB.GetLastFormat(WorkSession.PlaceID);
            }
            else
            {
                NomenclatureID = productSpool.C1CNomenclatureID;
                CharacteristicID = productSpool.C1CCharacteristicID;
                RealFormat = productSpool.RealFormat;
                RealBasisWeight = productSpool.RealBasisWeight;
                Diameter = productSpool.Diameter;
                Length = productSpool.Length ?? 0;
                BreakNumber = productSpool.BreakNumber;
                Weight = productSpool.DecimalWeight ?? 0;
                ToughnessKindID = productSpool.ToughnessKindID ?? 1;
                var stateInfo = (from d in GammaBase.DocChangeStateProducts
                                 where d.ProductID == ProductId
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
        [Range(100,5000,ErrorMessage=@"Необходимо указать реальный формат")]
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
        [Range(1,10000,ErrorMessage=@"Укажите вес тамбура")]
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

        public Guid? VMID { get; } = Guid.NewGuid();

        private DocProducts DocProduct { get; set; }
        /// <summary>
        /// ID продукта, используется для печати амбалажа
        /// </summary>
        public Guid ProductId { get; private set; }
        /// <summary>
        /// Сохранение в базу
        /// </summary>
        /// <param name="itemID">ID документа</param>
        /// <param name="gammaBase">Контекст БД</param>
        public override void SaveToModel(Guid itemID, GammaEntities gammaBase = null)
        {
            if (!DB.HaveWriteAccess("ProductSpools")) return;
            base.SaveToModel();
            var product =
                GammaBase.Products.Include(p => p.ProductSpools)
                    .FirstOrDefault(p => p.DocProducts.Select(dp => dp.DocID).Contains(itemID));
            if (product == null)
            {
                var id = SqlGuidUtil.NewSequentialid();
                product = new Products
                {
                    ProductID = id,
                    ProductKindID = (int)ProductKinds.ProductSpool,
                };
            }
            if (product.ProductSpools == null)
            {
                product.ProductSpools = new ProductSpools
                {
                    ProductID = product.ProductID
                };
            }

            var stateId = (from d in GammaBase.DocChangeStateProducts where d.ProductID == product.ProductID
                               orderby
                               d.Docs.Date descending
                                   select d.StateID).Take(1).FirstOrDefault();
            if (stateId != StateID)
            {
                var docChangeId = SqlGuidUtil.NewSequentialid();
                var doc = new Docs()
                    {
                        DocID = docChangeId,
                        Date = DB.CurrentDateTime,
                        DocTypeID = (byte)DocTypes.DocChangeState,
                        IsConfirmed = true,
                        UserID = WorkSession.UserID,
                        DocChangeStateProducts = new Collection<DocChangeStateProducts>
                        {
                            new DocChangeStateProducts()
                            {
                                DocID = docChangeId,
                                ProductID = product.ProductID,
                                StateID = StateID ?? 0,
                                C1CRejectionReasonID = RejectionReasonID,
                                Quantity = Weight
                            }
                        }
                    };
                    GammaBase.Docs.Add(doc);
            }
            product.ProductSpools.C1CNomenclatureID = (Guid)NomenclatureID;
            product.ProductSpools.C1CCharacteristicID = CharacteristicID;
            product.ProductSpools.RealBasisWeight = RealBasisWeight;
            product.ProductSpools.RealFormat = RealFormat;
            product.ProductSpools.BreakNumber = BreakNumber;
            product.ProductSpools.Diameter = Diameter;
            product.ProductSpools.Length = Length;
            product.ProductSpools.DecimalWeight = Weight;
            product.ProductSpools.ToughnessKindID = ToughnessKindID;
            GammaBase.SaveChanges();
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