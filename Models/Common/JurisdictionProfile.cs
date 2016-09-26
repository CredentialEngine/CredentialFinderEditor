using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{

	public class JurisdictionProfile : BaseObject
	{
		public JurisdictionProfile()
		{
			MainJurisdiction = new GeoCoordinates();
			JurisdictionException = new List<GeoCoordinates>();
		}
		public string ParentType { get; set; }
		public int ParentTypeId { get; set; }

		/// <summary>
		/// Identifer of the parent Entity
		/// </summary>
		public Guid ParentEntityId { get; set; }
		public bool IsGlobalJurisdiction { get; set; }
		public bool IsOnlineJurisdiction { get; set; }
		public int JProfilePurposeId { get; set; }
		
		public string Description { get; set; }
		public string ProfileSummary { get; set; }
		public GeoCoordinates MainJurisdiction { get; set; }
		public List<GeoCoordinates> MainJurisdictions { get; set; }
		public List<GeoCoordinates> JurisdictionException { get; set; }

	}
	//

}
