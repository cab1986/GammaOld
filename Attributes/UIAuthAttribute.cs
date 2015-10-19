using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Attributes
{
    public enum UIAuthLevel { Invisible, ReadOnly, Editable}

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    sealed class UIAuthAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        // This is a positional argument
        public UIAuthAttribute(UIAuthLevel AuthLevel)
        {
            this.AuthLevel = AuthLevel;
            // TODO: Implement code here
            //throw new NotImplementedException();
        }

        // This is a named argument
        public UIAuthLevel AuthLevel { get; set; }
    }
}
