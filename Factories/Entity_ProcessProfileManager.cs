using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBEntity = Data.Entity_ProcessProfile;
using ThisEntity = Models.ProfileModels.ProcessProfile;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;

namespace Factories
{
	public class Entity_ProcessProfileManager : BaseFactory
	{
		static string thisClassName = "Entity_ProcessProfileManager";
        public static bool enforcingMinimumData = UtilityManager.GetAppKeyValue( "enforcingProcessProfileMinimumData", false );

        public static int DEFAULT_PROCESS_TYPE = 1;
		public static int ADMIN_PROCESS_TYPE = 1;
		public static int DEV_PROCESS_TYPE = 2; //convert to 2 from 7
		public static int MTCE_PROCESS_TYPE = 3; //convert to 3 from 8

		public static int APPEAL_PROCESS_TYPE = 4;	//to 4 from 2
		public static int COMPLAINT_PROCESS_TYPE = 5;	//to 5 from 3
        public static int REVIEW_PROCESS_TYPE = 7;
        public static int REVOKE_PROCESS_TYPE = 8;

        [Obsolete]
		public static int CRITERIA_PROCESS_TYPE = 6;
		




		#region Entity Persistance ===================
		/// <summary>
		/// Persist ProcessProfile
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

			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}


			//determine type
			int profileTypeId = 0;
			if ( entity.ProcessTypeId > 0 )
				profileTypeId = entity.ProcessTypeId;
			else
			{
				//
				switch ( entity.ProcessProfileType )
				{
					case "AppealProcess":
						entity.ProcessTypeId = APPEAL_PROCESS_TYPE;
						break;
					case "ComplaintProcess":
						entity.ProcessTypeId = COMPLAINT_PROCESS_TYPE;
						break;
					//case "CriteriaProcess":
					//	entity.ProcessTypeId = CRITERIA_PROCESS_TYPE;
					//	break;

					case "ReviewProcess":
						entity.ProcessTypeId = REVIEW_PROCESS_TYPE;
						break;

					case "RevocationProcess":
						entity.ProcessTypeId = REVOKE_PROCESS_TYPE;
						break;

					case "ProcessProfile":
						entity.ProcessTypeId = DEFAULT_PROCESS_TYPE;
						break;

					case "CredentialProcess":
						entity.ProcessTypeId = DEFAULT_PROCESS_TYPE;
						break;

					case "MaintenanceProcess":
						entity.ProcessTypeId = MTCE_PROCESS_TYPE;
						break;

					case "AdministrationProcess":
						entity.ProcessTypeId = ADMIN_PROCESS_TYPE;
						break;

					case "DevelopmentProcess":
						entity.ProcessTypeId = DEV_PROCESS_TYPE;
						break;
					//
					default:
						entity.ProcessTypeId = 1;
						messages.Add( string.Format( "Error: Unexpected profile type of {0} was encountered.", entity.ProcessProfileType ) );
						return false;
				}
			}

