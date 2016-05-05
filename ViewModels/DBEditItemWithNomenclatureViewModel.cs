using DevExpress.Mvvm;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Gamma.Attributes;
using Gamma.Models;

namespace Gamma.ViewModels
{
    /// <summary>
    /// Этот класс содержит необходимые свойства для выбора номенклатуры
    /// <para>NomenclatureID: id Номенклатуры</para>
    /// <para>virtual CharacteristicID: ID характеристики. При необходимости можно переопределить</para>
    /// <para>NomenclatureName: Название номенклатуры</para>
    /// <para>ChooseNomenclatureCommand: Комманда вызывающая окно выбора номенклатуры</para>
    /// <para>Characteristics: Список характеристик выбранной номенклатуры</para>
    /// <para>CanChooseNomenclature: Для переопределения. По-умолчанию всегда true</para>
    /// <para>PlaceGroupID: Для переопределения. По-умолчанию 0</para>
    /// </summary>
    public class DbEditItemWithNomenclatureViewModel : SaveImplementedViewModel
    {
        /// <summary>
        /// Initializes a new instance of the DBEditItemWithNomenclatureViewModel class.
        /// </summary>
        protected DbEditItemWithNomenclatureViewModel(GammaEntities gammaBase = null) : base(gammaBase)
        {
            ChooseNomenclatureCommand = new DelegateCommand(ChooseNomenclature,CanChooseNomenclature);
        }
        private Guid? _nomenclatureid;
        [Required(ErrorMessage=@"Необходимо выбрать номенклатуру")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public Guid? NomenclatureID
        {
            get { return _nomenclatureid; }
            set
            {
                _nomenclatureid = value;
                RaisePropertyChanged("NomenclatureID");
                SetNomenclatureName(_nomenclatureid);
                Characteristics = DB.GetCharacteristics(_nomenclatureid);
            }
        }
        private string _nomenclatureName;
        [Required(ErrorMessage=@"Необходимо выбрать номенклатуру")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public string NomenclatureName
        {
            get { return _nomenclatureName; }
            set
            {
                _nomenclatureName = value;
                RaisePropertyChanged("NomenclatureName");
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DelegateCommand ChooseNomenclatureCommand { get; private set; }
        private void ChooseNomenclature()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, NomenclatureChanged);
            MessageManager.OpenNomenclature(PlaceGroupID);
        }

        protected virtual void NomenclatureChanged(Nomenclature1CMessage msg)
        {
            Messenger.Default.Unregister<Nomenclature1CMessage>(this);
            NomenclatureID = msg.Nomenclature1CID;
        }

        private void SetNomenclatureName(Guid? nomenclatureid)
        {
            if (nomenclatureid == null) return;
            NomenclatureName = (from nom in GammaBase.C1CNomenclature
                                where nom.C1CNomenclatureID == nomenclatureid
                                select nom.Name).FirstOrDefault();
        }
        private ObservableCollection<Characteristic> _characteristics;
        public virtual ObservableCollection<Characteristic> Characteristics
        {
            get
            {
                return _characteristics;
            }
            set
            {
                _characteristics = value;
                if (Characteristics.Count == 1) CharacteristicID = Characteristics[0].CharacteristicID;
                else CharacteristicID = null;
                RaisePropertyChanged("Characteristics");
            }
        }
        protected virtual bool CanChooseNomenclature()
        {
            return true;
        }

        private Guid? _characteristicID;
//        [Required(ErrorMessage = @"Необходимо выбрать характеристику")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public virtual Guid? CharacteristicID
        {
            get { return _characteristicID; }
            set
            {
                _characteristicID = value;
                RaisePropertiesChanged("CharacteristicID");
            }
        }

        /// <summary>
        /// Группа переделов для фильтрования номенклатуры
        /// </summary>
        protected int PlaceGroupID { get; set; }
    }
}