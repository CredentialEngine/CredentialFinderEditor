using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.ProfileModels;

namespace Models.Common
{
	public class CredentialSummary : BaseObject
	{
		public CredentialSummary()
		{
			//NaicsList = new List<CodeItem>();
			//LevelsList = new List<CodeItem>();
			//OccupationsList = new List<CodeItem>();
			IndustryResults = new CodeItemResult();
			IndustryOtherResults = new CodeItemResult();
			OccupationResults = new CodeItemResult();
			OccupationOtherResults = new CodeItemResult();
			LevelsResults = new CodeItemResult();
			QARolesResults = new CodeItemResult();
			Org_QAAgentAndRoles = new AgentRelationshipResult();
			AgentAndRoles = new AgentRelationshipResult();
			ConnectionsList = new CodeItemResult();
			CredentialsList = new CredentialConnectionsResult();
			HasPartsList = new CredentialConnectionsResult();
			IsPartOfList = new CredentialConnectionsResult();
			Addresses = new List<Address>();
			EstimatedTimeToEarn = new List<DurationProfile>();
			EstimatedCost = new List<CostProfile>();
			Subjects = new List<string>();
		//	CreatingOrgs = new List<CodeItem>();
		//	OwningOrgs = new List<CodeItem>();
		}
		public string Name { get; set; }
		public string FriendlyName { get; set; }
		public int StatusId { get; set; }
		public string ListTitle { get; set; }
		//public string effectiveDate { get; set; }
		public string Description { get; set; }
		public string Version { get; set; }
		public string LatestVersionUrl { get; set; }
		public string PreviousVersion { get; set; }
		public string AvailableOnlineAt { get; set; }
		
		public string SubjectWebpage { get; set; }
		public string ImageUrl { get; set; }

		public string CredentialType { get; set; }
		public string CredentialTypeSchema { get; set; }
		public string CTID { get; set; }
		public decimal TotalCost { get; set; }
		public string CredentialRegistryId { get; set; }
        public string LastPublishDate { get; set; } = "";
        public bool IsPublished{ get; set; }
        //public DateTime EntityLastUpdated { get; set; }
        public int ManagingOrgId { get; set; }
		public string ManagingOrganization { get; set; }

		//public int CreatorOrganizationId { get; set; }
		//public string CreatorOrganizationName { get; set; }
		public int OwnerOrganizationId { get; set; }
        public string OwningAgentUid { get; set; }

        public string OwnerOrganizationName { get; set; }
		public string OrganizationName
		{
			get
			{
				if ( OwnerOrganizationName != null  )
					return OwnerOrganizationName;
				else
					return "";
			}
		}

		//approvals
		public bool IsApproved { get; set; }
		public int ContentApprovedById { get; set; }
		public string ContentApprovedBy { get; set; }
        public string LastApprovalDate { get; set; } = "";

		//public List<CodeItem> CreatingOrgs { get; set; }
		//public List<CodeItem> OwningOrgs { get; set; }
		public int LearningOppsCompetenciesCount { get; set; }
		public int AssessmentsCompetenciesCount { get; set; }
		public int QARolesCount { get; set; }

		//credential connections
		public CredentialConnectionsResult HasPartsList { get; set; }
		public CredentialConnectionsResult IsPartOfList { get; set; }
		
		public int HasPartCount { get; set; }
		public int IsPartOfCount { get; set; }
		public int RequiresCount { get; set; }
		public int RecommendsCount { get; set; }
		public int RequiredForCount { get; set; }
		public int IsRecommendedForCount { get; set; }
		public int RenewalCount { get; set; }
		public int IsAdvancedStandingForCount { get; set; }
		public int AdvancedStandingFromCount { get; set; }
		public int PreparationForCount { get; set; }
		public int PreparationFromCount { get; set; }
		public List<string> Subjects { get; set; }

		//public List<CodeItem> NaicsList { get; set; }
		//public string NaicsList2 { get; set; }
		//public List<CodeItem> OccupationsList { get; set; }
		//public List<CodeItem> LevelsList { get; set; }

		public CodeItemResult IndustryResults { get; set; }
		public CodeItemResult IndustryOtherResults { get; set; }
		public CodeItemResult OccupationResults { get; set; }
		public CodeItemResult OccupationOtherResults { get; set; }
		public int InstructionalProgramCounts { get; set; }
		public int OtherInstructionalProgramCounts { get; set; }
		public CodeItemResult LevelsResults { get; set; }
		public CodeItemResult AssessmentDeliveryType { get; set; } = new CodeItemResult();
		public CodeItemResult LearningDeliveryType { get; set; } = new CodeItemResult();
		public CodeItemResult QARolesResults { get; set; }
        public AgentRelationshipResult AgentAndRoles { get; set; }

        public CodeItemResult Org_QARolesResults { get; set; } = new CodeItemResult();
        public AgentRelationshipResult Org_QAAgentAndRoles { get; set; } = new AgentRelationshipResult();

        //NOTE: ASSIGNED BUT NOT USED ANYWHERE!		
        public CodeItemResult ConnectionsList { get; set; }
        public CredentialConnectionsResult CredentialsList { get; set; }

		public List<Address> Addresses { get; set; }
		
		public bool IsAQACredential { get; set; }
		public bool HasQualityAssurance { get; set; }

		public List<DurationProfile> EstimatedTimeToEarn { get; set; }

		public int NumberOfCostProfileItems { get; set; }
		public List<CostProfile> EstimatedCost { get; set; }
		public bool HasVerificationType_Badge { get; set; }

        public List<string> TargetAssessmentsList { get; set; } = new List<string>();
        public List<string> TargetLearningOppsList { get; set; } = new List<string>();
        public List<string> TargetCredentialsList { get; set; } = new List<string>();
        public List<string> CommonCostsList { get; set; } = new List<string>();
        public List<string> CommonConditionsList { get; set; } = new List<string>();

    }
	public class CodeItemResult
	{
		public CodeItemResult()
		{
			HasAnIdentifer = true;
			Results = new List<CodeItem>();
		}
		public int CategoryId { get; set; }
		public bool HasAnIdentifer { get; set; }

		public List<CodeItem> Results { get; set; }
	}
	public class CredentialConnectionsResult
	{
		public CredentialConnectionsResult()
		{
			Results = new List<CredentialConnectionItem>();
		}
		public int CategoryId { get; set; }

		public List<CredentialConnectionItem> Results { get; set; }
	}
	public class CredentialConnectionItem
	{
		public int ConnectionId { get; set; }

		public string Connection { get; set; }

		public int CredentialId { get; set; }

		public string Credential { get; set; }
        public string CredentialOwningOrg { get; set; }
        public int CredentialOwningOrgId { get; set; }

    }
	public class AgentRelationshipResult
	{
		public AgentRelationshipResult()
		{
			HasAnIdentifer = true;
			Results = new List<AgentRelationship>();
		}
		public int CategoryId { get; set; }
		public bool HasAnIdentifer { get; set; }

		public List<AgentRelationship> Results { get; set; }
	} 
}
