// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Attributes;
using Gamma.Common;
using Gamma.Interfaces;
using Gamma.Models;
using System.Windows;
using Gamma.Entities;

namespace Gamma.ViewModels
{
    /// <summary>
    /// ViewModel задания на ПРС
    /// </summary>
    public sealed class ProductionTaskRwViewModel : SaveImplementedViewModel, ICheckedAccess, IProductionTask
    {
        /// <summary>
        /// Открытие для редактирования задания на ПРС
        /// </summary>
        /// <param name="productionTaskBatchID">id пакета заданий</param>
        /// <param name="gammaBase">Контекст БД</param>
        public ProductionTaskRwViewModel(Guid productionTaskBatchID)
        {
            ProductionTaskBatchID = productionTaskBatchID;
            var productionTask = GammaBase.GetProductionTaskByBatchID(productionTaskBatchID, (short)PlaceGroup.Rw).FirstOrDefault();
//            ProductionTask = gammaBase.ProductionTasks.
//                Include("ProductionTaskRWCutting").Include("ProductionTaskBatches").Include("ProductionTaskSGB").
//                Where(pt => pt.ProductionTaskBatches.FirstOrDefault().ProductionTaskBatchID == productionTaskBatchID).FirstOrDefault();
//           IsConfirmed = ProductionTask.ProductionTaskStateID == (byte)ProductionTaskStates.InProduction;
            if (productionTask != null)
            {
                DateBegin = productionTask.DateBegin;
                DateEnd = productionTask.DateEnd;
                TaskQuantity = productionTask.Quantity;
                ProductionTaskSGBViewModel = new ProductionTaskSGBViewModel(productionTask.ProductionTaskID);
                Cuttings = new ItemsChangeObservableCollection<Cutting>(GammaBase.ProductionTaskRWCutting.Where(
                    p => p.ProductionTaskID == productionTask.ProductionTaskID).GroupBy(p => new {p.C1CNomenclatureID, p.C1CCharacteristicID, p.C1CSpecificationID})
                    .Select(g => new Cutting()
                    {
                        NomenclatureID = g.Key.C1CNomenclatureID,
                        CharacteristicID = g.Key.C1CCharacteristicID,
                        Quantity = g.Count(),
                        SpecificationID = g.Key.C1CSpecificationID
                    }));
                TotalFormat = Cuttings.Sum(cutting => cutting.BaseFormat * cutting.Quantity).ToString();
            }
            else
            {
                ProductionTaskSGBViewModel = new ProductionTaskSGBViewModel();
            }
            ChooseNomenclatureCommand = new DelegateCommand(ChooseNomenclature, CanChooseNomenclature);
            AddCuttingCommand = new DelegateCommand(AddCutting, () => !IsConfirmed);
            DeleteCuttingCommand = new DelegateCommand(() =>
            {
                Cuttings.Remove(SelectedCutting);
                TotalFormat = Cuttings.Sum(cutting => cutting.BaseFormat * cutting.Quantity).ToString();
                MessageManager.ProductionTaskRwNomenclatureChanged(ProductionTaskBatchID, Cuttings.Where(c => c.NomenclatureID != null && c.CharacteristicID != null).Select(c => new Nomenclature()
                {
                    NomenclatureID = c.NomenclatureID,
                    CharacteristicID = c.CharacteristicID
                }).ToList());
            }
            , () => SelectedCutting != null && !IsConfirmed);
            IsConfirmed = (productionTask?.IsActual ?? false) && IsValid;
            ProductionTaskID = productionTask?.ProductionTaskID ?? SqlGuidUtil.NewSequentialid();
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

        private void AddCutting()
        {
            var cutting = new Cutting();
            if (Cuttings.Count > 0)
            {
                cutting.NomenclatureID = Cuttings[Cuttings.Count-1].NomenclatureID;
            }
            Cuttings.Add(cutting);
        }

        private void CuttingChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Quantity":
                    TotalFormat = Cuttings.Sum(cutting => cutting.BaseFormat * cutting.Quantity).ToString();
                    break;
                case "CharacteristicID":
                    TotalFormat = Cuttings.Sum(cutting => cutting.BaseFormat * cutting.Quantity).ToString();
                    if (Cuttings.Count > 1 && SelectedCutting?.CharacteristicID != null)
                    {
                        var ids =
                            Cuttings.Except(new List<Cutting> {SelectedCutting}).Where(c => c.CharacteristicID != null)
                                .Select(c => c.CharacteristicID)
                                .ToList();
                        var baseProps =
                            GammaBase.vCharacteristicSGBProperties.Where(
                                cp => ids.Contains(cp.C1CCharacteristicID)) 
                                .Select(cp => new
                                {
                                    cp.CoreDiameter,
                                    cp.LayerNumber,
                                    cp.Color
                                }).FirstOrDefault();
                        var newProps = GammaBase.vCharacteristicSGBProperties.Where(
                            cp => cp.C1CCharacteristicID == SelectedCutting.CharacteristicID)
                            .Select(cp => new
                            {
                                cp.CoreDiameter,
                                cp.LayerNumber,
                                cp.Color
                            }).First();
                        if (baseProps != null && (baseProps.CoreDiameter != newProps.CoreDiameter ||
                            baseProps.LayerNumber != newProps.LayerNumber || baseProps.Color != newProps.Color))
                        {
                            MessageBox.Show("Гильза, слойность или цвет не совпадают", "Характеристика не подходит",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            SelectedCutting.CharacteristicID = null;
                        }
                    }
                    MessageManager.ProductionTaskRwNomenclatureChanged(ProductionTaskBatchID, Cuttings.Where(c => c.NomenclatureID != null && c.CharacteristicID != null).Select(c => new Nomenclature
                    {
                        NomenclatureID = c.NomenclatureID,
                        CharacteristicID = c.CharacteristicID
                    }).ToList());
                    break;
            }
        }

/*
        private ProductionTasks ProductionTask { get; set; }
*/
        private Guid ProductionTaskBatchID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productionTaskBatchID">ID пакета заданий</param>
        public override bool SaveToModel(Guid productionTaskBatchID)
        {
            if (!DB.HaveWriteAccess("ProductionTasks")) return true;
            var productionTaskRw = GammaBase.ProductionTasks
                .Include(
                    pt =>
                        pt.ProductionTaskRWCutting).FirstOrDefault(pt => pt.ProductionTaskBatches.Select(ptb => ptb.ProductionTaskBatchID)
                            .Contains(productionTaskBatchID));
            if (productionTaskRw == null)
            {
                productionTaskRw = new ProductionTasks()
                {
                    ProductionTaskID = ProductionTaskID,
                    PlaceGroupID = (short)PlaceGroup.Rw,
                    ProductionTaskRWCutting = new List<ProductionTaskRWCutting>(),
                    ProductionTaskBatches = new List<ProductionTaskBatches> { GammaBase.ProductionTaskBatches.First(ptb => ptb.ProductionTaskBatchID == productionTaskBatchID) }
                };
                GammaBase.ProductionTasks.Add(productionTaskRw);
            }
            productionTaskRw.DateBegin = DateBegin;
            productionTaskRw.DateEnd = DateEnd;
            productionTaskRw.Quantity = TaskQuantity;
            GammaBase.ProductionTaskRWCutting.RemoveRange(productionTaskRw.ProductionTaskRWCutting);
            foreach (var cutting in Cuttings)
            {
                short index = 0;
                for (int i = 0; i < cutting.Quantity; i++)
                {
                    productionTaskRw.ProductionTaskRWCutting.Add(new ProductionTaskRWCutting()
                    {
                        C1CNomenclatureID = cutting.NomenclatureID,
                        C1CCharacteristicID = cutting.CharacteristicID,
                        ProductionTaskID = productionTaskRw.ProductionTaskID,
                        CutIndex = index,
                        ProductionTaskRWCuttingID = SqlGuidUtil.NewSequentialid(),
                        C1CSpecificationID = cutting.SpecificationID
                    });
                    index++;
                }
            }
            ProductionTaskID = productionTaskRw.ProductionTaskID;
            GammaBase.SaveChanges();
            ProductionTaskSGBViewModel.SaveToModel(productionTaskRw.ProductionTaskID);
            return true;
        }

