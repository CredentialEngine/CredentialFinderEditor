using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		public static List<string> AutocompleteCompetencies( string keyword, int maxTerms = 25 )
		{
			int userId = 0;
			string where = "";
			int pTotalRows = 0;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;
			SetAuthorizationFilter( user, ref where );

			SetCompetenciesAutocompleteFilter( keyword, ref where );

			return CredentialManager.Autocomplete( where, 1, maxTerms, userId, ref pTotalRows );
		}
		//public static List<string> Autocomplete_Subjects( string keyword, int maxTerms = 25 )
		//{
		//	//tough to do the user specific stuff

		//	//int userId = 0;
		//	//string where = "";
		//	//int pTotalRows = 0;
		//	//AppUser user = AccountServices.GetCurrentUser();
		//	//if ( user != null && user.Id > 0 )
		//	//	userId = user.Id;
		//	//SetAuthorizationFilter( user, ref where );

		//	List<string> list = 
		//	Entity_ReferenceManager.QuickSearch_TextValue(1, 34, keyword, maxTerms );

		//	//return CredentialManager.Autocomplete( where, 1, maxTerms, userId, ref pTotalRows );

		//	return list;
		//}
		////public static List<MC.CredentialSummary> QuickSearch( string keyword = "" )
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
			string filter = " (  IsAQACredential = 1 ) ";
			//string filter = "";

			if ( filter.Length > 0 )
				AND = " AND ";
			if ( !string.IsNullOrWhiteSpace( keywords ) )
			{
				keywords = ServiceHelper.HandleApostrophes( keywords );
				if ( keywords.IndexOf( "%" ) == -1 )
					keywords = "%" + keywords.Trim() + "%";
				filter = filter + AND + string.Format( " (base.name like '{0}' OR base.Description like '{0}'  OR base.Url like '{0}' OR CreatorOrgs like '{0}'  OR OwningOrgs like '{0}')", keywords );
			}
			if ( orgId > 0 )
			{
				if ( filter.Length > 0 )
					AND = " AND ";

				filter = filter + AND + string.Format( " (base.OrgId = {0} )", orgId );
			}
			return CredentialManager.Search( filter, pOrderBy, pageNumber, pageSize, ref pTotalRows, userId );

		}
	
		/// <summary>
		/// Used for limited custom searches
		/// </summary>
		/// <param name="keywords"></param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSize"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
		public static List<MC.CredentialSummary> Search( string keywords, int pageNumber, int pageSize, ref int pTotalRows )
		{
			string pOrderBy = "";
			string filter = "";
			int userId = 0;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;

			SetKeywordFilter( keywords, false, ref filter );
			SetAuthorizationFilter( user, ref filter );
			//if ( !string.IsNullOrWhiteSpace( keywords )) 
			//{
			//	keywords = ServiceHelper.HandleApostrophes( keywords );
			//	if ( keywords.IndexOf( "%" ) == -1 )
			//		keywords = "%" + keywords.Trim() + "%";
			//	//OR base.Url like '{0}' 
			//	filter = string.Format( " (base.name like '{0}' OR base.Description like '{0}'  OR CreatorOrgs like '{0}' OR OwningOrgs like '{0}')", keywords );
			//}
			return CredentialManager.Search( filter, pOrderBy, pageNumber, pageSize, ref pTotalRows, userId );
		}

		/// <summary>
		/// Full credentials search
		/// </summary>
		/// <param name="data"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
		public static List<MC.CredentialSummary> Search( MainSearchInput data, ref int pTotalRows )
		{
			string where = "";
			DateTime start = DateTime.Now;
			//Stopwatch stopwatch = new Stopwatch();
			//stopwatch.Start();
			LoggingHelper.DoTrace( 6, string.Format( "===CredentialServices.Search === Started: {0}", start ) );
			int userId = 0;
			List<string> competencies = new List<string>();

			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;

			SetKeywordFilter( data.Keywords, false, ref where );
			where = where.Replace( "[USERID]", user.Id.ToString() );

			SearchServices.SetSubjectsFilter( data, CodesManager.ENTITY_TYPE_CREDENTIAL, ref where );

			SetAuthorizationFilter( user, ref where );

			SearchServices.HandleCustomFilters( data, 58, ref where );

			//Should probably move this to its own method?
			string agentRoleTemplate = " ( id in (SELECT [CredentialId] FROM [dbo].[CredentialAgentRelationships_Summary] where RelationshipTypeId = {0} and OrgId = {1})) ";
			int roleId = 0;
			int orgId = 0;
			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";

			//Updated to use FilterV2
			foreach( var filter in data.FiltersV2.Where(m => m.Name == "qualityAssuranceBy" ).ToList() )
			{
				roleId = filter.GetValueOrDefault( "RoleId", 0 );
				orgId = filter.GetValueOrDefault( "AgentId", 0 );
				where = where + AND + string.Format( agentRoleTemplate, roleId, orgId );
				AND = " AND ";
			}

			/* //Retained for reference
			foreach ( MainSearchFilter filter in data.Filters.Where( s => s.Name == "qualityAssuranceBy" ) )
			{
				if ( filter.Data.ContainsKey( "RoleId" ) )
					roleId = (int)filter.Data[ "RoleId" ];
				if ( filter.Data.ContainsKey( "AgentId" ) )
					orgId = ( int ) filter.Data[ "AgentId" ];
				where = where + AND + string.Format( agentRoleTemplate, roleId, orgId );
			}
			*/

			SetPropertiesFilter( data, ref where );

			SearchServices.SetRolesFilter( data, ref where );
			SearchServices.SetBoundariesFilter( data, ref where );
			//need to fix rowId

			//naics, ONET
			SetFrameworksFilter( data, ref where );
			//Competencies
			SetCompetenciesFilter( data, ref where, ref competencies );
			SetCredCategoryFilter( data, ref where ); //Not updated for FiltersV2 - I don't think we're using this anymore - NA 5/11/2017
			SetConnectionsFilter( data, ref where );
			
			TimeSpan timeDifference = start.Subtract( DateTime.Now );
			LoggingHelper.DoTrace( 5, thisClassName + string.Format( ".Search(). Filter: {0}, elapsed: {1} ", where, timeDifference.TotalSeconds ) );
			List<MC.CredentialSummary> list = CredentialManager.Search( where, data.SortOrder, data.StartPage, data.PageSize, ref pTotalRows, userId );

			//stopwatch.Stop();
			timeDifference = start.Subtract( DateTime.Now );
			LoggingHelper.DoTrace( 6, string.Format( "===CredentialServices.Search === Ended: {0}, Elapsed: {1}", DateTime.Now, timeDifference.TotalSeconds ) );
			return list;
		}
	
		private static void SetKeywordFilter(string keywords, bool isBasic, ref string where)
		{
			if ( string.IsNullOrWhiteSpace( keywords ) || string.IsNullOrWhiteSpace( keywords.Trim() ) )
				return;
			//OR CreatorOrgs like '{0}' 
			bool isCustomSearch = false;
			string text = " (base.name like '{0}' OR base.Description like '{0}'  OR base.AlternateName like '{0}' OR OwningOrganization like '{0}'  ) ";
			//for ctid, needs a valid ctid or guid
			if ( keywords.IndexOf("ce-") > -1 && keywords.Length == 39 )
			{
				text = " ( CTID = '{0}' ) ";
				isCustomSearch = true;
			} else if (  ServiceHelper.IsValidGuid( keywords )  )
			{
				text = " ( CTID = 'ce-{0}' ) ";
				isCustomSearch = true;
			}
			else if ( ServiceHelper.IsInteger( keywords ) )
			{
				text = " ( Id = '{0}' ) ";
				isCustomSearch = true;
			}
			else if ( keywords.ToLower() == "[hascredentialregistryid]" )
			{
				text = " ( len(Isnull(CredentialRegistryId,'') ) = 36 ) ";
				isCustomSearch = true;
			}
			else if ( keywords.ToLower() == "[canedit]"  && !isBasic )
			{
				text = string.Format( "((base.StatusId = {0}) OR (base.Id in (SELECT cs.Id FROM [dbo].[Organization.Member] om inner join [Credential_Summary] cs on om.ParentOrgId = cs.ManagingOrgId where userId = {1}) ))", CodesManager.ENTITY_STATUS_PUBLISHED, "[USERID]" );
				isCustomSearch = true;
			}
			//removed 10,11 as part of the frameworkItemSummary
			string keywordsFilter = " OR (base.Id in (SELECT c.id FROM [dbo].[Entity.Reference] a inner join Entity b on a.EntityId = b.Id inner join Credential c on b.EntityUid = c.RowId where [CategoryId] = 35 and a.TextValue like '{0}' )) ";

			string subjects = " OR  (base.EntityUid in (SELECT EntityUid FROM [Entity_Subjects] a where EntityTypeId = 1 AND a.Subject like '{0}' )) ";

			string frameworkItems = " OR (EntityUid in (SELECT EntityUid FROM [dbo].[Entity.FrameworkItemSummary_ForCredentials] a where  a.title like '{0}' ) ) ";
			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";
			//
			keywords = ServiceHelper.HandleApostrophes( keywords );
			if ( keywords.IndexOf( "%" ) == -1 && !isCustomSearch )
			{
				keywords = SearchServices.SearchifyWord( keywords );
				//keywords = "%" + keywords.Trim() + "%";
				//keywords = keywords.Replace( "&", "%" ).Replace( " and ", "%" ).Replace( " in ", "%" ).Replace( " of ", "%" );
				//keywords = keywords.Replace( " - ", "%" );
				//keywords = keywords.Replace( " % ", "%" );
			}

			//skip url  OR base.Url like '{0}' 
			if ( isBasic || isCustomSearch )
				where = where + AND + string.Format( " ( " + text + " ) ", keywords );
			else
				where = where + AND + string.Format( " ( " + text + keywordsFilter + subjects + frameworkItems + " ) ", keywords );
			
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
			string searchCategories = UtilityManager.GetAppKeyValue( "credSearchCategories", "21,37," );
			string template = " ( base.Id in ( SELECT  [EntityBaseId] FROM [dbo].[EntityProperty_Summary] where EntityTypeId= 1 AND [PropertyValueId] in ({0}))) ";
			//.Where( s => s.CategoryId >= 2 && s.CategoryId < 5 ) 

			//Updated to use FiltersV2
			string next = "";
			if ( where.Length > 0 )
				AND = " AND ";
			foreach ( var filter in data.FiltersV2.Where( m => m.Type == MainSearchFilterV2Types.CODE ).ToList() )
			{
				var item = filter.AsCodeItem();
				if ( searchCategories.Contains( item.CategoryId.ToString() ) ) 
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
			foreach ( MainSearchFilter filter in data.Filters)
			{
				if ( searchCategories.IndexOf( filter.CategoryId.ToString() ) > -1 )
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
		private static void SetFrameworksFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			string codeTemplate = "  (base.Id in (SELECT c.id FROM [dbo].[Entity.FrameworkItemSummary] a inner join Entity b on a.EntityId = b.Id inner join Credential c on b.EntityUid = c.RowId where [CategoryId] = {0} and ([CodeGroup] in ({1})  OR ([CodeId] in ({2}) )  )) ) ";

			
			//string codeTemplate1 = "  (base.Id in (SELECT c.id FROM [dbo].[Entity.FrameworkItemSummary] a inner join Entity b on a.EntityId = b.Id inner join Credential c on b.EntityUid = c.RowId where [CategoryId] = {0} and [CodeId] in ({1}))  ) ";

			//Updated to use FiltersV2
			string next = "";
			string groups = "";
			if ( where.Length > 0 )
				AND = " AND ";
			var categoryID = 0;
			foreach( var filter in data.FiltersV2.Where(m => m.Type == MainSearchFilterV2Types.FRAMEWORK ) )
			{
				var item = filter.AsCodeItem();
				var isTopLevel = filter.GetValueOrDefault<bool>( "IsTopLevel", false );
				if( item.CategoryId == 10 || item.CategoryId == 11 )
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
			//just learning opps
			//string template = " ( base.Id in (SELECT distinct  CredentialId FROM [dbo].[ConditionProfile_Competencies_Summary]  where AlignmentType in ('teaches', 'assesses') AND ({0}) ) ) ";
			//learning opps and asmts:
			string template = " ( base.Id in (SELECT distinct  CredentialId FROM [dbo].[ConditionProfile_Competencies_Summary]  where ({0}) ) ) ";
			//
			string phraseTemplate = " ([Name] like '%{0}%' OR [Description] like '%{0}%') ";
			//

			//Updated to use FiltersV2
			string next = "";
			if ( where.Length > 0 )
				AND = " AND ";
			foreach ( var filter in data.FiltersV2.Where(m => m.Name == "competencies" ) )
			{
				var text = filter.AsText();

				//No idea what this is supposed to do
				try
				{
					if ( text.IndexOf( " - " ) > -1 )
					{
						text = text.Substring( text.IndexOf( " -- " ) + 4 );
					}
				}
				catch { }

				competencies.Add( text.Trim() );
				next += OR + string.Format( phraseTemplate, text.Trim() );
				OR = " OR ";

			}
			if ( !string.IsNullOrWhiteSpace( next ) )
			{
				where = where + AND + string.Format( template, next );
			}

			/* //Retained for reference
			foreach ( MainSearchFilter filter in data.Filters.Where( s => s.Name == "competencies" ) )
			{
				string next = "";
				if ( where.Length > 0 )
					AND = " AND ";
				foreach ( string item in filter.Texts )
				{
					keyword = ServiceHelper.HandleApostrophes( item );
					//if ( keyword.IndexOf( "%" ) == -1 )
					//	keyword = "%" + keyword.Trim() + "%";
					if ( keyword.IndexOf( ";" ) > -1 )
					{
						var words = keyword.Split( ';' );
						string nextWord = "";
						foreach ( string word in words )
						{
							nextWord = word;
							if ( nextWord.IndexOf( " - " ) > -1 )
								nextWord = nextWord.Substring( nextWord.IndexOf( " -- " ) + 4 );

							competencies.Add( nextWord.Trim() );
							next += OR + string.Format( phraseTemplate, nextWord.Trim() );
							OR = " OR ";
						}

					}
					else
					{
						if ( keyword.IndexOf( " -- " ) > -1 )
							keyword = keyword.Substring( keyword.IndexOf( " - " ) + 4 );

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
			*/
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

		public static void SetConnectionsFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			string OR = "";
			if ( where.Length > 0 )
				AND = " AND ";

			//Should probably get this from the database
			MC.Enumeration entity = CodesManager.GetCredentialsConditionProfileTypes();

			var validConnections = new List<string>(); 
			//{ 
			//	"requires", 
			//	"recommends", 
			//	"requiredFor", 
			//	"isRecommendedFor", 
			//	//"renewal", //Not a connection type
			//	"isAdvancedStandingFor", 
			//	"advancedStandingFrom", 
			//	"preparationFor", 
			//	"preparationFrom", 
			//	"isPartOf", 
			//	"hasPart"	
			//};
			//validConnections = validConnections.ConvertAll( m => m.ToLower() ); //Makes comparisons easier when combined with the .ToLower() below
			validConnections = entity.Items.Select(s => s.SchemaName.ToLower()).ToList(); 

			string conditionTemplate = " {0}Count > 0 ";

			//Updated for FiltersV2
			string next = "";
			string condition = "";
			if ( where.Length > 0 )
				AND = " AND ";
			foreach ( var filter in data.FiltersV2.Where(m => m.Type == MainSearchFilterV2Types.CODE ) )
			{
				var item = filter.AsCodeItem();
				if( item.CategoryId != 15 )
				{
					continue;
				}

				//Prevent query hijack attacks
				if ( validConnections.Contains( item.SchemaName.ToLower() ) )
				{
					condition = item.SchemaName;
					next += OR + string.Format( conditionTemplate, condition );
					OR = " OR ";
				}
			}
			next = next.Trim();
			next = next.Replace( "ceterms:", "" );
			if ( !string.IsNullOrWhiteSpace( next ) )
			{
				where = where + AND + "(" + next + ")";
			}

			/* //Retained for reference
			foreach ( MainSearchFilter filter in data.Filters.Where( s => s.CategoryId == 15 ) )
			{
				string next = "";
				string condition = "";
				if ( where.Length > 0 )
					AND = " AND ";

				/*foreach ( string item in filter.Items )
				{
					if ( item.Equals( "1" ) )
						condition = "Requires";
					else if ( item.Equals( "2" ) )
						condition = "recommends";
					else if ( item.Equals( "3" ) )
						condition = "requiredFor";
					else if ( item.Equals( "4" ) )
						condition = "isRecommendedFor";
					else if ( item.Equals( "5" ) )
						condition = "renewal";
					else if ( item.Equals( "6" ) )
						condition = "isAdvancedStandingFor";
					else if ( item.Equals( "7" ) )
						condition = "advancedStandingFrom";
					else if ( item.Equals( "8" ) )
						condition = "preparationFor";
					else if ( item.Equals( "9" ) )
						condition = "preparationFrom";
					else if ( item.Equals( "2293" ) )
						condition = "isPartOf";
					else if ( item.Equals( "2294" ) )
						condition = "hasPart";

					next += OR + string.Format( conditionTemplate, condition );
					OR = " OR ";
				}*//*
				foreach ( var item in filter.Schemas )
				{
					//Prevent query hijack attacks
					if ( validConnections.Contains( item.ToLower() ) )
					{
						condition = item;
						next += OR + string.Format( conditionTemplate, condition );
						OR = " OR ";
					}
				}
				next = next.Trim();
				next = next.Replace( "ceterms:", "" );
				where = where + AND + "(" +  next + ")";
			}
			*/
		}
		//private string GetCondtionSchema( string id )
		//{

		//}


		public static List<MC.Entity> CredentialAssetsSearch( int credentialId )
		{
			//SetKeywordFilter( keywords, false, ref filter );

			return CredentialManager.CredentialAssetsSearch( credentialId );
		}
		#endregion

		#region Retrievals
		public static MC.Credential GetForEdit( int id )
		{
			MC.Credential cred = CredentialManager.GetForEdit( id );
			return cred;
		}

		/// <summary>
		/// Get a minimal credential - typically for a link, or need just basic properties
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static MC.Credential GetBasicCredential( int credentialId )
		{
			return CredentialManager.GetBasic( credentialId );
		}
	
		/// <summary>
		/// Get a minimal credential - typically for a link, or need just basic properties
		/// </summary>
		/// <param name="rowId"></param>
		/// <returns></returns>
		public static MC.Credential GetBasicCredentialAsLink( Guid rowId )
		{
			return CredentialManager.GetBasic( rowId, false, true);
		}
		public static MC.Credential GetBasicCredential( Guid rowId )
		{
			return CredentialManager.GetBasic( rowId, false, false );
		}
		/// <summary>
		/// Get a credential for detailed display
		/// </summary>
		/// <param name="id"></param>
		/// <param name="user"></param>
		/// <param name="skippingCache">If true, do not use the cached version</param>
		/// <returns></returns>
		public static MC.Credential GetCredentialDetail( int id, AppUser user, bool skippingCache = false )
		{
			//
			string statusMessage = "";
			int cacheMinutes= UtilityManager.GetAppKeyValue( "credentialCacheMinutes", 0 );
			DateTime maxTime  = DateTime.Now.AddMinutes( cacheMinutes * -1 );

			string key = "credential_" + id.ToString();

			if ( skippingCache ==  false 
				&& HttpRuntime.Cache[ key ] != null && cacheMinutes > 0 )
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
				LoggingHelper.DoTrace( 8, string.Format( "****** CredentialServices.GetCredentialDetail === Retrieving full version of credential, Id: {0}", id ) );
			}

			DateTime start = DateTime.Now;

			CredentialRequest cr = new CredentialRequest();
			cr.IsDetailRequest();

			MC.Credential entity = CredentialManager.GetForDetail( id, cr );
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
		/// Get credential in preparation to publish
		/// </summary>
		/// <param name="id"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static MC.Credential GetCredentialForPublish( int id, AppUser user )
		{
			//NO caching for publishing
			string statusMessage = "";
			
			DateTime start = DateTime.Now;

			//not used

			CredentialRequest cr = new CredentialRequest();
			cr.IsPublishRequest();

			MC.Credential entity = CredentialManager.GetForDetail( id, cr );
			if ( CanUserUpdateCredential( entity, user, ref statusMessage ) )
				entity.CanUserEditEntity = true;

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

			entity = CredentialManager.GetForCompare( id, cr );

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
			MC.Credential entity = GetBasicCredential( credentialUid );

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
		/// Determine if user can view the credential
		/// - if published, all can view
		/// - if private, only staff, select roles, or user from the owning org can view
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static bool CanUserViewCredential( int credentialId, AppUser user, ref MC.Credential entity )
		{
			bool isValid = false;
			if ( credentialId == 0 )
				return false;

			entity = GetBasicCredential( credentialId );
			if ( entity.Id == 0 )
				return false;
			if ( entity.StatusId == CodesManager.ENTITY_STATUS_PUBLISHED )
				return true;

			if ( user == null || user.Id == 0 )
				return false;
			else if ( user.UserRoles.Contains( "Administrator" )
			  || user.UserRoles.Contains( "Site Manager" )
			  || user.UserRoles.Contains( "Site Staff" )
			  || user.UserRoles.Contains( "Site Partner" )
			  || user.UserRoles.Contains( "Site Reader" ) //????
				)
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
		public int Add( MC.Credential entity, AppUser user, ref bool valid, ref string status )
		{
			//, bool isNewVersion = true
			entity.CreatedById = entity.LastUpdatedById = user.Id;

			LoggingHelper.DoTrace( 5, string.Format( thisClassName + ".Credential_Add. Credential: {0}, userId: {1}", entity.Name, entity.CreatedById ) );
			var id = 0;
			List<string> messages = new List<string>();
			Mgr mgr = new Mgr();
			if ( !ValidateCredential( entity, false, ref messages ) )
			{
				status = string.Join( "<br/>", messages.ToArray() );
				valid = false;
				return 0;
			}
			try
			{
				//set by caller, to keep cleaner
				//entity.IsNewVersion = isNewVersion;

				//set the managing orgId
				if ( entity.ManagingOrgId == 0 )
				{
					MC.Organization org = OrganizationManager.GetForSummary( entity.OwningAgentUid );
					entity.ManagingOrgId = org.Id;
					//entity.ManagingOrgId = OrganizationManager.GetPrimaryOrganizationId( user.Id );
				}

				id = mgr.Add( entity, ref status );
				valid = id > 0;
				if ( id > 0 )
				{
					ConsoleMessageHelper.SetConsoleInfoMessage( "Successfully Added Credential" );
					activityMgr.AddActivity( "Credential", "Add", string.Format("{0} added a new credential: {1}", user.FullName(), entity.Name), entity.CreatedById, 0, id );

					string url = UtilityManager.FormatAbsoluteUrl( "/editor/Credential/" + id.ToString(), true );
					//notify administration
					string message = string.Format( "New Credential. <ul><li>Credential Id: {0}</li><li><a href='{4}'>Credential: {1}</a></li><li>Description:</br>{2}</li><li>Created By: {3}</li></ul>", entity.Id, entity.Name, entity.Description, user.FullName(), url );

					Utilities.EmailManager.SendSiteEmail( "New Credential has been created", message );
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
			//entity.IsNewVersion = true;
			return Credential_Update( entity, user, ref status );
		}

		public bool Credential_Update( MC.Credential entity, AppUser user, ref string status )
		{
			entity.LastUpdatedById = user.Id;
			LoggingHelper.DoTrace( 5, string.Format( thisClassName + ".Credential_Update. CredentialId: {0}, userId: {1}", entity.Id, entity.LastUpdatedById ) );
			Mgr mgr = new Mgr();
			bool valid = true;
			if ( !ValidateCredential( entity, false, ref messages ) )
			{
				status = string.Join( "<br/>", messages.ToArray() );
				return false;
			}
			try
			{
				if ( entity.ManagingOrgId == 0 )
					entity.ManagingOrgId = OrganizationManager.GetPrimaryOrganizationId( user.Id );

				valid = mgr.Update( entity, ref status );
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

	
		private bool ValidateCredential( MC.Credential entity, bool skippingCredentialTypeEdit, ref List<string> messages )
		{
			bool isValid = true;
//			List<string> messages = new List<string>();
			if ( string.IsNullOrWhiteSpace( entity.Name ) )
				messages.Add("Credential name is required");

			if ( entity.IsDescriptionRequired && string.IsNullOrWhiteSpace( entity.Description ) )
				messages.Add( "Credential description is required" );
			//entity.IsNewVersion && 
			//if ( entity.Id == 0 )
			//{		}
				if ( !ServiceHelper.IsValidGuid( entity.OwningAgentUid ) )
				{
					messages.Add( "An owning organization must be selected" );
				}

				if ( entity.OwnerRoles == null || entity.OwnerRoles.Items.Count == 0 )
				{
					messages.Add( "Invalid request, please select one or more roles for the owing agent." );
				}

			//if ( entity.CredentialOrganizationTypeId.GetFirstItemId() < 1 )
			//if ( entity.CredentialOrganizationTypeId < 1 )
			//{
			//	messages.Add( "The type of owning organization must be selected" );
			//}

			//if ( entity.EarningCredentialPrimaryMethodId < 1 )
			//{
			//	messages.Add( "Select the primary method to earn this credential" );
			//}
			

			//must have a type
			//==> unless from the quick add??

			if ( CodesManager.GetEnumerationSelection( entity.CredentialType ) == 0
				&& string.IsNullOrWhiteSpace( entity.CredentialType.OtherValue ) )
			{
				if ( skippingCredentialTypeEdit == false )
				{
					//OR set a default value?
					messages.Add( "Credential type is required" );
				}
			}

			if ( string.IsNullOrWhiteSpace( entity.SubjectWebpage ) )
				messages.Add( "Subject Webpage is required" );

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
		//			status += string.Join( "<br/>", messages.ToArray() );

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
		//			status += string.Join( "<br/>", messages.ToArray() );
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
		//		status += string.Join( "<br/>", messages.ToArray() );
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
		//			status += string.Join( "<br/>", messages.ToArray() );
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
		//		status += string.Join( "<br/>", messages.ToArray() );
		//		return false;
		//	}
		//	OrganizationRoleManager mgr = new OrganizationRoleManager();
		//	try
		//	{
		//		entity.ParentId = credentialId;
		//		if ( mgr.Credential_SaveQAActions( entity, user.Id, ref messages ) == false )
		//		{
		//			valid = false;
		//			status += string.Join( "<br/>", messages.ToArray() );
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
		//public static List<DurationProfile> DurationProfile_GetAll( Guid parentId )
		//{
		//	List<DurationProfile> list = DurationProfileManager.GetAll( parentId );
		//	return list;
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


		public bool DurationProfile_Update( DurationProfile entity, Guid contextParentUid, Guid contextMainUid, int userId, ref string statusMessage )
		{
			//LoggingHelper.DoTrace( 2, string.Format( "CredentialServices.DurationProfile_Update. contextParentUid: {0} contextMainUid: {1} ", contextParentUid.ToString(), contextMainUid.ToString() ) 				);

			List<String> messages = new List<string>();
			if ( entity == null || !BaseFactory.IsGuidValid( contextParentUid ) )
			{
				messages.Add( "Error - missing an identifier for the DurationProfile" );
				return false;
			}
			//validate credential and access
			//==> not just from a credential
			//MC.Credential credential = GetCredential()
			//if (CanUserUpdateCredential( contextMainUid, userId, ref statusMessage ) == false) 
			//{
			//	messages.Add( "Error - missing credential identifier" );
			//	return false;
			//}
			//CanUser update entity?
			MC.Entity e = EntityManager.GetEntity( contextParentUid );

			//remove this if properly passed from client
			//plus need to migrate to the use of EntityId
			//entity.ParentUid = parentUid;
			entity.EntityId = e.Id;
			entity.CreatedById = entity.LastUpdatedById = userId;

			//if an add, the new id will be returned in the entity
			bool isValid = new DurationProfileManager().DurationProfileUpdate( entity, userId, ref messages );
			statusMessage = string.Join( "<br/>", messages.ToArray() );
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

		#endregion

		
		#region Credential Revocation Profile
		public static RevocationProfile RevocationProfile_GetForEdit( int profileId,
				bool forEditView = true )
		{
			
			RevocationProfile profile = Entity_RevocationProfileManager.Get( profileId );

			return profile;
		}

		public bool RevocationProfile_Save( RevocationProfile entity, Guid credentialUid, string action, AppUser user, ref string status, bool isQuickCreate = false )
		{
			bool valid = true;
			status = "";
			List<string> messages = new List<string>();
			Entity_RevocationProfileManager mgr = new Entity_RevocationProfileManager();
			try
			{
				MC.Credential credential = GetBasicCredentialAsLink( credentialUid );

				int count = 0;
				//entity.IsNewVersion = true;
				if ( mgr.Save( entity, credential, user.Id, ref messages ) == false )
				{
					valid = false;
					status = string.Join( "<br/>", messages.ToArray() );
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
