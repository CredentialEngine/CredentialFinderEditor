using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using Models;
using Models.Common;
using Models.Helpers.Cass;
using Utilities;
using DBEntity = Data.Entity_Competency;
using ThisEntity = Models.ProfileModels.Entity_Competency;
using ThisFramework = Models.Common.CredentialAlignmentObjectFrameworkProfile;
using ViewContext = Data.Views.CTIEntities1;
using ThisEntityItem = Models.Common.CredentialAlignmentObjectItemProfile;


namespace Factories
{
    public class Entity_CompetencyManager : BaseFactory
    {
        static string thisClassName = "Entity_CompetencyManager";

       
        #region Persistance ===================

        /// <summary>
        /// Add a competency
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="parentUid"></param>
        /// <param name="userId"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public bool Save( ThisEntity entity,
                Guid parentUid,
                int userId,
                ref List<string> messages )
        {

            if ( !IsValidGuid( parentUid ) )
            {
                messages.Add( "Error: the parent identifier was not provided." );
                return false;
            }

            DBEntity efEntity = new DBEntity();

            Entity parent = EntityManager.GetEntity( parentUid );
            if ( parent == null || parent.Id == 0 )
            {
                messages.Add( "Error - the parent entity was not found." );
                return false;
            }

            return Save( entity, parent, userId, ref messages );
            
        }

