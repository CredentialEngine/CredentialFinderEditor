using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Search;
using ThisUser = Models.AppUser;
using Factories;
using Utilities;

using Data;

namespace CTIServices
{
	public class MessageServices
	{

		public static int DELIVER_AS_MESSAGE = 1;
		public static int DELIVER_AS_EMAIL = 2;
		public static int DELIVER_AS_BOTH = 3;

		public int Add( Message message, ref string status )
		{
			status = "";
			int siteAdminId = UtilityManager.GetAppKeyValue( "siteAdminUserId", 1 );
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					if ( message.SenderId == 0 )
						message.SenderId = siteAdminId;
					if ( message.DeliveryMethod == 0 )
						message.DeliveryMethod = 1;

					if (message.ReceiverId < 2)
					{
						status = "Error - a receiver identifier must be provided.<br/>";
					}
					if ((message.Content ?? "").Length < 5)
					{
						status += "Error - the message must be at least 5 characters.<br/>";
					}
					//if errors, log, and return
					//should not happen unless process assumes content was not empty
					if (status.Length > 0)
					{

						return 0;
					}

					message.Sent = DateTime.Now;

					context.Message.Add( message );

					// submit the change to database
					int count = context.SaveChanges();


					if ( count > 0 )
					{
						return message.Id;
					}
					else
					{
						//?no info on error
						return 0;
					}
				}
				catch ( Exception ex )
				{

					LoggingHelper.LogError( ex, "MessageServices.Add() \n\r" + ex.StackTrace.ToString() );

					return 0;
				}
			}
		} //
	}
}
