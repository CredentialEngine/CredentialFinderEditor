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
using DBentity = Data.Credential_ConnectionProfile;
using Entity = Models.ProfileModels.ConditionProfile;

namespace Factories
{
	public class ConnectionProfileManager : BaseFactory
	{
		static string thisClassName = "ConditionProfileManager";
		static int ConnectionProfileType_Requirement = 1;
		static int ConnectionProfileType_Recommendation = 2;
		static int ConnectionProfileType_NextIsRequiredFor = 3;
		static int ConnectionProfileType_NextIsRecommendedFor = 4;
		static int ConnectionProfileType_Renewal = 5;

		#region persistance ==================
		public bool Credential_UpdateCondition( Credential credential, string type, ref List<string> messages, ref int count )
		{
			count = 0;
			int profileTypeId = 0;
			
			switch ( type )
				{

					case "requires":
						profileTypeId = ConnectionProfileType_Requirement;
						return HandleProfiles( credential, credential.Requires, profileTypeId, ref messages, ref count );
						
					case "recommends":
						//profileTypeId = ConnectionProfileType_Recommendation;
						return HandleProfiles( credential, credential.Recommends, ConnectionProfileType_Recommendation, ref messages, ref count );
						
					case "isrequiredfor":
						//profileTypeId = ConnectionProfileType_NextIsRequiredFor;
						return HandleProfiles( credential, credential.IsRequiredFor, ConnectionProfileType_NextIsRequiredFor, ref messages, ref count );

					case "isrecommendedfor":
						profileTypeId = ConnectionProfileType_NextIsRecommendedFor;
						return HandleProfiles( credential, credential.IsRecommendedFor, ConnectionProfileType_NextIsRecommendedFor, ref messages, ref count );

					case "renew":
						profileTypeId = ConnectionProfileType_Renewal;
						return HandleProfiles( credential, credential.Renewal, ConnectionProfileType_Renewal, ref messages, ref count );

					default:
						profileTypeId = 0;
						messages.Add( string.Format("Error: Unexpected profile type of {0} was encountered.", type) );
						return false;
				}

			
		}

		/// <summary>
		/// Handle a condition profile
		/// </summary>
		/// <param name="credential"></param>
		/// <param name="list"></param>
		/// <param name="profileTypeId"></param>
		/// <param name="messages"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public bool HandleProfiles( Credential credential, List<ConditionProfile> list, int profileTypeId, ref List<string> messages, ref int count )
		{
			if ( list == null )
				list = new List<ConditionProfile>();
			bool isValid = true;
			count = 0;
			using ( var context = new Data.CTIEntities() )
			{
				//loop thru input, check for changes to existing, and for adds
				foreach ( ConditionProfile item in list )
				{
					//minimally interface requires a profile name, so if blank, skip the item
					if ( string.IsNullOrWhiteSpace( item.ProfileName ) )
						continue;

					if ( !IsValid( item, ref messages ) )
					{
						isValid = false;
						continue;
					}
					item.ParentId = credential.Id;
					item.CreatedById = item.LastUpdatedById = credential.LastUpdatedById;
					item.ConnectionProfileTypeId = profileTypeId;
					if ( item.Id > 0 )
					{
						EM.Credential_ConnectionProfile p = context.Credential_ConnectionProfile
								.FirstOrDefault( s => s.Id == item.Id );
						if ( p != null && p.Id > 0 )
						{
							//just in case missing
							p.CredentialId = credential.Id;
							p.ConnectionTypeId = profileTypeId;
							item.RowId = p.RowId;
							FromMap( item, p );

							if ( HasStateChanged( context ) )
							{
								p.LastUpdated = System.DateTime.Now;
								p.LastUpdatedById = credential.LastUpdatedById;
								context.SaveChanges();
								count++;
							}
							//regardless, check parts
							UpdateParts( item, ref messages );
						}
						else
						{
							//error should have been found
							isValid = false;
							messages.Add( string.Format( "Error: the requested role was not found: recordId: {0}", item.Id ) );
						}
					}
					else
					{
						if ( Entity_Add( credential, item, ref messages ) == 0 )
							isValid = false;
						else
							count++;
					}

				}

			}
			//status = string.Join( ",", messages.ToArray() );
			return isValid;
		}

