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
using Mgr = Factories.LearningOpportunityManager;

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
		public static LearningOpportunityProfile GetForEdit( int id )
		{
			LearningOpportunityProfile entity = Mgr.Get( id, true, false );

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
		public static LearningOpportunityProfile GetForDetail( int id, AppUser user )
		{
			LearningOpportunityProfile entity = Mgr.GetForDetail( id );
			string status = "";
			if ( CanUserUpdateLearningOpportunity( entity.RowId, user, ref status ) )
				entity.CanUserEditEntity = true;
			return entity;
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
			string where = string.Format( " base.Id = {0}", learningOpportunityId );
			int pTotalRows = 0;

			List<LearningOpportunityProfile> list = Mgr.Search( where, "", 1, 50, 0, ref pTotalRows );

			if ( list.Count > 0 )
				return list[ 0 ];
			else
				return null;
		}
		#endregion


		#region Searches
		public static List<CodeItem> SearchAsCodeItem( string keyword, int startingPageNbr, int pageSize, ref int totalRows )
		{
			List<LearningOpportunityProfile> list = Search( keyword, startingPageNbr, pageSize, ref totalRows );
			List<CodeItem> codes = new List<CodeItem>();
			foreach ( LearningOpportunityProfile item in list )
			{
				codes.Add( new CodeItem()
				{
					Id = item.Id,
					Name = item.Name,
					Description = item.Description
				} );
			}
			return codes;
		}
		public static List<string> Autocomplete( string keyword, int maxTerms = 25 )
		{
			int userId = 0;
			string where = "";
			int totalRows = 0;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;
			SetAuthorizationFilter( user, ref where );

			//if ( type == "learningopp" ) 
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
			;
		}
		public static List<LearningOpportunityProfile> Search( string keywords, int startingPageNbr, int pageSize, ref int totalRows )
		{
			MainSearchInput data = new MainSearchInput();
			data.Keywords = keywords;
			data.StartPage = startingPageNbr;
			data.PageSize = pageSize;

			return Search( data, ref totalRows );
		}
		public static List<LearningOpportunityProfile> Search( MainSearchInput data, ref int totalRows )
		{
			string where = "";
			int userId = 0;
			List<string> competencies = new List<string>();
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;

			//string pOrderBy = "";
			SearchServices.HandleCustomFilters( data, 61, ref where );

			SetKeywordFilter( data.Keywords, false, ref where );
			SearchServices.SetSubjectsFilter( data, CF.CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, ref where );
			SetAuthorizationFilter( user, ref where );

			SetPropertiesFilter( data, ref where );
			SearchServices.SetRolesFilter( data, ref where );
			SearchServices.SetBoundariesFilter( data, ref where );
			//SetBoundariesFilter( data, ref where );

			//CIP
			SetFrameworksFilter( data, ref where );

			//Competencies
			SetCompetenciesFilter( data, ref where, ref competencies );

			LoggingHelper.DoTrace( 5, "LearningOpportunityServices.Search(). Filter: " + where );
			return Mgr.Search( where, data.SortOrder, data.StartPage, data.PageSize, userId, ref totalRows, ref competencies );
		}
		private static void SetKeywordFilter( string keywords,  bool isBasic, ref string where )
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
			string subjectsEtc = " OR (base.Id in (SELECT c.id FROM [dbo].[Entity.Reference] a inner join Entity b on a.EntityId = b.Id inner join LearningOpportunity c on b.EntityUid = c.RowId where [CategoryId] in (34 ,35) and a.TextValue like '{0}' )) ";
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
			if ( isBasic || isCustomSearch )
				where = where + AND + string.Format( " ( " + text + " ) ", keywords );
			else 
				where = where + AND + string.Format( " ( " + text + subjectsEtc + competencies + " ) ", keywords );
				

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
			where = where + AND + string.Format( "((base.StatusId = {0}) OR (base.Id in (SELECT cs.Id FROM [dbo].[Organization.Member] om inner join [LearningOpportunity_Summary] cs on om.ParentOrgId = cs.ManagingOrgId where userId = {1}) ))", CF.CodesManager.ENTITY_STATUS_PUBLISHED, user.Id );

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
			string template = " ( base.Id in (SELECT distinct LearningOpportunityId FROM [dbo].LearningOpportunity_Competency_Summary  where AlignmentType = 'teaches' AND ({0}) ) )";
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
				if ( !string.IsNullOrWhiteSpace(next))
					where = where + AND + string.Format( template, next );
			}
			*/
		}
		//

		private static void SetPropertiesFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			string searchCategories = UtilityManager.GetAppKeyValue( "loppSearchCategories", "21,37," );
			string template = " ( base.Id in ( SELECT  [EntityBaseId] FROM [dbo].[EntityProperty_Summary] where EntityTypeId= 7 AND [PropertyValueId] in ({0}))) ";
			//.Where( s => s.CategoryId == 21) 

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
			foreach ( MainSearchFilter filter in data.Filters ) 
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
#endregion 
		#region === add/update/delete =============
		public static bool CanUserUpdateLearningOpportunity( int entityId, ref string status )
		{
			AppUser user = AccountServices.GetCurrentUser();
			if ( user == null || user.Id == 0 )
				return false;

			if ( entityId == 0 )
				return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;
			LearningOpportunityProfile entity = GetLightLearningOpportunityById( entityId );

			return CanUserUpdateLearningOpportunity( entity, user, ref status );
		}
		public static bool CanUserUpdateLearningOpportunity( Guid entityUid, AppUser user, ref string status )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( AccountServices.IsUserSiteStaff( user ) )
				return true;
			LearningOpportunityProfile entity = GetLightLearningOpportunityByRowId( entityUid.ToString() );

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
			LearningOpportunityProfile entity = GetLightLearningOpportunityById( learningOpportunityId );

			return CanUserUpdateLearningOpportunity( entity, user, ref status );
		}
		public static bool CanUserUpdateLearningOpportunity( LearningOpportunityProfile entity, AppUser user, ref string status )
		{
			if ( user == null || user.Id == 0 )
				return false;

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

		public static bool CanUserViewLearningOpportunity( LearningOpportunityProfile entity, AppUser user, ref string status )
		{
			bool isValid = false;
			status = "Error - you do not have view access for this record.";
			if ( entity == null || entity.Id == 0 )
			{
				status = "Learning Opportunity was Not found";
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
		/// Add a LearningOpportunity stack
		/// ??what to return - given the jumbotron form
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public int Add( LearningOpportunityProfile entity, AppUser user,ref string statusMessage )
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


				id = mgr.Add( entity, entity.LastUpdatedById, ref statusMessage );
				if ( id > 0 )
				{
					statusMessage = "Successfully Added Learning Opportunity";
					new ActivityServices().AddActivity( "Learning Opportunity", "Add", string.Format( "{0} added a new Learning Opportunity: {1}", user.FullName(), entity.Name ), entity.CreatedById, 0, id );
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

			if ( !CanUserUpdateLearningOpportunity( entity, user, ref statusMessage ) )
			{
				return false;
			}
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
				isOK = mgr.Update( entity, entity.LastUpdatedById, ref statusMessage );

				if ( isOK )
				{
					statusMessage = "Successfully Updated Learning Opportunity";
					new ActivityServices().AddActivity( "Learning Opportunity", "Update", string.Format( "{0} updated Learning Opportunity (or parts of): {1}", user.FullName(), entity.Name ), user.Id, 0, entity.Id );
				}

				CF.CacheManager.RemoveItemFromCache( "LearningOpportunity", entity.Id );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "LearningOpportunityServices.LearningOpportunity_Update" );
			}
			return isOK;
		}

		public bool Delete( int recordId, int userId, ref string statusMessage )
		{
			bool isOK = false;
			statusMessage = "";
			return Delete( recordId, userId, ref isOK, ref statusMessage );
		}

		/// <summary>
		/// to do - add logging
		/// </summary>
		/// <param name="assessmentId"></param>
		/// <param name="valid"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool Delete( int recordId, int userId, ref bool valid, ref string status )
		{
			Mgr mgr = new Mgr();
			try
			{
				valid = mgr.Delete( recordId, ref status );
				CF.CacheManager.RemoveItemFromCache( "LearningOpportunity", recordId );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "LearningOpportunityServices.LearningOpportunity_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
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
					ActivityServices.SiteActivityAdd( "Learning Opportunity Profile", "Add Learning Opportunity", string.Format( "{0} added Learning Opportunity part {1} to Learning Opportunity {2}", user.FullName(), childLearningOppId, parent.Id ), user.Id, 0, childLearningOppId );
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
					ActivityServices.SiteActivityAdd( "Connection Profile", "Delete Learning Opportunity Part", string.Format( "{0} deleted Learning Opportunity part {1} from Learning Opportunity Profile  {2}", user.FullName(), recordId, parent.Id ), user.Id, 0, recordId );
					status = "";
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
