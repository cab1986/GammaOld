﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
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
                RejectionReasonsList = new List<RejectionReason>(gammaBase.C1CRejectionReasons
                    .Where(r => (!r.IsFolder??true) && (!r.IsMarked??true) && (r.ParentID == null || (r.ParentID != null
                    && gammaBase.ProductKinds.FirstOrDefault(pk => pk.ProductKindID == (int)brokeProduct.ProductKind).C1CRejectionReasons.Select(rr => rr.C1CRejectionReasonID).Contains((Guid)r.ParentID))))
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
