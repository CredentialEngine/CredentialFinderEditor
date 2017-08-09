using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using Models;
using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBentity = Data.LearningOpportunity;
using ThisEntity = Models.ProfileModels.LearningOpportunityProfile;
using CondProfileMgr = Factories.Entity_ConditionProfileManager;
//using CondProfileMgrOld = Factories.ConnectionProfileManager;

namespace Factories
{
	public class LearningOpportunityManager : BaseFactory
	{
		static string thisClassName = "LearningOpportunityManager";
		List<string> messages = new List<string>();

		#region LearningOpportunity - persistance ==================

		/// <summary>
		/// add a LearningOpportunity
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Add( ThisEntity entity, int userId, ref string statusMessage )
		{
			DBentity efEntity = new DBentity();
			bool isEmpty = false;
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					if ( ValidateProfile( entity, ref isEmpty, ref  messages ) == false )
					{
						statusMessage = string.Join( "<br/>", messages.ToArray() );
						return 0;
					}
					FromMap( entity, efEntity );

					efEntity.StatusId = 1;
					efEntity.RowId = Guid.NewGuid();
					efEntity.CTID = "ce-" + efEntity.RowId.ToString();
					efEntity.CreatedById = efEntity.LastUpdatedById = userId;
					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdated = System.DateTime.Now;

					context.LearningOpportunity.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";
						entity.Id = efEntity.Id;
						entity.RowId = efEntity.RowId;
				
						if ( UpdateParts( entity, userId, ref messages ) == false )
						{
							statusMessage += string.Join( ", ", messages.ToArray() );
						}

						return entity.Id;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the add was not successful. ";
						string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a Learning Opportunity. The process appeared to not work, but was not an exception, so we have no message, or no clue. LearningOpportunity: {0}, createdById: {1}", entity.Name, entity.CreatedById );
						EmailManager.NotifyAdmin( thisClassName + ".Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					
					string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", entity.ProfileName );
					statusMessage = "Error - the add was not successful. " + message;
					
				}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex );
					statusMessage = "Error - the add was not successful. " + message;
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Name: {0} \r\n", entity.Name ) + message );
				}
			}

			return entity.Id;
		}
		/// <summary>
		/// Update a LearningOpportunity
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Update( ThisEntity entity, int userId, ref string statusMessage )
		{
			bool isValid = true;
			int count = 0;
			bool isEmpty = false;
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					context.Configuration.LazyLoadingEnabled = false;
					DBentity efEntity = context.LearningOpportunity
								.SingleOrDefault( s => s.Id == entity.Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						if ( ValidateProfile( entity, ref isEmpty, ref  messages ) == false )
						{
							statusMessage = string.Join( "<br/>", messages.ToArray() );
							return false;
						}

						//for updates, chances are some fields will not be in interface, don't map these (ie created stuff)
						//fill in fields that may not be in entity
						entity.RowId = efEntity.RowId;

						FromMap( entity, efEntity );
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
								string message = thisClassName + string.Format( ". Update Failed", "Attempted to update a LearningOpportunity. The process appeared to not work, but was not an exception, so we have no message, or no clue. LearningOpportunity: {0}, Id: {1}, updatedById: {2}", entity.Name, entity.Id, entity.LastUpdatedById );
								EmailManager.NotifyAdmin( thisClassName + ". Update Failed", message );
							}
						}
						//continue with parts regardless

						if ( UpdateParts( entity, userId, ref messages ) == false )
						{
							isValid = false;
							statusMessage += string.Join( "<br/>", messages.ToArray() );
						}

					}
					else
					{
						statusMessage = "Error - update failed, as record was not found.";
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".Update. id: {0}", entity.Id ) );
				statusMessage = ex.Message;
				isValid = false;
			}


			return isValid;
		}

		/// <summary>
		/// Update credential registry id, and set status published
		/// </summary>
		/// <param name="learningOppId"></param>
		/// <param name="envelopeId"></param>
		/// <param name="userId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool UpdateEnvelopeId( int learningOppId, string envelopeId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			bool updatingStatus = UtilityManager.GetAppKeyValue( "onRegisterSetEntityToPublic", false );
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					EM.LearningOpportunity efEntity = context.LearningOpportunity
									.SingleOrDefault( s => s.Id == learningOppId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						efEntity.CredentialRegistryId = envelopeId;
						if ( updatingStatus )
							efEntity.StatusId = CodesManager.ENTITY_STATUS_PUBLISHED;

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
								string message = string.Format( thisClassName + ". UpdateEnvelopeId Failed", "Attempted to update an EnvelopeId. The process appeared to not work, but was not an exception, so we have no message, or no clue. LearningOpportunity: {0}, envelopeId: {1}, updatedById: {2}", learningOppId, envelopeId, userId );
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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UpdateEnvelopeId(), LearningOpportunity: {0}, envelopeId: {1}, updatedById: {2}", learningOppId, envelopeId, userId ) );
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
		public bool UnPublish( int learningOppId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			EM.LearningOpportunity efEntity = new DBentity();
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					efEntity = context.LearningOpportunity
									.SingleOrDefault( s => s.Id == learningOppId );

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
							string message = string.Format( thisClassName + ".UnPublish Failed", "Attempted to unpublish the LearningOpportunity. The process appeared to not work, but was not an exception, so we have no message, or no clue. LearningOpportunityId: {0}, updatedById: {1}", learningOppId, userId );
							EmailManager.NotifyAdmin( thisClassName + ".UnPublish Failed", message );
						}
					}
					else
					{
						statusMessage = "Error - update failed, as record was not found.";
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{

					string message = HandleDBValidationError( dbex, thisClassName + ".)() ", efEntity.Name );
					statusMessage = "Error - the unpublish was not successful. " + message;

				}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex );
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UnPublish(), LearningOpportunityId: {0}, updatedById: {1} \r\n", learningOppId, userId ) + message );
				}
			}

			return isValid;
		}
		/// <summary>
		/// Delete a Learning Opportunity, and related Entity
		/// </summary>
		/// <param name="Id"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( int Id, ref string statusMessage )
		{
			bool isValid = false;
			if ( Id == 0 )
			{
				statusMessage = "Error - missing an identifier for the LearningOpportunity";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					DBentity efEntity = context.LearningOpportunity
								.SingleOrDefault( s => s.Id == Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						Guid rowId = efEntity.RowId;

						context.LearningOpportunity.Remove( efEntity );
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
					LoggingHelper.LogError( ex, thisClassName + ".Delete()" );

					if ( ex.InnerException != null && ex.InnerException.Message != null )
					{
						statusMessage = ex.InnerException.Message;

						if ( ex.InnerException.InnerException != null && ex.InnerException.InnerException.Message != null )
							statusMessage = ex.InnerException.InnerException.Message;
					}
					if ( statusMessage.ToLower().IndexOf( "the delete statement conflicted with the reference constraint" ) > -1 )
					{
						statusMessage = "Error: this learning opportunity cannot be deleted as it is being referenced by other items, such as roles or credentials. These associations must be removed before this learning opportunity can be deleted.";
					}
				}
			}

			return isValid;
		}

		#region LearningOpportunity properties ===================
		public bool UpdateParts( ThisEntity entity, int userId, ref List<string> messages )
		{
			bool isAllValid = true;
		
			if ( UpdateProperties( entity, userId, ref messages ) == false )
			{
				isAllValid = false;
			}
			Entity_ReferenceManager erm = new 
				Entity_ReferenceManager();
			EntityPropertyManager mgr = new EntityPropertyManager();


			if ( entity.OwnerRoles == null || entity.OwnerRoles.Items.Count == 0 )
			{
				messages.Add( "Invalid request, please select one or more roles for the owing agent." );
				isAllValid = false;
			}
			else
			{

				if ( entity.OwnerRoles.GetFirstItemId() != Entity_AgentRelationshipManager.ROLE_TYPE_OWNER )
				{
					messages.Add( "Invalid request. The role \"Owned By\" must be one of the roles selected." );
					isAllValid = false;
				}
				else
				{
					OrganizationRoleProfile profile = new OrganizationRoleProfile();
					profile.ParentUid = entity.RowId;
					profile.ActingAgentUid = entity.OwningAgentUid;
					profile.AgentRole = entity.OwnerRoles;
					profile.CreatedById = entity.LastUpdatedById;
					profile.LastUpdatedById = entity.LastUpdatedById;

					if ( !new Entity_AgentRelationshipManager().Agent_EntityRoles_Save( profile, Entity_AgentRelationshipManager.VALID_ROLES_OWNER, entity.LastUpdatedById, ref messages ) )
						isAllValid = false;
				}
			}

			if ( erm.Entity_Reference_Update( entity.OtherInstructionalProgramCategory, entity.RowId, CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_CIP, false ) == false )
				isAllValid = false;


			if ( erm.Entity_Reference_Update( entity.Subject, entity.RowId, CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, userId, ref messages, CodesManager.PROPERTY_CATEGORY_SUBJECT, false ) == false )
				isAllValid = false;

			if ( erm.Entity_Reference_Update( entity.Keyword, entity.RowId, CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, userId, ref messages, CodesManager.PROPERTY_CATEGORY_KEYWORD, false ) == false )
				isAllValid = false;
			
			return isAllValid;
		} //

		public bool UpdateProperties( ThisEntity entity, int userId, ref List<string> messages )
		{
			bool isAllValid = true;
			string statusMessage = "";

			EntityPropertyManager mgr = new EntityPropertyManager();
			if ( entity.IsNewVersion )
			{	}


				if ( mgr.UpdateProperties( entity.LearningMethodType, entity.RowId, CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, CodesManager.PROPERTY_CATEGORY_Learning_Method_Type, userId, ref messages ) == false )
					isAllValid = false;
		

			if ( mgr.UpdateProperties( entity.DeliveryType, entity.RowId, CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, CodesManager.PROPERTY_CATEGORY_LEARNING_OPP_DELIVERY_TYPE, userId, ref messages ) == false )
				isAllValid = false;
			return isAllValid;
		}


		#endregion

		public bool ValidateProfile( ThisEntity profile, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;
			int count = messages.Count;

			isEmpty = false;
			//check if empty
			if ( string.IsNullOrWhiteSpace( profile.Name )
				&& string.IsNullOrWhiteSpace( profile.Description )
				&& string.IsNullOrWhiteSpace( profile.AvailableOnlineAt )
				&& string.IsNullOrWhiteSpace( profile.DateEffective )
				&& ( profile.Jurisdiction == null || profile.Jurisdiction.Count == 0 )
				&& ( profile.EstimatedDuration == null || profile.EstimatedDuration.Count == 0 )
				)
			{
				//isEmpty = true;
				//return isValid;
			}
					//&& ( profile.EstimatedCost == null || profile.EstimatedCost.Count == 0 )
			if ( string.IsNullOrWhiteSpace( profile.Name ) )
			{
				messages.Add( "A Learning Opportunity name must be entered" );
			}
			if ( string.IsNullOrWhiteSpace( profile.Description ) )
			{
				messages.Add( "A Learning Opportunity Description must be entered" );
			}
			if ( !IsValidGuid( profile.OwningAgentUid ) )
			{
				messages.Add( "An owning organization must be selected" );
			}

			if ( !string.IsNullOrWhiteSpace( profile.DateEffective ) && !IsValidDate( profile.DateEffective ) )
			{
				messages.Add( "Please enter a valid effective date" );
			}

			if ( string.IsNullOrWhiteSpace( profile.SubjectWebpage ) )
				messages.Add( "A Subject Webpage name must be entered" );

			else if ( !IsUrlValid( profile.SubjectWebpage, ref commonStatusMessage ) )
			{
				messages.Add( "The Subject Webpage Url is invalid" + commonStatusMessage );
			}

			if ( !IsUrlValid( profile.AvailableOnlineAt, ref commonStatusMessage ) )
			{
				messages.Add( "The Available Online At Url is invalid" + commonStatusMessage );
			}
			
			if ( !IsUrlValid( profile.AvailabilityListing, ref commonStatusMessage ) )
			{
				messages.Add( "The Availability Listing Url is invalid" + commonStatusMessage );
			}

			if ( profile.CreditHourValue < 0 || profile.CreditHourValue > 100 )
				messages.Add( "Error: invalid value for Credit Hour Value. Must be a reasonable decimal value greater than zero." );

			if ( profile.CreditUnitValue < 0 || profile.CreditUnitValue > 100 )
				messages.Add( "Error: invalid value for Credit Unit Value. Must be a reasonable decimal value greater than zero." );


			//can only have credit hours properties, or credit unit properties, not both
			bool hasCreditHourData = false;
			bool hasCreditUnitData = false;
			if ( profile.CreditHourValue > 0 || ( profile.CreditHourType ?? "" ).Length > 0 )
				hasCreditHourData = true;
			if ( profile.CreditUnitTypeId > 0
				|| ( profile.CreditUnitTypeDescription ?? "" ).Length > 0
				|| profile.CreditUnitValue > 0 )
				hasCreditUnitData = true;

			if ( hasCreditHourData && hasCreditUnitData )
				messages.Add( "Error: Data can be entered for Credit Hour related properties or Credit Unit related properties, but not for both." );

			if ( messages.Count > count )
				isValid = false;

			return isValid;
		}

		#endregion

		#region == Retrieval =======================
		public static ThisEntity GetForDetail( int id)
		{
			ThisEntity entity = new ThisEntity();
			bool includingProfiles = true;

			using ( var context = new Data.CTIEntities() )
			{
				//context.Configuration.LazyLoadingEnabled = false;
				DBentity item = context.LearningOpportunity
						.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity,
						true, //includingProperties
						includingProfiles,
						false, //forEditView
						true);
				}
			}

			return entity;
		}
		public static ThisEntity Get( int id, 
			bool forEditView = false, 
			bool includeWhereUsed = false)
		{
			ThisEntity entity = new ThisEntity();
			bool includingProfiles = false;
			if ( forEditView )
				includingProfiles = true;

			using ( var context = new Data.CTIEntities() )
			{
				//TODO - make configurable
				//CURRENTLY, DON'T NEED THE RELATED CHILD TABLES 
				//	161003 MP - actually do need costs
				//if ( forEditView )
				//	context.Configuration.LazyLoadingEnabled = false;


				DBentity item = context.LearningOpportunity
						.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity,
						true, //includingProperties
						includingProfiles, 
						forEditView,  
						includeWhereUsed);
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
				context.Configuration.LazyLoadingEnabled = false;
				DBentity item = context.LearningOpportunity
						.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					entity.Id = item.Id;
					entity.RowId = item.RowId;
					entity.Name = item.Name;
					entity.Description = item.Description;
					entity.SubjectWebpage = item.Url;
					entity.ctid = item.CTID;
					if ( IsGuidValid( item.OwningAgentUid ) )
					{
						entity.OwningAgentUid = ( Guid ) item.OwningAgentUid;

						entity.OwningOrganization = OrganizationManager.GetForSummary( entity.OwningAgentUid );
						
					}
				}
			}

			return entity;
		}
		public static ThisEntity GetAs_IsPartOf( Guid rowId, bool forEditView )
		{
			ThisEntity entity = new ThisEntity();

			using ( var context = new Data.CTIEntities() )
			{
				//	REVIEW	- seems like will need to almost always bubble up costs
				//			- just confirm that this method is to simply list parent Lopps
				context.Configuration.LazyLoadingEnabled = false;

				DBentity item = context.LearningOpportunity
						.SingleOrDefault( s => s.RowId == rowId );

				if ( item != null && item.Id > 0 )
				{
					entity.Id = item.Id;
					entity.RowId = item.RowId;
					entity.Name = item.Name;
					entity.Description = item.Description;
					entity.SubjectWebpage = item.Url;
					entity.ctid = item.CTID;
					if ( IsGuidValid( item.OwningAgentUid ) )
					{
						entity.OwningAgentUid = ( Guid ) item.OwningAgentUid;
					}
					//costs? = shouldn't need
					entity.EstimatedCost = CostProfileManager.GetAll( entity.RowId, forEditView );
				}
			}

			return entity;
		}
	
		public static List<string> Autocomplete( string pFilter, int pageNumber, int pageSize, int userId, ref int pTotalRows )
		{
			bool autocomplete = true;
			List<string> results = new List<string>();
			List<string> competencyList = new List<string>();
			//get minimal entity
			List<ThisEntity> list = Search( pFilter, "", pageNumber, pageSize, userId, ref pTotalRows, ref competencyList, autocomplete );

			string prevName = "";
			foreach ( LearningOpportunityProfile item in list )
			{
				//note excluding duplicates may have an impact on selected max terms
				if ( item.Name.ToLower() != prevName )
					results.Add( item.Name );

				prevName = item.Name.ToLower();
			}
			return results;
		}
		public static List<ThisEntity> Search( string pFilter, string pOrderBy, int pageNumber, int pageSize, int userId, ref int pTotalRows )
		{
			List<string> competencyList = new List<string>();
			return Search( pFilter, pOrderBy, pageNumber, pageSize, userId, ref pTotalRows, ref competencyList );
		}
		public static List<ThisEntity> Search( string pFilter, string pOrderBy, int pageNumber, int pageSize, int userId, ref int pTotalRows, ref List<string> competencyList, bool autocomplete = false )
		{
			string connectionString = DBConnectionRO();
			ThisEntity item = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			var result = new DataTable();
			string temp = "";
			string org = "";
			int orgId = 0;

			using ( SqlConnection c = new SqlConnection( connectionString ) )
			{
				c.Open();

				if ( string.IsNullOrEmpty( pFilter ) )
				{
					pFilter = "";
				}

				using ( SqlCommand command = new SqlCommand( "[LearningOpportunity_Search]", c ) )
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
					item = new ThisEntity();
					item.Id = GetRowColumn( dr, "Id", 0 );
					item.Name = GetRowColumn( dr, "Name", "missing" );
					item.FriendlyName = FormatFriendlyTitle( item.Name );

					//for autocomplete, only need name
					if ( autocomplete )
					{
						list.Add( item );
						continue;
					}
					item.Description = GetRowColumn( dr, "Description", "" );

					string rowId = GetRowColumn( dr, "RowId" );
					item.RowId = new Guid( rowId );

					item.SubjectWebpage = GetRowColumn( dr, "URL", "" );
					item.AvailableOnlineAt = GetRowPossibleColumn( dr, "AvailableOnlineAt", "" );
					item.CanEditRecord = GetRowColumn( dr, "CanEditRecord", false );
					item.StatusId = GetRowColumn( dr, "StatusId", 1 );
					item.ctid = GetRowPossibleColumn( dr, "CTID", "" );
					item.CredentialRegistryId = GetRowPossibleColumn( dr, "CredentialRegistryId", "" );

					item.CodedNotation = GetRowColumn( dr, "IdentificationCode", "" );
					item.ManagingOrganization = GetRowPossibleColumn( dr, "ManagingOrganization", "" );
					item.ManagingOrgId = GetRowPossibleColumn( dr, "ManagingOrgId", 0 );

					org = GetRowPossibleColumn( dr, "Organization", "" );
					orgId = GetRowPossibleColumn( dr, "OrgId", 0 );
					if ( orgId > 0 )
					{
						item.OwningOrganization = new Organization() { Id = orgId, Name = org };
					}

					//item.CreatedByOrganization = GetRowColumn( dr, "Organization", "" );
					//item.CreatedByOrganizationId = GetRowColumn( dr, "OrgId", 0 );

					temp = GetRowColumn( dr, "DateEffective", "" );
					if ( IsValidDate( temp ) )
						item.DateEffective = DateTime.Parse( temp ).ToShortDateString();
					else
						item.DateEffective = "";

					item.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
					item.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );

					//addressess
					int addressess = GetRowPossibleColumn( dr, "AvailableAddresses", 0 );
					if ( addressess > 0 )
					{
						item.Addresses = AddressProfileManager.GetAll( item.RowId );
					}
					//competencies. either arbitrarily get all, or if filters exist, only return matching ones
					int competencies = GetRowPossibleColumn( dr, "Competencies", 0 );
					if ( competencies > 0 )
					{
						if (competencyList.Count == 0)
							item.TeachesCompetenciesFrameworks = Entity_CompetencyFrameworkManager.GetAll( item.RowId, "teaches" );
						else
						{
							item.TeachesCompetenciesFrameworks = new List<CredentialAlignmentObjectFrameworkProfile>();
							List<CredentialAlignmentObjectFrameworkProfile> all = Entity_CompetencyFrameworkManager.GetAll( item.RowId, "teaches" );
							foreach ( CredentialAlignmentObjectFrameworkProfile next in all )
							{
								//just do desc for now
								string orig = (next.Description ?? "");
								foreach ( string filter in competencyList )
								{
									//not ideal, as would be an exact match
									orig = orig.Replace( filter, string.Format( "<span class='highlight'>{0}<\\span>", filter ) );
								}
								if ( orig != ( next.Description ?? "" ) )
								{
									next.Description = orig;
									item.TeachesCompetenciesFrameworks.Add( next );
								}
							}
						}
					}

					list.Add( item );
				}

				return list;

			}
		} //
		

		public static void FromMap( ThisEntity from, DBentity to )
		{

			//want to ensure fields from create are not wiped
			if ( to.Id == 0 )
			{
				if ( IsValidDate( from.Created ) )
					to.Created = from.Created;
				to.CreatedById = from.CreatedById;
			}
			//don't map rowId, ctid, or dates as not on form

			to.Id = from.Id;
			to.Name = GetData(from.Name);
			//to.StatusId = from.StatusId > 0 ? from.StatusId : ( to.StatusId > 0 ? to.StatusId: 1 );

			to.Description = GetData(from.Description);

			//generally the managing orgId should not be allowed to change in the interface - yet
			if ( from.ManagingOrgId > 0
				&& from.ManagingOrgId != ( to.ManagingOrgId ?? 0 ) )
				to.ManagingOrgId = from.ManagingOrgId;

			if ( IsGuidValid( from.OwningAgentUid ) )
			{
				if ( to.Id > 0 && to.OwningAgentUid != from.OwningAgentUid )
				{
					if ( IsGuidValid( to.OwningAgentUid ) )
					{
						//need to remove the owner role, or could have been others
						string statusMessage = "";
						new Entity_AgentRelationshipManager().Delete( to.RowId, to.OwningAgentUid, Entity_AgentRelationshipManager.ROLE_TYPE_OWNER, ref statusMessage );
					}
				}
				to.OwningAgentUid = from.OwningAgentUid;
			}
			else
			{
				//always have to have an owner
				//to.OwningAgentUid = null;
			}

			//OLD??
			if ( IsGuidValid( from.ProviderUid ) )
				to.ProviderUid = from.ProviderUid;
			else
				to.ProviderUid = null;

			to.Url = GetUrlData( from.SubjectWebpage, null );
			to.IdentificationCode = GetData(from.CodedNotation);
			to.AvailableOnlineAt = GetUrlData( from.AvailableOnlineAt, null );
			to.AvailabilityListing = GetUrlData( from.AvailabilityListing, null );
			to.DeliveryTypeDescription = from.DeliveryTypeDescription;
			to.VerificationMethodDescription = from.VerificationMethodDescription;

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = DateTime.Parse( from.DateEffective );
			else
				to.DateEffective = null;

			if ( from.InLanguageId > 0 )
				to.InLanguageId = from.InLanguageId;
			else
				to.InLanguageId = null;

			to.CreditHourType = GetData( from.CreditHourType );
			to.CreditHourValue = SetData( from.CreditHourValue, 0.5M );
			to.CreditUnitTypeId = SetData( from.CreditUnitTypeId, 1 );
			to.CreditUnitTypeDescription = GetData( from.CreditUnitTypeDescription );
			to.CreditUnitValue = SetData( from.CreditUnitValue, 0.5M );

			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById;
		}
		public static void ToMap( DBentity from, ThisEntity to,
				bool includingProperties = false,
				bool includingProfiles = true,
				bool forEditView = true,
				bool includeWhereUsed = true )
		{

			//TODO add a tomap basic, and handle for lists
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.StatusId = from.StatusId ?? 1;
			to.ManagingOrgId = from.ManagingOrgId ?? 0;

			if ( IsGuidValid( from.OwningAgentUid ) )
			{
				to.OwningAgentUid = ( Guid ) from.OwningAgentUid;
				to.OwningOrganization = OrganizationManager.GetForSummary( to.OwningAgentUid );

				//get roles
				OrganizationRoleProfile orp = Entity_AgentRelationshipManager.AgentEntityRole_GetAsEnumerationFromCSV( to.RowId, to.OwningAgentUid );
				to.OwnerRoles = orp.AgentRole;
			}

			to.Name = from.Name;
			to.Description = from.Description == null ? "" : from.Description;
			to.ctid = from.CTID;
			to.CredentialRegistryId = from.CredentialRegistryId;

			to.AvailabilityListing = from.AvailabilityListing;

			to.SubjectWebpage = from.Url;
			to.CodedNotation = from.IdentificationCode;
			to.AvailableOnlineAt = from.AvailableOnlineAt;
			to.DeliveryTypeDescription = from.DeliveryTypeDescription;
			to.VerificationMethodDescription = from.VerificationMethodDescription;

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = ( ( DateTime ) from.DateEffective ).ToShortDateString();
			else
				to.DateEffective = "";
			if ( from.ProviderUid != null )
				to.ProviderUid = ( Guid ) from.ProviderUid;

			if ( from.InLanguageId != null )
			{
				to.InLanguageId = ( int ) from.InLanguageId;
				to.InLanguage = from.Codes_Language.LanguageName;
				to.InLanguageCode = from.Codes_Language.LangugeCode;
			}
			else
			{
				to.InLanguageId = 0;
				to.InLanguage = "";
				to.InLanguageCode = "";
			}

			to.CreditHourType = from.CreditHourType ?? "";
			to.CreditHourValue = ( from.CreditHourValue ?? 0M );
			to.CreditUnitTypeId = ( from.CreditUnitTypeId ?? 0 );
			to.CreditUnitTypeDescription = from.CreditUnitTypeDescription;
			to.CreditUnitValue = from.CreditUnitValue ?? 0M;

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


			//to.LearningResourceUrl2 = CommaSeparatedListToStringList( from.LearningResourceUrl );

			//to.LearningResourceUrl = from.LearningResourceUrl;
			//will need a category
			//to.ResourceUrls = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS );

			//to.LearningResourceUrls = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_LEARNING_RESOURCE_URLS );

			to.Subject = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SUBJECT );

			to.Keyword = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD );


			//properties
			if ( includingProperties )
			{
				to.DeliveryType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_LEARNING_OPP_DELIVERY_TYPE );
				to.LearningMethodType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_Learning_Method_Type );

				to.Addresses = AddressProfileManager.GetAll( to.RowId );

				// Begin edits - Need these to populate Credit Unit Type -  NA 3/31/2017
				if ( to.CreditUnitTypeId > 0 )
				{
					to.CreditUnitType = new Enumeration();
					var match = CodesManager.GetEnumeration( "creditUnit" ).Items.FirstOrDefault( m => m.CodeId == to.CreditUnitTypeId );
					if ( match != null )
					{
						to.CreditUnitType.Items.Add( match );
					}
				}

				//Fix costs
				to.EstimatedCost = CostProfileManager.GetAll( to.RowId, forEditView );

				to.FinancialAssistance = Entity_FinancialAlignmentProfileManager.GetAll( to.RowId, forEditView );

				//Include currencies to fix null errors in various places (detail, compare, publishing) - NA 3/17/2017
				var currencies = CodesManager.GetCurrencies();
				//Include cost types to fix other null errors - NA 3/31/2017
				var costTypes = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_CREDENTIAL_ATTAINMENT_COST );
				foreach ( var cost in to.EstimatedCost )
				{
					cost.CurrencyTypes = currencies;

					foreach ( var costItem in cost.Items )
					{
						costItem.CostType.Items.Add( costTypes.Items.FirstOrDefault( m => m.CodeId == costItem.CostTypeId ) );
					}
				}
				//End edits - NA 3/31/2017

			}
			//get condition profiles
			List<ConditionProfile> list = new List<ConditionProfile>();
			if ( forEditView )
			{
				list = Entity_ConditionProfileManager.GetAllForLinks( to.RowId );
				to.CommonConditions = Entity_CommonConditionManager.GetAll( to.RowId, true );
				to.CommonCosts = Entity_CommonCostManager.GetAll( to.RowId, true );
			}
			else
			{
				list = Entity_ConditionProfileManager.GetAll( to.RowId );
				to.CommonConditions = Entity_CommonConditionManager.GetAll( to.RowId, false );
				to.CommonCosts = Entity_CommonCostManager.GetAll( to.RowId, false );
			}
				
			if ( list != null && list.Count > 0 )
			{
				foreach ( ConditionProfile item in list )
				{
					if ( item.ConditionSubTypeId == Entity_ConditionProfileManager.ConditionSubType_LearningOpportunity )
					{
						to.LearningOppConnections.Add( item );
					}
					else if( item.ConnectionProfileTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Requirement )
						to.Requires.Add( item );
					else if ( item.ConnectionProfileTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Recommendation )
						to.Recommends.Add( item );
					else if ( item.ConnectionProfileTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Corequisite )
						to.Corequisite.Add( item );
					else if ( item.ConnectionProfileTypeId == Entity_ConditionProfileManager.ConnectionProfileType_EntryCondition )
						to.EntryCondition.Add( item );
					else
					{
						EmailManager.NotifyAdmin( "Unexpected Condition Profile for learning opportunity", string.Format( "LearningOppId: {0}, ConditionProfileTypeId: {1}", to.Id, item.ConnectionProfileTypeId ) );

						//add to required, for dev only?
						if ( IsDevEnv() )
						{
							item.ProfileName = ( item.ProfileName ?? "" ) + " unexpected condition type of " + item.ConnectionProfileTypeId.ToString();
							to.Requires.Add( item );
						}
					}
				}
			}

			//if ( includingProfiles )
			//{
			to.InstructionalProgramCategory = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CIP );

			to.OtherInstructionalProgramCategory = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_CIP );
		
			if ( forEditView )
			{
				//just get profile links
				to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAllExceptOwnerSummary( to.RowId, to.OwningAgentUid, false, false );
				//USING OwnerRoles, not OwnerOrganizationRoles for edit
				//to.OwnerOrganizationRoles = Entity_AgentRelationshipManager.AgentEntityRole_GetOwnerSummary( to.RowId, to.OwningAgentUid, false );
			}
			else
			{
			//get as ennumerations
				to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAll_ToEnumeration( to.RowId, true );
			}

			//to.QualityAssuranceAction = Entity_QualityAssuranceActionManager.QualityAssuranceActionProfile_GetAll( to.RowId );


			to.EstimatedDuration = DurationProfileManager.GetAll( to.RowId );

			to.Jurisdiction = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId );
			to.Region = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_RESIDENT );

			to.JurisdictionAssertions = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, 3 );

			//TODO - re: forEditView, not sure about approach for learning opp parts
			//for now getting all, although may only need as links - except may also need to get competencies
			bool forProfilesList = false;
			if ( forEditView )
				forProfilesList = true;

			to.HasPart = Entity_LearningOpportunityManager.LearningOpps_GetAll( to.RowId, forEditView, forProfilesList );
			foreach ( ThisEntity e in to.HasPart )
			{
				if ( e.HasCompetencies || e.ChildHasCompetencies )
				{
					to.ChildHasCompetencies = true;
					break;
				}
			}
			//}
			ToMap_Competencies( to );

				

				//to.EmbeddedAssessment = Entity_AssessmentManager.EntityAssessments_GetAll( to.RowId, forEditView );

				//to.LearningOpportunityProcess = Entity_ProcessProfileManager.GetAll( to.RowId, true );

				//16-09-02 mp - always get for now
				//really only needed for detail view
				//===> need a means to determine request is from microsearch, so only minimal is returned!
				//if ( includeWhereUsed )
				//{
				to.WhereReferenced = new List<string>();
				if ( from.Entity_LearningOpportunity != null && from.Entity_LearningOpportunity.Count > 0 )
				{
					//the Entity_LearningOpportunity could be for a parent lopp, or a condition profile
					foreach ( EM.Entity_LearningOpportunity item in from.Entity_LearningOpportunity )
					{
						to.WhereReferenced.Add( string.Format( "EntityUid: {0}, Type: {1}", item.Entity.EntityUid, item.Entity.Codes_EntityType.Title ) );
						if ( item.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE )
						{
							to.IsPartOf.Add( GetAs_IsPartOf( item.Entity.EntityUid, forEditView ) );
						}
						else if ( item.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_CONNECTION_PROFILE )
						{
							ConditionProfile cp = CondProfileMgr.GetAs_IsPartOf( item.Entity.EntityUid );
							to.IsPartOfConditionProfile.Add( cp );
							//need to check cond prof for parent of credential
							//will need to ensure no dups, or realistically, don't do the direct credential check
							if ( cp.ParentCredential != null && cp.ParentCredential.Id > 0 )
							{
								//to.IsPartOfCredential.Add( cp.ParentCredential );
								AddCredentialReference( cp.ParentCredential.Id, to );
							}
					}
					else if ( item.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_CREDENTIAL )
					{
						//to.IsPartOfCredential.Add( CredentialManager.GetBasic( item.Entity.EntityUid, false ) );
						AddCredentialReference( (int)item.Entity.EntityBaseId, to );
					}
				}
				}
				//}

		
		}

		public static void ToMap_Competencies( ThisEntity to )
		{
			to.TeachesCompetenciesFrameworks = Entity_CompetencyFrameworkManager.GetAll( to.RowId, "teaches" );
			if ( to.TeachesCompetenciesFrameworks.Count > 0 )
				to.HasCompetencies = true;
			to.RequiresCompetenciesFrameworks = Entity_CompetencyFrameworkManager.GetAll( to.RowId, "requires" );
			if ( to.RequiresCompetenciesFrameworks.Count > 0 )
				to.HasCompetencies = true;
		}
		private static void AddCredentialReference( int credentialId, ThisEntity to )
		{
			Credential exists = to.IsPartOfCredential.SingleOrDefault( s => s.Id == credentialId );
			if ( exists == null || exists.Id == 0 )
				to.IsPartOfCredential.Add( CredentialManager.GetBasic( ( int ) credentialId ) );
		}
		#endregion
	}
}
