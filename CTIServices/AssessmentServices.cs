using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Models;
using Models.Common;
using Models.ProfileModels;
using Models.Search;
using Utilities;
using CF = Factories;
using Mgr = Factories.AssessmentManager;
using Factories;

namespace CTIServices
{
	public class AssessmentServices
	{
	
		#region Searches 
	//	public static List<CodeItem> SearchAsCodeItem( string keyword, int startingPageNbr, int pageSize, ref int totalRows )
	//	{
	//		List<AssessmentProfile> list = Search( keyword, startingPageNbr, pageSize, ref totalRows );
	//		List<CodeItem> codes = new List<CodeItem>();
	//		foreach (AssessmentProfile item in list) 
	//		{
	//			codes.Add(new CodeItem() {
	//				Id = item.Id,
	//				Name = item.Name,
	//				Description = item.Description
	//			});
	//		}
	//		return codes;
	//}
		public static List<string> Autocomplete( string keyword, int maxTerms = 25 )
		{
			//List<string> results = new List<string>();
			int userId = 0;
			string filter = "";
			int totalRows = 0;
            //only target records with a ctid
            filter = " (len(Isnull(base.Ctid,'')) = 39) ";
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;
			//SetAuthorizationFilter( user, ref filter );
            SearchServices.SetAuthorizationFilter( user, "Assessment_Summary", ref filter );
            //if ( type == "assessment" )
            //{
            //nor sure why this was commented out
            SetKeywordFilter( keyword, true, ref filter );


			return Mgr.Autocomplete( filter, 1, maxTerms, userId, ref totalRows );

		}
		public static List<AssessmentProfile> MicroSearch( string keywords, int pageNumber, int pageSize, ref int totalRows )
		{
			string pOrderBy = "";
			string filter = "";
			int userId = 0;
            AppUser user = new AppUser();
            //if we filter by user, then will only get results for member orgs
            //but if we remove user, then don't get stuff they do own, and is not published
            user = AccountServices.GetCurrentUser();
            if ( user != null && user.Id > 0 )
				userId = user.Id;

			SetKeywordFilter( keywords, true, ref filter );
            //18-02-07 will now only get public data
            //SetAuthorizationFilter( user, ref filter );
            SearchServices.SetAuthorizationFilter( user, "Assessment_Summary", ref filter, true );
            BaseSearchModel bsm = new BaseSearchModel()
            {
                Filter = filter,
                OrderBy = pOrderBy,
                PageNumber = pageNumber,
                PageSize = pageSize,
                UserId = userId, IsMicrosearch = true
            };
            return Mgr.Search( bsm, ref totalRows );
		}
        public static List<AssessmentProfile> GetAllForOwningOrganization( string orgUid, ref int pTotalRows )
        {
            List<AssessmentProfile> list = new List<AssessmentProfile>();
            if ( string.IsNullOrWhiteSpace( orgUid ) )
                return list;
            string keywords = "";
            int pageNumber = 1;
            int pageSize = 0;
            string pOrderBy = "";
            string AND = "";
            int userId = AccountServices.GetCurrentUserId();
            string filter = string.Format( " ( len(Isnull(base.Ctid,'')) = 39 AND  base.OwningAgentUid = '{0}' ) ", orgUid );
            //string filter = "";

            if ( filter.Length > 0 )
                AND = " AND ";
            //no other filters yet. Future:
            // not approved, need publishing, etc
            if ( !string.IsNullOrWhiteSpace( keywords ) )
            {
                //    keywords = ServiceHelper.HandleApostrophes( keywords );
                //    if ( keywords.IndexOf( "%" ) == -1 )
                //        keywords = "%" + keywords.Trim() + "%";
                //    filter = filter + AND + string.Format( " (base.name like '{0}' OR base.Description like '{0}'  OR base.Url like '{0}' OR CreatorOrgs like '{0}'  OR OwningOrgs like '{0}')", keywords );
            }

            return Mgr.Search( filter, pOrderBy, pageNumber, pageSize, userId, ref pTotalRows );

        }
   
