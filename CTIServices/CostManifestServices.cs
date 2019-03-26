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
	public class CostManifestServices
	{
		string thisClassName = "CostManifestServices";
		ActivityServices activityMgr = new ActivityServices();
		public List<string> messages = new List<string>();


		#region CostManifest Profile
		public static CostManifest GetForEdit( int profileId )
		{
			CostManifest profile = CostManifestManager.Get( profileId, true );

			return profile;
		}
		public static CostManifest GetForDetail( int profileId, AppUser user )
		{
			CostManifest profile = CostManifestManager.Get( profileId, false );
			string status = "";
			if ( CanUserUpdateCostManifest( profile, user, ref status ) )
				profile.CanUserEditEntity = true;
			return profile;
		}
		public static string GetForFormat( int profileId, AppUser user, ref bool isValid, ref List<string> messages, ref bool isApproved, ref DateTime? lastPublishDate, ref DateTime lastUpdatedDate, ref DateTime lastApprovedDate, ref string ctid )
		{
			CostManifest entity = CostManifestManager.Get( profileId, false );
            if (entity == null || entity.Id == 0)
            {
                isValid = false;
                messages.Add( "Error - the requested Cost Manifest was not found." );
                return "";
            }
            isApproved = entity.IsEntityApproved();
			ctid = entity.CTID;
			string payload = RegistryAssistantServices.CostManifestMapper.FormatPayload( entity, ref isValid, ref messages );
			lastUpdatedDate = entity.EntityLastUpdated;
			lastApprovedDate = entity.EntityApproval.Created;
            lastPublishDate = ActivityManager.GetLastPublishDateTime( "costmanifest", profileId );
            return payload;
		}
		public static bool CanUserUpdateCostManifest( CostManifest entity, AppUser user, ref string status )
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
		public static CostManifest GetBasic( int profileId )
		{
			CostManifest profile = CostManifestManager.GetBasic( profileId );

			return profile;
		}
		public static CostManifest GetBasic( Guid rowID )
		{
			var profile = CostManifestManager.GetBasic( rowID );
			return profile;
		}
		public bool Save( CostManifest entity, Guid parentUid, string action, AppUser user, ref string status, bool isQuickCreate = false )
		{
			bool isValid = true;
			List<String> messages = new List<string>();
			//parent is the org - not sure if int or guid yet
			if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
			{
				messages.Add( "Error - missing an identifier for the CostManifest Profile" );
				return false;
			}

			try
			{
                //this is the org, based on current editor interface
				Entity e = EntityManager.GetEntity( parentUid );
				//remove this if properly passed from client
				//plus need to migrate to the use of EntityId
				entity.ParentId = e.Id;
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				if ( new CostManifestManager().Save( entity, parentUid, user.Id, ref messages ) )
				{

					//if valid, status contains the cred id, category, and codeId
					status = "";
                    //technically the org is not the top parent for activity
					activityMgr.AddEditorActivity( "Cost Manifest", action, string.Format( "{0} added/updated Cost Manifest profile: {1} for organization: {2}", user.FullName(), entity.ProfileName, e.EntityBaseId ), user.Id, entity.Id, entity.RowId );
                    //we do want to update the last updated for the org entity to reflect need to republish.
                    new ProfileServices().UpdateTopLevelEntityLastUpdateDate( e.Id, string.Format( "Entity Update triggered by {0} adding/updating Cost Manifest profile : {1}, for Organization: {2}", user.FullName(), entity.ProfileName, e.EntityBaseId ) );
                }
				else
				{
					status += string.Join( "<br/>", messages.ToArray() );
					return false;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Save" );
				status = ex.Message;
				isValid = false;
			}

			return isValid;
		}

        public static List<CostManifest> GetAllForOwningOrganization( string orgUid, ref int pTotalRows )
        {
            List<CostManifest> list = new List<CostManifest>();
            if ( string.IsNullOrWhiteSpace( orgUid ) )
                return list;
            string keywords = "";
            int pageNumber = 1;
            int pageSize = 0;
            string pOrderBy = "";
            string AND = "";
            int userId = AccountServices.GetCurrentUserId();
            string filter = string.Format( " (  org.RowId = '{0}' ) ", orgUid );


            return CostManifestManager.MainSearch( filter, pOrderBy, pageNumber, pageSize, userId, ref pTotalRows );

        }
        public static List<CostManifest> Search( int orgId, int pageNumber, int pageSize, ref int pTotalRows )
		{
			List<CostManifest> list = CostManifestManager.Search( orgId, pageNumber, pageSize, ref pTotalRows );
			return list;
		}

		public bool Delete( Guid parentUid, int profileId, AppUser user, ref string status )
		{
			bool valid = true;
            List<SiteActivity> list = new List<SiteActivity>();
            CostManifestManager mgr = new CostManifestManager();
			try
			{
				//get first to validate (soon)
				//to do match to the CostProfileId
				CostManifest profile = CostManifestManager.GetBasic( profileId );
                if (!string.IsNullOrWhiteSpace(profile.CredentialRegistryId))
                {
                    //should be deleting from registry!
                    //will be change to handling with managed keys
                    new RegistryServices().Unregister_CostManifest(profileId, user, ref status, ref list);
                }
                valid = mgr.Delete( profileId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddEditorActivity( "Cost Manifest", "Delete", string.Format( "{0} deleted CostManifest Profile {1} from OrganizationId:  {2}", user.FullName(), profileId, profile.OrganizationId ), user.Id, 0, profileId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".CostManifest_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		#endregion

		#region Entity_CommonCost
		/// <summary>
		/// Add a Entity_CommonCost to a profile
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="CostManifestId"></param>
		/// <param name="user"></param>
		/// <param name="valid"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public int Entity_CommonCost_Add( Guid parentUid, int CostManifestId, AppUser user, ref bool valid, ref string status, bool allowMultiples = true )
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
                bool skipDuplicatesError = false;
				id = new Entity_CommonCostManager().Add( parentUid, CostManifestId, user.Id, ref messages, skipDuplicatesError );

				if ( id > 0 )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddEditorActivity( "Entity_CommonCost", "Add Entity_CommonCost", string.Format( "{0} added Entity_CommonCost {1} to {3} EntityId: {2}", user.FullName(), CostManifestId, parent.Id, parent.EntityType ), user.Id, CostManifestId, parentUid );
					status = "";

                    new ProfileServices().UpdateTopLevelEntityLastUpdateDate(parent.Id, string.Format("Entity Update triggered by {0} adding a common cost to : {1}, BaseId: {2}", user.FullName(), parent.EntityType, parent.EntityBaseId));
                }
				else
				{
					valid = false;
					status += string.Join( "<br/>", messages.ToArray() );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Entity_CommonCost_Add" );
				status = ex.Message;
				valid = false;
			}

			return id;
		}

		public bool Entity_CommonCost_Delete( Guid parentUid, int manifestId, AppUser user, ref string status )
		{
			bool valid = true;

			Entity_CommonCostManager mgr = new Entity_CommonCostManager();
			try
			{
				Entity parent = EntityManager.GetEntity( parentUid );
				if ( parent == null || parent.Id == 0 )
				{
					messages.Add( "Error - the parent entity was not found." );
					return false;
				}
				//get profile and ensure user has access
				Entity_CommonCost profile = Entity_CommonCostManager.Get( parent.Id, manifestId );
				if ( profile == null || profile.Id == 0 )
				{
					status = "Error - the requested profile was not found.";
					return false;
				}
				string cmName = profile.ProfileSummary ?? "no name";
				valid = mgr.Delete( parentUid, manifestId, ref messages );

				//if valid, and no message (assuming related to the targer not being found)
				if ( valid && status.Length == 0 )
				{
					//activity
					activityMgr.AddEditorActivity( "Entity_CommonCost", "Remove Entity_CommonCost", string.Format( "{0} removed Entity_CommonCost {1} ({2}) from Entity: {3} (4)", user.FullName(), cmName, manifestId, parent.EntityType, parent.EntityBaseId ), user.Id, 0, manifestId, parent.EntityBaseId );

					status = "";
                    new ProfileServices().UpdateTopLevelEntityLastUpdateDate(parent.Id, string.Format("Entity Update triggered by {0} deleting a common cost from : {1}, BaseId: {2}", user.FullName(), parent.EntityType, parent.EntityBaseId));
                }
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Entity_CommonCost_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}


		#endregion

	}
}
