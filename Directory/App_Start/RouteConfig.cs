using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CTI.Directory
{
	public class RouteConfig
	{
		public static void RegisterRoutes( RouteCollection routes )
		{
			routes.IgnoreRoute( "{resource}.axd/{*pathInfo}" );

			routes.MapMvcAttributeRoutes();

			routes.MapRoute(
				name: "CredentialEdit",
				url: "credential/Edit/{id}",
				defaults: new { controller = "Credential", action = "Edit" }
			);

			routes.MapRoute(
				name: "Credentials",
				url: "credential/{id}/{name}",
				defaults: new { controller = "Detail", action = "Credential", name = UrlParameter.Optional }
			);

            routes.MapRoute(
				name: "Organizations",
				url: "organization/{id}/{name}",
				defaults: new { controller = "Detail", action = "Organization", name = UrlParameter.Optional }
			);

            routes.MapRoute(
				name: "Assessments",
				url: "assessment/{id}/{name}",
				defaults: new { controller = "Detail", action = "Assessment", name = UrlParameter.Optional }
			);

            routes.MapRoute(
                name: "AssessmentProfiles",
                url: "assessmentProfile/{id}/{name}",
                defaults: new { controller = "Detail", action = "Assessment", name = UrlParameter.Optional }
            );

            routes.MapRoute(
				name: "LearningOpps",
				url: "learningOpportunity/{id}/{name}",
				defaults: new { controller = "Detail", action = "LearningOpportunity", name = UrlParameter.Optional }
			);

            routes.MapRoute(
            name: "CompetencyRedirect",
            url: "Competency",
            defaults: new { controller = "Competencies", action = "Competency", name = UrlParameter.Optional }
        );
            routes.MapRoute(
			name: "userAdminHome",
			url: "admin/User/",
			defaults: new { controller = "User", action = "Index" }
			);

			routes.MapRoute(
				name: "userAdmin",
				url: "admin/User/{id}",
				defaults: new { controller = "User", action = "Edit" }
			);

			routes.MapRoute(
				name: "activityAdminHome",
				url: "admin/Activity",
				defaults: new { controller = "Activity", action = "Index" }
			);

            routes.MapRoute(
                name: "Pages",
                url: "page/{page}",
                defaults: new { controller = "Page", action = "Page" }
            );

            routes.MapRoute(
					name: "Default",
					url: "{controller}/{action}/{id}",
					defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			);

		}
	}
}
