using DevExpress.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Gamma.Common;
using Gamma.Models;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    ///  </summary>
    public class NomenclatureViewModel : RootViewModel
    {
        private NomenclatureViewModel(GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb;
            FindNomenclatureByStringCommand = new DelegateCommand(FindNomenclatureByString);
        }
        /// <summary>
        /// Initializes a new instance of the NomenclatureViewModel class.
        /// </summary>
        /// <param name="placeGroupID">ID группы переделов</param>
        public NomenclatureViewModel(int placeGroupID, GammaEntities gammaBase = null): this(gammaBase)
        {
            FilterID = placeGroupID;
            FilterByPlaceGroup = true;
            Nomenclature1CFolders = new ReadOnlyObservableCollection<Nomenclature1CFolder>
                                    (new ObservableCollection<Nomenclature1CFolder>
                                    (from nf in GammaBase.GetNomenclatureFolders(placeGroupID)
                                    select
                                        new Nomenclature1CFolder
                                        {
                                            FolderID = nf.FolderID,
                                            Name = nf.Name,
                                            ParentFolderID = nf.ParentID
                                        })
                                    );
            ChooseSelectedNomenclature = new DelegateCommand(ChooseNomenclature);
        }

        public NomenclatureViewModel(PlaceGroups placeGroup): this((int)placeGroup) { }

        /// <summary>
        /// Инициализация новой NomenclatureViewModel
        /// </summary>
        /// <param name="materialType">Материалы какого цеха</param>
        public NomenclatureViewModel(MaterialTypes materialType, GammaEntities gammaBase = null): this(gammaBase)
        {
            FilterID = (int) materialType;
            Nomenclature1CFolders = new ReadOnlyObservableCollection<Nomenclature1CFolder>
                                    (new ObservableCollection<Nomenclature1CFolder>
                                    (from nf in GammaBase.GetMaterialNomenclatureFolders((int)materialType)
                                     select
                                         new Nomenclature1CFolder
                                         {
                                             FolderID = nf.FolderID,
                                             Name = nf.Name,
                                             ParentFolderID = nf.ParentID
                                         })
                                    );
            ChooseSelectedNomenclature = new DelegateCommand(ChooseNomenclature);
        }

        private ReadOnlyObservableCollection<Nomenclature1CFolder> _nomenclature1CFolders;
        public ReadOnlyObservableCollection<Nomenclature1CFolder> Nomenclature1CFolders 
        {
            get { return _nomenclature1CFolders; }
            set 
            { 
                _nomenclature1CFolders = value;
                RaisePropertyChanged("Nomenclature1CFolders");
            }
        }
        private ReadOnlyObservableCollection<Nomenclature1C> _nomenclature;
        public ReadOnlyObservableCollection<Nomenclature1C> Nomenclature
        {
            get { return _nomenclature; }
            set 
            { 
                _nomenclature = value;
                RaisePropertyChanged("Nomenclature");
            }
        }
        public class Nomenclature1C
        {
            public string Name { get; set; }
            public Guid Nomenclature1CID { get; set; }
        }
        
        private Nomenclature1CFolder _selectedNomenclatureFolder;

        public ObservableCollection<string> NomenclatureCharacteristics
        {
            get
            {
                return _nomenclatureCharacteristics;
            }
            set
            {
                _nomenclatureCharacteristics = value;
                RaisePropertyChanged("NomenclatureCharacteristics");
            }
        }
        /// <summary>
        /// Используется во вьюхе. Выбранная папка номенклатуры
        /// </summary>
        // ReSharper disable once UnusedMember.Global 
        public Nomenclature1CFolder SelectedNomenclatureFolder 
        {
            get { return _selectedNomenclatureFolder; }
            set
            {
                if (_selectedNomenclatureFolder != value)
                {
                    _selectedNomenclatureFolder = value;
                    if (value != null)
                        GetNomenclature(value.FolderID);
                    RaisePropertyChanged("SelectedNomenclatureFolder");
                }
            }
        }

        private Nomenclature1C _selectedNomenclature;
        public Nomenclature1C SelectedNomenclature
        {
            get { return _selectedNomenclature; }
            set 
            { 
                if (_selectedNomenclature != value)
                {
                    _selectedNomenclature = value;
                    RaisePropertyChanged("SelectedNomenclature");
                    if (value != null)
                    {
                        NomenclatureCharacteristics = new ObservableCollection<string>(
                            GammaBase.C1CCharacteristics.Where(c => c.C1CNomenclatureID == value.Nomenclature1CID).
                            Select(c => c.Name));
                    }
                    else NomenclatureCharacteristics = null;
                }
            }
        }

        public DelegateCommand FindNomenclatureByStringCommand { get; private set; }

        private int FilterID { get; set; }
        private bool FilterByPlaceGroup { get; set; }

        private void FindNomenclatureByString()
        {
            if (SearchString == string.Empty) return;
            Nomenclature = new ReadOnlyObservableCollection<Nomenclature1C>(
                new ObservableCollection<Nomenclature1C>(
                    GammaBase.FindNomenclatureByStringWithFilter(SearchString, FilterID, FilterByPlaceGroup)
                    .Select(n => new Nomenclature1C
                    {
                        Nomenclature1CID = n.C1CNomenclatureID,
                        Name = n.Name
                    })
                    ));
        }

        private string _searchString;

        public string SearchString
        {
            get { return _searchString; }
            set
            {
                _searchString = value;
                RaisePropertiesChanged("SearchString");
            }
        }

        private void GetNomenclature(Guid folderid)
        {
            Nomenclature = new ReadOnlyObservableCollection<Nomenclature1C>
            (new ObservableCollection<Nomenclature1C>(
                from nom in GammaBase.C1CNomenclature where nom.C1CParentID == folderid && !nom.IsFolder && !(bool)nom.IsArchive
                select
        new Nomenclature1C
        {
            Name = nom.Name,Nomenclature1CID = nom.C1CNomenclatureID
        }));
        }

        public DelegateCommand ChooseSelectedNomenclature { get; private set; }
        private void ChooseNomenclature()
        {
            UIServices.SetBusyState();
            var msg = new Nomenclature1CMessage {Nomenclature1CID = SelectedNomenclature.Nomenclature1CID};
            Messenger.Default.Send(msg);
            CloseWindow();
        }
        private ObservableCollection<string> _nomenclatureCharacteristics;
    }
}