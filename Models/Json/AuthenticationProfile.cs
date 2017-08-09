using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace Models.Json
{
	[DataContract]
	public class AuthenticationProfile :JsonLDObject
	{
		public AuthenticationProfile()
		{
			type = "ceterms:AuthenticationProfile";
		}

		[DataMember( Name = "ceterms:description" )]
		public string description { get; set; }

	}
}
