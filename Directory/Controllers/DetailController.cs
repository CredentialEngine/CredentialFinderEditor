using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CTIServices;
using Models;
using Models.Common;
using Models.ProfileModels;
using Utilities;


namespace CTI.Directory.Controllers
{
	public class DetailController : BaseController
    {
		AppUser user = AccountServices.GetCurrentUser();
		string status = "";
		//string notAuthMessage = "You are not authorized to view this page. <p>During the Beta period, only authorized people may view private data.</p>";
		SiteMessage msg = new SiteMessage() { Title = "<h2>Unauthorized Action</h2>", Message = "<p>You are not authorized to view this page.</p> <p>Only authorized people may view private data.</p>" };
		SiteMessage notFoundMsg = new SiteMessage() { Title = "<h2>Not Found</h2>", Message = "<p>The requested record does not exist. Please use the search to locate the correct record.</p>" };

		public ActionResult Print()
		{
			return View( "~/Views/V2/DetailV4/PrintDetail.cshtml" );
		}

		public ActionResult Credential( string id, string name = "" )
		{
			//if ( User.Identity.IsAuthenticated )
			//	user = AccountServices.GetCurrentUser( User.Identity.Name );

			//Credential entity = new Credential();
			if ( !AccountServices.IsUserAuthenticated() )
			{
				string nextUrl = string.Format( "/credential/{0}/{0}",  id, name );
				return RedirectToAction( "CE_LoginRedirect", "Account", new { nextUrl = nextUrl } );
			}
			//HttpContext.Server.ScriptTimeout = 300;
			string refresh = Request.Params[ "refresh" ];
			bool skippingCache = FormHelper.GetRequestKeyValue( "skipCache", false );
            List<string> messages = new List<string>();;
            int credId = 0;
            var entity = new Credential();
            if ( int.TryParse( id, out credId ) )
            {
                if ( !CredentialServices.CanUserViewCredential( credId, user, ref entity ) )
                {
                    if ( entity.Id > 0 )
                        Session[ "SystemMessage" ] = msg;
                    else
                        Session[ "SystemMessage" ] = notFoundMsg;
                    return RedirectToAction( "Index", "Message" );
                }

                entity = CredentialServices.GetCredentialDetail( credId, user, skippingCache );
            }
            else if ( ServiceHelper.IsValidCtid( id, ref messages ) )
            {
                entity = CredentialServices.GetDetailByCtid( id,user, skippingCache );
            }

            if ( entity.Id == 0 )
			{
				SetPopupErrorMessage( "ERROR - A valid Credential identifier was not provided." );
				return RedirectToAction( "Index", "Home" );
			}

			if ( entity.IsReferenceVersion )
			{
				Session[ "siteMessage" ] = "The selected record is a Reference only, there is no detail to display";
				return RedirectToAction( "Index", "Message" );
			}
            new ActivityServices().AddActivity( new SiteActivity()
            {
                ActivityType = "Credential",
                Activity = "Detail",
                Event = "View",
                Comment = string.Format( "User viewed Credential: {0} ({1})", entity.Name, entity.Id ),
                ActivityObjectId = credId,
                ActionByUserId = user.Id,
                ActivityObjectParentEntityUid = entity.RowId
            } );

            return View( "~/Views/V2/DetailV4/Detail.cshtml", entity );

		}
		//

