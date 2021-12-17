// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using Gamma.Interfaces;
using System.Collections.ObjectModel;
using System.Data.Entity;
using DevExpress.Mvvm;
using Gamma.Attributes;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Models;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.Generic;
using Gamma.DialogViewModels;
using System.ComponentModel;

namespace Gamma.ViewModels
{
    /// <summary>
    /// ViewModel для задания на СГИ
    /// </summary>
    public sealed class ProductionTaskSGIViewModel : DbEditItemWithNomenclatureViewModel, IProductionTask, ICheckedAccess
    {
        /// <summary>
        /// Инициализация ProductionTaskSGIViewModel
        /// </summary>
        public ProductionTaskSGIViewModel(GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb;
            PlaceGroupID = (int)PlaceGroup.Convertings;
            Places = new ObservableCollection<Place>(GammaBase.Places.Where(p => p.PlaceGroupID == (short)PlaceGroup.Convertings && p.BranchID == WorkSession.BranchID).
                Select(p => new Place()
                {
                    PlaceID = p.PlaceID, PlaceName = p.Name
                }));
            if (Places.Count > 0)
                PlaceID = Places[0].PlaceID;
            TypeTransportLabels = GammaBase.C1CPropertyValues.Where(p => p.ParentID == new Guid("A6139C32-C3BE-11E3-B873-002590304E93")).
                OrderBy(mu => mu.C1CCode).ToDictionary(x => x.C1CPropertyValueID, v => v.PrintDescription);
            try
             {
                 RobotNomenclatures = new ObservableCollection<RobotNomenclature>(GammaBase.vRobotNomenclatures.Where(p => p.PlaceID == PlaceID).
                     Select(p => new RobotNomenclature()
                     {
                         ProdNumber = p.ProdNumber,
                         ProdDescription = p.ProdDescription,
                         EANFullPallet = p.EANFullPallet,
                         ProductionLine = p.ProductionLine,
                         PlaceID = p.PlaceID,
                         ProdName = p.ProdName
                     }));
             }
             catch
             {
                 if (RobotNomenclatures == null)
                     RobotNomenclatures = new ObservableCollection<RobotNomenclature>();
             }
            PrintExampleCommand = new DelegateCommand(PrintExample);
            SetActualDateCommand = new DelegateCommand(SetActualDate, () => ProductionTaskID != Guid.Empty);
            UsedSpools = new SpoolWithdrawByShiftViewModel();
            RecalcOEECommand = new DelegateCommand(() => RaisePropertiesChanged("OEE"));
        }
        /// <summary>
        /// Открытие задания для редактирования
        /// </summary>
        /// <param name="productionTaskBatchID">id пакета заданий(для СГИ одно задание)</param>
        public ProductionTaskSGIViewModel(Guid productionTaskBatchID) : this()
        {
            using (var gammaBase = DB.GammaDb)
            {
                var productionTask = gammaBase.GetProductionTaskByBatchID(productionTaskBatchID, (short)PlaceGroup.Convertings).FirstOrDefault();
                if (productionTask != null)
                {
                    ProductionTaskID = productionTask.ProductionTaskID;
                    NomenclatureID = productionTask.C1CNomenclatureID;
                    CharacteristicID = productionTask.C1CCharacteristicID;
                    PlaceID = productionTask.PlaceID;
                    //после NomenclatureID, чтобы обновился список Places
                    DateBegin = productionTask.DateBegin;
                    DateEnd = productionTask.DateEnd;
                    Quantity = (int)productionTask.Quantity;
                    ActualStartDate = productionTask.ActualStartDate;
                    ActualEndDate = productionTask.ActualEndDate;
                    if (!Places.Any(p => p.PlaceID == PlaceID))
                        foreach (var place in GammaBase.Places.Where(p => p.PlaceID == PlaceID).
                                                Select(p => new Place()
                                                {
                                                    PlaceID = p.PlaceID,
                                                    PlaceName = p.Name
                                                }))
                            Places.Add(place);
                    if (productionTask.PlaceID == WorkSession.PlaceID)
                    {
                        var checkResult = gammaBase.CheckProductionTaskSourceSpools(productionTask.PlaceID,
                            productionTask.ProductionTaskID).FirstOrDefault();
                        if (!string.IsNullOrEmpty(checkResult?.ResultMessage))
                        {
                            MessageBox.Show(checkResult.ResultMessage, "Предупреждение", MessageBoxButton.OK,
                                MessageBoxImage.Asterisk);
                        }
                    }
                    /*RobotNomenclatures = new ObservableCollection<RobotNomenclature>(GammaBase.vRobotNomenclatures.Where(p => p.PlaceID == PlaceID).
                        Select(p => new RobotNomenclature()
                        {
                            ProdNumber = p.ProdNumber,
                            ProdDescription = p.ProdDescription,
                            EANFullPallet = p.EANFullPallet,
                            ProductionLine = p.ProductionLine,
                            PlaceID = p.PlaceID,
                            ProdName = p.ProdName
                        }));*/
                    RobotProductNumber =
                        gammaBase.ProductionTaskConverting.Where(p => p.ProductionTaskID == productionTask.ProductionTaskID)
                            .Select(p => p.RobotProductNumber)
                            .FirstOrDefault() ??
                        gammaBase.RobotProduct1CCharacteristic.Where(p => p.C1CNomenclatureID == NomenclatureID && p.C1CCharacteristicID == CharacteristicID && p.PlaceID == PlaceID)
                            .Select(p => p.ProdNumber)
                            .FirstOrDefault();

                    var oldCharacteristicID = gammaBase.C1CCharacteristics.FirstOrDefault(c => c.C1CCharacteristicID == CharacteristicID)?.C1COldCharacteristicID;
                    TypeTransportLabelID = gammaBase.C1CCharacteristicProperties.FirstOrDefault(pr => pr.C1CPropertyID == new Guid("4CA4FA70-EE6C-11E9-B660-002590EBA5B6") && pr.C1CCharacteristicID == oldCharacteristicID)?.C1CPropertyValueID;
                    UpdateGroupPackageLabelImage(productionTask.ProductionTaskID);
                    ProductionTaskSpecificationViewModel = new ProductionTaskSpecificationViewModel(productionTask.C1CSpecificationID, NomenclatureID, CharacteristicID, PlaceID, IsReadOnly);
                }
                else
                {
                    ProductionTaskSpecificationViewModel = new ProductionTaskSpecificationViewModel();

                }
                ProductionTaskStateID =
                    gammaBase.ProductionTaskBatches.Where(p => p.ProductionTaskBatchID == productionTaskBatchID)
                        .Select(p => p.ProductionTaskStateID)
                        .FirstOrDefault();
                IsConfirmed = ProductionTaskStateID > 0; // Если статус задания в производстве или выполнено, то считаем его подтвержденным
                
            }
        }

