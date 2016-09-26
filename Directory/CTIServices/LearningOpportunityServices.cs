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
		public static LearningOpportunityProfile GetForEdit( int id )
		{
			LearningOpportunityProfile entity = Mgr.Get( id, true, false );

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

		public static LearningOpportunityProfile GetLightLearningOpportunityById( int entityId )
		{
			if ( entityId < 1 )
				return null;
			string where = string.Format( " base.Id = {0}", entityId );
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
			SetKeywordFilter( data.Keywords, false, ref where );
			SearchServices.SetSubjectsFilter( data, "LearningOpportunity", ref where );
			SetAuthorizationFilter( user, ref where );

			SetPropertiesFilter( data, ref where );
			SearchServices.SetRolesFilter( data, ref where );
			SearchServices.SetBoundariesFilter( data, ref where );
			//SetBoundariesFilter( data, ref where );

			//CIP
			SetFrameworksFilter( data, ref where );

			//Competencies
			SetCompetenciesFilter( data, ref where, ref competencies );

			return Mgr.Search( where, data.SortOrder, data.StartPage, data.PageSize, userId, ref totalRows, ref competencies );
		}
		private static void SetKeywordFilter( string keywords,  bool isBasic, ref string where )
		{
			if ( string.IsNullOrWhiteSpace( keywords ) )
				return;
			string text = " (base.name like '{0}' OR base.Description like '{0}'  OR base.Organization like '{0}' OR base.owingOrganization like '{0}' ) ";
			string subjectsEtc = " OR (base.Id in (SELECT c.id FROM [dbo].[Entity.Reference] a inner join Entity b on a.EntityId = b.Id inner join LearningOpportunity c on b.EntityUid = c.RowId where [CategoryId] in (34 ,35) and a.TextValue like '{0}' )) ";
			string competencies = " OR ( base.Id in (SELECT LearningOpportunityId FROM [dbo].LearningOpportunity_Competency_Summary  where [Description] like '{0}' ) ) ";
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
				if ( !string.IsNullOrWhiteSpace(next))
					where = where + AND + string.Format( template, next );
			}
		}
		//

		private static void SetPropertiesFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			string template = " ( base.RowId in ( SELECT  [ParentUid] FROM [dbo].[Entity.Property] where [PropertyValueId] in ({0}))) ";
			//what are the valid categories: delivery types
			foreach ( MainSearchFilter filter in data.Filters.Where( s => s.CategoryId == 21  ) )
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
			string codeTemplate = "  (base.Id in (SELECT c.id FROM [dbo].[Entity.FrameworkItemSummary] a inner join Entity b on a.EntityId = b.Id inner join LearningOpportunity c on b.EntityUid = c.RowId where [CategoryId] = {0} and [FrameworkGroup] in ({1}))  ) ";
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
		public static bool CanUserUpdateLearningOpportunity( int entityId, AppUser user, ref string status )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( entityId == 0 )
				return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;
			LearningOpportunityProfile entity = GetLightLearningOpportunityById( entityId );

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
				entity.ManagingOrgId = CF.OrganizationManager.GetPrimaryOrganizationId( entity.CreatedById );

				id = mgr.Add( entity, entity.LastUpdatedById, ref statusMessage );
				if ( id > 0 )
					statusMessage = "Successfully Added Learning Opportunity";
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
					statusMessage = "Successfully Updated Learning Opportunity";

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

		//[Obsolete]
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
		//			case "costprofilesplit":
		//				valid = new CF.CostProfileManager().CostProfile_Delete( profileId, ref status );
		//				break;
		//			case "CostProfileItem":
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
		//			ActivityServices.SiteActivityAdd( "LearningOpportunity", "Delete profileName", string.Format( "{0} deleted Learning Opportunity Profile {1}", user.FullName(), profileName ), user.Id, 0, profileId );
		//			status = "";
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, "LearningOpportunityServices.DeleteProfile" );
		//		status = ex.Message;
		//		valid = false;
		//	}

		//	return valid;
		//}
		#endregion

		#region Entity FrameworkItems==> moved TO Profile services
		//public EnumeratedItem FrameworkItem_Add( int parentId, int categoryId, int codeID, string searchType, ref bool valid, ref string status )
		//{

		//	AppUser user = AccountServices.GetCurrentUser();
		//	if ( user == null || user.Id == 0 )
		//	{
		//		valid = false;
		//		status = "Error - you must be authenticated in order to update data";
		//		return new EnumeratedItem();
		//	}
		//	return FrameworkItem_Add( parentId, categoryId, codeID, user, ref valid, ref status );
		//}
		//public EnumeratedItem FrameworkItem_Add( int parentId, int categoryId, int codeID, AppUser user, ref bool valid, ref string status )
		//{
		//	if ( parentId == 0 || categoryId == 0 || codeID == 0 )
		//	{
		//		valid = false;
		//		status = "Error - invalid request - missing code identifiers";
		//		return new EnumeratedItem();
		//	}

		//	CF.Entity_FrameworkItemManager mgr = new CF.Entity_FrameworkItemManager();
		//	int frameworkItemId = 0;
		//	EnumeratedItem item = new EnumeratedItem();
		//	try
		//	{
		//		EntitySummary e = CF.EntityManager.GetEntitySummary( parentId, CF.CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE );

		//		frameworkItemId = mgr.ItemAdd( parentId, categoryId, codeID, user.Id, ref status );

		//		if ( frameworkItemId > 0 )
		//		{
		//			//get full item, as a codeItem to return
		//			item = CF.Entity_FrameworkItemManager.ItemGet( frameworkItemId );
		//			//if valid, status contains the cred id, category, and codeId
		//			ActivityServices.SiteActivityAdd( "LearningOpportunity FrameworkItem", "Add item", string.Format( "{0} added Learning Opportunity FrameworkItem. ParentId: {1}, categoryId: {2}, codeId: {3}, summary: {4}", user.FullName(), parentId, categoryId, codeID, item.ItemSummary ), user.Id, 0, frameworkItemId );
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

		//public bool FrameworkItem_Delete( int frameworkItemId, AppUser user, ref bool valid, ref string status )
		//{
		//	CF.Entity_FrameworkItemManager mgr = new CF.Entity_FrameworkItemManager();

		//	try
		//	{
		//		valid = mgr.ItemDelete( frameworkItemId, ref status );

		//		if ( valid )
		//		{
		//			//if valid, status contains the cred id, category, and codeId
		//			ActivityServices.SiteActivityAdd( "Learning Opportunity FrameworkItem", "Delete item", string.Format( "{0} deleted credential FrameworkItem {1}", user.FullName(), frameworkItemId ), user.Id, 0, frameworkItemId );
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

		#region Learning Opportunity parts
		public int AddLearningOpportunity_AsPart( int parentId, int recordId, AppUser user, ref bool valid, ref string status )
		{
			int id = 0;
			List<string> messages = new List<string>();
			CF.Entity_LearningOpportunityManager mgr = new CF.Entity_LearningOpportunityManager();
			try
			{
				id = mgr.EntityLearningOpp_Add( parentId, CF.CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, recordId, user.Id, ref messages );

				if ( id > 0 )
				{
					//if valid, status contains the cred id, category, and codeId
					ActivityServices.SiteActivityAdd( "Learning Opportunity Profile", "Add Learning Opportunity", string.Format( "{0} added Learning Opportunity part {1} to Learning Opportunity {2}", user.FullName(), recordId, parentId ), user.Id, 0, recordId );
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
				LoggingHelper.LogError( ex, thisClassName + ".AddLearningOpportunity_AsPart" );
				status = ex.Message;
				valid = false;
			}

			return id;
		}

		public bool DeleteLearningOpportunityPart( int parentId, int recordId, AppUser user, ref string status )
		{
			bool valid = true;

			CF.Entity_LearningOpportunityManager mgr = new CF.Entity_LearningOpportunityManager();
			try
			{
				valid = mgr.EntityLearningOpp_Delete( parentId, CF.CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, recordId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					ActivityServices.SiteActivityAdd( "Connection Profile", "Delete Learning Opportunity Part", string.Format( "{0} deleted Learning Opportunity part {1} from Learning Opportunity Profile  {2}", user.FullName(), recordId, parentId ), user.Id, 0, recordId );
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
