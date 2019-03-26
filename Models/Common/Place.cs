using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{

	public class Place : BaseObject
	{
		public Place()
		{
			TargetContactPoint = new List<ContactPoint>();
		}


		public string GeoURI { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }
		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string StreetAddress { get { return Address1 + ( string.IsNullOrWhiteSpace( Address2 ) ? "" : " " + Address2 ); } set { Address1 = value; } } //Can't determine address1 vs address2

		public string PostOfficeBoxNumber { get; set; }

		public string City { get; set; }
		public string AddressLocality { get { return City; } set { City = value; } } //Alias used for publishing

		public string AddressRegion { get; set; }
		public string PostalCode { get; set; }

		public string Country { get; set; }
		public string AddressCountry { get { return Country; } set { Country = value; } } //Alias used for publishing
		public int CountryId { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public List<ContactPoint> TargetContactPoint { get; set; }

		//helpers
		public bool IsMainAddress { get; set; }
		public Guid ParentRowId { get; set; }
		public string DisplayAddress( string separator = ", " )
		{
			string address = "";
			if ( !string.IsNullOrWhiteSpace( Address1 ) )
				address = Address1;
			if ( !string.IsNullOrWhiteSpace( Address2 ) )
				address += separator + Address2;
			if ( !string.IsNullOrWhiteSpace( City ) )
				address += separator + City;
			if ( !string.IsNullOrWhiteSpace( AddressRegion ) )
				address += separator + AddressRegion;
			if ( !string.IsNullOrWhiteSpace( PostalCode ) )
				address += " " + PostalCode;
			if ( !string.IsNullOrWhiteSpace( Country ) )
				address += separator + Country;
			return address;
		}
		public bool HasAddress()
		{
			bool hasAddress = true;

			if ( string.IsNullOrWhiteSpace( Address1 )
			&& string.IsNullOrWhiteSpace( Address2 )
			&& string.IsNullOrWhiteSpace( City )
			&& string.IsNullOrWhiteSpace( AddressRegion )
			&& string.IsNullOrWhiteSpace( PostalCode )
				)
				hasAddress = false;



			return hasAddress;
		}
	}

}