        public override bool IsValid => base.IsValid && DateEnd != null && DateBegin != null && DateEnd >= DateBegin;

        private bool _isEditingQuantity { get; set; }
        public bool IsEditingQuantity
        {
            get
            {
                return _isEditingQuantity;
            }
            set
            {
                _isEditingQuantity = value;
                RaisePropertyChanged("IsEditingQuantity");
            }
        }


        public event Func<bool> PrintExampleEvent;

        public DelegateCommand PrintExampleCommand { get; private set; }

        public DelegateCommand SetActualDateCommand { get; private set; }

        public DelegateCommand RecalcOEECommand { get; private set; }

        public bool IsHidePrintExample => WorkSession.ShiftID != 0;

        public SpoolWithdrawByShiftViewModel UsedSpools { get; private set; }

        private BitmapImage _groupPackageLabelImage;
        /// <summary>
        /// Групповая этикетка.
        /// </summary>
        public BitmapImage GroupPackageLabelImage
        {
            get { return _groupPackageLabelImage; }
            set
            {
                _groupPackageLabelImage = value;
                RaisePropertiesChanged("GroupPackageLabelImage");
            }
        }

        private BitmapImage _transportPackageLabelImage;
        /// <summary>
        /// Групповая этикетка.
        /// </summary>
        public BitmapImage TransportPackageLabelImage
        {
            get { return _transportPackageLabelImage; }
            set
            {
                _transportPackageLabelImage = value;
                RaisePropertiesChanged("TransportPackageLabelImage");
            }
        }

