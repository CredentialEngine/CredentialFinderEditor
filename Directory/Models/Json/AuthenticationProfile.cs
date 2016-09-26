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
			type = "ctdl:AuthenticationProfile";
		}

		[DataMember( Name = "schema:description" )]
		public string description { get; set; }

	}
}
