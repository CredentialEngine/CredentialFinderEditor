using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.ProfileModels
{

	public class BaseProfile : BaseObject
	{
		public BaseProfile()
		{
			//Regions = new List<GeoCoordinates>();
			Jurisdiction = new List<JurisdictionProfile>();
		
		}
		public string ProfileName { get; set; }
		public string Description { get; set; }
		public string ProfileSummary { get; set; }

		//public List<GeoCoordinates> Regions { get; set; }
		public List<JurisdictionProfile> Jurisdiction { get; set; }
	}
	//

}
