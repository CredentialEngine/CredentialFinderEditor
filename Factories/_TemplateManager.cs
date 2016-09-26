using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.ProfileModels;
using Models.Common;
using EM = Data;
using Utilities;

using DBentity = Data.Credential_ConnectionProfile;
using Entity = Models.ProfileModels.ConditionProfile;


namespace Factories
{
	/// <summary>
	/// Instructions:
	/// - for a top level profile table -with base add, etc
	/// - create a new manager class, and then copy and paste this class into new class
	/// - change all Credential_ConnectionProfile to the entity frameworks entity name
	/// - change all ConditionProfile to a custom profile class
	/// - change  to the proper class name
	/// - update the to and from map methods
	/// </summary>
	public class _TemplateManager : BaseFactory
	{
		static string thisClassName = "ConditionProfileManager";
		

		#region persistance ==================
		
		/// <summary>
		/// add a ConditionProfile
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Entity_Add( Credential credential, Entity entity, ref List<String> messages )
		{
			DBentity efEntity = new DBentity();
			entity.ParentId = credential.Id;
			using ( var context = new Data.CTIEntities() )
			{
				try
				{

					Entity_FromMap( entity, efEntity );

					efEntity.CredentialId = credential.Id;
					if ( efEntity.RowId == null || efEntity.RowId.ToString() == DEFAULT_GUID )
						efEntity.RowId = Guid.NewGuid();
					efEntity.CreatedById = credential.LastUpdatedById;
					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdatedById = credential.LastUpdatedById;
					efEntity.LastUpdated = System.DateTime.Now;

					context.Credential_ConnectionProfile.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						entity.Id = efEntity.Id;

						//opMgr.ConditionProfile_UpdateParts( entity, true, ref statusMessage );

						return efEntity.Id;
					}
					else
					{
						//?no info on error
						messages.Add( "Error - the profile was not saved. " );
						string message = string.Format( "ConditionProfileManager. ConditionProfile_Add Failed", "Attempted to add a ConditionProfile. The process appeared to not work, but was not an exception, so we have no message, or no clue.ConditionProfile. CredentialId: {0}, createdById: {1}", entity.ParentId, entity.CreatedById );
						EmailManager.NotifyAdmin( thisClassName + ". ConditionProfile_Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					//LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
					string message = thisClassName + string.Format( ".ConditionProfile_Add() DbEntityValidationException, CredentialId: {0}", credential.Id );
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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".ConditionProfile_Add(), CredentialId: {0}", entity.ParentId ) );
				}
			}

			return efEntity.Id;
		}
		/// <summary>
		/// Update a ConditionProfile
		/// - base only, caller will handle parts?
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Entity_Update( Entity entity, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentity efEntity = context.Credential_ConnectionProfile
								.SingleOrDefault( s => s.Id == entity.Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						//for updates, chances are some fields will not be in interface, don't map these (ie created stuff)
						Entity_FromMap( entity, efEntity );
						if ( HasStateChanged( context ) )
						{
							efEntity.LastUpdated = System.DateTime.Now;
							efEntity.LastUpdatedById = entity.LastUpdatedById;
							count = context.SaveChanges();
							//can be zero if no data changed
							if ( count >= 0 )
							{
								isValid = true;
							}
							else
							{
								//?no info on error
								statusMessage = "Error - the update was not successful. ";
								string message = string.Format( thisClassName + ".ConditionProfile_Update Failed", "Attempted to update a ConditionProfile. The process appeared to not work, but was not an exception, so we have no message, or no clue. CredentialId: {0}, Id: {1}, updatedById: {2}", entity.ParentId, entity.Id, entity.LastUpdatedById );
								EmailManager.NotifyAdmin( thisClassName + ". ConditionProfile_Update Failed", message );
							}
						}
						//continue with parts regardless
						//opMgr.ConditionProfile_UpdateParts( entity, false, ref statusMessage );
					}
					else
					{
						statusMessage = "Error - update failed, as record was not found.";
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".ConditionProfile_Update. id: {0}", entity.Id ) );
			}


			return isValid;
		}

		public bool ConditionProfile_Delete( int Id, ref string statusMessage )
		{
			bool isValid = false;
			if ( Id == 0 )
			{
				statusMessage = "Error - missing an identifier for the ConditionProfile";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				DBentity efEntity = context.Credential_ConnectionProfile
							.SingleOrDefault( s => s.Id == Id );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					context.Credential_ConnectionProfile.Remove( efEntity );
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

		private bool IsValid( ConditionProfile item, ref List<string> messages )
		{
			bool isValid = true;
			
			if ( string.IsNullOrWhiteSpace( item.ProfileName ) )
			{
				isValid = false;
				messages.Add( "Error: missing profile name" );
			}
			return isValid;
		}
		#endregion

		#region == Retrieval =======================

		public static Entity ConditionProfile_Get( int id, bool includeProperties = false )
		{
			Entity entity = new Entity();
			using ( var context = new Data.CTIEntities() )
			{

				DBentity item = context.Credential_ConnectionProfile
						.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					Entity_ToMap( item, entity, true );
					if ( includeProperties )
					{
						//TBD
					}
				}
			}

			return entity;
		}

		public static void Entity_FromMap( Entity fromEntity, DBentity to )
		{

			//want to ensure fields from create are not wiped
			if ( to.Id < 1 )
			{
				if ( IsValidDate( fromEntity.Created ) )
					to.Created = fromEntity.Created;
				to.CreatedById = fromEntity.CreatedById;
			}

			to.Id = fromEntity.Id;
			//to.Name = fromEntity.Name;
			to.Description = fromEntity.Description;
			to.CredentialId = fromEntity.ParentId;

			
			

			if ( IsValidDate( fromEntity.LastUpdated ) )
				to.LastUpdated = fromEntity.LastUpdated;
			to.LastUpdatedById = fromEntity.LastUpdatedById;
		}
		public static void Entity_ToMap( DBentity fromEntity, Entity to, bool includingProperties = false )
		{
			to.Id = fromEntity.Id;
			to.RowId = fromEntity.RowId;
			to.ParentId = fromEntity.CredentialId;
			to.ProfileName = fromEntity.Name;
			to.Description = fromEntity.Description;
			
			//....

			if ( IsValidDate( fromEntity.Created ) )
				to.Created = ( DateTime ) fromEntity.Created;
			to.CreatedById = fromEntity.CreatedById == null ? 0 : ( int ) fromEntity.CreatedById;
			if ( IsValidDate( fromEntity.LastUpdated ) )
				to.LastUpdated = ( DateTime ) fromEntity.LastUpdated;
			to.LastUpdatedById = fromEntity.LastUpdatedById == null ? 0 : ( int ) fromEntity.LastUpdatedById;

			//properties
			//if ( includingProperties )
			//	ConditionProfilePropertyManager.ConditionProfilePropertyFill_ToMap( fromEntity, to );
		}

		#endregion


	}
}
