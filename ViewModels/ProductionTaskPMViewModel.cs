using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Gamma.Models;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTaskPMViewModel : DBEditItemWithNomenclatureViewModel
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskPMViewModel class.
        /// </summary>
        /// 
        public ProductionTaskPMViewModel()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, NomenclatureChanged);
        }

        public ProductionTaskPMViewModel(Guid? ProductionTaskID) : this()
        {
        }
        [Range(1,1000000,ErrorMessage="Значение должно быть больше 0")]
        public int TaskQuantity { get; set; }
        public Guid? ProductionTaskID { get; set; }

        private Characteristic _selectedCharacteristic;
        [Required(ErrorMessage="Необходимо выбрать характеристику")]
        public Characteristic SelectedCharacteristic
        {
            get { return _selectedCharacteristic; }
            set 
            {
                _selectedCharacteristic = value;
                RaisePropertyChanged("SelectedCharacteristic");
            }
        }

        public override void SaveToModel(Guid itemID)
        {
            base.SaveToModel(itemID);
          DB.GammaBase.SaveChanges();
        }

    }
}