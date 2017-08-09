using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{
	[Profile( DBType = typeof( Models.Common.ConditionManifest ) )]
	public class ConditionManifest : BaseProfile
	{
		public ConditionManifest ()
		{
			Requires = new List<ProfileLink>();
			Recommends = new List<ProfileLink>();
			EntryCondition = new List<ProfileLink>();
			Corequisite = new List<ProfileLink>();
		}

		public int OrganizationId { get; set; }
		//public string Name { get; set; }
		//public string Description { get; set; }
		public string SubjectWebpage { get; set; }
		//public int ConditionTypeId { get; set; }
		public string CredentialRegistryId { get; set; }

		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> ConditionProfiles { get; set; }

		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> Requires { get; set; }

		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> Recommends { get; set; }
		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> EntryCondition { get; set; }

		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> Corequisite { get; set; }

		//[Property( Type = typeof( ConditionManifest ) )]
		//public List<ProfileLink> CommonConditions { get; set; }

	}
}
