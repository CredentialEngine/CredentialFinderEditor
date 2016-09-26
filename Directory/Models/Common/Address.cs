using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{
	/*
	public class Address
	{
		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string City { get; set; }
		public string Region { get; set; }
		public string State
		{
			get { return Region; }
			set { Region = value; }
		}
		public string Country { get; set; }
		public string PostalCode { get; set; }
		
		//public int ZipcodeMain { get; set; }
		//public int ZipcodeExtension { get; set; }
	}
	//
	*/

	public class Address : BaseObject
	{
		public Address()
		{
			GeoCoordinates = new GeoCoordinates();
		}
		public string Name { get; set; }
		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string City { get; set; }
		public string AddressRegion { get; set; }
		public string State 
		{
			get { return AddressRegion; }
			set { AddressRegion = value; }
		}
		public string StreetAddress { get { return Address1 + ( string.IsNullOrWhiteSpace( Address2 ) ? "" : " " + Address2 ); } }
		public string Country { get; set; }
		public int CountryId { get; set; }
		public string PostalCode { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public bool IsMainAddress { get; set; }
		public string DisplayAddress(string separator = ", ")
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
		/// <summary>
		/// Note: the GeoCoordinates use the rowId from the parent for the FK. If the parent of the address object can have other regions, then there will be a problem!
		/// This may lead to the addition of concrete rowIds as needed to a parent with an address.
		/// </summary>
		public GeoCoordinates GeoCoordinates { get; set; }
	}
	//

}
