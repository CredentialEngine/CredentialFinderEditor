using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RAPlace = RA.Models.Input.Place;
//using RAPlace = RA.Models.Input.GeoCoordinates;
namespace RA.Models.Input
{
	public class Jurisdiction
	{
        public Jurisdiction()
        {
			MainJurisdiction = new RAPlace();
			JurisdictionException = new List<RAPlace>();
        }
		public bool? GlobalJurisdiction { get; set; }
		public string Description { get; set; }

		/// <summary>
		/// TBD - does it make sense to offer providing the full GeoCoordinates.
		/// Will be useful where the request can be populated programatically.
		/// </summary>
		public RAPlace MainJurisdiction { get; set; }

		//public List<string> JurisdictionException { get; set; }
		public List<RAPlace> JurisdictionException { get; set; }
	}

	public class JurisdictionAssertedInProfile 
	{
		public JurisdictionAssertedInProfile ()
		{
			AssertedBy = new OrganizationReference();
			Jurisdiction = new Jurisdiction();
		}
		public Jurisdiction Jurisdiction { get; set; }
		/// <summary>
		/// Organization that asserts this condition
		/// Required
		/// </summary>
		public OrganizationReference AssertedBy { get; set; }

		//assertion types
		//at least one assertion must be selected

		/// <summary>
		/// Organization asserts the related resource is accredited in the referenced jurisdiction
		/// </summary>
		public bool AssertsAccreditedIn { get; set; }
		public bool AssertsApprovedIn { get; set; }
		public bool AssertsOfferedIn { get; set; }
		public bool AssertsRecognizedIn { get; set; }
		public bool AssertsRegulatedIn { get; set; }
        public bool AssertsRenewedIn { get; set; }
		public bool AssertsRevokedIn { get; set; }

	}

	public class GeoCoordinates
	{
		public GeoCoordinates()
		{
			Name = "";
			//ToponymName = "";
			Region = "";
			Country = "";
		//	Address = null;
		//	Bounds = null;

		}

		public int GeoNamesId { get; set; } //ID used by GeoNames.org
		public string Name { get; set; }
		public bool IsException { get; set; }

		//public string ToponymName { get; set; }
		public string Region { get; set; }
		public string Country { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }

		//public PostalAddress Address { get; set; }

		/// <summary>
		/// ceterms:geoURI
		/// Entity that describes the longitude, latitude and other location details of a place.
		/// </summary>
		public string GeoUri { get; set; } 

		//public BoundingBox Bounds { get; set; }
	}

	public class BoundingBox
	{
		public bool? IsDefined { get { return !( North == 0 && South == 0 && East == 0 && West == 0 ); } }
		public decimal North { get; set; }
		public decimal South { get; set; }
		public decimal East { get; set; }
		public decimal West { get; set; }
	}
}
