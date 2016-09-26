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
using DBentity = Data.Organization_Address;
using DBentity2 = Data.Entity_Address;
using Entity = Models.Common.Address;

using Utilities;
using Models.Search.ThirdPartyApiModels;

namespace Factories
{
	public class AddressProfileManager : BaseFactory
	{
		static string thisClassName = "AddressProfileManager";

		#region Persistance - OrgAddress
		public bool Save( Entity entity, Guid parentUid, int userId, ref List<string> messages )
		{
			bool isValid = true;
			int intialCount = messages.Count;

			if ( !IsValidGuid( parentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}

			if ( messages.Count > intialCount )
				return false;

			int count = 0;

			DBentity efEntity = new DBentity();

			//*** don't have to use the entity summary here, but leaving in case we add other address implementations
			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.BaseId == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{

				if ( ValidateProfile( entity, ref  messages ) == false )
				{
					return false;
				}
				bool resetIsPrimaryFlag = false;

				if ( entity.Id == 0 )
				{
					//add
					efEntity = new DBentity();
					FromMap( entity, efEntity, ref resetIsPrimaryFlag );
					efEntity.OrgId = parent.BaseId;

					efEntity.Created = efEntity.LastUpdated = DateTime.Now;
					efEntity.CreatedById = efEntity.LastUpdatedById = userId;
					efEntity.RowId = Guid.NewGuid();

					context.Organization_Address.Add( efEntity );
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
							ResetPriorISPrimaryFlags( efEntity.OrgId, entity.Id );
						}
					}
				}
				else
				{
					entity.ParentId = parent.Id;

					efEntity = context.Organization_Address.SingleOrDefault( s => s.Id == entity.Id );
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
							ResetPriorISPrimaryFlags( efEntity.OrgId, entity.Id );
						}
					}
				}
			}

			return isValid;
}

		public bool SyncOldAddressToNew( Entity entity, Guid parentUid, int parentId, int userId, ref List<string> messages )
		{
			bool isValid = true;
			List<Entity> list = GetAllOrgAddresses( parentId );
			foreach ( Entity item in list )
			{
				//only check first one, or may need a consistancy check
				
				item.Address1 = entity.Address1;
				item.Address2 = entity.Address2;
				item.City = entity.City;
				item.AddressRegion = entity.AddressRegion;
				item.PostalCode = entity.PostalCode;
				item.Country = entity.Country;
				item.CountryNumber = entity.CountryNumber;

				isValid = Save( item, parentUid, userId, ref messages );

				break;
			}

			return isValid;
		}
		public bool ResetPriorISPrimaryFlags( int orgId, int newPrimaryProfileId )
		{
			bool isValid = true;
			string sql = string.Format( "UPDATE [dbo].[Organization.Address]   SET [IsPrimaryAddress] = 0 WHERE OrgId = {0} AND [IsPrimaryAddress] = 1  AND Id <> {1}", orgId, newPrimaryProfileId );
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
		public bool ValidateProfile( Entity profile, ref List<string> messages )
		{
			bool isValid = true;

			//check minimum
			if ( string.IsNullOrWhiteSpace( profile.Address1 )
			|| string.IsNullOrWhiteSpace( profile.City )
			|| string.IsNullOrWhiteSpace( profile.AddressRegion )
			|| string.IsNullOrWhiteSpace( profile.PostalCode )
				)
			{
				messages.Add( "Please enter a valid complete address" );
				isValid = false;
			}
			if ( string.IsNullOrWhiteSpace( profile.Name ) )
			{
				messages.Add( "A profile name must be entered" );
				isValid = false;
			}

			return isValid;
		}

		public bool Delete( int profileId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBentity p = context.Organization_Address.FirstOrDefault( s => s.Id == profileId );
				if ( p != null && p.Id > 0 )
				{
					context.Organization_Address.Remove( p );
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

		#endregion 

		#region Persistance - Entity Address
		public bool Entity_Address_Save( Entity entity, Guid parentUid, int userId, ref List<string> messages )
		{
			bool isValid = true;
			int intialCount = messages.Count;

			if ( !IsValidGuid( parentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}

			if ( messages.Count > intialCount )
				return false;

			int count = 0;

			DBentity2 efEntity = new DBentity2();

			//*** don't have to use the entity summary here, but leaving in case we add other address implementations
			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.BaseId == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{

				if ( ValidateProfile( entity, ref  messages ) == false )
				{
					return false;
				}
				bool resetIsPrimaryFlag = false;

				if ( entity.Id == 0 )
				{
					//add
					efEntity = new DBentity2();
					FromMap( entity, efEntity, ref resetIsPrimaryFlag );
					efEntity.EntityId = parent.Id;

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
							ResetPriorISPrimaryFlags( entity.ParentId, entity.Id );
						}
					}
				}
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

		#endregion 
		#region  retrieval ==================
		#region org address
		public static List<Entity> GetAllOrgAddresses( int parentId )
		{
			Entity entity = new Entity();
			List<Entity> list = new List<Entity>();
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBentity> results = context.Organization_Address
							.Where( s => s.OrgId == parentId )
							.OrderByDescending( s => s.IsPrimaryAddress )
							.ThenBy( s => s.Id)
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity item in results )
						{
							entity = new Entity();
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


		public static Entity GetOrganizationAddress( int profileId )
		{
			Entity entity = new Entity();
			try
			{

				using ( var context = new Data.CTIEntities() )
				{
					DBentity item = context.Organization_Address
							.SingleOrDefault( s => s.Id == profileId );

					if ( item != null && item.Id > 0 )
					{
						ToMap( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + string.Format(".Get. profileId: {0}", profileId) );
			}
			return entity;
		}//
	
		public static void ToMap( EM.Organization_Address from, CM.Address to )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.ParentId = from.OrgId;

			to.Name = from.Name;
			to.IsMainAddress = from.IsPrimaryAddress ?? false;
			to.Address1 = from.Address1;
			to.Address2 = from.Address2;
			to.City = from.City;
			to.PostalCode = from.PostalCode;
			to.AddressRegion = from.Region;
			to.Country = from.Country;
			to.CountryNumber = (int) (from.CountryId ?? 0);

			to.Latitude = from.Latitude ?? 0;
			to.Longitude = from.Longitude ?? 0;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

			

		}

		public static void FromMap( CM.Address from, EM.Organization_Address to, ref bool resetIsPrimaryFlag )
		{
			resetIsPrimaryFlag = false;
			//want to ensure fields from create are not wiped
			if ( to.Id == 0 )
			{
				if ( IsValidDate( from.Created ) )
					to.Created = from.Created;
				to.CreatedById = from.CreatedById;
			}
			//NOTE: the parentId - currently orgId, is handled in the update code
			to.Id = from.Id;
			to.Name = from.Name;
			//if this address is primary, and not previously primary, set indicator to reset existing settings
			//will need setting to default first address to primary if not entered
			if ( from.IsMainAddress && ( bool ) ( !to.IsPrimaryAddress ?? false ) )
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
			to.Address2 = from.Address2;
			to.City = from.City;
			to.PostalCode = from.PostalCode;
			to.Region = from.AddressRegion;
			to.Country = from.Country;

			//these will likely not be present? 
			//If new, or address has changed, do the geo lookup
			if ( hasAddress )
			{
				if ( hasChanged )
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
					}
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
		public static bool HasAddressChanged( CM.Address from, EM.Organization_Address to )
		{
			bool hasChanged = false;

			if ( to.Address1 != from.Address1
			|| to.Address2 != from.Address2
			|| to.City != from.City
			|| to.PostalCode != from.PostalCode
			|| to.Region != from.AddressRegion
			|| to.Country != from.Country )
				hasChanged = true;

			return hasChanged;
		}
		#endregion 

		#region  entity address
		public static List<Entity> GetAll( Guid parentUid )
		{
			Entity entity = new Entity();
			List<Entity> list = new List<Entity>();
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
							entity = new Entity();
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


		public static Entity Entity_Address_Get( int profileId )
		{
			Entity entity = new Entity();
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

			to.Name = from.Name;
			to.IsMainAddress = from.IsPrimaryAddress ?? false;
			to.Address1 = from.Address1;
			to.Address2 = from.Address2;
			to.City = from.City;
			to.PostalCode = from.PostalCode;
			to.AddressRegion = from.Region;
			to.Country = from.Country;
			to.CountryNumber = ( int ) ( from.CountryId ?? 0 );

			to.Latitude = from.Latitude ?? 0;
			to.Longitude = from.Longitude ?? 0;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;



		}

		public static void FromMap( CM.Address from, EM.Entity_Address to, ref bool resetIsPrimaryFlag )
		{
			resetIsPrimaryFlag = false;
			//want to ensure fields from create are not wiped
			if ( to.Id == 0 )
			{
				if ( IsValidDate( from.Created ) )
					to.Created = from.Created;
				to.CreatedById = from.CreatedById;
			}
			//NOTE: the parentId - currently orgId, is handled in the update code
			to.Id = from.Id;
			to.Name = from.Name;
			//if this address is primary, and not previously primary, set indicator to reset existing settings
			//will need setting to default first address to primary if not entered
			if ( from.IsMainAddress && ( bool ) ( !to.IsPrimaryAddress ?? false ) )
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
			to.Address2 = from.Address2;
			to.City = from.City;
			to.PostalCode = from.PostalCode;
			to.Region = from.AddressRegion;
			to.Country = from.Country;

			//these will likely not be present? 
			//If new, or address has changed, do the geo lookup
			if ( hasAddress )
			{
				if ( hasChanged )
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
					}
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
		public static bool HasAddressChanged( CM.Address from, EM.Entity_Address to )
		{
			bool hasChanged = false;

			if ( to.Address1 != from.Address1
			|| to.Address2 != from.Address2
			|| to.City != from.City
			|| to.PostalCode != from.PostalCode
			|| to.Region != from.AddressRegion
			|| to.Country != from.Country )
				hasChanged = true;

			return hasChanged;
		}
		#endregion
		#endregion
	}
}
