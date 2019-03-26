using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using Utilities;
using DBEntity = Data.CASS_CompetencyFramework;
using EM = Data;
using ThisEntity = Models.Common.CASS_CompetencyFramework;

namespace Factories
{

	public class CASS_CompetencyFrameworkManager : BaseFactory
    {
        static string thisClassName = "CASS_CompetencyFrameworkManager";


        #region persistance ==================

        /// <summary>
        /// add a CASS_CompetencyFramework
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Add( ThisEntity entity, int userId, ref List<string> messages )
        {
            DBEntity efEntity = new DBEntity();
            using (var context = new Data.CTIEntities())
            {
                try
                {
                    if (!ValidateProfile( entity, ref messages ))
                        return 0;

                    MapToDB( entity, efEntity );
                    if (efEntity.RowId == null || efEntity.RowId.ToString() == DEFAULT_GUID)
                        efEntity.RowId = Guid.NewGuid();
                    efEntity.CreatedById = userId;
                    efEntity.Created = System.DateTime.Now;
                    efEntity.LastUpdatedById = userId;
                    efEntity.LastUpdated = System.DateTime.Now;

                    context.CASS_CompetencyFramework.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if (count > 0)
                    {
                        entity.Id = efEntity.Id;
						entity.RowId = efEntity.RowId;
                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error
                        messages.Add( "Error - the profile was not saved. " );
                        string message = string.Format( thisClassName + ".Add Failed", "Attempted to add a CASS_CompetencyFramework. The process appeared to not work, but was not an exception, so we have no message, or no clue.CASS_CompetencyFramework. OrgId: {0}, createdById: {1}", entity.OrgId, entity.CreatedById );
                        EmailManager.NotifyAdmin( thisClassName + ". Add Failed", message );
                    }
                }
                
                catch (Exception ex)
                {
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), OrgId: {0}", entity.OrgId ) );
                }
            }

            return efEntity.Id;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Update( ThisEntity entity, ref List<string> messages, bool skippingLastUpdated = false )
        {
            bool isValid = false;
            int count = 0;
            try
            {
                if (!ValidateProfile( entity, ref messages ))
                    return false;

                entity.CTID = entity.CTID.ToLower();
                using (var context = new Data.CTIEntities())
                {
                    DBEntity efEntity = context.CASS_CompetencyFramework
                                .SingleOrDefault( s => s.CTID == entity.CTID );

                    if (efEntity != null && efEntity.Id > 0)
                    {
                        //for updates, chances are some fields will not be in interface, don't map these (ie created stuff)
                        MapToDB( entity, efEntity );
                        if (HasStateChanged( context ))
                        {
                            if ( !skippingLastUpdated )
                            {
                                efEntity.LastUpdated = System.DateTime.Now;
                                efEntity.LastUpdatedById = entity.LastUpdatedById;
                            }
							entity.Id = efEntity.Id;
							entity.RowId = efEntity.RowId;
							count = context.SaveChanges();
                            //can be zero if no data changed
                            if (count >= 0)
                            {
								isValid = true;
                            }
                            else
                            {
                                //?no info on error
                                messages.Add("Error - the update was not successful. ");
                                string message = string.Format( thisClassName + ".Update Failed", "Attempted to update a CASS_CompetencyFramework. The process appeared to not work, but was not an exception, so we have no message, or no clue. OrgId: {0}, Id: {1}, updatedById: {2}", entity.OrgId, entity.Id, entity.LastUpdatedById );
                                EmailManager.NotifyAdmin( thisClassName + ". Update Failed", message );
                            }
                        }

                    }
                    else
                    {
                        messages.Add( "Error - update failed, as record was not found. CTID: " + entity.CTID);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Update. CTID: {0}", entity.CTID ) );
            }


            return isValid;
        }

        /// <summary>
        /// Update credential registry id, and set status published
        /// </summary>
        /// <param name="frameworkId"></param>
        /// <param name="envelopeId"></param>
        /// <param name="userId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool UpdateEnvelopeId( int frameworkId, string envelopeId, int userId, ref string statusMessage )
        {
            bool isValid = false;
            int count = 0;

            using (var context = new Data.CTIEntities())
            {
                try
                {
                    context.Configuration.LazyLoadingEnabled = false;
                    EM.CASS_CompetencyFramework efEntity = context.CASS_CompetencyFramework
                                    .SingleOrDefault( s => s.Id == frameworkId );

                    if (efEntity != null && efEntity.Id > 0)
                    {
                        efEntity.CredentialRegistryId = envelopeId;
                        if (HasStateChanged( context ))
                        {
                            //don't set updated for this action, maybe for frameworks
                            //the reason for not updating is that will result in last updated > last published
                            efEntity.LastUpdated = DateTime.Now;
                            efEntity.LastUpdatedById = userId;
							//these should be set separately by the caller
							//efEntity.LastPublished = DateTime.Now;
							//efEntity.LastPublishedById = userId;

							count = context.SaveChanges();
                            //can be zero if no data changed
                            if (count >= 0)
                            {
                                isValid = true;
                            }
                            else
                            {
                                //?no info on error
                                statusMessage = "Error - the update was not successful. ";
                                string message = string.Format( thisClassName + ". UpdateEnvelopeId Failed", "Attempted to update an EnvelopeId. The process appeared to not work, but was not an exception, so we have no message, or no clue. frameworkId: {0}, envelopeId: {1}, updatedById: {2}", frameworkId, envelopeId, userId );
                                EmailManager.NotifyAdmin( thisClassName + ".UpdateEnvelopeId Failed", message );
                            }
                        }
                    }
                    else
                    {
                        statusMessage = "Error - update failed, as record was not found.";
                    }
                }
                catch (Exception ex)
                {
                    statusMessage = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".UpdateEnvelopeId(), frameworkId: {0}, envelopeId: {1}, updatedById: {2}", frameworkId, envelopeId, userId ) );

                }
            }

            return isValid;
        }

