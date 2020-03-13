// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Entities;
using Gamma.Interfaces;

namespace Gamma.Models
{
    public class DocCloseShiftRemainder : ViewModelBase,IProduct
    {
        public DocCloseShiftRemainder()
        {
            GammaBase = DB.GammaDb;
        }

        private GammaEntities GammaBase { get; }

        public string Number { get; set; }
        public string Nomenclature { get; set; }
        public decimal Quantity { get; set; }
        public ProductKind ProductKind { get; set; }
        public Guid? NomenclatureID { get; set; }
        public Guid? CharacteristicID { get; set; }
        public bool? IsSourceProduct { get; set; }
        private int? _remainderTypeID;
        public int? RemainderTypeID
        {
            get { return _remainderTypeID; }
            set
            {
                _remainderTypeID = value;
                GetRemainderTypeName(value);
            }
        }
        public string RemainderTypeName { get; set; }

        private int? _stateID;
        public int? StateID
        {
            get { return _stateID; }
            set
            {
                _stateID = value;
                GetProductBrokeStateName(value);
            }
        }
        public string StateName { get; set; }

        private Guid _productid;
        public Guid ProductID
        {
            get { return _productid; }
            set
            {
                _productid = value;
                GetProductSpoolNomenclature(value);
            }
        }

        private void GetProductSpoolNomenclature(Guid? productId)
        {
            var product = GammaBase.vProductsInfo.Where(p => p.ProductID == productId).FirstOrDefault();
            if (product == null)
            {
                Number = "";
                Nomenclature = "";
            }
            else
            {
                Number = product.Number;
                Nomenclature = product.NomenclatureName;
            };
        }

        private void GetProductBrokeStateName(int? stateId)
        {
            var state = stateId == null ? null : GammaBase.ProductStates.Where(p => p.StateID == stateId).First();
            StateName = state == null ? "" : state.Name ;
            
        }

        private void GetRemainderTypeName(int? remainderTypeId)
        {
            var remainderType = remainderTypeId == null ? null : GammaBase.RemainderTypes.Where(p => p.RemainderTypeID == remainderTypeId).First();
            RemainderTypeName = remainderType == null ? "" : remainderType.Name;

        }
    }
}
