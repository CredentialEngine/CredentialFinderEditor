using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CTIServices;
using Models;
using Models.Common;
using MC = Models.Common;
using Models.ProfileModels;
using Models.Helpers.Review;
using Models.Node.Interface;
using Models.Node;
using Utilities;
using System.Net.Http;

namespace CTI.Directory.Controllers
{
    public class ReviewController : BaseController
    {
		static AppUser currentUser = new AppUser();
		string status = "";
		List<string> messages = new List<string>();
		bool valid = true;
		bool approved = false;
        DateTime? lastPublishDate = null;
		DateTime lastUpdatedDate = DateTime.MinValue;
		DateTime lastApprovedDate = DateTime.MinValue;
        bool isPublished = false;
        string ctid = "";

		//Ensure user is logged in, and set the currentUser to the logged in user - otherwise, kick the user back out to the home page with a message.
		public class LoggedInFilterAttribute : ActionFilterAttribute
		{
			public override void OnActionExecuting( ActionExecutingContext filterContext )
			{
				if ( !filterContext.HttpContext.User.Identity.IsAuthenticated )
				{
					filterContext.HttpContext.Session[ "SystemMessage" ] = "You must be logged in to view that item.";
					filterContext.Result = new RedirectResult( "~/" );
				}
				else
				{
					currentUser = AccountServices.GetCurrentUser( filterContext.HttpContext.User.Identity.Name );
					base.OnActionExecuting( filterContext );
				}
			}
		}

		//Ensure the user is logged in - otherwise reject
		public class LoggedInJsonFilterAttribute : ActionFilterAttribute
		{
			public override void OnActionExecuting( ActionExecutingContext filterContext )
			{
				if ( !filterContext.HttpContext.User.Identity.IsAuthenticated )
				{
					filterContext.Result = JsonHelper.GetJsonWithWrapper( null, false, "You must be logged in to access this data.", null );
				}
				else
				{
					currentUser = AccountServices.GetCurrentUser( filterContext.HttpContext.User.Identity.Name );
					base.OnActionExecuting( filterContext );
				}
			}
		}
		//

		//Load the previewer
		public ActionResult Index( List<ReviewWrapper> items )
		{
			return View( "~/Views/V2/DetailV4/ReviewV2.cshtml", items );
		}
		public ActionResult Index( string type, string typeTitle, int id )
		{
			var vm = new List<ReviewWrapper>();
			vm.Add( new ReviewWrapper()
			{
				EntityId = id,
				EntityType = type.ToLower(),
				EntityTypeTitle = typeTitle,
			} );
			return Index( vm );
		}
		public ActionResult Organization( int id )
		{
			return Index( "organization", "Organization", id );
		}
        public ActionResult QAOrganization( int id )
        {
            return Index( "organization", "Organization", id );
        }
        public ActionResult Credential( int id )
		{
			return Index( "credential", "Credential", id );
		}
		public ActionResult Assessment( int id )
		{
			return Index( "assessment", "Assessment", id );
		}
		public ActionResult LearningOpportunity( int id )
		{
			return Index( "learningopportunity", "Learning Opportunity", id );
		}
		public ActionResult AssessmentProfile(int id )
		{
			return Assessment( id );
		}
		public ActionResult LearningOpportunityProfile(int id )
		{
			return LearningOpportunity( id );
		}
		//

		public ActionResult CTID( string id )
		{
			EntitySummary entity = ProfileServices.GetEntityByCtid( ctid );
			if ( entity != null && entity.Id > 0)
			{
				if (entity.EntityTypeId == 1)
					return Index( "credential", "Credential", entity.BaseId );
				else if ( entity.EntityTypeId == 2 )
					return Index( "organization", "Organization", entity.BaseId );
				else if ( entity.EntityTypeId == 3 )
					return Index( "assessment", "Assessment", entity.BaseId );
				else if ( entity.EntityTypeId == 7 )
					return Index( "learningopportunity", "Learning Opportunity", entity.BaseId );
				else if ( entity.EntityTypeId == 19 )
					return Index( "conditionmanifest", "Condition Manifest", entity.BaseId );
				else if ( entity.EntityTypeId == 20 )
					return Index( "costmanifest", "Cost Manifest", entity.BaseId );

			}

			//Otherwise
			SetSystemMessage( "Error", "Error identifying review target.", "error" );
			return RedirectToAction( "Index", "Home" );
		}
		//

