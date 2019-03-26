using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Models;
using Models.Common;
using ThisEntity = Models.Common.ConceptScheme;
using MN = Models.Node;
using MH = Models.Helpers.Cass;
using Factories;
using Manager =  Factories.ConceptSchemeManager;
using Utilities;



namespace CTIServices
{
	public class ConceptSchemeServices
	{
		public const string EntityType = "ConceptScheme";
		public int Add( ThisEntity framework, AppUser user, ref List<string> messages )
		{
			var newFrameworkID = new ConceptSchemeManager().Add( framework, user.Id, ref messages );
			if ( newFrameworkID > 0 )
			{
				new ActivityServices().AddActivity( new SiteActivity()
				{
					ActivityType = "ConceptScheme",
					Activity = "Editor",
					Event = "Add",
					Comment = string.Format( "{0} added Concept Scheme: {1}, Id: {2}", user.FullName(), framework.Name, newFrameworkID ),
					ActivityObjectId = newFrameworkID,
					ActionByUserId = user.Id,
					ActivityObjectParentEntityUid = framework.RowId
				} );
			}
			return newFrameworkID;
		}
		//

		public static void Approve( string ctid, AppUser user, ref List<string> messages )
		{
			//Validate
			var conceptScheme = Manager.GetByCtid( ctid );
			if ( conceptScheme == null || conceptScheme.Id == 0 )
			{
				messages.Add( "Concept Scheme Not Found for CTID: " + ctid );
				return;
			}
			if ( !ValidateFrameworkAction( conceptScheme, user, ref messages ) )
			{
				return;
			}
			//string payload = GetPayload( ctid );
			//if ( payload.ToLower().IndexOf( "conceptScheme not found" ) > -1 )
			//{
			//	//messages.Add( "Concept Scheme has not been saved. You must add competencies to a conceptScheme before doing an approval. " + ctid );
			//	//return;
			//}
			//Save
			conceptScheme.LastApproved = DateTime.Now;
			conceptScheme.LastApprovedById = user.Id;
			new Manager().Update( conceptScheme, ref messages, true );

			//TODO - replace this with a direct save. Just need to confirm other if still used by other processes
			//**** main issue appears to be that the interface doesn't update the dates after approve
			//		** the interface is now updated, so skip this step
			bool isPublished = false;
			string status = "";
			//TBD - do we need an Entity for ConceptScheme - doesn't appear so, except for approvals?
			//	- actually need to send an email!
			//if ( new ProfileServices().Entity_Approval_Save( "ConceptScheme", conceptScheme.RowId, user, ref isPublished, ref status, true ) == false )
			//{
			//	messages.Add( status );
			//}

			new ActivityServices().AddActivity( new SiteActivity()
			{
				ActivityType = EntityType,
				Activity = "Editor",
				Event = "Approval",
				Comment = string.Format( "{0} Approved conceptScheme: {1}, Id: {2}, Name: {3}", user.FullName(), EntityType, conceptScheme.Id, conceptScheme.Name ),
				ActivityObjectId = conceptScheme.Id,
				ActionByUserId = user.Id,
				ActivityObjectParentEntityUid = conceptScheme.RowId
			} );
			EntitySummary es = new EntitySummary()
			{
				Name = conceptScheme.Name,
				EntityType = EntityType,
				BaseId = conceptScheme.Id,
				OwningOrgId = conceptScheme.OrgId
			};

			if ( conceptScheme.OrgId > 0 && conceptScheme.OwningOrganization != null )
				es.OwningOrganization = conceptScheme.OwningOrganization.Name;

			new ProfileServices().SendApprovalEmail( es, user, ref status );

			string lastPublishDate = ActivityManager.GetLastPublishDate( EntityType.ToLower(), conceptScheme.Id );
			if ( lastPublishDate.Length > 5 )
				isPublished = true;
		}
		//Publish a Concept Scheme, if the user is allowed to do so and the framework is approved
		public static void Publish( string ctid, string frameworkExportJSON, AppUser user, ref List<string> messages )
		{
			//Validate
			var framework = Manager.GetByCtid( ctid );
			if ( !ValidateFrameworkAction( framework, user, ref messages ) )
			{
				return;
			}
			if ( !framework.IsEntityApproved() )
			{
				messages.Add( "You cannot publish the Concept Scheme until it has been approved." );
				return;
			}
			if ( framework.CTID != ctid )
			{
				messages.Add( "The Concept Scheme CTID doesn't match the version in the publisher." );
				return;
			}
			//validate payload
			if ( string.IsNullOrWhiteSpace( frameworkExportJSON ) )
			{
				messages.Add( "Error: a valid payload was not provided" );
				return;

			}
			else if ( frameworkExportJSON.IndexOf( "@context" ) == -1 
				|| (
					frameworkExportJSON.IndexOf( "@graph" ) == -1
					&& frameworkExportJSON.IndexOf( "\"@type\":\"ConceptScheme\"") == -1) //temp
				)
			{
				messages.Add( "Error: the payload is not formatted properly." );
				return;
			}

			//Do the publish using the JSON exported from CASS
			framework.Payload = frameworkExportJSON;
			List<SiteActivity> list = new List<SiteActivity>();
			bool valid = new RegistryServices().PublishConceptScheme( framework, user, ref messages, ref list );

			//Update
			if ( valid )
			{
				//doesn't work this way. Details come from activity log
				//framework.IsPublished = true;
				framework.LastPublished = DateTime.Now;
				framework.LastPublishedById = user.Id;

				//Save
				new Manager().Update( framework, ref messages, true );
			}
		}
		//
		public static void UnPublish( string ctid, AppUser user, ref List<string> messages )
		{
			//Validate
			var framework = Manager.GetByCtid( ctid );
			if ( !ValidateFrameworkAction( framework, user, ref messages ) )
			{
				return;
			}

			if ( framework.CTID != ctid )
			{
				messages.Add( "The Concept Scheme CTID doesn't match the version in the publisher." );
				return;
			}

			List<SiteActivity> list = new List<SiteActivity>();
			bool valid = new RegistryServices().UnPublishConceptScheme( framework, user, ref messages, ref list );

			//Update
			if ( valid )
			{

			}
		}
		//
		public static ThisEntity GetByCTID( string ctid )
		{
			return Manager.GetByCtid( ctid );
		}
		//
		public static ThisEntity GetByID( int id )
		{
			return Manager.Get( id );
		}
		//
		//Set the LastUpdated date of the framework to now
		public void MarkUpdated( string ctid, string name, AppUser user, ref List<string> messages )
		{
			//Validate
			var framework = Manager.GetByCtid( ctid );
			if ( !ValidateFrameworkAction( framework, user, ref messages ) )
			{
				return;
			}

			//Update
			framework.Name = string.IsNullOrWhiteSpace( name ) ? framework.Name : name;
			framework.LastUpdated = DateTime.Now;
			framework.LastUpdatedById = user.Id;

			//Save
			Update( framework, ref messages );
		}
		//
		public bool Update( ThisEntity framework, ref List<string> messages )
		{
			var user = AccountServices.GetCurrentUser();
			framework.LastUpdatedById = user.Id;
			//if there are no changes to the framework, the Update method will return false, but without any messages
			bool isValid = new Manager().Update( framework, ref messages );
			if ( isValid )
			{

				new ActivityServices().AddActivity( new SiteActivity()
				{
					ActivityType = "ConceptScheme",
					Activity = "Editor",
					Event = "Update",
					Comment = string.Format( "{0} updated Concept Scheme: {1}, Id: {2}", user.FullName(), framework.Name, framework.Id ),
					ActivityObjectId = framework.Id,
					ActionByUserId = user.Id,
					ActivityObjectParentEntityUid = framework.RowId
				} );

			}
			else if ( messages.Count > 0 )
			{
				isValid = false;
			}

			return isValid;
		}
		//
		public static bool ValidateFrameworkAction( ThisEntity framework, AppUser user, ref List<string> messages )
		{
			if ( framework == null || framework.Id == 0 )
			{
				messages.Add( "Concept Scheme Not Found" );
				return false;
			}

			if ( !CanUserUpdateFramework( user, framework.OrgId ) )
			{
				messages.Add( "You don't have access to manage that Concept Scheme." );
				return false;
			}

			return true;
		}

