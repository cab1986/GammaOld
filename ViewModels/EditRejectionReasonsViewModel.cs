using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class EditRejectionReasonsViewModel : RootViewModel
    {
        public EditRejectionReasonsViewModel(List<DocBrokeProductRejectionReasons> rejectionReasons, Guid docId, Guid productId)
        {
            DocId = docId;
            ProductId = productId;
            RejectionReasons = rejectionReasons;
            using (var gammaBase = DB.GammaDb)
            {
                RejectionReasonsList = new List<RejectionReason>(gammaBase.C1CRejectionReasons.Select(r => new RejectionReason
                {
                    RejectionReasonID = r.C1CRejectionReasonID,
                    Description = r.Description,
                    FullDescription = r.FullDescription
                }));
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

        private Guid DocId { get; }
        private Guid ProductId { get;  }

        private List<DocBrokeProductRejectionReasons> _rejectionReasons;

        public List<DocBrokeProductRejectionReasons> RejectionReasons
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
            RejectionReasons.Add(new DocBrokeProductRejectionReasons
            {
                ProductID = ProductId,
                DocID = DocId,
                C1CRejectionReasonID = RejectionReasonsList.First().RejectionReasonID
            });
        }

        private void DeleteRejectionReason()
        {
            if (SelectedRejectionReason == null) return;
            RejectionReasons.Remove(SelectedRejectionReason);
        }

        public DocBrokeProductRejectionReasons SelectedRejectionReason { get; set; }
    }
}
