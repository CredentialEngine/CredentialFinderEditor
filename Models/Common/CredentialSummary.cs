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
			NaicsResults = new CodeItemResult();
			IndustryOtherResults = new CodeItemResult();
			OccupationResults = new CodeItemResult();
			OccupationOtherResults = new CodeItemResult();
			LevelsResults = new CodeItemResult();
			QARolesResults = new CodeItemResult();
			ConnectionsList = new CodeItemResult();
			CredentialsList = new CredentialConnectionsResult();
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
		
		public string Url { get; set; }
		public string ImageUrl { get; set; }

		public string CredentialType { get; set; }
		public string CredentialTypeSchema { get; set; }
		public string CTID { get; set; }
		public decimal TotalCost { get; set; }
		public string CredentialRegistryId { get; set; }
		public int ManagingOrgId { get; set; }
		public string ManagingOrganization { get; set; }

		//public int CreatorOrganizationId { get; set; }
		//public string CreatorOrganizationName { get; set; }
		public int OwnerOrganizationId { get; set; }
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
		//public List<CodeItem> CreatingOrgs { get; set; }
		//public List<CodeItem> OwningOrgs { get; set; }
		public int LearningOppsCompetenciesCount { get; set; }
		public int AssessmentsCompetenciesCount { get; set; }
		public int QARolesCount { get; set; }

		//credential connections
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

		public CodeItemResult NaicsResults { get; set; }
		public CodeItemResult IndustryOtherResults { get; set; }
		public CodeItemResult OccupationResults { get; set; }
		public CodeItemResult OccupationOtherResults { get; set; }
		public CodeItemResult LevelsResults { get; set; }

		public CodeItemResult QARolesResults { get; set; }
		public CodeItemResult QAOrgRolesResults { get; set; }
		public AgentRelationshipResult AgentAndRoles { get; set; }
		
		public CodeItemResult ConnectionsList { get; set; }
		public CredentialConnectionsResult CredentialsList { get; set; }

		public List<Address> Addresses { get; set; }
		public bool IsAQACredential { get; set; }
		public bool HasQualityAssurance { get; set; }

		public List<DurationProfile> EstimatedTimeToEarn { get; set; }

		public int NumberOfCostProfileItems { get; set; }
		public List<CostProfile> EstimatedCost { get; set; }
		public bool HasVerificationType_Badge { get; set; }
		
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
