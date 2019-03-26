using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Models;

using Models.Common;
using CM = Models.Common;
using MN = Models.Node;
//using ME = Models.Elastic;
using Models.ProfileModels;
using EM = Data;
using DBEntity = Data.Organization;
using ThisEntity = Models.Common.Organization;
using Utilities;
using Models.Search.ThirdPartyApiModels;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class OrganizationManager : BaseFactory
	{
		static string thisClassName = "OrganizationManager";
		List<string> messages = new List<string>();
		#region Constants
		public static int ORG_MEMBER_PENDING = 0;
		public static int ORG_MEMBER_ADMIN = 1;
		public static int ORG_MEMBER_EMPLOYEE = 2;
		public static int ORG_MEMBER_STUDENT = 3;
		public static int ORG_MEMBER_EXTERNAL = 4;
		#endregion
		#region Organization - persistance ==================

		/// <summary>
		/// add a Organization
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Add( ThisEntity entity, bool isSiteStaff, ref string statusMessage )
		{
			DBEntity efEntity = new DBEntity();
			
			using ( var context = new Data.CTIEntities() )
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
                    if ( !entity.IsReferenceVersion )
                    {
                        if ( !string.IsNullOrWhiteSpace( entity.CTID ) && entity.CTID.Length == 39 )
                            efEntity.CTID = entity.CTID.ToLower();
                        else
                            efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
                    }

                    efEntity.CreatedById = entity.CreatedById;
					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdatedById = entity.CreatedById;
					efEntity.LastUpdated = System.DateTime.Now;

					context.Organization.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";
						entity.Id = efEntity.Id;
						entity.RowId = efEntity.RowId;

						//TODO
						//notify admin - done in services
						//add current user as an admin member, or entity partner
						//check if has a primary orgId
						if ( !isSiteStaff && !entity.IsReferenceVersion )
						{
							OrganizationMember_Save( efEntity.Id, entity.CreatedById, ORG_MEMBER_ADMIN, entity.CreatedById, ref statusMessage );
						}
						UpdateParts( entity, ref statusMessage );
						return efEntity.Id;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the add was not successful. ";
						string message = string.Format( "OrganizationManager. Add Failed", "Attempted to add a Organization. The process appeared to not work, but was not an exception, so we have no message, or no clue.Organization: {0}, createdById: {1}", entity.Name, entity.CreatedById );
						EmailManager.NotifyAdmin( "OrganizationManager. Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, thisClassName + ".Save() ", "Organization" );
					LoggingHelper.LogError(thisClassName + string.Format( ".Add(), Name: {0} \r\n", efEntity.Name ) + message, true );
					statusMessage = "Error - the save was not successful. <br/> " + message;
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Organization_Add(), Name: {0}", efEntity.Name ) );
					statusMessage = "Unexpected system error. The site administration has been notified.";
				}
			}

			return efEntity.Id;
		}
		public int Add_QAOrg( CM.QAOrganization entity, bool isSiteStaff, ref string statusMessage )
		{
			DBEntity efEntity = new DBEntity();
			using ( var context = new Data.CTIEntities() )
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
                    if ( !entity.IsReferenceVersion )
                    {
                        if ( !string.IsNullOrWhiteSpace( entity.CTID ) && entity.CTID.Length == 39 )
                            efEntity.CTID = entity.CTID.ToLower();
                        else
                            efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
                    }

                    efEntity.CreatedById = entity.CreatedById;
					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdatedById = entity.CreatedById;
					efEntity.LastUpdated = System.DateTime.Now;

					context.Organization.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";
						entity.Id = efEntity.Id;
						entity.RowId = efEntity.RowId;

						//TODO
						//notify admin - done in services
						//add current user as an admin member, or entity partner
						//check if has a primary orgId
						if ( !isSiteStaff )
							OrganizationMember_Save( efEntity.Id, entity.CreatedById, ORG_MEMBER_ADMIN, entity.CreatedById, ref statusMessage );

						UpdateParts( entity, ref statusMessage );
						return efEntity.Id;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the add was not successful. ";
						string message = string.Format( "OrganizationManager. Add_QAOrg Failed", "Attempted to add a QA Organization. The process appeared to not work, but was not an exception, so we have no message, or no clue.Organization: {0}, createdById: {1}", entity.Name, entity.CreatedById );
						EmailManager.NotifyAdmin( "OrganizationManager. Add_QAOrg Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					//LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
					string message = thisClassName + string.Format( ".Organization_Add() DbEntityValidationException, Name: {0}", efEntity.Name );
					statusMessage = "Error - missing fields. ";
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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add_QAOrg(), Name: {0}", efEntity.Name ) );
					statusMessage = "Unexpected system error. The site administration has been notified.";
				}
			}

			return efEntity.Id;
		}
		/// <summary>
		/// Update a Organization
		/// - base only, caller will handle parts?
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Update( ThisEntity entity, ref string statusMessage )
		{
			bool isValid = true;
			int count = 0;
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBEntity efEntity = context.Organization
								.SingleOrDefault( s => s.Id == entity.Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						if ( ValidateProfile( entity, ref messages ) == false )
						{
							statusMessage = string.Join( "<br/>", messages.ToArray() );
							return false;
						}

						//for updates, chances are some fields will not be in interface, don't map these (ie created stuff)
						//**ensure rowId is passed down for use by profiles, etc
						entity.RowId = efEntity.RowId;

						MapToDB( entity, efEntity );
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
								isValid = false;
								string message = string.Format( "OrganizationManager. Update Failed", "Attempted to update a Organization. The process appeared to not work, but was not an exception, so we have no message, or no clue. Organization: {0}, Id: {1}, updatedById: {2}", entity.Name, entity.Id, entity.LastUpdatedById );
								EmailManager.NotifyAdmin( "OrganizationManager. Update Failed", message );
							}
						}
						//continue with parts regardless
						if ( !UpdateParts( entity, ref statusMessage ) )
							isValid = false;

					}
					else
					{
						statusMessage = "Error - update failed, as record was not found.";
						isValid = false;
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".Update. id: {0}", entity.Id ) );
				isValid = false;
			}


			return isValid;
		}
		public bool Update_QAOrg( CM.QAOrganization entity, ref string statusMessage )
		{
			bool isValid = true;
			int count = 0;
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBEntity efEntity = context.Organization
								.SingleOrDefault( s => s.Id == entity.Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						if ( ValidateProfile( entity, ref messages ) == false )
						{
							statusMessage = string.Join( "<br/>", messages.ToArray() );
							return false;
						}

						//for updates, chances are some fields will not be in interface, don't map these (ie created stuff)
						//**ensure rowId is passed down for use by profiles, etc
						entity.RowId = efEntity.RowId;

						MapToDB( entity, efEntity );
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
								isValid = false;
								string message = string.Format( "OrganizationManager. Update_QAOrg Failed", "Attempted to update a Organization. The process appeared to not work, but was not an exception, so we have no message, or no clue. Organization: {0}, Id: {1}, updatedById: {2}", entity.Name, entity.Id, entity.LastUpdatedById );
								EmailManager.NotifyAdmin( "OrganizationManager. Update_QAOrg Failed", message );
							}
						}
						//continue with parts regardless
						if ( !UpdateParts( entity, ref statusMessage ) )
							isValid = false;

					}
					else
					{
						statusMessage = "Error - update failed, as record was not found.";
						isValid = false;
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".Update_QAOrg. id: {0}", entity.Id ) );
				isValid = false;
			}


			return isValid;
		}
		/// <summary>
		/// Update credential registry id, and set status published
		/// </summary>
		/// <param name="recordId"></param>
		/// <param name="envelopeId"></param>
		/// <param name="userId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool UpdateEnvelopeId( int recordId, string envelopeId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			bool updatingStatus = UtilityManager.GetAppKeyValue( "onRegisterSetEntityToPublic", false );
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					DBEntity efEntity = context.Organization
									.SingleOrDefault( s => s.Id == recordId );

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
								string message = string.Format( thisClassName + ".UpdateEnvelopeId Failed", "Attempted to update an EnvelopeId. The process appeared to not work, but was not an exception, so we have no message, or no clue. Organization: {0}, envelopeId: {1}, updatedById: {2}", recordId, envelopeId, userId );
								EmailManager.NotifyAdmin( thisClassName + ". UpdateEnvelopeId Failed", message );
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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UpdateEnvelopeId(), Organization: {0}, envelopeId: {1}, updatedById: {2}", recordId, envelopeId, userId ) );
                    statusMessage = FormatExceptions( ex );
                }
			}

			return isValid;
		}

		/// <summary>
		/// Reset credential registry id, and set status to in process
		/// </summary>
		/// <param name="recordId"></param>
		/// <param name="userId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool UnPublish( int recordId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					DBEntity efEntity = context.Organization
									.SingleOrDefault( s => s.Id == recordId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						efEntity.CredentialRegistryId = null;
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
							string message = string.Format( thisClassName + ".UnPublish Failed", "Attempted to reset a EnvelopeId. The process appeared to not work, but was not an exception, so we have no message, or no clue. Organization: {0}, updatedById: {1}", recordId, userId );
							EmailManager.NotifyAdmin( thisClassName + ". UnPublish Failed", message );
						}
					}
					else
					{
						statusMessage = "Error - update failed, as record was not found.";
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UnPublish(), Organization: {0}, updatedById: {1}", recordId, userId ) );
                    statusMessage = FormatExceptions( ex );
                }
			}

			return isValid;
		}
		public bool UpdateParts( Organization entity, ref string statusMessage )
		{
			//if ( entity.IsReferenceVersion )
			//	return true;

			bool isAllValid = UpdatePartsCommon( entity, ref statusMessage );

			return isAllValid;
		}
		public bool UpdateParts( QAOrganization entity, ref string statusMessage )
		{
            if ( entity.IsReferenceVersion )
                return true;

            bool isAllValid = UpdatePartsCommon( entity, ref statusMessage );

			//specific
			//may not be any, if only profiles

			if ( messages.Count > 0 )
				statusMessage += string.Join( "<br/>", messages.ToArray() );

			return isAllValid;
		}
		public bool UpdatePartsCommon( Organization entity, ref string statusMessage )
		{
			bool isAllValid = true;
			int count = 0;
			List<string> messages = new List<string>();
            //for now allow updating properties for a reference - to allow setting to QA
			if ( UpdateProperties( entity, ref messages ) == false )
				isAllValid = false;

            if ( entity.IsReferenceVersion )
                return true;

            if ( !new OrganizationServiceManager().OrganizationService_Update( entity, false, ref statusMessage ) )
				isAllValid = false;
			//handle jurisdictions? ==> direct

			Entity_ReferenceManager erm = new
				Entity_ReferenceManager();

			if ( erm.Update( entity.Keyword, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_KEYWORD, false ) == false )
				isAllValid = false;
			if ( erm.Update( entity.AlternateName, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_ALTERNATE_NAME, false ) == false )
				isAllValid = false;

			if ( erm.Update( entity.AlternativeIndustries, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_NAICS, false ) == false )
				isAllValid = false;

            var lm = new Entity_LanguageManager();
            if ( lm.Update( entity.InLanguageCodeList, entity.RowId, entity.LastUpdatedById, ref messages ) == false )
                isAllValid = false;

            //departments

            //address - separate

            //subsiduaries
            if ( messages.Count > 0 )
				statusMessage += string.Join( "<br/>", messages.ToArray() );

			return isAllValid;
		}
		public bool UpdateProperties( Organization entity, ref List<string> messages )
		{
			bool isAllValid = true;
			//==== convert to entity properties ========================

			EntityPropertyManager mgr = new EntityPropertyManager();
			

			if ( mgr.UpdateProperties( entity.OrganizationType, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;
			//TODO - may want a check to toggle the IsQaOrg property. It is used for other checks
			// however this would not be dependable, need to query on MapFromDB

			if ( mgr.UpdateProperties( entity.OrganizationSectorType, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SECTORTYPE, entity.LastUpdatedById, ref messages ) == false )
			{
				isAllValid = false;
			}

            if ( entity.IsReferenceVersion )
                return true;

            Entity_ReferenceManager erm = new Entity_ReferenceManager();
            if ( erm.Update( entity.SocialMediaPages, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SOCIAL_MEDIA, true ) == false )
				isAllValid = false;

			//how to handle notifications on 'other'?
			if ( erm.Update( entity.IdentificationCodes, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_IDENTIFIERS, true ) == false )
				isAllValid = false;

			if ( erm.Update( entity.Emails, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_EMAIL_TYPE, true ) == false )
				isAllValid = false;

			if ( erm.Update( entity.PhoneNumbers, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_PHONE_TYPE, true ) == false )
				isAllValid = false;

			//}
			return isAllValid;
		}
		/// <summary>
		/// Delete an Organization, and related Entity
		/// </summary>
		/// <param name="orgId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( int orgId, ref string statusMessage )
		{
			bool isValid = false;
			bool doingVirtualDelete = true;
			if ( orgId == 0 )
			{
				statusMessage = "Error - missing an identifier for the Organization";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					//ensure exists
					DBEntity efEntity = context.Organization
								.SingleOrDefault( s => s.Id == orgId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						if ( doingVirtualDelete )
						{
							statusMessage = string.Format( "Organization: {0}, Id:{1}", efEntity.Name, efEntity.Id );

							//context.Credential.Remove( efEntity );
							efEntity.LastUpdated = System.DateTime.Now;
							//efEntity.LastUpdatedById = userId;
							efEntity.StatusId = CodesManager.ENTITY_STATUS_DELETED;
						}
						else
						{
							Guid rowId = efEntity.RowId;
							int roleCount = 0;
							//check for any existing org roles, and reject delete if any found
							if ( Entity_AgentRelationshipManager.AgentEntityHasRoles( rowId, ref roleCount ) )
							{
								statusMessage = string.Format( "Error - this organization cannot be deleted as there are existing roles {0}.", roleCount );
								return false;
							}
							//16-10-19 mp - we have a 'before delete' trigger to remove the Entity
							//new EntityManager().Delete( rowId, ref statusMessage );

							context.Organization.Remove( efEntity );
						}


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
				catch ( Exception ex )
				{
					statusMessage = ex.Message;
					LoggingHelper.LogError( ex, thisClassName + ".Organization_Delete()" );
                    statusMessage = FormatExceptions( ex );
                    if ( statusMessage.ToLower().IndexOf( "the delete statement conflicted with the reference constraint" ) > -1 )
					{
						statusMessage = "Error: this organization cannot be deleted as it is being referenced by other items, such as credentials. These associations must be removed before this organization can be deleted.";
					}
				}
			}

			return isValid;
		}
        public static bool HasMinimumData( ThisEntity profile, ref List<string> messages, bool validatingUrls = true )
        {
            bool isValid = true;
            if ( !ValidateProfile( profile, ref messages, false ) )
                isValid = false;

            if ( ( profile.Emails == null || profile.Emails.Count == 0 ) &&
                ( profile.PhoneNumbers == null || profile.PhoneNumbers.Count == 0 ) &&
                ( profile.Addresses == null || profile.Addresses.Count == 0 ) &&
                ( profile.ContactPoint == null || profile.ContactPoint.Count == 0 )
                )
                messages.Add("At least one type of contact property, like  physical address, contact point, email address, or phone number is required");

            //need to check offered by as well!
            if ( !profile.ISQAOrganization
                && ( profile.CreatedCredentials == null || profile.CreatedCredentials.Count == 0 )
                && ( profile.OfferedCredentials == null || profile.OfferedCredentials.Count == 0 )
                && !profile.ChildOrganizationsHaveCredentials
                )
            {
                //check for sub-organizations/departments

                //ensure only done with minimum data checks
                messages.Add("A credentialing organization (or a subsidiary/department) must 'own' or 'offer' at least one credential in order to be published to the Credential Registry.</br> ");
            }

            if ( messages.Count > 0 )
                isValid = false;
            return isValid;
        }
        public static bool ValidateProfile( ThisEntity profile, ref List<string> messages, bool validatingUrls = true )
		{
			bool isValid = true;
			int count = messages.Count;

			if ( string.IsNullOrWhiteSpace( profile.Name ) )
			{
				messages.Add( "An organization name must be entered" );
			}
		
			if ( string.IsNullOrWhiteSpace( profile.SubjectWebpage ) )
				messages.Add( "A Subject Webpage must be entered" );

			else if ( validatingUrls && !IsUrlValid( profile.SubjectWebpage, ref commonStatusMessage ) )
				messages.Add( string.Format("The Subject Webpage Url ({0}) is invalid. " + commonStatusMessage, profile.SubjectWebpage) );

            if ( profile.IsReferenceVersion || profile.IsThirdPartyOrganization )
            {
                //no more edits
                if ( FormHelper.HasHtmlTags( profile.Description ) )
                {
                    messages.Add( "HTML or Script Tags are not allowed in the description" );
                }
            }
            else
            {
                if ( string.IsNullOrWhiteSpace( profile.Description ) )
                {
                    messages.Add( "An organization description must be entered" );
                }
                else if ( FormHelper.HasHtmlTags( profile.Description ) )
                {
                    messages.Add( "HTML or Script Tags are not allowed in the description" );
                }
				else if ( profile.Description.Length < MinimumDescriptionLength && !IsDevEnv() )
				{
					messages.Add( string.Format( "The Organization description must be at least {0} characters in length.", MinimumDescriptionLength ) );
				}

				if ( profile.OrganizationType == null
                    || profile.OrganizationType.HasItems() == false )
                    messages.Add( "At least one organization type must be selected." );

                if ( profile.OrganizationSectorType == null
                    || profile.OrganizationSectorType.HasItems() == false )
                    messages.Add( "At least one organization sector type must be selected." );

                if ( validatingUrls && !IsUrlValid( profile.AgentPurposeUrl, ref commonStatusMessage ) )
                    messages.Add( "The Agent Purpose Url is invalid. " + commonStatusMessage );

                if ( validatingUrls && !IsUrlValid( profile.MissionAndGoalsStatement, ref commonStatusMessage ) )
                    messages.Add( "The Mission and Goals Statement Url is invalid. " + commonStatusMessage );

                if ( validatingUrls && !IsUrlValid( profile.AvailabilityListing, ref commonStatusMessage ) )
                    messages.Add( "The Availability Listing Url is invalid. " + commonStatusMessage );

                if ( !string.IsNullOrWhiteSpace( profile.DateEffective ) && !IsValidDate( profile.DateEffective ) )
                {
                    messages.Add( "Please enter a valid effective date" );
                }
                if ( validatingUrls && !string.IsNullOrEmpty( profile.ImageUrl ) && !IsImageUrlValid( profile.ImageUrl, ref commonStatusMessage ) )
                {
                    messages.Add( "The Image Url is invalid. " + commonStatusMessage );
                }

                if ( !profile.InLanguageCodeList.Any() )
                {
                    //messages.Add( "A language must be selected." );
                    //just default to englist
                    profile.InLanguageCodeList.Add( new LanguageProfile()
                    {
                        LanguageCodeId = 40
                    } );
            
                } else if  ( profile.InLanguageCodeList.Count > 1)
                    messages.Add( "Only one language may be entered for an Organization."  );

                IsFoundingDateValid( profile, ref messages );
			}
			

			if ( messages.Count > count )
				isValid = false;

			return isValid;
		}

		/// <summary>
		/// Validate founding date
		/// 17-06-01 mp - date is back to being entered as a string of either 
		/// - yyyy
		/// - yyyy-mm
		/// - yyyy-mm-dd
		/// Future consider allowing friendly dates (April, 1990), not sure about edits
		/// </summary>
		/// <param name="profile"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		private static bool IsFoundingDateValid( ThisEntity profile, ref List<string> messages )
		{
			bool isValid = true;
			bool enforcingFoundingDateFormats = UtilityManager.GetAppKeyValue( "enforcingFoundingDateFormats", false );

			if ( !enforcingFoundingDateFormats )
				return true;

			string format = "<br/>Enter either Year (as yyyy), Year and Month (as yyyy-mm), or Year, Month, and Day (as (yyyy-mm-dd).";
			int validNbr = 0;
			if ( string.IsNullOrWhiteSpace( profile.FoundingDate ) )
				return true;
			if ( profile.FoundingDate.Length == 10)
			{
				if ( IsValidDate( profile.FoundingDate ) )
				{
					//NOTE probably need to enforce yyyy-mm-dd??
					return true;
				}
				else
				{
					messages.Add( "The Founding Date is invalid." + format );
					return false;
				}
			} else if ( profile.FoundingDate.Length == 4 )
			{
				if ( Int32.TryParse( profile.FoundingDate, out validNbr ) )
				{
					if (validNbr < 1800 || validNbr > DateTime.Now.Year)
					{
						messages.Add( "The Founding Date is an invalid range." );
						return false;
					}
					else 
						return true;
				}
				else
				{
					messages.Add( "The Founding Date is invalid. " + format );
					return false;
				}
			}
			else if ( profile.FoundingDate.Length == 6 || profile.FoundingDate.Length == 7 )
			{
				string year = profile.FoundingDate.Substring( 0, 4 );
				if ( Int32.TryParse( year, out validNbr ) )
				{
					if ( validNbr < 1800 || validNbr > DateTime.Now.Year )
					{
						messages.Add( "The Founding Date is an invalid range." );
						return false;
					}
				}
				else
				{
					messages.Add( "The Founding Date is invalid. " + format );
					return false;
				}
				//here we have a valid year, test mth
				string mth = profile.FoundingDate.Substring( 5 );
				if ( Int32.TryParse( mth, out validNbr ) )
				{
					if ( validNbr < 1 || validNbr > 12 )
					{
						messages.Add( "The Founding month has an invalid range." );
						return false;
					}
					else
						return true;
				}
				else
				{
					messages.Add( "The Founding Date is invalid (month). " + format );
					return false;
				}
			}
			//all other formats are invalid
			messages.Add( "The Founding Date has invalid format. " + format );
			return false;
			//if ( !string.IsNullOrWhiteSpace( profile.FoundingYear ) )
			//{
			//	if ( int.TryParse( profile.FoundingYear, out validNbr ) )
			//	{
			//		if ( validNbr != 0 && ( validNbr < 1700 || validNbr > DateTime.Now.Year ) )
			//			messages.Add( "The Founding Year must be a valid year" );
			//	}
			//	else
			//	{
			//		messages.Add( "The Founding Year must be a valid year" );
			//	}
			//}
			//if ( !string.IsNullOrWhiteSpace( profile.FoundingMonth ) )
			//{
			//	if ( int.TryParse( profile.FoundingMonth, out validNbr ) )
			//	{
			//		if ( validNbr != 0 && ( validNbr < 1 || validNbr > 12 ) )
			//			messages.Add( "The Founding Month must be a valid month" );
			//	}
			//	else
			//	{
			//		messages.Add( "The Founding Month must be a valid month" );
			//	}
			//}
			//if ( !string.IsNullOrWhiteSpace( profile.FoundingDay ) )
			//{
			//	if ( int.TryParse( profile.FoundingDay, out validNbr ) )
			//	{
			//		if ( validNbr != 0 && ( validNbr < 1 || validNbr > 31 ) )
			//			messages.Add( "The Founding Day must be a valid calendar day" );
			//	}
			//	else
			//	{
			//		messages.Add( "The Founding Day must be a valid day" );
			//	}
			//}
			////a year must be provided if month is provided
			//if ( string.IsNullOrWhiteSpace( profile.FoundingYear )
			//	&& ( !string.IsNullOrWhiteSpace( profile.FoundingMonth )
			//		|| !string.IsNullOrWhiteSpace( profile.FoundingDay ) )
			//		)
			//{
			//	messages.Add( "The Founding Year must be provided for the founding date" );
			//}
			////a month must be provided if day is provided
			//if ( string.IsNullOrWhiteSpace( profile.FoundingMonth )
			//	&& ( !string.IsNullOrWhiteSpace( profile.FoundingDay ) )
			//		)
			//{
			//	messages.Add( "The Founding Month must be provided with a Founding Day" );
			//}
			//return isValid;
		}
		#endregion

		#region == Retrieval =======================
		public static ThisEntity GetDetail( int id, bool isForPublishing )
		{

			bool includeCredentials = true;
			bool includingRoles = true;
			ThisEntity entity = new ThisEntity();

			using ( var context = new Data.CTIEntities() )
			{
				DBEntity item = context.Organization
						.SingleOrDefault( s => s.Id == id
						&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB_Detail( item, entity
						, true      //includingProperties
						, includeCredentials
						, includingRoles
						, isForPublishing );
                    if (isForPublishing)
                    {
                        //do min value checks!
                    }
				}
			}

			return entity;
		}
       
        /// <summary>
        /// Get org for edit view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ThisEntity GetForEdit( int id )
		{

			ThisEntity entity = new ThisEntity();

			using ( var context = new Data.CTIEntities() )
			{
				DBEntity item = context.Organization
						.SingleOrDefault( s => s.Id == id
						&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
						);

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity,
						true,
						true,
						true,
						true );
				}
			}

			return entity;
		}
		public static CM.QAOrganization GetQAOrgForEdit( int id )
		{
			CM.QAOrganization entity = new CM.QAOrganization();

			using ( var context = new Data.CTIEntities() )
			{
				DBEntity item = context.Organization
						.SingleOrDefault( s => s.Id == id
						&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
						);

				if ( item != null && item.Id > 0 )
				{
					MapFromDB_QA( item, entity,
						true,
						true,
						true,
						false );
				}
			}

			return entity;
		}
        public static ThisEntity GetForApproval( int id, ref List<string> messages )
        {
            ThisEntity entity = new ThisEntity();

            using ( var context = new Data.CTIEntities() )
            {
                DBEntity item = context.Organization
                            .FirstOrDefault( s => s.Id == id
                                && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
                                );

                if ( item != null && item.Id > 0 )
                {
                    //??
                    MapFromDB_Common( item, entity, false, true, true, true, false );
                    //string dataCheck = "";
                    //if ( !entity.HasMinimumData( ref dataCheck ) )
                    //    messages.Add( dataCheck );
                    if ( HasMinimumData( entity, ref messages, false ) == false )
                    {
                        LoggingHelper.DoTrace( 1, string.Format( "PublishOrganization for recordId: {0} failed due to missing minimum required data. ", entity.Id ) + string.Join( "<br/>", messages.ToArray() ));
                       
                    }
                }
            }

            return entity;
        }
        /// <summary>
        /// Get minimum org for display in search results
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ThisEntity GetForSummary( int id, bool includingRoles = false )
		{

			ThisEntity to = new ThisEntity();

			using ( var context = new Data.CTIEntities() )
			{
				context.Configuration.LazyLoadingEnabled = false;
				DBEntity from = context.Organization
						.SingleOrDefault( s => s.Id == id
						&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
						);

				if ( from != null && from.Id > 0 )
				{
					MapFromDB_Summary( from, to );
					if ( includingRoles )
					{
						GetOrgRoles( from, to, false );
					}
				}
			}

			return to;
		}

        public static ThisEntity GetForSummary( string agentRowId )
        {
            Guid agentUid = new Guid( agentRowId );

            return GetForSummary(agentUid);
        }
        /// <summary>
        /// Get agent (only active records: StatusId <= published)
        /// </summary>
        /// <param name="agentRowId"></param>
        /// <returns></returns>
        public static ThisEntity GetForSummary( Guid agentRowId, bool allowingCaching = true)
        {
            ThisEntity to = new ThisEntity();
            //this was a problem for checking published status!
            string key = "organizationSummary_" + agentRowId.ToString();
            if (allowingCaching &&
                CacheManager.IsOrganizationAvailableFromCache(key, ref to) )
                return to;

            using ( var context = new Data.CTIEntities() )
            {
                context.Configuration.LazyLoadingEnabled = false;
                DBEntity from = context.Organization
                        .SingleOrDefault( s => s.RowId == agentRowId
                        && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
                        );

                if ( from != null && from.Id > 0 )
                {
                    MapFromDB_Summary( from, to );
                    CacheManager.AddOrganizationToCache(to, key);
                }
            }

            return to;
        }

        /// <summary>
        /// Get a BASIC Organization by Ctid
        /// </summary>
        /// <param name="ctid"></param>
        /// <param name="includingRoles"></param>
        /// <returns></returns>
        public static ThisEntity GetByCtid( string ctid, bool includingRoles = false, bool includingAddresses = false, bool includingManifests = false )
        {

            ThisEntity to = new ThisEntity();
            if (string.IsNullOrWhiteSpace( ctid ))
                return to;

            using (var context = new Data.CTIEntities())
            {
                context.Configuration.LazyLoadingEnabled = false;
                DBEntity from = context.Organization
                        .FirstOrDefault( s => s.CTID.ToLower() == ctid.ToLower()
                        && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
                        );
                if ( ctid == "ce-8f1a526b-3e18-4a4e-829f-05b85197772c" )
                {
                    //WACKY TAFFY ERROR - FOR SOME REASON THE LATTER CTID IS NOT FOUND THOUGH VALID
                    ctid = "Microsoft";
                    //from = context.Organization
                    //        .FirstOrDefault( s => s.Name == ctid );
                }
                if (from != null && from.Id > 0)
                {
                    MapFromDB_Summary( from, to );
                    if (includingRoles)
                    {
                        GetOrgRoles( from, to, false );
                    }
					if ( includingAddresses )
					{
						to.Addresses = AddressProfileManager.GetAll( to.RowId );
					}
                    if (includingManifests)
                    {
                        to.HasConditionManifest = ConditionManifestManager.GetAll( to.Id, true );
                        to.HasCostManifest = CostManifestManager.GetAll( to.Id, true );
                    }
                }
            }

            return to;
        }
        //public static ThisEntity GetByName( string name, ref string status )
        //{

        //    ThisEntity to = new ThisEntity();

        //    using (var context = new Data.CTIEntities())
        //    {
        //        context.Configuration.LazyLoadingEnabled = false;
        //        //can be many, so use list and reject if multiple
        //        List<DBEntity> results = context.Organization
        //                .Where( s => s.Name.ToLower() == name.ToLower()
        //                && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
        //                ).ToList();

        //        if (results != null )
        //        {
        //            if (results.Count == 1)
        //                MapFromDB_Summary( results[ 0 ], to );
        //            else if (results.Count > 1)
        //                status = "Error - there are mulitple organizations with the name: " + name + ". Please ensure a unique organization name is used, or use a CTID for an existing organization instead.";
        //        }
        //    }

        //    return to;
        //}
        public static ThisEntity GetByNameAndUrl( string name, string url, ref string status )
        {

            ThisEntity to = new ThisEntity();
            //warning the trailing slash is trimmed during save so need to handle, or do both
            
            if (url.EndsWith("/"))
                url = url.TrimEnd( '/' );

            using (var context = new Data.CTIEntities())
            {
                context.Configuration.LazyLoadingEnabled = false;
                //can be many, so use list and reject if multiple
                List<DBEntity> results = context.Organization
                        .Where( s => s.Name.ToLower() == name.ToLower() 
                        && (s.URL.ToLower() == url.ToLower())
                        && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
                        ).ToList();

                if (results != null)
                {
                    if (results.Count == 1)
                        MapFromDB_Summary( results[ 0 ], to );
                    else if (results.Count > 1)
                        status = "Error - there are mulitple organizations with the name: " + name + ". Please ensure a unique organization name is used, or use a CTID for an existing organization instead.";
                }
            }

            return to;
        }


		

		public static MN.ProfileLink Agent_GetProfileLink( Guid agentId )
		{
			MN.ProfileLink entity = new MN.ProfileLink();
			using ( var context = new ViewContext() )
			{
				Views.Agent_Summary efEntity = context.Agent_Summary
						.FirstOrDefault( s => s.AgentRowId == agentId );

				if ( efEntity != null && efEntity.AgentRelativeId > 0 )
				{
					entity.RowId = efEntity.AgentRowId;
					entity.Id = efEntity.AgentRelativeId;
					entity.Name = efEntity.AgentName;
					entity.Type = typeof( Models.Node.Organization );
				}
			}

			return entity;
		}

		/// <summary>
		/// Retrieve a list of all orgs by name
		/// </summary>
		/// <returns></returns>
		public static List<Organization> QuickSearch( int userId, string keyword, int pageNumber, int pageSize, ref int pTotalRows )
		{
			List<ThisEntity> list = new List<ThisEntity>();
			ThisEntity to = new ThisEntity();
			keyword = string.IsNullOrWhiteSpace( keyword ) ? "" : keyword.Trim();
			if ( pageSize == 0 )
				pageSize = 500;
			int skip = 0;
			if ( pageNumber > 1 )
				skip = ( pageNumber - 1 ) * pageSize;

			using ( var context = new Data.CTIEntities() )
			{
				context.Configuration.LazyLoadingEnabled = false;

				var Query = from Results in context.Organization
						.Where( s => s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
							&& ( keyword == "" || s.Name.Contains( keyword ) ) )
						.OrderBy( s => s.Name )
							select Results;

				pTotalRows = Query.Count();
				var results = Query.Skip( skip ).Take( pageSize )
					.ToList();

				//List<DBEntity> results2 = context.Organization
				//	.Where( s => keyword == "" || s.Name.Contains( keyword ) )
				//	.Take( pageSize )
				//	.OrderBy( s => s.Name )
				//	.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( DBEntity from in results )
					{
						MapFromDB_Summary( from, to );

						list.Add( to );
					}

					//Other parts
				}
			}

			return list;
		} //

		/// <summary>
		/// Select Quality assurance orgs
		/// 17-02-20 Previously the summary view returned records based on org type of quality assurance. This has been updated to also check for ISQAOrganization. The latter is set where user explicitly creates a QA org - may need to enforce selecting org type of QA??
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="keyword"></param>
		/// <param name="maxTerms"></param>
		/// <returns></returns>
		public static List<Organization> Organization_SelectQAOrgs( int userId = 0, string keyword = "", int maxTerms = 0 )
		{
			List<Organization> list = new List<Organization>();
			Organization entity = new Organization();
			keyword = keyword.Trim();
			if ( maxTerms == 0 )
				maxTerms = 500;

			using ( var context = new ViewContext() )
			{
				List<Views.Organization_Summary> results = context.Organization_Summary
						.Where( s => s.IsAQAOrganization > 0
						   && ( keyword == "" || s.Name.Contains( keyword ) ) )
						.Take( maxTerms )
						.OrderBy( s => s.Name )
						.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Views.Organization_Summary item in results )
					{
						entity = new Organization();
						entity.Id = item.Id;
						entity.RowId = item.RowId;
						entity.Name = item.Name;
						if ( string.IsNullOrWhiteSpace( item.City ) == false )
							entity.Name += " ( " + item.City + " )";
						entity.CTID = item.CTID;
						if ( string.IsNullOrWhiteSpace( item.CTID ) )
							entity.IsReferenceVersion = true;

						list.Add( entity );
					}
				}
			}

			return list;
		}

        

        /// <summary>
        /// Used to retrieve managing orgs in the editor
        /// </summary>
        /// <param name="insertSelectTitle"></param>
        /// <returns></returns>
        public static List<CodeItem> Organization_SelectAllAsCodes( bool insertingSelectTitle = false )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity = new CodeItem();
			using ( var context = new Data.CTIEntities() )
			{
				List<DBEntity> results = context.Organization
					.Where( s => s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED )
									.OrderBy( s => s.Name )
									.ToList();

				if ( results != null && results.Count > 0 )
				{
					if ( insertingSelectTitle )
					{
						entity = new CodeItem();
						entity.Id = 0;
						entity.Title = "Select Organization";
						entity.URL = "";
						list.Add( entity );
					}

					foreach ( DBEntity item in results )
					{
						entity = new CodeItem();
						entity.Id = item.Id;
						entity.Name = item.Name;

						list.Add( entity );
					}
				}
			}

			return list;
		}

		public static List<DBEntity> GetOrganizations( string search )
		{
			using ( var context = new Data.CTIEntities() )
			{
				context.Configuration.ProxyCreationEnabled = false;
				return context.Organization
					.Where( x => (x.CTID != null  && x.CTID.Length == 39)
						&& x.Name.Contains( search ) )
					.ToList();
			}
		}

		public static List<string> Autocomplete( string pFilter, int pageNumber, int pageSize, int userId, ref int pTotalRows )
		{
			bool autocomplete = true;
			List<string> results = new List<string>();
			List<Organization> list = Search( pFilter, "", pageNumber, pageSize, ref pTotalRows, userId, false, autocomplete );

			string prevName = "";
			foreach ( Organization item in list )
			{
				//note excluding duplicates may have an impact on selected max terms
				if ( item.Name.ToLower() != prevName )
					results.Add( item.Name );

				prevName = item.Name.ToLower();
			}
			return results;
		}
		public static List<Organization> Search( string pFilter, string pOrderBy, int pageNumber, int pageSize, ref int pTotalRows, int userId = 0, bool idsOnly = false, bool autocomplete = false )
		{
			string connectionString = DBConnectionRO();
			Organization item = new Organization();
			List<Organization> list = new List<Organization>();
			var result = new DataTable();
			using ( SqlConnection c = new SqlConnection( connectionString ) )
			{
				c.Open();

				if ( string.IsNullOrEmpty( pFilter ) )
				{
					pFilter = "";
				}
                LoggingHelper.DoTrace( 6, thisClassName + string.Format( ".Search() START. Filter: {0}", pFilter) );

                using ( SqlCommand command = new SqlCommand( "OrganizationSearch", c ) )
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
                        try
                        {
                            pTotalRows = Int32.Parse( rows );
                        }
                        catch
                        {
                            pTotalRows = 0;
                        }
                    }
                    catch ( Exception ex )
                    {
                        pTotalRows = 0;

                        LoggingHelper.DoTrace( 6, thisClassName + string.Format( ".Search() EXCEPTION. \r\n{0}", ex.Message ) );
                        LoggingHelper.LogError( ex, thisClassName + string.Format( ".Search() - Execute proc, Message: {0} \r\n Filter: {1} \r\n", ex.Message, pFilter ) );

                        item = new Organization();
                        item.Name = "Unexpected error encountered. System administration has been notified. Please try again later. ";
                        item.Description = ex.Message;
                        item.AlternativeIdentifier = "error";
                        list.Add( item );
                        return list;
                    }
                }

				foreach ( DataRow dr in result.Rows )
				{
					item = new Organization();
					item.Id = GetRowColumn( dr, "Id", 0 );
					item.Name = GetRowColumn( dr, "Name", "missing" );
					item.FriendlyName = FormatFriendlyTitle( item.Name );

					item.Description = GetRowColumn( dr, "Description", "" );
					string rowId = GetRowColumn( dr, "RowId" );
					item.RowId = new Guid( rowId );
					item.ctid = GetRowPossibleColumn( dr, "CTID", "" );
					if ( string.IsNullOrWhiteSpace( item.CTID ) )
						item.IsReferenceVersion = true;
                    else
                        item.IsThirdPartyOrganization = GetRowColumn( dr, "IsThirdPartyOrganization", false );
                    item.CredentialRegistryId = GetRowPossibleColumn( dr, "CredentialRegistryId", "" );

                    item.SubjectWebpage = GetRowColumn( dr, "URL", "" );
					item.CanEditRecord = GetRowColumn( dr, "CanEditRecord", false );

                    //make sure ok for micro search get profile
                    if ( idsOnly || autocomplete )
                    {
                        list.Add( item );
                        continue;
                    }
                    //=====================================
                    if ( item.CredentialRegistryId.Length == 36 )
                        item.Name += " [published] ";
                    item.EntityLastUpdated = GetRowColumn(dr, "EntityLastUpdated", System.DateTime.Now);
                    item.LastPublishDate = GetRowPossibleColumn(dr, "LastPublishDate", "");
                    DateTime testdate;
                    if (DateTime.TryParse(item.LastPublishDate, out testdate))
                        item.IsPublished = true;

                    //approvals
                    item.LastApprovalDate = GetRowPossibleColumn( dr, "LastApprovalDate", "" );
					item.ContentApprovedBy = GetRowPossibleColumn( dr, "ContentApprovedBy" );
					item.ContentApprovedById = GetRowPossibleColumn( dr, "ContentApprovedById", 0 );
					if ( item.ContentApprovedById > 0 )
						item.IsApproved = true;

					item.ImageUrl = GetRowColumn( dr, "ImageUrl", "" );
					if ( GetRowColumn( dr, "CredentialCount", 0 ) > 0 )
						item.IsACredentialingOrg = true;
					item.IsAQAOrg = GetRowColumn( dr, "IsAQAOrganization", false );
                    

                    item.MainPhoneNumber = PhoneNumber.DisplayPhone( GetRowColumn( dr, "MainPhoneNumber", "" ) );
					
					//all addressess
					int addressess = GetRowPossibleColumn( dr, "AvailableAddresses", 0 );
					if ( addressess > 0 )
					{
						//item.Addresses = AddressProfileManager.GetAllOrgAddresses( item.Id );
						item.Addresses = AddressProfileManager.GetAll( item.RowId );
						//just in case (short term
						if ( item.Addresses.Count > 0 )
							item.Address = item.Addresses[ 0 ];
					}

					//Edit - Added to fill out gray boxes in results - NA 5/12/2017
					item.OrganizationType = EntityPropertyManager.FillEnumeration( item.RowId, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_TYPE );
					item.OrganizationSectorType = EntityPropertyManager.FillEnumeration( item.RowId, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SECTORTYPE );
					//End Edit

					list.Add( item );
				}

                LoggingHelper.DoTrace( 6, thisClassName + string.Format( ".Search() END. Total rows: {0}", pTotalRows) );

                return list;

			}
		}

		public static List<OrganizationSummary> MainSearch( string pFilter, string pOrderBy, int pageNumber, int pageSize, ref int pTotalRows, int userId = 0, bool idsOnly = false, bool autocomplete = false )
		{
			string connectionString = DBConnectionRO();
			OrganizationSummary item = new OrganizationSummary();
			List<OrganizationSummary> list = new List<OrganizationSummary>();
			var result = new DataTable();

			using ( SqlConnection c = new SqlConnection( connectionString ) )
			{
				c.Open();

				if ( string.IsNullOrEmpty( pFilter ) )
				{
					pFilter = "";
				}
                LoggingHelper.DoTrace( 6, thisClassName + string.Format( ".Search() START. Filter: {0}", pFilter ) );
                using ( SqlCommand command = new SqlCommand( "OrganizationSearch", c ) )
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
					}
					catch ( Exception ex )
					{
						pTotalRows = 0;
                        LoggingHelper.DoTrace( 6, thisClassName + string.Format( ".Search() EXCEPTION. \r\n{0}", ex.Message ) );
                        LoggingHelper.LogError( ex, thisClassName + string.Format( ".MainSearch() - Execute proc, Message: {0} \r\n Filter: {1} \r\n", ex.Message, pFilter ) );

						item = new CM.OrganizationSummary();
						item.Name = "Unexpected error encountered. System administration has been notified. Please try again later. ";
						item.Description = ex.Message;
						item.AlternativeIdentifier = "error";
						list.Add( item );
						return list;
					}
				}
			}

			try
			{
                LoggingHelper.DoTrace( 6, thisClassName + string.Format( ".MainSearch(). Filter: {0}, TotalRows: {1}", pFilter, pTotalRows ) );

				foreach ( DataRow dr in result.Rows )
				{
					item = new OrganizationSummary();
					item.Id = GetRowColumn( dr, "Id", 0 );
					item.Name = GetRowColumn( dr, "Name", "missing" );
					item.FriendlyName = FormatFriendlyTitle( item.Name );

					if ( idsOnly || autocomplete )
					{
						list.Add( item );
						continue;
					}
					item.Description = GetRowColumn( dr, "Description", "" );
					string rowId = GetRowColumn( dr, "RowId" );
					item.RowId = new Guid( rowId );
					item.ctid = GetRowPossibleColumn( dr, "CTID", "" );
					if ( string.IsNullOrWhiteSpace( item.CTID ) )
						item.IsReferenceVersion = true;

					item.SubjectWebpage = GetRowColumn( dr, "URL", "" );
					item.CanEditRecord = GetRowColumn( dr, "CanEditRecord", false );
                    //=====================================
                    item.CredentialRegistryId = GetRowPossibleColumn( dr, "CredentialRegistryId", "" );
                    item.EntityLastUpdated = GetRowColumn(dr, "EntityLastUpdated", System.DateTime.Now);
                    item.LastPublishDate = GetRowPossibleColumn(dr, "LastPublishDate", "");
                    DateTime testdate;
                    if ( DateTime.TryParse( item.LastPublishDate, out testdate ) )
                    {
                        item.LastPublished = testdate;
                        item.IsPublished = true;
                    }

                    //approvals
                    item.LastApprovalDate = GetRowPossibleColumn(dr, "LastApprovalDate", "");
                    if ( DateTime.TryParse( item.LastApprovalDate, out testdate ) )
                    {
                        item.IsApproved = true;
                        item.LastApproved = testdate;
                    }
                    item.ContentApprovedBy = GetRowPossibleColumn(dr, "ContentApprovedBy");
                    item.ContentApprovedById = GetRowPossibleColumn(dr, "ContentApprovedById", 0);
                    if (item.ContentApprovedById > 0)
                        item.IsApproved = true;

                    item.ImageUrl = GetRowColumn( dr, "ImageUrl", "" );
					if ( GetRowColumn( dr, "CredentialCount", 0 ) > 0 )
						item.IsACredentialingOrg = true;
					item.IsAQAOrg = GetRowColumn( dr, "IsAQAOrganization", false );

					item.MainPhoneNumber = PhoneNumber.DisplayPhone( GetRowColumn( dr, "MainPhoneNumber", "" ) );

					//all addressess
					int addressess = GetRowPossibleColumn( dr, "AvailableAddresses", 0 );
					if ( addressess > 0 )
					{
						//item.Addresses = AddressProfileManager.GetAllOrgAddresses( item.Id );
						item.Addresses = AddressProfileManager.GetAll( item.RowId );
					}

                    item.AssertionsTotal = GetRowPossibleColumn( dr, "AssertionsTotal", 0 );

                    //Edit - Added to fill out gray boxes in results - NA 5/12/2017
                    item.OrganizationType = EntityPropertyManager.FillEnumeration( item.RowId, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_TYPE );
					item.OrganizationSectorType = EntityPropertyManager.FillEnumeration( item.RowId, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SECTORTYPE );
					//End Edit

					item.NaicsResults = Fill_CodeItemResults( dr, "NaicsList", CodesManager.PROPERTY_CATEGORY_NAICS, true, true );
					item.IndustryOtherResults = Fill_CodeItemResults( dr, "OtherIndustriesList", CodesManager.PROPERTY_CATEGORY_NAICS, false, false, false );

					item.OwnedByResults = Fill_CodeItemResults( dr, "OwnedByList", CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE, false, false );

					item.OfferedByResults = Fill_CodeItemResults( dr, "OfferedByList", CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE, false, false );

					item.AsmtsOwnedByResults = Fill_CodeItemResults( dr, "AsmtsOwnedByList", CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE, false, false );

					item.LoppsOwnedByResults = Fill_CodeItemResults( dr, "LoppsOwnedByList", CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE, false, false );

                   // item.AccreditedByResults = Fill_CodeItemResults( dr, "AccreditedByList", CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE, false, false );

                   // item.ApprovedByResults = Fill_CodeItemResults( dr, "ApprovedByList", CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE, false, false );

                    item.QualityAssurance = Fill_AgentItemResultsFromXml( dr, "QualityAssurance", CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE, false, false );

                    string relatedItems = GetRowColumn( dr, "ChildOrganizations" );
                    string[] array = relatedItems.Split( ',' );
                    if ( array.Count() > 0 )
                        foreach ( var i in array )
                        {
                            if ( !string.IsNullOrWhiteSpace( i ) )
                                item.ChildOrganizationsList.Add( i.ToLower() );
                        }

                    list.Add( item );
				}

                LoggingHelper.DoTrace( 6, thisClassName + string.Format( ".MainSearch() END. Total rows: {0}", pTotalRows ) );
                return list;


			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError(ex, thisClassName + string.Format( ".MainSearch() - results, Message: {0} \r\n Filter: {1} \r\n", pFilter )  );

				item = new CM.OrganizationSummary();
				item.Name = "Unexpected error encountered. System administration has been notified. Please try again later. ";
				item.Description = ex.Message;
				item.AlternativeIdentifier = "error";
				list.Add( item );
				return list;
			}
		}

		public static void MapToDB( ThisEntity from, DBEntity to )
		{
			MapToDB_Base( from, to );
            if (from.IsAQAOrg || from.ISQAOrganization)
                to.ISQAOrganization = true;
            else 
                to.ISQAOrganization = false;

		}
		public static void MapToDB( CM.QAOrganization from, DBEntity to )
		{
			MapToDB_Base( from, to );
			to.ISQAOrganization = true;

		}
		public static void MapToDB_Base( ThisEntity from, DBEntity to )
		{
			//want to ensure fields from create are not wiped
			if ( to.Id > 0 )
			{
				//to.RowId = from.rowId;
				if ( from.StatusId > 0 )
					to.StatusId = from.StatusId;
			}
			else
			{
				if ( IsValidDate( from.Created ) )
					to.Created = from.Created;
				to.CreatedById = from.CreatedById;
				to.StatusId = 1;
				//this can only be set on add
				to.IsThirdPartyOrganization = from.IsReferenceVersion || from.IsThirdPartyOrganization;
                if ( (bool)to.IsThirdPartyOrganization )
                    to.StatusId = 2;
            }

			to.Id = from.Id;
			to.Name = ConvertSpecialCharacters( from.Name );
			to.URL = NormalizeUrlData( from.SubjectWebpage, null );

			to.Description = ConvertSpecialCharacters( from.Description );
            to.ISQAOrganization = from.ISQAOrganization;
            //helper for QA reference org, to ensure marked as QA
            if ( from.OrganizationType.HasItems() )
            {
                EnumeratedItem item = from.OrganizationType.Items.SingleOrDefault( s => s.SchemaName == "orgType:QualityAssurance" );
                if ( item != null && item.Id > 0 )
                    to.ISQAOrganization = true;
            }

            if ( from.IsReferenceVersion )
				return;

			to.AgentPurposeUrl = NormalizeUrlData( from.AgentPurposeUrl, null );
			to.Purpose = ConvertSpecialCharacters(from.Purpose);

			
			//to.UniqueURI = GetUrlData( from.UniqueURI, null );
			to.AvailabilityListing = NormalizeUrlData( from.AvailabilityListing, null );
			to.ImageURL = NormalizeUrlData( from.ImageUrl, null );
			//to.AlternativeIdentifier = from.AlternativeIdentifier;

			//FoundingDate is now a string
			//interface must handle? Or do we have to fix here?
			//depends if just text is passed or separates
			//already validated
			if ( !string.IsNullOrWhiteSpace( from.FoundingDate ) )
				to.FoundingDate = from.FoundingDate;
			else
				to.FoundingDate = null;
			//to.FoundingDate = FormatFoundingDate( from );

			to.MissionAndGoalsStatement = NormalizeUrlData( from.MissionAndGoalsStatement, null );
			to.MissionAndGoalsStatementDescription = ConvertSpecialCharacters( from.MissionAndGoalsStatementDescription );
			to.Versioning = from.Versioning;


			if ( from.ServiceType != null )
			{
				to.ServiceTypeOther = from.ServiceType.OtherValue;
			}
			//to.ServiceTypeOther = from.ServiceTypeOther;

			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById;
		}
		private static string FormatFoundingDate( ThisEntity from )
		{
			string foundingDate = "";
			if ( !string.IsNullOrWhiteSpace( from.FoundingYear ) && from.FoundingYear.Length == 4 && IsInteger( from.FoundingYear ) )
				foundingDate = from.FoundingYear;
			else
				return ""; //must have at least a year

			if ( !string.IsNullOrWhiteSpace( from.FoundingMonth )
				&& IsInteger( from.FoundingMonth )
				&& ( from.FoundingMonth != "0"
					&& from.FoundingMonth != "00" ) )
			{
				if ( from.FoundingMonth.Length == 1 )
					from.FoundingMonth = "0" + from.FoundingMonth;
				foundingDate += "-" + from.FoundingMonth;
			}
			else
				return foundingDate;

			if ( !string.IsNullOrWhiteSpace( from.FoundingDay )
				&& IsInteger( from.FoundingDay )
					&& ( from.FoundingDay != "0"
					&& from.FoundingDay != "00" ) )
			{
				if ( from.FoundingDay.Length == 1 )
					from.FoundingDay = "0" + from.FoundingDay;

				foundingDate += "-" + from.FoundingDay;
			}

			return foundingDate;
		}
		public static void MapFromDB( DBEntity from, ThisEntity to,
			bool isForEditView,
			bool includingProperties = false,
			bool includingRoles = false,
			bool includeCredentials = false )
		{
			MapFromDB_Common( from, to,
				isForEditView,
				includingProperties,
				includeCredentials,
				includingRoles,
				false );

			if ( to.IsReferenceVersion )
				return;

			//===============================================================

			//ISQAOrganization can be assigned in mapping if there is an org type of QA body
			//17-07-01 currently no difference, moved VerificationStatus here to claify as previously QA only
			to.VerificationStatus = Organization_VerificationStatusManager.GetAll( to.Id );
			if ( to.ISQAOrganization )
			{
				//MapFromDB_QA( from, to, false );
			}
		}

		private static void MapFromDB_QA( DBEntity from, CM.QAOrganization to,
					bool isForEditView,
					bool includingProperties = false,
					bool includingRoles = false,
					bool includeCredentials = false )
		{
			MapFromDB_Common( from, to, isForEditView, includingProperties, includeCredentials, includingRoles, false );
			//now do QA specific
			//17-07-01 currently no difference, moved VerificationStatus here to clarify as previously QA only
			to.VerificationStatus = Organization_VerificationStatusManager.GetAll( to.Id );
			//ToMap_QA( from, to, isForEditView );

		}


		public static void MapFromDB_Detail( DBEntity from, ThisEntity to,
					bool includingProperties,
					bool includeCredentials,
					bool includingRoles,
					bool isForPublishing 
			)
		{
			MapFromDB_Common( from, to,
				false,      //isForEditView
				includingProperties,
				includeCredentials,
				includingRoles,
				true, //includingQAWhereUsed
                isForPublishing
                );
			//17-07-01 currently no difference, moved VerificationStatus here to claify as previously QA only
			to.VerificationStatus = Organization_VerificationStatusManager.GetAll( to.Id );
			if ( to.ISQAOrganization )
			{
				//ToMap_QA( from, to, false );
			}
			//if ( isForPublishing )
			//{
			//	GetPublishingOrgRoles( to );
   //             //check if has credentials
			//}
		}
		public static void MapFromDB_Summary( DBEntity from, ThisEntity to )
		{
			if ( to == null )
				to = new ThisEntity();
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.StatusId = ( int ) ( from.StatusId ?? 1 );

			to.Name = from.Name;
			to.Description = from.Description;
			to.SubjectWebpage = from.URL;
			if ( from.ImageURL != null && from.ImageURL.Trim().Length > 0 )
				to.ImageUrl = from.ImageURL;
			else
				to.ImageUrl = null;
			to.CredentialRegistryId = from.CredentialRegistryId;
            if ( !string.IsNullOrWhiteSpace( to.CredentialRegistryId ) )
            {
                SetLastPublishedData( to );
            }
            to.ctid = from.CTID;
			if ( string.IsNullOrWhiteSpace( to.CTID ) )
				to.IsReferenceVersion = true;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

            to.EntityApproval = Entity_ApprovalManager.GetByParent( to.RowId );
            if ( to.EntityApproval != null && to.EntityApproval.Id > 0 )
			{
                to.LastApproved = to.EntityApproval.Created;
				to.IsApproved = true;
			}

            to.RelatedEntity = EntityManager.GetEntity( to.RowId, false );
            if ( to.RelatedEntity != null && to.RelatedEntity.Id > 0 )
                to.EntityLastUpdated = to.RelatedEntity.LastUpdated;

            to.ISQAOrganization = from.ISQAOrganization == null ? false : ( bool ) from.ISQAOrganization;

            to.InLanguageCodeList = Entity_LanguageManager.GetAll( to.RowId );
        }
		public static void MapFromDB_Common( DBEntity from, ThisEntity to,
					bool isForEditView,
					bool includingProperties,
					bool includeCredentials, //true for detail, and publishing
					bool includingRoles,
					bool includingQAWhereUsed,
                    bool isForPublishing = false)

		{
			if ( to == null )
				to = new ThisEntity();
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.StatusId = ( int ) ( from.StatusId ?? 1 );

			to.Name = from.Name;
			to.Description = from.Description;
			to.SubjectWebpage = from.URL;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;
			if ( from.Account_Modifier != null )
			{
				to.LastUpdatedBy = from.Account_Modifier.FirstName + " " + from.Account_Modifier.LastName;
			}
			else
			{
				AppUser user = AccountManager.AppUser_Get( to.LastUpdatedById );
				to.LastUpdatedBy = user.FullName();
			}
            to.RelatedEntity = EntityManager.GetEntity( to.RowId, false );
            if ( to.RelatedEntity != null && to.RelatedEntity.Id > 0 )
                to.EntityLastUpdated = to.RelatedEntity.LastUpdated;

            to.IsThirdPartyOrganization = from.IsThirdPartyOrganization != null ? ( bool ) from.IsThirdPartyOrganization : false;
			to.ctid = from.CTID;


            //need to use the service types as well. See assignment for to.OrganizationType below
            to.ISQAOrganization = from.ISQAOrganization == null ? false : ( bool )from.ISQAOrganization;
            to.OrganizationType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_TYPE );
            if ( to.OrganizationType.HasItems() )
            {
                EnumeratedItem item = to.OrganizationType.Items.SingleOrDefault( s => s.SchemaName == "orgType:QualityAssurance" );
                if ( item != null && item.Id > 0 )
                    to.ISQAOrganization = true;


                //remove helper properties, if not edit
                if ( !isForEditView )
                {
                    if ( to.OrganizationType != null && to.OrganizationType.Items != null )
                    {
                        foreach ( EnumeratedItem item2 in to.OrganizationType.Items )
                        {
                            if ( item2.SchemaName == "{none}" )
                            {
                                to.OrganizationType.Items.Remove( item2 );
                                break;
                            }
                        }
                    }
                }
            }
            to.InLanguageCodeList = Entity_LanguageManager.GetAll( to.RowId );
            if ( string.IsNullOrWhiteSpace( to.ctid )  || to.IsThirdPartyOrganization )
			{
				to.IsReferenceVersion = true;
				to.IsThirdPartyOrganization = true;
				return;
			}

			//====================================================
			if ( from.ImageURL != null && from.ImageURL.Trim().Length > 0 )
				to.ImageUrl = from.ImageURL;
			else
				to.ImageUrl = null;

			to.EntityApproval = Entity_ApprovalManager.GetByParent( to.RowId );
            if ( to.EntityApproval != null && to.EntityApproval.Id > 0 )
                to.LastApproved = to.EntityApproval.Created;

            to.CredentialRegistryId = from.CredentialRegistryId;
            if ( !string.IsNullOrWhiteSpace( to.CredentialRegistryId ) )
            {
                SetLastPublishedData( to );
            }


			//=========================================================
			to.AgentPurposeUrl = from.AgentPurposeUrl;
			to.Purpose = from.Purpose;
			to.MissionAndGoalsStatement = from.MissionAndGoalsStatement;
			to.MissionAndGoalsStatementDescription = from.MissionAndGoalsStatementDescription;


			to.AvailabilityListing = from.AvailabilityListing;
			to.Versioning = from.Versioning;


			//map, although not currently used in interface
			to.FoundingDate = from.FoundingDate;
			
			//not used from db? Using property other - except this is now the only other property????
			//16-09-02 mp - push to enumeration
			to.ServiceTypeOther = from.ServiceTypeOther;


			//to.UniqueURI = from.UniqueURI;

			to.Keyword = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD );

			to.AlternateName = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_ALTERNATE_NAME );
			//properties
			if ( includingProperties )
			{
				to.Addresses = AddressProfileManager.GetAll( to.RowId );

				to.ContactPoint = Entity_ContactPointManager.GetAll( to.RowId );

				//to.ServiceType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_ORG_SERVICE );
				OrganizationServiceManager.FillOrganizationService( from, to );

				
				//sector type? - as an enumeration, will be stored in properties
				to.OrganizationSectorType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SECTORTYPE );
				to.Jurisdiction = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId );

				//identifcation codes
				//ALL
				to.IdentificationCodes = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_IDENTIFIERS );
				//Others alternate identifiers

				to.AlternativeIdentifiers = Entity_ReferenceManager.GetOrgAlternativeIdentifiers( to.RowId );

				//to.QAPurposeType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_AGENT_QAPURPOSE_TYPE );
				//to.QATargetType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_QA_TARGET_TYPE );
				to.SocialMediaPages = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SOCIAL_MEDIA );

				//get separate other identifiers
				to.PhoneNumbers = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_PHONE_TYPE );
				to.Emails = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_EMAIL_TYPE );

				to.Industry = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );

				to.AlternativeIndustries = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );
				//}
			}

			//credentialing?
			if ( includeCredentials )
			{
                GetOrgArtifacts( to, isForEditView );

    //            to.CreatedCredentials = Entity_AgentRelationshipManager.Credentials_ForOwningOrg( to.RowId );
				//if ( to.CreatedCredentials != null && to.CreatedCredentials.Count > 0 )
				//	to.IsACredentialingOrg = true;
    //            if (!isForEditView)
    //            {
    //                //don't include assessments and lopps in edit view, for now
    //                to.OwnedAssessments = AssessmentManager.GetAllForOwningOrg( to.RowId );
    //                to.OwnedLearningOpportunities = LearningOpportunityManager.GetAllForOwningOrg( to.RowId );

    //                to.OfferedCredentials = Entity_AgentRelationshipManager.OfferedCredentials_ForOwningOrg( to.RowId );
    //                //aslo 
    //                if ( to.CreatedCredentials.Count == 0 && to.OfferedCredentials.Count == 0)
    //                {
    //                    //also c
    //                }
    //            }
			}
			else
			{
				//need to distinguish QA from non-QA credentials
				if ( CountCredentials( from ) > 0 )
					to.IsACredentialingOrg = true;
			}



			if ( isForEditView )
			{
				to.HasConditionManifest = ConditionManifestManager.GetAll( to.Id, true );
				to.HasCostManifest= CostManifestManager.GetAll( to.Id, true );

			}
			else if ( includingProperties )
			{
				to.HasConditionManifest = ConditionManifestManager.GetAll( to.Id, false );
				to.HasCostManifest = CostManifestManager.GetAll( to.Id, false );
			}

            if ( includingRoles )
            {
                //need to call after GetOrgArtifacts, as GetOrgRoles has a check for owns credentials
                GetOrgRoles( from, to, isForEditView, isForPublishing );
            }
            if ( includingQAWhereUsed )
            {
                //to.QualityAssuranceActor = Entity_QualityAssuranceActionManager.QualityAssuranceActionProfile_GetAllForAgent( to.RowId );
            }



            MapProcessProfiles( from, to, isForEditView );

			//need to distiguish between edit, list, and detail
			to.VerificationServiceProfiles = Entity_VerificationProfileManager.GetAll( to.RowId, isForEditView );
		}

        /// <summary>
        /// Get last published date
        /// NOTE: need to ensure after an un publish event
        /// </summary>
        /// <param name="record"></param>
        public static void SetLastPublishedData( ThisEntity record )
        {
            //NOTE there is a method in ActivityServices for this
            //  string GetLastPublishDate( string entityType, int entityId )
            //which calls:
            //SiteActivity GetLastPublishRecord( string entityType, int entityId )

            string filter = string.Format( " [ActivityObjectId] = {0} AND [ActivityType] = 'Organization' AND Activity = 'Credential Registry'", record.Id );
            int totalRows = 0;
            List<SiteActivity> list = ActivityManager.Search( filter, "base.CreatedDate desc", 1, 1, ref totalRows );
            if ( list != null && list.Count > 0 )
            {
                //check first record
                SiteActivity item = list[ 0 ];
                if ( item.Event.ToLower().IndexOf( "registered" ) > -1 || item.Event.ToLower().IndexOf( "updated" ) > -1 )
                {
                    record.LastPublished = item.Created;
                    record.LastUpdatedById = ( int ) item.ActionByUserId;
                    record.IsPublished = true;
                }
            }

        }
        [Obsolete]
        private static void GetPublishingOrgRoles( ThisEntity to )
		{
			to.OrganizationThirdPartyAssertions = Entity_AgentRelationshipManager.GetAll_OrganizationOwnsEtcRolesSummary( to.Id );

            //to.OrganizationAssertions = Entity_AgentRelationshipManager.GetAll_OrganizationOwnsEtcRolesSummaries( to.Id );
        }

		private static void GetOrgRoles( DBEntity from, ThisEntity to,
					bool isForEditView, bool isForPublishing = false )
		{
			if ( isForEditView )
			{
				//gets targets, with just a csv of roles
				to.OrganizationRole_Recipient = Entity_AgentRelationshipManager.AgentEntityRole_GetAllSummary( to.RowId, false );

                //compare with GetPublishingOrgRoles
                //nothing returned for ACBSP
                //to.OrganizationRole_QAPerformed = Entity_AgentRelationshipManager.GetAll_QATargets_ForAgent( to.RowId );

                //not sure we want this, can't use for publishing, as uses the CSV view 
                var assertions = Entity_AssertionManager.GetAllAssertionsForDetailView( to.RowId );
                to.CredentialAssertions = assertions.Where( x => x.TargetEntityTypeId == 1 ).ToList();
                to.OrganizationAssertions = assertions.Where( x => x.TargetEntityTypeId == 2 ).ToList();
                to.AssessmentAssertions = assertions.Where( x => x.TargetEntityTypeId == 3 ).ToList();
                to.LoppAssertions = assertions.Where( x => x.TargetEntityTypeId == 7 ).ToList();
            }
			else
			{
                if ( isForPublishing )
                {
					to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAll_ToEnumeration( to.RowId, true );

					//need this view for publishing - rename for clarity
					to.OrganizationFirstPartyAssertions = Entity_AssertionManager.GetAllDirectAssertions( to.RowId );
                    to.OrganizationThirdPartyAssertions = Entity_AgentRelationshipManager.GetAll_OrganizationOwnsEtcRolesSummary( to.Id );
                } else
                {

                    //not sure we want this, can't use for publishing, as uses the CSV view 
                    //TBD: this should be replace by GetAllCombined, leaving both for now
                    var assertions = Entity_AssertionManager.GetAllAssertionsForDetailView( to.RowId );
                    to.CredentialAssertions = assertions.Where( x => x.TargetEntityTypeId == 1 ).ToList();
                    to.OrganizationAssertions = assertions.Where( x => x.TargetEntityTypeId == 2 ).ToList();
                    to.AssessmentAssertions = assertions.Where( x => x.TargetEntityTypeId == 3 ).ToList();
                    to.LoppAssertions = assertions.Where( x => x.TargetEntityTypeId == 7 ).ToList();

					//not sure why true here?
                    to.OrganizationRole_Recipient = Entity_AgentRelationshipManager.AgentEntityRole_GetAll_ToEnumeration( to.RowId, true );

                    //also want the inverses - where this org was providing the QA for asmts, etc. 
                    //NOTE: currently this is just mapped to a ProfileLink
                    //to.OrganizationRole_QAPerformed = Entity_AgentRelationshipManager.GetAll_QATargets_ForAgent( to.RowId );
                    //turns out, not used for detail page YET!!!
                    to.OrganizationRole_QAPerformed = Entity_AssertionManager.GetAllCombined( to.RowId );
                }
            }
			to.JurisdictionAssertions = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_OFFERREDIN );

			//to.QualityAssuranceAction = Entity_QualityAssuranceActionManager.QualityAssuranceActionProfile_GetAll( to.RowId );

			//dept and subsiduaries ????
			Entity_AgentRelationshipManager.AgentRole_FillAllSubOrganizations( to, 0, isForEditView );
            //checks for publishing
            if ( !isForEditView )
            {
                if ( !to.ISQAOrganization
               && ( to.OwnsOrOffersCredentials  == false)
               )
                {
                    //check for sub-organizations/departments
                    if ( to.OrganizationRole_Subsidiary.Count > 0 )
                    {
                        //may have to go to multiple levels
                        foreach ( var item in to.OrganizationRole_Subsidiary )
                        {
                            GetOrgArtifacts( item.ActingAgent, isForEditView );
                            if ( item.ActingAgent.OwnsOrOffersCredentials )
                            {
                                to.ChildOrganizationsHaveCredentials = true;
                                break;
                            }
                        }
                    }
                    if ( !to.ChildOrganizationsHaveCredentials && to.OrganizationRole_Dept.Count > 0 )
                    {
                        foreach ( var item in to.OrganizationRole_Dept )
                        {
                            GetOrgArtifacts( item.ActingAgent, isForEditView );
                            if ( item.ActingAgent.OwnsOrOffersCredentials )
                            {
                                to.ChildOrganizationsHaveCredentials = true;
                                break;
                            }
                        }
                    }
                }
            }
                
            //parent org 
            Entity_AgentRelationshipManager.GetParentOrganizations( to, isForEditView );
        }

        private static void GetOrgArtifacts( ThisEntity to, bool isForEditView )
        {
            to.CreatedCredentials = Entity_AgentRelationshipManager.Credentials_ForOwningOrg( to.RowId );
            if ( to.CreatedCredentials != null && to.CreatedCredentials.Count > 0 )
                to.IsACredentialingOrg = true;
            if ( !isForEditView )
            {
                //don't include assessments and lopps in edit view, for now
                to.OwnedAssessments = AssessmentManager.GetAllForOwningOrg( to.RowId );
                to.OwnedLearningOpportunities = LearningOpportunityManager.GetAllForOwningOrg( to.RowId );

                to.OfferedCredentials = Entity_AgentRelationshipManager.OfferedCredentials_ByOrg( to.RowId );
                 
                if ( to.OwnsOrOffersCredentials == false )
                {
                    //also check child orgs - actually now done from Mapper
                }
            }
        }

		private static void MapProcessProfiles( DBEntity from, ThisEntity to,
					bool isForEditView )
		{
			//get all and then split
			List<ProcessProfile> list = Entity_ProcessProfileManager.GetAll( to.RowId, isForEditView );
			foreach ( ProcessProfile item in list )
			{
				//some default for 1??
				if ( item.ProcessTypeId == Entity_ProcessProfileManager.ADMIN_PROCESS_TYPE )
					to.AdministrationProcess.Add( item );
				else if ( item.ProcessTypeId == Entity_ProcessProfileManager.DEV_PROCESS_TYPE )
					to.DevelopmentProcess.Add( item );
				else if ( item.ProcessTypeId == Entity_ProcessProfileManager.MTCE_PROCESS_TYPE )
					to.MaintenanceProcess.Add( item );

				else if ( item.ProcessTypeId == Entity_ProcessProfileManager.REVIEW_PROCESS_TYPE )
					to.ReviewProcess.Add( item );
				else if ( item.ProcessTypeId == Entity_ProcessProfileManager.APPEAL_PROCESS_TYPE )
					to.AppealProcess.Add( item );
				else if ( item.ProcessTypeId == Entity_ProcessProfileManager.COMPLAINT_PROCESS_TYPE )
					to.ComplaintProcess.Add( item );
				//else if ( item.ProcessTypeId == Entity_ProcessProfileManager.CRITERIA_PROCESS_TYPE )
				//	to.CriteriaProcess.Add( item );
				else if ( item.ProcessTypeId == Entity_ProcessProfileManager.REVOKE_PROCESS_TYPE )
					to.RevocationProcess.Add( item );
				else
				{
					//produce warning where not mapped
					to.ReviewProcess.Add( item );
					LoggingHelper.LogError( string.Format("OrganizationManager.MapProcessProfiles Unexpected ProcessProfile. OrgId: {0}, Type: {1} ", from.Id, item.ProcessTypeId), true );
				}


			}
		}


		#endregion

		#region **** COUNTS ****
		private static int CountCredentials( DBEntity entity )
		{
			//change to use owning org
			//
			//return Entity_AgentRelationshipManager.CredentialCount_ForCreatingOrg( entity.RowId );
			return Entity_AgentRelationshipManager.CredentialCount_ForOwningOrg( entity.RowId );
		}

		public static int QAOrgCounts()
		{
			int count = 0;
			using ( var context = new Data.CTIEntities() )
			{
				count = context.Organization.Count( x => x.StatusId < 4
						&& x.ISQAOrganization == true );
			}

			return count;
		}
		#endregion



		#region org member methods

		public static void UpdateOrganizations( int userId, int[] orgs, int memberTypeId, int createdBy )
		{
			using ( var db = new Data.CTIEntities() )
			{
				try
				{
					var existOrgs = db.Organization_Member.Where( x => x.UserId == userId );
					var oldOrgs = existOrgs.Select( x => x.ParentOrgId ).ToArray();

					if ( orgs == null )
						orgs = new int[] { };

					//Adding New Organizations 
					orgs.Except( oldOrgs ).ToList().ForEach( x =>
					{
						var newOrg = new Data.Organization_Member { UserId = userId, ParentOrgId = x, CreatedById = createdBy, OrgMemberTypeId = memberTypeId, Created = DateTime.Now, LastUpdatedById = createdBy, LastUpdated = DateTime.Now, IsPrimaryOrganization = !( GetPrimaryOrganizationId( userId ) > 0 ) };

						db.Entry( newOrg ).State = System.Data.Entity.EntityState.Added;
					} );

					//Delete old Organizations which are unselected
					existOrgs.Where( x => !orgs.Contains( x.ParentOrgId ) ).ToList().ForEach( x =>
					{
						db.Entry( x ).State = System.Data.Entity.EntityState.Deleted;
					} );

					db.SaveChanges();
				}
				catch ( Exception ex )
				{ }
			}
		}

		public int OrganizationMember_Save( int orgId, int userId, int orgMemberTypeId, int createdById, ref string statusMessage )
		{
			int id = 0;
			//statusMessage = ""; //??don't want to remove existing status
			EM.Organization_Member efEntity = new EM.Organization_Member();
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					//check if exists
					efEntity = context.Organization_Member
						.SingleOrDefault( s => s.ParentOrgId == orgId && s.UserId == userId );

					//CM.OrganizationMember orgMember = OrganizationMember_Get( orgId, userId );
					if ( efEntity == null || efEntity.Id == 0 )
					{
						efEntity = new EM.Organization_Member();
						efEntity.CreatedById = createdById;
						efEntity.Created = System.DateTime.Now;

						efEntity.ParentOrgId = orgId;
						efEntity.UserId = userId;
					}
					else
					{

					}

					efEntity.OrgMemberTypeId = orgMemberTypeId;

					efEntity.LastUpdatedById = createdById;
					efEntity.LastUpdated = System.DateTime.Now;

					//determine if primary - would check if user has other
					//or should this be an input parameter
					if ( GetPrimaryOrganizationId( userId ) > 0 )
						efEntity.IsPrimaryOrganization = false;
					else
						efEntity.IsPrimaryOrganization = true;

					if ( efEntity.Id == 0 )
					{
						context.Organization_Member.Add( efEntity );
					}

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						//statusMessage = "";
						id = efEntity.Id;
					}
					else
					{
						//?no info on error
						string msg = "Error - adding the organization member was not successful. ";
						statusMessage = string.IsNullOrWhiteSpace( statusMessage ) ? msg : statusMessage + "<br/>" + msg;
						string message = string.Format( "OrganizationManager. OrganizationMember_Save Failed", "Attempted to add an Organization member. The process appeared to not work, but was not an exception, so we have no message, or no clue. Organization: {0}, userId: {1}, createdById: {2}", orgId, userId, createdById );
						EmailManager.NotifyAdmin( "OrganizationManager. OrganizationMember_Save Failed", message );
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".OrganizationMember_Save(), Organization: {0}, userId: {1}, createdById: {2}", orgId, userId, createdById ) );
				}
			}

			return id;
		}

		public bool OrganizationMember_Delete( int orgId, int userId, ref string statusMessage, ref bool userWasFound)
		{
			bool isValid = false;
			if ( orgId == 0 || userId == 0 )
			{
				statusMessage = "Error - please provide a valid organization Id, and user Id";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				EM.Organization_Member efEntity = context.Organization_Member
							.SingleOrDefault( s => s.ParentOrgId == orgId && s.UserId == userId );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					userWasFound = true;
					context.Organization_Member.Remove( efEntity );
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					}
				}
				else
				{
					statusMessage = "Error/Warning - user is not part of this organization.";
				}
			}

			return isValid;
		}


		/// <summary>
		/// Return list of members for an organization
		/// </summary>
		/// <param name="orgId"></param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public static List<CM.OrganizationMember> OrganizationMembers_ListByName( int orgId, int pageNumber, int pageSize, ref int pTotalRows )
		{
			pTotalRows = 0;
			List<CM.OrganizationMember> list = new List<CM.OrganizationMember>();
			CM.OrganizationMember entity = new CM.OrganizationMember();

			if ( pageSize == 0 )
				pageSize = 500;
			int skip = 0;
			if ( pageNumber > 1 )
				skip = ( pageNumber - 1 ) * pageSize;

			using ( var context = new Data.CTIEntities() )
			{
				var Query = from Results in context.Organization_Member
						.Where( s => s.ParentOrgId == orgId )
						.OrderBy( s => s.Account.LastName )
							select Results;

				pTotalRows = Query.Count();
				var results = Query.Skip( skip ).Take( pageSize )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( EM.Organization_Member item in results )
					{
						entity = new CM.OrganizationMember();
						entity.Id = item.Id;
						entity.ParentOrgId = item.ParentOrgId;
						entity.UserId = item.UserId;
						entity.OrgMemberTypeId = item.OrgMemberTypeId;
						entity.IsPrimaryOrganization = ( bool ) ( item.IsPrimaryOrganization ?? false );

						entity.Created = item.Created;
						entity.CreatedById = item.CreatedById ?? 0;
						entity.LastUpdated = item.LastUpdated;
						entity.LastUpdatedById = item.LastUpdatedById ?? 0;

						//Hmm no reason to map the org, as most likely in context of org
						//OR ensure minimum
						MapFromDB_Summary( item.Organization, entity.Organization );
						AccountManager.MapFromDB( item.Account, entity.Account );

						list.Add( entity );
					}

				}
			}

			return list;
		}

		/// <summary>
		/// Get all active organization memberships for a user.
		/// Note the number is likely to be low, so paging is unlikely. The caller should use a high page size.
		/// NOTE: should we be returning org member, or orgs? Depending on the purpose, the member type might be useful.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public static List<CM.OrganizationMember> GetUserOrganizations( int userId, int pageNumber, int pageSize, ref int pTotalRows )
		{
			pTotalRows = 0;
			List<CM.OrganizationMember> list = new List<CM.OrganizationMember>();
			CM.OrganizationMember entity = new CM.OrganizationMember();

			if ( pageSize == 0 )
				pageSize = 500;
			int skip = 0;
			if ( pageNumber > 1 )
				skip = ( pageNumber - 1 ) * pageSize;

			using ( var context = new Data.CTIEntities() )
			{
				var Query = from Results in context.Organization_Member
						.Where( s => s.UserId == userId && s.Organization.StatusId < 4 )
						.OrderBy( s => s.Organization.Name )
							select Results;

				pTotalRows = Query.Count();
				var results = Query.Skip( skip ).Take( pageSize )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( EM.Organization_Member item in results )
					{
						entity = new CM.OrganizationMember();
						entity.Id = item.Id;
						entity.ParentOrgId = item.ParentOrgId;
						entity.UserId = item.UserId;
						entity.OrgMemberTypeId = item.OrgMemberTypeId;
						entity.IsPrimaryOrganization = ( bool ) ( item.IsPrimaryOrganization ?? false );

						entity.Created = item.Created;
						entity.CreatedById = item.CreatedById ?? 0;
						entity.LastUpdated = item.LastUpdated;
						entity.LastUpdatedById = item.LastUpdatedById ?? 0;

						//Hmm no reason to map the org, as most likely in context of org
						//OR ensure minimum
						MapFromDB_Summary( item.Organization, entity.Organization );
                        //why doing account, will be the same for each item
						AccountManager.MapFromDB( item.Account, entity.Account );

						list.Add( entity );
					}

				}
			}

			return list;
		}

		public static List<CodeItem> SelectUserOrganizationsAsCodeItems( int userId )
		{
			int pTotalRows = 0;
			List<CodeItem> profiles = new List<CodeItem>();
			CodeItem profile = new CodeItem();

			//get all ogrs
			List<CM.OrganizationMember> list = GetUserOrganizations( userId, 1, 100, ref pTotalRows );
			if ( pTotalRows > 100 && pTotalRows < 500 )
			{
				//just get all???
				list = GetUserOrganizations( userId, 1, pTotalRows, ref pTotalRows );
			}
			if ( list != null && list.Count > 0 )
			{
				foreach ( CM.OrganizationMember e in list )
				{

					profile = new CodeItem();
					profile.Id = e.Organization.Id;
					profile.Name = e.Organization.Name;
					profile.Description = e.Organization.Description;
					profile.URL = e.Organization.SubjectWebpage;
					profiles.Add( profile );
				}
			}
			return profiles;

		}
		public static List<MN.ProfileLink> SelectUserOrganizationsForProfileList( int userId )
		{
			int pTotalRows = 0;
			List<MN.ProfileLink> profiles = new List<MN.ProfileLink>();
			MN.ProfileLink profile = new MN.ProfileLink();

			//get all ogrs
			List<CM.OrganizationMember> list = GetUserOrganizations( userId, 1, 100, ref pTotalRows );
			if ( pTotalRows > 100 && pTotalRows < 500 )
			{
				//just get all???
				GetUserOrganizations( userId, 1, pTotalRows, ref pTotalRows );
			}
			if ( list != null && list.Count > 0 )
			{
				foreach ( CM.OrganizationMember e in list )
				{

					profile = new MN.ProfileLink();
					profile.RowId = e.Organization.RowId;
					profile.Id = e.Organization.Id;
					profile.Name = e.Organization.Name;
					profile.Type = typeof( Models.Node.Organization );
					profiles.Add( profile );
				}
			}
			return profiles;

		}

		

		/// <summary>
		/// Get the orgId of the primary org for a user
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public static int GetPrimaryOrganizationId( int userId )
		{
			//TODO - reduce columns accessed!!!!!!
			Organization org = GetPrimaryOrganization( userId );
			if ( org != null && org.Id > 0 )
				return org.Id;
			else
				return 0;
		}
		/// <summary>
		/// Get the primary organization for a user
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public static Organization GetPrimaryOrganization( int userId )
		{
			Organization org = new Organization();
			using ( var context = new Data.CTIEntities() )
			{
				//to do include isprimaryOrg in list - don't filter upon the latter until process is fully implemented. 
				//otherwise just take first one
				List<EM.Organization_Member> list = context.Organization_Member
						.Where( s => s.UserId == userId )
						.OrderBy( s => s.Organization.Name )
						.ToList();

				if ( list != null && list.Count() > 0 )
				{
					int cntr = 0;
					foreach ( EM.Organization_Member item in list )
					{
						cntr++;
						if ( ( bool ) ( item.IsPrimaryOrganization ?? false )
							|| list.Count == cntr )
						{
							//MapFromDB( item.Organization, org, false, false, false );
							MapFromDB_Summary( item.Organization, org );
							break;
						}
					}
				}

				return org;
			}
		}

        /// <summary>
		/// Return true if user is a member of any organization
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public static bool IsMemberOfAnyOrganization( int userId )
        {

            using (var context = new Data.CTIEntities())
            {
                List<EM.Organization_Member> list = context.Organization_Member
                        .Where( s => s.UserId == userId ).ToList();

                if (list != null && list.Count() > 0)
                    return true;
                else
                    return false;
            }
        }
        /// <summary>
        /// Return true if user is a member of the provided organization
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public static bool IsOrganizationMember( int userId, int orgId )
		{

			using ( var context = new Data.CTIEntities() )
			{
				var orgMember = context.Organization_Member
						.SingleOrDefault( s => s.ParentOrgId == orgId && s.UserId == userId );

				if ( orgMember != null && orgMember.Id > 0 )
					return true;
				else
					return false;
			}
		}

		/// <summary>
		/// Return true if user is a member of the provided organization
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="orgUid"></param>
		/// <returns></returns>
		public static bool IsOrganizationMember( int userId, Guid orgUid )
		{

			using ( var context = new Data.CTIEntities() )
			{
				var orgMember = context.Organization_Member
						.SingleOrDefault( s => s.UserId == userId && s.Organization.RowId == orgUid  );

				if ( orgMember != null && orgMember.Id > 0 )
					return true;
				else
					return false;
			}
		}
		/// <summary>
		/// Get an org member
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public static CM.OrganizationMember OrganizationMember_Get( int orgId, int userId )
		{
			CM.OrganizationMember entity = new CM.OrganizationMember();

			using ( var context = new Data.CTIEntities() )
			{
				var orgMember = context.Organization_Member
						.SingleOrDefault( s => s.ParentOrgId == orgId && s.UserId == userId );

				if ( orgMember != null && orgMember.Id > 0 )
				{
					entity.Id = orgMember.Id;
					entity.ParentOrgId = orgMember.ParentOrgId;
					entity.UserId = orgMember.UserId;
					entity.OrgMemberTypeId = orgMember.OrgMemberTypeId;
					entity.IsPrimaryOrganization = ( bool ) ( orgMember.IsPrimaryOrganization ?? false );

					entity.Created = orgMember.Created;
					entity.CreatedById = orgMember.CreatedById ?? 0;
					entity.LastUpdated = orgMember.LastUpdated;
					entity.LastUpdatedById = orgMember.LastUpdatedById ?? 0;
				}
			}
			return entity;
		}

		#endregion

	}
}
