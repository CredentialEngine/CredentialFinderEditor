using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBentity = Data.Entity_Reference;
using Entity = Models.ProfileModels.TextValueProfile;

using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class Entity_ReferenceManager : BaseFactory
	{
		static string thisClassName = "Entity_ReferenceManager";
		static int defaultCategoryId = CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS;
		int maxReferenceTextLength = UtilityManager.GetAppKeyValue( "maxReferenceTextLength", 500 );
		int maxKeywordLength = UtilityManager.GetAppKeyValue( "maxKeywordLength", 200 );

		#region Entity Persistance ===================
		/// <summary>
		/// Persist Entity Reference
		/// </summary>
		/// <param name="profiles"></param>
		/// <param name="parentUid"></param>
		/// <param name="parentTypeId"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <param name="categoryId"></param>
		/// <param name="isTitleRequired">If true, a title must exist</param>
		/// <returns></returns>
		public bool EntityUpdate( List<Entity> profiles, 
				Guid parentUid, 
				int parentTypeId, 
				int userId, 
				ref List<string> messages, 
				int categoryId = 25, 
				bool isTitleRequired = true)
		{
			bool isValid = true;
			int intialCount = messages.Count;
			int maxTextLength = 500;
			if ( !IsValidGuid( parentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}
			if ( parentTypeId == 0 )
			{
				messages.Add( "Error: the parent type was not provided." );
			}
			if ( messages.Count > intialCount )
				return false;

			int count = 0;
			bool hasData = false;
			if ( profiles == null )
				profiles = new List<Entity>();

			DBentity efEntity = new DBentity();

			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				//check add/updates first
				if ( profiles.Count() > 0 )
				{
					hasData = true;
					bool isEmpty = false;

					foreach ( Entity entity in profiles )
					{
						entity.CategoryId = categoryId;
						if ( Validate( entity, isTitleRequired, ref isEmpty, ref  messages ) == false )
						{
							messages.Add( "Reference profile was invalid. " 
								+ (string.IsNullOrWhiteSpace( entity.TextTitle ) ? "" : entity.TextTitle)  );
							isValid = false;
							continue;
						}
						if ( entity.CategoryId == 0)
							entity.CategoryId = categoryId;
						if ( isEmpty ) //skip
							continue;
						entity.EntityBaseId = parent.BaseId;
						entity.LastUpdatedById = userId;

						if ( entity.Id == 0 )
						{
							//add
							efEntity = new DBentity();
							FromMap( entity, efEntity );
							efEntity.EntityId = parent.Id;

							efEntity.Created = efEntity.LastUpdated = DateTime.Now;
							efEntity.CreatedById = efEntity.LastUpdatedById = userId;
							

							context.Entity_Reference.Add( efEntity );
							count = context.SaveChanges();
							//update profile record so doesn't get deleted
							entity.Id = efEntity.Id;
							entity.ParentId = parent.Id;
							
							if ( count == 0 )
							{
								ConsoleMessageHelper.SetConsoleErrorMessage( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.TextTitle ) ? "no description" : entity.TextTitle ) );
							}

						}
						else
						{
							entity.ParentId = parent.Id;

							efEntity = context.Entity_Reference.SingleOrDefault( s => s.Id == entity.Id );
							if ( efEntity != null && efEntity.Id > 0 )
							{
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

					} //foreach

				}

				#region deletes should be direct?? NOW
				//check for deletes ====================================
				//need to ensure ones just added don't get deleted
				//deletes should be direct?? NOW
				//get existing ==> use category Id
				//List<DBentity> results = context.Entity_Reference
				//		.Where( s => s.EntityId == parent.Id )
				//		.OrderBy( s => s.Id )
				//		.ToList();

				////if profiles is null, need to delete all!!
				//if ( results.Count() > 0 && profiles.Count() == 0 )
				//{
				//	foreach ( var item in results )
				//		context.Entity_Reference.Remove( item );

				//	context.SaveChanges();
				//}
				//else
				//{

				//	//should only have existing ids, where not in current list, so should be deletes
				//	var deleteList = from existing in results
				//					 join item in profiles
				//							 on existing.Id equals item.Id
				//							 into joinTable
				//					 from result in joinTable.DefaultIfEmpty( new Entity { Id = 0, ParentId = 0 } )
				//					 select new { DeleteId = existing.Id, ParentId = ( result.ParentId ) };

				//	foreach ( var v in deleteList )
				//	{
				//		if ( v.ParentId == 0 )
				//		{
				//			//delete item
				//			DBentity p = context.Entity_Reference.FirstOrDefault( s => s.Id == v.DeleteId );
				//			if ( p != null && p.Id > 0 )
				//			{
				//				context.Entity_Reference.Remove( p );
				//				count = context.SaveChanges();
				//			}
				//		}
				//	}
				//}
				#endregion 
			}

			return isValid;
		}

		//public bool Entity_Update( Entity entity, Guid parentUid, int userId, ref List<string> messages, int categoryId = 25 )
		//{
		//	bool isValid = true;
		//	int intialCount = messages.Count;

		//	if ( !IsValidGuid( parentUid ) )
		//	{
		//		messages.Add( "Error: the parent identifier was not provided." );
		//	}
			
		//	if ( messages.Count > intialCount )
		//		return false;

		//	DBentity efEntity = new DBentity();
		//	Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
		//	if ( parent == null || parent.Id == 0 )
		//	{
		//		messages.Add( "Error - the parent entity was not found." );
		//		return false;
		//	}
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		bool isEmpty = false;

		//		if ( Validate( entity, ref isEmpty, ref  messages ) == false )
		//		{
		//			messages.Add( "Reference profile was invalid. "
		//				+ ( string.IsNullOrWhiteSpace( entity.TextTitle ) ? "" : entity.TextTitle ) );
		//			return false;
		//		}
		//		if ( entity.CategoryId == 0 )
		//			entity.CategoryId = categoryId;
		//		if ( isEmpty ) //skip
		//		{
		//			messages.Add( "Reference profile is empty. ");
		//			return false;
		//		}

		//		if ( entity.Id == 0 )
		//		{
		//			//add
		//			efEntity = new DBentity();
		//			FromMap( entity, efEntity );
		//			efEntity.EntityId = parent.Id;

		//			efEntity.Created = efEntity.LastUpdated = DateTime.Now;
		//			efEntity.CreatedById = efEntity.LastUpdatedById = userId;


		//			context.Entity_Reference.Add( efEntity );
		//			int count = context.SaveChanges();
		//			//update profile record so doesn't get deleted
		//			entity.Id = efEntity.Id;
		//			entity.ParentId = parent.Id;

		//			if ( count == 0 )
		//			{
		//				ConsoleMessageHelper.SetConsoleErrorMessage( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.TextTitle ) ? "no description" : entity.TextTitle ) );
		//			}

		//		}
		//		else
		//		{
		//			entity.ParentId = parent.Id;

		//			efEntity = context.Entity_Reference.SingleOrDefault( s => s.Id == entity.Id );
		//			if ( efEntity != null && efEntity.Id > 0 )
		//			{
		//				//update
		//				FromMap( entity, efEntity );
		//				//has changed?
		//				if ( HasStateChanged( context ) )
		//				{
		//					efEntity.LastUpdated = System.DateTime.Now;
		//					efEntity.LastUpdatedById = userId;

		//					int count = context.SaveChanges();
		//				}
		//			}
		//		}
		//	}

		//	return isValid;
		//}

		public bool Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBentity p = context.Entity_Reference.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_Reference.Remove( p );
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


		public bool Validate( Entity profile, bool isTitleRequired, 
			ref bool isEmpty, 
			ref List<string> messages )
		{
			bool isValid = true;

			isEmpty = false;
			//check if empty
			if ( string.IsNullOrWhiteSpace( profile.TextTitle )
				&& string.IsNullOrWhiteSpace( profile.TextValue )
				)
			{
				isEmpty = true;
				return true;
			}
			//16-07-22 mparsons - changed to, for now, let user enter one or the other (except for urls), this gives flexibility to the interface choosing which to show or require
			//ultimately, we will make the profile configurable
			if ( profile.CategoryId == CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS )
			{
				if ( isTitleRequired && string.IsNullOrWhiteSpace( profile.TextTitle ) )
				{
					messages.Add( "A title must be entered" );
					isValid = false;
				}
				//text is normally required, unless a competency item
				if ( string.IsNullOrWhiteSpace( profile.TextValue ) )
				{
					messages.Add( "A URL must be entered" );
					isValid = false;
				}
			}

			if ( profile.CategoryId == CodesManager.PROPERTY_CATEGORY_KEYWORD )
			{
				if ( !string.IsNullOrWhiteSpace( profile.TextValue ) && profile.TextValue.Length > maxKeywordLength )
				{
					messages.Add( string.Format("Error - the keyword must be less than {0} characters.",maxKeywordLength) );
					isValid = false;
				}
			}
			else
			{
				if ( !string.IsNullOrWhiteSpace( profile.TextTitle ) && profile.TextTitle.Length > maxReferenceTextLength )
				{
					messages.Add( string.Format( "Error - the title must be less than {0} characters.", maxReferenceTextLength ) );
					isValid = false;
				}

				if ( !string.IsNullOrWhiteSpace( profile.TextValue ) && profile.TextValue.Length > maxReferenceTextLength )
				{
					messages.Add( string.Format( "Error - the value must be less than {0} characters.", maxReferenceTextLength ) );
					isValid = false;
				}
			}
			//if ( profile.CategoryId != CodesManager.PROPERTY_CATEGORY_CONDITION_ITEM
			//	&& string.IsNullOrWhiteSpace( profile.TextTitle ) )
			//{
			//	messages.Add( "A title must be entered" );
			//	isValid = false;
			//}
			////text is normally required, unless a competency item
			//if ( profile.CategoryId != CodesManager.PROPERTY_CATEGORY_COMPETENCY
			//	&& string.IsNullOrWhiteSpace( profile.TextValue ) )
			//{
			//	messages.Add( "A text value must be entered" );
			//	isValid = false;
			//}
			return isValid;
		}

		#endregion

		#region  retrieval ==================

		/// <summary>
		/// Get all profiles for the parent
		/// Uses the parent Guid to retrieve the related Entity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		public static List<Entity> Entity_GetAll( Guid parentUid, int categoryId = 25 )
		{
			Entity entity = new Entity();
			List<Entity> list = new List<Entity>();
			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBentity> results = context.Entity_Reference
							.Where( s => s.EntityId == parent.Id && s.CategoryId == categoryId )
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity item in results )
						{
							entity = new Entity();
							ToMap( item, entity );

							list.Add( entity );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Entity_GetAll" );
			}
			return list;
		}//

		/// <summary>
		/// Get a list of Entity_References using a list of integers
		/// </summary>
		/// <param name="Ids"></param>
		/// <returns></returns>
		public static List<Entity> GetList( List<int> Ids )
		{
			List<TextValueProfile> entities = new List<TextValueProfile>();
			using ( var context = new Data.CTIEntities() )
			{
				List<DBentity> items = context.Entity_Reference.Where( m => Ids.Contains( m.Id ) ).ToList();
				foreach ( var item in items )
				{
					TextValueProfile entity = new TextValueProfile();
					ToMap( item, entity );
					entities.Add( entity );
				}
			}

			return entities;
		}
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
					DBentity item = context.Entity_Reference
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

		public static Entity GetSummary( int profileId )
		{
			Entity entity = new Entity();
			if ( profileId == 0 )
			{
				return entity;
			}
			try
			{
				using ( var context = new ViewContext() )
				{
					Views.Entity_Reference_Summary item = context.Entity_Reference_Summary
							.SingleOrDefault( s => s.EntityReferenceId == profileId );

					if ( item != null && item.EntityReferenceId > 0 )
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
		private static void FromMap( Entity from, DBentity to )
		{
			//want to ensure fields from create are not wiped
			if ( to.Id == 0 )
			{
				if ( IsValidDate( from.Created ) )
					to.Created = from.Created;
				to.CreatedById = from.CreatedById;
			}
			to.Id = from.Id;
			//in some cases may not require text, so fill with empty string
			to.Title = from.TextTitle != null ? from.TextTitle : "";
			if ( to.CategoryId == CodesManager.PROPERTY_CATEGORY_PHONE_TYPE )
			{
				to.TextValue = PhoneNumber.StripPhone( GetData( from.TextValue ) );
			}
			else if ( to.CategoryId == CodesManager.PROPERTY_CATEGORY_ORGANIZATION_IDENTIFIERS )
			{
				//if other, notify admin, and need to handle separately
				if ( from.CodeId == 88 )
				{
					if ( from.Id == 0 )
					{
						//will want to create a property value, and send email
						//could append to text for now
						//op.OtherValue += "{" + ( frameworkName ?? "missing framework name" ) + "; " + schemaUrl + "}";
						LoggingHelper.DoTrace( 2, "A new organization identifier of 'other' has been added:" + from.TextValue );
						SendNewOtherIdentityNotice( from );
					}
				}
				else
				{
					//should ignore to.Title
					to.Title = "";
				}
			}
			else
			{
				to.TextValue = from.TextValue;
			}
			to.CategoryId = from.CategoryId;
			if ( from.CodeId  > 0)
				to.PropertyValueId = from.CodeId;
			else
				to.PropertyValueId = null;

		}
		private static void ToMap( DBentity from, Entity to )
		{
			to.Id = from.Id;
			to.ParentId = from.EntityId;
			to.TextTitle = from.Title;
			to.CategoryId = from.CategoryId;
			if ( to.CategoryId == CodesManager.PROPERTY_CATEGORY_PHONE_TYPE )
			{
				to.TextValue = PhoneNumber.DisplayPhone( from.TextValue );
			}
			else if ( to.CategoryId == CodesManager.PROPERTY_CATEGORY_ORGANIZATION_IDENTIFIERS )
			{
				to.TextValue = from.TextValue;
			}
			else
			{
				to.TextValue = from.TextValue;
			}

			//if ( from.Codes_PropertyValue != null)
			//	to.CodeTitle = from.Codes_PropertyValue.Title;
			to.CodeId = (int)(from.PropertyValueId ?? 0);

			to.ProfileSummary = to.TextTitle + " - " + to.TextValue;
			if ( from.Codes_PropertyValue != null
				&& from.Codes_PropertyValue.Title != null )
			{
				to.CodeTitle = from.Codes_PropertyValue.Title;
				to.CodeSchema = from.Codes_PropertyValue.SchemaName ?? "";

				//to.ProfileSummary += " (" + from.Codes_PropertyValue.Title + ")";
				to.ProfileSummary = from.Codes_PropertyValue.Title + " - " + to.TextValue;
				//to.TextTitle = from.Codes_PropertyValue.Title;
				//just in case
				if ( to.CodeId ==0 )
					to.CodeId = from.Codes_PropertyValue.Id;
			}


			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;


		}

		private static void ToMap( Views.Entity_Reference_Summary from, Entity to )
		{
			to.Id = from.EntityReferenceId;
			to.EntityId = from.EntityId;
			to.EntityBaseId = from.EntityBaseId;
			to.TextTitle = from.Title;
			to.CategoryId = from.CategoryId;
			if ( to.CategoryId == CodesManager.PROPERTY_CATEGORY_PHONE_TYPE )
				to.TextValue = PhoneNumber.DisplayPhone( from.TextValue );
			else
				to.TextValue = from.TextValue;
			to.CodeTitle = from.PropertyValue;
			to.CodeId = ( int ) ( from.PropertyValueId ?? 0 );

			to.ProfileSummary = to.TextTitle + " - " + to.TextValue;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;


		}

		private static void SendNewOtherIdentityNotice( Entity entity )
		{
			string message = string.Format( "New identity. <ul><li>OrganizationId: {0}</li><li>PersonId: {1}</li><li>Title: {2}</li><li>Value: {3}</li></ul>", entity.EntityBaseId, entity.LastUpdatedById, entity.TextTitle, entity.TextValue );
			Utilities.EmailManager.NotifyAdmin( "New Organization Identity has been created", message );
		}
		#endregion

	}
}
