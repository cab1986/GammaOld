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
        public DocCloseShiftPMRemainderViewModel()
        {
            var productSpool = (from d in DB.GammaBase.Docs
                                where d.PlaceID == WorkSession.PlaceID && d.ShiftID == WorkSession.ShiftID
                                join dp in DB.GammaBase.DocProducts on d.DocID equals dp.DocID
                                join ps in DB.GammaBase.ProductSpools on dp.ProductID equals ps.ProductID
                                orderby d.Date descending
                                select ps).FirstOrDefault();
            if (productSpool != null)
            {
                NomenclatureID = productSpool.C1CNomenclatureID;
                CharacteristicID = productSpool.C1CCharacteristicID;
            }
        }
        public DocCloseShiftPMRemainderViewModel(Guid docID)
        {
            DocCloseShiftRemainder = DB.GammaBase.DocCloseShiftRemainders.Include("Docs").Where(d => d.DocID == docID).
                Select(d => d).FirstOrDefault();
            if (DocCloseShiftRemainder == null)
            {
                var doc = DB.GammaBase.Docs.First(d => d.DocID == docID);
                IsConfirmed = doc.IsConfirmed;
                return;
            }
            IsConfirmed = DocCloseShiftRemainder.Docs.IsConfirmed;
            var productSpool = DB.GammaBase.ProductSpools.FirstOrDefault(p => p.ProductID == DocCloseShiftRemainder.ProductID);
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
        public override void SaveToModel(Guid itemID)
        {
            base.SaveToModel();
            var doc = DB.GammaBase.Docs.First(d => d.DocID == itemID);
            if (DocCloseShiftRemainder == null && Quantity > 0)
            {
                var productid = SqlGuidUtil.NewSequentialid();
                var product = new Products()
                {
                    ProductID = productid,
                    ProductKindID = (byte)ProductKinds.ProductSpool,
                    ProductSpools = new ProductSpools()
                    {
                        C1CCharacteristicID = CharacteristicID,
                        C1CNomenclatureID = (Guid)NomenclatureID,
                        Diameter = 0,
                        Weight = 0,
                        ProductID = productid
                    }
                };
                DB.GammaBase.Products.Add(product);
                var docID = SqlGuidUtil.NewSequentialid();
                var docProducts = new ObservableCollection<DocProducts>();
                docProducts.Add(new DocProducts()
                    {
                        DocID = docID,
                        ProductID = productid
                    }
                    );
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
                DB.GammaBase.Docs.Add(docProduction);
                DocCloseShiftRemainder = new DocCloseShiftRemainders()
                {
                    DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                    DocID = itemID,
                    ProductID = productid,
                    Quantity = Quantity,
                    IsSourceProduct = false
                };
                DB.GammaBase.DocCloseShiftRemainders.Add(DocCloseShiftRemainder);
            }
            else if (DocCloseShiftRemainder != null)
            {
                DocCloseShiftRemainder.Quantity = Quantity;
            } 
            DB.GammaBase.SaveChanges();
        }
        public bool IsReadOnly
        {
            get { return !DB.HaveWriteAccess("DocCloseShiftRemainders") && !WorkSession.DBAdmin || IsConfirmed; }
        }
    }
}