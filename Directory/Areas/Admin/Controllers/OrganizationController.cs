using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CTIServices;
using Utilities;

namespace CTI.Directory.Areas.Admin.Controllers
{
    public class OrganizationController : Controller
    {

		public string RoleName { get; set; }
		// GET: Users
		public Boolean IsAdminUser()
		{
			if ( User.Identity.IsAuthenticated )
			{
				var identity = User.Identity;
				var user = AccountServices.GetCurrentUser();

				if ( user.UserRoles.Contains( "Administrator" ) )
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			return false;
		}

        // GET: Admin/Organization
        public ActionResult Index()
        {
			return RedirectToAction( "Members" );
            //return View( );
        }

		// GET: Admin/Organization/Members
		public ActionResult Members()
		{
			//if admin, show org search
			if ( !AccountServices.IsUserAuthenticated() )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "You must be logged in and authorized to perform this action." );

				return RedirectToAction( "Login", "Account", new { area = "" } );
			}
			return View();
		}
    }
}
