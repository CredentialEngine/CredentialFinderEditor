//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Data.Views
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrganizationOwnsEtcRolesSummary
    {
        public int OrgId { get; set; }
        public string OrgName { get; set; }
        public int EntityId { get; set; }
        public System.Guid AgentUid { get; set; }
        public int RelationshipTypeId { get; set; }
        public string ReverseRelation { get; set; }
        public int EntityTypeId { get; set; }
        public string EntityType { get; set; }
        public int BaseId { get; set; }
        public string EntityName { get; set; }
        public string CTID { get; set; }
        public string Description { get; set; }
        public string SubjectWebpage { get; set; }
        public string CtdlType { get; set; }
        public Nullable<bool> IsQARole { get; set; }
        public Nullable<bool> IsOwnerAgentRole { get; set; }
        public Nullable<bool> IsEntityToAgentRole { get; set; }
    }
}
