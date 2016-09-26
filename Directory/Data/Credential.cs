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
    
    public partial class Credential
    {
        public Credential()
        {
            this.Credential_ConnectionProfile = new HashSet<Credential_ConnectionProfile>();
            this.Entity_Credential = new HashSet<Entity_Credential>();
            this.Entity_QA_Action = new HashSet<Entity_QA_Action>();
            this.Entity_VerificationProfile = new HashSet<Entity_VerificationProfile>();
        }
    
        public int Id { get; set; }
        public System.Guid RowId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public string Url { get; set; }
        public string LatestVersionUrl { get; set; }
        public string ReplacesVersionUrl { get; set; }
        public string ImageUrl { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
        public Nullable<int> StatusId { get; set; }
        public string CredentialRegistryId { get; set; }
        public Nullable<int> ManagingOrgId { get; set; }
        public string AlternateName { get; set; }
        public string CTID { get; set; }
        public string AvailableOnlineAt { get; set; }
    
        public virtual Codes_Status Codes_Status { get; set; }
        public virtual ICollection<Credential_ConnectionProfile> Credential_ConnectionProfile { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual ICollection<Entity_Credential> Entity_Credential { get; set; }
        public virtual ICollection<Entity_QA_Action> Entity_QA_Action { get; set; }
        public virtual ICollection<Entity_VerificationProfile> Entity_VerificationProfile { get; set; }
    }
}
