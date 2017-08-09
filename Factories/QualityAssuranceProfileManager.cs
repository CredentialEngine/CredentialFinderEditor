using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;

using Models.Common;
using CM = Models.Common;
using EM = Data;
using Utilities;


using DBentity = Data.Organization_QAProfile;
using Entity = Models.Common.OrganizationQAProfile;

namespace Factories
{
public class QualityAssuranceProfileManager : BaseFactory
	{
	static string thisClassName = "QualityAssuranceProfileManager";
		

		#region persistance ==================

		/// <summary>
		/// add a OrganizationQAProfile
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int OrganizationQAProfile_Add( Entity entity, ref string statusMessage )
		{
			DBentity efEntity = new DBentity();
			//OrganizationQAProfilePropertyManager opMgr = new OrganizationQAProfilePropertyManager();
			using ( var context = new Data.CTIEntities() )
			{
				try
				{

					OrganizationQAProfile_FromMap( entity, efEntity );

					if ( efEntity.RowId.ToString() == DEFAULT_GUID )
						efEntity.RowId = Guid.NewGuid();
					efEntity.CreatedById = entity.CreatedById;
					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdatedById = entity.CreatedById;
					efEntity.LastUpdated = System.DateTime.Now;

					context.Organization_QAProfile.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";
						entity.Id = efEntity.Id;

						//opMgr.OrganizationQAProfile_UpdateParts( entity, true, ref statusMessage );

						return efEntity.Id;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the add was not successful. ";
						string message = string.Format( "OrganizationQAProfileManager. OrganizationQAProfile_Add Failed", "Attempted to add a OrganizationQAProfile. The process appeared to not work, but was not an exception, so we have no message, or no clue.OrganizationQAProfile. OrganizationId: {0}, createdById: {1}", entity.OrganizationId, entity.CreatedById );
						EmailManager.NotifyAdmin( "OrganizationQAProfileManager. OrganizationQAProfile_Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					//LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
					string message = thisClassName + string.Format( ".OrganizationQAProfile_Add() DbEntityValidationException, OrgId: {0}", efEntity.OrganizationId );
					foreach ( var eve in dbex.EntityValidationErrors )
					{
						message += string.Format( "\rEntity of type \"{0}\" in state \"{1}\" has the following validation errors:",
							eve.Entry.Entity.GetType().Name, eve.Entry.State );
						foreach ( var ve in eve.ValidationErrors )
						{
							message += string.Format( "- Property: \"{0}\", Error: \"{1}\"",
								ve.PropertyName, ve.ErrorMessage );
						}

						LoggingHelper.LogError( message, true );
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".OrganizationQAProfile_Add(), OrganizationId: {0}", entity.OrganizationId ) );
				}
			}

			return efEntity.Id;
		}
		/// <summary>
		/// Update a OrganizationQAProfile
		/// - base only, caller will handle parts?
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool OrganizationQAProfile_Update( Entity entity, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			//OrganizationQAProfilePropertyManager opMgr = new OrganizationQAProfilePropertyManager();
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentity efEntity = context.Organization_QAProfile
								.SingleOrDefault( s => s.Id == entity.Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						//for updates, chances are some fields will not be in interface, don't map these (ie created stuff)
						OrganizationQAProfile_FromMap( entity, efEntity );
						if ( HasStateChanged( context ) )
						{
							efEntity.LastUpdated = System.DateTime.Now;
							efEntity.LastUpdatedById = entity.LastUpdatedById;
							count = context.SaveChanges();
							//can be zero if no data changed
							if ( count >= 0 )
							{
								isValid = true;
							}
							else
							{
								//?no info on error
								statusMessage = "Error - the update was not successful. ";
								string message = string.Format( "OrganizationQAProfileManager. OrganizationQAProfile_Update Failed", "Attempted to update a OrganizationQAProfile. The process appeared to not work, but was not an exception, so we have no message, or no clue. OrganizationQAProfile: {0}, Id: {1}, updatedById: {2}", entity.OrganizationName, entity.Id, entity.LastUpdatedById );
								EmailManager.NotifyAdmin( "OrganizationQAProfileManager. OrganizationQAProfile_Update Failed", message );
							}
						}
						//continue with parts regardless
						//opMgr.OrganizationQAProfile_UpdateParts( entity, false, ref statusMessage );
					}
					else
					{
						statusMessage = "Error - update failed, as record was not found.";
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + string.Format(".OrganizationQAProfile_Update. id: {0}", entity.Id) );
			}
			

			return isValid;
		}
		
