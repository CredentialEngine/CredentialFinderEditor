using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Net.Http;
using Newtonsoft.Json;

using Utilities;
using CTIServices;
using CTIModels = Models;
using Models.Common;
using RAModels = RA.Models;
using RAServices = RA.Services;
using RATest = RA_UnitTestProject;

namespace CTI.Directory.Controllers
{
    public class DemoController : Controller
    {
		//Boilerplate
		public bool valid = true;
		public string status = "";
		public List<string> statusMessages = new List<string>();
		public string statusResult { get { return status + Environment.NewLine + string.Join( Environment.NewLine, statusMessages ); } }

		#region RegistryAssistant

		//Registry Assistant demo page
		public ActionResult RegistryAssistant()
		{
			return View( "~/Views/V2/Demo/RegistryAssistant.cshtml" );
		}
		//

		public JsonResult FetchData( string type, int id )
		{
			object result;
			try
			{
				switch ( type.ToLower() )
				{
					case "credential":
						result = RATest.DemoControllerHelpers.GetRaCredential( id, ref valid, ref status );
						break;
					case "organization":
						result = RATest.DemoControllerHelpers.GetRaOrganization( id, ref valid, ref status );
						break;
					case "assessment":
						result = RATest.DemoControllerHelpers.GetRaAssessment( id, ref valid, ref status );
						break;
					case "learningopportunity":
						result = RATest.DemoControllerHelpers.GetRaLearningOpportunity( id, ref valid, ref status );
						break;
					default:
						result = null;
						break;
				}
			}
			catch( Exception ex )
			{
				result = null;
				valid = false;
				status = ex.Message;
			}
			return JsonHelper.GetJsonWithWrapper( result, valid, statusResult, null );
		}
		//

		public JsonResult FormatData( string type, string data )
		{
			object result;
			try
			{
				switch ( type.ToLower() )
				{
					case "credential":
						{
							var deserialized = JsonConvert.DeserializeObject<RAModels.Input.Credential>( data );
							result = RAServices.CredentialServices.FormatAsJson( new RAModels.Input.CredentialRequest() { Credential = deserialized }, ref valid, ref statusMessages );
							
							break;
						}
					case "organization":
						{
							var deserialized = JsonConvert.DeserializeObject<RAModels.Input.Organization>( data );
							result = RAServices.AgentServices.FormatAsJson( new RAModels.Input.OrganizationRequest() { Organization = deserialized }, ref valid, ref statusMessages );
							break;
						}
					case "assessment":
						{
							var deserialized = JsonConvert.DeserializeObject<RAModels.Input.Assessment>( data );
							result = RAServices.AssessmentServices.FormatAsJson( deserialized, ref valid, ref statusMessages );
							break;
						}
					case "learningopportunity":
						{
							var deserialized = JsonConvert.DeserializeObject<RAModels.Input.LearningOpportunity>( data );
							result = RAServices.LearningOpportunityServices.FormatAsJson( new RAModels.Input.LearningOpportunityRequest() { LearningOpportunity = deserialized }, ref valid, ref statusMessages );
							break;
						}
					default:
						result = null;
						break;
				}
			}
			catch ( Exception ex )
			{
				result = null;
				valid = false;
				status = ex.Message;
			}
			return JsonHelper.GetJsonWithWrapper( result, valid, statusResult, null );
		}
		//

		public JsonResult DetailData( string type, int id )
		{
			object result;
			var user = AccountServices.GetCurrentUser( User.Identity.Name );
			try
			{
				switch ( type.ToLower() )
				{
					case "credential":
						{
							var skippingCache = FormHelper.GetRequestKeyValue( "skipCache", false );
							var credential = CredentialServices.GetCredentialDetail( id, user, skippingCache );
							var roleCodes = new EnumerationServices().GetCredentialAllAgentRoles( EnumerationType.CUSTOM );
							result = new JsonLDServices().GetProfileV2( credential, new CTIModels.JsonV2.Credential(), roleCodes.Items );
							break;
						}
					case "organization":
						{
							var organization = OrganizationServices.GetOrganizationDetail( id, user );
							var roleCodes = new EnumerationServices().GetAllAgentReverseRoles( EnumerationType.CUSTOM );
							if ( organization.OrganizationType.Items.Where( m => m.SchemaName == "orgType:QualityAssurance" ).Count() > 0 )
							{
								result = new JsonLDServices().GetProfileV2( organization, new CTIModels.JsonV2.QACredentialOrganization(), roleCodes.Items );
							}
							else
							{
								result = new JsonLDServices().GetProfileV2( organization, new CTIModels.JsonV2.CredentialOrganization(), roleCodes.Items );
							}
							break;
						}
					case "assessment":
						{
							var assessment = AssessmentServices.GetDetail( id );
							var roleCodes = new EnumerationServices().GetAssessmentAgentRoles( EnumerationType.CUSTOM );
							result = new JsonLDServices().GetProfileV2( assessment, new CTIModels.JsonV2.AssessmentProfile(), roleCodes.Items );
							break;
						}
					case "learningopportunity":
						{
							var learningOpportunity = LearningOpportunityServices.GetForDetail( id );
							var roleCodes = new EnumerationServices().GetLearningOppAgentRoles( EnumerationType.CUSTOM );
							result = new JsonLDServices().GetProfileV2( learningOpportunity, new CTIModels.JsonV2.LearningOpportunityProfile(), roleCodes.Items );
							break;
						}
					default:
						result = null;
						break;
				}
			}
			catch ( Exception ex )
			{
				result = null;
				valid = false;
				status = ex.Message;
			}
			return JsonHelper.GetJsonWithWrapper( result, valid, statusResult, null );
		}
		//

