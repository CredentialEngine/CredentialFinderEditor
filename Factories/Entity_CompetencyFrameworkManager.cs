using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBentity = Data.Entity_CompetencyFramework;
using DBentityItem = Data.Entity_CompetencyFrameworkItem;
using ThisEntity = Models.Common.CredentialAlignmentObjectFrameworkProfile;
using Views = Data.Views;
using ThisEntityItem = Models.Common.CredentialAlignmentObjectItemProfile;
using ViewContext = Data.Views.CTIEntities1;

namespace Factories
{
	public class Entity_CompetencyFrameworkManager : BaseFactory
	{
		static string thisClassName = "Entity_CompetencyFrameworkManager";
		#region --- Entity_CompetencyFramework ---
		#region Persistance ===================

		/// <summary>
		/// Add/Update a competency
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

			if ( !IsValidGuid( parentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}

			if ( messages.Count > 0 )
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

				bool isEmpty = false;

				if ( ValidateProfile( entity, ref isEmpty, ref  messages ) == false )
				{
					return false;
				}
				if ( isEmpty )
				{
					messages.Add( "The Competency Framework Profile is empty. " );
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

					context.Entity_CompetencyFramework.Add( efEntity );

					count = context.SaveChanges();

					entity.Id = efEntity.Id;
					entity.ParentId = parent.Id;
					entity.RowId = efEntity.RowId;
					if ( count == 0 )
					{
						messages.Add( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.EducationalFrameworkName ) ? "no description" : entity.EducationalFrameworkName ) );
					}
				}
				else
				{
					entity.ParentId = parent.Id;

					efEntity = context.Entity_CompetencyFramework.SingleOrDefault( s => s.Id == entity.Id );
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
				DBentity p = context.Entity_CompetencyFramework.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_CompetencyFramework.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "The record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;

		}


		public bool ValidateProfile( ThisEntity profile, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;

			isEmpty = false;
			//check if empty
			//if ( string.IsNullOrWhiteSpace( profile.EducationalFrameworkName )
			//	&& string.IsNullOrWhiteSpace( profile.EducationalFrameworkUrl )
			//	)
			//{
			//	isEmpty = true;
			//	return isValid;
			//}

			if ( string.IsNullOrWhiteSpace( profile.EducationalFrameworkName ) )
			{
				messages.Add( "An educational framework name must be entered" );
				isValid = false;
			}

			//could check for alignment type, but this is typically set by context, and not entered.
			return isValid;
		}

		#endregion
		#region  retrieval ==================

		/// <summary>
		/// Get all Task profiles for the parent
		/// Uses the parent Guid to retrieve the related ThisEntity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="alignmentType">If blank, get all types.</param>
		public static List<ThisEntity> GetAll( Guid parentUid, string alignmentType )
		{
			ThisEntity entity = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			//Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBentity> results = context.Entity_CompetencyFramework
							.Where( s => s.EntityId == parent.Id
							&& ( alignmentType == "" || s.AlignmentType == alignmentType ) )
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity item in results )
						{
							entity = new ThisEntity();
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
		public static ThisEntity Get( int profileId )
		{
			ThisEntity entity = new ThisEntity();
			if ( profileId == 0 )
				return entity;
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentity item = context.Entity_CompetencyFramework
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


		public static void FromMap( ThisEntity from, DBentity to )
		{
			//want to ensure fields from create are not wiped
			//to.Id = from.Id;

			to.EducationalFrameworkName = from.EducationalFrameworkName;
			to.EducationalFrameworkUrl = from.EducationalFrameworkUrl;

			//TODO work to eliminate
			to.AlignmentType = from.AlignmentType;
			if ( from.AlignmentTypeId > 0 )
				to.AlignmentTypeId = from.AlignmentTypeId;
			else if ( !string.IsNullOrWhiteSpace( to.AlignmentType ) )
			{
				CodeItem item = CodesManager.Codes_PropertyValue_Get( CodesManager.PROPERTY_CATEGORY_ALIGNMENT_TYPE, to.AlignmentType );
				if ( item != null && item.Id > 0 )
					to.AlignmentTypeId = item.Id;
			}

		}
		public static void ToMap( DBentity from, ThisEntity to, bool includingItems = true )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;

			to.ProfileName = from.EducationalFrameworkName;
			if ( from.EducationalFrameworkName == "*** new profile ***" )
				to.EducationalFrameworkName = "";
			else 
				to.EducationalFrameworkName = from.EducationalFrameworkName;

			to.EducationalFrameworkUrl = from.EducationalFrameworkUrl;

			to.AlignmentTypeId = from.AlignmentTypeId ?? 0;
			to.AlignmentType = from.AlignmentType;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

			ThisEntityItem ip = new ThisEntityItem();
			//get all competencies - as profile links?
			foreach (DBentityItem item in from.Entity_CompetencyFrameworkItem) 
			{
				ip = new ThisEntityItem();
				ip.Id = item.Id;
				ip.ParentId = item.EntityFrameworkId;

				ip.Name = item.Name;
				ip.Description = item.Description;
				ip.TargetName  = item.TargetName;
				ip.TargetDescription = item.TargetDescription;
				ip.TargetUrl = item.TargetUrl;
				ip.CodedNotation = item.CodedNotation;

				to.Items.Add(ip);
			}

		}

		#endregion
		#endregion


		#region --- Entity_CompetencyFrameworkItem ---

		#region Persistance ===================

		/// <summary>
		/// Add/Update a competency
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool Entity_Competency_Save( ThisEntityItem entity,
				int userId,
				ref List<string> messages )
		{
			bool isValid = true;
			int intialCount = messages.Count;

			if ( entity.ParentId == 0 )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}

			if ( messages.Count > intialCount )
				return false;

			int count = 0;

			DBentityItem efEntity = new DBentityItem();

			using ( var context = new Data.CTIEntities() )
			{

				bool isEmpty = false;

				if ( ValidateProfile( entity, ref isEmpty, ref  messages ) == false )
				{
					return false;
				}
				if ( isEmpty )
				{
					messages.Add( "The Competency Profile is empty. " );
					return false;
				}

				if ( entity.Id == 0 )
				{
					//add
					efEntity = new DBentityItem();
					FromMap( entity, efEntity );

					efEntity.Created = efEntity.LastUpdated = DateTime.Now;
					efEntity.CreatedById = efEntity.LastUpdatedById = userId;
					efEntity.RowId = Guid.NewGuid();

					context.Entity_CompetencyFrameworkItem.Add( efEntity );

					count = context.SaveChanges();

					entity.Id = efEntity.Id;
					entity.RowId = efEntity.RowId;
					if ( count == 0 )
					{
						messages.Add( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.Name ) ? "no description" : entity.Description ) );
					}

				}
				else
				{

					efEntity = context.Entity_CompetencyFrameworkItem.SingleOrDefault( s => s.Id == entity.Id );
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
		/// Delete a competency
		/// </summary>
		/// <param name="recordId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Entity_Competency_Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBentityItem p = context.Entity_CompetencyFrameworkItem.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_CompetencyFrameworkItem.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "The record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;

		}


