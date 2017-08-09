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
    public class ReportsController : Controller
    {
        // GET: Report
        public ActionResult Index()
        {
			CommonTotals vm = new CommonTotals();
			

			vm = ReportServices.SiteTotals();

			return View( "~/Views/V2/Reports/Reports.cshtml", vm );
        }
    }
}