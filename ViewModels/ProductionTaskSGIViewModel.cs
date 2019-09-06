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

namespace Gamma.ViewModels
{
    /// <summary>
    /// ViewModel для задания на СГИ
    /// </summary>
    public sealed class ProductionTaskSGIViewModel : DbEditItemWithNomenclatureViewModel, ICheckedAccess
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
            UsedSpools = new SpoolWithdrawByShiftViewModel();
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
                    DateBegin = productionTask.DateBegin;
                    DateEnd = productionTask.DateEnd;
                    NomenclatureID = productionTask.C1CNomenclatureID;
                    CharacteristicID = productionTask.C1CCharacteristicID;
                    Quantity = (int)productionTask.Quantity;
                    PlaceID = productionTask.PlaceID;
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
                    RobotProductNumber =
                        gammaBase.ProductionTaskConverting.Where(p => p.ProductionTaskID == productionTask.ProductionTaskID)
                            .Select(p => p.RobotProductNumber)
                            .FirstOrDefault();

                    UpdateGroupPackageLabelImage(productionTask.ProductionTaskID);

                }
                ProductionTaskStateID =
                    gammaBase.ProductionTaskBatches.Where(p => p.ProductionTaskBatchID == productionTaskBatchID)
                        .Select(p => p.ProductionTaskStateID)
                        .FirstOrDefault();
                IsConfirmed = ProductionTaskStateID > 0; // Если статус задания в производстве или выполнено, то считаем его подтвержденным

            }
        }

        public event Func<bool> PrintExampleEvent;

        public DelegateCommand PrintExampleCommand { get; private set; }

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
                    GroupPackageLabelPNG.Save(ms, ImageFormat.Bmp);
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
                    TransportPackageLabelPNG.Save(ms, ImageFormat.Bmp);
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

        private Guid ProductionTaskId { get; set; }
        
        public byte ProductionTaskStateID { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        [Required(ErrorMessage = @"Необходимо выбрать характеристику")]
        public override Guid? CharacteristicID
        {
            get { return base.CharacteristicID; }
            set
            {
                base.CharacteristicID = value;
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
            //RobotProductNumber = null;
        }

        /// <summary>
        /// Количество рулончиков в шт.
        /// </summary>
        [Range(100, int.MaxValue, ErrorMessage = @"Необходимо указать корректное количество")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public int Quantity { get; set; }

        protected override void NomenclatureChanged(Nomenclature1CMessage msg)
        {
            base.NomenclatureChanged(msg);
            RefreshPlaces();
        }
        
        public void RefreshCharacteristics()
        {
            Characteristics = new ObservableCollection<Characteristic>(GammaBase.GetSpecificationNomenclatureOnPlace(PlaceID, null)
                .Where(n => n.C1CNomenclatureID == NomenclatureID && n.C1CCharacteristicID != Guid.Empty)
                .Select(n => new Characteristic()
                {
                    CharacteristicID = (Guid)n.C1CCharacteristicID,
                    CharacteristicName = n.CharacteristicName
                }).Distinct()
                );
        }
        public void RefreshPlaces()
        {
            var places = GammaBase.GetSpecificationNomenclatureOnPlace(null, null)
                .Where(n => n.C1CNomenclatureID == NomenclatureID)
                .Select(n => new Place()
                {
                    PlaceID = n.PlaceID,
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
                var productionTaskStateID =
                    GammaBase.ProductionTaskBatches.Where(p => p.ProductionTaskBatchID == productionTaskBatchID)
                            .Select(p => p.ProductionTaskStateID)
                            .FirstOrDefault();
                if (productionTaskStateID > 0 && placeIsRobot == true && RobotProductNumber == null)
                {
                    MessageBox.Show("Вы попытались сохранить задание в производстве без указания номера задания робота. Оно не будет сохранено");
                    return false;
                }
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
        private bool IsConfirmed { get; }
        /// <summary>
        /// Только для чтения, если по каким-то причинам не задание невалидно, то есть возможность редактирования
        /// </summary>
        public bool IsReadOnly => (IsConfirmed || !DB.HaveWriteAccess("ProductionTasks")) && IsValid;

        public Visibility _robotNomenclatureVisible { get; set; } = Visibility.Visible;
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
        [UIAuth(UIAuthLevel.ReadOnly)]
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
