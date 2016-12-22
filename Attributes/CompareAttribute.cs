// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.ComponentModel.DataAnnotations;

namespace Gamma.Attributes
{
    [AttributeUsage(AttributeTargets.Property |
        AttributeTargets.Field, AllowMultiple = false)]
    class CompareAttribute : ValidationAttribute
    {
        public CompareAttribute(string otherProperty, string errormessage)
            : base(errormessage)
        {
            if (otherProperty == null) throw new ArgumentNullException();
            OtherProperty = otherProperty;
        }
        public CompareAttribute(string otherProperty)
        {
            if (otherProperty == null) throw new ArgumentNullException();
            OtherProperty = otherProperty;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // the the other property
            if (validationContext == null) return null;
            var property = validationContext.ObjectType.GetProperty(OtherProperty);

            // check it is not null
            if (property == null)
                return new ValidationResult(String.Format("Unknown property: {0}.", OtherProperty));
            // check types
            if (validationContext.ObjectType.GetProperty(validationContext.MemberName).PropertyType != property.PropertyType)
                return new ValidationResult(String.Format("The types of {0} and {1} must be the same.", validationContext.DisplayName, OtherProperty));

            // get the other value
            var other = property.GetValue(validationContext.ObjectInstance, null);
            if (Object.Equals(value, other)) return null;
            return new ValidationResult(ErrorMessage);
        }
        
        private string OtherProperty { get; set; }
    }
    
}
