using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBentity = Data.EducationFramework;
using DBentityItem = Data.EducationFramework_Competency;
using ThisEntity = Models.ProfileModels.EducationFramework;
using Views = Data.Views;
using ThisEntityItem = Models.ProfileModels.EducationFrameworkCompetency;
using Models.Helpers.Cass;

namespace Factories
{
	public class EducationFrameworkManager : BaseFactory
	{
		static string thisClassName = "EducationFrameworkManager";
		#region --- EducationFrameworkManager ---
		#region Persistance ===================
		/// <summary>
		/// Check if the provided framework has already been sync'd. 
		/// If not, it will be added. 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <param name="frameworkId"></param>
		/// <returns></returns>
		public bool HandleFrameworkRequest( CassFramework request,
				int userId,
				ref List<string> messages,
				ref int frameworkId )
		{
			bool isValid = true;
			if ( request == null || string.IsNullOrWhiteSpace(request._IdAndVersion) )
			{
				messages.Add( "The Cass Request doesn't contain a valid Cass Framework class." );
				return false;
			}
			ThisEntity item = Get( request._IdAndVersion );
			if (item != null && item.Id > 0)
			{
				//TODO - do we want to attempt an update - if changed
				//		- if we plan to implement a batch refresh of sync'd content, then not necessary
				frameworkId = item.Id;
				return true;
			}
			//add the framework...
			ThisEntity entity = new ThisEntity();
			entity.Name = request.Name;
			entity.Description = request.Description;
			entity.FrameworkUrl = request.Url;
			entity.RepositoryUri = request._IdAndVersion;

			//TDO - need owning org - BUT, first person to reference a framework is not necessarily the owner!!!!!
			//actually, we may not care here. Eventually get a ctid from CASS
			//entity.OwningOrganizationId = 0;

			isValid = Save( entity, userId, ref messages );
			frameworkId = entity.Id;
			return isValid;
		}
		/// <summary>
		/// Add/Update a EducationFramework
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool Save( ThisEntity entity,
				int userId,
				ref List<string> messages )
		{
			bool isValid = true;
			int count = 0;

			DBentity efEntity = new DBentity();

			using ( var context = new Data.CTIEntities() )
			{

				bool isEmpty = false;

				if ( ValidateProfile( entity, ref isEmpty, ref messages ) == false )
				{
					return false;
				}
				if ( isEmpty )
				{
					messages.Add( "The Education Framework Profile is empty. " );
					return false;
				}

				if ( entity.Id == 0 )
				{
					//add
					efEntity = new DBentity();
					FromMap( entity, efEntity );

					efEntity.Created = DateTime.Now;
					efEntity.CreatedById = userId;
					efEntity.RowId = Guid.NewGuid();

					context.EducationFramework.Add( efEntity );

					count = context.SaveChanges();

					entity.Id = efEntity.Id;
					entity.RowId = efEntity.RowId;
					if ( count == 0 )
					{
						messages.Add( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.Name ) ? "no description" : entity.Name ) );
					}
				}
				else
				{

					efEntity = context.EducationFramework.SingleOrDefault( s => s.Id == entity.Id );
					if ( efEntity != null && efEntity.Id > 0 )
					{
						entity.RowId = efEntity.RowId;
						//update
						FromMap( entity, efEntity );
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
		/// Delete a framework - only if no remaining references!!
		/// MAY NOT expose initially
		/// </summary>
		/// <param name="recordId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBentity p = context.EducationFramework.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.EducationFramework.Remove( p );
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

			if ( string.IsNullOrWhiteSpace( profile.Name ) )
			{
				messages.Add( "An educational framework name must be entered" );
				isValid = false;
			}

			if ( string.IsNullOrWhiteSpace( profile.RepositoryUri ) )
			{
				messages.Add( "An Educational Repository Uri must be entered" );
				isValid = false;
			}
		
			return isValid;
		}

		#endregion
		#region  retrieval ==================

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
					DBentity item = context.EducationFramework
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
		public static ThisEntity Get( string repositoryUri )
		{
			ThisEntity entity = new ThisEntity();
			if ( string.IsNullOrWhiteSpace( repositoryUri ))
				return entity;
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentity item = context.EducationFramework
							.SingleOrDefault( s => s.RepositoryUri == repositoryUri );

					if ( item != null && item.Id > 0 )
					{
						ToMap( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Get( string repositoryUri )" );
			}
			return entity;
		}//


		public static void FromMap( ThisEntity from, DBentity to )
		{
			//want to ensure fields from create are not wiped
			//to.Id = from.Id;

			to.Name = StripJqueryTag( from.Name );
			to.FrameworkUrl = from.FrameworkUrl;
			to.RepositoryUri = from.RepositoryUri;
			to.Description = from.Description;


		}
		public static void ToMap( DBentity from, ThisEntity to)
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.Name = StripJqueryTag( from.Name );
			to.FrameworkUrl = from.FrameworkUrl;
			to.RepositoryUri = from.RepositoryUri;
			to.Description = from.Description;

			ThisEntityItem ip = new ThisEntityItem();
			
			foreach ( DBentityItem item in from.EducationFramework_Competency )
			{
				ip = new ThisEntityItem();
				ip.Id = item.Id;
				ip.EducationFrameworkId = item.EducationFrameworkId;

				ip.Name = item.Name;
				ip.Description = item.Description;
				ip.RepositoryUri = item.RepositoryUri;
				ip.Url = item.Url;
				ip.CodedNotation = item.CodedNotation;

				to.EducationFrameworkCompetencies.Add( ip );
			}

		}

		#endregion
		#endregion

		#region --- EducationFramework_Competency ---

		#region Persistance ===================
		public bool HandleCompetencyRequest( CassCompetency request,
				int frameworkId, ///???
				int userId,
				ref int competencyId,
				ref List<string> messages
				)
		{
			bool isValid = true;
			if ( request == null || string.IsNullOrWhiteSpace( request._IdAndVersion ) )
			{
				messages.Add( "The Cass Request doesn't contain a valid Cass Competency." );
				return false;
			}
			ThisEntityItem item = EducationFramework_Competency_Get( request._IdAndVersion );
			if ( item != null && item.Id > 0 )
			{
				//TODO - do we want to attempt an update - if changed
				//		- if we plan to implement a batch refresh of sync'd content, then not necessary
				competencyId = item.Id;
				return true;
			}
			//add the record...
			ThisEntityItem entity = new ThisEntityItem();
			entity.EducationFrameworkId = frameworkId;
			entity.Name = request.Name;
			entity.Description = request.Description;
			entity.Url = request.Url;
			entity.RepositoryUri = request._IdAndVersion;
			entity.CodedNotation = request.CodedNotation;

			isValid = EducationFramework_Competency_Save( entity, userId, ref messages );
			competencyId = entity.Id;
			return isValid;
		}

		/// <summary>
		/// Add/Update a competency
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool EducationFramework_Competency_Save( ThisEntityItem entity,
				int userId,
				ref List<string> messages )
		{
			bool isValid = true;
			int intialCount = messages.Count;

			if ( entity.EducationFrameworkId == 0 )
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

				if ( ValidateProfile( entity, ref isEmpty, ref messages ) == false )
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

					efEntity.Created = DateTime.Now;
					efEntity.CreatedById = userId;
					efEntity.RowId = Guid.NewGuid();

					context.EducationFramework_Competency.Add( efEntity );

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

					efEntity = context.EducationFramework_Competency.SingleOrDefault( s => s.Id == entity.Id );
					if ( efEntity != null && efEntity.Id > 0 )
					{
						entity.RowId = efEntity.RowId;
						//update
						FromMap( entity, efEntity );
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
		/// NOT likely to be allowed from interface?
		/// </summary>
		/// <param name="recordId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool EducationFramework_Competency_Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBentityItem p = context.EducationFramework_Competency.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.EducationFramework_Competency.Remove( p );
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
			int count = messages.Count;
			isEmpty = false;
			//check if empty
			if ( string.IsNullOrWhiteSpace( profile.Name )
				&& string.IsNullOrWhiteSpace( profile.Description )
				&& string.IsNullOrWhiteSpace( profile.RepositoryUri )
				&& string.IsNullOrWhiteSpace( profile.CodedNotation )
				)
			{
				messages.Add( "Please enter, at minimum, a competency name." );
				//isEmpty = true;
				return false;
			}

			if ( string.IsNullOrWhiteSpace( profile.Name ) )
			{
				messages.Add( "A competency name must be entered" );
			}

			if ( string.IsNullOrWhiteSpace( profile.RepositoryUri ) )
			{
				messages.Add( "A Repository Uri must be entered" );
			}
			else if ( !IsUrlValid( profile.RepositoryUri, ref commonStatusMessage ) )
			{
				messages.Add( "The Repository Uri is invalid. " + commonStatusMessage );
			}

			if ( messages.Count > count )
				isValid = false;

			return isValid;
		}

		#endregion
		#region  retrieval ==================

		/// <summary>
		/// Get all records for the parent
		/// Uses the parent Id to retrieve the related competencies.
		/// </summary>
		/// <param name="frameworkId"></param>
		public static List<ThisEntityItem> EducationFramework_Competency_GetAll( int frameworkId )
		{
			ThisEntityItem entity = new ThisEntityItem();
			List<ThisEntityItem> list = new List<ThisEntityItem>();

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBentityItem> results = context.EducationFramework_Competency
							.Where( s => s.EducationFrameworkId == frameworkId )
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentityItem item in results )
						{
							entity = new ThisEntityItem();
							ToMap( item, entity );
							list.Add( entity );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".EducationFramework_Competency_GetAll" );
			}
			return list;
		}//

