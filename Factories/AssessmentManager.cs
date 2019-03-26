using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


using Models;

using Models.Common;
using Models.ProfileModels;
using Models.Search;
using EM = Data;
using Utilities;
using DBEntity = Data.Assessment;
using ThisEntity = Models.ProfileModels.AssessmentProfile;
using CondProfileMgr = Factories.Entity_ConditionProfileManager;
//using CondProfileMgrOld = Factories.ConnectionProfileManager;
namespace Factories
{
    public class AssessmentManager : BaseFactory
	{
		static string thisClassName = "AssessmentManager";
		List<string> messages = new List<string>();

		#region Assessment - persistance ==================

		/// <summary>
		/// add a Assessment
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Add( ThisEntity entity, ref string statusMessage )
		{
			statusMessage = "";
			DBEntity efEntity = new DBEntity();
			//AssessmentPropertyManager opMgr = new AssessmentPropertyManager();
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					if ( ValidateProfile( entity, ref  messages ) == false )
					{
						statusMessage = string.Join( "<br/>", messages.ToArray() );
						return 0;
					}

					MapToDB( entity, efEntity );

                    if ( !entity.IsReferenceVersion )
                        efEntity.StatusId = 1; //obsolete?
                    else
                        efEntity.StatusId = 2;
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
					efEntity.LastUpdatedById = entity.LastUpdatedById = entity.CreatedById;
					efEntity.LastUpdated = System.DateTime.Now;

					context.Assessment.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";
						entity.Id = efEntity.Id;
						entity.RowId = efEntity.RowId;

						if ( UpdateParts( entity, ref messages ) == false )
						{
							statusMessage += string.Join( ", ", messages.ToArray() );
						}

						return efEntity.Id;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the add was not successful. ";
						string message = string.Format( "AssessmentManager. Assessment_Add Failed", "Attempted to add a Assessment. The process appeared to not work, but was not an exception, so we have no message, or no clue. Assessment: {0}, createdById: {1}", entity.Name, entity.CreatedById );
						EmailManager.NotifyAdmin( "AssessmentManager. Assessment_Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "Assessment" );
					LoggingHelper.LogError( thisClassName + string.Format( ".Add(), Name: {0}; userId: {1} \r\n", entity.Name, entity.CreatedById ) + message, true );

					statusMessage = "Error - the save was not successful. <br/> " + message;
					
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Name: {0}; userId: {1}", entity.Name, entity.CreatedById ) );
				}
			}

