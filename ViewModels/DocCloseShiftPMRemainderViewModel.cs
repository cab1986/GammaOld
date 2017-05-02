using System;
using Gamma.Models;
using System.Linq;
using System.Collections.ObjectModel;
using Gamma.Interfaces;
using Gamma.Attributes;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocCloseShiftPMRemainderViewModel : DbEditItemWithNomenclatureViewModel, ICheckedAccess
    {
        /// <summary>
        /// Initializes a new instance of the DocCloseShiftPMRemainderViewModel class.
        /// </summary>
        public DocCloseShiftPMRemainderViewModel(GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb;
            var productSpool = (from d in GammaBase.Docs
                                where d.PlaceID == WorkSession.PlaceID && d.ShiftID == WorkSession.ShiftID
                                join dp in GammaBase.DocProducts on d.DocID equals dp.DocID
                                join ps in GammaBase.ProductSpools on dp.ProductID equals ps.ProductID
                                orderby d.Date descending
                                select ps).FirstOrDefault();
            if (productSpool != null)
            {
                NomenclatureID = productSpool.C1CNomenclatureID;
                CharacteristicID = productSpool.C1CCharacteristicID;
            }
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
            IsConfirmed = DocCloseShiftRemainder.Docs.IsConfirmed;
            var productSpool = GammaBase.ProductSpools.FirstOrDefault(p => p.ProductID == DocCloseShiftRemainder.ProductID);
            if (productSpool != null)
            {
                NomenclatureID = productSpool.C1CNomenclatureID;
                CharacteristicID = productSpool.C1CCharacteristicID;
            }
            Quantity = DocCloseShiftRemainder.Quantity;
        }
        private bool IsConfirmed { get; set; }
        private DocCloseShiftRemainders DocCloseShiftRemainder { get; set; }
        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal Quantity { get; set; }
        protected override bool CanChooseNomenclature()
        {
            return base.CanChooseNomenclature() && DB.HaveWriteAccess("ProductSpools") && !IsConfirmed;
        }
        public override void SaveToModel(Guid itemID, GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            base.SaveToModel(itemID, gammaBase);
            var doc = gammaBase.Docs.First(d => d.DocID == itemID);
            if (DocCloseShiftRemainder == null && Quantity > 0)
            {
                var productid = SqlGuidUtil.NewSequentialId();
                var product = new Products()
                {
                    ProductID = productid,
                    ProductKindID = (byte)ProductKinds.ProductSpool,
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
                var docID = SqlGuidUtil.NewSequentialId();
                var docProducts = new ObservableCollection<DocProducts>
                {
                    new DocProducts()
                    {
                        DocID = docID,
                        ProductID = productid
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
                        InPlaceID = doc.PlaceID
                    },
                    DocProducts = docProducts
                };
                gammaBase.Docs.Add(docProduction);
                DocCloseShiftRemainder = new DocCloseShiftRemainders()
                {
                    DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialId(),
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
            } 
            gammaBase.SaveChanges();
        }
        public bool IsReadOnly
        {
            get { return !DB.HaveWriteAccess("DocCloseShiftRemainders") && !WorkSession.DBAdmin || IsConfirmed; }
        }
    }
}