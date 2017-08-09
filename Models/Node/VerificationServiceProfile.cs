using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{

	[Profile( DBType = typeof( Models.ProfileModels.VerificationServiceProfile ) )]
	public class VerificationServiceProfile : BaseProfile
	{
		[Property( DBName = "EstimatedCost", DBType = typeof( CostProfile ) )]
		public List<ProfileLink> Cost { get; set; }

		[Property( DBName = "RelevantCredential", DBType = typeof( Models.Common.Credential ), SaveAsProfile = true )]
		public ProfileLink Credential { get; set; }

		[Property( DBName = "Provider", DBType = typeof( Models.Common.Organization ) )]
		public List<ProfileLink> Verifier { get; set; } //Agent

		public List<ProfileLink> Jurisdiction { get; set; }
		public bool? HolderMustAuthorize { get; set; }

		[Property( DBName = "ClaimType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> ClaimType { get; set; }

		[Property( DBName = "OfferedByAgentUid", DBType = typeof( Guid ) )]
		public ProfileLink OfferedByAgentUid { get; set; }

		public string VerificationServiceUrl { get; set; }

		public string VerificationDirectory { get; set; }
		public string VerificationMethodDescription { get; set; }

		[Property( Type = typeof( JurisdictionProfile ) )]
		public List<ProfileLink> Region { get; set; }

		[Property( Type = typeof( JurisdictionProfile ) )]
		public List<ProfileLink> JurisdictionAssertions { get; set; }


		//[Property( DBName = "VerificationStatus", DBType = typeof( Models.Common.CredentialAlignmentObjectProfile ) )]
		//public List<ProfileLink> VerificationStatus { get; set; }


	}
	//

}
