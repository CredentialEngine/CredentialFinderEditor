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
using DBentity = Data.Assessment;
using ThisEntity = Models.ProfileModels.AssessmentProfile;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
using CondProfileMgr = Factories.Entity_ConditionProfileManager;
//using CondProfileMgrOld = Factories.ConnectionProfileManager;
namespace Factories
{
	public class AssessmentManager : BaseFactory
	{
		static string thisClassName = "AssessmentManager";
		List<string> messages = new List<string>();

		#region Assessment - persistance ==================

		/// <summary>
		/// add a Assessment
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Add( ThisEntity entity, ref string statusMessage )
		{
			DBentity efEntity = new DBentity();
			//AssessmentPropertyManager opMgr = new AssessmentPropertyManager();
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					if ( ValidateProfile( entity, ref  messages ) == false )
					{
						statusMessage = string.Join( "<br/>", messages.ToArray() );
						return 0;
					}

					FromMap( entity, efEntity );

					efEntity.StatusId = 1;
					efEntity.RowId = Guid.NewGuid();
					efEntity.CTID = "ce-" + efEntity.RowId.ToString();
					efEntity.CreatedById = entity.CreatedById;
					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdatedById = entity.CreatedById;
					efEntity.LastUpdated = System.DateTime.Now;

					context.Assessment.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";
						entity.Id = efEntity.Id;
						entity.RowId = efEntity.RowId;

						if ( UpdateParts( entity, ref messages ) == false )
						{
							statusMessage += string.Join( ", ", messages.ToArray() );
						}

						return efEntity.Id;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the add was not successful. ";
						string message = string.Format( "AssessmentManager. Assessment_Add Failed", "Attempted to add a Assessment. The process appeared to not work, but was not an exception, so we have no message, or no clue. Assessment: {0}, createdById: {1}", entity.Name, entity.CreatedById );
						EmailManager.NotifyAdmin( "AssessmentManager. Assessment_Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					//LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
					string message = thisClassName + string.Format( ".Assessment_Add() DbEntityValidationException, Name: {0}", efEntity.Name );
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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Assessment_Add(), Name: {0}", efEntity.Name ) );
				}
			}

