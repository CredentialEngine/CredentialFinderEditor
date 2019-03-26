using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.ProfileModels
{
	public class Entity_Credential : BaseObject
	{

		public int CredentialId { get; set; }
		public Guid CredentialUid { get; set; }
		public string CredentialTitle { get; set; }
		public Credential Credential { get; set; } = new Credential();
		public string ProfileSummary { get; set; }
	}
}
