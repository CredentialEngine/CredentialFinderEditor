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
		DateTime checkDate = new DateTime( 2016, 1, 1 );
		public static bool IncludingDescriptionInKeywordFilter = UtilityManager.GetAppKeyValue( "includingDescriptionInKeywordFilter", false );
		public MainSearchResults MainSearch( MainSearchInput data, ref bool valid, ref string status )
		{
			//Sanitize input
			data.Keywords = string.IsNullOrWhiteSpace( data.Keywords ) ? "" : data.Keywords;
			data.Keywords = ServiceHelper.CleanText( data.Keywords );
			data.Keywords = ServiceHelper.HandleApostrophes( data.Keywords );
			data.Keywords = data.Keywords.Trim();

			//Sanitize input
			var validSortOrders = new List<string>() { "newest", "relevance", "alpha", "cost_lowest", "cost_highest", "duration_shortest", "duration_longest", "org_alpha", "oldest" };
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
								return CredentialServices.Autocomplete( text, 20 );
							//case "competencies":
							//	return CredentialServices.AutocompleteCompetencies( text, 10 );
							case "subjects":
								return Autocomplete_Subjects( CF.CodesManager.ENTITY_TYPE_CREDENTIAL, CF.CodesManager.PROPERTY_CATEGORY_SUBJECT, text, 10 );
							case "occupations":
								return Autocomplete_Occupations( CF.CodesManager.ENTITY_TYPE_CREDENTIAL, text, 10 );
							case "industries":
								return Autocomplete_Industries( CF.CodesManager.ENTITY_TYPE_CREDENTIAL, text, 10 );
							case "instructionalprogramtype":
								return Autocomplete_Cip( CF.CodesManager.ENTITY_TYPE_CREDENTIAL, text, 10 );
							default: break;
						}
						break;
					}
				case "organization":
					{
						switch ( context.ToLower() )
						{
							case "mainsearch":
								return OrganizationServices.Autocomplete( text, 10 );
							case "industries":
								return Autocomplete_Industries( CF.CodesManager.ENTITY_TYPE_ORGANIZATION, text, 10 );
							default:
								break;
						}
						break;

					}
				case "assessment":
					{
						switch ( context.ToLower() )
						{
							case "mainsearch":
								return AssessmentServices.Autocomplete( text, 20 );
							//case "competencies":
							//	return AssessmentServices.Autocomplete( text, "competencies", 10 );
							case "subjects":
								return Autocomplete_Subjects( CF.CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, CF.CodesManager.PROPERTY_CATEGORY_SUBJECT, text, 10 );
							case "occupations":
								return Autocomplete_Occupations( CF.CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, text, 10 );
							case "industries":
								return Autocomplete_Industries( CF.CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, text, 10 );
							case "instructionalprogramtype":
								return Autocomplete_Cip( CF.CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, text, 10 );
							default: break;
						}
						break;
					}
				case "learningopportunity":
					{
						switch ( context.ToLower() )
						{
							case "mainsearch":
								return LearningOpportunityServices.Autocomplete( text, 20 );
							//case "competencies":
							//	return LearningOpportunityServices.Autocomplete( text, "competencies", 10 );
							case "subjects":
								return Autocomplete_Subjects( CF.CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, CF.CodesManager.PROPERTY_CATEGORY_SUBJECT, text, 10 );
							case "occupations":
								return Autocomplete_Occupations( CF.CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, text, 10 );
							case "industries":
								return Autocomplete_Industries( CF.CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, text, 10 );
							case "instructionalprogramtype":
								return Autocomplete_Cip( CF.CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, text, 10 );
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
						filter = string.Format( "(CredentialId = {0})", entityId );
						return CF.Entity_CompetencyManager.Search( filter, "", 1, maxRecords, ref pTotalRows );
					}
				case "assessment":
					{
						filter = string.Format( "(SourceEntityTypeId = 3 AND [SourceId] = {0})", entityId );
						return CF.Entity_CompetencyManager.Search( filter, "", 1, maxRecords, ref pTotalRows );
					}
				case "learningopportunity":
					{
						filter = string.Format( "(SourceEntityTypeId = 7 AND [SourceId] = {0})", entityId );
						return CF.Entity_CompetencyManager.Search( filter, "", 1, maxRecords, ref pTotalRows );
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
		public static List<string> EntityCIPSList( string searchType, int entityBaseId, int maxRecords = 10 )
		{
			var results = new List<string>();
			int entityTypeId = 0;
			int pTotalRows = 0;

			switch ( searchType.ToLower() )
			{
				case "credential":
					entityTypeId = 1;
					break;
				case "assessment":
					entityTypeId = 3;
					break;
				case "learningopportunity":
					entityTypeId = 7;
					break;
				default:
					break;
			}

			results = CF.Entity_FrameworkItemManager.GetAll( entityTypeId, entityBaseId, 23, maxRecords, ref pTotalRows );
			if ( pTotalRows < 10 )
			{
				//other is now included in the above method
				maxRecords = 10 - pTotalRows;
				//int totalRows = 0; 
				//var otherCip = CF.Entity_ReferenceManager.GetAll( entityTypeId, entityBaseId, 23, maxRecords, ref totalRows );
				//pTotalRows += totalRows;
				//results.AddRange( otherCip );
			}
			return results;
			
		}

		public static List<OrganizationAssertion> QAPerformedList( int orgId, int maxRecords = 10 )
		{
			return CF.Entity_AssertionManager.GetAllDirectAssertions( orgId, maxRecords );
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
						return CF.Entity_CompetencyManager.Search( filter, "", 1, maxRecords, ref pTotalRows );
					}
				case "assessment":
					{
						filter = string.Format( "(SourceEntityTypeId = 3 AND [SourceId] = {0})", entityId );
						return CF.Entity_CompetencyManager.Search( filter, "", 1, maxRecords, ref pTotalRows );
					}
				case "learningopportunity":
					{
						filter = string.Format( "(SourceEntityTypeId = 7 AND [SourceId] = {0})", entityId );
						return CF.Entity_CompetencyManager.Search( filter, "", 1, maxRecords, ref pTotalRows );
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

		public enum TagTypes { CONNECTIONS, QUALITY, LEVEL, OCCUPATIONS, INDUSTRIES, SUBJECTS, COMPETENCIES, TIME, COST, ORGANIZATIONTYPE, ORGANIZATIONSECTORTYPE, OWNED_BY, OFFERED_BY, ASMTS_OWNED_BY, LOPPS_OWNED_BY, ASMNT_DELIVER_METHODS, DELIVER_METHODS, SCORING_METHODS, ASSESSMENT_USE_TYPES, ASSESSMENT_METHOD_TYPES, INSTRUCTIONAL_PROGRAM, QAPERFORMED }

		public MainSearchResults ConvertCredentialResults( List<CredentialSummary> results, int totalResults, string searchType )
		{
			var output = new MainSearchResults() { TotalResults = totalResults, SearchType = searchType };
			foreach ( var item in results )
			{
				var mergedCosts = item.NumberOfCostProfileItems; // CostProfileMerged.FlattenCosts( item.EstimatedCost );
				var subjects = Deduplicate( item.Subjects );
				//var mergedConnections = item.CredentialsList.Results.Concat( item.HasPartsConnections.Results ).ToList();
				var mergedConnections = item.CredentialsList.Results.Concat( item.HasPartsList.Results ).Concat( item.IsPartOfList.Results ).ToList();

				var mergedQA = item.AgentAndRoles.Results.Concat( item.Org_QAAgentAndRoles.Results ).ToList();

				//var agentAndRoles = item.AgentAndRoles.Results;
				//var qaAgentAndRoles = item.Org_QAAgentAndRoles.Results;

				string entityLastUpdated = "";
				if ( item.EntityLastUpdated > checkDate )
				{
					entityLastUpdated = item.EntityLastUpdated.ToString( "yyyy-MM-dd HH:mm" );
				}
				string approvedStatus = "Not  Approved";
				if ( item.IsApproved )
					approvedStatus = string.Format( "Approved ({0})", item.LastApprovalDate );
				string publishedStatus = "Not Published";
				if ( !string.IsNullOrWhiteSpace( item.CredentialRegistryId ) )
				{
					//should have a published date, so leave as is until problem
					if ( !string.IsNullOrWhiteSpace( item.LastPublishDate ) )
						publishedStatus = string.Format( "Published ({0})", item.LastPublishDate );
					else
						publishedStatus = "Published";
				}

				output.Results.Add( Result( item.Name, item.FriendlyName, item.Description, item.Id,
					new Dictionary<string, object>()
					{
						{ "Name", item.Name },
						{ "Description", item.Description },
						{ "Type", item.CredentialType },
						{ "Owner", item.OwnerOrganizationName },
						{ "OwnerId", item.OwnerOrganizationId },
						{ "CanEditRecord", item.CanEditRecord },
						{ "ctid", item.CTID },
						{ "TypeSchema", item.CredentialTypeSchema.ToLower()},
						{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( item.Addresses ) } },
						{ "Locations", ConvertAddresses( item.Addresses ) },
						{ "SearchType", searchType },
						{ "EditorType", searchType },
						{ "RecordId", item.Id },
						{ "UrlTitle", item.FriendlyName },
						{ "HasBadge", item.HasVerificationType_Badge }, //Indicate existence of badge here
                        { "EntityLastUpdated", entityLastUpdated },

						{ "IsApproved", item.IsApproved },
						{ "LastApprovalDate", item.LastApprovalDate },
						{ "ContentApprovedBy", item.ContentApprovedBy },
						{ "ApprovedStatus", approvedStatus },
						{ "CredentialRegistryId", item.CredentialRegistryId },

						 { "PublishedStatus", publishedStatus }
                        //{ "PublishedStatus", string.IsNullOrWhiteSpace( item.CredentialRegistryId ) ? "Not Published" : "Published" }

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
							TotalItems = mergedConnections.Count(),
							SearchQueryType = "link",
							//Items = GetSearchTagItems_Filter( item.ConnectionsList.Results, "{Name} Credential(s)", item.ConnectionsList.CategoryId )
							//Something like this...
							/*	*/
							Items = mergedConnections.Take(10).ToList().ConvertAll(m => new Models.Helpers.SearchTagItem()
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
						
						//Credential Quality Assurance
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Quality Assurance",
							DisplayTemplate = "{#} Quality Assurance",
							Name = "qualityAssuranceBy", //Using the "quality" enum breaks this filter since it tries to find the matching item in the checkbox list and it doesn't exist
							//TotalItems = item.AgentAndRoles.Results.Count(),
							TotalItems = mergedQA.Count(),
							SearchQueryType = "merged", //Change this to "custom", or back to detail
							Items = mergedQA.ConvertAll( m => new Models.Helpers.SearchTagItem() {
							Display = "<b>" + m.Relationship + "</b>" + " by " + m.Agent, //[Accredited By] [Organization 1]
							QueryValues = new Dictionary<string, object>() {
									{ "RoleId", m.RelationshipId },
									{ "AgentId", m.AgentId },
									{ "AgentUrl", m.AgentUrl },
									{ "IsThirdPartyOrganization", m.IsThirdPartyOrganization },
									{ "Relationship", m.Relationship },
									{ "TextValue", m.Agent },
                                //    { "RelationshipId", m.RelationshipId },
                                     //  { "CodeId", m.AgentId },
									{ "TargetType", m.EntityType }, //Probably okay to hard code this for now
                                    //{ "AgentUrl", m.AgentUrl},
                                    // { "ctid", m.CTID },
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
							Name = TagTypes.OCCUPATIONS.ToString().ToLower(),
							TotalItems = item.OccupationResults.Results.Count(),
                           // SearchQueryType = "framework",
                            SearchQueryType = "text",
							Items = item.OccupationResults.Results.Take(10).ToList().ConvertAll( m => new Models.Helpers.SearchTagItem() {
								Display = m.CodeTitle,
								QueryValues = new Dictionary<string, object>(){
									{ "CategoryId", m.CategoryId },
									{ "CodeId", m.Id },
									{ "SchemaName", m.SchemaName },
									{ "TextValue", m.CodeTitle }} } )
                            //Items = GetSearchTagItems_Filter( item.OccupationResults.Results, "{Name}", item.OccupationResults.CategoryId )
                        },
						//Industries
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Industries",
							DisplayTemplate = "{#} Industr{ies}",
							Name = TagTypes.INDUSTRIES.ToString().ToLower(),
							TotalItems = item.IndustryResults.Results.Count(),
                            //SearchQueryType = "framework",
                            //Items = GetSearchTagItems_Filter( item.NaicsResults.Results, "{Name}", item.NaicsResults.CategoryId )
                            SearchQueryType = "text",
							Items = item.IndustryResults.Results.Take(10).ToList().ConvertAll( m => new Models.Helpers.SearchTagItem() {
								Display = m.CodeTitle,
								QueryValues = new Dictionary<string, object>() {
									{ "CategoryId", m.CategoryId },
									{ "CodeId", m.Id },
									{ "SchemaName", m.SchemaName },
									{ "TextValue", m.CodeTitle } } } )
						},
						
						//Instructional Program Classfication
				        new Models.Helpers.SearchTag()
						{
							CategoryName = "InstructionalProgramType",
							DisplayTemplate = "{#} Instructional Program{s}",
							Name = "instructionalprogramtype",
							TotalItems = item.InstructionalProgramCounts,
							SearchQueryType = "text",
							IsAjaxQuery = true,
							AjaxQueryName = "GetSearchResultsCIPs",
							AjaxQueryValues = new Dictionary<string, object>()
							{
								{ "SearchType", "credential" },
								{ "RecordId", item.Id },
								{ "TargetEntityType", "INSTRUCTIONAL_PROGRAM" },
								//{ "TextValue", m.CodeTitle }
							}
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
						
						//Assessment Delivery Type
                        new Models.Helpers.SearchTag()
						{
							CategoryName = "AssessmentDeliveryType",
							DisplayTemplate = "{#} Assessment Delivery Type{s}",
							Name = TagTypes.ASMNT_DELIVER_METHODS.ToString().ToLower(),
							TotalItems = item.AssessmentDeliveryType.Results.Count(),
							SearchQueryType = "code",
							Items = GetSearchTagItems_Filter(item.AssessmentDeliveryType.Results, "{Name}", item.AssessmentDeliveryType.CategoryId)
						},
						//Learning Delivery Type
                        new Models.Helpers.SearchTag()
						{
							CategoryName = "LearningDeliveryType",
							DisplayTemplate = "{#} Learning Delivery Type{s}",
							Name = TagTypes.DELIVER_METHODS.ToString().ToLower(),
							TotalItems = item.LearningDeliveryType.Results.Count(),
							SearchQueryType = "code",
							Items = GetSearchTagItems_Filter(item.LearningDeliveryType.Results, "{Name}", item.LearningDeliveryType.CategoryId)
						},
						//Durations
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Time Estimates",
							DisplayTemplate = "Time Estimate{s}",
							Name = TagTypes.TIME.ToString().ToLower(),
							TotalItems = item.EstimatedTimeToEarn.Count(), //# of duration profiles
							SearchQueryType = "detail", //Not sure how this could be any kind of search query
							Items = item.EstimatedTimeToEarn.ConvertAll(m => new Models.Helpers.SearchTagItem()
							{
								Display = m.IsRange ? m.MinimumDuration.Print() + " - " + m.MaximumDuration.Print() :  string.IsNullOrEmpty(m.ExactDuration.Print()) ? m.Conditions : m.ExactDuration.Print(),
								QueryValues = new Dictionary<string, object>()
								{
									{ "ExactDuration", m.ExactDuration },
									{ "MinimumDuration", m.MinimumDuration },
									{ "MaximumDuration", m.MaximumDuration },
									{ "Conditions", m.Conditions}
								}
							} ).ToList()
						},
						//Costs
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Costs",
							DisplayTemplate = "Cost{s}",
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
			foreach ( var item in items )
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
				Display = Models.Helpers.SearchTagHelper.Count( displayTemplate.Replace( "{Name}", m.CodeTitle ), 1 ),
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
							Items = data.ConvertAll( m => new TagItem() { CodeId = m.Id, Label = m.TargetNodeName, Description = m.Description } )
						};
						break;
					}

				case TagTypes.COST:
					{
						var data = SearchServices.EntityCostsList( searchType, recordID, maxRecords );
						result = new TagSet()
						{
							Schema = TagTypes.COST.ToString().ToLower(),
							Label = "Costs",
							Method = "direct",
							CostItems = data.ConvertAll( c => new CostTagItem()
							{
								CodeId = c.CostProfileId,
								Price = c.Price,
								CostType = c.CostTypeName,
								CurrencySymbol = c.CurrencySymbol,
								SourceEntity = c.ParentEntityType
							} ),
							Items = data.ConvertAll( m => new TagItem() { CodeId = m.Id, Label = m.CostTypeName, Description = "" } )
						};
						break;
					}
				case TagTypes.INSTRUCTIONAL_PROGRAM:
					{
						var data = SearchServices.EntityCIPSList( searchType, recordID, maxRecords );
						result = new TagSet()
						{
							Schema = TagTypes.INSTRUCTIONAL_PROGRAM.ToString().ToLower(),
							Label = "InstructionalProgramType",
							Method = "detail",
							Items = data.ConvertAll( m => new TagItem() { Label = m  } )
						};
						break;
					}
				case TagTypes.QAPERFORMED:
					{
						var data = SearchServices.QAPerformedList( recordID, maxRecords );
						result = new TagSet()
						{
							Schema = TagTypes.QAPERFORMED.ToString().ToLower(),
							Label = "Quality Assurance Performed",
							Method = "qaperformed",
							QAItems = data.ConvertAll( q => new QAPerformedTagItem()
							{
								TargetEntityBaseId = q.TargetEntityBaseId,
								TargetEntityName = q.TargetEntityName,
								TargetEntityType = q.TargetEntityType,
								AssertionTypeId = q.AssertionTypeId,
								TargetEntitySubjectWebpage = q.TargetEntitySubjectWebpage,
								AgentToTargetRelationship = q.Relationship,
								IsReference = string.IsNullOrEmpty( q.TargetCTID )
							} )
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
			string editorType = searchType;
			string qaorgEditor = "qaorganization";

			foreach ( var item in results )
			{
				string entityLastUpdated = "";
				if ( item.EntityLastUpdated > checkDate )
				{
					entityLastUpdated = item.EntityLastUpdated.ToString( "yyyy-MM-dd HH:mm" );
				}
				string approvedStatus = "Not  Approved";
				if ( item.IsApproved )
					approvedStatus = string.Format( "Approved ({0})", item.LastApprovalDate );
				string publishedStatus = "Not Published";
				if ( !string.IsNullOrWhiteSpace( item.CredentialRegistryId ) )
				{
					//should have a published date, so leave as is until problem
					if ( !string.IsNullOrWhiteSpace( item.LastPublishDate ) )
						publishedStatus = string.Format( "Published ({0})", item.LastPublishDate );
					else
						publishedStatus = "Published";
				}

				if ( item.IsAQAOrg )
					editorType = qaorgEditor;
				else
					editorType = searchType;
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
						{ "EditorType", editorType },
						{ "ctid", item.CTID },
						{ "RecordId", item.Id },
						{ "UrlTitle", item.FriendlyName },
						{ "Logo", item.ImageUrl },
						{ "ResultImageUrl", item.ImageUrl ?? "" },
						{ "Location", item.Addresses.Count() > 1 ? "Multiple Locations" : item.Address.Country + ( string.IsNullOrWhiteSpace( item.Address.Country ) ? "" : " - " ) + item.Address.City + ( string.IsNullOrWhiteSpace( item.Address.City ) ? "" : ", " ) + item.Address.AddressRegion },

						{ "Coordinates", new { Type = "coordinates", Data = new { Latitude = item.Address.Latitude, Longitude = item.Address.Longitude } } },
						{ "IsQA", item.IsAQAOrg ? "true" : "false" },

						{ "EntityLastUpdated", entityLastUpdated},
						{ "IsApproved", item.IsApproved },
						{ "LastApprovalDate", item.LastApprovalDate },
						{ "ContentApprovedBy", item.ContentApprovedBy },
						{ "ApprovedStatus", approvedStatus },
						{ "CredentialRegistryId", item.CredentialRegistryId },

						 { "PublishedStatus", publishedStatus }
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
							DisplayTemplate = "{#} Sector{s}",
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
                   #region OLD QA
        //              //  accredited by orgs

        //                new Models.Helpers.SearchTag()
        //                {
        //                    CategoryName = "AccreditedByOrgs",
        //                    DisplayTemplate = "Accredited by {#} Organization{s}",
        //                    Name = TagTypes.CONNECTIONS.ToString().ToLower(),
        //                    TotalItems = item.AccreditedByResults.Results.Count(),
        //                    SearchQueryType = "link",
        //                    Items = item.AccreditedByResults.Results.ConvertAll(m => new Models.Helpers.SearchTagItem()
        //                    {
        //                        Display = m.Title,
        //                        QueryValues = new Dictionary<string, object>()
        //                        {
        //                            { "TargetId", m.Id }, //org ID
								//	{ "TargetType", "organization" }, //??
								//}
        //                    } ).ToList()
        //                },
              
        //                //approved by orgs

        //                new Models.Helpers.SearchTag()
        //                {
        //                    CategoryName = "ApprovedByOrgs",
        //                    DisplayTemplate = "Approved by {#} Organization{s}",
        //                    Name = TagTypes.CONNECTIONS.ToString().ToLower(),
        //                    TotalItems = item.ApprovedByResults.Results.Count(),
        //                    SearchQueryType = "link",
        //                    Items = item.ApprovedByResults.Results.ConvertAll(m => new Models.Helpers.SearchTagItem()
        //                    {
        //                        Display = m.Title,
        //                        QueryValues = new Dictionary<string, object>()
        //                        {
        //                            { "TargetId", m.Id },
        //                            { "TargetType", "organization" }, //??
								//}
        //                    } ).ToList()
        //                },
                 #endregion
                        //Quality Assurance
                        new Models.Helpers.SearchTag()
						{
							CategoryName = "Quality Assurance",
							DisplayTemplate = "{#} Quality Assurance",
							Name = "qualityAssuranceBy", //Using the "quality" enum breaks this filter since it tries to find the matching item in the checkbox list and it doesn't exist
							TotalItems = item.QualityAssurance.Results.Count(),
							SearchQueryType = "merged", //Change this to "custom", or back to detail
							Items = item.QualityAssurance.Results.Take(10).ToList().ConvertAll( m => new Models.Helpers.SearchTagItem() {
							Display = "<b>" + m.Relationship + "</b>" + " " + m.Agent,  //[Accredited By] [Organization 1]
							QueryValues = new Dictionary<string, object>() {
									{ "TargetType", "organization" },
									{ "AgentId", m.AgentId },
									{ "AgentUrl", m.AgentUrl },
									{ "IsThirdPartyOrganization", m.IsThirdPartyOrganization == "1" },
									{ "Relationship", m.Relationship },
									{ "TextValue", m.Agent },
									 { "RoleId", m.RelationshipId },
								}
							} ).ToList()

						},
						//Quality Assurance Performed
                        new Models.Helpers.SearchTag()
						{
							CategoryName = "Quality Assurance Performed",
							DisplayTemplate = "{#} Quality Assurance Performed",
							Name = "qualityassuranceperformed",
							TotalItems = item.AssertionsTotal,
							SearchQueryType = "qaperformed",
							IsAjaxQuery = true,
							AjaxQueryName = "GetSearchResultPerformed",
							AjaxQueryValues = new Dictionary<string, object>()
							{
								{ "SearchType", "organiation" },
								{ "RecordId", item.Id },
								{ "TargetEntityType", "QAPERFORMED" }
							}
						},
						//Industries
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Industries",
							DisplayTemplate = "{#} Industr{ies}",
							Name = TagTypes.INDUSTRIES.ToString().ToLower(),
							TotalItems = item.NaicsResults.Results.Count(),
                            //SearchQueryType = "framework",
                            //Items = GetSearchTagItems_Filter( item.NaicsResults.Results, "{Name}", item.NaicsResults.CategoryId )
                            SearchQueryType = "text",
							Items = item.NaicsResults.Results.Take(10).ToList().ConvertAll( m => new Models.Helpers.SearchTagItem() { Display = m.CodeTitle, QueryValues = new Dictionary<string, object>() { { "TextValue", m.CodeTitle } } } )
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

			string editorType = searchType;
			string qaorgEditor = "qaorganization";
			foreach ( var item in results )
			{
				string entityLastUpdated = "";
				if ( item.EntityLastUpdated > checkDate )
				{
					entityLastUpdated = item.EntityLastUpdated.ToString( "yyyy-MM-dd HH:mm" );
				}
				string approvedStatus = "Not  Approved";
				if ( item.IsApproved )
					approvedStatus = string.Format( "Approved ({0})", item.LastApprovalDate );
				string publishedStatus = "Not Published";
				if ( !string.IsNullOrWhiteSpace( item.CredentialRegistryId ) )
				{
					//should have a published date, so leave as is until problem
					if ( !string.IsNullOrWhiteSpace( item.LastPublishDate ) )
						publishedStatus = string.Format( "Published ({0})", item.LastPublishDate );
					else
						publishedStatus = "Published";
				}

				if ( item.IsAQAOrg )
					editorType = qaorgEditor;
				else
					editorType = searchType;

				output.Results.Add( Result( item.Name, item.FriendlyName, item.Description, item.Id,
					new Dictionary<string, object>()
					{
						{ "Name", item.Name },
						{ "Description", item.Description },
						{ "OwnerId", 0 },
						{ "CanEditRecord", item.CanEditRecord },
						{ "ctid", item.CTID },
						{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( new List<Address>() { item.Address } ) } },
						{ "Locations", ConvertAddresses( new List<Address>() { item.Address } ) },
						{ "SearchType", searchType },
						{ "EditorType", editorType },
						{ "RecordId", item.Id },
						{ "UrlTitle", item.FriendlyName },
						{ "Logo", item.ImageUrl },
						{ "ResultImageUrl", item.ImageUrl ?? "" },
						{ "Location", item.Address.Country + ( string.IsNullOrWhiteSpace( item.Address.Country ) ? "" : " - " ) + item.Address.City + ( string.IsNullOrWhiteSpace( item.Address.City ) ? "" : ", " ) + item.Address.AddressRegion },

						{ "Coordinates", new { Type = "coordinates", Data = new { Latitude = item.Address.Latitude, Longitude = item.Address.Longitude } } },
						{ "IsQA", item.IsAQAOrg ? "true" : "false" },

						{ "EntityLastUpdated", entityLastUpdated},
						{ "IsApproved", item.IsApproved },
						{ "LastApprovalDate", item.LastApprovalDate },
						{ "ContentApprovedBy", item.ContentApprovedBy },
						{ "ApprovedStatus", approvedStatus },
						{ "CredentialRegistryId", item.CredentialRegistryId },

						 { "PublishedStatus", publishedStatus }
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
				var subjects = Deduplicate( item.Subjects );
				//18-03-12 mp - this doesn't work, as AssessmentConnectionsList is not populated - need to update search results!
				var mergedConnections = item.AssessmentConnectionsList.Results;

				string entityLastUpdated = "";
				if ( item.EntityLastUpdated > checkDate )
					entityLastUpdated = item.EntityLastUpdated.ToString( "yyyy-MM-dd HH:mm" );
				string approvedStatus = "Not  Approved";
				if ( item.IsApproved )
					approvedStatus = string.Format( "Approved ({0})", item.LastApprovalDate );
				string publishedStatus = "Not Published";
				if ( !string.IsNullOrWhiteSpace( item.CredentialRegistryId ) )
				{
					//should have a published date, so leave as is until problem
					if ( !string.IsNullOrWhiteSpace( item.LastPublishDate ) )
						publishedStatus = string.Format( "Published ({0})", item.LastPublishDate );
					else
						publishedStatus = "Published";
				}
				output.Results.Add( Result( item.Name, item.Description, item.Id,
					new Dictionary<string, object>()
					{
						{ "Name", item.Name },
						{ "Description", item.Description },
						{ "Owner", string.IsNullOrWhiteSpace( item.OrganizationName ) ? "" : item.OrganizationName },
						{ "OwnerId", item.OwningOrganizationId },
						{ "CanEditRecord", item.CanEditRecord },
						{ "ctid", item.CTID },
						{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( item.Addresses ) } },
						{ "Locations", ConvertAddresses( item.Addresses ) },
						{ "SearchType", searchType },
						{ "EditorType", searchType },
						{ "RecordId", item.Id },
						{ "UrlTitle", item.FriendlyName },
						{ "EntityLastUpdated", entityLastUpdated },

						{ "IsApproved", item.IsApproved },
						{ "LastApprovalDate", item.LastApprovalDate },
						{ "ContentApprovedBy", item.ContentApprovedBy },
						{ "CredentialRegistryId", item.CredentialRegistryId },

						{ "LastPublishDate", item.LastPublishDate },
						 { "ApprovedStatus", approvedStatus },
						 { "PublishedStatus", publishedStatus }
					},
					null,

					new List<Models.Helpers.SearchTag>()
					{
                        //Connections
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Connections",
							DisplayTemplate = "{#} Connection{s}",
							Name = TagTypes.CONNECTIONS.ToString().ToLower(),
							TotalItems = mergedConnections.Count(),
							SearchQueryType = "link",
							Items = mergedConnections.ConvertAll(m => new Models.Helpers.SearchTagItem()
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
                        //Subjects
                        new Models.Helpers.SearchTag()
						{
							CategoryName = "Subjects",
							DisplayTemplate = "{#} Subject{s}",
							Name = TagTypes.SUBJECTS.ToString().ToLower(),
							TotalItems = subjects.Count(), //Returns a count of the de-duplicated items
                            SearchQueryType = "text",
							Items = subjects.ConvertAll(m => new Models.Helpers.SearchTagItem() { Display = m, QueryValues = new Dictionary<string, object>() { { "TextValue", m } } })
						},
                        //AssessmentMethodTypes
                        new Models.Helpers.SearchTag()
						{
							CategoryName = "AssessmentMethodTypes",
							DisplayTemplate = "{#} Assessment Method Type{s}",
							Name = TagTypes.ASSESSMENT_METHOD_TYPES.ToString().ToLower(),
							TotalItems = item.AssessmentMethodTypes.Results.Count(),
							SearchQueryType = "code",
							Items = GetSearchTagItems_Filter(item.AssessmentMethodTypes.Results, "{Name}", item.AssessmentMethodTypes.CategoryId)
						},
                        //AssessmentUseTypes
                        new Models.Helpers.SearchTag()
						{
							CategoryName = "AssessmentUseTypes",
							DisplayTemplate = "{#} Assessment Use Type{s}",
							Name = TagTypes.ASSESSMENT_USE_TYPES.ToString().ToLower(),
							TotalItems = item.AssessmentUseTypes.Results.Count(),
							SearchQueryType = "code",
							Items = GetSearchTagItems_Filter(item.AssessmentUseTypes.Results, "{Name}", item.AssessmentUseTypes.CategoryId)
						},
                        //ScoringMethodTypes
                        new Models.Helpers.SearchTag()
						{
							CategoryName = "ScoringMethodTypes",
							DisplayTemplate = "{#} Scoring Method Type{s}",
							Name = TagTypes.SCORING_METHODS.ToString().ToLower(),
							TotalItems = item.ScoringMethodTypes.Results.Count(),
							SearchQueryType = "code",
							Items = GetSearchTagItems_Filter(item.ScoringMethodTypes.Results, "{Name}", item.ScoringMethodTypes.CategoryId)
						},
                        //Delivery Type
                        new Models.Helpers.SearchTag()
						{
							CategoryName = "DeliveryMethodTypes",
							DisplayTemplate = "{#} Delivery Method Type{s}",
							Name = TagTypes.DELIVER_METHODS.ToString().ToLower(),
							TotalItems = item.DeliveryMethodTypes.Results.Count(),
							SearchQueryType = "code",
							Items = GetSearchTagItems_Filter(item.DeliveryMethodTypes.Results, "{Name}", item.DeliveryMethodTypes.CategoryId)
						},

                        //Quality Assurance
                        new Models.Helpers.SearchTag()
						{
							CategoryName = "Quality Assurance",
							DisplayTemplate = "{#} Quality Assurance",
							Name = "qualityAssuranceBy", //Using the "quality" enum breaks this filter since it tries to find the matching item in the checkbox list and it doesn't exist
							TotalItems = item.QualityAssurance.Results.Count(),
							SearchQueryType = "merged", //Change this to "custom", or back to detail
							Items = item.QualityAssurance.Results.Take(10).ToList().ConvertAll( m => new Models.Helpers.SearchTagItem() {
							Display = "<b>" + m.Relationship + "</b>" + " " + m.Agent,  //[Accredited By] [Organization 1]
							QueryValues = new Dictionary<string, object>() {
									{ "TargetType", "assessment" },
									{ "AgentId", m.AgentId },
									{ "AgentUrl", m.AgentUrl },
									{ "IsThirdPartyOrganization", m.IsThirdPartyOrganization == "1" },
									{ "Relationship", m.Relationship },
									{ "TextValue", m.Agent },
									{ "RoleId", m.RelationshipId },
								}
							} ).ToList()

						},

						//Occupations
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Occupations",
							DisplayTemplate = "{#} Occupation{s}",
							Name = TagTypes.OCCUPATIONS.ToString().ToLower(),
							TotalItems = item.OccupationResults.Results.Count(),
                            SearchQueryType = "text",
							Items = item.OccupationResults.Results.Take(10).ToList().ConvertAll( m => new Models.Helpers.SearchTagItem() {
								Display = m.CodeTitle,
								QueryValues = new Dictionary<string, object>(){
									{ "CategoryId", m.CategoryId },
									{ "CodeId", m.Id },
									{ "SchemaName", m.SchemaName },
									{ "TextValue", m.CodeTitle }} } )
                        },
						//Industries
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Industries",
							DisplayTemplate = "{#} Industr{ies}",
							Name = TagTypes.INDUSTRIES.ToString().ToLower(),
							TotalItems = item.IndustryResults.Results.Count(),
                            SearchQueryType = "text",
							Items = item.IndustryResults.Results.Take(10).ToList().ConvertAll( m => new Models.Helpers.SearchTagItem() {
								Display = m.CodeTitle,
								QueryValues = new Dictionary<string, object>() {
									//{ "CategoryId", m.CategoryId },
									//{ "CodeId", m.Id },
									//{ "SchemaName", m.SchemaName },
									{ "TextValue", m.CodeTitle } } } )
						},
						
                        //Instructional Program Classfication
						new Models.Helpers.SearchTag()
						{
							CategoryName = "InstructionalProgramType",
							DisplayTemplate = "{#} Instructional Program{s}",
							Name = "instructionalprogramtype",
							TotalItems = item.InstructionalProgramResults.Results.Count(),
                            SearchQueryType = "text",
							Items = item.InstructionalProgramResults.Results.Take(10).ToList().ConvertAll( m => new Models.Helpers.SearchTagItem() {
								Display = m.CodeTitle,
								QueryValues = new Dictionary<string, object>() {
									{ "TextValue", m.CodeTitle } } } )
						},

						//Competencies
						new Models.Helpers.SearchTag()
						{
							CategoryName = "AssessesCompetencies",
							DisplayTemplate = "Assesses {#} Competenc{ies}",
							Name = TagTypes.COMPETENCIES.ToString().ToLower(),
							TotalItems = item.AssessesCompetenciesCount,
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
								Display = string.IsNullOrWhiteSpace(m.TargetNodeDescription) ?
								m.TargetNodeName :
								"<b>" + m.TargetNodeName + "</b>" + System.Environment.NewLine + m.TargetNodeDescription,
								QueryValues = new Dictionary<string, object>()
								{
									{ "SchemaName", null },
									{ "CodeId", m.Id },
									{ "TextValue", m.TargetNodeName },
									{ "TextDescription", m.TargetNodeDescription }
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
			/*
             * 
						{ "Competencies", new { Type = "tags", Data = ConvertCompetenciesToDictionary( item.TeachesCompetencies ) } },
             */
			var output = new MainSearchResults() { TotalResults = totalResults, SearchType = searchType };
			foreach ( var item in results )
			{
				var subjects = Deduplicate( item.Subjects );
				var mergedConnections = item.LearningOppConnectionsList.Results;

				string entityLastUpdated = "";
				if ( item.EntityLastUpdated > checkDate )
					entityLastUpdated = item.EntityLastUpdated.ToString( "yyyy-MM-dd HH:mm" );
				string approvedStatus = "Not  Approved";
				if ( item.IsApproved )
					approvedStatus = string.Format( "Approved ({0})", item.LastApprovalDate );
				string publishedStatus = "Not Published";
				if ( !string.IsNullOrWhiteSpace( item.CredentialRegistryId ) )
				{
					//should have a published date, so leave as is until problem
					if ( !string.IsNullOrWhiteSpace( item.LastPublishDate ) )
						publishedStatus = string.Format( "Published ({0})", item.LastPublishDate );
					else
						publishedStatus = "Published";
				}
				output.Results.Add( Result( item.Name, item.Description, item.Id,
					new Dictionary<string, object>()
					{
						{ "Name", item.Name },
						{ "Description", item.Description },
						{ "Owner", string.IsNullOrWhiteSpace( item.OrganizationName ) ? "" : item.OrganizationName },
						{ "OwnerId", item.OwningOrganizationId },
						{ "CanEditRecord", item.CanEditRecord },
						{ "ctid", item.CTID },
						{ "AvailableAt", new { Type = "locations", Data = ConvertAddresses( item.Addresses ) } },
						{ "Locations", ConvertAddresses( item.Addresses ) },
						{ "SearchType", searchType },
						{ "EditorType", searchType },
						{ "RecordId", item.Id },
						{ "UrlTitle", item.FriendlyName },
						{ "EntityLastUpdated", entityLastUpdated },

						{ "IsApproved", item.IsApproved },
						{ "LastApprovalDate", item.LastApprovalDate },
						{ "ContentApprovedBy", item.ContentApprovedBy },

						{ "LastPublishDate", item.LastPublishDate },
						{ "ApprovedStatus", approvedStatus },
						{ "CredentialRegistryId", item.CredentialRegistryId },
						{ "PublishedStatus", publishedStatus }
					},
					null,
					new List<Models.Helpers.SearchTag>()
					{
                        //Connections
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Connections",
							DisplayTemplate = "{#} Connection{s}",
							Name = TagTypes.CONNECTIONS.ToString().ToLower(),
							TotalItems = mergedConnections.Count(),
							SearchQueryType = "link",
							Items = mergedConnections.ConvertAll(m => new Models.Helpers.SearchTagItem()
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
						//  //Delivery Type
      //                  new Models.Helpers.SearchTag()
						//{
						//	CategoryName = "DeliveryMethodTypes",
						//	DisplayTemplate = "{#} Delivery Method Type{s}",
						//	Name = TagTypes.DELIVER_METHODS.ToString().ToLower(),
						//	TotalItems = item.DeliveryMethodTypes.Results.Count(),
						//	SearchQueryType = "code",
						//	Items = GetSearchTagItems_Filter(item.DeliveryMethodTypes.Results, "{Name}", item.DeliveryMethodTypes.CategoryId)
						//},
                        
                        //Quality Assurance
                        new Models.Helpers.SearchTag()
						{
							CategoryName = "Quality Assurance",
							DisplayTemplate = "{#} Quality Assurance",
							Name = "qualityAssuranceBy", //Using the "quality" enum breaks this filter since it tries to find the matching item in the checkbox list and it doesn't exist
							TotalItems = item.QualityAssurance.Results.Count(),
							SearchQueryType = "merged", //Change this to "custom", or back to detail
							Items = item.QualityAssurance.Results.Take(10).ToList().ConvertAll( m => new Models.Helpers.SearchTagItem() {
							Display = "<b>" + m.Relationship + "</b>" + " " + m.Agent,  //[Accredited By] [Organization 1]
							QueryValues = new Dictionary<string, object>() {
									{ "TargetType", "learningopportunity" },
									{ "AgentId", m.AgentId },
									{ "AgentUrl", m.AgentUrl },
									{ "IsThirdPartyOrganization", m.IsThirdPartyOrganization == "1"},
									{ "Relationship", m.Relationship },
									{ "TextValue", m.Agent },
									{ "RoleId", m.RelationshipId },
								}
							} ).ToList()

						},

                        //Subjects
                        new Models.Helpers.SearchTag()
						{
							CategoryName = "Subjects",
							DisplayTemplate = "{#} Subject{s}",
							Name = TagTypes.SUBJECTS.ToString().ToLower(),
							TotalItems = subjects.Count(), //Returns a count of the de-duplicated items
                            SearchQueryType = "text",
							Items = subjects.ConvertAll(m => new Models.Helpers.SearchTagItem() { Display = m, QueryValues = new Dictionary<string, object>() { { "TextValue", m } } })
						},

						//Occupations
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Occupations",
							DisplayTemplate = "{#} Occupation{s}",
							Name = TagTypes.OCCUPATIONS.ToString().ToLower(),
							TotalItems = item.OccupationResults.Results.Count(),
							SearchQueryType = "text",
							Items = item.OccupationResults.Results.Take(10).ToList().ConvertAll( m => new Models.Helpers.SearchTagItem() {
								Display = m.CodeTitle,
								QueryValues = new Dictionary<string, object>(){
									{ "CategoryId", m.CategoryId },
									{ "CodeId", m.Id },
									{ "SchemaName", m.SchemaName },
									{ "TextValue", m.CodeTitle }} } )
						},
						//Industries
						new Models.Helpers.SearchTag()
						{
							CategoryName = "Industries",
							DisplayTemplate = "{#} Industr{ies}",
							Name = TagTypes.INDUSTRIES.ToString().ToLower(),
							TotalItems = item.IndustryResults.Results.Count(),
							SearchQueryType = "text",
							Items = item.IndustryResults.Results.Take(10).ToList().ConvertAll( m => new Models.Helpers.SearchTagItem() {
								Display = m.CodeTitle,
								QueryValues = new Dictionary<string, object>() {
									//{ "CategoryId", m.CategoryId },
									//{ "CodeId", m.Id },
									//{ "SchemaName", m.SchemaName },
									{ "TextValue", m.CodeTitle } } } )
						},
						
                        //Instructional Program Classfication
				        new Models.Helpers.SearchTag()
						{
							CategoryName = "InstructionalProgramType",
							DisplayTemplate = "{#} Instructional Program{s}",
							Name = "instructionalprogramtype",
							TotalItems = item.InstructionalProgramResults.Results.Count(),
							SearchQueryType = "text",
                           //Items = GetSearchTagItems_Filter( item.InstructionalProgramClassification.Results, "{Name}", item.InstructionalProgramClassification.CategoryId )
                            Items = item.InstructionalProgramResults.Results.Take(10).ToList().ConvertAll( m => new Models.Helpers.SearchTagItem() { Display = m.CodeTitle, QueryValues = new Dictionary<string, object>() { { "TextValue", m.CodeTitle } } } )
						},
						//Competencies
						new Models.Helpers.SearchTag()
						{
							CategoryName = "TeachesCompetencies",
							DisplayTemplate = "Teaches {#} Competenc{ies}",
							Name = TagTypes.COMPETENCIES.ToString().ToLower(),
							TotalItems = item.TeachesCompetenciesCount,
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
						//Competencies direct???
						new Models.Helpers.SearchTag()
						{
							CategoryName = "TeachesCompetenciesDirect",
							DisplayTemplate = "Teaches {#} Competenc{ies} Direct",
							Name = TagTypes.COMPETENCIES.ToString().ToLower(),
							TotalItems = item.TeachesCompetencies.Count(),
							SearchQueryType = "detail",
							Items = item.TeachesCompetencies.ConvertAll( m => new Models.Helpers.SearchTagItem()
							{
								Display = string.IsNullOrWhiteSpace(m.TargetNodeDescription) ?
								m.TargetNodeName :
								"<b>" + m.TargetNodeName + "</b>" + System.Environment.NewLine + m.TargetNodeDescription,
								QueryValues = new Dictionary<string, object>()
								{
									{ "SchemaName", null },
									{ "CodeId", m.Id },
									{ "TextValue", m.TargetNodeName },
									{ "TextDescription", m.TargetNodeDescription }
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
						result.Add( item.Id.ToString(), item.TargetNodeDescription );
					}
					catch { }
				}
			}
			return result;
		}
		public MainSearchResult Result( string name, string description, int recordID,
			Dictionary<string, object> properties,
			List<TagSet> tags,
			List<Models.Helpers.SearchTag> tagsV2 = null )
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
		public static List<string> Autocomplete_Occupations( int entityTypeId, string keyword, int maxTerms = 25 )
		{
			return CF.Entity_ReferenceManager.QuickSearch_ReferenceFrameworks( entityTypeId, 11, keyword, maxTerms );
		}

		public static List<string> Autocomplete_Industries( int entityTypeId, string keyword, int maxTerms = 25 )
		{
			return CF.Entity_ReferenceManager.QuickSearch_ReferenceFrameworks( entityTypeId, 10, keyword, maxTerms );
		}
		public static List<string> Autocomplete_Cip( int entityTypeId, string keyword, int maxTerms = 25 )
		{
			return CF.Entity_ReferenceManager.QuickSearch_ReferenceFrameworks( entityTypeId, 23, keyword, maxTerms );
		}

		#region Common filters
		public static void HandleCustomFilters( MainSearchInput data, int searchCategory, ref string where, int userId = 0 )
		{
			string AND = "";
			string OR = "";
			//may want custom category for each one, to prevent requests that don't match the current search

			string sql = "";
			string customFilter = "";
			//Updated to use FilterV2
			if ( where.Length > 0 )
			{
				AND = " AND ";
			}
			foreach ( var filter in data.FiltersV2.Where( m => m.Type == MainSearchFilterV2Types.CODE ).ToList() )
			{
				var item = filter.AsCodeItem();
				if ( item.CategoryId != searchCategory )
				{
					continue;
				}

				sql = GetPropertySql( item.Id );
				if ( string.IsNullOrWhiteSpace( sql ) == false )
				{
					sql = sql.Replace( "[UserId]", userId.ToString() );
					customFilter = customFilter + OR + sql;
					OR = " OR ";
					//AND = " AND ";
				}
			}
			if ( sql.Length > 0 )
			{
				where = where + AND + "( " + customFilter + " ) ";

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
		public static void HandleApprovalFilters( MainSearchInput data, int searchCategory, int entityTypeId, ref string where, int userId = 0 )
		{
			string AND = "";
			//may want custom category for each one, to prevent requests that don't match the current search

			string sql = "";
			string view = "Credential_Updated_Approval_Publish_Summary";
			switch ( entityTypeId )
			{
				case 1:
					{
						view = "Credential_Updated_Approval_Publish_Summary";
						break;
					}
				case 2:
					{
						view = "Organization_Updated_Approval_Publish_Summary";
						break;
					}
				case 3:
					{
						view = "Assessment_Updated_Approval_Publish_Summary";
						break;
					}
				case 7:
					{
						view = "LearningOpportunity_Updated_Approval_Publish_Summary";
						break;
					}
				default:
					{
						return;
					}
			}

			//Updated to use FilterV2
			if ( where.Length > 0 )
			{
				AND = " AND ";
			}
			foreach ( var filter in data.FiltersV2.Where( m => m.Type == MainSearchFilterV2Types.CODE ).ToList() )
			{
				var item = filter.AsCodeItem();
				if ( item.CategoryId != searchCategory )
				{
					continue;
				}

				sql = GetPropertySql( item.Id );
				if ( string.IsNullOrWhiteSpace( sql ) == false )
				{
					sql = sql.Replace( "*EntityTypeId*", entityTypeId.ToString() );
					sql = sql.Replace( "Credential_Updated_Approval_Publish_Summary", view );
					//
					where = where + AND + sql;
					AND = " AND ";
				}
			}
			if ( sql.Length > 0 )
			{
				LoggingHelper.DoTrace( 6, "SearchServices.HandleCustomFilters. result: \r\n" + where );
			}
		}

		private static string GetPropertySql( int id )
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
		public static void SetDatesFilter( MainSearchInput data, int entityTypeId, ref string where, ref string messages )
		{
			//LastApprovalDate is now in the views
			string approvalsTemplate = "   (base.Id in (SELECT [EntityBaseId] FROM [dbo].[Entity_ApprovalSummary] where [IsActive]= 1 and [EntityTypeId] = {0} and (LastApprovalDate between '{1}' AND '{2}' )) )  ";
			string lastupdatedTemplate = " (base.EntityLastUpdated between '{0}' AND '{1}' ) ";
			//only want the last published date
			string publishedTemplate = "  ( base.IsPublished = 'yes' and [LastPublishDate] between '{0}' AND '{1}' )  ";
			//string publishedTemplate = "   ( Len(Isnull(base.CredentialRegistryId,'')) = 36 AND base.Id in (SELECT DISTINCT [ActivityObjectId] FROM [dbo].[ActivityLog] a inner join [Codes.EntityType] b on a.[ActivityType] = b.Title  where a.Activity = 'Credential Registry' and b.Id = {0} and ([CreatedDate] between '{1}' AND '{2}' ) order by created desc ) )  ";

			GetDates( data, entityTypeId, "approvedFrom", "approvedTo", approvalsTemplate, ref where );

			GetDates( data, 0, "lastUpdatedFrom", "lastUpdatedTo", lastupdatedTemplate, ref where );

			GetDates( data, 0, "publishedFrom", "publishedTo", publishedTemplate, ref where );
		}


		public static void GetDates( MainSearchInput data, int entityTypeId, string fromDateName, string toDateName, string template, ref string where )
		{
			string fromDate = "";
			string toDate = "";
			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";

			foreach ( var filter in data.FiltersV2.Where( m => m.Name == "approvals" ) )
			{
				if ( filter.Type == MainSearchFilterV2Types.DATE && filter.Name == "approvals" )
				{
					if ( filter.GetValueOrDefault( "PropertyName", "" ) == fromDateName )
						fromDate = filter.GetValueOrDefault( "TextValue", "" );
					if ( filter.GetValueOrDefault( "PropertyName", "" ) == toDateName )
						toDate = filter.GetValueOrDefault( "TextValue", "" );
					//
				}
				if ( !string.IsNullOrWhiteSpace( fromDate ) && !string.IsNullOrWhiteSpace( toDate ) )
					break;
			}

			if ( string.IsNullOrWhiteSpace( fromDate ) && string.IsNullOrWhiteSpace( toDate ) )
				return;
			DateTime searchFromDate = new DateTime( 2015, 1, 1 );
			DateTime searchToDate = DateTime.Now.AddDays( 1 );
			//have at least one date now. Ensure dates are valid
			if ( !string.IsNullOrWhiteSpace( fromDate ) )
				DateTime.TryParse( fromDate, out searchFromDate );
			if ( !string.IsNullOrWhiteSpace( toDate ) )
				DateTime.TryParse( toDate, out searchToDate );
			if ( searchToDate < searchFromDate )
				searchToDate = searchFromDate;

			//format to db friendly
			fromDate = searchFromDate.ToString( "yyyy-MM-dd" );
			toDate = searchToDate.ToString( "yyyy-MM-dd" );
			string filter2 = "";
			if ( entityTypeId > 0 )
				filter2 = string.Format( template, entityTypeId, fromDate, toDate );
			else
				filter2 = string.Format( template, fromDate, toDate );
			where = where + AND + filter2;

		}
		/// <summary>
		/// Filter FrameworkItem using passed code ids
		/// </summary>
		/// <param name="data"></param>
		/// <param name="tableName"></param>
		/// <param name="where"></param>
		public static void SetFrameworksFilter( MainSearchInput data, string tableName, ref string where )
		{
			string AND = "";
			string codeTemplate = "  (base.Id in (SELECT c.id FROM [dbo].[Entity.FrameworkItemSummary] a inner join Entity b on a.EntityId = b.Id inner join [{0}] c on b.EntityUid = c.RowId where [CategoryId] = {1} and ([FrameworkGroup] in ({2})  OR ([CodeId] in ({3}) )  ))  ) ";


			//Updated to use FiltersV2
			string next = "";
			string groups = "";
			if ( where.Length > 0 )
				AND = " AND ";

			var targetCategoryID = 0;
			foreach ( var filter in data.FiltersV2.Where( m => m.Type == MainSearchFilterV2Types.FRAMEWORK ) )
			{
				var item = filter.AsCodeItem();
				var isTopLevel = filter.GetValueOrDefault<bool>( "IsTopLevel", false );
				//if ( item.CategoryId == targetCategoryID )
				//{
				//	if ( isTopLevel )
				//		groups += item.Id + ",";
				//	else
				//		next += item.Id + ",";
				//}
				if ( item.CategoryId == 10 || item.Name == "industries" )
				{
					targetCategoryID = item.CategoryId;
					if ( isTopLevel )
						groups += item.Id + ",";
					else
						next += item.Id + ",";
				}
				else if ( item.CategoryId == 11 || item.Name == "occupations" )
				{
					targetCategoryID = item.CategoryId;
					if ( isTopLevel )
						groups += item.Id + ",";
					else
						next += item.Id + ",";
				}
				else if ( item.CategoryId == 23 || item.Name == "instructionalprogramtype" )
				{
					targetCategoryID = item.CategoryId;
					if ( isTopLevel )
						groups += item.Id + ",";
					else
						next += item.Id + ",";
				}
			} //

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
				where = where + AND + string.Format( codeTemplate, tableName, targetCategoryID, groups, next );
			}

		}
		/// <summary>
		/// Filter FrameworkItem using text
		/// </summary>
		/// <param name="data"></param>
		/// <param name="tableName"></param>
		/// <param name="where"></param>
		public static void SetFrameworkTextFilter( MainSearchInput data, string tableName, int categoryId, ref string where  )
		{
			string AND = "";
			string OR = "";

			string codeTemplate = "  (base.Id in (SELECT c.id FROM [dbo].[Entity.FrameworkItemSummary] a inner join Entity b on a.EntityId = b.Id inner join [{0}] c on b.EntityUid = c.RowId where [CategoryId] = {1} AND {2} ) ) ";

			string phraseTemplate = " (case when LTRIM(RTRIM(a.FrameworkCode)) = '' then LTRIM(RTRIM(a.Title)) else LTRIM(RTRIM(a.Title)) + ' (' + LTRIM(RTRIM(a.FrameworkCode)) + ')' end like '%{0}%') ";

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
				where = where + AND + " ( " + string.Format( codeTemplate, tableName,  categoryId , next) + ")";
			}
		}
		public static void SetSubjectsFilter( MainSearchInput data, int entityTypeId, ref string where )
		{
			string subjects = "  (base.RowId in (SELECT EntityUid FROM [Entity_Subjects] a where EntityTypeId = {0} AND {1} )) ";
			if ( data.SearchType == "credential" )
				subjects = subjects.Replace( "base.RowId", "base.EntityUid" );

			string frameworkItems = " OR (RowId in (SELECT EntityUid FROM [dbo].[Entity_Reference_Summary] a where CategoryId= 23 AND {0} ) ) ";

			string phraseTemplate = " (a.Subject like '{0}') ";
			string titleTemplate = " (a.TextValue like '{0}') ";

			string AND = "";
			string OR = "";

			//Updated to use FilterV2
			string next = "";
			string fnext = "";
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
				fnext += OR + string.Format( titleTemplate, SearchifyWord( text ) );
				OR = " OR ";
			}
			string fsubject = "";
			if ( !string.IsNullOrWhiteSpace( fnext )
				&& ( entityTypeId == 3 || entityTypeId == 7 ) )
			{
				fsubject = string.Format( frameworkItems, fnext );
			}
			if ( !string.IsNullOrWhiteSpace( next ) )
			{
				where = where + AND + " ( " + string.Format( subjects, entityTypeId, next ) + fsubject + ")";
			}


		}
		/// <summary>
		/// May want to make configurable, in case don't want to always perform check.
		/// </summary>
		/// <param name="word"></param>
		/// <returns></returns>
		public static string SearchifyWord( string word )
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

		public static void SetLanguageFilter( MainSearchInput data, int entityTypeId, ref string where )
		{
			string AND = "";

			string template = " ( base.Id in ( SELECT b.[EntityBaseId] FROM [dbo].[Entity.Language] l inner join Entity b on l.EntityId = b.Id where b.EntityTypeId = {0} AND l.[LanguageCodeId] in ({1}))) ";

			//Updated to use FiltersV2
			string next = "";
			if ( where.Length > 0 )
				AND = " AND ";
			foreach ( var filter in data.FiltersV2.Where( m => m.Type == MainSearchFilterV2Types.CODE ).ToList() )
			{
				var item = filter.AsCodeItem();
				if ( item.CategoryId == 65 )
					next += item.Id + ",";
			}
			next = next.Trim( ',' );
			if ( !string.IsNullOrWhiteSpace( next ) )
			{
				where = where + AND + string.Format( template, entityTypeId, next );
			}
		}
		public static void SetPropertiesFilter( MainSearchInput data, int entityTypeId, string searchCategories, ref string where )
		{
			string AND = "";
			//string searchCategories = UtilityManager.GetAppKeyValue( "orgSearchCategories", "7,8,9,30," );
			string template = " ( base.Id in ( SELECT  [EntityBaseId] FROM [dbo].[EntityProperty_Summary] where EntityTypeId= {0} AND [PropertyValueId] in ({1}))) ";
			int prevCategoryId = 0;

			//Updated to use FiltersV2
			string next = "";
			if ( where.Length > 0 )
				AND = " AND ";
			foreach ( var filter in data.FiltersV2.Where( m => m.Type == MainSearchFilterV2Types.CODE ).ToList() )
			{
				var item = filter.AsCodeItem();
				if ( searchCategories.Contains( item.CategoryId.ToString() ) )
				{
					if ( item.CategoryId != prevCategoryId )
					{
						if ( prevCategoryId > 0 )
						{
							next = next.Trim( ',' );
							where = where + AND + string.Format( template, entityTypeId, next );
							AND = " AND ";
						}
						prevCategoryId = item.CategoryId;
						next = "";
					}
					next += item.Id + ",";
				}
			}
			next = next.Trim( ',' );
			if ( !string.IsNullOrWhiteSpace( next ) )
			{
				where = where + AND + string.Format( template, entityTypeId, next );
			}

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
			string template = " ( base.RowId in ( SELECT distinct b.EntityUid FROM [dbo].[Entity.AgentRelationship] a inner join Entity b on a.EntityId = b.Id INNER JOIN Organization o ON a.AgentUid = o.RowId  where a.[RelationshipTypeId] in ({0}) AND o.StatusId < 3 ) ) ";
			//string template = " ( base.Id in ( SELECT CredentialId FROM [dbo].[CredentialAgentRelationships_Summary] where [RelationshipTypeId] IN ({0}) ) ) ";

			if ( data.SearchType == "credential" )
				template = template.Replace( "base.RowId", "base.EntityUid" );

			//Updated to use FiltersV2
			string next = "";
			if ( where.Length > 0 )
				AND = " AND ";
			foreach ( var filter in data.FiltersV2.Where( m => m.Type == MainSearchFilterV2Types.CODE && m.Name != "qualityassuranceperformed" ) )
			{
				var item = filter.AsCodeItem();
				if ( item.CategoryId == 13 )
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


		public static void SetQAbyOrgFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";
			string template = " ( base.RowId in ( SELECT distinct EntityUid FROM [dbo].[Entity.AgentRelationship] a inner join organization o ON a.AgentUid = o.rowId inner join Entity b on a.EntityId = b.Id   where a.[RelationshipTypeId] = {0} AND o.Id = {1}) ) ";

			int roleId = 0;
			int orgId = 0;

			foreach ( var filter in data.FiltersV2.Where( m => m.Name == "qualityAssuranceBy" ).ToList() )
			{
				roleId = filter.GetValueOrDefault( "RoleId", 0 );
				orgId = filter.GetValueOrDefault( "AgentId", 0 );
				where = where + AND + string.Format( template, roleId, orgId );
				AND = " AND ";
			}
		}

		public static void SetQAPerformedbyOrgFilter( MainSearchInput data, ref string where )
		{
			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";
			string template = " ( base.RowId in ( SELECT distinct b.EntityUid FROM [dbo].[Entity.Assertion] a inner join Entity b on a.EntityId = b.Id where a.[AssertionTypeId] in ({0})  ) ) ";

			string next = "";
			if ( where.Length > 0 )
				AND = " AND ";
			foreach ( var filter in data.FiltersV2.Where( m => m.Type == MainSearchFilterV2Types.CODE && m.Name == "qualityassuranceperformed" ) )
			{
				var item = filter.AsCodeItem();
				if ( item.CategoryId == 13 )
				{
					next += item.Id + ",";
				}
			}
			next = next.Trim( ',' );
			if ( !string.IsNullOrWhiteSpace( next ) )
			{
				where = where + AND + string.Format( template, next );
			}

		}

		public static void SetAuthorizationFilter( AppUser user, string summaryView, ref string where, bool isMicroSearch = false )
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
			//NOTE - the summary views only allows status of 1, or 2, so don't need in filters
			if ( isMicroSearch )
			{
				bool limitOrgMicroSearchResultsToRelatedOrReferences = UtilityManager.GetAppKeyValue( "limitOrgMicroSearchResultsToRelatedOrReferences", false );
				//include published references (which are set to published by default)
				if ( limitOrgMicroSearchResultsToRelatedOrReferences == true )
					where = where + AND + string.Format( "( ( Isnull(base.CTID,'') = '') OR (base.Id in (SELECT cs.Id FROM [dbo].[Organization.Member] om inner join [{0}] cs on om.ParentOrgId = cs.ManagingOrgId where userId = {1}) ))", summaryView, user.Id );

				else
				{
					//18-02-26 - allow entities that are published, references, and related
					where = where + AND + string.Format( " ( ( ( Isnull(base.CTID,'') = '' OR len(base.CredentialRegistryId) = 36 ) ) OR (base.Id in (SELECT cs.Id FROM [dbo].[Organization.Member] om inner join [{0}] cs on om.ParentOrgId = cs.ManagingOrgId where userId = {1}) ))", summaryView, user.Id );
				}

				return;
			}

			//can only view where status is published (should only be where a reference??, or associated with 
			//Hmm: (base.StatusId = {0} and base.CTID <> '') could result 
			where = where + AND + string.Format( " (base.Id in (SELECT cs.Id FROM [dbo].[Organization.Member] om inner join [{0}] cs on om.ParentOrgId = cs.ManagingOrgId where userId = {1}) )", summaryView, user.Id );

		}
		#endregion
	}
}
