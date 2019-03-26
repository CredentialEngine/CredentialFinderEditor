using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.ProfileModels;

namespace Models.Node
{
	[Profile( DBType = typeof( Models.ProfileModels.AssessmentProfile ) )]
	public class Assessment : BaseMainProfile
	{
		public Assessment()
		{
			AssessmentMethodType = new List<int>();
			AssessmentUseType = new List<int>();
			DeliveryType = new List<int>();
			ScoringMethodType = new List<int>();

			Subject = new List<TextValueProfile>();
			Keyword = new List<TextValueProfile>();
			JurisdictionAssertions = new List<ProfileLink>();
			CommonCosts = new List<ProfileLink>();
			CommonConditions = new List<ProfileLink>();
			Requires = new List<ProfileLink>();
			Recommends = new List<ProfileLink>();
			Corequisite = new List<ProfileLink>();
			EntryCondition = new List<ProfileLink>();
			AssessmentConnections = new List<ProfileLink>();

			//AssessmentProcess = new List<ProfileLink>();
			AdministrationProcess = new List<ProfileLink>();
			DevelopmentProcess = new List<ProfileLink>();
			MaintenanceProcess = new List<ProfileLink>();

			Cost = new List<ProfileLink>();
			FinancialAssistance = new List<ProfileLink>();

			RequiresCompetenciesFrameworks = new List<ProfileLink>();

			//to be deleted
			AssessmentExamples = new List<TextValueProfile>();
			IsPartOfCredential = new List<ProfileLink>();

		}

		//Basic Info

		//List-based Info
	
