using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using MN = Models.Node;
using Models.ProfileModels;
using EM = Data;
using Views = Data.Views;
using Utilities;
using DBentity = Data.Entity_QA_Action;
using Entity = Models.ProfileModels.QualityAssuranceActionProfile;
using ViewContext = Data.Views.CTIEntities1;

namespace Factories
{
	public class Entity_QualityAssuranceActionManager : BaseFactory
	{
		static string thisClassname = "Entity_QualityAssuranceActionManager";
		#region Quality Assurance Organization Roles ===================================================
		public bool QualityAssuranceAction_SaveProfile( QualityAssuranceActionProfile profile, int userId, ref List<string> messages )
		{
			bool isValid = true;
			bool isEmpty = false;

			if ( !IsValidGuid( profile.ParentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
				return false;
			}

			//validate profile
			if ( !ValidateProfile( profile, ref isEmpty, ref messages ) )
			{
				return false;
			}

			//the parent needs to be established by using isParentActor
			Views.Entity_Summary parent = EntityManager.GetDBEntity( profile.ParentUid );
			using ( var context = new EM.CTIEntities() )
			{

				if ( profile.Id > 0 )
				{
					DBentity dbEntity = context.Entity_QA_Action.FirstOrDefault( s => s.Id == profile.Id );
					if ( dbEntity != null && dbEntity.Id > 0 )
					{
						//should not be able to change the profile/parentId
						//dbEntity.EntityId = parent.Id;
						dbEntity.RelationshipTypeId = profile.RoleTypeId;
						dbEntity.AgentUid = profile.ActingAgentUid;
						dbEntity.IssuedCredentialId = profile.IssuedCredentialId;

						DateTime date;
						if ( DateTime.TryParse( profile.StartDate, out date ) )
							dbEntity.StartDate = date;
						else
							dbEntity.StartDate = null;
						if ( DateTime.TryParse( profile.EndDate, out date ) )
							dbEntity.EndDate = date;
						else
							dbEntity.EndDate = null;

						dbEntity.Description = profile.Description;

						if ( HasStateChanged( context ) )
						{
							dbEntity.LastUpdated = System.DateTime.Now;
							dbEntity.LastUpdatedById = userId;
							context.SaveChanges();

						}
					}
					else
					{
						//error should have been found
						isValid = false;
						messages.Add( string.Format( "Error: the requested role was not found: recordId: {0}", profile.Id ) );
					}
				}
				else
				{
					
					//TODO update code to assume credential id is passed in profile
					if ( QualityAssuranceAction_Add( profile, parent.Id, userId, ref isEmpty, ref messages ) == false )
						isValid = false;
					else if ( isEmpty )
					{
						isValid = false;
						messages.Add( "Error: Please enter the required information" );
					}
				}


			}
			return isValid;
		}

		private bool QualityAssuranceAction_Add( QualityAssuranceActionProfile profile, int entityId, int userId, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;
			//should assume validated

			profile.ParentId = entityId;
			//ensure doesn't already exist
			//TODO - can the same agent be used with a different cred - I would guess NOT
			if ( DoesQualityAssuranceActionProfileExist( entityId, profile.ActingAgentUid, profile.RoleTypeId) )
			{
				messages.Add( "Error: the selected Quality Assurance action already exists!" );
				return false;
			}
			
			using ( var context = new EM.CTIEntities() )
			{
				DBentity dbEntity = new DBentity();
				dbEntity.EntityId = profile.ParentId;
				dbEntity.AgentUid = profile.ActingAgentUid;
				dbEntity.RelationshipTypeId = profile.RoleTypeId;
				dbEntity.IssuedCredentialId = profile.IssuedCredentialId;

				DateTime date;
				if ( DateTime.TryParse( profile.StartDate, out date ) )
					dbEntity.StartDate = date;
				else
					dbEntity.StartDate = null;
				if ( DateTime.TryParse( profile.EndDate, out date ) )
					dbEntity.EndDate = date;

				dbEntity.Description = profile.Description;
				dbEntity.Created = dbEntity.LastUpdated = System.DateTime.Now;
				dbEntity.CreatedById = dbEntity.LastUpdatedById = userId;
				
				dbEntity.RowId = Guid.NewGuid();
				context.Entity_QA_Action.Add( dbEntity );

				// submit the change to database
				int count = context.SaveChanges();
				profile.Id = dbEntity.Id;
				profile.RowId = dbEntity.RowId;
			}

			return isValid;
		}

		/// <summary>
		/// Delete an Entity.QA record
		/// </summary>
		/// <param name="recordId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool QualityAssuranceAction_Delete( int recordId, ref string statusMessage )
		{
			bool isValid = false;
			if ( recordId == 0 )
			{
				statusMessage = "Error - missing identifier, please select a record.";
				return false;
			}

			using ( var context = new EM.CTIEntities() )
			{
				DBentity efEntity =
	context.Entity_QA_Action.SingleOrDefault( s => s.Id == recordId );
				if ( efEntity != null && efEntity.Id > 0 )
				{
					context.Entity_QA_Action.Remove( efEntity );
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					}
				}
				else
				{
					statusMessage = string.Format( "Record was not found: {0}", recordId );
					isValid = false;
				}
			}

			return isValid;
		}


