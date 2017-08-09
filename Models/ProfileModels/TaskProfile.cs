using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Common;

namespace Models.ProfileModels
{
	public class TaskProfile : BaseProfile
	{
		public TaskProfile()
		{
			EstimatedCost = new List<CostProfile>();
			AffiliatedAgent = new Organization();
			//ResourceUrl = "";
			//IsNewVersion = false;
			ResourceUrl = new List<TextValueProfile>();
		}
		//public bool IsNewVersion { get; set; }
		//public string ResourceUrl { get; set; }
		public string AvailableOnlineAt { get; set; }
		public List<TextValueProfile> Auto_AvailableOnlineAt { get
			{
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( AvailableOnlineAt ) )
				{
					result.Add( new TextValueProfile() { TextValue = AvailableOnlineAt } );
				}
				return result;
			} }
		public List<TextValueProfile> ResourceUrl { get; set; }

		public Guid AffiliatedAgentUid { get; set; }
		public List<CostProfile> EstimatedCost { get; set; }
		public List<DurationProfile> EstimatedDuration { get; set; }
		public Organization AffiliatedAgent { get; set; }

		public List<Address> Addresses { get; set; }
		public string AvailabilityListing { get; set; }
		public List<TextValueProfile> Auto_AvailabilityListing { get
			{
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( AvailabilityListing ) )
				{
					result.Add( new TextValueProfile() { TextValue = AvailabilityListing } );
				}
				return result;
			} }
	}
	//
}
