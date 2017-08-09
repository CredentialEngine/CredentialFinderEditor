using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Factories;
using Models;
using MN= Models.Node;
using Models.Common;
using Models.ProfileModels;
using Utilities;

namespace CTIServices
{
	public class ProfileServices
	{
		string thisClassName = "ProfileServices";
		ActivityServices activityMgr = new ActivityServices();
		public List<string> messages = new List<string>();

		#region Cost Profile

		#region retrieval
		/// <summary>
		/// Get all CostProfile for a parent
		/// </summary>
		/// <param name="parentId"></param>
		/// <returns></returns>
		//public static List<CostProfile> CostProfile_GetAll( Guid parentUid )
		//{
		//	List<CostProfile> list = CostProfileManager.CostProfile_GetAll( parentUid );
		//	return list;
		//}

		/// <summary>
		/// Get a CostProfile By integer Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static CostProfile CostProfile_Get( int profileId )
		{
			CostProfile profile = CostProfileManager.GetForEdit( profileId );
			return profile;
		}

		public static List<CostProfile> CostProfile_Search( string parentRowId, int pageNumber, int pageSize, ref int pTotalRows )
		{
			List<CostProfile> list = CostProfileManager.SearchByOwningOrg( parentRowId, pageNumber, pageSize, ref pTotalRows );
			return list;
		}


		//public static List<CostManifest> CostManifest_Search( int orgId, int pageNumber, int pageSize, ref int pTotalRows )
		//{
		//	List<CostManifest> list = CostManifestManager.Search( orgId, pageNumber, pageSize, ref pTotalRows );
		//	return list;
		//}
		#endregion

		#region Persistance
		public bool CostProfile_Save( CostProfile entity, Guid parentUid, string action, AppUser user, ref string status )
		{
			bool valid = true;
			status = "";

			List<string> messages = new List<string>();
			CostProfileManager mgr = new CostProfileManager();

			//TODO - watch the parent, if editing an asmt cost from the credential


			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				status = "Error - the parent entity was not found.";
				valid = false;
				return false;
			}
			//validate user has access. Parent can be multiple types, but aways is an entity
			//Credential credential = CredentialServices.GetBasicCredential( credentialUid );

			try
			{
				//is this still necessary? - handled in the Save
				if ( action == "Add" || action == "Initial" )
					entity.EntityId = parent.Id;
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				if ( mgr.Save( entity, parentUid, user.Id, ref messages ) == false )
				{
					valid = false;
					status = string.Join( "<br/>", messages.ToArray() );
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
				string message = ex.Message;
				LoggingHelper.LogError( ex, thisClassName + ".CostProfile_Save" );
				if ( ex.InnerException != null )
				{
					message = message + " Inner exception: " + ex.InnerException.Message;
					if ( ex.InnerException.InnerException != null )
					{
						message = message + " Inner-Inner exception: " + ex.InnerException.InnerException.Message;
					}
				}
				valid = false;
				status = message;
			}
			return valid;
		}

