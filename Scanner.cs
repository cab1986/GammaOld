using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace Gamma
{
    public static class Scanner
    {        
        private static bool _isready;
        public static bool IsReady
        {
            get { 
                return _isready;
            }
            private set { _isready = value; }
        }

        private static SerialPort ComPort;
        static Scanner()
        {
            var AppSettings = GammaSettings.Get();
            try
            {
                ComPort = new SerialPort(AppSettings.ScannerComPort.ComPortNumber) 
                { BaudRate = AppSettings.ScannerComPort.BaudRate, Parity = AppSettings.ScannerComPort.Parity, 
                    StopBits = AppSettings.ScannerComPort.StopBits, DataBits = AppSettings.ScannerComPort.DataBits, 
                    Handshake = AppSettings.ScannerComPort.HandShake, NewLine = "\r" };
            }
            catch (Exception)
            {
                IsReady = false;
                return;
            }

            ComPort.DataReceived += BarcodeReceive;

            try
            {
                ComPort.Open();
            }
            catch (Exception)
            {
                IsReady = false;
                return;
            }

            IsReady = true;

        }

        private static void BarcodeReceive(object sender, SerialDataReceivedEventArgs e)
        {
            string received_data = ComPort.ReadLine();
            Messenger.Default.Send<BarcodeMessage>(new BarcodeMessage {Barcode = received_data});
        }
        public static bool TryToOpen()
        {
            try
            {
                ComPort.Open();
            }
            catch (Exception)
            {
                IsReady = false;
                return false;
            }

            IsReady = true;
            return true;
        }


    }
}
