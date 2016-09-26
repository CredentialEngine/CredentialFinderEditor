//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Data.Tables
{
    using System;
    using System.Collections.Generic;
    
    public partial class Entity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Entity()
        {
            this.Entity_AgentRelationship = new HashSet<Entity_AgentRelationship>();
            this.Entity_Assessment = new HashSet<Entity_Assessment>();
            this.Entity_Competency = new HashSet<Entity_Competency>();
            this.Entity_CostProfile = new HashSet<Entity_CostProfile>();
            this.Entity_Credential = new HashSet<Entity_Credential>();
            this.Entity_DurationProfile = new HashSet<Entity_DurationProfile>();
            this.Entity_FrameworkItem = new HashSet<Entity_FrameworkItem>();
            this.Entity_LearningOpportunity = new HashSet<Entity_LearningOpportunity>();
            this.Entity_Property = new HashSet<Entity_Property>();
            this.Entity_PropertyOther = new HashSet<Entity_PropertyOther>();
            this.Entity_QA_Action = new HashSet<Entity_QA_Action>();
            this.Entity_Reference = new HashSet<Entity_Reference>();
            this.Entity_RevocationProfile = new HashSet<Entity_RevocationProfile>();
            this.Entity_TaskProfile = new HashSet<Entity_TaskProfile>();
            this.Entity_VerificationProfile = new HashSet<Entity_VerificationProfile>();
        }
    
        public int Id { get; set; }
        public System.Guid EntityUid { get; set; }
        public int EntityTypeId { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
    
        public virtual Codes_EntityType Codes_EntityType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_AgentRelationship> Entity_AgentRelationship { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_Assessment> Entity_Assessment { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_Competency> Entity_Competency { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_CostProfile> Entity_CostProfile { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_Credential> Entity_Credential { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_DurationProfile> Entity_DurationProfile { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_FrameworkItem> Entity_FrameworkItem { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_LearningOpportunity> Entity_LearningOpportunity { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_Property> Entity_Property { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_PropertyOther> Entity_PropertyOther { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_QA_Action> Entity_QA_Action { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_Reference> Entity_Reference { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_RevocationProfile> Entity_RevocationProfile { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_TaskProfile> Entity_TaskProfile { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_VerificationProfile> Entity_VerificationProfile { get; set; }
    }
}
