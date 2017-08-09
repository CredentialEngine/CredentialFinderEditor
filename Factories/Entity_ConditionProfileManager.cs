using System;
using System.Collections.Generic;
using System.Linq;

using Models.ProfileModels;
using Models.Common;
using MN = Models.Node;
using EM = Data;
using Utilities;

using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
using DBentity = Data.Entity_ConditionProfile;
using ThisEntity = Models.ProfileModels.ConditionProfile;

namespace Factories
{
	public class Entity_ConditionProfileManager : BaseFactory
	{
		static string thisClassName = "Entity_ConditionProfileManager";
		public static int ConnectionProfileType_Requirement = 1;
		public static int ConnectionProfileType_Recommendation = 2;
		public static int ConnectionProfileType_NextIsRequiredFor = 3;
		public static int ConnectionProfileType_NextIsRecommendedFor = 4;
		public static int ConnectionProfileType_Renewal = 5;
		public static int ConnectionProfileType_AdvancedStandingFor = 6;
		public static int ConnectionProfileType_AdvancedStandingFrom = 7;
		public static int ConnectionProfileType_PreparationFor = 8;
		public static int ConnectionProfileType_PreparationFrom = 9;
		public static int ConnectionProfileType_Corequisite = 10;
		public static int ConnectionProfileType_EntryCondition = 11;

