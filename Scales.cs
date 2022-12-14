// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections;
using System.IO.Ports;
using System.Linq;
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
                TypeDateReadFromComPort = appSettings.ScalesComPort.TypeDateReadFromComPort;
                Prefix = appSettings.ScalesComPort.Prefix;
                Postfix = appSettings.ScalesComPort.Postfix;
            }
            catch (Exception)
            {
                IsReady = false;
//                ComPortError = true;
                return;
            }

            try
            {
                ComPort.Open();
            }
            catch (Exception)
            {
                IsReady = false;
                return;
            }

            if (TypeDateReadFromComPort == DateReadFromComPortType.Byte)
                ComPort.DataReceived += WeightReceivedFromScalesReturnedByte;
            else if (TypeDateReadFromComPort == DateReadFromComPortType.String)
                ComPort.DataReceived += WeightReceivedFromScalesReturnedString;
            else
            {
                IsReady = false;
                return;
            }
            IsReady = true;
        }

        
        private static void WeightReceivedFromScalesReturnedString(object sender, SerialDataReceivedEventArgs e)
        {
            var stringFromScales = ReadStringFromScales();
            if (stringFromScales == null) return;
            IsStable = true;
            ReadLineFromSerialPort = ReadLineFromSerialPort + "/" + Prefix + "#" + stringFromScales.IndexOf(Prefix);
            if (Prefix != null && Prefix != String.Empty
                && stringFromScales.IndexOf(Prefix) > -1)
            {
                stringFromScales = stringFromScales.Remove(stringFromScales.IndexOf(Prefix), Prefix.Length);
            }
            ReadLineFromSerialPort = ReadLineFromSerialPort + "/" + stringFromScales + "/" + Postfix + "#" + stringFromScales.IndexOf(Postfix);
            if (Postfix != null && Postfix != String.Empty
                && stringFromScales.IndexOf(Postfix) > -1)
            {
                stringFromScales = stringFromScales.Remove(stringFromScales.IndexOf(Postfix));
            }
            ReadLineFromSerialPort = ReadLineFromSerialPort + "/" + stringFromScales;
            double weight;
            Weight = Double.TryParse(stringFromScales, out weight) ? weight : 0;
        }

        private static void WeightReceivedFromScalesReturnedByte(object sender, SerialDataReceivedEventArgs e)
        {
            var number = 0;
            var byteArray = ReadByteFromScales();
            if (byteArray == null) return;
         
            var bitArray = new BitArray(new[] { byteArray[3] });
            IsStable = bitArray[5];
            // Перевод BCD в int
            for (var i = 2; i > -1; i--)
            {
                number *= 100;
                number += (10 * (byteArray[i] >> 4));
                number += byteArray[i] & 0xf;
            }
            var multiplier = 0;
            for (var i = 0; i < 3; i++)
            {
                multiplier *= 2;
                multiplier += bitArray[i] ? 1 : 0;
            }
            Weight = (number / Math.Pow(10, multiplier)) * (bitArray[0] ? -1 : 1);
        }

        public static double Weight { get; private set; }
        public static bool IsStable { get; private set; }
        public static string ReadLineFromSerialPort { get; private set; }
        private static string Prefix { get; set; }
        private static string Postfix { get; set; }
        private static DateReadFromComPortType TypeDateReadFromComPort { get; set; }

        //        public static bool ComPortError { get; private set; }

        public static bool IsReady { get; private set; }

        private static readonly SerialPort ComPort;
        /*
                private static double? GetWeight()
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
         //                       ComPortError = true;
                                IsReady = false;
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
        */

        private static string ReadStringFromScales()
        {
            try
            {
                var stringToRead = ComPort.ReadLine();
                ReadLineFromSerialPort = stringToRead;
                return stringToRead == null || stringToRead?.Length < 4 ? String.Empty : stringToRead;
                //byteArray = byteArray.Trim(' ', '\n', '\r', '\t');
                //byteArray = byteArray.Substring(byteArray.Length - 4);
            }
            catch (Exception)
            {
                IsReady = false;
                return null;
            }
        }

        private static byte[] ReadByteFromScales()
        {
            try
            {
                var bytesToRead = ComPort.BytesToRead;
                if (bytesToRead < 4) return new byte[4];
                var byteArray = new byte[bytesToRead];
                ComPort.Read(byteArray, 0, bytesToRead);
//                if (byteArray.Length < 4) return 
//                if (byteArray.Length > 11) byteArray = byteArray.Substring(byteArray.Length - 11, 11);
                var index = LastIndex(byteArray, new byte[] {13, 10}); // Поиск последнего конца строки в массиве
                ReadLineFromSerialPort = System.Text.Encoding.Default.GetString(byteArray);
                return index == null || index < 4 ? new byte[4] : byteArray.Skip((int) index - 4).Take(4).ToArray();
                //byteArray = byteArray.Trim(' ', '\n', '\r', '\t');
                //byteArray = byteArray.Substring(byteArray.Length - 4);
            }
            catch (Exception)
            {
                IsReady = false;
                return null;
            }
        }

        private static int? LastIndex(byte[] array, byte[] pattern)
        {
            if (array.Length < pattern.Length) return null;
            for (var i = array.Length; i > -1; i--)
            {
                if (array.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                    return i;
            }
            return null;
        }

/*
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
                //ComPortError = false;
            }
            catch (Exception)
            {
                IsReady = false;
                //ComPortError = true;
                return false;
            }

            IsReady = true;
            return true;
        }
     */   
    }
}
