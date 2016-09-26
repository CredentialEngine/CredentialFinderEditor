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
using MN = Models.Node;
using EM = Data;
using Utilities;
using DBentity = Data.LearningOpportunity;
using ThisEntity = Models.ProfileModels.LearningOpportunityProfile;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
using CondProfileMgr = Factories.Entity_ConditionProfileManager;
using CondProfileMgrOld = Factories.ConnectionProfileManager;

namespace Factories
{
	public class LearningOpportunityManager : BaseFactory
	{
		static string thisClassName = "LearningOpportunityManager";


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

					context.LearningOpportunity.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";
						entity.Id = efEntity.Id;
						entity.RowId = efEntity.RowId;
						List<string> messages = new List<string>();
						if ( UpdateParts( entity, userId, ref messages ) == false )
						{
							statusMessage += string.Join( ", ", messages.ToArray() );
						}

						return efEntity.Id;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the add was not successful. ";
						string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a Learning Opportunity. The process appeared to not work, but was not an exception, so we have no message, or no clue. LearningOpportunity: {0}, createdById: {1}", entity.Name, entity.CreatedById );
						EmailManager.NotifyAdmin( thisClassName + ". Entity_Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					//LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
					string message = thisClassName + string.Format( ".Entity_Add() DbEntityValidationException, Name: {0}", efEntity.Name );
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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Name: {0}", efEntity.Name ) );
				}
			}

