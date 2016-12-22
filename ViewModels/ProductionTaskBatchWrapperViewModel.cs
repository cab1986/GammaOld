// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using DevExpress.Mvvm;

namespace Gamma.ViewModels
{
    /// <summary>
    /// Класс обертка для viewmodel задания
    /// </summary>
    public class ProductionTaskBatchWrapperViewModel: RootViewModel
    {
        public ProductionTaskBatchWrapperViewModel(OpenProductionTaskBatchMessage msg)
        {
            ProductionTaskBatchViewModel = new ProductionTaskBatchViewModel(msg);
            ActivatedCommand = ProductionTaskBatchViewModel.ActivatedCommand;
            DeactivatedCommand = ProductionTaskBatchViewModel.DeactivatedCommand;
            Title = ProductionTaskBatchViewModel.Title;
            SaveAndCloseCommand = new DelegateCommand(() =>
            {
                ProductionTaskBatchViewModel.SaveToModel();
                CloseWindow();
            }, () => ProductionTaskBatchViewModel.CanSaveExecute());
        }

        public ProductionTaskBatchViewModel ProductionTaskBatchViewModel { get; set; }
        public DelegateCommand ActivatedCommand { get; set; }
        public DelegateCommand DeactivatedCommand { get; set; }
        public string Title { get; set; }
        public DelegateCommand SaveAndCloseCommand { get; set; }
    }
}
