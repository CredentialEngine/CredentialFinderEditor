using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CTIServices;
using Models.Helpers;

namespace CTI.Directory.Controllers
{
    public class NotificationsController : BaseController
    {
        // GET: Notifications
		[Route("notifications")]
        public ActionResult Notifications()
        {
			//Page to show notifications for a user
			return View( "~/views/v2/notifications/index.cshtml" );
        }
		//

		[Route("notifications/search")]
		public JsonResult Search( NotificationQuery query )
		{
			//Forcibly limit the query to just emails for the current user if the current user is not an admin
			var user = AccountServices.GetUserFromSession();
			if ( !AccountServices.IsUserAnAdmin() )
			{
				query.ForAccountRowId = user.RowId;
			}

			//Do the search
			var totalResults = 0;
			//query.PageSize = -1; //Don't do paging for now
			//query.PageNumber = 1;
			var results = NotificationServices.Search( query, ref totalResults );

			return JsonResponse(results, true, "okay", new { TotalResults = totalResults });
		}
		//

	}
}