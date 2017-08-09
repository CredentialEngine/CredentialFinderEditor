using System;
using System.Collections.Generic;
using System.Linq;

using Models.Common;
using Utilities;
using DBentity = Data.Entity_Competency;
using ThisEntity = Models.ProfileModels.Entity_Competency;

namespace Factories
{
	public class Entity_CompetencyManager : BaseFactory
	{
		static string thisClassName = "Entity_CompetencyManager";
		#region Persistance ===================

		/// <summary>
		/// Add a competency
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool Save( ThisEntity entity,
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

			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}

			using ( var context = new Data.CTIEntities() )
			{
				if ( ValidateProfile( entity, ref messages ) == false )
				{
					return false;
				}

				if ( entity.Id == 0 )
				{
					//check if already exists
					//TODO - will need to add alignment type
					ThisEntity item = Get( parent.Id, entity.CompetencyId, "" );
					if ( entity != null && entity.Id > 0 )
					{
						messages.Add( string.Format("Error: the selected competency {0} already exists!", entity.FrameworkCompetency.Name) );
						return false;
					}
					//add
					efEntity = new DBentity();
					efEntity.EntityId = parent.Id;
					efEntity.CompetencyId = entity.CompetencyId;
					efEntity.Created = DateTime.Now;
					efEntity.CreatedById = userId;

					context.Entity_Competency.Add( efEntity );
					count = context.SaveChanges();
					//update profile record so doesn't get deleted
					entity.Id = efEntity.Id;
					entity.EntityId = parent.Id;

					if ( count == 0 )
					{
						messages.Add( string.Format( " Unable to add Competency: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.FrameworkCompetency.Name ) ? "no description" : entity.FrameworkCompetency.Description ) );
					}

				}
				else
				{
					//no update possible at this time
					entity.EntityId = parent.Id;

					efEntity = context.Entity_Competency.SingleOrDefault( s => s.Id == entity.Id );
					if ( efEntity != null && efEntity.Id > 0 )
					{
						//update
						efEntity.CompetencyId = entity.CompetencyId;
						//has changed?
						if ( HasStateChanged( context ) )
						{
							count = context.SaveChanges();
						}
					}
				}
			}

			return isValid;
		}

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
					statusMessage = string.Format( "Entity Competency record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;
		}


		public bool ValidateProfile( ThisEntity profile, ref List<string> messages )
		{
			bool isValid = true;

			if ( profile.CompetencyId < 1 )
			{
				messages.Add( "A competency identifier must be included." );
				isValid = false;
			}
			
			return isValid;
		}

		#endregion
		#region  retrieval ==================

		/// <summary>
		/// Get all records for the parent
		/// Uses the parent Guid to retrieve the related ThisEntity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="alignmentType">If blank, get all types</param>
		public static List<ThisEntity> GetAll( Guid parentUid, string alignmentType )
		{
			ThisEntity entity = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			CredentialAlignmentObjectItemProfile ao = new CredentialAlignmentObjectItemProfile();

			Entity parent = EntityManager.GetEntity( parentUid );
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
							)
							.OrderBy( s => s.EducationFramework_Competency.EducationFramework.Name )
							.ThenBy( s => s.EducationFramework_Competency.Name)
							.ToList();
					//&& ( alignmentType == "" || s.AlignmentType == alignmentType ) 
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

		public static List<CredentialAlignmentObjectItemProfile> GetAllAsAlignmentObjects( Guid parentUid, string alignmentType )
		{
			//ThisEntity entity = new ThisEntity();
			List<CredentialAlignmentObjectItemProfile> list = new List<CredentialAlignmentObjectItemProfile>();
			CredentialAlignmentObjectItemProfile entity = new CredentialAlignmentObjectItemProfile();

			Entity parent = EntityManager.GetEntity( parentUid );
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
							)
							.OrderBy( s => s.EducationFramework_Competency.EducationFramework.Name )
							.ThenBy( s => s.EducationFramework_Competency.Name )
							.ToList();
					//&& ( alignmentType == "" || s.AlignmentType == alignmentType ) 
					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity item in results )
						{
							entity = new CredentialAlignmentObjectItemProfile();
							ToMapAsAlignmentObjects( item, entity );
							list.Add( entity );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".GetAllAsAlignmentObjects" );
			}
			return list;
		}//

		/// <summary>
		/// Get a competency record
		/// </summary>
		/// <param name="profileId"></param>
		/// <returns></returns>
		public static ThisEntity Get( int profileId )
		{
			ThisEntity entity = new ThisEntity();
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
						ToMap( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Get" );
			}
			return entity;
		}//

		/// <summary>
		/// Get entity to determine if one exists for the entity and alignment type
		/// </summary>
		/// <param name="entityId"></param>
		/// <param name="competencyId"></param>
		/// <param name="alignmentType"></param>
		/// <returns></returns>
		public static ThisEntity Get( int entityId, int competencyId, string alignmentType )
		{
			ThisEntity entity = new ThisEntity();
			if ( competencyId == 0 )
				return entity;
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentity item = context.Entity_Competency
							.SingleOrDefault( s => s.EntityId == entityId && s.CompetencyId == competencyId );

					if ( item != null && item.Id > 0 )
					{
						ToMap( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Get" );
			}
			return entity;
		}//

		public static void ToMap( DBentity from, ThisEntity to )
		{
			to.Id = from.Id;
			to.EntityId = from.EntityId;

			to.CompetencyId = from.CompetencyId;
			

			//to.AlignmentTypeId = from.AlignmentTypeId ?? 0;
			//to.AlignmentType = from.AlignmentType;


			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;

			EducationFrameworkManager.ToMap( from.EducationFramework_Competency, to.FrameworkCompetency );

			EducationFrameworkManager.ToMap( from.EducationFramework_Competency.EducationFramework, to.FrameworkCompetency.EducationFramework );

		}

		public static void ToMapAsAlignmentObjects( DBentity from, CredentialAlignmentObjectItemProfile to )
		{
			to.Id = from.Id;
			to.ParentId = from.EntityId;

			to.CompetencyId = from.CompetencyId;


			//to.AlignmentTypeId = from.AlignmentTypeId ?? 0;
			//to.AlignmentType = from.AlignmentType;


			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;



			to.Name = StripJqueryTag( from.EducationFramework_Competency.Name );
			to.TargetUrl = from.EducationFramework_Competency.Url;
			to.RepositoryUri = from.EducationFramework_Competency.RepositoryUri;
			to.Description = from.EducationFramework_Competency.Description;
			to.CodedNotation = from.EducationFramework_Competency.CodedNotation;
		}

		#endregion

	}
}
