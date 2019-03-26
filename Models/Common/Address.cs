using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{

    public class Location : Address
    {
        public int OrganizationId { get; set; }
    }

    public class Address : BaseObject
    {
        public Address()
        {
            GeoCoordinates = new GeoCoordinates();
            ContactPoint = new List<ContactPoint>();
        }
        public string Name { get; set; }
        //in BaseObject
        //public string ExternalIdentifier { get; set; }
        public string Description { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string PostOfficeBoxNumber { get; set; }

        public string City { get; set; }
        public string AddressLocality { get { return City; } set { City = value; } } //Alias used for publishing
        public string AddressRegion { get; set; }
        public string State
        {
            get { return AddressRegion; }
            set { AddressRegion = value; }
        }
        public string StreetAddress { get { return Address1 + ( string.IsNullOrWhiteSpace( Address2 ) ? "" : " " + Address2 ); } set { Address1 = value; } } //Can't determine address1 vs address2
        public string Country { get; set; }
        public string AddressCountry { get { return Country; } set { Country = value; } } //Alias used for publishing
        public int CountryId { get; set; }
        public string PostalCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
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
        public string LooseDisplayAddress( string separator = ", " ) //For easier geocoding
        {
            return
                ( string.IsNullOrWhiteSpace( City ) ? "" : City + separator ) +
                ( string.IsNullOrWhiteSpace( AddressRegion ) ? "" : AddressRegion + separator ) +
                ( string.IsNullOrWhiteSpace( PostalCode ) ? "" : PostalCode + " " ) +
                ( string.IsNullOrWhiteSpace( Country ) ? "" : Country );
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
        public bool HasLatLng()
        {
            if ( Latitude != 0 && Longitude != 0 )
                return true;
            else
                return false;
        }
        /// <summary>
        /// Note: the GeoCoordinates use the rowId from the parent for the FK. If the parent of the address object can have other regions, then there will be a problem!
        /// This may lead to the addition of concrete rowIds as needed to a parent with an address.
        /// </summary>
        public GeoCoordinates GeoCoordinates { get; set; }


        public List<ContactPoint> ContactPoint { get; set; }
        public List<ContactPoint> Auto_TargetContactPoint { get { return ContactPoint; } } //Alias used for publishing
    }
    //

	public class Entity_Location
	{
		public int Id { get; set; }
		public int EntityId { get; set; }
		public int LocationId { get; set; }
		public DateTime Created { get; set; }
		public int CreatedById { get; set; }
		public List<Entity_LocationContactPoint> Contacts { get; set; } = new List<Entity_LocationContactPoint>();

		public virtual Entity Entity { get; set; }
		public virtual Location Location { get; set; }
	}
	public partial class Entity_LocationContactPoint
	{
		public int Id { get; set; }
		public int EntityLocationId { get; set; }
		public int EntityContactPointId { get; set; }
		public DateTime Created { get; set; }
		public int CreatedById { get; set; }

		public virtual ContactPoint Entity_ContactPoint { get; set; }
		public virtual Entity_Location Entity_Location { get; set; }

	}
}
