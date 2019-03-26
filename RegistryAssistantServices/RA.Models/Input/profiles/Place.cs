using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Models.Input
{
	public class Place
	{
		public Place()
		{
			ContactPoint = new List<Input.ContactPoint>();
		}


		public string Name { get; set; }

		public string Description { get; set; }

		public string Address1 { get; set; }
		public string Address2 { get; set; }

		public string PostOfficeBoxNumber { get; set; }

		public string City { get; set; }

		public string AddressRegion { get; set; }
		public string PostalCode { get; set; }

		public string Country { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public string GeoURI { get; set; }
		public List<ContactPoint> ContactPoint { get; set; }
	}
}
