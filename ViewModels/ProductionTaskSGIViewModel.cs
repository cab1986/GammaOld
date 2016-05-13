﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using Gamma.Interfaces;
using System.Collections.ObjectModel;
using System.Data.Entity;
using Gamma.Attributes;
using Gamma.Models;

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
            PlaceGroupID = (int)PlaceGroups.Convertings;
            Places = new ObservableCollection<Place> (GammaBase.Places.Where(p => p.PlaceGroupID == (short)PlaceGroups.Convertings).
                Select(p => new Place()
                {
                    PlaceID = p.PlaceID,PlaceName = p.Name
                }));
            if (Places.Count > 0)
                PlaceID = Places[0].PlaceID;
        }
        /// <summary>
        /// Открытие задания для редактирования
        /// </summary>
        /// <param name="productionTaskBatchID">id пакета заданий(для СГИ одно задание)</param>
        public ProductionTaskSGIViewModel(Guid productionTaskBatchID) : this()
        {
            var productionTask = DB.GammaDb.GetProductionTaskByBatchID(productionTaskBatchID, (short)PlaceGroups.Convertings).FirstOrDefault();
            if (productionTask != null)
            {
                DateBegin = productionTask.DateBegin;
                DateEnd = productionTask.DateEnd;
                NomenclatureID = productionTask.C1CNomenclatureID;
                CharacteristicID = productionTask.C1CCharacteristicID;
                Quantity = (int)productionTask.Quantity;
            }
            ProductionTaskStateID =
                DB.GammaDb.ProductionTaskBatches.Where(p => p.ProductionTaskBatchID == productionTaskBatchID)
                    .Select(p => p.ProductionTaskStateID)
                    .FirstOrDefault();
            IsConfirmed = ProductionTaskStateID > 0; // Если статус задания в производстве или выполнено, то считаем его подтвержденным
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
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
        /// <summary>
        /// Количество рулончиков в шт.
        /// </summary>
        [Range(100, int.MaxValue, ErrorMessage = @"Необходимо указать корректное количество")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public int Quantity { get; set; }

        protected override void NomenclatureChanged(Nomenclature1CMessage msg)
        {
            base.NomenclatureChanged(msg);
            if (
                GammaBase.C1CMainSpecifications.Any(
                    ms => ms.C1CNomenclatureID == NomenclatureID && !ms.C1CPlaceID.HasValue))
            {
                Places = new ObservableCollection<Place>(GammaBase.Places.Where(p => p.PlaceGroupID == (short)PlaceGroups.Convertings
                    && p.BranchID == WorkSession.BranchID).Select(p => new Place()
                    {
                        PlaceID = p.PlaceID,
                        PlaceName = p.Name
                    }));
                return;
            }
            Places = new ObservableCollection<Place>(GammaBase.C1CMainSpecifications.Where(ms => ms.C1CNomenclatureID == NomenclatureID 
                && ms.C1CPlaces.Places.FirstOrDefault().BranchID == WorkSession.BranchID && ms.C1CPlaces.Places.FirstOrDefault().PlaceGroupID == (short)PlaceGroups.Convertings)
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
        public override void SaveToModel(Guid productionTaskBatchID, GammaEntities gammaBase = null)
        {
            SaveToModel();
                var productionTaskBatch =
                    GammaBase.ProductionTaskBatches.Include(pt => pt.ProductionTasks)
                        .First(pt => pt.ProductionTaskBatchID == productionTaskBatchID);
                if (productionTaskBatch == null)
                {
                    MessageBox.Show("Не удалось сохранить задание!");
                    return;
                }
                ProductionTasks productionTask;
                if (productionTaskBatch.ProductionTasks.Count == 0)
                {
                    productionTask = new ProductionTasks()
                    {
                        ProductionTaskID = SqlGuidUtil.NewSequentialid(),
                        PlaceGroupID = (short)PlaceGroups.Convertings
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
                return;
            }
                productionTask.C1CNomenclatureID = (Guid)NomenclatureID;
                productionTask.C1CCharacteristicID = CharacteristicID;
                productionTask.DateBegin = DateBegin;
                productionTask.DateEnd = DateEnd;
                productionTask.PlaceID = PlaceID;
                productionTask.Quantity = Quantity;
                GammaBase.SaveChanges();
        }
        private int _madeQuantiy;
        /// <summary>
        /// Кол-во выпущенных рулончиков в шт.
        /// </summary>
        public int MadeQuantity
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
    }
}