		/// <summary>
		/// WARNING this seems inconsistant with other uses of the cassExportUrl
		/// Turns out that there are two means to retrieve the payload from cass
		/// using: 
		///		"https://cass.credentialengine.org/api/ceasn/{0}" with the framework URI converted to an MD5 string
		///		The downside here, is having construct the Uri like 
		///		string cassURI = "https://credentialengineregistry.org/resources/" + item.CTID;
		///		string resourceUrl = string.Format( exportUrl, UtilityManager.GenerationMD5String( cassURI ) );
		///	OR
		///		"https://cass.credentialengine.org/api/ceasn/?id=https://credentialengineregistry.org/resources/ce-{0}"
		///	with the note below regarding the CaSS dev env not have ce-. 
		///	NOTE2: The latter can be a problem in the dev environment, as sometimes a sandbox subdomain is added, and sometimes not!
		/// </summary>
		/// <param name="CTID"></param>
		/// <returns></returns>
		public static string GetPayload( string CTID )
		{
			var getURL = ServiceHelper.GetAppKeyValue( "cassGetUrl" );
			var cassResourceUrlPrefix = ServiceHelper.GetAppKeyValue( "cassResourceUrlPrefix" );
			//dev env of CASS doesn't use the ce- so strip. The app key for prod will include the ce- in the appKey
			var resourceURI = string.Format( getURL, CTID.Replace( "ce-", "" ) );
			var getter = new HttpClient();
			var response = getter.GetAsync( resourceURI ).Result;
			var responseData = response.Content.ReadAsStringAsync().Result;

			responseData = GetCaSSPayload( CTID );

			return responseData;
		}
		/// <summary>
		/// Get CaSS payload using the MD5 string route
		/// may need to handle CTID or URI(@id).
		/// </summary>
		/// <param name="identifier"></param>
		/// <returns></returns>
		public static string GetCaSSPayload( string identifier )
		{
			List<string> messages = new List<string>();
			//change to conceptSchemeExportUrl
			string exportUrl = UtilityManager.GetAppKeyValue( "cassExportUrl", "" );
			var cassResourceUrlPrefix = ServiceHelper.GetAppKeyValue( "cassResourceUrlPrefix" );
			string cassURI = "";
			if ( ServiceHelper.IsValidCtid( identifier, ref messages ) )
			{
				//WARNING: in the dev environment, the CER type URI for CaSS frameworks don't always use sandbox domain!
				//also the dev env doesn't use ce-??? ==> is this still true?
				cassURI = "https://credentialengineregistry.org/resources/" + identifier;
			}
			else
				cassURI = identifier;
			string resourceUrl = string.Format( exportUrl, UtilityManager.GenerationMD5String( cassURI ) );

			//var getURL = ServiceHelper.GetAppKeyValue( "cassExportUrl" );
			//dev env of CASS doesn't use the ce- so strip. The app key for prod will include the ce-
			//var resourceURI = string.Format( getURL, CTID.Replace( "ce-", "" ) );
			var getter = new HttpClient();
			var response = getter.GetAsync( resourceUrl ).Result;
			var responseData = response.Content.ReadAsStringAsync().Result;

			return responseData;
		}
		public static bool CanUserUpdateFramework( AppUser user, int orgId )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( OrganizationManager.IsOrganizationMember( user.Id, orgId ) )
				return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;

			return false;
		}
	}
}
