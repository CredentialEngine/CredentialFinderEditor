using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

using CF = Factories;
using Mgr = Factories.OrganizationManager;


using Models;
using Models.Common;
using Models.ProfileModels;
using Models.Search;

using Utilities;
using CM = Models.Common;

namespace CTIServices
{
	public class OrganizationServices
	{
		static string thisClassName = "OrganizationServices";
		ActivityServices activityMgr = new ActivityServices();

		//
		#region Search 
		public static List<CM.Organization> Agent_Search( )
		{
			int totalRows = 0;
			return Agent_Search( "", 1, 500, ref totalRows );
		}
		/// <summary>
		/// Agent search - in progress
		/// Currently only returns minimum info for use in a list
		/// </summary>
		/// <param name="keyword"></param>
		/// <returns></returns>
		public static List<CM.Organization> Agent_Search( string keyword, int pageNumber, int pageSize, ref int pTotalRows )
		{
			int userId = 0;

			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;

			return Mgr.Agent_Search( userId, keyword, pageNumber, pageSize, ref pTotalRows );
		}
		public static List<CM.Organization> QuickSearch( string keyword = "" )
		{
			//TBD: only return items use has access to
			//or this may be a short lived method. For the search all may see, but only auth people may be able to edit an org
			int userId = 0;
			int pTotalRows = 0;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;

			return Mgr.QuickSearch( userId, keyword, ref pTotalRows );
		}


		/// <summary>
		/// Custom search for QA organizations only
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="startPage"></param>
		/// <param name="pageSize"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
		public static List<CM.Organization> QAOrgsSearch( string keywords, int startPage, int pageSize, ref int pTotalRows )
		{
			string where = "";
			string AND = "";
			int userId = AccountServices.GetCurrentUserId();

			if ( !string.IsNullOrWhiteSpace( keywords ) )
			{
				keywords = ServiceHelper.HandleApostrophes( keywords );
				if ( keywords.IndexOf( "%" ) == -1 )
					keywords = "%" + keywords.Trim() + "%";
				where = string.Format( " (base.name like '{0}' OR base.Description like '{0}'  OR base.Url like '{0}')", keywords );
				AND = " AND ";
			}
			where = where + AND + " ([IsAQAOrganization] = 1) ";
			return Mgr.Search( where, "", startPage, pageSize, ref pTotalRows, userId );
		}

		public static List<CM.Organization> Search( MainSearchInput data, ref int pTotalRows )
		{
			string pOrderBy = "";
			string where = "";
			int userId = 0;

			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;

			if ( !string.IsNullOrWhiteSpace( data.Keywords ) )
			{
				string keywords = ServiceHelper.HandleApostrophes( data.Keywords );
				if ( keywords.IndexOf( "%" ) == -1 )
					keywords = "%" + keywords.Trim() + "%";
				where = string.Format( " (base.name like '{0}' OR base.Description like '{0}'  OR base.Url like '{0}')", keywords );
			}

			SetAuthorizationFilter( user, ref where );

			SetOrgPropertiesFilter( data, ref where );

			SetBoundariesFilter( data, ref where );

			SetOrgServicesFilter( data, ref where );

			//check for org category (credentially, or QA). Only valid if one item
			SetOrgCategoryFilter( data, ref where );

			return Mgr.Search( where, pOrderBy, data.StartPage, data.PageSize, ref pTotalRows, userId );
		}
	

		private static void SetBoundariesFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";
			string template = " ( base.Id in ( SELECT  [OrgId] FROM [dbo].[Organization.Address] where [Longitude] < {0} and [Longitude] > {1} and [Latitude] < {2} and [Latitude] > {3} ) ) ";

