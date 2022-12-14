// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.ComponentModel;
using Gamma.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Gamma.ViewModels
{
    /// <summary>
    /// Класс с валидацией свойств
    /// </summary>
    public class ValidationViewModelBase : RootViewModel, IDataErrorInfo, IValidationExceptionHandler
    {
        private readonly Dictionary<string, Func<ValidationViewModelBase, object>> _propertyGetters;
        private readonly Dictionary<string, ValidationAttribute[]> _validators;

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
            public string this[string propertyName]
            {
                get
                {
                if (_propertyGetters.ContainsKey(propertyName))
                    {
                        var propertyValue = _propertyGetters[propertyName](this);
                        var errorMessages = _validators[propertyName]
                            .Where(v => !v.IsValid(propertyValue))
                            .Select(v => v.ErrorMessage).ToArray();

                        return string.Join(Environment.NewLine, errorMessages);
                    }

                    return string.Empty;
                }
            }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        public string Error
        {
            get
            {
                var errors = from validator in _validators
                             from attribute in validator.Value
                             where !attribute.IsValid(_propertyGetters[validator.Key](this))
                             select attribute.ErrorMessage;

                return string.Join(Environment.NewLine, errors.ToArray());
            }
        }

        /// <summary>
        /// Gets the number of properties which have a validation attribute and are currently valid
        /// </summary>
        public int ValidPropertiesCount
        {
            get
            {
                var query = from validator in _validators
                            where validator.Value.All(attribute => attribute.IsValid(_propertyGetters[validator.Key](this)))
                            select validator;

                var count = query.Count() - _validationExceptionCount;
                return count;
            }
        }
        
        /// <summary>
        /// Gets the number of properties which have a validation attribute
        /// </summary>
        public int TotalPropertiesWithValidationCount
        {
            get
            {
                return _validators.Count();
            }
        }

        public ValidationViewModelBase()
        {
            _validators = GetType()
                .GetProperties()
                .Where(p => GetValidations(p).Length != 0)
                .ToDictionary(p => p.Name, p => GetValidations(p));

            _propertyGetters = GetType()
                .GetProperties()
                .Where(p => GetValidations(p).Length != 0)
                .ToDictionary(p => p.Name, p => GetValueGetter(p));
        }

        private ValidationAttribute[] GetValidations(PropertyInfo property)
        {
            return (ValidationAttribute[])property.GetCustomAttributes(typeof(ValidationAttribute), true);
        }

        private Func<ValidationViewModelBase, object> GetValueGetter(PropertyInfo property)
        {
            return new Func<ValidationViewModelBase, object>(viewmodel => property.GetValue(viewmodel, null));
        }

        private int _validationExceptionCount;

        public void ValidationExceptionsChanged(int count)
        {
            _validationExceptionCount = count;
            RaisePropertyChanged("ValidPropertiesCount");
        }

        public virtual bool IsValid
        {
            get { return ValidPropertiesCount == TotalPropertiesWithValidationCount; }
        } 
    }
        
}