using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{
	[Profile( DBType = typeof( Models.Common.Organization ) )]
	public class Organization : BaseMainProfile
	{
		//Basic Info
		//public string Founded { get; set; }
		public string FoundingYear { get; set; }
		public string FoundingMonth { get; set; }
		public string FoundingDay { get; set; }
		public string Purpose { get; set; }
		public string ImageUrl { get; set; }
		public int ManagingOrgId { get; set; }

		//List-based Info
		[Property( DBName = "OrganizationType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> OrganizationTypeIds { get; set; }

		[Property( DBName = "ServiceType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> OrganizationServiceTypeIds { get; set; }

		[Property( DBName = "OrganizationSectorType", DBType = typeof( Models.Common.Enumeration ) )]
		public int OrganizationSectorTypeId { get; set; }

		[Property( DBName = "QAPurposeType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> QAPurposeTypeIds { get; set; }

		//public List<TextValueProfile> Subjects { get; set; }
		public List<TextValueProfile> Keywords { get; set; }
		//Text Value Items
		public List<TextValueProfile> SocialMediaPages { get; set; }

		public List<TextValueProfile> IdentificationCodes { get; set; } //Currently uses enumeration server-side
		public List<TextValueProfile> PhoneNumbers { get; set; }
		public List<TextValueProfile> Emails { get; set; }

		//Profiles
		[Property( DBName = "Addresses", DBType = typeof( Models.Common.Address ) )]
		public List<ProfileLink> Address { get; set; }

		[Property( DBName = "OrganizationRole_Dept", DBType = typeof( AgentRoleProfile_Recipient ) )]
		public List<ProfileLink> Department { get; set; }

		[Property( DBName = "OrganizationRole_Subsiduary", DBType = typeof( AgentRoleProfile_Recipient ) )]
		public List<ProfileLink> Subsidiary { get; set; }

		[Property( DBName = "OrganizationRole_Actor", DBType = typeof( AgentRoleProfile_Actor ) )]
		public List<ProfileLink> AgentRole_Actor { get; set; }

		[Property( DBName = "Authentication", DBType = typeof( Models.ProfileModels.AuthenticationProfile ) )]
		public List<ProfileLink> VerificationService { get; set; }

		//Not available
		[Property( DBName = "QualityAssuranceAction", DBType = typeof( AgentRoleProfile_Actor ) )]
		public List<ProfileLink> QualityAssuranceAction_Actor { get; set; }

	}
	//
}
