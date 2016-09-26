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
			type = "ctdl:AssessmentProfile";
		}

		[DataMember( Name = "schema:name" )]
		public string name { get; set; }
		[DataMember( Name = "schema:description" )]
		public string description { get; set; }
		[DataMember( Name = "schema:url" )]
		public string url { get; set; }

	}
	//
}
