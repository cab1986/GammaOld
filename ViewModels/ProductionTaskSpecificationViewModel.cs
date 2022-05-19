// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using Gamma.Interfaces;
using System.Linq;
using System;
using Gamma.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Data.Entity;
using Gamma.Entities;
using System.Collections.Generic;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTaskSpecificationViewModel : SaveImplementedViewModel, ICheckedAccess
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskSpecificationViewModel class.
        /// </summary>
        public ProductionTaskSpecificationViewModel()
        {
        }

        public ProductionTaskSpecificationViewModel(Guid? specificationID, Guid? nomenclatureID, Guid? characteristicID, int? placeID, bool isReadOnly)
        {
            SetSpecifications(nomenclatureID, characteristicID, placeID);
            SpecificationID = specificationID;
            IsReadOnly = isReadOnly;
        }


        private Guid? _specificationID;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public Guid? SpecificationID
        {
            get
            {
                return _specificationID;
            }
            set
            {
                _specificationID = value;
                RaisePropertyChanged("SpecificationID");
               /* SelectedSpecification = Specifications.FirstOrDefault(s => s.Key == value);
                if (value != null && value != Guid.Empty && SelectedSpecification.Key == Guid.Empty)
                {
                    var selectedSpecification = GammaBase.C1CSpecifications
                        .FirstOrDefault(s => s.C1CSpecificationID == value);
                    if (selectedSpecification != null)
                    {
                        Specifications.Add(new KeyValuePair<Guid, string>(selectedSpecification.C1CSpecificationID, selectedSpecification.C1CCode));
                        SelectedSpecification = new KeyValuePair<Guid, string>(selectedSpecification.C1CSpecificationID, selectedSpecification.C1CCode);
                    }
                }*/
            }
        }
        private List<KeyValuePair<Guid, string>> _specifications;
        public List<KeyValuePair<Guid, string>> Specifications
        {
            get
            {
                return _specifications;
            }
            set
            {
                _specifications = value;
                RaisePropertyChanged("Specifications");
            }
        }
        /*
        private KeyValuePair<Guid, string> _selectedSpecification;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public KeyValuePair<Guid, string> SelectedSpecification
        {
            get
            {
                return _selectedSpecification;
            }
            set
            {
                _selectedSpecification = value;
                RaisePropertyChanged("SelectedSpecification");
            }
        }
        */
        public void SetSpecifications(Guid? nomenclatureID, Guid? characteristicID, int? PlaceID)
        {
            if (Specifications == null)
                Specifications = new List<KeyValuePair<Guid, string>>();

            using (var gammaBase = DB.GammaDbWithNoCheckConnection)
            {
                /*var specifications = gammaBase.v1CWorkingSpecifications
                        .Where(s => s.C1CNomenclatureID == NomenclatureID && ((CharacteristicID != null && s.C1CCharacteristicID == CharacteristicID) || (CharacteristicID == null && s.C1CCharacteristicID == null))
                                   )// && s.C1CPlaceID == gammaBase.Places.FirstOrDefault(p => p.PlaceID == PlaceID).C1CPlaceID)
                        .OrderBy(s => s.Period);
                foreach (var item in specifications)
                {
                    Specifications.Add(new KeyValuePair<Guid, string>(item.C1CSpecificationID, item.C1CCode));
                }
                */
                Specifications = new List<KeyValuePair<Guid, string>>(
                    gammaBase.v1CWorkingSpecifications
                        .Where(s => s.C1CNomenclatureID == nomenclatureID && ((characteristicID != null && s.C1CCharacteristicID == characteristicID) || (characteristicID == null && s.C1CCharacteristicID == null))
                                   && s.C1CPlaceID == gammaBase.Places.FirstOrDefault(p => p.PlaceID == PlaceID).C1CPlaceID)
                        .OrderBy(s => s.Period)
                        .Select(s => new { s.C1CSpecificationID, s.C1CCode, s.ValidTill, s.Description })
                        .AsEnumerable()
                        .Select(s => new KeyValuePair<Guid, string>
                        (
                            s.C1CSpecificationID,
                            "№ " + s.C1CCode + (s.ValidTill != null ? "(до " + s.ValidTill?.ToString("MM.yyyy") + ") " : " ") + s.Description
                        ))).ToList();
                if (SpecificationID != null && SpecificationID != Guid.Empty && Specifications.Count(s => s.Key == SpecificationID) == 0)
                    SpecificationID = null;
                if (SpecificationID == null && Specifications.Count() == 1)
                    SpecificationID = Specifications.First().Key;
                //if (SelectedSpecification.Key != Guid.Empty && Specifications.Count(s => s.Key == SelectedSpecification.Key) == 0)
                //    SelectedSpecification = new KeyValuePair<Guid, string>();
            }

        }

        public void SetIsReadOnly(bool isReadOnly)
        {
            IsReadOnly = isReadOnly;
        }

        public bool IsReadOnly { get; private set; } //=> IsConfirmed || !DB.HaveWriteAccess("ProductionTaskSGB");
    }
}