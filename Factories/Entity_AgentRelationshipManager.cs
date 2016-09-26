using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.ProfileModels;
using Models.Common;
using MN = Models.Node;
using EM = Data;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
using Utilities;

using Entity = Models.ProfileModels.OrganizationRoleProfile;
using DBentity = Data.Views.Entity_AgentRelationshipIdCSV;
using DBentitySummary = Data.Views.Entity_Relationship_AgentSummary;

namespace Factories
{
	public class Entity_AgentRelationshipManager : BaseFactory
	{
		/// <summary>
		/// Entity_AgentRelationshipManager
		/// The entity is acted upon by the agent. ex
		/// Credential accredited by an agent
		///		Entity: credential ??? by Agent: org
		///	Org accredits another org (entity)
		///		Entity: target org ?? by Agent: current org
		///	Org is accredited by another org
		///		Entity: current org ?? by Agent: entered org
		/// </summary>
		string thisClassname = "Entity_AgentRelationshipManager";

		#region role type constants
		public static int ROLE_TYPE_DEPARTMENT = 20;
		public static int ROLE_TYPE_SUBSIDUARY = 21;
		#endregion

		#region roles persistance ==================
	
		public bool Agent_EntityRoles_Save( OrganizationRoleProfile profile,
					int userId,
					ref List<string> messages )
		{
			bool isValid = true;
			string statusMessage = "";
			int msgCount = messages.Count;

			//not sure if will user isParentActor - will start with assuming parent is the recipient/acted upon
			bool isParentActor = false;
			bool isInverseRole = !isParentActor;

			if ( !IsValidGuid( profile.ParentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}

			if ( messages.Count > msgCount )
				return false;

			List<OrganizationRoleProfile> list = FillAllOrgRoles( profile, ref messages, ref isValid );
			if ( messages.Count > msgCount )
				return false;

			//the parent needs to be established by using isParentActor
			Views.Entity_Summary parent = EntityManager.GetDBEntity( profile.ParentUid );

			using ( var context = new EM.CTIEntities() )
			{
				//get all existing roles for the parent
				//will need some context here
				//also why it may be good to keep credential QA actions separate
				var results = GetAllRolesForAgent( profile.ParentUid, profile.ActingAgentUid, isParentActor );

				#region deletes/updates check

				var deleteList = from existing in results
								 join item in list
								 on new { existing.ActingAgentUid, existing.RoleTypeId }
								 equals new { item.ActingAgentUid, item.RoleTypeId }
								 into joinTable
								 from result in joinTable.DefaultIfEmpty( new OrganizationRoleProfile { ActingAgentId = 0, ActingAgentUid = Guid.NewGuid(), ParentId = 0, Id = 0 } )
								 select new { ActingAgentUid = existing.ActingAgentUid, DeleteId = existing.Id, ItemId = ( result.RoleTypeId ), IsInverseRole = result.IsInverseRole };

				foreach ( var v in deleteList )
				{

					if ( v.ItemId == 0 )
					{
						//delete item
						if ( EntityAgentRole_Delete( v.DeleteId, ref statusMessage ) == false )
						{
							messages.Add( statusMessage );
							isValid = false;
						}

					}
				}
				#endregion

				#region new items
				//should only empty ids, where not in current list, so should be adds
				var newList = from item in list
							  join existing in results
									 on new { item.ActingAgentUid, item.RoleTypeId }
								 equals new { existing.ActingAgentUid, existing.RoleTypeId }
									into joinTable
							  from addList in joinTable.DefaultIfEmpty( new OrganizationRoleProfile { Id = 0, ActingAgentId = 0, ActingAgentUid = Guid.NewGuid(), RoleTypeId = 0 } )
							  select new { ActingAgentUid = item.ActingAgentUid, RoleTypeId = item.RoleTypeId, ExistingId = addList.Id };
				foreach ( var v in newList )
				{
					if ( v.ExistingId == 0 )
					{
						bool isEmpty = false;
						if ( Entity_AgentRole_Add( parent.Id,
									profile.ActingAgentUid,
									v.RoleTypeId,
									isInverseRole,
									userId,
									ref messages,
									ref isEmpty ) == 0 )
						{
							if ( !isEmpty )
								isValid = false;
						}

					}
				}
				#endregion
			}
			return isValid;
		}
		/// <summary>
		/// Retrieve all existing roles for a parent and agent - only used for Agent_EntityRoles_Save
		/// </summary>
		/// <param name="pParentUid"></param>
		/// <param name="agentUid"></param>
		/// <param name="isParentActor"></param>
		/// <returns></returns>
		private static List<OrganizationRoleProfile> GetAllRolesForAgent( Guid pParentUid, Guid agentUid, bool isParentActor = false )
		{
			//If parent is actor, then this is a direct role. 
			//for ex. if called from assessments, then it is inverse, as the parent is the assessment, and the relate org is the actor
			bool isInverseRole = !isParentActor;

			OrganizationRoleProfile p = new OrganizationRoleProfile();
			List<OrganizationRoleProfile> list = new List<OrganizationRoleProfile>();
			List<Views.Entity_Relationship_AgentSummary> roles = new List<Views.Entity_Relationship_AgentSummary>();
			using ( var context = new ViewContext() )
			{
				roles = context.Entity_Relationship_AgentSummary
						.Where( s => s.SourceEntityUid == pParentUid
							&& s.ActingAgentUid == agentUid
							&& s.IsInverseRole == isInverseRole )
						.ToList();

				foreach ( Views.Entity_Relationship_AgentSummary entity in roles )
				{
					p = new OrganizationRoleProfile();
					MapTo( entity, p );

					list.Add( p );
				}

			}
			return list;

		} //
		private static void MapTo( Views.Entity_Relationship_AgentSummary from, Entity to )
		{

			to.Id = from.EntityAgentRelationshipId;
			to.RowId = from.RowId;

			to.ParentUid = from.SourceEntityUid;
			to.ParentTypeId = from.SourceEntityTypeId;
			to.ActingAgentUid = from.ActingAgentUid;
			//useful for compare when doing deletes, and New checks
			to.ActingAgentId = from.AgentRelativeId;

			to.ActingAgent = new Organization()
			{
				Id = from.AgentRelativeId,
				RowId = from.ActingAgentUid,
				Name = from.AgentName,
				Url = from.AgentUrl,
				Description = from.AgentDescription,
				ImageUrl = from.AgentImageUrl
			};

			to.RoleTypeId = from.RelationshipTypeId;

			string relation = "";
			if ( from.SourceToAgentRelationship != null )
			{
				relation = from.SourceToAgentRelationship;
			}
			to.IsInverseRole = from.IsInverseRole ?? false;

			to.ProfileSummary = from.AgentName;
			//can only use a detail summary where only one relationship exists!!
			//to.ProfileSummary = string.Format( "{0} is a {1} of {2}", from.AgentName, relation, from.SourceEntityName );
			to.ProfileName = to.ProfileSummary;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById;

		}

		private List<OrganizationRoleProfile> FillAllOrgRoles( OrganizationRoleProfile profile,
			ref List<string> messages,
			ref bool isValid)
		{
			isValid = false;

			OrganizationRoleProfile entity = new OrganizationRoleProfile();
			List<OrganizationRoleProfile> list = new List<OrganizationRoleProfile>();
			if ( !IsValidGuid( profile.ParentUid ) )
			{
				//roles, no agent
				messages.Add( "Invalid request, the parent entity was not provided." );
				return list;
			}
			if ( !IsGuidValid( profile.ActingAgentUid ) )
			{
				//roles, no agent
				messages.Add( "Invalid request, please select an agent for selected roles." );
				return list;
			}
			if ( profile.RoleType == null || profile.RoleType.Items.Count == 0 )
			{
				messages.Add( "Invalid request, please select one or more roles for this selected agent." );
				return list;
			}

			//loop thru the roles
			foreach ( EnumeratedItem e in profile.RoleType.Items )
			{
				entity = new OrganizationRoleProfile();
				entity.ParentId = profile.ParentId;				
				entity.ActingAgentUid = profile.ActingAgentUid;
				entity.ActingAgentId = profile.ActingAgentId;
				entity.RoleTypeId = e.Id;
				entity.IsInverseRole = profile.IsInverseRole;

				list.Add( entity );
			}
		
			
			isValid = true;
			return list;
		}
		public bool Entity_AgentRole_SaveSingleRole( OrganizationRoleProfile profile,
					int userId,
					ref List<string> messages )
		{
			bool isValid = true;
			bool isInverseRole = true;
			if ( !IsValidGuid( profile.ParentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}

			if ( messages.Count > 0 )
				return false;

			if ( profile == null )
			{
				return false;
			}

			Views.Entity_Summary parent = EntityManager.GetDBEntity( profile.ParentUid );
			using ( var context = new EM.CTIEntities() )
			{

				int roleId = profile.RoleTypeId;
				int entityId = parent.Id;
				if ( profile.Id > 0 )
				{
					EM.Entity_AgentRelationship p = context.Entity_AgentRelationship.FirstOrDefault( s => s.Id == profile.Id );
					if ( p != null && p.Id > 0 )
					{
						//p.ParentUid = parentUid;
						if ( roleId == 0 )
						{
							isValid = false;
							messages.Add( "Error: a role was not entered. Select a role and try again. " );
							return false;
						}
						if ( !IsValidGuid( profile.ActingAgentUid ) )
						{
							isValid = false;
							messages.Add( "Error: an agent was not selected. Select an agent and try again. " );
							return false;
						}
						p.RelationshipTypeId = roleId;

						p.URL = profile.Url;
						p.Description = profile.Description;
						p.EntityId = parent.Id;

						if ( HasStateChanged( context ) )
						{
							p.LastUpdated = System.DateTime.Now;
							p.LastUpdatedById = userId;
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
					if ( !IsValidGuid( profile.ActingAgentUid ) && profile.ActingAgentId > 0 )
					{
						//NOTE - need to handle agent!!!
						Organization org = OrganizationManager.Organization_Get( profile.ActingAgentId, false );
						if ( org == null || org.Id == 0 )
						{
							isValid = false;
							messages.Add( string.Format( "Error: the selected organization was not found: {0}", profile.ActingAgentId ) );
							return false;
						}
						profile.ActingAgentUid = org.RowId;
					}
					bool isEmpty = false;
					profile.Id = Entity_AgentRole_Add( entityId, profile.ActingAgentUid, roleId, isInverseRole, userId, ref messages, ref isEmpty );
					if ( profile.Id == 0 )
						isValid = false;

				}
			}
			return isValid;
		}

		private int Entity_AgentRole_Add( int entityId, Guid agentUid, int roleId, bool isInverseRole, int userId, ref List<string> messages, ref bool isEmpty )
		{
			int newId = 0;
			//assume if all empty, then ignore
			if ( entityId == 0 && !IsValidGuid( agentUid ) )
			{
				return newId;
			}
			if ( !IsValidGuid( agentUid ) && roleId == 0 )
			{
				return newId;
			}
			//
			if ( IsValidGuid( agentUid ) && roleId == 0 )
			{
				messages.Add( "Error: invalid request, please select a role." );
				return 0;
			}
			else if ( !IsValidGuid( agentUid ) && roleId > 0 )
			{
				messages.Add( "Error: invalid request, please select an agent." );
				return 0;
			}

			if ( AgentEntityRoleExists( entityId, agentUid, roleId ))
			{
				messages.Add( "Error: the selected relationship already exists!" );
				return 0;
			}
			//TODO - need to handle agent
			MN.ProfileLink org = OrganizationManager.Agent_GetProfileLink( agentUid );
			if ( org == null || org.Name.Length == 0 )
			{
				messages.Add( "Error: the selected agent was not found!" );
				LoggingHelper.DoTrace( 5, thisClassname + string.Format( ".Entity_AgentRole_Add the agent was not found, for entityId: {0}, AgentId:{1}, RoleId: {2}", entityId, agentUid, roleId ) );
				return 0;
			}

			using ( var context = new EM.CTIEntities() )
			{
				//add
				EM.Entity_AgentRelationship car = new EM.Entity_AgentRelationship();

				car.EntityId = entityId;
				//TODO - remove the use of these two
				//car.ParentUid = parentUid;
				//car.ParentTypeId = parentTypeId;

				car.AgentUid = agentUid;
				car.RelationshipTypeId = roleId;
				car.IsInverseRole = isInverseRole;

				car.Created = System.DateTime.Now;
				car.CreatedById = userId;
				car.LastUpdated = System.DateTime.Now;
				car.LastUpdatedById = userId;
				car.RowId = Guid.NewGuid();
				context.Entity_AgentRelationship.Add( car );

				// submit the change to database
				int count = context.SaveChanges();
				newId = car.Id;
			}

			return newId;
		}

		/// <summary>
		/// Delete a single role
		/// </summary>
		/// <param name="recordId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool EntityAgentRole_Delete( int recordId, ref string statusMessage )
		{
			bool isValid = false;

			using ( var context = new EM.CTIEntities() )
			{
				if ( recordId == 0 )
				{
					statusMessage = "Error - missing an identifier for the Entity-Agent Role";
					return false;
				}

				EM.Entity_AgentRelationship efEntity =
					context.Entity_AgentRelationship.SingleOrDefault( s => s.Id == recordId );
				if ( efEntity != null && efEntity.Id > 0 )
				{
					context.Entity_AgentRelationship.Remove( efEntity );
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					}
				}
				else
				{
					statusMessage = string.Format( "Agent role record was not found: {0}", recordId );
					isValid = false;
				}
			}

			return isValid;
		}

		/// <summary>
		/// Delete all roles for the provided entityId (parent) and agent combination.
		/// Note: this should be inverse relationships, but we don't have direct at this time
		/// </summary>
		/// <param name="entityId"></param>
		/// <param name="agentUid"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete_EntityAgentRoles( int entityId, Guid agentUid, ref string statusMessage )
		{
			bool isValid = true;

			using ( var context = new EM.CTIEntities() )
			{
				if ( entityId == 0 || !IsValidGuid( agentUid ) )
				{
					statusMessage = "Error - missing identifiers, please provide proper keys.";
					return false;
				}

				context.Entity_AgentRelationship.RemoveRange( context.Entity_AgentRelationship.Where( s => s.EntityId == entityId && s.AgentUid == agentUid ) );
				int count = context.SaveChanges();
				if ( count > 0 )
				{
					isValid = true;
				}
				else
				{
					//this can happen initially where a role was visible, and is not longer
					statusMessage = string.Format( "Warning Delete failed, Agent role record(s) were not found for: entityId: {0}, agentUid: {1}", entityId, agentUid );
					isValid = false;
				}
			}

			return isValid;
		}
		/// <summary>
		/// Delete all records for a parent (typically due to delete of parent)
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool EntityAgentRole_DeleteAll( Guid parentUid, ref string statusMessage )
		{
			bool isValid = false;

			using ( var context = new EM.CTIEntities() )
			{
				if ( parentUid.ToString().IndexOf( "0000" ) == 0 )
				{
					statusMessage = "Error - missing an identifier for the Parent Entity";
					return false;
				}

				List<EM.Entity_AgentRelationship> list =
					context.Entity_AgentRelationship
						.Where( s => s.ParentUid == parentUid )
						.ToList();
				//don't need this way
				if ( list != null && list.Count > 0 )
				{
					//context.Entity_AgentRelationship.Remove( efEntity );
					context.Entity_AgentRelationship.RemoveRange( context.Entity_AgentRelationship.Where( s => s.ParentUid == parentUid ) );
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					}
				}

			}

			return isValid;
		}
		#endregion

		#region obsolete or will soon be - maybe not
		/// <summary>
		/// Update roles where entity is the primary, and agent is the target
		/// IsInverseRole = true
		/// ==> Assumes a single roleId
		/// </summary>
		/// <param name="profiles"></param>
		/// <param name="parentUid"></param>
		/// <param name="parentTypeId"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		//[Obsolete]
		//public bool Entity_UpdateAgent_SingleRole( List<OrganizationRoleProfile> profiles,
		//			Guid parentUid,
		//			int parentTypeId,
		//			int userId,
		//			ref List<string> messages,
		//			ref int count )
		//{
		//	bool isValid = true;
		//	bool isInverseRole = true;
		//	count = 0;
		//	if ( !IsValidGuid( parentUid ) )
		//	{
		//		messages.Add( "Error: the parent identifier was not provided." );
		//	}
		//	if ( parentTypeId == 0 )
		//	{
		//		messages.Add( "Error: the parent type was not provided." );
		//	}
		//	if ( messages.Count > 0 )
		//		return false;

		//	//
		//	if ( profiles == null )
		//		profiles = new List<OrganizationRoleProfile>();

		//	Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );

		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		//loop thru input, check for changes to existing, and for adds
		//		foreach ( OrganizationRoleProfile item in profiles )
		//		{
		//			int roleId = item.RoleTypeId;
		//			int entityId = parent.Id;
		//			if ( item.Id > 0 )
		//			{
		//				EM.Entity_AgentRelationship p = context.Entity_AgentRelationship.FirstOrDefault( s => s.Id == item.Id );
		//				if ( p != null && p.Id > 0 )
		//				{
		//					//p.ParentUid = parentUid;
		//					if ( roleId == 0 )
		//					{
		//						isValid = false;
		//						messages.Add( "Error: a role was not entered. Select a role and try again. " );
		//						continue;
		//					}
		//					if ( !IsValidGuid( item.ActingAgentUid ) )
		//					{
		//						isValid = false;
		//						messages.Add( "Error: an agent was not selected. Select an agent and try again. " );
		//						continue;
		//					}
		//					p.RelationshipTypeId = roleId;

		//					p.URL = item.Url;
		//					p.Description = item.Description;
		//					p.EntityId = parent.Id;

		//					if ( HasStateChanged( context ) )
		//					{
		//						p.LastUpdated = System.DateTime.Now;
		//						p.LastUpdatedById = userId;
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
		//				if ( !IsValidGuid( item.ActingAgentUid ) && item.ActingAgentId > 0 )
		//				{
		//					//NOTE - need to handle agent!!!
		//					Organization org = OrganizationManager.Organization_Get( item.ActingAgentId, false );
		//					if ( org == null || org.Id == 0 )
		//					{
		//						isValid = false;
		//						messages.Add( string.Format( "Error: the selected organization was not found: {0}", item.ActingAgentId ) );
		//						continue;
		//					}
		//					item.ActingAgentUid = org.RowId;
		//				}
		//				bool isEmpty = false;
		//				if ( EntityAgentRole_Add( entityId, item.ActingAgentUid, parentUid, parentTypeId, roleId, isInverseRole, userId, ref messages, ref isEmpty ) == false )
		//					isValid = false;
		//				else
		//				{
		//					if ( !isEmpty )
		//						count++;
		//				}
		//			}

		//		}

		//	}
		//	return isValid;
		//}
		///// <summary>
		///// Update roles
		///// If isInverseRole = true, then agent is the primary, and entity is the target
		///// ====> soon to be obsolete??
		///// </summary>
		///// <param name="profiles"></param>
		///// <param name="parentUid">Parent will be the ReverseAgentUid, and ActingAgentUid, will be the parentUid!!!!</param>
		///// <param name="parentTypeId">make obsolete</param>
		///// <param name="isParentActor">If True, set acting agent as parent, otherwise set parent to acted upon</param>
		///// <param name="userId"></param>
		///// <param name="messages"></param>
		///// <param name="count"></param>
		///// <returns></returns>
		//[Obsolete]
		//public bool Agent_UpdateEntityRoles( List<OrganizationRoleProfile> profiles,
		//			Guid parentUid,
		//			int parentTypeId,
		//			bool isParentActor,
		//			int userId,
		//			ref List<string> messages,
		//			ref int count )
		//{
		//	bool isValid = true;
		//	//bool isInverseRole = false;
		//	string statusMessage = "";
		//	int msgCount = messages.Count;

		//	count = 0;
		//	if ( !IsValidGuid( parentUid ) )
		//	{
		//		messages.Add( "Error: the parent identifier was not provided." );
		//	}
		//	//aim to eliminate including parentTypeId
		//	if ( parentTypeId == 0 )
		//	{
		//		messages.Add( "Error: the parent type was not provided." );
		//	}
		//	if ( messages.Count > msgCount )
		//		return false;

		//	//
		//	if ( profiles == null )
		//		profiles = new List<OrganizationRoleProfile>();

		//	List<OrganizationRoleProfile> list = FillAllOrgRoles( profiles,
		//			parentUid,
		//			isParentActor,
		//			ref messages );
		//	if ( messages.Count > msgCount )
		//		return false;

		//	//the parent needs to be established by using isParentActor
		//	Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );

		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		//get all existing roles for the parent
		//		var results = AgentEntityRole_GetAll( parentUid, isParentActor );

		//		#region deletes/updates check
		//		var deleteList = from existing in results
		//						 join item in list
		//						 on new { existing.ActingAgentId, existing.RoleTypeId }
		//						 equals new { item.ActingAgentId, item.RoleTypeId }
		//						 into joinTable
		//						 from result in joinTable.DefaultIfEmpty( new OrganizationRoleProfile { ActingAgentId = 0, ParentId = 0, Id = 0 } )
		//						 select new { ActingAgentId = existing.ActingAgentId, DeleteId = existing.Id, ItemId = ( result.RoleTypeId ), IsInverseRole = result.IsInverseRole };

		//		foreach ( var v in deleteList )
		//		{

		//			if ( v.ItemId == 0 )
		//			{
		//				//delete item
		//				if ( EntityAgentRole_Delete( v.DeleteId, ref statusMessage ) == false )
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
		//					  from addList in joinTable.DefaultIfEmpty( new OrganizationRoleProfile { Id = 0, ActingAgentId = 0, RoleTypeId = 0 } )
		//					  select new { ActingAgentId = item.ActingAgentId, RoleTypeId = item.RoleTypeId, ExistingId = addList.Id };
		//		foreach ( var v in newList )
		//		{
		//			if ( v.ExistingId == 0 )
		//			{
		//				bool isEmpty = false;
		//				if ( EntityAgentRole_Add( parent.Id,
		//							v.ActingAgentId,
		//							parent.EntityUid,
		//							parentTypeId,
		//							v.RoleTypeId,
		//							isParentActor,
		//							userId,
		//							ref messages,
		//							ref isEmpty ) == false )
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

		//public bool Agent_UpdateEntityRolesByActor( List<OrganizationRoleProfile> profiles,
		//					Guid parentUid,
		//					int parentTypeId,
		//					bool isParentActor,
		//					int userId,
		//					ref List<string> messages,
		//					ref int count )
		//{
		//	bool isValid = true;
		//	//bool isInverseRole = false;
		//	string statusMessage = "";
		//	int msgCount = messages.Count;

		//	count = 0;
		//	if ( !IsValidGuid( parentUid ) )
		//	{
		//		messages.Add( "Error: the parent identifier was not provided." );
		//	}
		//	//aim to eliminate including parentTypeId
		//	if ( parentTypeId == 0 )
		//	{
		//		messages.Add( "Error: the parent type was not provided." );
		//	}
		//	if ( messages.Count > msgCount )
		//		return false;

		//	//
		//	if ( profiles == null )
		//		profiles = new List<OrganizationRoleProfile>();

		//	List<OrganizationRoleProfile> list = FillAllOrgRoles( profiles,
		//			parentUid,
		//			isParentActor,
		//			ref messages );
		//	if ( messages.Count > msgCount )
		//		return false;

		//	//the parent needs to be established by using isParentActor
		//	Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );

		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		//get all existing roles for the parent
		//		var results = AgentEntityRole_GetAll( parentUid, isParentActor );

		//		#region deletes/updates check
		//		var deleteList = from existing in results
		//						 join item in list
		//						 on new { existing.ActingAgentId, existing.RoleTypeId }
		//						 equals new { item.ActingAgentId, item.RoleTypeId }
		//						 into joinTable
		//						 from result in joinTable.DefaultIfEmpty( new OrganizationRoleProfile { ActingAgentId = 0, ParentId = 0, Id = 0 } )
		//						 select new { ActingAgentId = existing.ActingAgentId, DeleteId = existing.Id, ItemId = ( result.RoleTypeId ), IsInverseRole = result.IsInverseRole };

		//		foreach ( var v in deleteList )
		//		{

		//			if ( v.ItemId == 0 )
		//			{
		//				//delete item
		//				if ( EntityAgentRole_Delete( v.DeleteId, ref statusMessage ) == false )
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
		//					  from addList in joinTable.DefaultIfEmpty( new OrganizationRoleProfile { Id = 0, ActingAgentId = 0, RoleTypeId = 0 } )
		//					  select new { ActingAgentId = item.ActingAgentId, RoleTypeId = item.RoleTypeId, ExistingId = addList.Id };
		//		foreach ( var v in newList )
		//		{
		//			if ( v.ExistingId == 0 )
		//			{
		//				bool isEmpty = false;
		//				if ( EntityAgentRole_Add( parent.Id,
		//							v.ActingAgentId,
		//							parent.EntityUid,
		//							parentTypeId,
		//							v.RoleTypeId,
		//							isParentActor,
		//							userId,
		//							ref messages,
		//							ref isEmpty ) == false )
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
		//private bool Agent_UpdateEntityRolesSingle( List<OrganizationRoleProfile> profiles, Guid parentUid, int parentTypeId, bool isInverseRole, int userId, ref List<string> messages, ref int count )
		//{
		//	bool isValid = true;
		//	//bool isInverseRole = false;
		//	count = 0;
		//	if ( !IsValidGuid( parentUid ) )
		//	{
		//		messages.Add( "Error: the parent identifier was not provided." );
		//	}
		//	if ( parentTypeId == 0 )
		//	{
		//		messages.Add( "Error: the parent type was not provided." );
		//	}
		//	if ( messages.Count > 0 )
		//		return false;

		//	//
		//	if ( profiles == null )
		//		profiles = new List<OrganizationRoleProfile>();

		//	List<OrganizationRoleProfile> list = FillAllOrgRoles( profiles, parentUid, ref messages );
		//	if ( messages.Count > 0 )
		//		return false;
		//	Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );

		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		//loop thru input, check for changes to existing, and for adds
		//		foreach ( OrganizationRoleProfile item in profiles )
		//		{
		//			int roleId = item.RoleTypeId;
		//			int entityId = parent.Id;
		//			if ( item.Id > 0 )
		//			{
		//				EM.Entity_AgentRelationship p = context.Entity_AgentRelationship.FirstOrDefault( s => s.Id == item.Id );
		//				if ( p != null && p.Id > 0 )
		//				{
		//					//p.ParentUid = parentUid;
		//					if ( roleId == 0 )
		//					{
		//						isValid = false;
		//						messages.Add( "Error: a role was not entered. Select a role and try again. " );
		//						continue;
		//					}
		//					if ( !IsValidGuid( item.ReverseAgentUid ) )
		//					{
		//						isValid = false;
		//						messages.Add( "Error: an agent was not selected. Select an agent and try again. " );
		//						continue;
		//					}
		//					p.RelationshipTypeId = roleId;

		//					p.URL = item.Url;
		//					p.Description = item.Description;
		//					p.EntityId = parent.Id;

		//					if ( HasStateChanged( context ) )
		//					{
		//						p.LastUpdated = System.DateTime.Now;
		//						p.LastUpdatedById = userId;
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
		//				if ( !IsValidGuid( item.ActingAgentUid ) && item.ActingAgentId > 0 )
		//				{
		//					//NOTE - need to handle agent!!!
		//					Organization org = OrganizationManager.Organization_Get( item.ActingAgentId, false );
		//					if ( org == null || org.Id == 0 )
		//					{
		//						isValid = false;
		//						messages.Add( string.Format( "Error: the selected organization was not found: {0}", item.ActingAgentId ) );
		//						continue;
		//					}
		//					item.ActingAgentUid = org.RowId;
		//				}

		//				bool isEmpty = false;
		//				if ( EntityAgentRole_Add( entityId, item.ActingAgentUid, parentUid, parentTypeId, roleId, isInverseRole, userId, ref messages, ref isEmpty ) == false )
		//					isValid = false;
		//				else
		//				{
		//					if ( !isEmpty )
		//						count++;
		//				}
		//			}

		//		}

		//	}
		//	return isValid;
		//}
		/// <summary>
		/// Add a single Entity to Agent relationship 
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="agentUid"></param>
		/// <param name="parentTypeId"></param>
		/// <param name="roleId"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <param name="url"></param>
		/// <param name="description"></param>
		/// <returns></returns>
		private bool EntityAgentRole_Add( int entityId, int agentId, Guid parentUid, int parentTypeId, int roleId, bool isParentActor, int userId, ref List<string> messages, ref bool isEmpty )
		{
			bool isValid = true;
			bool isInverseRole = !isParentActor;

			//assume if all empty, then ignore
			if ( !IsValidGuid( parentUid ) || ( agentId == 0 && roleId == 0 ) )
			{
				isEmpty = true;
				return true;
			}

			//
			if ( agentId > 0 && roleId == 0 )
			{
				messages.Add( "Error: invalid request, please select a role." );
				return false;
			}
			else if ( agentId == 0 && roleId > 0 )
			{
				messages.Add( "Error: invalid request, please select an agent." );
				return false;
			}
			//TODO - need to handle agent
			//Organization org = OrganizationManager.Organization_Get( agentId, false, false, true );
			//get lite object
			MN.ProfileLink org = OrganizationManager.Agent_GetProfileLink( agentId, CodesManager.ENTITY_TYPE_ORGANIZATION );
			if ( org == null || org.Name.Length == 0 )
			{
				messages.Add( "Error: the selected agent was not found!" );
				LoggingHelper.DoTrace( 5, thisClassname + string.Format( ".EntityAgentRole_Add the agent was not found, for entityTypeId: {0}, parent: {1}, AgentId:{1}, RoleId: {2}", parentTypeId, parentUid, agentId, roleId ) );
				return false;
			}

			//EntityAgentRelationship entity = AgentEntityRoleGet( parentUid, org.RowId, roleId );
			if ( AgentEntityRoleExists( parentUid, org.RowId, roleId ) )
			{
				messages.Add( "Error: the selected relationship already exists!" );
				return false;
			}


			using ( var context = new EM.CTIEntities() )
			{
				//add
				EM.Entity_AgentRelationship car = new EM.Entity_AgentRelationship();

				car.EntityId = entityId;
				if ( isParentActor )
				{

				}
				else
				{

				}
				//TODO - remove the use of these two, and only use entityId?
				car.ParentUid = parentUid;
				car.ParentTypeId = parentTypeId;

				car.AgentUid = org.RowId;
				car.RelationshipTypeId = roleId;
				car.IsInverseRole = isInverseRole;

				car.Created = System.DateTime.Now;
				car.CreatedById = userId;
				car.LastUpdated = System.DateTime.Now;
				car.LastUpdatedById = userId;
				car.RowId = Guid.NewGuid();
				context.Entity_AgentRelationship.Add( car );

				// submit the change to database
				int count = context.SaveChanges();
			}

			return isValid;
		}
		private bool EntityAgentRole_Add( int entityId, Guid agentUid, Guid parentUid, int parentTypeId, int roleId, bool isInverseRole, int userId, ref List<string> messages, ref bool isEmpty )
		{
			bool isValid = true;
			//assume if all empty, then ignore
			if ( !IsValidGuid( parentUid ) && !IsValidGuid( agentUid ) )
			{
				return true;
			}
			if ( !IsValidGuid( agentUid ) && roleId == 0 )
			{
				return true;
			}
			//
			if ( IsValidGuid( agentUid ) && roleId == 0 )
			{
				messages.Add( "Error: invalid request, please select a role." );
				return false;
			}
			else if ( !IsValidGuid( agentUid ) && roleId > 0 )
			{
				messages.Add( "Error: invalid request, please select an agent." );
				return false;
			}

			//EntityAgentRelationship entity = AgentEntityRoleGet( parentUid, agentUid, roleId );
			if ( AgentEntityRoleExists( parentUid, agentUid, roleId ) )
			{
				messages.Add( "Error: the selected relationship already exists!" );
				return false;
			}
			//TODO - need to handle agent
			Organization org = OrganizationManager.Agent_Get( agentUid, false );
			if ( org == null || org.Name.Length == 0 )
			{
				messages.Add( "Error: the selected agent was not found!" );
				LoggingHelper.DoTrace( 5, thisClassname + string.Format( ".EntityAgentRole_Add the agent was not found, for entityTypeId: {0}, parent: {1}, AgentId:{1}, RoleId: {2}", parentTypeId, parentUid, agentUid, roleId ) );
				return false;
			}

			using ( var context = new EM.CTIEntities() )
			{
				//add
				EM.Entity_AgentRelationship car = new EM.Entity_AgentRelationship();

				car.EntityId = entityId;
				//TODO - remove the use of these two
				car.ParentUid = parentUid;
				car.ParentTypeId = parentTypeId;

				car.AgentUid = agentUid;
				car.RelationshipTypeId = roleId;
				car.IsInverseRole = isInverseRole;

				car.Created = System.DateTime.Now;
				car.CreatedById = userId;
				car.LastUpdated = System.DateTime.Now;
				car.LastUpdatedById = userId;
				car.RowId = Guid.NewGuid();
				context.Entity_AgentRelationship.Add( car );

				// submit the change to database
				int count = context.SaveChanges();
			}

			return isValid;
		}

		#endregion

		/// <summary>
		/// In some cases, the interface allows selecting multiple roles for a single agent. These roles need to be split out into separate entities 
		/// Loop through all the org roles, and fill a work list
		/// </summary>
		/// <param name="profiles"></param>
		/// <param name="parentUid"></param>
		/// <param name="isParentActor"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		private List<OrganizationRoleProfile> FillAllOrgRoles( List<OrganizationRoleProfile> profiles,
					Guid parentUid,
					bool isParentActor,
					ref List<string> messages )
		{
			OrganizationRoleProfile entity = new OrganizationRoleProfile();
			List<OrganizationRoleProfile> list = new List<OrganizationRoleProfile>();
			if ( !IsValidGuid( parentUid ) )
				return list;

			int orgId = 0;
			Guid agentUid;
			bool isValidAgent = false;

			foreach ( OrganizationRoleProfile item in profiles )
			{
				orgId = item.ActingAgentId;
				agentUid = item.ActingAgentUid;

				if ( orgId > 0 || IsGuidValid( agentUid ) )
					isValidAgent = true;
				else
					isValidAgent = false;

				//loop thru the roles
				if ( item.RoleType != null && item.RoleType.Items.Count > 0 )
				{
					if ( !isValidAgent )
					{
						//roles, no agent
						messages.Add( "Invalid request, please select an agent for selected roles." );
						continue;
					}
					foreach ( EnumeratedItem e in item.RoleType.Items )
					{
						entity = new OrganizationRoleProfile();
						//entity.ParentId = parentId;
						if ( isParentActor )
						{
							entity.ActingAgentUid = parentUid;
							entity.ActingAgentId = orgId;
							entity.ActedUponEntityUid = agentUid;
						}
						else
						{
							entity.ActingAgentId = orgId;
							entity.ActingAgentUid = agentUid;
							entity.ActedUponEntityUid = parentUid;
						}

						entity.RoleTypeId = e.Id;
						list.Add( entity );
					}
				}
				else
				{
					//no roles
					if ( isValidAgent )
					{
						messages.Add( "Invalid request, please select one or more roles for this selected agent." );
					}
				}


			}
			return list;
		}

		#region roles retrieval ==================
		private static bool AgentEntityRoleExists( int entityId, Guid agentUid, int roleId )
		{
			EntityAgentRelationship item = new EntityAgentRelationship();
			using ( var context = new EM.CTIEntities() )
			{
				EM.Entity_AgentRelationship entity = context.Entity_AgentRelationship.FirstOrDefault( s => s.EntityId == entityId
						&& s.AgentUid == agentUid
						&& s.RelationshipTypeId == roleId );
				if ( entity != null && entity.Id > 0 )
				{
					return true;
				}
			}
			return false;
		}
		private static bool AgentEntityRoleExists( Guid pParentUid, Guid agentUid, int roleId )
		{
			EntityAgentRelationship item = new EntityAgentRelationship();
			using ( var context = new EM.CTIEntities() )
			{
				EM.Entity_AgentRelationship entity = context.Entity_AgentRelationship.FirstOrDefault( s => s.ParentUid == pParentUid
						&& s.AgentUid == agentUid
						&& s.RelationshipTypeId == roleId );
				if ( entity != null && entity.Id > 0 )
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Determine if any roles exist for the provided agent identifier.
		/// Typically used
		/// </summary>
		/// <param name="agentUid"></param>
		/// <returns></returns>
		public static bool AgentEntityHasRoles( Guid agentUid, ref int roleCount )
		{
			roleCount = 0;
			using ( var context = new EM.CTIEntities() )
			{
				List<EM.Entity_AgentRelationship> list = context.Entity_AgentRelationship
						.Where( s => s.AgentUid == agentUid )
						.ToList();
				if ( list != null && list.Count > 0 )
				{
					roleCount = list.Count;
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Retrieve a Parent - Agent - Role record, typically to determine if it exists
		/// </summary>
		/// <param name="pParentUid"></param>
		/// <param name="agentUid"></param>
		/// <param name="roleId"></param>
		/// <returns></returns>
		//private static EntityAgentRelationship AgentEntityRoleGet( Guid pParentUid, Guid agentUid, int roleId )
		//{
		//	EntityAgentRelationship item = new EntityAgentRelationship();
		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		EM.Entity_AgentRelationship entity = context.Entity_AgentRelationship.FirstOrDefault( s => s.ParentUid == pParentUid
		//				&& s.AgentUid == agentUid
		//				&& s.RelationshipTypeId == roleId );
		//		if ( entity != null && entity.Id > 0 )
		//		{
		//			item.Id = entity.Id;
		//			item.ParentUid = entity.ParentUid;
		//			item.AgentUid = entity.AgentUid;
	
		//			item.RelationshipTypeId = entity.RelationshipTypeId;

		//			if ( IsValidDate( entity.Created ) )
		//				entity.Created = ( DateTime ) entity.Created;
		//			entity.CreatedById = entity.CreatedById == null ? 0 : ( int ) entity.CreatedById;
		//			if ( IsValidDate( entity.LastUpdated ) )
		//				entity.LastUpdated = ( DateTime ) entity.LastUpdated;
		//			entity.LastUpdatedById = entity.LastUpdatedById == null ? 0 : ( int ) entity.LastUpdatedById;
		//			//item.Relationship = entity.Codes_EntityAgentRelationship.Description;
		//		}
		//		else
		//		{
		//			item.Id = 0;
		//		}

		//	}
		//	return item;

		//}

		/// <summary>
		/// Get summary version of roles (using CSV) - for use in lists
		/// It will not return org to org roles like departments, and subsiduaries
		/// </summary>
		/// <param name="pParentUid"></param>
		/// <param name="isParentActor"></param>
		/// <returns></returns>
		public static List<OrganizationRoleProfile> AgentEntityRole_GetAllSummary( Guid pParentUid, bool isParentActor = false )
		{
			//If parent is actor, then this is a direct role. 
			//for ex. if called from assessments, then it is inverse, as the parent is the assessment, and the relate org is the actor
			bool isInverseRole = !isParentActor;

			OrganizationRoleProfile orp = new OrganizationRoleProfile();
			List<OrganizationRoleProfile> list = new List<OrganizationRoleProfile>();
			using ( var context = new ViewContext() )
			{
				List<DBentity> agentRoles = context.Entity_AgentRelationshipIdCSV
					.Where( s => s.ParentUid == pParentUid
						 && s.IsInverseRole == isInverseRole )
					.ToList();
				foreach ( DBentity entity in agentRoles )
				{
					
					orp = new OrganizationRoleProfile();

					//warning for purposes of the editor, need to set the agent/object id to the orgId, and rowId from the org
					orp.Id = entity.AgentRelativeId;
					orp.RowId = ( Guid ) entity.AgentUid;

					//parent, ex credential, assessment, or org in org-to-org
					//Hmm should this be the entityId - need to be consistant
					orp.ParentId = entity.BaseId;
					orp.ParentUid = entity.ParentUid;  //this the entityUid
					orp.ParentTypeId = entity.ParentTypeId; //this is wrong, it is the parent of the entity

					orp.ActingAgentUid = entity.AgentUid;
					orp.ActingAgentId = entity.AgentRelativeId;
					orp.ProfileSummary = entity.AgentName;
					orp.ProfileName = entity.AgentName;

					//may be included now, but with addition of person, and use of agent, it won't
					
					orp.ActingAgent = new Organization()
					{
						Id = entity.AgentRelativeId,
						RowId = orp.ActingAgentUid,
						Name = entity.AgentName,
						Url = entity.AgentUrl,
						Description = entity.AgentDescription,
						ImageUrl = entity.AgentImageUrl
					};
					//useful for compare when doing deletes, and New checks
					orp.ActingAgentUid = entity.AgentUid;
					orp.ActingAgentId = entity.AgentRelativeId;
					
					//don't need actual roles for summary, but including
					orp.AllRoleIds = entity.RoleIds;
					orp.AllRoles = entity.Roles;
					//could include roles in profile summary??, particularly if small)

					orp.ProfileSummary = entity.AgentName;
					list.Add( orp );
				}

				if ( list.Count > 0 )
				{
					var Query = ( from roles in list.OrderBy( p => p.ProfileSummary )
								  select roles ).ToList();
					list = Query;
					//var Query = from roles in credential.OrganizationRole select roles;
					//Query = Query.OrderBy( p => p.ProfileSummary );
					//credential.OrganizationRole = Query.ToList();
				}
			}
			return list;

		} //

		/// <summary>
		/// Get all roles for a parent
		/// </summary>
		/// <param name="pParentUid"></param>
		/// <param name="isParentActor"></param>
		/// <returns></returns>
		public static List<OrganizationRoleProfile> AgentEntityRole_GetAll( Guid pParentUid, bool isParentActor = false )
		{
			//If parent is actor, then this is a direct role. 
			//for ex. if called from assessments, then it is inverse, as the parent is the assessment, and the relate org is the actor
			bool isInverseRole = !isParentActor;

			OrganizationRoleProfile p = new OrganizationRoleProfile();
			List<OrganizationRoleProfile> list = new List<OrganizationRoleProfile>();
			List<Views.Entity_Relationship_AgentSummary> roles = new List<Views.Entity_Relationship_AgentSummary>();
			using ( var context = new ViewContext() )
			{
				roles = context.Entity_Relationship_AgentSummary
						.Where( s => s.SourceEntityUid == pParentUid
						 && s.IsInverseRole == isInverseRole )
						.ToList();

				foreach ( Views.Entity_Relationship_AgentSummary entity in roles )
				{
					p = new OrganizationRoleProfile();
					p.Id = entity.EntityAgentRelationshipId;

					p.ParentUid = entity.SourceEntityUid;
					p.ParentTypeId = entity.SourceEntityTypeId;

					p.ActingAgentUid = entity.ActingAgentUid;
					//useful for compare when doing deletes, and New checks
					p.ActingAgentId = entity.AgentRelativeId;
					p.RoleTypeId = entity.RelationshipTypeId;

					string relation = entity.AgentToSourceRelationship;
					//if ( entity.Codes_CredentialAgentRelationship != null )
					//{
					//	relation = entity.Codes_CredentialAgentRelationship.ReverseRelation;
					//}
					p.IsInverseRole = (bool) (entity.IsInverseRole ?? true);
					//p.Url = entity.URL;
					//p.Description = entity.Description;

					//may be included now, but with addition of person, and use of agent, it won't
					//TODO - replace from view, when added
					//MN.ProfileLink agent = OrganizationManager.Agent_GetProfileLink( entity.ActingAgentUid );

					p.ActingAgent = new Organization()
					{
						Id = entity.AgentRelativeId,
						RowId = entity.ActingAgentUid,
						Name = entity.AgentName,
						Url = entity.AgentUrl,
						Description = entity.AgentDescription,
						ImageUrl = entity.AgentImageUrl
					};

					//p.ProfileSummary = string.Format( "{0} {1} this {2}", agent.Name, relation, entity.Codes_EntityType.Description );
					p.ProfileSummary = entity.AgentName;

					if ( IsValidDate( entity.Created ) )
						p.Created = ( DateTime ) entity.Created;
					p.CreatedById = entity.CreatedById;
					if ( IsValidDate( entity.LastUpdated ) )
						p.LastUpdated = ( DateTime ) entity.LastUpdated;
					p.LastUpdatedById = entity.LastUpdatedById;

					list.Add( p );
				}

			}
			return list;

		} //


		public static OrganizationRoleProfile AgentEntityRole_GetAsEnumerationFromCSV( Guid pParentUid, Guid agentUid, bool isInverseRole = true )
		{
			OrganizationRoleProfile orp = new OrganizationRoleProfile();
			EnumeratedItem eitem = new EnumeratedItem();

			using ( var context = new ViewContext() )
			{
				//there can be inconsistancies, resulting in more than one.
				//So use a list, and log/send email
				List<DBentity> agentRoles = context.Entity_AgentRelationshipIdCSV
					.Where( s => s.ParentUid == pParentUid
						 && s.AgentUid == agentUid )
					.ToList();

				foreach ( DBentity entity in agentRoles )
				{

					//DBentity entity = context.Entity_AgentRelationshipIdCSV
					//			.SingleOrDefault( s => s.ParentUid == pParentUid
					//				&& s.AgentUid == agentUid );
					//if ( entity != null && entity.AgentRelativeId > 0 )
					//{

						//warning for purposes of the editor, need to set the agent/object id to the orgId, and rowId from the org
						orp.Id = entity.AgentRelativeId;
						orp.RowId = ( Guid ) entity.AgentUid;

						//parent, ex credential, assessment, or org in org-to-org
						orp.ParentId = entity.BaseId;
						orp.ParentUid = entity.ParentUid;
						orp.ParentTypeId = entity.ParentTypeId;

						orp.ActingAgentUid = entity.AgentUid;
						orp.ActingAgentId = entity.AgentRelativeId;

						//TODO - do we still need this ==> YES
						orp.ActingAgent = new Organization()
						{
							Id = entity.AgentRelativeId,
							RowId = orp.ActingAgentUid,
							Name = entity.AgentName,
							Url = entity.AgentUrl,
							Description = entity.AgentDescription,
							ImageUrl = entity.AgentImageUrl
						};

						orp.ProfileSummary = entity.AgentName;
						orp.ProfileName = entity.AgentName;

						orp.AgentRole = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_ENTITY_AGENT_ROLE );
						orp.AgentRole.ParentId = entity.BaseId;


						orp.AgentRole.Items = new List<EnumeratedItem>();
						string[] roles = entity.RoleIds.Split( ',' );

						foreach ( string role in roles )
						{
							eitem = new EnumeratedItem();
							//??
							eitem.Id = int.Parse( role );
							//not used here
							eitem.RecordId = int.Parse( role );
							eitem.CodeId = int.Parse( role );
							eitem.Value = role.Trim();

							eitem.Selected = true;
							orp.AgentRole.Items.Add( eitem );

						}
					//}
						if ( agentRoles.Count > 1 )
						{
							//log an exception
							LoggingHelper.LogError( string.Format("Entity_AgentRelationshipManager.AgentEntityRole_GetAsEnumeration. Multiple records found where one expected. entity.BaseId: {0}, entity.ParentTypeId: {1}, entity.AgentRelativeId: {2}",entity.BaseId, entity.ParentTypeId, entity.AgentRelativeId), true );
						}
						break;
				}

			}
			return orp;

		} //
		

		/// <summary>
		/// Get all entity agent roles as an enumeration - uses the summary CSV view
		/// </summary>
		/// <param name="pParentUid"></param>
		/// <param name="isInverseRole"></param>
		/// <returns></returns>
		public static List<OrganizationRoleProfile> AgentEntityRole_GetAll_AsEnumeration( Guid pParentUid, bool isInverseRole = true )
		{
			OrganizationRoleProfile orp = new OrganizationRoleProfile();
			List<OrganizationRoleProfile> list = new List<OrganizationRoleProfile>();
			EnumeratedItem eitem = new EnumeratedItem();

			using ( var context = new ViewContext() )
			{
				List<DBentity> agentRoles = context.Entity_AgentRelationshipIdCSV
					.Where( s => s.ParentUid == pParentUid
						 && s.IsInverseRole == isInverseRole )
					.ToList();

				foreach ( DBentity entity in agentRoles )
				{
					orp = new OrganizationRoleProfile();
					orp.Id = 0;
					orp.ParentUid = entity.ParentUid;
					orp.ParentTypeId = entity.ParentTypeId;
					orp.ActingAgentUid = entity.AgentUid;
					orp.ActingAgentId = entity.AgentRelativeId;
					orp.ActingAgent = new Organization()
					{
						Id = entity.AgentRelativeId,
						RowId = entity.AgentUid,
						Name = entity.AgentName,
						Url = entity.AgentUrl,
						Description = entity.AgentDescription,
						ImageUrl = entity.AgentImageUrl
					};

					orp.ProfileSummary = entity.Name;

					orp.AgentRole = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_ENTITY_AGENT_ROLE );
					orp.AgentRole.ParentId = entity.BaseId;
					orp.AgentRole.Items = new List<EnumeratedItem>();
					string[] roles = entity.RoleIds.Split( ',' );

					foreach ( string role in roles )
					{
						eitem = new EnumeratedItem();
						//??
						eitem.Id = int.Parse( role );
						//not used here
						eitem.RecordId = int.Parse( role );
						eitem.CodeId = int.Parse( role );
						eitem.Value = role.Trim();

						eitem.Selected = true;
						//don't have this from the csv list
						//if ( ( bool ) role.IsQARole )
						//{
						//	eitem.IsSpecialValue = true;
						//	if ( IsDevEnv() )
						//		eitem.Name += " (QA)";
						//}

						orp.AgentRole.Items.Add( eitem );

					}

					list.Add( orp );
					if ( list.Count > 0 )
					{
						var Query = ( from items in list.OrderBy( p => p.ProfileSummary )
									  select items ).ToList();
						list = Query;
					}
				}

			}
			return list;

		} //

		/// <summary>
		/// Get all roles for an entity. 
		/// The flat roles (one entity - role - agent per record) are read and returned as enumerations - fully filled out
		/// </summary>
		/// <param name="pParentUid"></param>
		/// <param name="isInverseRole"></param>
		/// <returns></returns>
		public static List<OrganizationRoleProfile> AgentEntityRole_GetAll_ToEnumeration( Guid pParentUid, bool isInverseRole )
		{
			OrganizationRoleProfile orp = new OrganizationRoleProfile();
			List<OrganizationRoleProfile> list = new List<OrganizationRoleProfile>();
			EnumeratedItem eitem = new EnumeratedItem();
			int prevAgentId = 0;

			using ( var context = new ViewContext() )
			{
				//order by type, name (could have duplicate names!), then relationship
				List<DBentitySummary> agentRoles = context.Entity_Relationship_AgentSummary
					.Where( s => s.SourceEntityUid == pParentUid
						 && s.IsInverseRole == isInverseRole )
						 .OrderBy( s => s.ActingAgentEntityType )
						 .ThenBy( s => s.AgentName ).ThenBy( s => s.AgentRelativeId )
						 .ThenBy( s => s.SourceToAgentRelationship )
					.ToList();

				foreach ( DBentitySummary entity in agentRoles )
				{
					//loop until change in agent
					if ( prevAgentId != entity.AgentRelativeId )
					{
						//handle previous fill
						if ( prevAgentId > 0)
							list.Add( orp );

						prevAgentId = entity.AgentRelativeId;

						orp = new OrganizationRoleProfile();
						orp.Id = 0;
						orp.ParentUid = entity.SourceEntityUid;
						orp.ParentTypeId = entity.SourceEntityTypeId;

						orp.ActingAgentUid = entity.ActingAgentUid;
						orp.ActingAgentId = entity.AgentRelativeId;
						orp.ActingAgent = new Organization()
						{
							Id = entity.AgentRelativeId,
							RowId = entity.ActingAgentUid,
							Name = entity.AgentName,
							Url = entity.AgentUrl,
							Description = entity.AgentDescription,
							ImageUrl = entity.AgentImageUrl
						};

						orp.ProfileSummary = entity.AgentName;

						orp.AgentRole = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_ENTITY_AGENT_ROLE );
						orp.AgentRole.ParentId = entity.AgentRelativeId;

						orp.AgentRole.Items = new List<EnumeratedItem>();
					}
					

					eitem = new EnumeratedItem();
					//??
					eitem.Id = entity.EntityAgentRelationshipId;
					eitem.RowId = entity.RowId.ToString();
					//not used here
					eitem.RecordId = entity.EntityAgentRelationshipId;
					eitem.CodeId = entity.RelationshipTypeId;
					eitem.Value = entity.RelationshipTypeId.ToString();
					//WARNING - the code table uses Accredited by as the title and the latter is actually the reverse (using our common context), so we need to reverse the returned values here 
					if ( !isInverseRole )
					{
						eitem.Name = entity.AgentToSourceRelationship;
						eitem.SchemaName = entity.ReverseSchemaTag;
					}
					else
					{
						eitem.Name = entity.SourceToAgentRelationship;
						eitem.SchemaName = entity.SchemaTag;
					}
					//TODO - if needed	
					//eitem.Description = entity.RelationshipDescription;

					eitem.Selected = true;
					if ( ( bool ) entity.IsQARole )
					{
						eitem.IsSpecialValue = true;
						if ( IsDevEnv() )
							eitem.Name += " (QA)";
					}

					orp.AgentRole.Items.Add( eitem );

				}
				//check for remaining
				if ( prevAgentId > 0 )
					list.Add( orp );

				if ( list.Count > 0 )
				{
					var Query = ( from items in list.OrderBy( p => p.ProfileSummary )
								  select items ).ToList();
					list = Query;
				}

			}
			return list;

		} //


		/// <summary>
		/// Get all departments and subsiduaries for the parent org
		/// </summary>
		/// <param name="pParentUid"></param>
		/// <param name="roleTypeId">If zero, get both otherwise get specific roles</param>
		/// <returns></returns>
		public static void AgentRole_FillAllSubOrganizations( Organization parent, int roleTypeId = 0 )
		{
			OrganizationRoleProfile p = new OrganizationRoleProfile();
			List<OrganizationRoleProfile> list = new List<OrganizationRoleProfile>();
			parent.OrganizationRole_Dept = new List<OrganizationRoleProfile>();
			parent.OrganizationRole_Subsiduary = new List<OrganizationRoleProfile>();

			using ( var context = new ViewContext() )
			{
				List<Views.Entity_Relationship_AgentSummary> roles = context.Entity_Relationship_AgentSummary
					.Where( s => s.SourceEntityUid == parent.RowId
						 && (
								( roleTypeId == 0
								&& ( s.RelationshipTypeId == ROLE_TYPE_DEPARTMENT || s.RelationshipTypeId == ROLE_TYPE_SUBSIDUARY ) )
							|| ( s.RelationshipTypeId == roleTypeId )
						 )
						 )
						 .OrderBy( s => s.RelationshipTypeId ).ThenBy( s => s.AgentName )
					.ToList();

				foreach ( Views.Entity_Relationship_AgentSummary entity in roles )
				{
					p = new OrganizationRoleProfile();
					p.Id = entity.EntityAgentRelationshipId;
					p.ParentUid = entity.SourceEntityUid;
					p.ParentTypeId = entity.SourceEntityTypeId;
					p.ActingAgentUid = entity.ActingAgentUid;

					p.ActingAgent = new Organization()
					{
						Id = entity.AgentRelativeId,
						RowId = entity.ActingAgentUid,
						Name = entity.AgentName,
						Url = entity.AgentUrl,
						Description = entity.AgentDescription,
						ImageUrl = entity.AgentImageUrl
					};

					p.RoleTypeId = entity.RelationshipTypeId;
					string relation = "";
					if ( entity.SourceToAgentRelationship != null )
					{
						relation = entity.SourceToAgentRelationship;
					}
					p.IsInverseRole = entity.IsInverseRole ?? false;

					p.ProfileSummary = string.Format( "{0} is a {1} of {2}", entity.AgentName, relation, entity.SourceEntityName );

					//if ( IsValidDate( entity.Created ) )
					//	p.Created = ( DateTime ) entity.Created;
					//p.CreatedById = entity.CreatedById == null ? 0 : ( int ) entity.CreatedById;
					//if ( IsValidDate( entity.LastUpdated ) )
					//	p.LastUpdated = ( DateTime ) entity.LastUpdated;
					//p.LastUpdatedById = entity.LastUpdatedById == null ? 0 : ( int ) entity.LastUpdatedById;

					if ( entity.RelationshipTypeId == ROLE_TYPE_DEPARTMENT )
						parent.OrganizationRole_Dept.Add( p );
					else if ( entity.RelationshipTypeId == ROLE_TYPE_SUBSIDUARY )
						parent.OrganizationRole_Subsiduary.Add( p );
					//list.Add( p );
				}

			}
			//return list;

		} //
		//public static List<OrganizationRoleProfile> AgentToAgentRole_GetAll( Guid pAgentUid, bool isInverseRole = false )
		//{
		//	OrganizationRoleProfile p = new OrganizationRoleProfile();
		//	List<OrganizationRoleProfile> list = new List<OrganizationRoleProfile>();
		//	using ( var context = new ViewContext() )
		//	{
		//		List<EM.Entity_AgentRelationship> roles = context.Entity_AgentRelationship
		//			.Where( s => s.AgentUid == pAgentUid
		//			&& s.IsInverseRole == isInverseRole)
		//			.ToList();

		//		foreach ( EM.Entity_AgentRelationship entity in roles )
		//		{
		//			p = new OrganizationRoleProfile();
		//			p.Id = entity.Id;
		//			p.ParentUid = entity.ParentUid;
		//			p.ParentTypeId = entity.ParentTypeId;
		//			p.ActingAgentUid = entity.AgentUid;
		//			p.RoleTypeId = entity.RelationshipTypeId;
		//			string relation = "";
		//			if ( entity.Codes_CredentialAgentRelationship != null )
		//			{
		//				relation = entity.Codes_CredentialAgentRelationship.ReverseRelation;
		//			}

		//			p.Url = entity.URL;
		//			p.Description = entity.Description;

		//			//may be included now, but with addition of person, and use of agent, it won't
		//			//TODO - replace from view, when added
		//			Organization agent = OrganizationManager.Agent_Get( entity.AgentUid );

		//			p.ProfileSummary = string.Format( "{0} {1} this {2}", agent.Name, relation, entity.Codes_EntityType.Description );


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

		#endregion

		#region CREDENTIAL relationships
		/// <summary>
		/// Get all credentials for the organization, and relationship
		/// </summary>
		/// <param name="orgId"></param>
		/// <param name="relationship">Defaults to Created by (5)</param>
		/// <returns></returns>
		public static List<Credential> Credentials_ForCreatingOrg(Guid orgId)
		{
			List<Credential> list = new List<Credential>();
			Credential credential = new Credential();
			using (var context = new Data.CTIEntities())
			{
				var creds = from cred in context.Credential
							join entity in context.Entity
							on cred.RowId equals entity.EntityUid
							join agent in context.Entity_AgentRelationship
							on entity.Id equals agent.EntityId
							where cred.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED 
								&& agent.AgentUid == orgId 
								&& agent.RelationshipTypeId == 5
							select cred;
				var results = creds.ToList();

				if (results != null && results.Count > 0)
				{
					foreach (EM.Credential item in results)
					{
						credential = new Credential();
						credential.IsNewVersion = true;
						//TODO - don't need a full map for the list!
						//ToMap(item, credential, false);
						credential.Id = item.Id;
						credential.StatusId = (int) (item.StatusId ?? 1);
						credential.RowId = item.RowId;
						credential.Name = item.Name;
						credential.AlternateName = item.AlternateName;
						credential.Url = item.Url;
						credential.Description = item.Description;

						list.Add(credential);
					}
				}
			}

			return list;
		}

		/// <summary>
		/// Get total count of credentials where the provided org is the creator
		/// </summary>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public static int CredentialCount_ForCreatingOrg(Guid orgId)
		{
			int count = 0;
			using (var context = new Data.CTIEntities())
			{
				var creds = from cred in context.Credential
							join entity in context.Entity
							on cred.RowId equals entity.EntityUid
							join agent in context.Entity_AgentRelationship
							on entity.Id equals agent.EntityId
							where cred.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
								&& agent.AgentUid == orgId
								&& agent.RelationshipTypeId == 5
							select cred;
				var results = creds.ToList();

				if (results != null && results.Count > 0)
				{
					count = results.Count;
				}
			}

			return count;
		}

		#endregion

		#region role codes retrieval ==================
		/// <summary>
		/// Get agent to agent roles
		/// </summary>
		/// <param name="isInverseRole">false - Created by, true - created</param>
		/// <returns></returns>
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
						ToMap( item, val, isInverseRole );
						entity.Items.Add( val );
					}

				}
			}

			return entity;
		}