		public JsonResult PublishData( string type, string data, bool forceSkipValidation = false )
		{
			object result;
			var rawResponse = "";
			var envelopeID = "";
			try
			{
				switch ( type.ToLower() )
				{
					case "credential":
						{
							var deserialized = JsonConvert.DeserializeObject<RAModels.Json.Credential>( data );
							envelopeID = RAServices.CredentialServices.DemoPublish( deserialized, ref valid, ref statusMessages, ref rawResponse, forceSkipValidation );
							break;
						}
					case "organization":
						{
							var deserialized = JsonConvert.DeserializeObject<RAModels.Json.Agent>( data );
							envelopeID = RAServices.AgentServices.DemoPublish( deserialized, ref valid, ref statusMessages, ref rawResponse, forceSkipValidation );
							break;
						}
					case "assessment":
						{
							var deserialized = JsonConvert.DeserializeObject<RAModels.Json.AssessmentProfile>( data );
							envelopeID = RAServices.AssessmentServices.DemoPublish( deserialized, ref valid, ref statusMessages, ref rawResponse, forceSkipValidation );
							break;
						}
					case "learningopportunity":
						{
							var deserialized = JsonConvert.DeserializeObject<RAModels.Json.LearningOpportunityProfile>( data );
							envelopeID = RAServices.LearningOpportunityServices.DemoPublish( deserialized, ref valid, ref statusMessages, ref rawResponse, forceSkipValidation );
							break;
						}
					default:
						result = null;
						break;
				}

				if ( valid )
				{
					result = GetPublishedEnvelope( envelopeID );
				}
				else
				{
					result = rawResponse;
					valid = true;
				}
			}
			catch ( Exception ex )
			{
				result = null;
				valid = false;
				status = ex.Message;
			}
			return JsonHelper.GetJsonWithWrapper( result, valid, statusResult, envelopeID );
		}
		//

		public string GetPublishedEnvelope( string envelopeID )
		{
			var getURL = ServiceHelper.GetAppKeyValue( "credentialRegistryGet" );
			var resourceURI = string.Format( getURL, envelopeID );
			return new HttpClient().GetAsync( resourceURI ).Result.Content.ReadAsStringAsync().Result;
		}
		//

		#endregion

		#region CASS

		//CASS Demo page
		public ActionResult Cass()
		{
			return View( "~/Views/V2/CASS/Search.cshtml" );
		}
		//

		//Experimental
		public ActionResult FrameworkEditor()
		{
			return View( "~/Views/V2/Demo/FrameworkEditor.cshtml" );
		}
		public ActionResult FrameworkEditorV4()
		{
			return View( "~/Views/V2/Demo/FrameworkEditorV4.cshtml" );
		}
		//

		#endregion

		#region CER Search

		//CER Search Demo page
		public ActionResult CERSearch()
		{
			return View( "~/Views/V2/Demo/CERSearch.cshtml" );
		}
		//

		public JsonResult ProxyQuery( string query )
		{
			var queryBasis = UtilityManager.GetAppKeyValue( "credentialRegistrySearch", "http://lr-staging.learningtapestry.com/ce-registry/search?" ); //Should get this from web.config

			var data = new HttpClient().GetAsync( queryBasis + query ).Result.Content.ReadAsStringAsync().Result;
			return JsonHelper.GetJsonWithWrapper( data, true, "", queryBasis + query );
		}
		//

		#endregion
	}
}