		private bool ValidateProfile( QualityAssuranceActionProfile item, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;
			//if all missing, assume that there was just a preselection
			if ( item.ParentId == 0 && !IsValidGuid( item.ActingAgentUid ) && item.RoleTypeId == 0 )
			{
				isEmpty = true;
				return true;
			}
			if ( item.ParentId == 0 )
			{
				isValid = false;
				messages.Add( "Error: the related entity has not been selected. Select a top level entity, and then add Quality Assurance action" );
			}
			if ( item.RoleTypeId == 0 )
			{
				//do we have an agent name at this time?
				messages.Add( string.Format( "Error: a role was not entered. Select a role and try again. AgentId: {0}", item.ActingAgentId ) );
			}

			if ( IsValidGuid( item.ActingAgentUid ) && item.RoleTypeId == 0 )
			{
				messages.Add( "Error: invalid request, please select a role." );
				isValid = false;
			}
			else if ( !IsValidGuid( item.ActingAgentUid ) && item.RoleTypeId > 0 )
			{
				messages.Add( "Error: invalid request, please select an agent." );
				isValid = false;
			}

			if ( IsValidGuid( item.ActingAgentUid ) )
			{
				//TODO - need to handle person (the follow should handle person
				MN.ProfileLink org = OrganizationManager.Agent_GetProfileLink( item.ActingAgentUid );
				if ( org == null || org.Name.Length == 0 )
				{
					messages.Add( "Error: the selected agent was not found!" );
					LoggingHelper.DoTrace( 5, thisClassname + string.Format( ".ValidateProfile the agent was not found, for entityId: {0}, AgentId:{1}, RoleId: {2}", item.ParentId, item.ActingAgentId, item.RoleTypeId ) );
					return false;
				}
			}

			if ( item.IssuedCredentialId == 0 )
			{
				isValid = false;
				messages.Add( "Error: please select an issued credential from the agent" );
			}
			if ( !string.IsNullOrWhiteSpace( item.StartDate ) && !IsValidDate( item.StartDate ) )
			{
				isValid = false;
				messages.Add( "Error: please provide a valid Start Date" );
			}
			if ( !string.IsNullOrWhiteSpace( item.EndDate ) && !IsValidDate( item.EndDate ) )
			{
				isValid = false;
				messages.Add( "Error: please provide a valid End Date" );
			}
			return isValid;
		}
		public static QualityAssuranceActionProfile QualityAssuranceActionProfile_Get( int profileId )
		{
			QualityAssuranceActionProfile entity = new QualityAssuranceActionProfile();
			using ( var context = new ViewContext() )
			{
				Views.Entity_QAAction_Summary dbEntity = context.Entity_QAAction_Summary.FirstOrDefault( s => s.EntityQAActionId == profileId );

				if ( dbEntity != null && dbEntity.EntityQAActionId > 0 )
				{
					MapTo( dbEntity, entity) ;

				}
				else
				{
					entity.Id = 0;
				}

			}
			return entity;

		}
		public static List<QualityAssuranceActionProfile> QualityAssuranceActionProfile_GetAll( Guid pParentUid )
		{
			Entity entity = new QualityAssuranceActionProfile();
			List<QualityAssuranceActionProfile> list = new List<QualityAssuranceActionProfile>();

			//Views.Entity_Summary parent = EntityManager.GetDBEntity( pParentUid );
			using ( var context = new ViewContext() )
			{
				List<Views.Entity_QAAction_Summary> results = context.Entity_QAAction_Summary
						.Where( s => s.EntityUid == pParentUid )
						.ToList();

				foreach ( Views.Entity_QAAction_Summary dbEntity in results )
				{
					entity = new Entity();
					MapTo( dbEntity, entity );
					list.Add(entity);
				}
			}
			return list;
		}

