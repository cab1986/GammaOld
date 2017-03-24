// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Models;
using System.Data.Entity;
using Gamma.Entities;

namespace Gamma.ViewModels
{
    public class MaterialTypesNomenclatureViewModel: SaveImplementedViewModel
    {
        public MaterialTypesNomenclatureViewModel()
        {        
            MaterialTypes = GammaBase.MaterialTypes.ToList();
            _materialTypeId = MaterialTypes[0].MaterialTypeID;
            GetMaterialTypeNomenclature(MaterialTypeId);
            MoveFromMaterialTypeNomenclatureComand = new DelegateCommand(MoveFromMaterialTypeNomenclature);
            MoveToMaterialTypeNomenclatureCommand = new DelegateCommand(MoveToMaterialTypeNomenclature);
            SelectedNomenclatureFolders = new ObservableCollection<Nomenclature1CFolder>();
            SelectedMaterialTypeNomenclature = new ObservableCollection<Nomenclature1CFolder>();
        }
        
        private void MoveToMaterialTypeNomenclature()
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
                MaterialTypeNomenclature.Add(folder);
                NomenclatureFolders.Remove(folder);
            }
        }
        private void MoveFromMaterialTypeNomenclature()
        {
            var nomenclatureTree = new ObservableCollection<Nomenclature1CFolder>();
            foreach (var folder in SelectedMaterialTypeNomenclature)
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
                MaterialTypeNomenclature.Remove(folder);
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
                    (MaterialTypeNomenclature.Where(p => p.ParentFolderID == folderid).Select(p => p));
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

        public List<Entities.MaterialTypes> MaterialTypes { get; set; }
        private int _materialTypeId;
        public int MaterialTypeId
        {
            get
            {
                return _materialTypeId;
            }
            set
            {
                if (_materialTypeId == value) return;
                if (MaterialTypeNomenclature != null) SaveToModel();
                _materialTypeId = value;
                RaisePropertyChanged("MaterialTypeId");
                GetMaterialTypeNomenclature(value);
            }
        }

        private void GetMaterialTypeNomenclature(int materialTypeId)
        {
            var materialType = GammaBase.MaterialTypes.Include("C1CNomenclature").Where(mt => mt.MaterialTypeID == materialTypeId).Select(pg => pg).First();
            MaterialTypeNomenclature = new ObservableCollection<Nomenclature1CFolder>
                                        (from pgn in materialType.C1CNomenclature
                                         select new Nomenclature1CFolder
                                         {
                                             FolderID = pgn.C1CNomenclatureID,
                                             ParentFolderID = pgn.C1CParentID,
                                             Name = pgn.Name
                                         });
            NomenclatureFolders = new ObservableCollection<Nomenclature1CFolder>(from n in GammaBase.C1CNomenclature
                                                                                 where n.IsFolder &&
                                                                                 !(from mt in n.MaterialTypes select mt.MaterialTypeID).Contains(materialTypeId)
                                                                                 select new Nomenclature1CFolder
                                                                                 {
                                                                                     FolderID = n.C1CNomenclatureID,
                                                                                     ParentFolderID = n.C1CParentID,
                                                                                     Name = n.Name
                                                                                 });
        }
        private ObservableCollection<Nomenclature1CFolder> _nomenclatureFolders;
        private ObservableCollection<Nomenclature1CFolder> _materialTypeNomenclature;
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
        public ObservableCollection<Nomenclature1CFolder> MaterialTypeNomenclature
        {
            get
            {
                return _materialTypeNomenclature;
            }
            set
            {
                _materialTypeNomenclature = value;
                RaisePropertyChanged("MaterialTypeNomenclature");
            }
        }
        public DelegateCommand MoveToMaterialTypeNomenclatureCommand { get; private set; }
        public DelegateCommand MoveFromMaterialTypeNomenclatureComand { get; private set; }
        private ObservableCollection<Nomenclature1CFolder> _selectedMaterialTypeNomenclature;

        public ObservableCollection<Nomenclature1CFolder> SelectedMaterialTypeNomenclature
        {
            get
            {
                return _selectedMaterialTypeNomenclature;
            }
            set
            {
                _selectedMaterialTypeNomenclature = value;
                RaisePropertyChanged("SelectedMaterialTypeNomenclature");
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

        public override bool SaveToModel()
        {
            var materialType = GammaBase.MaterialTypes.Include(mt => mt.C1CNomenclature).First(p => p.MaterialTypeID == MaterialTypeId);
            if (materialType.C1CNomenclature == null) materialType.C1CNomenclature = new List<C1CNomenclature>();
            else
                materialType.C1CNomenclature.Clear();
            var nomenclatureIds = MaterialTypeNomenclature.Select(p => p.FolderID).ToList();
            var nomenclatureTree = GammaBase.C1CNomenclature.Where(n1C => nomenclatureIds.Contains(n1C.C1CNomenclatureID));
            foreach (var nomenclature in nomenclatureTree)
            {
                materialType.C1CNomenclature.Add(nomenclature);
            }
            GammaBase.SaveChanges();
            return true;
        }
    }
}
