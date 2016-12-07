using System;
using Gamma.Models;
using System.Linq;
using System.Collections.ObjectModel;
using Gamma.Interfaces;
using Gamma.Attributes;
using System.Data.Entity;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public sealed class DocCloseShiftPMRemainderViewModel : DbEditItemWithNomenclatureViewModel, ICheckedAccess
    {
        /// <summary>
        /// Initializes a new instance of the DocCloseShiftPMRemainderViewModel class.
        /// </summary>
        public DocCloseShiftPMRemainderViewModel(GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb;
            var productSpool =
                GammaBase.Docs.Where(
                    d =>
                        d.PlaceID == WorkSession.PlaceID && d.ShiftID == WorkSession.ShiftID &&
                        d.DocTypeID == (int) DocTypes.DocProduction)
                    .OrderByDescending(d => d.Date)
                    .FirstOrDefault()?
                    .DocProduction.DocProductionProducts.FirstOrDefault()?
                    .Products.ProductSpools;
            if (productSpool == null) return;
            NomenclatureID = productSpool.C1CNomenclatureID;
            CharacteristicID = productSpool.C1CCharacteristicID;
        }

        public DocCloseShiftPMRemainderViewModel(Guid docID, GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb;
            DocCloseShiftRemainder = GammaBase.DocCloseShiftRemainders.Include("Docs").Where(d => d.DocID == docID).
                Select(d => d).FirstOrDefault();
            if (DocCloseShiftRemainder == null)
            {
                var doc = GammaBase.Docs.First(d => d.DocID == docID);
                IsConfirmed = doc.IsConfirmed;
                return;
            }
            IsConfirmed = DocCloseShiftRemainder.DocCloseShifts.IsConfirmed;
            var productSpool = GammaBase.ProductSpools.FirstOrDefault(p => p.ProductID == DocCloseShiftRemainder.ProductID);
            if (productSpool != null)
            {
                NomenclatureID = productSpool.C1CNomenclatureID;
                CharacteristicID = productSpool.C1CCharacteristicID;
            }
            Quantity = DocCloseShiftRemainder.Quantity;
        }
        private bool IsConfirmed { get; }
        private DocCloseShiftRemainders DocCloseShiftRemainder { get; set; }
        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal Quantity { get; set; }
        protected override bool CanChooseNomenclature()
        {
            return base.CanChooseNomenclature() && DB.HaveWriteAccess("ProductSpools") && !IsConfirmed;
        }
        public override bool SaveToModel(Guid itemID, GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            var doc = gammaBase.Docs.First(d => d.DocID == itemID);
            if (DocCloseShiftRemainder == null && Quantity > 0)
            {
                var productid = SqlGuidUtil.NewSequentialid();
                var product = new Products()
                {
                    ProductID = productid,
                    ProductKindID = (byte)ProductKind.ProductSpool,
                    ProductSpools = new ProductSpools()
                    {
                        C1CCharacteristicID = CharacteristicID,
                        C1CNomenclatureID = (Guid)NomenclatureID,
                        Diameter = 0,
                        DecimalWeight = 0,
                        ProductID = productid
                    }
                };
                gammaBase.Products.Add(product);
                var docID = SqlGuidUtil.NewSequentialid();
                var docProductionProducts = new ObservableCollection<DocProductionProducts>
                {
                    new DocProductionProducts()
                    {
                        DocID = docID,
                        ProductID = productid,
                        C1CNomenclatureID = NomenclatureID,
                        C1CCharacteristicID = CharacteristicID
                    }
                };
                var docProduction = new Docs()
                {
                    DocID = docID,
                    Date = DB.CurrentDateTime,
                    PlaceID = doc.PlaceID,
                    DocTypeID = (byte)DocTypes.DocProduction,
                    DocProduction = new DocProduction()
                    {
                        DocID = docID,
                        InPlaceID = doc.PlaceID,
                        DocProductionProducts = docProductionProducts
                    }
                };
                gammaBase.Docs.Add(docProduction);
                DocCloseShiftRemainder = new DocCloseShiftRemainders()
                {
                    DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                    DocID = itemID,
                    ProductID = productid,
                    Quantity = Quantity,
                    IsSourceProduct = false
                };
                gammaBase.DocCloseShiftRemainders.Add(DocCloseShiftRemainder);
            }
            else if (DocCloseShiftRemainder != null)
            {
                DocCloseShiftRemainder.Quantity = Quantity;
                if (
                    gammaBase.DocProductionProducts.Any(
                        d => d.ProductID == DocCloseShiftRemainder.ProductID && d.DocProduction.Docs.ShiftID == null))
                {
                    var docProductionProduct =
                        gammaBase.DocProductionProducts.Include(d => d.Products.ProductSpools)
                            .FirstOrDefault(d => d.ProductID == DocCloseShiftRemainder.ProductID);
                    if (docProductionProduct != null && NomenclatureID != null)
                    {
                        docProductionProduct.C1CNomenclatureID = NomenclatureID;
                        docProductionProduct.C1CCharacteristicID = CharacteristicID;
                        docProductionProduct.Products.ProductSpools.C1CNomenclatureID = (Guid)NomenclatureID;
                        docProductionProduct.Products.ProductSpools.C1CCharacteristicID = CharacteristicID;
                    }
                }
            } 
            gammaBase.SaveChanges();
            return true;
        }

        public bool IsReadOnly => !DB.HaveWriteAccess("DocCloseShiftRemainders") && !WorkSession.DBAdmin || IsConfirmed;
    }
}