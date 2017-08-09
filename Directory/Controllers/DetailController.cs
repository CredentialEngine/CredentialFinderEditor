using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CTIServices;
using Models;
using Models.Common;
using Models.ProfileModels;
using Utilities;


namespace CTI.Directory.Controllers
{
	public class DetailController : BaseController
    {
		AppUser user = new AppUser();
		string status = "";
		//string notAuthMessage = "You are not authorized to view this page. <p>During the Beta period, only authorized people may view private data.</p>";
		SiteMessage msg = new SiteMessage() { Title = "<h2>Unauthorized Action</h2>", Message = "<p>You are not authorized to view this page.</p> <p>During the Beta period, only authorized people may view private data.</p>" };
		SiteMessage notFoundMsg = new SiteMessage() { Title = "<h2>Not Found</h2>", Message = "<p>The requested record does not exist. Please use the search to locate the correct record.</p>" };

		public ActionResult Credential( int id, string name = "" )
		{
			if ( User.Identity.IsAuthenticated )
				user = AccountServices.GetCurrentUser( User.Identity.Name );

			Credential entity = new Credential();
			if ( !CredentialServices.CanUserViewCredential( id, user, ref entity ) )
			{
				if ( entity.Id > 0 )
					Session[ "SystemMessage" ] = msg;
				else
					Session[ "SystemMessage" ] = notFoundMsg;
				return RedirectToAction( "Index", "Message" );
			}
			//HttpContext.Server.ScriptTimeout = 300;
			string refresh = Request.Params[ "refresh" ];
			bool skippingCache = FormHelper.GetRequestKeyValue( "skipCache", false );

			var vm = CredentialServices.GetCredentialDetail( id, user, skippingCache );

			if ( id > 0 && vm.Id == 0 )
			{
				SetPopupErrorMessage( "ERROR - the requested Credential record was not found " );
				return RedirectToAction( "Index", "Home" );
			}

			if ( Request.Params[ "v2" ] == "true" )
			{
				return View( "~/Views/V2/Detail/Index.cshtml", vm );
			}

			if(Request.Params["v3"] == "true" )
			{
				return View( "~/Views/V2/DetailV3/Detail.cshtml", vm );
			}

			return View( "~/Views/V2/DetailV4/Detail.cshtml", vm );

		}
		//

		public ActionResult Organization( int id, string name = "" )
		{
			if ( User.Identity.IsAuthenticated )
				user = AccountServices.GetCurrentUser( User.Identity.Name );

			Organization vm = new Organization();
			//check if can view the org
			//method returns the org as well
			if ( !OrganizationServices.CanUserViewOrganization( id, user, ref vm ) )
			{
				//ConsoleMessageHelper.SetConsoleErrorMessage( notAuthMessage, "", false );
				if (vm.Id > 0)
					Session[ "SystemMessage" ] = msg;
				else
					Session[ "SystemMessage" ] = notFoundMsg;
				return RedirectToAction( "Index", "Message" );
			}

			vm = OrganizationServices.GetOrganizationDetail( id, user );

			if ( id > 0 && vm.Id == 0 )
			{
				SetPopupErrorMessage ("ERROR - the requested organization record was not found ");
				return RedirectToAction( "Index", "Home" );
			}

			if ( Request.Params[ "v2" ] == "true" )
			{
				return View( "~/Views/V2/Detail/Index.cshtml", vm );
			}

			if ( Request.Params[ "v3" ] == "true" )
			{
				return View( "~/Views/V2/DetailV3/Detail.cshtml", vm );
			}

			return View( "~/Views/V2/DetailV4/Detail.cshtml", vm );
		}
		//
		public ActionResult QAOrganization( int id, string name = "" )
		{
			return Organization( id, name );
			/*
			if ( User.Identity.IsAuthenticated )
				user = AccountServices.GetCurrentUser( User.Identity.Name );

			Organization vm = new Organization();
			//check if can view the org
			//method returns the org as well
			//17-03-08 mp - so far no difference in call for a QA org. Appropriate data will be returned for view to handle
			if ( !OrganizationServices.CanUserViewQAOrganization( id, user, ref vm ) )
			{
				if ( vm.Id > 0 )
					Session[ "SystemMessage" ] = msg;
				else
					Session[ "SystemMessage" ] = notFoundMsg;
				return RedirectToAction( "Index", "Message" );
			}

			//var vm = OrganizationServices.GetOrganizationDetail( id, user );

			if ( id > 0 && vm.Id == 0 )
			{
				SetPopupErrorMessage( "ERROR - the requested organization record was not found " );
				return RedirectToAction( "Index", "Home" );
			}

			if ( Request.Params[ "v2" ] == "true" )
			{
				return View( "~/Views/V2/Detail/Index.cshtml", vm );
			}

			if ( Request.Params[ "v3" ] == "true" )
			{
				return View( "~/Views/V2/DetailV3/Detail.cshtml", vm );
			}

			return View( "~/Views/V2/DetailV4/Detail.cshtml", vm );
			*/
		}
		//

		public ActionResult Assessment( int id, string name = "" )
		{
			if ( User.Identity.IsAuthenticated )
				user = AccountServices.GetCurrentUser( User.Identity.Name );

			AssessmentProfile vm = AssessmentServices.GetDetail( id, user );
			if ( !AssessmentServices.CanUserViewAssessment( vm, user, ref status ) )
			{
				if ( vm.Id > 0 )
					Session[ "SystemMessage" ] = msg;
				else
					Session[ "SystemMessage" ] = notFoundMsg;
				return RedirectToAction( "Index", "Message" );
			}

			if ( id > 0 && vm.Id == 0 )
			{
				SetPopupErrorMessage( "ERROR - the requested Assessment record was not found " );
				return RedirectToAction( "Index", "Home" );
			}

			if ( Request.Params[ "v2" ] == "true" )
			{
				return View( "~/Views/V2/Detail/Index.cshtml", vm );
			}

			if ( Request.Params[ "v3" ] == "true" )
			{
				return View( "~/Views/V2/DetailV3/Detail.cshtml", vm );
			}

			return View( "~/Views/V2/DetailV4/Detail.cshtml", vm );
		}
		//

		public ActionResult LearningOpportunity( int id, string name = "" )
		{
			if ( User.Identity.IsAuthenticated )
				user = AccountServices.GetCurrentUser( User.Identity.Name );

			var vm = LearningOpportunityServices.GetForDetail( id, user );
			if ( !LearningOpportunityServices.CanUserViewLearningOpportunity( vm,  user,  ref status ) )
			{
				//ConsoleMessageHelper.SetConsoleErrorMessage( notAuthMessage, "", false );
				Session[ "SystemMessage" ] = msg;
				return RedirectToAction( "Index", "Message" );
			}
			

			if ( id > 0 && vm.Id == 0 )
			{
				SetPopupErrorMessage( "ERROR - the requested Learning Opportunity record was not found " );
				return RedirectToAction( "Index", "Home" );
			}

			if ( Request.Params[ "v2" ] == "true" )
			{
				return View( "~/Views/V2/Detail/Index.cshtml", vm );
			}

			if ( Request.Params[ "v3" ] == "true" )
			{
				return View( "~/Views/V2/DetailV3/Detail.cshtml", vm );
			}

			return View( "~/Views/V2/DetailV4/Detail.cshtml", vm );
		}
		//

	}
}