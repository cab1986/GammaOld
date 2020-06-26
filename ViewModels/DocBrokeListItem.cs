// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace Gamma.ViewModels
{
    public class DocBrokeListItem{
        public Guid DocId { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string PlaceDiscover { get; set; }
        public string PlaceStore { get; set; }
        public string Comment { get; set; }
        public bool IsConfirmed { get; set; }
    }
}