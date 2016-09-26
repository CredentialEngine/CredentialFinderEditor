using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Models;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBentity = Data.Entity;
using Entity = Models.Common.Entity;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	/// <summary>
	/// manager for entities
	/// NOTE: May 7, 2016 mparsons - using after insert triggers to create the entity related a new created major entities like:
	/// - Credential
	/// - Organization
	/// - Assessment
	/// - ConnectionProfile
	/// - LearningOpportunity
	/// However, the issue will be not having the EntityId for the entity child components
	/// </summary>
	public class EntityManager : BaseFactory
	{
		string thisClassName = "EntityManager";
		#region 
		/// <summary>
		/// Add an Entity mirror
		/// </summary>
		/// <param name="entityUid">RowId of the base Object</param>
		/// <param name="baseId">Integer PK of the base object</param>
		/// <param name="entityTypeId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Add( Guid entityUid, int baseId, int entityTypeId, ref string statusMessage )
		{

			DBentity efEntity = new DBentity();
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					efEntity.EntityUid = entityUid;
					//TODO
					//efEntity.BaseId = baseId;

					efEntity.EntityTypeId = entityTypeId;
					efEntity.Created = System.DateTime.Now;

					context.Entity.Add( efEntity );

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
						statusMessage = "Error - the add was not successful. ";
						string message = thisClassName + string.Format( ". Add Failed", "Attempted to add an Entity. The process appeared to not work, but was not an exception, so we have no message, or no clue. entityUid: {0}, entityTypeId: {1}", entityUid.ToString(), entityTypeId );
						EmailManager.NotifyAdmin( "AssessmentManager. Assessment_Add Failed", message );
						return 0;
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(). entityUid: {0}, entityTypeId: {1}", entityUid.ToString(), entityTypeId ));
				}
			}

			return 0;
		}

		/// <summary>
		/// Delete an Entity
		/// </summary>
		/// <param name="entityUid"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( Guid entityUid, ref string statusMessage )
		{
			bool isValid = false;
			if ( !IsValidGuid(entityUid))
			{
				statusMessage = "Error - missing a valid identifier for the Entity";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				DBentity efEntity = context.Entity
							.SingleOrDefault( s => s.EntityUid == entityUid );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					int entityTypeId = efEntity.EntityTypeId;
					string entityType = efEntity.Codes_EntityType.Title;

					context.Entity.Remove( efEntity );
					int count = context.SaveChanges();
					if ( count >= 0 )
					{
						isValid = true;
						LoggingHelper.DoTrace( 4, thisClassName + string.Format( ".Delete - likely a related indirect delete from another object. TypeId: {0}, Type: {1} ", entityTypeId, entityType ) );
					}
				}
				else
				{
					statusMessage = "Error - delete failed, as record was not found.";
					LoggingHelper.LogError( thisClassName + string.Format( ".Delete - WIERD - delete failed, as record was not found. entityUid: {0}", entityUid ), true );
				}
			}

			return isValid;
		}
		#endregion 
		#region retrieval
		public static int GetEntityId( Guid entityUid )
		{
			int entityId = 0;
			using ( var context = new Data.CTIEntities() )
			{
				DBentity item = context.Entity
						.SingleOrDefault( s => s.EntityUid == entityUid );

				if ( item != null && item.Id > 0 )
				{
					entityId = item.Id;
				}
			}

			return entityId;
		}
		public static Entity GetEntity( Guid entityUid )
		{
			Entity entity = new Entity();
			using ( var context = new Data.CTIEntities() )
			{
				DBentity item = context.Entity
						.SingleOrDefault( s => s.EntityUid == entityUid );

				if ( item != null && item.Id > 0 )
				{
					entity.Id = item.Id;
					entity.EntityTypeId = item.EntityTypeId;
					entity.EntityUid = item.EntityUid;
					entity.Created = (DateTime)item.Created;

				}
				return entity;
			}


		}
		public static Views.Entity_Summary GetDBEntity( Guid entityUid )
		{
			using ( var context = new ViewContext() )
			{
				Views.Entity_Summary item = context.Entity_Summary
						.SingleOrDefault( s => s.EntityUid == entityUid );

				if ( item != null && item.Id > 0 )
				{
					
				}
				return item;
			}
		}

		/// <summary>
		/// Get an Entity Summary object by entityUid
		/// </summary>
		/// <param name="entityUid"></param>
		/// <returns></returns>
		public static EntitySummary GetEntitySummary( Guid entityUid )
		{
			EntitySummary entity = new EntitySummary();
			using ( var context = new ViewContext() )
			{
				Views.Entity_Summary item = context.Entity_Summary
						.SingleOrDefault( s => s.EntityUid == entityUid );

				if ( item != null && item.Id > 0 )
				{
					entity.Id = item.Id;
					entity.EntityTypeId = item.EntityTypeId;
					entity.EntityType = item.EntityType;
					entity.EntityUid = item.EntityUid;
					entity.Name = item.Name;
					entity.BaseId = item.BaseId;
					entity.Description = item.Description;
					entity.StatusId = (int) (item.StatusId ?? 1);
					if ( IsValidDate( item.Created ))
						entity.Created = (DateTime) item.Created;
					entity.CreatedById = item.CreatedById ?? 0;

					entity.ManagingOrgId = ( int ) ( item.ManagingOrgId ?? 0 );

					entity.parentEntityId = item.parentEntityId;
					entity.parentEntityUid = item.parentEntityUid;
					entity.parentEntityType = item.parentEntityType;
					entity.parentEntityTypeId = item.parentEntityTypeId;
				}
				return entity;
			}
		}

		/// <summary>
		/// Get an Entity Summary object by entityId 
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public static EntitySummary GetEntitySummary( int entityId)
		{
			EntitySummary entity = new EntitySummary();
			using ( var context = new ViewContext() )
			{
				Views.Entity_Summary item = context.Entity_Summary
							.SingleOrDefault( s => s.Id == entityId );
			
				if ( item != null && item.Id > 0 )
				{
					entity.Id = item.Id;
					entity.EntityTypeId = item.EntityTypeId;
					entity.EntityType = item.EntityType;
					entity.EntityUid = item.EntityUid;
					entity.Name = item.Name;
					entity.BaseId = item.BaseId;
					entity.Description = item.Description;
					entity.StatusId = ( int ) ( item.StatusId ?? 1 );
					if ( IsValidDate( item.Created ) )
						entity.Created = ( DateTime ) item.Created;
					entity.CreatedById = item.CreatedById ?? 0;

					entity.ManagingOrgId = (int) (item.ManagingOrgId ?? 0);

					entity.parentEntityId = item.parentEntityId;
					entity.parentEntityUid = item.parentEntityUid;
					entity.parentEntityType = item.parentEntityType;
					entity.parentEntityTypeId = item.parentEntityTypeId;
				}
				return entity;
			}
		}

		public static EntitySummary GetEntitySummary( int entityBaseId, int entityTypeId )
		{
			EntitySummary entity = new EntitySummary();
			Views.Entity_Summary efEntity = GetDBEntityByBaseId( entityBaseId, entityTypeId );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					entity.Id = efEntity.Id;
					entity.EntityTypeId = efEntity.EntityTypeId;
					entity.EntityType = efEntity.EntityType;
					entity.EntityUid = efEntity.EntityUid;
					entity.Name = efEntity.Name;
					entity.BaseId = efEntity.BaseId;
					entity.Description = efEntity.Description;
					entity.StatusId = ( int ) ( efEntity.StatusId ?? 1 );
					if ( IsValidDate( efEntity.Created ) )
						entity.Created = ( DateTime ) efEntity.Created;
					entity.CreatedById = efEntity.CreatedById ?? 0;

					entity.ManagingOrgId = ( int ) ( efEntity.ManagingOrgId ?? 0 );

					entity.parentEntityId = efEntity.parentEntityId;
					entity.parentEntityUid = efEntity.parentEntityUid;
					entity.parentEntityType = efEntity.parentEntityType;
					entity.parentEntityTypeId = efEntity.parentEntityTypeId;
				}
				return entity;
			
		}
		public static Views.Entity_Summary GetDBEntity( int entityId )
		{
			using ( var context = new ViewContext() )
			{
				Views.Entity_Summary item = context.Entity_Summary
						.SingleOrDefault( s => s.Id == entityId );

				if ( item != null && item.Id > 0 )
				{

				}
				return item;
			}
		}

		/// <summary>
		/// Get Entity by the base or child id (ex: Credential.Id)
		/// </summary>
		/// <param name="baseId"></param>
		/// <returns></returns>
		public static Views.Entity_Summary GetDBEntityByBaseId( int baseId, int entityTypeId )
		{
			using ( var context = new ViewContext() )
			{
				Views.Entity_Summary item = context.Entity_Summary
						.SingleOrDefault( s => s.BaseId == baseId
						&& s.EntityTypeId == entityTypeId );

				if ( item != null && item.Id > 0 )
				{

				}
				return item;
			}
		}

		/// <summary>
		/// Get the top level entity for a entity component. 
		/// This currently means one of:
		/// 1 - credential
		/// 2 - Organization
		/// 3 - Assessment
		/// 4 - Learning Opp
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public static Views.Entity_Summary GetTopLevelParentEntity( Guid entityUid)
		{
			Views.Entity_Summary topParent = new Views.Entity_Summary();

			using ( var context = new ViewContext() )
			{
				while ( topParent != null )
				{
					topParent = context.Entity_Summary
						.SingleOrDefault( s => s.EntityUid == entityUid );

					if ( topParent == null && topParent.Id == 0 )
					{
						//should not happen, so ???

						break;
					}
				}

				return topParent;
			}
		}

		#endregion 

	}
}
