using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Models;
using Models.Common;
using Models.Import;
using Models.ProfileModels;
using Utilities;

namespace Factories.Import
{
    public class CredentialImport : BaseFactory
    {
		#region module properties
		private readonly static int CurrentEntityTypeId = 1;
        //CredentialManager cmgr = new CredentialManager( isBulkUploadTransaction: true).SetUploadContext;
        CredentialManager cmgr = new CredentialManager();

        ActivityManager activityMgr = new ActivityManager();
        AddressProfileManager addrMgr = new AddressProfileManager();
        AssessmentManager amgr = new AssessmentManager();
        CostProfileManager costProfileManager = new CostProfileManager();
        CostProfileItemManager costProfileItemManager = new CostProfileItemManager();
        DurationProfileManager dpmgr = new DurationProfileManager();
        //initial 
        Entity_AssessmentManager eaMgr = new Entity_AssessmentManager( returnErrorOnDuplicate: false );
        Entity_AgentRelationshipManager earMgr = new Entity_AgentRelationshipManager();
        Entity_CommonConditionManager eCommonCndsMgr = new Entity_CommonConditionManager();
        Entity_CommonCostManager eCommonCostsMgr = new Entity_CommonCostManager();
        Entity_ConditionProfileManager cpMgr = new Entity_ConditionProfileManager();
        Entity_FrameworkItemManager efimgr = new Entity_FrameworkItemManager();
        Entity_ReferenceManager erefMgr = new Entity_ReferenceManager();
        ImportHelpers helpers = new ImportHelpers();

        Enumeration credActiveStatus = new Enumeration();
        //public bool IsPartialUpdate = false;

        CodeItem credTypeCode = new CodeItem();
        CodeItem prevCredTypeCode = new CodeItem();
        Enumeration eProperty = new Enumeration();
        int englishCodeId = 40;
        public string DELETE_ME = "#DELETE";
        public int AddCount = 0;
        public int UpdateCount = 0;
        public int ErrorCount = 0; //confirm if really errors, or maybe separate warnings count
        //start at one so that no confusion versus header row. 
        private int CurrentRowNbr = 1;
        public Organization owningOrg = new Organization();
        #endregion