		public static int ConditionSubType_Basic = 1;
		public static int ConditionSubType_CredentialConnection = 2;
		public static int ConditionSubType_Assessment = 3;
		public static int ConditionSubType_LearningOpportunity = 4;
		public static int ConditionSubType_Alternative = 5;
		public static int ConditionSubType_Additional = 6;
		#region persistance ==================

		
		public bool Save( ConditionProfile item, Guid parentUid, int userId, ref List<string> messages )
		{
			bool isValid = true;
			
			if ( !IsValidGuid( parentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			item.ParentId = parent.Id;

			int profileTypeId = 0;
			if ( item.ConnectionProfileTypeId > 0 )
				profileTypeId = item.ConnectionProfileTypeId;
			else
			{
				//
				switch ( item.ConnectionProfileType.ToLower() )
				{
					case "requires":
						profileTypeId = ConnectionProfileType_Requirement;
						break;
					case "alternativecondition": //NO - can have different types OR ??
						profileTypeId = ConnectionProfileType_Requirement;
						break;
					case "additionalcondition": //NO - can have different types OR ??
						profileTypeId = ConnectionProfileType_Requirement;
						break;
					case "recommends":
						profileTypeId = ConnectionProfileType_Recommendation;
						break;

					case "isrequiredfor":
						profileTypeId = ConnectionProfileType_NextIsRequiredFor;
						break;

					case "isrecommendedfor":
						profileTypeId = ConnectionProfileType_NextIsRecommendedFor;
						break;

					case "renewal":
						profileTypeId = ConnectionProfileType_Renewal;
						break;
					case "advancedstandingfor":
						profileTypeId = ConnectionProfileType_AdvancedStandingFor;
						break;
					case "advancedstandingfrom":
						profileTypeId = ConnectionProfileType_AdvancedStandingFrom;
						break;
					case "ispreparationfor":
						profileTypeId = ConnectionProfileType_PreparationFor;
						break;
					case "preparationfrom":
						profileTypeId = ConnectionProfileType_PreparationFrom;
						break;
					case "corequisite":
						profileTypeId = Entity_ConditionProfileManager.ConnectionProfileType_Corequisite;
						break;
					case "entrycondition":
						profileTypeId = Entity_ConditionProfileManager.ConnectionProfileType_EntryCondition;
						break;
					//
					default:
						
						if ( item.IsStarterProfile == false )
						{
							profileTypeId = 0;
							messages.Add( "Error: The Condition Profile type is missing." );
							return false;
						}
						profileTypeId = 1;
						break;
				}
			}

			using (var context = new Data.CTIEntities())
			{
				if (!ValidateProfile(item, ref messages))
				{
					return false;
				}

				item.CreatedById = item.LastUpdatedById = userId;
				item.ConnectionProfileTypeId = profileTypeId;
				if (item.Id > 0)
				{
					DBentity p = context.Entity_ConditionProfile
							.FirstOrDefault(s => s.Id == item.Id);
					if (p != null && p.Id > 0)
					{
						item.RowId = p.RowId;
						FromMap(item, p);

						if (HasStateChanged(context))
						{
							p.LastUpdated = System.DateTime.Now;
							p.LastUpdatedById = userId;
							context.SaveChanges();
						}
						//regardless, check parts
						isValid = UpdateParts( item, ref messages );
					}
					else
					{
						//error should have been found
						isValid = false;
						messages.Add(string.Format("Error: the requested record was not found: recordId: {0}", item.Id));
					}
				}
				else
				{
					int newId = Add( item, userId, ref messages );
					if ( newId == 0 || messages.Count > 0)
						isValid = false;
				}
			}
			return isValid;
		}


		/// <summary>
		/// add a ConditionProfile
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		private int Add(ThisEntity entity, int userId, ref List<String> messages)
		{
			DBentity efEntity = new DBentity();
			using (var context = new Data.CTIEntities())
			{
				try
				{
					FromMap(entity, efEntity);

					efEntity.EntityId = entity.ParentId;
					efEntity.RowId = Guid.NewGuid();
					efEntity.CreatedById = efEntity.LastUpdatedById = userId;
					efEntity.Created = efEntity.LastUpdated = System.DateTime.Now;
					
					context.Entity_ConditionProfile.Add(efEntity);

					// submit the change to database
					int count = context.SaveChanges();
					if (count > 0)
					{
						entity.Id = efEntity.Id;
						entity.RowId = efEntity.RowId;

						UpdateParts(entity, ref messages);

						return efEntity.Id;
					}
					else
					{
						//?no info on error
						messages.Add("Error - the profile was not saved. ");
						string message = string.Format("{0}.Add() Failed", "Attempted to add a ConditionProfile. The process appeared to not work, but was not an exception, so we have no message, or no clue. ConditionProfile. EntityId: {1}, createdById: {2}", thisClassName, entity.ParentId, entity.CreatedById);
						EmailManager.NotifyAdmin(thisClassName + ".Add() Failed", message);
					}
				}
				catch (System.Data.Entity.Validation.DbEntityValidationException dbex)
				{
					string message = HandleDBValidationError( dbex, "Entity_ConditionProfileManager.Add()", string.Format( "EntityId: 0 , CostTypeId: {1}  ", entity.ParentId, entity.ConnectionProfileTypeId ) );
					messages.Add( message );
					
					//string message = thisClassName + string.Format(".Add() DbEntityValidationException, EntityId: {0}", entity.ParentId);
					//foreach (var eve in dbex.EntityValidationErrors)
					//{
					//	message += string.Format("\rEntity of type \"{0}\" in state \"{1}\" has the following validation errors:",
					//		eve.Entry.Entity.GetType().Name, eve.Entry.State);
					//	foreach (var ve in eve.ValidationErrors)
					//	{
					//		message += string.Format("- Property: \"{0}\", Error: \"{1}\"",
					//			ve.PropertyName, ve.ErrorMessage);
					//	}

					//	LoggingHelper.LogError(message, true);
				}
				catch (Exception ex)
				{
					LoggingHelper.LogError(ex, thisClassName + string.Format(".Add(), EntityId: {0}", entity.ParentId));
				}
			}

			return efEntity.Id;
		}

		public bool UpdateParts(ThisEntity entity, ref List<String> messages)
		{
			bool isAllValid = true;

			EntityPropertyManager mgr = new EntityPropertyManager();
			if ( mgr.UpdateProperties( entity.AudienceLevel, entity.RowId, CodesManager.ENTITY_TYPE_CONDITION_PROFILE, CodesManager.PROPERTY_CATEGORY_AUDIENCE_LEVEL, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			if ( mgr.UpdateProperties( entity.ApplicableAudienceType, entity.RowId, CodesManager.ENTITY_TYPE_CONDITION_PROFILE, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			//Asmts
			//N/A AS done immediately
			Entity_ReferenceManager erm = new Entity_ReferenceManager();

			//if (erm.Entity_Reference_Update(entity.ReferenceUrl, entity.RowId, CodesManager.ENTITY_TYPE_CONDITION_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS, false) == false)
			//	isAllValid = false;

			if (erm.Entity_Reference_Update(entity.Condition, entity.RowId, CodesManager.ENTITY_TYPE_CONDITION_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_CONDITION_ITEM, false) == false)
				isAllValid = false;

			if ( erm.Entity_Reference_Update( entity.SubmissionOf, entity.RowId, CodesManager.ENTITY_TYPE_CONDITION_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_SUBMISSION_ITEM, false ) == false )
				isAllValid = false;

			//
			return isAllValid;
		}

		/// <summary>
		/// Delete a Condition Profile, and related Entity
		/// </summary>
		/// <param name="Id"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( int profileId, ref string statusMessage )
		{
			bool isValid = false;
			if ( profileId == 0 )
			{
				statusMessage = "Error - missing an identifier for the ConditionProfile";
				return false;
			}
			using (var context = new Data.CTIEntities())
			{
				DBentity efEntity = context.Entity_ConditionProfile
							.SingleOrDefault( s => s.Id == profileId );

				if (efEntity != null && efEntity.Id > 0)
				{
					Guid rowId = efEntity.RowId;
					context.Entity_ConditionProfile.Remove(efEntity);
					int count = context.SaveChanges();
					if (count > 0)
					{
						isValid = true;
						//16-10-19 mp - create 'After Delete' triggers to delete the Entity
						//new EntityManager().Delete(rowId, ref statusMessage);
					}
				}
				else
				{
					statusMessage = "Error - delete was not possible, as record was not found.";
				}
			}

			return isValid;
		}

		private bool ValidateProfile(ConditionProfile item, ref List<string> messages)
		{
			bool isValid = true;
			bool isNameRequired = true;
			int count = messages.Count;
			string firstEntityName = "";

			if ( item.IsStarterProfile )
				return true;

			if ( item.ConnectionProfileType == "AssessmentConnections" )
			{
				isNameRequired = false;
				//List<AssessmentProfile> list = Entity_AssessmentManager.GetAll( item.RowId, false );
				//if ( item.Id > 0 && list.Count == 0 )
				//{
				//	messages.Add( "Error: an assessment must be selected" );
				//	firstEntityName = "Assessment Connection";
				//}
				//else
				//{
				//	if ( item.Id > 0 )
				//		firstEntityName = list[ 0 ].Name;
				//}
			}
			else if ( item.ConnectionProfileType == "LearningOppConnections" )
			{
				isNameRequired = false;
				//can't fully edit this. On initial create, no lopp, and the TargetLopp is not returned, as these are immediate saves.
				//would have to determine if not initial, and then do a check for existing (as for to.TargetLearningOpportunity in toMap)
				//List<LearningOpportunityProfile> list = Entity_LearningOpportunityManager.LearningOpps_GetAll( item.RowId, false, true );
				//if ( item.Id > 0 && list.Count == 0 )
				//{
				//	messages.Add( "Error: a learning opportunity must be selected" );
				//	firstEntityName = "Learning Opportunity Connection";
				//}
				//else
				//{
				//	if ( item.Id > 0 )
				//		firstEntityName = list[ 0 ].Name;
				//}
			}
			else
			if ( item.ConnectionProfileType == "CredentialConnections" 
				|| item.ConnectionProfileType == "Corequisite" 
				)
			{
				isNameRequired = false;
				firstEntityName = item.ConnectionProfileType;
				List<Credential> list = Entity_CredentialManager.GetAll( item.RowId );
				if ( item.Id > 0 && list.Count == 0 )
				{
					messages.Add( "Error: a credential must be selected" );
					firstEntityName = "Credential Connection";
				}
				else
				{
					if ( item.Id > 0)
					 firstEntityName = list[ 0 ].Name;
				}
				item.ProfileSummary = firstEntityName;
			}

			//ProfileSummary is used by edit, and should have a value for an update
			if ( string.IsNullOrWhiteSpace( item.ProfileSummary) )
			{
				item.ProfileName = firstEntityName;
			} else
				item.ProfileName = item.ProfileSummary;

			//if not a starter, then name is required
			if ( !item.IsStarterProfile 
				&& isNameRequired
				&& string.IsNullOrWhiteSpace( item.ProfileSummary ) )
			{
				//only for a full condition profile!
				messages.Add( "Enter a meaningful name for this condition." );
			}
			if ( !IsUrlValid( item.SubjectWebpage, ref commonStatusMessage ) )
			{
				messages.Add( "The condition Subject Webpage is invalid" + commonStatusMessage );
			}
			if ( item.MinimumAge < 0 || item.MinimumAge > 100 )
			{
				messages.Add( "Error: invalid value for minimum age" );
			}
			if ( item.YearsOfExperience < 0 || item.YearsOfExperience > 50 )
			{
				messages.Add( "Error: invalid value for years of experience" );
			}

			if ( item.Weight < 0 || item.Weight > 1 )
				messages.Add( "Error: invalid value for Weight. Must be a decimal value between 0 and 1." );
			if ( item.ConnectionProfileType == "LearningOppConnections" && item.TargetLearningOpportunity.Count == 0 )
			{
				//the list may always be empty!!
				//messages.Add( "Error: At least one Learning Opportunity must be added to this condition profile." );
			}

			if ( item.CreditHourValue < 0 || item.CreditHourValue > 100 )
				messages.Add( "Error: invalid value for Credit Hour Value. Must be a reasonable decimal value greater than zero." );

			if ( item.CreditUnitValue < 0 || item.CreditUnitValue > 100 )
				messages.Add( "Error: invalid value for Credit Unit Value. Must be a reasonable decimal value greater than zero." );


			//can only have credit hours properties, or credit unit properties, not both
			bool hasCreditHourData = false;
			bool hasCreditUnitData = false;
			if ( item.CreditHourValue > 0 || ( item.CreditHourType ?? "" ).Length > 0 )
				hasCreditHourData = true;
			if (  item.CreditUnitTypeId > 0
				|| (item.CreditUnitTypeDescription ?? "").Length > 0
				|| item.CreditUnitValue > 0)
				hasCreditUnitData = true;

			if ( hasCreditHourData && hasCreditUnitData )
				messages.Add( "Error: Data can be entered for Credit Hour related properties or Credit Unit related properties, but not for both." );

			if ( messages.Count > count )
				isValid = false;
			return isValid;
		}
		#endregion

		#region == Retrieval =======================

		/// <summary>
		/// Get a condition profile
		/// </summary>
		/// <param name="id"></param>
		/// <param name="includeProperties">If true (default), include properties</param>
		/// /// <param name="incudingResources">If true, include resources like assessments, learning opps,</param>
		/// <returns></returns>
		public static ThisEntity GetForEdit( int id)
		{
			ThisEntity entity = new ThisEntity();
			using (var context = new Data.CTIEntities())
			{
				//if ( forEditView  == false)
				//	context.Configuration.LazyLoadingEnabled = false;

				DBentity efEntity = context.Entity_ConditionProfile
						.SingleOrDefault(s => s.Id == id);

				if (efEntity != null && efEntity.Id > 0)
				{
					ToMap(efEntity, entity, true, true, true);
					//strip off extra
					//VERIFY - NOT NECESSARY NOW THAT ProfileSummary is used for the edit view name
					//if ( efEntity.Codes_PropertyValue != null )
					//{
					//	string suffix = " ( " + efEntity.Codes_PropertyValue.Title + " )";
					//	int pos = entity.ProfileName.IndexOf( suffix );
					//	if ( pos > 1 )
					//	{
					//		entity.ProfileName = entity.ProfileName.Substring( 0, pos );
					//	}
					//}
					////check for <span class=
					//int pos2 = entity.ProfileName.ToLower().IndexOf( "</span>" );
					//if ( pos2 > -1 )
					//{
					//	entity.ProfileName = entity.ProfileName.Substring( pos2 + 7 );
					//}
				}
			}

			return entity;
		}

		/// <summary>
		/// Get all condition profiles for parent as minimum for display as links
		/// For this method, the parent is resonsible for assigning to the proper condition profile types, if more than one expected.
		/// </summary>
		/// <param name="parentUid"></param>
		/// <returns></returns>
		public static List<ThisEntity> GetAllForLinks( Guid parentUid )
		{
			ThisEntity to = new ThisEntity();
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
					List<DBentity> results = context.Entity_ConditionProfile
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.ConnectionTypeId )
							.ThenBy( s => s.Created )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity from in results )
						{
							to = new ThisEntity();
							to.Id = from.Id;
							to.RowId = from.RowId;

							to.ParentId = from.EntityId;
							to.ConnectionProfileTypeId = ( int ) from.ConnectionTypeId;
							to.ConditionSubTypeId = ( int ) from.ConditionSubTypeId;

							string conditionType = "";
							if ( from.Codes_PropertyValue != null )
								conditionType = from.Codes_PropertyValue.Title;
							to.ProfileName = (from.Name ?? "").Length > 0 ? from.Name : conditionType;
							to.ProfileSummary = to.ProfileName;
							to.Description = from.Description;
							//ToMap( item, entity, true,true );


							list.Add( to );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".GetAllForLinks" );
			}
			return list;
		}//

		/// <summary>
		///Get all condition profiles for parent 
		/// For this method, the parent is resonsible for assigning to the proper condition profile types, if more than one expected.
		/// </summary>
		/// <param name="parentUid"></param>
		/// <returns></returns>
		public static List<ThisEntity> GetAll( Guid parentUid )
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
					//context.Configuration.LazyLoadingEnabled = false;

					List<DBentity> results = context.Entity_ConditionProfile
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.ConnectionTypeId )
							.ThenBy( s => s.Created )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity item in results )
						{
							entity = new ThisEntity();
							ToMap( item, entity, true, true, false );


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
		

		public static MN.ProfileLink GetProfileLink( Guid profileRowId )
		{
			MN.ProfileLink entity = new MN.ProfileLink();
			using ( var context = new Data.CTIEntities() )
			{
				DBentity efEntity = context.Entity_ConditionProfile
						.SingleOrDefault( s => s.RowId == profileRowId );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					entity.RowId = efEntity.RowId;
					entity.Id = efEntity.Id;
					entity.Name = efEntity.Name;
					entity.Type = typeof( Models.Node.ConditionProfile );

					//get parent entityEUID
					if ( efEntity.Entity != null )
					{
						entity.ParentEntityRowId = efEntity.Entity.EntityUid;
						entity.ParentEntityTypeId = efEntity.Entity.EntityTypeId;
					}

					if ( IsGuidValid( efEntity.AgentUid ) )
					{
						entity.OwningAgentUid = (Guid) efEntity.AgentUid;
					}
					else
					{
						if ( efEntity.Entity != null )
						{
							if ( efEntity.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_CREDENTIAL )
							{
								Credential cred = CredentialManager.GetBasic( efEntity.Entity.EntityUid, false, true );
								entity.OwningAgentUid = cred.OwningAgentUid;
							}
							else if ( efEntity.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_CONDITION_PROFILE )
							{
								MN.ProfileLink cp = GetProfileLink( efEntity.Entity.EntityUid );
								entity.OwningAgentUid = cp.OwningAgentUid;
							}
						}
					}

				}
			}

			return entity;
		}
		public static ThisEntity GetAs_IsPartOf(Guid rowId)
		{
			ThisEntity entity = new ThisEntity();

			using ( var context = new ViewContext() )
			{
				Views.ConditionProfile_ParentSummary item = context.ConditionProfile_ParentSummary
						.SingleOrDefault( s => s.RowId == rowId );

				if ( item != null && item.EntityConditionProfileId > 0 )
				{
					entity.Id = item.EntityConditionProfileId;
					entity.RowId = item.RowId;
					//not use if we want the baseId or entityId
					//this method is primarily for display, and so would not be used to edit or delete an item. Although if a link is exposed to the detai l page, then the baseId should be used.
					entity.ParentId = item.ParentBaseId;

					if (!string.IsNullOrWhiteSpace(item.Name))
						entity.ProfileName = item.Name;
					else
						entity.ProfileName = item.ParentName + " Condition profile";

					if ( item.ParentEntityTypeId == CodesManager.ENTITY_TYPE_CREDENTIAL )
					{
						entity.ParentCredential = new Credential() { Id = item.ParentBaseId, Name = item.ParentName, Description = item.ParentDescription, RowId = item.ParentRowId };
					}
					else if ( item.ParentEntityTypeId == CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE )
					{
						entity.ParentAssessment = new AssessmentProfile() { Id = item.ParentBaseId, Name = item.ParentName, Description = item.ParentDescription, RowId = item.ParentRowId };
					}
					else if ( item.ParentEntityTypeId == CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE )
					{
						entity.ParentLearningOpportunity = new LearningOpportunityProfile() { Id = item.ParentBaseId, Name = item.ParentName, Description = item.ParentDescription, RowId = item.ParentRowId };
					}
					else if ( item.ParentEntityTypeId == CodesManager.ENTITY_TYPE_CONDITION_MANIFEST )
					{
						entity.ParentConditionManifest = new ConditionManifest() { Id = item.ParentBaseId, ProfileName = item.ParentName, Description = item.ParentDescription, RowId = item.ParentRowId };
					}
					//else if ( item.ParentEntityTypeId == CodesManager.ENTITY_TYPE_ORGANIZATION )
					//{
					//	entity.TargetOrganization = new Organization() { Id = item.ParentBaseId, Name = item.ParentName, Description = item.ParentDescription, RowId = item.ParentRowId };
					//}


					//LoggingHelper.DoTrace(8, thisClassName + ".GetAs_IsPartOf()\r\n" +  JsonConvert.SerializeObject( entity ));
				}
			}

			return entity;
		}
		private static void FromMap(ThisEntity from, DBentity to)
		{

			//want to ensure fields from create are not wiped
			if ( to.Id < 1 )
			{
				to.ConnectionTypeId = from.ConnectionProfileTypeId;
				//we may not get the subtype back on update, so only set if > 0, otherwise leave as is???
				if ( from.ConditionSubTypeId > 0 )
				{
					to.ConditionSubTypeId = from.ConditionSubTypeId;
				}
				else
				{
					if ( to.ConditionSubTypeId == 0 )
					{
						if ( from.ConnectionProfileType == "CredentialConnections" )
							to.ConditionSubTypeId = ConditionSubType_CredentialConnection;
						else if ( from.ConnectionProfileType == "AssessmentsConnections" )
							to.ConditionSubTypeId = ConditionSubType_Assessment;
						else if ( from.ConnectionProfileType == "LearningOppConnections" )
							to.ConditionSubTypeId = ConditionSubType_LearningOpportunity;
						else if ( from.ConnectionProfileType == "AlternativeCondition" )
							to.ConditionSubTypeId = ConditionSubType_Alternative;
						else if ( from.ConnectionProfileType == "AdditionalCondition" )
							to.ConditionSubTypeId = ConditionSubType_Additional;
						else
							to.ConditionSubTypeId = 1;
					}
				}
			} else
			{
				if ( from.ConnectionProfileTypeId > 0 )
					to.ConnectionTypeId = from.ConnectionProfileTypeId;
				else if ( to.ConnectionTypeId < 1 )
					to.ConnectionTypeId = 1;

				//ConditionSubTypeId should be left as is from ADD
			}

			to.Id = from.Id;
			
			
			//170316 mparsons - ProfileSummary is used in the edit interface for Name
			if ( string.IsNullOrWhiteSpace( from.ProfileName ) )
				from.ProfileName = from.ProfileSummary ?? "";

			//strip off extra
			if ( to.Codes_PropertyValue != null )
			{
				string suffix = " ( " + to.Codes_PropertyValue.Title + " )";
				int pos = from.ProfileName.IndexOf( suffix );
				if ( pos > 1 )
				{
					from.ProfileName = from.ProfileName.Substring( 0, pos );
				}
			}
			
			//check for wierd jquery addition
			int pos2 = from.ProfileName.ToLower().IndexOf( "jquery" );
			if ( pos2 > 1 )
			{
				from.ProfileName = from.ProfileName.Substring( 0, pos2 );
			}

			//check for <span class=
			pos2 = from.ProfileName.ToLower().IndexOf( "</span>" );
			if ( from.ProfileName.ToLower().IndexOf( "</span>" ) > -1 )
			{
				from.ProfileName = from.ProfileName.Substring( pos2 + 7 );
			}

			to.Name = GetData( from.ProfileName );
			to.Description = GetData( from.Description );
			to.EntityId = from.ParentId;

			if (from.AssertedByAgentUid == null || from.AssertedByAgentUid.ToString() == DEFAULT_GUID)
			{
				to.AgentUid = null;//			
			}
			else
			{
				to.AgentUid = from.AssertedByAgentUid;
			}

			to.Experience = GetData(from.Experience);
			to.SubjectWebpage = from.SubjectWebpage;

			if (from.ApplicableAudienceType != null)
				to.OtherAudienceType = from.ApplicableAudienceType.OtherValue ?? "";
			else
				to.OtherAudienceType = "";

			//if (from.EducationLevel != null)
			//	to.OtherCredentialType = from.EducationLevel.OtherValue ?? "";
			//else
			//	to.OtherCredentialType = "";

			if ( from.MinimumAge > 0 )
				to.MinimumAge = from.MinimumAge;
			else
				to.MinimumAge = null;
			if ( from.YearsOfExperience > 0 )
				to.YearsOfExperience = from.YearsOfExperience;
			else
				to.YearsOfExperience = null;
			if ( from.Weight > 0 )
				to.Weight = from.Weight;
			else
				to.Weight = null;
			to.CreditHourType = GetData( from.CreditHourType );
			to.CreditHourValue = SetData(from.CreditHourValue, 0.5M);
			to.CreditUnitTypeId = SetData(from.CreditUnitTypeId, 1);
			to.CreditUnitTypeDescription = GetData(from.CreditUnitTypeDescription);
			to.CreditUnitValue = SetData(from.CreditUnitValue, 0.5M);

			if (IsValidDate(from.DateEffective))
				to.DateEffective = DateTime.Parse(from.DateEffective);
			else
				to.DateEffective = null;

		}

		public static void ToMap(DBentity from, ThisEntity to
				, bool includingProperties
				, bool incudingResources
				, bool forEditView
				, bool isForCredentialDetails = false)
		{
			ToMapBasics( from, to, forEditView );

			//========================================================
			//TODO - determine what is really needed for the detail page for conditions

			to.Experience = from.Experience;
			to.MinimumAge = GetField(from.MinimumAge, 0);
			to.YearsOfExperience = GetField(from.YearsOfExperience, 0m);
			to.Weight = GetField( from.Weight, 0m );

			to.CreditHourType = from.CreditHourType ?? "";
			to.CreditHourValue = (from.CreditHourValue ?? 0M);
			to.CreditUnitTypeId = (from.CreditUnitTypeId ?? 0);
			to.CreditUnitTypeDescription = from.CreditUnitTypeDescription;
			to.CreditUnitValue = from.CreditUnitValue ?? 0M;

			//to.RequiredCredential = CommaSeparatedListToStringList( from.RequiredCredentials );
			//to.OtherAudienceType = from.OtherAudienceType;
			//to.OtherCredentialType = from.OtherCredentialType;

			if (IsValidDate(from.DateEffective))
				to.DateEffective = ((DateTime)from.DateEffective).ToShortDateString();
			else
				to.DateEffective = "";
			

			//will need a category
			//to.ReferenceUrl = Entity_ReferenceManager.Entity_GetAll(to.RowId, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS);

			//to.RequiredCredentialUrl = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_URLS );

			to.Condition = Entity_ReferenceManager.Entity_GetAll(to.RowId, CodesManager.PROPERTY_CATEGORY_CONDITION_ITEM);

			to.SubmissionOf = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SUBMISSION_ITEM );
			
			to.RequiresCompetenciesFrameworks = Entity_CompetencyFrameworkManager.GetAll( to.RowId, "requires" );
			to.EstimatedCosts = CostProfileManager.GetAll( to.RowId, forEditView );

			if (includingProperties)
			{
				to.AudienceLevel = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_AUDIENCE_LEVEL );
				to.ApplicableAudienceType = EntityPropertyManager.FillEnumeration(to.RowId, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE);

				to.Jurisdiction = Entity_JurisdictionProfileManager.Jurisdiction_GetAll(to.RowId);

				to.ResidentOf = Entity_JurisdictionProfileManager.Jurisdiction_GetAll(to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_RESIDENT);

			}

