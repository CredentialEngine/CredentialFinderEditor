using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.ProfileModels
{

	//Note - referred to as AgentRoleProfile in the spreadsheet
	public class OrganizationRoleProfile : BaseProfile
	{
		public OrganizationRoleProfile()
		{
			AgentRole = new Enumeration();
			TargetOrganization = new Organization();
		}
		public Guid ParentUid { get; set; }
		public Guid ActedUponEntityUid
		{
			get { return ParentUid; }
			set { ParentUid = value; }
		}
		public int ParentTypeId { get; set; }
		public string ParentType { get; set; }
		//TargetCredentialId is the parentId in credential to org roles
		public int TargetCredentialId { get; set; }

		public Organization ActingAgent { get; set; }
		public int ActingAgentId { get; set; }
		public Guid ActingAgentUid { get; set; }

		public Guid ReverseAgentUid
		{
			get { return ActingAgentUid; }
			set { ActingAgentUid = value; }
		}
		public bool IsQAActionRole { get; set; }
		public Enumeration AgentRole { get; set; }
		//public Enumeration RoleType 
		//{ 
		//	get { return AgentRole; }
		//	set { AgentRole = value; } 
		//}
		public int RoleTypeId { get; set; }
		public string AllRoleIds { get; set; }
		public string AllRoles { get; set; } 
		public bool IsInverseRole { get; set; }

		/// <summary>
		/// If referenced, indicates that the TargetOrganizationId is the parent in the action
		/// </summary>
		public int TargetOrganizationId { get; set; }
		public Organization TargetOrganization { get; set; }

		public string Url { get; set; } // url

		public string SchemaTag { get; set; }
		public string ReverseSchemaTag { get; set; } 

		/// <summary>
		/// If referenced, indicates that the TargetAssessment is the parent in the action
		/// </summary>
		public AssessmentProfile TargetAssessment { get; set; }
		public int TargetAssessmentId { get; set; }
		public int TargetLearningOpportunityId { get; set; }
		public string TargetCompetency { get; set; } // url
		public string TargetCompetencyFramework { get; set; } // url

		//Used for publishing
		public object TargetDetermined
		{
			get
			{
				if ( TargetCredentialId > 0 ) { return new Credential() { Id = TargetCredentialId }; }
				else if ( TargetOrganizationId > 0 ) { return new Organization() { Id = TargetOrganizationId }; }
				else if ( TargetAssessmentId > 0 ) { return new AssessmentProfile() { Id = TargetAssessmentId }; }
				else if ( TargetLearningOpportunityId > 0 ) { return new LearningOpportunityProfile() { Id = TargetLearningOpportunityId }; }
				else { return null; }
		} }
		public object TargetOverride { get; set; }
	}
	//

}
