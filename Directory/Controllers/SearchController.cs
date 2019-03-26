using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Models;
using CTIServices;
using Models.Search;
using Utilities;
using Models.Helpers;

namespace CTI.Directory.Controllers
{
    public class SearchController : Controller
    {
        SearchServices searchService = new SearchServices();
        bool valid = true;
        string status = "";
        AppUser user = new AppUser();


        //Main Search Page
        public ActionResult Index()
        {
            return View( "~/Views/V2/SearchV4/Search.cshtml" );
        }
        //

        [Obsolete]
        public ActionResult V2()
        {
            return Index();
        }

        //Do a search
        public JsonResult MainSearch( MainSearchInput query )
        {
            DateTime start = DateTime.Now;
            LoggingHelper.DoTrace( 6, "$$$$SearchController.MainSearch === Started for: " + query.SearchType );
            var results = searchService.MainSearch( query, ref valid, ref status );

            TimeSpan timeDifference = DateTime.Now.Subtract( start );
            LoggingHelper.DoTrace( 6, string.Format( "$$$$SearchController.MainSearch === Ended - Elapsed: {0}", timeDifference.TotalSeconds ) );

            return JsonHelper.GetJsonWithWrapper( results, valid, status, null );
        }
        /// <summary>
        /// prepare export
        /// - get filters, extract ctids, call 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public JsonResult Export( MainSearchInput query )
        {
            //DateTime start = DateTime.Now;
            //LoggingHelper.DoTrace( 6, string.Format( "$$$$SearchController.MainSearch === Started: " ) );
            var results = searchService.MainSearch( query, ref valid, ref status );

            //TimeSpan timeDifference = start.Subtract( DateTime.Now );
            //LoggingHelper.DoTrace( 6, string.Format( "$$$$SearchController.MainSearch === Ended - Elapsed: {0}", timeDifference.TotalSeconds ) );

            return JsonHelper.GetJsonWithWrapper( results, valid, status, null );
        }
        //Do a MicroSearch
        public JsonResult DoMicroSearch( MicroSearchInputV2 query )
        {
            var totalResults = 0;
            var data = MicroSearchServicesV2.DoMicroSearch( query, ref totalResults, ref valid, ref status );

            return JsonHelper.GetJsonWithWrapper( data, valid, status, totalResults );
        }
        //

        //Find Locations
        public JsonResult FindLocations( string text )
        {
            var total = 0;
            var data = new ThirdPartyApiServices().GeoNamesSearch( text, 1, 5, null, ref total, true );

            return JsonHelper.GetJsonWithWrapper( data, valid, status, total );
        }
        //

        //Do AutoComplete
        public JsonResult DoAutoComplete( string text, string context, string searchType )
        {
            var data = SearchServices.DoAutoComplete( text, context, searchType );

            return JsonHelper.GetJsonWithWrapper( data, true, "", null );
        }

        //Get Tag Item data
        public JsonResult GetTagItemData( string searchType, string entityType, int recordID, int maxRecords = 10 )
        {
            try
            {
                var data = SearchServices.GetTagSet( searchType, ( SearchServices.TagTypes ) Enum.Parse( typeof( SearchServices.TagTypes ), entityType, true ), recordID, maxRecords );
                return JsonHelper.GetJsonWithWrapper( data, true, "", null );
            }
            catch
            {
                return JsonHelper.GetJsonWithWrapper( null, false, "Invalid entity type", null );
            }
        }

        //Get Tag Item Data (Used with V2 tag items)
        public JsonResult GetTagsV2Data( string AjaxQueryName, int RecordId, string SearchType, string TargetEntityType, int MaxRecords = 10 )
        {
            try
            {
                var tagType = ( SearchServices.TagTypes ) Enum.Parse( typeof( SearchServices.TagTypes ), TargetEntityType, true );
                var data = SearchServices.GetTagSet( SearchType, tagType, RecordId, MaxRecords );
                var items = new List<SearchTagItem>();

                switch ( AjaxQueryName )
                {
                    case "GetSearchResultCompetencies":
                        {
                            items = data.Items.ConvertAll( m => new SearchTagItem()
                            {
                                Display = string.IsNullOrWhiteSpace( m.Description ) ?
                                m.Label :
                                "<b>" + m.Label + "</b>" + System.Environment.NewLine + m.Description,
                                QueryValues = new Dictionary<string, object>()
                                {
                                    { "SchemaName", m.Schema },
                                    { "CodeId", m.CodeId },
                                    { "TextValue", m.Label },
                                    { "TextDescription", m.Description }
                                }
                            } );
                            break;
                        }
                    case "GetSearchResultCosts":
                        {
                            items = data.CostItems.ConvertAll( m => new SearchTagItem()
                            {
                                Display = m.CostType + ": " + m.CurrencySymbol + m.Price + " ( " + ( m.SourceEntity ?? "direct" ) + " )",
                                QueryValues = new Dictionary<string, object>()
                                {
                                    { "CurrencySymbol", m.CurrencySymbol },
                                    { "Price", m.Price },
                                    { "CostType", m.CostType }
                                }
                                //Something that probably looks like that -^
                            } );
                            break;
                        }
				   case "GetSearchResultsCIPs":
						{
							items = data.Items.ConvertAll( m => new SearchTagItem()
							{
								Display = m.Label,
								QueryValues = new Dictionary<string, object>()
								{
									{ "TextValue", m.Label }
								}
							} );
							break;
						}
                    case "GetSearchResultPerformed":
                        {
                            items = data.QAItems.ConvertAll( m => new SearchTagItem()
                            {
                                Display = "<b>" + m.AgentToTargetRelationship + "</b> <b>" + m.TargetEntityType + "</b> " + m.TargetEntityName,
                                QueryValues = new Dictionary<string, object>()
                                {
                                    { "SearchType", m.TargetEntityType.ToLower() },
                                    { "RecordId", m.TargetEntityBaseId },
                                    { "TargetEntityType", m.TargetEntityType },
                                    { "IsReference", m.IsReference },
                                    {"SubjectWebpage", m.TargetEntitySubjectWebpage }
                                }
                            } );
                        }
					
						break;

                    default: break;
                }
                return JsonHelper.GetJsonWithWrapper( items, true, "", null );
            }
            catch ( Exception ex )
            {
                return JsonHelper.GetJsonWithWrapper( null, false, ex.Message, null );
            }
        }

