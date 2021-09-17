// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using Gamma.Attributes;
using DevExpress.Mvvm;
using System;
using Gamma.Interfaces;
using System.Linq;
using Gamma.Models;
using System.Windows;
using System.Collections.Generic;
using System.Data.Entity;
using Gamma.Common;
using Gamma.Entities;
using Gamma.DialogViewModels;
using Gamma.Dialogs;
using System.ComponentModel;
using System.Diagnostics;
using System.Data.Entity.SqlServer;
using System.Collections.ObjectModel;

namespace Gamma.ViewModels
{
    /// <summary>
    /// ViewModel для пакета заданий
    /// </summary>
    public class ProductionTaskBatchViewModel : SaveImplementedViewModel, ICheckedAccess
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskViewModel class.
        /// </summary>
        public ProductionTaskBatchViewModel(OpenProductionTaskBatchMessage msg)
        {
            Contractors = GammaBase.C1CContractors.Where(c => c.IsBuyer ?? false).ToList();
            ProductionTaskBatchID = msg.ProductionTaskBatchID ?? SqlGuidUtil.NewSequentialid();
            ChangeStateReadOnly = !DB.HaveWriteAccess("ProductionTasks");
            BatchKind = msg.BatchKind;
            TaskStates = new ProductionTaskStates().ToDictionary();
            if (WorkSession.RoleName != "Planner")
                TaskStates.Remove(3);
            ProcessModels = new ProcessModels().ToDictionary();
            RefreshProductionCommand = new DelegateCommand(RefreshProduction);
            ShowProductCommand = new DelegateCommand<Guid>(ShowProduct);//, () => SelectedProductionTaskProduct != null);
            DeleteProdutCommand = new DelegateCommand(DeleteProduct, () => SelectedProductionTaskProduct != null);
            UploadTo1CCommand = new DelegateCommand(UploadTo1C, () => ProductionTaskStateID == (byte)ProductionTaskStates.InProduction);
            Intervals = new List<string> { "Все", "За мою смену", "За последние сутки"};
            if (WorkSession.IsProductionPlace)
            {
                Intervalid = 1;
            }
            if (msg.ProductionTaskBatchID == null)
            {
                //Date = DB.CurrentDateTime;
                //                ProductionTaskBatchID = SqlGuidUtil.NewSequentialid();
                Title = "Новое задание на производство";
                IsActual = false;
                //                if (Places.Count > 0)
                //                    PlaceID = Places[0].PlaceID;
                ProductionTaskStateID = 0;
                if (msg.BatchKind == BatchKinds.SGB)
                {
                    ProcessModelID = 0;
                }
            }
            else
            {
                GetProductionTaskInfo(ProductionTaskBatchID);
                RefreshProduction();
                RefreshRepack();
                RefreshSample();
                RefreshDowntime();
                Title = "Задание на производство № " + Number;
            }

            switch (msg.BatchKind)
            {
                case BatchKinds.SGB:
                    //                    PlaceGroupID = (int)PlaceGroups.PM;
                    ProcessModelVisible = Visibility.Visible;
                    switch (WorkSession.PlaceGroup)
                    {
                        case PlaceGroup.PM:
                            NewProductText = "Создать новый тамбур";
                            break;
                        case PlaceGroup.Rw:
                            NewProductText = "Создать новый съём";
                            break;
                        case PlaceGroup.Wr:
                            NewProductText = "Создать групповую упаковку";
                            break;
                    }
                    //                    PlaceGroupID = (int)PlaceGroups.RW;
                    break;
                case BatchKinds.SGI:
                    NewProductText = "Транспортная этикетка";//WorkSession.IsRemotePrinting? "Сделать задание активным" : "Печать транспортной этикетки";
                    NewProductRText = "Неполная этикетка";
                    GetStatusApplicator();
                    //ChangeStatusApplicatorText = WorkSession.IsRemotePrinting ? true ? "Отключить принтер" : "Включить принтер" : "Не нажимать";
                    ExpandProductionTaskProducts = WorkSession.PlaceGroup != PlaceGroup.Other;
                    break;

            }
            MakeProductionTaskActiveForPlaceCommand = new DelegateCommand(MakeProductionTaskActiveForPlace, CanMakeProductionTaskActiveForPlace);
            CreateNewProductCommand = new DelegateCommand(CreateNewProduct, CanCreateNewProduct);
            CreateNewProductRCommand = new DelegateCommand(CreateNewProductR, CanCreateNewProductR);
            //!WorkSession.IsRemotePrinting ? new DelegateCommand(CreateNewProduct, CanCreateNewProduct) 
            //: new DelegateCommand(MakeProductionTaskActiveForPlace, DB.HaveWriteAccess("ActiveProductionTasks"));
            ChangeStatusApplicatorCommand = new DelegateCommand(ChangeStatusApplicator, CanChangeStatusApplicator);
            PrintGroupPackLabelCommand = new DelegateCommand(PrintGroupPackLabel, CanPrintGroupPack);
            ActivatedCommand = new DelegateCommand(() => IsActive = true);
            DeactivatedCommand = new DelegateCommand(() => IsActive = false);
            AddRepackCommand = new DelegateCommand(AddRepack);
            RefreshRepackCommand = new DelegateCommand(RefreshRepack);
            //DeleteRepackCommand = new DelegateCommand(DeleteRepack, () => SelectedRepack != null);
            AddSampleCommand = new DelegateCommand(AddSample);
            ShowSampleCommand = new DelegateCommand(ShowSample, () => SelectedSample != null);
            DeleteSampleCommand = new DelegateCommand(DeleteSample, () => SelectedSample != null);
            AddDowntimeCommand = new DelegateCommand(AddDowntime);
            ShowDowntimeCommand = new DelegateCommand(ShowDowntime, () => SelectedDowntime != null);
            DeleteDowntimeCommand = new DelegateCommand(DeleteDowntime, () => SelectedDowntime != null);
            RefreshDowntimeCommand = new DelegateCommand(RefreshDowntime);
            Messenger.Default.Register<BarcodeMessage>(this, BarcodeReceived);
            Messenger.Default.Register<ProductChangedMessage>(this, ProductChanged);

            using (var gammaBase = DB.GammaDb)
            {
                var permissionOnCreateNewProduct = gammaBase.CheckPermissionOnCreateNewProduct(ProductionTaskBatchID, WorkSession.UserID).FirstOrDefault();
                if (permissionOnCreateNewProduct != null)
                {
                    GrantPermissionOnCreateNewProduct = (bool)permissionOnCreateNewProduct;
                }
                else
                {
                    GrantPermissionOnCreateNewProduct = false;
                }
            }

            //VisiblityMakeProductionTaskActiveForPlace = Visibility.Visible;
            IsProductionTaskActiveForPlace = CheckProductionTaskActiveForPlace();
            VisiblityPrintGroupPack = Visibility.Visible;

            //VisiblityCreateNewProduct = VisiblityMakeProductionTaskActiveForPlace == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            var barFirst = new BarViewModel();
            barFirst.Commands.Add(new BarCommand<object>(p => AddDowntime()) { Caption = "Добавить" });
            barFirst.Commands.Add(new BarCommand<object>(p => DeleteDowntime()) { Caption = "Удалить" });
            barFirst.Commands.Add(new BarCommand<object>(p => RefreshDowntime()) { Caption = "Обновить" });
            DowntimeBars.Add(barFirst);
            var placeID = GammaBase.ProductionTaskBatches.Where(t => t.ProductionTaskBatchID == ProductionTaskBatchID).FirstOrDefault().ProductionTasks.FirstOrDefault().PlaceID;
            var templates = GammaBase.DowntimeTemplates.Where(t => t.PlaceID == placeID).ToList();
            var bar = new BarViewModel();
            foreach (var template in templates)
            {
                var command = new BarCommand<object>(dnt => AddDowntime((AddDowntimeParameter)dnt))
                {
                    Caption = template.C1CDowntimeTypes.Description.Substring(0, Math.Min(7, template.C1CDowntimeTypes.Description.Length))
                                + (template.C1CDowntimeTypeDetails != null ? "__" + template.C1CDowntimeTypeDetails.Description.Substring(0,Math.Min(5, template.C1CDowntimeTypeDetails.Description.Length)) : "")
                                + (template.C1CEquipmentNodes != null ? "#" + template.C1CEquipmentNodes.Description.Substring(0, Math.Min(7, template.C1CEquipmentNodes.Description.Length)) : "")
                                + (template.C1CEquipmentNodeDetails != null ? "__" + template.C1CEquipmentNodeDetails.Description.Substring(0, Math.Min(5, template.C1CEquipmentNodeDetails.Description.Length)) : "") +"  |",
                    CommandParameter = new AddDowntimeParameter
                    {
                        DowntimeTypeID = template.C1CDowntimeTypeID,
                        DowntimeTypeDetailID = template.C1CDowntimeTypeDetailID,
                        EquipmentNodeID = template.C1CEquipmentNodeID,
                        EquipmentNodeDetailID = template.C1CEquipmentNodeDetailID,
                        Comment = template.Comment
                    }
                };
                bar.Commands.Add(command);
            }
            DowntimeBars.Add(bar);
        }

        public ObservableCollection<BarViewModel> DowntimeBars { get; set; } = new ObservableCollection<BarViewModel>();

        public DelegateCommand DeleteProdutCommand { get; private set; }

