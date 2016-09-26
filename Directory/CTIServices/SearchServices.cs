using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CTIServices;
using CF = Factories;
using Models;
using Models.Search;
using Models.Common;
using Models.ProfileModels;

namespace CTIServices
{
	public class SearchServices
	{
		public MainSearchResults MainSearch( MainSearchInput data, ref bool valid, ref string status )
		{
			//Sanitize input
			data.Keywords = string.IsNullOrWhiteSpace( data.Keywords ) ? "" : data.Keywords;
			data.Keywords = ServiceHelper.CleanText( data.Keywords );
			data.Keywords = ServiceHelper.HandleApostrophes( data.Keywords );
			data.Keywords = data.Keywords.Trim();

			//Sanitize input
			//var sortOrder = "relevance";
			//switch ( data.SortOrder )
			//{
			//	case "relevance": sortOrder = "relevance"; break;
			//	case "alpha": sortOrder = "alpha"; break;
			//	default: break;
			//}
			//data.SortOrder = sortOrder;

			//Determine search type
			var searchType = data.SearchType;
			if ( string.IsNullOrWhiteSpace( searchType ) )
			{
				valid = false;
				status = "Unable to determine search mode";
				return null;
			}

			//Do the search
			var totalResults = 0;
			switch ( searchType )
			{
				case "credential": 
					{
						var qaSettings = data.GetFilterValues_Strings( "qualityAssurance" );

						var results = CredentialServices.Search( data, ref totalResults ); 
						return ConvertCredentialResults( results, totalResults, searchType );
					}
				case "organization":
					{
						var qaSettings = data.GetFilterValues_Strings( "qualityAssurance" );

						var results = OrganizationServices.Search( data, ref totalResults ); 
						return ConvertOrganizationResults( results, totalResults, searchType );
					}
				case "assessment":
					{
						var results = AssessmentServices.Search( data, ref totalResults );
						return ConvertAssessmentResults( results, totalResults, searchType );
					}
				case "learningopportunity":
					{
						//var results = LearningOpportunityServices.Search( data.Keywords, data.StartPage, data.PageSize, ref totalResults );
						var results = LearningOpportunityServices.Search( data, ref totalResults );
						return ConvertLearningOpportunityResults( results, totalResults, searchType );
					}
				default:
					{
						valid = false;
						status = "Unknown search mode: " + searchType;
						return null;
					}
			}
		}
		//

		//Do an autocomplete
		public static List<string> DoAutoComplete( string text, string context, string searchType )
		{
		var results = new List<string>();

			switch ( searchType.ToLower() )
			{
				case "credential":
					{
						switch ( context.ToLower() )
						{
							//case "mainsearch": return CredentialServices.Autocomplete( text, 10 ).Select( m => m.Name ).ToList();
							case "mainsearch":
								return CredentialServices.Autocomplete( text, 10 );
							//case "competencies":
							//	return CredentialServices.Autocomplete( text, "competencies", 10 );
							case "subjects":
								return Autocomplete_Subjects( CF.CodesManager.ENTITY_TYPE_CREDENTIAL, CF.CodesManager.PROPERTY_CATEGORY_SUBJECT, text, 10 );
							default: break;
						}
						break;
					}
				case "organization":
					{
						return OrganizationServices.Autocomplete( text, 10 );
					}
				case "assessment":
					{
						switch ( context.ToLower() )
						{
							case "mainsearch":
								return AssessmentServices.Autocomplete( text, 10 );
							//case "competencies":
							//	return AssessmentServices.Autocomplete( text, "competencies", 10 );
							case "subjects":
								return Autocomplete_Subjects( CF.CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, CF.CodesManager.PROPERTY_CATEGORY_SUBJECT, text, 10 );
							default: break;
						}
						break;
					}
				case "learningopportunity":
					{
						switch ( context.ToLower() )
						{
							case "mainsearch":
								return LearningOpportunityServices.Autocomplete( text, 10 );
							//case "competencies":
							//	return LearningOpportunityServices.Autocomplete( text, "competencies", 10 );
							case "subjects":
								return Autocomplete_Subjects( CF.CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, CF.CodesManager.PROPERTY_CATEGORY_SUBJECT, text, 10 );
							default: break;
						}
						break;
					}
				default: break;
			}

			return results;
		}
		//

		//Convenience method to handle location data
		//For convenience, check boundaries.IsDefined to see if a boundary is defined
		public static BoundingBox GetBoundaries( MainSearchInput data, string name )
		{
			var boundaries = new BoundingBox();
			try
			{
				boundaries = data.Filters.FirstOrDefault( m => m.Name == name ).Boundaries;
				//boundaries = ( BoundingBox ) item;
			}
			catch { }

			return boundaries;
		}
		//

