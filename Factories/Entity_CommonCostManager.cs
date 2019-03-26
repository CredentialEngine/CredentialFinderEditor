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
using DBEntity = Data.Entity_CommonCost;
using ThisEntity = Models.Common.Entity_CommonCost;

using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;

namespace Factories
{
	public class Entity_CommonCostManager : BaseFactory
	{
		static string thisClassName = "Entity_CommonCostManager";

		#region === Persistance ===================

		/// <summary>
		/// Add an Entity_CommonCost
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
				messages.Add( string.Format( "A valid CostManifest identifier was not provided to the {0}.Add method.", thisClassName ) );
                return 0;
            }
			
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return 0;
			}

			//need to check the whole chain
			if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_COST_MANIFEST
				&& parent.EntityBaseId == profileId )
			{
				messages.Add( "Error - you cannot add a Cost manifest as a common Cost to itself." );
				return 0;
			}
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity efEntity = new DBEntity();
				try
				{
					//first check for duplicates
					efEntity = context.Entity_CommonCost
							.SingleOrDefault( s => s.EntityId == parent.Id
							&& s.CostManifestId == profileId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
                        if ( hideDuplicatesError )
                            return efEntity.Id;
                        else
                        {
                            messages.Add( string.Format( "Error - this CostManifest has already been added to this profile.", thisClassName ) );
                            return 0;
                        }
					}

					efEntity = new DBEntity();
					efEntity.EntityId = parent.Id;
					efEntity.CostManifestId = profileId;

					efEntity.CreatedById = userId;
					efEntity.Created = System.DateTime.Now;

					context.Entity_CommonCost.Add( efEntity );

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
						string message = thisClassName + string.Format( ".Add Failed", "Attempted to add a CostManifest for a profile. The process appeared to not work, but there was no exception, so we have no message, or no clue. Parent Profile: {0}, Type: {1}, learningOppId: {2}, createdById: {3}", parentUid, parent.EntityType, profileId, userId );
						EmailManager.NotifyAdmin( thisClassName + ".Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "Entity_CommonCost" );
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
		public bool IsParent( Entity parent, int CostManifestBeingCheckedId, ref string statusMessage )
		{
			bool isOK = true;
			if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_COST_MANIFEST
				&& parent.EntityBaseId == CostManifestBeingCheckedId )
			{
				statusMessage = "Error - you cannot add this Cost manifest as a common Cost as it is the same as the parent Cost manifest or a grand parent (or somewhere up the hierarchy).";
				return false;
			}
			//check for a parent Cost manifest
			//get all commonCosts, with parent CM



			return isOK;
		}
        public bool Delete( Guid parentUid, int profileId, ref List<string> messages )
        {
            bool isValid = false;
            if ( profileId == 0 )
            {
                messages.Add( "Error - missing an identifier for the Assessment to remove" );
                return false;
            }
            //need to get Entity.Id 
            Entity parent = EntityManager.GetEntity( parentUid );
            if ( parent == null || parent.Id == 0 )
            {
                messages.Add( "Error - the parent entity was not found." );
                return false;
            }

            using ( var context = new Data.CTIEntities() )
            {
                DBEntity efEntity = context.Entity_CommonCost
                                .SingleOrDefault( s => s.EntityId == parent.Id && s.CostManifestId == profileId );

                if ( efEntity != null && efEntity.Id > 0 )
                {
                    context.Entity_CommonCost.Remove( efEntity );
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        isValid = true;
                    }
                }
                else
                {
                    messages.Add( "Warning - the common cost record was not found - probably because the target had been previously deleted" );
                    isValid = true;
                }
            }
            return isValid;
        }

		#endregion

		/// <summary>
		/// Get all CostManifests for the provided entity
		/// The returned entities are just the basic, unless for the detail view
		/// </summary>
		/// <param name="parentUid"></param>
		/// <returns></returns>
		public static List<CostManifest> GetAll( Guid parentUid, bool forEditView )
		{
			List<CostManifest> list = new List<CostManifest>();
			CostManifest entity = new CostManifest();

			//if ( !forEditView )
			//{
			//	includingProperties = true;
			//	includingRoles = true;
			//}

			Entity parent = EntityManager.GetEntity( parentUid );
			LoggingHelper.DoTrace( 7, string.Format( "Entity_CommonCostManager_GetAll: parentUid:{0} entityId:{1}, e.EntityTypeId:{2}", parentUid, parent.Id, parent.EntityTypeId ) );

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBEntity> results = context.Entity_CommonCost
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.CostManifestId )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBEntity item in results )
						{
							entity = new CostManifest();
							//
							//Need all the data for detail page - NA 6/2/2017

							if ( forEditView )
								entity = CostManifestManager.GetBasic( item.CostManifestId );
							else
								entity = CostManifestManager.Get( item.CostManifestId, true );

							list.Add( entity );
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

		public static List<CostManifest> GetAllManifestInCommonCostsFor( int CostManifestId, int CostManifestBeingCheckedId )
		{
			List<CostManifest> list = new List<CostManifest>();


			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBEntity> results = context.Entity_CommonCost
							.Where( s => s.CostManifestId == CostManifestId )
							.OrderBy( s => s.EntityId )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBEntity item in results )
						{
							if ( item.Entity.EntityBaseId == CostManifestBeingCheckedId )
							{

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
		/// <summary>
		/// Get - NOTE currently only for verifying before a delete
		/// </summary>
		/// <param name="parentId"></param>
		/// <param name="profileId"></param>
		/// <returns></returns>
		public static ThisEntity Get( int parentId, int profileId )
		{
			ThisEntity entity = new ThisEntity();
			CostManifest cm = new CostManifest();
			if ( parentId < 1 || profileId < 1 )
			{
				return entity;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					EM.Entity_CommonCost from = context.Entity_CommonCost
							.SingleOrDefault( s => s.CostManifestId == profileId && s.EntityId == parentId );

					if ( from != null && from.Id > 0 )
					{
						entity.Id = from.Id;
						entity.CostManifestId = from.CostManifestId;
						entity.EntityId = from.EntityId;
						//entity.CostManifest = CostManifestManager.GetBasic( from.CostManifestId );
						entity.ProfileSummary = from.CostManifest.Name;

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
