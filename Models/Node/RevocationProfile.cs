using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{

	[Profile( DBType = typeof( Models.ProfileModels.RevocationProfile ) )]
	public class RevocationProfile : BaseProfile
	{
		public RevocationProfile()
		{
			//Other = new Dictionary<string, string>();
			Jurisdiction = new List<ProfileLink>();
			Region = new List<ProfileLink>();
			CredentialProfiled = new List<ProfileLink>();
		}

		//List-based Info
		//public Dictionary<string, string> Other { get; set; }
		//in base:
		//public string DateEffective { get; set; }

		//[Property( DBName = "RevocationCriteriaType", DBType = typeof( Models.Common.Enumeration ) )]
		//public List<int> RevocationCriteriaTypeIds { get; set; }

		//[Property( DBName = "RevocationResourceUrl" )]
		//public List<TextValueProfile> ReferenceUrl { get; set; }

		public List<ProfileLink> Jurisdiction { get; set; }
		public List<ProfileLink> Region { get; set; }
		public string RevocationCriteriaUrl { get; set; }
		public string RevocationCriteriaDescription { get; set; }
		//public List<TextValueProfile> RevocationItems { get; set; }

		[Property( DBName = "CredentialProfiled", DBType = typeof( Credential ) )]
		public List<ProfileLink> CredentialProfiled { get; set; }
	}
	//

}
