using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web;

using Factories;
using Mgr = Factories.CredentialManager;
using Models;
using Models.ProfileModels;
using Models.Search;
using MC = Models.Common;
using MN = Models.Node;

//using Data;
using Utilities;

namespace CTIServices
{
	public class CredentialServices
	{
		static string thisClassName = "CredentialServices";
		ActivityServices activityMgr = new ActivityServices();

		public List<string> messages = new List<string>();

		public CredentialServices()
		{
		}


		#region search 
		/// <summary>
		/// Credential autocomplete
		/// Needs to check authorization level for credential
		/// </summary>
		/// <param name="keyword"></param>
		/// <param name="maxTerms"></param>
		/// <returns></returns>
		public static List<string> Autocomplete( string keyword,  int maxTerms = 25 )
		{
			int userId = 0;
			string where = "";
			int pTotalRows = 0;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;
			SetAuthorizationFilter( user, ref where );

			SetKeywordFilter( keyword, true, ref where );
			
			return CredentialManager.Autocomplete( where, 1, maxTerms, userId, ref pTotalRows );
		}

		public static List<string> Autocomplete_Subjects( string keyword, int maxTerms = 25 )
		{
			//tough to do the user specific stuff

			//int userId = 0;
			//string where = "";
			//int pTotalRows = 0;
			//AppUser user = AccountServices.GetCurrentUser();
			//if ( user != null && user.Id > 0 )
			//	userId = user.Id;
			//SetAuthorizationFilter( user, ref where );

			List<string> list = 
			Entity_ReferenceManager.QuickSearch_TextValue(1, 34, keyword, maxTerms );

			//return CredentialManager.Autocomplete( where, 1, maxTerms, userId, ref pTotalRows );

			return list;
		}
		//public static List<MC.CredentialSummary> QuickSearch( string keyword = "" )
		//{
		//	int userId = 0;
		//	string where = "";
		//	int pTotalRows = 0;
		//	AppUser user = AccountServices.GetCurrentUser();
		//	if ( user != null && user.Id > 0 )
		//		userId = user.Id;
		//	SetAuthorizationFilter( user, ref where );

		//	SetKeywordFilter( keyword, true, ref where );

		//	//return CredentialManager.QuickSearch( keyword, userId );

		//	return CredentialManager.Search( where, "", 1, 25, ref pTotalRows, userId );
		//}

		public static List<MC.CredentialSummary> QACredentialsSearch( int orgId = 0, string keywords = "" )
		{
			int pTotalRows = 0;
			return QACredentialsSearch( orgId, keywords, 1, 400, ref pTotalRows );
		}

		/// <summary>
		/// Called from _EditorCore to return list of QA credentials
		/// TODO - allow filter by creator org
		/// </summary>
		/// <param name="keyword"></param>
		/// <returns></returns>
		public static List<MC.CredentialSummary> QACredentialsSearch( int orgId, string keywords, int pageNumber, int pageSize, ref int pTotalRows )
		{
			string pOrderBy = "";
			string AND = "";
			int userId = AccountServices.GetCurrentUserId();
			//string filter = " ( base.EntityUid in ( SELECT  [ParentUid] FROM [dbo].[EntityProperty_Summary] where categoryid = 2  and PropertySchemaName = 'qualityAssurance')) ";
			string filter = " ( base.CredentialTypeSchema = 'qualityAssurance') ";
			//string filter = "";

			if ( filter.Length > 0 )
				AND = " AND ";
			if ( !string.IsNullOrWhiteSpace( keywords ) )
			{
				keywords = ServiceHelper.HandleApostrophes( keywords );
				if ( keywords.IndexOf( "%" ) == -1 )
					keywords = "%" + keywords.Trim() + "%";
				filter = filter + AND + string.Format( " (base.name like '{0}' OR base.Description like '{0}'  OR base.Url like '{0}' OR organizationName like '{0}'  OR owingOrganization like '{0}')", keywords );
			}
			if ( orgId > 0 )
			{
				if ( filter.Length > 0 )
					AND = " AND ";

				filter = filter + AND + string.Format( " (base.OrgId = {0} )", orgId );
			}
			return CredentialManager.Search( filter, pOrderBy, pageNumber, pageSize, ref pTotalRows, userId );

		}
	
		public static List<MC.CredentialSummary> Search( string keywords, int pageNumber, int pageSize, ref int pTotalRows )
		{
			string pOrderBy = "";
			string filter = "";
			int userId = AccountServices.GetCurrentUserId();

			if ( !string.IsNullOrWhiteSpace( keywords )) 
			{
				keywords = ServiceHelper.HandleApostrophes( keywords );
				if ( keywords.IndexOf( "%" ) == -1 )
					keywords = "%" + keywords.Trim() + "%";
				//OR base.Url like '{0}' 
				filter = string.Format( " (base.name like '{0}' OR base.Description like '{0}'  OR organizationName like '{0}' OR owingOrganization like '{0}')", keywords );
			}
			return CredentialManager.Search( filter, pOrderBy, pageNumber, pageSize, ref pTotalRows, userId );
		}

		public static List<MC.CredentialSummary> Search( MainSearchInput data, ref int pTotalRows )
		{
			string where = "";
			int userId = 0;
			List<string> competencies = new List<string>();

			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;

			SetKeywordFilter( data.Keywords, false, ref where );
			SearchServices.SetSubjectsFilter( data, "Credential", ref where );

			SetAuthorizationFilter( user, ref where );

			SetPropertiesFilter( data, ref where );

			SearchServices.SetRolesFilter( data, ref where );
			SearchServices.SetBoundariesFilter( data, ref where );
			//SetBoundariesFilter( data, ref where );

			//naics, ONET
			SetFrameworksFilter( data, ref where );
			//Competencies
			SetCompetenciesFilter( data, ref where, ref competencies );
			SetCredCategoryFilter( data, ref where );

			return CredentialManager.Search( where, data.SortOrder, data.StartPage, data.PageSize, ref pTotalRows, userId );
		}
	
		private static void SetKeywordFilter(string keywords, bool isBasic, ref string where)
		{
			if ( string.IsNullOrWhiteSpace( keywords ) )
				return;
			string text = " (base.name like '{0}' OR base.Description like '{0}'  OR organizationName like '{0}' OR owingOrganization like '{0}' ) ";
			string subjectsEtc = " OR (base.Id in (SELECT c.id FROM [dbo].[Entity.Reference] a inner join Entity b on a.EntityId = b.Id inner join Credential c on b.EntityUid = c.RowId where [CategoryId] in (10,11,34 ,35) and a.TextValue like '{0}' )) ";
			string frameworkItems = " OR (EntityUid in (SELECT EntityUid FROM [dbo].[Entity.FrameworkItemSummary_ForCredentials] a where  a.title like '{0}' )) ";
			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";

			keywords = ServiceHelper.HandleApostrophes( keywords );
			if ( keywords.IndexOf( "%" ) == -1 )
				keywords = "%" + keywords.Trim() + "%";

			//skip url  OR base.Url like '{0}' 
			if ( isBasic )
				where = where + AND + string.Format( " ( " + text + " ) ", keywords );
			else 
				where = where + AND + string.Format( " ( " + text + subjectsEtc + frameworkItems + " ) ", keywords );
			
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
				where = where + AND + string.Format( " (base.StatusId = {0}) ", CodesManager.ENTITY_STATUS_PUBLISHED );
				return;
			}

			if ( AccountServices.IsUserSiteStaff( user )
			  || AccountServices.CanUserViewAllContent( user) )
			{
				//can view all, edit all
				return;
			}
			
