using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web;

using CF = Factories;
using Mgr = Factories.OrganizationManager;

using EM = Data;
using Models;
using Models.Common;
using MN = Models.Node;
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
		//public static List<CM.Organization> Agent_Search( )
		//{
		//	int totalRows = 0;
		//	return Agent_Search( "", 1, 500, ref totalRows );
		//}
		/// <summary>
		/// Agent search - in progress
		/// Currently only returns minimum info for use in a list
		/// </summary>
		/// <param name="keyword"></param>
		/// <returns></returns>
		//public static List<CM.Organization> Agent_Search( string keyword, int pageNumber, int pageSize, ref int pTotalRows )
		//{
		//	int userId = 0;

		//	AppUser user = AccountServices.GetCurrentUser();
		//	if ( user != null && user.Id > 0 )
		//		userId = user.Id;

		//	return Mgr.Agent_Search( userId, keyword, pageNumber, pageSize, ref pTotalRows );
		//}
		//public static List<CM.Organization> QuickSearch( string keyword = "" )
		//{
		//	//TBD: only return items use has access to
		//	//or this may be a short lived method. For the search all may see, but only auth people may be able to edit an org
		//	int userId = 0;
		//	int pTotalRows = 0;
		//	AppUser user = AccountServices.GetCurrentUser();
		//	if ( user != null && user.Id > 0 )
		//		userId = user.Id;

		//	return Mgr.QuickSearch( userId, keyword, ref pTotalRows );
		//}


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

		public static List<string> Autocomplete( string keyword = "", int maxTerms = 25 )
		{
			int userId = 0;
			string where = "";
			int totalRows = 0;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;
			SetAuthorizationFilter( user, ref where );

			//SetKeywordFilter( keyword, true, ref where );
			string keywords = ServiceHelper.HandleApostrophes( keyword );
			if ( keywords.IndexOf( "%" ) == -1 )
				keywords = "%" + keywords.Trim() + "%";
			where = string.Format( " (base.name like '{0}') ", keywords );
			return Mgr.Autocomplete( where, 1, maxTerms, userId, ref totalRows );
		}
		public static List<CM.Organization> OrgAutocomplete( string keyword = "", int maxTerms = 25 )
		{
			int userId = 0;
			string where = "";
			int totalRows = 0;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;
			SetAuthorizationFilter( user, ref where );

			//SetKeywordFilter( keyword, true, ref where );
			string keywords = ServiceHelper.HandleApostrophes( keyword );
			if ( keywords.IndexOf( "%" ) == -1 )
				keywords = "%" + keywords.Trim() + "%";
			where = string.Format( " (base.name like '{0}') ", keywords );

			List<CM.Organization> list = Mgr.Search( where, "", 1, maxTerms, ref totalRows, userId, false, true );

			return list;
		}
		/// <summary>
		/// Main search
		/// </summary>
		/// <param name="data"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
		public static List<CM.OrganizationSummary> Search( MainSearchInput data, ref int pTotalRows )
		{
			string where = "";
			int userId = 0;

			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;

			SetKeywordFilter( data.Keywords, true, ref where );

			SetAuthorizationFilter( user, ref where );
			SearchServices.HandleCustomFilters( data, 59, ref where );

			SetOrgPropertiesFilter( data, ref where );
			SearchServices.SetRolesFilter( data, ref where );
			//SearchServices.SetOrgRolesFilter( data, ref where );
			SetBoundariesFilter( data, ref where );

			SetFrameworksFilter( data, ref where );

			SetOrgServicesFilter( data, ref where );

			//check for org category (credentially, or QA). Only valid if one item
			SetOrgCategoryFilter( data, ref where ); //Not updated - I'm not sure we're still using this. - NA 5/12/2017

			LoggingHelper.DoTrace( 5, thisClassName + ".Search(). Filter: " + where );
			return Mgr.MainSearch( where, data.SortOrder, data.StartPage, data.PageSize, ref pTotalRows, userId );
		}

		public static List<CM.Organization> Search( string keywords, int pageNumber, int pageSize, ref int pTotalRows )
		{
			string pOrderBy = "";
			string filter = "";
			int userId = 0;

			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;

			SetKeywordFilter( keywords, true, ref filter );

			return Mgr.Search( filter, pOrderBy, pageNumber, pageSize, ref pTotalRows, userId );
		}


		public static List<CM.Organization> Search( MicroSearchInputV2 query, int pageNumber, int pageSize, ref int pTotalRows )
		{
			string pOrderBy = "";
			string filter = "";
			int userId = 0;
			string keywords = query.GetFilterValueString( "Keywords" );
			string orgMbrs = query.GetFilterValueString( "OrgFilters" );

			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;
			if ( orgMbrs == "myOrgs" )
				SetMyOrgsFilter( user, ref filter );

			SetKeywordFilter( keywords, true, ref filter );

			return Mgr.Search( filter, pOrderBy, pageNumber, pageSize, ref pTotalRows, userId );
		}
		
		private static void SetBoundariesFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";
			string template = " ( base.Id in ( SELECT  [EntityBaseId] FROM [dbo].[Entity_AddressSummary] where EntityTypeId = 2 AND  [Longitude] < {0} and [Longitude] > {1} and [Latitude] < {2} and [Latitude] > {3} ) ) ";

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
		private static void SetFrameworksFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			string codeTemplate = "  (base.Id in (SELECT c.id FROM [dbo].[Entity.FrameworkItemSummary] a inner join Entity b on a.EntityId = b.Id inner join Organization c on b.EntityUid = c.RowId where [CategoryId] = {0} and ([CodeGroup] in ({1})  OR ([CodeId] in ({2}) )  )) ) ";
			

			//Updated to use FiltersV2
			string next = "";
			string groups = "";
			if ( where.Length > 0 )
				AND = " AND ";
			var categoryID = 0;
			foreach ( var filter in data.FiltersV2.Where( m => m.Type == MainSearchFilterV2Types.FRAMEWORK ) )
			{
				var item = filter.AsCodeItem();
				var isTopLevel = filter.GetValueOrDefault<bool>( "IsTopLevel", false );
				if ( item.CategoryId == 10 || item.CategoryId == 11 )
				{
					categoryID = item.CategoryId;
					if ( isTopLevel )
						groups += item.Id + ",";
					else
						next += item.Id + ",";
				}
			}
			
			if ( next.Length > 0 )
				next = next.Trim( ',' );
			else
				next = "''";
			if ( groups.Length > 0 )
				groups = groups.Trim( ',' );
			else
				groups = "''";
			if ( groups != "''" || next != "''" )
			{
				where = where + AND + string.Format( codeTemplate, categoryID, groups, next );
			}

			/* //Retained for reference
			foreach ( MainSearchFilter filter in data.Filters.Where( s => s.CategoryId == 10 || s.CategoryId == 11 ) )
			{
				string next = "";
				if ( where.Length > 0 )
					AND = " AND ";
				foreach ( string item in filter.Items )
				{
					next += item + ",";
				}
				next = next.Trim( ',' );
				where = where + AND + string.Format( codeTemplate, filter.CategoryId, next );
			}
			*/
		}
		private static void SetKeywordFilter( string keywords, bool isBasic, ref string where )
		{
			if ( string.IsNullOrWhiteSpace( keywords ) )
				return;
			string text = " (base.name like '{0}' OR base.Description like '{0}'  OR base.Url like '{0}') ";
			string orgDepts = "( base.Id in (SELECT o.Id FROM dbo.Entity e INNER JOIN dbo.[Entity.AgentRelationship] ear ON e.Id = ear.EntityId INNER JOIN dbo.Organization o ON e.EntityUid = o.RowId WHERE ear.RelationshipTypeId = {0} AND o.StatusId < 4) )";
			bool isCustomSearch = false;
			//for ctid, needs a valid ctid or guid
			if ( keywords.IndexOf( "ce-" ) > -1 && keywords.Length == 45 )
			{
				text = " ( CTID = '{0}' ) ";
				isCustomSearch = true;
			}
			else if ( ServiceHelper.IsValidGuid( keywords ) )
			{
				text = " ( CTID = 'ce-{0}' ) ";
				isCustomSearch = true;
			}
			else if ( keywords.ToLower() == "[hascredentialregistryid]" )
			{
				text = " ( len(Isnull(CredentialRegistryId,'') ) = 36 ) ";
				isCustomSearch = true;
			}
			else if ( keywords.ToLower() == "has subsidiary" )
			{
				text = string.Format( orgDepts, 21);
				isCustomSearch = true;
			}
			else if ( keywords.ToLower() == "has department" )
			{
				text = string.Format( orgDepts, 20 );
				isCustomSearch = true;
			}
			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";

			keywords = ServiceHelper.HandleApostrophes( keywords );
			if ( keywords.IndexOf( "%" ) == -1 && !isCustomSearch )
			{
				keywords = SearchServices.SearchifyWord( keywords );
				//keywords = "%" + keywords.Trim() + "%";
				//keywords = keywords.Replace( "&", "%" ).Replace( " and ", "%" ).Replace( " in ", "%" ).Replace( " of ", "%" );
				//keywords = keywords.Replace( " - ", "%" );
				//keywords = keywords.Replace( " % ", "%" );
			}

			//same for now, but will chg
			if ( isBasic || isCustomSearch )
				where = where + AND + string.Format( " ( " + text + " ) ", keywords );
			else
				where = where + AND + string.Format( " ( " + text + " ) ", keywords );
		}

		private static void SetMyOrgsFilter( AppUser user, ref string where )
		{
			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";
			if ( user == null || user.Id == 0 )
				return;

			where = where + AND + string.Format( "(id in (	select parentOrgId from [Organization.Member] where userid = {0}) )", user.Id );
		}

		private static void SetOrgPropertiesFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			string searchCategories = UtilityManager.GetAppKeyValue( "orgSearchCategories", "7,8,9,30," );
			string template = " ( base.Id in ( SELECT  [EntityBaseId] FROM [dbo].[EntityProperty_Summary] where EntityTypeId= 2 AND [PropertyValueId] in ({0}))) ";
			//.Where( s => s.CategoryId >= 7 && s.CategoryId < 10 )

			//Updated to use FiltersV2
			string next = "";
			if ( where.Length > 0 )
				AND = " AND ";
			foreach ( var filter in data.FiltersV2.Where( m => m.Type == MainSearchFilterV2Types.CODE ).ToList() )
			{
				var item = filter.AsCodeItem();
				if( searchCategories.Contains( item.CategoryId.ToString() ) )
				{
					next += item.Id + ",";
				}
			}
			next = next.Trim( ',' );
			if ( !string.IsNullOrWhiteSpace( next ) )
			{
				where = where + AND + string.Format( template, next );
			}

			/* //Retained for reference
			foreach ( MainSearchFilter filter in data.Filters )
			{
				if (searchCategories.IndexOf(filter.CategoryId.ToString()) > -1)
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
			*/
		}
		private static void SetOrgServicesFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			string template = " ( base.Id in ( SELECT  [OrganizationId] FROM [dbo].[Organization.ServiceSummary]  where [CodeId] in ({0}))) ";
			//don't really need categoryId - yet

			//Updated to use FiltersV2
			string next = "";
			if ( where.Length > 0 )
				AND = " AND ";
			foreach ( var filter in data.FiltersV2.Where(m => m.Type == MainSearchFilterV2Types.CODE ) )
			{
				var item = filter.AsCodeItem();
				if( item.CategoryId == 6 )
				{
					next += item.Id + ",";
				}
			}
			next = next.Trim( ',' );
			if ( !string.IsNullOrWhiteSpace( next ) )
			{
				where = where + AND + string.Format( template, next );
			}

			/* //Retained for reference
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
			*/
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
		//public static CM.Organization GetLightOrgById( int orgId )
		//{
		//	if ( orgId < 1 )
		//		return null;
		//	string where = string.Format( " base.Id = {0}", orgId );
		//	int pTotalRows = 0;

		//	List<CM.Organization> list = Mgr.Search( where, "", 1, 50, ref pTotalRows );

		//	if ( list.Count > 0 )
		//		return list[ 0 ];
		//	else
		//		return null;
		//}
		#endregion 

		#region Retrievals
		/// <summary>
		/// get organization for edit
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static CM.Organization GetOrganization( int id )
		{
			return Mgr.GetForEdit( id);
		}
		public static CM.Organization GetForSummary( int id )
		{
			return Mgr.GetForSummary( id );
		}
		public static CM.QAOrganization Get_QAOrganization( int id )
		{
			return Mgr.GetQAOrgForEdit( id );
		}
		public static CM.Organization GetOrganizationDetail( int id )
		{
			AppUser user = AccountServices.GetCurrentUser();
			return GetOrganizationDetail( id, user  );

		}
		public static CM.Organization GetOrganizationDetail( int id, AppUser user  )
		{
			CM.Organization entity = Mgr.GetDetail( id, false );
			if (user == null || user.Id == 0)
				return entity;

			if ( CanUserUpdateOrganization( user, entity.Id ) )
				entity.CanUserEditEntity = true;

			return entity;
		}
		public static CM.Organization GetOrganizationForPublish( int id, AppUser user )
		{
			CM.Organization entity = Mgr.GetDetail( id, true );
			if ( user == null || user.Id == 0 )
				return entity;

			if ( CanUserUpdateOrganization( user, entity.Id ) )
				entity.CanUserEditEntity = true;

			return entity;
		}
		public static List<EM.Organization> GetOrganizations( string search )
		{
			return Mgr.GetOrganizations( search );
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
		public static bool CanUserUpdateOrganization( AppUser user, Guid orgRowId )
		{
			CM.Organization org = GetLightOrgByRowId( orgRowId.ToString() );
			if ( org == null || org.Id < 1 )
				return false;
			return CanUserUpdateOrganization( user, org.Id );
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
			if (user == null || user.Id == 0)
				return false;

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
			else if (user == null || user.Id == 0)
				return false;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;

			if ( Mgr.IsOrganizationMember( user.Id, entity.Id ) )
				return true;

			else if ( entity.CreatedById == user.Id || entity.LastUpdatedById == user.Id )
				return true;
			else
				return false;
		}

		/// <summary>
		/// Determine if user can view the organization
		/// - if published, all can view
		/// - if private, only staff, select roles, or user from the owning org can view
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static bool CanUserViewOrganization( int orgId, AppUser user, ref CM.Organization entity )
		{
			bool isValid = false;
			entity = new Organization();
			if ( orgId == 0 )
				return false;

			entity = Mgr.GetForSummary( orgId );
			if ( entity.Id == 0 )
				return false;
			if ( entity.StatusId == CF.CodesManager.ENTITY_STATUS_PUBLISHED )
				return true;

			if ( user == null || user.Id == 0 )
				return false;
			else if ( AccountServices.CanUserViewAllOfSite( user ) )
				return true;

			//is a member of the managing organization 
			if ( OrganizationServices.IsOrganizationMember( user.Id, entity.Id ) )
				return true;

			return isValid;
		}
		public static bool CanUserViewQAOrganization( int orgId, AppUser user, ref CM.Organization entity )
		{
			bool isValid = false;
			entity = new Organization();
			if ( orgId == 0 )
				return false;

			entity = Mgr.GetForSummary( orgId );
			if ( entity.Id == 0 )
				return false;
			if ( entity.StatusId == CF.CodesManager.ENTITY_STATUS_PUBLISHED )
				return true;

			if ( user == null || user.Id == 0 )
				return false;
			else if ( AccountServices.CanUserViewAllOfSite( user ) )
				return true;

			//is a member of the managing organization 
			if ( OrganizationServices.IsOrganizationMember( user.Id, entity.Id ) )
				return true;

			return isValid;
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
		/// Return true if user is a member of the provided organization
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="orgUid"></param>
		/// <returns></returns>
		public static bool IsOrganizationMember( int userId, Guid orgUid )
		{

			if ( Mgr.IsOrganizationMember( userId, orgUid ) )
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
		public int Add( CM.Organization entity, AppUser user, ref bool valid, ref string statusMessage )
		{
			entity.CreatedById = entity.LastUpdatedById = user.Id;
			LoggingHelper.DoTrace( 5, string.Format( "OrganizationServices.Organization_Add. Org: {0}, userId: {1}", entity.Name, entity.CreatedById ) );

			int id = 0;

			Mgr mgr = new Mgr();
			try
			{
				bool isSiteStaff = AccountServices.IsUserSiteStaff( user );
				id = mgr.Add( entity, isSiteStaff, ref statusMessage );
				if ( id > 0 )
				{
					//ConsoleMessageHelper.SetConsoleInfoMessage( "Successfully Added Organization" );
					activityMgr.AddActivity( "Organization", "Add", string.Format( "{0} added a new organization: {1}", user.FullName(), entity.Name ), entity.CreatedById, 0, id );
					
					string url = UtilityManager.FormatAbsoluteUrl( "/editor/Organization/" + id.ToString(),true );

					//notify administration
					string message = string.Format( "New Organization. <ul><li>OrganizationId: {0}</li><li><a href='{3}'>Organization: {1}</a></li><li>Created By: {2}</li></ul>", entity.Id, entity.Name, user.FullName(), url );

					Utilities.EmailManager.SendSiteEmail( "New Organization has been created", message );
				}
				else
				{
					valid = false;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "OrganizationServices.Add" );
			}
			return id;
		}
		public int Add_QAOrg( CM.QAOrganization entity, AppUser user, ref bool valid, ref string statusMessage )
		{
			entity.CreatedById = entity.LastUpdatedById = user.Id;
			LoggingHelper.DoTrace( 5, string.Format( "OrganizationServices.Add_QAOrg. Org: {0}, userId: {1}", entity.Name, entity.CreatedById ) );

			int id = 0;

			Mgr mgr = new Mgr();
			try
			{
				bool isSiteStaff = AccountServices.IsUserSiteStaff( user );
				id = mgr.Add_QAOrg( entity, isSiteStaff, ref statusMessage );
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
				LoggingHelper.LogError( ex, "OrganizationServices.Add_QAOrg" );
			}
			return id;
		}
		/// <summary>
		/// Update an organization
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public bool Update( CM.Organization entity, AppUser user, ref string statusMessage )
		{
			entity.LastUpdatedById = user.Id;
			LoggingHelper.DoTrace( 5, string.Format( "OrganizationServices.Organization_Update. OrgId: {0}, userId: {1}", entity.Id, entity.LastUpdatedById ) );
			Mgr mgr = new Mgr();
			bool isOK = false;
			try
			{
				isOK = mgr.Update( entity, ref statusMessage );
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

		public bool Update_QAOrg( CM.QAOrganization entity, AppUser user, ref string statusMessage )
		{
			entity.LastUpdatedById = user.Id;
			LoggingHelper.DoTrace( 5, string.Format( "OrganizationServices.Update_QAOrg. OrgId: {0}, userId: {1}", entity.Id, entity.LastUpdatedById ) );
			Mgr mgr = new Mgr();
			bool isOK = false;
			try
			{
				isOK = mgr.Update_QAOrg( entity, ref statusMessage );
				if ( isOK )
				{
					ConsoleMessageHelper.SetConsoleInfoMessage( "Successfully Updated Organization" );
					activityMgr.AddActivity( "Organization", "Update", string.Format( "{0} updated organization (or parts of): {1}", user.FullName(), entity.Name ), user.Id, 0, entity.Id );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "OrganizationServices.Update_QAOrg" );
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
					valid = mgr.Delete( organizationID, ref status );
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
        //			status += string.Join( "<br/>", messages.ToArray() );

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

        public static void UpdateOrganizations(int userId, int[] orgs, string currentUser)
        {
            int memberTypeId = 2; //employee - only option for now
            int currentUserId = AccountServices.GetUserByUserName(currentUser).Id;
            Mgr.UpdateOrganizations(userId, orgs, memberTypeId, currentUserId);
        }

        public int OrganizationMember_Save( int orgId, int userId, int orgMemberTypeId, int createdById, ref string statusMessage )
		{

			return new Mgr().OrganizationMember_Save( orgId, userId, orgMemberTypeId, createdById, ref statusMessage );
		}
		public bool OrganizationMember_Delete( int orgId, int userId, ref string statusMessage )
		{
			return new Mgr().OrganizationMember_Delete( orgId, userId, ref statusMessage );
		}

		public static List<CM.OrganizationMember> OrganizationMember_List( int orgId, int pageNumber, int pageSize, ref int pTotalRows )
		{
			List<CM.OrganizationMember> list = Mgr.OrganizationMembers_ListByName( orgId, pageNumber, pageSize, ref pTotalRows );

			return list;
		}
		public static List<MN.ProfileLink> OrganizationMember_Orgs( int userId )
		{
			List<MN.ProfileLink> list = Mgr.SelectUserOrganizationsForProfileList( userId );

			return list;
		}
		public static List<CodeItem> OrganizationMember_OrgsAsCodeItems( int userId )
		{
			List<CodeItem> list = Mgr.SelectUserOrganizationsAsCodeItems( userId );

			return list;
		}
		public static bool IsMemberOfAnOrganization( int userId )
		{
			List<CodeItem> list = Mgr.SelectUserOrganizationsAsCodeItems( userId );
			if ( list != null && list.Count > 0 )
				return true;
			else
				return false;
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
				string property,
				AppUser user, 
				ref string status ) 
		{
			bool isValid = true;

			//TODO - setting context for roles
			string contextRoles = "";
			if ( property == "OfferedByOrganizationRole" )
				contextRoles = CF.Entity_AgentRelationshipManager.VALID_ROLES_OFFERED_BY;
			else if ( property == "QAOrganizationRole" )
				contextRoles = CF.Entity_AgentRelationshipManager.VALID_ROLES_QA;
			else if ( property == "AgentRole_Recipient" )
				contextRoles = CF.Entity_AgentRelationshipManager.VALID_ROLES_QA;

			//parentUid should already be in the entity,
			//or set now
			entity.ParentUid = parentUid;

			List<string> messages = new List<string>();
			isValid = new CF.Entity_AgentRelationshipManager().Agent_EntityRoles_Save( entity, contextRoles,
				user.Id, ref messages );

			if ( !isValid )
				status += string.Join( "<br/>", messages.ToArray() );
			return isValid;
		}

		/// <summary>
		/// Add a single role
		/// </summary>
		/// <param name="contextParentUid"></param>
		/// <param name="agentUid"></param>
		/// <param name="relationshipId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool EntityAgent_SaveRole( Guid contextParentUid, 
				Guid agentUid,
				int relationshipId,
				AppUser user,
				ref string status )
		{
			bool isValid = true;
			bool isEmpty = false;
			Entity entity = CF.EntityManager.GetEntity( contextParentUid );

			List<string> messages = new List<string>();
			int id = new CF.Entity_AgentRelationshipManager().Add( entity.Id, agentUid, relationshipId
				, entity.Id
				, true
				, user.Id, ref messages, ref isEmpty );

			if ( !isValid )
				status += string.Join( "<br/>", messages.ToArray() );
			return isValid;
		}

		[Obsolete]
		public bool CredentialAssets_EntityAgentRole_Save( OrganizationRoleProfile entity,
				Guid parentUid,
				string property,
				AppUser user,
				ref string status )
		{
			bool isValid = true;
			//parentUid should already be in the entity,
			//or set now
			entity.ParentUid = parentUid;
			string contextRoles = "";
			if (property == "OfferedByOrganizationRole" )
				contextRoles = CF.Entity_AgentRelationshipManager.VALID_ROLES_OFFERED_BY;
			else if ( property == "QAOrganizationRole" )
				contextRoles = CF.Entity_AgentRelationshipManager.VALID_ROLES_QA;
			else
				contextRoles = CF.Entity_AgentRelationshipManager.VALID_ROLES_QA;

			List<string> messages = new List<string>();
			isValid = new CF.Entity_AgentRelationshipManager().Agent_EntityRoles_Save( entity, contextRoles,
				user.Id, ref messages );

			if ( !isValid )
				status += string.Join( "<br/>", messages.ToArray() );
			return isValid;
		} //

		public int AddChildOrganization( Guid parentUid, Guid orgChildUid, int roleTypeId, AppUser user, ref bool isValid, ref string status )
		{
			//LoggingHelper.DoTrace( 5, string.Format( "OrganizationServices.Organization_Update. OrgId: {0}, userId: {1}", entity.Id, entity.LastUpdatedById ) );
			Mgr mgr = new Mgr();

			OrganizationRoleProfile entity = new OrganizationRoleProfile();
			entity.RoleTypeId = roleTypeId;
			entity.IsInverseRole = false;
			//The parentOrg is the agent in this scenario, and the child is the entity being acted upon
			//ex: Agent is a Subsidiary of the related Agent
			//so while confusing, the Parent is the dept/subsidiary, and the agent is the parent org. The key is that almost all roles are entered as inverse roles
			//But... seems we want to save the standard way, and handle appropriately for display!
			entity.ParentUid = parentUid;
			entity.ActingAgentUid = orgChildUid;
			//entity.ParentUid = orgChildUid;
			//entity.ActingAgentUid = parentUid;
		
			try
			{
				if ( parentUid == orgChildUid)
				{
					status = "Error - you cannot add the parent organization to itself as Department/Subsidiary.";
					return 0;
				}

				List<string> messages = new List<string>();
				isValid = new CF.Entity_AgentRelationshipManager().Entity_AgentRole_SaveSingleRole( entity,
					user.Id, ref messages );
				if ( messages.Count > 0 )
				{
					status += string.Join( "<br/>", messages.ToArray() );
				}
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
		public bool EntityAgent_DeleteAgentRoles( Guid parentUid, Guid agentUid, AppUser user, ref string status )
		{
			bool valid = true;
			//check if user can edit the entity
			Entity parent = CF.EntityManager.GetEntity( parentUid );
			if ( CanUserEditEntity( user, parent.Id, ref status ) == false )
			{
				status = "Error - you don't have access to perform this action.";
				return false;
			}

			try
			{

				if ( new CF.Entity_AgentRelationshipManager().Delete( parent.Id, agentUid, ref status ) )
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

		public bool EntityAgent_DeleteRole( Guid parentUid, Guid agentUid, int roleId, AppUser user, ref string status )
		{
			bool valid = true;
			//check if user can edit the entity
			Entity parent = CF.EntityManager.GetEntity( parentUid );
			if ( CanUserEditEntity( user, parent.Id, ref status ) == false )
			{
				status = "Error - you don't have access to perform this action.";
				return false;
			}

			try
			{

				if ( new CF.Entity_AgentRelationshipManager().Delete( parentUid, agentUid, roleId, ref status ) )
				{
					//if valid, log. The status message has details of entities and properties
					activityMgr.AddActivity( new SiteActivity()
						{
							Activity ="Agent Role",
							Event = "Delete",
							Comment = string.Format( "{0} {1}", user.FullName(), status ),
							ActionByUserId = user.Id,
							ObjectRelatedId = parent.EntityBaseId
						}
					);
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
			Entity parent = CF.EntityManager.GetEntity( parentUid );
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
				status += string.Join( "<br/>", messages.ToArray() );
				return false;
			}
		
			try
			{
				entity.ParentId = parent.Id;
				entity.ParentUid = parentUid;
				if ( new CF.Entity_QualityAssuranceActionManager().QualityAssuranceAction_SaveProfile( entity, user.Id, ref messages ) == false )
				{
					valid = false;
					status += string.Join( "<br/>", messages.ToArray() );
				}
				else
				{
					status = "Successfully saved Quality Assurance Action ";
					activityMgr.AddActivity( "Quality Assurance Action", "Save " + parent.EntityType, string.Format( "{0} saved Quality Assurance Action for entity: {1}, ID: {2}", user.FullName(), parent.EntityType, parent.EntityBaseId ), user.Id, parent.EntityBaseId, parent.Id );
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
			Entity parent = CF.EntityManager.GetEntity( parentUid );
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
					activityMgr.AddActivity( "Quality Assurance Action", "Delete for " + parent.EntityType, string.Format( "{0} deleted Quality Assurance Action for entity: {1}, ID: {2}", user.FullName(), parent.EntityType, parent.EntityBaseId ), user.Id, parent.EntityBaseId, parent.Id );
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



		#region VerificationStatus

		#region retrieval

		/// <summary>
		/// Get a VerificationStatus By integer Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static VerificationStatus VerificationStatus_Get( int profileId )
		{
			VerificationStatus profile = CF.Organization_VerificationStatusManager.Get( profileId );
			return profile;
		}

		#endregion

		#region Persistance
		public bool VerificationStatus_Save( VerificationStatus entity, Guid parentUid, string action, AppUser user, ref string status )
		{
			bool valid = true;
			status = "";
			//TODO - will int orgId or guid be passed?
			List<string> messages = new List<string>();
			CF.Organization_VerificationStatusManager mgr = new CF.Organization_VerificationStatusManager();
			//validate user has access. Parent can be multiple types, but aways is an entity
			Organization parent = CF.OrganizationManager.GetForSummary( parentUid );
			//Credential credential = CredentialServices.GetBasicCredential( credentialUid );

			try
			{
				if ( mgr.Save( entity, parent.Id, user.Id, ref messages ) == false )
				{
					valid = false;
					status = string.Join( "<br/>", messages.ToArray() );
				}
				else
				{
					status = "Successful";
					//should the activity be for the parent
					activityMgr.AddActivity( "Verification Status", action, string.Format( "{0} saved Verification Status item: {1}, for Organization: {2} (id:{3})", user.FullName(), entity.ProfileName, parent.Name, parent.Id ), user.Id, 0, entity.Id );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".VerificationStatus_Save" );
				valid = false;
				status = ex.Message;
			}
			return valid;
		}
		public bool VerificationStatus_Delete( int parentId, int profileId, AppUser user, ref string status )
		{
			bool valid = true;
			CF.Organization_VerificationStatusManager mgr = new CF.Organization_VerificationStatusManager();
			try
			{
				//get profile and ensure user has access
				VerificationStatus profile = CF.Organization_VerificationStatusManager.Get( profileId, false );
				if ( profile == null || profile.Id == 0 )
				{
					status = "Error - the requested profile was not found.";
					return false;
				}
				else if ( parentId != profile.ParentId )
				{
					status = "Error - invalid request based on parentId";
					return false;
				}

				if ( mgr.Delete( profileId, ref status ) )
				{
					//if valid, log
					activityMgr.AddActivity( "Verification Status Item", "Delete", string.Format( "{0} deleted Verification Status Item {1} ({2}) from Organization: {3}", user.FullName(), profile.ProfileName, profileId, parentId ), user.Id, 0, profileId );
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
				LoggingHelper.LogError( ex, thisClassName + ".VerificationStatus_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}
		#endregion

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
			if (user == null || user.Id == 0)
				return false;
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
