using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.IO.Ports;

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
            BaudRatesList = new List<int>(new int[] { 75, 150, 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400 });
            DataBitsList = new List<int>(new int[] { 5, 6, 7, 8 });
            StopBitsList = new StopBits().ToDictionary();
            ParityList = new Parity().ToDictionary();
            HandShakeList = new Handshake().ToDictionary();
            var settings = GammaSettings.Get();
            ComPort = settings.ComPort;
            BaudRate = settings.BaudRate;
            DataBits = settings.DataBits;
            StopBits = (byte)settings.StopBits;
            Parity = (byte)settings.Parity;
            HandShake = (byte)settings.HandShake;
        }
        public List<string> ComPortsList { get; set; }
        public List<int> BaudRatesList { get; set; }
        public List<int> DataBitsList { get; set; }
        public Dictionary<byte,string> StopBitsList { get; set; }
        public Dictionary<byte, string> ParityList { get; set; }
        public Dictionary<byte, string> HandShakeList { get; set; }
        public String ComPort { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public byte StopBits { get; set; }
        public byte Parity { get; set; }
        public byte HandShake { get; set; }
        public override void SaveToModel()
        {
            base.SaveToModel();
            var settings = GammaSettings.Get();
            settings.ComPort = ComPort;
            settings.BaudRate = BaudRate;
            settings.DataBits = DataBits;
            settings.Parity = (Parity)Parity;
            settings.StopBits = (StopBits)StopBits;
            settings.HandShake = (Handshake)HandShake;
        }
    }
}