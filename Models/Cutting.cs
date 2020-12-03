// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Entities;
using System.Collections.Generic;

namespace Gamma.Models
{
    public class Cutting : ViewModelBase
    {
        public int BaseFormat { get; set; }
        private Guid? _nomenclatureID;
        public Guid? NomenclatureID
        {
            get { return _nomenclatureID; }
            set
            {
                _nomenclatureID = value;
                if (_nomenclatureID == null) return;
                using (var gammaBase = DB.GammaDb)
                {
                    var nomInfo =
                        gammaBase.C1CNomenclature.Include(n => n.C1CCharacteristics)
                            .First(n => n.C1CNomenclatureID == _nomenclatureID);
                    NomenclatureName = nomInfo.Name;
                    var characteristicIds =
                        gammaBase.v1CWorkingSpecifications.Where(
                            ws => gammaBase.Places.Where(p => p.PlaceGroupID == (int) PlaceGroup.Rw)
                                .Select(p => p.C1CPlaceID)
                                .ToList().Contains(ws.C1CPlaceID) && ws.C1CNomenclatureID == NomenclatureID)
                            .Select(ws => ws.C1CCharacteristicID)
                            .ToList();                        
                    Characteristics = new ObservableCollection<C1CCharacteristics>(
                        nomInfo.C1CCharacteristics.Where(c => characteristicIds.Contains(c.C1CCharacteristicID)).OrderBy(c => c.Name));
                }
            }
        }

        private string _nomenclatureName;

        public string NomenclatureName
        {
            get { return _nomenclatureName; }
            set
            {
                _nomenclatureName = value;
                RaisePropertyChanged("NomenclatureName");
            }
        }

        private Guid? _characteristicID;

        public Guid? CharacteristicID
        {
            get { return _characteristicID; }
            set
            {
                _characteristicID = value;
                using (var gammaBase = DB.GammaDb)
                {
                    if (_characteristicID != null)
                        BaseFormat =
                            gammaBase.vCharacteristicSGBProperties.FirstOrDefault(
                                p => p.C1CCharacteristicID == _characteristicID)?.FormatNumeric ?? 0;
                    else BaseFormat = 0;
                    Specifications = new List<KeyValuePair<Guid, string>>(
                        gammaBase.v1CWorkingSpecifications
                            .Where(s => s.C1CNomenclatureID == NomenclatureID && ((value != null && s.C1CCharacteristicID == value) || (value == null && s.C1CCharacteristicID == null))
                                       && gammaBase.Places.Where(p => p.PlaceGroupID == (int)PlaceGroup.Rw)
                                .Select(p => p.C1CPlaceID)
                                .ToList().Contains(s.C1CPlaceID))
                            .OrderBy(s => s.Period)
                            .Select(s => new { s.C1CSpecificationID, s.C1CCode, s.ValidTill })
                            .Distinct()
                            .AsEnumerable()
                            .Select(s => new KeyValuePair<Guid, string>
                            (
                                s.C1CSpecificationID,
                                "Спец-я № " + s.C1CCode + " действует до " + s.ValidTill?.ToString("MM.yyyy") //+ " для передела " + gammaBase.Places.FirstOrDefault(p => p.C1CPlaceID == s.C1CPlaceID).Name
                            ))).ToList();
                    if (SpecificationID != null && SpecificationID != Guid.Empty && Specifications.Count(s => s.Key == SpecificationID) == 0)
                        SpecificationID = null;
                    if (SpecificationID == null && Specifications.Count() == 1)
                        SpecificationID = Specifications.First().Key;
                }
                RaisePropertyChanged("CharacteristicID");
            }
        }

        private ObservableCollection<C1CCharacteristics> _characteristics;

        public ObservableCollection<C1CCharacteristics> Characteristics
        {
            get { return _characteristics; }
            set
            {
                _characteristics = value;
                RaisePropertyChanged("Characteristics");
            }
        }

        private Guid? _specificationID;

        public Guid? SpecificationID
        {
            get { return _specificationID; }
            set
            {
                _specificationID = value;
                RaisePropertyChanged("SpecificationID");
            }
        }

        private List<KeyValuePair<Guid, string>> _specifications;

        public List<KeyValuePair<Guid, string>> Specifications
        {
            get { return _specifications; }
            set
            {
                _specifications = value;
                RaisePropertyChanged("Specifications");
            }
        }

        private int _quantity = 1;

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                RaisePropertyChanged("Quantity");
            }
        }
    }
}
