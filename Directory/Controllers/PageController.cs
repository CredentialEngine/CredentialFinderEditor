using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.IO;

namespace CTI.Directory.Controllers
{
    public class PageController : Controller
    {
        //
        // GET: /Page/
        public ActionResult Page( string name )
        {
					var file = Server.MapPath( "~/Views/Page/" + name + ".cshtml" );
					if ( System.IO.File.Exists( file ) )
					{
						return View( file );
					}

					return RedirectToAction( "Index", "Home" );
        }
	}
}