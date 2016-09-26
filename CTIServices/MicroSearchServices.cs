using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CTIServices;
using Models.Search;
using Factories;
using Models;
using Models.Common;
using Models.ProfileModels;
using Models.Search.ThirdPartyApiModels;

namespace CTIServices
{
	public class MicroSearchServices
	{

		//Do a micro search and return results
		public MicroSearchResults MicroSearch( MicroSearchInput data, ref bool valid, ref string status )
		{
			if ( data.Filters.Count() == 0 )
			{
				valid = false;
				status = "No parameters found!";
				return null;
			}

			foreach ( var item in data.Filters )
			{
				item.Name = ServiceHelper.CleanText( item.Name ?? "" );
				item.Value = ServiceHelper.CleanText( item.Value as string ?? "" );
			}

		var totalResults = 0;
			
			switch ( data.SearchType )
			{
				case "JurisdictionSearch":
					{
						var locationType = data.GetFilterValueString( "LocationType" ).Split( ',' ).ToList();
						var results = new ThirdPartyApiServices().GeoNamesSearch( data.GetFilterValueString( "Keywords" ), data.PageNumber, data.PageSize, locationType, ref totalResults, false );
						return ConvertGeoNamesSearchToResults( results, totalResults );
					}
				case "IndustrySearch":
					{
						var results = EnumerationServices.NAICS_Search( data.GetFilterValueInt( "HeaderId" ), data.GetFilterValueString( "Keywords" ), data.PageNumber, data.PageSize, ref totalResults );
						return ConvertCodeSearchToResults( results, totalResults );
					}
				case "OccupationSearch":
					{
						var results = CodesManager.SOC_Search( data.GetFilterValueInt( "HeaderId" ), data.GetFilterValueString( "Keywords" ), data.PageNumber, data.PageSize, ref totalResults );
						return ConvertCodeSearchToResults( results, totalResults );
					}
				case "CIPSearch":
					{
						var results = CodesManager.CIPS_Search( data.GetFilterValueInt( "HeaderId" ), data.GetFilterValueString( "Keywords" ), data.PageNumber, data.PageSize, ref totalResults );
						return ConvertCodeSearchToResults( results, totalResults );
					}
				case "AssessmentSearch":
					{
						var results = AssessmentServices.Search( data.GetFilterValueString( "Keywords" ), data.PageNumber, data.PageSize, ref totalResults ); 
						return ConvertAssessmentProfileToResults( results, totalResults );
					}
				case "OrganizationSearch":
					{
						var results = OrganizationServices.Search( data.GetFilterValueString( "Keywords" ), data.PageNumber, data.PageSize, ref totalResults );
						return ConvertOrganizatonProfileToResults( results, totalResults );
					}
				case "LearningOpportunitySearch":
				case "LearningOpportunityHasPartSearch":
					{
						var results = LearningOpportunityServices.Search( data.GetFilterValueString( "Keywords" ), data.PageNumber, data.PageSize, ref totalResults );
						return ConvertLearningOpportunityProfileToResults( results, totalResults );
					}
				default:
					valid = false;
					status = "Unable to find Search Type";
					return new MicroSearchResults() { TotalResults = 0 };
			}
		}
		//