			//alternative/additional conditions
			//in basics now
			//PopulateSubconditions( to, forEditView );
			

			if (incudingResources)
			{
				//if for the detail page, want to include more info, but not all
				to.TargetCredential = Entity_CredentialManager.GetAll( to.RowId, isForCredentialDetails );

				//assessment
				//for entity.condition(ec) - entity = ec.rowId
				to.TargetAssessment = Entity_AssessmentManager.GetAll(to.RowId, forEditView, isForCredentialDetails );
				foreach ( AssessmentProfile ap in to.TargetAssessment )
					
				{
					if ( ap.HasCompetencies || ap.ChildHasCompetencies )
					{
						to.ChildHasCompetencies = true;
						break;
					}
				}
				//to.TargetTask = Entity_TaskProfileManager.TaskProfile_GetAll(to.RowId);

				//re: forProfilesList, can just pass forEditView, as only use profile list for edit

				to.TargetLearningOpportunity = Entity_LearningOpportunityManager.LearningOpps_GetAll( to.RowId, forEditView, forEditView, isForCredentialDetails );
				foreach (LearningOpportunityProfile e in to.TargetLearningOpportunity)
				{
					if (e.HasCompetencies || e.ChildHasCompetencies)
					{
						to.ChildHasCompetencies = true;
						break;
					}
				}
				//to.TargetCompetency = Entity_CompetencyManager.Competency_GetAll( to.RowId );
			}
		}
		
