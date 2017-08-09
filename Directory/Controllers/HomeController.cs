using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CTIServices;

namespace CTI.Directory.Controllers
{
	public class HomeController : Controller
	{
		//
		// GET: /Home/
		public ActionResult Index()
		{
			//string envType = Utilities.UtilityManager.GetAppKeyValue("envType", "dev");

			//return View( "Index" );
			return V2();
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
			else
			{
				if ( !AccountServices.CanUserViewSite() )
				{
					Utilities.ConsoleMessageHelper.SetConsoleErrorMessage( "This site is not currently open to the public. You must be logged in and authorized in order to use this site.", "", true );
				}
			}
			return Index();
		}

		public ActionResult V2()
		{
			return View( "~/Views/V2/Home/Index.cshtml" );
		}

	}
}