		//Make AJAX requests on behalf of the controller
		public string GetDataViaHTTP( string url )
		{
			return new HttpClient().GetAsync( url ).Result.Content.ReadAsStringAsync().Result;
		}
		//
		
		//Get summary data by CTID without knowing the type of object
		public JsonResult GetJSON_SummaryByCTID( string ctid )
		{
			EntitySummary entity = ProfileServices.GetEntityByCtid( ctid );
			if ( entity != null && entity.Id > 0 )
			{
				var reference = new Dictionary<string, object>()
				{
					{ "ceterms:name", entity.Name },
					{ "ceterms:description", entity.Description },
					{ "ceterms:ctid", entity.CTID },
					{ "credentialeditor:BaseId", entity.BaseId }
				};
				return JsonResponse( reference );
			}
			return JsonResponse( null, false, "Error determining data type", null );
		}

		//Get data by CTID without knowing the type of object
		public JsonResult GetJSON_ByCTID( string ctid )
		{
			EntitySummary entity = ProfileServices.GetEntityByCtid( ctid );
			if ( entity != null && entity.Id > 0 )
			{
				switch( entity.EntityTypeId )
				{
					case 1:
						return GetJSON_Credential( entity.BaseId );
					case 2:
						return GetJSON_Organization( entity.BaseId );
					case 3:
						return GetJSON_Assessment( entity.BaseId );
					case 7:
						return GetJSON_LearningOpportunity( entity.BaseId );
					case 19:
						return GetJSON_ConditionManifest( entity.BaseId );
					case 20:
						return GetJSON_CostManifest( entity.BaseId );
				}
			}
			return JsonResponse( null, false, "Error determining data type", null );
		}
		//

		//Organization
		[LoggedInJsonFilter]
		public JsonResult GetJSON_Organization( int id )
		{
			var json = OrganizationServices.GetForFormat( id, currentUser, ref valid, ref messages, ref approved, ref lastPublishDate, ref lastUpdatedDate, ref lastApprovedDate, ref ctid );
			return FormatGetJSONResponse( json );
		}
		//

		//Credential
		[LoggedInJsonFilter]
		public JsonResult GetJSON_Credential( int id )
		{
            
            var json = CredentialServices.GetForReview( id, currentUser, ref valid, ref messages, ref approved, ref lastPublishDate, ref lastUpdatedDate, ref lastApprovedDate, ref ctid );
			return FormatGetJSONResponse( json );
		}
		//

		//Assessment
		[LoggedInJsonFilter]
		public JsonResult GetJSON_Assessment( int id )
		{
			var json = AssessmentServices.GetForFormat( id, currentUser, ref valid, ref messages, ref approved, ref lastPublishDate, ref lastUpdatedDate, ref lastApprovedDate, ref ctid );
			return FormatGetJSONResponse( json );
		}
		//

		//Learning Opportunity
		[LoggedInJsonFilter]
		public JsonResult GetJSON_LearningOpportunity( int id )
		{
			var json = LearningOpportunityServices.GetForFormat( id, currentUser, ref valid, ref messages, ref approved, ref lastPublishDate, ref lastUpdatedDate, ref lastApprovedDate, ref ctid );
			return FormatGetJSONResponse( json );
		}
		//

		//Condition Manifest
		[LoggedInJsonFilter]
		public JsonResult GetJSON_ConditionManifest( int id )
		{
			var json = ConditionManifestServices.GetForFormat( id, currentUser, ref valid, ref messages, ref approved, ref lastPublishDate, ref lastUpdatedDate, ref lastApprovedDate, ref ctid );
			return FormatGetJSONResponse( json );
		}
		//

		//Cost Manifest
		[LoggedInJsonFilter]
		public JsonResult GetJSON_CostManifest( int id )
		{
			
			var json = CostManifestServices.GetForFormat( id, currentUser, ref valid, ref messages, ref approved, ref lastPublishDate, ref lastUpdatedDate, ref lastApprovedDate, ref ctid );
			return FormatGetJSONResponse( json );
		}
		//

