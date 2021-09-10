// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using Gamma.Entities;
using Gamma.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.ComponentModel;
using System.Windows.Controls;

namespace Gamma.Dialogs
{
    /// <summary>
    /// Interaction logic for AddDowntimeDialog.xaml
    /// </summary>
    public partial class AddDowntimeDialog : UserControl //Window, INotifyPropertyChanged
    {
        public AddDowntimeDialog()
        {
            InitializeComponent();
           /* GammaBase = DB.GammaDb;
            EdtType.ItemsSource = (from p in GammaBase.C1CDowntimeTypes
                                       //where (p.IsProductionPlace ?? false) || (p.IsWarehouse ?? false) || (p.IsShipmentWarehouse ?? false) || (p.IsTransitWarehouse ?? false)
                                   select new
                                   DowntimeType
                                   {
                                       DowntimeTypeName = p.Description,
                                       DowntimeTypeID = p.C1CDowntimeTypeID
                                   }
                      ).ToList();
            TypeDetails = (from p in GammaBase.C1CDowntimeTypeDetails
                               //where (p.IsProductionPlace ?? false) || (p.IsWarehouse ?? false) || (p.IsShipmentWarehouse ?? false) || (p.IsTransitWarehouse ?? false)
                           select new
                           DowntimeType
                           {
                               DowntimeTypeName = p.Description,
                               DowntimeTypeID = p.C1CDowntimeTypeDetailID,
                               DowntimeTypeMasterID = p.C1CDowntimeTypeID
                           }
                      ).ToList();
            EdtTypeDetail.ItemsSource = TypeDetails;*/
        }
        /*private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private List<DowntimeType> TypeDetails { get; set; }
        public DateTime DateBegin { get { return EdtDateBegin.DateTime; } }
        public DateTime DateEnd { get { return EdtDateEnd.DateTime; } }
        private Guid _typeID { get; set; }
        public Guid TypeID
        {
            get { return _typeID; }
            set
            {
                _typeID = value;
                //System.Data.DataView view = (System.Data.DataView)EdtType.ItemsSource;
                //view.RowFilter = ("Name like '*" + Cmb.Text + "*'");
                EdtTypeDetail.ItemsSource = TypeDetails.Where(t => t.DowntimeTypeMasterID == value);
                this.OnPropertyChanged("TypeID");
            }
        }
        public Guid? TypeDetailID { get { return (Guid?)EdtTypeDetail.EditValue; } }
        public string Comment { get { return (string)EdtComment.EditValue; } }
        private GammaEntities GammaBase { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var curDate = DateTime.Today;
            EdtDateBegin.DateTime = DateTime.Now;//new DateTime(curDate.Year, curDate.Month, 1).AddMonths(-1);
            EdtDateEnd.DateTime = DateTime.Now;//new DateTime(curDate.Year, curDate.Month, 1).AddSeconds(-1);
            //EdtPlace.EditValue = WorkSession.PlaceID;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }*/
    }
}