        public CredentialImport()
        {
            cmgr.SetUploadContext();
        }
        public void Import( CredentialImportRequest importHelper, List<CredentialDTO> inputList, AppUser user, ref ImportStatus status )
        {
			//bool isPartialUpdate, 
			//IsPartialUpdate = isPartialUpdate;
			ImportHelpers.CurrentEntityType = "Credential";
			CommonSetup();
            List<string> messages = new List<string>();
			string statusMessage = "";
			Credential credential = new Credential();
            int previousRecordId = 0;
			ParentObject po = new ParentObject();
			foreach (var request in inputList)
            {
                CurrentRowNbr++;
                messages = new List<string>();
				if ( CurrentRowNbr % 50 == 0 )
				{

				}
				if ( CurrentRowNbr == 6 )
                {

                }
                //handle credential - must exist for partial
                bool recordExists = false;
                LoggingHelper.DoTrace( 6, string.Format( "CredentialImport.Import. Row: {0}, Name: {1}", CurrentRowNbr, request.Name ) );
                //create credential
                credential = new Credential();
                try
                {
                    //the upload method now checks if credential exists, so we should know by now.
                    //TODO - handle duplicate rows (say due to multiple conditions, by checking if current credential equals previous one
                    //first proposal will be to skip the body updates, and just do the profiles
                    if ( !request.FoundExistingRecord || 
                        ( request.FoundExistingRecord && previousRecordId != request.ExistingRecord.Id ))
                    {
                        if ( HandleCredential( importHelper, request, ref credential, user, ref recordExists, ref status ) < 1 )
                        {
                            status.RecordsFailed++;
                            continue;
                        }
                        else
                        {
                            if ( recordExists )
                                status.RecordsUpdated++;
                            else
                                status.RecordsAdded++;
                        }
                    }

                    previousRecordId = credential.Id;
					po = new ParentObject() { Id = credential.Id, RowId = credential.RowId, Name = credential.Name, OwningAgentUid = credential.OwningAgentUid, ParentTypeId = 1, EntityType = "Credential" };
					
					if ( request.HasConditionProfile )
                    {
						LoggingHelper.DoTrace( 9, string.Format( "CredentialImport.Import. Row: {0} ___ condition profile", CurrentRowNbr, request.Name ) );
						//if ( !HandleConditions( request, ref credential, user.Id, recordExists, ref status ) )
						//{
						//    //actions?
						//}
						List<ConditionProfile> allNonConnectionConditions = credential.AllConditions.Where( n => n.ConditionSubTypeId == 1 ).ToList();
						helpers.HandleConditionProfiles( request.ConditionProfiles, po, allNonConnectionConditions, user.Id, recordExists, CurrentRowNbr, ref status );
						//foreach ( var cp in request.ConditionProfiles )
						//{
						//	if ( cp.DeletingProfile )
						//	{

						//	}
						//	else
						//	{
						//		if ( !helpers.HandleConditionProfile( request.ConditionProfile, po, allNonConnectionConditions, user.Id, recordExists, CurrentRowNbr, ref status ) )
						//		{
						//			//actions?
						//		}
						//	}
						//}
					}

					if ( request.HasConnectionProfile )
					{
						helpers.HandleConnectionProfiles( request.ConnectionProfiles, po, credential.CredentialConnections, user.Id, recordExists, CurrentRowNbr, ref status );
						//foreach ( var cp in request.ConnectionProfiles )
						//{
						//	if ( cp.DeletingProfile )
						//	{

						//	}
						//	else
						//	{
						//		if ( !helpers.HandleConnection( cp, po, credential.CredentialConnections, user.Id, recordExists, CurrentRowNbr, ref status ) )
						//		{
						//			//actions?
						//		}
						//	}
						//}

						//if ( request.ConditionProfile.DeletingProfile )
						//{

						//}
						//else
						//{
						//	//TODO: should attempt to use the basic HandleConditions!!!
						//	//need to especially handle the types, and multiple artifacts
						//	if ( !helpers.HandleConnections( request.ConnectionProfile, po, credential.CredentialConnections, user.Id, recordExists, CurrentRowNbr, ref status ) )
						//	{
						//		//actions?
						//	}

						//}
					}

					if ( request.HasCostProfileInput )
                    {
                        LoggingHelper.DoTrace( 9, string.Format( "CredentialImport.Import. Row: {0} ___ cost profile", CurrentRowNbr, request.Name ) );
						//if ( !helpers.HandleCostProfile( request, 1, po, user.Id, recordExists, ref status ) )
						//	{
						//                      //actions?
						//                  }
						helpers.HandleCostProfiles( request.CostProfiles, 1, po, user.Id, recordExists, CurrentRowNbr, ref status );
					}
                    if ( request.HasAssessments )
                    {

                    }
                    if ( request.HasLearningOpps )
                    {

                    }
					helpers.HandleAddresses( request, po, user.Id, CurrentRowNbr, ref status );

					helpers.HandleCommonConditions( request, credential.RowId, credential.CommonConditions, user.Id, recordExists, ref status );

                    helpers.HandleCommonCosts(request, credential.RowId, credential.CommonCosts, user.Id, recordExists, ref status);
                    //HandleCommonCosts( request, credential, user.Id, recordExists, ref status );

                } catch (Exception ex)
                {
                    //only fail current, and allow to continue
                    status.AddError( string.Format( "Exception encountered. Row: {0}, Name: {1}, Message: {2}", CurrentRowNbr, request.Name, ex.Message ) );

                    LoggingHelper.LogError( ex, string.Format( "CredentialImport.Import Row: {0}, :Credential: {1} ({2}) Exception encountered", CurrentRowNbr, request.Name, credential.Id ) );
                } finally
                {
                    //do regardless
                    //this needs to be async
                    //update cache tables-N/A this is done in Credential manager
                    //-adding back as in the latter it is done before adding industries, occs, etc
                    //new CacheManager().PopulateEntityRelatedCaches( entity.RowId, true, true, false );
                    ThreadPool.QueueUserWorkItem( UpdateCaches, credential.RowId );
                }
                

                    

            } //loop

            //update cache tables for org - assumes only one org
            new CacheManager().PopulateOrgRelatedCaches( credential.ManagingOrgId );
            //update cache tables for all credentials
            //new CacheManager().PopulateCredentialRelatedCaches( 0 );
        } //
        static void UpdateCaches( Object rowId )
        {
            if (rowId == null || !IsValidGuid( rowId.ToString() ))
                return;
            Guid guid = new Guid( rowId.ToString() );
            LoggingHelper.DoTrace( 6, "Doing PopulateEntityRelatedCaches " +  guid.ToString() );
            new CacheManager().PopulateEntityRelatedCaches( guid, true, true, false );

        }
        private int HandleCredential( CredentialImportRequest importHelper, CredentialDTO item, ref Credential record, AppUser user,
                 ref bool recordExists, ref ImportStatus status )
        {
            int newId = 0;
            List<string>  messages = new List<string>();
            string statusMessage = "";
            //check if cred exists based on ctid, name and subject webpage
            //TBD - if found may just to a complete replace, or only update properties available in import
            if (DoesCredentialExist( item, ref record ))
            {
                //need to distinguish between info messages and errors
                if ( !item.FoundExistingRecord )
                {
                   // status.AddWarning( string.Format( "Credential already exists. Row: {0}, Name: {1}, Id: {2}", item.RowNumber, item.Name, cred.Id ) );
                }
                recordExists = true;
                //return cred.Id;
                //may want to do deletes here
            }

            if (!recordExists || !string.IsNullOrWhiteSpace(item.Name))
                record.Name = item.Name;
            //CTID is likely empty if using external id. Note that CTID is only set on add , and not mapped on update. 
            //  will there be a case where we would want to overwrite?
            if (!recordExists || !string.IsNullOrWhiteSpace( item.CTID ))
                record.ctid = string.IsNullOrWhiteSpace( item.CTID ) ? record.ctid : item.CTID ;
            //should not be able to change the ext id?, even if forget to include successively 
            if (string.IsNullOrWhiteSpace( item.ExternalIdentifier ))
            {
                //if already has value, ignore assignment and warn.
                if (!string.IsNullOrWhiteSpace( record.ExternalIdentifier ))
                {
                    status.AddWarning( string.Format( "Row: {0}. The external identifier is empty and the existing credential has an existing external identifer. The identifer cannot not be set empty. Contact system support for assistance.  Name: {1}, Id: {2}, Existing External Identifier: {3}", item.RowNumber, item.Name, record.Id, record.ExternalIdentifier ) );
                    //just in case
                    item.ExternalIdentifier = record.ExternalIdentifier;
                }
            } else if (!string.IsNullOrWhiteSpace( record.ExternalIdentifier )
                && record.ExternalIdentifier != item.ExternalIdentifier)
            {
                //should not be allowed to change
                status.AddWarning( string.Format( "Row: {0}. The external identifier has a different value than was previously uploaded for this credential. The identifer should not be changed. Contact system support for assistance.  Name: {1}, Id: {2}, Existing External Identifier: {3}, Uploaded UId: {4}", item.RowNumber, item.Name, record.Id, record.ExternalIdentifier, item.ExternalIdentifier ) );
            } else 
                record.ExternalIdentifier = item.ExternalIdentifier;

            if (!recordExists || !string.IsNullOrWhiteSpace( item.Description ))
                record.Description = item.Description;
            if (!recordExists || !string.IsNullOrWhiteSpace( item.SubjectWebpage ))
                record.SubjectWebpage = item.SubjectWebpage;
            //can't delete this
            record.OwningAgentUid = item.OwningAgentUid;

            //if (!entityExists || !string.IsNullOrWhiteSpace( item.ImageUrl ))
            //    cred.ImageUrl = item.ImageUrl == DELETE_ME ? "": item.ImageUrl;
            record.ImageUrl = helpers.AssignUrl( item.ImageUrl, record.ImageUrl, recordExists );

            //already verified and can't delete
            if (!string.IsNullOrWhiteSpace(item.CredentialTypeSchema ))
                record.CredentialType = item.CredentialType; ;
            //can't delete
            if (item.CredentialStatus != null && item.CredentialStatus.Items != null && item.CredentialStatus.Items.Count == 1)
            {
                record.CredentialStatusType = item.CredentialStatus;
            }
            else
            {
                record.CredentialStatusType = credActiveStatus;
            }
            if ( importHelper.Copyrightholder_CtidHdr > -1 )
            {
                if ( item.DeleteCopyrightHolder )
                    record.CopyrightHolder = new Guid();
                else if (IsValidGuid( item.CopyrightHolder ) )
                    record.CopyrightHolder = item.CopyrightHolder;
            }
            //TBD
            if (!recordExists )
                record.ManagingOrgId = item.OrganizationId;
            
            if (item.DateEffective == DELETE_ME)
            {
                record.DateEffective = "";
            }
            else if (IsValidDate( item.DateEffective ))
                record.DateEffective = item.DateEffective;

			//NOTE: the credential manager will add the entity relationships for these roles
			//18-04-16 - import sets owned by, the other roles are handled separately. this needs to be handled for export.
			if ( item.OwnerRoles.HasItems() == false )
			{
				EnumeratedItem ei = Entity_AgentRelationshipManager.GetAgentRole( "Owned By" );
				if ( ei == null || ei.Id == 0 )
				{
					messages.Add( string.Format( "The organization role: {0} is not valid", "OwnedBy" ) );
				}
				else
				{
					item.OwnerRoles.Items.Add( ei );
				}
			}
			record.OwnerRoles = item.OwnerRoles; 
            if (!recordExists)
                record.CreatedById = user.Id;
            record.LastUpdatedById = user.Id;

            //language ==========================================
            //18-07-10 - convert to handling list
            //if no language, default to english
            if ( item.LanguageCodeList.Count == 0 )
                item.LanguageCodeList.Add(englishCodeId);
            foreach (var code in item.LanguageCodeList)
            {
                //should be populated
                var exists = record.InLanguageCodeList.FirstOrDefault(a => a.LanguageCodeId == code);
                if (exists == null || exists.LanguageCodeId == 0)
                    record.InLanguageCodeList.Add(new LanguageProfile() { LanguageCodeId = code });
            }

            //=================
            record.CodedNotation = helpers.Assign( item.CodedNotation, record.CodedNotation, recordExists );
            record.CredentialId = helpers.Assign( item.CredentialId, record.CredentialId, recordExists );
            record.VersionIdentifier = helpers.Assign( item.VersionIdentifier, record.VersionIdentifier, recordExists );
            record.AlternateName = helpers.Assign( item.AlternateName, record.AlternateName, recordExists );
            record.ProcessStandards = helpers.AssignUrl( item.ProcessStandards, record.ProcessStandards, recordExists );
            record.ProcessStandardsDescription = helpers.Assign( item.ProcessStandardsDescription, record.ProcessStandardsDescription, recordExists );

            record.AvailabilityListing = helpers.AssignUrl( item.AvailabilityListing, record.AvailabilityListing, recordExists );
            record.AvailableOnlineAt = helpers.AssignUrl( item.AvailableOnlineAt, record.AvailableOnlineAt, recordExists );
            record.LatestVersion = helpers.AssignUrl( item.LatestVersion, record.LatestVersion, recordExists );
            record.PreviousVersion = helpers.AssignUrl( item.PreviousVersion, record.PreviousVersion, recordExists );

            if ( item.AudienceLevelTypesList == DELETE_ME )
            {
                if ( recordExists )
                {
                    new EntityPropertyManager().DeleteAll( record.RowId, CodesManager.PROPERTY_CATEGORY_AUDIENCE_LEVEL, ref statusMessage );
                    record.AudienceLevelType =new Enumeration();
                }

            } else if ( item.AudienceLevelType != null && item.AudienceLevelType.Items.Count > 0)
            {
                record.AudienceLevelType = item.AudienceLevelType;
            }

            if ( item.AudienceTypesList == DELETE_ME )
            {
                if ( recordExists )
                {
                    new EntityPropertyManager().DeleteAll( record.RowId, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE, ref statusMessage );
                    record.AudienceType = new Enumeration();
                }
            }
            else if ( item.AudienceType != null && item.AudienceType.Items.Count > 0 )
            {
                record.AudienceType = item.AudienceType;
            }

			//
			if ( item.AssessmentDeliveryTypeList == DELETE_ME )
			{
                if ( recordExists )
                {
                    new EntityPropertyManager().DeleteAll( record.RowId, CodesManager.PROPERTY_CATEGORY_ASMT_DELIVERY_TYPE, ref statusMessage );
                    record.AssessmentDeliveryType = new Enumeration();
                }
			}
			else if ( item.AssessmentDeliveryType != null && item.AssessmentDeliveryType.Items.Count > 0 )
			{
				record.AssessmentDeliveryType = item.AssessmentDeliveryType;
			}
			//
			if ( item.LearningDeliveryTypeList == DELETE_ME )
			{
                if ( recordExists )
                {
                    new EntityPropertyManager().DeleteAll( record.RowId, CodesManager.PROPERTY_CATEGORY_DELIVERY_TYPE, ref statusMessage );
                    record.LearningDeliveryType = new Enumeration();
                }
			}
			else if ( item.LearningDeliveryType != null && item.LearningDeliveryType.Items.Count > 0 )
			{
				record.LearningDeliveryType = item.LearningDeliveryType;
			}


			if ( !recordExists )
            {
                LoggingHelper.DoTrace( 7, string.Format( "CredentialImport.Import. Row: {0} Adding credential", CurrentRowNbr ) );
                newId = cmgr.Add( record, ref statusMessage, false );
                if ( newId > 0 )
                {
                    LoggingHelper.DoTrace( 9, string.Format( "CredentialImport.Import. Row: {0} adding activity", CurrentRowNbr ) );
                    activityMgr.SiteActivityAdd( new SiteActivity()
                    {
                        ActivityType = "Credential",
                        Activity = "Bulk Upload",
                        Event = "Add",
                        Comment = string.Format( "{0} added credential via Bulk Upload: {1}", user.FullName(), record.Name ),
                        ActivityObjectId = newId,
                        ActionByUserId = user.Id,
                        ActivityObjectParentEntityUid = record.RowId,
						DataOwnerCTID = item.OwningOrganizationCtid
					} );
                }
            }
            else
            {
                LoggingHelper.DoTrace( 7, string.Format( "CredentialImport.Import. Row: {0} updating credential: {1}", CurrentRowNbr, record.Id ) );
                cmgr.Update( record, ref statusMessage, false );
                newId = record.Id;
                activityMgr.SiteActivityAdd( new SiteActivity()
                {
                    ActivityType = "Credential",
                    Activity = "Bulk Upload",
                    Event = "Update",
                    Comment = string.Format( "{0} updated credential via Bulk Upload: {1}", user.FullName(), record.Name ),
                    ActivityObjectId = record.Id,
                    ActionByUserId = user.Id,
                    ActivityObjectParentEntityUid = record.RowId,
					DataOwnerCTID = item.OwningOrganizationCtid
				} );
            }

            if (newId == 0 || (!string.IsNullOrWhiteSpace( statusMessage ) && statusMessage != "successful" ) )
            {
                status.AddError( string.Format( "Row: {0}, Issue encountered updating credential: {1}", item.RowNumber,statusMessage ));
                return 0;
            }
            //==================================================
            string siteUrl = UtilityManager.FormatAbsoluteUrl("~/");
            string editUrl = UtilityManager.FormatAbsoluteUrl( string.Format( "~/editor/credential/{0}", record.Id ) );
            string editLink = string.Format( "<a href='{0}' target='editWindow'>Edit Credential</a>", editUrl ) ;
            string editUrl2 = UtilityManager.FormatAbsoluteUrl(string.Format("~/editor/credential/{0}", record.Id));
            string detailLink = string.Format("<a href='{0}' target='detailWindow'>Detail Page</a>", siteUrl + string.Format("credential/{0}", record.Id));
            status.AddInformation( string.Format( "Added credential: '{0}', id: {1}, {2}, {3}", record.Name, record.Id, editLink, detailLink) );

            // ===================================================
            //add other parts, or add special method for properties available during imports
            LoggingHelper.DoTrace( 9, string.Format( "CredentialImport.Import. Row: {0} updating parts =======", CurrentRowNbr ) );
			//Parent entity of the credential
            Entity parentEntity = EntityManager.GetEntity( record.RowId );
            //general
            helpers.UpdateRoles( item.DeleteOfferedBy, parentEntity, "Offered By", Entity_AgentRelationshipManager.ROLE_TYPE_OFFERED_BY, item.OfferedByList, user.Id, CurrentRowNbr, ref status );

            LoggingHelper.DoTrace( 9, string.Format( "CredentialImport.Import. Row: {0} ___________ ROLES", CurrentRowNbr ) );
            if (item.DeleteApprovedBy)
            {
                earMgr.DeleteAllForRoleType( parentEntity.EntityUid, Entity_AgentRelationshipManager.ROLE_TYPE_ApprovedBy, ref messages );
            }
            else if (item.ApprovedByList != null && item.ApprovedByList.Count > 0)
            {
                foreach (var org in item.ApprovedByList)
                {
                    if (earMgr.Add( parentEntity.Id, org.RowId, Entity_AgentRelationshipManager.ROLE_TYPE_ApprovedBy, true, user.Id, ref messages ) == 0)
                        status.AddErrorRange( string.Format( "Row: {0}, Issue encountered updating Approved By for: {1}", item.RowNumber, org.Name ), messages );
                }
            }
            if (item.DeleteAccreditedBy)
            {
                earMgr.DeleteAllForRoleType( parentEntity.EntityUid, Entity_AgentRelationshipManager.ROLE_TYPE_AccreditedBy, ref messages );
            }
            else if (item.AccreditedByList != null && item.AccreditedByList.Count > 0)
            {
                foreach (var org in item.AccreditedByList)
                {
                    if (earMgr.Add( parentEntity.Id, org.RowId, Entity_AgentRelationshipManager.ROLE_TYPE_AccreditedBy, true, user.Id, ref messages ) == 0)
                        status.AddErrorRange( string.Format( "Row: {0}, Issue encountered updating Accredited By for: {1}", item.RowNumber, org.Name ), messages );
                }
            }

            helpers.UpdateRoles( item.DeleteRecognizedBy, parentEntity, "Recognized By", Entity_AgentRelationshipManager.ROLE_TYPE_RECOGNIZED_BY, item.RecognizedByList, user.Id, CurrentRowNbr, ref status );

            helpers.UpdateRoles( item.DeleteRegulatedBy, parentEntity, "Regulated By", Entity_AgentRelationshipManager.ROLE_TYPE_REGULATED_BY, item.RegulatedByList, user.Id, CurrentRowNbr, ref status );

            LoggingHelper.DoTrace( 9, string.Format( "CredentialImport.Import. Row: {0} ___________ DURATION", CurrentRowNbr ) );
            if ( item.DeleteEstimatedDuration )
            {
                if ( !dpmgr.DeleteAll( record.RowId, 0, ref messages ) )
                    status.AddError( string.Format( "Row: {0}, Issue encountered removing existing durations: {1}", item.RowNumber, statusMessage ) );
            }
            else
            {
                //estimate duration - only handling exact initially, or maybe description
                if ( item.HasEstimatedDuration )
                {
                    item.EstimatedDuration.EntityId = parentEntity.Id;
                    //for an existing cred, check for existing profiles. Approach may be to remove all, and then add, OR ??
                    item.EstimatedDuration.ParentUid = record.RowId;
                    if ( recordExists )
                    {
                        List<DurationProfile> existing = DurationProfileManager.GetAll( record.RowId );
                        if ( existing.Count > 0 )
                        {
                            foreach ( var dp in existing )
                            {
                                if ( !dpmgr.DurationProfile_Delete( dp.Id, ref statusMessage ) )
                                    status.AddError( string.Format( "Row: {0}, Issue encountered removing existing durations: {1}", item.RowNumber, statusMessage ) );
                            }
                        }
                    }

                    if ( dpmgr.Save( item.EstimatedDuration, user.Id, ref messages ) == false )
                        status.AddErrorRange( string.Format( "Row: {0}, Following issue encountered for Duration:", item.RowNumber ), messages );
                }
            }
            //
            if ( item.DeleteRenewalFrequency)
            {
                if ( !dpmgr.DeleteAll( record.RowId, 3, ref messages ) )
                    status.AddError( string.Format( "Row: {0}, Issue encountered removing existing renewal frequency: {1}", item.RowNumber, statusMessage ) );
            }
            else
            {
                //estimate duration - only handling exact initially, or maybe description
                if ( item.HasRenewalFrequency )
                {
                    item.RenewalFrequency.EntityId = parentEntity.Id;
                    //for an existing cred, check for existing profiles. Approach may be to remove all, and then add, OR ??
                    item.RenewalFrequency.ParentUid = record.RowId;
                    if ( recordExists )
                    {
                        List<DurationProfile> existing = DurationProfileManager.GetAll( record.RowId, 3 );
                        if ( existing.Count > 0 )
                        {
                            foreach ( var dp in existing )
                            {
                                if ( !dpmgr.DurationProfile_Delete( dp.Id, ref statusMessage ) )
                                    status.AddError( string.Format( "Row: {0}, Issue encountered removing existing renewal frequencies: {1}", item.RowNumber, statusMessage ) );
                            }
                        }
                    }
                    item.RenewalFrequency.DurationProfileTypeId = 3;
                    if ( dpmgr.Save( item.RenewalFrequency, user.Id, ref messages ) == false )
                        status.AddErrorRange( string.Format( "Row: {0}, Following issue encountered for Renewal Frequency:", item.RowNumber ), messages );
                }
            }
            //
            //also need existance checks
            //ACTUALLY there is a dup check on save - so OK
            if (recordExists && item.OnetList.Count > 0)
            {

            }
            if (recordExists && item.NaicsList.Count > 0)
            {

            }
            LoggingHelper.DoTrace( 9, string.Format( "CredentialImport.Import. Row: {0} ___________ OCCUPATIONS", CurrentRowNbr ) );
			// ===================================================
			//occupations
			if ( item.DeletingSOCCodes )
			{
				if ( !efimgr.DeleteAll( parentEntity.EntityUid, CodesManager.PROPERTY_CATEGORY_SOC, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Issue encountered removing SOC items from credential: {1}", item.RowNumber, item.Name ), messages );
				}
			}
			else if ( item.OnetCodesList.Count > 0 )
			{
				if ( !efimgr.Replace( parentEntity.Id, CodesManager.PROPERTY_CATEGORY_SOC, item.OnetCodesList, user.Id, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Issue encountered updating SOC items for credential: {1}", item.RowNumber, item.Name ), messages );
				}
			}

			//if ( item.OnetList != null && item.OnetList.Count > 0 )
   //         {
   //             messages = new List<string>();
   //             if ( item.OnetList[ 0 ] == DELETE_ME )
   //             {
   //                 string statusMsg = "";
   //                 if ( !efimgr.DeleteAll( parentEntity.EntityUid, CodesManager.PROPERTY_CATEGORY_SOC, ref messages ) )
   //                 {
   //                     status.AddErrorRange( string.Format( "Row: {0}, Issue encountered removing SOC items from credential: {1}", item.RowNumber, item.Name ), messages );
   //                 }
   //             }
   //             else
   //             {
   //                 List<CodeItem> list = CodesManager.SOC_Search( item.OnetList );
   //                 foreach ( var code in list )
   //                 {
   //                     if ( efimgr.Add( parentEntity.Id, CodesManager.PROPERTY_CATEGORY_SOC, code.Id, user.Id, ref statusMessage ) == 0 )
   //                         status.AddError( string.Format( "Row: {0}, ONET Code: {1}, Error: {2}", item.RowNumber, code, statusMessage ) );
   //                 }
   //             }
   //         }

            LoggingHelper.DoTrace( 9, string.Format( "CredentialImport.Import. Row: {0} ___________ industries", CurrentRowNbr ) );
			//industries
			if ( item.DeletingNaicsCodes )
			{
				if ( !efimgr.DeleteAll( parentEntity.EntityUid, CodesManager.PROPERTY_CATEGORY_NAICS, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Issue encountered removing SOC items from credential: {1}", item.RowNumber, item.Name ), messages );
				}
			}
			else if ( item.NaicsCodesList.Count > 0)
			{
				if ( !efimgr.Replace( parentEntity.Id, CodesManager.PROPERTY_CATEGORY_NAICS, item.NaicsCodesList, user.Id, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Issue encountered updating NAICS items for credential: {1}", item.RowNumber, item.Name ), messages );
				}
			}
			
			// ===================================================
			//CIP
			if ( item.DeletingCIPCodes )
			{
				if ( !efimgr.DeleteAll( parentEntity.EntityUid, CodesManager.PROPERTY_CATEGORY_CIP, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Issue encountered removing CIP items from credential: {1}", item.RowNumber, item.Name ), messages );
				}
			}
			else if ( item.CIPCodesList.Count > 0 )
			{
				//this needs to be an exact match, not a full text
				//actually should do this is upload step now that added a check to the latter.
				if ( !efimgr.Replace( parentEntity.Id, CodesManager.PROPERTY_CATEGORY_CIP, item.CIPCodesList, user.Id, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Issue encountered updating CIP items for credential: {1}", item.RowNumber, item.Name ), messages );
				}
			}
            // ===================================================
            //replace keyword, etc.

            //if ( !erefMgr.Replace( record.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD, item.Keywords, user.Id, ref messages ) )
            //{
            //	status.AddErrorRange( string.Format( "Row: {0}, Issue encountered updating keywords", CurrentRowNbr ), messages );
            //}
            helpers.ReplaceEntityReferences( record.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD, item.Keywords, "Keywords", user.Id, CurrentRowNbr, ref status );

            helpers.ReplaceEntityReferences( record.RowId, CodesManager.PROPERTY_CATEGORY_SUBJECT, item.Subjects, "Subjects", user.Id, CurrentRowNbr, ref status );
            
            helpers.ReplaceEntityReferences( record.RowId, CodesManager.PROPERTY_CATEGORY_NAICS, item.Industries, "Industries", user.Id, CurrentRowNbr, ref status );
            
            helpers.ReplaceEntityReferences( record.RowId, CodesManager.PROPERTY_CATEGORY_SOC, item.Occupations, "Occupations", user.Id, CurrentRowNbr, ref status );
           
            helpers.ReplaceEntityReferences( record.RowId, CodesManager.PROPERTY_CATEGORY_CIP, item.Programs, "Programs", user.Id, CurrentRowNbr, ref status );

            helpers.ReplaceEntityReferences( record.RowId, CodesManager.PROPERTY_CATEGORY_DEGREE_MAJOR, item.DegreeMajors, "DegreeMajors", user.Id, CurrentRowNbr, ref status );

            helpers.ReplaceEntityReferences( record.RowId, CodesManager.PROPERTY_CATEGORY_DEGREE_MINOR, item.DegreeMinors, "DegreeMinors", user.Id, CurrentRowNbr, ref status );

            helpers.ReplaceEntityReferences( record.RowId, CodesManager.PROPERTY_CATEGORY_DEGREE_CONCENTRATION, item.DegreeConcentrations, "DegreeConcentrations", user.Id, CurrentRowNbr, ref status );




            return newId;
        }
        

        
        private bool DoesCredentialExist( CredentialDTO input, ref Credential cred )
        {
            int ptotalRows = 0;
            bool isFound = false;
            string filter = "";
            cred = new Credential();

            string url = NormalizeUrlData( input.SubjectWebpage );

            if (!string.IsNullOrWhiteSpace(input.CTID))
            {
                filter = string.Format( " ( base.CTID = '{0}' ) ", input.CTID );
            } else if (!string.IsNullOrWhiteSpace( input.ExternalIdentifier ))
            {
                filter = string.Format( " ( base.ExternalIdentifier = '{0}' AND OwningAgentUid = '{1}' )  ", input.ExternalIdentifier, input.OwningAgentUid);
            } else
            {   //shouldn't get to here
                //should actually return an error. Maybe add a check prior to here
                filter = string.Format( " ( base.Id in (Select Id from Credential where (name = '{0}' AND Url = '{1}') )) ", input.Name, url );
            }

            List<CredentialSummary> exists = CredentialManager.Search( filter, "", 1, 25, ref ptotalRows );
            if (exists != null && exists.Count > 0)
            {
                //note if multiple, but return first
                //really only need . Actually may need other if doing an existance check to prevent overwriting existing data with empty data (ex copyright)
                //180404 - mp - to preserve data, get everything, like for edit. Don't really need a lot of profiles, except to check for existance of these?
                cred = CredentialManager.GetForEdit( exists[ 0 ].Id );
                //CredentialManager.MapFromSummary( exists[ 0 ], cred );
                isFound = true;
            }

            return isFound;
        }
        private string AssignUrl( string input, string currentValue, bool doesEntityExist )
        {
            //start with string
            string value = Assign( input, currentValue, doesEntityExist );
            if (!string.IsNullOrWhiteSpace( value ) && ( value.ToLower() != (currentValue ?? "").ToLower()))
            {
                //url will be validated in manager. So unless can force to skip in manager, will not do here
            }
            return value;
        }

        /// <summary>
        /// Assign
        /// NOTE: the upload section should already prevent sending a delete request for a required property.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="currentValue"></param>
        /// <param name="doesEntityExist"></param>
        /// <returns></returns>
        private string Assign(string input, string currentValue, bool doesEntityExist)
        {
            string value = "";
            if (doesEntityExist)
            {
                value = input == DELETE_ME ? "" : string.IsNullOrWhiteSpace( input ) ? currentValue : input;
            }
            else if ( !string.IsNullOrWhiteSpace( input ) )
            {
                //don't allow delete for initial
                value = input == DELETE_ME ? "" : input;
            }
            return value;
        }

		//replaced by common method
        //private bool HandleCostProfile( CredentialDTO item, ref Credential entity, int userId, bool parentExists, ref ImportStatus status )
        //{
        //    bool isValid = true;
        //    List<string> messages = new List<string>();
            
        //    CostProfile cp = new CostProfile();
        //    bool profileExists = false;
        //    //TBD - to enable import from production, allow identifier with new cred. 
        //    if ( parentExists )
        //    {
        //        if ( item.CostProfile.IsExistingCostProfile
        //        || IsValidGuid( item.CostProfile.Identifier ) )
        //        {
        //            cp = CostProfileManager.GetBasicProfile( item.CostProfile.Identifier );
        //            if ( cp == null || cp.Id == 0 )
        //            {
        //                cp.RowId = item.CostProfile.Identifier;
        //            }
        //            else
        //                profileExists = true;
        //        }
        //    } else
        //    {
        //        //handling is TBD
        //        if ( IsValidGuid( item.CostProfile.Identifier ) )
        //        {
        //            cp.RowId = item.CostProfile.Identifier;
        //        }
        //    }

        //    //required, so can't set blank
        //    if ( !profileExists || !string.IsNullOrWhiteSpace( item.CostProfile.Description ) )
        //        cp.Description = Assign( item.CostProfile.Description, cp.Description, profileExists );

        //    if ( !profileExists || !string.IsNullOrWhiteSpace( item.CostProfile.Name ) )
        //    {
        //        cp.ProfileName = Assign( item.CostProfile.Name, cp.ProfileName, profileExists );
        //        if ( string.IsNullOrWhiteSpace( cp.ProfileName ) )
        //            cp.ProfileName = "Cost Profile for " + entity.Name;
        //    }

        //    if ( !profileExists || !string.IsNullOrWhiteSpace( item.CostProfile.DetailsUrl ) )
        //        cp.DetailsUrl = AssignUrl( item.CostProfile.DetailsUrl, cp.DetailsUrl, profileExists );

        //    if ( !profileExists || !string.IsNullOrWhiteSpace( item.CostProfile.CurrencyType ) )
        //    {
        //        cp.Currency = Assign( item.CostProfile.CurrencyType, cp.Currency, profileExists );
        //        cp.CurrencyTypeId = item.CostProfile.CurrencyTypeId;
        //    }

        //    if ( costProfileManager.Save( cp, entity.RowId, userId, ref messages ))
        //    {

        //        if (!string.IsNullOrWhiteSpace( item.CostProfile.ExternalIdentifier))
        //        {
        //            //may want to do this for existing as well
        //            if ( !profileExists )
        //            {
        //                int nId = new ImportHelpers().ExternalIdentifierXref_Add( CurrentEntityTypeId, entity.Id, CodesManager.ENTITY_TYPE_COST_PROFILE, item.CostProfile.ExternalIdentifier, cp.RowId, userId, ref messages );
        //            }
        //        }

        //        if ( item.CostProfile.CostItems != null && item.CostProfile.CostItems.Count > 0 )
        //        {
        //            messages = new List<string>();
        //            foreach ( var input in item.CostProfile.CostItems )
        //            {
        //                var cpi = new CostProfileItem()
        //                {
        //                    CostTypeId = input.DirectCostTypeId,
        //                    Price = input.Price,
        //                    CostProfileId = cp.Id
        //                };

        //                if (!costProfileItemManager.Save( cpi, cp.Id, userId, ref messages ))
        //                {
        //                    status.AddErrorRange( string.Format( "Row: {0}, Following issue encountered attempting to add a direct cost item to the credential:", item.RowNumber ), messages );
        //                }
        //            }
        //        }
        //    } else
        //    {
        //        //may attempt to add items anyway?
        //        status.AddErrorRange( string.Format( "Row: {0}, Following issue encountered attempting to add a cost profile to the credential:", item.RowNumber ), messages );

        //        if (cp.Id > 0)
        //        {

        //        }
        //    }

        //    return isValid;
        //}


        private void CommonSetup()
        {
            CodeItem ci = CodesManager.GetPropertyBySchema( "ceterms:CredentialStatus", "credentialStat:Active" );
            credActiveStatus = new Enumeration();
            //get property, and remember
            credActiveStatus.Id = CodesManager.PROPERTY_CATEGORY_CREDENTIAL_STATUS_TYPE;
            credActiveStatus.Items.Add( new EnumeratedItem()
            {
                CodeId = ci.Id,
                Id = ci.Id
            } );

        }
    }
}
