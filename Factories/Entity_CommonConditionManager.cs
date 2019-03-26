using System;
using System.Collections.Generic;
using System.Linq;

using Models.Common;
using EM = Data;
using Utilities;
using DBEntity = Data.Entity_CommonCondition;
using ThisEntity = Models.Common.Entity_CommonCondition;

namespace Factories
{
	public class Entity_CommonConditionManager : BaseFactory
	{
		static string thisClassName = "Entity_CommonConditionManager";

		#region === Persistance ===================

		/// <summary>
		/// Add an Entity_CommonCondition
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="profileId"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public int Add( Guid parentUid,
					int profileId,
					int userId,
					ref List<string> messages,
                    bool hideDuplicatesError = false )
		{
			int id = 0;
			int count = messages.Count();
			if ( profileId < 1 )
			{
				messages.Add( string.Format( "A valid ConditionManifest identifier was not provided to the {0}.Add method.", thisClassName ) );
                return 0;
            }

			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return 0;
			}

			//need to check the whole chain
			if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_CONDITION_MANIFEST
				&& parent.EntityBaseId == profileId )
			{
				messages.Add( "Error - you cannot add a condition manifest as a common condition to itself." );
				return 0;
			}
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity efEntity = new DBEntity();
				try
				{
					//first check for duplicates
					efEntity = context.Entity_CommonCondition
							.SingleOrDefault( s => s.EntityId == parent.Id
							&& s.ConditionManifestId == profileId );
                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        if ( hideDuplicatesError )
                            return efEntity.Id;
                        else
                        {
                            messages.Add( string.Format( "Error - this ConditionManifest has already been added to this profile.", thisClassName ) );
                            return 0;
                        }
                    }

                    efEntity = new DBEntity();
					efEntity.EntityId = parent.Id;
					efEntity.ConditionManifestId = profileId;

					efEntity.CreatedById = userId;
					efEntity.Created = System.DateTime.Now;

					context.Entity_CommonCondition.Add( efEntity );

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
						string message = thisClassName + string.Format( ".Add Failed", "Attempted to add a ConditionManifest for a profile. The process appeared to not work, but there was no exception, so we have no message, or no clue. Parent Profile: {0}, Type: {1}, learningOppId: {2}, createdById: {3}", parentUid, parent.EntityType, profileId, userId );
						EmailManager.NotifyAdmin( thisClassName + ".Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "Entity_CommonCondition" );
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( dbex, thisClassName + string.Format( ".Save(), Parent: {0} ({1}), UserId: {2}", parent.EntityBaseName, parent.EntityBaseId, userId ) );

				}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex );
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save(), Parent: {0} ({1}), UserId: {2}", parent.EntityBaseName, parent.EntityBaseId, userId ) );

				}
			}
			return id;
		}
		public bool IsParent( Entity parent, int conditionManifestBeingCheckedId, ref string statusMessage )
		{
			bool isOK = true;
			if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_CONDITION_MANIFEST
				&& parent.EntityBaseId == conditionManifestBeingCheckedId )
			{
				statusMessage = "Error - you cannot add this condition manifest as a common condition as it is the same as the parent condition manifest or a grand parent (or somewhere up the hierarchy)." ;
				return false;
			}
			//check for a parent condition manifest
			//get all commonconditions, with parent CM



			return isOK;
		}
		public bool Delete( Guid parentUid, int profileId, ref List<string> messages )
		{
			bool isValid = false;
			if ( profileId == 0 )
			{
                messages.Add( "Error - missing an identifier for the Assessment to remove");
				return false;
			}
			//need to get Entity.Id 
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
                messages.Add( "Error - the parent entity was not found.");
				return false;
			}

			using ( var context = new Data.CTIEntities() )
			{
				DBEntity efEntity = context.Entity_CommonCondition
								.SingleOrDefault( s => s.EntityId == parent.Id && s.ConditionManifestId == profileId );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					context.Entity_CommonCondition.Remove( efEntity );
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					}
				}
				else
				{
                    messages.Add( "Warning - the record was not found - probably because the target had been previously deleted");
					isValid = true;
				}
			}

			return isValid;
		}

		#endregion

		/// <summary>
		/// Get all ConditionManifests for the provided entity
		/// The returned entities are just the basic, unless for the detail view
		/// </summary>
		/// <param name="parentUid"></param>
		/// <returns></returns>
		public static List<ConditionManifest> GetAll( Guid parentUid, bool forEditView )
		{
			List<ConditionManifest> list = new List<ConditionManifest>();
			ConditionManifest entity = new ConditionManifest();
		
			//if ( !forEditView )
			//{
			//	includingProperties = true;
			//	includingRoles = true;
			//}

			Entity parent = EntityManager.GetEntity( parentUid );
			LoggingHelper.DoTrace( 7, string.Format( "Entity_CommonConditionManager_GetAll: parentUid:{0} entityId:{1}, e.EntityTypeId:{2}", parentUid, parent.Id, parent.EntityTypeId ) );

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBEntity> results = context.Entity_CommonCondition
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.ConditionManifestId )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBEntity item in results )
						{
							entity = new ConditionManifest();
							//
							//Need all the data for detail page - NA 6/2/2017
							if ( item.ConditionManifest != null )
							{
								if ( forEditView )
									entity = ConditionManifestManager.GetBasic( item.ConditionManifestId );
								else
									entity = ConditionManifestManager.Get( item.ConditionManifestId, forEditView );

								list.Add( entity );
							}
						}
					}
					return list;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".GetAll" );
			}
			return list;
		}

		//public static List<ConditionManifest> GetAllManifestInCommonConditionsFor( int conditionManifestId, int conditionManifestBeingCheckedId )
		//{
		//	List<ConditionManifest> list = new List<ConditionManifest>();
			

		//	try
		//	{
		//		using ( var context = new Data.CTIEntities() )
		//		{
		//			List<DBEntity> results = context.Entity_CommonCondition
		//					.Where( s => s.ConditionManifestId == conditionManifestId )
		//					.OrderBy( s => s.EntityId )
		//					.ToList();

		//			if ( results != null && results.Count > 0 )
		//			{
		//				foreach ( DBEntity item in results )
		//				{
		//					if (item.Entity.EntityBaseId == conditionManifestBeingCheckedId )
		//					{

		//					}
		//				}
		//			}
		//			return list;
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".GetAll" );
		//	}
		//	return list;
		//}
		/// <summary>
		/// Get - NOTE currently only for verifying before a delete
		/// </summary>
		/// <param name="parentId"></param>
		/// <param name="profileId"></param>
		/// <returns></returns>
		public static ThisEntity Get( int parentId, int profileId )
		{
			ThisEntity entity = new ThisEntity();
			ConditionManifest cm = new ConditionManifest();
			if ( parentId < 1 || profileId < 1 )
			{
				return entity;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					EM.Entity_CommonCondition from = context.Entity_CommonCondition
							.SingleOrDefault( s => s.ConditionManifestId == profileId && s.EntityId == parentId );

					if ( from != null && from.Id > 0 )
					{
						entity.Id = from.Id;
						entity.ConditionManifestId = from.ConditionManifestId;
						entity.EntityId = from.EntityId;

						entity.ConditionManifest = ConditionManifestManager.GetBasic( from.ConditionManifestId );
						entity.ProfileSummary = entity.ConditionManifest.ProfileName;

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

		
	}
}
