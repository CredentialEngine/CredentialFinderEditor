using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Utilities;

namespace CTI.Directory.Controllers
{
    public class MessageController : Controller
    {
        // GET: Message
        public ActionResult Index()
        {
			string pageMessage = "";

			if ( Session[ "siteMessage" ] != null )
			{
				pageMessage = Session[ "siteMessage" ].ToString();

				Utilities.ConsoleMessageHelper.SetConsoleErrorMessage( pageMessage, "", true );
				Session.Remove( "siteMessage" );
			}

			return View( "~/Views/V2/Home/Index.cshtml" );
        }

		public ActionResult NotAuthorized()
		{
			return View();
		}
    }
}