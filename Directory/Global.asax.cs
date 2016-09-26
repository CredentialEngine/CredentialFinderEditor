﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
			
			//AntiForgeryConfig.SuppressXFrameOptionsHeader = true;


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
		}
		private void Application_EndRequest( object sender, EventArgs e )
		{
			//Response.Headers[ "X-FRAME-OPTIONS" ] = string.Empty;
		}

		void Session_Start( object sender, EventArgs e )
		{
			try
			{
				//Do we want to track the referer somehow??
				string lRefererPage = GetUserReferrer();
				bool isBot = false;
				string ipAddress = this.GetUserIPAddress();
				//check for bots
				//use common method
				string agent = GetUserAgent( ref isBot );

				if ( isBot == false )
				{
					AppUser user = new AppUser();

					if ( User.Identity.IsAuthenticated ) 
						user = AccountServices.GetCurrentUser( User.Identity.Name );
					string userState = user.Id > 0 ? string.Format("User: {0}", user.FullName()) : "none";

					LoggingHelper.DoTrace( 6, string.Format( "Session_Start. referrer: {0}, agent: {1}, IP Address: {2}, User?: {3}", lRefererPage, agent, ipAddress, userState ) );

					string startMsg = "Session Started. SessionID: " + Session.SessionID;
					
					startMsg += ", IP Address: " + ipAddress;

					startMsg += ", User: " + userState;
					startMsg += ", Agent: " + agent;
					ActivityServices.SessionStartActivity( startMsg, Session.SessionID.ToString(), ipAddress, lRefererPage, isBot );

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
