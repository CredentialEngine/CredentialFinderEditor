using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{
	public class CredentialSummary : BaseObject
	{
		public CredentialSummary()
		{
			NaicsList = new List<CodeItem>();
			LevelsList = new List<CodeItem>();
			Addresses = new List<Address>();
		}
		public string Name { get; set; }
		public int StatusId { get; set; }
		public string ListTitle { get; set; }
		//public string effectiveDate { get; set; }
		public string Description { get; set; }
		public string Version { get; set; }
		public string LatestVersionUrl { get; set; }
		public string ReplacesVersionUrl { get; set; }
		public string AvailableOnlineAt { get; set; }
		
		public string Url { get; set; }
		public string NaicsList2 { get; set; }
		public string CredentialType { get; set; }
		public string CredentialTypeSchema { get; set; }
		public string CTID { get; set; }
		public string CredentialRegistryId { get; set; }
		public int ManagingOrgId { get; set; }
		public string ManagingOrganization { get; set; }
		public int CreatorOrganizationId { get; set; }
		public string CreatorOrganizationName { get; set; }
		public int OwnerOrganizationId { get; set; }
		public string OwnerOrganizationName { get; set; }
		public List<CodeItem> NaicsList { get; set; }
		public List<CodeItem> LevelsList { get; set; }
		public List<Address> Addresses { get; set; }
		
	}
}