        /// <summary>
        /// Get a summary of QA role for this entity
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="entityId"></param>
        /// <param name="maxRecords"></param>
        /// <returns></returns>
        public JsonResult GetEntityRoles( string searchType, int entityId, int maxRecords = 10 )
        {
            var data = SearchServices.EntityQARolesList( searchType, entityId, maxRecords );

            return JsonHelper.GetJsonWithWrapper( data, true, "", null );
        }
        //
        #region delete methods
        [AcceptVerbs( HttpVerbs.Post )]
        public JsonResult DeleteCredential( int id )
        {
            if ( AccountServices.AuthorizationCheck( "delete", true, ref status, ref user ) == false )
            {
                return JsonHelper.GetJsonWithWrapper( null, false, status, null );
            }
            var valid = true;


            if ( id > 0 )
            {
                valid = new CredentialServices().Delete( id, user, ref status );
                if ( !valid )
                {
                    return JsonHelper.GetJsonWithWrapper( null, false, "Error deleting credential: " + status, null );
                }
            }
            else
            {
                return JsonHelper.GetJsonWithWrapper( null, false, "You must select a credential to delete.", null );
            }

            return JsonHelper.GetJsonWithWrapper( null, true, "Deleted successfully", null );
        }
        //
        [AcceptVerbs( HttpVerbs.Post )]
        public JsonResult DeleteOrganization( int id )
        {
            if ( AccountServices.AuthorizationCheck( "delete", true, ref status, ref user ) == false )
            {
                return JsonHelper.GetJsonWithWrapper( null, false, status, null );
            }

            var valid = true;
            if ( id > 0 )
            {
                //Organization_Delete will check authorization
                valid = new OrganizationServices().Organization_Delete( id, user, ref status );
                if ( !valid )
                {
                    return JsonHelper.GetJsonWithWrapper( null, false, "Error deleting organization: " + status, null );
                }
            }
            else
            {
                return JsonHelper.GetJsonWithWrapper( null, false, "You must select an organization to delete.", null );
            }

            return JsonHelper.GetJsonWithWrapper( null, true, "Deleted successfully", null );
        }
        //
        [AcceptVerbs( HttpVerbs.Post )]
        public JsonResult DeleteAssessment( int id )
        {
            if ( AccountServices.AuthorizationCheck( "delete", true, ref status, ref user ) == false )
            {
                return JsonHelper.GetJsonWithWrapper( null, false, status, null );
            }

            var valid = true;
            if ( id > 0 )
            {
                valid = new AssessmentServices().Delete( id, user, ref status );
                if ( !valid )
                {
                    return JsonHelper.GetJsonWithWrapper( null, false, "Error deleting assessment: " + status, null );
                }
            }
            else
            {
                return JsonHelper.GetJsonWithWrapper( null, false, "You must select an assessment to delete.", null );
            }

            return JsonHelper.GetJsonWithWrapper( null, true, "Deleted successfully", null );
        }
        //

        //
        [AcceptVerbs( HttpVerbs.Post )]
        public JsonResult DeleteLearningOpportunity( int id )
        {
            if ( AccountServices.AuthorizationCheck( "delete", true, ref status, ref user ) == false )
            {
                return JsonHelper.GetJsonWithWrapper( null, false, status, null );
            }

            var valid = true;
            if ( id > 0 )
            {
                new LearningOpportunityServices().Delete( id, user, ref valid, ref status );
                if ( !valid )
                {
                    return JsonHelper.GetJsonWithWrapper( null, false, "Error deleting Learning Opportunity: " + status, null );
                }
            }
            else
            {
                return JsonHelper.GetJsonWithWrapper( null, false, "You must select a Learning Opportunity to delete.", null );
            }

            return JsonHelper.GetJsonWithWrapper( null, true, "Deleted successfully", null );
        }

        #endregion
        //

    }
}