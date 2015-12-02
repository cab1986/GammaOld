using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using Gamma.Models;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

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
                _color = value;
                RaisePropertyChanged("Color");
                Diameter = null;
                Destination = null;
                ClearFormats();
                if (CharacteristicProperties != null)
                {
                    Diameters = new ObservableCollection<string>
                    (CharacteristicProperties.Where(cp => cp.CoreDiameter == CoreDiameter && cp.LayerNumber == LayerNumber && cp.Color == Color).
                    Select(cp => cp.Diameter).Distinct());
                }
                else Diameters = null;
            }
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
                if (Colors != null && Colors.Count == 1) Color = Colors[0];
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
                _coreDiameter = value;
                RaisePropertyChanged("CoreDiameter");
                LayerNumber = null;
                Color = null;
                Diameter = null;
                Destination = null;
                ClearFormats();
                if (CharacteristicProperties != null)
                {
                    LayerNumbers = new ObservableCollection<string>
                        (CharacteristicProperties.Where(cp => cp.CoreDiameter == _coreDiameter).Select(cp => cp.LayerNumber).Distinct());
                    if (LayerNumbers != null && LayerNumbers.Count == 1) LayerNumber = LayerNumbers[0]; 
                }
                else LayerNumbers = null;
                
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
                if (CoreDiameters != null && _coreDiameters.Count == 1) CoreDiameter = _coreDiameters[0];
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
                _destination = value;
                RaisePropertyChanged("Destination");
                ClearFormats();
                if (CharacteristicProperties != null)
                {
                    Formats = new ObservableCollection<string>
                    (CharacteristicProperties.Where
                    (cp => cp.CoreDiameter == CoreDiameter && cp.Color == Color && cp.Destination == Destination && cp.LayerNumber == LayerNumber
                    && cp.Diameter == Diameter).Select(cp => cp.Format).Distinct());
                }
                else Formats = null;   
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
                if (Destinations != null && Destinations.Count == 1) Destination = Destinations[0];
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
                if (Diameters != null && Diameters.Count == 1) Diameter = Diameters[0];
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
                _diameter = value;
                RaisePropertyChanged("Diameter");
                Destination = null;
                ClearFormats();
                if (CharacteristicProperties != null)
                {
                    Destinations = new ObservableCollection<string>
                    (CharacteristicProperties.Where
                    (cp => cp.CoreDiameter == CoreDiameter && cp.LayerNumber == LayerNumber && cp.Color == Color &&
                     cp.Diameter == Diameter).Select(cp => cp.Destination).Distinct());
                }
                else Destinations = null;
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
                _layerNumber = value;
                RaisePropertyChanged("LayerNumber");
                Color = null;
                Diameter = null;
                Destination = null;
                ClearFormats();
                if (CharacteristicProperties != null)
                {
                    Colors = new ObservableCollection<string>
                    (CharacteristicProperties.Where(cp => cp.CoreDiameter == CoreDiameter && cp.LayerNumber == LayerNumber).
                    Select(cp => cp.Color).Distinct());
                }
                else Colors = null;  
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
                if (LayerNumbers != null && _layerNumbers.Count == 1) LayerNumber = _layerNumbers[0];
            }
        }

        protected override void NomenclatureChanged(Nomenclature1CMessage msg)
        {
            CoreDiameter = null;
            LayerNumber = null;
            Color = null;
            Diameter = null;
            Destination = null;
            ClearFormats();
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
                CoreDiameters = new ObservableCollection<string>(_characteristicProperties.Select(cp => cp.CoreDiameter).Distinct());
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
    }
    
}