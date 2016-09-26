using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBentity = Data.Entity_Competency;
using Entity = Models.Common.CredentialAlignmentObjectProfile;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;

namespace Factories
{
	public class Entity_CompetencyManager : BaseFactory
	{
		static string thisClassName = "Entity_CompetencyManager";
		#region Persistance ===================

		/// <summary>
		/// Add/Update a competency
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool Save( Entity entity, 
				Guid parentUid, 
				int userId, 
				ref List<string> messages )
		{
			bool isValid = true;
			int intialCount = messages.Count;

			if ( !IsValidGuid( parentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}

			if ( messages.Count > intialCount )
				return false;

			int count = 0;

			DBentity efEntity = new DBentity();

			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}

			using ( var context = new Data.CTIEntities() )
			{

				bool isEmpty = false;

				if ( ValidateProfile( entity, ref isEmpty, ref  messages ) == false )
				{
					return false;
				}
				if ( isEmpty )
				{
					messages.Add( "The Competency Profile is empty. "  );
					return false;
				}

				if ( entity.Id == 0 )
				{
					//add
					efEntity = new DBentity();
					FromMap( entity, efEntity );
					efEntity.EntityId = parent.Id;

					efEntity.Created = efEntity.LastUpdated = DateTime.Now;
					efEntity.CreatedById = efEntity.LastUpdatedById = userId;
					efEntity.RowId = Guid.NewGuid();

					context.Entity_Competency.Add( efEntity );
					count = context.SaveChanges();
					//update profile record so doesn't get deleted
					entity.Id = efEntity.Id;
					entity.ParentId = parent.Id;
					entity.RowId = efEntity.RowId;
					if ( count == 0 )
					{
						messages.Add( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.Name ) ? "no description" : entity.Description ) );
					}
					
				}
				else
				{
					entity.ParentId = parent.Id;

					efEntity = context.Entity_Competency.SingleOrDefault( s => s.Id == entity.Id );
					if ( efEntity != null && efEntity.Id > 0 )
					{
						entity.RowId = efEntity.RowId;
						//update
						FromMap( entity, efEntity );
						//has changed?
						if ( HasStateChanged( context ) )
						{
							efEntity.LastUpdated = System.DateTime.Now;
							efEntity.LastUpdatedById = userId;

							count = context.SaveChanges();
						}
					}
				}
			}

