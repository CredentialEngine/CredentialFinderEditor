using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Factories;
using Models;
using Models.Common;
using MN = Models.Node;
using Models.ProfileModels;
using Utilities;
using ConditionMgr = Factories.Entity_ConditionProfileManager;
//using CredConditionManager = Factories.ConnectionProfileManager;

namespace CTIServices
{
	public class ConditionProfileServices : ServiceHelper
	{

		string thisClassName = "ConditionProfileServices";
		ActivityServices activityMgr = new ActivityServices();
		ConditionMgr cmgr = new ConditionMgr();
		public List<string> messages = new List<string>();

		#region ConditionProfile Profile
		/// <summary>
		/// Get a full ConditionProfile for editor (usually, so forEditView is true)
		/// TODO - this meds to be customized to only return  what is needed, in terms of profile links
		/// </summary>
		/// <param name="profileId"></param>
		/// <param name="includeProperties"></param>
		/// <param name="incudingResources"></param>
		/// <returns></returns>
		public static ConditionProfile ConditionProfile_GetForEdit( int profileId )
		{
			//bool includeProperties = true;
			//bool incudingResources = true;
			//if ( forEditView == false )
			//{
			//	includeProperties = false;
			//	incudingResources = false;
			//}
			ConditionProfile profile = ConditionMgr.GetForEdit( profileId );

			return profile;
		}
		//public static ConditionProfile ConditionProfile_Get( int profileId )
		//{
		//	bool includeProperties = true;
		//	bool incudingResources = false;
		//	bool forEditView = false;
		//	ConditionProfile profile = ConditionMgr.Get( profileId, includeProperties, incudingResources, forEditView );

		//	return profile;
		//}

		/// <summary>
		/// Get condition profile in order to get the owning org of the parent credential
		/// </summary>
		/// <param name="rowId"></param>
		/// <returns></returns>
		public static MN.ProfileLink GetAsProfileLink( Guid rowId )
		{
			return ConditionMgr.GetProfileLink( rowId );

		}

		public static Credential GetProfileParentCredential( Guid conditionProfileRowId )
		{
			Credential cred = new Credential();
			//can use the link method to get the minimal info
			MN.ProfileLink link = ConditionMgr.GetProfileLink( conditionProfileRowId );

			//was parent a credential?
			if (link != null 
				&& IsValidGuid(link.ParentEntityRowId )
				&& link.ParentEntityTypeId == CodesManager.ENTITY_TYPE_CREDENTIAL)
			{
				cred = CredentialServices.GetBasicCredentialAsLink( link.ParentEntityRowId );
			}

			return cred;
		}
		public bool ConditionProfile_Save( ConditionProfile entity, Guid parentUid, string action, AppUser user, ref string status )
		{
			bool isValid = true;
			List<String> messages = new List<string>();
			if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
			{
				status =  "Error - missing an identifier for the Condition Profile" ;
				return false;
			}
			if ( entity.ConnectionProfileTypeId < 1 && (entity.ConnectionProfileType ?? "").Length == 0 && !entity.IsStarterProfile )
			{
				status = "Error - a Condition Profile Type must be selected." ;
				return false;
			}
			try
			{
				Entity parent = EntityManager.GetEntity( parentUid );
				if ( !OrganizationServices.CanUserEditEntity( user, parent.Id, ref status ) )
				{
					status = "Error - you do not have authorization for this action.";
					return false;
				}
				//remove this if properly passed from client
				//plus need to migrate to the use of EntityId
				entity.ParentId = parent.Id;
				//if sure this is always provided, don't need userId in methods
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				if ( new ConditionMgr().Save( entity, parentUid, user.Id, ref messages ) )
				{
					if ( action == "Initial" )
					{
						status = "Created an initial Condition Profile. Please provide a meaningful name, and fill out the remainder of the profile";
						//test concept ==> the client is not checking for a message
						return true;
					}
					else
					{
						status = "Successfully Saved Condition Profile";
						activityMgr.AddActivity( "Condition Profile", action, string.Format( "{0} saved condition profile: {1} for {2}", user.FullName(), entity.ProfileName, parent.EntityType ), user.Id, 0, entity.Id );
					}
				}
				else
				{
					status += string.Join( ",", messages.ToArray() );
					return false;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".ConditionProfile_Save" );
				status = ex.Message;
				isValid = false;
			}

			return isValid;
		}

