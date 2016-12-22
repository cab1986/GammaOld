// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Gamma.Common
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RequiredCollectionAttribute: ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var collection = value as ICollection;
            return collection?.Count > 0;
        }
    }
}
