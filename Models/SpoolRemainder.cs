// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Attributes;
using Gamma.Entities;
using Gamma.Interfaces;
using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace Gamma.Models
{
    public class SpoolRemainder : ViewModelBase, ICheckedAccess
    {
        public SpoolRemainder(DateTime docDate, byte? shiftID, Guid? productId, bool isSourceProduct, Guid? docWithdrawalId)
        {
            GammaBase = DB.GammaDb;
            DocDate = docDate;
            DocShiftID = shiftID;
            IsSourceProduct = isSourceProduct;
            DocWithdrawalId = docWithdrawalId;
            ProductID = productId;
        }

        private DateTime DocDate { get; set; }
        private byte? DocShiftID { get; set; }
        private GammaEntities GammaBase { get; }
        public int Index { get; set; }
        private Guid? _productid;
        private decimal _weight;
        private decimal _length;
        
        public Guid? NomenclatureID { get; set; }
        public Guid? CharacteristicID { get; set; }
        public Guid? ProductID
        {
            get { return _productid; }
            set
            {
                _productid = value;
                if (value == null) return;
                using (var gammaBase = DB.GammaDb)
                {
                    var date = (DocWithdrawalId == null || DocWithdrawalId == Guid.Empty) 
                        ? DB.CurrentDateTime 
                        : gammaBase.DocWithdrawal.Where(
                            dw => dw.DocID == (Guid)DocWithdrawalId)
                            .Select(dw => dw.Docs.Date)
                            .FirstOrDefault();
                    MaxWeight = GetRemainderMaxWeight((Guid)value, date);
                    var productSpool =
                        gammaBase.ProductSpools.Include(ps => ps.Products.DocProductionProducts)
                            .FirstOrDefault(ps => ps.ProductID == value);
                    NomenclatureID = productSpool?.C1CNomenclatureID;
                    CharacteristicID = productSpool?.C1CCharacteristicID;
                    var quantity = productSpool?.Products.DocProductionProducts.First().Quantity * 1000 ?? 0;
                    if (quantity == 0) return;
                    MaxLength = (productSpool?.Length ?? 0) * MaxWeight / quantity;
                }
            }
        }

        public string Nomenclature { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal Weight
        {
            get { return _weight; }
            set
            {
                if (_weight == value) return;
                var delta = value - _weight;
                _weight = value;
                MessageManager.RecalcQuantityEndFromUnwinderReaminderEvent(ProductID, NomenclatureID, CharacteristicID, value, delta);
                if (MaxWeight == 0) return;
                Length = Math.Round(Weight * MaxLength / MaxWeight, 2);
                RaisePropertyChanged("Weight");
            }
        }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal Length
        {
            get { return _length; }
            set
            {
                if (_length == value) return;
                _length = value;
                if (MaxLength == 0) return;
                Weight = Math.Round(Length * MaxWeight / MaxLength);
                RaisePropertyChanged("Length");
            }
        }

        private decimal _maxWeight { get; set; }
        public decimal MaxWeight
        {
            get { return _maxWeight; }
            set
            {
                _maxWeight = value;
                Nomenclature = GetProductSpoolNomenclature((Guid)ProductID) + value + "кг";
            }
        }
        public decimal MaxLength { get; private set; }

        private string GetProductSpoolNomenclature(Guid productid)
        {
            return
                GammaBase.ProductSpools.Where(p => p.ProductID == productid)
                    .Select(p => "№ " + p.Products.Number + " " + p.C1CNomenclature.Name + " " +
                                 p.C1CCharacteristics.Name + " Масса: ").First();

        }

        private decimal GetRemainderMaxWeight(Guid productId, DateTime docDate)
        {
            var date = docDate.AddMilliseconds(-1);
            return DB.CalculateSpoolWeightBeforeDate(productId, (DateTime)date, GammaBase) * 1000;
        }

        public bool IsReadOnly { get; set; }
        public bool IsSourceProduct { get; set; }
        public Guid? DocWithdrawalId { get; set; }
    }
}
