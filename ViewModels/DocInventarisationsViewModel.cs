using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Interfaces;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class DocInventarisationsViewModel : SaveImplementedViewModel, IItemManager
    {
        public DocInventarisationsViewModel()
        {
            Intervals = new List<string> { "Активные", "Последние 500", "Поиск" };
            IntervalId = 0;
            RefreshCommand = new DelegateCommand(Find);
            EditItemCommand = new DelegateCommand(() => MessageManager.OpenDocInventarisation(SelectedDocInventarisation.DocId), SelectedDocInventarisation != null);
            Find();
        }

        private List<DocInventarisation> _docInventarisationsList;

        public List<DocInventarisation> DocInventarisationsList
        {
            get { return _docInventarisationsList; }
            set
            {
                _docInventarisationsList = value;
                RaisePropertyChanged("DocInventarisationsList");
            }
        }

        public void Find()
        {
            UIServices.SetBusyState();
            SelectedDocInventarisation = null;
            using (var gammaBase = DB.GammaDb)
            {
                switch (IntervalId)
                {
                    case 0:
                        DocInventarisationsList = gammaBase.Docs.Include(d => d.Places)
                            .Where(d => !d.IsConfirmed && d.DocTypeID == (int)DocTypes.DocInventarisation).OrderByDescending(d => d.Date).Take(500)
                            .Select(d => new DocInventarisation
                            {
                                DocId = d.DocID,
                                Number = d.Number,
                                Date = d.Date,
                                Warehouse = d.Places.Name,
                            }).ToList();
                        break;
                    case 1:
                        DocInventarisationsList = gammaBase.Docs.Include(d => d.Places).Where(d => d.DocTypeID == (int)DocTypes.DocInventarisation)
                            .Take(500)
                            .Select(d => new DocInventarisation
                            {
                                DocId = d.DocID,
                                Number = d.Number,
                                Date = d.Date,
                                Warehouse = d.Places.Name,
                            }).ToList();
                        break;
                    case 2:
                        DocInventarisationsList = gammaBase.Docs.Include(d => d.Places)
                            .Where(d => (string.IsNullOrEmpty(Number) || Number == d.Number)
                                && (DateBegin == null || d.Date >= DateBegin)
                                && (DateEnd == null || d.Date <= DateEnd)
                            )
                            .Take(500)
                            .Select(d => new DocInventarisation
                            {
                                DocId = d.DocID,
                                Number = d.Number,
                                Date = d.Date,
                                Warehouse = d.Places.Name,
                            }).ToList();
                        break;
                }
            }
        }

        private int _intervalId;

        public int IntervalId
        {
            get { return _intervalId; }
            set
            {
                if (_intervalId == value) return;
                _intervalId = value;
                if (_intervalId < 2) Find();
            }
        }

        public DocInventarisation SelectedDocInventarisation { get; set; }

        public List<string> Intervals { get; private set; }

        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public string Number { get; set; }

        public DelegateCommand DeleteItemCommand { get; }

        public DelegateCommand<object> EditItemCommand { get; private set; }

        public DelegateCommand NewItemCommand { get; }

        public DelegateCommand RefreshCommand { get; private set; }

    }
}
