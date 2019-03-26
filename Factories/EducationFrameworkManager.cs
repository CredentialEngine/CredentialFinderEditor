using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Common;
using MN = Models.Node;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBEntity = Data.EducationFramework;
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
		/// Handle list of selected competencies:
		/// - check if eduction framework exists
		/// - if not, add it and return frameworkId
		/// </summary>
		/// <param name="input"></param>
		/// <param name="valid"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public List<CassCompetencyV2> SaveCassCompetencyList( MN.CassInput input, Guid parentUid, AppUser user, ref bool isValid, ref List<string> messages )
		{
			//Get the user

			int currentMesssage = messages.Count();
			List<CassCompetencyV2> list = new List<CassCompetencyV2>();

			//EducationFrameworkManager mgr = new EducationFrameworkManager();
			int frameworkId = 0;
			//check if framework exists. 
			//if found, the frameworkId is returned, 
			//otherwise framework is added and frameworkId is returned
			if ( HandleFrameworkRequest( input.Framework, user.Id, ref messages, ref frameworkId ) )
			{
				//will not create an Entity.EducationFramework with this competency centrix method

				//handle competencies
				int competencyId = 0;
				Entity_Competency entity = new Entity_Competency();
				Entity_CompetencyManager ecmMgr = new Entity_CompetencyManager();
				Entity parent = EntityManager.GetEntity( parentUid );
				int addedCount = 0;
				foreach ( var competency in input.Competencies )
				{
					if ( HandleCompetencyRequest( competency, frameworkId, user.Id, ref competencyId, ref messages ) )
					{
						//add Entity.Competency
						entity = new Entity_Competency();
						entity.CompetencyId = competencyId;
						entity.CreatedById = user.Id;
						//included for tracing:
						entity.FrameworkCompetency.Name = competency.Name;
						entity.FrameworkCompetency.Description = competency.Description;
						entity.Uri = competency.Uri;
						entity.CTID = competency.CTID;

						if ( ecmMgr.Save( entity, parent, user.Id, ref messages ) )
						{
							addedCount++;
							//get cass version for display on editor - where applicable
							list.Add( Entity_CompetencyManager.GetAsCassCompetency( entity.Id ) );
						}

					}
				}
				//if ( addedCount > 0 )
				//{
				//should have an activity for the whole framework
				//    new ActivityServices().AddEditorActivity( parent.EntityType, "Add Competencies", string.Format( "{0} added {1} Competencies to record: {2}", user.FullName(), addedCount, parent.EntityBaseId ), user.Id, 0, parent.EntityBaseId );

				//    new ProfileServices().UpdateTopLevelEntityLastUpdateDate( parent.Id, string.Format( "Entity Update triggered by {0} adding competencies to : {1}, BaseId: {2}", user.FullName(), parent.EntityType, parent.EntityBaseId ) );
				//}
			}

			if ( messages.Count > currentMesssage )
			{
				isValid = false;

			}

			return list;
		}

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
			}//
			string cassFrameworkName = "";
			//just in case, look up in CASS_CompetencyFramework
			int pos = request._IdAndVersion.IndexOf( "/resources/" );
			if ( pos > 0 )
			{
				var CTID = request._IdAndVersion.Substring( pos + "/resources/".Length );
				var f = CASS_CompetencyFrameworkManager.GetByCtid( CTID );
				if ( f != null && !string.IsNullOrWhiteSpace( f.FrameworkName ) )
				{
					cassFrameworkName = f.FrameworkName;
					if ( string.IsNullOrWhiteSpace( request.Name ) )
						request.Name = cassFrameworkName;
				}
			}
			//
			ThisEntity item = Get( request._IdAndVersion );
			if (item != null && item.Id > 0)
			{
				//TODO - do we want to attempt an update - if changed
				//		- if we plan to implement a batch refresh of sync'd content, then not necessary
				if ( item.Name == request.Name )
				{
					frameworkId = item.Id;
					return true;
				}
				else
				{
					//YES need to handle changes to the name
					if ( request.Name != "DEFAULT FRAMEWORK" )
						item.Name = request.Name ?? cassFrameworkName;
					item.Description = request.Description ?? "";
					item.FrameworkUrl = request.Url;
					item.RepositoryUri = request._IdAndVersion;
					isValid = Save( item, userId, ref messages );
					frameworkId = item.Id;
					return isValid;
				}
			}

			
			//add the framework...
			ThisEntity entity = new ThisEntity();
			entity.Name = request.Name  ?? cassFrameworkName;
			entity.Description = request.Description ?? "";
			entity.FrameworkUrl = request.Url;
			entity.RepositoryUri = request._IdAndVersion;
            entity.CTID = request.CTID;
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

			DBEntity efEntity = new DBEntity();

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
					efEntity = new DBEntity();
					MapToDB( entity, efEntity );

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
						MapToDB( entity, efEntity );
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
				DBEntity p = context.EducationFramework.FirstOrDefault( s => s.Id == recordId );
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
			else if ( !IsUrlValid( profile.RepositoryUri, ref commonStatusMessage ) )
			{
				messages.Add( "The Educational Repository Uri is invalid. " + commonStatusMessage );
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
					DBEntity item = context.EducationFramework
							.SingleOrDefault( s => s.Id == profileId );

					if ( item != null && item.Id > 0 )
					{
						MapFromDB( item, entity );
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
					DBEntity item = context.EducationFramework
							.FirstOrDefault( s => s.RepositoryUri == repositoryUri );

					if ( item != null && item.Id > 0 )
					{
						MapFromDB( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Get( string repositoryUri )" );
			}
			return entity;
		}//

		public static List<ThisEntity> GetAllFrameworksForParent( Guid parentUid )
		{
			List<ThisEntity> list = new List<ThisEntity>();
			ThisEntity thisEntity = new ThisEntity();
			using ( var context = new Data.CTIEntities() )
			{
				var results = from ecompetency in context.Entity_Competency
							join entity in context.Entity
							on ecompetency.EntityId equals entity.Id

							join competency in context.EducationFramework_Competency
							on ecompetency.CompetencyId equals competency.Id

							join framework in context.EducationFramework
							on competency.EducationFrameworkId equals framework.Id

							where entity.EntityUid == parentUid
							select new { framework.Name, framework.Id, FrameworkUri = framework.FrameworkUrl, framework.RepositoryUri };
				var frameworks = results.Distinct().ToList();

				foreach (var item in results )
				{
					thisEntity = new ThisEntity
					{
						Id = item.Id,
						Name = item.Name,
						FrameworkUrl = item.FrameworkUri,
						RepositoryUri = item.RepositoryUri
					};
					list.Add( thisEntity );
				}
			}

			return list;
		}

		public static void MapToDB( ThisEntity from, DBEntity to )
		{
			//want to ensure fields from create are not wiped
			//to.Id = from.Id;

			to.Name = StripJqueryTag( from.Name );
			to.FrameworkUrl = from.FrameworkUrl;
			to.RepositoryUri = from.RepositoryUri;
			to.Description = from.Description;
            //need to ensure not overridden, so maybe not here?
            //will not be provided - could extract from RepositoryUri
            if ( string.IsNullOrWhiteSpace(to.CTID) && string.IsNullOrWhiteSpace( from.CTID ))
                to.CTID = from.CTID;
            /*
             * else {
			string ctid = "";
            var sections = from.RepositoryUri.Split( '/' );
            if ( sections != null && sections.Length > 0)
            {
                ctid = "ce-" + sections[ sections.Length - 2 ];
            }
            to.CTID = from.CTID != null ? request.CTID : ctid;
            }
			*/

        }
        public static void MapFromDB( DBEntity from, ThisEntity to)
		{
            if ( to == null )
                to = new ThisEntity();

			to.Id = from.Id;
			to.RowId = from.RowId;
			to.Name = StripJqueryTag( from.Name );
			to.FrameworkUrl = from.FrameworkUrl;
			to.RepositoryUri = from.RepositoryUri;
			to.Description = from.Description;
            to.CTID = from.CTID;

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
			if ( request == null || string.IsNullOrWhiteSpace( request.Uri ) )
			{
                messages.Add( "The Cass Request doesn't contain a valid Cass Competency URI." );
                return false;
            }
			ThisEntityItem item = EducationFramework_Competency_Get( request.Uri );
			if ( item != null && item.Id > 0 )
			{
				//TODO - do we want to attempt an update - if changed
				//		- if we plan to implement a batch refresh of sync'd content, then not necessary
				competencyId = item.Id;

				item.Name = request.Name;
				item.Description = request.Description;
				//URI should not change, but????
				item.RepositoryUri = request.Uri;
				item.CodedNotation = request.CodedNotation ?? "";
				//careful here, CTID shouldn't change - although was error where was null from interface pre: 19-02-19
				item.CTID = request.CTID;
				isValid = EducationFramework_Competency_Save( item, userId, ref messages );

				return true;
			}
			//add the record...
			ThisEntityItem entity = new ThisEntityItem();
			entity.EducationFrameworkId = frameworkId;
			entity.Name = request.Name;
			entity.Description = request.Description;
			entity.CTID = request.CTID;
			//entity.Url = request.Url;
			//will the CTID come from CASS - 18-03-08: not now
			/*
			string ctid = "";
            var sections = request.Url.Split( '/' );
            if ( sections != null && sections.Length > 0)
            {
                ctid = "ce-" + sections[ sections.Length - 2 ];
            }
            entity.CTID = request.CTID != null ? request.CTID : ctid;
			*/
            entity.RepositoryUri = request.Uri;
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
				return false;
			}

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
					MapToDB( entity, efEntity );

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
						MapToDB( entity, efEntity );
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
				//Was causing competencies to fail to save even though their URLs were legit
				//messages.Add( "The Repository Uri is invalid. " + commonStatusMessage );
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
							MapFromDB( item, entity );
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
						MapFromDB( item, entity );
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
							.FirstOrDefault( s => s.RepositoryUri == repositoryUri );

					if ( item != null && item.Id > 0 )
					{
						MapFromDB( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".EducationFramework_Competency_Get( string repositoryUri )" );
			}
			return entity;
		}//
		public static void MapToDB( ThisEntityItem from, DBentityItem to )
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
            //need to ensure not overridden, so maybe not here?
            to.CTID = from.CTID;

        }
        public static void MapFromDB( DBentityItem from, ThisEntityItem to )
		{
			to.Id = from.Id;
			to.EducationFrameworkId = from.EducationFrameworkId;
			to.RowId = from.RowId;

			to.Name = StripJqueryTag( from.Name );
			
			to.Description = from.Description;
			to.RepositoryUri = from.RepositoryUri;
			to.Url = from.Url;
			to.CodedNotation = from.CodedNotation;
            to.CTID = from.CTID;

            if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
		}

		public static void MapFromDB( DBentityItem from, CredentialAlignmentObjectItemProfile to )
		{
			to.Id = from.Id;
			to.ParentId = from.EducationFrameworkId;
			to.RowId = from.RowId;
			to.TargetNodeName = StripJqueryTag( from.Name );

			to.RepositoryUri = from.RepositoryUri;
			to.Description = from.Description;
			to.CodedNotation = from.CodedNotation;
            //ctid not in CredentialAlignmentObjectItemProfile, yet
            //to.CTID = from.CTID;

        }
		#endregion

		#endregion

	}
}
