using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using CM = Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBEntity = Data.CostManifest;
using ThisEntity = Models.Common.CostManifest;

namespace Factories
{
	public class CostManifestManager : BaseFactory
	{
		static string thisClassName = "Factories.CostManifestManager";

		List<string> messages = new List<string>();


		#region === -Persistance ==================
		/// <summary>
		/// Persist CostManifest
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool Save( ThisEntity entity, Guid parentUid, int userId, ref List<string> messages )
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

			DBEntity efEntity = new DBEntity();
			int parentOrgId = 0;

			Guid condtionManifestParentUid = new Guid();
			CM.Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_ORGANIZATION )
			{
				parentOrgId = parent.EntityBaseId;
				condtionManifestParentUid = parent.EntityUid;
				//no common Cost in this context
			}
			//else if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_COST_MANIFEST )
			//{
			//	//get 
			//	CM.CostManifest pcm = GetBasic( parent.EntityBaseId );
			//	parentOrgId = pcm.OrganizationId;
			//	commonCostParentUid = pcm.RowId;
			//	condtionManifestParentUid = pcm.OwningAgentUid;
			//}
			else
			{
				//should not happen - error Cost
				messages.Add( "Error: the parent for a Cost Manifest must be an organization." );
				return false;
			}

			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					entity.ParentId = parentOrgId;
					entity.OwningAgentUid = parent.EntityUid;

					if ( ValidateProfile( entity, ref messages ) == false )
					{
						return false;
					}

