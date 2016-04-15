using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Common;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;

namespace Gamma.ViewModels
{
    class PlaceGroupsNomenclatureViewModel : SaveImplementedViewModel
    {
        public PlaceGroupsNomenclatureViewModel()
        {
            PlaceGroups = (from pg in DB.GammaBase.PlaceGroups select pg).ToList();
            _placeGroupID = PlaceGroups[0].PlaceGroupID;
            GetPlaceGroupNomenclature(PlaceGroupID);
            MoveFromPlaceGroupNomenclatureComand = new DelegateCommand(MoveFromPlaceGroupNomenclature);
            MoveToPlaceGroupNomenclatureCommand = new DelegateCommand(MoveToPlaceGroupNomenclature);
            SelectedNomenclatureFolders = new ObservableCollection<Nomenclature1CFolder>();
            SelectedPlaceGroupNomenclature = new ObservableCollection<Nomenclature1CFolder>();
        }
        private void MoveToPlaceGroupNomenclature()
        {
            var nomenclatureTree = new ObservableCollection<Nomenclature1CFolder>();
            foreach (var folder in SelectedNomenclatureFolders)
            {
                if (!nomenclatureTree.Contains(folder))
                    nomenclatureTree.Add(folder);
                foreach (var childFolder in GetChildFolders(folder.FolderID, true))
                {
                    if (!nomenclatureTree.Contains(childFolder))
                        nomenclatureTree.Add(childFolder);
                }
            }
            foreach (var folder in nomenclatureTree)
            {
                PlaceGroupNomenclature.Add(folder);
                NomenclatureFolders.Remove(folder);
            }
        }
        private void MoveFromPlaceGroupNomenclature()
        {
            var nomenclatureTree = new ObservableCollection<Nomenclature1CFolder>();
            foreach (var folder in SelectedPlaceGroupNomenclature)
            {
                if (!nomenclatureTree.Contains(folder))
                    nomenclatureTree.Add(folder);
                foreach (var childFolder in GetChildFolders(folder.FolderID, false))
                {
                    if (!nomenclatureTree.Contains(childFolder))
                        nomenclatureTree.Add(childFolder);
                }
            }
            foreach (var folder in nomenclatureTree)
            {
                PlaceGroupNomenclature.Remove(folder);
                NomenclatureFolders.Add(folder);
            }
        }
        private ObservableCollection<Nomenclature1CFolder> GetChildFolders(Guid folderid, bool toPlaceGroup)
        {
            var childFolders = new ObservableCollection<Nomenclature1CFolder>();
            if (toPlaceGroup)
                childFolders = new ObservableCollection<Nomenclature1CFolder>
                    (NomenclatureFolders.Where(p => p.ParentFolderID == folderid).Select(p => p));
            else
                childFolders = new ObservableCollection<Nomenclature1CFolder>
                    (PlaceGroupNomenclature.Where(p => p.ParentFolderID == folderid).Select(p => p));
            var tempCollection = new ObservableCollection<Nomenclature1CFolder>();
            foreach (var folder in childFolders)
            {
                foreach (var tempFolder in GetChildFolders(folder.FolderID, toPlaceGroup))
                {
                    tempCollection.Add(tempFolder);
                }
            }
            foreach (var folder in tempCollection)
            {
                childFolders.Add(folder);
            }
            return childFolders;
        }
        
        public List<Models.PlaceGroups> PlaceGroups { get; set; }
        private Int16 _placeGroupID;
        public Int16 PlaceGroupID
        {
            get
            {
                return _placeGroupID;
            }
            set
            {
                if (_placeGroupID == value) return;
                if (PlaceGroupNomenclature != null) SaveToModel();
            	_placeGroupID = value;
                RaisePropertyChanged("PlaceGroupID");
                GetPlaceGroupNomenclature(value);
            }
        }

        private void GetPlaceGroupNomenclature(Int16 placeGroupID)
        {
            var placeGroup = DB.GammaBase.PlaceGroups.Include("C1CNomenclature").Where(pg => pg.PlaceGroupID == placeGroupID).Select(pg => pg).FirstOrDefault();
            PlaceGroupNomenclature = new ObservableCollection<Nomenclature1CFolder>
                                        (from pgn in placeGroup.C1CNomenclature
                                      select new Nomenclature1CFolder
                                      {
                                          FolderID = pgn.C1CNomenclatureID,
                                          ParentFolderID = pgn.C1CParentID,
                                          Name = pgn.Name
                                      });
            NomenclatureFolders = new ObservableCollection<Nomenclature1CFolder>(from n in DB.GammaBase.C1CNomenclature
                                                                                  where n.IsFolder &&
                                                                                  !(from pg in n.PlaceGroups select pg.PlaceGroupID).Contains(placeGroupID)
                                                                                  select new Nomenclature1CFolder
                                                                                  {
                                                                                      FolderID = n.C1CNomenclatureID,
                                                                                      ParentFolderID = n.C1CParentID,
                                                                                      Name = n.Name
                                                                                  });       
        }
        private ObservableCollection<Nomenclature1CFolder> _nomenclatureFolders;
        private ObservableCollection<Nomenclature1CFolder> _placeGroupNomenclature;
        public ObservableCollection<Nomenclature1CFolder> NomenclatureFolders
        {
            get
            {
                return _nomenclatureFolders;
            }
            set
            {
            	_nomenclatureFolders = value;
                RaisePropertyChanged("NomenclatureFolders");
            }
        }
        public ObservableCollection<Nomenclature1CFolder> PlaceGroupNomenclature
        {
            get
            {
                return _placeGroupNomenclature;
            }
            set
            {
            	_placeGroupNomenclature = value;
                RaisePropertyChanged("PlaceGroupNomenclature");
            }
        }
        public DelegateCommand MoveToPlaceGroupNomenclatureCommand { get; private set; }
        public DelegateCommand MoveFromPlaceGroupNomenclatureComand { get; private set; }
        private ObservableCollection<Nomenclature1CFolder> _selectedPlaceGroupNomenclature;

        public ObservableCollection<Nomenclature1CFolder> SelectedPlaceGroupNomenclature
        {
            get
            {
                return _selectedPlaceGroupNomenclature;
            }
            set
            {
                _selectedPlaceGroupNomenclature = value;
                RaisePropertyChanged("SelectedPlaceGroupNomenclature");
            }
        }
        private ObservableCollection<Nomenclature1CFolder> _selectedNomenclatureFolders;
        public ObservableCollection<Nomenclature1CFolder> SelectedNomenclatureFolders
        {
            get
            {
                return _selectedNomenclatureFolders;
            }
            set
            {
                _selectedNomenclatureFolders = value;
                RaisePropertyChanged("SelectedNomenclatureFolders");
            }
        }
        public override void SaveToModel()
        {
            base.SaveToModel();
            var placeGroup = DB.GammaBase.PlaceGroups.Where(p => p.PlaceGroupID == PlaceGroupID).Select(p => p).FirstOrDefault();
            placeGroup.C1CNomenclature.Clear();
            var nomenclatureids = PlaceGroupNomenclature.Select(p => p.FolderID).ToList();
            var nomenclatureTree = DB.GammaBase.C1CNomenclature.Where(n1C => nomenclatureids.Contains(n1C.C1CNomenclatureID)).Select(n => n);
            foreach (var nomenclature in nomenclatureTree)
            {
                placeGroup.C1CNomenclature.Add(nomenclature);
            }
            DB.GammaBase.SaveChanges();
        }

    }
}