        public bool Save( ThisEntity entity,
                Entity parent,
                int userId,
                ref List<string> messages )
        {
            bool isValid = true;
            int count = 0;

            DBEntity efEntity = new DBEntity();

            using ( var context = new Data.CTIEntities() )
            {
                if ( ValidateProfile( entity, ref messages ) == false )
                {
                    return false;
                }

                if ( entity.Id == 0 )
                {
                    //check if already exists
                    //TODO - will need to add alignment type
                    ThisEntity item = Get( parent.Id, entity.CompetencyId );
                    if ( item != null && item.Id > 0 )
                    {
                        //messages.Add( string.Format( "Warning: the selected competency {0} - {1} already exists!", entity.Name, entity.Description ) );
                        return false;
                    }
                    //add
                    efEntity = new DBEntity();
                    efEntity.EntityId = parent.Id;
                    efEntity.CompetencyId = entity.CompetencyId;
                    efEntity.Created = DateTime.Now;
                    efEntity.CreatedById = userId;

                    context.Entity_Competency.Add( efEntity );
                    count = context.SaveChanges();
                    //update profile record so doesn't get deleted
                    entity.Id = efEntity.Id;
                    entity.EntityId = parent.Id;

                    if ( count == 0 )
                    {
                        messages.Add( string.Format( " Unable to add Competency: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.FrameworkCompetency.Name ) ? "no description" : entity.FrameworkCompetency.Description ) );
                    }

                }
                else
                {
                    //no update possible at this time - that is nothing to update
                    entity.EntityId = parent.Id;

                    efEntity = context.Entity_Competency.FirstOrDefault( s => s.Id == entity.Id );
                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        //update
                        efEntity.CompetencyId = entity.CompetencyId;
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
		/// </summary>
		/// <param name="recordId"></param>
		/// <param name="user"></param>
		/// <param name="parent">Return the parent Entity</param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( int recordId, Models.AppUser user, ref Entity parent, ref string statusMessage )
        {
            bool isOK = true;
            parent = new Entity();
            try
            {
                using ( var context = new Data.CTIEntities() )
                {
                    DBEntity p = context.Entity_Competency.FirstOrDefault( s => s.Id == recordId );
                    if ( p != null && p.Id > 0 )
                    {
                        //save Entity for logging
                        EntityManager.MapFromDB( p.Entity, parent );
                        string competency = p.EducationFramework_Competency.Name;

                        context.Entity_Competency.Remove( p );
                        if ( context.SaveChanges() > 0)
                        {
							new ActivityManager().SiteActivityAdd( new SiteActivity()
							{
								ActivityType = parent.EntityType,
								Activity = "Editor",
								Event = "Delete Competency",
								Comment = string.Format( "{0} removed Competency '{1}' from record: {2}", user.FullName(), competency, parent.EntityBaseId ),
								ActivityObjectId = parent.EntityBaseId,
								ActionByUserId = user.Id,
								ActivityObjectParentEntityUid = parent.EntityUid
								//,DataOwnerCTID = item.OwningOrganizationCtid		//TBD
							} );
							//new ProfileServices().UpdateTopLevelEntityLastUpdateDate( parent.Id,string.Format( "Entity Update triggered by {0} adding competencies to : {1}, BaseId: {2}",user.FullName(),parent.EntityType,parent.EntityBaseId ) );
						}
                    }
                    else
                    {
                        statusMessage = string.Format( "Entity Competency record was not found: {0}", recordId );
                        isOK = false;
                    }
                }
            } catch (Exception ex)
            {
                statusMessage = string.Format( "Error encountered attempting to delete a Competency: {0}", ex.Message );
                isOK = false;
            }
            return isOK;
        }

		public bool DeleteAll( Guid parentUid, int frameworkId, string frameworkName, Models.AppUser user, ref List<string> messages )
		{
			bool isOK = true;
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Entity_CompetencyManager.DeleteAll() Error - the parent entity was not found.");
				return false;
			}

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					//option one
					var list = context.Entity_Competency
								.Where( s => s.EntityId == parent.Id 
								&& s.EducationFramework_Competency.EducationFrameworkId == frameworkId)
								.ToList();
					
					//option two
					context.Entity_Competency.RemoveRange( context.Entity_Competency
							.Where( s => s.EntityId == parent.Id
							&& s.EducationFramework_Competency.EducationFrameworkId == frameworkId ) );
					if ( context.SaveChanges() > 0 )
					{

						ActivityManager activityMgr = new ActivityManager();
						activityMgr.SiteActivityAdd( new SiteActivity()
						{
							ActivityType = parent.EntityType,
							Activity = "Competency Framework",
							Event = "Delete All Competencies",
							Comment = string.Format( "Action by {0} (likely during a Bulk Upload), triggered deleting all competencies from framework '{1}' for this : {2} ({3}).", user.FullName(), frameworkName, parent.EntityType, parent.EntityBaseId ),
							ActionByUserId = user.Id,
							TargetObjectId = frameworkId,
							ActivityObjectParentEntityUid = parentUid
						} );
					}
				}
			}
			catch ( Exception ex )
			{
				messages.Add( string.Format( "Error encountered attempting to delete all Competencies for a framework. Context: User: {0} (likely during a Bulk Upload), triggered deleting all competencies from framework '{1}' for this : {2} ({3}). Mesage: {4} ", user.FullName(), frameworkName, parent.EntityType, parent.EntityBaseId, ex.Message)) ;
				LoggingHelper.LogError( ex, "Entity_CompetencyManager.DeleteAll()" );
				isOK = false;
			}
			return isOK;
		}

		public bool ValidateProfile( ThisEntity profile, ref List<string> messages )
        {
            bool isValid = true;

            if ( profile.CompetencyId < 1 )
            {
                messages.Add( "A competency identifier must be included." );
                isValid = false;
            }

            return isValid;
        }

		#endregion
		#region  retrieval ==================

		/// <summary>
		/// Get all competency frameworks for the parent as alignment objects
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="alignmentType">If blank, get all types</param>
		public static List<ThisFramework> GetAll( Guid parentUid, string alignmentType )
		{
			//return GetAllAsAlignmentObjects( parentUid, alignmentType );

			ThisFramework entity = new ThisFramework();
			List<ThisFramework> list = new List<ThisFramework>();

			List<CredentialAlignmentObjectItemProfile> list2 = new List<CredentialAlignmentObjectItemProfile>();
			CredentialAlignmentObjectItemProfile compItem = new CredentialAlignmentObjectItemProfile();
			string viewerUrl = UtilityManager.GetAppKeyValue( "cassResourceViewerUrl" );
			Entity parent = EntityManager.GetEntity( parentUid );
			int prevFrameworkId = 0;
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBEntity> results = context.Entity_Competency
							.Where( s => s.EntityId == parent.Id
							)
							.OrderBy( s => s.EducationFramework_Competency.EducationFramework.Name )
							.ThenBy( s => s.EducationFramework_Competency.Name )
							.ToList();
					//&& ( alignmentType == "" || s.AlignmentType == alignmentType ) 
					if ( results != null && results.Count > 0 )
					{
						foreach ( DBEntity item in results )
						{
							compItem = new CredentialAlignmentObjectItemProfile();
							if ( item.EducationFramework_Competency.EducationFrameworkId != prevFrameworkId )
							{
								if ( prevFrameworkId > 0 )
								{
									list.Add( entity );
									entity = new ThisFramework();
								}
								entity.EducationalFrameworkName = item.EducationFramework_Competency.EducationFramework.Name;
								entity.EducationalFrameworkUrl = item.EducationFramework_Competency.EducationFramework.RepositoryUri;
								entity.AlignmentType = alignmentType;
								if ( !string.IsNullOrWhiteSpace( viewerUrl ) && entity.IsARegistryFrameworkUrl )
								{
									entity.CaSSViewerUrl = string.Format( viewerUrl, UtilityManager.GenerationMD5String( entity.EducationalFrameworkUrl ) );
								}
							}

							MapFromDBAsAlignmentObjects( item, compItem );
							entity.Items.Add( compItem );

							prevFrameworkId = item.EducationFramework_Competency.EducationFrameworkId;
						}

						if ( prevFrameworkId > 0 )
							list.Add( entity );

					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".GetAllAsAlignmentObjects" );
			}
			return list;
		} //



		public static List<CassCompetencyV2> GetAllAsCassCompetencies( Guid parentUid )
        {
            CassCompetencyV2 entity = new CassCompetencyV2();
            List<CassCompetencyV2> list = new List<CassCompetencyV2>();

            Entity parent = EntityManager.GetEntity( parentUid );
            if ( parent == null || parent.Id == 0 )
            {
                return list;
            }
            try
            {
                using ( var context = new Data.CTIEntities() )
                {
                    List<DBEntity> results = context.Entity_Competency
                            .Where( s => s.EntityId == parent.Id )
                            .OrderBy( s => s.EducationFramework_Competency.EducationFramework.Name )
                            .ThenBy( s => s.EducationFramework_Competency.Name )
                            .ToList();
                    //&& ( alignmentType == "" || s.AlignmentType == alignmentType ) 
                    if ( results != null && results.Count > 0 )
                    {
                        foreach ( DBEntity item in results )
                        {
                            entity = new CassCompetencyV2();
                            MapFromDB( item, entity ); 
                            list.Add( entity );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".GetAllAsCassCompetencies( Guid parentUid, string alignmentType )" );
            }
            return list;
        }//

		//public static List<ThisFramework> GetAllAsAlignmentObjects( Guid parentUid, string alignmentType )
		//{

		//}//

		/// <summary>
		/// Get all records for the parent
		/// Uses the parent Guid to retrieve the related ThisEntity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="alignmentType">If blank, get all types</param>
		//public static List<ThisEntity> GetAll( Guid parentUid, string alignmentType = "" )
		//{
		//    ThisEntity entity = new ThisEntity();
		//    List<ThisEntity> list = new List<ThisEntity>();

		//    Entity parent = EntityManager.GetEntity( parentUid );
		//    if ( parent == null || parent.Id == 0 )
		//    {
		//        return list;
		//    }
		//    try
		//    {
		//        using ( var context = new Data.CTIEntities() )
		//        {
		//            List<DBEntity> results = context.Entity_Competency
		//                    .Where( s => s.EntityId == parent.Id
		//                    )
		//                    .OrderBy( s => s.EducationFramework_Competency.EducationFramework.Name )
		//                    .ThenBy( s => s.EducationFramework_Competency.Name )
		//                    .ToList();
		//            //&& ( alignmentType == "" || s.AlignmentType == alignmentType ) 
		//            if ( results != null && results.Count > 0 )
		//            {
		//                foreach ( DBEntity item in results )
		//                {
		//                    entity = new ThisEntity();
		//                    MapFromDB( item, entity ); 
		//                    list.Add( entity );
		//                }
		//            }
		//        }
		//    }
		//    catch ( Exception ex )
		//    {
		//        LoggingHelper.LogError( ex, thisClassName + ".GetAll( Guid parentUid, string alignmentType )" );
		//    }
		//    return list;
		//}//

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
                    DBEntity item = context.Entity_Competency
                            .FirstOrDefault( s => s.Id == profileId );

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

        public static CassCompetencyV2 GetAsCassCompetency( int profileId )
        {
            CassCompetencyV2 entity = new CassCompetencyV2();
            if ( profileId == 0 )
                return entity;
            try
            {
                using ( var context = new Data.CTIEntities() )
                {
                    DBEntity item = context.Entity_Competency
                            .FirstOrDefault( s => s.Id == profileId );

                    if ( item != null && item.Id > 0 )
                    {
                        MapFromDB( item,entity );
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex,thisClassName + ".GetAsCassCompetency" );
            }
            return entity;
        }//

        /// <summary>
        /// Get entity to determine if one exists for the entity and alignment type
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="competencyId"></param>
        /// <returns></returns>
        public static ThisEntity Get( int entityId, int competencyId )
        {
            ThisEntity entity = new ThisEntity();
            if ( competencyId == 0 )
                return entity;
            try
            {
                using ( var context = new Data.CTIEntities() )
                {
                    DBEntity item = context.Entity_Competency
                            .FirstOrDefault( s => s.EntityId == entityId && s.CompetencyId == competencyId );

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
        public static void MapFromDB( DBEntity from, CassCompetencyV2 to )
        {
            to.Id = from.Id;
            to.EntityId = from.EntityId;
            to.CompetencyId = from.CompetencyId;
            if ( IsValidDate( from.Created ) )
                to.Created = ( DateTime ) from.Created;
            to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;

			to.Name = from.EducationFramework_Competency.Name;
            to.Description = from.EducationFramework_Competency.Description;
            to.CodedNotation = from.EducationFramework_Competency.CodedNotation;
            to.FrameworkName = from.EducationFramework_Competency.EducationFramework.Name;
			to.FrameworkUri = from.EducationFramework_Competency.EducationFramework.RepositoryUri;

			to.Uri = from.EducationFramework_Competency.RepositoryUri;
			to.CTID = from.EducationFramework_Competency.CTID;
			to.AssociationId = from.Id;
        }

        public static void MapFromDB( DBEntity from, ThisEntity to )
        {
            to.Id = from.Id;
            to.EntityId = from.EntityId;

            to.CompetencyId = from.CompetencyId;


            //to.AlignmentTypeId = from.AlignmentTypeId ?? 0;
            //to.AlignmentType = from.AlignmentType;


            if ( IsValidDate( from.Created ) )
                to.Created = ( DateTime ) from.Created;
            to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;

            EducationFrameworkManager.MapFromDB( from.EducationFramework_Competency, to.FrameworkCompetency );

            EducationFrameworkManager.MapFromDB( from.EducationFramework_Competency.EducationFramework, to.FrameworkCompetency.EducationFramework );

        }

        public static void MapFromDBAsAlignmentObjects( DBEntity from, CredentialAlignmentObjectItemProfile to )
        {
            to.Id = from.Id;
            to.ParentId = from.EntityId;

            to.CompetencyId = from.CompetencyId;


            //to.AlignmentTypeId = from.AlignmentTypeId ?? 0;
            //to.AlignmentType = from.AlignmentType;


            if ( IsValidDate( from.Created ) )
                to.Created = ( DateTime ) from.Created;
            to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( from.EducationFramework_Competency != null && from.EducationFramework_Competency.EducationFramework != null)
				to.EducationalFrameworkName = from.EducationFramework_Competency.EducationFramework.Name ?? "missing";

			to.TargetNodeName = ConvertSpecialCharacters( StripJqueryTag( from.EducationFramework_Competency.Name ));
            to.TargetNode = from.EducationFramework_Competency.Url;
            to.RepositoryUri = from.EducationFramework_Competency.RepositoryUri;
            to.Description = from.EducationFramework_Competency.Description;
            to.CodedNotation = from.EducationFramework_Competency.CodedNotation;
        }
		//

		public static List<ThisEntityItem> Search( string pFilter, string pOrderBy, int pageNumber, int pageSize, ref int pTotalRows )
		{
			string connectionString = DBConnectionRO();
			ThisEntityItem item = new ThisEntityItem();
			List<ThisEntityItem> list = new List<ThisEntityItem>();
			var result = new DataTable();

			using ( SqlConnection c = new SqlConnection( connectionString ) )
			{
				c.Open();

				if ( string.IsNullOrEmpty( pFilter ) )
				{
					pFilter = "";
				}
				
				using ( SqlCommand command = new SqlCommand( "[Competencies_search]", c ) )
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
						if ( pTotalRows > 0 && result.Rows.Count == 0 )
						{
							item = new ThisEntityItem();
							item.TargetNodeName = "Error: invalid page number. Select displayed page button only. ";
							item.Description = "Error: invalid page number. Select displayed page button only.";
							list.Add( item );
							return list;
						}
					}
					catch ( Exception ex )
					{
						pTotalRows = 0;
						LoggingHelper.LogError( ex, thisClassName + string.Format( ".Search() - Execute proc, Message: {0} \r\n Filter: {1} \r\n", ex.Message, pFilter ) );

						item = new ThisEntityItem();
						item.TargetNodeName = "Unexpected error encountered. System administration has been notified. Please try again later. ";
						item.Description = ex.Message;
						list.Add( item );
						return list;
					}
				}
				
				foreach ( DataRow dr in result.Rows )
				{
					item = new ThisEntityItem();
					item.EducationalFrameworkName = GetRowColumn( dr, "EducationalFrameworkName", "???" );
					item.Id = GetRowColumn( dr, "CompetencyFrameworkItemId", 0 );
					item.TargetNodeName = GetRowColumn( dr, "Competency", "???" );
					//item.ProfileName = GetRowPossibleColumn( dr, "Competency2", "???" );
					item.Description = GetRowColumn( dr, "Description", "" );

					//don't include credentialId, as will work with source of the search will often be for a credential./ Same for condition profiles for now. 
					item.SourceParentId = GetRowColumn( dr, "SourceId", 0 );
					item.SourceEntityTypeId = GetRowColumn( dr, "SourceEntityTypeId", 0 );
					//item.AlignmentTypeId = GetRowColumn( dr, "AlignmentTypeId", 0 );
					item.AlignmentType = GetRowColumn( dr, "AlignmentType", "" );
					//Although the condition profile type may be significant?
					item.ConnectionTypeId = GetRowColumn( dr, "ConnectionTypeId", 0 );

					list.Add( item );
				}

				return list;

			}
		} //

		#endregion

		#region Code to export old competency format for import to CaSS
		/// <summary>
		/// NOTE: the view: Entity_Competencies_ForExport was created to use as a direct export to csv. 
		/// </summary>
		/// <returns></returns>
		public static List<Data.Views.Entity_Competencies_ForExport> ExportAllCTDLASNCompetencies()
        {
            var list = new List<Data.Views.Entity_Competencies_ForExport>();
            try
            {
                using ( var context = new ViewContext() )
                {
                    list = context.Entity_Competencies_ForExport
                            .ToList();
                }

                //Temporary(?) fix for competency text
                foreach ( var item in list )
                {
                    if ( string.IsNullOrWhiteSpace( item.Description ) )
                    {
                        item.Description = "";
                    }
                    else if ( item.Description == "''" )
                    {
                        item.Description = "";
                    }
                    else if ( item.Description[ 0 ] == '\'' && item.Description[ item.Description.Length - 1 ] == '\'' )
                    {
                        item.Description = item.Description.Substring( 1, item.Description.Length - 2 );
                    }
                }
                //End temporary(?) fix
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ExportAllCTDLASNCompetencies( Guid parentUid, string alignmentType )" );
            }

            return list;
        }//

        public static List<Data.Views.Entity_Competencies_Summary> ExportAllCompetenciesAsCTDLASN( bool requireHasApproval = false)
        {
            var list2 = new List<Data.Views.Entity_Competencies_Summary>();
            var list = new List<Data.Views.Entity_Competencies_Summary>();
            //bool requireHasApproval = UtilityManager.GetAppKeyValue( "requireHasApprovalForCompentencyExport", false );
            try
            {
                using (var context = new ViewContext())
                {
                    list2 = context.Entity_Competencies_Summary
                        .Where( s => s.IsPublished == "yes" && ( requireHasApproval == false || s.HasApproval == "yes"))
                        .OrderBy( s => s.EntityTypeId)
                        .ThenBy( s => s.BaseId)
                        .ThenBy(s => s.EducationalFrameworkName)
                        .ThenBy(s => s.CompetencyFrameworkItemId )
                        .ToList();

                    list = context.Entity_Competencies_Summary
                        .OrderBy( s => s.EntityTypeId )
                        .ThenBy( s => s.BaseId )
                        .ThenBy( s => s.EducationalFrameworkName )
                        .ThenBy( s => s.CompetencyFrameworkItemId )
                        .ToList();
                }

                //Temporary(?) fix for competency text
                //may not be necessary for this view
                foreach (var item in list)
                {
                    item.CompetencyCtid = item.CompetencyCtid.ToLower();
                    item.FrameworkCtid = item.FrameworkCtid.ToLower();
                    if (string.IsNullOrWhiteSpace(item.Description))
                    {
                        item.Description = "";
                    }
                    else if (item.Description == "''")
                    {
                        item.Description = "";
                    }
                    else if (item.Description[0] == '\'' && item.Description[item.Description.Length - 1] == '\'')
                    {
                        item.Description = item.Description.Substring(1, item.Description.Length - 2);
                    }
                }
                //End temporary(?) fix
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(ex, thisClassName + ".ExportAllCompetenciesAsCTDLASN( Guid parentUid, string alignmentType )");
            }

            return list;
        }//


        public static List<Data.Views.Entity_ConditionProfileCompetencies_Summary> ExportAllConditonProfileCompetenciesAsCTDLASN( bool requireHasApproval = false )
        {
            var list = new List<Data.Views.Entity_Competencies_Summary>();
            Data.Views.Entity_Competencies_Summary entity = new Data.Views.Entity_Competencies_Summary();
            var results = new List<Data.Views.Entity_ConditionProfileCompetencies_Summary>();
            //bool requireHasApproval = UtilityManager.GetAppKeyValue( "requireHasApprovalForCompentencyExport", false );
            try
            {
                using ( var context = new ViewContext() )
                {

                    results = context.Entity_ConditionProfileCompetencies_Summary
                        .OrderBy( s => s.EntityTypeId )
                        .ThenBy( s => s.BaseId )
                        .ThenBy( s => s.EducationalFrameworkName )
                        .ThenBy( s => s.CompetencyFrameworkItemId )
                        .ToList();
                }

                //Temporary(?) fix for competency text
                //may not be necessary for this view
                foreach ( var item in results )
                {
                    entity = new Data.Views.Entity_Competencies_Summary();
                    entity.Id = item.Id;
                    entity.EntityType = item.EntityType;
                    entity.EntityTypeId = item.EntityTypeId;
                    entity.BaseId = item.BaseId;
                    entity.ParentEntityName = item.ParentEntityName;
                    entity.OwningOrganization = item.OwningOrganization;
                    entity.OwningOrgId = item.OwningOrgId;
                    entity.orgUid = item.orgUid;
                    entity.OrganizationCtid = item.OrganizationCtid;
                    entity.CompetencyFrameworkId = item.CompetencyFrameworkId;
                    entity.NumberOfFrameworks = item.NumberOfFrameworks;
                    entity.RecommendedFrameworkName = item.RecommendedFrameworkName;
                    entity.EducationalFrameworkName = item.EducationalFrameworkName;
                    entity.FrameworkCtid = item.FrameworkCtid.ToLower();
                    entity.EducationalFrameworkUrl = item.EducationalFrameworkUrl;
                    entity.AlignmentType = item.AlignmentType;
                    entity.CompetencyFrameworkId = item.CompetencyFrameworkId;
                    entity.Competency = item.Competency;
                    entity.Description = item.Description;
                    entity.TargetName = item.TargetName;
                    entity.TargetDescription = item.TargetDescription;
                    entity.TargetUrl = item.TargetUrl;
                    entity.CodedNotation = item.CodedNotation;
                    entity.AlignmentDate = item.AlignmentDate;
                    entity.CompetencyCtid = item.CompetencyCtid.ToLower();
                    entity.FrameworkUid = item.FrameworkUid;
                    entity.CompetencyUid = item.CompetencyUid;
                    entity.CompetencyCreated = item.CompetencyCreated;
                    entity.CompetencyCreatedById = item.CompetencyCreatedById;
                    entity.IsPublished = item.IsPublished;
                    entity.HasApproval = item.HasApproval;
                    entity.CompetencyPEMKey = item.CompetencyPEMKey;
                    entity.CompetencyLastUpdated = item.CompetencyLastUpdated;
                    entity.EntityId = item.EntityId;
                    entity.ExportDate = item.ExportDate;
                    entity.MergedWithFrameworkCtid = item.MergedWithFrameworkCtid;

                    if ( string.IsNullOrWhiteSpace( entity.Description ) )
                    {
                        entity.Description = "";
                    }
                    else if ( entity.Description == "''" )
                    {
                        entity.Description = "";
                    }
                    else if ( entity.Description[ 0 ] == '\'' && entity.Description[ entity.Description.Length - 1 ] == '\'' )
                    {
                        entity.Description = entity.Description.Substring( 1, entity.Description.Length - 2 );
                    }

                    list.Add( entity );
                }
                //End temporary(?) fix
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ExportAllConditonProfileCompetenciesAsCTDLASN( Guid parentUid, string alignmentType )" );
            }

            return results;
        }//
        #endregion

    }
}
