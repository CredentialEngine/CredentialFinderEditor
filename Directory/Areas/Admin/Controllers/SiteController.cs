using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using CM = Models.Common;
using Utilities;
using CTIServices;
using Data;
using EntityContext = Data.CTIEntities;
using Factories;

namespace CTI.Directory.Areas.Admin.Controllers
{
    public class SiteController : CTI.Directory.Controllers.BaseController
	{
        // GET: Admin/Site
        public ActionResult Index()
        {
            return View();
        }

		[Authorize( Roles = "Administrator, Site Staff" )]
		public ActionResult RefreshOrganizationCache()
		{
			if ( !User.Identity.IsAuthenticated
				|| ( User.Identity.Name != "mparsons"
				&& User.Identity.Name != "cwd-mparsons@ad.siu.edu"
				&& User.Identity.Name != "cwd-mparsons@ad.siu.edu"
				&& User.Identity.Name != "nathan.argo@siuccwd.com" )
				)
			{
				SetSystemMessage( "Unauthorized Action", "You are not authorized to import users." );

				return RedirectToAction( "Index", "Message" );
			}

			using ( var context = new EntityContext() )
			{
				var result = context.Cache_Organization_ActorRoles_Populate(0);

				//now what
			}

			return RedirectToAction( "Index", "Message", new { area = "" } );
		}
                
		// Import users
		[Authorize( Roles = "Administrator, Site Staff" )]
		public ActionResult ImportMicrosoft( int maxRecords = 100 )
		{
			if ( !User.Identity.IsAuthenticated
				|| ( User.Identity.Name != "mparsons"
				&& User.Identity.Name != "cwd-mparsons@ad.siu.edu"
				&& User.Identity.Name != "nathan.argo@siuccwd.com" )
				)
			{
				//

				SetSystemMessage( "Unauthorized Action", "You are not authorized to request this import of Microsoft certifications." );

				return RedirectToAction( "Index", "Message" );
			}

			string report = "";
			List<string> summary = new List<string>();
			SiteServices mgr = new SiteServices();
			mgr.ImportMicrosoftCredentials( ref summary, maxRecords );
			foreach (string s in summary)
			{
				report += s + "<br/>";
			}
			LoggingHelper.DoTrace( 1, "Microsoft Import report: \r\n" + report );

			SetSystemMessage( "Microsoft Import", report );

			return RedirectToAction( "Index", "Message", new { area = "" } );
		}


        // fix addresses missing lat/lng, and normalize
        [Authorize( Roles = "Administrator, Site Staff" )]
        public ActionResult NormalizeAddresses( int maxRecords = 100 )
        {
            if ( !User.Identity.IsAuthenticated
                || ( User.Identity.Name != "mparsons"
                && User.Identity.Name != "cwd-mparsons@ad.siu.edu"
                && User.Identity.Name != "nathan.argo@siuccwd.com" )
                )
            {
                SetSystemMessage( "Unauthorized Action", "You are not authorized to invoke NormalizeAddresses." );
                return RedirectToAction( "Index", "Message" );
            }
            string report = "";
            string messages = "";
            List<CM.Address> list = new AddressProfileManager().ResolveMissingGeodata( ref messages, maxRecords: maxRecords );

            if ( !string.IsNullOrWhiteSpace( messages ) )
                report = "<p>Normalize Addresses: <br/>" + messages + "</p>"  ;

            foreach ( var address in list )
            {
                string msg = string.Format( " - Unable to resolve address: Id: {0}, address1: {1}, city: {2}, region: {3}, postalCode: {4}, country: {5} ", address.Id, address.Address1, address.City, address.AddressRegion, address.PostalCode, address.Country );
                LoggingHelper.DoTrace(2, msg );
                report += System.Environment.NewLine + msg;
            }

            SetSystemMessage( "Normalize Addresses", report );

            return RedirectToAction( "Index", "Message", new { area = "" } );
        }
    }
}