using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBEntity = Data.Entity_Location;
using DBentityContact = Data.Entity_LocationContactPoint;
using ThisEntity = Models.Common.Entity_Location;
using EntityContactPoint = Models.Common.Entity_LocationContactPoint;

namespace Factories
{
	public class Entity_LocationManager : BaseFactory
	{
		static string thisClassName = "Entity_LocationManager";
		private bool ReturningErrorOnDuplicate { get; set; }
		public Entity_LocationManager()
		{
			ReturningErrorOnDuplicate = false;
		}

		#region Entity_Location

		public int Add( Guid parentUid, int locationId, int userId, ref List<string> messages )
		{
			int id = 0;
			int count = messages.Count();
			if ( locationId == 0 )
			{
				messages.Add( string.Format( "A valid location identifier was not provided to the {0}.EntityLocation_Add method.", thisClassName ) );
				return 0;
			}

			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return 0;
			}
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity efEntity = new DBEntity();
				try
				{
					//first check for duplicates
					efEntity = context.Entity_Location
							.SingleOrDefault( s => s.EntityId == parent.Id && s.LocationId == locationId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						if ( ReturningErrorOnDuplicate )
							messages.Add( string.Format( "Error - this location has already been added to this profile.", thisClassName ) );

						return efEntity.Id;
					}


					efEntity = new DBEntity();
					efEntity.EntityId = parent.Id;
					efEntity.LocationId = locationId;

					efEntity.CreatedById = userId;
					efEntity.Created = System.DateTime.Now;

					context.Entity_Location.Add( efEntity );

					// submit the change to database
					count = context.SaveChanges();
					if ( count > 0 )
					{
						return efEntity.Id;
					}
					else
					{
						//?no info on error
						messages.Add( "Error - the add was not successful." );
						string message = thisClassName + string.Format( ".Add Failed", "Attempted to add a Location for an entity. The process appeared to not work, but there was no exception, so we have no message, or no clue. Parent Profile: {0}, Type: {1}, locationId: {2}, createdById: {3}", parentUid, parent.EntityType, locationId, userId );
						EmailManager.NotifyAdmin( thisClassName + ".Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "Entity_Location" );
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( dbex, thisClassName + string.Format( ".Save(), Parent: {0} ({1})", parent.EntityBaseName, parent.EntityBaseId ) );

				}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex );
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save(), Parent: {0} ({1})", parent.EntityBaseName, parent.EntityBaseId ) );
				}


			}
			return id;
		}

		public bool Delete( int profileId, ref List<string> messages )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity p = context.Entity_Location.FirstOrDefault( s => s.Id == profileId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_Location.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					messages.Add( string.Format( "Requested record was not found: {0}", profileId ));
					isOK = false;
				}
			}
			return isOK;

		}
		#endregion

		#region Entity_LocationContactPoint

		/// <summary>
		/// Add a contact point reference to an entity location reference
		/// </summary>
		/// <param name="entityLocationId"></param>
		/// <param name="entityContactPointId"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public int AddContact( int entityLocationId, int entityContactPointId, int userId, ref List<string> messages )
		{
			int id = 0;
			int count = messages.Count();
			if ( entityLocationId == 0 )
			{
				messages.Add( string.Format( "A valid location identifier was not provided to the {0}.EntityLocation_Add method.", thisClassName ) );
				return 0;
			}
			if ( entityContactPointId == 0 )
			{
				messages.Add( string.Format( "A valid contact point identifier was not provided to the {0}.AddContact method.", thisClassName ) );
				return 0;
			}

			using ( var context = new Data.CTIEntities() )
			{
				DBentityContact efEntity = new DBentityContact();
				try
				{
					//first check for duplicates
					efEntity = context.Entity_LocationContactPoint
							.SingleOrDefault( s => s.EntityLocationId == entityLocationId && s.EntityContactPointId == entityContactPointId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						if ( ReturningErrorOnDuplicate )
							messages.Add( string.Format( "Error - this location has already been added to this profile.", thisClassName ) );

						return efEntity.Id;
					}

					efEntity = new DBentityContact
					{
						EntityLocationId = entityLocationId,
						EntityContactPointId = entityContactPointId,
						CreatedById = userId,
						Created = System.DateTime.Now
					};
					context.Entity_LocationContactPoint.Add( efEntity );

					// submit the change to database
					count = context.SaveChanges();
					if ( count > 0 )
					{
						return efEntity.Id;
					}
					else
					{
						//?no info on error
						messages.Add( "Error - the add was not successful." );
						string message = thisClassName + string.Format( ".Add Failed", "Attempted to add a Contact for a Location. The process appeared to not work, but there was no exception, so we have no message, or no clue. entityLocationId: {0}, entityContactPointId: {1}, createdById: {2}", entityLocationId, entityContactPointId, userId );
						EmailManager.NotifyAdmin( thisClassName + ".Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "Entity_LocationContactPoint" );
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( dbex, thisClassName + string.Format( ".Save(), entityLocationId: {0}, entityContactPointId: {1}, createdById: {2}", entityLocationId, entityContactPointId, userId ) );

				}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex );
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save(), entityLocationId: {0}, entityContactPointId: {1}, createdById: {2}", entityLocationId, entityContactPointId, userId ) );
				}


			}
			return id;
		}

		public bool ContactDelete( int profileId, ref List<string> messages )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBentityContact p = context.Entity_LocationContactPoint.FirstOrDefault( s => s.Id == profileId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_LocationContactPoint.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					messages.Add( string.Format( "Requested record was not found: {0}", profileId ) );
					isOK = false;
				}
			}
			return isOK;

		}
		#endregion
	}
}
