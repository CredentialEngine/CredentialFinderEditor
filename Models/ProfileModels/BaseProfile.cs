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
			ViewHeading = "";
			ReferenceUrl = new List<TextValueProfile>();
			ParentSummary = "";
		}
		public string ProfileName { get; set; }
		public string Description { get; set; }
		public string ProfileSummary { get; set; }

		public string ParentSummary { get; set; }
		public string ViewHeading { get; set; }
		//public List<GeoCoordinates> Regions { get; set; }
		/// <summary>
		/// The geo-political region in which the described resource is applicable.
		/// </summary>
		public List<JurisdictionProfile> Jurisdiction { get; set; }

		public List<TextValueProfile> ReferenceUrl { get; set; }

		public List<TextValueProfile> Auto_Helper (string text)
		{
			var result = new List<TextValueProfile>();
			if ( !string.IsNullOrWhiteSpace( text ) )
			{
				result.Add( new TextValueProfile() { TextValue = text } );
			}
			return result;
			
		}
	}
	//

}
