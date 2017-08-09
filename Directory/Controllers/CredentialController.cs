using System.Linq;
using System.Web.Mvc;

using VMs = CTI.Directory.Models;
using CTIServices;
using Utilities;
using Models;
using Models.Common;
//using Models.Node;
//using Models.Node.Interface;


namespace CTI.Directory.Controllers
{
	public class CredentialController : BaseController
    {
		AppUser user = new AppUser();
		EnumerationServices enumServices = new EnumerationServices();

		string status = "";
		string notAuthMessage = "You are not authorized to view this page. <p>During the Beta period, only authorized people may view private data.</p>";
		SiteMessage msg = new SiteMessage() { Title = "<h2>Unauthorized Action</h2>", Message = "<p>You are not authorized to view this page.</p> <p>During the Beta period, only authorized people may view private data.</p>" };


        // GET: Credential
		public ActionResult Index( int id )
        {

			if ( User.Identity.IsAuthenticated )
				user = AccountServices.GetCurrentUser( User.Identity.Name );

			if ( !CredentialServices.CanUserUpdateCredential( id, user, ref status ) )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( notAuthMessage, "", false );

				Session[ "SystemMessage" ] = msg;

				return View( "Error" );
			}
			//HttpContext.Server.ScriptTimeout = 300;

			var vm = CredentialServices.GetCredentialDetail( id, user );

			if ( id > 0 && vm.Id == 0 )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested credential was not found", "", false );
				return View( "Error" );
			}

			if ( Request.Params[ "v2" ] == "true" )
			{
				return View( "~/Views/V2/Detail/Index.cshtml", vm );
			}

			return View( "~/Views/V2/DetailV3/Detail.cshtml", vm );
        }

		//Create or edit a Credential
		[Authorize]
		public ActionResult Add()
		{
			//if ( !AuthorizationCheck( "Credential", "Add", 0 ) )
			//{
			//	return RedirectToAction( "Index", "Message" );
			//}
			if ( AccountServices.AuthorizationCheck( "add", true, ref status, ref user ) == false )
			{
				ConsoleMessageHelper.SetConsoleInfoMessage( "ERROR - NOT AUTHENTICATED. You will not be able to add or update content", "", false );
				return RedirectToAction( "Index", "Home" );
			}
			VMs.CredentialViewModel vm = new VMs.CredentialViewModel();
			FillViewContent( vm );
			//var types = enumServices.GetCredentialType( EnumerationType.CUSTOM );
			//vm.CredentialTypes = types.Items.Select( v => new SelectListItem
			//{
			//	Text = v.Name.ToString(),
			//	Value = ( ( int ) v.CodeId ).ToString()
			//} ).ToList();

			
			return View(vm);
		}

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Add( VMs.CredentialViewModel model )
		{
			
			if ( AccountServices.AuthorizationCheck( "add", true, ref status, ref user ) == false )
			{
				ConsoleMessageHelper.SetConsoleInfoMessage( "ERROR - NOT AUTHENTICATED. You will not be able to add or update content", "", false );
				return RedirectToAction( "Index", "Message" );
			}

			CredentialServices mgr = new CredentialServices();
			//VMs.CredentialViewModel vm = new VMs.CredentialViewModel();

			if ( ModelState.IsValid )
			{
				bool valid = true;
				if ( model.EarningCredentialPrimaryMethodId == "1" )
				{
					//showing assessment
				}
				//no desc, so set not required
				//model.Credential.IsDescriptionRequired = false;

				model.Credential.Id = mgr.Add( model.Credential, user, ref valid, ref status );

				if ( !valid )
				{
					ConsoleMessageHelper.SetConsoleErrorMessage( status );
					FillViewContent( model );
					return View( model );
				}
				else
				{
					//direct to update view
					return RedirectToAction( "Edit", new { id = model.Credential.Id } );
				}
			}
			else
			{
				FillViewContent( model );
			}

			// If we got this far, something failed, redisplay form
			return View( model );
			
		}
		private void FillViewContent( VMs.CredentialViewModel vm )
		{
			var types = enumServices.GetCredentialType( EnumerationType.CUSTOM );
			vm.CredentialTypes = types.Items.Select( v => new SelectListItem
			{
				Text = v.Name.ToString(),
				Value = ( ( int ) v.CodeId ).ToString()
			} ).ToList();
		}
		/// <summary>
		/// Load the page to edit a credential
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[AcceptVerbs( HttpVerbs.Get )]
		public ActionResult Edit( int id )
		{
			//if ( !AuthorizationCheck( "Credential", "Edit", 0 ) )
			//{
			//	return RedirectToAction( "Index", "Message" );
			//}

			ViewBag.Message = "";
			//ViewBag.GetOrgs = true;
			//ViewBag.GetQAOrgs = true;

			VMs.CredentialViewModel vm = new VMs.CredentialViewModel();

			if ( AccountServices.AuthorizationCheck( "edit", true, ref status, ref user ) == false )
			{
				ConsoleMessageHelper.SetConsoleInfoMessage( "ERROR - NOT AUTHENTICATED. You will not be able to add or update content", "", false );
				return RedirectToAction( "Index", "Home" );
			}
			else
			{
				if ( id < 1 )
				{
					ConsoleMessageHelper.SetConsoleInfoMessage( "ERROR - Invalid identifier. A valid identifier is required for this request.", "", false );
					return RedirectToAction( "Index", "Home" );
				}
				else
				{
					//check if user can edit the entity
					if ( CredentialServices.CanUserUpdateCredential( id, user, ref status ) == false )
					{
						//todo - may want to redirect elsewhere
						ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - you do not have authorization to edit that credential", "", false );
						return RedirectToAction( "Index", "Home" );
					}
					else
					{
						vm.Credential = CredentialServices.GetForEdit( id );
						if ( vm.Credential.Id == 0 )
						{
							ConsoleMessageHelper.SetConsoleErrorMessage( "ERROR - the requested credential was not found", "", false );
						}
					}
				}
			}
			FillViewContent( vm );
			return View( "Edit", vm );
		}
    }
}