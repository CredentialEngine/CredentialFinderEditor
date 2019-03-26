using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Common;
using Models.ProfileModels;
using ThisEntity = Models.ProfileModels.LearningOpportunityProfile;
using Models.Search;
using Utilities;
using CF = Factories;
using Mgr = Factories.LearningOpportunityManager;
using Factories;

namespace CTIServices
{
	public class LearningOpportunityServices 
	{
		static string thisClassName = "LearningOpportunityServices";

		#region retrievals
		public static LearningOpportunityProfile Get( int id, bool forEditView = false )
		{
			LearningOpportunityProfile entity = Mgr.Get( id, forEditView );

			return entity;
		}
		public static ThisEntity GetBasic( int id )
		{
			ThisEntity entity = Mgr.GetBasic( id );
			return entity;
		}
		public static LearningOpportunityProfile GetForEdit( int id, bool afterAddRecord, ref string status )
		{
			LearningOpportunityProfile entity = Mgr.Get( id, true, true );
			if ( entity.IsReferenceVersion && !afterAddRecord && !AccountServices.IsUserSiteStaff() )
			{
				entity.Id = 0;
				status = "A reference Learning Opportunity cannot be edited - Sorry";
			}

			return entity;
		}
		public static LearningOpportunityProfile GetForMicroProfile( int id, bool forEditView = false )
		{
			LearningOpportunityProfile entity = Mgr.GetBasic( id );

			return entity;
		}
		public static LearningOpportunityProfile GetForDetail( int id )
		{
			AppUser user = AccountServices.GetCurrentUser();
			return GetForDetail( id, user );

		}
		public static LearningOpportunityProfile GetForPublish( int id, AppUser user )
		{
			string statusMessage = "";
			LearningOpportunityProfile entity = Mgr.GetForPublish( id );
			if ( CanUserUpdateLearningOpportunity( entity, user, ref statusMessage ) )
				entity.CanUserEditEntity = true;

			return entity;
		}
		public static string GetForFormat( int id, AppUser user, ref bool isValid, ref List<string> messages, ref bool isApproved, ref DateTime? lastPublishDate, ref DateTime lastUpdatedDate, ref DateTime lastApprovedDate, ref string ctid )
		{
			string statusMessage = "";
			LearningOpportunityProfile entity = Mgr.GetForPublish( id );
            if (entity == null || entity.Id == 0)
            {
                isValid = false;
                messages.Add( "Error - the requested Learning Opportunity was not found." );
                return "";
            }
            if ( CanUserUpdateLearningOpportunity( entity, user, ref statusMessage ) )
				entity.CanUserEditEntity = true;
			isApproved = entity.IsEntityApproved();
			ctid = entity.CTID;
			string payload = RegistryAssistantServices.LearningOpportunityMapper.FormatPayload( entity, ref isValid, ref messages );
			lastUpdatedDate = entity.EntityLastUpdated;
			lastApprovedDate = entity.EntityApproval.Created;
            if ( ( entity.CredentialRegistryId ?? "" ).Length == 36 )
                lastPublishDate = CF.ActivityManager.GetLastPublishDateTime( "learningopportunity", id );
            return payload;
		}

        public static LearningOpportunityProfile GetDetailByCtid( string ctid, AppUser user, bool skippingCache = false )
        {
            LearningOpportunityProfile entity = new LearningOpportunityProfile();
            if ( string.IsNullOrWhiteSpace( ctid ) )
                return entity;
            var assessment = Mgr.GetByCtid( ctid );
            return GetForDetail( assessment.Id, user, skippingCache );
        }
        public static LearningOpportunityProfile GetForDetail( int id, AppUser user, bool skippingCache = false )
		{
			LearningOpportunityProfile entity = Mgr.GetForDetail( id );
			string status = "";
			if ( CanUserUpdateLearningOpportunity( entity.RowId, user, ref status ) )
				entity.CanUserEditEntity = true;
			return entity;
		}
        public static string GetPublishedPayload( int id )
        {
            LearningOpportunityProfile entity = Mgr.GetBasic( id );
            string payload =  CF.RegistryPublishManager.GetMostRecentPublishedPayload( entity.ctid );
            return payload;
        }
        public static LearningOpportunityProfile GetLightLearningOpportunityByRowId( string rowId )
		{
			if ( !Mgr.IsValidGuid( rowId ) )
				return null;

			string where = string.Format( " RowId = '{0}'", rowId );
			int pTotalRows = 0;

			List<LearningOpportunityProfile> list = Mgr.Search( where, "", 1, 50, 0, ref pTotalRows );

			if ( list.Count > 0 )
				return list[ 0 ];
			else
				return null;
		}

