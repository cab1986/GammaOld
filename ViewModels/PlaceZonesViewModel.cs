// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Models;
using System;

namespace Gamma.ViewModels
{
    public class PlaceZonesViewModel : RootViewModel
    {
        public PlaceZonesViewModel()
        {
            Bars.Add(ReportManager.GetReportBar("PlaceZones", VMID));

            Places = GammaBase.Places.Where(p => WorkSession.BranchIds.Contains(p.BranchID) && (p.IsWarehouse ?? false))
                .Select(p => new Place()
                {
                    PlaceID = p.PlaceID,
                    PlaceName = p.Name
                }).ToList();
            if (Places.Count > 0) SelectedPlace = Places.First();
            IsReadOnly = !DB.HaveWriteAccess("PlaceZones");
            AddPlaceZoneCommand = new DelegateCommand(AddPlaceZone, () => !IsReadOnly && SelectedPlace != null);
            DeletePlaceZoneCommand = new DelegateCommand(DeletePlaceZone, () => !IsReadOnly && SelectedPlaceZone != null);
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
        }

        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != VMID) return;
            ReportManager.PrintReport(msg.ReportID);
        }

        public bool IsReadOnly { get; set; }

        private Guid VMID { get; } = Guid.NewGuid();
        public List<BarViewModel> Bars { get; set; } = new List<BarViewModel>();


        private void PlaceZonesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (IsReadOnly || args.NewItems == null) return;
            if (args.Action != NotifyCollectionChangedAction.Add || SelectedPlace == null) return;
            foreach (var placeZone in args.NewItems.OfType<PlaceZone>())
            {
                GammaBase.PlaceZones.Add(new PlaceZones
                {
                    PlaceID = SelectedPlace.PlaceID,
                    PlaceZoneID = placeZone.PlaceZoneId,
                    PlaceZoneParentID = placeZone.PlaceZoneParentId,
                    Name = placeZone.Name
                });
            }
            GammaBase.SaveChanges();
        }

        private List<Place> _places;
        public List<Place> Places
        {
            get { return _places; }
            set
            {
                _places = value;
                RaisePropertyChanged("Places");
            }
        }

        private ItemsChangeObservableCollection<PlaceZone> _placeZones;
        public ItemsChangeObservableCollection<PlaceZone> PlaceZones
        {
            get { return _placeZones; }
            set
            {
                if (PlaceZones != null)
                    PlaceZones.CollectionChanged -= PlaceZonesOnCollectionChanged;
                _placeZones = value;
                if (_placeZones != null)
                {
                    PlaceZones.CollectionChanged += PlaceZonesOnCollectionChanged;
                }
                SelectedPlaceZone = null;
                RaisePropertyChanged("PlaceZones");
            }
        }

        private Place _selectedPlace;
        public Place SelectedPlace
        {
            get { return _selectedPlace; }
            set
            {
                _selectedPlace = value;
                if (SelectedPlace != null)
                {
                    PlaceZones = new ItemsChangeObservableCollection<PlaceZone>(
                        GammaBase.PlaceZones.Where(pz => pz.PlaceID == SelectedPlace.PlaceID)
                        .Select(pz => new PlaceZone
                        {
                            PlaceZoneId = pz.PlaceZoneID,
                            PlaceZoneParentId = pz.PlaceZoneParentID,
                            Name = pz.Name
                        }));
                }
                RaisePropertyChanged("SelectedPlace");
            }
        }

        private PlaceZone _selectedPlaceZone;

       
        public PlaceZone SelectedPlaceZone
        {
            get { return _selectedPlaceZone; }
            set
            {
                if (SelectedPlaceZone != null)
                    SelectedPlaceZone.PlaceZoneChanged -= SelectedPlaceZoneOnPlaceZoneChanged;
                _selectedPlaceZone = value;
                if (SelectedPlaceZone != null)
                {
                    SelectedPlaceZone.PlaceZoneChanged += SelectedPlaceZoneOnPlaceZoneChanged;
                }
                RaisePropertyChanged("SelectedPlaceZone");
            }
        }

        private void SelectedPlaceZoneOnPlaceZoneChanged()
        {
            var gammaPlaceZone =
                       GammaBase.PlaceZones.FirstOrDefault(pz => pz.PlaceZoneID == SelectedPlaceZone.PlaceZoneId);
            if (gammaPlaceZone == null) return;
            gammaPlaceZone.Name = SelectedPlaceZone.Name;
            gammaPlaceZone.PlaceZoneParentID = SelectedPlaceZone.PlaceZoneParentId;
            GammaBase.SaveChanges();
        }

        public DelegateCommand AddPlaceZoneCommand { get; private set; }

        private void AddPlaceZone()
        {
            if (SelectedPlace == null) return;
            var placeZone = new PlaceZone
            {
                PlaceZoneId = SqlGuidUtil.NewSequentialid(),
                PlaceZoneParentId = SelectedPlaceZone?.PlaceZoneId,
                Name = "Новая зона"
            };
            PlaceZones.Add(placeZone);
        }

        public DelegateCommand DeletePlaceZoneCommand { get; private set; }

        private void DeletePlaceZone()
        {
            if (SelectedPlaceZone == null) return;
            var result = GammaBase.DeletPlaceZone(SelectedPlaceZone.PlaceZoneId).First();
            if (string.IsNullOrEmpty(result))
            {
                PlaceZones.Remove(SelectedPlaceZone);
            }
            else
            {
                MessageBox.Show(result, @"Ошибка удаления зоны", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }
    }
}
