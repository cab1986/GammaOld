using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class ImportOldProductsViewModel : DbEditItemWithNomenclatureViewModel
    {
        public ImportOldProductsViewModel(GammaEntities gammaBase = null) : base(gammaBase)
        {
            ProductKindsList = Functions.EnumDescriptionsToList(typeof(ProductKinds));
            SaveCommand = new DelegateCommand(Save, () => ProductId == null && !string.IsNullOrWhiteSpace(OldNomenclature));
            FindCommand = new DelegateCommand(Find, () => !string.IsNullOrWhiteSpace(Number));
        }

        private void Save()
        {
            SaveToModel();
        }

        private string _oldNomenclature;

        public string OldNomenclature
        {
            get { return _oldNomenclature; }
            set
            {
                _oldNomenclature = value;
                RaisePropertyChanged("OldNomenclature");
            }
        }

        private string _newProductNumber;

        public string NewProductNumber
        {
            get { return _newProductNumber; }
            set
            {
                _newProductNumber = value;
                RaisePropertyChanged("NewProductNumber");
            }
        }

        private Guid? _productId;

        public Guid? ProductId
        {
            get { return _productId; }
            set
            {
                _productId = value;
                RaisePropertyChanged("ProductId");
            }
        }

        public List<string> ProductKindsList { get; set; }

        private string SavedNumber { get; set; }

        public string Number { get; set; }

        public DelegateCommand FindCommand { get; private set; }

        public DelegateCommand SaveCommand { get; private set; }

        private byte _selectedProductKindIndex;
        public byte SelectedProductKindIndex
        {
            get
            {
                return _selectedProductKindIndex;
            }
            set
            {
                _selectedProductKindIndex = value;
                RaisePropertyChanged("SelectedProductKindIndex");
            }
        }

        private bool _isAssociated;

        public bool IsAssociated
        {
            get { return _isAssociated; }
            set
            {
                _isAssociated = value;
                RaisePropertyChanged("IsAssociated");
            }
        }

        private long? OldProductId { get; set; }

        private void Find()
        {
            var productInfo = GammaBase.GetOldProductInfo(Number, SelectedProductKindIndex).First();
            SavedNumber = Number;
            ProductId = productInfo.ProductId;
            OldNomenclature = productInfo.OldNomenclature;
            NomenclatureID = productInfo.NomenclatureId;
            CharacteristicID = productInfo.CharacteristicId;
            NewProductNumber = productInfo.ProductNumber;
            OldProductId = productInfo.OldProductId;
            if (string.IsNullOrWhiteSpace(OldNomenclature))
                MessageBox.Show("Продукт с таким номером не найден в старой базе");
            IsAssociated = ProductId != null;
        }
        
        public override void SaveToModel(GammaEntities gammaBase = null)
        {
            GammaBase.SaveOldProductToNewBase(OldProductId, SelectedProductKindIndex, NomenclatureID, CharacteristicID);
            Number = SavedNumber;
            Find();
        }
        
    }
}
