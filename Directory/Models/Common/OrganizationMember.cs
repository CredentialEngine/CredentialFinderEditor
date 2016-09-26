using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;

namespace Models.Common
{
	public class OrganizationMember
	{

		public int Id { get; set; }
		public int ParentOrgId { get; set; }
		public int UserId { get; set; }
		public int OrgMemberTypeId { get; set; }
		public bool IsPrimaryOrganization { get; set; }

		public System.DateTime Created { get; set; }
		public int CreatedById { get; set; }
		public System.DateTime LastUpdated { get; set; }
		public int LastUpdatedById { get; set; }

		public virtual Organization Organization { get; set; }
		public virtual AppUser Account { get; set; }
	}
}