			return isValid;
		}


		/// <summary>
		/// Persist Competencies
		/// </summary>
		/// <param name="profiles"></param>
		/// <param name="parentUid"></param>
		/// <param name="parentTypeId"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		//public bool Update( List<Enumeration> profiles, Guid parentUid, int parentTypeId, int userId, ref List<string> messages )
		//{
		//	bool isValid = true;
		//	int intialCount = messages.Count;

		//	if ( !IsValidGuid( parentUid ) )
		//	{
		//		messages.Add( "Error: the parent identifier was not provided." );
		//	}
		//	if ( parentTypeId == 0 )
		//	{
		//		messages.Add( "Error: the parent type was not provided." );
		//	}
		//	if ( messages.Count > intialCount )
		//		return false;

		//	int count = 0;
		//	bool hasData = false;
		//	if ( profiles == null )
		//		profiles = new List<Enumeration>();

		//	DBentity efEntity = new DBentity();

		//	Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
		//	if ( parent == null || parent.Id == 0 )
		//	{
		//		messages.Add( "Error - the parent entity was not found." );
		//		return false;
		//	}

		//	return isValid;
		//}
		//public bool Update( List<Entity> profiles, Guid parentUid, int parentTypeId, int userId, ref List<string> messages )
		//{
		//	bool isValid = true;
		//	int intialCount = messages.Count;

		//	if ( !IsValidGuid( parentUid ) )
		//	{
		//		messages.Add( "Error: the parent identifier was not provided." );
		//	}
		//	if ( parentTypeId == 0 )
		//	{
		//		messages.Add( "Error: the parent type was not provided." );
		//	}
		//	if ( messages.Count > intialCount )
		//		return false;

		//	int count = 0;
		//	bool hasData = false;
		//	if ( profiles == null )
		//		profiles = new List<Entity>();

		//	DBentity efEntity = new DBentity();

		//	Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
		//	if ( parent == null || parent.Id == 0 )
		//	{
		//		messages.Add( "Error - the parent entity was not found." );
		//		return false;
		//	}
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		//check add/updates first
		//		if ( profiles.Count() > 0 )
		//		{
		//			hasData = true;
		//			bool isEmpty = false;

		//			foreach ( Entity entity in profiles )
		//			{
		//				if ( ValidateProfile( entity, ref isEmpty, ref  messages ) == false )
		//				{
		//					messages.Add( "Competency was invalid. " + SetEntitySummary( entity ) );
		//					continue;
		//				}
		//				if ( isEmpty ) //skip
		//					continue;

		//				if ( entity.Id == 0 )
		//				{
		//					//add
		//					efEntity = new DBentity();
		//					FromMap( entity, efEntity );
		//					efEntity.EntityId = parent.Id;

		//					efEntity.Created = efEntity.LastUpdated = DateTime.Now;
		//					efEntity.CreatedById = efEntity.LastUpdatedById = userId;
		//					efEntity.RowId = Guid.NewGuid();

		//					context.Entity_Competency.Add( efEntity );
		//					count = context.SaveChanges();
		//					//update profile record so doesn't get deleted
		//					entity.Id = efEntity.Id;
		//					entity.ParentId = parent.Id;
		//					entity.RowId = efEntity.RowId;
		//					if ( count == 0 )
		//					{
		//						ConsoleMessageHelper.SetConsoleErrorMessage( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.ProfileName ) ? "no description" : entity.ProfileName ) );
		//					}
		//				}
		//				else
		//				{
		//					entity.ParentId = parent.Id;

		//					efEntity = context.Entity_Competency.SingleOrDefault( s => s.Id == entity.Id );
		//					if ( efEntity != null && efEntity.Id > 0 )
		//					{
		//						entity.RowId = efEntity.RowId;
		//						//update
		//						FromMap( entity, efEntity );
		//						//has changed?
		//						if ( HasStateChanged( context ) )
		//						{
		//							efEntity.LastUpdated = System.DateTime.Now;
		//							efEntity.LastUpdatedById = userId;

		//							count = context.SaveChanges();
		//						}
		//					}

		//				}

		//			} //foreach

		//		}

		//		//check for deletes ====================================
		//		//need to ensure ones just added don't get deleted

		//		//get existing 
		//		List<DBentity> results = context.Entity_Competency
		//				.Where( s => s.EntityId == parent.Id )
		//				.OrderBy( s => s.Id )
		//				.ToList();

		//		//if profiles is null, need to delete all!!
		//		if ( results.Count() > 0 && profiles.Count() == 0 )
		//		{
		//			foreach ( var item in results )
		//				context.Entity_Competency.Remove( item );

		//			context.SaveChanges();
		//		}
		//		else
		//		{
		//			//deletes should be direct??
		//			//should only have existing ids, where not in current list, so should be deletes
		//			var deleteList = from existing in results
		//							 join item in profiles
		//									 on existing.Id equals item.Id
		//									 into joinTable
		//							 from result in joinTable.DefaultIfEmpty( new Entity { Id = 0, ParentId = 0 } )
		//							 select new { DeleteId = existing.Id, ParentId = ( result.ParentId ) };

		//			foreach ( var v in deleteList )
		//			{
		//				if ( v.ParentId == 0 )
		//				{
		//					//delete item
		//					DBentity p = context.Entity_Competency.FirstOrDefault( s => s.Id == v.DeleteId );
		//					if ( p != null && p.Id > 0 )
		//					{
		//						context.Entity_Competency.Remove( p );
		//						count = context.SaveChanges();
		//					}
		//				}
		//			}
		//		}

		//	}

		//	return isValid;
		//}
		/// <summary>
		/// Delete a competency
		/// </summary>
		/// <param name="recordId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBentity p = context.Entity_Competency.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_Competency.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "Task Profile record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;

		}


		public bool ValidateProfile( Entity profile, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;

			isEmpty = false;
			//check if empty
			if ( string.IsNullOrWhiteSpace( profile.Name )
				&& string.IsNullOrWhiteSpace( profile.Description )
				&& string.IsNullOrWhiteSpace( profile.TargetDescription )
				)
			{
				isEmpty = true;
				return isValid;
			}

			if ( string.IsNullOrWhiteSpace( profile.Name )
				&& string.IsNullOrWhiteSpace( profile.Description ) )
			{
				messages.Add( "A competency name or description must be entered" );
				isValid = false;
			}
			//if ( string.IsNullOrWhiteSpace( profile.Description ) )
			//{
			//	messages.Add( "A competency Description must be entered" );
			//	isValid = false;
			//}

			return isValid;
		}

		#endregion
		#region  retrieval ==================

		/// <summary>
		/// Get all Task profiles for the parent
		/// Uses the parent Guid to retrieve the related Entity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="alignmentType">If blank, get all types</param>
		public static List<Entity> GetAll( Guid parentUid, string alignmentType )
		{
			Entity entity = new Entity();
			List<Entity> list = new List<Entity>();
			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBentity> results = context.Entity_Competency
							.Where( s => s.EntityId == parent.Id
							&& ( alignmentType == "" || s.AlignmentType == alignmentType ) )
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity item in results )
						{
							entity = new Entity();
							ToMap( item, entity, true );
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
		/// Get a competency record
		/// </summary>
		/// <param name="profileId"></param>
		/// <returns></returns>
		public static Entity Get( int profileId )
		{
			Entity entity = new Entity();
			if ( profileId == 0 )
				return entity;
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentity item = context.Entity_Competency
							.SingleOrDefault( s => s.Id == profileId );

					if ( item != null && item.Id > 0 )
					{
						ToMap( item, entity, true );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Get" );
			}
			return entity;
		}//


		public static void FromMap( Entity from, DBentity to )
		{
			//want to ensure fields from create are not wiped
			if ( to.Id == 0 )
			{
				if ( IsValidDate( from.Created ) )
					to.Created = from.Created;
				to.CreatedById = from.CreatedById;
			}
			//to.Id = from.Id;

			to.Name = from.Name;
			to.Description = from.Description;
			to.CodedNotation = from.CodedNotation;
			to.EducationalFramework = from.EducationalFramework;

			to.TargetName = from.TargetName;
			to.TargetDescription = from.TargetDescription;
			to.TargetUrl = from.TargetUrl;

			to.AlignmentType = from.AlignmentType;

		}
		public static void ToMap( DBentity from, Entity to, bool includingItems = true )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;

			to.Name = from.Name;
			to.Description = from.Description;
			to.CodedNotation = from.CodedNotation;
			to.EducationalFramework = from.EducationalFramework;

			to.TargetName = from.TargetName;
			to.TargetDescription = from.TargetDescription;
			to.TargetUrl = from.TargetUrl;

			if ( !string.IsNullOrWhiteSpace( to.Name ) )
				to.ProfileName = to.Name;
			else if ( !string.IsNullOrWhiteSpace( to.TargetUrl ) )
				to.ProfileName = to.TargetUrl;
			else if ( !string.IsNullOrWhiteSpace( to.Description ) )
			{
				to.ProfileName = to.Description.Length > 200 ? to.Description.Substring(0, 200) +  " ..." : to.Description;
			}
			else
				to.ProfileName = "Competency";

			to.AlignmentType = from.AlignmentType;

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