		//Select a micro search result
		public MicroSearchResult SelectResult( MicroSearchSelection data, ref bool valid, ref string status )
		{
			var result = new MicroSearchResult();
			if ( data.ParentId == 0 || data.ParentType == "" || data.SearchType == "" )
			{
				valid = false;
				status = "Not enough parameters selected (ParentID: " + data.ParentId + "), (ParentType: " + data.ParentType + "), (SearchType: " + data.SearchType + ")";
				return result;
			}

			switch ( data.SearchType )
			{
				case "IndustrySearch":
				case "OccupationSearch":
					{
						//var rawData = new ProfileServices().FrameworkItem_Add( data.ParentId, data.GetValueInt( "CategoryId" ), data.GetValueInt( "CodeId" ), AccountServices.GetUserFromSession(), ref valid, ref status );
						var rawData = new ProfileServices().FrameworkItem_Add( data.ParentId,
							CodesManager.ENTITY_TYPE_CREDENTIAL,
							data.GetValueInt( "CategoryId" ),
							data.GetValueInt( "CodeId" ),
							AccountServices.GetUserFromSession(),
							ref valid,
							ref status );

						var properties = new Dictionary<string, object>()
						{
							{ "Code", rawData.Value },
							{ "RecordId", rawData.Id }
						};
						return ConvertEnumeratedItemToResult( rawData, properties );
					}
				case "CIPSearch":
					{
						int categoryId = data.GetValueInt( "CategoryId" ) > 0 ? data.GetValueInt( "CategoryId" ) : CodesManager.PROPERTY_CATEGORY_CIP;

						var rawData = new ProfileServices().FrameworkItem_Add( data.ParentId, 
							CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, 
							categoryId, 
							data.GetValueInt( "CodeId" ),
							AccountServices.GetUserFromSession(), 
							ref valid, 
							ref status );

						var properties = new Dictionary<string, object>()
						{
							{ "Code", rawData.Value },
							{ "RecordId", rawData.Id }
						};
						return ConvertEnumeratedItemToResult( rawData, properties );
					}
				case "AssessmentSearch":
					{
						var assessmentID = data.GetValueInt( "RecordId" );
						var rawData = new CredentialServices().ConditionProfile_AddAsmt( data.ParentId, assessmentID, AccountServices.GetUserFromSession(), ref valid, ref status );
						if ( rawData == 0 )
						{
							valid = false;
							return null;
						}
						else
						{
							var assessment = AssessmentServices.Get( assessmentID );
							return ConvertAssessmentProfileToResult( assessment );
						}
					}
				case "LearningOpportunitySearch":
					{
						var recordID = data.GetValueInt( "RecordId" );
						var rawData = new CredentialServices().ConditionProfile_AddLearningOpportunity( data.ParentId, recordID, AccountServices.GetUserFromSession(), ref valid, ref status );
						if ( rawData == 0 )
						{
							valid = false;
							return null;
						}
						else
						{
							var results = LearningOpportunityServices.Get( recordID );
							return ConvertLearningOpportunityProfileToResults( results );
						}
							
						
					}
				case "LearningOpportunityHasPartSearch":
					{
						var recordID = data.GetValueInt( "RecordId" );
						var rawData = new LearningOpportunityServices().AddLearningOpportunity_AsPart( data.ParentId, recordID, AccountServices.GetUserFromSession(), ref valid, ref status );
						if ( rawData == 0 )
						{
							valid = false;
							return null;
						}
						else
						{
							var results = LearningOpportunityServices.Get( recordID );
							return ConvertLearningOpportunityProfileToResults( results );
						}


					}
				default:
					valid = false;
					status = "Unable to find Search Type";
					return result;
			}
		}
		//

		//Delete a micro search result
		public void DeleteResult( MicroSearchSelection data, ref bool valid, ref string status )
		{
			if ( data.ParentId == 0 || data.ParentType == "" || data.SearchType == "" )
			{
				valid = false;
				status = "No parameters selected";
				return;
			}

			switch ( data.SearchType )
			{
				case "IndustrySearch":
				case "OccupationSearch": 
					{

						valid = new ProfileServices().FrameworkItem_Delete( data.ParentId, 
							CodesManager.ENTITY_TYPE_CREDENTIAL,
							data.GetValueInt( "RecordId" ), 
							AccountServices.GetUserFromSession(), 
							ref status );
						
						return;
					}
				case "CIPSearch":
					{
						valid = new ProfileServices().FrameworkItem_Delete( data.ParentId,
							CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE,
							data.GetValueInt( "RecordId" ),
							AccountServices.GetUserFromSession(),
							ref status );

						return;
					}
				case "AssessmentSearch":
					{
						switch ( data.ParentType )
						{
							case "ConditionProfile":
							{
								valid = new CredentialServices().ConditionProfile_DeleteAsmt( data.ParentId, data.GetValueInt( "RecordId" ), AccountServices.GetUserFromSession(), ref status );
								return;
							}
							default:
								valid = false;
								status = "Unable to find Parent Type";
								return;
						}
					}
				case "LearningOpportunitySearch":
					{
						switch ( data.ParentType )
						{
							case "ConditionProfile":
								{
									valid = new CredentialServices().ConditionProfile_DeleteLearningOpportunity( data.ParentId, data.GetValueInt( "RecordId" ), AccountServices.GetUserFromSession(), ref status );
									return;
								}
							default:
								valid = false;
								status = "Unable to find Parent Type";
								return;
						}
					}
				default:
					valid = false;
					status = "Unable to find Search Type";
					return;
			}
		}
		//

