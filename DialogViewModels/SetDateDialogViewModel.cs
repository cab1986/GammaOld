// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.ComponentModel.DataAnnotations;
using Gamma.ViewModels;
using System;
using Gamma.Common;

namespace Gamma.DialogViewModels
{
    public class SetDateDialogViewModel : ValidationViewModelBase
    {
        public SetDateDialogViewModel(string message, string label, DateParam startDate, DateParam endDate = null)
        {
            Message = message;
            Label = Label;
            IsVisibleStartDate = startDate != null;
            IsVisibleEndDate = endDate != null;
            StartDate = startDate?.DefaultDate;
            EndDate = endDate?.DefaultDate;
            LabelStartDate = startDate ?.Label;
            LabelEndDate = endDate?.Label;
            MinStartDate = startDate?.MinDate;
            MaxStartDate = startDate?.MaxDate;
            MinEndDate = endDate?.MinDate;
            MaxEndDate = endDate?.MaxDate;
        }

        public string Message { get; set; }
        public string Label { get; set; }
        public string LabelStartDate { get; set; }
        public string LabelEndDate { get; set; }

        private string _errorStartDate { get; set; }
        public string ErrorStartDate
        {
            get
            {
                return _errorStartDate;
            }
            set
            {
                _errorStartDate = value;
                RaisePropertiesChanged("ErrorStartDate");
            }
        }
        public string _errorEndDate { get; set; }
        public string ErrorEndDate
        {
            get
            {
                return _errorEndDate;
            }
            set
            {
                _errorEndDate = value;
                RaisePropertiesChanged("ErrorEndDate");
            }
        }

        public bool IsVisibleStartDate { get; set; }
        public bool IsVisibleEndDate { get; set; }

        public DateTime? MinStartDate { get; set; }
        public DateTime? MaxStartDate { get; set; }
        public DateTime? MinEndDate { get; set; }
        public DateTime? MaxEndDate { get; set; }

        private DateTime? _startDate { get; set; }
        public DateTime? StartDate
        {
            get { return _startDate; }
            set
            {
                if (value >= EndDate)
                {
                    ErrorStartDate = "Ошибка! Дата начала должна быть меньше даты окончания";
                }
                else if (value < MinStartDate)
                {
                    ErrorStartDate = "Ошибка! Дата начала должна быть больше минимального - " + MinStartDate.ToString();
                }
                else if (value > MaxStartDate)
                {
                    ErrorStartDate = "Ошибка! Дата начала должна быть меньше максимального - " + MaxStartDate.ToString();
                }
                else
                {
                    _startDate = value;
                    ErrorStartDate = "";
                }
            }
        }

        private DateTime? _endDate { get; set; }
        public DateTime? EndDate
        {
            get { return _endDate; }
            set
            {
                if (value <= StartDate)
                {
                    ErrorEndDate = "Ошибка! Дата окончания должна быть больше даты начала";
                }
                else if (value < MinEndDate)
                {
                    ErrorEndDate = "Ошибка! Дата окончания должна быть больше минимального - " + MinEndDate.ToString();
                }
                else if (value > MaxEndDate)
                {
                    ErrorEndDate = "Ошибка! Дата окончания должна быть меньше максимального - " + MaxEndDate.ToString();
                }
                else
                {
                    _endDate = value;
                    ErrorEndDate = "";
                }
            }
        }

    }
}
