using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json;
using System.Net.Http;

using Models.Common;
using CTIServices;
using Utilities;

namespace CTI.Directory.Controllers
{
    public class CompetenciesController : BaseController
    {
        // GET: Competencies
        public ActionResult Index()
        {
			return View( "~/Views/V2/Competencies/Competencies.cshtml" );
        }
        //
        public ActionResult Competency()
        {
            return RedirectToAction( "Index", "Competencies" );
        }
        //
        public ActionResult Overview()
        {
            return View("~/Views/V2/Competencies/Overview.cshtml");
        }

		#region exports
		//

		//public JsonResult ExportAllCompetencies( string password )
		//{
		//	if( password != "cass2017!" )
		//	{
		//		return null;
		//	}

		//	var data = CompetencyServices.ExportAllCTDLASNCompetencies();

		//	return new JsonResult() { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		//}

		//public JsonResult ExportAllApprovedCompetencies( string password, bool requireHasApproval = false )
  //      {
  //          if ( password != "cass2017!" )
  //          {
  //              return null;
  //          }

  //          var data = CompetencyServices.ExportAllApprovedCompetencies( requireHasApproval );

  //          return new JsonResult() { Data = data,
  //              JsonRequestBehavior = JsonRequestBehavior.AllowGet,
  //              MaxJsonLength = 2147483647 };
  //      }
       /// <summary>
       /// Retrieve all competencies approved/published excluding last dataset (2/2018?)
       /// NOTE: may need to check for changes to published competencies
       /// </summary>
       /// <param name="password"></param>
       /// <param name="requireHasApproval">Or do we mean published?</param>
       /// <returns></returns>
        //public JsonResult ExportAllUpdatedApprovedCompetencies( string password, bool requireHasApproval = false )
        //{
        //    if ( password != "cass2017!" )
        //    {
        //        return null;
        //    }

        //    var data = CompetencyServices.ExportAllApprovedCompetencies( requireHasApproval );

        //    return new JsonResult() { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        //}

        //public JsonResult ExportAllConditonProfileCompetenciesAsCTDLASN( string password, bool requireHasApproval = false )
        //{
        //    if ( password != "cass2017!" )
        //    {
        //        return null;
        //    }

        //    var data = CompetencyServices.ExportAllConditonProfileCompetenciesAsCTDLASN( requireHasApproval );

        //    return new JsonResult() { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        //}

		#endregion 
		//
		public string GetOrganizationsForUser()
		{
			return OrganizationServices.GetAccountOrganizationsForUserAsJson();
			//var user = AccountServices.GetUserFromSession();
			//var password = UtilityManager.GetAppKeyValue( "CEAccountSystemStaticPassword", "" );
			//var environment = UtilityManager.GetAppKeyValue( "envType", "" ) == "production" ? "production" : "development";
            
   //         var organizationsForUserURL = UtilityManager.GetAppKeyValue( "CEAccountOrganizationsForUserApi" );
   //         var payload = new
			//{
			//	email = user.Email,
			//	password = password
			//};

			//var content = new StringContent( JsonConvert.SerializeObject( payload ), System.Text.Encoding.UTF8, "application/json" );
			//var organizationsForUserJSON = new HttpClient().PostAsync( organizationsForUserURL, content ).Result.Content.ReadAsStringAsync().Result;

			//return organizationsForUserJSON;
		}
		//

