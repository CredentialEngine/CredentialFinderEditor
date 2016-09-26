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
    
    public partial class Organization
    {
        public Organization()
        {
            this.Assessment = new HashSet<Assessment>();
            this.Credential = new HashSet<Credential>();
            this.LearningOpportunity = new HashSet<LearningOpportunity>();
            this.Organization_Address = new HashSet<Organization_Address>();
            this.Organization_Member = new HashSet<Organization_Member>();
            this.Organization_PropertyOther = new HashSet<Organization_PropertyOther>();
            this.Organization_Service = new HashSet<Organization_Service>();
        }
    
        public int Id { get; set; }
        public System.Guid RowId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public string UniqueURI { get; set; }
        public string ImageURL { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
        public string MainPhoneNumber { get; set; }
        public string Email { get; set; }
        public Nullable<int> StatusId { get; set; }
        public string Purpose { get; set; }
        public string FoundingDate { get; set; }
        public string CredentialRegistryId { get; set; }
        public string ServiceTypeOther { get; set; }
        public string FaxNumber { get; set; }
        public string CTID { get; set; }
    
        public virtual ICollection<Assessment> Assessment { get; set; }
        public virtual Codes_Status Codes_Status { get; set; }
        public virtual ICollection<Credential> Credential { get; set; }
        public virtual ICollection<LearningOpportunity> LearningOpportunity { get; set; }
        public virtual ICollection<Organization_Address> Organization_Address { get; set; }
        public virtual ICollection<Organization_Member> Organization_Member { get; set; }
        public virtual ICollection<Organization_PropertyOther> Organization_PropertyOther { get; set; }
        public virtual ICollection<Organization_Service> Organization_Service { get; set; }
    }
}