        private Image ByteArrayToImage(byte[] inputArray)
        {
            var memoryStream = new System.IO.MemoryStream(inputArray);
            return Image.FromStream(memoryStream);
        }

        public void UpdateGroupPackageLabelImage(Guid productionTaskId)
        {
            using (var gammaBase = DB.GammaDb)
            {
                var png = gammaBase.ProductionTaskConverting.Where(p => p.ProductionTaskID == productionTaskId)
                                .Select(p => p.GroupPackLabelPNG)
                                .FirstOrDefault();
                if (png != null)
                {
                    var GroupPackageLabelPNG = ByteArrayToImage(png);

                    // ImageSource ...
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    MemoryStream ms = new MemoryStream();
                    // Save to a memory stream...
                    GroupPackageLabelPNG.Save(ms, ImageFormat.Png);
                    // Rewind the stream... 
                    ms.Seek(0, SeekOrigin.Begin);
                    // Tell the WPF image to use this stream... 
                    bi.StreamSource = ms;
                    bi.EndInit();
                    GroupPackageLabelImage = bi;
                }
            }
            UpdateTransportPackageLabelImage(productionTaskId);
        }

        public void UpdateTransportPackageLabelImage(Guid productionTaskId)
        {
            using (var gammaBase = DB.GammaDb)
            {
                var png = gammaBase.ProductionTaskConverting.Where(p => p.ProductionTaskID == productionTaskId)
                                .Select(p => p.TransportPackLabelPNG)
                                .FirstOrDefault();
                if (png != null)
                {
                    var TransportPackageLabelPNG = ByteArrayToImage(png);

                    // ImageSource ...
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    MemoryStream ms = new MemoryStream();
                    // Save to a memory stream...
                    TransportPackageLabelPNG.Save(ms, ImageFormat.Png);
                    // Rewind the stream... 
                    ms.Seek(0, SeekOrigin.Begin);
                    // Tell the WPF image to use this stream... 
                    bi.StreamSource = ms;
                    bi.EndInit();
                    TransportPackageLabelImage = bi;
                }
            }
        }

        private void PrintExample()
        {
            PrintExampleEvent?.Invoke(); // Вызывает сохранение батча, при этом productionTaskID получает значение
            ReportManager.PrintReport("PalletExample","Examples",ProductionTaskId);
        }

        public Guid ProductionTaskId { get; private set; }

