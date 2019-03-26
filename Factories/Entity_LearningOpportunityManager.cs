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
using DBEntity = Data.Entity_LearningOpportunity;
using ThisEntity = Models.ProfileModels.LearningOpportunityProfile;

using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class Entity_LearningOpportunityManager : BaseFactory
	{
		static string thisClassName = "Entity_LearningOpportunityManager";
		/// <summary>
		/// if true, return an error message if the lopp is already associated with the parent
		/// </summary>
		private bool ReturningErrorOnDuplicate { get; set; }
		public Entity_LearningOpportunityManager()
		{
			ReturningErrorOnDuplicate = false;
		}
		public Entity_LearningOpportunityManager( bool returnErrorOnDuplicate )
		{
			ReturningErrorOnDuplicate = returnErrorOnDuplicate;
		}

		#region Entity LearningOpp Persistance ===================
		/// <summary>
		/// Add a learning opp to a parent (typically a stub was created, so can be associated before completing the full profile)
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="learningOppId">The just create lopp</param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public int Add( Guid parentUid, 
					int learningOppId, 
					int userId,
					bool allowMultiples,
					ref List<string> messages,
					bool warnOnDuplicate = true
			)
		{
			int id = 0;
			int count = messages.Count();
			if ( learningOppId == 0 )
			{
				messages.Add( string.Format( "A valid Learning Opportunity identifier was not provided to the {0}.Add method.", thisClassName ) );
			}
			if ( messages.Count > count )
				return 0;

            Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found.");
				return 0;
			}

            //this check should have already been done, but do again just in case
            if ( parent.EntityTypeId == 7  )
            {
                if ( parent.Id == learningOppId )
                {
                    messages.Add( "You cannot add the main learning opportunity as a part to itself." );
                    return 0;
                }
            } else if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_CONDITION_PROFILE)
            {
                //get its parent and do check
                ConditionProfile cp = Entity_ConditionProfileManager.GetBasic( parent.EntityUid );
                if (cp.ParentEntity != null && cp.ParentEntity.Id > 0)
                {
                    if ( cp.ParentEntity.EntityTypeId == 7
                && cp.ParentEntity.EntityBaseId == learningOppId )
                    {
                        messages.Add( "You cannot add the main learning opportunity as a part to itself." );
                        return 0;
                    }
                }
            }
            using ( var context = new Data.CTIEntities() )
			{
				DBEntity efEntity = new DBEntity();
				try
				{
					//first check for duplicates
					efEntity = context.Entity_LearningOpportunity
							.FirstOrDefault( s => s.EntityId == parent.Id && s.LearningOpportunityId == learningOppId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						if ( warnOnDuplicate )
						{
							if ( ReturningErrorOnDuplicate )
								messages.Add( string.Format( "Error - this Learning Opportunity has already been added to this profile. {0}", parent.EntityBaseName ) );
						}
						return efEntity.Id;
					}

					if ( allowMultiples == false )
					{
						//check if one exists, and replace if found
						efEntity = context.Entity_LearningOpportunity
							.FirstOrDefault( s => s.EntityId == parent.Id );
						if ( efEntity != null && efEntity.Id > 0 )
						{
							efEntity.LearningOpportunityId = learningOppId;

							count = context.SaveChanges();

							return efEntity.Id;
						}
					}
					efEntity = new DBEntity();
					efEntity.EntityId = parent.Id;
					efEntity.LearningOpportunityId = learningOppId;

					efEntity.CreatedById = userId;
					efEntity.Created = System.DateTime.Now;

					context.Entity_LearningOpportunity.Add( efEntity );

					// submit the change to database
					count = context.SaveChanges();
					if ( count > 0 )
					{
						messages.Add( "Successful" );
						id = efEntity.Id;
						return efEntity.Id;
					}
					else
					{
						//?no info on error
						messages.Add( "Error - the add was not successful." );
						string message = thisClassName + string.Format( ".Add Failed", "Attempted to add a LearningOpp for a connection profile. The process appeared to not work, but there was no exception, so we have no message, or no clue. Parent Profile: {0}, learningOppId: {1}, createdById: {2}", parent.Id, learningOppId, userId );
						EmailManager.NotifyAdmin( thisClassName + ".Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "Entity_LearningOpp" );
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( dbex, thisClassName + string.Format( ".Save(), Parent: {0} ({1})", parent.EntityBaseName, parent.EntityBaseId ) );

				}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex );
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save(), Parent: {0} ({1})", parent.EntityBaseName, parent.EntityBaseId ) );
				}


			}
			return id;
		}
		
		/// <summary>
		/// Delete a learning opportunity from a parent entity
		/// </summary>
		/// <param name="parentId"></param>
		/// <param name="entityTypeId"></param>
		/// <param name="learningOppId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( Guid parentUid, int learningOppId, ref string statusMessage )
		{
			bool isValid = false;
			if ( learningOppId == 0 )
			{
				statusMessage = "Error - missing an identifier for the LearningOpp to remove";
				return false;
			}
			//need to get Entity.Id 
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				statusMessage = "Error - the parent entity was not found.";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity efEntity = context.Entity_LearningOpportunity
								.SingleOrDefault( s => s.EntityId == parent.Id && s.LearningOpportunityId == learningOppId );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					context.Entity_LearningOpportunity.Remove( efEntity );
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					}
				}
				else
				{
					statusMessage = "Warning - the record was not found - probably because the target had been previously deleted";
					isValid = true;
				}
			}

			return isValid;
		}
		public bool DeleteAll( Guid parentUid, ref List<string> messages )
		{
			bool isValid = true;
			messages = new List<string>();
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				context.Entity_LearningOpportunity.RemoveRange( context.Entity_LearningOpportunity.Where( s => s.EntityId == parent.Id ) );
				int count = context.SaveChanges();
				if ( count > 0 )
				{
					isValid = true;
					messages.Add( string.Format( "removed {0} related learning opportunities.", count ) );
				}
			}
			return isValid;

		}

		/// <summary>
		/// Delete all records that are not in the provided list. 
		/// This method is typically called from bulk upload, and want to remove any records not in the current list to upload.
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="list"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool DeleteNotInList( Guid parentUid, List<LearningOpportunityProfile> list, ref List<string> messages )
		{
			bool isValid = true;
			if ( !list.Any() )
			{
				return true;
			}
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( thisClassName + string.Format( ".DeleteNotInList() Error - the parent entity for [{0}] was not found.", parentUid ) );
				return false;
			}

			using ( var context = new Data.CTIEntities() )
			{
				var existing = context.Entity_LearningOpportunity.Where( s => s.EntityId == parent.Id ).ToList();
				var inputIds = list.Select( x => x.Id ).ToList();

				//delete records which are not selected 
				var notExisting = existing.Where( x => !inputIds.Contains( x.LearningOpportunityId ) ).ToList();
				foreach ( var item in notExisting )
				{
					context.Entity_LearningOpportunity.Remove( item );
					context.SaveChanges();
				}

			}
			return isValid;

		}

		#endregion
		/// <summary>
		/// Get all learning opportunties for the parent
		/// Uses the parent Guid to retrieve the related Entity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="forEditView"></param>
		/// <param name="forProfilesList"></param>
		/// <returns></returns>
		public static List<ThisEntity> LearningOpps_GetAll( Guid parentUid, 
					bool forEditView, 
					bool forProfilesList,
					bool isForCredentialDetails  = false,
                    bool forSummaryView = false)
		{
			List<ThisEntity> list  = new List<ThisEntity>();
			ThisEntity entity = new ThisEntity();

			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}

			bool includingProperties = isForCredentialDetails;
			bool includingProfiles = false;
            bool includingCosts = true;

            if ( !forEditView )
			{
				includingProperties = true;
				includingProfiles = true;
			}
            if ( forSummaryView )
                includingCosts = false;

            LoggingHelper.DoTrace( 7, string.Format( "Entity_LearningOpps_GetAll: parentUid:{0} entityId:{1}, e.EntityTypeId:{2}", parentUid, parent.Id, parent.EntityTypeId ) );
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBEntity> results = context.Entity_LearningOpportunity
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.LearningOpportunity.Name )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBEntity item in results )
						{
							entity = new ThisEntity();
                            if ( forSummaryView )
                            {
                                LearningOpportunityManager.MapFromDB_ForSummary(item.LearningOpportunity, entity);
								LearningOpportunityManager.MapFromDB_Competencies( entity );
								list.Add( entity );
                            }
                            else if ( forProfilesList || isForCredentialDetails )
                            {
                                //17-08-31 mp - change to use lopp basic mapper
                                LearningOpportunityManager.MapFromDB_Basic(item.LearningOpportunity, entity,
                                    includingCosts, //includingCosts-not sure -yes need for details page
                                    forEditView);

                                //also get costs - really only need the profile list view 
                                if ( isForCredentialDetails )
                                {
                                    //part MapFromDB_Basic 
                                    //entity.EstimatedCost = CostProfileManager.GetAllForList( entity.RowId, forEditView );

                                    entity.CommonConditions = Entity_CommonConditionManager.GetAll(entity.RowId, false);
                                    //part MapFromDB_Basic 
                                    //entity.CommonCosts = Entity_CommonCostManager.GetAll( entity.RowId, false );
                                    LearningOpportunityManager.MapFromDB_HasPart(entity, false, false);
                                    LearningOpportunityManager.MapFromDB_Competencies(entity);
                                }

                                list.Add(entity);
                                //get durations - need this for search and compare
                                entity.EstimatedDuration = DurationProfileManager.GetAll(entity.RowId);


                            }
                            else
                            {
                                if ( !forEditView
                                  && CacheManager.IsLearningOpportunityAvailableFromCache(item.LearningOpportunityId, ref entity) )
                                {
                                    list.Add(entity);
                                }
                                else
                                {
                                    //to determine minimum needed for a or detail page
                                    LearningOpportunityManager.MapFromDB(item.LearningOpportunity, entity,
                                        includingProperties,
                                        includingProfiles,
                                        forEditView, //forEditView
                                        false //includeWhereUsed
                                        );

                                    list.Add(entity);

                                    if ( !forEditView && entity.HasPart.Count > 0 )
                                    {
                                        CacheManager.AddLearningOpportunityToCache(entity);
                                    }
                                }
                            }
						}
					}
					return list;
				}

				
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".LearningOpps_GetAll" );
			}
			return list;
		}

		public static LearningOpportunityProfile MapFromDB_FirstRecord( ICollection<EM.Entity_LearningOpportunity> results )
		{
			ThisEntity entity = new ThisEntity();

			if ( results != null && results.Count > 0 )
			{
				foreach ( EM.Entity_LearningOpportunity item in results )
				{
					entity = new ThisEntity();
					if ( item.LearningOpportunity != null && item.LearningOpportunity.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED )
					{

						LearningOpportunityManager.MapFromDB_Basic( item.LearningOpportunity, entity,
								false, //includeCosts - propose to use for credential editor
								false
								);
						return entity;
						//break;
					}
				}
			}


			return null;

		}

		/// <summary>
		/// Get all learning opportunties where the source learning opportunity is a part
		/// Steps: 1 use the learning opportunity Id to get All the Entity_LearningOpp, use the entity Uses the parent Guid to retrieve the related Entity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		/// <returns></returns>
		public static List<ThisEntity> LearningOpps_GetAll_IsPart( int learningOpportunityId, int parentTypeId )
		{
			List<ThisEntity> list = new List<ThisEntity>();
			ThisEntity entity = new ThisEntity();

			try
			{
				using ( var context = new ViewContext() )
				{
					List<Views.Entity_LearningOpportunity_IsPartOfSummary> results = context.Entity_LearningOpportunity_IsPartOfSummary
							.Where( s => s.LearningOpportunityId == learningOpportunityId 
								&& s.EntityTypeId == parentTypeId )
							.OrderBy( s => s.EntityTypeId ).ThenBy( s => s.ParentName)
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( Views.Entity_LearningOpportunity_IsPartOfSummary item in results )
						{
							entity = new ThisEntity();
							//LearningOpportunityManager.Entity_ToMap( item.LearningOpportunity, entity, false, false );


							list.Add( entity );
						}
					}
					return list;
				}


			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".LearningOpps_GetAll" );
			}
			return list;
		}
	
	}
}
