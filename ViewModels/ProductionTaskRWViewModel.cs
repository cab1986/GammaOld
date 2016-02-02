using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using Gamma.Models;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTaskRWViewModel : DBEditItemWithNomenclatureViewModel
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskRWViewModel class.
        /// </summary>
        public ProductionTaskRWViewModel()
        {
            Initialize();
        }
        public ProductionTaskRWViewModel(Guid productionTaskID)
        {
            Initialize();
            var productionTask = DB.GammaBase.ProductionTasks.Include("ProductionTaskRWCutting").Where(pt => pt.ProductionTaskID == productionTaskID).FirstOrDefault();
            NomenclatureID = productionTask.C1CNomenclatureID;
            SetCharacteristicProperties();
            var cutting = productionTask.ProductionTaskRWCutting.First();
            var charprops = CharacteristicProperties.Where(c => c.CharacteristicID == cutting.C1CCharacteristicID).FirstOrDefault();
            CoreDiameter = charprops.CoreDiameter;
            LayerNumber = charprops.LayerNumber;
            Color = charprops.Color;
            Diameter = charprops.Diameter;
            Destination = charprops.Destination;
            int i = 0;
            foreach (var rwcutting in productionTask.ProductionTaskRWCutting)
            {
                CuttingFormats[0].Format[i] = DB.GammaBase.GetCharSpoolFormat(rwcutting.C1CCharacteristicID).FirstOrDefault().ToString();
                i++;
            }
        }
        private void Initialize()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, NomenclatureChanged);
            CuttingFormats = new ObservableCollection<Cutting>();
            CuttingFormats.Add(new Cutting());
            ClearFilterCommand = new RelayCommand(ClearFilter);
        }

        private void ClearFilter()
        {
            ClearFormats();
            CoreDiameter = null;
            LayerNumber = null;
            Color = null;
            Diameter = null;
            Destination = null;
            CuttingsEnabled = false;
            FilterCharacteristics(Filters.All);
        }
        public override void SaveToModel(Guid ItemID)
        {
            base.SaveToModel(ItemID);
            var productionTask = DB.GammaBase.ProductionTasks.Include("ProductionTaskRWCutting").Where(pt => pt.ProductionTaskID == ItemID).FirstOrDefault();
            var prodTaskCuttings = new ObservableCollection<ProductionTaskRWCutting>();
            for (int i = 0; i < CuttingFormats[0].Format.Count; i++)
            {
                var format = CuttingFormats[0].Format[i];
            	if (format == null) continue;
                var characteristicID = (from cp in CharacteristicProperties
                                       where cp.Color == Color && cp.CoreDiameter == CoreDiameter
                                           && cp.Destination == Destination && cp.Diameter == Diameter && cp.Format == format &&
                                           cp.LayerNumber == LayerNumber
                                       select cp.CharacteristicID).FirstOrDefault();
                if (characteristicID != null)
                {
                    prodTaskCuttings.Add(new ProductionTaskRWCutting()
                    {
                        C1CCharacteristicID = characteristicID,
                        CutIndex = (short)i,
                        ProductionTaskID = ItemID,
                        ProductionTaskRWCuttingID = SQLGuidUtil.NewSequentialId()
                    });
                }
            }
            DB.GammaBase.ProductionTaskRWCutting.RemoveRange(productionTask.ProductionTaskRWCutting);
            DB.GammaBase.ProductionTaskRWCutting.AddRange(prodTaskCuttings);
            DB.GammaBase.SaveChanges();
        }
        public string Color
        {
            get
            {
                return _color;
            }
            set
            {
                if (_color == value) return;
                _color = value;
                RaisePropertyChanged("Color");
                if (value != null) FilterCharacteristics(Filters.Color);
            }
        }

        private void FilterCharacteristics(Filters filter)
        {
            if (CharacteristicProperties == null) return;
            var filteredCharacteristics = CharacteristicProperties.Where(c =>
                    (Color == null || c.Color == Color) &&
                    (LayerNumber == null || c.LayerNumber == LayerNumber) &&
                    (CoreDiameter == null || c.CoreDiameter == CoreDiameter) &&
                    (Destination == null || c.Destination == Destination) &&
                    (Diameter == null || c.Diameter == Diameter)
                ).Distinct();
            if (filter != Filters.Color)
                Colors = new ObservableCollection<string>(filteredCharacteristics.Select(c => c.Color).Distinct());
            if (filter != Filters.LayerNumber)
                LayerNumbers = new ObservableCollection<string>(filteredCharacteristics.Select(c => c.LayerNumber).Distinct());
            if (filter != Filters.CoreDiameter)
                CoreDiameters = new ObservableCollection<string>(filteredCharacteristics.Select(c => c.CoreDiameter).Distinct());
            if (filter != Filters.Destination)
                Destinations = new ObservableCollection<string>(filteredCharacteristics.Select(c => c.Destination).Distinct());
            if (filter != Filters.Diameter)
                Diameters = new ObservableCollection<string>(filteredCharacteristics.Select(c => c.Diameter).Distinct());
            Formats = new ObservableCollection<string>(filteredCharacteristics.Select(c => c.Format).Distinct());
            var charsWithoutFormat = (from fc in filteredCharacteristics
                                      select new
                                      {
                                          Color = fc.Color,
                                          LayerNumber = fc.LayerNumber,
                                          CoreDiameter = fc.CoreDiameter,
                                          Destination = fc.Destination,
                                          Diameter = fc.Diameter
                                      }).Distinct().ToList();
            if (charsWithoutFormat.Count == 1) CuttingsEnabled = true; else CuttingsEnabled = false;
            if (Colors.Count == 1) Color = Colors[0];
            if (LayerNumbers.Count == 1) LayerNumber = LayerNumbers[0];
            if (CoreDiameters.Count == 1) CoreDiameter = CoreDiameters[0];
            if (Destinations.Count == 1) Destination = Destinations[0];
            if (Diameters.Count == 1) Diameter = Diameters[0];
        }
        public ObservableCollection<string> Colors
        {
            get
            {
                return _colors;
            }
            set
            {
                _colors = value;
                RaisePropertyChanged("Colors");
            }
        }
        public string CoreDiameter
        {
            get
            {
                return _coreDiameter;
            }
            set
            {
                if (_coreDiameter == value) return;
                _coreDiameter = value;
                RaisePropertyChanged("CoreDiameter");
                if (value != null) FilterCharacteristics(Filters.CoreDiameter);
            }
        }
        public ObservableCollection<string> CoreDiameters
        {
            get
            {
                return _coreDiameters;
            }
            set
            {
                _coreDiameters = value;
                RaisePropertyChanged("CoreDiameters");
            }
        }
        public ObservableCollection<Cutting> CuttingFormats
        {
            get
            {
                return _cuttingFormats;
            }
            set
            {
                _cuttingFormats = value;
                RaisePropertyChanged("CuttingFormats");
            }
        }
        public string Destination
        {
            get
            {
                return _destination;
            }
            set
            {
                if (_destination == value) return;
                _destination = value;
                RaisePropertyChanged("Destination");
                if (value != null) FilterCharacteristics(Filters.Destination);
            }
        }
        public ObservableCollection<string> Destinations
        {
            get
            {
                return _destinations;
            }
            set
            {
                _destinations = value;
                RaisePropertyChanged("Destinations");
            }
        }
        public ObservableCollection<string> Diameters
        {
            get
            {
                return _diameters;
            }
            set
            {
                _diameters = value;
                RaisePropertyChanged("Diameters");
            }
        }
        public string Diameter
        {
            get
            {
                return _diameter;
            }
            set
            {
                if (_diameter == value) return;
                _diameter = value;
                RaisePropertyChanged("Diameter");
                if (value != null) FilterCharacteristics(Filters.Diameter);
            }
        }
        public ObservableCollection<string> Formats
        {
            get
            {
                return _formats;
            }
            set
            {
                _formats = value;
                RaisePropertyChanged("Formats");
            }
        }
        public string LayerNumber
        {
            get
            {
                return _layerNumber;
            }
            set
            {
                if (_layerNumber == value) return;
                _layerNumber = value;
                RaisePropertyChanged("LayerNumber");
                if (value != null) FilterCharacteristics(Filters.LayerNumber);
            }
        }
        public ObservableCollection<string> LayerNumbers
        {
            get
            {
                return _layerNumbers;
            }
            set
            {
                _layerNumbers = value;
                RaisePropertyChanged("LayerNumbers");
            }
        }

        protected override void NomenclatureChanged(Nomenclature1CMessage msg)
        {
            ClearFilter();
            NomenclatureID = msg.Nomenclature1CID;
            SetCharacteristicProperties();
        }

        private void SetCharacteristicProperties()
        {
            var tempCollection = new ObservableCollection<CharacteristicProperty>();
            foreach (var characteristic in Characteristics)
            {
                var charprops = DB.GammaBase.GetCharPropsDescriptions(characteristic.CharacteristicID).FirstOrDefault();
                tempCollection.Add
                    (new CharacteristicProperty()
                    {
                        CharacteristicID = charprops.C1CCharacteristicID,
                        Format = ((int)DB.GammaBase.GetCharSpoolFormat(charprops.C1CCharacteristicID).FirstOrDefault()).ToString(),
                        Diameter = charprops.Diameter,
                        LayerNumber = charprops.LayerNumber,
                        Destination = charprops.Destination,
                        Color = charprops.Color,
                        CoreDiameter = charprops.CoreDiameter
                    });
            }
            CharacteristicProperties = tempCollection;
        }

        private void ClearFormats()
        {
            for (int i = 0; i < CuttingFormats[0].Format.Count; i++)
            {
                CuttingFormats[0].Format[i] = null;
            }
        }
        private ObservableCollection<string> _coreDiameters;
        private ObservableCollection<string> _layerNumbers;
        private ObservableCollection<string> _colors;
        private ObservableCollection<string> _diameters;
        private ObservableCollection<string> _destinations;
        private ObservableCollection<string> _formats;
        private ObservableCollection<Cutting> _cuttingFormats;
        private string _coreDiameter;
        private string _layerNumber;
        private string _color;
        private string _diameter;
        private string _destination;
        
        private ObservableCollection<CharacteristicProperty> _characteristicProperties;
        private ObservableCollection<CharacteristicProperty> CharacteristicProperties 
        {
            get
            {
                return _characteristicProperties;
            }
            set
            {
            	_characteristicProperties = value;
                FilterCharacteristics(Filters.All);
            }
        }
        private class CharacteristicProperty
        {
            public Guid? CharacteristicID { get; set; }
            public string CoreDiameter { get; set; }
            public string LayerNumber { get; set; }
            public string Diameter { get; set; }
            public string Destination { get; set; }
            public string Format { get; set; }
            public string Color { get; set; }
        }
        public class Cutting : ViewModelBase
        {
            private ObservableCollection<string> _format = new ObservableCollection<string>(new string[16]);
            public ObservableCollection<string> Format
            {
                get
                {
                    return _format;
                }
                set
                {
                	_format = value;
                    RaisePropertyChanged("Format");
                }
            }
            
        }
        public override bool IsValid
        {
            get
            {
                return base.IsValid && CuttingsAreNotEmpty();
            }
        }
        private bool CuttingsAreNotEmpty()
        {
            var result = false;
            foreach (var format in CuttingFormats[0].Format)
            {
                if (format != null)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        private bool _cuttingsEnabled;
        public bool CuttingsEnabled
        {
            get
            {
                return _cuttingsEnabled;
            }
            set
            {
            	_cuttingsEnabled = value;
                RaisePropertyChanged("CuttingsEnabled");
            }
        }
        public RelayCommand ClearFilterCommand { get; private set; }
        private enum Filters { All, Color, Diameter, CoreDiameter, LayerNumber, Destination }
    }
}