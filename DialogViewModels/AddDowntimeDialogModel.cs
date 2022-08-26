// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.ComponentModel.DataAnnotations;
using Gamma.ViewModels;
using System;
using System.Collections.Generic;
using Gamma.Models;
using System.Linq;
using System.Windows;

namespace Gamma.DialogViewModels
{
    public partial class AddDowntimeDialogModel : ValidationViewModelBase
    {
        public AddDowntimeDialogModel(int? placeID)
        {
            var curDate = DateTime.Now;
            DateBegin = curDate;//new DateTime(curDate.Year, curDate.Month, 1).AddMonths(-1);
            DateEnd = curDate;//new DateTime(curDate.Year, curDate.Month, 1).AddSeconds(-1);
            Name = "1";
            GammaBase = DB.GammaDb;
            Types = (from p in GammaBase.C1CDowntimeTypes
                                       where ((!p.C1CDeleted ?? true) && (!p.Folder ?? true))
                                   select new
                                   DowntimeType
                                   {
                                       DowntimeTypeName = p.Description + " (" + p.DowntimeKind + ")",
                                       DowntimeTypeID = p.C1CDowntimeTypeID,
                                       DowntimeKind = p.DowntimeKind
                                   }
                      ).ToList();
            TypeDetails = (from p in GammaBase.C1CDowntimeTypeDetails
                           where ((!p.C1CDeleted ?? true) && (!p.Folder ?? true))
                           select new
                           DowntimeType
                           {
                               DowntimeTypeName = p.Description,
                               DowntimeTypeID = p.C1CDowntimeTypeDetailID,
                               DowntimeTypeMasterID = p.C1CDowntimeTypeID
                           }
                      ).ToList();
            TypeDetailsFiltered = new List<DowntimeType>(TypeDetails);
            EquipmentNodes = (from p in GammaBase.C1CEquipmentNodes
                              join pl in GammaBase.C1CEquipmentNodesPlaces on p.C1CEquipmentNodeID equals pl.C1CEquipmentNodeID
                              join places in GammaBase.Places on pl.C1CPlaceID equals places.C1CPlaceID
                              where ((!p.C1CDeleted ?? true) && (!p.Folder ?? true) && (places.PlaceID == placeID))
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
        }

        public AddDowntimeDialogModel(int? placeID, Guid? downtimeTypeID, Guid? downtimeTypeDetailID = null, Guid? equipmentNodeID = null, Guid? equipmentNodeDetailID = null, int? duration = null, string comment = null):this(placeID)
        {
            if (downtimeTypeID != null) TypeID = (Guid)downtimeTypeID;
            if (downtimeTypeDetailID != null) TypeDetailID = (Guid)downtimeTypeDetailID;
            if (equipmentNodeID != null) EquipmentNodeID = (Guid)equipmentNodeID;
            if (equipmentNodeDetailID != null) EquipmentNodeDetailID = (Guid)equipmentNodeDetailID;
            if (comment != null) Comment = comment;
            if (duration != null) DateEnd = DateBegin.AddMinutes((int)duration);
        }

        public AddDowntimeDialogModel(Guid? DowntimeTemplateID, int? placeID, Guid? downtimeTypeID, Guid? downtimeTypeDetailID = null, Guid? equipmentNodeID = null, Guid? equipmentNodeDetailID = null, int? duration = null, string name = null, string comment = null) : this(placeID)
        {
            if (placeID != null) PlaceID = placeID;
            if (downtimeTypeID != null) TypeID = (Guid)downtimeTypeID;
            if (downtimeTypeDetailID != null) TypeDetailID = (Guid)downtimeTypeDetailID;
            if (equipmentNodeID != null) EquipmentNodeID = (Guid)equipmentNodeID;
            if (equipmentNodeDetailID != null) EquipmentNodeDetailID = (Guid)equipmentNodeDetailID;
            if (duration != null) Duration = duration;
            if (name != null) Name = name;
            if (comment != null) Comment = comment;
            Places = (from p in WorkSession.Places
                      where (p.IsProductionPlace ?? false)
                      select new
                      Place
                      {
                          PlaceName = p.Name,
                          PlaceID = p.PlaceID
                      }
                    ).ToList();
            VisiblityChangeDT = Visibility.Visible;
            VisiblityAddDT = Visibility.Collapsed;
            EnabledChangeDuration = true;
        }
        [Required(ErrorMessage = @"Поле Наименование не может быть пустым")]
        public string Name { get; set; }
        public List<Place> Places { get; set; }
        private int? _placeID;

        public int? PlaceID
        {
            get { return _placeID; }
            set
            {
                _placeID = value;
                RaisePropertyChanged("PlaceID");
            }
        }
        private Visibility _visiblityChangeDT = Visibility.Collapsed;
        public Visibility VisiblityChangeDT
        {
            get { return _visiblityChangeDT; }
            set
            {
                _visiblityChangeDT = value;
                RaisePropertyChanged("VisiblityChangeDT");
            }
        }
        private Visibility _visiblityAddDT = Visibility.Visible;
        public Visibility VisiblityAddDT
        {
            get { return _visiblityAddDT; }
            set
            {
                _visiblityAddDT = value;
                RaisePropertyChanged("VisiblityAddDT");
            }
        }
        public bool EnabledChangeDuration
        {
            get { return _enabledChangeDuration; }
            set
            {
                _enabledChangeDuration = value;
                RaisePropertyChanged("EnabledChangeDuration");
            }
        }
        private bool _enabledChangeDuration = false;


        public List<DowntimeType> Types { get; private set; }
        public List<DowntimeType> TypeDetails { get; private set; }
        public List<DowntimeType> TypeDetailsFiltered { get; private set; }
        public List<EquipmentNode> EquipmentNodes { get; private set; }
        public List<EquipmentNode> EquipmentNodeDetails { get; private set; }
        public List<EquipmentNode> EquipmentNodeDetailsFiltered { get; private set; }
        public override bool IsValid => base.IsValid && TypeID != Guid.Empty && EquipmentNodeID != Guid.Empty;
        private DateTime _dateBegin { get; set; }
        public DateTime DateBegin
        {
            get { return _dateBegin; }
            set
            {
                _dateBegin = value;
                RefreshDates();
                RaisePropertyChanged("DateBegin");
            }
        }
        private DateTime _dateEnd { get; set; }
        public DateTime DateEnd
        {
            get { return _dateEnd; }
            set
            {
                _dateEnd = value;
                RefreshDates();
                RaisePropertyChanged("DateEnd");
            }
        }
        public int? Duration { get; set; }

        public void RefreshDates()
        {
            Duration = (int)(DateEnd - DateBegin).TotalMinutes;
            RaisePropertyChanged("Duration");
        }

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
        private Guid _equipmentNodeID { get; set; }
        public Guid EquipmentNodeID
        {
            get { return _equipmentNodeID; }
            set
            {
                _equipmentNodeID = value;
                //System.Data.DataView view = (System.Data.DataView)EdtEquipmentNode.ItemsSource;
                //view.RowFilter = ("Name like '*" + Cmb.Text + "*'");
                EquipmentNodeDetailsFiltered = new List<EquipmentNode>(EquipmentNodeDetails.FindAll(t => t.EquipmentNodeMasterID == value));
                RaisePropertyChanged("EquipmentNodeDetailsFiltered");
            }
        }
        public Guid? EquipmentNodeDetailID { get; set; }
        public string Comment { get; set; }
        public bool IsSaveEnabled => IsValid && (Types.FirstOrDefault(t => t.DowntimeTypeID == TypeID && (t.DowntimeKind == "Внеплановый" || t.DowntimeKind == "Недоступность")) == null || (Types.FirstOrDefault(t => t.DowntimeTypeID == TypeID && (t.DowntimeKind == "Внеплановый" || t.DowntimeKind == "Недоступность")) != null && Comment?.Length > 0))
                    && (TypeDetailsFiltered?.Count() == 0 || (TypeDetailsFiltered?.Count() > 0 && TypeDetailID != null))
                    && (EquipmentNodeDetailsFiltered?.Count() == 0 || (EquipmentNodeDetailsFiltered?.Count() > 0 && EquipmentNodeDetailID != null));
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
