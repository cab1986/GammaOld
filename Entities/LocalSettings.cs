//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gamma.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class LocalSettings
    {
        public int BranchID { get; set; }
        public string LabelPath { get; set; }
        public string MailServiceAddress { get; set; }
        public string GammaServiceAddress { get; set; }
        public Nullable<int> DocLastNumber { get; set; }
        public Nullable<int> TaskLastNumber { get; set; }
        public Nullable<bool> IsGet1CNomenclatureLanched { get; set; }
        public Nullable<System.DateTime> LastExecGet1CNomenclature { get; set; }
        public Nullable<bool> IsUploadDocBrokeTo1CWhenSave { get; set; }
        public Nullable<bool> IsUsedInOneDocMaterialDirectCalcAndComposition { get; set; }
        public Nullable<System.DateTime> LastSuccessCompletedGet1CNomenclature { get; set; }
        public Nullable<System.DateTime> LastSuccessCompletedImportCellulose { get; set; }
    }
}
