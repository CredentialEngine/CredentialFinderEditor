using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.ProfileModels;
using Models.Common;
using DBentity = Data.Views.Credential_AgentRoleIdCSV;
using ThisEntity = Models.ProfileModels.OrganizationRoleProfile;
using EM = Data;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
using Utilities;

namespace Factories
{
	public class OrganizationRoleManager : BaseFactory
	{
		public static int CredentialToOrgRole_AccreditedBy = 1;
		public static int CredentialToOrgRole_ApprovedBy = 2;
		public static int CredentialToOrgRole_QualityAssuredBy = 3;
		public static int CredentialToOrgRole_ConferredBy = 4;
		public static int CredentialToOrgRole_CreatedBy = 5;
		public static int CredentialToOrgRole_OwnedBy = 6;
		public static int CredentialToOrgRole_OfferedBy = 7;
		public static int CredentialToOrgRole_EndorsedBy = 8;
		public static int CredentialToOrgRole_AssessedBy = 9;
		public static int CredentialToOrgRole_RecognizedBy = 10;
		public static int CredentialToOrgRole_RevokedBy = 11;
		public static int CredentialToOrgRole_RegulatedBy = 12;
		public static int CredentialToOrgRole_RenewalsBy = 13;
		public static int CredentialToOrgRole_UpdatedVersionBy = 14;


		public static int CredentialToOrgRole_MonitoredBy = 15;
		public static int CredentialToOrgRole_VerifiedBy = 16;
		public static int CredentialToOrgRole_ValidatedBy = 17;
		public static int CredentialToOrgRole_Contributor = 18;
		public static int CredentialToOrgRole_WIOAApproved = 19;

		#region OBSOLETE
		#region Organization Roles =================================================
		#region roles persistance ==================
		//[Obsolete]
		//public bool Credential_UpdateOrgRoles( Credential credential, ref List<string> messages, ref int count )
		//{
		//	//bool allowingMultipleOrgRoles = UtilityManager.GetAppKeyValue( "allowingMultipleOrgRoles", false );

		//	//if ( allowingMultipleOrgRoles )
		//	return Credential_UpdateOrgRoles2( credential, ref messages, ref count );
		//	//else
		//	//	return Credential_UpdateOrgRoles1( credential, ref messages, ref count );
		//}
		//public bool Credential_UpdateOrgRoles1( Credential credential, ref List<string> messages, ref int count )
		//{
		//	bool isValid = true;
		//	count = 0;

		//	if ( credential.OrganizationRole == null )
		//		credential.OrganizationRole = new List<ThisEntity>();

		//	//test 


		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		//loop thru input, check for changes to existing, and for adds
		//		foreach ( ThisEntity item in credential.OrganizationRole )
		//		{
		//			int codeId = item.RoleTypeId;

		//			if ( item.Id > 0 )
		//			{
		//				EM.Credential_AgentRelationship p = context.Credential_AgentRelationship.FirstOrDefault( s => s.Id == item.Id );
		//				if ( p != null && p.Id > 0 )
		//				{
		//					p.CredentialId = credential.Id;
		//					if ( codeId == 0 )
		//					{
		//						isValid = false;
		//						messages.Add( string.Format( "Error: a role was not entered. Select a role and try again. AgentId: {0}", item.ActingAgentId ) );
		//						continue;
		//					}
		//					p.RelationshipTypeId = codeId;
		//					p.IsActionType = false;
		//					//actually need to get the rowId!
		//					if ( p.OrgId != item.ActingAgentId )
		//					{
		//						//NOTE - need to handle agent!!!
		//						Organization org = OrganizationManager.Organization_Get( item.ActingAgentId, false );
		//						if ( org == null || org.Id == 0 )
		//						{
		//							isValid = false;
		//							messages.Add( string.Format( "Error: the selected organization was not found: {0}", item.ActingAgentId ) );
		//							continue;
		//						}
		//						p.AgentUid = org.RowId;
		//					}
		//					p.OrgId = item.ActingAgentId;
		//					p.URL = item.Url;
		//					p.Description = item.Description;

		//					if ( HasStateChanged( context ) )
		//					{
		//						p.LastUpdated = System.DateTime.Now;
		//						p.LastUpdatedById = credential.LastUpdatedById;
		//						context.SaveChanges();
		//						count++;
		//					}
		//				}
		//				else
		//				{
		//					//error should have been found
		//					isValid = false;
		//					messages.Add( string.Format( "Error: the requested role was not found: recordId: {0}", item.Id ) );
		//				}
		//			}
		//			else
		//			{
		//				bool isEmpty = false;
		//				if ( CredentialOrgRole_Add( credential.Id, item.ActingAgentId, codeId, credential.LastUpdatedById, ref messages, ref isEmpty, item.Url, item.Description ) == false )
		//				{
		//					if ( !isEmpty )
		//						isValid = false;
		//				}

		//				else
		//					count++;
		//			}

		//		}

		//	}
		//	//status = string.Join( ",", messages.ToArray() );
		//	return isValid;
		//}[Obsolete]
		//[Obsolete]
		//public bool Credential_UpdateOrgRoles2( Credential credential, ref List<string> messages, ref int count )
		//{
		//	bool isValid = true;
		//	count = 0;
		//	string statusMessage = "";
		//	if ( credential.OrganizationRole == null )
		//		credential.OrganizationRole = new List<ThisEntity>();
		//	ThisEntity entity = new ThisEntity();

		//	List<ThisEntity> list = FillListOneRolePerOrg( credential.OrganizationRole, credential.Id, ref messages );
		//	if ( messages.Count > 0 )
		//		return false;

		//	using ( var context = new EM.CTIEntities() )
		//	{

		//		//get all existing roles for the parent
		//		var results = CredentialAgentRole_GetAll( credential.Id );
		//		#region deletes/updates check
		//		var deleteList = from existing in results
		//						 join item in list
		//						 on new { existing.ActingAgentId, existing.RoleTypeId }
		//						 equals new { item.ActingAgentId, item.RoleTypeId }
		//						 into joinTable
		//						 from result in joinTable.DefaultIfEmpty( new ThisEntity { ActingAgentId = 0, ParentId = 0, Id = 0 } )
		//						 select new { ActingAgentId = existing.ActingAgentId, DeleteId = existing.Id, ItemId = ( result.RoleTypeId ) };

		//		foreach ( var v in deleteList )
		//		{

		//			if ( v.ItemId == 0 )
		//			{
		//				//delete item
		//				if ( CredentialOrgRole_Delete( v.DeleteId, ref statusMessage ) == false )
		//				{
		//					messages.Add( statusMessage );
		//					isValid = false;
		//				}

		//			}
		//		}
		//		#endregion

		//		#region new items
		//		//should only empty ids, where not in current list, so should be adds
		//		var newList = from item in list
		//					  join existing in results
		//							 on new { item.ActingAgentId, item.RoleTypeId }
		//						 equals new { existing.ActingAgentId, existing.RoleTypeId }
		//							into joinTable
		//					  from addList in joinTable.DefaultIfEmpty( new ThisEntity { Id = 0, ActingAgentId = 0, RoleTypeId = 0 } )
		//					  select new { ActingAgentId = item.ActingAgentId, RoleTypeId = item.RoleTypeId, ExistingId = addList.Id };
		//		foreach ( var v in newList )
		//		{
		//			if ( v.ExistingId == 0 )
		//			{
		//				bool isEmpty = false;
		//				if ( CredentialOrgRole_Add( credential.Id, v.ActingAgentId, v.RoleTypeId, credential.LastUpdatedById, ref messages, ref isEmpty ) == false )
		//				{
		//					if ( !isEmpty )
		//						isValid = false;
		//				}

		//			}
		//		}
		//		#endregion

		//	}
		//	return isValid;
		//}

		/// <summary>
		/// Add/Update an OrganizationRoleProfile 
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="credentialId"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		//public bool Credential_UpdateOrganizationRoleProfile( ThisEntity entity, int credentialId, int userId, ref List<string> messages )
		//{
		//	bool isValid = true;

		//	string statusMessage = "";
		//	//use existing method, so create a list of one item and call
		//	List<ThisEntity> profiles = new List<ThisEntity>();
		//	profiles.Add( entity );
		//	List<ThisEntity> flattenedList = FillListOneRolePerOrg( profiles, credentialId, ref messages );
		//	if ( messages.Count > 0 )
		//		return false;

		//	using ( var context = new EM.CTIEntities() )
		//	{

		//		//get all existing roles for the parent, and agent combination
		//		//TODO - do we have ActingAgentId populated??????
		//		var results = CredentialAgentRole_GetAll( credentialId, entity.ActingAgentId );
		//		#region deletes/updates check
		//		var deleteList = from existing in results
		//						 join item in flattenedList
		//						 on new { existing.ActingAgentId, existing.RoleTypeId }
		//						 equals new { item.ActingAgentId, item.RoleTypeId }
		//						 into joinTable
		//						 from result in joinTable.DefaultIfEmpty( new ThisEntity { ActingAgentId = 0, ParentId = 0, Id = 0 } )
		//						 select new { ActingAgentId = existing.ActingAgentId, DeleteId = existing.Id, ItemId = ( result.RoleTypeId ) };

		//		foreach ( var v in deleteList )
		//		{

		//			if ( v.ItemId == 0 )
		//			{
		//				//delete item
		//				if ( CredentialOrgRole_Delete( v.DeleteId, ref statusMessage ) == false )
		//				{
		//					messages.Add( statusMessage );
		//					isValid = false;
		//				}

		//			}
		//		}
		//		#endregion

