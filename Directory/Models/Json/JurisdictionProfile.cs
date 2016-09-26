using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace Models.Json
{
	[DataContract]
	public class JurisdictionProfile : JsonLDObject
	{
		public JurisdictionProfile()
		{
			type = "ctdl:JurisdictionProfile";
		}

		[DataMember( Name = "ctdl:isOnlineJurisdiction" )]
		public bool isOnlineJurisdiction { get; set; }

		[DataMember( Name = "ctdl:isGlobalJurisdiction" )]
		public bool isGlobalJurisdiction { get; set; }

		[DataMember( Name = "ctdl:mainJurisdiction" )]
		public GeoCoordinates mainJurisdiction { get; set; }

		[DataMember( Name = "ctdl:jurisdictionException" )]
		public GeoCoordinates jurisdictionException { get; set; }

	}
}
