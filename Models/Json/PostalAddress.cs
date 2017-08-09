using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace Models.Json
{
	//Borrowed from http://schema.org/PostalAddress
	[DataContract]
	public class PostalAddress : JsonLDObject
	{
		public PostalAddress()
		{
			type = "ceterms:PostalAddress";
		}

		[DataMember( Name = "ceterms:addressCountry" )]
		public string addressCountry { get; set; } //The country. For example, USA. You can also provide the two-letter ISO 3166-1 alpha-2 country code.
		[DataMember( Name = "ceterms:addressLocality" )]
		public string addressLocality { get; set; } //The locality. For example, Mountain View.
		[DataMember( Name = "ceterms:addressRegion" )]
		public string addressRegion { get; set; } //The region. For example, CA.
		[DataMember( Name = "ceterms:postOfficeBoxNumber" )]
		public string postOfficeBoxNumber { get; set; } //The post office box number for PO box addresses.
		[DataMember( Name = "ceterms:postalCode" )]
		public string postalCode { get; set; } //The postal code. For example, 94043.
		[DataMember( Name = "ceterms:streetAddress" )]
		public string streetAddress { get; set; } //The street address. For example, 1600 Amphitheatre Pkwy.
	}
}
