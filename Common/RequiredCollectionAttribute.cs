using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

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
