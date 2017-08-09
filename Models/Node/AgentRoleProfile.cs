using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{

	[Profile( DBType = typeof( Models.ProfileModels.OrganizationRoleProfile ) )]
	public class AgentRoleProfile : BaseProfile //BaseProfile properties are not currently used, but the inheritance makes processing easier
	{
		[Property( DBName = "ActingAgent", DBType = typeof( Models.Common.Organization ), SaveAsProfile = true )]
		public ProfileLink Actor { get; set; }

		[Property( DBName = "ActedUponEntity", DBType = typeof( Models.Common.Entity ), SaveAsProfile = true )]
		public ProfileLink ActedUponEntityUid { get; set; }


		//Could be one of many types - requires special handling in the services layer
		public ProfileLink Recipient { get; set; }

		[Property( DBName = "AgentRole", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> RoleTypeIds { get; set; }
	}
	public class AgentRoleProfile_Recipient : AgentRoleProfile { }
	public class AgentRoleProfile_Actor : AgentRoleProfile { }
	public class OrganizationRole_Recipient : AgentRoleProfile { }
	public class AgentRoleProfile_Assets : AgentRoleProfile { }
	public class AgentRoleProfile_OfferedBy : AgentRoleProfile { }
	//
}
