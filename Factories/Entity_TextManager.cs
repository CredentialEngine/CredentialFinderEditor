using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBentity = Data.Entity_Text;
using ThisEntity = Models.ProfileModels.TextValueProfile;

using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;

namespace Factories
{
	public class Entity_TextManager : BaseFactory
	{
		static string thisClassName = "Entity_TextManager";
		int maxKeywordLength = UtilityManager.GetAppKeyValue( "maxKeywordLength", 200 );

		#region Persistence
		/// <summary>
		/// Persist Entity Reference
		/// </summary>
		/// <param name="profiles"></param>
		/// <param name="parentUid"></param>
		/// <param name="parentTypeId"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <param name="categoryId"></param>
		/// <param name="isTitleRequired">If true, a title must exist</param>
		/// <returns></returns>
		public bool Save( List<ThisEntity> profiles,
				Guid parentUid,
				int parentTypeId,
				int userId,
				ref List<string> messages,
				int categoryId)
		{
			bool isValid = true;
			int intialCount = messages.Count;
			
			if ( !IsValidGuid( parentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}
			if ( parentTypeId == 0 )
			{
				messages.Add( "Error: the parent type was not provided." );
			}
			if ( messages.Count > intialCount )
				return false;

			int count = 0;
		
			if ( profiles == null )
				profiles = new List<ThisEntity>();

			DBentity efEntity = new DBentity();
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}

			using ( var context = new Data.CTIEntities() )
			{
				//check add/updates first
				if ( profiles.Count() > 0 )
				{
					bool isEmpty = false;

					foreach ( ThisEntity entity in profiles )
					{
						entity.CategoryId = categoryId;
						if ( Validate( entity, ref isEmpty, ref  messages ) == false )
						{
							messages.Add( "Reference profile was invalid. "	 );
							isValid = false;
							continue;
						}
						
						if ( isEmpty ) //skip
							continue;

						if ( entity.Id == 0 )
						{
							//add
							efEntity = new DBentity();
							efEntity.EntityId = parent.Id;
							efEntity.CategoryId = entity.CategoryId;
							efEntity.TextValue = entity.TextValue;
							
							efEntity.Created = efEntity.LastUpdated = DateTime.Now;
							efEntity.CreatedById = efEntity.LastUpdatedById = userId;

							context.Entity_Text.Add( efEntity );
							count = context.SaveChanges();
							//update profile record so doesn't get deleted
							entity.Id = efEntity.Id;
							entity.ParentId = parent.Id;

							if ( count == 0 )
							{
								ConsoleMessageHelper.SetConsoleErrorMessage( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.TextTitle ) ? "no description" : entity.TextTitle ) );
							}
							else
							{
								AddConnections( context, parent, entity.Id );
							}

						}
						else
						{
							entity.ParentId = parent.Id;

							efEntity = context.Entity_Text.SingleOrDefault( s => s.Id == entity.Id );
							if ( efEntity != null && efEntity.Id > 0 )
							{
								//update
								efEntity.TextValue = entity.TextValue;
								//has changed?
								if ( HasStateChanged( context ) )
								{
									efEntity.LastUpdated = System.DateTime.Now;
									efEntity.LastUpdatedById = userId;

									count = context.SaveChanges();
								}
							}
						}
					} //foreach
				}
			}

			return isValid;

		} //

		public void AddRelatedConnections( Data.CTIEntities context, Entity parent, int entityTextId )
		{
			System.Threading.ThreadPool.QueueUserWorkItem( delegate
			{
				AddConnections( context, parent, entityTextId );
			} );
		}

		/// <summary>
		/// Add connections
		/// - check parent type
		/// - recursively
		/// </summary>
		/// <param name="recordId"></param>
		/// <param name="statusMessage"></param>
		public void AddConnections( Data.CTIEntities context, Entity parent, int entityTextId )
		{
			string statusMessage = "";

			EM.Entity_ReferenceConnection efEntity = new EM.Entity_ReferenceConnection();
			efEntity.EntityUid = parent.RowId;

			efEntity.EntityReferenceId = entityTextId;
			efEntity.Created = DateTime.Now;
			context.Entity_ReferenceConnection.Add( efEntity );
			int count = context.SaveChanges();
			
			if ( count == 0 )
			{
				LoggingHelper.LogError( thisClassName + string.Format( ".AddConnections. The add failed, there was no error message. Entity.Text.Id: {0}", entityTextId ), true );
			}
		} //

		/// <summary>
		/// Delete a record
		/// </summary>
		/// <param name="recordId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBentity p = context.Entity_Text.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_Text.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "Requested record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;
		} //

		public bool Validate( ThisEntity profile, 
			ref bool isEmpty,
			ref List<string> messages )
		{
			bool isValid = true;

			isEmpty = false;
			//check if empty
			if ( string.IsNullOrWhiteSpace( profile.TextValue )
				)
			{
				isEmpty = true;
				return true;
			}
		//set max for both for now
			//if ( profile.CategoryId == CodesManager.PROPERTY_CATEGORY_KEYWORD )
			//{
				if ( !string.IsNullOrWhiteSpace( profile.TextValue ) && profile.TextValue.Length > maxKeywordLength )
				{
					messages.Add( string.Format( "Error - the keyword must be less than {0} characters.", maxKeywordLength ) );
					isValid = false;
				}
			//}
			

			return isValid;
		}

		#endregion

		#region Retrievals
		/// <summary>
		/// Get all profiles for the parent
		/// Uses the parent Guid to retrieve the related Entity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		public static List<ThisEntity> GetAll( Guid parentUid, int categoryId = 25 )
		{
			ThisEntity entity = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBentity> results = context.Entity_Text
							.Where( s => s.EntityId == parent.Id && s.CategoryId == categoryId )
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity item in results )
						{
							entity = new ThisEntity();
							ToMap( item, entity );

							list.Add( entity );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".GetAll" );
			}
			return list;
		}//

		/// <summary>
		/// quick search for subjects
		/// </summary>
		/// <param name="entityTypeId"></param>
		/// <param name="keyword"></param>
		/// <param name="maxTerms"></param>
		/// <returns></returns>
		public static List<string> QuickSearch_Subjects( int entityTypeId, string keyword, int maxTerms = 0 )
		{
			List<string> list = new List<string>();

			keyword = keyword.Trim();

			if ( maxTerms == 0 )
				maxTerms = 50;

			using ( var context = new ViewContext() )
			{
				// will only return active credentials
				var results = context.Entity_Subjects
					.Where( s => s.EntityTypeId == entityTypeId
						&& s.Subject.Contains( keyword )
						)
					.OrderBy( s => s.Subject )
					.Select( m => m.Subject ).Distinct()
					.Take( maxTerms )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( string item in results )
					{
						list.Add( item );
					}
				}
			}

			return list;
		} //


		private static void ToMap( DBentity from, ThisEntity to )
		{
			to.Id = from.Id;
			to.ParentId = from.EntityId;
			to.CategoryId = from.CategoryId;
			to.TextValue = from.TextValue;
			
			to.ProfileSummary = to.TextValue;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

		}
		#endregion 
	}
}