		//		#region new items
		//		//should only empty ids, where not in current flattenedList, so should be adds
		//		var newList = from item in flattenedList
		//					  join existing in results
		//							 on new { item.ActingAgentId, item.RoleTypeId }
		//						 equals new { existing.ActingAgentId, existing.RoleTypeId }
		//							into joinTable
		//					  from addList in joinTable.DefaultIfEmpty( new ThisEntity { Id = 0, ActingAgentId = 0, RoleTypeId = 0 } )
		//					  select new { ActingAgentId = item.ActingAgentId, RoleTypeId = item.RoleTypeId, ExistingId = addList.Id };
		//		foreach ( var v in newList )
		//		{
		//			if ( v.ExistingId == 0 )
		//			{
		//				bool isEmpty = false;
		//				if ( CredentialOrgRole_Add( credentialId, v.ActingAgentId, v.RoleTypeId, userId, ref messages, ref isEmpty ) == false )
		//				{
		//					if ( !isEmpty )
		//						isValid = false;
		//				}

		//			}
		//		}
		//		#endregion

		//	}
		//	return isValid;
		//}


		/// <summary>
		/// Custom method only handle the creator and owned by roles
		/// </summary>
		/// <param name="credential"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		//[Obsolete]
		//public bool CredentialOwnerRolesUpdate( Credential credential, ref List<string> messages )
		//{
		//	bool isValid = true;
		//	List<EM.Credential_AgentRelationship> list = new List<EM.Credential_AgentRelationship>();

		//	int orgCreatorId = credential.CreatorOrganizationId;
		//	int orgOwnerId = credential.OwnerOrganizationId;
		//	bool orgCreatorExisted = false;
		//	bool orgOwnerExisted = false;

		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		//get existing relationships
		//		//however, not sure the credential will contain all relationships
		//		//will probably need some context specific methods
		//		//TBD - if we are assuming only one selection, then 
		//		List<EM.Credential_AgentRelationship> results = context.Credential_AgentRelationship
		//				.Where( s => s.CredentialId == credential.Id
		//				&& ( s.RelationshipTypeId == CredentialToOrgRole_CreatedBy
		//				|| s.RelationshipTypeId == CredentialToOrgRole_OwnedBy ) )
		//				.OrderBy( s => s.RelationshipTypeId )
		//				.ToList();

		//		if ( results != null && results.Count > 0 )
		//		{
		//			foreach ( EM.Credential_AgentRelationship item in results )
		//			{
		//				if ( item.RelationshipTypeId == CredentialToOrgRole_CreatedBy )
		//				{
		//					orgCreatorExisted = true;
		//					if ( orgCreatorId == item.OrgId )
		//					{
		//						//no action
		//					}
		//					else if ( orgCreatorId > 0 )
		//					{
		//						//not equal, so a change
		//						item.OrgId = orgCreatorId;

		//						//NOTE - need to handle agent!!!
		//						Organization org = OrganizationManager.Organization_Get( orgCreatorId, false );
		//						if ( org == null || org.Id == 0 )
		//						{
		//							isValid = false;
		//							//messages.Add( string.Format( "Error: the selected organization was not found: {0}", item.OrganizationId ) );
		//							LoggingHelper.DoTrace( 5, string.Format( "OrganizationRoleManager.CredentialOwnerRolesUpdate the creator organization was not found, for credential: {0}, OrgId:{1}", credential.Id, orgCreatorId ) );
		//							messages.Add( string.Format( "Error: the creator organization was not found, for credential: {0}, OrgId:{1}", credential.Id, orgCreatorId ) );
		//							continue;
		//						}
		//						item.AgentUid = org.RowId;

		//						item.LastUpdated = DateTime.Now;
		//						item.LastUpdatedById = credential.LastUpdatedById;
		//						context.SaveChanges();
		//					}
		//					else
		//					{
		//						//delete
		//						context.Credential_AgentRelationship.Remove( item );
		//						int count = context.SaveChanges();
		//						if ( count > 0 )
		//						{
		//							LoggingHelper.DoTrace( 5, string.Format( "For credential: {0}, removed CredentialToOrgRole_CreatedBy: OrgId:{1}", credential.Id, item.OrgId ) );
		//							isValid = true;
		//						}
		//					}
		//				}
		//				else if ( item.RelationshipTypeId == CredentialToOrgRole_OwnedBy )
		//				{
		//					orgOwnerExisted = true;
		//					if ( orgOwnerId == item.OrgId )
		//					{
		//						//no action
		//					}
		//					else if ( orgOwnerId > 0 )
		//					{
		//						//not equal, so a change
		//						item.OrgId = orgOwnerId;
		//						//NOTE - need to handle agent!!!
		//						Organization org = OrganizationManager.Organization_Get( orgOwnerId, false );
		//						if ( org == null || org.Id == 0 )
		//						{
		//							isValid = false;
		//							LoggingHelper.DoTrace( 5, string.Format( "OrganizationRoleManager.CredentialOwnerRolesUpdate the owner organization was not found, for credential: {0}, OrgId:{1}", credential.Id, orgOwnerId ) );
		//							messages.Add( string.Format( "Error: the owner organization was not found, for credential: {0}, OrgId:{1}", credential.Id, orgOwnerId ) );
		//							continue;
		//						}
		//						item.AgentUid = org.RowId;

		//						item.LastUpdated = DateTime.Now;
		//						item.LastUpdatedById = credential.LastUpdatedById;
		//						context.SaveChanges();
		//					}
		//					else
		//					{
		//						//delete
		//						context.Credential_AgentRelationship.Remove( item );
		//						int count = context.SaveChanges();
		//						if ( count > 0 )
		//						{
		//							LoggingHelper.DoTrace( 5, string.Format( "For credential: {0}, removed CredentialToOrgRole_OwnedBy: OrgId:{1}", credential.Id, item.OrgId ) );
		//							isValid = true;
		//						}
		//					}
		//				}
		//			}

		//		}
		//		bool isEmpty = false;
		//		if ( !orgCreatorExisted && orgCreatorId > 0 )
		//		{
		//			CredentialOrgRole_Add( credential.Id, orgCreatorId, CredentialToOrgRole_CreatedBy, credential.LastUpdatedById, ref messages, ref isEmpty );
		//		}

		//		if ( !orgOwnerExisted && orgOwnerId > 0 )
		//		{
		//			CredentialOrgRole_Add( credential.Id, orgOwnerId, CredentialToOrgRole_OwnedBy, credential.LastUpdatedById, ref messages, ref isEmpty );
		//		}
		//	}

		//	return isValid;
		//}
		///// <summary>
		///// Add a single Credential to org relationship ==> not complete
		///// </summary>
		///// <param name="credentialId"></param>
		///// <param name="orgId"></param>
		///// <param name="roleId"></param>
		///// <param name="userId"></param>
		///// <param name="status"></param>
		///// <returns></returns>
		//private bool CredentialOrgRole_Add( int credentialId, int agentId, int roleId, int userId, ref List<string> messages, ref bool isEmpty, string url = "", string description = "" )
		//{
		//	bool isValid = true;
		//	//assume if all empty, then ignore
		//	if ( credentialId == 0 || ( agentId == 0 && roleId == 0 ) )
		//	{
		//		isEmpty = true;
		//		return true;
		//	}
		//	if ( agentId > 0 && roleId == 0 )
		//	{
		//		messages.Add( "Error: invalid request, please select a role." );
		//		return false;
		//	}
		//	else if ( agentId == 0 && roleId > 0 )
		//	{
		//		messages.Add( "Error: invalid request, please select an organization." );
		//		return false;
		//	}

		//	//CredentialAgentRelationship entity = AgentCredentialRoleGet( credentialId, agentId, roleId );
		//	if ( DoesAgentCredentialRoleExist( credentialId, agentId, roleId ))
		//	{
		//		messages.Add( "Error: the selected relationship already exists!" );
		//		return false;
		//	}
		//	//TODO - need to handle agent
		//	Organization org = OrganizationManager.Organization_Get( agentId, false );
		//	if ( org == null || org.Id == 0 )
		//	{
		//		messages.Add( "Error: the selected organization was not found!" );
		//		LoggingHelper.DoTrace( 5, string.Format( "OrganizationRoleManager.CredentialOrgRole_Add the organization was not found, for credential: {0}, AgentId:{1}, RoleId: {2}", credentialId, agentId, roleId ) );
		//		return false;
		//	}

		//	using ( var context = new EM.CTIEntities() )
		//	{

		//		//add
		//		EM.Credential_AgentRelationship car = new EM.Credential_AgentRelationship();
		//		car.CredentialId = credentialId;
		//		car.OrgId = agentId;
		//		car.AgentUid = org.RowId;
		//		car.RelationshipTypeId = roleId;
		//		car.IsActionType = false;
		//		car.URL = url;
		//		car.Description = description;

		//		car.Created = System.DateTime.Now;
		//		car.CreatedById = userId;
		//		car.LastUpdated = System.DateTime.Now;
		//		car.LastUpdatedById = userId;
		//		car.RowId = Guid.NewGuid();
		//		context.Credential_AgentRelationship.Add( car );

		//		// submit the change to database
		//		int count = context.SaveChanges();
		//	}

		//	return isValid;
		//}

		//[Obsolete]
		//public bool CredentialOrgRole_Delete( int recordId, ref string statusMessage )
		//{
		//	bool isValid = false;

		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		if ( recordId == 0 )
		//		{
		//			statusMessage = "Error - missing an identifier for the Agent Role";
		//			return false;
		//		}

		//		EM.Credential_AgentRelationship efEntity =
		//			context.Credential_AgentRelationship.SingleOrDefault( s => s.Id == recordId );
		//		if ( efEntity != null && efEntity.Id > 0 )
		//		{
		//			context.Credential_AgentRelationship.Remove( efEntity );
		//			int count = context.SaveChanges();
		//			if ( count > 0 )
		//			{
		//				isValid = true;
		//			}
		//		}
		//		else
		//		{
		//			statusMessage = string.Format( "Agent role record was not found: {0}", recordId );
		//			isValid = false;
		//		}
		//	}

