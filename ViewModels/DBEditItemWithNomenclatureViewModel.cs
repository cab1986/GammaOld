using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DBEditItemWithNomenclatureViewModel : DataBaseEditViewModel
    {
        /// <summary>
        /// Initializes a new instance of the DBEditItemWithNomenclatureViewModel class.
        /// </summary>
        public DBEditItemWithNomenclatureViewModel()
        {
            ChooseNomenclatureCommand = new RelayCommand(ChooseNomenclature,CanChooseNomenclature);
        }

        private Guid _nomenclatureID;
        public Guid NomenclatureID
        {
            get { return _nomenclatureID; }
            set
            {
                _nomenclatureID = value;
                RaisePropertyChanged("NomenclatureID");
                SetNomenclatureName(_nomenclatureID);
                Characteristics = DB.GetCharacteristics(_nomenclatureID);
            }
        }
        private string _nomenclatureName;
        [Required(ErrorMessage="Необходимо выбрать номенклатуру")]
        public string NomenclatureName
        {
            get { return _nomenclatureName; }
            set
            {
                _nomenclatureName = value;
                RaisePropertyChanged("NomenclatureName");
            }
        }

        public RelayCommand ChooseNomenclatureCommand { get; private set; }
        private void ChooseNomenclature()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, NomenclatureChanged);
            MessageManager.OpenNomenclature();
        }

        protected virtual void NomenclatureChanged(Nomenclature1CMessage msg)
        {
            Messenger.Default.Unregister<Nomenclature1CMessage>(this);
            NomenclatureID = msg.Nomenclature1CID;
        }

        private void SetNomenclatureName(Guid nomenclatureID)
        {
            NomenclatureName = (from nom in DB.GammaBase.C1CNomenclature
                                where nom.C1CNomenclatureID == nomenclatureID
                                select nom.Name).FirstOrDefault();
        }
        private ObservableCollection<Characteristic> _characteristics;
        public ObservableCollection<Characteristic> Characteristics
        {
            get
            {
                return _characteristics;
            }
            set
            {
                _characteristics = value;
                RaisePropertyChanged("Characteristics");
            }
        }
        protected virtual bool CanChooseNomenclature()
        {
            return true;
        }
    }
}