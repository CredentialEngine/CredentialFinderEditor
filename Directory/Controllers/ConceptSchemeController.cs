using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json;
using System.Net.Http;

using ThisEntity = Models.Common.ConceptScheme;
using CTIServices;
using Utilities;

namespace CTI.Directory.Controllers
{
    public class ConceptSchemeController : BaseController
	{
        // GET: ConceptScheme
        public ActionResult Index()
        {
			return View( "~/Views/V2/ConceptScheme/Index.cshtml" );
		}
		public JsonResult CheckStatus( string ctid, string name, string url, string ownerOrganizationCTID )
		{
			//Get the data
			var user = AccountServices.GetUserFromSession();
			var record = ConceptSchemeServices.GetByCTID( ctid );
			var messages = new List<string>();
			if ( name == "New Concept Scheme" || name == "New Concept" )
			{
				//ignore as occurs on intial click of the add icon
				//if the name doesn't change, so be it
				return JsonResponse( new StatusSummary( new ThisEntity() ), true, string.Join( ", ", messages ), null );
			}
			//If there is no record record yet, create one
			if ( record == null || record.Id == 0 )
			{
				if ( string.IsNullOrWhiteSpace( ownerOrganizationCTID ) )
				{
					messages.Add( "Error - the organization CTID is null." );
					return JsonResponse( new StatusSummary( record ), record != null && record.Id > 0, "", new { statusList = messages } );
				}
				var ownerOrganization = OrganizationServices.GetByCtid( ownerOrganizationCTID );
				if ( ownerOrganization == null || ownerOrganization.Id == 0 )
				{
					messages.Add( "Error - the owing organization was not found." );
					return JsonResponse( new StatusSummary( record ), record != null && record.Id > 0, "", new { statusList = messages } );
				}
				var newFramework = new ThisEntity()
				{
					CTID = ctid,
					Name = name,
					CreatedById = user.Id,
					OrgId = ownerOrganization.Id,
					EditorUri = url 
				};
				var newFrameworkID = new ConceptSchemeServices().Add( newFramework, user, ref messages );
				record = ConceptSchemeServices.GetByID( newFrameworkID );
			}

			//Return status
			return JsonResponse( new StatusSummary( record ), record != null && record.Id > 0 && messages.Count() == 0, string.Join( ", ", messages ), null );
		}

		//
		public JsonResult MarkUpdated( string ctid, string name, string ownerOrganizationCTID )
		{
			//Get the data
			var user = AccountServices.GetUserFromSession();


			//Mark the update
			var messages = new List<string>();
			if ( name == "New Concept Scheme" )
			{
				//ignore as occurs on intial click of the add icon
				//if the name doesn't change, so be it
				return JsonResponse( new StatusSummary( new ThisEntity() ), true, string.Join( ", ", messages ), null );
			}

			new ConceptSchemeServices().MarkUpdated( ctid, name, user, ref messages );

			//Return status
			var record = ConceptSchemeServices.GetByCTID( ctid );
			return JsonResponse( new StatusSummary( record ), messages.Count() == 0, string.Join( ", ", messages ), null );
		}
		//
		//
		public JsonResult Approve( string ctid, string ownerOrganizationCTID )
		{
			//Get the data
			var user = AccountServices.GetUserFromSession();

			//Do the approval
			var messages = new List<string>();
			ConceptSchemeServices.Approve( ctid, user, ref messages );
			bool isValid = messages.Count() == 0;
			//Return status
			var record = ConceptSchemeServices.GetByCTID( ctid );
			return JsonResponse( new StatusSummary( record ), isValid, string.Join( ", ", messages ), null );
		}
		//

		public JsonResult Publish( string ctid, string frameworkExportJSON, string ownerOrganizationCTID )
		{
			//Get the data
			var user = AccountServices.GetUserFromSession();
			var messages = new List<string>();

			//Do the publish
			ConceptSchemeServices.Publish( ctid, frameworkExportJSON, user, ref messages );

			//Return status
			var record = ConceptSchemeServices.GetByCTID( ctid );
			return JsonResponse( new StatusSummary( record ), messages.Count() == 0, string.Join( ", ", messages ), null );
		}
		//
		public JsonResult UnPublish( string ctid, string ownerOrganizationCTID )
		{
			//Get the data
			var user = AccountServices.GetUserFromSession();

			//Do the publish
			var messages = new List<string>();
			ConceptSchemeServices.UnPublish( ctid, user, ref messages );

			//Return status
			var record = ConceptSchemeServices.GetByCTID( ctid );
			return JsonResponse( new StatusSummary( record ), messages.Count() == 0, string.Join( ", ", messages ), null );
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
			public StatusSummary( ThisEntity record ) : this()
			{
				var fakeMinDate = new DateTime( 2000, 1, 1 );
				CTID = record.CTID;
				LastUpdatedDate = record.LastUpdated;
				if ( record == null || record.Id == 0 )
				{
					//Do nothing
				}
				else
				{
					LastApprovalDate = record.LastApproved;
					LastPublishDate = record.LastPublished == fakeMinDate ? new DateTime() : record.LastPublished != null ? record.LastPublished : DateTime.MinValue;
					IsApproved = record.IsApproved;
					IsPublished = record.IsPublished;
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
	}
}