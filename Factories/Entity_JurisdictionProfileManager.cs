using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MC = Models.Common;
using EM = Data;
using Utilities;
using Data;
using DBEntity = Data.Entity_JurisdictionProfile;
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
		public static int JURISDICTION_PURPOSE_OFFERREDIN = 3;

		#region JurisdictionProfile  =======================
		#region JurisdictionProfile Core  =======================

		/// <summary>
		/// Add a jurisdiction profile
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="property">Can be blank. Set to a property where additional validation is necessary</param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool Add( ThisEntity entity, string property, ref List<String> messages )
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
				MC.Entity parent = EntityManager.GetEntity( entity.ParentEntityId );
				if ( parent == null || parent.Id == 0 )
				{
					messages.Add( "Error - the parent entity was not found." );
					return false;
				}

				//check for Empty
				//==> not sure what is the minimum required fields!
				bool isEmpty = false;

				if ( ValidateProfile( entity, property, ref isEmpty, ref messages ) == false )
				{
					return false;
				}
				if ( isEmpty )
				{
					messages.Add( "Error - Jurisdiction profile is empty. " );
					return false;
				}
				

				//id = JurisdictionProfile_Add( entity, ref messages );
				DBEntity efEntity = new DBEntity();
				MapToDB( entity, efEntity );
				efEntity.EntityId = parent.Id;
				efEntity.RowId = Guid.NewGuid();
				entity.RowId = efEntity.RowId;

				if ( efEntity.JProfilePurposeId == null || efEntity.JProfilePurposeId == 0 )
					efEntity.JProfilePurposeId = 1;

				efEntity.Created = efEntity.LastUpdated = DateTime.Now;
				efEntity.CreatedById = efEntity.LastUpdatedById = entity.LastUpdatedById;

				context.Entity_JurisdictionProfile.Add( efEntity );

				int count = context.SaveChanges();
				if ( count > 0 )
				{
					entity.Id = efEntity.Id;
					//update parts
					UpdateParts( entity, true, ref messages );

					UpdateJPRegions( entity, entity.LastUpdatedById, ref  messages );
				}
	

			}

			return isValid;
		}
		/// <summary>
		/// Update a jurisdiction profile
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="property">Can be blank. Set to a property where additional validation is necessary</param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool Update( ThisEntity entity, string property, ref List<String> messages )
		{
			bool isValid = true;

			using ( var context = new Data.CTIEntities() )
			{
				if ( entity == null || entity.Id == 0 || !IsValidGuid( entity.ParentEntityId ) )
				{
					messages.Add( "Error - missing an identifier for the JurisdictionProfile" );
					return false;
				}

				MC.Entity parent = EntityManager.GetEntity( entity.ParentEntityId );
				if ( parent == null || parent.Id == 0 )
				{
					messages.Add( "Error - the parent entity was not found." );
					return false;
				}
				bool isEmpty = false;
				if ( ValidateProfile( entity, property, ref isEmpty, ref messages ) == false )
				{
					return false;
				}

				DBEntity efEntity =
					context.Entity_JurisdictionProfile.SingleOrDefault( s => s.Id == entity.Id );

				entity.RowId = efEntity.RowId;
				MapToDB( entity, efEntity );
				

				if ( HasStateChanged( context ) )
				{
					efEntity.LastUpdated = DateTime.Now;
					efEntity.LastUpdatedById = entity.LastUpdatedById;
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						
					}
				}
				//always do parts
				isValid = UpdateParts( entity, false, ref messages );
			}

			return isValid;
		}
		public bool UpdateParts( ThisEntity entity, bool isAdd, ref List<String> messages )
		{
			bool isValid = true;

			EntityPropertyManager mgr = new EntityPropertyManager();

			if ( mgr.UpdateProperties( entity.JurisdictionAssertion, entity.RowId, CodesManager.ENTITY_TYPE_JURISDICTION_PROFILE, CodesManager.PROPERTY_CATEGORY_JurisdictionAssertionType, entity.LastUpdatedById, ref messages ) == false )
				isValid = false;

			return isValid;
		}
		public bool UpdateJPRegions( ThisEntity entity, int userId, ref List<String> messages )
		{
			bool isValid = true;
			List<MC.GeoCoordinates> list = new List<MC.GeoCoordinates>();
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
		public bool ValidateProfile( ThisEntity profile, string property, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;
			int count = messages.Count;
			isEmpty = false;
			//this will be problematic as the two bools default to false
			if ( string.IsNullOrWhiteSpace( profile.Description )
				&& ( profile.MainJurisdiction == null || profile.MainJurisdiction.GeoNamesId == 0 )
				&& ( profile.JurisdictionException == null || profile.JurisdictionException.Count == 0 )
				&& ( !IsValidGuid( profile.AssertedBy ) )
				&& ( profile.JurisdictionAssertion == null || profile.JurisdictionAssertion.Items.Count == 0 )
				)
			{
				//isEmpty = true;
				//messages.Add( "No data has been entered, save was cancelled." );
				//return false;
			}
			if ( property == "JurisdictionAssertions"  && profile.Id > 0)
			{
				//if (!IsValidGuid(profile.AssertedBy))
				//	messages.Add( "Please select the Agent that makes these assertions." );
				if ( profile.JurisdictionAssertion == null || profile.JurisdictionAssertion.Items.Count == 0 )
					messages.Add( "Please select at least one assertion." );
			}

			if ( profile.MainJurisdiction == null || profile.MainJurisdiction.GeoNamesId == 0 )
			{
				List<MC.GeoCoordinates> regions = GetAll( profile.RowId, false );
				if ( regions != null && regions.Count > 0 )
				{
					profile.MainJurisdiction = regions[ 0 ];
				}
			}
			//need to have a main jurisdiction, or is global
			if ( profile.MainJurisdiction != null
				&& profile.MainJurisdiction.GeoNamesId > 0
				&& profile.MainJurisdiction.Name != "Earth" )
			{
				//should not have global
				if ((profile.IsGlobalJurisdiction ?? false) == true)
				{
					messages.Add( "Is Global cannot be set to 'Is Global' when an main region has been selected." );
				}
			} else
			{
				//no regions, must specify global
				//may want to make configurable
				if ( ( profile.IsGlobalJurisdiction ?? false ) == false )
				{
					if ( profile.Description != "Auto-saved Jurisdiction" )
					{
						if ( UtilityManager.GetAppKeyValue( "requireRegionOrIsGlobal", false ) == true )
						{
							messages.Add( "Please select a main region, OR set 'Is Global' to 'This jurisdiction is global'." );
						}
					}
				}
			}
				

			if ( messages.Count > count )
				isValid = false;
			return isValid;
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

				DBEntity efEntity =
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

			MC.Entity  parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}

			using ( var context = new Data.CTIEntities() )
			{
				List<DBEntity> Items = context.Entity_JurisdictionProfile
							.Where( s => s.EntityId == parent.Id 
								&& s.JProfilePurposeId == jprofilePurposeId )
							.OrderBy( s => s.Id ).ToList();

				if ( Items.Count > 0 )
				{
					foreach ( DBEntity item in Items )
					{
						entity = new ThisEntity();
						count++;
						//map and get regions
						MapFromDB( item, entity, count );
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
		public static ThisEntity Get( int id )
		{
			ThisEntity entity = new ThisEntity();
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity item = context.Entity_JurisdictionProfile
							.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity, 1 );
				}

			}

			return entity;
		}
		/// <summary>
		/// Get a single Jurisdiction Profile by Guid
		/// </summary>
		/// <param name="rowId"></param>
		/// <returns></returns>
		public static ThisEntity Get( Guid rowId )
		{
			ThisEntity entity = new ThisEntity();
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity item = context.Entity_JurisdictionProfile
							.SingleOrDefault( s => s.RowId == rowId );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity, 1 );
				}

			}

			return entity;
		}

		/// <summary>
		/// Mapping from interface model to entity model
		/// Assuming that for updates, the entity model is always populated from DB, so here we can make assumptions regarding what can be updated.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		private static void MapToDB( ThisEntity from, DBEntity to )
		{
			to.Id = from.Id;
			//to.EntityId = from.ParentId;
			//don't allow a change if an update
			if ( from.Id == 0 )
				to.JProfilePurposeId = from.JProfilePurposeId > 0 ? from.JProfilePurposeId : 1;
			else
			{
				//handle unexpected
				if ( to.JProfilePurposeId == null )
					to.JProfilePurposeId = 1;
			}

			//from.MainJurisdiction is likely null
			if ( from.MainJurisdiction != null && from.MainJurisdiction.GeoNamesId == 0 )
			{
				List<MC.GeoCoordinates> regions = GetAll( to.RowId, false );
				if ( regions != null && regions.Count > 0 )
				{
					from.MainJurisdiction = regions[ 0 ];
				}
			}

			if ( from.MainJurisdiction != null && !string.IsNullOrWhiteSpace( from.MainJurisdiction.Name ) )
				to.Name = from.MainJurisdiction.Name;
			else
				to.Name = "Default jurisdiction";
			if ( from.Description.ToLower() != "auto-saved jurisdiction" )
			{
				to.Description = from.Description;
				if ( to.Description.IndexOf( "jQuery" ) > 5 )
				{
					int len = to.Description.Length - to.Description.IndexOf( "jQuery" );
					to.Description = to.Description.Substring( 0, len );
				}
			}
			else
				to.Description = null;

			//TODO - if a main jurisdiction exists, then global should be false
			//may not be available
			if ( from.MainJurisdiction != null 
				&& from.MainJurisdiction.GeoNamesId > 0 
				&& from.MainJurisdiction.Name != "Earth" )
				to.IsGlobalJurisdiction = false;

			else if ( from.IsGlobalJurisdiction != null )
				to.IsGlobalJurisdiction = from.IsGlobalJurisdiction;
			else
				to.IsGlobalJurisdiction = null;

			if ( IsGuidValid( from.AssertedBy ) )
			{
				if ( to.Id > 0 && to.AssertedByAgentUid != from.AssertedBy )
				{
					if ( IsGuidValid( to.AssertedByAgentUid ) )
					{
						//need to remove the previous roles on change of asserted by
						string statusMessage = "";
						new Entity_AgentRelationshipManager().Delete( to.RowId, to.AssertedByAgentUid, Entity_AgentRelationshipManager.ROLE_TYPE_OWNER, ref statusMessage );
					}
				}
				to.AssertedByAgentUid = from.AssertedBy;
			}
			else
			{
				to.AssertedByAgentUid = null;
			}
		}
		private static void MapFromDB( DBEntity from, ThisEntity to, int count )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.ParentId = (int)from.EntityId;

			//these will probably no lonber be necessary
			to.ParentTypeId = from.Entity.EntityTypeId;
			to.ParentEntityId = from.Entity.EntityUid;

			to.JProfilePurposeId = from.JProfilePurposeId != null ? ( int ) from.JProfilePurposeId : 1;

			if ( IsGuidValid( from.AssertedByAgentUid ) )
			{
				to.AssertedBy = ( Guid ) from.AssertedByAgentUid;

				to.AssertedByOrganization = OrganizationManager.GetForSummary( to.AssertedBy );

			}
			//to.Name = from.Name;
			if ( (from.Description ?? "") == "Auto-saved Jurisdiction" )
				to.Description = "";
			else
				to.Description = from.Description;

			if ( from.Created != null )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;

			if ( from.LastUpdated != null )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

			//get geoCoordinates via jp.Id
			//apparantly only allowing one, but that will get old
			//List<MC.GeoCoordinates> regions = GetAllForJurisdiction( to.Id, false );
			//if ( regions != null && regions.Count > 0 )
			//{
			//	to.MainJurisdiction = regions[ 0 ];
			//}
			//to.JurisdictionException = GetAllForJurisdiction( to.Id, true );
			//to.IsOnlineJurisdiction = ( bool ) from.IsOnlineJurisdiction;


			List<MC.GeoCoordinates> regions = GetAll( to.RowId, false );
			if ( regions != null && regions.Count > 0 )
			{
				to.MainJurisdiction = regions[ 0 ];
			}
			to.JurisdictionException = GetAll( to.RowId, true );


			if ( to.MainJurisdiction != null && to.MainJurisdiction.GeoNamesId > 0 && to.MainJurisdiction.Name != "Earth")
				to.IsGlobalJurisdiction = false;
			else
				to.IsGlobalJurisdiction = from.IsGlobalJurisdiction;

			if ( !string.IsNullOrWhiteSpace( from.Description ) )
			{
				to.ProfileSummary = from.Description;
			}
			else
			{
				if ( to.MainJurisdiction != null && to.MainJurisdiction.GeoNamesId > 0 )
				{
					to.ProfileSummary = to.MainJurisdiction.ProfileSummary;
				}
				else
				{
					if ( (bool)(to.IsGlobalJurisdiction ?? false) )
						to.ProfileSummary = "Global";
					else
						to.ProfileSummary = "JurisdictionProfile Summary - " + count.ToString();
				}
			}
			//use of properities, requires creating an Entity for Jurisdiction???
			to.JurisdictionAssertion = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_JurisdictionAssertionType );
		} //


		//public static MC.Enumeration FillEnumeration( Guid parentUid, int categoryId )
		//{
		//	MC.Enumeration entity = new MC.Enumeration();
		//	entity = CodesManager.GetEnumeration( categoryId );

		//	entity.Items = new List<MC.EnumeratedItem>();
		//	MC.EnumeratedItem item = new MC.EnumeratedItem();

		//	using ( var context = new ViewContext() )
		//	{
		//		List<EntityProperty_Summary> results = context.EntityProperty_Summary
		//			.Where( s => s.EntityUid == parentUid
		//				&& s.CategoryId == categoryId )
		//			.OrderBy( s => s.CategoryId ).ThenBy( s => s.SortOrder ).ThenBy( s => s.Property )
		//			.ToList();

		//		if ( results != null && results.Count > 0 )
		//		{
		//			foreach ( EntityProperty_Summary prop in results )
		//			{

		//				item = new MC.EnumeratedItem();
		//				item.Id = prop.PropertyValueId;
		//				item.Value = prop.PropertyValueId.ToString();
		//				item.Selected = true;

		//				item.Name = prop.Property;
		//				item.SchemaName = prop.PropertySchemaName;
		//				entity.Items.Add( item );

		//			}
		//		}
		//		//entity.OtherValue = EntityOtherProperty_Get( parentUid, categoryId );
		//		return entity;
		//	}
		//}
		#endregion
		#endregion

		#region GeoCoordinate  =======================
		#region GeoCoordinate Core  =======================
		public bool GeoCoordinate_Update( List<MC.GeoCoordinates> list,
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
		
			using ( var context = new Data.CTIEntities() )
			{
				foreach ( MC.GeoCoordinates item in list )
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

							MapToDB( item, efEntity );

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

		public int GeoCoordinates_Add( MC.GeoCoordinates entity, ref string statusMessage )
		{
			EM.GeoCoordinate efEntity = new EM.GeoCoordinate();
			MC.GeoCoordinates existing = new MC.GeoCoordinates();
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
				MapToDB( entity, efEntity );
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


		public bool GeoCoordinate_Update( MC.GeoCoordinates entity, ref List<String> messages )
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

				MapToDB( entity, efEntity );

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
		public static MC.GeoCoordinates GeoCoordinates_Get( int Id )
		{
			MC.GeoCoordinates entity = new MC.GeoCoordinates();
			List<MC.GeoCoordinates> list = new List<MC.GeoCoordinates>();
			using ( var context = new Data.CTIEntities() )
			{
				EM.GeoCoordinate item = context.GeoCoordinate
							.SingleOrDefault( s => s.Id == Id );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity );
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
		public static bool Jurisdiction_HasMainRegion( Guid parentUid, ref MC.GeoCoordinates entity )
		{
			bool isFound = false;
			//MC.GeoCoordinates entity = new MC.GeoCoordinates();
			using ( var context = new Data.CTIEntities() )
			{
				List<EM.GeoCoordinate> list = context.GeoCoordinate
							.Where( s => s.ParentId == parentUid && s.IsException == false ).ToList();

				if ( list != null && list.Count > 0 )
				{
					isFound = true;
					MapFromDB( list[ 0 ], entity );
				}
			}

			return isFound;
		}

		/// <summary>
		/// Get a list of geocoordinates from a list of IDs
		/// </summary>
		/// <param name="Ids"></param>
		/// <returns></returns>
		public static List<MC.GeoCoordinates> GeoCoordinates_GetList( List<int> Ids )
		{
			List<MC.GeoCoordinates> entities = new List<MC.GeoCoordinates>();
			using ( var context = new Data.CTIEntities() )
			{
				List<EM.GeoCoordinate> items = context.GeoCoordinate.Where( m => Ids.Contains( m.Id ) ).ToList();
				foreach ( var item in items )
				{
					MC.GeoCoordinates entity = new MC.GeoCoordinates();
					MapFromDB( item, entity );
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
		private static List<MC.GeoCoordinates> GetAllForJurisdiction( int jurisdictionId, bool isException )
		{
			MC.GeoCoordinates entity = new MC.GeoCoordinates();
			List<MC.GeoCoordinates> list = new List<MC.GeoCoordinates>();
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
						entity = new MC.GeoCoordinates();
						MapFromDB( item, entity );
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
		public static List<MC.GeoCoordinates> GetAll( Guid parentId, bool isException = false )
		{
			MC.GeoCoordinates entity = new MC.GeoCoordinates();
			List<MC.GeoCoordinates> list = new List<MC.GeoCoordinates>();
			using ( var context = new Data.CTIEntities() )
			{
				List<EM.GeoCoordinate> Items = context.GeoCoordinate
							.Where( s => s.ParentId == parentId && s.IsException == isException )
							.OrderBy( s => s.Id ).ToList();

				if ( Items.Count > 0 )
				{
					foreach ( EM.GeoCoordinate item in Items )
					{
						entity = new MC.GeoCoordinates();
						MapFromDB( item, entity );
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
		public static List<MC.GeoCoordinates> GetAll( int userId )
		{
			MC.GeoCoordinates entity = new MC.GeoCoordinates();
			List<MC.GeoCoordinates> list = new List<MC.GeoCoordinates>();
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
							entity = new MC.GeoCoordinates();
							MapFromDB( item, entity );
							list.Add( entity );
							prevGeoId = ( int ) item.GeoNamesId;
						}
					}
				}
			}

			return list;
		}

		private static void MapToDB( MC.GeoCoordinates from, EM.GeoCoordinate to )
		{
			to.Id = from.Id;
			to.ParentId = from.ParentEntityId;

			to.GeoNamesId = from.GeoNamesId;
			to.Name = from.Name;
			to.IsException = from.IsException;
			to.AddressRegion = from.Region;
			to.Country = from.Country;
			to.Latitude = from.Latitude;
			to.Longitude = from.Longitude;
			to.Url = from.Url;

			//if ( to.Id < 1 )
			//{
			//	to.CreatedById = from.CreatedById;
			//	to.LastUpdatedById = from.LastUpdatedById;
			//}
			//if ( from.Created != null )
			//	to.Created = ( DateTime ) from.Created;

			//if ( from.LastUpdated != null )
			//	to.LastUpdated = from.LastUpdated;


		}
		private static void MapFromDB( EM.GeoCoordinate from, MC.GeoCoordinates to )
		{
			to.Id = from.Id;
			to.ParentEntityId = from.ParentId;
			to.GeoNamesId = from.GeoNamesId != null ? ( int ) from.GeoNamesId : 0;

			to.Name = from.Name;
			to.IsException = from.IsException != null ? ( bool ) from.IsException : false;
			to.Region = from.AddressRegion;
			to.Country = from.Country;
			to.Latitude = from.Latitude;
			to.Longitude = from.Longitude;
			to.Url = from.Url;
			to.ProfileSummary = to.Name;
			if ( !string.IsNullOrWhiteSpace( to.Region ) )
			{
				to.ProfileSummary += ", " + to.Region;
			}
			if ( !string.IsNullOrWhiteSpace( to.Country ) && to.Country != to.Name )
			{
				to.ProfileSummary += ", " + to.Country;
			}

			if ( from.Created != null )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;

			if ( from.LastUpdated != null )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;


		}

		#endregion
		#endregion
	}
}