		public static void ToMapBasics( DBentity from, ThisEntity to, bool forEditView )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.Description = from.Description;
			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;
			to.LastUpdatedBy = SetLastUpdatedBy( to.LastUpdatedById, from.Account_Modifier );

			to.ParentId = from.EntityId;
			if ( IsGuidValid( from.AgentUid ) )
			{
				to.AssertedByAgentUid = ( Guid ) from.AgentUid;
			}
			else
			{
				//attempt to get from parent?
				if ( from.Entity != null  )
				{
					if ( from.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_CREDENTIAL )
					{
						Credential cred = CredentialManager.GetBasic( from.Entity.EntityUid, false, true );
						to.AssertedByAgentUid = cred.OwningAgentUid;
					}
					else if ( from.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_CONDITION_PROFILE )
					{
						MN.ProfileLink cp = GetProfileLink( from.Entity.EntityUid );
						to.AssertedByAgentUid = cp.OwningAgentUid;
					}
				}
			}
			if ( IsGuidValid( to.AssertedByAgentUid ) )
			{
				//TODO - get org and then create profile link
				//to.AssertedByOrgProfileLink = OrganizationManager.Agent_GetProfileLink( to.AssertedByAgentUid );

				to.AssertedBy = OrganizationManager.GetBasics( to.AssertedByAgentUid );

				to.AssertedByOrgProfileLink = new Models.Node.ProfileLink()
				{
					RowId = to.AssertedBy.RowId,
					Id = to.AssertedBy.Id,
					Name = to.AssertedBy.Name,
					Type = typeof( Models.Node.Organization )
				};

			}

