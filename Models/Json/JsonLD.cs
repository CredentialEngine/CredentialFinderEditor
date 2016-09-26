using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Models.Json
{
	[DataContract]
	public class JsonLDDocument
	{
		public JsonLDDocument()
		{
			context = new Dictionary<string, object>()
			{
				{ "schema", "http://schema.org/" },
				{ "dc", "http://purl.org/dc/elements/1.1/" },
				{ "dct", "http://dublincore.org/terms/" },
				{ "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#" },
				{ "rdfs", "http://www.w3.org/2000/01/rdf-schema#" },
				{ "ctdl", "[CTI Namespace Not Determined Yet]" }
			};
		}

		[DataMember( Name="@context" )]
		[JsonProperty(Order=-10)]
		public Dictionary<string, object> context { get; set; }

		[DataMember( Name = "@type" )]
		[JsonProperty( Order = -9 )]
		public string type { get; set; }

	}
	//

	[DataContract]
	public class JsonLDObject
	{
		[DataMember( Name = "@type" )]
		public string type { get; set; }

		[DataMember( Name = "@id" )]
		public string id { get; set; }

	}
	//

}