        public static List<Dictionary<string, object>> GetAllForExport_DictionaryList(string owningOrgUid, bool includingConditionProfile = true)
        {
            return Mgr.GetAllForExport_DictionaryList(owningOrgUid, includingConditionProfile);
        }
        public static List<AssessmentProfile> Search( MainSearchInput data, ref int totalRows )
		{
			string where = "";
			List<string> competencies = new List<string>();
			int userId = 0;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;
			//only target records with a ctid
			where = " (len(Isnull(base.Ctid,'')) = 39) ";

			SetKeywordFilter( data.Keywords, false, ref where );
			SearchServices.SetLanguageFilter( data, 3, ref where );
			SearchServices.SetSubjectsFilter( data, CF.CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, ref where );
			//SetAuthorizationFilter( user, ref where );
            SearchServices.SetAuthorizationFilter( user, "Assessment_Summary", ref where );

            SearchServices.HandleCustomFilters( data, 60, ref where );
            string messages = "";
            SearchServices.SetDatesFilter( data, CF.CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, ref where, ref messages );
            SearchServices.HandleApprovalFilters( data, 16, 3, ref where );

            SetPropertiesFilter( data, ref where );
			SearchServices.SetRolesFilter( data, ref where );
			SearchServices.SetBoundariesFilter( data, ref where );
			//CIP
			SearchServices.SetFrameworksFilter( data, "Assessment", ref where );
			if ( data.FiltersV2.Any( m => m.Name == "occupations" ) )
				SearchServices.SetFrameworkTextFilter( data, "Assessment", CodesManager.PROPERTY_CATEGORY_SOC, ref where );
			if ( data.FiltersV2.Any( m => m.Name == "industries" ) )
				SearchServices.SetFrameworkTextFilter( data, "Assessment", CodesManager.PROPERTY_CATEGORY_NAICS, ref where );
			if ( data.FiltersV2.Any( m => m.Name == "instructionalprogramtype" ) )
				SearchServices.SetFrameworkTextFilter( data, "Assessment", CodesManager.PROPERTY_CATEGORY_CIP, ref where );
			//SetFrameworksFilter( data, ref where );
   //         if (data.FiltersV2.Any(m => m.Name == "instructionalprogramtype"))
   //             SetFrameworkTextFilter(data, ref where);
            //Competencies
            SetCompetenciesFilter( data, ref where, ref competencies );

            SetConnectionsFilter( data, ref where );
            //SearchServices.SetQAbyOrgFilter( data, ref where );
            LoggingHelper.DoTrace( 5, "AssessmentServices.Search(). Filter: " + where );
            BaseSearchModel bsm = new BaseSearchModel()
            {
                Filter = where,
                OrderBy = data.SortOrder,
                PageNumber = data.StartPage,
                PageSize = data.PageSize,
                UserId = userId
            };

            return Mgr.Search( bsm, ref totalRows );
		}
		
		private static void SetKeywordFilter( string keywords, bool isBasic, ref string where )
		{
			if ( string.IsNullOrWhiteSpace( keywords ) || string.IsNullOrWhiteSpace( keywords.Trim() ) )
				return;

			bool includingFrameworkItemsInKeywordSearch = UtilityManager.GetAppKeyValue( "includingFrameworkItemsInKeywordSearch", true );
			bool using_EntityIndexSearch = UtilityManager.GetAppKeyValue( "using_EntityIndexSearch", false );

			//trim trailing (org)
			if ( keywords.IndexOf( "('" ) > 0 )
				keywords = keywords.Substring( 0, keywords.IndexOf( "('" ) );

			//OR base.Description like '{0}'  
			string text = " (base.name like '{0}' OR base.Organization like '{0}'  ) ";
			if ( SearchServices.IncludingDescriptionInKeywordFilter )
			{
				text = " (base.name like '{0}' OR base.Organization like '{0}' OR base.Description like '{0}' ) ";
			}

			bool isCustomSearch = false;
			//for ctid, needs a valid ctid or guid
			if ( keywords.IndexOf( "ce-" ) > -1 && keywords.Length == 39 )
			{
				text = " ( CTID = '{0}' ) ";
				isCustomSearch = true;
			}
			else if ( ServiceHelper.IsValidGuid( keywords ) )
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


			//use Entity.SearchIndex for all
			string indexFilter = " OR (base.Id in (SELECT c.id FROM [dbo].[Entity.SearchIndex] a inner join Entity b on a.EntityId = b.Id inner join Assessment c on b.EntityUid = c.RowId where (b.EntityTypeId = 3 AND ( a.TextValue like '{0}' OR a.[CodedNotation] like '{0}' ) ))) ";

			string subjectsEtc = " OR (base.Id in (SELECT c.id FROM [dbo].[Entity.Reference] a inner join Entity b on a.EntityId = b.Id inner join Assessment c on b.EntityUid = c.RowId where [CategoryId] in (34 ,35) and a.TextValue like '{0}' )) ";

			string frameworkItems = " OR (RowId in (SELECT EntityUid FROM [dbo].[Entity.FrameworkItemSummary] a where CategoryId= 23 and entityTypeId = 3 AND  a.title like '{0}' ) ) ";

			string otherFrameworkItems = " OR (RowId in (SELECT EntityUid FROM [dbo].[Entity_Reference_Summary] a where  a.TextValue like '{0}' ) ) ";


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

			//skip url  OR base.Url like '{0}' 
			if ( isBasic || isCustomSearch )
			{
				if ( !includingFrameworkItemsInKeywordSearch )
					where = where + AND + string.Format( " ( " + text + " ) ", keywords );
				else
					where = where + AND + string.Format( " ( " + text + frameworkItems + " ) ", keywords );
			}
			else
			{
				if ( using_EntityIndexSearch )
					where = where + AND + string.Format( " ( " + text + indexFilter + " ) ", keywords );
				else
					where = where + AND + string.Format( " ( " + text + subjectsEtc + frameworkItems + otherFrameworkItems + " ) ", keywords );
			}

		}

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
			  || AccountServices.CanUserViewAllContent( user) )
			{
				//can view all, edit all
				return;
			}