			to.ConnectionProfileTypeId = ( int ) from.ConnectionTypeId;
			to.ConditionSubTypeId = GetField( from.ConditionSubTypeId, 1 );
			//todo reset to.ConnectionProfileTypeId if after a starter profile
			if ( to.ConditionSubTypeId == ConditionSubType_CredentialConnection )
			{
				if ( to.Created == to.LastUpdated )
				{
					//reset as was an auto created, so allow use to set type
					to.ConnectionProfileTypeId = 0;
				}
			}

			to.SubjectWebpage = from.SubjectWebpage;

			string parentName = "";
			string conditionType = "";
			if ( from.Entity != null && from.Entity.EntityTypeId == 1 )
				parentName = from.Entity.EntityBaseName;
			if ( from.Codes_PropertyValue != null )
				conditionType = from.Codes_PropertyValue.Title;

			//TODO - need to have a default for a missing name
			//17-03-16 mparsons - using ProfileName for the list view, and ProfileSummary for the edit view
			if ( ( from.Name ?? "" ).Length > 0 )
			{
				//note could have previously had a name, and no longer shown!
				to.ProfileName = from.Name;
			}
			else if ( from.Codes_PropertyValue != null )
			{
				to.ProfileName = parentName ;
			}

			to.ProfileSummary = to.ProfileName;

