using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.ProfileModels
{

    public class AssessmentProfile : BaseProfile
    {
        public AssessmentProfile()
        {
            //Publish_Type = "ceterms:AssessmentProfile";
            OwningOrganization = new Organization();
            AssessmentMethodType = new Enumeration();
            AssessmentUseType = new Enumeration();
            DeliveryType = new Enumeration();
            OrganizationRole = new List<OrganizationRoleProfile>();

            //QualityAssuranceAction = new List<QualityAssuranceActionProfile>();

            EstimatedDuration = new List<DurationProfile>();
            WhereReferenced = new List<string>();
            Subject = new List<TextValueProfile>();
            Keyword = new List<TextValueProfile>();
            Addresses = new List<Address>();
            CommonCosts = new List<CostManifest>();
            EstimatedCost = new List<CostProfile>();
            FinancialAssistance = new List<FinancialAlignmentObject>();
            CommonConditions = new List<ConditionManifest>();

            Requires = new List<ConditionProfile>();
            Recommends = new List<ConditionProfile>();
            Corequisite = new List<ConditionProfile>();
            EntryCondition = new List<ConditionProfile>();
            AssessmentConnections = new List<ConditionProfile>();
            //AssessmentProcess = new List<ProcessProfile>();
            AdministrationProcess = new List<ProcessProfile>();
            DevelopmentProcess = new List<ProcessProfile>();
            MaintenanceProcess = new List<ProcessProfile>();

            IsPartOfConditionProfile = new List<ConditionProfile>();
            IsPartOfCredential = new List<Credential>();
            IsPartOfLearningOpp = new List<LearningOpportunityProfile>();

            AssessesCompetenciesFrameworks = new List<CredentialAlignmentObjectFrameworkProfile>();
            RequiresCompetenciesFrameworks = new List<CredentialAlignmentObjectFrameworkProfile>();

            AssessesCompetencies = new List<CredentialAlignmentObjectProfile>();
            //RequiresCompetencies = new List<CredentialAlignmentObjectProfile>();
            InstructionalProgramType = new Enumeration();
            AlternativeInstructionalProgramType = new List<TextValueProfile>();
            Region = new List<JurisdictionProfile>();
            JurisdictionAssertions = new List<JurisdictionProfile>();
            ScoringMethodType = new Enumeration();
            OwnerRoles = new Enumeration();
            //to delete

            InLanguageCodeList = new List<LanguageProfile>();
        }
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public int ManagingOrgId { get; set; }
        public string ManagingOrganization { get; set; }

        public bool IsNotEmpty
        {
            get
            {
                if ( !string.IsNullOrEmpty( Name )
                    || ( !string.IsNullOrEmpty( Description ) )
                    || ( !string.IsNullOrEmpty( SubjectWebpage ) )
                    )
                    return true;
                else
                    return false;
            }
        }

        public System.Guid OwningAgentUid { get; set; }
        public Organization OwningOrganization { get; set; }
        public string OrganizationName
        {
            get
            {
                if ( OwningOrganization != null && OwningOrganization.Id > 0 )
                    return OwningOrganization.Name;
                else
                    return "";
            }
        }
        public int OwningOrganizationId
        {
            get
            {
                if ( OwningOrganization != null && OwningOrganization.Id > 0 )
                    return OwningOrganization.Id;
                else
                    return 0;
            }
        }
        public Enumeration OwnerRoles { get; set; }
        public List<OrganizationRoleProfile> OwnerOrganizationRoles { get; set; }

        //approvals
        public bool IsApproved { get; set; }
        public int ContentApprovedById { get; set; }
        public string ContentApprovedBy { get; set; }
        public string LastApprovalDate { get; set; }
        //public string CreatedByOrganization { get; set; }
        //public int CreatedByOrganizationId { get; set; }

        public int StatusId { get; set; }
        public string ctid { get; set; }
        public string CTID { get { return ctid; } set { ctid = value; } }
        public string CredentialRegistryId { get; set; }
        public string LastPublishDate { get; set; } = "";
        public bool IsPublished { get; set; }
        //See BaseObject
        //public Entity RelatedEntity { get; set; } = new Entity();
        //public DateTime EntityLastUpdated { get; set; }
        public string AssessedBy { get; set; } // url to organization
        //public int InLanguageId { get; set; }
        //public string InLanguage { get; set; }
        //public string InLanguageCode { get; set; }

        public List<int> InLanguageIds { get { return InLanguageCodeList.Select( x => x.LanguageCodeId ).ToList(); } }

        public List<LanguageProfile> InLanguageCodeList { get; set; }
        public List<LanguageProfile> Auto_InLanguageCode
        {
            get
            {
                var result = new List<LanguageProfile>().Concat( InLanguageCodeList ).ToList();
                //if ( !string.IsNullOrWhiteSpace( InLanguageCode ) )
                //{
                    //if ( !result.Exists( x => x.LanguageCodeId == InLanguageId ) )
                    //    result.Add( new LanguageProfile()
                    //    {
                    //        LanguageCodeId = InLanguageId,
                    //        LanguageName = InLanguage,
                    //        LanguageCode = InLanguageCode
                    //    } );
                //}
                return result;
            }
        }

        //public List<LanguageProfile> InLanguageCodeList2 { get; set; } = new List<LanguageProfile>();
        //public List<LanguageProfile> Auto_InLanguageCode2
        //{
        //    get
        //    {
        //        var result = new List<LanguageProfile>().Concat( InLanguageCodeList2 ).ToList();
        //        //if ( !string.IsNullOrWhiteSpace( InLanguageCode ) )
        //        //{
        //        //    result.Add( new LanguageProfile()
        //        //    {
        //        //        LanguageCodeId = InLanguageId,
        //        //        LanguageName = InLanguage,
        //        //        LanguageCode = InLanguageCode
        //        //    } );
        //        //}
        //        return result;
        //    }
        //}

        public Enumeration AudienceType { get; set; } = new Enumeration();

        [Obsolete]
        public Enumeration AssessmentType { get; set; }
        //[Obsolete]
        //public string OtherAssessmentType { get; set; }

        public string CreditHourType { get; set; }
        public decimal CreditHourValue { get; set; }
        public Enumeration CreditUnitType { get; set; } //Used for publishing
        public int CreditUnitTypeId { get; set; }
        public string CreditUnitTypeDescription { get; set; }
        public decimal CreditUnitValue { get; set; }

        //=======================================

        public Enumeration AssessmentUseType { get; set; }
        public Enumeration DeliveryType { get; set; }
        public string DeliveryTypeDescription { get; set; }
        public string VerificationMethodDescription { get; set; }

        public List<OrganizationRoleProfile> OrganizationRole { get; set; }
        //public List<QualityAssuranceActionProfile> QualityAssuranceAction { get; set; }

        //public List<ProcessProfile> AssessmentProcess { get; set; }
        public List<ProcessProfile> AdministrationProcess { get; set; }
        public List<ProcessProfile> DevelopmentProcess { get; set; }
        public List<ProcessProfile> MaintenanceProcess { get; set; }

        public List<CostProfile> EstimatedCost { get; set; }
        public List<CostProfileMerged> EstimatedCost_Merged
        {
            get { return CostProfileMerged.FlattenCosts( EstimatedCost ); }
        } //Used for publishing

        public List<FinancialAlignmentObject> FinancialAssistance { get; set; }

        public List<DurationProfile> EstimatedDuration { get; set; }
        //public List<TextValueProfile> ResourceUrl { get; set; } = new List<TextValueProfile>();
        //public List<TextValueProfile> AssessmentExample { get; set; }
        public string AssessmentExample { get; set; }
        public string AssessmentExampleDescription { get; set; }

        [Obsolete]
        public string AssessmentInformationUrl { get; set; }
        public List<TextValueProfile> Subject { get; set; }
        public List<string> Subjects { get; set; } = new List<string>();
        public List<CredentialAlignmentObject> Auto_Subject { get { return Subject.ConvertAll( m => new CredentialAlignmentObject() { TargetNodeName = m.TextValue } ).ToList(); } }
        public List<TextValueProfile> Keyword { get; set; }

        /***************************************
			need to replace all references to "AssessesCompetencies" 
			with (maybe)
			AssessesCompetenciesFrameworks
			==> not until we are converted to the use of CASS!
		***************************************
		*/
        public List<CredentialAlignmentObjectProfile> AssessesCompetencies { get; set; }

        public int AssessesCompetenciesCount { get; set; }

        public List<CredentialAlignmentObjectFrameworkProfile> AssessesCompetenciesFrameworks { get; set; }
        public List<CredentialAlignmentObjectFrameworkProfile> RequiresCompetenciesFrameworks { get; set; }

        /// <summary>
        /// Alias used for publishing
        /// </summary>
        public List<CredentialAlignmentObjectProfile> TargetCompetency
        {
            get { return TargetCompetencies; }
            set { TargetCompetencies = value; }
        }
        private List<CredentialAlignmentObjectProfile> TargetCompetencies
        {
            get
            {
                return CredentialAlignmentObjectFrameworkProfile.FlattenAlignmentObjects( AssessesCompetenciesFrameworks.ToList() );
            }
            set
            {
                CredentialAlignmentObjectFrameworkProfile.ExpandAlignmentObjects( value, AssessesCompetenciesFrameworks, "assesses" );
                CredentialAlignmentObjectFrameworkProfile.ExpandAlignmentObjects( value, RequiresCompetenciesFrameworks, "requires" );
            }
        }


        public string SubjectWebpage { get; set; }
        public List<TextValueProfile> Auto_SubjectWebpage { get { return string.IsNullOrWhiteSpace( SubjectWebpage ) ? null : new List<TextValueProfile>() { new TextValueProfile() { TextValue = SubjectWebpage } }; } }

        public string AvailableOnlineAt { get; set; }
        public List<TextValueProfile> Auto_AvailableOnlineAt
        {
            get
            {
                var result = new List<TextValueProfile>();
                if ( !string.IsNullOrWhiteSpace( AvailableOnlineAt ) )
                {
                    result.Add( new TextValueProfile() { TextValue = AvailableOnlineAt } );
                }
                return result;
            }
        }
        public string VersionIdentifier { get; set; }
        public List<IdentifierValue> Auto_VersionIdentifier
        {
            get
            {
                var result = new List<IdentifierValue>();
                if ( !string.IsNullOrWhiteSpace( VersionIdentifier ) )
                {
                    result.Add( new IdentifierValue()
                    {
                        IdentifierValueCode = VersionIdentifier
                    } );
                }
                return result;
            }
        }
        public List<GeoCoordinates> AvailableAt
        {
            get
            {
                return Addresses.ConvertAll( m => new GeoCoordinates()
                {
                    Address = m,
                    Latitude = m.Latitude,
                    Longitude = m.Longitude,
                    Name = m.Name
                    //Url = ???
                } ).ToList();
            }
            set
            {
                Addresses = value.ConvertAll( m => new Address()
                {
                    GeoCoordinates = m,
                    Latitude = m.Latitude,
                    Longitude = m.Longitude,
                    Name = m.Name
                    //??? = m.Url
                } ).ToList();
            }
        } //Alias used for publishing

        public string CodedNotation { get; set; }
        public List<TextValueProfile> Auto_CodedNotation
        {
            get
            {
                var result = new List<TextValueProfile>();
                if ( !string.IsNullOrWhiteSpace( CodedNotation ) )
                {
                    result.Add( new TextValueProfile() { TextValue = CodedNotation } );
                }
                return result;
            }
        }

        public List<string> WhereReferenced { get; set; }
        public List<ConditionProfile> IsPartOfConditionProfile { get; set; }
        public List<Credential> IsPartOfCredential { get; set; }
        public List<LearningOpportunityProfile> IsPartOfLearningOpp { get; set; }
        public List<Address> Addresses { get; set; }
        public string AvailabilityListing { get; set; }
        public List<TextValueProfile> Auto_AvailabilityListing
        {
            get
            {
                var result = new List<TextValueProfile>();
                if ( !string.IsNullOrWhiteSpace( AvailabilityListing ) )
                {
                    result.Add( new TextValueProfile() { TextValue = AvailabilityListing } );
                }
                return result;
            }
        }
        public List<CostManifest> CommonCosts { get; set; }
        public List<ConditionManifest> CommonConditions { get; set; }

        public List<ConditionProfile> AllConditions { get; set; } = new List<ConditionProfile>();
        public List<ConditionProfile> Requires { get; set; }
        public List<ConditionProfile> Recommends { get; set; }

        public List<ConditionProfile> AssessmentConnections { get; set; }
        public CredentialConnectionsResult AssessmentConnectionsList { get; set; } = new CredentialConnectionsResult();
        public List<ConditionProfile> PreparationFrom
        {
            get { return ConditionManifestExpanded.DisambiguateConditionProfiles( AssessmentConnections ).PreparationFrom; }
        }
        public List<ConditionProfile> AdvancedStandingFrom
        {
            get { return ConditionManifestExpanded.DisambiguateConditionProfiles( AssessmentConnections ).AdvancedStandingFrom; }
        }
        public List<ConditionProfile> IsRequiredFor { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( AssessmentConnections ).IsRequiredFor; } }
        public List<ConditionProfile> IsRecommendedFor { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( AssessmentConnections ).IsRecommendedFor; } }
        public List<ConditionProfile> IsAdvancedStandingFor { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( AssessmentConnections ).IsAdvancedStandingFor; } }
        public List<ConditionProfile> IsPreparationFor { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( AssessmentConnections ).IsPreparationFor; } }

        /// <summary>
        /// The resource being referenced must be pursued concurrently with the resource being described.
        /// </summary>
        public List<ConditionProfile> Corequisite { get; set; }

        /// <summary>
        /// The prerequisites for entry into the resource being described.
        /// Comment:
        /// Such requirements might include transcripts, previous experience, lower-level learning opportunities, etc.
        /// </summary>
        public List<ConditionProfile> EntryCondition { get; set; }


        public AgentRelationshipResult AgentAndRoles { get; set; } = new AgentRelationshipResult();

		#region Frameworks
		public Enumeration Occupation { get; set; } = new Enumeration();
		public List<TextValueProfile> AlternativeOccupations { get; set; }
		public CodeItemResult OccupationResults { get; set; } = new CodeItemResult();
		public CodeItemResult OccupationOtherResults { get; set; } = new CodeItemResult();
		public Enumeration Industry { get; set; } = new Enumeration();
		public List<TextValueProfile> AlternativeIndustries { get; set; }
		public CodeItemResult IndustryResults { get; set; } = new CodeItemResult();
		public CodeItemResult IndustryOtherResults { get; set; } = new CodeItemResult();

		//this name is used in Models.Node.Assessment, keep in sync
		public Enumeration InstructionalProgramType { get; set; }
        public CodeItemResult InstructionalProgramResults { get; set; } = new CodeItemResult();
		public CodeItemResult OtherInstructionalProgramResults { get; set; } = new CodeItemResult();
		public List<TextValueProfile> AlternativeInstructionalProgramType { get; set; }
		#endregion	
		//===========================================
		public Enumeration AssessmentMethodType { get; set; }
        public string AssessmentOutput { get; set; }
        public string ExternalResearch { get; set; }
        public List<TextValueProfile> Auto_ExternalResearch
        {
            get
            {
                var result = new List<TextValueProfile>();
                if ( !string.IsNullOrWhiteSpace( ExternalResearch ) )
                {
                    result.Add( new TextValueProfile() { TextValue = ExternalResearch } );
                }
                return result;
            }
        }
        public bool? HasGroupEvaluation { get; set; }
        public bool? HasGroupParticipation { get; set; }
        public bool? IsProctored { get; set; }

        public string ProcessStandards { get; set; }
        public string ProcessStandardsDescription { get; set; }


        public Enumeration ScoringMethodType { get; set; }
        public string ScoringMethodDescription { get; set; }
        public string ScoringMethodExample { get; set; }
        public string ScoringMethodExampleDescription { get; set; }

        public List<JurisdictionProfile> Region { get; set; }
        public List<JurisdictionProfile> JurisdictionAssertions { get; set; }

        public CodeItemResult AssessmentMethodTypes { get; set; } = new CodeItemResult();
        public CodeItemResult AssessmentUseTypes { get; set; } = new CodeItemResult();
        public CodeItemResult ScoringMethodTypes { get; set; } = new CodeItemResult();
        public CodeItemResult DeliveryMethodTypes { get; set; } = new CodeItemResult();
        public AgentRelationshipResult QualityAssurance { get; set; } = new AgentRelationshipResult();
        #region helpers, for search, etc
        public List<string> CommonCostsList { get; set; } = new List<string>();
        public List<string> CommonConditionsList { get; set; } = new List<string>();
        #endregion
    }

}
