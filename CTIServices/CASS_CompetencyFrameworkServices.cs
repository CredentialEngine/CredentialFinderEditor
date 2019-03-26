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
using ThisEntity = Models.Common.CASS_CompetencyFramework;
using MN = Models.Node;
using MH = Models.Helpers.Cass;
using Factories;
using Manager = Factories.CASS_CompetencyFrameworkManager;
using Utilities;

namespace CTIServices
{
	//2018 version - other CASS services should be checked for removability
	public class CASS_CompetencyFrameworkServices
	{

		public int AddFramework( ThisEntity framework, AppUser user, ref List<string> messages )
		{
			var newFrameworkID = new Manager().Add( framework, user.Id, ref messages );
			if ( newFrameworkID > 0 )
			{
				//var user = AccountServices.GetAccount( userID );
				new ActivityServices().AddActivity( new SiteActivity()
				{
					ActivityType = "CASS_CompetencyFramework",
					Activity = "Editor",
					Event = "Add",
					Comment = string.Format( "{0} added competency framework: {1}, Id: {2}", user.FullName(), framework.FrameworkName, newFrameworkID),
					ActivityObjectId = newFrameworkID,
					ActionByUserId = user.Id,
					ActivityObjectParentEntityUid = framework.RowId
				} );
			}
			return newFrameworkID;
		}
		//

		
		public bool DeleteFramework( string frameworkCTID, ref string message ) {
			return new Manager().Delete( frameworkCTID, ref message );
		}
		//

		public static ThisEntity GetFrameworkByCTID( string frameworkCTID )
		{
			return Manager.GetByCtid( frameworkCTID );
		}
		//

		public static ThisEntity GetFrameworkByID( int frameworkID )
		{
			return Manager.Get( frameworkID );
		}
		//


