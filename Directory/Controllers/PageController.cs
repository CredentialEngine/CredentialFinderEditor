using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.IO;

namespace CTI.Directory.Controllers
{
    public class PageController : BaseController
    {
        public ActionResult Index()
        {
            return Redirect( "~/" );
        }

        //
        // GET: /Page/
        //     public ActionResult Page( string name )
        //     {
        //var file = Server.MapPath( "~/Views/Page/" + name + ".cshtml" );
        //if ( System.IO.File.Exists( file ) )
        //{
        //	return View( file );
        //}

        //return RedirectToAction( "Index", "Home" );
        //     }
        public ActionResult Page( string page )
        {
            return RoutePage( "Page/" + page );
        }

        public ActionResult RoutePage( string routePage )
        {
            return ViewPage( "~/Views/" + routePage + ".cshtml", "Index" );
        }
        public ActionResult Competencies()
		{
			return View( "~/Views/Page/Competencies.cshtml" );
		}
	}
}