		public MainSearchResults ConvertCredentialResults( List<CredentialSummary> results, int totalResults, string searchType )
		{
			var output = new MainSearchResults() { TotalResults = totalResults, SearchType = searchType };
			foreach ( var item in results )
			{
				output.Results.Add( Result( item.Name, item.Description, item.Id,
					new Dictionary<string, object>()
					{
						{ "Creator", item.CreatorOrganizationName },
						{ "CreatorId", item.CreatorOrganizationId },
						{ "Owner", item.OwnerOrganizationName },
						{ "OwnerId", item.OwnerOrganizationId },
						{ "Type", item.CredentialType },
						{ "CanEditRecord", item.CanEditRecord },
						{ "TypeSchema", item.CredentialTypeSchema.ToLower()},
						{ "Industry", new { Type = "tags", Title = "Industr" + (item.NaicsList.Count() == 1 ? "y" : "ies"), Data = ConvertCodeItemsToDictionary( item.NaicsList ) } },
						{ "Level", new { Type = "tags", Title = "Level" + (item.LevelsList.Count() == 1 ? "" : "s"), Data = ConvertCodeItemsToDictionary( item.LevelsList ) } },
						{ "Cost", "Estimated Cost Placeholder" },
						{ "Time", "Estimated Time to Complete" },
						{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( item.Addresses ) } }
					}
				) );
			}
			return output;
		}
		//

