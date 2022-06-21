// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;
using Gamma.Interfaces;
using System.Linq;
using System.Data.Entity;
using Gamma.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Entities;
using Gamma.DialogViewModels;
using System.ComponentModel;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public sealed class DocProductSpoolViewModel : DbEditItemWithNomenclatureViewModel, IBarImplemented, ICheckedAccess, IProductValidate
    {
        /// <summary>
        /// Инициализация информации о тамбуре
        /// </summary>
        /// <param name="productId">ID продукта</param>
        public DocProductSpoolViewModel(Guid productId, Guid docId, bool docIsReadOnly, Guid? productionTaskID = null, int? docPlaceID = null)
        {
            ShowCreateGroupPack = (WorkSession.PlaceID == 21 || WorkSession.PlaceID == 30);
            ToughnessKinds = new ToughnessKinds().ToDictionary();
            Messenger.Default.Register<DocChangedMessage>(this, DocChanged);
            Bars.Add(ReportManager.GetReportBar("Spool", VMID));
            CreateGroupPackCommand = new DelegateCommand(CreateGroupPack, () => IsValid);
            GetWeightCommand = new DelegateCommand(GetWeight, () => Scales.IsReady && !IsReadOnly);
            SetWeightCommand = new DelegateCommand(SetWeight, () => RestWeight > 0 && (WorkSession.RoleName == "Dispetcher" || WorkSession.RoleName == "WarehouseOperator"));
            var permissionOnChooseNomenclatureAndCharacteristic = GammaBase.CheckPermissionOnChooseNomenclatureAndCharacteristic((int)ProductKind.ProductSpool, docPlaceID, WorkSession.UserID).FirstOrDefault();
            if (permissionOnChooseNomenclatureAndCharacteristic != null)
            {
                GrantPermissionOnChooseNomenclatureAndCharacteristic = (bool)permissionOnChooseNomenclatureAndCharacteristic;
            }
            else
            {
                GrantPermissionOnChooseNomenclatureAndCharacteristic = false;
            }
            UpdateViewModel(productId, docId, docIsReadOnly, productionTaskID, docPlaceID);
        }

        public void UpdateViewModel(Guid productId, Guid docId, bool docIsReadOnly, Guid? productionTaskID = null, int? docPlaceID = null)
        {
            StartInitialization();
            ProductId = productId;
            DocID = docId; //GammaBase.DocProductionProducts.FirstOrDefault(d => d.ProductID == productId)?.DocID ??
                            //SqlGuidUtil.NewSequentialid();
            //var doc = GammaBase.Docs.Include(d => d.DocProduction).FirstOrDefault(d => d.DocID == DocID);
            //            States = new ProductState().ToDictionary();
            /*
            RejectionReasons = new ObservableCollection<RejectionReason>
                (from r in GammaBase.GetSpoolRejectionReasons()
                 select new RejectionReason
                 {
                     RejectionReasonID = (Guid)r.RejectionReasonID,
                     Description = r.Description,
                     FullDescription = r.FullDescription
                 });
            */
            var productSpool = GammaBase.ProductSpools.Include(p => p.Products.DocProductionProducts)
                .FirstOrDefault(p => p.ProductID == ProductId);
            if (productSpool == null)
            {
                DB.AddLogMessageInformation("Загрузка данных: новый продукт тамбур",
                        "UpdateViewModel in DocProductSpoolViewModel: productSpool == null", docId, productId);
                var productionTask =
                    GammaBase.ProductionTasks.FirstOrDefault(
                        //pt => pt.ProductionTaskID == doc.DocProduction.ProductionTaskID);
                        pt => pt.ProductionTaskID == productionTaskID);
                if (productionTask != null)
                {
                    NomenclatureID = productionTask.C1CNomenclatureID;
                    CharacteristicID = productionTask.C1CCharacteristicID;
                }
                else
                {
                    Functions.ShowMessageInformation("Не удалось получить информацию о задании, выберите номенклатуру вручную",
                        "Open DocProductSpoolViewModel: Job information could not be retrieved", docId, productId);
                }
                RealFormat = DB.GetLastFormat(WorkSession.PlaceID);
                SetIsChanged(true);
            }
            else
            {
                DB.AddLogMessageInformation("Загрузка данных: существующий продукт тамбур",
                        "UpdateViewModel in DocProductSpoolViewModel: productSpool == null", docId, productId);
                NomenclatureID = productSpool.C1CNomenclatureID;
                CharacteristicID = productSpool.C1CCharacteristicID;
                RealFormat = productSpool.RealFormat;
                RealBasisWeight = productSpool.RealBasisWeight;
                Diameter = productSpool.Diameter;
                CurrentDiameter = productSpool.CurrentDiameter ?? 0;
                Length = productSpool.Length ?? 0;
                CurrentLength = productSpool.CurrentLength ?? 0;
                BreakNumber = productSpool.BreakNumber;
                var productionQuantity = productSpool.Products.DocProductionProducts.FirstOrDefault()?.Quantity??0;
                Weight = productionQuantity*1000; // перевод в кг
                RestWeight = productSpool.DecimalWeight*1000;
                ToughnessKindID = productSpool.ToughnessKindID ?? 1;
            }
            //IsConfirmed = docIsConfirmed && IsValid;
            CalculateIsReadOnly(docIsReadOnly);
            //AllowEditProduct = DB.AllowEditProduct(ProductId, GammaBase);

            EndInitialization();
        }

        private void CalculateIsReadOnly(bool isReadOnly)
        {
            IsReadOnly = isReadOnly || !DB.HaveWriteAccess("ProductSpools") || !DB.AllowEditProduct(ProductId, GammaBase);
            RaisePropertiesChanged("IsReadOnly");
        }

        private void DocChanged(DocChangedMessage msg)
        {
            if (DocID != msg.DocId) return;
            CalculateIsReadOnly(msg.IsConfirmed);
        }

        public bool IsReadOnlyCharacteristic = true;

        private decimal? CoreDiameter { get; set; }

        //[UIAuth(UIAuthLevel.ReadOnly)]
        //[Required(ErrorMessage = @"Необходимо выбрать характеристику")]
        public override Guid? CharacteristicID
        {
            get { return base.CharacteristicID; }
            set
            {
                base.CharacteristicID = value;
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Выбрана характеристика " + value.ToString(),
                        "SET CharacteristicID in DocProductSpoolViewModel: Selected characteristic" + value.ToString(), DocID, ProductId);
                }
                if (value == null) return;
                using (var gammaBase = DB.GammaDbWithNoCheckConnection)
                {
                    CoreDiameter =
                        gammaBase.vCharacteristicSGBProperties.FirstOrDefault(
                            c => c.C1CCharacteristicID == CharacteristicID)?.CoreDiameterNumeric;
                }
            }
        }

        public string Density
        {
            get { return _density; }
            set
            {
                _density = value;
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменена Плотность рулона " + value.ToString(),
                        "SET Density in DocProductSpoolViewModel: set value " + value.ToString(), DocID, ProductId);
                }
                RaisePropertyChanged("Density");
            }
        }

        public decimal CurrentLength
        {
            get { return _currentLength; }
            set
            {
                _currentLength = value;
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменена Текущая длина " + value.ToString(),
                        "SET CurrentLength in DocProductSpoolViewModel: set value " + value.ToString(), DocID, ProductId);
                }
                RaisePropertyChanged("CurrentLength");
            }
        }

        

        public bool ShowCreateGroupPack { get; private set; }

        private bool DiameterIsChanged = false;

        [UIAuth(UIAuthLevel.ReadOnly)]
        [Range(1,5000,ErrorMessage = @"Необходимо указать диаметр")]
        
        public int Diameter
        {
            get
            {
                return _diameter;
            }
            set
            {
                _diameter = value;
                GetDensity(Weight, CoreDiameter, Diameter, RealFormat);
                RaisePropertyChanged("Diameter");
                if (!GetInitialization)
                {
                    DiameterIsChanged = true;
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменен диаметр " + value.ToString() + " в ProductID",
                        "SET Diameter in DocProductSpoolViewModel: Set value = " + value.ToString(), DocID, ProductId);
                }
            }
        }

        public int CurrentDiameter
        {
            get { return _currentDiameter; }
            set
            {
                _currentDiameter = value;
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменена текущая длина " + value.ToString() + " в ProductID",
                            "SET CurrentDiameter in DocProductSpoolViewModel: Set value = " + value.ToString(), DocID, ProductId);
                }
                RaisePropertyChanged("CurrentDiameter");
            }
        }

        private bool LengthIsChanged = false;

        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal? Length
        {
            get
            {
                return _length;
            }
            set
            {
                _length = value;
                RaisePropertyChanged("Length");
                if (!GetInitialization)
                {
                    LengthIsChanged = true;
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменена длина " + value.ToString() + " в ProductID",
                            "SET Length in DocProductSpoolViewModel: Set value = " + value.ToString(), DocID, ProductId);
                }
            }
        }

        public DelegateCommand GetWeightCommand { get; private set; }
        public DelegateCommand SetWeightCommand { get; private set; }

        public DelegateCommand CreateGroupPackCommand { get; private set; }

        private void CreateGroupPack()
        {
            DB.AddLogMessageInformation("Нажатие кнопки Cоздать упаковку в продукте ProductID",
                "Start CreateGroupPack in DocProductSpoolViewModel", DocID, ProductId);
            SaveToModel(DocID);
            var groupPackId = GammaBase.CreateGroupPackWithSpool(ProductId, WorkSession.PrintName).FirstOrDefault();
            if (groupPackId != null)
                MessageManager.OpenDocProduct(DocProductKinds.DocProductGroupPack, (Guid) groupPackId);
            else
            {
                Functions.ShowMessageError("Ошибка! Вы пытаетесь создать упаковку из тамбура, который уже переработан",
                    "Error CreateGroupPack in DocProductSpoolViewModel: Spool is worked", DocID, ProductId);
            }
        }

        private Guid DocID { get; set; }

        protected override void NomenclatureChanged(Nomenclature1CMessage msg)
        {
            base.NomenclatureChanged(msg);
            SetIsChanged(true);
            Characteristics = DB.GetCharacteristics(NomenclatureID);
        }
        private decimal? _realBasisWeight;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal? RealBasisWeight
        {
            get { return _realBasisWeight; }
            set
            {
                _realBasisWeight = value;
                RaisePropertyChanged("RealBasisWeight");
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменена масса фактич " + value.ToString() + " в ProductID",
                            "SET RealBasisWeight in DocProductSpoolViewModel: Set value = " + value.ToString(), DocID, ProductId);
                }
            }
        }
        private int? _realFormat;
        [UIAuth(UIAuthLevel.ReadOnly)]
        [Range(100,5000,ErrorMessage=@"Необходимо указать реальный формат")]
        public int? RealFormat
        {
            get
            {
                return _realFormat;
            }
            set
            {
                _realFormat = value;
                GetDensity(Weight, CoreDiameter, Diameter, RealFormat);
                RaisePropertyChanged("RealFormat");
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменен фактич. формат " + value.ToString() + " в ProductID",
                            "SET RealFormat in DocProductSpoolViewModel: Set value = " + value.ToString(), DocID, ProductId);
                }
            }
        }
        private byte? _breakNumber;
        [UIAuth(UIAuthLevel.ReadOnly)]
        [Required(ErrorMessage = @"Необходимо указать количество обрывов")]
        public byte? BreakNumber
        {
            get
            {
                return _breakNumber;
            }
            set
            {
                _breakNumber = value;
                RaisePropertyChanged("BreakNumber");
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменено кол-во обрывов " + value.ToString() + " в ProductID",
                            "SET BreakNumber in DocProductSpoolViewModel: value = " + value.ToString(), DocID, ProductId);
                }
            }
        }

        /*private bool _isConfirmed { get; set; }
        private bool IsConfirmed
        {
            get
            {
                return _isConfirmed;
            }
            set
            {
                _isConfirmed = value;
                RaisePropertyChanged("IsConfirmed");
                RaisePropertyChanged("IsReadOnly");
                //RaisePropertyChanged("CannotChooseCharacteristic");
            }
        }*/
        private decimal _weight;   
        [Range(1,10000,ErrorMessage=@"Укажите вес тамбура")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal Weight
        {
            get
            {
                return _weight;
            }
            set
            {
                _weight = value;
                GetDensity(Weight, CoreDiameter, Diameter, RealFormat);
                RaisePropertyChanged("Weight");
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменен вес " + value.ToString() + " в ProductID",
                            "SET Weight in DocProductSpoolViewModel: value = " + value.ToString(), DocID, ProductId);
                }
            }
        }

        private void GetDensity(decimal? weight, decimal? coreDiameter, decimal? diameter, decimal? format)
        {
            if (weight == null || coreDiameter == null || diameter == null || format == null)
            {
                Density = "н/д";
                return;
            }
            var density = Functions.GetDensity((double) weight, (double) coreDiameter, (double) diameter,
                (double) format);
            Density = density == null ? "н/д" : ((int) density).ToString();
        }


        private decimal _restWeight;

        /// <summary>
        /// Остаточный вес тамбура (текущий)
        /// </summary>
        public decimal RestWeight
        {
            get { return _restWeight; }
            set
            {
                _restWeight = value;
                RaisePropertyChanged("RestWeight");
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменен Остаточный вес тамбура (текущий) " + value.ToString() + " в ProductID",
                            "SET RestWeight in DocProductSpoolViewModel: value = " + value.ToString(), DocID, ProductId);
                }
            }
        }

        private decimal? _length;
        private int _diameter;
        private ObservableCollection<BarViewModel> _bars = new ObservableCollection<BarViewModel>();
        public ObservableCollection<BarViewModel> Bars
        {
            get
            {
                return _bars;
            }
            set
            {
                _bars = value;
                RaisePropertyChanged("Bars");
            }
        }

        //private bool AllowEditProduct { get; set; }

