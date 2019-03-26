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
using DBEntity = Data.Entity_Language;
using ThisEntity = Models.ProfileModels.LanguageProfile;

using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;

namespace Factories
{
    public class Entity_LanguageManager : BaseFactory
    {
        /// <summary>
        /// if true, return an error message if the assessment is already associated with the parent
        /// </summary>
        private bool ReturningErrorOnDuplicate { get; set; }
        public Entity_LanguageManager()
        {
        }

        public Entity_LanguageManager( bool returnErrorOnDuplicate )
        {
            ReturningErrorOnDuplicate = returnErrorOnDuplicate;
        }
        static string thisClassName = "Entity_LanguageManager";
        /// <summary>
        /// Get all assessments for the provided entity
        /// The returned entities are just the base
        /// </summary>
        /// <param name="parentUid"></param>
        /// <returns></returns>
        public static List<LanguageProfile> GetAll( Guid parentUid )
        {
            List<LanguageProfile> list = new List<LanguageProfile>();
            LanguageProfile entity = new LanguageProfile();

            Entity parent = EntityManager.GetEntity( parentUid );
            LoggingHelper.DoTrace( 7, string.Format( "EntityAssessments_GetAll: parentUid:{0} entityId:{1}, e.EntityTypeId:{2}", parentUid, parent.Id, parent.EntityTypeId ) );

            try
            {
                using ( var context = new Data.CTIEntities() )
                {
                    List<DBEntity> results = context.Entity_Language.Where( s => s.EntityId == parent.Id ).ToList();

                    if ( results != null && results.Count > 0 )
                    {
                        foreach ( DBEntity item in results )
                        {
                            entity = new LanguageProfile { LanguageCodeId = item.LanguageCodeId, LanguageName = item.Codes_Language.LanguageName, LanguageCode = item.Codes_Language.LangugeCode };

                            list.Add( entity );
                        }
                    }
                    return list;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".EntityAssessments_GetAll" );
            }
            return list;
        }

        public static ThisEntity Get( int parentId, int assessmentId )
        {
            ThisEntity entity = new ThisEntity();
            if ( parentId < 1 || assessmentId < 1 )
            {
                return entity;
            }
            try
            {
                using ( var context = new Data.CTIEntities() )
                {
                    EM.Entity_Assessment from = context.Entity_Assessment
                            .SingleOrDefault( s => s.AssessmentId == assessmentId && s.EntityId == parentId );

                    if ( from != null && from.Id > 0 )
                    {
                        entity.Id = from.Id;
                        //entity.AssessmentId = from.AssessmentId;
                        //entity.EntityId = from.EntityId;

                        entity.ProfileSummary = from.Assessment.Name;
                        //to.Credential = from.Credential;
                        //entity.Assessment = new AssessmentProfile();
                        //AssessmentManager.MapFromDB_Basic( from.Assessment, entity.Assessment,
                        //		false, //includeCosts - propose to use for credential editor
                        //		false
                        //		);

                        if ( IsValidDate( from.Created ) )
                            entity.Created = ( DateTime )from.Created;
                        entity.CreatedById = from.CreatedById == null ? 0 : ( int )from.CreatedById;
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".Get" );
            }
            return entity;
        }//

        public bool Update( List<ThisEntity> profiles,
             Guid parentUid,
             int userId,
             ref List<string> messages )
        {
            bool isValid = true;
            int intialCount = messages.Count;
            if ( !IsValidGuid( parentUid ) )
            {
                messages.Add( "Error: the parent identifier was not provided." );
                return false;
            }


            int count = 0;
            if ( profiles == null ) profiles = new List<ThisEntity>();

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
                    //need to be careful here - as required, can't delete all
                    if ( !profiles.Any() )
                    {
                        //delete all languages?
                        var existingLanguages = context.Entity_Language.Where( s => s.EntityId == parent.Id );
                        //foreach ( var language in existingLanguages )
                        //    context.Entity_Language.Remove( language );

                        //context.SaveChanges();
                    }
                    else
                    {
                        var existingLanguages = context.Entity_Language.Where( s => s.EntityId == parent.Id );
                        var languageIds = profiles.Select( x => x.LanguageCodeId ).ToList();

                        //delete languages which are not selected 
                        var notExistLanguages = existingLanguages.Where( x => !languageIds.Contains( x.LanguageCodeId ) ).ToList();
                        foreach ( var language in notExistLanguages )
                        {
                            context.Entity_Language.Remove( language );
                            context.SaveChanges();
                        }

                        foreach ( ThisEntity entity in profiles )
                        {
                            efEntity = context.Entity_Language.FirstOrDefault( s => s.EntityId == parent.Id && s.LanguageCodeId == entity.LanguageCodeId );

                            if ( efEntity == null || efEntity.Id == 0)
                            {
                                //add new language which are not exist in table
                                efEntity = new DBEntity { EntityId = parent.Id,LanguageCodeId = entity.LanguageCodeId,
                                    Created = DateTime.Now,
                                    CreatedById = userId };
                                context.Entity_Language.Add( efEntity );
                                count = context.SaveChanges();
                            }

                        } //foreach
                    }
                }
            } catch (Exception ex)
            {
                string message = FormatExceptions( ex );
                messages.Add( message );
                LoggingHelper.LogError( ex,"Entity_LanguageManager.Update()" );
            }
            return isValid;
        } //
    }
}