		public static LearningOpportunityProfile GetLightLearningOpportunityById( int learningOpportunityId )
		{
			if ( learningOpportunityId < 1 )
				return null;
			
			var entity = LearningOpportunityManager.GetBasic( learningOpportunityId );
			return entity;

			//string where = string.Format( " base.Id = {0}", learningOpportunityId );
			//int pTotalRows = 0;
			//List<LearningOpportunityProfile> list = Mgr.Search( where, "", 1, 50, 0, ref pTotalRows );

			//if ( list.Count > 0 )
			//	return list[ 0 ];
			//else
			//	return null;
		}
		#endregion


		#region Searches
		//public static List<CodeItem> SearchAsCodeItem( string keyword, int startingPageNbr, int pageSize, ref int totalRows )
		//{
		//	List<LearningOpportunityProfile> list = Search( keyword, startingPageNbr, pageSize, ref totalRows, true );
		//	List<CodeItem> codes = new List<CodeItem>();
		//	foreach ( LearningOpportunityProfile item in list )
		//	{
		//		codes.Add( new CodeItem()
		//		{
		//			Id = item.Id,
		//			Name = item.Name,
		//			Description = item.Description
		//		} );
		//	}
		//	return codes;
		//}
		public static List<string> Autocomplete( string keyword, int maxTerms = 25 )
		{
			int userId = 0;
			string where = "";
			int totalRows = 0;
			//only target records with a ctid
			where = " (len(Isnull(base.Ctid,'')) = 39) ";

			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;
			//SetAuthorizationFilter( user, ref where );
            SearchServices.SetAuthorizationFilter( user, "LearningOpportunity_Summary", ref where );

            //if ( type == "learningopp" ) 
            //{
            SetKeywordFilter( keyword, true, ref where );
				//string keywords = ServiceHelper.HandleApostrophes( keyword );
				//if ( keywords.IndexOf( "%" ) == -1 )
				//	keywords = "%" + keywords.Trim() + "%";
				//where = string.Format( " (base.name like '{0}') ", keywords );
			//}


			return Mgr.Autocomplete( where, 1, maxTerms, userId, ref totalRows );
			;
		}
		public static List<LearningOpportunityProfile> MicroSearch( string keywords, int pageNumber, int pageSize, ref int totalRows )
		{
			string pOrderBy = "";
			string filter = "";

			int userId = 0;
            AppUser user = new AppUser();
            //if we filter by user, then will only get results for member orgs, and we want to be able to get references!
            user = AccountServices.GetCurrentUser();
            if ( user != null && user.Id > 0 )
				userId = user.Id;

			SetKeywordFilter( keywords, true, ref filter );
            //18-02-07 will now only get public data
			//SetAuthorizationFilter( user, ref filter );
            SearchServices.SetAuthorizationFilter( user, "LearningOpportunity_Summary", ref filter, true );

            return Mgr.Search( filter, pOrderBy, pageNumber, pageSize, userId, ref totalRows );
		}

