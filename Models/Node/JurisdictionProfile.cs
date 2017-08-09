using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{

	[Profile( DBType = typeof( Models.Common.JurisdictionProfile ) )]
	public class JurisdictionProfile : BaseProfile
	{
		public bool? IsGlobalJurisdiction { get; set; }
		//public bool IsOnlineJurisdiction { get; set; }

		[Property( DBName = "MainJurisdiction", SaveAsProfile = true )]
		public ProfileLink MainRegion { get; set; }

		[Property( DBName = "JurisdictionException", SaveAsProfile = true )]
		public List<ProfileLink> RegionException { get; set; }
		[Property( DBName = "ProfileSummary" )]
		public override string Name { get; set; } //Override the annotation on the base profile name


		[Property( DBName = "AssertedBy", DBType = typeof( Guid ) )]
		public ProfileLink AssertedBy { get; set; }

		[Property( DBName = "JurisdictionAssertion", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> JurisdictionAssertionIds { get; set; }
	}

	public class JurisdictionProfile_QA : JurisdictionProfile 	{ }

	//

	//May not be used - will have to see
	public class RegionProfile : BaseProfile
	{
		public string ToponymName { get; set; }
		public string Region { get; set; }
		public string Country { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public string GeonamesUrl { get; set; } //URL of a geonames place
		public string TitleFormatted
		{
			get
			{
				string taxName = string.IsNullOrWhiteSpace( this.ToponymName ) ? "" : this.ToponymName;
				if ( !string.IsNullOrWhiteSpace( this.Name ) )
				{
					return this.Name + ( ( taxName.ToLower() == this.Name.ToLower() || taxName == "" ) ? "" : " (" + taxName + ")" );
				}
				else
				{
					return "";
				}
			}
		}
		public string LocationFormatted { get { return string.IsNullOrWhiteSpace( this.Region ) ? this.Country : this.Region + ", " + this.Country; } }
	}
	//
}
