using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.ProfileModels
{
	public class CredentialAgentRelationship : BaseProfile
	{

		public CredentialAgentRelationship()
		{
			TargetOrganization = new Organization();
			Relationship = "";
			ProfileSummary = "";
			Description = "";
			URL = "";
		}
		
		public int CredentialId { get; set; }
		public int OrganizationId { get; set; }
		public Organization TargetOrganization { get; set; } 
		public Guid AgentUid { get; set; }
		public int RelationshipId { get; set; }

		public string Relationship { get; set; } 
		//public string ProfileSummary { get; set; } 
		public string URL { get; set; }

		public int TargetCredentialId { get; set; }
		public System.DateTime EffectiveDate { get; set; }
		public DateTime StartDate
		{
			get { return EffectiveDate; }
			set { EffectiveDate = value; }
		}
		public System.DateTime EndDate { get; set; }
		public bool IsQAActionRole { get; set; }
		public string Description { get; set; }
		

		
		
	}
}
