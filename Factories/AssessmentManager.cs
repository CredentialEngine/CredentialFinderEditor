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
using CondProfileMgrOld = Factories.ConnectionProfileManager;
namespace Factories
{
	public class AssessmentManager : BaseFactory
	{
		static string thisClassName = "AssessmentManager";


		#region Assessment - persistance ==================

		/// <summary>
		/// add a Assessment
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Assessment_Add( ThisEntity entity, ref string statusMessage )
		{
			DBentity efEntity = new DBentity();
			//AssessmentPropertyManager opMgr = new AssessmentPropertyManager();
			using ( var context = new Data.CTIEntities() )
			{
				try
				{

					FromMap( entity, efEntity );

					efEntity.RowId = Guid.NewGuid();
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
						List<string> messages = new List<string>();
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
		public bool Assessment_Update( ThisEntity entity, ref string statusMessage )
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
						List<string> messages = new List<string>();
						if ( UpdateParts( entity, ref messages ) == false )
						{
							isValid = false;
							statusMessage += string.Join( ",", messages.ToArray() );
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

			if ( mgr.UpdateProperties( entity.AssessmentType, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, CodesManager.PROPERTY_CATEGORY_ASSESSMENT_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			if ( mgr.UpdateProperties( entity.Modality, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, CodesManager.PROPERTY_CATEGORY_MODALITY_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			if ( mgr.UpdateProperties( entity.DeliveryType, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, CodesManager.PROPERTY_CATEGORY_LEARNING_OPP_DELIVERY_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;	
			//if ( UpdateProperties( entity, ref messages ) == false )
			//{
			//	isAllValid = false;
			//}

			if ( erm.EntityUpdate( entity.AssessmentExampleUrl, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS, false ) == false )
				isAllValid = false;

			if ( erm.EntityUpdate( entity.Subjects, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_SUBJECT, false ) == false )
				isAllValid = false;

			if ( erm.EntityUpdate( entity.Keywords, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_KEYWORD, false ) == false )
				isAllValid = false;

			//if ( new DurationProfileManager().DurationProfileUpdate( entity.EstimatedDuration, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, entity.LastUpdatedById, ref messages ) == false )
			//{
			//	isAllValid = false;
			//}

			//if ( !entity.IsNewVersion )
			//{
			//	if ( new Entity_AgentRelationshipManager().Entity_UpdateAgent_SingleRole( entity.OrganizationRole,
			//		entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, entity.LastUpdatedById, ref messages, ref count ) == false )
			//	{
			//		isAllValid = false;
			//	}
			//}

			//if ( new RegionsManager().JurisdictionProfile_Update( entity.Jurisdiction, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, entity.LastUpdatedById, RegionsManager.JURISDICTION_PURPOSE_SCOPE, ref messages ) == false )
			//{
			//	isAllValid = false;
			//}

			//if ( new CostProfileManager().CostProfileUpdate( entity.EstimatedCost, entity.RowId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE, entity.LastUpdatedById, ref messages ) == false )
			//{
			//	isAllValid = false;
			//}
			return isAllValid;
		}


		#endregion
		#endregion

		#region == Retrieval =======================

		public static ThisEntity Assessment_Get( int id, 
			bool forEditView = false, 
			bool includeWhereUsed = false, 
			bool isNewVersion = false )
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
							includeWhereUsed, 
							isNewVersion );
				}
			}

			return entity;
		}
	
		public static List<ThisEntity> Assessment_SelectAll( int userId = 0 )
		{
			int pageSize = 0;
			int startingPageNbr = 1;
			int pTotalRows= 0;

			return QuickSearch( userId, "", startingPageNbr, pageSize, ref pTotalRows );
		}

		public static List<string> Autocomplete( string pFilter, int pageNumber, int pageSize, int userId, ref int pTotalRows )
		{
			bool autocomplete = true;
			List<string> results = new List<string>();
			List<string> competencyList = new List<string>();
			//ref competencyList, 
			List<ThisEntity> list = Search( pFilter, "", pageNumber, pageSize, userId, ref pTotalRows, autocomplete );

			foreach ( AssessmentProfile item in list )
				results.Add( item.Name );

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
								false, //includeWhereUsed
								true );
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
					//for autocomplete, only need name
					if ( autocomplete )
					{
						list.Add( item );
						continue;
					}

					item.Description = GetRowColumn( dr, "Description", "" );
					string rowId = GetRowColumn( dr, "RowId" );
					item.RowId = new Guid( rowId );

					item.Url = GetRowColumn( dr, "URL", "" );
					item.AvailableOnlineAt = GetRowPossibleColumn( dr, "AvailableOnlineAt", "" );
					item.CanEditRecord = GetRowColumn( dr, "CanEditRecord", false );
					item.StatusId = GetRowColumn( dr, "StatusId", 1 );

					item.IdentificationCode = GetRowColumn( dr, "IdentificationCode", "" );
					item.ManagingOrganization = GetRowPossibleColumn( dr, "ManagingOrganization", "" );
					item.ManagingOrgId = GetRowPossibleColumn( dr, "ManagingOrgId", 0 );

					item.CreatedByOrganization = GetRowColumn( dr, "Organization", "" );
					item.CreatedByOrganizationId = GetRowColumn( dr, "OrgId", 0 );
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
			to.Name = from.Name;
			to.StatusId = from.StatusId > 0 ? from.StatusId : 1;

			to.Description = from.Description;
			to.AssessmentExampleUrl = GetMessages( from.AssessmentExampleUrl2);
			//generally the managing orgId should not be allowed to change in the interface - yet
			if ( from.ManagingOrgId > 0
				&& from.ManagingOrgId != ( to.ManagingOrgId ?? 0 ) )
				to.ManagingOrgId = from.ManagingOrgId;

			to.Url = from.Url;
			to.IdentificationCode = from.IdentificationCode;
			to.OtherAssessmentType = from.OtherAssessmentType;
			to.AvailableOnlineAt = from.AvailableOnlineAt;

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = DateTime.Parse( from.DateEffective );
			else
				to.DateEffective = null;


			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById;
		}

		public static void ToMap( DBentity from, ThisEntity to, 
				bool includingProperties = false, 
				bool includingRoles = true, 
				bool forEditView = true,
				bool includeWhereUsed = true, 
				bool newVersion = false )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.StatusId = from.StatusId ?? 1;
			to.ManagingOrgId = from.ManagingOrgId ?? 0;
			to.IsNewVersion = newVersion;

			to.Name = from.Name;
			to.Description = from.Description == null ? "" : from.Description;
			//will need a category
			to.AssessmentExampleUrl = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS );
			to.AssessmentExampleUrl2 = CommaSeparatedListToStringList( from.AssessmentExampleUrl );
			if ( to.AssessmentExampleUrl2.Count > 0 &&  (to.AssessmentExampleUrl == null || to.AssessmentExampleUrl.Count == 0 ))
			{
				to.AssessmentExampleUrl  = new List<TextValueProfile>();
				foreach ( string item in to.AssessmentExampleUrl2 )
				{
					TextValueProfile tvp = new TextValueProfile() { TextValue = item, ProfileSummary = item, TextTitle = "TBD", CategoryId = 25 };
					
					to.AssessmentExampleUrl.Add( tvp );
				}
			}

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective =( (DateTime) from.DateEffective).ToShortDateString();
			else
				to.DateEffective = "";
			to.Url = from.Url;
			to.IdentificationCode = from.IdentificationCode;
			to.AvailableOnlineAt = from.AvailableOnlineAt;

			to.OtherAssessmentType = from.OtherAssessmentType;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;


			to.Subjects = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SUBJECT );

			to.Keywords = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD );
			//properties
			if ( includingProperties )
			{
				//FillAssessmentType( from, to );
				//FillModalityType( from, to );
				to.AssessmentType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_ASSESSMENT_TYPE );

				to.Modality = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_MODALITY_TYPE );

