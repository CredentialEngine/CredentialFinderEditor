using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Models;
using ThisUser = Models.AppUser;
using Factories;
using Utilities;

namespace CTIServices
{
	public class ActivityServices : ServiceHelper
	{
		private static string thisClassName = "ActivityServices";
		ActivityManager mgr = new ActivityManager();

		/// <summary>
		/// General purpose create of a site activity
		/// </summary>
		/// <param name="activity"></param>
		/// <param name="activityEvent"></param>
		/// <param name="comment">A formatted user friendly description of the activity</param>
		/// <param name="actionByUserId">Optional userId of person initiating the activity</param>
		/// <param name="activityObjectId"></param>
		public void AddActivity( string activity, string activityEvent, string comment, int actionByUserId, int targetUserId = 0, int activityObjectId = 0 )
		{

			SiteActivity log = new SiteActivity();
			log.CreatedDate = System.DateTime.Now;
			log.ActivityType = "Audit";
			log.Activity = activity;
			log.Event = activityEvent;
			log.Comment = comment;
			log.SessionId = ActivityManager.GetCurrentSessionId();
			log.IPAddress = ActivityManager.GetUserIPAddress();

			log.ActionByUserId = actionByUserId;
			log.ActivityObjectId = activityObjectId;
			log.TargetUserId = targetUserId;
			
			
			try
			{
				mgr.SiteActivityAdd( log );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".AddActivity()" );
				return;
			}
		}

