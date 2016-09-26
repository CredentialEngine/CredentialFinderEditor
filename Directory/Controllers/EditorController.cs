using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CTIServices;
using Utilities;
using Models;
using Models.Node;
using Models.Node.Interface;
using Newtonsoft.Json;

using PM = Models.ProfileModels;

namespace CTI.Directory.Controllers
{
	public class EditorController : Controller
	{
		bool valid = true;
		AppUser user = new AppUser();
		string status = "";

		//Create or edit a Credential
		[Authorize]
		public ActionResult Credential( int id = 0 )
		{
			return LoadEditor( typeof( Models.Node.Credential ), id, EditorSettings.EditorType.CREDENTIAL );
		}
		//

		//Create or edit an Organization
		[Authorize]
		public ActionResult Organization( int id = 0 )
		{
			return LoadEditor( typeof( Models.Node.Organization ), id, EditorSettings.EditorType.ORGANIZATION );
		}
		//

		//Create or edit an Assessment
		[Authorize]
		public ActionResult Assessment( int id = 0 )
		{
			return LoadEditor( typeof( Models.Node.Assessment ), id, EditorSettings.EditorType.ASSESSMENT );
		}
		//

		//Create or edit a Learning Opportunity
		[Authorize]
		public ActionResult LearningOpportunity( int id = 0 )
		{
			return LoadEditor( typeof( Models.Node.LearningOpportunity ), id, EditorSettings.EditorType.LEARNINGOPPORTUNITY );
		}
		//

		//Standardized way of loading an editor
		private ActionResult LoadEditor( Type profileType, int profileID, EditorSettings.EditorType editorType )
		{
			if ( !AuthorizationCheck( editorType, profileID ) )
			{
				return RedirectToAction( "Index", "Message" );
			}

			var context = new ProfileContext()
			{
				IsTopLevel = true,
				Profile = new ProfileLink()
				{
					Id = profileID,
					Type = profileType,
				}
			};

			var data = EditorServices.GetProfile( context, false, ref valid, ref status );

			var settings = new EditorSettings()
			{
				MainProfile = new Models.Node.ProfileLink()
				{
					Id = profileID,
					Type = profileType,
					Name = data.Name,
					RowId = data.RowId
				},
				Editor = editorType,
				Data = data
			};

			return View( "~/Views/Editor/Editor.cshtml", settings );
		}
		//

		//Standardized authorization/authentication checks
		private bool AuthorizationCheck( EditorSettings.EditorType type, int profileID )
		{
			//Initial data
			var canEdit = true;
			var name = "";

			//Check to see if the user has any edit permissions
			if ( AccountServices.AuthorizationCheck( "edit", true, ref status, ref user ) == false )
			{
				ConsoleMessageHelper.SetConsoleInfoMessage( "ERROR - NOT AUTHENTICATED. You will not be able to add or update content", "", false );
				Session[ "siteMessage" ] = "ERROR - NOT AUTHENTICATED. You will not be able to add or update content";
				canEdit = false;
			}

			//If the target is a new item, just return true/false depending on whether or not user can edit content
			if ( profileID == 0 )
			{
				return canEdit;
			}

			//Otherwise, if the item exists, check to see if the user can edit it specifically
			switch ( type )
			{
				case EditorSettings.EditorType.CREDENTIAL:
					canEdit = CredentialServices.CanUserUpdateCredential( profileID, user, ref status );
					name = "credential";
					break;
				case EditorSettings.EditorType.ORGANIZATION:
					canEdit = OrganizationServices.CanUserUpdateOrganization( user, profileID );
					name = "organization";
					break;
				case EditorSettings.EditorType.ASSESSMENT:
					canEdit = AssessmentServices.CanUserUpdateAssessment( profileID, user, ref status );
					name = "assessment";
					break;
				case EditorSettings.EditorType.LEARNINGOPPORTUNITY:
					canEdit = LearningOpportunityServices.CanUserUpdateLearningOpportunity( profileID, user, ref status );
					AccountServices.EditCheck( ref canEdit, ref status );
					name = "learning opportunity";
					break;
				default:
					break;
			}

			//If the user can't edit, set a standardized message
			if ( !canEdit )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - you do not have authorization to edit this " + name, "", false );
				Session[ "siteMessage" ] = "ERROR - you do not have authorization to edit this " + name;
			}

			//Return the result of all the checks
			return canEdit;
		}

		#region Profile-based operations

		//Get a Profile
		public JsonResult GetProfile( ProfileContext context )
		{
			//Get the clientProfile based on the provided context
			var data = EditorServices.GetProfile( context, false, ref valid, ref status );

			//Return the data
			return JsonHelper.GetJsonWithWrapper( data, valid, status, null );
		}
		//

