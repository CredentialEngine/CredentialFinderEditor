using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace Models.Json
{
	[DataContract]
	public class ConditionProfile : JsonLDObject
	{
		public ConditionProfile() {
			type = "ceterms:ConditionProfile";
		}

		[DataMember( Name="ceterms:assertedBy" )]
		public string assertedBy { get; set; }
		[DataMember( Name = "ceterms:description" )]
		public string description { get; set; }
		[DataMember( Name = "ceterms:experience" )]
		public string experience { get; set; }
		[DataMember( Name = "ceterms:minimumAge" )]
		public int minimumAge { get; set; }
		[DataMember( Name = "ceterms:applicableAudienceType" )]
		public List<string> applicableAudienceType { get; set; }

		[DataMember( Name = "ceterms:educationLevel" )]
		public List<string> educationLevel { get; set; }

		[DataMember( Name = "ceterms:jurisdiction" )]
		public List<JurisdictionProfile> jurisdiction { get; set; }
		[DataMember( Name = "ceterms:residentOf" )]
		public List<JurisdictionProfile> residentOf { get; set; }
		[DataMember( Name = "ceterms:targetTask" )]
		public List<TaskProfile> targetTask { get; set; }
		[DataMember( Name = "ceterms:targetCompetency" )]
		public List<string> targetCompetency { get; set; } //URLs
		[DataMember( Name = "ceterms:targetAssessment" )]
		public List<AssessmentProfile> targetAssessment { get; set; }
		[DataMember( Name = "ceterms:targetLearningOpportunity" )]
		public List<string> targetLearningOpportunity { get; set; } //URLs
		[DataMember( Name = "ceterms:targetCredential" )]
		public List<string> targetCredential { get; set; } //URLs
	}

}
