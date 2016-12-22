// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections;
using System.IO.Ports;

namespace Gamma
{
    public static class Scales
    {
        static Scales()
        {
            var appSettings = GammaSettings.Get();
            try
            {
                _comPort = new SerialPort(appSettings.ScalesComPort.ComPortNumber)
                {
                    BaudRate = appSettings.ScalesComPort.BaudRate,
                    Parity = appSettings.ScalesComPort.Parity,
                    StopBits = appSettings.ScalesComPort.StopBits,
                    DataBits = appSettings.ScalesComPort.DataBits,
                    Handshake = appSettings.ScalesComPort.HandShake,
                    NewLine = "\r\n"
                };
            }
            catch (Exception)
            {
                IsReady = false;
                return;
            }
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
        private static bool _isready;
        public static bool IsReady
        {
            get
            {
                return _isready;
            }
            private set { _isready = value; }
        }
        private static SerialPort _comPort;
        public static double GetWeight()
        {
            DateTime start = DateTime.Now;
            TimeSpan time = new TimeSpan();
            if (!IsReady)
            {
                if (!TryToOpen()) return -1;
            }
            bool isStable = false;
            double weight = 0;
            do
            {
                int number = 0;
                var byteArray = ReadFromScales();
                var bitArray = new BitArray(new byte[] { (byte)byteArray[3] });
                if (bitArray[5]) isStable = true;
                else 
                {
                    time = DateTime.Now - start;
                    if (time.Seconds > 5)
                    {
                        return -1;
                    }
                    continue;
                }
                for (int i = 0; i < 3; i++)
                {
                    number *= 100;
                    number += (10 * ((byte)byteArray[i] >> 4));
                    number += (byte)byteArray[i] & 0xf;
                }
                int multiplier = 0;
                for (int i = 0; i < 3; i++)
                {
                    multiplier *= 2;
                    multiplier += bitArray[i] ? 1 : 0;
                }
                weight = number / Math.Pow(10,multiplier);
            } while (!isStable);
            return weight;
        }
        private static int[] ReadFromScales()
        {
            var byteArray = new int[4];
            int i = -1;
            do
            {
                var curByte = _comPort.ReadByte();
                if (i >= 0)
                {
                    byteArray[i] = curByte;
                    i++;
                }
                if (curByte == 10 || curByte == 13) i = 0;
            } while (i < 4);
            return byteArray;
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
