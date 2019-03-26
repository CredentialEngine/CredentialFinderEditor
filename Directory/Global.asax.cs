using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using System.Web.Optimization;
using System.Web.Routing;

using CTIServices;
using Models;
using Utilities;

namespace CTI.Directory
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			RouteConfig.RegisterRoutes( RouteTable.Routes );

			FilterConfig.RegisterGlobalFilters( GlobalFilters.Filters );

			BundleConfig.RegisterBundles( BundleTable.Bundles );

            AntiForgeryConfig.SuppressXFrameOptionsHeader = true;
            ActivityServices.ApplicationStartActivity();

        }
		protected void Application_BeginRequest( object sender, EventArgs e )
		{
			//if ( Request.Url.Host.StartsWith( "www" ) && !Request.Url.IsLoopback )
			//{
				//LoggingHelper.DoTrace( 4, string.Format("Doing redirect from {0}",  Request.Url.Host));
				//now handled in _header.cshtml
				//UriBuilder builder = new UriBuilder( Request.Url );
				//builder.Host = Request.Url.Host.Replace( "www.", "" );
				//Response.StatusCode = 301;
				//Response.AddHeader( "Location", builder.ToString() );
				//Response.End();
			//}
			bool doingWWWCheck = UtilityManager.GetAppKeyValue( "doingHttpsRedirectForWWW", false );
			if ( doingWWWCheck && Context.Request.IsSecureConnection )
			{
				//if www redirect to non-www
				if ( Request.Url.Host.StartsWith( "www" ) && !Request.Url.IsLoopback )
				{
					LoggingHelper.DoTrace( 2, "doing www redirect" );
					Response.Redirect( Context.Request.Url.ToString().Replace( "www.", "" ) );
				}
				
			}
		}
		private void Application_EndRequest( object sender, EventArgs e )
		{
			//Response.Headers[ "X-FRAME-OPTIONS" ] = string.Empty;

		}
		protected void Application_Error( object sender, EventArgs e )
		{
			Exception exception = Server.GetLastError();
			Response.Clear();

			HttpException httpException = exception as HttpException;
			if ( httpException != null )
			{
				RouteData routeData = new RouteData();
				routeData.Values.Add( "controller", "Error" );
				switch ( httpException.GetHttpCode() )
				{
					case 404:
						// page not found
						routeData.Values.Add( "action", "HttpError404" );
						break;
                    //case 400:
                    //    // page not found
                    //    routeData.Values.Add( "action", "HttpError404" );
                    //    break;
                    case 500:
						// server error
						routeData.Values.Add( "action", "HttpError500" );
						break;
					default:
						routeData.Values.Add( "action", "General" );
						break;
				}
				routeData.Values.Add( "error", exception );
				bool loggingError = true;
				if ( exception.Message.IndexOf( "Server cannot set status after HTTP headers;" ) > -1 )
					loggingError = false;
                string url = "MISSING";
                try
                {
                    HttpContext con = HttpContext.Current;
                    url= con.Request.Url.ToString();
                    url = url.Trim();
                } catch
                {
                    //skip
                }
                string lRefererPage = GetUserReferrer();
                var user = AccountServices.GetCurrentUser();
                string currentUser = "unknown";
                if (user != null && user.Id > 0 )
                {
                    currentUser = string.Format( "User: {0} ({1})", user.FullName(), user.Id );
                }
                if ( url.EndsWith( "&" ) || url.EndsWith( ";" ) )
                {
                    LoggingHelper.DoTrace( 4, string.Format( "Application_Error. url: {0} referer: {1}, currentUser: {2}, message: {3}", url.Trim(), lRefererPage, currentUser, exception.Message ) );
                    url = url.TrimEnd( '&' ).Trim();
                    url = url.TrimEnd( ';' ).Trim();
                    Response.Redirect( url, true );
                }
                else
                {
                    if ( loggingError )
                        LoggingHelper.LogError( exception, string.Format( "Application HttpException. url: {0}, Referer: {1}, currentUser: {2}", url, lRefererPage, currentUser ) );
                }
				// clear error on server ==> this would hide the error in dev as well
				//Server.ClearError();

				// at this point how to properly pass route data to error controller?
			}
			else
			{
				string url = "MISSING";
				try
				{
					HttpContext con = HttpContext.Current;
					url = con.Request.Url.ToString();
					url = url.Trim();
				}
				catch
				{
					//skip
				}
				string lRefererPage = "unknown";
				try
				{
					if ( Request.UrlReferrer != null )
					{
						lRefererPage = Request.UrlReferrer.ToString();
					}
				}
				catch ( Exception ex )
				{
					//skip
				}
				var user = AccountServices.GetUserFromSession();
				LoggingHelper.LogError( exception, string.Format( "Application Exception. url: {0}, Referer: {1}, currentUser: {2}", url, lRefererPage, user ) );
			}
		}

		void Session_Start( object sender, EventArgs e )
		{
			// Code that runs when a new session is started
			//apparantly can prevent error:
			/*
			Session state has created a session id, but cannot save it because the response was already flushed by the application
			*/
    		string sessionId = Session.SessionID;


			try
			{
				//Do we want to track the referer somehow??
				string lRefererPage = GetUserReferrer();
				bool isBot = false;
				string ipAddress = this.GetUserIPAddress();
				//check for bots
				//use common method
				string agent = GetUserAgent( ref isBot );
                string url = "MISSING";
                try
                {
                    HttpContext con = HttpContext.Current;
                    url = con.Request.Url.ToString();
                }
                catch
                {
                    //skip
                }
                if ( isBot == false )
				{
					AppUser user = new AppUser();
					//not always helpful
					if ( User.Identity.IsAuthenticated )
					{
						user = AccountServices.GetCurrentUser( User.Identity.Name, true );
						//If already in session, do we need to do something official
					} else
					{
						//get session?, can't be if a new session
						//user = AccountServices.GetCurrentUser();
					}
					string userState = user.Id > 0 ? string.Format("{0}", user.FullName()) : "none";

					LoggingHelper.DoTrace( 6, string.Format( "Session_Start. referrer: {0}, starting Url: {1} agent: {2}, IP Address: {3}, User?: {4}", lRefererPage, url, agent, ipAddress, userState ) );

					string startMsg = "Session Started. SessionID: " + sessionId;
					
					startMsg += ", IP Address: " + ipAddress;

					startMsg += ", User: " + userState;
					startMsg += ", Agent: " + agent;
					ActivityServices.SessionStartActivity( startMsg, sessionId, ipAddress, lRefererPage, isBot );

				}
				else
				{
					LoggingHelper.DoBotTrace( 8, string.Format( "Session_Start. Skipping bot: referrer: {0}, agent: {1}", lRefererPage, agent ) );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "Session_Start. =================" );
			}

		} //
		private static string GetUserAgent( ref bool isBot )
		{
			string agent = "";
			isBot = false;
			try
			{
				if ( HttpContext.Current.Request.UserAgent != null )
				{
					agent = HttpContext.Current.Request.UserAgent;
				}

				if ( agent.ToLower().IndexOf( "bot" ) > -1
					|| agent.ToLower().IndexOf( "spider" ) > -1
					|| agent.ToLower().IndexOf( "slurp" ) > -1
					|| agent.ToLower().IndexOf( "crawl" ) > -1
					|| agent.ToLower().IndexOf( "addthis.com" ) > -1
					)
					isBot = true;
				if ( isBot )
				{
					//what should happen? Skip completely? Should add attribute to track
					//user agent may NOT be available in this context
				}
			}
			catch ( Exception ex )
			{
				//agent = ex.Message;
			}

			return agent;
		} //

		private string GetUserReferrer()
		{
			string lRefererPage = "unknown";
			try
			{
				if ( Request.UrlReferrer != null )
				{
					lRefererPage = Request.UrlReferrer.ToString();
					//check for link to us parm
					//??

					//handle refers from illinoisworknet.com 
					if ( lRefererPage.ToLower().IndexOf( ".illinoisworknet.com" ) > -1 )
					{
						//may want to keep reference to determine source of this condition. 
						//For ex. user may have let referring page get stale and so a new session was started when user returned! 

					}
				}
			}
			catch ( Exception ex )
			{
				lRefererPage = ex.Message;
			}

			return lRefererPage;
		} //
		private string GetUserIPAddress()
		{
			string ip = "unknown";
			try
			{
				ip = Request.ServerVariables[ "HTTP_X_FORWARDED_FOR" ];
				if ( ip == null || ip == "" || ip.ToLower() == "unknown" )
				{
					ip = Request.ServerVariables[ "REMOTE_ADDR" ];
				}
			}
			catch ( Exception ex )
			{

			}

			return ip;
		} //
	}
}
