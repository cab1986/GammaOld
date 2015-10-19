using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class NomenclatureViewModel : RootViewModel
    {
        /// <summary>
        /// Initializes a new instance of the NomenclatureViewModel class.
        /// </summary>
        public NomenclatureViewModel()
        {

            Nomenclature1CFolders = new ReadOnlyObservableCollection<Nomenclature1CFolder>
                                    (new ObservableCollection<Nomenclature1CFolder>
                                    (from nf in DB.GammaBase.C1CNomenclature where nf.IsFolder == true
                                    select
                                        new Nomenclature1CFolder
                                        {
                                            Nomenclature1CFolderID = nf.C1CNomenclatureID,
                                            Name = nf.Name,
                                            ParentFolderID = nf.C1CParentID
                                        })
                                    );
            ChooseSelectedNomenclature = new RelayCommand(ChooseNomenclature);
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
        public class Nomenclature1CFolder
        {
            public string Name {get; set;}
            public Guid Nomenclature1CFolderID {get; set; }
            public Guid? ParentFolderID { get; set; }
        }

        private Nomenclature1CFolder _selectedNomenclatureFolder;

        public Nomenclature1CFolder SelectedNomenclatureFolder
        {
            get { return _selectedNomenclatureFolder; }
            set
            {
                if (_selectedNomenclatureFolder != value)
                {
                    _selectedNomenclatureFolder = value;
                    GetNomenclature(value.Nomenclature1CFolderID);
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
                }
            }
        }

        private void GetNomenclature(Guid FolderID)
        {
            Nomenclature = new ReadOnlyObservableCollection<Nomenclature1C>
            (new ObservableCollection<Nomenclature1C>(
                from nom in DB.GammaBase.C1CNomenclature where nom.C1CParentID == FolderID && !nom.IsFolder
                select
        new Nomenclature1C
        {
            Name = nom.Name,Nomenclature1CID = nom.C1CNomenclatureID
        }));
        }

        public RelayCommand ChooseSelectedNomenclature { get; private set; }
        private void ChooseNomenclature()
        {
            var msg = new Nomenclature1CMessage {Nomenclature1CID = SelectedNomenclature.Nomenclature1CID};
            Messenger.Default.Send<Nomenclature1CMessage>(msg);
            CloseWindow();
        }
    }
}