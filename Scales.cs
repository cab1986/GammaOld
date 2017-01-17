// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Gamma
{
    public static class Scales
    {
        static Scales()
        {
            var appSettings = GammaSettings.Get();
            try
            {
                ComPort = new SerialPort(appSettings.ScalesComPort.ComPortNumber)
                {
                    BaudRate = appSettings.ScalesComPort.BaudRate,
                    Parity = appSettings.ScalesComPort.Parity,
                    StopBits = appSettings.ScalesComPort.StopBits,
                    DataBits = appSettings.ScalesComPort.DataBits,
                    Handshake = appSettings.ScalesComPort.HandShake,
                    NewLine = "\r\n",
                    Encoding = Encoding.ASCII,
                    ReadTimeout = 5000
                };
            }
            catch (Exception)
            {
                IsReady = false;
                ComPortError = true;
                return;
            }
/*
            try
            {
                _comPort.Open();
            }
            catch (Exception)
            {
                IsReady = false;
                return;
            }
*/
            IsReady = true;
        }

        public static bool ComPortError { get; private set; }

        public static bool IsReady { get; private set; }

        private static readonly SerialPort ComPort;

        public static double? GetWeight()
        {
            DateTime start = DateTime.Now;
            if (!TryToOpen()) return null;
            bool isStable = false;
            double? weight = 0;
            do
            {
                int number = 0;
                var byteArray = ReadFromScales();
                if (byteArray == null) return null;
                var bitArray = new BitArray(new[] { byteArray[3] });
                if (bitArray[5]) isStable = true;
                else 
                {
                    var time = DateTime.Now - start;
                    if (time.TotalSeconds > 20)
                    {
                        if (ComPort.IsOpen) ComPort.Close();
                        return null;
                    }
                    continue;
                }
                for (int i = 2; i > -1; i--)
                {
                    number *= 100;
                    number += (10 * (byteArray[i] >> 4));
                    number += byteArray[i] & 0xf;
                }
                int multiplier = 0;
                for (int i = 0; i < 3; i++)
                {
                    multiplier *= 2;
                    multiplier += bitArray[i] ? 1 : 0;
                }
                weight = (number / Math.Pow(10,multiplier))*(bitArray[0] ? -1: 1);

            } while (!isStable);
            if (ComPort.IsOpen) ComPort.Close();
            return weight < 0 ? 0 : weight;
        }
        private static byte[] ReadFromScales()
        {
            var byteArray = ComPort.ReadLine();
            //byteArray = byteArray.Trim(' ', '\n', '\r', '\t');
            if (byteArray.Length < 4) return new byte[4];
            byteArray = byteArray.Substring(byteArray.Length - 4);
            return Encoding.ASCII.GetBytes(byteArray);
        }

        private static bool TryToOpen()
        {
            if (ComPort.IsOpen)
            {
                ComPort.DiscardInBuffer();
                Thread.Sleep(500);
                return true;
            }
            try
            {
                ComPort.Open();
                ComPort.DiscardInBuffer();
                Thread.Sleep(500);
                ComPortError = false;
            }
            catch (Exception)
            {
                IsReady = false;
                ComPortError = true;
                return false;
            }

            IsReady = true;
            return true;
        }
        
    }
}
