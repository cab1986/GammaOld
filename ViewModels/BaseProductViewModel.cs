using GalaSoft.MvvmLight;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class BaseProductViewModel : DBEditItemWithNomenclatureViewModel
    {
        /// <summary>
        /// Initializes a new instance of the BaseProductViewModel class.
        /// </summary>
        public BaseProductViewModel()
        {
        }

        private string _productionTaskInfo;
        public string ProductionTaskInfo
        {
            get
            {
                return _productionTaskInfo;
            }
            set
            {
                _productionTaskInfo = value;
                RaisePropertyChanged("ProductionTaskInfo");
            }
        }
        
    }
}