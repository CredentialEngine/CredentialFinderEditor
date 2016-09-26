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
			type = "ctdl:ConditionProfile";
		}

		[DataMember( Name="ctdl:assertedBy" )]
		public string assertedBy { get; set; }
		[DataMember( Name = "schema:description" )]
		public string description { get; set; }
		[DataMember( Name = "ctdl:experience" )]
		public string experience { get; set; }
		[DataMember( Name = "ctdl:minimumAge" )]
		public int minimumAge { get; set; }
		[DataMember( Name = "ctdl:applicableAudienceType" )]
		public List<string> applicableAudienceType { get; set; }
		[DataMember( Name = "ctdl:credentialType" )]
		public List<string> credentialType { get; set; }
		[DataMember( Name = "ctdl:jurisdiction" )]
		public List<JurisdictionProfile> jurisdiction { get; set; }
		[DataMember( Name = "ctdl:residentOf" )]
		public List<JurisdictionProfile> residentOf { get; set; }
		[DataMember( Name = "ctdl:targetTask" )]
		public List<TaskProfile> targetTask { get; set; }
		[DataMember( Name = "ctdl:targetCompetency" )]
		public List<string> targetCompetency { get; set; } //URLs
		[DataMember( Name = "ctdl:targetAssessment" )]
		public List<AssessmentProfile> targetAssessment { get; set; }
		[DataMember( Name = "ctdl:targetLearningOpportunity" )]
		public List<string> targetLearningOpportunity { get; set; } //URLs
		[DataMember( Name = "ctdl:targetCredential" )]
		public List<string> targetCredential { get; set; } //URLs
	}

}
