using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Factories;
using Models;
using Models.Common;
using Models.ProfileModels;
using Utilities;
using ProfileMgr = Factories.CostProfileManager;

namespace CTIServices
{
	public class ProfileServices
	{
		string thisClassName = "ProfileServices";
		ActivityServices activityMgr = new ActivityServices();
		#region Cost Profile

		#region retrieval
		/// <summary>
		/// Get all CostProfile for a parent
		/// </summary>
		/// <param name="parentId"></param>
		/// <returns></returns>
		public static List<CostProfile> CostProfile_GetAll( Guid parentUid )
		{
			List<CostProfile> list = ProfileMgr.CostProfile_GetAll( parentUid );
			return list;
		}

		/// <summary>
		/// Get a CostProfile By integer Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static CostProfile CostProfile_Get( int profileId )
		{
			CostProfile profile = ProfileMgr.CostProfile_Get( profileId );
			return profile;
		}
		public static CostProfile CostProfile_Get( Guid profileUid )
		{
			CostProfile profile = ProfileMgr.CostProfile_Get( profileUid );
			return profile;
		}
		#endregion

		#region Persistance
		public bool CostProfile_Save( CostProfile entity, Guid parentUid, string action, AppUser user, ref string status )
		{
			bool valid = true;
			status = "";

			entity.IsNewVersion = true;

			List<string> messages = new List<string>();
			CostProfileManager mgr = new CostProfileManager();
			//validate user has access. Parent can be multiple types, but aways is an entity
			//Credential credential = CredentialServices.GetBasicCredential( credentialUid );

			try
			{
				entity.ParentUid = parentUid;
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				if ( mgr.CostProfile_Save( entity, parentUid, user.Id, ref messages ) == false )
				{
					valid = false;
					status = string.Join( ",", messages.ToArray() );
				}
				else
				{
					status = "Successfully Saved Cost Profile ";
					//should the activity be for the parent
					activityMgr.AddActivity( "Cost Profile", action, string.Format( "{0} saved cost profile: {1}", user.FullName(), entity.ProfileName ), user.Id, 0, entity.Id );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".CostProfile_Save" );
				valid = false;
				status = ex.Message;
			}
			return valid;
		}
		public bool CostProfile_Delete( int profileId, AppUser user, ref string status )
		{
			bool valid = true;
			CostProfileManager mgr = new CostProfileManager();
			try
			{
				//get profile and ensure user has access
				CostProfile profile = ProfileMgr.CostProfile_Get( profileId, false );
				if ( profile == null || profile.Id == 0 )
				{
					status = "Error - the requested profile was not found.";
					return false;
				}

				//if ( CanUserUpdateCredential( profile.ParentId, user, ref status ) )
				//{
				if ( mgr.CostProfile_Delete( profileId, ref status ) )
					{
						//if valid, log
						activityMgr.AddActivity( "Cost Profile", "Delete", string.Format( "{0} deleted Cost Profile {1} ({2}) from Credential: {3}", user.FullName(), profile.ProfileName, profileId, profile.ParentId ), user.Id, 0, profileId );
						status = "";
					}
					else
					{
						status = "Error - delete failed: " + status;
						return false;
					}
				//}
				//else
				//{
				//	//reject and log
				//	status = "Error - the requested profile was not found.";
				//	return false;
				//}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".CostProfile_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}
		#endregion

		#endregion

		#region CostProfileItem

		#region retrieval
		/// <summary>
		/// Get all CostProfileItem for a parent
		/// </summary>
		/// <param name="parentId"></param>
		/// <returns></returns>
		public static List<CostProfileItem> CostProfileItem_GetAll( int parentId )
		{
			List<CostProfileItem> list = CostProfileItemManager.CostProfileItem_GetAll( parentId );
			return list;
		}

		/// <summary>
		/// Get a CostProfileItem By integer Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static CostProfileItem CostProfileItem_Get( int profileId )
		{
			CostProfileItem profile = CostProfileItemManager.CostProfileItem_Get( profileId );
			return profile;
		}
	
		#endregion

		#region Persistance
		public bool CostProfileItem_Save( CostProfileItem entity, Guid parentUid, string action, AppUser user, ref string status )
		{
			bool valid = true;
			status = "";

			entity.IsNewVersion = true;

			List<string> messages = new List<string>();
			CostProfileItemManager mgr = new CostProfileItemManager();
			//validate user has access. Parent can be multiple types, but aways is an entity
			CostProfile parent = CostProfile_Get( parentUid );
			//Credential credential = CredentialServices.GetBasicCredential( credentialUid );

			try
			{

				if ( mgr.CostProfileItem_Save( entity, parent.Id, user.Id, ref messages ) == false )
				{
					valid = false;
					status = string.Join( ",", messages.ToArray() );
				}
				else
				{
					status = "Successfully Saved Cost Profile Item";
					//should the activity be for the parent
					activityMgr.AddActivity( "Cost Profile Item", action, string.Format( "{0} saved cost profile item: {1}", user.FullName(), entity.ProfileName ), user.Id, 0, entity.Id );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".CostProfileItem_Save" );
				valid = false;
				status = ex.Message;
			}
			return valid;
		}
		public bool CostProfileItem_Delete( int profileId, AppUser user, ref string status )
		{
			bool valid = true;
			CostProfileItemManager mgr = new CostProfileItemManager();
			try
			{
				//get profile and ensure user has access
				CostProfileItem profile = CostProfileItemManager.CostProfileItem_Get( profileId, false );
				if ( profile == null || profile.Id == 0 )
				{
					status = "Error - the requested profile was not found.";
					return false;
				}

				//if ( CanUserUpdateCredential( profile.ParentId, user, ref status ) )
				//{
				if ( mgr.CostProfileItem_Delete( profileId, ref status ) )
				{
					//if valid, log
					activityMgr.AddActivity( "Cost Profile Item", "Delete", string.Format( "{0} deleted Cost Profile Item {1} ({2}) from Credential: {3}", user.FullName(), profile.ProfileName, profileId, profile.ParentId ), user.Id, 0, profileId );
					status = "";
				}
				else
				{
					status = "Error - delete failed: " + status;
					return false;
				}
				//}
				//else
				//{
				//	//reject and log
				//	status = "Error - the requested profile was not found.";
				//	return false;
				//}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".CostProfileItem_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}
		#endregion

		#endregion
		#region Entity Credential Reference Profile
		#region retrieval
		/// <summary>
		/// Get all Entity_Credential for a parent
		/// </summary>
		/// <param name="parentId"></param>
		/// <returns></returns>
		public static List<Entity_Credential> Entity_Credential_GetAll( int parentId )
		{
			List<Entity_Credential> list = Entity_CredentialManager.GetAll( parentId );
			return list;
		}

		/// <summary>
		/// Get a Entity_Credential By integer Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Entity_Credential EntityCredential_Get( int profileId )
		{
			Entity_Credential profile = Entity_CredentialManager.Entity_Get( profileId );
			return profile;
		}

		#region Persistance
		public int EntityCredential_Save( Guid parentUid, int credentialId, AppUser user, ref bool valid, ref string status )
		{
			valid = true;
			int newId = 0;
			status = "";
			//AppUser user = AccountServices.GetCurrentUser();
			if ( user == null || user.Id == 0 )
			{
				status = "Error user must be logged in";
				return 0;
			}

			List<string> messages = new List<string>();
			Entity_CredentialManager mgr = new Entity_CredentialManager();
			//validate user has access. Parent can be multiple types, but aways is an entity
			//??
			try
			{

				if ( mgr.EntityCredential_Add( credentialId, parentUid, user.Id, ref newId, ref messages ) == false )
				{
					valid = false;
					status = string.Join( ",", messages.ToArray() );
				}
				else
				{
					status = "Successfully Saved Item";
					//should the activity be for the parent
					//activityMgr.AddActivity( "Cost Profile Item", action, string.Format( "{0} saved cost profile item: {1}", user.FullName(), entity.ProfileName ), user.Id, 0, entity.Id );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".EntityCredential_Save" );
				valid = false;
				newId = 0;
				status = ex.Message;
			}
			return newId;
		}

		/// <summary>
		/// delete a related credential from an Entity
		/// </summary>
		/// <param name="profileId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool EntityCredential_Delete( Guid parentUid, int credentialId, AppUser user, ref string status )
		{
			bool valid = true;
			Entity_CredentialManager mgr = new Entity_CredentialManager();
			try
			{

				EntitySummary parent = EntityManager.GetEntitySummary( parentUid );
				if ( parent == null || parent.Id == 0 )
				{
					status = "Error - the parent entity was not found." ;
					return false;
				}

				//get profile and ensure user has access
				Entity_Credential profile = Entity_CredentialManager.Entity_Get( parent.Id, credentialId );
				if ( profile == null || profile.Id == 0 )
				{
					status = "Error - the requested profile was not found.";
					return false;
				}
				//a little more difficult - plan to use the entity and bubble up to the top object
				//if ( CanUserUpdateCredential( profile.ParentId, user, ref status ) )
				//{
				if ( mgr.Entity_Delete( parent.Id, credentialId, ref status ) )
				{
					//if valid, log
					//activityMgr.AddActivity( "Cost Profile Item", "Delete", string.Format( "{0} deleted Cost Profile Item {1} ({2}) from Credential: {3}", user.FullName(), profile.ProfileName, profileId, profile.ParentId ), user.Id, 0, profileId );
					status = "Successfully removed credential";
				}
				else
				{
					status = "Error - delete failed: " + status;
					return false;
				}

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".EntityCredential_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}
		#endregion
		#endregion
		#endregion

		#region Entity Reference Profile
	
		/// <summary>
		/// Get all Entity_Credential for a parent
		/// </summary>
		/// <param name="parentId"></param>
		/// <param name="categoryId"></param>
		/// <returns></returns>
		public static List<TextValueProfile> Entity_Reference_GetAll( Guid parentUid, int categoryId = 25 )
		{
			List<TextValueProfile> list = Entity_ReferenceManager.Entity_GetAll( parentUid, categoryId );
			return list;
		}

		/// <summary>
		/// Get a list of Entity.References using a list of integers
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public static List<TextValueProfile> Entity_Reference_GetList( List<int> ids )
		{
			List<TextValueProfile> profiles = Entity_ReferenceManager.GetList( ids );
			return profiles;

		}
		/// <summary>
		/// Get a Entity_Reference By integer Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static TextValueProfile Entity_Reference_Get( int profileId )
		{
			TextValueProfile profile = Entity_ReferenceManager.Entity_Get( profileId );
			return profile;
		}

		/// <summary>
		/// Delete an entity.reference entity
		/// The 
		/// </summary>
		/// <param name="parentId">This is the Id of the container, not the EntityId from Entity</param>
		/// <param name="profileId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool Entity_Reference_Delete( int parentId, int profileId, AppUser user, ref string status )
		{
			bool valid = true;
			Entity_ReferenceManager mgr = new Entity_ReferenceManager();
			try
			{
				//get profile and ensure user has access
				TextValueProfile profile = Entity_ReferenceManager.GetSummary( profileId );
				if ( profile == null || profile.Id == 0 )
				{
					status = "Error - the requested profile was not found.";
					return false;
				}
				else if ( profile.EntityBaseId != parentId )
				{
					status = "Error - invalid parentId";
					return false;
				}

				//if ( CanUserUpdateCredential( profile.ParentId, user, ref status ) )
				//{
				if ( mgr.Delete( profileId, ref status ) )
				{
					//if valid, log
					activityMgr.AddActivity( "Entity Reference", "Delete", string.Format( "{0} deleted Entity Reference {1} ({2}) from Parent: {3}", user.FullName(), profile.ProfileName, profileId, profile.ParentId ), user.Id, 0, profileId );
					status = "";
				}
				else
				{
					status = "Error - delete failed: " + status;
					return false;
				}
				//}
				//else
				//{
				//	//reject and log
				//	status = "Error - the requested profile was not found.";
				//	return false;
				//}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Entity_Reference_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}
		#endregion

		#region AddressProfile

		#region retrieval
		/// <summary>
		/// Get all Addresses for a parent
		/// </summary>
		/// <param name="parentId"></param>
		/// <returns></returns>
		public static List<Address> AddressProfile_GetAllOrgAddresses( int parentId )
		{
			List<Address> list = AddressProfileManager.GetAllOrgAddresses( parentId );
			return list;
		}

		/// <summary>
		/// Get a Address By integer Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Address AddressProfile_Get( int profileId, string parentType )
		{
			if ( parentType == "Organization" )
			{
				Address profile = AddressProfileManager.GetOrganizationAddress( profileId );
				return profile;
			}
			else
			{
				Address profile = AddressProfileManager.Entity_Address_Get( profileId );
				return profile;
			}
		}

		#endregion

		#region Persistance
		public bool AddressProfile_Save( Address entity, Guid parentUid, string action, AppUser user, ref string status )
		{
			return AddressProfile_Save( entity, parentUid, "Add", "Organization", user, ref status );
		}

		public bool AddressProfile_Save( Address entity, Guid parentUid, string action, string parentType, AppUser user, ref string status )
		{
			bool valid = true;
			status = "";

			entity.IsNewVersion = true;

			List<string> messages = new List<string>();
			AddressProfileManager mgr = new AddressProfileManager();
			//validate user has access. Parent can be multiple types, but always is an entity
			//Credential credential = CredentialServices.GetBasicCredential( credentialUid );

			try
			{
				entity.CreatedById = entity.LastUpdatedById = user.Id;
				if ( parentType == "Organization" )
				{
					valid = mgr.Save( entity, parentUid, user.Id, ref messages );
				}
				else
				{
					valid = mgr.Entity_Address_Save( entity, parentUid, user.Id, ref messages );
				}
				if ( valid )
				{
					status = "Successfully Saved Address";
					//should the activity be for the parent
					activityMgr.AddActivity( "Address Profile", action, string.Format( "{0} saved address profile: {1}", user.FullName(), entity.Name ), user.Id, 0, entity.Id );
				}
				else
				{
					status = string.Join( ",", messages.ToArray() );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".AddressProfile_Save" );
				valid = false;
				status = ex.Message;
			}
			return valid;
		}
		/// <summary>
		/// Delete an Address profile
		/// </summary>
		/// <param name="profileId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool AddressProfile_Delete( int profileId, string parentType, AppUser user, ref string status )
		{
			bool valid = true;
			AddressProfileManager mgr = new AddressProfileManager();
			try
			{
				//get profile and ensure user has access
				if ( parentType == "Organization" )
				{
					Address profile = AddressProfileManager.GetOrganizationAddress( profileId );
					if ( profile == null || profile.Id == 0 )
					{
						status = "Error - the requested address was not found.";
						return false;
					}

					if ( OrganizationServices.CanUserUpdateOrganization( user, profile.ParentId ) )
					{
						if ( mgr.Delete( profileId, ref status ) )
						{
							//if valid, log
							activityMgr.AddActivity( "Address Profile", "Delete", string.Format( "{0} deleted Address Profile {1} ({2}) from Organization: {3}", user.FullName(), profile.Name, profileId, profile.ParentId ), user.Id, 0, profileId );
							status = "";
						}
						else
						{
							status = "Error - delete failed: " + status;
							return false;
						}
					}
					else
					{
						//reject and log
						status = "Error - the requested profile was not found.";
						string msg = string.Format( "UNAUTHORIZED USER: {0} attempted to delete Address Profile {1} ({2}) from Organization: {3}", user.FullName(), profile.Name, profileId, profile.ParentId );
						//activityMgr.AddActivity( "Address Profile", "Delete", msg , user.Id, 0, profileId );
						activityMgr.AddActivity( new SiteActivity()
						{
							Activity = "Address Profile",
							Event = "Unauthorized Delete Attempt",
							Comment = msg,
							ActionByUserId = user.Id,
							TargetObjectId = profileId,
							ObjectRelatedId = profile.ParentId
						} );

						LoggingHelper.LogError( msg, true );
						return false;
					}
				}
				else
				{
					Address profile = AddressProfileManager.Entity_Address_Get( profileId );
					if ( profile == null || profile.Id == 0 )
					{
						status = "Error - the requested address was not found.";
						return false;
					}

					if ( OrganizationServices.CanUserUpdateOrganization( user, profile.ParentId ) )
					{
						if ( mgr.Entity_Address_Delete( profileId, ref status ) )
						{
							//if valid, log
							activityMgr.AddActivity( "Address Profile", "Delete", string.Format( "{0} deleted Address Profile {1} ({2}) from Organization: {3}", user.FullName(), profile.Name, profileId, profile.ParentId ), user.Id, 0, profileId );
							status = "";
						}
						else
						{
							status = "Error - delete failed: " + status;
							return false;
						}
					}
					else
					{
						//reject and log
						status = "Error - the requested profile was not found.";
						string msg = string.Format( "UNAUTHORIZED USER: {0} attempted to delete Address Profile {1} ({2}) from Organization: {3}", user.FullName(), profile.Name, profileId, profile.ParentId );
						//activityMgr.AddActivity( "Address Profile", "Delete", msg , user.Id, 0, profileId );
						activityMgr.AddActivity( new SiteActivity()
						{
							Activity = "Address Profile",
							Event = "Unauthorized Delete Attempt",
							Comment = msg,
							ActionByUserId = user.Id,
							TargetObjectId = profileId,
							ObjectRelatedId = profile.ParentId
						} );

						LoggingHelper.LogError( msg, true );
						return false;
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".AddressProfile_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}
		#endregion

		#endregion


		#region Verification Profile
		public static AuthenticationProfile AuthenticationProfile_Get( int profileId )
		{
			AuthenticationProfile profile = Entity_VerificationProfileManager.VerificationProfile_Get( profileId );

			return profile;
		}
		public bool AuthenticationProfile_Save( AuthenticationProfile entity, Guid parentUid, string action, AppUser user, ref string status, bool isQuickCreate = false )
		{
			bool isValid = true;
			List<String> messages = new List<string>();
			if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
			{
				messages.Add( "Error - missing an identifier for the Verification Profile" );
				return false;
			}

			try
			{
				Entity e = EntityManager.GetEntity( parentUid );
				//remove this if properly passed from client
				//plus need to migrate to the use of EntityId
				entity.ParentId = e.Id;
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				if ( isQuickCreate )
				{
					//?may not be necessary
				}
				entity.IsNewVersion = true;

				if ( new Entity_VerificationProfileManager().VerificationProfile_Update( entity, parentUid, user.Id, ref messages ) )
				{
					if ( isQuickCreate )
					{
						status = "Created an initial Verification Profile. Please provide a meaningful name, and fill out the remainder of the profile";
						//test concept
						return false;
					}
					else
					{
						//if valid, status contains the cred id, category, and codeId
						status = "Successfully Saved Credential - Verification Profile";
						activityMgr.AddActivity( "Verification Profile", action, string.Format( "{0} added/updated credential connection task profile: {1}", user.FullName(), entity.ProfileName ), user.Id, 0, entity.Id );
					}
				}
				else
				{
					status += string.Join( ",", messages.ToArray() );
					return false;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".VerificationProfile_Save" );
				status = ex.Message;
				isValid = false;
			}

			return isValid;
		}

		public bool AuthenticationProfile_Delete( int conditionProfileId, int profileId, AppUser user, ref string status )
		{
			bool valid = true;

			Entity_VerificationProfileManager mgr = new Entity_VerificationProfileManager();
			try
			{
				//get first to validate (soon)
				//to do match to the conditionProfileId
				AuthenticationProfile profile = Entity_VerificationProfileManager.VerificationProfile_Get( profileId );

				valid = mgr.VerificationProfile_Delete( profileId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "Verification Profile", "Delete Task", string.Format( "{0} deleted Verification Profile {1} from Condition Profile  {2}", user.FullName(), profileId, conditionProfileId ), user.Id, 0, profileId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".AuthenticationProfile_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		#endregion

		#region Entity FrameworkItems
		public static List<EnumeratedItem> FrameworkItem_GetItems( List<int> recordIds )
		{
			List<EnumeratedItem> list = Entity_FrameworkItemManager.ItemsGet( recordIds );

			return list;
		}
		/// <summary>
		/// Add a framework item
		/// </summary>
		/// <param name="parentId"></param>
		/// <param name="parentEntityTypeId"></param>
		/// <param name="categoryId"></param>
		/// <param name="codeID"></param>
		/// <param name="user"></param>
		/// <param name="valid"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		[Obsolete]
		public EnumeratedItem FrameworkItem_Add( int parentId, int parentEntityTypeId, int categoryId, int codeID, AppUser user, ref bool valid, ref string status )
		{

			if ( user == null || user.Id == 0 )
			{
				valid = false;
				status = "Error - you must be authenticated in order to update data";
				return new EnumeratedItem();
			}
			EntitySummary parent = EntityManager.GetEntitySummary( parentId, parentEntityTypeId );

			return FrameworkItem_Add( parent.EntityUid, categoryId, codeID, user, ref valid, ref status );
		}

		/// <summary>
		/// Add a framework item to a parent
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="categoryId"></param>
		/// <param name="codeID"></param>
		/// <param name="user"></param>
		/// <param name="valid"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public EnumeratedItem FrameworkItem_Add( Guid parentUid, int categoryId, int codeID, AppUser user, ref bool valid, ref string status )
		{
			if ( !BaseFactory.IsGuidValid( parentUid ) || categoryId == 0 || codeID == 0 )
			{
				valid = false;
				status = "Error - invalid request - missing code identifiers";
				return new EnumeratedItem();
			}
			EntitySummary parent = EntityManager.GetEntitySummary( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				valid = false;
				status = "Error - invalid request - missing parent";
				return new EnumeratedItem();
			}
			Entity_FrameworkItemManager mgr = new Entity_FrameworkItemManager();
			int frameworkItemId = 0;
			EnumeratedItem item = new EnumeratedItem();
			try
			{
				frameworkItemId = mgr.ItemAdd( parent.Id, categoryId, codeID, user.Id, ref status );

				if ( frameworkItemId > 0 )
				{
					//get full item, as a codeItem to return
					item = Entity_FrameworkItemManager.ItemGet( frameworkItemId );
					
					ActivityServices.SiteActivityAdd( parent.EntityType + " FrameworkItem", "Add item", string.Format( "{0} added {1} FrameworkItem. ParentId: {2}, categoryId: {3}, codeId: {4}, summary: {5}", user.FullName(), parent.EntityType, parent.Id, categoryId, codeID, item.ItemSummary ), user.Id, 0, frameworkItemId );
					status = "";
				}
				else
				{
					valid = false;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".FrameworkItem_Add" );
				status = ex.Message;
				valid = false;
			}

			return item;
	}
		/// <summary>
		/// Temp
		/// delete a framework item
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="parentEntityTypeId"></param>
		/// <param name="frameworkItemId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		[Obsolete]
		public bool FrameworkItem_Delete( int parentId, int parentEntityTypeId, int frameworkItemId, AppUser user, ref string status )
		{
			EntitySummary parent = EntityManager.GetEntitySummary( parentId, CodesManager.ENTITY_TYPE_CREDENTIAL );

			return FrameworkItem_Delete( parent.EntityUid, frameworkItemId, user, ref status );
		}

		/// <summary>
		/// Delete a framework item
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="frameworkItemId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool FrameworkItem_Delete( Guid parentUid, int frameworkItemId, AppUser user, ref string status )
		{
			Entity_FrameworkItemManager mgr = new Entity_FrameworkItemManager();
			bool valid = true;

			try
			{
				EntitySummary parent = EntityManager.GetEntitySummary( parentUid );
				if ( parent == null || parent.Id == 0 )
				{
					valid = false;
					status = "Error - invalid request - missing parent";
					return false;
				}

				valid = mgr.ItemDelete( frameworkItemId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					ActivityServices.SiteActivityAdd( "FrameworkItem", "Delete item", string.Format( "{0} deleted FrameworkItem {1} from {2} {3}", user.FullName(), frameworkItemId, parent.EntityType, parent.Name ), user.Id, 0, frameworkItemId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".FrameworkItem_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}
		#endregion


		#region CredentialAlignmentObjectProfile Profile
		/// <summary>
		/// Get a Credential Alignment profile
		/// </summary>
		/// <param name="profileId"></param>
		/// <returns></returns>
		public static CredentialAlignmentObjectProfile CredentialAlignmentObject_Get( int profileId )
		{
			CredentialAlignmentObjectProfile profile = Entity_CompetencyManager.Get( profileId );

			return profile;
		}
		/// <summary>
		/// Add/Update Credential Alignment profile
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="action"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool CredentialAlignmentObject_Save( CredentialAlignmentObjectProfile entity, Guid parentUid, string action, AppUser user, ref string status )
		{
			bool isValid = true;
			List<String> messages = new List<string>();
			if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
			{
				messages.Add( "Error - missing an identifier for the Competency Profile" );
				return false;
			}
			if ( string.IsNullOrWhiteSpace(entity.AlignmentType ) )
			{
				messages.Add( "Error - missing an alignment type" );
				return false;
			}
			try
			{
				Entity e = EntityManager.GetEntity( parentUid );
				//remove this if properly passed from client
				//plus need to migrate to the use of EntityId
				entity.ParentId = e.Id;
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				entity.IsNewVersion = true;

				if ( new Entity_CompetencyManager().Save( entity, parentUid, user.Id, ref messages ) )
				{
					//if valid, status contains the cred id, category, and codeId
					status = "Successfully Saved CredentialAlignmentObjectProfile Profile";
					activityMgr.AddActivity( "CredentialAlignmentObjectProfile Profile", action, string.Format( "{0} added/updated CredentialAlignmentObjectProfile profile: {1}", user.FullName(), entity.Name ), user.Id, 0, entity.Id );
				}
				else
				{
					status += string.Join( ",", messages.ToArray() );
					return false;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".CredentialAlignmentObject_Save" );
				status = ex.Message;
				isValid = false;

				if ( ex.InnerException != null && ex.InnerException.Message != null )
				{
					status = ex.InnerException.Message;

					if ( ex.InnerException.InnerException != null && ex.InnerException.InnerException.Message != null )
						status = ex.InnerException.InnerException.Message;
				}
			}

			return isValid;
		}

		/// <summary>
		/// Delete Credential Alignment profile
		/// </summary>
		/// <param name="conditionProfileId"></param>
		/// <param name="profileId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool CredentialAlignmentObject_Delete( int conditionProfileId, int profileId, AppUser user, ref string status )
		{
			bool valid = true;

			Entity_CompetencyManager mgr = new Entity_CompetencyManager();
			try
			{
				//get first to validate (soon)
				//to do match to the conditionProfileId
				CredentialAlignmentObjectProfile profile = Entity_CompetencyManager.Get( profileId );

				valid = mgr.Delete( profileId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "CredentialAlignmentObjectProfile", "Delete", string.Format( "{0} deleted CredentialAlignmentObjectProfile Profile {1} from Profile  {2}", user.FullName(), profileId, conditionProfileId ), user.Id, 0, profileId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".CredentialAlignmentObject_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		#endregion
	}
}
