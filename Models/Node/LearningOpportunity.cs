using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.Node
{
	[Profile( DBType = typeof( Models.ProfileModels.LearningOpportunityProfile ) )]
	public class LearningOpportunity : BaseMainProfile
	{
		public LearningOpportunity()
		{
			DeliveryType = new List<int>();
			Subject = new List<TextValueProfile>();
			Keyword = new List<TextValueProfile>();

			//delete me
			ResourceUrls = new List<TextValueProfile>();
			EmbeddedAssessment = new List<ProfileLink>();
			CommonCosts = new List<ProfileLink>();
			CommonConditions = new List<ProfileLink>();
			Requires = new List<ProfileLink>();
			Recommends = new List<ProfileLink>();
			Corequisite = new List<ProfileLink>();
			EntryCondition = new List<ProfileLink>();
			LearningOppConnections = new List<ProfileLink>();
			LearningCompetencies = new List<TextValueProfile>();
			IdentificationCode = new List<TextValueProfile>();
			LearningOpportunityProcess = new List<ProfileLink>();
			JurisdictionAssertions = new List<ProfileLink>();
			IsPartOfCredential = new List<ProfileLink>();

			Cost = new List<ProfileLink>();
			FinancialAssistance = new List<ProfileLink>();
		}
		

		//Basic Info

		//List-based Info
		[ Property( DBName = "DeliveryType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> DeliveryType { get; set; }
		public string DeliveryTypeDescription { get; set; }
		public string VerificationMethodDescription { get; set; }
		//SubjectWebpage was added to BaseMainProfile.
		//public string SubjectWebpage { get; set; }
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

		public string CodedNotation { get; set; }
		public string CredentialRegistryId { get; set; }
		public string CTID { get; set; }
		public string AvailableOnlineAt { get; set; }
		public string AvailabilityListing { get; set; }
		public int InLanguageId { get; set; }
		public string CreditHourType { get; set; }
		public decimal CreditHourValue { get; set; }

		[Property( DBName = "CreditUnitTypeId" )]
		public int CreditUnitType { get; set; }
		public string CreditUnitTypeDescription { get; set; }
		public decimal CreditUnitValue { get; set; }

	
		[Property( DBName = "InstructionalProgramCategory", Type = typeof( MicroProfile ), DBType = typeof( Models.Common.Enumeration ) )]
		public List<ProfileLink> CipCode { get; set; }

		public List<TextValueProfile> OtherInstructionalProgramCategory { get; set; }


		//Profiles
		//[Property( DBName = "LearningResourceUrls" )]
		//public List<TextValueProfile> LearningResourceUrls { get; set; }

		public List<TextValueProfile> Subject { get; set; }
		public List<TextValueProfile> Keyword { get; set; }

		[Property( DBName = "EstimatedCost" )]
		public List<ProfileLink> Cost { get; set; }

		[Property( Type = typeof( FinancialAlignmentObject ) )]
		public List<ProfileLink> FinancialAssistance { get; set; }

		[Property( DBName = "EstimatedDuration" )]
		public List<ProfileLink> Duration { get; set; }
		[Property( DBName = "HasPart" )]
		public List<ProfileLink> EmbeddedLearningOpportunity { get; set; }
		[Property( DBName = "IsPartOf" )]
		public List<ProfileLink> ParentLearningOpportunity { get; set; }

		[Property( Type = typeof( Credential ) )]
		public List<ProfileLink> IsPartOfCredential { get; set; }

		[Property( DBName = "Addresses", DBType = typeof( Models.Common.Address ) )]
		public List<ProfileLink> Addresses { get; set; }

	

		[Property( DBName = "RequiresCompetenciesFrameworks", DBType = typeof( Models.Common.CredentialAlignmentObjectFrameworkProfile ) )]
		public List<ProfileLink> RequiresCompetenciesFrameworks { get; set; }

		[Property( DBName = "TeachesCompetenciesFrameworks", DBType = typeof( Models.Common.CredentialAlignmentObjectFrameworkProfile ) )]
		public List<ProfileLink> TeachesCompetenciesFrameworks { get; set; }


		[Property( DBName = "LearningMethodType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> LearningMethodType { get; set; }

		//Profile Info
		[Property( Type = typeof( JurisdictionProfile ) )]
		public List<ProfileLink> Region { get; set; }

		[Property( Type = typeof( JurisdictionProfile ) )]
		public List<ProfileLink> JurisdictionAssertions { get; set; }

		#region TO BE DELETED
		[Property( DBName = "ResourceUrls" )]
		public List<TextValueProfile> ResourceUrls { get; set; }

		public List<ProfileLink> EmbeddedAssessment { get; set; }
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


		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> LearningOppConnections { get; set; }

		[Property( DBName = "LearningCompetencies" )]
		public List<TextValueProfile> LearningCompetencies { get; set; }

		//Text Value Info
		public List<TextValueProfile> IdentificationCode { get; set; }

		[Property( Type = typeof( ProcessProfile ) )]
		public List<ProfileLink> LearningOpportunityProcess { get; set; }
		#endregion

	}

	public class TargetLearningOpportunity : LearningOpportunity { }
	//
}
