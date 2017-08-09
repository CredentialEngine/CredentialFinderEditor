using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.ProfileModels
{
	[Obsolete]
	abstract class AuthenticationProfile : BaseProfile
	{
		//public AuthenticationProfile()
		//{
		//	//EstimatedCost = new List<CostProfile>();
		//	//RelevantCredential = new Credential();
		//	//ClaimType = new Enumeration();
		//}

		//public string VerificationServiceUrl { get; set; }
		//public bool HolderMustAuthorize { get; set; }
		//public List<CostProfile> EstimatedCost { get; set; }
		//public List<CostProfile> EstimatedCosts { get { return EstimatedCost; } set { EstimatedCost = value; } } //Convenience for publishing


		////note the credential will now be the context entity
		//public string TargetCredential { get; set; } //url
		//public int TargetCredentialId { get { return RelevantCredential.Id; } set { RelevantCredential.Id = value; } }
		//public Credential RelevantCredential { get; set; } //Workaround

		//public Enumeration ClaimType { get; set; }


		//public Guid OfferedByAgentUid { get; set; }
		//public string VerificationDirectory { get; set; }
		//public string VerificationMethodDescription { get; set; }
		//public List<JurisdictionProfile> OfferedIn { get; set; }
	}
	//

}
