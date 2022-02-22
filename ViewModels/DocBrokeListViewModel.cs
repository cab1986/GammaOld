﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Interfaces;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class DocBrokeListViewModel: RootViewModel, IItemManager
    {
        public DocBrokeListViewModel()
        {
            
            FindCommand = new DelegateCommand(Find);
            CreateNewDocBrokeCommand = new DelegateCommand(() => OpenDocBroke());
            OpenDocBrokeCommand = new DelegateCommand(() => OpenDocBroke(SelectedDocBroke.DocId), () => SelectedDocBroke != null);
            using (var gammaBase = DB.GammaDb)
            {
                PlacesList = new List<Place>(gammaBase.Places.Where(p => p.PlaceGuid != null).Select(p => new Place
                {
                    PlaceID = p.PlaceID,
                    PlaceGuid = p.PlaceGuid,
                    PlaceName = p.Name
                }));
            }
            Find();
            RefreshCommand = FindCommand;
            NewItemCommand = CreateNewDocBrokeCommand;
            EditItemCommand = OpenDocBrokeCommand;
        }

        public List<Place> PlacesList { get; set; }

        private DateTime? _dateBegin;

        public DateTime? DateBegin
        {
            get { return _dateBegin; }
            set
            {
                _dateBegin = value;
                RaisePropertyChanged("DateBegin");
            }
        }

        private DateTime? _dateEnd;

        public DateTime? DateEnd
        {
            get { return _dateEnd; }
            set
            {
                _dateEnd = value; 
                RaisePropertyChanged("DateEnd");
            }
        }

        private string _number;

        public string Number
        {
            get { return _number; }
            set
            {
                _number = value;
                RaisePropertyChanged("Number");
            }
        }

        private Guid? _placeDiscoverId;

        public Guid? PlaceDiscoverId
        {
            get { return _placeDiscoverId; }
            set
            {
                _placeDiscoverId = value;
                RaisePropertyChanged("PlaceDiscoverId");
            }
        }

        private Guid? _placeStoreId;

        public Guid? PlaceStoreId
        {
            get { return _placeStoreId; }
            set
            {
                _placeStoreId = value;
                RaisePropertyChanged("PlaceStoreId");
            }
        }

        private DocBrokeListItem _selectedDocBroke;

        public DocBrokeListItem SelectedDocBroke
        {
            get { return _selectedDocBroke; }
            set
            {
                _selectedDocBroke = value;
                RaisePropertyChanged("SelectedDocBroke");
            }
        }

        private List<DocBrokeListItem> _docBrokeList;

        public List<DocBrokeListItem> DocBrokeList
        {
            get { return _docBrokeList; }
            set
            {
                _docBrokeList = value;
                RaisePropertyChanged("DocBrokeList");
            }
        }

        public DelegateCommand OpenDocBrokeCommand { get; private set; }
        public DelegateCommand CreateNewDocBrokeCommand { get; private set; }
        public DelegateCommand FindCommand { get; private set; }

        private void OpenDocBroke(Guid? docId = null)
        {
            Messenger.Default.Register<RefreshBrokeListMessage>(this, Find);
            if (docId == null)
                MessageManager.OpenDocBroke(SqlGuidUtil.NewSequentialid());
            else
            {
                MessageManager.OpenDocBroke((Guid)docId);
            }
        }

        private void Find(RefreshBrokeListMessage msg)
        {
            Find();
            Messenger.Default.Unregister<RefreshBrokeListMessage>(this, Find);
        }

        private void Find()
        {
            UIServices.SetBusyState();
            using (var gammaBase = DB.GammaDb)
            {
                DocBrokeList = new List<DocBrokeListItem>(
                    from d in gammaBase.Docs.Where(d => d.DocTypeID == (int)DocTypes.DocBroke &&
                    (DateBegin == null || d.Date >= DateBegin) &&
                    (DateEnd == null || d.Date <= DateEnd)
                    ).OrderByDescending(d => d.Date).Take(500)
                    //&&
                                                              //(PlaceDiscoverId == null || d.DocBroke.PlaceDiscoverID == PlaceDiscoverId) &&
                                                              //(PlaceStoreId == null || d.DocBroke.PlaceStoreID == PlaceStoreId)).OrderByDescending(d => d.Date).Take(500)
                                                              //join pd in gammaBase.Places on d.DocBroke.PlaceDiscoverID equals pd.PlaceGuid
                                                              //into ds
                                                              //from x in ds.DefaultIfEmpty()
                                                              //join ps in gammaBase.Places on d.DocBroke.PlaceStoreID equals ps.PlaceGuid into dps
                                                              //from db in dps.DefaultIfEmpty()
                    select new DocBrokeListItem
                    {
                        Number = d.Number,
                        DocId = d.DocID,
                        Date = d.Date,
                        //PlaceStore = db.Name,
                        //PlaceDiscover = x.Name,
                        Comment = d.Comment,
                        IsConfirmed = d.IsConfirmed,
                        LastUploadedTo1C = d.LastUploadedTo1C
                    });
            }
                
        }

        public DelegateCommand NewItemCommand { get; }
        public DelegateCommand<object> EditItemCommand { get; }
        public DelegateCommand DeleteItemCommand { get; }
        public DelegateCommand RefreshCommand { get; }
    }
}
