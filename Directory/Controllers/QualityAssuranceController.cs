using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CTIServices;
using Models;
using Models.Common;
using Utilities;

namespace CTI.Directory.Controllers
{
    public class QualityAssuranceController : Controller
    {
		AppUser user = new AppUser();
		string status = "";

        // GET: QualityAssurance
		public ActionResult Index( string keyword = "" )
        {
			AuthorizationCheck( "", false, ref status );

			var vm = OrganizationServices.QA_Search( user, keyword );
			return View( vm );
		}
		public JsonResult List( string keyword, int maxTerms = 25 )
		{
			AuthorizationCheck( "", false, ref status );
			var result = OrganizationServices.QA_Autocomplete( user, keyword, maxTerms );

			return Json( result, JsonRequestBehavior.AllowGet );
		}

        // GET: QualityAssurance/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: QualityAssurance/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: QualityAssurance/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: QualityAssurance/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: QualityAssurance/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: QualityAssurance/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: QualityAssurance/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


		private bool AuthorizationCheck( string action, bool mustBeLoggedIn, ref string status )
		{
			bool isAuth = true;

			if ( mustBeLoggedIn &&
				!User.Identity.IsAuthenticated )
			{
				status = string.Format( "You must be logged in to do that ().", action );
				return false;
			}

			if ( User.Identity.IsAuthenticated )
				user = AccountServices.GetCurrentUser( User.Identity.Name );

			user = AccountServices.GetCurrentUser( User.Identity.Name );
			if ( action == "Delete" )
			{

				//TODO: validate user's ability to delete a specific credential (though this should probably be handled by the delete method?)
				if ( AccountServices.IsUserSiteStaff( user ) == false )
				{
					ConsoleMessageHelper.SetConsoleInfoMessage( "Sorry - You have not been authorized to delete content on this site during this <strong>BETA</strong> period.", "", false );

					status = "You have not been authorized to delete content on this site during this BETA period.";
					return false;
				}
			}
			return isAuth;

		}
    }
}
