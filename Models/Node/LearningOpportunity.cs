using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.Node
{
	[Profile( DBType = typeof( Models.ProfileModels.LearningOpportunityProfile ) )]
	public class LearningOpportunity : BaseMainProfile
	{
		public LearningOpportunity()
		{
			DeliveryTypeIds = new List<int>();
		}

		//Basic Info

		//List-based Info
		[Property( DBName = "LearningOpportunityDeliveryType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> DeliveryTypeIds { get; set; }
		public int ManagingOrgId { get; set; }
		//Text Value Info
		//public List<TextValueProfile> IdentificationCode { get; set; }
		public string IdentificationCode { get; set; }
		public string AvailableOnlineAt { get; set; }
		//[Property( DBName = "InstructionalProgramCategory" )]
		//public List<ProfileLink> CipCode { get; set; }

		[Property( DBName = "InstructionalProgramCategory", Type = typeof( MicroProfile ), DBType = typeof( Models.Common.Enumeration ) )]
		public List<ProfileLink> CipCode { get; set; }

		//Profiles
		[Property( DBName = "LearningResourceUrl" )]
		public List<TextValueProfile> LearningResource { get; set; }

		public List<TextValueProfile> Subjects { get; set; }
		public List<TextValueProfile> Keywords { get; set; }

		[Property( DBName = "EstimatedCost" )]
		public List<ProfileLink> Cost { get; set; }
		[Property( DBName = "EstimatedDuration" )]
		public List<ProfileLink> Duration { get; set; }
		[Property( DBName = "HasPart" )]
		public List<ProfileLink> EmbeddedLearningOpportunity { get; set; }
		[Property( DBName = "IsPartOf" )]
		public List<ProfileLink> ParentLearningOpportunity { get; set; }

		[Property( DBName = "Addresses", DBType = typeof( Models.Common.Address ) )]
		public List<ProfileLink> Addresses { get; set; }

		//[Property( DBName = "Requires", DBType = typeof( Models.ProfileModels.ConditionProfile ) )]
		//public List<ConditionProfile> Requires { get; set; }
		//OR
		[Property(Type = typeof(ConditionProfile))]
		public List<ProfileLink> Requires { get; set; }

		//[Property( DBName = "LearningCompetencies" )]
		//public List<TextValueProfile> LearningCompetencies { get; set; }

		[Property( DBName = "RequiresCompetencies", DBType= typeof( Models.Common.CredentialAlignmentObjectProfile  ))]
		public List<ProfileLink> RequiresCompetencies { get; set; }

		[Property( DBName = "TeachesCompetencies", DBType= typeof( Models.Common.CredentialAlignmentObjectProfile ))]
		public List<ProfileLink> TeachesCompetencies { get; set; }

		[Property( DBName = "RequiresCompetenciesFrameworks", DBType = typeof( Models.Common.CredentialAlignmentObjectFrameworkProfile ) )]
		public List<ProfileLink> RequiresCompetenciesFrameworks { get; set; }

		[Property( DBName = "TeachesCompetenciesFrameworks", DBType = typeof( Models.Common.CredentialAlignmentObjectFrameworkProfile ) )]
		public List<ProfileLink> TeachesCompetenciesFrameworks { get; set; }

	}
	//
}
