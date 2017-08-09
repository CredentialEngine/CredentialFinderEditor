using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{
	public class OrganizationSummary : Organization
	{
		public OrganizationSummary()
		{

		}

		public int QARolesCount { get; set; }
		public List<string> Subjects { get; set; }
		public CodeItemResult NaicsResults { get; set; }
		public CodeItemResult IndustryOtherResults { get; set; }

		public CodeItemResult OwnedByResults { get; set; }
		public CodeItemResult OfferedByResults { get; set; }
		public CodeItemResult AsmtsOwnedByResults { get; set; }
		public CodeItemResult LoppsOwnedByResults { get; set; }

		public CodeItemResult AccreditedByResults { get; set; }
		public CodeItemResult ApprovedByResults { get; set; }

		//public CredentialConnectionsResult OwnedByResults { get; set; }
		//public CredentialConnectionsResult OfferedByResults { get; set; }

		public AgentRelationshipResult AgentAndRoles { get; set; }
	}

}
