using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Common;
using EM = Data;
using Utilities;
using DBEntity = Data.Entity_Approval;
using ThisEntity = Models.Common.Entity_Approval;

namespace Factories
{

	public class Entity_ApprovalManager : BaseFactory
	{
		static string thisClassName = "Entity_ApprovalManager";
	
		/// <summary>
		/// Get/check for an approval record for provided entity
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public static ThisEntity Get( int entityId )
		{
			ThisEntity entity = new ThisEntity();
			entity.IsActive =  false ;
			if ( entityId < 1  )
			{
				return entity;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					EM.Entity_Approval from = context.Entity_Approval
							.FirstOrDefault( s => s.EntityId == entityId && s.IsActive == true );

					if ( from != null && from.Id > 0 )
					{
                        MapFromDB( from, entity );
                    }
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Get" );
			}
			return entity;
		}//

		public static ThisEntity GetByParent( Guid parentRowId)
		{
			ThisEntity entity = new ThisEntity();
			entity.IsActive = false;
			if ( !IsGuidValid( parentRowId) )
			{
				return entity;
			}
			try
			{
				Entity parent = EntityManager.GetEntity( parentRowId, false );
				using ( var context = new Data.CTIEntities() )
				{
					EM.Entity_Approval from = context.Entity_Approval
							.FirstOrDefault( s => s.EntityId == parent.Id && s.IsActive == true );

					if ( from != null && from.Id > 0 )
					{
                        MapFromDB( from, entity);
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".GetByParent" );
			}
			return entity;
		}//

        public static void MapFromDB( DBEntity from, ThisEntity to )
        {
            to.Id = from.Id;
            to.IsActive = from.IsActive;
            to.EntityId = from.EntityId;


            if (IsValidDate( from.Created ))
                to.Created = ( DateTime ) from.Created;
            to.CreatedById = from.CreatedById;
            //if (from.Account != null)
            //{
            //    AppUser user = new AppUser();
            //    AccountManager.MapFromDB( from.Account, user );
            //}
            to.CreatedBy = SetLastUpdatedBy( to.CreatedById, from.Account );

        }
        #region Entity Approval Persistance ===================

        /// <summary>
        /// Add an Entity assessment
        /// </summary>
        /// <param name="parentUid"></param>
        /// <param name="userId"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public int Add( Guid parentUid,
					int userId,
                    string entityType,

                    ref List<string> messages )
		{
			int id = 0;
			int count = messages.Count();
			//TBD - might have entityId?
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( string.Format( "Entity Approval.Add(). Error - the parent entity (GUID: {0}) was not found for entity type: {1}", parentUid, entityType));
				return 0;
			}
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity efEntity = new DBEntity();
				try
				{
					//check if one exists, and replace if found
					//can only have one active approval
					efEntity = context.Entity_Approval
						.FirstOrDefault( s => s.EntityId == parent.Id && s.IsActive == true );
					if ( efEntity != null && efEntity.Id > 0 )
					{
                        //adding a re-approve, so just set userId
                        efEntity.CreatedById = userId;
						efEntity.Created = System.DateTime.Now;
						count = context.SaveChanges();

						return efEntity.Id;
					}
				
					efEntity = new DBEntity();
					efEntity.EntityId = parent.Id;
					efEntity.IsActive = true;
					efEntity.CreatedById = userId;
					efEntity.Created = System.DateTime.Now;

					context.Entity_Approval.Add( efEntity );

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
						messages.Add( "Error - the approval was not successful." );
						string message = thisClassName + string.Format( " Approval Failed", "Attempted to add a Entity_Approval for a profile. The process appeared to not work, but there was no exception, so we have no message, or no clue. Parent Profile: {0}, ParentType: {1}, createdById: {2}", parentUid, parent.EntityType, userId );
						EmailManager.NotifyAdmin( thisClassName + ".Approval Failed", message );
					}
				}
				//catch ( System.Data.Entity.Validation.DBEntityValidationException dbex )
				//{
				//	string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "Entity_Approval" );
				//	messages.Add( "Error - the Approval was not successful. " + message );
				//	LoggingHelper.LogError( dbex, thisClassName + string.Format( ".Save(), Parent: {0} ({1})", parent.EntityBaseName, parent.EntityBaseId ) );

				//}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex );
					messages.Add( "Error - the Approval was not successful. " + message );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Parent: {0} ({1})", parent.EntityBaseName, parent.EntityBaseId ) );
				}


			}
			return id;
		}


		/// <summary>
		/// For simplicity, we can start with a delete, but may prefer to retain for a history
		/// So we will assume deleting the active approval record
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( Guid parentUid, ref string statusMessage )
		{
			bool isValid = false;

			//need to get Entity.Id 
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				statusMessage = "Error - the parent entity was not found for the Entity.Approval.";
				return false;
			}

			using ( var context = new Data.CTIEntities() )
			{
				DBEntity efEntity = context.Entity_Approval
								.SingleOrDefault( s => s.EntityId == parent.Id && s.IsActive == true);

				if ( efEntity != null && efEntity.Id > 0 )
				{
					context.Entity_Approval.Remove( efEntity );
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

		#endregion
	}
}
