using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CTIServices;
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
			var sortOrder = "relevance";
			switch ( data.SortOrder )
			{
				case "relevance": sortOrder = "relevance"; break;
				case "alpha": sortOrder = "alpha"; break;
				default: break;
			}
			data.SortOrder = sortOrder;

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
	}
}
