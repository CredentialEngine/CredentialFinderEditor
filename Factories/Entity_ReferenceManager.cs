using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBEntity = Data.Entity_Reference;
using ThisEntity = Models.ProfileModels.TextValueProfile;

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
        int maxReferenceUrlLength = UtilityManager.GetAppKeyValue( "maxReferenceUrlLength", 600 );

        #region Entity Persistance ===================
        public bool Update( List<ThisEntity> profiles,
             Guid parentUid,
             int parentTypeId,
             int userId,
             ref List<string> messages,
             int categoryId,
             bool isTitleRequired,
             bool skipIsDuplicateMessage = false )
        {
            //bool skipIsDuplicateMessage = false;
            bool skippingUpdateOfEntityLastUpdated = false;
            return UpdateMain( profiles, parentUid, parentTypeId, userId, ref messages, categoryId, skipIsDuplicateMessage, skippingUpdateOfEntityLastUpdated );
        }

        public bool Update( List<ThisEntity> profiles,
             Guid parentUid,
             int parentTypeId,
             int userId,
             ref List<string> messages,
             int categoryId,
             bool isTitleRequired,
             bool skippingUpdateOfEntityLastUpdated,
             bool skipIsDuplicateMessage )
        {

            return UpdateMain( profiles, parentUid, parentTypeId, userId, ref messages, categoryId, skipIsDuplicateMessage, skippingUpdateOfEntityLastUpdated );
        }

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
        private bool UpdateMain( List<ThisEntity> profiles,
                Guid parentUid,
                int parentTypeId,
                int userId,
                ref List<string> messages,
                int categoryId,
                bool isTitleRequired,
                bool skipIsDuplicateMessage,
                bool skippingUpdateOfEntityLastUpdated = false )
        {
            bool isValid = true;
            int intialCount = messages.Count;
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
            if ( profiles == null )
                profiles = new List<ThisEntity>();

            DBEntity efEntity = new DBEntity();
            Entity parent = EntityManager.GetEntity( parentUid );
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
                    bool isEmpty = false;
                    bool isDuplicate = false;

                    foreach ( ThisEntity entity in profiles )
                    {
                        entity.CategoryId = categoryId;
                        CodeItem category = CodesManager.Codes_PropertyCategory_Get( categoryId );
                        if ( Validate( entity, parentUid, isTitleRequired, ref isEmpty, ref isDuplicate, ref messages ) == false )
                        {
                            isValid = false;
                            continue;
                        }

                        if ( isDuplicate )
                        {
                            if ( !skipIsDuplicateMessage )
                                messages.Add( "Error: the entered value already exists: " + entity.TextValue );
                            continue;
                        }
                        if ( isEmpty ) //skip
                            continue;
                        entity.EntityBaseId = parent.EntityBaseId;

                        //MapToDB( entity, efEntity );
                        if ( entity.Id != 0 )
                            efEntity = context.Entity_Reference.SingleOrDefault( s => s.Id == entity.Id );
                        else
                            efEntity = context.Entity_Reference.SingleOrDefault( s => s.EntityId == parent.Id 
							&& s.CategoryId == entity.CategoryId
							&& s.TextValue == entity.TextValue );

                        if ( efEntity == null )
                        //if ( entity.Id == 0 )
                        {
                            //add
                            efEntity = new DBEntity();
                            MapToDB( entity, efEntity );
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
                                messages.Add( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.TextTitle ) ? entity.TextValue : entity.TextTitle ) );
                            }
                            else
                            {
                                if ( !skippingUpdateOfEntityLastUpdated )
                                    new EntityManager().UpdateTopLevelEntityLastUpdateDate( parent.Id, string.Format( "Entity Update triggered by userId: {0} adding a {1} of  {2} to: {3}, BaseId: {4}", userId, category.Title, entity.TextValue, parent.EntityType, parent.EntityBaseId ), userId );

                                if ( categoryId == CodesManager.PROPERTY_CATEGORY_SUBJECT )
                                    AddConnections( entity.Id );
                            }
                        }
                        else
                        {
                            entity.ParentId = parent.Id;

                            //efEntity = context.Entity_Reference.SingleOrDefault( s => s.Id == entity.Id );
                            if ( efEntity != null && efEntity.Id > 0 )
                            {
                                //update
                                MapToDB( entity, efEntity );
                                //has changed?
                                if ( HasStateChanged( context ) )
                                {
                                    efEntity.LastUpdated = System.DateTime.Now;
                                    efEntity.LastUpdatedById = userId;

                                    count = context.SaveChanges();

                                    new EntityManager().UpdateTopLevelEntityLastUpdateDate( parent.Id, string.Format( "Entity Update triggered by userId: {0} updating a {1} of  {2} to: {3}, BaseId: {4}", userId, category.Title, entity.TextValue, parent.EntityType, parent.EntityBaseId ), userId );
                                }
                            }
                        }

                    } //foreach
                }

                #region deletes should be direct?? YES - but not from bulk upload
                //check for deletes ====================================
                //need to ensure ones just added don't get deleted
                //deletes should be direct?? NOW
                //get existing ==> use category Id

                //List<DBEntity> results = context.Entity_Reference
                //        .Where( s => s.EntityId == parent.Id )
                //        .OrderBy( s => s.Id )
                //        .ToList();

                //if profiles is null, need to delete all!!
                //if ( results.Count() > 0 && profiles.Count() == 0 )
                //{
                //	foreach ( var item in results )
                //		context.Entity_Reference.Remove( item );

                //	context.SaveChanges();
                //}
                //else
                //{

                //should only have existing ids, where not in current list, so should be deletes
                //	var deleteList = from existing in results
                //					 join item in profiles
                //							 on existing.Id equals item.Id
                //							 into joinTable
                //					 from result in joinTable.DefaultIfEmpty( new ThisEntity { Id = 0, ParentId = 0 } )
                //					 select new { DeleteId = existing.Id, ParentId = ( result.ParentId ) };

                //	foreach ( var v in deleteList )
                //	{
                //		if ( v.ParentId == 0 )
                //		{
                //			//delete item
                //			DBEntity p = context.Entity_Reference.FirstOrDefault( s => s.Id == v.DeleteId );
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
        } //

		public bool Replace( Guid parentUid, int categoryId, List<string> profiles, int userId, ref List<string> messages )
		{
			bool isValid = true;
			messages = new List<string>();
			string statusMessage = "";
			//there will be variations dependent on categoryId. So make clear only applicable to subjects and keywords at this time
		
			switch (categoryId)
			{
				case 34: // CodesManager.PROPERTY_CATEGORY_SUBJECT:
				case 35: // CodesManager.PROPERTY_CATEGORY_KEYWORD:
				case 10: // CodesManager.PROPERTY_CATEGORY_NAICS:
				case 11: // CodesManager.PROPERTY_CATEGORY_SOC:
				case 23: // CodesManager.PROPERTY_CATEGORY_CIP:
                case 62: //CodesManager.PROPERTY_CATEGORY_DEGREE_CONCENTRATION
                case 63://CodesManager.PROPERTY_CATEGORY_DEGREE_MAJOR
                case 64: //CodesManager.PROPERTY_CATEGORY_DEGREE_MINOR
                    break;
				default:
					messages.Add( "Error: the Entity_ReferenceManager.Replace method is only applicable to keywords, subjects, and others but not this type, at this time." );
					return false;
			}
			if ( !IsValidGuid( parentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
				return false;
			}
			int count = 0;
			DBEntity efEntity = new DBEntity();
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					if ( !profiles.Any() )
					{
						//no action, this is used initially by bulk upload
					}
					else
					{
						var existing = context.Entity_Reference.Where( s => s.EntityId == parent.Id && s.CategoryId == categoryId );
						var inputCodes = profiles.Select( x => x ).ToList();

						//delete records which are not selected 
						//var notExisting = existing.Where( x => !inputCodes.Equals( x.TextValue ?? "" ) ).ToList();
						//foreach ( var item in notExisting )
						//{
						//	context.Entity_Reference.Remove( item );
						//	context.SaveChanges();
						//}
						foreach ( var item in existing )
						{
							int index = profiles.FindIndex( a => a.ToLower() == item.TextValue.ToLower() );
							if ( index < 0 )
							{
								Delete( item.Id, ref statusMessage );
							}
						}
						foreach ( var text in inputCodes )
						{
							efEntity = context.Entity_Reference.FirstOrDefault( s => s.EntityId == parent.Id
								&& s.CategoryId == categoryId
								&& s.TextValue.ToLower() == text.ToLower() );

							if ( efEntity == null || efEntity.Id == 0 )
							{
								//add new record 
								efEntity = new DBEntity
								{
									EntityId = parent.Id,
									CategoryId = categoryId,
									TextValue = text,
									Created = DateTime.Now,
									CreatedById = userId
								};
								context.Entity_Reference.Add( efEntity );
								count = context.SaveChanges();

								if ( categoryId == CodesManager.PROPERTY_CATEGORY_SUBJECT )
									AddConnections( efEntity.Id );
							}

						} //foreach
					}
				}
			}
			catch ( Exception ex )
			{
				string message = FormatExceptions( ex );
				messages.Add( message );
				LoggingHelper.LogError( ex, thisClassName + ".Replace()" );
				isValid = false;
			}
			return isValid;
		} //
		public void AddRelatedConnections( int entityRefId )
        {
            System.Threading.ThreadPool.QueueUserWorkItem( delegate
            {
                AddConnections( entityRefId );
            } );
        }

        private void AddConnections( int entityRefId )
        {
            if ( entityRefId < 0 )
                return;

            string connectionString = DBConnectionRO();
            using ( SqlConnection c = new SqlConnection( connectionString ) )
            {
                c.Open();

                using ( SqlCommand command = new SqlCommand( "[Entity_ReferenceConnection_Populate]", c ) )
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add( new SqlParameter( "@EntityReferenceId", entityRefId ) );

                    command.ExecuteNonQuery();
                    command.Dispose();
                    c.Close();

                }
            }
        } //

        public bool Delete( int recordId, ref string statusMessage )
        {
            bool isOK = true;
            using ( var context = new Data.CTIEntities() )
            {
                DBEntity p = context.Entity_Reference.FirstOrDefault( s => s.Id == recordId );

                if ( p != null && p.Id > 0 )
                {
                    //CodeItem category = CodesManager.Codes_PropertyCategory_Get( p.CategoryId );
                    context.Entity_Reference.Remove( p );
                    int count = context.SaveChanges();
                    //NOTE: the service method handles doing UpdateTopLevelEntityLastUpdateDate
                    //new EntityManager().UpdateTopLevelEntityLastUpdateDate(entityId, string.Format("Entity Update triggered by userId: {0} removing a {1} of  {2} to: {3}, BaseId: {4}", userId, category.Title, entity.TextValue, parent.EntityType, parent.EntityBaseId));
                }
                else
                {
                    statusMessage = string.Format( "Requested record was not found: {0}", recordId );
                    isOK = false;
                }
            }
            return isOK;

        }

        public bool DeleteAll( int recordId, ref string statusMessage )
        {
            bool isOK = true;
            using ( var context = new Data.CTIEntities() )
            {
                DBEntity p = context.Entity_Reference.FirstOrDefault( s => s.Id == recordId );

                if ( p != null && p.Id > 0 )
                {
                    CodeItem category = CodesManager.Codes_PropertyCategory_Get( p.CategoryId );
                    context.Entity_Reference.Remove( p );
                    int count = context.SaveChanges();
                    //NOTE: the service method handles doing UpdateTopLevelEntityLastUpdateDate
                    //new EntityManager().UpdateTopLevelEntityLastUpdateDate(entityId, string.Format("Entity Update triggered by userId: {0} removing a {1} of  {2} to: {3}, BaseId: {4}", userId, category.Title, entity.TextValue, parent.EntityType, parent.EntityBaseId));
                }
                else
                {
                    statusMessage = string.Format( "Requested record was not found: {0}", recordId );
                    isOK = false;
                }
            }
            return isOK;

        }

        public bool DeleteAll( Guid parentUid, int categoryId, ref string statusMessage )
        {
            bool isOK = true;
            Entity parent = EntityManager.GetEntity( parentUid );
            if ( parent == null || parent.Id == 0 )
            {
                statusMessage = string.Format( "{0}.DeleteAll. Error Entity not found for: {1}", thisClassName, parentUid );
                return false;
            }
            if ( categoryId < 1 )
            {
                statusMessage = string.Format( "{0}.DeleteAll. Error Invalid CategoryId: {1} for parentId: {2}", thisClassName, categoryId, parent.EntityBaseId );
                return false;
            }
            using ( var context = new Data.CTIEntities() )
            {
                context.Entity_Reference.RemoveRange( context.Entity_Reference.Where( s => s.EntityId == parent.Id && s.CategoryId == categoryId ) );
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    isOK = true;
                }

            }
            return isOK;

        } //

        private bool Validate( ThisEntity profile, Guid parentUid, bool isTitleRequired,
            ref bool isEmpty, ref bool isDuplicate,
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

            if ( profile.CategoryId == CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS ||
                 profile.CategoryId == CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SOCIAL_MEDIA ||
                 profile.CategoryId == CodesManager.PROPERTY_CATEGORY_CREDENTIAL_URLS ||
                 profile.CategoryId == CodesManager.PROPERTY_CATEGORY_LEARNING_RESOURCE_URLS )
            {

                if ( ( profile.TextValue ?? "" ).Length > maxReferenceUrlLength )
                {
                    messages.Add( string.Format( "The Url is too long. It must be less than {0} characters", maxReferenceUrlLength ) );
                    isValid = false;

                }
                else if ( !IsUrlValid( profile.TextValue, ref commonStatusMessage ) )
                {
                    messages.Add( string.Format( "The Url is invalid: {0}. {1}", profile.TextValue, commonStatusMessage ) );
                    isValid = false;
                }
                else
                {

                    profile.TextValue = profile.TextValue.TrimEnd( '/' );
                }
            }
            else
            if ( profile.CategoryId == CodesManager.PROPERTY_CATEGORY_KEYWORD )
            {
                if ( !string.IsNullOrWhiteSpace( profile.TextValue ) && profile.TextValue.Length > maxKeywordLength )
                {
                    messages.Add( string.Format( "Error - the keyword must be less than {0} characters.", maxKeywordLength ) );
                    isValid = false;
                }
                else
                {
                    //duplicate check?
                    if ( profile.Id == 0
                        && DoesTextValueExist( parentUid, profile.CategoryId, profile.TextValue ) )
                    {
                        isDuplicate = true;
                        //leave as true for now, ideally make configurable, so can skip from import, and return message from editor.
                        return true;
                    }
                }
            }
            else
            if ( profile.CategoryId == CodesManager.PROPERTY_CATEGORY_SUBJECT )
            {
                if ( !string.IsNullOrWhiteSpace( profile.TextValue ) && profile.TextValue.Length > maxKeywordLength )
                {
                    messages.Add( string.Format( "Error - the subject must be less than {0} characters.", maxKeywordLength ) );
                    isValid = false;
                }
                else
                {
                    //duplicate check?
                    if ( profile.Id == 0
                        && DoesTextValueExist( parentUid, profile.CategoryId, profile.TextValue ) )
                    {
                        isDuplicate = true;
                        //leave as true for now, ideally make configurable, so can skip from import, and return message from editor.
                        return true;
                    }
                }
            } //
            else
            if ( profile.CategoryId == CodesManager.PROPERTY_CATEGORY_SOC )
            {
                if ( !string.IsNullOrWhiteSpace( profile.TextValue ) && profile.TextValue.Length > maxKeywordLength )
                {
                    messages.Add( string.Format( "Error - An other occupation must be less than {0} characters.", maxKeywordLength ) );
                    isValid = false;
                }
                else
                {
                    //duplicate check?
                    if ( profile.Id == 0
                        && DoesTextValueExist( parentUid, profile.CategoryId, profile.TextValue ) )
                    {
                        isDuplicate = true;
                        //leave as true for now, ideally make configurable, so can skip from import, and return message from editor.
                        return true;
                    }
                }
            }
            else
            if ( profile.CategoryId == CodesManager.PROPERTY_CATEGORY_NAICS )
            {
                if ( !string.IsNullOrWhiteSpace( profile.TextValue ) && profile.TextValue.Length > maxKeywordLength )
                {
                    messages.Add( string.Format( "Error - An other industry must be less than {0} characters.", maxKeywordLength ) );
                    isValid = false;
                }
                else
                {
                    //duplicate check?
                    if ( profile.Id == 0
                        && DoesTextValueExist( parentUid, profile.CategoryId, profile.TextValue ) )
                    {
                        isDuplicate = true;
                        //leave as true for now, ideally make configurable, so can skip from import, and return message from editor.
                        return true;
                    }
                }
            }
            else if ( profile.CategoryId == CodesManager.PROPERTY_CATEGORY_PHONE_TYPE )
            {
                string phoneNbr = PhoneNumber.StripPhone( GetData( profile.TextValue ) );

                if ( string.IsNullOrWhiteSpace( phoneNbr ) )
                {
                    messages.Add( "Error - a phone number must be entered." );
                    isValid = false;

                }
                else if ( phoneNbr != "711" && phoneNbr.Length < 10 )
                {
                    messages.Add( string.Format( "Error - A phone number ({0}) must have at least 10 numbers.", profile.TextValue ) );
                    isValid = false;
                }
                //need an other check

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
            if ( profile.CategoryId != CodesManager.PROPERTY_CATEGORY_CONDITION_ITEM
                && string.IsNullOrWhiteSpace( profile.TextTitle ) )
            {
                //messages.Add( "A title must be entered" );
                //isValid = false;
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
        public static List<ThisEntity> GetAll( Guid parentUid, int categoryId )
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
                    var query = from Q in context.Entity_Reference
                            .Where( s => s.EntityId == parent.Id && s.CategoryId == categoryId )
                                select Q;
                    if ( categoryId == CodesManager.PROPERTY_CATEGORY_SUBJECT
                      || categoryId == CodesManager.PROPERTY_CATEGORY_KEYWORD )
                    {
                        query = query.OrderBy( p => p.TextValue );
                    }
                    else
                    {
                        query = query.OrderBy( p => p.Id );
                    }
                    var count = query.Count();
                    var results = query.ToList();

                    if ( results != null && results.Count > 0 )
                    {
                        foreach ( DBEntity item in results )
                        {
                            entity = new ThisEntity();
                            MapFromDB( item, entity );

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

		public static List<string> GetAll( int entityTypeId, int entityBaseId, int categoryId, int maxRecords, ref int pTotalRows )
		{
			List<string> results = new List<string>();
			using ( var context = new ViewContext() )
			{
				var list = context.Entity_Reference_Summary.Where( s => s.EntityTypeId == entityTypeId
							&& s.EntityBaseId == entityBaseId
							&& s.CategoryId == categoryId ).ToList();
				pTotalRows = list.Count();
				foreach ( var entity in list )
				{
					results.Add( entity.TextValue );
					if ( results.Count >= maxRecords )
					{
						break;
					}
				}
			}
			return results;

		}

		public static List<ThisEntity> GetOrgAlternativeIdentifiers( Guid parentUid )
        {
            ThisEntity entity = new ThisEntity();
            List<ThisEntity> list = new List<ThisEntity>();

            Entity parent = EntityManager.GetEntity( parentUid );
            if ( parent == null || parent.Id == 0 )
                return list;

            try
            {
                using ( var context = new ViewContext() )
                {
                    List<Views.Entity_Reference_Summary> results = context.Entity_Reference_Summary
                            .Where( s => s.EntityReferenceId == CodesManager.PROPERTY_CATEGORY_ORGANIZATION_IDENTIFIERS
                            && s.PropertyValue == "Other" )
                            .ToList();

                    if ( results != null && results.Count > 0 )
                    {
                        foreach ( var item in results )
                        {
                            entity = new ThisEntity();
                            MapFromDB( item, entity );

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

        public static List<ThisEntity> GetAllSubjects( Guid parentUid )
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
                string prevSubject = "";
                using ( var context = new ViewContext() )
                {
                    List<Views.Entity_Subjects> results = context.Entity_Subjects
                                .Where( s => s.EntityUid == parentUid )
                                .OrderBy( s => s.Subject )
                                .ToList();

                    if ( results != null && results.Count > 0 )
                    {
                        foreach ( Views.Entity_Subjects item in results )
                        {
                            entity = new ThisEntity();
                            if ( item.Subject != prevSubject )
                            {
                                entity.EntityId = item.EntityId;
                                entity.TextValue = item.Subject;
                                list.Add( entity );
                            }
                            prevSubject = item.Subject;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".Entity_GetAllSubjects" );
            }
            return list;
        }//

        /// <summary>
        /// Get a list of Entity_References using a list of integers
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        public static List<ThisEntity> GetList( List<int> Ids )
        {
            List<TextValueProfile> entities = new List<TextValueProfile>();
            using ( var context = new Data.CTIEntities() )
            {
                List<DBEntity> items = context.Entity_Reference.Where( m => Ids.Contains( m.Id ) ).ToList();
                foreach ( var item in items )
                {
                    TextValueProfile entity = new TextValueProfile();
                    MapFromDB( item, entity );
                    entities.Add( entity );
                }
            }

            return entities;
        }

        /// <summary>
        /// quick search for subjects
        /// </summary>
        /// <param name="entityTypeId"></param>
        /// <param name="keyword"></param>
        /// <param name="maxTerms"></param>
        /// <returns></returns>
        public static List<string> QuickSearch_Subjects( int entityTypeId, string keyword, int maxTerms = 0 )
        {
            List<string> list = new List<string>();

            keyword = keyword.Trim();

            if ( maxTerms == 0 )
                maxTerms = 50;

            using ( var context = new ViewContext() )
            {

                List<Views.Entity_Subjects> results = context.Entity_Subjects
                    .Where( s => s.EntityTypeId == entityTypeId
                        && s.Subject.Contains( keyword )
                        )
                    .OrderBy( s => s.Subject )
                    .Take( maxTerms )
                    .ToList();

                if ( results != null && results.Count > 0 )
                {
                    string prev = "";

                    foreach ( Views.Entity_Subjects item in results )
                    {
                        if ( prev != item.Subject )
                        {
                            list.Add( item.Subject );
                            prev = item.Subject;
                        }
                    }
                }
            }

            return list;
        } //

        public static List<string> QuickSearch_ReferenceFrameworks( int entityTypeId, int categoryId, string keyword, int maxTerms = 0 )
        {
            var list = new List<string>();

            keyword = keyword.Trim();
            var coded = keyword.Replace( "-", "" ).Replace( " ", "" );

            if ( maxTerms == 0 ) maxTerms = 50;

            using ( var context = new ViewContext() )
            {
                var results = ( from rf in context.Entity_FrameworkItems_Totals.AsNoTracking()
                                let cn = rf.FrameworkCode.Replace( "-", "" ).Replace( " ", "" )
                                where rf.CategoryId == categoryId
                                        && rf.EntityTypeId == entityTypeId
                                        && ( keyword == ""
                                        || rf.FrameworkCode.Contains( keyword )
                                        || cn.Contains( coded )
                                        || rf.Title.Contains( keyword ) )
                                        && rf.TOTALS > 0
                                orderby rf.Title
                                select rf ).Distinct().Take( maxTerms ).ToList();

                results.ForEach( x =>
                      {
                          var cd = "";
                          if ( !string.IsNullOrEmpty( x.FrameworkCode ) )
                              cd = string.Format( " ({0})", x.FrameworkCode.Trim() );

                          list.Add( string.Format( "{0}{1}", x.Title.Trim(), cd ) );
                      } );
            }

            return list;
        } //


        public static bool DoesTextValueExist( Guid parentUid, int categoryId, string keyword, int maxTerms = 0 )
        {
            bool doesExist = false;

            keyword = keyword.Trim();

            if ( maxTerms == 0 )
                maxTerms = 50;

            using ( var context = new ViewContext() )
            {
                var results = context.Entity_Reference_Summary
                    .Where( s => s.EntityUid == parentUid
                        && s.CategoryId == categoryId
                        && s.TextValue.Equals( keyword )
                        )
                    .OrderBy( s => s.TextValue )
                    .Select( m => m.TextValue ).Distinct()
                    .Take( maxTerms )
                    .ToList();

                if ( results != null && results.Count > 0 )
                {
                    return true;
                }
            }

            return doesExist;
        }
        public static List<string> QuickSearch_TextValue( int entityTypeId, int categoryId, string keyword, int maxTerms = 0 )
        {
            List<string> list = new List<string>();

            keyword = keyword.Trim();

            if ( maxTerms == 0 )
                maxTerms = 50;

            using ( var context = new ViewContext() )
            {
                // will only return active credentials
                var results = context.Entity_Reference_Summary
                    .Where( s => s.EntityTypeId == entityTypeId
                        && s.CategoryId == categoryId
                        && (
                            ( s.TextValue.Contains( keyword ) ||
                            ( s.Title.Contains( keyword ) )
                            )
                        ) )
                    .OrderBy( s => s.TextValue )
                    .Select( m => m.TextValue ).Distinct()
                    .Take( maxTerms )
                    .ToList();

                if ( results != null && results.Count > 0 )
                {

                    foreach ( string item in results )
                    {
                        list.Add( item );
                    }

                }
            }

            return list;
        }
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
                    DBEntity item = context.Entity_Reference
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

        public static ThisEntity GetSummary( int profileId )
        {
            ThisEntity entity = new ThisEntity();
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
        private static void MapToDB( ThisEntity from, DBEntity to )
        {
            //want to ensure fields from create are not wiped
            if ( to.Id == 0 )
            {
                //if ( IsValidDate( from.Created ) )
                //	to.Created = from.Created;
                //to.CreatedById = from.CreatedById;
            }
            //to.Id = from.Id;
            to.CategoryId = from.CategoryId;
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
                to.TextValue = from.TextValue;
            }
            else
            {
                to.TextValue = from.TextValue;
            }

            if ( from.CodeId > 0 )
                to.PropertyValueId = from.CodeId;
            else
                to.PropertyValueId = null;

        }
        private static void MapFromDB( DBEntity from, ThisEntity to )
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
            to.CodeId = ( int )( from.PropertyValueId ?? 0 );

            to.ProfileSummary = to.TextTitle + " - " + to.TextValue;
            if ( from.Codes_PropertyValue != null
                && from.Codes_PropertyValue.Title != null )
            {
                to.CodeTitle = from.Codes_PropertyValue.Title;
                to.CodeSchema = from.Codes_PropertyValue.SchemaName ?? "";
                //set TextTitle if null
                if ( string.IsNullOrWhiteSpace( to.TextTitle ) )
                    to.TextTitle = to.CodeTitle;

                to.ProfileSummary = from.Codes_PropertyValue.Title + " - " + to.TextValue;

                //just in case
                if ( to.CodeId == 0 )
                    to.CodeId = from.Codes_PropertyValue.Id;
            }


            if ( IsValidDate( from.Created ) )
                to.Created = ( DateTime )from.Created;
            to.CreatedById = from.CreatedById == null ? 0 : ( int )from.CreatedById;
            if ( IsValidDate( from.LastUpdated ) )
                to.LastUpdated = ( DateTime )from.LastUpdated;
            to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int )from.LastUpdatedById;


        }

        private static void MapFromDB( Views.Entity_Reference_Summary from, ThisEntity to )
        {
            to.Id = from.EntityReferenceId;
            to.EntityId = from.EntityId;
            to.EntityBaseId = from.EntityBaseId;
            to.EntityType = from.EntityType;
            to.TextTitle = from.Title;
            to.CategoryId = from.CategoryId;
            if ( to.CategoryId == CodesManager.PROPERTY_CATEGORY_PHONE_TYPE )
                to.TextValue = PhoneNumber.DisplayPhone( from.TextValue );
            else
                to.TextValue = from.TextValue;
            to.CodeTitle = from.PropertyValue;
            to.CodeId = ( int )( from.PropertyValueId ?? 0 );

            to.ProfileSummary = to.TextTitle + " - " + to.TextValue;

            if ( IsValidDate( from.Created ) )
                to.Created = ( DateTime )from.Created;
            to.CreatedById = from.CreatedById == null ? 0 : ( int )from.CreatedById;
            if ( IsValidDate( from.LastUpdated ) )
                to.LastUpdated = ( DateTime )from.LastUpdated;
            to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int )from.LastUpdatedById;


        }

        private static void SendNewOtherIdentityNotice( ThisEntity entity )
        {
            string message = string.Format( "New identity. <ul><li>OrganizationId: {0}</li><li>PersonId: {1}</li><li>Title: {2}</li><li>Value: {3}</li></ul>", entity.EntityBaseId, entity.LastUpdatedById, entity.TextTitle, entity.TextValue );
            Utilities.EmailManager.NotifyAdmin( "New Organization Identity has been created", message );
        }
        #endregion

    }
}
