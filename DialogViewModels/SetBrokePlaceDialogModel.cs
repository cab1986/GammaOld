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
    public partial class SetBrokePlaceDialogModel : ValidationViewModelBase
    {
        public SetBrokePlaceDialogModel()
        {
            GammaBase = DB.GammaDb;
            Places = (from p in GammaBase.Places
                                       //where ((!p.IsMarked ?? true) && (!p.IsFolder ?? true))
                                   select new
                                   Place
                                   {
                                       PlaceName = p.Name,
                                       PlaceID = p.PlaceID
                                   }
                      ).ToList();

            foreach (var shiftItem in GammaBase.Shifts)
            {
                Shifts.Add(new KeyValuePair<byte, string>(shiftItem.ShiftID, shiftItem.Name));
            }

            /*Shifts = (from p in GammaBase.C1CRejectionReasons
                            where ((!p.IsMarked ?? true) && (!p.IsFolder ?? true))
                            select new
                           RejectionReason
                           {
                                Description = p.Description,
                                RejectionReasonID = p.C1CRejectionReasonID,
                                       ParentID = p.ParentID
                            }
                      ).ToList();
            ShiftsFiltered = new List<RejectionReason>(RejectionReasons);         */
        }

        public SetBrokePlaceDialogModel(int? placeID) : this()
        {
            if (placeID != null) PlaceID = (int)placeID;
            IsVisibleShift = false;
            IsVisibleComment = false;
        }

        public SetBrokePlaceDialogModel(int? placeID, byte? shiftID, string comment):this()
        {
            if (placeID != null) PlaceID = (int)placeID;
            if (shiftID != null) ShiftID = (byte)shiftID;
            if (comment != null) Comment = comment;
        }

        public List<Place> Places { get; private set; }
        public List<KeyValuePair<byte, string>> Shifts { get; private set; } = new List<KeyValuePair<byte, string>>();
        //public List<RejectionReason> Shifts { get; private set; }
        //public List<RejectionReason> ShiftsFiltered { get; private set; }
        public override bool IsValid => base.IsValid;

        public bool IsVisibleShift { get; private set; } = true;
        public bool IsVisibleComment { get; private set; } = true;


        private int _placeID { get; set; }
        public int PlaceID
        {
            get { return _placeID; }
            set
            {
                _placeID = value;
                //RejectionReasonName = RejectionReasons.FirstOrDefault(r => r.RejectionReasonID == _rejectionReasonID).Description;
                //ShiftsFiltered = new List<RejectionReason>(Shifts.FindAll(t => t.ParentID == value));
                //RaisePropertyChanged("TypeDetailsFiltered");
            }
        }
        private byte _shiftID { get; set; }
        public byte ShiftID
        {
            get { return _shiftID; }
            set
            {
                _shiftID = value;
                //ShiftName = RejectionReasons.FirstOrDefault(r => r.RejectionReasonID == _shiftID).Description;
                //ShiftsFiltered = new List<RejectionReason>(Shifts.FindAll(t => t.ParentID == value));
                //RaisePropertyChanged("TypeDetailsFiltered");
            }
        }

        public string Comment { get; set; }
        public bool IsSaveEnabled => IsValid; 
                    //&& (Types.FirstOrDefault(t => t.RejectionReasonID == TypeID && (t.RejectionReasonKind == "Внеплановый" || t.RejectionReasonKind == "Недоступность")) == null || (Types.FirstOrDefault(t => t.RejectionReasonID == TypeID && (t.RejectionReasonKind == "Внеплановый" || t.RejectionReasonKind == "Недоступность")) != null && Comment?.Length > 0))
                    //&& (TypeDetailsFiltered?.Count() == 0 || (TypeDetailsFiltered?.Count() > 0 && TypeDetailID != null))
                    //&& (EquipmentNodeDetailsFiltered?.Count() == 0 || (EquipmentNodeDetailsFiltered?.Count() > 0 && EquipmentNodeDetailID != null));
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