        private void DeleteProduct()
        {
            if (SelectedProductionTaskProduct == null) return;
            if (WorkSession.ShiftID != 0 && (SelectedProductionTaskProduct.PlaceID != WorkSession.PlaceID || SelectedProductionTaskProduct.ShiftID != WorkSession.ShiftID))
            {
                MessageBox.Show("Вы не можете удалить продукцию другой смены или другого передела");
                return;
            }
            if (MessageBox.Show(
                "Вы уверены, что хотите удалить продукт № " + SelectedProductionTaskProduct.Number + "?",
                "Удаление продукта", MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.Yes) != MessageBoxResult.Yes)
            {
                return;
            };
            string delResult = "";
            switch (SelectedProductionTaskProduct.ProductKind)
            {
                case ProductKind.ProductSpool:
                    if (DB.HaveWriteAccess("ProductSpools"))
                    {
                        delResult = GammaBase.DeleteSpool(SelectedProductionTaskProduct.ProductID).FirstOrDefault();
                    }
                    else
                    {
                        MessageBox.Show("Не достаточно прав для удаления тамбура");
                    }
                    break;
                case ProductKind.ProductGroupPack:
                    if (DB.HaveWriteAccess("ProductGroupPacks"))
                    {
                        delResult = GammaBase.DeleteGroupPack(SelectedProductionTaskProduct.ProductID).FirstOrDefault();
                    }
                    else
                    {
                        MessageBox.Show("Не достаточно прав для удаления групповой упаковки");
                    }
                    break;
                case ProductKind.ProductPallet:
                    if (DB.HaveWriteAccess("ProductPallets"))
                    {
                        delResult = GammaBase.DeletePallet(SelectedProductionTaskProduct.ProductID).FirstOrDefault();
                    }
                    else
                    {
                        MessageBox.Show("Не достаточно прав для удаления паллеты");
                    }
                    break;
                case ProductKind.ProductPalletR:
                    if (DB.HaveWriteAccess("ProductPallets"))
                    {
                        delResult = GammaBase.DeletePallet(SelectedProductionTaskProduct.ProductID).FirstOrDefault();
                    }
                    else
                    {
                        MessageBox.Show("Не достаточно прав для удаления неполной паллеты");
                    }
                    break;                    
            }
            if (delResult != "")
            {
                MessageBox.Show(delResult, "Удалить не удалось", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
                ProductionTaskProducts.Remove(SelectedProductionTaskProduct);
        }

        public DelegateCommand UploadTo1CCommand { get; private set; }

        private void UploadTo1C()
        {
            UIServices.SetBusyState();
            if (CurrentView != null && ProductionTaskBatchID != null)
            {
                DB.UploadProductionTaskBatchTo1C(ProductionTaskBatchID, GammaBase);
            }
        }

        public override bool IsValid => base.IsValid && (!PartyControl || ContractorId != null);

        private bool IsActive { get; set; } = true;

        private void ProductChanged(ProductChangedMessage msg)
        {
            var product = ProductionTaskProducts.FirstOrDefault(p => p.ProductID == msg.ProductID);
            if (product == null) return;
            using (var gammaBase = DB.GammaDb)
            {
                var info = gammaBase.vProductsInfo.FirstOrDefault(p => p.ProductID == product.ProductID);
                if (info == null) return;
                product.NomenclatureID = info.C1CNomenclatureID;
                product.CharacteristicID = info.C1CCharacteristicID;
                product.NomenclatureName = info.NomenclatureName;
                product.Quantity = info.Quantity;
                if (product.IsConfirmed == info.IsConfirmed) return;
                product.IsConfirmed = info.IsConfirmed;
                var view = CurrentView as ProductionTaskSGIViewModel;
                if (view != null)
                {
                    view.MadeQuantity =
                        _productionTaskProducts.Where(p => p.IsConfirmed).Sum(p => p.Quantity ?? 0);
                }
            }

        }

        /// <summary>
        /// Действие при получении шк от сканера
        /// </summary>
        /// <param name="msg">сообщение с шк</param>
        private void BarcodeReceived(BarcodeMessage msg)
        {
            if (!IsActive) return;
            var docProductionProducts = GammaBase.DocProductionProducts.Include(dp => dp.DocProduction.Docs).Include(dp => dp.Products)
                .FirstOrDefault(
                    dp => dp.Products.BarCode == msg.Barcode);
            if (docProductionProducts == null) return;
            GammaBase.Entry(docProductionProducts.DocProduction.Docs).Reload();
            docProductionProducts.DocProduction.Docs.IsConfirmed = true;
            string message;
            switch (docProductionProducts.Products.ProductKindID)
            {
                case (byte)ProductKind.ProductSpool:
                    message = $"Тамбур №{docProductionProducts.Products.Number} подтвержден";
                    break;
                case (byte)ProductKind.ProductPallet:
                    message = $"Паллета №{docProductionProducts.Products.Number} подтверждена";
                    break;
                case (byte)ProductKind.ProductGroupPack:
                    message = $"Групповая упаковка №{docProductionProducts.Products.Number} подтверждена";
                    break;
                default:
                    message = $"Продукт №{docProductionProducts.Products.Number} подтвержден";
                    break;
            }
            GammaBase.SaveChanges();
            MessageBox.Show(message, "Подтверждение", MessageBoxButton.OK, MessageBoxImage.Information);
            var productionTaskProduct =
                ProductionTaskProducts.FirstOrDefault(ptp => ptp.ProductID == docProductionProducts.ProductID);
            if (productionTaskProduct != null) productionTaskProduct.IsConfirmed = true;
            var view = CurrentView as ProductionTaskSGIViewModel;
            if (view != null)
            {
                view.MadeQuantity =
                    _productionTaskProducts.Where(p => p.IsConfirmed).Sum(p => p.Quantity ?? 0);
            }
        }

        private bool CanMakeProductionTaskActiveForPlace()
        {
            return DB.HaveWriteAccess("ActiveProductionTasks") && !IsProductionTaskActiveForPlace && GrantPermissionOnProductionTaskActiveForPlace;
        }

        private bool CanCreateNewProduct()
        {
            switch (BatchKind)
            {
                case BatchKinds.SGB:
                    return DB.HaveWriteAccess("ProductSpools") && IsActual && GrantPermissionOnCreateNewProduct && !WorkSession.IsRemotePrinting;
                case BatchKinds.SGI:
                    return DB.HaveWriteAccess("ProductPallets") && IsActual && GrantPermissionOnCreateNewProduct && !WorkSession.IsRemotePrinting;
                default:
                    return false;
            }
        }

        private bool CanCreateNewProductR()
        {
            return CanCreateNewProduct();
        }

        /*
        private Visibility _visiblityMakeProductionTaskActiveForPlace = Visibility.Collapsed;

        public Visibility VisiblityMakeProductionTaskActiveForPlace
        {
            get { return _visiblityMakeProductionTaskActiveForPlace; }
            set
            {
                //if (value == Visibility.Visible && !CanMakeProductionTaskActiveForPlace())
                //    return;
                _visiblityMakeProductionTaskActiveForPlace = value;
                RaisePropertyChanged("VisiblityMakeProductionTaskActiveForPlace");
                //VisiblityCreateNewProduct = value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            }
        }
        */
        private Visibility _visiblityCreateNewProduct = Visibility.Collapsed;
        public Visibility VisiblityCreateNewProduct
        {
            get { return _visiblityCreateNewProduct; }
            set
            {
                if (value == Visibility.Visible && !CanCreateNewProduct())
                    return;
                _visiblityCreateNewProduct = value;
                if (VisiblityCreateNewProductR != value)
                    VisiblityCreateNewProductR = value;
                RaisePropertyChanged("VisiblityCreateNewProduct");
            }
        }


        private Visibility _visiblityCreateNewProductR = Visibility.Collapsed;
        public Visibility VisiblityCreateNewProductR
        {
            get { return _visiblityCreateNewProductR; }
            set
            {
                if (value == Visibility.Visible && BatchKind != BatchKinds.SGI)
                    return;
                _visiblityCreateNewProductR = value;
                RaisePropertyChanged("VisiblityCreateNewProductR");
            }
        }

        private Visibility _visiblityPrintGroupPack = Visibility.Collapsed;
        public Visibility VisiblityPrintGroupPack
        {
            get { return _visiblityPrintGroupPack; }
            set
            {
                if (value == Visibility.Visible && !CanChangeStatusApplicator())
                    return;
                _visiblityPrintGroupPack = value;
                RaisePropertyChanged("VisiblityPrintGroupPack");
            }
        }

        
        private bool _isProductionTaskActiveForPlace;
        public bool IsProductionTaskActiveForPlace
        {
            get { return _isProductionTaskActiveForPlace; }
            set
            {
                _isProductionTaskActiveForPlace = value;
                if (value)
                {
                    MakeProductionTaskActiveForPlaceText = "Задание активно.";
                    VisiblityCreateNewProduct = Visibility.Visible;
                    VisiblityPrintGroupPack = Visibility.Visible;

                }
                else
                {
                    MakeProductionTaskActiveForPlaceText = "Сделать задание активным";
                    //VisiblityCreateNewProduct = Visibility.Collapsed; Всегда можно напечатать транспортную этикетку
                    VisiblityCreateNewProduct = Visibility.Visible;

                }
            }
        }


        private bool CanChangeStatusApplicator()
        {
            return BatchKind == BatchKinds.SGI && WorkSession.UseApplicator ? DB.HaveWriteAccess("ProductPallets") && IsActual && IsProductionTaskActiveForPlace : false;
        }

        private bool CanPrintGroupPack()
        {
            return BatchKind == BatchKinds.SGI && WorkSession.UseApplicator && StatusApplicator != null ? DB.HaveWriteAccess("ProductPallets") && IsActual && IsProductionTaskActiveForPlace : false;
        }

        private SaveImplementedViewModel _currentView;
        public SaveImplementedViewModel CurrentView
        {
            get { return _currentView; }
            private set 
            { 
                _currentView = value;
                RaisePropertyChanged("CurrentView");
            }
        }

        private DateTime? _date;
        public DateTime? Date
        {
            get { return _date; }
            set 
            { 
                _date = value;
                RaisePropertyChanged("Date");
            }
        }

        private string _number;
        public string NewProductText { get; set; }
        /// <summary>
        /// Печать транспортной этикетки  неполной паллеты
        /// </summary>
        public string NewProductRText { get; set; }


        private string _makeProductionTaskActiveForPlaceText;
        public string MakeProductionTaskActiveForPlaceText
        {
            get { return _makeProductionTaskActiveForPlaceText; }
            private set
            {
                _makeProductionTaskActiveForPlaceText = value;
                RaisePropertyChanged("MakeProductionTaskActiveForPlaceText");
            }
        }

        private string _changeStatusApplicatorText;
        public string ChangeStatusApplicatorText
        {
            get { return _changeStatusApplicatorText; }
            private set
            {
                _changeStatusApplicatorText = value;
                RaisePropertyChanged("ChangeStatusApplicatorText");
            }
        }

        private bool IsActual { get; set; } = true;
        private bool GrantPermissionOnCreateNewProduct { get; set; } = false;
        private bool GrantPermissionOnProductionTaskActiveForPlace { get; set; } = false;

        public string Number
        {
            get { return _number; }
            set 
            { 
                _number = value;
                RaisePropertyChanged("Number");
            }
        }
        //[UIAuth(UIAuthLevel.ReadOnly)]
        public string Comment { get; set; }
        /// <summary>
        /// Создание нового продукта. В конструкторе привязка к CreateNewProduct();
        /// </summary>
        public DelegateCommand CreateNewProductCommand { get; private set; }

        /// <summary>
        /// Создание нового продукта. В конструкторе привязка к CreateNewProductR();
        /// </summary>
        public DelegateCommand CreateNewProductRCommand { get; private set; }

        /// <summary>
        /// Активация задания. В конструкторе привязка к MakeProductionTaskActiveForPlace();
        /// </summary>
        public DelegateCommand MakeProductionTaskActiveForPlaceCommand { get; private set; }

        /// <summary>
        /// Отключение/включение аппликатора. В конструкторе привязка к ChangeStatusApplicator();
        /// </summary>
        public DelegateCommand ChangeStatusApplicatorCommand { get; private set; }

        /// <summary>
        /// Печать групповой упаковки. В конструкторе привязка к PrintGroupPackLabel();
        /// </summary>
        public DelegateCommand PrintGroupPackLabelCommand { get; private set; }

        //private bool IsInProduction { get; set; }
        /// <summary>
        /// Только для чтения, если нет прав или задание не в состоянии "на рассмотрении"
        /// </summary>
        public bool IsReadOnly => !DB.HaveWriteAccess("ProductionTasks") || ProductionTaskStateID == (byte)ProductionTaskStates.InProduction;


        private BatchKinds _batchKind;

 /*       public ObservableCollection<Place> Places
        {
            get
            {
                return _places;
            }
            set
            {
                _places = value;
                RaisePropertyChanged("Places");
            }
        }
  * */
        public BatchKinds BatchKind
        {
            get { return _batchKind; }
            set 
            { 
                _batchKind = value;
                ChangeCurrentView(_batchKind);
            }
        }
        private byte? _processModelId;

        private bool _partyControl;

        /// <summary>
        /// Идентификатор контрагента
        /// </summary>
        [UIAuth(UIAuthLevel.ReadOnly)]
        public Guid? ContractorId { get; set; }

        /// <summary>
        /// Список контрагентов
        /// </summary>
        public List<C1CContractors> Contractors { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public bool PartyControl
        {
            get
            {
                return _partyControl;
            }
            set
            {
            	_partyControl = value;
                RaisePropertyChanged("PartyControl");
            }
        }
        [UIAuth(UIAuthLevel.ReadOnly)]
        public byte? ProcessModelID
        {
            get
            {
                return _processModelId;
            }
            set
            {
                _processModelId = value;
                if (CurrentView is IProductionTaskBatch)
                {
                    (CurrentView as IProductionTaskBatch).ProcessModelID = _processModelId ?? 0;
                }
            }
        }

        public Visibility ProcessModelVisible { get; private set; } = Visibility.Hidden;

        public Guid ProductionTaskBatchID { get; set; }

        private void GetProductionTaskInfo(Guid productionTaskBatchID)
        {
            var productionTaskBatch = (from pt in DB.GammaDb.ProductionTaskBatches.Include(pt => pt.ProductionTaskStates)
                                  where pt.ProductionTaskBatchID == productionTaskBatchID
                                  select pt).FirstOrDefault();
//            ProductionTaskBatchID = productionTaskBatchID;
            PartyControl = productionTaskBatch?.PartyControl ?? false;
            Number = productionTaskBatch?.Number;
            Date = productionTaskBatch?.Date;         
            Comment = productionTaskBatch?.Comment;
            ProductionTaskStateID = productionTaskBatch?.ProductionTaskStateID;
            //IsInProduction = ProductionTaskStateID != (byte) ProductionTaskStates.NeedsDecision;
            ProcessModelID = (byte?)productionTaskBatch?.ProcessModelID;
            ContractorId = productionTaskBatch?.C1CContractorID;
            if (productionTaskBatch?.ProductionTaskStates != null)
                IsActual = productionTaskBatch.ProductionTaskStates.IsActual;
            var place = productionTaskBatch?.ProductionTasks.FirstOrDefault().Places;
            IsEnabledSamples = place?.IsEnabledSamplesInDocCloseShift ?? true;
            IsEnabledRepack = place?.IsEnabledRepackInProductionTask ?? true;
            IsEnabledDowntimes = place?.IsEnabledDowntimes ?? false;
        }

        private void ChangeCurrentView(BatchKinds batchKind)
        {
            switch (batchKind)
            {
                case BatchKinds.SGB:
                    CurrentView = new ProductionTaskBatchSGBViewModel(ProductionTaskBatchID);
                    break;
                case BatchKinds.SGI:
                    CurrentView = new ProductionTaskSGIViewModel(ProductionTaskBatchID);
                    ((ProductionTaskSGIViewModel)CurrentView).PrintExampleEvent += PrinExampleLabelFromSGICurrentView;
                    break;
                case BatchKinds.Baler:
                    CurrentView = new ProductionTaskBalerViewModel(ProductionTaskBatchID);
                    break;
            }
        }


        public bool PrinExampleLabelFromSGICurrentView()
        {
            if (SaveToModel())
            {
                var currentView = CurrentView as ProductionTaskSGIViewModel;

                if (currentView?.ProductionTaskId != null && currentView?.ProductionTaskId != Guid.Empty)
                {
                    if ((WorkSession.EndpointAddressOnGroupPackService ?? WorkSession.EndpointAddressOnTransportPackService) == null)
                        CheckGroupAndTransportPackLabel((Guid)currentView?.ProductionTaskId, WorkSession.EndpointAddressOnMailService); 
                    else
                        CheckGroupAndTransportPackLabel((Guid)currentView?.ProductionTaskId, WorkSession.EndpointAddressOnGroupPackService ?? WorkSession.EndpointAddressOnTransportPackService);

                }
                return true;
            }
            else
                return false;
        }

        public override bool SaveToModel()
        {
            if (!DB.HaveWriteAccess("ProductionTaskBatches")) return true;
            using (var gammaBase = DB.GammaDb)
            {
                var productionTaskBatch = gammaBase.ProductionTaskBatches.FirstOrDefault(p => p.ProductionTaskBatchID == ProductionTaskBatchID);
                if (productionTaskBatch == null)
                {
                    productionTaskBatch = new ProductionTaskBatches()
                    {
                        ProductionTaskBatchID = ProductionTaskBatchID,
                        UserID = WorkSession.UserID
                    };
                    gammaBase.ProductionTaskBatches.Add(productionTaskBatch);
                }
                productionTaskBatch.ProcessModelID = ProcessModelID;
                productionTaskBatch.ProductionTaskStateID = ProductionTaskStateID ?? 0;
                productionTaskBatch.Date = Date ?? DB.CurrentDateTime;
                productionTaskBatch.Comment = Comment;
                productionTaskBatch.PartyControl = PartyControl;
                productionTaskBatch.BatchKindID = (short)BatchKind;
                productionTaskBatch.C1CContractorID = ContractorId;
                gammaBase.SaveChanges();
            }
            CurrentView?.SaveToModel(ProductionTaskBatchID);
            if (ProductionTaskStateID == (byte)ProductionTaskStates.InProduction || ProductionTaskStateID == (byte)ProductionTaskStates.Completed)
                UploadTo1C();
            return true;
        }

        /*
                private void SetProductionTaskProperties(ProductionTaskBatches productionTaskBatch)
                {
                    productionTaskBatch.Date = Date;
                    productionTaskBatch.Comment = Comment;
                    productionTaskBatch.UserID = WorkSession.UserID;
                    productionTaskBatch.BatchKindID = (short)BatchKind;
                    productionTaskBatch.ProductionTaskStateID = ProductionTaskStateID;
                }
        */

        private bool CheckProductionTaskActiveForPlace()
        {
            using (var gammaBase = DB.GammaDb)
            {
                var productionTask =
                    gammaBase.ProductionTasks.Include(d => d.ActiveProductionTasks).FirstOrDefault(
                        p => p.ProductionTaskBatches.Any(ptb => ptb.ProductionTaskBatchID == ProductionTaskBatchID) &&
                             ((p.PlaceID != null && p.PlaceID == WorkSession.PlaceID) || (p.PlaceID == null && p.PlaceGroupID == (byte)WorkSession.PlaceGroup)));
                GrantPermissionOnProductionTaskActiveForPlace = (productionTask != null);
                if (productionTask == null)
                    return false;
                return false; // Всегда ложь для того, чтобы активировать задание надо было каждый раз вручную (productionTask.ActiveProductionTasks != null);
            }
        }

        private void MakeProductionTaskActiveForPlace()
        {
            using (var gammaBase = DB.GammaDb)
            {
                gammaBase.MakeProductionTaskActiveForPlace(WorkSession.PlaceID, ProductionTaskBatchID);
	            var productionTask =
		            gammaBase.ProductionTasks.FirstOrDefault(
			            p => p.ProductionTaskBatches.Any(ptb => ptb.ProductionTaskBatchID == ProductionTaskBatchID) &&
                             ((p.PlaceID != null && p.PlaceID == WorkSession.PlaceID) || (p.PlaceID == null && p.PlaceGroupID == (byte)WorkSession.PlaceGroup)));
	            if (productionTask == null)
                {
		            return;
	            }
                //VisiblityMakeProductionTaskActiveForPlace = Visibility.Collapsed;
                IsProductionTaskActiveForPlace = true;
                CheckGroupAndTransportPackLabel(productionTask.ProductionTaskID, WorkSession.EndpointAddressOnGroupPackService ?? WorkSession.EndpointAddressOnTransportPackService);
                if (WorkSession.UseApplicator && WorkSession.EndpointAddressOnGroupPackService != null) //пока такое условие нормально, так как в принтер по zpl только для групповых, а транспортные чрез виндовый spooler
                {
                    try
                    {
                        using (var client = new GammaService.PrinterServiceClient(WorkSession.EndpointConfigurationName,WorkSession.EndpointAddressOnGroupPackService))
                        {
                            if (!(bool)client.ActivateProductionTask(productionTask.ProductionTaskID, WorkSession.PlaceID, 2))
                            {
                                MessageBox.Show("Не удалось сменить амбалаж для аппликатора", "Аппликатор",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                            }
                            /*
                            bool? res = client.ChangePrinterStatus(WorkSession.PlaceID, 2);
                            if (res == null)
                            {
                                MessageBox.Show("Не удалось сменить состояние принтера", "Принтер",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                            }
                            else if ((bool) res)
                            {
                                MessageBox.Show("Принтер активирован", "Принтер",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                            }
                            else
                            {
                                MessageBox.Show("Принтер остановлен", "Принтер",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                            }
                            */
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Техническая проблема при смене амбалажа для аппликатора: сервис недоступен. Обратитесь в техподдержку Гаммы.", "Аппликатор",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void CreateNewProductR()
        {
            var dialogResult = MessageBox.Show("Создание транспортной этикетки неполной паллеты! Вы уверены?", "Внимание!",MessageBoxButton.YesNo,MessageBoxImage.Warning);
            if (dialogResult == MessageBoxResult.Yes)
            {
                var productionTaskID = GammaBase.ProductionTaskBatches.Where(p => p.ProductionTaskBatchID == ProductionTaskBatchID).
                        Select(p => p.ProductionTasks.FirstOrDefault(pt => pt.PlaceGroupID == (byte)WorkSession.PlaceGroup).ProductionTaskID).
                        FirstOrDefault();
                var baseQuantity = (int)(GammaBase.C1CCharacteristics.Where(
                                c => c.C1CCharacteristicID == GammaBase.ProductionTasks.Where(p => p.ProductionTaskID == productionTaskID).Select(p => p.C1CCharacteristicID).FirstOrDefault())
                                .Select(c => c.C1CMeasureUnitsPallet.Coefficient).First() ?? 9999);

                var model = new SetQuantityDialogModel("Укажите количество рулончиков или пачек(для салфеток) в неполной паллете","Кол-во, рул/пачка", 1, baseQuantity);
                var okCommand = new UICommand()
                {
                    Caption = "OK",
                    IsCancel = false,
                    IsDefault = true,
                    Command = new DelegateCommand<CancelEventArgs>(
                x => DebugFunc(),
                x => model.Quantity >= 1 && model.Quantity < baseQuantity),
                };
                var cancelCommand = new UICommand()
                {
                    Id = MessageBoxResult.Cancel,
                    Caption = "Отмена",
                    IsCancel = true,
                    IsDefault = false,
                };
                var dialogService = GetService<IDialogService>("SetQuantityDialog");
                var result = dialogService.ShowDialog(
                    dialogCommands: new List<UICommand>() { okCommand, cancelCommand },
                    title: "Кол-во рулончиков/пачек",
                    viewModel: model);
                if (result == okCommand)
                    CreateNewProduct(model.Quantity);
            }
        }

        //protected IDialogService dialogService { get { return this.GetService<IDialogService>(); } }

        private void DebugFunc()
        {
            Debug.Print("Кол-во задано");
        }

        private void CreateNewProduct()
        {
            CreateNewProduct(null);
        }

        //Создание нового продукта
        private void CreateNewProduct(int? baseQuantity )
        {
            using (var gammaBase = DB.GammaDb)
            {
                var docProductKind = new DocProductKinds();
                var productionTaskID = gammaBase.ProductionTaskBatches.Where(p => p.ProductionTaskBatchID == ProductionTaskBatchID).
                        Select(p => p.ProductionTasks.FirstOrDefault(pt => pt.PlaceGroupID == (byte)WorkSession.PlaceGroup).ProductionTaskID).
                        FirstOrDefault();
                var productId = SqlGuidUtil.NewSequentialid();
                var checkResult = SourceSpoolsCheckResult.Correct;
                Guid c1CCharacteristicID = Guid.Empty;
                switch (BatchKind)
                {
                    case BatchKinds.SGB:
                        switch (WorkSession.PlaceGroup)
                        {
                            case PlaceGroup.PM:
                                docProductKind = DocProductKinds.DocProductSpool;
                                // Проверка на наличие тамбура с предыдущей смены
                                var curDate = DB.CurrentDateTime;
                                // получаем предыдущий документ в базе
                                var docProduction = gammaBase.Docs.Include(d => d.DocProduction)
                                    .Where(d => d.PlaceID == WorkSession.PlaceID // && d.ShiftID == null
                                                                                 //    && d.Date >= SqlFunctions.DateAdd("hh",-12, DB.GetShiftBeginTime(curDate))
                                    && d.DocTypeID == (byte)DocTypes.DocProduction).OrderByDescending(d => d.Date)
                                    .FirstOrDefault();
                                //Если не указана смена, то переходный тамбур. Устанавливаем недостающие свойства и открываем для редактирования
                                if (docProduction != null && docProduction.ShiftID == null)
                                {
                                    docProduction.Date = curDate;
                                    docProduction.ShiftID = WorkSession.ShiftID;
                                    docProduction.UserID = WorkSession.UserID;
                                    docProduction.PrintName = WorkSession.PrintName;
                                    docProduction.DocProduction.ProductionTaskID = productionTaskID;
                                    productId = gammaBase.DocProductionProducts.Where(d => d.DocID == docProduction.DocID)
                                        .Select(d => d.ProductID)
                                        .First();
                                    var productSpool =
                                        gammaBase.ProductSpools.First(p => p.ProductID == productId);
                                    var productionTaskPM =
                                        gammaBase.ProductionTasks.FirstOrDefault(
                                            p => p.ProductionTaskID == productionTaskID);
                                    if (productionTaskPM != null)
                                    {
                                        productSpool.C1CNomenclatureID = (Guid)productionTaskPM.C1CNomenclatureID;
                                        productSpool.C1CCharacteristicID = (Guid)productionTaskPM.C1CCharacteristicID;
                                    }
                                    gammaBase.SaveChanges();
                                    gammaBase.GenerateNewNumbersForDoc(docProduction.DocID); //Генерация номера документа
                                    MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, productId);
                                    return;
                                }
                                //если предыдущий тамбур этой смены и не подтвержден, то открываем для редактирования
                                if (docProduction != null && docProduction.ShiftID == WorkSession.ShiftID && !docProduction.IsConfirmed)
                                {
                                    var firstOrDefault = gammaBase.DocProductionProducts.FirstOrDefault(d => d.DocID == docProduction.DocID);
                                     if (firstOrDefault !=
                                            null)
                                            productId =
                                                firstOrDefault
                                                    .ProductID;
                                     else
                                     {
                                         break;
                                     }
                                    MessageBox.Show("Предыдущий тамбур не подтвержден. Он будет открыт для редактирования");
                                    MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, productId);
                                    return;
                                }
                                break;
                            case PlaceGroup.Rw:
                                docProductKind = DocProductKinds.DocProductUnload;
                                // Проверка на наличие неподтвержденного съема
                                var notConfirmedDocUnload = gammaBase.Docs.Where(d => d.PlaceID == WorkSession.PlaceID
                                && d.DocTypeID == (byte)DocTypes.DocProduction).OrderByDescending(d => d.Date).FirstOrDefault();
                                if (notConfirmedDocUnload != null && !notConfirmedDocUnload.IsConfirmed)
                                {
                                    MessageBox.Show("Предыдущий съем не подтвержден. Он будет открыт для редактирования");
                                    MessageManager.OpenDocProduct(DocProductKinds.DocProductUnload, notConfirmedDocUnload.DocID);
                                    return;
                                }
                                checkResult = SourceSpoolsCorrect(WorkSession.PlaceID, productionTaskID);
                                break;
                        }
                        if (checkResult == SourceSpoolsCheckResult.Block) return;
                        var sourceIds = gammaBase.GetActiveSourceSpools(WorkSession.PlaceID).ToList();
                        if (
                            gammaBase.Products.Where(p => sourceIds.Contains(p.ProductID))
                                .Any(p => p.StateID == (int) ProductState.Limited))
                        {
                            MessageBox.Show(
                                @"Исходные тамбура ограниченно годны. Возможно потребуется принятие решения по выпущенной продукции.",
                                @"Ограниченно годны", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        MessageManager.CreateNewProduct(docProductKind, productionTaskID, checkResult);
                        break;
                    // Действия для конвертингов
                    case BatchKinds.SGI:
                        checkResult = SourceSpoolsCorrect(WorkSession.PlaceID, productionTaskID);
                        if (checkResult == SourceSpoolsCheckResult.Block) return;
                        var currentDateTime = DB.CurrentDateTime;
                        var productionTask =
                            gammaBase.GetProductionTaskByBatchID(ProductionTaskBatchID,
                                    (short)PlaceGroup.Convertings).FirstOrDefault();
                        if (productionTask == null) return;
                        byte productKindID = (byte)ProductKind.ProductPallet;
                        var reportName = "Амбалаж";
                        if (baseQuantity == null)
                            baseQuantity = (int)(gammaBase.C1CCharacteristics.Where(
                            c => c.C1CCharacteristicID == productionTask.C1CCharacteristicID)
                            .Select(c => c.C1CMeasureUnitsPallet.Coefficient).First() ?? 0);
                        else
                        {
                            productKindID = (byte)ProductKind.ProductPalletR;
                            reportName = "Неполная паллета";
                        }
                        // получаем предыдущий документ в базе
                        var doc = gammaBase.Docs.Include(d => d.DocProduction)
                            .Where(d => d.PlaceID == WorkSession.PlaceID // && d.ShiftID == null
                                                                         //    && d.Date >= SqlFunctions.DateAdd("hh",-12, DB.GetShiftBeginTime(curDate))
                            && d.DocTypeID == (byte)DocTypes.DocProduction).OrderByDescending(d => d.Date)
                            .FirstOrDefault();

                        //Если не указана смена, то переходная паллета. Устанавливаем недостающие свойства и открываем для редактирования
                        Products product;
                        if (doc != null && doc.ShiftID == null)
                        {
                            productId =
                                gammaBase.DocProductionProducts.First(dp => dp.DocID == doc.DocID)
                                    .ProductID;
                            product =
                                gammaBase.Products.Include(p => p.ProductPallets)
                                    .Include(p => p.ProductPallets.ProductItems)
                                    .FirstOrDefault(p => p.ProductID == productId);
                            if (product == null)
                            {
                                product = new Products()
                                {
                                    ProductID = productId,
                                    ProductKindID = productKindID,
                                    ProductPallets = new ProductPallets()
                                    {
                                        ProductID = productId,
                                        ProductItems = new List<ProductItems>
                                        {
                                            new ProductItems()
                                            {
                                                ProductItemID = SqlGuidUtil.NewSequentialid(),
                                                ProductID = productId,
                                                C1CNomenclatureID = (Guid) productionTask.C1CNomenclatureID,
                                                C1CCharacteristicID = (Guid) productionTask.C1CCharacteristicID,
                                                Quantity = baseQuantity
                                            }
                                        }
                                    }
                                };
                                gammaBase.Products.Add(product);
                            }
                            else if (product.ProductPallets == null)
                            {
                                product.ProductPallets = new ProductPallets()
                                {
                                    ProductID = productId,
                                    ProductItems = new List<ProductItems>
                                    {
                                        new ProductItems()
                                        {
                                            ProductItemID = SqlGuidUtil.NewSequentialid(),
                                            ProductID = productId,
                                            C1CNomenclatureID = (Guid) productionTask.C1CNomenclatureID,
                                            C1CCharacteristicID = (Guid) productionTask.C1CCharacteristicID,
                                            Quantity = baseQuantity
                                        }
                                    }
                                };
                            }
                            else if (product.ProductPallets.ProductItems == null)
                            {
                                product.ProductPallets.ProductItems = new List<ProductItems>
                                {
                                    new ProductItems()
                                    {
                                        ProductItemID = SqlGuidUtil.NewSequentialid(),
                                        ProductID = productId,
                                        C1CNomenclatureID = (Guid) productionTask.C1CNomenclatureID,
                                        C1CCharacteristicID = (Guid) productionTask.C1CCharacteristicID,
                                        Quantity = baseQuantity
                                    }
                                };
                            }
                            else
                            {
                                var productItem = product.ProductPallets.ProductItems.First();
                                productItem.C1CNomenclatureID = (Guid) productionTask.C1CNomenclatureID;
                                productItem.C1CCharacteristicID = (Guid) productionTask.C1CCharacteristicID;
                                productItem.Quantity = baseQuantity;
                            }
                            doc.ShiftID = WorkSession.ShiftID;
                            doc.PrintName = WorkSession.PrintName;
                            doc.UserID = WorkSession.UserID;
                            doc.DocProduction.ProductionTaskID = productionTask.ProductionTaskID;
                            var docProductionProduct =
                                gammaBase.DocProductionProducts.FirstOrDefault(dp => dp.DocID == doc.DocID);
                            if (docProductionProduct != null)
                            {
                                docProductionProduct.C1CNomenclatureID = productionTask.C1CNomenclatureID;
                                docProductionProduct.C1CCharacteristicID = productionTask.C1CCharacteristicID;
                                docProductionProduct.Quantity =
                                product.ProductPallets.ProductItems.First().Quantity;
                            }
                            gammaBase.SaveChanges();
                            gammaBase.GenerateNewNumbersForDoc(doc.DocID);
                        }
                        else
                        {
                            product = new Products()
                            {
                                ProductID = productId,
                                ProductKindID = productKindID,
                                ProductPallets = new ProductPallets()
                                {
                                    ProductID = productId,
                                    ProductItems = new List<ProductItems>
                                        {
                                            new ProductItems()
                                            {
                                                ProductItemID = SqlGuidUtil.NewSequentialid(),
                                                ProductID = productId,
                                                C1CNomenclatureID = (Guid) productionTask.C1CNomenclatureID,
                                                C1CCharacteristicID = (Guid) productionTask.C1CCharacteristicID,
                                                Quantity = baseQuantity
                                            }
                                        }
                                }
                            };
                            gammaBase.Products.Add(product);
                            var docID = SqlGuidUtil.NewSequentialid();
                            doc = new Docs
                            {
                                DocID = docID,
                                Date = currentDateTime,
                                DocTypeID = (int)DocTypes.DocProduction,
                                IsConfirmed = false,
                                PlaceID = WorkSession.PlaceID,
                                ShiftID = WorkSession.ShiftID,
                                UserID = WorkSession.UserID,
                                PrintName = WorkSession.PrintName,
                                DocProduction = new DocProduction
                                {
                                    DocID = docID,
                                    InPlaceID = WorkSession.PlaceID,
                                    ProductionTaskID = productionTask.ProductionTaskID,
                                    HasWarnings = checkResult == SourceSpoolsCheckResult.Warning,
                                    DocProductionProducts = new List<DocProductionProducts>
                                    {
                                        new DocProductionProducts
                                        {
                                            DocID = docID,
                                            ProductID = productId,
                                            C1CNomenclatureID = (Guid)productionTask.C1CNomenclatureID,
                                            C1CCharacteristicID = (Guid)productionTask.C1CCharacteristicID,
                                            Quantity = baseQuantity
                                        }
                                    }
                                }
                            };
                            gammaBase.Docs.Add(doc);
                        }
                        var sourceSpools = gammaBase.GetActiveSourceSpools(WorkSession.PlaceID).ToList();
                        foreach (var spoolId in sourceSpools.Where(s => s != null))
                            {
                                var docWithdrawalProduct =
                                    gammaBase.DocWithdrawalProducts.Include(d => d.DocWithdrawal).Include(d => d.DocWithdrawal.Docs)
                                    .Include(d => d.DocWithdrawal.DocProduction)
                                    .FirstOrDefault(d => d.ProductID == spoolId
                                                        && ((d.Quantity == null && (d.CompleteWithdrawal == null || d.CompleteWithdrawal == false)) 
                                                            || (d.Quantity != null 
                                                                && gammaBase.DocCloseShiftRemainders.Any(r => r.ProductID == spoolId && (r.IsSourceProduct ?? false) && r.DocWithdrawalID == d.DocID && r.DocCloseShifts.PlaceID == WorkSession.PlaceID && r.DocCloseShifts.ShiftID == WorkSession.ShiftID 
                                                                        && r.DocCloseShifts.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, DB.CurrentDateTime))) 
                                                                        && r.DocCloseShifts.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, DB.CurrentDateTime)))))));
                                if (docWithdrawalProduct == null)
                                {
                                    var docWithdrawalid = SqlGuidUtil.NewSequentialid();
                                    docWithdrawalProduct = new DocWithdrawalProducts
                                    {
                                        DocID = docWithdrawalid,
                                        ProductID = (Guid)spoolId,
                                        DocWithdrawal = new DocWithdrawal
                                        {
                                            DocID =  docWithdrawalid,
                                            OutPlaceID = WorkSession.PlaceID,
                                            Docs = new Docs()
                                            {
                                                DocID = docWithdrawalid,
                                                Date = currentDateTime,
                                                DocTypeID = (int)DocTypes.DocWithdrawal,
                                                PlaceID = WorkSession.PlaceID,
                                                UserID = WorkSession.UserID,
                                                ShiftID = WorkSession.ShiftID,
                                                PrintName = WorkSession.PrintName,
                                                IsConfirmed = false
                                            }
                                        }
                                    };
                                    gammaBase.DocWithdrawalProducts.Add(docWithdrawalProduct);
                                }
                                if (docWithdrawalProduct.DocWithdrawal.DocProduction == null) docWithdrawalProduct.DocWithdrawal.DocProduction = new List<DocProduction>();
                                docWithdrawalProduct.DocWithdrawal.DocProduction.Add(doc.DocProduction);
                            }
                        gammaBase.SaveChanges();

//#if (!DEBUG)
                        ReportManager.PrintReport(reportName, "Pallet", doc.DocID, false, 1);
//#endif
                        RefreshProduction();
                        break;
                }
            }
        }
        private bool? _statusApplicator;
        public bool? StatusApplicator 
        {
            get { return _statusApplicator; }
            private set 
            {
                _statusApplicator = value;
                ChangeStatusApplicatorText = WorkSession.UseApplicator ? (value != null) ? (bool)value ? "Остановить печать ГЭ" : "Запустить печать ГЭ" : "Обновить статус принтера ГЭ" : "";
                //RaisePropertyChanged("CurrentView");
            }
        }

        private void GetStatusApplicator()
        {
            try
            {
                if (WorkSession.UseApplicator && WorkSession.EndpointAddressOnGroupPackService != null)
                {
                    using (var client = new GammaService.PrinterServiceClient(WorkSession.EndpointConfigurationName,WorkSession.EndpointAddressOnGroupPackService))
                    {
                        StatusApplicator = client.GetPrinterStatus(WorkSession.PlaceID, 2);
                    }
                }
                else
                    StatusApplicator = null;
            }
            catch
            {
                StatusApplicator = null;
                MessageBox.Show("Не удалось получить состояние принтера групповых этикеток", "Принтер групповых этикеток",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }


        private void ChangeStatusApplicator()
        {
            try
            {
                if (WorkSession.EndpointAddressOnGroupPackService != null)
                {
                    using (var client = new GammaService.PrinterServiceClient(WorkSession.EndpointConfigurationName, WorkSession.EndpointAddressOnGroupPackService))
                    {
                        if (StatusApplicator == null)
                            StatusApplicator = client.GetPrinterStatus(WorkSession.PlaceID, 2);
                        else
                        {
                            StatusApplicator = client.ChangePrinterStatus(WorkSession.PlaceID, 2);
                            if (StatusApplicator == null)
                            {
                                MessageBox.Show("Не удалось сменить состояние принтера групповых этикеток", "Принтер групповых этикеток",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                            }
                        }
                    }
                }
            }
            catch
            {
                StatusApplicator = null;
                MessageBox.Show("Не удалось изменить состояние принтера групповых этикеток", "Принтер групповых этикеток",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void PrintGroupPackLabel()
        {
            try
            {
                if (WorkSession.EndpointAddressOnGroupPackService != null)
                {
                    using (var client = new GammaService.PrinterServiceClient(WorkSession.EndpointConfigurationName, WorkSession.EndpointAddressOnGroupPackService))
                    {
                        bool? res = client.PrintLabel(WorkSession.PlaceID, 2, null);
                        if (!res ?? true)
                        {
                            MessageBox.Show("Не удалось распечатать групповую этикетку", "Принтер групповых этикеток",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                        //else
                        //{
                        //    MessageBox.Show("Групповая этикетка распечатана", "Принтер групповых этикеток",
                        //        MessageBoxButton.OK,
                        //        MessageBoxImage.Warning);
                        //}
                    }
                }
            }
            catch
            {
                StatusApplicator = null;
                MessageBox.Show("Не удалось распечатать групповую этикетку", "Принтер групповых этикеток",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        public override bool CanSaveExecute()
        {
            return base.CanSaveExecute() && (CurrentView?.IsValid ?? true) && (CurrentView?.CanSaveExecute() ?? true) && (ProductionTaskStateID != (byte)ProductionTaskStates.OnEditing) &&
                (DB.HaveWriteAccess("ProductionTasks"));
        }
        public DelegateCommand RefreshProductionCommand { get; private set; }
        public DelegateCommand<Guid> ShowProductCommand { get; private set; }
        private ProductInfo _selectedProductionTaskProduct;
        public ProductInfo SelectedProductionTaskProduct
        {
            get
            {
            	return _selectedProductionTaskProduct;
            }
            set
            {
                _selectedProductionTaskProduct = value;
                RaisePropertyChanged("SelectedProductionTaskProduct");
            }
        }

        private void RefreshProduction()
        {

            switch (Intervalid)
            {
                case 0:
                    ProductionTaskProducts = new ItemsChangeObservableCollection<ProductInfo>(from taskProducts in
                                                                               GammaBase.GetBatchProducts(ProductionTaskBatchID)
                                                                                              select new ProductInfo
                                                                                              {
                                                                                                  DocID = taskProducts.DocID,
                                                                                                  ProductKind = (ProductKind)taskProducts.ProductKindID,
                                                                                                  CharacteristicID = taskProducts.CharacteristicID,
                                                                                                  NomenclatureID = taskProducts.NomenclatureID,
                                                                                                  Date = taskProducts.Date,
                                                                                                  NomenclatureName = taskProducts.NomenclatureName,
                                                                                                  Number = taskProducts.Number,
                                                                                                  Quantity = taskProducts.Quantity,
                                                                                                  ProductID = taskProducts.ProductID,
                                                                                                  Place = taskProducts.Place,
                                                                                                  IsConfirmed = taskProducts.IsConfirmed,
                                                                                                  PlaceID = taskProducts.PlaceID,
                                                                                                  ShiftID = taskProducts.ShiftID,
                                                                                                  CurrentPlace = taskProducts.CurrentPlace
                                                                                              });
                    break;
                case 1:
                    ProductionTaskProducts = new ItemsChangeObservableCollection<ProductInfo>(from taskProducts in
                                                                               GammaBase.GetBatchProducts(ProductionTaskBatchID)
                                                                               where taskProducts.ShiftID == WorkSession.ShiftID && taskProducts.Date >= (DateTime)(DB.CurrentDateTime).AddHours(-10)//SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime(DB.CurrentDateTime))
                                                                                              select new ProductInfo
                                                                                              {
                                                                                                  DocID = taskProducts.DocID,
                                                                                                  ProductKind = (ProductKind)taskProducts.ProductKindID,
                                                                                                  CharacteristicID = taskProducts.CharacteristicID,
                                                                                                  NomenclatureID = taskProducts.NomenclatureID,
                                                                                                  Date = taskProducts.Date,
                                                                                                  NomenclatureName = taskProducts.NomenclatureName,
                                                                                                  Number = taskProducts.Number,
                                                                                                  Quantity = taskProducts.Quantity,
                                                                                                  ProductID = taskProducts.ProductID,
                                                                                                  Place = taskProducts.Place,
                                                                                                  IsConfirmed = taskProducts.IsConfirmed,
                                                                                                  PlaceID = taskProducts.PlaceID,
                                                                                                  ShiftID = taskProducts.ShiftID,
                                                                                                  CurrentPlace = taskProducts.CurrentPlace
                                                                                              });
                    break;
                case 2:
                    ProductionTaskProducts = new ItemsChangeObservableCollection<ProductInfo>(from taskProducts in
                                                                               GammaBase.GetBatchProducts(ProductionTaskBatchID)
                                                                               where taskProducts.Date >= (DateTime)(DB.CurrentDateTime).AddHours(-24)//DateTime.Now.AddHours(-24)
                                                                                              select new ProductInfo
                                                                                              {
                                                                                                  DocID = taskProducts.DocID,
                                                                                                  ProductKind = (ProductKind)taskProducts.ProductKindID,
                                                                                                  CharacteristicID = taskProducts.CharacteristicID,
                                                                                                  NomenclatureID = taskProducts.NomenclatureID,
                                                                                                  Date = taskProducts.Date,
                                                                                                  NomenclatureName = taskProducts.NomenclatureName,
                                                                                                  Number = taskProducts.Number,
                                                                                                  Quantity = taskProducts.Quantity,
                                                                                                  ProductID = taskProducts.ProductID,
                                                                                                  Place = taskProducts.Place,
                                                                                                  IsConfirmed = taskProducts.IsConfirmed,
                                                                                                  PlaceID = taskProducts.PlaceID,
                                                                                                  ShiftID = taskProducts.ShiftID,
                                                                                                  CurrentPlace = taskProducts.CurrentPlace
                                                                                              });
                    break;
                default:
                    ProductionTaskProducts = new ItemsChangeObservableCollection<ProductInfo>(from taskProducts in
                                                                              GammaBase.GetBatchProducts(ProductionTaskBatchID)
                                                                                              select new ProductInfo
                                                                                              {
                                                                                                  DocID = taskProducts.DocID,
                                                                                                  ProductKind = (ProductKind)taskProducts.ProductKindID,
                                                                                                  CharacteristicID = taskProducts.CharacteristicID,
                                                                                                  NomenclatureID = taskProducts.NomenclatureID,
                                                                                                  Date = taskProducts.Date,
                                                                                                  NomenclatureName = taskProducts.NomenclatureName,
                                                                                                  Number = taskProducts.Number,
                                                                                                  Quantity = taskProducts.Quantity,
                                                                                                  ProductID = taskProducts.ProductID,
                                                                                                  Place = taskProducts.Place,
                                                                                                  IsConfirmed = taskProducts.IsConfirmed,
                                                                                                  PlaceID = taskProducts.PlaceID,
                                                                                                  ShiftID = taskProducts.ShiftID,
                                                                                                  CurrentPlace = taskProducts.CurrentPlace
                                                                                              });
                    break;
            }
            }
        private void ShowProduct(Guid productID)
        {
            //var productKind = (ProductionTaskProducts != null ? ProductionTaskProducts.FirstOrDefault(p => p.ProductID == productID)?.ProductKind : (ProductKind?)null) ?? (Repacks != null ? Repacks.FirstOrDefault(p => p.ProductID == productID)?.ProductKind : (ProductKind?)null);
            var productKind = Repacks.FirstOrDefault(p => p.ProductID == productID)?.ProductKind ?? ProductionTaskProducts?.FirstOrDefault(p => p.ProductID == productID)?.ProductKind;
            switch (productKind)
            {
                case ProductKind.ProductSpool:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, productID);
                    break;
                case ProductKind.ProductGroupPack:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductGroupPack, productID);
                    break;
                case ProductKind.ProductPallet:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductPallet, productID);
                    break;
                case ProductKind.ProductPalletR:
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductPalletR, productID);
                    break;
                default:
                    //MessageBox.Show("Ошибка программы, действие не предусмотрено");
                    return;
            }
        }

        public bool ExpandProductionTaskProducts { get; private set; }

        private ItemsChangeObservableCollection<ProductInfo> _productionTaskProducts = new ItemsChangeObservableCollection<ProductInfo>();
        public ItemsChangeObservableCollection<ProductInfo> ProductionTaskProducts
        {
            get
            {
                return _productionTaskProducts;
            }
            set
            {
                _productionTaskProducts = value;
                var view = CurrentView as ProductionTaskSGIViewModel;
                if (view != null)
                {
                    view.MadeQuantity =
                        _productionTaskProducts.Where(p => p.IsConfirmed).Sum(p => p.Quantity??0);
                }
                RaisePropertyChanged("ProductionTaskProducts");
            }
        }
        /*        private Visibility _characteristicVisible = Visibility.Visible;
                public Visibility CharacteristicVisible
                {
                    get
                    {
                        return _characteristicVisible;
                    }
                    set
                    {
                        _characteristicVisible = value;
                        RaisePropertyChanged("CharacteristicVisible");
                    }
                }
         * */
        //        [Required(ErrorMessage="Необходимо выбрать характеристику")]
        /*        [UIAuth(UIAuthLevel.ReadOnly)]
                public Guid? CharacteristicID { get; set; }
         * */

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DelegateCommand ActivatedCommand { get; private set; }
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DelegateCommand DeactivatedCommand { get; private set; }

        private byte? _productionTaskStateID;
        // ReSharper disable once MemberCanBePrivate.Global
        public byte? ProductionTaskStateID
        {
            get
            {
                return _productionTaskStateID;
            }
            set
            {
                if (_productionTaskStateID == null)
                {
                    _productionTaskStateID = value;
                    IsEditingComment = (!IsReadOnly) || ProductionTaskStateID == (byte)ProductionTaskStates.OnEditing;
                }
                else
                    if (!(_productionTaskStateID == (byte)ProductionTaskStates.OnEditing && value == (byte)ProductionTaskStates.InProduction) && !(_productionTaskStateID == (byte)ProductionTaskStates.InProduction && value == (byte)ProductionTaskStates.OnEditing) && ((_productionTaskStateID == (byte)ProductionTaskStates.InProduction || _productionTaskStateID == (byte)ProductionTaskStates.Completed) && _productionTaskStateID != value))
                {
                    if (value != null) //для того, чтобы при нажатии кнопки Назад (когда окно открытовнутри основного окна, а не отдельно) не переходило в этот блок. Падает при MessageBox, так как окна уже не существует.
                    {
                        var res = DB.GetAbilityChangeProductionTaskState(ProductionTaskBatchID);
                        if (res == null)
                            MessageBox.Show("Ошибка при получении данных из 1С");
                        else
                        {
                            if ((int)res == 1)
                                _productionTaskStateID = value;
                            else
                                MessageBox.Show("Запрещено изменение. Задание заблокировано, в 1С созданы этапы.");
                        }
                    }
                }
                else
                {
                    _productionTaskStateID = value;
                    IsEditingComment = (!IsReadOnly) || ProductionTaskStateID == (byte)ProductionTaskStates.OnEditing;
                }
                RaisePropertyChanged("ProductionTaskStateID");
            }
        }

        private bool _isEditingComment { get; set; }
        public bool IsEditingComment
        {
            get { return _isEditingComment; }
            set
            {
                if (_isEditingComment == value) return;
                _isEditingComment = value;
                if (CurrentView is IProductionTaskBatch)
                {
                    (CurrentView as IProductionTaskBatch).OnEditingStatus = IsEditingComment;
                }
                if (CurrentView is IProductionTask)
                {
                    (CurrentView as IProductionTask).IsEditingQuantity = IsEditingComment;
                }
                RaisePropertyChanged("IsEditingComment");
            }
        }           

        public Dictionary<byte, string> TaskStates { get; set; }
        public Dictionary<byte, string> ProcessModels { get; set; }
        public bool ChangeStateReadOnly { get; set; }
        public string Title { get; private set; }

        public List<string> Intervals { get; set; }
        private int _intervalid;

        public int Intervalid
        {
            get { return _intervalid; }
            set
            {
                if (_intervalid == value) return;
                _intervalid = value < 0 ? 0 : value;
                if (_intervalid < 3) RefreshProduction();
            }
        }

        private SourceSpoolsCheckResult SourceSpoolsCorrect(int placeId, Guid productionTaskId)
        {
            using (var gammaBase = DB.GammaDb)
            {
                var sourceSpools = gammaBase.GetActiveSourceSpools(placeId).ToList();
                if (sourceSpools.Count == 0)
                {
                    MessageBox.Show("Нет активных раскатов");
                    return SourceSpoolsCheckResult.Block;
                }
                /*
                if (!gammaBase.ProductionTaskRWCutting.Any(ptrw => ptrw.ProductionTasks.ProductionTaskBatches.FirstOrDefault().ProductionTaskBatchID == ProductionTaskBatchID))
                {
                    MessageBox.Show("В задании не указан раскрой");
                    return false;
                }
                */
                var result = gammaBase.CheckProductionTaskSourceSpools(placeId, productionTaskId).First();
                var resultMessage = result.ResultMessage;
                if (string.IsNullOrWhiteSpace(resultMessage) && !result.BlockCreation)
                    return SourceSpoolsCheckResult.Correct;
                if (result.BlockCreation)
                {
                    MessageBox.Show(resultMessage, "Проверка исходных тамбуров", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return SourceSpoolsCheckResult.Block;
                }
                var dialogResult = MessageBox.Show(resultMessage, "Проверка исходных тамбуров", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                return dialogResult == MessageBoxResult.Yes ? SourceSpoolsCheckResult.Warning : SourceSpoolsCheckResult.Block;
            }
        }

        public void CheckGroupAndTransportPackLabel(Guid productionTaskId, string endpointAddress)
        {
            try
            {
                var view = CurrentView as ProductionTaskSGIViewModel;
                if (view != null)
                {
                    if ((endpointAddress) != null && endpointAddress != String.Empty && WorkSession.LabelPath != null)
                    {
                        using (var client = new GammaService.PrinterServiceClient(WorkSession.EndpointConfigurationName, endpointAddress))
                        {
                            //if (!client.UpdateGroupPackageLabelInProductionTask(productionTaskId))
                            var result = client.UpdateGroupPackLabelInProductionTask(productionTaskId);
                            if (!result.Item1)
                            {
                                //MessageBox.Show("Не удалось обновить этикетку групповой упаковки в задании", "Аппликатор",
                                MessageBox.Show(result.Item2, "Аппликатор",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                            }
                        }
                    }
                    view.UpdateGroupPackageLabelImage(productionTaskId);
                }
            }
            catch
            {
                MessageBox.Show("Техническая проблема при обновлении этикетки упаковки в задании: сервис недоступен. Обратитесь в техподдержку Гаммы.", "Аппликатор",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        public DelegateCommand AddSampleCommand { get; private set; }
        public DelegateCommand ShowSampleCommand { get; private set; }
        public DelegateCommand DeleteSampleCommand { get; private set; }

        private int _sampleIntervalid;

        public int SampleIntervalid
        {
            get { return _sampleIntervalid; }
            set
            {
                if (_sampleIntervalid == value) return;
                _sampleIntervalid = value < 0 ? 0 : value;
                if (_sampleIntervalid < 3) RefreshSample();
            }
        }
        public bool IsEnabledSamples { get; set; } = false;

        private ItemsChangeObservableCollection<Sample> _samples = new ItemsChangeObservableCollection<Sample>();
        public ItemsChangeObservableCollection<Sample> Samples
        {
            get
            {
                return _samples;
            }
            set
            {
                _samples = value;
                RaisePropertyChanged("Samples");
            }
        }

        private Sample _selectedSample;
        public Sample SelectedSample
        {
            get
            {
                return _selectedSample;
            }
            set
            {
                _selectedSample = value;
                RaisePropertyChanged("SelectedSample");
            }
        }

        private void RefreshSample()
        {
            Samples = new ItemsChangeObservableCollection<Sample>
                (from sample in GammaBase.GetBatchSamples(ProductionTaskBatchID, SampleIntervalid)
                    select new Sample
                    {
                        ProductionTaskConvertingSampleID = sample.ProductionTaskConvertingSampleID,
                        NomenclatureID = sample.C1CNomenclatureID,
                        CharacteristicID = sample.C1CCharacteristicID,
                        Date = sample.Date,
                        ShiftID = sample.ShiftID,
                        NomenclatureName = sample.NomenclatureName,
                        Quantity = sample.Quantity,
                        MeasureUnitId = sample.C1CMeasureUnitID,
                        MeasureUnit = sample.MeasureUnitName
                    });
        }
        private void ShowSample()
        {
            MessageBox.Show(SelectedSample?.Date.ToString());
        }

        private void AddSample()
        {
            var model = new SetQuantityDialogModel("Укажите кол-во в рулончиках (или пачках для салфеток)", "Отобранные образцы", 1, 1000);
            var okCommand = new UICommand()
            {
                Caption = "OK",
                IsCancel = false,
                IsDefault = true,
                Command = new DelegateCommand<CancelEventArgs>(
            x => DebugFunc(),
            x => model.Quantity > 0 && model.Quantity < 1000),
            };
            var cancelCommand = new UICommand()
            {
                Id = MessageBoxResult.Cancel,
                Caption = "Отмена",
                IsCancel = true,
                IsDefault = false,
            };
            var dialogService = GetService<IDialogService>("SetQuantityDialog");
            var result = dialogService.ShowDialog(
                dialogCommands: new List<UICommand>() { okCommand, cancelCommand },
                title: "Отобранные образцы",
                viewModel: model);
            if (result == okCommand)
            {
                string addResult = "";
                if (DB.HaveWriteAccess("ProductionTaskConvertingSamples"))
                {
                    addResult = GammaBase.CreateSample(ProductionTaskBatchID, model.Quantity).FirstOrDefault();
                }
                else
                {
                    MessageBox.Show("Недостаточно прав для добавления!");
                }

                if (addResult != "")
                {
                    MessageBox.Show(addResult, "Добавить не удалось", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
                else
                    RefreshSample();
            }
        }

        private void DeleteSample()
        {
            if (SelectedSample == null) return;
            if (WorkSession.ShiftID != 0 && (SelectedSample.ShiftID != WorkSession.ShiftID))
            {
                MessageBox.Show("Вы не можете удалить отобранные образцы другой смены");
                return;
            }
            if (MessageBox.Show(
                "Вы уверены, что хотите удалить отобранные образцы от " + SelectedSample.Date + " смена " + SelectedSample.ShiftID + "?",
                "Удаление отобранных образцов", MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.Yes) != MessageBoxResult.Yes)
            {
                return;
            };
            string delResult = "";
            if (DB.HaveWriteAccess("ProductionTaskConvertingSamples"))
            {
                delResult = GammaBase.DeleteSample(SelectedSample.ProductionTaskConvertingSampleID).FirstOrDefault();
            }
            else
            {
                MessageBox.Show("Недостаточно прав для удаления!");
            }

            if (delResult != "")
            {
                MessageBox.Show(delResult, "Внимание!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            RefreshSample();
        }

        public DelegateCommand AddRepackCommand { get; private set; }
        public DelegateCommand ShowRepackCommand { get; private set; }
        //public DelegateCommand DeleteRepackCommand { get; private set; }
        public DelegateCommand RefreshRepackCommand { get; private set; }

        private int _RepackIntervalid;

        public int RepackIntervalid
        {
            get { return _RepackIntervalid; }
            set
            {
                if (_RepackIntervalid == value) return;
                _RepackIntervalid = value < 0 ? 0 : value;
                if (_RepackIntervalid < 3) RefreshRepack();
            }
        }
        public bool IsEnabledRepack { get; set; } = false;

        private ItemsChangeObservableCollection<RepackProduct> _Repacks = new ItemsChangeObservableCollection<RepackProduct>();
        public ItemsChangeObservableCollection<RepackProduct> Repacks
        {
            get
            {
                return _Repacks;
            }
            set
            {
                _Repacks = value;
                RaisePropertyChanged("Repacks");
            }
        }

        private RepackProduct _selectedRepack;
        public RepackProduct SelectedRepack
        {
            get
            {
                return _selectedRepack;
            }
            set
            {
                _selectedRepack = value;
                RaisePropertyChanged("SelectedRepack");
            }
        }

        private void RefreshRepack()
        {
            Repacks = new ItemsChangeObservableCollection<RepackProduct>
                (from Repack in GammaBase.GetBatchRepackProducts(ProductionTaskBatchID, RepackIntervalid)
                 select new RepackProduct
                 {
                     DocRepackID = Repack.DocRepackID,
                     DocRepackNumber = Repack.DocRepackNumber,
                     ProductID = Repack.ProductID,
                     ProductNumber = Repack.ProductNumber,
                     NomenclatureID = Repack.C1CNomenclatureID,
                     CharacteristicID = Repack.C1CCharacteristicID,
                     NomenclatureName = Repack.NomenclatureName,
                     Date = Repack.Date,
                     Quantity = Repack.Quantity ?? 0,
                     QuantityGood = Repack.QuantityGood,
                     QuantityBroke = Repack.QuantityBroke,
                     ShiftID = Repack.ShiftID ?? 0,
                     ProductionTaskID = Repack.ProductionTaskID,
                     DocBrokeID = Repack.DocBrokeID,
                     DocBrokeNumber = Repack.DocBrokeNumber,
                     ProductKind = (ProductKind)Repack.ProductKindID
        });            
        }
        private void ShowRepack()
        {
            if (SelectedRepack?.DocRepackID != null)
                MessageManager.OpenDocRepack(SelectedRepack.DocRepackID);
        }

        private void AddRepack()
        {
            MessageManager.OpenDocRepack(ProductionTaskBatchID);
            RefreshRepack();
        }

        /*private void DeleteRepack()
        {
            if (SelectedRepack == null) return;
            if (WorkSession.ShiftID != 0 && (SelectedRepack.ShiftID != WorkSession.ShiftID))
            {
                MessageBox.Show("Вы не можете удалить переупакованную продукцию другой смены");
                return;
            }
            if (MessageBox.Show(
                "Вы уверены, что хотите удалить переупакованную продукцию от " + SelectedRepack.Date + " смена " + SelectedRepack.ShiftID + "?",
                "Удаление переупакованной продукции", MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.Yes) != MessageBoxResult.Yes)
            {
                return;
            };
            
            RefreshRepack();
        }*/

        public DelegateCommand AddDowntimeCommand { get; private set; }
        public DelegateCommand ShowDowntimeCommand { get; private set; }
        public DelegateCommand DeleteDowntimeCommand { get; private set; }
        public DelegateCommand RefreshDowntimeCommand { get; private set; }

        private int _downtimeIntervalid;

        public int DowntimeIntervalid
        {
            get { return _downtimeIntervalid; }
            set
            {
                if (_downtimeIntervalid == value) return;
                _downtimeIntervalid = value < 0 ? 0 : value;
                if (_downtimeIntervalid < 3) RefreshDowntime();
            }
        }
        public bool IsEnabledDowntimes { get; set; } = false;

        private ItemsChangeObservableCollection<Downtime> _downtimes = new ItemsChangeObservableCollection<Downtime>();
        public ItemsChangeObservableCollection<Downtime> Downtimes
        {
            get
            {
                return _downtimes;
            }
            set
            {
                _downtimes = value;
                RaisePropertyChanged("Downtimes");
            }
        }

        private Downtime _selectedDowntime;
        public Downtime SelectedDowntime
        {
            get
            {
                return _selectedDowntime;
            }
            set
            {
                _selectedDowntime = value;
                RaisePropertyChanged("SelectedDowntime");
            }
        }

        private void RefreshDowntime()
        {
            Downtimes = new ItemsChangeObservableCollection<Downtime>
                (from downtime in GammaBase.GetBatchDowntimes(ProductionTaskBatchID, DowntimeIntervalid)
                 select new Downtime
                 {
                     ProductionTaskConvertingDowntimeID = downtime.ProductionTaskDowntimeID,
                     DowntimeTypeID = downtime.C1CDowntimeTypeID,
                     DowntimeTypeDetailID = downtime.C1CDowntimeTypeDetailID,
                     Date = downtime.Date,
                     ShiftID = downtime.ShiftID,
                     DowntimeType = downtime.DowntimeType,
                     DowntimeTypeDetail = downtime.DowntimeTypeDetail,
                     Duration = downtime.Duration,
                     Comment = downtime.Comment,
                     DateBegin = downtime.DateBegin,
                     DateEnd = downtime.DateEnd,
                     EquipmentNodeID = downtime.C1CEquipmentNodeID,
                     EquipmentNodeDetailID = downtime.C1CEquipmentNodeDetailID,
                     EquipmentNode = downtime.EquipmentNode,
                     EquipmentNodeDetail = downtime.EquipmentNodeDetail
                 });
        }
        private void ShowDowntime()
        {
            MessageBox.Show(SelectedDowntime?.Date.ToString());
        }

        private void AddDowntime(AddDowntimeParameter downtime)
        {
            AddDowntime(downtime.DowntimeTypeID, downtime.DowntimeTypeDetailID, downtime.EquipmentNodeID, downtime.EquipmentNodeDetailID, downtime.Comment);
        }

        private void AddDowntime()
        {
            AddDowntime(null,null);
        }

        private void AddDowntime(Guid? downtimeTypeID, Guid? downtimeTypeDetailID = null, Guid? equipmentNodeID = null, Guid? equipmentNodeDetailID = null, string comment = null)
        {
            var model = new AddDowntimeDialogModel(downtimeTypeID, downtimeTypeDetailID, equipmentNodeID, equipmentNodeDetailID, comment);
            var setCurrentTimeEndAndOkCommand = new UICommand()
            {
                Caption = "Сохранить текущим временем окончания",
                IsCancel = false,
                IsDefault = false,
                Command = new DelegateCommand<CancelEventArgs>(
            x => DebugFunc(),
            x => model.IsValid && (model.DateEnd - model.DateBegin).TotalMinutes == 0),
            };
            var okCommand = new UICommand()
             {
                 Caption = "Сохранить",
                 IsCancel = false,
                 IsDefault = true,
                 Command = new DelegateCommand<CancelEventArgs>(
             x => DebugFunc(),
             x => model.IsValid && (model.DateEnd - model.DateBegin).TotalMinutes > 0 && (model.DateEnd - model.DateBegin).TotalMinutes <= 14 * 60),
             };
             var cancelCommand = new UICommand()
             {
                 Id = MessageBoxResult.Cancel,
                 Caption = "Отмена",
                 IsCancel = true,
                 IsDefault = false,
             };
             var dialogService = GetService<IDialogService>("AddDowntimeDialog");
             var result = dialogService.ShowDialog(
                 dialogCommands: new List<UICommand>() { setCurrentTimeEndAndOkCommand, okCommand, cancelCommand },
                 title: "Добавление простоя",
                 viewModel: model);
             if (result == okCommand || result == setCurrentTimeEndAndOkCommand)
            //var dialog = new AddDowntimeDialog();
            //dialog.ShowDialog();
            //if (dialog.DialogResult == true)
            {
                string addResult = "";
                if (DB.HaveWriteAccess("ProductionTaskDowntimes"))
                {
                    addResult = GammaBase.CreateDowntime(ProductionTaskBatchID, null, model.TypeID, model.TypeDetailID, model.DateBegin, result == setCurrentTimeEndAndOkCommand ? DateTime.Now : model.DateEnd, model.Comment, model.EquipmentNodeID, model.EquipmentNodeDetailID).FirstOrDefault();
                    //addResult = GammaBase.CreateDowntime(ProductionTaskBatchID, null, dialog.TypeID, dialog.TypeDetailID, dialog.DateBegin, dialog.DateEnd, dialog.Comment).FirstOrDefault();
                }
                else
                {
                    MessageBox.Show("Недостаточно прав для добавления!");
                }

                if (addResult != "")
                {
                    MessageBox.Show(addResult, "Добавить не удалось", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
                else
                    RefreshDowntime();
            }
        }

        private void DeleteDowntime()
        {
            if (SelectedDowntime == null) return;
            if (WorkSession.ShiftID != 0 && (SelectedDowntime.ShiftID != WorkSession.ShiftID))
            {
                MessageBox.Show("Вы не можете удалить простои другой смены");
                return;
            }
            if (MessageBox.Show(
                "Вы уверены, что хотите удалить простой от " + SelectedDowntime.Date + " смена " + SelectedDowntime.ShiftID + "?",
                "Удаление простоев", MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.Yes) != MessageBoxResult.Yes)
            {
                return;
            };
            string delResult = "";
            if (DB.HaveWriteAccess("ProductionTaskDowntimes"))
            {
                delResult = GammaBase.DeleteDowntime(SelectedDowntime.ProductionTaskConvertingDowntimeID).FirstOrDefault();
            }
            else
            {
                MessageBox.Show("Недостаточно прав для удаления!");
            }

            if (delResult != "")
            {
                MessageBox.Show(delResult, "Внимание!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            RefreshDowntime();
        }

    }

}