		[Property( DBName = "AssessmentMethodType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> AssessmentMethodType { get; set; }
		
		[Property( DBName = "AssessmentUseType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> AssessmentUseType { get; set; }

		//SubjectWebpage was added to BaseMainProfile.
		//public string SubjectWebpage { get; set; }

		[Property( DBName = "DeliveryType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> DeliveryType { get; set; }
		public string DeliveryTypeDescription { get; set; }
		public string VerificationMethodDescription { get; set; }

        [Property(DBName = "AudienceType", DBType = typeof(Models.Common.Enumeration))]
        public List<int> AudienceType { get; set; }

        public int ManagingOrgId { get; set; }

		[Property( DBName = "OwningOrganization", DBType = typeof( Models.Common.Organization ) )]
		public ProfileLink DisplayOwningOrganization { get; set; }

		[Property( DBName = "OwningAgentUid", DBType = typeof( Guid ) )]
		public ProfileLink OwningOrganization { get; set; }

		/// <summary>
		/// OwnerRoles are used only for add
		/// </summary>
		[Property( DBName = "OwnerRoles", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> RoleTypeIds { get; set; }

		[Property( Type = typeof( AgentRoleProfile_Recipient ), DBName = "OwnerOrganizationRoles" )]
		public List<ProfileLink> OwnerOrganizationRoles { get; set; }
		[Property( Type = typeof( AgentRoleProfile_Recipient ), DBName = "OfferedByOrganizationRole" )]
		public List<ProfileLink> OfferedByOrganizationRole { get; set; }
		public List<TextValueProfile> Subject { get; set; }
		public List<TextValueProfile> Keyword { get; set; }


		public string CodedNotation { get; set; }
		public string CredentialRegistryId { get; set; }
		public string CTID { get; set; }
		public string VersionIdentifier { get; set; }
		public List<Models.Common.IdentifierValue> Auto_VersionIdentifier
		{
			get
			{
				var result = new List<Models.Common.IdentifierValue>();
				if ( !string.IsNullOrWhiteSpace( VersionIdentifier ) )
				{
					result.Add( new Models.Common.IdentifierValue()
					{
						IdentifierValueCode = VersionIdentifier
					} );
				}
				return result;
			}
		}
		public string AvailableOnlineAt { get; set; }
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
        public string CreditHourType { get; set; }
		public decimal CreditHourValue { get; set; }

		[Property( DBName = "CreditUnitTypeId" )]
		public int CreditUnitType { get; set; }
		public string CreditUnitTypeDescription { get; set; }
		public decimal CreditUnitValue { get; set; }


		[Property( DBName = "ResourceUrl" )]
		public List<TextValueProfile> ResourceUrl { get; set; }
		

		public string AssessmentExample { get; set; }
		public string AssessmentExampleDescription { get; set; }

		
		public string AvailabilityListing { get; set; }

		
		
		//Profiles
		[Property( DBName = "EstimatedCost" )]
		public List<ProfileLink> Cost { get; set; }

		[Property( Type = typeof( FinancialAlignmentObject ) )]
		public List<ProfileLink> FinancialAssistance { get; set; }

		[Property( DBName = "EstimatedDuration" )]
		public List<ProfileLink> Duration { get; set; }

		[Property( DBName = "RequiresCompetenciesFrameworks", DBType = typeof( Models.Common.CredentialAlignmentObjectFrameworkProfile ) )]
		public List<ProfileLink> RequiresCompetenciesFrameworks { get; set; }

		[Property( DBName = "AssessesCompetenciesFrameworks", DBType = typeof( Models.Common.CredentialAlignmentObjectFrameworkProfile ) )]
		public List<ProfileLink> AssessesCompetenciesFrameworks { get; set; }

		[Property( DBName = "Addresses", DBType = typeof( Models.Common.Address ) )]
		public List<ProfileLink> Addresses { get; set; }

		[Property( Type = typeof( MicroProfile ), DBType = typeof( Models.Common.Enumeration ) )]
		public List<ProfileLink> Occupation { get; set; }

		[Property( Type = typeof( MicroProfile ), DBType = typeof( Models.Common.Enumeration ) )]
		public List<ProfileLink> Industry { get; set; }
		public List<TextValueProfile> AlternativeIndustries { get; set; } = new List<TextValueProfile>();
		public List<TextValueProfile> AlternativeOccupations { get; set; } = new List<TextValueProfile>();

		[Property( DBName = "InstructionalProgramType", Type = typeof( MicroProfile ), DBType = typeof( Models.Common.Enumeration ) )]
		public List<ProfileLink> CipCode { get; set; }

		public List<TextValueProfile> AlternativeInstructionalProgramType { get; set; }

		[Property( Type = typeof( Credential ) )]
		public List<ProfileLink> IsPartOfCredential { get; set; }

		public string AssessmentOutput { get; set; }
		public string ExternalResearch { get; set; }
		public bool? HasGroupEvaluation { get; set; }
		public bool? HasGroupParticipation { get; set; }
		public bool? IsProctored { get; set; }

		public string ProcessStandards { get; set; }
		public string ProcessStandardsDescription { get; set; }


		[Property( DBName = "ScoringMethodType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> ScoringMethodType { get; set; }
		public string ScoringMethodDescription { get; set; }
		public string ScoringMethodExample { get; set; }
		public string ScoringMethodExampleDescription { get; set; }


		[Property( Type = typeof( JurisdictionProfile ) )]
		public List<ProfileLink> JurisdictionAssertions { get; set; }

		[Property( Type = typeof( ConditionManifest ) )]
		public List<ProfileLink> CommonCosts { get; set; }
		[Property( Type = typeof( ConditionManifest ) )]
		public List<ProfileLink> CommonConditions { get; set; }

		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> Requires { get; set; }

		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> Recommends { get; set; }


		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> Corequisite { get; set; }

		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> EntryCondition { get; set; }

		[Property( Type = typeof( ProcessProfile ) )]
		public List<ProfileLink> AdministrationProcess { get; set; }

		[Property( Type = typeof( ProcessProfile ) )]
		public List<ProfileLink> DevelopmentProcess { get; set; }

		[Property( Type = typeof( ProcessProfile ) )]
		public List<ProfileLink> MaintenanceProcess { get; set; }

		#region TO BE DELETED  =========================================

		//Text Value Info
		//public List<TextValueProfile> VersionIdentifier { get; set; }

		public List<TextValueProfile> AssessmentExamples { get; set; }
		public string AssessmentInformationUrl { get; set; }
		

		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> AssessmentConnections { get; set; }

		#endregion

	}
	public class TargetAssessmentProfile : Assessment { }
	//
}