            try
            {
                using (var context = new Data.CTIEntities())
                {

                    bool isEmpty = false;

                    if (ValidateProfile( entity, ref isEmpty, ref messages ) == false)
                    {
                        return false;
                    }
                    if (isEmpty)
                    {
                        messages.Add( "The Process Profile is empty. " + SetEntitySummary( entity ) );
                        return false;
                    }

                    if (entity.Id == 0)
                    {
                        //add
                        efEntity = new DBEntity();
                        MapToDB( entity, efEntity );
                        efEntity.EntityId = parent.Id;

                        efEntity.Created = efEntity.LastUpdated = DateTime.Now;
                        efEntity.CreatedById = efEntity.LastUpdatedById = userId;
                        efEntity.RowId = Guid.NewGuid();

                        context.Entity_ProcessProfile.Add( efEntity );
                        count = context.SaveChanges();
                        //update profile record so doesn't get deleted
                        entity.Id = efEntity.Id;
                        entity.ParentId = parent.Id;
                        entity.RowId = efEntity.RowId;
                        if (count == 0)
                        {
                            messages.Add( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.ProfileName ) ? "no description" : entity.ProfileName ) );
                        }
                        else
                        {
                            //other entity components use a trigger to create the entity Object. If a trigger is not created, then child adds will fail (as typically use entity_summary to get the parent. As the latter is easy, make the direct call?

                            UpdateParts( entity, userId, ref messages );
                        }
                    }
                    else
                    {
                        entity.ParentId = parent.Id;

                        efEntity = context.Entity_ProcessProfile.SingleOrDefault( s => s.Id == entity.Id );
                        if (efEntity != null && efEntity.Id > 0)
                        {
                            entity.RowId = efEntity.RowId;
                            //update
                            MapToDB( entity, efEntity );
                            //has changed?
                            if (HasStateChanged( context ))
                            {
                                efEntity.LastUpdated = System.DateTime.Now;
                                efEntity.LastUpdatedById = userId;

                                count = context.SaveChanges();
                            }
                            //always check parts
                            UpdateParts( entity, userId, ref messages );
                        }

                    }


                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbex)
            {
                string message = HandleDBValidationError( dbex, thisClassName + ".Save() ", "Entity.ProcessProfile" );
                LoggingHelper.LogError( dbex, thisClassName + string.Format( ".Save(). parent.EntityBaseId: {0}, EntityTypeId: {1}, userId: {2}", parent.EntityBaseId, parent.EntityTypeId, userId ) + message, true );
                messages.Add( "Error - the save was not successful. <br/> " + message);

            }
            catch (Exception ex)
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save(). parent.EntityBaseId: {0}, EntityTypeId: {1}, userId: {2}", parent.EntityBaseId, parent.EntityTypeId, userId) );
                messages.Add( ex.Message );
                isValid = false;
            }
			return isValid;
		}

		private bool UpdateParts( ThisEntity entity, int userId, ref List<string> messages )
		{
			bool isAllValid = true;

			EntityPropertyManager mgr = new EntityPropertyManager();
			if ( mgr.UpdateProperties( entity.ExternalInput, entity.RowId, CodesManager.ENTITY_TYPE_PROCESS_PROFILE, CodesManager.PROPERTY_CATEGORY_EXTERNAL_INPUT_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			//if ( mgr.UpdateProperties( entity.ProcessMethod, entity.RowId, CodesManager.ENTITY_TYPE_PROCESS_PROFILE, CodesManager.PROPERTY_CATEGORY_PROCESS_METHOD, entity.LastUpdatedById, ref messages ) == false )
			//	isAllValid = false;

			//if ( mgr.UpdateProperties( entity.StaffEvaluationMethod, entity.RowId, CodesManager.ENTITY_TYPE_PROCESS_PROFILE, CodesManager.PROPERTY_CATEGORY_STAFF_EVALUATION_METHOD, entity.LastUpdatedById, ref messages ) == false )
			//	isAllValid = false;



			return isAllValid;
		}

		public bool Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity p = context.Entity_ProcessProfile.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_ProcessProfile.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "Process Profile record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;

		}


		public bool ValidateProfile( ThisEntity profile, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;
			int count = messages.Count;
			isEmpty = false;
            if (profile.IsStarterProfile)
                return true;

            //check if empty ==> as precreated, won't be empty other than potential required fields
            //  ignore:
            //&& string.IsNullOrWhiteSpace( profile.DateEffective ) 
            //                && ( profile.ProcessingAgentUid == null || profile.ProcessingAgentUid.ToString() == DEFAULT_GUID )
            //actually description should be required.
            if (string.IsNullOrWhiteSpace( profile.Description )
                && string.IsNullOrWhiteSpace( profile.ProcessStandards )
                && string.IsNullOrWhiteSpace( profile.ProcessStandardsDescription )
                && string.IsNullOrWhiteSpace( profile.ScoringMethodExample )
                && string.IsNullOrWhiteSpace( profile.ScoringMethodExampleDescription )
                && string.IsNullOrWhiteSpace( profile.SubjectWebpage )
                && string.IsNullOrWhiteSpace( profile.ProcessFrequency )
                && string.IsNullOrWhiteSpace( profile.ProcessMethodDescription )
                && string.IsNullOrWhiteSpace( profile.VerificationMethodDescription )
                )
            {
                //isEmpty = true;
                if (enforcingMinimumData)
                {
                    //need to check for any selected targets, or a jurisdiction - actual the latter is useless without other data.
                    messages.Add( "The Process profile does not contain a useful amount of data." );
                    return false;
                }
            }


			//should be something else
			if ( !IsUrlValid( profile.ProcessMethod, ref commonStatusMessage ) )
			{
				messages.Add( "The Process Method Url is invalid. " + commonStatusMessage );
			}
			if ( !IsUrlValid( profile.ProcessStandards, ref commonStatusMessage ) )
			{
				messages.Add( "The Process Standards Url is invalid. " + commonStatusMessage );
			}
			if ( !IsUrlValid( profile.ScoringMethodExample, ref commonStatusMessage ) )
			{
				messages.Add( "The Scoring Method Example Url is invalid. " + commonStatusMessage );
			}
			if ( !IsUrlValid( profile.SubjectWebpage, ref commonStatusMessage ) )
				messages.Add( "The Subject Webpage is invalid. " + commonStatusMessage );


				if ( messages.Count > count )
				isValid = false;

			return isValid;
		}
		
		#endregion
		#region  retrieval ==================

		/// <summary>
		/// Get all ProcessProfile for the parent
		/// Uses the parent Guid to retrieve the related Entity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		public static List<ThisEntity> GetAll( Guid parentUid, bool getForProfileLink )
		{
			ThisEntity entity = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBEntity> results = context.Entity_ProcessProfile
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBEntity item in results )
						{
							entity = new ThisEntity();
							MapFromDB( item, entity, true, getForProfileLink );
							list.Add( entity );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".GetAll" );
			}
			return list;
		}//

		public static ThisEntity Get( int profileId )
		{
			ThisEntity entity = new ThisEntity();

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBEntity item = context.Entity_ProcessProfile
							.SingleOrDefault( s => s.Id == profileId );

					if ( item != null && item.Id > 0 )
					{
						MapFromDB( item, entity, true, false );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Get" );
			}
			return entity;
		}//

		public static void MapToDB( ThisEntity from, DBEntity to )
		{
			//want to ensure fields from create are not wiped
			if ( to.Id == 0 )
			{
				to.ProcessTypeId = from.ProcessTypeId;
			}
			to.Id = from.Id;
			
			to.ProfileName = from.ProcessProfileType;
			to.Description = from.Description;
			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = DateTime.Parse( from.DateEffective );
			else //handle reset
				to.DateEffective = null;

			if ( from.ProcessingAgentUid == null || from.ProcessingAgentUid.ToString() == DEFAULT_GUID )
			{
				to.ProcessingAgentUid = null;//			
			}
			else
			{
				to.ProcessingAgentUid = from.ProcessingAgentUid;
			}

			to.SubjectWebpage = NormalizeUrlData( from.SubjectWebpage );

			to.ProcessFrequency = from.ProcessFrequency;
			//to.TargetCompetencyFramework = from.TargetCompetencyFramework;

			to.ProcessMethod = from.ProcessMethod;
			to.ProcessMethodDescription = from.ProcessMethodDescription;

			to.ProcessStandards = from.ProcessStandards;
			to.ProcessStandardsDescription = from.ProcessStandardsDescription;

			to.ScoringMethodDescription = from.ScoringMethodDescription;
			to.ScoringMethodExample = from.ScoringMethodExample;
			to.ScoringMethodExampleDescription = from.ScoringMethodExampleDescription;
			to.VerificationMethodDescription = from.VerificationMethodDescription;

			//to.DecisionInformationUrl = from.DecisionInformationUrl;
			//to.OfferedByDirectoryUrl = from.OfferedByDirectoryUrl;
			//to.PublicInformationUrl = from.PublicInformationUrl;
			//to.StaffEvaluationUrl = from.StaffEvaluationUrl;
			//to.OutcomeReviewUrl = from.OutcomeReviewUrl;
			//to.PoliciesAndProceduresUrl = from.PoliciesAndProceduresUrl;
			//to.ProcessCriteriaUrl = from.ProcessCriteriaUrl;
			//to.ProcessCriteriaValidationUrl = from.ProcessCriteriaValidationUrl;
			//to.StaffSelectionCriteriaUrl = from.StaffSelectionCriteriaUrl;
			

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = DateTime.Parse( from.DateEffective );
			else
				to.DateEffective = null;


		}
		public static void MapFromDB( DBEntity from, ThisEntity to, bool includingItems, bool getForProfileLink )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
            to.ParentId = from.EntityId;

			//HANDLE PROCESS TYPES
			if ( from.ProcessTypeId != null && ( int ) from.ProcessTypeId > 0)
				to.ProcessTypeId = ( int ) from.ProcessTypeId;
			else
				to.ProcessTypeId = 1;
            //need to distinguish if for detail
            to.ProcessProfileType = GetProfileType( to.ProcessTypeId, getForProfileLink );
			to.ProfileName = GetProfileType( to.ProcessTypeId );
			
			to.Description = from.Description;
			if ( ( to.Description ?? "" ).Length > 5 )
			{
				to.ProfileName = to.Description.Length > 100 ? to.Description.Substring(0,100) + " . . ." : to.Description;
			}

			if ( from.Entity != null )
				to.ParentId = from.Entity.Id;

			//- provide minimum option, for lists
			if ( getForProfileLink )
				return;

			if ( IsGuidValid( from.ProcessingAgentUid ) )
			{
				to.ProcessingAgentUid = ( Guid ) from.ProcessingAgentUid;

				//TODO - remove use of ProcessingAgentProfileLink and replace with ProcessingAgent
				to.ProcessingAgentProfileLink = OrganizationManager.Agent_GetProfileLink( to.ProcessingAgentUid );
				to.ProcessingAgent = OrganizationManager.GetForSummary( to.ProcessingAgentUid );
			}

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = ( ( DateTime ) from.DateEffective ).ToShortDateString();
			else
				to.DateEffective = "";
			to.SubjectWebpage = from.SubjectWebpage;

			to.ProcessFrequency = from.ProcessFrequency;
			//will only have required!
			//to.RequiresCompetenciesFrameworks = Entity_CompetencyFrameworkManager.GetAll( to.RowId, "requires" );
            to.RequiresCompetenciesFrameworks = Entity_CompetencyManager.GetAll( to.RowId, "requires" );

            to.ProcessMethod = from.ProcessMethod;
			to.ProcessMethodDescription = from.ProcessMethodDescription;

			to.ProcessStandards = from.ProcessStandards;
			to.ProcessStandardsDescription = from.ProcessStandardsDescription;

			to.ScoringMethodDescription = from.ScoringMethodDescription;
			to.ScoringMethodExample = from.ScoringMethodExample;
			to.ScoringMethodExampleDescription = from.ScoringMethodExampleDescription;
			to.VerificationMethodDescription = from.VerificationMethodDescription;

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = ( ( DateTime ) from.DateEffective ).ToShortDateString();
			else
				to.DateEffective = "";

			//enumerations
			to.ExternalInput = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_EXTERNAL_INPUT_TYPE );
			//to.StaffEvaluationMethod = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_STAFF_EVALUATION_METHOD );
			//to.ProcessMethod = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_PROCESS_METHOD );

			to.ProfileSummary = SetEntitySummary( to );


			if ( includingItems )
			{
				
				to.Jurisdiction = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId );
				//will only be one, but could model with multiple
				to.TargetCredential = Entity_CredentialManager.GetAll( to.RowId );
				to.TargetAssessment = Entity_AssessmentManager.GetAll( to.RowId, false, false );

				to.TargetLearningOpportunity = Entity_LearningOpportunityManager.LearningOpps_GetAll( to.RowId, false, true );
			}


			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;
			to.LastUpdatedBy = SetLastUpdatedBy( to.LastUpdatedById, from.Account_Modifier );
		}
		static string SetEntitySummary( ThisEntity to )
		{
			string summary = "Process Profile ";
            string processType = GetProfileType( to.ProcessTypeId);

            summary = processType + ( string.IsNullOrWhiteSpace( to.ProfileName ) ? "Process Profile" : to.ProfileName );

			if ( !string.IsNullOrWhiteSpace( to.ProfileName ) )
			{
				return to.ProfileName;
			}

			if ( to.Id > 1 )
			{
				summary += to.Id.ToString();
			}
			return summary;
        }
        static string GetProfileType( int processTypeId, bool returningLabelFormat  = true )
        {
            switch (processTypeId)
            {
                case 1:
                    return returningLabelFormat ? "AdministrationProcess" : "Administration Process";
                case 2:
                    return returningLabelFormat ? "DevelopmentProcess" : "Development Process";
                case 3:
                    return returningLabelFormat ? "MaintenanceProcess" : "Maintenance Process";
                case 4:
                    return returningLabelFormat ? "AppealProcess" : "Appeal Process";
                case 5:
                    return returningLabelFormat ? "ComplaintProcess" : "Complaint Process";
                case 7:
                    return returningLabelFormat ? "ReviewProcess" : "Review Process";
                case 8:
                    return returningLabelFormat ? "RevocationProcess" : "Revocation Process";

                default:
                    return returningLabelFormat ? "ProcessProfile" : "Process Process";
            }
        }
        #endregion

            #region  validations
        public static bool IsParentBeingAddedAsChildToItself( int profileId, int childId, int childEntityTypeId )
		{
			bool isOk = false;
			using ( var context = new Data.CTIEntities() )
			{
				//get the profile that is the parent of the child
				DBEntity efEntity = context.Entity_ProcessProfile
						.SingleOrDefault( s => s.Id == profileId );

				if ( efEntity != null 
					&& efEntity.Id > 0 
					&& efEntity.Entity != null )
				{
					//check if the parent entity is the same one being added as a child
					if ( efEntity.Entity.EntityTypeId == childEntityTypeId
						&& efEntity.Entity.EntityBaseId == childId )
					{
						return true;
					}
				}
			}

			return isOk;
		}

		#endregion
	}
}
