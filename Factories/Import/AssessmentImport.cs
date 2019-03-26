using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Models;
using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;

using Models.Import;
using ImportModel = Models.Import.AssessmentDTO;
using ThisEntity = Models.ProfileModels.AssessmentProfile;

namespace Factories.Import
{
    public class AssessmentImport : BaseFactory
    {
		#region properties
		private readonly static int CurrentEntityTypeId = 3;
		AssessmentManager asmtMgr = new AssessmentManager();
        Entity_AssessmentManager eaMgr = new Entity_AssessmentManager();
        ActivityManager activityMgr = new ActivityManager();
        AddressProfileManager addrMgr = new AddressProfileManager();
        CostProfileManager costProfileManager = new CostProfileManager();
        CostProfileItemManager CostProfileItemManager = new CostProfileItemManager();
        DurationProfileManager dpmgr = new DurationProfileManager();
        
        Entity_CommonConditionManager eCommonCndsMgr = new Entity_CommonConditionManager();
        Entity_CommonCostManager eCommonCostsMgr = new Entity_CommonCostManager();
        Entity_ConditionProfileManager cpMgr = new Entity_ConditionProfileManager();
        Entity_FrameworkItemManager efimgr = new Entity_FrameworkItemManager();
        Entity_ReferenceManager erefMgr = new Entity_ReferenceManager();
        ImportHelpers helpers = new ImportHelpers();

        CodeItem examCode = new CodeItem();
        CodeItem inPersonCode = new CodeItem();
        CodeItem onlineCode = new CodeItem();
        CodeItem blendedCode = new CodeItem();
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

