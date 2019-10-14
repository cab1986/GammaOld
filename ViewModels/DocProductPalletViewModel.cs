// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Gamma.Interfaces;
using System.Data.Entity;
using System.Diagnostics;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.DialogViewModels;
using Gamma.Entities;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class DocProductPalletViewModel: SaveImplementedViewModel, IBarImplemented, ICheckedAccess
    {
        public DocProductPalletViewModel()
        {
            PalletItems = new ObservableCollection<ProductItem>();
            Bars.Add(ReportManager.GetReportBar("Pallet", VMID));
            AddNomenclatureToPalletCommand = new DelegateCommand<string>(AddMomenclatureToPallet, (s) => !IsReadOnly );
            DeleteNomenclatureFromPalletCommand = new DelegateCommand(DeleteNomenclatureFromPallet, () => !IsReadOnly);
        }

        /// <summary>
        /// Создание модели для редактирования паллеты
        /// </summary>
        /// <param name="docId">ID документа выработки</param>
        /// <param name="gammaBase">Контекст БД</param>
        //public DocProductPalletViewModel(Guid docId, GammaEntities gammaBase = null) : this()
        public DocProductPalletViewModel(Guid productId, GammaEntities gammaBase = null) : this()
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            ProductId = productId;
            DocId = gammaBase.DocProductionProducts.FirstOrDefault(d => d.ProductID == productId)?.DocID ??
                SqlGuidUtil.NewSequentialid();
            PalletItems = new ObservableCollection<ProductItem>(
                from palItems in gammaBase.ProductItems
                where palItems.ProductID == productId
                select new ProductItem()
                {
                    NomenclatureId = palItems.C1CNomenclatureID,
                    CharacteristicId = (Guid)palItems.C1CCharacteristicID,
                    Quantity = palItems.Quantity??0,
                    NomenclatureName = palItems.C1CNomenclature.Name + " " + palItems.C1CCharacteristics.Name,
                    ProductItemId = palItems.ProductItemID
                } 
                );
            IsConfirmed = gammaBase.Docs.First(d => d.DocID == DocId).IsConfirmed;
        }

        /// <summary>
        /// ID продукта
        /// </summary>
        public Guid? ProductId { get; private set; }

        public Guid? DocId { get; private set; }

        public ObservableCollection<ProductItem> PalletItems
        {
            get;
            set;
        }
        private bool IsConfirmed { get; set; }
        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();
        public Guid? VMID { get; } = Guid.NewGuid();
        public bool IsReadOnly => IsConfirmed && IsValid;

        public override bool CanSaveExecute()
        {
            return base.CanSaveExecute() && PalletItems.Count > 0 && (WorkSession.DBAdmin || DB.HaveWriteAccess("ProductPallets"));
        }
        /// <summary>
        /// Сохранение в БД
        /// </summary>
        /// <param name="itemID">Id документа выработки</param>
        public override bool SaveToModel(Guid itemID)
        {
            if (!CanSaveExecute()) return true;
            using (var gammaBase = DB.GammaDb)
            {
                var product =
                gammaBase.Products.Include(p => p.ProductPallets).Include(p => p.DocProductionProducts)
                    .FirstOrDefault(p => p.ProductID == ProductId);
                if (product == null)
                {
                    var productId = SqlGuidUtil.NewSequentialid();
                    product = new Products()
                    {
                        ProductID = productId,
                        ProductPallets = new ProductPallets()
                        {
                            ProductID = productId,
                            ProductItems = new List<ProductItems>()
                        },
                        DocProductionProducts = new List<DocProductionProducts>
                    {
                        new DocProductionProducts
                        {
                            DocID = itemID,
                            ProductID = productId,
                        }
                    }
                    };
                    gammaBase.Products.Add(product);
                }
                else if (product.ProductPallets == null)
                {
                    product.ProductPallets = new ProductPallets()
                    {
                        ProductID = product.ProductID,
                        ProductItems = new List<ProductItems>()
                    };
                }
                var palItemsToRemove =
                    product.ProductPallets.ProductItems.Where(
                        palItem => !PalletItems.Select(p => p.ProductItemId).Contains(palItem.ProductItemID))
                        .ToArray();
                foreach (var palItem in palItemsToRemove)
                {
                    product.ProductPallets.ProductItems.Remove(palItem);
                }
                gammaBase.SaveChanges();
                var palItemsToAdd =
                    PalletItems.Where(
                        p => !gammaBase.ProductItems.Where(prodItem => prodItem.ProductID == product.ProductID)
                            .Select(prodItem => prodItem.ProductItemID).Contains(p.ProductItemId));
                var docProductionProduct = product.DocProductionProducts.FirstOrDefault();
                var productItems = palItemsToAdd as IList<ProductItem> ?? palItemsToAdd.ToList();
                if (docProductionProduct != null && productItems.Any())
                {
                    docProductionProduct.C1CNomenclatureID = productItems.First().NomenclatureId;
                    docProductionProduct.C1CCharacteristicID = productItems.First().CharacteristicId;
                    docProductionProduct.Quantity = productItems.First().Quantity;
                }
                foreach (var palItem in productItems)
                {
                    product.ProductPallets.ProductItems.Add(new ProductItems()
                    {
                        ProductID = product.ProductID,
                        C1CNomenclatureID = palItem.NomenclatureId,
                        C1CCharacteristicID = palItem.CharacteristicId,
                        ProductItemID = palItem.ProductItemId,
                        Quantity = palItem.Quantity
                    });
                }
                gammaBase.SaveChanges();
                MessageManager.ProductChanged(product.ProductID);
            }
            return true;
        }

        public DelegateCommand<string> AddNomenclatureToPalletCommand { get; private set; }
        public DelegateCommand DeleteNomenclatureFromPalletCommand { get; private set; }
        public ProductItem SelectedProductItem { get; set; }

        private void AddMomenclatureToPallet(string barcode  = null)
        {
            var model = barcode == null ? new AddNomenclatureToPalletDialogModel() : new AddNomenclatureToPalletDialogModel(barcode);
            var okCommand = new UICommand()
            {
                Caption = "OK",
                IsCancel = false,
                IsDefault = true,
                Command = new DelegateCommand<CancelEventArgs>(
            x => DebugFunc(),
            x => model.Quantity > 0 || model.NomenclatureID != null || model.CharacteristicID != null),
            };
            var cancelCommand = new UICommand()
            {
                Id = MessageBoxResult.Cancel,
                Caption = "Отмена",
                IsCancel = true,
                IsDefault = false,
            };
            var dialogService = GetService<IDialogService>();
            var result = dialogService.ShowDialog(
                dialogCommands: new List<UICommand>() { okCommand, cancelCommand },
                title: "Упаковки в паллету",
                viewModel: model);
            if (result != okCommand) return;
            if (model.NomenclatureID == null || model.CharacteristicID == null || model.Quantity <= 0)
            {
                MessageBox.Show("Не удалось добавить упаковки в паллету");
                return;
            }
            string nomenclatureName;
            using (var gammaBase = DB.GammaDb)
            {
                nomenclatureName = gammaBase.C1CNomenclature.FirstOrDefault(
                    n => n.C1CNomenclatureID == model.NomenclatureID)?.Name
                                   + " " +
                                   gammaBase.C1CCharacteristics.FirstOrDefault(
                                       c => c.C1CCharacteristicID == model.CharacteristicID)?.Name;
            }
            var item = new ProductItem((Guid)model.NomenclatureID, (Guid)model.CharacteristicID, (int)model.Quantity, model.NomenclatureName);
            PalletItems.Add(item);
        }

        private void DebugFunc()
        {
            Debug.Print("Упаковки заданы");
        }

        private void DeleteNomenclatureFromPallet()
        {
            if (SelectedProductItem == null) return;
            PalletItems.Remove(SelectedProductItem);
        }
    }
}
