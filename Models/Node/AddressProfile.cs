using System.Collections.Generic;

namespace Models.Node
{

	[Profile( DBType = typeof( Models.Common.Address ) )]
	public class AddressProfile : BaseProfile
	{
		[Property( DBName = "Name" )]
		public override string Name { get; set; }
		public bool IsMainAddress { get; set; }
		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string PostOfficeBoxNumber { get; set; }
		public string City { get; set; }

		[Property( DBName = "AddressRegion" )]
		public string Region { get; set; } //State, Province, etc.
		public int CountryId { get; set; }
		//leave, as used with DisplayAddress (and is populated in factory)
		public string Country { get; set; }
		public string PostalCode { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }


		[Property( DBName = "ContactPoint", DBType = typeof( Models.Common.ContactPoint ) )]
		public List<ProfileLink> ContactPoint { get; set; }

		public string DisplayAddress( string separator = ", " )
		{
			var parts = new List<string>() { Address1 ?? "", Address2 ?? "", City ?? "", Region ?? "", PostalCode ?? "", Country ?? "" };
			var joined = string.Join( separator, parts );
			if ( !string.IsNullOrWhiteSpace( PostalCode ) )
			{
				joined = joined.Replace( PostalCode + separator, PostalCode + " " );
			}
			return joined;
		}

		public bool HasAddress()
		{
			return !( string.IsNullOrWhiteSpace( Address1 )
				&& string.IsNullOrWhiteSpace( Address2 )
				&& string.IsNullOrWhiteSpace( City )
				&& string.IsNullOrWhiteSpace( Region )
				&& string.IsNullOrWhiteSpace( PostalCode )
			);
		}

	}
}
