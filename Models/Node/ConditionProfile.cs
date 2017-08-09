using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{
	[Profile( DBType = typeof( Models.ProfileModels.ConditionProfile ) )]
	public class ConditionProfile : BaseProfile
	{
		public ConditionProfile()
		{
			Other = new Dictionary<string, string>();
			ReferenceUrl = new List<TextValueProfile>();
			Condition = new List<TextValueProfile>();
			SubmissionOf = new List<TextValueProfile>();
		}

		//List-based Info
		public Dictionary<string, string> Other { get; set; }

		public int ConnectionProfileTypeId { get; set; }
		public int ConditionSubTypeId { get; set; }
		public int DisplayTypeId { get; set; }

		[Property( DBName = "AssertedByAgentUid", DBType = typeof( Guid ) )]
		public ProfileLink ConditionProvider { get; set; }

		#region general condition
		[Property( DBName = "ApplicableAudienceType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> AudienceType { get; set; }

		public string SubjectWebpage { get; set; }
		[Property( DBName = "AudienceLevel", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> AudienceLevel { get; set; }

		//[Property( DBName = "TargetMiniCompetency" )]
		//public List<TextValueProfile> MiniCompetency { get; set; } //Not being used currently
		[Property( DBName = "RequiresCompetenciesFrameworks", DBType = typeof( Models.Common.CredentialAlignmentObjectFrameworkProfile ) )]
		public List<ProfileLink> RequiresCompetencies { get; set; }

		public string Experience { get; set; }
		public int MinimumAge { get; set; }
		public decimal YearsOfExperience { get; set; }
		public decimal Weight { get; set; }

		public string CreditHourType { get; set; }
		public decimal CreditHourValue { get; set; }
		[Property( DBName = "CreditUnitTypeId" )]
		public int CreditUnitType { get; set; }
		public string CreditUnitTypeDescription { get; set; }
		public decimal CreditUnitValue { get; set; }

		public List<ProfileLink> Jurisdiction { get; set; }

		public List<TextValueProfile> Condition { get; set; }
		public List<TextValueProfile> SubmissionOf { get; set; }
		public List<TextValueProfile> ReferenceUrl { get; set; }

		[Property( DBName = "ResidentOf" )]
		public List<ProfileLink> Residency { get; set; }


		[Property( Type = typeof( CostProfile ) )]
		public List<ProfileLink> EstimatedCosts { get; set; }

		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> AlternativeCondition { get; set; }

		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> AdditionalCondition { get; set; }

		#endregion

		[Property( DBName = "TargetCredential", DBType = typeof( Credential ) )]
		public List<ProfileLink> Credential { get; set; }

		//[Property( DBName = "TargetLearningOpportunity" )]
		//public List<ProfileLink> LearningOpportunity { get; set; }

		[Property( DBName = "TargetLearningOpportunity", Type = typeof( LearningOpportunityProfile ) )]
		public List<ProfileLink> LearningOpportunity { get; set; }


		//[Property( DBName = "TargetAssessment" )]
		//public List<ProfileLink> Assessment { get; set; }
		[Property( DBName = "TargetAssessment", Type = typeof( AssessmentProfile ) )]
		public List<ProfileLink> Assessment { get; set; }
		//[Property( DBName = "TargetTask" )]
		//public List<ProfileLink> Task { get; set; }

		
	}

	//add aliases to the condition profile
	public class CredentialsConditionProfile : ConditionProfile { }
	public class CorequisiteConditionProfile : ConditionProfile { }


	public class ConditionManifestConditionProfile : ConditionProfile { }
	//public class LearningOpportunityConditionProfile : ConditionProfile { }

	public class AlternateConditionProfile : ConditionProfile { }
}
