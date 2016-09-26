using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Models;
using Data;
using Utilities;
using Views = Data.Views;

using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class ActivityManager
	{
		private static string thisClassName = "ActivityManager";


		#region Persistance
		public int SiteActivityAdd( SiteActivity entity )
		{
			ActivityLog log = new ActivityLog();
			 FromMap( entity, log);
			return SiteActivityAdd( log );
		} //

		private static int SiteActivityAdd( ActivityLog log )
		{
			int count = 0;
			string truncateMsg = "";
			bool isBot = false;
			string server = UtilityManager.GetAppKeyValue( "serverName", "" );

			string agent = GetUserAgent( ref isBot );

			if ( log.RelatedTargetUrl == null )
				log.RelatedTargetUrl = "";
			if ( log.RelatedImageUrl == null )
				log.RelatedImageUrl = "";
			if ( log.Referrer == null )
				log.Referrer = "";
			if ( log.Comment == null )
				log.Comment = "";
			if ( log.SessionId == null || log.SessionId.Length < 10 )
				log.SessionId = GetCurrentSessionId();

			if ( log.IPAddress == null || log.IPAddress.Length < 10 )
				log.IPAddress = GetUserIPAddress();
			if ( log.IPAddress.Length > 50 )
				log.IPAddress = log.IPAddress.Substring( 0, 50 );

			//================================
			if ( isBot )
			{
				LoggingHelper.DoBotTrace( 6, string.Format( ".SiteActivityAdd Skipping Bot: activity. Agent: {0}, Activity: {1}, Event: {2}, \r\nRelatedTargetUrl: {3}", agent, log.Activity, log.Event, log.RelatedTargetUrl ) );
				//should this be added with isBot attribute for referencing when crawled?
				return 0;
			}
			//================================
			if ( IsADuplicateRequest( log.Comment ) )
				return 0;

			StoreLastRequest( log.Comment );

			//----------------------------------------------
			if ( log.Referrer == null || log.Referrer.Trim().Length < 5 )
			{
				string referrer = GetUserReferrer();
				log.Referrer = referrer;
			}
			if ( log.Referrer.Length > 1000 )
			{
				truncateMsg += string.Format( "Referrer overflow: {0}; ", log.Referrer.Length );
				log.Referrer = log.Referrer.Substring( 0, 1000 );
			}


			if ( log.RelatedTargetUrl != null && log.RelatedTargetUrl.Length > 500 )
			{
				truncateMsg += string.Format( "RelatedTargetUrl overflow: {0}; ", log.RelatedTargetUrl.Length );
				log.RelatedTargetUrl = log.RelatedTargetUrl.Substring( 0, 500 );
			}
			if ( log.RelatedImageUrl != null && log.RelatedImageUrl.Length > 500 )
			{
				truncateMsg += string.Format( "RelatedImageUrl overflow: {0}; ", log.RelatedImageUrl.Length );
				log.RelatedImageUrl = log.RelatedImageUrl.Substring( 0, 500 );
			}
			//if ( log.Referrer.Length > 0 )
			//    log.Comment += ", Referrer: " + log.Referrer;

			//log.Comment += GetUserAgent();

			if ( log.Comment != null && log.Comment.Length > 1000 )
			{
				truncateMsg += string.Format( "Comment overflow: {0}; ", log.Comment.Length );
				log.Comment = log.Comment.Substring( 0, 1000 );
			}

			//the following should not be necessary but getting null related exceptions
			if ( log.TargetUserId == null )
				log.TargetUserId = 0;
			if ( log.ActionByUserId == null )
				log.ActionByUserId = 0;
			if ( log.ActivityObjectId == null )
				log.ActivityObjectId = 0;
			if ( log.ObjectRelatedId == null )
				log.ObjectRelatedId = 0;
			if ( log.TargetObjectId == null )
				log.TargetObjectId = 0;


			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					log.CreatedDate = System.DateTime.Now;
					if ( log.ActivityType == null || log.ActivityType.Length < 5 )
						log.ActivityType = "Audit";

					context.ActivityLog.Add( log );

					// submit the change to database
					count = context.SaveChanges();

					if ( truncateMsg.Length > 0 )
					{
						string msg = string.Format( "ActivityId: {0}, Message: {1}", log.Id, truncateMsg );

						EmailManager.NotifyAdmin( "ActivityLog Field Overflow", msg );
					}
					if ( count > 0 )
					{
						return log.Id;
					}
					else
					{
						//?no info on error
						return 0;
					}
				}
				catch ( Exception ex )
				{

					LoggingHelper.LogError( ex, thisClassName + ".SiteActivityAdd(EFDAL.ActivityLog) ==> trying via proc\n\r" + ex.StackTrace.ToString() );
					//call stored proc as backup!

					//count = ActivityAuditManager.LogActivity( log.ActivityType,
					//	log.Activity,
					//	log.Event, log.Comment,
					//	log.TargetUserId == null ? 0 : ( int ) log.TargetUserId,
					//	log.ActivityObjectId == null ? 0 : ( int ) log.ActivityObjectId,
					//	log.ActionByUserId == null ? 0 : ( int ) log.ActionByUserId,
					//	log.ObjectRelatedId == null ? 0 : ( int ) log.ObjectRelatedId,
					//	log.RelatedImageUrl,
					//	log.RelatedTargetUrl,
					//	log.SessionId,
					//	log.IPAddress,
					//	log.TargetObjectId == null ? 0 : ( int ) log.TargetObjectId,
					//	log.Referrer );

					return count;
				}
			}
		} //
		private void FromMap( SiteActivity from, ActivityLog to )
		{
			to.Id = from.Id;
			to.ActivityType = from.ActivityType;
			to.Activity = from.Activity;
			to.Event = from.Event;
			to.Comment = from.Comment;
			to.TargetUserId = from.TargetUserId;
			to.ActionByUserId = from.ActionByUserId;
			to.ActivityObjectId = from.ActivityObjectId;
			to.ObjectRelatedId = from.ObjectRelatedId;
			to.RelatedImageUrl = from.RelatedImageUrl;
			to.RelatedTargetUrl = from.RelatedTargetUrl;
			to.TargetObjectId = from.TargetObjectId;
			to.SessionId = from.SessionId;
			to.IPAddress = from.IPAddress;
			to.Referrer = from.Referrer;
			to.IsBot = from.IsBot;

		}
		private static void ToMap( ActivityLog from, SiteActivity to )
		{
			to.Id = from.Id;
			to.ActivityType = from.ActivityType;
			to.Activity = from.Activity;
			to.Event = from.Event;
			to.Comment = from.Comment;
			to.TargetUserId = from.TargetUserId;
			to.ActionByUserId = from.ActionByUserId;
			to.ActivityObjectId = from.ActivityObjectId;
			to.ObjectRelatedId = from.ObjectRelatedId;
			to.RelatedImageUrl = from.RelatedImageUrl;
			to.RelatedTargetUrl = from.RelatedTargetUrl;
			to.TargetObjectId = from.TargetObjectId;
			to.SessionId = from.SessionId;
			to.IPAddress = from.IPAddress;
			to.Referrer = from.Referrer;
			to.IsBot = from.IsBot;

		}

		#endregion

		#region Publishing 
		public static List<SiteActivity> GetPublishHistory( string entityType, int entityId )
		{
			SiteActivity entity = new SiteActivity();
			List<SiteActivity> list = new List<SiteActivity>();

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<ActivityLog> results = context.ActivityLog
							.Where( s => s.ActivityType == entityType
									&& s.Activity == "Metadata Registry"
									&& s.ActivityObjectId == entityId)
							.OrderByDescending( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( ActivityLog item in results )
						{
							//probably want something more specific
							entity = new SiteActivity();
							ToMap( item, entity );

							list.Add( entity );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".GetPublishHistory" );
			}
			return list;
		}//

		#endregion

		#region helpers

		public static void StoreLastRequest( string actionComment )
		{
			string sessionKey = GetCurrentSessionId() + "_lastHit";

			try
			{
				if ( HttpContext.Current.Session != null )
				{
					HttpContext.Current.Session[ sessionKey ] = actionComment;
				}
			}
			catch
			{
			}

		} //

		public static bool IsADuplicateRequest( string actionComment )
		{
			string sessionKey = GetCurrentSessionId() + "_lastHit";
			bool isDup = false;
			try
			{
				if ( HttpContext.Current.Session != null )
				{
					string lastAction = HttpContext.Current.Session[ sessionKey ].ToString();
					if ( lastAction.ToLower() == actionComment.ToLower() )
					{
						LoggingHelper.DoTrace( 7, "ActivityServices. Duplicate action: " + actionComment );
						return true;
					}
				}
			}
			catch
			{

			}
			return isDup;
		}
		public static string GetCurrentSessionId()
		{
			string sessionId = "unknown";

			try
			{
				if ( HttpContext.Current.Session != null )
				{
					sessionId = HttpContext.Current.Session.SessionID;
				}
			}
			catch
			{
			}
			return sessionId;
		}

		public static string GetUserIPAddress()
		{
			string ip = "unknown";
			try
			{
				ip = HttpContext.Current.Request.ServerVariables[ "HTTP_X_FORWARDED_FOR" ];
				if ( ip == null || ip == "" || ip.ToLower() == "unknown" )
				{
					ip = HttpContext.Current.Request.ServerVariables[ "REMOTE_ADDR" ];
				}
			}
			catch ( Exception ex )
			{

			}

			return ip;
		} //
		private static string GetUserReferrer()
		{
			string lRefererPage = "";
			try
			{
				if ( HttpContext.Current.Request.UrlReferrer != null )
				{
					lRefererPage = HttpContext.Current.Request.UrlReferrer.ToString();
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
				lRefererPage = "unknown";// ex.Message;
			}

			return lRefererPage;
		} //
		public static string GetUserAgent( ref bool isBot )
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

		#endregion
	}
}
