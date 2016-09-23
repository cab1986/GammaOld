using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data.Entity;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class DocMovementOrderViewModel : SaveImplementedViewModel
    {
        public DocMovementOrderViewModel(GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            Number = DB.GetNextDocNumber(DocTypes.DocMovementOrder, WorkSession.PlaceID, WorkSession.ShiftID);
            Date = DB.CurrentDateTime;
            AddMovementOrderItemCommand = new DelegateCommand(AddMovementOrderItem);
            DeleteMovementOrderItemCommand = new DelegateCommand(DeleteMovementOrderItem, () => SelectedMovementOrderNomenclatureItem != null);
            RefreshProductsCommand = new DelegateCommand(RefreshProducts);
            WareHouses = gammaBase.Places.Where(p => p.BranchID == WorkSession.BranchID && (p.IsWarehouse ?? false))
                .Select(p => new Place
                {
                    PlaceID = p.PlaceID,
                    PlaceGuid = p.PlaceGuid,
                    PlaceName = p.Name
                }).ToList();
            PlaceTo = WareHouses.FirstOrDefault()?.PlaceID ?? 0;
        }

        public DocMovementOrderViewModel(Guid docId, GammaEntities gammaBase = null) : this(gammaBase)
        {
            DocId = docId;
            using (gammaBase = gammaBase ?? DB.GammaDb)
            {
                var doc =
                    gammaBase.Docs.Include(d => d.DocMovementOrder)
                        .Include(d => d.DocMovementOrder.DocMovementOrderNomenclature).First(d => d.DocID == docId);
                PlaceTo = doc.DocMovementOrder.PlaceTo;
                PlaceFrom = doc.DocMovementOrder.PlaceFrom;
                Date = doc.Date;
                Number = doc.Number;
                IsConfirmed = doc.IsConfirmed;
                foreach (var nomenclatureItem in doc.DocMovementOrder.DocMovementOrderNomenclature)
                {
                    DocMovementOrderItems.Add(new DocMovementOrderNomenclatureItem
                    {
                        NomenclatureID = nomenclatureItem.C1CNomenclatureID,
                        CharacteristicID = nomenclatureItem.C1CCharacteristicID,
                        Quantity = nomenclatureItem.Amount ??0,
                        DocId = DocId
                    });
                }
            }
            RefreshProducts();
        }

        public DateTime Date { get; set; }
        public string Number { get; set; }

        public ItemsChangeObservableCollection<DocMovementOrderNomenclatureItem> DocMovementOrderItems
        {
            get { return _docMmovementOrderItems; }
            set
            {
                _docMmovementOrderItems = value;
                RaisePropertyChanged("DocMovemementOrderItems");
            }
        }

        public int PlaceTo { get; set; }
        public int? PlaceFrom { get; set; }
        public List<Place> Places { get; private set; }

        public DocMovementOrderNomenclatureItem SelectedMovementOrderNomenclatureItem { get; set; }

        public Guid DocId { get; private set; } = SqlGuidUtil.NewSequentialid();

        private ObservableCollection<ProductInfo> _movementOrderProducts;
        private ItemsChangeObservableCollection<DocMovementOrderNomenclatureItem> _docMmovementOrderItems = new ItemsChangeObservableCollection<DocMovementOrderNomenclatureItem>();

        public ObservableCollection<ProductInfo> DocMovementOrderProducts
        {
            get { return _movementOrderProducts; }
            private set
            {
                _movementOrderProducts = value;
                RaisePropertyChanged("DocMovementOrderProducts");
            }
        }

        public DelegateCommand AddMovementOrderItemCommand { get; private set; }
        public DelegateCommand DeleteMovementOrderItemCommand { get; private set; }

        private void DeleteMovementOrderItem()
        {
            if (SelectedMovementOrderNomenclatureItem == null) return;
            DocMovementOrderItems.Remove(SelectedMovementOrderNomenclatureItem);
        }

        private void AddMovementOrderItem()
        {
            DocMovementOrderItems.Add(new DocMovementOrderNomenclatureItem()
            {
                DocId = DocId
            });
        }

        public List<Place> WareHouses { get; private set; }

        public DelegateCommand RefreshProductsCommand { get; private set; }

        private void RefreshProducts()
        {
            using (var gammaBase = DB.GammaDb)
            {
                DocMovementOrderProducts = new ItemsChangeObservableCollection<ProductInfo>(gammaBase.GetDocMovementOrderProducts(DocId)
                    .Select(d => new ProductInfo()
                    {
                        NomenclatureID = d.C1CNomenclatureID,
                        CharacteristicID = d.C1CCharacteristicID,
                        ProductID = d.ProductID,
                        Quantity = d.Quantity,
                        Number = d.Number,
                        ProductKind = (ProductKinds)d.ProductKindID,
                        NomenclatureName = d.NomenclatureName
                    }));
            }
        }

        public bool IsConfirmed { get; set; }

        public override bool SaveToModel(GammaEntities gammaBase = null)
        {
            using (gammaBase = gammaBase ?? DB.GammaDb)
            {
                if (!DB.HaveWriteAccess("DocMovementOrder")) return true;
                var doc =
                    gammaBase.Docs.Include(d => d.DocMovementOrder)
                        .Include(d => d.DocMovementOrder.DocMovementOrderNomenclature)
                        .FirstOrDefault(d => d.DocID == DocId);
                if (doc == null)
                {
                    doc = new Docs()
                    {
                        DocID = DocId,
                        Date = DB.CurrentDateTime,
                        DocTypeID = (int) DocTypes.DocMovementOrder,
                        IsConfirmed = true,
                        UserID = WorkSession.UserID,
                        PlaceID = WorkSession.PlaceID,
                        ShiftID = WorkSession.ShiftID,
                        PrintName = WorkSession.PrintName,
                        DocMovementOrder = new DocMovementOrder()
                        {
                            DocID = DocId,
                            DocMovementOrderNomenclature = new List<DocMovementOrderNomenclature>()
                        }
                    };
                    gammaBase.Docs.Add(doc);
                }
                doc.Number = Number;
                doc.Date = Date;
                doc.IsConfirmed = IsConfirmed;
                doc.DocMovementOrder.PlaceFrom = PlaceFrom;
                doc.DocMovementOrder.PlaceTo = PlaceTo;
                doc.DocMovementOrder.DocMovementOrderNomenclature = new List<DocMovementOrderNomenclature>();
                foreach (var item in DocMovementOrderItems)
                {
                    doc.DocMovementOrder.DocMovementOrderNomenclature.Add(new DocMovementOrderNomenclature
                    {
                        DocID = DocId,
                        C1CNomenclatureID = item.NomenclatureID,
                        C1CCharacteristicID = item.CharacteristicID,
                        Amount = item.Quantity,
                        DocMovementOrderNomenclatureID = SqlGuidUtil.NewSequentialid()
                    });
                }
                gammaBase.SaveChanges();
            }
            return true;
        }
    }
}