        /// <summary>
		/// Reset credential registry id, and set status to in process
		/// </summary>
		/// <param name="recordId"></param>
		/// <param name="userId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool UnPublish( int recordId, int userId, ref string statusMessage )
        {
            bool isValid = false;
            int count = 0;
            using (var context = new Data.CTIEntities())
            {
                DBEntity efEntity = new DBEntity();
                try
                {
                    context.Configuration.LazyLoadingEnabled = false;

                     efEntity = context.CASS_CompetencyFramework
                                    .SingleOrDefault( s => s.Id == recordId );

                    if (efEntity != null && efEntity.Id > 0)
                    {
                        efEntity.CredentialRegistryId = null;
                        efEntity.LastPublished = null;
                        efEntity.LastPublishedById = null;
                        if (HasStateChanged( context ))
                        {
                            //technically, do we record this?
                            //efEntity.LastUpdated = System.DateTime.Now;
                            //efEntity.LastUpdatedById = userId;

                            count = context.SaveChanges();
                        }

                        //can be zero if no data changed
                        if (count >= 0)
                        {
                            isValid = true;
                        }
                        else
                        {
                            //?no info on error
                            statusMessage = "Error - the update was not successful. ";
                            string message = string.Format( thisClassName + ".UnPublish Failed", "Attempted to unpublish the Framework. The process appeared to not work, but was not an exception, so we have no message, or no clue. Credential: {0}, updatedById: {1}", recordId, userId );
                            EmailManager.NotifyAdmin( thisClassName + ".UnPublish Failed", message );
                        }
                    }
                    else
                    {
                        statusMessage = "Error - update failed, as record was not found.";
                    }
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbex)
                {
                    string message = HandleDBValidationError( dbex, thisClassName + ".)() ", efEntity.FrameworkName );
                    statusMessage = "Error - the unpublish was not successful. " + message;

                }
                catch (Exception ex)
                {
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".UnPublish(), Credential: {0}, updatedById: {1}", recordId, userId ) );
                    statusMessage = FormatExceptions( ex );
                }
            }

