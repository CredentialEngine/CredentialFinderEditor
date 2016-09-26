using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;

using Models.ProfileModels;
using Models.Common;
using MN = Models.Node;
using EM = Data;
using Utilities;

using Views = Data.Views;
using Data.Views;
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

		#region persistance ==================
		//public bool UpdateConditions(Credential credential, string type, ref List<string> messages, ref int count)
		//{
		//	count = 0;
		//	int profileTypeId = 0;

		//	switch (type)
		//	{

		//		case "requires":
		//			profileTypeId = ConnectionProfileType_Requirement;
		//			return HandleProfiles(credential, credential.Requires, profileTypeId, ref messages, ref count);

		//		case "recommends":
		//			//profileTypeId = ConnectionProfileType_Recommendation;
		//			return HandleProfiles(credential, credential.Recommends, ConnectionProfileType_Recommendation, ref messages, ref count);

		//		case "isrequiredfor":
		//			//profileTypeId = ConnectionProfileType_NextIsRequiredFor;
		//			return HandleProfiles(credential, credential.IsRequiredFor, ConnectionProfileType_NextIsRequiredFor, ref messages, ref count);

		//		case "isrecommendedfor":
		//			profileTypeId = ConnectionProfileType_NextIsRecommendedFor;
		//			return HandleProfiles(credential, credential.IsRecommendedFor, ConnectionProfileType_NextIsRecommendedFor, ref messages, ref count);

		//		case "renew":
		//			profileTypeId = ConnectionProfileType_Renewal;
		//			return HandleProfiles(credential, credential.Renewal, ConnectionProfileType_Renewal, ref messages, ref count);

		//		default:
		//			profileTypeId = 0;
		//			messages.Add(string.Format("Error: Unexpected profile type of {0} was encountered.", type));
		//			return false;
		//	}


		//}

		/// <summary>
		/// Handle a condition profile
		/// </summary>
		/// <param name="credential"></param>
		/// <param name="list"></param>
		/// <param name="profileTypeId"></param>
		/// <param name="messages"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		//public bool HandleProfiles(int credentialId, List<ConditionProfile> list, int profileTypeId, int userId, ref List<string> messages, ref int count)
		//{
		//	if (list == null)
		//		list = new List<ConditionProfile>();
		//	bool isValid = true;
		//	count = 0;
		//	using (var context = new EM.CTIEntities())
		//	{
		//		//loop thru input, check for changes to existing, and for adds
		//		foreach (ConditionProfile item in list)
		//		{
		//			//minimally interface requires a profile name, so if blank, skip the item
		//			if (string.IsNullOrWhiteSpace(item.ProfileName))
		//				continue;

		//			if (!IsValid(item, ref messages))
		//			{
		//				isValid = false;
		//				continue;
		//			}
		//			item.ParentId = credentialId;
		//			item.CreatedById = item.LastUpdatedById = userId;
		//			item.ConnectionProfileTypeId = profileTypeId;
		//			if (item.Id > 0)
		//			{
		//				DBentity p = context.Entity_ConditionProfile
		//						.FirstOrDefault(s => s.Id == item.Id);
		//				if (p != null && p.Id > 0)
		//				{
		//					//just in case missing
		//					p.EntityId = credentialId;
		//					p.ConnectionTypeId = profileTypeId;
		//					item.RowId = p.RowId;
		//					FromMap(item, p);

		//					if (HasStateChanged(context))
		//					{
		//						p.LastUpdated = System.DateTime.Now;
		//						p.LastUpdatedById = userId;
		//						context.SaveChanges();
		//						count++;
		//					}
		//					//regardless, check parts
		//					UpdateParts(item, ref messages);
		//				}
		//				else
		//				{
		//					//error should have been found
		//					isValid = false;
		//					messages.Add(string.Format("Error: the requested role was not found: recordId: {0}", item.Id));
		//				}
		//			}
		//			else
		//			{
		//				if (Add(credential, item, ref messages) == 0)
		//					isValid = false;
		//				else
		//					count++;
		//			}

		//		}

		//	}
		//	//status = string.Join( ",", messages.ToArray() );
		//	return isValid;
		//}

		public bool Save( ConditionProfile item, Guid parentUid, int userId, ref List<string> messages )
		{
			if (string.IsNullOrWhiteSpace(item.ConnectionProfileType ) )
			{
				messages.Add("Error: The profile type must be provided.");
				return false;
			}
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
			switch (item.ConnectionProfileType.ToLower())
			{
				case "requires":
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

				default:
					profileTypeId = 0;
					messages.Add(string.Format("Error: Unexpected profile type of {0} was encountered.", item.ConnectionProfileType));
					return false;
			}
			bool isValid = true;
			using (var context = new Data.CTIEntities())
			{
				if (!IsValid(item, ref messages))
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
						UpdateParts(item, ref messages);
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
					if (Add( item, userId, ref messages) == 0)
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
					//LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
					string message = thisClassName + string.Format(".Add() DbEntityValidationException, EntityId: {0}", entity.ParentId);
					foreach (var eve in dbex.EntityValidationErrors)
					{
						message += string.Format("\rEntity of type \"{0}\" in state \"{1}\" has the following validation errors:",
							eve.Entry.Entity.GetType().Name, eve.Entry.State);
						foreach (var ve in eve.ValidationErrors)
						{
							message += string.Format("- Property: \"{0}\", Error: \"{1}\"",
								ve.PropertyName, ve.ErrorMessage);
						}

						LoggingHelper.LogError(message, true);
					}
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
			if (mgr.UpdateProperties(entity.CredentialType, entity.RowId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE, entity.LastUpdatedById, ref messages) == false)
				isAllValid = false;

			if (mgr.UpdateProperties(entity.ApplicableAudienceType, entity.RowId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE, entity.LastUpdatedById, ref messages) == false)
				isAllValid = false;

			//Asmts
			//N/A AS done immediately
			Entity_ReferenceManager erm = new Entity_ReferenceManager();

			if (erm.EntityUpdate(entity.ReferenceUrl, entity.RowId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS) == false)
				isAllValid = false;

			if (erm.EntityUpdate(entity.ConditionItem, entity.RowId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_CONDITION_ITEM, false) == false)
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
						new EntityManager().Delete(rowId, ref statusMessage);
					}
				}
				else
				{
					statusMessage = "Error - delete was not possible, as record was not found.";
				}
			}

			return isValid;
		}

		private bool IsValid(ConditionProfile item, ref List<string> messages)
		{
			bool isValid = true;

			if (string.IsNullOrWhiteSpace(item.ProfileName))
			{
				isValid = false;
				messages.Add("Error: missing profile name");
			}
			return isValid;
		}
		#endregion

		#region == Retrieval =======================
		/// <summary>
		/// Get all profiles for a credential
		/// </summary>
		/// <param name="fromEntity"></param>
		/// <param name="to"></param>
		/// <param name="forEditView">If false, get fuller child objects (ie assessment)</param>
		//public static void FillProfiles(EM.Credential fromEntity, Credential to, bool forEditView = false)
		//{
		//	to.InitializeConnectionProfiles();
		//	ConditionProfile entity = new ThisEntity();
		//	using (var context = new Data.CTIEntities())
		//	{
		//		List<DBentity> results = new List<DBentity>();

		//		//credential should already have the conditions
		//		//the order should not matter
		//		if (fromEntity.Entity_ConditionProfile != null && fromEntity.Entity_ConditionProfile.Count > 0)
		//		{
		//			results = fromEntity.Entity_ConditionProfile.ToList();
		//		}
		//		//else
		//		//{
		//		//	results = context.Entity_ConditionProfile
		//		//		.Where( s => s.CredentialId == to.Id )
		//		//		.OrderBy( s => s.CredentialId ).ThenBy( s => s.ConnectionTypeId ).ThenBy( s => s.Created )
		//		//		.ToList();
		//		//}

		//		if (results != null && results.Count > 0)
		//		{
		//			foreach (DBentity item in results)
		//			{
		//				entity = new ThisEntity();
		//				ToMap(item, entity, true, true, forEditView);

		//				if (entity.HasCompetencies || entity.ChildHasCompetencies)
		//					to.ChildHasCompetencies = true;

		//				if (entity.ConnectionProfileTypeId == ConnectionProfileType_Requirement)
		//					to.Requires.Add(entity);
		//				else if (entity.ConnectionProfileTypeId == ConnectionProfileType_Recommendation)
		//					to.Recommends.Add(entity);
		//				else if (entity.ConnectionProfileTypeId == ConnectionProfileType_NextIsRequiredFor)
		//					to.IsRequiredFor.Add(entity);
		//				else if (entity.ConnectionProfileTypeId == ConnectionProfileType_NextIsRecommendedFor)
		//					to.IsRecommendedFor.Add(entity);
		//				else if (entity.ConnectionProfileTypeId == ConnectionProfileType_Renewal)
		//					to.Renewal.Add(entity);
		//				else
		//				{
		//					EmailManager.NotifyAdmin(thisClassName + ".FillProfiles. Unhandled connection type", string.Format("Unhandled connection type of {0} was encountered", entity.ConnectionProfileTypeId));
		//				}
		//			}
		//		}
		//	}
		//}//
		/// <summary>
		/// Get a condition profile
		/// </summary>
		/// <param name="id"></param>
		/// <param name="includeProperties">If true (default), include properties</param>
		/// /// <param name="incudingResources">If true, include resources like assessments, learning opps,</param>
		/// <returns></returns>
		public static ThisEntity Get(int id,
				bool includeProperties = true,
				bool incudingResources = false,
				bool forEditView = false)
		{
			ThisEntity entity = new ThisEntity();
			using (var context = new Data.CTIEntities())
			{

				DBentity efEntity = context.Entity_ConditionProfile
						.SingleOrDefault(s => s.Id == id);

				if (efEntity != null && efEntity.Id > 0)
				{
					ToMap(efEntity, entity, includeProperties, incudingResources, forEditView);
				}
			}

			return entity;
		}

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
							ToMap( item, entity, true );


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
		/// <summary>
		/// Get Condition Profile as a ProfileLink
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static MN.ProfileLink GetProfileLink(int profileId)
		{
			MN.ProfileLink entity = new MN.ProfileLink();
			using (var context = new Data.CTIEntities())
			{
				DBentity efEntity = context.Entity_ConditionProfile
						.SingleOrDefault(s => s.Id == profileId);

				if (efEntity != null && efEntity.Id > 0)
				{
					entity.RowId = efEntity.RowId;
					entity.Id = efEntity.Id;
					entity.Name = efEntity.Name;
					entity.Type = typeof(Models.Node.ConditionProfile);
				}
			}

			return entity;
		}
		public static ThisEntity GetAs_IsPartOf(Guid rowId)
		{
			ThisEntity entity = new ThisEntity();

			using (var context = new Data.CTIEntities())
			{
				DBentity item = context.Entity_ConditionProfile
						.SingleOrDefault(s => s.RowId == rowId);

				if (item != null && item.Id > 0)
				{
					entity.Id = item.Id;
					entity.RowId = item.RowId;
					entity.ParentId = item.EntityId;

					if (!string.IsNullOrWhiteSpace(item.Name))
						entity.ProfileName = item.Name;
					else
						entity.ProfileName = item.Codes_PropertyValue.Title + " ( for " + item.EntityId + " )";

					entity.Description = item.Description;
				}
			}

			return entity;
		}
		private static void FromMap(ThisEntity fromEntity, DBentity to)
		{

			//want to ensure fields from create are not wiped
			//if (to.Id < 1)
			//{
			//	if (IsValidDate(fromEntity.Created))
			//		to.Created = fromEntity.Created;
			//	to.CreatedById = fromEntity.CreatedById;
			//}

			to.Id = fromEntity.Id;
			to.ConnectionTypeId = fromEntity.ConnectionProfileTypeId;
			to.Name = fromEntity.ProfileName;
			to.Description = fromEntity.Description;
			to.EntityId = fromEntity.ParentId;

			if (fromEntity.AssertedByAgentUid == null || fromEntity.AssertedByAgentUid.ToString() == DEFAULT_GUID)
			{
				to.AgentUid = null;//			
			}
			else
			{
				to.AgentUid = fromEntity.AssertedByAgentUid;
			}

			to.Experience = fromEntity.Experience;

			if (fromEntity.ApplicableAudienceType != null)
				to.OtherAudienceType = fromEntity.ApplicableAudienceType.OtherValue ?? "";
			else
				to.OtherAudienceType = "";

			//if (fromEntity.CredentialType != null)
			//	to.OtherCredentialType = fromEntity.CredentialType.OtherValue ?? "";
			//else
			//	to.OtherCredentialType = "";

			to.MinimumAge = fromEntity.MinimumAge;
			to.YearsOfExperience = fromEntity.YearsOfExperience;

			if (IsValidDate(fromEntity.DateEffective))
				to.DateEffective = DateTime.Parse(fromEntity.DateEffective);
			else
				to.DateEffective = null;

		}

		public static void ToMap(DBentity fromEntity, ThisEntity to
				, bool includingProperties = false
				, bool incudingResources = false
				, bool forEditView = false)
		{
			to.Id = fromEntity.Id;
			to.RowId = fromEntity.RowId;

			to.ParentId = fromEntity.EntityId;
			to.ConnectionProfileTypeId = (int)fromEntity.ConnectionTypeId;
			to.ProfileName = fromEntity.Name;
			to.ProfileSummary = fromEntity.Name;
			to.Description = fromEntity.Description;
			if (IsGuidValid(fromEntity.AgentUid))
			{
				to.AssertedByAgentUid = (Guid)fromEntity.AgentUid;
				//get agentId - AssertedById will be dropped
				//to.AssertedById = OrganizationManager.Agent_Get(to.AssertedByAgentUid).Id;

				to.AssertedByOrgProfileLink = OrganizationManager.Agent_GetProfileLink(to.AssertedByAgentUid);
			}

			to.Experience = fromEntity.Experience;
			to.MinimumAge = GetField(fromEntity.MinimumAge, 0);
			to.YearsOfExperience = GetField(fromEntity.YearsOfExperience, 0m);


			//to.RequiredCredential = CommaSeparatedListToStringList( fromEntity.RequiredCredentials );
			to.OtherAudienceType = fromEntity.OtherAudienceType;
			//to.OtherCredentialType = fromEntity.OtherCredentialType;

			if (IsValidDate(fromEntity.DateEffective))
				to.DateEffective = ((DateTime)fromEntity.DateEffective).ToShortDateString();
			else
				to.DateEffective = "";
			if (IsValidDate(fromEntity.Created))
				to.Created = (DateTime)fromEntity.Created;
			to.CreatedById = fromEntity.CreatedById == null ? 0 : (int)fromEntity.CreatedById;
			if (IsValidDate(fromEntity.LastUpdated))
				to.LastUpdated = (DateTime)fromEntity.LastUpdated;
			to.LastUpdatedById = fromEntity.LastUpdatedById == null ? 0 : (int)fromEntity.LastUpdatedById;

			//will need a category
			to.ReferenceUrl = Entity_ReferenceManager.Entity_GetAll(to.RowId, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS);

			to.RequiredCredentialUrl = Entity_ReferenceManager.Entity_GetAll(to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_URLS);

			to.ConditionItem = Entity_ReferenceManager.Entity_GetAll(to.RowId, CodesManager.PROPERTY_CATEGORY_CONDITION_ITEM);

			to.RequiresCompetencies = Entity_CompetencyManager.GetAll(to.RowId, "requires");

			if (includingProperties)
			{
				to.CredentialType = EntityPropertyManager.FillEnumeration(to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE);
				to.ApplicableAudienceType = EntityPropertyManager.FillEnumeration(to.RowId, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE);

				to.Jurisdiction = RegionsManager.Jurisdiction_GetAll(to.RowId);

				to.ResidentOf = RegionsManager.Jurisdiction_GetAll(to.RowId, RegionsManager.JURISDICTION_PURPOSE_RESIDENT);

				to.RequiredCredential = Entity_CredentialManager.GetAll(to.RowId);
			}

			if (incudingResources)
			{
				//assessment
				to.TargetAssessment = Entity_AssessmentManager.EntityAssessments_GetAll(to.RowId, forEditView);

				to.TargetTask = Entity_TaskProfileManager.TaskProfile_GetAll(to.RowId);

				to.TargetLearningOpportunity = Entity_LearningOpportunityManager.LearningOpps_GetAll(to.RowId, forEditView);
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


		#endregion

	}
}
