using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using MN = Models.Node;

namespace Models.ProfileModels
{

	public class ConditionProfile : BaseProfile
	{
		public ConditionProfile()
		{
			AssertedBy = new Organization();
			CredentialType = new Enumeration();
			ApplicableAudienceType = new Enumeration();
			//ResidentOf = new List<GeoCoordinates>();
			ResidentOf = new List<JurisdictionProfile>();
			//TargetCompetency = new List<Enumeration>();
			TargetAssessment = new List<AssessmentProfile>();
			TargetLearningOpportunity = new List<LearningOpportunityProfile>();
			TargetTask = new List<TaskProfile>();
			RequiredCredential = new List<Credential>();
			ReferenceUrl = new List<TextValueProfile>();
			AssertedByOrgProfileLink = new MN.ProfileLink();
			//ReferenceUrl = new List<TextValueProfile>();
			ConditionItem = new List<TextValueProfile>();
			RequiresCompetencies = new List<CredentialAlignmentObjectProfile>();
		}

		public string ConnectionProfileType { get; set; }
		public int ConnectionProfileTypeId { get; set; }
		public Organization AssertedBy { get; set; }
		public MN.ProfileLink AssertedByOrgProfileLink { get; set; }
		public int AssertedById { get; set; } //organization Uid/Agent Uid
		public Guid AssertedByAgentUid { get; set; }
		public string Experience { get; set; }
		public int MinimumAge { get; set; }
		public decimal YearsOfExperience { get; set; }
		
		public Enumeration CredentialType { get; set; }
		public string OtherCredentialType { get; set; }
		public Enumeration ApplicableAudienceType { get; set; }
		public string OtherAudienceType { get; set; }
		//public List<GeoCoordinates> ResidentOf { get; set; }
		public List<JurisdictionProfile> ResidentOf { get; set; }
		//public List<Enumeration> TargetCompetency { get; set; }
		public List<TextValueProfile> TargetMiniCompetency { get; set; }
		public List<CredentialAlignmentObjectProfile> RequiresCompetencies { get; set; }
		public List<AssessmentProfile> TargetAssessment { get; set; }
		public List<LearningOpportunityProfile> TargetLearningOpportunity { get; set; }
		public List<TaskProfile> TargetTask { get; set; }

		public List<Credential> RequiredCredential { get; set; } //holds values of RequiredCredential
		public string IdentificationCode { get; set; }

		public List<TextValueProfile> RequiredCredentialUrl { get; set; }
		//OLD
		//public List<TextValueProfile> ReferenceUrl { get; set; }
		//NEW
		public List<TextValueProfile> ReferenceUrl { get; set; }
		
		public List<TextValueProfile> ConditionItem { get; set; }
		

	}
	//

}
