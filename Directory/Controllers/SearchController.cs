using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Models;
using CTIServices;
using Models.Search;
using Utilities;

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
			return V2();
	  }
	  //

		public ActionResult V2()
		{
			return View( "~/Views/V2/Search/Index.cshtml" );
		}

	  //Do a search
	  public JsonResult MainSearch( MainSearchInput query )
	  {
		  var results = searchService.MainSearch( query, ref valid, ref status );

		  return JsonHelper.GetJsonWithWrapper( results, valid, status, null );
	  }
	  //

		//Do a MicroSearch
		public JsonResult DoMicroSearch( Models.Search.MicroSearchInputV2 query )
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

	  [AcceptVerbs( HttpVerbs.Post )]
	  public JsonResult DeleteCredential( int id )
	  {
		  if ( AccountServices.AuthorizationCheck( "delete", true, ref status ) == false )
		  {
			  return JsonHelper.GetJsonWithWrapper( null, false, status, null );
		  }
		  var valid = true;


		  if ( id > 0 )
		  {
			  valid = new CredentialServices().Credential_Delete( id, user, ref status );
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
		  if ( AccountServices.AuthorizationCheck( "delete", true, ref status ) == false )
		  {
			  return JsonHelper.GetJsonWithWrapper( null, false, status, null );
		  }

		  var valid = true;
		  if ( id > 0 )
		  {
			  new AssessmentServices().Delete( id, user.Id, ref valid, ref status );
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

	  //
	  [AcceptVerbs( HttpVerbs.Post )]
	  public JsonResult DeleteLearningOpportunity( int id )
	  {
		  if ( AccountServices.AuthorizationCheck( "delete", true, ref status ) == false )
		  {
			  return JsonHelper.GetJsonWithWrapper( null, false, status, null );
		  }

		  var valid = true;
		  if ( id > 0 )
		  {
			  new LearningOpportunityServices().Delete( id, user.Id, ref valid, ref status );
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

	}
}