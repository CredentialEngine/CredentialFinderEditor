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
using DBentity = Data.Entity_Assessment;

using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;

namespace Factories
{
	public class Entity_AssessmentManager
	{
		static string thisClassName = "Entity_AssessmentManager";
		/// <summary>
		/// Get all assessments for the provided entity
		/// The returned entities are just the base
		/// </summary>
		/// <param name="parentUid"></param>
		/// <returns></returns>
		public static List<AssessmentProfile> EntityAssessments_GetAll( Guid parentUid, bool forEditView, bool newVersion = false )
		{
			List<AssessmentProfile> list = new List<AssessmentProfile>();
			AssessmentProfile entity = new AssessmentProfile();
			bool includingProperties = false;
			bool includingRoles = false;
			if ( !forEditView )
			{
				includingProperties = true;
				includingRoles = true;
			}
			Views.Entity_Summary e = EntityManager.GetDBEntity( parentUid );
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBentity> results = context.Entity_Assessment
							.Where( s => s.EntityId == e.Id )
							.OrderBy( s => s.Assessment.Name )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity item in results )
						{
							entity = new AssessmentProfile();
							AssessmentManager.ToMap( item.Assessment, entity, 
								includingProperties, 
								includingRoles,
								forEditView, //forEditView
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
				LoggingHelper.LogError( ex, thisClassName + ".EntityAssessments_GetAll" );
			}
			return list;
		}

		#region Entity Assessment Persistance ===================
		public int EntityAssessment_Add( int parentId, int entityTypeId,
					int assessmentId,
					int userId,
					ref List<string> messages )
		{
			int id = 0;
			int count = messages.Count();
			if ( parentId == 0 || entityTypeId == 0 )
			{
				messages.Add( string.Format( "A valid profile identifier was not provided to the {0}.EntityAssessment_Add method.", thisClassName ) );
			}
			if ( assessmentId == 0 )
			{
				messages.Add( string.Format( "A valid Assessment identifier was not provided to the {0}.EntityAssessment_Add method.", thisClassName ) );
			}
			if ( messages.Count > count )
				return 0;

			Views.Entity_Summary parent = EntityManager.GetDBEntityByBaseId( parentId, entityTypeId );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add("Error - the parent entity was not found.");
				return 0;
			}
			using ( var context = new Data.CTIEntities() )
			{
				DBentity efEntity = new DBentity();
				try
				{
					//first check for duplicates
					efEntity = context.Entity_Assessment
							.SingleOrDefault( s => s.EntityId == parent.Id && s.AssessmentId == assessmentId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						messages.Add( string.Format( "Error - this Assessment has already been added to this profile.", thisClassName ) );
						return 0;
					}

					efEntity = new DBentity();
					efEntity.EntityId = parent.Id;
					efEntity.AssessmentId = assessmentId;

					efEntity.CreatedById = userId;
					efEntity.Created = System.DateTime.Now;

					context.Entity_Assessment.Add( efEntity );

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
						string message = thisClassName + string.Format( ".EntityAssessment_Add Failed", "Attempted to add a Assessment for a profile. The process appeared to not work, but there was no exception, so we have no message, or no clue. Parent Profile: {0}, Type: {1}, learningOppId: {2}, createdById: {3}", parentId, parent.EntityType, assessmentId, userId ) ;
						EmailManager.NotifyAdmin( thisClassName + ".ConditionAssessment_Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = thisClassName + string.Format( ".ConditionAssessment_Add() DbEntityValidationException, Parent Profile: {0}, Type: {1}, learningOppId: {2}, createdById: {3}", parentId, parent.EntityType, assessmentId, userId );
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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".EntityAssessment_Add(), Parent Profile: {0}, Type: {1}, learningOppId: {2}, createdById: {3}", parentId, parent.EntityType, assessmentId, userId ) );
				}


			}
			return id;
		}

		public bool EntityAssessment_Delete( int parentId, int entityTypeId, int assessmentId, ref string statusMessage )
		{
			bool isValid = false;
			if ( parentId == 0 || entityTypeId == 0 )
			{
				statusMessage = "Error - missing an identifier for the connection profile to remove the Assessment.";
				return false;
			}
			if ( assessmentId == 0 )
			{
				statusMessage = "Error - missing an identifier for the Assessment to remove";
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
				DBentity efEntity = context.Entity_Assessment
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
					statusMessage = "Error - delete failed, as record was not found.";
				}
			}

			return isValid;
		}

		public bool EntityAssessment_Delete( int Id, ref string statusMessage )
		{
			bool isValid = false;
			if ( Id == 0 )
			{
				statusMessage = "Error - missing an identifier for the Learning Opportunity to remove";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				DBentity efEntity = context.Entity_Assessment
							.SingleOrDefault( s => s.Id == Id );

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
					statusMessage = "Error - delete failed, as record was not found.";
				}
			}

			return isValid;
		}

		#endregion
	}
}
