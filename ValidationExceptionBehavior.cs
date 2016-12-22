// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm.UI.Interactivity;
using Gamma.Interfaces;

namespace Gamma
{
    class ValidationExceptionBehavior : Behavior<FrameworkElement>
    {
        private int _validationExceptionCount;

        protected override void OnAttached()
        {
            _handler = OnValidationError;
            this.AssociatedObject.AddHandler(Validation.ErrorEvent, _handler);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler(Validation.ErrorEvent, _handler);
        }

        private EventHandler<ValidationErrorEventArgs> _handler ;

        private void OnValidationError(object sender, ValidationErrorEventArgs e)
        {
            // we want to count only the validation error with an exception
            // other error are handled by using the attribute on the properties
            if (e.Error.Exception == null)
            {
                return;
            }

            if (e.Action == ValidationErrorEventAction.Added)
            {
                this._validationExceptionCount++;
            }
            else
            {
                this._validationExceptionCount--;
            }

            if (this.AssociatedObject.DataContext is IValidationExceptionHandler)
            {
                // transfer the information back to the viewmodel
                var viewModel = (IValidationExceptionHandler)this.AssociatedObject.DataContext;
                viewModel.ValidationExceptionsChanged(this._validationExceptionCount);
            }
        }
    }
}
