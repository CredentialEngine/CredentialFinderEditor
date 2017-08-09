using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Models.Common;

namespace CTI.Directory.Models
{
	public class CredentialViewModel 
	{
		public CredentialViewModel()
		{
			Credential = new Credential();
		}
		public Credential Credential { get; set; }

		public IEnumerable<SelectListItem> CredentialTypes { get; set; }

		public string CredentialOrganizationTypeId { get; set; }
		//public CredentialOrgTypes CredentialOrganizationTypes { get; set; }

		public string EarningCredentialPrimaryMethodId { get; set; }
		//public EarningCredentialPrimaryMethods EarningCredentialPrimaryMethods { get; set; }
	}
	public enum CredentialOrgTypes
	{
		CredentialOrganization = 1,
		QAOrganization = 2
	}

	public enum EarningCredentialPrimaryMethods
	{
		Assessment = 1,
		LearningOpportunity = 2
	}
}