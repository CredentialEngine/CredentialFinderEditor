using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Factories;
using Models;
using MN = Models.Node;
using Models.Common;
using Models.ProfileModels;
using Utilities;

namespace CTIServices
{
	public class CompetencyServices
	{

		string thisClassName = "CompetencyServices";
		ActivityServices activityMgr = new ActivityServices();
		public List<string> messages = new List<string>();


		#region competencies - CASS based

		/// <summary>
		/// Handle list of selected competencies:
		/// - check if eduction framework exists
		/// - if not, add it and return frameworkId
		/// </summary>
		/// <param name="input"></param>
		/// <param name="valid"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static MN.BaseProfile SaveCassCompetencyList( MN.CassInput input, ref bool isValid, ref string status )
		{
			//Get the user
			var user = AccountServices.GetUserFromSession();
			List<string> messages = new List<string>();
			//Determine which type of server profile to initialize
			//probably not necessary, as only one type of profile
			MN.BaseProfile clientProfile = new MN.BaseProfile();

			//var profile = Activator.CreateInstance( input.Context.Profile.Type );
			//var attribute = ( MN.Profile ) profile.GetType().GetCustomAttributes( typeof( MN.Profile ), true ).FirstOrDefault() ?? new MN.Profile();
			//var serverType = attribute.DBType;

			////Convert from client profile to server profile
			//var serverProfile = Activator.CreateInstance( serverType );
			//EditorServices.ConvertToServerProfile( serverProfile, clientProfile );

			EducationFrameworkManager mgr = new EducationFrameworkManager();
			int frameworkId = 0;
			//check if framework exists. 
			//if found, the frameworkId is returned, 
			//otherwise framework is added and frameworkId is returned
			if (mgr.HandleFrameworkRequest( input.Framework, user.Id, ref messages, ref frameworkId ) )
			{
				//will not create an Entity.EducationFramework with this competency centrix method

				//handle competencies
				int competencyId = 0;
				Entity_Competency entity = new Entity_Competency();
				Entity_CompetencyManager ecmMgr = new Entity_CompetencyManager();
				foreach ( var competency in input.Competencies )
				{
					if ( mgr.HandleCompetencyRequest( competency, frameworkId, user.Id, ref competencyId, ref messages  ) )
					{
						//add Entity.Competency
						entity = new Entity_Competency();
						entity.CompetencyId = competencyId;
						entity.CreatedById = user.Id;
						entity.FrameworkCompetency.Name = competency.Name;
						entity.FrameworkCompetency.Description = competency.Description;

						ecmMgr.Save( entity, input.Context.Profile.RowId, user.Id, ref messages );

					}
				}
			}

			if ( messages.Count > 0 )
				isValid = false;
			//Save the server profile
			//always an add
			//if ( input.Context.Profile.Id == 0 )
			//{
			//	return AddProfile( input.Context, serverProfile, user, ref valid, ref status, isNewVersion );
			//}
			////If existing, call the Update method
			//else
			//{
			//	return UpdateProfile( input.Context, serverProfile, user, ref valid, ref status );
			//}

			return clientProfile;
		}

		#region Get  
		/// <summary>
		/// Get a CredentialAlignmentObjectFramework profile
		/// </summary>
		/// <param name="profileId"></param>
		/// <returns></returns>
		public static CredentialAlignmentObjectFrameworkProfile EducationFramework_Get( int profileId )
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
		public bool EducationFramework_Save( CredentialAlignmentObjectFrameworkProfile entity, Guid parentUid, string action, AppUser user, ref string status )
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
				status = "Error - missing an alignment type";
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
		public bool EducationFramework_Delete( Guid parentUid, int profileId, AppUser user, ref string status )
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

		#region CassCompetency Profile
		/// <summary>
		/// Get a Credential Alignment profile
		/// </summary>
		/// <param name="profileId"></param>
		/// <returns></returns>
		public static CredentialAlignmentObjectItemProfile CassCompetency_Get( int profileId )
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
		public bool CassCompetency_Save( CredentialAlignmentObjectItemProfile entity, Guid parentUid, string action, AppUser user, ref string status )
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
		public bool CassCompetency_Delete( int conditionProfileId, int profileId, AppUser user, ref string status )
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
