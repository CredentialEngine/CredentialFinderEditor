using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CM = Models.Common;
using EM = Data;
using Utilities;
using Data;
using DBentity = Data.Entity_JurisdictionProfile;
using ThisEntity = Models.Common.JurisdictionProfile;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class Entity_JurisdictionProfileManager : BaseFactory
	{
		static string thisClassName = "Entity_JurisdictionProfileManager";
		//static string DEFAULT_GUID = "00000000-0000-0000-0000-000000000000";
		public static int JURISDICTION_PURPOSE_SCOPE = 1;
		public static int JURISDICTION_PURPOSE_RESIDENT = 2;

		#region JurisdictionProfile  =======================
		#region JurisdictionProfile Core  =======================

		public bool JurisdictionProfile_Add( ThisEntity entity, ref List<String> messages )
		{
			bool isValid = true;

			using ( var context = new Data.CTIEntities() )
			{
				if ( entity == null || !IsValidGuid( entity.ParentEntityId ) )
				{
					messages.Add( "Error - missing an identifier for the JurisdictionProfile" );
					return false;
				}

				//ensure we have a parentId/EntityId
				//Views.Entity_Summary parent = EntityManager.GetDBEntity( entity.ParentId );
				Views.Entity_Summary parent = EntityManager.GetDBEntity( entity.ParentEntityId );
				//Entity parent = EntityManager.GetEntity(entity.ParentEntityId  );
				if ( parent == null || parent.Id == 0 )
				{
					messages.Add( "Error - the parent entity was not found." );
					return false;
				}


				//will be obsolete!!!!
				entity.ParentTypeId = parent.EntityTypeId;
				//check for Empty
				//==> not sure what is the minimum required fields!
				if ( !IsEmpty( entity ) )
				{
					//prob not necessary, check
					entity.ParentEntityId = parent.EntityUid;

					//id = JurisdictionProfile_Add( entity, ref messages );
					DBentity efEntity = new DBentity();
					FromMap( entity, efEntity );
					efEntity.EntityId = parent.Id;
					efEntity.RowId = Guid.NewGuid();
					entity.RowId = efEntity.RowId;

					if ( efEntity.JProfilePurposeId == null || efEntity.JProfilePurposeId == 0 )
						efEntity.JProfilePurposeId = 1;

					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdated = System.DateTime.Now;

					context.Entity_JurisdictionProfile.Add( efEntity );

					int count = context.SaveChanges();
					if ( count > 0 )
					{
						entity.Id = efEntity.Id;
						UpdateJPRegions( entity, entity.LastUpdatedById, ref  messages );
					}
				}
				else
				{
					messages.Add( "You must enter a description." );
				}

			}

			return isValid;
		}
		public bool JurisdictionProfile_Update( ThisEntity entity, ref List<String> messages )
		{
			bool isValid = true;

			using ( var context = new Data.CTIEntities() )
			{
				if ( entity == null || entity.Id == 0 || !IsValidGuid( entity.ParentEntityId ) )
				{
					messages.Add( "Error - missing an identifier for the JurisdictionProfile" );
					return false;
				}

				Views.Entity_Summary parent = EntityManager.GetDBEntity( entity.ParentEntityId );
				if ( parent == null || parent.Id == 0 )
				{
					messages.Add( "Error - the parent entity was not found." );
					return false;
				}


				DBentity efEntity =
					context.Entity_JurisdictionProfile.SingleOrDefault( s => s.Id == entity.Id );

				entity.RowId = efEntity.RowId;
				FromMap( entity, efEntity );

				if ( HasStateChanged( context ) )
				{
					efEntity.LastUpdated = DateTime.Now;
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					}
				}
			}

			return isValid;
		}
		public bool UpdateJPRegions( ThisEntity entity, int userId, ref List<String> messages )
		{
			bool isValid = true;
			List<CM.GeoCoordinates> list = new List<CM.GeoCoordinates>();
			if ( entity.MainJurisdiction != null && entity.MainJurisdiction.GeoNamesId > 0 )
			{
				list.Add( entity.MainJurisdiction );
				isValid = GeoCoordinate_Update( list, entity.RowId, userId, false, ref messages );
				//do exceptions
				if ( GeoCoordinate_Update( entity.JurisdictionException, entity.RowId, userId, true, ref messages ) == false )
				{
					isValid = false;
				}
			}
			else
			{
				//can't have exceptions (what if exceptions are to world wide, or to text?
				if ( entity.JurisdictionException != null && entity.JurisdictionException.Count() > 0 )
				{
					isValid = false;
					messages.Add( "Error: you must have a main region before entering exceptions" );
				}
			}


			return isValid;
		}
		/// <summary>
		/// May want to use an isLast check to better handle an empty object
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool IsEmpty( ThisEntity entity, bool isLastItem = false )
		{
			bool isEmpty = false;
			//this will be problematic as the two bools default to false
			//radio buttons?
			if ( string.IsNullOrWhiteSpace( entity.Description )
				&& ( entity.MainJurisdiction == null || entity.MainJurisdiction.GeoNamesId == 0 )
				&& ( entity.JurisdictionException == null || entity.JurisdictionException.Count == 0 )
				)
				return true;

			return isEmpty;
		}
		/// <summary>
		/// Delete a JurisdictionProfile
		/// </summary>
		/// <param name="Id"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool JurisdictionProfile_Delete( int Id, ref string statusMessage )
		{
			bool isValid = false;

			using ( var context = new Data.CTIEntities() )
			{
				if ( Id == 0 )
				{
					statusMessage = "Error - missing an identifier for the JurisdictionProfile";
					return false;
				}

				DBentity efEntity =
					context.Entity_JurisdictionProfile.SingleOrDefault( s => s.Id == Id );
				if ( efEntity != null && efEntity.Id > 0 )
				{
					context.Entity_JurisdictionProfile.Remove( efEntity );
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					}
				}
				else
				{
					statusMessage = string.Format( "JurisdictionProfile record was not found: {0}", Id );
					isValid = false;
				}
			}

			return isValid;
		}

		#endregion
		#region JurisdictionProfile retrieve  =======================

		/// <summary>
		/// get all related JurisdictionProfiles for the parent
		/// </summary>
		/// <param name="parentUId"></param>
		/// <returns></returns>
		public static List<ThisEntity> Jurisdiction_GetAll( Guid parentUid, int jprofilePurposeId = 1 )
		{
			//efEntity.JProfilePurposeId
			ThisEntity entity = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			int count = 0;

			//TODO - pass entityId if available?
			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}

			using ( var context = new Data.CTIEntities() )
			{
				List<DBentity> Items = context.Entity_JurisdictionProfile
							.Where( s => s.EntityId == parent.Id && s.JProfilePurposeId == jprofilePurposeId )
							.OrderBy( s => s.Id ).ToList();

				if ( Items.Count > 0 )
				{
					foreach ( DBentity item in Items )
					{
						entity = new ThisEntity();
						count++;
						//map and get regions
						ToMap( item, entity, count );
						list.Add( entity );
					}
				}
			}

			return list;
		}

		/// <summary>
		/// Get a single Jurisdiction Profile by integer Id
		/// </summary>
		/// <param name="rowId"></param>
		/// <returns></returns>
		public static ThisEntity Jurisdiction_Get( int id )
		{
			ThisEntity entity = new ThisEntity();
			using ( var context = new Data.CTIEntities() )
			{
				DBentity item = context.Entity_JurisdictionProfile
							.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, 1 );
				}

			}

			return entity;
		}
		/// <summary>
		/// Get a single Jurisdiction Profile by Guid
		/// </summary>
		/// <param name="rowId"></param>
		/// <returns></returns>
		public static ThisEntity Jurisdiction_Get( Guid rowId )
		{
			ThisEntity entity = new ThisEntity();
			using ( var context = new Data.CTIEntities() )
			{
				DBentity item = context.Entity_JurisdictionProfile
							.SingleOrDefault( s => s.RowId == rowId );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, 1 );
				}

			}

			return entity;
		}

		/// <summary>
		/// Mapping from interface model to entity model
		/// Assuming that for updates, the entity model is always populated from DB, so here we can make assumptions regarding what can be updated.
		/// </summary>
		/// <param name="fromEntity"></param>
		/// <param name="to"></param>
		private static void FromMap( ThisEntity fromEntity, DBentity to )
		{
			to.Id = fromEntity.Id;
			//to.EntityId = fromEntity.ParentId;
			//don't allow a change if an update
			if ( fromEntity.Id == 0 )
				to.JProfilePurposeId = fromEntity.JProfilePurposeId > 0 ? fromEntity.JProfilePurposeId : 1;
			else
			{
				//handle unexpected
				if ( to.JProfilePurposeId == null )
					to.JProfilePurposeId = 1;
			}

			if ( fromEntity.MainJurisdiction != null && !string.IsNullOrWhiteSpace( fromEntity.MainJurisdiction.Name ) )
				to.Name = fromEntity.MainJurisdiction.Name;
			else
				to.Name = "Default jurisdiction";
			if ( fromEntity.Description.ToLower() != "auto-saved jurisdiction" )
				to.Description = fromEntity.Description;
			else
				to.Description = null;
			to.IsOnlineJurisdiction = fromEntity.IsOnlineJurisdiction;
			to.IsGlobalJurisdiction = fromEntity.IsGlobalJurisdiction;


			if ( to.Id < 1 )
			{
				to.CreatedById = fromEntity.CreatedById;
				to.LastUpdatedById = fromEntity.LastUpdatedById;
			}


		}
		private static void ToMap( DBentity fromEntity, ThisEntity to, int count )
		{
			to.Id = fromEntity.Id;
			to.RowId = fromEntity.RowId;
			to.ParentId = (int)fromEntity.EntityId;

			//these will probably no lonber be necessary
			to.ParentTypeId = fromEntity.Entity.EntityTypeId;
			to.ParentEntityId = fromEntity.Entity.EntityUid;

			to.JProfilePurposeId = fromEntity.JProfilePurposeId != null ? ( int ) fromEntity.JProfilePurposeId : 1;

			//to.Name = fromEntity.Name;
			to.Description = fromEntity.Description;
			to.IsOnlineJurisdiction = ( bool ) fromEntity.IsOnlineJurisdiction;
			to.IsGlobalJurisdiction = ( bool ) fromEntity.IsGlobalJurisdiction;

			if ( fromEntity.Created != null )
				to.Created = ( DateTime ) fromEntity.Created;
			to.CreatedById = fromEntity.CreatedById == null ? 0 : ( int ) fromEntity.CreatedById;

			if ( fromEntity.LastUpdated != null )
				to.LastUpdated = ( DateTime ) fromEntity.LastUpdated;
			to.LastUpdatedById = fromEntity.LastUpdatedById == null ? 0 : ( int ) fromEntity.LastUpdatedById;

			//get geoCoordinates via jp.Id
			//apparantly only allowing one, but that will get old
			//List<CM.GeoCoordinates> regions = GetAllForJurisdiction( to.Id, false );
			//if ( regions != null && regions.Count > 0 )
			//{
			//	to.MainJurisdiction = regions[ 0 ];
			//}
			//to.JurisdictionException = GetAllForJurisdiction( to.Id, true );

			List<CM.GeoCoordinates> regions = GetAll( to.RowId, false );
			if ( regions != null && regions.Count > 0 )
			{
				to.MainJurisdiction = regions[ 0 ];
			}
			to.JurisdictionException = GetAll( to.RowId, true );

			if ( !string.IsNullOrWhiteSpace( fromEntity.Description ) )
			{
				to.ProfileSummary = fromEntity.Description;
			}
			else
			{
				if ( to.MainJurisdiction != null && to.MainJurisdiction.GeoNamesId > 0 )
				{
					to.ProfileSummary = to.MainJurisdiction.ProfileSummary;
				}
				else
					to.ProfileSummary = "JurisdictionProfile Summary - " + count.ToString();
			}
		}

		#endregion
		#endregion

		#region GeoCoordinate  =======================
		#region GeoCoordinate Core  =======================
		public bool GeoCoordinate_Update( List<CM.GeoCoordinates> list,
					Guid parentUid,
					int userId, bool isExceptions,
					ref List<String> messages )
		{
			bool isValid = true;
			if ( !IsValidGuid( parentUid ) )
			{
				messages.Add( "Error - missing a parent identifier" );
				return false;
			}

			if ( list == null || list.Count == 0 )
			{
				//list could be empty, but may need to process deletes
				//===> actually all deletes will now be handled immediately
				//				messages.Add("Error - there were no regions to process.");
				return true;
			}
			EM.GeoCoordinate efEntity = new EM.GeoCoordinate();
			int id = 0;
			using ( var context = new Data.CTIEntities() )
			{
				foreach ( CM.GeoCoordinates item in list )
				{
					efEntity = new EM.GeoCoordinate();
					item.IsException = isExceptions;

					if ( item.Id == 0 )
					{
						//check for Empty
						if ( item.GeoNamesId > 0 )
						{
							//add
							item.CreatedById = item.LastUpdatedById = userId;
							//prob not necessary, check
							item.ParentEntityId = parentUid;

							FromMap( item, efEntity );

							efEntity.Created = System.DateTime.Now;
							efEntity.LastUpdated = System.DateTime.Now;

							context.GeoCoordinate.Add( efEntity );

							// submit the change to database
							int count = context.SaveChanges();
							if ( count == 0 )
							{
								isValid = false;
								messages.Add( string.Format( "Failed to add a region: {0}", efEntity.Name ) );
							}
						}
					}
					else
					{
						item.LastUpdatedById = userId;
						//prob not necessary, check
						item.ParentEntityId = parentUid;
						if ( GeoCoordinate_Update( item, ref messages ) == false )
						{
							isValid = false;
						}
					}
				}

			}

			return isValid;
		}

		public int GeoCoordinates_Add( CM.GeoCoordinates entity, ref string statusMessage )
		{
			EM.GeoCoordinate efEntity = new EM.GeoCoordinate();
			CM.GeoCoordinates existing = new CM.GeoCoordinates();
			List<String> messages = new List<string>();
			if ( entity.GeoNamesId == 0 )
			{
				return 0;
			}
			else if ( GeoCoordinates_Exists( entity.ParentEntityId, entity.GeoNamesId, entity.IsException ) )
			{
				statusMessage = "Error this Region has aleady been selected.";
				return 0;
			}
			else if ( entity.IsException == false && Jurisdiction_HasMainRegion( entity.ParentEntityId, ref existing ) )
			{
				entity.Id = existing.Id;
				bool isValid = GeoCoordinate_Update( entity, ref messages );

				return existing.Id;
			}
			else
			{
				FromMap( entity, efEntity );
				return GeoCoordinate_Add( efEntity, ref statusMessage );
			}


		}
		/// <summary>
		/// Probably want to combine with region to have access to keys
		/// </summary>
		/// <param name="efEntity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int GeoCoordinate_Add( EM.GeoCoordinate efEntity, ref string statusMessage )
		{

			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					if ( !IsValidGuid( efEntity.ParentId ) )
					{
						statusMessage = "Error - missing a parent identifier";
						return 0;
					}

					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdated = System.DateTime.Now;

					context.GeoCoordinate.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";

						return efEntity.Id;
					}
					else
					{
						//?no info on error
					}
				}

				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".RelatedRegion_Add(), Name: {0}, ParentId: {1)", efEntity.Name, efEntity.ParentId ) );
				}
			}

			return 0;
		}


		public bool GeoCoordinate_Update( CM.GeoCoordinates entity, ref List<String> messages )
		{
			bool isValid = true;

			using ( var context = new Data.CTIEntities() )
			{
				if ( entity == null || entity.Id == 0 || entity.GeoNamesId == 0 )
				{
					messages.Add( "Error - missing an identifier for the GeoCoordinate" );
					return false;
				}

				EM.GeoCoordinate efEntity =
					context.GeoCoordinate.SingleOrDefault( s => s.Id == entity.Id );
				if ( !IsValidGuid( efEntity.ParentId ) )
				{
					messages.Add( "Error - missing a parent identifier" );
					return false;
				}

				FromMap( entity, efEntity );

				if ( HasStateChanged( context ) )
				{
					efEntity.LastUpdated = DateTime.Now;
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					}
				}
				else
				{
					//GeoCoordinate_Update skipped, as no change to the contents
				}
			}

			return isValid;
		}

		/// <summary>
		/// Delete a region
		/// </summary>
		/// <param name="Id"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool GeoCoordinate_Delete( int Id, ref string statusMessage )
		{
			bool isValid = false;

			using ( var context = new Data.CTIEntities() )
			{
				if ( Id == 0 )
				{
					statusMessage = "Error - missing an identifier for the GeoCoordinate";
					return false;
				}

				EM.GeoCoordinate efEntity =
					context.GeoCoordinate.SingleOrDefault( s => s.Id == Id );
				if ( efEntity != null && efEntity.Id > 0 )
				{
					context.GeoCoordinate.Remove( efEntity );
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					}
				}
				else
				{
					statusMessage = string.Format( "GeoCoordinate record was not found: {0}", Id );
					isValid = false;
				}
			}

			return isValid;
		}

		#endregion
		#region GeoCoordinate retrieve  =======================

		/// <summary>
		/// get GeoCoordinates
		/// </summary>
		/// <param name="Id"></param>
		/// <returns></returns>
		public static CM.GeoCoordinates GeoCoordinates_Get( int Id )
		{
			CM.GeoCoordinates entity = new CM.GeoCoordinates();
			List<CM.GeoCoordinates> list = new List<CM.GeoCoordinates>();
			using ( var context = new Data.CTIEntities() )
			{
				EM.GeoCoordinate item = context.GeoCoordinate
							.SingleOrDefault( s => s.Id == Id );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity );
				}
			}

			return entity;
		}

		/// <summary>
		/// Determine if a geoName already exists for the parent and type (is or is not an exception)
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="geoNamesId"></param>
		/// <param name="isException"></param>
		/// <returns></returns>
		public static bool GeoCoordinates_Exists( Guid parentUid, int geoNamesId, bool isException )
		{
			bool isFound = false;
			using ( var context = new Data.CTIEntities() )
			{
				EM.GeoCoordinate item = context.GeoCoordinate
							.SingleOrDefault( s => s.ParentId == parentUid && s.GeoNamesId == geoNamesId && s.IsException == isException );

				if ( item != null && item.Id > 0 )
				{
					isFound = true;
				}
			}

			return isFound;
		}

		/// <summary>
		/// Determine if a main region already exists for a jurisdiction. If found, then an update will be done rather than an add.
		/// This requirement may change in the future.
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="isException"></param>
		/// <returns></returns>
		public static bool Jurisdiction_HasMainRegion( Guid parentUid, ref CM.GeoCoordinates entity )
		{
			bool isFound = false;
			//CM.GeoCoordinates entity = new CM.GeoCoordinates();
			using ( var context = new Data.CTIEntities() )
			{
				List<EM.GeoCoordinate> list = context.GeoCoordinate
							.Where( s => s.ParentId == parentUid && s.IsException == false ).ToList();

				if ( list != null && list.Count > 0 )
				{
					isFound = true;
					ToMap( list[ 0 ], entity );
				}
			}

			return isFound;
		}

		/// <summary>
		/// Get a list of geocoordinates from a list of IDs
		/// </summary>
		/// <param name="Ids"></param>
		/// <returns></returns>
		public static List<CM.GeoCoordinates> GeoCoordinates_GetList( List<int> Ids )
		{
			List<CM.GeoCoordinates> entities = new List<CM.GeoCoordinates>();
			using ( var context = new Data.CTIEntities() )
			{
				List<EM.GeoCoordinate> items = context.GeoCoordinate.Where( m => Ids.Contains( m.Id ) ).ToList();
				foreach ( var item in items )
				{
					CM.GeoCoordinates entity = new CM.GeoCoordinates();
					ToMap( item, entity );
					entities.Add( entity );
				}
			}

			return entities;
		}
		//

		/// <summary>
		/// Get all GeoCoordinates for a jurisdiction
		/// </summary>
		/// <param name="jurisdictionId"></param>
		/// <returns></returns>
		[Obsolete]
		private static List<CM.GeoCoordinates> GetAllForJurisdiction( int jurisdictionId, bool isException )
		{
			CM.GeoCoordinates entity = new CM.GeoCoordinates();
			List<CM.GeoCoordinates> list = new List<CM.GeoCoordinates>();
			using ( var context = new Data.CTIEntities() )
			{
				List<EM.GeoCoordinate> Items = context.GeoCoordinate
							.Where( s => s.JurisdictionId == jurisdictionId
							&& s.IsException == isException )
							.OrderBy( s => s.Id ).ToList();

				if ( Items.Count > 0 )
				{
					foreach ( EM.GeoCoordinate item in Items )
					{
						entity = new CM.GeoCoordinates();
						ToMap( item, entity );
						list.Add( entity );
					}
				}
			}

			return list;
		}


		/// <summary>
		/// get all related GeoCoordinates for the parent
		/// </summary>
		/// <param name="parentId"></param>
		/// <returns></returns>
		public static List<CM.GeoCoordinates> GetAll( Guid parentId, bool isException = false )
		{
			CM.GeoCoordinates entity = new CM.GeoCoordinates();
			List<CM.GeoCoordinates> list = new List<CM.GeoCoordinates>();
			using ( var context = new Data.CTIEntities() )
			{
				List<EM.GeoCoordinate> Items = context.GeoCoordinate
							.Where( s => s.ParentId == parentId && s.IsException == isException )
							.OrderBy( s => s.Id ).ToList();

				if ( Items.Count > 0 )
				{
					foreach ( EM.GeoCoordinate item in Items )
					{
						entity = new CM.GeoCoordinates();
						ToMap( item, entity );
						list.Add( entity );
					}
				}
			}

			return list;
		}

		/// <summary>
		/// Get recent selected regions for a user
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public static List<CM.GeoCoordinates> GetAll( int userId )
		{
			CM.GeoCoordinates entity = new CM.GeoCoordinates();
			List<CM.GeoCoordinates> list = new List<CM.GeoCoordinates>();
			int prevGeoId = 0;
			using ( var context = new Data.CTIEntities() )
			{
				List<EM.GeoCoordinate> Items = context.GeoCoordinate
							.Where( s => s.CreatedById == userId )
							.Take( 50 )
							.OrderBy( s => s.Name ).ThenBy( s => s.GeoNamesId )
							.ToList();

				if ( Items.Count > 0 )
				{
					foreach ( EM.GeoCoordinate item in Items )
					{
						if ( prevGeoId != ( int ) item.GeoNamesId )
						{
							entity = new CM.GeoCoordinates();
							ToMap( item, entity );
							list.Add( entity );
							prevGeoId = ( int ) item.GeoNamesId;
						}
					}
				}
			}

			return list;
		}

		private static void FromMap( CM.GeoCoordinates fromEntity, EM.GeoCoordinate to )
		{
			to.Id = fromEntity.Id;
			to.ParentId = fromEntity.ParentEntityId;

			to.GeoNamesId = fromEntity.GeoNamesId;
			to.Name = fromEntity.Name;
			to.IsException = fromEntity.IsException;
			to.AddressRegion = fromEntity.Region;
			to.Country = fromEntity.Country;
			to.Latitude = fromEntity.Latitude;
			to.Longitude = fromEntity.Longitude;
			to.Url = fromEntity.Url;

			//if ( to.Id < 1 )
			//{
			//	to.CreatedById = fromEntity.CreatedById;
			//	to.LastUpdatedById = fromEntity.LastUpdatedById;
			//}
			//if ( fromEntity.Created != null )
			//	to.Created = ( DateTime ) fromEntity.Created;

			//if ( fromEntity.LastUpdated != null )
			//	to.LastUpdated = fromEntity.LastUpdated;


		}
		private static void ToMap( EM.GeoCoordinate fromEntity, CM.GeoCoordinates to )
		{
			to.Id = fromEntity.Id;
			to.ParentEntityId = fromEntity.ParentId;
			to.GeoNamesId = fromEntity.GeoNamesId != null ? ( int ) fromEntity.GeoNamesId : 0;

			to.Name = fromEntity.Name;
			to.IsException = fromEntity.IsException != null ? ( bool ) fromEntity.IsException : false;
			to.Region = fromEntity.AddressRegion;
			to.Country = fromEntity.Country;
			to.Latitude = fromEntity.Latitude;
			to.Longitude = fromEntity.Longitude;
			to.Url = fromEntity.Url;
			to.ProfileSummary = to.Name;
			if ( !string.IsNullOrWhiteSpace( to.Region ) )
			{
				to.ProfileSummary += ", " + to.Region;
			}
			if ( !string.IsNullOrWhiteSpace( to.Country ) && to.Country != to.Name )
			{
				to.ProfileSummary += ", " + to.Country;
			}

			if ( fromEntity.Created != null )
				to.Created = ( DateTime ) fromEntity.Created;
			to.CreatedById = fromEntity.CreatedById == null ? 0 : ( int ) fromEntity.CreatedById;

			if ( fromEntity.LastUpdated != null )
				to.LastUpdated = ( DateTime ) fromEntity.LastUpdated;
			to.LastUpdatedById = fromEntity.LastUpdatedById == null ? 0 : ( int ) fromEntity.LastUpdatedById;


		}

		#endregion
		#endregion
	}
}
