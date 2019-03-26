using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBEntity = Data.Entity_ContactPoint;
using ThisEntity = Models.Common.ContactPoint;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class Entity_ContactPointManager : BaseFactory
	{
		static string thisClassName = "Entity_ContactPointManager";
		#region Entity Persistance ===================
		/// <summary>
		/// Persist ContactPoint
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool Save( ThisEntity entity, Guid parentUid, int userId, ref List<string> messages )
		{
			bool isValid = true;
			int intialCount = messages.Count;

			if ( !IsValidGuid( parentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}

			if ( messages.Count > intialCount )
				return false;

			int count = 0;

			DBEntity efEntity = new DBEntity();

			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{

				bool isEmpty = false;

				if ( ValidateProfile( entity, ref  messages ) == false )
				{
					return false;
				}
				

				if ( entity.Id == 0 )
				{
					//add
					efEntity = new DBEntity();
					MapToDB( entity, efEntity );
					efEntity.ParentEntityId = parent.Id;

					efEntity.Created = efEntity.LastUpdated = DateTime.Now;
					efEntity.CreatedById = efEntity.LastUpdatedById = userId;
					efEntity.RowId = Guid.NewGuid();

					context.Entity_ContactPoint.Add( efEntity );
					count = context.SaveChanges();
					//update profile record so doesn't get deleted
					entity.Id = efEntity.Id;
					entity.ParentId = parent.Id;
					entity.RowId = efEntity.RowId;
					if ( count == 0 )
					{
						messages.Add( string.Format( " Unable to add Contact Point: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.ProfileName ) ? "no description" : entity.ProfileName ) );
					}
					else
					{

						UpdateParts( entity, userId, ref messages );
					}
				}
				else
				{
					entity.ParentId = parent.Id;

					efEntity = context.Entity_ContactPoint.SingleOrDefault( s => s.Id == entity.Id );
					if ( efEntity != null && efEntity.Id > 0 )
					{
						entity.RowId = efEntity.RowId;
						//update
						MapToDB( entity, efEntity );
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

			//EntityPropertyManager mgr = new EntityPropertyManager();
			Entity_ReferenceManager erm = new Entity_ReferenceManager();

			if ( erm.Update( entity.SocialMediaPages, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SOCIAL_MEDIA, true ) == false )
				isAllValid = false;


			if ( erm.Update( entity.Emails, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_EMAIL_TYPE, true ) == false )
				isAllValid = false;

			if ( erm.Update( entity.PhoneNumbers, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_PHONE_TYPE, true ) == false )
				isAllValid = false;

			return isAllValid;
		}

		public bool Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity p = context.Entity_ContactPoint.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_ContactPoint.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "Contact Point record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;

		}


		public bool ValidateProfile( ThisEntity profile, ref List<string> messages )
		{
			bool isValid = true;
			int count = messages.Count;

			if ( string.IsNullOrWhiteSpace( profile.ProfileName ) )
			{
				messages.Add( "A contact point name must be entered" );
			}
			//not sure if should be required?
			if ( string.IsNullOrWhiteSpace( profile.ContactType ) )
			{
				//messages.Add( "A contact point type must be entered" );
			}

			//if ( !IsUrlWellFormed( profile.SocialMedia ) )
			//{
			//	messages.Add( "The Social Media Url is invalid" );
			//}

			//make this a method
			//IsPhoneValid( profile.Telephone, "phone", ref messages );
			//IsPhoneValid( profile.FaxNumber, "fax", ref messages );

			//string phoneNbr = PhoneNumber.StripPhone( GetData( profile.Telephone ) );

			//if ( !string.IsNullOrWhiteSpace( phoneNbr ) && phoneNbr.Length < 10 )
			//{
			//	messages.Add( string.Format( "Error - A phone number ({0}) must have at least 10 numbers.", profile.Telephone ) );
			//}
			//phoneNbr = PhoneNumber.StripPhone( GetData( profile.FaxNumber ) );

			//if ( !string.IsNullOrWhiteSpace( phoneNbr ) && phoneNbr.Length < 10 )
			//{
			//	messages.Add( string.Format( "Error - A Fax number ({0}) must have at least 10 numbers.", profile.FaxNumber ) );
			//}

			//needs to be one of email, phone, fax, or list
			//will be this or a check of the lists
			bool hasContent = false;
			//if ( !string.IsNullOrWhiteSpace( profile.Email )
			//	|| !string.IsNullOrWhiteSpace( profile.FaxNumber )
			//	|| !string.IsNullOrWhiteSpace( profile.Telephone )
			//	|| !string.IsNullOrWhiteSpace( profile.SocialMedia )
			//	)
			//{
			//	hasContent = true;
			//}

			if ( profile.PhoneNumbers.Count > 0 ||
				profile.Emails.Count > 0 ||
				profile.SocialMediaPages.Count > 0 )
			{
				hasContent = true;
			}
			if ( !hasContent )
				messages.Add( "A contact point must have at least one phone, email, or URL" );

			if ( messages.Count > count )
				isValid = false;
			return isValid;
		}

		#endregion
		#region  retrieval ==================

		/// <summary>
		/// Get all ContactPoint for the parent
		/// Uses the parent Guid to retrieve the related Entity, then uses the ParentEntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		public static List<ThisEntity> GetAll( Guid parentUid )
		{
			ThisEntity entity = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			//Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBEntity> results = context.Entity_ContactPoint
							.Where( s => s.ParentEntityId == parent.Id )
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
					DBEntity item = context.Entity_ContactPoint
							.SingleOrDefault( s => s.Id == profileId );

					if ( item != null && item.Id > 0 )
					{
						MapFromDB( item, entity, true );
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
			to.Name = from.ProfileName;

			to.ContactType = from.ContactType;
			//to.ContactOption = from.ContactOption;

			//to.Email = from.Email;
			//to.Telephone = from.Telephone;
			//to.Fax = from.FaxNumber;
			//to.SocialMedia = from.SocialMedia;


		}
		public static void MapFromDB( DBEntity from, ThisEntity to, bool includingItems = true )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.ParentId = from.ParentEntityId;
			if ( from.Entity != null )
				to.ParentRowId = from.Entity.EntityUid;
			to.ProfileName = from.Name;

			to.ContactType = from.ContactType;
			//to.ContactOption = from.ContactOption;

			//to.Email = from.Email;
			//to.Telephone = from.Telephone;
			//to.FaxNumber = from.Fax;
			//to.SocialMedia = from.SocialMedia;

			if ( string.IsNullOrWhiteSpace( to.ProfileName ) )
				to.ProfileName = SetEntitySummary( to );


			if ( includingItems )
			{
				to.SocialMediaPages = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SOCIAL_MEDIA );
				to.PhoneNumbers = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_PHONE_TYPE );
				to.Emails = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_EMAIL_TYPE );
			}


			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;
			to.LastUpdatedBy = SetLastUpdatedBy( to.LastUpdatedById, from.Account_Modifier );
		}
		static string SetEntitySummary( ThisEntity to )
		{
			string summary = "Contact Point ";
			if ( !string.IsNullOrWhiteSpace( to.ProfileName ) )
			{
				return to.ProfileName;
			}
			else if ( !string.IsNullOrWhiteSpace( to.ContactType ) )
			{
				return to.ContactType;
			}
			//else if ( !string.IsNullOrWhiteSpace( to.ContactOption ) )
			//{
			//	return to.ContactOption;
			//}
			//else if ( !string.IsNullOrWhiteSpace( to.Telephone ) )
			//{
			//	return "Telephone: " + to.Telephone;
			//}
			//else if ( !string.IsNullOrWhiteSpace( to.FaxNumber ) )
			//{
			//	return "Fax: " + to.FaxNumber;
			//}
			//else if ( !string.IsNullOrWhiteSpace( to.Email ) )
			//{
			//	return "Email: " + to.Email;
			//}
			if ( to.Id > 1 )
			{
				summary += to.Id.ToString();
			}
			return summary;

		}
		#endregion

	}
}
