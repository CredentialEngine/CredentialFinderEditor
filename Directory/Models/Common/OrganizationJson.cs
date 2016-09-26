using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.ProfileModels;

namespace Models.Common
{
	public class OrganizationJson
	{
		public int id { get; set; }
		
		public string name { get; set; }
		public string description { get; set; }
		public string url { get; set; }
		public string imageUrl { get; set; }
		public string date { get; set; }
		public AddressJson address { get; set; }
		public EnumerationJson organizationType { get; set; }
		public List<OrganizationRoleProfileJson> organizationRole { get; set; }
		public List<EnumerationJson> identifiers { get; set; }
		public List<AuthenticationProfileJson> authentication { get; set; }
		public List<QualityAssuranceProfileJson> qualityAssurance { get; set; }
		public List<OtherItemJson> other { get; set; }
	}
	//

}
