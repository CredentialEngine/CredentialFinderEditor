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

namespace CTIServices
{
	public class ConditionManifestServices
	{
		string thisClassName = "ConditionManifestServices";
		ActivityServices activityMgr = new ActivityServices();
		public List<string> messages = new List<string>();


		#region ConditionManifest Profile
		public static ConditionManifest GetForEdit( int profileId )
		{
			ConditionManifest profile = ConditionManifestManager.Get( profileId, true );

			return profile;
		}
		//public static ConditionManifest GetForDetail( int profileId )
		//{
		//	ConditionManifest profile = ConditionManifestManager.Get( profileId, false );

		//	return profile;
		//}
		public static ConditionManifest GetForDetail( int profileId, AppUser user )
		{
			ConditionManifest profile = ConditionManifestManager.Get( profileId, false );
			string status = "";
			if ( CanUserUpdateConditionManifest( profile, user, ref status ) )
				profile.CanUserEditEntity = true;
			return profile;
		}

		public static bool CanUserUpdateConditionManifest( ConditionManifest entity, AppUser user, ref string status )
		{
			if ( user == null || user.Id == 0 )
				return false;

			bool isValid = false;
			if ( entity.Id == 0 )
				return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;

			//is a member of the assessment managing organization 
			if ( OrganizationServices.IsOrganizationMember( user.Id, entity.OwningAgentUid ) )
				return true;

			status = "Error - you do not have edit access for this record.";
			return isValid;
		}

		public static ConditionManifest GetBasic( int profileId )
		{
			ConditionManifest profile = ConditionManifestManager.GetBasic( profileId );

			return profile;
		}
		public bool Save( ConditionManifest entity, Guid parentUid, string action, AppUser user, ref string status, bool isQuickCreate = false )
		{
			bool isValid = true;
			List<String> messages = new List<string>();
			//parent is the org - not sure if int or guid yet
			if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
			{
				messages.Add( "Error - missing an identifier for the ConditionManifest Profile" );
				return false;
			}

			try
			{
				Entity e = EntityManager.GetEntity( parentUid );
				//remove this if properly passed from client
				//plus need to migrate to the use of EntityId
				entity.ParentId = e.Id;
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				if ( new ConditionManifestManager().Save( entity, parentUid, user.Id, ref messages ) )
				{

					//if valid, status contains the cred id, category, and codeId
					status = "Successfully Saved Condition Manifest";
					activityMgr.AddActivity( "Condition Manifest", action, string.Format( "{0} added/updated Condition Manifest profile: {1}", user.FullName(), entity.ProfileName ), user.Id, 0, entity.Id );

				}
				else
				{
					status += string.Join( "<br/>", messages.ToArray() );
					return false;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".ConditionManifest_Save" );
				status = ex.Message;
				isValid = false;
			}

			return isValid;
		}


		public static List<ConditionManifest> Search( int orgId, int pageNumber, int pageSize, ref int pTotalRows )
		{
			List<ConditionManifest> list = ConditionManifestManager.Search( orgId, pageNumber, pageSize, ref pTotalRows );
			return list;
		}
		
		public bool Delete( Guid parentUid, int profileId, AppUser user, ref string status )
		{
			bool valid = true;

			ConditionManifestManager mgr = new ConditionManifestManager();
			try
			{
				//get first to validate (soon)
				//to do match to the conditionProfileId
				ConditionManifest profile = ConditionManifestManager.GetBasic( profileId );

				valid = mgr.Delete( profileId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "ConditionManifest Profile", "Delete Task", string.Format( "{0} deleted ConditionManifest Profile {1} from OrganizationId:  {2}", user.FullName(), profileId, profile.OrganizationId ), user.Id, 0, profileId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".ConditionManifest_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		#endregion

		#region Entity_CommonCondition
		/// <summary>
		/// Add a Entity_CommonCondition to a profile
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="conditionManifestId"></param>
		/// <param name="user"></param>
		/// <param name="valid"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public int Entity_CommonCondition_Add( Guid parentUid, int conditionManifestId, AppUser user, ref bool valid, ref string status, bool allowMultiples = true )
		{
			int id = 0;
			try
			{
				Entity parent = EntityManager.GetEntity( parentUid );
				if ( parent == null || parent.Id == 0 )
				{
					status = "Error - the parent entity was not found.";
					valid = false;
					return 0;
				}

				id = new Entity_CommonConditionManager().Add( parentUid, conditionManifestId, user.Id,  ref messages );

				if ( id > 0 )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "Entity_CommonCondition", "Add Entity_CommonCondition", string.Format( "{0} added Entity_CommonCondition {1} to {3} EntityId: {2}", user.FullName(), conditionManifestId, parent.Id, parent.EntityType ), user.Id, 0, conditionManifestId );
					status = "";

				}
				else
				{
					valid = false;
					status += string.Join( "<br/>", messages.ToArray() );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Entity_CommonCondition_Add" );
				status = ex.Message;
				valid = false;
			}

			return id;
		}

		public bool Entity_CommonCondition_Delete( Guid parentUid, int manifestId, AppUser user, ref string status )
		{
			bool valid = true;

			Entity_CommonConditionManager mgr = new Entity_CommonConditionManager();
			try
			{
				Entity parent = EntityManager.GetEntity( parentUid );
				if ( parent == null || parent.Id == 0 )
				{
					messages.Add( "Error - the parent entity was not found." );
					return false;
				}
				//get profile and ensure user has access
				Entity_CommonCondition profile = Entity_CommonConditionManager.Get( parent.Id, manifestId );
				if ( profile == null || profile.Id == 0 )
				{
					status = "Error - the requested profile was not found.";
					return false;
				}
				string cmName = profile.ConditionManifest.Name ?? "no name";
				valid = mgr.Delete( parentUid, manifestId, ref status );

				//if valid, and no message (assuming related to the targer not being found)
				if ( valid && status.Length == 0 )
				{
					//activity
					activityMgr.AddActivity( "Entity_CommonCondition", "Remove Entity_CommonCondition", string.Format( "{0} removed Entity_CommonCondition {1} ({2}) from Entity: {3} (4)", user.FullName(), cmName, manifestId, parent.EntityType, parent.EntityBaseId ), user.Id, 0, manifestId, parent.EntityBaseId );

					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Entity_CommonCondition_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}


		#endregion

	}
}
