using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{
	[Profile( DBType = typeof( Models.ProfileModels.TaskProfile ) )]
	public class TaskProfile : BaseProfile
	{
		public TaskProfile()
		{
			ResourceUrl = new List<TextValueProfile>();
			Addresses = new List<ProfileLink>();
		}
		[Property( DBName = "AffiliatedAgentUid", DBType = typeof( Guid ) )]
		public ProfileLink TaskProvider { get; set; }
		[Property( DBName = "EstimatedCost", DBType = typeof( Models.ProfileModels.CostProfile ) )]
		public List<ProfileLink> Cost { get; set; }

		[Property( DBName = "EstimatedDuration", DBType = typeof( Models.ProfileModels.DurationProfile ) )]
		public List<ProfileLink> Duration { get; set; }
		public List<ProfileLink> Jurisdiction { get; set; }

		[Property( DBName = "ResourceUrl", DBType = typeof( Models.ProfileModels.TextValueProfile ) )]
		public List<TextValueProfile> ResourceUrl { get; set; }

		//public string ResourceUrl { get; set; }

		public string AvailableOnlineAt { get; set; }
		public string AvailabilityListing { get; set; }

		[Property( DBName = "Addresses", DBType = typeof( Models.Common.Address ) )]
		public List<ProfileLink> Addresses { get; set; }
	}
	//
}
