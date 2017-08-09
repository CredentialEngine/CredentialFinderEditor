using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;

using Models.Common;
using CM = Models.Common;
using MN = Models.Node;
using EM = Data;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
//using DBentity = Data.Organization_Address;
using DBentity2 = Data.Entity_Address;
using ThisEntity = Models.Common.Address;

using Utilities;
using Models.Search.ThirdPartyApiModels;

namespace Factories
{
	public class AddressProfileManager : BaseFactory
	{
		static string thisClassName = "AddressProfileManager";

		#region Persistance - Entity_Address
		public bool Save( ThisEntity entity, Guid parentUid, int userId, ref List<string> messages )
		{
			bool isValid = true;
			int intialCount = messages.Count;

			if ( !IsValidGuid( parentUid ) )
			{
				messages.Add( "Error: a valid parent identifier was not provided." );
			}

			if ( messages.Count > intialCount )
				return false;

			int count = 0;

			DBentity2 efEntity = new DBentity2();

			//*** don't have to use the entity summary here, but leaving in case we add other address implementations
			//Views.Entity_Summary parent1 = EntityManager.GetDBEntity( parentUid );
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.EntityBaseId == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}

			try
			{
				using ( var context = new Data.CTIEntities() )
				{

					if ( ValidateProfile( entity, parent, ref messages ) == false )
					{
						return false;
					}
					bool resetIsPrimaryFlag = false;

					if ( entity.Id == 0 )
					{
						//add
						efEntity = new DBentity2();
						efEntity.EntityId = parent.Id;
						entity.ParentId = parent.Id;
						FromMap( entity, efEntity, ref resetIsPrimaryFlag );


						efEntity.Created = efEntity.LastUpdated = DateTime.Now;
						efEntity.CreatedById = efEntity.LastUpdatedById = userId;
						efEntity.RowId = Guid.NewGuid();

						context.Entity_Address.Add( efEntity );
						count = context.SaveChanges();

						//update profile record so doesn't get deleted
						entity.Id = efEntity.Id;
						entity.ParentId = parent.Id;
						entity.RowId = efEntity.RowId;
						if ( count == 0 )
						{
							messages.Add( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.Name ) ? "no description" : entity.Name ) );
						}
						else
						{
							if ( resetIsPrimaryFlag )
							{
								Reset_Prior_ISPrimaryFlags( efEntity.EntityId, entity.Id );
							}
						}
					}
					else
					{
						entity.ParentId = parent.Id;

						efEntity = context.Entity_Address.SingleOrDefault( s => s.Id == entity.Id );
						if ( efEntity != null && efEntity.Id > 0 )
						{
							entity.RowId = efEntity.RowId;
							//update
							FromMap( entity, efEntity, ref resetIsPrimaryFlag );
							//has changed?
							if ( HasStateChanged( context ) )
							{
								efEntity.LastUpdated = System.DateTime.Now;
								efEntity.LastUpdatedById = userId;

								count = context.SaveChanges();
							}
							if ( resetIsPrimaryFlag )
							{
								Reset_Prior_ISPrimaryFlags( entity.ParentId, entity.Id );
							}
						}
					}
				}
			}
			catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
			{

				string message = HandleDBValidationError( dbex, thisClassName + ".Save() ", "Address Profile" );
				messages.Add( "Error - the save was not successful. " + message );
				LoggingHelper.LogError( dbex, thisClassName + string.Format( ".Save(), Parent: {0} ({1}), UserId: {2}", parent.EntityBaseName, parent.EntityBaseId, userId ) );
				isValid = false;
			}
			catch ( Exception ex )
			{
				string message = FormatExceptions( ex );
				messages.Add( "Error - the save was not successful. " + message );
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save(), Parent: {0} ({1}), UserId: {2}", parent.EntityBaseName, parent.EntityBaseId, userId ) );
				isValid = false;
			}
			return isValid;
		}

		public bool Reset_Prior_ISPrimaryFlags( int entityId, int newPrimaryProfileId )
		{
			bool isValid = true;
			string sql = string.Format( "UPDATE [dbo].[Entity.Address]   SET [IsPrimaryAddress] = 0 WHERE EntityId = {0} AND [IsPrimaryAddress] = 1  AND Id <> {1}", entityId, newPrimaryProfileId );
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Database.ExecuteSqlCommand( sql );
				}

				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, "AddressManager.ResetPriorISPrimaryFlags()" );
					isValid = false;
				}
			}
			return isValid;
		}
		public bool Entity_Address_Delete( int profileId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBentity2 p = context.Entity_Address.FirstOrDefault( s => s.Id == profileId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_Address.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "Requested record was not found: {0}", profileId );
					isOK = false;
				}
			}
			return isOK;

		}

		public bool ValidateProfile( ThisEntity profile, Entity parent, ref List<string> messages )
		{
			bool isValid = true;

			//check minimum
			if ( string.IsNullOrWhiteSpace( profile.Address1 )
			&& string.IsNullOrWhiteSpace( profile.PostOfficeBoxNumber )
				)
			{
				messages.Add( "Please enter at least Street Address 1 or a Post Office Box Number" );
			}
			if ( string.IsNullOrWhiteSpace( profile.City ) )
			{
				messages.Add( "Please enter a valid Locality/City" );
			}
			if ( string.IsNullOrWhiteSpace( profile.AddressRegion ) )
			{
				messages.Add( "Please enter a valid Region/State/Province" );
			}
			if ( string.IsNullOrWhiteSpace( profile.PostalCode )
			   )
			{
				messages.Add( "Please enter a valid Postal Code" );
			}
			if ( string.IsNullOrWhiteSpace( profile.Name ) )
			{
				//if for org, always default to org name
				if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_ORGANIZATION )
					profile.Name = parent.EntityBaseName;
				else
				{
					if ( profile.Id == 0 )
					{


					}
					//messages.Add( "A profile name must be entered" );
					//isValid = false;
					if ( !string.IsNullOrWhiteSpace( profile.City ) )
						profile.Name = profile.City;
					else if ( !string.IsNullOrWhiteSpace( profile.Address1 ) )
						profile.Name = profile.Address1;
					else
						profile.Name = "Main Address";
				}

			}

			if ( ( profile.Name ?? "").Length > 200 ) 
				messages.Add( "The address name must be less than 200 characters" );
			if ( ( profile.Address1 ?? "" ).Length > 200 )
				messages.Add( "The address1 must be less than 200 characters" );
			if ( ( profile.Address2 ?? "" ).Length > 200 )
				messages.Add( "The address2 must be less than 200 characters" );

			if ( messages.Count > 0 )
				isValid = false;
			return isValid;
		}
		#endregion
		#region  retrieval ==================
		#region org address OBSOLETE
		//public static List<ThisEntity> GetAllOrgAddresses( int parentId )
		//{
		//	ThisEntity entity = new ThisEntity();
		//	List<ThisEntity> list = new List<ThisEntity>();
		//	try
		//	{
		//		using ( var context = new Data.CTIEntities() )
		//		{
		//			List<DBentity> results = context.Organization_Address
		//					.Where( s => s.OrgId == parentId )
		//					.OrderByDescending( s => s.IsPrimaryAddress )
		//					.ThenBy( s => s.Id)
		//					.ToList();

		//			if ( results != null && results.Count > 0 )
		//			{
		//				foreach ( DBentity item in results )
		//				{
		//					entity = new ThisEntity();
		//					ToMap( item, entity );

		//					list.Add( entity );
		//				}
		//			}
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".GetAll" );
		//	}
		//	return list;
		//}//


		//public static ThisEntity GetOrganizationAddress( int profileId )
		//{
		//	ThisEntity entity = new ThisEntity();
		//	try
		//	{

		//		using ( var context = new Data.CTIEntities() )
		//		{
		//			DBentity item = context.Organization_Address
		//					.SingleOrDefault( s => s.Id == profileId );

		//			if ( item != null && item.Id > 0 )
		//			{
		//				ToMap( item, entity );
		//			}
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + string.Format(".Get. profileId: {0}", profileId) );
		//	}
		//	return entity;
		//}//

		//public static void ToMap( EM.Organization_Address from, CM.Address to )
		//{
		//	to.Id = from.Id;
		//	to.RowId = from.RowId;
		//	to.ParentId = from.OrgId;

		//	to.Name = from.Name;
		//	to.IsMainAddress = from.IsPrimaryAddress ?? false;
		//	to.Address1 = from.Address1;
		//	to.Address2 = from.Address2;
		//	to.City = from.City;
		//	to.PostalCode = from.PostalCode;
		//	to.AddressRegion = from.Region;
		//	//to.Country = from.Country;
		//	to.CountryId = (int) (from.CountryId ?? 0);
		//	if ( from.Codes_Countries != null )
		//	{
		//		to.Country = from.Codes_Countries.CommonName;
		//	}

		//	to.Latitude = from.Latitude ?? 0;
		//	to.Longitude = from.Longitude ?? 0;

		//	if ( IsValidDate( from.Created ) )
		//		to.Created = ( DateTime ) from.Created;
		//	to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
		//	if ( IsValidDate( from.LastUpdated ) )
		//		to.LastUpdated = ( DateTime ) from.LastUpdated;
		//	to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;



		//}

		//public static void FromMap( CM.Address from, EM.Organization_Address to, ref bool resetIsPrimaryFlag )
		//{
		//	resetIsPrimaryFlag = false;
		//	//want to ensure fields from create are not wiped
		//	if ( to.Id == 0 )
		//	{
		//		if ( IsValidDate( from.Created ) )
		//			to.Created = from.Created;
		//		to.CreatedById = from.CreatedById;
		//	}
		//	//NOTE: the parentId - currently orgId, is handled in the update code
		//	to.Id = from.Id;
		//	to.Name = from.Name;
		//	//if this address is primary, and not previously primary, set indicator to reset existing settings
		//	//will need setting to default first address to primary if not entered
		//	if ( from.IsMainAddress && ( bool ) ( !to.IsPrimaryAddress ?? false ) )
		//	{
		//		//initially attempt to only allow adding new primary,not unchecking
		//		resetIsPrimaryFlag = true;

		//	}
		//	to.IsPrimaryAddress = from.IsMainAddress;

		//	bool hasChanged = false;
		//	bool hasAddress = false;

		//	if ( from.HasAddress() )
		//	{
		//		hasAddress = true;
		//		if ( to.Latitude == null || to.Latitude == 0 )
		//			hasChanged = true;
		//	}
		//	if ( hasChanged == false )
		//	{
		//		if ( to.Id == 0 )
		//			hasChanged = true;
		//		else
		//			hasChanged = HasAddressChanged( from, to );
		//	}

		//	to.Address1 = from.Address1;
		//	to.Address2 = from.Address2;
		//	to.City = from.City;
		//	to.PostalCode = from.PostalCode;
		//	to.Region = from.AddressRegion;
		//	//to.Country = from.Country;
		//	if ( from.CountryId == 0 )
		//		to.CountryId = null;
		//	else
		//		to.CountryId = from.CountryId;

		//	//these will likely not be present? 
		//	//If new, or address has changed, do the geo lookup
		//	if ( hasAddress )
		//	{
		//		if ( hasChanged )
		//		{
		//			GoogleGeocoding.Results results = GeoServices.GeocodeAddress( from.DisplayAddress() );
		//			if ( results != null )
		//			{
		//				GoogleGeocoding.Location location = results.GetLocation();
		//				if ( location != null )
		//				{
		//					to.Latitude = location.lat;
		//					to.Longitude = location.lng;
		//				}
		//			}
		//		}
		//	}
		//	else
		//	{
		//		to.Latitude = 0;
		//		to.Longitude = 0;
		//	}

		//	//these will be set in the update code anyway
		//	if ( IsValidDate( from.LastUpdated ) )
		//		to.LastUpdated = from.LastUpdated;
		//	to.LastUpdatedById = from.LastUpdatedById;
		//}
		//public static bool HasAddressChanged( CM.Address from, EM.Organization_Address to )
		//{
		//	bool hasChanged = false;

		//	if ( to.Address1 != from.Address1
		//	|| to.Address2 != from.Address2
		//	|| to.City != from.City
		//	|| to.PostalCode != from.PostalCode
		//	|| to.Region != from.AddressRegion
		//	|| to.CountryId != from.CountryId )
		//		hasChanged = true;

		//	return hasChanged;
		//}
		#endregion

		#region  entity address
		public static List<ThisEntity> GetAll( Guid parentUid )
		{
			ThisEntity entity = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<EM.Entity_Address> results = context.Entity_Address
							.Where( s => s.Entity.EntityUid == parentUid )
							.OrderByDescending( s => s.IsPrimaryAddress )
							.ThenBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( EM.Entity_Address item in results )
						{
							entity = new ThisEntity();
							ToMap( item, entity );

							list.Add( entity );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".GetAll" );
			}
			return list;
		}//


		public static ThisEntity Entity_Address_Get( int profileId )
		{
			ThisEntity entity = new ThisEntity();
			try
			{

				using ( var context = new Data.CTIEntities() )
				{
					DBentity2 item = context.Entity_Address
							.SingleOrDefault( s => s.Id == profileId );

					if ( item != null && item.Id > 0 )
					{
						ToMap( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".Entity_Address_Get. profileId: {0}", profileId ) );
			}
			return entity;
		}//
		public static void ToMap( EM.Entity_Address from, CM.Address to )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.ParentId = from.EntityId;
			if ( from.Entity != null )
				to.ParentRowId = from.Entity.EntityUid;

			to.Name = from.Name;
			to.IsMainAddress = from.IsPrimaryAddress ?? false;
			to.Address1 = from.Address1;
			to.Address2 = from.Address2 ?? "";
			to.PostOfficeBoxNumber = from.PostOfficeBoxNumber ?? "";
			to.City = from.City;
			to.PostalCode = from.PostalCode;
			to.AddressRegion = from.Region;
			//to.Country = from.Country;
			to.CountryId = ( int ) ( from.CountryId ?? 0 );
			if ( from.Codes_Countries != null )
			{
				to.Country = from.Codes_Countries.CommonName;
			}
			to.Latitude = from.Latitude ?? 0;
			to.Longitude = from.Longitude ?? 0;

			to.ContactPoint = Entity_ContactPointManager.GetAll( to.RowId );

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

			to.LastUpdatedBy = SetLastUpdatedBy( to.LastUpdatedById, from.Account_Modifier );


		}

		public static void FromMap( CM.Address from, EM.Entity_Address to, ref bool resetIsPrimaryFlag )
		{
			resetIsPrimaryFlag = false;
			//want to ensure fields from create are not wiped
			//if ( to.Id == 0 )
			//{
			//	if ( IsValidDate( from.Created ) )
			//		to.Created = from.Created;
			//	to.CreatedById = from.CreatedById;
			//}
			//NOTE: the parentId - currently orgId, is handled in the update code
			to.Id = from.Id;
			to.Name = from.Name;
			//if this address is primary, and not previously primary, set indicator to reset existing settings
			//will need setting to default first address to primary if not entered
			if ( from.IsMainAddress && ( bool ) ( !( to.IsPrimaryAddress ?? false ) ) )
			{
				//initially attempt to only allow adding new primary,not unchecking
				resetIsPrimaryFlag = true;

			}
			to.IsPrimaryAddress = from.IsMainAddress;

			bool hasChanged = false;
			bool hasAddress = false;

			if ( from.HasAddress() )
			{
				hasAddress = true;
				if ( to.Latitude == null || to.Latitude == 0 )
					hasChanged = true;
			}
			if ( hasChanged == false )
			{
				if ( to.Id == 0 )
					hasChanged = true;
				else
					hasChanged = HasAddressChanged( from, to );
			}

			to.Address1 = from.Address1;
			to.Address2 = GetData( from.Address2, null );
			to.PostOfficeBoxNumber = GetData( from.PostOfficeBoxNumber, null );
			to.City = from.City;
			to.PostalCode = from.PostalCode;
			to.Region = from.AddressRegion;
			//to.Country = from.Country;
			if ( from.CountryId == 0 )
				to.CountryId = null;
			else
				to.CountryId = from.CountryId;

			//these will likely not be present? 
			//If new, or address has changed, do the geo lookup
			if ( hasAddress )
			{
				if ( hasChanged )
				{
					UpdateGeo( from, to );
					//GoogleGeocoding.Results results = GeoServices.GeocodeAddress( from.DisplayAddress() );
					//if ( results != null )
					//{
					//	GoogleGeocoding.Location location = results.GetLocation();
					//	if ( location != null )
					//	{
					//		to.Latitude = location.lat;
					//		to.Longitude = location.lng;
					//	}
					//	int pIdx = results.results[ 0 ].address_components.Count - 1;
					//	int cIdx = results.results[ 0 ].address_components.Count - 2;
					//	string postalCode = results.results[ 0 ].address_components[ pIdx ].short_name;
					//	string country = results.results[ 0 ].address_components[ cIdx ].long_name;
					//	if ( string.IsNullOrEmpty( to.PostalCode ) ||
					//		to.PostalCode != postalCode)
					//	{
					//		//?not sure if should assume the google result is accurate
					//		//to.PostalCode = postalCode;
					//		//what about country?
					//	}
					//	if ( !string.IsNullOrEmpty( country ) &&
					//		to.CountryId == null )
					//	{
					//		//?not sure if should assume the google result is accurate
					//		to.Country = country;
					//		//do lookup
					//	}
					//}
				}
			}
			else
			{
				to.Latitude = 0;
				to.Longitude = 0;
			}

			//these will be set in the update code anyway
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById;
		}
		public static void UpdateGeo( CM.Address from, EM.Entity_Address to )
		{
			GoogleGeocoding.Results results = GeoServices.GeocodeAddress( from.DisplayAddress() );
			if ( results != null )
			{
				GoogleGeocoding.Location location = results.GetLocation();
				if ( location != null )
				{
					to.Latitude = location.lat;
					to.Longitude = location.lng;
				}
				try
				{
					if ( results.results.Count > 0 )
					{
						int pIdx = results.results[ 0 ].address_components.Count - 1;
						int cIdx = results.results[ 0 ].address_components.Count - 2;
						string postalCode = results.results[ 0 ].address_components[ pIdx ].short_name;
						string country = results.results[ 0 ].address_components[ cIdx ].long_name;
						if ( string.IsNullOrEmpty( to.PostalCode ) ||
							to.PostalCode != postalCode )
						{
							//?not sure if should assume the google result is accurate
							//to.PostalCode = postalCode;
							//what about country?
						}
						if ( !string.IsNullOrEmpty( country ) &&
							to.CountryId == null )
						{
							//set country string, and perhaps plan update process.
							to.Country = country;
							//do lookup, OR at least notify for now
							//probably should make configurable - or spin off process to attempt update
							EmailManager.NotifyAdmin( "CTI Missing country to update", string.Format( "Address without country entered, but resolved via GoogleGeocoding.Location. entity.ParentId: {0}, country: {1}", from.ParentId, country ) );
						}
					}

				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + "UpdateGeo" );
				}
			}
		}
		public static bool HasAddressChanged( CM.Address from, EM.Entity_Address to )
		{
			bool hasChanged = false;

			if ( to.Address1 != from.Address1
			|| to.Address2 != from.Address2
			|| to.City != from.City
			|| to.PostalCode != from.PostalCode
			|| to.Region != from.AddressRegion
			|| to.CountryId != from.CountryId )
				hasChanged = true;

			return hasChanged;
		}
		#endregion

		public static List<string> Autocomplete( string keyword, int typeId, int maxTerms = 25 )
		{
			int pTotalRows = 0;
			List<string> results = new List<string>();
			string address1 = "";
			string city = "";
			string postalCode = "";
			if ( typeId == 3 )
				postalCode = keyword;
			else if ( typeId == 2 )
				city = keyword;
			else
				address1 = keyword;
			string result = "";

			List<ThisEntity> list = QuickSearch( address1, city, postalCode, 1, maxTerms, ref pTotalRows );

			string prevName = "";
			string suffix = "";
			foreach ( ThisEntity item in list )
			{
				result = "";
				suffix = "";

				if ( typeId == 3 )
				{
					result = item.PostalCode;
					suffix = " [[" + item.City + "]] ";
				}
				else if ( typeId == 2 )
				{
					result = item.City;
				}
				else
				{
					result = item.Address1;
					suffix = " [[" + item.City + "]] ";
				}

				if ( result.ToLower() != prevName )
					results.Add( result + suffix );

				prevName = result.ToLower();
			}

			return results;
		}
		public static List<ThisEntity> QuickSearch( string keyword, int pageNumber, int pageSize, ref int pTotalRows )
		{
			List<ThisEntity> list = new List<ThisEntity>();
			ThisEntity entity = new ThisEntity();
			keyword = CleanTerm( keyword );

			int skip = ( pageNumber - 1 ) * pageSize;
			using ( var context = new Data.CTIEntities() )
			{
				//turn off - so Entity et will not be included!!!!
				context.Configuration.LazyLoadingEnabled = false;


				var addresses = context.Entity_Address
					.Where( s => keyword == ""
								|| s.Entity.EntityBaseName.Contains( keyword )
								|| s.Address1.Contains( keyword )
								|| s.City.Contains( keyword )
								|| s.PostalCode.Contains( keyword )
								)
					.GroupBy( a => new
					{
						Name = a.Name,
						Address1 = a.Address1,
						Address2 = a.Address2 ?? "",
						City = a.City,
						PostalCode = a.PostalCode,
						AddressRegion = a.Region,
						CountryId = a.CountryId,
						Country = a.Country ?? ""
					} )
					.Select( g => new ThisEntity
					{
						Name = g.Key.Name,
						Address1 = g.Key.Address1,
						Address2 = g.Key.Address2 ?? "",
						City = g.Key.City,
						PostalCode = g.Key.PostalCode,
						AddressRegion = g.Key.AddressRegion,
						CountryId = g.Key.CountryId ?? 0,
						Country = g.Key.Country
					} )
					.OrderByDescending( a => a.Address1 )
					.ThenByDescending( a => a.City );
				//.ToList();


				pTotalRows = addresses.Count();
				List<ThisEntity> results = addresses
					.OrderBy( s => s.Address1 )
					.Skip( skip )
					.Take( pageSize )
					.ToList();
				if ( results != null && results.Count > 0 )
				{
					//??enough
					list = results;
					//return list;
				}

				//var addresses2 = from address in context.Entity_Address
				//			.Where( s => keyword == ""
				//				|| s.Entity.EntityBaseName.Contains( keyword )
				//				|| s.Address1.Contains( keyword )
				//				|| s.City.Contains( keyword )
				//				|| s.PostalCode.Contains( keyword )
				//				)
				//				 select address;
				//pTotalRows = addresses2.Count();
				//List<DBentity2> results2 = addresses2
				//	.OrderBy( s => s.Address1 )
				//	.Skip( skip )
				//	.Take( pageSize )
				//	.ToList();

				//if ( results2 != null && results2.Count > 0 )
				//{
				//	foreach ( DBentity2 item in results2 )
				//	{
				//		entity = new ThisEntity();
				//		ToMap( item, entity );

				//		list.Add( entity );
				//	}
				//}
			}

			return list;
		}
		public static List<ThisEntity> QuickSearch( string address1, string city, string postalCode, int pageNumber, int pageSize, ref int pTotalRows )
		{
			List<ThisEntity> list = new List<ThisEntity>();
			ThisEntity entity = new ThisEntity();
			address1 = CleanTerm( address1 );
			city = CleanTerm( city );
			postalCode = CleanTerm( postalCode );

			int skip = ( pageNumber - 1 ) * pageSize;
			using ( var context = new Data.CTIEntities() )
			{

				List<DBentity2> results = context.Entity_Address
					.Where( s =>
						   ( address1 == "" || s.Address1.Contains( address1 ) )
						&& ( city == "" || s.City.Contains( city ) )
						&& ( postalCode == "" || s.PostalCode.Contains( postalCode ) )
						)
					.OrderBy( s => s.Address1 )
					.Skip( skip )
					.Take( pageSize )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( DBentity2 item in results )
					{
						entity = new ThisEntity();
						ToMap( item, entity );

						list.Add( entity );
					}

					//Other parts
				}
			}

			return list;
		}
		private static string CleanTerm( string item )
		{
			string term = item == null ? "" : item.Trim();
			if ( term.IndexOf( "[[" ) > 1 )
			{
				term = term.Substring( 0, term.IndexOf( "[[" ) );
				term = term.Trim();
			}
			return term;
		}
		#endregion
	}
}
