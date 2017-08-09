using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace Models.Json
{
	[DataContract]
	public class GeoCoordinates : JsonLDObject
	{
		public GeoCoordinates()
		{
			type = "ceterms:GeoCoordinates";
		}

		[DataMember( Name = "latitude" )]
		public double latitude { get; set; }

		[DataMember( Name = "longitude" )]
		public double longitude { get; set; }

		[DataMember( Name = "name" )]
		public string name { get; set; }

		[DataMember( Name = "url" )]
		public string url { get; set; }

	}
}
