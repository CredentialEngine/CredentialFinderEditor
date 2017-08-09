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


namespace CTIServices
{
	public class AssessmentServices
	{
	
		#region Searches 
		public static List<CodeItem> SearchAsCodeItem( string keyword, int startingPageNbr, int pageSize, ref int totalRows )
		{
			List<AssessmentProfile> list = Search( keyword, startingPageNbr, pageSize, ref totalRows );
			List<CodeItem> codes = new List<CodeItem>();
			foreach (AssessmentProfile item in list) 
			{
				codes.Add(new CodeItem() {
					Id = item.Id,
					Name = item.Name,
					Description = item.Description
				});
			}
			return codes;
	}
		public static List<string> Autocomplete( string keyword, int maxTerms = 25 )
		{
			//List<string> results = new List<string>();
			int userId = 0;
			string where = "";
			int totalRows = 0;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;
			SetAuthorizationFilter( user, ref where );

			//if ( type == "assessment" )
			//{
				//SetKeywordFilter( keyword, true, ref where );
				string keywords = ServiceHelper.HandleApostrophes( keyword );
				if ( keywords.IndexOf( "%" ) == -1 )
					keywords = "%" + keywords.Trim() + "%";
				where = string.Format( " (base.name like '{0}') ", keywords );
			//}
			//else if ( type == "subjects" )
			//	SearchServices.SetSubjectsAutocompleteFilter( keyword, type, ref where );
			//else if ( type == "competencies" )
			//	SetCompetenciesAutocompleteFilter( keyword, ref where );

			return Mgr.Autocomplete( where, 1, maxTerms, userId, ref totalRows );

		}
		public static List<AssessmentProfile> Search( string keywords, int startingPageNbr, int pageSize, ref int totalRows )
		{
			MainSearchInput data = new MainSearchInput();
			data.Keywords = keywords;
			data.StartPage = startingPageNbr;
			data.PageSize = pageSize;

			return Search( data, ref totalRows );
		}
		public static List<AssessmentProfile> Search( MainSearchInput data, ref int totalRows )
		{
			string where = "";
			List<string> competencies = new List<string>();
			int userId = 0;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;

			SetKeywordFilter( data.Keywords, false, ref where );

			SearchServices.SetSubjectsFilter( data, CF.CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, ref where );
			SetAuthorizationFilter( user, ref where );

			SearchServices.HandleCustomFilters( data, 60, ref where );

			SetPropertiesFilter( data, ref where );
			SearchServices.SetRolesFilter( data, ref where );
			SearchServices.SetBoundariesFilter( data, ref where );
			//CIP
			SetFrameworksFilter( data, ref where );
			//Competencies
			SetCompetenciesFilter( data, ref where, ref competencies );

			LoggingHelper.DoTrace( 5, "AssessmentServices.Search(). Filter: " + where );
			return Mgr.Search( where, data.SortOrder, data.StartPage, data.PageSize, userId, ref totalRows );
		}
		
		private static void SetKeywordFilter( string keywords, bool isBasic, ref string where )
		{
			if ( string.IsNullOrWhiteSpace( keywords ) )
				return;
			string text = " (base.name like '{0}' OR base.Description like '{0}'  OR base.Organization like '{0}'  ) ";

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
			string subjectsEtc = " OR (base.Id in (SELECT c.id FROM [dbo].[Entity.Reference] a inner join Entity b on a.EntityId = b.Id inner join Assessment c on b.EntityUid = c.RowId where [CategoryId] in (34 ,35) and a.TextValue like '{0}' )) ";
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
				where = where + AND + string.Format( " ( " + text + " ) ", keywords );
			else 
				where = where + AND + string.Format( " ( " + text + subjectsEtc + " ) ", keywords );

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
			where = where + AND + string.Format( "((base.StatusId = {0}) OR (base.Id in (SELECT cs.Id FROM [dbo].[Organization.Member] om inner join [Assessment_Summary] cs on om.ParentOrgId = cs.ManagingOrgId where userId = {1}) ))", CF.CodesManager.ENTITY_STATUS_PUBLISHED, user.Id );

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
			string template = " ( base.Id in (SELECT distinct  AssessmentId FROM [dbo].Assessment_Competency_Summary  where AlignmentType = 'assesses' AND ({0}) ) )";
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
			string AND = "";
			string searchCategories = UtilityManager.GetAppKeyValue( "asmtSearchCategories", "21,37," );
			string template = " ( base.Id in ( SELECT  [EntityBaseId] FROM [dbo].[EntityProperty_Summary] where EntityTypeId= 3 AND [PropertyValueId] in ({0}))) ";
			//what are the valid categories: delivery types.
			//Where( s => s.CategoryId == 16 || s.CategoryId == 18 ) 

			//Updated to use FiltersV2
			string next = "";
			if ( where.Length > 0 )
				AND = " AND ";
			foreach ( var filter in data.FiltersV2.Where(m => m.Type == MainSearchFilterV2Types.CODE ) )
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
		} //

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
			AssessmentProfile entity = Mgr.Assessment_Get( id, false, false );
			return entity;
		}
		public static AssessmentProfile GetDetail( int id )
		{
			AppUser user = AccountServices.GetCurrentUser();
			return GetDetail( id, user );

		}
		public static AssessmentProfile GetDetail( int id, AppUser user )
		{
			string statusMessage = "";
			AssessmentProfile entity = Mgr.Assessment_Get( id, false, true );
			if ( CanUserUpdateAssessment( entity, user, ref statusMessage ) )
				entity.CanUserEditEntity = true;

			return entity;
		}
		public static AssessmentProfile GetForEdit( int id )
		{
			AssessmentProfile entity = Mgr.Assessment_Get( id, true, true );


			return entity;
		}
		public static AssessmentProfile GetBasic( int asmtId )
		{
			AssessmentProfile entity = Mgr.GetBasic( asmtId );


			return entity;
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

		#endregion 
		#region === add/update/delete =============
		public static bool CanUserUpdateAssessment( int asmtId, ref string status )
		{
			AppUser user = AccountServices.GetCurrentUser();
			if ( user == null || user.Id == 0 )
				return false;

			if ( asmtId == 0 )
				return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;
			AssessmentProfile entity = GetLightAssessmentById( asmtId );

			return CanUserUpdateAssessment( entity, user, ref status );
		}
		public static bool CanUserUpdateAssessment( int asmtId, AppUser user, ref string status )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( AccountServices.IsUserSiteStaff( user ) )
				return true;
			AssessmentProfile entity = GetLightAssessmentById( asmtId );

			return CanUserUpdateAssessment( entity, user, ref status );
		}

