using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{
	[Profile( DBType = typeof( Models.ProfileModels.VerificationStatus ) )]
	public class VerificationStatus : BaseProfile
	{

		public string VerifiedClaim { get; set; }
		public string DecisionInformation { get; set; }

	}
	//
}