		/// <summary>
		/// Get a competency record
		/// </summary>
		/// <param name="profileId"></param>
		/// <returns></returns>
		public static ThisEntityItem EducationFramework_Competency_Get( int profileId )
		{
			ThisEntityItem entity = new ThisEntityItem();
			if ( profileId == 0 )
				return entity;
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentityItem item = context.EducationFramework_Competency
							.SingleOrDefault( s => s.Id == profileId );

					if ( item != null && item.Id > 0 )
					{
						ToMap( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".EducationFramework_Competency_Get( int profileId )" );
			}
			return entity;
		}//

		public static ThisEntityItem EducationFramework_Competency_Get( string repositoryUri )
		{
			ThisEntityItem entity = new ThisEntityItem();
			if ( string.IsNullOrWhiteSpace( repositoryUri ) )
				return entity;

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentityItem item = context.EducationFramework_Competency
							.SingleOrDefault( s => s.RepositoryUri == repositoryUri );

					if ( item != null && item.Id > 0 )
					{
						ToMap( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".EducationFramework_Competency_Get( string repositoryUri )" );
			}
			return entity;
		}//
		public static void FromMap( ThisEntityItem from, DBentityItem to )
		{
			//want to ensure fields from create are not wiped
			//to.Id = from.Id;
			//this may not be available
			to.EducationFrameworkId = from.EducationFrameworkId;

			to.Name = StripJqueryTag( from.Name );

			to.Description = from.Description;
			to.RepositoryUri = from.RepositoryUri;
			to.Url = from.Url;
			to.CodedNotation = from.CodedNotation;


		}
		public static void ToMap( DBentityItem from, ThisEntityItem to )
		{
			to.Id = from.Id;
			to.EducationFrameworkId = from.EducationFrameworkId;
			to.RowId = from.RowId;

			to.Name = StripJqueryTag( from.Name );
			
			to.Description = from.Description;
			to.RepositoryUri = from.RepositoryUri;
			to.Url = from.Url;
			to.CodedNotation = from.CodedNotation;


			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
		}

		public static void ToMap( DBentityItem from, CredentialAlignmentObjectItemProfile to )
		{
			to.Id = from.Id;
			to.ParentId = from.EducationFrameworkId;
			to.RowId = from.RowId;
			to.Name = StripJqueryTag( from.Name );

			to.RepositoryUri = from.RepositoryUri;
			to.Description = from.Description;
			to.CodedNotation = from.CodedNotation;
			

		}
		#endregion

		#endregion

	}
}
