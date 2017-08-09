using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{
	[Profile( DBType = typeof( Models.Common.CostManifest ) )]
	public class CostManifest : BaseProfile
	{
		public CostManifest()
		{
			EstimatedCosts = new List<ProfileLink>();
			
		}

		public int OrganizationId { get; set; }
		public string CostDetails { get; set; }
		public string StartDate { get; set; }
		public string EndDate { get; set; }
		public string CredentialRegistryId { get; set; }
		public string CTID { get; set; }

		[Property( Type = typeof( CostProfile ) )]
		public List<ProfileLink> EstimatedCosts { get; set; }

		
	}
}
