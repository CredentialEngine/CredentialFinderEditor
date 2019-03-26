using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Factories;
using Models;
using MN = Models.Node;
using Models.Common;
using Models.ProfileModels;
using Models.Helpers.Cass;
using Utilities;

using JsonObject = System.Collections.Generic.Dictionary<string, object>;

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
		public static List<CassCompetencyV2> SaveCassCompetencyList( MN.CassInput input, ref bool isValid, ref string status )
		{
			//Get the user
			var user = AccountServices.GetUserFromSession();
			List<string> messages = new List<string>();
            List<CassCompetencyV2> list = new List<CassCompetencyV2>();
            //Determine which type of server profile to initialize
            //probably not necessary, as only one type of profile
            //MN.BaseProfile clientProfile = new MN.BaseProfile();

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
                Entity parent = EntityManager.GetEntity( input.Context.Profile.RowId );
                int addedCount = 0;
				foreach ( var competency in input.Competencies )
				{
					if ( mgr.HandleCompetencyRequest( competency, frameworkId, user.Id, ref competencyId, ref messages  ) )
					{
						//add Entity.Competency
						entity = new Entity_Competency();
						entity.CompetencyId = competencyId;
						entity.CreatedById = user.Id;
                        //included for tracing:
						entity.FrameworkCompetency.Name = competency.Name;
						entity.FrameworkCompetency.Description = competency.Description;
						entity.Uri = competency.Uri;
						entity.CTID = competency.CTID;

                        if ( ecmMgr.Save( entity,parent,user.Id,ref messages ) )
                        {
                            addedCount++;
                            //get cass version for display on editor
                            list.Add(Entity_CompetencyManager.GetAsCassCompetency(entity.Id));
                        }

                    }
				}
                if ( addedCount > 0)
                {
                    new ActivityServices().AddEditorActivity( parent.EntityType, "Add Competencies", string.Format( "{0} added {1} Competencies to record: {2}", user.FullName(), addedCount, parent.EntityBaseId ), user.Id, 0, parent.EntityBaseId);

                    new ProfileServices().UpdateTopLevelEntityLastUpdateDate( parent.Id,string.Format( "Entity Update triggered by {0} adding competencies to : {1}, BaseId: {2}",user.FullName(),parent.EntityType,parent.EntityBaseId ) );
                }
			}

            if ( messages.Count > 0 )
            {
                isValid = false;
                status = string.Join("<br/>", messages.ToArray());
            }

			return list;
		}
		//

		public static bool RemoveCassCompetency( int competencyConnectorID, ref string status )
		{
            //Entity parent = new Entity();
            var user = AccountServices.GetUserFromSession();
            Entity parent = new Entity();
            bool isValid = new Entity_CompetencyManager().Delete( competencyConnectorID, user, ref parent, ref status );

            if (isValid && parent != null && parent.Id > 0)
            {
                new ProfileServices().UpdateTopLevelEntityLastUpdateDate( parent.Id,string.Format( "Entity Update triggered by {0} deleting a competency from : {1}, BaseId: {2}",user.FullName(),parent.EntityType,parent.EntityBaseId ) );
            }

            return isValid;
		}
		//
		public static List<CassCompetencyV2> CassCompetency_GetAll( Guid parentUid )
		{
			List<CassCompetencyV2> list = Entity_CompetencyManager.GetAllAsCassCompetencies( parentUid );

			return list;
		}



		#endregion
		#region MORE OBSOLETE
		/// <summary>
		/// Get a Credential Alignment profile
		/// </summary>
		/// <param name="profileId"></param>
		/// <returns></returns>
		//public static CredentialAlignmentObjectItemProfile CassCompetency_Get( int profileId )
		//{
		//	CredentialAlignmentObjectItemProfile profile = Entity_CompetencyFrameworkManager.Entity_CompetencyFrameworkItem_Get( profileId );

		//	return profile;
		//}


		/// <summary>
		/// Add/Update CredentialAlignmentObjectItemProfile
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="action"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		//      public bool CassCompetency_Save( CredentialAlignmentObjectItemProfile entity, Guid parentUid, string action, AppUser user, ref string status )
		//{
		//	bool isValid = true;
		//	List<String> messages = new List<string>();
		//	if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
		//	{
		//		messages.Add( "Error - missing an identifier for the Competency Profile" );
		//		return false;
		//	}

		//	try
		//	{
		//		Entity e = EntityManager.GetEntity( parentUid );
		//		//remove this if properly passed from client
		//		//plus need to migrate to the use of EntityId
		//		//entity.ParentId = e.Id;
		//		entity.CreatedById = entity.LastUpdatedById = user.Id;

		//		if ( new Entity_CompetencyFrameworkManager().Entity_CompetencyFrameworkItem_Save( entity, user.Id, ref messages ) )
		//		{
		//			//if valid, status contains the cred id, category, and codeId
		//			status = "Successfully Saved Profile";
		//			activityMgr.AddEditorActivity( "CredentialAlignmentObjectItemProfile Profile", action, string.Format( "{0} added/updated CredentialAlignmentObjectItemProfile profile: {1}", user.FullName(), entity.TargetNodeName ), user.Id, 0, entity.Id );
		//		}
		//		else
		//		{
		//			status += string.Join( "<br/>", messages.ToArray() );
		//			return false;
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".CredentialAlignmentObjectItemProfile_Save" );
		//		status = ex.Message;
		//		isValid = false;

		//		if ( ex.InnerException != null && ex.InnerException.Message != null )
		//		{
		//			status = ex.InnerException.Message;

		//			if ( ex.InnerException.InnerException != null && ex.InnerException.InnerException.Message != null )
		//				status = ex.InnerException.InnerException.Message;
		//		}
		//	}

		//	return isValid;
		//}

		/// <summary>
		/// Delete CredentialAlignmentObjectItemProfile
		/// </summary>
		/// <param name="conditionProfileId"></param>
		/// <param name="profileId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		//public bool CassCompetency_Delete( int conditionProfileId, int profileId, AppUser user, ref string status )
		//{
		//	bool valid = true;

		//	Entity_CompetencyFrameworkManager mgr = new Entity_CompetencyFrameworkManager();
		//	try
		//	{
		//		//get first to validate (soon)
		//		//to do match to the conditionProfileId
		//		CredentialAlignmentObjectItemProfile profile = Entity_CompetencyFrameworkManager.Entity_CompetencyFrameworkItem_Get( profileId );

		//		valid = mgr.Entity_CompetencyFrameworkItem_Delete( profileId, ref status );

		//		if ( valid )
		//		{
		//			//if valid, status contains the cred id, category, and codeId
		//			activityMgr.AddEditorActivity( "CredentialAlignmentObjectItemProfile", "Delete", string.Format( "{0} deleted CredentialAlignmentObjectItemProfile Profile {1} from Profile  {2}", user.FullName(), profileId, conditionProfileId ), user.Id, 0, profileId );
		//			status = "";
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".CredentialAlignmentObjectItemProfile_Delete" );
		//		status = ex.Message;
		//		valid = false;
		//	}

		//	return valid;
		//}
		#region Get  
		/// <summary>
		/// Get a CredentialAlignmentObjectFramework profile
		/// </summary>
		/// <param name="profileId"></param>
		/// <returns></returns>
		//public static CredentialAlignmentObjectFrameworkProfile EducationFramework_Get( int profileId )
		//{
		//	CredentialAlignmentObjectFrameworkProfile profile = Entity_CompetencyFrameworkManager.Get( profileId );

		//	return profile;
		//}
		/// <summary>
		/// Add/Update CredentialAlignmentObjectFrameworkProfile
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="action"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		//[Obsolete]
		//public bool EducationFramework_Save( CredentialAlignmentObjectFrameworkProfile entity, Guid parentUid, string action, AppUser user, ref string status )
		//{
		//	bool isValid = true;
		//	List<String> messages = new List<string>();
		//	if ( entity == null || !BaseFactory.IsGuidValid( parentUid ) )
		//	{
		//		messages.Add( "Error - missing an identifier for the Competency Framework Profile" );
		//		return false;
		//	}
		//	if ( string.IsNullOrWhiteSpace( entity.AlignmentType ) )
		//	{
		//		status = "Error - missing an alignment type";
		//		return false;
		//	}
		//	try
		//	{
		//		Entity e = EntityManager.GetEntity( parentUid );
		//		//remove this if properly passed from client
		//		//plus need to migrate to the use of EntityId
		//		entity.ParentId = e.Id;
		//		entity.CreatedById = entity.LastUpdatedById = user.Id;

		//		if ( new Entity_CompetencyFrameworkManager().Save( entity, parentUid, user.Id, ref messages ) )
		//		{
		//			//if valid, status contains the cred id, category, and codeId
		//			status = "Successfully Saved Profile";
		//			activityMgr.AddEditorActivity( "CredentialAlignmentObjectFrameworkProfile Profile", action, string.Format( "{0} added/updated CredentialAlignmentObjectFrameworkProfile profile: {1}", user.FullName(), entity.EducationalFrameworkName ), user.Id, 0, entity.Id );
		//		}
		//		else
		//		{
		//			status += string.Join( "<br/>", messages.ToArray() );
		//			return false;
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".CredentialAlignmentObjectFrameworkProfile_Save" );
		//		status = ex.Message;
		//		isValid = false;

		//		if ( ex.InnerException != null && ex.InnerException.Message != null )
		//		{
		//			status = ex.InnerException.Message;

		//			if ( ex.InnerException.InnerException != null && ex.InnerException.InnerException.Message != null )
		//				status = ex.InnerException.InnerException.Message;
		//		}
		//	}

		//	return isValid;
		//}

		/// <summary>
		/// Delete CredentialAlignmentObjectFrameworkProfile
		/// </summary>
		/// <param name="conditionProfileId"></param>
		/// <param name="profileId"></param>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		//public bool EducationFramework_Delete( Guid parentUid, int profileId, AppUser user, ref string status )
		//{
		//	bool valid = true;

		//	Entity_CompetencyFrameworkManager mgr = new Entity_CompetencyFrameworkManager();
		//	try
		//	{
		//		//get first to validate (soon)
		//		Entity parent = EntityManager.GetEntity( parentUid );

		//		//to do match to the conditionProfileId
		//		CredentialAlignmentObjectFrameworkProfile profile = Entity_CompetencyFrameworkManager.Get( profileId );
		//		if ( profile.ParentId != parent.Id )
		//		{
		//			status = "Error - invalid parentId";
		//			return false;
		//		}
		//		valid = mgr.Delete( profileId, ref status );

		//		if ( valid )
		//		{
		//			//if valid, status contains the cred id, category, and codeId
		//			activityMgr.AddEditorActivity( "CredentialAlignmentObjectFrameworkProfile", "Delete", string.Format( "{0} deleted CredentialAlignmentObjectFrameworkProfile ProfileId {1} from Parent Profile {2} (Id {3})", user.FullName(), profileId, parent.EntityType, parent.Id ), user.Id, 0, profileId );
		//			status = "";
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + ".CredentialAlignmentObjectFrameworkProfile_Delete" );
		//		status = ex.Message;
		//		valid = false;
		//	}

		//	return valid;
		//}
		#endregion
		#endregion
		#region CTDL-ASN EXPORT OLD COMPETENCIES FOR IMPORT TO CaSS - OBSOLETE

		//public static JsonObject ExportAllCTDLASNCompetencies()
		//{
		//          LoggingHelper.DoTrace(2, "CompetencyServices.ExportAllCTDLASNCompetencies");
		//	//Get data
		//	var rawData = Entity_CompetencyManager.ExportAllCTDLASNCompetencies();
		//	var result = new JsonObject();

		//	//Transform to CTDL-ASN
		//	var frameworks = new List<JsonObject>();
		//	var competencies = new List<JsonObject>();
		//	var errors = new List<string>();
		//	result.Add( "ceasn:CompetencyFramework", frameworks );
		//	result.Add( "ceasn:Competency", competencies );
		//	result.Add( "meta:Errors", errors );

		//	//Frameworks
		//	var frameworkIDs = rawData.Select( m => m.FrameworkCtid ).Distinct().ToList();
		//	foreach( var ctid in frameworkIDs )
		//	{
		//		try
		//		{
		//			var data = rawData.FirstOrDefault( m => m.FrameworkCtid == ctid );
		//			var holder = new JsonObject();
		//			AddPropertyIfNotEmpty( holder, "@id", "http://credentialengineregistry.org/resources/", data.FrameworkCtid );
		//			AddPropertyIfNotEmpty( holder, "@type", "", "ceasn:CompetencyFramework" );
		//			AddPropertyIfNotEmpty( holder, "ceasn:name", "", data.RecommendedFrameworkName, true );
		//			AddPropertyIfNotEmpty( holder, "ceterms:ctid", "", data.FrameworkCtid );
		//			AddPropertyIfNotEmpty( holder, "ceasn:derivedFrom", "", data.EducationalFrameworkUrl); //TODO: put framework URL here
		//			AddPropertyIfNotEmpty( holder, "ceasn:creator", "http://credentialengineregistry.org/resources/", data.OrganizationCtid );

		//			if( holder.Keys.Count() > 0 )
		//			{
		//				frameworks.Add( holder );
		//			}
		//		}
		//		catch( Exception ex )
		//		{
		//			errors.Add( "Error adding Competency Framework " + ctid + ": " + ex.Message );
		//		}
		//	}

		//	//Competencies
		//	var competencyIDs = rawData.Select( m => m.CompetencyCtid ).Distinct().ToList();
		//	foreach( var ctid in competencyIDs )
		//	{
		//		try
		//		{
		//			var data = rawData.FirstOrDefault( m => m.CompetencyCtid == ctid );
		//			var holder = new JsonObject();
		//			AddPropertyIfNotEmpty( holder, "@id", "http://credentialengineregistry.org/resources/", data.CompetencyCtid );
		//			AddPropertyIfNotEmpty( holder, "@type", "", "ceasn:Competency" );
		//			AddPropertyIfNotEmpty( holder, "ceterms:ctid", "", data.CompetencyCtid );
		//			AddPropertyIfNotEmpty( holder, "ceasn:derivedFrom", "", data.TargetUrl == "http://credentialengineregistry.org/resources/" + data.CompetencyCtid ? "" : data.TargetUrl );
		//			AddPropertyIfNotEmpty( holder, "ceasn:name", "", data.Competency, true );
		//			AddPropertyIfNotEmpty( holder, "ceasn:competencyText", "", data.Description, true );
		//			AddPropertyIfNotEmpty( holder, "ceasn:codedNotation", "", data.CodedNotation );
		//			AddPropertyIfNotEmpty( holder, "ceasn:isPartOf", "http://credentialengineregistry.org/resources/", data.FrameworkCtid, false, true );

		//			if ( holder.Keys.Count() > 0 )
		//			{
		//				competencies.Add( holder );
		//			}
		//		}
		//		catch( Exception ex )
		//		{
		//			errors.Add( "Error adding Competency " + ctid + ": " + ex.Message );
		//		}
		//	}

		//	//Return data
		//	return result;
		//}
		//public static JsonObject ExportAllApprovedCompetencies( bool requireHasApprovalForCompentencyExport )
		//{
		//    LoggingHelper.DoTrace( 2, "CompetencyServices.ExportAllApprovedCompetencies" );
		//    //Get data
		//    var rawData = Entity_CompetencyManager.ExportAllCompetenciesAsCTDLASN( requireHasApprovalForCompentencyExport );
		//    var result = new JsonObject();

		//    //Transform to CTDL-ASN
		//    var frameworks = new List<JsonObject>();
		//    var competencies = new List<JsonObject>();
		//    var errors = new List<string>();
		//    result.Add( "ceasn:CompetencyFramework", frameworks );
		//    result.Add( "ceasn:Competency", competencies );
		//    result.Add( "meta:Errors", errors );

		//    //Frameworks
		//    var frameworkIDs = rawData.Select( m => m.FrameworkCtid ).Distinct().ToList();
		//    foreach ( var ctid in frameworkIDs )
		//    {
		//        try
		//        {
		//            var data = rawData.FirstOrDefault( m => m.FrameworkCtid == ctid );
		//            var holder = new JsonObject();
		//            AddPropertyIfNotEmpty( holder, "@id", "http://credentialengineregistry.org/resources/", data.FrameworkCtid );
		//            AddPropertyIfNotEmpty( holder, "@type", "", "ceasn:CompetencyFramework" );
		//            AddPropertyIfNotEmpty( holder, "@owner", "", data.CompetencyPEMKey, false );
		//            AddPropertyIfNotEmpty( holder, "ceasn:name", "", data.RecommendedFrameworkName, true );
		//            AddPropertyIfNotEmpty( holder, "ceterms:ctid", "", data.FrameworkCtid );
		//            AddPropertyIfNotEmpty( holder, "ceasn:derivedFrom", "", data.EducationalFrameworkUrl ); //TODO: put framework URL here
		//            AddPropertyIfNotEmpty( holder, "ceasn:creator", "http://credentialengineregistry.org/resources/", data.OrganizationCtid );

		//            if ( holder.Keys.Count() > 0 )
		//            {
		//                frameworks.Add( holder );
		//            }
		//        }
		//        catch ( Exception ex )
		//        {
		//            errors.Add( "Error adding Competency Framework " + ctid + ": " + ex.Message );
		//        }
		//    }

		//    //Competencies
		//    var competencyIDs = rawData.Select( m => m.CompetencyCtid ).Distinct().ToList();
		//    foreach ( var ctid in competencyIDs )
		//    {
		//        try
		//        {
		//            var data = rawData.FirstOrDefault( m => m.CompetencyCtid == ctid );
		//            var holder = new JsonObject();
		//            AddPropertyIfNotEmpty( holder, "@id", "http://credentialengineregistry.org/resources/", data.CompetencyCtid );
		//            AddPropertyIfNotEmpty( holder, "@type", "", "ceasn:Competency" );
		//            AddPropertyIfNotEmpty( holder, "ceterms:ctid", "", data.CompetencyCtid );
		//            AddPropertyIfNotEmpty( holder, "ceasn:derivedFrom", "", data.TargetUrl == "http://credentialengineregistry.org/resources/" + data.CompetencyCtid ? "" : data.TargetUrl );
		//            AddPropertyIfNotEmpty( holder, "ceasn:name", "", data.Competency, true );
		//            AddPropertyIfNotEmpty( holder, "ceasn:competencyText", "", data.Description, true );
		//            AddPropertyIfNotEmpty( holder, "ceasn:codedNotation", "", data.CodedNotation );
		//            AddPropertyIfNotEmpty( holder, "ceasn:isPartOf", "http://credentialengineregistry.org/resources/", data.FrameworkCtid, false, true );

		//            if ( holder.Keys.Count() > 0 )
		//            {
		//                competencies.Add( holder );
		//            }
		//        }
		//        catch ( Exception ex )
		//        {
		//            errors.Add( "Error adding Competency " + ctid + ": " + ex.Message );
		//        }
		//    }
		//    string jsoninput = JsonConvert.SerializeObject( result, GetJsonSettings() );
		//    Utilities.LoggingHelper.WriteLogFile( 5, "competencies_export.json", jsoninput, "", false );


		//    //Return data
		//    return result;
		//}
		//public static JsonObject ExportAllConditonProfileCompetenciesAsCTDLASN( bool requireHasApprovalForCompentencyExport )
		//{
		//    LoggingHelper.DoTrace( 2, "CompetencyServices.ExportAllApprovedCompetencies" );
		//    //Get data
		//    var rawData = Entity_CompetencyManager.ExportAllConditonProfileCompetenciesAsCTDLASN( requireHasApprovalForCompentencyExport );
		//    var result = new JsonObject();

		//    //Transform to CTDL-ASN
		//    var frameworks = new List<JsonObject>();
		//    var competencies = new List<JsonObject>();
		//    var errors = new List<string>();
		//    result.Add( "ceasn:CompetencyFramework", frameworks );
		//    result.Add( "ceasn:Competency", competencies );
		//    result.Add( "meta:Errors", errors );

		//    //Frameworks
		//    var frameworkIDs = rawData.Select( m => m.FrameworkCtid ).Distinct().ToList();
		//    foreach ( var ctid in frameworkIDs )
		//    {
		//        try
		//        {
		//            var data = rawData.FirstOrDefault( m => m.FrameworkCtid == ctid );
		//            var holder = new JsonObject();
		//            AddPropertyIfNotEmpty( holder, "@id", "http://credentialengineregistry.org/resources/", data.FrameworkCtid );
		//            AddPropertyIfNotEmpty( holder, "@type", "", "ceasn:CompetencyFramework" );
		//            AddPropertyIfNotEmpty( holder, "@owner", "", data.CompetencyPEMKey, false );
		//            AddPropertyIfNotEmpty( holder, "ceasn:name", "", data.RecommendedFrameworkName, true );
		//            AddPropertyIfNotEmpty( holder, "ceterms:ctid", "", data.FrameworkCtid );
		//            AddPropertyIfNotEmpty( holder, "ceasn:derivedFrom", "", data.EducationalFrameworkUrl ); //TODO: put framework URL here
		//            AddPropertyIfNotEmpty( holder, "ceasn:creator", "http://credentialengineregistry.org/resources/", data.OrganizationCtid );

		//            if ( holder.Keys.Count() > 0 )
		//            {
		//                frameworks.Add( holder );
		//            }
		//        }
		//        catch ( Exception ex )
		//        {
		//            errors.Add( "Error adding Competency Framework " + ctid + ": " + ex.Message );
		//        }
		//    }

		//    //Competencies
		//    var competencyIDs = rawData.Select( m => m.CompetencyCtid ).Distinct().ToList();
		//    foreach ( var ctid in competencyIDs )
		//    {
		//        try
		//        {
		//            var data = rawData.FirstOrDefault( m => m.CompetencyCtid == ctid );
		//            var holder = new JsonObject();
		//            AddPropertyIfNotEmpty( holder, "@id", "http://credentialengineregistry.org/resources/", data.CompetencyCtid );
		//            AddPropertyIfNotEmpty( holder, "@type", "", "ceasn:Competency" );
		//            AddPropertyIfNotEmpty( holder, "ceterms:ctid", "", data.CompetencyCtid );
		//            AddPropertyIfNotEmpty( holder, "ceasn:derivedFrom", "", data.TargetUrl == "http://credentialengineregistry.org/resources/" + data.CompetencyCtid ? "" : data.TargetUrl );
		//            AddPropertyIfNotEmpty( holder, "ceasn:name", "", data.Competency, true );
		//            AddPropertyIfNotEmpty( holder, "ceasn:competencyText", "", data.Description, true );
		//            AddPropertyIfNotEmpty( holder, "ceasn:codedNotation", "", data.CodedNotation );
		//            AddPropertyIfNotEmpty( holder, "ceasn:isPartOf", "http://credentialengineregistry.org/resources/", data.FrameworkCtid, false, true );

		//            if ( holder.Keys.Count() > 0 )
		//            {
		//                competencies.Add( holder );
		//            }
		//        }
		//        catch ( Exception ex )
		//        {
		//            errors.Add( "Error adding Competency " + ctid + ": " + ex.Message );
		//        }
		//    }
		//    string jsoninput = JsonConvert.SerializeObject( result, GetJsonSettings() );
		//    Utilities.LoggingHelper.WriteLogFile( 5, "competencies_export.json", jsoninput, "", false );


		//    //Return data
		//    return result;
		//}
		private static void AddPropertyIfNotEmpty( JsonObject holder, string propertyName, string prefix, string value, bool isLanguageString = false, bool isList = false )
		{
			if ( !string.IsNullOrWhiteSpace( value ) )
			{
				if ( isLanguageString )
				{
					holder.Add( propertyName, new JsonObject() { { "en-US", prefix + value } } );
				}
				else if ( isList )
				{
					holder.Add( propertyName, new List<string>() { prefix + value } );
				}
				else
				{
					holder.Add( propertyName, prefix + value );
				}
			}
		}
        //
        public static JsonSerializerSettings GetJsonSettings()
        {
            var settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            return settings;
        }
        #endregion
    }
}
