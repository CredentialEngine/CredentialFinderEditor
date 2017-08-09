using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using MN = Models.Node;

namespace Models.ProfileModels
{

	public class ProcessProfile : BaseProfile
	{
		public ProcessProfile()
		{
			ProcessingAgent = new Organization();
			ExternalInput = new Enumeration();
			//ProcessMethod = new Enumeration();
			
			//ProcessType = new Enumeration();
			ProcessTypeId = 1;

			//Jurisdiction = new List<JurisdictionProfile>();
			TargetAssessment = new List<AssessmentProfile>();
			TargetCredential = new List<Credential>();
			TargetLearningOpportunity = new List<LearningOpportunityProfile>();

			Region = new List<JurisdictionProfile>();
		}

		public int ProcessTypeId { get; set; }
		public string ProcessProfileType { get; set; }
		public Organization ProcessingAgent { get; set; }
		public MN.ProfileLink ProcessingAgentProfileLink { get; set; }
		public Guid ProcessingAgentUid { get; set; }
		public Enumeration ExternalInput { get; set; }
		/// <summary>
		/// Alias used for publishing
		/// </summary>
		public Enumeration ExternalInputType { get { return ExternalInput; } set { ExternalInput = value; } } //

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

		/// <summary>
		/// A geo-political area of the described resource.
		/// </summary>
		public List<JurisdictionProfile> Region { get; set; }

		#region obsolete 
		//public Enumeration ProcessMethod { get; set; }
		//[Obsolete]
		//public Enumeration StaffEvaluationMethod { get; set; }
		//[Obsolete]
		//public string DecisionInformationUrl { get; set; }
		//[Obsolete]
		//public string OfferedByDirectoryUrl { get; set; }
		//[Obsolete]
		//public string PublicInformationUrl { get; set; }
		//[Obsolete]
		//public string StaffEvaluationUrl { get; set; }
		//[Obsolete]
		//public string OutcomeReviewUrl { get; set; }
		//[Obsolete]
		//public string PoliciesAndProceduresUrl { get; set; }
		//[Obsolete]
		//public string ProcessCriteriaUrl { get; set; }
		//[Obsolete]
		//public string ProcessCriteriaValidationUrl { get; set; }
		//[Obsolete]
		//public string StaffSelectionCriteriaUrl { get; set; }
		#endregion 



		//TODO - chg
		public string TargetCompetencyFramework { get; set; }
		public List<CredentialAlignmentObjectFrameworkProfile> RequiresCompetenciesFrameworks { get; set; }

		public List<AssessmentProfile> TargetAssessment { get; set; }
		public List<LearningOpportunityProfile> TargetLearningOpportunity  { get; set; }
		public List<Credential> TargetCredential { get; set; }

		public string ProcessType
		{
			get
			{
				if ( ProcessTypeId == 2 )
					return "Appeal Process ";
				else if ( ProcessTypeId == 3 )
					return "Complaint Process ";
				else if ( ProcessTypeId == 4 )
					return "Criteria Process ";
				else if ( ProcessTypeId == 5 )
					return "Review Process ";
				else if ( ProcessTypeId == 6 )
					return "Revoke Process ";
				
				else
					return "Process Profile ";

			}
		}

	}
	//

}
