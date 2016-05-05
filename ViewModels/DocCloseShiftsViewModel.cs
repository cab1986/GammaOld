﻿using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Gamma.Models;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocCloseShiftsViewModel : RootViewModel
    {
        /// <summary>
        /// Initializes a new instance of the DocCloseShiftsViewModel class.
        /// </summary>
        /// 
        public DocCloseShiftsViewModel(GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb;
            Initialize();
            Places = (from p in GammaBase.Places where p.IsProductionPlace ?? false
                      select new
                      Place
                      {
                          PlaceName = p.Name,
                          PlaceID = p.PlaceID
                      }
                     ).ToList();
            Places.Insert(0, new Place() { PlaceName = "Все" });
            FindDocCloseShifts();
        }
        public DocCloseShiftsViewModel(PlaceGroups placeGroup, GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb;
            Initialize();
            Places = (from p in GammaBase.Places
                      where p.PlaceGroupID == (byte)placeGroup && (p.IsProductionPlace ?? false)
                      select new
                      Place
                      {
                          PlaceName = p.Name,
                          PlaceID = p.PlaceID
                      }
                     ).ToList();
            Places.Insert(0, new Place() { PlaceName = "Все" });
            FindDocCloseShifts();
        }

        private void Initialize()
        {
            OpenDocCloseShiftCommand = new DelegateCommand(OpenDocCloseShift, () => SelectedDocCloseShift != null);
            FindDocCloseShiftsCommand = new DelegateCommand(FindDocCloseShifts);
        }

        public List<Place> Places { get; set; }
        public int PlaceID { get; set; }
        private ObservableCollection<DocCloseShift> _docCloseShifts;
        public ObservableCollection<DocCloseShift> DocCloseShifts
        {
            get
            {
                return _docCloseShifts;
            }
            set
            {
            	_docCloseShifts = value;
                RaisePropertyChanged("DocCloseShifts");
            }
        }
        public DocCloseShift SelectedDocCloseShift { get; set; }
        public DelegateCommand OpenDocCloseShiftCommand { get; set; }
        private void OpenDocCloseShift()
        {
            MessageManager.OpenDocCloseShift(SelectedDocCloseShift.DocCloseShiftID);
        }
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public DelegateCommand FindDocCloseShiftsCommand { get; private set; }
        private void FindDocCloseShifts()
        {
            var placeIDs = Places.Select(p => p.PlaceID).ToList();
            DocCloseShifts = new ObservableCollection<DocCloseShift>
            ((
            from d in GammaBase.Docs
            where d.DocTypeID == (byte)DocTypes.DocCloseShift &&
            (PlaceID == 0 ? placeIDs.Contains(d.PlaceID ?? 0) : PlaceID == d.PlaceID) &&
            (DateBegin == null || d.Date >= DateBegin) &&
            (DateEnd == null || d.Date <= DateEnd)
            orderby d.Date descending
            select new DocCloseShift
            {
                DocCloseShiftID = d.DocID,
                Date = d.Date,
                ShiftID = d.ShiftID ?? 0,
                Place = d.Places.Name
            }
            ).Take(100));
        }

        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
    }
}