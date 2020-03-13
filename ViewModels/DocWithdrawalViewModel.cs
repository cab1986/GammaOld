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
        private ObservableCollection<Products> _productWithdrawals;

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
                UserName = doc.Users?.Name;
                ShiftID = doc.ShiftID;
                IsConfirmed = doc.IsConfirmed || !AllowEditDoc;
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
                var dW = gammaBase.DocWithdrawal.FirstOrDefault(dw => dw.DocID == DocId);
                var docProductionIDs = dW?.DocProduction?.Select(x => x.DocID).ToList();
                ProductWithdrawals = docProductionIDs == null ? new ObservableCollection<Products>() : new ObservableCollection<Products>(
                    gammaBase.Products.Where(x => x.DocProductionProducts.Any(dpp => docProductionIDs.Contains(dpp.DocID))));
            }
            AllowEditDoc = DB.AllowEditDoc(DocId);
            IsReadOnly = !DB.HaveWriteAccess("DocWithdrawalProducts") || (IsConfirmed && IsValid);
        }

        public bool IsReadOnly { get; }
        public bool IsAllowEditing
        { get
            {
                return !IsReadOnly;
            }
        }

        public string Place { get; set; }
        public DateTime Date { get; set; }
        public string PrintName { get; set; }
        public string DocNumber { get; set; }
        private Guid DocId { get; set; }
        public string UserName { get; set; }
        public byte? ShiftID { get; set; }
        private bool _isConfirmed;

        private bool AllowEditDoc { get; set; } = true;

        public bool IsConfirmed
        {
            get
            {
                return _isConfirmed;
            }
            set
            {
                if (_isConfirmed)
                    if (!AllowEditDoc)
                    {
                        MessageBox.Show("Правка невозможна. Продукция уже в выработке или с ней связаны другие документы");
                        return;
                    }
                    //else
                    //{
                    //    if (Doc != null && GammaBase.Docs.Any(p => p.DocID == Doc.DocID && p.DocCloseShift.Any(d => !d.IsConfirmed)))
                    //    {
                    //        var docCloseShift = Doc.DocCloseShift.FirstOrDefault();
                    //        MessageBox.Show("Продукт № " + Number + " будет удален из рапорта закрытия смены № " + docCloseShift?.Number + " от " + docCloseShift?.Date.ToString() + " Смена " + docCloseShift?.ShiftID.ToString() + ". ОБЯЗАТЕЛЬНО откройте рапорт закрытия смены и заполните повторно!");
                    //        var delResult = GammaBase.DeleteDocFromDocCloseShiftDocs(Doc.DocID).FirstOrDefault();
                    //        if (delResult != "")
                    //        {
                    //            MessageBox.Show(delResult, "Удалить не удалось", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    //            return;
                    //        }
                    //    }
                    //};
                if (value && !IsValid)
                    _isConfirmed = false;
                else
                    _isConfirmed = value;
                
                RaisePropertyChanged("IsConfirmed");
            }
        }

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

        public ObservableCollection<Products> ProductWithdrawals
        {
            get { return _productWithdrawals; }
            set
            {
                _productWithdrawals = value;
                RaisePropertyChanged("ProductWithdrawals");
            }
        }


        public override bool CanSaveExecute()
        {
            return IsValid && DB.HaveWriteAccess("DocWithdrawalProducts");
        }

        public override bool SaveToModel()
        {
            //if (!DB.HaveWriteAccess("DocWithdrawalProducts")) return true;
            if (IsReadOnly && IsConfirmed) return true;
            using (var gammaBase = DB.GammaDb)
            {
                var doc = gammaBase.Docs.FirstOrDefault(d => d.DocID == DocId);
                if (doc == null)
                {
                    MessageBox.Show("Не удалось сохранить документ");
                    return false;
                }
                if (doc.IsConfirmed != IsConfirmed)
                    doc.IsConfirmed = IsConfirmed;

                if (WithdrawalProducts != null)
                {
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
                }
                /* Изменения можно внести только в продуктах списанных
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
                }*/
                gammaBase.SaveChanges();
            }
            return true;
        }
    }
}
