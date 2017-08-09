using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
namespace Models.Elastic
{
	//[ElasticType(IdProperty = "Id", Name = "credential")]
	public class Credential : ElasticBaseObject
	{
		public Credential()
		{
			Purpose = new Enumeration();
			CredentialType = new Enumeration();
			CredentialLevel = new Enumeration();
			Requires = new List<ConditionProfile>();
			Addresses = new List<Address>();
			Subjects = new List<string>();
			Keywords = new List<string>();

			Region = new List<GeoCoordinates>();
			Jurisdiction = new List<JurisdictionProfile>();
			EstimatedTimeToEarn = new List<DurationProfile>();
			EstimatedCosts = new List<CostProfile>();
			Industry = new Enumeration();
			Occupation = new Enumeration();
		}

		public string AlternateName { get; set; }
		public string Version { get; set; }
		public Enumeration Purpose { get; set; }
		public Enumeration CredentialType { get; set; }
		public Enumeration CredentialLevel { get; set; }
		public string AvailableOnlineAt { get; set; }
		public string LatestVersionUrl { get; set; }
		public string PreviousVersion { get; set; }
		public List<string> Subjects { get; set; }
		public List<string> Keywords { get; set; }

		public List<Address> Addresses { get; set; }
		public List<GeoCoordinates> Region { get; set; } //Soon(TM) to be replaced
		public List<JurisdictionProfile> Jurisdiction { get; set; }

		public List<DurationProfile> EstimatedTimeToEarn { get; set; }

		public List<Credential> IsPartOf { get; set; }

		public Enumeration Industry { get; set; }
		public List<TextValueProfile> OtherIndustries { get; set; }
		public Enumeration Occupation { get; set; }
		public List<TextValueProfile> OtherOccupations { get; set; }

		public List<OrganizationRoleProfile> OrganizationRole { get; set; }

		public List<ConditionProfile> Requires { get; set; }

		public List<CostProfile> EstimatedCosts { get; set; }

		//handle efficiently
		//public List<Credential> EmbeddedCredentials { get; set; }

		public bool HasCompetencies { get; set; }
		public bool ChildHasCompetencies { get; set; }
	}
}
