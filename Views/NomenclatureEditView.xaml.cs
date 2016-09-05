using System;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для NomenclatureEditView.xaml
    /// </summary>
    public partial class NomenclatureEditView
    {
        public NomenclatureEditView(Guid nomenclatureId)
        {
            DataContext = new NomenclatureEditViewModel(nomenclatureId);
            InitializeComponent();
        }
    }
}
