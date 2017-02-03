// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Windows;
using Gamma.Common;
using Gamma.Entities;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ComPortSettingsViewModel : SaveImplementedViewModel
    {
        /// <summary>
        /// Initializes a new instance of the ComPortSettingsViewModel class.
        /// </summary>
        public ComPortSettingsViewModel()
        {
            ComPortsList = new List<string>(SerialPort.GetPortNames());
            BaudRatesList = new List<int>(new[] { 75, 150, 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400 });
            DataBitsList = new List<int>(new[] { 5, 6, 7, 8 });
            StopBitsList = new StopBits().ToDictionary();
            ParityList = new Parity().ToDictionary();
            HandShakeList = new Handshake().ToDictionary();
            var settings = GammaSettings.Get();
            ScannerComPortNumber = settings.ScannerComPort.ComPortNumber;
            ScannerBaudRate = settings.ScannerComPort.BaudRate;
            ScannerDataBits = settings.ScannerComPort.DataBits;
            ScannerStopBits = (byte)settings.ScannerComPort.StopBits;
            ScannerParity = (byte)settings.ScannerComPort.Parity;
            ScannerHandShake = (byte)settings.ScannerComPort.HandShake;
            ScalesComPortNumber = settings.ScalesComPort.ComPortNumber;
            ScalesBaudRate = settings.ScalesComPort.BaudRate;
            ScalesDataBits = settings.ScalesComPort.DataBits;
            ScalesStopBits = (byte)settings.ScalesComPort.StopBits;
            ScalesParity = (byte)settings.ScalesComPort.Parity;
            ScalesHandShake = (byte)settings.ScalesComPort.HandShake;
            GetWeightCommand = new DelegateCommand(GetWeight);
 
        }
        public List<string> ComPortsList { get; set; }
        public List<int> BaudRatesList { get; set; }
        public List<int> DataBitsList { get; set; }
        public Dictionary<byte,string> StopBitsList { get; set; }
        public Dictionary<byte, string> ParityList { get; set; }
        public Dictionary<byte, string> HandShakeList { get; set; }
        public String ScannerComPortNumber { get; set; }
        public int ScannerBaudRate { get; set; }
        public int ScannerDataBits { get; set; }
        public byte ScannerStopBits { get; set; }
        public byte ScannerParity { get; set; }
        public byte ScannerHandShake { get; set; }
        public String ScalesComPortNumber { get; set; }
        public int ScalesBaudRate { get; set; }
        public int ScalesDataBits { get; set; }
        public byte ScalesStopBits { get; set; }
        public byte ScalesParity { get; set; }
        public byte ScalesHandShake { get; set; }
        private string _weight;
        public string Weight
        {
            get
            {
                return _weight;
            }
            set
            {
            	_weight = value;
                RaisePropertyChanged("Weight");
            }
        }
        public DelegateCommand GetWeightCommand { get; private set; }
        private void GetWeight()
        {
            if (Scales.IsReady)
            {
                Weight = Scales.Weight.ToString(CultureInfo.InvariantCulture);
                //Weight = weight?.ToString(CultureInfo.InvariantCulture) ?? "Ошибка";
            }
            else
            {
                MessageBox.Show("Не удалось открыть com-port. Если параметры порта верны, то сохраните изменения и перезапустите программу");
            }
        }

        public override bool SaveToModel(GammaEntities gammaBase = null)
        {
            var settings = GammaSettings.Get();
            settings.ScannerComPort.ComPortNumber = ScannerComPortNumber;
            settings.ScannerComPort.BaudRate = ScannerBaudRate;
            settings.ScannerComPort.DataBits = ScannerDataBits;
            settings.ScannerComPort.Parity = (Parity)ScannerParity;
            settings.ScannerComPort.StopBits = (StopBits)ScannerStopBits;
            settings.ScannerComPort.HandShake = (Handshake)ScannerHandShake;
            settings.ScalesComPort.ComPortNumber = ScalesComPortNumber;
            settings.ScalesComPort.BaudRate = ScalesBaudRate;
            settings.ScalesComPort.DataBits = ScalesDataBits;
            settings.ScalesComPort.Parity = (Parity)ScalesParity;
            settings.ScalesComPort.StopBits = (StopBits)ScalesStopBits;
            settings.ScalesComPort.HandShake = (Handshake)ScalesHandShake;
            return true;
        }
        
    }
}