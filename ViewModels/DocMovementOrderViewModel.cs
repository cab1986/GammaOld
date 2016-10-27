using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Data.Entity;
using System.Globalization;
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
            //RefreshProductsCommand = new DelegateCommand(RefreshProducts);
            WareHouses = gammaBase.Places.Where(p => p.BranchID == WorkSession.BranchID && (p.IsWarehouse ?? false))
                .Select(p => new Place
                {
                    PlaceID = p.PlaceID,
                    PlaceGuid = p.PlaceGuid,
                    PlaceName = p.Name
                }).ToList();
            PlaceTo = WareHouses.FirstOrDefault()?.PlaceID ?? 0;
        }

        public DocMovementOrderViewModel(Guid docMovementOrderId, GammaEntities gammaBase = null) : this(gammaBase)
        {
//            IsMovementOrder = isMovementOrder;
            DocId = docMovementOrderId;
            using (gammaBase = gammaBase ?? DB.GammaDb)
            {
                var doc =
                    gammaBase.Docs.Include(d => d.DocMovementOrder)
                        .Include(d => d.DocMovementOrder.DocMovementOrderNomenclature).First(d => d.DocID == docMovementOrderId);
                PlaceTo = doc.DocMovementOrder.InPlaceID;
                PlaceFrom = doc.DocMovementOrder.OutPlaceID;
                Date = doc.Date;
                Number = doc.Number;
                IsConfirmed = doc.IsConfirmed;
                foreach (var nomenclatureItem in doc.DocMovementOrder.DocMovementOrderNomenclature)
                {
                    DocMovementOrderItems.Add(new MovementGood
                    {
                        NomenclatureID = nomenclatureItem.C1CNomenclatureID,
                        CharacteristicID = nomenclatureItem.C1CCharacteristicID,
                        Amount = (nomenclatureItem.Amount??0).ToString(CultureInfo.CurrentCulture)
                    });
                }
            }
            FillProducts(DocMovementOrderItems, DocId);
        }

        public DateTime Date { get; set; }
        public string Number { get; set; }

        public ItemsChangeObservableCollection<MovementGood> DocMovementOrderItems
        {
            get { return _docMmovementOrderItems; }
            set
            {
                _docMmovementOrderItems = value;
                RaisePropertyChanged("DocMovemementOrderItems");
            }
        }

        [Required(ErrorMessage = @"Необходимо указать склад приемки")]
        public int? PlaceTo { get; set; }
        [Required(ErrorMessage = @"Необходимо указать исходный склад")]
        public int? PlaceFrom { get; set; }
//        public List<Place> Places { get; private set; }

        public MovementGood SelectedMovementOrderNomenclatureItem { get; set; }

        public Guid DocId { get; private set; } = SqlGuidUtil.NewSequentialid();

//        private ObservableCollection<MovementProduct> _movementOrderProducts;
        private ItemsChangeObservableCollection<MovementGood> _docMmovementOrderItems = new ItemsChangeObservableCollection<MovementGood>();
/*
        public ObservableCollection<MovementProduct> DocMovementOrderProducts
        {
            get { return _movementOrderProducts; }
            private set
            {
                _movementOrderProducts = value;
                RaisePropertyChanged("DocMovementOrderProducts");
            }
        }
*/
//        private bool IsMovementOrder { get; set; }

        public DelegateCommand AddMovementOrderItemCommand { get; private set; }
        public DelegateCommand DeleteMovementOrderItemCommand { get; private set; }

        private void DeleteMovementOrderItem()
        {
            if (SelectedMovementOrderNomenclatureItem == null) return;
            DocMovementOrderItems.Remove(SelectedMovementOrderNomenclatureItem);
        }

        private void AddMovementOrderItem()
        {
            DocMovementOrderItems.Add(new MovementGood());
        }

        public List<Place> WareHouses { get; private set; }

//        public DelegateCommand RefreshProductsCommand { get; private set; }
/// <summary>
/// Заполнение номенклатурных позиций продуктами
/// </summary>
/// <param name="movementGoods">Номенклатурный список</param>
/// <param name="docId">ID заказа на перемещение</param>
        private void FillProducts(ICollection<MovementGood> movementGoods, Guid docId)
        {
            using (var gammaBase = DB.GammaDb)
            {
                var products = gammaBase.GetDocMovementOrderProducts(docId).ToList();
                foreach (var good in movementGoods)
                {
                    good.Products =
                        products.Where(
                            p =>
                                p.C1CNomenclatureID == good.NomenclatureID &&
                                p.C1CCharacteristicID == good.CharacteristicID)
                            .Select(p => new MovementProduct
                            {
                                Quantity = p.Quantity??0,
                                NomenclatureId = p.C1CNomenclatureID,
                                CharacteristicId = p.C1CCharacteristicID,
                                ProductId = p.ProductID,
                                IsConfirmed = p.IsConfirmed,
                                Number = p.Number,
                                NomenclatureName = p.NomenclatureName,
                                IsShipped = p.IsShipped??false,
                                ProductKind = (ProductKind)p.ProductKindID,
                                IsAccepted = p.IsAccepted??false,
                                DocMovementId = p.DocMovementID
                            }).ToList();
                }
            }
        }

        public bool IsConfirmed { get; set; }

        public override bool SaveToModel(GammaEntities gammaBase = null)
        {
            using (gammaBase = gammaBase ?? DB.GammaDb)
            {
                if (!DB.HaveWriteAccess("DocMovementOrder")) return true;
                var docMovementOrder =
                    gammaBase.DocMovementOrder.Include(d => d.Docs)
                        .Include(d => d.DocMovementOrderNomenclature)
                        .FirstOrDefault(d => d.DocID == DocId);
                if (docMovementOrder == null)
                {
                    docMovementOrder = new DocMovementOrder
                    {
                        DocID = DocId,
                        Docs = new Docs
                        {
                            Date = DB.CurrentDateTime,
                            DocTypeID = (int)DocTypes.DocMovementOrder,
                            IsConfirmed = true,
                            UserID = WorkSession.UserID,
                            PlaceID = WorkSession.PlaceID,
                            ShiftID = WorkSession.ShiftID,
                            PrintName = WorkSession.PrintName
                        }
                    };
                    gammaBase.DocMovementOrder.Add(docMovementOrder);
                }
                docMovementOrder.Docs.Number = Number;
                docMovementOrder.Docs.Date = Date;
                docMovementOrder.Docs.IsConfirmed = IsConfirmed;
                docMovementOrder.OutPlaceID = PlaceFrom;
                docMovementOrder.InPlaceID = PlaceTo;
                docMovementOrder.DocMovementOrderNomenclature = new List<DocMovementOrderNomenclature>();
                foreach (var item in DocMovementOrderItems)
                {
                    docMovementOrder.DocMovementOrderNomenclature.Add(new DocMovementOrderNomenclature
                    {
                        DocID = DocId,
                        C1CNomenclatureID = item.NomenclatureID,
                        C1CCharacteristicID = item.CharacteristicID,
                        Amount = Convert.ToDecimal(item.Amount),
                        DocMovementOrderNomenclatureID = SqlGuidUtil.NewSequentialid()
                    });
                }
                gammaBase.SaveChanges();
            }
            return true;
        }
    }
}
