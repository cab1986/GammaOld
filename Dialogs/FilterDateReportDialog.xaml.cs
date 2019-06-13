// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using Gamma.Entities;
using Gamma.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;


namespace Gamma.Dialogs
{
    /// <summary>
    /// Interaction logic for FilterDateReportDialog.xaml
    /// </summary>
    public partial class FilterDateReportDialog : Window
    {
        public FilterDateReportDialog()
        {
            InitializeComponent();
            GammaBase = DB.GammaDb;
            EdtPlace.ItemsSource = (from p in GammaBase.Places
                          //where (p.IsProductionPlace ?? false) || (p.IsWarehouse ?? false) || (p.IsShipmentWarehouse ?? false) || (p.IsTransitWarehouse ?? false)
                      select new
                      Place
                      {
                          PlaceName = p.Name,
                          PlaceID = p.PlaceID
                      }
                      ).ToList();

        }
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public DateTime DateBegin { get { return EdtDateBegin.DateTime; } }
        public DateTime DateEnd { get { return EdtDateEnd.DateTime; } }
        public int? PlaceID { get { return (int?)EdtPlace.EditValue; } }
/*        public List<Place> Places { get; set; }
        private int? _placeID;
        public int? PlaceID
        {
            get { return _placeID; }
            set
            {
                _placeID = value;
                //RaisePropertyChanged("PlaceID");
            }
        }
        */
        private GammaEntities GammaBase { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var curDate = DateTime.Today;
            EdtDateBegin.DateTime = new DateTime(curDate.Year, curDate.Month, 1).AddMonths(-1);
            EdtDateEnd.DateTime = new DateTime(curDate.Year, curDate.Month, 1).AddSeconds(-1);
            EdtPlace.EditValue = WorkSession.PlaceID;
        }
    }
}
