using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.ProfileModels;

namespace Models.Node
{
    [Profile( DBType = typeof( Models.Common.Credential ) )]
    public class Credential : BaseMainProfile
    {
        public Credential()
        {
            Subject = new List<TextValueProfile>();
            Keyword = new List<TextValueProfile>();
            DegreeConcentration = new List<TextValueProfile>();
            DegreeMajor = new List<TextValueProfile>();
            DegreeMinor = new List<TextValueProfile>();
            CommonCosts = new List<ProfileLink>();
            CommonConditions = new List<ProfileLink>();
            Requires = new List<ProfileLink>();
            Recommends = new List<ProfileLink>();
            Corequisite = new List<ProfileLink>();
            EstimatedCosts = new List<ProfileLink>();
            FinancialAssistance = new List<ProfileLink>();
            AlternativeIndustries = new List<TextValueProfile>();
            AlternativeOccupations = new List<TextValueProfile>();

            JurisdictionAssertions = new List<ProfileLink>();
            OfferedByOrganizationRole = new List<ProfileLink>();
            OfferedByOrganization = new List<ProfileLink>();
        }
        //Basic Info
        //Url is in BaseMainProfile
        //SubjectWebpage was added to BaseMainProfile.
        //public string SubjectWebpage { get; set; }
        public string ImageUrl { get; set; }
        public string AlternateName { get; set; }
        public string VersionIdentifier { get; set; }
        public string LatestVersionUrl { get; set; }
        public string PreviousVersion { get; set; }
        public int ManagingOrgId { get; set; }

        //maintain old
        public int InLanguageId { get; set; }
        public List<int> InLanguageIds { get; set; }
        public List<LanguageProfile> InLanguageCodeList 
        {
            get
            {
                var list = new List<LanguageProfile>();
                foreach ( var item in InLanguageIds )
                {
                    var newItem = new LanguageProfile { LanguageCodeId = item };
                    list.Add( newItem );
                }
                return list;
            }
        }
        public string CredentialRegistryId { get; set; }
        public string CTID { get; set; }

        [Property( DBName = "OwningOrganization", DBType = typeof( Models.Common.Organization ) )]
        public ProfileLink DisplayOwningOrganization { get; set; }

        [Property( DBName = "OwningAgentUid", DBType = typeof( Guid ) )]
        public ProfileLink OwningOrganization { get; set; }

        /// <summary>
        /// OwnerRoles are used only for add
        /// </summary>
        [Property( DBName = "OwnerRoles", DBType = typeof( Models.Common.Enumeration ) )]
        public List<int> RoleTypeIds { get; set; }

        //NOTE: AgentRole_Recipient/OrganizationRole is defined in BaseMainProfile
        [Property( Type = typeof( AgentRoleProfile_Recipient ), DBName = "OwnerOrganizationRoles" )]
        public List<ProfileLink> OwnerOrganizationRoles { get; set; }

        [Property( Type = typeof( AgentRoleProfile_Recipient ), DBName = "OrganizationRole" )]
        public List<ProfileLink> QAOrganizationRole { get; set; }

        [Property( Type = typeof( AgentRoleProfile_Recipient ), DBName = "OfferedByOrganizationRole" )]
        public List<ProfileLink> OfferedByOrganizationRole { get; set; }

        [Property( Type = typeof( Organization ) )]
        public List<ProfileLink> OfferedByOrganization { get; set; }

        public int EarningCredentialPrimaryMethodId { get; set; }
        public bool FeatureLearningOpportunities { get; set; }
        public bool FeatureAssessments { get; set; }

        //List-based Info
        [Property( DBName = "CredentialType", DBType = typeof( Models.Common.Enumeration ) )]
        public int CredentialType { get { return CredentialTypeIds.FirstOrDefault(); } set { CredentialTypeIds = new List<int>() { value }; } }

        [Property( DBName = "null" )] //Database processes need to skip this item
        public List<int> CredentialTypeIds { get; set; }

        public string CredentialTypeDisplay { get; set; }
        public string OwningOrgDisplay { get; set; }


