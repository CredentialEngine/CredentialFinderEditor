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
using DBEntity = Data.Entity_Assessment;
using ThisEntity = Models.ProfileModels.Entity_Assessment;

using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;

namespace Factories
{
	public class Entity_AssessmentManager : BaseFactory
	{
        /// <summary>
        /// if true, return an error message if the assessment is already associated with the parent
        /// </summary>
        private bool ReturningErrorOnDuplicate { get; set; }
        public Entity_AssessmentManager ()
        {
            ReturningErrorOnDuplicate = false;
        }
        public Entity_AssessmentManager( bool returnErrorOnDuplicate )
        {
            ReturningErrorOnDuplicate = returnErrorOnDuplicate;
        }
        static string thisClassName = "Entity_AssessmentManager";
		/// <summary>
		/// Get all assessments for the provided entity
		/// The returned entities are just the base
		/// </summary>
		/// <param name="parentUid"></param>
		/// <returns></returns>
		public static List<AssessmentProfile> GetAll( Guid parentUid, 
			bool forEditView,
			bool isForCredentialDetails,
            bool forSummaryView = false)
		{
			List<AssessmentProfile> list = new List<AssessmentProfile>();
			AssessmentProfile entity = new AssessmentProfile();
			bool includingProperties = false;
			bool includingRoles = false;
            bool includingCosts = true;
			if ( !forEditView )
			{
				includingProperties = true;
				includingRoles = true;
			}
            if ( forSummaryView )
                includingCosts = false;

            Entity parent = EntityManager.GetEntity( parentUid );
			LoggingHelper.DoTrace( 7, string.Format( "EntityAssessments_GetAll: parentUid:{0} entityId:{1}, e.EntityTypeId:{2}", parentUid, parent.Id, parent.EntityTypeId ) );

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBEntity> results = context.Entity_Assessment
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.Assessment.Name )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBEntity item in results )
						{
							entity = new AssessmentProfile();
                            if ( forSummaryView )
							{ 
                                AssessmentManager.MapFromDB_ForSummary(item.Assessment, entity);
								AssessmentManager.MapFromDB_Competencies( entity );
							}
                            else if ( forEditView || !isForCredentialDetails )
								AssessmentManager.MapFromDB_Basic( item.Assessment, entity,
                                includingCosts, //includeCosts - propose to use for credential editor
								forEditView
								);
							else
							{
								//need to distinguish between on a detail page for conditions and assessment detail
								//would usually only want basics here??
								//17-05-26 mp- change to MapFromDB_Basic
								AssessmentManager.MapFromDB_Basic( item.Assessment, entity,
									true, //includingCosts-not sure -yes need for details page
									forEditView);
								//add competencies
								AssessmentManager.MapFromDB_Competencies( entity );

								//AssessmentManager.MapFromDB( item.Assessment, entity,
								//isForCredentialDetails, //includingProperties-not sure
								//false, //includingRoles
								//forEditView,
								//false //includeWhereUsed
								//);
							}
								

							list.Add( entity );
						}
					}
					return list;
				}


			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".EntityAssessments_GetAll" );
			}
			return list;
		}

		public static ThisEntity Get( int parentId, int assessmentId )
		{
			ThisEntity entity = new ThisEntity();
			if ( parentId < 1 || assessmentId < 1 )
			{
				return entity;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					EM.Entity_Assessment from = context.Entity_Assessment
							.SingleOrDefault( s => s.AssessmentId == assessmentId && s.EntityId == parentId );

					if ( from != null && from.Id > 0 )
					{
						entity.Id = from.Id;
						entity.AssessmentId = from.AssessmentId;
						entity.EntityId = from.EntityId;

						entity.ProfileSummary = from.Assessment.Name;
						//to.Credential = from.Credential;
						entity.Assessment = new AssessmentProfile();
						AssessmentManager.MapFromDB_Basic( from.Assessment, entity.Assessment,
								false, //includeCosts - propose to use for credential editor
								false
								);

						if ( IsValidDate( from.Created ) )
							entity.Created = ( DateTime ) from.Created;
						entity.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Get" );
			}
			return entity;
		}//
		public static AssessmentProfile MapFromDB_FirstRecord( ICollection<EM.Entity_Assessment> results )
		{
			ThisEntity entity = new ThisEntity();

			if ( results != null && results.Count > 0 )
			{
				foreach ( EM.Entity_Assessment item in results )
				{
					entity = new ThisEntity();
					if ( item.Assessment != null && item.Assessment.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED )
					{
						entity.AssessmentId = item.AssessmentId;
						AssessmentManager.MapFromDB_Basic( item.Assessment, entity.Assessment,
								false, //includeCosts - propose to use for credential editor
								false
								);
						return entity.Assessment;
						break;
					}
				}
			}


			return null;

		}
		#region Entity Assessment Persistance ===================

		/// <summary>
		/// Add an Entity assessment
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="assessmentId"></param>
		/// <param name="userId"></param>
		/// <param name="allowMultiples">If false, check if an assessment exists. If found, do an update</param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public int Add( Guid parentUid, 
					int assessmentId,
					int userId,
					bool allowMultiples,
					ref List<string> messages )
		{
			int id = 0;
			int count = messages.Count();
			if ( assessmentId == 0 )
			{
				messages.Add( string.Format( "A valid Assessment identifier was not provided to the {0}.EntityAssessment_Add method.", thisClassName ) );
			}
			if ( messages.Count > count )
				return 0;

			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return 0;
			}
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity efEntity = new DBEntity();
				try
				{
					//first check for duplicates
					efEntity = context.Entity_Assessment
							.SingleOrDefault( s => s.EntityId == parent.Id && s.AssessmentId == assessmentId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
                        if ( ReturningErrorOnDuplicate )
						    messages.Add( string.Format( "Error - this Assessment has already been added to this profile.", thisClassName ) );

						return efEntity.Id;
					}

					if ( allowMultiples == false )
					{
						//check if one exists, and replace if found
						efEntity = context.Entity_Assessment
							.FirstOrDefault( s => s.EntityId == parent.Id  );
						if ( efEntity != null && efEntity.Id > 0 )
						{
							efEntity.AssessmentId = assessmentId;

							count = context.SaveChanges();

							return efEntity.Id;
						}
					}
					efEntity = new DBEntity();
					efEntity.EntityId = parent.Id;
					efEntity.AssessmentId = assessmentId;

					efEntity.CreatedById = userId;
					efEntity.Created = System.DateTime.Now;

					context.Entity_Assessment.Add( efEntity );

					// submit the change to database
					count = context.SaveChanges();
					if ( count > 0 )
					{
						//messages.Add( "Successful" );
						id = efEntity.Id;
						return efEntity.Id;
					}
					else
					{
						//?no info on error
						messages.Add( "Error - the add was not successful." );
						string message = thisClassName + string.Format( ".Add Failed", "Attempted to add a Assessment for a profile. The process appeared to not work, but there was no exception, so we have no message, or no clue. Parent Profile: {0}, Type: {1}, learningOppId: {2}, createdById: {3}", parentUid, parent.EntityType, assessmentId, userId );
						EmailManager.NotifyAdmin( thisClassName + ".Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "Entity_Assessment" );
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
		public bool Delete( Guid parentUid, int assessmentId, ref string statusMessage )
		{
			bool isValid = false;
			if ( assessmentId == 0 )
			{
				statusMessage = "Error - missing an identifier for the Assessment to remove";
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
				DBEntity efEntity = context.Entity_Assessment
								.SingleOrDefault( s => s.EntityId == parent.Id && s.AssessmentId == assessmentId );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					context.Entity_Assessment.Remove( efEntity );
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
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				context.Entity_Assessment.RemoveRange( context.Entity_Assessment.Where( s => s.EntityId == parent.Id ) );
				int count = context.SaveChanges();
				if ( count > 0 )
				{
					isValid = true;
					messages.Add( string.Format( "removed {0} related assessments.", count ) );
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
		public bool DeleteNotInList( Guid parentUid, List<AssessmentProfile> list, ref List<string> messages )
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
				var existing = context.Entity_Assessment.Where( s => s.EntityId == parent.Id ).ToList();
				var inputIds = list.Select( x => x.Id ).ToList();

				//delete records which are not selected 
				var notExisting = existing.Where( x => !inputIds.Contains( x.AssessmentId ) ).ToList();
				foreach ( var item in notExisting )
				{
					context.Entity_Assessment.Remove( item );
					context.SaveChanges();
				}

			}
			return isValid;

		}
		#endregion
	}
}
