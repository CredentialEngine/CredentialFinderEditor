using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.ProfileModels
{
	
	public class AuthenticationProfile : BaseProfile
	{
		public AuthenticationProfile()
		{
			EstimatedCost = new List<CostProfile>();
			RelevantCredential = new Credential();
		}

		public bool HolderMustAuthorize { get; set; }
		public List<CostProfile> EstimatedCost { get; set; }
		public string TargetCredential { get; set; } //url
		public int TargetCredentialId { get { return RelevantCredential.Id; } set { RelevantCredential.Id = value; } }
		public Credential RelevantCredential { get; set; } //Workaround
	}
	//

}
