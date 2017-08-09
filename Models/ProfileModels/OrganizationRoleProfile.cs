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
			TargetCredential = new Credential();
			TargetAssessment = new AssessmentProfile();
			TargetLearningOpportunity = new LearningOpportunityProfile();
		}

		//parent had been an entity like credential. this may now be the context, and 
		//will use ActedUponEntityUid separately as the target entity
		public Guid ParentUid { get; set; }

		public Guid ActedUponEntityUid { get; set; }
		public Entity ActedUponEntity { get; set; }
		public int ActedUponEntityId { get; set; }

		public int ParentTypeId { get; set; }
		public string ParentType { get; set; }
		public string ParentName { get; set; }
		

		public Organization ActingAgent { get; set; }
		public int ActingAgentId { get; set; }
		public Guid ActingAgentUid { get; set; }

		//????how is participant different from acting
		public Guid ParticipantAgentUid { get; set; }
		public Organization ParticipantAgent { get; set; }

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

		[Obsolete]
		public string Url { get; set; } // url

		//public string SchemaTag { get; set; }
		//public string ReverseSchemaTag { get; set; }

		#region === Targets - where acted upon by the agent ======================

		/// <summary>
		/// TargetCredentialId is the parentId in credential to org roles
		/// The credential acted upon by the agent
		/// </summary>
		public Credential TargetCredential { get; set; }
		public int TargetCredentialId { get; set; }

		/// <summary>
		/// If referenced, indicates that the TargetOrganizationId is the parent in the action - again acted upon by the agent
		/// </summary>
		public int TargetOrganizationId { get; set; }
		public Organization TargetOrganization { get; set; }


		/// <summary>
		/// If referenced, indicates that the TargetAssessment is the parent in the action
		/// </summary>
		public AssessmentProfile TargetAssessment { get; set; }
		public int TargetAssessmentId { get; set; }

		public LearningOpportunityProfile TargetLearningOpportunity { get; set; }
		public int TargetLearningOpportunityId { get; set; }

		public string TargetCompetency { get; set; } // url
		public string TargetCompetencyFramework { get; set; } // url

		//Used for publishing
		//TODO - make more concrete, and use entityUid to get actual entity/concrete object
		public object TargetDetermined
		{
			get
			{
				if ( TargetCredential != null && TargetCredential.Id != 0 )
				{
					return TargetCredential;
				}
				if ( TargetOrganization != null && TargetOrganization.Id != 0 )
				{
					return TargetOrganization;
				}
				if ( TargetAssessment != null && TargetAssessment.Id != 0 )
				{
					return TargetAssessment;
				}
				if ( TargetLearningOpportunity != null && TargetLearningOpportunity.Id != 0 )
				{
					return TargetLearningOpportunity;
				}
				return null;
		} }
		public object TargetOverride { get; set; }

		#endregion 
	}
	//

}
