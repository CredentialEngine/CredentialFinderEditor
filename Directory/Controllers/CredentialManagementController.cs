using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

using CTIServices;
using Models;
using Models.Common;
using Utilities;

/* Nothing in here should be used anymore - but the methods might be useful for references later */
namespace CTI.Directory.Controllers
{
  public class CredentialManagementController : Controller
  {

		AppUser user = new AppUser();
		string status = "";
		List<string> messages = new List<string>();
		//
		// GET: /CredentialManagement/
		public ActionResult Index( string keyword = "" )
		{
			AuthorizationCheck( "", false, ref status );

			var vm = CredentialServices.QuickSearch( keyword );
			return View( vm );
		}
		public JsonResult CredentialList( string keyword, int maxTerms = 25 )
		{

			var result = CredentialServices.Credential_Autocomplete( keyword, maxTerms );

			return Json( result, JsonRequestBehavior.AllowGet );
		}
	
		/// <summary>
		/// View the details for a Credential
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[AcceptVerbs( HttpVerbs.Get )]
		public ActionResult Detail( int id = 0 )
		{
			if ( id == 0 )
			{
				RedirectToAction( "Index" );
			}

			AuthorizationCheck( "detail", false, ref status );

			var vm = CredentialServices.GetCredentialDetail( id, user );
			if ( id > 0 && vm.Id == 0 )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested credential was not found", "", false );
			}

			ViewBag.IsAdmin = false;
			if ( AccountServices.IsUserAnAdmin( user ) )
			{
				ViewBag.IsAdmin = true;
			}
			return View( "Detail", vm );
		}
		//

		[AcceptVerbs( HttpVerbs.Get )]
		public ActionResult Detail2( int id = 0 )
		{
			if ( id == 0 )
			{
				RedirectToAction( "Index" );
			}

			AuthorizationCheck( "detail", false, ref status );

			var vm = CredentialServices.GetCredentialDetail( id, user );
			if ( id > 0 && vm.Id == 0 )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested credential was not found", "", false );
			}

