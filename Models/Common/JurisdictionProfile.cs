using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{

	public class JurisdictionProfile : BaseObject
	{
		public JurisdictionProfile()
		{
			MainJurisdiction = new GeoCoordinates();
			JurisdictionException = new List<GeoCoordinates>();
			JurisdictionAssertion = new Enumeration();
		}
		public string ParentType { get; set; }
		public int ParentTypeId { get; set; }

		/// <summary>
		/// Identifer of the parent Entity
		/// </summary>
		public Guid ParentEntityId { get; set; }
		public bool? IsGlobalJurisdiction { get; set; }
		/// <summary>
		/// Alias used for publishing
		/// </summary>
		public bool? GlobalJurisdiction {
			get { return IsGlobalJurisdiction; }
			set { IsGlobalJurisdiction = value; } } //

		//[Obsolete]
		//public bool IsOnlineJurisdiction { get; set; }
		public int JProfilePurposeId { get; set; }
		
		public string Description { get; set; }
		public string ProfileSummary { get; set; }
		public GeoCoordinates MainJurisdiction { get; set; }
		//public List<GeoCoordinates> MainJurisdictions { get; set; }
		public List<GeoCoordinates> Auto_MainJurisdiction { get
			{
				var result = new List<GeoCoordinates>();
				if( MainJurisdiction != null && MainJurisdiction.Id > 0 )
				{
					result.Add( MainJurisdiction );
				}
				return result;
			} }
		public List<GeoCoordinates> JurisdictionException { get; set; }

		public Enumeration JurisdictionAssertion { get; set; }
		public Organization AssertedByOrganization { get; set; }
		public Guid AssertedBy { get; set; }
		public List<Models.ProfileModels.TextValueProfile> Auto_AssertedBy
		{
			get
			{
				var result = new List<Models.ProfileModels.TextValueProfile>();
				if ( AssertedBy != null && AssertedBy != Guid.Empty )
				{
					result.Add( new Models.ProfileModels.TextValueProfile() { TextValue = Utilities.GetWebConfigValue( "credRegistryResourceUrl" ) + AssertedBy.ToString() } );
				}
				return result;
			}
		}
	}
	//
	
}
