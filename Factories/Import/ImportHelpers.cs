using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Common;
using Models.Import;
using MN = Models.Node;
using Models.ProfileModels;
using EM = Data;
using Utilities;

namespace Factories.Import
{
	public class ImportHelpers : BaseFactory
	{
		public string thisClassName = "ImportHelpers";
		AddressProfileManager addrMgr = new AddressProfileManager();
		Entity_AgentRelationshipManager earMgr = new Entity_AgentRelationshipManager();
		Entity_AssessmentManager eaMgr = new Entity_AssessmentManager();
		Entity_CommonConditionManager eCommonCndsMgr = new Entity_CommonConditionManager();
		Entity_CommonCostManager eCommonCostsMgr = new Entity_CommonCostManager();
		Entity_ConditionProfileManager cpMgr = new Entity_ConditionProfileManager();
		CostProfileManager costProfileManager = new CostProfileManager();
		CostProfileItemManager costProfileItemManager = new CostProfileItemManager();
		Entity_CredentialManager ecrMgr = new Entity_CredentialManager();
		Entity_LearningOpportunityManager eloMgr = new Entity_LearningOpportunityManager();
		Entity_ReferenceManager erefMgr = new Entity_ReferenceManager();

		public static string CurrentEntityType = "unknown";

		public string DELETE_ME = "#DELETE";

