using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Gamma.Interfaces;
using Gamma.Models;
using System.Data.Entity;

namespace Gamma.ViewModels
{
    public class DocProductPalletViewModel: SaveImplementedViewModel, IBarImplemented, ICheckedAccess
    {
        public DocProductPalletViewModel()
        {
            PalletItems = new ObservableCollection<ProductPalletItem>();
            Bars.Add(ReportManager.GetReportBar("Pallet", VMID));
        }

        /// <summary>
        /// Создание модели для редактирования паллеты
        /// </summary>
        /// <param name="docId">ID документа выработки</param>
        /// <param name="gammaBase">Контекст БД</param>
        public DocProductPalletViewModel(Guid docId, GammaEntities gammaBase = null) : this()
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            var productId = gammaBase.DocProducts.Where(d => d.DocID == docId).Select(d => d.ProductID).First();
            PalletItems = new ObservableCollection<ProductPalletItem>(
                from palItems in gammaBase.ProductPalletItems
                where palItems.ProductID == productId
                select new ProductPalletItem
                {
                    ProductPalletItemId = palItems.ProductPalletItemID,
                    NomenclatureName = palItems.C1CNomenclature.Name + " " + palItems.C1CCharacteristics.Name,
                    Quantity = palItems.Quantity??0,
                    NomenclatureId = palItems.C1CNomenclatureID,
                    CharacteristicId = palItems.C1CCharacteristicID
                });
            IsConfirmed = gammaBase.Docs.First(d => d.DocID == docId).IsConfirmed;
        }

        public ObservableCollection<ProductPalletItem> PalletItems
        {
            get;
            set;
        }
        private bool IsConfirmed { get; set; }
        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();
        public Guid? VMID { get; } = Guid.NewGuid();
        public bool IsReadOnly => IsConfirmed && IsValid;

        public class ProductPalletItem
        {
            public Guid ProductPalletItemId { get; set; } = SqlGuidUtil.NewSequentialid();
            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            public string NomenclatureName { get; set; }
            public int Quantity { get; set; }
            public Guid CharacteristicId { get; set; }
            public Guid NomenclatureId { get; set; }
        }

        public override bool CanSaveExecute()
        {
            return base.CanSaveExecute() && PalletItems.Count > 0 && (WorkSession.DBAdmin || DB.HaveWriteAccess("ProductPallets"));
        }
        /// <summary>
        /// Сохранение в БД
        /// </summary>
        /// <param name="itemID">Id документа выработки</param>
        /// <param name="gammaBase">Контекст базы данныъ</param>
        public override void SaveToModel(Guid itemID, GammaEntities gammaBase = null)
        {
            if (!CanSaveExecute()) return;
            gammaBase = gammaBase ?? DB.GammaDb;
            base.SaveToModel(itemID, gammaBase);
            var product =
                gammaBase.Products.Include(p => p.ProductPallets)
                    .Include(
                        p =>
                            p.ProductPallets.ProductPalletItems).First(p => gammaBase.DocProducts.Where(d => d.DocID == itemID)
                                .Select(d => d.ProductID)
                                .Contains(p.ProductID));
            if (product == null)
            {
                var productId = SqlGuidUtil.NewSequentialid();
                product = new Products()
                {
                    ProductID = productId,
                    ProductPallets = new ProductPallets()
                    {
                        ProductID = productId,
                        ProductPalletItems = new List<ProductPalletItems>()
                    },
                    DocProducts = new List<DocProducts>
                    {
                        new DocProducts
                        {
                            DocID = itemID,
                            ProductID = productId,
                            IsInConfirmed = true
                        }
                    }
                };
                gammaBase.Products.Add(product);
            }
            var palItemsToRemove =
                product.ProductPallets.ProductPalletItems.Where(
                    palItem => !PalletItems.Select(p => p.ProductPalletItemId).Contains(palItem.ProductPalletItemID))
                    .ToArray();
            foreach (var palItem in palItemsToRemove)
            {
                product.ProductPallets.ProductPalletItems.Remove(palItem);
            }
            var palItemsToAdd =
                PalletItems.Where(
                    p => !gammaBase.ProductPalletItems.Where(prodItem => prodItem.ProductID == product.ProductID)
                        .Select(prodItem => prodItem.ProductPalletItemID).Contains(p.ProductPalletItemId));
            foreach (var palItem in palItemsToAdd)
            {
                product.ProductPallets.ProductPalletItems.Add(new ProductPalletItems()
                {
                    ProductID = product.ProductID,
                    C1CNomenclatureID = palItem.NomenclatureId,
                    C1CCharacteristicID = palItem.CharacteristicId,
                    ProductPalletItemID = palItem.ProductPalletItemId,
                    Quantity = palItem.Quantity
                });
            }
            gammaBase.SaveChanges();
        }
    }
}
