using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace Models.Json
{
	[DataContract]
	public class AssessmentProfile : JsonLDObject
	{
		public AssessmentProfile()
		{
			type = "ceterms:AssessmentProfile";
		}

		[DataMember( Name = "ceterms:name" )]
		public string name { get; set; }
		[DataMember( Name = "ceterms:description" )]
		public string description { get; set; }
		[DataMember( Name = "ceterms:url" )]
		public string url { get; set; }

	}
	//
}
