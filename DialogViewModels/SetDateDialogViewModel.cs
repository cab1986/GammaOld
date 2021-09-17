// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.ComponentModel.DataAnnotations;
using Gamma.ViewModels;
using System;

namespace Gamma.DialogViewModels
{
    public class SetDateDialogViewModel : ValidationViewModelBase
    {
        public SetDateDialogViewModel(string message, string label, bool isVisibleStartDate = false, DateTime? defaultStartDate = null, string labelStartDate = null, bool isVisibleEndDate = false, DateTime? defaultEndDate = null, string labelEndDate = null)
        {
            Message = message;
            Label = Label;
            IsVisibleStartDate = isVisibleStartDate;
            IsVisibleEndDate = isVisibleEndDate;
            StartDate = defaultStartDate;
            EndDate = defaultEndDate;
            LabelStartDate = labelStartDate;
            LabelEndDate = labelEndDate;
        }

        public string Message { get; set; }
        public string Label { get; set; }
        public string LabelStartDate { get; set; }
        public string LabelEndDate { get; set; }

        public bool IsVisibleStartDate { get; set; }
        public bool IsVisibleEndDate { get; set; }

        private DateTime? _startDate { get; set; }
        public DateTime? StartDate
        {
            get { return _startDate; }
            set
            {
                //if (value < MinQuantity)
                //{
                //    ErrorText = "Ошибка! Значение должно быть больше минимального - " + MinQuantity.ToString();
                //}
                //else
                //{
                    _startDate = value;
                //    ErrorText = "";
                //}
            }
        }

        private DateTime? _endDate { get; set; }
        public DateTime? EndDate
        {
            get { return _endDate; }
            set
            {
                //if (value < MinQuantity)
                //{
                //    ErrorText = "Ошибка! Значение должно быть больше минимального - " + MinQuantity.ToString();
                //}
                //else
                //{
                _endDate = value;
                //    ErrorText = "";
                //}
            }
        }

    }
}
