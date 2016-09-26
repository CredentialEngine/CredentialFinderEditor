using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CTIServices;
using Models;
using Models.Common;
using Utilities;

/* Nothing in here should be used anymore - but the methods might be useful for references later */
namespace CTI.Directory.Controllers
{
	public class OrganizationManagementController : Controller
	{

		AppUser user = new AppUser();
		string status = "";
		List<string> messages = new List<string>();

		//
		// GET: /Organization/
		public ActionResult Index( string keyword = "" )
		{
			AccountServices.AuthorizationCheck( "", false, ref status );

			var vm = OrganizationServices.QuickSearch( keyword );

			return View( "Index", vm );
		}
		public JsonResult List( string keyword, int maxTerms = 25 )
		{
			AccountServices.AuthorizationCheck( "", false, ref status );
			var result = OrganizationServices.Organization_Autocomplete( user, keyword, maxTerms );

			return Json( result, JsonRequestBehavior.AllowGet );
		}
		/// <summary>
		/// View the details for a Organization
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
			AccountServices.AuthorizationCheck( "detail", false, ref status , ref user);
			
			var vm = OrganizationServices.GetOrganizationDetail( id, user );
			if ( id > 0 && vm.Id == 0 )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested organization was not found", "", false );
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
			AccountServices.AuthorizationCheck( "detail", false, ref status, ref user );

			var vm = OrganizationServices.GetOrganizationDetail( id, user );
			if ( id > 0 && vm.Id == 0 )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested organization was not found", "", false );
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
		/// Create or edit a organization
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[AcceptVerbs( HttpVerbs.Get )]
		public ActionResult Edit( int id = 0 )
		{
			Organization vm = new Organization();

			ViewBag.Message = "";
			ViewBag.GetOrgs = false;

			if (AccountServices.AuthorizationCheck( "edit", true, ref status, ref user ) == false)
			{
				ConsoleMessageHelper.SetConsoleInfoMessage( "ERROR - NOT AUTHENTICATED. You will not be able to add or update content", "", false );
				vm.Id = 0;
				return RedirectToAction( "Index", "Home" );
			}
			else
			{
				if ( id == 0 )
				{
					if (AccountServices.CanUserAddOrganizations( user, ref status ) == false )
						ConsoleMessageHelper.SetConsoleInfoMessage( status, "", false );

				}
				else
				{
					//check if user can edit the org
					if ( OrganizationServices.CanUserUpdateOrganization( user, id ) == false )
					{
						ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - you do not have authorization to edit this organization", "", false );
						vm = new Organization();
						return RedirectToAction( "Index", "Home" );
					}
					else
					{
						vm = OrganizationServices.GetOrganization( id );

						if ( vm.Id == 0 )
						{
							ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested organization was not found", "", false );
						}
					}
				}
			}
			return View( "Edit", vm );
		}
		//

		/// <summary>
		/// Add new department for an organization
		/// </summary>
		/// <param name="orgId"></param>
		/// <returns></returns>
		//[AcceptVerbs( HttpVerbs.Get )]
		//public ActionResult AddDepartment( int orgId )
		//{
		//	Organization vm = new Organization();
		//	int deptId = 0;
		//	ViewBag.Message = "";
		//	ViewBag.GetOrgs = false;

		//	if ( AccountServices.AuthorizationCheck( "edit", true, ref status ) == false )
		//	{
		//		ConsoleMessageHelper.SetConsoleInfoMessage( "ERROR - NOT AUTHENTICATED. You will not be able to add or update content", "", false );
		//		vm.Id = 0;
		//	}
		//	else
		//	{
		//		if ( AccountServices.CanUserPublishContent( user, ref status ) == false )
		//		{
		//			ConsoleMessageHelper.SetConsoleInfoMessage( status, "", false );

		//			deptId = 0;
		//		}
		//		else
		//		{
		//			vm = OrganizationServices.GetOrganization( deptId );

		//		}
		//	}
		//	return View( "Edit", vm );
		//	//return View( "Edit" );
		//}
		//

		/// <summary>
		/// Echo data sent back from the client after parsing it into an object - useful for testing
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		[AcceptVerbs( HttpVerbs.Post )]
		public JsonResult Echo( Organization input )
		{
			return JsonHelper.GetJsonWithWrapper( input, true, "okay", null );
		}