		/// <summary>
		/// Delete a condition profile
		/// System will ensure user has access to the parent
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="profileId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool ConditionProfile_Delete( Guid parentUid, int profileId, AppUser user, ref string status )
		{
			bool valid = true;

			ConditionMgr mgr = new ConditionMgr();
			try
			{
				Entity parent = EntityManager.GetEntity( parentUid );
				//validate access
				if ( !OrganizationServices.CanUserEditEntity( user, parent.Id, ref status ) )
				{
					status = "Error - you do not have authorization for this action.";
					return false;
				}

				valid = mgr.Delete( profileId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "Condition Profile", "Delete", string.Format( "{0} deleted Condition Profile {1} from {2} -  {3}", user.FullName(), profileId, parent.EntityType, parent.EntityBaseId ), user.Id, 0, profileId, parent.EntityBaseId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".ConditionProfile_Delete" );
				status = ex.Message;
				valid = false;
			}
			return valid;
		}


		public bool UpsertConditionProfileForLearningOpp( Guid credentialUid, int learningOppId, AppUser user, ref string status )
		{
			bool isValid = true;
		
			
			List<String> messages = new List<string>();
			if ( learningOppId == 0 || !BaseFactory.IsGuidValid( credentialUid ) )
			{
				status = "Error - missing an identifier for the Condition Profile Upsert";
				return false;
			}
			
			//check if credential has condition profiles (type 1 or 2)
			//if not create a new one and attach the learning opp
			//if not, hmmm, notify user to add to the lopp?
			//could send an email?
			//value of some system messages!!!

			try
			{
				Entity parent = EntityManager.GetEntity( credentialUid );
				Credential cred = CredentialManager.GetBasicWithConditions( credentialUid );
				if (cred.Requires.Count > 0  )
				{
					//user has to handle - send message
					status = "Multiple condition profiles already exist. You must manually add this learning opportunity to the correct one.";
					string statusMsg = "";
					new MessageServices().Add( new Data.Message() { SenderId = 1, ReceiverId = user.Id, DeliveryMethod = 1, Content = status }, ref statusMsg );

					return false;
				}

				ConditionProfile entity = new ConditionProfile();
				entity.ConnectionProfileTypeId = 1;
				entity.ConditionSubTypeId = 1;
				entity.IsStarterProfile = true;
				entity.AssertedByAgentUid = cred.OwningAgentUid;
				LearningOpportunityProfile lopp = LearningOpportunityServices.Get( learningOppId, false );
				entity.ProfileName = "Auto-created for new Learning Opportunity";
				entity.Description = "Added a learning opportunity condition";

				//set the ParentId which is the EntityId
				entity.ParentId = parent.Id;
				//if sure this is always provided, don't need userId in methods
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				if ( new ConditionMgr().Save( entity, credentialUid, user.Id, ref messages ) )
				{
					status = "Successfully Saved Condition Profile";
						activityMgr.AddActivity( "Condition Profile", "Auto create for learning opportunity", string.Format( "{0} saved condition profile: {1} for {2}", user.FullName(), entity.ProfileName, parent.EntityType ), user.Id, 0, entity.Id );

					//now all lopp to the latter
					var newId = new ProfileServices().LearningOpportunity_Add( entity.RowId, learningOppId, user, ref isValid, ref status, true );

					string msg2 = "A condition profile has been automatically created for this new Learning Opportunity";
					string statusMsg = "";
					new MessageServices().Add( new Data.Message() { SenderId = 1, ReceiverId = user.Id, DeliveryMethod = 1, Content = msg2 }, ref statusMsg );
				}
				else
				{
					status += string.Join( ",", messages.ToArray() );
					return false;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".ConditionProfile_Save" );
				status = ex.Message;
				isValid = false;
			}

			return isValid;
		}