					if ( entity.Id == 0 )
					{
						//add
						efEntity = new DBEntity();
						FromMap( entity, efEntity );
						efEntity.OrganizationId = parentOrgId;

						efEntity.Created = efEntity.LastUpdated = DateTime.Now;
						efEntity.CreatedById = efEntity.LastUpdatedById = userId;
						efEntity.RowId = Guid.NewGuid();
                        if ( !string.IsNullOrWhiteSpace( entity.CTID ) && entity.CTID.Length == 39 )
                            efEntity.CTID = entity.CTID.ToLower();
                        else
                            efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();

                        context.CostManifest.Add( efEntity );
						count = context.SaveChanges();

						entity.Id = efEntity.Id;
						entity.RowId = efEntity.RowId;
						if ( count == 0 )
						{
							messages.Add( " Unable to add Cost Manifest Profile" );
						}
						else
						{
							//create the Entity.CostManifest
							//ensure to handle this properly when adding a commonCost CM to a CM
							EntityCostManifest_Add( condtionManifestParentUid, efEntity.Id, userId, ref messages );

							//if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_COST_MANIFEST )
							//{
							//	//if CM: add a common conditon for the CM
							//	new Entity_CommonCostManager().Add( commonCostParentUid, efEntity.Id, userId, ref messages );
							//}

							// a trigger is used to create the entity Object. 
							
						}
					}
					else
					{

						efEntity = context.CostManifest.SingleOrDefault( s => s.Id == entity.Id );
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
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{

					string message = HandleDBValidationError( dbex, thisClassName + ".Save() ", "CostManifest" );
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( dbex, thisClassName + string.Format( ".Save(), Parent: {0} ({1}), UserId: {2}", parent.EntityBaseName, parent.EntityBaseId, userId ) );
					isValid = false;
				}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex );
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save(), Parent: {0} ({1}), UserId: {2}", parent.EntityBaseName, parent.EntityBaseId, userId ) );
					isValid = false;
				}

			}

			return isValid;
		}

		/// <summary>
		/// Update credential registry id, and set status published
		/// </summary>
		/// <param name="costManifestId"></param>
		/// <param name="envelopeId"></param>
		/// <param name="userId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool UpdateEnvelopeId( int costManifestId, string envelopeId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					EM.CostManifest efEntity = context.CostManifest
									.SingleOrDefault( s => s.Id == costManifestId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						efEntity.CredentialRegistryId = envelopeId;

						if ( HasStateChanged( context ) )
						{
							//don't set updated for this action
							//efEntity.LastUpdated = System.DateTime.Now;
							//efEntity.LastUpdatedById = userId;

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
								string message = string.Format( thisClassName + ". UpdateEnvelopeId Failed", "Attempted to update an EnvelopeId. The process appeared to not work, but was not an exception, so we have no message, or no clue. CostManifest: {0}, envelopeId: {1}, updatedById: {2}", costManifestId, envelopeId, userId );
								EmailManager.NotifyAdmin( thisClassName + ".UpdateEnvelopeId Failed", message );
							}
						}


					}
					else
					{
						statusMessage = "Error - update failed, as record was not found.";
					}
				}
				catch ( Exception ex )
				{
					statusMessage = FormatExceptions( ex );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UpdateEnvelopeId(), CostManifest: {0}, envelopeId: {1}, updatedById: {2}", costManifestId, envelopeId, userId ) );
					
				}
			}

			return isValid;
		}

		/// <summary>
		/// Reset credential registry id, and set status to in process
		/// </summary>
		/// <param name="costManifestId"></param>
		/// <param name="userId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool UnPublish( int costManifestId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					EM.CostManifest efEntity = context.CostManifest
									.SingleOrDefault( s => s.Id == costManifestId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						efEntity.CredentialRegistryId = null;

						if ( HasStateChanged( context ) )
						{
							efEntity.LastUpdated = System.DateTime.Now;
							efEntity.LastUpdatedById = userId;

							count = context.SaveChanges();
						}

						//can be zero if no data changed
						if ( count >= 0 )
						{
							isValid = true;
						}
						else
						{
							//?no info on error
							statusMessage = "Error - the update was not successful. ";
							string message = string.Format( thisClassName + ".UnPublish Failed", "Attempted to unpublish the CostManifest. The process appeared to not work, but was not an exception, so we have no message, or no clue. CostManifestId: {0}, updatedById: {1}", costManifestId, userId );
							EmailManager.NotifyAdmin( thisClassName + ".UnPublish Failed", message );
						}
					}
					else
					{
						statusMessage = "Error - update failed, as record was not found.";
					}
				}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UnPublish(), CostManifestId: {0}, updatedById: {1}", costManifestId, userId ) );
				}
			}

			return isValid;
		}
		/// <summary>
		/// Delete a Cost Manifest, and related Entity
		/// </summary>
		/// <param name="Id"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( int Id, ref string statusMessage )
		{
			bool isValid = false;
			if ( Id == 0 )
			{
				statusMessage = "Error - missing an identifier for the CostManifest";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					DBEntity efEntity = context.CostManifest
								.SingleOrDefault( s => s.Id == Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						Guid rowId = efEntity.RowId;

						context.CostManifest.Remove( efEntity );
						int count = context.SaveChanges();
						if ( count > 0 )
						{
							isValid = true;
							//do with trigger now
							//new EntityManager().Delete( rowId, ref statusMessage );
						}
					}
					else
					{
						statusMessage = "Error - delete failed, as record was not found.";
					}
				}
				catch ( Exception ex )
				{
					statusMessage = FormatExceptions( ex );
					LoggingHelper.LogError( ex, thisClassName + ".Delete()" );

					if ( statusMessage.ToLower().IndexOf( "the delete statement conflicted with the reference constraint" ) > -1 )
					{
						statusMessage = "Error: this Cost Manifest cannot be deleted as it is being referenced by other items, such as roles or credentials. These associations must be removed before this Cost Manifest can be deleted.";
					}
				}
			}

			return isValid;
		}


		public static bool ValidateProfile( ThisEntity profile, ref List<string> messages, bool validatingUrls = true )
		{
			bool isValid = true;
			int count = messages.Count;
			if ( profile.IsStarterProfile )
				return true;

			//&& ( profile.EstimatedCost == null || profile.EstimatedCost.Count == 0 )
			if ( string.IsNullOrWhiteSpace( profile.ProfileName ) )
			{
				messages.Add( "A Cost Manifest name must be entered" );
			}
			if ( string.IsNullOrWhiteSpace( profile.Description ) )
			{
				messages.Add( "A Cost Manifest Description must be entered" );
			}
			else if ( FormHelper.HasHtmlTags( profile.Description ) )
			{
				messages.Add( "HTML or Script Tags are not allowed in the description" );
			}
			else if ( profile.Description.Length < MinimumDescriptionLength && !IsDevEnv() )
			{
				messages.Add( string.Format( "The Cost Manifest description must be at least {0} characters in length.", MinimumDescriptionLength ) );
			}
			//added for use from publishing and check for minimum values
			if ( !IsGuidValid( profile.OwningAgentUid ) )
			{
				messages.Add( "An owning organization must be selected" );
			}


			if ( string.IsNullOrWhiteSpace( profile.CostDetails ) )
				messages.Add( "A Cost Details Url must be entered" );

			else if ( validatingUrls && !IsUrlValid( profile.CostDetails, ref commonStatusMessage ) )
			{
				messages.Add( "The Cost Detail Url is invalid " + commonStatusMessage );
			}
			DateTime startDate = DateTime.Now;
			DateTime endDate = DateTime.Now;
			if ( !string.IsNullOrWhiteSpace( profile.StartDate ) )
			{
				if ( !IsValidDate( profile.StartDate ) )
					messages.Add( "Please enter a valid start date" );
				else
				{
					DateTime.TryParse( profile.StartDate, out startDate );
				}
			}
			if ( !string.IsNullOrWhiteSpace( profile.EndDate ) )
			{
				if ( !IsValidDate( profile.EndDate ) )
					messages.Add( "Please enter a valid end date" );
				else
				{
					DateTime.TryParse( profile.EndDate, out endDate );
					if ( IsValidDate( profile.StartDate )
						&& startDate > endDate )
						messages.Add( "The end date must be greater than the start date." );
				}
			}
		

			if ( messages.Count > count )
				isValid = false;

			return isValid;
		}

		#endregion

		#region == Retrieval =======================
		public static ThisEntity Get( int id,
			bool forEditView = false )
		{
			ThisEntity entity = new ThisEntity();
			bool includingProfiles = false;
			if ( forEditView )
				includingProfiles = true;

			using ( var context = new Data.CTIEntities() )
			{
				DBEntity item = context.CostManifest
						.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity,
						true, //includingProperties
						includingProfiles,
						forEditView );
				}
			}

			return entity;
		}

		/// <summary>
		/// Get absolute minimum for display as profile link
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static ThisEntity GetBasic( int id )
		{
			ThisEntity entity = new ThisEntity();

			using ( var context = new Data.CTIEntities() )
			{
				//want to get org, deal with others
				//context.Configuration.LazyLoadingEnabled = false;

				DBEntity item = context.CostManifest
						.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					entity.Id = item.Id;
					entity.RowId = item.RowId;
					entity.Name = item.Name;
                    entity.CTID = item.CTID;
					entity.Description = item.Description;
					entity.CostDetails = item.CostDetails;
                    entity.CredentialRegistryId = item.CredentialRegistryId;
                    entity.OrganizationId = item.OrganizationId;
					entity.OwningOrganization = new CM.Organization();
					if ( item.OrganizationId > 0 )
					{
						if ( item.Organization != null && item.Organization.Id > 0 )
						{
							entity.OwningOrganization.Id = item.Organization.Id;
							entity.OwningOrganization.Name = item.Organization.Name;
							entity.OwningOrganization.RowId = item.Organization.RowId;
							entity.OwningOrganization.SubjectWebpage = item.Organization.URL;
							entity.OwningOrganization.CTID = item.CTID;
                        }
						else
						{
							entity.OwningOrganization = OrganizationManager.GetForSummary( entity.OrganizationId );
							entity.OwningAgentUid = entity.OwningOrganization.RowId;
						}

						entity.OrganizationName = entity.OwningOrganization.Name;
						entity.OwningAgentUid = entity.OwningOrganization.RowId;
					}
				}
			}

			return entity;
		}
		public static ThisEntity GetBasic( Guid rowID )
		{
			using ( var context = new Data.CTIEntities() )
			{
				var match = context.CostManifest.FirstOrDefault( m => m.RowId == rowID );
				return match == null ? null : GetBasic( match.Id );
			}
		}

        public static ThisEntity GetForApproval( int id, ref List<string> messages )
        {
            ThisEntity entity = GetBasic(id);
            ValidateProfile( entity, ref messages );

            return entity;
        }
        /// <summary>
        /// Get all the Cost manifests for the parent organization
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public static List<ThisEntity> GetAll( int orgId, bool isForLinks )
		{
			ThisEntity to = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();

			CM.Entity parent = EntityManager.GetEntity( 2, orgId );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<EM.Entity_CostManifest> results = context.Entity_CostManifest
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.Created )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( EM.Entity_CostManifest from in results )
						{
							to = new ThisEntity();
							if ( isForLinks )
							{
								to.Id = from.CostManifestId;
								to.RowId = from.CostManifest.RowId;
                                to.CTID = from.CostManifest.CTID;
                                to.OrganizationId = ( int ) from.Entity.EntityBaseId;
								to.OwningAgentUid = from.Entity.EntityUid;
								//
								to.ProfileName = from.CostManifest.Name + " ( " + from.CostManifest.CTID + " )";
                            }
							else
							{
								MapFromDB( from.CostManifest, to, true, true, false );
							}
							list.Add( to );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".GetAll(int orgId)" );
			}
			return list;
		}//
		 /// <summary>
		 /// Get all the Cost manifests for the parent entity (ex a credential)
		 /// </summary>
		 /// <param name="parentUid"></param>
		 /// <returns></returns>
		public static List<ThisEntity> GetAll( Guid parentUid, bool isForLinks )
		{
			ThisEntity to = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			CM.Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					//context.Configuration.LazyLoadingEnabled = false;

					List<EM.Entity_CostManifest> results = context.Entity_CostManifest
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.Created )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( EM.Entity_CostManifest from in results )
						{
							to = new ThisEntity();
							if ( isForLinks )
							{
								to.Id = from.CostManifest.Id;
								to.RowId = from.CostManifest.RowId;

								to.OrganizationId = ( int ) from.Entity.EntityBaseId;
								to.OwningAgentUid = from.Entity.EntityUid;
								to.ProfileName = from.CostManifest.Name;
							}
							else
							{
								MapFromDB( from.CostManifest, to, true, true, false );
							}
							list.Add( to );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".GetAll (Guid parentUid)" );
			}
			return list;
		}//

		public static List<ThisEntity> Search( int orgId, int pageNumber, int pageSize, ref int pTotalRows )
		{
			ThisEntity to = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			CM.Entity parent = EntityManager.GetEntity( 2, orgId );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<EM.Entity_CostManifest> results = context.Entity_CostManifest
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.Created )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( EM.Entity_CostManifest from in results )
						{
							to = new ThisEntity();

							to.Id = from.CostManifestId;
							to.RowId = from.CostManifest.RowId;
							to.Description = from.CostManifest.Description;
							to.OrganizationId = ( int ) from.Entity.EntityBaseId;
							to.OwningAgentUid = from.Entity.EntityUid;
							to.ProfileName = from.CostManifest.Name;

							list.Add( to );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Search()" );
			}

			return list;
		} //

		/// <summary>
		/// Search for a cost manifest.
		/// Currently should only allow where owned by the same org as the owning org of the current context. Will be used by batch processes, such as publishing
		/// </summary>
		/// <param name="pFilter"></param>
		/// <param name="pOrderBy"></param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSize"></param>
		/// <param name="userId"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
		public static List<ThisEntity> MainSearch( string pFilter, string pOrderBy, int pageNumber, int pageSize, int userId, ref int pTotalRows )
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

				using ( SqlCommand command = new SqlCommand( "[CostManifest_Search]", c ) )
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add( new SqlParameter( "@Filter", pFilter ) );
					command.Parameters.Add( new SqlParameter( "@SortOrder", pOrderBy ) );
					command.Parameters.Add( new SqlParameter( "@StartPageIndex", pageNumber ) );
					command.Parameters.Add( new SqlParameter( "@PageSize", pageSize ) );

					SqlParameter totalRows = new SqlParameter( "@TotalRows", pTotalRows );
					totalRows.Direction = ParameterDirection.Output;
					command.Parameters.Add( totalRows );

                    try
                    {
                        using ( SqlDataAdapter adapter = new SqlDataAdapter() )
					{
						adapter.SelectCommand = command;
						adapter.Fill( result );
					}
					string rows = command.Parameters[ 4 ].Value.ToString();
					
						pTotalRows = Int32.Parse( rows );
					}
                    catch ( Exception ex )
                    {
						pTotalRows = 0;
                        LoggingHelper.LogError( ex, thisClassName + string.Format( ".MainSearch() - Execute proc, Message: {0} \r\n Filter: {1} \r\n", ex.Message, pFilter ) );

                        item = new ThisEntity();
                        item.Name = "Unexpected error encountered. System administration has been notified. Please try again later. ";
                        item.Description = ex.Message;

                        list.Add( item );
                        return list;
                    }
				}

				foreach ( DataRow dr in result.Rows )
				{
					item = new ThisEntity();
					item.Id = GetRowColumn( dr, "Id", 0 );
					item.OrganizationId = GetRowColumn( dr, "OrganizationId", 0 );
					item.ProfileName = GetRowColumn( dr, "Name", "missing" );

					item.Description = GetRowColumn( dr, "Description", "" );

					string rowId = GetRowColumn( dr, "RowId" );
					item.RowId = new Guid( rowId );
					item.CTID = GetRowColumn( dr, "CTID" );
					item.CostDetails = GetRowColumn( dr, "CostDetails", "" );
                    item.EntityLastUpdated = GetRowColumn( dr, "EntityLastUpdated", System.DateTime.Now );

                    //published ==========================
                    string date = GetRowPossibleColumn( dr, "LastPublishDate", "" );
                    DateTime testdate;
                    if ( DateTime.TryParse( date, out testdate ) )
                    {
                        //item.IsPublished = true;
                        item.LastPublished = testdate;
                    }
                    //approvals ==========================
                    date = GetRowPossibleColumn( dr, "LastApprovalDate", "" );
                    if ( DateTime.TryParse( date, out testdate ) )
                    {
                        //item.IsApproved = true;
                        item.LastApproved = testdate;
                    }
                    //item.ContentApprovedBy = GetRowPossibleColumn( dr, "ContentApprovedBy" );
                    //item.ContentApprovedById = GetRowPossibleColumn( dr, "ContentApprovedById", 0 );
                    //if ( item.ContentApprovedById > 0 )
                    //    item.IsApproved = true;

                    list.Add( item );
				}

				return list;

			}
		} //

		public static void FromMap( ThisEntity from, DBEntity to )
		{

			//want to ensure fields from create are not wiped
			if ( to.Id == 0 )
			{

			}

			//don't map rowId, ctid, or dates as not on form

			to.Id = from.Id;
			//just in case handle in caller
			//to.OrganizationId = from.OrganizationId;
			to.Name = GetData( from.Name );
			to.Description = GetData( from.Description );
			to.CostDetails = NormalizeUrlData( from.CostDetails, null );
			if ( IsValidDate( from.StartDate ) )
				to.StartDate = DateTime.Parse( from.StartDate );
			else
				to.StartDate = null;

			if ( IsValidDate( from.EndDate ) )
				to.EndDate = DateTime.Parse( from.EndDate );
			else
				to.EndDate = null;


		}
		public static void MapFromDB( DBEntity from, ThisEntity to,
				bool includingProperties = false,
				bool includingProfiles = true,
				bool forEditView = true )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;

			to.OrganizationId = from.OrganizationId;

			if ( to.OrganizationId > 0 )
			{
				if ( from.Organization != null && from.Organization.Id > 0 )
				{
					//ensure there is no infinite loop
					//the following results in an infinite loop
					//OrganizationManager.ToMapCommon( from.Organization, to.OwningOrganization, false, false, false, false, false );
					//maybe: ToMapForSummary
					//OrganizationManager.ToMapForSummary( from.Organization, to.OwningOrganization );

					to.OwningOrganization = OrganizationManager.GetForSummary( to.OrganizationId );
					to.OwningAgentUid = to.OwningOrganization.RowId;
				}
				else
				{
					to.OwningOrganization = OrganizationManager.GetForSummary( to.OrganizationId );
					to.OwningAgentUid = to.OwningOrganization.RowId;
				}

				to.OrganizationName = to.OwningOrganization.Name;
				to.OwningAgentUid = to.OwningOrganization.RowId;
			}

			to.Name = from.Name;
			to.Description = from.Description == null ? "" : from.Description;

			to.CTID = from.CTID;
			to.CredentialRegistryId = from.CredentialRegistryId;
			to.EntityApproval = Entity_ApprovalManager.GetByParent( to.RowId );
            if ( to.EntityApproval != null && to.EntityApproval.Id > 0 )
                to.LastApproved = to.EntityApproval.Created;

            to.RelatedEntity = EntityManager.GetEntity( to.RowId, false );
            if ( to.RelatedEntity != null && to.RelatedEntity.Id > 0 )
                to.EntityLastUpdated = to.RelatedEntity.LastUpdated;

            to.CostDetails = from.CostDetails;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

			if ( from.Account_Modifier != null )
			{
				to.LastUpdatedBy = from.Account_Modifier.FirstName + " " + from.Account_Modifier.LastName;
			}
			else
			{
				AppUser user = AccountManager.AppUser_Get( to.LastUpdatedById );
				to.LastUpdatedBy = user.FullName();
			}
			

			if ( IsValidDate( from.StartDate ) )
				to.StartDate = ( ( DateTime ) from.StartDate ).ToShortDateString();
			else
				to.StartDate = "";

			if ( IsValidDate( from.EndDate ) )
				to.EndDate = ( ( DateTime ) from.EndDate ).ToShortDateString();
			else
				to.EndDate = "";
			

			//get Costs
			
			to.EstimatedCosts = CostProfileManager.GetAll( to.RowId, forEditView );


		}

		#endregion

		#region === EntityCostManifest ================

		/// <summary>
		/// Add an Entity_CommonCost
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="profileId"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public int EntityCostManifest_Add( Guid parentUid,
					int profileId,
					int userId,
					ref List<string> messages )
		{
			int id = 0;
			int count = messages.Count();
			if ( profileId == 0 )
			{
				messages.Add( string.Format( "A valid CostManifest identifier was not provided to the {0}.Add method.", thisClassName ) );
			}
			if ( messages.Count > count )
				return 0;

			CM.Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return 0;
			}
			using ( var context = new Data.CTIEntities() )
			{
				EM.Entity_CostManifest efEntity = new EM.Entity_CostManifest();
				try
				{
					//first check for duplicates
					efEntity = context.Entity_CostManifest
							.SingleOrDefault( s => s.EntityId == parent.Id
							&& s.CostManifestId == profileId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						messages.Add( string.Format( "Error - this CostManifest has already been added to this profile.", thisClassName ) );
						return 0;
					}

					efEntity = new EM.Entity_CostManifest();
					efEntity.EntityId = parent.Id;
					efEntity.CostManifestId = profileId;

					efEntity.CreatedById = userId;
					efEntity.Created = System.DateTime.Now;

					context.Entity_CostManifest.Add( efEntity );

					// submit the change to database
					count = context.SaveChanges();
					if ( count > 0 )
					{
						messages.Add( "Successful" );
						id = efEntity.Id;
						return efEntity.Id;
					}
					else
					{
						//?no info on error
						messages.Add( "Error - the add was not successful." );
						string message = thisClassName + string.Format( ".Add Failed", "Attempted to add a CostManifest for a profile. The process appeared to not work, but there was no exception, so we have no message, or no clue. Parent Profile: {0}, Type: {1}, learningOppId: {2}, createdById: {3}", parentUid, parent.EntityType, profileId, userId );
						EmailManager.NotifyAdmin( thisClassName + ".Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "Entity_CommonCost" );
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( dbex, thisClassName + string.Format( ".Save(), Parent: {0} ({1}), UserId: {2}", parent.EntityBaseName, parent.EntityBaseId, userId ) );

				}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex );
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save(), Parent: {0} ({1}), UserId: {2}", parent.EntityBaseName, parent.EntityBaseId, userId ) );

				}


			}
			return id;
		}
		public bool Delete_EntityCostManifest( Guid parentUid, int profileId, ref string statusMessage )
		{
			bool isValid = false;
			if ( profileId == 0 )
			{
				statusMessage = "Error - missing an identifier for the Assessment to remove";
				return false;
			}
			//need to get Entity.Id 
			CM.Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				statusMessage = "Error - the parent entity was not found.";
				return false;
			}

			using ( var context = new Data.CTIEntities() )
			{
				EM.Entity_CostManifest efEntity = context.Entity_CostManifest
								.SingleOrDefault( s => s.EntityId == parent.Id && s.CostManifestId == profileId );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					context.Entity_CostManifest.Remove( efEntity );
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					}
				}
				else
				{
					statusMessage = "Warning - the record was not found - probably because the target had been previously deleted";
					isValid = true;
				}
			}

			return isValid;
		}

		public static CM.Entity_CostManifest EntityCostManifest_Get( int parentId, int profileId )
		{
			CM.Entity_CostManifest entity = new CM.Entity_CostManifest();
			if ( parentId < 1 || profileId < 1 )
			{
				return entity;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					EM.Entity_CostManifest from = context.Entity_CostManifest
							.SingleOrDefault( s => s.CostManifestId == profileId && s.EntityId == parentId );

					if ( from != null && from.Id > 0 )
					{
						entity.Id = from.Id;
						entity.CostManifestId = from.CostManifestId;
						entity.EntityId = from.EntityId;
						//entity.CostManifest = CostManifestManager.GetBasic( from.CostManifestId );
						entity.ProfileSummary = entity.CostManifest.ProfileName;

						//entity.CostManifest = from.CostManifest;
						if ( IsValidDate( from.Created ) )
							entity.Created = ( DateTime ) from.Created;
						entity.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".EntityCostManifest_Get" );
			}
			return entity;
		}//
		//public static List<ThisEntity> GetAll_EntityCostManifests( Guid parentUid, bool forEditView )
		//{
		//	List<ThisEntity> list = new List<ThisEntity>();
		//	ThisEntity entity = new ThisEntity();
		//	CM.Entity parent = EntityManager.GetEntity( parentUid );

		//	try
		//	{
		//		using ( var context = new Data.CTIEntities() )
		//		{
		//			List<EM.Entity_CostManifest> results = context.Entity_CostManifest
		//					.Where( s => s.EntityId == parent.Id )
		//					.OrderBy( s => s.CostManifestId )
		//					.ToList();

		//			if ( results != null && results.Count > 0 )
		//			{
		//				foreach ( EM.Entity_CostManifest item in results )
		//				{
		//					//TODO - optimize the appropriate MapFromDB methods
		//					entity = new ThisEntity();
		//					CostManifestManager.MapFromDB( item.CostManifest, entity );
		//					//if ( forEditView )
		//					//	CostManifestManager.MapFromDB( item.CostManifest, entity,
		//					//	true, 
		//					//	forEditView
		//					//	);
		//					//else
		//					//	CostManifestManager.MapFromDB( item.CostManifest, entity,
		//					//	true,
		//					//	forEditView
		//					//	);

		//					list.Add( entity );
		//				}
		//			}
		//			return list;
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".GetAllManifests" );
		//	}
		//	return list;
		//}
		#endregion


	}
}
