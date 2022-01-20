// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class MovementItem : ViewModelBase
    {
        public Guid DocId { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public int? ShiftID { get; set; }
        public string PlaceTo { get; set; }
        public string PlaceFrom { get; set; }
        public bool IsConfirmed { get; set; }
        public string Person { get; set; }
        public string PlacePerson { get; set; }
        public DateTime? LastUploadedTo1C { get; set; }

        private ObservableCollection<DocNomenclatureItem> _nomenclatureItems;

        public ObservableCollection<DocNomenclatureItem> NomenclatureItems
        {
            get { return _nomenclatureItems; }
            set
            {
                _nomenclatureItems = value;
                RaisePropertyChanged("NomenclatureItems");
            }
        }
    }
}
