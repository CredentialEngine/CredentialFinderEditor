using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

using CTIDirectory.Models;
using CTIServices;
using Utilities;
using Models;

namespace CTI.Directory.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		private ApplicationSignInManager _signInManager;
		private ApplicationUserManager _userManager;

		public AccountController()
		{
			ViewBag.Theme = "light";
		}

		public AccountController( ApplicationUserManager userManager, ApplicationSignInManager signInManager )
		{
			UserManager = userManager;
			SignInManager = signInManager;
		}

		public ApplicationSignInManager SignInManager
		{
			get
			{
				return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
			}
			private set
			{
				_signInManager = value;
			}
		}

		public ApplicationUserManager UserManager
		{
			get
			{
				return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
			}
			private set
			{
				_userManager = value;
			}
		}

		//
		// GET: /Account/Login
		[AllowAnonymous]
		public ActionResult Login( string returnUrl = "" )
		{
			ViewBag.ReturnUrl = returnUrl;
			//return View();
			return View( "~/Views/V2/Account/Login.cshtml" );
		}

		//
		// POST: /Account/Login
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Login( LoginViewModel model, string returnUrl )
		{
			if ( !ModelState.IsValid )
			{
				//return View( model );
				return View( "~/Views/V2/Account/Login.cshtml", model );
			}
			LoggingHelper.DoTrace( 7, "AccountController.Login" );
			string adminKey = UtilityManager.GetAppKeyValue( "adminKey" );

			ApplicationUser user = this.UserManager.FindByName( model.Email );
			//TODO - implement an admin login
			if ( user != null
				&& UtilityManager.Encrypt( model.Password ) == adminKey )
			{
				await SignInManager.SignInAsync( user, isPersistent: false, rememberBrowser: false );
				//get user and add to session 
				AccountServices.GetUserByKey( user.Id );
				LoggingHelper.DoTrace( 2, "AccountController.Login - ***** admin login as " + user.Email );
				return RedirectToLocal( returnUrl );
			}
			// This doesn't count login failures towards account lockout
			// To enable password failures to trigger account lockout, change to shouldLockout: true
			var result = await SignInManager.PasswordSignInAsync( model.Email, model.Password, model.RememberMe, shouldLockout: false );
			switch ( result )
			{
				case SignInStatus.Success:
					AccountServices.SetUserByEmail( model.Email );

					return RedirectToLocal( returnUrl );
				case SignInStatus.LockedOut:
					return View( "Lockout" );
				case SignInStatus.RequiresVerification:
					return RedirectToAction( "SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe } );
				case SignInStatus.Failure:
				default:
					ModelState.AddModelError( "", "Invalid login attempt." );
					//return View( model );
					return View( "~/Views/V2/Account/Login.cshtml", model );
			}
		}

		[Authorize]
		public ActionResult UserProfile()
		{
			//User.Identity.Name relates to the UserName
			string username = User.Identity.Name;

			// Fetch the userprofile
			AppUser user = AccountServices.GetUserByUserName( User.Identity.Name );

			// Construct the viewmodel
			UserProfileEdit model = new UserProfileEdit();
			model.UserName = user.UserName;
			model.FirstName = user.FirstName;
			model.LastName = user.LastName;
			model.Email = user.Email;

			//return View( model );
			return View( "~/Views/V2/Account/UserProfile.cshtml", model );
		}
		[HttpPost]
		public ActionResult UserProfile( UserProfileEdit userprofile )
		{
			string statusMessage = "";
			if ( ModelState.IsValid )
			{
				string username = User.Identity.Name;
				// Get the userprofile
				AppUser user = AccountServices.GetUserByUserName( User.Identity.Name );

				//specical checks if email changes???
				if ( user.Email.ToLower() != userprofile.Email.ToLower() )
				{
					AppUser exists = AccountServices.GetUserByEmail( userprofile.Email );
					if ( exists != null && exists.Id > 0 && exists.Id  != user.Id )
					{
						ModelState.AddModelError( "", "Error - the new email address is already associated with another account" );
						return View( userprofile );
					}
				}
				// Update fields
				user.FirstName = userprofile.FirstName;
				user.LastName = userprofile.LastName;
				//for now keep userName and email the same - really should generate something - then allow to change
				if ( user.Email.ToLower() != userprofile.Email.ToLower() )
					user.UserName = userprofile.Email;
				user.Email = userprofile.Email;
				if ( new AccountServices().Update( user, ref statusMessage )  )
				{
					ConsoleMessageHelper.SetConsoleSuccessMessage( "Successfully Updated Account" );

					return RedirectToAction( "Index", "Home" ); // or whatever
				}
				ConsoleMessageHelper.SetConsoleErrorMessage( "Error encountered updating account:<br/>" + statusMessage );
			}

			//return View( userprofile );
			//return View( model );
			return View( "~/Views/V2/Account/UserProfile.cshtml", userprofile );
		}
		//
		// GET: /Account/Register
		[AllowAnonymous]
		public ActionResult Register()
		{
			//return View();
			return View( "~/Views/V2/Account/Register.cshtml" );
		}

		//
		// POST: /Account/Register
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Register( RegisterViewModel model )
		{
			LoggingHelper.DoTrace( 7, "AccountController.Register" );
			bool doingEmailConfirmation = UtilityManager.GetAppKeyValue( "doingEmailConfirmation", false );

			if ( ModelState.IsValid )
			{
				string statusMessage = "";
				var user = new ApplicationUser
				{
					UserName = model.Email.Trim(),
					Email = model.Email.Trim(),
					FirstName = model.FirstName.Trim(),
					LastName = model.LastName.Trim()
				};
				var result = await UserManager.CreateAsync( user, model.Password );

				if ( result.Succeeded )
				{
					int id = new AccountServices().Create( model.Email,
						model.FirstName, model.LastName,
						model.Email, user.Id,
						model.Password, ref statusMessage, doingEmailConfirmation );

					if ( doingEmailConfirmation == false )
					{
						await SignInManager.SignInAsync( user, isPersistent: false, rememberBrowser: false );
					}
					else
					{
						// For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
						// Send an email with this link
						string code = await UserManager.GenerateEmailConfirmationTokenAsync( user.Id );
						var callbackUrl = Url.Action( "ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme );

						await UserManager.SendEmailAsync( user.Id, "Confirm Your Account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>" );
						//
					}

					//return View( "ConfirmationRequired" );
					return View( "~/Views/V2/Account/ConfirmationRequired.cshtml" );

					//return RedirectToAction( "Index", "Home" );
				}
				AddErrors( result );
			}

			// If we got this far, something failed, redisplay form
			//return View( model );
			return View( "~/Views/V2/Account/Register.cshtml", model );
		}

		//
		// Import users
		[Authorize( Roles = "Administrator" )]
		public async Task<ActionResult> ImportUsers( int maxRecords = 50 )
		{
			if ( !User.Identity.IsAuthenticated
				|| (User.Identity.Name != "mparsons@illinoisworknet.com" 
				&& User.Identity.Name != "mparsons"))
			{
				ConsoleMessageHelper.SetConsoleInfoMessage( "ERROR - NOT AUTHORIZED for that action. ", "", false );
				return RedirectToAction( "Index", "Home" );
			}
			AccountServices mgr = new AccountServices();
			List<AppUser> users = AccountServices.ImportUsers_GetAll( maxRecords );
			string password = "";
			int cntr = 0;
			foreach ( AppUser newUser in users )
			{
				cntr++;
				string statusMessage = "";
				var user = new ApplicationUser
				{
					UserName = newUser.Email.Trim(),
					Email = newUser.Email.Trim(),
					FirstName = newUser.FirstName.Trim(),
					LastName = newUser.LastName.Trim()
				};
				Random rnd = new Random();
				int nbr = rnd.Next( 1000, 9999 );

				password = string.Format( "ctiAcct_{0}", nbr );

				var result = await UserManager.CreateAsync( user, password );

				if ( result.Succeeded )
				{
					int userId = new AccountServices().Create( user.Email,
						user.FirstName, user.LastName,
						user.Email, user.Id,
						password, ref statusMessage, false );

					if ( userId > 0 )
					{
						//no don't chg userId:
						//newUser.Id = userId;
						//log
						LoggingHelper.DoTrace( 2, "Imported user: " + newUser.Email );

						//add to org
						if ( newUser.PrimaryOrgId > 0)
							new OrganizationServices().OrganizationMember_Save( newUser.PrimaryOrgId, userId, 1, 12, ref statusMessage );

						if ( newUser.DefaultRoleId > 0 )
							mgr.Account_AddRole( userId, newUser.DefaultRoleId, 12, ref statusMessage );

						//updated
						mgr.ImportUsers_SetCompleted( newUser.Id, userId );
					}
					else
					{
						//log error
						LoggingHelper.LogError( string.Format( "Unable to create Account (aspUser was created). Email: {0}, Message", newUser.Email, statusMessage ) );

						ConsoleMessageHelper.SetConsoleErrorMessage( string.Format( "Unable to create Account (aspUser was created). Email: {0}, Message", newUser.Email, statusMessage ) );
					}
				}
				else
				{
					//error??
					//if is email already taken, may imply needing to add to a second org
					string status = string.Join( ",", result.Errors.ToArray() );
					LoggingHelper.LogError( string.Format( "Unable to create AspUser. Email: {0}, Message", user.Email, status ) );
					ConsoleMessageHelper.SetConsoleErrorMessage( string.Format( "Unable to create AspUser. Email: {0}, Message", user.Email, status ) );
				}
			}
			ConsoleMessageHelper.SetConsoleInfoMessage( string.Format("Imported {0} accounts", cntr) );
			//anything will do
			//return View( "ConfirmEmail" );
			return View( "~/Views/V2/Account/ConfirmEmail.cshtml" );
		}

		//
		// GET: /Account/ConfirmEmail
		[AllowAnonymous]
		public async Task<ActionResult> ConfirmEmail( string userId, string code )
		{
			if ( userId == null || code == null )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "Sorry, that confirmation information was invalid." );
				return RedirectToAction( "Index", "Home" );
				//return View( "Error" );
			}
			var result = await UserManager.ConfirmEmailAsync( userId, code );
			if ( result.Succeeded )
			{
				//activate user
				new AccountServices().ActivateUser( userId );
				//return View( "ConfirmEmail" );
				return View( "~/Views/V2/Account/ConfirmEmail.cshtml" );
			}
			else
			{
				return View( "Error" );
			}

		}

		//
		// GET: /Account/ForgotPassword
		[AllowAnonymous]
		public ActionResult ForgotPassword()
		{
			//return View();
			return View( "~/Views/V2/Account/ForgotPassword.cshtml" );
		}

		//
		// POST: /Account/ForgotPassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ForgotPassword( ForgotPasswordViewModel model )
		{
			if ( ModelState.IsValid )
			{
				//var user = await UserManager.FindByNameAsync( model.Email );
				var user = await UserManager.FindByEmailAsync( model.Email );
				if ( user == null )
				{
					// Don't reveal that the user does not exist or is not confirmed????
					// 16-09-02 mp - actually for now inform user of incorrect email
					if ( UtilityManager.GetAppKeyValue( "notifyOnEmailNotFound", false ) )
					{
						ConsoleMessageHelper.SetConsoleErrorMessage( "Error - the entered email was not found in our system.<br/>Please try again or contact site administration for help" );

						return View( "~/Views/V2/Account/ForgotPassword.cshtml" );
					}
					else
					{
						return View( "~/Views/V2/Account/ForgotPasswordConfirmation.cshtml" );
					}
					
				}
				else if (!( await UserManager.IsEmailConfirmedAsync( user.Id ) ) )
				{
					// Don't reveal that the user is not confirmed????
					//log this in anticipation of issues
					AccountServices.SendEmail_OnUnConfirmedEmail( model.Email );
					return View( "~/Views/V2/Account/ForgotPasswordConfirmation.cshtml" );
				}

				// For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
				// Send an email with this link
				string code = await UserManager.GeneratePasswordResetTokenAsync( user.Id );
				var callbackUrl = Url.Action( "ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme );
				await UserManager.SendEmailAsync( user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>" );
				return RedirectToAction( "ForgotPasswordConfirmation", "Account" );
			}

			// If we got this far, something failed, redisplay form
			//return View( model );
			return View( "~/Views/V2/Account/ForgotPassword.cshtml", model );
		}

		//
		// GET: /Account/ForgotPasswordConfirmation
		[AllowAnonymous]
		public ActionResult ForgotPasswordConfirmation()
		{
			//return View();
			return View( "~/Views/V2/Account/ForgotPasswordConfirmation.cshtml" );
		}

		//
		// GET: /Account/ResetPassword
		[AllowAnonymous]
		public ActionResult ResetPassword( string code )
		{
			//return code == null ? View( "Error" ) : View();
			if( code == null )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( "Error - A reset password code should have been sent to your email address. Please check your email, do a Forgot Password request from the Login page." );
				return RedirectToAction( "Index", "Home" );
			}
			else 
			{
				return View( "~/Views/V2/Account/ResetPassword.cshtml" );
			}
		}

		//
		// POST: /Account/ResetPassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ResetPassword( ResetPasswordViewModel model )
		{
			if ( !ModelState.IsValid )
			{
				//return View( model );
				return View( "~/Views/V2/Account/ResetPassword.cshtml", model );
			}
			var user = await UserManager.FindByNameAsync( model.Email );
			if ( user == null )
			{
				// Don't reveal that the user does not exist
				return RedirectToAction( "ResetPasswordConfirmation", "Account" );
			}
			var result = await UserManager.ResetPasswordAsync( user.Id, model.Code, model.Password );
			if ( result.Succeeded )
			{
				return RedirectToAction( "ResetPasswordConfirmation", "Account" );
			}
			AddErrors( result );
			//return View();
			return View( "~/Views/V2/Account/ResetPassword.cshtml" );
		}

		//
		// GET: /Account/ResetPasswordConfirmation
		[AllowAnonymous]
		public ActionResult ResetPasswordConfirmation()
		{
			//return View();
			return View( "~/Views/V2/Account/ResetPasswordConfirmation.cshtml" );
		}

		//
		// POST: /Account/ExternalLogin
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult ExternalLogin( string provider, string returnUrl )
		{
			// Request a redirect to the external login provider
			return new ChallengeResult( provider, Url.Action( "ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl } ) );
		}


		#region account code methods - not implemented
		//
		// GET: /Account/SendCode
		[AllowAnonymous]
		public async Task<ActionResult> SendCode( string returnUrl, bool rememberMe )
		{
			var userId = await SignInManager.GetVerifiedUserIdAsync();
			if ( userId == null )
			{
				return View( "Error" );
			}
			var userFactors = await UserManager.GetValidTwoFactorProvidersAsync( userId );
			var factorOptions = userFactors.Select( purpose => new SelectListItem { Text = purpose, Value = purpose } ).ToList();
			return View( new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe } );
		}

		//
		// POST: /Account/SendCode
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> SendCode( SendCodeViewModel model )
		{
			if ( !ModelState.IsValid )
			{
				return View();
			}

			// Generate the token and send it
			if ( !await SignInManager.SendTwoFactorCodeAsync( model.SelectedProvider ) )
			{
				return View( "Error" );
			}
			return RedirectToAction( "VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe } );
		}


		// GET: /Account/VerifyCode
		[AllowAnonymous]
		public async Task<ActionResult> VerifyCode( string provider, string returnUrl, bool rememberMe )
		{
			// Require that the user has already logged in via username/password or external login
			if ( !await SignInManager.HasBeenVerifiedAsync() )
			{
				return View( "Error" );
			}
			return View( new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe } );
		}

		//
		// POST: /Account/VerifyCode
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> VerifyCode( VerifyCodeViewModel model )
		{
			if ( !ModelState.IsValid )
			{
				return View( model );
			}

			// The following code protects for brute force attacks against the two factor codes. 
			// If a user enters incorrect codes for a specified amount of time then the user account 
			// will be locked out for a specified amount of time. 
			// You can configure the account lockout settings in IdentityConfig
			var result = await SignInManager.TwoFactorSignInAsync( model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser );
			switch ( result )
			{
				case SignInStatus.Success:
					return RedirectToLocal( model.ReturnUrl );
				case SignInStatus.LockedOut:
					return View( "Lockout" );
				case SignInStatus.Failure:
				default:
					ModelState.AddModelError( "", "Invalid code." );
					return View( model );
			}
		}

		#endregion

		///called each time for an external login like google.
		// GET: /Account/ExternalLoginCallback
		[AllowAnonymous]
		public async Task<ActionResult> ExternalLoginCallback( string returnUrl )
		{


			var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
			if ( loginInfo == null )
			{
				LoggingHelper.DoTrace( 2, "AccountController.ExternalLoginCallback loginInfo == null! " );

				//work around: Seems the google login worked, but not detected here. 
				//found that doing a sign out before the redirect, seems to fix the issue. 
				//As well had added <location path="signin-google"> to the web.config. The latter did not work immediately, maybe there is a relation?
				AuthenticationManager.SignOut( DefaultAuthenticationTypes.ApplicationCookie );
				ConsoleMessageHelper.SetConsoleInfoMessage( "Sorry - a minor glitch was encountered with the external login. Please try again.", "", false );
				return RedirectToAction( "Login" );
			}

			// Sign in the user with this external login provider if the user already has a login
			var result = await SignInManager.ExternalSignInAsync( loginInfo, isPersistent: false );
			switch ( result )
			{
				case SignInStatus.Success:
					AppUser user = AccountServices.SetUserByEmail( loginInfo.Email );
					string message = string.Format( "External login. Email: {0}, provider: {1}", loginInfo.Email, loginInfo.Login.LoginProvider );
					LoggingHelper.DoTrace( 2, "AccountController.ExternalLoginCallback: " + message );

					ActivityServices.UserExternalAuthentication( user, loginInfo.Login.LoginProvider );

					return RedirectToLocal( returnUrl );
				case SignInStatus.LockedOut:
					return View( "Lockout" );
				case SignInStatus.RequiresVerification:
					return RedirectToAction( "SendCode", new { ReturnUrl = returnUrl, RememberMe = false } );
				case SignInStatus.Failure:
				default:
					// If the user does not have an account, then prompt the user to create an account
					ViewBag.ReturnUrl = returnUrl;
					ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
					//return View( "ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email } );
					return View( "~/Views/V2/Account/ExternalLoginConfirmation.cshtml", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email } );
			}
		}

		//
		// POST: /Account/ExternalLoginConfirmation
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ExternalLoginConfirmation( ExternalLoginConfirmationViewModel model, string returnUrl )
		{
			LoggingHelper.DoTrace( 5, "AccountController.ExternalLoginConfirmation - enter " );

			if ( User.Identity.IsAuthenticated )
			{
				LoggingHelper.DoTrace( 5, "AccountController.ExternalLoginConfirmation - user is already authenticated " );

				return RedirectToAction( "Index", "Manage" );
			}
			string statusMessage = "";

			if ( ModelState.IsValid )
			{
				// Get the information about the user from the external login provider
				var info = await AuthenticationManager.GetExternalLoginInfoAsync();
				if ( info == null )
				{
					//return View( "ExternalLoginFailure" );
					return RedirectToAction( "ExternalLoginFailure" );
				}
				//todo - may change to not persist the names
				var user = new ApplicationUser
				{
					UserName = model.Email,
					Email = model.Email,
					FirstName = model.FirstName,
					LastName = model.LastName
				};
				var result = await UserManager.CreateAsync( user );
				if ( result.Succeeded )
				{
					//add mirror account
					new AccountServices().Create( model.Email,
						model.FirstName, model.LastName,
						model.Email,
						user.Id,
						"",
						ref statusMessage, false, true );

					result = await UserManager.AddLoginAsync( user.Id, info.Login );
					if ( result.Succeeded )
					{
						await SignInManager.SignInAsync( user, isPersistent: false, rememberBrowser: false );
						//get user and add to session (TEMP)
						//or only do on demand?
						AccountServices.GetUserByKey( user.Id );

						return RedirectToLocal( returnUrl );
					}
				}
				AddErrors( result );
			}

			ViewBag.ReturnUrl = returnUrl;
			//return View( model );
			return View( "~/Views/V2/Account/ExternalLoginConfirmation.cshtml", model );
		}

		//
		// POST: /Account/LogOff
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult LogOff()
		{
			AuthenticationManager.SignOut( DefaultAuthenticationTypes.ApplicationCookie );

			Session.Abandon();
			return RedirectToAction( "Index", "Home" );
		}

		//
		// GET: /Account/ExternalLoginFailure
		[AllowAnonymous]
		public ActionResult ExternalLoginFailure()
		{
			//return View();
			return View( "~/Views/V2/Account/ExternalLoginFailure.cshtml" );
		}

		protected override void Dispose( bool disposing )
		{
			if ( disposing )
			{
				if ( _userManager != null )
				{
					_userManager.Dispose();
					_userManager = null;
				}

				if ( _signInManager != null )
				{
					_signInManager.Dispose();
					_signInManager = null;
				}
			}

			base.Dispose( disposing );
		}

		#region Helpers
		// Used for XSRF protection when adding external logins
		private const string XsrfKey = "XsrfId";

		private IAuthenticationManager AuthenticationManager
		{
			get
			{
				return HttpContext.GetOwinContext().Authentication;
			}
		}

		private void AddErrors( IdentityResult result )
		{
			foreach ( var error in result.Errors )
			{
				ModelState.AddModelError( "", error );
			}
		}

		private ActionResult RedirectToLocal( string returnUrl )
		{
			if ( Url.IsLocalUrl( returnUrl ) )
			{
				return Redirect( returnUrl );
			}
			return RedirectToAction( "Index", "Home" );
		}

		internal class ChallengeResult : HttpUnauthorizedResult
		{
			public ChallengeResult( string provider, string redirectUri )
				: this( provider, redirectUri, null )
			{
			}

			public ChallengeResult( string provider, string redirectUri, string userId )
			{
				LoginProvider = provider;
				RedirectUri = redirectUri;
				UserId = userId;
			}

			public string LoginProvider { get; set; }
			public string RedirectUri { get; set; }
			public string UserId { get; set; }

			public override void ExecuteResult( ControllerContext context )
			{
				var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
				if ( UserId != null )
				{
					properties.Dictionary[ XsrfKey ] = UserId;
				}
				context.HttpContext.GetOwinContext().Authentication.Challenge( properties, LoginProvider );
			}
		}
		#endregion
	}
}