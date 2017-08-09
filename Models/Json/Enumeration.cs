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
			type = "ceterms:Enumeration";
		}

		[DataMember( Name="ceterms:name" )]
		public string name { get; set; }
		[DataMember( Name = "ceterms:description" )]
		public string description { get; set; }
		[DataMember( Name = "ceterms:url" )]
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

		[DataMember( Name="ceterms:name" )]
		public string name { get; set; }
		[DataMember( Name="ceterms:url" )]
		public string url { get; set; }
	}
	//

}
