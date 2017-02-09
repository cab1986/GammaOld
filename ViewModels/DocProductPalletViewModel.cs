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
            PalletItems = new ObservableCollection<ProductPalletItem>();
            Bars.Add(ReportManager.GetReportBar("Pallet", VMID));
            AddNomenclatureToPalletCommand = new DelegateCommand<string>(AddMomenclatureToPallet, (s) => !IsReadOnly );
            DeleteNomenclatureFromPalletCommand = new DelegateCommand(DeleteNomenclatureFromPallet, () => !IsReadOnly);
        }

        /// <summary>
        /// Создание модели для редактирования паллеты
        /// </summary>
        /// <param name="docId">ID документа выработки</param>
        /// <param name="gammaBase">Контекст БД</param>
        public DocProductPalletViewModel(Guid docId, GammaEntities gammaBase = null) : this()
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            var productId = gammaBase.DocProductionProducts.Where(d => d.DocID == docId).Select(d => d.ProductID).First();
            PalletItems = new ObservableCollection<ProductPalletItem>(
                from palItems in gammaBase.ProductPalletItems
                where palItems.ProductID == productId
                select new ProductPalletItem()
                {
                    NomenclatureId = palItems.C1CNomenclatureID,
                    CharacteristicId = palItems.C1CCharacteristicID,
                    Quantity = palItems.Quantity??0,
                    NomenclatureName = palItems.C1CNomenclature.Name + " " + palItems.C1CCharacteristics.Name,
                    ProductPalletItemId = palItems.ProductPalletItemID
                } 
                );
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

        public override bool CanSaveExecute()
        {
            return base.CanSaveExecute() && PalletItems.Count > 0 && (WorkSession.DBAdmin || DB.HaveWriteAccess("ProductPallets"));
        }
        /// <summary>
        /// Сохранение в БД
        /// </summary>
        /// <param name="itemID">Id документа выработки</param>
        /// <param name="gammaBase">Контекст базы данныъ</param>
        public override bool SaveToModel(Guid itemID, GammaEntities gammaBase = null)
        {
            if (!CanSaveExecute()) return true;
            gammaBase = gammaBase ?? DB.GammaDb;
            var product =
                gammaBase.Products.Include(p => p.ProductPallets)
                    .Include(
                        p =>
                            p.ProductPallets.ProductPalletItems).First(p => gammaBase.DocProductionProducts.Where(d => d.DocID == itemID)
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
                    ProductPalletItems = new List<ProductPalletItems>()
                };
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
            var docProductionProduct = product.DocProductionProducts.FirstOrDefault();
            var productPalletItems = palItemsToAdd as IList<ProductPalletItem> ?? palItemsToAdd.ToList();
            if (docProductionProduct != null && productPalletItems.Any())
            {
                docProductionProduct.C1CNomenclatureID = productPalletItems.First().NomenclatureId;
                docProductionProduct.C1CCharacteristicID = productPalletItems.First().CharacteristicId;
                docProductionProduct.Quantity = productPalletItems.First().Quantity;
            }
            foreach (var palItem in productPalletItems)
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
            MessageManager.ProductChanged(product.ProductID);
            return true;
        }

        public DelegateCommand<string> AddNomenclatureToPalletCommand { get; private set; }
        public DelegateCommand DeleteNomenclatureFromPalletCommand { get; private set; }
        public ProductPalletItem SelectedProductPalletItem { get; set; }

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
            var item = new ProductPalletItem((Guid)model.NomenclatureID, (Guid)model.CharacteristicID, (int)model.Quantity, model.NomenclatureName);
            PalletItems.Add(item);
        }

        private void DebugFunc()
        {
            Debug.Print("Упаковки заданы");
        }

        private void DeleteNomenclatureFromPallet()
        {
            if (SelectedProductPalletItem == null) return;
            PalletItems.Remove(SelectedProductPalletItem);
        }
    }
}
