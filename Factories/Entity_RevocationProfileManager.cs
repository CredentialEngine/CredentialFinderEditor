using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBEntity = Data.Entity_RevocationProfile;
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
		/// <param name="entity"></param>
		/// <param name="credential"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool Save( ThisEntity entity, Credential credential, int userId, ref List<string> messages )
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

			DBEntity efEntity = new DBEntity();

			//????SHOULD NOT DO THIS HERE??
			Entity parent = EntityManager.GetEntity( credential.RowId );
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
						messages.Add( string.Format( "Revocation Profile ({0}) was invalid. ", profNbr ) );
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
					efEntity = new DBEntity();
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
					}

				}

			

			}

			return isValid;
		}
		

		public bool Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity p = context.Entity_RevocationProfile.FirstOrDefault( s => s.Id == recordId );
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
			int count = messages.Count;
			isEmpty = false;
			if ( profile.IsStarterProfile )
				return true;
			/*
			&& ( profile.RevocationResourceUrl == null || profile.RevocationResourceUrl.Count == 0 )
			&& ( ( profile.RevocationCriteriaType == null || profile.RevocationCriteriaType.Items.Count == 0 ) && string.IsNullOrWhiteSpace( profile.OtherRevocationCriteriaType ) )
			*/
			//check if empty
			if ( string.IsNullOrWhiteSpace( profile.ProfileName) 
				&& string.IsNullOrWhiteSpace( profile.Description )
				&& string.IsNullOrWhiteSpace( profile.RevocationCriteriaUrl )
				&& string.IsNullOrWhiteSpace( profile.RevocationCriteriaDescription )
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
			if ( !IsUrlValid( profile.RevocationCriteriaUrl, ref commonStatusMessage ) )
			{
				messages.Add( "The 'Revocation Criteria Url' format is invalid. " + commonStatusMessage );
			}
			//if ( ( profile.RevocationCriteriaType == null || profile.RevocationCriteriaType.Items.Count == 0 ) && string.IsNullOrWhiteSpace( profile.OtherRevocationCriteriaType ) 
			//	)
			//{
			//	messages.Add( "Please select a criteria type, or enter Other Criteria" );
			//	isValid = false;
			//}
			
			if ( messages.Count > count )
				isValid = false;
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
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBEntity> results = context.Entity_RevocationProfile
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBEntity item in results )
						{
							entity = new ThisEntity();
							MapFromDB( item, entity, true );

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
					DBEntity item = context.Entity_RevocationProfile
							.SingleOrDefault( s => s.Id == profileId );

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

		public static void FromMap( ThisEntity from, DBEntity to )
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
			to.RevocationCriteriaDescription = from.RevocationCriteriaDescription;
			
			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = DateTime.Parse( from.DateEffective );
			else
				to.DateEffective = null;
			//if ( IsValidDate( from.RenewalDateEffective ) )
			//	to.RenewalDateEffective = DateTime.Parse( from.RenewalDateEffective );
			//else
			//	to.RenewalDateEffective = null;
			

		}
		public static void MapFromDB( DBEntity from, ThisEntity to, bool includingItems = true )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.ParentId = from.EntityId;

			to.Description = from.Description;

			to.RevocationCriteriaDescription = from.RevocationCriteriaDescription;
			to.RevocationCriteriaUrl = from.RevocationCriteriaUrl;

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = ( ( DateTime ) from.DateEffective ).ToShortDateString();
			else
				to.DateEffective = "";
			//if ( IsValidDate( from.RenewalDateEffective ) )
			//	to.RenewalDateEffective = ( ( DateTime ) from.RenewalDateEffective ).ToShortDateString();
			//else
			//	to.RenewalDateEffective = "";

			to.RevocationCriteriaUrl = from.RevocationCriteriaUrl;
			if ( ( from.Entity.EntityBaseName ?? "" ).Length > 3 )
				to.ParentSummary = from.Entity.EntityBaseName;
			//not used:
			to.ProfileSummary = SetEntitySummary( to );
			//no longer using name, but need for the editor list
			to.ProfileName = to.ProfileSummary;
			

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;
			to.LastUpdatedBy = SetLastUpdatedBy( to.LastUpdatedById, from.Account_Modifier );

			if ( includingItems )
			{
				//to.CredentialProfiled = Entity_CredentialManager.GetAll( to.RowId );
				//
				to.Jurisdiction = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId );
				to.Region = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_RESIDENT );

				//to.RevocationCriteriaType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_REVOCATION_CRITERIA_TYPE );
				//to.RevocationResourceUrl = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS );

				//to.RevocationItems = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_CONDITION_ITEM );

			}
		}
		static string SetEntitySummary( ThisEntity to )
		{
			string summary = "Revocation Profile ";
			if ( !string.IsNullOrWhiteSpace( to.ProfileName ) )
			{
				//shouldn't use, as some old data may appear, and can't be updated
				//return to.ProfileName;
			}
			if ( !string.IsNullOrWhiteSpace( to.ParentSummary ) )
			{
				summary += " for " + to.ParentSummary;
			}
			if ( to.Id > 1 )
			{
				//summary += to.Id.ToString();
			}
			return summary;

		}
		#endregion

	}
}
