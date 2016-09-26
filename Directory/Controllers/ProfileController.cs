using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Models;
using Models.Common;
using Models.ProfileModels;
using Utilities;
using CTIServices;

/* Nothing in here should be used anymore - but the methods might be useful for references later */
namespace CTI.Directory.Controllers
{
	public class ProfileController : Controller
	{
		AppUser user = new AppUser();
		string status = "";
		bool valid = true;
		List<string> messages = new List<string>();

		#region Assessment
		// Test method
		public JsonResult Echo_Assessment( AssessmentProfile input )
		{
			return JsonHelper.GetJsonWithWrapper( input, true, "okay", null );
		}

		//View all assessments
		public ActionResult Assessments( string keyword = "" )
		{
			int totalRows = 0;
			var vm = AssessmentServices.Search( keyword, 1, 100, ref totalRows );

			return View( "~/Views/Profile/Assessments.cshtml", vm );
		}
		//

		// view assessment
		public ActionResult Assessment( int id = 0 )
		{
			if ( id == 0 )
			{
				RedirectToAction( "Index" );
			}
			AssessmentProfile vm = AssessmentServices.Get( id, false );

			if ( vm.Id == 0 && id > 0 )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested Assessment was not found", "", false );
			}

			return View( "~/Views/Profile/Assessment.cshtml", vm );
		}
		//
		// Edit assessment
		public ActionResult AssessmentEdit( int id = 0 )
		{
			AssessmentProfile vm = new AssessmentProfile();

			if ( id > 0 )
			{
				if ( AssessmentServices.CanUserUpdateAssessment( id, ref status ) )
				{
					vm = AssessmentServices.GetForEdit( id, false );

					if ( vm.Id == 0 && id > 0 )
					{
						ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested Assessment was not found", "", false );
					}
				}
				else
				{
					//todo - may want to redirect elsewhere
					ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - you do not have authorization to edit that assessment", "", false );
					return RedirectToAction( "Index", "Home" );
				}
			}

			return View( "~/Views/Profile/AssessmentEditor.cshtml", vm );
		}
		//
		//Update an assessment
		[AcceptVerbs( HttpVerbs.Post )]
		public JsonResult AssessmentUpdate( AssessmentProfile input )
		{
			ViewBag.Message = "";
			ViewBag.GetOrgs = false;
			string returnMessage = "okay";
			AssessmentServices mgr = new AssessmentServices();
			//do auth checks and get current user
			if ( AccountServices.AuthorizationCheck( "updates", true, ref status, ref user ) == false )
			{
				return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}

			int id = 0;
			
			if ( input.Id > 0 )
			{
				input.LastUpdatedById = user.Id;
				if ( mgr.Update( input, user, ref status ) == false )
					return JsonHelper.GetJsonWithWrapper( null, false, status, null );
				else
				{
					if ( status.Length > 4 )
						returnMessage = status;
				}
			}
			else
			{
				input.LastUpdatedById = user.Id;
				input.CreatedById = user.Id;
				id = mgr.Add( input, user, ref status );
				if ( id > 0 )
				{
					input.Id = id;
					if ( status.Length > 4 )
						returnMessage = status;
				}
				else
					return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}


			var output = AssessmentServices.GetForEdit( input.Id, false );
			return JsonHelper.GetJsonWithWrapper( output, true, returnMessage, null );
		}
		//
		[AcceptVerbs( HttpVerbs.Post )]
		public JsonResult DeleteAssessment( int id )
		{
			if ( AccountServices.AuthorizationCheck( "delete", true, ref status ) == false )
			{
				return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}

			var valid = true;
			if ( id > 0 )
			{
				new AssessmentServices().Delete( id, user.Id, ref valid, ref status );
				if ( !valid )
				{
					return JsonHelper.GetJsonWithWrapper( null, false, "Error deleting assessment: " + status, null );
				}
			}
			else
			{
				return JsonHelper.GetJsonWithWrapper( null, false, "You must select an assessment to delete.", null );
			}

			return JsonHelper.GetJsonWithWrapper( null, true, "Deleted successfully", null );
		}
		//
		[AcceptVerbs( HttpVerbs.Post )]
		public JsonResult DeleteProfile( int id, string profile )
		{
			if ( AccountServices.AuthorizationCheck( "delete", true, ref status ) == false )
			{
				return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}

			var valid = true;


			if ( id > 0 && string.IsNullOrWhiteSpace( profile ) == false )
			{

				new AssessmentServices().DeleteProfile( id, profile, user, ref valid, ref status );
				if ( !valid )
				{
					return JsonHelper.GetJsonWithWrapper( null, false, "Error deleting record: " + status, null );
				}
			}
			else
			{
				return JsonHelper.GetJsonWithWrapper( null, false, "You must select an item to delete.", null );
			}

			return JsonHelper.GetJsonWithWrapper( null, true, "Deleted successfully", null );
		}
		//
		#endregion

