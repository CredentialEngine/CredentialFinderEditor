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
			type = "ceterms:JurisdictionProfile";
		}

		//[DataMember( Name = "ceterms:isOnlineJurisdiction" )]
		//public bool isOnlineJurisdiction { get; set; }

		[DataMember( Name = "ceterms:isGlobalJurisdiction" )]
		public bool isGlobalJurisdiction { get; set; }

		[DataMember( Name = "ceterms:mainJurisdiction" )]
		public GeoCoordinates mainJurisdiction { get; set; }

		[DataMember( Name = "ceterms:jurisdictionException" )]
		public GeoCoordinates jurisdictionException { get; set; }

	}
}