		public ActionResult Organization( string id, string name = "" )
		{
			//if ( !User.Identity.IsAuthenticated )
			//	user = AccountServices.GetCurrentUser( User.Identity.Name );

			//Organization entity = new Organization();
            string refresh = Request.Params[ "refresh" ];
            bool skippingCache = FormHelper.GetRequestKeyValue( "skipCache", false );
            List<string> messages = new List<string>();;
            int orgId = 0;
            var entity = new Organization();

            if ( int.TryParse( id, out orgId ) )
            {
                if ( !OrganizationServices.CanUserViewOrganization( orgId, user, ref entity ) )
                {
                    if ( entity.Id > 0 )
                        Session[ "SystemMessage" ] = msg;
                    else
                        Session[ "SystemMessage" ] = notFoundMsg;
                    return RedirectToAction( "Index", "Message" );
                }

                entity = OrganizationServices.GetOrganizationDetail( orgId, user, skippingCache );
            }
            else if ( ServiceHelper.IsValidCtid( id, ref messages ) )
            {
                entity = OrganizationServices.GetDetailByCtid( id, user, skippingCache );
            }

            if ( entity.Id == 0 )
            {
                SetPopupErrorMessage( "Error: A valid Organization identifier was not provided." );
                return RedirectToAction( "Index", "Home" );
            }

            if ( entity.IsReferenceVersion )
            {
                Session[ "siteMessage" ] = "The selected record is a Reference only, there is no detail to display";
                return RedirectToAction( "Index", "Message" );
            }
            new ActivityServices().AddActivity( new SiteActivity()
            {
                ActivityType = "Organization",
                Activity = "Detail",
                Event = "View",
                Comment = string.Format( "User viewed Organization: {0} ({1})", entity.Name, entity.Id ),
                ActivityObjectId = orgId,
                ActionByUserId = user.Id,
                ActivityObjectParentEntityUid = entity.RowId
            } );

            return View( "~/Views/V2/DetailV4/Detail.cshtml", entity );

        }
        //
        public ActionResult QAOrganization(string id, string name = "" )
		{
			return Organization( id, name );
		
		}
        //
        public ActionResult AssessmentProfile( string id, string name = "" )
        {
            return Assessment( id, name );

        }
        public ActionResult Assessment( string id, string name = "" )
		{
            //if ( User.Identity.IsAuthenticated )
            //	user = AccountServices.GetCurrentUser( User.Identity.Name );
            AssessmentProfile entity = new AssessmentProfile();
            string refresh = Request.Params[ "refresh" ];
            bool skippingCache = FormHelper.GetRequestKeyValue( "skipCache", false );
            List<string> messages = new List<string>();;
            int assmid = 0;
           

            if ( int.TryParse( id, out assmid ) )
            {
                entity = AssessmentServices.GetDetail( assmid, user );
                if ( !AssessmentServices.CanUserViewAssessment( entity, user, ref status ) )               {
                    if ( entity.Id > 0 )
                        Session[ "SystemMessage" ] = msg;
                    else
                        Session[ "SystemMessage" ] = notFoundMsg;
                    return RedirectToAction( "Index", "Message" );
                }               
            }
            else if ( ServiceHelper.IsValidCtid( id, ref messages ) )
            {
                entity = AssessmentServices.GetDetailByCtid( id, user );               
            }

            if ( entity.Id == 0 )
            {
                SetPopupErrorMessage( "Error: A valid Assessment identifier was not provided." );
                return RedirectToAction( "Index", "Home" );
            }

            if ( entity.IsReferenceVersion )
            {
                Session[ "siteMessage" ] = "The selected record is a Reference only, there is no detail to display";
                return RedirectToAction( "Index", "Message" );
            }
            new ActivityServices().AddActivity( new SiteActivity()
            {
                ActivityType = SiteActivity.AssessmentType,
                Activity = "Detail",
                Event = "View",
                Comment = string.Format( "User viewed Assessment: {0} ({1})", entity.Name, entity.Id ),
                ActivityObjectId = assmid,
                ActionByUserId = user.Id,
                ActivityObjectParentEntityUid = entity.RowId
            } );

            return View( "~/Views/V2/DetailV4/Detail.cshtml", entity );

            
		}
		//

		public ActionResult LearningOpportunity( string id, string name = "" )
		{
            //if ( User.Identity.IsAuthenticated )
            //	user = AccountServices.GetCurrentUser( User.Identity.Name );
            LearningOpportunityProfile entity = new LearningOpportunityProfile();
            string refresh = Request.Params[ "refresh" ];
            bool skippingCache = FormHelper.GetRequestKeyValue( "skipCache", false );
            List<string> messages = new List<string>();;
            int loppid = 0;

            if ( int.TryParse( id, out loppid ) )
            {
                entity = LearningOpportunityServices.GetForDetail( loppid, user );
                if ( !LearningOpportunityServices.CanUserViewLearningOpportunity(  entity, user, ref status ) )
                {
                    if ( entity.Id > 0 )
                        Session[ "SystemMessage" ] = msg;
                    else
                        Session[ "SystemMessage" ] = notFoundMsg;
                    return RedirectToAction( "Index", "Message" );
                }
            }
            else if ( ServiceHelper.IsValidCtid( id, ref messages ) )
            {
                entity = LearningOpportunityServices.GetDetailByCtid( id, user );
            }
            if ( loppid > 0 && entity.Id == 0 )
            {
                SetPopupErrorMessage( "Error: A valid Learning Opportunity identifier was not provided." );
                return RedirectToAction( "Index", "Home" );
            }
            if ( entity.IsReferenceVersion )
            {
                Session[ "siteMessage" ] = "The selected record is a Reference only, there is no detail to display";
                return RedirectToAction( "Index", "Message" );
            }
            new ActivityServices().AddActivity( new SiteActivity()
            {
                ActivityType = SiteActivity.AssessmentType,
                Activity = "Detail",
                Event = "View",
                Comment = string.Format( "User viewed LearningOpportunity: {0} ({1})", entity.Name, entity.Id ),
                ActivityObjectId = loppid,
                ActionByUserId = user.Id,
                ActivityObjectParentEntityUid = entity.RowId
            } );
            return View( "~/Views/V2/DetailV4/Detail.cshtml", entity );

           
		}
		//

	}
}