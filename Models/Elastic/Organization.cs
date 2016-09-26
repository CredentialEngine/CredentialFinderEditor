using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using Nest;
//using Elasticsearch.Net;
using Models.Common;
using Models.ProfileModels;

namespace Models.Elastic
{
	//[ElasticType(IdProperty = "Id", Name = "organization")]
	public class Organization : ElasticBaseObject
	{
		public Organization()
		{
			Addresses = new List<Address>();
			Subjects = new List<string>();
			Keywords = new List<string>();
		}

		public string Purpose { get; set; }
		public string UniqueURI { get; set; }

		public Enumeration OrganizationType { get; set; }
		public Enumeration QAPurposeType { get; set; }
		public List<Address> Addresses { get; set; }
		//consider value of combining these, as not likely to handle separately (with a category)
		public List<string> Subjects { get; set; }
		public List<string> Keywords { get; set; }


		//need to simplify these
		public List<OrganizationRoleProfile> OrganizationRole_Recipient { get; set; }
		//public List<OrganizationRoleProfile> OrganizationRole
		//{
		//	get { return OrganizationRole_Recipient; }
		//	set { OrganizationRole_Recipient = value; }
		//}
		public List<QualityAssuranceActionProfile> QualityAssuranceAction { get; set; }
	}
}
