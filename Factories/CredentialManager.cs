using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Xml.Linq;

using Models;
using MC = Models.Common;
using Models.Import;
using Models.ProfileModels;
using EM = Data;
using DBEntity = Data.Credential;
using Utilities;
using ThisEntity = Models.Common.Credential;
using Views = Data.Views;
using EntityContext = Data.CTIEntities;
using ViewContext = Data.Views.CTIEntities1;
using CondProfileMgr = Factories.Entity_ConditionProfileManager;
//using CondProfileMgrOld = Factories.ConnectionProfileManager;

namespace Factories
{
    public class CredentialManager : BaseFactory
    {
        static string thisClassName = "Factories.CredentialManager";
        List<string> messages = new List<string>();
        private bool SkippingUpdateOfEntityLastUpdated { get; set; }
        private bool SkippingIsDuplicateMessage { get; set; }

        static bool doingExistanceCheck = true;
        public CredentialManager()
        {
            SkippingUpdateOfEntityLastUpdated = false;
        }
        /// <summary>
        /// Actually want to have a context for uploads, and change several properties.
        /// probably better to use method
        /// </summary>
        /// <param name="skipUpdateOfEntityLastUpdated"></param>
        //public CredentialManager( bool isBulkUploadTransaction )
        //{
        //    if ( isBulkUploadTransaction )
        //    {
        //        SkippingUpdateOfEntityLastUpdated = true;
        //        SkippingIsDuplicateMessage = true;
        //        doingExistanceCheck = false;
        //    }
        //}

        /// <summary>
        /// If for an upload, modify processing.
        /// </summary>
        public void SetUploadContext()
        {
            SkippingUpdateOfEntityLastUpdated = true;
            SkippingIsDuplicateMessage = true;
            //existance check now down by upload method, so can skip here.
            doingExistanceCheck = false;
        }
        #region Credential - presistance =======================

        /// <summary>
        /// add a credential
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Add( MC.Credential entity, ref string statusMessage, bool updatingCacheTables = true )
        {
            EM.Credential efEntity = new EM.Credential();
            using ( var context = new EntityContext() )
            {
                try
                {
                    if ( ValidateProfile( entity, ref messages ) == false )
                    {
                        statusMessage = string.Join( "<br/>", messages.ToArray() );
                        return 0;
                    }

                    MapToDB( entity, efEntity );

                    efEntity.RowId = Guid.NewGuid();
                    //check owning org
                    //actually need something concrete
                    //if (IsGuidValid( entity.OwningAgentUid) )
                    //
                    //	MC.Organization org = OrganizationManager.GetBasics( entity.OwningAgentUid );
                    //}
                    if ( !entity.IsReferenceVersion )
                    {
                        if ( !string.IsNullOrWhiteSpace( entity.CTID ) && entity.CTID.Length == 39 )
                            efEntity.CTID = entity.CTID.ToLower();
                        else
                            efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
                    }

                    efEntity.CreatedById = entity.CreatedById;
                    efEntity.Created = System.DateTime.Now;
                    efEntity.LastUpdatedById = entity.LastUpdatedById = entity.CreatedById;
                    efEntity.LastUpdated = System.DateTime.Now;
                    if ( !entity.IsReferenceVersion )
                        efEntity.StatusId = 1; //obsolete?
                    else
                        efEntity.StatusId = 2;

                    context.Credential.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        entity.RowId = efEntity.RowId;
                        statusMessage = "successful";
                        entity.Id = efEntity.Id;

                        UpdateParts( entity, true, ref statusMessage );
                        //add to cache
                        if ( updatingCacheTables )
                            new CacheManager().PopulateEntityRelatedCaches( entity.RowId, true, true );

                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error
                        statusMessage = "Error - the add was not successful. ";
                        string message = string.Format( "CredentialManager. Credential_Add Failed", "Attempted to add a credential. The process appeared to not work, but was not an exception, so we have no message, or no clue.Credential: {0}, createdById: {1}", entity.Name, entity.CreatedById );
                        EmailManager.NotifyAdmin( "CredentialManager. Credential_Add Failed", message );
                    }
                }
                catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
                {
                    statusMessage = HandleDBValidationError( dbex, thisClassName + ".Add() ", "Credential" );
                    LoggingHelper.LogError( dbex, thisClassName + string.Format( ".Add(), Name: {0}, UserId: {1}", entity.Name, entity.CreatedById ) );


                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Name: {0}", efEntity.Name ) );
                    statusMessage = FormatExceptions( ex );

                }
            }

            return efEntity.Id;
        }
        /// <summary>
        /// Update a credential
        /// - base only, caller will handle parts?
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Update( MC.Credential entity, ref string statusMessage, bool updatingCacheTables = true )
        {
            bool isValid = false;
            int count = 0;
            using ( var context = new EntityContext() )
            {
                try
                {
                    //context.Configuration.LazyLoadingEnabled = false;

                    EM.Credential efEntity = context.Credential
                                .SingleOrDefault( s => s.Id == entity.Id );

                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        if ( ValidateProfile( entity, ref messages ) == false )
                        {
                            statusMessage = string.Join( "<br/>", messages.ToArray() );
                            return false;
                        }
                        //**ensure rowId is passed down for use by profiles, etc
                        entity.RowId = efEntity.RowId;
                        MapToDB( entity, efEntity );

                        if ( context.ChangeTracker.Entries().Any( e => e.State == EntityState.Added
                                                    || e.State == EntityState.Modified
                                                    || e.State == EntityState.Deleted ) == true )
                        {
                            //note: testing - the latter may be true if the child has changed - but shouldn't as the mapping only updates the parent
                            efEntity.LastUpdated = System.DateTime.Now;
                            efEntity.LastUpdatedById = entity.LastUpdatedById;
                            if ( efEntity.StatusId < CodesManager.ENTITY_STATUS_PUBLISHED )
                                efEntity.StatusId = CodesManager.ENTITY_STATUS_IN_PROGRESS;
                            count = context.SaveChanges();
                        }

                        //can be zero if no data changed
                        if ( count >= 0 )
                        {
                            //TODO - handle first time owner roles here????
                            isValid = true;

                            if ( !UpdateParts( entity, false, ref statusMessage ) )
                                isValid = false;

                            //update cache
                            if ( updatingCacheTables )
                                new CacheManager().PopulateCredentialRelatedCaches( entity.Id );
                        }
                        else
                        {
                            //?no info on error
                            statusMessage = "Error - the update was not successful. ";
                            string message = string.Format( "CredentialManager. Credential_Update Failed", "Attempted to update a credential. The process appeared to not work, but was not an exception, so we have no message, or no clue. Credential: {0}, Id: {1}, updatedById: {2}", entity.Name, entity.Id, entity.LastUpdatedById );
                            EmailManager.NotifyAdmin( "CredentialManager. Credential_Update Failed", message );
                        }
                    }
                    else
                    {
                        statusMessage = "Error - update failed, as record was not found.";
                    }
                }
                catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
                {
                    //LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
                    string message = thisClassName + string.Format( ".Credential_Update() DbEntityValidationException, Name: {0}", entity.Name );
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

                    statusMessage = string.Join( ", ", dbex.EntityValidationErrors.SelectMany( m => m.ValidationErrors.Select( n => n.ErrorMessage ) ).ToList() );
                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Credential_Update(), Name: {0}", entity.Name ) );
                    statusMessage = FormatExceptions( ex );
                }
            }