			if (forEditView && from.Codes_PropertyValue != null )
				to.ProfileName += " ( " + from.Codes_PropertyValue.Title + " )";

			if ( to.ConditionSubTypeId == ConditionSubType_CredentialConnection )
			{
				List<Credential> list = Entity_CredentialManager.GetAll( to.RowId );
				if ( list.Count > 0 )
				{
					to.ProfileName = list[ 0 ].Name;
					if ( list.Count > 1 )
					{
						to.ProfileName += string.Format(" [plus {0} other(s)] ", list.Count-1);
					}
					if ( to.AssertedByOrgProfileLink != null && !string.IsNullOrWhiteSpace(to.AssertedByOrgProfileLink.Name ) )
					{
						to.ProfileName += " ( " + to.AssertedByOrgProfileLink.Name + " ) ";
					}
				}
			}

			if ( to.ConditionSubTypeId == ConditionSubType_Alternative
				&& forEditView 
				&& to.ProfileName.ToLower().IndexOf( "<span class=" ) == -1 )
				to.ProfileName = string.Format( "<span class='alternativeCondition'>ALTERNATIVE&nbsp;</span>{0}", to.ProfileName );
			else if ( to.ConditionSubTypeId == ConditionSubType_Additional
				&& forEditView 
				&& to.ProfileName.ToLower().IndexOf( "<span class=" ) == -1 )
				to.ProfileName = string.Format( "<span class='additionalCondition'>ADDITIONAL&nbsp;</span>{0}", to.ProfileName );


