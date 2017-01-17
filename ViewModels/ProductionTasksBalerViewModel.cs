// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.DialogViewModels;
using Gamma.Entities;
using Gamma.Interfaces;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class ProductionTasksBalerViewModel : RootViewModel, IItemManager
    {
        private List<ProductionTaskBalerGridItem> _productionTasksBaler;

        public ProductionTasksBalerViewModel()
        {
            RefreshCommand = new DelegateCommand(Refresh);
            NewItemCommand = new DelegateCommand(NewProductionTask);
            CreateNewBaleCommand = new DelegateCommand(CreateNewBale, () => WorkSession.PlaceGroup == PlaceGroup.Baler && DB.HaveWriteAccess("ProductBales"));
            Refresh();
        }

        public DelegateCommand NewItemCommand { get; }
        public DelegateCommand<object> EditItemCommand { get; }
        public DelegateCommand DeleteItemCommand { get; }
        public DelegateCommand RefreshCommand { get; }

        public List<ProductionTaskBalerGridItem> ProductionTasksBaler
        {
            get { return _productionTasksBaler; }
            set
            {
                _productionTasksBaler = value;
                RaisePropertyChanged("ProductionTasksBaler");
            }
        }

        public ProductionTaskBalerGridItem SelectedProductionTaskBaler { get; set; }

        private void Refresh()
        {
            UIServices.SetBusyState();
            using (var gammaBase = DB.GammaDb)
            {
                ProductionTasksBaler = new List<ProductionTaskBalerGridItem>(
                    gammaBase.GetProductionTasks((int) BatchKinds.Baler)
                        .Select(pt => new ProductionTaskBalerGridItem(pt.ProductionTaskBatchID, pt.Nomenclature)));
            }
        }

        private void NewProductionTask()
        {
            MessageManager.NewProductionTask(BatchKinds.Baler);
        }

        public DelegateCommand CreateNewBaleCommand { get; private set; }

        private void CreateNewBale()
        {
            var model = new SetBaleWeightDialogModel(SelectedProductionTaskBaler.NomenclatureName);
            var okCommand = new UICommand()
            {
                Caption = "OK",
                IsCancel = false,
                IsDefault = true,
                Command = new DelegateCommand<CancelEventArgs>(
            x => DebugFunc(),
            x => model.Weight > 0),
            };
            var cancelCommand = new UICommand()
            {
                Id = MessageBoxResult.Cancel,
                Caption = "Отмена",
                IsCancel = true,
                IsDefault = false,
            };
            var dialogService = GetService<IDialogService>();
            var result = dialogService.ShowDialog(
                dialogCommands: new List<UICommand>() { okCommand, cancelCommand },
                title: "Вес кипы",
                viewModel: model);
            if (result != okCommand) return;
            using (var gammaBase = DB.GammaDb)
            {
                var productionTask =
                    gammaBase.GetProductionTaskByBatchID(SelectedProductionTaskBaler.ProductionTaskBatchID, (short)BatchKinds.Baler).FirstOrDefault();
                if (productionTask == null)
                {
                    MessageBox.Show("Ошибка при создании кипы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var docId = SqlGuidUtil.NewSequentialid();
                var productId = SqlGuidUtil.NewSequentialid();
                var doc = new Docs()
                {
                    DocID = docId,
                    DocTypeID = (int)DocTypes.DocProduction,
                    IsConfirmed = true,
                    PlaceID = WorkSession.PlaceID,
                    Date = DB.CurrentDateTime,
                    UserID = WorkSession.UserID,
                    ShiftID = WorkSession.ShiftID,
                    PrintName = WorkSession.PrintName,
                    DocProduction = new DocProduction()
                    {
                        DocID = docId,
                        InPlaceID = WorkSession.PlaceID,
                        ProductionTaskID = productionTask.ProductionTaskID,
                        DocProductionProducts = new List<DocProductionProducts>()
                        {
                            new DocProductionProducts()
                            {
                                DocID = docId,
                                ProductID = productId,
                                Quantity = (decimal)model.Weight/1000,
                                C1CNomenclatureID = productionTask.C1CNomenclatureID,
                                C1CCharacteristicID = productionTask.C1CCharacteristicID,
                                Products = new Products()
                                {
                                    ProductID = productId,
                                    ProductKindID = (int) ProductKind.ProductBale,
                                    StateID = (int) ProductState.Good,
                                    ProductBales = new ProductBales()
                                    {
                                        ProductID = productId,
                                        C1CNomenclatureID = (Guid)productionTask.C1CNomenclatureID,
                                        C1CCharacteristicID = productionTask.C1CCharacteristicID,
                                        Weight = (decimal)model.Weight/1000
                                    }
                                }
                            }
                        }
                    }
                };
                gammaBase.Docs.Add(doc);
                gammaBase.SaveChanges();
                ReportManager.PrintReport("Амбалаж", "ProductBale", productId, false, 2);
            }
        }

        private void DebugFunc()
        {
            Debug.Print("Вес задан");
        }
    }
}
