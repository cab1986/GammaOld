// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class EditRejectionReasonsViewModel : RootViewModel
    {
        public EditRejectionReasonsViewModel(BrokeProduct brokeProduct)
        {
            //RejectionReasons = brokeProduct.RejectionReasons;
            using (var gammaBase = DB.GammaDb)
            {
                var IDs = WorkSession.C1CRejectionReasons.Where(r => r.IsMarked == false && r.ProductKinds.Any(p => p.ProductKindID == (int)brokeProduct.ProductKind))
                            .Select(r => r.C1CRejectionReasonID).ToList();
                RejectionReasonsList = new List<RejectionReason>(WorkSession.C1CRejectionReasons
                    .Where(r => (!r.IsFolder ?? true) && (!r.IsMarked ?? true)
                    && (IDs.Contains((Guid)r.C1CRejectionReasonID) ||
                        (r.ParentID == null || (r.ParentID != null && IDs.Contains((Guid)r.ParentID)))))
                    .Select(r => new RejectionReason
                {
                    RejectionReasonID = r.C1CRejectionReasonID,
                    Description = r.Description,
                    FullDescription = r.FullDescription
                }).OrderBy(r => r.Description));
            }
            if (RejectionReasonsList.Count == 0)
            {
                MessageBox.Show("Не удалось получить список дефектов!");
                CloseWindow();
                return;
            }
            AddRejectionReasonCommand = new DelegateCommand(AddRejectionReason);
            DeleteRejectionReasonCommand = new DelegateCommand(DeleteRejectionReason);
        }

        private ItemsChangeObservableCollection<RejectionReason> _rejectionReasons;

        public ItemsChangeObservableCollection<RejectionReason> RejectionReasons
        {
            get { return _rejectionReasons; }
            set
            {
                _rejectionReasons = value;
                RaisePropertyChanged("RejectionReasons");
            }
        }

        public List<RejectionReason> RejectionReasonsList { get; private set; }

        public DelegateCommand AddRejectionReasonCommand { get; private set; }
        public DelegateCommand DeleteRejectionReasonCommand { get; private set; }

        private void AddRejectionReason()
        {
            RejectionReasons.Add(new RejectionReason());
            //{
            //RejectionReasonID = RejectionReasonsList.First().RejectionReasonID,
            //Description = RejectionReasonsList.First().Description
        //});
        }

        private void DeleteRejectionReason()
        {
            if (SelectedRejectionReason == null) return;
            RejectionReasons.Remove(SelectedRejectionReason);
        }

        public RejectionReason SelectedRejectionReason { get; set; }

    }
}
