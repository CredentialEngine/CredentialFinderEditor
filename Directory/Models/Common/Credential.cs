using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.ProfileModels;

namespace Models.Common
{
	public class Credential : BaseObject
	{
		public Credential()
		{
			Id = 0;
			Name = "";
			StatusId = 1;
			Requires = new List<ConditionProfile>();
			Recommends = new List<ConditionProfile>();
			Renewal = new List<ConditionProfile>();
			IsRequiredFor = new List<ConditionProfile>();
			IsRecommendedFor = new List<ConditionProfile>();
			Addresses = new List<Address>();
			Region = new List<GeoCoordinates>();
			Jurisdiction = new List<JurisdictionProfile>();
			EstimatedTimeToEarn = new List<DurationProfile>();
			Purpose = new Enumeration();
			CredentialType = new Enumeration();
			CredentialLevel = new Enumeration();
			EmbeddedCredentials = new List<Credential>();
			IsPartOf = new List<Credential>();
			HasPartIds = new List<int>();
			IsPartOfIds = new List<int>();
			CredentialProcess = new List<ProcessProfile>();
			Earnings = new List<EarningsProfile>();
			EmploymentOutcome = new List<EmploymentOutcomeProfile>();
			Holders = new List<HoldersProfile>();
			Industry = new Enumeration();
			Occupation = new Enumeration();
			MilitaryOccupation = new Enumeration();
			Revocation = new List<RevocationProfile>();
			OrganizationRole = new List<OrganizationRoleProfile>();
			//OrganizationRole2 = new List<OrganizationRoleProfile>();
			QualityAssuranceAction = new List<QualityAssuranceActionProfile>();
			CredentialAgentRelationships = new List<CredentialAgentRelationship>();
			Keywords = new List<TextValueProfile>();
			Subjects = new List<TextValueProfile>();
			EstimatedCosts = new List<CostProfile>();
			RequiresCompetencies = new List<CredentialAlignmentObjectProfile>();

		}
		public string Name { get; set; }
		public string AlternateName { get; set; }
		public string Description { get; set; }
		public string Version { get; set; }
		public int StatusId { get; set; }
		public string ctid { get; set; }
		public string CredentialRegistryId { get; set; }
		
		public string LatestVersionUrl { get; set; }
		public string ReplacesVersionUrl { get; set; }
		public string Url { get; set; }
		public string AvailableOnlineAt { get; set; }
		public string ImageUrl { get; set; } //image URL
		public List<Address> Addresses { get; set; }
		public List<GeoCoordinates> Region { get; set; } //Soon(TM) to be replaced
		public List<JurisdictionProfile> Jurisdiction { get; set; }

		public List<DurationProfile> EstimatedTimeToEarn { get; set; }
		public Enumeration Purpose { get; set; }
		public Enumeration CredentialType { get; set; }
		public Enumeration CredentialLevel { get; set; }
		public List<Credential> EmbeddedCredentials { get; set; } //bundled/sub-credentials
		public List<Credential> IsPartOf { get; set; } //pseudo-"parent" credentials that this credential is a part of or included with (could be multiple)
		public List<int> HasPartIds { get; set; }
		public List<int> IsPartOfIds { get; set; }

		public List<ProcessProfile> CredentialProcess { get; set; }
		public List<EarningsProfile> Earnings { get; set; }
		public List<EmploymentOutcomeProfile> EmploymentOutcome { get; set; }
		public List<HoldersProfile> Holders { get; set; }
		public Enumeration Industry { get; set; }
		public List<TextValueProfile> OtherIndustries { get; set; }
		public Enumeration Occupation { get; set; }
		public List<TextValueProfile> OtherOccupations { get; set; }
		public Enumeration MilitaryOccupation { get; set; }

		public List<OrganizationRoleProfile> OrganizationRole { get; set; }
		//public List<OrganizationRoleProfile> OrganizationRole2 { get; set; }
		public List<QualityAssuranceActionProfile> QualityAssuranceAction { get; set; }

		public List<ConditionProfile> Requires { get; set; }
		public List<ConditionProfile> Recommends { get; set; }
		public List<ConditionProfile> Renewal { get; set; }

		//next steps
		public List<ConditionProfile> IsRequiredFor { get; set; }
		public List<ConditionProfile> IsRecommendedFor { get; set; }

		public List<RevocationProfile> Revocation { get; set; }
		//public int CreatorOrganizationId { get; set; }
		//public int OwnerOrganizationId { get; set; }
		public int ManagingOrgId { get; set; }
		public Organization CreatorOrganization
		{
			get
			{
				try
				{
					return CredentialAgentRelationships.FirstOrDefault( m => m.RelationshipId == 5 ).TargetOrganization;
				}
				catch
				{
					return new Organization();
				}
			}
		}
		public List<CredentialAgentRelationship> CredentialAgentRelationships { get; set; }
		public List<TextValueProfile> Keywords { get; set; }
		public List<TextValueProfile> Subjects { get; set; }
		public List<CostProfile> EstimatedCosts { get; set; }
		public List<CredentialAlignmentObjectProfile> RequiresCompetencies { get; set; }
		public void InitializeConnectionProfiles()
		{
			Requires = new List<ConditionProfile>();
			Recommends = new List<ConditionProfile>();
			Renewal = new List<ConditionProfile>();
			IsRequiredFor = new List<ConditionProfile>();
			IsRecommendedFor = new List<ConditionProfile>();
		}
	}
}