		public bool UpsertConditionProfileForAssessment( Guid credentialUid, int asmtId, AppUser user, ref string status )
		{
			bool isValid = true;


			List<String> messages = new List<string>();
			if ( asmtId == 0 || !BaseFactory.IsGuidValid( credentialUid ) )
			{
				status = "Error - missing an identifier for the Condition Profile Upsert";
				return false;
			}

			//check if credential has condition profiles (type 1 or 2)
			//if not create a new one and attach the learning opp
			//if not, hmmm, notify user to add to the lopp?
			//could send an email?
			//value of some system messages!!!

			try
			{
				Entity parent = EntityManager.GetEntity( credentialUid );
				Credential cred = CredentialManager.GetBasicWithConditions( credentialUid );
				if ( cred.Requires.Count > 0  )
				{
					//user has to handle - send message
					status = "Multiple condition profiles already exist. You must manually add this Assessment to the correct one.";
					string statusMsg = "";
					new MessageServices().Add( new Data.Message() { SenderId = 1, ReceiverId = user.Id, DeliveryMethod = 1, Content = status }, ref statusMsg );

					return false;
				}

				ConditionProfile entity = new ConditionProfile();
				entity.ConnectionProfileTypeId = 1;
				entity.ConditionSubTypeId = 1;
				entity.IsStarterProfile = true;
				entity.AssertedByAgentUid = cred.OwningAgentUid;

				AssessmentProfile profile = AssessmentServices.GetBasic( asmtId );
				entity.ProfileName = "Auto-created for new assessment";
				entity.Description = "Added an Assessment condition";

				//set the ParentId which is the EntityId
				entity.ParentId = parent.Id;
				//if sure this is always provided, don't need userId in methods
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				if ( new ConditionMgr().Save( entity, credentialUid, user.Id, ref messages ) )
				{
					status = "Successfully Saved Condition Profile";
					activityMgr.AddActivity( "Condition Profile", "Auto create for Assessment", string.Format( "{0} saved condition profile: {1} for {2}", user.FullName(), entity.ProfileName, parent.EntityType ), user.Id, 0, entity.Id );

					//now add asmt to the latter
					var newId = new ProfileServices().Assessment_Add( entity.RowId, asmtId, user, ref isValid, ref status, true );

					string msg2 = "A condition profile has been automatically created for this new Assessment";
					string statusMsg = "";
					new MessageServices().Add( new Data.Message() { SenderId = 1, ReceiverId = user.Id, DeliveryMethod = 1, Content = msg2 }, ref statusMsg );
				}
				else
				{
					status += string.Join( ",", messages.ToArray() );
					return false;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".ConditionProfile_Save" );
				status = ex.Message;
				isValid = false;
			}

			return isValid;
		}
		#endregion

		public static int GetConditionTypeId( string connectionProfileType )
		{
			int conditionTypeId = 1;
			switch ( connectionProfileType.ToLower() )
			{
				case "requires":
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_Requirement;
					break;
				case "alternativecondition": //NO - can have different types OR ??
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_Requirement;
					break;
				case "additionalcondition": //NO - can have different types OR ??
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_Requirement;
					break;
				case "recommends":
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_Recommendation;
					break;

				case "isrequiredfor":
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_NextIsRequiredFor;
					break;

				case "isrecommendedfor":
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_NextIsRecommendedFor;
					break;
				case "renewal":
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_Renewal;
					break;
				case "advancedstandingfor":
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_AdvancedStandingFor;
					break;
				case "advancedstandingfrom":
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_AdvancedStandingFrom;
					break;
				case "ispreparationfor":
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_PreparationFor;
					break;
				case "preparationfrom":
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_PreparationFrom;
					break;
				case "credentialconnections":
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_PreparationFrom;
					break;
				case "assessmentconnections":
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_PreparationFrom;
					break;
				case "learningoppconnections":
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_PreparationFrom;
					break;
				case "corequisite":
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_Corequisite;
					break;
				case "entrycondition":
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_EntryCondition;
					break;
				case "hasconditionmanifest":
					//?????????????
					conditionTypeId = Entity_ConditionProfileManager.ConnectionProfileType_Requirement;
					break;
				//CredentialConnections
				default:
					conditionTypeId = 0;
					break;
			}
			return conditionTypeId;
		}

