using System;

namespace Gamma.Attributes
{
    public enum UIAuthLevel { Invisible, ReadOnly, Editable}

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    sealed class UIAuthAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?Linkid=85236

        // This is a positional argument
        public UIAuthAttribute(UIAuthLevel authLevel)
        {
            this.AuthLevel = authLevel;
        }

        // This is a named argument
        public UIAuthLevel AuthLevel { get; set; }
    }
}
