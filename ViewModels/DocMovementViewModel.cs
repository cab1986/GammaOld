// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Interfaces;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class DocMovementViewModel : SaveImplementedViewModel, IBarImplemented
    {
        public DocMovementViewModel(Guid docMovementId)
        {
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
            DocMovementId = docMovementId;
            //IsInitialize = true;
            using (var gammaBase = DB.GammaDb)
            {
                /*
                Warehouses = gammaBase.Places.Where(p => p.IsWarehouse ?? false).Select(p => new Place
                {
                    PlaceID = p.PlaceID,
                    PlaceGuid = p.PlaceGuid,
                    PlaceName = p.Name
                }).ToList();
                */
                var docMovement =
                    gammaBase.DocMovement.Include(d => d.Docs)
                    .FirstOrDefault(d => d.DocID == docMovementId);
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
                IsInVisible = docMovement.OrderTypeID != (int) OrderType.ShipmentOrer;
                IsConfirmed = docMovement.Docs.IsConfirmed;
                DocOrderId = docMovement.DocOrderID;
                DenyEditIn = InPlaceId != null && !WorkSession.PlaceIds.Contains((int)InPlaceId);// || docShipmentOrderInfo.InBranchID != WorkSession.BranchID;
                DenyEditOut = OutPlaceId != null && !WorkSession.PlaceIds.Contains((int)OutPlaceId);//docShipmentOrderInfo.OutBranchID != WorkSession.BranchID;
                if (DocOrderId != null)
                {
                    var orderType =
                      (OrderType) (gammaBase.DocShipmentOrders.FirstOrDefault(d => d.DocOrderID == DocOrderId)?.OrderTypeID ?? 0);
                    switch (orderType)
                    {
                            case OrderType.ShipmentOrer:
                                DocOrderInfo = "Приказ № " +
                                               gammaBase.C1CDocShipmentOrder.FirstOrDefault(
                                                   dso => dso.C1CDocShipmentOrderID == DocOrderId)?.C1CNumber;
                                break;
                            case OrderType.InternalOrder:
                                DocOrderInfo = "Заказ № " +
                                           gammaBase.C1CDocInternalOrders.FirstOrDefault(
                                               dso => dso.C1CDocInternalOrderID == DocOrderId)?.C1CNumber;
                                break;
                            case OrderType.MovementOrder:
                                DocOrderInfo = "Заказ пер.№ " +
                                           gammaBase.C1CDocInternalOrders.FirstOrDefault(
                                               dso => dso.C1CDocInternalOrderID == DocOrderId)?.C1CNumber;
                            break;
                    }
                }
                MovementProducts = new ItemsChangeObservableCollection<MovementProduct>(gammaBase.vDocMovementProducts.Where(dp => dp.DocMovementID == docMovementId)
                    .Select(dp => new MovementProduct
                    {
                        DocMovementId = dp.DocMovementID,
                        Number = dp.Number,
                        Quantity = dp.Quantity??0,
                        IsConfirmed = dp.IsConfirmed,
                        ProductId = dp.ProductID,
                        NomenclatureName = dp.NomenclatureName,
                        IsShipped = dp.IsShipped ?? false,
                        IsAccepted = dp.IsAccepted ?? false,
                        ProductKind = (ProductKind) dp.ProductKindID
                    }));
            }
            OpendDocOrderCommand = new DelegateCommand(OpenDocOrder, () => DocOrderId != null);
            MovementProducts.CollectionChanged += MovementProductsOnCollectionChanged;
            ShowProductCommand = new DelegateCommand(ShowProduct, SelectedProduct != null);
            DeleteProductCommand = new DelegateCommand(DeleteProduct, () => !DenyEditOut && SelectedProduct != null);
            var canUploadTo1CCommand = IsConfirmed && DocOrderId == null;
            UploadTo1CCommand = new DelegateCommand(UploadTo1C,() => canUploadTo1CCommand);
            Bars.Add(ReportManager.GetReportBar("DocMovement", VMID));
            //IsInitialize = false;
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
            if (!SaveToModel()) return;
            ReportManager.PrintReport(msg.ReportID, DocMovementId);
        }

        public bool DenyEditOut { get; private set; }
        public bool DenyEditIn { get; private set; }

        public bool CanChangeIsConfirmed => DocOrderId == null;

        public bool IsDateReadOnly
        {
            get
            { return !(!IsConfirmed && DB.HaveWriteAccess("Docs") && WorkSession.ShiftID == 0 && (WorkSession.DBAdmin || WorkSession.RoleName == "Dispetcher")); }
        }

        public DelegateCommand UploadTo1CCommand { get; private set; }

        //public bool CanUploadTo1C => IsConfirmed && DocOrderId == null;

        private void UploadTo1C()
        {
            UIServices.SetBusyState();
            if (DocOrderId == null)
            {
                DB.UploadFreeMovementTo1C(DocMovementId);
            }
        }

        public MovementProduct SelectedProduct { get; set; }
        public DelegateCommand ShowProductCommand { get; private set; }

        private void ShowProduct()
        {
            switch (SelectedProduct.ProductKind)
            {
                case ProductKind.ProductSpool:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedProduct.ProductId);
                    break;
                case ProductKind.ProductGroupPack:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductGroupPack, SelectedProduct.ProductId);
                    break;
                case ProductKind.ProductPallet:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductPallet, SelectedProduct.ProductId);
                    break;
                case ProductKind.ProductPalletR:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductPalletR, SelectedProduct.ProductId);
                    break;
                default:
                    MessageBox.Show("Ошибка программы, действие не предусмотрено");
                    return;
            }
        }

        public DelegateCommand DeleteProductCommand { get; private set; }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;
            if (SelectedProduct.IsConfirmed == true || SelectedProduct.IsAccepted)
            {
                MessageBox.Show("Нельзя удалить продукт, который уже получен или принят");
                return;
            }
