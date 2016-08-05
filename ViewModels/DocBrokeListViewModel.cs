using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class DocBrokeListViewModel: RootViewModel
    {
        public DocBrokeListViewModel(GammaEntities gammaBase = null): base(gammaBase)
        {
            FindCommand = new DelegateCommand(Find);
            CreateNewDocBrokeCommand = new DelegateCommand(() => OpenDocBroke());
            OpenDocBrokeCommand = new DelegateCommand(() => OpenDocBroke(SelectedDocBroke.DocId), () => SelectedDocBroke != null);
            PlacesList = new List<Place>(GammaBase.Places.Where(p => p.PlaceGuid != null).Select(p => new Place
            {
                PlaceID = p.PlaceID,
                PlaceGuid = p.PlaceGuid,
                PlaceName = p.Name
            }));
            Find();
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
            if (docId == null)
                MessageManager.OpenDocBroke(SqlGuidUtil.NewSequentialid());
            else
            {
                MessageManager.OpenDocBroke((Guid)docId);
            }
        }

        private void Find()
        {
            UIServices.SetBusyState();
            using (var gammaBase = DB.GammaDb)
            {
                DocBrokeList = new List<DocBrokeListItem>(
                    from d in gammaBase.Docs.Where(d => d.DocTypeID == (int)DocTypes.DocBroke &&
                    (DateBegin == null || d.Date >= DateBegin) &&
                    (DateEnd == null || d.Date <= DateEnd) &&
                    (PlaceDiscoverId == null || d.DocBroke.PlaceDiscoverID == PlaceDiscoverId) &&
                    (PlaceStoreId == null || d.DocBroke.PlaceStoreID == PlaceStoreId)).OrderByDescending(d => d.Date).Take(500)
                    join pd in gammaBase.Places on d.DocBroke.PlaceDiscoverID equals pd.PlaceGuid
                    into ds
                    from x in ds.DefaultIfEmpty()
                    join ps in gammaBase.Places on d.DocBroke.PlaceStoreID equals ps.PlaceGuid into dps
                    from db in dps.DefaultIfEmpty()
                    select new DocBrokeListItem
                    {
                        Number = d.Number,
                        DocId = d.DocID,
                        Date = d.Date,
                        PlaceStore = x.Name,
                        PlaceDiscover = db.Name
                    });
            }
                
        }
    }

    public class DocBrokeListItem{
        public Guid DocId { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string PlaceDiscover { get; set; }
        public string PlaceStore { get; set; }
    }

}
