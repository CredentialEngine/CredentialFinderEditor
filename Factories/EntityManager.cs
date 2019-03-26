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
using DBEntity = Data.Entity;
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
		#region persistance
		/// <summary>
		/// Add an Entity mirror
		/// NOTE: ALL ENTITY ADDS SHOULD BE DONE VIA TRIGGERS
		/// 17-11-13 MP - there can be timing issues, where the trigger may create the entity, but won't be available in entity_cache, so may revisit the unconditional use of trigger
		/// </summary>
		/// <param name="entityUid">RowId of the base Object</param>
		/// <param name="baseId">Integer PK of the base object</param>
		/// <param name="entityTypeId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Add( Guid entityUid, int baseId, int entityTypeId, string name, ref string statusMessage )
		{

			DBEntity efEntity = new DBEntity();
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					efEntity.EntityUid = entityUid;
					efEntity.EntityBaseId = baseId;
					efEntity.EntityTypeId = entityTypeId;
					efEntity.EntityBaseName = name;
					
					efEntity.Created = efEntity.LastUpdated = System.DateTime.Now;

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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(). entityUid: {0}, entityTypeId: {1}", entityUid.ToString(), entityTypeId ) );
				}
			}

			return 0;
		}

        /// <summary>
        /// Update last updated for top level entity
        /// NOTE: usage is for factory updates that are not possible to check at services level
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="triggeringEvent"></param>
        public void UpdateTopLevelEntityLastUpdateDate(int entityId, string triggeringEvent, int userId)
        {
            EntitySummary item = new EntitySummary();
            string statusMessage = "";
            int cntr = 0;
            do
            {
                cntr++;
                item = EntityManager.GetEntitySummary(entityId);
                entityId = item.parentEntityId;
                LoggingHelper.DoTrace(9, string.Format("____GetTopLevelEntity: entityId:{0}, nextParent: {1}", entityId, item.parentEntityId));

            } while (item.IsTopLevelEntity == false
                    && item.parentEntityId > 0);

            if (item != null && item.Id > 0 && item.IsTopLevelEntity)
            {
                //set last updated, and log
                UpdateModifiedDate(item.EntityUid, ref statusMessage);
                new ActivityManager().SiteActivityAdd( new SiteActivity()
                {
                    ActivityType = item.EntityType,
                    Activity = "Child Event",
                    Event = "Set Entity Modified",
                    Comment = triggeringEvent,
                    ActivityObjectId = item.BaseId,
                    ActionByUserId = userId,
                    ActivityObjectParentEntityUid = item.EntityUid
                } );
            }

            //return item;
        }
        public bool UpdateModifiedDate(Guid entityUid, ref string statusMessage)
        {
            bool isValid = false;
            if (!IsValidGuid(entityUid))
            {
                statusMessage = "Error - missing a valid identifier for the Entity";
                return false;
            }
            using (var context = new Data.CTIEntities())
            {
                DBEntity efEntity = context.Entity
                            .FirstOrDefault(s => s.EntityUid == entityUid);

                if (efEntity != null && efEntity.Id > 0)
                {
                    efEntity.LastUpdated = DateTime.Now;
                    int count = context.SaveChanges();
                    if (count >= 0)
                    {
                        isValid = true;
                        //LoggingHelper.DoTrace(6, thisClassName + string.Format(".UpdateModifiedDate - update last updated for TypeId: {0}, BaseId: {1}", efEntity.EntityTypeId, efEntity.EntityBaseId));
                    }
                }
                else
                {
                    statusMessage = thisClassName + ".UpdateModifiedDate. Error - Entity  was not found.";
                    LoggingHelper.LogError(thisClassName + string.Format(".UpdateModifiedDate - record was not found. entityUid: {0}", entityUid), true);
                }
            }

            return isValid;
        }/// <summary>
         /// Delete an Entity
         /// This should be handled by triggers as well, or at least with the child entity
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
				DBEntity efEntity = context.Entity
							.FirstOrDefault( s => s.EntityUid == entityUid );

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
					statusMessage = "Error - Entity delete unnecessary, as record was not found.";
					LoggingHelper.LogError( thisClassName + string.Format( ".Delete - WIERD - delete failed, as record was not found. entityUid: {0}", entityUid ), true );
				}
			}

			return isValid;
		}
		#endregion
		#region retrieval

		/// <summary>
		/// Get the first target entity related to a Connections condition profile
		/// </summary>
		/// <param name="entityUid"></param>
		/// <returns></returns>
		public static string GetDefaultTargetNameForEntity( Guid entityUid )
		{
			DBEntity entity = new DBEntity();
			string firstEntityName = "";
			using ( var context = new Data.CTIEntities() )
			{
				entity = context.Entity
						.FirstOrDefault( s => s.EntityUid == entityUid );

				if ( entity != null && entity.Id > 0 )
				{


					if ( entity.Entity_Credential != null && entity.Entity_Credential.Count > 0 )
					{
						Credential c = Entity_CredentialManager.MapFromDB_FirstCredential( entity.Entity_Credential );
						firstEntityName = c != null && c.Id > 0 ? c.Name : "Credential";
					}

					else if ( entity.Entity_Assessment != null && entity.Entity_Assessment.Count > 0 )
					{
						AssessmentProfile c = Entity_AssessmentManager.MapFromDB_FirstRecord( entity.Entity_Assessment );
						firstEntityName = c != null && c.Id > 0 ? c.Name : "Assessment";
					}
					else if ( entity.Entity_LearningOpportunity != null && entity.Entity_LearningOpportunity.Count > 0 )
					{
						firstEntityName = "Learning Opportunity";
						LearningOpportunityProfile c = Entity_LearningOpportunityManager.MapFromDB_FirstRecord( entity.Entity_LearningOpportunity );
						firstEntityName = c != null && c.Id > 0 ? c.Name : "Learning Opportunity";
					}
					
				}
				return firstEntityName;
			}


		}
		

		public static Entity GetEntity( Guid entityUid, bool includingAllChildren = true )
		{
			Entity entity = new Entity();
			using ( var context = new Data.CTIEntities() )
			{
                if ( includingAllChildren  == false)
                    context.Configuration.LazyLoadingEnabled = false;
                DBEntity item = context.Entity
						.FirstOrDefault( s => s.EntityUid == entityUid );

				if ( item != null && item.Id > 0 )
				{
                    MapFromDB( item, entity );
				}
				return entity;
			}


		}

		public static Entity GetEntity( int entityId)
		{
			Entity entity = new Entity();
            if ( entityId < 1 )
                return entity;

			using ( var context = new Data.CTIEntities() )
			{
				DBEntity item = context.Entity
						.FirstOrDefault( s => s.Id == entityId);

				if ( item != null && item.Id > 0 )
				{
                    MapFromDB( item, entity );

                }
				return entity;
			}


		}
       
		public static Entity GetEntity( int entityTypeId, int entityBaseId )
		{
			Entity entity = new Entity();
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity item = context.Entity
						.FirstOrDefault( s => s.EntityTypeId == entityTypeId 
							&& s.EntityBaseId == entityBaseId );

				if ( item != null && item.Id > 0 )
				{
                    MapFromDB( item, entity );
                }
				return entity;
			}
		}
        /// <summary>
        /// Get an Entity Summary object by entityId 
        /// NOTE: work to minimize use of this method - slow
        ///     - try entity_cache
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public static EntitySummary GetEntitySummary( int entityId )
        {
            EntitySummary entity = new EntitySummary();
            using ( var context = new Data.CTIEntities() )
            {
                var item = context.Entity_Cache
                            .FirstOrDefault( s => s.Id == entityId );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
                return entity;
            }

            //using ( var context = new ViewContext() )
            //{
            //	Views.Entity_Summary item = context.Entity_Summary
            //				.FirstOrDefault( s => s.Id == entityId );

            //	if ( item != null && item.Id > 0 )
            //	{
            //                 MapFromDB( item, entity );
            //	}
            //	return entity;
            //}
        }
        /// <summary>
        /// Get an Entity Summary object by entityId 
        /// NOTE: work to minimize use of this method - slow
        ///     - try entity_cache
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public static EntitySummary GetEntitySummary( int entityTypeId, int baseId )
        {
            EntitySummary entity = new EntitySummary();
            using ( var context = new Data.CTIEntities() )
            {
                var item = context.Entity_Cache
                            .FirstOrDefault( s => s.EntityTypeId == entityTypeId && s.BaseId == baseId );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
                return entity;
            }

        }

        public static void MapFromDB( DBEntity item, Entity entity )
        {
			try
			{
				if ( entity == null )
					entity = new Entity();
				entity.Id = item.Id;
				entity.EntityTypeId = item.EntityTypeId;
				if ( item.Codes_EntityType != null )
					entity.EntityType = item.Codes_EntityType.Title;
				else
				{
					entity.EntityType = CodesManager.CodeEntity_GetEntityType( entity.EntityTypeId );
				}
				entity.EntityUid = item.EntityUid;
				entity.EntityBaseId = item.EntityBaseId ?? 0;
				entity.EntityBaseName = item.EntityBaseName;
				entity.Created = ( DateTime )item.Created;
				entity.LastUpdated = ( DateTime )item.LastUpdated;
			} catch (Exception ex)
			{
				LoggingHelper.LogError( ex, "EntityManager.MapFromDB" );
			}
        }
        //public string GetEntityType(int entityTypeId)
        //{
            
        //    string entityType = "";
        //    switch ( entityTypeId )
        //    {
        //        case 1:
        //            entityType= "Credential";
        //            break;
        //        case 2:
        //            entityType = "Organization";
        //            break;
        //        case 3:
        //            entityType = "Assessment";
        //            break;
        //        case 4:
        //            entityType = "Condition Profile";
        //            break;
        //        case 7:
        //            entityType = "Learning Opportunity";
        //            break;
        //        case 1:
        //            entityType = "Credential";
        //            break;
        //    }
        //    return entityType;
        //}
        /// <summary>
        /// Get Entity_Summary
        /// NOTE: work to minimize use of this method - slow
        /// </summary>
        /// <param name="entityUid"></param>
        /// <returns></returns>
        //      public static Views.Entity_Summary GetDBEntity_Summary( Guid entityUid )
        //{
        //	using ( var context = new ViewContext() )
        //	{
        //		Views.Entity_Summary item = context.Entity_Summary
        //				.FirstOrDefault( s => s.EntityUid == entityUid );

        //		if ( item != null && item.Id > 0 )
        //		{

        //		}
        //		return item;
        //	}
        //}

        

        /// <summary>
        /// Get Entity_Summary
        /// NOTE: work to minimize use of this method - slow
        ///     - try entity_cache
        /// </summary>
        /// <param name="ctid"></param>
        /// <returns></returns>
		public static EntitySummary GetEntityByCtidOLD( string ctid )
		{
			EntitySummary entity = new EntitySummary();
			using ( var context = new ViewContext() )
			{
				Views.Entity_Summary item = context.Entity_Summary
							.FirstOrDefault( s => s.CTID == ctid );

				if ( item != null && item.Id > 0 )
				{
                    MapFromDB( item, entity );
                }
				return entity;
			}
		}

        public static EntitySummary GetEntityByCtid( string ctid )
        {
            EntitySummary entity = new EntitySummary();
            using ( var context = new Data.CTIEntities() )
            {
                var item = context.Entity_Cache
                            .FirstOrDefault( s => s.CTID == ctid );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
                return entity;
            }
        }
        /// <summary>
        /// Get Entity_Summary
        /// NOTE: work to minimize use of this method - slow
        /// </summary>
        /// <param name="entityUid"></param>
        /// <returns></returns>
        public static EntitySummary GetSummary( Guid entityUid )
        {
            EntitySummary entity = new EntitySummary();
            using ( var context = new Data.CTIEntities() )
            {
                var item = context.Entity_Cache
                            .FirstOrDefault( s => s.EntityUid == entityUid );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
                return entity;
            }

            //using ( var context = new ViewContext() )
            //{
            //    Views.Entity_Summary item = context.Entity_Summary
            //                .FirstOrDefault( s => s.EntityUid == entityUid );

            //    if ( item != null && item.Id > 0 )
            //    {
            //        MapFromDB( item, entity );
            //    }
            //    return entity;
            //}
        }
        //
        public static void MapFromDB( EM.Entity_Cache item, EntitySummary entity )
        {
            entity.Id = item.Id;
            entity.EntityTypeId = item.EntityTypeId;
            entity.EntityType = item.EntityType;
            entity.EntityUid = item.EntityUid;
            entity.Name = item.Name;
            entity.BaseId = item.BaseId;
            entity.Description = item.Description;
            entity.CTID = item.CTID;
            entity.StatusId = ( int ) ( item.StatusId ?? 1 );
            if ( IsValidDate( item.Created ) )
                entity.Created = ( DateTime ) item.Created;
            entity.CreatedById = item.CreatedById ?? 0;

            entity.OwningOrgId = ( int ) ( item.OwningOrgId ?? 0 );
            //entity.OwningOrganization = item.OwningOrganization ?? "";

            entity.parentEntityId = item.parentEntityId;
            entity.parentEntityUid = item.parentEntityUid;
            entity.parentEntityType = item.parentEntityType;
            entity.parentEntityTypeId = item.parentEntityTypeId; ;
        }
        public static void MapFromDB( Views.Entity_Summary item, EntitySummary entity )
        {
            entity.Id = item.Id;
            entity.EntityTypeId = item.EntityTypeId;
            entity.EntityType = item.EntityType;
            entity.EntityUid = item.EntityUid;
            entity.Name = item.Name;
            entity.BaseId = item.BaseId;
            entity.Description = item.Description;
            entity.CTID = item.CTID;
            entity.StatusId = ( int ) ( item.StatusId ?? 1 );
            if ( IsValidDate( item.Created ) )
                entity.Created = ( DateTime ) item.Created;
            entity.CreatedById = item.CreatedById ?? 0;

            entity.OwningOrgId = ( int ) ( item.OwningOrgId ?? 0 );
            entity.OwningOrganization = item.OwningOrganization ?? "";

            entity.parentEntityId = item.parentEntityId;
            entity.parentEntityUid = item.parentEntityUid;
            entity.parentEntityType = item.parentEntityType;
            entity.parentEntityTypeId = item.parentEntityTypeId; ;
        }
        /// <summary>
        /// Get the top level entity for a entity component. 
        /// This currently means one of (EntityTypeId/name):
        /// 1 - credential
        /// 2 - Organization
        /// 3 - Assessment
        /// 7 - Learning Opp
        /// 19 - Condition Manifest
        /// 20 - Cost Manifest
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
  //      public static Views.Entity_Summary GetTopLevelParentEntity( Guid entityUid)
		//{
		//	Views.Entity_Summary topParent = new Views.Entity_Summary();

		//	using ( var context = new ViewContext() )
		//	{
		//		while ( topParent != null )
		//		{
		//			topParent = context.Entity_Summary
		//				.FirstOrDefault( s => s.EntityUid == entityUid );

		//			if ( topParent == null && topParent.Id == 0 )
		//			{
		//				//should not happen, so ???

		//				break;
		//			}
		//		}

		//		return topParent;
		//	}
		//}

		#endregion 

	}
}
