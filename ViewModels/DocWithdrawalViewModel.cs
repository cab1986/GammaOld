using System;
using System.Collections.ObjectModel;
using System.Linq;
using Gamma.Entities;
using Gamma.Interfaces;
using Gamma.Models;
using System.Data.Entity;
using System.Windows;

namespace Gamma.ViewModels
{
    public class DocWithdrawalViewModel : SaveImplementedViewModel, ICheckedAccess
    {
        private ObservableCollection<WithdrawalProduct> _withdrawalProducts;
        private ObservableCollection<WithdrawalMaterial> _withdrawalMaterials;

        public DocWithdrawalViewModel(Guid docId)
        {
            DocId = docId;
            using (var gammaBase = DB.GammaDb)
            {
                var doc = gammaBase.Docs.Include(d => d.DocWithdrawal).Include(d => d.DocWithdrawal.Places)
                    .FirstOrDefault(d => d.DocID == DocId);
                if (doc == null)
                {
                    MessageBox.Show("Не удалось получить информацию о документе", "Ошибка загрузки документа",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Date = doc.Date;
                PrintName = doc.PrintName;
                DocNumber = doc.Number;
                Place = doc.DocWithdrawal.Places?.Name;
                WithdrawalProducts = new ObservableCollection<WithdrawalProduct>(
                    gammaBase.DocWithdrawalProducts.Where(dw => dw.DocID == DocId).Join(gammaBase.vProductsInfo, dw => dw.ProductID, pi => pi.ProductID, (dw, pi) => new WithdrawalProduct
                    {
                        ProductId = dw.ProductID,
                        Quantity = dw.Quantity,
                        MeasureUnit = pi.BaseMeasureUnit,
                        NomenclatureName = pi.NomenclatureName,
                        Number = pi.Number,
                        CompleteWithdrawal = dw.CompleteWithdrawal ?? false,
                    }));
                foreach (var product in WithdrawalProducts.AsParallel())
                {
                    product.MaxQuantity = DB.CalculateSpoolWeightBeforeDate(product.ProductId, Date);
                }
                WithdrawalMaterials = new ObservableCollection<WithdrawalMaterial>(
                    gammaBase.DocWithdrawalMaterials.Where(wm => wm.DocID == DocId)
                    .Select(wm => new WithdrawalMaterial
                    {
                        NomenclatureName = wm.C1CNomenclature.Name + " " + wm.C1CCharacteristics.Name,
                        NomenclatureID = wm.C1CNomenclatureID,
                        Quantity = wm.Quantity,
                        CharacteristicID = wm.C1CCharacteristicID,
                        MeasureUnit = wm.C1CNomenclature.C1CMeasureUnitStorage.Name,
                        MeasureUnitID = wm.C1CNomenclature.C1CMeaureUnitStorage,
                        DocWithdrawalMaterialID = wm.DocWithdrawalMaterialID
                    }));

            }
        }

        public bool IsReadOnly { get; }

        public string Place { get; set; }
        public DateTime Date { get; set; }
        public string PrintName { get; set; }
        public string DocNumber { get; set; }
        private Guid DocId { get; set; }

        public ObservableCollection<WithdrawalProduct> WithdrawalProducts
        {
            get { return _withdrawalProducts; }
            set
            {
                _withdrawalProducts = value;
                RaisePropertyChanged("WithdrawalProducts");
            }
        }

        public ObservableCollection<WithdrawalMaterial> WithdrawalMaterials
        {
            get { return _withdrawalMaterials; }
            set
            {
                _withdrawalMaterials = value;
                RaisePropertyChanged("WithdrawalMaterials");
            }
        }

        public override bool SaveToModel(GammaEntities gammaDb = null)
        {
            if (!DB.HaveWriteAccess("DocWithdrawalProducts")) return true;
            using (var gammaBase = DB.GammaDb)
            {
                var doc = gammaBase.Docs.FirstOrDefault(d => d.DocID == DocId);
                if (doc == null)
                {
                    MessageBox.Show("Не удалось сохранить документ");
                    return false;
                }
                var productIds = WithdrawalProducts.Select(wp => wp.ProductId).ToList();
                gammaBase.DocWithdrawalProducts.RemoveRange(
                    gammaBase.DocWithdrawalProducts.Where(dw => dw.DocID == DocId && !productIds.Contains(dw.ProductID)));
                foreach (var product in WithdrawalProducts)
                {
                    var withdrawalProduct =
                        gammaBase.DocWithdrawalProducts.FirstOrDefault(
                            dw => dw.DocID == DocId && dw.ProductID == product.ProductId);
                    if (withdrawalProduct == null)
                    {
                        withdrawalProduct = new DocWithdrawalProducts
                        {
                            DocID = DocId,
                            ProductID = product.ProductId                            
                        };
                        gammaBase.DocWithdrawalProducts.Add(withdrawalProduct);
                    }
                    withdrawalProduct.Quantity = product.Quantity;
                    withdrawalProduct.CompleteWithdrawal = product.CompleteWithdrawal;
                }
                var materilaIds = WithdrawalMaterials.Select(wm => wm.DocWithdrawalMaterialID).ToList();
                gammaBase.DocWithdrawalMaterials.RemoveRange(
                    gammaBase.DocWithdrawalMaterials.Where(
                        wm => wm.DocID == DocId && !materilaIds.Contains(wm.DocWithdrawalMaterialID)));
                foreach (var material in WithdrawalMaterials)
                {
                    var withdrawalMaterial =
                        gammaBase.DocWithdrawalMaterials.FirstOrDefault(
                            wm => wm.DocID == DocId && wm.DocWithdrawalMaterialID == material.DocWithdrawalMaterialID);
                    if (withdrawalMaterial == null)
                    {
                        withdrawalMaterial = new DocWithdrawalMaterials
                        {
                            DocID = DocId,
                            DocWithdrawalMaterialID = material.DocWithdrawalMaterialID
                        };
                        gammaBase.DocWithdrawalMaterials.Add(withdrawalMaterial);
                    }
                    withdrawalMaterial.Quantity = material.Quantity;
                    withdrawalMaterial.C1CNomenclatureID = material.NomenclatureID;
                    withdrawalMaterial.C1CCharacteristicID = material.CharacteristicID;
                }
                gammaBase.SaveChanges();
            }
            return true;
        }
    }
}