		/// <summary>
		/// Accept data from the client and create/update as necessary
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		[AcceptVerbs( HttpVerbs.Post )]
		public JsonResult Update( Organization input )
		{
			ViewBag.Message = "";
			ViewBag.GetOrgs = false;
			bool isValid = true;
			status = "okay";
			OrganizationServices mgr = new OrganizationServices();
			if ( AccountServices.AuthorizationCheck( "updates", true, ref status, ref user ) == false )
			{
				return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}

			int id = 0;
			//AppUser user = AccountServices.GetCurrentUser( User.Identity.Name );
			
			if ( ValidateOrganization( input, ref messages ) == false )
			{
				status = string.Join( ",", messages.ToArray() );
				return JsonHelper.GetJsonWithWrapper( null, false, status, input );
			}

			if ( input.Id > 0 )
			{
				if ( OrganizationServices.CanUserUpdateOrganization( user, input.Id ) == false )
				{
					ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - you do not have authorization to update this organization", "", false );
					var output = new Organization();
					return JsonHelper.GetJsonWithWrapper( output, true, "okay", null );
				}
				else
				{
					input.LastUpdatedById = user.Id;
					isValid = mgr.Organization_Update( input, user, ref status );
				}
			}
			else
			{
				input.LastUpdatedById = user.Id;
				input.CreatedById = user.Id;
				id = mgr.Organization_Add( input, user, ref isValid, ref status );
				if ( id > 0 )
					input.Id = id;
			}


			var org = OrganizationServices.GetOrganization( input.Id );
			return JsonHelper.GetJsonWithWrapper( org, isValid, status, null );
		}
		//

		[AcceptVerbs( HttpVerbs.Post )]
		public JsonResult Delete( int id )
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
		public JsonResult DeleteProfile( int id, string profile )
		{
			if ( AccountServices.AuthorizationCheck( "delete", true, ref status, ref user ) == false )
			{
				return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}
			var valid = true;
		

			if ( id > 0 && string.IsNullOrWhiteSpace( profile ) == false )
			{
				new OrganizationServices().DeleteProfile( id, profile, ref valid, ref status );
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
		//[AcceptVerbs( HttpVerbs.Post )]
		//public JsonResult SocialMediaDelete( int parentId, int recordId )
		//{
		//	if ( AccountServices.AuthorizationCheck( "delete", true, ref status ) == false )
		//	{
		//		return JsonHelper.GetJsonWithWrapper( null, false, status, null );
		//	}
		//	var valid = true;
	
		//	//don't really need parent
		//	if ( parentId > 0 && recordId > 0  )
		//	{
		//		return JsonHelper.GetJsonWithWrapper( null, false, "Section deletes not implemented yet.", null );
		//		//new OrganizationServices().Organization_DeleteSection( parentId, section, recordId, ref valid, ref status );
		//		//if ( !valid )
		//		//{
		//		//	return JsonHelper.GetJsonWithWrapper( null, false, "Error deleting credential: " + status, null );
		//		//}
		//	}
		//	else
		//{
		//		return JsonHelper.GetJsonWithWrapper( null, false, "You must select an item to delete.", null );
		//	}

		//	return JsonHelper.GetJsonWithWrapper( null, true, "Deleted successfully", null );
		//}
		////
	//	private bool AuthorizationCheck(string action, bool mustBeLoggedIn, ref string status)
	//	{
	//		bool isAuth = true;

	//		if ( mustBeLoggedIn && 
	//			!User.Identity.IsAuthenticated )
	//		{
	//			status = string.Format("You must be logged in to do that ({0}).",action);
	//			return false;
	//		}

	//		if ( User.Identity.IsAuthenticated )
	//			user = AccountServices.GetCurrentUser( User.Identity.Name );

	//		//user = AccountServices.GetCurrentUser( User.Identity.Name );
	//		if ( action == "Delete" )
	//		{

	//			//TODO: validate user's ability to delete a specific credential (though this should probably be handled by the delete method?)
	//			if ( AccountServices.IsUserSiteStaff( user ) == false )
	//			{
	//				ConsoleMessageHelper.SetConsoleInfoMessage( "Sorry - You have not been authorized to delete content on this site during this <strong>BETA</strong> period.", "", false );

	//				status = "You have not been authorized to delete content on this site during this BETA period.";
	//				return false;
	//			}
	//		}
	//		return isAuth;

	//}

		private bool ValidateOrganization( Organization entity, ref List<string> messages )
		{
			bool isValid = true;
			if ( string.IsNullOrWhiteSpace( entity.Name ) )
				messages.Add( "Organization name is required" );

			if ( string.IsNullOrWhiteSpace( entity.Description ) )
				messages.Add( "Organization description is required" );
			if ( string.IsNullOrWhiteSpace( entity.Url ) )
				messages.Add( "Organization URL is required" );

			//need different validation now
			//if ( !string.IsNullOrWhiteSpace( entity.FoundingDate ) && ServiceHelper.IsDate(entity.FoundingDate ) == false)
			//{
			//	messages.Add( "Organization Founding Date is invalid" );
			//}

			if ( messages.Count > 0 )
				isValid = false;

			return isValid;
		}
		
	}
}