        [Property( DBName = "CopyrightHolder", DBType = typeof( Guid ) )]
        public ProfileLink CopyrightHolder { get; set; }
        //[Property( DBName = "Purpose", DBType = typeof( Models.Common.Enumeration ) )]
        //public List<int> IntendedPurpose { get; set; }

        [Property( DBName = "AudienceLevelType", DBType = typeof( Models.Common.Enumeration ) )]
        public List<int> AudienceLevelType { get; set; }

        [Property( DBName = "AudienceType", DBType = typeof( Models.Common.Enumeration ) )]
        public List<int> AudienceType { get; set; }

        [Property( DBName = "CredentialStatusType", DBType = typeof( Models.Common.Enumeration ) )]
        public int CredentialStatusType { get; set; }

		[Property( DBName = "LearningDeliveryType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> LearningDeliveryType { get; set; }

		[Property( DBName = "AssessmentDeliveryType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> AssessmentDeliveryType { get; set; }

		public string AvailableOnlineAt { get; set; }
        public string AvailabilityListing { get; set; }

        public List<TextValueProfile> Subject { get; set; }
        public List<TextValueProfile> DegreeConcentration { get; set; }
        public List<TextValueProfile> DegreeMajor { get; set; }
        public List<TextValueProfile> DegreeMinor { get; set; }
        public List<TextValueProfile> Keyword { get; set; }
        public List<TextValueProfile> AlternativeIndustries { get; set; }
        public List<TextValueProfile> AlternativeOccupations { get; set; }
        //Text Value Info
        [Property( Type = typeof( MicroProfile ), DBType = typeof( Models.Common.Enumeration ) )]
        public List<ProfileLink> Industry { get; set; }

        [Property( Type = typeof( MicroProfile ), DBType = typeof( Models.Common.Enumeration ) )]
        public List<ProfileLink> Occupation { get; set; }

		[Property( DBName = "InstructionalProgramType", Type = typeof( MicroProfile ), DBType = typeof( Models.Common.Enumeration ) )]
		public List<ProfileLink> CipCode { get; set; }

		public List<TextValueProfile> AlternativeInstructionalProgramType { get; set; }

		//Profile Info
		[Property( DBName = "EstimatedDuration", Type = typeof( DurationProfile ) )]
        public List<ProfileLink> DurationProfile { get; set; }

        [Property( DBName = "RenewalFrequency", Type = typeof( DurationProfile ) )]
        public List<ProfileLink> RenewalFrequency { get; set; }

        #region Condition profile related, and all targets
        //[Property( DBName = "TargetCredential", DBType = typeof( Credential ) )]
        //public List<ProfileLink> TargetCredential { get; set; }

        [Property( DBName = "TargetLearningOpportunity", Type = typeof( LearningOpportunityProfile ) )]
        public List<ProfileLink> TargetLearningOpportunity { get; set; }

        [Property( DBName = "TargetAssessment", Type = typeof( AssessmentProfile ) )]
        public List<ProfileLink> TargetAssessment { get; set; }

        //not sure which will be correct
        //[Property( DBName = "TargetLearningOpportunity" )]
        //public List<ProfileLink> LearningOpportunity { get; set; }


        //[Property( DBName = "TargetAssessment" )]
        //public List<ProfileLink> Assessment { get; set; }


        [Property( Type = typeof( ConditionProfile ) )]
        public List<ProfileLink> CredentialConnections { get; set; }


        #endregion
        [Property( Type = typeof( ConditionManifest ) )]
        public List<ProfileLink> CommonCosts { get; set; }

        #region Condition profile related
        [Property( Type = typeof( ConditionManifest ) )]
        public List<ProfileLink> CommonConditions { get; set; }

        [Property( Type = typeof( ConditionProfile ) )]
        public List<ProfileLink> Requires { get; set; }

        [Property( Type = typeof( ConditionProfile ) )]
        public List<ProfileLink> Recommends { get; set; }


        [Property( Type = typeof( ConditionProfile ) )]
        public List<ProfileLink> Corequisite { get; set; }

        //[Property( Type = typeof( ConditionProfile ) )]
        //public List<ProfileLink> IsRequiredFor { get; set; }

        //[Property( Type = typeof( ConditionProfile ) )]
        //public List<ProfileLink> IsRecommendedFor { get; set; }

        //[Property( Type = typeof( ConditionProfile ) )]
        //public List<ProfileLink> AdvancedStandingFor { get; set; }

        //[Property( Type = typeof( ConditionProfile ) )]
        //public List<ProfileLink> AdvancedStandingFrom { get; set; }

        //[Property( Type = typeof( ConditionProfile ) )]
        //public List<ProfileLink> IsPreparationFor { get; set; }

        //[Property( Type = typeof( ConditionProfile ) )]
        //public List<ProfileLink> PreparationFrom { get; set; }

        [Property( Type = typeof( ConditionProfile ) )]
        public List<ProfileLink> Renewal { get; set; }

        //[Property( Type = typeof( RevocationProfile ) )]
        //public List<ProfileLink> Revocation { get; set; }
        #endregion

        [Property( Type = typeof( ProcessProfile ) )]
        public List<ProfileLink> AdministrationProcess { get; set; }

        [Property( Type = typeof( ProcessProfile ) )]
        public List<ProfileLink> DevelopmentProcess { get; set; }

        [Property( Type = typeof( ProcessProfile ) )]
        public List<ProfileLink> MaintenanceProcess { get; set; }

        [Property( Type = typeof( ProcessProfile ) )]
        public List<ProfileLink> AppealProcess { get; set; }

        [Property( Type = typeof( ProcessProfile ) )]
        public List<ProfileLink> ComplaintProcess { get; set; }

        [Property( Type = typeof( ProcessProfile ) )]
        public List<ProfileLink> RevocationProcess { get; set; }

        [Property( Type = typeof( ProcessProfile ) )]
        public List<ProfileLink> ReviewProcess { get; set; }


        //[Property( Type = typeof( EarningsProfile ) )]
        //public List<ProfileLink> Earnings { get; set; }

        //[Property( Type = typeof( EmploymentOutcomeProfile ) )]
        //public List<ProfileLink> EmploymentOutcome { get; set; }

        //[Property( Type = typeof( HoldersProfile ) )]
        //public List<ProfileLink> Holders { get; set; }

        [Property( Type = typeof( Credential ) )]
        public List<ProfileLink> EmbeddedCredentials { get; set; }

        [Property( Type = typeof( Credential ) )]
        public List<ProfileLink> ParentCredential { get; set; }

        [Property( Type = typeof( CostProfile ) )]
        public List<ProfileLink> EstimatedCosts { get; set; }

        [Property( Type = typeof( FinancialAlignmentObject ) )]
        public List<ProfileLink> FinancialAssistance { get; set; }

        [Property( Type = typeof( CostProfile ) )]
        public List<ProfileLink> AssessmentEstimatedCosts { get; set; }

        [Property( Type = typeof( CostProfile ) )]
        public List<ProfileLink> LearningOpportunityEstimatedCosts { get; set; }

        [Property( DBName = "Addresses", DBType = typeof( Models.Common.Address ) )]
        public List<ProfileLink> Addresses { get; set; }
        public string CredentialId { get; set; }
        public string CodedNotation { get; set; }

        /// <summary>
        /// processStandards (Nov2016)
        /// URL
        /// </summary>
        public string ProcessStandards { get; set; }
        /// <summary>
        /// ProcessStandardsDescription (Nov2016)
        /// </summary>
        public string ProcessStandardsDescription { get; set; }

        [Property( DBName = "VerificationServiceProfiles", DBType = typeof( Models.ProfileModels.VerificationServiceProfile ) )]
        public List<ProfileLink> VerificationService { get; set; }

        [Property( Type = typeof( JurisdictionProfile ) )]
        public List<ProfileLink> JurisdictionAssertions { get; set; }


        [Property( Type = typeof( RevocationProfile ) )]
        public List<ProfileLink> Revocation { get; set; }

		//Profile Info
		[Property( Type = typeof( JurisdictionProfile ) )]
        public List<ProfileLink> Region { get; set; }
    }
    //

}
