using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBentity = Data.Entity_Credential;
using Entity = Models.ProfileModels.Entity_Credential;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class Entity_CredentialManager : BaseFactory
	{
		static string thisClassName = "Entity_CredentialManager";
		
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
		public bool EntityCredential_Add( int credentialId, Guid parentUid, int userId, ref int newId, ref List<string> messages )
		{
			bool isValid = true;
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
				return false;

			DBentity efEntity = new DBentity();
			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			//check for duplicate
			Entity entity = Entity_Get( parent.Id, credentialId );
			if ( entity != null && entity.Id > 0 )
			{
				messages.Add( "Error - the credential is already part of this profile." );
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				//bool isEmpty = false;

				//if ( ValidateEntity( entity, ref isEmpty, ref  messages ) == false )
				//{
				//	messages.Add( "Credential Reference profile was invalid. ");
				//	return false;
				//}
			
				//if ( isEmpty ) //skip
				//{
				//	messages.Add( "Credential Reference profile is empty. " );
				//	return false;
				//}

				//if ( entity.Id == 0 )
				//{	}
					//add
					efEntity = new DBentity();
					efEntity.CredentialId = credentialId;
					efEntity.EntityId = parent.Id;

					efEntity.Created = DateTime.Now;
					efEntity.CreatedById = userId;

					context.Entity_Credential.Add( efEntity );
					int count = context.SaveChanges();
					//update profile record so doesn't get deleted
					newId = efEntity.Id;

					if ( count == 0 )
					{
						messages.Add( string.Format( " Unable to add the related credential: {0}  ", credentialId ) );
						isValid = false;
					}

			}

			return isValid;
		}

		/// <summary>
		/// Delete a entity credentail via the entity id and credential id
		/// </summary>
		/// <param name="parentId"></param>
		/// <param name="credentialId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Entity_Delete( int parentId, int credentialId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBentity p = context.Entity_Credential.FirstOrDefault( s => s.EntityId == parentId && s.CredentialId == credentialId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_Credential.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "Requested record was not found: {0}", credentialId );
					isOK = false;
				}
			}
			return isOK;

		}
		public bool Entity_Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBentity p = context.Entity_Credential.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_Credential.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "Requested record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;

		}

		public bool ValidateEntity( Entity profile, ref bool isEmpty, ref List<string> messages )
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
		/// Get all profiles for the parent
		/// Uses the parent Guid to retrieve the related Entity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		public static List<Entity> GetAll( int parentId )
		{
			Entity entity = new Entity();
			List<Entity> list = new List<Entity>();
			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentId );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBentity> results = context.Entity_Credential
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity item in results )
						{
							entity = new Entity();
							Entity_ToMap( item, entity );

							list.Add( entity );
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

		/// <summary>
		/// get all the base credentials for an EntityCredential
		/// </summary>
		/// <param name="parentUid"></param>
		/// <returns></returns>
		public static List<Credential> GetAll( Guid parentUid )
		{
			Entity entity = new Entity();
			List<Credential> list = new List<Credential>();
			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBentity> results = context.Entity_Credential
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity item in results )
						{
							entity = new Entity();
							Entity_ToMap( item, entity );

							list.Add( entity.Credential );
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

		public static Entity Entity_Get( int profileId )
		{
			Entity entity = new Entity();
			if ( profileId == 0 )
			{
				return entity;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentity item = context.Entity_Credential
							.SingleOrDefault( s => s.Id == profileId );

					if ( item != null && item.Id > 0 )
					{
						Entity_ToMap( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Entity_Get" );
			}
			return entity;
		}//

		public static Entity Entity_Get( int parentId, int credentialId )
		{
			Entity entity = new Entity();
			if ( parentId < 1 || credentialId < 1 )
			{
				return entity;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentity item = context.Entity_Credential
							.SingleOrDefault( s => s.CredentialId == credentialId && s.EntityId == parentId);

					if ( item != null && item.Id > 0 )
					{
						Entity_ToMap( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Entity_Get" );
			}
			return entity;
		}//

		public static void Entity_FromMap( Entity from, DBentity to )
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
		public static void Entity_ToMap( DBentity from, Entity to )
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
		public static void CredentialMinimumMap( EM.Credential from, Credential to )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.Name = from.Name;
			to.Description = from.Description;

			to.Url = from.Url;
			to.StatusId = (int)(from.StatusId ?? 1);

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;

		}
		#endregion

	}
}
