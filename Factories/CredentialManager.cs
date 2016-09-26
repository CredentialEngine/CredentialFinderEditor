using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

using CM = Models.Common;
using ME = Models.Elastic;
using Models.ProfileModels;
using EM = Data;
using Utilities;
//using PropertyMgr = Factories.CredentialPropertyManager;
using ThisEntity = Models.Common.Credential;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
using CondProfileMgr = Factories.Entity_ConditionProfileManager;
using CondProfileMgrOld = Factories.ConnectionProfileManager;

namespace Factories
{
	public class CredentialManager : BaseFactory
	{
		static string thisClassName = "Factories.CredentialManager";
		
		#region Credential - presistance =======================

		/// <summary>
		/// add a credential
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Credential_Add( CM.Credential entity, ref string statusMessage )
		{
			EM.Credential efEntity = new EM.Credential();
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					
					FromMap( entity, efEntity );

					efEntity.RowId = Guid.NewGuid();
					efEntity.CTID = "urn:ctid:" + efEntity.RowId.ToString();
					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdated = System.DateTime.Now;
					efEntity.StatusId = 1;

					context.Credential.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						entity.RowId = efEntity.RowId;
						statusMessage = "successful";
						entity.Id = efEntity.Id;

						UpdateParts( entity, true, ref statusMessage );

						return efEntity.Id;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the add was not successful. ";
						string message = string.Format( "CredentialManager. Credential_Add Failed", "Attempted to add a credential. The process appeared to not work, but was not an exception, so we have no message, or no clue.Credential: {0}, createdById: {1}", entity.Name, entity.CreatedById );
						EmailManager.NotifyAdmin( "CredentialManager. Credential_Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					//LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
					string message = thisClassName + string.Format( ".Credential_Add() DbEntityValidationException, Name: {0}", efEntity.Name );
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

					statusMessage = string.Join( ", ", dbex.EntityValidationErrors.SelectMany( m => m.ValidationErrors.Select( n => n.ErrorMessage ) ).ToList() );
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Credential_Add(), Name: {0}", efEntity.Name ) );
					statusMessage = ex.Message + ( ex.InnerException != null && ex.InnerException.InnerException != null ? " - " + ex.InnerException.InnerException.Message : "" );
				}
			}

