// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.ComponentModel.DataAnnotations;
using Gamma.ViewModels;

namespace Gamma.DialogViewModels
{
    public class SetBaleWeightDialogModel : ValidationViewModelBase
    {
        public SetBaleWeightDialogModel(string nomenclatureName)
        {
            Message = "Укажите вес кипы " + nomenclatureName;
        }

        public string Message { get; set; }

        [Range(1,10000, ErrorMessage = @"Вес должен бьть больше 0")]
        public int Weight { get; set; }
    }
}
