using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Web.Razor;
using System.Web.Razor.Parser;
using System.Web.Razor.Editor;
using System.Web.Razor.Generator;
using System.Web.Razor.Resources;
using System.Web.Razor.Text;
using System.Web.Razor.Tokenizer;
using System.Web.Razor.Utils;

namespace CTI.Directory.Controllers
{
  public class StyleController : Controller
  {
		public ActionResult CommonV2()
		{
			Response.ContentType = "text/css";
			return View( "~/Views/V2/Style/commonV2.cshtml" );
		}
		//

		public ActionResult CassStyles()
		{
			Response.ContentType = "text/css";
			return View( "~/Views/V2/Style/CassStyles.cshtml" );
		}
		//

		public ActionResult AccountBox()
		{
			Response.ContentType = "text/css";
			return View( "~/Views/V2/Style/account.cshtml" );
		}
		//

	}
}