		/// <summary>
		/// Get agent roles for assessments and learning opportunities
		/// </summary>
		/// <param name="isInverseRole">false - Created by, true - created</param>
		/// <returns></returns>
		public static Enumeration GetAllOtherAgentRoles( bool isInverseRole = true )
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

					var Query = from P in context.Codes_CredentialAgentRelationship
							.Where( s => s.IsActive == true 
								&& s.IsAssessmentAgentRole == true )
								select P;

					Query = Query.OrderBy( p => p.Title );
					var results = Query.ToList();

					foreach ( EM.Codes_CredentialAgentRelationship item in results )
					{
						val = new EnumeratedItem();
						ToMap( item, val, isInverseRole );
						entity.Items.Add( val );
					}

				}
			}

			return entity;
		}

		public static Enumeration GetLearningOppAgentRoles( bool isInverseRole = true )
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

					var Query = from P in context.Codes_CredentialAgentRelationship
							.Where( s => s.IsActive == true
								&& s.IsLearningOppAgentRole == true )
								select P;

					Query = Query.OrderBy( p => p.Title );
					var results = Query.ToList();

					foreach ( EM.Codes_CredentialAgentRelationship item in results )
					{
						val = new EnumeratedItem();
						ToMap( item, val, isInverseRole );
						entity.Items.Add( val );
						//val.Id = item.Id;
						//val.CodeId = item.Id;
						//val.Value = item.Id.ToString();//????
						//val.Description = item.Description;
						//val.SchemaName = item.SchemaTag;

						//if ( isInverseRole )
						//{
						//	val.Name = item.ReverseRelation;
						//}
						//else
						//{
						//	val.Name = item.Title;
						//}

						//if ( ( bool ) item.IsQARole )
						//{
						//	val.IsSpecialValue = true;
						//	if ( IsDevEnv() )
						//		val.Name += " (QA)";
						//}
						//entity.Items.Add( val );
					}

				}
			}

			return entity;

		}
		private static void ToMap( EM.Codes_CredentialAgentRelationship from, EnumeratedItem to, bool isInverseRole = true )
		{
			to.Id = from.Id;
			to.CodeId = from.Id;
			to.Value = from.Id.ToString();//????
			to.Description = from.Description;
			to.SchemaName = from.SchemaTag;

			if ( isInverseRole )
			{
				to.Name = from.ReverseRelation;
			}
			else
			{
				to.Name = from.Title;
				//val.Description = string.Format( "{0} is {1} by this Organization ", entityType, item.Title );
			}

			if ( ( bool ) from.IsQARole )
			{
				to.IsSpecialValue = true;
				if ( IsDevEnv() )
					to.Name += " (QA)";
			}
		}
		#endregion
	}
}
