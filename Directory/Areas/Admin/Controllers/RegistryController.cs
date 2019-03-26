using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Models;
using Models.Common;
using CTIServices;
using Utilities;

namespace CTI.Directory.Areas.Admin.Controllers
{
    public class RegistryController : CTI.Directory.Controllers.BaseController
	{
        // GET: Admin/Registry
        public ActionResult Index()
        {
			return RedirectToAction( "Delete", "Registry" );
		}

		public ActionResult Delete()
		{
			if ( !AccountServices.IsUserAuthenticated() )
			{
				SetSystemMessage( "Unauthorized Action", "You must be logged in and authorized to perform this action." );

				return RedirectToAction( "Index", "Message", new { area = "" } );
			}
			else if ( !User.Identity.IsAuthenticated
				 || ( User.Identity.Name != "mparsons"
				 && User.Identity.Name != "mparsons@siuccwd.com"
				 && User.Identity.Name != "cwd-mparsons@ad.siu.edu"
				 && User.Identity.Name != "cwd-nathan.argo@ad.siu.edu" )
				 )
			{
				SetSystemMessage( "Unauthorized Action", "You are not authorized to perform this action." );

				return RedirectToAction( "Index", "Message", new { area = "" } );
			}

			SaveStatus status = new SaveStatus();

			return View( status );
		}

		/// <summary>
		/// this delete should only be used for data not in the current publisher.
		/// Perhaps should add a check!
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public ActionResult DoDelete( SaveStatus model )
		{
			if ( !AccountServices.IsUserAuthenticated() )
			{
				SetSystemMessage( "Unauthorized Action", "You must be logged in and authorized to perform this action." );

				return RedirectToAction( "Index", "Message" );
			}
			else if ( !User.Identity.IsAuthenticated
				 || ( User.Identity.Name != "mparsons"
				 && User.Identity.Name != "mparsons@siuccwd.com"
				 && User.Identity.Name != "cwd-mparsons@ad.siu.edu"
				 && User.Identity.Name != "cwd-nathan.argo@ad.siu.edu" )
				 )
			{
				SetSystemMessage( "Unauthorized Action", "You are not authorized to perform this action." );

				return RedirectToAction( "Index", "Message", new { area = "" } );
			}
			var user = AccountServices.GetCurrentUser();
			SaveStatus status = new SaveStatus();
			SiteMessage msg = new SiteMessage();
			if ( string.IsNullOrWhiteSpace( model.Ctid ) || model.Ctid.Length != 39 )
			{
				msg.Title = "ERROR - provide a valid CTID and envelopeId";
				msg.Message = "Provide both a valid CTID, and a valid registry envelope identifier must be provided.";
				Session[ "siteMessage" ] = msg;
				return View();

			}
			if ( string.IsNullOrWhiteSpace( model.EnvelopeId ) || model.EnvelopeId.Length != 36 )
			{
				msg.Title = "ERROR - provide a valid CTID and envelopeId";
				msg.Message = "Provide both a valid CTID, and a valid registry envelope identifier must be provided.";
				Session[ "siteMessage" ] = msg;
				return View();
			}
			string message = "";
			if (!new RegistryServices().CredentialRegistry_Delete( model.EnvelopeId, model.Ctid, user.FullName(), ref message ))
			{
				msg.Title = "ERROR encountered during delete attempt.";
				msg.Message = message;
				Session[ "siteMessage" ] = msg;
				status.AddError( message );
			}
			else 
			{
				status.Messages.Add( new StatusMessage() { Message = "Delete was successful", IsWarning = false } );
			}
			return View( "Delete", status );
		}

	}
}