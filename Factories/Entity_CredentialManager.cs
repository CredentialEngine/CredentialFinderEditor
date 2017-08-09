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
using ThisEntity = Models.ProfileModels.Entity_Credential;
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
		public bool Add( int credentialId, Guid parentUid, int userId, 
			bool allowMultiples, 
			ref int newId, 
			ref List<string> messages )
		{
			bool isValid = true;
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
				return false;

			DBentity efEntity = new DBentity();
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}

			using ( var context = new Data.CTIEntities() )
			{
				//first check for duplicates
				efEntity = context.Entity_Credential
						.SingleOrDefault( s => s.EntityId == parent.Id && s.CredentialId == credentialId );
				if ( efEntity != null && efEntity.Id > 0 )
				{
					messages.Add( "Error - the credential is already part of this profile." );
					return false;
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

						return true;
					}
				}

				//if ( entity.Id == 0 )
				//{	}
					//add
					efEntity = new DBentity();
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
		public bool Delete( int parentId, int credentialId, ref string statusMessage )
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
		public bool Delete( int recordId, ref string statusMessage )
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
					statusMessage = "Warning - the record was not found - probably because the target had been previously deleted";
					isOK = true;
				}
			}
			return isOK;

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

					List<DBentity> results = context.Entity_Credential
							.Include( "Credential")
							.AsNoTracking()
							.Where( s => s.EntityId == parent.Id)
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity item in results )
						{
							entity = new ThisEntity();
							if ( item.Credential != null && item.Credential.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED )
							{
								ToMap( item, entity, isForDetailPageCondition );

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
					DBentity item = context.Entity_Credential
							.SingleOrDefault( s => s.Id == profileId );

					if ( item != null && item.Id > 0 )
					{
						ToMap( item, entity );
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
					DBentity item = context.Entity_Credential
							.SingleOrDefault( s => s.CredentialId == credentialId && s.EntityId == parentId);

					if ( item != null && item.Id > 0 )
					{
						ToMap( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Get" );
			}
			return entity;
		}//

		public static void FromMap( ThisEntity from, DBentity to )
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
		public static void ToMap( DBentity from, ThisEntity to, bool isForDetailPageCondition = false )
		{
			to.Id = from.Id;
			to.CredentialId = from.CredentialId;
			to.ParentId = from.EntityId;

			to.ProfileSummary = from.Credential.Name;
			//to.Credential = from.Credential;
			to.Credential = new Credential();

			//actually decided to use the same method, just added the additional props
			if ( isForDetailPageCondition )
				CredentialMinimumMap( from.Credential, to.Credential );
			else
				CredentialMinimumMap( from.Credential, to.Credential );

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
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
			to.OtherOccupations = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SOC );

			to.Industry = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );
			to.OtherIndustries = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );

			to.Subject = Entity_ReferenceManager.GetAllSubjects( to.RowId );

			to.Keyword = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD );

			//Added these because they were needed on the detail page - NA 6/1/2017
			to.OwningAgentUid = from.OwningAgentUid ?? Guid.Empty;
			to.OwningOrganization = OrganizationManager.GetForSummary( to.OwningAgentUid );

		}


		#endregion

	}
}
