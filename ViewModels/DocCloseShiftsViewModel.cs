using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
        public DocCloseShiftsViewModel()
        {
            Initialize();
            Places = (from p in DB.GammaBase.Places where p.IsProductionPlace ?? false
                      select new
                      Place
                      {
                          PlaceName = p.Name,
                          PlaceID = p.PlaceID
                      }
                     ).ToList<Place>();
            Places.Insert(0, new Place() { PlaceName = "Все" });
            FindDocCloseShifts();
        }
        public DocCloseShiftsViewModel(PlaceGroups placeGroup)
        {
            Initialize();
            Places = (from p in DB.GammaBase.Places
                      where p.PlaceGroupID == (byte)placeGroup && (p.IsProductionPlace ?? false)
                      select new
                      Place
                      {
                          PlaceName = p.Name,
                          PlaceID = p.PlaceID
                      }
                     ).ToList<Place>();
            Places.Insert(0, new Place() { PlaceName = "Все" });
            FindDocCloseShifts();
        }

        private void Initialize()
        {
            OpenDocCloseShiftCommand = new RelayCommand(OpenDocCloseShift, () => SelectedDocCloseShift != null);
            FindDocCloseShiftsCommand = new RelayCommand(FindDocCloseShifts);
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
        public RelayCommand OpenDocCloseShiftCommand { get; set; }
        private void OpenDocCloseShift()
        {
            MessageManager.OpenDocCloseShift(SelectedDocCloseShift.DocCloseShiftID);
        }
        public RelayCommand FindDocCloseShiftsCommand { get; private set; }
        private void FindDocCloseShifts()
        {
            var placeIDs = Places.Select(p => p.PlaceID).ToList();
            DocCloseShifts = new ObservableCollection<DocCloseShift>
            ((
            from d in DB.GammaBase.Docs
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