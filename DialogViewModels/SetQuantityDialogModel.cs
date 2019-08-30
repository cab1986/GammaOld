// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.ComponentModel.DataAnnotations;
using Gamma.ViewModels;

namespace Gamma.DialogViewModels
{
    public class SetQuantityDialogModel : ValidationViewModelBase
    {
        public SetQuantityDialogModel(string message, string label)
        {
            Message = message;
            Label = Label;
        }

        public string Message { get; set; }
        public string Label { get; set; }

        [Range(1,10000, ErrorMessage = @"Кол-во должно быть больше 0")]
        public int Quantity { get; set; }
    }
}
