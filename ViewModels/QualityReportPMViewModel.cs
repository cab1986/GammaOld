// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class QualityReportPMViewModel : RootViewModel
    {
        public QualityReportPMViewModel()
        {
            using (var gammaBase = DB.GammaDb)
            {
                Places = gammaBase.Places.Where(
                    p => p.PlaceGroupID == (int) PlaceGroup.PM && WorkSession.BranchIds.Contains(p.BranchID))
                    .Select(p => new Place
                    {
                        PlaceID = p.PlaceID,
                        PlaceName = p.Name
                    }).ToList();
            }
            FindCommand = new DelegateCommand(Find);
        }

        private ItemsChangeObservableCollection<QualityReportItem> _qualityReportItems;
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
        public decimal BasisWeight { get; set; }
        public int Format { get; set; }
        public string Color { get; set; }
        public string Destination { get; set; }
        public int PlaceId { get; set; }

        public List<Place> Places { get; set; }

        public ItemsChangeObservableCollection<QualityReportItem> QualityReportItems
        {
            get { return _qualityReportItems; }
            set
            {
                _qualityReportItems = value;
                RaisePropertyChanged("QualityReportItems");
            }
        }

        public DelegateCommand FindCommand { get; private set; }

        private void Find()
        {
            
        }
    }
}
