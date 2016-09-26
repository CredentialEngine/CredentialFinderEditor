using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Common;

namespace Models.ProfileModels
{
	public class QualityAssuranceActionProfile : OrganizationRoleProfile
	{
		public Credential IssuedCredential { get; set; } //QA credential (used by QA roles)

		//QA actions only have one instance, so the Agent Role enumeration will only have one entry, so expose at the object level
		public string QAAction { get; set; }
		public string QAActionSchema { get; set; }
		public string ReverseQAAction { get; set; }
		public string ReverseQAActionSchema { get; set; }
		///QA credential (used by QA roles)
		public int IssuedCredentialId { get; set; }
		public int QualityAssuranceTypeId { get; set; }
		public string StartDate
		{
			get { return this.DateEffective; }
			set { this.DateEffective = value; }
		}
		public string EndDate { get; set; }
		//public string Description { get; set; }

	}
	//
}
