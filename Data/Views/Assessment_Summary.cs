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
    
    public partial class Assessment_Summary
    {
        public int Id { get; set; }
        public Nullable<int> StatusId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> DateEffective { get; set; }
        public string Url { get; set; }
        public string IdentificationCode { get; set; }
        public Nullable<int> ManagingOrgId { get; set; }
        public string ManagingOrganization { get; set; }
        public int OrgId { get; set; }
        public string Organization { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
        public System.Guid RowId { get; set; }
        public string availableOnlineAt { get; set; }
        public string CTID { get; set; }
        public string CredentialRegistryId { get; set; }
        public int EntityId { get; set; }
        public Nullable<System.DateTime> EntityLastUpdated { get; set; }
        public string ExternalIdentifier { get; set; }
        public string cerEnvelopeUrl { get; set; }
        public string editUrl { get; set; }
        public string detailUrl { get; set; }
        public Nullable<System.Guid> OwningAgentUid { get; set; }
        public string AvailabilityListing { get; set; }
        public int AvailableAtCount { get; set; }
        public int InLanguageId { get; set; }
        public string DeliveryTypeDescription { get; set; }
        public string VerificationMethodDescription { get; set; }
        public string AssessmentExampleUrl { get; set; }
        public string AssessmentExampleDescription { get; set; }
        public string AssessmentOutput { get; set; }
        public string ExternalResearch { get; set; }
        public Nullable<bool> HasGroupEvaluation { get; set; }
        public Nullable<bool> HasGroupParticipation { get; set; }
        public Nullable<bool> IsProctored { get; set; }
        public string ProcessStandards { get; set; }
        public string ProcessStandardsDescription { get; set; }
        public string ScoringMethodDescription { get; set; }
        public string ScoringMethodExample { get; set; }
        public string ScoringMethodExampleDescription { get; set; }
        public string VersionIdentifier { get; set; }
        public int HasCompetencyCount { get; set; }
        public int CredentialConnections { get; set; }
        public string CreditHourType { get; set; }
        public decimal CreditHourValue { get; set; }
        public string CreditUnitType { get; set; }
        public decimal CreditUnitValue { get; set; }
        public string CreditUnitTypeDescription { get; set; }
        public string LastApprovalDate2 { get; set; }
        public string LastApprovalDate { get; set; }
        public string ContentApprovedBy { get; set; }
        public Nullable<int> ContentApprovedById { get; set; }
        public string IsPublished { get; set; }
        public string LastPublishDate { get; set; }
    }
}