			PopulateSubconditions( to, forEditView );
		} //

		private static void PopulateSubconditions( ThisEntity to, bool forEditView )
		{
			//alternative/additional conditions
			//all required at this time!
			List<ConditionProfile> cpList = new List<ConditionProfile>();
			//this is wrong need to differentiate edit of condition profile versus edit view of credential
			if ( forEditView )
				cpList = Entity_ConditionProfileManager.GetAllForLinks( to.RowId );
			else
				cpList = Entity_ConditionProfileManager.GetAll( to.RowId );

			if ( cpList != null && cpList.Count > 0 )
			{
				foreach ( ConditionProfile item in cpList )
				{
					if ( item.ConditionSubTypeId == Entity_ConditionProfileManager.ConditionSubType_Alternative )
						to.AlternativeCondition.Add( item );
					//else if ( item.ConditionSubTypeId == Entity_ConditionProfileManager.ConditionSubType_Additional )
					//	to.AdditionalCondition.Add( item );
					else
					{
						EmailManager.NotifyAdmin( "Unexpected Alternative/Additional Condition Profile for a condition profile", string.Format( "ConditionProfileId: {0}, ConditionProfileTypeId: {1}, ConditionSubTypeId: {2}", to.Id, item.ConnectionProfileTypeId, item.ConditionSubTypeId ) );
						to.AlternativeCondition.Add( item );
					}
				}
			}
		}


		/// <summary>
		/// Get all condition profiles for parent as minimum for display as links
		/// For this method, the parent is resonsible for assigning to the proper condition profile types, if more than one expected.
		/// </summary>
		/// <param name="parentUid"></param>
		/// <returns></returns>
		//public static List<ThisEntity> GetAllSubconditonsForLinks( Guid parentUid )
		//{
		//	ThisEntity to = new ThisEntity();
		//	List<ThisEntity> list = new List<ThisEntity>();
		//	Entity parent = EntityManager.GetEntity( parentUid );
		//	if ( parent == null || parent.Id == 0 )
		//	{
		//		return list;
		//	}

		//	try
		//	{
		//		using ( var context = new Data.CTIEntities() )
		//		{
		//			List<DBentity> results = context.Entity_ConditionProfile
		//					.Where( s => s.EntityId == parent.Id )
		//					.OrderBy( s => s.ConnectionTypeId )
		//					.ThenBy( s => s.Created )
		//					.ToList();

		//			if ( results != null && results.Count > 0 )
		//			{
		//				foreach ( DBentity from in results )
		//				{
		//					to = new ThisEntity();
		//					to.Id = from.Id;
		//					to.RowId = from.RowId;

		//					to.ParentId = from.EntityId;
		//					to.ConnectionProfileTypeId = ( int ) from.ConnectionTypeId;
		//					to.ConditionSubTypeId = ( int ) from.ConditionSubTypeId;
		//					to.ProfileName = from.Name;
		//					to.ProfileSummary = from.Name;
		//					if ( IsGuidValid( from.AgentUid ) )
		//						to.AssertedByAgentUid = ( Guid ) from.AgentUid;

		//					//may want to get alternative and additional conditions???

		//					list.Add( to );
		//				}
		//			}
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".GetAll" );
		//	}
		//	return list;
		//}//

		private static string GetDefaultName( DBentity from, ThisEntity to )
		{
			string name = "";

			return name;
		}

		/// <summary>
		/// Get all condition profiles for a credential for use on detail page. 
		/// Will need to ensure any target entities return all the necessary (but pointless) extra data.
		/// </summary>
		/// <param name="to"></param>
		public static void FillConditionProfilesForDetailDisplay( Credential to )
		{
			bool forEditView = false;

			//get entity for credential
			using ( var context = new Data.CTIEntities() )
			{
				EM.Entity dbEntity = context.Entity
						.Include( "Entity_ConditionProfile" )
						.AsNoTracking()
						.SingleOrDefault( s => s.EntityUid == to.RowId );

				if ( dbEntity != null && dbEntity.Id > 0 )
				{
					if ( dbEntity.Entity_ConditionProfile != null
				&& dbEntity.Entity_ConditionProfile.Count > 0 )
					{
						ConditionProfile entity = new ConditionProfile();
						//could use this, but need to do mapping get related data


						//to.Requires = dbEntity.Entity_ConditionProfile
						//			.Where( x => x.ConnectionTypeId == ConnectionProfileType_Requirement ) 
						//			as List<ConditionProfile>;

						var creditUnitTypeCodes = CodesManager.GetEnumeration( "creditUnit" ); //Get code table one time - NA 3/17/2017

						foreach ( EM.Entity_ConditionProfile item in dbEntity.Entity_ConditionProfile )
						{
							entity = new ConditionProfile();
							ToMap( item, entity, true, true, false, true );

							//Add the credit unit type enumeration with the selected item, to fix null error in publishing and probably detail - NA 3/17/2017
							entity.CreditUnitType = new Enumeration()
							{
								Items = new List<EnumeratedItem>()
							};
							entity.CreditUnitType.Items.Add( creditUnitTypeCodes.Items.FirstOrDefault( m => m.CodeId == entity.CreditUnitTypeId ) );
							//End edits - NA 3/17/2017

							if ( entity.HasCompetencies || entity.ChildHasCompetencies )
								to.ChildHasCompetencies = true;
							//to.AllConditions = new List<ThisEntity>();
							//add to allConditions - TODO - replace or review?
							//to.AllConditions.Add( entity );

							if ( entity.ConditionSubTypeId == ConditionSubType_CredentialConnection )
							{
								to.CredentialConnections.Add( entity );
							}
							else
							{
								//eventually will only be required or recommends here
								//may want to add logging, and notification - but should be covered via conversion
								if ( entity.ConnectionProfileTypeId == ConnectionProfileType_Requirement )
								{
									//to.Requires.Add( entity );
									to.Requires= HandleSubConditions( to.Requires, entity, forEditView );
								}
								else if ( entity.ConnectionProfileTypeId == ConnectionProfileType_Recommendation )
								{ 
									//to.Recommends.Add( entity );
									to.Recommends = HandleSubConditions( to.Recommends, entity, forEditView );
								}
								else if ( entity.ConnectionProfileTypeId == ConnectionProfileType_Renewal )
								{
									to.Renewal = HandleSubConditions( to.Renewal, entity, forEditView );
								}
								else if ( entity.ConnectionProfileTypeId == ConnectionProfileType_Corequisite )
								{
									to.Corequisite.Add( entity );
								}
								else
								{
									EmailManager.NotifyAdmin( thisClassName + ".FillConditionProfiles. Unhandled connection type", string.Format( "Unhandled connection type of {0} was encountered", entity.ConnectionProfileTypeId ) );
								}
							}
							
						}
					}
				}
				
			}
				
		}//

		public static void FillConditionProfilesForList( Credential to, bool forEditView )
		{
			//get entity for credential
			using ( var context = new Data.CTIEntities() )
			{
				EM.Entity dbEntity = context.Entity
						.Include( "Entity_ConditionProfile" )
						.AsNoTracking()
						.SingleOrDefault( s => s.EntityUid == to.RowId );

				if ( dbEntity != null && dbEntity.Id > 0 )
				{
					if ( dbEntity.Entity_ConditionProfile != null
				&& dbEntity.Entity_ConditionProfile.Count > 0 )
					{
						ConditionProfile entity = new ConditionProfile();

						foreach ( EM.Entity_ConditionProfile item in dbEntity.Entity_ConditionProfile )
						{
							entity = new ConditionProfile();
							//this method is called from the edit view of credential, but here we want to set editView to true?
							ToMapBasics( item, entity, false);						

							if ( entity.ConditionSubTypeId == ConditionSubType_CredentialConnection )
							{
								to.CredentialConnections.Add( entity );
							}
							else
							{

								//eventually will only be required or recommends here
								//may want to add logging, and notification - but should be covered via conversion
								if ( entity.ConnectionProfileTypeId == ConnectionProfileType_Requirement )
								{
									//to.Requires.Add( entity );
									to.Requires= HandleSubConditions( to.Requires, entity, forEditView );
								}
								else if ( entity.ConnectionProfileTypeId == ConnectionProfileType_Recommendation )
								{
									//to.Recommends.Add( entity );
									to.Recommends = HandleSubConditions( to.Recommends, entity, forEditView );
								}
								else if ( entity.ConnectionProfileTypeId == ConnectionProfileType_Renewal )
								{
									to.Renewal = HandleSubConditions( to.Renewal, entity, forEditView );
								}
								else if ( entity.ConnectionProfileTypeId == ConnectionProfileType_Corequisite )
								{
									to.Corequisite.Add( entity );
								}
								else
								{
									EmailManager.NotifyAdmin( thisClassName + ".FillConditionProfilesForList. Unhandled connection type", string.Format( "Unhandled connection type of {0} was encountered", entity.ConnectionProfileTypeId ) );
									//add to required, for dev only?
									if (IsDevEnv())
									{
										entity.ProfileName = ( entity.ProfileName ?? "" ) + " unexpected condition type of " + entity.ConnectionProfileTypeId.ToString();
										to.Requires.Add( entity );
									}
								}
							}

						}
					}
				}

			}
}//

		private static List<ConditionProfile> HandleSubConditions( List<ConditionProfile> profiles, ThisEntity entity, bool forEditView )
		{
			profiles.Add( entity );
			List<ConditionProfile> list = profiles;

			foreach ( ConditionProfile item in entity.AlternativeCondition )
			{
				if ( IsGuidValid( entity.AssertedByAgentUid ) && !IsGuidValid( item.AssertedByAgentUid ) )
					item.AssertedByAgentUid = entity.AssertedByAgentUid;

				if ( forEditView && entity.ProfileName.ToLower().IndexOf( "<span class=" ) == -1 )
				{
					item.ProfileName = string.Format( "<span class='alternativeCondition'>ALTERNATIVE&nbsp;</span>{0}", item.ProfileName );

				}

				
				list.Add( item );
			}

			//foreach ( ConditionProfile item in entity.AdditionalCondition )
			//{
			//	if ( forEditView && entity.ProfileName.ToLower().IndexOf( "<span class=" ) == -1 )
			//	{
			//		item.ProfileName = string.Format( "<span class='additionalCondition'>ADDITIONAL&nbsp;</span>{0}", item.ProfileName );
			//	}
			//	list.Add( item );
			//}
			return list;

		}//


		#endregion


		#region  validations
		public static bool IsParentBeingAddedAsChildToItself( int condProfId, int childId, int childEntityTypeId )
		{
			bool isOk = false;
			using ( var context = new Data.CTIEntities() )
			{

				DBentity efEntity = context.Entity_ConditionProfile
						.SingleOrDefault( s => s.Id == condProfId );

				if ( efEntity != null && efEntity.Id > 0 && efEntity.Entity != null )
				{
					if (efEntity.Entity.EntityTypeId == childEntityTypeId
						&& efEntity.Entity.EntityBaseId == childId)
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
