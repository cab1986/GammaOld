// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Gamma.Attributes;
using Gamma.Entities;
using Gamma.Interfaces;
using Gamma.Models;

namespace Gamma.ViewModels
{
    sealed public class ProductionTaskBalerViewModel : DbEditItemWithNomenclatureViewModel, ICheckedAccess
    {
        /// <summary>
        /// Инициализация ProductionTaskSGIViewModel
        /// </summary>
        public ProductionTaskBalerViewModel(GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb;
            PlaceGroupID = (int)PlaceGroup.Baler;
            Places = new ObservableCollection<Place>(GammaBase.Places.Where(p => p.PlaceGroupID == (short)PlaceGroup.Baler).
                Select(p => new Place()
                {
                    PlaceID = p.PlaceID,
                    PlaceName = p.Name
                }));
            if (Places.Count > 0)
                PlaceID = Places[0].PlaceID;
//            PrintExampleCommand = new DelegateCommand(PrintExample);
        }
        
        
        /// <summary>
        /// Открытие задания для редактирования
        /// </summary>
        /// <param name="productionTaskBatchID">id пакета заданий(для СГИ одно задание)</param>
        public ProductionTaskBalerViewModel(Guid productionTaskBatchID) : this()
        {
            using (var gammaBase = DB.GammaDb)
            {
                var productionTask = gammaBase.GetProductionTaskByBatchID(productionTaskBatchID, (short)PlaceGroup.Baler).FirstOrDefault();
                if (productionTask != null)
                {
                    NomenclatureID = productionTask.C1CNomenclatureID;
                    CharacteristicID = productionTask.C1CCharacteristicID;
                    PlaceID = productionTask.PlaceID;
                }
                ProductionTaskStateID =
                    gammaBase.ProductionTaskBatches.Where(p => p.ProductionTaskBatchID == productionTaskBatchID)
                        .Select(p => p.ProductionTaskStateID)
                        .FirstOrDefault();
                IsConfirmed = ProductionTaskStateID > 0; // Если статус задания в производстве или выполнено, то считаем его подтвержденным
            }
        }


/*
        public event Func<GammaEntities, bool> PrintExampleEvent;

        public DelegateCommand PrintExampleCommand { get; private set; }

        private void PrintExample()
        {
            PrintExampleEvent?.Invoke(GammaBase); // Вызывает сохранение батча, при этом productionTaskID получает значение
            ReportManager.PrintReport("PalletExample", "Examples", ProductionTaskId);
        }
*/

        private Guid ProductionTaskId { get; set; }

        public byte ProductionTaskStateID { get; set; }

/*
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
*/

        private ObservableCollection<Place> _places;

        /// <summary>
        /// Список конвертингов
        /// </summary>
        public ObservableCollection<Place> Places
        {
            get { return _places; }
            set
            {
                _places = value;
                if (_places.Select(p => p.PlaceID).Contains(PlaceID ?? -1)) return;
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
        [Required(ErrorMessage = @"Передел не может быть пустым")]
        public int? PlaceID
        {
            get { return _placeID; }
            set
            {
                _placeID = value;
                RaisePropertiesChanged("PlaceID");
            }
        }

        /*
        /// <summary>
        /// Количество рулончиков в шт.
        /// </summary>
        [Range(100, int.MaxValue, ErrorMessage = @"Необходимо указать корректное количество")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public int Quantity { get; set; }
        */
/*
        protected override void NomenclatureChanged(Nomenclature1CMessage msg)
        {
            base.NomenclatureChanged(msg);
            if (
                GammaBase.C1CMainSpecifications.Any(
                    ms => ms.C1CNomenclatureID == NomenclatureID && !ms.C1CPlaceID.HasValue))
            {
                Places = new ObservableCollection<Place>(GammaBase.Places.Where(p => p.PlaceGroupID == (short)PlaceGroup.Convertings
                    && p.BranchID == WorkSession.BranchID).Select(p => new Place()
                    {
                        PlaceID = p.PlaceID,
                        PlaceName = p.Name
                    }));
                return;
            }
            Places = new ObservableCollection<Place>(GammaBase.C1CMainSpecifications.Where(ms => ms.C1CNomenclatureID == NomenclatureID
                && ms.C1CPlaces.Places.FirstOrDefault().BranchID == WorkSession.BranchID && ms.C1CPlaces.Places.FirstOrDefault().PlaceGroupID == (short)PlaceGroup.Convertings)
                .Select(ms => new Place()
                {
                    PlaceID = ms.C1CPlaces.Places.FirstOrDefault().PlaceID,
                    PlaceName = ms.C1CPlaces.Places.FirstOrDefault().Name
                }).Distinct());
            if (Places.Count == 0)
                MessageBox.Show(
                    "Не найдено ни одного подходящего передела для данной номенклатуры!\r\nВозможно вы выбрали номенклатуру другого филиала",
                    "Нет переделов", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }
*/

        public override bool SaveToModel(Guid productionTaskBatchID, GammaEntities gammaBase = null)
        {
            SaveToModel();
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
            productionTask.PlaceID = PlaceID;
            GammaBase.SaveChanges();
            ProductionTaskId = productionTask.ProductionTaskID;
            return true;
        }

/*
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
*/
        protected override bool CanChooseNomenclature()
        {
            return base.CanChooseNomenclature() && !IsReadOnly;
        }

        /*
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
*/

        /// <summary>
        /// Статус задания в производстве или выполнено
        /// </summary>
        private bool IsConfirmed { get; }
        /// <summary>
        /// Только для чтения, если по каким-то причинам не задание невалидно, то есть возможность редактирования
        /// </summary>
        public bool IsReadOnly => (IsConfirmed || !DB.HaveWriteAccess("ProductionTasks")) && IsValid;
    }
}
