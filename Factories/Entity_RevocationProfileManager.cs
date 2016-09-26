using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBentity = Data.Entity_RevocationProfile;
using ThisEntity = Models.ProfileModels.RevocationProfile;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class Entity_RevocationProfileManager : BaseFactory
	{
		static string thisClassName = "Entity_RevocationProfileManager";
		#region Entity Persistance ===================
	
		/// <summary>
		/// Persist Revocation Profiles
		/// </summary>
		/// <param name="profiles"></param>
		/// <param name="parentUid"></param>
		/// <param name="parentTypeId"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		//public bool Update( List<ThisEntity> profiles, int credentialId,  int userId, ref List<string> messages )
		//{
		//	bool isValid = true;
		//	int intialCount = messages.Count;

		//	if ( credentialId < 1 )
		//	{
		//		messages.Add( "Error: the credential identifier was not provided." );
		//	}
			
		//	if ( messages.Count > intialCount )
		//		return false;
		//	Credential credential = CredentialManager.Credential_GetBasic( credentialId, false, false );
		//	int count = 0;
		//	bool hasData = false;
		//	if ( profiles == null )
		//		profiles = new List<ThisEntity>();

		//	DBentity efEntity = new DBentity();

		//	Views.Entity_Summary parent = EntityManager.GetDBEntity( credential.RowId );
		//	if ( parent == null || parent.Id == 0 )
		//	{
		//		messages.Add( "Error - the parent entity was not found." );
		//		return false;
		//	}
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		//check add/updates first
		//		if ( profiles.Count() > 0 )
		//		{
		//			hasData = true;
		//			bool isEmpty = false;
		//			int profNbr = 0;

		//			foreach ( ThisEntity entity in profiles )
		//			{
		//				profNbr++;
		//				if ( ValidateProfile( entity, ref isEmpty, ref  messages ) == false )
		//				{
		//					messages.Add( string.Format("Revocation Profile ({0}) was invalid. ", profNbr) + SetEntitySummary( entity ) );
		//					isValid = false;
		//					continue;
		//				}
		//				if ( isEmpty ) //skip
		//					continue;

		//				if ( entity.Id == 0 )
		//				{
		//					//add
		//					efEntity = new DBentity();
		//					FromMap( entity, efEntity );
		//					efEntity.EntityId = parent.Id;

		//					efEntity.Created = efEntity.LastUpdated = DateTime.Now;
		//					efEntity.CreatedById = efEntity.LastUpdatedById = userId;
		//					efEntity.RowId = Guid.NewGuid();

		//					context.Entity_RevocationProfile.Add( efEntity );
		//					count = context.SaveChanges();
		//					//update profile record so doesn't get deleted
		//					entity.Id = efEntity.Id;
		//					entity.ParentId = parent.Id;
		//					entity.RowId = efEntity.RowId;
		//					if ( count == 0 )
		//					{
		//						ConsoleMessageHelper.SetConsoleErrorMessage( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.ProfileName ) ? "no description" : entity.ProfileName ) );
		//					}
		//					else
		//					{
		//						UpdateParts( entity, userId, ref messages );
		//					}
		//				}
		//				else
		//				{
		//					entity.ParentId = parent.Id;

		//					efEntity = context.Entity_RevocationProfile.SingleOrDefault( s => s.Id == entity.Id );
		//					if ( efEntity != null && efEntity.Id > 0 )
		//					{
		//						entity.RowId = efEntity.RowId;
		//						//update
		//						FromMap( entity, efEntity );
		//						//has changed?
		//						if ( HasStateChanged( context ) )
		//						{
		//							efEntity.LastUpdated = System.DateTime.Now;
		//							efEntity.LastUpdatedById = userId;

		//							count = context.SaveChanges();
		//						}
		//						//always check parts
		//						UpdateParts( entity, userId, ref messages );
		//					}

		//				}

		//			} //foreach

		//		}

		//		//check for deletes ====================================
		//		//need to ensure ones just added don't get deleted

		//		//get existing 
		//		List<DBentity> results = context.Entity_RevocationProfile
		//				.Where( s => s.EntityId == parent.Id )
		//				.OrderBy( s => s.Id )
		//				.ToList();

		//		//if profiles is null, need to delete all!!
		//		if ( results.Count() > 0 && profiles.Count() == 0 )
		//		{
		//			foreach ( var item in results )
		//				context.Entity_RevocationProfile.Remove( item );

		//			context.SaveChanges();
		//		}
		//		else
		//		{
		//			//deletes should be direct??
		//			//should only have existing ids, where not in current list, so should be deletes
		//			var deleteList = from existing in results
		//							 join item in profiles
		//									 on existing.Id equals item.Id
		//									 into joinTable
		//							 from result in joinTable.DefaultIfEmpty( new ThisEntity { Id = 0, ParentId = 0 } )
		//							 select new { DeleteId = existing.Id, ParentId = ( result.ParentId ) };

		//			foreach ( var v in deleteList )
		//			{
		//				if ( v.ParentId == 0 )
		//				{
		//					//delete item
		//					DBentity p = context.Entity_RevocationProfile.FirstOrDefault( s => s.Id == v.DeleteId );
		//					if ( p != null && p.Id > 0 )
		//					{
		//						context.Entity_RevocationProfile.Remove( p );
		//						count = context.SaveChanges();
		//					}
		//				}
		//			}
		//		}

		//	}

		//	return isValid;
		//}

		/// <summary>
		/// Persist Revocation Profiles
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="credential"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool Update( ThisEntity entity, Credential credential, int userId, ref List<string> messages )
		{
			bool isValid = true;
			int intialCount = messages.Count;

			if ( credential.Id < 1 )
			{
				messages.Add( "Error: the credential identifier was not provided." );
			}

			if ( messages.Count > intialCount )
				return false;
			
			int count = 0;

			DBentity efEntity = new DBentity();

			//????SHOULD NOT DO THIS HERE??
			Views.Entity_Summary parent = EntityManager.GetDBEntity( credential.RowId );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{

				bool isEmpty = false;
				int profNbr = 0;

				profNbr++;
				if ( ValidateProfile( entity, ref isEmpty, ref  messages ) == false )
				{
					if (messages.Count == 0)
						messages.Add( string.Format( "Revocation Profile ({0}) was invalid. ", profNbr ) + SetEntitySummary( entity ) );
					return false;
				}
				if ( isEmpty ) //skip
				{
					messages.Add( "Revocation Profile was empty. " );
					return false;
				}

				if ( entity.Id == 0 )
				{
					//add
					efEntity = new DBentity();
					FromMap( entity, efEntity );
					efEntity.EntityId = parent.Id;

					efEntity.Created = efEntity.LastUpdated = DateTime.Now;
					efEntity.CreatedById = efEntity.LastUpdatedById = userId;
					efEntity.RowId = Guid.NewGuid();

					context.Entity_RevocationProfile.Add( efEntity );
					count = context.SaveChanges();
					//update profile record so doesn't get deleted
					entity.Id = efEntity.Id;
					entity.ParentId = parent.Id;
					entity.RowId = efEntity.RowId;
					if ( count == 0 )
					{
						messages.Add( string.Format( " Unable to add Profile: {0} ", string.IsNullOrWhiteSpace( entity.ProfileName ) ? "no description" : entity.ProfileName ) );
					}
					else
					{
						UpdateParts( entity, userId, ref messages );
					}
				}
				else
				{
					entity.ParentId = parent.Id;

					efEntity = context.Entity_RevocationProfile.SingleOrDefault( s => s.Id == entity.Id );
					if ( efEntity != null && efEntity.Id > 0 )
					{
						entity.RowId = efEntity.RowId;
						//update
						FromMap( entity, efEntity );
						//has changed?
						if ( HasStateChanged( context ) )
						{
							efEntity.LastUpdated = System.DateTime.Now;
							efEntity.LastUpdatedById = userId;

							count = context.SaveChanges();
						}
						//always check parts
						UpdateParts( entity, userId, ref messages );
					}

				}

			

			}

			return isValid;
		}
		private bool UpdateParts( ThisEntity entity, int userId, ref List<string> messages )
		{
			bool isAllValid = true;
			//properties
			if ( new EntityPropertyManager().UpdateProperties( entity.RevocationCriteriaType, entity.RowId, CodesManager.ENTITY_TYPE_REVOCATION_PROFILE, CodesManager.PROPERTY_CATEGORY_REVOCATION_CRITERIA_TYPE, userId, ref messages ) == false )
				isAllValid = false;

			Entity_ReferenceManager erm = new Entity_ReferenceManager();

			if ( erm.EntityUpdate( entity.RevocationResourceUrl, entity.RowId, CodesManager.ENTITY_TYPE_REVOCATION_PROFILE, userId, ref messages, 25, false ) == false )
				isAllValid = false;
			

			if ( !entity.IsNewVersion )
			{
				//if ( new RegionsManager().JurisdictionProfile_Update( entity.Jurisdiction, entity.RowId, CodesManager.ENTITY_TYPE_REVOCATION_PROFILE, userId, RegionsManager.JURISDICTION_PURPOSE_SCOPE, ref messages ) == false )
				//	isAllValid = false;


				//if ( new Entity_ReferenceManager().EntityUpdate( entity.RevocationResourceUrl, entity.RowId, CodesManager.ENTITY_TYPE_REVOCATION_PROFILE, userId, ref messages ) == false )
				//	isAllValid = false;
			}

			return isAllValid;
		}

		public bool Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBentity p = context.Entity_RevocationProfile.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_RevocationProfile.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "Revocation Profile record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;

		}
		
		public bool ValidateProfile( ThisEntity profile, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;

			isEmpty = false;
			//check if empty
			if ( string.IsNullOrWhiteSpace( profile.ProfileName) 
				&& string.IsNullOrWhiteSpace( profile.Description )
				&& ( (profile.RevocationCriteriaType == null || profile.RevocationCriteriaType.Items.Count == 0 ) && string.IsNullOrWhiteSpace(profile.OtherRevocationCriteriaType))
				&& ( profile.RevocationResourceUrl == null || profile.RevocationResourceUrl.Count == 0 )
				&& string.IsNullOrWhiteSpace( profile.DateEffective )
				&& ( profile.Jurisdiction == null || profile.Jurisdiction.Count == 0 )
				)
			{
				isEmpty = true;
				return isValid;
			}

			//date check, can this be in the future?
			if ( !string.IsNullOrWhiteSpace( profile.DateEffective )
				&& !IsValidDate( profile.DateEffective ) )
			{
				messages.Add( "Please enter a valid effective date" );
				isValid = false;
			}

			//if ( ( profile.RevocationCriteriaType == null || profile.RevocationCriteriaType.Items.Count == 0 ) && string.IsNullOrWhiteSpace( profile.OtherRevocationCriteriaType ) 
			//	)
			//{
			//	messages.Add( "Please select a criteria type, or enter Other Criteria" );
			//	isValid = false;
			//}

			return isValid;
		}

		#endregion
		#region  retrieval ==================

		/// <summary>
		/// Get all Revocation Profiles for the parent
		/// Uses the parent Guid to retrieve the related Entity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		public static List<ThisEntity> GetAll( Guid parentUid )
		{
			ThisEntity entity = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBentity> results = context.Entity_RevocationProfile
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity item in results )
						{
							entity = new ThisEntity();
							ToMap( item, entity, true );

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

		public static ThisEntity Get( int profileId )
		{
			ThisEntity entity = new ThisEntity();

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentity item = context.Entity_RevocationProfile
							.SingleOrDefault( s => s.Id == profileId );

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
			to.ProfileName = from.ProfileName;
			to.Description = from.Description;
			to.RevocationCriteriaUrl = from.RevocationCriteriaUrl;

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = DateTime.Parse( from.DateEffective );
			else
				to.DateEffective = null;
			//if ( IsValidDate( from.RenewalDateEffective ) )
			//	to.RenewalDateEffective = DateTime.Parse( from.RenewalDateEffective );
			//else
			//	to.RenewalDateEffective = null;
			

		}
		public static void ToMap( DBentity from, ThisEntity to, bool includingItems = true )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.ParentId = from.EntityId;
			to.ProfileName = from.ProfileName;
			to.Description = from.Description;

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = ( ( DateTime ) from.DateEffective ).ToShortDateString();
			else
				to.DateEffective = "";
			//if ( IsValidDate( from.RenewalDateEffective ) )
			//	to.RenewalDateEffective = ( ( DateTime ) from.RenewalDateEffective ).ToShortDateString();
			//else
			//	to.RenewalDateEffective = "";

			to.RevocationCriteriaUrl = from.RevocationCriteriaUrl;

			to.ProfileSummary = SetEntitySummary( to );

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

			if ( includingItems )
			{
				to.RevocationCriteriaType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_REVOCATION_CRITERIA_TYPE );
				//
				to.Jurisdiction = RegionsManager.Jurisdiction_GetAll( to.RowId );

				if ( !to.IsNewVersion )
				{
					to.RevocationResourceUrl = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS );
				}
			}
		}
		static string SetEntitySummary( ThisEntity to )
		{
			string summary = "Revocation Profile ";
			if ( !string.IsNullOrWhiteSpace( to.ProfileName ) )
			{
				return to.ProfileName;
			}

			if ( to.Id > 1 )
			{
				summary += to.Id.ToString();
			}
			return summary;

		}
		#endregion

	}
}
