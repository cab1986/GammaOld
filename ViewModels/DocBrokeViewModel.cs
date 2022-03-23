// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Common;
using Gamma.Models;
using System.Data.Entity;
using DevExpress.Mvvm;
using Gamma.Interfaces;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using Gamma.Attributes;
using Gamma.Entities;
using Gamma.DialogViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Threading;

namespace Gamma.ViewModels
{
    public class DocBrokeViewModel : SaveImplementedViewModel, IBarImplemented, ICheckedAccess
    {
        public DocBrokeViewModel(Guid docBrokeId, Guid? productId = null, bool isInFuturePeriod = false)
        {
            ClosingCommand = new DelegateCommand<CancelEventArgs>(Closing);
            DB.AddLogMessageInformation("Открытие Акта о браке DocID", "Open DocBrokeViewModel (docBrokeId = '" + docBrokeId+"', productId='"+ productId+ "', isInFuturePeriod='" + isInFuturePeriod+"')",docBrokeId);
            Bars.Add(ReportManager.GetReportBar("DocBroke", VMID));
            Messenger.Default.Register<BarcodeMessage>(this, BarcodeReceived);
            DocId = docBrokeId;
            using (var gammaBase = DB.GammaDb)
            {
                DiscoverPlaces = gammaBase.Places.Where(p => ((p.IsProductionPlace ?? false) || (p.IsWarehouse ?? false)) && WorkSession.BranchIds.Contains(p.BranchID))
                .Select(p => new Place
                {
                    PlaceGuid = p.PlaceGuid,
                    PlaceID = p.PlaceID,
                    PlaceName = p.Name
                }).ToList();

                BrokePlaces = gammaBase.Places.Where(p => ((p.IsProductionPlace ?? false) || (p.IsWarehouse ?? false)) && WorkSession.BranchIds.Contains(p.BranchID))
                .Select(p => new Place
                {
                    PlaceGuid = p.PlaceGuid,
                    PlaceID = p.PlaceID,
                    PlaceName = p.Name
                })
                .ToList();

                var brokePlaces = gammaBase.Places.Where(p => ((p.IsProductionPlace ?? false) || (p.IsWarehouse ?? false)) && !WorkSession.BranchIds.Contains(p.BranchID))
                .Join(gammaBase.Branches, p => p.BranchID, b => b.BranchID, (p, b)
                 => new Place
                 {
                     PlaceGuid = p.PlaceGuid,
                     PlaceID = p.PlaceID,
                     PlaceName = (WorkSession.BranchIds.Contains(p.BranchID) ? p.Name : b.Name + "#" + p.Name)
                 })
                .ToList();
                foreach (var place in brokePlaces)
                {
                    BrokePlaces.Add(place);
                }

                StorePlaces = gammaBase.Places.Where(p => (p.IsWarehouse ?? false) && WorkSession.BranchIds.Contains(p.BranchID))
                    .Select(p => new Place
                    {
                        PlaceGuid = p.PlaceGuid,
                        PlaceID = p.PlaceID,
                        PlaceName = p.Name
                    }).ToList();
                var storePlaces = gammaBase.Places.Where(p => (p.IsWarehouse ?? false) && !WorkSession.BranchIds.Contains(p.BranchID))
                .Join(gammaBase.Branches, p => p.BranchID, b => b.BranchID, (p, b)
                 => new Place
                 {
                     PlaceGuid = p.PlaceGuid,
                     PlaceID = p.PlaceID,
                     PlaceName = (WorkSession.BranchIds.Contains(p.BranchID) ? p.Name : b.Name + "#" + p.Name)
                 })
                .ToList();
                foreach (var place in storePlaces)
                {
                    StorePlaces.Add(place);
                }

                var needsDecisionStateList = GammaBase.ProductStates
                        .Where(r => r.C1CQualityID == new Guid("fe253c85-4e67-11eb-832e-00155d00f107"))
                        .OrderBy(r => r.StateID);
                foreach (var item in needsDecisionStateList)
                {
                    NeedsDecisionStateList.Add(new KeyValuePair<byte, string>(item.StateID, item.Name));
                }                

                var doc = gammaBase.Docs.Include(d => d.DocBroke)
                    .Include(d => d.DocBroke.DocBrokeProducts)
                    .Include(d => d.DocBroke.DocBrokeProducts.Select(dp => dp.DocBrokeProductRejectionReasons))
                    .FirstOrDefault(d => d.DocID == DocId);
                InFuturePeriodList = new Dictionary<bool, string>();
                InFuturePeriodList.Add(false, "25к - I передел");
                InFuturePeriodList.Add(true, "10к - II передел");
                BrokeProducts = new ItemsChangeObservableCollection<BrokeProduct>();
                DocBrokeDecision = new DocBrokeDecisionViewModel(DocId);// this);
                if (doc != null)
                {
                    DocNumber = doc.Number;
                    DocComment = doc.Comment;
                    Date = doc.Date;
                   // PlaceDiscoverId = doc.DocBroke.PlaceDiscoverID;
                    //PlaceStoreId = doc.DocBroke.PlaceStoreID;
                    IsInFuturePeriod = doc.DocBroke.IsInFuturePeriod ?? false;
                    //SelectedInFuturePeriod = doc.DocBroke.IsInFuturePeriod ?? false ? FuturePeriodDocBrokeType.NextPlace : FuturePeriodDocBrokeType.FirstPlace;
                    IsConfirmed = doc.IsConfirmed;
                    UserID = doc.UserID;
                    ShiftID = doc.ShiftID;
                    foreach (var brokeProduct in doc.DocBroke.DocBrokeProducts)
                    {
                        AddProduct(brokeProduct.ProductID, DocId, BrokeProducts, DocBrokeDecision.BrokeDecisionProducts, false);
                    }
                }
                else
                {
                    Date = DB.CurrentDateTime;
                    IsInFuturePeriod = isInFuturePeriod;
                    //SelectedInFuturePeriod = isInFuturePeriod ? FuturePeriodDocBrokeType.NextPlace : FuturePeriodDocBrokeType.FirstPlace;
                    UserID = WorkSession.UserID;
                    ShiftID = WorkSession.ShiftID;
                   // if (DiscoverPlaces.Select(dp => dp.PlaceID).Contains(WorkSession.PlaceID))
                   // {
                   //     PlaceDiscoverId = DiscoverPlaces.First(dp => dp.PlaceID == WorkSession.PlaceID).PlaceGuid;
                   // }
                    /*var number =
                        gammaBase.Docs.Where(d => d.DocTypeID == (int)DocTypes.DocBroke && d.Number != null)
                            .OrderByDescending(d => d.Date)
                            .FirstOrDefault();
                    try
                    {
                        DocNumber = (Convert.ToInt32(number) + 1).ToString();
                    }
                    catch
                    {
                        DocNumber = "1";
                    }*/
                }
                if (productId != null)
                {
                    AddProduct((Guid)productId, DocId, BrokeProducts, DocBrokeDecision.BrokeDecisionProducts);
                }

                RefreshRejectionReasonsList();
                RefreshProductStateList();

                AddProductCommand = new DelegateCommand(ChooseProductToAdd, () => !IsReadOnly);
                DeleteProductCommand = new DelegateCommand(DeleteBrokeProduct, () => !IsReadOnly && SelectedBrokeProduct != null);
                EditRejectionReasonsCommand = new DelegateCommand(EditRejectionReasons, () => !IsReadOnly);
                EditBrokePlaceCommand = new DelegateCommand(EditBrokePlace, () => !IsReadOnly);
                EditPlaceCommand = new DelegateCommand(EditPlace, () => !IsReadOnly);
                OpenProductCommand = new DelegateCommand(OpenProduct);
                UploadTo1CCommand = new DelegateCommand(UploadTo1C, () => DocId != null);
                UnpackGroupPackCommand = new DelegateCommand(ChooseGroupPackToUnpack, () => !IsReadOnly && SelectedBrokeProduct != null && SelectedBrokeProduct.ProductKind == ProductKind.ProductGroupPack);
                var IsEditableCollection = new ObservableCollection<bool?>
                (
                    from pt in GammaBase.GetDocBrokeEditable(Date, UserID, (int?)ShiftID, (bool)(doc?.IsConfirmed ?? false), WorkSession.UserID, (int)WorkSession.ShiftID, doc?.DocID)
                    select pt
                );
#if (DEBUG)
                IsEditable = true;
#else
                IsEditable = (IsEditableCollection.Count > 0) ? (bool)IsEditableCollection[0] : false;
#endif
                //IsReadOnly = (doc?.IsConfirmed ?? false) || !DB.HaveWriteAccess("DocBroke");
                IsReadOnly = (!IsEditable || !DB.HaveWriteAccess("DocBroke"));
            }
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
            SetRejectionReasonForAllProductCommand = new DelegateCommand(SetRejectionReasonForAllProduct, () => !IsReadOnly && SelectedTabIndex != 1 && ForAllProductRejectionReasonID?.RejectionReasonID != null && ForAllProductRejectionReasonID?.RejectionReasonID != Guid.Empty && ForAllProductRejectionReasonComment != null && ForAllProductRejectionReasonComment != String.Empty);
            SetDecisionForAllProductCommand = new DelegateCommand(SetDecisionForAllProduct, () => !IsReadOnly && SelectedTabIndex != 0 && !DocBrokeDecision.BrokeDecisionProducts.Any(p => p.ProductState != ProductState.NeedsDecision));
        }

