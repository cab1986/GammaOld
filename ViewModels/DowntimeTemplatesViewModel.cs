// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Entities;
using Gamma.Models;
using System.Collections.Generic;
using System.Diagnostics;
using Gamma.DialogViewModels;
using System.ComponentModel;

namespace Gamma.ViewModels
{
    /// <summary>
    /// Класс для отображения работников склада и управления ими
    /// </summary>
    public class DowntimeTemplatesViewModel : RootViewModel
    {
        public DowntimeTemplatesViewModel()
        {
            AddDowntimeTemplateCommand = new DelegateCommand(AddDowntimeTemplate, () => DowntimeTypeID != null);
            ChangeDowntimeTemplateCommand = new DelegateCommand(ChangeDowntimeTemplate, () => SelectedDowntimeTemplate != null);
            DeleteDowntimeTemplateCommand = new DelegateCommand(DeleteDowntimeTemplate, () => SelectedDowntimeTemplate != null);
            RefreshDowntimeTemplates();
            Bars.Add(ReportManager.GetReportBar("DowntimeTemplatesList", VMID));
            Bars.Add(new BarViewModel
            {
                Name = "",
                Commands = new ObservableCollection<BarCommand<object>>
                {
                    new BarCommand<object>(p => DeleteDowntimeTemplate())
                    {
                        Caption = "Удалить"
                    }
                }
            });
            Places = (from p in WorkSession.Places
                      where (p.IsProductionPlace ?? false)
                      select new
                      Place
                      {
                          PlaceName = p.Name,
                          PlaceID = p.PlaceID
                      }
                    ).ToList();
            DowntimeTypes = (from p in GammaBase.C1CDowntimeTypes
                             where ((!p.C1CDeleted ?? true) && (!p.Folder ?? true))
                             select new
                      DowntimeType
                      {
                          DowntimeTypeName = p.Description,
                          DowntimeTypeID = p.C1CDowntimeTypeID
                      }
                    ).ToList();
            DowntimeTypeDetails = (from p in GammaBase.C1CDowntimeTypeDetails
                                   where ((!p.C1CDeleted ?? true) && (!p.Folder ?? true))
                                   select new
                             DowntimeType
                             {
                                 DowntimeTypeName = p.Description,
                                 DowntimeTypeID = p.C1CDowntimeTypeDetailID,
                                 DowntimeTypeMasterID = p.C1CDowntimeTypeID
                             }
                    ).ToList();
            DowntimeTypeDetailsFiltered = new List<DowntimeType>(DowntimeTypeDetails);
            EquipmentNodes = (from p in GammaBase.C1CEquipmentNodes
                              where ((!p.C1CDeleted ?? true) && (!p.Folder ?? true))
                              select new
                             EquipmentNode
                             {
                                 EquipmentNodeName = p.Description,
                                 EquipmentNodeID = p.C1CEquipmentNodeID
                             }
                    ).ToList();
            EquipmentNodeDetails = (from p in GammaBase.C1CEquipmentNodeDetails
                                    where ((!p.C1CDeleted ?? true) && (!p.Folder ?? true))
                                    select new
                                   EquipmentNode
                                   {
                                       EquipmentNodeName = p.Description,
                                       EquipmentNodeID = p.C1CEquipmentNodeDetailID,
                                       EquipmentNodeMasterID = p.C1CEquipmentNodeID
                                   }
                    ).ToList();
            EquipmentNodeDetailsFiltered = new List<EquipmentNode>(EquipmentNodeDetails);
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
        }

        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != VMID) return;
            ReportManager.PrintReport(msg.ReportID);
        }

        private Guid VMID { get; set; } = Guid.NewGuid();

        private void AddDowntimeTemplate()
        {
            if (Name != null)
            {
                var DowntimeTemplate = new DowntimeTemplates()
                {
                    DowntimeTemplateID = SqlGuidUtil.NewSequentialid(),
                    Name = Name,
                    PlaceID = (int)PlaceID,
                    C1CDowntimeTypeID = DowntimeTypeID,
                    C1CDowntimeTypeDetailID = DowntimeTypeDetailID,
                    C1CEquipmentNodeID = EquipmentNodeID,
                    C1CEquipmentNodeDetailID = EquipmentNodeDetailID,
                    Comment = Comment,
                    Duration = Duration
                };
                GammaBase.DowntimeTemplates.Add(DowntimeTemplate);
                GammaBase.SaveChanges();
                RefreshDowntimeTemplates();
            }
            else MessageBox.Show("Поле наименование не должно быть пустым");
        }

        private void DebugFunc()
        {
            Debug.Print("Кол-во задано");
        }

        private void ChangeDowntimeTemplate()
        { 
            //MessageBox.Show("Выбрано" + SelectedDowntimeTemplate.DowntimeTemplateID.ToString(), SelectedDowntimeTemplate.Name);
            if (SelectedDowntimeTemplate == null) return;
            var model = new AddDowntimeDialogModel(SelectedDowntimeTemplate.DowntimeTemplateID, SelectedDowntimeTemplate.PlaceID, SelectedDowntimeTemplate.DowntimeTypeID, SelectedDowntimeTemplate.DowntimeTypeDetailID, SelectedDowntimeTemplate.EquipmentNodeID, SelectedDowntimeTemplate.EquipmentNodeDetailID, SelectedDowntimeTemplate.Duration, SelectedDowntimeTemplate.Name, SelectedDowntimeTemplate.Comment);
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
            var dialogService = GetService<IDialogService>("AddDowntimeDialog");
            var result = dialogService.ShowDialog(
                dialogCommands: new List<UICommand>() { okCommand, cancelCommand },
                title: "Изменение простоя",
                viewModel: model);
            if (result == okCommand)
            {

                var DowntimeTemplate = GammaBase.DowntimeTemplates.Find(SelectedDowntimeTemplate.DowntimeTemplateID);

                DowntimeTemplate.Name = model.Name;
                DowntimeTemplate.PlaceID = (int)model.PlaceID;
                DowntimeTemplate.C1CDowntimeTypeID = model.TypeID;
                DowntimeTemplate.C1CDowntimeTypeDetailID = model.TypeDetailID;
                DowntimeTemplate.C1CEquipmentNodeID = model.EquipmentNodeID;
                DowntimeTemplate.C1CEquipmentNodeDetailID = model.EquipmentNodeDetailID;
                DowntimeTemplate.Comment = model.Comment;
                DowntimeTemplate.Duration = model.Duration;

                GammaBase.SaveChanges();
            }

            RefreshDowntimeTemplates();
        }

        private void DeleteDowntimeTemplate()
        {
            if (SelectedDowntimeTemplate == null) return;
            /*if (GammaBase.DocShipmentOrders.Any(d => d.OutActiveDowntimeTemplateID == SelectedDowntimeTemplate.DowntimeTemplateId || d.InActiveDowntimeTemplateID == SelectedDowntimeTemplate.DowntimeTemplateId) ||
                GammaBase.DocInProducts.Any(d => d.DowntimeTemplateID == SelectedDowntimeTemplate.DowntimeTemplateId) ||
                GammaBase.DocOutProducts.Any(d => d.DowntimeTemplateID == SelectedDowntimeTemplate.DowntimeTemplateId))
            {
                MessageBox.Show("Данный сотрудник уже собирал приказы или ему назначен приказ. Удалить невозможно",
                    "Удаление", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }*/
            GammaBase.DowntimeTemplates.Remove(GammaBase.DowntimeTemplates.FirstOrDefault(p => p.DowntimeTemplateID == SelectedDowntimeTemplate.DowntimeTemplateID));
            GammaBase.SaveChanges();
            RefreshDowntimeTemplates();
        }

        private void RefreshDowntimeTemplates()
        {
            DowntimeTemplates = new ObservableCollection<DowntimeTemplate>(GammaBase.DowntimeTemplates
                .Where(p => (PlaceID == null || (PlaceID != null && p.PlaceID == PlaceID)))
                .Select(p => new DowntimeTemplate
                {
                    DowntimeTemplateID = p.DowntimeTemplateID,
                    Name = p.Name,
                    PlaceID = p.PlaceID,
                    PlaceName = p.Places.Name,
                    DowntimeTypeID = p.C1CDowntimeTypeID,
                    DowntimeTypeDetailID = p.C1CDowntimeTypeDetailID,
                    EquipmentNodeID = p.C1CEquipmentNodeID ?? Guid.Empty,
                    EquipmentNodeDetailID = p.C1CEquipmentNodeDetailID,
                    DowntimeType = p.C1CDowntimeTypes.Description,
                    DowntimeTypeDetail = p.C1CDowntimeTypeDetails.Description,
                    EquipmentNode = p.C1CEquipmentNodes.Description,
                    EquipmentNodeDetail = p.C1CEquipmentNodeDetails.Description,
                    Comment = p.Comment,
                    Duration = p.Duration
                }));
        }

        public DowntimeTemplate SelectedDowntimeTemplate { get; set; }

        public string Comment { get; set; }

        public string Name { get; set; }

        public int? Duration { get; set; }

        public List<Place> Places { get; set; }

        public List<DowntimeType> DowntimeTypes { get; private set; }
        public List<DowntimeType> DowntimeTypeDetails { get; private set; }
        public List<DowntimeType> DowntimeTypeDetailsFiltered { get; private set; }
        public List<EquipmentNode> EquipmentNodes { get; private set; }
        public List<EquipmentNode> EquipmentNodeDetails { get; private set; }
        public List<EquipmentNode> EquipmentNodeDetailsFiltered { get; private set; }

        public DelegateCommand AddDowntimeTemplateCommand { get; set; }
        public DelegateCommand ChangeDowntimeTemplateCommand { get; set; }
        public DelegateCommand DeleteDowntimeTemplateCommand { get; private set; }

        private int? _placeID;

        public int? PlaceID
        {
            get { return _placeID; }
            set
            {
                _placeID = value;
                RaisePropertyChanged("PlaceID");
                RefreshDowntimeTemplates();
            }
        }


        private Guid _downtimeTypeID;

        public Guid DowntimeTypeID
        {
            get { return _downtimeTypeID; }
            set
            {
                _downtimeTypeID = value;
                RaisePropertyChanged("DowntimeTypeID");
                DowntimeTypeDetailsFiltered = new List<DowntimeType>(DowntimeTypeDetails.FindAll(t => t.DowntimeTypeMasterID == value));
                RaisePropertyChanged("DowntimeTypeDetailsFiltered");
                RefreshDowntimeTemplates();
            }
        }

        private Guid? _downtimeTypeDetailID;

        public Guid? DowntimeTypeDetailID
        {
            get { return _downtimeTypeDetailID; }
            set
            {
                _downtimeTypeDetailID = value;
                RaisePropertyChanged("DowntimeTypeDetailID");
                RefreshDowntimeTemplates();
            }
        }

        private Guid? _EquipmentNodeID;

        public Guid? EquipmentNodeID
        {
            get { return _EquipmentNodeID; }
            set
            {
                _EquipmentNodeID = value;
                RaisePropertyChanged("EquipmentNodeID");
                EquipmentNodeDetailsFiltered = new List<EquipmentNode>(EquipmentNodeDetails.FindAll(t => t.EquipmentNodeMasterID == value));
                RaisePropertyChanged("EquipmentNodeDetailsFiltered");
                RefreshDowntimeTemplates();
            }
        }

        private Guid? _EquipmentNodeDetailID;

        public Guid? EquipmentNodeDetailID
        {
            get { return _EquipmentNodeDetailID; }
            set
            {
                _EquipmentNodeDetailID = value;
                RaisePropertyChanged("EquipmentNodeDetailID");
                RefreshDowntimeTemplates();
            }
        }

        private ObservableCollection<DowntimeTemplate> _DowntimeTemplates;

        public ObservableCollection<DowntimeTemplate> DowntimeTemplates
        {
            get { return _DowntimeTemplates; }
            set
            {
                _DowntimeTemplates = value;
                RaisePropertyChanged("DowntimeTemplates");
            }
        }

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

    }
}