            return isValid;
        }

        public static bool ValidateProfile( ThisEntity profile, ref List<string> messages, bool validatingUrls = true )
        {
            bool isValid = true;
            int count = messages.Count;
            LoggingHelper.DoTrace( 7, string.Format( "{0}.ValidateProfile for {1}", thisClassName, profile.Name ?? "missing" ) );
            if ( string.IsNullOrWhiteSpace( profile.Name ) )
            {
                messages.Add( "A credential name must be entered" );
            }
            else if ( profile.Name.Length > 400 )
                messages.Add( "The credential name must be less than 400 characters." );


            //if ( !IsUrlWellFormed( profile.Url ) )
            //{
            //	messages.Add( "The value for Url is invalid" );
            //}
            if ( string.IsNullOrWhiteSpace( profile.SubjectWebpage ) )
                messages.Add( "A Subject Webpage name must be entered" );

            else if ( validatingUrls &&
                !IsUrlValid( profile.SubjectWebpage, ref commonStatusMessage, doingExistanceCheck ) )
            {
                messages.Add( "The Subject Webpage Url is invalid. " + commonStatusMessage );
            }

            if ( profile.CredentialType == null
            || profile.CredentialType.HasItems() == false 
			|| profile.CredentialType.Items[0].Id == 0)
                messages.Add( "A credential type must be selected." );

            if ( profile.IsReferenceVersion )
            {
                //no more edits
                //actually check description
                if ( FormHelper.HasHtmlTags( profile.Description ) )
                {
                    messages.Add( "HTML or Script Tags are not allowed in the description" );
                }
            }
            else
            {
                if ( profile.IsDescriptionRequired && string.IsNullOrWhiteSpace( profile.Description ) )
                {
                    messages.Add( "A description must be entered" );
                }
                else if ( FormHelper.HasHtmlTags( profile.Description ) )
                {
                    messages.Add( "HTML or Script Tags are not allowed in the description" );
                }
				else if ( profile.Description.Length < MinimumDescriptionLength && !IsDevEnv() )
				{
					messages.Add( string.Format("The Credential description must be at least {0} characters in length.", MinimumDescriptionLength) );
				}
				//need a check here for a reference type
				//will not be an owning org, will have to disguish from full cred though
				//Note there are also checks in the CredentialServices.

				if ( !IsGuidValid( profile.OwningAgentUid ) )
                {
                    //first determine if this is populated in edit mode
                    messages.Add( "An owning organization must be selected." );
                }

                if ( profile.CredentialStatusType == null
                    || profile.CredentialStatusType.HasItems() == false )
                    messages.Add( "A Credential Status must be selected." );
                if ( !string.IsNullOrWhiteSpace( profile.DateEffective ) && !IsValidDate( profile.DateEffective ) )
                {
                    messages.Add( "Please enter a valid effective date" );
                }
                //TODO - need to change to check for list
                //  - need to handle both until all code is cut over, including bulk import
                //if ( profile.InLanguageId < 1 )
                //{   }
                if ( !profile.InLanguageCodeList.Any() )
                    messages.Add( "A language must be selected." );

                //if ( profile.InLanguageCodeList == null || profile.InLanguageCodeList.Count == 0)
                //{
                //    messages.Add( "A language must be selected." );
                //}

                if ( validatingUrls )
                {
                    if ( !IsUrlValid( profile.AvailableOnlineAt, ref commonStatusMessage, doingExistanceCheck ) )
                    {
                        messages.Add( "The 'Available Online At' URL format is invalid. " + commonStatusMessage );
                    }

                    if ( !IsUrlValid( profile.AvailabilityListing, ref commonStatusMessage, doingExistanceCheck ) )
                    {
                        messages.Add( "The 'Availability Listing' Url is invalid. " + commonStatusMessage );
                    }
                    if ( !IsUrlValid( profile.ProcessStandards, ref commonStatusMessage, doingExistanceCheck ) )
                    {
                        messages.Add( "The 'Process Standards' URL format is invalid. " + commonStatusMessage );
                    }
                    if ( !IsUrlValid( profile.LatestVersionUrl, ref commonStatusMessage, doingExistanceCheck ) )
                    {
                        messages.Add( "The 'Latest Version' URL format is invalid. " + commonStatusMessage );
                    }
                    if ( !IsUrlValid( profile.PreviousVersion, ref commonStatusMessage, doingExistanceCheck ) )
                    {
                        messages.Add( "The 'Replaces Version' URL format is invalid. " + commonStatusMessage );
                    }
                    if ( !string.IsNullOrEmpty( profile.ImageUrl )
                        && !IsImageUrlValid( profile.ImageUrl, ref commonStatusMessage, doingExistanceCheck ) )
                    {
                        messages.Add( "The Image Url is invalid. " + commonStatusMessage );
                    }
                }

            }


            if ( messages.Count > count )
                isValid = false;

            return isValid;
        }

        /// <summary>
        /// Update credential registry id, and set status published
        /// </summary>
        /// <param name="credentialId"></param>
        /// <param name="envelopeId"></param>
        /// <param name="userId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool UpdateEnvelopeId( int credentialId, string envelopeId, int userId, ref string statusMessage )
        {
            bool isValid = false;
            int count = 0;
            bool updatingStatus = UtilityManager.GetAppKeyValue( "onRegisterSetEntityToPublic", false );
            using ( var context = new EntityContext() )
            {
                try
                {
                    context.Configuration.LazyLoadingEnabled = false;

                    EM.Credential efEntity = context.Credential
                                    .SingleOrDefault( s => s.Id == credentialId );

                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        efEntity.CredentialRegistryId = envelopeId;
                        if ( updatingStatus )
                            efEntity.StatusId = CodesManager.ENTITY_STATUS_PUBLISHED;

                        if ( HasStateChanged( context ) )
                        {
                            //don't set updated for this action
                            //efEntity.LastUpdated = System.DateTime.Now;
                            //efEntity.LastUpdatedById = userId;

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
                                string message = string.Format( thisClassName + ". UpdateEnvelopeId Failed", "Attempted to update an EnvelopeId. The process appeared to not work, but was not an exception, so we have no message, or no clue. Credential: {0}, envelopeId: {1}, updatedById: {2}", credentialId, envelopeId, userId );
                                EmailManager.NotifyAdmin( thisClassName + ".UpdateEnvelopeId Failed", message );
                            }
                        }


                    }
                    else
                    {
                        statusMessage = "Error - update failed, as record was not found.";
                    }
                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".UpdateEnvelopeId(), Credential: {0}, envelopeId: {1}, updatedById: {2}", credentialId, envelopeId, userId ) );
                    statusMessage = FormatExceptions( ex );
                }
            }

            return isValid;
        }

        /// <summary>
        /// Reset credential registry id, and set status to in process
        /// </summary>
        /// <param name="credentialId"></param>
        /// <param name="userId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool UnPublish( int credentialId, int userId, ref string statusMessage )
        {
            bool isValid = false;
            int count = 0;
            using ( var context = new EntityContext() )
            {
                EM.Credential efEntity = new EM.Credential();
                try
                {
                    context.Configuration.LazyLoadingEnabled = false;

                    efEntity = context.Credential
                                    .SingleOrDefault( s => s.Id == credentialId );

                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        efEntity.CredentialRegistryId = null;
                        //may not know reason for unpublish
                        efEntity.StatusId = CodesManager.ENTITY_STATUS_IN_PROGRESS;

                        if ( HasStateChanged( context ) )
                        {
                            efEntity.LastUpdated = System.DateTime.Now;
                            efEntity.LastUpdatedById = userId;

                            count = context.SaveChanges();
                        }

                        //can be zero if no data changed
                        if ( count >= 0 )
                        {
                            isValid = true;
                        }
                        else
                        {
                            //?no info on error
                            statusMessage = "Error - the update was not successful. ";
                            string message = string.Format( thisClassName + ".UnPublish Failed", "Attempted to unpublish the credential. The process appeared to not work, but was not an exception, so we have no message, or no clue. Credential: {0}, updatedById: {1}", credentialId, userId );
                            EmailManager.NotifyAdmin( thisClassName + ".UnPublish Failed", message );
                        }
                    }
                    else
                    {
                        statusMessage = "Error - update failed, as record was not found.";
                    }
                }
                catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
                {
                    string message = HandleDBValidationError( dbex, thisClassName + ".)() ", efEntity.Name );
                    statusMessage = "Error - the unpublish was not successful. " + message;

                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".UnPublish(), Credential: {0}, updatedById: {1}", credentialId, userId ) );
                    statusMessage = FormatExceptions( ex );
                }
            }

            return isValid;
        }
        public bool UpdateParts( MC.Credential entity, bool isAdd, ref string statusMessage )
        {

            bool isAllValid = true;
            statusMessage = "";
            List<string> messages = new List<string>();

            if ( UpdateProperties( entity, ref messages ) == false )
            {
                isAllValid = false;
            }

            //if a reference, then only the credential type is pertinent here
            if ( entity.IsReferenceVersion )
                return true;

            Entity_ReferenceManager erm = new Entity_ReferenceManager();
            LoggingHelper.DoTrace( 9, string.Format( "{0}.UpdateParts-Subjects Id: {1}", thisClassName, entity.Id ) );
            if ( erm.Update( entity.Subject, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_SUBJECT, false, SkippingUpdateOfEntityLastUpdated, SkippingIsDuplicateMessage ) == false )
                isAllValid = false;

            if ( erm.Update( entity.Keyword, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_KEYWORD, false, SkippingUpdateOfEntityLastUpdated, SkippingIsDuplicateMessage ) == false )
                isAllValid = false;

            if ( erm.Update( entity.AlternativeIndustries, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_NAICS, false, SkippingUpdateOfEntityLastUpdated, SkippingIsDuplicateMessage ) == false )
                isAllValid = false;

			if ( erm.Update( entity.AlternativeOccupations, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_SOC, false, SkippingUpdateOfEntityLastUpdated, SkippingIsDuplicateMessage ) == false )
				isAllValid = false;

			if ( erm.Update( entity.AlternativeInstructionalProgramType, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_CIP, false, SkippingUpdateOfEntityLastUpdated, SkippingIsDuplicateMessage ) == false )
				isAllValid = false;

			if ( erm.Update( entity.DegreeConcentration, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_DEGREE_CONCENTRATION, false, SkippingUpdateOfEntityLastUpdated, SkippingIsDuplicateMessage ) == false )
                isAllValid = false;

            if ( erm.Update( entity.DegreeMajor, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_DEGREE_MAJOR, false, SkippingUpdateOfEntityLastUpdated, SkippingIsDuplicateMessage ) == false )
                isAllValid = false;

            if ( erm.Update( entity.DegreeMinor, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_DEGREE_MINOR, false, SkippingUpdateOfEntityLastUpdated, SkippingIsDuplicateMessage ) == false )
                isAllValid = false;

			var lm = new Entity_LanguageManager();
            if ( lm.Update( entity.InLanguageCodeList, entity.RowId, entity.LastUpdatedById, ref messages ) == false )
                isAllValid = false;

            LoggingHelper.DoTrace( 9, string.Format( "{0}.UpdateParts-Roles Id: {1}", thisClassName, entity.Id ) );

            //note this may only be necessary on add, if we use the agent role popup for updates!
            //17-02-08 mparsons - will always do, as now updatable from page
            //need to prevent removing owner

            if ( isAdd || ( entity.OwnerRoles != null && entity.OwnerRoles.Items.Count > 0 ) )
            {
                if ( entity.OwnerRoles == null || entity.OwnerRoles.Items.Count == 0 )
                {
                    messages.Add( "Invalid request, please select one or more roles for the owning agent." );
                    isAllValid = false;
                }
                //the owner role must be selected
                else if ( entity.OwnerRoles.GetFirstItemId() != Entity_AgentRelationshipManager.ROLE_TYPE_OWNER )
                {
                    messages.Add( "Invalid request. The role \"Owned By\" must be one of the roles selected." );
                    isAllValid = false;
                }
                else
                {
                    OrganizationRoleProfile profile = new OrganizationRoleProfile();
                    profile.ParentUid = entity.RowId;
                    profile.ActingAgentUid = entity.OwningAgentUid;
                    profile.AgentRole = entity.OwnerRoles;
                    profile.CreatedById = entity.LastUpdatedById;
                    profile.LastUpdatedById = entity.LastUpdatedById;

                    if ( !new Entity_AgentRelationshipManager().Agent_EntityRoles_Save( profile, Entity_AgentRelationshipManager.VALID_ROLES_OWNER, entity.LastUpdatedById, ref messages ) )
                        isAllValid = false;
                }
            }

            statusMessage = string.Join( "<br/>", messages.ToArray() );
            return isAllValid;
        }
        public bool UpdateProperties( ThisEntity entity, ref List<string> messages )
        {
            bool isAllValid = true;
            LoggingHelper.DoTrace( 9, string.Format( "{0}.UpdateParts-UpdateProperties Id: {1}", thisClassName, entity.Id ) );
            //============================
            EntityPropertyManager mgr = new EntityPropertyManager();

            if ( mgr.UpdateProperties( entity.CredentialType, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE, entity.LastUpdatedById, ref messages ) == false )
                isAllValid = false;

            if ( mgr.UpdateProperties( entity.AudienceLevelType, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, CodesManager.PROPERTY_CATEGORY_AUDIENCE_LEVEL, entity.LastUpdatedById, ref messages ) == false )
                isAllValid = false;

            if ( mgr.UpdateProperties( entity.AudienceType, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE, entity.LastUpdatedById, ref messages ) == false )
                isAllValid = false;

            if ( mgr.UpdateProperties( entity.CredentialStatusType, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_STATUS_TYPE, entity.LastUpdatedById, ref messages ) == false )
                isAllValid = false;

			if ( mgr.UpdateProperties( entity.AssessmentDeliveryType, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, CodesManager.PROPERTY_CATEGORY_ASMT_DELIVERY_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			if ( mgr.UpdateProperties( entity.LearningDeliveryType, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, CodesManager.PROPERTY_CATEGORY_DELIVERY_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			return isAllValid;
        }

        /// <summary>
        /// Delete a credential
        /// 16-04-27 mparsons - changed to a virual delete
        /// </summary>
        /// <param name="id"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( int id, int userId, ref string statusMessage )
        {
            bool isValid = false;
            if ( id == 0 )
            {
                statusMessage = "Error - missing an identifier for the Credential";
                return false;
            }
            using ( var context = new EntityContext() )
            {
                EM.Credential efEntity = context.Credential
                            .SingleOrDefault( s => s.Id == id );

                if ( efEntity != null && efEntity.Id > 0 )
                {
                    statusMessage = string.Format( "Credential: {0}, Id:{1}", efEntity.Name, efEntity.Id );

                    //context.Credential.Remove( efEntity );
                    efEntity.LastUpdated = System.DateTime.Now;
                    efEntity.LastUpdatedById = userId;
                    efEntity.StatusId = CodesManager.ENTITY_STATUS_DELETED;

                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        isValid = true;
                    }
                    else
                        statusMessage = "Error - delete failed, but no message was provided.";
                }
                else
                {
                    statusMessage = "Error - delete failed, as record was not found.";
                }
            }

            return isValid;
        }

        public bool DeleteAllForOrganization( Guid owningOrgUid, ref List<string> messages )
        {
            bool isValid = true;
            MC.Organization org = OrganizationManager.GetForSummary( owningOrgUid );
            if ( org == null || org.Id == 0 )
            {
                messages.Add( "Error - the provided organization was not found." );
                return false;
            }
            if ( UtilityManager.GetAppKeyValue( "envType" ) == "production" )
            {
                messages.Add( "Deleting all credentials for an organization is not allowed in this environment." );
                return false;
            }
            string sql = "";
            try
            {
                using ( var context = new EntityContext() )
                {
                    context.Credential.RemoveRange( context.Credential.Where( s => s.OwningAgentUid == owningOrgUid ) );
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        isValid = true;
                        messages.Add( string.Format( "removed {0} credentials.", count ) );
                    }

                    //OR
                    //sql = string.Format( "DELETE FROM [dbo].[Credential] WHERE( OwningAgentUid = '{0}' )", owningOrgUid );
                    //context.Database.ExecuteSqlCommand( sql );


                    sql = string.Format( "DELETE FROM [dbo].[Credential.SummaryCache] WHERE( OwningAgentUid = '{0}' )", owningOrgUid );
                    context.Database.ExecuteSqlCommand( sql );

                    //sql = string.Format( "DELETE D FROM [dbo].[Cache.Organization_ActorRoles] D Inner Join Organization b on D.OrganizationId = b.Id WHERE b.RowId = '{0}'", owningOrgUid );
                    sql = string.Format( "DELETE FROM [dbo].[Cache.Organization_ActorRoles]   WHERE OrganizationId = {0} ", org.Id );
                    context.Database.ExecuteSqlCommand( sql );
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "DeleteAllForOrganization" );
                messages.Add( ex.Message );
            }
            return isValid;
        }

        #endregion

        #region credential - retrieval ===================
        public static MC.Credential GetForEdit( int id )
        {
            MC.Credential entity = new MC.Credential();
            CredentialRequest cr = new CredentialRequest();
            cr.IsEditRequest();

            using ( var context = new EntityContext() )
            {
                EM.Credential item = context.Credential
                            .SingleOrDefault( s => s.Id == id
                                && s.StatusId <= CodesManager.ENTITY_STATUS_EXTERNAL_REFERENCE
                                );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, cr );
                }
            }

            return entity;
        }
        /// <summary>
        /// Get credential and do validation for approvals
        /// </summary>
        /// <param name="id"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static ThisEntity GetForApproval( int id, ref List<string> messages )
        {
            ThisEntity entity = new ThisEntity();
            CredentialRequest cr = new CredentialRequest();
            //don't want edit version, unless attempting deep validation
            cr.IsCompareRequest();

            using ( var context = new EntityContext() )
            {
                EM.Credential item = context.Credential
                            .FirstOrDefault( s => s.Id == id
                                && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
                                );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, cr );
                    if ( ValidateProfile( entity, ref messages, false ) )
                    {

                    }
                }
            }

            return entity;
        }
        public static MC.Credential GetForCompare( int id, CredentialRequest cr )
        {
            MC.Credential entity = new MC.Credential();
            if ( id < 1 )
                return entity;
            using ( var context = new EntityContext() )
            {
                //context.Configuration.LazyLoadingEnabled = false;
                EM.Credential item = context.Credential
                            .SingleOrDefault( s => s.Id == id
                                && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
                                );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, cr );
                }
            }

            return entity;
        }
        /// <summary>
        /// Get a credential
        /// ?should we allow get on a 'deleted' cred? Most people wouldn't remember the Id, although could be from a report
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static MC.Credential GetBasic( int id, bool includingConditions = false )
        {

            MC.Credential entity = new MC.Credential();
            CredentialRequest cr = new CredentialRequest();
            cr.IsForProfileLinks = true;
            if ( id < 1 )
                return entity;

            using ( var context = new EntityContext() )
            {
                if ( cr.IsForProfileLinks )
                    context.Configuration.LazyLoadingEnabled = false;
                EM.Credential item = context.Credential
                            .SingleOrDefault( s => s.Id == id
                                && s.StatusId <= CodesManager.ENTITY_STATUS_EXTERNAL_REFERENCE
								);

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, cr );

                    //Other parts
                    if ( includingConditions )
                    {
                        Entity_ConditionProfileManager.FillConditionProfilesForList( entity, cr.IsForEditView );
                    }
                }
            }

            return entity;
        }
        public static MC.Credential GetBasic( Guid rowId, bool isForEditView, bool isForLink = false )
        {
            MC.Credential entity = new MC.Credential();
            CredentialRequest cr = new CredentialRequest();
            if ( isForEditView || isForLink )
                cr.IsForProfileLinks = true;
            else
            {
                cr.IncludingProperties = true;
                cr.IncludingSubjectsKeywords = true;
                cr.BubblingUpSubjects = true;
                cr.IncludingFrameworkItems = true;
            }

            cr.IsForEditView = isForEditView;

            using ( var context = new EntityContext() )
            {
                if ( cr.IsForProfileLinks ) //get minimum
                    context.Configuration.LazyLoadingEnabled = false;

                EM.Credential item = context.Credential
                            .FirstOrDefault( s => s.RowId == rowId
                                && s.StatusId <= CodesManager.ENTITY_STATUS_EXTERNAL_REFERENCE
								);

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, cr );
                }
            }

            return entity;
        }

		/// <summary>
		/// Get a basic credential by CTID
		/// </summary>
		/// <param name="ctid"></param>
		/// <param name="includingConditions"></param>
		/// <returns></returns>
        public static ThisEntity GetByCtid( string ctid, bool includingConditions = false )
        {

            MC.Credential entity = new MC.Credential();
            CredentialRequest cr = new CredentialRequest();
            cr.IsForProfileLinks = true;
            if ( string.IsNullOrWhiteSpace( ctid ) )
                return entity;

            using ( var context = new EntityContext() )
            {
                context.Configuration.LazyLoadingEnabled = false;
                EM.Credential item = context.Credential
                            .FirstOrDefault( s => s.CTID.ToLower() == ctid.ToLower()
                                && s.StatusId <= CodesManager.ENTITY_STATUS_EXTERNAL_REFERENCE
								);

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, cr );

                    //Other parts
                    if ( includingConditions )
                    {
                        Entity_ConditionProfileManager.FillConditionProfilesForList( entity, cr.IsForEditView );
                    }
                }
            }

            return entity;
        }
        public static ThisEntity GetBasicByUniqueId( string identifier, Guid owningOrgUid, bool includingConditions = false )
        {

            MC.Credential entity = new MC.Credential();
            CredentialRequest cr = new CredentialRequest();
            cr.IsForProfileLinks = true;
            if ( string.IsNullOrWhiteSpace( identifier ) )
                return entity;

            using ( var context = new EntityContext() )
            {
                context.Configuration.LazyLoadingEnabled = false;
                EM.Credential item = context.Credential
                            .FirstOrDefault( s => s.ExternalIdentifier.ToLower() == identifier.ToLower()
                                && s.OwningAgentUid == owningOrgUid
                                && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
                                );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, cr );

                    //Other parts
                    if ( includingConditions )
                    {
                        Entity_ConditionProfileManager.FillConditionProfilesForList( entity, cr.IsForEditView );
                    }
                }
            }

            return entity;
        }
        public static ThisEntity GetBasicWithConditions( Guid rowId )
        {
            MC.Credential entity = new MC.Credential();
            CredentialRequest cr = new CredentialRequest();
            cr.IsForProfileLinks = true;
            cr.IsForEditView = false;

            using ( var context = new EntityContext() )
            {
                if ( cr.IsForProfileLinks ) //get minimum
                    context.Configuration.LazyLoadingEnabled = false;

                EM.Credential item = context.Credential
                            .FirstOrDefault( s => s.RowId == rowId
                                && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
                                );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, cr );

                    Entity_ConditionProfileManager.FillConditionProfilesForList( entity, cr.IsForEditView );
                }
            }

            return entity;
        }

        public static List<ThisEntity> GetAllForOrg( Guid orgUid )
        {
            List<ThisEntity> list = new List<ThisEntity>();
            ThisEntity entity = new ThisEntity();
            CredentialRequest cr = new CredentialRequest();
            //set true to get minimum
            cr.IsForProfileLinks = true;
            cr.IsForEditView = false;

            using ( var context = new EntityContext() )
            {
                if ( cr.IsForProfileLinks ) //get minimum?????
                    context.Configuration.LazyLoadingEnabled = false;

                List<EM.Credential> results = context.Credential
                            .Where( s => s.OwningAgentUid == orgUid
                                && s.CTID.Length == 39
                                && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
                                ).ToList();

                if ( results != null && results.Count > 0 )
                {
                    foreach ( var item in results )
                    {
                        entity = new ThisEntity();
                        MapFromDB( item, entity, cr );
                        //already included
                        //entity.EntityApproval = Entity_ApprovalManager.GetByParent( entity.RowId );
                        if ( !string.IsNullOrWhiteSpace( entity.CredentialRegistryId ) )
                        {
                            SetLastPublishedData( entity );
                        }
                        //need to customize to only get asmts, lopps, and creds
                        CondProfileMgr.FillConditionProfilesForList( entity, cr.IsForEditView );
                        //need to make these as lite as possible
                        entity.CommonConditions = Entity_CommonConditionManager.GetAll( entity.RowId, cr.IsForEditView );

                        entity.CommonCosts = Entity_CommonCostManager.GetAll( entity.RowId, cr.IsForEditView );

                        list.Add( entity );
                    }
                }
            }

            return list;
        }

        public static void SetLastPublishedData( ThisEntity record )
        {
            string filter = string.Format( " [ActivityObjectId] = {0} AND [ActivityType] = 'Credential' AND Activity = 'Credential Registry'", record.Id );
            int totalRows = 0;
            List<SiteActivity> list = ActivityManager.Search( filter, "base.CreatedDate desc", 1, 1, ref totalRows );
            if ( list != null && list.Count > 0 )
            {
                SiteActivity item = list[ 0 ];
                record.LastPublished = item.Created;
                record.LastUpdatedById = ( int ) item.ActionByUserId;
            }

        }

        public static MC.Credential GetForDetail( int id, CredentialRequest cr )
        {
            MC.Credential entity = new MC.Credential();

            using ( var context = new EntityContext() )
            {

                //context.Configuration.LazyLoadingEnabled = false;
                EM.Credential item = context.Credential
                            .FirstOrDefault( s => s.Id == id
                                && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
                                );
                try
                {
                    if ( item != null && item.Id > 0 )
                    {
                        MapFromDB( item, entity, cr );
                        //get summary for some totals
						//TODO eliminate the use of cache here
                        EM.Credential_SummaryCache cache = GetSummary( item.Id );
                        if ( cache != null && cache.BadgeClaimsCount > 0 )
                            entity.HasVerificationType_Badge = true;
                    }
                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".GetForDetail(), Name: {0} ({1}", item.Name, item.Id ) );
                    entity.StatusMessage = FormatExceptions( ex );
                    //entity.Id = 0;
                }
            }

            return entity;
        }

        /// <summary>
        /// Get summary view of a credential
        /// Useful for accessing counts
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static EM.Credential_SummaryCache GetSummary( int id )
        {

            EM.Credential_SummaryCache item = new Data.Credential_SummaryCache();
            using ( var context = new EntityContext() )
            {

                item = context.Credential_SummaryCache
                            .SingleOrDefault( s => s.CredentialId == id );

                if ( item != null && item.CredentialId > 0 )
                {

                }
            }

            return item;
        }


        /// <summary>
        /// Credential Autocomplete
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="userId"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static List<string> Autocomplete( string pFilter, int pageNumber, int pageSize, int userId, ref int pTotalRows )
        {
            bool autocomplete = true;
            List<string> results = new List<string>();

            List<MC.CredentialSummary> list = Search( pFilter, "", pageNumber, pageSize, ref pTotalRows, userId, autocomplete );
            bool appendingOrgNameToAutocomplete = UtilityManager.GetAppKeyValue( "appendingOrgNameToAutocomplete", false );
            string prevName = "";
            foreach ( MC.CredentialSummary item in list )
            {
                //note excluding duplicates may have an impact on selected max terms
                if ( string.IsNullOrWhiteSpace( item.OwnerOrganizationName )
                    || !appendingOrgNameToAutocomplete )
                {
                    if ( item.Name.ToLower() != prevName )
                        results.Add( item.Name );
                }
                else
                {
                    results.Add( item.Name + " ('" + item.OwnerOrganizationName + "')" );
                }

                prevName = item.Name.ToLower();
            }
            return results;
        }


        public static List<MC.CredentialSummary> SearchByUrl( string subjectWebpage, string pOrderBy, int pageNumber, int pageSize, ref int pTotalRows, int userId = 0, bool autocomplete = false )
        {
            string url = NormalizeUrlData( subjectWebpage );
            //skip if an example url
            string filter = string.Format( " ( base.Id in (Select Id from Credential where (Url like '{0}%') )) ", url );
            int ptotalRows = 0;
            var exists = Search( filter, "", 1, 100, ref ptotalRows );
            return exists;
        }


        /// <summary>
        /// Credential search using proc
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="pTotalRows"></param>
        /// <param name="userId"></param>
        /// <param name="autocomplete"></param>
        /// <returns></returns>
        public static List<MC.CredentialSummary> Search( string pFilter, string pOrderBy, int pageNumber, int pageSize, ref int pTotalRows, int userId = 0, bool autocomplete = false )
        {
            string connectionString = DBConnectionRO();
            MC.CredentialSummary item = new MC.CredentialSummary();
            List<MC.CredentialSummary> list = new List<MC.CredentialSummary>();
            var result = new DataTable();
            string creatorOrgs = "";
            string owningOrgs = "";

            bool includingHasPartIsPartWithConnections = UtilityManager.GetAppKeyValue( "includeHasPartIsPartWithConnections", false );

            using ( SqlConnection c = new SqlConnection( connectionString ) )
            {
                c.Open();

                if ( string.IsNullOrEmpty( pFilter ) )
                {
                    pFilter = "";
                }

                using ( SqlCommand command = new SqlCommand( "[Credential.Search]", c ) )
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add( new SqlParameter( "@Filter", pFilter ) );
                    command.Parameters.Add( new SqlParameter( "@SortOrder", pOrderBy ) );
                    command.Parameters.Add( new SqlParameter( "@StartPageIndex", pageNumber ) );
                    command.Parameters.Add( new SqlParameter( "@PageSize", pageSize ) );
                    command.Parameters.Add( new SqlParameter( "@CurrentUserId", userId ) );

                    SqlParameter totalRows = new SqlParameter( "@TotalRows", pTotalRows );
                    totalRows.Direction = ParameterDirection.Output;
                    command.Parameters.Add( totalRows );


                    try
                    {
                        using ( SqlDataAdapter adapter = new SqlDataAdapter() )
                        {
                            adapter.SelectCommand = command;
                            adapter.Fill( result );
                        }
                        string rows = command.Parameters[ 5 ].Value.ToString();
                        pTotalRows = Int32.Parse( rows );
                        if ( pTotalRows > 0 && result.Rows.Count == 0)
                        {
                            item = new MC.CredentialSummary();
                            item.Name = "Error: invalid page number. Select displayed page button only. ";
                            item.Description = "Error: invalid page number. Select displayed page button only.";
                            item.CredentialTypeSchema = "error";
                            list.Add( item );
                            return list;
                        }
                    }
                    catch ( Exception ex )
                    {
                        pTotalRows = 0;
                        LoggingHelper.LogError( ex, thisClassName + string.Format( ".Search() - Execute proc, Message: {0} \r\n Filter: {1} \r\n", ex.Message, pFilter ) );

                        item = new MC.CredentialSummary();
                        item.Name = "Unexpected error encountered. System administration has been notified. Please try again later. ";
                        item.Description = ex.Message;
                        item.CredentialTypeSchema = "error";
                        list.Add( item );
                        return list;
                    }
                }

				//Used for costs. Only need to get these once. See below. - NA 5/12/2017
				//var currencies = CodesManager.GetCurrencies();
				//var costTypes = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_CREDENTIAL_ATTAINMENT_COST );
				int cntr = 0;
                try
                {
                    foreach ( DataRow dr in result.Rows )
                    {
						cntr++;
						if (cntr == 10)
						{

						}
                        //avgMinutes = 0;
                        item = new MC.CredentialSummary();
                        item.Id = GetRowColumn( dr, "Id", 0 );

                        //item.Name = GetRowColumn( dr, "Name", "missing" );
                        item.Name = dr[ "Name" ].ToString();
                        item.FriendlyName = FormatFriendlyTitle( item.Name );

                        item.OwnerOrganizationId = GetRowPossibleColumn( dr, "OwningOrganizationId", 0 );
                        item.OwnerOrganizationName = GetRowPossibleColumn( dr, "owningOrganization" );

                        //for autocomplete, only need name
                        if ( autocomplete )
                        {
                            list.Add( item );
                            continue;
                        }
                        //string rowId = GetRowColumn( dr, "RowId" );
                        string rowId = GetRowColumn( dr, "EntityUid" );
                        //string rowId = dr[ "EntityUid" ].ToString();
                        //if ( IsGuidValid( rowId ) )
                        item.RowId = new Guid( rowId );


                        //item.Description = GetRowColumn( dr, "Description", "" );
                        item.Description = dr[ "Description" ].ToString();
                        //item.Url = GetRowColumn( dr, "Url", "" );
                        item.SubjectWebpage = dr[ "Url" ].ToString();

                        item.CanEditRecord = GetRowColumn( dr, "CanEditRecord", false );

                        item.ManagingOrgId = GetRowPossibleColumn( dr, "ManagingOrgId", 0 );
                        item.ManagingOrganization = GetRowPossibleColumn( dr, "ManagingOrganization" );
                        item.OwningAgentUid = GetRowColumn( dr, "OwningAgentUid" );
                        //creatorOrgs = GetRowPossibleColumn( dr, "CreatorOrgs" );
                        creatorOrgs = dr[ "CreatorOrgs" ].ToString();

                        //owningOrgs = GetRowPossibleColumn( dr, "OwningOrgs" );
                        owningOrgs = dr[ "OwningOrgs" ].ToString();
                        DateTime testdate;
                        //=====================================
                        string date = GetRowPossibleColumn( dr, "EntityLastUpdated", "" );
                        if ( DateTime.TryParse( date, out testdate ) )
                            item.EntityLastUpdated = testdate;

                        //=====================================
                        item.CredentialRegistryId = dr[ "CredentialRegistryId" ].ToString();
                        item.LastPublishDate = GetRowPossibleColumn( dr, "LastPublishDate", "" );
                        if ( DateTime.TryParse( item.LastPublishDate, out testdate ) )
                        {
                            item.IsPublished = true;
                            item.LastPublished = testdate;
                        }
                        //approvals
                        item.LastApprovalDate = GetRowPossibleColumn( dr, "LastApprovalDate", "" );
                        if ( DateTime.TryParse( item.LastApprovalDate, out testdate ) )
                        {
                            item.IsApproved = true;
                            item.LastApproved = testdate;
                        }

                        item.ContentApprovedBy = GetRowPossibleColumn( dr, "ContentApprovedBy" );
                        item.ContentApprovedById = GetRowPossibleColumn( dr, "ContentApprovedById", 0 );
                        if ( item.ContentApprovedById > 0 )
                            item.IsApproved = true;

                        item.CTID = GetRowColumn( dr, "CTID" );
                        if ( string.IsNullOrWhiteSpace( item.CTID ) )
                            item.IsReferenceVersion = true;

                        date = GetRowColumn( dr, "EffectiveDate", "" );
                        if ( IsValidDate( date ) )
                            item.DateEffective = ( DateTime.Parse( date ).ToShortDateString() );
                        else
                            item.DateEffective = "";
                        date = GetRowColumn( dr, "Created", "" );
                        if ( IsValidDate( date ) )
                            item.Created = DateTime.Parse( date );
                        date = GetRowColumn( dr, "LastUpdated", "" );
                        if ( IsValidDate( date ) )
                            item.LastUpdated = DateTime.Parse( date );

                        //item.Version = GetRowPossibleColumn( dr, "Version", "" );
                        item.Version = dr[ "Version" ].ToString();
                        //item.LatestVersionUrl = GetRowPossibleColumn( dr, "LatestVersionUrl", "" );
                        item.LatestVersionUrl = dr[ "LatestVersionUrl" ].ToString();
                        //item.PreviousVersion = GetRowPossibleColumn( dr, "PreviousVersion", "" );
                        item.PreviousVersion = dr[ "ReplacesVersionUrl" ].ToString();
                        //item.AvailableOnlineAt = GetRowPossibleColumn( dr, "AvailableOnlineAt", "" );

                        //item.CredentialType = GetRowPossibleColumn( dr, "CredentialType", "" );
                        item.CredentialType = dr[ "CredentialType" ].ToString();
                        //item.CredentialTypeSchema = GetRowPossibleColumn( dr, "CredentialTypeSchema", "" );
                        item.CredentialTypeSchema = dr[ "CredentialTypeSchema" ].ToString();
                        item.TotalCost = GetRowPossibleColumn( dr, "TotalCost", 0m );

                        //AverageMinutes is a rough approach to sorting. If present, get the duration profiles
                        //if ( GetRowPossibleColumn( dr, "AverageMinutes", 0 ) > 0 )
                        // {
                        item.EstimatedTimeToEarn = DurationProfileManager.GetAll( item.RowId );
						// }
						
						item.IsAQACredential = GetRowColumn( dr, "IsAQACredential", false );
                        item.HasQualityAssurance = GetRowColumn( dr, "HasQualityAssurance", false );

                        item.LearningOppsCompetenciesCount = GetRowColumn( dr, "LearningOppsCompetenciesCount", 0 );
                        item.AssessmentsCompetenciesCount = GetRowColumn( dr, "AssessmentsCompetenciesCount", 0 );

                        item.QARolesCount = GetRowColumn( dr, "QARolesCount", 0 );

                        item.HasPartCount = GetRowColumn( dr, "HasPartCount", 0 );
                        item.IsPartOfCount = GetRowColumn( dr, "IsPartOfCount", 0 );
                        item.RequiresCount = GetRowColumn( dr, "RequiresCount", 0 );
                        item.RecommendsCount = GetRowColumn( dr, "RecommendsCount", 0 );
                        item.RequiredForCount = GetRowColumn( dr, "isRequiredForCount", 0 );
                        item.IsRecommendedForCount = GetRowColumn( dr, "IsRecommendedForCount", 0 );
                        item.RenewalCount = 0;// GetRowColumn( dr, "RenewalCount", 0 );
                        item.IsAdvancedStandingForCount = GetRowColumn( dr, "IsAdvancedStandingForCount", 0 );
                        item.AdvancedStandingFromCount = GetRowColumn( dr, "AdvancedStandingFromCount", 0 );
                        item.PreparationForCount = GetRowColumn( dr, "isPreparationForCount", 0 );
                        item.PreparationFromCount = GetRowColumn( dr, "isPreparationFromCount", 0 );

                        //NAICS CSV
                        //16-09-12 mp - changed to use pipe (|) rather than ; due to conflicts with actual embedded semicolons
                        item.IndustryResults = Fill_CodeItemResults( dr, "NaicsList", CodesManager.PROPERTY_CATEGORY_NAICS, true, true );
                        item.IndustryOtherResults = Fill_CodeItemResults( dr, "OtherIndustriesList", CodesManager.PROPERTY_CATEGORY_NAICS, false, false, false );

                        //OccupationsCSV
                        item.OccupationResults = Fill_CodeItemResults( dr, "OccupationsList", CodesManager.PROPERTY_CATEGORY_SOC, true, true );
                        item.OccupationOtherResults = Fill_CodeItemResults( dr, "OtherOccupationsList", CodesManager.PROPERTY_CATEGORY_SOC, false, false, false );
						
						item.InstructionalProgramCounts = GetRowColumn( dr, "InstructionalProgramCounts", 0 );
						if ( item.InstructionalProgramCounts > 0 )
						{
						}
						item.OtherInstructionalProgramCounts = GetRowColumn( dr, "OtherInstructionalProgramCounts", 0 );
						//education levels CSV
						//16-09-12 mp - changed to use pipe (|) rather than ; due to conflicts with actual embedded semicolons
						item.LevelsResults = Fill_CodeItemResults( dr, "LevelsList", CodesManager.PROPERTY_CATEGORY_AUDIENCE_LEVEL, false, false );

						item.AssessmentDeliveryType = Fill_CodeItemResultsFromXml( dr, "AssessmentDeliveryTypes", CodesManager.PROPERTY_CATEGORY_ASMT_DELIVERY_TYPE, false, false );
						item.LearningDeliveryType = Fill_CodeItemResultsFromXml( dr, "LearningDeliveryTypes", CodesManager.PROPERTY_CATEGORY_DELIVERY_TYPE, false, false );
						//count of QA on credential
						item.QARolesResults = Fill_CodeItemResults( dr, "QARolesList", CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE, false, true );
                        //actual orgs doing QA
                        item.AgentAndRoles = Fill_AgentRelationship( dr, "AgentAndRoles", CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE, false, false, true, "Credential" );

                        //this is just the count of the orgs by QA role
                        //set to different category
                        item.Org_QARolesResults = Fill_CodeItemResults( dr, "QAOrgRolesList", 130, false, true );
                        //this is QA on the owning org
                        //also set to different category
                        item.Org_QAAgentAndRoles = Fill_AgentRelationship( dr, "QAAgentAndRoles", 130, false, false, true, "Organization" );

                        //includes actual id and name for credential. ex:
                        //8~Is Preparation For~31~TESTING_Certified ISO/IEC 27005 Risk Manager~| 8~Is Preparation For~32~TESTING_Certified ISO 31000 Risk Manager~| 8~Is Preparation For~1179~TESTING_A Simple Cerificate Credential~| 6~Advanced Standing For~17~TESTING_Bachelor of Science in Computer Science~| 2~Recommends~31~TESTING_Certified ISO/IEC 27005 Risk Manager~| 1~Requires&#x0D; ~1209~TESTING_Master of Science in Project Management~| 9~Preparation From~1164~TESTING_CompTIA Security+~| 10~Corequisite~1179~TESTING_A Simple Cerificate Credential~
                        item.CredentialsList = Fill_CredentialConnectionsResult( dr, "CredentialsList", CodesManager.PROPERTY_CATEGORY_CONNECTION_PROFILE_TYPE );
                        //Count of each connection type. ex:
                        //1~Requires~ceterms:requires~1| 2~Recommends~ceterms:recommends~1| 6~Advanced Standing For~ceterms:isAdvancedStandingFor~1| 8~Is Preparation For~ceterms:isPreparationFor~2| 9~Preparation From~ceterms:isPreparationFrom~1| 10~Corequisite~ceterms:corequisite~1
                        //NOTE: ASSIGNED BUT NOT USED ANYWHERE!
                        item.ConnectionsList = Fill_CodeItemResults( dr, "ConnectionsList", CodesManager.PROPERTY_CATEGORY_CONNECTION_PROFILE_TYPE, true, true );
                        if ( includingHasPartIsPartWithConnections )
                        {
                            //manually add other connections
                            if ( item.HasPartCount > 0 )
                            {
                                item.ConnectionsList.Results.Add( new CodeItem() { Id = 0, Title = "Includes", SchemaName = "hasPart", Totals = item.HasPartCount } );
                            }
                            if ( item.IsPartOfCount > 0 )
                            {
                                item.ConnectionsList.Results.Add( new CodeItem() { Id = 0, Title = "Included With", SchemaName = "isPartOf", Totals = item.IsPartOfCount } );
                            }
                        }

                        item.HasPartsList = Fill_CredentialConnectionsResult( dr, "HasPartsList", CodesManager.PROPERTY_CATEGORY_CONNECTION_PROFILE_TYPE );

                        item.IsPartOfList = Fill_CredentialConnectionsResult( dr, "IsPartOfList", CodesManager.PROPERTY_CATEGORY_CONNECTION_PROFILE_TYPE );



                        //
                        item.ListTitle = item.Name + " (" + item.OwnerOrganizationName + ")";

                        string subjects = dr[ "SubjectsList" ].ToString();//GetRowPossibleColumn( dr, "", "" );

                        if ( !string.IsNullOrWhiteSpace( subjects ) )
                        {
                            var codeGroup = subjects.Split( '|' );
                            foreach ( string codeSet in codeGroup )
                            {
                                var codes = codeSet.Split( '~' );
                                item.Subjects.Add( codes[ 0 ].Trim() );
                            }
                        }

                        //addressess
                        int addressess = GetRowPossibleColumn( dr, "AvailableAddresses", 0 );
                        if ( addressess > 0 )
                        {
                            item.Addresses = AddressProfileManager.GetAll( item.RowId );
                        }

                        //Edit - Estimated Costs - needed for gray buttons in search results. Copied from MapFromDB method, then edited to move database calls outside of foreach loop. - NA 5/12/2017
                        //this only gets for the credential, need to alter to get all - should change to an ajax call
                        /*
						 * - cred
						 *		- conditions
						 *			- asmts
						 *				costs
						 *			- lopp
						 *				costs
						 */

                        item.NumberOfCostProfileItems = GetRowColumn( dr, "NumberOfCostProfileItems", 0 );

                        //item.EstimatedCost = CostProfileManager.GetAll( item.RowId, false );
                        //foreach ( var cost in item.EstimatedCost )
                        //{
                        //	cost.CurrencyTypes = currencies;
                        //	foreach ( var costItem in cost.Items )
                        //	{
                        //		costItem.CostType.Items.Add( costTypes.Items.FirstOrDefault( m => m.CodeId == costItem.CostTypeId ) );
                        //	}
                        //}
                        //End edit
                        //badgeClaimsCount
                        if ( GetRowPossibleColumn( dr, "badgeClaimsCount", 0 ) > 0 )
                        {
                            //Edit - Has Badge Verification Service.  Needed in search results. - NA 6/1/2017
                            item.HasVerificationType_Badge = true;  //Update this with appropriate source data
                        }

                        string relatedItems = GetRowColumn( dr, "TargetAssessments" );
                        string[] array = relatedItems.Split( ',' );
                        if ( array.Count() > 0 )
                            foreach ( var i in array )
                            {
                                if ( !string.IsNullOrWhiteSpace( i ) )
                                    item.TargetAssessmentsList.Add( i.ToLower() );
                            }
                        relatedItems = GetRowColumn( dr, "TargetLearningOpps" );
                        array = relatedItems.Split( ',' );
                        if ( array.Count() > 0 )
                            foreach ( var i in array )
                            {
                                if ( !string.IsNullOrWhiteSpace( i ) )
                                    item.TargetLearningOppsList.Add( i.ToLower() );
                            }
                        relatedItems = GetRowColumn( dr, "TargetCredentials" );
                        array = relatedItems.Split( ',' );
                        if ( array.Count() > 0 )
                            foreach ( var i in array )
                            {
                                if ( !string.IsNullOrWhiteSpace( i ) )
                                    item.TargetCredentialsList.Add( i.ToLower() );
                            }
                        relatedItems = GetRowColumn( dr, "CommonCosts" );
                        array = relatedItems.Split( ',' );
                        if ( array.Count() > 0 )
                            foreach ( var i in array )
                            {
                                if ( !string.IsNullOrWhiteSpace( i ) )
                                    item.CommonCostsList.Add( i.ToLower() );
                            }
                        relatedItems = GetRowColumn( dr, "CommonConditions" );
                        array = relatedItems.Split( ',' );
                        if ( array.Count() > 0 )
                            foreach ( var i in array )
                            {
                                if ( !string.IsNullOrWhiteSpace( i ) )
                                    item.CommonConditionsList.Add( i.ToLower() );
                            }
                        list.Add( item );
                    }

                    return list;
                }
                catch ( Exception ex )
                {
                    pTotalRows = 0;
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Search() - Execute proc, Message: {0} \r\n Filter: {1} \r\n", pFilter ) );

                    item = new MC.CredentialSummary();
                    item.Name = "Unexpected error encountered. System administration has been notified. Please try again later. ";
                    item.Description = ex.Message;
                    item.CredentialTypeSchema = "error";
                    list.Add( item );
                    return list;
                }
            }
        }

        public static List<Dictionary<string, object>> GetAllForExport_DictionaryList( string owningOrgUid, bool includingConditionProfile = true )
        {
            //
            var result = new List<Dictionary<string, object>>();
            var table = GetAllForExport_DataTable( owningOrgUid, includingConditionProfile );

            foreach ( DataRow dr in table.Rows )
            {
                var rowData = new Dictionary<string, object>();
                for ( var i = 0; i < dr.ItemArray.Count(); i++ )
                {
                    rowData[ table.Columns[ i ].ColumnName ] = dr.ItemArray[ i ];
                }
                result.Add( rowData );
            }
            return result;
        }
        //
        public static DataTable GetAllForExport_DataTable( string owningOrgUid, bool includingConditionProfile )
        {
            var result = new DataTable();
            string connectionString = DBConnectionRO();
            //
            using ( SqlConnection c = new SqlConnection( connectionString ) )
            {
                c.Open();
                using ( SqlCommand command = new SqlCommand( "[Credentials_Export]", c ) )
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add( new SqlParameter( "@OwningOrgUid", owningOrgUid ) );

                    try
                    {
                        using ( SqlDataAdapter adapter = new SqlDataAdapter() )
                        {
                            adapter.SelectCommand = command;
                            adapter.Fill( result );
                        }

                    }
                    catch ( Exception ex )
                    {
                        LoggingHelper.LogError( ex, thisClassName + string.Format( ".GetCredentialsForExport_DataTable() - Execute proc, Message: {0} \r\n owningOrgUid: {1} ", ex.Message, owningOrgUid ) );
                    }
                }
            }
            return result;
        }
        public static ThisEntity GetByNameAndUrl( string name, string url, ref string status )
        {
            CredentialRequest cr = new CredentialRequest();
            cr.IsForProfileLinks = true;
            ThisEntity to = new ThisEntity();
            //warning the trailing slash is trimmed during save so need to handle, or do both

            if ( url.EndsWith( "/" ) )
                url = url.TrimEnd( '/' );

            using ( var context = new Data.CTIEntities() )
            {
                context.Configuration.LazyLoadingEnabled = false;
                //can be many, so use list and reject if multiple
                List<DBEntity> results = context.Credential
                        .Where( s => s.Name.ToLower() == name.ToLower()
                        && ( s.Url.ToLower() == url.ToLower() )
                        && s.StatusId <= CodesManager.ENTITY_STATUS_EXTERNAL_REFERENCE
                        ).ToList();

                if ( results != null )
                {
                    if ( results.Count == 1 )
                        MapFromDB( results[ 0 ], to, cr );
                    else if ( results.Count > 1 )
                        status = "Error - there are mulitple credentials with the name: " + name + ". Please ensure a unique credential name is used, or use a CTID for an existing credential instead.";
                }
            }

            return to;
        }
        public static List<CredentialExport> GetCredentialsForExport( string owningOrgUid, bool includingConditionProfile )
        {
            List<CredentialExport> list = new List<CredentialExport>();
            CredentialExport item = new CredentialExport();
            var result = GetAllForExport_DataTable( owningOrgUid, includingConditionProfile );
            var result2 = new List<Dictionary<string, object>>();
            foreach ( DataRow dr in result.Rows )
            {
                var rowData = new Dictionary<string, object>();
                for ( var i = 0; i < dr.ItemArray.Count(); i++ )
                {
                    rowData[ result.Columns[ i ].ColumnName ] = dr.ItemArray[ i ];
                }
                result2.Add( rowData );
                item = new CredentialExport();

                item.UniqueIdentifier = GetRowColumn( dr, "Unique Identifier", "" );
                item.CTID = GetRowColumn( dr, "CTID", "" );
                item.Name = GetRowColumn( dr, "Credential Name", "" );
                item.OwnedBy = GetRowColumn( dr, "Owned By", "" );
                item.Description = GetRowColumn( dr, "Description", "" );

                item.CredentialType = GetRowColumn( dr, "Credential Type", "" );
                item.CredentialStatus = GetRowColumn( dr, "CredentialStatus", "" );
                item.SubjectWebpage = GetRowColumn( dr, "Webpage", "" );

                string conditions = GetRowPossibleColumn( dr, "CredentialConditions" );
                if ( !string.IsNullOrWhiteSpace( conditions ) )
                {
                    var xDoc = XDocument.Parse( conditions );
                    //Actually handling one for now
                    foreach ( var child in xDoc.Root.Elements() )
                    {
                        string agentName = ( string ) child.Attribute( "AgentName" ) ?? "";
                        string ConditionType = ( string ) child.Attribute( "ConditionType" ) ?? "";

                        item.ConditionIdentifier = child.Attribute( "Identifier" ).ToString();
                        item.ConditionType = ( string ) child.Attribute( "ConditionType" ) ?? "";
                        item.Description = ( string ) child.Attribute( "Description" ) ?? "";
                        item.SubjectWebpage = ( string ) child.Attribute( "SubjectWebpage" ) ?? "";
                        item.Condition_Experience = ( string ) child.Attribute( "Experience" ) ?? "";
                        item.Condition_YearsOfExperience = ( string ) child.Attribute( "YearsOfExperience" ) ?? "";
                        item.Condition_SubmissionOfItems = ( string ) child.Attribute( "SubmissionOfItems" ) ?? "";
                        item.Condition_ConditionItems = ( string ) child.Attribute( "ConditionItems" ) ?? "";

                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Expect
        /// - relationshipId (RoleId)
        /// - Relationship
        /// - AgentId
        /// - Agent Name
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="fieldName"></param>
        /// <param name="categoryId"></param>
        /// <param name="hasSchemaName"></param>
        /// <param name="hasTotals"></param>
        /// <param name="hasAnIdentifer"></param>
        /// <returns></returns>
        private static MC.AgentRelationshipResult Fill_ConditionExport( DataRow dr, string fieldName, int categoryId, bool hasSchemaName, bool hasTotals, bool hasAnIdentifer = true )
        {
            string list = dr[ fieldName ].ToString();
            MC.AgentRelationshipResult item = new MC.AgentRelationshipResult() { CategoryId = categoryId };
            item.HasAnIdentifer = hasAnIdentifer;
            AgentRelationship code = new AgentRelationship();
            int id = 0;

            if ( !string.IsNullOrWhiteSpace( list ) )
            {

                var codeGroup = list.Split( '|' );
                foreach ( string codeSet in codeGroup )
                {
                    code = new AgentRelationship();

                    var codes = codeSet.Split( '~' );
                    id = 0;
                    if ( hasAnIdentifer )
                    {
                        Int32.TryParse( codes[ 0 ].Trim(), out id );
                        code.RelationshipId = id;
                        code.Relationship = codes[ 1 ].Trim();

                        Int32.TryParse( codes[ 2 ].Trim(), out id );
                        code.AgentId = id;
                        code.Agent = codes[ 3 ].Trim();
                        if ( codes.Length > 5 )
                            code.AgentUrl = codes[ 5 ].Trim();
                        code.IsThirdPartyOrganization = "0";
                        if ( codes.Length > 6 )
                            code.IsThirdPartyOrganization = codes[ 6 ].Trim();
                        // code.CTID = codes[ 5 ].Trim();
                        //if ( !string.IsNullOrEmpty( entityType ) )
                        //{
                        //    code.EntityType = entityType.ToLower();
                        //    code.Relationship = entityType + " " + code.Relationship;
                        //}
                    }
                    else
                    {
                        //currently if no Id, assume only text value
                        //title = codes[ 0 ].Trim();
                    }
                    item.Results.Add( code );
                }
            }

            return item;
        }


        /// <summary>
        /// Expect
        /// - relationshipId (RoleId)
        /// - Relationship
        /// - AgentId
        /// - Agent Name
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="fieldName"></param>
        /// <param name="categoryId"></param>
        /// <param name="hasSchemaName"></param>
        /// <param name="hasTotals"></param>
        /// <param name="hasAnIdentifer"></param>
        /// <returns></returns>
        private static MC.AgentRelationshipResult Fill_AgentRelationship( DataRow dr, string fieldName, int categoryId, bool hasSchemaName, bool hasTotals, bool hasAnIdentifer = true, string entityType = "" )
        {
            string list = dr[ fieldName ].ToString();
            MC.AgentRelationshipResult item = new MC.AgentRelationshipResult() { CategoryId = categoryId };
            item.HasAnIdentifer = hasAnIdentifer;
            AgentRelationship code = new AgentRelationship();
            int id = 0;

            if ( !string.IsNullOrWhiteSpace( list ) )
            {

                var codeGroup = list.Split( '|' );
                foreach ( string codeSet in codeGroup )
                {
                    code = new AgentRelationship();

                    var codes = codeSet.Split( '~' );
                    id = 0;
                    if ( hasAnIdentifer )
                    {
                        Int32.TryParse( codes[ 0 ].Trim(), out id );
                        code.RelationshipId = id;
                        code.Relationship = codes[ 1 ].Trim();

                        Int32.TryParse( codes[ 2 ].Trim(), out id );
                        code.AgentId = id;
                        code.Agent = codes[ 3 ].Trim();
                        if ( codes.Length > 5 )
                            code.AgentUrl = codes[ 5 ].Trim();
                        code.IsThirdPartyOrganization = "0";
                        if ( codes.Length > 6 )
                            code.IsThirdPartyOrganization = codes[ 6 ].Trim();
                        // code.CTID = codes[ 5 ].Trim();
                        if ( !string.IsNullOrEmpty( entityType ) )
                        {
                            code.EntityType = entityType.ToLower();
                            code.Relationship = entityType + " " + code.Relationship;
                        }
                    }
                    else
                    {
                        //currently if no Id, assume only text value
                        //title = codes[ 0 ].Trim();
                    }
                    item.Results.Add( code );
                }
            }

            return item;
        }


        private static bool ExtractOrg( string data, ref int orgId, ref string orgName )
        {
            var org = data.Split( ',' );
            orgName = org[ 1 ].Trim();
            if ( Int32.TryParse( org[ 0 ].Trim(), out orgId ) )
                return true;
            else
                return false;


        }

        /// <summary>
        /// Search for credential assets.
        /// At this time the number would seem to be small, so not including paging
        /// </summary>
        /// <param name="credentialId"></param>
        /// <returns></returns>
        public static List<MC.Entity> CredentialAssetsSearch( int credentialId )
        {
            MC.Entity result = new MC.Entity();
            List<MC.Entity> list = new List<MC.Entity>();
            using ( var context = new ViewContext() )
            {
                List<Views.Credential_Assets> results = context.Credential_Assets
                    .Where( s => s.CredentialId == credentialId )
                    .ToList();

                if ( results != null && results.Count > 0 )
                {
                    foreach ( Views.Credential_Assets item in results )
                    {
                        result = new MC.Entity();
                        result.Id = item.AssetEntityId;
                        result.EntityBaseId = item.AssetId;
                        result.EntityUid = item.AssetEntityUid;
                        result.EntityTypeId = item.AssetTypeId;
                        result.EntityType = item.AssetType;
                        result.EntityBaseName = item.Name;

                        list.Add( result );
                    }

                }
            }

            return list;
        }
        public static List<CodeItem> CredentialAssetsSearch2( int credentialId )
        {
            CodeItem result = new CodeItem();
            List<CodeItem> list = new List<CodeItem>();
            using ( var context = new ViewContext() )
            {
                List<Views.Credential_Assets> results = context.Credential_Assets
                    .Where( s => s.CredentialId == credentialId )
                    .ToList();

                if ( results != null && results.Count > 0 )
                {
                    foreach ( Views.Credential_Assets item in results )
                    {
                        result = new CodeItem();
                        result.Id = item.AssetEntityId;
                        result.Title = item.AssetType + " - " + item.Name;

                        list.Add( result );
                    }

                }
            }

            return list;
        }
        /// <summary>
        /// Map properties from the database to the class
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="cr"></param>
        public static void MapFromDB( EM.Credential from, MC.Credential to,
                    CredentialRequest cr )
        {
            LoggingHelper.DoTrace( 7, thisClassName + string.Format( "MapFromDB.start: {0}, cr.IsForDetailView: {1}", cr.IsForEditView, cr.IsForDetailView ) );
            to.Id = from.Id;
            to.StatusId = from.StatusId ?? 1;
            to.RowId = from.RowId;

            to.Name = from.Name;
            //no point converting for display
            //to.Description = ConvertWordFluff( from.Description);
            to.Description = from.Description;
            to.SubjectWebpage = from.Url != null ? from.Url : "";

            to.ManagingOrgId = from.ManagingOrgId ?? 0;
            if ( IsValidDate( from.Created ) )
                to.Created = ( DateTime ) from.Created;
            to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
            if ( IsValidDate( from.LastUpdated ) )
                to.LastUpdated = ( DateTime ) from.LastUpdated;
            to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;
            to.LastUpdatedBy = SetLastUpdatedBy( to.LastUpdatedById, from.Account_Modifier );

            to.RelatedEntity = EntityManager.GetEntity( to.RowId, false );
            if ( to.RelatedEntity != null && to.RelatedEntity.Id > 0 )
                to.EntityLastUpdated = to.RelatedEntity.LastUpdated;

            // 16-06-15 mp - always include credential type
            to.CredentialType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE );

            to.CredentialTypeDisplay = to.CredentialType.GetFirstItem().Name;
            to.CredentialTypeSchema = to.CredentialType.GetFirstItem().SchemaName;
            to.ExternalIdentifier = from.ExternalIdentifier;

            to.ctid = from.CTID;
            if ( string.IsNullOrWhiteSpace( to.ctid ) )
            {
                to.IsReferenceVersion = true;
                return;
            }
            //===========================================================

            to.CredentialRegistryId = from.CredentialRegistryId;

            if ( from.ImageUrl != null && from.ImageUrl.Trim().Length > 0 )
                to.ImageUrl = from.ImageUrl;
            else
                to.ImageUrl = null;

            if ( IsGuidValid( from.OwningAgentUid ) )
            {
                to.OwningAgentUid = ( Guid ) from.OwningAgentUid;
                to.OwningOrganization = OrganizationManager.GetForSummary( to.OwningAgentUid, false );

                //get roles
                OrganizationRoleProfile orp = Entity_AgentRelationshipManager.AgentEntityRole_GetAsEnumerationFromCSV( to.RowId, to.OwningAgentUid );
                to.OwnerRoles = orp.AgentRole;
            }

            //
            to.OwningOrgDisplay = to.OrganizationName;

            to.AudienceLevelType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_AUDIENCE_LEVEL );
            to.AudienceType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE );


            if ( cr.IsForProfileLinks ) //return minimum ===========
                return;
            LoggingHelper.DoTrace( 7, thisClassName + string.Format( "MapFromDB.IsForProfileLinks..." ) );
            //===================================================================
            try
            {
                if ( !cr.IsForDetailView )
                {
                    to.EntityApproval = Entity_ApprovalManager.GetByParent( to.RowId );
                    if ( to.EntityApproval != null && to.EntityApproval.Id > 0 )
                        to.LastApproved = to.EntityApproval.Created;
                }
                if ( IsGuidValid( from.CopyrightHolder ) )
                {
                    to.CopyrightHolder = ( Guid ) from.CopyrightHolder;
                    //not sure if we need the org for display?
                    to.CopyrightHolderOrganization = OrganizationManager.GetForSummary( to.CopyrightHolder );
                }

                to.AlternateName = from.AlternateName;
                to.CredentialId = from.CredentialId;
                to.CodedNotation = from.CodedNotation;
                to.AvailabilityListing = from.AvailabilityListing;

                to.VersionIdentifier = from.Version;
                if ( IsValidDate( from.EffectiveDate ) )
                    to.DateEffective = ( ( DateTime ) from.EffectiveDate ).ToShortDateString();
                else
                    to.DateEffective = "";

                to.LatestVersionUrl = from.LatestVersionUrl;
                to.PreviousVersion = from.ReplacesVersionUrl;

                to.AvailableOnlineAt = from.AvailableOnlineAt;

                to.InLanguageCodeList = Entity_LanguageManager.GetAll( to.RowId );

                to.EarningCredentialPrimaryMethodId = from.EarningCredentialPrimaryMethodId ?? 0;
                to.FeatureLearningOpportunities = ( bool ) ( from.FeatureLearningOpportunities ?? false );
                to.FeatureAssessments = ( bool ) ( from.FeatureAssessments ?? false );

                to.ProcessStandards = from.ProcessStandards ?? "";
                to.ProcessStandardsDescription = from.ProcessStandardsDescription ?? "";
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".MapFromDB.A(), Name: {0} ({1}", to.Name, to.Id ) );
                to.StatusMessage = FormatExceptions( ex );
                //entity.Id = 0;
            }
            //properties ===========================================
            LoggingHelper.DoTrace( 7, thisClassName + string.Format( "MapFromDB.properties..." ) );
            try
            {
                if ( cr.IncludingProperties )
                {

                    if ( cr.IncludingEstimatedCosts )
                    {
                        to.EstimatedCosts = CostProfileManager.GetAll( to.RowId, cr.IsForEditView );

                        //Include currencies to fix null errors in various places (detail, compare, publishing) - NA 3/17/2017
                        var currencies = CodesManager.GetCurrencies();
                        //Include cost types to fix other null errors - NA 3/17/2017
                        var costTypes = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_CREDENTIAL_ATTAINMENT_COST );
                        foreach ( var cost in to.EstimatedCosts )
                        {
                            cost.CurrencyTypes = currencies;

                            foreach ( var costItem in cost.Items )
                            {
                                int index = costItem.CostType.Items.FindIndex( a => a.Id == costItem.CostTypeId );
                                if ( index < 0 )
                                    costItem.CostType.Items.Add( costTypes.Items.FirstOrDefault( m => m.CodeId == costItem.CostTypeId ) );
                            }
                        }
                        //End edits - NA 3/17/2017
                    }

                    to.CredentialStatusType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_STATUS_TYPE );
                    MC.EnumeratedItem statusItem = to.CredentialStatusType.GetFirstItem();
                    if ( statusItem != null && statusItem.Id > 0 && statusItem.Name != "Active" )
                    {
                        if ( cr.IsForDetailView )
                            to.Name += string.Format( " ({0})", statusItem.Name );
                    }

					to.AssessmentDeliveryType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_ASMT_DELIVERY_TYPE );

					to.LearningDeliveryType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_DELIVERY_TYPE );
				}
                //just in case
                if ( to.EstimatedCosts == null )
                    to.EstimatedCosts = new List<CostProfile>();

                //profiles ==========================================
                to.FinancialAssistance = Entity_FinancialAlignmentProfileManager.GetAll( to.RowId, cr.IsForEditView );

                if ( cr.IncludingAddesses )
                    to.Addresses = AddressProfileManager.GetAll( to.RowId );

                if ( cr.IncludingDuration )
                    to.EstimatedDuration = DurationProfileManager.GetAll( to.RowId );

                to.RenewalFrequency = DurationProfileManager.GetAll( to.RowId, 3 );

                if ( cr.IncludingFrameworkItems )
                {
                    to.Occupation = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_SOC );
                    to.AlternativeOccupations = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SOC );

                    to.Industry = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );
                    to.AlternativeIndustries = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );

					to.InstructionalProgramType = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CIP );

					to.AlternativeInstructionalProgramType = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_CIP );
				}
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".MapFromDB.B(), Name: {0} ({1}", to.Name, to.Id ) );
                to.StatusMessage = FormatExceptions( ex );
                //entity.Id = 0;
            }

            try
            {
                if ( cr.IncludingConnectionProfiles )
                {
                    //get all associated top level learning opps, and assessments
                    //will always be for profile lists - not expected any where else other than edit
                    LoggingHelper.DoTrace( 7, thisClassName + string.Format( "MapFromDB.IncludingConnectionProfiles..." ) );

                    //assessment
                    //for entity.condition(ec) - entity = ec.rowId
                    //actually, should these only be for edit view. For detail, they will be drawn from conditions!!!
                    if ( cr.IsForEditView )
                    {
                        //**** these are only used in edit view. Just need summary, no properties
                        to.TargetAssessment = Entity_AssessmentManager.GetAll( to.RowId, false, false, true );
                        foreach ( AssessmentProfile ap in to.TargetAssessment )
                        {
                            if ( ap.EstimatedCost != null && ap.EstimatedCost.Count > 0 )
                            {
                                to.AssessmentEstimatedCosts.AddRange( ap.EstimatedCost );
                                //to.EstimatedCosts.AddRange( ap.EstimatedCost );
                            }
                        }

                        to.TargetLearningOpportunity = Entity_LearningOpportunityManager.LearningOpps_GetAll( to.RowId, false, false, false, true );
                        foreach ( LearningOpportunityProfile lp in to.TargetLearningOpportunity )
                        {
                            if ( lp.EstimatedCost != null && lp.EstimatedCost.Count > 0 )
                            {
                                to.LearningOpportunityEstimatedCosts.AddRange( lp.EstimatedCost );
                                //to.EstimatedCosts.AddRange( lp.EstimatedCost );
                            }
                        }

                        //not sure if competencies are germain
                        foreach ( LearningOpportunityProfile e in to.TargetLearningOpportunity )
                        {
                            if ( e.HasCompetencies || e.ChildHasCompetencies )
                            {
                                to.ChildHasCompetencies = true;
                                break;
                            }
                        }
                    }

                    //TODO - need distinguish between embedded and those for a condition profile - maybe-defer use of this until certain
                    //to.TargetCredential = Entity_CredentialManager.GetAll( to.RowId );

                    //******************get all condition profiles *******************
                    //TODO - have custom version of this to only get minimum!!
                    //NOTE - the IsForEditView relates to cred, but probably don't want to sent true to the fill
                    //re: commonConditions - consider checking if any exist, and if not, don't show
                    if ( cr.IsForEditView )
                    {
                        CondProfileMgr.FillConditionProfilesForList( to, cr.IsForEditView );

                        to.CommonConditions = Entity_CommonConditionManager.GetAll( to.RowId, cr.IsForEditView );

                        to.CommonCosts = Entity_CommonCostManager.GetAll( to.RowId, cr.IsForEditView );
                    }
                    else
                    {
                        //need to ensure competencies are bubbled up

                        LoggingHelper.DoTrace( 7, thisClassName + string.Format( "MapFromDB.FillConditionProfilesForDetailDisplay..." ) );
                        if ( cr.IsForPublishRequest )
                        {
                            Entity_ConditionProfileManager.FillConditionProfilesForDetailDisplay( to, true );
                        }
                        else
                        {
                            Entity_ConditionProfileManager.FillConditionProfilesForDetailDisplay( to, false );
                        }
                        to.CommonConditions = Entity_CommonConditionManager.GetAll( to.RowId, true );
                        to.CommonCosts = Entity_CommonCostManager.GetAll( to.RowId, true );
                    }
                }

                if ( cr.IncludingRevocationProfiles )
                {
                    to.Revocation = Entity_RevocationProfileManager.GetAll( to.RowId );
                }

                if ( cr.IncludingJurisdiction )
                {
                    to.Jurisdiction = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId );
                    to.Region = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_RESIDENT );
                }
                LoggingHelper.DoTrace( 7, thisClassName + string.Format( "MapFromDB.ProcessProfiles..." ) );
                //TODO - CredentialProcess is used in the detail pages. Should be removed and use individual profiles
                //List<ProcessProfile>  credentialProcess =
                to.CredentialProcess = Entity_ProcessProfileManager.GetAll( to.RowId, cr.IsForEditView );
                foreach ( ProcessProfile item in to.CredentialProcess )
                {
                    if ( item.ProcessTypeId == Entity_ProcessProfileManager.ADMIN_PROCESS_TYPE )
                        to.AdministrationProcess.Add( item );
                    else if ( item.ProcessTypeId == Entity_ProcessProfileManager.DEV_PROCESS_TYPE )
                        to.DevelopmentProcess.Add( item );
                    else if ( item.ProcessTypeId == Entity_ProcessProfileManager.MTCE_PROCESS_TYPE )
                        to.MaintenanceProcess.Add( item );
                    else if ( item.ProcessTypeId == Entity_ProcessProfileManager.REVIEW_PROCESS_TYPE )
                        to.ReviewProcess.Add( item );
                    else if ( item.ProcessTypeId == Entity_ProcessProfileManager.REVOKE_PROCESS_TYPE )
                        to.RevocationProcess.Add( item );
                    else if ( item.ProcessTypeId == Entity_ProcessProfileManager.APPEAL_PROCESS_TYPE )
                        to.AppealProcess.Add( item );
                    else if ( item.ProcessTypeId == Entity_ProcessProfileManager.COMPLAINT_PROCESS_TYPE )
                        to.ComplaintProcess.Add( item );
                    else
                    {
                        //unexpected
                    }
                }

                if ( cr.IncludingEmbeddedCredentials )
                {
                    to.EmbeddedCredentials = Entity_CredentialManager.GetAll( to.RowId );
                }


                //populate is part of - when??
                if ( from.Entity_Credential != null && from.Entity_Credential.Count > 0 )
                {
                    foreach ( EM.Entity_Credential ec in from.Entity_Credential )
                    {
                        if ( ec.Entity != null )
                        {
                            //This method needs to be enhanced to get enumerations for the credential for display on the detail page - NA 6/2/2017
                            //Need to determine is when non-edit, is actually for the detail reference
                            //only get where parent is a credential, ex not a condition profile
                            if ( ec.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_CREDENTIAL )
                                to.IsPartOf.Add( GetBasic( ec.Entity.EntityUid, cr.IsForEditView, false ) );
                        }
                    }
                }

                if ( cr.IncludingSubjectsKeywords )
                {
                    if ( cr.BubblingUpSubjects )
                        to.Subject = Entity_ReferenceManager.GetAllSubjects( to.RowId );
                    else
                        to.Subject = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SUBJECT );

                    to.Keyword = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD );
                }
                to.DegreeConcentration = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_DEGREE_CONCENTRATION );
                to.DegreeMajor = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_DEGREE_MAJOR );
                to.DegreeMinor = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_DEGREE_MINOR );

                //---------------
                if ( cr.IncludingRolesAndActions )
                {
                    LoggingHelper.DoTrace( 7, thisClassName + string.Format( "MapFromDB.IncludingRolesAndActions..." ) );
                    to.JurisdictionAssertions = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_OFFERREDIN );

                    if ( cr.IsForEditView )
                    {
                        //get all except owned by, and offered by
                        to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAllExceptOwnerSummary( to.RowId, to.OwningAgentUid, true, false );
                        //to.OrganizationRole = Entity_AgentRelationshipManager.CredentialAssets_GetAllQARoles( to.Id );
                        //to.OfferedByOrganizationRole = Entity_AgentRelationshipManager.CredentialAssets_GetAllOfferedByRoles( to.Id );

                        to.OfferedByOrganization = Entity_AgentRelationshipManager.GetAllOfferingOrgs( to.RowId );
                        //get owner roles
                        to.OwnerOrganizationRoles = Entity_AgentRelationshipManager.AgentEntityRole_GetOwnerSummary( to.RowId, to.OwningAgentUid, false );

                    }
                    else
                    {
                        //get as ennumerations
                        to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAll_ToEnumeration( to.RowId, true );
                    }
					//to.QualityAssuranceAction = Entity_QualityAssuranceActionManager.QualityAssuranceActionProfile_GetAll( to.RowId );
				}
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".MapFromDB.C(), Name: {0} ({1}", to.Name, to.Id ) );
                to.StatusMessage = FormatExceptions( ex );
                //entity.Id = 0;
            }

        }

        public static void MapFromSummary( MC.CredentialSummary from, MC.Credential to )
        {
            to.Id = from.Id;
            to.StatusId = from.StatusId;
            to.RowId = from.RowId;

            to.Name = from.Name;
            to.Description = from.Description;

            to.SubjectWebpage = from.SubjectWebpage != null ? from.SubjectWebpage : "";

            to.ManagingOrgId = from.ManagingOrgId;
            if ( IsValidDate( from.Created ) )
                to.Created = ( DateTime ) from.Created;
            to.CreatedById = from.CreatedById;
            if ( IsValidDate( from.LastUpdated ) )
                to.LastUpdated = ( DateTime ) from.LastUpdated;
            to.LastUpdatedById = from.LastUpdatedById;

            to.ctid = from.CTID;
            if ( string.IsNullOrWhiteSpace( to.ctid ) )
            {
                to.IsReferenceVersion = true;
                return;
            }
            //===========================================================

            to.CredentialRegistryId = from.CredentialRegistryId;
            to.CredentialType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE );


            if ( from.OwnerOrganizationId > 0 )
            {

                to.OwningOrganization = OrganizationManager.GetForSummary( from.OwnerOrganizationId );
                to.OwningAgentUid = to.OwningOrganization.RowId;
                //get roles
                OrganizationRoleProfile orp = Entity_AgentRelationshipManager.AgentEntityRole_GetAsEnumerationFromCSV( to.RowId, to.OwningAgentUid );
                to.OwnerRoles = orp.AgentRole;
            }

            //to.CredentialTypeDisplay = to.CredentialType.GetFirstItem().Name;
            //to.CredentialTypeSchema = to.CredentialType.GetFirstItem().SchemaName;
            ////
            //to.OwningOrgDisplay = to.OwningOrganization.Name;

            //to.AudienceLevelType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_AUDIENCE_LEVEL );

        }

        private static void MapToDB( MC.Credential from, EM.Credential to )
        {
            to.Id = from.Id;
            if ( to.Id < 1 )
            {

                //will need to be carefull here, will this exist in the input??
				//there could be a case where an external Id was added to bulk upload for an existing record
                to.ExternalIdentifier = string.IsNullOrWhiteSpace( from.ExternalIdentifier ) ? null : from.ExternalIdentifier;
            } else
			{
				if ( string.IsNullOrWhiteSpace( to.ExternalIdentifier) )
				{
					to.ExternalIdentifier = string.IsNullOrWhiteSpace( from.ExternalIdentifier ) ? null : from.ExternalIdentifier;
				}
			}
            //don't map rowId, ctid, or dates as not on form
            //to.RowId = from.RowId;

            to.Name = ConvertSpecialCharacters( from.Name );
            to.Description = ConvertSpecialCharacters( from.Description );
            to.Url = NormalizeUrlData( from.SubjectWebpage, null );

            //generally the managing orgId should not be allowed to change in the interface - yet
            if ( from.ManagingOrgId > 0
                && from.ManagingOrgId != ( to.ManagingOrgId ?? 0 ) )
                to.ManagingOrgId = from.ManagingOrgId;


            if ( from.IsReferenceVersion )
                return;

            to.AlternateName = GetData( from.AlternateName );
            //to.CTID = from.ctid;
            to.CredentialId = string.IsNullOrWhiteSpace( from.CredentialId ) ? null : from.CredentialId;
            to.CodedNotation = GetData( from.CodedNotation );


            //handle old version setting to zero
            if ( IsGuidValid( from.OwningAgentUid ) )
            {
                if ( to.Id > 0 && to.OwningAgentUid != from.OwningAgentUid )
                {
                    if ( IsGuidValid( to.OwningAgentUid ) )
                    {
                        //need to remove the owner role, or could have been others
                        string statusMessage = "";
                        new Entity_AgentRelationshipManager().Delete( to.RowId, to.OwningAgentUid, Entity_AgentRelationshipManager.ROLE_TYPE_OWNER, ref statusMessage );
                    }
                }
                to.OwningAgentUid = from.OwningAgentUid;
            }
            else
            {
                //always have to have an owner
                //to.OwningAgentUid = null;
            }

            if ( from.OwnerOrganizationRoles != null && from.OwnerOrganizationRoles.Count > 0 )
            {
                //may need to do something in case was a change via the roles popup
            }

            to.Version = GetData( from.VersionIdentifier );
            if ( IsValidDate( from.DateEffective ) )
                to.EffectiveDate = DateTime.Parse( from.DateEffective );
            else //handle reset
                to.EffectiveDate = null;



            to.LatestVersionUrl = NormalizeUrlData( from.LatestVersionUrl, null );
            to.ReplacesVersionUrl = NormalizeUrlData( from.PreviousVersion, null );
            to.AvailabilityListing = NormalizeUrlData( from.AvailabilityListing, null );
            to.AvailableOnlineAt = NormalizeUrlData( from.AvailableOnlineAt, null );
            to.ImageUrl = NormalizeUrlData( from.ImageUrl, null );
            //if ( from.InLanguageId > 0 )
            //    to.InLanguageId = from.InLanguageId;
            //else
            //    to.InLanguageId = null;

            to.FeatureLearningOpportunities = from.FeatureLearningOpportunities;
            to.FeatureAssessments = from.FeatureAssessments;

            to.ProcessStandards = NormalizeUrlData( from.ProcessStandards, null );
            to.ProcessStandardsDescription = ConvertSpecialCharacters( from.ProcessStandardsDescription );

            if ( IsGuidValid( from.CopyrightHolder ) )
                to.CopyrightHolder = from.CopyrightHolder;
            else
                to.CopyrightHolder = null;

        } //

        #endregion

    }
    public class CredentialRequest
    {
        public CredentialRequest()
        {
        }
        public void DoCompleteFill()
        {
            IncludingProperties = true;
        }
        public void IsDetailRequest()
        {
            IsForDetailView = true;
            IncludingProperties = true;
            IncludingEstimatedCosts = true;
            IncludingDuration = true;
            IncludingFrameworkItems = true;
            IncludingRolesAndActions = true;
            SkippingOrganizationCaching = true;
            //add all conditions profiles for now - to get all costs
            IncludingConnectionProfiles = true;
            ConditionProfilesAsList = false;
            IncludingAddesses = true;
            IncludingSubjectsKeywords = true;
            BubblingUpSubjects = true;
            IncludingEmbeddedCredentials = true;

            IncludingJurisdiction = true;
            IncludingRevocationProfiles = true;
        }
        public void IsPublishRequest()
        {
            //check if this is valid for publishing
            IsForPublishRequest = true;
            IsForDetailView = true;
            SkippingOrganizationCaching = true;
            IncludingProperties = true;
            IncludingEstimatedCosts = true;
            IncludingDuration = true;
            IncludingFrameworkItems = true;
            IncludingRolesAndActions = true;

            //add all conditions profiles for now - to get all costs
            IncludingConnectionProfiles = true;
            ConditionProfilesAsList = false;
            IncludingAddesses = true;
            IncludingSubjectsKeywords = true;
            BubblingUpSubjects = false;
            IncludingEmbeddedCredentials = true;

            IncludingJurisdiction = true;
            IncludingRevocationProfiles = true;
        }
        public void IsCompareRequest()
        {
            IncludingProperties = true;
            IncludingEstimatedCosts = true;
            IncludingDuration = true;
            IncludingFrameworkItems = true;
            IncludingRolesAndActions = true;

            //add all conditions profiles for now - to get all costs
            IncludingConnectionProfiles = true;
        }

        /// <summary>
        /// Indicate options for the edit view
        /// under construction
        /// - really only need details for the basic info view
        /// - other properties would need only the minimum content for profile links 
        /// </summary>
        public void IsEditRequest()
        {
            IsForEditView = true;
            IncludingProperties = true;
            SkippingOrganizationCaching = true;
            IncludingDuration = true;
            IncludingAddesses = true;
            IncludingEstimatedCosts = true;
            IncludingJurisdiction = true;
            IncludingSubjectsKeywords = true;
            IncludingConnectionProfiles = true;
            ConditionProfilesAsList = true;
            IncludingFrameworkItems = true;
            IncludingEmbeddedCredentials = true;
            IncludingRolesAndActions = true;
            IncludingRevocationProfiles = true;

            //need handle only ProfileLink equivalent views for most
        }

        public bool IsForEditView { get; set; }
        public bool IsForDetailView { get; set; }
        public bool IsForPublishRequest { get; set; }
        public bool IsForProfileLinks { get; set; }
        public bool AllowCaching { get; set; }
        public bool SkippingOrganizationCaching { get; set; }
        public bool AllowingOrganizationCaching
        {
            get { return !SkippingOrganizationCaching; }
        }

        public bool IncludingProperties { get; set; }

        public bool IncludingRolesAndActions { get; set; }
        public bool IncludingConnectionProfiles { get; set; }
        public bool ConditionProfilesAsList { get; set; }
        public bool IncludingRevocationProfiles { get; set; }
        public bool IncludingEstimatedCosts { get; set; }
        public bool IncludingDuration { get; set; }
        public bool IncludingAddesses { get; set; }
        public bool IncludingJurisdiction { get; set; }

        public bool IncludingSubjectsKeywords { get; set; }
        public bool BubblingUpSubjects { get; set; }

        //public bool IncludingKeywords{ get; set; }
        //both occupations and industries, and others for latter
        public bool IncludingFrameworkItems { get; set; }

        public bool IncludingEmbeddedCredentials { get; set; }
    }


}