			return efEntity.Id;
		}
		/// <summary>
		/// Update a Assessment
		/// - base only, caller will handle parts?
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Update( ThisEntity entity, ref string statusMessage )
		{
			bool isValid = true;
			int count = 0;
			statusMessage = "";
			//AssessmentPropertyManager opMgr = new AssessmentPropertyManager();
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBEntity efEntity = context.Assessment
								.SingleOrDefault( s => s.Id == entity.Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						if ( ValidateProfile( entity, ref  messages ) == false )
						{
							statusMessage = string.Join( "<br/>", messages.ToArray() );
							return false;
						}

						//for updates, chances are some fields will not be in interface, don't map these (ie created stuff)
						//fill in fields that may not be in entity
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
								string message = string.Format( "AssessmentManager. Assessment_Update Failed", "Attempted to update a Assessment. The process appeared to not work, but was not an exception, so we have no message, or no clue. Assessment: {0}, Id: {1}, updatedById: {2}", entity.Name, entity.Id, entity.LastUpdatedById );
								EmailManager.NotifyAdmin( "AssessmentManager. Assessment_Update Failed", message );
							}
						}
						//continue with parts regardless
						if ( UpdateParts( entity, ref messages ) == false )
						{
							isValid = false;
							statusMessage += string.Join( "<br/>", messages.ToArray() );
						}
						
					}
					else
					{
						statusMessage = "Error - update failed, as record was not found.";
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".Update. id: {0}; userId: {1}", entity.Id, entity.LastUpdatedById ) );
				statusMessage = ex.Message;
				isValid = false;
			}


			return isValid;
        }

        public static bool HasMinimumData( ThisEntity profile, ref List<string> messages, bool validatingUrls = true )
        {
            bool isValid = true;
            if (!ValidateProfile( profile, ref messages, false ))
                isValid = false;
            //has to have at least one connection or part of something
            if (profile.IsPartOfCredential == null || profile.IsPartOfCredential.Count == 0)
            {
                messages.Add( string.Format( "The assessment: {0} ({1}) is not connected to any credential, via connections or conditions. It cannot be published as a standalone document.", profile.Name, profile.Id ) );
                isValid = false;
            }
            return isValid;
        }

		public static bool ValidateProfile( ThisEntity profile, ref List<string> messages, bool validatingUrls = true )
		{
			bool isValid = true;
			int count = messages.Count;

			if ( string.IsNullOrWhiteSpace( profile.Name ) )
			{
				messages.Add( "An Assessment name must be entered" );
			}

			if ( string.IsNullOrWhiteSpace( profile.SubjectWebpage ) )
				messages.Add( "A Subject Webpage name must be entered" );

			else if ( validatingUrls && !IsUrlValid( profile.SubjectWebpage, ref commonStatusMessage ) )
			{
				messages.Add( "The Assessment Subject Webpage is invalid. " + commonStatusMessage );
			}


            if ( profile.IsReferenceVersion )
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
					messages.Add( "An Assessment Description must be entered" );
				}
				else if ( FormHelper.HasHtmlTags( profile.Description ) )
                {
                    messages.Add( "HTML or Script Tags are not allowed in the description" );
                }
				else if ( profile.Description.Length < MinimumDescriptionLength && !IsDevEnv() )
				{
					messages.Add( string.Format( "The Assessment description must be at least {0} characters in length.", MinimumDescriptionLength ) );
				}

				if ( !IsGuidValid( profile.OwningAgentUid ) )
				{
					messages.Add( "An owning organization must be selected" );
				}
                if (profile.OwnerRoles == null || profile.OwnerRoles.Items.Count == 0)
                {
                    messages.Add( "Invalid request, please select one or more roles for the owning agent." );
                }

                if ( !profile.InLanguageCodeList.Any() )
                    messages.Add( "A language must be selected." );

                //require at least one avaialble type property 
                //on initial add, cannot add an address!
                if ( string.IsNullOrWhiteSpace( profile.AvailableOnlineAt )
					&& string.IsNullOrWhiteSpace( profile.AvailabilityListing )
					 )
				{
					//addresses probably are not in the profile at this point
					if ( profile.Id > 0 && AddressProfileManager.HasAddress( profile.RowId ) == false )
					{
						messages.Add( "At least one of: 'Available Online At', 'Availability Listing', or 'Available At' (address) must be entered" );
					}
				}

				if ( !string.IsNullOrWhiteSpace( profile.DateEffective ) && !IsValidDate( profile.DateEffective ) )
				{
					messages.Add( "Please enter a valid effective date" );
				}

				if ( validatingUrls )
				{
					if ( !IsUrlValid( profile.AvailabilityListing, ref commonStatusMessage ) )
					{
						messages.Add( "The Availability Listing Url is invalid. " + commonStatusMessage );
					}
					if ( !IsUrlValid( profile.AvailableOnlineAt, ref commonStatusMessage ) )
					{
						messages.Add( "The Available Online At Url is invalid. " + commonStatusMessage );
					}
					if ( !IsUrlValid( profile.ExternalResearch, ref commonStatusMessage ) )
						messages.Add( "The External Research Url is invalid. " + commonStatusMessage );
					if ( !IsUrlValid( profile.ProcessStandards, ref commonStatusMessage ) )
						messages.Add( "The Process Standards Url is invalid. " + commonStatusMessage );
					if ( !IsUrlValid( profile.ScoringMethodExample, ref commonStatusMessage ) )
						messages.Add( "The Scoring Method Example Url is invalid. " + commonStatusMessage );
					if ( !IsUrlValid( profile.AssessmentExample, ref commonStatusMessage ) )
						messages.Add( "The Assessment Example Url is invalid. " + commonStatusMessage );
				}

				if ( profile.CreditHourValue < 0 || profile.CreditHourValue > 10000 )
					messages.Add( "Error: invalid value for Credit Hour Value. Must be a reasonable decimal value greater than zero." );

				if ( profile.CreditUnitValue < 0 || profile.CreditUnitValue > 10000 )
					messages.Add( "Error: invalid value for Credit Unit Value. Must be a reasonable decimal value greater than zero." );


				//can only have credit hours properties, or credit unit properties, not both
				bool hasCreditHourData = false;
				bool hasCreditUnitData = false;
				if ( profile.CreditHourValue > 0 || ( profile.CreditHourType ?? "" ).Length > 0 )
					hasCreditHourData = true;
				if ( profile.CreditUnitTypeId > 0
					|| ( profile.CreditUnitTypeDescription ?? "" ).Length > 0
					|| profile.CreditUnitValue > 0 )
					hasCreditUnitData = true;

				if ( hasCreditHourData && hasCreditUnitData )
					messages.Add( "Error: Data can be entered for Credit Hour related properties or Credit Unit related properties, but not for both." );
			}

			if ( messages.Count > count )
				isValid = false;

			return isValid;
		}

		/// <summary>
		/// Update credential registry id, and set status published
		/// </summary>
		/// <param name="assessmentId"></param>
		/// <param name="envelopeId"></param>
		/// <param name="userId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool UpdateEnvelopeId( int assessmentId, string envelopeId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			bool updatingStatus = UtilityManager.GetAppKeyValue( "onRegisterSetEntityToPublic", false );
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					EM.Assessment efEntity = context.Assessment
									.SingleOrDefault( s => s.Id == assessmentId );

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
								string message = string.Format( thisClassName + ". UpdateEnvelopeId Failed", "Attempted to update an EnvelopeId. The process appeared to not work, but was not an exception, so we have no message, or no clue. assessment: {0}, envelopeId: {1}, updatedById: {2}", assessmentId, envelopeId, userId );
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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UpdateEnvelopeId(), assessment: {0}, envelopeId: {1}, updatedById: {2}", assessmentId, envelopeId, userId ) );
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
		public bool UnPublish( int assessmentId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					EM.Assessment efEntity = context.Assessment
									.SingleOrDefault( s => s.Id == assessmentId );

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
							string message = string.Format( thisClassName + ".UnPublish Failed", "Attempted to unpublish the Assessment. The process appeared to not work, but was not an exception, so we have no message, or no clue. AssessmentId: {0}, updatedById: {1}", assessmentId, userId );
							EmailManager.NotifyAdmin( thisClassName + ".UnPublish Failed", message );
						}
					}
					else
					{
						statusMessage = "Error - update failed, as record was not found.";
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UnPublish(), AssessmentId: {0}, updatedById: {1}", assessmentId, userId ) );
                    statusMessage = FormatExceptions( ex );
                }
			}

			return isValid;
		}
		/// <summary>
		/// Delete an Assessment, and related Entity
		/// </summary>
		/// <param name="Id"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( int Id, ref string statusMessage )
		{
			bool isValid = false;
			if ( Id == 0 )
			{
				statusMessage = "Error - missing an identifier for the Assessment";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					DBEntity efEntity = context.Assessment
								.SingleOrDefault( s => s.Id == Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{

						Guid rowId = efEntity.RowId;

						//need to remove from Entity.
						//could use a pre-delete trigger?
						//what about roles

						context.Assessment.Remove( efEntity );
						int count = context.SaveChanges();
						if ( count > 0 )
						{
							isValid = true;
							//do with trigger now
							///new EntityManager().Delete( rowId, ref statusMessage );
						}
					}
					else
					{
						statusMessage = "Error - delete failed, as record was not found.";
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + ".Assessment_Delete()" );

                    statusMessage = FormatExceptions( ex );
                    if ( statusMessage.ToLower().IndexOf( "the delete statement conflicted with the reference constraint" ) > -1 )
					{
						statusMessage = "Error: this assessment cannot be deleted as it is being referenced by other items, such as roles or assessments. These associations must be removed before this assessment can be deleted.";
					}
				}
			}
			return isValid;
		}

        public bool DeleteAllForOrganization( Guid owningOrgUid,ref List<string> messages )
        {
            bool isValid = true;
            Organization org = OrganizationManager.GetForSummary( owningOrgUid );
            if ( org == null || org.Id == 0 )
            {
                messages.Add( "Error - the provided organization was not found." );
                return false;
            }
            if ( UtilityManager.GetAppKeyValue( "envType" ) == "production" )
            {
                messages.Add( "Deleting all assessments for an organization is not allowed in this environment." );
                return false;
            }
            string sql = "";
            try
            {
                using ( var context = new EM.CTIEntities() )
                {
                    context.Assessment.RemoveRange( context.Assessment.Where( s => s.OwningAgentUid == owningOrgUid ) );
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        isValid = true;
                        messages.Add( string.Format( "removed {0} assessments.",count ) );
                    }

                    //this may not work, needs to be assessment specific
                    //sql = string.Format( "DELETE FROM [dbo].[Cache.Organization_ActorRoles]   WHERE OrganizationId = {0} ",org.Id );
                    sql = string.Format( "UPDATE [dbo].[Cache.Organization_ActorRoles] SET [AsmtsOwnedBy] = ''  WHERE OrganizationId = {0} ",org.Id );
                    context.Database.ExecuteSqlCommand( sql );
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex,thisClassName + "DeleteAllForOrganization" );
                messages.Add( ex.Message );
            }
            return isValid;
        }
        #region Assessment properties ===================
        public bool UpdateParts( ThisEntity entity, ref List<string> messages )
		{
			if ( entity.IsReferenceVersion )
				return true;

			bool isAllValid = true;
			EntityPropertyManager mgr = new EntityPropertyManager();
			Entity_ReferenceManager erm = new Entity_ReferenceManager();
			//CodesManager.PROPERTY_CATEGORY_ASSESSMENT_TYPE
			if ( mgr.UpdateProperties( entity.AssessmentMethodType, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, CodesManager.PROPERTY_CATEGORY_Assessment_Method_Type, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			if ( mgr.UpdateProperties( entity.AssessmentUseType, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, CodesManager.PROPERTY_CATEGORY_ASSESSMENT_USE_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			if ( mgr.UpdateProperties( entity.DeliveryType, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, CodesManager.PROPERTY_CATEGORY_DELIVERY_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			if ( mgr.UpdateProperties( entity.ScoringMethodType, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, CodesManager.PROPERTY_CATEGORY_Scoring_Method, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

            if ( mgr.UpdateProperties(entity.AudienceType, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE, entity.LastUpdatedById, ref messages) == false )
                isAllValid = false;

            if ( entity.OwnerRoles == null || entity.OwnerRoles.Items.Count == 0 )
			{
				messages.Add( "Invalid request, please select one or more roles for the owning agent." );
				isAllValid = false;
			} else {
				
				if ( entity.OwnerRoles.GetFirstItemId() != Entity_AgentRelationshipManager.ROLE_TYPE_OWNER )
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

            var lm = new Entity_LanguageManager();
            if ( lm.Update( entity.InLanguageCodeList, entity.RowId, entity.LastUpdatedById, ref messages ) == false )
                isAllValid = false;

            if ( erm.Update( entity.Subject, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_SUBJECT, false ) == false )
				isAllValid = false;

			if ( erm.Update( entity.Keyword, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_KEYWORD, false ) == false )
				isAllValid = false;

			if ( erm.Update( entity.AlternativeInstructionalProgramType, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_CIP, false ) == false )
				isAllValid = false;

			if ( erm.Update( entity.AlternativeIndustries, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_NAICS, false) == false )
				isAllValid = false;

			if ( erm.Update( entity.AlternativeOccupations, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_SOC, false ) == false )
				isAllValid = false;

			return isAllValid;
		}


		#endregion
		#endregion

		#region == Retrieval =======================
		public static ThisEntity GetBasic( int id )
		{
			ThisEntity entity = new ThisEntity();
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity item = context.Assessment
						.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB_Basic( item, entity, false, false );
				}
			}

			return entity;
		}
        public static ThisEntity GetBasic( Guid rowId )
        {
            ThisEntity entity = new ThisEntity();
            using ( var context = new Data.CTIEntities() )
            {
                DBEntity item = context.Assessment
                        .SingleOrDefault( s => s.RowId == rowId );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB_Basic( item, entity, false, false );
                }
            }

            return entity;
        }
        /// <summary>
        /// Get record and do validation for approvals
        /// </summary>
        /// <param name="id"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static ThisEntity GetForApproval( int id, ref List<string> messages )
        {
            ThisEntity entity = new ThisEntity();

            using ( var context = new Data.CTIEntities() )
            {
                DBEntity item = context.Assessment
                            .FirstOrDefault( s => s.Id == id
                                && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
                                );

                if ( item != null && item.Id > 0 )
                {
                    //??
                    MapFromDB_Basic( item, entity, false, false );
                    HasMinimumData( entity, ref messages, false );
                }
            }

            return entity;
        }
        public static List<ThisEntity> GetAllForOwningOrg( Guid owningOrgUid )
        {
            List<ThisEntity> list = new List<ThisEntity>();
            ThisEntity entity = new ThisEntity();
            using (var context = new Data.CTIEntities())
            {
                List<DBEntity> results = context.Assessment
                             .Where( s => s.OwningAgentUid == owningOrgUid )
                             .OrderBy( s => s.Name )
                             .ToList();
                if (results != null && results.Count > 0)
                {
                    foreach (DBEntity item in results)
                    {
                        entity = new ThisEntity();
                        MapFromDB_Basic( item, entity, false, false );
                        list.Add( entity );
                    }
                }
            }

            return list;
        }
        public static ThisEntity Get( int id, 
			bool forEditView = false, 
			bool includeWhereUsed = false)
		{
			ThisEntity entity = new ThisEntity();
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBEntity item = context.Assessment
							.SingleOrDefault( s => s.Id == id );

					if ( item != null && item.Id > 0 )
					{
						MapFromDB( item, entity,
								true, //includingProperties
								true, //includingRoles
								forEditView,
								includeWhereUsed );
					}
				}
			}catch(Exception ex)
			{
				LoggingHelper.LogError( ex, "AssessmentManager.Get(id)" );
			}
			return entity;
		}

        /// <summary>
        /// Get summary view of an assessment by ctid
        /// </summary>
        /// <param name="ctid"></param>
        /// <param name="includingRoles"></param>
        /// <returns></returns>
        public static ThisEntity GetByCtid( string ctid, bool forSummaryView = true )
        {

            ThisEntity to = new ThisEntity();

            using (var context = new Data.CTIEntities())
            {
                //assessment should have few child entities retrieved, allow lazy
                //context.Configuration.LazyLoadingEnabled = false;
                DBEntity from = context.Assessment
                        .SingleOrDefault( s => s.CTID == ctid
                        && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
                        );

                if (from != null && from.Id > 0)
                {
                    MapFromDB_Basic( from, to, false, false,forSummaryView );
                }
            }

            return to;
        }
        public static ThisEntity GetBasicByUniqueId(string identifier, Guid owningOrgUid)
        {

            ThisEntity to = new ThisEntity();
            if ( string.IsNullOrWhiteSpace(identifier) )
                return to;

            using ( var context = new Data.CTIEntities() )
            {
                context.Configuration.LazyLoadingEnabled = false;
                var from = context.Assessment
                            .FirstOrDefault(s => s.ExternalIdentifier.ToLower() == identifier.ToLower()
                               && s.OwningAgentUid == owningOrgUid
                               && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
                                );

                if ( from != null && from.Id > 0 )
                {
                    MapFromDB_Basic(from, to, false, false);
                }
            }

            return to;
        }
        public static ThisEntity GetByName( string name, ref string status )
        {

            ThisEntity to = new ThisEntity();

            using (var context = new Data.CTIEntities())
            {
                context.Configuration.LazyLoadingEnabled = false;
                //can be many, so use list and reject if multiple
                List<EM.Assessment> results = context.Assessment
                        .Where( s => s.Name.ToLower() == name.ToLower()
                        && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
                        ).ToList();

                if (results != null)
                {
                    if (results.Count == 1)
                    {
                        MapFromDB_Basic( results[ 0 ], to, false, false );
                    }
                    else if (results.Count > 1)
                        status = "Error - there are mulitple assessments with the name: " + name + ". Please ensure a unique organization name is used, or use a CTID for an existing organization instead.";
                }
            }

            return to;
        }

        public static ThisEntity GetForPublish( int id)
		{
			ThisEntity entity = new ThisEntity();
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity item = context.Assessment
						.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity,
							true, //includingProperties
							true, //includingRoles
							false, //forEditView,
							true );	//includeWhereUsed );
				}
			}

			return entity;
		}
		public static List<string> Autocomplete( string pFilter, int pageNumber, int pageSize, int userId, ref int pTotalRows )
		{
			bool autocomplete = true;
			List<string> results = new List<string>();
			List<string> competencyList = new List<string>();
			//ref competencyList, 
			List<ThisEntity> list = Search( pFilter, "", pageNumber, pageSize, userId, ref pTotalRows, autocomplete );
			bool appendingOrgNameToAutocomplete = UtilityManager.GetAppKeyValue( "appendingOrgNameToAutocomplete", false );
			string prevName = "";
			foreach ( AssessmentProfile item in list )
			{
				//note excluding duplicates may have an impact on selected max terms
				//if ( item.Name.ToLower() != prevName )
				//	results.Add( item.Name );

				if ( string.IsNullOrWhiteSpace( item.OrganizationName )
					|| !appendingOrgNameToAutocomplete )
				{
					if ( item.Name.ToLower() != prevName )
						results.Add( item.Name );
				}
				else
				{
					results.Add( item.Name + " ('" + item.OrganizationName + "')" );
				}

				prevName = item.Name.ToLower();
			}
			return results;
		}
		/// <summary>
		/// Search for assessments
		/// </summary>
		/// <returns></returns>
		public static List<ThisEntity> QuickSearch( int userId, string keyword, int pageNumber, int pageSize, ref int pTotalRows )
		{
			List<ThisEntity> list = new List<ThisEntity>();
			ThisEntity entity = new ThisEntity();
			keyword = string.IsNullOrWhiteSpace( keyword ) ? "" : keyword.Trim();
			if ( pageSize == 0 )
				pageSize = 500;
			int skip = 0;
			if ( pageNumber > 1 )
				skip = ( pageNumber - 1 ) * pageSize;

			using ( var context = new Data.CTIEntities() )
			{
				var Query = from Results in context.Assessment
						.Where( s => keyword == "" || s.Name.Contains( keyword ) )
						.OrderBy( s => s.Name )
						select Results;
				pTotalRows = Query.Count();
				var results = Query.Skip(skip).Take( pageSize )
					.ToList();

				//List<DBEntity> results = context.Assessment
				//	.Where( s => keyword == "" || s.Name.Contains( keyword ) )
				//	.Take( pageSize )
				//	.OrderBy( s => s.Name )
				//	.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( DBEntity item in results )
					{
						entity = new ThisEntity();
						MapFromDB( item, entity,
								false, //includingProperties
								false, //includingRoles
								false, //forEditView
								false //includeWhereUsed
								 );
						list.Add( entity );
					}

					//Other parts
				}
			}

			return list;
        }
        public static List<ThisEntity> SearchByUrl(string subjectWebpage, string pOrderBy, int pageNumber, int pageSize, ref int pTotalRows, int userId = 0, bool autocomplete = false)
        {
            string url = NormalizeUrlData(subjectWebpage);
            //skip if an example url
            string filter = string.Format(" ( base.Id in (Select Id from Assessment where (Url like '{0}%') )) ", url);
            int ptotalRows = 0;
            var exists = Search(filter, "", 1, 100, userId, ref ptotalRows);
            return exists;
        }
        public static List<ThisEntity> Search( BaseSearchModel bsm, ref int pTotalRows )
        {
            //not sure why we have two separate methods that use the same proc
            return Search( bsm.Filter, bsm.OrderBy, bsm.PageNumber, bsm.PageSize, bsm.UserId, ref pTotalRows, bsm.IsAutocomplete );

        }

        public static List<ThisEntity> Search( string pFilter, string pOrderBy, int pageNumber, int pageSize, int userId, ref int pTotalRows, bool autocomplete = false )
		{
			string connectionString = DBConnectionRO();
			ThisEntity item = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			var result = new DataTable();
			string temp = "";
			string org = "";
			int orgId = 0;

			using ( SqlConnection c = new SqlConnection( connectionString ) )
			{
				c.Open();

				if ( string.IsNullOrEmpty( pFilter ) )
				{
					pFilter = "";
				}

				using ( SqlCommand command = new SqlCommand( "[Assessment_Search]", c ) )
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
                    catch (Exception ex)
                    {
                        pTotalRows = 0;
                        LoggingHelper.LogError(ex, thisClassName + string.Format(".Search() - Execute proc, Message: {0} \r\n Filter: {1} \r\n", ex.Message, pFilter));

                        item = new AssessmentProfile();
                        item.Name = "Unexpected error encountered. System administration has been notified. Please try again later. ";
                        item.Description = ex.Message;

                        list.Add(item);
                        return list;
                    }
                }

				foreach ( DataRow dr in result.Rows )
				{
					item = new ThisEntity();
					item.Id = GetRowColumn( dr, "Id", 0 );
					item.Name = GetRowColumn( dr, "Name", "missing" );
					item.FriendlyName = FormatFriendlyTitle( item.Name );

					item.ManagingOrganization = GetRowPossibleColumn( dr, "ManagingOrganization", "" );
					item.ManagingOrgId = GetRowPossibleColumn( dr, "ManagingOrgId", 0 );
					org = GetRowPossibleColumn( dr, "Organization", "" );
					orgId = GetRowPossibleColumn( dr, "OrgId", 0 );
					if ( orgId > 0 )
					{
						item.OwningOrganization = new Organization() { Id = orgId, Name = org };
					}
                    item.ctid = GetRowPossibleColumn( dr, "CTID", "" );
                    if ( string.IsNullOrWhiteSpace( item.CTID ) )
                        item.IsReferenceVersion = true;

					item.Description = GetRowColumn( dr, "Description", "" );
					string rowId = GetRowColumn( dr, "RowId" );
					item.RowId = new Guid( rowId );
					rowId = GetRowColumn( dr, "OwningAgentUid" );
					if ( IsValidGuid( rowId ) )
						item.OwningAgentUid = new Guid( rowId );

					item.CredentialRegistryId = GetRowPossibleColumn( dr, "CredentialRegistryId", "" );

                    //for autocomplete, only need name
                    if ( autocomplete )
					{
						list.Add( item );
						continue;
					}



					item.SubjectWebpage = GetRowColumn( dr, "URL", "" );
					item.AvailableOnlineAt = GetRowPossibleColumn( dr, "AvailableOnlineAt", "" );
					item.CanEditRecord = GetRowColumn( dr, "CanEditRecord", false );
					item.StatusId = GetRowColumn( dr, "StatusId", 1 );
					item.CodedNotation = GetRowColumn( dr, "IdentificationCode", "" );
					

                    item.EntityLastUpdated = GetRowColumn(dr, "EntityLastUpdated", System.DateTime.Now);
                    item.LastPublishDate = GetRowPossibleColumn(dr, "LastPublishDate", "");
                    DateTime testdate;
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

					temp = GetRowColumn( dr, "DateEffective", "" );
					if ( IsValidDate( temp ) )
						item.DateEffective = DateTime.Parse( temp ).ToShortDateString();
					else
						item.DateEffective = "";

					item.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
					item.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );

					//addressess
					int addressess = GetRowPossibleColumn( dr, "AvailableAddresses", 0 );
					if ( addressess > 0 )
					{
						item.Addresses = AddressProfileManager.GetAll( item.RowId );
					}
					//not used yet
					item.AssessesCompetenciesCount = GetRowPossibleColumn( dr, "Competencies", 0 );

                    string subjects = GetRowPossibleColumn( dr, "SubjectsList", "" );
                    if (!string.IsNullOrWhiteSpace(subjects))
                    {
                        var codeGroup = subjects.Split('|');
                        foreach (string codeSet in codeGroup)
                        {
                            var codes = codeSet.Split('~');
                            item.Subjects.Add(codes[0].Trim());
                        }
                    }

                    item.AssessmentMethodTypes = Fill_CodeItemResultsFromXml(dr, "AssessmentMethodTypes", CodesManager.PROPERTY_CATEGORY_Assessment_Method_Type, false, false);

                    item.AssessmentUseTypes = Fill_CodeItemResultsFromXml(dr, "AssessmentUseTypes", CodesManager.PROPERTY_CATEGORY_ASSESSMENT_USE_TYPE, false, false);

                    item.DeliveryMethodTypes = Fill_CodeItemResultsFromXml(dr, "DeliveryMethodTypes", CodesManager.PROPERTY_CATEGORY_DELIVERY_TYPE, false, false);

                    item.ScoringMethodTypes = Fill_CodeItemResultsFromXml(dr, "ScoringMethodTypes", CodesManager.PROPERTY_CATEGORY_Scoring_Method, false, false);

					item.IndustryResults = Fill_CodeItemResultsFromXml( dr, "Frameworks", CodesManager.PROPERTY_CATEGORY_NAICS, true, true );
					item.IndustryOtherResults = Fill_CodeItemResults( dr, "OtherFrameworks", CodesManager.PROPERTY_CATEGORY_NAICS, false, false, false );

					//OccupationsCSV
					item.OccupationResults = Fill_CodeItemResultsFromXml( dr, "Frameworks", CodesManager.PROPERTY_CATEGORY_SOC, true, true );
					item.OccupationOtherResults = Fill_CodeItemResults( dr, "OtherFrameworks", CodesManager.PROPERTY_CATEGORY_SOC, false, false, false );

					item.InstructionalProgramResults = Fill_CodeItemResultsFromXml( dr, "Frameworks", CodesManager.PROPERTY_CATEGORY_CIP, false, false );
					item.OtherInstructionalProgramResults = Fill_CodeItemResults( dr, "OtherFrameworks", CodesManager.PROPERTY_CATEGORY_CIP, false, false, false );


					item.QualityAssurance = Fill_AgentItemResultsFromXml( dr, "QualityAssurance", CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE, false, false );

                    //IN PROGRESS
                    item.AssessmentConnectionsList = Fill_CredentialConnectionsResult( dr, "ConnectionsList", CodesManager.PROPERTY_CATEGORY_CONNECTION_PROFILE_TYPE );

                    string relatedItems = GetRowColumn( dr, "CommonCosts" );
                    string[] array = relatedItems.Split( ',' );
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
		} //

        public static List<Dictionary<string, object>> GetAllForExport_DictionaryList(string owningOrgUid, bool includingConditionProfile = true)
        {
            //
            var result = new List<Dictionary<string, object>>();
            var table = GetAllForExport_DataTable(owningOrgUid, includingConditionProfile);

            foreach (DataRow dr in table.Rows)
            {
                var rowData = new Dictionary<string, object>();
                for (var i = 0; i < dr.ItemArray.Count(); i++)
                {
                    rowData[table.Columns[i].ColumnName] = dr.ItemArray[i];
                }
                result.Add(rowData);
            }
            return result;
        }
        //
        public static DataTable GetAllForExport_DataTable(string owningOrgUid, bool includingConditionProfile)
        {
            var result = new DataTable();
            string connectionString = DBConnectionRO();
            //
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                c.Open();
                using (SqlCommand command = new SqlCommand("[Assessments_Export]", c))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@OwningOrgUid", owningOrgUid));

                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter())
                        {
                            adapter.SelectCommand = command;
                            adapter.Fill(result);
                        }

                    }
                    catch (Exception ex)
                    {
                        LoggingHelper.LogError(ex, thisClassName + string.Format(".GetAllForExport_DataTable() - Execute proc, Message: {0} \r\n owningOrgUid: {1} ", ex.Message, owningOrgUid));
                    }
                }
            }
            return result;
        }

        public static ThisEntity GetByNameAndUrl( string name, string url, ref string status )
        {
            ThisEntity to = new ThisEntity();
            //warning the trailing slash is trimmed during save so need to handle, or do both

            if ( url.EndsWith( "/" ) )
                url = url.TrimEnd( '/' );

            using ( var context = new Data.CTIEntities() )
            {
                context.Configuration.LazyLoadingEnabled = false;
                //can be many, so use list and reject if multiple
                List<DBEntity> results = context.Assessment
                        .Where( s => s.Name.ToLower() == name.ToLower()
                        && ( s.Url.ToLower() == url.ToLower() )
                        && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
                        ).ToList();

                if ( results != null )
                {
                    if ( results.Count == 1 )
                        MapFromDB_Basic( results[ 0 ], to, false, false );
                    else if ( results.Count > 1 )
                        status = "Error - there are mulitple assessments with the name: " + name + ". Please ensure a unique assessment name is used, or use a CTID for an existing assessment instead.";
                }
            }

            return to;
        }

        public static void MapToDB( ThisEntity from, DBEntity to )
		{

			//want to ensure fields from create are not wiped
			if ( to.Id == 0 )
			{
				to.ExternalIdentifier = string.IsNullOrWhiteSpace( from.ExternalIdentifier ) ? null : from.ExternalIdentifier;
			}
			else
			{
				if ( string.IsNullOrWhiteSpace( to.ExternalIdentifier ) )
				{
					to.ExternalIdentifier = string.IsNullOrWhiteSpace( from.ExternalIdentifier ) ? null : from.ExternalIdentifier;
				}
			}

			to.Id = from.Id;
			to.Name = ConvertSpecialCharacters(from.Name);
            //to.StatusId = from.StatusId > 0 ? from.StatusId : ( to.StatusId > 0 ? to.StatusId : 1 );

            to.Description = ConvertSpecialCharacters( from.Description );
            to.Url = NormalizeUrlData( from.SubjectWebpage );

			//generally the managing orgId should not be allowed to change in the interface - yet
			if ( from.ManagingOrgId > 0
				&& from.ManagingOrgId != ( to.ManagingOrgId ?? 0 ) )
				to.ManagingOrgId = from.ManagingOrgId;

			if ( from.IsReferenceVersion )
				return;

			to.IdentificationCode = GetData( from.CodedNotation );
			to.VersionIdentifier = GetData( from.VersionIdentifier );
			
			//to.OtherAssessmentType = GetData( from.OtherAssessmentType );

			to.AvailableOnlineAt = NormalizeUrlData( from.AvailableOnlineAt );
			to.AvailabilityListing = NormalizeUrlData( from.AvailabilityListing );
			to.AssessmentExampleUrl = GetData(from.AssessmentExample);
			
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

			//if ( from.InLanguageId > 0 )
			//	to.InLanguageId = from.InLanguageId;
			//else
			//	to.InLanguageId = null;

            to.CreditHourType = GetData( from.CreditHourType, null );
			to.CreditHourValue = SetData( from.CreditHourValue, 0.5M );
			to.CreditUnitTypeId = SetData( from.CreditUnitTypeId, 1 );
			to.CreditUnitTypeDescription = ConvertSpecialCharacters( from.CreditUnitTypeDescription );
			to.CreditUnitValue = SetData( from.CreditUnitValue, 0.5M );

			to.DeliveryTypeDescription = ConvertSpecialCharacters(from.DeliveryTypeDescription);
			to.VerificationMethodDescription = ConvertSpecialCharacters(from.VerificationMethodDescription);
			to.AssessmentExampleDescription = ConvertSpecialCharacters(from.AssessmentExampleDescription);
			to.AssessmentOutput = from.AssessmentOutput;
			to.ExternalResearch = from.ExternalResearch;

            to.IsProctored = from.IsProctored;
            to.HasGroupEvaluation = from.HasGroupEvaluation;
			to.HasGroupParticipation = from.HasGroupParticipation;

			to.ProcessStandards = from.ProcessStandards;
			to.ProcessStandardsDescription = ConvertSpecialCharacters(from.ProcessStandardsDescription);

			to.ScoringMethodDescription = ConvertSpecialCharacters(from.ScoringMethodDescription);
			to.ScoringMethodExample = from.ScoringMethodExample;
			to.ScoringMethodExampleDescription = ConvertSpecialCharacters(from.ScoringMethodExampleDescription);

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = DateTime.Parse( from.DateEffective );
			else
				to.DateEffective = null;
		}

		public static void MapFromDB( DBEntity from, ThisEntity to, 
				bool includingProperties, 
				bool includingRoles, 
				bool forEditView,
				bool includeWhereUsed)
		{
			MapFromDB_Basic( from, to, true, forEditView );
			if ( to.IsReferenceVersion )
				return;

			//===============================================================

			//to.ResourceUrl = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS );

			
			to.AssessmentExample = from.AssessmentExampleUrl;
			to.AssessmentExampleDescription = from.AssessmentExampleDescription;
			
			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective =( (DateTime) from.DateEffective).ToShortDateString();
			else
				to.DateEffective = "";
			
			to.CodedNotation = from.IdentificationCode;
			


            to.CreditHourType = from.CreditHourType ?? "";
			to.CreditHourValue = ( from.CreditHourValue ?? 0M );
			to.CreditUnitTypeId = ( from.CreditUnitTypeId ?? 0 );
			to.CreditUnitTypeDescription = from.CreditUnitTypeDescription;
			to.CreditUnitValue = from.CreditUnitValue ?? 0M;

			// Begin edits - Need these to populate Credit Unit Type -  NA 3/24/2017
			if ( to.CreditUnitTypeId > 0 )
			{
				to.CreditUnitType = new Enumeration();
				var match = CodesManager.GetEnumeration( "creditUnit" ).Items.FirstOrDefault( m => m.CodeId == to.CreditUnitTypeId );
				if ( match != null )
				{
					to.CreditUnitType.Items.Add( match );
				}
			}
			to.DeliveryTypeDescription = from.DeliveryTypeDescription;
			to.VerificationMethodDescription = from.VerificationMethodDescription;
			
			to.AssessmentOutput = from.AssessmentOutput;
			to.ExternalResearch = from.ExternalResearch;
			if ( from.HasGroupEvaluation != null )
				to.HasGroupEvaluation = (bool)from.HasGroupEvaluation;
			if ( from.HasGroupParticipation != null )
				to.HasGroupParticipation = ( bool ) from.HasGroupParticipation;
			if ( from.IsProctored != null )
				to.IsProctored = ( bool ) from.IsProctored;

			to.ProcessStandards = from.ProcessStandards;
			to.ProcessStandardsDescription = from.ProcessStandardsDescription;

			to.ScoringMethodDescription = from.ScoringMethodDescription;
			to.ScoringMethodExample = from.ScoringMethodExample;
			to.ScoringMethodExampleDescription = from.ScoringMethodExampleDescription;
			
			to.Subject = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SUBJECT );

			to.Keyword = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD );
			//properties
			if ( includingProperties )
			{
				//FillAssessmentType( from, to );
				//FillModalityType( from, to );
				to.AssessmentMethodType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_Assessment_Method_Type );

				to.AssessmentUseType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_ASSESSMENT_USE_TYPE );

				to.DeliveryType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_DELIVERY_TYPE );

				to.ScoringMethodType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_Scoring_Method );

                to.AudienceType = EntityPropertyManager.FillEnumeration(to.RowId, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE);

                to.Addresses = AddressProfileManager.GetAll( to.RowId );

				//this is in MapFromDB_Basic
				//to.EstimatedCost = CostProfileManager.GetAll( to.RowId, forEditView );

			}
			//get competencies
			MapFromDB_Competencies( to );

			to.Occupation = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_SOC );
			to.AlternativeOccupations = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SOC );

			to.Industry = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );
			to.AlternativeIndustries = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );

			to.InstructionalProgramType = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CIP );
			to.AlternativeInstructionalProgramType = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_CIP );

			if (includingRoles) 
			{
				if ( forEditView )
				{
					//just get profile links
					//to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAllSummary( to.RowId, false );

					to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAllExceptOwnerSummary( to.RowId, to.OwningAgentUid, false, false );
					//USING OwnerRoles, not OwnerOrganizationRoles for edit
					//to.OwnerOrganizationRoles = Entity_AgentRelationshipManager.AgentEntityRole_GetOwnerSummary( to.RowId, to.OwningAgentUid, false );
				}
				else
				{
					//get as ennumerations
					to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAll_ToEnumeration( to.RowId, true );
				}
				//to.QualityAssuranceAction =	Entity_QualityAssuranceActionManager.QualityAssuranceActionProfile_GetAll( to.RowId );

			to.EstimatedDuration = DurationProfileManager.GetAll( to.RowId );

			to.Jurisdiction = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId );
				to.Region = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_RESIDENT );
				to.JurisdictionAssertions = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_OFFERREDIN );

			}


			//get condition profiles
			List<ConditionProfile> list = new List<ConditionProfile>();
			if ( forEditView )
			{
				list = Entity_ConditionProfileManager.GetAllForLinks( to.RowId );
				to.CommonConditions = Entity_CommonConditionManager.GetAll( to.RowId, true );
				//handled in MapFromDB_Basic
				//to.CommonCosts = Entity_CommonCostManager.GetAll( to.RowId, forEditView );
			}
			else
			{
				//get all, including connections
				list = Entity_ConditionProfileManager.GetAll( to.RowId, 0 );
				to.CommonConditions = Entity_CommonConditionManager.GetAll( to.RowId, false );
				//handled in MapFromDB_Basic
				//to.CommonCosts = Entity_CommonCostManager.GetAll( to.RowId, forEditView );
			}
			if ( list != null && list.Count > 0 )
			{
				foreach ( ConditionProfile item in list )
				{
                    to.AllConditions.Add(item);

                    if ( item.RequiresCompetenciesFrameworks.Count > 0 )
                        to.RequiresCompetenciesFrameworks.AddRange( item.RequiresCompetenciesFrameworks );

                    if ( item.ConditionSubTypeId == Entity_ConditionProfileManager.ConditionSubType_Assessment )
					{
						to.AssessmentConnections.Add( item );
					}
					else if ( item.ConnectionProfileTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Requirement )
						to.Requires.Add( item );
					else if ( item.ConnectionProfileTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Recommendation )
						to.Recommends.Add( item );
					else if ( item.ConnectionProfileTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Corequisite )
						to.Corequisite.Add( item );
					else if ( item.ConnectionProfileTypeId == Entity_ConditionProfileManager.ConnectionProfileType_EntryCondition )
						to.EntryCondition.Add( item );
					else
					{
						EmailManager.NotifyAdmin( "Unexpected Condition Profile for assessment", string.Format( "AssessmentId: {0}, ConditionProfileTypeId: {1}", to.Id, item.ConnectionProfileTypeId ) );

						//add to required, for dev only?
						if ( IsDevEnv() )
						{
							item.ProfileName = ( item.ProfileName ?? "" ) + " unexpected condition type of " + item.ConnectionProfileTypeId.ToString();
							to.Requires.Add( item );
						}
					}
				}

				
			}

			//to.AssessmentProcess = Entity_ProcessProfileManager.GetAll( to.RowId, true );
			List<ProcessProfile> processes = Entity_ProcessProfileManager.GetAll( to.RowId, forEditView );
			foreach ( ProcessProfile item in processes )
			{
				if ( item.ProcessTypeId == Entity_ProcessProfileManager.ADMIN_PROCESS_TYPE )
					to.AdministrationProcess.Add( item );
				else if ( item.ProcessTypeId == Entity_ProcessProfileManager.DEV_PROCESS_TYPE )
					to.DevelopmentProcess.Add( item );
				else if ( item.ProcessTypeId == Entity_ProcessProfileManager.MTCE_PROCESS_TYPE )
					to.MaintenanceProcess.Add( item );
				else
				{
					//unexpected
				}
			}

			
		}
		private static void AddCredentialReference( int credentialId, ThisEntity to )
		{
			Credential exists = to.IsPartOfCredential.SingleOrDefault( s => s.Id == credentialId );
			if ( exists == null || exists.Id == 0 )
                    to.IsPartOfCredential.Add( CredentialManager.GetBasic( ( int ) credentialId ) );
        } //

        public static void MapFromDB_ForSummary(DBEntity from, ThisEntity to)
        {
            MapFromDB_Basic(from, to, false, false, true );
        } //

		public static void MapFromDB_Basic( DBEntity from, ThisEntity to, 
			bool includingCosts, 
			bool forEditView,
            bool forSummaryView = false)
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.StatusId = from.StatusId ?? 1;
			
			to.Name = from.Name;
            to.Description = from.Description;
            to.SubjectWebpage = from.Url;
			to.ctid = from.CTID;

			to.ManagingOrgId = from.ManagingOrgId ?? 0;
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
                if ( to.LastUpdatedById > 0 )
                {
                    AppUser user = AccountManager.AppUser_Get( to.LastUpdatedById );
                    to.LastUpdatedBy = user.FullName();
                }
			}
            to.CredentialRegistryId = from.CredentialRegistryId;

            if ( string.IsNullOrWhiteSpace( to.ctid ) )
			{
				to.IsReferenceVersion = true;
				return;
			}

            if ( IsGuidValid(from.OwningAgentUid) )
            {
                to.OwningAgentUid = ( Guid )from.OwningAgentUid;
                to.OwningOrganization = OrganizationManager.GetForSummary(to.OwningAgentUid, false);

                //get roles
                OrganizationRoleProfile orp = Entity_AgentRelationshipManager.AgentEntityRole_GetAsEnumerationFromCSV(to.RowId, to.OwningAgentUid);
                to.OwnerRoles = orp.AgentRole;
            }
            if ( forSummaryView )
                return;

			//======================================================

			to.RelatedEntity = EntityManager.GetEntity( to.RowId, false );
			if ( to.RelatedEntity != null && to.RelatedEntity.Id > 0 )
				to.EntityLastUpdated = to.RelatedEntity.LastUpdated;

			to.EntityApproval = Entity_ApprovalManager.GetByParent( to.RowId );
            if ( to.EntityApproval != null && to.EntityApproval.Id > 0 )
                to.LastApproved = to.EntityApproval.Created;

            //multiple languages, soon in entity.reference
            to.InLanguageCodeList = Entity_LanguageManager.GetAll( to.RowId );

            //short term convenience
            //if ( to.InLanguageCodeList != null && to.InLanguageCodeList.Count > 0 )
            //    to.InLanguage = to.InLanguageCodeList[ 0 ].LanguageName;


			to.VersionIdentifier = from.VersionIdentifier;
            to.AvailabilityListing = from.AvailabilityListing;
            to.AvailableOnlineAt = from.AvailableOnlineAt;

            //costs may be required for the list view, when called by the credential editor
            //make configurable
            if ( includingCosts )
			{
				to.EstimatedCost = CostProfileManager.GetAll( to.RowId, forEditView );
				to.CommonCosts = Entity_CommonCostManager.GetAll( to.RowId, forEditView );

				//Include currencies to fix null errors in various places (detail, compare, publishing) - NA 3/17/2017
				var currencies = CodesManager.GetCurrencies();
				//Include cost types to fix other null errors - NA 3/31/2017
				var costTypes = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_CREDENTIAL_ATTAINMENT_COST );
				foreach ( var cost in to.EstimatedCost )
				{
					cost.CurrencyTypes = currencies;

					foreach ( var costItem in cost.Items )
					{
                        int index = costItem.CostType.Items.FindIndex( a => a.Id == costItem.CostTypeId );
                        if ( index < 0 )
                            costItem.CostType.Items.Add( costTypes.Items.FirstOrDefault( m => m.CodeId == costItem.CostTypeId ) );
					}
				}
				//End edits - NA 3/31/2017
			}

			//Need this for the detail page, since we now show durations by profile name - NA 4/13/2017
			to.EstimatedDuration = DurationProfileManager.GetAll( to.RowId );

			to.FinancialAssistance = Entity_FinancialAlignmentProfileManager.GetAll( to.RowId, forEditView );


            to.WhereReferenced = new List<string>();
            //including with edit for now
            //we need to populate isRequired/isRecommendedFor
            //18-07-20 mp - may not want this for basic - i.e. particate example!
            //              - also the count for this was double the actual!
            if ( from.Entity_Assessment != null && from.Entity_Assessment.Count > 0 )
            {
                foreach ( EM.Entity_Assessment item in from.Entity_Assessment )
                {
                    to.WhereReferenced.Add( string.Format( "EntityUid: {0}, Type: {1}", item.Entity.EntityUid, item.Entity.Codes_EntityType.Title ) );
                    //only parent for now
                    if ( item.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_CONNECTION_PROFILE )
                    {
                        ConditionProfile cp = CondProfileMgr.GetAs_IsPartOf( item.Entity.EntityUid, false );
                        to.IsPartOfConditionProfile.Add( cp );

                        //need to check cond prof for parent of credential
                        //will need to ensure no dups, or realistically, don't do the direct credential check
                        if ( cp.ParentCredential != null && cp.ParentCredential.Id > 0 )
                        {
                            AddCredentialReference( cp.ParentCredential.Id, to );
                        }

                        //NOTE: if condition is for a condition manifest, this condition would be OK - need to handle! - even though unlikely?
                    }
                    else if ( item.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE )
                    {
                        to.IsPartOfLearningOpp.Add( LearningOpportunityManager.GetAs_IsPartOf( item.Entity.EntityUid, forEditView ) );
                    }
                    else if ( item.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_CREDENTIAL )
                    {
                        //if only directly under a credential, not valid for publishing
                        //want to exclude from the detail page.
                        //18-05-11 - confusing if shown, need to be clear if used, so only show via the condition profile
                        //                 if (forEditView)
                        //AddCredentialReference( (int)item.Entity.EntityBaseId, to );

                    }
                }
            }

        } //

		public static void MapFromDB_Competencies( ThisEntity to )
		{

            to.AssessesCompetenciesFrameworks = Entity_CompetencyManager.GetAll( to.RowId, "assesses" );
            //=========== OLD ==========================================
            //to.AssessesCompetenciesFrameworks = Entity_CompetencyFrameworkManager.GetAll( to.RowId, "assesses" );
			if ( to.AssessesCompetenciesFrameworks.Count > 0 )
				to.HasCompetencies = true;
            //the required competencies are under condition profiles
			//to.RequiresCompetenciesFrameworks = Entity_CompetencyFrameworkManager.GetAll( to.RowId, "requires" );
			if ( to.RequiresCompetenciesFrameworks.Count > 0 )
				to.HasCompetencies = true;

            //=========== NEW ==========================================
            //to.AssessesCompetenciesFrameworks = Entity_CompetencyManager.GetAllAsAlignmentObjects( to.RowId, "Assesses" );

        }
		#endregion

	}
}
