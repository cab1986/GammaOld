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
        public AddRejectionReasonDialogModel(ProductKind? kind)
        {
            GammaBase = DB.GammaDb;
            var IDs = GammaBase.C1CRejectionReasons.Where(r => r.IsMarked == false && r.ProductKinds.Any(p => kind == null || p.ProductKindID == (byte)kind)).Select(r => r.C1CRejectionReasonID).ToList();
            RejectionReasons = GammaBase.C1CRejectionReasons.Where(r => r.IsMarked == false && r.IsFolder == false && (IDs.Contains((Guid)r.C1CRejectionReasonID) || IDs.Contains((Guid)r.ParentID)))
                      .OrderBy(r => r.Description)
                      .Select(r => new RejectionReason
                      {
                          Description = r.Description,
                          RejectionReasonID = r.C1CRejectionReasonID,
                          ParentID = r.ParentID
                      }).ToList();
        }

        public AddRejectionReasonDialogModel(ProductKind kind, Guid? rejectionReasonID, Guid? secondRejectionReasonID, string comment = null):this(kind)
        {
            if (rejectionReasonID != null) this.RejectionReasonID = (Guid)rejectionReasonID;
            if (secondRejectionReasonID != null) SecondRejectionReasonID = (Guid)secondRejectionReasonID;
            if (comment != null) Comment = comment;
        }

        public List<RejectionReason> RejectionReasons { get; private set; }
        public override bool IsValid => base.IsValid && RejectionReasonID != Guid.Empty && Comment?.Length > 3;
        
        private Guid _rejectionReasonID { get; set; }
        public Guid RejectionReasonID
        {
            get { return _rejectionReasonID; }
            set
            {
                _rejectionReasonID = value;
                RejectionReasonName = RejectionReasons.FirstOrDefault(r => r.RejectionReasonID == _rejectionReasonID).Description;
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
            }
        }

        public string Comment { get; set; }
        public string RejectionReasonName { get; set; }
        public string SecondRejectionReasonName { get; set; }
        public bool IsSaveEnabled => IsValid; 
    }
}
