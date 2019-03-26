using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.ProfileModels
{

	public class RevocationProfile : BaseProfile
	{
		public RevocationProfile()
		{
			//RevocationCriteriaType = new Enumeration();
			Region = new List<JurisdictionProfile>();
			//CredentialProfiled = new List<Credential>();
			RemovalDateEffective = "";
			//RenewalDateEffective = "";
		}
		
		public string RemovalDateEffective
		{
			get { return DateEffective; }
			set { DateEffective = value; }
		}
		//public string RenewalDateEffective { get; set; }
		
		public string RevocationCriteriaUrl { get; set; }
		public string RevocationCriteriaDescription { get; set; }

		//deprecated 170825
		//public List<Credential> CredentialProfiled { get; set; } //holds values of RequiredCredential

		public List<JurisdictionProfile> Region { get; set; }

		//obsolete
		//public Enumeration RevocationCriteriaType { get; set; }
		//public string OtherRevocationCriteriaType { get; set; }
		//public List<TextValueProfile> RevocationResourceUrl { get; set; }
		//public List<TextValueProfile> RevocationItems { get; set; }
	}
	//

}
