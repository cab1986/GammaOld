// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.ComponentModel.DataAnnotations;
using Gamma.ViewModels;
using System;

namespace Gamma.DialogViewModels
{
    public class SetQuantityDialogModel : ValidationViewModelBase
    {
        public SetQuantityDialogModel(string message, string label, int minQuantity, int maxQuantity)
        {
            Message = message + " (от " + minQuantity.ToString() + " до " + maxQuantity.ToString() + ")";
            Label = Label;
            MinQuantity = minQuantity;
            MaxQuantity = maxQuantity;
        }

        public string Message { get; set; }
        public string Label { get; set; }

        public int MinQuantity { get; set; }
        public int MaxQuantity { get; set; }

        private int _quantity { get; set; }
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                //if (value < MinQuantity)
                //{
                //    ErrorText = "Ошибка! Значение должно быть больше минимального - " + MinQuantity.ToString();
                //}
                //else
                //{
                    _quantity = value;
                //    ErrorText = "";
                //}
            }
        }

        //private string _errorText { get; set; }
        //public string ErrorText
        //{
        //    get { return _errorText; }
        //    set
        //    {
        //        _errorText = value;
        //    }
        //}
        
    }
}
