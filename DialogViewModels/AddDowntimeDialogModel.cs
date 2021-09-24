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
            var curDate = DateTime.Now;
            DateBegin = curDate;//new DateTime(curDate.Year, curDate.Month, 1).AddMonths(-1);
            DateEnd = curDate;//new DateTime(curDate.Year, curDate.Month, 1).AddSeconds(-1);

            GammaBase = DB.GammaDb;
            Types = (from p in GammaBase.C1CDowntimeTypes
                                       where ((!p.C1CDeleted ?? true) && (!p.Folder ?? true))
                                   select new
                                   DowntimeType
                                   {
                                       DowntimeTypeName = p.Description,
                                       DowntimeTypeID = p.C1CDowntimeTypeID
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
                              where ((!p.C1CDeleted ?? true) && (!p.Folder ?? true))
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

        public AddDowntimeDialogModel(Guid? downtimeTypeID, Guid? downtimeTypeDetailID = null, Guid? equipmentNodeID = null, Guid? equipmentNodeDetailID = null, string comment = null):this()
        {
            if (downtimeTypeID != null) TypeID = (Guid)downtimeTypeID;
            if (downtimeTypeDetailID != null) TypeDetailID = (Guid)downtimeTypeDetailID;
            if (equipmentNodeID != null) EquipmentNodeID = (Guid)equipmentNodeID;
            if (equipmentNodeDetailID != null) EquipmentNodeDetailID = (Guid)equipmentNodeDetailID;
            if (comment != null) Comment = comment;
        }

        public List<DowntimeType> Types { get; private set; }
        public List<DowntimeType> TypeDetails { get; private set; }
        public List<DowntimeType> TypeDetailsFiltered { get; private set; }
        public List<EquipmentNode> EquipmentNodes { get; private set; }
        public List<EquipmentNode> EquipmentNodeDetails { get; private set; }
        public List<EquipmentNode> EquipmentNodeDetailsFiltered { get; private set; }
        public override bool IsValid => base.IsValid && TypeID != Guid.Empty && EquipmentNodeID != Guid.Empty;
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
