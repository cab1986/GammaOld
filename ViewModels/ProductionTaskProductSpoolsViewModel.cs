using GalaSoft.MvvmLight;
using System;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTaskProductSpoolsViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskProductSpoolsViewModel class.
        /// </summary>
        public ProductionTaskProductSpoolsViewModel(Guid ProductionTaskID)
        {

        }
        public class ProductionTaskProductSpool
        {
            public string Number { get; set; }
            public DateTime Date { get; set; }
            public Guid NomenclatureID {get; set;}
            public string NomenclatureName { get; private set; }
            public Guid CharacteristicID { get; set; }
            public string CharacteristicName { get; private set; }

        }
    }
}