		public bool OrganizationQAProfile_Delete( int Id, ref string statusMessage )
		{
			bool isValid = false;
			if ( Id == 0 )
			{
				statusMessage = "Error - missing an identifier for the OrganizationQAProfile";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				DBentity efEntity = context.Organization_QAProfile
							.SingleOrDefault( s => s.Id == Id );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					context.Organization_QAProfile.Remove( efEntity );
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					}
				}
				else
				{
					statusMessage = "Error - delete failed, as record was not found.";
				}
			}

			return isValid;
		}

		#endregion

		#region == Retrieval =======================
		
		public static Entity OrganizationQAProfile_Get( int id, bool includeProperties = false )
		{
			Entity entity = new Entity();
			using ( var context = new Data.CTIEntities() )
			{
				
				DBentity item = context.Organization_QAProfile
						.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					OrganizationQAProfile_ToMap( item, entity, true );
					if ( includeProperties )
					{
						//TBD
					}
				}
			}

			return entity;
		}


		public static List<OrganizationQAProfile> Search( int userId = 0, string keyword = "", int maxTerms = 0 )
		{
			List<OrganizationQAProfile> list = new List<OrganizationQAProfile>();
			OrganizationQAProfile entity = new OrganizationQAProfile();

			keyword = keyword.Trim();
			if ( maxTerms == 0 )
				maxTerms = 500;

			using ( var context = new Data.CTIEntities() )
			{
				List<DBentity> results = context.Organization_QAProfile
								.Where( s => keyword == "" || s.Organization.Name.Contains( keyword ) )
								.Take( maxTerms )
								.OrderBy( s => s.Organization.Name )
								.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( DBentity item in results )
					{
						OrganizationQAProfile_ToMap( item, entity, true );
						//entity = new OrganizationQAProfile();
						//entity.Id = item.Id;
						//entity.OrganizationName = item.Organization.Name;
						//entity.Description = item.Description;
						//entity.BodyTypeId = item.BodyTypeId;
						
						list.Add( entity );
					}
				}
			}

			return list;
		}


		public static void OrganizationQAProfile_FromMap( Entity fromEntity, DBentity to )
		{
			
			//want to ensure fields from create are not wiped
			if ( to.Id < 1 )
			{
				if ( IsValidDate( fromEntity.Created ) )
					to.Created = fromEntity.Created;
				to.CreatedById = fromEntity.CreatedById;
			}

			to.Id = fromEntity.Id;
			to.OrganizationId = fromEntity.OrganizationId;
			to.Description = fromEntity.Description;

			to.BodyTypeId = fromEntity.BodyTypeId;
			to.Url = fromEntity.Url;
			to.ManagingConflictsUrl = fromEntity.ManagingConflictsUrl;
			to.ComplaintsUrl = fromEntity.ComplaintsUrl;
			to.AppealsUrl = fromEntity.AppealsUrl;
			to.ExternalRecognitionUrl = fromEntity.ComplaintsUrl;

			if ( IsValidDate(fromEntity.LastUpdated) )
				to.LastUpdated = fromEntity.LastUpdated;
			to.LastUpdatedById = fromEntity.LastUpdatedById;
		}
		public static void OrganizationQAProfile_ToMap( DBentity fromEntity, Entity to, bool includingProperties = false )
		{
			to.Id = fromEntity.Id;
			to.OrganizationId = fromEntity.OrganizationId;
			to.Description = fromEntity.Description;

			to.BodyTypeId = (int)fromEntity.BodyTypeId;
			to.Url = fromEntity.Url;
			to.ManagingConflictsUrl = fromEntity.ManagingConflictsUrl;
			to.ComplaintsUrl = fromEntity.ComplaintsUrl;
			to.AppealsUrl = fromEntity.AppealsUrl;
			to.ExternalRecognitionUrl = fromEntity.ComplaintsUrl;

			if ( IsValidDate(fromEntity.Created) )
				to.Created = ( DateTime ) fromEntity.Created;
			to.CreatedById = fromEntity.CreatedById == null ? 0 : ( int ) fromEntity.CreatedById;
			if ( IsValidDate(fromEntity.LastUpdated) )
				to.LastUpdated = ( DateTime ) fromEntity.LastUpdated;
			to.LastUpdatedById = fromEntity.LastUpdatedById == null ? 0 : ( int ) fromEntity.LastUpdatedById;

			//properties
			//if ( includingProperties )
			//	OrganizationQAProfilePropertyManager.OrganizationQAProfilePropertyFill_ToMap( fromEntity, to );
		}
	
		#endregion

	}
}