        public ICommand ClosingCommand { get; private set; }

        void Closing(CancelEventArgs e)
        {
            if (SelectedTabIndex == 1)
                DocBrokeDecision?.SaveToModel();
            else if (SelectedTabIndex == 0 && IsChanged)
            {
                if (Functions.ShowMessageQuestion("Закрытие Акта о браке: " + Environment.NewLine + "Есть несохраненные данные! Нажмите Да, чтобы сохранить, или Нет, чтобы закрыть без сохранения?", "QUEST ClosingDocBroke docId = '" + DocId + "'", DocId)
                    == MessageBoxResult.Yes)
                {
                    SaveToModel();
                    DocBrokeDecision?.SaveToModel();
                }
                return;
            }
            
            
            //if (MessageBox.Show("Close?", "", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.No)
            //{
            //    e.Cancel = true;
            //}
        }
        
        private void BarcodeReceived(BarcodeMessage msg)
        {
            switch (SelectedTabIndex)
            {
                case 0:
                    if (BrokeProducts?.Count > 0)
                    {
                        var selectedBrokeProduct = BrokeProducts.Where(b => GammaBase.Products.Any(p => p.BarCode == msg.Barcode && p.ProductID == b.ProductId)).FirstOrDefault();
                        if (selectedBrokeProduct == null)
                        {
                            MessageBox.Show("Продукт со штрихкодом " + msg.Barcode + " в 'Несоответствующей продукции' не найден");
                            return;
                        }
                        SelectedBrokeProduct = selectedBrokeProduct;
                    }

                    break;
                case 1:
                    if (DocBrokeDecision.BrokeDecisionProducts?.Count > 0)
                    {
                        var selectedBrokeDecisionProduct = DocBrokeDecision.BrokeDecisionProducts.Where(b => GammaBase.Products.Any(p => p.BarCode == msg.Barcode && p.ProductID == b.ProductId)).FirstOrDefault();
                        if (selectedBrokeDecisionProduct == null)
                        {
                            MessageBox.Show("Продукт со штрихкодом " + msg.Barcode + " в 'Решениях' не найден");
                            return;
                        }
                        DocBrokeDecision.SelectedBrokeDecisionProduct = selectedBrokeDecisionProduct;
                    }
                    break;
            }
        }

        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != VMID) return;
            if (!IsValid)
            {
                MessageBox.Show("Не заполнены некоторые обязательные поля!", "Поля не заполнены", MessageBoxButton.OK,
                    MessageBoxImage.Asterisk);
                return;
            }
            if(CanSaveExecute())
                SaveToModel();
            ReportManager.PrintReport(msg.ReportID, DocId);
        }


        /// <summary>
        /// Добавление продукта к списку продукции акта
        /// </summary>
        /// <param name="productId">ID продукта</param>
        /// <param name="docId">ID документа акта о браке</param>
        /// <param name="brokeProducts">Список продукции</param>
        /// <param name="brokeDecisionProducts">Список решений по продукции</param>
        private void AddProduct(Guid productId, Guid docId, ICollection<BrokeProduct> brokeProducts, ItemsChangeObservableCollection<BrokeDecisionProduct> brokeDecisionProducts, bool isChanged = true)
        {
            DB.AddLogMessageInformation("Добавление продукта ProductID в Акт о браке DocID", "AddProduct (docId = '" + docId + "', productId='" + productId+"')", docId, productId);
            using (var gammaBase = DB.GammaDb)
            {
                if (BrokeProducts.Select(bp => bp.ProductId).Contains(productId)) return;
                var product = gammaBase.vProductsInfo
                    .FirstOrDefault(p => p.ProductID == productId);
                if (product == null) return;
                if (!IsInFuturePeriod && BrokeProducts.Count > 0 &&
                    BrokeProducts.Select(bp => bp.ProductionPlaceId).First() != product.PlaceID)
                {
                    Functions.ShowMessageError("Распаковка ГУ в Акт о браке: " + Environment.NewLine + "Нельзя добавлять продукт другого передела в акт '25к - I передел'", "AddProduct (docId = '" + docId + ", ProductId='" + productId + "')", docId, productId);
                    return;
                }
#region AddBrokeProduct
                var docBrokeProductInfo =
                        gammaBase.DocBrokeProducts.Include(d => d.DocBrokeProductRejectionReasons)
                        .FirstOrDefault(d => d.DocID == docId && d.ProductID == productId);
                var brokeProduct = new BrokeProduct(docBrokeProductInfo?.C1CRejectionReasonID, docBrokeProductInfo?.C1CSecondRejectionReasonID, docBrokeProductInfo?.RejectionReasonComment, docBrokeProductInfo?.PlaceID, docBrokeProductInfo?.BrokePlaceID, docBrokeProductInfo?.BrokeShiftID, docBrokeProductInfo?.BrokePrintName)
                /*new BrokeProduct(docBrokeProductInfo == null ? new ItemsChangeObservableCollection<RejectionReason>() :
                    new ItemsChangeObservableCollection<RejectionReason>(docBrokeProductInfo.DocBrokeProductRejectionReasons
                    .Select(d => new RejectionReason()
                    {
                        RejectionReasonID = d.C1CRejectionReasonID,
                        Comment = d.Comment
                    })), docBrokeProductInfo?.PlaceID , docBrokeProductInfo?.BrokePlaceID, docBrokeProductInfo?.BrokeShiftID, docBrokeProductInfo?.BrokePrintName)*/
                {
                    Date = product.Date,
                    NomenclatureName = DB.GetProductNomenclatureNameBeforeDate(product.ProductID, Date),
                    Number = product.Number,
                    Place = product.Place,
                    ProductionPlaceId = (int)product.PlaceID,
                    ProductionPrintName = product.PrintName,
                    ShiftId = product.ShiftID,
                    BaseMeasureUnit = product.BaseMeasureUnit,
                    ProductId = product.ProductID,
                    ProductKind = (ProductKind)product.ProductKindID,
                    Quantity = docBrokeProductInfo == null ? product.BaseMeasureUnitQuantity ?? 0 : docBrokeProductInfo.Quantity ?? 0,
                    PlaceId = docBrokeProductInfo == null ? product.CurrentPlaceID : docBrokeProductInfo.PlaceID,
                    IsChanged = IsChanged
                };
                if (brokeProduct.BrokePlaceId == brokeProduct.ProductionPlaceId && brokeProduct.PrintName == null)
                    brokeProduct.PrintName = brokeProduct.ProductionPrintName;
                brokeProducts.Add(brokeProduct);
#endregion AddBrokeProduct
#region AddBrokeDecisionProduct
                DocBrokeDecision.AddBrokeDecisionProduct(docId, productId, Date, brokeProduct.Quantity, brokeProduct.RejectionReasonID, brokeProduct.PlaceId, isChanged);
                /*var docBrokeDecisionProducts = gammaBase.DocBrokeDecisionProducts.Where(d => d.DocID == docId && d.ProductID == productId).ToList();
                if (docBrokeDecisionProducts.Count == 0)
                {
                    brokeDecisionProducts.Add(new BrokeDecisionProduct(
                        product.ProductID,
                        (ProductKind)product.ProductKindID,
                        product.Number,
                        ProductState.NeedsDecision,
                        DB.GetProductNomenclatureNameBeforeDate(product.ProductID, Date),
                        product.BaseMeasureUnit,
                        product.C1CNomenclatureID,
                        product.C1CCharacteristicID,
                        product.BaseMeasureUnitQuantity ?? 0
                        )
                    { DocId = this.DocId}
                    );
                }
                else
                {
                    foreach (var decisionProduct in docBrokeDecisionProducts.OrderBy(d =>d.DocID).OrderBy(d => d.ProductID).OrderByDescending(d => d.StateID == (byte)ProductState.ForConversion || d.StateID == (byte)ProductState.Repack))
                    {
                        List<KeyValuePair<Guid, String>> docWithdrawals = new List<KeyValuePair<Guid, string>>();
                        decimal docWithdrawalSum = 0;
                        foreach (var item in decisionProduct.DocBrokeDecisionProductWithdrawalProducts)
                        {
                            var productionProduct = item.DocWithdrawal.DocProduction.FirstOrDefault()?.DocProductionProducts.FirstOrDefault();
                            Guid? itemID = null;
                            string itemName = "";
                            decimal? itemSum = null;
                            if (decisionProduct.StateID == (byte)ProductState.Broke)
                            {
                                itemID = item.DocWithdrawalID;
                                itemSum = item.DocWithdrawal.DocWithdrawalProducts.Where(d => d.ProductID == decisionProduct.ProductID).Sum(d => d.Quantity);
                                itemName = "Списание " + item.DocWithdrawal.Docs.Number + " от " + item.DocWithdrawal.Docs.Date.ToString("dd.MM.yyyy HH.mm") + " на " + itemSum;
                            }
                            else if (productionProduct != null && (decisionProduct.StateID == (byte)ProductState.ForConversion || decisionProduct.StateID == (byte)ProductState.Repack))
                            {
                                itemID = productionProduct.ProductID;
                                itemSum = productionProduct.Quantity;
                                itemName = "Продукт " + productionProduct.Products.Number + " на " + itemSum;
                            }
                            if (itemID != null && itemID != Guid.Empty)
                            {
                                docWithdrawals.Add(new KeyValuePair<Guid, string>((Guid)itemID, itemName));
                                docWithdrawalSum += (decimal)itemSum;
                            } 
                        }
                        var addItem = new BrokeDecisionProduct(
                                decisionProduct.ProductID,
                                (ProductKind)product.ProductKindID,
                                product.Number,
                                (ProductState)decisionProduct.StateID,
                                DB.GetProductNomenclatureNameBeforeDate(product.ProductID, Date),
                                product.BaseMeasureUnit,
                                product.C1CNomenclatureID,
                                product.C1CCharacteristicID,
                                decisionProduct.Quantity ?? 0,
                                decisionProduct.DecisionApplied,
                                //(docWithdrawalID?.DocWithdrawalID == Guid.Empty ? null : docWithdrawalID?.DocWithdrawalID),
                                docWithdrawals,
                                decisionProduct.DecisionDate,
                                decisionProduct.DecisionPlaceID
                            )
                        {
                            DocId = decisionProduct.DocID,
                            Comment = decisionProduct.Comment,
                            NomenclatureId = decisionProduct.C1CNomenclatureID,
                            CharacteristicId = decisionProduct.C1CCharacteristicID,
                            ProductKind = (ProductKind)product.ProductKindID,
                            DecisionPlaceName = gammaBase.Places.FirstOrDefault(p => p.PlaceID == decisionProduct.DecisionPlaceID)?.Name,
                            DocWithdrawalSum = docWithdrawalSum
                        };
                        var brokeRepackOrConv = brokeDecisionProducts.FirstOrDefault(p => p.DocId == addItem.DocId && p.ProductId == addItem.ProductId && (p.ProductState == ProductState.ForConversion || p.ProductState == ProductState.Repack));
                        if (addItem.ProductState == ProductState.Good && brokeRepackOrConv != null && brokeRepackOrConv?.DocWithdrawalSum == addItem.Quantity)
                        {
                            if (brokeRepackOrConv.ProductState == ProductState.ForConversion)
                                addItem.Decision = "Переделано";
                            else if (brokeRepackOrConv.ProductState == ProductState.Repack)
                                addItem.Decision = "Переупаковано";
                        }
                        brokeDecisionProducts.Add(addItem);
                    }
                }*/
                #endregion AddBrokeDecisionProduct
                RefreshRejectionReasonsList();
                RefreshProductStateList();
            }
        }

        private void ChooseProductToAdd()
        {
            DB.AddLogMessageInformation("Выбрано Добавить продукт в Акт о браке DocID","ChooseProductToAdd docId = '" + DocId+"'", DocId);
            Messenger.Default.Register<ChoosenProductMessage>(this, AddChoosenProduct);
            MessageManager.OpenFindProduct(ProductKind.ProductSpool - 1, true, null, true, true, false);
        }

        private void AddChoosenProduct(ChoosenProductMessage msg)
        {
            IsChanged = true;
            if (msg.ProductIDs == null || msg.ProductIDs?.Count == 0)
            {
                SetProductIsChanged(msg.ProductID,true);
                AddProduct(msg.ProductID, DocId, BrokeProducts, DocBrokeDecision.BrokeDecisionProducts);
            }
            else
            {
                foreach (var product in msg.ProductIDs)
                {
                    SetProductIsChanged(product,true);
                    AddProduct(product, DocId, BrokeProducts, DocBrokeDecision.BrokeDecisionProducts);
                }
            }
            Messenger.Default.Unregister<ChoosenProductMessage>(this);
        }

        private void ChooseGroupPackToUnpack()
        {
            DB.AddLogMessageInformation("Распаковка ГУ ProductID в Акте о браке DocID","ChooseGroupPackToUnpack (docId='" + DocId + ", ProductId='" + SelectedBrokeProduct?.ProductId+"')",DocId, SelectedBrokeProduct?.ProductId);
            if (SelectedBrokeProduct == null || SelectedBrokeProduct.ProductKind != ProductKind.ProductGroupPack) return;
                if (!GammaBase.Rests.Any(r => r.ProductID == SelectedBrokeProduct.ProductId))
                {
                    Functions.ShowMessageError("Распаковка ГУ в Акт о браке: "+Environment.NewLine+"Данная упаковка не числится на остатках", "ERROR ChooseGroupPackToUnpack docId = '" + DocId + ", ProductId='" + SelectedBrokeProduct?.ProductId + "'", DocId, SelectedBrokeProduct?.ProductId);
                return;
                }
                if (Functions.ShowMessageQuestion("Распаковка ГУ в Акт о браке: " + Environment.NewLine + "Вы уверены, что хотите распаковать данную упаковку?", "QUEST ChooseGroupPackToUnpack docId = '" + DocId + ", ProductId='" + SelectedBrokeProduct?.ProductId + "'", DocId, SelectedBrokeProduct?.ProductId)
                        //MessageBox.Show("Вы уверены, что хотите распаковать данную упаковку?", "Распаковка", MessageBoxButton.YesNo, MessageBoxImage.Question) 
                    != MessageBoxResult.Yes)
            {
                return;
            }
            IsChanged = true;
            UIServices.SetBusyState();
            var docProductionID = GammaBase.DocProductionProducts.FirstOrDefault(p => p.ProductID == SelectedBrokeProduct.ProductId)?.DocID;
            var groupPackProductIDs = GammaBase.DocWithdrawalProducts.Where(p => p.DocWithdrawal.DocProduction.Any(dp => dp.DocID == docProductionID)).Select(p => p.ProductID).ToList();
            GammaBase.UnpackGroupPack(SelectedBrokeProduct.ProductId, WorkSession.PrintName);
            DB.AddLogMessageInformation("Групповая упаковка ProductID уничтожена, рулоны возвращены на остатки", "ChooseGroupPackToUnpack.UnpackGroupPack (docId='" + DocId + ", ProductId='" + SelectedBrokeProduct?.ProductId + "')", DocId, SelectedBrokeProduct?.ProductId);

            var selectedBrokeDecisionProduct = DocBrokeDecision.BrokeDecisionProducts.FirstOrDefault(p => p.ProductId == SelectedBrokeProduct.ProductId);
            foreach (var product in groupPackProductIDs)
            {
                SetProductIsChanged(product,true);
                AddProduct(product, DocId, BrokeProducts, DocBrokeDecision.BrokeDecisionProducts);
                var addedBrokeProduct = BrokeProducts.FirstOrDefault(p => p.ProductId == product);
                addedBrokeProduct.BrokePlaceId = SelectedBrokeProduct.BrokePlaceId;
                addedBrokeProduct.BrokeShiftId = SelectedBrokeProduct.BrokeShiftId;
                addedBrokeProduct.PrintName = SelectedBrokeProduct.PrintName;
                addedBrokeProduct.PlaceId = SelectedBrokeProduct.PlaceId;
                addedBrokeProduct.RejectionReasonID = SelectedBrokeProduct.RejectionReasonID;
                addedBrokeProduct.SecondRejectionReasonID = SelectedBrokeProduct.SecondRejectionReasonID;
                addedBrokeProduct.RejectionReasonComment = SelectedBrokeProduct.RejectionReasonComment;
                //исходим из предположения, что по ГУ можно принять только одно решение, причем на весь обьем и только из блока Требует решения
                var addedBrokeDecisionProduct = DocBrokeDecision.BrokeDecisionProducts.FirstOrDefault(p => p.ProductId == product);
                addedBrokeDecisionProduct.ProductState = selectedBrokeDecisionProduct.ProductState;
                addedBrokeDecisionProduct.Comment = selectedBrokeDecisionProduct.Comment;
                addedBrokeDecisionProduct.Quantity = selectedBrokeDecisionProduct.ProductQuantity;
                addedBrokeDecisionProduct.NomenclatureId = selectedBrokeDecisionProduct.NomenclatureId;
                addedBrokeDecisionProduct.CharacteristicId = selectedBrokeDecisionProduct.CharacteristicId;
                addedBrokeDecisionProduct.RejectionReasonID = SelectedBrokeProduct.RejectionReasonID;
                addedBrokeDecisionProduct.BrokePlaceID = SelectedBrokeProduct.PlaceId;
                //var decisionDate = addedBrokeDecisionProduct?.DecisionDate;
                //var curDate = DB.CurrentDateTime;
                //if (decisionDate != null && curDate != null 
                //    && (1001 - (int)((DateTime)decisionDate - (DateTime)curDate).TotalMilliseconds) > 0
                //    && (1001 - (int)((DateTime)decisionDate - (DateTime)curDate).TotalMilliseconds) <=1000)
                //    Thread.Sleep(1001 - (int)((DateTime)decisionDate - curDate).TotalMilliseconds);
            }

            var decisionProductsToRemove =
                DocBrokeDecision.BrokeDecisionProducts.Where(p => p.ProductId == SelectedBrokeProduct.ProductId).ToList();
            foreach (var product in decisionProductsToRemove)
            {
                DocBrokeDecision.BrokeDecisionProducts.Remove(product);
            }
            BrokeProducts.Remove(SelectedBrokeProduct);
            RefreshRejectionReasonsList();

        }


        public List<Place> DiscoverPlaces { get; set; }
        public List<Place> StorePlaces { get; set; }
        public List<Place> BrokePlaces { get; set; }
        public Dictionary<bool, string> InFuturePeriodList { get; set; }

        private bool? _isConfirmed { get; set; }
        public bool IsConfirmed
        {
            get
            {
                return _isConfirmed == true;
            }
            set
            {
                if (_isConfirmed != null)
                    IsChanged = true;
                _isConfirmed = value;
                RaisePropertiesChanged("IsConfirmed");
            }
        }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public string DocNumber { get; set; }

        private string _docComment { get; set; }
        [UIAuth(UIAuthLevel.ReadOnly)]
        public string DocComment
        {
            get { return _docComment; }
            set
            {
                if (_docComment != null && value != null)
                    IsChanged = true;
                _docComment = value;
                RaisePropertiesChanged("DocComment");
            }
        }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public DateTime Date { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public Guid? UserID { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public byte? ShiftID { get; set; }

       // [UIAuth(UIAuthLevel.ReadOnly)]
       // [Required(ErrorMessage = @"Место обнаружения не может быть пустым")]
       // public Guid? PlaceDiscoverId { get; set; }

        //[UIAuth(UIAuthLevel.ReadOnly)]
        //[Required(ErrorMessage = @"Место хранения не может быть пустым")]
        //public Guid? PlaceStoreId { get; set; }

        private ItemsChangeObservableCollection<BrokeProduct> _brokeProducts = new ItemsChangeObservableCollection<BrokeProduct>();

        public ItemsChangeObservableCollection<BrokeProduct> BrokeProducts
        {
            get { return _brokeProducts; }
            set
            {
                _brokeProducts = value;
                RaisePropertyChanged("BrokeProducts");
            }
        }

        public override bool CanSaveExecute()
        {
            //return IsValid && DB.HaveWriteAccess("DocBroke");
            return IsValid && DB.HaveWriteAccess("DocBroke") && IsEditable;
        }

        private int _selectedTabIndex { get; set; }
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (_selectedTabIndex == 0 && value == 1)
                {
                    if (IsChanged && CanSaveExecute())
                    {
                        if (!SaveToModel())
                            return;
                    }
                    if (DocBrokeDecision.GetService_SetBrokePlaceDialog == null)
                    {
                        var dialogService = GetService<IDialogService>("SetBrokePlaceDialog");
                        DocBrokeDecision.GetService_SetBrokePlaceDialog = dialogService;
                    }
                }
                if (_selectedTabIndex == 1 && value == 0)
                {
                    if (DocBrokeDecision?.SelectedBrokeDecisionProduct != null)
                    {
                        DocBrokeDecision.SelectedBrokeDecisionProduct = null;
                    }
                    
                }
                /*foreach (var productItem in BrokeProducts)
                {
                    foreach (var decisionItem in DocBrokeDecision?.BrokeDecisionProducts?.Where(d => d.ProductId == productItem.ProductId))
                    {
                        decisionItem.RejectionReasonID = productItem.RejectionReasonID;
                        decisionItem.BrokePlaceID = productItem.PlaceId;
                    }
                }*/
                _selectedTabIndex = value;
                IsVisibleSetRejectionReasonForAllProduct = (value == 0);
                IsVisibleSetDecisionForAllProduct = (value == 1);
                RaisePropertyChanged("RejectionReasonsList");

                DB.AddLogMessageInformation("Выбрана вкладка "+(value == 0 ? "Продукция": value == 1 ? "Решение":"") +" в Акте о браке DocID", "SET SelectedTabIndex ='" + value + "')", DocId);
                               

                //Это неправильно, но меняется только здесь
                RaisePropertyChanged("IsVisibleSetRejectionReasonForAllProduct");
                RaisePropertyChanged("IsVisibleSetDecisionForAllProduct");
                //
            }
        }

        public List<KeyValuePair<byte, string>> NeedsDecisionStateList { get; private set; } = new List<KeyValuePair<byte, string>>();

        public bool IsVisibleSetRejectionReasonForAllProduct { get; private set; }
        public bool IsVisibleSetDecisionForAllProduct { get; private set; }

        public DelegateCommand SetRejectionReasonForAllProductCommand { get; private set; }

        public string ForAllProductRejectionReasonComment { get; set; }

        public RejectionReason ForAllProductRejectionReasonID { get; set; }
        private List<RejectionReason> _rejectionReasonsList { get; set; }
        public List<RejectionReason> RejectionReasonsList
        {
            get { return _rejectionReasonsList; }
            set
            {
                _rejectionReasonsList = value;
                RaisePropertyChanged("RejectionReasonsList");
            }
        }

        private void RefreshRejectionReasonsList()
        {
            if (BrokeProducts?.Count > 0)
            {
                if (BrokeProducts.Select(p => p.ProductKind).Distinct().Count() == 1 ||
                    (BrokeProducts.Select(p => p.ProductKind).Distinct().Count() == 2 && BrokeProducts.Any(p => p.ProductKind == ProductKind.ProductPallet) && BrokeProducts.Any(p => p.ProductKind == ProductKind.ProductPalletR)))
                {
                    if ((RejectionReasonsList == null || RejectionReasonsList?.Count == 0))
                    {
                        var productKind = (int)BrokeProducts.Select(p => p.ProductKind).FirstOrDefault();
                        RejectionReasonsList = new List<RejectionReason>(GammaBase.C1CRejectionReasons
                            .Where(r => (!r.IsFolder ?? true) && (!r.IsMarked ?? true) && (r.ParentID == null || (r.ParentID != null
                            && GammaBase.ProductKinds.FirstOrDefault(pk => pk.ProductKindID == productKind).C1CRejectionReasons.Select(rr => rr.C1CRejectionReasonID).Contains((Guid)r.ParentID))))
                            .Select(r => new RejectionReason
                            {
                                RejectionReasonID = r.C1CRejectionReasonID,
                                Description = r.Description,
                                FullDescription = r.FullDescription
                            }).OrderBy(r => r.Description));
                    }
                }
                else
                {
                    RejectionReasonsList = new List<RejectionReason>();
                }
            }
            else
            {
                RejectionReasonsList = new List<RejectionReason>();
            }
        }

        private void SetRejectionReasonForAllProduct()
        {
            if (BrokeProducts != null)
            {
                DB.AddLogMessageInformation("Установлен для всех дефект '" + ForAllProductRejectionReasonID.Description+"' и причина '"+ ForAllProductRejectionReasonComment + "' в Акте о браке DocID", "SetRejectionReasonForAllProduct (RejectionReasonID='" + ForAllProductRejectionReasonID.RejectionReasonID + "', RejectionReasonComment" + ForAllProductRejectionReasonComment + "')", DocId);
                foreach (var brokeProduct in BrokeProducts)
                {
                    /*brokeProduct.RejectionReasons.Clear();
                    brokeProduct.RejectionReasons.Add(new RejectionReason()
                    {
                        RejectionReasonID = ForAllProductRejectionReasonID.RejectionReasonID,
                        Comment = ForAllProductRejectionReasonComment
                    });*/
                    brokeProduct.RejectionReasonID = ForAllProductRejectionReasonID.RejectionReasonID;
                    brokeProduct.RejectionReasonComment = ForAllProductRejectionReasonComment;
                }
            }
        }

        public DelegateCommand SetDecisionForAllProductCommand { get; private set; }
        //public string ForAllProductDecisionComment { get; set; }
        //public KeyValuePair<int, string> ForAllProductStateID { get; set; }
        //private List<KeyValuePair<int,string>> _productStateList { get; set; }
        //public List<KeyValuePair<int, string>> ProductStateList
        //{
        //    get { return _productStateList; }
        //    set
        //    {
        //        _productStateList = value;
        //        RaisePropertyChanged("ProductStateList");
        //    }
        //}

        public BrokeDecisionForAllProducts ForAllProductsProkeDecision { get; private set; } = new BrokeDecisionForAllProducts();

        private void RefreshProductStateList()
        {
            if (DocBrokeDecision.BrokeDecisionProducts?.Count > 0)
            {
                ForAllProductsProkeDecision.RefreshProductStateList();
            }

            /*if (ProductStateList == null)
                ProductStateList = new List<KeyValuePair<int, string>>();
            if (BrokeDecisionProducts?.Count > 0)
            {
                //if (BrokeDecisionProducts.Select(p => p.ProductKind).Distinct().Count() == 1 ||
                //    (BrokeDecisionProducts.Select(p => p.ProductKind).Distinct().Count() == 2 && BrokeDecisionProducts.Any(p => p.ProductKind == ProductKind.ProductPallet) && BrokeDecisionProducts.Any(p => p.ProductKind == ProductKind.ProductPalletR)))
                {
                    if (ProductStateList?.Count == 0)
                    {
                        var productStateList = GammaBase.ProductStates
                            .Where(r => r.StateID != (int)ProductState.NeedsDecision && r.StateID != (int)ProductState.ForConversion && (WorkSession.PlaceGroup == PlaceGroup.Other || (WorkSession.PlaceGroup != PlaceGroup.Other && r.StateID != (int)ProductState.InternalUsage && r.StateID != (int)ProductState.Limited && r.StateID != (int)ProductState.ForConversion)))
                            .OrderBy(r => r.StateID);
                        foreach (var item in productStateList)
                        {
                            ProductStateList.Add(new KeyValuePair<int, string> ( item.StateID, item.Name ));
                        }
                    }
                }
                //    else
                //    {
                //        ProductStateList = new List<KeyValuePair<int, string>>();
                //    }
            }
            //else
            //{
            //    ProductStateList = new List<KeyValuePair<int, string>>();
            //}*/
        }

        private void SetDecisionForAllProduct()
        {
            DocBrokeDecision.SetDecisionForAllProduct(ForAllProductsProkeDecision);
/*            if (BrokeDecisionProducts != null)
            {
                var selectedBrokeDecisionProduct = SelectedBrokeDecisionProduct;
                foreach (var brokeDecisionProduct in BrokeDecisionProducts)
                {

                    brokeDecisionProduct.ProductState = (ProductState)ForAllProductsProkeDecision.ProductStateID.Key;
                    brokeDecisionProduct.Comment = ForAllProductsProkeDecision.Comment;
                    brokeDecisionProduct.Quantity = BrokeProducts.Where(p => p.ProductId == brokeDecisionProduct.ProductId).Select(p => p.Quantity).First();
                    if (ForAllProductsProkeDecision.ProductStateID.Key == (int)ProductState.ForConversion)
                    {
                        brokeDecisionProduct.NomenclatureId = ForAllProductsProkeDecision.NomenclatureID;
                        brokeDecisionProduct.CharacteristicId = ForAllProductsProkeDecision.CharacteristicID;
                    }
                }
                SelectedBrokeDecisionProduct = null;
                SelectedBrokeDecisionProduct = BrokeDecisionProducts.Where(p => p.ProductId == selectedBrokeDecisionProduct?.ProductId).FirstOrDefault();
            }*/
        }

        public DelegateCommand AddProductCommand { get; private set; }
        public DelegateCommand DeleteProductCommand { get; private set; }
        public DelegateCommand UnpackGroupPackCommand { get; private set; }

        private void DeleteBrokeProduct()
        {
            if (SelectedBrokeProduct == null) return;
            DB.AddLogMessageInformation("Удаление продукта ProductID в Акт о браке DocID", "DeleteBrokeProduct (docId = '" + DocId + "', productId='" + SelectedBrokeProduct?.ProductId + "')", DocId, SelectedBrokeProduct?.ProductId);
            IsChanged = true;
            var decisionProductsToRemove =
                DocBrokeDecision.BrokeDecisionProducts.Where(p => p.ProductId == SelectedBrokeProduct.ProductId).ToList();
            foreach (var product in decisionProductsToRemove)
            {
                DocBrokeDecision.BrokeDecisionProducts.Remove(product);
            }
            BrokeProducts.Remove(SelectedBrokeProduct);
            RefreshRejectionReasonsList();
        }

        private bool? _isInFuturePeriod { get; set; }
        public bool IsInFuturePeriod
        {
            get
            {
                return _isInFuturePeriod == true;
            }
            set
            {
                if (_isInFuturePeriod == true && !value && BrokeProducts.Count > 0 &&
                    BrokeProducts.Select(bp => bp.ProductionPlaceId).Distinct().Count() > 1)
                {
                    Functions.ShowMessageError("Изменение в Акт о браке: " + Environment.NewLine + "Нельзя изменить акт на '25к - I передел', так как в акте продукты других переделов", "ERROR SET IsInFuturePeriod", DocId);
                    return;
                }
                if (_isInFuturePeriod != null)
                    IsChanged = true;
                _isInFuturePeriod = value;
                DB.AddLogMessageInformation("Изменение признака Передел на '"+ InFuturePeriodList[value] + "' в Акт о браке DocID", "SET IsInFuturePeriod (IsInFuturePeriod='" + value + "')", DocId);
                RaisePropertiesChanged("IsInFuturePeriod");
            }
        }

        public Guid DocId { get; private set; }

        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();

        public Guid? VMID { get; } = Guid.NewGuid();

        public bool IsReadOnly { get; }
        
        public bool IsEditable { get; }

        private void OpenProduct()
        {
            if (SelectedBrokeProduct == null) return;
            MessageManager.OpenDocProduct(SelectedBrokeProduct.ProductKind, SelectedBrokeProduct.ProductId);
        }

        public DelegateCommand OpenProductCommand { get; private set; }
        public DelegateCommand EditRejectionReasonsCommand { get; private set; }
        public DelegateCommand EditBrokePlaceCommand { get; private set; }
        public DelegateCommand EditPlaceCommand { get; private set; }
        public DelegateCommand UploadTo1CCommand { get; private set; }

        public bool IsChanged { get; set; } = false;
        public void SetProductIsChanged(Guid productID, bool isChanged)
        {
            if (isChanged)
                IsChanged = true;
            foreach (var product in BrokeProducts?.Where(p => p.ProductId == productID))
            {
                product.IsChanged = isChanged;
            }
        }

        private void DebugFunc()
        {
            Debug.Print("Кол-во задано");
        }

        private void EditRejectionReasons()
        {
            //if (SelectedBrokeProduct?.RejectionReasons == null) return;
            //MessageManager.EditRejectionReasons(SelectedBrokeProduct);
            if (SelectedBrokeProduct == null) return;
            var model = new AddRejectionReasonDialogModel(SelectedBrokeProduct.ProductKind,SelectedBrokeProduct.RejectionReasonID, SelectedBrokeProduct.SecondRejectionReasonID, SelectedBrokeProduct.RejectionReasonComment);
            var deleteCommand = new UICommand()
            {
                Caption = "Удалить",
                IsCancel = false,
                IsDefault = false,
                Command = new DelegateCommand<CancelEventArgs>(
            x => DebugFunc(),
            x => model.IsSaveEnabled),
            };
            var okCommand = new UICommand()
            {
                Caption = "Сохранить",
                IsCancel = false,
                IsDefault = true,
                Command = new DelegateCommand<CancelEventArgs>(
             x => DebugFunc(),
             x => model.IsSaveEnabled),
            };
            var cancelCommand = new UICommand()
            {
                Id = MessageBoxResult.Cancel,
                Caption = "Отмена",
                IsCancel = true,
                IsDefault = false,
            };
            var dialogService = GetService<IDialogService>("AddRejectionReasonDialog");
            var result = dialogService.ShowDialog(
                dialogCommands: new List<UICommand>() { deleteCommand, okCommand, cancelCommand },
                title: "Добавление дефектов",
                viewModel: model);
            if (result == deleteCommand)
            {
                SetProductIsChanged(SelectedBrokeProduct.ProductId,true);
                SelectedBrokeProduct.RejectionReasonID = null;
                SelectedBrokeProduct.SecondRejectionReasonID = null;
                SelectedBrokeProduct.RejectionReasonComment = null;
                SelectedBrokeProduct.RejectionReasonName = null;
                SelectedBrokeProduct.SecondRejectionReasonName = null;
                DB.AddLogMessageInformation("Удаление дефекта ProductID в Акт о браке DocID" + Environment.NewLine + "Дефект='" + model.RejectionReasonName + (model.SecondRejectionReasonID != null ? "', Дефект(доп)='" + model.SecondRejectionReasonName : "") + "', Причина='" + model.Comment + "'", "SetRejectionReason (RejectionReasonID='" + model.RejectionReasonID + "', SecondRejectionReasonID='" + model.SecondRejectionReasonID + "', Comment='" + model.Comment + "')", DocId, SelectedBrokeProduct?.ProductId);
            }
            else if (result == okCommand)
            {
                SetProductIsChanged(SelectedBrokeProduct.ProductId,true);
                SelectedBrokeProduct.RejectionReasonID = model.RejectionReasonID;
                SelectedBrokeProduct.SecondRejectionReasonID = model.SecondRejectionReasonID;
                SelectedBrokeProduct.RejectionReasonComment = model.Comment;
                SelectedBrokeProduct.RejectionReasonName = model.RejectionReasonName;
                SelectedBrokeProduct.SecondRejectionReasonName = model.SecondRejectionReasonName;
                DB.AddLogMessageInformation("Изменение дефекта ProductID в Акт о браке DocID" + Environment.NewLine + "Дефект='" + model.RejectionReasonName + (model.SecondRejectionReasonID != null ? "', Дефект(доп)='" + model.SecondRejectionReasonName : "") + "', Причина='" + model.Comment + "'", "SetRejectionReason (RejectionReasonID='" + model.RejectionReasonID + "', SecondRejectionReasonID='" + model.SecondRejectionReasonID + "', Comment='" + model.Comment + "')", DocId, SelectedBrokeProduct?.ProductId);
            }
            foreach (var decisionItem in DocBrokeDecision?.BrokeDecisionProducts?.Where(d => d.ProductId == SelectedBrokeProduct?.ProductId))
            {
                decisionItem.RejectionReasonID = SelectedBrokeProduct.RejectionReasonID;
                decisionItem.BrokePlaceID = SelectedBrokeProduct.PlaceId;
            }
        }

        private void EditBrokePlace()
        {
            //if (SelectedBrokeProduct?.RejectionReasons == null) return;
            //MessageManager.EditRejectionReasons(SelectedBrokeProduct);
            if (SelectedBrokeProduct == null) return;
            var model = new SetBrokePlaceDialogModel(SelectedBrokeProduct.BrokePlaceId, SelectedBrokeProduct.BrokeShiftId, SelectedBrokeProduct.PrintName);
            var deleteCommand = new UICommand()
            {
                Caption = "Удалить",
                IsCancel = false,
                IsDefault = false,
                Command = new DelegateCommand<CancelEventArgs>(
            x => DebugFunc(),
            x => model.IsSaveEnabled),
            };
            var okCommand = new UICommand()
            {
                Caption = "Сохранить",
                IsCancel = false,
                IsDefault = true,
                Command = new DelegateCommand<CancelEventArgs>(
             x => DebugFunc(),
             x => model.IsSaveEnabled),
            };
            var cancelCommand = new UICommand()
            {
                Id = MessageBoxResult.Cancel,
                Caption = "Отмена",
                IsCancel = true,
                IsDefault = false,
            };
            var dialogService = GetService<IDialogService>("SetBrokePlaceDialog");
            var result = dialogService.ShowDialog(
                dialogCommands: new List<UICommand>() { deleteCommand, okCommand, cancelCommand },
                title: "Указание виновника",
                viewModel: model);
            if (result == deleteCommand)
            {
                DB.AddLogMessageInformation("Удаление Виновника ProductID в Акт о браке DocID" + Environment.NewLine + "Передел='" + model.Places.FirstOrDefault(p => p.PlaceID == model.PlaceID)?.PlaceName + "', Смена='" + model.ShiftID + "', ФИО='" + model.Comment + "'", "SetBrokePlace (PlaceID='" + model.PlaceID + "', ShiftID='" + model.ShiftID + "', PrintName='" + model.Comment + "')", DocId, SelectedBrokeProduct?.ProductId);
                SetProductIsChanged(SelectedBrokeProduct.ProductId,true);
                SelectedBrokeProduct.BrokePlaceId = null;
                SelectedBrokeProduct.BrokeShiftId = null;
                SelectedBrokeProduct.PrintName = null;
            }
            else if (result == okCommand)
            {
                DB.AddLogMessageInformation("Изменение Виновника ProductID в Акт о браке DocID" + Environment.NewLine + "Передел='" + model.Places.FirstOrDefault(p => p.PlaceID == model.PlaceID)?.PlaceName + "', Смена='" + model.ShiftID + "', ФИО='" + model.Comment + "'", "SetBrokePlace (PlaceID='" + model.PlaceID + "', ShiftID='" + model.ShiftID + "', PrintName='" + model.Comment + "')", DocId, SelectedBrokeProduct?.ProductId);
                SetProductIsChanged(SelectedBrokeProduct.ProductId,true);
                SelectedBrokeProduct.BrokePlaceId = model.PlaceID;
                SelectedBrokeProduct.BrokeShiftId = model.ShiftID;
                SelectedBrokeProduct.PrintName = model.Comment;
            }
        }

        private void EditPlace()
        {
            //if (SelectedBrokeProduct?.RejectionReasons == null) return;
            //MessageManager.EditRejectionReasons(SelectedBrokeProduct);
            if (SelectedBrokeProduct == null) return;
            var model = new SetBrokePlaceDialogModel(SelectedBrokeProduct.PlaceId);
            var deleteCommand = new UICommand()
            {
                Caption = "Удалить",
                IsCancel = false,
                IsDefault = false,
                Command = new DelegateCommand<CancelEventArgs>(
            x => DebugFunc(),
            x => model.IsSaveEnabled),
            };
            var okCommand = new UICommand()
            {
                Caption = "Сохранить",
                IsCancel = false,
                IsDefault = true,
                Command = new DelegateCommand<CancelEventArgs>(
             x => DebugFunc(),
             x => model.IsSaveEnabled),
            };
            var cancelCommand = new UICommand()
            {
                Id = MessageBoxResult.Cancel,
                Caption = "Отмена",
                IsCancel = true,
                IsDefault = false,
            };
            var dialogService = GetService<IDialogService>("SetBrokePlaceDialog");
            var result = dialogService.ShowDialog(
                dialogCommands: new List<UICommand>() { deleteCommand, okCommand, cancelCommand },
                title: "Указание места актирования",
                viewModel: model);
            if (result == deleteCommand)
            {
                DB.AddLogMessageInformation("Удаление места актирования ProductID в Акт о браке DocID" + Environment.NewLine + "Передел='" + model.Places.FirstOrDefault(p => p.PlaceID == model.PlaceID)?.PlaceName + "'", "SetPlace (PlaceID='" + model.PlaceID + "')", DocId, SelectedBrokeProduct?.ProductId);
                SetProductIsChanged(SelectedBrokeProduct.ProductId,true);
                SelectedBrokeProduct.PlaceId = null;
            }
            else if (result == okCommand)
            {
                DB.AddLogMessageInformation("Изменение места актирования ProductID в Акт о браке DocID" + Environment.NewLine + "Передел='" + model.Places.FirstOrDefault(p => p.PlaceID == model.PlaceID)?.PlaceName + "'", "SetPlace (PlaceID='" + model.PlaceID + "')", DocId, SelectedBrokeProduct?.ProductId);
                SetProductIsChanged(SelectedBrokeProduct.ProductId,true);
                SelectedBrokeProduct.PlaceId = model.PlaceID;
            }
            foreach (var decisionItem in DocBrokeDecision?.BrokeDecisionProducts?.Where(d => d.ProductId == SelectedBrokeProduct?.ProductId))
            {
                decisionItem.RejectionReasonID = SelectedBrokeProduct.RejectionReasonID;
                decisionItem.BrokePlaceID = SelectedBrokeProduct.PlaceId;
            }
        }

        private void UploadTo1C()
        {
            DB.AddLogMessageInformation("Выгрузка в 1С Акт о браке DocID", "UploadDocBrokeTo1C", DocId);
            if (!IsValid)
            {
                Functions.ShowMessageError("Выгрузка в 1С Акт о браке: " + Environment.NewLine + "Не заполнены некоторые обязательные поля!", "ERROR UploadTo1C", DocId);
                return;
            }
            if (CanSaveExecute())
                SaveToModel();
            else
                DB.UploadDocBrokeTo1C(DocId);
        }

        public BrokeProduct SelectedBrokeProduct { get; set; }

        public DocBrokeDecisionViewModel DocBrokeDecision { get; set; }

        public override bool SaveToModel()
        {
            DB.AddLogMessageInformation("Сохранение Акт о браке DocID", "DocBroke.SaveToModel", DocId);
            if (!DB.HaveWriteAccess("DocBroke"))
            {
                DB.AddLogMessageError("Ошибка при сохранении Акт о браке DocID: нет прав на запись", "Error DocBroke.SaveToModel", DocId);
                return false;
            }
            /*if (
                DocBrokeDecision.BrokeDecisionProducts.Any(
                    dp =>
                        dp.ProductState == ProductState.ForConversion &&
                        (dp.NomenclatureId == null || dp.CharacteristicId == null)))
            {
                MessageBox.Show("При решении \"на переделку\" необходимо указать номенклатуру и характеристику");
                return false;
            }*/
            if (
                BrokeProducts.Any(
                    dp =>
                        (dp.RejectionReasonID == null 
                        || dp.RejectionReasonComment == null || dp.RejectionReasonComment.Length == 0)))
                        //(dp.RejectionReasonsString == null || dp.RejectionReasonsString.Length == 0
                        //|| dp.RejectionReasonCommentsString == null || dp.RejectionReasonCommentsString.Length == 0)))
            {
                Functions.ShowMessageError("Ошибка при сохранении Акт о браке: " + Environment.NewLine + "Обязательно требуется заполнить поле Дефекты и Причины несоответствия во всех продуктах", "Error DocBroke.SaveToModel", DocId);
                return false;
            }
            using (var gammaBase = DB.GammaDb)
            {
                var doc = gammaBase.Docs.Include(d => d.DocBroke).Include(d => d.DocBroke.DocBrokeProducts)
                    .Include(d => d.DocBroke.DocBrokeProducts.Select(dp => dp.DocBrokeProductRejectionReasons)).FirstOrDefault(d => d.DocID == DocId && d.DocTypeID == (int)DocTypes.DocBroke);
                if (doc == null)
                {
                    doc = new Docs
                    {
                        DocID = DocId,
                        DocTypeID = (int)DocTypes.DocBroke,
                        Date = Date,
                        PlaceID = WorkSession.PlaceID,
                        ShiftID = WorkSession.ShiftID,
                        UserID = WorkSession.UserID,
                        PrintName = WorkSession.PrintName
                    };
                    gammaBase.Docs.Add(doc);
                    gammaBase.SaveChanges();
                    doc = gammaBase.Docs.Include(d => d.DocBroke).Include(d => d.DocBroke.DocBrokeProducts)
                        .Include(d => d.DocBroke.DocBrokeProducts.Select(dp => dp.DocBrokeProductRejectionReasons)).FirstOrDefault(d => d.DocID == DocId && d.DocTypeID == (int)DocTypes.DocBroke);
                    DocNumber = doc.Number;
                }
                //doc.Number = DocNumber;
                doc.Comment = DocComment;
                doc.IsConfirmed = IsConfirmed;
                if (doc.DocBroke == null)
                    doc.DocBroke = new DocBroke
                    {
                        DocID = DocId,
                        //PlaceDiscoverID = PlaceDiscoverId,
                        //PlaceStoreID = PlaceStoreId,
                        DocBrokeProducts = new List<DocBrokeProducts>(),
                    };
                //doc.DocBroke.PlaceDiscoverID = PlaceDiscoverId;
                //doc.DocBroke.PlaceStoreID = PlaceStoreId;
                doc.DocBroke.IsInFuturePeriod = IsInFuturePeriod;
                foreach (var docBrokeProduct in doc.DocBroke.DocBrokeProducts)
                {
                    if (docBrokeProduct.DocBrokeProductRejectionReasons.Count > 0)
                    {
                        docBrokeProduct.DocBrokeProductRejectionReasons.Clear();
                    }
                }
                doc.DocBroke.DocBrokeProducts.Clear();
                foreach (var docBrokeProduct in BrokeProducts)
                {
                    var brokeProduct = new DocBrokeProducts
                    {
                        ProductID = docBrokeProduct.ProductId,
                        DocID = doc.DocID,
                        Quantity = docBrokeProduct.Quantity,
                        BrokePlaceID = docBrokeProduct.BrokePlaceId,
                        BrokeShiftID = docBrokeProduct.BrokeShiftId,
                        BrokePrintName = docBrokeProduct.PrintName,
                        PlaceID = docBrokeProduct.PlaceId,
                        DocBrokeProductRejectionReasons = new List<DocBrokeProductRejectionReasons>(),
                        C1CRejectionReasonID = docBrokeProduct.RejectionReasonID,
                        C1CSecondRejectionReasonID = docBrokeProduct.SecondRejectionReasonID,
                        RejectionReasonComment = docBrokeProduct.RejectionReasonComment
                    };
                    /*foreach (var reason in docBrokeProduct.RejectionReasons)
                    {
                        brokeProduct.DocBrokeProductRejectionReasons.Add(new DocBrokeProductRejectionReasons
                        {
                            ProductID = brokeProduct.ProductID,
                            DocID = brokeProduct.DocID,
                            C1CRejectionReasonID = reason.RejectionReasonID,
                            Comment = reason.Comment
                        });
                    }*/
                    doc.DocBroke.DocBrokeProducts.Add(brokeProduct);
                    docBrokeProduct.IsChanged = false;
                }
                gammaBase.SaveChanges();
                DB.AddLogMessageInformation("закончено сохранение Акт о браке DocID", "DocBroke.SaveToModel", DocId);

                #region Сохранение решений по продукции
                DocBrokeDecision.SaveToModel();
                /*
                if (doc.DocBroke.DocBrokeDecisionProducts == null)
                    doc.DocBroke.DocBrokeDecisionProducts = new List<DocBrokeDecisionProducts>();
                else
                    doc.DocBroke.DocBrokeDecisionProducts.Clear();
                foreach (var decisionProduct in DocBrokeDecision.BrokeDecisionProducts)
                {
                    doc.DocBroke.DocBrokeDecisionProducts.Add(new DocBrokeDecisionProducts
                    {
                        C1CCharacteristicID = decisionProduct.CharacteristicId,
                        C1CNomenclatureID = decisionProduct.NomenclatureId,
                        Quantity = decisionProduct.Quantity,
                        ProductID = decisionProduct.ProductId,
                        DocID = DocId,
                        StateID = (byte)decisionProduct.ProductState,
                        Comment = decisionProduct.Comment,
                        DecisionApplied = decisionProduct.DecisionApplied,
                        DecisionDate = decisionProduct.DecisionDate,
                        DecisionPlaceID = decisionProduct.DecisionPlaceId
                    });
                }*/
#endregion
                
            }
            IsChanged = false;

            if (WorkSession.IsUploadDocBrokeTo1CWhenSave)
                DB.UploadDocBrokeTo1C(DocId);
            else
                DB.AddLogMessageError("Выгрузка в 1С Акт о браке: " + Environment.NewLine + "Документ не выгружен", "DocBroke.SaveToModel NOT UploadTo1C because IsUploadDocBrokeTo1CWhenSave=false", DocId);
            Messenger.Default.Send(new RefreshBrokeListMessage { DocID = DocId });
            return true;
        }

        //public IDialogService GetService(string dialogName) => GetService<IDialogService>(dialogName);
        public IDialogService GetService_SetBrokePlaceDialog => GetService<IDialogService>("SetBrokePlaceDialog");

    }
}