			ViewBag.IsAdmin = false;
			if ( AccountServices.IsUserAnAdmin( user ) )
			{
				ViewBag.IsAdmin = true;
			}
			return View( "Detail2", vm );
		}
		//

		/// <summary>
		/// Load the page to create or edit a credential
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[AcceptVerbs( HttpVerbs.Get )]
		public ActionResult Edit( int id = 0, string section = "basic" )
		{
			ViewBag.ActiveSection = section;
			ViewBag.Message = "";
			ViewBag.GetOrgs = true;
			ViewBag.GetQAOrgs = true;

			Credential vm = new Credential();

			if ( AuthorizationCheck( "edit", true, ref status ) == false )
			{
				ConsoleMessageHelper.SetConsoleInfoMessage( "ERROR - NOT AUTHENTICATED. You will not be able to add or update content", "", false );
				return RedirectToAction( "Index", "Home" );
			}
			else
			{
				if ( id == 0 )
				{
					if ( AccountServices.CanUserPublishContent( user, ref status ) == false )
					{
						ConsoleMessageHelper.SetConsoleInfoMessage( status, "", false );
					}
				}
				else
				{
					//check if user can edit the entity
					if ( CredentialServices.CanUserUpdateCredential( id, user, ref status, ref vm ) == false )
					{
						//todo - may want to redirect elsewhere
						ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - you do not have authorization to edit that credential", "", false );
						return RedirectToAction( "Index", "Home" );
					}
					else
					{
						vm = CredentialServices.GetCredential( id, true );
						if ( vm.Id == 0 )
						{
							ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested credential was not found", "", false );
						}
					}
				}
			}

			return View( "Edit", vm );
		}
		//

		/// <summary>
		/// Echo data sent back from the client after parsing it into an object - useful for testing
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		[AcceptVerbs(HttpVerbs.Post)]
		public JsonResult Echo( Credential input )
		{
			return JsonHelper.GetJsonWithWrapper( input, true, "okay", null );
		}

		/// <summary>
		/// Accept data from the client and create/update as necessary
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		[AcceptVerbs( HttpVerbs.Post )]
		public JsonResult Update( Credential input, string section )
		{
			CredentialServices mgr = new CredentialServices();
			
			if ( AccountServices.AuthorizationCheck( "updates", true, ref status, ref user ) == false )
			{
				return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}

			if ( CredentialServices.CanUserUpdateCredential( input.Id, user, ref status ) == false )
			{
				//todo - may want to redirect elsewhere
				ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - you do not have authorization to edit this credential", "", false );
				input = new Credential();

				return JsonHelper.GetJsonWithWrapper( input, true, "okay", null );
			}

			if ( section == "basic")
			{
				if ( ValidateCredential( input, ref messages ) == false )
				{
					status = string.Join( ",", messages.ToArray() );
					return JsonHelper.GetJsonWithWrapper( null, false, status, input );
				}
			}
			var valid = true;
		
			if ( input.Id > 0 )
			{
				input.LastUpdatedById = user.Id;
				if ( section == "basic" )
				{
					valid = mgr.Credential_Update( input, user, ref status );
				}
				else
				{
					valid = mgr.Credential_UpdateSection( input, user, section, ref status );
				}
			}
			else
			{
				if ( section == "basic" )
				{
					input.LastUpdatedById = user.Id;
					input.CreatedById = user.Id;
					input.Id = mgr.Credential_Add( input, user, ref valid, ref status );
				}
				else
				{
					status = "You cannot update this section without first saving the credential.";
					return JsonHelper.GetJsonWithWrapper( null, false, status, null );
				}
			}

			if ( !valid  )
			{
				return JsonHelper.GetJsonWithWrapper( null, false, status, input );
			}

			var output = CredentialServices.GetCredential( input.Id, true );
			return JsonHelper.GetJsonWithWrapper( output, true, "okay", null );
		}
		//
		private bool ValidateCredential( Credential entity, ref List<string> messages )
		{
			bool isValid = true;
			//			List<string> messages = new List<string>();
			if ( string.IsNullOrWhiteSpace( entity.Name ) )
				messages.Add( "Credential name is required" );

			if ( string.IsNullOrWhiteSpace( entity.Description ) )
				messages.Add( "Credential description is required" );
			if ( !string.IsNullOrWhiteSpace( entity.DateEffective ) && ServiceHelper.IsDate( entity.DateEffective ) == false )
			{
				messages.Add( "Credential Effective Date is invalid" );
			}
			if ( messages.Count > 0 )
				isValid = false;

			return isValid;
		}
		
		[AcceptVerbs( HttpVerbs.Post )]
		public JsonResult Delete( int id )
		{
			if ( AuthorizationCheck( "delete", true, ref status ) == false )
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
		public JsonResult DeleteProfile( int id, string profile )
		{
			if ( AuthorizationCheck( "delete", true, ref status ) == false )
			{
				return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}

			var valid = true;


			if ( id > 0 && string.IsNullOrWhiteSpace( profile ) == false )
			{

				valid = new CredentialServices().DeleteProfile( id, profile, user, ref status );
				if ( !valid )
				{
					return JsonHelper.GetJsonWithWrapper( null, false, "Error deleting record: " + status, null );
				}
			}
			else
		{
				return JsonHelper.GetJsonWithWrapper( null, false, "You must select an item to delete.", null );
			}

			return JsonHelper.GetJsonWithWrapper( null, true, "Deleted successfully", null );
		}
		//

		#region framework items
		public JsonResult SocList( string keyword, int headerId = 0, int maxRows = 25 )
		{

			var result = EnumerationServices.SOC_Autocomplete( 0, headerId, keyword, maxRows );

			return Json( result, JsonRequestBehavior.AllowGet );
		}

		public JsonResult NaicsList( string keyword, int headerId = 0, int maxRows = 25 )
		{

			var result = EnumerationServices.NAICS_Autocomplete( 0, headerId, keyword, maxRows );

			return Json( result, JsonRequestBehavior.AllowGet );
		}

		[AcceptVerbs( HttpVerbs.Post )]
		public JsonResult FrameworkItemAdd( int credentialId, int categoryId, int codeID )
		{
			if ( AuthorizationCheck( "add", true, ref status ) == false )
			{
				return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}

			var valid = true;

			if ( credentialId > 0 && categoryId > 0 && codeID > 0  )
			{

				EnumeratedItem item = new ProfileServices().FrameworkItem_Add( credentialId, 
						1, 
						categoryId, 
						codeID, 
						user, 
						ref valid, 
						ref status );
				if ( !valid )
				{
					return JsonHelper.GetJsonWithWrapper( null, false, "Error adding record: " + status, null );
				}
			}
			else
			{
				return JsonHelper.GetJsonWithWrapper( null, false, "You must provide all the necessary identifiers in order to do an add.", null );
			}

			return JsonHelper.GetJsonWithWrapper( null, true, "Added successfully", null );
		}
		//
		//[AcceptVerbs( HttpVerbs.Post )]
		//public JsonResult FrameworkItemDelete( int id )
		//{
		//	if ( AuthorizationCheck( "delete", true, ref status ) == false )
		//	{
		//		return JsonHelper.GetJsonWithWrapper( null, false, status, null );
		//	}

		//	var valid = true;
		//	if ( id > 0 )
		//	{
		//		new ProfileServices().FrameworkItem_Delete(0 , id, 1, user, ref valid, ref status );
		//		if ( !valid )
		//		{
		//			return JsonHelper.GetJsonWithWrapper( null, false, "Error deleting record: " + status, null );
		//		}
		//	}
		//	else
		//	{
		//		return JsonHelper.GetJsonWithWrapper( null, false, "You must select an item to delete.", null );
		//	}

		//	return JsonHelper.GetJsonWithWrapper( null, true, "Deleted successfully", null );
		//}
		//
		#endregion 
		private bool AuthorizationCheck( string action, bool mustBeLoggedIn, ref string status )
		{
			bool isAuth = true;

			if ( mustBeLoggedIn &&
				!User.Identity.IsAuthenticated )
			{
				status = string.Format( "You must be logged in to do that ({0}).", action );
				return false;
			}

			if ( User.Identity.IsAuthenticated )
				user = AccountServices.GetCurrentUser( User.Identity.Name );

			if ( action.ToLower() == "delete" )
			{

				//TODO: validate user's ability to delete a specific credential (though this should probably be handled by the delete method?)
				if ( AccountServices.IsUserSiteStaff( user ) == false )
				{
					ConsoleMessageHelper.SetConsoleInfoMessage( "Sorry - You have not been authorized to delete content on this site during this <strong>BETA</strong> period.", "", false );

					status = "You have not been authorized to delete content on this site during this BETA period.";
					return false;
				}
			}
			return isAuth;

		}
	}
}