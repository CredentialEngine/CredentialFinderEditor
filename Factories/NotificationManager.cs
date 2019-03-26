using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Helpers;
using Utilities;
using DBEntity = Data.Notification;
using ThisEntity = Models.Helpers.Notification;
using Context = Data.CTIEntities;
using Newtonsoft.Json;

namespace Factories
{
	public class NotificationManager
	{
		//Map an object from the DB
		public static ThisEntity MapFromDB( DBEntity data )
		{
			//Basic mapping
			var result = UtilityManager.SimpleMap<ThisEntity>( data ) ?? new ThisEntity();
			//Custom mapping
			result.ToEmails = JsonConvert.DeserializeObject<List<string>>( data.ToEmails ?? "[]" );
			result.Tags = JsonConvert.DeserializeObject<List<string>>( data.Tags ?? "[]" );
			//Return data
			return result;
		}
		//

		//Map a list of objects from the DB
		public static List<ThisEntity> MapFromDB( List<DBEntity> data )
		{
			var result = new List<ThisEntity>();
			foreach( var item in data.Where(m => m != null ).ToList() )
			{
				result.Add( MapFromDB( item ) );
			}
			return result.OrderByDescending( m => m.Created ).ToList();
		}
		//

		//Map an object to the DB
		public static DBEntity MapToDB( ThisEntity data )
		{
			//Basic mapping
			var result = UtilityManager.SimpleMap<DBEntity>( data );
			//Custom mapping
			result.ToEmails = JsonConvert.SerializeObject( data.ToEmails ?? new List<string>() );
			result.Tags = JsonConvert.SerializeObject( data.Tags ?? new List<string>() );
			//Return data
			return result;
		}
		//

		//Get by integer Id
		public static ThisEntity GetById( int id )
		{
			using ( var context = new Context() )
			{
				var data = context.Notification.FirstOrDefault( m => m.Id == id );
				return MapFromDB( data );
			}
		}
		//

		//Get by RowId
		public static ThisEntity GetByRowId ( Guid rowID )
		{
			using ( var context = new Context() )
			{
				var data = context.Notification.FirstOrDefault( m => m.RowId == rowID );
				return MapFromDB( data );
			}
		}
		//

		//Get by ForAccountRowId
		public static List<ThisEntity> GetAllForAccountRowId ( Guid accountRowID )
		{
			using ( var context = new Context() )
			{
				var data = context.Notification.Where( m => m.ForAccountRowId == accountRowID ).ToList();
				return MapFromDB( data );
			}
		}
		//

		//Get all notifications sent to a given email address
		public static List<ThisEntity> GetAllForRecipientEmailAddress( string emailAddress )
		{
			using( var context = new Context() )
			{
				var data = context.Notification.Where( m => m.ToEmails.Contains( emailAddress ) ).ToList();
				return MapFromDB( data );
			}
		}
		//

		//Search
		public static List<ThisEntity> Search( NotificationQuery query, ref int totalResults )
		{
			var results = new List<ThisEntity>();
			query.Keywords = query.Keywords ?? "";
			query.PageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;

			using ( var context = new Context() )
			{
				//Startup
				var q = context.Notification.Where( m => m != null );

				//ForAccountRowId
				if( query.ForAccountRowId != null && query.ForAccountRowId != Guid.Empty )
				{
					q = q.Where( m => m.ForAccountRowId == query.ForAccountRowId );
				}

				//ToEmails
			if ( !string.IsNullOrWhiteSpace( query.ToEmails ) )
				{
					q = q.Where( m => m.ToEmails.Contains( query.ToEmails ) );
				}

				//Keywords
				if ( !string.IsNullOrWhiteSpace( query.Keywords ) )
				{
					q = q.Where( m =>
						 m.Subject.Contains( query.Keywords ) ||
						 m.FromEmail.Contains( query.Keywords ) ||
						 m.ToEmails.Contains( query.Keywords ) ||
						 m.BodyHtml.Contains( query.Keywords ) ||
						 m.Tags.Contains( query.Keywords )
					);
				}

				//Do the query
				totalResults = q.Count();
				if( query.PageSize <= 0 )
				{
					results = MapFromDB( q.ToList() );
				}
				else
				{
					var skip = query.PageSize * (query.PageNumber -1);
					results = MapFromDB( q.OrderBy(x=>x.Subject).Skip( skip ).Take( query.PageSize ).ToList() );
				}
			}

			return results;
		}
		//

		//Create or update
		public static ThisEntity Save( ThisEntity input )
		{
			//Assign GUID if necessary
			input.RowId = input.RowId == null || input.RowId == Guid.Empty ? Guid.NewGuid() : input.RowId;

			//Create or update
			using ( var context = new Context() )
			{
				var existing = context.Notification.FirstOrDefault( m => m.RowId == input.RowId );
				//New
				if ( existing == null )
				{
					input.Created = DateTime.Now;
					input.LastUpdated = DateTime.Now;
					var toSave = MapToDB( input );
					context.Notification.Add( toSave );
					context.SaveChanges();
				}
				//Existing
				else
				{
					input.LastUpdated = DateTime.Now;
					var toSave = MapToDB( input ); //Handles special fields, otherwise this would be redundant with SimpleUpdate
					UtilityManager.SimpleUpdate( toSave, existing, false );
					context.SaveChanges();
				}
			}

			//Return the newly-saved object
			return GetByRowId( input.RowId );
		}
		//

		//Delete
		public static void Delete(Guid rowID )
		{
			using ( var context = new Context() )
			{
				var match = context.Notification.FirstOrDefault( m => m.RowId == rowID );
				context.Notification.Remove( match );
				context.SaveChanges();
			}
		}
		//
	}
}