		//Approve an entity
		[LoggedInJsonFilter]
		public JsonResult ApproveEntity( string type, int id )
		{
			//Check permission
			if ( AccountServices.AuthorizationCheck( "publish", true, ref status, ref currentUser ) == false )
			{
				return JsonResponse( null, false, status, null );
			}

			var context = GetProfileContext( type, id );
			EditorServices.ApproveEntity( context, ref valid, ref isPublished, ref status, true );

			//Return the result
			return JsonResponse( null, valid, status, new { IsApproved = valid, IsPublished = isPublished, StatusMessages = new List<string>() } );
		}
		//

		//Unapprove an entity
		[LoggedInJsonFilter]
		public JsonResult UnapproveEntity( string type, int id )
		{
			//Check permission
			if ( AccountServices.AuthorizationCheck( "publish", true, ref status, ref currentUser ) == false )
			{
				return JsonResponse( null, false, status, null );
			}

			//publish to the registry
			var context = GetProfileContext( type, id );
			EditorServices.UnApproveEntity( context, ref valid, ref status );

			//Return the result
			return JsonResponse( null, valid, status, new { IsApproved = false, IsPublished = false, StatusMessages = new List<string>() } );
		}
		//

		//Publish an entity
		[LoggedInJsonFilter]
		public JsonResult PublishEntity( string type, int id )
		{
			//Check permission
			if ( AccountServices.AuthorizationCheck( "publish", true, ref status, ref currentUser ) == false )
			{
				return JsonResponse( null, false, status, null );
			}

			//Must be site staff
			if ( !AccountServices.IsUserSiteStaff() )
			{
				return JsonResponse( null, false, "You are not allowed to do that.", null );
			}

			//publish to the registry
			var context = GetProfileContext( type, id );
			EditorServices.RegisterEntity( context, ref valid, ref status );

			//Return the result
			return JsonResponse( null, valid, status, new { IsApproved = true, IsPublished = true, StatusMessages = new List<string>() } );
		}
		//

		//Build profile context for use with editor methods
		private ProfileContext GetProfileContext( string type, int id )
		{
			var context = new ProfileContext()
			{
				Profile = new ProfileLink()
				{
					Id = id,
					RowId = Guid.Empty
				}
			};

			switch ( type )
			{
				case "credential":
					context.Profile.Type = typeof( MC.Credential );
					break;
				case "organization":
					context.Profile.Type = typeof( MC.Organization );
					break;
				case "assessment":
					context.Profile.Type = typeof( Assessment );
					break;
				case "learningopportunity":
					context.Profile.Type = typeof( LearningOpportunity );
					break;
				default:
					break;
			}

			return context;
		}
		//

		private JsonResult FormatGetJSONResponse( string json )
		{
			if ( string.IsNullOrWhiteSpace( json ) )
			{
				//TODO: improve this
				return JsonResponse( null, false, "There was some invalid data with this entity. Formatting failed." );
			}
			isPublished = lastPublishDate != null;
			/*if ( IsAGraphResource( json ) )
			{
				Dictionary<string, object> dictionary = new JsonLDServices().JsonToDictionary( json );
				object graph = dictionary[ "@graph" ];
				//serialize the graph object
				var glist = JsonConvert.SerializeObject( graph );

				//parse graph in to list of objects, for now just take the first one
				JArray graphList = JArray.Parse( glist );
				int cntr = 0;
				foreach ( var item in graphList )
				{
					cntr++;
					if ( cntr == 1 )
					{
						json = item.ToString();
						break;
					}
				}
			}*/
			return JsonResponse( json, true, "", new { IsApproved = approved, IsPublished = isPublished, CTID = ctid, NeedsReapproval = approved && lastUpdatedDate > lastApprovedDate } );
		}
		//

		public static bool IsAGraphResource( string payload )
		{
			if ( string.IsNullOrWhiteSpace( payload ) )
				return false;

			//may need additional checks?
			if ( payload.IndexOf( "@graph" ) > 0 )
				return true;
			//else if ( payload.IndexOf( "\"en\":" ) > 0 ) //actually this should be an error condition
			//    return true;
			else
				return false;
		}
	}
}