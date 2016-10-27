using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Windows;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class DocMovementViewModel : SaveImplementedViewModel
    {
        public DocMovementViewModel(Guid docMovementId)
        {
            DocMovementId = docMovementId;
            using (var gammaBase = DB.GammaDb)
            {
                Warehouses = gammaBase.Places.Where(p => p.IsWarehouse ?? false).Select(p => new Place
                {
                    PlaceID = p.PlaceID,
                    PlaceGuid = p.PlaceGuid,
                    PlaceName = p.Name
                }).ToList();
                var docMovement =
                    gammaBase.DocMovement.Include(d => d.Docs).FirstOrDefault(d => d.DocID == docMovementId);
                if (docMovement == null)
                {
                    MessageBox.Show("Перемещение не найдено в базе. Видимо уже удалено");
                    CloseWindow();
                    return;
                }
                Number = docMovement.Docs.Number;
                Date = docMovement.Docs.Date;
                OutPlaceId = docMovement.OutPlaceID;
                InPlaceId = docMovement.InPlaceID;
                IsInVisible = docMovement.OrderTypeID != (int) MovementOrderType.ShipmentOrer;
                IsConfirmed = docMovement.Docs.IsConfirmed;
                MovementProducts = new ObservableCollection<MovementProduct>(gammaBase.vDocMovementProducts.Where(dp => dp.DocMovementID == docMovementId)
                    .Select(dp => new MovementProduct
                    {
                        DocMovementId = dp.DocMovementID,
                        Number = dp.Number,
                        Quantity = dp.Quantity??0,
                        IsConfirmed = dp.IsConfirmed,
                        ProductId = dp.ProductID,
                        NomenclatureName = dp.NomenclatureName,
                        IsShipped = dp.IsShipped ?? false,
                        IsAccepted = dp.IsAccepted ?? false
                    }));
            }
        }

        private Guid DocMovementId { get; set; }
        public bool IsInVisible { get; private set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public int? OutPlaceId { get; set; }
        public int? InPlaceId { get; set; }
        public bool IsConfirmed { get; set; }
        public ObservableCollection<MovementProduct> MovementProducts { get; set; }
        public List<Place> Warehouses { get; private set; }

        public override bool SaveToModel(GammaEntities gammaDb = null)
        {
            if (!DB.HaveWriteAccess("DocMovement")) return true;
            using (var gammaBase = DB.GammaDb)
            {
                var doc =
                    gammaBase.DocMovement.Include(d => d.DocInProducts).Include(d => d.Docs).FirstOrDefault(d => d.DocID == DocMovementId);
                if (doc == null)
                {
                    MessageBox.Show("Документ уже не существует в базе. Скорей всего он был кем-то удален");
                    return true;
                }
                doc.Docs.Number = Number;
                doc.Docs.Date = Date;
                doc.InPlaceID = InPlaceId;
                doc.OutPlaceID = OutPlaceId;
                foreach (var movementProduct in MovementProducts)
                {
                    var inProduct = doc.DocInProducts.FirstOrDefault(p => p.ProductID == movementProduct.ProductId);
                    if (inProduct != null)
                        inProduct.IsConfirmed =
                            movementProduct.IsConfirmed;
                }
            }
            return true;
        }
    }
}
