// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.ComponentModel.DataAnnotations;
using Gamma.ViewModels;
using System;
using System.Collections.Generic;
using Gamma.Models;
using System.Linq;


namespace Gamma.DialogViewModels
{
    public partial class AddDowntimeDialogModel : ValidationViewModelBase
    {
        public AddDowntimeDialogModel()
        {
            //var curDate = DateTime.Today;
            DateBegin = DateTime.Today;//new DateTime(curDate.Year, curDate.Month, 1).AddMonths(-1);
            DateEnd = DateTime.Today;//new DateTime(curDate.Year, curDate.Month, 1).AddSeconds(-1);

            GammaBase = DB.GammaDb;
            Types = (from p in GammaBase.C1CDowntimeTypes
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
            TypeDetailsFiltered = new List<DowntimeType>(TypeDetails);
        }

        public List<DowntimeType> Types { get; private set; }
        public List<DowntimeType> TypeDetails { get; private set; }
        public List<DowntimeType> TypeDetailsFiltered { get; private set; }
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
        private Guid _typeID { get; set; }
        public Guid TypeID
        {
            get { return _typeID; }
            set
            {
                _typeID = value;
                //System.Data.DataView view = (System.Data.DataView)EdtType.ItemsSource;
                //view.RowFilter = ("Name like '*" + Cmb.Text + "*'");
                TypeDetailsFiltered = new List<DowntimeType>(TypeDetails.FindAll(t => t.DowntimeTypeMasterID == value));
                RaisePropertyChanged("TypeDetailsFiltered");
            }
        }
        public Guid? TypeDetailID { get; set; }
        public string Comment { get; set; }
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
    }
}