				to.DeliveryType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_LEARNING_OPP_DELIVERY_TYPE );


				//to.Modality = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_MODALITY_TYPE );

				to.Addresses = AddressProfileManager.GetAll( to.RowId );
			}

			to.AssessesCompetencies = Entity_CompetencyManager.GetAll( to.RowId, "assesses" );
			if ( to.AssessesCompetencies.Count > 0 )
				to.HasCompetencies = true;
			to.RequiresCompetencies = Entity_CompetencyManager.GetAll( to.RowId, "requires" );

			to.AssessesCompetenciesFrameworks = Entity_CompetencyFrameworkManager.GetAll( to.RowId, "assesses" );
			if ( to.AssessesCompetenciesFrameworks.Count > 0 )
				to.HasCompetencies = true;
			to.RequiresCompetenciesFrameworks = Entity_CompetencyFrameworkManager.GetAll( to.RowId, "requires" );


			if (includingRoles) 
			{

				if ( forEditView )
				{
					//just get profile links
					to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAllSummary( to.RowId, false );
				}
				else
				{
					//get as ennumerations
					to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAll_ToEnumeration( to.RowId, true );
				}
			to.QualityAssuranceAction = Entity_QualityAssuranceActionManager.QualityAssuranceActionProfile_GetAll( to.RowId );

			to.EstimatedDuration = DurationProfileManager.GetAll( to.RowId );

			to.Jurisdiction = RegionsManager.Jurisdiction_GetAll( to.RowId );

			to.EstimatedCost = CostProfileManager.CostProfile_GetAll( to.RowId );

			//need to exclude in light versions
			to.Requires = Entity_ConditionProfileManager.GetAll( to.RowId );

			}

			to.WhereReferenced = new List<string>();
			if ( from.Entity_Assessment != null && from.Entity_Assessment.Count > 0 )
			{
				foreach (EM.Entity_Assessment item in from.Entity_Assessment) 
				{
					to.WhereReferenced.Add( string.Format("EntityUid: {0}, Type: {1}", item.Entity.EntityUid, item.Entity.Codes_EntityType.Title ));
					//only parent for now
					if ( item.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_CONNECTION_PROFILE )
					{
						to.IsPartOfConditionProfile.Add( CondProfileMgr.GetAs_IsPartOf( item.Entity.EntityUid ) );
					}
				}
			}
		

		}
		
		#endregion

	}
}