			return efEntity.Id;
		}
		/// <summary>
		/// Update a credential
		/// - base only, caller will handle parts?
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Credential_Update( CM.Credential entity, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
				EM.Credential efEntity = context.Credential
								.SingleOrDefault( s => s.Id == entity.Id );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					//**ensure rowId is passed down for use by profiles, etc
					entity.RowId = efEntity.RowId;
					FromMap( entity, efEntity );

					if (context.ChangeTracker.Entries().Any(e => e.State == EntityState.Added
                                              || e.State == EntityState.Modified
											  || e.State == EntityState.Deleted ) == true)
					{
						//note: testing - the latter may be true if the child has changed - but shouldn't as the mapping only updates the parent
						efEntity.LastUpdated = System.DateTime.Now;
						efEntity.LastUpdatedById = entity.LastUpdatedById;
						if ( efEntity.StatusId < CodesManager.ENTITY_STATUS_PUBLISHED )
							efEntity.StatusId = CodesManager.ENTITY_STATUS_IN_PROGRESS;
						count = context.SaveChanges();
					}
					
					//can be zero if no data changed
					if ( count >= 0 )
					{
						
						isValid = true;

						if ( !UpdateParts( entity, false, ref statusMessage ) )
							isValid = false;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the update was not successful. ";
							string message = string.Format( "CredentialManager. Credential_Update Failed", "Attempted to update a credential. The process appeared to not work, but was not an exception, so we have no message, or no clue. Credential: {0}, Id: {1}, updatedById: {2}", entity.Name, entity.Id, entity.LastUpdatedById );
						EmailManager.NotifyAdmin( "CredentialManager. Credential_Update Failed", message );
					}
				}
				else
				{
					statusMessage = "Error - update failed, as record was not found.";
				}
			}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					//LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
					string message = thisClassName + string.Format( ".Credential_Update() DbEntityValidationException, Name: {0}", entity.Name );
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

					statusMessage = string.Join( ", ", dbex.EntityValidationErrors.SelectMany( m => m.ValidationErrors.Select( n => n.ErrorMessage ) ).ToList() );
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Credential_Update(), Name: {0}", entity.Name ) );
					statusMessage = ex.Message + ( ex.InnerException != null && ex.InnerException.InnerException != null ? " - " + ex.InnerException.InnerException.Message : "" );
				}
			}

			return isValid;
	}

		/// <summary>
		/// Update credential registry id, and set status published
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="envelopeId"></param>
		/// <param name="userId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool UpdateEnvelopeId( int credentialId, string envelopeId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			bool updatingStatus = UtilityManager.GetAppKeyValue( "onRegisterSetEntityToPublic", false );
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					EM.Credential efEntity = context.Credential
									.SingleOrDefault( s => s.Id == credentialId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						efEntity.CredentialRegistryId = envelopeId;
						if ( updatingStatus )
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
								string message = string.Format( thisClassName + ". UpdateEnvelopeId Failed", "Attempted to update an EnvelopeId. The process appeared to not work, but was not an exception, so we have no message, or no clue. Credential: {0}, envelopeId: {1}, updatedById: {2}", credentialId, envelopeId, userId );
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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UpdateEnvelopeId(), Credential: {0}, envelopeId: {1}, updatedById: {2}", credentialId, envelopeId, userId ) );
					statusMessage = ex.Message + ( ex.InnerException != null && ex.InnerException.InnerException != null ? " - " + ex.InnerException.InnerException.Message : "" );
				}
			}

			return isValid;
		}

		/// <summary>
		/// Reset credential registry id, and set status to in process
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="userId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool UnPublish( int credentialId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					EM.Credential efEntity = context.Credential
									.SingleOrDefault( s => s.Id == credentialId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						efEntity.CredentialRegistryId = null;
						//may not know reason for unpublish
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
							string message = string.Format( thisClassName + ".UnPublish Failed", "Attempted to unpublish the credential. The process appeared to not work, but was not an exception, so we have no message, or no clue. Credential: {0}, updatedById: {1}", credentialId, userId );
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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UnPublish(), Credential: {0}, updatedById: {1}", credentialId, userId ) );
					statusMessage = ex.Message + ( ex.InnerException != null && ex.InnerException.InnerException != null ? " - " + ex.InnerException.InnerException.Message : "" );
				}
			}

			return isValid;
		}
		public bool UpdateParts( CM.Credential entity, bool isAdd, ref string statusMessage )
		{
			bool isAllValid = true;
			statusMessage = "";
			int count = 0;
			List<string> messages = new List<string>();
			OrganizationRoleManager orgMgr = new OrganizationRoleManager();
			if ( UpdateProperties( entity, ref messages ) == false )
			{
				isAllValid = false;
			}
			Entity_ReferenceManager erm = new
						Entity_ReferenceManager();

			if ( erm.EntityUpdate( entity.Subjects, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_SUBJECT, false ) == false )
				isAllValid = false;

			if ( erm.EntityUpdate( entity.Keywords, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_KEYWORD, false ) == false )
				isAllValid = false;

			if ( erm.EntityUpdate( entity.OtherIndustries, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_NAICS, false ) == false )
				isAllValid = false;

			if ( erm.EntityUpdate( entity.OtherOccupations, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_SOC, false ) == false )
				isAllValid = false;
			//don't call from here, as will from a separate call
			//if (orgMgr.Credential_UpdateOrgRoles( entity, ref messages, ref count ))
			//	isAllValid = false;
			//if ( orgMgr.Credential_UpdateQAActions( entity, ref messages, ref count ) )
			//	isAllValid = false;


			//only do the following if using OLD editor
			//if ( !entity.IsNewVersion )
			//{
			//	//update owner and creator roles
				//will skip this once new interface is active
				//if ( UtilityManager.GetAppKeyValue( "usingV1Interface", true ) )
				//{	}
				//if ( orgMgr.CredentialOwnerRolesUpdate( entity, ref messages ) == false )
				//	isAllValid = false;
			

				//if ( new CredentialTimeToEarnManager().Credential_TimeToEarnUpdate( entity ) == false )
				//	isAllValid = false;

				//if ( new DurationProfileManager().DurationProfileUpdate( entity.EstimatedTimeToEarn, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages ) == false )
				//	isAllValid = false;
				//to do remove regions once done change over
				//if (new RegionsManager().GeoCoordinate_Update( entity.Region, entity.RowId, entity.LastUpdatedById, false, ref messages ) == false )
				//	isAllValid = false;

				//if ( new RegionsManager().JurisdictionProfile_Update( entity.Jurisdiction, entity.RowId,
				//	CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, RegionsManager.JURISDICTION_PURPOSE_SCOPE, ref messages ) == false )
				//	isAllValid = false;
			//}
			statusMessage = string.Join( ",", messages.ToArray() );
			return isAllValid;
		}
		public bool UpdateProperties( ThisEntity entity, ref List<string> messages )
		{
			bool isAllValid = true;
			//OLD
			//PropertyMgr cpm = new PropertyMgr();
			//if ( cpm.UpdateProperties( entity, ref messages ) == false )
			//	isAllValid = false;


			//============================
			EntityPropertyManager mgr = new EntityPropertyManager();

			if ( mgr.UpdateProperties( entity.Purpose, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_PURPOSE, entity.LastUpdatedById, ref messages ) == false )
			{
				isAllValid = false;
			}

			if ( mgr.UpdateProperties( entity.CredentialType, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE, entity.LastUpdatedById, ref messages ) == false )
			{
				isAllValid = false;
			}

			if ( mgr.UpdateProperties( entity.CredentialLevel, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_LEVEL, entity.LastUpdatedById, ref messages ) == false )
			{
				isAllValid = false;
			}
			return isAllValid;
		}



		/// <summary>
		/// Delete a credential
		/// 16-04-27 mparsons - changed to a virual delete
		/// </summary>
		/// <param name="id"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Credential_Delete( int id, int userId, ref string statusMessage )
		{
			bool isValid = false;
			if ( id == 0 )
				{
					statusMessage = "Error - missing an identifier for the Credential";
					return false;
				}
			using ( var context = new Data.CTIEntities() )
			{
				EM.Credential efEntity = context.Credential
							.SingleOrDefault( s => s.Id == id );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					statusMessage = string.Format("Credential: {0}, Id:{1}", efEntity.Name, efEntity.Id);

					//context.Credential.Remove( efEntity );
					efEntity.LastUpdated = System.DateTime.Now;
					efEntity.LastUpdatedById = userId;
					efEntity.StatusId = CodesManager.ENTITY_STATUS_DELETED;

					int count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					} else
						statusMessage = "Error - delete failed, but no message was provided.";
				}
				else
				{
					statusMessage = "Error - delete failed, as record was not found.";
				}
			}

			return isValid;
		}


		#endregion

		#region credential - retrieval ===================
		public static CM.Credential Credential_Get( int id, CredentialRequest cr )
		{
			CM.Credential entity = new CM.Credential();

			using ( var context = new Data.CTIEntities() )
			{
				EM.Credential item = context.Credential
							.SingleOrDefault( s => s.Id == id
								&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
								);

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, cr);
				}
			}

			return entity;
		}
		/// <summary>
		/// Get a credential
		/// ?should we allow get on a 'deleted' cred? Most people wouldn't remember the Id, although could be from a report
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static CM.Credential Credential_GetBasic( int id, bool includingProperties = true, bool includingProfiles = false, bool isNewVersion = true )
		{

			CM.Credential entity = new CM.Credential();
			entity.IsNewVersion = isNewVersion;
			using ( var context = new Data.CTIEntities() )
			{
				EM.Credential item = context.Credential
							.SingleOrDefault( s => s.Id == id
								&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
								);

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, includingProperties, includingProfiles );

					//Other parts
				}
			}

			return entity;
		}

		public static CM.Credential Credential_Get( int id, bool forEditView = false, bool isNewVersion = true )
		{
			CM.Credential entity = new CM.Credential();
			entity.IsNewVersion = isNewVersion;
			using ( var context = new Data.CTIEntities() )
			{
				EM.Credential item = context.Credential
							.SingleOrDefault( s => s.Id == id 
								&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED 
								);

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, true, true, forEditView );
				}
			}

			return entity;
		}
		public static CM.Credential Credential_GetByRowId( Guid uid, bool includingProperties = false, bool includingProfiles = false, bool forEditView = false )
		{
			CM.Credential entity = new CM.Credential();
			using ( var context = new Data.CTIEntities() )
			{
				EM.Credential item = context.Credential
							.SingleOrDefault( s => s.RowId == uid
								&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
								);

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, includingProperties, includingProfiles, forEditView );
				}
			}

			return entity;
		}
		/*
		[Obsolete]
		private static CM.Credential Credential_Get( int Id )
		{
			CM.Credential entity = new CM.Credential();
			using ( var context = new Data.CTIEntities() )
			{
				EM.Credential item = context.Credential
							.SingleOrDefault( s => s.Id == Id );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity );
				}
			}

			return entity;
		}
		*/

		public static List<string> Autocomplete( string pFilter, int pageNumber, int pageSize, int userId, ref int pTotalRows )
		{
			bool autocomplete = true;
			List<string> results = new List<string>();

			List<CM.CredentialSummary> list = Search( pFilter, "", pageNumber, pageSize, ref pTotalRows, userId, autocomplete );

			foreach ( CM.CredentialSummary item in list )
				results.Add( item.Name );

			return results;
		}

		/// <summary>
		/// Quick search
		/// NOTE: 16-07-06 mp - changed to use Credential_Summary instead of Credential_Summary2. Need to determine impact
		/// </summary>
		/// <param name="keyword"></param>
		/// <param name="maxTerms"></param>
		/// <returns></returns>
		public static List<CM.CredentialSummary> QuickSearch( string keyword = "", int userId= 0, int maxTerms = 0 )
		{
			List<CM.CredentialSummary> list = new List<CM.CredentialSummary>();
			CM.CredentialSummary entity = new CM.CredentialSummary();
			keyword = keyword.Trim();
			if ( maxTerms == 0 )
				maxTerms = 500;

			using ( var context = new ViewContext() )
			{
				// will only return active credentials
				List<Views.Credential_Summary> results = context.Credential_Summary
					.Where( s => keyword == "" 
						|| ( s.Name.Contains( keyword ) 
						|| (s.Description.Contains(keyword))
						|| s.OrganizationName.Contains( keyword ) )
						)
					.Take( maxTerms )
					.OrderBy( s => s.Name )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Views.Credential_Summary item in results )
					{
						entity = new CM.CredentialSummary();
						//TODO - don't need a full map for the list!
						CredentialSummary_ToMap( item, entity, false );
						list.Add( entity );
					}

					//Other parts
				}
			}

			return list;
		}
		public static List<CM.CredentialSummary> Search( string pFilter, string pOrderBy, int pageNumber, int pageSize, ref int pTotalRows, int userId = 0, bool autocomplete = false )
		{
			string connectionString = DBConnectionRO();
			CM.CredentialSummary item = new CM.CredentialSummary();
			List<CM.CredentialSummary> list = new List<CM.CredentialSummary>();
			var result = new DataTable();
			string creatorOrgs = "";
			string owningOrgs = "";
			int orgId = 0;
			string orgName = "";
			using ( SqlConnection c = new SqlConnection( connectionString ) )
			{
				c.Open();

				if ( string.IsNullOrEmpty( pFilter ) )
				{
					pFilter = "";
				}

				using ( SqlCommand command = new SqlCommand( "[Credential.Search]", c ) )
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
					item = new CM.CredentialSummary();
					item.Id = GetRowColumn( dr, "Id", 0 );
					item.Name = GetRowColumn( dr, "Name", "missing" );

					//for autocomplete, only need name
					if ( autocomplete )
					{
						list.Add( item );
						continue;
					}
					string rowId = GetRowColumn( dr, "EntityUid" );
					//if ( IsGuidValid( rowId ) )
					item.RowId = new Guid( rowId );

					
					item.Description = GetRowColumn( dr, "Description", "" );
					item.Url = GetRowColumn( dr, "Url", "" );

					item.CanEditRecord = GetRowColumn( dr, "CanEditRecord", false );

					item.ManagingOrgId = GetRowPossibleColumn( dr, "ManagingOrgId", 0 );
					item.ManagingOrganization = GetRowPossibleColumn( dr, "ManagingOrganization" );
					creatorOrgs = GetRowPossibleColumn( dr, "CreatorOrgs" );
					if ( !string.IsNullOrWhiteSpace( creatorOrgs ) )
					{
						var orgs = creatorOrgs.Split( '|' );
						foreach ( string orgSet in orgs )
						{
							//step one, just handle first one
							if (ExtractOrg( orgSet, ref orgId, ref orgName )) 
							{
								item.CreatorOrganizationId = orgId;
								item.CreatorOrganizationName = orgName;
								break;
							}
						}
					}
					owningOrgs = GetRowPossibleColumn( dr, "OwningOrgs" );
					if ( !string.IsNullOrWhiteSpace( owningOrgs ) )
					{
						var orgs = owningOrgs.Split( '|' );
						foreach ( string orgSet in orgs )
						{
							//step one, just handle first one
							if ( ExtractOrg( orgSet, ref orgId, ref orgName ) )
							{
								item.OwnerOrganizationId = orgId;
								item.OwnerOrganizationName = orgName;
								break;
							}
						}
					}
					//item.CreatorOrganizationId = GetRowColumn( dr, "OrgId", 0 );
					//item.CreatorOrganizationName = GetRowColumn( dr, "OrganizationName", "" );

					//item.OwnerOrganizationId = GetRowPossibleColumn( dr, "owingOrgId", 0 );
					//item.OwnerOrganizationName = GetRowPossibleColumn( dr, "owingOrganization" );

					item.CTID = GetRowPossibleColumn( dr, "CTID" );
					item.CredentialRegistryId = GetRowPossibleColumn( dr, "CredentialRegistryId" );
				
					string date = GetRowColumn( dr, "EffectiveDate", "" );
					if ( IsValidDate( date ) )
						item.DateEffective = ( DateTime.Parse(date ).ToShortDateString());
					else
						item.DateEffective = "";
					date = GetRowColumn( dr, "Created", "" );
					if ( IsValidDate( date ) )
						item.Created = DateTime.Parse(date );
					date = GetRowColumn( dr, "LastUpdated", "" );
					if ( IsValidDate( date ) )
						item.LastUpdated = DateTime.Parse(date );

					item.Version = GetRowPossibleColumn( dr, "Version", "" );
					item.LatestVersionUrl = GetRowPossibleColumn( dr, "LatestVersionUrl", "" );
					item.ReplacesVersionUrl = GetRowPossibleColumn( dr, "ReplacesVersionUrl", "" );
					//item.AvailableOnlineAt = GetRowPossibleColumn( dr, "AvailableOnlineAt", "" );

					item.CredentialType = GetRowPossibleColumn( dr, "CredentialType", "" );
					item.CredentialTypeSchema = GetRowPossibleColumn( dr, "CredentialTypeSchema", "" );
					//NAICS CSV
					//16-09-12 mp - changed to use pipe (|) rather than ; due to conflicts with actual embedded semicolons
					string naicsList = GetRowPossibleColumn( dr, "NaicsList", "" );
					//item.NaicsList = new List<Models.CodeItem>();
					if ( !string.IsNullOrWhiteSpace( naicsList ) )
					{
						var codeGroup = naicsList.Split( '|' );
						foreach ( string codeSet in codeGroup )
						{
							var codes = codeSet.Split( ',' );
							item.NaicsList.Add( new Models.CodeItem() { Code = codes[ 0 ].Trim(), Title = codes[ 1 ].Trim() } );
						}
					}
					//credential levels CSV
					//16-09-12 mp - changed to use pipe (|) rather than ; due to conflicts with actual embedded semicolons
					string levelsList = GetRowPossibleColumn( dr, "LevelsList", "" );
					if ( !string.IsNullOrWhiteSpace( levelsList ) )
					{
						var codeGroup = levelsList.Split( '|' );
						foreach ( string codeSet in codeGroup )
						{
							var codes = codeSet.Split( ',' );
							item.LevelsList.Add( new Models.CodeItem() { Code = codes[ 0 ].Trim(), Title = codes[ 1 ].Trim() } );
						}
					}

					item.ListTitle = item.Name + " (" + item.CreatorOrganizationName + ")";

					//addressess
					int addressess = GetRowPossibleColumn( dr, "AvailableAddresses", 0 );
					if ( addressess > 0 )
					{
						item.Addresses = AddressProfileManager.GetAll( item.RowId );
					}

					list.Add( item );
				}

				return list;

			}
		}
		private static bool ExtractOrg( string data, ref int orgId, ref string orgName )
		{
			var org = data.Split( ',' );
			orgName = org[ 1 ].Trim();
			if (Int32.TryParse(org[ 0 ].Trim(), out orgId))
				return true;
			else
				return false;
			
		
		}
		/// <summary>
		/// Fill
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="includingProperties"></param>
		/// <param name="includingProfiles"></param>
		private static void ToMap( EM.Credential from, CM.Credential to, 
					bool includingProperties = false, 
					bool includingProfiles = false, 
					bool forEditView = false)
		{
			to.Id = from.Id;
			to.StatusId = from.StatusId ?? 1;
			to.RowId = from.RowId;
			 
			to.Name = from.Name;
			to.AlternateName = from.AlternateName;

			to.Description = from.Description;
			to.ctid = from.CTID;
			to.CredentialRegistryId = from.CredentialRegistryId;

			to.Version = from.Version;
			//if ( from.EffectiveDate != null )
			//	to.DateEffective = ( DateTime ) from.EffectiveDate;
			if ( IsValidDate( from.EffectiveDate ) )
				to.DateEffective = ( ( DateTime ) from.EffectiveDate ).ToShortDateString();
			else
				to.DateEffective = "";

			to.Url = from.Url;
			to.LatestVersionUrl = from.LatestVersionUrl;
			to.ReplacesVersionUrl = from.ReplacesVersionUrl;
			to.ManagingOrgId = from.ManagingOrgId ?? 0;
			to.AvailableOnlineAt = from.AvailableOnlineAt;

			//to.JurisdictionId = from.JurisdictionId;

			if ( from.ImageUrl != null && from.ImageUrl.Trim().Length > 0 )
				to.ImageUrl = from.ImageUrl;
			else
				to.ImageUrl = null;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

			//properties
			
			// 16-06-15 mp - always include credential type
			to.CredentialType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE );

			to.Subjects = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SUBJECT );

			to.Keywords = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD );

			to.OtherIndustries = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );

			to.OtherOccupations = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SOC );
			

			if ( includingProperties )
			{
				//Credential_Property_FillCredType( from, to );
				//Credential_Property_FillPurpose( from, to );
				//Credential_Property_FillCredLevel( from, to );

				to.CredentialLevel = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_LEVEL );

				to.Purpose = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_PURPOSE );

				to.EstimatedCosts = CostProfileManager.CostProfile_GetAll( to.RowId );
			}

			if ( includingProfiles )
			{

				to.Addresses = AddressProfileManager.GetAll( to.RowId );

				//CredentialTimeToEarnManager.TimeToEarn_Get( to );
				to.EstimatedTimeToEarn = DurationProfileManager.GetAll( to.RowId );

				//CredentialFrameworkItemManager.Item_FillOccupations( to );
				//CredentialFrameworkItemManager.Item_FillNaics( to );

				to.Occupation = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_SOC );
				to.Industry = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );

				//get all condition profiles
				//ConnectionProfileManager.FillProfiles( from, to, forEditView );
				FillConditionProfiles( from, to, forEditView );

				to.Revocation = Entity_RevocationProfileManager.GetAll( to.RowId );

				//to.Region = RegionsManager.GetAll( to.RowId );
				to.Jurisdiction = RegionsManager.Jurisdiction_GetAll( to.RowId );

				to.EmbeddedCredentials = Entity_CredentialManager.GetAll( to.RowId );
			}

			//if ( to.IsNewVersion )
			//{
				if ( forEditView )
				{
					to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAllSummary( to.RowId, false );
				}
				else
				{
					//get as ennumerations
					to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAll_ToEnumeration( to.RowId, true );


				}

				to.QualityAssuranceAction = Entity_QualityAssuranceActionManager.QualityAssuranceActionProfile_GetAll( to.RowId );

		}
	
		private static void ToMap( EM.Credential from, CM.Credential to, 
					CredentialRequest cr)
		{
			to.Id = from.Id;
			to.StatusId = from.StatusId ?? 1;
			to.RowId = from.RowId;
			 
			to.Name = from.Name;
			to.AlternateName = from.AlternateName;

			to.Description = from.Description;
			to.ctid = from.CTID;
			to.CredentialRegistryId = from.CredentialRegistryId;

			to.Version = from.Version;
			if ( IsValidDate( from.EffectiveDate ) )
				to.DateEffective = ( ( DateTime ) from.EffectiveDate ).ToShortDateString();
			else
				to.DateEffective = "";

			to.Url = from.Url;
			to.LatestVersionUrl = from.LatestVersionUrl;
			to.ReplacesVersionUrl = from.ReplacesVersionUrl;
			to.ManagingOrgId = from.ManagingOrgId ?? 0;
			to.AvailableOnlineAt = from.AvailableOnlineAt;

			if ( from.ImageUrl != null && from.ImageUrl.Trim().Length > 0 )
				to.ImageUrl = from.ImageUrl;
			else
				to.ImageUrl = null;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

			//properties
			// 16-06-15 mp - always include credential type
			to.CredentialType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE );

			if ( cr.IncludingProperties)
			{
				to.CredentialLevel = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_LEVEL );

				to.Purpose = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_PURPOSE );
			}

			if (cr.IncludingRolesAndActions) {
				if ( cr.IsForEditView )
				{
					to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAllSummary( to.RowId, false );
				}
				else
				{
					//get as ennumerations
					to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAll_ToEnumeration( to.RowId, true );
				}

				to.QualityAssuranceAction = Entity_QualityAssuranceActionManager.QualityAssuranceActionProfile_GetAll( to.RowId );
			}

			if (cr.IncludingConnectionProfiles) {
				//get all condition profiles
				//TODO - have custom version of this to only get minimum!!
				//ConnectionProfileManager.FillProfiles( from, to, cr.IsForEditView );
				FillConditionProfiles( from, to, cr.IsForEditView );
			}

			if ( cr.IncludingRevocationProfiles )
				to.Revocation = Entity_RevocationProfileManager.GetAll( to.RowId );

			if (cr.IncludingEstimatedCosts) 
				to.EstimatedCosts = CostProfileManager.CostProfile_GetAll( to.RowId );
			
			if (cr.IncludingDuration) 
				to.EstimatedTimeToEarn = DurationProfileManager.GetAll( to.RowId );
			
			if (cr.IncludingAddesses) 
				to.Addresses = AddressProfileManager.GetAll( to.RowId );

			if (cr.IncludingJurisdiction)
				to.Jurisdiction = RegionsManager.Jurisdiction_GetAll( to.RowId );

			if (cr.IncludingSubjectsKeywords) 
			{
				to.Subjects = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SUBJECT );

				to.Keywords = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD );
			}

			if (cr.IncludingFrameworkItems) {
				to.Occupation = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_SOC );
				to.Industry = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );
				to.OtherIndustries = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );

				to.OtherOccupations = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SOC );
			
			}
			if (cr.IncludingEmbeddedCredentials) 
				to.EmbeddedCredentials = Entity_CredentialManager.GetAll( to.RowId );
		}
		private static void FillConditionProfiles( EM.Credential from, CM.Credential to, bool forEditView = true )
		{

			if ( UtilityManager.GetAppKeyValue("usingEntityConditionProfileForAll", false) )
			{
				//ConditionProfile entity = new ConditionProfile();
				List<ConditionProfile> list = Entity_ConditionProfileManager.GetAll( to.RowId );
				foreach ( ConditionProfile entity in list )
				{
					//entity = new ConditionProfile();
					//ToMap( item, entity, true, true, forEditView );

					if ( entity.HasCompetencies || entity.ChildHasCompetencies )
						to.ChildHasCompetencies = true;

					if ( entity.ConnectionProfileTypeId == CondProfileMgr.ConnectionProfileType_Requirement )
						to.Requires.Add( entity );
					else if ( entity.ConnectionProfileTypeId == CondProfileMgr.ConnectionProfileType_Recommendation )
						to.Recommends.Add( entity );
					else if ( entity.ConnectionProfileTypeId == CondProfileMgr.ConnectionProfileType_NextIsRequiredFor )
						to.IsRequiredFor.Add( entity );
					else if ( entity.ConnectionProfileTypeId == CondProfileMgr.ConnectionProfileType_NextIsRecommendedFor )
						to.IsRecommendedFor.Add( entity );
					else if ( entity.ConnectionProfileTypeId == CondProfileMgr.ConnectionProfileType_Renewal )
						to.Renewal.Add( entity );
					else
					{
						EmailManager.NotifyAdmin( thisClassName + ".FillConditionProfiles. Unhandled connection type", string.Format( "Unhandled connection type of {0} was encountered", entity.ConnectionProfileTypeId ) );
					}
				}
			}
			else
			{
				ConnectionProfileManager.FillProfiles( from, to, forEditView );
			}
			
		}
		private static void CredentialSummary_ToMap( Views.Credential_Summary from, CM.CredentialSummary to, bool includingProperties = false, bool includingProfiles = false )
		{
			to.Id = from.Id;

			//to.RowId = from.RowId;

			to.Name = from.Name;
			to.Description = from.Description;
			to.StatusId = from.StatusId ?? 1;

			to.CTID = from.CTID;
			to.CredentialRegistryId = from.CredentialRegistryId;

			to.CreatorOrganizationId = from.OrgId;
			to.CreatorOrganizationName = from.OrganizationName;
			to.ListTitle = to.Name + " (" + to.CreatorOrganizationName + ")";
			to.Version = from.Version;
			if ( IsValidDate( from.EffectiveDate ) )
				to.DateEffective = ( ( DateTime ) from.EffectiveDate ).ToShortDateString();
			else
				to.DateEffective = "";

			to.Url = from.Url;
			to.LatestVersionUrl = from.LatestVersionUrl;
			to.ReplacesVersionUrl = from.ReplacesVersionUrl;
			to.CredentialType = from.CredentialType;
			to.CredentialTypeSchema = from.CredentialTypeSchema;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById;


		}
	

		private static void FromMap( CM.Credential from, EM.Credential to )
		{
			to.Id = from.Id;
			if ( to.Id < 1 )
			{
				if ( IsValidDate( from.Created ) )
					to.Created = ( DateTime ) from.Created;
				to.CreatedById = ( int ) from.CreatedById;
				to.LastUpdatedById = ( int ) from.CreatedById;
			}
			//don't map rowId, or dates as not on form
			//to.RowId = from.RowId;
			//if ( IsValidDate( from.Created ) )
			//	to.Created = ( DateTime ) from.Created;
			//to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			//if ( IsValidDate( from.LastUpdated ) )
			//	to.LastUpdated = ( DateTime ) from.LastUpdated;
			//to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

			to.Name = from.Name;
			to.Description = from.Description;
			to.AlternateName = from.AlternateName;
			//to.CTID = from.ctid;

			//generally the managing orgId should not be allowed to change in the interface - yet
			if ( from.ManagingOrgId > 0
				&& from.ManagingOrgId != ( to.ManagingOrgId ?? 0 ) )
				to.ManagingOrgId = from.ManagingOrgId;

			to.Version = from.Version;
			if ( IsValidDate( from.DateEffective ) )
				to.EffectiveDate = DateTime.Parse(from.DateEffective);
			else //handle reset
				to.EffectiveDate = null;

			to.Url = from.Url;
			to.LatestVersionUrl = from.LatestVersionUrl;
			to.ReplacesVersionUrl = from.ReplacesVersionUrl;
			to.AvailableOnlineAt = from.AvailableOnlineAt;

			//????
			//to.CreatorOrganizationId = from.CreatorOrganizationId;
			//to.OwnerOrganizationId = from.OwnerOrganizationId;

			if ( from.ImageUrl != null && from.ImageUrl.Trim().Length > 0 )
				to.ImageUrl = from.ImageUrl;
			else
				to.ImageUrl = null;


		}

		#endregion

		#region Elastic search methods
		/// <summary>
		/// Get organizations as Elastic format
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="keyword"></param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSize"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
		public static List<ME.Credential> GetAllForElastic( string keyword, int pageNumber, int pageSize, ref int pTotalRows )
		{
			List<ME.Credential> list = new List<ME.Credential>();
			ME.Credential entity = new ME.Credential();
			//keyword = string.IsNullOrWhiteSpace(keyword) ? "" : keyword.Trim();
			if ( pageSize == 0 )
				pageSize = 500;
			int skip = 0;
			if ( pageNumber > 1 )
				skip = ( pageNumber - 1 ) * pageSize;

			using ( var context = new Data.CTIEntities() )
			{
				var Query = from Results in context.Credential
						.Where( s => s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED )
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
					foreach ( EM.Credential item in results )
					{
						entity = new ME.Credential();
						ToMap( item, entity );
						list.Add( entity );
					}
				}
			}

			return list;
		}
		/// <summary>
		/// Get organization as Elastic format
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static ME.Credential Organization_GetForElastic( int id )
		{

			ME.Credential entity = new ME.Credential();

			using ( var context = new Data.CTIEntities() )
			{
				EM.Credential item = context.Credential
						.SingleOrDefault( s => s.Id == id && s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity );
				}
			}

			return entity;
		}

		public static void ToMap( EM.Credential from, ME.Credential to )
		{
			to.Id = from.Id;
			to.StatusId = from.StatusId ?? 1;
			to.RowId = from.RowId;

			to.Name = from.Name;
			to.AlternateName = from.AlternateName;

			to.Description = from.Description;
			to.CTID = from.CTID;
			to.CredentialRegistryId = from.CredentialRegistryId;

			to.Version = from.Version;
			
			if ( IsValidDate( from.EffectiveDate ) )
				to.DateEffective = ( ( DateTime ) from.EffectiveDate );

			to.Url = from.Url;
			to.LatestVersionUrl = from.LatestVersionUrl;
			to.ReplacesVersionUrl = from.ReplacesVersionUrl;
			to.ManagingOrgId = from.ManagingOrgId ?? 0;
			to.AvailableOnlineAt = from.AvailableOnlineAt;

			if ( from.ImageUrl != null && from.ImageUrl.Trim().Length > 0 )
				to.ImageUrl = from.ImageUrl;
			else
				to.ImageUrl = null;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;

			List<TextValueProfile> tvps = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD );
			if ( tvps != null )
			{
				foreach ( TextValueProfile item in tvps )
				{
					to.Keywords.Add( item.TextValue );
				}
			}
			//
			tvps = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SUBJECT );
			if ( tvps != null )
			{
				foreach ( TextValueProfile item in tvps )
				{
					to.Subjects.Add( item.TextValue );
				}
			}
			//properties
			to.CredentialType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE );
			to.OtherIndustries = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );

			to.OtherOccupations = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SOC );
			to.CredentialLevel = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_LEVEL );

			to.Purpose = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_PURPOSE );

			to.EstimatedCosts = CostProfileManager.CostProfile_GetAll( to.RowId );

			to.Addresses = AddressProfileManager.GetAll( to.RowId );
			to.EstimatedTimeToEarn = DurationProfileManager.GetAll( to.RowId );

			to.Occupation = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_SOC );
			to.Industry = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );

			to.Jurisdiction = RegionsManager.Jurisdiction_GetAll( to.RowId );

			List<CM.Credential> isPart = Entity_CredentialManager.GetAll( to.RowId );
			if ( isPart != null && isPart.Count > 0 )
			{
				ME.Credential credPart = new ME.Credential();
				foreach ( CM.Credential item in isPart )
				{
					 credPart = new ME.Credential() {Id = item.Id, Name = item.Name, Description = item.Description, Url = item.Url};
					 to.EmbeddedCredentials.Add( credPart );
				}
			}

			//get all condition profiles
			ElasticFillRequirementProfiles( from, to );
				

		}
		public static void ElasticFillRequirementProfiles( EM.Credential fromEntity, ME.Credential to )
		{
			//only want requires
			ConditionProfile entity = new ConditionProfile();
			List<EM.Credential_ConnectionProfile> results = new List<EM.Credential_ConnectionProfile>();
			using ( var context = new Data.CTIEntities() )
			{
				//??NOTE - the Credential will contain Credential_ConnectionProfile 
				if ( fromEntity.Credential_ConnectionProfile != null && fromEntity.Credential_ConnectionProfile.Count > 0 )
				{
					//results = fromEntity.Credential_ConnectionProfile.ToList();
					//may want to do some ordering

					results = fromEntity.Credential_ConnectionProfile
							.Where( s => s.ConnectionTypeId == CondProfileMgr.ConnectionProfileType_Requirement)
							.OrderBy( s => s.CredentialId ).ThenBy( s => s.ConnectionTypeId ).ThenBy( s => s.Created ).ToList();
								//select Row;
				}

				//results = context.Credential_ConnectionProfile
				//		.Where( s => s.CredentialId == fromEntity.Id )
				//		.OrderBy( s => s.CredentialId ).ThenBy( s => s.ConnectionTypeId ).ThenBy( s => s.Created )
				//		.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( EM.Credential_ConnectionProfile item in results )
					{
						//only process requires profile for now
						if ( item.ConnectionTypeId != CondProfileMgr.ConnectionProfileType_Requirement )
							continue;

						entity = new ConditionProfile();
						CondProfileMgrOld.ToMap( item, entity, true, true );

						if ( entity.HasCompetencies || entity.ChildHasCompetencies )
							to.ChildHasCompetencies = true;

						to.Requires.Add( entity );
						
					}
				}
			}
		}//
		
		#endregion

	}
	public class CredentialRequest
	{
		public CredentialRequest()
		{
		}
		public void DoCompleteFill() {
			IncludingProperties = true;
		}
		public void IsCompareRequest() {
			IncludingProperties = true;
			IncludingEstimatedCosts = true;
			IncludingDuration = true;
			IncludingFrameworkItems = true;
			IncludingRolesAndActions = true;

			//add all conditions profiles for now - to get all costs
			IncludingConnectionProfiles = true;
		}
		public void IsEditRequest() {
			IsForEditView = true;
			IncludingProperties = true;

			//need handle only needing ProfileLink equivalent views for most
		}
       // public int CredentialId { get; set; }

        public bool IsForEditView { get; set; }

        public bool AllowCaching { get; set; }

        public bool IncludingProperties { get; set; }
		
        public bool IncludingRolesAndActions { get; set; }
		public bool IncludingConnectionProfiles { get; set; }
		public bool IncludingRevocationProfiles { get; set; }
		public bool IncludingEstimatedCosts{ get; set; }
		public bool IncludingDuration{ get; set; }
		public bool IncludingAddesses { get; set; }
		public bool IncludingJurisdiction { get; set; }

        public bool IncludingSubjectsKeywords{ get; set; }

        //public bool IncludingKeywords{ get; set; }
		//both occupations and industries, and others for latter
		public bool IncludingFrameworkItems{ get; set; }

		public bool IncludingEmbeddedCredentials { get; set; }
	}
		

}