		public JsonResult CheckFrameworkStatus( string frameworkCTID, string frameworkName, string frameworkURL, string ownerOrganizationCTID )
		{
			//Get the data
			var user = AccountServices.GetUserFromSession();
			var framework = CASS_CompetencyFrameworkServices.GetFrameworkByCTID( frameworkCTID );
			var messages = new List<string>();
			if ( frameworkName == "New Framework")
			{
				//ignore as occurs on intial click of the add icon
				//if the name doesn't change, so be it
				return JsonResponse( new StatusSummary( new CASS_CompetencyFramework() ), true, string.Join( ", ", messages ), null );
			}
			//If there is no framework record yet, create one
			if ( framework == null || framework.Id == 0 )
			{
                if (string.IsNullOrWhiteSpace( ownerOrganizationCTID ))
                {
                    messages.Add( "Error - the organization CTID is null." );
                    return JsonResponse( new StatusSummary( framework ), framework != null && framework.Id > 0, "", new { statusList = messages } );
                }
                var ownerOrganization = OrganizationServices.GetByCtid( ownerOrganizationCTID );
                if (ownerOrganization == null || ownerOrganization.Id == 0)
                {
                    messages.Add( "Error - the owing organization was not found." );
                    return JsonResponse( new StatusSummary( framework ), framework != null && framework.Id > 0, "", new { statusList = messages } );
                }
				var newFramework = new CASS_CompetencyFramework()
				{
					CTID = frameworkCTID,
					FrameworkName = frameworkName,
					CreatedById = user.Id,
					OrgId = ownerOrganization.Id,
					ExternalIdentifier = frameworkURL //Not sure if this is correct
				};
				var newFrameworkID = new CASS_CompetencyFrameworkServices().AddFramework( newFramework, user, ref messages );
				framework = CASS_CompetencyFrameworkServices.GetFrameworkByID( newFrameworkID );
			}

			//Return status
			return JsonResponse( new StatusSummary( framework ), framework != null && framework.Id > 0 && messages.Count() == 0, string.Join( ", ", messages ), null );
		}
	
        //
        public JsonResult MarkFrameworkUpdated( string frameworkCTID, string frameworkName, string ownerOrganizationCTID )
        {
            //Get the data
            var user = AccountServices.GetUserFromSession();


			//Mark the update
			var messages = new List<string>();
			if ( frameworkName == "New Framework" )
			{
				//ignore as occurs on intial click of the add icon
				//if the name doesn't change, so be it
				return JsonResponse( new StatusSummary( new CASS_CompetencyFramework() ), true, string.Join( ", ", messages ), null );
			}

			new CASS_CompetencyFrameworkServices().MarkFrameworkUpdated(frameworkCTID, frameworkName, user, ref messages);

            //Return status
            var framework = CASS_CompetencyFrameworkServices.GetFrameworkByCTID(frameworkCTID);
            return JsonResponse(new StatusSummary(framework), messages.Count() == 0, string.Join(", ", messages), null);
        }
        //
        public JsonResult ApproveFramework( string frameworkCTID, string ownerOrganizationCTID )
		{
			//Get the data
			var user = AccountServices.GetUserFromSession();

			//Do the approval
			var messages = new List<string>();
			CASS_CompetencyFrameworkServices.ApproveFramework( frameworkCTID, user, ref messages );
            bool isValid = messages.Count() == 0;
            //Return status
            var framework = CASS_CompetencyFrameworkServices.GetFrameworkByCTID( frameworkCTID );
			return JsonResponse( new StatusSummary( framework ), isValid, string.Join( ", ", messages ), null );
		}
		//

