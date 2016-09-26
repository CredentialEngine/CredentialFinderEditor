using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{

	public class GeoCoordinates : BaseObject
	{
		//public new Guid ParentId { get; set; } //Hides integer ParentId. Should perhaps be called ParentGuid?
		//public int Id { get; set; }
		public GeoCoordinates()
		{
			Name = "";
			ToponymName = "";
			Region = "";
			Country = "";
			Bounds = new BoundingBox();
			//Address = new Address();  //Do not initialize this here, it will cause an infinite recursive loop with the constructor of GeoCoordinates
		}

		public Guid ParentEntityId { get; set; }
		public int GeoNamesId { get; set; } //ID used by GeoNames.org
		public string Name { get; set; }
		public bool IsException { get; set; }
		public Address Address { get; set; }

		public string ToponymName { get; set; }
		public string Region { get; set; }
		public string Country { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public string Url { get; set; } //URL of a geonames place
		public string TitleFormatted 
		{
			get
			{
				string taxName = string.IsNullOrWhiteSpace( this.ToponymName ) ? "" : this.ToponymName;
				if ( !string.IsNullOrWhiteSpace( this.Name ) )
				{
					return this.Name + ( (taxName.ToLower() == this.Name.ToLower() || taxName == "") ? "" : " (" + taxName + ")" );
				}
				else
				{
					return "";

				}
			}
		}
		public string LocationFormatted { get { return string.IsNullOrWhiteSpace( this.Region ) ? this.Country : this.Region + ", " + this.Country; } }

		public string ProfileSummary { get; set; }
		public BoundingBox Bounds { get; set; }
	}
	//

	public class BoundingBox
	{
		public bool IsDefined { get { return !( North == 0 && South == 0 && East == 0 && West == 0 ); } }
		public decimal North { get; set; }
		public decimal South { get; set; }
		public decimal East { get; set; }
		public decimal West { get; set; }
	}
	//
}
