using Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{

	[Profile( DBType = typeof( Models.ProfileModels.OrganizationAssertion ) )]
	public class OrganizationAssertion : BaseProfile //BaseProfile properties are not currently used, but the inheritance makes processing easier
	{
        //[Property( DBName = "Recipient", DBType = typeof( Models.ProfileModels.TargetEntity ), SaveAsProfile = true )]
        //public ProfileLink Actor { get; set; }

        [Property( DBName = "ActedUponEntity", DBType = typeof( Models.Common.Entity ), SaveAsProfile = true )]
		public ProfileLink ActedUponEntityUid { get; set; }


		//Could be one of many types - requires special handling in the services layer
        [Property(SaveAsProfile = true )]
		public ProfileLink Recipient { get; set; }

		[Property( DBName = "AgentAssertion", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> RoleTypeIds { get; set; }
    }

    //public class Agent_QAPerformed_Assertion : AgentRoleProfile { }
    public class Agent_QAPerformed_Credential : OrganizationAssertion { }
    public class Agent_QAPerformed_Organization : OrganizationAssertion { }
    public class Agent_QAPerformed_Assessment : OrganizationAssertion { }
    public class Agent_QAPerformed_Lopp : OrganizationAssertion { }
    //
}
