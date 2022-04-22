using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Interfaces;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class DocWithdrawalsViewModel : RootViewModel, IItemManager
    {
        public DocWithdrawalsViewModel()
        {
            Intervals = new List<string> { "Последние 500", "Поиск" };
            RefreshCommand = new DelegateCommand(Find);
            EditItemCommand = new DelegateCommand(OpenDocWithdrawal, () => SelectedDocWithdrawal != null);
            Find();
            Places = WorkSession.Places.Where(
                    p => p.BranchID == WorkSession.BranchID && (p.IsProductionPlace ?? false))
                    .Select(p => new Place()
                    {
                        PlaceID = p.PlaceID,
                        PlaceGuid = p.PlaceGuid,
                        PlaceName = p.Name
                    }).ToList();
        }

        private void OpenDocWithdrawal()
        {
            if (SelectedDocWithdrawal == null) return;
            MessageManager.OpenDocWithdrawal(SelectedDocWithdrawal.DocId);
        }

        private void Find()
        {
            UIServices.SetBusyState();
            using (var gammaBase = DB.GammaDb)
            {
                switch (Intervalid)
                {
                    case 0:
                        DocWithdrawals = gammaBase.Docs.Where(
                            d => d.DocTypeID == (int)DocTypes.DocWithdrawal || d.DocTypeID == (int)DocTypes.DocUtilization
                            ).OrderByDescending(d => d.Date).Take(500).Select(d => new DocWithdrawalsItem
                            {
                                DocId = d.DocID,
                                Number = d.Number,
                                Place = d.Places.Name,
                                Date = d.Date,
                                ShiftID = d.ShiftID,
                                IsConfirmed = d.IsConfirmed
                            }).ToList();
                        break;
                    case 1:
                        DocWithdrawals = gammaBase.Docs.Where(
                            d => (d.DocTypeID == (int)DocTypes.DocWithdrawal || d.DocTypeID == (int)DocTypes.DocUtilization) &&
                                (DateBegin == null || d.Date >= DateBegin) &&
                                (DateEnd == null || d.Date <= DateEnd) &&
                                (PlaceId == null || d.PlaceID == PlaceId) &&
                                (Number == null || Number == string.Empty || d.Number.Contains(Number))
                            ).OrderByDescending(d => d.Date).Take(500).Select(d => new DocWithdrawalsItem
                            {
                                DocId = d.DocID,
                                Number = d.Number,
                                Place = d.Places.Name,
                                Date = d.Date,
                                ShiftID = d.ShiftID,
                                IsConfirmed = d.IsConfirmed
                            }).ToList();
                        break;
                    
                }
            }
        }

        public DocWithdrawalsItem SelectedDocWithdrawal { get; set; }
        public string Number { get; set; }
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public List<string> Intervals { get; set; }
        public int? PlaceId { get; set; }
        public List<Place> Places { get; set; }

        private int _intervalid;
        private List<DocWithdrawalsItem> _docWithdrawals;

        public int Intervalid
        {
            get { return _intervalid; }
            set
            {
                if (_intervalid == value) return;
                _intervalid = value < 0 ? 0 : value;
                if (_intervalid < 3) Find();
            }
        }

        public List<DocWithdrawalsItem> DocWithdrawals
        {
            get { return _docWithdrawals; }
            set
            {
                _docWithdrawals = value;
                RaisePropertyChanged("DocWithdrawals");
            }
        }

        public DelegateCommand NewItemCommand { get; }
        public DelegateCommand<object> EditItemCommand { get; }
        public DelegateCommand DeleteItemCommand { get; }
        public DelegateCommand RefreshCommand { get; }
    }
}
