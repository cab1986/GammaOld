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

namespace Gamma.ViewModels
{
    /// <summary>
    /// Класс для отображения работников склада и управления ими
    /// </summary>
    public class PlaceAuxiliaryMaterialsViewModel : DbEditItemWithNomenclatureViewModel
    {
        public PlaceAuxiliaryMaterialsViewModel()
        {
            ChooseAuxiliaryMaterialCommand = new DelegateCommand(ChooseAuxiliaryMaterial, () => PlaceID != null);
            AddPlaceAuxiliaryMaterialCommand = new DelegateCommand(AddPlaceAuxiliaryMaterial, () => NomenclatureID != null);
            DeletePlaceAuxiliaryMaterialCommand = new DelegateCommand(DeletePlaceAuxiliaryMaterial, () => SelectedPlaceAuxiliaryMaterial != null);
            RefreshPlaceAuxiliaryMaterials();
            Bars.Add(ReportManager.GetReportBar("PlaceAuxiliaryMaterialsList", VMID));
            Bars.Add(new BarViewModel
            {
                Name = "",
                Commands = new ObservableCollection<BarCommand<object>>
                {
                    new BarCommand<object>(p => DeletePlaceAuxiliaryMaterial())
                    {
                        Caption = "Удалить"
                    }
                }
            });
            Places = (from p in GammaBase.Places
                      where (p.IsProductionPlace ?? false)
                      select new
                      Place
                      {
                          PlaceName = p.Name,
                          PlaceID = p.PlaceID
                      }
                    ).ToList();
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
        }

        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != VMID) return;
            ReportManager.PrintReport(msg.ReportID);
        }

        private Guid VMID { get; set; } = Guid.NewGuid();

        private void ChooseAuxiliaryMaterial()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, SetAuxiliaryMaterialNomenclature);
            MessageManager.FindNomenclature(MaterialType.MaterialsSGI);
        }

        private void SetAuxiliaryMaterialNomenclature(Nomenclature1CMessage msg)
        {
            Messenger.Default.Unregister<Nomenclature1CMessage>(this);
            NomenclatureID = msg.Nomenclature1CID;
        }

        private void AddPlaceAuxiliaryMaterial()
        {
            var PlaceAuxiliaryMaterial = new PlaceAuxiliaryMaterials()
            {
                PlaceAuxiliaryMaterialID = SqlGuidUtil.NewSequentialid(),
                PlaceID = (int)PlaceID,
                C1CNomenclatureID = (Guid)NomenclatureID,
                C1CCharacteristicID = CharacteristicID
            };
            GammaBase.PlaceAuxiliaryMaterials.Add(PlaceAuxiliaryMaterial);
            GammaBase.SaveChanges();
            RefreshPlaceAuxiliaryMaterials();
        }

        private void DeletePlaceAuxiliaryMaterial()
        {
            if (SelectedPlaceAuxiliaryMaterial == null) return;
            /*if (GammaBase.DocShipmentOrders.Any(d => d.OutActivePlaceAuxiliaryMaterialID == SelectedPlaceAuxiliaryMaterial.PlaceAuxiliaryMaterialId || d.InActivePlaceAuxiliaryMaterialID == SelectedPlaceAuxiliaryMaterial.PlaceAuxiliaryMaterialId) ||
                GammaBase.DocInProducts.Any(d => d.PlaceAuxiliaryMaterialID == SelectedPlaceAuxiliaryMaterial.PlaceAuxiliaryMaterialId) ||
                GammaBase.DocOutProducts.Any(d => d.PlaceAuxiliaryMaterialID == SelectedPlaceAuxiliaryMaterial.PlaceAuxiliaryMaterialId))
            {
                MessageBox.Show("Данный сотрудник уже собирал приказы или ему назначен приказ. Удалить невозможно",
                    "Удаление", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }*/
            GammaBase.PlaceAuxiliaryMaterials.Remove(GammaBase.PlaceAuxiliaryMaterials.FirstOrDefault(p => p.PlaceAuxiliaryMaterialID == SelectedPlaceAuxiliaryMaterial.PlaceAuxiliaryMaterialID));
            GammaBase.SaveChanges();
            RefreshPlaceAuxiliaryMaterials();
        }

        private void RefreshPlaceAuxiliaryMaterials()
        {
            PlaceAuxiliaryMaterials = new ObservableCollection<PlaceAuxiliaryMaterial>(GammaBase.PlaceAuxiliaryMaterials
                .Where(p => (PlaceID == null || (PlaceID != null && p.PlaceID == PlaceID)))
                .Select(p => new PlaceAuxiliaryMaterial
                {
                    PlaceAuxiliaryMaterialID = p.PlaceAuxiliaryMaterialID,
                    PlaceID = p.PlaceID,
                    PlaceName = p.Places.Name,
                    NomenclatureID = p.C1CNomenclatureID,
                    CharacteristicID = p.C1CCharacteristicID,
                    NomenclatureName = p.C1CNomenclature.Name,
                    CharacteristicName = p.C1CCharacteristics.Name
                }));
        }

        public PlaceAuxiliaryMaterial SelectedPlaceAuxiliaryMaterial { get; set; }

        public string Comment { get; set; }

        public List<Place> Places { get; set; }

        private ObservableCollection<PlaceAuxiliaryMaterial> _placeAuxiliaryMaterials;

        public ObservableCollection<PlaceAuxiliaryMaterial> PlaceAuxiliaryMaterials
        {
            get { return _placeAuxiliaryMaterials; }
            set
            {
                _placeAuxiliaryMaterials = value;
                RaisePropertyChanged("PlaceAuxiliaryMaterials");
            }
        }

        public DelegateCommand ChooseAuxiliaryMaterialCommand { get; private set; }
        public DelegateCommand AddPlaceAuxiliaryMaterialCommand { get; private set; }
        public DelegateCommand DeletePlaceAuxiliaryMaterialCommand { get; private set; }

        private int? _placeID;

        public int? PlaceID
        {
            get { return _placeID; }
            set
            {
                _placeID = value;
                RaisePropertyChanged("PlaceID");
                RefreshPlaceAuxiliaryMaterials();
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