//            if (SelectedProduct == null || SelectedProduct.IsConfirmed == true || SelectedProduct.IsAccepted) return;
            if (MessageBox.Show("Вы уверены, что хотите удалить данный продукт из приказа?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            using (var gammaBase = DB.GammaDb)
            {
                var outProduct =
                    gammaBase.DocOutProducts.FirstOrDefault(
                        op => op.ProductID == SelectedProduct.ProductId && op.DocID == SelectedProduct.DocMovementId);
                if (outProduct != null)
                {
                    gammaBase.DocOutProducts.Remove(outProduct);
                    gammaBase.SaveChanges();
                }
            }
        }

        private void MovementProductsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            IsConfirmed = MovementProducts.All(p => p.IsConfirmed == true);
        }

        private Guid? DocOrderId { get; set; }
        public string DocOrderInfo { get; set; }
        public DelegateCommand OpendDocOrderCommand { get; private set; }

        private void OpenDocOrder()
        {
            if (DocOrderId == null || !DB.HaveReadAccess("DocShipmentOrders")) return;
            UIServices.SetBusyState();
            MessageManager.OpenDocShipmentOrder((Guid)DocOrderId);
        }

        private bool? _checkAll;

        public bool? CheckAll
        {
            get { return _checkAll; }
            set
            {
                _checkAll = value;
                RaisePropertyChanged("CheckAll");
                if (_checkAll == null) return;
                foreach (var product in MovementProducts)
                {
                    product.IsConfirmed = _checkAll;
                    IsConfirmed = (bool)_checkAll;
                }
            }
        }

        private Guid DocMovementId { get; set; }
        public bool IsInVisible { get; private set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }

        private int? _outPlaceId;
        public int? OutPlaceId
        {
            get { return _outPlaceId; }
            set
            {
                _outPlaceId = value;
                if (_outPlaceId == null) return;
                using (var gammaBase = DB.GammaDb)
                {
                    OutPlace = gammaBase.Places.FirstOrDefault(p => p.PlaceID == _outPlaceId)?.Name;
                }
            }
        }
        public string OutPlace { get; set; }

        private int? _inPlaceId;
        public int? InPlaceId
        {
            get { return _inPlaceId; }
            set
            {
                _inPlaceId = value;
                if (_inPlaceId == null) return;
                using (var gammaBase = DB.GammaDb)
                {
                    InPlace = gammaBase.Places.FirstOrDefault(p => p.PlaceID == _inPlaceId)?.Name;
                }
            }
        }

        public string InPlace { get; set; }
        //private bool IsInitialize { get; set; } = false;
        private bool _isConfirmed;

        public bool IsConfirmed
        {
            get { return _isConfirmed; }
            set
            {
                if (_isConfirmed != value)
                {
                    _isConfirmed = value;
                    //saveDoc();
                    RaisePropertyChanged("IsConfirmed");
                }
            }
        }

       /* private void saveDoc()
        {
            if (!IsInitialize)
            {
                using (var gammaBase = DB.GammaDb)
                {
                    var doc = gammaBase.Docs.Where(d => d.DocID == DocMovementId).FirstOrDefault();
                    if (doc != null)
                    {
                        doc.IsConfirmed = IsConfirmed;
                    }
                    gammaBase.SaveChanges();
                }
                UploadTo1C();
            }
        }*/

        public ItemsChangeObservableCollection<MovementProduct> MovementProducts { get; set; }
//        public List<Place> Warehouses { get; private set; }

        public override bool SaveToModel()
        {
            if (!DB.HaveWriteAccess("DocMovement")) return true;
            using (var gammaBase = DB.GammaDb)
            {
                var docMovement =
                    gammaBase.DocMovement.Include(d => d.DocInProducts).Include(d => d.Docs).FirstOrDefault(d => d.DocID == DocMovementId);
                if (docMovement == null)
                {
                    MessageBox.Show("Документ уже не существует в базе. Скорей всего он был кем-то удален");
                    return true;
                }
                docMovement.Docs.Number = Number;
                docMovement.Docs.Date = new DateTime(Date.Year, Date.Month, Date.Day, Date.Hour,Date.Minute, 0);
                docMovement.InPlaceID = InPlaceId;
                docMovement.OutPlaceID = OutPlaceId;
                docMovement.Docs.IsConfirmed = IsConfirmed;
                foreach (var movementProduct in MovementProducts)
                {
                    var inProduct = docMovement.DocInProducts.FirstOrDefault(p => p.ProductID == movementProduct.ProductId);
                    if (inProduct != null)
                        inProduct.IsConfirmed =
                            movementProduct.IsConfirmed;
                    else if (movementProduct.IsConfirmed == true)
                    {
                        inProduct = new DocInProducts
                        {
                            DocID = movementProduct.DocMovementId,
                            Date = DB.CurrentDateTime,
                            IsConfirmed = movementProduct.IsConfirmed,
                            ProductID = movementProduct.ProductId
                        };
                        gammaBase.DocInProducts.Add(inProduct);
                    }
                }
                gammaBase.SaveChanges();
                UploadTo1C();
            }
            return true;
        }

        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();
        public Guid? VMID { get; } = Guid.NewGuid();
    }
}
