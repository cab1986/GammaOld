// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.IO.Ports;
using System.Windows;
using DevExpress.Mvvm;

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

        private static SerialPort _comPort;
        static Scanner()
        {
            var appSettings = GammaSettings.Get();
            try
            {
                _comPort = new SerialPort(appSettings.ScannerComPort.ComPortNumber) 
                { BaudRate = appSettings.ScannerComPort.BaudRate, Parity = appSettings.ScannerComPort.Parity, 
                    StopBits = appSettings.ScannerComPort.StopBits, DataBits = appSettings.ScannerComPort.DataBits, 
                    Handshake = appSettings.ScannerComPort.HandShake, NewLine = "\r" };
            }
            catch (Exception)
            {
                IsReady = false;
                return;
            }

            _comPort.DataReceived += BarcodeReceive;

            try
            {
                _comPort.Open();
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
            var receivedData = _comPort.ReadLine();
            receivedData = receivedData.Trim(' ', '\n', '\r', '\t');
            // Посылаем данные в основной поток приложения
            Application.Current.Dispatcher.Invoke(new Action(() => Messenger.Default.Send(new BarcodeMessage { Barcode = receivedData }))) ;
        }
        public static bool TryToOpen()
        {
            try
            {
                _comPort.Open();
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