			var boundaries = SearchServices.GetBoundaries( data, "bounds" );
			if ( boundaries.IsDefined )
			{
				where = where + AND + string.Format( template, boundaries.East, boundaries.West, boundaries.North, boundaries.South );
			}
		}
		/// <summary>
		/// determine which results a user may view, and eventually edit
		/// </summary>
		/// <param name="data"></param>
		/// <param name="user"></param>
		/// <param name="where"></param>
		private static void SetAuthorizationFilter( AppUser user, ref string where )
		{
			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";
			if ( user == null || user.Id == 0 )
			{
				//public only records
				where = where + AND + string.Format( " (base.StatusId = {0}) ", CF.CodesManager.ENTITY_STATUS_PUBLISHED );
				return;
			}

			if ( AccountServices.IsUserSiteStaff( user )
			  || AccountServices.CanUserViewAllContent( user ) )
			{
				//can view all, edit all
				return;
			}

			//can only view where status is published, or associated with 
			where = where + AND + string.Format( "((base.StatusId = {0}) OR (base.Id in (SELECT ParentOrgId FROM [dbo].[Organization.Member] where userId  = {1}) ))", CF.CodesManager.ENTITY_STATUS_PUBLISHED, user.Id );

		}
		private static void SetOrgPropertiesFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			//string template = " ( base.Id in ( SELECT  [OrganizationId] FROM [dbo].[OrganizationProperty.Summary]  where [PropertyValueId] in ({0}))) ";
			string template = " ( base.RowId in ( SELECT  [ParentUid] FROM [dbo].[Entity.Property] where [PropertyValueId] in ({0}))) ";
			//don't really need categoryId - yet
			foreach ( MainSearchFilter filter in data.Filters.Where( s => s.CategoryId >= 7 && s.CategoryId < 10 ) )
			{
				string next = "";
				if ( where.Length > 0 )
					AND = " AND ";
				foreach ( string item in filter.Items )
				{
					next += item + ",";
				}
				next = next.Trim( ',' );
				where = where + AND + string.Format( template, next );
			}
		}
		private static void SetOrgServicesFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			string template = " ( base.Id in ( SELECT  [OrganizationId] FROM [dbo].[Organization.ServiceSummary]  where [CodeId] in ({0}))) ";
			//don't really need categoryId - yet
			foreach ( MainSearchFilter filter in data.Filters.Where( s => s.CategoryId == 6 ) )
			{
				string next = "";
				if ( where.Length > 0 )
					AND = " AND ";
				foreach ( string item in filter.Items )
				{
					next += item + ",";
				}
				next = next.Trim( ',' );
				where = where + AND + string.Format( template, next );
			}
		}
		private static void SetOrgCategoryFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			//check for org category (credentially, or QA). Only valid if one item
			var qaSettings = data.GetFilterValues_Strings( "qualityAssurance" );
			if ( qaSettings.Count == 1 )
			{
				//ignore unless one filter
				string item = qaSettings[ 0 ];
				if ( where.Length > 0 )
					AND = " AND ";
				if ( item == "includeNormal" ) //IsAQAOrganization = false
					where = where + AND + " ([IsAQAOrganization] = 0 OR [CredentialCount] > 0) ";
				else if ( item == "includeQualityAssurance" )  //IsAQAOrganization = true
					where = where + AND + " ([IsAQAOrganization] = 1) ";
			}
		}
		public static List<CM.Organization> Search( string keywords, int pageNumber, int pageSize, ref int pTotalRows )
		{
			string pOrderBy = "";
			string filter = "";
			if ( !string.IsNullOrWhiteSpace( keywords ) )
			{
				keywords = ServiceHelper.HandleApostrophes( keywords );
				if ( keywords.IndexOf( "%" ) == -1 )
					keywords = "%" + keywords.Trim() + "%";
				filter = string.Format( " (base.name like '{0}' OR base.Description like '{0}'  OR base.Url like '{0}')", keywords );
			}
			return Mgr.Search( filter, pOrderBy, pageNumber, pageSize, ref pTotalRows );
		}
		public static List<CM.Organization> Organization_QAOrgs( string keyword = "" )
		{
			//TBD: only return items use has access to
			//or this may be a short lived method. For the search all may see, but only auth people may be able to edit an org
			int userId = 0;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;

			return Mgr.Organization_SelectQAOrgs( userId, keyword );
		}
		public static List<CM.Organization> Organization_Autocomplete( AppUser user, string keyword = "", int maxTerms = 25 )
		{
			//TBD: only return items use has access to
			//or this may be a short lived method. For the search all may see, but only auth people may be able to edit an org
			int userId = 0;
			if ( user != null && user.Id > 0 )
				userId = user.Id;

			return Mgr.Organization_ListByName( userId, keyword, maxTerms );
		}
		/// <summary>
		/// get all orgs as code item to display in a list
		/// Assuming for a drop down, so will only return those the user has access to. 
		/// Can we assume the caller has done the authentication verification
		/// </summary>
		/// <returns></returns>
		public static List<CM.Organization> OrganizationsForCredentials_Select(int userId)
		{
			return Mgr.Organization_SelectAll( userId );
		}

		public static CM.Organization GetLightOrgByRowId( string rowId )
		{
			if ( !Mgr.IsValidGuid( rowId ) )
				return null;

			string where = string.Format( " RowId = '{0}'", rowId );
			int pTotalRows = 0;
			//AppUser user = AccountServices.GetCurrentUser();

			List<CM.Organization> list = Mgr.Search( where, "", 1, 50, ref pTotalRows );
			if ( list.Count > 0 )
				return list[ 0 ];
			else
				return null;
		}
		public static CM.Organization GetLightOrgById( int orgId )
		{
			if ( orgId < 1 )
				return null;
			string where = string.Format( " base.Id = {0}", orgId );
			int pTotalRows = 0;

			List<CM.Organization> list = Mgr.Search( where, "", 1, 50, ref pTotalRows );

			if ( list.Count > 0 )
				return list[ 0 ];
			else
				return null;
		}
		#endregion 

		#region Retrievals
		public static CM.Organization GetOrganization( int id, bool newVersion = false )
		{
			return Mgr.Organization_Get(id, false, true, newVersion) ;
}
		public static CM.Organization GetOrganizationDetail( int id )
		{
			AppUser user = AccountServices.GetCurrentUser();
			return GetOrganizationDetail( id, user  );

		}
		public static CM.Organization GetOrganizationDetail( int id, AppUser user  )
		{
			CM.Organization entity = Mgr.Organization_GetDetail( id );
			if ( CanUserUpdateOrganization( user, entity.Id ) )
				entity.CanUserEditEntity = true;

			return entity;
		}
		/// <summary>
		/// Return true if user can edit the org:
		/// - either site admin
		/// - is an admin or employee member of the org
		/// - created the org (interim rule?)
		/// </summary>
		/// <param name="user"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public static bool CanUserUpdateOrganization( AppUser user,  int orgId )
		{
			if ( orgId == 0 )
				return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;

			if ( Mgr.IsOrganizationMember( user.Id, orgId ) )
				return true;
			else
				return false;
		}

		public static bool CanUserUpdateOrganization( AppUser user, CM.Organization entity )
		{
			if ( entity.Id == 0 )
				return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;

			if ( Mgr.IsOrganizationMember( user.Id, entity.Id ) )
				return true;

			else if ( entity.CreatedById == user.Id || entity.LastUpdatedById == user.Id )
				return true;
			else
				return false;
		}
		#endregion

		#region === add/update/delete =============
		/// <summary>
		/// Return true if user is a member of the provided organization
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public static bool IsOrganizationMember( int userId, int orgId )
		{

			if ( Mgr.IsOrganizationMember( userId, orgId ) )
				return true;
			else
				return false;
		}
		/// <summary>
		/// Add a Organization stack
		/// ??what to return - given the jumbotron form
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public int Organization_Add( CM.Organization entity, AppUser user, ref bool valid, ref string statusMessage )
		{
			LoggingHelper.DoTrace( 5, string.Format( "OrganizationServices.Organization_Add. Org: {0}, userId: {1}", entity.Name, entity.CreatedById ) );

			int id = 0;

			Mgr mgr = new Mgr();
			try
			{
				entity.CreatedById = user.Id;
				entity.LastUpdatedById = user.Id;

				id = mgr.Organization_Add( entity, ref statusMessage );
				if ( id > 0 )
				{
					ConsoleMessageHelper.SetConsoleInfoMessage( "Successfully Added Organization" );
					activityMgr.AddActivity( "Organization", "Add", string.Format( "{0} added a new organization: {1}", user.FullName(), entity.Name ), entity.CreatedById, 0, id );

					//notify administration
					string message = string.Format( "New Organization. <ul><li>OrganizationId: {0}</li><li>Organization: {1}</li><li>Created By: {2}</li></ul>", entity.Id, entity.Name, user.FullName() );

					Utilities.EmailManager.NotifyAdmin( "New Organization has been created", message );
				}
				else
				{
					valid = false;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "OrganizationServices.Organization_Add" );
			}
			return id;
		}

		/// <summary>
		/// Update an organization
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public bool Organization_Update( CM.Organization entity, AppUser user, ref string statusMessage )
		{
			LoggingHelper.DoTrace( 5, string.Format( "OrganizationServices.Organization_Update. OrgId: {0}, userId: {1}", entity.Id, entity.LastUpdatedById ) );
			Mgr mgr = new Mgr();
			bool isOK = false;
			try
			{
				isOK = mgr.Organization_Update( entity, ref statusMessage );
				if ( isOK )
				{
					ConsoleMessageHelper.SetConsoleInfoMessage( "Successfully Updated Organization" );
					activityMgr.AddActivity( "Organization", "Update", string.Format( "{0} updated organization (or parts of): {1}", user.FullName(), entity.Name ), user.Id, 0, entity.Id );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "OrganizationServices.Organization_Update" );
			}
			return isOK;
		}


		/// <summary>
		/// to do - add logging
		/// </summary>
		/// <param name="organizationID"></param>
		/// <param name="valid"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool Organization_Delete( int organizationID, AppUser user, ref string status )
		{
			bool valid = true;
			Mgr mgr = new Mgr();
			try
			{
				if ( CanUserUpdateOrganization( user, organizationID ) == false )
				{
					status = "ERROR - you do not have authorization to update this organization";
					return false;
				}
				else
				{
					valid = mgr.Organization_Delete( organizationID, ref status );
					if ( valid )
					{
						//if valid, status contains the cred name and id
						activityMgr.AddActivity( "Organization", "Delete", string.Format( "{0} deleted Organization {1}", user.FullName(), status ), user.Id, 0, organizationID );
						status = "";
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "OrganizationServices.Organization_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		//public bool DeleteProfile( int profileId, string profileName, ref bool valid, ref string status )
		//{
		//	List<string> messages = new List<string>();
		//	status = "";
		//	try
		//	{
		//		if ( profileName.ToLower() == "socialmediaprofile" )
		//			valid = new CF.OrganizationPropertyManager().Organization_PropertyOtherDelete( profileId, ref messages );
		//		else if ( profileName.ToLower() == "uniqueidentifier" )
		//			valid = new CF.OrganizationPropertyManager().Organization_PropertyDelete( profileId, ref messages );
		//		else
		//		{
		//			valid = false;
		//			status = "Deleting the requested clientProfile is not handled at this time.";
		//			return false;
		//		}

		//		if ( !valid )
		//			status += string.Join( ",", messages.ToArray() );

		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, "OrganizationServices.Organization_DeleteSection" );
		//		status = ex.Message;
		//		valid = false;
		//	}

		//	return valid;
		//}

		
		#endregion
		#region organization Member
		public int OrganizationMember_Save( int orgId, int userId, int orgMemberTypeId, int createdById, ref string statusMessage )
		{

			return new Mgr().OrganizationMember_Save( orgId, userId, orgMemberTypeId, createdById, ref statusMessage );
		}
		public bool OrganizationMember_Delete( int orgId, int userId, ref string statusMessage )
		{
			return new Mgr().OrganizationMember_Delete( orgId, userId, ref statusMessage );
		}

		#endregion 
		#region Entity Agent Relationships

		/// <summary>
		/// Add/Update an Agent - Entity relationship
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid">parentUid should already be in the entity, - so look to remove this</param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool EntityAgentRole_Save( OrganizationRoleProfile entity, 
				Guid parentUid, 
				AppUser user, 
				ref string status ) 
		{
			bool isValid = true;
			//parentUid should already be in the entity,
 			//or set now
			entity.ParentUid = parentUid;

			List<string> messages = new List<string>();
			isValid = new CF.Entity_AgentRelationshipManager().Agent_EntityRoles_Save( entity,
				user.Id, ref messages );

			if ( !isValid )
				status += string.Join( ",", messages.ToArray() );
			return isValid;
}
		public int AddChildOrganization( Guid parentUid, Guid orgChildUid, int roleTypeId, AppUser user, ref bool isValid, ref string status )
		{
			//LoggingHelper.DoTrace( 5, string.Format( "OrganizationServices.Organization_Update. OrgId: {0}, userId: {1}", entity.Id, entity.LastUpdatedById ) );
			Mgr mgr = new Mgr();
			bool isOK = false;

			OrganizationRoleProfile entity = new OrganizationRoleProfile();
			entity.ParentUid = parentUid;
			entity.ActingAgentUid = orgChildUid;
			entity.RoleTypeId = roleTypeId;

			
			try
			{
				List<string> messages = new List<string>();
				isValid = new CF.Entity_AgentRelationshipManager().Entity_AgentRole_SaveSingleRole( entity,
					user.Id, ref messages );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "OrganizationServices.AddChildOrganization" );
			}
			return entity.Id;

		}
		/// <summary>
		/// Delete all roles for the provided parentUid (parent RowId - not entityUid) and agent combination.
		/// Note: this should be inverse relationships, but we don't have direct at this time
		/// </summary>
		/// <param name="entityId"></param>
		/// <param name="agentUid"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool Delete_EntityAgentRoles( Guid parentUid, Guid agentUid, AppUser user, ref string status )
		{
			bool valid = true;
			//check if user can edit the entity
			EntitySummary entity = CF.EntityManager.GetEntitySummary( parentUid );
			if ( CanUserEditEntity( user, entity.Id, ref status ) == false )
			{
				status = "Error - you don't have access to perform this action.";
				return false;
			}

			try
			{

				if ( new CF.Entity_AgentRelationshipManager().Delete_EntityAgentRoles( entity.Id, agentUid, ref status ) )
				{
					//if valid, log
					//activityMgr.AddActivity( "Cost Profile Item", "Delete", string.Format( "{0} deleted Cost Profile Item {1} ({2}) from Credential: {3}", user.FullName(), profile.ProfileName, profileId, profile.ParentId ), user.Id, 0, profileId );
					status = "";
				}
				else
				{
					status = "Error - delete failed: " + status;
					return false;
				}


			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Delete_EntityAgentRoles" );
				status = ex.Message;
				valid = false;
			}

			return valid;

		}
		public static OrganizationRoleProfile GetEntityAgentRoles_AsEnumeration( Guid parentUid, Guid agentUid )
		{
			return CF.Entity_AgentRelationshipManager.AgentEntityRole_GetAsEnumerationFromCSV( parentUid, agentUid );
		}


		#endregion

		#region Quality Assurance Actions

		public static QualityAssuranceActionProfile QualityAssuranceAction_GetProfile( Guid parentUid, int profileId )
		{
			//ensure has access to parent?
			QualityAssuranceActionProfile entity = CF.Entity_QualityAssuranceActionManager.QualityAssuranceActionProfile_Get( profileId );
			return entity;
		
		}
		public bool QualityAssuranceAction_SaveProfile( QualityAssuranceActionProfile entity, Guid parentUid, AppUser user, ref string status )
		{
			bool valid = true;
			status = "";
			List<string> messages = new List<string>();
			if ( !ServiceHelper.IsValidGuid( parentUid ) )
			{
				messages.Add( "Error - you must select a valid entity before performing this action." );
			}

			//check if user can edit the entity
			EntitySummary parent = CF.EntityManager.GetEntitySummary( parentUid );
			if ( parent.Id == 0 )
			{
				messages.Add( "Error - Invalid context, please start by editing a valid entity." );
				return false;
			}

			if ( CanUserEditEntity( user, parent.Id, ref status ) == false )
			{
				status = "Error - you don't have access to update this credential.";
				return false;
			}

			if ( !ServiceHelper.IsValidGuid(entity.ActingAgentUid)  )
			{
				messages.Add( "Error - you must select an organization/agent." );

			}
			if ( entity.IssuedCredentialId == 0 )
			{
				messages.Add( "Error - you must select an Issued Credential." );

			}
			if ( entity.RoleTypeId == 0 && ( entity.AgentRole == null || entity.AgentRole.Items.Count == 0 ) )
			{
				messages.Add( "Error - you must select an Action ." );
			}
			if ( messages.Count > 0 )
			{
				status += string.Join( ",", messages.ToArray() );
				return false;
			}
		
			try
			{
				entity.ParentId = parent.Id;
				entity.ParentUid = parentUid;
				if ( new CF.Entity_QualityAssuranceActionManager().QualityAssuranceAction_SaveProfile( entity, user.Id, ref messages ) == false )
				{
					valid = false;
					status += string.Join( ",", messages.ToArray() );
				}
				else
				{
					status = "Successfully saved Quality Assurance Action ";
					activityMgr.AddActivity( "Quality Assurance Action", "Save " + parent.EntityType, string.Format( "{0} saved Quality Assurance Action for entity: {1}, ID: {2}", user.FullName(), parent.EntityType, parent.BaseId ), user.Id, parent.BaseId, parent.Id );
				}

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".QualityAssuranceAction_SaveProfile" );
				valid = false;
				status = ex.Message;
			}
			return valid;
		}

		/// <summary>
		/// Delete a QualityAssurance record
		/// </summary>
		/// <param name="entityId"></param>
		/// <param name="profileId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool QualityAssuranceAction_DeleteProfile( int profileId, Guid parentUid, AppUser user, ref string status )
		{
			bool valid = true;
			//check if user can edit the entity
			EntitySummary parent = CF.EntityManager.GetEntitySummary( parentUid );
			if ( CanUserEditEntity( user, parent.Id, ref status ) == false )
			{
				status = "Error - you don't have access to perform this action.";
				return false;
			}

			try
			{
				if ( new CF.Entity_QualityAssuranceActionManager().QualityAssuranceAction_Delete( profileId, ref status ) )
				{
					//if valid, log
					//activityMgr.AddActivity( "Cost Profile Item", "Delete", string.Format( "{0} deleted Cost Profile Item {1} ({2}) from Credential: {3}", user.FullName(), profile.ProfileName, profileId, profile.ParentId ), user.Id, 0, profileId );
					status = "Successfully deleted Quality Assurance Action ";
					activityMgr.AddActivity( "Quality Assurance Action", "Delete for " + parent.EntityType, string.Format( "{0} deleted Quality Assurance Action for entity: {1}, ID: {2}", user.FullName(), parent.EntityType, parent.BaseId ), user.Id, parent.BaseId, parent.Id );
					status = "";
				}
				else
				{
					status = "Error - delete failed: " + status;
					return false;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".QualityAssuranceAction_DeleteProfile" );
				status = ex.Message;
				valid = false;
			}

			return valid;

		}
		#endregion

		/// <summary>
		/// Check if user has access to a parent entity
		/// NOTE: should only be used for a top level parent - until multiple levels are handled
		/// </summary>
		/// <param name="user"></param>
		/// <param name="entityId"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static bool CanUserEditEntity( AppUser user, int entityId, ref string status )
		{
			if ( entityId == 0 )
				return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;

			EntitySummary item = GetTopLevelEntity( entityId );
			if ( !item.IsTopLevelEntity )
			{
				//log and allow
				LoggingHelper.LogIssue( thisClassName + ".CanUserEditEntity(): Looping thru entities didn't find a top level entity", true );
				return true;
			}

			if ( Mgr.IsOrganizationMember( user.Id, item.ManagingOrgId ) )
				return true;
			else
				return false;
		}

		private static EntitySummary GetTopLevelEntity( int entityId )
		{
			EntitySummary item = new EntitySummary();
			int cntr = 0;
			do
			{
				cntr++;
				item = CF.EntityManager.GetEntitySummary( entityId );
				entityId = item.parentEntityId;
				LoggingHelper.DoTrace( 6, string.Format( "GetTopLevelEntity: entityId:{0}, nextParent: {1}", entityId, item.parentEntityId ) );

			} while (	item.IsTopLevelEntity == false 
					&&	item.parentEntityId > 0);
			

			return item;
		}
	}
}
