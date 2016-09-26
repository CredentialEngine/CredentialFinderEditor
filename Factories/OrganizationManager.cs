using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;

using Models.Common;
using CM = Models.Common;
using MN = Models.Node;
using EM = Data;
using Utilities;
using Models.Search.ThirdPartyApiModels;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class OrganizationManager : BaseFactory
	{
		static string thisClassName = "OrganizationManager";
		List<string> messages = new List<string>();
		#region Constants
		public static int ORG_MEMBER_PENDING = 0;
		public static int ORG_MEMBER_ADMIN = 1;
		public static int ORG_MEMBER_EMPLOYEE = 2;
		public static int ORG_MEMBER_STUDENT = 3;
		public static int ORG_MEMBER_EXTERNAL = 4;
		#endregion
		#region Organization - persistance ==================

		/// <summary>
		/// add a Organization
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Organization_Add( CM.Organization entity, ref string statusMessage )
		{
			EM.Organization efEntity = new EM.Organization();
			OrganizationPropertyManager opMgr = new OrganizationPropertyManager();
			using ( var context = new Data.CTIEntities() )
			{
				try
				{

					FromMap( entity, efEntity );

					//efEntity.CTID = "urn:ctid:" + efEntity.RowId.ToString();

					efEntity.RowId = Guid.NewGuid();
					efEntity.CreatedById = entity.CreatedById;
					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdatedById = entity.CreatedById;
					efEntity.LastUpdated = System.DateTime.Now;

					context.Organization.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";
						entity.Id = efEntity.Id;
						entity.RowId = efEntity.RowId;

						//need to update the first AddressProfile
						//should no longer be necessary, a very least, ignore validation errors
						bool isOK = new AddressProfileManager().Save( entity.Address, entity.RowId, entity.CreatedById, ref messages );

						//TODO
						//notify admin - done in services
						//add current user as an admin member, or entity partner
						//check if has a primary orgId
						OrganizationMember_Save( efEntity.Id, entity.CreatedById, ORG_MEMBER_ADMIN, entity.CreatedById, ref statusMessage );

						UpdateParts( entity, ref statusMessage );
						return efEntity.Id;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the add was not successful. ";
						string message = string.Format( "OrganizationManager. Organization_Add Failed", "Attempted to add a Organization. The process appeared to not work, but was not an exception, so we have no message, or no clue.Organization: {0}, createdById: {1}", entity.Name, entity.CreatedById );
						EmailManager.NotifyAdmin( "OrganizationManager. Organization_Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					//LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
					string message = thisClassName + string.Format( ".Organization_Add() DbEntityValidationException, Name: {0}", efEntity.Name );
					statusMessage = "Error - missing fields. ";
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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Organization_Add(), Name: {0}", efEntity.Name ) );
				}
			}

			return efEntity.Id;
		}
		/// <summary>
		/// Update a Organization
		/// - base only, caller will handle parts?
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Organization_Update( CM.Organization entity, ref string statusMessage )
		{
			bool isValid = true;
			int count = 0;
			//OrganizationPropertyManager opMgr = new OrganizationPropertyManager();
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					EM.Organization efEntity = context.Organization
								.SingleOrDefault( s => s.Id == entity.Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						//for updates, chances are some fields will not be in interface, don't map these (ie created stuff)
						//**ensure rowId is passed down for use by profiles, etc
						entity.RowId = efEntity.RowId;

						FromMap( entity, efEntity );
						if ( HasStateChanged( context ) )
						{
							efEntity.LastUpdated = System.DateTime.Now;
							efEntity.LastUpdatedById = entity.LastUpdatedById;
							count = context.SaveChanges();
							//can be zero if no data changed
							if ( count >= 0 )
							{
								isValid = true;

								//need to update the first AddressProfile
								bool isOK = new AddressProfileManager().SyncOldAddressToNew( entity.Address, entity.RowId, entity.Id, entity.LastUpdatedById, ref messages );
							}
							else
							{
								//?no info on error
								statusMessage = "Error - the update was not successful. ";
								isValid = false;
								string message = string.Format( "OrganizationManager. Organization_Update Failed", "Attempted to update a Organization. The process appeared to not work, but was not an exception, so we have no message, or no clue. Organization: {0}, Id: {1}, updatedById: {2}", entity.Name, entity.Id, entity.LastUpdatedById );
								EmailManager.NotifyAdmin( "OrganizationManager. Organization_Update Failed", message );
							}
						}
						//continue with parts regardless
						if ( !UpdateParts( entity, ref statusMessage ) )
							isValid = false;

					}
					else
					{
						statusMessage = "Error - update failed, as record was not found.";
						isValid = false;
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + string.Format(".Organization_Update. id: {0}", entity.Id) );
				isValid = false;
			}
			

			return isValid;
		}

		/// <summary>
		/// Update credential registry id, and set status published
		/// </summary>
		/// <param name="recordId"></param>
		/// <param name="envelopeId"></param>
		/// <param name="userId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool UpdateEnvelopeId( int recordId, string envelopeId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					EM.Organization efEntity = context.Organization
									.SingleOrDefault( s => s.Id == recordId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						efEntity.CredentialRegistryId = envelopeId;
						efEntity.StatusId = CodesManager.ENTITY_STATUS_PUBLISHED;

						if ( HasStateChanged( context ) )
						{
							efEntity.LastUpdated = System.DateTime.Now;
							efEntity.LastUpdatedById = userId;

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
								string message = string.Format( thisClassName + ".UpdateEnvelopeId Failed", "Attempted to update an EnvelopeId. The process appeared to not work, but was not an exception, so we have no message, or no clue. Organization: {0}, envelopeId: {1}, updatedById: {2}", recordId, envelopeId, userId );
								EmailManager.NotifyAdmin( thisClassName + ". UpdateEnvelopeId Failed", message );
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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UpdateEnvelopeId(), Organization: {0}, envelopeId: {1}, updatedById: {2}", recordId, envelopeId, userId ) );
					statusMessage = ex.Message + ( ex.InnerException != null && ex.InnerException.InnerException != null ? " - " + ex.InnerException.InnerException.Message : "" );
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
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					EM.Organization efEntity = context.Organization
									.SingleOrDefault( s => s.Id == recordId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						efEntity.CredentialRegistryId = null;
						efEntity.StatusId = CodesManager.ENTITY_STATUS_IN_PROGRESS;

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
							string message = string.Format( thisClassName + ".UnPublish Failed", "Attempted to reset a EnvelopeId. The process appeared to not work, but was not an exception, so we have no message, or no clue. Organization: {0}, updatedById: {1}", recordId, userId );
							EmailManager.NotifyAdmin( thisClassName + ". UnPublish Failed", message );
						}
					}
					else
					{
						statusMessage = "Error - update failed, as record was not found.";
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UnPublish(), Organization: {0}, updatedById: {1}", recordId, userId ) );
					statusMessage = ex.Message + ( ex.InnerException != null && ex.InnerException.InnerException != null ? " - " + ex.InnerException.InnerException.Message : "" );
				}
			}

			return isValid;
		}
		public bool UpdateParts( Organization entity,ref string statusMessage )
		{
			bool isAllValid = true;
			int count = 0;
			List<string> messages = new List<string>();

			if ( UpdateProperties( entity, ref messages ) == false )
				isAllValid = false;

			if ( !new OrganizationServiceManager().OrganizationService_Update( entity, false, ref statusMessage ) )
				isAllValid = false;
			//handle jurisdictions? ==> direct

			Entity_ReferenceManager erm = new
				Entity_ReferenceManager();

			if ( erm.EntityUpdate( entity.Keywords, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_KEYWORD, false ) == false )
				isAllValid = false;

			

			//departments

			//address - separate

			//subsiduaries
			if ( messages.Count > 0)
				statusMessage += string.Join( ",", messages.ToArray() );

			return isAllValid;
}
		public bool UpdateProperties( Organization entity, ref List<string> messages )
		{
			bool isAllValid = true;
			//==== convert to entity properties ========================
			OrganizationPropertyManager opMgr = new OrganizationPropertyManager();
			EntityPropertyManager mgr = new EntityPropertyManager();
			Entity_ReferenceManager erm = new Entity_ReferenceManager();

			if ( mgr.UpdateProperties( entity.OrganizationType, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;


			if ( mgr.UpdateProperties( entity.QAPurposeType, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, CodesManager.PROPERTY_CATEGORY_AGENT_QAPURPOSE_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			//if ( entity.IsNewVersion == false )
			//{
			//	if ( !opMgr.UpdateProperties( entity, ref messages ) )
			//		isAllValid = false;

			//	if ( mgr.UpdateProperties( entity.OrganizationSectorType, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SECTORTYPE, entity.LastUpdatedById, ref messages ) == false )
			//		isAllValid = false;
			//}
			//else
			//{

				if ( mgr.UpdateProperties( entity.OrganizationSectorType, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SECTORTYPE, entity.LastUpdatedById, ref messages ) == false )
				{
					isAllValid = false;
				}

				if ( erm.EntityUpdate( entity.SocialMediaPages, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SOCIAL_MEDIA ) == false )
					isAllValid = false;

			//how to handle notifications on 'other'?
				if ( erm.EntityUpdate( entity.IdentificationCodes, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_IDENTIFIERS ) == false )
					isAllValid = false;

				if ( erm.EntityUpdate( entity.Emails, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_EMAIL_TYPE ) == false )
					isAllValid = false;

				if ( erm.EntityUpdate( entity.PhoneNumbers, entity.RowId, CodesManager.ENTITY_TYPE_ORGANIZATION, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_PHONE_TYPE ) == false )
					isAllValid = false;

			//}
			return isAllValid;
		}
		/// <summary>
		/// Delete an Organization, and related Entity
		/// </summary>
		/// <param name="orgId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Organization_Delete( int orgId, ref string statusMessage )
		{
			bool isValid = false;
			if ( orgId == 0 )
			{
				statusMessage = "Error - missing an identifier for the Organization";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					//ensure exists
					EM.Organization efEntity = context.Organization
								.SingleOrDefault( s => s.Id == orgId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						Guid rowId = efEntity.RowId;
						int roleCount = 0;
						//check for any existing org roles, and reject delete if any found
						if ( Entity_AgentRelationshipManager.AgentEntityHasRoles( rowId, ref roleCount ) )
						{
							statusMessage = string.Format("Error - this organization cannot be deleted as there are existing roles {0}.", roleCount);
							return false;
						}
						

						context.Organization.Remove( efEntity );
						int count = context.SaveChanges();
						if ( count > 0 )
						{
							isValid = true;
							new EntityManager().Delete( rowId, ref statusMessage );
						}
					}
					else
					{
						statusMessage = "Error - delete failed, as record was not found.";
					}
				}
				catch ( Exception ex )
				{
					statusMessage = ex.Message;
					LoggingHelper.LogError( ex, thisClassName + ".Organization_Delete()" );
					if ( ex.InnerException != null && ex.InnerException.Message != null )
					{
						statusMessage = ex.InnerException.Message;

						if ( ex.InnerException.InnerException != null && ex.InnerException.InnerException.Message != null )
							statusMessage = ex.InnerException.InnerException.Message;
					}
					if ( statusMessage.ToLower().IndexOf( "the delete statement conflicted with the reference constraint" ) > -1 )
					{
						statusMessage = "Error: this organization cannot be deleted as it is being referenced by other items, such as credentials. These associations must be removed before this organization can be deleted.";
					}
				}
			}

			return isValid;
		}

		#endregion

		#region == Retrieval =======================
		public static CM.Organization Organization_GetDetail( int id )
		{
			bool isNewVersion = true;
			bool includeCredentials = true;
			bool includingRoles = true;
			CM.Organization entity = new CM.Organization();
			entity.IsNewVersion = isNewVersion;

			using ( var context = new Data.CTIEntities() )
			{
				EM.Organization item = context.Organization
						.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					ToMapForDetail( item, entity, true, includeCredentials, includingRoles );
				}
			}

			return entity;
		}
		public static CM.Organization Organization_Get( int id, bool includeCredentials = false, bool includingRoles = false, bool isNewVersion = true )
		{
			CM.Organization entity = new CM.Organization();
			entity.IsNewVersion = isNewVersion;
			using ( var context = new Data.CTIEntities() )
			{
				EM.Organization item = context.Organization
						.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, true, includeCredentials, includingRoles );
				}
			}

			return entity;
		}
		public static CM.Organization Organization_Get( Guid agentId, bool includingProperties = true, bool includeCredentials = false, bool includingRoles = false )
		{
			CM.Organization entity = new CM.Organization();
			using ( var context = new Data.CTIEntities() )
			{

				EM.Organization item = context.Organization
						.Include( "Organization_Property" )
						.Include( "Organization_PropertyOther" )
						.SingleOrDefault( s => s.RowId == agentId );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, includingProperties, includeCredentials, includingRoles );
				}
			}

			return entity;
		}
		public static CM.Organization Agent_Get( Guid agentRowId, bool includeCredentials = false )
		{
			CM.Organization to = new CM.Organization();
			using ( var context = new ViewContext() )
			{
				//HACK note- there is currently only one organization property type, so we can get all. 
				//In the case of multiple properties (ie creds), need to use a view to get selectively - or add more includes
				//						.Include( "Codes_PropertyValue" )
				Views.Agent_Summary fromEntity = context.Agent_Summary
						.SingleOrDefault( s => s.AgentRowId == agentRowId );

				if ( fromEntity != null && fromEntity.AgentType != null && fromEntity.AgentType.Length > 4 )
				{
					//ToMap( item, entity, true );
					to.RowId = fromEntity.AgentRowId;

					to.Name = fromEntity.AgentName + " (" + fromEntity.AgentType + ")";
					
					to.Email = fromEntity.Email;
					to.Address.AddressRegion = fromEntity.Region;
					to.Address.Country = fromEntity.Country;
				}
			}

			return to;
}

		public static MN.ProfileLink Agent_GetProfileLink( Guid agentId )
		{
			MN.ProfileLink entity = new MN.ProfileLink();
			using ( var context = new ViewContext() )
			{
				Views.Agent_Summary efEntity = context.Agent_Summary
						.SingleOrDefault( s => s.AgentRowId == agentId );

				if ( efEntity != null && efEntity.AgentRelativeId > 0 )
				{
					entity.RowId = efEntity.AgentRowId;
					entity.Id = efEntity.AgentRelativeId;
					entity.Name = efEntity.AgentName;
					entity.Type = typeof( Models.Node.Organization );
				}
			}

			return entity;
		}

		/// <summary>
		/// Get agent using relativeId - this should require an entity typeId, to avoid duplicates
		/// </summary>
		/// <param name="agentId"></param>
		/// <returns></returns>
		public static MN.ProfileLink Agent_GetProfileLink( int agentId, int agentTypeId = 2 )
		{
			MN.ProfileLink entity = new MN.ProfileLink();
			using ( var context = new ViewContext() )
			{
				Views.Agent_Summary efEntity = context.Agent_Summary
						.SingleOrDefault( s => s.AgentRelativeId == agentId 
							&& s.AgentTypeId == agentTypeId );

				if ( efEntity != null && efEntity.AgentRelativeId > 0 )
				{
					entity.RowId = efEntity.AgentRowId;
					entity.Id = efEntity.AgentRelativeId;
					entity.Name = efEntity.AgentName;
					entity.Type = typeof( Models.Node.Organization );
				}
			}

			return entity;
		}
		/// <summary>
		/// Retrieve a list of all orgs by name
		/// </summary>
		/// <returns></returns>
		public static List<Organization> Organization_ListByName( int userId, string keyword, int maxTerms = 25 )
		{
			int pTotalRows = 0;
			return QuickSearch( userId, keyword, 1, maxTerms, ref pTotalRows );
		}

		public static List<Organization> QuickSearch( int userId, string keyword, ref int pTotalRows )
		{
			return QuickSearch( userId, keyword, 1, 200, ref pTotalRows );
		}

		/// <summary>
		/// Retrieve a list of all orgs by name
		/// </summary>
		/// <returns></returns>
		public static List<Organization> QuickSearch( int userId, string keyword, int pageNumber, int pageSize, ref int pTotalRows )
		{
			List<CM.Organization> list = new List<CM.Organization>();
			CM.Organization entity = new CM.Organization();
			keyword = string.IsNullOrWhiteSpace(keyword) ? ""  : keyword.Trim();
			if ( pageSize == 0 )
				pageSize = 500;
			int skip = 0;
			if ( pageNumber > 1 )
				skip = ( pageNumber - 1 ) * pageSize;

			using ( var context = new Data.CTIEntities() )
			{
				var Query = from Results in context.Organization
						.Where( s => keyword == "" || s.Name.Contains( keyword ) )
						.OrderBy( s => s.Name )
							select Results;

				pTotalRows = Query.Count();
				var results = Query.Skip( skip ).Take( pageSize )
					.ToList();

				//List<EM.Organization> results2 = context.Organization
				//	.Where( s => keyword == "" || s.Name.Contains( keyword ) )
				//	.Take( pageSize )
				//	.OrderBy( s => s.Name )
				//	.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( EM.Organization item in results )
					{
						entity = new CM.Organization();
						ToMap( item, entity, false );
						list.Add( entity );
					}

					//Other parts
				}
			}

			return list;
		}

		public static List<Organization> Agent_Search( int userId, string keyword, int pageNumber, int pageSize, ref int pTotalRows )
		{
			List<CM.Organization> list = new List<CM.Organization>();
			CM.Organization entity = new CM.Organization();
			keyword = string.IsNullOrWhiteSpace(keyword) ? ""  : keyword.Trim();
			if ( pageSize == 0 )
				pageSize = 500;
			int skip = 0;
			if ( pageNumber > 1 )
				skip = ( pageNumber - 1 ) * pageSize;

			using ( var context = new ViewContext() )
			{
				var Query = from Results in context.Agent_Summary
						.Where( s => keyword == "" || s.AgentName.Contains( keyword ) )
						.OrderBy( s => s.AgentName )
							select Results;

				pTotalRows = Query.Count();
				var results = Query.Skip( skip ).Take( pageSize )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Views.Agent_Summary fromEntity in results )
					{
						entity = new CM.Organization();
						entity.RowId = fromEntity.AgentRowId;
						//include relativeid for use where understand the difference
						entity.Id = fromEntity.AgentRelativeId;
						entity.Name = fromEntity.AgentName;
						entity.Email = fromEntity.Email;
						entity.Address.AddressRegion = fromEntity.Region;
						entity.Address.Country = fromEntity.Country;
						list.Add( entity );
					}

					//Other parts
				}
			}

			return list;
		}
		
		public static List<Organization> Organization_SelectQAOrgs( int userId = 0, string keyword = "", int maxTerms = 0 )
		{
			List<Organization> list = new List<Organization>();
			Organization entity = new Organization();
			keyword = keyword.Trim();
			if ( maxTerms == 0 )
				maxTerms = 500;

			using ( var context = new ViewContext() )
			{
				List<Views.Organization_Summary> results = context.Organization_Summary
						.Where( s => s.IsAQAOrganization > 0 
						   && ( keyword == "" || s.Name.Contains( keyword ) ) )
						.Take( maxTerms )
						.OrderBy( s => s.Name )
						.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Views.Organization_Summary item in results )
					{
						entity = new Organization();
						entity.Id = item.Id;
						entity.RowId = item.RowId;
						entity.Name = item.Name;
						if ( string.IsNullOrWhiteSpace( item.City ) == false )
							entity.Name += " ( " + item.City + " )";

						list.Add( entity );
					}
				}
			}

			return list;
		}

		public static List<Organization> Organization_SelectAll( int userId = 0 )
		{
			List<Organization> list = new List<Organization>();
			Organization entity = new Organization();
			using ( var context = new Data.CTIEntities() )
			{
				List<EM.Organization> results = context.Organization
									.OrderBy( s => s.Name )
									.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( EM.Organization item in results )
					{
						entity = new Organization();
						entity.Id = item.Id;
						entity.RowId = item.RowId;
						entity.Name = item.Name;
						//if ( string.IsNullOrWhiteSpace( item.Address ) == false )
						//	entity.Name += " ( " + item.Address.City + " )";
						
						list.Add( entity );
					}
				}
			}

			return list;
		}

		public static List<Organization> Search( string pFilter, string pOrderBy, int pageNumber, int pageSize, ref int pTotalRows, int userId = 0 )
		{
			string connectionString = DBConnectionRO();
			Organization item = new Organization();
			List<Organization> list = new List<Organization>();
			var result = new DataTable();
			using ( SqlConnection c = new SqlConnection( connectionString ) )
			{
				c.Open();

				if ( string.IsNullOrEmpty( pFilter ) )
				{
					pFilter = "";
				}

				using ( SqlCommand command = new SqlCommand( "OrganizationSearch", c ) )
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add( new SqlParameter( "@Filter", pFilter ) );
					command.Parameters.Add( new SqlParameter( "@SortOrder", pOrderBy ) );
					command.Parameters.Add( new SqlParameter( "@StartPageIndex", pageNumber ) );
					command.Parameters.Add( new SqlParameter( "@PageSize", pageSize ) );
					command.Parameters.Add( new SqlParameter( "@CurrentUserId", userId ) );

					SqlParameter totalRows = new SqlParameter( "@TotalRows", pTotalRows );
					totalRows.Direction = ParameterDirection.Output;
					command.Parameters.Add( totalRows );

					using ( SqlDataAdapter adapter = new SqlDataAdapter() )
					{
						adapter.SelectCommand = command;
						adapter.Fill( result );
					}
					string rows = command.Parameters[ 5 ].Value.ToString();
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
					item = new Organization();
					item.Id = GetRowColumn(dr, "Id", 0);
					item.Name = GetRowColumn(dr, "Name", "missing");
					item.Description = GetRowColumn( dr, "Description", "" );
					string rowId = GetRowColumn( dr, "RowId" );
						item.RowId = new Guid(rowId);

					item.Url = GetRowColumn( dr, "URL", "" );
					item.CanEditRecord = GetRowColumn( dr, "CanEditRecord", false );
					item.CredentialRegistryId = GetRowPossibleColumn( dr, "CredentialRegistryId", "" );

					item.ImageUrl = GetRowColumn( dr, "ImageUrl", "" );
					if ( GetRowColumn( dr, "CredentialCount", 0 ) > 0 )
						item.IsACredentialingOrg = true;
					item.IsAQAOrg = GetRowColumn( dr, "IsAQAOrganization", false );

					item.MainPhoneNumber = PhoneNumber.DisplayPhone( GetRowColumn( dr, "MainPhoneNumber", "" ) );
					//item.Address.Address1 = GetRowColumn( dr, "Address1", "" );
					//item.Address.Address2 = GetRowColumn( dr, "Address2", "" );
					//item.Address.City = GetRowColumn( dr, "City", "" );
					//item.Address.AddressRegion = GetRowColumn( dr, "Region", "" );
					//item.Address.PostalCode = GetRowColumn( dr, "PostalCode", "" );
					//item.Address.Country = GetRowColumn( dr, "Country", "" );

					//item.Address.Latitude = GetRowColumn( dr, "Latitude", 0D );
					//item.Address.Longitude = GetRowColumn( dr, "Longitude", 0D );
					//all addressess
					int addressess = GetRowPossibleColumn( dr, "AvailableAddresses", 0 );
					if ( addressess > 0   )
					{
						item.Addresses = AddressProfileManager.GetAllOrgAddresses( item.Id );
						//just in case (short term
						if ( item.Addresses.Count > 0 )
							item.Address = item.Addresses[ 0 ];
					}
					list.Add(item);
				}

				return list;

			}
		}
		
		public static void FromMap( CM.Organization fromEntity, EM.Organization to )
		{
			
			//want to ensure fields from create are not wiped
			if ( to.Id > 0 )
			{
				//to.RowId = fromEntity.rowId;
				if ( fromEntity.StatusId > 0 )
					to.StatusId = fromEntity.StatusId;
			}
			else
			{
				if ( IsValidDate( fromEntity.Created ) )
					to.Created = fromEntity.Created;
				to.CreatedById = fromEntity.CreatedById;
				to.StatusId = 1;
			}

			to.Id = fromEntity.Id;
			to.Name = fromEntity.Name != null ? fromEntity.Name.Trim() : null;
			to.Description = fromEntity.Description != null ? fromEntity.Description.Trim() : null;
			to.Purpose = fromEntity.Purpose != null ? fromEntity.Purpose.Trim() : null;

			//if ( fromEntity.IsNewVersion == false )
			//{
			//	bool hasChanged = false;
			//	bool hasAddress = false;
			//	if ( fromEntity.Address != null )
			//	{
			//		if ( fromEntity.Address.HasAddress() )
			//		{
			//			hasAddress = true;
			//			if ( to.Latitude == null || to.Latitude == 0 )
			//				hasChanged = true;
			//		}
			//		if ( hasChanged == false )
			//		{
			//			if ( to.Id == 0 )
			//				hasChanged = true;
			//			else
			//				hasChanged = HasAddressChanged( fromEntity, to );
			//		}


			//		to.Address1 = fromEntity.Address.Address1;
			//		to.Address2 = fromEntity.Address.Address2;
			//		to.City = fromEntity.Address.City;
			//		to.PostalCode = fromEntity.Address.PostalCode;
			//		to.Region = fromEntity.Address.AddressRegion;
			//		to.Country = fromEntity.Address.Country;

			//	}


			//	//these will likely not be present? 
			//	//If new, or address has changed, do the geo lookup
			//	if ( hasAddress )
			//	{
			//		if ( hasChanged )
			//		{
			//			GoogleGeocoding.Results results = GeoServices.GeocodeAddress( fromEntity.Address.DisplayAddress() );
			//			if ( results != null )
			//			{
			//				GoogleGeocoding.Location location = results.GetLocation();
			//				if ( location != null )
			//				{
			//					to.Latitude = location.lat;
			//					to.Longitude = location.lng;
			//				}
			//			}
			//		}
			//	}
			//	else
			//	{
			//		to.Latitude = 0;
			//		to.Longitude = 0;
			//	}
			//}

			//if ( fromEntity.IsNewVersion == false )
			//{
			//	to.Email = GetData( fromEntity.Email );
			//	to.MainPhoneNumber = PhoneNumber.StripPhone( GetData( fromEntity.MainPhoneNumber ) );
			//	to.TollFreeNumber = PhoneNumber.StripPhone( GetData( fromEntity.TollFreeNumber ) );
			//	to.FaxNumber = PhoneNumber.StripPhone( GetData( fromEntity.FaxNumber ) );
			//	to.TTY = PhoneNumber.StripPhone( GetData( fromEntity.TTYNumber ) );
			//}
			

			//FoundingDate is now a string
			//interface must handle? Or do we have to fix here?
			//depends if just text is passed or separates
			//this should be removed soon
			if ( !string.IsNullOrWhiteSpace( fromEntity.FoundingDate ) )
				to.FoundingDate = fromEntity.FoundingDate;
			to.FoundingDate = FormatFoundingDate( fromEntity );
			
			//if ( IsValidDate( fromEntity.FoundingDateOld ) )
			//	to.FoundingDateOld = fromEntity.FoundingDateOld;
			//else
			//	to.FoundingDateOld = null;

			if ( fromEntity.ServiceType != null )
			{
				to.ServiceTypeOther = fromEntity.ServiceType.OtherValue;
			}
			//to.ServiceTypeOther = fromEntity.ServiceTypeOther;
			to.URL = fromEntity.Url;
			to.UniqueURI = fromEntity.UniqueURI;
			
			if ( fromEntity.ImageUrl != null && fromEntity.ImageUrl.Trim().Length > 0 )
				to.ImageURL = fromEntity.ImageUrl;
			else
				to.ImageURL = null;
			
			if ( IsValidDate(fromEntity.LastUpdated) )
				to.LastUpdated = fromEntity.LastUpdated;
			to.LastUpdatedById = fromEntity.LastUpdatedById;
		}
		//public static bool HasAddressChanged( CM.Organization fromEntity, EM.Organization to )
		//{
		//	bool hasChanged = false;

		//	if ( to.Address1 != fromEntity.Address.Address1
		//	|| to.Address2 != fromEntity.Address.Address2
		//	|| to.City != fromEntity.Address.City
		//	|| to.PostalCode != fromEntity.Address.PostalCode
		//	|| to.Region != fromEntity.Address.AddressRegion
		//	|| to.Country != fromEntity.Address.Country )
		//		hasChanged = true;

		//	return hasChanged;
		//}
		private static string FormatFoundingDate( CM.Organization fromEntity )
		{
			string foundingDate = "";
			if ( !string.IsNullOrWhiteSpace( fromEntity.FoundingYear ) && fromEntity.FoundingYear.Length == 4 && IsInteger( fromEntity.FoundingYear ) )
				foundingDate = fromEntity.FoundingYear;
			else
				return ""; //must have at least a year

			if ( !string.IsNullOrWhiteSpace( fromEntity.FoundingMonth )
				&& IsInteger( fromEntity.FoundingMonth ) )
			{
				if ( fromEntity.FoundingMonth.Length == 1 )
					fromEntity.FoundingMonth = "0" + fromEntity.FoundingMonth;
				foundingDate += "-" + fromEntity.FoundingMonth;
			}
			else
				return foundingDate;

			if ( !string.IsNullOrWhiteSpace( fromEntity.FoundingDay )
				&& IsInteger( fromEntity.FoundingDay ) )
			{
				if ( fromEntity.FoundingDay.Length == 1 )
					fromEntity.FoundingDay = "0" + fromEntity.FoundingDay;

				foundingDate += "-" + fromEntity.FoundingDay;
			}

			return foundingDate;
}
		public static void ToMap( EM.Organization fromEntity, CM.Organization to,
					bool includingProperties = false,
					bool includeCredentials = false,
					bool includingRoles = false )
		{
			ToMap( fromEntity, to, true, true, includeCredentials, includingRoles );
		}
		public static void ToMapForDetail( EM.Organization fromEntity, CM.Organization to,
					bool includingProperties,
					bool includeCredentials,
					bool includingRoles )
		{
			ToMap( fromEntity, to, false, true, includeCredentials, includingRoles );
		}
		public static void ToMap( EM.Organization fromEntity, CM.Organization to,
 					bool forEditView,
					bool includingProperties, 
					bool includeCredentials,
					bool includingRoles )
		{
			to.Id = fromEntity.Id;
			to.RowId = fromEntity.RowId;

			to.Name = fromEntity.Name;
			to.Description = fromEntity.Description;
			to.Purpose = fromEntity.Purpose;
			to.CredentialRegistryId = fromEntity.CredentialRegistryId;

			//soon replace
			FillAddresses( fromEntity, to );
			//with
			//to.Addresses = AddressProfileManager.GetAll( to.RowId );

			//OLD
			//to.Email = fromEntity.Email;
			//to.MainPhoneNumber = PhoneNumber.DisplayPhone( fromEntity.MainPhoneNumber );
			//to.TollFreeNumber = PhoneNumber.DisplayPhone( fromEntity.TollFreeNumber );
			//to.FaxNumber = PhoneNumber.DisplayPhone( fromEntity.FaxNumber );
			//to.TTYNumber = PhoneNumber.DisplayPhone( fromEntity.TTY );

			to.FoundingDate = fromEntity.FoundingDate;
			//if ( IsValidDate( fromEntity.FoundingDateOld ) )
			//	to.FoundingDateOld = ( DateTime ) fromEntity.FoundingDateOld;

			if ( !string.IsNullOrWhiteSpace( fromEntity.FoundingDate ) )
			{
				string[] array = fromEntity.FoundingDate.Split( '-' );
				if ( array.Length > 0 )
				{
					to.FoundingYear = array[ 0 ];
					to.FoundingDate = to.FoundingYear;
				}
				if ( array.Length > 1 )
				{
					to.FoundingMonth = array[ 1 ];
					to.FoundingDate += "-" + to.FoundingMonth;
				}
				if ( array.Length > 2 )
				{
					to.FoundingDay = array[ 2 ];
					to.FoundingDate += "-" + to.FoundingDay;
				}
			}
			//not used from db? Using property other - except this is now the only other property????
			//16-09-02 mp - push to enumeratoin
			to.ServiceTypeOther = fromEntity.ServiceTypeOther;

			to.Url = fromEntity.URL;
			to.UniqueURI = fromEntity.UniqueURI;

			if ( fromEntity.ImageURL != null && fromEntity.ImageURL.Trim().Length > 0 )
				to.ImageUrl = fromEntity.ImageURL;
			else
				to.ImageUrl = null;

			if ( IsValidDate(fromEntity.Created) )
				to.Created = ( DateTime ) fromEntity.Created;
			to.CreatedById = fromEntity.CreatedById == null ? 0 : ( int ) fromEntity.CreatedById;
			if ( IsValidDate(fromEntity.LastUpdated) )
				to.LastUpdated = ( DateTime ) fromEntity.LastUpdated;
			to.LastUpdatedById = fromEntity.LastUpdatedById == null ? 0 : ( int ) fromEntity.LastUpdatedById;

			//sector type? - as an enumeration, will be stored in properties
			to.OrganizationSectorType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SECTORTYPE );

			//to.Subjects = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SUBJECT );

			to.Keywords = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD );
			//properties
			if ( includingProperties )
			{
				OrganizationServiceManager.FillOrganizationService( fromEntity, to );

				to.Jurisdiction = RegionsManager.Jurisdiction_GetAll( to.RowId );

				to.OrganizationType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_TYPE );

				to.QAPurposeType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_AGENT_QAPURPOSE_TYPE );

				//if ( to.IsNewVersion == false)
				//{
				//	//OrganizationPropertyManager.OrganizationPropertyFill_ToMap( fromEntity, to );
				//	//OrganizationPropertyManager.FillOrganizationType( fromEntity, to );
				//	OrganizationPropertyManager.FillSocialMedia( fromEntity, to );
				//	OrganizationPropertyManager.FillOrganizationIdentities( fromEntity, to );
				//}
				//else
				//{
					//OrganizationPropertyManager.FillOrganizationType( fromEntity, to );

					to.SocialMediaPages =	Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SOCIAL_MEDIA );
					to.IdentificationCodes = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_ORGANIZATION_IDENTIFIERS );
					to.PhoneNumbers = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_PHONE_TYPE );
					to.Emails = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_EMAIL_TYPE );

				//}
			}

			//credentialing?
			if ( includeCredentials )
			{
				to.CreatedCredentials = Entity_AgentRelationshipManager.Credentials_ForCreatingOrg(to.RowId);
				if ( to.CreatedCredentials != null && to.CreatedCredentials.Count > 0 )
					to.IsACredentialingOrg = true;
			}
			else
			{
				if (CountCredentials(fromEntity) > 0)
					to.IsACredentialingOrg = true;
			}

			if ( includingRoles )
			{

				if ( forEditView )
					to.OrganizationRole_Recipient = Entity_AgentRelationshipManager.AgentEntityRole_GetAllSummary( to.RowId, false );
				else
					to.OrganizationRole_Recipient = Entity_AgentRelationshipManager.AgentEntityRole_GetAll_ToEnumeration( to.RowId, true );

				to.QualityAssuranceAction = Entity_QualityAssuranceActionManager.QualityAssuranceActionProfile_GetAll( to.RowId );

				//dept and subsiduaries ????
				Entity_AgentRelationshipManager.AgentRole_FillAllSubOrganizations( to, 0 );
			
				//
				to.Authentication = Entity_VerificationProfileManager.VerificationProfile_GetAll( to.RowId );
			}
		}
		private static void FillAddresses( EM.Organization fromEntity, CM.Organization to )
		{
			to.Address = new Address();
			if ( fromEntity.Organization_Address != null && fromEntity.Organization_Address.Count > 0 )
			{
				Address address = new Address();
				int cntr = 0;
				foreach ( EM.Organization_Address item in fromEntity.Organization_Address )
				{
					cntr++;
					address = new Address();
					address.Id = item.Id;
					address.RowId = item.RowId;
					address.Name = item.Name;
					address.IsMainAddress = (bool) (item.IsPrimaryAddress ?? false);
					address.Address1 = item.Address1;
					address.Address2 = item.Address2;
					address.City = item.City;
					address.PostalCode = item.PostalCode;
					address.AddressRegion = item.Region;
					address.Country = item.Country;

					address.Latitude = item.Latitude ?? 0;
					address.Longitude = item.Longitude ?? 0;

					if ( fromEntity.Organization_Address.Count == 1 )
					{
						address.IsMainAddress = true;
					}

					to.Addresses.Add( address );
					if (address.IsMainAddress
						|| fromEntity.Organization_Address.Count == 1 )
					{
						to.Address = address;
					}

					//if first address, add to old address fields to allow for old editor
					if ( cntr == 1 )
					{
						//to.Address.Address1 = address.Address1;
						//to.Address.Address2 = address.Address2;
						//to.Address.City = address.City;
						//to.Address.PostalCode = address.PostalCode;
						//to.Address.AddressRegion = address.AddressRegion;
						//to.Address.Country = address.Country;

						//to.Address.Latitude = address.Latitude;
						//to.Address.Longitude = address.Longitude;
					}
				}
			}
			else
			{
				//do handling for old editor
				//to.Address.Address1 = fromEntity.Address1;
				//to.Address.Address2 = fromEntity.Address2;
				//to.Address.City = fromEntity.City;
				//to.Address.PostalCode = fromEntity.PostalCode;
				//to.Address.AddressRegion = fromEntity.Region;
				//to.Address.Country = fromEntity.Country;

				//to.Address.Latitude = fromEntity.Latitude ?? 0;
				//to.Address.Longitude = fromEntity.Longitude ?? 0;
			}
		}
		private static int CountCredentials(EM.Organization entity)
		{
			return Entity_AgentRelationshipManager.CredentialCount_ForCreatingOrg(entity.RowId);
		}
		#endregion

		#region org member methods
		public int OrganizationMember_Save( int orgId, int userId, int orgMemberTypeId, int createdById, ref string statusMessage )
		{
			int id = 0;
			//statusMessage = ""; //??don't want to remove existing status
			EM.Organization_Member efEntity = new EM.Organization_Member();
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					//check if exists
					efEntity = context.Organization_Member
						.SingleOrDefault( s => s.ParentOrgId == orgId && s.UserId == userId );

					//CM.OrganizationMember orgMember = OrganizationMember_Get( orgId, userId );
					if ( efEntity == null || efEntity.Id == 0 )
					{
						efEntity = new EM.Organization_Member();
						efEntity.CreatedById = createdById;
						efEntity.Created = System.DateTime.Now;

						efEntity.ParentOrgId = orgId;
						efEntity.UserId = userId;
					}
					else
					{
						
					}
					
					efEntity.OrgMemberTypeId = orgMemberTypeId;

					efEntity.LastUpdatedById = createdById;
					efEntity.LastUpdated = System.DateTime.Now;

					//determine if primary - would check if user has other
					//or should this be an input parameter
					if ( GetPrimaryOrganizationId( userId ) > 0 )
						efEntity.IsPrimaryOrganization = false;
					else
						efEntity.IsPrimaryOrganization = true;

					if ( efEntity.Id == 0 )
					{
						context.Organization_Member.Add( efEntity );
					}	

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						//statusMessage = "";
						id = efEntity.Id;
					}
					else
					{
						//?no info on error
						string msg = "Error - adding the organization member was not successful. ";
						statusMessage = string.IsNullOrWhiteSpace( statusMessage ) ? msg : statusMessage + "<br/>" + msg;
						string message = string.Format( "OrganizationManager. OrganizationMember_Save Failed", "Attempted to add an Organization member. The process appeared to not work, but was not an exception, so we have no message, or no clue. Organization: {0}, userId: {1}, createdById: {2}", orgId, userId, createdById );
						EmailManager.NotifyAdmin( "OrganizationManager. OrganizationMember_Save Failed", message );
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".OrganizationMember_Save(), Organization: {0}, userId: {1}, createdById: {2}", orgId, userId, createdById ) );
				}
			}

			return id;
		}

		public bool OrganizationMember_Delete( int orgId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			if ( orgId == 0 || userId == 0)
			{
				statusMessage = "Error - please provide a valid organization Id, and user Id";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				EM.Organization_Member efEntity = context.Organization_Member
							.SingleOrDefault( s => s.ParentOrgId == orgId && s.UserId == userId );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					context.Organization_Member.Remove( efEntity );
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


		/// <summary>
		/// Return list of members for an organization
		/// </summary>
		/// <param name="orgId"></param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public static List<CM.OrganizationMember> OrganizationMembers_ListByName( int orgId, int pageNumber = 1, int pageSize = 25 )
		{
			int pTotalRows = 0;
			List<CM.OrganizationMember> list = new List<CM.OrganizationMember>();
			CM.OrganizationMember entity = new CM.OrganizationMember();

			if ( pageSize == 0 )
				pageSize = 500;
			int skip = 0;
			if ( pageNumber > 1 )
				skip = ( pageNumber - 1 ) * pageSize;

			using ( var context = new Data.CTIEntities() )
			{
				var Query = from Results in context.Organization_Member
						.Where( s => s.ParentOrgId == orgId)
						.OrderBy( s => s.Account.LastName )
							select Results;

				pTotalRows = Query.Count();
				var results = Query.Skip( skip ).Take( pageSize )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( EM.Organization_Member item in results )
					{
						entity = new CM.OrganizationMember();
						entity.Id = item.Id;
						entity.ParentOrgId = item.ParentOrgId;
						entity.UserId = item.UserId;
						entity.OrgMemberTypeId = item.OrgMemberTypeId;
						entity.IsPrimaryOrganization = (bool) (item.IsPrimaryOrganization ?? false);

						entity.Created = item.Created;
						entity.CreatedById = item.CreatedById ?? 0;
						entity.LastUpdated = item.LastUpdated;
						entity.LastUpdatedById = item.LastUpdatedById ?? 0;

						//Hmm no reason to map the org, as most likely in context of org
						//OR ensure minimum
						ToMap(item.Organization, entity.Organization, false, false, false);
						AccountManager.ToMap( item.Account, entity.Account );
						
						list.Add( entity );
					}

				}
			}

			return list;
		}

		/// <summary>
		/// Return true if user is a member of any organization
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public static bool IsMemberOfAnyOrganization( int userId )
		{

			using ( var context = new Data.CTIEntities() )
			{
				List<EM.Organization_Member> list = context.Organization_Member
						.Where( s => s.UserId == userId ).ToList();

				if ( list != null && list.Count() > 0 )
					return true;
				else
					return false;
			}
		}

		/// <summary>
		/// Get the orgId of the primary org for a user
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public static int GetPrimaryOrganizationId( int userId )
		{
			Organization org = GetPrimaryOrganization( userId );
			if ( org != null && org.Id > 0 )
				return org.Id;
			else
				return 0;
		}
		/// <summary>
		/// Get the primary organization for a user
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public static Organization GetPrimaryOrganization( int userId )
		{
			Organization org = new Organization();
			using ( var context = new Data.CTIEntities() )
			{
				//to do include isprimaryOrg in list - don't filter upon the latter until process is fully implemented. 
				//otherwise just take first one
				List<EM.Organization_Member> list = context.Organization_Member
						.Where( s => s.UserId == userId )
						.OrderBy( s => s.Organization.Name)
						.ToList();

				if ( list != null && list.Count() > 0 )
				{
					int cntr = 0;
					foreach (EM.Organization_Member item in list) 
					{
						cntr++;
						if ( ( bool ) ( item.IsPrimaryOrganization ?? false )
							|| list.Count == cntr )
						{
							ToMap( item.Organization, org, false, false, false );
							break;
						}
					}
				}
					
				return org;
			}
		}
		/// <summary>
		/// Return true if user is a member of the provided organization
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public static bool IsOrganizationMember( int userId, int orgId )
		{

			using ( var context = new Data.CTIEntities() )
			{
				var orgMember = context.Organization_Member
						.SingleOrDefault( s => s.ParentOrgId == orgId && s.UserId == userId );

				if ( orgMember != null && orgMember.Id > 0 )
					return true;
				else
					return false;
			}
		}

		/// <summary>
		/// Get an org member
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public static CM.OrganizationMember OrganizationMember_Get( int orgId, int userId)
		{
			CM.OrganizationMember entity = new CM.OrganizationMember();

			using ( var context = new Data.CTIEntities() )
			{
				var orgMember = context.Organization_Member
						.SingleOrDefault( s => s.ParentOrgId == orgId && s.UserId == userId );

				if ( orgMember != null && orgMember.Id > 0 )
				{
					entity.Id = orgMember.Id;
					entity.ParentOrgId = orgMember.ParentOrgId;
					entity.UserId = orgMember.UserId;
					entity.OrgMemberTypeId = orgMember.OrgMemberTypeId;
					entity.IsPrimaryOrganization = ( bool ) ( orgMember.IsPrimaryOrganization ?? false );

					entity.Created = orgMember.Created;
					entity.CreatedById = orgMember.CreatedById ?? 0;
					entity.LastUpdated = orgMember.LastUpdated;
					entity.LastUpdatedById = orgMember.LastUpdatedById ?? 0;
				}
			}
			return entity;
		}

		#endregion

	}
}