		#region Helper Methods

		//Convert code item to result with arbitrary properties
		public MicroSearchResult ConvertCodeItemToResult( CodeItem input, Dictionary<string, object> properties )
		{
			return new MicroSearchResult()
				{
					Title = input.Title,
					Description = input.Description,
					CodeId = input.Id,
					Properties = properties
				};
			}
		//

		//Convert enumerated item to result with arbitrary properties
		public MicroSearchResult ConvertEnumeratedItemToResult( EnumeratedItem input, Dictionary<string, object> properties )
		{
			return new MicroSearchResult()
			{
				Title = input.Name,
				Description = input.Description,
				CodeId = input.CodeId,
				Properties = properties
			};
		}
		//

		//Used for code (NAICS, SOC, MOC, etc) searches
		public MicroSearchResults ConvertCodeSearchToResults( List<CodeItem> input, int totalResults )
		{
			var output = new MicroSearchResults() { TotalResults = totalResults };
			foreach ( var item in input )
			{
				var properties = new Dictionary<string, object>()
				{ 
					{ "Code", item.SchemaName }
				};
				output.Results.Add( ConvertCodeItemToResult( item, properties ) );
			}

			return output;
		}
		//

		//Used for GeoNames API searches
		public MicroSearchResults ConvertGeoNamesSearchToResults( List<GeoCoordinates> input, int totalResults )
		{
			var output = new MicroSearchResults() { TotalResults = totalResults };
            if (input == null)
                return output;

			foreach ( var item in input )
			{
				var result = new MicroSearchResult() {
					Title = item.TitleFormatted,
					Description = item.LocationFormatted,
					Properties = new Dictionary<string, object>()
					{
						{ "GeoNamesId", item.GeoNamesId },
						{ "Name", item.Name },
						{ "ToponymName", item.ToponymName },
						{ "Region", item.Region },
						{ "Country", item.Country },
						{ "Latitude", item.Latitude },
						{ "Longitude", item.Longitude },
						{ "Url", item.Url },
						{ "TitleFormatted", item.TitleFormatted },
						{ "LocationFormatted", item.LocationFormatted }
					}
				};
				output.Results.Add( result );
			}

			return output;
		}
		//

		//Used for Assessment searches
		public MicroSearchResult ConvertAssessmentProfileToResult( AssessmentProfile input )
		{
			return new MicroSearchResult()
			{
				Title = input.Name,
				Description = input.Description,
				Properties = new Dictionary<string, object>()
				{
					{ "RecordId", input.Id }
				}
			};
		}
		//
		public MicroSearchResults ConvertAssessmentProfileToResults( List<AssessmentProfile> input, int totalResults )
		{
			var output = new MicroSearchResults() { TotalResults = totalResults };
			foreach ( var item in input )
			{
				output.Results.Add( ConvertAssessmentProfileToResult( item ) );
			}

			return output;
		}
		//
		public MicroSearchResults ConvertOrganizatonProfileToResults( List<Organization> input, int totalResults )
		{
			var output = new MicroSearchResults() { TotalResults = totalResults };
			foreach ( var item in input )
			{
				output.Results.Add( ConvertOrganizatonProfileToResult( item ) );
			}

			return output;
		}
		//Used for Organization searches
		public MicroSearchResult ConvertOrganizatonProfileToResult( Organization input )
		{
			return new MicroSearchResult()
			{
				Title = input.Name,
				Description = input.Description,
				Properties = new Dictionary<string, object>()
				{
					{ "RecordId", input.Id }
				}
			};
		}
		//Used for LearningOpportunityProfile searches
		public MicroSearchResult ConvertLearningOpportunityProfileToResults( LearningOpportunityProfile input )
		{
			return new MicroSearchResult()
			{
				Title = input.Name,
				Description = input.Description,
				Properties = new Dictionary<string, object>()
				{
					{ "RecordId", input.Id }
				}
			};
		}
		//
		public MicroSearchResults ConvertLearningOpportunityProfileToResults( List<LearningOpportunityProfile> input, int totalResults )
		{
			var output = new MicroSearchResults() { TotalResults = totalResults };
			foreach ( var item in input )
			{
				output.Results.Add( ConvertLearningOpportunityProfileToResults( item ) );
			}

			return output;
		}
		#endregion
	}
}
