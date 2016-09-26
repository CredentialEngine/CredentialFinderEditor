using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Factories;
using Models;
using Models.Common;
using Models.ProfileModels;
using Utilities;
using ConditionMgr = Factories.Entity_ConditionProfileManager;
using CredConditionManager = Factories.ConnectionProfileManager;

namespace CTIServices
{
	public class ConditionProfileServices
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
			bool includeProperties = true;
			bool incudingResources = true;
			//if ( forEditView == false )
			//{
			//	includeProperties = false;
			//	incudingResources = false;
			//}
			ConditionProfile profile = ConditionMgr.Get( profileId, includeProperties, incudingResources, true );

			return profile;
		}
		public static ConditionProfile ConditionProfile_Get( int profileId )
		{
			bool includeProperties = true;
			bool incudingResources = false;
			bool forEditView = false;
			ConditionProfile profile = ConditionMgr.Get( profileId, includeProperties, incudingResources, forEditView );

			return profile;
		}
		public bool ConditionProfile_Save( ConditionProfile entity, Guid parentUid, string action, AppUser user, ref string status )
		{
			bool isValid = true;
			List<String> messages = new List<string>();
			if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
			{
				messages.Add( "Error - missing an identifier for the Condition Profile" );
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
				EntitySummary parent = EntityManager.GetEntitySummary( parentUid );
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
					activityMgr.AddActivity( "Condition Profile", "Delete", string.Format( "{0} deleted Condition Profile {1} from {2} -  {3}", user.FullName(), profileId, parent.EntityType, parent.BaseId ), user.Id, 0, profileId );
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

		#endregion
		#region Task Profile
		public static TaskProfile TaskProfile_Get( int profileId )
		{
			TaskProfile profile = Entity_TaskProfileManager.TaskProfile_Get( profileId );

			return profile;
		}
		public bool TaskProfile_Save( TaskProfile entity, Guid parentUid, string action, AppUser user, ref string status )
		{
			bool isValid = true;
			List<String> messages = new List<string>();
			if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
			{
				messages.Add( "Error - missing an identifier for the Task Profile" );
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
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				if ( new Entity_TaskProfileManager().Update( entity, parentUid, user.Id, ref messages ) )
				{
					if ( action == "Initial" )
					{
						status = "Created an initial Task Profile. Please provide a meaningful name, and fill out the remainder of the profile";
						//test concept
						return true; //false;
					}
					else
					{
						//if valid, status contains the cred id, category, and codeId
						status = "Successfully Saved Task Profile";
						activityMgr.AddActivity( "Task Profile", action, string.Format( "{0} saved task profile: {1} for {2}", user.FullName(), entity.ProfileName, parent.EntityType ), user.Id, 0, entity.Id );
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
				LoggingHelper.LogError( ex, thisClassName + ".TaskProfile_Save" );
				status = ex.Message;
				isValid = false;
			}

			return isValid;
		}

		public bool TaskProfile_Delete( Guid parentUid, int profileId, AppUser user, ref string status )
		{
			bool valid = true;

			Entity_TaskProfileManager mgr = new Entity_TaskProfileManager();
			try
			{
				EntitySummary parent = EntityManager.GetEntitySummary( parentUid );
				//validate access
				if ( !OrganizationServices.CanUserEditEntity( user, parent.Id, ref status ) )
				{
					status = "Error - you do not have authorization for this action.";
					return false;
				}

				valid = mgr.TaskProfile_Delete( profileId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "Task Profile", "Delete", string.Format( "{0} deleted Task Profile {1} from {2} -  {3}", user.FullName(), profileId, parent.EntityType, parent.BaseId ), user.Id, 0, profileId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".TaskProfile_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		#endregion

		#region Assessments and lopps
		public int Assessment_Add( int conditionProfileId, int assessmentId, AppUser user, ref bool valid, ref string status )
		{
			int id = 0;

			try
			{

				id = new Entity_AssessmentManager().EntityAssessment_Add( conditionProfileId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, assessmentId, user.Id, ref messages );

				if ( id > 0 )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "Condition Profile", "Add Assessment", string.Format( "{0} added Assessment {1} to Condition Profile  {2}", user.FullName(), assessmentId, conditionProfileId ), user.Id, 0, assessmentId );
					status = "";
				}
				else
				{
					valid = false;
					status += string.Join( ",", messages.ToArray() );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Assessment_Add" );
				status = ex.Message;
				valid = false;
			}

			return id;
		}

		public bool Assessment_Delete( int conditionProfileId, int assessmentId, AppUser user, ref string status )
		{
			bool valid = true;

			Entity_AssessmentManager mgr = new Entity_AssessmentManager();
			try
			{
				valid = mgr.EntityAssessment_Delete( conditionProfileId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, assessmentId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "Condition Profile", "Delete Assessment", string.Format( "{0} deleted Assessment {1} from Condition Profile  {2}", user.FullName(), assessmentId, conditionProfileId ), user.Id, 0, assessmentId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Assessment_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}


		public int LearningOpportunity_Add( int conditionProfileId, int recordId, AppUser user, ref bool valid, ref string status )
		{
			int id = 0;
			Entity_LearningOpportunityManager mgr = new Entity_LearningOpportunityManager();
			try
			{

				id = mgr.EntityLearningOpp_Add( conditionProfileId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, recordId, user.Id, ref messages );

				if ( id > 0 )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "Condition Profile", "Add Learning Opportunity", string.Format( "{0} added Learning Opportunity {1} to Condition Profile  {2}", user.FullName(), recordId, conditionProfileId ), user.Id, 0, recordId );
					status = "";
				}
				else
				{
					valid = false;
					status += string.Join( ",", messages.ToArray() );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".LearningOpportunity_Add" );
				status = ex.Message;
				valid = false;
			}

			return id;
		}

		public bool LearningOpportunity_Delete( int conditionProfileId, int recordId, AppUser user, ref string status )
		{
			bool valid = true;

			Entity_LearningOpportunityManager mgr = new Entity_LearningOpportunityManager();
			try
			{
				valid = mgr.EntityLearningOpp_Delete( conditionProfileId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, recordId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "Condition Profile", "Delete Learning Opportunity", string.Format( "{0} deleted Learning Opportunity {1} from Condition Profile  {2}", user.FullName(), recordId, conditionProfileId ), user.Id, 0, recordId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".ConditionProfile_DeleteLearningOpportunity" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		#endregion
	}
}