		public MainSearchResults ConvertOrganizationResults( List<Organization> results, int totalResults, string searchType )
		{
			var output = new MainSearchResults() { TotalResults = totalResults, SearchType = searchType };
			foreach ( var item in results )
			{
				output.Results.Add( Result( item.Name, item.Description, item.Id,
					new Dictionary<string, object>()
					{
						{ "Location", item.Address.Country + ( string.IsNullOrWhiteSpace( item.Address.Country ) ? "" : " - " ) + item.Address.City + ( string.IsNullOrWhiteSpace( item.Address.City ) ? "" : ", " ) + item.Address.AddressRegion },
						{ "Coordinates", new { Type = "coordinates", Data = new { Latitude = item.Address.Latitude, Longitude = item.Address.Longitude } } },
						{ "IsQA", item.IsAQAOrg ? "true" : "false" },
						{ "CanEditRecord", item.CanEditRecord },
						{ "Logo", item.ImageUrl },
						//{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( item.Addresses ) } },
						{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( new List<Address>() { item.Address } ) } }

					}
				) );
			}
			return output;
		}
		//

		public MainSearchResults ConvertAssessmentResults( List<AssessmentProfile> results, int totalResults, string searchType )
		{
			var output = new MainSearchResults() { TotalResults = totalResults, SearchType = searchType };
			foreach ( var item in results )
			{
				output.Results.Add( Result( item.Name, item.Description, item.Id,
					new Dictionary<string, object>()
					{
						{ "CanEditRecord", item.CanEditRecord },
						{ "Owner", string.IsNullOrWhiteSpace( item.CreatedByOrganization ) ? "" : item.CreatedByOrganization },
						{ "OwnerId", item.CreatedByOrganizationId },
						{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( item.Addresses ) } }
					}
				) );
			}
			return output;
		}
		//

		public MainSearchResults ConvertLearningOpportunityResults( List<LearningOpportunityProfile> results, int totalResults, string searchType )
		{
			var output = new MainSearchResults() { TotalResults = totalResults, SearchType = searchType };
			foreach ( var item in results )
			{
				output.Results.Add( Result( item.Name, item.Description, item.Id,
					new Dictionary<string, object>()
					{
						{ "CanEditRecord", item.CanEditRecord },
						{ "Owner", string.IsNullOrWhiteSpace( item.CreatedByOrganization ) ? "" : item.CreatedByOrganization },
						{ "OwnerId", item.CreatedByOrganizationId },
						{ "Competencies", new { Type = "tags", Data = ConvertCompetenciesToDictionary( item.TeachesCompetencies ) } },
						{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( item.Addresses ) } }
					}
				) );
			}
			return output;
		}
		//
		public Dictionary<string, string> ConvertCompetenciesToDictionary( List<CredentialAlignmentObjectProfile> input )
		{
			var result = new Dictionary<string, string>();
			if ( input != null )
			{
				foreach ( var item in input )
				{
					try
					{
						result.Add( item.Id.ToString(), item.Description );
					}
					catch { }
				}
			}
			return result;
		}
		public MainSearchResult Result( string name, string description, int recordID, Dictionary<string, object> properties )
		{
			return new MainSearchResult()
			{
				Name = string.IsNullOrWhiteSpace( name ) ? "No name" : name,
				Description = string.IsNullOrWhiteSpace( description ) ? "No description" : description,
				RecordId = recordID,
				Properties = properties == null ? new Dictionary<string, object>() : properties
			};
		}
		//

		public Dictionary<string, string> ConvertCodeItemsToDictionary( List<CodeItem> input )
		{
			var result = new Dictionary<string, string>();
			foreach ( var item in input )
			{
				try
				{
					result.Add( item.Code, item.Name );
				}
				catch { }
			}
			return result;
		}
		//

		public List<Dictionary<string, object>> ConvertAddresses( List<Address> input )
		{
			var result = new List<Dictionary<string, object>>();
			foreach ( var item in input )
			{
				try
				{
					var data = new Dictionary<string, object>()
					{
						{ "Latitude", item.Latitude },
						{ "Longitude", item.Longitude },
						{ "Address", item.DisplayAddress() }
					};
					result.Add( data );
				}
				catch { }
			}
			return result;
		}
		//

		public static List<string> Autocomplete_Subjects( int entityTypeId, int categoryId, string keyword, int maxTerms = 25 )
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
			CF.Entity_ReferenceManager.QuickSearch_TextValue( entityTypeId, categoryId, keyword, maxTerms );

			return list;
		}

		#region Common filters
		/// <summary>
		/// Generic
		/// </summary>
		/// <param name="data"></param>
		/// <param name="where"></param>
		public static void SetSubjectsFilter( MainSearchInput data, string entity, ref string where )
		{
			string subjects = " (base.RowId in (SELECT b.EntityUid FROM [dbo].[Entity.Reference] a inner join Entity b on a.EntityId = b.Id inner join {0} c on b.EntityUid = c.RowId where [CategoryId] = 34 and ({1}) )) ";
			string phraseTemplate = " (a.TextValue like '%{0}%') ";

			string AND = "";
			string OR = "";
			string keyword = "";

			foreach ( MainSearchFilter filter in data.Filters.Where( s => s.Name == "subjects" ) )
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
						foreach ( string word in words )
						{
							next += OR + string.Format( phraseTemplate, word.Trim() );
							OR = " OR ";
						}
					}
					else
					{
						next = string.Format( phraseTemplate, keyword.Trim() );
					}
					//next += keyword;	//					+",";
					//just handle one for now
					break;
				}
				//next = next.Trim( ',' );
				if ( !string.IsNullOrWhiteSpace( next ) )
					where = where + AND + string.Format( subjects, entity, next );

				break;
			}
		}
		public static void SetSubjectsAutocompleteFilter( string keywords, string entity, ref string where )
		{
			string subjects = " (base.RowId in (SELECT b.EntityUid FROM [dbo].[Entity.Reference] a inner join Entity b on a.EntityId = b.Id inner join {0} c on b.EntityUid = c.RowId where [CategoryId] = 34 and a.TextValue like '{1}' )) ";

			string AND = "";
			keywords = ServiceHelper.HandleApostrophes( keywords );
			if ( keywords.IndexOf( "%" ) == -1 )
				keywords = "%" + keywords.Trim() + "%";

			where = where + AND + string.Format( " ( " + subjects + " ) ", entity, keywords );
		}
		public static void SetBoundariesFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";
			string template = " ( base.RowId in ( SELECT  b.EntityUid FROM [dbo].[Entity.Address] a inner join Entity b on a.EntityId = b.Id    where [Longitude] < {0} and [Longitude] > {1} and [Latitude] < {2} and [Latitude] > {3} ) ) ";

			var boundaries = SearchServices.GetBoundaries( data, "bounds" );
			if ( boundaries.IsDefined )
			{
				where = where + AND + string.Format( template, boundaries.East, boundaries.West, boundaries.North, boundaries.South );
			}
		}
		//
		public static void SetRolesFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";
			string template = " ( base.RowId in ( SELECT distinct b.EntityUid FROM [dbo].[Entity.AgentRelationship] a inner join Entity b on a.EntityId = b.Id   where [RelationshipTypeId] in ({0})   ) ) ";

			foreach ( MainSearchFilter filter in data.Filters.Where( s => s.CategoryId == 13  ) )
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

		public static void SetOrgRolesFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";
			string template = " ( base.RowId in ( SELECT distinct EntityUid FROM [dbo].[Entity.AgentRelationship] a inner join Entity b on a.EntityId = b.Id   where [RelationshipTypeId] in ({0})   ) ) ";

			foreach ( MainSearchFilter filter in data.Filters.Where( s => s.CategoryId == 13 ) )
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

		private static void SetAuthorizationFilter( AppUser user, string summaryView, ref string where )
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
			where = where + AND + string.Format( "((base.StatusId = {0}) OR (base.Id in (SELECT cs.Id FROM [dbo].[Organization.Member] om inner join [{1}] cs on om.ParentOrgId = cs.ManagingOrgId where userId = {2}) ))", CF.CodesManager.ENTITY_STATUS_PUBLISHED, summaryView,user.Id );

		}
		#endregion 
	}
}