		public bool ValidateProfile( ThisEntityItem profile, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;

			isEmpty = false;
			//check if empty
			if ( string.IsNullOrWhiteSpace( profile.Name )
				&& string.IsNullOrWhiteSpace( profile.Description )
				&& string.IsNullOrWhiteSpace( profile.TargetName )
				&& string.IsNullOrWhiteSpace( profile.TargetDescription )
				)
			{
				messages.Add( "Please enter, at minimum, a competency name." );
				//isEmpty = true;
				return false;
			}

			if ( string.IsNullOrWhiteSpace( profile.Name ) )
			{
				messages.Add( "A competency name must be entered" );
				isValid = false;
			}

			if ( !string.IsNullOrWhiteSpace( profile.AlignmentDate )
	&& !IsValidDate( profile.AlignmentDate ) )
			{
				messages.Add( "Please enter a valid alignment date" );
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
		/// Uses the parent Guid to retrieve the related ThisEntityItem, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="alignmentType">If blank, get all types</param>
		public static List<ThisEntityItem> Entity_Competency_GetAll( int frameworkId )
		{
			ThisEntityItem entity = new ThisEntityItem();
			List<ThisEntityItem> list = new List<ThisEntityItem>();

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBentityItem> results = context.Entity_CompetencyFrameworkItem
							.Where( s => s.EntityFrameworkId == frameworkId )
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentityItem item in results )
						{
							entity = new ThisEntityItem();
							ToMap( item, entity, true );
							list.Add( entity );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Entity_Competency_GetAll" );
			}
			return list;
		}//

		/// <summary>
		/// Get a competency record
		/// </summary>
		/// <param name="profileId"></param>
		/// <returns></returns>
		public static ThisEntityItem Entity_Competency_Get( int profileId )
		{
			ThisEntityItem entity = new ThisEntityItem();
			if ( profileId == 0 )
				return entity;
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentityItem item = context.Entity_CompetencyFrameworkItem
							.SingleOrDefault( s => s.Id == profileId );

					if ( item != null && item.Id > 0 )
					{
						ToMap( item, entity, true );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Entity_Competency_Get" );
			}
			return entity;
		}//


		public static void FromMap( ThisEntityItem from, DBentityItem to )
		{
			//want to ensure fields from create are not wiped
			//to.Id = from.Id;
			to.EntityFrameworkId = from.ParentId;
			to.Name = from.Name;
			to.Description = from.Description;
			to.TargetName = from.TargetName;
			to.TargetDescription = from.TargetDescription;
			to.TargetUrl = from.TargetUrl;
			to.CodedNotation = from.CodedNotation;

			if ( IsValidDate( from.AlignmentDate ) )
				to.AlignmentDate = DateTime.Parse( from.AlignmentDate );
			else
				to.AlignmentDate = null;

		}
		public static void ToMap( DBentityItem from, ThisEntityItem to, bool includingItems = true )
		{
			to.Id = from.Id;
			to.ParentId = from.EntityFrameworkId;
			to.RowId = from.RowId;

			to.Name = from.Name;
			to.Description = from.Description;
			to.TargetName = from.TargetName;
			to.TargetDescription = from.TargetDescription;
			to.TargetUrl = from.TargetUrl;
			to.CodedNotation = from.CodedNotation;

			if ( IsValidDate( from.AlignmentDate ) )
				to.AlignmentDate = ( ( DateTime ) from.AlignmentDate ).ToShortDateString();
			else
				to.AlignmentDate = "";

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

		}

		#endregion

		#endregion
	}
}
