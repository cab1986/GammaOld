using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

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
                ComPort = new SerialPort(AppSettings.ComPort) 
                { BaudRate = AppSettings.BaudRate, Parity = AppSettings.Parity, StopBits = AppSettings.StopBits, DataBits = AppSettings.DataBits, 
                    Handshake = AppSettings.HandShake, NewLine = "\r" };
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
