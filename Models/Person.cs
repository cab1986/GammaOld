// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace Gamma.Models
{
    public class Person
    {
        public Guid PersonId { get; set; }
        public string Name { get; set; }
        public string PlaceName { get; set; }
    }
}