			//can only view where status is published, or associated with 
			where = where + AND + string.Format( "((base.Id in (SELECT cs.Id FROM [dbo].[Organization.Member] om inner join [Assessment_Summary] cs on om.ParentOrgId = cs.ManagingOrgId where userId = {0}) ))", user.Id );
			//where = where + AND + string.Format( "((base.StatusId = {0}) OR (base.Id in (SELECT cs.Id FROM [dbo].[Organization.Member] om inner join [Assessment_Summary] cs on om.ParentOrgId = cs.ManagingOrgId where userId = {1}) ))", CF.CodesManager.ENTITY_STATUS_PUBLISHED, user.Id );

		}

		private static void SetCompetenciesAutocompleteFilter( string keywords, ref string where )
		{
			List<string> competencies = new List<string>();
			MainSearchInput data = new MainSearchInput();
			MainSearchFilter filter = new MainSearchFilter() { Name = "competencies", CategoryId = 29 };
			filter.Items.Add( keywords );
			SetCompetenciesFilter( data, ref where, ref competencies );

		}
		private static void SetCompetenciesFilter( MainSearchInput data, ref string where, ref List<string> competencies )
		{
			string AND = "";
			string OR = "";
			string keyword = "";
			string template = " ( base.Id in (SELECT distinct  AssessmentId FROM [dbo].Assessment_Competency_SummaryV2  where AlignmentType = 'assesses' AND ({0}) ) )";
			string phraseTemplate = " ([Name] like '%{0}%' OR [Description] like '%{0}%') ";
			//

			//Updated to use FiltersV2
			string next = "";
			if ( where.Length > 0 )
				AND = " AND ";
			foreach ( var filter in data.FiltersV2.Where( m => m.Name == "competencies" ) )
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
				catch
				{
					continue;
				}
				text = ServiceHelper.HandleApostrophes( text );
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
				foreach ( string item in filter.Items )
				{
					keyword = ServiceHelper.HandleApostrophes( item );
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
			}
			*/
		}
		//
		private static void SetPropertiesFilter( MainSearchInput data, ref string where )
		{
			//string AND = "";
			string searchCategories = UtilityManager.GetAppKeyValue( "asmtSearchCategories", "21,37," );
			SearchServices.SetPropertiesFilter( data, 3, searchCategories, ref where );
		}
        public static void SetConnectionsFilter( MainSearchInput data, ref string where )
        {
            string AND = "";
            string OR = "";
            if ( where.Length > 0 )
                AND = " AND ";

            //Should probably get this from the database
            Enumeration entity = CF.CodesManager.GetCredentialsConditionProfileTypes();

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
            validConnections = entity.Items.Select( s => s.SchemaName.ToLower() ).ToList();

            string conditionTemplate = " {0}Count > 0 ";

            //Updated for FiltersV2
            string next = "";
            string condition = "";
            if ( where.Length > 0 )
                AND = " AND ";
            foreach ( var filter in data.FiltersV2.Where( m => m.Type == MainSearchFilterV2Types.CODE ) )
            {
                var item = filter.AsCodeItem();
                if ( item.CategoryId != 15 )
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

        }
        private static void SetFrameworksFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			string codeTemplate = "  (base.Id in (SELECT c.id FROM [dbo].[Entity.FrameworkItemSummary] a inner join Entity b on a.EntityId = b.Id inner join Assessment c on b.EntityUid = c.RowId where [CategoryId] = {0} and ([FrameworkGroup] in ({1})  OR ([CodeId] in ({2}) )  ))  ) ";

			//Updated to use FiltersV2
			string next = "";
			string groups = "";
			if ( where.Length > 0 )
				AND = " AND ";
			var targetCategoryID = 23;
			foreach ( var filter in data.FiltersV2.Where(m => m.Type == MainSearchFilterV2Types.FRAMEWORK ) )
			{
				var item = filter.AsCodeItem();
				var isTopLevel = filter.GetValueOrDefault<bool>( "IsTopLevel", false );
				if ( item.CategoryId == targetCategoryID )
				{
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
				where = where + AND + string.Format( codeTemplate, targetCategoryID, groups, next );
			}

			/* //Retained for reference
			foreach ( MainSearchFilter filter in data.Filters.Where( s => s.CategoryId == 23 ) )
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

        private static void SetFrameworkTextFilter(MainSearchInput data, ref string where)
        {
            string AND = "";
            string OR = "";
            
            string codeTemplate = "  (base.Id in (SELECT c.id FROM [dbo].[Entity.FrameworkItemSummary] a inner join Entity b on a.EntityId = b.Id inner join Assessment c on b.EntityUid = c.RowId where ({0})) ) ";

            string phraseTemplate = " (case when a.FrameworkCode = '' then a.Title else a.Title + ' (' + a.FrameworkCode + ')' end like '%{0}%') ";

            //Updated to use FiltersV2
            string next = "";
            if (where.Length > 0)
                AND = " AND ";

            foreach (var filter in data.FiltersV2.Where(m => m.Type == MainSearchFilterV2Types.TEXT))
            {
                var text = ServiceHelper.HandleApostrophes(filter.AsText());
                next += OR + string.Format(phraseTemplate, text);

                OR = " OR ";
            }
            if (!string.IsNullOrWhiteSpace(next))
            {
                where = where + AND + " ( " + string.Format(codeTemplate, next) + ")";
            }
        }
        public static List<AssessmentProfile> QuickSearch( MainSearchInput data, ref int totalRows )
		{

			int userId = 0;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;
			//string filter = "";
			//if ( !string.IsNullOrWhiteSpace( keyword ) )
			//{
			//	keyword = ServiceHelper.HandleApostrophes( keywords );
			//	if ( keyword.IndexOf( "%" ) == -1 )
			//		keyword = "%" + keywords.Trim() + "%";
			//	filter = string.Format( " (base.name like '{0}' OR base.Description like '{0}'  OR base.Url like '{0}')", keyword );
			//}
			return Mgr.QuickSearch( userId, data.Keywords, data.StartPage, data.PageSize, ref totalRows );
		}
        #endregion

        #region Retrievals
        public static AssessmentProfile Get( int id )
        {
            AssessmentProfile entity = Mgr.Get( id, false, false );
            return entity;
        }
       
        public static AssessmentProfile GetBasic( int asmtId )
        {
            AssessmentProfile entity = Mgr.GetBasic( asmtId );
            return entity;
        }
        public static AssessmentProfile GetDetail( int id )
		{
			AppUser user = AccountServices.GetCurrentUser();
			return GetDetail( id, user );
		}



        public static AssessmentProfile GetDetailByCtid( string ctid, AppUser user, bool skippingCache = false )
        {
            AssessmentProfile entity = new AssessmentProfile();
            if ( string.IsNullOrWhiteSpace( ctid ) )
                return entity;
            var assessment = Mgr.GetByCtid( ctid );
            return GetDetail( assessment.Id, user, skippingCache );
        }


        public static AssessmentProfile GetDetail(  int id, AppUser user, bool skippingCache = false )
		{
			string statusMessage = "";
			AssessmentProfile entity = Mgr.Get( id, false, true );
			if ( CanUserUpdateAssessment( entity, user, ref statusMessage ) )
				entity.CanUserEditEntity = true;

			return entity;
		}
        public static AssessmentProfile GetForPublish( int id, AppUser user )
        {
            string statusMessage = "";
            AssessmentProfile entity = Mgr.GetForPublish( id );
            if ( CanUserUpdateAssessment( entity, user, ref statusMessage ) )
                entity.CanUserEditEntity = true;

            return entity;
        }
        public static AssessmentProfile GetForEdit( int id, bool afterAddRecord, ref string status )
		{
			AssessmentProfile entity = Mgr.Get( id, true, true );
			if ( entity.IsReferenceVersion && !afterAddRecord && !AccountServices.IsUserSiteStaff() )
			{
				entity.Id = 0;
				status = "A reference Assessment cannot be edited - Sorry";
			}

			return entity;
		}

        public static string GetForFormat( int id, AppUser user, ref bool isValid, ref List<string> messages, ref bool isApproved, ref DateTime? lastPublishDate, ref DateTime lastUpdatedDate, ref DateTime lastApprovedDate, ref string ctid )
        {
            //List<string> messages = new List<string>();
            string statusMessage = "";
            AssessmentProfile entity = Mgr.GetForPublish( id );
            if (entity == null || entity.Id == 0)
            {
                isValid = false;
                messages.Add( "Error - the requested Assessment was not found." );
                return "";
            }
            if ( CanUserUpdateAssessment( entity, user, ref statusMessage ) )
                entity.CanUserEditEntity = true;
            isApproved = entity.IsEntityApproved();
            ctid = entity.ctid;
            string payload = RegistryAssistantServices.AssessmentMapper.FormatPayload( entity, ref isValid, ref messages );
			lastUpdatedDate = entity.EntityLastUpdated;
			lastApprovedDate = entity.EntityApproval.Created;
            if ( ( entity.CredentialRegistryId ?? "" ).Length == 36 )
                lastPublishDate = CF.ActivityManager.GetLastPublishDateTime( "assessmentprofile", id );
            return payload;
        }
        public static AssessmentProfile GetLightAssessmentByRowId( string rowId )
		{
			if ( !Mgr.IsValidGuid( rowId ) )
				return null;
			string where = string.Format( " RowId = '{0}'", rowId );
			int pTotalRows = 0;

			List<AssessmentProfile> list = Mgr.Search( where, "", 1, 50, 0, ref pTotalRows );

			if ( list.Count > 0 )
				return list[ 0 ];
			else
				return null;
		}
		public static AssessmentProfile GetLightAssessmentById( int asmtId )
		{
			if ( asmtId < 1 )
				return null;
			string where = string.Format( " base.Id = {0}", asmtId );
			int pTotalRows = 0;

			List<AssessmentProfile> list = Mgr.Search( where, "", 1, 50, 0, ref pTotalRows );

			if ( list.Count > 0 )
				return list[ 0 ];
			else
				return null;
		}

        public static string GetPublishedPayload(int id)
        {
            AssessmentProfile entity = Mgr.GetBasic(id);
            if ( string.IsNullOrWhiteSpace(entity.CredentialRegistryId) )
                return "";

            string payload = CF.RegistryPublishManager.GetMostRecentPublishedPayload(entity.CTID);
            return payload;
        }
        #endregion
        #region === add/update/delete =============

		public static bool CanUserUpdateAssessment( int asmtId, AppUser user, ref string status )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( AccountServices.IsUserSiteStaff( user ) )
				return true;
			AssessmentProfile entity = AssessmentManager.GetBasic( asmtId );

			return CanUserUpdateAssessment( entity, user, ref status );
		}

		public static bool CanUserUpdateAssessment( Guid entityUid, AppUser user, ref string status )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( AccountServices.IsUserSiteStaff( user ) )
				return true;
			AssessmentProfile entity = AssessmentManager.GetBasic( entityUid );

			return CanUserUpdateAssessment( entity, user, ref status );
		}
		public static bool CanUserUpdateAssessment( AssessmentProfile entity, AppUser user, ref string status )
		{
			bool isValid = false;
            if ( entity == null || entity.Id == 0 )
                return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;

			//is a member of the assessment managing organization 
			if ( OrganizationServices.IsOrganizationMember( user.Id, entity.OwningAgentUid ) )
				return true;
			status = "Error - you do not have edit access for this record.";
			return isValid;
		}

		public static bool CanUserViewAssessment( AssessmentProfile entity, AppUser user, ref string status )
        {
			bool isValid = false;
			status = "Error - you do not have view access for this record.";
			if ( entity == null || entity.Id == 0 )
			{
				status = "Assessment was Not found";
				return false;
			}

			if ( user == null || user.Id == 0 )
				return false;
			else if ( AccountServices.CanUserViewAllOfSite( user ) )
				return true;

			//is a member of the assessment managing organization 
			if ( OrganizationServices.IsOrganizationMember( user.Id, entity.OwningAgentUid ) )
				return true;
			
			return isValid;
		}
		/// <summary>
		/// Add a Assessment stack
		/// ??what to return - given the jumbotron form
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public int Add( AssessmentProfile entity, AppUser user,ref bool valid, ref string statusMessage )
		{
			entity.CreatedById = entity.LastUpdatedById = user.Id;
			LoggingHelper.DoTrace( 5, string.Format( "AssessmentServices.Assessment_Add. Org: {0}, userId: {1}", entity.Name, entity.CreatedById ) );

			int id = 0;
			statusMessage = "";
			Mgr mgr = new Mgr();
			try
			{
				if ( entity.ManagingOrgId == 0 )
				{
					if ( ServiceHelper.IsValidGuid( entity.OwningAgentUid ) )
					{
						Organization org = CF.OrganizationManager.GetForSummary( entity.OwningAgentUid );
						entity.ManagingOrgId = org.Id;
					}
					else
					{
						entity.ManagingOrgId = CF.OrganizationManager.GetPrimaryOrganizationId( user.Id );
					}
				}

				id = mgr.Add( entity, ref statusMessage );
				if ( id > 0 )
				{
                    valid = true;
					statusMessage = "";
					if ( entity.IsReferenceVersion )
					{
						new ActivityServices().AddEditorActivity( "AssessmentProfile", "Add Reference", string.Format( "{0} added a new Reference Assessment: {1}", user.FullName(), entity.Name ), entity.CreatedById, id, entity.RowId );
					}
					else
					{
						new ActivityServices().AddEditorActivity("AssessmentProfile", "Add", string.Format( "{0} added a new Assessment: {1}", user.FullName(), entity.Name ), entity.CreatedById, id, entity.RowId );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "AssessmentServices.Add" );
			}
			return id;
		}


		public bool Update( AssessmentProfile entity, AppUser user, ref string statusMessage )
		{
			entity.LastUpdatedById = user.Id;

			LoggingHelper.DoTrace( 5, string.Format( "AssessmentServices.Assessment_Update. OrgId: {0}, userId: {1}", entity.Id, entity.LastUpdatedById ) );
			//if ( !CanUserUpdateAssessment( entity, user, ref statusMessage ) )
			//{
			//	return false;
			//}
			statusMessage = "";
			Mgr mgr = new Mgr();
			bool isOK = false;
			try
			{
				isOK = mgr.Update( entity, ref statusMessage );
				if ( isOK )
				{
					statusMessage = "Successfully Updated Assessment";
					new ActivityServices().AddEditorActivity( "AssessmentProfile", "Update", string.Format( "{0} updated Assessment (or parts of): {1}", user.FullName(), entity.Name ), user.Id, entity.Id, entity.RowId );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "AssessmentServices.Update" );
			}
			return isOK;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assessmentId"></param>
        /// <param name="user"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
		public bool Delete( int assessmentId, AppUser user, ref string status)
		{
			bool valid = true;
            status = "";
		
			Mgr mgr = new Mgr();
			try
			{
                AssessmentProfile entity = Mgr.Get(assessmentId, false, false);
                if (!string.IsNullOrWhiteSpace(entity.CredentialRegistryId))
                {
                    List<SiteActivity> list = new List<SiteActivity>();
                    //should be deleting from registry!
                    //will be change to handling with managed keys
                    new RegistryServices().Unregister_Assessment(assessmentId, user, ref status, ref list);
                }

                valid = mgr.Delete( assessmentId, ref status );
                new ActivityServices().AddEditorActivity("AssessmentProfile", "Delete", string.Format("{0} deleted Assessment: {1} (id: {2})", user.FullName(), entity.Name, entity.Id), user.Id, 0, assessmentId);
                //related entity will be deleted via triggers
               
            }
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "AssessmentServices.Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

        public bool DeleteAllForOrganization( AppUser user,string owningRowid,ref List<string> messages )
        {
            Mgr mgr = new Mgr();
            Guid owningOrgUid = new Guid( owningRowid );

            if ( !OrganizationServices.CanUserUpdateOrganization( user,owningOrgUid ) )
            {
                messages.Add( "You don't have the necessary authorization to remove assessments from this organization." );
                return false;
            }
            return mgr.DeleteAllForOrganization( owningOrgUid,ref messages );
        }
        #endregion
    }
}
