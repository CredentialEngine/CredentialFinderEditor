using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Helpers;
using Factories;

namespace CTIServices
{
	public class NotificationServices
	{
		//Get by integer Id
		public static Notification GetById( int id )
		{
			return NotificationManager.GetById( id );
		}
		//

		//Get by RowId
		public static Notification GetByRowId( Guid rowID )
		{
			return NotificationManager.GetByRowId( rowID );
		}
		//

		//Get by ForAccountRowId
		public static List<Notification> GetAllForAccountRowId( Guid accountRowID )
		{
			return NotificationManager.GetAllForAccountRowId( accountRowID );
		}
		//

		//Get all notifications sent to a given email address
		public static List<Notification> GetAllForRecipientEmailAddress( string emailAddress )
		{
			return NotificationManager.GetAllForRecipientEmailAddress( emailAddress );
		}
		//

		public static List<Notification> Search( NotificationQuery query, ref int totalResults )
		{
			return NotificationManager.Search( query, ref totalResults );
		}
		//

		//Create or update
		public static Notification Save( Notification input )
		{
			return NotificationManager.Save( input );
		}
		//

		//Delete
		public static void Delete( Guid rowID )
		{
			NotificationManager.Delete( rowID );
		}
		//

	}
}
