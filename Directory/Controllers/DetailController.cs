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
    public class DetailController : Controller
    {
		AppUser user = new AppUser();
		public ActionResult Credential( int id )
		{
			if ( User.Identity.IsAuthenticated )
				user = AccountServices.GetCurrentUser( User.Identity.Name );

			var vm = CredentialServices.GetCredentialDetail( id, user );

			if ( id > 0 && vm.Id == 0 )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested credential was not found", "", false );
			}
			return View( "~/Views/V2/Detail/Index.cshtml", vm );
		}
		//

		public ActionResult Organization( int id )
		{
			if ( User.Identity.IsAuthenticated )
				user = AccountServices.GetCurrentUser( User.Identity.Name );
			var vm = OrganizationServices.GetOrganizationDetail( id, user );

			if ( id > 0 && vm.Id == 0 )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested organization was not found", "", false );
			}
			return View( "~/Views/V2/Detail/Index.cshtml", vm );
		}
		//

		public ActionResult Assessment( int id )
		{
			if ( User.Identity.IsAuthenticated )
				user = AccountServices.GetCurrentUser( User.Identity.Name );
			AssessmentProfile vm = AssessmentServices.GetDetail( id, user );

			if ( id > 0 && vm.Id == 0 )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested Assessment was not found", "", false );
			}
			return View( "~/Views/V2/Detail/Index.cshtml", vm );
		}
		//

		public ActionResult LearningOpportunity( int id )
		{
			if ( User.Identity.IsAuthenticated )
				user = AccountServices.GetCurrentUser( User.Identity.Name );

			var vm = LearningOpportunityServices.GetForDetail( id, user );

			if ( id > 0 && vm.Id == 0 )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested Learning Opportunity was not found", "", false );
			}
			return View( "~/Views/V2/Detail/Index.cshtml", vm );
		}
		//

		}
}