		/// <summary>
		/// Add site activity
		/// </summary>
		/// <param name="log"></param>
		public void AddActivity( SiteActivity log )
		{

			if ( log.SessionId == null || log.SessionId.Length < 10 )
				log.SessionId = HttpContext.Current.Session.SessionID;

			if ( log.IPAddress == null || log.IPAddress.Length < 10 )
				log.IPAddress = ActivityManager.GetUserIPAddress();

			try
			{
				mgr.SiteActivityAdd( log );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".AddActivity(SiteActivity log)" );
				return;
			}
		}
		#region Site Activity - account
		public static int UserRegistration( AppUser entity )
		{
			string ipAddress = ActivityManager.GetUserIPAddress();
			return UserRegistration( entity, ipAddress, "Registration" );

		}
		public static int UserRegistration( AppUser entity, string ipAddress )
		{
			return UserRegistration( entity, ipAddress, "Registration" );
			
		}
		public static int UserRegistrationFromGoogle( ThisUser entity, string ipAddress )
		{
			return UserRegistration( entity, ipAddress, "Google SSO Registration" );
			
		}

		public static int UserRegistration( ThisUser entity, string ipAddress, string type )
		{
			string server = UtilityManager.GetAppKeyValue( "serverName", "" );

			SiteActivity log = new SiteActivity();
			log.CreatedDate = System.DateTime.Now;
			log.ActivityType = "Audit";
			log.Activity = "Account";
			log.Event = type;
			log.Comment = string.Format( "{0} ({1}) {4}. From IPAddress: {2}, on server: {3}", entity.FullName(), entity.Id, ipAddress, server, type );
			//actor type - person, system
			log.ActionByUserId = entity.Id;
			log.TargetUserId = entity.Id;
			log.SessionId = ActivityManager.GetCurrentSessionId();
			try
			{
				return new ActivityManager().SiteActivityAdd( log );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".UserRegistrationFromPortal()" );
				return 0;
			}
		}
		public static int UserRegistrationConfirmation( ThisUser entity, string type )
		{
			string server = UtilityManager.GetAppKeyValue( "serverName", "" );
			//EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
			SiteActivity log = new SiteActivity();

			try
			{
				log.CreatedDate = System.DateTime.Now;
				log.ActivityType = "Audit";
				log.Activity = "Account";
				log.Event = "Confirmation";
				log.Comment = string.Format( "{0} ({1}) Registration Confirmation, on server: {2}, tyep: {3}", entity.FullName(), entity.Id, server, type );
				//actor type - person, system
				log.ActionByUserId = entity.Id;
				log.TargetUserId = entity.Id;

				log.SessionId = ActivityManager.GetCurrentSessionId();

				return new ActivityManager().SiteActivityAdd( log );

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".UserRegistrationConfirmation()" );
				return 0;
			}
		}
		public static int UserAutoAuthentication( ThisUser entity )
		{
			return UserAuthentication( entity, "auto" );
		}
		public static int UserAuthentication( ThisUser entity )
		{
			return UserAuthentication( entity, "login" );
		}
		public static int UserExternalAuthentication( ThisUser entity, string provider )
		{
			return UserAuthentication( entity, provider );
		}
		private static int UserAuthentication( ThisUser entity, string type )
		{

			string server = UtilityManager.GetAppKeyValue( "serverName", "" );
			SiteActivity log = new SiteActivity();
			log.CreatedDate = System.DateTime.Now;
			log.ActivityType = "Audit";
			log.Activity = "Account";
			log.Event = "Authentication: " + type;
			log.Comment = string.Format( "{0} ({1}) logged in ({2}) on server: {3}", entity.FullName(), entity.Id, type, server );
			//actor type - person, system
			log.ActionByUserId = entity.Id;
			log.TargetUserId = entity.Id;
			log.SessionId = ActivityManager.GetCurrentSessionId();
			try
			{
				return new ActivityManager().SiteActivityAdd( log );

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".UserAuthentication()" );
				return 0;
			}
		}

		public static void SessionStartActivity( string comment, string sessionId, string ipAddress, string referrer, bool isBot )
		{
			SiteActivity log = new SiteActivity();
			//bool isBot = false;
			string server = UtilityManager.GetAppKeyValue( "serverName", "" );
			try
			{
				if ( comment == null )
					comment = "";

				log.CreatedDate = System.DateTime.Now;
				log.ActivityType = "Audit";
				log.Activity = "Session";
				log.Event = "Start";
				log.Comment = comment + string.Format( " (on server: {0})", server );
				//already have agent
				//log.Comment += GetUserAgent(ref isBot);

				log.SessionId = sessionId;
				log.IPAddress = ipAddress;
				log.Referrer = referrer;

				new ActivityManager().SiteActivityAdd( log );

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".SessionStartActivity()" );
				return;
			}

		}
		#endregion

		public static int SiteActivityAdd( string activity, string eventType, string comment
					, int actionByUserId, int targetUserId, int activityObjectId )
		{
			return SiteActivityAdd( activity, eventType, comment, actionByUserId, targetUserId, activityObjectId, "", "" );
		}

		public static int SiteActivityAdd( string activity, string eventType, string comment
					, int actionByUserId, int targetUserId, int activityObjectId
					, string sessionId, string ipAddress )
		{
			string server = UtilityManager.GetAppKeyValue( "serverName", "" );
			SiteActivity log = new SiteActivity();
			if ( sessionId == null || sessionId.Length < 10 )
				sessionId = HttpContext.Current.Session.SessionID;

			if ( ipAddress == null || ipAddress.Length < 10 )
				ipAddress = ActivityManager.GetUserIPAddress();

			try
			{
				log.CreatedDate = System.DateTime.Now;
				log.ActivityType = "Audit";
				log.Activity = activity;
				log.Event = eventType;
				log.Comment = comment + string.Format( " (on server: {0})", server );
				//actor type - person, system
				if ( actionByUserId > 0 )
					log.ActionByUserId = actionByUserId;
				if ( targetUserId > 0 )
					log.TargetUserId = targetUserId;
				if ( activityObjectId > 0 )
					log.ActivityObjectId = activityObjectId;

				log.SessionId = sessionId;
				log.IPAddress = ipAddress;

				return new ActivityManager().SiteActivityAdd( log );

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".SiteActivityAdd()" );
				return 0;
			}
		} //


	}
}
