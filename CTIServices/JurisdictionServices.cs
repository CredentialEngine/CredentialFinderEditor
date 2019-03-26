using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mgr = Factories.Entity_JurisdictionProfileManager;
using Factories;
using Models;
using Models.Common;
using Utilities;

namespace CTIServices
{
	public class JurisdictionServices
	{
		Mgr mgr = new Mgr();


		#region Retrievals
		/// <summary>
		/// Get all jurisdiction profiles for a parent
		/// </summary>
		/// <param name="parentId"></param>
		/// <returns></returns>
		public List<JurisdictionProfile>  GetAll( Guid parentId )
		{

			List<JurisdictionProfile> list = Mgr.Jurisdiction_GetAll( parentId );
			return list;
		}

		/// <summary>
		/// Get a Jurisdiction Profile By integer Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public JurisdictionProfile Get( int id )
		{

			JurisdictionProfile profile = Mgr.Get( id );
			return profile;
		}

		/// <summary>
		/// Get a Jurisdiction Profile By Guid
		/// </summary>
		/// <param name="rowId"></param>
		/// <returns></returns>
		public JurisdictionProfile Get( Guid rowId )
		{

			JurisdictionProfile profile = Mgr.Get( rowId );
			return profile;
		}



		#endregion

		#region JurisdictionProfile Persistance
		public bool JurisdictionProfile_Add( JurisdictionProfile entity, 
				Guid parentUid, 
				int jprofilePurposeId, 
				string property, 
				AppUser user, 
				ref string statusMessage )
		{
			List<String> messages = new List<string>();
			//entity.Id is expected
			if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
			{
				messages.Add( "Error - missing an identifier for the JurisdictionProfile" );
				return false;
			}
			//remove this if properly passed from client
			entity.ParentEntityId = parentUid;
			entity.CreatedById = entity.LastUpdatedById = user.Id;
			entity.JProfilePurposeId = jprofilePurposeId;

			bool isValid = new Mgr().Add( entity, property, ref messages );

            Entity parent = EntityManager.GetEntity(parentUid);
            new ProfileServices().UpdateTopLevelEntityLastUpdateDate(parent.Id, string.Format("Entity Update triggered by {0} adding a Jurisdiction profile for : {1}, BaseId: {2}", user.FullName(), parent.EntityType, parent.EntityBaseId));

            statusMessage = string.Join( "<br/>", messages.ToArray() );
			return isValid;

		}
		public bool JurisdictionProfile_Update( JurisdictionProfile entity, Guid parentUid, string property, AppUser user, ref string statusMessage )
		{
			List<String> messages = new List<string>();
			//entity.Id is expected
			if ( entity == null || entity.Id == 0 || !BaseFactory.IsGuidValid( parentUid ) )
			{
				messages.Add( "Error - missing an identifier for the JurisdictionProfile" );
				return false;
			}

			//remove this if properly passed from client
			entity.ParentEntityId = parentUid;
			entity.LastUpdatedById = user.Id;

			bool isValid = mgr.Update( entity, property, ref messages );

            Entity parent = EntityManager.GetEntity(parentUid);
            new ProfileServices().UpdateTopLevelEntityLastUpdateDate(parent.Id, string.Format("Entity Update triggered by {0} updating a Jurisdiction profile for : {1}, BaseId: {2}", user.FullName(), parent.EntityType, parent.EntityBaseId));

            statusMessage = string.Join( "<br/>", messages.ToArray() );
			return isValid;

		}