		public int CostProfile_Copy( Guid costProfileGuid, Guid parentUid, AppUser user, ref bool valid, ref string status )
		{
			valid = true;
			status = "";

			List<string> messages = new List<string>();
			CostProfileManager mgr = new CostProfileManager();
			CostProfile newCostProfile = new CostProfile();
			try
			{
				int newId = mgr.Copy( costProfileGuid, parentUid, user.Id, ref newCostProfile, ref messages );
				if ( newId == 0 )
				{
					valid = false;
					status = string.Join( "<br/>", messages.ToArray() );
				}
				else
				{
					status = "Successfully Copied Cost Profile ";
					//should the activity be for the parent
					activityMgr.AddActivity( "Cost Profile", "copy", string.Format( "{0} copied cost profile: {1}", user.FullName(), newCostProfile.ProfileName ), user.Id, 0, newCostProfile.Id );
				}
			}
			catch ( Exception ex )
			{
				string message = ex.Message;
				LoggingHelper.LogError( ex, thisClassName + ".CostProfile_Copy" );
				if ( ex.InnerException != null )
				{
					message = message + " Inner exception: " + ex.InnerException.Message;
					if ( ex.InnerException.InnerException != null )
					{
						message = message + " Inner-Inner exception: " + ex.InnerException.InnerException.Message;
					}
				}
				valid = false;
				status = message;
				return 0;
			}
			return newCostProfile.Id;
		}
		public bool CostProfile_Delete( int profileId, AppUser user, ref string status )
		{
			bool valid = true;
			CostProfileManager mgr = new CostProfileManager();
			try
			{
				//get profile and ensure user has access
				CostProfile profile = CostProfileManager.GetBasicProfile( profileId );
				if ( profile == null || profile.Id == 0 )
				{
					status = "Error - the requested profile was not found.";
					return false;
				}

				//if ( CanUserUpdateCredential( profile.ParentId, user, ref status ) )
				//{
				if ( mgr.Delete( profileId, ref status ) )
					{
						//if valid, log
						activityMgr.AddActivity( "Cost Profile", "Delete", string.Format( "{0} deleted Cost Profile {1} ({2}) from Credential: {3}", user.FullName(), profile.ProfileName, profileId, profile.ParentId ), user.Id, 0, profileId, profile.ParentId );
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
		//public static List<CostProfileItem> CostProfileItem_GetAll( int parentId )
		//{
		//	List<CostProfileItem> list = CostProfileItemManager.CostProfileItem_GetAll( parentId );
		//	return list;
		//}

		/// <summary>
		/// Get a CostProfileItem By integer Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static CostProfileItem CostProfileItem_GetForEdit( int profileId )
		{
			CostProfileItem profile = CostProfileItemManager.Get( profileId, true, true );
			return profile;
		}
	
		#endregion

		#region Persistance
		public bool CostProfileItem_Save( CostProfileItem entity, Guid parentUid, string action, AppUser user, ref string status )
		{
			bool valid = true;
			status = "";

			List<string> messages = new List<string>();
			CostProfileItemManager mgr = new CostProfileItemManager();
			//validate user has access. Parent can be multiple types, but aways is an entity
			CostProfile parent = CostProfileManager.GetBasicProfile( parentUid );
			//Credential credential = CredentialServices.GetBasicCredential( credentialUid );

			try
			{

				if ( mgr.Save( entity, parent.Id, user.Id, ref messages ) == false )
				{
					valid = false;
					status = string.Join( "<br/>", messages.ToArray() );
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
				CostProfileItem profile = CostProfileItemManager.Get( profileId, false, false );
				if ( profile == null || profile.Id == 0 )
				{
					status = "Error - the requested profile was not found.";
					return false;
				}

				//if ( CanUserUpdateCredential( profile.ParentId, user, ref status ) )
				//{
				if ( mgr.Delete( profileId, ref status ) )
				{
					//if valid, log
					activityMgr.AddActivity( "Cost Profile Item", "Delete", string.Format( "{0} deleted Cost Profile Item {1} ({2}) from Credential: {3}", user.FullName(), profile.ProfileName, profileId, profile.ParentId ), user.Id, 0, profileId, profile.ParentId );
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
		//public static List<Entity_Credential> Entity_Credential_GetAll( Guid parentUid )
		//{
		//	List<Entity_Credential> list = Entity_CredentialManager.GetAll( parentUid );
		//	return list;
		//}

		/// <summary>
		/// Get a Entity_Credential By integer Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Entity_Credential EntityCredential_Get( int profileId )
		{
			Entity_Credential profile = Entity_CredentialManager.Get( profileId );
			return profile;
		}

		#region Persistance
		public int EntityCredential_Save( Guid parentUid, int credentialId, AppUser user, bool allowMultipleSavedItems, ref bool valid, ref string status )
		{
			valid = true;
			int newId = 0;
			//bool allowMultiples = true;
			status = "";

			if ( user == null || user.Id == 0 )
			{
				status = "Error user must be logged in.";
				return 0;
			}

			List<string> messages = new List<string>();
			Entity_CredentialManager mgr = new Entity_CredentialManager();
			//validate user has access. Parent can be multiple types, but aways is an entity
			//??
			try
			{
				Entity parent = EntityManager.GetEntity( parentUid );
				if ( parent == null || parent.Id == 0 )
				{
					status = "Error - the parent entity was not found." ;
					valid = false;
					return 0;
				}
				if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_CREDENTIAL
					&& parent.EntityBaseId == credentialId )
				{
					status ="Error - a credential cannot be embedded within itself." ;
					valid = false;
					return 0;
				} else if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_CONDITION_PROFILE )
				{
					/* 
					 * ensure not adding the parent assessment to itself:
					 * - get the condition profile
					 * - get parent entity of the CP
					 * - check if of type asmt, and base id matches the assessmentId
					 * - if true reject
					 * - are there other levels of recursion to test?
					 */
					if ( Entity_ConditionProfileManager.IsParentBeingAddedAsChildToItself( parent.EntityBaseId, credentialId, CodesManager.ENTITY_TYPE_CREDENTIAL ) )
					{
						status = "Error - The Credential cannot be added to this condition profile as this same Credential is the parent of the condition profile.";
						valid = false;
						return 0;
					}
				}
				else if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_PROCESS_PROFILE )
				{
					/* 
					 * ensure not adding the parent credential to itself:
					 * - get the process profile
					 * - get parent entity
					 * - check if of type ???, and base id matches the Id
					 * - if true reject
					 * - are there other levels of recursion to test?
					 */
					if ( Entity_ProcessProfileManager.IsParentBeingAddedAsChildToItself( parent.EntityBaseId, credentialId, CodesManager.ENTITY_TYPE_CREDENTIAL ) )
					{
						status = "Error - The Credential cannot be added to this process profile as this same Credential is the parent of the process profile.";
						valid = false;
						return 0;
					}
				}

				//if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_PROCESS_PROFILE )
				//	allowMultiples = false;

				if ( mgr.Add( credentialId, parentUid, user.Id, allowMultipleSavedItems, ref newId, ref messages ) == false )
				{
					valid = false;
					status = string.Join( "<br/>", messages.ToArray() );
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

				Entity parent = EntityManager.GetEntity( parentUid );
				if ( parent == null || parent.Id == 0 )
				{
					status = "Error - the parent entity was not found." ;
					return false;
				}

				//get profile and ensure user has access
				Entity_Credential profile = Entity_CredentialManager.Get( parent.Id, credentialId );
				if ( profile == null || profile.Id == 0 )
				{
					status = "Error - the requested profile was not found.";
					return false;
				}
				//a little more difficult - plan to use the entity and bubble up to the top object
				//if ( CanUserUpdateCredential( profile.ParentId, user, ref status ) )
				//{
				if ( mgr.Delete( parent.Id, credentialId, ref status ) )
				{
					//if valid, log
					activityMgr.AddActivity( "Related Credential", "Remove", string.Format( "{0} removed Credential {1} ({2}) from Entity: {3} (4)", user.FullName(), profile.Credential.Name, credentialId, parent.EntityType, parent.EntityBaseId ), user.Id, 0, credentialId, parent.EntityBaseId );
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
			TextValueProfile profile = Entity_ReferenceManager.Get( profileId );
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
		/// Search for an address
		/// </summary>
		/// <param name="address1"></param>
		/// <param name="city"></param>
		/// <param name="postalCode"></param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSize"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
		public static List<Address> AddressProfile_Search( string address1, string city, string postalCode, int pageNumber, int pageSize, ref int pTotalRows )
		{
			List<Address> list = AddressProfileManager.QuickSearch( address1, city, postalCode, pageNumber, pageSize, ref pTotalRows );
			return list;
		}
		public static List<Address> AddressProfile_Search( string filter, int pageNumber, int pageSize, ref int pTotalRows )
		{
			List<Address> list = AddressProfileManager.QuickSearch( filter, pageNumber, pageSize, ref pTotalRows );
			return list;
		}
		/// <summary>
		/// Perform an autocomplete for a part of an address, based on typeId
		/// </summary>
		/// <param name="keyword"></param>
		/// <param name="typeId">1: address, 2: city, 3: postal code</param>
		/// <param name="maxTerms">Number of terms returned</param>
		/// <returns></returns>
		public static List<string> Autocomplete( string keyword, int typeId, int maxTerms = 25 )
		{
			List<string> items = AddressProfileManager.Autocomplete( keyword, typeId, maxTerms );

			return items;
		}
		/// <summary>
		/// Get a Address By integer Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Address AddressProfile_Get( int profileId )
		{
			Address profile = AddressProfileManager.Entity_Address_Get( profileId );
			return profile;
		
		}

		#endregion

		#region Persistance

		public bool AddressProfile_Save( Address entity, Guid parentUid, string action, string parentType, AppUser user, ref string status )
		{
			bool valid = true;
			status = "";

			List<string> messages = new List<string>();
			AddressProfileManager mgr = new AddressProfileManager();
			//validate user has access. Parent can be multiple types, but always is an entity

			try
			{
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				valid = mgr.Save( entity, parentUid, user.Id, ref messages );
			
				if ( valid )
				{
					status = "Successfully Saved Address";
					//should the activity be for the parent
					activityMgr.AddActivity( "Address Profile", action, string.Format( "{0} saved address profile: {1}", user.FullName(), entity.Name ), user.Id, 0, entity.Id );
				}
				else
				{
					status = string.Join( "<br/>", messages.ToArray() );
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
		public bool AddressProfile_Delete( Guid parentUid, int profileId, string parentType, AppUser user, ref string status )
		{
			bool valid = true;
			AddressProfileManager mgr = new AddressProfileManager();
			try
			{
				//get profile and ensure user has access
				Address profile = AddressProfileManager.Entity_Address_Get( profileId );
				if ( profile == null || profile.Id == 0 )
				{
					status = "Error - the requested address was not found.";
					return false;
				}
				else if ( parentUid != profile.ParentRowId )
				{
					status = "Error - the requested address does not match the current context.";
					return false;
				}

				//TODO - remove if after merge
				//if ( parentType == "Organization" )
				//{
				//	//Address profile = AddressProfileManager.GetOrganizationAddress( profileId );
				//	//if ( profile == null || profile.Id == 0 )
				//	//{
				//	//	status = "Error - the requested address was not found.";
				//	//	return false;
				//	//}

				//	if ( OrganizationServices.CanUserUpdateOrganization( user, parentUid ) )
				//	{
				//		//if ( mgr.Delete( profileId, ref status ) )
				//		if ( mgr.Entity_Address_Delete( profileId, ref status ) )
				//		{
				//			//if valid, log
				//			activityMgr.AddActivity( "Address Profile", "Delete", string.Format( "{0} deleted Address Profile {1} ({2}) from Organization: {3}", user.FullName(), profile.Name, profileId, profile.ParentId ), user.Id, 0, profileId );
				//			status = "";
				//		}
				//		else
				//		{
				//			status = "Error - delete failed: " + status;
				//			return false;
				//		}
				//	}
				//	else
				//	{
				//		//reject and log
				//		status = "Error - the requested profile was not found.";
				//		string msg = string.Format( "UNAUTHORIZED USER: {0} attempted to delete Address Profile {1} ({2}) from Organization: {3}", user.FullName(), profile.Name, profileId, profile.ParentId );
				//		//activityMgr.AddActivity( "Address Profile", "Delete", msg , user.Id, 0, profileId );
				//		activityMgr.AddActivity( new SiteActivity()
				//		{
				//			Activity = "Address Profile",
				//			Event = "Unauthorized Delete Attempt",
				//			Comment = msg,
				//			ActionByUserId = user.Id,
				//			TargetObjectId = profileId,
				//			ObjectRelatedId = profile.ParentId
				//		} );

				//		LoggingHelper.LogError( msg, true );
				//		return false;
				//	}
				//}
				//else
				//{
					//Address profile = AddressProfileManager.Entity_Address_Get( profileId );
					//if ( profile == null || profile.Id == 0 )
					//{
					//	status = "Error - the requested address was not found.";
					//	return false;
					//}

					//if ( OrganizationServices.CanUserUpdateOrganization( user, profile.ParentId ) )
					//{
						if ( mgr.Entity_Address_Delete( profileId, ref status ) )
						{
							//if valid, log
							activityMgr.AddActivity( "Address Profile", "Delete", string.Format( "{0} deleted Address Profile {1} ({2}) from ParentId: {3}", user.FullName(), profile.Name, profileId, profile.ParentId ), user.Id, 0, profileId );
							status = "";
						}
						else
						{
							status = "Error - delete failed: " + status;
							return false;
						}
					
				//}
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

		#region ContactPoint

		#region retrieval

		/// <summary>
		/// Search for an Contact Point????
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSize"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
		//public static List<ContactPoint> ContactPoint_Search( string filter, int pageNumber, int pageSize, ref int pTotalRows )
		//{
		//	List<ContactPoint> list = Entity_ContactPointManager.QuickSearch( filter, pageNumber, pageSize, ref pTotalRows );
		//	return list;
		//}
		
		/// <summary>
		/// Get a ContactPoint By integer Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static ContactPoint ContactPoint_Get( int profileId )
		{
			ContactPoint profile = Entity_ContactPointManager.Get( profileId );
			return profile;
		}

		#endregion

		#region Persistance

		public bool ContactPoint_Save( ContactPoint entity, Guid parentUid, string action, AppUser user, ref string status )
		{
			bool valid = true;
			status = "";

			List<string> messages = new List<string>();
			Entity_ContactPointManager mgr = new Entity_ContactPointManager();
			//validate user has access. Parent can be multiple types, but always is an entity

			try
			{
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				valid = mgr.Save( entity, parentUid, user.Id, ref messages );
				//}
				if ( valid )
				{
					status = "Successfully Saved ContactPoint";
					//should the activity be for the parent
					activityMgr.AddActivity( "ContactPoint Profile", action, string.Format( "{0} saved Contact Point: {1}", user.FullName(), entity.ProfileName ), user.Id, 0, entity.Id );
				}
				else
				{
					status = string.Join( "<br/>", messages.ToArray() );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".ContactPoint_Save" );
				valid = false;
				status = ex.Message;
			}
			return valid;
		}
		/// <summary>
		/// Delete an ContactPoint profile
		/// </summary>
		/// <param name="profileId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool ContactPoint_Delete( Guid parentUid, int profileId, AppUser user, ref string status )
		{
			bool valid = true;
			Entity_ContactPointManager mgr = new Entity_ContactPointManager();
			try
			{
				//get profile and ensure user has access
				ContactPoint profile = Entity_ContactPointManager.Get( profileId );
				if ( profile == null || profile.Id == 0 )
				{
					status = "Error - the requested contact point was not found.";
					return false;
				}
				else if ( parentUid != profile.ParentRowId )
				{
					status = "Error - the requested contact point does not match the current context.";
					return false;
				}


				if ( mgr.Delete( profileId, ref status ) )
				{
					//if valid, log
					activityMgr.AddActivity( "ContactPoint Profile", "Delete", string.Format( "{0} deleted ContactPoint Profile {1} ({2}) from ParentId: {3}", user.FullName(), profile.ProfileName, profileId, profile.ParentId ), user.Id, 0, profileId );
					status = "";
				}
				else
				{
					status = "Error - delete failed: " + status;
					return false;
				}
					
				
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".ContactPoint_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}
		#endregion

		#endregion

		#region Verification Profile
		public static VerificationServiceProfile VerificationServiceProfile_GetForEdit( int profileId )
		{
			VerificationServiceProfile profile = Entity_VerificationProfileManager.Get( profileId, true );

			return profile;
		}
		public bool VerificationServiceProfile_Save( VerificationServiceProfile entity, Guid parentUid, string action, AppUser user, ref string status, bool isQuickCreate = false )
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

				if ( new Entity_VerificationProfileManager().Save( entity, parentUid, user.Id, ref messages ) )
				{

					//if valid, status contains the cred id, category, and codeId
					status = "Successfully Saved Verification Profile";
					activityMgr.AddActivity( "Verification Profile", action, string.Format( "{0} added/updated credential connection task profile: {1}", user.FullName(), entity.ProfileName ), user.Id, 0, entity.Id );

				}
				else
				{
					status += string.Join( "<br/>", messages.ToArray() );
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

		public bool VerificationServiceProfile_Delete( int conditionProfileId, int profileId, AppUser user, ref string status )
		{
			bool valid = true;

			Entity_VerificationProfileManager mgr = new Entity_VerificationProfileManager();
			try
			{
				//get first to validate (soon)
				//to do match to the conditionProfileId
				VerificationServiceProfile profile = Entity_VerificationProfileManager.Get( profileId, false );

				valid = mgr.Delete( profileId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "Verification Profile", "Delete Task", string.Format( "{0} deleted Verification Profile {1} from Condition Profile  {2}", user.FullName(), profileId, conditionProfileId ), user.Id, 0, profileId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".VerificationServiceProfile_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		#endregion

		#region Process Profile
		public static ProcessProfile ProcessProfile_Get( int profileId )
		{
			ProcessProfile profile = Entity_ProcessProfileManager.Get( profileId );

			return profile;
		}
		public bool ProcessProfile_Save( ProcessProfile entity, Guid parentUid, string action, AppUser user, ref string status, bool isQuickCreate = false )
		{
			bool isValid = true;
			List<String> messages = new List<string>();
			if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
			{
				messages.Add( "Error - missing an identifier for the Process Profile" );
				return false;
			}

			try
			{
				Entity parent = EntityManager.GetEntity( parentUid );
				//remove this if properly passed from client
				//plus need to migrate to the use of EntityId
				entity.ParentId = parent.Id;
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				if ( new Entity_ProcessProfileManager().Save( entity, parentUid, user.Id, ref messages ) )
				{
					//if ( isQuickCreate )
					//{
					//	status = "Created an initial Process Profile. Please provide a meaningful name, and fill out the remainder of the profile";
					//	//test concept
					//	return false;
					//}
					//else
					//{
						//if valid, status contains the cred id, category, and codeId
						status = "Successfully Saved Process Profile";
						activityMgr.AddActivity( "Process Profile", action, string.Format( "{0} added/updated credential connection task profile: {1}", user.FullName(), entity.ProfileName ), user.Id, 0, entity.Id );
					//}
				}
				else
				{
					status += string.Join( "<br/>", messages.ToArray() );
					return false;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".ProcessProfile_Save" );
				status = ex.Message;
				isValid = false;
			}

			return isValid;
		}

		public static int DetermineProcessProfileTypeId( string processProfileType )
		{
			int typeId = 1;
			switch ( processProfileType )
			{
				//common processes
				case "AdministrationProcess":
					typeId = Entity_ProcessProfileManager.ADMIN_PROCESS_TYPE;
					break;

				case "DevelopmentProcess":
					typeId = Entity_ProcessProfileManager.DEV_PROCESS_TYPE;
					break;

				case "MaintenanceProcess":
					typeId = Entity_ProcessProfileManager.MTCE_PROCESS_TYPE;
					break;

				//org processes
				case "AppealProcess":
					typeId = Entity_ProcessProfileManager.APPEAL_PROCESS_TYPE;
					break;
				case "ComplaintProcess":
					typeId = Entity_ProcessProfileManager.COMPLAINT_PROCESS_TYPE;
					break;
				//case "CriteriaProcess":
				//	typeId = Entity_ProcessProfileManager.CRITERIA_PROCESS_TYPE;
				//	break;

				case "ReviewProcess":
					typeId = Entity_ProcessProfileManager.REVIEW_PROCESS_TYPE;
					break;

				case "RevocationProcess":
					typeId = Entity_ProcessProfileManager.REVOKE_PROCESS_TYPE;
					break;

				//case "ProcessProfile":
				//	typeId = Entity_ProcessProfileManager.DEFAULT_PROCESS_TYPE;
				//	break;

				//case "CredentialProcess":
				//	typeId = Entity_ProcessProfileManager.DEFAULT_PROCESS_TYPE;
				//	break;


				//
				default:
					typeId = Entity_ProcessProfileManager.DEFAULT_PROCESS_TYPE;
					LoggingHelper.LogError( "Received request for unhandled process profile type of [" + processProfileType + "] ", true, "Unexpected ProcesProfile type encountered" );
					//messages.Add( string.Format( "Error: Unexpected profile type of {0} was encountered.", entity.ProcessProfileType ) );
					break;
					
			}
			return typeId;
		}

		public bool ProcessProfile_Delete( int conditionProfileId, int profileId, AppUser user, ref string status )
		{
			bool valid = true;

			Entity_ProcessProfileManager mgr = new Entity_ProcessProfileManager();
			try
			{
				//get first to validate (soon)
				//to do match to the conditionProfileId
				ProcessProfile profile = Entity_ProcessProfileManager.Get( profileId );

				valid = mgr.Delete( profileId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "Process Profile", "Delete", string.Format( "{0} deleted Process Profile {1} from Credential {2}", user.FullName(), profileId, conditionProfileId ), user.Id, 0, profileId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".ProcessProfile_Delete" );
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

			Entity parent = EntityManager.GetEntity( parentEntityTypeId, parentId );
			//EntitySummary parent = EntityManager.GetEntitySummary( parentId, parentEntityTypeId );

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
			Entity parent = EntityManager.GetEntity( parentUid );
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
				Entity parent = EntityManager.GetEntity( parentUid );
				//EntitySummary parent = EntityManager.GetEntitySummary( parentUid );
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
					ActivityServices.SiteActivityAdd( "FrameworkItem", "Delete item", string.Format( "{0} deleted FrameworkItem {1} from {2} {3}", user.FullName(), frameworkItemId, parent.EntityType, parent.EntityBaseName ), user.Id, 0, frameworkItemId );
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

		#region Entity_Assessments and Entity_LearningOpportunities
		/// <summary>
		/// Add an assessment profile to a profile
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="assessmentId"></param>
		/// <param name="user"></param>
		/// <param name="valid"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public int Assessment_Add( Guid parentUid, int assessmentId, AppUser user, ref bool valid, ref string status, bool allowMultiples = true )
		{
			int id = 0;
			//bool allowMultiples = true;
			try
			{
				Entity parent = EntityManager.GetEntity( parentUid );
				if ( parent == null || parent.Id == 0 )
				{
					status = "Error - the parent entity was not found.";
					valid = false;
					return 0;
				}
				if (parent.EntityTypeId == CodesManager.ENTITY_TYPE_CONDITION_PROFILE)
				{
					/* 
					 * ensure not adding the parent assessment to itself:
					 * - get the condition profile
					 * - get parent entity of the CP
					 * - check if of type asmt, and base id matches the assessmentId
					 * - if true reject
					 * - are there other levels of recursion to test?
					 */
					if (Entity_ConditionProfileManager.IsParentBeingAddedAsChildToItself( parent.EntityBaseId, assessmentId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE ) )
					{
						status = "Error - The assessment cannot be added to this condition profile as this same assessment is the parent of the condition profile.";
						valid = false;
						return 0;
					}
				}
				else if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_PROCESS_PROFILE )
				{
					/* 
					 * ensure not adding the parent credential to itself:
					 * - get the process profile
					 * - get parent entity
					 * - check if of type ???, and base id matches the Id
					 * - if true reject
					 * - are there other levels of recursion to test?
					 */
					if ( Entity_ProcessProfileManager.IsParentBeingAddedAsChildToItself( parent.EntityBaseId, assessmentId, CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE ) )
					{
						status = "Error - The Assessment cannot be added to this process profile as this same Assessment is the parent of the process profile.";
						valid = false;
						return 0;
					}
				}
				//if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_PROCESS_PROFILE )
				//	allowMultiples = false;

				id = new Entity_AssessmentManager().Add( parentUid, assessmentId, user.Id, allowMultiples, ref messages );

				if ( id > 0 )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "Assessment", "Add Assessment", string.Format( "{0} added Assessment {1} to {3} EntityId: {2}", user.FullName(), assessmentId, parent.Id, parent.EntityType ), user.Id, 0, assessmentId );
					status = "";

					if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_CONDITION_PROFILE )
					{
						//CHECK IF NEED TO ADD LOPP TO THE PARENT CREDENTIAL
						//get credential entity
						Credential cred = ConditionProfileServices.GetProfileParentCredential( parent.EntityUid );
						if ( cred != null && cred.Id > 0 )
						{
							//add relationship
							//need to check for duplicates, and return without an error
							bool valid2 = true;
							string status2 = "";
							Assessment_Add( cred.RowId, assessmentId, user, ref valid2, ref status2, allowMultiples );
						}
					}
				}
				else
				{
					valid = false;
					status += string.Join( "<br/>", messages.ToArray() );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Assessment_Add" );
				status = ex.Message;
				valid = false;
			}

			return id;
		}

		public bool Assessment_Delete( Guid parentUid, int assessmentId, AppUser user, ref string status )
		{
			bool valid = true;

			Entity_AssessmentManager mgr = new Entity_AssessmentManager();
			try
			{
				Entity parent = EntityManager.GetEntity( parentUid );
				if ( parent == null || parent.Id == 0 )
				{
					messages.Add( "Error - the parent entity was not found." );
					return false;
				}
				//get profile and ensure user has access
				Entity_Assessment profile = Entity_AssessmentManager.Get( parent.Id, assessmentId );
				if ( profile == null || profile.Id == 0 )
				{
					status = "Error - the requested profile was not found.";
					return false;
				}

				valid = mgr.Delete( parentUid, assessmentId, ref status );

				//if valid, and no message (assuming related to the targer not being found)
				if ( valid && status.Length == 0 )
				{
					//activity
					activityMgr.AddActivity( "Assessment", "Remove Assessment", string.Format( "{0} removed Assessment {1} ({2}) from Entity: {3} (4)", user.FullName(), profile.Assessment.Name, assessmentId, parent.EntityType, parent.EntityBaseId ), user.Id, 0, assessmentId, parent.EntityBaseId );

					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Assessment_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}


		public int LearningOpportunity_Add( Guid parentUid, int recordId, 
				AppUser user, 
				ref bool valid, 
				ref string status, 
				bool allowMultiples = true,
				bool warnOnDuplicate = true)
		{
			int id = 0;
			//bool allowMultiples = true;
			Entity_LearningOpportunityManager mgr = new Entity_LearningOpportunityManager();
			try
			{
				Entity parent = EntityManager.GetEntity( parentUid );
				if ( parent == null || parent.Id == 0 )
				{
					status = "Error - the parent entity was not found." ;
					valid = false;
					return 0;
				}
				if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_CONDITION_PROFILE )
				{
					/* 
					 * ensure not adding the parent assessment to itself:
					 * - get the condition profile
					 * - get parent entity of the CP
					 * - check if of type asmt, and base id matches the assessmentId
					 * - if true reject
					 * - are there other levels of recursion to test?
					 */
					if ( Entity_ConditionProfileManager.IsParentBeingAddedAsChildToItself( parent.EntityBaseId, recordId, CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE ) )
					{
						status = "Error - The Learning Opportunity cannot be added to this condition profile as this same Learning Opportunity is the parent of the condition profile.";
						valid = false;
						return 0;
					}
				}
				else if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_PROCESS_PROFILE )
				{
					/* 
					 * ensure not adding the parent credential to itself:
					 * - get the process profile
					 * - get parent entity
					 * - check if of type ???, and base id matches the Id
					 * - if true reject
					 * - are there other levels of recursion to test?
					 */
					if ( Entity_ProcessProfileManager.IsParentBeingAddedAsChildToItself( parent.EntityBaseId, recordId, CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE ) )
					{
						status = "Error - The Learning opportunity cannot be added to this process profile as this same Learning opportunity is the parent of the process profile.";
						valid = false;
						return 0;
					}
				}

				//this should be handled in config of the profile
				//if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_PROCESS_PROFILE )
				//	allowMultiples = false;

				id = mgr.Add( parentUid, recordId, user.Id, allowMultiples, ref messages, warnOnDuplicate );

				if ( id > 0 )
				{
					//if valid, save activity
					activityMgr.AddActivity( "Learning Opportunity", "Add Learning Opportunity", string.Format( "{0} added Learning Opportunity {1} to {3} EntityId: {2}", user.FullName(), recordId, parent.Id, parent.EntityType ), user.Id, 0, recordId );

					status = "";
					if ( parent.EntityTypeId == CodesManager.ENTITY_TYPE_CONDITION_PROFILE )
					{
						//CHECK IF NEED TO ADD LOPP TO THE PARENT CREDENTIAL
						//get credential entity
						Credential cred = ConditionProfileServices.GetProfileParentCredential( parent.EntityUid );
						if ( cred != null && cred.Id > 0 )
						{
							//add relationship
							//need to check for duplicates, and return without an error
							bool valid2 = true;
							string status2 = "";
							LearningOpportunity_Add( cred.RowId, recordId, user, ref valid2, ref status2,
								allowMultiples,
								false);
						}
					}
				}
				else
				{
					valid = false;
					status += string.Join( "<br/>", messages.ToArray() );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Entity_LearningOpportunity_Add" );
				status = ex.Message;
				valid = false;
			}

			return id;
		}

		public bool LearningOpportunity_Delete( Guid parentUid, int recordId, AppUser user, ref string status )
		{
			bool valid = true;

			Entity_LearningOpportunityManager mgr = new Entity_LearningOpportunityManager();
			try
			{
				Entity parent = EntityManager.GetEntity( parentUid );
				if ( parent == null || parent.Id == 0 )
				{
					messages.Add( "Error - the parent entity was not found." );
					return false;
				}

				valid = mgr.Delete( parentUid, recordId, ref status );

				//if valid, and no message (assuming related to the targer not being found)
				if ( valid && status.Length == 0 )
				{
					//activity
					activityMgr.AddActivity( "Learning Opportunity", "Remove Learning Opportunity", string.Format( "{0} removed Learning Opportunity {1} from {3} EntityId: {2}", user.FullName(), recordId, parent.Id, parent.EntityType ), user.Id, 0, recordId );

					//activityMgr.AddActivity( "Assessment", "Remove Assessment", string.Format( "{0} removed Assessment {1} ({2}) from Entity: {3} (4)", user.FullName(), profile.Assessment.Name, assessmentId, parent.EntityType, parent.EntityBaseId ), user.Id, 0, assessmentId, parent.EntityBaseId );

					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Entity_LearningOpportunity_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		#endregion


		#region FinancialAlignmentObject
		/// <summary>
		/// Get a FinancialAlignmentObject profile
		/// </summary>
		/// <param name="profileId"></param>
		/// <returns></returns>
		public static FinancialAlignmentObject FinancialAlignmentProfile_Get( int profileId )
		{
			FinancialAlignmentObject profile = Entity_FinancialAlignmentProfileManager.Get( profileId );

			return profile;
		}
		/// <summary>
		/// Add/Update FinancialAlignmentObject
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="action"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool FinancialAlignmentProfile_Save( FinancialAlignmentObject entity, Guid parentUid, string action, AppUser user, ref string status )
		{
			bool isValid = true;
			List<String> messages = new List<string>();
			if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
			{
				messages.Add( "Error - missing an identifier for the FinancialAlignmentObject" );
				return false;
			}
			
			try
			{
				Entity e = EntityManager.GetEntity( parentUid );
				//remove this if properly passed from client
				//plus need to migrate to the use of EntityId
				entity.ParentId = e.Id;
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				if ( new Entity_FinancialAlignmentProfileManager().Save( entity, parentUid, user.Id, ref messages ) )
				{
					//if valid, status contains the cred id, category, and codeId
					status = "Successfully Saved Profile";
					activityMgr.AddActivity( "FinancialAlignmentObject Profile", action, string.Format( "{0} added/updated FinancialAlignmentObject profile: {1}", user.FullName(), entity.FrameworkName ), user.Id, 0, entity.Id );
				}
				else
				{
					status += string.Join( "<br/>", messages.ToArray() );
					return false;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".FinancialAlignmentProfile_Save" );
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
		/// Delete FinancialAlignmentObject
		/// </summary>
		/// <param name="conditionProfileId"></param>
		/// <param name="profileId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool FinancialAlignmentProfile_Delete( Guid parentUid, int profileId, AppUser user, ref string status )
		{
			bool valid = true;

			Entity_FinancialAlignmentProfileManager mgr = new Entity_FinancialAlignmentProfileManager();
			try
			{
				//get first to validate (soon)
				Entity parent = EntityManager.GetEntity( parentUid );

				//to do match to the conditionProfileId
				FinancialAlignmentObject profile = Entity_FinancialAlignmentProfileManager.Get( profileId );
				if ( profile.ParentId != parent.Id )
				{
					status = "Error - invalid parentId";
					return false;
				}
				valid = mgr.Delete( profileId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "FinancialAlignmentObject", "Delete", string.Format( "{0} deleted FinancialAlignmentObject ProfileId {1} from Parent Profile {2} (Id {3})", user.FullName(), profileId, parent.EntityType, parent.Id ), user.Id, 0, profileId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".FinancialAlignmentProfile_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}
		#endregion


		#region competencies OLD

		#region CredentialAlignmentObjectFrameworkProfile 
		/// <summary>
		/// Get a CredentialAlignmentObjectFramework profile
		/// </summary>
		/// <param name="profileId"></param>
		/// <returns></returns>
		public static CredentialAlignmentObjectFrameworkProfile CredentialAlignmentObjectFrameworkProfile_Get( int profileId )
		{
			CredentialAlignmentObjectFrameworkProfile profile = Entity_CompetencyFrameworkManager.Get( profileId );

			return profile;
		}
		/// <summary>
		/// Add/Update CredentialAlignmentObjectFrameworkProfile
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="action"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool CredentialAlignmentObjectFrameworkProfile_Save( CredentialAlignmentObjectFrameworkProfile entity, Guid parentUid, string action, AppUser user, ref string status )
		{
			bool isValid = true;
			List<String> messages = new List<string>();
			if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
			{
				messages.Add( "Error - missing an identifier for the Competency Framework Profile" );
				return false;
			}
			if ( string.IsNullOrWhiteSpace( entity.AlignmentType ) )
			{
				status = "Error - missing an alignment type" ;
				return false;
			}
			try
			{
				Entity e = EntityManager.GetEntity( parentUid );
				//remove this if properly passed from client
				//plus need to migrate to the use of EntityId
				entity.ParentId = e.Id;
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				if ( new Entity_CompetencyFrameworkManager().Save( entity, parentUid, user.Id, ref messages ) )
				{
					//if valid, status contains the cred id, category, and codeId
					status = "Successfully Saved Profile";
					activityMgr.AddActivity( "CredentialAlignmentObjectFrameworkProfile Profile", action, string.Format( "{0} added/updated CredentialAlignmentObjectFrameworkProfile profile: {1}", user.FullName(), entity.EducationalFrameworkName ), user.Id, 0, entity.Id );
				}
				else
				{
					status += string.Join( "<br/>", messages.ToArray() );
					return false;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".CredentialAlignmentObjectFrameworkProfile_Save" );
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
		/// Delete CredentialAlignmentObjectFrameworkProfile
		/// </summary>
		/// <param name="conditionProfileId"></param>
		/// <param name="profileId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool CredentialAlignmentObjectFrameworkProfile_Delete( Guid parentUid, int profileId, AppUser user, ref string status )
		{
			bool valid = true;

			Entity_CompetencyFrameworkManager mgr = new Entity_CompetencyFrameworkManager();
			try
			{
				//get first to validate (soon)
				Entity parent = EntityManager.GetEntity( parentUid );

				//to do match to the conditionProfileId
				CredentialAlignmentObjectFrameworkProfile profile = Entity_CompetencyFrameworkManager.Get( profileId );
				if ( profile.ParentId != parent.Id )
				{
					status = "Error - invalid parentId";
					return false;
				}
				valid = mgr.Delete( profileId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "CredentialAlignmentObjectFrameworkProfile", "Delete", string.Format( "{0} deleted CredentialAlignmentObjectFrameworkProfile ProfileId {1} from Parent Profile {2} (Id {3})", user.FullName(), profileId, parent.EntityType, parent.Id ), user.Id, 0, profileId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".CredentialAlignmentObjectFrameworkProfile_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}
		#endregion

		#region CredentialAlignmentObjectItemProfile Profile
		/// <summary>
		/// Get a Credential Alignment profile
		/// </summary>
		/// <param name="profileId"></param>
		/// <returns></returns>
		public static CredentialAlignmentObjectItemProfile CredentialAlignmentObjectItemProfile_Get( int profileId )
		{
			CredentialAlignmentObjectItemProfile profile = Entity_CompetencyFrameworkManager.Entity_Competency_Get( profileId );

			return profile;
		}
		/// <summary>
		/// Add/Update CredentialAlignmentObjectItemProfile
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="action"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool CredentialAlignmentObjectItemProfile_Save( CredentialAlignmentObjectItemProfile entity, Guid parentUid, string action, AppUser user, ref string status )
		{
			bool isValid = true;
			List<String> messages = new List<string>();
			if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
			{
				messages.Add( "Error - missing an identifier for the Competency Profile" );
				return false;
			}

			try
			{
				Entity e = EntityManager.GetEntity( parentUid );
				//remove this if properly passed from client
				//plus need to migrate to the use of EntityId
				//entity.ParentId = e.Id;
				entity.CreatedById = entity.LastUpdatedById = user.Id;

				if ( new Entity_CompetencyFrameworkManager().Entity_Competency_Save( entity, user.Id, ref messages ) )
				{
					//if valid, status contains the cred id, category, and codeId
					status = "Successfully Saved Profile";
					activityMgr.AddActivity( "CredentialAlignmentObjectItemProfile Profile", action, string.Format( "{0} added/updated CredentialAlignmentObjectItemProfile profile: {1}", user.FullName(), entity.Name ), user.Id, 0, entity.Id );
				}
				else
				{
					status += string.Join( "<br/>", messages.ToArray() );
					return false;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".CredentialAlignmentObjectItemProfile_Save" );
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
		/// Delete CredentialAlignmentObjectItemProfile
		/// </summary>
		/// <param name="conditionProfileId"></param>
		/// <param name="profileId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool CredentialAlignmentObjectItemProfile_Delete( int conditionProfileId, int profileId, AppUser user, ref string status )
		{
			bool valid = true;

			Entity_CompetencyFrameworkManager mgr = new Entity_CompetencyFrameworkManager();
			try
			{
				//get first to validate (soon)
				//to do match to the conditionProfileId
				CredentialAlignmentObjectItemProfile profile = Entity_CompetencyFrameworkManager.Entity_Competency_Get( profileId );

				valid = mgr.Entity_Competency_Delete( profileId, ref status );

				if ( valid )
				{
					//if valid, status contains the cred id, category, and codeId
					activityMgr.AddActivity( "CredentialAlignmentObjectItemProfile", "Delete", string.Format( "{0} deleted CredentialAlignmentObjectItemProfile Profile {1} from Profile  {2}", user.FullName(), profileId, conditionProfileId ), user.Id, 0, profileId );
					status = "";
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".CredentialAlignmentObjectItemProfile_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		#endregion
		#endregion
	}
}