		public static int SetConditionSubTypeId( string connectionProfileType )
		{
			int conditionSubTypeId = 1;
			if ( connectionProfileType == "CredentialConnections" )
			{
				conditionSubTypeId = 2;
			}
			else if ( connectionProfileType == "AssessmentConnections" )
			{
				conditionSubTypeId = 3;
			}
			else if ( connectionProfileType == "LearningOppConnections" )
			{
				conditionSubTypeId = 4;
			}
			else if ( connectionProfileType == "AlternativeCondition" )
			{
				conditionSubTypeId = 5;
			}
			else if ( connectionProfileType == "AdditionalCondition" )
			{
				conditionSubTypeId = 6;
			}
			else
				conditionSubTypeId = 1;

			return conditionSubTypeId;
		}


		#region Task Profile - OBSOLETE
		//public static TaskProfile TaskProfile_Get( int profileId )
		//{
		//	TaskProfile profile = Entity_TaskProfileManager.TaskProfile_Get( profileId );

		//	return profile;
		//}
		//public bool TaskProfile_Save( TaskProfile entity, Guid parentUid, string action, AppUser user, ref string status )
		//{
		//	bool isValid = true;
		//	List<String> messages = new List<string>();
		//	if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
		//	{
		//		messages.Add( "Error - missing an identifier for the Task Profile" );
		//		return false;
		//	}

		//	try
		//	{
		//		Entity parent = EntityManager.GetEntity( parentUid );
		//		if ( !OrganizationServices.CanUserEditEntity( user, parent.Id, ref status ) )
		//		{
		//			status = "Error - you do not have authorization for this action.";
		//			return false;
		//		}
		//		//remove this if properly passed from client
		//		//plus need to migrate to the use of EntityId
		//		entity.ParentId = parent.Id;
		//		entity.CreatedById = entity.LastUpdatedById = user.Id;

		//		if ( new Entity_TaskProfileManager().Update( entity, parentUid, user.Id, ref messages ) )
		//		{
		//			if ( action == "Initial" )
		//			{
		//				status = "Created an initial Task Profile. Please provide a meaningful name, and fill out the remainder of the profile";
		//				//test concept
		//				return true; //false;
		//			}
		//			else
		//			{
		//				//if valid, status contains the cred id, category, and codeId
		//				status = "Successfully Saved Task Profile";
		//				activityMgr.AddActivity( "Task Profile", action, string.Format( "{0} saved task profile: {1} for {2}", user.FullName(), entity.ProfileName, parent.EntityType ), user.Id, 0, entity.Id );
		//			}
		//		}
		//		else
		//		{
		//			status += string.Join( ",", messages.ToArray() );
		//			return false;
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".TaskProfile_Save" );
		//		status = ex.Message;
		//		isValid = false;
		//	}

		//	return isValid;
		//}

		//public bool TaskProfile_Delete( Guid parentUid, int profileId, AppUser user, ref string status )
		//{
		//	bool valid = true;

		//	Entity_TaskProfileManager mgr = new Entity_TaskProfileManager();
		//	try
		//	{
		//		Entity parent = EntityManager.GetEntity( parentUid );
		//		//EntitySummary parent = EntityManager.GetEntitySummary( parentUid );
		//		//validate access
		//		if ( !OrganizationServices.CanUserEditEntity( user, parent.Id, ref status ) )
		//		{
		//			status = "Error - you do not have authorization for this action.";
		//			return false;
		//		}

		//		valid = mgr.TaskProfile_Delete( profileId, ref status );

		//		if ( valid )
		//		{
		//			//if valid, status contains the cred id, category, and codeId
		//			activityMgr.AddActivity( "Task Profile", "Delete", string.Format( "{0} deleted Task Profile {1} from {2} -  {3}", user.FullName(), profileId, parent.EntityType, parent.EntityBaseId ), user.Id, 0, profileId );
		//			status = "";
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".TaskProfile_Delete" );
		//		status = ex.Message;
		//		valid = false;
		//	}

		//	return valid;
		//}

		#endregion

	}
}