		/// <summary>
		/// Primary purpose is to check if a proposed relationship already exists
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="orgId"></param>
		/// <param name="roleId"></param>
		/// <param name="issuedCredentialId"></param>
		/// <returns></returns>
		private static bool DoesQualityAssuranceActionProfileExist( int entityId, Guid agentUid, int roleId)
		{
			QualityAssuranceActionProfile item = new QualityAssuranceActionProfile();
			using ( var context = new EM.CTIEntities() )
			{
				DBentity entity = context.Entity_QA_Action.FirstOrDefault( s => s.EntityId == entityId
						&& s.AgentUid == agentUid
						&& s.RelationshipTypeId == roleId
						);

				if ( entity != null && entity.Id > 0 )
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}


		//private static void MapTo( DBentity from, Entity to )
		//{
		//	//QualityAssuranceActionProfile to = new QualityAssuranceActionProfile();
		//	to.Id = from.Id;
		//	to.RowId = from.RowId;

		//	to.ParentId = from.EntityId;
		//	to.ActingAgentUid = from.AgentUid;
		//	to.RoleTypeId = from.RelationshipTypeId;
		//	to.Description = from.Description;

		//	string relation = "";
		//	if ( from.Codes_CredentialAgentRelationship != null )
		//	{
		//		//using direct or reverse? - believe the latter
		//		relation = from.Codes_CredentialAgentRelationship.ReverseRelation;
		//	}

		//	to.IssuedCredentialId = from.IssuedCredentialId != null ? ( int ) from.IssuedCredentialId : 0;

		//	//to.IssuedCredential = new Credential() { Id = from.Credential1.Id, RowId = from.Credential1.RowId, Name = from.Credential1.Name };

		//	//to.ActingAgent = new Organization() { Id = from.Organization.Id, RowId = from.Organization.RowId, Name = from.Organization.Name };

		//	//TODO - get agent info from a view
		//	to.ProfileSummary = string.Format( "Agent - TBD ,credential:{0}", to.IssuedCredentialId );
		//	//OR
		//	//to.ProfileSummary = string.Format( "{0} {1} this credential", from.Organization.Name, relation );
		//	to.ProfileName = to.ProfileSummary;

		//	if ( IsValidDate( from.StartDate ) )
		//		to.StartDate = ( ( DateTime ) from.StartDate ).ToShortDateString();
		//	if ( IsValidDate( from.EndDate ) )
		//		to.EndDate = ( ( DateTime ) from.EndDate ).ToShortDateString();

		//	if ( IsValidDate( from.Created ) )
		//		to.Created = ( DateTime ) from.Created;
		//	to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
		//	if ( IsValidDate( from.LastUpdated ) )
		//		to.LastUpdated = ( DateTime ) from.LastUpdated;
		//	to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;
		//}

		private static void MapTo( Views.Entity_QAAction_Summary from, Entity to )
		{
			
			to.Id = from.EntityQAActionId;
			to.RowId = from.RowId;

			to.ParentId = from.EntityId;
			to.ActingAgentUid = from.AgentUid;
			to.RoleTypeId = from.RelationshipTypeId;
			to.Description = from.Description;

			string relation = from.Relationship;
			
			to.IssuedCredentialId = from.IssuedCredentialId;

			to.IssuedCredential = new Credential() { Id = (int)from.IssuedCredentialId, RowId = from.CredentialRowId, Name = from.CredentialName };

			to.ActingAgent = new Organization() { Id = from.AgentRelativeId, 
				RowId = from.AgentUid, 
				Name = from.AgentName };

			to.ProfileSummary = string.Format( "{0} - {1}; credential:{1}", relation, from.AgentName, from.CredentialName );
			to.ProfileName = to.ProfileSummary;

			if ( IsValidDate( from.StartDate ) )
				to.StartDate = ( ( DateTime ) from.StartDate ).ToShortDateString();
			if ( IsValidDate( from.EndDate ) )
				to.EndDate = ( ( DateTime ) from.EndDate ).ToShortDateString();

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;
		}

		#endregion 


		#region role codes retrieval ==================
		//public static Enumeration GetCredentialAgentQAActions( bool isOrgToCredentialRole = true, string entityType = "Credential" )
		//{
		//	return GetCredentialOrgRolesCodes( isOrgToCredentialRole, 1, entityType );

		//}


		//private static Enumeration GetCredentialOrgRolesCodes( bool isInverseRole = true, int qaRoleState = 0, string entityType = "Credential" )
		//{
		//	Enumeration entity = new Enumeration();

		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		EM.Codes_PropertyCategory category = context.Codes_PropertyCategory
		//					.SingleOrDefault( s => s.Id == CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE );

		//		if ( category != null && category.Id > 0 )
		//		{
		//			entity.Id = category.Id;
		//			entity.Name = category.Title;
		//			entity.SchemaName = category.SchemaName;
		//			entity.Url = category.SchemaUrl;
		//			entity.Items = new List<EnumeratedItem>();

		//			EnumeratedItem val = new EnumeratedItem();
		//			//var sortedList = context.Codes_CredentialAgentRelationship
		//			//		.Where( s => s.IsActive == true && ( qaOnlyRoles == false || s.IsQARole == true) )
		//			//		.OrderBy( x => x.Title )
		//			//		.ToList();

		//			var Query = from P in context.Codes_CredentialAgentRelationship
		//					.Where( s => s.IsActive == true )
		//						select P;
		//			if ( qaRoleState == 1 ) //qa only
		//			{
		//				Query = Query.Where( p => p.IsQARole == true );
		//			}
		//			else if ( qaRoleState == 2 )
		//			{
		//				//this is state is for showing org roles for a credential.
		//				//16-06-01 mp - for now show qa and no qa, just skip agent to agent which for now is dept and subsiduary
		//				if ( entityType.ToLower() == "credential" )
		//					Query = Query.Where( p => p.IsEntityToAgentRole == true );
		//				else
		//					Query = Query.Where( p => p.IsQARole == false && p.IsEntityToAgentRole == true );
		//			}
		//			else //all
		//			{

		//			}
		//			Query = Query.OrderBy( p => p.Title );
		//			var results = Query.ToList();

		//			//add Select option
		//			//need to only do if for a dropdown, not a checkbox list
		//			if ( qaRoleState == 1 )
		//			{
		//				val = new EnumeratedItem();
		//				val.Id = 0;
		//				val.CodeId = val.Id;
		//				val.Name = "Select an Action";
		//				val.Description = "";
		//				val.SortOrder = 0;
		//				val.Value = val.Id.ToString();
		//				entity.Items.Add( val );
		//			}


		//			//foreach ( Codes_PropertyValue item in category.Codes_PropertyValue )
		//			foreach ( EM.Codes_CredentialAgentRelationship item in results )
		//			{
		//				val = new EnumeratedItem();
		//				val.Id = item.Id;
		//				val.CodeId = item.Id;
		//				val.Value = item.Id.ToString();//????
		//				val.Description = item.Description;

		//				if ( isInverseRole )
		//				{
		//					val.Name = item.ReverseRelation;
		//					//if ( string.IsNullOrWhiteSpace( entityType ) )
		//					//{
		//					//	//may not matter
		//					//	val.Description = string.Format( "Organization has {0} service.", item.ReverseRelation );
		//					//}
		//					//else
		//					//{
		//					//	val.Description = string.Format( "Organization {0} this {1}", item.ReverseRelation, entityType );
		//					//}
		//				}
		//				else
		//				{
		//					val.Name = item.Title;
		//					//val.Description = string.Format( "{0} is {1} by this Organization ", entityType, item.Title );
		//				}

		//				if ( ( bool ) item.IsQARole )
		//					val.Name += " (QA)";

		//				entity.Items.Add( val );
		//			}

		//		}
		//	}

		//	return entity;
		//}


		#endregion

	}
}
