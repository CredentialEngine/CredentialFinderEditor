using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
//using System.Web.Mvc;
using System.Web.SessionState;

using Models;

using Utilities;
using Factories;

namespace CTIServices
{
	public class AccountServices
	{
		private static string thisClassName = "AccountServices";
		#region Authorization methods
		public static bool IsUserAnAdmin()
		{
			AppUser user = GetUserFromSession();
			if ( user == null || user.Id == 0 )
				return false;

			return IsUserAnAdmin( user );
		}
		public static bool IsUserAnAdmin( AppUser user )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( user.Roles.Contains( "Administrator" ) )
				return true;
			else
				return false;
		}
		public static bool IsUserSiteStaff()
		{
			AppUser user = GetUserFromSession();
			if ( user == null || user.Id == 0 )
				return false;

			return IsUserSiteStaff( user );
		}
		public static bool IsUserSiteStaff( AppUser user )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( user.Roles.Contains( "Administrator" )
			  || user.Roles.Contains( "Site Manager" )
			  || user.Roles.Contains( "Site Staff" )
				)
				return true;
			else
				return false;
		}

		public static bool CanUserViewSite()
		{
			//this method will not expect a status message
			string status = "";
			AppUser user = GetUserFromSession();
			if ( user == null || user.Id == 0 )
				return false;
			return CanUserViewSite( user, ref status );
		}
		public static bool CanUserViewSite( AppUser user, ref string status )
		{
			//this method will not expect a status message
			status = "";
			if ( user == null || user.Id == 0 )
			{
				status = "You must be authenticated and authorized before being allowed to view any content.";
				return false;
			}

			if ( user.Roles.Contains( "Administrator" )
			  || user.Roles.Contains( "Site Manager" )
			  || user.Roles.Contains( "Site Staff" )
			  || user.Roles.Contains( "Site Partner" )
			  || user.Roles.Contains( "Site Reader" )
				)
				return true; 

			// allow if user is member of an org
			//depends on purpose, if site in general, ok, but not for viewing unpublished stuff
			if ( OrganizationManager.IsMemberOfAnyOrganization( user.Id ) )
				return true;

			return false;
		}
		public static bool CanUserViewAllContent( AppUser user)
		{
			//this method will not expect a status message
			//status = "";
			if ( user == null || user.Id == 0 )
			{
				//status = "You must be authenticated and authorized before being allowed to view any content.";
				return false;
			}

			if ( user.Roles.Contains( "Administrator" )
			  || user.Roles.Contains( "Site Manager" )
			  || user.Roles.Contains( "Site Staff" )
			  || user.Roles.Contains( "Site Partner" )
			  || user.Roles.Contains( "Site Reader" )
				)
				return true;

			return false;
		}
		public static bool CanUserEditAllContent()
		{
			AppUser user = GetUserFromSession();
			if ( user == null || user.Id == 0 )
			{
				//status = "You must be authenticated and authorized before being allowed to view any content.";
				return false;
			}

			if ( user.Roles.Contains( "Administrator" )
			  || user.Roles.Contains( "Site Manager" )
			  || user.Roles.Contains( "Site Staff" )
				)
				return true;

			return false;
		}
		public static bool CanUserPublishContent()
		{
			//this method will not expect a status message
			string status = "";
			AppUser user = GetUserFromSession();
			if ( user == null || user.Id == 0 )
				return false;
			return CanUserPublishContent( user, ref status );
		}
		public static bool CanUserPublishContent( ref string status )
		{
			AppUser user = GetUserFromSession();
			if ( user == null || user.Id == 0 )
				return false;

			return CanUserPublishContent( user, ref status );
		}
		/// <summary>
		/// Return true if user can publish content
		/// Essentially this relates to being able to create credentials and related entities. 
		/// </summary>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static bool CanUserPublishContent( AppUser user, ref string status )
		{
			status = "";
			if ( user == null || user.Id == 0 )
			{
				status = "You must be authenticated and authorized before being allowed to create any content.";
				return false;
			}

			if ( user.Roles.Contains( "Administrator" )
			  || user.Roles.Contains( "Site Manager" )
			  || user.Roles.Contains( "Site Staff" )
			  || user.Roles.Contains( "Site Partner" )
				)
				return true;

			//allow once out of beta, and user is member of an org
			if ( UtilityManager.GetAppKeyValue( "isSiteInBeta", true ) == false
				&& OrganizationManager.IsMemberOfAnyOrganization( user.Id ) )
				return true;

			status = "Sorry - You have not been authorized to add or update content on this site during this <strong>BETA</strong> period. Please contact site management if you believe that you should have access during this <strong>BETA</strong> period.";
			
			return false;
		}

		public static bool CanUserAddOrganizations( AppUser user, ref string status )
		{
			status = "";
			if ( user == null || user.Id == 0 )
			{
				status = "You must be authenticated and authorized before being allowed to create any content.";
				return false;
			}

			if ( user.Roles.Contains( "Administrator" )
			  || user.Roles.Contains( "Site Manager" )
			  || user.Roles.Contains( "Site Staff" )
			  || user.Roles.Contains( "Site Partner" )
				)
				return true;

			//allow once out of beta, will allow creating an org
			if ( UtilityManager.GetAppKeyValue( "isSiteInBeta", true ) == false )
				return true;

			else
			{
				status = "Sorry - You have not been authorized to add or update content on this site during this <strong>BETA</strong> period. Please contact site management if you believe that you should have access during this <strong>BETA</strong> period.";
				return false;
			}
		}
		/// <summary>
		/// Perform basic authorization checks. First establish an initial user object.
		/// Used where the user object is not to be returned.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="mustBeLoggedIn"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static bool AuthorizationCheck( string action, bool mustBeLoggedIn, ref string status )
		{
			AppUser user = new AppUser(); //			GetCurrentUser();
			return AuthorizationCheck( "", false, ref status, ref user );
		}
		/// <summary>
		/// Perform basic authorization checks
		/// </summary>
		/// <param name="action"></param>
		/// <param name="mustBeLoggedIn"></param>
		/// <param name="status"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static bool AuthorizationCheck( string action, bool mustBeLoggedIn, ref string status, ref AppUser user )
		{
			bool isAuthorized = true;
			user = GetCurrentUser();
			bool isAuthenticated = IsUserAuthenticated( user );
			if ( mustBeLoggedIn && !isAuthenticated )
			{
				status = string.Format( "You must be logged in to do that ({0}).", action );
				return false;
			}

			if ( action == "Delete" )
			{

				//TODO: validate user's ability to delete a specific entity (though this should probably be handled by the delete method?)
				if ( AccountServices.IsUserSiteStaff( user ) == false )
				{
					ConsoleMessageHelper.SetConsoleInfoMessage( "Sorry - You have not been authorized to delete content on this site during this <strong>BETA</strong> period.", "", false );

					status = "You have not been authorized to delete content on this site during this BETA period.";
					return false;
				}
			}
			return isAuthorized;

		}

		/// <summary>
		/// Perform common checks to see if a user can edit something
		/// </summary>
		/// <param name="valid"></param>
		/// <param name="status"></param>
		public static void EditCheck( ref bool valid, 
							ref string status )
		{
			var user = GetUserFromSession();

			if ( !AuthorizationCheck( "edit", true, ref status, ref user ) )
			{
				valid = false;
				status = "ERROR - NOT AUTHENTICATED. You will not be able to add or update content";
				ConsoleMessageHelper.SetConsoleInfoMessage( status, "", false );
				return;
			}

			if ( !CanUserPublishContent( user, ref status ) )
			{
				valid = false;
				//Status already set
				ConsoleMessageHelper.SetConsoleInfoMessage( status, "", false );
				return;
			}

			valid = true;
			status = "okay";
			return;
		}
		//

		#endregion

		#region Create/Update
		/// <summary>
		/// Create a new account, based on the AspNetUser info!
		/// </summary>
		/// <param name="email"></param>
		/// <param name="firstName"></param>
		/// <param name="lastName"></param>
		/// <param name="userName"></param>
		/// <param name="userKey"></param>
		/// <param name="password">NOTE: may not be necessary as the hash in the aspNetUsers table is used?</param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Create( string email, string firstName, string lastName, string userName, string userKey, string password, ref string statusMessage, bool doingEmailConfirmation = false, bool isExternalSSO = false )
		{
			int id = 0;
			statusMessage = "";
			//this password, as stored in the account table, is not actually used
			string encryptedPassword = "";
			if (!string.IsNullOrWhiteSpace(password))
				encryptedPassword = UtilityManager.Encrypt( password );

			AppUser user = new AppUser()
			{
				Email = email,
				UserName = email,
				FirstName = firstName,
				LastName = lastName,
				IsActive = !doingEmailConfirmation,
				AspNetUserId = userKey,
				Password = encryptedPassword
			};
			id = new AccountManager().Account_Add( user, ref statusMessage );
			if ( id > 0 )
			{
				AddUserToSession( HttpContext.Current.Session, user );

				ActivityServices.UserRegistration( user );
				string msg = string.Format( "New user registration. <br/>Email: {0}, <br/>Name: {1}<br/>Type: {2}", email, firstName + " " + lastName, ( isExternalSSO ? "External SSO" : "Forms" ) );

				EmailManager.NotifyAdmin( "New CTI registration", msg );
			}
			
			return id;
		} //

		/// <summary>
		/// update account, and AspNetUser
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public bool Update( AppUser user, ref string statusMessage )
		{
			bool successful = true;
			statusMessage = "";

			if ( new AccountManager().Account_Update( user, ref statusMessage ) )
			{
				AddUserToSession( HttpContext.Current.Session, user );
			}


			return successful;
		} //


		public bool ActivateUser( string aspNetId )
		{
			string statusMessage = "";
			AppUser user = GetUserByKey( aspNetId );
			if ( user != null && user.Id > 0 )
			{
				user.IsActive = true;
				if ( new AccountManager().Account_Update( user, ref statusMessage ) )
				{
					EmailManager.NotifyAdmin( "User Activated Account", string.Format( "{0} activated a CTI account. <br/>Email: {1}", user.FullName(), user.Email ) );
					return true;
				}
				else
				{
					EmailManager.NotifyAdmin( "Activate user failed", string.Format( "Attempted to activate user: {0}. <br/>Received invalid status: {1}", user.Email, statusMessage ) );
					return false;
				}
			}
			else
			{
				EmailManager.NotifyAdmin( "Activate user failed", string.Format( "Attempted to activate user aspNetId: {0}. <br/>However latter aspNetId was not found", aspNetId ) );
				return false;
			}

		}
			
		public static List<AppUser> ImportUsers_GetAll( int maxRecords )
		{
			List<AppUser> users = AccountManager.ImportUsers_GetAll( maxRecords );

			return users;

		}
		public void ImportUsers_SetCompleted( int importId, int userId, string initialPassword )
		{
			string statusMessage = "";
			bool isOk = new AccountManager().ImportUsers_Update( importId, userId, initialPassword , ref statusMessage);
		}

		public bool Account_AddRole( int userId, int roleId, int createdByUserId, ref string statusMessage ) 
		{
			return new AccountManager().Account_AddRole( userId, roleId, createdByUserId, ref statusMessage );
		}
		
		#endregion

		#region email methods
		/// <summary>
		/// Send reset password email
		/// </summary>
		/// <param name="subject"></param>
		/// <param name="toEmail"></param>
		/// <param name="body"></param>
		public static void SendEmail_ResetPassword( string subject, string toEmail, string body )
		{
			//should have a valid email at this point (if from identityConfig)
			AppUser user = GetUserByEmail( toEmail );

			bool isSecure = false;
			//string toEmail = user.Email;
			string bcc = UtilityManager.GetAppKeyValue( "systemAdminEmail", "mparsons@siuccwd.com" );

			string fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", "mparsons@siuccwd.com" );
			//string subject = "Forgot Password";
			string email = EmailManager.GetEmailText( "ForgotPassword" );
			string eMessage = "";

			try
			{
				if ( UtilityManager.GetAppKeyValue( "SSLEnable", "0" ) == "1" )
					isSecure = true;
				//string link = "/Account/Login.aspx?pg={0}&nextUrl=/My/LearningList/{1}";
				//TODO - first depends on what the Identity frameworks provides
				//string proxyId = new AccountServices().Create_3rdPartyAddProxyLoginId( user.Id, "AddedContentPartner-existing", ref statusMessage );
				//action: provide confirm url to ???. 
				//string confirmUrl = string.Format( link, proxyId.ToString(), contentId );
				//confirmUrl = UtilityManager.FormatAbsoluteUrl( confirmUrl, isSecure );

				//assign and substitute: 0-FirstName, 1-body from AccountController
				eMessage = string.Format( email, user.FirstName, body );


				EmailManager.SendEmail( toEmail, fromEmail, subject, eMessage, "", bcc );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".SendEmail_ResetPassword()" );
			}

		}

		public static void SendEmail_ConfirmAccount( string subject, string toEmail, string body )
		{
			//should have a valid email at this point (if from identityConfig)
			AppUser user = GetUserByEmail( toEmail );

			bool isSecure = false;
			//string toEmail = user.Email;
			string bcc = UtilityManager.GetAppKeyValue( "systemAdminEmail", "mparsons@siuccwd.com" );

			string fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", "mparsons@siuccwd.com" );
			//string subject = "Forgot Password";
			string email = EmailManager.GetEmailText( "ConfirmAccount" );
			string eMessage = "";

			try
			{

				//assign and substitute: 0-FirstName, 1-body from AccountController
				eMessage = string.Format( email, user.FirstName, body );

				EmailManager.SendEmail( toEmail, fromEmail, subject, eMessage, "", bcc );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".SendEmail_ConfirmPassword()" );
			}

		}

		public static void SendEmail_OnUnConfirmedEmail( string userEmail  )
		{
			//should have a valid email at this point
			AppUser user = GetUserByEmail( userEmail );
			string subject = "Forgot password attempt with unconfirmed email";
			
			string toEmail = UtilityManager.GetAppKeyValue( "systemAdminEmail", "mparsons@siuccwd.com" );

			string fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", "mparsons@siuccwd.com" );
			//string subject = "Forgot Password";
			string email = "User: {0} attempted Forgot Password, and email has not been confirmed.<br/>Email: {1}<br/>Created: {2}";
			string eMessage = "";

			try
			{
				eMessage = string.Format( email, user.FullName(), user.Email, user.Created );

				EmailManager.SendEmail( toEmail, fromEmail, subject, eMessage, "", "" );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".SendEmail_OnUnConfirmedEmail()" );
			}
		}
		#endregion

		#region Read methods
		/// <summary>
		/// Retrieve a user by email address
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		public static AppUser GetUserByEmail( string email )
		{
			AppUser user = AccountManager.AppUser_GetByEmail( email );

			return user;
		} //
		public static AppUser GetUserByUserName( string username )
		{
			AppUser user = AccountManager.GetUserByUserName( username );

			return user;
		} //
		/// <summary>
		/// Get user by email, and add to the session
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		public static AppUser SetUserByEmail( string email )
		{
			AppUser user = AccountManager.AppUser_GetByEmail( email );
			AddUserToSession( HttpContext.Current.Session, user );
			return user;
		} //

		/// <summary>
		/// User is authenticated, either get from session or via the Identity name
		/// </summary>
		/// <param name="identityName"></param>
		/// <returns></returns>
		public static AppUser GetCurrentUser( string identityName = "" )
		{
			AppUser user = AccountServices.GetUserFromSession();
			if ( ( user == null || user.Id == 0 ) && !string.IsNullOrWhiteSpace( identityName ) )
			{
				//NOTE identityName is related to the UserName
				//TODO - need to add code to prevent dups between google register and direct register
				user = GetUserByUserName( identityName );
				if ( user != null && user.Id > 0 )
					AddUserToSession( HttpContext.Current.Session, user );
			}

			return user;
		} //
		public static int GetCurrentUserId()
		{
			AppUser user = AccountServices.GetUserFromSession();
			if ( user == null || user.Id == 0 ) 
				return 0;
			else 
				return user.Id;
		} //

		/// <summary>
		/// set the current user via an identity name at session start
		/// </summary>
		/// <param name="identityName"></param>
		/// <returns></returns>
		public static AppUser SetCurrentUser( string identityName )
		{
			AppUser user = AccountServices.GetUserFromSession();
			if ( !string.IsNullOrWhiteSpace( identityName ) )
			{
				//assume identityName is email
				//TODO - need to add code to prevent dups between google register and direct register
				user = GetUserByEmail( identityName );
				if ( user != null && user.Id > 0 )
					AddUserToSession( HttpContext.Current.Session, user );
			}

			return user;
		} //

		/// <summary>
		/// get account by the aspNetId,and add to session
		/// </summary>
		/// <param name="aspNetId"></param>
		/// <returns></returns>
		public static AppUser GetUserByKey( string aspNetId )
		{
			AppUser user = AccountManager.AppUser_GetByKey( aspNetId );

			AddUserToSession( HttpContext.Current.Session, user );

			return user;
		} //
		public static AppUser GetUser( int id )
		{
			AppUser user = AccountManager.AppUser_Get( id );

			return user;
		} //

		#endregion
		#region Session methods
		/// <summary>
		/// Determine if current user is a logged in (authenticated) user 
		/// </summary>
		/// <returns></returns>
		public static bool IsUserAuthenticated()
		{
			bool isUserAuthenticated = false;
			try
			{
				AppUser appUser = GetUserFromSession();
				isUserAuthenticated = IsUserAuthenticated( appUser );
			}
			catch
			{

			}

			return isUserAuthenticated;
		} //
		public static bool IsUserAuthenticated( AppUser appUser )
		{
			bool isUserAuthenticated = false;
			try
			{
				if ( appUser == null || appUser.Id == 0 || appUser.IsActive == false )
				{
					isUserAuthenticated = false;
				}
				else
				{
					isUserAuthenticated = true;
				}
			}
			catch
			{

			}

			return isUserAuthenticated;
		} //
		public static AppUser GetUserFromSession()
		{
			if (HttpContext.Current != null && HttpContext.Current.Session != null )
			{
				return GetUserFromSession( HttpContext.Current.Session );
			}
			else
				return null;
		} //

		public static AppUser GetUserFromSession( HttpSessionState session )
		{
			AppUser user = new AppUser();
			try
			{ 		//Get the user
				user = ( AppUser ) session[ "user" ];

				if ( user.Id == 0 || !user.IsValid )
				{
					user.IsValid = false;
					user.Id = 0;
				}
			}
			catch
			{
				user = new AppUser();
				user.IsValid = false;
			}
			return user;
		}

		/// <summary>
		/// Sets the current user to the session.
		/// </summary>
		/// <param name="session">HTTP Session</param>
		/// <param name="appUser">application User</param>
		public static void AddUserToSession( HttpSessionState session, AppUser appUser )
		{
			session[ "user" ] = appUser;

		} //
		#endregion
	}
}
