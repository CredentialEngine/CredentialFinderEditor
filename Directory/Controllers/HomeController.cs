using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CTIServices;
using Utilities;

namespace CTI.Directory.Controllers
{
	public class HomeController : Controller
	{
		//
		// GET: /Home/
		public ActionResult Index()
		{
			return View( "~/Views/V2/Home/Index.cshtml" );
		}

		public ActionResult About()
		{
			string pageMessage = "";

			if ( Session[ "siteMessage" ] != null )
			{
				pageMessage = Session[ "siteMessage" ].ToString();
				//setting console message doesn't work when switching to a different controller
				Utilities.ConsoleMessageHelper.SetConsoleErrorMessage( pageMessage, "", true );
				Session.Remove( "siteMessage" );
			}
			//else
			//if ( Session[ "SystemMessage" ] != null )
			//{
			//	pageMessage = Session[ "SystemMessage" ].ToString();
			//	//setting console message doesn't work when switching to a different controller
			//	Utilities.ConsoleMessageHelper.SetConsoleErrorMessage( pageMessage, "", true );
			//	Session.Remove( "SystemMessage" );
			//}
			else if ( AccountServices.IsUserAuthenticated())
			{
				if ( !AccountServices.CanUserCreateContent() )
				{
					string noOrganizationMessage = UtilityManager.GetAppKeyValue( "noOrganizationMessage", "" );
					if ( !string.IsNullOrWhiteSpace( noOrganizationMessage ) )
					{
						Utilities.ConsoleMessageHelper.SetConsoleErrorMessage( noOrganizationMessage, "", true );
					}
				}
				else 
				if ( !AccountServices.CanUserViewSite() )
				{
					string loginRequiredMessage = UtilityManager.GetAppKeyValue( "loginRequiredMessage", "" );
					if ( !string.IsNullOrWhiteSpace( loginRequiredMessage ))
					{
						Utilities.ConsoleMessageHelper.SetConsoleErrorMessage( loginRequiredMessage, "", true );
						//Utilities.ConsoleMessageHelper.SetConsoleErrorMessage( "This site is not currently open to the public. You must be logged in and authorized in order to use this site.", "", true );
					}
				}
			}

			//just show the home page. The difference is that user can be a guest. 
			return Index();
		}

		public ActionResult GettingStarted()
		{
		

			return View();
		}

        public ActionResult History()
        {
            return View( "~/Views/V2/Home/History.cshtml" );
        }
    }
}