        public static List<LearningOpportunityProfile> GetAllForOwningOrganization( string orgUid, ref int pTotalRows )
        {
            List<LearningOpportunityProfile> list = new List<LearningOpportunityProfile>();
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

        public static List<Dictionary<string, object>> GetAllForExport_DictionaryList( string owningOrgUid, bool includingConditionProfile = true )
        {
            return Mgr.GetAllForExport_DictionaryList( owningOrgUid, includingConditionProfile );
        }
        /// <summary>
        /// Lopp Search
        /// </summary>
        /// <param name="data"></param>
        /// <param name="totalRows"></param>
        /// <param name="directOnly">Context is searching for a specific entity, not one that matches a fuzzy search </param>
        /// <returns></returns>
        public static List<LearningOpportunityProfile> Search( MainSearchInput data, ref int totalRows, bool directOnly = false )
		{
			string where = "";
			int userId = 0;
			List<string> competencies = new List<string>();
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;

			//only target records with a ctid
			where = " (len(Isnull(base.Ctid,'')) = 39) ";

			//string pOrderBy = "";
			if ( directOnly )
			{
				SetKeywordFilter( data.Keywords, true, ref where );
			}
			else
			{
				SearchServices.HandleCustomFilters( data, 61, ref where );
				SearchServices.SetSubjectsFilter( data, CF.CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, ref where );
				SearchServices.SetRolesFilter( data, ref where );
				SearchServices.SetBoundariesFilter( data, ref where );
				//SearchServices.SetQAbyOrgFilter( data, ref where );
				//CIP
				SearchServices.SetFrameworksFilter( data, "LearningOpportunity", ref where );
				if ( data.FiltersV2.Any( m => m.Name == "occupations" ) )
					SearchServices.SetFrameworkTextFilter( data, "LearningOpportunity", CodesManager.PROPERTY_CATEGORY_SOC, ref where );
				if ( data.FiltersV2.Any( m => m.Name == "industries" ) )
					SearchServices.SetFrameworkTextFilter( data, "LearningOpportunity", CodesManager.PROPERTY_CATEGORY_NAICS, ref where );
				if ( data.FiltersV2.Any( m => m.Name == "instructionalprogramtype" ) )
					SearchServices.SetFrameworkTextFilter( data, "LearningOpportunity", CodesManager.PROPERTY_CATEGORY_CIP, ref where );
				//SetFrameworksFilter( data, ref where );
    //            if ( data.FiltersV2.Any( m => m.Name == "instructionalprogramtype" ) )
    //                SetFrameworkTextFilter( data, ref where );
                //Competencies
                SetCompetenciesFilter( data, ref where, ref competencies );
                string messages = "";
                SearchServices.SetDatesFilter( data, CF.CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, ref where, ref messages );
                SearchServices.HandleApprovalFilters( data, 16, 7, ref where );
				SearchServices.SetLanguageFilter( data, 7, ref where );
				SetPropertiesFilter( data, ref where );

				SetKeywordFilter( data.Keywords, false, ref where );

                SetConnectionsFilter( data, ref where );
            } 
			
			//SetAuthorizationFilter( user, ref where );
            SearchServices.SetAuthorizationFilter( user, "LearningOpportunity_Summary", ref where );

            LoggingHelper.DoTrace( 6, "LearningOpportunityServices.Search(). Filter: " + where );

			return Mgr.Search( where, data.SortOrder, data.StartPage, data.PageSize, userId, ref totalRows, ref competencies );
		}
		private static void SetKeywordFilter( string keywords,  bool isBasic, ref string where )
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
			string indexFilter = " OR (base.Id in (SELECT c.id FROM [dbo].[Entity.SearchIndex] a inner join Entity b on a.EntityId = b.Id inner join LearningOpportunity c on b.EntityUid = c.RowId where (b.EntityTypeId = 3 AND ( a.TextValue like '{0}' OR a.[CodedNotation] like '{0}' ) ))) ";

			string subjectsEtc = " OR (base.Id in (SELECT c.id FROM [dbo].[Entity.Reference] a inner join Entity b on a.EntityId = b.Id inner join LearningOpportunity c on b.EntityUid = c.RowId where [CategoryId] in (34 ,35) and a.TextValue like '{0}' )) ";

			string frameworkItems = " OR (RowId in (SELECT EntityUid FROM [dbo].[Entity.FrameworkItemSummary] a where CategoryId= 23 and entityTypeId = 7 AND  a.title like '{0}' ) ) ";

			string otherFrameworkItems = " OR (RowId in (SELECT EntityUid FROM [dbo].[Entity_Reference_Summary] a where  a.TextValue like '{0}' ) ) ";


			string competencies = " OR ( base.Id in (SELECT LearningOpportunityId FROM [dbo].LearningOpportunity_Competency_Summary  where [Description] like '{0}' ) ) ";
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
			if ( isBasic || isCustomSearch)
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
					where = where + AND + string.Format( " ( " + text + subjectsEtc + frameworkItems + otherFrameworkItems + competencies + " ) ", keywords );

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
				where = where + AND + string.Format(" (base.StatusId = {0}) ",CF.CodesManager.ENTITY_STATUS_PUBLISHED);
				return;
			}

