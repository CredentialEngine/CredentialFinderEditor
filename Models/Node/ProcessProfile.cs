using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{
	[Profile( DBType = typeof( Models.ProfileModels.ProcessProfile ) )]
	public class ProcessProfile : BaseProfile
	{
		public ProcessProfile()
		{

		}

		//List-based Info
		[Property( DBName = "ExternalInput", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> ExternalInput { get; set; }

		//[Property( DBName = "ProcessMethod", DBType = typeof( Models.Common.Enumeration ) )]
		//public List<int> ProcessMethod { get; set; }

		//[Property( DBName = "StaffEvaluationMethod", DBType = typeof( Models.Common.Enumeration ) )]
		//public List<int> StaffEvaluationMethod { get; set; }

		public string TargetCompetencyFramework { get; set; }

		//public string DecisionInformationUrl { get; set; }
		//public string OfferedByDirectoryUrl { get; set; }
		//public string PublicInformationUrl { get; set; }
		//public string StaffEvaluationUrl { get; set; }
		//public string OutcomeReviewUrl { get; set; }
		//public string PoliciesAndProceduresUrl { get; set; }
		//public string ProcessCriteriaUrl { get; set; }
		//public string ProcessCriteriaValidationUrl { get; set; }
		//public string StaffSelectionCriteriaUrl { get; set; }

		public List<ProfileLink> Jurisdiction { get; set; }
		//=== review
		public string ProcessFrequency { get; set; }

		public string ProcessMethod { get; set; }
		public string ProcessMethodDescription { get; set; }
		public string ProcessStandards { get; set; }
		public string ProcessStandardsDescription { get; set; }
		public string ScoringMethodDescription { get; set; }
		public string ScoringMethodExample { get; set; }
		public string ScoringMethodExampleDescription { get; set; }
		public string VerificationMethodDescription { get; set; }
		public string SubjectWebpage { get; set; }

		[Property( DBName = "ProcessingAgentUid", DBType = typeof( Guid ) )]
		public ProfileLink ProcessingAgent { get; set; }

		[Property( DBName = "TargetAssessment" )]
		public List<ProfileLink> TargetAssessment { get; set; }

		[Property( DBName = "TargetLearningOpportunity" )]
		public List<ProfileLink> TargetLearningOpportunity { get; set; }

		[Property( DBName = "TargetCredential" )]
		public List<ProfileLink> TargetCredential { get; set; }

		//===  not used  - confirm
		//public ProfileLink RolePlayer { get; set; }
		//public List<int> ProcessTypeIds { get; set; }
		//public List<int> ExternalStakeholderTypeIds { get; set; }
		//public List<int> ProcessMethodTypeIds { get; set; }
		//public List<ProfileLink> MoreInformationUrl { get; set; }
		//public List<ProfileLink> Context { get; set; }
		//public List<ProfileLink> Frequency { get; set; } //Duration
		
	}
	//
}