			//can only view where status is published, or associated with the org
			where = where + AND + string.Format( "((base.StatusId = {0}) OR (base.Id in (SELECT cs.Id FROM [dbo].[Organization.Member] om inner join [Credential_Summary] cs on om.ParentOrgId = cs.ManagingOrgId where userId = {1}) ))", CodesManager.ENTITY_STATUS_PUBLISHED, user.Id );
			
		}
		private static void SetPropertiesFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			string template = " ( base.EntityUid in ( SELECT  [ParentUid] FROM [dbo].[Entity.Property] where [PropertyValueId] in ({0}))) ";
			foreach ( MainSearchFilter filter in data.Filters.Where( s => s.CategoryId >= 2 && s.CategoryId < 5 ) )
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
		private static void SetFrameworksFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			string codeTemplate = "  (base.Id in (SELECT c.id FROM [dbo].[Entity.FrameworkItemSummary] a inner join Entity b on a.EntityId = b.Id inner join Credential c on b.EntityUid = c.RowId where [CategoryId] = {0} and ([CodeGroup] in ({1})  OR ([CodeId] in ({1}) )  )) ) ";
			//string codeTemplate1 = "  (base.Id in (SELECT c.id FROM [dbo].[Entity.FrameworkItemSummary] a inner join Entity b on a.EntityId = b.Id inner join Credential c on b.EntityUid = c.RowId where [CategoryId] = {0} and [CodeId] in ({1}))  ) ";
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
}
		private static void SetCompetenciesAutocompleteFilter( string keywords, ref string where)
		{
			List<string> competencies = new List<string>();
			MainSearchInput data = new MainSearchInput();
			MainSearchFilter filter = new MainSearchFilter() {Name = "competencies", CategoryId=29};
			filter.Items.Add(keywords);
			SetCompetenciesFilter( data, ref where, ref competencies );

		}
		private static void SetCompetenciesFilter( MainSearchInput data, ref string where, ref List<string> competencies )
		{
			string AND = "";
			string OR = "";
			string keyword = "";
			string template = " ( base.Id in (SELECT distinct  CredentialId FROM [dbo].[ConditionProfile_LearningOpp_Competencies_Summary]  where AlignmentType in ('teaches', 'assesses') AND ({0}) ) ) ";
			string phraseTemplate = " ([Name] like '%{0}%' OR [Description] like '%{0}%') ";
			//
			foreach ( MainSearchFilter filter in data.Filters.Where( s => s.Name == "competencies" ) )
			{
				string next = "";
				if ( where.Length > 0 )
					AND = " AND ";
				foreach ( string item in filter.Items )
				{
					keyword = ServiceHelper.HandleApostrophes( item );
					//if ( keyword.IndexOf( "%" ) == -1 )
					//	keyword = "%" + keyword.Trim() + "%";
					if ( keyword.IndexOf( ";" ) > -1 )
					{
						var words = keyword.Split( ';' );
						foreach ( string word in words )
						{
							competencies.Add( word.Trim() );
							next += OR + string.Format( phraseTemplate, word.Trim() );
							OR = " OR ";
						}

					}
					else
					{
						competencies.Add( keyword.Trim() );
						//next = "%" + keyword.Trim() + "%";
						next = string.Format( phraseTemplate, keyword.Trim() );
					}
					//next += keyword;	//					+",";
					//just handle one for now
					break;
				}
				//next = next.Trim( ',' );
				if ( !string.IsNullOrWhiteSpace( next ) )
					where = where + AND + string.Format( template, next );

				break;
			}
		}
		//
		private static void SetCredCategoryFilter( MainSearchInput data, ref string where )
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
					where = where + AND + " ( base.CredentialTypeSchema <> 'qualityAssurance') ";
				else if ( item == "includeQualityAssurance" )  //IsAQAOrganization = true
					where = where + AND + " ( base.CredentialTypeSchema = 'qualityAssurance') ";
			}
		}

		#endregion

		#region Retrievals
		public static MC.Credential GetCredential( int id, bool forEditView = false, bool isNewVersion = true)
		{

			MC.Credential cred = CredentialManager.Credential_Get( id, forEditView, isNewVersion );

			//get properties
			return cred;
		}
		/// <summary>
		/// Get a minimal credential
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static MC.Credential GetBasicCredential( int id, bool isNewVersion = true )
		{

			MC.Credential cred = CredentialManager.Credential_GetBasic( id, false, false );

			return cred;
		}
		public static MC.Credential GetBasicCredential( Guid uid, bool forEditView = false, bool isNewVersion = true )
		{
			return CredentialManager.Credential_GetByRowId( uid,false, false, forEditView );
		
		}

		/// <summary>
		/// Get a 'light' credential by Id
		/// This will return mimimum information, and NO child properties
		/// Used by search, so user check is not necessary as done by search
		/// </summary>
		/// <param name="rowId"></param>
		/// <returns></returns>
		public static MC.CredentialSummary GetLightCredentialByRowId( string rowId )
		{
			if ( !CredentialManager.IsValidGuid( rowId ) )
				return null;

			string where = string.Format( " EntityUid = '{0}'", rowId );
			int pTotalRows = 0;

			List<MC.CredentialSummary> list = CredentialManager.Search( where, "", 1, 50, ref pTotalRows );

			if ( list.Count > 0 )
				return list[ 0 ];
			else
				return null;
		}

		/// <summary>
		/// Get a 'light' credential by Id
		/// This will return mimimum information, and NO child properties
		/// Used by search, so user check is not necessary as done by search
		/// </summary>
		/// <param name="credentialId"></param>
		/// <returns></returns>
		public static MC.CredentialSummary GetLightCredentialById( int credentialId )
		{
			if ( credentialId < 1 )
				return null;
			string where = string.Format( " base.Id = {0}", credentialId );
			int pTotalRows = 0;

			List<MC.CredentialSummary> list = Mgr.Search( where, "", 1, 50, ref pTotalRows );

			if ( list.Count > 0 )
				return list[ 0 ];
			else
				return null;
		}
		public static MC.Credential GetCredentialDetail( int id )
		{
			AppUser user = AccountServices.GetCurrentUser();
			return GetCredentialDetail( id, user );

		}
		public static MC.Credential GetCredentialDetail( int id, AppUser user )
		{
			string statusMessage = "";
			int cacheMinutes= UtilityManager.GetAppKeyValue( "credentialCacheMinutes", 0 );
			DateTime maxTime  = DateTime.Now.AddMinutes( cacheMinutes * -1 );

			string key = "credential_" + id.ToString();

			if ( HttpRuntime.Cache[ key ] != null && cacheMinutes > 0 )
			{
				var cache = ( CachedCredential ) HttpRuntime.Cache[ key ];
				try
				{
					if ( cache.lastUpdated > maxTime )
					{
						LoggingHelper.DoTrace( 6, string.Format( "===CredentialServices.GetCredentialDetail === Using cached version of Credential, Id: {0}, {1}", cache.Item.Id, cache.Item.Name ) );

						//check if user can update the object
						string status = "";
						if ( !CanUserUpdateCredential( id, user, ref status ) )
							cache.Item.CanEditRecord = false;

						return cache.Item;
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.DoTrace( 6, "===CredentialServices.GetCredentialDetail === exception " + ex.Message );
				}
			}
			else
			{
				LoggingHelper.DoTrace( 6, string.Format( "****** CredentialServices.GetCredentialDetail === Retrieving full version of credential, Id: {0}", id ) );
			}

			DateTime start = DateTime.Now;

			MC.Credential entity = CredentialManager.Credential_Get( id, false, true );
			if ( CanUserUpdateCredential( entity, user, ref statusMessage ) )
				entity.CanUserEditEntity = true;

			DateTime end = DateTime.Now;
			int elasped = ( end - start ).Seconds;
			//Cache the output if more than 3? seconds
			if ( key.Length > 0 && cacheMinutes > 0 && elasped > 3)
			{
				var newCache = new CachedCredential()
				{
					Item = entity,
					lastUpdated = DateTime.Now
				};
				if ( HttpContext.Current != null )
				{
					if ( HttpContext.Current.Cache[ key ] != null )
					{
						HttpRuntime.Cache.Remove( key );
						HttpRuntime.Cache.Insert( key, newCache );

						LoggingHelper.DoTrace( 5, string.Format( "===CredentialServices.GetCredentialDetail $$$ Updating cached version of credential, Id: {0}, {1}", entity.Id, entity.Name ) );

					}
					else
					{
						LoggingHelper.DoTrace( 5, string.Format( "===CredentialServices.GetCredentialDetail ****** Inserting new cached version of credential, Id: {0}, {1}", entity.Id, entity.Name ) );

						System.Web.HttpRuntime.Cache.Insert( key, newCache, null, DateTime.Now.AddHours( cacheMinutes ), TimeSpan.Zero );
					}
				}
			}
			else
			{
				LoggingHelper.DoTrace( 5, string.Format( "===CredentialServices.GetCredentialDetail $$$$$$ skipping caching of credential, Id: {0}, {1}, elasped:{2}", entity.Id, entity.Name, elasped ) );
			}

			return entity;
		}

		/// <summary>
		/// Retrieve Credential for compare purposes
		/// - name, description, cred type, education level, 
		/// - industries, occupations
		/// - owner role
		/// - duration
		/// - estimated costs
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static MC.Credential GetCredentialForCompare( int id )
		{
			//not clear if checks necessary, as interface only allows selection of those to which the user has access.
			AppUser user = AccountServices.GetCurrentUser();
			if ( Utilities.UtilityManager.GetAppKeyValue( "usingNewCompareMethod", true ) == false )
			{
				LoggingHelper.DoTrace( 2, string.Format( "GetCredentialForCompare - using OLD GetCredentialDetail for cred: {0}", id ) );

				return GetCredentialDetail( id, user );
			}

			LoggingHelper.DoTrace( 2, string.Format( "GetCredentialForCompare - using new compare get for cred: {0}", id ) );
			//================================================
			string statusMessage = "";
			string key = "credentialCompare_" + id.ToString();

			 MC.Credential entity = new MC.Credential();

			 if ( CacheManager.IsCredentialAvailableFromCache( id, key, ref entity ) )
			 {
				 //check if user can update the object
				 string status = "";
				 if ( !CanUserUpdateCredential( id, user, ref status ) )
					 entity.CanEditRecord = false;
				 return entity;
			 }
				
			CredentialRequest cr = new CredentialRequest();
			cr.IsCompareRequest();
			

			DateTime start = DateTime.Now;

			entity = CredentialManager.Credential_Get( id, cr );

			if ( CanUserUpdateCredential( entity, user, ref statusMessage ) )
				entity.CanUserEditEntity = true;

			DateTime end = DateTime.Now;
			int elasped = ( end - start ).Seconds;
			if ( elasped > 1)
				CacheManager.AddCredentialToCache( entity, key );

			return entity;
		} //


		private void RemoveCredentialFromCache( int credentialId )
		{
			CacheManager.RemoveItemFromCache( "credential", credentialId );
			CacheManager.RemoveItemFromCache( "credentialCompare", credentialId );
		} //


		//private void RemoveFromCache( string type, int id)
		//{
		//	string key = string.Format("{0}_{1}", type, id);
		//	if ( HttpContext.Current != null 
		//		&& HttpContext.Current.Cache[ key ] != null )
		//	{
		//		HttpRuntime.Cache.Remove( key );

		//		LoggingHelper.DoTrace( 6, string.Format( "===CredentialServices.RemoveFromCache $$$ Removed cached version of a {0}, Id: {1}", type, id ) );

		//	}
		//}
		#endregion
		//public MC.Credential GetDemoCredential( int id )
		//{
		//	return new Data.Development.Sample().GetSampleCredential( id );
		//}

		#region === authorization =============
		/// <summary>
		/// Determine if user has edit access to the credential
		/// Use this version where it is useful to have the credential available to display information for activity logging
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static bool CanUserUpdateCredential( int credentialId, AppUser user, ref string status, ref MC.Credential entity )
		{
			if ( credentialId == 0 )
				return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;
			entity = GetBasicCredential( credentialId );

			return CredentialServices.CanUserUpdateCredential( entity, user, ref status );
		}

		/// <summary>
		/// Determine if user has edit access to the credential
		/// Use this version where there is no current credential, or the current one may not have all pertinent properties, especially ManagingOrgId
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static bool CanUserUpdateCredential( int credentialId, AppUser user, ref string status)
		{
			if ( credentialId == 0 )
				return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;
			
			MC.Credential entity = GetBasicCredential( credentialId );

			return CredentialServices.CanUserUpdateCredential( entity, user, ref status );
		}
		/// <summary>
		/// Determine if user has edit access to the credential
		/// </summary>
		/// <param name="credentialUid"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static bool CanUserUpdateCredential( Guid credentialUid, AppUser user, ref string status )
		{
			//if empty, OK
			if ( !ServiceHelper.IsValidGuid(credentialUid) )
				return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;
			MC.Credential entity = GetBasicCredential( credentialUid, false );

			return CanUserUpdateCredential( entity, user, ref status );
		}
		/// <summary>
		/// Determine credential access for a user
		/// If Admin - all
		/// else If member of an org that has a creator or owned by org role with credential - all
		/// else none
		/// NOTE: this version assumes all data has been read/refreshed from the db, including ManagingOrgId
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static bool CanUserUpdateCredential( MC.Credential entity, AppUser user, ref string status )
		{
			bool isValid = false;
			if ( entity.Id == 0 )
				return true;
			else if (user == null || user.Id == 0)
				return false;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;

			//is a member of the credential managing organization 
			if ( OrganizationServices.IsOrganizationMember( user.Id, entity.ManagingOrgId ) )
				return true;

			return isValid;
		}


		/// <summary>
		/// Determine if user has edit access to the credential profile
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="profileId"></param>
		/// <param name="profileType"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static bool CanUserUpdateCredentialProfile( int credentialId, int profileId, string profileType, AppUser user, ref string status )
		{
			if ( AccountServices.IsUserSiteStaff( user ) )
				return true;
			MC.Credential entity = GetBasicCredential( credentialId );
			//first determine if user can update credential
			if ( !CredentialServices.CanUserUpdateCredential( entity, user, ref status ) )
				return false;
			if ( profileId == 0 )
				return true;
			
			//TODO ==> incomplete
			//now check if profile is related to the credential. How?
			//upper level profiles will have a Entity parent, but may be difficult to be too generic here
			//Context 1: profile is under Entity, and would therefore also have an Entity where Entity.EntityUid = profile.RowId. The profile.EntityId should point to the Entity where the Entity.EntityUid = credential.RowId
			return CredentialServices.CanUserUpdateCredential( entity, user, ref status );
		}
		#endregion

		#region === add/update/delete =============
		/// <summary>
		/// Add a credential stack
		/// NOTE: should we do the basic add, then to the additional parts from here? Primarily for allowing the cred to be created, and then ensure the other parts are valid
		/// ==> NO, on return the whole cred is read, which would wipe out the data that had issues!
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public int Credential_Add( MC.Credential entity, AppUser user, ref bool valid, ref string status )
		{
			//, bool isNewVersion = true
			LoggingHelper.DoTrace( 5, string.Format( thisClassName + ".Credential_Add. Credential: {0}, userId: {1}", entity.Name, entity.CreatedById ) );
			var id = 0;
			List<string> messages = new List<string>();
			Mgr mgr = new Mgr();
			if ( !ValidateCredential( entity, ref messages ) )
			{
				status = string.Join( ",", messages.ToArray() );
				valid = false;
				return 0;
			}
			try
			{
				//set by caller, to keep cleaner
				//entity.IsNewVersion = isNewVersion;

				//set the managing orgId
				entity.ManagingOrgId = OrganizationManager.GetPrimaryOrganizationId( user.Id );

				id = mgr.Credential_Add( entity, ref status );
				valid = id > 0;
				if ( id > 0 )
				{
					ConsoleMessageHelper.SetConsoleInfoMessage( "Successfully Added Credential" );
					activityMgr.AddActivity( "Credential", "Add", string.Format("{0} added a new credential: {1}", user.FullName(), entity.Name), entity.CreatedById, 0, id );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Credential_Add" );
				status = ex.Message;
				valid = false;
			}

			return id;
}
		/// <summary>
		/// Save a credential - vai new editor
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool Credential_Save( MC.Credential entity, AppUser user, ref string status )
		{
			entity.IsNewVersion = true;
			return Credential_Update( entity, user, ref status );
		}

		public bool Credential_Update( MC.Credential entity, AppUser user, ref string status )
		{
			LoggingHelper.DoTrace( 5, string.Format( thisClassName + ".Credential_Update. CredentialId: {0}, userId: {1}", entity.Id, entity.LastUpdatedById ) );
			Mgr mgr = new Mgr();
			bool valid = true;
			if ( !ValidateCredential( entity, ref messages ) )
			{
				status = string.Join( ",", messages.ToArray() );
				return false;
			}
			try
			{
				valid = mgr.Credential_Update( entity, ref status );
				if ( valid )
				{
					ConsoleMessageHelper.SetConsoleInfoMessage( "Successfully Updated Credential" );
					activityMgr.AddActivity( "Credential", "Update", string.Format( "{0} updated credential (or parts of): {1}", user.FullName(), entity.Name ), user.Id, 0, entity.Id );

					//remove from cache
					//RemoveFromCache( "credential",entity.Id );
					RemoveCredentialFromCache( entity.Id );
					
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Credential_Update" );
				valid = false;
				status = ex.Message;
			}

			return valid;
		}

		//public bool Credential_Delete( int credentialId, AppUser user )
		//{
		//	var isOK = false;
		//	var statusMessage = "";
		//	return Credential_Delete( credentialId, user,ref isOK, ref statusMessage );
		//}
		public bool Credential_Delete( int credentialId, AppUser user, ref string status )
		{
			Mgr mgr = new Mgr();
			bool valid = true;
			try
			{
				MC.Credential entity = new MC.Credential();
				if (CanUserUpdateCredential( credentialId, user, ref status, ref entity ) == false) 
				{
					status = "You do not have authorization to delete this credential";
					valid = false;
					return false;
				}
				valid = mgr.Credential_Delete( credentialId, user.Id, ref status );
				if ( valid )
				{
					activityMgr.AddActivity( "Credential", "Deactivate", string.Format( "{0} deactivated credential: {1} (id: {2})", user.FullName(), entity.Name, entity.Id ), user.Id, 0, credentialId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Credential_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		//[Obsolete]
		//public bool DeleteProfile( int profileId, string profileName, AppUser user, ref string status )
		//{
		//	//TODO - need to validate user has access to the current credential
		//	//		- either interface passes the credentialId or look up based on clientProfile
		//	bool valid = true;
		//	try
		//	{
		//		switch ( profileName.ToLower() )
		//		{

		//			case "durationprofile":
		//				//valid = new CredentialTimeToEarnManager().Credential_TimeToEarnDelete( profileId, ref status );

		//				valid = new DurationProfileManager().DurationProfile_Delete( profileId, ref status );
		//				break;
		//			case "geocoordinates":
		//				valid = new RegionsManager().GeoCoordinate_Delete( profileId, ref status );
		//				break;
		//			case "jurisdictionprofile":
		//				valid = new RegionsManager().JurisdictionProfile_Delete( profileId, ref status );
		//				break;
		//			case "organizationrole":
		//				valid = new OrganizationRoleManager().CredentialOrgRole_Delete( profileId, ref status );
		//				break;
		//			case "qualityassuranceaction":
		//				valid = new OrganizationRoleManager().CredentialOrgRole_Delete( profileId, ref status );
		//				break;
		//			case "conditionprofile":
		//				valid = new ConnectionProfileManager().ConditionProfile_Delete( profileId, ref status );
		//				break;
		//			case "taskprofile":
		//				valid = new Entity_TaskProfileManager().TaskProfile_Delete( profileId, ref status );
		//				break;
		//			case "remove":
		//				valid = new Entity_RevocationProfileManager().Delete( profileId, ref status );
		//				break;
		//			case "revocationprofile":
		//				valid = new Entity_RevocationProfileManager().Delete( profileId, ref status );
		//				break;
		//			case "urlprofile":
		//				valid = new Entity_ReferenceManager().Delete( profileId, ref status );
		//				break;
		//			default:
		//				valid = false;
		//			status = "Deleting the requested clientProfile is not handled at this time.";
		//				return false;
		//		}
			

		//		if ( valid )
		//		{
		//			//if valid, status contains the cred name and id
		//			activityMgr.AddActivity( "Credential Profile", "Delete profileName", string.Format( "{0} deleted credential clientProfile {1}", user.FullName(), profileName ), user.Id, 0, profileId );
		//			status = "";
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".DeleteProfile" );
		//		status = ex.Message;
		//		valid = false;
		//	}

		//	return valid;
		//}
		private bool ValidateCredential( MC.Credential entity, ref List<string> messages )
		{
			bool isValid = true;
//			List<string> messages = new List<string>();
			if ( string.IsNullOrWhiteSpace( entity.Name ) )
				messages.Add("Credential name is required");

			if ( string.IsNullOrWhiteSpace( entity.Description ) )
				messages.Add( "Credential description is required" );

			//must have a type
			if ( CodesManager.GetEnumerationSelection( entity.CredentialType ) == 0
				&& string.IsNullOrWhiteSpace( entity.CredentialType.OtherValue))
				messages.Add( "Credential type is required" );


			if ( messages.Count > 0 )
				isValid = false;

			return isValid;
		}
		

		#endregion

		#region Credential Profiles
		//public bool Credential_UpdateSection( MC.Credential entity, AppUser user, string section, ref string status )
		//{
		//	LoggingHelper.DoTrace( 5, string.Format( thisClassName + ".Credential_UpdateSection. CredentialId: {0}, section: {1}, userId: {2}", entity.Id, entity.LastUpdatedById, section ) );
		//	bool valid = true;

		//	try
		//	{
		//		status = "Not handled yet";
		//		switch ( section )
		//		{
		//			//
		//			case "organizationRole":
		//				return Credential_UpdateOrgRole( entity, user, ref status );
		//			case "qualityRole":
		//				return Credential_UpdateQAActions( entity, user, ref status );
		//			case "codes":
		//			case "soc":
		//			case "cip":
		//			case "naics":
		//				//no action necessary
		//				status = "NOTE: No action - data is saved upon selection";
		//				return false;

		//			case "requires":
		//			case "recommends":
		//			case "isrequiredfor":
		//			case "isrecommendedfor":
		//			case "renew":
		//				return Credential_UpdateConditionProfiles( entity, section, user, ref status );
		//			case "remove":
		//				return RevocationProfile_Update( entity, section, user, ref status );
		//			case "retain":
		//				return false;
		//			case "qaOrganizationRole":
		//				return false;
		//			case "earnings":
		//				return false;
		//			case "outcomes":
		//				return false;
		//			case "process":
		//				return false;
		//			default:
		//				valid = false;
		//				return false;
		//		}

		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".Credential_UpdateSection" );
		//		valid = false;
		//		status = ex.Message;
		//	}

		//	return valid;
		//}

		#endregion

		#region org roles OBSOLETE
		//[Obsolete]
		//private bool Credential_UpdateOrgRole( MC.Credential entity, AppUser user, ref string status )
		//{
		//	bool valid = true;
		//	status = "";
		//	List<string> messages = new List<string>();
		//	OrganizationRoleManager mgr = new OrganizationRoleManager();
		//	try
		//	{
		//		int count = 0;
		//		//if handling all from a list ==> prefer to NOT do this - one at a time?
		//		//also assume delete will be immediate, and so only adds are done? Actually could change the role at any time!
		//		if ( mgr.Credential_UpdateOrgRoles( entity, ref messages, ref count ) == false )
		//		{
		//			valid = false;
		//		}
		//		else if ( count > 0 )
		//		{
		//			status = "Successfully Added Organization Role(s); ";
		//			activityMgr.AddActivity( "CredentialOrgRole", "Modify", string.Format( "{0} added/updated credential to org role(s): {1}, count:{2}", user.FullName(), entity.Name, count ), user.Id, 0, entity.Id );
		//		}


		//		if ( !valid )
		//			status += string.Join( ",", messages.ToArray() );

		//		if ( valid )
		//		{
		//			if ( count == 0 )
		//			{
		//				//nothing to update
		//				status = "No Action - There were no role(s) to update";
		//			}
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".Credential_UpdateOrgRole" );
		//		valid = false;
		//		status = ex.Message;
		//	}
		//	return valid;
		//}
		//private bool Credential_UpdateQAActions( MC.Credential entity, AppUser user, ref string status )
		//{
		//	bool valid = true;
		//	status = "";
		//	List<string> messages = new List<string>();
		//	OrganizationRoleManager mgr = new OrganizationRoleManager();
		//	try
		//	{
		//		int count = 0;
		//		int actionsCount = 0;

		//		if ( mgr.Credential_UpdateQAActions( entity, ref messages, ref actionsCount ) == false )
		//		{
		//			valid = false;
		//		}
		//		else if ( actionsCount > 0 )
		//		{
		//			status += "Successfully Added Organization QA Action(s)";
		//			activityMgr.AddActivity( "CredentialOrg QA Action", "Modify", string.Format( "{0} added/updated Org to credential action(s): {1}, count:{2}", user.FullName(), entity.Name, actionsCount ), user.Id, 0, entity.Id );
		//		}

		//		if ( !valid )
		//			status += string.Join( ",", messages.ToArray() );
		//		count += actionsCount;

		//		if ( valid )
		//		{
		//			if ( count == 0 )
		//			{
		//				//nothing to update
		//				status = "No Action - There were no role(s) to update";
		//			}
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".Credential_UpdateQAActions" );
		//		valid = false;
		//		status = ex.Message;
		//	}
		//	return valid;
		//}
		#endregion

		#region Single OrgRoleProfile - OBSOLETE
		//public static OrganizationRoleProfile GetCredentialOrgRoles_AsEnumeration( int credentialId, int orgId )
		//{
		//	return OrganizationRoleManager.GetCredentialOrgRoles_AsEnumeration( credentialId, orgId );
		//}


		/// <summary>
		/// Add/Update a single OrganizationRoleProfile
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="credentialId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		//public bool Credential_SaveOrgRole( OrganizationRoleProfile entity, int credentialId, AppUser user, ref string status )
		//{
		//	bool valid = true;
		//	status = "";
		//	List<string> messages = new List<string>();
		//	//check if user can edit the entity
		//	MC.Credential cred = new MC.Credential();
		//	if ( CanUserUpdateCredential( credentialId, user, ref status, ref cred ) == false ) 
		//	{
		//		status = "Error - you don't have access to update this credential.";
		//		return false;
		//	}
		//	if ( entity.ActingAgentId == 0 )
		//	{
		//		messages.Add( "Error - you must select an organization/agent.");
				
		//	}
		//	if ( entity.AgentRole == null || entity.AgentRole.Items.Count == 0 )
		//	{
		//		messages.Add( "Error - you must select one or more roles .");
		//	}
		//	if ( messages.Count > 0 )
		//	{
		//		status += string.Join( ",", messages.ToArray() );
		//		return false;
		//	}
		//	OrganizationRoleManager mgr = new OrganizationRoleManager();
		//	try
		//	{
		//		int count = 0;
		//		//temp testing if roles don't exist
		//		//if ( entity.RoleType == null || entity.RoleType.Items.Count == 0 )
		//		//{
		//		//	entity.RoleType = new MC.Enumeration();
		//		//	entity.RoleType.Items = new List<MC.EnumeratedItem>();
		//		//	entity.RoleType.Items.Add( new MC.EnumeratedItem() { Id = 1, CodeId = 1 } );
		//		//}

		//		if ( mgr.Credential_UpdateOrganizationRoleProfile( entity, credentialId, user.Id, ref messages ) == false )
		//		{
		//			valid = false;
		//			status += string.Join( ",", messages.ToArray() );
		//		}
		//		else 
		//		{
		//			status = "Successfully Added Organization Role(s) ";
		//			activityMgr.AddActivity( "CredentialOrgRole", "Modify", string.Format( "{0} added/updated credential to org role(s) Credential: {1} (id: {2})", user.FullName(), cred.Name, credentialId ), user.Id, 0, credentialId );
		//		}

		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".Credential_SaveOrgRole" );
		//		valid = false;
		//		status = ex.Message;
		//	}
		//	return valid;
		//}

		//public bool Credential_DeleteOrgRoles( int credentialId, Guid agentUid, AppUser user, ref string status )
		//{
		//	bool valid = true;
		//	//check if user can edit the entity
		//	MC.Credential entity = new MC.Credential();
		//	if ( CanUserUpdateCredential( credentialId, user, ref status, ref entity ) == false ) 
		//	{
		//		status = "Error - you don't have access to update this credential.";
		//		return false;
		//	}

		//	try
		//	{
		//		OrganizationRoleManager mgr = new OrganizationRoleManager();
		//		if ( mgr.CredentialOrgRole_Delete( credentialId, agentUid, ref status ) )
		//		{
		//			//if valid, log
		//			//activityMgr.AddActivity( "Cost Profile Item", "Delete", string.Format( "{0} deleted Cost Profile Item {1} ({2}) from Credential: {3}", user.FullName(), profile.ProfileName, profileId, profile.ParentId ), user.Id, 0, profileId );
		//			status = "";
		//		}
		//		else
		//		{
		//			status = "Error - delete failed: " + status;
		//			return false;
		//		}
			

		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".Credential_DeleteOrgRoles" );
		//		status = ex.Message;
		//		valid = false;
		//	}

		//	return valid;

		//}
		#endregion

		#region Single QA Role Profiles - OBSOLETE
		//public static QualityAssuranceActionProfile GetCredentialQARole( int credentialId, int profileId )
		//{
		//	return OrganizationRoleManager.QualityAssuranceActionProfile_Get( profileId );
		//}
		//public bool Credential_SaveQAOrgRole( QualityAssuranceActionProfile entity, int credentialId, AppUser user, ref string status )
		//{
		//	bool valid = true;
		//	status = "";
		//	List<string> messages = new List<string>();
		//	if ( credentialId == 0 )
		//	{
		//		messages.Add( "Error - Invalid context, please start by editing a credential." );
		//		return false;

		//	}
		//	//check if user can edit the entity
		//	MC.Credential cred = new MC.Credential();
		//	if ( CanUserUpdateCredential( credentialId, user, ref status, ref cred ) == false ) 
		//	{
		//		status = "Error - you don't have access to update this credential.";
		//		return false;
		//	}

		//	if ( entity.ActingAgentId == 0 )
		//	{
		//		messages.Add( "Error - you must select an organization/agent." );

		//	}
		//	if ( entity.IssuedCredentialId == 0 )
		//	{
		//		messages.Add( "Error - you must select an Issued Credential." );

		//	}
		//	if ( entity.RoleTypeId == 0 && (entity.AgentRole == null || entity.AgentRole.Items.Count == 0) )
		//	{
		//		messages.Add( "Error - you must select one or more roles ." );
		//	}
		//	if ( messages.Count > 0 )
		//	{
		//		status += string.Join( ",", messages.ToArray() );
		//		return false;
		//	}
		//	OrganizationRoleManager mgr = new OrganizationRoleManager();
		//	try
		//	{
		//		entity.ParentId = credentialId;
		//		if ( mgr.Credential_SaveQAActions( entity, user.Id, ref messages ) == false )
		//		{
		//			valid = false;
		//			status += string.Join( ",", messages.ToArray() );
		//		}
		//		else
		//		{
		//			status = "Successfully saved Quality Assurance Action ";
		//			activityMgr.AddActivity( "Credential Quality Assurance Action", "Modify", string.Format( "{0} saved Quality Assurance Action for credential: {1}", user.FullName(), credentialId ), user.Id, 0, credentialId );
		//		}

		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".Credential_SaveOrgRole" );
		//		valid = false;
		//		status = ex.Message;
		//	}
		//	return valid;
		//}

		//public bool Credential_DeleteQAOrgRoles( int credentialId, int profileId, AppUser user, ref string status )
		//{
		//	bool valid = true;
		//	//check if user can edit the entity
		//	MC.Credential entity = new MC.Credential();
		//	if ( CanUserUpdateCredential( credentialId, user, ref status, ref entity ) == false ) 
		//	{
		//		status = "Error - you don't have access to update this credential.";
		//		return false;
		//	}

		//	try
		//	{
		//		OrganizationRoleManager mgr = new OrganizationRoleManager();
		//		//need specific QA delete
		//		if ( mgr.Credential_AgentRelationship_Delete( profileId, ref status ) )
		//		{
		//			//if valid, log
		//			//activityMgr.AddActivity( "Cost Profile Item", "Delete", string.Format( "{0} deleted Cost Profile Item {1} ({2}) from Credential: {3}", user.FullName(), profile.ProfileName, profileId, profile.ParentId ), user.Id, 0, profileId );
		//			status = "";
		//		}
		//		else
		//		{
		//			status = "Error - delete failed: " + status;
		//			return false;
		//		}


		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".Credential_DeleteOrgRoles" );
		//		status = ex.Message;
		//		valid = false;
		//	}

		//	return valid;

		//}
		#endregion

		#region Duration Profiles
		/// <summary>
		/// Get all Duration profiles for a parent
		/// </summary>
		/// <param name="parentId"></param>
		/// <returns></returns>
		public static List<DurationProfile> DurationProfile_GetAll( Guid parentId )
		{
			List<DurationProfile> list = DurationProfileManager.GetAll( parentId );
			return list;
		}

		/// <summary>
		/// Get all Duration Profiles for credential from Credential.TimeToEarn
		/// NOTE: the base Credential is still using Credential_TimeToEarn, rather than Entity.DurationProfile. So use this method until converted. 
		/// Again for duration on Credential ONLY
		/// </summary>
		/// <param name="credentialId"></param>
		/// <returns></returns>
		//public static List<DurationProfile> DurationProfile_GetAllTimeToEarn( int credentialId )
		//{
		//	List<DurationProfile> list = CredentialTimeToEarnManager.DurationProfile_GetAll( credentialId );
		//	return list;
		//}
		/// <summary>
		/// Get a Duration Profile By integer Id from Credential.TimeToEarn
		/// NOTE: the base Credential is still using Credential_TimeToEarn, rather than Entity.DurationProfile. So use this method until converted. 
		/// Again for duration on Credential ONLY
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		//public static DurationProfile DurationProfile_GetTimeToEarn( int id )
		//{
		//	DurationProfile profile = CredentialTimeToEarnManager.Get( id );
		//	return profile;
		//}

		/// <summary>
		/// Get a Duration Profile By integer Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static DurationProfile DurationProfile_Get( int id )
		{
			DurationProfile profile = DurationProfileManager.Get( id );
			return profile;
		}
		//public int DurationProfile_Add( DurationProfile entity, Guid parentUid, Guid credentialUid, int userId, ref string statusMessage )
		//{
		//	int newId = 0;
		//	List<String> messages = new List<string>();
		//	if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
		//	{
		//		statusMessage = "Error - missing an identifier for the DurationProfile" ;
		//		return 0;
		//	}
		//	//validate credential and access
		//	//MC.Credential credential = GetCredential()
		//	//if (CanUserUpdateCredential( credentialUid, userId, ref statusMessage ) == false) 
		//	//{
		//	//	messages.Add( "Error - missing credential identifier" );
		//	//	return false;
		//	//}
		//	//CanUser update entity?
		//	MC.Entity e = EntityManager.GetEntity( parentUid );

		//	//remove this if properly passed from client
		//	//plus need to migrate to the use of EntityId
		//	entity.ParentUid = parentUid;
		//	entity.EntityId = e.Id;
		//	entity.CreatedById = entity.LastUpdatedById = userId;

		//	bool isValid = new DurationProfileManager().DurationProfileUpdate( entity, e.EntityTypeId, ref messages );
		//	if ( isValid )
		//	{
		//		newId = entity.Id;
		//	}
		//	else
		//	{
		//		statusMessage = string.Join( ",", messages.ToArray() );
		//	}
		//	return newId;

		//}

		public bool DurationProfile_Update( DurationProfile entity, Guid parentUid, Guid credentialUid, int userId, ref string statusMessage )
		{
			List<String> messages = new List<string>();
			if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
			{
				messages.Add( "Error - missing an identifier for the DurationProfile" );
				return false;
			}
			//validate credential and access
			//MC.Credential credential = GetCredential()
			//if (CanUserUpdateCredential( credentialUid, userId, ref statusMessage ) == false) 
		//{
			//	messages.Add( "Error - missing credential identifier" );
			//	return false;
			//}
			//CanUser update entity?
			MC.Entity e = EntityManager.GetEntity( parentUid );

			//remove this if properly passed from client
			//plus need to migrate to the use of EntityId
			entity.ParentUid = parentUid;
			entity.EntityId = e.Id;
			entity.CreatedById = entity.LastUpdatedById = userId;

			//if an add, the new id will be returned in the entity
			bool isValid = new DurationProfileManager().DurationProfileUpdate( entity, e.EntityTypeId, ref messages );
			statusMessage = string.Join( ",", messages.ToArray() );
			return isValid;

		}

		public bool DurationProfile_Delete( int profileID, ref string status )
		{
			bool valid = false;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user == null || user.Id == 0 )
			{
				status = "You must be logged and authorized to perform this action.";
				return false;
			}
			try
			{
				DurationProfile profile = DurationProfileManager.Get( profileID );
				//ensure has access

				valid = new DurationProfileManager().DurationProfile_Delete( profileID, ref status );
				if ( valid )
				{
					//if valid, status contains the cred name and id
					activityMgr.AddActivity( "DurationProfile", "Delete", string.Format( "{0} deleted {1}", user.FullName(), status ), user.Id, 0, profileID );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".DurationProfile_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		//public bool DurationProfile_AddTimeToEarn( DurationProfile entity, Guid credentialUid, int userId, ref string statusMessage )
		//{
		//	bool isValid = true;
		//	List<String> messages = new List<string>();
		//	if ( entity == null || !BaseFactory.IsGuidValid( credentialUid ) )
		//	{
		//		messages.Add( "Error - missing an identifier for the DurationProfile" );
		//		return false;
		//	}
		//	//validate credential and access
		//	MC.Credential credential = GetBasicCredential(credentialUid);
		//	//MC.Credential credential = GetCredential()
		//	//if (CanUserUpdateCredential( credentialUid, userId, ref statusMessage ) == false) 
		//	//{
		//	//	messages.Add( "Error - missing credential identifier" );
		//	//	return false;
		//	//}
		//	//CanUser
		//	//MC.Entity e = EntityManager.GetEntity( credentialUid );

		//	//remove this if properly passed from client
		//	//plus need to migrate to the use of EntityId
		//	entity.ParentUid = credentialUid;
		//	entity.CredentialId = credential.Id;
		//	//N/A here entity.EntityId = e.Id;
		//	entity.CreatedById = entity.LastUpdatedById = userId;

		//	isValid = new CredentialTimeToEarnManager().TimeToEarn_Update( entity, credential.Id, ref statusMessage );

		//	//bool isValid = new DurationProfileManager().DurationProfileUpdate( entity, e.EntityTypeId, ref messages );

		//	return isValid;

		//}
		//public bool DurationProfile_UpdateTimeToEarn( DurationProfile entity, Guid credentialUid, int userId, ref string statusMessage )
		//{
		//	bool isValid = true;
		//	List<String> messages = new List<string>();
		//	if ( entity == null || !BaseFactory.IsGuidValid( credentialUid ) )
		//	{
		//		messages.Add( "Error - missing an identifier for the DurationProfile" );
		//		return false;
		//	}
		//	//validate credential and access
		//	MC.Credential credential = GetBasicCredential( credentialUid );
		//	//MC.Credential credential = GetCredential()
		//	//if (CanUserUpdateCredential( credentialUid, userId, ref statusMessage ) == false) 
		//	//{
		//	//	messages.Add( "Error - missing credential identifier" );
		//	//	return false;
		//	//}
		//	//CanUser
		//	//MC.Entity e = EntityManager.GetEntity( credentialUid );

		//	//remove this if properly passed from client
		//	//plus need to migrate to the use of EntityId
		//	entity.ParentUid = credentialUid;
		//	entity.CredentialId = credential.Id;
		//	//N/A here entity.EntityId = e.Id;
		//	entity.CreatedById = entity.LastUpdatedById = userId;

		//	//if an add, the new id will be returned in the entity
		//	isValid = new CredentialTimeToEarnManager().TimeToEarn_Update( entity, credential.Id, ref statusMessage );

		//	return isValid;

		//}


		//public bool DurationProfile_DeleteTimeToEarn( int profileID, ref string status )
		//{
		//	bool isValid = false;
		//	AppUser user = AccountServices.GetCurrentUser();
		//	if ( user == null || user.Id == 0 )
		//	{
		//		status = "You must be logged and authorized to perform this action.";
		//		return false;
		//	}
		//	try
		//	{
		//		//ensure has access

		//		isValid = new CredentialTimeToEarnManager().Credential_TimeToEarnDelete( profileID, ref status );
		//		if ( isValid )
		//		{
		//			//if isValid, status contains the cred name and id
		//			activityMgr.AddActivity( "DurationProfile", "Delete", string.Format( "{0} deleted CredentialTimeToEarn {1}", user.FullName(), status ), user.Id, 0, profileID );
		//			status = "";
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".DurationProfile_DeleteTimeToEarn" );
		//		status = ex.Message;
		//		isValid = false;
		//	}

		//	return isValid;
		//}
		#endregion

		#region Credential Condition Profiles
		/// <summary>
		/// Get a full ConditionProfile for editor (usually, so forEditView is true)
		/// </summary>
		/// <param name="profileId"></param>
		/// <param name="includeProperties"></param>
		/// <param name="incudingResources"></param>
		/// <returns></returns>
		public static ConditionProfile ConditionProfile_GetForEdit( int profileId,
				bool forEditView = true )
		{
			bool includeProperties = true;
			bool incudingResources = true;
			if ( forEditView == false )
			{
				includeProperties = false;
				incudingResources = false;
			}
			ConditionProfile profile = ConnectionProfileManager.ConditionProfile_Get( profileId, includeProperties, incudingResources, forEditView );

			return profile;
		}
		/// <summary>
		/// Get a minimal ConditionProfile
		/// </summary>
		/// <param name="profileId"></param>
		/// <returns></returns>
		public static ConditionProfile ConditionProfile_GetBasic( int profileId )
		{
			ConditionProfile profile = ConnectionProfileManager.ConditionProfile_Get( profileId, false, false, false );

			return profile;
		}
		/// <summary>
		/// Get Condition Profile as a ProfileLink
		/// </summary>
		/// <param name="profileId"></param>
		/// <returns></returns>
		//public static MN.ProfileLink ConditionProfile_GetProfileLink( int profileId )
		//{
			
		//	MN.ProfileLink entity = ConnectionProfileManager.ConditionProfile_GetProfileLink( profileId );
		
		//	return entity;
		//}

		//private bool Credential_UpdateConditionProfiles( MC.Credential entity, string type,  AppUser user, ref string status )
		//{
		//	bool valid = true;
		//	status = "";
		//	List<string> messages = new List<string>();
		//	ConnectionProfileManager mgr = new ConnectionProfileManager();
		//	try
		//	{
		//		int count = 0;
		//		entity.LastUpdatedById = user.Id;
		//		//if handling all from a list ==> prefer to NOT do this - one at a time?
		//		//also assume delete will be immediate, and so only adds are done? Actually could change the role at any time!
		//		if ( mgr.Credential_UpdateCondition( entity, type, ref messages, ref count ) == false )
		//		{
		//			valid = false;
		//		}
		//		else if ( count > 0 )
		//		{
		//			status = "Successful";
		//			ConsoleMessageHelper.SetConsoleInfoMessage( "Successfully Added Credential - Organization Role(s)" );
		//			activityMgr.AddActivity( "CredentialOrgRole", "Modify", string.Format( "{0} added/updated credential to org role(s): {1}, count:{2}", user.FullName(), entity.Name, count ), user.Id, 0, entity.Id );
		//		}

				
		//		status = string.Join( ",", messages.ToArray() );
				
		//		//single updates
		//		//bool roleUpdates = new OrganizationRoleManager().CredentialOrgRole_Add( credentialId, orgId, roleId, userId, ref string status )
		//		//need to check for a count!!!!
		//		if ( valid )
		//		{
		//			if ( count == 0 )
		//			{
		//				//nothing to update
		//				ConsoleMessageHelper.SetConsoleInfoMessage( "No Action - There were no role(s) to update" );
		//			}
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".Credential_UpdateConditionProfiles" );
		//		valid = false;
		//		status = ex.Message;
		//	}
		//	return valid;
		//}

		public bool ConditionProfile_Save( ConditionProfile entity, Guid credentialUid, string type, string action, AppUser user, ref string status, bool isQuickCreate = false )
		{
			bool valid = true;
			status = "";
			
			entity.IsNewVersion = true;

			List<string> messages = new List<string>();
			ConnectionProfileManager mgr = new ConnectionProfileManager();
			MC.Credential credential = GetBasicCredential( credentialUid );
			try
			{
				entity.ParentId = credential.Id;
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				if ( mgr.HandleProfile( credential, entity, type, ref messages ) == false )
				{
					valid = false;
				}
				else 
				{
					if ( isQuickCreate )
					{
						status = "Created an initial Profile. Please provide a meaningful name, and fill out the remainder of the profile";
					}
					else
					{
						status = "Successfully Saved Credential - Condition Profile";
						activityMgr.AddActivity( "Condition Profile", action, string.Format( "{0} added/updated credential connection profile: {1}", user.FullName(), entity.ProfileName ), user.Id, 0, entity.Id );
					}

					RemoveCredentialFromCache( credential.Id );
				}


				status = string.Join( ",", messages.ToArray() );

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".ConditionProfile_Save" );
				valid = false;
				status = ex.Message;
			}
			return valid;
		}

		/// <summary>
		/// Delete a condition profile
		/// First ensure user has edit access
		/// </summary>
		/// <param name="conditionProfileId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool ConditionProfile_Delete( int credentialId, int conditionProfileId, AppUser user, ref string status )
		{
			bool valid = true;
			ConnectionProfileManager mgr = new ConnectionProfileManager();
			try
			{
				//get profile and ensure user has access
				ConditionProfile profile = ConditionProfile_GetBasic( conditionProfileId );
				if ( profile == null || profile.Id == 0 )
				{
					status = "Error - the requested profile was not found.";
					return false;
				}
				//ensure profile is part of provided credential
				if ( profile.ParentId != credentialId )
				{
					status = "Error - you don't have delete access to this profile.";
					return false;
				}
				MC.Credential entity = new MC.Credential();
				if ( CanUserUpdateCredential( profile.ParentId, user, ref status, ref entity ) )
				{
					if ( mgr.ConditionProfile_Delete( conditionProfileId, ref status ) )
					{
						//if valid, log
						activityMgr.AddActivity( "Condition Profile", "Delete", string.Format( "{0} deleted Condition Profile {1} ({2}) from Credential: {3}", user.FullName(), profile.ProfileName, conditionProfileId, profile.ParentId ), user.Id, 0, conditionProfileId );
						status = "";
						RemoveCredentialFromCache( credentialId );
					}
					else
					{
						status = "Error - delete failed: " + status;
						return false;
					}
				}
				else
				{
					//reject and log
					status = "Error - you don't have access to update this credential.";
					return false;
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

		public int ConditionProfile_AddAsmt( int ConditionProfileId, int assessmentId, AppUser user, ref bool valid, ref string status )
		{
			int id = 0;
			//ConditionProfile_PartsManager mgr = new ConditionProfile_PartsManager();
			try
			{
				//id = mgr.Assessment_Add( ConditionProfileId, assessmentId, user.Id, ref messages );

				id = new Entity_AssessmentManager().EntityAssessment_Add( ConditionProfileId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, assessmentId, user.Id, ref messages );

				if ( id > 0 )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "Condition Profile Assessment", "Add item", string.Format( "{0} added Assessment {1} from Condition Profile  {2}", user.FullName(), assessmentId, ConditionProfileId ), user.Id, 0, assessmentId );
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
				LoggingHelper.LogError( ex, thisClassName + ".ConditionProfile_AddAsmt" );
				status = ex.Message;
				valid = false;
			}

			return id;
		}

		public bool ConditionProfile_DeleteAsmt( int conditionProfileId, int assessmentId, AppUser user, ref string status )
		{
			bool valid = true;

			//ConditionProfile_PartsManager mgr = new ConditionProfile_PartsManager();
			Entity_AssessmentManager mgr = new Entity_AssessmentManager();
			try
			{
				valid = mgr.EntityAssessment_Delete( conditionProfileId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, assessmentId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "Condition Profile Assessment", "Delete item", string.Format( "{0} deleted Assessment {1} from Condition Profile  {2}", user.FullName(), assessmentId, conditionProfileId ), user.Id, 0, assessmentId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".ConditionProfile_DeleteAsmt" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}


		public int ConditionProfile_AddLearningOpportunity( int conditionProfileId, int recordId, AppUser user, ref bool valid, ref string status )
		{
			int id = 0;
			Entity_LearningOpportunityManager mgr = new Entity_LearningOpportunityManager();
			try
			{
				//id = mgr.ConditionLearningOpp_Add( connectionProfileId, recordId, user.Id, ref messages );

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
				LoggingHelper.LogError( ex, thisClassName + ".ConditionProfile_AddLearningOpportunity" );
				status = ex.Message;
				valid = false;
			}

			return id;
		}

		public bool ConditionProfile_DeleteLearningOpportunity( int conditionProfileId, int recordId, AppUser user, ref string status )
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


		#region Task Profile
		public static TaskProfile ConditionProfile_GetTask( int profileId )
		{
			TaskProfile profile = Entity_TaskProfileManager.TaskProfile_Get( profileId);

			return profile;
		}
		public bool TaskProfile_Save( TaskProfile entity, Guid parentUid, string action, AppUser user, ref string status, bool isQuickCreate = false )
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
				MC.Entity e = EntityManager.GetEntity( parentUid );
				//remove this if properly passed from client
				//plus need to migrate to the use of EntityId
				entity.ParentId = e.Id;
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				if ( isQuickCreate )
				{
					//?may not be necessary
				}
				entity.IsNewVersion = true;

				if ( new Entity_TaskProfileManager().Update( entity, parentUid, user.Id, ref messages ) )
				{
					if ( isQuickCreate )
					{
						status = "Created an initial Task Profile. Please provide a meaningful name, and fill out the remainder of the profile";
						//test concept
						return true; //false;
					}
					else
					{
						//if valid, status contains the cred id, category, and codeId
						status = "Successfully Saved Credential - Task Profile";
						activityMgr.AddActivity( "Task Profile", action, string.Format( "{0} added/updated credential connection task profile: {1}", user.FullName(), entity.ProfileName ), user.Id, 0, entity.Id );
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

		public bool ConditionProfile_DeleteTask( int conditionProfileId, int profileId, AppUser user, ref string status )
		{
			bool valid = true;

			Entity_TaskProfileManager mgr = new Entity_TaskProfileManager();
			try
			{
				//get first to validate (soon)
				TaskProfile entity = ConditionProfile_GetTask( profileId );
				//to do match to the conditionProfileId

				valid = mgr.TaskProfile_Delete( profileId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "Condition Profile", "Delete Task", string.Format( "{0} deleted Task Profile {1} from Condition Profile  {2}", user.FullName(), profileId, conditionProfileId ), user.Id, 0, profileId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".ConditionProfile_DeleteTask" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		#endregion


		#endregion

		#region Credential Revocation Profile
		public static RevocationProfile RevocationProfile_GetForEdit( int profileId,
				bool forEditView = true )
		{
			
			RevocationProfile profile = Entity_RevocationProfileManager.Get( profileId );

			return profile;
		}
		//private bool RevocationProfile_Update( MC.Credential entity, string type, AppUser user, ref string status )
		//{
		//	bool valid = true;
		//	status = "";
		//	List<string> messages = new List<string>();
		//	Entity_RevocationProfileManager mgr = new Entity_RevocationProfileManager();
		//	try
		//	{
		//		int count = 0;
		//		//if handling all from a list ==> prefer to NOT do this - one at a time?
		//		//also assume delete will be immediate, and so only adds are done? Actually could change the role at any time!
		//		if ( mgr.Update( entity.Revocation, entity.Id, user.Id, ref messages ) == false )
		//		{
		//			valid = false;
		//		}
		//		else if ( count > 0 )
		//		{
		//			status = "Successful";

		//			activityMgr.AddActivity( "RevocationProfile", "Modify", string.Format( "{0} added/updated Revocation Profiles under credential: {1}, count:{2}", user.FullName(), entity.Name, count ), user.Id, 0, entity.Id );
		//		}

		//		status = string.Join( ",", messages.ToArray() );

		//		//need to check for a count!!!!
		//		if ( valid )
		//		{
		//			if ( count == 0 )
		//			{
		//				//nothing to update - this message is not display if called via ajax
		//				//ConsoleMessageHelper.SetConsoleInfoMessage( "No Action - There were no role(s) to update" );
		//			}
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".RevocationProfile_Update" );
		//		valid = false;
		//		status = ex.Message;
		//	}
		//	return valid;
		//}

		public bool RevocationProfile_Save( RevocationProfile entity, Guid credentialUid, string action, AppUser user, ref string status, bool isQuickCreate = false )
		{
			bool valid = true;
			status = "";
			List<string> messages = new List<string>();
			Entity_RevocationProfileManager mgr = new Entity_RevocationProfileManager();
			try
			{
				MC.Credential credential = GetBasicCredential( credentialUid );

				int count = 0;
				entity.IsNewVersion = true;
				if ( mgr.Update( entity, credential, user.Id, ref messages ) == false )
				{
					valid = false;
					status = string.Join( ",", messages.ToArray() );
				}
				else 
				{
					if ( isQuickCreate )
					{
						status = "Created an initial Profile. Please provide a meaningful name, and fill out the remainder of the profile";
					}
					else
					{
						status = "Successful";
						activityMgr.AddActivity( "RevocationProfile", "Modify", string.Format( "{0} added/updated Revocation Profiles under credential: {1}, count:{2}", user.FullName(), credential.Name, count ), user.Id, 0, entity.Id );
					}
				}

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".RevocationProfile_Save" );
				valid = false;
				status = ex.Message;
			}
			return valid;
		}
		/// <summary>
		/// Delete a revocation Profile ??????????????
		/// TODO - ensure current user has access to the credential
		/// </summary>
		/// <param name="credenialId"></param>
		/// <param name="profileId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool RevocationProfile_Delete( int credentialId, int profileId, AppUser user, ref string status )
		{
			bool valid = true;

			try
			{
				valid = new Entity_RevocationProfileManager().Delete( profileId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "Revocation Profile", "Delete", string.Format( "{0} deleted Revocation Profile: {1} from Credential: {2}", user.FullName(), profileId, credentialId ), user.Id, 0, profileId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".RevocationProfile_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		#endregion

		#region Credential FrameworkItems OBSOLETE ==> CONVERTED TO ENTITY.FrameworkItems
		//public MC.EnumeratedItem FrameworkItem_Add( int credentialId, int categoryId, int codeID, string searchType, ref bool valid, ref string status )
		//{
			
		//	AppUser user = AccountServices.GetCurrentUser();
		//	if ( user == null || user.Id == 0 )
		//	{
		//		valid = false;
		//		status = "Error - you must be authenticated in order to update data";
		//		return new MC.EnumeratedItem();
		//	}
		//	return FrameworkItem_Add( credentialId, categoryId, codeID, user, ref valid, ref status );
		//}
		//public MC.EnumeratedItem FrameworkItem_Add( int credentialId, int categoryId, int codeID, AppUser user, ref bool valid, ref string status )
		//{
		//	if ( credentialId == 0 || categoryId == 0 || codeID == 0 )
		//	{
		//		valid = false;
		//		status = "Error - invalid request - missing code identifiers";
		//		return new MC.EnumeratedItem();
		//	}

		//	CredentialFrameworkItemManager mgr = new CredentialFrameworkItemManager();
		//	int credentialFrameworkItemId = 0;
		//	MC.EnumeratedItem item = new MC.EnumeratedItem();
		//	try
		//	{
		//		credentialFrameworkItemId = mgr.ItemAdd( credentialId, categoryId, codeID, user.Id, ref status );

		//		if ( credentialFrameworkItemId > 0 )
		//		{
		//			//get full item, as a codeItem to return
		//			item = CredentialFrameworkItemManager.ItemGet( credentialFrameworkItemId );
		//			//if valid, status contains the cred id, category, and codeId
		//			activityMgr.AddActivity( "Credential FrameworkItem", "Add item", string.Format( "{0} added credential FrameworkItem. CredentialId: {1}, categoryId: {2}, codeId: {3}, summary: {4}", user.FullName(), credentialId, categoryId, codeID, item.ItemSummary ), user.Id, 0, credentialFrameworkItemId );
		//			status = "";
		//		}
		//		else
		//		{
		//			valid = false;
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".FrameworkItem_Add" );
		//		status = ex.Message;
		//		valid = false;
		//	}

		//	return item;
		//}

		//public bool FrameworkItem_Delete( int credentialFrameworkItemId, AppUser user, ref bool valid, ref string status )
		//{
		//	CredentialFrameworkItemManager mgr = new CredentialFrameworkItemManager();

		//	try
		//	{
		//		valid = mgr.ItemDelete( credentialFrameworkItemId, ref status );
				
		//		if ( valid )
		//		{
		//			//if valid, status contains the cred id, category, and codeId
		//			activityMgr.AddActivity( "Credential FrameworkItem", "Delete item", string.Format( "{0} deleted credential FrameworkItem {1}", user.FullName(), status ), user.Id, 0, credentialFrameworkItemId );
		//			status = "";
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".FrameworkItem_Delete" );
		//		status = ex.Message;
		//		valid = false;
		//	}

		//	return valid;
		//}
		#endregion
	}

	public class CachedCredential
	{
		public CachedCredential()
		{
			lastUpdated = DateTime.Now;
		}
		public DateTime lastUpdated { get; set; }
		public MC.Credential Item { get; set; }

	}
}
