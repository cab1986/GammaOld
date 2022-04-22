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
    public class DocWithdrawalMaterialsViewModel : RootViewModel
    {
        public DocWithdrawalMaterialsViewModel(BrokeProduct brokeProduct)
        {
            //WithdrawalMaterials = brokeProduct.RejectionReasons;
            using (var gammaBase = DB.GammaDb)
            {
                var IDs = WorkSession.C1CRejectionReasons.Where(r => r.IsMarked == false && r.ProductKinds.Any(p => p.ProductKindID == (int)brokeProduct.ProductKind))
                            .Select(r => r.C1CRejectionReasonID).ToList();
                WithdrawalMaterialsList = new List<WithdrawalMaterial>(WorkSession.C1CRejectionReasons
                    .Where(r => (!r.IsFolder ?? true) && (!r.IsMarked ?? true)
                    && (IDs.Contains((Guid)r.C1CRejectionReasonID) ||
                        (r.ParentID == null || (r.ParentID != null && IDs.Contains((Guid)r.ParentID)))))
                    .Select(r => new WithdrawalMaterial
                    {
                   /* RejectionReasonID = r.C1CRejectionReasonID,
                    Description = r.Description,
                    FullDescription = r.FullDescription*/
                }).OrderBy(r => r.Quantity));
            }
            if (WithdrawalMaterialsList.Count == 0)
            {
                MessageBox.Show("Не удалось получить список материалов!");
                CloseWindow();
                return;
            }
            AddWithdrawalMaterialCommand = new DelegateCommand(AddWithdrawalMaterial);
            DeleteWithdrawalMaterialCommand = new DelegateCommand(DeleteWithdrawalMaterial);
        }

        private ItemsChangeObservableCollection<WithdrawalMaterial> _withdrawalMaterials;

        public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterials
        {
            get { return _withdrawalMaterials; }
            set
            {
                _withdrawalMaterials = value;
                RaisePropertyChanged("WithdrawalMaterials");
            }
        }

        public List<WithdrawalMaterial> WithdrawalMaterialsList { get; private set; }

        public DelegateCommand AddWithdrawalMaterialCommand { get; private set; }
        public DelegateCommand DeleteWithdrawalMaterialCommand { get; private set; }

        private void AddWithdrawalMaterial()
        {
            WithdrawalMaterials.Add(new WithdrawalMaterial());
            //{
            //RejectionReasonID = RejectionReasonsList.First().RejectionReasonID,
            //Description = RejectionReasonsList.First().Description
        //});
        }

        private void DeleteWithdrawalMaterial()
        {
            if (SelectedWithdrawalMaterial == null) return;
            WithdrawalMaterials.Remove(SelectedWithdrawalMaterial);
        }

        public WithdrawalMaterial SelectedWithdrawalMaterial { get; set; }

    }
}
