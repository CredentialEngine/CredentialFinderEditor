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
using DBentity = Data.Entity_LearningOpportunity;
using Entity = Models.ProfileModels.LearningOpportunityProfile;

using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class Entity_LearningOpportunityManager
	{
		static string thisClassName = "Entity_LearningOpportunityManager";

		/// <summary>
		/// Get all learning opportunties for the parent
		/// Uses the parent Guid to retrieve the related Entity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="forEditView"></param>
		/// <param name="newVersion"></param>
		/// <returns></returns>
		public static List<Entity> LearningOpps_GetAll( Guid parentUid, bool forEditView, bool newVersion = true )
		{
			List<Entity> list  = new List<Entity>();
			Entity entity = new Entity();

			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}

			bool includingProperties = false;
			bool includingProfiles = false;
			if ( !forEditView )
			{
				includingProperties = true;
				includingProfiles = true;
			}

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBentity> results = context.Entity_LearningOpportunity
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.LearningOpportunity.Name )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity item in results )
						{
							entity = new Entity();
							LearningOpportunityManager.ToMap( item.LearningOpportunity, entity, 
								includingProperties, 
								includingProfiles,
 								false, //forEditView
								false, //includeWhereUsed
								newVersion );

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
		/// <summary>
		/// Get all learning opportunties where the source learning opportunity is a part
		/// Steps: 1 use the learning opportunity Id to get All the Entity_LearningOpp, use the entity Uses the parent Guid to retrieve the related Entity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		/// <returns></returns>
		public static List<Entity> LearningOpps_GetAll_IsPart( int learningOpportunityId, int parentTypeId )
		{
			List<Entity> list = new List<Entity>();
			Entity entity = new Entity();

			//Views.Entity_Summary e = EntityManager.GetDBEntity( parentUid );
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
							entity = new Entity();
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
	
		#region Entity LearningOpp Persistance ===================
		public int EntityLearningOpp_Add( int parentId, int entityTypeId,
					int learningOppId, 
					int userId, 
					ref List<string> messages )
		{
			int id = 0;
			int count = messages.Count();
			if ( parentId == 0 || entityTypeId == 0 )
			{
				messages.Add( string.Format( "A valid profile identifier was not provided to the {0}.EntityLearningOpp_Add method.", thisClassName ) );
			}
			if ( learningOppId == 0 )
			{
				messages.Add( string.Format( "A valid Learning Opportunity identifier was not provided to the {0}.EntityLearningOpp_Add method.", thisClassName ) );
			}
			if ( messages.Count > count )
				return 0;
			if ( parentId == learningOppId )
			{
				messages.Add("You cannot add the main learning opportunity as a part to itself." );
				return 0;
			}
			Views.Entity_Summary parent = EntityManager.GetDBEntityByBaseId( parentId, entityTypeId );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found.");
				return 0;
			}

			using ( var context = new Data.CTIEntities() )
			{
				DBentity efEntity = new DBentity();
				try
				{
					//first check for duplicates
					efEntity = context.Entity_LearningOpportunity
							.SingleOrDefault( s => s.EntityId == parent.Id && s.LearningOpportunityId == learningOppId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						messages.Add( string.Format( "Error - this LearningOpp has already been added to this profile.", thisClassName ) );
						return 0;
					}

					efEntity = new DBentity();
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
						string message = thisClassName + string.Format( ".EntityLearningOpp_Add Failed", "Attempted to add a LearningOpp for a connection profile. The process appeared to not work, but there was no exception, so we have no message, or no clue. Parent Profile: {0}, learningOppId: {1}, createdById: {2}", parentId, learningOppId, userId );
						EmailManager.NotifyAdmin( thisClassName + ".EntityLearningOpp_Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = thisClassName + string.Format( ".EntityLearningOpp_Add() DbEntityValidationException, Parent Profile: {0}, learningOppId: {1}, createdById: {2}", parentId, learningOppId, userId );
					messages.Add( "Error - missing fields." );
					foreach ( var eve in dbex.EntityValidationErrors )
					{
						message += string.Format( "\rEntity of type \"{0}\" in state \"{1}\" has the following validation errors:",
							eve.Entry.Entity.GetType().Name, eve.Entry.State );
						foreach ( var ve in eve.ValidationErrors )
						{
							message += string.Format( "- Property: \"{0}\", Error: \"{1}\"",
								ve.PropertyName, ve.ErrorMessage );
						}

						LoggingHelper.LogError( message, true );
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".EntityLearningOpp_Add(), Parent Profile: {0}, Type: {1}, learningOppId: {2}, createdById: {3}", parentId, parent.EntityType, learningOppId, userId ) );
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
		public bool EntityLearningOpp_Delete( int parentId, int entityTypeId, int learningOppId, ref string statusMessage )
		{
			bool isValid = false;
			if ( parentId == 0 || entityTypeId == 0 )
			{
				statusMessage = "Error - missing an identifier for the connection profile to remove the LearningOpp.";
				return false;
			}
			if ( learningOppId == 0 )
			{
				statusMessage = "Error - missing an identifier for the LearningOpp to remove";
				return false;
			}
			//need to get Entity.Id 
			Views.Entity_Summary parent = EntityManager.GetDBEntityByBaseId( parentId, entityTypeId );
			if ( parent == null || parent.Id == 0 )
			{
				statusMessage = "Error - the parent entity was not found.";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				DBentity efEntity = context.Entity_LearningOpportunity
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
					statusMessage = "Error - delete failed, as record was not found.";
				}
			}

			return isValid;
		}

		public bool EntityLearningOpp_Delete( int Id, ref string statusMessage )
		{
			bool isValid = false;
			if ( Id == 0 )
			{
				statusMessage = "Error - missing an identifier for the Learning Opportunity to remove";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				DBentity efEntity = context.Entity_LearningOpportunity
							.SingleOrDefault( s => s.Id == Id );

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
					statusMessage = "Error - delete failed, as record was not found.";
				}
			}

			return isValid;
		}

		#endregion
	}
}