		//	return isValid;
		//}

		/// <summary>
		/// Delete all credential roles for the the agent
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="agentUid"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		//public bool CredentialOrgRole_Delete( int credentialId, Guid agentUid, ref string statusMessage )
		//{
		//	bool isValid = true;

		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		if ( credentialId == 0 || !IsValidGuid(agentUid))
		//		{
		//			statusMessage = "Error - missing identifiers, please provide proper keys.";
		//			return false;
		//		}

		//		context.Credential_AgentRelationship.RemoveRange( context.Credential_AgentRelationship.Where( s => s.CredentialId == credentialId && s.AgentUid == agentUid ) );
		//		int count = context.SaveChanges();
		//		if ( count > 0 )
		//		{
		//			isValid = true;
		//		}
		//		else
		//		{
		//			statusMessage = string.Format( "Delete failed, Agent role record(s) not found for: credId: {0}, agentUid: {1}", credentialId, agentUid );
		//			isValid = false;
		//		}
		//	}

		//	return isValid;
		//}

	//	public bool Credential_AgentRelationship_Delete( int recordId, ref string statusMessage )
	//	{
	//		bool isValid = false;
	//		if ( recordId == 0 )
	//		{
	//			statusMessage = "Error - missing identifier, please select a record.";
	//			return false;
	//		}

	//		using ( var context = new EM.CTIEntities() )
	//		{
	//			EM.Credential_AgentRelationship efEntity =
	//context.Credential_AgentRelationship.SingleOrDefault( s => s.Id == recordId );
	//			if ( efEntity != null && efEntity.Id > 0 )
	//			{
	//				context.Credential_AgentRelationship.Remove( efEntity );
	//				int count = context.SaveChanges();
	//				if ( count > 0 )
	//				{
	//					isValid = true;
	//				}
	//			}
	//			else
	//			{
	//				statusMessage = string.Format( "Record was not found: {0}", recordId );
	//				isValid = false;
	//			}
	//		}

	//		return isValid;
	//	}

		#endregion
		#endregion 

		#region roles retrieval - Obsolete ==================

		/// <summary>
		/// Get all the distinct org role profiles
		/// </summary>
		/// <param name="parentId"></param>
		/// <param name="agentId"></param>
		/// <returns></returns>
		//[Obsolete]
		//private static List<ThisEntity> CredentialAgentRole_GetAll( int parentId, int agentId = 0 )
		//{
		//	ThisEntity p = new ThisEntity();
		//	List<ThisEntity> list = new List<ThisEntity>();
		//	using ( var context = new ViewContext() )
		//	{
		//		List<Views.CredentialAgentRelationships_Summary> roles = context.CredentialAgentRelationships_Summary
		//			.Where( s => s.CredentialId == parentId
		//			&& ( agentId == 0 || s.OrgId == agentId ) )
		//			.OrderBy( s => s.OrganizationName ).ThenBy( s => s.RelationshipType )
		//			.ToList();

		//		foreach ( Views.CredentialAgentRelationships_Summary entity in roles )
		//		{
		//			p = new ThisEntity();
		//			p.Id = entity.CredentialAgentRelationshipId;
		//			p.ParentUid = entity.ParentUid;
		//			p.ParentTypeId = CodesManager.ENTITY_TYPE_CREDENTIAL;

		//			p.ActingAgentUid = ( Guid ) entity.AgentUid;
		//			p.ActingAgentId = entity.OrgId;
		//			p.RoleTypeId = entity.RelationshipTypeId;
		//			//p.ActingAgent = new Organization()
		//			//{
		//			//	Id = entity.OrgId,
		//			//	RowId = p.ActingAgentUid,
		//			//	Name = entity.OrganizationName
		//			//};

		//			string relation = entity.RelationshipType;

		//			//may be included now, but with addition of person, and use of agent, it won't
		//			//TODO - replace from view, when added
		//			Organization agent = OrganizationManager.Agent_Get( p.ActingAgentUid );

		//			p.ProfileSummary = string.Format( "{0} {1} this {2}", agent.Name, relation, "credential" );


		//			if ( IsValidDate( entity.Created ) )
		//				p.Created = ( DateTime ) entity.Created;
		//			p.CreatedById = entity.CreatedById == null ? 0 : ( int ) entity.CreatedById;
		//			if ( IsValidDate( entity.LastUpdated ) )
		//				p.LastUpdated = ( DateTime ) entity.LastUpdated;
		//			p.LastUpdatedById = entity.LastUpdatedById == null ? 0 : ( int ) entity.LastUpdatedById;

		//			list.Add( p );
		//		}

		//	}
		//	return list;

		//} //
		/// <summary>
		/// Loop through all the org roles, and fill a work list for update logic
		/// That is: flatten class with the array of roles for an org into one unique role for the org + roleId for the parent
		/// </summary>
		/// <param name="profiles"></param>
		/// <param name="parentId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		//[Obsolete]
		//private List<ThisEntity> FillListOneRolePerOrg( List<ThisEntity> profiles, int parentId, ref List<string> messages )
		//{
		//	ThisEntity entity = new ThisEntity();
		//	List<ThisEntity> list = new List<ThisEntity>();
		//	if ( parentId == 0 )
		//		return list;
		//	int orgId = 0;
		//	Guid agentUid;
		//	bool isValidAgent = false;

		//	foreach ( ThisEntity item in profiles )
		//	{
		//		//TODO - ensure we have ActingAgentId filled, version Id, or something else
		//		orgId = item.ActingAgentId;
		//		agentUid = item.ActingAgentUid;
		//		if ( orgId > 0 || IsGuidValid( agentUid ) )
		//			isValidAgent = true;
		//		else
		//			isValidAgent = false;

		//		//loop thru the roles
		//		if ( item.RoleType != null && item.RoleType.Items.Count > 0 )
		//		{
		//			if ( !isValidAgent )
		//			{
		//				//roles, no agent
		//				messages.Add( "Invalid request, please select an agent for selected roles." );
		//				continue;
		//			}
		//			foreach ( EnumeratedItem e in item.RoleType.Items )
		//			{
		//				entity = new ThisEntity();
		//				entity.ParentId = parentId;
		//				entity.ActingAgentId = orgId;
		//				entity.ActingAgentUid = agentUid; //

		//				entity.RoleTypeId = e.Id;
		//				list.Add( entity );
		//			}
		//		}
		//		else
		//		{
		//			//no roles
		//			if ( isValidAgent )
		//			{
		//				messages.Add( "Invalid request, please select one or more roles for this selected agent." );
		//			}
		//		}


		//	}
		//	return list;
		//}
		
