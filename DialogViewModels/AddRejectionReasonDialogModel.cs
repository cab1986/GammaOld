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
    public partial class AddRejectionReasonDialogModel : ValidationViewModelBase
    {
        public AddRejectionReasonDialogModel()
        {
            GammaBase = DB.GammaDb;
            RejectionReasons = (from p in GammaBase.C1CRejectionReasons
                                       where ((!p.IsMarked ?? true) && (!p.IsFolder ?? true))
                                   select new
                                   RejectionReason
                                   {
                                       Description = p.Description,
                                       RejectionReasonID = p.C1CRejectionReasonID,
                                       ParentID = p.ParentID
                                   }
                      ).ToList();
            /*SecondRejectionReasons = (from p in GammaBase.C1CRejectionReasons
                            where ((!p.IsMarked ?? true) && (!p.IsFolder ?? true))
                            select new
                           RejectionReason
                           {
                                Description = p.Description,
                                RejectionReasonID = p.C1CRejectionReasonID,
                                       ParentID = p.ParentID
                            }
                      ).ToList();
            SecondRejectionReasonsFiltered = new List<RejectionReason>(RejectionReasons);         */
        }

        public AddRejectionReasonDialogModel(Guid? rejectionReasonID, Guid? secondRejectionReasonID, string comment = null):this()
        {
            if (rejectionReasonID != null) this.RejectionReasonID = (Guid)rejectionReasonID;
            if (secondRejectionReasonID != null) SecondRejectionReasonID = (Guid)secondRejectionReasonID;
            if (comment != null) Comment = comment;
        }

        public List<RejectionReason> RejectionReasons { get; private set; }
        //public List<RejectionReason> SecondRejectionReasons { get; private set; }
        //public List<RejectionReason> SecondRejectionReasonsFiltered { get; private set; }
        public override bool IsValid => base.IsValid && RejectionReasonID != Guid.Empty;
        
        private Guid _rejectionReasonID { get; set; }
        public Guid RejectionReasonID
        {
            get { return _rejectionReasonID; }
            set
            {
                _rejectionReasonID = value;
                RejectionReasonName = RejectionReasons.FirstOrDefault(r => r.RejectionReasonID == _rejectionReasonID).Description;
                //SecondRejectionReasonsFiltered = new List<RejectionReason>(SecondRejectionReasons.FindAll(t => t.ParentID == value));
                //RaisePropertyChanged("TypeDetailsFiltered");
            }
        }
        private Guid? _secondRejectionReasonID { get; set; }
        public Guid? SecondRejectionReasonID
        {
            get { return _secondRejectionReasonID; }
            set
            {
                _secondRejectionReasonID = value;
                SecondRejectionReasonName = RejectionReasons.FirstOrDefault(r => r.RejectionReasonID == _secondRejectionReasonID).Description;
                //SecondRejectionReasonsFiltered = new List<RejectionReason>(SecondRejectionReasons.FindAll(t => t.ParentID == value));
                //RaisePropertyChanged("TypeDetailsFiltered");
            }
        }

        public string Comment { get; set; }
        public string RejectionReasonName { get; set; }
        public string SecondRejectionReasonName { get; set; }
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