			return efEntity.Id;
		}
		/// <summary>
		/// Update a Assessment
		/// - base only, caller will handle parts?
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Update( ThisEntity entity, ref string statusMessage )
		{
			bool isValid = true;
			int count = 0;
			//AssessmentPropertyManager opMgr = new AssessmentPropertyManager();
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentity efEntity = context.Assessment
								.SingleOrDefault( s => s.Id == entity.Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						if ( ValidateProfile( entity, ref  messages ) == false )
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
								string message = string.Format( "AssessmentManager. Assessment_Update Failed", "Attempted to update a Assessment. The process appeared to not work, but was not an exception, so we have no message, or no clue. Assessment: {0}, Id: {1}, updatedById: {2}", entity.Name, entity.Id, entity.LastUpdatedById );
								EmailManager.NotifyAdmin( "AssessmentManager. Assessment_Update Failed", message );
							}
						}
						//continue with parts regardless
						if ( UpdateParts( entity, ref messages ) == false )
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
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".Assessment_Update. id: {0}", entity.Id ) );
				statusMessage = ex.Message;
				isValid = false;
			}


			return isValid;
		}

		public bool ValidateProfile( ThisEntity profile, ref List<string> messages )
		{
			bool isValid = true;
			int count = messages.Count;

			if ( string.IsNullOrWhiteSpace( profile.Name ) )
			{
				messages.Add( "An Assessment name must be entered" );
			}
			if ( string.IsNullOrWhiteSpace( profile.Description ) )
			{
				messages.Add( "An Assessment Description must be entered" );
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
				messages.Add( "The Assessment Subject Webpage is invalid. " + commonStatusMessage );
			}

			if ( !IsUrlValid( profile.AvailableOnlineAt, ref commonStatusMessage ) )
			{
				messages.Add( "The Available Online At Url is invalid. " + commonStatusMessage );
			}

			if ( !IsUrlValid( profile.AvailabilityListing, ref commonStatusMessage ) )
			{
				messages.Add( "The Availability Listing Url is invalid. " + commonStatusMessage );
			}

			if ( !IsUrlValid( profile.ExternalResearch, ref commonStatusMessage ) )
				messages.Add( "The External Research Url is invalid. " + commonStatusMessage );
			if ( !IsUrlValid( profile.ProcessStandards, ref commonStatusMessage ) )
				messages.Add( "The Process Standards Url is invalid. " + commonStatusMessage );
			if ( !IsUrlValid( profile.ScoringMethodExample, ref commonStatusMessage ) )
				messages.Add( "The Scoring Method Example Url is invalid. " + commonStatusMessage );
			if ( !IsUrlValid( profile.AssessmentExample, ref commonStatusMessage ) )
				messages.Add( "The Assessment Example Url is invalid. " + commonStatusMessage );


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

		/// <summary>
		/// Update credential registry id, and set status published
		/// </summary>
		/// <param name="assessmentId"></param>
		/// <param name="envelopeId"></param>
		/// <param name="userId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool UpdateEnvelopeId( int assessmentId, string envelopeId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			bool updatingStatus = UtilityManager.GetAppKeyValue( "onRegisterSetEntityToPublic", false );
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					EM.Assessment efEntity = context.Assessment
									.SingleOrDefault( s => s.Id == assessmentId );

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
								string message = string.Format( thisClassName + ". UpdateEnvelopeId Failed", "Attempted to update an EnvelopeId. The process appeared to not work, but was not an exception, so we have no message, or no clue. assessment: {0}, envelopeId: {1}, updatedById: {2}", assessmentId, envelopeId, userId );
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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UpdateEnvelopeId(), assessment: {0}, envelopeId: {1}, updatedById: {2}", assessmentId, envelopeId, userId ) );
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
		public bool UnPublish( int assessmentId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					EM.Assessment efEntity = context.Assessment
									.SingleOrDefault( s => s.Id == assessmentId );

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
							string message = string.Format( thisClassName + ".UnPublish Failed", "Attempted to unpublish the Assessment. The process appeared to not work, but was not an exception, so we have no message, or no clue. AssessmentId: {0}, updatedById: {1}", assessmentId, userId );
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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UnPublish(), AssessmentId: {0}, updatedById: {1}", assessmentId, userId ) );
					statusMessage = ex.Message + ( ex.InnerException != null && ex.InnerException.InnerException != null ? " - " + ex.InnerException.InnerException.Message : "" );
				}
			}

			return isValid;
		}
		/// <summary>
		/// Delete an Assessment, and related Entity
		/// </summary>
		/// <param name="Id"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( int Id, ref string statusMessage )
		{
			bool isValid = false;
			if ( Id == 0 )
			{
				statusMessage = "Error - missing an identifier for the Assessment";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					DBentity efEntity = context.Assessment
								.SingleOrDefault( s => s.Id == Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						Guid rowId = efEntity.RowId;

						//need to remove from Entity.
						//could use a pre-delete trigger?
						//what about roles

						context.Assessment.Remove( efEntity );
						int count = context.SaveChanges();
						if ( count > 0 )
						{
							isValid = true;
							//do with trigger now
							///new EntityManager().Delete( rowId, ref statusMessage );
						}
					}
					else
					{
						statusMessage = "Error - delete failed, as record was not found.";
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + ".Assessment_Delete()" );

					if ( ex.InnerException != null && ex.InnerException.Message != null )
					{
						statusMessage = ex.InnerException.Message;

						if ( ex.InnerException.InnerException != null && ex.InnerException.InnerException.Message != null )
							statusMessage = ex.InnerException.InnerException.Message;
					}
					if ( statusMessage.ToLower().IndexOf( "the delete statement conflicted with the reference constraint" ) > -1 )
					{
						statusMessage = "Error: this assessment cannot be deleted as it is being referenced by other items, such as roles or credentials. These associations must be removed before this assessment can be deleted.";
					}
				}
			}
			return isValid;
		}

		#region Assessment properties ===================
		public bool UpdateParts( ThisEntity entity, ref List<string> messages )
		{
			bool isAllValid = true;
			EntityPropertyManager mgr = new EntityPropertyManager();
			Entity_ReferenceManager erm = new Entity_ReferenceManager();
			//CodesManager.PROPERTY_CATEGORY_ASSESSMENT_TYPE
			if ( mgr.UpdateProperties( entity.AssessmentMethodType, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, CodesManager.PROPERTY_CATEGORY_Assessment_Method_Type, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			if ( mgr.UpdateProperties( entity.AssessmentUseType, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, CodesManager.PROPERTY_CATEGORY_ASSESSMENT_USE_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			if ( mgr.UpdateProperties( entity.DeliveryType, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, CodesManager.PROPERTY_CATEGORY_LEARNING_OPP_DELIVERY_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			if ( mgr.UpdateProperties( entity.ScoringMethodType, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, CodesManager.PROPERTY_CATEGORY_Scoring_Method, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			if ( entity.OwnerRoles == null || entity.OwnerRoles.Items.Count == 0 )
			{
				messages.Add( "Invalid request, please select one or more roles for the owing agent." );
				isAllValid = false;
			} else {
				
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


			if ( erm.Entity_Reference_Update( entity.Subject, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_SUBJECT, false ) == false )
				isAllValid = false;

			if ( erm.Entity_Reference_Update( entity.Keyword, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_KEYWORD, false ) == false )
				isAllValid = false;

			if ( erm.Entity_Reference_Update( entity.OtherInstructionalProgramCategory, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_CIP, false ) == false )
				isAllValid = false;

			return isAllValid;
		}


		#endregion
		#endregion

		#region == Retrieval =======================
		public static ThisEntity GetBasic( int id )
		{
			ThisEntity entity = new ThisEntity();
			using ( var context = new Data.CTIEntities() )
			{
				DBentity item = context.Assessment
						.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					ToMap_Basic( item, entity, false, false );
				}
			}

			return entity;
		}
		public static ThisEntity Assessment_Get( int id, 
			bool forEditView = false, 
			bool includeWhereUsed = false)
		{
			ThisEntity entity = new ThisEntity();
			using ( var context = new Data.CTIEntities() )
			{
				DBentity item = context.Assessment
						.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity,
							true, //includingProperties
							true, //includingRoles
							forEditView, 
							includeWhereUsed);
				}
			}

			return entity;
		}
	
		//public static List<ThisEntity> Assessment_SelectAll( int userId = 0 )
		//{
		//	int pageSize = 0;
		//	int startingPageNbr = 1;
		//	int pTotalRows= 0;

		//	return QuickSearch( userId, "", startingPageNbr, pageSize, ref pTotalRows );
		//}

		public static List<string> Autocomplete( string pFilter, int pageNumber, int pageSize, int userId, ref int pTotalRows )
		{
			bool autocomplete = true;
			List<string> results = new List<string>();
			List<string> competencyList = new List<string>();
			//ref competencyList, 
			List<ThisEntity> list = Search( pFilter, "", pageNumber, pageSize, userId, ref pTotalRows, autocomplete );

			string prevName = "";
			foreach ( AssessmentProfile item in list )
			{
				//note excluding duplicates may have an impact on selected max terms
				if ( item.Name.ToLower() != prevName )
					results.Add( item.Name );

				prevName = item.Name.ToLower();
			}
			return results;
		}
		/// <summary>
		/// Search for assessments
		/// </summary>
		/// <returns></returns>
		public static List<ThisEntity> QuickSearch( int userId, string keyword, int pageNumber, int pageSize, ref int pTotalRows )
		{
			List<ThisEntity> list = new List<ThisEntity>();
			ThisEntity entity = new ThisEntity();
			keyword = string.IsNullOrWhiteSpace( keyword ) ? "" : keyword.Trim();
			if ( pageSize == 0 )
				pageSize = 500;
			int skip = 0;
			if ( pageNumber > 1 )
				skip = ( pageNumber - 1 ) * pageSize;

			using ( var context = new Data.CTIEntities() )
			{
				var Query = from Results in context.Assessment
						.Where( s => keyword == "" || s.Name.Contains( keyword ) )
						.OrderBy( s => s.Name )
						select Results;
				pTotalRows = Query.Count();
				var results = Query.Skip(skip).Take( pageSize )
					.ToList();

				//List<DBentity> results = context.Assessment
				//	.Where( s => keyword == "" || s.Name.Contains( keyword ) )
				//	.Take( pageSize )
				//	.OrderBy( s => s.Name )
				//	.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( DBentity item in results )
					{
						entity = new ThisEntity();
						ToMap( item, entity,
								false, //includingProperties
								false, //includingRoles
								false, //forEditView
								false //includeWhereUsed
								 );
						list.Add( entity );
					}

					//Other parts
				}
			}

			return list;
		}

		public static List<ThisEntity> Search( string pFilter, string pOrderBy, int pageNumber, int pageSize, int userId, ref int pTotalRows, bool autocomplete = false )
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

				using ( SqlCommand command = new SqlCommand( "[Assessment_Search]", c ) )
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
					item.CodedNotation = GetRowColumn( dr, "IdentificationCode", "" );
					item.ctid = GetRowPossibleColumn( dr, "CTID", "" );
					item.CredentialRegistryId = GetRowPossibleColumn( dr, "CredentialRegistryId", "" );

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
					//item.RowId = GetRowColumn( dr, "RowId", "" );

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
					//not used yet
					int competencies = GetRowPossibleColumn( dr, "Competencies", 0 );

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

			to.Id = from.Id;
			to.Name = GetData(from.Name);
			//to.StatusId = from.StatusId > 0 ? from.StatusId : ( to.StatusId > 0 ? to.StatusId : 1 );

			to.Description = GetData(from.Description);
			to.IdentificationCode = GetData( from.CodedNotation );
			to.VersionIdentifier = GetData( from.VersionIdentifier );
			
			//to.OtherAssessmentType = GetData( from.OtherAssessmentType );

			to.Url = GetUrlData( from.SubjectWebpage );
			to.AvailableOnlineAt = GetUrlData( from.AvailableOnlineAt );
			to.AvailabilityListing = GetUrlData( from.AvailabilityListing );
			to.AssessmentExampleUrl = GetData(from.AssessmentExample);
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

			if ( from.IsNewVersion )
			{
			}
			else
			{
				//to.AssessmentInformationUrl = GetUrlData( from.AssessmentInformationUrl );

				//to.AssessmentExampleUrl = GetMessages( from.AssessmentExampleUrl2);

			}
			if ( from.InLanguageId > 0 )
				to.InLanguageId = from.InLanguageId;
			else
				to.InLanguageId = null;

			to.CreditHourType = GetData( from.CreditHourType, null );
				to.CreditHourValue = SetData( from.CreditHourValue, 0.5M );
				to.CreditUnitTypeId = SetData( from.CreditUnitTypeId, 1 );
				to.CreditUnitTypeDescription = GetData( from.CreditUnitTypeDescription );
				to.CreditUnitValue = SetData( from.CreditUnitValue, 0.5M );

				to.DeliveryTypeDescription = from.DeliveryTypeDescription;
				to.VerificationMethodDescription = from.VerificationMethodDescription;
				to.AssessmentExampleDescription = from.AssessmentExampleDescription;
				to.AssessmentOutput = from.AssessmentOutput;
				to.ExternalResearch = from.ExternalResearch;

				to.HasGroupEvaluation = from.HasGroupEvaluation;
				to.HasGroupParticipation = from.HasGroupParticipation;
				to.IsProctored = from.IsProctored;

				to.ProcessStandards = from.ProcessStandards;
				to.ProcessStandardsDescription = from.ProcessStandardsDescription;

				to.ScoringMethodDescription = from.ScoringMethodDescription;
				to.ScoringMethodExample = from.ScoringMethodExample;
				to.ScoringMethodExampleDescription = from.ScoringMethodExampleDescription;

			

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = DateTime.Parse( from.DateEffective );
			else
				to.DateEffective = null;


			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById;
		}

		public static void ToMap( DBentity from, ThisEntity to, 
				bool includingProperties, 
				bool includingRoles, 
				bool forEditView,
				bool includeWhereUsed)
		{
			ToMap_Basic( from, to, true, forEditView );

			to.CredentialRegistryId = from.CredentialRegistryId;

			to.ResourceUrl = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS );
			//will need a category - should be reference, but used by latter
			//to.AssessmentExample = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_LEARNING_RESOURCE_URLS );

			to.AvailabilityListing = from.AvailabilityListing;
			//to.AssessmentInformationUrl = from.AssessmentInformationUrl;

			//to.AssessmentExampleUrl2 = CommaSeparatedListToStringList( from.AssessmentExampleUrl );
			to.AssessmentExample = from.AssessmentExampleUrl;
			to.AssessmentExampleDescription = from.AssessmentExampleDescription;
			//temp code to convert - was all accomplished
			//if ( to.AssessmentExampleUrl2.Count > 0 && ( to.ResourceUrl == null || to.ResourceUrl.Count == 0 ) )
			//{
			//	to.ResourceUrl = new List<TextValueProfile>();
			//	foreach ( string item in to.AssessmentExampleUrl2 )
			//	{
			//		TextValueProfile tvp = new TextValueProfile() { TextValue = item, ProfileSummary = item, TextTitle = "TBD", CategoryId = 25 };

			//		to.ResourceUrl.Add( tvp );
			//	}
			//}

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective =( (DateTime) from.DateEffective).ToShortDateString();
			else
				to.DateEffective = "";
			
			to.CodedNotation = from.IdentificationCode;
			to.AvailableOnlineAt = from.AvailableOnlineAt;

			//to.OtherAssessmentType = from.OtherAssessmentType;
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

			to.DeliveryTypeDescription = from.DeliveryTypeDescription;
			to.VerificationMethodDescription = from.VerificationMethodDescription;
			
			to.AssessmentOutput = from.AssessmentOutput;
			to.ExternalResearch = from.ExternalResearch;
			if ( from.HasGroupEvaluation != null )
				to.HasGroupEvaluation = (bool)from.HasGroupEvaluation;
			if ( from.HasGroupParticipation != null )
				to.HasGroupParticipation = ( bool ) from.HasGroupParticipation;
			if ( from.IsProctored != null )
				to.IsProctored = ( bool ) from.IsProctored;

			to.ProcessStandards = from.ProcessStandards;
			to.ProcessStandardsDescription = from.ProcessStandardsDescription;

			to.ScoringMethodDescription = from.ScoringMethodDescription;
			to.ScoringMethodExample = from.ScoringMethodExample;
			to.ScoringMethodExampleDescription = from.ScoringMethodExampleDescription;
			

			to.Subject = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SUBJECT );

			to.Keyword = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD );
			//properties
			if ( includingProperties )
			{
				//FillAssessmentType( from, to );
				//FillModalityType( from, to );
				to.AssessmentMethodType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_Assessment_Method_Type );

				to.AssessmentUseType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_ASSESSMENT_USE_TYPE );

				to.DeliveryType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_LEARNING_OPP_DELIVERY_TYPE );

				to.ScoringMethodType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_Scoring_Method );

				//to.AssessmentUseType = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_MODALITY_TYPE );

				to.Addresses = AddressProfileManager.GetAll( to.RowId );

				// Begin edits - Need these to populate Credit Unit Type -  NA 3/24/2017
				if( to.CreditUnitTypeId > 0 )
				{
					to.CreditUnitType = new Enumeration();
					var match = CodesManager.GetEnumeration( "creditUnit" ).Items.FirstOrDefault( m => m.CodeId == to.CreditUnitTypeId );
					if( match != null )
					{
						to.CreditUnitType.Items.Add( match );
					}
				}

				//this is ToMap_Basic
				//to.EstimatedCost = CostProfileManager.GetAll( to.RowId, forEditView );

			}
			//get competencies
			ToMap_Competencies( to );

			to.InstructionalProgramCategory = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CIP );

			to.OtherInstructionalProgramCategory = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_CIP );

			if (includingRoles) 
			{

				if ( forEditView )
				{
					//just get profile links
					//to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAllSummary( to.RowId, false );

					to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAllExceptOwnerSummary( to.RowId, to.OwningAgentUid, false, false );
					//USING OwnerRoles, not OwnerOrganizationRoles for edit
					//to.OwnerOrganizationRoles = Entity_AgentRelationshipManager.AgentEntityRole_GetOwnerSummary( to.RowId, to.OwningAgentUid, false );
				}
				else
				{
					//get as ennumerations
					to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAll_ToEnumeration( to.RowId, true );
				}
				//to.QualityAssuranceAction =	Entity_QualityAssuranceActionManager.QualityAssuranceActionProfile_GetAll( to.RowId );

			to.EstimatedDuration = DurationProfileManager.GetAll( to.RowId );

			to.Jurisdiction = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId );
				to.Region = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_RESIDENT );
				to.JurisdictionAssertions = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_OFFERREDIN );

			}


			//get condition profiles
			List<ConditionProfile> list = new List<ConditionProfile>();
			if ( forEditView )
			{
				list = Entity_ConditionProfileManager.GetAllForLinks( to.RowId );
				to.CommonConditions = Entity_CommonConditionManager.GetAll( to.RowId, true );
				to.CommonCosts = Entity_CommonCostManager.GetAll( to.RowId, forEditView );
			}
			else
			{
				list = Entity_ConditionProfileManager.GetAll( to.RowId );
				to.CommonConditions = Entity_CommonConditionManager.GetAll( to.RowId, false );

				to.CommonCosts = Entity_CommonCostManager.GetAll( to.RowId, forEditView );
			}
			if ( list != null && list.Count > 0 )
			{
				foreach ( ConditionProfile item in list )
				{
					if ( item.ConditionSubTypeId == Entity_ConditionProfileManager.ConditionSubType_Assessment )
					{
						to.AssessmentConnections.Add( item );
					}
					else if ( item.ConnectionProfileTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Requirement )
						to.Requires.Add( item );
					else if ( item.ConnectionProfileTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Recommendation )
						to.Recommends.Add( item );
					else if ( item.ConnectionProfileTypeId == Entity_ConditionProfileManager.ConnectionProfileType_Corequisite )
						to.Corequisite.Add( item );
					else if ( item.ConnectionProfileTypeId == Entity_ConditionProfileManager.ConnectionProfileType_EntryCondition )
						to.EntryCondition.Add( item );
					else
					{
						EmailManager.NotifyAdmin( "Unexpected Condition Profile for assessment", string.Format( "AssessmentId: {0}, ConditionProfileTypeId: {1}", to.Id, item.ConnectionProfileTypeId ) );

						//add to required, for dev only?
						if ( IsDevEnv() )
						{
							item.ProfileName = ( item.ProfileName ?? "" ) + " unexpected condition type of " + item.ConnectionProfileTypeId.ToString();
							to.Requires.Add( item );
						}
					}
				}

				
			}

			//to.AssessmentProcess = Entity_ProcessProfileManager.GetAll( to.RowId, true );
			List<ProcessProfile> processes = Entity_ProcessProfileManager.GetAll( to.RowId, forEditView );
			foreach ( ProcessProfile item in processes )
			{
				if ( item.ProcessTypeId == Entity_ProcessProfileManager.ADMIN_PROCESS_TYPE )
					to.AdministrationProcess.Add( item );
				else if ( item.ProcessTypeId == Entity_ProcessProfileManager.DEV_PROCESS_TYPE )
					to.DevelopmentProcess.Add( item );
				else if ( item.ProcessTypeId == Entity_ProcessProfileManager.MTCE_PROCESS_TYPE )
					to.MaintenanceProcess.Add( item );
				else
				{
					//unexpected
				}
			}

			to.WhereReferenced = new List<string>();
			//including with edit for now
			if ( includeWhereUsed || forEditView )
			{
				if ( from.Entity_Assessment != null && from.Entity_Assessment.Count > 0 )
				{
					foreach ( EM.Entity_Assessment item in from.Entity_Assessment )
					{
						to.WhereReferenced.Add( string.Format( "EntityUid: {0}, Type: {1}", item.Entity.EntityUid, item.Entity.Codes_EntityType.Title ) );
						//only parent for now
						if ( item.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_CONNECTION_PROFILE )
						{
							ConditionProfile cp = CondProfileMgr.GetAs_IsPartOf( item.Entity.EntityUid );
							to.IsPartOfConditionProfile.Add(cp);
							//to.IsPartOfConditionProfile.Add( CondProfileMgr.GetAs_IsPartOf( item.Entity.EntityUid ) );
							//need to check cond prof for parent of credential
							//will need to ensure no dups, or realistically, don't do the direct credential check
							if ( cp.ParentCredential != null && cp.ParentCredential.Id > 0)
							{
								AddCredentialReference( cp.ParentCredential.Id, to );
								//to.IsPartOfCredential.Add( cp.ParentCredential );
							}

						}
						else if ( item.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE )
						{
							to.IsPartOfLearningOpp.Add( LearningOpportunityManager.GetAs_IsPartOf( item.Entity.EntityUid, forEditView ) );
						}
						else if ( item.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_CREDENTIAL )
						{
							AddCredentialReference( (int)item.Entity.EntityBaseId, to );
							
						}
					}
				}
			}
			
		}
		private static void AddCredentialReference( int credentialId, ThisEntity to )
		{
			Credential exists = to.IsPartOfCredential.SingleOrDefault( s => s.Id == credentialId );
			if ( exists == null || exists.Id == 0 )
				to.IsPartOfCredential.Add( CredentialManager.GetBasic( ( int ) credentialId ) );
		} //

		public static void ToMap_Basic( DBentity from, ThisEntity to, bool includingCosts, bool forEditView )
		{
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

			to.SubjectWebpage = from.Url;
			to.VersionIdentifier = from.VersionIdentifier;

			//costs may be required for the list view, when called by the credential editor
			//make configurable
			if ( includingCosts )
			{
				to.EstimatedCost = CostProfileManager.GetAll( to.RowId, forEditView );


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

			//Need this for the detail page, since we now show durations by profile name - NA 4/13/2017
			to.EstimatedDuration = DurationProfileManager.GetAll( to.RowId );

			to.FinancialAssistance = Entity_FinancialAlignmentProfileManager.GetAll( to.RowId, forEditView );

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

		} //

		public static void ToMap_Competencies( ThisEntity to )
		{
			to.AssessesCompetenciesFrameworks = Entity_CompetencyFrameworkManager.GetAll( to.RowId, "assesses" );
			if ( to.AssessesCompetenciesFrameworks.Count > 0 )
				to.HasCompetencies = true;
			to.RequiresCompetenciesFrameworks = Entity_CompetencyFrameworkManager.GetAll( to.RowId, "requires" );
			if ( to.RequiresCompetenciesFrameworks.Count > 0 )
				to.HasCompetencies = true;
		}
		#endregion

	}
}