		public void HandleTargetCredentials( List<Credential> targetCredentials, ParentObject parent, int conditionTypeId, int subConditionTypeId, List<ConditionProfile> connectionProfiles, int userId, int currentRowNbr, ref ImportStatus status )
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
			foreach ( var credential in targetCredentials )
			{
				if ( credential.OwningAgentUid != parent.OwningAgentUid )
				{
					//what - create a connection profile
					//      - note could be acceptable within multiple universities
					//Need to reverse the connection type from required (1) to is required for (3), and recommended (2) to is recommended for (4)
					//need a description
					ConditionProfileDTO cp = new ConditionProfileDTO
					{
						ConditionTypeId = conditionTypeId
					};
					if ( cp.ConditionTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Requirement )
					{
						cp.ConditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_NextIsRequiredFor;
						cp.ConditionType = "Is Required For";
						cp.Description = string.Format( "This {0} is required for credential '{1}'", parent.EntityType.ToLower(), credential.Name );
					}
					else if ( cp.ConditionTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Recommendation )
					{
						cp.ConditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_NextIsRecommendedFor;
						cp.ConditionType = "Is Recommended For";
						cp.Description = string.Format( "This {0} is recommended for credential '{1}'", parent.EntityType.ToLower(), credential.Name );
					}
					else
					{
						cp.Description = string.Format( "This {0} has a connection for credential '{1}'", parent.EntityType.ToLower(), credential.Name );
					}
					cp.TargetCredentialList.Add( credential );
					cp.ConditionSubTypeId = subConditionTypeId;
					//ensure this does updates as needed, not just adds
					//should use AssessmentConnections not AllConditions
					//ensure 
					if ( !HandleConnection( cp, parent, connectionProfiles, userId, true, currentRowNbr, ref status ) )
					{
						//messages should be handled
					}
				}
				else
				{
					//add under credential
					ConditionProfileDTO cp = new ConditionProfileDTO();
					cp.ConditionTypeId = conditionTypeId;
					//this step will hurt reuse
					//cp.TargetAssessmentList.Add( asmt );
					if ( cp.ConditionTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Requirement )
					{
						cp.ConditionType = "Requires";
						cp.Description = string.Format( "This {0} is required for credential '{1}'", parent.EntityType.ToLower(), credential.Name );
					}
					else if ( cp.ConditionTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Recommendation )
					{
						cp.ConditionType = "Recommends";
						cp.Description = string.Format( "This {0} is recommended for credential '{1}'", parent.EntityType.ToLower(), credential.Name );
					}
					else
					{
						cp.Description = string.Format( "This {0} has a connection for credential '{1}'", parent.EntityType.ToLower(), credential.Name );

					}

					ParentObject cpo = new ParentObject() { Id = credential.Id, RowId = credential.RowId, Name = credential.Name, OwningAgentUid = credential.OwningAgentUid, ParentTypeId = 1, EntityType = "Credential" };
					if ( !HandleConditionProfile( cp, cpo, credential.AllConditions, userId, true, currentRowNbr, ref status ) )
					{
						//messages should be handled
					}
				}
			}
		} //
		public void HandleConditionProfiles( List<ConditionProfileDTO> profiles, ParentObject po, List<ConditionProfile> existingProfiles, int userId, bool parentExists, int currentRowNbr, ref ImportStatus status )
		{
			string statusMessage = "";
			foreach ( var cp in profiles )
			{
				if ( cp.DeletingProfile )
				{
					if ( new Entity_ConditionProfileManager().Delete( cp.RowId, ref statusMessage ) == false )
					{
						status.AddError( string.Format( "Row: {0}, Error removing Condition Profile: {1}", currentRowNbr, statusMessage ) );
					}
				}
				else
				{
					if ( !HandleConditionProfile( cp, po, existingProfiles, userId, parentExists, currentRowNbr, ref status ) )
					{
						//actions?
					}
				}
			}
		} //
		  /// <summary>
		  /// NOTE: this can be also called for a asmt BU where the asmt org <> credential org, and an connection will be added to the asmt, rather than a condition profile being created for the credential
		  /// </summary>
		  /// <param name="profile"></param>
		  /// <param name="parentEntityTypeId"></param>
		  /// <param name="parentUid"></param>
		  /// <param name="parentId"></param>
		  /// <param name="parentName"></param>
		  /// <param name="parentOwningOrgUid"></param>
		  /// <param name="parentConditionProfiles"></param>
		  /// <param name="userId"></param>
		  /// <param name="parentExists"></param>
		  /// <param name="currentRowNbr"></param>
		  /// <param name="status"></param>
		  /// <returns></returns>
		public bool HandleConditionProfile( ConditionProfileDTO profile, ParentObject parent, List<ConditionProfile> parentConditionProfiles, int userId, bool parentExists, int currentRowNbr, ref ImportStatus status )
		{
			bool isValid = true;
			List<string> messages = new List<string>();
			string statusMessage = "";
			bool skippingConditionTargets = false;
			ConditionProfile cp = new ConditionProfile();
			cp.ConnectionProfileTypeId = profile.ConditionTypeId;
			cp.ConditionSubTypeId = 1;
			bool profileExists = false;
			bool matchesExisting = false;

			if ( parentExists )
			{
				//if an existing parent, get conditions, and if more than one, fail
				//NOTE: in the check for parent existing (ex. DoesCredentialExist), the Get will retrieve all the condition profiles
				if ( parentConditionProfiles != null && parentConditionProfiles.Count > 0 )
				{
					if ( parentConditionProfiles.Count == 1 )
					{
						//should check the type, also will need to eventually handle multiples
						//actually multiples will require identifiers
						cp = parentConditionProfiles[ 0 ];
						if ( cp.ConnectionProfileTypeId != profile.ConditionTypeId
							&& profile.ConditionTypeId > 0 )
						{
							//will we allow changing the type - convenient if careful
							cp.ConnectionProfileTypeId = profile.ConditionTypeId;
						}
						matchesExisting = true;
						profileExists = true;
					}
					else
					{
						//** more than one condition **

						//could use a provided identifier
						if ( IsValidGuid( profile.RowId ) )
						{
							//may need to get all depending on how the save works!
							cp = Entity_ConditionProfileManager.GetForImport( profile.RowId );
							if ( cp == null || cp.Id == 0 )
							{
								status.AddError( string.Format( "Row: {0}. The provided identifier ({1}) for the condition profile is invalid - not found. Update of condition profile was skipped.", currentRowNbr, profile.RowId ) );
								return false;
							}
							matchesExisting = true;
							profileExists = true;
						}
						else
						{
							//could check for if only one matches the input type, of course means can't change the type
							var matches = parentConditionProfiles.FindAll( s => s.ConnectionProfileTypeId == profile.ConditionTypeId );
							if ( matches != null && matches.Count == 1 )
							{
								//small risk
								cp = matches[ 0 ];
								matchesExisting = true;
								profileExists = true;
							}
							else
							{
								//reject
								status.AddError( string.Format( "Row: {0}. The existing credential has multiple condition profiles and an identifier was not provided for the input condition profile, so the system cannot determine which condition to update. Update of condition profile was skipped. Do an export to get the identifier of the condition to update. ", currentRowNbr, profile.RowId ) );
								return false;
							}
						}

					}
				}
			}
			else
			{
				//handling is TBD
				if ( IsValidGuid( profile.RowId ) )
				{
					cp.RowId = profile.RowId;
				}
			}
			if ( profileExists )
			{
				cp.LastUpdatedById = userId;
				if (
						( profile.Description ?? "" ) == cp.Description ||
						( profile.SubjectWebpage ?? "" ) == cp.SubjectWebpage

					)
					matchesExisting = true;
			}

			//required, so can't set blank
			if ( !profileExists || !string.IsNullOrWhiteSpace( profile.Description ) )
				cp.Description = Assign( profile.Description, cp.Description, profileExists );

			if ( !profileExists || !string.IsNullOrWhiteSpace( profile.SubjectWebpage ) )
				cp.SubjectWebpage = AssignUrl( profile.SubjectWebpage, cp.SubjectWebpage, profileExists );

			if ( !profileExists || !string.IsNullOrWhiteSpace( profile.Name ) )
				cp.ProfileName = Assign( profile.Name, cp.ProfileName, profileExists );

			if ( !profile.IsAConnectionProfile )
			{

            }

			if ( profile.SubmissionItems.Count > 0 )
			{
				if ( profile.SubmissionItems[ 0 ] == DELETE_ME )
				{
					if ( profileExists )
					{
						string statusMsg = "";
						if ( !erefMgr.DeleteAll( cp.RowId, CodesManager.PROPERTY_CATEGORY_SUBMISSION_ITEM, ref statusMsg ) )
						{
							status.AddError( string.Format( "Row: {0}, Issue encountered removing submission items from the condition profile: {1}, Message: {2}", currentRowNbr, cp.Name, statusMsg ) );
						}
					}
				}
				else
				{
					//always have to clear, before adding
					if ( profileExists )
					{
						string statusMsg = "";
						if ( !erefMgr.DeleteAll( cp.RowId, CodesManager.PROPERTY_CATEGORY_SUBMISSION_ITEM, ref statusMsg ) )
						{
							status.AddError( string.Format( "Row: {0}, Issue encountered removing submission items from the condition profile: {1}, Message: {2}", currentRowNbr, cp.Name, statusMsg ) );
						}
					}
					foreach ( var word in profile.SubmissionItems )
					{
						cp.SubmissionOf.Add( new TextValueProfile() { TextValue = word } );
					}
				}
			}
			if ( profile.ConditionItems.Count > 0 )
			{
				if ( profile.ConditionItems[ 0 ] == DELETE_ME )
				{
					if ( profileExists )
					{
						string statusMsg = "";
						if ( !erefMgr.DeleteAll( cp.RowId, CodesManager.PROPERTY_CATEGORY_CONDITION_ITEM, ref statusMsg ) )
						{
							status.AddError( string.Format( "Row: {0}, Issue encountered removing condition items from the condition profile: {1}, Message: {2}", currentRowNbr, cp.Name, statusMsg ) );
						}
					}
				}
				else
				{
					//always have to clear, before adding
					if ( profileExists )
					{
						string statusMsg = "";
						if ( !erefMgr.DeleteAll( cp.RowId, CodesManager.PROPERTY_CATEGORY_CONDITION_ITEM, ref statusMsg ) )
						{
							status.AddError( string.Format( "Row: {0}, Issue encountered removing condition items from the condition profile: {1}, Message: {2}", currentRowNbr, cp.Name, statusMsg ) );
						}
					}
					foreach ( var word in profile.ConditionItems )
					{
						cp.Condition.Add( new TextValueProfile() { TextValue = word } );
					}
				}
			}

			//if ( !profileExists || !string.IsNullOrWhiteSpace( item.RequiresCondition.RequiresExperience ) )
			cp.Experience = Assign( profile.Experience, cp.Experience, profileExists );

			//setting to 0 is equivalent to deleting
			//if ( !profileExists
			//    || profile.YearsOfExperience > 0 )
			//    cp.YearsOfExperience = profile.YearsOfExperience;
			cp.YearsOfExperience = Assign( profile.YearsOfExperience, cp.YearsOfExperience, profileExists );

			#region credit hours/units
			//need to add an edit for hours or units!
			cp.CreditHourType = Assign( profile.CreditHourType, cp.CreditHourType, profileExists );
			cp.CreditUnitTypeDescription = Assign( profile.CreditUnitTypeDescription, cp.CreditUnitTypeDescription, profileExists );
			cp.CreditHourValue = Assign( profile.CreditHourValue, cp.CreditHourValue, profileExists );
			cp.CreditUnitValue = Assign( profile.CreditUnitValue, cp.CreditUnitValue, profileExists );
			if ( profile.CreditUnitType == DELETE_ME )
			{
				if ( profileExists )
					new EntityPropertyManager().DeleteAll( cp.RowId, CodesManager.PROPERTY_CATEGORY_CREDIT_UNIT_TYPE, ref statusMessage );
			}
			else if ( profile.CreditUnitTypeId > 0 )
			{
				cp.CreditUnitTypeId = profile.CreditUnitTypeId;
			}

			#endregion

			//code to construct name or description for auto profiles
			//would be better to assume done prior to getting here!
			string targetName = "";
			if ( profile.TargetAssessmentList != null && profile.TargetAssessmentList.Count() > 0 )
			{
				targetName = profile.TargetAssessmentList[ 0 ].Name;
			}
			else if ( profile.TargetLearningOpportunityList != null && profile.TargetLearningOpportunityList.Count() > 0 )
			{
				targetName = profile.TargetLearningOpportunityList[ 0 ].Name;
			}
			else if ( profile.TargetCredentialList != null && profile.TargetCredentialList.Count() > 0 )
			{
				targetName = profile.TargetCredentialList[ 0 ].Name;
			}

			if ( string.IsNullOrEmpty( cp.Description ) )
			{
				//in cases where connection is generated, will not have a description
				if ( !string.IsNullOrWhiteSpace( targetName ) )
					cp.Description = string.Format( "Condition created for: '{0}'.", targetName );
				else
				{
					cp.Description = "Condition for " + parent.Name;
				}
			}
			if ( parentExists )
			{
				//for existing cred, not sure we can create a condition, plus risky adding to existing CP
				//may be useful to have a merge conditions process????
				//could try to get all and see if only one, and possibly if a required one
				//Entity_ConditionProfileManager.FillConditionProfilesForList( entity, false );
				if ( parentConditionProfiles == null || parentConditionProfiles.Count == 0 )
				{
					SaveConditionProfile( parent.Name, parent.RowId, parent.OwningAgentUid, cp, cpMgr, userId, targetName, ref messages );

				}
				else
				{
					if ( matchesExisting )
					{
						SaveConditionProfile( parent.Name, parent.RowId, parent.OwningAgentUid, cp, cpMgr, userId, targetName, ref messages );
					}
					else
					{
						//a requires cp exists, so either abandon all asmts or create asmts as orphans

						status.AddWarning( string.Format( "Credential exists and more than one condition profile. A unique internal or external identifier was not provided. To avoid issues and incorrect guesses, the system will not be updating the condition, or adding assesments to any condition. <br>____These asmts will need to be manually added to the appropriate Condition Profile Name: {0}", cp.ProfileName ) );
						skippingConditionTargets = true;
					}
				}
			}
			else
			{
				SaveConditionProfile( parent.Name, parent.RowId, parent.OwningAgentUid, cp, cpMgr, userId, targetName, ref messages );

			}
			//
			if ( !string.IsNullOrWhiteSpace( profile.ExternalIdentifier ) )
			{
				//may want to do this for existing as well
				if ( !profileExists )
				{
					int nId = new ImportHelpers().ExternalIdentifierXref_Add( parent.ParentTypeId, parent.Id, CodesManager.ENTITY_TYPE_CONDITION_PROFILE, profile.ExternalIdentifier, cp.RowId, userId, ref messages );
				}
			}
			//
			if ( profile.DeleteTargetCredentials )
			{
				if ( !ecrMgr.DeleteAll( cp.RowId, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Following issue(s) were encountered attempting to remove all target credentials from the condition profile.", currentRowNbr ), messages );
				}
			}
			else
			{
				//first do a method handle deletes not in current list
				ecrMgr.DeleteNotInList( cp.RowId, profile.TargetCredentialList, ref messages );
				foreach ( var item in profile.TargetCredentialList )
				{
					if ( item != null && item.Id > 0 )
					{
						SaveConditionProfileTarget( parent.ParentTypeId, parent.RowId, cp, userId, item.Name, item.Id, 1, skippingConditionTargets, currentRowNbr, ref messages, ref status );
					}
				}
			}
			if ( profile.DeleteTargetLearningOpportunities )
			{
				if ( !eloMgr.DeleteAll( cp.RowId, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Following issue(s) were encountered attempting to remove all target learning opportunities from the condition profile.", currentRowNbr ), messages );
				}
			}
			else
			{
				//first do a method handle deletes not in current list
				//note may need to remove from under the credential!!
				eloMgr.DeleteNotInList( cp.RowId, profile.TargetLearningOpportunityList, ref messages );
				foreach ( var item in profile.TargetLearningOpportunityList )
				{
					if ( item != null && item.Id > 0 )
					{
						SaveConditionProfileTarget( parent.ParentTypeId, parent.RowId, cp, userId, item.Name, item.Id, 7, skippingConditionTargets, currentRowNbr, ref messages, ref status );
					}
				}
			}
			if ( profile.DeleteTargetAssessments )
			{
				if ( !eaMgr.DeleteAll( cp.RowId, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Following issue(s) were encountered attempting to remove all target assessments from the condition profile.", currentRowNbr ), messages );
				}
			}
			else
			{
				//first do a method handle deletes not in current list
				//note may need to remove from under the credential!!
				eaMgr.DeleteNotInList( cp.RowId, profile.TargetAssessmentList, ref messages );
				foreach ( var item in profile.TargetAssessmentList )
				{

					if ( item != null && item.Id > 0 )
					{
						SaveConditionProfileTarget( parent.ParentTypeId, parent.RowId, cp, userId, item.Name, item.Id, 3, skippingConditionTargets, currentRowNbr, ref messages, ref status );

					}
				}
			}
			//
			

            return isValid;
        }
		public void HandleConnectionProfiles( List<ConditionProfileDTO> profiles, ParentObject po, List<ConditionProfile> existingConnections, int userId, bool parentExists, int currentRowNbr, ref ImportStatus status )
		{
			string statusMessage = "";
			foreach ( var cp in profiles )
			{
				if ( cp.DeletingProfile )
				{
					if ( new Entity_ConditionProfileManager().Delete( cp.RowId, ref statusMessage ) == false )
					{
						status.AddError( string.Format( "Row: {0}, Error removing Condition Profile: {1}", currentRowNbr, statusMessage ) );
					}
				}
				else
				{
					if ( !HandleConnection( cp, po, existingConnections, userId, parentExists, currentRowNbr, ref status ) )
					{
						//actions?
					}
				}
			}
		} //
		public bool HandleConnection( ConditionProfileDTO profile, ParentObject parent, List<ConditionProfile> parentConnectionProfiles, int userId,
				 bool parentExists, int currentRowNbr, ref ImportStatus status )
		{

			//int parent.ParentTypeId, Guid parent.RowId, int parent.Id, string parent.Name, Guid parent.OwningAgentUid, 
			bool isValid = true;
			List<string> messages = new List<string>();
			string statusMessage = "";
			bool skippingConditionTargets = false;
			ConditionProfile cp = new ConditionProfile();
			cp.ConnectionProfileTypeId = profile.ConditionTypeId;
			cp.ConditionSubTypeId = profile.ConditionSubTypeId;
			bool profileExists = false;
			bool matchesExisting = false;


			if ( parentExists )
			{
				//if an existing cred, get conditions, and if more than one, fail
				//NOTE: in DoesCredentialExist, the GetForEdit will retrieve all the condition profiles
				if ( parentConnectionProfiles != null && parentConnectionProfiles.Count > 0 )
				{
					if ( parentConnectionProfiles.Count == 1 )
					{
						//should check the type, also will need to eventually handle multiples
						//actually multiples will require identifiers
						cp = parentConnectionProfiles[ 0 ];
						if ( cp.ConnectionProfileTypeId != profile.ConditionTypeId
							&& profile.ConditionTypeId > 0 )
						{
							//will we allow changing the type - convenient if careful
							cp.ConnectionProfileTypeId = profile.ConditionTypeId;
						}
						matchesExisting = true;
						profileExists = true;
					}
					else
					{
						//** more than one condition **

						//could use a provided identifier
						if ( IsValidGuid( profile.RowId ) )
						{
							//may need to get all depending on how the save works!
							cp = Entity_ConditionProfileManager.GetForImport( profile.RowId );
							if ( cp == null || cp.Id == 0 )
							{
								status.AddError( string.Format( "Row: {0}. The provided identifier ({1}) for the connection profile is invalid - not found. Update of connection profile was skipped.", currentRowNbr, profile.RowId ) );
								return false;
							}
							matchesExisting = true;
							profileExists = true;
						}
						else
						{
							//could check for if only one matches the input type, of course means can't change the type
							var matches = parentConnectionProfiles.FindAll( s => s.ConnectionProfileTypeId == profile.ConditionTypeId );
							if ( matches != null && matches.Count == 1 )
							{
								//small risk
								cp = matches[ 0 ];
								matchesExisting = true;
								profileExists = true;
							}
							else
							{
								//reject
								status.AddError( string.Format( "Row: {0}. The existing artifact has multiple connection profiles and an identifier was not provided for the input connection profile, so the system cannot determine which condition to update. Update of connection profile was skipped. Do an export to get the identifier of the connection to update. ", currentRowNbr ) );
								return false;
							}
						}

					}
				}
			}
			else
			{
				//handling is TBD
				if ( IsValidGuid( profile.RowId ) )
				{
					cp.RowId = profile.RowId;
				}
			}
			if ( profileExists )
			{
				cp.LastUpdatedById = userId;
				if (
						( profile.Description ?? "" ) == cp.Description ||
						( profile.SubjectWebpage ?? "" ) == cp.SubjectWebpage

					)
				{
					matchesExisting = true;
				}
			}

			//code to construct name or description for auto profiles
			//would be better to assume done prior to getting here!
			string targetName = "";
			if ( profile.TargetAssessmentList != null && profile.TargetAssessmentList.Count() > 0 )
			{
				targetName = profile.TargetAssessmentList[ 0 ].Name;
			}
			else if ( profile.TargetLearningOpportunityList != null && profile.TargetLearningOpportunityList.Count() > 0 )
			{
				targetName = profile.TargetLearningOpportunityList[ 0 ].Name;
			}
			else if ( profile.TargetCredentialList != null && profile.TargetCredentialList.Count() > 0 )
			{
				targetName = profile.TargetCredentialList[ 0 ].Name;
			}

			//required, so can't set blank
			if ( !profileExists || !string.IsNullOrWhiteSpace( profile.Description ) )
				cp.Description = Assign( profile.Description, cp.Description, profileExists );
			if ( string.IsNullOrEmpty( cp.Description ) )
			{
				//in cases where connection is generated, will not have a description
				if ( !string.IsNullOrWhiteSpace( targetName ) )
					cp.Description = string.Format( "Connection created for: '{0}'.", targetName );
				else
				{
					cp.Description = "Connection for " + parent.Name;
				}
			}
			//need to add an edit for hours or units!
			cp.CreditHourType = Assign( profile.CreditHourType, cp.CreditHourType, profileExists );
			cp.CreditUnitTypeDescription = Assign( profile.CreditUnitTypeDescription, cp.CreditUnitTypeDescription, profileExists );
			cp.CreditHourValue = Assign( profile.CreditHourValue, cp.CreditHourValue, profileExists );
			cp.CreditUnitValue = Assign( profile.CreditUnitValue, cp.CreditUnitValue, profileExists );
			if ( profile.CreditUnitType == DELETE_ME )
			{
				if ( profileExists )
					new EntityPropertyManager().DeleteAll( cp.RowId, CodesManager.PROPERTY_CATEGORY_CREDIT_UNIT_TYPE, ref statusMessage );
			}
			else if ( profile.CreditUnitTypeId > 0 )
			{
				cp.CreditUnitTypeId = profile.CreditUnitTypeId;
			}



			if ( parentExists )
			{
				//for existing cred, not sure we can create a condition, plus risky adding to existing CP
				//may be useful to have a merge conditions process????
				//could try to get all and see if only one, and possibly if a required one
				//Entity_ConditionProfileManager.FillConditionProfilesForList( entity, false );
				if ( parentConnectionProfiles == null || parentConnectionProfiles.Count == 0 )
				{
					SaveConditionProfile( parent.Name, parent.RowId, parent.OwningAgentUid, cp, cpMgr, userId, "", ref messages );

				}
				else
				{
					if ( matchesExisting )
					{
						SaveConditionProfile( parent.Name, parent.RowId, parent.OwningAgentUid, cp, cpMgr, userId, "", ref messages );
					}
					else
					{
						//a requires cp exists, so either abandon all asmts or create asmts as orphans

						status.AddWarning( string.Format( "Parent artifact exists and more than one connection profile was found. A unique internal or external identifier was not provided. To avoid issues and incorrect guesses, the system will not be updating the connection.<br>____These asmts will need to be manually added to the appropriate connection Profile Name: {0}", cp.ProfileName ) );
						skippingConditionTargets = true;
					}
				}
			}
			else
			{
				SaveConditionProfile( parent.Name, parent.RowId, parent.OwningAgentUid, cp, cpMgr, userId, "", ref messages );
				if ( messages.Count > 0 )
				{
					status.SetMessages( messages, false );
					return false;
				}

			}
			//
			if ( !string.IsNullOrWhiteSpace( profile.ExternalIdentifier ) )
			{
				//may want to do this for existing as well
				//TODO determinist if there is any approach
				if ( !profileExists )
				{
					int nId = new ImportHelpers().ExternalIdentifierXref_Add( parent.ParentTypeId, parent.Id, CodesManager.ENTITY_TYPE_CONDITION_PROFILE, profile.ExternalIdentifier, cp.RowId, userId, ref messages );
				}
			}
			if ( profile.DeleteTargetCredentials )
			{
				if ( !ecrMgr.DeleteAll( cp.RowId, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Following issue(s) were encountered attempting to remove all target credentials from the condition profile.", currentRowNbr ), messages );
				}
			}
			else
			{
				//first do a method handle deletes not in current list
				ecrMgr.DeleteNotInList( cp.RowId, profile.TargetCredentialList, ref messages );
				foreach ( var item in profile.TargetCredentialList )
				{
					if ( item != null && item.Id > 0 )
					{
						SaveConditionProfileTarget( parent.ParentTypeId, parent.RowId, cp, userId, item.Name, item.Id, 1, skippingConditionTargets, currentRowNbr, ref messages, ref status );
					}
				}
			}
			if ( profile.DeleteTargetLearningOpportunities )
			{
				if ( !eloMgr.DeleteAll( cp.RowId, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Following issue(s) were encountered attempting to remove all target learning opportunities from the condition profile.", currentRowNbr ), messages );
				}
			}
			else
			{
				//first do a method handle deletes not in current list
				//note may need to remove from under the credential!!
				eloMgr.DeleteNotInList( cp.RowId, profile.TargetLearningOpportunityList, ref messages );
				foreach ( var item in profile.TargetLearningOpportunityList )
				{
					if ( item != null && item.Id > 0 )
					{
						SaveConditionProfileTarget( parent.ParentTypeId, parent.RowId, cp, userId, item.Name, item.Id, 7, skippingConditionTargets, currentRowNbr, ref messages, ref status );
					}
				}
			}
			if ( profile.DeleteTargetAssessments )
			{
				if ( !eaMgr.DeleteAll( cp.RowId, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Following issue(s) were encountered attempting to remove all target assessments from the condition profile.", currentRowNbr ), messages );
				}
			}
			else
			{
				//first do a method handle deletes not in current list
				//note may need to remove from under the credential!!
				eaMgr.DeleteNotInList( cp.RowId, profile.TargetAssessmentList, ref messages );
				foreach ( var item in profile.TargetAssessmentList )
				{

					if ( item != null && item.Id > 0 )
					{
						SaveConditionProfileTarget( parent.ParentTypeId, parent.RowId, cp, userId, item.Name, item.Id, 3, skippingConditionTargets, currentRowNbr, ref messages, ref status );

                    }
                }
            }

			return isValid;
		}

		public void SaveConditionProfileTarget( int parentEntityTypeId, Guid parentUid, ConditionProfile cp, int userId, string artifactName, int artifactId, int artifactEntityTypeId, bool skippingConditionTargets, int currentRowNbr, ref List<string> messages, ref ImportStatus status )
		{
			messages = new List<string>();
			if ( skippingConditionTargets )
			{
				status.AddWarning( string.Format( "___ Will need to add this Artifact to a condition profile. Name: {0}, Id: {1}", artifactName, artifactId ) );
				return;
			}
			else
			{
				int newId = 0;

				if ( artifactEntityTypeId == 1 )
				{
					if ( ecrMgr.Add( artifactId, cp.RowId, userId, true, ref newId, ref messages ) == 0 )
					{
						if ( messages.Count > 0 )
							status.AddErrorRange( string.Format( "Row: {0}, Following issue encountered attempting to add credential (id: {1}, Name: {2} to the condition profile:", currentRowNbr, artifactId, artifactName ), messages );
					}

				}
				else if ( artifactEntityTypeId == 3 )
				{
					//add to condition, doing always now
					if ( eaMgr.Add( cp.RowId, artifactId, userId, true, ref messages ) == 0 )
					{
						if ( messages.Count > 0 )
							status.AddErrorRange( string.Format( "Row: {0}, Following issue encountered attempting to add assessment (id: {1}, Name: {2} to the condition profile:", currentRowNbr, artifactId, artifactName ), messages );
					}

					//add assessment to credential/parent - actually only if a credential
					//really don't like doing this!!!
					if ( parentEntityTypeId == 1 )
					{
						if ( eaMgr.Add( parentUid, artifactId, userId, true, ref messages ) == 0 )
						{
							if ( messages.Count > 0 )
								status.AddErrorRange( string.Format( "Row: {0}, Following issue encountered attempting to add assessment to the credential:", currentRowNbr ), messages );
						}
					}
				}
				else if ( artifactEntityTypeId == 7 )
				{
					//add to condition, doing always now
					if ( eloMgr.Add( cp.RowId, artifactId, userId, true, ref messages ) == 0 )
					{
						if ( messages.Count > 0 )
							status.AddErrorRange( string.Format( "Row: {0}, Following issue encountered attempting to add learning opportunity (id: {1}, Name: {2} to the condition profile:", currentRowNbr, artifactId, artifactName ), messages );
					}

					//add assessment to credential/parent - actually only if a credential
					//really don't like doing this!!!
					if ( parentEntityTypeId == 1 )
					{
						if ( eloMgr.Add( parentUid, artifactId, userId, true, ref messages ) == 0 )
						{
							if ( messages.Count > 0 )
								status.AddErrorRange( string.Format( "Row: {0}, Following issue encountered attempting to add assessment to the credential:", currentRowNbr ), messages );
						}
					}
				}
			}
		}

		public void SaveConditionProfile( string parentName, Guid parentUid, Guid owningOrgUid, ConditionProfile cp, Entity_ConditionProfileManager cpMgr, int defaultUserId, string targetName, ref List<string> messages, bool isAdditionalCondition = false )
		{
			//cp = new ConditionProfile();
			cp.ProfileName = string.IsNullOrWhiteSpace( cp.ProfileName ) ? "Conditions for " + parentName : cp.ProfileName;
			if ( isAdditionalCondition )
				cp.ProfileName = "Additional " + cp.ProfileName;

			cp.ProfileSummary = cp.ProfileName;
			//should be resolved by now
			if ( string.IsNullOrEmpty( cp.Description ) )
			{
				if ( !string.IsNullOrWhiteSpace( targetName ) )
					cp.Description = string.Format( "To earn this artifact, candidates must complete the referenced target(s), such as: '{0}'.", targetName );
			}
			cp.ConnectionProfileTypeId = cp.ConnectionProfileTypeId > 0 ? cp.ConnectionProfileTypeId : 1;
			cp.ConditionSubTypeId = cp.ConditionSubTypeId > 0 ? cp.ConditionSubTypeId : 1;

			cp.AssertedByAgentUid = owningOrgUid;
			cp.CreatedById = defaultUserId;
			cpMgr.Save( cp, parentUid, defaultUserId, ref messages );

			//would need to now add the target assessment child
		}


        public void HandleCompetencyFrameworks( BaseDTO request, Guid parentUid, AppUser user,
                bool parentExists, ref ImportStatus status )
        {
            bool isValid = true;
            List<string> messages = new List<string>();
            messages = new List<string>();



            if ( request.DeleteFrameworks )
            {
                //need to be careful

                var existingFrameworks = EducationFrameworkManager.GetAllFrameworksForParent( parentUid );
                if ( parentExists )
                {
                    Entity_CompetencyManager ecMgr = new Entity_CompetencyManager();
                    //add method to get any current frameworks for parent
                    foreach ( var item in existingFrameworks )
                    {
                        //existing framework not in current list, so delete all
                        ecMgr.DeleteAll( parentUid, item.Id, item.Name, user, ref messages );
                    }
                }
            }
            else if ( request.Frameworks.Count > 0 )
            {
                EducationFrameworkManager mgr = new EducationFrameworkManager();
                Entity_CompetencyManager ecMgr = new Entity_CompetencyManager();
                //Like other cases for lists, if any are provided, all are replaced
                //TODO - get all frameworks for aligned competencies
				//if frameworks are provided the action is replace. So existing frameworks not in the current list are deleted. 
                var existingFrameworks = EducationFrameworkManager.GetAllFrameworksForParent( parentUid );
                if ( parentExists )
                {
                    //add method to get any current frameworks for parent
                    foreach ( var item in existingFrameworks )
                    {
                        //if 
                        int index = request.Frameworks.FindIndex( a => a.Framework._IdAndVersion.ToLower() == item.FrameworkUrl.ToLower() );
                        if ( index == -1 )
                        {
                            //existing framework not in current list, so delete all
                            ecMgr.DeleteAll( parentUid, item.Id, item.Name, user, ref messages );
                        }

					}
				}
				//handle adds
				foreach ( var framework in request.Frameworks )
				{
					mgr.SaveCassCompetencyList( framework, parentUid, user, ref isValid, ref messages );
				}
				status.AddErrorRange( messages );
			}
		}

		/// <summary>
		/// Deletes any existing records, and then return list to add
		/// </summary>
		/// <param name="categoryId"></param>
		/// <param name="refList"></param>
		/// <param name="property"></param>
		/// <param name="parentExists"></param>
		/// <param name="parentUid"></param>
		/// <param name="currentRowNbr"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public List<TextValueProfile> UpdateEntityReferences( int categoryId, List<string> refList, string property, bool parentExists, Guid parentUid, int currentRowNbr, ref ImportStatus status )
		{
			List<string> messages = new List<string>();
			List<TextValueProfile> output = new List<TextValueProfile>();
			if ( refList != null && refList.Count > 0 )
			{
				if ( refList[ 0 ] == DELETE_ME )
				{
					if ( parentExists )
					{
						string statusMsg = "";
						if ( !erefMgr.DeleteAll( parentUid, categoryId, ref statusMsg ) )
						{
							status.AddError( string.Format( "Row: {0}, Issue encountered removing property: {1}, Message: {2}", currentRowNbr, property, statusMsg ) );
						}
					}
				}
				else
				{
					//an update is a replace, so clear regardless
					if ( parentExists )
					{
						string statusMsg = "";
						if ( !erefMgr.DeleteAll( parentUid, categoryId, ref statusMsg ) )
						{
							status.AddError( string.Format( "Row: {0}, Issue encountered removing property: {1}, Message: {2}", currentRowNbr, property, statusMsg ) );
						}
					}
					foreach ( var word in refList )
					{
						if ( output.Where( s => s.TextValue == word ).ToList().Count == 0 )
							output.Add( new TextValueProfile() { TextValue = word } );
					}
				}
			}

			return output;
		}
        public void ReplaceEntityReferences( Guid parentUid,int categoryId, List<string> refList, string property,int userId, int currentRowNbr, ref ImportStatus status )
        {
            List<string> messages = new List<string>();
            List<TextValueProfile> output = new List<TextValueProfile>();
            if ( refList != null && refList.Count > 0 )
            {
                if ( refList[ 0 ] == DELETE_ME )
                {
                    
                        string statusMsg = "";
                        if ( !erefMgr.DeleteAll( parentUid, categoryId, ref statusMsg ) )
                        {
                            status.AddError( string.Format( "Row: {0}, Issue encountered removing property: {1}, Message: {2}", currentRowNbr, property, statusMsg ) );
                        }
                    
                }
                else
                {
                    //an update is a replace, so clear regardless
                    if ( !erefMgr.Replace( parentUid, categoryId, refList, userId, ref messages ) )
                    {
                        status.AddErrorRange( string.Format( "Row: {0}, Issue encountered updating {1}", currentRowNbr, property ), messages );
                    }

                }
            }

         
        }

        public bool UpdateRoles( bool doingDelete, Entity parent, string roleType, int roleTypeId, List<Organization> orgList, int userId, int currentRowNbr, ref ImportStatus status )
		{
			List<string> messages = new List<string>();
			if ( doingDelete )
			{
				if ( !earMgr.DeleteAllForRoleType( parent.EntityUid, roleTypeId, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Issue encountered removing role types of {1}.", currentRowNbr, roleType ), messages );
				}
			}
			else if ( orgList != null && orgList.Count > 0 )
			{
				foreach ( var org in orgList )
				{
					if ( earMgr.Add( parent.Id, org.RowId, roleTypeId, true, userId, ref messages ) == 0 )
						status.AddErrorRange( string.Format( "Row: {0}, Issue encountered updating {1} for: {2}", currentRowNbr, roleType, org.Name ), messages );
				}
			}
			if ( messages.Count > 0 )
				return false;
			else
				return true;
		}


		public void HandleCommonConditions( BaseDTO request, Guid parentUid, List<ConditionManifest> CommonConditions, int userId,
				 bool parentExists, ref ImportStatus status )
		{
			List<string> messages = new List<string>();
			messages = new List<string>();
			if ( request.DeleteCommonConditions )
			{
				if ( parentExists )
				{
					//need to be careful
				}

			}
			else if ( request.CommonConditionsIdentifiers.Count > 0 )
			{
				List<int> existingItems = new List<int>();
				//Like other cases for lists, if any are provided, all are replaced
				if ( parentExists )
				{
					foreach ( var cm in CommonConditions )
					{
						int index = request.CommonConditionsIdentifiers.FindIndex( a => a == cm.Id );
						if ( index == -1 )
						{
							//delete
							eCommonCndsMgr.Delete( parentUid, cm.Id, ref messages );
						}
						else
							existingItems.Add( cm.Id );
					}
				}
				//handle adds
				foreach ( var cmi in request.CommonConditionsIdentifiers )
				{
					int index = existingItems.FindIndex( a => a == cmi );
					if ( index == -1 )
					{
						//add
						eCommonCndsMgr.Add( parentUid, cmi, userId, ref messages, true );
					}
				}
			}
		} //

		public void HandleCommonCosts( BaseDTO request, Guid parentUid, List<CostManifest> CommonCosts, int userId,
				 bool parentExists, ref ImportStatus status )
		{
			List<string> messages = new List<string>();
			messages = new List<string>();
			if ( request.DeleteCommonCosts )
			{
				if ( parentExists )
				{
					//need to be careful
				}

			}
			else if ( request.CommonCostsIdentifiers.Count > 0 )
			{
				List<int> existingItems = new List<int>();
				//Like other cases for lists, if any are provided, all are replaced
				if ( parentExists )
				{
					foreach ( var cm in CommonCosts )
					{
						int index = request.CommonCostsIdentifiers.FindIndex( a => a == cm.Id );
						if ( index == -1 )
						{
							//delete
							eCommonCostsMgr.Delete( parentUid, cm.Id, ref messages );
						}
						else
							existingItems.Add( cm.Id );
					}
				}
				//handle adds
				foreach ( var cmi in request.CommonCostsIdentifiers )
				{
					int index = existingItems.FindIndex( a => a == cmi );
					if ( index == -1 )
					{
						//add
						eCommonCostsMgr.Add( parentUid, cmi, userId, ref messages, true );
					}
				}
			}
		} //

		public void HandleCostProfiles( List<CostProfileDTO> profiles, int parentEntityTypeId, ParentObject po, int userId, bool parentExists, int currentRowNbr, ref ImportStatus status )
		{
			string statusMessage = "";
			foreach ( var cp in profiles )
			{
				if ( cp.DeletingProfile )
				{
					if ( new CostProfileManager().Delete( cp.Identifier, ref statusMessage ) == false )
					{
						status.AddError( string.Format( "Row: {0}, Error removing Cost Profile: {1}", currentRowNbr, statusMessage ) );
					}
				}
				else
				{
					if ( !HandleCostProfile( cp, parentEntityTypeId, po, userId, parentExists, currentRowNbr, ref status ) )
					{
						//actions?
					}
				}
			}
		} //

        public bool HandleCostProfile( CostProfileDTO profile, int parentEntityTypeId, ParentObject po, int userId, bool parentExists, int rowNumber, ref ImportStatus status )
        {
            bool isValid = true;
            List<string> messages = new List<string>();

			CostProfile cp = new CostProfile();
			bool profileExists = false;
			//TBD - to enable import from production, allow identifier with new parent. 
			if ( parentExists )
			{
				if ( profile.IsExistingCostProfile
				|| IsValidGuid( profile.Identifier ) )
				{
					cp = CostProfileManager.GetBasicProfile( profile.Identifier );
					if ( cp == null || cp.Id == 0 )
					{
						cp.RowId = profile.Identifier;
					}
					else
					{
						//will need to handle existing cost items
						profileExists = true;
					}
				}
			}
			else
			{
				//handling is TBD
				if ( IsValidGuid( profile.Identifier ) )
				{
					cp.RowId = profile.Identifier;
				}
			}

            //required, so can't set blank
            if ( !profileExists || !string.IsNullOrWhiteSpace( profile.Description ) )
                cp.Description = Assign( profile.Description, cp.Description, profileExists );

            if ( !profileExists || !string.IsNullOrWhiteSpace( profile.Name ) )
            {
                cp.ProfileName = Assign( profile.Name, cp.ProfileName, profileExists );
                if ( string.IsNullOrWhiteSpace( cp.ProfileName ) )
                    cp.ProfileName = "Cost Profile for " + po.Name;
            }

            if ( !profileExists || !string.IsNullOrWhiteSpace( profile.DetailsUrl ) )
                cp.DetailsUrl = AssignUrl( profile.DetailsUrl, cp.DetailsUrl, profileExists );

            if ( !profileExists || !string.IsNullOrWhiteSpace( profile.CurrencyType ) )
            {
                cp.Currency = Assign( profile.CurrencyType, cp.Currency, profileExists );
                cp.CurrencyTypeId = profile.CurrencyTypeId;
            }

            if ( costProfileManager.Save( cp, po.RowId, userId, ref messages ) )
            {

                if ( !string.IsNullOrWhiteSpace( profile.ExternalIdentifier ) )
                {
                    //may want to do this for existing as well
                    if ( !profileExists )
                    {
                        int nId = new ImportHelpers().ExternalIdentifierXref_Add( parentEntityTypeId, po.Id, CodesManager.ENTITY_TYPE_COST_PROFILE, profile.ExternalIdentifier, cp.RowId, userId, ref messages );
                    }
                }

				if ( profile.CostItems != null && profile.CostItems.Count > 0 )
				{
					//need to handle existing cost items
					//generally if from BU, can only have one
					messages = new List<string>();

					if ( !costProfileItemManager.Replace( cp.Id, profile.CostItems, userId, ref messages ) )
					{
						status.AddErrorRange( string.Format( "Row: {0}, Following issue encountered attempting to add a direct cost item to the credential:", rowNumber ), messages );
					}
				}
			}
			else
			{
				//may attempt to add items anyway?
				status.AddErrorRange( string.Format( "Row: {0}, Following issue encountered attempting to add a cost profile to the credential:", rowNumber ), messages );

                if ( cp.Id > 0 )
                {

                }
            }

            return isValid;
        }

		public int ExternalIdentifierXref_Add( int parentEntityTypeId, int parentBaseId, int inputEntityTypeId, string inputIdentifier, Guid targetRowId, int userId, ref List<string> messages )
		{
			int newId = 0;
			EM.Import_IdentifierToObjectXref efEntity = new Data.Import_IdentifierToObjectXref();

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					//need  a duplicates check
					//if ( ValidateProfile( entity, ref messages ) == false )
					//{
					//    return false;
					//}


					//add
					efEntity.ParentEntityTypeId = parentEntityTypeId;
					efEntity.ParentId = parentBaseId;
					efEntity.InputEntityTypeId = inputEntityTypeId;
					efEntity.InputIdentifier = inputIdentifier;
					efEntity.TargetRowId = targetRowId;
					efEntity.Created = DateTime.Now;
					efEntity.CreatedById = userId;

					context.Import_IdentifierToObjectXref.Add( efEntity );
					int count = context.SaveChanges();
					if ( count == 0 )
					{
						messages.Add( string.Format( " Unable to add Import Xref. parentEntityTypeId: {0}, parentBaseId: {1}, inputEntityTypeId: {2}, userId: {3}  ", parentEntityTypeId, parentBaseId, inputEntityTypeId, userId ) );
					}
					else
					{
						newId = efEntity.Id;
					}


				}
			}
			catch ( Exception ex )
			{
				//will fail on duplicates
				string message = FormatExceptions( ex );
				messages.Add( string.Format( " Unable to add Import Xref. parentEntityTypeId: {0}, parentBaseId: {1}, inputEntityTypeId: {2}, userId: {3} \r\n ", parentEntityTypeId, parentBaseId, inputEntityTypeId, userId ) + message );
			}

			return newId;
		}

		/// <summary>
		/// Get Guid related to an entity that can be retrieved by an external identifier
		/// </summary>
		/// <param name="parentEntityTypeId"></param>
		/// <param name="parentBaseId"></param>
		/// <param name="inputEntityTypeId"></param>
		/// <param name="externalIdentifier"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static Guid ExternalIdentifierXref_Get( int parentEntityTypeId, int parentBaseId, int inputEntityTypeId, string externalIdentifier, ref string status )
		{
			Guid targetRowId = new Guid();
			EM.Import_IdentifierToObjectXref efEntity = new Data.Import_IdentifierToObjectXref();

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					var item = context.Import_IdentifierToObjectXref
						.FirstOrDefault( s => s.ParentEntityTypeId == parentEntityTypeId
						 && s.ParentId == parentBaseId
						 && s.InputEntityTypeId == inputEntityTypeId
						 && s.InputIdentifier == externalIdentifier );

					if ( item != null && item.Id > 0 )
					{
						targetRowId = item.TargetRowId;
					}
				}
			}
			catch ( Exception ex )
			{
				string message = FormatExceptions( ex );
				status = string.Format( " Error encountered attempting to read Import Xref record. parentEntityTypeId: {0}, parentBaseId: {1}, inputEntityTypeId: {2}, inputIdentifier: {3} ", parentEntityTypeId, parentBaseId, inputEntityTypeId, externalIdentifier ) + message;
			}

			return targetRowId;
		}


		public string AssignUrl( string input, string currentValue, bool doesEntityExist )
		{
			//start with string
			string value = Assign( input, currentValue, doesEntityExist );
			if ( !string.IsNullOrWhiteSpace( value ) && ( value.ToLower() != ( currentValue ?? "" ).ToLower() ) )
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
		public string Assign( string input, string currentValue, bool doesEntityExist )
		{
			string value = "";
			if ( doesEntityExist )
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

		public decimal Assign( decimal input, decimal currentValue, bool doesEntityExist )
		{
			decimal value = 0;
			if ( doesEntityExist )
			{
				value = input == -99 ? 0 : input == -100 ? currentValue : input;
			}
			else if ( input > 0 )
			{
				//don't allow delete for initial
				value = input;
			}
			return value;
		}
		/// <summary>
		/// Input:
		/// 0 - entered false
		/// 1 - entered true
		/// 2 - entered delete
		/// 3 - no entry
		/// </summary>
		/// <param name="input"></param>
		/// <param name="currentValue"></param>
		/// <param name="doesEntityExist"></param>
		/// <returns></returns>
		public bool? Assign( int input, bool? currentValue, bool doesEntityExist )
		{
			bool? value = null;
			if ( doesEntityExist )
			{
				switch ( input )
				{
					case 0:
						value = false;
						break;
					case 1:
						value = true;
						break;
					case 2:
						value = null;
						break;
					default:
						value = currentValue;
						break;

				}
			}
			else
			{
				switch ( input )
				{
					case 0:
						value = false;
						break;
					case 1:
						value = true;
						break;
					default:
						value = null;
						break;

				}
			}
			return value;
		}

		public bool HandleAddresses( BaseDTO request, ParentObject parent, int userId, int currentRowNbr, ref ImportStatus status )
		{
			bool isValid = true;
			List<string> messages = new List<string>();
			if ( request.DeleteAvailableAt )
			{
				//need to be careful
				if ( !addrMgr.DeleteAll( parent.RowId, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Following issue(s) were encountered attempting to remove all addresses for this {1}.", currentRowNbr, parent.EntityType ), messages );
				}
			}
			else if ( request.AvailableAt.Count > 0 )
			{
				//TODO - confirm action will no longer be copy, but replace
				//      - need to check for addition of contact points
				if ( !addrMgr.CopyList( request.AvailableAt, parent.RowId, userId, ref messages ) )
				{
					status.AddErrorRange( string.Format( "Row: {0}, Issue encountered copying addresses for: {1}", currentRowNbr, parent.Name ), messages );
				}
			}

			return isValid;
		}
	}
	//public class ParentObject
	//{
	//	public ParentObject()
	//	{

	//	}
	//	public int Id{ get; set; }
	//	public Guid RowId { get; set; }
	//	public string Name { get; set; }
	//	public int ParentTypeId { get; set; }
	//	public Guid OwningAgentUid { get; set; }
	//}
}
