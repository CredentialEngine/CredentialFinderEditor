using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using CTIServices;
using CF = Factories;
using Models;
using Models.Search;
using Models.Common;
using Models.ProfileModels;
using Utilities;

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
			var validSortOrders = new List<string>() { "newest", "relevance", "alpha", "cost_lowest", "cost_highest", "duration_shortest", "duration_longest" };
			if ( !validSortOrders.Contains( data.SortOrder ) )
			{
				data.SortOrder = validSortOrders.First();
			}

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
								return CredentialServices.Autocomplete( text, 15 );
							//case "competencies":
							//	return CredentialServices.AutocompleteCompetencies( text, 10 );
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
								return AssessmentServices.Autocomplete( text, 15 );
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
								return LearningOpportunityServices.Autocomplete( text, 15 );
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
		public static List<CredentialAlignmentObjectItemProfile> EntityCompetenciesList( string searchType, int entityId, int maxRecords = 10 )
		{
			var results = new List<CredentialAlignmentObjectItemProfile>();
			string filter = "";
			int pTotalRows = 0;
			switch ( searchType.ToLower() )
			{
				case "credential":
					{
						//not sure if will be necessary to include alignment type (ie teaches, and assesses, but not required)
						filter = string.Format("(CredentialId = {0})",entityId);
						return CF.Entity_CompetencyFrameworkManager.Search( filter, "", 1, maxRecords, ref pTotalRows );
					}
				case "assessment":
					{
						filter = string.Format( "(SourceEntityTypeId = 3 AND [SourceId] = {0})", entityId );
						return CF.Entity_CompetencyFrameworkManager.Search( filter, "", 1, maxRecords, ref pTotalRows );
					}
				case "learningopportunity":
					{
						filter = string.Format( "(SourceEntityTypeId = 7 AND [SourceId] = {0})", entityId );
						return CF.Entity_CompetencyFrameworkManager.Search( filter, "", 1, maxRecords, ref pTotalRows );
					}
				default:
					break;
			}

			return results;
		}

		public static List<CostProfileItem> EntityCostsList( string searchType, int entityId, int maxRecords = 10 )
		{
			var results = new List<CostProfileItem>();
			string filter = "";
			int pTotalRows = 0;

			switch ( searchType.ToLower() )
			{
				case "credential":
					{
						filter = "";
						return CF.CostProfileItemManager.Search( 1, entityId, filter, "", 1, maxRecords, ref pTotalRows );
					}
				case "assessment":
					{
						filter = "";
						return CF.CostProfileItemManager.Search( 3, entityId, filter, "", 1, maxRecords, ref pTotalRows );
					}
				case "learningopportunity":
					{
						filter = "";
						return CF.CostProfileItemManager.Search( 7, entityId, filter, "", 1, maxRecords, ref pTotalRows );
					}
				default:
					break;
			}

			return results;
		}
		public static List<CredentialAlignmentObjectItemProfile> EntityQARolesList( string searchType, int entityId, int maxRecords = 10 )
		{
			var results = new List<CredentialAlignmentObjectItemProfile>();
			string filter = "";
			int pTotalRows = 0;
			switch ( searchType.ToLower() )
			{
				case "credential":
					{
						//not sure if will be necessary to include alignment type (ie teaches, and assesses, but not required)
						filter = string.Format( "(CredentialId = {0})", entityId );
						return CF.Entity_CompetencyFrameworkManager.Search( filter, "", 1, maxRecords, ref pTotalRows );
					}
				case "assessment":
					{
						filter = string.Format( "(SourceEntityTypeId = 3 AND [SourceId] = {0})", entityId );
						return CF.Entity_CompetencyFrameworkManager.Search( filter, "", 1, maxRecords, ref pTotalRows );
					}
				case "learningopportunity":
					{
						filter = string.Format( "(SourceEntityTypeId = 7 AND [SourceId] = {0})", entityId );
						return CF.Entity_CompetencyFrameworkManager.Search( filter, "", 1, maxRecords, ref pTotalRows );
					}
				default:
					break;
			}

			return results;
		}
		//Convenience method to handle location data
		//For convenience, check boundaries.IsDefined to see if a boundary is defined
		public static BoundingBox GetBoundaries( MainSearchInput data, string name )
		{
			var boundaries = new BoundingBox();
			try
			{
				//boundaries = data.Filters.FirstOrDefault( m => m.Name == name ).Boundaries;
				boundaries = data.FiltersV2.FirstOrDefault( m => m.Name == name ).AsBoundaries();
				//boundaries = ( BoundingBox ) item;
			}
			catch { }

			return boundaries;
		}
		//

		public enum TagTypes { CONNECTIONS, QUALITY, LEVEL, OCCUPATION, INDUSTRY, SUBJECTS, COMPETENCIES, TIME, COST, ORGANIZATIONTYPE, ORGANIZATIONSECTORTYPE, OWNED_BY, OFFERED_BY, ASMTS_OWNED_BY, LOPPS_OWNED_BY }

		public MainSearchResults ConvertCredentialResults( List<CredentialSummary> results, int totalResults, string searchType )
		{
			var output = new MainSearchResults() { TotalResults = totalResults, SearchType = searchType };
			foreach ( var item in results )
			{
				var mergedCosts = item.NumberOfCostProfileItems; // CostProfileMerged.FlattenCosts( item.EstimatedCost );
				var subjects = Deduplicate( item.Subjects );
				output.Results.Add( Result( item.Name, item.FriendlyName, item.Description, item.Id,
					new Dictionary<string, object>()
					{
						{ "Name", item.Name },
						{ "Description", item.Description },
						{ "Type", item.CredentialType },
						{ "Owner", item.OwnerOrganizationName },
						{ "OwnerId", item.OwnerOrganizationId },
						{ "CanEditRecord", item.CanEditRecord },
						{ "TypeSchema", item.CredentialTypeSchema.ToLower()},
						{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( item.Addresses ) } },
						{ "Locations", ConvertAddresses( item.Addresses ) },
						{ "SearchType", searchType },
						{ "RecordId", item.Id },
						{ "UrlTitle", item.FriendlyName },
						{ "HasBadge", item.HasVerificationType_Badge } //Indicate existence of badge here

						//{ "Creator", item.OwnerOrganizationName },
						//{ "CreatorId", item.OwnerOrganizationId },
						//{ "Industry", new { Type = "tags", Title = "Industr" + (item.NaicsResults.Results.Count() == 1 ? "y" : "ies"), Data = ConvertCodeItemsToDictionary( item.NaicsResults.Results ) } },
						//{ "Occupation", new { Type = "tags", Title = "Occupation" + (item.OccupationResults.Results.Count() == 1 ? "" : "s"), Data = ConvertCodeItemsToDictionary( item.OccupationResults.Results ) } },
						//{ "Level", new { Type = "tags", Title = "Level" + (item.LevelsResults.Results.Count() == 1 ? "" : "s"), Data = ConvertCodeItemsToDictionary( item.LevelsResults.Results ) } },
						//{ "Cost", "Estimated Cost Placeholder" },
						//{ "Time", "Estimated Time to Complete" },
						//{ "IsQA", item.IsAQACredential ? "true" : "false" },
						//{ "HasQA", item.HasQualityAssurance ? "true" : "false" }
					},
					new List<TagSet>(),
					new List<Models.Helpers.SearchTag>()
					{
						//Connections
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Connections",
							DisplayTemplate = "{#} Connection{s}",
							Name = TagTypes.CONNECTIONS.ToString().ToLower(),
							TotalItems = item.CredentialsList.Results.Count(),
							SearchQueryType = "link",
							//Items = GetSearchTagItems_Filter( item.ConnectionsList.Results, "{Name} Credential(s)", item.ConnectionsList.CategoryId )
							//Something like this...
							/*	*/
							Items = item.CredentialsList.Results.ConvertAll(m => new Models.Helpers.SearchTagItem()
							{
								Display = "<b>" + m.Connection + "</b>" + " " + m.Credential, //[Is Preparation For] [Some Credential Name] 
								QueryValues = new Dictionary<string, object>()
								{
									{ "TargetId", m.CredentialId }, //AgentId?
									{ "TargetType", "credential" }, //Probably okay to hard code this for now
									{ "ConnectionTypeId", m.ConnectionId }, //Connection type
								}
							} ).ToList()
						
						},
						//Quality Assurance
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Quality Assurance",
							DisplayTemplate = "{#} Quality Assurance",
							Name = "qualityAssuranceBy", //Using the "quality" enum breaks this filter since it tries to find the matching item in the checkbox list and it doesn't exist
							TotalItems = item.AgentAndRoles.Results.Count(),
							SearchQueryType = "custom", //Change this to "custom", or back to detail
							//Something like this...
							//QAOrgRolesResults is a list of 1 role and 1 org (org repeating for each relevant role)
							//e.g. [Accredited By] [Organization 1], [Approved By] [Organization 1], [Accredited By] [Organization 2], etc.
							Items = item.AgentAndRoles.Results.ConvertAll( m => new Models.Helpers.SearchTagItem() {
								Display = "<b>" + m.Relationship + "</b>" + " by " + m.Agent, //[Accredited By] [Organization 1]
								QueryValues = new Dictionary<string, object>() {
									{ "RoleId", m.RelationshipId },
									{ "AgentId", m.AgentId }
								}
							} ).ToList()
							
							//Items = GetSearchTagItems_Filter( item.QARolesResults.Results, "{Name} by Quality Assurance Organization(s)", item.QARolesResults.CategoryId )
						},
						//Audience Level Type
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Levels",
							DisplayTemplate = "{#} Level{s}",
							Name = TagTypes.LEVEL.ToString().ToLower(),
							TotalItems = item.LevelsResults.Results.Count(),
							SearchQueryType = "code",
							Items = GetSearchTagItems_Filter( item.LevelsResults.Results, "{Name}", item.LevelsResults.CategoryId )
						},
						//Occupations
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Occupations",
							DisplayTemplate = "{#} Occupation{s}",
							Name = TagTypes.OCCUPATION.ToString().ToLower(),
							TotalItems = item.OccupationResults.Results.Count(),
							SearchQueryType = "framework",
							Items = GetSearchTagItems_Filter( item.OccupationResults.Results, "{Name}", item.OccupationResults.CategoryId )
						},
						//Industries
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Industries",
							DisplayTemplate = "{#} Industr{ies}",
							Name = TagTypes.INDUSTRY.ToString().ToLower(),
							TotalItems = item.NaicsResults.Results.Count(),
							SearchQueryType = "framework",
							Items = GetSearchTagItems_Filter( item.NaicsResults.Results, "{Name}", item.NaicsResults.CategoryId )
						},
						//Subjects
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Subjects",
							DisplayTemplate = "{#} Subject{s}",
							Name = TagTypes.SUBJECTS.ToString().ToLower(),
							TotalItems = subjects.Count(), //Returns a count of the de-duplicated items
							SearchQueryType = "text",
							Items = subjects.ConvertAll( m => new Models.Helpers.SearchTagItem() { Display = m, QueryValues = new Dictionary<string, object>() { { "TextValue", m } } } )
						},
						//Competencies
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Competencies",
							DisplayTemplate = "{#} Competenc{ies}",
							Name = TagTypes.COMPETENCIES.ToString().ToLower(),
							TotalItems = item.AssessmentsCompetenciesCount + item.LearningOppsCompetenciesCount,
							SearchQueryType = "text",
							IsAjaxQuery = true,
							AjaxQueryName = "GetSearchResultCompetencies",
							AjaxQueryValues = new Dictionary<string, object>()
							{
								{ "SearchType", "credential" },
								{ "RecordId", item.Id },
								{ "TargetEntityType", "competencies" }
							}
						},
						//Durations
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Time Estimates",
							DisplayTemplate = "{#} Time Estimate{s}",
							Name = TagTypes.TIME.ToString().ToLower(),
							TotalItems = item.EstimatedTimeToEarn.Count(), //# of duration profiles
							SearchQueryType = "detail", //Not sure how this could be any kind of search query
							Items = item.EstimatedTimeToEarn.ConvertAll(m => new Models.Helpers.SearchTagItem()
							{
								Display = m.IsRange ? m.MinimumDuration.Print() + " - " + m.MaximumDuration.Print() : m.ExactDuration.Print(),
								QueryValues = new Dictionary<string, object>()
								{
									{ "ExactDuration", m.ExactDuration },
									{ "MinimumDuration", m.MinimumDuration },
									{ "MaximumDuration", m.MaximumDuration }
								}
							} ).ToList()
						},
						//Costs
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Costs",
							DisplayTemplate = "{#} Cost{s}",
							Name = TagTypes.COST.ToString().ToLower(),
							TotalItems = item.NumberOfCostProfileItems, //# of cost profiles items
							SearchQueryType = "text",
							IsAjaxQuery = true,
							AjaxQueryName = "GetSearchResultCosts",
							AjaxQueryValues = new Dictionary<string, object>()
							{
								{ "SearchType", "credential" },
								{ "RecordId", item.Id },
								{ "TargetEntityType", "cost" }
							}
						},
						//CostsOLD
						//new Models.Helpers.SearchTag()
						//{
						//	CategoryName = "Costs",
						//	DisplayTemplate = "{#} Cost{s}",
						//	Name = TagTypes.COST.ToString().ToLower(),
						//	TotalItems = mergedCosts.Count(), //# of cost profiles
						//	SearchQueryType = "detail", //Not sure how this could be any kind of search query
						//	Items = mergedCosts.ConvertAll(m => new Models.Helpers.SearchTagItem()
						//	{
						//		Display = m.CostType.Items.FirstOrDefault().Name + ": " + m.CurrencySymbol + m.Price,
						//		QueryValues = new Dictionary<string, object>()
						//		{
						//			{ "CurrencySymbol", m.CurrencySymbol },
						//			{ "Price", m.Price },
						//			{ "CostType", m.CostType.Items.FirstOrDefault().SchemaName }
						//		}
						//	} ).ToList()
						//},
					}
				) );
			}
			return output;
		}
		//

		public List<string> Deduplicate( List<string> items )
		{
			var added = new List<string>();
			var result = new List<string>();
			foreach(var item in items )
			{
				var text = item.ToLower().Trim();
				if ( !added.Contains( text ) )
				{
					added.Add( text );
					result.Add( item.Trim() );
				}
			}
			return result;
		}
		//

		public List<Models.Helpers.SearchTagItem> GetSearchTagItems_Filter( List<CodeItem> items, string displayTemplate, int categoryID )
		{
			return items.ConvertAll( m => new Models.Helpers.SearchTagItem()
			{
				Display = Models.Helpers.SearchTagHelper.Count( displayTemplate.Replace( "{Name}", m.Name ), 1 ),
				QueryValues = new Dictionary<string, object>()
				{
					{ "CategoryId", categoryID },
					{ "CodeId", m.Id },
					{ "SchemaName", m.SchemaName }
				}
			} );
		}
		//

		public List<Models.Helpers.SearchTagItem> GetSearchTagItems_Filter( List<EnumeratedItem> items, string displayTemplate, int categoryID )
		{
			return items.ConvertAll( m => new Models.Helpers.SearchTagItem()
			{
				Display = Models.Helpers.SearchTagHelper.Count( displayTemplate.Replace( "{Name}", m.Name ), 1 ),
				QueryValues = new Dictionary<string, object>()
				{
					{ "CategoryId", categoryID },
					{ "CodeId", m.Id },
					{ "SchemaName", m.SchemaName }
				}
			} );
		}
		//

		public static TagSet GetTagSet( string searchType, TagTypes entityType, int recordID, int maxRecords = 10 )
		{
			var result = new TagSet();
			switch ( entityType ) //Match "Schema" in ConvertCredentialResults() method above
			{
				case TagTypes.COMPETENCIES:
					{
						var data = SearchServices.EntityCompetenciesList( searchType, recordID, maxRecords );
						result = new TagSet()
						{
							Schema = TagTypes.COMPETENCIES.ToString().ToLower(),
							Label = "Competencies",
							Method = "direct",
							Items = data.ConvertAll( m => new TagItem() { CodeId = m.Id, Label = m.Name, Description = m.Description } )
						};
						break;
					}

				case TagTypes.COST:
					{
						//future
						var data = SearchServices.EntityCostsList( searchType, recordID, maxRecords );
						result = new TagSet()
						{
							Schema = TagTypes.COST.ToString().ToLower(),
							Label = "Costs",
							Method = "direct",
							CostItems = data.ConvertAll( c => new CostTagItem()
							{
								CodeId = c.CostProfileId, Price = c.Price, CostType = c.CostTypeName, CurrencySymbol = c.CurrencySymbol, SourceEntity = c.ParentEntityType
							} ),
							Items = data.ConvertAll( m => new TagItem() { CodeId = m.Id, Label = m.CostTypeName, Description = m.Description } )
						};
						break;
					}
				default: break;
			}
			return result;
		}
		//

		/*
		private List<TagItem> ConvertCodeItemsToTagItems( List<CodeItem> input )
		{
			var result = new List<TagItem>();
			foreach ( var item in input )
			{
				if ( result.FirstOrDefault( m => m.CodeId == item.Id ) == null ) //Prevent duplicates
				{
					result.Add( new TagItem() { CodeId = item.Id, Schema = item.SchemaName, Label = item.Name } );
				}
			}
			result = result.OrderBy( m => m.Label ).ToList();
			return result;
		}
		//
		*/
		public MainSearchResults ConvertOrganizationResults( List<OrganizationSummary> results, int totalResults, string searchType )
		{
			var output = new MainSearchResults() { TotalResults = totalResults, SearchType = searchType };
			foreach ( var item in results )
			{
				output.Results.Add( Result( item.Name, item.FriendlyName, item.Description, item.Id,
					new Dictionary<string, object>()
					{
						{ "Name", item.Name },
						{ "Description", item.Description },
						{ "OwnerId", 0 },
						{ "CanEditRecord", item.CanEditRecord },
						{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( new List<Address>() { item.Address } ) } },
						{ "Locations", ConvertAddresses( item.Auto_Address ) },
						{ "SearchType", searchType },
						{ "RecordId", item.Id },
						{ "UrlTitle", item.FriendlyName },
						{ "Logo", item.ImageUrl },
						{ "ResultImageUrl", item.ImageUrl ?? "" },
						{ "Location", item.Address.Country + ( string.IsNullOrWhiteSpace( item.Address.Country ) ? "" : " - " ) + item.Address.City + ( string.IsNullOrWhiteSpace( item.Address.City ) ? "" : ", " ) + item.Address.AddressRegion },

						{ "Coordinates", new { Type = "coordinates", Data = new { Latitude = item.Address.Latitude, Longitude = item.Address.Longitude } } },
						{ "IsQA", item.IsAQAOrg ? "true" : "false" },
						//{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( item.Addresses ) } },

					},
					null,
					new List<Models.Helpers.SearchTag>()
					{
						//Organization Type
						new Models.Helpers.SearchTag()
						{
							CategoryName = "OrganizationType",
							DisplayTemplate = "{#} Organization Type{s}",
							Name = TagTypes.ORGANIZATIONTYPE.ToString().ToLower(),
							TotalItems = item.OrganizationType.Items.Count(),
							SearchQueryType = "code",
							Items = GetSearchTagItems_Filter( item.OrganizationType.Items, "{Name}", item.OrganizationType.Id )
						},
						//Organization Sector Type
						new Models.Helpers.SearchTag()
						{
							CategoryName = "OrganizationSector",
							DisplayTemplate = "{#} Economic Sector{s}",
							Name = TagTypes.ORGANIZATIONSECTORTYPE.ToString().ToLower(),
							TotalItems = item.OrganizationSectorType.Items.Count(),
							SearchQueryType = "code",
							Items = GetSearchTagItems_Filter( item.OrganizationSectorType.Items, "{Name}", item.OrganizationSectorType.Id )
						},
						//owns
						new Models.Helpers.SearchTag()
						{
							CategoryName = "OwnsCredentials",
							DisplayTemplate = "Owns {#} Credential{s}",
							Name = TagTypes.OWNED_BY.ToString().ToLower(),
							TotalItems = item.OwnedByResults.Results.Count(),
							SearchQueryType = "link",
							Items = item.OwnedByResults.Results.ConvertAll(m => new Models.Helpers.SearchTagItem()
							{
								Display = m.Title,
								QueryValues = new Dictionary<string, object>()
								{
									{ "TargetId", m.Id }, //Credential ID
									{ "TargetType", "credential" },
								}
							} ).ToList()
						},
						//offers
						new Models.Helpers.SearchTag()
						{
							CategoryName = "OffersCredentials",
							DisplayTemplate = "Offers {#} Credential{s}",
							Name = TagTypes.OFFERED_BY.ToString().ToLower(),
							TotalItems = item.OfferedByResults.Results.Count(),
							SearchQueryType = "link",
							Items = item.OfferedByResults.Results.ConvertAll(m => new Models.Helpers.SearchTagItem()
							{
								Display = m.Title, 
								QueryValues = new Dictionary<string, object>()
								{
									{ "TargetId", m.Id }, //Credential ID
									{ "TargetType", "credential" }, 
								}
							} ).ToList()
						},
						//asmts owned by
						new Models.Helpers.SearchTag()
						{
							CategoryName = "OwnsAssessments",
							DisplayTemplate = "Owns {#} Assessment{s}",
							Name = TagTypes.ASMTS_OWNED_BY.ToString().ToLower(),
							TotalItems = item.AsmtsOwnedByResults.Results.Count(),
							SearchQueryType = "link",
							Items = item.AsmtsOwnedByResults.Results.ConvertAll(m => new Models.Helpers.SearchTagItem()
							{
								Display = m.Title,
								QueryValues = new Dictionary<string, object>()
								{
									{ "TargetId", m.Id }, //asmt ID
									{ "TargetType", "assessment" },
								}
							} ).ToList()
						},
						//lopps owned by
						new Models.Helpers.SearchTag()
						{
							CategoryName = "OwnsLearningOpportunity",
							DisplayTemplate = "Owns {#} Learning Opportunit{ies}",
							Name = TagTypes.LOPPS_OWNED_BY.ToString().ToLower(),
							TotalItems = item.LoppsOwnedByResults.Results.Count(),
							SearchQueryType = "link",
							Items = item.LoppsOwnedByResults.Results.ConvertAll(m => new Models.Helpers.SearchTagItem()
							{
								Display = m.Title,
								QueryValues = new Dictionary<string, object>()
								{
									{ "TargetId", m.Id }, //lopp ID
									{ "TargetType", "learningopportunity" }, //??
								}
							} ).ToList()
						},
						//accredited by orgs
						new Models.Helpers.SearchTag()
						{
							CategoryName = "AccreditedByOrgs",
							DisplayTemplate = "Accredited by {#} Organization{s}",
							Name = TagTypes.CONNECTIONS.ToString().ToLower(),
							TotalItems = item.AccreditedByResults.Results.Count(),
							SearchQueryType = "link",
							Items = item.AccreditedByResults.Results.ConvertAll(m => new Models.Helpers.SearchTagItem()
							{
								Display = m.Title,
								QueryValues = new Dictionary<string, object>()
								{
									{ "TargetId", m.Id }, //org ID
									{ "TargetType", "organization" }, //??
								}
							} ).ToList()
						},
						//approved by orgs
						new Models.Helpers.SearchTag()
						{
							CategoryName = "ApprovedByOrgs",
							DisplayTemplate = "Approved by {#} Organization{s}",
							Name = TagTypes.CONNECTIONS.ToString().ToLower(),
							TotalItems = item.ApprovedByResults.Results.Count(),
							SearchQueryType = "link",
							Items = item.ApprovedByResults.Results.ConvertAll(m => new Models.Helpers.SearchTagItem()
							{
								Display = m.Title,
								QueryValues = new Dictionary<string, object>()
								{
									{ "TargetId", m.Id },
									{ "TargetType", "organization" }, //??
								}
							} ).ToList()
						},
						//Industries
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Industries",
							DisplayTemplate = "{#} Industr{ies}",
							Name = TagTypes.INDUSTRY.ToString().ToLower(),
							TotalItems = item.NaicsResults.Results.Count(),
							SearchQueryType = "framework",
							Items = GetSearchTagItems_Filter( item.NaicsResults.Results, "{Name}", item.NaicsResults.CategoryId )
						},
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
				output.Results.Add( Result( item.Name, item.FriendlyName, item.Description, item.Id,
					new Dictionary<string, object>()
					{
						{ "Name", item.Name },
						{ "Description", item.Description },
						{ "OwnerId", 0 },
						{ "CanEditRecord", item.CanEditRecord },
						{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( new List<Address>() { item.Address } ) } },
						{ "Locations", ConvertAddresses( new List<Address>() { item.Address } ) },
						{ "SearchType", searchType },
						{ "RecordId", item.Id },
						{ "UrlTitle", item.FriendlyName },
						{ "Logo", item.ImageUrl },
						{ "ResultImageUrl", item.ImageUrl ?? "" },
						{ "Location", item.Address.Country + ( string.IsNullOrWhiteSpace( item.Address.Country ) ? "" : " - " ) + item.Address.City + ( string.IsNullOrWhiteSpace( item.Address.City ) ? "" : ", " ) + item.Address.AddressRegion },

						{ "Coordinates", new { Type = "coordinates", Data = new { Latitude = item.Address.Latitude, Longitude = item.Address.Longitude } } },
						{ "IsQA", item.IsAQAOrg ? "true" : "false" },
						//{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( item.Addresses ) } },

					},
					null,
					new List<Models.Helpers.SearchTag>()
					{
						//Organization Type
						new Models.Helpers.SearchTag()
						{
							CategoryName = "OrganizationType",
							DisplayTemplate = "{#} Organization Type{s}",
							Name = TagTypes.ORGANIZATIONTYPE.ToString().ToLower(),
							TotalItems = item.OrganizationType.Items.Count(),
							SearchQueryType = "code",
							Items = GetSearchTagItems_Filter( item.OrganizationType.Items, "{Name}", item.OrganizationType.Id )
						},
						//Organization Sector Type
						new Models.Helpers.SearchTag()
						{
							CategoryName = "OrganizationSector",
							DisplayTemplate = "{#} Economic Sector{s}",
							Name = TagTypes.ORGANIZATIONSECTORTYPE.ToString().ToLower(),
							TotalItems = item.OrganizationSectorType.Items.Count(),
							SearchQueryType = "code",
							Items = GetSearchTagItems_Filter( item.OrganizationSectorType.Items, "{Name}", item.OrganizationSectorType.Id )
						},
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
						{ "Name", item.Name },
						{ "Description", item.Description },
						{ "Owner", string.IsNullOrWhiteSpace( item.OrganizationName ) ? "" : item.OrganizationName },
						{ "OwnerId", item.OwningOrganizationId },
						{ "CanEditRecord", item.CanEditRecord },
						{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( item.Addresses ) } },
						{ "Locations", ConvertAddresses( item.Addresses ) },
						{ "SearchType", searchType },
						{ "RecordId", item.Id },
						{ "UrlTitle", item.FriendlyName },
					},
					null,
					new List<Models.Helpers.SearchTag>()
					{
						//Competencies
						new Models.Helpers.SearchTag()
						{
							CategoryName = "AssessesCompetencies",
							DisplayTemplate = "Assesses {#} Competenc{ies}",
							Name = TagTypes.COMPETENCIES.ToString().ToLower(),
							TotalItems = item.AssessesCompetencies.Count(),
							SearchQueryType = "text",
							IsAjaxQuery = true,
							AjaxQueryName = "GetSearchResultCompetencies",
							AjaxQueryValues = new Dictionary<string, object>()
							{
								{ "SearchType", "assessment" },
								{ "RecordId", item.Id },
								{ "TargetEntityType", "competencies" }
							}
						},
						//Competencies direct
						new Models.Helpers.SearchTag()
						{
							CategoryName = "AssessesCompetenciesDirect",
							DisplayTemplate = "Assesses {#} Competenc{ies} Direct",
							Name = TagTypes.COMPETENCIES.ToString().ToLower(),
							TotalItems = item.AssessesCompetencies.Count(),
							SearchQueryType = "detail",
							Items = item.AssessesCompetencies.ConvertAll( m => new Models.Helpers.SearchTagItem()
							{
								Display = string.IsNullOrWhiteSpace(m.TargetDescription) ?
								m.TargetName :
								"<b>" + m.TargetName + "</b>" + System.Environment.NewLine + m.TargetDescription,
								QueryValues = new Dictionary<string, object>()
								{
									{ "SchemaName", null },
									{ "CodeId", m.Id },
									{ "TextValue", m.TargetName },
									{ "TextDescription", m.TargetDescription }
								}
							} )
						},
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
						{ "Name", item.Name },
						{ "Description", item.Description },
						{ "Owner", string.IsNullOrWhiteSpace( item.OrganizationName ) ? "" : item.OrganizationName },
						{ "OwnerId", item.OwningOrganizationId },
						{ "CanEditRecord", item.CanEditRecord },
						{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( item.Addresses ) } },
						{ "Locations", ConvertAddresses( item.Addresses ) },
						{ "SearchType", searchType },
						{ "RecordId", item.Id },
						{ "UrlTitle", item.FriendlyName },

						{ "Competencies", new { Type = "tags", Data = ConvertCompetenciesToDictionary( item.TeachesCompetencies ) } },
					},
					null,
					new List<Models.Helpers.SearchTag>()
					{
						//Competencies
						new Models.Helpers.SearchTag()
						{
							CategoryName = "TeachesCompetencies",
							DisplayTemplate = "Teaches {#} Competenc{ies}",
							Name = TagTypes.COMPETENCIES.ToString().ToLower(),
							TotalItems = item.TeachesCompetencies.Count(),
							SearchQueryType = "text",
							IsAjaxQuery = true,
							AjaxQueryName = "GetSearchResultCompetencies",
							AjaxQueryValues = new Dictionary<string, object>()
							{
								{ "SearchType", "learningopportunity" },
								{ "RecordId", item.Id },
								{ "TargetEntityType", "competencies" }
							}
						},
						//Competencies direct
						new Models.Helpers.SearchTag()
						{
							CategoryName = "TeachesCompetenciesDirect",
							DisplayTemplate = "Teaches {#} Competenc{ies} Direct",
							Name = TagTypes.COMPETENCIES.ToString().ToLower(),
							TotalItems = item.TeachesCompetencies.Count(),
							SearchQueryType = "detail",
							Items = item.TeachesCompetencies.ConvertAll( m => new Models.Helpers.SearchTagItem()
							{
								Display = string.IsNullOrWhiteSpace(m.TargetDescription) ?
								m.TargetName :
								"<b>" + m.TargetName + "</b>" + System.Environment.NewLine + m.TargetDescription,
								QueryValues = new Dictionary<string, object>()
								{
									{ "SchemaName", null },
									{ "CodeId", m.Id },
									{ "TextValue", m.TargetName },
									{ "TextDescription", m.TargetDescription }
								}
							} )
						},

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
		public MainSearchResult Result( string name, string description, int recordID, Dictionary<string, object> properties, List<TagSet> tags, List<Models.Helpers.SearchTag> tagsV2 = null )
		{
			return new MainSearchResult()
			{
				Name = string.IsNullOrWhiteSpace( name ) ? "No name" : name,
				Description = string.IsNullOrWhiteSpace( description ) ? "No description" : description,
				RecordId = recordID,
				Properties = properties == null ? new Dictionary<string, object>() : properties,
				Tags = tags == null ? new List<TagSet>() : tags,
				TagsV2 = tagsV2 ?? new List<Models.Helpers.SearchTag>()
			};
		}
		//
		public MainSearchResult Result( string name, string friendlyName, string description, int recordID, Dictionary<string, object> properties, List<TagSet> tags, List<Models.Helpers.SearchTag> tagsV2 = null )
		{
			return new MainSearchResult()
			{
				Name = string.IsNullOrWhiteSpace( name ) ? "No name" : name,
				FriendlyName = string.IsNullOrWhiteSpace( friendlyName ) ? "Record" : friendlyName,
				Description = string.IsNullOrWhiteSpace( description ) ? "No description" : description,
				RecordId = recordID,
				Properties = properties == null ? new Dictionary<string, object>() : properties,
				Tags = tags == null ? new List<TagSet>() : tags,
				TagsV2 = tagsV2 ?? new List<Models.Helpers.SearchTag>()
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

			//List<string> list =
			//CF.Entity_ReferenceManager.QuickSearch_TextValue( entityTypeId, categoryId, keyword, maxTerms );
			List<string> list =
			CF.Entity_ReferenceManager.QuickSearch_Subjects( entityTypeId, keyword, maxTerms );
			return list;
		}

		#region Common filters
		public static void HandleCustomFilters( MainSearchInput data, int searchCategory, ref string where )
		{
			string AND = "";
			//may want custom category for each one, to prevent requests that don't match the current search
	
			string sql = "";

			//Updated to use FilterV2
			if ( where.Length > 0 )
			{
				AND = " AND ";
			}
			foreach ( var filter in data.FiltersV2.Where( m => m.Type == MainSearchFilterV2Types.CODE ).ToList() )
			{
				var item = filter.AsCodeItem();
				if( item.CategoryId != searchCategory )
				{
					continue;
				}

				sql = GetPropertySql( item.Id );
				if ( string.IsNullOrWhiteSpace( sql ) == false )
				{
					where = where + AND + sql;
					AND = " AND ";
				}
			}
			if(sql.Length > 0 )
			{
				LoggingHelper.DoTrace( 6, "SearchServices.HandleCustomFilters. result: \r\n" + where );
			}

			/* //Retained for reference
			foreach ( MainSearchFilter filter in data.Filters.Where( s => s.CategoryId == searchCategory ) )
			{
				//each item will be a custom sql 
				//the propertyId will differ in env, so can't use it for lookup in web.config. Could get from db, and cache
				if ( where.Length > 0 )
					AND = " AND ";
				int id = 0;
				foreach ( string item in filter.Items )
				{
					if (Int32.TryParse(item, out id)) 
					{
						sql = GetPropertySql( id );
						if ( string.IsNullOrWhiteSpace( sql ) == false )
						{
							where = where + AND + sql;
							AND = " AND ";
						}
					}
				}
				
			}
			if ( sql.Length > 0 )
			{
				LoggingHelper.DoTrace( 6, "SearchServices.HandleCustomFilters. result: \r\n" + where );
			}
			*/
		}
		private static string GetPropertySql(int id)
		{
			string sql = "";
			string key = "propertySql_" + id.ToString();
			//check cache for vocabulary
			if ( HttpRuntime.Cache[ key ] != null )
			{
				sql = ( string ) HttpRuntime.Cache[ key ];
				return sql;
			}

			CodeItem item = CF.CodesManager.Codes_PropertyValue_Get( id );
			if ( item != null && ( item.Description ?? "" ).Length > 5 )
			{
				sql = item.Description;
				HttpRuntime.Cache.Insert( key, sql );
			}

			return sql;
		}

		/// <summary>
		/// Generic
		/// </summary>
		/// <param name="data"></param>
		/// <param name="where"></param>
		//public static void SetSubjectsFilter( MainSearchInput data, string entity, ref string where )
		//{
		//	string subjects = " (base.RowId in (SELECT b.EntityUid FROM [dbo].[Entity.Reference] a inner join Entity b on a.EntityId = b.Id inner join {0} c on b.EntityUid = c.RowId where [CategoryId] = 34 and ({1}) )) ";
		//	string phraseTemplate = " (a.TextValue like '%{0}%') ";
		//	//string credSubjects = " (base.RowId in (SELECT CredentialRowId FROM [ConditionProfile_Subjects_Summary] a where ({1}) )) ";
		//	//string phraseTemplate = " (a.TextValue like '%{0}%') ";
		//	string credSubjects = " (base.RowId in (SELECT EntityUid FROM [Entity_Subjects] a where ({1}) )) ";
		//	string credPhraseTemplate = " (a.Subject like '%{0}%') ";

		//	string sfilter = "";
		//	string phrase = "";
		//	if ( entity == "Credential" )
		//	{
		//		//revert for now
		//		sfilter = credSubjects;
		//		phrase = credPhraseTemplate;
		//		//phrase = phraseTemplate;
		//		//sfilter = subjects;
		//	}
		//	else
		//	{
		//		phrase = phraseTemplate;
		//		sfilter = subjects;
		//	}

		//	string AND = "";
		//	string OR = "";
		//	string keyword = "";

		//	foreach ( MainSearchFilter filter in data.Filters.Where( s => s.Name == "subjects" ) )
		//	{
		//		string next = "";
		//		if ( where.Length > 0 )
		//			AND = " AND ";
		//		foreach ( string item in filter.Texts )
		//		{
		//			keyword = ServiceHelper.HandleApostrophes( item );
		//			if ( keyword.IndexOf( ";" ) > -1 )
		//			{
		//				var words = keyword.Split( ';' );
		//				foreach ( string word in words )
		//				{
		//					next += OR + string.Format( phrase, word.Trim() );
		//					OR = " OR ";
		//				}
		//			}
		//			else
		//			{
		//				next = string.Format( phrase, keyword.Trim() );
		//			}
		//			//next += keyword;	//					+",";
		//			//just handle one for now
		//			break;
		//		}
		//		//next = next.Trim( ',' );
		//		if ( !string.IsNullOrWhiteSpace( next ) )
		//			where = where + AND + string.Format( sfilter, entity, next );

		//		break;
		//	}
		//}

		public static void SetSubjectsFilter( MainSearchInput data, int entityTypeId, ref string where )
		{
			string subjects = "  (base.RowId in (SELECT EntityUid FROM [Entity_Subjects] a where EntityTypeId = {0} AND {1} )) ";
			if ( data.SearchType == "credential" )
				subjects = subjects.Replace( "base.RowId", "base.EntityUid" );

			string phraseTemplate = " (a.Subject like '%{0}%') ";

			string AND = "";
			string OR = "";
			string keyword = "";

			//Updated to use FilterV2
			string next = "";
			if ( where.Length > 0 )
			{
				AND = " AND ";
			}
			foreach ( var filter in data.FiltersV2.Where( m => m.Name == "subjects" ) )
			{
				var text = ServiceHelper.HandleApostrophes( filter.AsText() );
				if ( string.IsNullOrWhiteSpace( text ) )
				{
					continue;
				}

				next += OR + string.Format( phraseTemplate, SearchifyWord( text ) );
				OR = " OR ";
			}
			if ( !string.IsNullOrWhiteSpace( next ) )
			{
				where = where + AND + string.Format( subjects, entityTypeId, next );
			}

			/* //Retained for reference
			foreach ( MainSearchFilter filter in data.Filters.Where( s => s.Name == "subjects" ) )
			{
				string next = "";
				if ( where.Length > 0 )
					AND = " AND ";
				foreach ( string item in filter.Texts )
				{
					keyword = ServiceHelper.HandleApostrophes( item );
					if ( keyword.IndexOf( ";" ) > -1 )
					{
						var words = keyword.Split( ';' );
						foreach ( string word in words )
						{
							next += OR + string.Format( phraseTemplate, PrepWord( word) );
							OR = " OR ";
						}
					}
					else
					{
						next = string.Format( phraseTemplate, PrepWord( keyword ) );
					}
					//next += keyword;	//					+",";
					//just handle one for now
					break;
				}
				//next = next.Trim( ',' );
				if ( !string.IsNullOrWhiteSpace( next ) )
					where = where + AND + string.Format( subjects, entityTypeId, next );

				break;
			}
			*/
		} 
		/// <summary>
		/// May want to make configurable, in case don't want to always perform check.
		/// </summary>
		/// <param name="word"></param>
		/// <returns></returns>
		public static string SearchifyWord( string word)
		{
			string keyword = word.Trim() + "^";


			if ( keyword.ToLower().LastIndexOf( "es^" ) > 4 )
			{
				keyword = keyword.Substring( 0, keyword.ToLower().LastIndexOf( "es" ) );
			}
			else if ( keyword.ToLower().LastIndexOf( "s^" ) > 4 )
			{
				keyword = keyword.Substring( 0, keyword.ToLower().LastIndexOf( "s" ) );
			}

			if ( keyword.ToLower().LastIndexOf( "ing^" ) > 3 )
			{
				keyword = keyword.Substring( 0, keyword.ToLower().LastIndexOf( "ing^" ) );
			}
			else if ( keyword.ToLower().LastIndexOf( "ed^" ) > 4 )
			{
				keyword = keyword.Substring( 0, keyword.ToLower().LastIndexOf( "ed^" ) );
			}
			else if ( keyword.ToLower().LastIndexOf( "ion^" ) > 3 )
			{
				keyword = keyword.Substring( 0, keyword.ToLower().LastIndexOf( "ion^" ) );
			}
			else if ( keyword.ToLower().LastIndexOf( "ive^" ) > 3 )
			{
				keyword = keyword.Substring( 0, keyword.ToLower().LastIndexOf( "ive^" ) );
			}

			if ( keyword.IndexOf( "%" ) == -1 )
			{
				keyword = "%" + keyword.Trim() + "%";
				keyword = keyword.Replace( "&", "%" ).Replace( " and ", "%" ).Replace( " in ", "%" ).Replace( " of ", "%" ).Replace( " for ", "%" ).Replace( " with ", "%" );
				keyword = keyword.Replace( " from ", "%" );
				keyword = keyword.Replace( " a ", "%" );
				keyword = keyword.Replace( " - ", "%" );
				keyword = keyword.Replace( " % ", "%" );

				//just replace all spaces with %?
				keyword = keyword.Replace( "  ", "%" );
				keyword = keyword.Replace( " ", "%" );
				keyword = keyword.Replace( "%%", "%" );
			}


			keyword = keyword.Replace( "^", "" );
			return keyword;
		}
			
			//public static void SetSubjectsAutocompleteFilter( string keywords, string entity, ref string where )
		//{
		//	string subjects = " (base.RowId in (SELECT b.EntityUid FROM [dbo].[Entity.Reference] a inner join Entity b on a.EntityId = b.Id inner join {0} c on b.EntityUid = c.RowId where [CategoryId] = 34 and a.TextValue like '{1}' )) ";

		//	string AND = "";
		//	keywords = ServiceHelper.HandleApostrophes( keywords );
		//	if ( keywords.IndexOf( "%" ) == -1 )
		//		keywords = "%" + keywords.Trim() + "%";

		//	where = where + AND + string.Format( " ( " + subjects + " ) ", entity, keywords );
		//}

		//public static void SetCredentialSubjectsFilter( MainSearchInput data, string entity, ref string where )
		//{
		//	string subjects = " (base.RowId in (SELECT b.EntityUid FROM [dbo].[Entity.Reference] a inner join Entity b on a.EntityId = b.Id inner join {0} c on b.EntityUid = c.RowId where [CategoryId] = 34 and ({1}) )) ";
		//	string phraseTemplate = " (a.TextValue like '%{0}%') ";

		//	string AND = "";
		//	string OR = "";
		//	string keyword = "";

		//	foreach ( MainSearchFilter filter in data.Filters.Where( s => s.Name == "subjects" ) )
		//	{
		//		string next = "";
		//		if ( where.Length > 0 )
		//			AND = " AND ";
		//		foreach ( string item in filter.Items )
		//		{
		//			keyword = ServiceHelper.HandleApostrophes( item );
		//			if ( keyword.IndexOf( ";" ) > -1 )
		//			{
		//				var words = keyword.Split( ';' );
		//				foreach ( string word in words )
		//				{
		//					next += OR + string.Format( phraseTemplate, word.Trim() );
		//					OR = " OR ";
		//				}
		//			}
		//			else
		//			{
		//				next = string.Format( phraseTemplate, keyword.Trim() );
		//			}
		//			//next += keyword;	//					+",";
		//			//just handle one for now
		//			break;
		//		}
		//		//next = next.Trim( ',' );
		//		if ( !string.IsNullOrWhiteSpace( next ) )
		//			where = where + AND + string.Format( subjects, entity, next );

		//		break;
		//	}
		//}
		public static void SetBoundariesFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";
			string template = " ( base.RowId in ( SELECT  b.EntityUid FROM [dbo].[Entity.Address] a inner join Entity b on a.EntityId = b.Id    where [Longitude] < {0} and [Longitude] > {1} and [Latitude] < {2} and [Latitude] > {3} ) ) ";
			if ( data.SearchType == "credential" )
				template = template.Replace( "base.RowId", "base.EntityUid" );

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

			if ( data.SearchType == "credential" )
				template = template.Replace( "base.RowId", "base.EntityUid" );

			//Updated to use FiltersV2
			string next = "";
			if ( where.Length > 0 )
				AND = " AND ";
			foreach ( var filter in data.FiltersV2.Where( m => m.Type == MainSearchFilterV2Types.CODE ) )
			{
				var item = filter.AsCodeItem();
				if( item.CategoryId == 13 )
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
			*/
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
