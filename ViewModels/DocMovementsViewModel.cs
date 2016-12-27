// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Interfaces;
using Gamma.Models;
using System.Data.Entity;

namespace Gamma.ViewModels
{
    public class DocMovementsViewModel : RootViewModel, IItemManager
    {
        private int _intervalId;

        public DocMovementsViewModel()
        {
            Intervals = new List<string> { "Активные", "Последние 500", "Поиск" };
            IntervalId = 0;
            RefreshCommand = new DelegateCommand(Find);
            EditItemCommand = new DelegateCommand(() => MessageManager.OpenDocMovement(SelectedDocMovement.DocId), SelectedDocMovement != null);
            Find();
        }

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

        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public string Number { get; set; }

        public DelegateCommand DeleteItemCommand { get; }
        
        public DelegateCommand<object> EditItemCommand { get; private set; }

        public DelegateCommand NewItemCommand { get; }
        
        public DelegateCommand RefreshCommand { get; private set; }

        private List<MovementItem> _docMovements;

        public List<MovementItem> DocMovements
        {
            get { return _docMovements; }
            set
            {
                _docMovements = value;
                RaisePropertyChanged("DocMovements");
            }
        }

        public MovementItem SelectedDocMovement { get; set; }

        public List<string> Intervals { get; private set; }

        private void Find()
        {
            UIServices.SetBusyState();
            SelectedDocMovement = null;
            using (var gammaBase = DB.GammaDb)
            {
                switch (IntervalId)
                {
                    case 0:
                        DocMovements = gammaBase.DocMovement.Include(dm => dm.Docs).Include(dm => dm.OutPlaces).Include(dm => dm.InPlaces)
                            .Where(d => !d.Docs.IsConfirmed).OrderByDescending(d => d.Docs.Date).Take(500)
                            .Select(d => new MovementItem
                            {
                                DocId = d.DocID,
                                Number = d.Docs.Number,
                                Date = d.Docs.Date,
                                PlaceFrom = d.OutPlaces.Name,
                                PlaceTo = d.InPlaces.Name,
                                IsConfirmed = d.Docs.IsConfirmed
                            }).ToList();
                        break;
                    case 1:
                        DocMovements = gammaBase.DocMovement.Include(dm => dm.Docs).Include(dm => dm.OutPlaces).Include(dm => dm.InPlaces)
                            .Take(500)
                            .Select(d => new MovementItem
                            {
                                DocId = d.DocID,
                                Number = d.Docs.Number,
                                Date = d.Docs.Date,
                                PlaceFrom = d.OutPlaces.Name,
                                PlaceTo = d.InPlaces.Name,
                                IsConfirmed = d.Docs.IsConfirmed
                            }).ToList();
                        break;
                    case 2:
                        DocMovements = gammaBase.DocMovement.Include(dm => dm.Docs).Include(dm => dm.OutPlaces).Include(dm => dm.InPlaces)
                            .Where(d => (string.IsNullOrEmpty(Number) || Number == d.Docs.Number)
                                && (DateBegin == null || d.Docs.Date >= DateBegin)
                                && (DateEnd == null || d.Docs.Date <= DateEnd)
                            )
                            .Take(500)
                            .Select(d => new MovementItem
                            {
                                DocId = d.DocID,
                                Number = d.Docs.Number,
                                Date = d.Docs.Date,
                                PlaceFrom = d.OutPlaces.Name,
                                PlaceTo = d.InPlaces.Name,
                                IsConfirmed = d.Docs.IsConfirmed
                            }).ToList();
                        break;
                }
            }
            FillDocMovement(DocMovements);
        }

        private void FillDocMovement(List<MovementItem> docMovements)
        {
            using (var gammaBase = DB.GammaDb)
            {
                var movementIds = docMovements.Select(d => d.DocId).ToList();
                var movementGoods = gammaBase.vDocMovementGoods.Where(d => movementIds.Contains(d.DocMovementID))
                    .Select(d => new
                    {
                        d.DocMovementID,
                        d.NomenclatureName,
                        InQuantity = d.InQuantity ?? 0,
                        OutQuantity = d.OutQuantity ?? 0
                    }).ToList();
                foreach (var docMovement in docMovements)
                {
                    docMovement.NomenclatureItems = new ObservableCollection<DocNomenclatureItem>
                        (
                            movementGoods.Where(d => d.DocMovementID == docMovement.DocId)
                            .Select(d => new DocNomenclatureItem
                            {
                                NomenclatureName = d.NomenclatureName,
                                InQuantity = d.InQuantity,
                                OutQuantity = d.OutQuantity
                            })
                        );
                }
            }
        }
        
    }
}
