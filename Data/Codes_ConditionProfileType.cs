//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Codes_ConditionProfileType
    {
        public Codes_ConditionProfileType()
        {
            this.Entity_ConditionProfile = new HashSet<Entity_ConditionProfile>();
        }
    
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Nullable<int> SortOrder { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsCommonCondtionType { get; set; }
        public Nullable<bool> IsLearningOpportunityType { get; set; }
        public Nullable<bool> IsAssessmentType { get; set; }
        public Nullable<bool> IsCredentialsConnectionType { get; set; }
        public string SchemaName { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<int> Totals { get; set; }
        public string ConditionManifestTitle { get; set; }
        public Nullable<int> CredentialTotals { get; set; }
        public Nullable<int> OrganizationTotals { get; set; }
        public Nullable<int> AssessmentTotals { get; set; }
        public Nullable<int> LoppTotals { get; set; }
    
        public virtual ICollection<Entity_ConditionProfile> Entity_ConditionProfile { get; set; }
    }
}
