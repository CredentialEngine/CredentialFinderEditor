﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CTIEntities1 : DbContext
    {
        public CTIEntities1()
            : base("name=CTIEntities1")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Entity_QAAction_Summary> Entity_QAAction_Summary { get; set; }
        public virtual DbSet<Entity_Relationship_AgentSummary> Entity_Relationship_AgentSummary { get; set; }
        public virtual DbSet<Account_Summary> Account_Summary { get; set; }
        public virtual DbSet<Credential_AgentRoleIdCSV> Credential_AgentRoleIdCSV { get; set; }
        public virtual DbSet<Credential_Summary> Credential_Summary { get; set; }
        public virtual DbSet<Entity_AgentRelationshipIdCSV> Entity_AgentRelationshipIdCSV { get; set; }
        public virtual DbSet<Entity_Reference_Summary> Entity_Reference_Summary { get; set; }
        public virtual DbSet<Entity_Summary> Entity_Summary { get; set; }
        public virtual DbSet<Assessment_Summary> Assessment_Summary { get; set; }
        public virtual DbSet<CodesProperty_Summary> CodesProperty_Summary { get; set; }
        public virtual DbSet<EntityProperty_Summary> EntityProperty_Summary { get; set; }
        public virtual DbSet<LearningOpportunity_Summary> LearningOpportunity_Summary { get; set; }
        public virtual DbSet<Organization_Summary> Organization_Summary { get; set; }
        public virtual DbSet<Entity_LearningOpportunity_IsPartOfSummary> Entity_LearningOpportunity_IsPartOfSummary { get; set; }
        public virtual DbSet<Entity_LearningOpportunity_Summary> Entity_LearningOpportunity_Summary { get; set; }
        public virtual DbSet<Entity_PropertyOtherSummary> Entity_PropertyOtherSummary { get; set; }
        public virtual DbSet<Entity_ReferenceUrls_Summary> Entity_ReferenceUrls_Summary { get; set; }
        public virtual DbSet<ListValidEntitiesAndAliases> ListValidEntitiesAndAliases { get; set; }
        public virtual DbSet<Naics_Select2DigitCodes> Naics_Select2DigitCodes { get; set; }
        public virtual DbSet<Naics_Select6DigitCodes> Naics_Select6DigitCodes { get; set; }
        public virtual DbSet<Organization_ServiceSummary> Organization_ServiceSummary { get; set; }
        public virtual DbSet<OrganizationMemberSummary> OrganizationMemberSummaries { get; set; }
        public virtual DbSet<AspNetUserRoles_Summary> AspNetUserRoles_Summary { get; set; }
        public virtual DbSet<Entity_FrameworkItemSummary> Entity_FrameworkItemSummary { get; set; }
        public virtual DbSet<Agent_Summary> Agent_Summary { get; set; }
        public virtual DbSet<CredentialAgentRelationships_Summary> CredentialAgentRelationships_Summary { get; set; }
        public virtual DbSet<Entity_NaicsCSV> Entity_NaicsCSV { get; set; }
        public virtual DbSet<Organization_PropertyOther_Summary> Organization_PropertyOther_Summary { get; set; }
        public virtual DbSet<OrganizationProperty_Summary> OrganizationProperty_Summary { get; set; }
        public virtual DbSet<Activity_MetadataRegistrySummary> Activity_MetadataRegistrySummary { get; set; }
        public virtual DbSet<ConditionProfile_ParentSummary> ConditionProfile_ParentSummary { get; set; }
        public virtual DbSet<Entity_Subjects> Entity_Subjects { get; set; }
        public virtual DbSet<Activity_Summary> Activity_Summary { get; set; }
        public virtual DbSet<Activity_Today_Summary> Activity_Today_Summary { get; set; }
        public virtual DbSet<Credential_Assets> Credential_Assets { get; set; }
        public virtual DbSet<CostProfile_SummaryForSearch> CostProfile_SummaryForSearch { get; set; }
        public virtual DbSet<Entity_AgentRelationship_Totals> Entity_AgentRelationship_Totals { get; set; }
        public virtual DbSet<Credential_Assets_AgentRelationship_Totals> Credential_Assets_AgentRelationship_Totals { get; set; }
        public virtual DbSet<SiteTotalsSummary> SiteTotalsSummaries { get; set; }
        public virtual DbSet<CodesProperty_Counts_ByEntity> CodesProperty_Counts_ByEntity { get; set; }
        public virtual DbSet<Entity_FrameworkCIPGroupSummary> Entity_FrameworkCIPGroupSummary { get; set; }
        public virtual DbSet<Entity_FrameworkIndustryGroupSummary> Entity_FrameworkIndustryGroupSummary { get; set; }
        public virtual DbSet<Entity_FrameworkOccupationGroupSummary> Entity_FrameworkOccupationGroupSummary { get; set; }
        public virtual DbSet<Entity_FrameworkCIPCodeSummary> Entity_FrameworkCIPCodeSummary { get; set; }
        public virtual DbSet<Entity_FrameworkIndustryCodeSummary> Entity_FrameworkIndustryCodeSummary { get; set; }
        public virtual DbSet<Entity_AssertionCSV> Entity_AssertionCSV { get; set; }
        public virtual DbSet<Entity_Assertion_Summary> Entity_Assertion_Summary { get; set; }
        public virtual DbSet<Entity_Competencies_ForExport> Entity_Competencies_ForExport { get; set; }
        public virtual DbSet<Entity_Competencies_Summary> Entity_Competencies_Summary { get; set; }
        public virtual DbSet<Entity_FrameworkItems_Totals> Entity_FrameworkItems_Totals { get; set; }
        public virtual DbSet<Entity_Relationship_ToOrgSummary> Entity_Relationship_ToOrgSummary { get; set; }
        public virtual DbSet<Organization_CombinedQAPerformed> Organization_CombinedQAPerformed { get; set; }
        public virtual DbSet<OrganizationOwnsEtcRolesSummary> OrganizationOwnsEtcRolesSummary { get; set; }
        public virtual DbSet<Entity_ConditionProfileCompetencies_Summary> Entity_ConditionProfileCompetencies_Summary { get; set; }
    }
}
