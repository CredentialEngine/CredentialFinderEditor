using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MgrOld = Factories.RegionsManager;
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

			JurisdictionProfile profile = Mgr.Jurisdiction_Get( id );
			return profile;
		}

		/// <summary>
		/// Get a Jurisdiction Profile By Guid
		/// </summary>
		/// <param name="rowId"></param>
		/// <returns></returns>
		public JurisdictionProfile Get( Guid rowId )
		{

			JurisdictionProfile profile = Mgr.Jurisdiction_Get( rowId );
			return profile;
		}



		#endregion

		#region JurisdictionProfile Persistance
		public bool JurisdictionProfile_Add( JurisdictionProfile entity, Guid parentUid, int jprofilePurposeId, int userId, ref string statusMessage )
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
			entity.CreatedById = entity.LastUpdatedById = userId;
			entity.JProfilePurposeId = jprofilePurposeId;

			bool isValid = new Mgr().JurisdictionProfile_Add( entity, ref messages );

			return isValid;

		}
		public bool JurisdictionProfile_Update( JurisdictionProfile entity, Guid parentUid, int userId, ref string statusMessage )
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
			entity.LastUpdatedById = userId;

			bool isValid = mgr.JurisdictionProfile_Update( entity, ref messages );

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
				JurisdictionProfile profile = Mgr.Jurisdiction_Get( profileID );

				valid = mgr.JurisdictionProfile_Delete( profileID, ref status );
				if ( valid )
				{
					//if valid, status contains the cred name and id
					ActivityServices.SiteActivityAdd( "JurisdictionProfile", "Delete", string.Format( "{0} deleted {1}", user.FullName(), status ), user.Id, 0, profileID );
					status = "";
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
		public bool GeoCoordinates_Add( GeoCoordinates entity, Guid parentId, int userId, ref string statusMessage )
		{
			if ( entity == null || !BaseFactory.IsGuidValid( parentId ) )
			{
				statusMessage = "Error - missing an identifier for the GeoCoordinates" ;
				return false;
			}
			//remove this if properly passed from client
			entity.ParentEntityId = parentId;
			entity.CreatedById = entity.LastUpdatedById = userId;

			int id = new Mgr().GeoCoordinates_Add( entity, ref statusMessage );
			if ( id > 0 )
			{
				entity.Id = id;
				return true;
			} else 
				return false;

		}
		public bool GeoCoordinates_Update( GeoCoordinates entity, Guid parentId, int userId, ref string statusMessage )
		{
			List<String> messages = new List<string>();
			//entity.Id is expected
			if ( entity == null || entity.Id == 0 || !BaseFactory.IsGuidValid( parentId ) )
			{
				messages.Add( "Error - missing an identifier for the GeoCoordinates" );
				return false;
			}

			//remove this if properly passed from client
			entity.ParentEntityId = parentId;
			entity.LastUpdatedById = userId;

			bool isValid = mgr.GeoCoordinate_Update( entity, ref messages );

			return isValid;

		}

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
				//GeoCoordinates profile = Mgr.Jurisdiction_Get( profileID );

				valid = mgr.GeoCoordinate_Delete( profileID, ref status );
				if ( valid )
				{
					//if valid, status contains the cred name and id
					ActivityServices.SiteActivityAdd( "GeoCoordinates", "Delete", string.Format( "{0} deleted {1}", user.FullName(), status ), user.Id, 0, profileID );
					status = "";
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