		public static bool CanUserUpdateAssessment( Guid entityUid, AppUser user, ref string status )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( AccountServices.IsUserSiteStaff( user ) )
				return true;
			AssessmentProfile entity = GetLightAssessmentByRowId( entityUid.ToString() );

			return CanUserUpdateAssessment( entity, user, ref status );
		}
		public static bool CanUserUpdateAssessment( AssessmentProfile entity, AppUser user, ref string status )
		{
			bool isValid = false;
			if ( entity.Id == 0 )
				return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;

			//is a member of the assessment managing organization 
			if ( OrganizationServices.IsOrganizationMember( user.Id, entity.ManagingOrgId ) )
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
			if ( entity.StatusId == CF.CodesManager.ENTITY_STATUS_PUBLISHED )
				return true;

			if ( user == null || user.Id == 0 )
				return false;
			else if ( AccountServices.CanUserViewAllOfSite( user ) )
				return true;

			//is a member of the assessment managing organization 
			if ( OrganizationServices.IsOrganizationMember( user.Id, entity.ManagingOrgId ) )
				return true;
			
			return isValid;
		}
		/// <summary>
		/// Add a Assessment stack
		/// ??what to return - given the jumbotron form
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public int Add( AssessmentProfile entity, AppUser user,ref string statusMessage )
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
					if ( id > 0 )
					{
						statusMessage = "Successfully Added Assessment";
						new ActivityServices().AddActivity( "Assessment", "Add", string.Format( "{0} added a new Assessment: {1}", user.FullName(), entity.Name ), entity.CreatedById, 0, id );
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
			if ( !CanUserUpdateAssessment( entity, user, ref statusMessage ) )
			{
				return false;
			}
			statusMessage = "";
			Mgr mgr = new Mgr();
			bool isOK = false;
			try
			{
				isOK = mgr.Update( entity, ref statusMessage );
				if ( isOK )
				{
					statusMessage = "Successfully Updated Assessment";
					new ActivityServices().AddActivity( "Assessment", "Update", string.Format( "{0} updated Assessment (or parts of): {1}", user.FullName(), entity.Name ), user.Id, 0, entity.Id );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "AssessmentServices.Update" );
			}
			return isOK;
		}

		public bool Delete( int assessmentId, int userId, ref string statusMessage )
		{
			bool isOK = false;
			statusMessage = "";
			return Delete( assessmentId, userId, ref isOK, ref statusMessage );
		}

		/// <summary>
		/// to do - add logging
		/// </summary>
		/// <param name="assessmentId"></param>
		/// <param name="valid"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool Delete( int assessmentId, int userId, ref bool valid, ref string status )
		{
			Mgr mgr = new Mgr();
			try
			{
				valid = mgr.Delete( assessmentId, ref status );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "AssessmentServices.Assessment_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		//public bool DeleteProfile( int profileId, string profileName, AppUser user, ref bool valid, ref string status )
		//{

		//	try
		//	{
		//		switch ( profileName.ToLower() )
		//		{

		//			case "durationprofile":
		//				valid = new CF.DurationProfileManager().DurationProfile_Delete( profileId, ref status );
		//				break;
		//			case "geocoordinates":
		//				valid = new CF.RegionsManager().GeoCoordinate_Delete( profileId, ref status );
		//				break;
		//			case "jurisdictionprofile":
		//				valid = new CF.RegionsManager().JurisdictionProfile_Delete( profileId, ref status );
		//				break;
		//			case "organizationrole":
		//			case "entityrole":
		//				valid = new CF.Entity_AgentRelationshipManager().EntityAgentRole_Delete( profileId, ref status );
		//				break;
		//			case "costprofilesplit":
		//				valid = new CF.CostProfileManager().CostProfile_Delete( profileId, ref status );
		//				break;
		//			case "costprofileitem":
		//				valid = new CF.CostProfileItemManager().CostProfileItem_Delete( profileId, ref status );
		//				break;
		//			default:
		//				valid = false;
		//				status = "Deleting the requested clientProfile is not handled at this time.";
		//				return false;
		//		}
			
		//		if ( valid )
		//		{
		//			//if valid, status contains the cred name and id
		//			ActivityServices.SiteActivityAdd( "Assessment Profile", "Delete profileName", string.Format( "{0} deleted Assessment clientProfile {1}", user.FullName(), profileName ), user.Id, 0, profileId );
		//			status = "";
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, "AssessmentServices.DeleteProfile" );
		//		status = ex.Message;
		//		valid = false;
		//	}

		//	return valid;
		//}
		#endregion
	}
}