            return isValid;
        }
        public bool Delete( string ctid, ref string statusMessage )
        {
            bool isValid = false;
            if (string.IsNullOrWhiteSpace(ctid))
            {
                statusMessage = "Error - missing an identifier for the CASS_CompetencyFramework";
                return false;
            }
            ctid = ctid.ToLower();
            using (var context = new Data.CTIEntities())
            {
                DBEntity efEntity = context.CASS_CompetencyFramework
                            .FirstOrDefault( s => s.CTID == ctid );

                if (efEntity != null && efEntity.Id > 0)
                {
                    context.CASS_CompetencyFramework.Remove( efEntity );
                    int count = context.SaveChanges();
                    if (count > 0)
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

        private bool ValidateProfile( ThisEntity item, ref List<string> messages )
        {
            bool isValid = true;
            //******* TBD *******
            //if (string.IsNullOrWhiteSpace( item.ProfileName ))
            //{
            //    isValid = false;
            //    messages.Add( "Error: missing profile name" );
            //}
            return isValid;
        }
        #endregion

        #region == Retrieval =======================
        public static ThisEntity GetByCtid( string ctid )
        {
            ThisEntity entity = new ThisEntity();
            if (string.IsNullOrWhiteSpace( ctid ))
                return entity;
            ctid = ctid.ToLower();
            using (var context = new Data.CTIEntities())
            {

                DBEntity item = context.CASS_CompetencyFramework
                        .FirstOrDefault( s => s.CTID == ctid );

                if (item != null && item.Id > 0)
                {
                    MapFromDB( item, entity );

                }
            }

            return entity;
        }

		public static List<ThisEntity> GetAllApproved()
        {
            List<ThisEntity> list = new List<ThisEntity>();
            using ( var context = new Data.CTIEntities() )
            {
                List<DBEntity> results = context.CASS_CompetencyFramework
                        .Where( s => s.LastApprovedById > 0 )
                        .ToList();

                if ( results != null && results.Count > 0 )
                {
                    foreach ( var item in results )
                    {
                        ThisEntity entity = new ThisEntity();
                        MapFromDB( item, entity );
                        list.Add( entity );
                    }

                }
            }

            return list;
        }
		public static List<ThisEntity> GetAllPublished()
		{
			List<ThisEntity> list = new List<ThisEntity>();
			using ( var context = new Data.CTIEntities() )
			{
				List<DBEntity> results = context.CASS_CompetencyFramework
						.Where( s => s.CredentialRegistryId != null && s.CredentialRegistryId.Length ==36  )
						.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( var item in results )
					{
						ThisEntity entity = new ThisEntity();
						MapFromDB( item, entity );
						list.Add( entity );
					}
				}
			}

			return list;
		}
		public static ThisEntity Get( int id )
        {
            ThisEntity entity = new ThisEntity();
            using (var context = new Data.CTIEntities())
            {

                DBEntity item = context.CASS_CompetencyFramework
                        .FirstOrDefault( s => s.Id == id );

                if (item != null && item.Id > 0)
                {
                    MapFromDB( item, entity );

                }
            }

            return entity;
        }

        public static void MapToDB( ThisEntity from, DBEntity to )
        {

            //want to ensure fields from create are not wiped
            if (to.Id < 1)
            {
                if (IsValidDate( from.Created ))
                    to.Created = from.Created;
                to.CreatedById = from.CreatedById;
            }
			//from may not have these values
            //to.Id = from.Id;
            //to.RowId = from.RowId;
            to.OrgId = from.OrgId;
            to.FrameworkName = from.FrameworkName;
            to.CTID = from.CTID.ToLower();
            //make sure not overwritten
            if (!string.IsNullOrWhiteSpace( from.CredentialRegistryId ))
                to.CredentialRegistryId = from.CredentialRegistryId;

            //using last updated date from interface, as we don't have all data here. 
            if (IsValidDate( from.LastUpdated ))
                to.LastUpdated = from.LastUpdated;
            to.LastUpdatedById = from.LastUpdatedById;

            if ( IsValidDate( from.LastApproved ) )
                to.LastApproved = ( DateTime ) from.LastApproved;
            if ( from.LastApprovedById > 0 )
                to.LastApprovedById = from.LastApprovedById;
            else
                to.LastApprovedById = null;

            if ( IsValidDate( from.LastPublished ) )
                to.LastPublished = ( DateTime ) from.LastPublished;
            if ( from.LastPublishedById > 0 )
                to.LastPublishedById = from.LastApprovedById;
            else
                to.LastPublishedById = null;

        }
        public static void MapFromDB( DBEntity from, ThisEntity to )
        {
            to.Id = from.Id;
            to.RowId = from.RowId;
            to.OrgId = from.OrgId;
            to.FrameworkName = from.FrameworkName;
            to.CTID = from.CTID.ToLower(); ;
            to.CredentialRegistryId = from.CredentialRegistryId;
            if (to.OrgId > 0)
            {
                to.OwningOrganization = OrganizationManager.GetForSummary( to.OrgId );
            }

            if ( IsValidDate( from.Created ) )
                to.Created = ( DateTime ) from.Created;
            to.CreatedById = from.CreatedById;
            if ( IsValidDate( from.LastUpdated ) )
                to.LastUpdated = ( DateTime ) from.LastUpdated;
            to.LastUpdatedById = from.LastUpdatedById;

            if ( IsValidDate( from.LastApproved ) )
            {
                to.LastApproved = ( DateTime ) from.LastApproved;
                to.IsApproved = true;
            }
            to.LastApprovedById = (from.LastApprovedById ?? 0);
            if ( IsValidDate( from.LastPublished ) )
            {
                to.LastPublished = ( DateTime ) from.LastPublished;
                to.IsPublished = true;
            }
            to.LastPublishedById = ( from.LastPublishedById ?? 0 );

            //FOllowing WILL BE OBSOLETE SOON
            //will contain CreatedBy - name of person who did approval
            //to.EntityApproval = Entity_ApprovalManager.GetByParent( to.RowId );
            //DateTime workDate;
            //string date = ActivityManager.GetLastPublishDate( "cass_competencyframework", to.Id );
            //if (DateTime.TryParse( date, out workDate ))
            //{
            //    to.LastPublished = workDate;
            //   // to.IsPublished = true;
            //} else
            //{
            //    to.LastPublished = new DateTime(2000,1,1);
            //}


        }

		public static List<ThisEntity> Search( string pFilter, string pOrderBy, int pageNumber, int pageSize, ref int pTotalRows )
		{
			string connectionString = DBConnectionRO();
			ThisEntity item = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			var result = new DataTable();

			using ( SqlConnection c = new SqlConnection( connectionString ) )
			{
				c.Open();

				if ( string.IsNullOrEmpty( pFilter ) )
				{
					pFilter = "";
				}

				using ( SqlCommand command = new SqlCommand( "[CompetencyFrameworks_search]", c ) )
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add( new SqlParameter( "@Filter", pFilter ) );
					command.Parameters.Add( new SqlParameter( "@SortOrder", pOrderBy ) );
					command.Parameters.Add( new SqlParameter( "@StartPageIndex", pageNumber ) );
					command.Parameters.Add( new SqlParameter( "@PageSize", pageSize ) );

					SqlParameter totalRows = new SqlParameter( "@TotalRows", pTotalRows );
					totalRows.Direction = ParameterDirection.Output;
					command.Parameters.Add( totalRows );

					using ( SqlDataAdapter adapter = new SqlDataAdapter() )
					{
						adapter.SelectCommand = command;
						adapter.Fill( result );
					}
					string rows = command.Parameters[ 4 ].Value.ToString();
					try
					{
						pTotalRows = Int32.Parse( rows );
					}
					catch
					{
						pTotalRows = 0;
					}
				}

				foreach ( DataRow dr in result.Rows )
				{
					item = new ThisEntity();
					item.Id = GetRowColumn( dr, "Id", 0 );
					item.OrgId = GetRowColumn( dr, "OrgId", 0 );
					if ( item.OrgId > 0 )
					{
						item.OwningOrganization = OrganizationManager.GetForSummary( item.OrgId );
					}
					item.FrameworkName = GetRowColumn( dr, "FrameworkName", "???" );
					item.CTID = GetRowColumn( dr, "CTID", "" );

					item.CredentialRegistryId = GetRowColumn( dr, "CredentialRegistryId" );
					DateTime testdate;
					//=====================================
					item.CreatedById = GetRowColumn( dr, "[CreatedById]", 0 );
					string date = GetRowPossibleColumn( dr, "[Created]", "" );
					if ( DateTime.TryParse( date, out testdate ) )
						item.Created = testdate;

					item.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
					date = GetRowPossibleColumn( dr, "LastUpdated", "" );
					if ( DateTime.TryParse( date, out testdate ) )
						item.LastUpdated = testdate;

					item.LastPublishedById = GetRowColumn( dr, "LastPublishedById", 0 );
					date = GetRowPossibleColumn( dr, "LastPublished", "" );
					if ( DateTime.TryParse( date, out testdate ) )
						item.LastPublished = testdate;

					item.LastApprovedById = GetRowColumn( dr, "LastApprovedById", 0 );
					date = GetRowPossibleColumn( dr, "LastApproved", "" );
					if ( DateTime.TryParse( date, out testdate ) )
						item.LastApproved = testdate;

					list.Add( item );
				}

				return list;

			}
		} //

		#endregion


	}
}
