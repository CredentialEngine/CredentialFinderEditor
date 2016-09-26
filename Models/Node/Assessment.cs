using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{
	[Profile( DBType = typeof( Models.ProfileModels.AssessmentProfile ) )]
	public class Assessment : BaseMainProfile
	{
		public Assessment()
		{
			AssessmentTypeIds = new List<int>();
			AssessmentModalityTypeIds = new List<int>();
			Subjects = new List<TextValueProfile>();
			Keywords = new List<TextValueProfile>();
		}

		//Basic Info

		//List-based Info
		[Property( DBName = "AssessmentType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> AssessmentTypeIds { get; set; }

		[Property( DBName = "Modality", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> AssessmentModalityTypeIds { get; set; }

		public List<TextValueProfile> Subjects { get; set; }
		public List<TextValueProfile> Keywords { get; set; }

		//Text Value Info
		//public List<TextValueProfile> IdentificationCode { get; set; }
		public string IdentificationCode { get; set; }
		public string AvailableOnlineAt { get; set; }

		[Property( DBName = "AssessmentExampleUrl" )]
		public List<TextValueProfile> Example { get; set; }

		//Profiles
		[Property( DBName = "EstimatedCost" )]
		public List<ProfileLink> Cost { get; set; }
		[Property( DBName = "EstimatedDuration" )]
		public List<ProfileLink> Duration { get; set; }

		[Property( DBName = "RequiresCompetencies", DBType = typeof( Models.Common.CredentialAlignmentObjectProfile ) )]
		public List<ProfileLink> RequiresCompetencies { get; set; }

		[Property( DBName = "AssessesCompetencies", DBType= typeof( Models.Common.CredentialAlignmentObjectProfile  ))]
		public List<ProfileLink> AssessesCompetencies { get; set; }

		[Property( DBName = "Addresses", DBType = typeof( Models.Common.Address ) )]
		public List<ProfileLink> Addresses { get; set; }

	}
	//
}