		//Set the LastUpdated date of the framework to now
		public void MarkFrameworkUpdated( string frameworkCTID, string frameworkName, AppUser user, ref List<string> messages )
		{
			//Validate
			var framework = Manager.GetByCtid( frameworkCTID );
			if( !ValidateFrameworkAction( framework, user, ref messages ) )
			{
				return;
			}

            //Update
            framework.FrameworkName = string.IsNullOrWhiteSpace(frameworkName) ? framework.FrameworkName : frameworkName;
            framework.LastUpdated = DateTime.Now;
			framework.LastUpdatedById = user.Id;
			
			//Save
			UpdateFramework( framework, ref messages );
		}
		//
		public bool UpdateFramework( ThisEntity framework, ref List<string> messages )
		{
			var user = AccountServices.GetCurrentUser();
			framework.LastUpdatedById = user.Id;
			//if there are no changes to the framework, the Update method will return false, but without any messages
			bool isValid = new Manager().Update( framework, ref messages );
			if ( isValid )
			{

				new ActivityServices().AddActivity( new SiteActivity()
				{
					ActivityType = "CASS_CompetencyFramework",
					Activity = "Editor",
					Event = "Update",
					Comment = string.Format( "{0} updated competency framework: {1}, Id: {2}", user.FullName(), framework.FrameworkName, framework.Id ),
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
		//Approve a Framework, if the user is allowed to do so
		public static void ApproveFramework( string frameworkCTID, AppUser user, ref List<string> messages )
		{
			//Validate
			var framework = Manager.GetByCtid( frameworkCTID );
            if (framework == null || framework.Id == 0)
            {
                messages.Add( "Framework Not Found for CTID: " + frameworkCTID);
                return;
            }
			if ( !ValidateFrameworkAction( framework, user, ref messages ) )
			{
				return;
			}
            string payload = GetPayload( frameworkCTID );
            if ( payload.ToLower().IndexOf( "framework not found") > -1)
            {
                //messages.Add( "Framework has not been saved. You must add competencies to a framework before doing an approval. " + frameworkCTID );
                //return;
            }
            //Save
            framework.LastApproved = DateTime.Now;
            framework.LastApprovedById = user.Id;
            new Manager().Update( framework, ref messages, true );

            //TODO - replace this with a direct save. Just need to confirm other if still used by other processes
            //**** main issue appears to be that the interface doesn't update the dates after approve
            bool isPublished = false;
            string status = "";
            if ( new ProfileServices().Entity_Approval_Save( "CASS_CompetencyFramework", framework.RowId, user, ref isPublished, ref status, true ) == false)
            {
				messages.Add( status );
            }
		}
		//

		//Publish a Framework, if the user is allowed to do so and the framework is approved
		public static void PublishFramework( string frameworkCTID, string frameworkExportJSON, AppUser user, ref List<string> messages )
		{
			//Validate
			var framework = Manager.GetByCtid( frameworkCTID );
			if ( !ValidateFrameworkAction( framework, user, ref messages ) )
			{
				return;
			}
			if ( !framework.IsEntityApproved() )
			{
				messages.Add( "You cannot publish the framework until it has been approved." );
				return;
			}
            if (framework.CTID != frameworkCTID)
            {
                messages.Add( "The framework CTID doesn't match the version in the publisher." );
                return;
            }
			//validate payload
			if ( string.IsNullOrWhiteSpace( frameworkExportJSON ) )
			{
				messages.Add( "Error: a valid payload was not provided" );
				return;

			}
			else if ( frameworkExportJSON.IndexOf( "@context" ) == -1 || frameworkExportJSON.IndexOf( "@graph" ) == -1 )
			{
				messages.Add( "Error: the payload is not formatted properly." );
				return;
			}

			//Do the publish using the JSON exported from CASS
			framework.Payload = frameworkExportJSON;
            List<SiteActivity> list = new List<SiteActivity>();
            bool valid = new RegistryServices().PublishCompetencyFramework( framework, user, ref messages, ref list );

            //Update
            if (valid)
            {
                //doesn't work this way. Details come from activity log
                //framework.IsPublished = true;
                framework.LastPublished = DateTime.Now;
                framework.LastPublishedById = user.Id;

                //Save
                new Manager().Update( framework, ref messages, true );
            }
		}

        public static void PublishAllApprovedFrameworks( AppUser user, ref List<string> messages )
        {
            //Validate - this 
            var list = Manager.GetAllApproved();
            foreach (var framework in list )
            {

                //Do the publish using the JSON exported from CASS
                framework.Payload = GetPayload(framework.CTID);
                List<SiteActivity> history = new List<SiteActivity>();
                bool valid = new RegistryServices().PublishCompetencyFramework( framework, user, ref messages, ref history );

                //Update
                if ( valid )
                {
					//doesn't work this way. Details come from activity log
					//framework.IsPublished = true;
					framework.LastApproved = DateTime.Now;
					framework.LastApprovedById = user.Id;
					framework.LastPublished = DateTime.Now;
                    framework.LastPublishedById = user.Id;

                    //Save
                    new Manager().Update( framework, ref messages, true );
                } else
                {
                    LoggingHelper.DoTrace( 2, string.Format( "CASS_CompetencyFrameworkServices.PublishAllApprovedFrameworks() Error publishing: {0}", framework.CTID ) + String.Join( "\r\n", messages ));
                }
            }

        }
		/// <summary>
		/// may not need this, although there may be some that have a null approval date.
		/// at this time there may be too many to do this way?
		/// </summary>
		/// <param name="user"></param>
		/// <param name="messages"></param>
		public static void RepublishAllPublishedFrameworks( AppUser user, ref List<string> messages )
		{
			//Validate
			var list = Manager.GetAllPublished();
			foreach ( var framework in list )
			{

				//Do the publish using the JSON exported from CASS
				framework.Payload = GetPayload( framework.CTID );
				List<SiteActivity> history = new List<SiteActivity>();
				bool valid = new RegistryServices().PublishCompetencyFramework( framework, user, ref messages, ref history );

				//Update
				if ( valid )
				{
					//doesn't work this way. Details come from activity log
					//framework.IsPublished = true;
					framework.LastApproved = DateTime.Now;
					framework.LastApprovedById = user.Id;
					framework.LastPublished = DateTime.Now;
					framework.LastPublishedById = user.Id;

					//Save
					new Manager().Update( framework, ref messages, true );
				}
				else
				{
					LoggingHelper.DoTrace( 2, string.Format( "CASS_CompetencyFrameworkServices.PublishAllApprovedFrameworks() Error publishing: {0}", framework.CTID ) + String.Join( "\r\n", messages ) );
				}
			}

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
            //string getURL = "https://cass.credentialengine.org/api/ceasn/?id=https://credentialengineregistry.org/resources/ce-{0}";
            var getURL = ServiceHelper.GetAppKeyValue( "cassGetUrl" );
			var cassResourceUrlPrefix = ServiceHelper.GetAppKeyValue( "cassResourceUrlPrefix" );
			//dev env of CASS doesn't use the ce- so strip. The app key for prod will include the ce- in the appKey
			var resourceURI = string.Format( getURL, CTID.Replace( "ce-", "" ));
            var getter = new HttpClient();
            var response = getter.GetAsync( resourceURI ).Result;
            var responseData = response.Content.ReadAsStringAsync().Result;

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
			string exportUrl = UtilityManager.GetAppKeyValue( "cassExportUrl", "" );
			var cassResourceUrlPrefix = ServiceHelper.GetAppKeyValue( "cassResourceUrlPrefix" );
			string cassURI = "";
			if (ServiceHelper.IsValidCtid(identifier, ref messages))
			{
				//WARNING: in the dev environment, the CER type URI for CaSS frameworks don't always use sandbox domain!
				//also the dev env doesn't use ce-??? ==> s this still true?
				cassURI = "https://credentialengineregistry.org/resources/" + identifier;
			} else
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

		/// <summary>
		/// may need to handle CTID or @id. OR GetCaSSPayload can handle this!
		/// NOTE needs to be transposed to MN.CassInput for saving. May want to merge classes!
		/// </summary>
		/// <param name="CTID"></param>
		/// <param name="status"></param>
		/// <returns>CassInput - this is used as input to the target save method.</returns>
		public MN.CassInput ImportComptencyFramework( string CTID, ref  List<string>messages)
		{
			CompetencyFramework output = new CompetencyFramework();
			Competency comp = new Competency();
			MN.CassInput cassOutput = new MN.CassInput();
			MH.CassCompetency cc = new MH.CassCompetency();

			string payload = GetCaSSPayload( CTID );
			if (string.IsNullOrWhiteSpace( payload ) )
			{
                messages.Add( "Error: a Competency framework was not found using the provided CTID: " + CTID);
				return cassOutput;
			}

			try
			{
				int cntr = 0;

				//option one- 
				Dictionary<string, object> dictionary = new JsonLDServices().JsonToDictionary( payload );
				//should exist, but need to validate
				object graph = dictionary[ "@graph" ];
				//serialize the graph object
				var glist = JsonConvert.SerializeObject( graph );
				//parse graph in to list of objects
				JArray graphList = JArray.Parse( glist );
				//each object should be a framework(only one, often first, but no guarantee) or competency
				
				foreach ( var item in graphList )
				{
					cntr++;
					comp = new Competency();
					//not sure will work
					//actually == {Name = "JObject" FullName = "Newtonsoft.Json.Linq.JObject"}
					if ( item.GetType() == typeof( Models.Common.CompetencyFramework ) )
					{

					}
					else if ( item.GetType() == typeof( Newtonsoft.Json.Linq.JObject ) )
					{
						if ( item.ToString().IndexOf( "ceasn:CompetencyFramework" ) > -1 )
						{
							output = ( ( Newtonsoft.Json.Linq.JObject )item ).ToObject<CompetencyFramework>();
							//check for previous one

							//map - although this could be done by caller!
							cassOutput.Framework.Name = output.name.ToString();
                            cassOutput.Framework._IdAndVersion = output.CtdlId;
							cassOutput.Framework.Description = output.description.ToString();
							cassOutput.Framework.Url = output.source == null || output.source.Count == 0? "" : output.source[0];
							cassOutput.Framework.CTID = output.CTID;
						}
						else if ( item.ToString().IndexOf( "ceasn:Competency" ) > -1 )
						{
							comp = ( ( Newtonsoft.Json.Linq.JObject )item ).ToObject<Competency>();
							output.competencies.Add( comp );

							//map
							cc = new MH.CassCompetency();
							cc.Name = comp.competencyText.ToString();
							cc.FrameworkUri = comp.CtdlId;
							cc.CTID = comp.Ctid;
                            cc.Uri = cc.FrameworkUri;
                            cc.Description = comp.competencyText.ToString();
							cassOutput.Competencies.Add( cc );

						}
						else
						{
							//error, or just ignore
						}
					}
					else if ( item.GetType() == typeof( Models.Common.Competency ) )
					{

					} else
					{
						//error
					}
				}

			} catch (Exception ex)
			{
                //TBD
                messages.Add( ex.Message );
			}
			return cassOutput;
		}
		//
		public static void UnPublishFramework( string frameworkCTID, AppUser user, ref List<string> messages )
        {
            //Validate
            var framework = Manager.GetByCtid( frameworkCTID );
            if (!ValidateFrameworkAction( framework, user, ref messages ))
            {
                return;
            }

            if (framework.CTID != frameworkCTID)
            {
                messages.Add( "The framework CTID doesn't match the version in the publisher." );
                return;
            }

            List<SiteActivity> list = new List<SiteActivity>();
            bool valid = new RegistryServices().UnPublishCompetencyFramework( framework, user, ref messages, ref list );

            //Update
            if (valid)
            {

            }
        }
        //
        public static bool ValidateFrameworkAction( ThisEntity framework, AppUser user, ref List<string> messages )
		{
			if( framework == null || framework.Id == 0 )
			{
				messages.Add( "Framework Not Found" );
				return false;
			}

			if( !CanUserUpdateFramework( user, framework.OrgId ) )
			{
				messages.Add( "You don't have access to manage that framework." );
				return false;
			}

			return true;
		}

        public static bool CanUserUpdateFramework( AppUser user, int orgId )
        {
            if (user == null || user.Id == 0)
                return false;

            if (OrganizationManager.IsOrganizationMember( user.Id, orgId ))
                return true;
            else if (AccountServices.IsUserSiteStaff(user))
                return true;

            return false;
        }
        //
    }
}