			return efEntity.Id;
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
			
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentity efEntity = context.LearningOpportunity
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
								string message = thisClassName + string.Format( ". Update Failed", "Attempted to update a LearningOpportunity. The process appeared to not work, but was not an exception, so we have no message, or no clue. LearningOpportunity: {0}, Id: {1}, updatedById: {2}", entity.Name, entity.Id, entity.LastUpdatedById );
								EmailManager.NotifyAdmin( thisClassName + ". Update Failed", message );
							}
						}
						//continue with parts regardless
						List<string> messages = new List<string>();
						if ( UpdateParts( entity, userId, ref messages ) == false )
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
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".Update. id: {0}", entity.Id ) );
				statusMessage = ex.Message;
				isValid = false;
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

			if ( erm.EntityUpdate( entity.LearningResourceUrl, entity.RowId, CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, entity.LastUpdatedById, ref messages, 25, false ) == false )
				isAllValid = false;

			//TODO - make obsolete
			//if ( erm.EntityUpdate( entity.LearningCompetencies, entity.RowId, CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_COMPETENCY ) == false )
			//	isAllValid = false;


			if ( erm.EntityUpdate( entity.Subjects, entity.RowId, CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_SUBJECT, false ) == false )
				isAllValid = false;

			if ( erm.EntityUpdate( entity.Keywords, entity.RowId, CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_KEYWORD, false ) == false )
				isAllValid = false;
			
			
			//if ( new CostProfileManager().CostProfileUpdate( entity.EstimatedCost, entity.RowId, CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, userId, ref messages ) == false )
			//{
			//	isAllValid = false;
			//}
	

			//CIP codes???

			//regions
			//if ( !entity.IsNewVersion )
			//{
			//if ( new RegionsManager().JurisdictionProfile_Update( entity.Jurisdiction, entity.RowId, CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, userId, RegionsManager.JURISDICTION_PURPOSE_SCOPE, ref messages ) == false )
			//{
			//	isAllValid = false;
			//}
			//}
			return isAllValid;
		}
		public bool UpdateProperties( ThisEntity entity, int userId, ref List<string> messages )
		{
			bool isAllValid = true;
			string statusMessage = "";

			EntityPropertyManager mgr = new EntityPropertyManager();

			if ( mgr.UpdateProperties( entity.LearningOpportunityDeliveryType, entity.RowId, CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE, CodesManager.PROPERTY_CATEGORY_LEARNING_OPP_DELIVERY_TYPE, userId, ref messages ) == false )
			{
				isAllValid = false;
			}
			return isAllValid;
		}


		#endregion
		#endregion

		#region == Retrieval =======================
		public static ThisEntity GetForDetail( int id)
		{
			ThisEntity entity = new ThisEntity();
			bool includingProfiles = true;

			using ( var context = new Data.CTIEntities() )
			{
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

		public static MN.ProfileLink GetAsLinkProfile( int id )
		{
			MN.ProfileLink entity = new MN.ProfileLink();

			using ( var context = new Data.CTIEntities() )
			{
				DBentity item = context.LearningOpportunity
						.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					entity.Id = item.Id;
					entity.RowId = item.RowId;
					entity.Name = item.Name;
					entity.TypeName = "LearningOpportunity";
				}
			}

			return entity;
		}
		public static ThisEntity GetAs_IsPartOf( Guid rowId )
		{
			ThisEntity entity = new ThisEntity();

			using ( var context = new Data.CTIEntities() )
			{
				DBentity item = context.LearningOpportunity
						.SingleOrDefault( s => s.RowId == rowId );

				if ( item != null && item.Id > 0 )
				{
					entity.Id = item.Id;
					entity.RowId = item.RowId;
					entity.Name = item.Name;
					entity.Description = item.Description;
				}
			}

			return entity;
		}
		//public static List<ThisEntity> SelectAll( int userId = 0 )
		//{
		//	int pageSize = 0;
		//	int startingPageNbr = 1;
		//	int pTotalRows = 0;

		//	return Search( userId, "", startingPageNbr, pageSize, ref pTotalRows );
		//}

		/// <summary>
		/// Search for assessments
		/// </summary>
		/// <returns></returns>
		//public static List<ThisEntity> Search( int userId, string keyword, int pageNumber, int pageSize, ref int pTotalRows )
		//{
		//	List<ThisEntity> list = new List<ThisEntity>();
		//	ThisEntity entity = new ThisEntity();
		//	keyword = string.IsNullOrWhiteSpace( keyword ) ? "" : keyword.Trim();
		//	if ( pageSize == 0 )
		//		pageSize = 500;
		//	int skip = 0;
		//	if ( pageNumber > 1 )
		//		skip = ( pageNumber - 1 ) * pageSize;

		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		var Query = from Results in context.LearningOpportunity
		//				.Where( s => keyword == "" || s.Name.Contains( keyword ) )
		//				.OrderBy( s => s.Name )
		//					select Results;
		//		pTotalRows = Query.Count();
		//		var results = Query.Skip( skip ).Take( pageSize )
		//			.ToList();

		//		//List<DBentity> results = context.LearningOpportunity
		//		//	.Where( s => keyword == "" || s.Name.Contains( keyword ) )
		//		//	.Take( pageSize )
		//		//	.OrderBy( s => s.Name )
		//		//	.ToList();

		//		if ( results != null && results.Count > 0 )
		//		{
		//			foreach ( DBentity item in results )
		//			{
		//				entity = new ThisEntity();
		//				//set forEditView to as don't want deep results for a search
		//				ToMap( item, entity, false, true, false, true );
		//				list.Add( entity );
		//			}

		//			//Other parts
		//		}
		//	}

		//	return list;
		//}
		public static List<string> Autocomplete( string pFilter, int pageNumber, int pageSize, int userId, ref int pTotalRows )
		{
			bool autocomplete = true;
			List<string> results = new List<string>();
			List<string> competencyList = new List<string>();
			//get minimal entity
			List<ThisEntity> list = Search( pFilter, "", pageNumber, pageSize, userId, ref pTotalRows, ref competencyList, autocomplete );

			foreach ( LearningOpportunityProfile item in list )
				results.Add( item.Name );

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

					org = GetRowPossibleColumn( dr, "Organization", "" );
					orgId = GetRowPossibleColumn( dr, "OrgId", 0 );
					if ( orgId > 0 )
					{
						item.Provider = new Organization() { Id = orgId, Name = org };
					}

					item.CreatedByOrganization = GetRowColumn( dr, "Organization", "" );
					item.CreatedByOrganizationId = GetRowColumn( dr, "OrgId", 0 );

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
							item.TeachesCompetencies = Entity_CompetencyManager.GetAll( item.RowId, "teaches" );
						else
						{
							item.TeachesCompetencies = new List<CredentialAlignmentObjectProfile>();
							List<CredentialAlignmentObjectProfile> all = Entity_CompetencyManager.GetAll( item.RowId, "teaches" );
							foreach ( CredentialAlignmentObjectProfile next in all )
							{
								//just do desc for now
								string orig = next.Description;
								foreach ( string filter in competencyList )
								{
									//not ideal, as would be an exact match
									orig = orig.Replace( filter, string.Format( "<span class='highlight'>{0}<\\span>", filter ) );
								}
								if ( orig != next.Description )
								{
									next.Description = orig;
									item.TeachesCompetencies.Add( next );
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

			to.Id = from.Id;
			to.Name = from.Name;
			to.StatusId = from.StatusId > 0 ? from.StatusId : 1;

			to.Description = from.Description;
			to.LearningResourceUrl = GetMessages( from.LearningResourceUrl2 );
			//generally the managing orgId should not be allowed to change in the interface - yet
			if ( from.ManagingOrgId > 0
				&& from.ManagingOrgId != ( to.ManagingOrgId ?? 0 ) )
				to.ManagingOrgId = from.ManagingOrgId;

			to.Url = from.Url;
			to.IdentificationCode = from.IdentificationCode;
			to.AvailableOnlineAt = from.AvailableOnlineAt;

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = DateTime.Parse( from.DateEffective );
			else
				to.DateEffective = null;
			if ( IsGuidValid( from.ProviderUid ) )
				to.ProviderUid = from.ProviderUid;
			else
				to.ProviderUid = null;

			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById;
		}
		public static void ToMap( DBentity from, ThisEntity to, 
				bool includingProperties = false, 
				bool includingProfiles = true, 
				bool forEditView = true, 
				bool includeWhereUsed = true)
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.StatusId = from.StatusId ?? 1;
			to.ManagingOrgId = from.ManagingOrgId ?? 0;

			to.Name = from.Name;
			to.Description = from.Description == null ? "" : from.Description;

			to.Url = from.Url;
			to.IdentificationCode = from.IdentificationCode;
			to.AvailableOnlineAt = from.AvailableOnlineAt;

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = ( ( DateTime ) from.DateEffective ).ToShortDateString();
			else
				to.DateEffective = "";
			if ( from.ProviderUid != null )
				to.ProviderUid = (Guid) from.ProviderUid;
			

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

			//to.LearningResourceUrl2 = CommaSeparatedListToStringList( from.LearningResourceUrl );

			//will need a category
			to.LearningResourceUrl = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS );

			to.Subjects = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SUBJECT );

			to.Keywords = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD );

			
			//properties
			if ( includingProperties )
			{
				to.LearningOpportunityDeliveryType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_LEARNING_OPP_DELIVERY_TYPE );

				to.Addresses = AddressProfileManager.GetAll( to.RowId );
			}
			//need to exclude in light versions
			to.Requires = Entity_ConditionProfileManager.GetAll( to.RowId );

			//if ( includingProfiles )
			//{
				to.InstructionalProgramCategory = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CIP );

				//if ( to.IsNewVersion )
				//{
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
				//TODO - re: forEditView, not sure about approach for learning opp parts
				to.HasPart = Entity_LearningOpportunityManager.LearningOpps_GetAll( to.RowId, forEditView );
				foreach ( ThisEntity e in to.HasPart )
				{
					if ( e.HasCompetencies || e.ChildHasCompetencies )
					{
						to.ChildHasCompetencies = true;
						break;
					}
				}
			//}

			//TODO - remove once TeachesCompetenciesFrameworks is accepted
			to.TeachesCompetencies = Entity_CompetencyManager.GetAll( to.RowId, "teaches" );
			if ( to.TeachesCompetencies.Count > 0 )
				to.HasCompetencies = true;
			to.RequiresCompetencies = Entity_CompetencyManager.GetAll( to.RowId, "requires" );

			to.TeachesCompetenciesFrameworks = Entity_CompetencyFrameworkManager.GetAll( to.RowId, "teaches" );
			if ( to.TeachesCompetenciesFrameworks.Count > 0 )
				to.HasCompetencies = true;
			to.RequiresCompetenciesFrameworks = Entity_CompetencyFrameworkManager.GetAll( to.RowId, "requires" );
			//where used ==> not really used yet??
			//16-09-02 mp - always get for now
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
							to.IsPartOf.Add( GetAs_IsPartOf( item.Entity.EntityUid ) );
						}
						else if ( item.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_CONNECTION_PROFILE )
						{
							to.IsPartOfConditionProfile.Add( CondProfileMgr.GetAs_IsPartOf( item.Entity.EntityUid ) );
						}

					}
				}
			//}

		}
		
		#endregion
	}
}
