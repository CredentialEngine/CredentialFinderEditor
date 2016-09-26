using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{
	[Profile( DBType = typeof( Models.Common.Credential ) )]
	public class Credential : BaseMainProfile
	{
		//Basic Info
		public string ImageUrl { get; set; }
		public string AlternateName { get; set; }
		public string Version { get; set; }
		public string LatestVersionUrl { get; set; }
		public string ReplacesVersionUrl { get; set; }
		public int ManagingOrgId { get; set; }
		
		//List-based Info
		[Property( DBName = "CredentialType", DBType = typeof( Models.Common.Enumeration ) )]
		public int CredentialTypeId { get { return CredentialTypeIds.FirstOrDefault(); } set { CredentialTypeIds = new List<int>() { value }; } }

		[Property( DBName = "null" )] //Database processes need to skip this item
		public List<int> CredentialTypeIds { get; set; }

		[Property( DBName = "Purpose", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> CredentialPurposeTypeIds { get; set; }

		[Property( DBName = "CredentialLevel", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> CredentialLevelTypeIds { get; set; }

		public string AvailableOnlineAt { get; set; }

		public List<TextValueProfile> Subjects { get; set; }
		public List<TextValueProfile> Keywords { get; set; }
		public List<TextValueProfile> OtherIndustries { get; set; }
		public List<TextValueProfile> OtherOccupations { get; set; }
		//Text Value Info
		[Property( Type = typeof( MicroProfile ), DBType = typeof( Models.Common.Enumeration ) )]
		public List<ProfileLink> Industry { get; set; }

		[Property( Type = typeof( MicroProfile ), DBType = typeof( Models.Common.Enumeration ) )]
		public List<ProfileLink> Occupation { get; set; }

		//Profile Info
		[Property( Type = typeof( DurationProfile ) )]
		public List<ProfileLink> EstimatedTimeToEarn { get; set; }

		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> Requires { get; set; }

		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> Recommends { get; set; }

		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> IsRequiredFor { get; set; }

		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> IsRecommendedFor { get; set; }

		[Property( Type = typeof( ConditionProfile ) )]
		public List<ProfileLink> Renewal { get; set; }

		[Property( Type = typeof( RevocationProfile ) )]
		public List<ProfileLink> Revocation { get; set; }

		[Property( Type = typeof( ProcessProfile ) )]
		public List<ProfileLink> CredentialProcess { get; set; }

		[Property( Type = typeof( EarningsProfile ) )]
		public List<ProfileLink> Earnings { get; set; }

		[Property( Type = typeof( EmploymentOutcomeProfile ) )]
		public List<ProfileLink> EmploymentOutcome { get; set; }

		[Property( Type = typeof( HoldersProfile ) )]
		public List<ProfileLink> Holders { get; set; }

		[Property( Type = typeof( Credential ) )]
		public List<ProfileLink> EmbeddedCredentials { get; set; }

		[Property( Type = typeof( Credential ) )]
		public List<ProfileLink> ParentCredential { get; set; }

		[Property( Type = typeof( CostProfile ) )]
		public List<ProfileLink> EstimatedCosts { get; set; }

		[Property( DBName = "Addresses", DBType = typeof( Models.Common.Address ) )]
		public List<ProfileLink> Addresses { get; set; }
	}
	//

}
