using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Helpers;
using Utilities;

namespace CTIServices
{
	public class EmailServices
	{
		//Format an email address
		public static string FormatEmailAddress(string address, string userName )
		{
			return EmailManager.FormatEmailAddress( address, userName );
		}
		//

		//Send a site email
		public static bool SendSiteEmail( string subject, string message, string cc = "" )
		{
			var toEmail = UtilityManager.GetAppKeyValue( "contactUsMailTo", "email@email.com" );
			var fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", "email@email.com" );
			return SendEmail( toEmail, fromEmail, subject, message, cc );
		}
		//

		//Send a regular email
		public static bool SendEmail( string toEmail, string subject, string message, bool ccAdmin = false )
		{
			var fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", "email@email.com" );
			var cc = ccAdmin ? UtilityManager.GetAppKeyValue( "contactUsMailTo" ) : "";
			return SendEmail( toEmail, fromEmail, subject, message, cc );
		}
		//

		//Notify admin - it's not clear whether or not this needs to save notifications (currently every call to NotifyAdmin goes directly through EmailManager).
		public static bool NotifyAdmin( string subject, string message )
		{
			string emailTo = UtilityManager.GetAppKeyValue( "systemAdminEmail" );
			return NotifyAdmin( emailTo, subject, message );
		}
		//

		public static bool NotifyAdmin( string emailTo, string subject, string message )
		{
			string emailFrom = UtilityManager.GetAppKeyValue( "systemNotifyFromEmail", "TheWatcher@siuccwd.com" );
			string cc = UtilityManager.GetAppKeyValue( "systemAdminEmail" );

			//Logging
			var notification = new Notification()
			{
				ToEmails = emailTo.Split( ',' ).ToList(),
				FromEmail = emailFrom,
				Subject = subject,
				BodyHtml = message,
				Tags = new List<string>() { "Admin Notification" }
			};
			SaveNotificationsForUsers( notification );

			//Send the email
			return EmailManager.NotifyAdmin( emailTo, subject, message );
		}
		//

		public static bool SendEmail( string toEmail, string fromEmail, string subject, string message, string CC = "", string BCC = "" )
		{
			//Send the email
			var successful = EmailManager.SendEmail( toEmail, fromEmail, subject, message, CC, BCC );
			if ( !successful )
			{
				return false;
			}

			//Logging
			var notification = new Notification()
			{
				ToEmails = toEmail.Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries ).ToList().Concat( CC.Split( ',' ).ToList() ).Distinct().ToList(), //Should probably not include BCC
				FromEmail = fromEmail,
				Subject = subject,
				BodyHtml = message
			};
			SaveNotificationsForUsers( notification );

			return true;
		}
		//

		//Save a copy of the notification for each user in the ToEmails list
		public static void SaveNotificationsForUsers( Notification notification )
		{
			try
			{
				//Each user gets their own copy of the email, since this allows per-user "IsRead" tracking and probably other useful things
				foreach ( var userEmail in notification.ToEmails.Where( m => !string.IsNullOrWhiteSpace( m ) ).ToList() )
				{
					var user = AccountServices.GetUserByEmail( userEmail );
					if ( user != null && user.Id > 0 )
					{
						notification.ForAccountRowId = user.RowId;
						var temp = UtilityManager.SimpleMap<Notification>( notification );
						temp.ForAccountRowId = ( AccountServices.GetUserByEmail( userEmail ) ?? new Models.AppUser() ).RowId;
						NotificationServices.Save( temp );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "SaveNotificationsForUsers - Exception occurred ", true );
			}
		}

	}
	//
}
