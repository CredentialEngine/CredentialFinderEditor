using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.ProfileModels
{

	public class ProcessProfile : BaseProfile
	{
		public ProcessProfile()
		{
			ProcessingAgent = new Organization();
			TargetAssessment = new AssessmentProfile();
			ExternalInput = new Enumeration();
			ProcessMethod = new Enumeration();
			ProcessType = new Enumeration();
		}
		public AssessmentProfile TargetAssessment { get; set; }
		public string TargetCompetencyFramework { get; set; }
		public string TargetCredential { get; set; }
		public Organization ProcessingAgent { get; set; }
		public int ProcessingAgentId { get; set; }
		public Enumeration ExternalInput { get; set; }
		public Enumeration ProcessMethod { get; set; }
		public Enumeration ProcessType { get; set; }
		public string ProcessFrequency { get; set; }
		public string ProcessCriteriaUrl { get; set; }
		public string ProcessContextUrl { get; set; }
	}
	//

}
