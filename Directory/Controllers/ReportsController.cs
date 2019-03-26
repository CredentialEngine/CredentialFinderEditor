using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Models;
using Models.Helpers.Reports;
using CTIServices;

namespace CTI.Directory.Controllers
{
    public class ReportsController : BaseController
    {
        // GET: Report
        public ActionResult Index()
        {
			CommonTotals vm = new CommonTotals();
			vm = ReportServices.SiteTotals();

			return View( "~/Views/V2/Reports/ReportsV2.cshtml", vm );
        }
		public ActionResult V1()
		{
			CommonTotals vm = new CommonTotals();
			vm = ReportServices.SiteTotals();
			return View( "~/Views/V2/Reports/Reports.cshtml", vm );
		}

		public JsonResult GetOrganizationStatistics( string organizationCTID, string password )
		{
			if( password != Utilities.ConfigHelper.GetConfigValue( "CEAccountSystemStaticPassword", Guid.NewGuid().ToString() ) )
			{
				return null;
			}

			var data = ReportServices.GetOrganizationStatistics( organizationCTID );
			return JsonResponse( data, true, "", null );
		}
		//
	}
}