using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Gamma
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
        public static Dictionary<byte,string> ToDictionary(this Enum en)
        {
            var type = en.GetType();
            var list = EnumDescriptionsToList(type);
            return list.Select((s, i) => new { s, i }).ToDictionary(t =>(byte)t.i,t => t.s);
        }
    }
    
    public class DictionaryRow
    {
        public byte Key {get; set;}
        public string Name {get; set;}
    }
}