		public bool HandleProfile( Credential credential, ConditionProfile item, string profileType, ref List<string> messages )
		{
			int profileTypeId = 0;
			switch ( profileType.ToLower() )
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
					messages.Add( string.Format( "Error: Unexpected profile type of {0} was encountered.", profileType ) );
					return false;
			}

			return HandleProfile( credential, item, profileTypeId, ref messages );
		}
		public bool HandleProfile( Credential credential, ConditionProfile item, int profileTypeId, ref List<string> messages )
		{
		
			bool isValid = true;
			using ( var context = new Data.CTIEntities() )
			{

					//minimally interface requires a profile name, so if blank, skip the item
				//if ( string.IsNullOrWhiteSpace( item.ProfileName ) )
				//{
				//	messages.Add( "Error:  a profile name is required");
				//	return false;
				//}

					if ( !IsValid( item, ref messages ) )
					{
						return false;
					}
					item.ParentId = credential.Id;
					item.CreatedById = item.LastUpdatedById = credential.LastUpdatedById;
					item.ConnectionProfileTypeId = profileTypeId;
					if ( item.Id > 0 )
					{
						EM.Credential_ConnectionProfile p = context.Credential_ConnectionProfile
								.FirstOrDefault( s => s.Id == item.Id );
						if ( p != null && p.Id > 0 )
						{
							//just in case missing
							//p.CredentialId = credential.Id;
							//p.ConnectionTypeId = profileTypeId;
							item.RowId = p.RowId;
							FromMap( item, p );



							if ( HasStateChanged( context ) )
							{
								p.LastUpdated = System.DateTime.Now;
								p.LastUpdatedById = credential.LastUpdatedById;
								context.SaveChanges();
							}
							//regardless, check parts
							UpdateParts( item, ref messages);
						}
						else
						{
							//error should have been found
							isValid = false;
							messages.Add( string.Format( "Error: the requested role was not found: recordId: {0}", item.Id ) );
						}
					}
					else
					{
						if ( Entity_Add( credential, item, ref messages ) == 0 )
							isValid = false;
					}

				//}

			}
			//status = string.Join( ",", messages.ToArray() );
			return isValid;
		}


		/// <summary>
		/// add a ConditionProfile
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Entity_Add(Credential credential, Entity entity, ref List<String> messages )
		{
			DBentity efEntity = new DBentity();
			entity.ParentId = credential.Id;
			using ( var context = new Data.CTIEntities() )
			{
				try
				{

					FromMap( entity, efEntity );

					efEntity.CredentialId = credential.Id;
					if ( !IsValidGuid( efEntity.RowId ) )
						efEntity.RowId = Guid.NewGuid();
					efEntity.CreatedById = credential.LastUpdatedById;
					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdatedById = credential.LastUpdatedById;
					efEntity.LastUpdated = System.DateTime.Now;

					context.Credential_ConnectionProfile.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						entity.Id = efEntity.Id;
						entity.RowId = efEntity.RowId;

						UpdateParts( entity, ref messages );

						return efEntity.Id;
					}
					else
					{
						//?no info on error
						messages.Add ("Error - the profile was not saved. ");
						string message = string.Format( "ConditionProfileManager. ConditionProfile_Add Failed", "Attempted to add a ConditionProfile. The process appeared to not work, but was not an exception, so we have no message, or no clue.ConditionProfile. CredentialId: {0}, createdById: {1}", entity.ParentId, entity.CreatedById );
						EmailManager.NotifyAdmin( "ConditionProfileManager. ConditionProfile_Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					//LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
					string message = thisClassName + string.Format( ".ConditionProfile_Add() DbEntityValidationException, CredentialId: {0}", credential.Id );
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
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".ConditionProfile_Add(), CredentialId: {0}", entity.ParentId ) );
				}
			}

