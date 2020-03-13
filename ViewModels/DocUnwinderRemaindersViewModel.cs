// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Gamma.Entities;
using Gamma.Models;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocUnwinderRemaindersViewModel : RootViewModel
    {
        /// <summary>
        /// Initializes a new instance of the DocUnwinderRemaindersViewModel class.
        /// </summary>
        /// 
        public DocUnwinderRemaindersViewModel(GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb;
            Initialize();
            Places = (from p in GammaBase.Places where (p.IsProductionPlace ?? false) || (p.IsShipmentWarehouse ?? false) || (p.IsTransitWarehouse ?? false)
                      select new
                      Place
                      {
                          PlaceName = p.Name,
                          PlaceID = p.PlaceID
                      }
                     ).ToList();
            Places.Insert(0, new Place() { PlaceName = "Все" });
            FindDocUnwinderRemainders();
        }

        public DocUnwinderRemaindersViewModel(PlaceGroup placeGroup, GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb;
            Initialize();
            Places = (from p in GammaBase.Places
                      where p.PlaceGroupID == (byte)placeGroup && ((placeGroup != PlaceGroup.Warehouses && (p.IsProductionPlace ?? false)) || (placeGroup == PlaceGroup.Warehouses && ((p.IsShipmentWarehouse ?? false) || (p.IsTransitWarehouse ?? false))))
                      select new
                      Place
                      {
                          PlaceName = p.Name,
                          PlaceID = p.PlaceID
                      }
                     ).ToList();
            Places.Insert(0, new Place() { PlaceName = "Все" });
            FindDocUnwinderRemainders();
        }

        private void Initialize()
        {
            OpenDocUnwinderRemainderCommand = new DelegateCommand(OpenDocUnwinderRemainder, () => SelectedDocUnwinderRemainder != null);
            FindDocUnwinderRemaindersCommand = new DelegateCommand(FindDocUnwinderRemainders);
        }

        public List<Place> Places { get; set; }
        public int PlaceID { get; set; }
        private ObservableCollection<DocCloseShift> _docUnwinderRemainders;
        public ObservableCollection<DocCloseShift> DocUnwinderRemainders
        {
            get
            {
                return _docUnwinderRemainders;
            }
            set
            {
            	_docUnwinderRemainders = value;
                RaisePropertyChanged("DocUnwinderRemainders");
            }
        }
        public DocCloseShift SelectedDocUnwinderRemainder { get; set; }
        public DelegateCommand OpenDocUnwinderRemainderCommand { get; set; }
        private void OpenDocUnwinderRemainder()
        {
            MessageManager.OpenDocUnwinderRemainder(SelectedDocUnwinderRemainder.DocCloseShiftID);
        }
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public DelegateCommand FindDocUnwinderRemaindersCommand { get; private set; }
        private void FindDocUnwinderRemainders()
        {
            var placeIDs = Places.Select(p => p.PlaceID).ToList();
            DocUnwinderRemainders = new ObservableCollection<DocCloseShift>
            ((
            from d in GammaBase.Docs
            where d.DocTypeID == (byte)DocTypes.DocUnwinderRemainder &&
            (PlaceID == 0 ? placeIDs.Contains(d.PlaceID ?? 0) : PlaceID == d.PlaceID) &&
            (DateBegin == null || d.Date >= DateBegin) &&
            (DateEnd == null || d.Date <= DateEnd)
            orderby d.Date descending
            select new DocCloseShift
            {
                DocCloseShiftID = d.DocID,
                Date = d.Date,
                ShiftID = d.ShiftID ?? 0,
                Place = d.Places.Name,
                User = d.Users.Name,
                Person = d.Persons.Name
            }
            ).Take(120));
        }

        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
    }
}