        public void Import(List<ImportModel> inputList, AppUser user, ref ImportStatus status)
        {
			ImportHelpers.CurrentEntityType = "Assessment";

			CommonSetup();
            List<string> messages = new List<string>();
            ThisEntity asmt = new ThisEntity();
            int previousRecordId = 0;
			ParentObject po = new ParentObject();
            foreach ( var request in inputList )
            {
                CurrentRowNbr++;
                messages = new List<string>();
				string statusMessage = "";
				//handle assessment - must exist for partial
				bool recordExists = false;
                LoggingHelper.DoTrace( 6, string.Format( "AssessmentImport.Import. Row: {0}, Name: {1}", CurrentRowNbr, request.Name ) );
                //create assessment
                asmt = new ThisEntity();
                try
                {
                    if ( !request.FoundExistingRecord ||
    ( request.FoundExistingRecord && previousRecordId != request.ExistingRecord.Id ) )
                    {
                        if ( HandleAssessment( request, ref asmt, user, ref recordExists, ref status ) < 1 )
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

                    previousRecordId = asmt.Id;
					po = new ParentObject() { Id = asmt.Id, RowId = asmt.RowId, Name = asmt.Name, OwningAgentUid = asmt.OwningAgentUid, ParentTypeId = 3, EntityType = "Assessment" };
					if ( request.TargetCredentials.Count > 0 )
                    {
                        /*foreach credential:
                         *  check if has a condition profile (now will have already read)
                         *  - if it does, 
                         *      is the condition type different than entered
                         *          ?? - override, or error, or additional condition
                         *      else 
                         *          add to condition
                         *   - if doesn't exist
                         *      add new condition with default title, and description
                         *      then add assessment
                         */
                        foreach ( var credential in request.TargetCredentials )
                        {
                            if ( credential.OwningAgentUid != asmt.OwningAgentUid )
                            {
                                //what - create an asmt connection profile
                                //      - note could be acceptable within multiple universities
								//Need to reverse the connection type from required (1) to is required for (3), and recommended (2) to is recommended for (4)
								//need a description
                                ConditionProfileDTO cp = new ConditionProfileDTO();
                                cp.ConditionTypeId = request.CredentialConditionTypeId;
								if ( cp.ConditionTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Requirement )
								{
									cp.ConditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_NextIsRequiredFor;
									cp.ConditionType = "Is Required For";
									cp.Description = "This assessment is required for credential '" + credential.Name + "'";
								}
								else if ( cp.ConditionTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Recommendation )
								{
									cp.ConditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_NextIsRecommendedFor;
									cp.ConditionType = "Is Recommended For";
									cp.Description = "This assessment is recommended for credential '" + credential.Name + "'";
								} else
								{
									cp.Description = "This assessment has a connection for credential '" + credential.Name + "'";
								}
								cp.TargetCredentialList.Add(credential);
								cp.ConditionSubTypeId = Entity_ConditionProfileManager.ConditionSubType_Assessment;
								//ensure this does updates as needed, not just adds
								//should use AssessmentConnections not AllConditions
								//ensure 
								if ( !helpers.HandleConnection( cp, po, asmt.AssessmentConnections, user.Id, true, CurrentRowNbr, ref status ) )
                                {
                                    //messages should be handled
                                }
                            }
                            else
                            {
								//add under credential
								ConditionProfileDTO cp = new ConditionProfileDTO();
                                cp.ConditionTypeId = request.CredentialConditionTypeId;
								cp.TargetAssessmentList.Add( asmt );
								if ( cp.ConditionTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Requirement )
								{
									cp.ConditionType = "Requires";
									cp.Description = "This assessment is required for credential '" + credential.Name + "'";
								}
								else if ( cp.ConditionTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Recommendation )
								{
									cp.ConditionType = "Recommends";
									cp.Description = "This assessment is recommended for credential '" + credential.Name + "'";
								}
								else
								{
									cp.Description = "This assessment has a connection for credential '" + credential.Name + "'";
								}

								ParentObject cpo = new ParentObject() { Id = credential.Id, RowId = credential.RowId, Name = credential.Name, OwningAgentUid = credential.OwningAgentUid, ParentTypeId = 1, EntityType = "Credential" };
								if ( !helpers.HandleConditionProfile( cp, cpo, credential.AllConditions, user.Id, true, CurrentRowNbr, ref status ) )
                                {
									//messages should be handled
								}
							}
                        }

                    }
					if ( request.HasConditionProfile )
					{
						List<ConditionProfile> allNonConnectionConditions = asmt.AllConditions.Where( n => n.ConditionSubTypeId == 1 ).ToList();
						helpers.HandleConditionProfiles( request.ConditionProfiles, po, allNonConnectionConditions, user.Id, recordExists, CurrentRowNbr, ref status );
						//if ( !HandleConditions( request, ref credential, user.Id, recordExists, ref status ) )
						//{
						//    //actions?
						//}

					}

					if ( request.HasConnectionProfile )
					{
						helpers.HandleConnectionProfiles( request.ConnectionProfiles, po, asmt.AssessmentConnections, user.Id, recordExists, CurrentRowNbr, ref status );

						//TODO: should attempt to use the basic HandleConditions!!!
						//need to especially handle the types, and multiple artifacts
						//if ( !helpers.HandleConnections( request.ConnectionProfile, po, record.CredentialConnections, user.Id, recordExists, CurrentRowNbr, ref status ) )
						//{
						//	//actions?
						//}
					}

					if ( request.HasCostProfileInput )
                    {
                        LoggingHelper.DoTrace( 9, string.Format( "AssessmentImport.Import. Row: {0} ___ cost profile", CurrentRowNbr, request.Name ) );
						helpers.HandleCostProfiles( request.CostProfiles, 3, po, user.Id, recordExists, CurrentRowNbr, ref status );

						//if ( !helpers.HandleCostProfile( request.CostProfile, CurrentEntityTypeId, po, user.Id, recordExists, CurrentRowNbr, ref status ) )
						//{
						//	//actions?
						//}

					}
					//if ( request.Frameworks.Count > 0 )
					//{
						helpers.HandleCompetencyFrameworks( request, asmt.RowId, user, recordExists, ref status );
					//}

					//if ( request.DeleteAvailableAt )
     //               {
					//	//need to be careful
					//	if ( !eaMgr.DeleteAll( asmt.RowId, ref messages ) )
					//	{
					//		status.AddErrorRange( string.Format( "Row: {0}, Following issue(s) were encountered attempting to remove all addresses for this assessment.", CurrentRowNbr ), messages );
					//	}
					//}
     //               else if ( request.AvailableAt.Count > 0 )
     //               {
     //                   //TODO - confirm action will no longer be copy, but replace
     //                   //      - need to check for addition of contact points
     //                   if ( !addrMgr.CopyList( request.AvailableAt, asmt.RowId, user.Id, ref messages ) )
     //                   {
     //                       status.AddErrorRange( string.Format( "Row: {0}, Issue encountered copying addresses for: {1}", CurrentRowNbr, asmt.Name ), messages );
     //                   }
     //               }
					helpers.HandleAddresses( request, po, user.Id, CurrentRowNbr, ref status );

					helpers.HandleCommonConditions( request, asmt.RowId, asmt.CommonConditions, user.Id, recordExists, ref status );

                    helpers.HandleCommonCosts( request, asmt.RowId, asmt.CommonCosts, user.Id, recordExists, ref status );

                }
                catch ( Exception ex )
                {
                    //only fail current, and allow to continue
                    status.AddError( string.Format( "Exception encountered. Row: {0}, Name: {1}, Message: {2}", CurrentRowNbr, request.Name, ex.Message ) );

                    LoggingHelper.LogError( ex, string.Format( "AssessmentImport.Import Row: {0}, :Assessment: {1} ({2}) Exception encountered", CurrentRowNbr, request.Name, asmt.Id ) );
                }
                finally
                {
                    //do regardless
                    //this needs to be async
                    //actually not sure if needed for assessments
                    //ThreadPool.QueueUserWorkItem( UpdateCaches, asmt.RowId );
                }
            } //loop
        } //
        static void UpdateCaches(Object rowId)
        {
            if ( rowId == null || !IsValidGuid(rowId.ToString()) )
                return;
            Guid guid = new Guid(rowId.ToString());
            LoggingHelper.DoTrace(6, "Doing PopulateEntityRelatedCaches " + guid.ToString());
            new CacheManager().PopulateEntityRelatedCaches(guid, true, true, false);

        }
        private int HandleAssessment(ImportModel item, ref ThisEntity profile, AppUser user,
                 ref bool recordExists, ref ImportStatus status)
        {
            int newId = 0;
            List<string> messages = new List<string>();
            string statusMessage = "";

            if ( DoesAssessmentExist(item, ref profile) )
            {
                //doesn't really matter if the input FoundExistingRecord is false, and record found?
                if ( !item.FoundExistingRecord )
                {
                    // status.AddWarning( string.Format( "Record already exists. Row: {0}, Name: {1}, Id: {2}", item.RowNumber, item.Name, cred.Id ) );
                }
                recordExists = true;
                //return cred.Id;
                //may want to do deletes here
            }


            if ( !recordExists || !string.IsNullOrWhiteSpace(item.Name) )
                profile.Name = item.Name;
            //CTID is likely empty if using external id. Note that CTID is only set on add , and not mapped on update. 
            //  will there be a case where we would want to overwrite?
            if ( !recordExists || !string.IsNullOrWhiteSpace(item.CTID) )
                profile.ctid = string.IsNullOrWhiteSpace(item.CTID) ? profile.ctid : item.CTID;

            //should not be able to change the ext id?, even if forget to include successively 
            if ( string.IsNullOrWhiteSpace(item.ExternalIdentifier) )
            {
                //if already has value, ignore assignment and warn.
                if ( !string.IsNullOrWhiteSpace(profile.ExternalIdentifier) )
                {
                    status.AddWarning(string.Format("Row: {0}. The external identifier is empty and the existing assessment has a external identifer. The identifer should not be set empty. Contact system support for assistance.  Name: {1}, Id: {2}, Existing External Identifier: {3}", item.RowNumber, item.Name, profile.Id, profile.ExternalIdentifier));
                    //just in case
                    item.ExternalIdentifier = profile.ExternalIdentifier;
                }
            }
            else if ( !string.IsNullOrWhiteSpace(profile.ExternalIdentifier)
              && profile.ExternalIdentifier != item.ExternalIdentifier )
            {
                //should not be allowed to change
                status.AddWarning(string.Format("Row: {0}. The external identifier has a different value than was previously uploaded for this assessment. The identifer should not be changed. Contact system support for assistance.  Name: {1}, Id: {2}, Existing External Identifier: {3}, Uploaded UId: {4}", item.RowNumber, item.Name, profile.Id, profile.ExternalIdentifier, item.ExternalIdentifier));
            }
            else
                profile.ExternalIdentifier = item.ExternalIdentifier;

            if ( !recordExists || !string.IsNullOrWhiteSpace(item.Description) )
                profile.Description = item.Description;
            if ( !recordExists || !string.IsNullOrWhiteSpace(item.SubjectWebpage) )
                profile.SubjectWebpage = item.SubjectWebpage;
            //can't delete this
            profile.OwningAgentUid = item.OwningAgentUid;

            //TBD
            if ( !recordExists )
                profile.ManagingOrgId = item.OrganizationId;

            if ( item.DateEffective == DELETE_ME )
            {
                profile.DateEffective = "";
            }
            else if ( IsValidDate(item.DateEffective) )
                profile.DateEffective = item.DateEffective;

            //===============================
            //NOTE: the assessment manager will add the entity relationships for these roles
            //18-04-16 - import sets owned by, the other roles are handled separately. this needs to be handled for export. 
			if ( item.OwnerRoles.HasItems() == false)
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
            profile.OwnerRoles = item.OwnerRoles;
            if ( !recordExists )
                profile.CreatedById = user.Id;
            profile.LastUpdatedById = user.Id;

            //language ==========================================
            //18-07-10 - convert to handling list
            //if no language, default to english
            if ( item.LanguageCodeList.Count == 0 )
                item.LanguageCodeList.Add(englishCodeId);
            foreach ( var code in item.LanguageCodeList )
            {
                //should be populated
                var exists = profile.InLanguageCodeList.FirstOrDefault(a => a.LanguageCodeId == code);
                if ( exists == null || exists.Id == 0 )
                    profile.InLanguageCodeList.Add(new LanguageProfile() { LanguageCodeId = code });
            }

            profile.AssessmentExample = helpers.AssignUrl(item.AssessmentExampleUrl, profile.AssessmentExample, recordExists);
            profile.AssessmentExampleDescription = helpers.Assign(item.AssessmentExampleDescription, profile.AssessmentExampleDescription, recordExists);
            //
            if (item.AudienceTypesList == DELETE_ME)
            {
				if ( recordExists )
				{
					new EntityPropertyManager().DeleteAll( profile.RowId, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE, ref statusMessage );
					profile.AudienceType = new Enumeration();
				}
            }
            else if (item.AudienceType != null && item.AudienceType.Items.Count > 0)
            {
                profile.AudienceType = item.AudienceType;
            }
            //
            if (item.AssessmentMethodTypeList == DELETE_ME)
            {
				if ( recordExists )
				{
					new EntityPropertyManager().DeleteAll( profile.RowId, CodesManager.PROPERTY_CATEGORY_Assessment_Method_Type, ref statusMessage );
					profile.AssessmentMethodType = new Enumeration();
				}
            }
            else if (item.AssessmentMethodType != null && item.AssessmentMethodType.Items.Count > 0)
            {
                profile.AssessmentMethodType = item.AssessmentMethodType;
            }
            profile.AssessmentOutput = helpers.Assign(item.AssessmentOutput, profile.AssessmentOutput, recordExists);
            //
            if (item.AssessmentUseTypeList == DELETE_ME)
            {
				if ( recordExists )
				{
					new EntityPropertyManager().DeleteAll( profile.RowId, CodesManager.PROPERTY_CATEGORY_ASSESSMENT_USE_TYPE, ref statusMessage );
					profile.AssessmentUseType = new Enumeration();
				}
            }
            else if (item.AssessmentUseType != null && item.AssessmentUseType.Items.Count > 0)
            {
                profile.AssessmentUseType = item.AssessmentUseType;
            }
            //            
            
            profile.AvailabilityListing = helpers.AssignUrl(item.AvailabilityListing, profile.AvailabilityListing, recordExists);
            profile.AvailableOnlineAt = helpers.AssignUrl(item.AvailableOnlineAt, profile.AvailableOnlineAt, recordExists);
            profile.CodedNotation = helpers.Assign(item.CodedNotation, profile.CodedNotation, recordExists);
            //
            if (item.DeliveryTypeList == DELETE_ME)
            {
				if ( recordExists )
				{
					new EntityPropertyManager().DeleteAll( profile.RowId, CodesManager.PROPERTY_CATEGORY_DELIVERY_TYPE, ref statusMessage );
					profile.DeliveryType = new Enumeration();
				}
            }
            else if (item.DeliveryType != null && item.DeliveryType.Items.Count > 0)
            {
                profile.DeliveryType = item.DeliveryType;
            }
            //
            profile.ExternalResearch = helpers.AssignUrl(item.ExternalResearch, profile.ExternalResearch, recordExists);
            profile.ProcessStandards = helpers.AssignUrl(item.ProcessStandards, profile.ProcessStandards, recordExists);
            profile.ProcessStandardsDescription = helpers.Assign(item.ProcessStandardsDescription, profile.ProcessStandardsDescription, recordExists);

            //
            if (item.ScoringMethodTypeList == DELETE_ME)
            {
				if ( recordExists )
				{
					new EntityPropertyManager().DeleteAll( profile.RowId, CodesManager.PROPERTY_CATEGORY_Scoring_Method, ref statusMessage );
					profile.ScoringMethodType = new Enumeration();
				}
            }
            else if (item.ScoringMethodType != null && item.ScoringMethodType.Items.Count > 0)
            {
                profile.ScoringMethodType = item.ScoringMethodType;
            }
            profile.ScoringMethodDescription = helpers.Assign(item.ScoringMethodDescription, profile.ScoringMethodDescription, recordExists);
            profile.ScoringMethodExample = helpers.AssignUrl(item.ScoringMethodExample, profile.ScoringMethodExample, recordExists);
            profile.ScoringMethodExampleDescription = helpers.Assign(item.ScoringMethodExampleDescription, profile.ScoringMethodExampleDescription, recordExists);
            profile.VerificationMethodDescription = helpers.Assign(item.VerificationMethodDescription, profile.VerificationMethodDescription, recordExists);
            profile.VersionIdentifier = helpers.Assign(item.VersionIdentifier, profile.VersionIdentifier, recordExists);
            //
            profile.IsProctored = helpers.Assign( item.IsProctored, profile.IsProctored, recordExists );
            profile.HasGroupEvaluation = helpers.Assign( item.HasGroupEvaluation, profile.HasGroupEvaluation, recordExists );
            profile.HasGroupParticipation = helpers.Assign( item.HasGroupParticipation, profile.HasGroupParticipation, recordExists );

			//==================================================


            //need to add an edit for hours or units!
            profile.CreditHourType = helpers.Assign( item.CreditHourType, profile.CreditHourType, recordExists );
            profile.CreditUnitTypeDescription = helpers.Assign( item.CreditUnitTypeDescription, profile.CreditUnitTypeDescription, recordExists );
            profile.CreditHourValue = helpers.Assign( item.CreditHourValue, profile.CreditHourValue, recordExists );
            profile.CreditUnitValue = helpers.Assign( item.CreditUnitValue, profile.CreditUnitValue, recordExists );
            if ( item.CreditUnitType == DELETE_ME )
            {
				if ( recordExists )
				{
					new EntityPropertyManager().DeleteAll( profile.RowId, CodesManager.PROPERTY_CATEGORY_CREDIT_UNIT_TYPE, ref statusMessage );
					profile.CreditUnitTypeId = 0;
				}
            }
            else if ( item.CreditUnitTypeId > 0 )
            {
                profile.CreditUnitTypeId = item.CreditUnitTypeId;
            }

			if ( !recordExists )
            {
                LoggingHelper.DoTrace(7, string.Format("AssessmentImport.Import. Row: {0} Adding assessment", CurrentRowNbr));
                newId = asmtMgr.Add(profile, ref statusMessage);

                if ( newId > 0 )
                {
                    LoggingHelper.DoTrace( 9, string.Format( "AssessmentImport.Import. Row: {0} adding activity", CurrentRowNbr ) );
                    activityMgr.SiteActivityAdd( new SiteActivity()
                    {
                        ActivityType = "Assessment",
                        Activity = "Bulk Upload",
                        Event = "Add",
                        Comment = string.Format( "{0} added assessment via Bulk Upload: {1}", user.FullName(), profile.Name ),
                        ActivityObjectId = newId,
                        ActionByUserId = user.Id,
                        ActivityObjectParentEntityUid = profile.RowId,
						DataOwnerCTID = item.OwningOrganizationCtid
					} );
                }
            }
            else
            {
                LoggingHelper.DoTrace(7, string.Format("AssessmentImport.Import. Row: {0} updating assessment: {1}", CurrentRowNbr, profile.Id));
                asmtMgr.Update(profile, ref statusMessage);
                newId = profile.Id;
                activityMgr.SiteActivityAdd(new SiteActivity()
                {
                    ActivityType = "Assessment",
                    Activity = "Bulk Upload",
                    Event = "Update",
                    Comment = string.Format("{0} updated assessment via Bulk Upload: {1}", user.FullName(), profile.Name),
                    ActivityObjectId = profile.Id,
                    ActionByUserId = user.Id,
                    ActivityObjectParentEntityUid = profile.RowId,
					DataOwnerCTID = item.OwningOrganizationCtid
				} );
            }
            if ( newId == 0 || ( !string.IsNullOrWhiteSpace( statusMessage ) && statusMessage != "successful" ) )
            {
                status.AddError(string.Format("Row: {0}, Issue encountered updating assessment: {1}", item.RowNumber, statusMessage));
                return 0;
            }
            //==================================================
            string siteUrl = UtilityManager.FormatAbsoluteUrl("~/");            
            string editLink = string.Format("<a href='{0}' target='editWindow'>Edit Assessment</a>", siteUrl + string.Format("editor/assessment/{0}", profile.Id));
            string detailLink = string.Format("<a href='{0}' target='detailWindow'>Detail Page</a>", siteUrl + string.Format("assessment/{0}", profile.Id));
            status.AddInformation(string.Format("Added Assessment: '{0}', id: {1}, {2}, {3}", profile.Name, profile.Id, editLink, detailLink));
            // ===================================================
            //add other parts, or add special method for properties available during imports
            Entity parentEntity = EntityManager.GetEntity(profile.RowId);
            //general
            helpers.UpdateRoles(item.DeleteOfferedBy, parentEntity, "Offered By", Entity_AgentRelationshipManager.ROLE_TYPE_OFFERED_BY, item.OfferedByList, user.Id, CurrentRowNbr, ref status);

            helpers.UpdateRoles(item.DeleteApprovedBy, parentEntity, "Approved By", Entity_AgentRelationshipManager.ROLE_TYPE_ApprovedBy, item.ApprovedByList, user.Id, CurrentRowNbr, ref status);

            helpers.UpdateRoles(item.DeleteAccreditedBy, parentEntity, "Accredited By", Entity_AgentRelationshipManager.ROLE_TYPE_AccreditedBy, item.AccreditedByList, user.Id, CurrentRowNbr, ref status);

            helpers.UpdateRoles(item.DeleteRecognizedBy, parentEntity, "Recognized By", Entity_AgentRelationshipManager.ROLE_TYPE_RECOGNIZED_BY, item.RecognizedByList, user.Id, CurrentRowNbr, ref status);

            helpers.UpdateRoles(item.DeleteRegulatedBy, parentEntity, "Regulated By", Entity_AgentRelationshipManager.ROLE_TYPE_REGULATED_BY, item.RegulatedByList, user.Id, CurrentRowNbr, ref status);
            // ===================================================
            if ( item.DeleteEstimatedDuration )
            {
                if ( !dpmgr.DeleteAll(profile.RowId, 0, ref messages) )
                    status.AddError(string.Format("Row: {0}, Issue encountered removing existing durations: {1}", item.RowNumber, statusMessage));
            }
            else
            {
                //estimate duration - only handling exact initially, or maybe description
                if ( item.HasEstimatedDuration )
                {
                    item.EstimatedDuration.EntityId = parentEntity.Id;
                    //for an existing cred, check for existing profiles. Approach may be to remove all, and then add, OR ??
                    item.EstimatedDuration.ParentUid = profile.RowId;
                    if ( recordExists )
                    {
                        List<DurationProfile> existing = DurationProfileManager.GetAll(profile.RowId);
                        if ( existing.Count > 0 )
                        {
                            foreach ( var dp in existing )
                            {
                                if ( !dpmgr.DurationProfile_Delete(dp.Id, ref statusMessage) )
                                    status.AddError(string.Format("Row: {0}, Issue encountered removing existing durations: {1}", item.RowNumber, statusMessage));
                            }
                        }
                    }

                    if ( dpmgr.Save(item.EstimatedDuration, user.Id, ref messages) == false )
                        status.AddErrorRange(string.Format("Row: {0}, Following issue encountered for Duration:", item.RowNumber), messages);
                }
            }

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

			LoggingHelper.DoTrace( 9, string.Format( "AssessmentImport.Import. Row: {0} ___________ industries", CurrentRowNbr ) );
			
			//industries
			if ( item.DeletingNaicsCodes )
			{
				if ( !efimgr.DeleteAll( parentEntity.EntityUid, CodesManager.PROPERTY_CATEGORY_NAICS, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Issue encountered removing SOC items from credential: {1}", item.RowNumber, item.Name ), messages );
				}
			}
			else if ( item.NaicsCodesList.Count > 0 )
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
           helpers.ReplaceEntityReferences( profile.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD, item.Keywords, "Keywords", user.Id, CurrentRowNbr, ref status );

            //         if ( !erefMgr.Replace( profile.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD, item.Keywords, user.Id, ref messages ) )
            //{
            //	status.AddErrorRange( string.Format( "Row: {0}, Issue encountered updating keywords", CurrentRowNbr), messages );
            //}
            helpers.ReplaceEntityReferences( profile.RowId, CodesManager.PROPERTY_CATEGORY_SUBJECT, item.Subjects, "Subjects", user.Id, CurrentRowNbr, ref status );
            
            //
            helpers.ReplaceEntityReferences( profile.RowId, CodesManager.PROPERTY_CATEGORY_NAICS, item.Industries, "Industries", user.Id, CurrentRowNbr, ref status );
            
            helpers.ReplaceEntityReferences( profile.RowId, CodesManager.PROPERTY_CATEGORY_SOC, item.Occupations, "Occupations", user.Id, CurrentRowNbr, ref status );
            
            helpers.ReplaceEntityReferences( profile.RowId, CodesManager.PROPERTY_CATEGORY_CIP, item.Programs, "Programs", user.Id, CurrentRowNbr, ref status );

            
            return newId;
        }
        private bool DoesAssessmentExist(BaseDTO input, ref ThisEntity record)
        {
            int ptotalRows = 0;
            string filter = "";
            record = new ThisEntity();

            string url = NormalizeUrlData(input.SubjectWebpage);

            if ( !string.IsNullOrWhiteSpace(input.CTID) )
            {
                filter = string.Format(" ( base.CTID = '{0}' ) ", input.CTID);
            }
            else if ( !string.IsNullOrWhiteSpace(input.ExternalIdentifier) )
            {
                filter = string.Format(" ( base.ExternalIdentifier = '{0}' AND OwningAgentUid = '{1}' )  ", input.ExternalIdentifier, input.OwningAgentUid);
            }
            else
            {   //shouldn't get to here
                //should actually return an error. Maybe add a check prior to here
                filter = string.Format(" ( base.Id in (Select Id from Assessment where (name = '{0}' AND Url = '{1}') )) ", input.Name, url);
            }

            List<ThisEntity> exists = AssessmentManager.Search(filter, "", 1, 25, 0, ref ptotalRows);
            if ( exists != null && exists.Count > 0 )
            {
                //note if multiple, but return first
                //180404 - mp - to preserve data, get everything, like for edit. Don't really need a lot of profiles, except to check for existance of these?
                record = AssessmentManager.Get(exists[ 0 ].Id);
                return true;
            }

            return false;
        }
        private void CommonSetup()
        {
            //assessMethod(56) - exam
            int asmtMethodCatId = CodesManager.PROPERTY_CATEGORY_Assessment_Method_Type;
            examCode = CodesManager.GetPropertyBySchema( "ceterms:AssessmentMethod", "assessMethod:Exam" );
            //deliveryType (21) - in-person, online
            int deliveryTypeCatId = CodesManager.PROPERTY_CATEGORY_DELIVERY_TYPE;
            inPersonCode = CodesManager.GetPropertyBySchema( "ceterms:Delivery", "deliveryType:InPerson" );
            onlineCode = CodesManager.GetPropertyBySchema( "ceterms:Delivery", "deliveryType:OnlineOnly" );
            blendedCode = CodesManager.GetPropertyBySchema( "ceterms:Delivery", "deliveryType:BlendedDelivery" );
        }
    }
}