		//Save a Profile
		public JsonResult SaveProfile( ProfileContext context, string profile )
		{
			if ( AccountServices.AuthorizationCheck( "updates", true, ref status, ref user ) == false )
			{
				return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}
			if ( !User.Identity.IsAuthenticated )
			{

			}
			//Cast the clientProfile based on the provided context
			var data = JsonConvert.DeserializeObject( profile, context.Profile.Type, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore } );

			//Add or update the data
			var result = EditorServices.SaveProfile( context, ( BaseProfile ) data, ref valid, ref status );

			//Return the result
			if ( result == null )
			{
				valid = false;
			}
			ProfileLink link = null;
			if ( valid && result != null )
			{
				link = new ProfileLink() { Id = result.Id, RowId = result.RowId, Name = result.Name, Type = context.Profile.Type };
			}
			return JsonHelper.GetJsonWithWrapper( result, valid, status, link );
		}
		//

		//Delete a Profile
		public JsonResult DeleteProfile( ProfileContext context )
		{
			//Check permission
			if ( AccountServices.AuthorizationCheck( "delete", true, ref status, ref user ) == false )
			{
				return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}

			//Delete the data
			EditorServices.DeleteProfile( context, ref valid, ref status );

			//Return the result
			return JsonHelper.GetJsonWithWrapper( null, valid, status, null );
		}
		//
		/// <summary>
		/// Register/update an object in the registry
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public JsonResult Register( ProfileContext context )
		{
			//Check permission
			if ( AccountServices.AuthorizationCheck( "publish", true, ref status, ref user ) == false )
			{
				return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}

			//publish to the registry
			EditorServices.RegisterEntity( context, ref valid, ref status );

			//Return the result
			return JsonHelper.GetJsonWithWrapper( null, valid, status, null );
		}

		/// <summary>
		/// Remove an object from the registry
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public JsonResult Unregister( ProfileContext context )
		{
			//Check permission
			if ( AccountServices.AuthorizationCheck( "publish", true, ref status, ref user ) == false )
			{
				return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}

			//publish to the registry
			EditorServices.UnregisterEntity( context, ref valid, ref status );

			//Return the result
			return JsonHelper.GetJsonWithWrapper( null, valid, status, null );
		}
		#endregion

		#region MicroSearchV2 methods

		//Get existing/saved MicroSearch results for initial display
		public JsonResult GetMicroProfiles( string searchType, List<ProfileLink> items )
		{
			var data = MicroSearchServicesV2.GetMicroProfiles( searchType, items, ref valid, ref status );

			return JsonHelper.GetJsonWithWrapper( data, valid, status, null );
		}
		//

		//Do a MicroSearch
		public JsonResult DoMicroSearch( Models.Search.MicroSearchInputV2 query )
		{
			var totalResults = 0;
			var data = MicroSearchServicesV2.DoMicroSearch( query, ref totalResults, ref valid, ref status );

			return JsonHelper.GetJsonWithWrapper( data, valid, status, totalResults );
		}
		//

		//Add/Save a MicroProfile
		public JsonResult SaveMicroProfile( ProfileContext context, Dictionary<string, object> selectors, string searchType, string property )
		{
			var data = MicroSearchServicesV2.SaveMicroProfile( context, selectors, searchType, property, ref valid, ref status );

			return JsonHelper.GetJsonWithWrapper( data, valid, status, null );
		}
		//

		//Delete a MicroProfile
		public JsonResult DeleteMicroProfile( ProfileContext context, Dictionary<string, object> selectors, string searchType, string property )
		{
			MicroSearchServicesV2.DeleteMicroProfile( context, selectors, searchType, property, ref valid, ref status );

			return JsonHelper.GetJsonWithWrapper( null, valid, status, null );
		}
		//

		//Create a Starter Profile
		public JsonResult SaveStarterProfile( ProfileContext context, StarterProfile profile )
		{
			context.Profile.TypeName = profile.ProfileType;
			var data = EditorServices.SaveProfile( context, ( BaseProfile ) profile, ref valid, ref status );

			//Update the context to work with the MicroSearch methods
			var selectors = new Dictionary<string, object>() 
			{
				{ "Id", data.Id },
				{ "Name", data.Name },
				{ "RowId", data.RowId.ToString() },
				{ "TypeName", profile.ProfileType }
			};
			context.Profile = context.Parent;
			var result = MicroSearchServicesV2.SaveMicroProfile( context, selectors, profile.SearchType, context.Profile.Property, ref valid, ref status );

			return JsonHelper.GetJsonWithWrapper( result, valid, status, null );
		}
		//

		#endregion

	}
}