//        public bool WeightReadOnly => !AllowEditProduct || IsReadOnly;

        public Guid? VMID { get; } = Guid.NewGuid();

        //        private DocProductionProducts DocProductionProduct { get; set; }

        /// <summary>
        /// ID продукта, используется для печати амбалажа
        /// </summary>
        public Guid ProductId { get; private set; }

        public bool IsValidProduct => ValidateProduct();

        public bool ValidateProduct()
        {
            if (Weight > 1)
            {
                var result = GammaBase.ValidateSpoolBeforeSave(NomenclatureID, CharacteristicID, Diameter, Weight, RealFormat).FirstOrDefault();
                if (!string.IsNullOrEmpty(result))
                {
                    Functions.ShowMessageInformation(result, "ValidateSpoolBeforeSave return false: result is not null", DocID, ProductId);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Сохранение в базу
        /// </summary>
        /// <param name="itemID">ID документа</param>
        public override bool SaveToModel(Guid itemID)
        {
            DB.AddLogMessageInformation("Начало сохранения продукта тамбур",
                "Start SaveToModel in DocProductSpoolViewModel", DocID, ProductId);
            if (!IsChanged)
            {
                DB.AddLogMessageInformation("Успешный выход без сохранения продукта тамбур", "Quit successed from SaveToModel in DocProductSpoolViewModel: IsChanged = " + IsChanged.ToString(), DocID);
                return true;
            }
            if (!IsValidProduct)
            {
                DB.AddLogMessageInformation("Ошибочный выход без сохранения продукта тамбур", "Quit failed from SaveToModel in DocProductSpoolViewModel: IsValidProduct = " + IsValidProduct.ToString(), DocID);
                return false;
            }
            var product =
                GammaBase.Products.Include(p => p.ProductSpools).Include(p => p.DocProductionProducts)
                    .FirstOrDefault(p => p.ProductID == ProductId);
            if (product == null)
            {
                var id = SqlGuidUtil.NewSequentialid();
                product = new Products
                {
                    ProductID = id,
                    ProductKindID = (int)ProductKind.ProductSpool,
                };
                GammaBase.Products.Add(product);
            }
            if (product.ProductSpools == null)
            {
                product.ProductSpools = new ProductSpools
                {
                    ProductID = product.ProductID
                };
            }
            if (product.DocProductionProducts == null || product.DocProductionProducts.Count == 0)
            {
                product.DocProductionProducts = new List<DocProductionProducts>
                {
                    new DocProductionProducts
                    {
                        ProductID = product.ProductID,
                        DocID = itemID,
                        Quantity = Weight/1000,
                        C1CNomenclatureID = (Guid)NomenclatureID,
                        C1CCharacteristicID = (Guid)CharacteristicID
                    }
                };
            }

            //if (AllowEditProduct)
            {
                product.ProductSpools.C1CNomenclatureID = (Guid)NomenclatureID;
                product.ProductSpools.C1CCharacteristicID = (Guid)CharacteristicID;
                product.ProductSpools.DecimalWeight = Weight / 1000;
                var docProductionProduct = product.DocProductionProducts.FirstOrDefault();
                if (docProductionProduct != null)
                {
                    docProductionProduct.Quantity = Weight / 1000;
                    docProductionProduct.C1CNomenclatureID = (Guid)NomenclatureID;
                    docProductionProduct.C1CCharacteristicID = (Guid)CharacteristicID;
                }
            }
            product.ProductSpools.RealBasisWeight = RealBasisWeight;
            product.ProductSpools.RealFormat = RealFormat;
            product.ProductSpools.BreakNumber = BreakNumber;
            product.ProductSpools.Diameter = Diameter;
            product.ProductSpools.Length = Length;
            if (DiameterIsChanged) product.ProductSpools.CurrentDiameter = Diameter;
            if (LengthIsChanged) product.ProductSpools.CurrentLength = Length;

            product.ProductSpools.ToughnessKindID = ToughnessKindID;
            GammaBase.SaveChanges();
            SetIsChanged(false);
            DB.AddLogMessageInformation("Успешное окончание сохранения продукта тамбур", "End SaveToModel in DocProductSpoolViewModel: success", DocID);
            return true;
        }

        public bool GrantPermissionOnChooseNomenclatureAndCharacteristic { get; set; }

        //public bool IsReadOnly { get; set; } //=> DocIsReadOnly || !DB.HaveWriteAccess("ProductSpools") || !AllowEditProduct;

        /*public override bool CanSaveExecute()
        {
            return base.CanSaveExecute() && IsReadOnly;
        }
        */
        public bool CannotChooseCharacteristic => IsReadOnly || !GrantPermissionOnChooseNomenclatureAndCharacteristic;

        protected override bool CanChooseNomenclature() => !CannotChooseCharacteristic;

        //private byte? _stateid;
        /*
                [UIAuth(UIAuthLevel.ReadOnly)]
                [Required(ErrorMessage=@"Необходимо выбрать качество")]
                public byte? StateID
                {
                    get
                    {
                        return _stateid;
                    }
                    set
                    {
                        _stateid = value;
                        RaisePropertyChanged("StateID");
                    }
                }
        */
        private byte _toughnessKindID = 1;
        private decimal _currentLength;
        private int _currentDiameter;
        private string _density;

        [UIAuth(UIAuthLevel.ReadOnly)]
        public byte ToughnessKindID
        {
            get { return _toughnessKindID; }
            set
            {
                _toughnessKindID = value;
                RaisePropertiesChanged("ToughnessKindID");
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменена Прочность " + value.ToString() + " в ProductID",
                            "SET ToughnessKindID in DocProductSpoolViewModel: value = " + value.ToString(), DocID, ProductId);
                }
            }
        }
        public Dictionary<byte, string> ToughnessKinds { get; set; }
        //        public Dictionary<byte, string> States { get; set; }
        //        public ObservableCollection<RejectionReason> RejectionReasons { get; set; }
        //        [UIAuth(UIAuthLevel.ReadOnly)]
        //        public Guid? RejectionReasonID { get; set; }

        private void SetWeight()
        {
            DB.AddLogMessageInformation("Нажатие кнопки Уменьшить вес в продукте ProductID",
                "Start SetWeight in DocProductSpoolViewModel", DocID, ProductId);
            var model = new SetQuantityDialogModel("Укажите текущий вес тамбура в килограммах", "Текущий вес, кг", 0, (int)RestWeight);
            var okCommand = new UICommand()
            {
                Caption = "OK",
                IsCancel = false,
                IsDefault = true,
                Command = new DelegateCommand<CancelEventArgs>(
            x => DebugFunc(),
            x => model.Quantity >= 0 && model.Quantity < RestWeight),
            };
            var cancelCommand = new UICommand()
            {
                Id = MessageBoxResult.Cancel,
                Caption = "Отмена",
                IsCancel = true,
                IsDefault = false,
            };
            var dialogService = GetService<IDialogService>("");
            var result = dialogService.ShowDialog(
                dialogCommands: new List<UICommand>() { okCommand, cancelCommand },
                title: "Текущий вес тамбура",
                viewModel: model);
            if (result == okCommand)
            {
                if (model.Quantity < RestWeight)
                {
                    using (var gammaBase = DB.GammaDbWithNoCheckConnection)
                    {
                        var currentDateTime = DB.CurrentDateTime;
                        var docWithdrawalid = SqlGuidUtil.NewSequentialid();
                        var docWithdrawalProduct = new DocWithdrawalProducts
                        {
                            DocID = docWithdrawalid,
                            ProductID = ProductId,
                            DocWithdrawal = new DocWithdrawal
                            {
                                DocID = docWithdrawalid,
                                OutPlaceID = WorkSession.PlaceID,
                                Docs = new Docs()
                                {
                                    DocID = docWithdrawalid,
                                    Date = currentDateTime,
                                    DocTypeID = (int)DocTypes.DocSpecificstionQuantity,
                                    PlaceID = WorkSession.PlaceID,
                                    UserID = WorkSession.UserID,
                                    ShiftID = WorkSession.ShiftID,
                                    PrintName = WorkSession.PrintName,
                                    IsConfirmed = false
                                }
                            },
                            Quantity = Math.Round((decimal)(RestWeight - model.Quantity) / 1000, 3),
                            CompleteWithdrawal = (model.Quantity == 0) ? (bool?)true : null
                        };
                        gammaBase.DocWithdrawalProducts.Add(docWithdrawalProduct);
                        gammaBase.SaveChanges();
                        DB.AddLogMessageInformation("Уменьшен вес в продукте ProductID на " + model.Quantity + " кг.",
                                "End successed SetWeight in DocProductSpoolViewModel: minus " + model.Quantity + " kg.", DocID, ProductId);
                    }

                }
                else
                {
                    Functions.ShowMessageError("Не удалось уменьшить вес. Новый вес больше текущего.",
                        "Error SetWeight in DocProductSpoolViewModel: New weight is more than the current", DocID, ProductId);
                }
            }
        }

        private void DebugFunc()
        {
            System.Diagnostics.Debug.Print("Кол-во задано");
        }

        private void GetWeight()
        {
            Functions.ShowMessageInformation("Нажатие кнопки Получить вес в продукте ProductID",
                "Start GetWeight in DocProductSpoolViewModel", DocID, ProductId);
            if (!Scales.IsReady)
            {
                Functions.ShowMessageError("Не удалось соединиться с весами",
                    "Error GetWeight in DocProductSpoolViewModel: Failed to connect to scales", DocID, ProductId);
                //ManualWeightInput = true;
                return;
            }
            UIServices.SetBusyState();
            //var startTime = DateTime.Now;
            do
            {
                Weight = (int)Scales.Weight;
                //Thread.Sleep(500);
                //if (!((DateTime.Now - startTime).TotalSeconds > 15)) continue;
                //MessageBox.Show("Вес не стабилизировался в течении 15 секунд");
                //ManualWeightInput = true;
                break;
            } while (!Scales.IsStable);
        }

    }
}