		public bool JurisdictionProfile_Delete( int profileID, ref string status )
		{
			bool valid = true;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user == null || user.Id == 0 )
			{
				status = "You must be logged and authorized to perform this action.";
				return false;
			}
			try
			{
				JurisdictionProfile profile = Mgr.Get( profileID );

				valid = mgr.JurisdictionProfile_Delete( profileID, ref status );
				if ( valid )
				{
					//if valid, status contains the cred name and id
					new ActivityServices().AddEditorActivity( "JurisdictionProfile", "Delete", string.Format( "{0} deleted {1}", user.FullName(), status ), user.Id, 0, profileID );
					status = "";
                    Entity parent = EntityManager.GetEntity(profile.ParentId);
                    new ProfileServices().UpdateTopLevelEntityLastUpdateDate(parent.Id, string.Format("Entity Update triggered by {0} deleting a Jurisdiction profile for : {1}, BaseId: {2}", user.FullName(), parent.EntityType, parent.EntityBaseId));
                }
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "JurisdictionServices.JurisdictionProfile_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}

		#endregion

		#region GeoCoordinates 

		/// <summary>
		/// Get a GeoCoordinates By id
		/// </summary>
		/// <param name="id">Integer</param>
		/// <returns></returns>
		public GeoCoordinates GeoCoordiates_Get( int id )
		{

			GeoCoordinates profile = Mgr.GeoCoordinates_Get( id );
			return profile;
		}


		/// <summary>
		/// Get a list of GeoCoordinates from a list of IDs
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public List<GeoCoordinates> GeoCoordinates_GetList( List<int> ids )
		{
			List<GeoCoordinates> profiles = Mgr.GeoCoordinates_GetList( ids );
			return profiles;
		}
		//
		public bool GeoCoordinates_Add( GeoCoordinates entity, Guid parentId, AppUser user, ref string statusMessage )
		{
			if ( entity == null || !BaseFactory.IsGuidValid( parentId ) )
			{
				statusMessage = "Error - missing an identifier for the GeoCoordinates" ;
				return false;
			}
			//remove this if properly passed from client
			entity.ParentEntityId = parentId;
			entity.CreatedById = entity.LastUpdatedById = user.Id;

			int id = new Mgr().GeoCoordinates_Add( entity, ref statusMessage );
			if ( id > 0 )
			{
				entity.Id = id;

                JurisdictionProfile jp = Entity_JurisdictionProfileManager.Get(parentId);
                Entity parent = EntityManager.GetEntity(jp.RowId);
                new ProfileServices().UpdateTopLevelEntityLastUpdateDate(parent.Id, string.Format("Entity Update triggered by {0} adding Jursidiction/GeoCoordinates to {1} {2}", user.FullName(), parent.EntityType, parent.EntityBaseId));
                return true;
			} else 
				return false;

		}
		//public bool GeoCoordinates_Update( GeoCoordinates entity, Guid parentId, AppUser user, ref string statusMessage )
		//{
		//	List<String> messages = new List<string>();
		//	//entity.Id is expected
		//	if ( entity == null || entity.Id == 0 || !BaseFactory.IsGuidValid( parentId ) )
		//	{
		//		messages.Add( "Error - missing an identifier for the GeoCoordinates" );
		//		return false;
		//	}

		//	//remove this if properly passed from client
		//	entity.ParentEntityId = parentId;
		//	entity.LastUpdatedById = user.Id;

		//	bool isValid = mgr.GeoCoordinate_Update( entity, ref messages );

  //          new ProfileServices().UpdateTopLevelEntityLastUpdateDate(parent.Id, string.Format("Entity Update triggered by {0} adding a Learning Opportunity Part for : {1}, BaseId: {2}", user.FullName(), parent.EntityType, parent.EntityBaseId));
  //          return isValid;

		//}

		public bool GeoCoordinates_Delete( int profileID, ref bool valid, ref string status )
		{

			AppUser user = AccountServices.GetCurrentUser();
			if ( user == null || user.Id == 0 )
			{
				status = "You must be logged and authorized to perform this action.";
				return false;
			}
			try
			{
				GeoCoordinates profile = Mgr.GeoCoordinates_Get( profileID );

				valid = mgr.GeoCoordinate_Delete( profileID, ref status );
				if ( valid )
				{
					//if valid, status contains the cred name and id
					new ActivityServices().AddEditorActivity( "GeoCoordinates", "Delete", string.Format( "{0} deleted {1}", user.FullName(), status ), user.Id, 0, profileID );
					status = "";

                    JurisdictionProfile jp = Entity_JurisdictionProfileManager.Get(profile.ParentId);
                    Entity parent = EntityManager.GetEntity(jp.RowId);
                    new ProfileServices().UpdateTopLevelEntityLastUpdateDate(parent.Id, string.Format("Entity Update triggered by {0} deleting Jursidiction/GeoCoordinates from {1} {2}", user.FullName(), parent.EntityType, parent.EntityBaseId));
                }
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "JurisdictionServices.GeoCoordinates_Delete" );
				status = ex.Message;
				valid = false;
			}

			return valid;
		}




		#endregion
	}
}