        public byte ProductionTaskStateID { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        [Required(ErrorMessage = @"Необходимо выбрать характеристику")]
        public override Guid? CharacteristicID
        {
            get { return base.CharacteristicID; }
            set
            {
                base.CharacteristicID = value;
                ProductionTaskSpecificationViewModel?.SetSpecifications(NomenclatureID, value, PlaceID);
            }
        }

        private ObservableCollection<Place> _places = new ObservableCollection<Place>();

        /// <summary>
        /// Список конвертингов
        /// </summary>
        public ObservableCollection<Place> Places
        {
            get { return _places; }
            set
            {
                //_places = value;
                _places.Clear();
                foreach (var place in value)
                {
                    _places.Add(place);
                }
                if (_places.Select(p => p.PlaceID).Contains(PlaceID??-1)) return;
                if (_places.Count > 0) PlaceID = _places[0].PlaceID;
                else PlaceID = null;
                RaisePropertyChanged("Places");
            }
        }

        /// <summary>
        /// Список типов транспортных этикеток
        /// </summary>
        public Dictionary<Guid, string>  TypeTransportLabels { get; set; }

        public Guid ProductionTaskID { get; set; }
        public ProductionTaskSpecificationViewModel ProductionTaskSpecificationViewModel { get; private set; }

        private int? _placeID;
        /// <summary>
        /// id выбранного передела
        /// </summary>
        [UIAuth(UIAuthLevel.ReadOnly)]
        //[Required]
        [Required(ErrorMessage = @"Передел не может быть пустым")]
        public int? PlaceID
        {
            get { return _placeID; }
            set
            {
                _placeID = value;
                RefreshCharacteristics();
                ProductionTaskSpecificationViewModel?.SetSpecifications(NomenclatureID, CharacteristicID, value);
                RaisePropertiesChanged("PlaceID");
            }
        }

        
        private Place _selectedPlace;
        /// <summary>
        /// Выбранный передел. При смене обновляем список номеров робота. 
        /// </summary>
        public Place SelectedPlace
        {
            get { return _selectedPlace; }
            set
            {
                if (_selectedPlace != value)
                {
                    _selectedPlace = value;
                    if (value != null) PlaceChanged(value.PlaceID);
                }
            }
        }

        private Guid? _typeTransportLabelID;
        /// <summary>
        /// id выбранного типа транспортной этикетки
        /// </summary>        
        public Guid? TypeTransportLabelID
        {
            get { return _typeTransportLabelID; }
            set
            {
                _typeTransportLabelID = value;
                RaisePropertiesChanged("TypeTransportLabelID");
            }
        }

        private bool _isReadOnlyTypeTransportLabel = true;
        /// <summary>
        /// Доступность редактирования типа транспортной этикетки
        /// </summary>        
        public bool IsReadOnlyTypeTransportLabel
        {
            get { return _isReadOnlyTypeTransportLabel; }
            set
            {
                _isReadOnlyTypeTransportLabel = value;
                if (value)
                {
                    try
                    {
                        using (var gammaBase = DB.GammaDb)
                        {
                            var oldCharacteristicID = gammaBase.C1CCharacteristics.FirstOrDefault(c => c.C1CCharacteristicID == CharacteristicID)?.C1COldCharacteristicID;
                            var typeTransportLabel = gammaBase.C1CCharacteristicProperties.FirstOrDefault(pr => pr.C1CPropertyID == new Guid("4CA4FA70-EE6C-11E9-B660-002590EBA5B6") && pr.C1CCharacteristicID == oldCharacteristicID);
                            if (typeTransportLabel == null)
                            {
                                gammaBase.C1CCharacteristicProperties.Add(new C1CCharacteristicProperties()
                                {
                                    C1CCharacteristicID = (Guid)oldCharacteristicID,
                                    C1CPropertyID = new Guid("4CA4FA70-EE6C-11E9-B660-002590EBA5B6"),
                                    C1CPropertyValueID = TypeTransportLabelID
                                });
                            }
                            else
                            {
                                typeTransportLabel.C1CPropertyValueID = TypeTransportLabelID;
                            }
                            gammaBase.SaveChanges();
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка! Изменение типа транспортной этикетки не сохранено!");
                    }
                }
                RaisePropertiesChanged("IsReadOnlyTypeTransportLabel");
            }
        }

        private bool _isReadOnlyRobotNomenclature = true;
        /// <summary>
        /// Доступность редактирования привязки номера рецепта робота
        /// </summary>        
        public bool IsReadOnlyRobotNomenclature
        {
            get { return _isReadOnlyRobotNomenclature; }
            set
            {
                var prev_value = _isReadOnlyRobotNomenclature;
                _isReadOnlyRobotNomenclature = value;
                if (value && value != prev_value)
                {
                    try
                    {
                        using (var gammaBase = DB.GammaDb)
                        {
                            var productionTaskConverting =
                                gammaBase.ProductionTasks.FirstOrDefault(p => p.ProductionTaskID == ProductionTaskID).ProductionTaskConverting;

                            var robotProductDescription = GammaBase.vRobotNomenclatures.Where(p => p.ProdNumber == RobotProductNumber)
                                .Select(p => p.ProdDescription)
                                .FirstOrDefault();
                            if (productionTaskConverting == null)
                            {
                                gammaBase.ProductionTaskConverting.Add(new ProductionTaskConverting()
                                {
                                    ProductionTaskID = ProductionTaskID,
                                    RobotProductNumber = RobotProductNumber,
                                    RobotProductDescription = robotProductDescription
                                });
                            }
                            else
                            {

                                productionTaskConverting.RobotProductNumber = RobotProductNumber;
                                productionTaskConverting.RobotProductDescription = robotProductDescription;
                            }

                            var robotProduct1CCharacteristics = gammaBase.RobotProduct1CCharacteristic.Where(p => p.ProdNumber == RobotProductNumber || (p.C1CNomenclatureID == NomenclatureID && p.C1CCharacteristicID == CharacteristicID)).ToList();
                            if (robotProduct1CCharacteristics == null)
                            {
                                gammaBase.RobotProduct1CCharacteristic.Add(new RobotProduct1CCharacteristic()
                                {
                                    ProdNumber = (int)RobotProductNumber,
                                    C1CNomenclatureID = NomenclatureID,
                                    C1CCharacteristicID = (Guid)CharacteristicID,
                                    PlaceID = PlaceID
                                });
                            }
                            else
                            {
                                if (robotProduct1CCharacteristics.Count() == 1)
                                {
                                    robotProduct1CCharacteristics.First().ProdNumber = (int)RobotProductNumber;
                                    robotProduct1CCharacteristics.First().C1CNomenclatureID = NomenclatureID;
                                    robotProduct1CCharacteristics.First().C1CCharacteristicID = (Guid)CharacteristicID;
                                    robotProduct1CCharacteristics.First().PlaceID = PlaceID;
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
                            
                            gammaBase.SaveChanges();
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка! Изменение привязки рецепта робота не сохранено!");
                        DB.AddLogMessageError("Ошибка! Изменение привязки рецепта робота не сохранено! ProductionTask='"+ProductionTaskID + "', RobotProductNumber='" + RobotProductNumber
                             + "', NomenclatureID='" + NomenclatureID + "', CharacteristicID='" + CharacteristicID+"'");
                    }
                }
                RaisePropertiesChanged("IsReadOnlyRobotNomenclature");
            }
        }

        /// <summary>
        /// Обновить список номеров робота при смене передела
        /// </summary>
        private void PlaceChanged(int PlaceID)
        {
            using (var gammaBase = DB.GammaDb)
            {
                var IsRobot =
                gammaBase.Places.Where(p => p.PlaceID == PlaceID)
                    .Select(p => p.IsRobot)
                    .FirstOrDefault();
                if (IsRobot == true)
                {
                    RobotNomenclatureVisible = Visibility.Visible;
                    RobotNomenclatures = new ObservableCollection<RobotNomenclature>(GammaBase.vRobotNomenclatures.Where(p => p.PlaceID == PlaceID).
                        Select(p => new RobotNomenclature()
                        {
                            ProdNumber = p.ProdNumber,
                            ProdDescription = p.ProdDescription,
                            EANFullPallet = p.EANFullPallet,
                            ProductionLine = p.ProductionLine,
                            PlaceID = p.PlaceID,
                            ProdName = p.ProdName
                        }));
                }
                else
                {
                    RobotNomenclatureVisible = Visibility.Collapsed;
                    RobotNomenclatures = null;
                }
            }
            RobotProductNumber = GammaBase.RobotProduct1CCharacteristic.Where(p => p.C1CNomenclatureID == NomenclatureID && p.C1CCharacteristicID == CharacteristicID && p.PlaceID == PlaceID)
        .Select(p => p.ProdNumber)
        .FirstOrDefault();

        }

        /// <summary>
        /// Количество рулончиков в шт.
        /// </summary>
        [Range(100, int.MaxValue, ErrorMessage = @"Необходимо указать корректное количество")]
        //[UIAuth(UIAuthLevel.ReadOnly)]
        public int Quantity { get; set; }

        protected override void NomenclatureChanged(Nomenclature1CMessage msg)
        {
            base.NomenclatureChanged(msg);
            RefreshPlaces();
        }
        
        public void RefreshCharacteristics()
        {
            var currentCharacteristicID = CharacteristicID;
            Characteristics = new ObservableCollection<Characteristic>(GammaBase.GetSpecificationNomenclatureOnPlace(PlaceID, null)
                .Where(n => n.C1CNomenclatureID == NomenclatureID && n.C1CCharacteristicID != Guid.Empty)
                .Select(n => new Characteristic()
                {
                    CharacteristicID = (Guid)n.C1CCharacteristicID,
                    CharacteristicName = n.CharacteristicName
                }).Distinct()
                );
            if (currentCharacteristicID != CharacteristicID)
                CharacteristicID = currentCharacteristicID;
            if (!Characteristics.Any(p => p.CharacteristicID == CharacteristicID))
                foreach (var characteristic in GammaBase.C1CCharacteristics.Where(p => p.C1CCharacteristicID == CharacteristicID).
                                        Select(p => new Characteristic()
                                        {
                                            CharacteristicID = p.C1CCharacteristicID,
                                            CharacteristicName = p.Name
                                        }))
                    Characteristics.Add(characteristic);
        }
        public void RefreshPlaces()
        {
            var places = GammaBase.GetSpecificationNomenclatureOnPlace(null, null)
                .Where(n => n.C1CNomenclatureID == NomenclatureID)
                .Select(n => new Place()
                {
                    PlaceID = (int)n.PlaceID,
                    PlaceName = n.PlaceName
                }).Distinct();
            Places.Clear();
            foreach (var place in places)
            {
                if (!Places.Any(n => n.PlaceID == place.PlaceID))
                    Places.Add(place);
            }
            RefreshCharacteristics();
            if (Places.Count == 0 && NomenclatureID != null && DateBegin != null)
            MessageBox.Show(
                "Не найдено ни одной основной спецификации для данной номенклатуры!\r\nВозможно вы выбрали номенклатуру другого филиала",
                "Нет спецификаций", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }


        //public override bool CanSaveExecute()
        //{
        //    return false;
        //}
        public override bool SaveToModel(Guid productionTaskBatchID)
        {
            UIServices.SetBusyState();
                var productionTaskBatch =
                    GammaBase.ProductionTaskBatches.Include(pt => pt.ProductionTasks)
                        .First(pt => pt.ProductionTaskBatchID == productionTaskBatchID);
                if (productionTaskBatch == null)
                {
                    MessageBox.Show("Не удалось сохранить задание!");
                    return false;
                }
                ProductionTasks productionTask;
                if (productionTaskBatch.ProductionTasks.Count == 0)
                {
                    productionTask = new ProductionTasks()
                    {
                        ProductionTaskID = SqlGuidUtil.NewSequentialid(),
                        PlaceGroupID = (short)PlaceGroup.Convertings
                    };
                    productionTaskBatch.ProductionTasks.Add(productionTask);

                }
                else
                {
                    productionTask = productionTaskBatch.ProductionTasks.First();
                }
            if (NomenclatureID == null)
            {
                MessageBox.Show("Вы попытались сохранить задание без номенклатуры. Оно не будет сохранено");
                return false;
            }
                productionTask.C1CNomenclatureID = (Guid)NomenclatureID;
                productionTask.C1CCharacteristicID = CharacteristicID;
                productionTask.DateBegin = DateBegin;
                productionTask.DateEnd = DateEnd;
                productionTask.PlaceID = PlaceID;
                productionTask.Quantity = Quantity;
                productionTask.C1CSpecificationID = ProductionTaskSpecificationViewModel.SpecificationID;// SelectedSpecification.Key;
            
            var placeIsRobot =
                GammaBase.Places.Where(p => p.PlaceID == PlaceID)
                        .Select(p => p.IsRobot)
                        .FirstOrDefault();
            if (placeIsRobot == true)
            {
                var RobotProductDescription = GammaBase.vRobotNomenclatures.Where(p => p.ProdNumber == RobotProductNumber)
                .Select(p => p.ProdDescription)
                .FirstOrDefault();
                ProductionTaskConverting productionTaskConverting;
                if (productionTaskBatch.ProductionTasks.FirstOrDefault().ProductionTaskConverting == null)
                {
                    productionTaskConverting = new ProductionTaskConverting()
                    {
                        ProductionTaskID = productionTask.ProductionTaskID,
                        RobotProductNumber = RobotProductNumber,
                        RobotProductDescription = RobotProductDescription
                    };
                    productionTaskBatch.ProductionTasks.FirstOrDefault().ProductionTaskConverting = productionTaskConverting;
                }
                else
                {
                    productionTaskBatch.ProductionTasks.First().ProductionTaskConverting.RobotProductNumber = RobotProductNumber;
                    productionTaskBatch.ProductionTasks.First().ProductionTaskConverting.RobotProductDescription = RobotProductDescription;
                }
                /*var productionTaskStateID =
                    GammaBase.ProductionTaskBatches.Where(p => p.ProductionTaskBatchID == productionTaskBatchID)
                            .Select(p => p.ProductionTaskStateID)
                            .FirstOrDefault();
                if (productionTaskStateID > 0 && placeIsRobot == true && RobotProductNumber == null)
                {
                    MessageBox.Show("Вы попытались сохранить задание в производстве без указания номера задания робота. Оно не будет сохранено");
                    return false;
                }*/
            }
            //else
            //{
            //    GammaBase.ProductionTaskConverting.RemoveRange(GammaBase.ProductionTaskConverting.Where(c => c.ProductionTaskID == productionTask.ProductionTaskID));
            //}
            
            GammaBase.SaveChanges();
            ProductionTaskId = productionTask.ProductionTaskID;
            return true;
        }

        private decimal _madeQuantiy;
        /// <summary>
        /// Кол-во выпущенных рулончиков в шт.
        /// </summary>
        public decimal MadeQuantity
        {
            get { return _madeQuantiy; }
            set
            {
                _madeQuantiy = value;
                RaisePropertiesChanged("MadeQuantity");
            }
        }

        //private decimal? _oEE;
        /// <summary>
        /// ОЕЕ
        /// </summary>
        public decimal? OEE //=> DB.GetProductionTaskOEE(ProductionTaskID);
        {
            get { return DB.GetProductionTaskOEE(ProductionTaskID); }
            set
            {
                //_oEE = value;
                RaisePropertiesChanged("OEE");
            }
        }

        protected override bool CanChooseNomenclature()
        {
            return base.CanChooseNomenclature() && !IsReadOnly;
        }
        private DateTime? _dateBegin;
        /// <summary>
        /// Дата начала выполнения задания
        /// </summary>
        [Required(ErrorMessage = @"Необходимо указать дату начала")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public DateTime? DateBegin
        {
            get { return _dateBegin; }
            set
            {
                _dateBegin = value;
                DateEnd = value;
                RefreshPlaces();
                RaisePropertyChanged("DateBegin");
            }
        }

        private DateTime? _dateEnd;
        /// <summary>
        /// Дата окончания задания
        /// </summary>
        [UIAuth(UIAuthLevel.ReadOnly)]
        public DateTime? DateEnd
        {
            get { return _dateEnd; }
            set
            {
                _dateEnd = value;
                RaisePropertiesChanged("DateEnd");
            }
        }
        
        /// <summary>
        /// Статус задания в производстве или выполнено
        /// </summary>
        //private bool IsConfirmed { get; }
        private bool _isConfirmed { get; set; }
        private bool IsConfirmed
        {
            get { return _isConfirmed; }
            set
            {
                _isConfirmed = value;
                IsReadOnly = (value || !DB.HaveWriteAccess("ProductionTasks")) && IsValid;
            }
        }
        /// <summary>
        /// Только для чтения, если по каким-то причинам не задание невалидно, то есть возможность редактирования
        /// </summary>
        //public bool IsReadOnly => (IsConfirmed || !DB.HaveWriteAccess("ProductionTasks")) && IsValid;
        private bool _isReadOnly { get; set; }
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                ProductionTaskSpecificationViewModel?.SetIsReadOnly(value);
            }
        }

        private DateTime? _actualStartDate;
        /// <summary>
        /// Дата начала выполнения задания
        /// </summary>
        //[UIAuth(UIAuthLevel.ReadOnly)]
        public DateTime? ActualStartDate
        {
            get { return _actualStartDate; }
            set
            {
                _actualStartDate = value;
                RaisePropertyChanged("ActualStartDate");
            }
        }

        private DateTime? _actualEndDate;
        /// <summary>
        /// Дата окончания задания
        /// </summary>
        //[UIAuth(UIAuthLevel.ReadOnly)]
        public DateTime? ActualEndDate
        {
            get { return _actualEndDate; }
            set
            {
                _actualEndDate = value;
                RaisePropertiesChanged("ActualEndDate");
            }
        }

        public void DebugFunc()
        { }

        public void SetActualDate()
        {
            if (DB.HaveWriteAccess("ProductionTasks") || DB.HaveWriteAccess("ActiveProductionTasks"))
            {
                var model = new SetDateDialogViewModel("Укажите дату/время начала и окончания производства", "Период производства", true, ActualStartDate, "Начало", true, ActualEndDate, "Окончание");
                var okCommand = new UICommand()
                {
                    Caption = "OK",
                    IsCancel = false,
                    IsDefault = true,
                    Command = new DelegateCommand<CancelEventArgs>(
                        x => DebugFunc(),
                        x => true),// model.IsValid && (model.DateEnd - model.DateBegin).TotalMinutes > 0 && (model.DateEnd - model.DateBegin).TotalMinutes <= 14 * 60),
                };
                var cancelCommand = new UICommand()
                {
                    Id = MessageBoxResult.Cancel,
                    Caption = "Отмена",
                    IsCancel = true,
                    IsDefault = false,
                };
                var dialogService = GetService<IDialogService>("SetDateDialog");
                var result = dialogService.ShowDialog(
                    dialogCommands: new List<UICommand>() { okCommand, cancelCommand },
                    title: "Изменение периода производства",
                    viewModel: model);
                if (result == okCommand)
                //var dialog = new AddDowntimeDialog();
                //dialog.ShowDialog();
                //if (dialog.DialogResult == true)
                {
                    string addResult = "";
                    if (model.IsVisibleStartDate)
                        ActualStartDate = model.StartDate;
                    if (model.IsVisibleEndDate)
                        ActualEndDate = model.EndDate;
                    var productionTask =
                    GammaBase.ProductionTasks
                        .First(pt => pt.ProductionTaskID == ProductionTaskID);
                    if (productionTask == null)
                    {
                        addResult = "Ошибка при сохранении! Фактическая дата не изменена! " + Environment.NewLine + "Задание на производство должно быть сохранено.";
                    }
                    else
                    {
                        //ProductionTasks productionTask = productionTaskBatch.ProductionTasks.First();
                        try
                        {
                            if (model.IsVisibleStartDate)
                                productionTask.ActualStartDate = model.StartDate;
                            if (model.IsVisibleEndDate)
                                productionTask.ActualEndDate = model.EndDate;
                            GammaBase.SaveChanges();
                            if (model.IsVisibleStartDate)
                                ActualStartDate = model.StartDate;
                            if (model.IsVisibleEndDate)
                                ActualEndDate = model.EndDate;
                        }
                        catch
                        {
                            addResult = "Ошибка при сохранении! Фактическая дата не изменена! " + Environment.NewLine + "Попробуйте еще раз.";
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Ошибка при сохранении! Недостаточно прав!");
            }
        }

        
        public Visibility _robotNomenclatureVisible { get; set; } = Visibility.Collapsed;
        /// <summary>
        /// Видимость задания номера для робота
        /// </summary>
        public Visibility RobotNomenclatureVisible
        {
            get { return _robotNomenclatureVisible; }
            set
            {
                _robotNomenclatureVisible = value;
                RaisePropertiesChanged("RobotNomenclatureVisible");
            }
        }

        public int? _robotProductNumber { get; set; }
        /// <summary>
        /// номер выбранного задания
        /// </summary>
        //[UIAuth(UIAuthLevel.ReadOnly)]
        public int? RobotProductNumber
        {
            get { return _robotProductNumber; }
            set
            {
                _robotProductNumber = value;
                RaisePropertiesChanged("RobotProductNumber");
            }
        }

        private ObservableCollection<RobotNomenclature> _robotNomenclatures;

        /// <summary>
        /// Список номеров заданий робота
        /// </summary>
        public ObservableCollection<RobotNomenclature> RobotNomenclatures
        {
            get { return _robotNomenclatures; }
            set
            {
                _robotNomenclatures = value;
                //if (_robotProductNumbers.Select(p => p.RobotProductNumberID).Contains(PobotProductNumberID ?? -1)) return;
                //if (_robotProductNumbers.Count > 0) RobotProductNumberID = _robotProductNumbers[0].RobotProductNumberID;
                //else RobotProductNumberID = null;
                RaisePropertyChanged("RobotNomenclatures");
            }
        }
    }
}
