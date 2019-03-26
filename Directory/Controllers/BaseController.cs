using System.Web.Mvc;

using Newtonsoft.Json;
using System.Text;

using CTIServices;
using Utilities;
using Models;

namespace CTI.Directory.Controllers
{
	public class BaseController : Controller
    {

		protected bool AuthorizationCheck( string type, string action, int profileID )
		{
			//Initial data
			var canEdit = true;
			var name = "";
			string status = "";
			AppUser user = new AppUser();

			//Check to see if the user has any edit permissions
			if ( AccountServices.AuthorizationCheck( action, true, ref status, ref user ) == false )
			{
				ConsoleMessageHelper.SetConsoleInfoMessage( "ERROR - NOT AUTHENTICATED. You will not be able to add or update content", "", false );
				Session[ "siteMessage" ] = "ERROR - NOT AUTHENTICATED. You will not be able to add or update content";
				canEdit = false;
			}

			//If the target is a new item, just return true/false depending on whether or not user can edit content
			if ( profileID == 0 )
			{
				return canEdit;
			}

			//Otherwise, if the item exists, check to see if the user can edit it specifically
			switch ( type )
			{
				case "credential":
					canEdit = CredentialServices.CanUserUpdateCredential( profileID, user, ref status );
					name = "credential";
					break;
				case "organization":
					canEdit = OrganizationServices.CanUserUpdateOrganization( user, profileID );
					name = "organization";
					break;
				case "assessment":
					canEdit = AssessmentServices.CanUserUpdateAssessment( profileID, user, ref status );
					name = "assessment";
					break;
				case "learningopportunity":
					canEdit = LearningOpportunityServices.CanUserUpdateLearningOpportunity( profileID, user, ref status );
					//AccountServices.EditCheck( ref canEdit, ref status );
					name = "learning opportunity";
					break;
				default:
					break;
			}

			//If the user can't edit, set a standardized message
			if ( !canEdit )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - you do not have authorization to edit this " + name, "", false );
				Session[ "siteMessage" ] = "ERROR - you do not have authorization to edit this " + name;
			}

			//Return the result of all the checks
			return canEdit;
		}


        // GET: Base
		//public ActionResult Index()
		//{
		//	return View();
		//}

		protected void SetSystemMessage( string title, string message, string messageType = "success" )
		{
			SiteMessage msg = new SiteMessage() { Title = title, Message = message, MessageType = messageType };
			Session[ "SystemMessage" ] = msg;
		}

		protected void SetPopupMessage( string message, string messageType = "info" )
		{
			SiteMessage msg = new SiteMessage() { Message = message, MessageType = messageType };
			Session[ "popupMessage" ] = msg;
		}
		protected void SetPopupSuccessMessage( string message )
		{
			SiteMessage msg = new SiteMessage() { Message = message, MessageType = "success" };
			Session[ "popupMessage" ] = msg;
		}
		protected void SetPopupErrorMessage( string message )
		{
			SiteMessage msg = new SiteMessage() { Message = message, MessageType = "error" };
			Session[ "popupMessage" ] = msg;
		}
        public bool DoesViewExist( string path )
        {
            return System.IO.File.Exists( Server.MapPath( path ) );
        }

        public ActionResult ViewPage( string path, string redirectFallback )
        {
            if ( DoesViewExist( path ) )
            {
                return View( path );
            }
            else
            {
                return RedirectToAction( redirectFallback );
            }
        }
        //Common method for JSON
        public JsonResult JsonResponse( object data, bool valid = true, string status = "", object extra = null )
		{
			return JsonHelper.GetJsonWithWrapper( data, valid, status, extra );
		}
		//

		public ActionResult BigJsonResponse( object data, bool valid = true, string status = "", object extra = null )
		{
			var json = JsonConvert.SerializeObject( new
			{
				data = data,
				valid = valid,
				status = status,
				extra = extra
			} );
			return new ContentResult() { Content = json, ContentEncoding = Encoding.UTF8, ContentType = "application/json" };
		}
		//
	}
}