		/// <summary>
		/// Attempt to get a credential role, to determine if one exists
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="orgId"></param>
		/// <param name="roleId"></param>
		/// <returns></returns>
		//private static bool DoesAgentCredentialRoleExist( int credentialId, int orgId, int roleId )
		//{
		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		EM.Credential_AgentRelationship entity = context.Credential_AgentRelationship.FirstOrDefault( s => s.CredentialId == credentialId
		//				&& s.OrgId == orgId
		//				&& s.RelationshipTypeId == roleId
		//				&& s.IsActionType == false);
		//		if ( entity != null && entity.Id > 0 )
		//		{
		//			return true;
		//		}
		//		else
		//		{
		//			return false;
		//		}
		//	}
		//}
		//private static CredentialAgentRelationship AgentCredentialRoleGet( int credentialId, int orgId, int roleId )
		//{
		//	CredentialAgentRelationship item = new CredentialAgentRelationship();
		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		EM.Credential_AgentRelationship entity = context.Credential_AgentRelationship.FirstOrDefault( s => s.CredentialId == credentialId
		//				&& s.OrgId == orgId
		//				&& s.RelationshipTypeId == roleId );
		//		if ( entity != null && entity.Id > 0 )
		//		{
		//			item.Id = entity.Id;
		//			item.ParentId = entity.CredentialId;
		//			item.OrganizationId = entity.OrgId;
		//			item.RelationshipId = entity.RelationshipTypeId;
		//			item.AgentUid = entity.AgentUid != null ? ( Guid ) entity.AgentUid : Guid.Empty;

		//			//item.TargetOrganization = entity.Organization;
		//		}
		//		else
		//		{
		//			item.Id = 0;
		//		}

		//	}
		//	return item;

		//}


		/// <summary>
		/// get all roles with one role per record.
		/// </summary>
		/// <param name="fromCredential"></param>
		/// <param name="credential"></param>
		//public static void FillAllOrgRolesForCredential( EM.Credential fromCredential, Credential credential )
		//{
		//	//start by assuming all roles have been read
		//	if ( fromCredential.Credential_AgentRelationship == null || fromCredential.Credential_AgentRelationship.Count == 0 )
		//	{
		//		return;
		//	}

		//	credential.OrganizationRole = new List<ThisEntity>();
		//	credential.QualityAssuranceAction = new List<QualityAssuranceActionProfile>();

		//	foreach ( EM.Credential_AgentRelationship item in fromCredential.Credential_AgentRelationship )
		//	{
		//		bool isActionType = item.IsActionType == null ? false : ( bool ) item.IsActionType;

		//		if ( item.IssuedCredentialId > 0 || isActionType )
		//		{
		//			MapAgentToQAAction( credential, item );
		//		}
		//		else
		//		{
		//			credential.OrganizationRole.Add( MapAgentToOrgRole( item, "credential" ) );
		//		}


		//		//TODO - these two steps will be obsoleted by new editor
		//		if ( item.RelationshipTypeId == CredentialToOrgRole_CreatedBy )
		//			credential.CreatorOrganizationId = item.OrgId;

		//		if ( item.RelationshipTypeId == CredentialToOrgRole_OwnedBy )
		//			credential.OwnerOrganizationId = item.OrgId;

		//	}


		//}
		//public static void Fill_QAActionsForCredential( EM.Credential fromCredential, Credential credential )
		//{
		//	//start by assuming all roles have been read
		//	if ( fromCredential.Credential_AgentRelationship == null || fromCredential.Credential_AgentRelationship.Count == 0 )
		//	{
		//		return;
		//	}

		//	//credential.OrganizationRole = new List<ThisEntity>();
		//	credential.QualityAssuranceAction = new List<QualityAssuranceActionProfile>();

		//	foreach ( EM.Credential_AgentRelationship item in fromCredential.Credential_AgentRelationship )
		//	{
		//		bool isActionType = item.IsActionType == null ? false : ( bool ) item.IsActionType;

		//		if ( item.IssuedCredentialId > 0 || isActionType )
		//		{
		//			MapAgentToQAAction( credential, item );
		//		}
		//		else
		//		{
		//			//MapAgentToOrgRole( credential, item );
		//			//credential.OrganizationRole.Add( MapAgentToOrgRole( item, "credential" ) );
		//		}

		//		//if ( item.RelationshipTypeId == CredentialToOrgRole_CreatedBy )
		//		//	credential.CreatorOrganizationId = item.OrgId;

		//		//if ( item.RelationshipTypeId == CredentialToOrgRole_OwnedBy )
		//		//	credential.OwnerOrganizationId = item.OrgId;

		//	}


		//}

		/// <summary>
		/// Get all org roles.
		/// Multiple roles are possible. These will be read and 
		/// </summary>
		/// <param name="fromCredential"></param>
		/// <param name="credential"></param>
		//public static void Credential_FillAllOrgRolesAsEnumeration( EM.Credential fromCredential, Credential credential )
		//{
		//	//start by assuming all roles have been read
		//	if ( fromCredential.Credential_AgentRelationship == null
		//		|| fromCredential.Credential_AgentRelationship.Count == 0 )
		//	{
		//		return;
		//	}

		//	credential.OrganizationRole = new List<ThisEntity>();
		//	//credential.QualityAssuranceAction = new List<QualityAssuranceActionProfile>();
		//	EnumeratedItem eitem = new EnumeratedItem();

		//	using ( var context = new ViewContext() )
		//	{
		//		List<Views.Credential_AgentRoleIdCSV> agentRoles = context.Credential_AgentRoleIdCSV
		//			.Where( s => s.CredentialId == credential.Id )
		//			.ToList();

		//		foreach ( Views.Credential_AgentRoleIdCSV item in agentRoles )
		//		{
		//			ThisEntity orp = new ThisEntity();
		//			//warning for purposes of the editor, need to set the object id to the orgId, and rowId from the org
		//			orp.Id = item.OrgId;
		//			orp.RowId = ( Guid ) item.AgentUid;

		//			orp.ParentId = credential.Id;
		//			orp.ActingAgentUid = ( Guid ) item.AgentUid;
		//			orp.ActingAgentId = item.OrgId;
		//			orp.ActingAgent = new Organization()
		//			{
		//				Id = item.OrgId,
		//				RowId = orp.ActingAgentUid
		//			};
		//			orp.ProfileSummary = item.Name;

		//			orp.AgentRole = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE );
		//			//???
		//			orp.AgentRole.ParentId = credential.Id;
		//			orp.AgentRole.Items = new List<EnumeratedItem>();
		//			string[] roles = item.RoleIds.Split( ',' );
		//			foreach ( string role in roles )
		//			{
		//				eitem = new EnumeratedItem();
		//				//??
		//				eitem.Id = int.Parse( role );
		//				//not used here
		//				eitem.RecordId = int.Parse( role );
		//				eitem.CodeId = int.Parse( role );
		//				eitem.Value = role.Trim();

		//				eitem.Selected = true;
		//				orp.AgentRole.Items.Add( eitem );

		//				if ( Int32.Parse( role ) == CredentialToOrgRole_CreatedBy )
		//					credential.CreatorOrganizationId = item.OrgId;

		//				else if ( Int32.Parse( role ) == CredentialToOrgRole_OwnedBy )
		//					credential.OwnerOrganizationId = item.OrgId;
		//			}

		//			credential.OrganizationRole.Add( orp );
		//		}
		//		if ( credential.OrganizationRole.Count > 0 )
		//		{
		//			var Query = ( from roles in credential.OrganizationRole.OrderBy( p => p.ProfileSummary )
		//						  select roles ).ToList();
		//			credential.OrganizationRole = Query;
		//			//var Query = from roles in credential.OrganizationRole select roles;
		//			//Query = Query.OrderBy( p => p.ProfileSummary );
		//			//credential.OrganizationRole = Query.ToList();
		//		}
		//	}
		//}

		//public static ThisEntity GetCredentialOrgRoles_AsEnumeration( int credentialId, int orgId )
		//{

		//	ThisEntity orp = new ThisEntity();
		//	EnumeratedItem eitem = new EnumeratedItem();

		//	using ( var context = new ViewContext() )
		//	{
		//		DBentity item = context.Credential_AgentRoleIdCSV
		//					.SingleOrDefault( s => s.CredentialId == credentialId && s.OrgId == orgId);

		//		if ( item != null && item.CredentialId > 0 )
		//		{
		//			//warning for purposes of the editor, need to set the object id to the orgId, and rowId from the org
		//			orp.Id = item.OrgId;
		//			orp.RowId = ( Guid ) item.AgentUid;

		//			orp.ParentId = credentialId;
		//			orp.ActingAgentUid = ( Guid ) item.AgentUid;
		//			orp.ActingAgentId = item.OrgId;
		//			//TODO - do we still need this?
		//			orp.ActingAgent = new Organization()
		//			{
		//				Id = item.OrgId,
		//				RowId = orp.ActingAgentUid,
		//				Name = item.Name
		//			};
		//			orp.ProfileSummary = item.Name;
		//			orp.ProfileName = item.Name;

		//			orp.AgentRole = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE );
		//			//???
		//			orp.AgentRole.ParentId = credentialId;
		//			orp.AgentRole.Items = new List<EnumeratedItem>();
		//			string[] roles = item.RoleIds.Split( ',' );
		//			foreach ( string role in roles )
		//			{
		//				eitem = new EnumeratedItem();
		//				//??
		//				eitem.Id = int.Parse( role );
		//				//not used here
		//				eitem.RecordId = int.Parse( role );
		//				eitem.CodeId = int.Parse( role );
		//				eitem.Value = role.Trim();

		//				eitem.Selected = true;
		//				orp.AgentRole.Items.Add( eitem );
		//			}
		//		}

		//	}
		//	return orp;
		//}

		//private static ThisEntity MapAgentToOrgRole( EM.Credential_AgentRelationship entity, string targetEntity )
		//{
		//	ThisEntity orp = new ThisEntity();
		//	orp.Id = entity.Id;
		//	orp.RowId = ( Guid ) entity.RowId;
		//	orp.ParentId = entity.CredentialId;
		//	orp.Url = entity.URL;
		//	orp.Description = entity.Description;
		//	orp.SchemaTag = entity.Codes_CredentialAgentRelationship.SchemaTag;
		//	orp.ReverseSchemaTag = entity.Codes_CredentialAgentRelationship.ReverseSchemaTag;
		//	orp.ActingAgentId = entity.OrgId;
		//	if ( entity.AgentUid != null )
		//		orp.ActingAgentUid = ( Guid ) entity.AgentUid;
		//	orp.RoleTypeId = entity.RelationshipTypeId;
		//	string relation = "";
		//	if ( entity.Codes_CredentialAgentRelationship != null )
		//	{
		//		relation = entity.Codes_CredentialAgentRelationship.ReverseRelation;
		//	}

		//	//may be included now, but with addition of person, and use of agent, it won't
		//	if ( entity.Organization != null )
		//	{
		//		OrganizationManager.ToMap( entity.Organization, orp.TargetOrganization );
		//	}
		//	else
		//	{
		//		//get basic?
		//		orp.TargetOrganization = OrganizationManager.Organization_Get( orp.ActingAgentUid, false, false );
		//	}

		//	orp.ProfileSummary = string.Format( "{0} {1} this {2}", entity.Organization.Name, relation, targetEntity );
		//	if ( IsValidDate( entity.EffectiveDate ) )
		//		orp.DateEffective = ( ( DateTime ) entity.EffectiveDate ).ToShortDateString();
		//	else
		//		orp.DateEffective = "";

		//	if ( IsValidDate( entity.Created ) )
		//		orp.Created = ( DateTime ) entity.Created;
		//	orp.CreatedById = entity.CreatedById == null ? 0 : ( int ) entity.CreatedById;
		//	if ( IsValidDate( entity.LastUpdated ) )
		//		orp.LastUpdated = ( DateTime ) entity.LastUpdated;
		//	orp.LastUpdatedById = entity.LastUpdatedById == null ? 0 : ( int ) entity.LastUpdatedById;

		//	return orp;
		//}


		#endregion

		#region Quality Assurance Organization Roles ===================================================
		//public bool Credential_UpdateQAActions( Credential credential, ref List<string> messages, ref int count )
		//{
		//	bool isValid = true;
		//	bool isEmpty = false;
		//	int newId = 0;
		//	count = 0;
		//	//List<string> messages = new List<string>();

		//	if ( credential.QualityAssuranceAction == null )
		//		credential.QualityAssuranceAction = new List<QualityAssuranceActionProfile>();

		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		//loop thru input, check for changes to existing, and for adds
		//		foreach ( QualityAssuranceActionProfile item in credential.QualityAssuranceAction )
		//		{
		//			isEmpty = false;
		//			//int codeId = CodesManager.GetEnumerationSelection( item.RoleType );
		//			//just in case
		//			item.ParentId = credential.Id;
		//			if ( IsQaRoleValid( item, ref isEmpty, ref messages ) == false )
		//			{
		//				isValid = false;
		//				continue;
		//			}

		//			int codeId = item.RoleTypeId;
		//			if ( item.Id > 0 )
		//			{
		//				EM.Credential_AgentRelationship p = context.Credential_AgentRelationship.FirstOrDefault( s => s.Id == item.Id );
		//				if ( p != null && p.Id > 0 )
		//				{
		//					p.CredentialId = credential.Id;

		//					p.RelationshipTypeId = codeId;
		//					if ( codeId == 0 )
		//					{
		//						isValid = false;
		//						messages.Add( string.Format( "Error: a role was not entered. Select a role and try again. AgentId: {0}", item.ActingAgentId ) );
		//						continue;
		//					}
		//					//actually need to get the rowId!
		//					if ( p.OrgId != item.ActingAgentId )
		//					{
		//						//NOTE - need to handle agent!!!
		//						Organization org = OrganizationManager.Organization_Get( item.ActingAgentId, false );
		//						if ( org == null || org.Id == 0 )
		//						{
		//							isValid = false;
		//							messages.Add( string.Format( "Error: the selected organization was not found: {0}", item.ActingAgentId ) );
		//							continue;
		//						}
		//						p.AgentUid = org.RowId;
		//					}
		//					p.OrgId = item.ActingAgentId;
		//					//should be covered now
		//					if ( item.IssuedCredentialId == 0 )
		//					{
		//						isValid = false;
		//						messages.Add( "Error: please select an issued credential from the organization" );
		//						continue;
		//					}
		//					p.IssuedCredentialId = item.IssuedCredentialId;

		//					DateTime date;
		//					if ( DateTime.TryParse( item.StartDate, out date ) )
		//						p.EffectiveDate = date;
		//					if ( DateTime.TryParse( item.EndDate, out date ) )
		//						p.EndDate = date;

		//					p.URL = item.Url;
		//					p.Description = item.Description;

		//					if ( HasStateChanged( context ) )
		//					{
		//						p.LastUpdated = System.DateTime.Now;
		//						p.LastUpdatedById = credential.LastUpdatedById;
		//						context.SaveChanges();
		//						count++;
		//					}
		//				}
		//				else
		//				{
		//					//error should have been found
		//					isValid = false;
		//					messages.Add( string.Format( "Error: the requested role was not found: recordId: {0}", item.Id ) );
		//				}
		//			}
		//			else
		//			{


		//				if ( Credential_QAAction_Add( item, credential.Id, credential.LastUpdatedById, ref isEmpty, ref messages ) == false )
		//					isValid = false;
		//				else
		//					count++;
		//			}
		//		}

		//	}
		//	//status = string.Join( ",", messages.ToArray() );
		//	return isValid;
		//}

		//public bool Credential_SaveQAActions( QualityAssuranceActionProfile entity, int userId, ref List<string> messages )
		//{
		//	bool isValid = true;
		//	bool isEmpty = false;


		//	if ( !IsQaRoleValid( entity, ref isEmpty, ref messages ) )
		//	{
		//		return false;
		//	}
		//	using ( var context = new EM.CTIEntities() )
		//	{

		//		if ( entity.Id > 0 )
		//		{
		//			EM.Credential_AgentRelationship p = context.Credential_AgentRelationship.FirstOrDefault( s => s.Id == entity.Id );
		//			if ( p != null && p.Id > 0 )
		//			{
		//				//should not be able to change the credentialId
		//				//p.CredentialId = entity.ParentId;
		//				p.RelationshipTypeId = entity.RoleTypeId;

		//				//actually need to get the rowId!
		//				//NOW, confirm we have it
		//				p.AgentUid = entity.ActingAgentUid;
		//				//if latter OK, dump the following
		//				//====  start  ====================================
		//				if ( p.OrgId != entity.ActingAgentId )
		//				{
		//					//NOTE - need to handle agent!!!
		//					//Organization org = OrganizationManager.Organization_Get( entity.ActingAgentId, false );
		//					//if ( org == null || org.Id == 0 )
		//					//{
		//					//	messages.Add( string.Format( "Error: the selected organization was not found: {0}", entity.ActingAgentId ) );
		//					//	return false;
		//					//}
		//					//p.AgentUid = org.RowId;
		//				}
		//				p.OrgId = entity.ActingAgentId;
		//				//====  end  ======================================

		//				p.IssuedCredentialId = entity.IssuedCredentialId;

		//				DateTime date;
		//				if ( DateTime.TryParse( entity.StartDate, out date ) )
		//					p.EffectiveDate = date;
		//				if ( DateTime.TryParse( entity.EndDate, out date ) )
		//					p.EndDate = date;

		//				p.URL = entity.Url;
		//				p.Description = entity.Description;

		//				if ( HasStateChanged( context ) )
		//				{
		//					p.LastUpdated = System.DateTime.Now;
		//					p.LastUpdatedById = userId;
		//					context.SaveChanges();

		//				}
		//			}
		//			else
		//			{
		//				//error should have been found
		//				isValid = false;
		//				messages.Add( string.Format( "Error: the requested role was not found: recordId: {0}", entity.Id ) );
		//			}
		//		}
		//		else
		//		{
		//			//TODO update code to assume credential id is passed in entity
		//			if ( Credential_QAAction_Add( entity, 0, userId, ref isEmpty, ref messages ) == false )
		//				isValid = false;
		//			else if ( isEmpty )
		//			{
		//				isValid = false;
		//				messages.Add( "Error: Please enter the required information" );
		//			}
		//		}


		//	}
		//	return isValid;
		//}
		//private bool IsQaRoleValid( QualityAssuranceActionProfile item, ref bool isEmpty, ref List<string> messages )
		//{
		//	bool isValid = true;
		//	if ( item.ParentId == 0 )
		//	{
		//		isValid = false;
		//		messages.Add( "Error: please edit a credential, and then add Quality Assurance roles" );
		//	}
		//	if ( item.RoleTypeId == 0 )
		//	{
		//		if ( item.AgentRole == null || item.AgentRole.Items.Count == 0 )
		//		{
		//			messages.Add( string.Format( "Error: a role was not entered. Select a role and try again. AgentId: {0}", item.ActingAgentId ) );
		//		}
		//		else
		//		{
		//			item.RoleTypeId = item.AgentRole.Items[ 0 ].Id;

		//		}
		//	}

		//	if ( item.ActingAgentId > 0 && item.RoleTypeId == 0 )
		//	{
		//		messages.Add( "Error: invalid request, please select a role." );
		//		isValid = false;
		//	}
		//	else if ( item.ActingAgentId == 0 && item.RoleTypeId > 0 )
		//	{
		//		messages.Add( "Error: invalid request, please select an agent." );
		//		isValid = false;
		//	}

		//	if ( item.IssuedCredentialId == 0 )
		//	{
		//		isValid = false;
		//		messages.Add( "Error: please select an issued credential from the agent" );
		//	}
		//	if ( !string.IsNullOrWhiteSpace( item.StartDate ) && !IsValidDate( item.StartDate ) )
		//	{
		//		isValid = false;
		//		messages.Add( "Error: please provide a valid Start Date" );
		//	}
		//	if ( !string.IsNullOrWhiteSpace( item.EndDate ) && !IsValidDate( item.EndDate ) )
		//	{
		//		isValid = false;
		//		messages.Add( "Error: please provide a valid End Date" );
		//	}
		//	return isValid;
		//}
		//private bool Credential_QAAction_Add( QualityAssuranceActionProfile item, int credentialId, int userId, ref bool isEmpty, ref List<string> messages )
		//{
		//	bool isValid = true;
		//	if ( item.ParentId == 0 && credentialId > 0 )
		//		item.ParentId = credentialId;

		//	//if all missing, assume that there was just a preselection
		//	if ( item.ParentId == 0 && item.ActingAgentId == 0 && item.RoleTypeId == 0 )
		//	{
		//		isEmpty = true;
		//		return true;
		//	}
		//	if ( !IsQaRoleValid( item, ref isEmpty, ref messages ) )
		//	{
		//		return false;
		//	}

		//	if ( DoesQualityAssuranceActionProfileExist( credentialId, item.ActingAgentId, item.RoleTypeId, item.IssuedCredentialId ) )
		//	{
		//		messages.Add( "Error: the selected action already exists!" );
		//		return false;
		//	}
		//	//TODO - need to handle agent
		//	Organization org = OrganizationManager.Organization_Get( item.ActingAgentId, false );
		//	if ( org == null || org.Id == 0 )
		//	{
		//		messages.Add( "Error: the selected organization was not found!" );
		//		LoggingHelper.DoTrace( 5, string.Format( "OrganizationRoleManager.CredentialOrgRole_Add the organization was not found, for credential: {0}, AgentId:{1}, RoleId: {2}", credentialId, item.ActingAgentId, item.RoleTypeId ) );
		//		return false;
		//	}

		//	using ( var context = new EM.CTIEntities() )
		//	{

		//		//add
		//		EM.Credential_AgentRelationship car = new EM.Credential_AgentRelationship();
		//		car.CredentialId = item.ParentId;
		//		car.OrgId = item.ActingAgentId;
		//		car.AgentUid = org.RowId;
		//		car.RelationshipTypeId = item.RoleTypeId;
		//		car.IsActionType = true;
		//		car.IssuedCredentialId = item.IssuedCredentialId;

		//		DateTime date;
		//		if ( DateTime.TryParse( item.StartDate, out date ) )
		//			car.EffectiveDate = date;
		//		if ( DateTime.TryParse( item.EndDate, out date ) )
		//			car.EndDate = date;
		//		car.Description = item.Description;
		//		car.URL = item.Url;
		//		car.Created = System.DateTime.Now;
		//		car.CreatedById = userId;
		//		car.LastUpdated = System.DateTime.Now;
		//		car.LastUpdatedById = userId;
		//		car.RowId = Guid.NewGuid();
		//		context.Credential_AgentRelationship.Add( car );

		//		// submit the change to database
		//		int count = context.SaveChanges();
		//		item.Id = car.Id;
		//		item.RowId = car.RowId;
		//	}

		//	return isValid;
		//}

		//public static QualityAssuranceActionProfile QualityAssuranceActionProfile_Get( int profileId )
		//{
		//	QualityAssuranceActionProfile item = new QualityAssuranceActionProfile();
		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		EM.Credential_AgentRelationship entity = context.Credential_AgentRelationship.FirstOrDefault( s => s.Id == profileId );

		//		if ( entity != null && entity.Id > 0 )
		//		{

		//			item.Id = entity.Id;
		//			item.RowId = entity.RowId;
		//			item.ParentId = entity.CredentialId;
		//			item.Url = entity.URL;
		//			item.Description = entity.Description;

		//			item.IssuedCredentialId = entity.IssuedCredentialId != null ? ( int ) entity.IssuedCredentialId : 0;
		//			item.IssuedCredential = new Credential() { Id = entity.Credential1.Id, RowId = entity.Credential1.RowId, Name = entity.Credential1.Name };

		//			item.ActingAgentUid = entity.AgentUid != null ? ( Guid ) entity.AgentUid : Guid.Empty;
		//			//TODO - will need to handle differently with person
		//			item.ActingAgent = new Organization() { Id = entity.Organization.Id, RowId = entity.Organization.RowId, Name = entity.Organization.Name };

		//			//TODO eliminate ,and use guid only
		//			item.ActingAgentId = entity.OrgId;

		//			item.IsQAActionRole = ( bool ) ( entity.IsActionType ?? false );
		//			item.RoleTypeId = entity.RelationshipTypeId;
		//			string relation = "";
		//			if ( entity.Codes_CredentialAgentRelationship != null )
		//			{
		//				relation = entity.Codes_CredentialAgentRelationship.ReverseRelation;
		//			}

		//			item.ProfileSummary = (entity.Organization.Name ?? "Agent")
		//								+ " - " + relation + " credential with issuance: "
		//								+ ( entity.Credential1.Name ?? "" );
		//			//OR
		//			item.ProfileSummary = string.Format( "{0} {1} this credential", entity.Organization.Name, relation );
		//			item.ProfileName = item.ProfileSummary;

		//			if ( IsValidDate( entity.EffectiveDate ) )
		//				item.StartDate = ( ( DateTime ) entity.EffectiveDate ).ToShortDateString();
		//			if ( IsValidDate( entity.EndDate ) )
		//				item.EndDate = ( ( DateTime ) entity.EndDate ).ToShortDateString();

		//			if ( IsValidDate( entity.Created ) )
		//				item.Created = ( DateTime ) entity.Created;
		//			item.CreatedById = entity.CreatedById == null ? 0 : ( int ) entity.CreatedById;
		//			if ( IsValidDate( entity.LastUpdated ) )
		//				item.LastUpdated = ( DateTime ) entity.LastUpdated;
		//			item.LastUpdatedById = entity.LastUpdatedById == null ? 0 : ( int ) entity.LastUpdatedById;
		//		}
		//		else
		//		{
		//			item.Id = 0;
		//		}

		//	}
		//	return item;

		//}

		///// <summary>
		///// Primary purpose is to check if a proposed relationship already exists
		///// </summary>
		///// <param name="credentialId"></param>
		///// <param name="orgId"></param>
		///// <param name="roleId"></param>
		///// <param name="issuedCredentialId"></param>
		///// <returns></returns>
		//private static bool DoesQualityAssuranceActionProfileExist( int credentialId, int orgId, int roleId, int issuedCredentialId )
		//{
		//	QualityAssuranceActionProfile item = new QualityAssuranceActionProfile();
		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		EM.Credential_AgentRelationship entity = context.Credential_AgentRelationship.FirstOrDefault( s => s.CredentialId == credentialId
		//				&& s.OrgId == orgId
		//				&& s.RelationshipTypeId == roleId
		//				&& s.IssuedCredentialId == issuedCredentialId
		//				);

		//		if ( entity != null && entity.Id > 0 )
		//		{
		//			return true;
		//		}
		//		else
		//		{
		//			return false;
		//		}
		//	}
		//}

		//private static QualityAssuranceActionProfile QualityAssuranceActionProfile_Get( int credentialId, int orgId, int roleId, int issuedCredentialId )
		//{
		//	QualityAssuranceActionProfile item = new QualityAssuranceActionProfile();
		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		EM.Credential_AgentRelationship entity = context.Credential_AgentRelationship.FirstOrDefault( s => s.CredentialId == credentialId
		//				&& s.OrgId == orgId
		//				&& s.RelationshipTypeId == roleId
		//				&& s.IssuedCredentialId == issuedCredentialId
		//				);

		//		if ( entity != null && entity.Id > 0 )
		//		{

		//			item.Id = entity.Id;
		//			item.RowId = entity.RowId;
		//			item.ParentId = entity.CredentialId;
		//			item.IsQAActionRole = ( bool ) ( entity.IsActionType ?? false );

		//			item.TargetOrganizationId = entity.OrgId;
		//			item.RoleTypeId = entity.RelationshipTypeId;
		//			item.IssuedCredentialId = entity.IssuedCredentialId != null ? ( int ) entity.IssuedCredentialId : 0;

		//			item.ActingAgentUid = entity.AgentUid != null ? ( Guid ) entity.AgentUid : Guid.Empty;

		//			item.ActingAgent = new Organization() { Id = entity.Organization.Id, RowId = entity.Organization.RowId, Name = entity.Organization.Name };

		//			string relation = "";
		//			if ( entity.Codes_CredentialAgentRelationship != null )
		//			{
		//				relation = entity.Codes_CredentialAgentRelationship.ReverseRelation;
		//			}
		//			item.ProfileSummary = string.Format( "{0} {1} with issuance {2}", entity.Organization.Name, relation, entity.Credential.Name );

		//			//if ( IsValidDate( entity.EffectiveDate ) )
		//			//	item.DateEffective = ( ( DateTime ) entity.EffectiveDate ).ToShortDateString();
		//			//if ( IsValidDate( entity.EndDate ) )
		//			//	item.EndDate = ( DateTime ) entity.EndDate;

		//			//if ( IsValidDate( entity.Created ) )
		//			//	entity.Created = ( DateTime ) entity.Created;
		//			//entity.CreatedById = entity.CreatedById == null ? 0 : ( int ) entity.CreatedById;
		//			//if ( IsValidDate( entity.LastUpdated ) )
		//			//	entity.LastUpdated = ( DateTime ) entity.LastUpdated;
		//			//entity.LastUpdatedById = entity.LastUpdatedById == null ? 0 : ( int ) entity.LastUpdatedById;
		//		}
		//		else
		//		{
		//			item.Id = 0;
		//		}

		//	}
		//	return item;

		//}
		//private static void MapAgentToQAAction( Credential credential, EM.Credential_AgentRelationship entity )
		//{
		//	QualityAssuranceActionProfile item = new QualityAssuranceActionProfile();
		//	item.Id = entity.Id;
		//	item.RowId = entity.RowId;
		//	item.ParentId = entity.CredentialId;
		//	item.Url = entity.URL;
		//	item.Description = entity.Description;

		//	item.IssuedCredentialId = entity.IssuedCredentialId != null ? ( int ) entity.IssuedCredentialId : 0;
		//	item.IssuedCredential = new Credential() { Id = entity.Credential1.Id, RowId = entity.Credential1.RowId, Name = entity.Credential1.Name };

		//	item.ActingAgentUid = entity.AgentUid != null ? ( Guid ) entity.AgentUid : Guid.Empty;
		//	item.ActingAgent = new Organization() { Id = entity.Organization.Id, RowId = entity.Organization.RowId, Name = entity.Organization.Name };

		//	//TODO eliminate ,and use guid only
		//	item.ActingAgentId = entity.OrgId;

		//	item.IsQAActionRole = ( bool ) ( entity.IsActionType ?? false );
		//	item.RoleTypeId = entity.RelationshipTypeId;
			
		//	string relation = "";
		//	if ( entity.Codes_CredentialAgentRelationship != null )
		//	{
		//		relation = entity.Codes_CredentialAgentRelationship.ReverseRelation;
		//	}
			

		//	//may be included now, but with addition of person, and use of agent, it won't
		//	if ( entity.Organization != null )
		//	{
		//		//OrganizationManager.Organization_ToMap( entity.Organization, item.TargetOrganization );
		//	}
		//	else
		//	{
		//		//get basic?
		//		item.TargetOrganization = OrganizationManager.Organization_Get( entity.OrgId );
		//	}

		//	item.ActingAgent = new Organization() { Id = entity.Organization.Id, RowId = entity.Organization.RowId, Name = entity.Organization.Name };

		//	item.ProfileSummary = string.Format( "{0} {1} this credential", entity.Organization.Name, relation );

		//	if ( IsValidDate( entity.EffectiveDate ) )
		//		item.DateEffective = ( ( DateTime ) entity.EffectiveDate ).ToShortDateString();
		//	else
		//		item.DateEffective = "";

		//	if ( IsValidDate( entity.EndDate ) )
		//		item.EndDate = ( ( DateTime ) entity.EndDate ).ToShortDateString();

		//	if ( IsValidDate( entity.Created ) )
		//		item.Created = ( DateTime ) entity.Created;
		//	item.CreatedById = entity.CreatedById == null ? 0 : ( int ) entity.CreatedById;
		//	if ( IsValidDate( entity.LastUpdated ) )
		//		item.LastUpdated = ( DateTime ) entity.LastUpdated;
		//	item.LastUpdatedById = entity.LastUpdatedById == null ? 0 : ( int ) entity.LastUpdatedById;

		//	credential.QualityAssuranceAction.Add( item );
		//}


		#endregion 
		#endregion 

		#region role codes retrieval ==================
		public static Enumeration GetEntityAgentQAActionFilters( bool isOrgToCredentialRole, bool getAll, string entityType )
		{
			return GetEntityToOrgRolesCodes( isOrgToCredentialRole, 1, getAll, entityType );

		}

		public static Enumeration GetEntityAgentQAActions( bool isOrgToCredentialRole, bool getAll = true, string entityType = "Credential" )
		{
			return GetEntityToOrgRolesCodes( isOrgToCredentialRole, 1, getAll, entityType );

		}
		public static Enumeration GetCredentialOrg_NonQARoles( bool isOrgToCredentialRole = true, string entityType = "Credential" )
		{
			return GetEntityToOrgRolesCodes( isOrgToCredentialRole, 2, true, entityType );
		}

		/// <summary>
		/// Get roles as enumeration for edit view
		/// </summary>
		/// <param name="isOrgToCredentialRole"></param>
		/// <param name="entityType"></param>
		/// <returns></returns>
		public static Enumeration GetCredentialOrg_AllRoles( bool isInverseRole = true, string entityType = "Credential" )
		{
			return GetEntityToOrgRolesCodes( isInverseRole, 0, true, entityType );
		}
		private static Enumeration GetEntityToOrgRolesCodes( bool isInverseRole, 
					int qaRoleState, 
					bool getAll,
					string entityType)
		{
			Enumeration entity = new Enumeration();

			using ( var context = new EM.CTIEntities() )
			{
				EM.Codes_PropertyCategory category = context.Codes_PropertyCategory
							.SingleOrDefault( s => s.Id == CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE );

				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();

					EnumeratedItem val = new EnumeratedItem();
					//var sortedList = context.Codes_CredentialAgentRelationship
					//		.Where( s => s.IsActive == true && ( qaOnlyRoles == false || s.IsQARole == true) )
					//		.OrderBy( x => x.Title )
					//		.ToList();

					var Query = from P in context.Codes_CredentialAgentRelationship
							.Where( s => s.IsActive == true )
								select P;
					if ( qaRoleState == 1 ) //qa only
					{
						Query = Query.Where( p => p.IsQARole == true );
					}
					else if ( qaRoleState == 2 )
					{
						//this is state is for showing org roles for a credential.
						//16-06-01 mp - for now show qa and no qa, just skip agent to agent which for now is dept and subsiduary
						if ( entityType.ToLower() == "credential" )
							Query = Query.Where( p => p.IsEntityToAgentRole == true );
						else
							Query = Query.Where( p => p.IsQARole == false && p.IsEntityToAgentRole == true );
					}
					else //all
					{

					}
					Query = Query.OrderBy( p => p.Title );
					var results = Query.ToList();

					//add Select option
					//need to only do if for a dropdown, not a checkbox list
					if ( qaRoleState == 1 )
					{
						val = new EnumeratedItem();
						val.Id = 0;
						val.CodeId = val.Id;
						val.Name = "Select an Action";
						val.Description = "";
						val.SortOrder = 0;
						val.Value = val.Id.ToString();
						entity.Items.Add( val );
					}
					

					//foreach ( Codes_PropertyValue item in category.Codes_PropertyValue )
					foreach ( EM.Codes_CredentialAgentRelationship item in results )
					{
						val = new EnumeratedItem();
						val.Id = item.Id;
						val.CodeId = item.Id;
						val.Value = item.Id.ToString();//????
						val.Description = item.Description;

						if ( isInverseRole )
						{
							val.Name = item.ReverseRelation;
							//if ( string.IsNullOrWhiteSpace( entityType ) )
							//{
							//	//may not matter
							//	val.Description = string.Format( "Organization has {0} service.", item.ReverseRelation );
							//}
							//else
							//{
							//	val.Description = string.Format( "Organization {0} this {1}", item.ReverseRelation, entityType );
							//}
						}
						else
						{
							val.Name = item.Title;
							//val.Description = string.Format( "{0} is {1} by this Organization ", entityType, item.Title );
						}

						if ( ( bool ) item.IsQARole )
						{
							val.IsSpecialValue = true;
							if ( IsDevEnv() )
								val.Name += " (QA)";
						}

						entity.Items.Add( val );
					}

				}
			}

			return entity;
		}


		public static Enumeration GetAgentToAgentRolesCodes( bool isInverseRole = true )
		{
			Enumeration entity = new Enumeration();

			using ( var context = new EM.CTIEntities() )
			{
				EM.Codes_PropertyCategory category = context.Codes_PropertyCategory
							.SingleOrDefault( s => s.Id == CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE );

				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();

					EnumeratedItem val = new EnumeratedItem();
					//var sortedList = context.Codes_CredentialAgentRelationship
					//		.Where( s => s.IsActive == true && ( qaOnlyRoles == false || s.IsQARole == true) )
					//		.OrderBy( x => x.Title )
					//		.ToList();

					var Query = from P in context.Codes_CredentialAgentRelationship
							.Where( s => s.IsActive == true && s.IsAgentToAgentRole == true )
								select P;

					Query = Query.OrderBy( p => p.Title );
					var results = Query.ToList();

					foreach ( EM.Codes_CredentialAgentRelationship item in results )
					{
						val = new EnumeratedItem();
						val.Id = item.Id;
						val.CodeId = item.Id;
						val.Value = item.Id.ToString();//????
						val.Description = item.Description;

						if ( isInverseRole )
						{
							val.Name = item.ReverseRelation;
							//if ( string.IsNullOrWhiteSpace( entityType ) )
							//{
							//	//may not matter
							//	val.Description = string.Format( "Organization has {0} service.", item.ReverseRelation );
							//}
							//else
							//{
							//	val.Description = string.Format( "Organization {0} this {1}", item.ReverseRelation, entityType );
							//}
						}
						else
						{
							val.Name = item.Title;
							//val.Description = string.Format( "{0} is {1} by this Organization ", entityType, item.Title );
						}

						if ( ( bool ) item.IsQARole )
						{
							val.IsSpecialValue = true;
							if ( IsDevEnv() )
								val.Name += " (QA)";
						}

						entity.Items.Add( val );
					}

				}
			}

			return entity;
		}

		
		#endregion


		#region OBSOLETE
		//private static void MapAgentToOrgRole( Credential credential, EM.Credential_AgentRelationship entity )
		//{
		//	ThisEntity p = new ThisEntity();
		//	p.Id = entity.Id;
		//	p.ParentId = entity.CredentialId;
		//	p.Url = entity.URL;
		//	p.Description = entity.Description;

		//	p.ActingAgentId = entity.OrgId;
		//	if ( entity.AgentUid != null )
		//		p.ActingAgentUid = ( Guid ) entity.AgentUid;
		//	p.RoleTypeId = entity.RelationshipTypeId;
		//	string relation = "";
		//	if ( entity.Codes_CredentialAgentRelationship != null )
		//	{
		//		relation = entity.Codes_CredentialAgentRelationship.ReverseRelation;
		//	}

		//	//may be included now, but with addition of person, and use of agent, it won't
		//	if ( entity.Organization != null )
		//	{
		//		OrganizationManager.Organization_ToMap( entity.Organization, p.TargetOrganization );
		//	}
		//	else
		//	{
		//		//get basic?
		//		p.TargetOrganization = OrganizationManager.Organization_Get( entity.OrgId );
		//	}

		//	p.ProfileSummary = string.Format( "{0} {1} this credential", entity.Organization.Name, relation );
		//	if ( IsValidDate( entity.EffectiveDate ) )
		//		p.DateEffective = ( ( DateTime ) entity.EffectiveDate ).ToShortDateString();
		//	else
		//		p.DateEffective = "";

		//	if ( IsValidDate( entity.Created ) )
		//		p.Created = ( DateTime ) entity.Created;
		//	p.CreatedById = entity.CreatedById == null ? 0 : ( int ) entity.CreatedById;
		//	if ( IsValidDate( entity.LastUpdated ) )
		//		p.LastUpdated = ( DateTime ) entity.LastUpdated;
		//	p.LastUpdatedById = entity.LastUpdatedById == null ? 0 : ( int ) entity.LastUpdatedById;

		//	credential.OrganizationRole.Add(p);
		//}		//private bool CredentialOrgRole_Update( int recordId, int agentId, int roleId, int userId, ref string status )
		//{
		//	bool isValid = true;
		//	if ( recordId == 0 )
		//	{
		//		status = "Error: invalid request, please ensure a valid record has been selected.";
		//		return false;
		//	}

		//	//TODO - need to handle agent
		//	Organization org = OrganizationManager.Organization_Get( agentId, false );
		//	if ( org == null || org.Id == 0 )
		//	{
		//		status = "Error: the selected organization was not found!";
		//		LoggingHelper.DoTrace( 5, string.Format( "OrganizationRoleManager.CredentialOrgRole_Update the organization was not found, for credential: {0}, AgentId:{1}, RoleId: {2}", recordId, agentId, roleId ) );
		//		return false;
		//	}

		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		EM.Credential_AgentRelationship car = context.Credential_AgentRelationship.FirstOrDefault( s => s.Id == recordId );
		//		if ( car != null && car.Id > 0 )
		//		{
		//			status = "Error: the selected relationship was not found!";
		//			return false;
		//		}

		//		//assign, then check if there were any actual updates
		//		//this credential centric, so leave alone
		//		car.OrgId = agentId;
		//		car.AgentUid = org.RowId;
		//		car.RelationshipTypeId = roleId;

		//		if ( HasStateChanged( context ) )
		//		{
		//			car.LastUpdated = System.DateTime.Now;
		//			car.LastUpdatedById = userId;

		//			// submit the change to database
		//			int count = context.SaveChanges();
		//		}
		//	}

		//	return isValid;
		//}
		/// <summary>
		/// Persist all credential - org relationships
		/// ==> note will want to watch for creator and owner, and ignore as FOR NOW should not be handled here!
		/// </summary>
		/// <param name="credential"></param>
		/// <returns></returns>
		//public bool CredentialAgentRoles_Update( Credential credential, ref string status, ref int count )
		//{
		//	bool isValid = true;
		//	count = 0;
		//	int count1 = 0;
		//	string status1 = "";
		//	isValid = Credential_UpdateOrgRoles( credential, ref status1, ref count1 );
		//	count = count1;
		//	status = status1;
		//	if ( Credential_UpdateQAActions( credential, ref status, ref count1 ) )
		//	{
		//		isValid = false;
		//	}
		//	count += count1;
		//	status += status1;
		//	//List<string> messages = new List<string>();

		//	//if ( credential.OrganizationRole == null )
		//	//	credential.OrganizationRole = new List<ThisEntity>();

		//	//if ( credential.QualityAssuranceAction == null )
		//	//	credential.QualityAssuranceAction = new List<QualityAssuranceActionProfile>();

		//	//using ( var context = new EM.CTIEntities() )
		//	//{
		//	//	//loop thru input, check for changes to existing, and for adds
		//	//	foreach ( ThisEntity item in credential.OrganizationRole )
		//	//	{
		//	//		int codeId = CodesManager.GetEnumerationSelection( item.RoleType );
		//	//		if ( codeId == 0 )
		//	//		{
		//	//			isValid = false;
		//	//			messages.Add( string.Format( "Error: a role was not entered. Select a role and try again. AgentId: {0}", item.ActingAgentId ) );
		//	//			continue;
		//	//		}

		//	//		if ( item.Id > 0 )
		//	//		{
		//	//			EM.Credential_AgentRelationship p = context.Credential_AgentRelationship.FirstOrDefault( s => s.Id == item.Id);
		//	//			if ( p != null && p.Id > 0 )
		//	//			{
		//	//				p.CredentialId = credential.Id;
		//	//				//int itemId = CodesManager.GetEnumerationSelection( item.RoleType );
		//	//				//if ( itemId == 0 )
		//	//				//{
		//	//				//	isValid = false;
		//	//				//	messages.Add( string.Format( "Error: a role was not found: {0}", item.ActingAgentId ) );
		//	//				//	continue;
		//	//				//}
		//	//				p.RelationshipTypeId = codeId;
		//	//				//actually need to get the rowId!
		//	//				if ( p.OrgId != item.ActingAgentId )
		//	//				{
		//	//					//NOTE - need to handle agent!!!
		//	//					Organization org = OrganizationManager.Organization_Get( item.OrganizationId, false );
		//	//					if ( org == null || org.Id == 0 )
		//	//					{
		//	//						isValid = false;
		//	//						messages.Add( string.Format( "Error: the selected organization was not found: {0}", item.ActingAgentId ) );
		//	//						continue;
		//	//					}
		//	//					p.AgentUid = org.RowId;
		//	//				}
		//	//				p.OrgId = item.ActingAgentId;
		//	//				if ( HasStateChanged( context ) )
		//	//				{
		//	//					p.LastUpdated = System.DateTime.Now;
		//	//					p.LastUpdatedById = credential.LastUpdatedById;
		//	//					count = context.SaveChanges();
		//	//				}
		//	//			}
		//	//			else
		//	//			{
		//	//				//error should have been found
		//	//				isValid = false;
		//	//				messages.Add( string.Format("Error: the requested role was not found: recordId: {0}", item.Id ));
		//	//			}
		//	//		}
		//	//		else
		//	//		{
		//	//			CredentialOrgRole_Add( credential.Id, item.ActingAgentId, codeId, credential.LastUpdatedById, ref status );
		//	//		}
		//	//	}

		//	//}

		//	return isValid;
		//}

		//public static void FillAllOrgToOrgRoles( EM.Credential fromCredential, Credential credential )
		//{
		//	//start by assuming all roles have been read
		//	if ( fromCredential.Credential_AgentRelationship == null || fromCredential.Credential_AgentRelationship.Count == 0 )
		//	{
		//		return;
		//	}

		//	credential.OrganizationRole = new List<ThisEntity>();
		//	credential.QualityAssuranceAction = new List<QualityAssuranceActionProfile>();

		//	foreach ( EM.Credential_AgentRelationship item in fromCredential.Credential_AgentRelationship )
		//	{
		//		bool isActionType = item.IsActionType == null ? false : ( bool ) item.IsActionType;

		//		if ( item.TargetCredentialId > 0 || isActionType )
		//		{
		//			MapAgentToQAAction( credential, item );
		//		}
		//		else
		//		{
		//			//MapAgentToOrgRole( credential, item );
		//			credential.OrganizationRole.Add( MapAgentToOrgRole( item, "credential" ) );
		//		}


		//		if ( item.RelationshipTypeId == CredentialToOrgRole_CreatedBy )
		//			credential.CreatorOrganizationId = item.OrgId;

		//		if ( item.RelationshipTypeId == CredentialToOrgRole_OwnedBy )
		//			credential.OwnerOrganizationId = item.OrgId;


		//	}


		//}
		// item
		//public static CredentialAgentRelationship AgentCredentialRoleGet( int recordId )
		//{
		//	CredentialAgentRelationship item = new CredentialAgentRelationship();
		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		EM.Credential_AgentRelationship entity = context.Credential_AgentRelationship.FirstOrDefault( s => s.Id == recordId );
		//		if ( entity != null && entity.Id > 0 )
		//		{
		//			item.Id = entity.Id;
		//			item.ParentId = entity.CredentialId;
		//			item.OrganizationId = entity.OrgId;
		//			item.RelationshipId = entity.RelationshipTypeId;
		//			item.AgentUid = entity.AgentUid != null ? ( Guid ) entity.AgentUid : Guid.Empty;

		//			//item.TargetOrganization = entity.Organization;
		//		}
		//		else
		//		{
		//			item.Id = 0;
		//		}

		//	}
		//	return item;

		//}

		/// <summary>
		/// Retrieve and fill org roles of creator or owner only
		/// </summary>
		/// <param name="credential"></param>
		//public static void FillOwnerOrgRolesForCredential( Credential credential )
		//{
		//	EnumeratedItem row = new EnumeratedItem();
		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		List<Views.CredentialAgentRelationships_Summary> results = context.CredentialAgentRelationships_Summary
		//				.Where( s => s.CredentialId == credential.Id
		//				&& ( s.RelationshipTypeId == CredentialToOrgRole_CreatedBy || s.RelationshipTypeId == CredentialToOrgRole_OwnedBy ) )
		//				.OrderBy( s => s.RelationshipTypeId )
		//				.ToList();

		//		if ( results != null && results.Count > 0 )
		//		{
		//			foreach ( Views.CredentialAgentRelationships_Summary item in results )
		//			{
		//				if ( item.RelationshipTypeId == CredentialToOrgRole_CreatedBy )
		//				{
		//					//create an enumeration, but can be minimal
		//					//credential.CreatorUrl = new Enumeration();
		//					//credential.CreatorUrl.Name = "creatorUrl";
		//					//credential.CreatorUrl.SchemaName = "creatorUrl";
		//					//credential.CreatorUrl.ParentId = credential.Id;
		//					//credential.CreatorUrl.Id = item.OrgId;

		//					credential.CreatorOrganizationId = item.OrgId;

		//					//row = new EnumeratedItem()
		//					//{
		//					//	Id = item.OrgId,
		//					//	Name = item.OrganizationName,
		//					//	Description = item.RelationshipType,
		//					//	Selected = true, 
		//					//	Value = item.OrgId.ToString(),
		//					//	Created = item.Created ?? DateTime.Now,
		//					//	CreatedById = item.CreatedById ?? 0
		//					//};
		//					//credential.CreatorUrl.Items.Add( row );
		//				}
		//				else if ( item.RelationshipTypeId == CredentialToOrgRole_OwnedBy )
		//				{
		//					//credential.OwnerUrl = new Enumeration();
		//					//credential.OwnerUrl.Name = "ownerUrl";
		//					//credential.OwnerUrl.SchemaName = "ownerUrl";
		//					//credential.OwnerUrl.ParentId = credential.Id;
		//					//credential.OwnerUrl.Id = item.OrgId;

		//					credential.OwnerOrganizationId = item.OrgId;

		//					//row = new EnumeratedItem()
		//					//{
		//					//	Id = item.OrgId,
		//					//	Name = item.OrganizationName,
		//					//	Description = item.RelationshipType,
		//					//	Selected = true,
		//					//	Value = item.OrgId.ToString(),
		//					//	Created = item.Created ?? DateTime.Now,
		//					//	CreatedById = item.CreatedById ?? 0
		//					//};
		//					//credential.OwnerUrl.Items.Add( row );
		//				}
		//			}
		//		}
		//	}

		//}//

		#endregion
	}
}