		public JsonResult PublishFramework( string frameworkCTID, string frameworkExportJSON, string ownerOrganizationCTID )
		{
			//Get the data
			var user = AccountServices.GetUserFromSession();
			var messages = new List<string>();

			//Do the publish
			CASS_CompetencyFrameworkServices.PublishFramework( frameworkCTID, frameworkExportJSON, user, ref messages );

			//Return status
			var framework = CASS_CompetencyFrameworkServices.GetFrameworkByCTID( frameworkCTID );
			return JsonResponse( new StatusSummary( framework ), messages.Count() == 0, string.Join( ", ", messages ), null );
		}
        public JsonResult PublishApprovedFrameworks( string passkey )
        {
            //Get the data
            var user = AccountServices.GetUserFromSession();
            if (!AccountServices.IsUserSiteStaff(user))
            {
                return JsonResponse( null, false, "Error - you are not authorized for this action", null );
            }
            if ( passkey != "%6RDCiujp_oij" )
            {
                return JsonResponse( null,false,"Error - you are not authorized for this action. Invalid passkey.",null );
            }
            //Do the publish
            var messages = new List<string>();
            CASS_CompetencyFrameworkServices.PublishAllApprovedFrameworks(user, ref messages );

            return JsonResponse( null, messages.Count() == 0, string.Join( ", ", messages ), null );
        }
		//
		public JsonResult RepublishPublishedFrameworks( string passkey )
		{
			//Get the data
			var user = AccountServices.GetUserFromSession();
			if ( !AccountServices.IsUserSiteStaff( user ) )
			{
				return JsonResponse( null, false, "Error - you are not authorized for this action", null );
			}
			if ( passkey != "%6RDCiujp_oij" )
			{
				return JsonResponse( null, false, "Error - you are not authorized for this action. Invalid passkey.", null );
			}
			//Do the publish
			var messages = new List<string>();
			CASS_CompetencyFrameworkServices.PublishAllApprovedFrameworks( user, ref messages );

			return JsonResponse( null, messages.Count() == 0, string.Join( ", ", messages ), null );
		}
		//
		public JsonResult UnPublishFramework( string frameworkCTID, string ownerOrganizationCTID )
        {
            //Get the data
            var user = AccountServices.GetUserFromSession();

            //Do the publish
            var messages = new List<string>();
            CASS_CompetencyFrameworkServices.UnPublishFramework( frameworkCTID, user, ref messages );

            //Return status
            var framework = CASS_CompetencyFrameworkServices.GetFrameworkByCTID( frameworkCTID );
            return JsonResponse( new StatusSummary( framework ), messages.Count() == 0, string.Join( ", ", messages ), null );
        }
        //
        public class StatusSummary
		{
			public StatusSummary()
			{
				LastApprovalDate = new DateTime();
				LastPublishDate = new DateTime();
				LastUpdatedDate = new DateTime();
			}
			public StatusSummary( string ctid, bool isApproved, bool isPublished, DateTime lastApprovalDate = default( DateTime ), DateTime lastPublishDate = default( DateTime ), DateTime lastUpdatedDate = default( DateTime ) ) : this()
			{
				var fakeMinDate = new DateTime( 2000, 1, 1 );
				CTID = ctid;
				IsApproved = isApproved;
				IsPublished = isPublished;
				LastApprovalDate = lastApprovalDate == fakeMinDate ? new DateTime() : lastApprovalDate;
				LastPublishDate = lastPublishDate == fakeMinDate ? new DateTime() : lastPublishDate;
				LastUpdatedDate = lastUpdatedDate == fakeMinDate ? new DateTime() : lastUpdatedDate;
			}
			public StatusSummary( CASS_CompetencyFramework framework ) : this()
			{
				var fakeMinDate = new DateTime( 2000, 1, 1 );
				CTID = framework.CTID;
				LastUpdatedDate = framework.LastUpdated;
				if ( framework == null || framework.Id == 0  )
				{
					//Do nothing
				}
				else
				{
					LastApprovalDate = framework.LastApproved;
					LastPublishDate = framework.LastPublished == fakeMinDate ? new DateTime() : framework.LastPublished != null ? framework.LastPublished : DateTime.MinValue;
					IsApproved = framework.IsApproved;
					IsPublished = framework.IsPublished;
				}
			}
			public string CTID { get; set; }
			public bool IsApproved { get; set; }
			public bool IsPublished { get; set; }
			public DateTime LastApprovalDate { get; set; }
			public string LastApprovalDateString { get { return LastApprovalDate.ToShortDateString(); } }
			public DateTime LastPublishDate { get; set; }
			public string LastPublishDateString { get { return LastPublishDate.ToShortDateString(); } }
			public DateTime LastUpdatedDate { get; set; }
			public string LastUpdatedDateString { get { return LastUpdatedDate.ToShortDateString(); } }
			public bool NeedsReapproval { get { return IsApproved && LastUpdatedDate > LastApprovalDate; } }
			public bool NeedsRepublishing { get { return IsPublished && LastUpdatedDate > LastPublishDate; } }
		}
		//
	}
}