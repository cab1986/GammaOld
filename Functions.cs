using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
            if (list.Count == 0) list = new List<string>(Enum.GetNames(type));
            return list.Select((s, i) => new { s, i }).ToDictionary(t =>(byte)t.i,t => t.s);
        }
    }
    
    public class DictionaryRow
    {
        public byte Key {get; set;}
        public string Name {get; set;}
    }
}