        private bool CanChooseNomenclature()
        {
            return !IsReadOnly;
        }

        private Cutting _selectedCutting;

        public Cutting SelectedCutting
        {
            get { return _selectedCutting; }
            set
            {
                if (value == null)
                    SelectedCutting.PropertyChanged -= CuttingChanged;
                _selectedCutting = value;
                RaisePropertyChanged("SelectedCutting");
                if (_selectedCutting != null)
                {
                    SelectedCutting.PropertyChanged += CuttingChanged;
                }
            }
        }

        [RequiredCollection(ErrorMessage = @"Необходимо указать конфигурацию")]
        public ItemsChangeObservableCollection<Cutting> Cuttings
        {
            get
            {
                return _cuttings;
            }
            set
            {
                _cuttings = value;
                RaisePropertyChanged("CuttingFormats");
            }
        }
        private DateTime? _dateBegin;
        [Required(ErrorMessage = @"Необходимо указать дату начала")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public DateTime? DateBegin
        {
            get
            {
                return _dateBegin;
            }
            set
            {
                _dateBegin = value;
                if (value != null)
                {
                    DateEnd = value;
                    MessageManager.ProductionTaskRwDateBeginChanged(ProductionTaskBatchID, (DateTime)value);
                }
                RaisePropertyChanged("DateBegin");
            }
        }
        private DateTime? _dateEnd;
        [Required(ErrorMessage = @"Необходимо указать дату окончания")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public DateTime? DateEnd
        {
            get
            {
                return _dateEnd;
            }
            set
            {
                _dateEnd = value;
                RaisePropertyChanged("DateEnd");
            }
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public DelegateCommand ChooseNomenclatureCommand { get; private set; }
        private void ChooseNomenclature()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, NomenclatureChanged);
            MessageManager.FindNomenclature((int)PlaceGroup.Rw);
        }

        private void NomenclatureChanged(Nomenclature1CMessage msg)
        {
            Messenger.Default.Unregister<Nomenclature1CMessage>(this);
            SelectedCutting.NomenclatureID = msg.Nomenclature1CID;
            MessageManager.ProductionTaskRwNomenclatureChanged(ProductionTaskBatchID, Cuttings.Where(c => c.NomenclatureID != null && c.CharacteristicID != null).Select(c => new Nomenclature
            {
                NomenclatureID = c.NomenclatureID,
                CharacteristicID = c.CharacteristicID
            }).ToList());
        }

        private decimal _taskQuantity;
        [Range(1,1000000000,ErrorMessage=@"Задание должно быть больше 0")]
        //[UIAuth(UIAuthLevel.ReadOnly)]
        public decimal TaskQuantity
        {
            get
            {
                return _taskQuantity;
            }
            set
            {
            	_taskQuantity = value;
                RaisePropertyChanged("TaskQuantity");
            }
        }

        public bool IsConfirmed { get; set; }
        private ItemsChangeObservableCollection<Cutting> _cuttings = new ItemsChangeObservableCollection<Cutting>();

        public bool AllowEditing => !IsReadOnly;

        public bool IsReadOnly => IsConfirmed || !DB.HaveWriteAccess("ProductionTasks");
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DelegateCommand AddCuttingCommand { get; private set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DelegateCommand DeleteCuttingCommand { get; private set; }
        public Guid ProductionTaskID
        {
            get;
            set;
        }
        public ProductionTaskSGBViewModel ProductionTaskSGBViewModel { get; private set; }
        private string _totalFormat;
        public string TotalFormat
        {
            get
            {
                return _totalFormat;
            }
            set
            {
            	_totalFormat = value;
                RaisePropertyChanged("TotalFormat");
            }
        }

        public override bool CanSaveExecute()
        {
            if (!IsValid) return false;
            if (Cuttings.Any(c => c.NomenclatureID == null || c.CharacteristicID == null)) return false;
            if (Cuttings.Sum(c => c.Quantity) > 16) return false;
            return true;
        }
    }
}