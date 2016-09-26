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
			RevocationCriteriaType = new Enumeration();
			RemovalDateEffective = "";
			//RenewalDateEffective = "";
		}
		public Enumeration RevocationCriteriaType { get; set; }
		public string OtherRevocationCriteriaType { get; set; }
		public string RemovalDateEffective
		{
			get { return DateEffective; }
			set { DateEffective = value; }
		}
		//public string RenewalDateEffective { get; set; }
		public List<TextValueProfile> RevocationResourceUrl { get; set; }
		public string RevocationCriteriaUrl { get; set; }
	}
	//

}
