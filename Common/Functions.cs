// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Gamma.Common
{
    static class Functions
    {
        public static List<string> EnumDescriptionsToList(Type type)
        {
            var descs = new List<string>();
            var names = Enum.GetNames(type);
            foreach (var name in names)
            {
                var field = type.GetField(name);
                var fds = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
                foreach (DescriptionAttribute fd in fds)
                {
                    descs.Add(fd.Description);
                }
            }
            return descs;
        }

        public static string GetEnumDescription(Enum value)
        {
            // Get the Description attribute value for the enum value
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static Dictionary<int, string> EnumToDictionary(this Type type)
        {
            var list = EnumDescriptionsToList(type);
            if (list.Count == 0) list = new List<string>(Enum.GetNames(type));
            return list.Select((s, i) => new { s, i }).ToDictionary(t => (int)t.i, t => t.s);
        }

        public static Dictionary<byte,string> ToDictionary(this Enum en)
        {
            var type = en.GetType();
            var list = EnumDescriptionsToList(type);
            if (list.Count == 0) list = new List<string>(Enum.GetNames(type));
            return list.Select((s, i) => new { s, i }).ToDictionary(t =>(byte)t.i,t => t.s);
        }

        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example>string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;</example>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        /// <summary>
        /// Вычесление плотности бумаги-основы в кг/м3
        /// </summary>
        /// <param name="weight">вес в кг</param>
        /// <param name="coreDiameter">диаметр гильзы, мм</param>
        /// <param name="diameter">диаметр, мм</param>
        /// <param name="format">формат, мм</param>
        /// <returns></returns>
        public static decimal? GetDensity(double weight, double coreDiameter, double diameter, double format)
        {
            var volume = Math.PI*format*(Math.Pow(diameter, 2) - Math.Pow(coreDiameter, 2))/4000000000;
            if (volume == 0) return null;
            return (decimal)(weight/volume);
        }

        public static string CurrentVersion => "Текущая версия приложения: " + GammaSettings.Version;
            
        public static bool ShowMessageError(string message, string technicalMessage, Guid? docID = null, Guid? productID = null)
        {
            MessageBox.Show(message);
            DB.AddLogMessageError(message, technicalMessage, docID, productID);
            return true;
        }

        public static MessageBoxResult ShowMessageQuestion(string message, string technicalMessage, Guid? docID = null, Guid? productID = null)
        {
            var res = MessageBox.Show(message,"Вопрос", MessageBoxButton.YesNo, MessageBoxImage.Question);
            DB.AddLogMessageQuestion(message + " => Ответ: "+res, technicalMessage, docID, productID);
            return res;
        }
    }


    
    public class DictionaryRow
    {
        public byte Key {get; set;}
        public string Name {get; set;}
    }
}