			if ( AccountServices.IsUserSiteStaff( user )
			  || AccountServices.CanUserViewAllContent( user ) )
			{
				//can view all, edit all
				return;
			}

			//can only view where status is published, or associated with 
			where = where + AND + string.Format( "((base.Id in (SELECT cs.Id FROM [dbo].[Organization.Member] om inner join [LearningOpportunity_Summary] cs on om.ParentOrgId = cs.ManagingOrgId where userId = {0}) ))",  user.Id );

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
			string template = " ( base.Id in (SELECT distinct LearningOpportunityId FROM [dbo].LearningOpportunity_Competency_SummaryV2  where AlignmentType = 'teaches' AND ({0}) ) )";
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
				catch { }
				text = ServiceHelper.HandleApostrophes( text );
				competencies.Add( text.Trim() );
				next += OR + string.Format( phraseTemplate, text.Trim() );
				OR = " OR ";

			}
			if ( !string.IsNullOrWhiteSpace( next ) )
			{
				where = where + AND + string.Format( template, next );
			}

		}
		//

		private static void SetPropertiesFilter( MainSearchInput data, ref string where )
		{
			string searchCategories = UtilityManager.GetAppKeyValue( "loppSearchCategories", "21,37," );
            SearchServices.SetPropertiesFilter( data, 7, searchCategories, ref where );
   //         string AND = "";
   //         string template1 = " ( base.Id in ( SELECT  [EntityBaseId] FROM [dbo].[EntityProperty_Summary] where EntityTypeId= 7 AND [PropertyValueId] in ({0}) )) ";
   //         string template = " ( base.Id in ( SELECT  [EntityBaseId] FROM [dbo].[EntityProperty_Summary] where EntityTypeId= 7 AND {0} )) ";
            
   //         string properyListTemplate = " ( [PropertyValueId] in ({0}) ) ";
   //         string filterList = "";
   //         int prevCategoryId = 0;

   //         //Updated to use FiltersV2
   //         string next = "";
			//if ( where.Length > 0 )
			//	AND = " AND ";
			//foreach ( var filter in data.FiltersV2.Where(m => m.Type == MainSearchFilterV2Types.CODE ) )
			//{
			//	var item = filter.AsCodeItem();
			//	if ( searchCategories.Contains( item.CategoryId.ToString() ) )
			//	{
   //                 //18-03-27 mp - these are all property values, so using an AND with multiple categories will always fail - removing prevCategoryId check
   //                 //if (item.CategoryId != prevCategoryId)
   //                 //{
   //                 //    if (prevCategoryId > 0)
   //                 //    {
   //                 //        next = next.Trim(',');
   //                 //        filterList += (filterList.Length > 0 ? " AND " : "") + string.Format(properyListTemplate, next);
   //                 //    }
   //                 //    prevCategoryId = item.CategoryId;
   //                 //    next = "";
   //                 //}
   //                 next += item.Id + ",";
   //             }
			//}
			//next = next.Trim( ',' );
			//if ( !string.IsNullOrWhiteSpace( next ) )
			//{
   //             //where = where + AND + string.Format( template, next );
   //             filterList += (filterList.Length > 0 ? " AND " : "") + string.Format(properyListTemplate, next);
   //             where = where + AND + string.Format(template, filterList);
   //         }

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
			string codeTemplate = "  (base.Id in (SELECT c.id FROM [dbo].[Entity.FrameworkItemSummary] a inner join Entity b on a.EntityId = b.Id inner join LearningOpportunity c on b.EntityUid = c.RowId where [CategoryId] = {0} and ([FrameworkGroup] in ({1})  OR ([CodeId] in ({2}) )  ))  ) ";

			//Updated to use FiltersV2
			string next = "";
			string groups = "";
			if ( where.Length > 0 )
				AND = " AND ";
			var targetCategoryID = 23;
			foreach ( var filter in data.FiltersV2.Where( m => m.Type == MainSearchFilterV2Types.FRAMEWORK ) )
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

		}

        private static void SetFrameworkTextFilter( MainSearchInput data, ref string where )
        {
            string AND = "";
            string OR = "";

            string codeTemplate = "  (base.Id in (SELECT c.id FROM [dbo].[Entity.FrameworkItemSummary] a inner join Entity b on a.EntityId = b.Id inner join LearningOpportunity c on b.EntityUid = c.RowId where ({0})) ) ";

            string phraseTemplate = " (case when a.FrameworkCode = '' then a.Title else a.Title + ' (' + a.FrameworkCode + ')' end like '%{0}%') ";

            //Updated to use FiltersV2
            string next = "";
            if ( where.Length > 0 )
                AND = " AND ";

            foreach ( var filter in data.FiltersV2.Where( m => m.Type == MainSearchFilterV2Types.TEXT ) )
            {
                var text = ServiceHelper.HandleApostrophes( filter.AsText() );
                next += OR + string.Format( phraseTemplate, text );

                OR = " OR ";
            }
            if ( !string.IsNullOrWhiteSpace( next ) )   
            {
                where = where + AND + " ( " + string.Format( codeTemplate, next ) + ")";
            }
        }
        #endregion
        #region === add/update/delete =============

		public static bool CanUserUpdateLearningOpportunity( Guid entityUid, AppUser user, ref string status )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( AccountServices.IsUserSiteStaff( user ) )
				return true;
			LearningOpportunityProfile entity = LearningOpportunityManager.GetBasic( entityUid );

			return CanUserUpdateLearningOpportunity( entity, user, ref status );
		}
		public static bool CanUserUpdateLearningOpportunity( int learningOpportunityId, AppUser user, ref string status )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( learningOpportunityId < 1 )
				return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;

			//note the following method uses the search which returns CanEditRecord,  so could probably check here and return
			LearningOpportunityProfile entity = LearningOpportunityManager.GetBasic( learningOpportunityId );

			return CanUserUpdateLearningOpportunity( entity, user, ref status );
		}
		public static bool CanUserUpdateLearningOpportunity( LearningOpportunityProfile entity, AppUser user, ref string status )
		{
			if ( user == null || user.Id == 0 )
				return false;

			bool isValid = false;
			if ( entity == null || entity.Id == 0 )
				return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;

			//is a member of the assessment managing organization 
			if ( OrganizationServices.IsOrganizationMember( user.Id, entity.OwningOrganizationId ) )
				return true;

			status = "Error - you do not have edit access for this record.";
			return isValid;
		}

		public static bool CanUserViewLearningOpportunity( LearningOpportunityProfile entity, AppUser user, ref string status )
		{
			bool isValid = false;
			status = "Error - you do not have view access for this record.";
			if ( entity == null || entity.Id == 0 )
			{
				status = "Learning Opportunity was Not found";
				return false;
			}
			//if ( entity.StatusId == CF.CodesManager.ENTITY_STATUS_PUBLISHED )
			//	return true;

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
		/// Add a LearningOpportunity stack
		/// ??what to return - given the jumbotron form
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public int Add( LearningOpportunityProfile entity, AppUser user, ref bool valid, ref string statusMessage )
		{
			entity.CreatedById  = entity.LastUpdatedById = user.Id;
			LoggingHelper.DoTrace( 5, string.Format( "LearningOpportunityServices.LearningOpportunity_Add. Org: {0}, userId: {1}", entity.Name, entity.CreatedById ) );

			int id = 0;
			statusMessage = "";
			Mgr mgr = new Mgr();
			try
			{
				//set the managing orgId
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
						new ActivityServices().AddEditorActivity( SiteActivity.LearningOpportunity, "Add Reference", string.Format( "{0} added a new Reference Learning Opportunity: {1}", user.FullName(), entity.Name ), entity.CreatedById, id, entity.RowId );
					}
					else
					{
						new ActivityServices().AddEditorActivity( SiteActivity.LearningOpportunity, "Add", string.Format( "{0} added a new Learning Opportunity: {1}", user.FullName(), entity.Name ), entity.CreatedById, id, entity.RowId );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "LearningOpportunityServices.LearningOpportunity_Add" );
			}
			return id;
		}


		public bool Update( LearningOpportunityProfile entity, string section , AppUser user, ref string statusMessage )
		{
			entity.LastUpdatedById = user.Id;
			LoggingHelper.DoTrace( 5, string.Format( "LearningOpportunityServices.LearningOpportunity_Update. OrgId: {0}, userId: {1}", entity.Id, entity.LastUpdatedById ) );

			//if ( !CanUserUpdateLearningOpportunity( entity, user, ref statusMessage ) )
			//{
			//	return false;
			//}
			statusMessage = "";
			if ( section == "cip" )
			{
				statusMessage = "NOTE: CIP codes are saved immediately upon selection, you do not have to click the Save button";
				return false;
			}
			else if ( section == "part" )
			{
				statusMessage = "NOTE: Learning Opportunity Parts are saved immediately upon selection, you do not have to click the Save button";
				return false;
			}
			Mgr mgr = new Mgr();
			bool isOK = false;
			try
			{
				isOK = mgr.Update( entity, ref statusMessage );

				if ( isOK )
				{
					statusMessage = "Successfully Updated Learning Opportunity";
					new ActivityServices().AddEditorActivity( SiteActivity.LearningOpportunity, "Update", string.Format( "{0} updated Learning Opportunity (or parts of): {1}", user.FullName(), entity.Name ), user.Id, entity.Id, entity.RowId );
				}

				CF.CacheManager.RemoveItemFromCache( "LearningOpportunity", entity.Id );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "LearningOpportunityServices.LearningOpportunity_Update" );
			}
			return isOK;
		}

		public bool Delete( int recordId, AppUser user, ref string statusMessage )
		{
			bool isOK = false;
			statusMessage = "";
			return Delete( recordId, user, ref isOK, ref statusMessage );
		}

        /// <summary>
        /// Remove a learning opportunity
        /// </summary>
        /// <param name="recordId"></param>
        /// <param name="user"></param>
        /// <param name="valid"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool Delete( int recordId, AppUser user, ref bool valid, ref string status )
		{
			Mgr mgr = new Mgr();
			try
			{
                LearningOpportunityProfile entity = Mgr.Get(recordId, false, false);
                if (!string.IsNullOrWhiteSpace(entity.CredentialRegistryId))
                {
                    List<SiteActivity> list = new List<SiteActivity>();
                    //should be deleting from registry!
                    //will be change to handling with managed keys
                    new RegistryServices().Unregister_LearningOpportunity(recordId, user, ref status, ref list);
                }

                valid = mgr.Delete( recordId, ref status );

				CF.CacheManager.RemoveItemFromCache( "LearningOpportunity", recordId );
                new ActivityServices().AddEditorActivity("LearningOpportunity", "Deactivate", string.Format("{0} deactivated LearningOpportunity: {1} (id: {2})", user.FullName(), entity.Name, entity.Id), user.Id, 0, recordId);
            }
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "LearningOpportunityServices.LearningOpportunity_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

        public bool DeleteAllForOrganization( AppUser user, string owningRowid, ref List<string> messages )
        {
            Mgr mgr = new Mgr();
            Guid owningOrgUid = new Guid( owningRowid );

            if ( !OrganizationServices.CanUserUpdateOrganization( user, owningOrgUid ) )
            {
                messages.Add( "You don't have the necessary authorization to remove learning opportunities from this organization." );
                return false;
            }
            return mgr.DeleteAllForOrganization( owningOrgUid, ref messages );
        }
        #endregion

        #region Entity FrameworkItems==> moved TO Profile services

        #endregion

        #region Learning Opportunity parts
        /// <summary>
        /// Add a learning opp part to a parent
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="recordId"></param>
        /// <param name="user"></param>
        /// <param name="valid"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int AddLearningOpportunity_AsPart( Guid parentUid, int childLearningOppId, AppUser user, ref bool valid, ref string status )
		{
			int id = 0;
			List<string> messages = new List<string>();
			CF.Entity_LearningOpportunityManager mgr = new CF.Entity_LearningOpportunityManager();
			try
			{
				Entity parent = CF.EntityManager.GetEntity( parentUid );
				if ( parent == null || parent.Id == 0 )
				{
					status = "Error - the parent entity was not found.";
					return 0;
				}
				if ( parent.EntityBaseId == childLearningOppId )
				{
					status = "Error - you cannot add the parent learning opportunity as a child learning opportunity." ;
					return 0;
				}
				id = mgr.Add( parentUid, childLearningOppId, user.Id, true, ref messages );

				if ( id > 0 )
				{
					//if valid, status contains the cred id, category, and codeId
					new ActivityServices().AddEditorActivity( SiteActivity.LearningOpportunity, "Add Lopp Part", string.Format( "{0} added Learning Opportunity part {1} to Learning Opportunity {2}", user.FullName(), childLearningOppId, parent.Id ), user.Id, childLearningOppId, parent.EntityUid );
					status = "";

                    new ProfileServices().UpdateTopLevelEntityLastUpdateDate(parent.Id, string.Format("Entity Update triggered by {0} adding a Learning Opportunity Part for : {1}, BaseId: {2}", user.FullName(), parent.EntityType, parent.EntityBaseId));
                }
				else
				{
					valid = false;
					status += string.Join( "<br/>", messages.ToArray() );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".AddLearningOpportunity_AsPart" );
				status = ex.Message;
				valid = false;
			}

			return id;
		}

		public bool DeleteLearningOpportunityPart( Guid parentUid, int recordId, AppUser user, ref string status )
		{
			bool valid = true;

			CF.Entity_LearningOpportunityManager mgr = new CF.Entity_LearningOpportunityManager();
			try
			{
				Entity parent = CF.EntityManager.GetEntity( parentUid );
				if ( parent == null || parent.Id == 0 )
				{
					status = "Error - the parent entity was not found.";
					return false;
				}

				valid = mgr.Delete( parentUid, recordId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					new ActivityServices().AddEditorActivity( parent.EntityType, "Delete Learning Opportunity Part", string.Format( "{0} deleted Learning Opportunity part {1} from Learning Opportunity Profile  {2}", user.FullName(), recordId, parent.Id ), user.Id, 0, recordId );
					status = "";

                    new ProfileServices().UpdateTopLevelEntityLastUpdateDate(parent.Id, string.Format("Entity Update triggered by {0} deleting a Learning Opportunity Part for : {1}, BaseId: {2}", user.FullName(), parent.EntityType, parent.EntityBaseId));
                }
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + "DeleteLearningOpportunityPart" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}
		#endregion
	}
}
