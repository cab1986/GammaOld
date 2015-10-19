using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using Gamma.Interfaces;
using Gamma.Models;
using System.Linq;
using System.Data.Entity;
using GalaSoft.MvvmLight.Messaging;


namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocProductSpoolViewModel : DBEditItemWithNomenclatureViewModel, IBarImplemented
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
                var ProductionTask = (from p in DB.GammaBase.ProductionTaskConfig where p.ProductionTaskID == ID
                                         select
                                         new {NomenclatureID = p.C1CNomenclatureID,CharacteristicID = p.C1CCharacteristicID}).FirstOrDefault();
                NomenclatureID = ProductionTask.NomenclatureID;
                SelectedCharacteristic = Characteristics.Where(c => c.CharacteristicID == ProductionTask.CharacteristicID).FirstOrDefault();
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
                SelectedCharacteristic = Characteristics.Where(c => c.CharacteristicID == ProductSpool.C1CCharacteristicID).FirstOrDefault();
                RealFormat = ProductSpool.RealFormat;
                RealBasisWeight = ProductSpool.RealBasisWeight;
                Diameter = ProductSpool.Diameter;
                Length = ProductSpool.Length;
                BreakNumber = ProductSpool.BreakNumber;
                Weight = ProductSpool.Weight;
            }
        }
        private Characteristic _selectedCharacteristic;
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
        public Characteristic SelectedCharacteristic
        {
            get { return _selectedCharacteristic; }
            set
            {
                _selectedCharacteristic = value;
                RaisePropertyChanged("SelectedCharacteristic");
            }
        }
        protected override void NomenclatureChanged(Nomenclature1CMessage msg)
        {
            base.NomenclatureChanged(msg);
            Characteristics = DB.GetCharacteristics(NomenclatureID);
        }
        private decimal? _realBasisWeight;
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
        private int _weight;
        public string State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                RaisePropertyChanged("State");
            }
        }
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
        private string _state;
        private Guid? _docID;
        public RelayCommand ChangeStateCommand { get; private set; }
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

        private DocProducts DocProduct { get; set; }
        private Products Product { get; set; }
        private ProductSpools ProductSpool { get; set; }
        public override void SaveToModel(Guid DocID)
        {
            base.SaveToModel();
            if (Product == null)
            {
                Product = new Products();
                Product.ProductID = Guid.NewGuid();
                Product.ProductKindID = (int)ProductKinds.ProductSpool;
                ProductSpool = new ProductSpools();
                ProductSpool.ProductID = Product.ProductID;
                DocProduct = new DocProducts();
                DocProduct.DocID = DocID;
                DocProduct.ProductID = Product.ProductID;
                DocProduct.IsInConfirmed = (from doc in DB.GammaBase.Docs where doc.DocID == DocID select doc.IsConfirmed).FirstOrDefault();
                DB.GammaBase.Products.Add(Product);
                DB.GammaBase.ProductSpools.Add(ProductSpool);
                DB.GammaBase.DocProducts.Add(DocProduct);
            }
            ProductSpool.C1CNomenclatureID = NomenclatureID;
            ProductSpool.C1CCharacteristicID = SelectedCharacteristic.CharacteristicID;
            ProductSpool.RealBasisWeight = RealBasisWeight;
            ProductSpool.RealFormat = RealFormat;
            ProductSpool.BreakNumber = BreakNumber;
            ProductSpool.Diameter = Diameter;
            ProductSpool.Length = Length;
            ProductSpool.Weight = Weight;
            
            DB.GammaBase.SaveChanges();
            Messenger.Default.Send<ProductChangedMessage>(new ProductChangedMessage() {ProductID = Product.ProductID});
        }
    }
}