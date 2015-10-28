using GalaSoft.MvvmLight;
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
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ValidationViewModelBase : RootViewModel, IDataErrorInfo, IValidationExceptionHandler
    {
        private readonly Dictionary<string, Func<ValidationViewModelBase, object>> propertyGetters;
        private readonly Dictionary<string, ValidationAttribute[]> validators;

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
            public string this[string propertyName]
            {
                get
                {
                if (this.propertyGetters.ContainsKey(propertyName))
                    {
                        var propertyValue = this.propertyGetters[propertyName](this);
                        var errorMessages = this.validators[propertyName]
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
                var errors = from validator in this.validators
                             from attribute in validator.Value
                             where !attribute.IsValid(this.propertyGetters[validator.Key](this))
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
                var query = from validator in this.validators
                            where validator.Value.All(attribute => attribute.IsValid(this.propertyGetters[validator.Key](this)))
                            select validator;

                var count = query.Count() - this.validationExceptionCount;
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
                return this.validators.Count();
            }
        }

        public ValidationViewModelBase()
        {
            this.validators = this.GetType()
                .GetProperties()
                .Where(p => this.GetValidations(p).Length != 0)
                .ToDictionary(p => p.Name, p => this.GetValidations(p));

            this.propertyGetters = this.GetType()
                .GetProperties()
                .Where(p => this.GetValidations(p).Length != 0)
                .ToDictionary(p => p.Name, p => this.GetValueGetter(p));
        }

        private ValidationAttribute[] GetValidations(PropertyInfo property)
        {
            return (ValidationAttribute[])property.GetCustomAttributes(typeof(ValidationAttribute), true);
        }

        private Func<ValidationViewModelBase, object> GetValueGetter(PropertyInfo property)
        {
            return new Func<ValidationViewModelBase, object>(viewmodel => property.GetValue(viewmodel, null));
        }

        private int validationExceptionCount;

        public void ValidationExceptionsChanged(int count)
        {
            this.validationExceptionCount = count;
            this.RaisePropertyChanged("ValidPropertiesCount");
        }

        public virtual bool IsValid
        {
            get { return (this.ValidPropertiesCount == this.TotalPropertiesWithValidationCount); }
        }
        
    }
        
}