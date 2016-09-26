using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace Models.Json
{
	[DataContract]
	public class Enumeration : JsonLDObject
	{
		public Enumeration()
		{
			type = "schema:Enumeration";
		}

		[DataMember( Name="schema:name" )]
		public string name { get; set; }
		[DataMember( Name = "schema:description" )]
		public string description { get; set; }
		[DataMember( Name = "schema:url" )]
		public string url { get; set; }
		[DataMember( Name = "unknown:items" )]
		public List<EnumerationItem> items { get; set; }

	}
	//

	[DataContract]
	public class EnumerationItem : JsonLDObject
	{
		public EnumerationItem()
		{
			type = "unknown:EnumerationItem";
		}

		[DataMember( Name="schema:name" )]
		public string name { get; set; }
		[DataMember( Name="schema:url" )]
		public string url { get; set; }
	}
	//

}