			return efEntity.Id;
		}

		public bool UpdateParts( Entity entity, ref List<String> messages )
		{
			bool isAllValid = true;
		
			//OLD
			//ConnectionProfile_PartsManager cpm = new ConnectionProfile_PartsManager();
			//if ( cpm.UpdateProperties( entity, ref messages ) == false )
			//	isAllValid = false;

			EntityPropertyManager mgr = new EntityPropertyManager();
			if ( mgr.UpdateProperties( entity.CredentialType, entity.RowId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			if ( mgr.UpdateProperties( entity.ApplicableAudienceType, entity.RowId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;	


			//Asmts
			//N/A AS done immediately
			Entity_ReferenceManager erm = new Entity_ReferenceManager();
			//until new version incorporates, skip updates for existing stuff
			//if ( entity.IsNewVersion )
			//{

				if ( erm.EntityUpdate( entity.ReferenceUrl, entity.RowId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS ) == false )
					isAllValid = false;

				if ( erm.EntityUpdate( entity.ConditionItem, entity.RowId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_CONDITION_ITEM, false ) == false )
					isAllValid = false;

				//if ( erm.EntityUpdate( entity.TargetMiniCompetency, entity.RowId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_COMPETENCY ) == false )
				//	isAllValid = false;
			//}
			//else 
			//{
			//	if ( erm.EntityUpdate( entity.ReferenceUrl, entity.RowId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS ) == false )
			//		isAllValid = false;

			//	if ( erm.EntityUpdate( entity.RequiredCredentialUrl, entity.RowId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_URLS ) == false )
			//		isAllValid = false;


			//	if ( !new Entity_TaskProfileManager().TaskProfileUpdate( entity.TargetTask, entity.RowId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, entity.LastUpdatedById, ref messages ) )
			//		isAllValid = false;

			//	if ( !new Entity_CompetencyManager().CompetencyUpdate( entity.TargetCompetency, entity.RowId, CodesManager.ENTITY_TYPE_CONNECTION_PROFILE, entity.LastUpdatedById, ref messages ) )
			//		isAllValid = false;

			//	RegionsManager rmgr = new RegionsManager();
			//	//regions
			//	if ( !rmgr.JurisdictionProfile_Update( entity.Jurisdiction, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, RegionsManager.JURISDICTION_PURPOSE_SCOPE, ref messages ) )
			//		isAllValid = false;
			//	//resident
			//	if ( !rmgr.JurisdictionProfile_Update( entity.ResidentOf, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, RegionsManager.JURISDICTION_PURPOSE_RESIDENT, ref messages ) )
			//		isAllValid = false;
			//}

			//
			return isAllValid;
		}

		/// <summary>
		/// Delete a Condition Profile, and related Entity
		/// </summary>
		/// <param name="Id"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool ConditionProfile_Delete( int Id, ref string statusMessage )
		{
			bool isValid = false;
			if ( Id == 0 )
			{
				statusMessage = "Error - missing an identifier for the ConditionProfile";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				DBentity efEntity = context.Credential_ConnectionProfile
							.SingleOrDefault( s => s.Id == Id );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					Guid rowId = efEntity.RowId;
					context.Credential_ConnectionProfile.Remove( efEntity );
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

			return isValid;
		}

		private bool IsValid( ConditionProfile item, ref List<string> messages )
		{
			bool isValid = true;
			//if ( item.AssertedById == 0 && (item.AssertedByAgentUid == null || item.AssertedByAgentUid.ToString() == DEFAULT_GUID) )
			//{
			//	isValid = false;
			//	messages.Add( "Error: missing acting agent" );
			//}
			if (string.IsNullOrWhiteSpace(item.ProfileName))
			{
				isValid = false;
				messages.Add( "Error: missing profile name" );
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
		public static void FillProfiles( EM.Credential fromEntity, Credential to, bool forEditView = false )
		{
			to.InitializeConnectionProfiles();
			ConditionProfile entity = new Entity();
			using ( var context = new Data.CTIEntities() )
			{
				List<EM.Credential_ConnectionProfile> results = context.Credential_ConnectionProfile
						.Where( s => s.CredentialId == to.Id )
						.OrderBy( s => s.CredentialId ).ThenBy(s => s.ConnectionTypeId).ThenBy(s => s.Created)
						.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( EM.Credential_ConnectionProfile item in results )
					{
						entity = new Entity();
						ToMap( item, entity, true, true, forEditView );

						if ( entity.HasCompetencies || entity.ChildHasCompetencies )
							to.ChildHasCompetencies = true;

						if ( entity.ConnectionProfileTypeId ==  ConnectionProfileType_Requirement)
							to.Requires.Add( entity );
						else if ( entity.ConnectionProfileTypeId == ConnectionProfileType_Recommendation )
							to.Recommends.Add( entity );
						else if ( entity.ConnectionProfileTypeId == ConnectionProfileType_NextIsRequiredFor )
							to.IsRequiredFor.Add( entity );
						else if ( entity.ConnectionProfileTypeId == ConnectionProfileType_NextIsRecommendedFor )
							to.IsRecommendedFor.Add( entity );
						else if ( entity.ConnectionProfileTypeId == ConnectionProfileType_Renewal )
							to.Renewal.Add( entity );
						else
						{
							EmailManager.NotifyAdmin( thisClassName + ".FillProfiles. Unhandled connection type", string.Format( "Unhandled connection type of {0} was encountered", entity.ConnectionProfileTypeId ) );
						}
					}
				}
			}
		}//
		/// <summary>
		/// Get a condition profile
		/// </summary>
		/// <param name="id"></param>
		/// <param name="includeProperties">If true (default), include properties</param>
		/// /// <param name="incudingResources">If true, include resources like assessments, learning opps,</param>
		/// <returns></returns>
		public static Entity ConditionProfile_Get( int id, 
				bool includeProperties = true,
				bool incudingResources = false, 
				bool forEditView = false )
		{
			Entity entity = new Entity();
			using ( var context = new Data.CTIEntities() )
			{

				DBentity efEntity = context.Credential_ConnectionProfile
						.SingleOrDefault( s => s.Id == id );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					ToMap( efEntity, entity, includeProperties, incudingResources, forEditView );
				}
			}

			return entity;
		}

		/// <summary>
		/// Get Condition Profile as a ProfileLink
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static MN.ProfileLink ConditionProfile_GetProfileLink( int profileId )
		{
			MN.ProfileLink entity = new MN.ProfileLink();
			using ( var context = new Data.CTIEntities() )
			{
				DBentity efEntity = context.Credential_ConnectionProfile
						.SingleOrDefault( s => s.Id == profileId );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					entity.RowId = efEntity.RowId;
					entity.Id = efEntity.Id;
					entity.Name = efEntity.Name;
					entity.Type = typeof( Models.Node.ConditionProfile );
				}
			}

			return entity;
		}
		private static void FromMap( Entity fromEntity, DBentity to )
		{

			//want to ensure fields from create are not wiped
			if ( to.Id < 1 )
			{
				if ( IsValidDate( fromEntity.Created ) )
					to.Created = fromEntity.Created;
				to.CreatedById = fromEntity.CreatedById;
			}

			to.Id = fromEntity.Id;
			to.ConnectionTypeId = fromEntity.ConnectionProfileTypeId;
			to.Name = fromEntity.ProfileName;
			to.Description = fromEntity.Description;
			to.CredentialId = fromEntity.ParentId;

			//Guid? agentUid = null;
			//agentUid = fromEntity.AssertedByAgentUid;
			//int assertedByOrgId = fromEntity.AssertedById;
			if ( fromEntity.AssertedByAgentUid == null || fromEntity.AssertedByAgentUid.ToString() == DEFAULT_GUID )
			{
				to.AgentUid = null;//			
			}
			else
			{
				//agentUid = fromEntity.AssertedByAgentUid;

				//interim: handle usually orgId, and get Uid
				to.AgentUid = fromEntity.AssertedByAgentUid;
			}

			to.Experience = fromEntity.Experience;

			if ( fromEntity.IsNewVersion )
			{
				//don't overwrite properties not in the interface
				if ( fromEntity.ApplicableAudienceType != null )
					to.OtherAudienceType = fromEntity.ApplicableAudienceType.OtherValue ?? "";
				else
					to.OtherAudienceType = "";

				if ( fromEntity.CredentialType != null )
					to.OtherCredentialType = fromEntity.CredentialType.OtherValue ?? "";
				else
					to.OtherCredentialType = "";

				to.MinimumAge = fromEntity.MinimumAge;

			}
			else
			{

				to.OtherAudienceType = fromEntity.OtherAudienceType;
				to.OtherCredentialType = fromEntity.OtherCredentialType;

				to.MinimumAge = fromEntity.MinimumAge;
				//to.TargetCredentials = GetMessages( fromEntity.RequiredCredential );
			}
			
			//to.DateEffective = DateTime.Parse( fromEntity.DateEffective);
			if ( IsValidDate( fromEntity.DateEffective ) )
				to.DateEffective = DateTime.Parse( fromEntity.DateEffective );
			else
				to.DateEffective = null;


		}
		private static void ToMap( DBentity fromEntity, Entity to
				,bool includingProperties = false
				,bool incudingResources = false
				,bool forEditView = false)
		{
			to.Id = fromEntity.Id;
			to.RowId = fromEntity.RowId;

			to.ParentId = fromEntity.CredentialId;
			to.ConnectionProfileTypeId = (int)fromEntity.ConnectionTypeId;
			to.ProfileName = fromEntity.Name;
			to.ProfileSummary = fromEntity.Name;
			to.Description = fromEntity.Description;
			if ( IsGuidValid( fromEntity.AgentUid ) )
			{
				to.AssertedByAgentUid = ( Guid ) fromEntity.AgentUid;
				//get agentId - AssertedById will be dropped
				to.AssertedById = OrganizationManager.Agent_Get( to.AssertedByAgentUid ).Id;

				to.AssertedByOrgProfileLink = OrganizationManager.Agent_GetProfileLink( to.AssertedByAgentUid );
			}

			to.Experience = fromEntity.Experience;
			to.MinimumAge = GetField(fromEntity.MinimumAge, 0);

			//to.RequiredCredential = CommaSeparatedListToStringList( fromEntity.TargetCredentials );
			to.OtherAudienceType = fromEntity.OtherAudienceType;
			to.OtherCredentialType = fromEntity.OtherCredentialType;

			if ( IsValidDate( fromEntity.DateEffective ) )
				to.DateEffective = (( DateTime ) fromEntity.DateEffective).ToShortDateString();
			else
				to.DateEffective = "";
			if ( IsValidDate( fromEntity.Created ) )
				to.Created = ( DateTime ) fromEntity.Created;
			to.CreatedById = fromEntity.CreatedById == null ? 0 : ( int ) fromEntity.CreatedById;
			if ( IsValidDate( fromEntity.LastUpdated ) )
				to.LastUpdated = ( DateTime ) fromEntity.LastUpdated;
			to.LastUpdatedById = fromEntity.LastUpdatedById == null ? 0 : ( int ) fromEntity.LastUpdatedById;

			//TODO - remove calls to children once using new editor!!!!!!!!!!

			//will need a category
			to.ReferenceUrl = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS );

			to.RequiredCredentialUrl = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_URLS );

			to.ConditionItem = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_CONDITION_ITEM );

			//to.TargetMiniCompetency = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_COMPETENCY );

			to.RequiresCompetencies = Entity_CompetencyManager.GetAll( to.RowId, "requires" );

			if ( includingProperties )
			{
				//FillCredentialType( fromEntity, to );
				//FillAudienceType( fromEntity, to );

				to.CredentialType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE );
				to.ApplicableAudienceType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE );

				to.Jurisdiction = RegionsManager.Jurisdiction_GetAll( to.RowId );

				to.ResidentOf = RegionsManager.Jurisdiction_GetAll( to.RowId, RegionsManager.JURISDICTION_PURPOSE_RESIDENT );

				to.TargetCredential = Entity_CredentialManager.GetAll( to.RowId );
			}

			if ( incudingResources )
			{
				//assessment
				to.TargetAssessment = Entity_AssessmentManager.EntityAssessments_GetAll( to.RowId, forEditView );

				to.TargetTask = Entity_TaskProfileManager.TaskProfile_GetAll( to.RowId );

				to.TargetLearningOpportunity = Entity_LearningOpportunityManager.LearningOpps_GetAll( to.RowId, forEditView );
				foreach ( LearningOpportunityProfile e in to.TargetLearningOpportunity )
				{
					if ( e.HasCompetencies || e.ChildHasCompetencies )
					{
						to.ChildHasCompetencies = true;
						break;
					}
				}
				//to.TargetCompetency = Entity_CompetencyManager.Competency_GetAll( to.RowId );
			}
		}

		#endregion


		//private static void FillCredentialType( DBentity fromEntity, Entity to )
		//{
		//	to.CredentialType = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE );
			
		//	to.CredentialType.ParentId = to.Id;
		//	to.CredentialType.Items = new List<EnumeratedItem>();
		//	EnumeratedItem item = new EnumeratedItem();
		//	if ( !string.IsNullOrWhiteSpace( to.OtherCredentialType ) )
		//		to.CredentialType.OtherValue = to.OtherCredentialType;

		//	using ( var context = new ViewContext() )
		//	{
		//		List<ConnectionProfileProperty_Summary> results = context.ConnectionProfileProperty_Summary
		//			.Where( s => s.ConnectionProfileId == fromEntity.Id && s.CategoryId == CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE )
		//							.OrderBy( s => s.Category ).ThenBy( s => s.Property )
		//							.ToList();

		//		if ( results != null && results.Count > 0 )
		//		{
		//			foreach ( ConnectionProfileProperty_Summary prop in results )
		//			{

		//				item = new EnumeratedItem();
		//				item.Id = prop.PropertyValueId;
		//				item.Value = prop.PropertyValueId.ToString();
		//				item.Selected = true;
						
		//				item.Name = prop.Property;
		//				to.CredentialType.Items.Add( item );

		//			}
		//		}

		//	}

		//}

		//private static void FillAudienceType( DBentity fromEntity, Entity to )
		//{
		//	to.ApplicableAudienceType = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE );

		//	to.ApplicableAudienceType.ParentId = to.Id;
		//	to.ApplicableAudienceType.Items = new List<EnumeratedItem>();
		//	EnumeratedItem item = new EnumeratedItem();
		//	if ( !string.IsNullOrWhiteSpace( to.OtherAudienceType ) )
		//		to.ApplicableAudienceType.OtherValue = to.OtherAudienceType;

		//	using ( var context = new ViewContext() )
		//	{
		//		List<ConnectionProfileProperty_Summary> results = context.ConnectionProfileProperty_Summary
		//			.Where( s => s.ConnectionProfileId == fromEntity.Id && s.CategoryId == CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE )
		//							.OrderBy( s => s.Category ).ThenBy( s => s.Property )
		//							.ToList();

		//		if ( results != null && results.Count > 0 )
		//		{
		//			foreach ( ConnectionProfileProperty_Summary prop in results )
		//			{

		//				item = new EnumeratedItem();
		//				item.Id = prop.PropertyValueId;
		//				item.Value = prop.PropertyValueId.ToString();
		//				item.Selected = true;
						
		//				item.Name = prop.Property;
		//				to.ApplicableAudienceType.Items.Add( item );

		//			}
		//		}

		//	}

		//}

		#region == Assessments =======================
		//private static void FillAssessments( DBentity fromEntity, Entity to )
		//{
		//	to.TargetAssessment = new List<AssessmentProfile>();
		//	to.TargetAssessment = Entity_AssessmentManager.EntityAssessments_GetAll( to.RowId );

		//}

		#endregion

	}
}
