using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using DevExpress.Mvvm;

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
                if (_characteristicID != null)
                    BaseFormat =
                        DB.GammaDb.vCharacteristicSGBProperties.FirstOrDefault(
                            p => p.C1CCharacteristicID == _characteristicID)?.FormatNumeric ?? 0;
                else BaseFormat = 0;
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