		#region LearningOpportunity
		// Test method
		public JsonResult Echo_LearningOpportunity( LearningOpportunityProfile input )
		{
			return JsonHelper.GetJsonWithWrapper( input, true, "okay", null );
		}

		//View Learning Opportunities
		public ActionResult LearningOpportunities( string keyword = "" )
		{

			int totalRows = 0;
			var vm = LearningOpportunityServices.Search( keyword, 1, 100, ref totalRows );

			return View( "~/Views/Profile/LearningOpportunities.cshtml", vm );
		}
		//
		//View Learning Opportunity
		public ActionResult LearningOpportunity( int id = 0 )
		{
			if ( id == 0 )
			{
				RedirectToAction( "Index" );
			}
			var vm = LearningOpportunityServices.GetForEdit( id );

			if ( vm.Id == 0 && id > 0 )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested Learning Opportunity was not found", "", false );
			}

			return View( "~/Views/Profile/LearningOpportunity.cshtml", vm );
		}
		//
		//Edit Learning Opportunity
		public ActionResult LearningOpportunityEdit ( int id = 0 )
		{

			var vm = new LearningOpportunityProfile();
			AccountServices.EditCheck( ref valid, ref status );
			if ( valid )
			{
				if ( LearningOpportunityServices.CanUserUpdateLearningOpportunity( id, ref status ) )
				{
					vm = LearningOpportunityServices.GetForEdit( id );

					if ( vm.Id == 0 && id > 0 )
					{
						ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested Learning Opportunity was not found", "", false );
					}
				}
				else
				{
					//todo - may want to redirect elsewhere
					ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - you do not have authorization to edit that assessment", "", false );
					return RedirectToAction( "Index", "Home" );
				}
				
				
			}
			return View( "~/Views/Profile/LearningOpportunityEditor.cshtml", vm );
		}
		//

		//Update Learning Opportunity
		[AcceptVerbs( HttpVerbs.Post )]
		public JsonResult LearningOpportunityUpdate( LearningOpportunityProfile input, string section )
		{
			ViewBag.Message = "";
			ViewBag.GetOrgs = false;
			string returnMessage = "okay";
			LearningOpportunityServices mgr = new LearningOpportunityServices();
			if ( AccountServices.AuthorizationCheck( "updates", true, ref status, ref user ) == false )
			{
				return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}

			int id = 0;

			if ( input.Id > 0 )
			{
				input.LastUpdatedById = user.Id;
				if ( mgr.Update( input, section, user, ref status ) == false )
					return JsonHelper.GetJsonWithWrapper( null, false, status, null );
				else
				{
					if ( status.Length > 4 )
						returnMessage = status;
				}
			}
			else
			{
				input.LastUpdatedById = user.Id;
				input.CreatedById = user.Id;
				id = mgr.Add( input, user, ref status );
				if ( id > 0 )
				{
					input.Id = id;
					if ( status.Length > 4 )
						returnMessage = status;
				}
				else
					return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}


			var output = LearningOpportunityServices.GetForEdit( input.Id );
			return JsonHelper.GetJsonWithWrapper( output, true, returnMessage, null );
		}
		//

		//
		[AcceptVerbs( HttpVerbs.Post )]
		public JsonResult DeleteLearningOpportunity( int id )
		{
			if ( AccountServices.AuthorizationCheck( "delete", true, ref status ) == false )
			{
				return JsonHelper.GetJsonWithWrapper( null, false, status, null );
			}

			var valid = true;
			if ( id > 0 )
			{
				new LearningOpportunityServices().Delete( id, user.Id, ref valid, ref status );
				if ( !valid )
				{
					return JsonHelper.GetJsonWithWrapper( null, false, "Error deleting Learning Opportunity: " + status, null );
				}
			}
			else
			{
				return JsonHelper.GetJsonWithWrapper( null, false, "You must select a Learning Opportunity to delete.", null );
			}

			return JsonHelper.GetJsonWithWrapper( null, true, "Deleted successfully", null );
		}

		#endregion

	}
}