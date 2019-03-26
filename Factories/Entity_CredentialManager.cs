using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBEntity = Data.Entity_Credential;
using ThisEntity = Models.ProfileModels.Entity_Credential;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class Entity_CredentialManager : BaseFactory
	{
		static string thisClassName = "Entity_CredentialManager";
		/// <summary>
		/// if true, return an error message if the credential is already associated with the parent
		/// </summary>
		private bool ReturningErrorOnDuplicate { get; set; }
		public Entity_CredentialManager()
		{
			ReturningErrorOnDuplicate = false;
		}
		public Entity_CredentialManager( bool returnErrorOnDuplicate )
		{
			ReturningErrorOnDuplicate = returnErrorOnDuplicate;
		}

		#region Entity Persistance ===================
		/// <summary>
		/// Persist Entity Credential
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="parentUid"></param>
		/// <param name="userId"></param>
		/// <param name="newId">Return record id of the new record</param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public int Add( int credentialId, Guid parentUid, int userId, 
			bool allowMultiples, 
			ref int newId, 
			ref List<string> messages )
		{
			int count = 0;
			newId = 0;
			int intialCount = messages.Count;

			if ( !IsValidGuid( parentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}

			if ( credentialId < 1 )
			{
				messages.Add( "Error: a valid credential was not provided." );
			}
			if ( messages.Count > intialCount )
				return 0;

			DBEntity efEntity = new DBEntity();
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return 0;
			}

			using ( var context = new Data.CTIEntities() )
			{
				//first check for duplicates
				efEntity = context.Entity_Credential
						.SingleOrDefault( s => s.EntityId == parent.Id && s.CredentialId == credentialId );
				if ( efEntity != null && efEntity.Id > 0 )
				{
					if ( ReturningErrorOnDuplicate )
					{
						messages.Add( "Error - the credential is already part of this profile." );
					}
					return efEntity.Id;
				}

				if ( allowMultiples == false )
				{
					//check if one exists, and replace if found
					efEntity = context.Entity_Credential
						.FirstOrDefault( s => s.EntityId == parent.Id );
					if ( efEntity != null && efEntity.Id > 0 )
					{
						efEntity.CredentialId = credentialId;

						count = context.SaveChanges();

						return efEntity.Id;
					}
				}

				//if ( entity.Id == 0 )
				//{	}
					//add
					efEntity = new DBEntity();
					efEntity.CredentialId = credentialId;
					efEntity.EntityId = parent.Id;

					efEntity.Created = DateTime.Now;
					efEntity.CreatedById = userId;

					context.Entity_Credential.Add( efEntity );
					count = context.SaveChanges();
					//update profile record so doesn't get deleted
					newId = efEntity.Id;

					if ( count == 0 )
					{
						messages.Add( string.Format( " Unable to add the related credential: {0}  ", credentialId ) );
					}

			}

			return newId;
		}

		/// <summary>
		/// Delete a entity credentail via the entity id and credential id
		/// </summary>
		/// <param name="parentId"></param>
		/// <param name="credentialId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( int parentId, int credentialId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity p = context.Entity_Credential.FirstOrDefault( s => s.EntityId == parentId && s.CredentialId == credentialId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_Credential.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( thisClassName + ".Delete() Requested record was not found. parentId: {0}, credentialId: {1}", parentId, credentialId );
					isOK = false;
				}
			}
			return isOK;

		}
		public bool DeleteAll( Guid parentUid, ref List<string> messages )
		{
			bool isValid = true;
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( thisClassName + ".DeleteAll() Error - the parent entity was not found." );
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				context.Entity_Credential.RemoveRange( context.Entity_Credential.Where( s => s.EntityId == parent.Id ) );
				int count = context.SaveChanges();
				if ( count > 0 )
				{
					isValid = true;
					messages.Add( string.Format( "removed {0} related credentials.", count ) );
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
		public bool DeleteNotInList( Guid parentUid, List<Credential> list, ref List<string> messages )
		{
			bool isValid = true;
			if ( !list.Any() )
			{
				return true;
			}
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( thisClassName + string.Format(".DeleteNotInList() Error - the parent entity for [{0}] was not found.", parentUid) );
				return false;
			}

			using ( var context = new Data.CTIEntities() )
			{
				var existing = context.Entity_Credential.Where( s => s.EntityId == parent.Id ).ToList();
				var inputIds = list.Select( x => x.Id ).ToList();

				//delete records which are not selected 
				var notExisting = existing.Where( x => !inputIds.Contains( x.CredentialId ) ).ToList();
				foreach ( var item in notExisting )
				{
					context.Entity_Credential.Remove( item );
					context.SaveChanges();
				}

			}
			return isValid;

		}

		public bool ValidateEntity( ThisEntity profile, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;

			isEmpty = false;
			//check if empty
			if ( profile.CredentialId == 0 )
			{
				isEmpty = true;
				return isValid;
			}

	
			return isValid;
		}

		#endregion

		#region  retrieval ==================

		/// <summary>
		/// get all the base credentials for an EntityCredential
		/// Uses the parent Guid to retrieve the related Entity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		/// <returns></returns>
		public static List<Credential> GetAll( Guid parentUid, bool isForDetailPageCondition = false )
		{
			ThisEntity entity = new ThisEntity();
			List<Credential> list = new List<Credential>();
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					//commented out in order to get more data for detail page
					//context.Configuration.LazyLoadingEnabled = false;

					List<DBEntity> results = context.Entity_Credential
							.Include( "Credential")
							.AsNoTracking()
							.Where( s => s.EntityId == parent.Id)
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBEntity item in results )
						{
							entity = new ThisEntity();
							if ( item.Credential != null && item.Credential.StatusId <= CodesManager.ENTITY_STATUS_EXTERNAL_REFERENCE )
							{
								MapFromDB( item, entity, isForDetailPageCondition );

								list.Add( entity.Credential );
							}
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".GetAll" );
			}
			return list;
		}//

		public static ThisEntity Get( int profileId )
		{
			ThisEntity entity = new ThisEntity();
			if ( profileId == 0 )
			{
				return entity;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBEntity item = context.Entity_Credential
							.SingleOrDefault( s => s.Id == profileId );

					if ( item != null && item.Id > 0 )
					{
						MapFromDB( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Entity_Get" );
			}
			return entity;
		}//

		public static ThisEntity Get( int parentId, int credentialId )
		{
			ThisEntity entity = new ThisEntity();
			if ( parentId < 1 || credentialId < 1 )
			{
				return entity;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBEntity item = context.Entity_Credential
							.SingleOrDefault( s => s.CredentialId == credentialId && s.EntityId == parentId);

					if ( item != null && item.Id > 0 )
					{
						MapFromDB( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Get" );
			}
			return entity;
		}//

		public static void MapToDB( ThisEntity from, DBEntity to )
		{
			//want to ensure fields from create are not wiped
			if ( to.Id == 0 )
			{
				if ( IsValidDate( from.Created ) )
					to.Created = from.Created;
				to.CreatedById = from.CreatedById;
			}
			to.Id = from.Id;
			to.CredentialId = from.CredentialId;
			to.EntityId = from.ParentId;
			
		}
		public static void MapFromDB( DBEntity from, ThisEntity to, bool isForDetailPageCondition = false )
		{
			to.Id = from.Id;
			to.CredentialId = from.CredentialId;
			to.ParentId = from.EntityId;

			to.ProfileSummary = from.Credential.Name;
			//to.Credential = from.Credential;
			to.Credential = new Credential();

			CredentialMinimumMap( from.Credential, to.Credential );

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
		}
		public static Credential MapFromDB_FirstCredential( ICollection<EM.Entity_Credential> results )
		{
			ThisEntity entity = new ThisEntity();

			if ( results != null && results.Count > 0)
			{
				foreach ( EM.Entity_Credential item in results )
				{
					entity = new ThisEntity();
					if ( item.Credential != null && item.Credential.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED )
					{
						MapFromDB( item, entity, false );

						return entity.Credential;
						break;
					}
				}
			}
			

			return null;

		}
		public static ThisEntity MapFromDB( DBEntity from )
		{
			ThisEntity to = new ThisEntity();
			to.Id = from.Id;
			to.CredentialId = from.CredentialId;
			to.ParentId = from.EntityId;

			to.ProfileSummary = from.Credential.Name;
			//to.Credential = from.Credential;
			to.Credential = new Credential();

			CredentialMinimumMap( from.Credential, to.Credential );

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;

			return to;
		}
		public static void CredentialMinimumMap( EM.Credential from, Credential to )
		{
			CredentialRequest cr = new CredentialRequest();
			//probably too much
			cr.IsDetailRequest();

			to.Id = from.Id;
			to.RowId = from.RowId;
			to.Name = from.Name;
			to.Description = from.Description;

			to.SubjectWebpage = from.Url;
			to.StatusId = (int)(from.StatusId ?? 1);
			to.ctid = from.CTID;
			to.CredentialType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE );

			to.CredentialTypeDisplay = to.CredentialType.GetFirstItem().Name;
			to.CredentialTypeSchema = to.CredentialType.GetFirstItem().SchemaName;

			if ( from.ImageUrl != null && from.ImageUrl.Trim().Length > 0 )
				to.ImageUrl = from.ImageUrl;
			else
				to.ImageUrl = null;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;

			to.AudienceLevelType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_AUDIENCE_LEVEL );

			to.Occupation = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_SOC );
			to.AlternativeOccupations = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SOC );

			to.Industry = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );
			to.AlternativeIndustries = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );

			to.Subject = Entity_ReferenceManager.GetAllSubjects( to.RowId );

			to.Keyword = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD );

			//Added these because they were needed on the detail page - NA 6/1/2017
			to.OwningAgentUid = from.OwningAgentUid ?? Guid.Empty;
			to.OwningOrganization = OrganizationManager.GetForSummary( to.OwningAgentUid );

		}


		#endregion

	}
}
